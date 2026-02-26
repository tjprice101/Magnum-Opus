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
    /// VFX helper for the Chromatic Swan Song magic pistol.
    /// Handles hold-item ambient, item bloom, muzzle flash,
    /// projectile trail, small hit, and 3-hit combo explosion.
    ///
    /// The Chromatic Swan Song: prismatic aria — the most colorful
    /// Swan Lake weapon. Combo-driven saturation from monochrome to vivid rainbow.
    ///
    /// Shader-driven VFX (primary):
    ///   - ChromaticTrail.fx: Rainbow-banded trail with combo saturation (ChromaticTrailMain + ChromaticTrailGlow)
    ///   - AriaExplosion.fx: Full-spectrum prismatic detonation (AriaExplosionMain + AriaExplosionRing)
    ///
    /// Accent particles (reduced): feather drifts, music notes, prismatic sparkles.
    /// </summary>
    public static class ChromaticSwanSongVFX
    {
        // =====================================================================
        //  HOLD ITEM VFX
        // =====================================================================

        /// <summary>
        /// Per-frame held-item VFX: ambient aura and feather drift.
        /// Heavy prismatic flares removed — shader trail handles the spectral look.
        /// </summary>
        public static void HoldItemVFX(Player player)
        {
            if (Main.dedServ) return;

            Vector2 center = player.MountedCenter;
            float time = (float)Main.timeForVisualEffects;

            // Swan Lake aura
            try { UnifiedVFX.SwanLake.Aura(center, 30f); } catch { }

            // Feather drift (kept — lightweight)
            if (Main.rand.NextBool(20))
            {
                Color featherCol = Main.rand.NextBool() ? SwanLakePalette.FeatherWhite : SwanLakePalette.FeatherBlack;
                try { CustomParticles.SwanFeatherDrift(center + Main.rand.NextVector2Circular(18f, 18f), featherCol, 0.15f); } catch { }
            }

            // Rainbow light
            SwanLakeVFXLibrary.AddPulsingLight(center, time, 0.3f);
        }

        // =====================================================================
        //  PREDRAW IN WORLD BLOOM
        // =====================================================================

        /// <summary>
        /// Enhanced 4-layer prismatic item bloom for the Chromatic Swan Song sprite.
        /// </summary>
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
        //  MUZZLE FLASH VFX (reduced)
        // =====================================================================

        /// <summary>
        /// Muzzle flash: reduced from heavy spark burst to compact accents.
        /// </summary>
        public static void MuzzleFlashVFX(Vector2 muzzlePos, Vector2 direction)
        {
            if (Main.dedServ) return;

            // Core impact
            try { UnifiedVFX.SwanLake.Impact(muzzlePos, 0.8f); } catch { }

            // Directed spark burst (reduced: 8 -> 4)
            for (int i = 0; i < 4; i++)
            {
                float spreadAngle = Main.rand.NextFloat(-0.3f, 0.3f);
                Vector2 vel = direction.RotatedBy(spreadAngle) * Main.rand.NextFloat(3f, 7f);
                bool isWhite = Main.rand.NextBool();
                int dustType = isWhite ? DustID.WhiteTorch : DustID.Shadowflame;
                Dust d = Dust.NewDustPerfect(muzzlePos, dustType, vel, isWhite ? 0 : 100,
                    isWhite ? SwanLakePalette.PureWhite : SwanLakePalette.ObsidianBlack, 1.5f);
                d.noGravity = true;
            }

            // Prismatic sparkles (reduced: 4 -> 2)
            SwanLakeVFXLibrary.SpawnPrismaticSparkles(muzzlePos, 2, 10f);

            // Music notes (kept)
            SwanLakeVFXLibrary.SpawnMusicNotes(muzzlePos, 2, 12f, 0.7f, 0.9f, 22);

            Lighting.AddLight(muzzlePos, SwanLakePalette.PureWhite.ToVector3() * 0.8f);
        }

        // =====================================================================
        //  PROJECTILE TRAIL VFX (shader-driven + sparse accents)
        // =====================================================================

        /// <summary>
        /// Per-frame projectile trail: heavy per-frame particles removed.
        /// ChromaticTrail shader handles the primary visual.
        /// Sparse accent particles only.
        /// </summary>
        public static void ProjectileTrailVFX(Vector2 pos, Vector2 velocity)
        {
            if (Main.dedServ) return;

            // Feather trail (1-in-10, reduced from 1-in-6)
            if (Main.rand.NextBool(10))
            {
                Color featherCol = Main.rand.NextBool() ? SwanLakePalette.FeatherWhite : SwanLakePalette.FeatherBlack;
                try { CustomParticles.SwanFeatherDrift(pos + Main.rand.NextVector2Circular(6f, 6f), featherCol, 0.2f); } catch { }
            }

            // Music notes (1-in-10, reduced from 1-in-6)
            if (Main.rand.NextBool(10))
                SwanLakeVFXLibrary.SpawnMusicNotes(pos, 1, 8f, 0.7f, 0.85f, 22);

            SwanLakeVFXLibrary.AddSwanLight(pos, 0.4f);
        }

        // =====================================================================
        //  PROJECTILE TRAIL SHADER RENDERING
        // =====================================================================

        /// <summary>
        /// Draw shader-driven projectile trail using projectile oldPos history.
        /// Called from the projectile's PreDraw method.
        /// 3-pass rendering: Glow@3x -> Main@1x -> Glow@1.5x
        /// comboPhase controls spectral saturation (0 = first shot, 0.5 = second, 1.0 = third).
        /// </summary>
        public static void DrawProjectileShaderTrail(SpriteBatch sb, Projectile proj, float comboPhase)
        {
            if (Main.dedServ) return;
            if (!SwanLakeShaderManager.HasChromaticTrail)
            {
                DrawFallbackTrail(sb, proj);
                return;
            }

            Texture2D glowTex = MagnumTextureRegistry.GetSoftGlow();
            if (glowTex == null) return;

            Vector2 origin = glowTex.Size() * 0.5f;
            float time = Main.GlobalTimeWrappedHourly;

            try
            {
                SwanLakeShaderManager.BeginShaderAdditive(sb);
                SwanLakeShaderManager.BindSparklyNoiseTexture(Main.graphics.GraphicsDevice);

                // PASS 1: Glow underlay @ 3x width
                SwanLakeShaderManager.ApplySwanSongProjectileTrail(time, comboPhase, glow: true);
                DrawTrailPositions(sb, proj, glowTex, origin, scaleMult: 0.28f, alphaMult: 0.3f);

                // PASS 2: Main spectral core @ 1x width
                SwanLakeShaderManager.ApplySwanSongProjectileTrail(time, comboPhase, glow: false);
                DrawTrailPositions(sb, proj, glowTex, origin, scaleMult: 0.10f, alphaMult: 0.7f);

                // PASS 3: Glow overbright @ 1.5x width
                SwanLakeShaderManager.ApplySwanSongProjectileTrail(time, comboPhase, glow: true);
                DrawTrailPositions(sb, proj, glowTex, origin, scaleMult: 0.15f, alphaMult: 0.25f);

                SwanLakeShaderManager.RestoreSpriteBatch(sb);
            }
            catch
            {
                try { SwanLakeShaderManager.RestoreSpriteBatch(sb); } catch { }
                DrawFallbackTrail(sb, proj);
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
        /// Fallback trail rendering when shader is unavailable.
        /// Simple additive bloom dots with silver/white coloring.
        /// </summary>
        private static void DrawFallbackTrail(SpriteBatch sb, Projectile proj)
        {
            Texture2D glowTex = MagnumTextureRegistry.GetSoftGlow();
            if (glowTex == null) return;

            Vector2 origin = glowTex.Size() * 0.5f;

            for (int k = 0; k < proj.oldPos.Length; k++)
            {
                if (proj.oldPos[k] == Vector2.Zero) continue;
                Vector2 drawPos = proj.oldPos[k] - Main.screenPosition +
                    new Vector2(proj.width / 2f, proj.height / 2f);
                float progress = (proj.oldPos.Length - k) / (float)proj.oldPos.Length;

                Color trailColor = Color.Lerp(SwanLakePalette.Silver, SwanLakePalette.PureWhite, progress);
                sb.Draw(glowTex, drawPos, null,
                    SwanLakePalette.Additive(trailColor, 0.2f * progress),
                    proj.oldRot[k], origin, 0.07f + progress * 0.04f, SpriteEffects.None, 0f);
            }
        }

        // =====================================================================
        //  SMALL HIT VFX (reduced)
        // =====================================================================

        /// <summary>
        /// On-hit VFX for each shot: reduced from heavy dust bursts to
        /// compact sparkles + music note.
        /// </summary>
        public static void SmallHitVFX(Vector2 hitPos)
        {
            if (Main.dedServ) return;

            // Prismatic sparkles (reduced: 4 -> 2)
            SwanLakeVFXLibrary.SpawnPrismaticSparkles(hitPos, 2, 12f);

            // Halo ring (reduced: 3 -> 1)
            try { CustomParticles.HaloRing(hitPos, SwanLakePalette.PureWhite, 0.2f, 10); } catch { }

            // Music note (reduced: 2 -> 1)
            SwanLakeVFXLibrary.SpawnMusicNotes(hitPos, 1, 15f, 0.7f, 0.9f, 22);

            Lighting.AddLight(hitPos, SwanLakePalette.PureWhite.ToVector3() * 0.6f);
        }

        // =====================================================================
        //  3-HIT COMBO EXPLOSION VFX (shader-driven + reduced particles)
        // =====================================================================

        /// <summary>
        /// 3-hit combo explosion: AriaExplosion shader provides the core
        /// prismatic detonation. Heavy particle bursts replaced.
        /// </summary>
        public static void ComboExplosionVFX(Vector2 pos, float intensity = 1f)
        {
            if (Main.dedServ) return;

            // Core impact (kept — lightweight)
            try { UnifiedVFX.SwanLake.Impact(pos, 1.5f * intensity); } catch { }

            // Bloom (kept — lightweight)
            SwanLakeVFXLibrary.DrawBloom(pos, 0.8f * intensity);

            // Halo ring (reduced: 5+5 -> 1)
            SwanLakeVFXLibrary.SpawnGradientHaloRings(pos, 1, 0.3f * intensity);

            // Prismatic sparkles (reduced: 12 -> 4)
            SwanLakeVFXLibrary.SpawnPrismaticSparkles(pos, 4, 25f * intensity);

            // Feather burst (reduced: 8+4 -> 3)
            SwanLakeVFXLibrary.SpawnFeatherBurst(pos, 3, 0.35f);

            // Music notes (reduced: 6 -> 3)
            SwanLakeVFXLibrary.SpawnMusicNotes(pos, 3, 35f, 0.8f, 1.2f, 35);

            Lighting.AddLight(pos, SwanLakePalette.RainbowFlash.ToVector3() * 1.5f * intensity);
        }

        // =====================================================================
        //  COMBO EXPLOSION SHADER RENDERING
        // =====================================================================

        /// <summary>
        /// Draw shader-driven aria explosion using AriaExplosion.fx.
        /// Called from combo explosion rendering.
        /// 2-pass: Main spectral body + Ring shockwave.
        /// </summary>
        public static void DrawComboExplosionShader(SpriteBatch sb, Vector2 pos, float explosionAge, float scale = 1f)
        {
            if (Main.dedServ) return;
            if (!SwanLakeShaderManager.HasAriaExplosion) return;

            Texture2D glowTex = MagnumTextureRegistry.GetSoftGlow();
            if (glowTex == null) return;

            Vector2 origin = glowTex.Size() * 0.5f;
            Vector2 drawPos = pos - Main.screenPosition;
            float time = Main.GlobalTimeWrappedHourly;

            try
            {
                SwanLakeShaderManager.BeginShaderAdditive(sb);
                SwanLakeShaderManager.BindSparklyNoiseTexture(Main.graphics.GraphicsDevice);

                // PASS 1: Main spectral explosion body
                SwanLakeShaderManager.ApplySwanSongComboExplosion(time, explosionAge, ringOnly: false);
                float bodyScale = 1.8f * scale;
                float bodyAlpha = 1f - explosionAge * explosionAge;
                sb.Draw(glowTex, drawPos, null, Color.White * bodyAlpha,
                    0f, origin, bodyScale, SpriteEffects.None, 0f);

                // PASS 2: Rainbow ring shockwave overlay
                SwanLakeShaderManager.ApplySwanSongComboExplosion(time, explosionAge, ringOnly: true);
                float ringScale = 2.5f * scale * (0.5f + explosionAge * 0.5f);
                float ringAlpha = 0.8f * (1f - explosionAge);
                sb.Draw(glowTex, drawPos, null, Color.White * ringAlpha,
                    0f, origin, ringScale, SpriteEffects.None, 0f);

                SwanLakeShaderManager.RestoreSpriteBatch(sb);
            }
            catch
            {
                try { SwanLakeShaderManager.RestoreSpriteBatch(sb); } catch { }
            }
        }

        // =====================================================================
        //  DEATH VFX (reduced)
        // =====================================================================

        /// <summary>
        /// Projectile death VFX: reduced from heavy dust bursts.
        /// </summary>
        public static void ProjectileDeathVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            SwanLakeVFXLibrary.SpawnPrismaticSparkles(pos, 2, 10f);
            try { CustomParticles.HaloRing(pos, SwanLakePalette.SwanSilver, 0.2f, 10); } catch { }
        }
    }
}
