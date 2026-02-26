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
using MagnumOpus.Content.Eroica.Projectiles;
using MagnumOpus.Content.MoonlightSonata.Debuffs;

namespace MagnumOpus.Content.Eroica.Weapons.SakurasBlossom
{
    /// <summary>
    /// Swing projectile for Sakura's Blossom — Eroica's blooming sword of spring.
    /// 4-phase sakura combo: Petal Slash → Crimson Scatter → Blossom Bloom → Storm of Petals.
    /// Each phase spawns increasing numbers of spectral homing copies (SakurasBlossomSpectral).
    /// The blade literally blooms with petals — a flower unfurling across four movements.
    /// 
    /// Enhanced: phase transition tracking, petal trail recording, escalating
    /// sound design with pitch/volume curve, peak-boost lighting.
    /// </summary>
    public sealed class SakurasBlossomSwing : MeleeSwingBase
    {
        #region Theme Colors

        // 6-color Sakura palette — bud to full bloom (delegates to EroicaPalette)
        private static readonly Color[] SakuraPalette = EroicaPalette.SakurasBlossomBlade;

        #endregion

        #region Phase Transition Tracking

        /// <summary>Track last combo step for phase transition VFX.</summary>
        private int _lastComboStep = -1;

        #endregion

        #region Combo Phases

        // Phase 0: Petal Slash — quick horizontal opener, petals scatter
        //   Refined: slightly tighter timing +  snappier acceleration
        private static readonly ComboPhase Phase0_PetalSlash = new ComboPhase(
            curves: new CurveSegment[]
            {
                new CurveSegment(EasingType.PolyOut, 0f, -0.85f, 0.16f, 2),
                new CurveSegment(EasingType.PolyIn, 0.16f, -0.69f, 1.50f, 3),
                new CurveSegment(EasingType.PolyOut, 0.78f, 0.81f, 0.10f, 2)
            },
            maxAngle: MathHelper.PiOver2 * 1.4f,
            duration: 26,
            bladeLength: 155f,
            flip: false,
            squish: 0.92f,
            damageMult: 0.85f
        );

        // Phase 1: Crimson Scatter — backhand that tosses spectral copies wide
        //   Refined: wider arc, slightly longer for drama
        private static readonly ComboPhase Phase1_CrimsonScatter = new ComboPhase(
            curves: new CurveSegment[]
            {
                new CurveSegment(EasingType.PolyOut, 0f, 0.92f, -0.20f, 2),
                new CurveSegment(EasingType.PolyIn, 0.20f, 0.72f, -1.65f, 3),
                new CurveSegment(EasingType.PolyOut, 0.82f, -0.93f, -0.08f, 2)
            },
            maxAngle: MathHelper.PiOver2 * 1.65f,
            duration: 29,
            bladeLength: 160f,
            flip: true,
            squish: 0.89f,
            damageMult: 1.0f
        );

        // Phase 2: Blossom Bloom — rising arc, pollen explodes from blade
        //   Refined: broader bloom arc, longer blade
        private static readonly ComboPhase Phase2_BlossomBloom = new ComboPhase(
            curves: new CurveSegment[]
            {
                new CurveSegment(EasingType.SineOut, 0f, -1.0f, 0.22f, 2),
                new CurveSegment(EasingType.PolyIn, 0.22f, -0.78f, 1.85f, 3),
                new CurveSegment(EasingType.PolyOut, 0.84f, 1.07f, 0.06f, 2)
            },
            maxAngle: MathHelper.PiOver2 * 1.95f,
            duration: 33,
            bladeLength: 168f,
            flip: false,
            squish: 0.85f,
            damageMult: 1.18f
        );

        // Phase 3: Storm of Petals — massive slam, sakura storm erupts
        //   Refined: power-4 acceleration for devastating finisher, longest blade
        private static readonly ComboPhase Phase3_StormOfPetals = new ComboPhase(
            curves: new CurveSegment[]
            {
                new CurveSegment(EasingType.SineOut, 0f, -1.18f, 0.16f, 2),
                new CurveSegment(EasingType.PolyIn, 0.18f, -1.02f, 2.30f, 4),
                new CurveSegment(EasingType.PolyOut, 0.82f, 1.28f, 0.04f, 2)
            },
            maxAngle: MathHelper.PiOver2 * 2.35f,
            duration: 42,
            bladeLength: 178f,
            flip: true,
            squish: 0.78f,
            damageMult: 1.55f
        );

        #endregion

        #region Abstract Overrides

        protected override ComboPhase[] GetAllPhases() => new ComboPhase[]
        {
            Phase0_PetalSlash,
            Phase1_CrimsonScatter,
            Phase2_BlossomBloom,
            Phase3_StormOfPetals
        };

        protected override Color[] GetPalette() => SakuraPalette;

        protected override CalamityStyleTrailRenderer.TrailStyle GetTrailStyle()
            => CalamityStyleTrailRenderer.TrailStyle.Flame;

        protected override string GetSmearTexturePath(int comboStep) => comboStep switch
        {
            1 => "MagnumOpus/Assets/Particles/SwordArc3",
            2 => "MagnumOpus/Assets/Particles/CurvedSwordSlash",
            3 => "MagnumOpus/Assets/Particles/FlamingArcSwordSlash",
            _ => "MagnumOpus/Assets/Particles/SwordArc2"
        };

        #endregion

        #region Virtual Overrides

        protected override Texture2D GetBladeTexture()
            => ModContent.Request<Texture2D>("MagnumOpus/Content/Eroica/Weapons/SakurasBlossom/SakurasBlossom").Value;

        /// <summary>
        /// Escalating sakura sound — pitch rises through combo, volume grows,
        /// Phase 3 finisher gets extra reverb-like pitch drop.
        /// </summary>
        protected override SoundStyle GetSwingSound()
            => SoundID.Item71 with
            {
                Pitch = -0.22f + ComboStep * 0.18f,
                Volume = 0.88f + ComboStep * 0.06f
            };

        protected override int GetInitialDustType() => DustID.RedTorch;

        protected override int GetSecondaryDustType() => DustID.PinkTorch;

        /// <summary>
        /// Enhanced light color with progression-based peak boost.
        /// Pink → Gold gradient intensifies at swing peak (progression ~0.5).
        /// </summary>
        protected override Vector3 GetLightColor()
        {
            float baseIntensity = 0.55f + ComboStep * 0.14f;
            // Peak boost: sine curve peaking at mid-swing
            float peakFactor = (float)Math.Sin(Progression * MathHelper.Pi);
            float intensity = baseIntensity + peakFactor * 0.25f;
            Color c = Color.Lerp(EroicaPalette.Sakura, EroicaPalette.Gold, Progression);
            return c.ToVector3() * intensity;
        }

        #endregion

        #region Combo Specials — Escalating Spectral Copies

        protected override void HandleComboSpecials()
        {
            // Phase transition detection — petal burst between phases
            if (ComboStep != _lastComboStep)
            {
                if (_lastComboStep >= 0)
                {
                    SakurasBlossomVFX.PhaseTransitionBurst(Owner.MountedCenter, ComboStep);
                    SakurasBlossomVFX.ResetPetalTrail();
                }
                _lastComboStep = ComboStep;
            }

            if (hasSpawnedSpecial) return;

            // Phase 0 (Petal Slash): 1 spectral copy at 60%
            if (ComboStep == 0 && Progression >= 0.60f)
            {
                hasSpawnedSpecial = true;
                if (Main.myPlayer == Projectile.owner)
                {
                    Vector2 tipPos = GetBladeTipPosition();
                    Vector2 vel = SwordDirection * 15f;
                    Projectile.NewProjectile(
                        Projectile.GetSource_FromThis(), tipPos, vel,
                        ModContent.ProjectileType<SakurasBlossomSpectral>(),
                        (int)(Projectile.damage * 0.7f), 3f, Projectile.owner);
                }

                Vector2 vfxTip = GetBladeTipPosition();
                SakurasBlossomVFX.PetalSlashVFX(vfxTip, SwordDirection);
            }

            // Phase 1 (Crimson Scatter): 2 spectral copies at 55% with spread
            if (ComboStep == 1 && Progression >= 0.55f)
            {
                hasSpawnedSpecial = true;
                if (Main.myPlayer == Projectile.owner)
                {
                    Vector2 tipPos = GetBladeTipPosition();
                    float spread = MathHelper.ToRadians(25f);
                    for (int i = -1; i <= 1; i += 2)
                    {
                        Vector2 vel = SwordDirection.RotatedBy(spread * i) * 14f;
                        Projectile.NewProjectile(
                            Projectile.GetSource_FromThis(), tipPos, vel,
                            ModContent.ProjectileType<SakurasBlossomSpectral>(),
                            (int)(Projectile.damage * 0.7f), 3f, Projectile.owner);
                    }
                }

                Vector2 vfxTip = GetBladeTipPosition();
                SakurasBlossomVFX.CrimsonScatterVFX(vfxTip, SwordDirection);
            }

            // Phase 2 (Blossom Bloom): 3 spectral copies at 65% in fan
            if (ComboStep == 2 && Progression >= 0.65f)
            {
                hasSpawnedSpecial = true;
                if (Main.myPlayer == Projectile.owner)
                {
                    Vector2 tipPos = GetBladeTipPosition();
                    float spread = MathHelper.ToRadians(20f);
                    for (int i = -1; i <= 1; i++)
                    {
                        Vector2 vel = SwordDirection.RotatedBy(spread * i) * 15f;
                        Projectile.NewProjectile(
                            Projectile.GetSource_FromThis(), tipPos, vel,
                            ModContent.ProjectileType<SakurasBlossomSpectral>(),
                            (int)(Projectile.damage * 0.75f), 3f, Projectile.owner);
                    }
                }

                Vector2 vfxTip = GetBladeTipPosition();
                SakurasBlossomVFX.BlossomBloomVFX(vfxTip);
            }

            // Phase 3 (Storm of Petals): 4 spectral copies at 55% + spectacular VFX
            if (ComboStep == 3 && Progression >= 0.55f)
            {
                hasSpawnedSpecial = true;
                if (Main.myPlayer == Projectile.owner)
                {
                    Vector2 tipPos = GetBladeTipPosition();
                    float spread = MathHelper.ToRadians(18f);
                    for (int i = 0; i < 4; i++)
                    {
                        float angle = spread * (i - 1.5f);
                        Vector2 vel = SwordDirection.RotatedBy(angle) * 16f;
                        Projectile.NewProjectile(
                            Projectile.GetSource_FromThis(), tipPos, vel,
                            ModContent.ProjectileType<SakurasBlossomSpectral>(),
                            (int)(Projectile.damage * 0.8f), 4f, Projectile.owner);
                    }
                }

                // VFX: Full sakura storm — the climactic blooming
                Vector2 vfxTip = GetBladeTipPosition();
                SakurasBlossomVFX.StormOfPetalsVFX(vfxTip);

                // Finisher sound — low boom to complement the storm
                SoundEngine.PlaySound(SoundID.Item70 with { Pitch = -0.3f, Volume = 0.5f },
                    GetBladeTipPosition());
            }
        }

        #endregion

        #region On Hit — MusicsDissonance + Seeking Crystals

        protected override void OnSwingHitNPC(NPC target, NPC.HitInfo hit, int remainingDamageCount)
        {
            // Apply MusicsDissonance debuff
            target.AddBuff(ModContent.BuffType<MusicsDissonance>(), 240);

            // Impact VFX — delegated to VFX module
            SakurasBlossomVFX.SwingHitImpact(target.Center, ComboStep);

            // Seeking crystals on every third hit (33% chance)
            if (Main.rand.NextBool(3) && Main.myPlayer == Projectile.owner)
            {
                SeekingCrystalHelper.SpawnEroicaCrystals(
                    Projectile.GetSource_FromThis(),
                    target.Center,
                    (Main.MouseWorld - target.Center).SafeNormalize(Vector2.UnitX) * 8f,
                    (int)(Projectile.damage * 0.2f),
                    Projectile.knockBack * 0.4f,
                    Projectile.owner,
                    4);
            }
        }

        #endregion

        #region Custom VFX — Blooming Sakura Aura

        protected override void DrawCustomVFX(SpriteBatch sb)
        {
            if (Progression <= 0.06f || Progression >= 0.94f) return;

            Vector2 tipPos = GetBladeTipPosition();
            SakurasBlossomVFX.DrawSwingTrailVFX(tipPos, Owner.MountedCenter, SwordDirection, Progression, ComboStep);
        }

        #endregion
    }
}
