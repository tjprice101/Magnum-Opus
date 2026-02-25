using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Content.EnigmaVariations.ResonantWeapons
{
    /// <summary>
    /// VFX helper for The Unresolved Cadence melee weapon.
    /// Handles hold-item aura, item bloom, DimensionalSlash trail/line/impact,
    /// ParadoxCollapseUltimate 3-phase explosion (buildup, explosion, fade),
    /// and collapse on-hit VFX.
    /// Call from TheUnresolvedCadenceItem, DimensionalSlash, and ParadoxCollapseUltimate.
    /// </summary>
    public static class TheUnresolvedCadenceVFX
    {
        // =====================================================================
        //  HOLD ITEM VFX
        // =====================================================================

        /// <summary>
        /// Per-frame held-item VFX: subtle dimensional rift aura near the player,
        /// watching eyes, and unstable void shimmer.
        /// </summary>
        public static void HoldItemVFX(Player player)
        {
            if (Main.dedServ) return;

            Vector2 center = player.MountedCenter;
            float time = (float)Main.timeForVisualEffects;

            // Dimensional rift aura — unstable void motes
            if (Main.rand.NextBool(8))
            {
                Vector2 riftPos = center + Main.rand.NextVector2Circular(35f, 35f);
                Color riftColor = EnigmaPalette.GetEnigmaGradient(Main.rand.NextFloat()) * 0.5f;
                var rift = new GenericGlowParticle(riftPos, Main.rand.NextVector2Circular(1f, 1f),
                    riftColor, 0.2f, 15, true);
                MagnumParticleHandler.SpawnParticle(rift);
            }

            // Watching eye — the cadence observes
            if (Main.rand.NextBool(25))
            {
                Vector2 eyePos = center + Main.rand.NextVector2Circular(40f, 40f);
                CustomParticles.EnigmaEyeGaze(eyePos, EnigmaPalette.EyeGreen * 0.35f, 0.25f);
            }

            // Occasional music note
            if (Main.rand.NextBool(12))
                EnigmaVFXLibrary.SpawnMusicNotes(center, 1, 18f, 0.65f, 0.85f, 25);

            // Unresolved tension glow
            float pulse = 0.3f + MathF.Sin(time * 0.05f) * 0.1f;
            Lighting.AddLight(center, EnigmaPalette.UnresolvedTension.ToVector3() * pulse);
        }

        // =====================================================================
        //  PREDRAW IN WORLD BLOOM
        // =====================================================================

        /// <summary>
        /// Standard 3-layer PreDrawInWorld bloom for the weapon sprite.
        /// Enhanced bloom with color shift for this ultimate-tier item.
        /// </summary>
        public static void PreDrawInWorldBloom(
            Microsoft.Xna.Framework.Graphics.SpriteBatch sb,
            Microsoft.Xna.Framework.Graphics.Texture2D tex,
            Vector2 pos, Vector2 origin, float rotation, float scale)
        {
            float time = (float)Main.timeForVisualEffects;
            float pulse = 1f + MathF.Sin(time * 0.04f) * 0.12f;
            EnigmaPalette.DrawItemBloomEnhanced(sb, tex, pos, origin, rotation, scale, pulse, time * 0.025f);
        }

        // =====================================================================
        //  DIMENSIONAL SLASH TRAIL VFX
        // =====================================================================

        /// <summary>
        /// Per-frame trail particles for a DimensionalSlash projectile.
        /// Dense void dust, contrast sparkles, shimmer, and periodic music notes.
        /// </summary>
        public static void DimensionalSlashTrailVFX(Vector2 pos, Vector2 velocity, float intensity)
        {
            if (Main.dedServ) return;

            // Dense dual-color dust (2 per frame)
            for (int i = 0; i < 2; i++)
            {
                Vector2 offset = Main.rand.NextVector2Circular(25f, 25f);
                Color col = EnigmaPalette.GetEnigmaGradient(Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(pos + offset, DustID.PurpleTorch,
                    Main.rand.NextVector2Circular(3f, 3f), 0, col, (1.2f + Main.rand.NextFloat(0.4f)) * intensity);
                d.noGravity = true;
                d.fadeIn = 1.4f;
            }

            // Green flame contrast dust
            EnigmaVFXLibrary.SpawnContrastSparkle(pos, -velocity.SafeNormalize(Vector2.Zero));

            // Contrasting sparkle particles (1-in-2)
            if (Main.rand.NextBool(2) && intensity > 0.2f)
            {
                Color sparkleColor = EnigmaPalette.GetEnigmaGradient(Main.rand.NextFloat());
                var sparkle = new SparkleParticle(pos + Main.rand.NextVector2Circular(30f, 30f),
                    Main.rand.NextVector2Circular(2f, 2f),
                    sparkleColor * intensity, 0.45f, 20);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }

            // Enigma shimmer (1-in-3)
            if (Main.rand.NextBool(3) && intensity > 0.3f)
            {
                Color shimmer = EnigmaPalette.GetShimmer((float)Main.timeForVisualEffects);
                var glow = new GenericGlowParticle(pos + Main.rand.NextVector2Circular(20f, 20f),
                    Main.rand.NextVector2Circular(2.5f, 2.5f),
                    shimmer * intensity * 0.9f, 0.42f, 18, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }

            // Frequent flares (1-in-2)
            if (Main.rand.NextBool(2) && intensity > 0.2f)
            {
                Color flareColor = EnigmaPalette.GetEnigmaGradient(Main.rand.NextFloat());
                CustomParticles.GenericFlare(pos + Main.rand.NextVector2Circular(25f, 25f),
                    flareColor * intensity, 0.45f, 16);
            }

            // Music notes (1-in-6)
            if (Main.rand.NextBool(6) && intensity > 0.3f)
                EnigmaVFXLibrary.SpawnMusicNotes(pos, 1, 10f, 0.85f, 1.0f, 32);

            // Pulsing dimensional light
            EnigmaVFXLibrary.AddPulsingLight(pos, intensity * 0.5f);
        }

        // =====================================================================
        //  DIMENSIONAL SLASH LINE VFX
        // =====================================================================

        /// <summary>
        /// Jagged dimensional tear line particles along the slash direction.
        /// Creates a zigzag pattern of flares and glyphs perpendicular to the slash.
        /// </summary>
        public static void DimensionalSlashLineVFX(Vector2 center, float slashAngle, float intensity)
        {
            if (Main.dedServ) return;

            Vector2 slashDir = slashAngle.ToRotationVector2();
            Vector2 perpendicular = slashDir.RotatedBy(MathHelper.PiOver2);

            // Jagged slash line segments
            int segments = 5;
            for (int i = 0; i < segments; i++)
            {
                float t = ((float)i / segments - 0.5f) * 2f;
                float jaggedOffset = MathF.Sin((float)Main.timeForVisualEffects * 0.3f + i * 2f) * 10f * intensity;

                Vector2 slashPos = center + slashDir * t * 50f + perpendicular * jaggedOffset;
                Color slashColor = EnigmaPalette.PaletteLerp(EnigmaPalette.UnresolvedCadence,
                    ((float)i / segments + intensity * 0.5f) % 1f) * intensity;

                CustomParticles.GenericFlare(slashPos, slashColor, 0.5f + intensity * 0.3f, 12);
            }

            // Glyph along the tear
            if (Main.rand.NextBool(3))
            {
                Vector2 glyphPos = center + slashDir * Main.rand.NextFloat(-40f, 40f);
                CustomParticles.Glyph(glyphPos, EnigmaPalette.GreenFlame * intensity, 0.3f, -1);
            }

            // Watching eye at center of the slash
            if (Main.rand.NextBool(6) && intensity > 0.4f)
            {
                Vector2 eyePos = center + Main.rand.NextVector2Circular(15f, 15f);
                CustomParticles.EnigmaEyeGaze(eyePos, EnigmaPalette.EyeGreen * intensity * 0.6f, 0.3f);
            }
        }

        // =====================================================================
        //  DIMENSIONAL SLASH IMPACT VFX
        // =====================================================================

        /// <summary>
        /// On-hit VFX for DimensionalSlash: dimensional slice reality warp,
        /// watching eye, glyph circle, and sparkle burst.
        /// </summary>
        public static void DimensionalSlashImpactVFX(Vector2 pos, Vector2 targetCenter)
        {
            if (Main.dedServ) return;

            // Central flash
            CustomParticles.GenericFlare(pos, EnigmaPalette.GreenFlame, 0.7f, 15);
            CustomParticles.HaloRing(pos, EnigmaPalette.Purple, 0.4f, 12);

            // Watching eye at impact
            CustomParticles.EnigmaEyeImpact(pos, targetCenter, EnigmaPalette.GreenFlame, 0.5f);

            // Dazzling sparkle burst from dimensional slice
            for (int i = 0; i < 5; i++)
            {
                float angle = MathHelper.TwoPi * i / 5f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 4f);
                Color col = EnigmaPalette.PaletteLerp(EnigmaPalette.UnresolvedCadence, (float)i / 5f);
                var glow = new GenericGlowParticle(pos - new Vector2(0, 30f), vel, col, 0.35f, 18, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }

            // Glyph circle formation
            CustomParticles.GlyphCircle(pos, EnigmaPalette.Purple, count: 6, radius: 45f, rotationSpeed: 0.06f);

            // Music notes burst
            EnigmaVFXLibrary.SpawnMusicNotes(pos, 4, 18f, 0.8f, 1.0f, 28);

            // Radial dust burst
            EnigmaVFXLibrary.SpawnRadialDustBurst(pos, 10, 5f);

            Lighting.AddLight(pos, EnigmaPalette.GreenFlame.ToVector3() * 0.8f);
        }

        // =====================================================================
        //  COLLAPSE BUILDUP VFX — Phase 1: implosion
        // =====================================================================

        /// <summary>
        /// ParadoxCollapseUltimate Phase 1: particles spiraling inward,
        /// swirling arcane sparkles, building tension toward detonation.
        /// </summary>
        public static void CollapseBuildupVFX(Vector2 center, float buildupProgress, float currentRadius)
        {
            if (Main.dedServ) return;

            float intensity = buildupProgress;

            // Particles pulling inward (8 per burst, every 4 frames)
            if (Main.GameUpdateCount % 4 == 0)
            {
                for (int i = 0; i < 8; i++)
                {
                    float angle = Main.GameUpdateCount * 0.1f + MathHelper.TwoPi * i / 8f;
                    Vector2 particlePos = center + angle.ToRotationVector2() * currentRadius;
                    Vector2 vel = (center - particlePos).SafeNormalize(Vector2.Zero) * 10f * buildupProgress;

                    Color col = EnigmaPalette.PaletteLerp(EnigmaPalette.UnresolvedCadence, (float)i / 8f) * intensity;
                    var glow = new GenericGlowParticle(particlePos, vel, col, 0.5f, 15, true);
                    MagnumParticleHandler.SpawnParticle(glow);
                }
            }

            // Swirling arcane sparkles (every 8 frames)
            if (Main.GameUpdateCount % 8 == 0)
            {
                float sparkAngle = Main.rand.NextFloat() * MathHelper.TwoPi;
                Vector2 sparkPos = center + sparkAngle.ToRotationVector2() * currentRadius * 0.7f;
                Vector2 sparkVel = -sparkAngle.ToRotationVector2() * 3f;
                Color sparkColor = EnigmaPalette.GetEnigmaGradient(Main.rand.NextFloat()) * intensity;

                CustomParticles.GenericFlare(sparkPos, sparkColor, 0.55f, 15);
                var sparkGlow = new GenericGlowParticle(sparkPos, sparkVel, sparkColor * 0.8f, 0.4f, 18, true);
                MagnumParticleHandler.SpawnParticle(sparkGlow);
            }

            // Watching eyes — reality peers in during buildup (every 12 frames)
            if (Main.GameUpdateCount % 12 == 0 && buildupProgress > 0.3f)
            {
                float eyeAngle = Main.rand.NextFloat() * MathHelper.TwoPi;
                Vector2 eyePos = center + eyeAngle.ToRotationVector2() * currentRadius * 0.5f;
                CustomParticles.EnigmaEyeGaze(eyePos, EnigmaPalette.EyeGreen * intensity * 0.6f, 0.3f, center);
            }

            // Void swirl inward
            if (Main.GameUpdateCount % 6 == 0)
                EnigmaVFXLibrary.SpawnVoidSwirl(center, 3, currentRadius * 0.8f, 0.4f * intensity);

            // Music notes — tension mounting
            if (Main.rand.NextBool(8))
                EnigmaVFXLibrary.SpawnMusicNotes(center, 1, currentRadius * 0.4f, 0.7f, 0.9f, 30);

            // Central void pulse
            if (Main.GameUpdateCount % 4 == 0)
                CustomParticles.GenericFlare(center, EnigmaPalette.VoidBlack, 1f * intensity, 15);

            Lighting.AddLight(center, EnigmaPalette.GetEnigmaGradient(0.5f).ToVector3() * intensity);
        }

        // =====================================================================
        //  COLLAPSE EXPLOSION VFX — Phase 2: expanding ring of destruction
        // =====================================================================

        /// <summary>
        /// ParadoxCollapseUltimate Phase 2: massive expanding ring, radial beams,
        /// glyph circles, prismatic sparkle bursts, and halo ring cascade.
        /// </summary>
        public static void CollapseExplosionVFX(Vector2 center, float explosionProgress, float currentRadius)
        {
            if (Main.dedServ) return;

            // Massive expanding ring of destruction (12 points, every 4 frames)
            if (Main.GameUpdateCount % 4 == 0)
            {
                int points = 12;
                for (int i = 0; i < points; i++)
                {
                    float angle = MathHelper.TwoPi * i / points;
                    Vector2 particlePos = center + angle.ToRotationVector2() * currentRadius;

                    Color col = EnigmaPalette.PaletteLerp(EnigmaPalette.UnresolvedCadence, (float)i / points);
                    CustomParticles.GenericFlare(particlePos, col, 0.7f, 12);

                    // Inner ring — every 3rd point
                    if (i % 3 == 0)
                    {
                        Vector2 innerPos = center + angle.ToRotationVector2() * currentRadius * 0.6f;
                        CustomParticles.GenericFlare(innerPos, EnigmaPalette.Purple, 0.5f, 10);
                    }
                }

                // Radial beams (6 beams, 6 segments each, every 8 frames)
                if (Main.GameUpdateCount % 8 == 0)
                {
                    for (int beam = 0; beam < 6; beam++)
                    {
                        float beamAngle = MathHelper.TwoPi * beam / 6f + Main.GameUpdateCount * 0.02f;
                        for (int s = 0; s < 6; s++)
                        {
                            float t = (float)s / 6f * explosionProgress;
                            Vector2 beamPos = center + beamAngle.ToRotationVector2() * (600f * t);
                            Color beamColor = EnigmaPalette.GetEnigmaGradient(t) * (1f - t * 0.5f);
                            CustomParticles.GenericFlare(beamPos, beamColor, 0.4f, 8);
                        }
                    }
                }
            }

            // Halo rings at various sizes (every 6 frames)
            if (Main.GameUpdateCount % 6 == 0)
            {
                Color haloColor = EnigmaPalette.GetRevelationGradient(explosionProgress);
                CustomParticles.HaloRing(center, haloColor, 0.8f * explosionProgress, 15);
            }

            // Prismatic sparkle explosion burst (every 5 frames)
            if (Main.GameUpdateCount % 5 == 0)
            {
                for (int i = 0; i < 6; i++)
                {
                    float angle = MathHelper.TwoPi * i / 6f;
                    Vector2 vel = angle.ToRotationVector2() * (6f * explosionProgress);
                    Color col = EnigmaPalette.PaletteLerp(EnigmaPalette.UnresolvedCadence, (float)i / 6f);
                    var glow = new GenericGlowParticle(center, vel, col, 0.5f, 22, true);
                    MagnumParticleHandler.SpawnParticle(glow);
                }
            }

            // Glyph circles rotating (every 10 frames)
            if (Main.GameUpdateCount % 10 == 0)
                CustomParticles.GlyphCircle(center, EnigmaPalette.Purple, count: 8, radius: currentRadius * 0.5f, rotationSpeed: 0.15f);

            // Eye burst — watching the destruction unfold (every 15 frames)
            if (Main.GameUpdateCount % 15 == 0)
                EnigmaVFXLibrary.SpawnEyeImpactBurst(center, 4, currentRadius * 0.01f);

            // Music notes — the cadence explodes outward
            if (Main.rand.NextBool(4))
            {
                float noteAngle = Main.rand.NextFloat() * MathHelper.TwoPi;
                Vector2 notePos = center + noteAngle.ToRotationVector2() * Main.rand.NextFloat(30f, currentRadius * 0.6f);
                EnigmaVFXLibrary.SpawnMusicNotes(notePos, 1, 15f, 0.85f, 1.1f, 35);
            }

            // Central void pulse
            if (Main.GameUpdateCount % 4 == 0)
                CustomParticles.GenericFlare(center, EnigmaPalette.VoidBlack, 1f, 15);

            Lighting.AddLight(center, EnigmaPalette.ArcaneFlash.ToVector3() * 1.2f);
        }

        // =====================================================================
        //  COLLAPSE FADE VFX — Phase 3: fading residual
        // =====================================================================

        /// <summary>
        /// ParadoxCollapseUltimate Phase 3: fading residual particles,
        /// dissolving glyphs, and diminishing void aura.
        /// </summary>
        public static void CollapseFadeVFX(Vector2 center, float fadeProgress, float intensity)
        {
            if (Main.dedServ) return;

            float currentRadius = 600f;

            // Fading residual particles (8 per burst, every 4 frames)
            if (Main.GameUpdateCount % 4 == 0)
            {
                for (int i = 0; i < 8; i++)
                {
                    float angle = Main.rand.NextFloat() * MathHelper.TwoPi;
                    float radius = Main.rand.NextFloat() * currentRadius;
                    Vector2 particlePos = center + angle.ToRotationVector2() * radius;

                    Color col = EnigmaPalette.GetEnigmaGradient(Main.rand.NextFloat()) * intensity * 0.5f;
                    CustomParticles.GenericFlare(particlePos, col, 0.3f * intensity, 10);
                }
            }

            // Glyphs fading (every 12 frames)
            if (Main.GameUpdateCount % 12 == 0)
                CustomParticles.GlyphAura(center, EnigmaPalette.Purple * intensity, radius: currentRadius * 0.4f, count: 2);

            // Fading watching eye — the last gaze
            if (Main.rand.NextBool(20) && intensity > 0.3f)
            {
                float eyeAngle = Main.rand.NextFloat() * MathHelper.TwoPi;
                Vector2 eyePos = center + eyeAngle.ToRotationVector2() * currentRadius * 0.3f;
                CustomParticles.EnigmaEyeGaze(eyePos, EnigmaPalette.EyeGreen * intensity * 0.4f, 0.2f * intensity);
            }

            // Music notes — the unresolved melody fading
            if (Main.rand.NextBool(8) && intensity > 0.2f)
                EnigmaVFXLibrary.SpawnMusicNotes(center, 1, currentRadius * 0.3f, 0.6f, 0.8f, 25);

            // Central void pulse
            if (Main.GameUpdateCount % 4 == 0)
                CustomParticles.GenericFlare(center, EnigmaPalette.VoidBlack, 1f * intensity, 15);

            Lighting.AddLight(center, EnigmaPalette.GetEnigmaGradient(0.5f).ToVector3() * intensity);
        }

        // =====================================================================
        //  COLLAPSE IMPACT VFX — ParadoxCollapse on-hit
        // =====================================================================

        /// <summary>
        /// ParadoxCollapseUltimate on-hit VFX: massive flash cascade,
        /// glyph explosion, watching eyes, radiant sparkle burst, and fractal ring.
        /// </summary>
        public static void CollapseImpactVFX(Vector2 pos, Vector2 targetCenter)
        {
            if (Main.dedServ) return;

            // Massive impact flash cascade
            CustomParticles.GenericFlare(pos, Color.White, 1.0f, 20);
            CustomParticles.GenericFlare(pos, EnigmaPalette.ArcaneFlash, 0.8f, 18);
            CustomParticles.GenericFlare(pos, EnigmaPalette.GreenFlame, 0.7f, 16);
            CustomParticles.HaloRing(pos, EnigmaPalette.Purple, 0.6f, 15);

            // Watching eye at impact
            CustomParticles.EnigmaEyeImpact(pos, targetCenter, EnigmaPalette.GreenFlame, 0.6f);

            // Radiant sparkle impact burst
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 5f);
                Color col = EnigmaPalette.PaletteLerp(EnigmaPalette.UnresolvedCadence, (float)i / 8f);
                var glow = new GenericGlowParticle(pos - new Vector2(0, 40f), vel, col, 0.45f, 22, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }
            CustomParticles.HaloRing(pos - new Vector2(0, 40f), EnigmaPalette.GreenFlame, 0.35f, 15);

            // Glyph circle + burst
            CustomParticles.GlyphCircle(pos, EnigmaPalette.Purple, count: 8, radius: 55f, rotationSpeed: 0.08f);
            CustomParticles.GlyphBurst(pos, EnigmaPalette.GreenFlame, count: 6, speed: 4f);

            // Fractal ring
            for (int i = 0; i < 10; i++)
            {
                float angle = MathHelper.TwoPi * i / 10f;
                Vector2 offset = angle.ToRotationVector2() * 40f;
                Color burstColor = EnigmaPalette.GetRevelationGradient((float)i / 10f);
                CustomParticles.GenericFlare(pos + offset, burstColor, 0.6f, 15);
            }

            // Eye burst — reality shatters
            EnigmaVFXLibrary.SpawnEyeImpactBurst(pos, 6, 5f);

            // Music notes — the paradox collapses with a thundering chord
            EnigmaVFXLibrary.SpawnMusicNotes(pos, 6, 30f, 0.9f, 1.1f, 35);

            // Heavy radial dust
            EnigmaVFXLibrary.SpawnRadialDustBurst(pos, 18, 7f);

            Lighting.AddLight(pos, EnigmaPalette.WhiteGreenFlash.ToVector3() * 1.5f);
        }
    }
}
