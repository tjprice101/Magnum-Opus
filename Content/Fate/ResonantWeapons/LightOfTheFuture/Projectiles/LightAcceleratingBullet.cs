using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.Fate.Debuffs;
using MagnumOpus.Content.Fate.ResonantWeapons.LightOfTheFuture.Utilities;
using MagnumOpus.Content.Fate.ResonantWeapons.LightOfTheFuture.Particles;
using MagnumOpus.Content.Fate.ResonantWeapons.LightOfTheFuture.Primitives;

namespace MagnumOpus.Content.Fate.ResonantWeapons.LightOfTheFuture.Projectiles
{
    /// <summary>
    /// LightAcceleratingBullet — The primary projectile of Light of the Future.
    /// Starts at shootSpeed 6, accelerates to 40+ over ~0.5s.
    /// Pierces enemies, applies DestinyCollapse debuff.
    /// Trail intensifies with speed: void → violet → cyan → plasma white.
    /// Constellation-line trails connect impact points.
    /// </summary>
    public class LightAcceleratingBullet : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/SandboxLastPrism/Flare/flare_16";

        private float _currentSpeed = 6f;
        private const float MaxSpeed = 42f;
        private const float Acceleration = 1.2f;

        // Trail position cache
        private Vector2[] _trailPositions;
        private const int TrailLength = 20;

        private static Asset<Texture2D> _glowTex;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = TrailLength;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 12;
            Projectile.height = 12;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 4;
            Projectile.timeLeft = 180;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 8;
            Projectile.extraUpdates = 1;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();

            // Initialize trail array
            _trailPositions ??= new Vector2[TrailLength];

            // Accelerate — starts slow, ramps up fast
            _currentSpeed = Math.Min(_currentSpeed + Acceleration, MaxSpeed);
            Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.UnitX) * _currentSpeed;

            float speedRatio = MathHelper.Clamp((_currentSpeed - 6f) / (MaxSpeed - 6f), 0f, 1f);

            // === TRAIL VFX — scales with speed ===
            if (!Main.dedServ)
            {
                SpawnTrailParticles(speedRatio);
            }

            // Dynamic light
            Vector3 lightCol = Vector3.Lerp(
                LightUtils.TrailViolet.ToVector3(),
                LightUtils.LaserCyan.ToVector3(),
                speedRatio);
            Lighting.AddLight(Projectile.Center, lightCol * (0.3f + speedRatio * 0.5f));
        }

        private void SpawnTrailParticles(float speedRatio)
        {
            Vector2 awayDir = -Projectile.velocity.SafeNormalize(Vector2.Zero);

            // Primary glow motes — intensity scales with speed
            int moteCount = 1 + (int)(speedRatio * 2f);
            for (int i = 0; i < moteCount; i++)
            {
                Color trailCol = LightUtils.BulletGradient(speedRatio);
                float scale = 0.12f + speedRatio * 0.1f;
                var mote = new LightMote(
                    Projectile.Center + Main.rand.NextVector2Circular(4f, 4f),
                    awayDir * Main.rand.NextFloat(0.5f, 1.5f) + Main.rand.NextVector2Circular(0.5f, 0.5f),
                    trailCol * 0.6f, scale, 14);
                LightParticleHandler.SpawnParticle(mote);
            }

            // Speed line tracers at high speed
            if (speedRatio > 0.3f && Main.rand.NextBool(3))
            {
                Color tracerCol = Color.Lerp(LightUtils.LaserCyan, LightUtils.PlasmaWhite, speedRatio);
                var tracer = new LightTracer(
                    Projectile.Center + Main.rand.NextVector2Circular(3f, 3f),
                    awayDir * (3f + speedRatio * 6f),
                    tracerCol * 0.7f, 0.15f, 8);
                LightParticleHandler.SpawnParticle(tracer);
            }

            // Star sparks at high velocity
            if (speedRatio > 0.5f && Main.rand.NextBool(4))
            {
                Color sparkCol = Main.rand.NextBool(3) ? LightUtils.MuzzleGold : LightUtils.PlasmaWhite;
                var spark = new LightSpark(
                    Projectile.Center + Main.rand.NextVector2Circular(5f, 5f),
                    Main.rand.NextVector2Circular(1f, 1f),
                    sparkCol * 0.5f, 0.12f, 10);
                LightParticleHandler.SpawnParticle(spark);
            }

            // Dust trail
            if (Main.rand.NextBool(2))
            {
                Color dustCol = LightUtils.BulletGradient(speedRatio);
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.BlueTorch,
                    awayDir * Main.rand.NextFloat(1f, 2f), 0, dustCol, 0.8f + speedRatio * 0.4f);
                d.noGravity = true;
            }

            // Smoke wisps at high speed
            if (speedRatio > 0.6f && Main.rand.NextBool(5))
            {
                var smoke = new LightSmoke(
                    Projectile.Center,
                    awayDir * 0.5f + Main.rand.NextVector2Circular(0.3f, 0.3f),
                    LightUtils.DeepViolet * 0.4f, 0.2f, 25);
                LightParticleHandler.SpawnParticle(smoke);
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Apply DestinyCollapse debuff
            target.AddBuff(ModContent.BuffType<DestinyCollapse>(), 300);

            // Impact VFX
            if (!Main.dedServ)
            {
                SpawnImpactVFX(target.Center);
            }

            // Sound
            SoundEngine.PlaySound(SoundID.Item62 with { Pitch = 0.3f, Volume = 0.6f }, target.Center);
        }

        public override void OnKill(int timeLeft)
        {
            if (!Main.dedServ)
            {
                SpawnImpactVFX(Projectile.Center);
            }
            SoundEngine.PlaySound(SoundID.Item14 with { Pitch = 0.5f, Volume = 0.5f }, Projectile.Center);
        }

        private void SpawnImpactVFX(Vector2 pos)
        {
            // Central bloom flare
            LightParticleHandler.SpawnParticle(new LightBloomFlare(pos, LightUtils.LaserCyan, 0.6f, 16));
            LightParticleHandler.SpawnParticle(new LightBloomFlare(pos, LightUtils.PlasmaWhite, 0.35f, 12));

            // Spark burst
            for (int i = 0; i < 8; i++)
            {
                Vector2 sparkVel = Main.rand.NextVector2Circular(5f, 5f);
                Color sparkCol = Color.Lerp(LightUtils.LaserCyan, LightUtils.ImpactCrimson, Main.rand.NextFloat());
                LightParticleHandler.SpawnParticle(new LightSpark(pos, sparkVel, sparkCol * 0.7f, 0.18f, 12));
            }

            // Dust ring
            for (int i = 0; i < 10; i++)
            {
                float angle = MathHelper.TwoPi * i / 10f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 6f);
                Dust d = Dust.NewDustPerfect(pos, DustID.BlueTorch, vel, 0, LightUtils.LaserCyan, 1.1f);
                d.noGravity = true;
            }

            // Glyph accent
            LightParticleHandler.SpawnParticle(new LightGlyph(pos, LightUtils.TrailViolet * 0.6f, 0.2f, 20));

            Lighting.AddLight(pos, LightUtils.LaserCyan.ToVector3() * 0.8f);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            // Draw GPU trail
            if (Projectile.oldPos.Length >= 2)
            {
                float speedRatio = MathHelper.Clamp((_currentSpeed - 6f) / (MaxSpeed - 6f), 0f, 1f);

                var settings = new LightTrailSettings(
                    width: (progress, idx) =>
                    {
                        float baseWidth = 6f + speedRatio * 10f;
                        return baseWidth * (1f - progress * 0.8f);
                    },
                    color: (progress) =>
                    {
                        Color c = LightUtils.BulletGradient(speedRatio * (1f - progress * 0.5f));
                        float fade = 1f - MathF.Pow(progress, 1.3f);
                        return c * fade * 0.8f;
                    }
                );

                // Use oldPos for trail points
                Vector2[] points = new Vector2[TrailLength];
                int count = 0;
                for (int i = 0; i < Projectile.oldPos.Length && i < TrailLength; i++)
                {
                    if (Projectile.oldPos[i] == Vector2.Zero) break;
                    points[i] = Projectile.oldPos[i] + Projectile.Size / 2f;
                    count++;
                }

                if (count >= 2)
                {
                    LightTrailRenderer.RenderTrail(points, settings, count);
                }
            }

            // Draw bullet core sprite
            _glowTex ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/SandboxLastPrism/Orbs/SoftGlow");
            float sr = MathHelper.Clamp((_currentSpeed - 6f) / (MaxSpeed - 6f), 0f, 1f);
            Color coreCol = Color.Lerp(LightUtils.LaserCyan, LightUtils.PlasmaWhite, sr) * 0.9f;
            float coreScale = 0.15f + sr * 0.12f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            Main.spriteBatch.Draw(_glowTex.Value, drawPos, null, coreCol with { A = 0 },
                Projectile.rotation, _glowTex.Value.Size() / 2f, coreScale, SpriteEffects.None, 0f);

            // Outer bloom layer
            Color bloomCol = LightUtils.TrailViolet * 0.4f;
            Main.spriteBatch.Draw(_glowTex.Value, drawPos, null, bloomCol with { A = 0 },
                0f, _glowTex.Value.Size() / 2f, coreScale * 2.5f, SpriteEffects.None, 0f);

            return false;
        }
    }
}
