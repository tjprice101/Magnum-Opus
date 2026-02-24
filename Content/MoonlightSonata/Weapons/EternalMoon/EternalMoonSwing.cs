using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.BaseClasses;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.VFX.Trails;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Content.MoonlightSonata;
using MagnumOpus.Content.MoonlightSonata.Projectiles;
using MagnumOpus.Content.MoonlightSonata.Debuffs;
using static MagnumOpus.Common.Systems.Particles.Particle;

namespace MagnumOpus.Content.MoonlightSonata.Weapons
{
    /// <summary>
    /// Swing projectile for EternalMoon — a moonlight-themed sword with 3-phase combo.
    /// Phase 0: Lunar Sweep — smooth horizontal slash, spawns 3 EternalMoonWave
    /// Phase 1: Crescent Backhand — quick reverse arc, spawns 3 EternalMoonWave
    /// Phase 2: Crescendo Finale — massive overhead slam, spawns 3 EternalMoonWave + 3 EternalMoonBeam
    /// </summary>
    public sealed class EternalMoonSwing : MeleeSwingBase
    {
        #region Theme Colors

        private static readonly Color DarkPurple = MagnumThemePalettes.MoonlightDarkPurple;
        private static readonly Color MediumPurple = MagnumThemePalettes.MoonlightViolet;
        private static readonly Color LightBlue = MagnumThemePalettes.MoonlightIceBlue;
        private static readonly Color Silver = MagnumThemePalettes.MoonlightSilver;
        private static readonly Color Lavender = MagnumThemePalettes.MoonlightWeaponLavender;
        private static readonly Color LightPurple = MagnumThemePalettes.MoonlightLightPurple;

        private static readonly Color[] MoonlightPalette = new Color[]
        {
            DarkPurple,     // [0] Pianissimo — shadows, trail end
            MediumPurple,   // [1] Piano — outer glow
            LightBlue,      // [2] Mezzo — main body
            Silver,         // [3] Forte — bright areas
            Lavender,       // [4] Fortissimo — inner glow
            LightPurple     // [5] Sforzando — core, flare center
        };

        #endregion

        #region Combo Phases

        // Phase 0: Lunar Sweep — smooth horizontal slash
        private static readonly CurveSegment[] LunarSweepCurves = new CurveSegment[]
        {
            new CurveSegment(EasingType.PolyOut, 0f, -0.9f, 0.2f, 2),
            new CurveSegment(EasingType.PolyIn, 0.2f, -0.7f, 1.5f, 3),
            new CurveSegment(EasingType.PolyOut, 0.8f, 0.8f, 0.15f, 2)
        };

        // Phase 1: Crescent Backhand — quick reverse arc
        private static readonly CurveSegment[] CrescentBackhandCurves = new CurveSegment[]
        {
            new CurveSegment(EasingType.PolyOut, 0f, 0.8f, -0.15f, 2),
            new CurveSegment(EasingType.PolyIn, 0.15f, 0.65f, -1.55f, 3),
            new CurveSegment(EasingType.PolyOut, 0.75f, -0.9f, -0.1f, 2)
        };

        // Phase 2: Crescendo Finale — massive overhead slam
        private static readonly CurveSegment[] CrescendoFinaleCurves = new CurveSegment[]
        {
            new CurveSegment(EasingType.PolyOut, 0f, -1.1f, 0.3f, 2),
            new CurveSegment(EasingType.PolyIn, 0.3f, -0.8f, 2.0f, 4),
            new CurveSegment(EasingType.PolyOut, 0.85f, 1.2f, 0.05f, 2)
        };

        private static readonly ComboPhase Phase0_LunarSweep = new ComboPhase(
            curves: LunarSweepCurves,
            maxAngle: MathHelper.PiOver2 * 1.5f,
            duration: 30,
            bladeLength: 155f,
            flip: false,
            squish: 0.92f,
            damageMult: 0.9f
        );

        private static readonly ComboPhase Phase1_CrescentBackhand = new ComboPhase(
            curves: CrescentBackhandCurves,
            maxAngle: MathHelper.PiOver2 * 1.4f,
            duration: 26,
            bladeLength: 150f,
            flip: true,
            squish: 0.90f,
            damageMult: 1.0f
        );

        private static readonly ComboPhase Phase2_CrescendoFinale = new ComboPhase(
            curves: CrescendoFinaleCurves,
            maxAngle: MathHelper.PiOver2 * 2.0f,
            duration: 36,
            bladeLength: 165f,
            flip: false,
            squish: 0.85f,
            damageMult: 1.4f
        );

        #endregion

        #region Abstract Overrides

        protected override ComboPhase[] GetAllPhases() => new ComboPhase[]
        {
            Phase0_LunarSweep,
            Phase1_CrescentBackhand,
            Phase2_CrescendoFinale
        };

        protected override Color[] GetPalette() => MoonlightPalette;

        protected override CalamityStyleTrailRenderer.TrailStyle GetTrailStyle()
            => CalamityStyleTrailRenderer.TrailStyle.Cosmic;

        protected override string GetSmearTexturePath(int comboStep)
        {
            return comboStep switch
            {
                2 => "MagnumOpus/Assets/Particles/CurvedSwordSlash",
                1 => "MagnumOpus/Assets/Particles/SwordArc3",
                _ => "MagnumOpus/Assets/Particles/SwordArc2"
            };
        }

        #endregion

        #region Virtual Overrides

        protected override SoundStyle GetSwingSound()
            => SoundID.Item71 with { Pitch = -0.2f + ComboStep * 0.12f, Volume = 0.85f };

        protected override int GetInitialDustType() => DustID.PurpleTorch;

        protected override int GetSecondaryDustType() => DustID.Enchanted_Pink;

        protected override Texture2D GetBladeTexture()
            => ModContent.Request<Texture2D>("MagnumOpus/Content/MoonlightSonata/Weapons/EternalMoon").Value;

        protected override Vector3 GetLightColor()
        {
            float intensity = 0.6f + ComboStep * 0.15f;
            float pulse = 1f + MathF.Sin(Main.GlobalTimeWrappedHourly * 4f) * 0.1f;
            return MediumPurple.ToVector3() * intensity * pulse;
        }

        #endregion

        #region Combo Specials

        protected override void HandleComboSpecials()
        {
            if (hasSpawnedSpecial) return;

            // All phases: Spawn 3 EternalMoonWave at ~65% progress
            if (Progression >= 0.65f)
            {
                hasSpawnedSpecial = true;

                if (Main.myPlayer == Projectile.owner)
                {
                    Vector2 tipPos = GetBladeTipPosition();
                    float baseAngle = SwordDirection.ToRotation();

                    for (int i = -1; i <= 1; i++)
                    {
                        float spreadAngle = baseAngle + MathHelper.ToRadians(15f * i);
                        Vector2 vel = spreadAngle.ToRotationVector2() * 14f;
                        Projectile.NewProjectile(
                            Projectile.GetSource_FromThis(),
                            tipPos,
                            vel,
                            ModContent.ProjectileType<EternalMoonWave>(),
                            Projectile.damage / 2,
                            Projectile.knockBack * 0.5f,
                            Projectile.owner
                        );
                    }

                    // Phase 2 (Crescendo): Also spawn 3 EternalMoonBeam star projectiles
                    if (ComboStep == 2)
                    {
                        for (int i = -1; i <= 1; i++)
                        {
                            float beamAngle = baseAngle + MathHelper.ToRadians(20f * i);
                            Vector2 beamVel = beamAngle.ToRotationVector2() * 10f;
                            Projectile.NewProjectile(
                                Projectile.GetSource_FromThis(),
                                tipPos,
                                beamVel,
                                ModContent.ProjectileType<EternalMoonBeam>(),
                                (int)(Projectile.damage * 1.5f),
                                Projectile.knockBack * 0.3f,
                                Projectile.owner
                            );
                        }

                        // Crescendo finale VFX — full library treatment
                        MoonlightVFXLibrary.FinisherSlam(tipPos);
                    }
                }
            }
        }

        #endregion

        #region Hit Effects

        protected override void OnSwingHitNPC(NPC target, NPC.HitInfo hit, int remainingDamageCount)
        {
            // Apply Musical Dissonance debuff
            target.AddBuff(ModContent.BuffType<MusicsDissonance>(), 180);

            // Seeking moonlight crystals on crit
            if (hit.Crit && Main.myPlayer == Projectile.owner)
            {
                SeekingCrystalHelper.SpawnMoonlightCrystals(
                    Projectile.GetSource_FromThis(),
                    target.Center,
                    (target.Center - Owner.Center).SafeNormalize(Vector2.UnitY) * 8f,
                    (int)(Projectile.damage * 0.22f),
                    Projectile.knockBack * 0.2f,
                    Projectile.owner,
                    5
                );
            }

            // Moonlight impact VFX — unified library treatment
            MoonlightVFXLibrary.MeleeImpact(target.Center, ComboStep);
        }

        #endregion

        #region Custom VFX

        protected override void DrawCustomVFX(SpriteBatch sb)
        {
            // Moonlight shimmer particles during swing
            if (Progression > 0.08f && Progression < 0.92f)
            {
                Vector2 tipPos = GetBladeTipPosition();

                // Unified swing-frame VFX — shimmer, sparkles, music notes, lighting
                MoonlightVFXLibrary.SwingFrameVFX(tipPos, SwordDirection, ComboStep, Projectile.timeLeft, GetInitialDustType());

                // Blade-tip bloom — moonlit glow (combo-aware)
                {
                    float bloomOpacity = MathHelper.Clamp((Progression - 0.08f) / 0.12f, 0f, 1f)
                                       * MathHelper.Clamp((0.92f - Progression) / 0.12f, 0f, 1f);
                    MoonlightVFXLibrary.DrawComboBloom(tipPos, ComboStep, 0.4f, bloomOpacity);
                }
            }
        }

        #endregion
    }
}
