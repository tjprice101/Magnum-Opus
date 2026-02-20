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
using MagnumOpus.Common.Systems.VFX.Trails;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Content.Nachtmusik;
using MagnumOpus.Content.Nachtmusik.Projectiles;
using MagnumOpus.Content.Nachtmusik.Debuffs;
using static MagnumOpus.Common.Systems.Particles.Particle;

namespace MagnumOpus.Content.Nachtmusik.ResonantWeapons
{
    /// <summary>
    /// Swing projectile for Nocturnal Executioner — heavy cosmic greatsword.
    /// 3-phase combo: Shadow Cleave → Cosmic Divide → Stellar Execution.
    /// Phase 2 fires NocturnalBladeProjectile fan, Phase 3 finale fires seeking crystals.
    /// </summary>
    public sealed class NocturnalExecutionerSwing : MeleeSwingBase
    {
        #region Theme Colors

        private static readonly Color DeepPurple = NachtmusikCosmicVFX.DeepPurple;
        private static readonly Color Gold = NachtmusikCosmicVFX.Gold;
        private static readonly Color Violet = NachtmusikCosmicVFX.Violet;
        private static readonly Color NightBlue = NachtmusikCosmicVFX.NightBlue;
        private static readonly Color StarWhite = NachtmusikCosmicVFX.StarWhite;

        private static readonly Color[] NachtmusikPalette = new Color[]
        {
            new Color(20, 12, 45),      // [0] Pianissimo — deep void
            new Color(45, 27, 78),      // [1] Piano — cosmic purple
            new Color(80, 60, 140),     // [2] Mezzo — violet body
            new Color(140, 100, 200),   // [3] Forte — nebula lavender
            new Color(200, 175, 80),    // [4] Fortissimo — starlit gold
            new Color(255, 240, 200)    // [5] Sforzando — pure starlight
        };

        #endregion

        #region Combo Phases

        // Phase 0: Shadow Cleave — swift downward diagonal
        private static readonly ComboPhase Phase0_ShadowCleave = new ComboPhase(
            curves: new CurveSegment[]
            {
                new CurveSegment(EasingType.PolyOut, 0f, -0.85f, 0.2f, 2),
                new CurveSegment(EasingType.PolyIn, 0.22f, -0.65f, 1.55f, 3),
                new CurveSegment(EasingType.PolyOut, 0.82f, 0.9f, 0.1f, 2)
            },
            maxAngle: MathHelper.PiOver2 * 1.5f,
            duration: 28,
            bladeLength: 160f,
            flip: false,
            squish: 0.9f,
            damageMult: 0.85f
        );

        // Phase 1: Cosmic Divide — wider arc backhand
        private static readonly ComboPhase Phase1_CosmicDivide = new ComboPhase(
            curves: new CurveSegment[]
            {
                new CurveSegment(EasingType.PolyOut, 0f, -1.0f, 0.25f, 2),
                new CurveSegment(EasingType.PolyIn, 0.25f, -0.75f, 1.75f, 3),
                new CurveSegment(EasingType.PolyOut, 0.85f, 1.0f, 0.1f, 2)
            },
            maxAngle: MathHelper.PiOver2 * 1.9f,
            duration: 32,
            bladeLength: 165f,
            flip: true,
            squish: 0.86f,
            damageMult: 1.1f
        );

        // Phase 2: Stellar Execution — massive overhead slam
        private static readonly ComboPhase Phase2_StellarExecution = new ComboPhase(
            curves: new CurveSegment[]
            {
                new CurveSegment(EasingType.SineOut, 0f, -1.1f, 0.15f, 2),
                new CurveSegment(EasingType.PolyIn, 0.18f, -0.95f, 2.15f, 4),
                new CurveSegment(EasingType.PolyOut, 0.82f, 1.2f, 0.05f, 2)
            },
            maxAngle: MathHelper.PiOver2 * 2.3f,
            duration: 40,
            bladeLength: 175f,
            flip: false,
            squish: 0.80f,
            damageMult: 1.5f
        );

        #endregion

        #region Abstract Overrides

        protected override ComboPhase[] GetAllPhases() => new ComboPhase[]
        {
            Phase0_ShadowCleave,
            Phase1_CosmicDivide,
            Phase2_StellarExecution
        };

        protected override Color[] GetPalette() => NachtmusikPalette;

        protected override CalamityStyleTrailRenderer.TrailStyle GetTrailStyle()
            => CalamityStyleTrailRenderer.TrailStyle.Cosmic;

        protected override string GetSmearTexturePath(int comboStep) => comboStep switch
        {
            1 => "MagnumOpus/Assets/Particles/SwordArc3",
            2 => "MagnumOpus/Assets/Particles/FlamingArcSwordSlash",
            _ => "MagnumOpus/Assets/Particles/SwordArc2"
        };

        #endregion

        #region Virtual Overrides

        protected override Texture2D GetBladeTexture()
            => ModContent.Request<Texture2D>("MagnumOpus/Content/Nachtmusik/ResonantWeapons/NocturnalExecutioner").Value;

        protected override SoundStyle GetSwingSound()
            => SoundID.Item71 with { Pitch = -0.3f + ComboStep * 0.2f, Volume = 0.9f };

        protected override int GetInitialDustType() => DustID.PurpleTorch;

        protected override int GetSecondaryDustType() => DustID.Enchanted_Gold;

        protected override Vector3 GetLightColor()
        {
            float intensity = 0.6f + ComboStep * 0.15f;
            Color c = Color.Lerp(DeepPurple, Gold, Progression);
            return c.ToVector3() * intensity;
        }

        #endregion

        #region Combo Specials

        protected override void HandleComboSpecials()
        {
            if (hasSpawnedSpecial) return;

            // Phase 0: Small VFX accent at 55%
            if (ComboStep == 0 && Progression >= 0.55f)
            {
                hasSpawnedSpecial = true;
                Vector2 tip = GetBladeTipPosition();
                CustomParticles.GenericFlare(tip, Gold, 0.5f, 14);
                CustomParticles.HaloRing(tip, Violet, 0.3f, 12);
                NachtmusikCosmicVFX.SpawnMusicNoteBurst(tip, 2, 15f);
            }

            // Phase 1: Fire 3-projectile fan at 50%
            if (ComboStep == 1 && Progression >= 0.50f)
            {
                hasSpawnedSpecial = true;
                if (Main.myPlayer == Projectile.owner)
                {
                    Vector2 tip = GetBladeTipPosition();
                    Vector2 dir = SwordDirection;
                    for (int i = -1; i <= 1; i++)
                    {
                        Vector2 vel = dir.RotatedBy(MathHelper.ToRadians(i * 10f)) * 14f;
                        Projectile.NewProjectile(Projectile.GetSource_FromThis(), tip, vel,
                            ModContent.ProjectileType<NocturnalBladeProjectile>(),
                            (int)(Projectile.damage * 0.4f), 3f, Projectile.owner);
                    }
                }

                Vector2 vfxTip = GetBladeTipPosition();
                CustomParticles.GenericFlare(vfxTip, StarWhite, 0.7f, 18);
                CustomParticles.GenericFlare(vfxTip, Violet, 0.5f, 16);
                CustomParticles.HaloRing(vfxTip, DeepPurple, 0.4f, 14);
                NachtmusikCosmicVFX.SpawnMusicNoteBurst(vfxTip, 4, 25f);
                NachtmusikCosmicVFX.SpawnShatteredStarlightBurst(vfxTip, 5, 4f, 0.6f, false);
            }

            // Phase 2: Grand finale at 65% — explosion + seeking crystals
            if (ComboStep == 2 && Progression >= 0.65f)
            {
                hasSpawnedSpecial = true;

                // Seeking crystals on Phase 2 from the blade tip
                if (Main.myPlayer == Projectile.owner)
                {
                    Vector2 tip = GetBladeTipPosition();
                    SeekingCrystalHelper.SpawnNachtmusikCrystals(
                        Projectile.GetSource_FromThis(), tip,
                        SwordDirection * 8f, (int)(Projectile.damage * 0.25f),
                        Projectile.knockBack * 0.5f, Projectile.owner, 5);
                }

                Vector2 vfxTip = GetBladeTipPosition();
                NachtmusikCosmicVFX.SpawnGrandCelestialImpact(vfxTip, 1.3f);
                NachtmusikCosmicVFX.SpawnStarBurstImpact(vfxTip, 1.2f, 4);
                for (int i = 0; i < 5; i++)
                {
                    float p = i / 5f;
                    Color ringColor = Color.Lerp(DeepPurple, Gold, p);
                    CustomParticles.HaloRing(vfxTip, ringColor, 0.35f + i * 0.12f, 14 + i * 2);
                }
                NachtmusikCosmicVFX.SpawnMusicNoteBurst(vfxTip, 6, 40f);

                MagnumScreenEffects.AddScreenShake(7f);
            }
        }

        #endregion

        #region On Hit — Celestial Harmony + Execution Charge

        protected override void OnSwingHitNPC(NPC target, NPC.HitInfo hit, int remainingDamageCount)
        {
            target.AddBuff(ModContent.BuffType<CelestialHarmony>(), 480);
            if (target.TryGetGlobalNPC(out CelestialHarmonyNPC harmonyNPC))
                harmonyNPC.AddStack(target, 2);

            // Build execution charge on owning item
            if (Main.myPlayer == Projectile.owner)
            {
                Player player = Main.player[Projectile.owner];
                if (player.HeldItem?.ModItem is NocturnalExecutioner exec)
                {
                    exec.ExecutionCharge += hit.Crit ? 15 : 8;
                }
            }

            float impactScale = 0.8f + ComboStep * 0.2f;
            NachtmusikCosmicVFX.SpawnCelestialImpact(target.Center, impactScale);

            for (int ring = 0; ring < 2 + ComboStep; ring++)
            {
                float p = (float)ring / (2 + ComboStep);
                Color ringColor = Color.Lerp(DeepPurple, Gold, p);
                CustomParticles.HaloRing(target.Center, ringColor, 0.3f + ring * 0.08f, 12 + ring * 2);
            }

            int dustCount = 6 + ComboStep * 3;
            for (int i = 0; i < dustCount; i++)
            {
                float angle = MathHelper.TwoPi * i / dustCount;
                float dp = (float)i / dustCount;
                Color dc = Color.Lerp(DeepPurple, Gold, dp);
                int dustType = i % 2 == 0 ? DustID.PurpleTorch : DustID.Enchanted_Gold;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 7f);
                Dust d = Dust.NewDustPerfect(target.Center, dustType, vel, 0, dc, 1.4f);
                d.noGravity = true;
            }

            NachtmusikCosmicVFX.SpawnMusicNoteBurst(target.Center, 2 + ComboStep, 25f);

            if (hit.Crit)
            {
                NachtmusikCosmicVFX.SpawnStarBurstImpact(target.Center, 1.0f, 3);
                if (Main.myPlayer == Projectile.owner)
                {
                    SeekingCrystalHelper.SpawnNachtmusikCrystals(
                        Projectile.GetSource_FromThis(), target.Center,
                        (Main.MouseWorld - target.Center).SafeNormalize(Vector2.UnitX) * 8f,
                        (int)(Projectile.damage * 0.2f), Projectile.knockBack * 0.4f,
                        Projectile.owner, 4);
                }
            }

            Lighting.AddLight(target.Center, 0.6f, 0.35f, 0.9f);
        }

        #endregion

        #region Custom VFX

        protected override void DrawCustomVFX(SpriteBatch sb)
        {
            if (Progression < 0.08f || Progression > 0.92f) return;

            Vector2 tipPos = GetBladeTipPosition();

            // Dense cosmic dust trail
            for (int i = 0; i < 2; i++)
            {
                float dp = Main.rand.NextFloat();
                Color dc = Color.Lerp(DeepPurple, Gold, dp);
                int dustType = dp < 0.5f ? DustID.PurpleTorch : DustID.Enchanted_Gold;
                Vector2 dustPos = Vector2.Lerp(Owner.MountedCenter, tipPos, Main.rand.NextFloat(0.4f, 1f));
                Dust d = Dust.NewDustPerfect(dustPos, dustType,
                    -SwordDirection * Main.rand.NextFloat(1f, 3f) + Main.rand.NextVector2Circular(1f, 1f),
                    0, dc, 1.5f);
                d.noGravity = true;
                d.fadeIn = 1.2f;
            }

            // Celestial shimmer trail (hue oscillation in purple-gold range)
            if (Main.rand.NextBool(3))
            {
                float hue = Main.rand.NextBool()
                    ? 0.73f + (Main.GameUpdateCount * 0.015f % 0.12f)
                    : 0.12f + (Main.GameUpdateCount * 0.015f % 0.08f);
                Color shimmer = Main.hslToRgb(hue, 0.9f, 0.8f);
                Dust s = Dust.NewDustPerfect(tipPos, DustID.PurpleTorch,
                    Main.rand.NextVector2Circular(1.5f, 1.5f), 0, shimmer, 1.1f);
                s.noGravity = true;
            }

            // Star sparkle accents
            if (Main.rand.NextBool(3))
            {
                Vector2 sparklePos = tipPos + Main.rand.NextVector2Circular(10f, 10f);
                Dust star = Dust.NewDustPerfect(sparklePos, DustID.GoldFlame,
                    Main.rand.NextVector2Circular(2f, 2f), 0, StarWhite, 1.0f);
                star.noGravity = true;
            }

            // Music notes — cosmic serenade (hue-shifting)
            if (Main.rand.NextBool(5))
            {
                Vector2 noteVel = -SwordDirection * 1.5f + new Vector2(0, -0.5f);
                MagnumParticleHandler.SpawnParticle(new HueShiftingMusicNoteParticle(
                    tipPos, noteVel,
                    hueMin: 0.10f, hueMax: 0.78f,
                    saturation: 0.85f, luminosity: 0.65f,
                    scale: 0.80f, lifetime: 28, hueSpeed: 0.025f));
            }

            // Blade-tip bloom — cosmic glow
            {
                float bloomOpacity = MathHelper.Clamp((Progression - 0.08f) / 0.12f, 0f, 1f)
                                   * MathHelper.Clamp((0.92f - Progression) / 0.12f, 0f, 1f);
                float bloomScale = 0.45f + ComboStep * 0.08f;
                BloomRenderer.DrawBloomStackAdditive(tipPos, DeepPurple, Gold, bloomScale, bloomOpacity);
            }
        }

        #endregion
    }
}
