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
    /// VFX helper for Call of the Pearlescent Lake ranged assault rifle.
    /// Handles hold-item ambient, item bloom, muzzle flash,
    /// rocket trail, hit explosion, and death/explosion VFX.
    ///
    /// The Pearlescent Lake: lake's frozen beauty — pearlescent opalescence.
    ///
    /// Shader-driven VFX (primary):
    ///   - PearlescentRocketTrail.fx: Mother-of-pearl shimmer trail (PearlescentTrailMain + PearlescentTrailGlow)
    ///   - LakeExplosion.fx: Concentric water ripple explosion (LakeExplosionMain + LakeExplosionRing)
    ///
    /// Accent particles (reduced): feather drifts, music notes, prismatic sparkles.
    /// </summary>
    public static class CallofthePearlescentLakeVFX
    {
        // =====================================================================
        //  HOLD ITEM VFX
        // =====================================================================

        public static void HoldItemVFX(Player player)
        {
            if (Main.dedServ) return;

            Vector2 center = player.MountedCenter;
            float time = (float)Main.timeForVisualEffects;

            // Swan Lake aura
            try { UnifiedVFX.SwanLake.Aura(center, 28f); } catch { }

            // Directional feather drifts (kept — lightweight)
            if (Main.rand.NextBool(20))
            {
                Vector2 featherOffset = new Vector2(player.direction * Main.rand.NextFloat(10f, 30f), Main.rand.NextFloat(-10f, 10f));
                Color featherCol = Main.rand.NextBool() ? SwanLakePalette.FeatherWhite : SwanLakePalette.FeatherBlack;
                try { CustomParticles.SwanFeatherDrift(center + featherOffset, featherCol, 0.15f); } catch { }
            }

            SwanLakeVFXLibrary.AddPulsingLight(center, time, 0.25f);
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
            SwanLakePalette.DrawItemBloom(sb, tex, pos, origin, rotation, scale, pulse);
        }

        // =====================================================================
        //  MUZZLE VFX (reduced from heavy spark bursts)
        // =====================================================================

        /// <summary>
        /// Muzzle flash: reduced from 16-spark burst + rainbow dust to
        /// compact sparkles + music accents. Shader handles the trail.
        /// </summary>
        public static void MuzzleFlashVFX(Vector2 muzzlePos, Vector2 direction)
        {
            if (Main.dedServ) return;

            try { UnifiedVFX.SwanLake.Impact(muzzlePos, 0.9f); } catch { }

            // Directed spark dust (reduced: 16 -> 6)
            for (int i = 0; i < 6; i++)
            {
                float spreadAngle = Main.rand.NextFloat(-0.35f, 0.35f);
                Vector2 vel = direction.RotatedBy(spreadAngle) * Main.rand.NextFloat(3f, 7f);
                bool isWhite = Main.rand.NextBool();
                int dustType = isWhite ? DustID.WhiteTorch : DustID.Shadowflame;
                Dust d = Dust.NewDustPerfect(muzzlePos, dustType, vel, isWhite ? 0 : 100,
                    isWhite ? SwanLakePalette.PureWhite : SwanLakePalette.ObsidianBlack, 1.5f);
                d.noGravity = true;
            }

            // Prismatic sparkles (reduced: 5 -> 2)
            SwanLakeVFXLibrary.SpawnPrismaticSparkles(muzzlePos, 2, 12f);

            // Feather burst (kept — lightweight)
            SwanLakeVFXLibrary.SpawnFeatherBurst(muzzlePos, 1, 0.2f);

            // Music notes (kept — lightweight)
            SwanLakeVFXLibrary.SpawnMusicNotes(muzzlePos, 2, 12f, 0.7f, 0.9f, 22);

            Lighting.AddLight(muzzlePos, SwanLakePalette.PureWhite.ToVector3() * 0.9f);
        }

        // =====================================================================
        //  ROCKET TRAIL VFX (shader-driven + sparse accents)
        // =====================================================================

        /// <summary>
        /// Per-frame rocket trail: heavy per-frame particles removed.
        /// PearlescentRocketTrail shader handles the primary visual.
        /// Sparse accent particles only.
        /// </summary>
        public static void RocketTrailVFX(Vector2 pos, Vector2 velocity)
        {
            if (Main.dedServ) return;

            // Music notes (1-in-10, reduced from 1-in-6)
            if (Main.rand.NextBool(10))
                SwanLakeVFXLibrary.SpawnMusicNotes(pos, 1, 8f, 0.7f, 0.85f, 22);

            // Feather trail (1-in-10, reduced from 1-in-6)
            if (Main.rand.NextBool(10))
            {
                Color featherCol = Main.rand.NextBool() ? SwanLakePalette.FeatherWhite : SwanLakePalette.FeatherBlack;
                try { CustomParticles.SwanFeatherDrift(pos, featherCol, 0.18f); } catch { }
            }

            SwanLakeVFXLibrary.AddSwanLight(pos, 0.4f);
        }

        // =====================================================================
        //  ROCKET TRAIL SHADER RENDERING
        // =====================================================================

        /// <summary>
        /// Draw shader-driven rocket trail using projectile oldPos history.
        /// Called from the rocket projectile's PreDraw method.
        /// 3-pass rendering: Glow@3x -> Main@1x -> Glow@1.5x
        /// </summary>
        public static void DrawRocketShaderTrail(SpriteBatch sb, Projectile proj)
        {
            if (Main.dedServ) return;
            if (!SwanLakeShaderManager.HasPearlescentRocketTrail)
            {
                DrawFallbackRocketTrail(sb, proj);
                return;
            }

            Texture2D glowTex = MagnumTextureRegistry.GetSoftGlow();
            if (glowTex == null) return;

            Vector2 origin = glowTex.Size() * 0.5f;
            float time = Main.GlobalTimeWrappedHourly;

            try
            {
                SwanLakeShaderManager.BeginShaderAdditive(sb);
                SwanLakeShaderManager.BindNoiseTexture(Main.graphics.GraphicsDevice);

                // PASS 1: Glow underlay @ 3x width
                SwanLakeShaderManager.ApplyPearlescentLakeRocketTrail(time, glow: true);
                DrawTrailPositions(sb, proj, glowTex, origin, scaleMult: 0.25f, alphaMult: 0.3f);

                // PASS 2: Main opal shimmer @ 1x width
                SwanLakeShaderManager.ApplyPearlescentLakeRocketTrail(time, glow: false);
                DrawTrailPositions(sb, proj, glowTex, origin, scaleMult: 0.09f, alphaMult: 0.6f);

                // PASS 3: Glow overbright @ 1.5x width
                SwanLakeShaderManager.ApplyPearlescentLakeRocketTrail(time, glow: true);
                DrawTrailPositions(sb, proj, glowTex, origin, scaleMult: 0.13f, alphaMult: 0.2f);

                SwanLakeShaderManager.RestoreSpriteBatch(sb);
            }
            catch
            {
                try { SwanLakeShaderManager.RestoreSpriteBatch(sb); } catch { }
                DrawFallbackRocketTrail(sb, proj);
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
        /// Fallback rocket trail rendering when shader is unavailable.
        /// Simple additive bloom dots with pearlescent coloring.
        /// </summary>
        private static void DrawFallbackRocketTrail(SpriteBatch sb, Projectile proj)
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

                Color trailColor = Color.Lerp(SwanLakePalette.DarkSilver, SwanLakePalette.PureWhite, progress);
                sb.Draw(glowTex, drawPos, null,
                    SwanLakePalette.Additive(trailColor, 0.18f * progress),
                    proj.oldRot[k], origin, 0.06f + progress * 0.04f, SpriteEffects.None, 0f);
            }
        }

        // =====================================================================
        //  HIT EXPLOSION VFX (shader-driven + reduced particles)
        // =====================================================================

        /// <summary>
        /// Hit explosion: LakeExplosion shader handles the primary visual
        /// (water ripple rings). Reduced accent particles.
        /// </summary>
        public static void HitExplosionVFX(Vector2 pos, float intensity = 1f)
        {
            if (Main.dedServ) return;

            try { UnifiedVFX.SwanLake.Impact(pos, 1.2f * intensity); } catch { }

            // Prismatic sparkles (reduced: 8 -> 3)
            SwanLakeVFXLibrary.SpawnPrismaticSparkles(pos, 3, 20f);

            // Gradient halo rings (reduced: 4 -> 2)
            SwanLakeVFXLibrary.SpawnGradientHaloRings(pos, 2, 0.25f * intensity);

            // Feather burst (reduced: 4 -> 1)
            SwanLakeVFXLibrary.SpawnFeatherBurst(pos, 1, 0.3f);

            // Music notes (kept)
            SwanLakeVFXLibrary.SpawnMusicNotes(pos, 2, 20f, 0.75f, 1.0f, 25);

            Lighting.AddLight(pos, SwanLakePalette.PureWhite.ToVector3() * 1.0f * intensity);
        }

        // =====================================================================
        //  EXPLOSION SHADER RENDERING
        // =====================================================================

        /// <summary>
        /// Draw shader-driven lake explosion using LakeExplosion.fx.
        /// Called from hit/death explosion rendering.
        /// 2-pass: Main body + Ring overlay.
        /// </summary>
        public static void DrawExplosionShader(SpriteBatch sb, Vector2 pos, float explosionAge, float scale = 1f)
        {
            if (Main.dedServ) return;
            if (!SwanLakeShaderManager.HasLakeExplosion) return;

            Texture2D glowTex = MagnumTextureRegistry.GetSoftGlow();
            if (glowTex == null) return;

            Vector2 origin = glowTex.Size() * 0.5f;
            Vector2 drawPos = pos - Main.screenPosition;
            float time = Main.GlobalTimeWrappedHourly;

            try
            {
                SwanLakeShaderManager.BeginShaderAdditive(sb);
                SwanLakeShaderManager.BindNoiseTexture(Main.graphics.GraphicsDevice);

                // PASS 1: Main explosion body
                SwanLakeShaderManager.ApplyPearlescentLakeExplosion(time, explosionAge, ringOnly: false);
                float bodyScale = 1.5f * scale;
                float bodyAlpha = 1f - explosionAge * explosionAge;
                sb.Draw(glowTex, drawPos, null, Color.White * bodyAlpha,
                    0f, origin, bodyScale, SpriteEffects.None, 0f);

                // PASS 2: Ring shockwave overlay
                SwanLakeShaderManager.ApplyPearlescentLakeExplosion(time, explosionAge, ringOnly: true);
                float ringScale = 2.2f * scale * (0.5f + explosionAge * 0.5f);
                float ringAlpha = 0.7f * (1f - explosionAge);
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
        //  DEATH / MAJOR EXPLOSION VFX (shader-driven + reduced particles)
        // =====================================================================

        /// <summary>
        /// Rocket death explosion: LakeExplosion shader provides the core visual.
        /// Heavy particle bursts replaced. Modest accent particles kept.
        /// </summary>
        public static void DeathExplosionVFX(Vector2 pos, float intensity = 1f)
        {
            if (Main.dedServ) return;

            try { ThemedParticles.SwanLakeMusicalImpact(pos, intensity); } catch { }

            // Bloom (kept — lightweight)
            SwanLakeVFXLibrary.DrawBloom(pos, 0.7f * intensity);

            // Halo rings (reduced: 5+4 -> 2)
            SwanLakeVFXLibrary.SpawnGradientHaloRings(pos, 2, 0.3f * intensity);

            // Prismatic sparkles (reduced: 10 -> 4)
            SwanLakeVFXLibrary.SpawnPrismaticSparkles(pos, 4, 25f * intensity);

            // Feathers (reduced: 6 -> 2)
            SwanLakeVFXLibrary.SpawnFeatherBurst(pos, 2, 0.35f);

            // Music notes (reduced: 5 -> 3)
            SwanLakeVFXLibrary.SpawnMusicNotes(pos, 3, 30f, 0.8f, 1.1f, 30);

            // Light burst
            Lighting.AddLight(pos, SwanLakePalette.RainbowFlash.ToVector3() * 1.4f * intensity);
        }
    }
}
