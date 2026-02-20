using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.BaseClasses;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using static MagnumOpus.Common.Systems.Particles.Particle;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.VFX.Trails;
using MagnumOpus.Content.EnigmaVariations.Debuffs;

namespace MagnumOpus.Content.EnigmaVariations.ResonantWeapons
{
    /// <summary>
    /// THE UNRESOLVED CADENCE — Swing projectile (held-projectile combo).
    /// 3-phase combo: VoidCleave → ParadoxSlash → DimensionalSeverance
    /// Each swing a different movement in the Enigma's unknowable melody.
    /// </summary>
    public sealed class TheUnresolvedCadenceSwing : MeleeSwingBase
    {
        #region Theme Colors

        private static readonly Color EnigmaBlack = MagnumThemePalettes.EnigmaBlack;
        private static readonly Color EnigmaPurple = MagnumThemePalettes.EnigmaPurple;
        private static readonly Color EnigmaGreen = MagnumThemePalettes.EnigmaGreen;
        private static readonly Color EnigmaDeepPurple = MagnumThemePalettes.EnigmaDeepPurple;
        private static readonly Color EnigmaVoid = MagnumThemePalettes.EnigmaVoid;

        private static readonly Color[] EnigmaPalette = new Color[]
        {
            MagnumThemePalettes.EnigmaBlack,      // [0] Pianissimo — void darkness
            MagnumThemePalettes.EnigmaDeepPurple,  // [1] Piano — deep arcane
            MagnumThemePalettes.EnigmaPurple,      // [2] Mezzo — enigma purple
            new Color(100, 140, 200),              // [3] Forte — transitional
            MagnumThemePalettes.EnigmaGreen,       // [4] Fortissimo — eerie green
            new Color(180, 255, 180),              // [5] Sforzando — bright green-white
        };

        #endregion

        #region Combo Phases

        // Phase 0: VoidCleave — Quick, probing slash
        private static readonly ComboPhase Phase0_VoidCleave = new ComboPhase(
            new CurveSegment[]
            {
                new CurveSegment(EasingType.PolyOut, 0f, -0.9f, 0.2f, 2),
                new CurveSegment(EasingType.PolyIn, 0.2f, -0.7f, 1.5f, 3),
                new CurveSegment(EasingType.PolyOut, 0.8f, 0.8f, 0.1f, 2),
            },
            maxAngle: MathHelper.PiOver2 * 1.4f,
            duration: 18,
            bladeLength: 150f,
            flip: false,
            squish: 0.92f,
            damageMult: 0.85f
        );

        // Phase 1: ParadoxSlash — Reversed arc, reality-bending
        private static readonly ComboPhase Phase1_ParadoxSlash = new ComboPhase(
            new CurveSegment[]
            {
                new CurveSegment(EasingType.PolyOut, 0f, -1f, 0.3f, 2),
                new CurveSegment(EasingType.PolyIn, 0.25f, -0.7f, 1.55f, 3),
                new CurveSegment(EasingType.PolyOut, 0.85f, 0.85f, 0.12f, 2),
            },
            maxAngle: MathHelper.PiOver2 * 1.6f,
            duration: 20,
            bladeLength: 155f,
            flip: true,
            squish: 0.9f,
            damageMult: 1.0f
        );

        // Phase 2: DimensionalSeverance — Heavy finisher, tears space
        private static readonly ComboPhase Phase2_DimensionalSeverance = new ComboPhase(
            new CurveSegment[]
            {
                new CurveSegment(EasingType.PolyOut, 0f, -1.1f, 0.3f, 2),
                new CurveSegment(EasingType.PolyIn, 0.2f, -0.8f, 1.9f, 3),
                new CurveSegment(EasingType.PolyOut, 0.82f, 1.1f, 0.1f, 2),
            },
            maxAngle: MathHelper.PiOver2 * 2.0f,
            duration: 25,
            bladeLength: 168f,
            flip: false,
            squish: 0.85f,
            damageMult: 1.3f
        );

        #endregion

        #region Abstract Overrides

        protected override ComboPhase[] GetAllPhases() => new[]
        {
            Phase0_VoidCleave,
            Phase1_ParadoxSlash,
            Phase2_DimensionalSeverance,
        };

        protected override Color[] GetPalette() => EnigmaPalette;

        protected override CalamityStyleTrailRenderer.TrailStyle GetTrailStyle()
            => CalamityStyleTrailRenderer.TrailStyle.Cosmic;

        protected override string GetSmearTexturePath(int comboStep)
        {
            return comboStep switch
            {
                2 => "MagnumOpus/Assets/Particles/SwordArc3",
                _ => "MagnumOpus/Assets/Particles/SwordArc6",
            };
        }

        #endregion

        #region Virtual Overrides

        protected override Texture2D GetBladeTexture()
        {
            if (ModContent.HasAsset("MagnumOpus/Content/EnigmaVariations/ResonantWeapons/TheUnresolvedCadence"))
                return ModContent.Request<Texture2D>("MagnumOpus/Content/EnigmaVariations/ResonantWeapons/TheUnresolvedCadence").Value;
            return base.GetBladeTexture();
        }

        protected override SoundStyle GetSwingSound()
            => SoundID.Item71 with { Pitch = 0.2f + ComboStep * 0.15f, Volume = 0.8f };

        protected override int GetInitialDustType() => DustID.PurpleTorch;

        protected override int GetSecondaryDustType() => DustID.CursedTorch;

        protected override Vector3 GetLightColor()
        {
            float intensity = 0.55f + ComboStep * 0.15f;
            Color c = Color.Lerp(EnigmaPurple, EnigmaGreen, ComboStep / 3f);
            return c.ToVector3() * intensity;
        }

        #endregion

        #region Combo Specials

        protected override void HandleComboSpecials()
        {
            if (hasSpawnedSpecial) return;

            // Phase 0 at 55% — void ripple spark
            if (ComboStep == 0 && Progression >= 0.55f)
            {
                hasSpawnedSpecial = true;
                Vector2 tipPos = GetBladeTipPosition();
                CustomParticles.GenericFlare(tipPos, EnigmaPurple, 0.6f, 15);
                CustomParticles.GlyphBurst(tipPos, EnigmaDeepPurple, 3, 3f);
                ThemedParticles.EnigmaMusicNotes(tipPos, 2, 12f);
            }

            // Phase 1 at 60% — dimensional slash sub-projectile
            if (ComboStep == 1 && Progression >= 0.60f)
            {
                hasSpawnedSpecial = true;
                Vector2 tipPos = GetBladeTipPosition();

                if (Main.myPlayer == Projectile.owner)
                {
                    Vector2 slashVel = SwordDirection * 12f;
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), tipPos, slashVel,
                        ModContent.ProjectileType<DimensionalSlash>(),
                        (int)(Projectile.damage * 0.35f), 2f, Projectile.owner,
                        ai0: Projectile.rotation);
                }

                CustomParticles.GenericFlare(tipPos, EnigmaGreen, 0.7f, 18);
                CustomParticles.HaloRing(tipPos, EnigmaPurple, 0.4f, 15);
                ThemedParticles.EnigmaMusicNotes(tipPos, 3, 20f);
                CustomParticles.GlyphBurst(tipPos, EnigmaGreen, 4, 4f);
            }

            // Phase 2 at 50% — massive dimensional tear (finisher)
            if (ComboStep == 2 && Progression >= 0.50f)
            {
                hasSpawnedSpecial = true;
                Vector2 tipPos = GetBladeTipPosition();

                if (Main.myPlayer == Projectile.owner)
                {
                    // Forward slash
                    Vector2 slashVel = SwordDirection * 15f;
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), tipPos, slashVel,
                        ModContent.ProjectileType<DimensionalSlash>(),
                        (int)(Projectile.damage * 0.5f), 3f, Projectile.owner,
                        ai0: Projectile.rotation);

                    // Perpendicular slashes
                    for (int side = -1; side <= 1; side += 2)
                    {
                        Vector2 perpVel = SwordDirection.RotatedBy(MathHelper.PiOver2 * side) * 10f;
                        Projectile.NewProjectile(Projectile.GetSource_FromThis(), tipPos, perpVel,
                            ModContent.ProjectileType<DimensionalSlash>(),
                            (int)(Projectile.damage * 0.25f), 2f, Projectile.owner,
                            ai0: Projectile.rotation + MathHelper.PiOver2 * side);
                    }

                    // Homing question seekers
                    for (int i = 0; i < 4; i++)
                    {
                        float angle = MathHelper.TwoPi * i / 4f + Main.rand.NextFloat(-0.3f, 0.3f);
                        Vector2 seekerVel = angle.ToRotationVector2() * Main.rand.NextFloat(6f, 10f);
                        Projectile.NewProjectile(Projectile.GetSource_FromThis(), tipPos, seekerVel,
                            ModContent.ProjectileType<HomingQuestionSeeker>(),
                            Projectile.damage / 4, 1f, Projectile.owner);
                    }
                }

                UnifiedVFX.EnigmaVariations.Impact(tipPos, 1.3f);
                CustomParticles.GenericFlare(tipPos, Color.White, 1.0f, 22);
                CustomParticles.GlyphCircle(tipPos, EnigmaPurple, 6, 60f, 0.06f);

                for (int i = 0; i < 5; i++)
                {
                    float angle = MathHelper.TwoPi * i / 5f;
                    CustomParticles.HaloRing(tipPos, Color.Lerp(EnigmaPurple, EnigmaGreen, i / 5f),
                        0.35f + i * 0.1f, 15 + i * 3);
                }

                ThemedParticles.EnigmaMusicNoteBurst(tipPos, 5, 30f);
                MagnumScreenEffects.AddScreenShake(5f);
            }
        }

        #endregion

        #region On Hit

        protected override void OnSwingHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // ParadoxBrand debuff
            target.AddBuff(ModContent.BuffType<ParadoxBrand>(), 600);
            var brandNPC = target.GetGlobalNPC<ParadoxBrandNPC>();
            brandNPC.AddParadoxStack(target, hit.Crit ? 5 : 3);

            // Seeking crystals on crit
            if (hit.Crit && Main.myPlayer == Projectile.owner)
            {
                for (int i = 0; i < 3; i++)
                {
                    Vector2 seekerVel = Main.rand.NextVector2CircularEdge(8f, 8f);
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), target.Center, seekerVel,
                        ModContent.ProjectileType<HomingQuestionSeeker>(),
                        Projectile.damage / 4, 1f, Projectile.owner);
                }
            }

            // Impact VFX
            UnifiedVFX.EnigmaVariations.Impact(target.Center, 0.8f + ComboStep * 0.2f);
            CustomParticles.GlyphBurst(target.Center, EnigmaGreen, 3 + ComboStep, 4f);
            ThemedParticles.EnigmaMusicNotes(target.Center, 2 + ComboStep, 20f);

            // Dust burst
            for (int i = 0; i < 6 + ComboStep * 2; i++)
            {
                Vector2 dustVel = Main.rand.NextVector2Circular(6f, 6f);
                Dust d = Dust.NewDustPerfect(target.Center, DustID.PurpleTorch, dustVel, 0,
                    Color.Lerp(EnigmaPurple, EnigmaGreen, Main.rand.NextFloat()), 1.4f);
                d.noGravity = true;
            }

            Lighting.AddLight(target.Center, EnigmaPurple.ToVector3() * 0.9f);
        }

        #endregion

        #region Custom VFX

        protected override void DrawCustomVFX(SpriteBatch sb)
        {
            if (Progression < 0.08f || Progression > 0.92f) return;

            // Dense enigma dust trail (2 per frame)
            for (int i = 0; i < 2; i++)
            {
                Vector2 dustPos = Owner.MountedCenter + SwordDirection * CurrentPhase.BladeLength * Main.rand.NextFloat(0.4f, 1f);
                int dustType = Main.rand.NextBool() ? DustID.PurpleTorch : DustID.CursedTorch;
                Dust d = Dust.NewDustPerfect(dustPos, dustType,
                    -SwordDirection * Main.rand.NextFloat(1f, 3f), 0,
                    Color.Lerp(EnigmaPurple, EnigmaGreen, Main.rand.NextFloat()), 1.5f);
                d.noGravity = true;
            }

            // Enigma eye sparkle (1-in-3)
            if (Main.rand.NextBool(3))
            {
                Vector2 sparkPos = Owner.MountedCenter + SwordDirection * CurrentPhase.BladeLength * Main.rand.NextFloat(0.5f, 0.95f);
                CustomParticles.EnigmaEyeGaze(sparkPos, EnigmaPurple, 0.3f);
            }

            // Void shimmer (1-in-4)
            if (Main.rand.NextBool(4))
            {
                Vector2 shimmerPos = Owner.MountedCenter + SwordDirection * CurrentPhase.BladeLength * Main.rand.NextFloat(0.3f, 0.9f);
                Color shimmerColor = Color.Lerp(EnigmaBlack, EnigmaGreen, Main.rand.NextFloat());
                var shimmer = new GenericGlowParticle(shimmerPos, -SwordDirection * Main.rand.NextFloat(0.5f, 2f),
                    shimmerColor * 0.7f, 0.25f, 12, true);
                MagnumParticleHandler.SpawnParticle(shimmer);
            }

// Music notes (1-in-5) — HueShifting bloom notes cycling Enigma purple↔green
        if (Main.rand.NextBool(5))
        {
            Vector2 notePos = Owner.MountedCenter + SwordDirection * CurrentPhase.BladeLength * Main.rand.NextFloat(0.6f, 1f);
            MagnumParticleHandler.SpawnParticle(new HueShiftingMusicNoteParticle(
                notePos, -SwordDirection * 1.5f,
                hueMin: 0.38f, hueMax: 0.77f,   // green(0.38) ↔ purple(0.77)
                saturation: 0.85f, luminosity: 0.6f,
                scale: 0.75f, lifetime: 25, hueSpeed: 0.025f));
            }

            // Blade-tip bloom stack — Calamity-style 4-layer additive bloom
            {
                Vector2 tipWorld = Owner.MountedCenter + SwordDirection * CurrentPhase.BladeLength;
                float bloomOpacity = MathHelper.Clamp((Progression - 0.10f) / 0.15f, 0f, 1f)
                                   * MathHelper.Clamp((0.90f - Progression) / 0.15f, 0f, 1f);
                BloomRenderer.DrawBloomStackAdditive(tipWorld, EnigmaPurple, EnigmaGreen,
                    scale: 0.45f + ComboStep * 0.08f, opacity: bloomOpacity * 0.7f);
            }

            // Glyph accent on finisher (1-in-6 during phase 2)
            if (ComboStep == 2 && Main.rand.NextBool(6))
            {
                Vector2 glyphPos = Owner.MountedCenter + SwordDirection * CurrentPhase.BladeLength * Main.rand.NextFloat(0.4f, 1f);
                CustomParticles.Glyph(glyphPos, EnigmaGreen, 0.35f);
            }
        }

        #endregion
    }
}
