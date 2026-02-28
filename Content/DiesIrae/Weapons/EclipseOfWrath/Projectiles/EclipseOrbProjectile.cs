using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.DiesIrae.Weapons.EclipseOfWrath.Utilities;
using MagnumOpus.Content.DiesIrae.Weapons.EclipseOfWrath.Particles;
using MagnumOpus.Content.DiesIrae.Weapons.EclipseOfWrath.Primitives;

namespace MagnumOpus.Content.DiesIrae.Weapons.EclipseOfWrath.Projectiles
{
    /// <summary>
    /// Eclipse Orb — a dark sun projectile that tracks the cursor.
    /// While airborne, spawns blazing wrath shards that seek enemies.
    /// Explodes on impact or when player clicks again.
    /// Multi-layered corona glow rendering with orbiting flare points.
    /// </summary>
    public class EclipseOrbProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/PointBloom";

        private int shardTimer;
        private readonly List<Vector2> trailPoints = new List<Vector2>(14);

        private static Asset<Texture2D> bloomTex;
        private static Asset<Texture2D> glowTex;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 12;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 30;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 300;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
        }

        public override void AI()
        {
            // Track cursor
            if (Main.myPlayer == Projectile.owner)
            {
                Vector2 cursor = Main.MouseWorld;
                Vector2 dir = cursor - Projectile.Center;
                if (dir.Length() > 20f)
                {
                    dir.Normalize();
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, dir * 14f, 0.08f);
                }
            }

            Projectile.rotation += 0.1f;
            shardTimer++;

            // Trail cache
            trailPoints.Add(Projectile.Center);
            if (trailPoints.Count > 12)
                trailPoints.RemoveAt(0);

            // Spawn wrath shards every 20 ticks
            if (shardTimer % 20 == 0 && Main.myPlayer == Projectile.owner)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 spawnPos = Projectile.Center + angle.ToRotationVector2() * 20f;

                Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(), spawnPos, Vector2.Zero,
                    ModContent.ProjectileType<EclipseWrathShard>(),
                    Projectile.damage / 2, 0f, Projectile.owner);
            }

            // ── SELF-CONTAINED PARTICLE VFX ──

            // Corona flares orbiting
            float orbitAngle = Main.GlobalTimeWrappedHourly * 3f;
            if (Main.rand.NextBool(3))
            {
                for (int i = 0; i < 4; i++)
                {
                    float a = orbitAngle + MathHelper.TwoPi * i / 4f;
                    Vector2 offset = a.ToRotationVector2() * (20f + (float)Math.Sin(shardTimer * 0.08f) * 5f);
                    Color c = EclipseUtils.CoronaLerp((float)i / 4f);
                    EclipseParticleHandler.SpawnParticle(new CoronaFlareParticle(
                        Projectile.Center + offset, Projectile.velocity * 0.5f, c, 0.35f, 18));
                }
            }

            // Eclipse smoke
            if (Main.rand.NextBool(2))
            {
                EclipseParticleHandler.SpawnParticle(new EclipseSmokeParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(12f, 12f),
                    -Projectile.velocity * 0.15f + Main.rand.NextVector2Circular(1.5f, 1.5f),
                    Main.rand.NextFloat(0.5f, 0.9f), Main.rand.Next(25, 40)));
            }

            // Wrath embers trailing
            if (Main.rand.NextBool(2))
            {
                EclipseParticleHandler.SpawnParticle(new WrathEmberParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(10f, 10f),
                    -Projectile.velocity * 0.2f + Main.rand.NextVector2Circular(2f, 2f),
                    Main.rand.NextFloat(0.2f, 0.4f), Main.rand.Next(15, 25)));
            }

            // Eclipse notes (less frequent — more impactful)
            if (Main.rand.NextBool(8))
            {
                EclipseParticleHandler.SpawnParticle(new EclipseNoteParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(15f, 15f),
                    Main.rand.NextVector2Circular(2f, 2f) + Projectile.velocity * 0.5f,
                    EclipseUtils.SolarGold, 0.5f, Main.rand.Next(35, 50)));
            }

            // Dynamic lighting with eclipse pulse
            float pulse = EclipseUtils.SolarPulse(shardTimer * 0.1f);
            Lighting.AddLight(Projectile.Center, EclipseUtils.OuterCorona.ToVector3() * 0.8f * pulse);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.OnFire3, 300);
            Explode();
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Explode();
            return true;
        }

        private void Explode()
        {
            SoundEngine.PlaySound(SoundID.DD2_BetsyFireballImpact with { Pitch = -0.1f, Volume = 1.1f }, Projectile.Center);

            // Massive solar bloom
            EclipseParticleHandler.SpawnParticle(new SolarBloomParticle(Projectile.Center, EclipseUtils.OuterCorona, 3f, 28));
            EclipseParticleHandler.SpawnParticle(new SolarBloomParticle(Projectile.Center, EclipseUtils.SolarWhite, 1.5f, 20));

            // Corona flare burst (12-point radial)
            for (int i = 0; i < 12; i++)
            {
                float angle = MathHelper.TwoPi * i / 12f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(5f, 9f);
                Color c = EclipseUtils.CoronaLerp((float)i / 12f);
                EclipseParticleHandler.SpawnParticle(new CoronaFlareParticle(
                    Projectile.Center, vel, c, Main.rand.NextFloat(0.4f, 0.8f), Main.rand.Next(20, 35)));
            }

            // Ember spray
            for (int i = 0; i < 15; i++)
            {
                Vector2 vel = Main.rand.NextVector2CircularEdge(6f, 6f) + Main.rand.NextVector2Circular(2f, 2f);
                EclipseParticleHandler.SpawnParticle(new WrathEmberParticle(
                    Projectile.Center, vel, Main.rand.NextFloat(0.3f, 0.7f), Main.rand.Next(18, 32)));
            }

            // Smoke eruption
            for (int i = 0; i < 8; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(4f, 4f);
                EclipseParticleHandler.SpawnParticle(new EclipseSmokeParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(10f, 10f),
                    vel, Main.rand.NextFloat(0.8f, 1.5f), Main.rand.Next(30, 50)));
            }

            // Music notes finale
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 6f);
                Color c = EclipseUtils.MulticolorLerp((float)i / 8f,
                    EclipseUtils.SolarGold, EclipseUtils.MidCorona, EclipseUtils.InnerCorona);
                EclipseParticleHandler.SpawnParticle(new EclipseNoteParticle(
                    Projectile.Center, vel, c, Main.rand.NextFloat(0.6f, 1f), Main.rand.Next(40, 60)));
            }

            Lighting.AddLight(Projectile.Center, EclipseUtils.OuterCorona.ToVector3() * 1.8f);
            Projectile.Kill();
        }

        public override bool PreDraw(ref Color lightColor)
        {
            bloomTex ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftRadialBloom");
            glowTex ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/PointBloom");
            if (!bloomTex.IsLoaded || !glowTex.IsLoaded) return false;

            SpriteBatch sb = Main.spriteBatch;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float time = Main.GlobalTimeWrappedHourly;
            float pulse = EclipseUtils.SolarPulse(time);

            // Draw GPU primitive trail
            if (trailPoints.Count >= 3)
            {
                try
                {
                    sb.End();
                    var trailSettings = new EclipseTrailSettings(
                        p => 22f * (float)Math.Sin(p * MathHelper.Pi) * (1f - p * 0.3f),
                        p =>
                        {
                            Color c = EclipseUtils.CoronaLerp(p * 0.8f);
                            return c * (1f - p * 0.7f);
                        },
                        smoothing: 3,
                        shaderSetup: () =>
                        {
                            var device = Main.graphics.GraphicsDevice;
                            device.BlendState = BlendState.Additive;
                            device.RasterizerState = RasterizerState.CullNone;
                        });
                    EclipseTrailRenderer.RenderTrail(trailPoints, trailSettings);
                    Main.graphics.GraphicsDevice.BlendState = BlendState.AlphaBlend;
                }
                catch { }
                finally
                {
                    EclipseUtils.ResetSpriteBatch(sb);
                }
            }

            // Switch to additive for glow layers
            sb.End();
            EclipseUtils.BeginAdditive(sb);

            var bloom = bloomTex.Value;
            var glow = glowTex.Value;

            // Layer 1: Umbra disk (dark core)
            sb.Draw(bloom, drawPos, null, EclipseUtils.Additive(EclipseUtils.Umbra, 0.3f),
                0f, bloom.Size() / 2f, 0.6f * pulse, SpriteEffects.None, 0f);

            // Layer 2: Blood-red inner corona rotating
            sb.Draw(glow, drawPos, null, EclipseUtils.Additive(EclipseUtils.InnerCorona, 0.5f),
                time * 0.8f, glow.Size() / 2f, 0.5f * pulse, SpriteEffects.None, 0f);

            // Layer 3: Orange mid corona counter-rotating
            sb.Draw(glow, drawPos, null, EclipseUtils.Additive(EclipseUtils.OuterCorona, 0.6f),
                -time * 0.6f, glow.Size() / 2f, 0.45f * pulse, SpriteEffects.None, 0f);

            // Layer 4: Solar gold chromosphere
            sb.Draw(bloom, drawPos, null, EclipseUtils.Additive(EclipseUtils.SolarGold, 0.5f),
                time * 1.1f, bloom.Size() / 2f, 0.35f * pulse, SpriteEffects.None, 0f);

            // Layer 5: Hot white photosphere core
            sb.Draw(glow, drawPos, null, EclipseUtils.Additive(EclipseUtils.SolarWhite, 0.7f),
                0f, glow.Size() / 2f, 0.2f * pulse, SpriteEffects.None, 0f);

            // Orbiting spark points (5-point corona ring)
            float sparkOrbit = time * 4f;
            for (int i = 0; i < 5; i++)
            {
                float a = sparkOrbit + MathHelper.TwoPi * i / 5f;
                Vector2 sparkOff = a.ToRotationVector2() * 16f * pulse;
                Color sparkColor = EclipseUtils.CoronaLerp((float)i / 5f);
                sb.Draw(glow, drawPos + sparkOff, null, EclipseUtils.Additive(sparkColor, 0.5f),
                    -sparkOrbit, glow.Size() / 2f, 0.12f * pulse, SpriteEffects.None, 0f);
            }

            sb.End();
            EclipseUtils.ResetSpriteBatch(sb);

            return false;
        }
    }
}
