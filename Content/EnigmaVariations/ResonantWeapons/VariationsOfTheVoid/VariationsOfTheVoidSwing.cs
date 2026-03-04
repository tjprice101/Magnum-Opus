using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.BaseClasses;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.VFX.Trails;
using MagnumOpus.Content.EnigmaVariations.Debuffs;
using MagnumOpus.Content.EnigmaVariations.ResonantWeapons.TheSilentMeasure;
using MagnumOpus.Content.EnigmaVariations.ResonantWeapons.TheUnresolvedCadence;
using static MagnumOpus.Common.Systems.Particles.Particle;
using Terraria.GameContent;
using ReLogic.Content;
using MagnumOpus.Content.EnigmaVariations.ResonantWeapons.VariationsOfTheVoid.Particles;
using MagnumOpus.Content.EnigmaVariations.ResonantWeapons.VariationsOfTheVoid.Dusts;
using MagnumOpus.Content.EnigmaVariations.ResonantWeapons.VariationsOfTheVoid.Utilities;

namespace MagnumOpus.Content.EnigmaVariations.ResonantWeapons.VariationsOfTheVoid
{
    /// <summary>
    /// VARIATIONS OF THE VOID — Enigma Melee Sword (Swing Projectile).
    /// Held-projectile swing via MeleeSwingBase.
    /// 
    /// 3-Phase combo — each a variation of the void's voice:
    ///   Phase 0: HorizontalSweep — fast sweep, no sub-projectiles
    ///   Phase 1: DiagonalSlash — upward diagonal + 1 DimensionalSlash (33% damage)
    ///   Phase 2: HeavySlamFinisher — heavy slam + 3 DimensionalSlash + 3 HomingQuestionSeeker
    /// Every third strike (after Phase 2) spawns VoidConvergenceBeamSet tri-beam.
    /// Beams converge over 120 frames → Void Resonance Explosion (3x damage, 100→300 AoE).
    /// ParadoxBrand on hit (8s), seeking crystals on crit.
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

        private static readonly ComboPhase Phase0_HorizontalSweep = new ComboPhase(
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

        private static readonly ComboPhase Phase1_DiagonalSlash = new ComboPhase(
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

        private static readonly ComboPhase Phase2_HeavySlamFinisher = new ComboPhase(
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
            Phase0_HorizontalSweep,
            Phase1_DiagonalSlash,
            Phase2_HeavySlamFinisher
        };

        protected override Color[] GetPalette() => EnigmaPalette;

        protected override CalamityStyleTrailRenderer.TrailStyle GetTrailStyle()
            => CalamityStyleTrailRenderer.TrailStyle.Cosmic;

        protected override string GetSmearTexturePath(int comboStep)
        {
            return comboStep switch
            {
                0 => "MagnumOpus/Assets/VFX Asset Library/SlashArcSmears/SwordArcSmear",
                1 => "MagnumOpus/Assets/VFX Asset Library/SlashArcSmears/FlamingSwordArcSmear",
                2 => "MagnumOpus/Assets/VFX Asset Library/SlashArcSmears/FullCircleSwordArcSlash",
                _ => "MagnumOpus/Assets/VFX Asset Library/SlashArcSmears/SwordArcSmear"
            };
        }

        #endregion

        #region Virtual Overrides

        protected override Texture2D GetBladeTexture()
        {
            if (ModContent.HasAsset("MagnumOpus/Content/EnigmaVariations/ResonantWeapons/VariationsOfTheVoid/VariationsOfTheVoid"))
                return ModContent.Request<Texture2D>(
                    "MagnumOpus/Content/EnigmaVariations/ResonantWeapons/VariationsOfTheVoid/VariationsOfTheVoid", AssetRequestMode.ImmediateLoad).Value;
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

            // Phase 0 at 55% 窶・no gameplay effect (was VFX only)
            if (ComboStep == 0 && Progression >= 0.55f)
            {
                hasSpawnedSpecial = true;
            }

            // Phase 1 at 60% 窶・dimensional slash sub-projectile
            if (ComboStep == 1 && Progression >= 0.6f)
            {
                hasSpawnedSpecial = true;

                if (Main.myPlayer == Projectile.owner)
                {
                    Vector2 tipPos = GetBladeTipPosition();
                    Vector2 slashVel = SwordDirection * 10f;
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), tipPos, slashVel,
                        ModContent.ProjectileType<DimensionalSlash>(),
                        Projectile.damage / 3, 2f, Projectile.owner,
                        ai0: Projectile.rotation);
                }
            }

            // Phase 2 at 50% 窶・heavy finisher with sub-projectiles
            if (ComboStep == 2 && Progression >= 0.5f)
            {
                hasSpawnedSpecial = true;

                if (Main.myPlayer == Projectile.owner)
                {
                    Vector2 tipPos = GetBladeTipPosition();

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
            }
        }

        #endregion

        #region OnSwingHitNPC

        protected override void OnSwingHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // ParadoxBrand: 8 seconds per doc (480 frames)
            target.AddBuff(ModContent.BuffType<ParadoxBrand>(), 480);
            int stacks = hit.Crit ? 4 : 2;
            for (int s = 0; s < stacks; s++)
                target.AddBuff(ModContent.BuffType<ParadoxBrand>(), 480);

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

            Lighting.AddLight(target.Center, EnigmaPurple.ToVector3() * 0.8f);

            // === VFX: AbyssalEchoRing at target ===
            VoidVariationParticleHandler.Spawn(new AbyssalEchoRing(
                target.Center, VoidVariationUtils.VariationViolet, 0.3f, 30));

            // === VFX: 3-5 RiftSunderSpark burst from hit point ===
            for (int sp = 0; sp < Main.rand.Next(3, 6); sp++)
            {
                Vector2 sparkVel = Main.rand.NextVector2CircularEdge(5f, 5f);
                VoidVariationParticleHandler.Spawn(new RiftSunderSpark(
                    target.Center, sparkVel, Main.rand.NextFloat(0.15f, 0.25f), Main.rand.Next(15, 25)));
            }

            // === VFX: VoidWhisperMote at target ===
            VoidVariationParticleHandler.Spawn(new VoidWhisperMote(
                target.Center, Main.rand.NextVector2Circular(1f, 1f), VoidVariationUtils.VoidSurge, 0.3f, 35));
        }

        #endregion

        #region DrawCustomVFX

        protected override void DrawCustomVFX(SpriteBatch sb)
        {
            if (Progression <= 0f || Progression >= 1f) return;

            Vector2 tipPos = GetBladeTipPosition();
            int frame = (int)Main.GameUpdateCount;

            // === Base particles for ALL phases: 1-2 VoidWhisperMote per frame at swing tip ===
            for (int i = 0; i < Main.rand.Next(1, 3); i++)
            {
                Vector2 drift = Main.rand.NextVector2Circular(1.5f, 1.5f);
                Color whisperColor = Main.rand.NextBool() ? VoidVariationUtils.VariationViolet : VoidVariationUtils.RiftTeal;
                VoidVariationParticleHandler.Spawn(new VoidWhisperMote(
                    tipPos + Main.rand.NextVector2Circular(8f, 8f), drift, whisperColor,
                    Main.rand.NextFloat(0.08f, 0.15f), Main.rand.Next(25, 40)));
            }

            // === Void echo afterimage 窶・ghostly flicker fragments near blade (doc: "VoidEchoFlickerParticle") ===
            if (frame % 3 == 0 && Main.rand.NextBool(2))
            {
                // Rectangular glitch at a random point along the blade 窶・void echo aesthetic
                Vector2 echoPos = Vector2.Lerp(Owner.MountedCenter, tipPos, Main.rand.NextFloat(0.3f, 1f));
                echoPos += Main.rand.NextVector2Circular(10f, 10f);
                Color echoColor = Main.rand.NextBool() ? VoidVariationUtils.AbyssPurple : VoidVariationUtils.RiftTeal;
                VoidVariationParticleHandler.Spawn(new RiftSunderSpark(
                    echoPos, Vector2.Zero, Main.rand.NextFloat(0.12f, 0.22f), Main.rand.Next(3, 6)));
            }

            // === Phase 0 (Horizontal Sweep): RiftSunderSpark perpendicular to blade every 3 frames ===
            if (ComboStep == 0 && frame % 3 == 0)
            {
                Vector2 sparkVel = SwordDirection.RotatedBy(MathHelper.PiOver2 * Direction) * Main.rand.NextFloat(3f, 6f);
                VoidVariationParticleHandler.Spawn(new RiftSunderSpark(
                    tipPos, sparkVel, Main.rand.NextFloat(0.1f, 0.2f), Main.rand.Next(15, 25)));
            }

            // === Phase 1 (Diagonal Slash): Two flanking spark streams for diagonal X-feel ===
            if (ComboStep == 1 && frame % 2 == 0)
            {
                for (int side = -1; side <= 1; side += 2)
                {
                    Vector2 flankVel = SwordDirection.RotatedBy(MathHelper.PiOver4 * side) * Main.rand.NextFloat(3f, 5f);
                    VoidVariationParticleHandler.Spawn(new RiftSunderSpark(
                        tipPos + Main.rand.NextVector2Circular(6f, 6f), flankVel,
                        Main.rand.NextFloat(0.12f, 0.2f), Main.rand.Next(12, 20)));
                }
            }

            // === Phase 2 (Heavy Slam): AbyssalEchoRing every 3 frames + dense spark shower ===
            if (ComboStep >= 2 && frame % 3 == 0)
            {
                Color ringColor = Main.rand.NextBool() ? VoidVariationUtils.VoidSurge : VoidVariationUtils.RiftTeal;
                VoidVariationParticleHandler.Spawn(new AbyssalEchoRing(
                    tipPos, ringColor, 0.18f, Main.rand.Next(20, 35)));

                // Extra sparks for finisher weight
                for (int i = 0; i < 2; i++)
                {
                    Vector2 slamVel = SwordDirection * Main.rand.NextFloat(4f, 8f) + Main.rand.NextVector2Circular(3f, 3f);
                    VoidVariationParticleHandler.Spawn(new RiftSunderSpark(
                        tipPos, slamVel, Main.rand.NextFloat(0.15f, 0.25f), Main.rand.Next(15, 25)));
                }
            }

            // === Every 5 frames: 1 VoidVariationDust at swing tip ===
            if (frame % 5 == 0)
            {
                Vector2 dustVel = Main.rand.NextVector2Circular(2f, 2f);
                Dust.NewDustPerfect(tipPos, ModContent.DustType<VoidVariationDust>(), dustVel, 0, default, Main.rand.NextFloat(0.5f, 0.8f));
            }

            // === Mid-swing burst (progress 0.4-0.6): 4-7 RiftSunderSpark along the arc ===
            if (Progression >= 0.4f && Progression <= 0.6f && frame % 2 == 0)
            {
                int burstCount = Main.rand.Next(4, 8);
                for (int i = 0; i < burstCount; i++)
                {
                    float arcT = Main.rand.NextFloat();
                    Vector2 arcPos = Vector2.Lerp(Owner.MountedCenter, tipPos, arcT);
                    Vector2 burstVel = SwordDirection.RotatedByRandom(0.8f) * Main.rand.NextFloat(4f, 8f);
                    VoidVariationParticleHandler.Spawn(new RiftSunderSpark(
                        arcPos, burstVel, Main.rand.NextFloat(0.12f, 0.22f), Main.rand.Next(12, 20)));
                }
            }

            // === Attack peak burst (progress ~0.5): AbyssalEchoRing + VoidWhisperMote radial burst ===
            int attackFrame = (int)(SwingTime * 0.5f);
            if ((int)Timer == attackFrame)
            {
                VoidVariationParticleHandler.Spawn(new AbyssalEchoRing(
                    tipPos, VoidVariationUtils.VariationViolet, 0.25f + ComboStep * 0.1f, 30));

                for (int i = 0; i < Main.rand.Next(3, 6); i++)
                {
                    Vector2 radialVel = Main.rand.NextVector2CircularEdge(4f, 4f);
                    VoidVariationParticleHandler.Spawn(new VoidWhisperMote(
                        tipPos, radialVel, VoidVariationUtils.VoidSurge,
                        Main.rand.NextFloat(0.15f, 0.25f), Main.rand.Next(15, 25)));
                }
            }
        }

        #endregion
    }
}
