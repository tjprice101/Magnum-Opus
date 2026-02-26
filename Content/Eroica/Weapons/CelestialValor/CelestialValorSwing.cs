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
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.VFX.Trails;
using MagnumOpus.Content.Eroica;
using MagnumOpus.Content.MoonlightSonata.Debuffs;
using static MagnumOpus.Common.Systems.Particles.Particle;

namespace MagnumOpus.Content.Eroica.Weapons.CelestialValor
{
    /// <summary>
    /// Swing projectile for Celestial Valor  E"The Hero's Burning Oath".
    /// 
    /// 3-phase escalating combo that crescendos from a controlled opening slash
    /// to a roaring finisher. Each phase has distinct visual energy and particle
    /// choreography that builds tension like an orchestral movement.
    /// 
    /// Phase 0  E"Valor's Whisper": Swift controlled slash (1 projectile)
    /// Phase 1  E"Crimson Declaration": Powerful backhand (2 projectiles, spread)
    /// Phase 2  E"Heroic Finale": Massive overhead slam (3 projectiles, fan + finisher VFX)
    /// 
    /// Each swing integrates with the flame trail ring buffer in CelestialValorVFX
    /// for afterimage rendering and standing-wave flame resonance along the blade.
    /// </summary>
    public sealed class CelestialValorSwing : MeleeSwingBase
    {
        #region Theme Colors (via EroicaPalette)

        private static Color EroicaScarlet => EroicaPalette.Scarlet;
        private static Color EroicaCrimson => EroicaPalette.BladeCrimson;
        private static Color EroicaGold => EroicaPalette.Gold;
        private static Color SakuraPink => EroicaPalette.Sakura;
        private static Color[] SwingPalette => EroicaPalette.CelestialValorBlade;

        #endregion

        #region Combo Phases  ERefined easing for dramatic impact

        // Phase 0: Valor's Whisper  Eswift controlled opener
        // Easing: quick windup ↁEsharp acceleration ↁEsoft follow-through
        private static readonly ComboPhase Phase0_ValorSlash = new ComboPhase(
            curves: new CurveSegment[]
            {
                new CurveSegment(EasingType.PolyOut, 0f, -0.85f, 0.18f, 2),   // Short windup
                new CurveSegment(EasingType.PolyIn, 0.18f, -0.67f, 1.52f, 3), // Sharp swing
                new CurveSegment(EasingType.PolyOut, 0.80f, 0.85f, 0.12f, 2)  // Soft follow-through
            },
            maxAngle: MathHelper.PiOver2 * 1.5f,
            duration: 26,
            bladeLength: 155f,
            flip: false,
            squish: 0.92f,
            damageMult: 0.9f
        );

        // Phase 1: Crimson Declaration  Epowerful reverse slash
        // Easing: dramatic windup ↁEexplosive release ↁEweighted decel
        private static readonly ComboPhase Phase1_CrimsonCross = new ComboPhase(
            curves: new CurveSegment[]
            {
                new CurveSegment(EasingType.PolyOut, 0f, -1.05f, 0.25f, 2),   // Dramatic pull-back
                new CurveSegment(EasingType.PolyIn, 0.25f, -0.80f, 1.85f, 3), // Explosive swing
                new CurveSegment(EasingType.PolyOut, 0.84f, 1.05f, 0.08f, 2)  // Weighted deceleration
            },
            maxAngle: MathHelper.PiOver2 * 1.9f,
            duration: 30,
            bladeLength: 162f,
            flip: true,
            squish: 0.86f,
            damageMult: 1.15f
        );

        // Phase 2: Heroic Finale  Emassive overhead slam
        // Easing: long dramatic raise ↁEviolent downswing with power-4 accel ↁEabrupt stop
        private static readonly ComboPhase Phase2_HeroicFinale = new ComboPhase(
            curves: new CurveSegment[]
            {
                new CurveSegment(EasingType.SineOut, 0f, -1.15f, 0.16f, 2),    // Dramatic raise
                new CurveSegment(EasingType.PolyIn, 0.16f, -0.99f, 2.24f, 4),  // Violent slam 
                new CurveSegment(EasingType.PolyOut, 0.78f, 1.25f, 0.03f, 2)   // Abrupt halt = power
            },
            maxAngle: MathHelper.PiOver2 * 2.3f,
            duration: 36,
            bladeLength: 175f,
            flip: false,
            squish: 0.80f,
            damageMult: 1.55f
        );

        #endregion

        #region State tracking

        private int _lastComboStep = -1;

        #endregion

        #region Abstract Overrides

        protected override ComboPhase[] GetAllPhases() => new ComboPhase[]
        {
            Phase0_ValorSlash,
            Phase1_CrimsonCross,
            Phase2_HeroicFinale
        };

        protected override Color[] GetPalette() => SwingPalette;

        protected override CalamityStyleTrailRenderer.TrailStyle GetTrailStyle()
            => CalamityStyleTrailRenderer.TrailStyle.Flame;

        protected override string GetSmearTexturePath(int comboStep) => comboStep switch
        {
            1 => "MagnumOpus/Assets/Particles Asset Library/SwordArc3",
            2 => "MagnumOpus/Assets/Particles Asset Library/FlamingArcSwordSlash",
            _ => "MagnumOpus/Assets/Particles Asset Library/SwordArc2"
        };

        #endregion

        #region Virtual Overrides  EEnhanced

        public override string Texture => "MagnumOpus/Content/Eroica/Weapons/CelestialValor/CelestialValor_Swing";

        protected override Texture2D GetBladeTexture()
            => ModContent.Request<Texture2D>("MagnumOpus/Content/Eroica/Weapons/CelestialValor/CelestialValor").Value;

        protected override SoundStyle GetSwingSound()
        {
            // Escalating sound: deeper and louder with each combo step
            return SoundID.Item71 with
            {
                Pitch = -0.25f + ComboStep * 0.22f,
                Volume = 0.85f + ComboStep * 0.08f
            };
        }

        protected override int GetInitialDustType() => DustID.GoldFlame;

        protected override int GetSecondaryDustType() => DustID.RedTorch;

        protected override Vector3 GetLightColor()
        {
            // Light color shifts from scarlet on windup to gold-white at impact peak
            float intensity = 0.65f + ComboStep * 0.18f;
            float peakBoost = MathHelper.Clamp((Progression - 0.3f) / 0.4f, 0f, 1f)
                            * MathHelper.Clamp((0.85f - Progression) / 0.15f, 0f, 1f);
            Color c = Color.Lerp(EroicaScarlet, EroicaGold, Progression * 0.7f + peakBoost * 0.3f);
            return c.ToVector3() * (intensity + peakBoost * 0.3f);
        }

        #endregion

        #region Combo Specials  EEscalating Projectiles + Phase Transitions

        protected override void HandleComboSpecials()
        {
            // Track phase transitions for burst VFX
            if (ComboStep != _lastComboStep)
            {
                if (_lastComboStep >= 0)
                {
                    CelestialValorVFX.PhaseTransitionBurst(Owner.MountedCenter, ComboStep);
                }
                CelestialValorVFX.ResetFlameTrail();
                _lastComboStep = ComboStep;
            }

            if (hasSpawnedSpecial) return;

            // ── Phase 0 (Valor's Whisper): 1 projectile at 55% ──
            if (ComboStep == 0 && Progression >= 0.55f)
            {
                hasSpawnedSpecial = true;
                if (Main.myPlayer == Projectile.owner)
                {
                    Vector2 tipPos = GetBladeTipPosition();
                    Vector2 dir = SwordDirection;

                    Projectile.NewProjectile(
                        Projectile.GetSource_FromThis(), tipPos,
                        dir * 14f,
                        ModContent.ProjectileType<CelestialValorProjectile>(),
                        (int)(Projectile.damage * 0.88f), 3f, Projectile.owner);
                }

                CelestialValorVFX.SwingPhase0VFX(GetBladeTipPosition(), SwordDirection);
            }

            // ── Phase 1 (Crimson Declaration): 2 projectiles at 50% with spread ──
            if (ComboStep == 1 && Progression >= 0.50f)
            {
                hasSpawnedSpecial = true;
                if (Main.myPlayer == Projectile.owner)
                {
                    Vector2 tipPos = GetBladeTipPosition();
                    Vector2 dir = SwordDirection;
                    float spreadAngle = MathHelper.ToRadians(9f);

                    for (int i = -1; i <= 1; i += 2)
                    {
                        Vector2 vel = dir.RotatedBy(spreadAngle * i) * 14.5f;
                        Projectile.NewProjectile(
                            Projectile.GetSource_FromThis(), tipPos,
                            vel,
                            ModContent.ProjectileType<CelestialValorProjectile>(),
                            (int)(Projectile.damage * 0.88f), 3f, Projectile.owner);
                    }
                }

                CelestialValorVFX.SwingPhase1VFX(GetBladeTipPosition(), SwordDirection);
            }

            // ── Phase 2 (Heroic Finale): 3 projectiles at 58% with fan spread ──
            if (ComboStep == 2 && Progression >= 0.58f)
            {
                hasSpawnedSpecial = true;
                if (Main.myPlayer == Projectile.owner)
                {
                    Vector2 tipPos = GetBladeTipPosition();
                    Vector2 dir = SwordDirection;
                    float spreadAngle = MathHelper.ToRadians(13f);

                    for (int i = -1; i <= 1; i++)
                    {
                        Vector2 vel = dir.RotatedBy(spreadAngle * i) * 15.5f;
                        Projectile.NewProjectile(
                            Projectile.GetSource_FromThis(), tipPos,
                            vel,
                            ModContent.ProjectileType<CelestialValorProjectile>(),
                            (int)(Projectile.damage * 0.95f), 4.5f, Projectile.owner);
                    }
                }

                // Full heroic finale VFX cascade
                CelestialValorVFX.SwingPhase2VFX(GetBladeTipPosition());

                // Extra: heroic stomp sound for weight
                SoundEngine.PlaySound(SoundID.Item70 with { Pitch = -0.4f, Volume = 0.5f },
                    GetBladeTipPosition());
            }
        }

        #endregion

        #region On Hit  EMusicsDissonance + Seeking Crystals

        protected override void OnSwingHitNPC(NPC target, NPC.HitInfo hit, int remainingDamageCount)
        {
            // Apply MusicsDissonance debuff  Estandard across Eroica weapons (240 ticks)
            target.AddBuff(ModContent.BuffType<MusicsDissonance>(), 240);

            // Enhanced impact VFX
            CelestialValorVFX.SwingHitImpact(target.Center, ComboStep);

            // Spawn seeking crystals on critical hits  ECelestial Valor's signature
            if (hit.Crit && Main.myPlayer == Projectile.owner)
            {
                SeekingCrystalHelper.SpawnEroicaCrystals(
                    Projectile.GetSource_FromThis(),
                    target.Center,
                    (Main.MouseWorld - target.Center).SafeNormalize(Vector2.UnitX) * 8f,
                    (int)(Projectile.damage * 0.25f),
                    Projectile.knockBack * 0.5f,
                    Projectile.owner,
                    5);
            }
        }

        #endregion

        #region Custom VFX  EEnhanced Heroic Trail

        protected override void DrawCustomVFX(SpriteBatch sb)
        {
            // Full per-frame trail VFX with flame resonance, afterimages, music notes
            CelestialValorVFX.DrawSwingTrailVFX(
                GetBladeTipPosition(), Owner.MountedCenter,
                SwordDirection, Progression, ComboStep);
        }

        #endregion
    }
}
