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
    /// VFX helper for the Call of the Black Swan greatsword.
    /// Handles hold-item ambient, item bloom, swing-frame dust,
    /// blade-tip effects, on-hit impacts, combo specials, and finisher.
    ///
    /// The Black Swan: dual-polarity (black/white) with prismatic accents.
    ///
    /// Shader-driven VFX (primary):
    ///   - DualPolaritySwing.fx: Black/white polarity swing trail (DualPolarityFlow + DualPolarityGlow)
    ///   - SwanFlareTrail.fx: Polarity-swappable thin tracer (SwanFlareMain + SwanFlareGlow)
    ///
    /// Accent particles (reduced): feather drifts, music notes, prismatic sparkles.
    /// </summary>
    public static class CalloftheBlackSwanVFX
    {
        // =====================================================================
        //  HOLD ITEM VFX
        // =====================================================================

        /// <summary>
        /// Per-frame held-item VFX: ambient aura, subtle feather drift,
        /// and dual-polarity pulsing glow.
        /// </summary>
        public static void HoldItemVFX(Player player)
        {
            if (Main.dedServ) return;

            Vector2 center = player.MountedCenter;
            float time = (float)Main.timeForVisualEffects;

            // Swan Lake aura
            try { UnifiedVFX.SwanLake.Aura(center, 28f); } catch { }

            // Subtle ambient feather drift (reduced from 1-in-25 to 1-in-28)
            if (Main.rand.NextBool(28))
            {
                Color featherCol = Main.rand.NextBool() ? SwanLakePalette.FeatherWhite : SwanLakePalette.FeatherBlack;
                try { CustomParticles.SwanFeatherDrift(center + Main.rand.NextVector2Circular(20f, 20f), featherCol, 0.18f); } catch { }
            }

            // Pulsing dual-polarity glow
            float pulse = 0.25f + MathF.Sin(time * 0.05f) * 0.1f;
            SwanLakeVFXLibrary.AddDualPolarityLight(center, time, pulse);
        }

        // =====================================================================
        //  PREDRAW IN WORLD BLOOM
        // =====================================================================

        /// <summary>
        /// Standard 3-layer dual-polarity item bloom for the Black Swan blade sprite.
        /// </summary>
        public static void PreDrawInWorldBloom(
            SpriteBatch sb,
            Texture2D tex,
            Vector2 pos, Vector2 origin, float rotation, float scale)
        {
            float time = (float)Main.timeForVisualEffects;
            float pulse = 1f + MathF.Sin(time * 0.04f) * 0.03f;
            SwanLakePalette.DrawItemBloom(sb, tex, pos, origin, rotation, scale, pulse);
        }

        // =====================================================================
        //  SWING FRAME VFX (shader-driven + reduced particles)
        // =====================================================================

        /// <summary>
        /// Per-frame blade swing VFX: periodic feather drifts,
        /// music notes along the arc, and light bloom at blade tip.
        /// Heavy dual-polarity dust and rainbow shimmer removed —
        /// replaced by DualPolaritySwing shader trail.
        /// </summary>
        public static void SwingFrameVFX(Vector2 tipPos, Vector2 swordDirection,
            int comboStep, int timer)
        {
            if (Main.dedServ) return;

            // Periodic feather drift (every 10 frames, up from 8)
            if (timer % 10 == 0)
            {
                Color featherCol = Main.rand.NextBool() ? SwanLakePalette.FeatherWhite : SwanLakePalette.FeatherBlack;
                try { CustomParticles.SwanFeatherDrift(tipPos + Main.rand.NextVector2Circular(10f, 10f), featherCol, 0.25f); } catch { }
            }

            // Music notes (every 6 frames, up from 5)
            if (timer % 6 == 0)
                SwanLakeVFXLibrary.SpawnMusicNotes(tipPos, 1, 10f, 0.7f, 0.9f, 25);

            // Bloom at blade tip
            float bloomOpacity = 0.5f + comboStep * 0.15f;
            SwanLakeVFXLibrary.DrawBloom(tipPos, 0.3f + comboStep * 0.06f, bloomOpacity);

            SwanLakeVFXLibrary.AddPaletteLighting(tipPos, 0.4f + comboStep * 0.15f, 0.6f);
        }

        // =====================================================================
        //  SWING TRAIL SHADER RENDERING
        // =====================================================================

        /// <summary>
        /// Draw shader-driven swing trail using projectile oldPos history.
        /// Called from the swing projectile's PreDraw method.
        /// 3-pass rendering: Glow@3x -> Main@1x -> Glow@1.5x
        /// </summary>
        public static void DrawSwingTrail(SpriteBatch sb, Projectile proj, int comboStep)
        {
            if (Main.dedServ) return;
            if (!SwanLakeShaderManager.HasDualPolaritySwing)
            {
                DrawFallbackSwingTrail(sb, proj);
                return;
            }

            Texture2D glowTex = MagnumTextureRegistry.GetSoftGlow();
            if (glowTex == null) return;

            Vector2 origin = glowTex.Size() * 0.5f;
            float time = Main.GlobalTimeWrappedHourly;
            float comboPhase = comboStep / 2f; // 0.0, 0.5, 1.0

            try
            {
                SwanLakeShaderManager.BeginShaderAdditive(sb);
                SwanLakeShaderManager.BindNoiseTexture(Main.graphics.GraphicsDevice);

                // PASS 1: Glow underlay @ 3x width
                SwanLakeShaderManager.ApplyBlackSwanSwingTrail(time, comboPhase, glow: true);
                DrawTrailPositions(sb, proj, glowTex, origin, scaleMult: 0.30f, alphaMult: 0.3f);

                // PASS 2: Main sharp polarity trail @ 1x width
                SwanLakeShaderManager.ApplyBlackSwanSwingTrail(time, comboPhase, glow: false);
                DrawTrailPositions(sb, proj, glowTex, origin, scaleMult: 0.10f, alphaMult: 0.7f);

                // PASS 3: Glow overbright @ 1.5x width
                SwanLakeShaderManager.ApplyBlackSwanSwingTrail(time, comboPhase, glow: true);
                DrawTrailPositions(sb, proj, glowTex, origin, scaleMult: 0.15f, alphaMult: 0.25f);

                SwanLakeShaderManager.RestoreSpriteBatch(sb);
            }
            catch
            {
                try { SwanLakeShaderManager.RestoreSpriteBatch(sb); } catch { }
                DrawFallbackSwingTrail(sb, proj);
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
        /// Fallback swing trail rendering when shader is unavailable.
        /// Simple additive bloom dots with polarity coloring.
        /// </summary>
        private static void DrawFallbackSwingTrail(SpriteBatch sb, Projectile proj)
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

                // Polarity gradient: alternate black and white along trail
                Color trailColor = Color.Lerp(SwanLakePalette.ObsidianBlack, SwanLakePalette.PureWhite,
                    (k % 2 == 0) ? 0.2f : 0.8f);
                sb.Draw(glowTex, drawPos, null,
                    SwanLakePalette.Additive(trailColor, 0.2f * progress),
                    proj.oldRot[k], origin, 0.08f + progress * 0.05f, SpriteEffects.None, 0f);
            }
        }

        // =====================================================================
        //  COMBO SPECIAL: FEATHER SCATTER (Phase 1, 60%)
        // =====================================================================

        /// <summary>
        /// Phase 1 combo special: dual-polarity feather burst with
        /// reduced monochrome dust. Prismatic sparkles replaced by shader.
        /// </summary>
        public static void ComboFeatherScatter(Vector2 tipPos)
        {
            if (Main.dedServ) return;

            // Black/white feather burst (kept — lightweight)
            SwanLakeVFXLibrary.SpawnFeatherDuality(tipPos, 3, 0.3f);

            // Reduced monochrome dust burst (4 -> 3)
            for (int i = 0; i < 3; i++)
            {
                float angle = MathHelper.TwoPi * i / 3f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 5f);
                bool isWhite = i % 2 == 0;
                int dustType = isWhite ? DustID.WhiteTorch : DustID.Shadowflame;
                Color col = isWhite ? SwanLakePalette.PureWhite : SwanLakePalette.ObsidianBlack;
                Dust d = Dust.NewDustPerfect(tipPos, dustType, vel, isWhite ? 0 : 100, col, 1.6f);
                d.noGravity = true;
            }

            SwanLakeVFXLibrary.SpawnMusicNotes(tipPos, 2, 18f);
        }

        // =====================================================================
        //  COMBO SPECIAL: FLARE RELEASE (Phase 2, 70%)
        // =====================================================================

        /// <summary>
        /// Phase 2 combo special: prismatic burst when sub-projectiles launch.
        /// Reduced from heavy rainbow explosion + prismatic swirl.
        /// </summary>
        public static void ComboFlareRelease(Vector2 tipPos)
        {
            if (Main.dedServ) return;

            // Central impact (kept — lightweight)
            try { UnifiedVFX.SwanLake.Impact(tipPos, 1.0f); } catch { }

            // Rainbow flare ring (reduced: 6 -> 4 sparkles)
            SwanLakeVFXLibrary.SpawnPrismaticSparkles(tipPos, 4, 15f);

            // Dual-polarity halo rings (kept — lightweight custom particles)
            try { CustomParticles.HaloRing(tipPos, SwanLakePalette.PureWhite, 0.4f, 15); } catch { }
            try { CustomParticles.HaloRing(tipPos, SwanLakePalette.ObsidianBlack, 0.3f, 12); } catch { }

            SwanLakeVFXLibrary.SpawnMusicNotes(tipPos, 3, 25f);
        }

        // =====================================================================
        //  ON HIT VFX
        // =====================================================================

        /// <summary>
        /// On-hit impact VFX: melee impact, combo-scaling music notes,
        /// and reduced crit effects. Heavy rainbow explosion + prismatic
        /// swirl removed.
        /// </summary>
        public static void OnHitVFX(Vector2 hitPos, int comboStep, bool isCrit)
        {
            if (Main.dedServ) return;

            // Core melee impact (kept — lightweight)
            SwanLakeVFXLibrary.MeleeImpact(hitPos, comboStep);

            // Swan Lake themed impact (kept — lightweight)
            try { UnifiedVFX.SwanLake.Impact(hitPos, 1.2f + comboStep * 0.2f); } catch { }

            // Music accidentals (kept — lightweight)
            try { ThemedParticles.SwanLakeAccidentals(hitPos, 2 + comboStep, 20f); } catch { }

            // Crit: reduced from massive rainbow + prismatic swirl to targeted sparkles
            if (isCrit)
            {
                SwanLakeVFXLibrary.SpawnPrismaticSparkles(hitPos, 4, 20f);
                SwanLakeVFXLibrary.SpawnFeatherBurst(hitPos, 3, 0.35f);
            }

            Lighting.AddLight(hitPos, 1.2f, 1.2f, 1.5f);
        }

        // =====================================================================
        //  EMPOWERMENT VFX
        // =====================================================================

        /// <summary>
        /// VFX feedback when empowerment is gained: reduced from heavy
        /// rainbow explosion to targeted prismatic sparkles.
        /// </summary>
        public static void EmpowermentGainedVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            try { UnifiedVFX.SwanLake.Impact(pos, 0.8f); } catch { }

            // Reduced sparkles (6 -> 4)
            SwanLakeVFXLibrary.SpawnPrismaticSparkles(pos, 4, 20f);

            // Keep one halo ring (removed second)
            try { CustomParticles.HaloRing(pos, SwanLakePalette.PureWhite, 0.35f, 14); } catch { }

            SwanLakeVFXLibrary.SpawnMusicNotes(pos, 2, 20f);
        }

        // =====================================================================
        //  PROJECTILE (BLACK SWAN FLARE) VFX
        // =====================================================================

        /// <summary>
        /// Per-frame trail VFX for BlackSwanFlare sub-projectiles.
        /// Heavy monochrome blazing and rainbow shimmer removed —
        /// replaced by SwanFlareTrail shader. Sparse accent particles only.
        /// </summary>
        public static void FlareTrailVFX(Vector2 pos, Vector2 velocity, int flareType)
        {
            if (Main.dedServ) return;

            // Music notes (1-in-10, reduced from 1-in-6)
            if (Main.rand.NextBool(10))
                SwanLakeVFXLibrary.SpawnMusicNotes(pos, 1, 8f, 0.7f, 0.85f, 22);

            // Feather drift (1-in-12, new sparse accent)
            if (Main.rand.NextBool(12))
            {
                Color featherCol = flareType == 0 ? SwanLakePalette.FeatherBlack : SwanLakePalette.FeatherWhite;
                try { CustomParticles.SwanFeatherDrift(pos + Main.rand.NextVector2Circular(5f, 5f), featherCol, 0.12f); } catch { }
            }

            SwanLakeVFXLibrary.AddSwanLight(pos, 0.4f);
        }

        /// <summary>
        /// Draw shader-driven flare trail using projectile oldPos history.
        /// Called from the flare projectile's PreDraw method.
        /// 3-pass rendering: Glow@3x -> Main@1x -> Glow@1.5x
        /// </summary>
        public static void DrawFlareShaderTrail(SpriteBatch sb, Projectile proj, int flareType)
        {
            if (Main.dedServ) return;
            if (!SwanLakeShaderManager.HasSwanFlareTrail)
            {
                DrawFallbackFlareTrail(sb, proj, flareType);
                return;
            }

            Texture2D glowTex = MagnumTextureRegistry.GetSoftGlow();
            if (glowTex == null) return;

            Vector2 origin = glowTex.Size() * 0.5f;
            float time = Main.GlobalTimeWrappedHourly;
            float polarityPhase = flareType == 0 ? 0f : 1f;

            try
            {
                SwanLakeShaderManager.BeginShaderAdditive(sb);

                // PASS 1: Glow underlay @ 3x width
                SwanLakeShaderManager.ApplyBlackSwanFlareTrail(time, polarityPhase, glow: true);
                DrawTrailPositions(sb, proj, glowTex, origin, scaleMult: 0.20f, alphaMult: 0.25f);

                // PASS 2: Main razor tracer @ 1x width
                SwanLakeShaderManager.ApplyBlackSwanFlareTrail(time, polarityPhase, glow: false);
                DrawTrailPositions(sb, proj, glowTex, origin, scaleMult: 0.07f, alphaMult: 0.6f);

                // PASS 3: Glow overbright @ 1.5x width
                SwanLakeShaderManager.ApplyBlackSwanFlareTrail(time, polarityPhase, glow: true);
                DrawTrailPositions(sb, proj, glowTex, origin, scaleMult: 0.10f, alphaMult: 0.2f);

                SwanLakeShaderManager.RestoreSpriteBatch(sb);
            }
            catch
            {
                try { SwanLakeShaderManager.RestoreSpriteBatch(sb); } catch { }
                DrawFallbackFlareTrail(sb, proj, flareType);
            }
        }

        /// <summary>
        /// Fallback flare trail rendering when shader is unavailable.
        /// Simple additive bloom dots with polarity coloring.
        /// </summary>
        private static void DrawFallbackFlareTrail(SpriteBatch sb, Projectile proj, int flareType)
        {
            Texture2D glowTex = MagnumTextureRegistry.GetSoftGlow();
            if (glowTex == null) return;

            Vector2 origin = glowTex.Size() * 0.5f;
            Color primary = flareType == 0 ? SwanLakePalette.ObsidianBlack : SwanLakePalette.PureWhite;
            Color secondary = flareType == 0 ? SwanLakePalette.Silver : SwanLakePalette.DarkSilver;

            for (int k = 0; k < proj.oldPos.Length; k++)
            {
                if (proj.oldPos[k] == Vector2.Zero) continue;
                Vector2 drawPos = proj.oldPos[k] - Main.screenPosition +
                    new Vector2(proj.width / 2f, proj.height / 2f);
                float progress = (proj.oldPos.Length - k) / (float)proj.oldPos.Length;

                Color trailColor = Color.Lerp(secondary, primary, progress);
                sb.Draw(glowTex, drawPos, null,
                    SwanLakePalette.Additive(trailColor, 0.18f * progress),
                    proj.oldRot[k], origin, 0.05f + progress * 0.03f, SpriteEffects.None, 0f);
            }
        }

        /// <summary>
        /// Impact VFX when a BlackSwanFlare sub-projectile hits.
        /// </summary>
        public static void FlareImpactVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            SwanLakeVFXLibrary.ProjectileImpact(pos, 0.7f);
        }
    }
}
