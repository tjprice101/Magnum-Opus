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

            // Fire ember trail
            if (Main.rand.NextBool(3))
            {
                PiercingBellsParticleHandler.SpawnParticle(new BulletTracerParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(3, 3),
                    -Projectile.velocity * 0.05f + Main.rand.NextVector2Circular(0.5f, 0.5f),
                    Main.rand.Next(8, 15)));
            }

            Lighting.AddLight(Projectile.Center, PiercingBellsResonanceUtils.StaccatoPalette[2].ToVector3() * 0.3f);
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
                        ColorStart = PiercingBellsResonanceUtils.StaccatoPalette[2] * 0.7f,
                        ColorEnd = PiercingBellsResonanceUtils.StaccatoPalette[0] * 0.2f,
                        Width = 4f,
                        BloomIntensity = 0.2f,
                        Shader = bulletShader
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

            // Theme texture accents
            try { sb.End(); } catch { }
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            PiercingBellsResonanceUtils.DrawThemeAccents(sb, Projectile.Center - Main.screenPosition, Projectile.scale);
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

            // Impact sparks
            for (int i = 0; i < 3; i++)
            {
                PiercingBellsParticleHandler.SpawnParticle(new BulletTracerParticle(
                    Projectile.Center, Main.rand.NextVector2Circular(3f, 3f),
                    Main.rand.Next(10, 18)));
            }
        }
    }
}