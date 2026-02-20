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
    /// Swing projectile for Celestial Valor — Eroica's heroic resonant blade.
    /// 3-phase escalating combo: Valor Slash → Crimson Cross → Heroic Finale.
    /// Each phase fires increasingly more CelestialValorProjectile slashes (1/2/3),
    /// matching the original swingCounter escalation pattern.
    /// </summary>
    public sealed class CelestialValorSwing : MeleeSwingBase
    {
        #region Theme Colors (via EroicaPalette)

        // Shorthand aliases → canonical EroicaPalette source of truth
        private static Color EroicaScarlet => EroicaPalette.Scarlet;
        private static Color EroicaCrimson => EroicaPalette.BladeCrimson;
        private static Color EroicaGold => EroicaPalette.Gold;
        private static Color SakuraPink => EroicaPalette.Sakura;

        // 6-color swing palette from canonical source
        private static Color[] SwingPalette => EroicaPalette.CelestialValorBlade;

        #endregion

        #region Combo Phases

        // Phase 0: Valor Slash — swift opening strike
        private static readonly ComboPhase Phase0_ValorSlash = new ComboPhase(
            curves: new CurveSegment[]
            {
                new CurveSegment(EasingType.PolyOut, 0f, -0.9f, 0.2f, 2),
                new CurveSegment(EasingType.PolyIn, 0.2f, -0.7f, 1.5f, 3),
                new CurveSegment(EasingType.PolyOut, 0.82f, 0.8f, 0.15f, 2)
            },
            maxAngle: MathHelper.PiOver2 * 1.5f,
            duration: 28,
            bladeLength: 155f,
            flip: false,
            squish: 0.92f,
            damageMult: 0.9f
        );

        // Phase 1: Crimson Cross — powerful backhand with wider arc
        private static readonly ComboPhase Phase1_CrimsonCross = new ComboPhase(
            curves: new CurveSegment[]
            {
                new CurveSegment(EasingType.PolyOut, 0f, -1.0f, 0.3f, 2),
                new CurveSegment(EasingType.PolyIn, 0.28f, -0.7f, 1.7f, 3),
                new CurveSegment(EasingType.PolyOut, 0.85f, 1.0f, 0.1f, 2)
            },
            maxAngle: MathHelper.PiOver2 * 1.8f,
            duration: 30,
            bladeLength: 160f,
            flip: true,
            squish: 0.88f,
            damageMult: 1.1f
        );

        // Phase 2: Heroic Finale — massive overhead slam with full spectacle
        private static readonly ComboPhase Phase2_HeroicFinale = new ComboPhase(
            curves: new CurveSegment[]
            {
                new CurveSegment(EasingType.SineOut, 0f, -1.1f, 0.15f, 2),
                new CurveSegment(EasingType.PolyIn, 0.18f, -0.95f, 2.1f, 4),
                new CurveSegment(EasingType.PolyOut, 0.8f, 1.15f, 0.05f, 2)
            },
            maxAngle: MathHelper.PiOver2 * 2.2f,
            duration: 38,
            bladeLength: 170f,
            flip: false,
            squish: 0.82f,
            damageMult: 1.5f
        );

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
            1 => "MagnumOpus/Assets/Particles/SwordArc3",
            2 => "MagnumOpus/Assets/Particles/FlamingArcSwordSlash",
            _ => "MagnumOpus/Assets/Particles/SwordArc2"
        };

        #endregion

        #region Virtual Overrides

        public override string Texture => "MagnumOpus/Content/Eroica/Weapons/CelestialValor/CelestialValor_Swing";

        protected override Texture2D GetBladeTexture()
            => ModContent.Request<Texture2D>("MagnumOpus/Content/Eroica/Weapons/CelestialValor/CelestialValor").Value;

        protected override SoundStyle GetSwingSound()
            => SoundID.Item71 with { Pitch = -0.2f + ComboStep * 0.2f, Volume = 0.9f };

        protected override int GetInitialDustType() => DustID.GoldFlame;

        protected override int GetSecondaryDustType() => DustID.RedTorch;

        protected override Vector3 GetLightColor()
        {
            float intensity = 0.6f + ComboStep * 0.15f;
            Color c = Color.Lerp(EroicaScarlet, EroicaGold, Progression);
            return c.ToVector3() * intensity;
        }

        #endregion

        #region Combo Specials — Escalating Projectile Count

        protected override void HandleComboSpecials()
        {
            if (hasSpawnedSpecial) return;

            // Phase 0 (Valor Slash): 1 projectile at 55% progress
            if (ComboStep == 0 && Progression >= 0.55f)
            {
                hasSpawnedSpecial = true;
                if (Main.myPlayer == Projectile.owner)
                {
                    Vector2 tipPos = GetBladeTipPosition();
                    Vector2 dir = SwordDirection;
                    float speed = 14f;

                    Projectile.NewProjectile(
                        Projectile.GetSource_FromThis(), tipPos,
                        dir * speed,
                        ModContent.ProjectileType<CelestialValorProjectile>(),
                        (int)(Projectile.damage * 0.88f), 3f, Projectile.owner);
                }

                // VFX: Single heroic slash burst
                CelestialValorVFX.SwingPhase0VFX(GetBladeTipPosition(), SwordDirection);
            }

            // Phase 1 (Crimson Cross): 2 projectiles at 50% progress with spread
            if (ComboStep == 1 && Progression >= 0.50f)
            {
                hasSpawnedSpecial = true;
                if (Main.myPlayer == Projectile.owner)
                {
                    Vector2 tipPos = GetBladeTipPosition();
                    Vector2 dir = SwordDirection;
                    float speed = 14f;
                    float spreadAngle = MathHelper.ToRadians(8f);

                    for (int i = -1; i <= 1; i += 2)
                    {
                        Vector2 vel = dir.RotatedBy(spreadAngle * i) * speed;
                        Projectile.NewProjectile(
                            Projectile.GetSource_FromThis(), tipPos,
                            vel,
                            ModContent.ProjectileType<CelestialValorProjectile>(),
                            (int)(Projectile.damage * 0.88f), 3f, Projectile.owner);
                    }
                }

                // VFX: Double helix slash effect
                CelestialValorVFX.SwingPhase1VFX(GetBladeTipPosition(), SwordDirection);
            }

            // Phase 2 (Heroic Finale): 3 projectiles at 60% progress with fan spread
            if (ComboStep == 2 && Progression >= 0.60f)
            {
                hasSpawnedSpecial = true;
                if (Main.myPlayer == Projectile.owner)
                {
                    Vector2 tipPos = GetBladeTipPosition();
                    Vector2 dir = SwordDirection;
                    float speed = 15f;
                    float spreadAngle = MathHelper.ToRadians(12f);

                    for (int i = -1; i <= 1; i++)
                    {
                        Vector2 vel = dir.RotatedBy(spreadAngle * i) * speed;
                        Projectile.NewProjectile(
                            Projectile.GetSource_FromThis(), tipPos,
                            vel,
                            ModContent.ProjectileType<CelestialValorProjectile>(),
                            (int)(Projectile.damage * 0.95f), 4f, Projectile.owner);
                    }
                }

                // VFX: Full heroic impact — the climactic finale
                CelestialValorVFX.SwingPhase2VFX(GetBladeTipPosition());
            }
        }

        #endregion

        #region On Hit — MusicsDissonance + Seeking Crystals

        protected override void OnSwingHitNPC(NPC target, NPC.HitInfo hit, int remainingDamageCount)
        {
            // Apply MusicsDissonance debuff — standard across Eroica weapons (240 ticks)
            target.AddBuff(ModContent.BuffType<MusicsDissonance>(), 240);

            // Impact VFX (consolidated in CelestialValorVFX)
            CelestialValorVFX.SwingHitImpact(target.Center, ComboStep);

            // Spawn seeking crystals on critical hits — Celestial Valor's signature
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

        #region Custom VFX — Heroic Aura

        protected override void DrawCustomVFX(SpriteBatch sb)
        {
            CelestialValorVFX.DrawSwingTrailVFX(
                GetBladeTipPosition(), Owner.MountedCenter,
                SwordDirection, Progression, ComboStep);
        }

        #endregion
    }
}
