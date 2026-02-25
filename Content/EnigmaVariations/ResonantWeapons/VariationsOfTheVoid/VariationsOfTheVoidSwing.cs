using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.BaseClasses;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using static MagnumOpus.Common.Systems.Particles.Particle;
using MagnumOpus.Common.Systems.VFX.Trails;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Content.EnigmaVariations.Debuffs;

namespace MagnumOpus.Content.EnigmaVariations.ResonantWeapons
{
    /// <summary>
    /// VARIATIONS OF THE VOID — Enigma Melee Sword (Swing Projectile).
    /// Held-projectile swing via MeleeSwingBase.
    /// 
    /// 3-Phase combo — each a different voice of the void:
    ///   Phase 0: VoidWhisper — fast, subtle cleave
    ///   Phase 1: AbyssalEcho — medium sweep, flipped arc
    ///   Phase 2: RiftSunderFinisher — heavy finisher, spawns sub-projectiles
    /// </summary>
    public sealed class VariationsOfTheVoidSwing : MeleeSwingBase
    {
        #region Theme Colors & Palette

        private static readonly Color EnigmaBlack = MagnumThemePalettes.EnigmaBlack;
        private static readonly Color EnigmaPurple = MagnumThemePalettes.EnigmaPurple;
        private static readonly Color EnigmaGreen = MagnumThemePalettes.EnigmaGreen;
        private static readonly Color EnigmaDeepPurple = MagnumThemePalettes.EnigmaDeepPurple;
        private static readonly Color EnigmaVoid = MagnumThemePalettes.EnigmaVoid;

        private static readonly Color[] EnigmaPalette = new Color[]
        {
            MagnumThemePalettes.EnigmaBlack,  // [0] void darkness
            new Color(60, 20, 100),            // [1] deep arcane
            MagnumThemePalettes.EnigmaPurple,  // [2] primary purple
            MagnumThemePalettes.EnigmaGreen,   // [3] eerie green
            new Color(180, 140, 230),          // [4] bright arcane
            new Color(230, 250, 220)           // [5] whitehot
        };

        #endregion

        #region Combo Phase Definitions

        private static readonly ComboPhase Phase0_VoidWhisper = new ComboPhase(
            new CurveSegment[]
            {
                new CurveSegment(EasingType.PolyOut, 0f, -0.8f, 0.15f, 2),
                new CurveSegment(EasingType.PolyIn, 0.2f, -0.65f, 1.55f, 3),
                new CurveSegment(EasingType.PolyOut, 0.8f, 0.9f, 0.1f, 2)
            },
            maxAngle: MathHelper.PiOver2 * 1.4f,
            duration: 16,
            bladeLength: 145f,
            flip: false,
            squish: 0.92f,
            damageMult: 0.85f
        );

        private static readonly ComboPhase Phase1_AbyssalEcho = new ComboPhase(
            new CurveSegment[]
            {
                new CurveSegment(EasingType.PolyOut, 0f, -0.9f, 0.2f, 2),
                new CurveSegment(EasingType.PolyIn, 0.22f, -0.7f, 1.6f, 3),
                new CurveSegment(EasingType.PolyOut, 0.82f, 0.9f, 0.1f, 2)
            },
            maxAngle: MathHelper.PiOver2 * 1.6f,
            duration: 18,
            bladeLength: 150f,
            flip: true,
            squish: 0.90f,
            damageMult: 1.0f
        );

        private static readonly ComboPhase Phase2_RiftSunderFinisher = new ComboPhase(
            new CurveSegment[]
            {
                new CurveSegment(EasingType.PolyOut, 0f, -1.0f, 0.3f, 2),
                new CurveSegment(EasingType.PolyIn, 0.28f, -0.7f, 1.7f, 3),
                new CurveSegment(EasingType.PolyOut, 0.85f, 1.0f, 0.05f, 2)
            },
            maxAngle: MathHelper.PiOver2 * 2.0f,
            duration: 24,
            bladeLength: 165f,
            flip: false,
            squish: 0.85f,
            damageMult: 1.25f
        );

        #endregion

        #region Abstract Overrides

        protected override ComboPhase[] GetAllPhases() => new[]
        {
            Phase0_VoidWhisper,
            Phase1_AbyssalEcho,
            Phase2_RiftSunderFinisher
        };

        protected override Color[] GetPalette() => EnigmaPalette;

        protected override CalamityStyleTrailRenderer.TrailStyle GetTrailStyle()
            => CalamityStyleTrailRenderer.TrailStyle.Cosmic;

        protected override string GetSmearTexturePath(int comboStep)
        {
            return comboStep switch
            {
                2 => "MagnumOpus/Assets/Particles/SwordArc2",
                _ => "MagnumOpus/Assets/Particles/SwordArc8"
            };
        }

        #endregion

        #region Virtual Overrides

        protected override Texture2D GetBladeTexture()
        {
            if (ModContent.HasAsset("MagnumOpus/Content/EnigmaVariations/ResonantWeapons/VariationsOfTheVoid"))
                return ModContent.Request<Texture2D>(
                    "MagnumOpus/Content/EnigmaVariations/ResonantWeapons/VariationsOfTheVoid").Value;
            return base.GetBladeTexture();
        }

        protected override SoundStyle GetSwingSound()
            => SoundID.Item71 with { Pitch = 0.15f + ComboStep * 0.12f, Volume = 0.6f };

        protected override int GetInitialDustType() => DustID.PurpleTorch;
        protected override int GetSecondaryDustType() => DustID.CursedTorch;

        protected override Vector3 GetLightColor()
        {
            Color c = Color.Lerp(EnigmaDeepPurple, EnigmaGreen, Progression * 0.5f);
            return c.ToVector3() * 0.7f;
        }

        #endregion

        #region HandleComboSpecials

        protected override void HandleComboSpecials()
        {
            if (hasSpawnedSpecial) return;

            if (ComboStep == 0 && Progression >= 0.55f)
            {
                hasSpawnedSpecial = true;
                CustomParticles.GenericFlare(GetBladeTipPosition(), EnigmaPurple, 0.55f, 15);
                CustomParticles.GlyphBurst(GetBladeTipPosition(), EnigmaDeepPurple, 3, 3f);
                ThemedParticles.EnigmaMusicNotes(GetBladeTipPosition(), 2, 20f);
            }

            if (ComboStep == 1 && Progression >= 0.6f)
            {
                hasSpawnedSpecial = true;
                Vector2 tipPos = GetBladeTipPosition();

                if (Main.myPlayer == Projectile.owner)
                {
                    Vector2 slashVel = SwordDirection * 10f;
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), tipPos, slashVel,
                        ModContent.ProjectileType<DimensionalSlash>(),
                        Projectile.damage / 3, 2f, Projectile.owner,
                        ai0: Projectile.rotation);
                }

                CustomParticles.GenericFlare(tipPos, EnigmaGreen, 0.6f, 18);
                CustomParticles.HaloRing(tipPos, EnigmaPurple, 0.4f, 14);
                ThemedParticles.EnigmaMusicNotes(tipPos, 3, 25f);
            }

            if (ComboStep == 2 && Progression >= 0.5f)
            {
                hasSpawnedSpecial = true;
                Vector2 tipPos = GetBladeTipPosition();

                if (Main.myPlayer == Projectile.owner)
                {
                    // Forward slash
                    Vector2 fwdVel = SwordDirection * 12f;
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), tipPos, fwdVel,
                        ModContent.ProjectileType<DimensionalSlash>(),
                        (int)(Projectile.damage * 0.45f), 3f, Projectile.owner,
                        ai0: Projectile.rotation);

                    // Two perpendicular slashes
                    for (int side = -1; side <= 1; side += 2)
                    {
                        Vector2 sideVel = SwordDirection.RotatedBy(side * MathHelper.PiOver4) * 9f;
                        Projectile.NewProjectile(Projectile.GetSource_FromThis(), tipPos, sideVel,
                            ModContent.ProjectileType<DimensionalSlash>(),
                            Projectile.damage / 4, 2f, Projectile.owner,
                            ai0: Projectile.rotation + side * 0.5f);
                    }

                    // Homing question seekers
                    for (int i = 0; i < 3; i++)
                    {
                        float angle = Projectile.rotation + MathHelper.ToRadians(-30 + i * 30);
                        Vector2 crystalVel = angle.ToRotationVector2() * Main.rand.NextFloat(6f, 10f);
                        Projectile.NewProjectile(Projectile.GetSource_FromThis(), tipPos, crystalVel,
                            ModContent.ProjectileType<HomingQuestionSeeker>(),
                            Projectile.damage / 5, 1f, Projectile.owner);
                    }
                }

                // Heavy VFX
                UnifiedVFX.EnigmaVariations.Impact(tipPos, 1.3f);
                CustomParticles.GlyphCircle(tipPos, EnigmaPurple, 6, 55f, 0.03f);
                for (int i = 0; i < 4; i++)
                {
                    Color rc = Color.Lerp(EnigmaDeepPurple, EnigmaGreen, i / 4f);
                    CustomParticles.HaloRing(tipPos, rc, 0.35f + i * 0.12f, 14 + i * 3);
                }
                ThemedParticles.EnigmaMusicNotes(tipPos, 4, 30f);
                MagnumScreenEffects.AddScreenShake(4f);
            }
        }

        #endregion

        #region OnSwingHitNPC

        protected override void OnSwingHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<ParadoxBrand>(), 300);
            int stacks = hit.Crit ? 4 : 2;
            for (int s = 0; s < stacks; s++)
                target.AddBuff(ModContent.BuffType<ParadoxBrand>(), 300);

            if (hit.Crit && Main.myPlayer == Projectile.owner)
            {
                for (int i = 0; i < 3; i++)
                {
                    Vector2 crystalVel = Main.rand.NextVector2CircularEdge(7f, 7f);
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), target.Center, crystalVel,
                        ModContent.ProjectileType<HomingQuestionSeeker>(),
                        Projectile.damage / 5, 1f, Projectile.owner);
                }
            }

            UnifiedVFX.EnigmaVariations.Impact(target.Center, 0.9f);
            CustomParticles.GlyphBurst(target.Center, EnigmaPurple, 4, 4f);
            ThemedParticles.EnigmaMusicNotes(target.Center, 2, 18f);

            for (int i = 0; i < 6; i++)
            {
                Vector2 dv = Main.rand.NextVector2Circular(5f, 5f);
                Dust d = Dust.NewDustPerfect(target.Center, DustID.PurpleTorch, dv, 0, EnigmaPurple, 1.3f);
                d.noGravity = true;
            }

            Lighting.AddLight(target.Center, EnigmaPurple.ToVector3() * 0.8f);
        }

        #endregion

        #region DrawCustomVFX

        protected override void DrawCustomVFX(SpriteBatch sb)
        {
            if (Progression < 0.08f || Progression > 0.92f) return;

            Vector2 tipPos = GetBladeTipPosition();

            // Dense dust trail
            for (int i = 0; i < 2; i++)
            {
                Vector2 dustPos = Vector2.Lerp(Owner.MountedCenter, tipPos, Main.rand.NextFloat(0.4f, 1f));
                int dustType = i == 0 ? DustID.PurpleTorch : DustID.CursedTorch;
                Dust d = Dust.NewDustPerfect(dustPos, dustType,
                    -SwordDirection * Main.rand.NextFloat(1f, 3f), 0,
                    Color.Lerp(EnigmaPurple, EnigmaGreen, Main.rand.NextFloat()), 1.4f);
                d.noGravity = true;
            }

            // Void shimmer
            if (Main.rand.NextBool(3))
            {
                Vector2 shimmerPos = tipPos + Main.rand.NextVector2Circular(8f, 8f);
                Dust shimmer = Dust.NewDustPerfect(shimmerPos, DustID.Enchanted_Pink,
                    Vector2.Zero, 0, EnigmaGreen, 0.8f);
                shimmer.noGravity = true;
            }

            // Eye sparkle
            if (Main.rand.NextBool(4))
            {
                CustomParticles.EnigmaEyeGaze(tipPos + Main.rand.NextVector2Circular(12f, 12f),
                    EnigmaPurple, 0.35f);
            }

            // Music notes — hue-shifting through void spectrum
            if (Main.rand.NextBool(5))
            {
                Vector2 noteVel = -SwordDirection * 1.5f + Main.rand.NextVector2Circular(0.5f, 0.5f);
                MagnumParticleHandler.SpawnParticle(new HueShiftingMusicNoteParticle(
                    tipPos, noteVel,
                    hueMin: 0.38f, hueMax: 0.77f,
                    saturation: 0.85f, luminosity: 0.6f,
                    scale: 0.75f, lifetime: 25, hueSpeed: 0.025f));
            }

            // Blade-tip bloom — Enigma void glow
            {
                float bloomOpacity = MathHelper.Clamp((Progression - 0.08f) / 0.12f, 0f, 1f)
                                   * MathHelper.Clamp((0.92f - Progression) / 0.12f, 0f, 1f);
                float bloomScale = 0.45f + ComboStep * 0.08f;
                BloomRenderer.DrawBloomStackAdditive(tipPos, EnigmaPurple, EnigmaGreen, bloomScale, bloomOpacity);
            }

            // Glyph accent on finisher
            if (ComboStep == 2 && Main.rand.NextBool(6))
            {
                CustomParticles.Glyph(tipPos + Main.rand.NextVector2Circular(15f, 15f),
                    EnigmaDeepPurple, 0.35f, -1);
            }
        }

        #endregion
    }
}
