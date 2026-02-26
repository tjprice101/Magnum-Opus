using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.VFX.Core;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Content.SwanLake.ResonantWeapons
{
    /// <summary>
    /// VFX helper for the Iridescent Wingspan magic staff (3-flare spread).
    /// Handles hold-item ambient (ethereal wing silhouettes), item bloom,
    /// cast VFX (wing unfurl), projectile trail, impact, and death VFX.
    ///
    /// The Iridescent Wingspan: ethereal wings manifest as cascading prismatic energy.
    ///
    /// Shader-driven VFX (primary):
    ///   - EtherealWing.fx: Procedural wing silhouette overlay (EtherealWingMain + EtherealWingGlow)
    ///   - WingspanFlareTrail.fx: Feather-dissolve homing trail (WingspanFlareMain + WingspanFlareGlow)
    ///
    /// Accent particles (reduced): feather drifts, music notes.
    /// </summary>
    public static class IridescentWingspanVFX
    {
        // =====================================================================
        //  HOLD ITEM VFX — ETHEREAL WINGS (shader-driven + sparse accents)
        // =====================================================================

        /// <summary>
        /// Per-frame held-item VFX: EtherealWing shader renders the primary
        /// wing silhouette. Heavy GenericGlow wing particles removed.
        /// Sparse accent feathers and music notes kept.
        /// </summary>
        public static void HoldItemVFX(Player player)
        {
            if (Main.dedServ) return;

            Vector2 center = player.MountedCenter;
            float time = (float)Main.timeForVisualEffects;

            // Subtle Swan Lake aura
            try { UnifiedVFX.SwanLake.Aura(center, 22f); } catch { }

            // Falling feather drift (reduced: 1-in-12 -> 1-in-16)
            if (Main.rand.NextBool(16))
            {
                float side = Main.rand.NextBool() ? -1f : 1f;
                Vector2 driftPos = center + new Vector2(side * Main.rand.NextFloat(10f, 25f), -15f);
                Color featherCol = Main.rand.NextBool() ? SwanLakePalette.FeatherWhite : SwanLakePalette.Pearlescent;
                try { CustomParticles.SwanFeatherDrift(driftPos, featherCol, 0.18f); } catch { }
            }

            // Music notes (reduced: 1-in-15 -> 1-in-20)
            if (Main.rand.NextBool(20))
                SwanLakeVFXLibrary.SpawnMusicNotes(center, 1, 15f, 0.7f, 0.85f, 25);

            SwanLakeVFXLibrary.AddPulsingLight(center, time, 0.3f);
        }

        // =====================================================================
        //  ETHEREAL WING SHADER RENDERING
        // =====================================================================

        /// <summary>
        /// Draw shader-driven ethereal wing silhouettes behind the player.
        /// Called from hold-item rendering.
        /// 2-pass: Glow underlay + Main wing overlay.
        /// unfurlPhase: 0 = subtle idle, 1 = full cast burst.
        /// </summary>
        public static void DrawEtherealWingShader(SpriteBatch sb, Vector2 playerCenter, float unfurlPhase = 0f)
        {
            if (Main.dedServ) return;
            if (!SwanLakeShaderManager.HasEtherealWing) return;

            Texture2D glowTex = MagnumTextureRegistry.GetSoftGlow();
            if (glowTex == null) return;

            Vector2 origin = glowTex.Size() * 0.5f;
            Vector2 drawPos = playerCenter - Main.screenPosition;
            float time = Main.GlobalTimeWrappedHourly;

            try
            {
                SwanLakeShaderManager.BeginShaderAdditive(sb);
                SwanLakeShaderManager.BindNoiseTexture(Main.graphics.GraphicsDevice);

                // PASS 1: Glow underlay — wider, softer
                SwanLakeShaderManager.ApplyIridescentWingspanWings(time, unfurlPhase, glow: true);
                sb.Draw(glowTex, drawPos, null, Color.White * 0.3f,
                    0f, origin, 2.5f, SpriteEffects.None, 0f);

                // PASS 2: Main wing overlay — sharp feather lines
                SwanLakeShaderManager.ApplyIridescentWingspanWings(time, unfurlPhase, glow: false);
                sb.Draw(glowTex, drawPos, null, Color.White * 0.5f,
                    0f, origin, 2.0f, SpriteEffects.None, 0f);

                SwanLakeShaderManager.RestoreSpriteBatch(sb);
            }
            catch
            {
                try { SwanLakeShaderManager.RestoreSpriteBatch(sb); } catch { }
            }
        }

        // =====================================================================
        //  PREDRAW IN WORLD BLOOM
        // =====================================================================

        public static void PreDrawInWorldBloom(
            SpriteBatch sb,
            Texture2D tex,
            Vector2 pos, Vector2 origin, float rotation, float scale)
        {
            float time = (float)Main.timeForVisualEffects;
            float pulse = 1f + MathF.Sin(time * 0.04f) * 0.03f;
            SwanLakePalette.DrawItemBloomEnhanced(sb, tex, pos, origin, rotation, scale, pulse, time);
        }

        // =====================================================================
        //  CAST VFX — WING UNFURL (reduced)
        // =====================================================================

        /// <summary>
        /// Cast VFX: wing unfurl flash. EtherealWing shader provides the
        /// primary unfurl visual (unfurlPhase = 1). Heavy 12-particle burst
        /// and RainbowExplosion removed. Modest accent particles kept.
        /// </summary>
        public static void CastVFX(Vector2 pos, Vector2 direction)
        {
            if (Main.dedServ) return;

            try { ThemedParticles.SwanLakeMusicalImpact(pos, 0.7f); } catch { }

            // Prismatic sparkles (reduced: 8 -> 4)
            SwanLakeVFXLibrary.SpawnPrismaticSparkles(pos, 4, 20f);

            // Gradient halo rings (reduced: 4+3 -> 2)
            SwanLakeVFXLibrary.SpawnGradientHaloRings(pos, 2, 0.25f);

            // Feather burst (reduced: 4+3 -> 2)
            SwanLakeVFXLibrary.SpawnFeatherBurst(pos, 2, 0.3f);

            // Music notes (kept)
            SwanLakeVFXLibrary.SpawnMusicNotes(pos, 3, 20f, 0.75f, 1.0f, 28);

            Lighting.AddLight(pos, SwanLakePalette.PureWhite.ToVector3() * 0.9f);
        }

        // =====================================================================
        //  PROJECTILE TRAIL VFX (shader-driven + sparse accents)
        // =====================================================================

        /// <summary>
        /// Per-frame projectile trail: heavy per-frame particles removed.
        /// WingspanFlareTrail shader handles the primary visual.
        /// Sparse accent particles only.
        /// </summary>
        public static void ProjectileTrailVFX(Vector2 pos, Vector2 velocity)
        {
            if (Main.dedServ) return;

            // Feather trail (1-in-10, reduced from 1-in-6)
            if (Main.rand.NextBool(10))
            {
                Color featherCol = Main.rand.NextBool() ? SwanLakePalette.FeatherWhite : SwanLakePalette.FeatherBlack;
                try { CustomParticles.SwanFeatherDrift(pos, featherCol, 0.2f); } catch { }
            }

            // Music notes (1-in-12, reduced from 1-in-6)
            if (Main.rand.NextBool(12))
                SwanLakeVFXLibrary.SpawnMusicNotes(pos, 1, 8f, 0.7f, 0.85f, 22);

            SwanLakeVFXLibrary.AddSwanLight(pos, 0.4f);
        }

        // =====================================================================
        //  FLARE TRAIL SHADER RENDERING
        // =====================================================================

        /// <summary>
        /// Draw shader-driven flare trail using projectile oldPos history.
        /// Called from the flare projectile's PreDraw method.
        /// 3-pass rendering with per-projectile hue offset.
        /// projectileIndex: 0, 1, or 2 (selects hue identity).
        /// </summary>
        public static void DrawFlareShaderTrail(SpriteBatch sb, Projectile proj, int projectileIndex)
        {
            if (Main.dedServ) return;
            if (!SwanLakeShaderManager.HasWingspanFlareTrail)
            {
                DrawFallbackFlareTrail(sb, proj, projectileIndex);
                return;
            }

            Texture2D glowTex = MagnumTextureRegistry.GetSoftGlow();
            if (glowTex == null) return;

            Vector2 origin = glowTex.Size() * 0.5f;
            float time = Main.GlobalTimeWrappedHourly;
            float projPhase = projectileIndex / 3f; // 0.0, 0.33, 0.66

            try
            {
                SwanLakeShaderManager.BeginShaderAdditive(sb);
                SwanLakeShaderManager.BindFBMNoiseTexture(Main.graphics.GraphicsDevice);

                // PASS 1: Glow underlay @ 2.5x width
                SwanLakeShaderManager.ApplyIridescentWingspanFlareTrail(time, projPhase, glow: true);
                DrawTrailPositions(sb, proj, glowTex, origin, scaleMult: 0.20f, alphaMult: 0.3f);

                // PASS 2: Main feather-dissolve core @ 1x width
                SwanLakeShaderManager.ApplyIridescentWingspanFlareTrail(time, projPhase, glow: false);
                DrawTrailPositions(sb, proj, glowTex, origin, scaleMult: 0.08f, alphaMult: 0.7f);

                // PASS 3: Glow overbright @ 1.5x width
                SwanLakeShaderManager.ApplyIridescentWingspanFlareTrail(time, projPhase, glow: true);
                DrawTrailPositions(sb, proj, glowTex, origin, scaleMult: 0.12f, alphaMult: 0.25f);

                SwanLakeShaderManager.RestoreSpriteBatch(sb);
            }
            catch
            {
                try { SwanLakeShaderManager.RestoreSpriteBatch(sb); } catch { }
                DrawFallbackFlareTrail(sb, proj, projectileIndex);
            }
        }

        /// <summary>
        /// Loops through projectile oldPos to draw trail history at given scale/alpha.
        /// </summary>
        private static void DrawTrailPositions(SpriteBatch sb, Projectile proj,
            Texture2D tex, Vector2 origin, float scaleMult, float alphaMult)
        {
            for (int k = 0; k < proj.oldPos.Length; k++)
            {
                if (proj.oldPos[k] == Vector2.Zero) continue;
                Vector2 drawPos = proj.oldPos[k] - Main.screenPosition +
                    new Vector2(proj.width / 2f, proj.height / 2f);
                float progress = (proj.oldPos.Length - k) / (float)proj.oldPos.Length;
                float alpha = progress * alphaMult;
                float scale = scaleMult + progress * scaleMult * 0.5f;

                sb.Draw(tex, drawPos, null, Color.White * alpha,
                    proj.oldRot[k], origin, scale, SpriteEffects.None, 0f);
            }
        }

        /// <summary>
        /// Fallback flare trail rendering when shader is unavailable.
        /// Simple additive bloom dots with per-projectile coloring.
        /// </summary>
        private static void DrawFallbackFlareTrail(SpriteBatch sb, Projectile proj, int projectileIndex)
        {
            Texture2D glowTex = MagnumTextureRegistry.GetSoftGlow();
            if (glowTex == null) return;

            Vector2 origin = glowTex.Size() * 0.5f;

            // Per-projectile color variation
            Color primary = projectileIndex switch
            {
                0 => new Color(255, 230, 200), // Warm pearlescent
                1 => new Color(200, 240, 230), // Cool pearlescent
                _ => new Color(230, 200, 255), // Violet pearlescent
            };

            for (int k = 0; k < proj.oldPos.Length; k++)
            {
                if (proj.oldPos[k] == Vector2.Zero) continue;
                Vector2 drawPos = proj.oldPos[k] - Main.screenPosition +
                    new Vector2(proj.width / 2f, proj.height / 2f);
                float progress = (proj.oldPos.Length - k) / (float)proj.oldPos.Length;

                Color trailColor = Color.Lerp(SwanLakePalette.Silver, primary, progress);
                sb.Draw(glowTex, drawPos, null,
                    SwanLakePalette.Additive(trailColor, 0.2f * progress),
                    proj.oldRot[k], origin, 0.06f + progress * 0.04f, SpriteEffects.None, 0f);
            }
        }

        // =====================================================================
        //  IMPACT VFX (reduced)
        // =====================================================================

        /// <summary>
        /// Projectile impact: reduced from heavy rainbow explosion.
        /// </summary>
        public static void ImpactVFX(Vector2 pos, float intensity = 1f)
        {
            if (Main.dedServ) return;

            SwanLakeVFXLibrary.ProjectileImpact(pos, intensity);

            // Prismatic sparkles (reduced: keep lightweight)
            SwanLakeVFXLibrary.SpawnPrismaticSparkles(pos, 2, 15f);

            // Music notes (kept)
            SwanLakeVFXLibrary.SpawnMusicNotes(pos, 2, 12f, 0.7f, 0.9f, 22);

            Lighting.AddLight(pos, SwanLakePalette.PureWhite.ToVector3() * 0.8f * intensity);
        }

        // =====================================================================
        //  DEATH VFX (reduced)
        // =====================================================================

        /// <summary>
        /// Projectile death: reduced from heavy rainbow explosion + fractal gem burst.
        /// </summary>
        public static void ProjectileDeathVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            // Prismatic sparkles (reduced: 4 -> 2)
            SwanLakeVFXLibrary.SpawnPrismaticSparkles(pos, 2, 12f);

            // Music notes (reduced: 3 -> 2)
            SwanLakeVFXLibrary.SpawnMusicNotes(pos, 2, 15f, 0.7f, 0.9f, 22);

            // Feather burst (kept — lightweight)
            SwanLakeVFXLibrary.SpawnFeatherBurst(pos, 1, 0.2f);

            Lighting.AddLight(pos, SwanLakePalette.PureWhite.ToVector3() * 0.6f);
        }
    }
}
