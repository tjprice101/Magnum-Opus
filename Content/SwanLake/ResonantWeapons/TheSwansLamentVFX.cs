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
    /// VFX helper for The Swan's Lament ranger rifle.
    /// Handles hold-item ambient, item bloom, muzzle flash,
    /// bullet trail, destruction halo, and death VFX.
    ///
    /// The Swan's Lament: sorrowful monochrome shots that erupt in color on impact.
    ///
    /// Shader-driven VFX (primary):
    ///   - LamentBulletTrail.fx: Muted razor-narrow monochrome trail (LamentTrailMain + LamentTrailGlow)
    ///   - DestructionRevelation.fx: Monochrome-shatters-to-prismatic radial (RevelationBlastMain + RevelationBlastRing)
    ///
    /// Accent particles (reduced): feather drifts, music notes, prismatic sparkles.
    /// </summary>
    public static class TheSwansLamentVFX
    {
        // =====================================================================
        //  HOLD ITEM VFX
        // =====================================================================

        /// <summary>
        /// Per-frame held-item VFX: ambient aura, feather drifts, and warm light.
        /// </summary>
        public static void HoldItemVFX(Player player)
        {
            if (Main.dedServ) return;

            Vector2 center = player.MountedCenter;
            float time = (float)Main.timeForVisualEffects;

            // Swan Lake aura
            try { UnifiedVFX.SwanLake.Aura(center, 28f); } catch { }

            // Feather drift (reduced from 1-in-22 to 1-in-24)
            if (Main.rand.NextBool(24))
            {
                Color featherCol = Main.rand.NextBool() ? SwanLakePalette.FeatherWhite : SwanLakePalette.FeatherBlack;
                try { CustomParticles.SwanFeatherDrift(center + Main.rand.NextVector2Circular(20f, 20f), featherCol, 0.15f); } catch { }
            }

            SwanLakeVFXLibrary.AddPulsingLight(center, time, 0.25f);
        }

        // =====================================================================
        //  PREDRAW IN WORLD BLOOM
        // =====================================================================

        /// <summary>
        /// Standard 3-layer Swan Lake item bloom for the rifle sprite.
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
        //  MUZZLE VFX
        // =====================================================================

        /// <summary>
        /// Muzzle flash: directed spark burst, halo ring, music notes, feather burst.
        /// Reduced from original heavy particle spawning.
        /// </summary>
        public static void MuzzleFlashVFX(Vector2 muzzlePos, Vector2 direction)
        {
            if (Main.dedServ) return;

            // Core impact
            try { UnifiedVFX.SwanLake.Impact(muzzlePos, 1.0f); } catch { }

            // Directed spark burst (reduced from 16 to 8)
            for (int i = 0; i < 8; i++)
            {
                float spreadAngle = Main.rand.NextFloat(-0.4f, 0.4f);
                Vector2 vel = direction.RotatedBy(spreadAngle) * Main.rand.NextFloat(3f, 8f);
                bool isWhite = Main.rand.NextBool();
                int dustType = isWhite ? DustID.WhiteTorch : DustID.Shadowflame;
                Dust d = Dust.NewDustPerfect(muzzlePos, dustType, vel, isWhite ? 0 : 100,
                    isWhite ? SwanLakePalette.PureWhite : SwanLakePalette.ObsidianBlack, 1.6f);
                d.noGravity = true;
                d.fadeIn = 1.2f;
            }

            // Gradient halo rings (reduced from 4 to 2)
            SwanLakeVFXLibrary.SpawnGradientHaloRings(muzzlePos, 2, 0.25f);

            // Music notes
            SwanLakeVFXLibrary.SpawnMusicNotes(muzzlePos, 2, 15f, 0.75f, 1.0f, 25);

            // Feather burst (reduced from 3 to 2)
            SwanLakeVFXLibrary.SpawnFeatherBurst(muzzlePos, 2, 0.25f);

            Lighting.AddLight(muzzlePos, SwanLakePalette.PureWhite.ToVector3() * 1.0f);
        }

        // =====================================================================
        //  BULLET TRAIL VFX (shader-driven)
        // =====================================================================

        /// <summary>
        /// Per-frame bullet trail: shader trail is primary visual.
        /// Sparse accent particles only (music notes, feather drift).
        /// </summary>
        public static void BulletTrailVFX(Vector2 pos, Vector2 velocity)
        {
            if (Main.dedServ) return;

            // Music notes (1-in-10, reduced from 1-in-6)
            if (Main.rand.NextBool(10))
                SwanLakeVFXLibrary.SpawnMusicNotes(pos, 1, 8f, 0.7f, 0.85f, 22);

            // Feather drift (1-in-10, new sparse accent)
            if (Main.rand.NextBool(10))
            {
                Color featherCol = Main.rand.NextBool() ? SwanLakePalette.FeatherWhite : SwanLakePalette.FeatherBlack;
                try { CustomParticles.SwanFeatherDrift(pos + Main.rand.NextVector2Circular(5f, 5f), featherCol, 0.1f); } catch { }
            }

            SwanLakeVFXLibrary.AddSwanLight(pos, 0.3f);
        }

        /// <summary>
        /// Draw shader-driven bullet trail using projectile oldPos history.
        /// Called from the projectile's PreDraw method.
        /// 3-pass rendering: Glow@3x -> Main@1x -> Glow@1.5x
        /// </summary>
        public static void DrawBulletShaderTrail(SpriteBatch sb, Projectile proj)
        {
            if (Main.dedServ) return;
            if (!SwanLakeShaderManager.HasLamentBulletTrail)
            {
                // Fallback: simple additive bloom trail
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
                SwanLakeShaderManager.BindNoiseTexture(Main.graphics.GraphicsDevice);

                // PASS 1: Glow underlay @ 3x width (barely visible sorrowful bloom)
                SwanLakeShaderManager.ApplySwansLamentBulletTrail(time, glow: true);
                DrawTrailPositions(sb, proj, glowTex, origin, scaleMult: 0.25f, alphaMult: 0.3f);

                // PASS 2: Main sharp trail @ 1x width (muted razor core)
                SwanLakeShaderManager.ApplySwansLamentBulletTrail(time, glow: false);
                DrawTrailPositions(sb, proj, glowTex, origin, scaleMult: 0.08f, alphaMult: 0.6f);

                // PASS 3: Glow overbright @ 1.5x width (subtle whisper halo)
                SwanLakeShaderManager.ApplySwansLamentBulletTrail(time, glow: true);
                DrawTrailPositions(sb, proj, glowTex, origin, scaleMult: 0.12f, alphaMult: 0.2f);

                SwanLakeShaderManager.RestoreSpriteBatch(sb);
            }
            catch
            {
                // Restore SpriteBatch if shader fails
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
        /// Simple additive bloom dots along oldPos.
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

                Color trailColor = Color.Lerp(SwanLakePalette.SwanDarkGray, SwanLakePalette.FeatherWhite, progress);
                sb.Draw(glowTex, drawPos, null,
                    SwanLakePalette.Additive(trailColor, 0.15f * progress),
                    proj.oldRot[k], origin, 0.06f + progress * 0.04f, SpriteEffects.None, 0f);
            }
        }

        // =====================================================================
        //  DESTRUCTION HALO VFX (shader-driven)
        // =====================================================================

        /// <summary>
        /// Destruction halo on major impact: shader-driven prismatic revelation
        /// with reduced accent particles.
        /// </summary>
        public static void DestructionHaloVFX(Vector2 pos, float intensity = 1f)
        {
            if (Main.dedServ) return;

            // Core impact (kept — lightweight)
            try { UnifiedVFX.SwanLake.Impact(pos, 1.5f * intensity); } catch { }

            // Bloom (kept — lightweight)
            SwanLakeVFXLibrary.DrawBloom(pos, 0.7f * intensity);

            // Stacked halo rings (reduced: 6->2 gradient, 4->0 rainbow)
            SwanLakeVFXLibrary.SpawnGradientHaloRings(pos, 2, 0.3f * intensity);

            // Prismatic sparkles (reduced: 10->4)
            SwanLakeVFXLibrary.SpawnPrismaticSparkles(pos, 4, 25f * intensity);

            // Feather burst (reduced: 6->3)
            SwanLakeVFXLibrary.SpawnFeatherBurst(pos, (int)(3 * intensity), 0.35f);

            // Music notes (reduced: 5->2)
            SwanLakeVFXLibrary.SpawnMusicNotes(pos, 2, 30f, 0.8f, 1.1f, 30);

            Lighting.AddLight(pos, SwanLakePalette.RainbowFlash.ToVector3() * 1.3f * intensity);
        }

        /// <summary>
        /// Draw shader-driven destruction revelation overlay.
        /// Called from the projectile's PreDraw or on-hit handler.
        /// 2-pass rendering: Main body + Ring shockwave overlay.
        /// </summary>
        /// <param name="sb">Active SpriteBatch.</param>
        /// <param name="worldPos">World-space explosion center.</param>
        /// <param name="explosionAge">0 = just detonated, 1 = fully expanded/faded.</param>
        /// <param name="scale">Base quad scale.</param>
        public static void DrawDestructionRevelationShader(SpriteBatch sb, Vector2 worldPos,
            float explosionAge, float scale = 0.5f)
        {
            if (!SwanLakeShaderManager.HasDestructionRevelation) return;

            Texture2D glowTex = MagnumTextureRegistry.GetSoftGlow();
            if (glowTex == null) return;

            Vector2 drawPos = worldPos - Main.screenPosition;
            Vector2 origin = glowTex.Size() * 0.5f;
            float expandScale = scale * (1f + explosionAge * 2f);
            float fadeAlpha = MathHelper.Clamp(1f - explosionAge * 0.6f, 0.1f, 1f);
            float time = Main.GlobalTimeWrappedHourly;

            try
            {
                SwanLakeShaderManager.BeginShaderAdditive(sb);
                SwanLakeShaderManager.BindCrackNoiseTexture(Main.graphics.GraphicsDevice);

                // PASS 1: Main body (monochrome-to-rainbow revelation) @ 1.5x expand
                SwanLakeShaderManager.ApplySwansLamentDestructionHalo(time, explosionAge, ringOnly: false);

                sb.Draw(glowTex, drawPos, null,
                    Color.White * fadeAlpha, 0f, origin,
                    expandScale * 1.5f, SpriteEffects.None, 0f);

                // PASS 2: Expanding prismatic ring shockwave @ 2.2x expand
                SwanLakeShaderManager.ApplySwansLamentDestructionHalo(time, explosionAge, ringOnly: true);

                sb.Draw(glowTex, drawPos, null,
                    Color.White * fadeAlpha * 0.6f, 0f, origin,
                    expandScale * 2.2f, SpriteEffects.None, 0f);

                SwanLakeShaderManager.RestoreSpriteBatch(sb);
            }
            catch
            {
                try { SwanLakeShaderManager.RestoreSpriteBatch(sb); } catch { }
            }
        }

        // =====================================================================
        //  BULLET DEATH VFX
        // =====================================================================

        /// <summary>
        /// Bullet death VFX: small sparkle burst with halo ring.
        /// Reduced from heavy dual-polarity + rainbow burst.
        /// </summary>
        public static void BulletDeathVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            SwanLakeVFXLibrary.SpawnPrismaticSparkles(pos, 2, 8f);
            try { CustomParticles.HaloRing(pos, SwanLakePalette.SwanSilver, 0.15f, 8); } catch { }
        }
    }
}
