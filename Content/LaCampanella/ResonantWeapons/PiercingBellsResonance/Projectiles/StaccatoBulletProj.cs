using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Graphics.Shaders;
using MagnumOpus.Content.LaCampanella.ResonantWeapons.PiercingBellsResonance.Utilities;
using MagnumOpus.Content.LaCampanella.ResonantWeapons.PiercingBellsResonance.Particles;
using MagnumOpus.Content.LaCampanella.ResonantWeapons.PiercingBellsResonance.Primitives;
using MagnumOpus.Content.LaCampanella.ResonantWeapons.PiercingBellsResonance.Shaders;
using MagnumOpus.Content.LaCampanella.Debuffs;
using MagnumOpus.Content.FoundationWeapons.ImpactFoundation;
using MagnumOpus.Common.Systems.VFX;
using ReLogic.Content;

namespace MagnumOpus.Content.LaCampanella.ResonantWeapons.PiercingBellsResonance.Projectiles
{
    /// <summary>
    /// Staccato bullet wrapper 遯ｶ繝ｻreplaces vanilla bullets with fire-trail-enhanced versions.
    /// ai[0] stores original bullet type for visual reference.
    /// </summary>
    public class StaccatoBulletProj : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.Bullet;

        private List<Vector2> trailPositions = new List<Vector2>();
        private const int MaxTrailPoints = 10;
        private PiercingBellsPrimitiveRenderer trailRenderer;

        public override void SetDefaults()
        {
            Projectile.width = 8;
            Projectile.height = 8;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 120;
            Projectile.extraUpdates = 2;
            Projectile.aiStyle = 1;
            AIType = ProjectileID.Bullet;
        }

        public override void AI()
        {
            trailPositions.Insert(0, Projectile.Center);
            if (trailPositions.Count > MaxTrailPoints)
                trailPositions.RemoveAt(trailPositions.Count - 1);

            // Fire ember trail — frequent hot sparks scattering from the bullet
            if (Main.GameUpdateCount % 2 == 0)
            {
                Vector2 perpDir = Projectile.velocity.SafeNormalize(Vector2.Zero).RotatedBy(MathHelper.PiOver2);
                Vector2 emberOffset = perpDir * Main.rand.NextFloat(-4f, 4f);
                Vector2 emberVel = -Projectile.velocity * 0.08f + Main.rand.NextVector2Circular(1f, 1f);
                PiercingBellsParticleHandler.SpawnParticle(new BulletTracerParticle(
                    Projectile.Center + emberOffset, emberVel, Main.rand.Next(8, 16)));
            }

            // Occasional brighter tracer spark
            if (Main.rand.NextBool(4))
            {
                PiercingBellsParticleHandler.SpawnParticle(new BulletTracerParticle(
                    Projectile.Center, Main.rand.NextVector2Circular(2f, 2f),
                    Main.rand.Next(6, 12))
                    { Scale = Main.rand.NextFloat(0.25f, 0.4f) });
            }

            Lighting.AddLight(Projectile.Center, PiercingBellsResonanceUtils.StaccatoPalette[2].ToVector3() * 0.45f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.GetGlobalNPC<ResonantTollNPC>().AddStacks(target, 1);

            // Embed a Resonant Marker
            target.GetGlobalNPC<ResonantMarkerNPC>().AddMarker(target);

            // === FOUNDATION: RippleEffectProjectile — Marker embed ring ===
            Projectile.NewProjectile(
                Projectile.GetSource_FromThis(), target.Center, Vector2.Zero,
                ModContent.ProjectileType<RippleEffectProjectile>(),
                0, 0f, Projectile.owner, ai0: 1f);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            try
            {

            // Draw fire trail
            if (trailPositions.Count >= 2)
            {
                try
                {
                    trailRenderer ??= new PiercingBellsPrimitiveRenderer();

                    // === SHADER: BulletTrailShader — supersonic compression-wave trail ===
                    var bulletShader = PiercingBellsResonanceShaderLoader.GetBulletTrailShader();
                    if (bulletShader != null)
                    {
                        try
                        {
                            bulletShader.UseColor(PiercingBellsResonanceUtils.StaccatoPalette[2]);
                            bulletShader.UseSecondaryColor(PiercingBellsResonanceUtils.StaccatoPalette[0]);
                            bulletShader.UseOpacity(0.8f);
                            bulletShader.UseSaturation(0.9f); // uIntensity
                            var fx = bulletShader.Shader;
                            if (fx != null)
                            {
                                fx.Parameters["uTime"]?.SetValue((float)Main.GameUpdateCount * 0.03f);
                                fx.Parameters["uOverbrightMult"]?.SetValue(1.3f);
                                fx.Parameters["uScrollSpeed"]?.SetValue(2.5f);
                                fx.Parameters["uNoiseScale"]?.SetValue(4f);
                            }
                        }
                        catch { }
                    }

                    var settings = new BulletTrailSettings
                    {
                        ColorStart = PiercingBellsResonanceUtils.StaccatoPalette[2] * 0.8f,
                        ColorEnd = PiercingBellsResonanceUtils.StaccatoPalette[0] * 0.3f,
                        Width = 22f,
                        BloomIntensity = 0.6f,
                        Shader = bulletShader,
                        WidthFunc = t =>
                        {
                            float w = MathHelper.Lerp(22f, 3f, t);
                            return w * (1f + (float)Math.Sin(Main.GameUpdateCount * 0.2f) * 0.08f);
                        },
                        ColorFunc = t =>
                        {
                            Color c = Color.Lerp(PiercingBellsResonanceUtils.StaccatoPalette[2],
                                PiercingBellsResonanceUtils.StaccatoPalette[0], t);
                            return c * (1f - t * 0.6f) * 0.85f;
                        }
                    };
                    trailRenderer.DrawTrail(sb, trailPositions, settings, Main.screenPosition);
                }
                catch { }
            }

            // Draw bullet sprite
            var tex = ModContent.Request<Texture2D>(Texture, AssetRequestMode.ImmediateLoad).Value;
            Color drawColor = Color.Lerp(lightColor, PiercingBellsResonanceUtils.StaccatoPalette[2], 0.4f);
            sb.Draw(tex, Projectile.Center - Main.screenPosition, null, drawColor,
                Projectile.rotation, tex.Size() / 2f, Projectile.scale, SpriteEffects.None, 0f);

            // Additive bloom glow layers around bullet
            try { sb.End(); } catch { }
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            var bloomTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/SandboxLastPrism/Orbs/SoftGlow", AssetRequestMode.ImmediateLoad).Value;
            Vector2 bloomOrigin = bloomTex.Size() / 2f;
            Vector2 bulletScreen = Projectile.Center - Main.screenPosition;
            float bPulse = 0.9f + (float)Math.Sin(Main.GameUpdateCount * 0.25f) * 0.1f;

            // Outer ambient glow
            sb.Draw(bloomTex, bulletScreen, null,
                PiercingBellsResonanceUtils.StaccatoPalette[0] * (0.25f * bPulse),
                0f, bloomOrigin, 0.22f, SpriteEffects.None, 0f);
            // Mid warm glow
            sb.Draw(bloomTex, bulletScreen, null,
                PiercingBellsResonanceUtils.StaccatoPalette[2] * (0.4f * bPulse),
                0f, bloomOrigin, 0.12f, SpriteEffects.None, 0f);
            // Hot white core
            sb.Draw(bloomTex, bulletScreen, null,
                Color.White * (0.5f * bPulse * bPulse),
                0f, bloomOrigin, 0.05f, SpriteEffects.None, 0f);

            // Star sparkle accent at bullet tip
            Texture2D starTex = null;
            try { starTex = MagnumTextureRegistry.GetStar4Soft(); } catch { }
            if (starTex != null)
            {
                float starRot = Main.GameUpdateCount * 0.08f;
                sb.Draw(starTex, bulletScreen, null,
                    PiercingBellsResonanceUtils.StaccatoPalette[2] * (0.6f * bPulse),
                    starRot, starTex.Size() / 2f, 0.4f * bPulse, SpriteEffects.None, 0f);
            }

            // Theme texture accents
            PiercingBellsResonanceUtils.DrawThemeAccents(sb, bulletScreen, Projectile.scale);
            try { sb.End(); } catch { }
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            } // end outer try
            catch
            {
                try
                {
                    sb.End();
                    sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp,
                        DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
                }
                catch { }
            }
            return false;
        }

        public override void OnKill(int timeLeft)
        {
            trailRenderer?.Dispose();
            trailRenderer = null;

            // Impact spark burst — radial scatter of 8 tracers
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f + Main.rand.NextFloat(-0.3f, 0.3f);
                float speed = Main.rand.NextFloat(1.5f, 4f);
                PiercingBellsParticleHandler.SpawnParticle(new BulletTracerParticle(
                    Projectile.Center, angle.ToRotationVector2() * speed,
                    Main.rand.Next(10, 20))
                    { Scale = Main.rand.NextFloat(0.2f, 0.35f) });
            }

            // Central flash
            PiercingBellsParticleHandler.SpawnParticle(new ResonantBlastFlashParticle(
                Projectile.Center, 0.6f, 8));
        }
    }
}