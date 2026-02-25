using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Content.LaCampanella.ResonantWeapons
{
    /// <summary>
    /// VFX helper for the Infernal Bell Minion summoned projectile.
    /// Handles ambient fire spirit aura, attack VFX, bell swing animation VFX,
    /// shockwave every 5 hits, and smoke trail.
    /// </summary>
    public static class InfernalBellMinionVFX
    {
        // =====================================================================
        //  AMBIENT AURA
        // =====================================================================

        /// <summary>
        /// Per-frame ambient VFX for the fire spirit minion.
        /// Floating fire motes, ember particles, smoke trail, bell shimmer.
        /// </summary>
        public static void AmbientAuraVFX(Vector2 center, float intensity = 1f)
        {
            if (Main.dedServ) return;

            float time = (float)Main.timeForVisualEffects;

            // Floating fire motes orbiting center
            if (Main.rand.NextBool(3))
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                float radius = 15f + Main.rand.NextFloat(10f);
                Vector2 motePos = center + angle.ToRotationVector2() * radius;
                Vector2 vel = new Vector2(0, -0.5f) + Main.rand.NextVector2Circular(0.3f, 0.3f);
                Color col = LaCampanellaPalette.PaletteLerp(LaCampanellaPalette.InfernalMinionAura,
                    Main.rand.NextFloat(0.3f, 0.8f));
                Dust d = Dust.NewDustPerfect(motePos, DustID.Torch, vel, 0, col, 1.0f * intensity);
                d.noGravity = true;
                d.fadeIn = 0.6f;
            }

            // Ember particles rising
            if (Main.rand.NextBool(5))
            {
                Vector2 pos = center + Main.rand.NextVector2Circular(12f, 12f);
                Vector2 vel = new Vector2(Main.rand.NextFloat(-0.3f, 0.3f), -1f - Main.rand.NextFloat(0.5f));
                Color col = Color.Lerp(LaCampanellaPalette.EmberRed, LaCampanellaPalette.FlameYellow, Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(pos, DustID.Torch, vel, 0, col, 0.6f * intensity);
                d.noGravity = true;
            }

            // Smoke trail behind minion
            LaCampanellaVFXLibrary.SpawnAmbientSmoke(center, 15f);

            // Bell shimmer sparkle
            if (Main.rand.NextBool(8))
            {
                Color shimmer = LaCampanellaPalette.GetBellShimmer(time);
                Dust d = Dust.NewDustPerfect(center + Main.rand.NextVector2Circular(10f, 10f),
                    DustID.Enchanted_Gold, Vector2.Zero, 0, shimmer, 0.5f);
                d.noGravity = true;
            }

            // Sparse music notes
            if (Main.rand.NextBool(20))
                LaCampanellaVFXLibrary.SpawnMusicNotes(center, 1, 15f, 0.7f, 0.85f, 25);

            // Ambient fire glow
            float pulse = 0.4f + MathF.Sin(time * 0.06f) * 0.15f;
            Lighting.AddLight(center, LaCampanellaPalette.InfernalOrange.ToVector3() * pulse * intensity);
        }

        // =====================================================================
        //  BELL SWING ANIMATION VFX
        // =====================================================================

        /// <summary>
        /// VFX during bell swing attack animation.
        /// Arc of fire particles following the bell's swing path.
        /// </summary>
        public static void BellSwingVFX(Vector2 center, float swingAngle, float swingProgress)
        {
            if (Main.dedServ) return;

            // Fire arc along swing path
            Vector2 swingTip = center + swingAngle.ToRotationVector2() * 25f;
            for (int i = 0; i < 2; i++)
            {
                Vector2 vel = swingAngle.ToRotationVector2() * 2f + Main.rand.NextVector2Circular(1f, 1f);
                Color col = LaCampanellaPalette.PaletteLerp(LaCampanellaPalette.InfernalMinionAura, swingProgress);
                Dust d = Dust.NewDustPerfect(swingTip, DustID.Torch, vel, 0, col, 1.3f);
                d.noGravity = true;
            }

            // Chime dust at peak
            if (swingProgress > 0.8f)
            {
                Color chimeCol = LaCampanellaPalette.ChimeShimmer;
                Dust d = Dust.NewDustPerfect(swingTip, DustID.Enchanted_Gold,
                    Main.rand.NextVector2Circular(2f, 2f), 0, chimeCol, 1.0f);
                d.noGravity = true;
            }

            Lighting.AddLight(swingTip, LaCampanellaPalette.FlameYellow.ToVector3() * 0.6f);
        }

        // =====================================================================
        //  ATTACK HIT VFX
        // =====================================================================

        /// <summary>
        /// Minion attack on-hit VFX.
        /// </summary>
        public static void AttackHitVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            LaCampanellaVFXLibrary.MeleeImpact(pos, 0);
            LaCampanellaVFXLibrary.SpawnEmberScatter(pos, 4, 2.5f);
        }

        // =====================================================================
        //  SHOCKWAVE (every 5 hits)
        // =====================================================================

        /// <summary>
        /// Shockwave VFX triggered every 5th hit.
        /// Expanding fire ring + bell chime + heavy smoke.
        /// </summary>
        public static void ShockwaveVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            LaCampanellaVFXLibrary.BellShockwaveImpact(pos, 1f);
            MagnumScreenEffects.AddScreenShake(4f);
        }

        // =====================================================================
        //  INNER GLOW PULSE (during ringing state)
        // =====================================================================

        /// <summary>
        /// Enhanced inner glow VFX during ringing animation state.
        /// Intensified fire particles and golden bell shimmer.
        /// </summary>
        public static void RingingGlowVFX(Vector2 center, float glowIntensity)
        {
            if (Main.dedServ) return;

            // Intensified fire particles
            for (int i = 0; i < 3; i++)
            {
                Vector2 pos = center + Main.rand.NextVector2Circular(8f, 8f);
                Vector2 vel = Main.rand.NextVector2Circular(1.5f, 1.5f) + new Vector2(0, -1f);
                Color col = Color.Lerp(LaCampanellaPalette.FlameYellow, LaCampanellaPalette.WhiteHot,
                    glowIntensity);
                Dust d = Dust.NewDustPerfect(pos, DustID.Torch, vel, 0, col, 1.2f * glowIntensity);
                d.noGravity = true;
            }

            // Golden bell shimmer ring
            if (Main.rand.NextBool(3))
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 ringPos = center + angle.ToRotationVector2() * 10f;
                Dust d = Dust.NewDustPerfect(ringPos, DustID.GoldFlame, Vector2.Zero, 0,
                    LaCampanellaPalette.BellGold, 0.8f * glowIntensity);
                d.noGravity = true;
            }

            Lighting.AddLight(center, LaCampanellaPalette.BellGold.ToVector3() * glowIntensity);
        }

        // =====================================================================
        //  MINION DEATH / DESPAWN
        // =====================================================================

        /// <summary>
        /// Minion despawn VFX — fire spirit dissipation.
        /// </summary>
        public static void DespawnVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            // Fire burst
            LaCampanellaVFXLibrary.SpawnRadialDustBurst(pos, 12, 4f);
            LaCampanellaVFXLibrary.SpawnEmberScatter(pos, 6, 3f);
            LaCampanellaVFXLibrary.SpawnHeavySmoke(pos, 5, 0.8f, 2f, 50);
            LaCampanellaVFXLibrary.SpawnMusicNotes(pos, 3, 20f);
        }
    }
}
