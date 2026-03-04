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
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.VFX.Trails;
using MagnumOpus.Content.EnigmaVariations.Debuffs;
using MagnumOpus.Content.EnigmaVariations.ResonantWeapons.TheSilentMeasure;
using MagnumOpus.Content.EnigmaVariations.ResonantWeapons.TheUnresolvedCadence.Particles;
using MagnumOpus.Content.EnigmaVariations.ResonantWeapons.TheUnresolvedCadence.Dusts;
using MagnumOpus.Content.EnigmaVariations.ResonantWeapons.TheUnresolvedCadence.Utilities;
using static MagnumOpus.Common.Systems.Particles.Particle;
using ReLogic.Content;

namespace MagnumOpus.Content.EnigmaVariations.ResonantWeapons.TheUnresolvedCadence
{
    /// <summary>
    /// THE UNRESOLVED CADENCE 窶・Swing projectile (held-projectile combo).
    /// 3-phase combo: The Question 竊・The Doubt 竊・The Silence
    /// Each swing builds Inevitability. At 10 stacks, Paradox Collapse tears reality.
    /// Glitch aesthetic: chromatic aberration, scan-line flickers, dimensional tears.
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
            MagnumThemePalettes.EnigmaBlack,      // [0] Pianissimo 窶・void darkness
            MagnumThemePalettes.EnigmaDeepPurple,  // [1] Piano 窶・deep arcane
            MagnumThemePalettes.EnigmaPurple,      // [2] Mezzo 窶・enigma purple
            new Color(100, 140, 200),              // [3] Forte 窶・transitional
            MagnumThemePalettes.EnigmaGreen,       // [4] Fortissimo 窶・eerie green
            new Color(180, 255, 180),              // [5] Sforzando 窶・bright green-white
        };

        #endregion

        #region Combo Phases

        // Phase 0: The Question 窶・Quick, probing diagonal slash
        private static readonly ComboPhase Phase0_TheQuestion = new ComboPhase(
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

        // Phase 1: The Doubt 窶・Cross-slash (X pattern), reversed arc
        private static readonly ComboPhase Phase1_TheDoubt = new ComboPhase(
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

        // Phase 2: The Silence 窶・Heavy downward slam, tears space
        private static readonly ComboPhase Phase2_TheSilence = new ComboPhase(
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
            Phase0_TheQuestion,
            Phase1_TheDoubt,
            Phase2_TheSilence,
        };

        protected override Color[] GetPalette() => EnigmaPalette;

        protected override CalamityStyleTrailRenderer.TrailStyle GetTrailStyle()
            => CalamityStyleTrailRenderer.TrailStyle.Cosmic;

        protected override string GetSmearTexturePath(int comboStep)
        {
            return comboStep switch
            {
                0 => "MagnumOpus/Assets/VFX Asset Library/SlashArcs/SwordArcSmear",
                1 => "MagnumOpus/Assets/VFX Asset Library/SlashArcs/FlamingSwordArcSmear",
                2 => "MagnumOpus/Assets/VFX Asset Library/SlashArcs/FullCircleSwordArcSlash",
                _ => "MagnumOpus/Assets/VFX Asset Library/ImpactEffects/ImpactEllipse",
            };
        }

        #endregion

        #region Virtual Overrides

        protected override Texture2D GetBladeTexture()
        {
            if (ModContent.HasAsset("MagnumOpus/Content/EnigmaVariations/ResonantWeapons/TheUnresolvedCadence/TheUnresolvedCadence"))
                return ModContent.Request<Texture2D>("MagnumOpus/Content/EnigmaVariations/ResonantWeapons/TheUnresolvedCadence/TheUnresolvedCadence", AssetRequestMode.ImmediateLoad).Value;
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

            // Phase 0 at 55% 窶・no gameplay effect (was VFX only)
            if (ComboStep == 0 && Progression >= 0.55f)
            {
                hasSpawnedSpecial = true;
            }

            // Phase 1 at 60% 窶・dimensional slash sub-projectile
            if (ComboStep == 1 && Progression >= 0.60f)
            {
                hasSpawnedSpecial = true;

                if (Main.myPlayer == Projectile.owner)
                {
                    Vector2 tipPos = GetBladeTipPosition();
                    Vector2 slashVel = SwordDirection * 12f;
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), tipPos, slashVel,
                        ModContent.ProjectileType<DimensionalSlash>(),
                        (int)(Projectile.damage * 0.35f), 2f, Projectile.owner,
                        ai0: Projectile.rotation);
                }
            }

            // Phase 2 at 50% 窶・massive dimensional tear (finisher)
            if (ComboStep == 2 && Progression >= 0.50f)
            {
                hasSpawnedSpecial = true;

                if (Main.myPlayer == Projectile.owner)
                {
                    Vector2 tipPos = GetBladeTipPosition();

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
            }
        }

        #endregion

        #region On Hit

        protected override void OnSwingHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // ParadoxBrand debuff 窶・8 second duration per doc
            target.AddBuff(ModContent.BuffType<ParadoxBrand>(), 480);
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

            Lighting.AddLight(target.Center, EnigmaPurple.ToVector3() * 0.9f);

            // Impact VFX
            if (!Main.dedServ)
            {
                CadenceParticleHandler.Spawn(new ParadoxSlashRipple(
                    target.Center, CadenceUtils.CadenceViolet, 0.25f, 25));
                for (int j = 0; j < Main.rand.Next(3, 6); j++)
                {
                    Vector2 burstVel = Main.rand.NextVector2CircularEdge(4f, 4f) * Main.rand.NextFloat(0.5f, 1.5f);
                    CadenceParticleHandler.Spawn(new DimensionalRiftMote(
                        target.Center, burstVel, Main.rand.NextFloat(0.2f, 0.4f), Main.rand.Next(12, 22)));
                }
                // Inevitability glyph on stack increment
                float glyphAngle = Main.rand.NextFloat(MathHelper.TwoPi);
                CadenceParticleHandler.Spawn(new InevitabilityGlyphParticle(
                    target.Center, 35f, glyphAngle,
                    TheUnresolvedCadenceItem.GetInevitabilityStacks(),
                    CadenceUtils.CadenceViolet, 0.35f, 30));
            }
        }

        #endregion

        #region Custom VFX

        protected override void DrawCustomVFX(SpriteBatch sb)
        {
            if (Main.dedServ) return;

            Vector2 tipPos = GetBladeTipPosition();
            int timer = (int)Timer;
            int stacks = TheUnresolvedCadenceItem.GetInevitabilityStacks();
            float stackIntensity = stacks / 10f; // 0..1

            // Base particles for ALL phases: 1-2 DimensionalRiftMotes per frame at swing tip
            for (int i = 0; i < Main.rand.Next(1, 3); i++)
            {
                Vector2 offset = Main.rand.NextVector2Circular(8f, 8f);
                Vector2 vel = SwordDirection.RotatedBy(MathHelper.PiOver2 * Direction) * Main.rand.NextFloat(0.5f, 2f);
                CadenceParticleHandler.Spawn(new DimensionalRiftMote(
                    tipPos + offset, vel, Main.rand.NextFloat(0.15f, 0.35f), Main.rand.Next(15, 25)));
            }

            // === Glitch Aesthetic: rectangular glitch artifacts near blade ===
            if (timer % 2 == 0 && Main.rand.NextBool(3))
            {
                // Small rectangular "glitch" fragments 窶・Enigma's digital tear reality
                Vector2 glitchPos = Vector2.Lerp(Owner.MountedCenter, tipPos, Main.rand.NextFloat(0.3f, 1f));
                glitchPos += Main.rand.NextVector2Circular(12f, 12f);
                Color glitchColor = Main.rand.NextBool() ? CadenceUtils.DimensionalGreen : CadenceUtils.CadenceViolet;
                CadenceParticleHandler.Spawn(new VoidCleaveParticle(
                    glitchPos, Vector2.Zero, glitchColor * 0.9f, Main.rand.NextFloat(0.15f, 0.3f), Main.rand.Next(3, 6)));
            }

            // Phase 0 (The Question): VoidCleaveParticle every 3 frames at swing tip
            if (ComboStep == 0 && timer % 3 == 0)
            {
                Vector2 vel = SwordDirection * Main.rand.NextFloat(2f, 5f);
                CadenceParticleHandler.Spawn(new VoidCleaveParticle(
                    tipPos, vel, CadenceUtils.CadenceViolet, Main.rand.NextFloat(0.3f, 0.6f), Main.rand.Next(12, 20)));
            }

            // Phase 1 (The Doubt): Two perpendicular particle streams for X-pattern feel
            if (ComboStep == 1 && timer % 2 == 0)
            {
                for (int side = -1; side <= 1; side += 2)
                {
                    Vector2 crossVel = SwordDirection.RotatedBy(MathHelper.PiOver4 * side) * Main.rand.NextFloat(3f, 6f);
                    CadenceParticleHandler.Spawn(new DimensionalRiftMote(
                        tipPos + Main.rand.NextVector2Circular(6f, 6f), crossVel,
                        Main.rand.NextFloat(0.2f, 0.4f), Main.rand.Next(10, 18)));
                }
            }

            // Phase 2 (The Silence): Heavy ParadoxSlashRipple every 2 frames at swing tip
            if (ComboStep >= 2 && timer % 2 == 0)
            {
                Color rippleColor = Main.rand.NextBool() ? CadenceUtils.SeveranceLime : CadenceUtils.DimensionalGreen;
                CadenceParticleHandler.Spawn(new ParadoxSlashRipple(
                    tipPos, rippleColor, Main.rand.NextFloat(0.2f, 0.4f), Main.rand.Next(20, 35)));
            }

            // === Inevitability escalation VFX: more particles as stacks grow ===
            if (stacks >= 3 && timer % 4 == 0)
            {
                // Eerie green motes orbiting around the blade 窶・more as stacks increase
                int extraMotes = (int)(stacks * 0.3f);
                for (int i = 0; i < extraMotes; i++)
                {
                    Vector2 orbitOffset = Main.rand.NextVector2Circular(20f + stacks * 3f, 20f + stacks * 3f);
                    CadenceParticleHandler.Spawn(new DimensionalRiftMote(
                        tipPos + orbitOffset, -orbitOffset * 0.05f,
                        Main.rand.NextFloat(0.1f, 0.25f), Main.rand.Next(20, 35)));
                }
            }

            // === InevitabilityGlyph at high stacks: eye-like glyph flickers near blade ===
            if (stacks >= 7 && timer % 6 == 0)
            {
                float glyphAngle = Main.rand.NextFloat(MathHelper.TwoPi);
                CadenceParticleHandler.Spawn(new InevitabilityGlyphParticle(
                    tipPos, 30f, glyphAngle, stacks,
                    CadenceUtils.DimensionalGreen, 0.3f, 20));
            }

            // CadenceRiftDust every 5 frames
            if (timer % 5 == 0)
            {
                Vector2 dustVel = SwordDirection.RotatedByRandom(0.5) * Main.rand.NextFloat(1f, 3f);
                Dust.NewDust(tipPos, 0, 0, ModContent.DustType<CadenceRiftDust>(),
                    dustVel.X, dustVel.Y);
            }

            // Attack burst at peak swing: 3-6 VoidCleaveParticles along the arc + impact ripple
            int attackFrame = (int)(SwingTime * 0.5f);
            if (timer == attackFrame)
            {
                int burstCount = Main.rand.Next(3, 7);
                for (int i = 0; i < burstCount; i++)
                {
                    float arcOffset = MathHelper.Lerp(-0.4f, 0.4f, i / (float)Math.Max(burstCount - 1, 1));
                    Vector2 burstDir = SwordDirection.RotatedBy(arcOffset * Direction);
                    Vector2 burstPos = Owner.MountedCenter + burstDir * CurrentPhase.BladeLength * Main.rand.NextFloat(0.5f, 1f);
                    Vector2 burstVel = burstDir * Main.rand.NextFloat(3f, 7f);
                    CadenceParticleHandler.Spawn(new VoidCleaveParticle(
                        burstPos, burstVel, CadenceUtils.CadenceViolet, Main.rand.NextFloat(0.4f, 0.8f), Main.rand.Next(15, 25)));
                }

                // Slash impact ripple at tip
                CadenceParticleHandler.Spawn(new ParadoxSlashRipple(
                    tipPos, CadenceUtils.SeveranceLime, 0.35f + ComboStep * 0.1f, 25));
            }
        }

        #endregion
    }
}