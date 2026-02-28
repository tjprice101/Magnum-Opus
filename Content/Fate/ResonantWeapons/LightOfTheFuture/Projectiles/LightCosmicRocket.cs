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
    /// LightCosmicRocket — Homing rocket spawned every 3rd shot (3 in a spread).
    /// Spirals toward nearest enemy, explodes on contact with AoE damage.
    /// Trail: crimson-gold fire with violet smoke edges.
    /// </summary>
    public class LightCosmicRocket : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles Asset Library/WholeNote";

        private float _spiralPhase;
        private NPC _target;
        private int _homingDelay = 8; // Frames before homing kicks in

        private static Asset<Texture2D> _glowTex;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 18;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 180;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
            _spiralPhase += 0.15f;

            // Spiral motion offset
            float spiralOffset = MathF.Sin(_spiralPhase) * 2.5f;
            Vector2 perpendicular = Projectile.velocity.SafeNormalize(Vector2.UnitX).RotatedBy(MathHelper.PiOver2);

            if (_homingDelay > 0)
            {
                _homingDelay--;
            }
            else
            {
                // Find or update target
                if (_target == null || !_target.active || _target.dontTakeDamage)
                    _target = LightUtils.ClosestNPCAt(Projectile.Center, 600f);

                if (_target != null)
                {
                    // Homing toward target with spiral
                    Vector2 targetDir = LightUtils.SafeDirectionTo(Projectile.Center, _target.Center);
                    float turnSpeed = 0.08f;
                    Vector2 desiredVel = targetDir * 14f;
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, desiredVel, turnSpeed);

                    // Keep speed consistent
                    float speed = Projectile.velocity.Length();
                    if (speed < 10f) speed = 10f;
                    if (speed > 18f) speed = 18f;
                    Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.UnitX) * speed;
                }
                else
                {
                    // No target: fly in spiral pattern
                    float speed = Projectile.velocity.Length();
                    if (speed < 8f)
                        Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.UnitX) * 8f;
                }
            }

            // Apply spiral offset to position (visual only via old positions)
            Projectile.Center += perpendicular * spiralOffset * 0.3f;

            // === TRAIL VFX ===
            if (!Main.dedServ)
            {
                SpawnRocketTrailVFX();
            }

            // Dynamic light
            Lighting.AddLight(Projectile.Center, LightUtils.ImpactCrimson.ToVector3() * 0.35f);
        }

        private void SpawnRocketTrailVFX()
        {
            Vector2 awayDir = -Projectile.velocity.SafeNormalize(Vector2.Zero);

            // Fire glow motes
            if (Main.rand.NextBool(2))
            {
                float gradientT = (Main.GameUpdateCount * 0.03f) % 1f;
                Color trailCol = LightUtils.RocketGradient(gradientT);
                var mote = new LightMote(
                    Projectile.Center + Main.rand.NextVector2Circular(3f, 3f),
                    awayDir * Main.rand.NextFloat(0.5f, 1.2f) + Main.rand.NextVector2Circular(0.4f, 0.4f),
                    trailCol * 0.55f, 0.14f, 14);
                LightParticleHandler.SpawnParticle(mote);
            }

            // Fire sparks — rocket exhaust
            if (Main.rand.NextBool(3))
            {
                Vector2 sparkVel = awayDir * Main.rand.NextFloat(1f, 2.5f) + Main.rand.NextVector2Circular(0.8f, 0.8f);
                var spark = new LightSpark(
                    Projectile.Center, sparkVel,
                    LightUtils.ImpactCrimson * 0.6f, 0.12f, 10);
                LightParticleHandler.SpawnParticle(spark);
            }

            // Smoke wisps
            if (Main.rand.NextBool(3))
            {
                var smoke = new LightSmoke(
                    Projectile.Center, awayDir * 0.4f,
                    LightUtils.DeepViolet * 0.3f, 0.16f, 20);
                LightParticleHandler.SpawnParticle(smoke);
            }

            // Fire dust
            if (Main.rand.NextBool(3))
            {
                Color dustCol = Color.Lerp(LightUtils.ImpactCrimson, LightUtils.MuzzleGold, Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.RedTorch,
                    awayDir * Main.rand.NextFloat(0.5f, 1.5f), 0, dustCol, 0.8f);
                d.noGravity = true;
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<DestinyCollapse>(), 240);

            if (!Main.dedServ)
                SpawnExplosionVFX(target.Center);

            SoundEngine.PlaySound(SoundID.Item14 with { Pitch = 0.2f, Volume = 0.7f }, target.Center);
        }

        public override void OnKill(int timeLeft)
        {
            if (!Main.dedServ)
                SpawnExplosionVFX(Projectile.Center);

            SoundEngine.PlaySound(SoundID.Item14 with { Pitch = 0.1f, Volume = 0.6f }, Projectile.Center);

            // AoE damage on explosion
            float aoeRadius = 80f;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly || npc.dontTakeDamage) continue;
                if (Vector2.Distance(Projectile.Center, npc.Center) <= aoeRadius)
                {
                    npc.AddBuff(ModContent.BuffType<DestinyCollapse>(), 180);
                }
            }
        }

        private void SpawnExplosionVFX(Vector2 pos)
        {
            // Central bloom flares
            LightParticleHandler.SpawnParticle(new LightBloomFlare(pos, LightUtils.ImpactCrimson, 0.7f, 18));
            LightParticleHandler.SpawnParticle(new LightBloomFlare(pos, LightUtils.MuzzleGold, 0.5f, 14));
            LightParticleHandler.SpawnParticle(new LightBloomFlare(pos, LightUtils.PlasmaWhite, 0.3f, 10));

            // Spark ring explosion
            for (int i = 0; i < 12; i++)
            {
                float angle = MathHelper.TwoPi * i / 12f;
                Vector2 sparkVel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 8f);
                Color sparkCol = Color.Lerp(LightUtils.ImpactCrimson, LightUtils.MuzzleGold, (float)i / 12f);
                LightParticleHandler.SpawnParticle(new LightSpark(pos, sparkVel, sparkCol * 0.8f, 0.2f, 14));
            }

            // Glyph burst
            for (int i = 0; i < 3; i++)
            {
                float angle = MathHelper.TwoPi * i / 3f;
                Vector2 glyphPos = pos + angle.ToRotationVector2() * 15f;
                LightParticleHandler.SpawnParticle(new LightGlyph(glyphPos, LightUtils.TrailViolet * 0.5f, 0.22f, 22));
            }

            // Smoke burst
            for (int i = 0; i < 6; i++)
            {
                Vector2 smokeVel = Main.rand.NextVector2Circular(2f, 2f);
                LightParticleHandler.SpawnParticle(new LightSmoke(pos, smokeVel, LightUtils.DeepViolet * 0.4f, 0.25f, 30));
            }

            // Dust burst
            for (int i = 0; i < 10; i++)
            {
                Vector2 dustVel = Main.rand.NextVector2Circular(5f, 5f);
                Color dustCol = Color.Lerp(LightUtils.ImpactCrimson, LightUtils.MuzzleGold, Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(pos, DustID.RedTorch, dustVel, 0, dustCol, 1.1f);
                d.noGravity = true;
            }

            Lighting.AddLight(pos, LightUtils.ImpactCrimson.ToVector3() * 1.0f);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            // Draw GPU trail
            if (Projectile.oldPos.Length >= 2)
            {
                var settings = new LightTrailSettings(
                    width: (progress, idx) => 8f * (1f - progress * 0.7f),
                    color: (progress) =>
                    {
                        Color c = LightUtils.RocketGradient(progress);
                        float fade = 1f - MathF.Pow(progress, 1.5f);
                        return c * fade * 0.7f;
                    }
                );

                Vector2[] points = new Vector2[18];
                int count = 0;
                for (int i = 0; i < Projectile.oldPos.Length && i < 18; i++)
                {
                    if (Projectile.oldPos[i] == Vector2.Zero) break;
                    points[i] = Projectile.oldPos[i] + Projectile.Size / 2f;
                    count++;
                }

                if (count >= 2)
                    LightTrailRenderer.RenderTrail(points, settings, count);
            }

            // Draw rocket core
            _glowTex ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/SandboxLastPrism/Orbs/SoftGlow");
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float pulse = 1f + MathF.Sin(_spiralPhase * 2f) * 0.15f;

            // Crimson core
            Main.spriteBatch.Draw(_glowTex.Value, drawPos, null,
                LightUtils.Additive(LightUtils.ImpactCrimson, 0.7f),
                Projectile.rotation, _glowTex.Value.Size() / 2f, 0.12f * pulse, SpriteEffects.None, 0f);

            // Gold outer glow
            Main.spriteBatch.Draw(_glowTex.Value, drawPos, null,
                LightUtils.Additive(LightUtils.MuzzleGold, 0.35f),
                0f, _glowTex.Value.Size() / 2f, 0.25f * pulse, SpriteEffects.None, 0f);

            return false;
        }
    }
}
