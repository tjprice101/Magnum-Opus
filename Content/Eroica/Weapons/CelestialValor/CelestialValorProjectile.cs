using MagnumOpus.Common;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.VFX.Sparkle;
using MagnumOpus.Content.MoonlightSonata.Debuffs;
using MagnumOpus.Content.Eroica;
using MagnumOpus.Content.FoundationWeapons.SwordSmearFoundation;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Eroica.Weapons.CelestialValor
{
    /// <summary>
    /// Heroic flame projectile - thrown melee projectile with flame trail,
    /// afterimage chain, and crimson-gold bloom overlay.
    /// 
    /// ARCHITECTURE: Built on Foundation rendering (SMFTextures).
    /// - Trail: Afterimage chain using position buffer + SoftGlow/PointBloom bloom per point
    /// - Body: Multi-scale bloom stack (outer haze, main glow, hot core)
    /// - Accent: StarFlare rotating flare at head
    /// </summary>
    public class CelestialValorProjectile : ModProjectile
    {
        private const int TrailLength = 12;
        private Vector2[] trailPositions = new Vector2[TrailLength];
        private bool trailInitialized = false;

        public override void SetDefaults()
        {
            Projectile.width = 40;
            Projectile.height = 40;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 3;
            Projectile.timeLeft = 90;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.alpha = 0;
            Projectile.extraUpdates = 1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 15;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();

            // Trail tracking
            if (!trailInitialized)
            {
                trailInitialized = true;
                for (int i = 0; i < TrailLength; i++)
                    trailPositions[i] = Projectile.Center;
            }
            for (int i = TrailLength - 1; i > 0; i--)
                trailPositions[i] = trailPositions[i - 1];
            trailPositions[0] = Projectile.Center;

            // Flame trail dust
            EroicaVFXLibrary.SpawnFlameTrailDust(Projectile.Center, Projectile.velocity);

            // Gravity
            Projectile.velocity.Y += 0.05f;

            // Lighting
            EroicaVFXLibrary.AddPaletteLighting(Projectile.Center, 0.4f, 0.7f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<MusicsDissonance>(), 180);
            EroicaVFXLibrary.MeleeImpact(target.Center, 1);
            EroicaVFXLibrary.SpawnDirectionalSparks(target.Center,
                (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX), 4, 5f);
        }

        public override void OnKill(int timeLeft)
        {
            EroicaVFXLibrary.SpawnRadialDustBurst(Projectile.Center, 10, 5f);
            EroicaVFXLibrary.SpawnValorSparkles(Projectile.Center, 6, 25f);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            try
            {

            // === GPU Primitive Ribbon Trail (EnhancedTrailRenderer) ===
            EnhancedTrailRenderer.RenderMultiPassTrail(
                trailPositions,
                EnhancedTrailRenderer.LinearTaper(20f),
                EnhancedTrailRenderer.GradientColor(
                    EroicaPalette.Gold with { A = 0 },
                    EroicaPalette.Scarlet with { A = 0 } * 0.15f,
                    0.8f),
                bloomMultiplier: 2.5f, coreMultiplier: 0.4f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.TransformationMatrix);

            // LAYER 1: Trail sparkle chain — replaces SoftGlow×3 per-point stacking
            float time = (float)Main.timeForVisualEffects;
            Color[] trailColors = new Color[] {
                EroicaPalette.Scarlet,
                EroicaPalette.Gold,
                EroicaPalette.HotCore,
                Color.White,
            };
            for (int i = TrailLength - 1; i > 0; i--)
            {
                float progress = (float)i / TrailLength;
                float headStrength = 1f - progress;
                float fade = headStrength * headStrength;
                if (fade < 0.05f) continue;

                SparkleBloomHelper.DrawSparkleBloom(sb, trailPositions[i], SparkleTheme.Eroica,
                    trailColors, fade * 0.6f, MathHelper.Lerp(4f, 12f, headStrength), 2, time,
                    seed: i * 0.41f + Projectile.identity * 0.13f, sparkleScale: 0.018f);
            }

            // LAYER 2: Head sparkle bloom — replaces SoftGlow×2 + PointBloom + StarFlare
            Color[] headColors = new Color[] {
                EroicaPalette.Scarlet,
                EroicaPalette.Gold,
                EroicaPalette.HotCore,
                new Color(255, 150, 180),  // Sakura
                Color.White,
            };
            SparkleBloomHelper.DrawSparkleBloom(sb, Projectile.Center, SparkleTheme.Eroica,
                headColors, 0.8f, 22f, 6, time,
                seed: Projectile.identity * 0.67f, sparkleScale: 0.03f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.TransformationMatrix);

            }
            catch { }
            finally
            {
                try { sb.End(); } catch { }
                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                    DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            }

            return false;
        }
    }
}
