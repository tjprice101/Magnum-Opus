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
    /// Eclipse Wrath Shard — small tracking shard spawned by EclipseOrb.
    /// Seeks nearest enemy with aggressive homing. Solar corona trail and ember sparks.
    /// </summary>
    public class EclipseWrathShard : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/PointBloom";

        private int aiTimer;
        private readonly List<Vector2> trailPoints = new List<Vector2>(10);
        private static Asset<Texture2D> glowTex;

        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 180;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.extraUpdates = 1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }

        public override void AI()
        {
            aiTimer++;

            trailPoints.Add(Projectile.Center);
            if (trailPoints.Count > 8)
                trailPoints.RemoveAt(0);

            // Brief spread phase (10 ticks), then aggressive homing
            if (aiTimer < 10)
            {
                // Random drift outward from orb
                if (Projectile.velocity.Length() < 6f)
                    Projectile.velocity += Main.rand.NextVector2Circular(0.8f, 0.8f);
            }
            else
            {
                NPC target = EclipseUtils.ClosestNPCAt(Projectile.Center, 700f);
                if (target != null)
                {
                    Vector2 desired = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX) * 12f;
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, desired, 0.14f);
                }
                else
                {
                    Projectile.velocity *= 0.98f;
                }
            }

            Projectile.rotation = Projectile.velocity.ToRotation();

            // Trailing embers
            if (Main.rand.NextBool(3))
            {
                EclipseParticleHandler.SpawnParticle(new WrathEmberParticle(
                    Projectile.Center, -Projectile.velocity * 0.2f + Main.rand.NextVector2Circular(1f, 1f),
                    Main.rand.NextFloat(0.15f, 0.3f), Main.rand.Next(10, 18)));
            }

            Lighting.AddLight(Projectile.Center, EclipseUtils.MidCorona.ToVector3() * 0.4f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.OnFire3, 180);

            // Impact corona burst
            EclipseParticleHandler.SpawnParticle(new SolarBloomParticle(target.Center, EclipseUtils.OuterCorona, 1f, 15));

            for (int i = 0; i < 5; i++)
            {
                EclipseParticleHandler.SpawnParticle(new WrathEmberParticle(
                    target.Center, Main.rand.NextVector2Circular(4f, 4f),
                    Main.rand.NextFloat(0.2f, 0.5f), Main.rand.Next(12, 22)));
            }

            SoundEngine.PlaySound(SoundID.DD2_BetsyFireballImpact with { Pitch = 0.3f, Volume = 0.5f }, target.Center);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            glowTex ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/PointBloom");
            if (!glowTex.IsLoaded) return false;

            SpriteBatch sb = Main.spriteBatch;

            // GPU trail
            if (trailPoints.Count >= 3)
            {
                try
                {
                    sb.End();
                    var settings = new EclipseTrailSettings(
                        p => 10f * (float)Math.Sin(p * MathHelper.Pi) * (1f - p * 0.5f),
                        p =>
                        {
                            Color c = EclipseUtils.MulticolorLerp(p,
                                EclipseUtils.SolarWhite, EclipseUtils.OuterCorona, EclipseUtils.MidCorona, EclipseUtils.InnerCorona);
                            return c * (1f - p * 0.6f);
                        },
                        smoothing: 2,
                        shaderSetup: () =>
                        {
                            var device = Main.graphics.GraphicsDevice;
                            device.BlendState = BlendState.Additive;
                            device.RasterizerState = RasterizerState.CullNone;
                        });
                    EclipseTrailRenderer.RenderTrail(trailPoints, settings);
                    Main.graphics.GraphicsDevice.BlendState = BlendState.AlphaBlend;
                }
                catch { }
                finally
                {
                    EclipseUtils.ResetSpriteBatch(sb);
                }
            }

            // Core glow
            sb.End();
            EclipseUtils.BeginAdditive(sb);

            var glow = glowTex.Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            sb.Draw(glow, drawPos, null, EclipseUtils.Additive(EclipseUtils.OuterCorona, 0.5f),
                Projectile.rotation, glow.Size() / 2f, new Vector2(0.35f, 0.15f), SpriteEffects.None, 0f);
            sb.Draw(glow, drawPos, null, EclipseUtils.Additive(EclipseUtils.SolarWhite, 0.4f),
                0f, glow.Size() / 2f, 0.1f, SpriteEffects.None, 0f);

            sb.End();
            EclipseUtils.ResetSpriteBatch(sb);

            return false;
        }
    }
}
