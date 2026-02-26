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
    /// VFX helper for Feather of the Iridescent Flock summon weapon.
    /// Handles hold-item ambient, summon formation, crystal orbit trail,
    /// flare attack, explosive beam, and death VFX.
    ///
    /// The Iridescent Flock: prismatic crystal sentinels in graceful orbit.
    ///
    /// Shader-driven VFX (primary):
    ///   - CrystalOrbitTrail.fx: Faceted prismatic orbit trail (CrystalOrbitMain + CrystalOrbitGlow)
    ///   - FlockAura.fx: Formation-node aura overlay (FlockAuraMain + FlockAuraGlow)
    ///
    /// Accent particles (reduced): feather drifts, music notes.
    /// </summary>
    public static class FeatheroftheIridescentFlockVFX
    {
        // =====================================================================
        //  HOLD ITEM VFX
        // =====================================================================

        public static void HoldItemVFX(Player player)
        {
            if (Main.dedServ) return;

            Vector2 center = player.MountedCenter;
            float time = (float)Main.timeForVisualEffects;

            // Subtle Swan Lake aura
            try { UnifiedVFX.SwanLake.Aura(center, 22f); } catch { }

            // Subtle feathers (kept — lightweight)
            if (Main.rand.NextBool(30))
            {
                Color featherCol = Main.rand.NextBool() ? SwanLakePalette.FeatherWhite : SwanLakePalette.FeatherBlack;
                try { CustomParticles.SwanFeatherDrift(center + Main.rand.NextVector2Circular(16f, 16f), featherCol, 0.12f); } catch { }
            }

            SwanLakeVFXLibrary.AddPulsingLight(center, time, 0.2f);
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
        //  FLOCK AURA SHADER RENDERING
        // =====================================================================

        /// <summary>
        /// Draw shader-driven formation aura around player.
        /// Called from hold-item rendering when crystals are active.
        /// 2-pass: Glow underlay + Main formation overlay.
        /// crystalCount: 1, 2, or 3 active crystals.
        /// </summary>
        public static void DrawFlockAuraShader(SpriteBatch sb, Vector2 playerCenter, int crystalCount)
        {
            if (Main.dedServ) return;
            if (!SwanLakeShaderManager.HasFlockAura) return;
            if (crystalCount <= 0) return;

            Texture2D glowTex = MagnumTextureRegistry.GetSoftGlow();
            if (glowTex == null) return;

            Vector2 origin = glowTex.Size() * 0.5f;
            Vector2 drawPos = playerCenter - Main.screenPosition;
            float time = Main.GlobalTimeWrappedHourly;
            float crystalPhase = crystalCount / 3f; // 0.33, 0.66, 1.0

            try
            {
                SwanLakeShaderManager.BeginShaderAdditive(sb);
                SwanLakeShaderManager.BindCrystalNoiseTexture(Main.graphics.GraphicsDevice);

                // PASS 1: Glow underlay
                SwanLakeShaderManager.ApplyIridescentFlockAura(time, crystalPhase, glow: true);
                sb.Draw(glowTex, drawPos, null, Color.White * 0.3f,
                    0f, origin, 2.0f, SpriteEffects.None, 0f);

                // PASS 2: Main formation overlay
                SwanLakeShaderManager.ApplyIridescentFlockAura(time, crystalPhase, glow: false);
                sb.Draw(glowTex, drawPos, null, Color.White * 0.5f,
                    0f, origin, 1.5f, SpriteEffects.None, 0f);

                SwanLakeShaderManager.RestoreSpriteBatch(sb);
            }
            catch
            {
                try { SwanLakeShaderManager.RestoreSpriteBatch(sb); } catch { }
            }
        }

        // =====================================================================
        //  SUMMON FORMATION VFX (reduced)
        // =====================================================================

        /// <summary>
        /// VFX when crystals are first summoned: reduced from heavy explosion.
        /// </summary>
        public static void SummonFormationVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            try { ThemedParticles.SwanLakeMusicalImpact(pos, 0.7f); } catch { }

            // Halo rings (reduced: 4 -> 2)
            SwanLakeVFXLibrary.SpawnGradientHaloRings(pos, 2, 0.25f);

            // Prismatic sparkles (reduced: 6 -> 3)
            SwanLakeVFXLibrary.SpawnPrismaticSparkles(pos, 3, 15f);

            // Feather burst (reduced: 4 -> 2)
            SwanLakeVFXLibrary.SpawnFeatherBurst(pos, 2, 0.25f);

            // Music notes (kept)
            SwanLakeVFXLibrary.SpawnMusicNotes(pos, 3, 20f, 0.75f, 1.0f, 25);

            Lighting.AddLight(pos, SwanLakePalette.PureWhite.ToVector3() * 0.8f);
        }

        // =====================================================================
        //  CRYSTAL ORBIT TRAIL VFX (shader-driven + sparse accents)
        // =====================================================================

        /// <summary>
        /// Per-frame crystal orbit trail: heavy per-frame particles removed.
        /// CrystalOrbitTrail shader handles the primary visual.
        /// Sparse accents only.
        /// </summary>
        public static void CrystalOrbitTrailVFX(Vector2 pos, Vector2 velocity)
        {
            if (Main.dedServ) return;

            // Feather trail (1-in-10, reduced from 1-in-6)
            if (Main.rand.NextBool(10))
            {
                Color featherCol = Main.rand.NextBool() ? SwanLakePalette.FeatherWhite : SwanLakePalette.FeatherBlack;
                try { CustomParticles.SwanFeatherDrift(pos, featherCol, 0.18f); } catch { }
            }

            // Music notes (1-in-12, reduced from 1-in-8)
            if (Main.rand.NextBool(12))
                SwanLakeVFXLibrary.SpawnMusicNotes(pos, 1, 6f, 0.7f, 0.85f, 20);

            SwanLakeVFXLibrary.AddSwanLight(pos, 0.35f);
        }

        // =====================================================================
        //  CRYSTAL ORBIT TRAIL SHADER RENDERING
        // =====================================================================

        /// <summary>
        /// Draw shader-driven crystal orbit trail using projectile oldPos history.
        /// Called from the crystal projectile's PreDraw method.
        /// 3-pass rendering with per-crystal hue offset.
        /// crystalIndex: 0, 1, or 2 (selects hue identity).
        /// </summary>
        public static void DrawCrystalOrbitShaderTrail(SpriteBatch sb, Projectile proj, int crystalIndex)
        {
            if (Main.dedServ) return;
            if (!SwanLakeShaderManager.HasCrystalOrbitTrail)
            {
                DrawFallbackOrbitTrail(sb, proj, crystalIndex);
                return;
            }

            Texture2D glowTex = MagnumTextureRegistry.GetSoftGlow();
            if (glowTex == null) return;

            Vector2 origin = glowTex.Size() * 0.5f;
            float time = Main.GlobalTimeWrappedHourly;
            float crystalPhase = crystalIndex / 3f; // 0.0, 0.33, 0.66

            try
            {
                SwanLakeShaderManager.BeginShaderAdditive(sb);
                SwanLakeShaderManager.BindCrystalNoiseTexture(Main.graphics.GraphicsDevice);

                // PASS 1: Glow underlay @ 3x width
                SwanLakeShaderManager.ApplyIridescentFlockOrbitTrail(time, crystalPhase, glow: true);
                DrawTrailPositions(sb, proj, glowTex, origin, scaleMult: 0.22f, alphaMult: 0.3f);

                // PASS 2: Main faceted core @ 1x width
                SwanLakeShaderManager.ApplyIridescentFlockOrbitTrail(time, crystalPhase, glow: false);
                DrawTrailPositions(sb, proj, glowTex, origin, scaleMult: 0.08f, alphaMult: 0.7f);

                // PASS 3: Glow overbright @ 1.5x width
                SwanLakeShaderManager.ApplyIridescentFlockOrbitTrail(time, crystalPhase, glow: true);
                DrawTrailPositions(sb, proj, glowTex, origin, scaleMult: 0.12f, alphaMult: 0.25f);

                SwanLakeShaderManager.RestoreSpriteBatch(sb);
            }
            catch
            {
                try { SwanLakeShaderManager.RestoreSpriteBatch(sb); } catch { }
                DrawFallbackOrbitTrail(sb, proj, crystalIndex);
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
        /// Fallback orbit trail rendering when shader is unavailable.
        /// Simple additive bloom dots with per-crystal coloring.
        /// </summary>
        private static void DrawFallbackOrbitTrail(SpriteBatch sb, Projectile proj, int crystalIndex)
        {
            Texture2D glowTex = MagnumTextureRegistry.GetSoftGlow();
            if (glowTex == null) return;

            Vector2 origin = glowTex.Size() * 0.5f;

            // Per-crystal color variation
            Color primary = crystalIndex switch
            {
                0 => new Color(255, 200, 100), // Warm gold
                1 => new Color(100, 220, 150), // Emerald
                _ => new Color(150, 130, 255), // Violet
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
        //  FLARE ATTACK VFX (reduced)
        // =====================================================================

        /// <summary>
        /// Explosive flare burst when crystals attack: reduced from
        /// 14-flare loop + 24 dust to compact accents.
        /// </summary>
        public static void FlareAttackVFX(Vector2 pos, float intensity = 1f)
        {
            if (Main.dedServ) return;

            // Bloom (kept — lightweight)
            SwanLakeVFXLibrary.DrawBloom(pos, 0.6f * intensity);

            // Halo rings (reduced: 4+3 -> 2)
            SwanLakeVFXLibrary.SpawnGradientHaloRings(pos, 2, 0.25f * intensity);

            // Feather burst (reduced: 5 -> 2)
            SwanLakeVFXLibrary.SpawnFeatherBurst(pos, 2, 0.3f);

            // Music notes (reduced: 4 -> 2)
            SwanLakeVFXLibrary.SpawnMusicNotes(pos, 2, 20f, 0.75f, 1.0f, 25);

            Lighting.AddLight(pos, SwanLakePalette.PureWhite.ToVector3() * 1.0f * intensity);
        }

        // =====================================================================
        //  EXPLOSIVE BEAM VFX (reduced)
        // =====================================================================

        /// <summary>
        /// Explosive beam VFX: reduced from 36-dust meteor shower + massive
        /// particle bursts to compact shader-friendly accents.
        /// </summary>
        public static void ExplosiveBeamVFX(Vector2 pos, Vector2 targetDir, float intensity = 1f)
        {
            if (Main.dedServ) return;

            try { ThemedParticles.SwanLakeMusicalImpact(pos, intensity); } catch { }

            // Halo rings (reduced: 5+4 -> 3)
            SwanLakeVFXLibrary.SpawnGradientHaloRings(pos, 3, 0.25f * intensity);

            // Prismatic sparkles (reduced: 10 -> 4)
            SwanLakeVFXLibrary.SpawnPrismaticSparkles(pos, 4, 25f);

            // Feather burst (reduced: 8+4 -> 3)
            SwanLakeVFXLibrary.SpawnFeatherBurst(pos, 3, 0.35f);

            // Music notes (reduced: 6 -> 3)
            SwanLakeVFXLibrary.SpawnMusicNotes(pos, 3, 35f, 0.8f, 1.2f, 35);

            // Bloom
            SwanLakeVFXLibrary.DrawBloom(pos, 0.8f * intensity);

            Lighting.AddLight(pos, SwanLakePalette.RainbowFlash.ToVector3() * 1.5f * intensity);
        }

        // =====================================================================
        //  CRYSTAL IMPACT VFX (reduced)
        // =====================================================================

        /// <summary>
        /// Crystal hit impact: reduced from heavy rainbow explosion.
        /// </summary>
        public static void CrystalImpactVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            SwanLakeVFXLibrary.ProjectileImpact(pos, 0.8f);
        }
    }
}
