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
    /// Swing projectile for Midnight's Crescendo — momentum-building sword.
    /// 3-phase combo: Harmonic Strike → Resonant Arc → Crescendo Finale.
    /// VFX intensity scales with owning item's crescendo stacks.
    /// </summary>
    public sealed class MidnightsCrescendoSwing : MeleeSwingBase
    {
        #region Theme Colors

        private static readonly Color Violet = NachtmusikCosmicVFX.Violet;
        private static readonly Color Gold = NachtmusikCosmicVFX.Gold;
        private static readonly Color NebulaPink = NachtmusikCosmicVFX.NebulaPink;
        private static readonly Color DeepPurple = NachtmusikCosmicVFX.DeepPurple;
        private static readonly Color StarWhite = NachtmusikCosmicVFX.StarWhite;

        private static readonly Color[] CrescendoPalette = new Color[]
        {
            new Color(30, 20, 60),      // [0] Pianissimo — soft void
            new Color(60, 45, 110),     // [1] Piano — dim violet
            new Color(123, 104, 238),   // [2] Mezzo — bright violet
            new Color(180, 140, 220),   // [3] Forte — nebula pink
            new Color(230, 200, 140),   // [4] Fortissimo — warm gold
            new Color(255, 245, 220)    // [5] Sforzando — blazing white-gold
        };

        #endregion

        #region Combo Phases

        // Phase 0: Harmonic Strike — quick horizontal slash
        private static readonly ComboPhase Phase0_HarmonicStrike = new ComboPhase(
            curves: new CurveSegment[]
            {
                new CurveSegment(EasingType.PolyOut, 0f, -0.8f, 0.2f, 2),
                new CurveSegment(EasingType.PolyIn, 0.2f, -0.6f, 1.4f, 3),
                new CurveSegment(EasingType.PolyOut, 0.8f, 0.8f, 0.15f, 2)
            },
            maxAngle: MathHelper.PiOver2 * 1.4f,
            duration: 24,
            bladeLength: 150f,
            flip: false,
            squish: 0.92f,
            damageMult: 0.85f
        );

        // Phase 1: Resonant Arc — wider backhand arc
        private static readonly ComboPhase Phase1_ResonantArc = new ComboPhase(
            curves: new CurveSegment[]
            {
                new CurveSegment(EasingType.PolyOut, 0f, -0.9f, 0.25f, 2),
                new CurveSegment(EasingType.PolyIn, 0.25f, -0.65f, 1.6f, 3),
                new CurveSegment(EasingType.PolyOut, 0.83f, 0.95f, 0.1f, 2)
            },
            maxAngle: MathHelper.PiOver2 * 1.6f,
            duration: 26,
            bladeLength: 155f,
            flip: true,
            squish: 0.88f,
            damageMult: 1.0f
        );

        // Phase 2: Crescendo Finale — powerful overhead slam
        private static readonly ComboPhase Phase2_CrescendoFinale = new ComboPhase(
            curves: new CurveSegment[]
            {
                new CurveSegment(EasingType.SineOut, 0f, -1.05f, 0.18f, 2),
                new CurveSegment(EasingType.PolyIn, 0.2f, -0.87f, 2.0f, 4),
                new CurveSegment(EasingType.PolyOut, 0.82f, 1.13f, 0.05f, 2)
            },
            maxAngle: MathHelper.PiOver2 * 2.0f,
            duration: 35,
            bladeLength: 165f,
            flip: false,
            squish: 0.82f,
            damageMult: 1.35f
        );

        #endregion

        #region Abstract Overrides

        protected override ComboPhase[] GetAllPhases() => new ComboPhase[]
        {
            Phase0_HarmonicStrike,
            Phase1_ResonantArc,
            Phase2_CrescendoFinale
        };

        protected override Color[] GetPalette() => CrescendoPalette;

        protected override CalamityStyleTrailRenderer.TrailStyle GetTrailStyle()
            => CalamityStyleTrailRenderer.TrailStyle.Cosmic;

        protected override string GetSmearTexturePath(int comboStep) => comboStep switch
        {
            1 => "MagnumOpus/Assets/Particles/SwordArc6",
            2 => "MagnumOpus/Assets/Particles/CurvedSwordSlash",
            _ => "MagnumOpus/Assets/Particles/SwordArc1"
        };

        #endregion

        #region Virtual Overrides

        protected override Texture2D GetBladeTexture()
            => ModContent.Request<Texture2D>("MagnumOpus/Content/Nachtmusik/ResonantWeapons/MidnightsCrescendo").Value;

        protected override SoundStyle GetSwingSound()
            => SoundID.Item71 with { Pitch = 0.1f + ComboStep * 0.15f, Volume = 0.85f };

        protected override int GetInitialDustType() => DustID.PurpleTorch;

        protected override int GetSecondaryDustType() => DustID.Enchanted_Gold;

        protected override Vector3 GetLightColor()
        {
            float intensity = 0.5f + GetCrescendoIntensity() * 0.5f;
            Color c = Color.Lerp(Violet, Gold, GetCrescendoIntensity());
            return c.ToVector3() * intensity;
        }

        #endregion

        #region Crescendo Helpers

        private float GetCrescendoIntensity()
        {
            if (Projectile.owner >= 0 && Projectile.owner < Main.maxPlayers)
            {
                Player player = Main.player[Projectile.owner];
                if (player.HeldItem?.ModItem is MidnightsCrescendo mc)
                    return mc.CrescendoStacks / 15f;
            }
            return 0f;
        }

        private int GetCrescendoStacks()
        {
            if (Projectile.owner >= 0 && Projectile.owner < Main.maxPlayers)
            {
                Player player = Main.player[Projectile.owner];
                if (player.HeldItem?.ModItem is MidnightsCrescendo mc)
                    return mc.CrescendoStacks;
            }
            return 0;
        }

        #endregion

        #region Combo Specials

        protected override void HandleComboSpecials()
        {
            if (hasSpawnedSpecial) return;

            float ci = GetCrescendoIntensity();

            // Phase 0: Small accents at 50%
            if (ComboStep == 0 && Progression >= 0.50f)
            {
                hasSpawnedSpecial = true;
                Vector2 tip = GetBladeTipPosition();
                CustomParticles.GenericFlare(tip, Color.Lerp(Violet, Gold, ci), 0.4f + ci * 0.2f, 12);
                NachtmusikCosmicVFX.SpawnMusicNoteBurst(tip, 2, 15f);
            }

            // Phase 1: Music note cascade + wave at 55% (if high stacks)
            if (ComboStep == 1 && Progression >= 0.55f)
            {
                hasSpawnedSpecial = true;

                if (GetCrescendoStacks() >= 8 && Main.myPlayer == Projectile.owner)
                {
                    Vector2 tip = GetBladeTipPosition();
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), tip,
                        SwordDirection * 14f,
                        ModContent.ProjectileType<CrescendoWaveProjectile>(),
                        (int)(Projectile.damage * 0.4f), 2f, Projectile.owner);
                }

                Vector2 vfxTip = GetBladeTipPosition();
                float burstScale = 0.5f + ci * 0.5f;
                CustomParticles.GenericFlare(vfxTip, StarWhite, 0.6f * burstScale, 16);
                CustomParticles.GenericFlare(vfxTip, Violet, 0.4f * burstScale, 14);
                for (int i = 0; i < 3; i++)
                {
                    float p = i / 3f;
                    Color rc = Color.Lerp(DeepPurple, Gold, p);
                    CustomParticles.HaloRing(vfxTip, rc, 0.3f + i * 0.1f, 12 + i * 2);
                }
                NachtmusikCosmicVFX.SpawnMusicNoteBurst(vfxTip, 4, 30f);
            }

            // Phase 2: Crescendo Finale at 60% — seeking crystals, grand VFX
            if (ComboStep == 2 && Progression >= 0.60f)
            {
                hasSpawnedSpecial = true;

                if (Main.myPlayer == Projectile.owner)
                {
                    Vector2 tip = GetBladeTipPosition();
                    int crystalCount = 3 + (int)(ci * 4);
                    SeekingCrystalHelper.SpawnNachtmusikCrystals(
                        Projectile.GetSource_FromThis(), tip,
                        SwordDirection * 8f, (int)(Projectile.damage * 0.2f),
                        Projectile.knockBack * 0.4f, Projectile.owner, crystalCount);

                    // Always fire a crescendo wave on the finale
                    if (GetCrescendoStacks() >= 5)
                    {
                        Projectile.NewProjectile(Projectile.GetSource_FromThis(), tip,
                            SwordDirection * 16f,
                            ModContent.ProjectileType<CrescendoWaveProjectile>(),
                            (int)(Projectile.damage * 0.5f), 3f, Projectile.owner);
                    }
                }

                Vector2 vfxTip = GetBladeTipPosition();
                NachtmusikCosmicVFX.SpawnGrandCelestialImpact(vfxTip, 1.0f + ci * 0.5f);
                for (int i = 0; i < 4 + (int)(ci * 3); i++)
                {
                    float p = i / 7f;
                    Color rc = Color.Lerp(DeepPurple, Gold, p);
                    CustomParticles.HaloRing(vfxTip, rc, 0.3f + i * 0.1f, 13 + i * 2);
                }
                NachtmusikCosmicVFX.SpawnMusicNoteBurst(vfxTip, 5 + (int)(ci * 3), 40f);

                if (ci >= 0.5f)
                    MagnumScreenEffects.AddScreenShake(4f + ci * 4f);
            }
        }

        #endregion

        #region On Hit — Celestial Harmony + Stack Building

        protected override void OnSwingHitNPC(NPC target, NPC.HitInfo hit, int remainingDamageCount)
        {
            target.AddBuff(ModContent.BuffType<CelestialHarmony>(), 360);

            int stacksToAdd = hit.Crit ? 2 : 1;
            if (target.TryGetGlobalNPC(out CelestialHarmonyNPC harmonyNPC))
                harmonyNPC.AddStack(target, stacksToAdd);

            // Build crescendo stacks on the owning item
            if (Main.myPlayer == Projectile.owner)
            {
                Player player = Main.player[Projectile.owner];
                if (player.HeldItem?.ModItem is MidnightsCrescendo mc)
                {
                    mc.CrescendoStacks += hit.Crit ? 2 : 1;
                    mc.ResetDecayTimer();
                }
            }

            float ci = GetCrescendoIntensity();
            float impactScale = 0.7f + ComboStep * 0.15f + ci * 0.3f;
            NachtmusikCosmicVFX.SpawnCelestialImpact(target.Center, impactScale);

            int dustCount = 5 + ComboStep * 2 + (int)(ci * 5);
            for (int i = 0; i < dustCount; i++)
            {
                float angle = MathHelper.TwoPi * i / dustCount;
                float dp = (float)i / dustCount;
                Color dc = Color.Lerp(Violet, Gold, dp);
                int dustType = i % 2 == 0 ? DustID.PurpleTorch : DustID.Enchanted_Gold;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 6f + ci * 3f);
                Dust d = Dust.NewDustPerfect(target.Center, dustType, vel, 0, dc, 1.3f + ci * 0.3f);
                d.noGravity = true;
            }

            NachtmusikCosmicVFX.SpawnMusicNoteBurst(target.Center, 2 + ComboStep, 22f);

            Lighting.AddLight(target.Center, Violet.ToVector3() * (0.5f + ci * 0.5f));
        }

        #endregion

        #region Custom VFX

        protected override void DrawCustomVFX(SpriteBatch sb)
        {
            if (Progression < 0.08f || Progression > 0.92f) return;

            Vector2 tipPos = GetBladeTipPosition();
            float ci = GetCrescendoIntensity();

            // Violet/gold dust trail — density scales with crescendo
            int dustPerFrame = 1 + (int)(ci * 2);
            for (int i = 0; i < dustPerFrame; i++)
            {
                float dp = Main.rand.NextFloat();
                Color dc = Color.Lerp(Violet, Gold, dp);
                int dustType = dp < 0.5f ? DustID.PurpleTorch : DustID.Enchanted_Gold;
                Vector2 dustPos = Vector2.Lerp(Owner.MountedCenter, tipPos, Main.rand.NextFloat(0.4f, 1f));
                Dust d = Dust.NewDustPerfect(dustPos, dustType,
                    -SwordDirection * Main.rand.NextFloat(1f, 3f) + Main.rand.NextVector2Circular(1f, 1f),
                    0, dc, 1.3f + ci * 0.4f);
                d.noGravity = true;
                d.fadeIn = 1.1f;
            }

            // Crescendo shimmer accents
            if (ci >= 0.3f && Main.rand.NextBool(3))
            {
                float hue = 0.73f + (Main.GameUpdateCount * 0.02f % 0.15f);
                Color shimmer = Main.hslToRgb(hue, 0.9f, 0.7f + ci * 0.2f);
                Dust s = Dust.NewDustPerfect(tipPos, DustID.GemAmethyst,
                    Main.rand.NextVector2Circular(1.5f, 1.5f), 0, shimmer, 1.0f);
                s.noGravity = true;
            }

            // Music notes — crescendo-driven (hue-shifting)
            int noteChance = Math.Max(2, 7 - (int)(ci * 5));
            if (Main.rand.NextBool(noteChance))
            {
                Vector2 noteVel = -SwordDirection * 1.2f + new Vector2(0, -0.4f);
                MagnumParticleHandler.SpawnParticle(new HueShiftingMusicNoteParticle(
                    tipPos, noteVel,
                    hueMin: 0.10f, hueMax: 0.78f,
                    saturation: 0.85f, luminosity: 0.65f,
                    scale: 0.7f + ci * 0.3f, lifetime: 25, hueSpeed: 0.025f));
            }

            // Blade-tip bloom — crescendo glow
            {
                float bloomOpacity = MathHelper.Clamp((Progression - 0.08f) / 0.12f, 0f, 1f)
                                   * MathHelper.Clamp((0.92f - Progression) / 0.12f, 0f, 1f);
                float bloomScale = (0.4f + ComboStep * 0.07f) * (1f + ci * 0.3f);
                BloomRenderer.DrawBloomStackAdditive(tipPos, Violet, Gold, bloomScale, bloomOpacity);
            }
        }

        #endregion
    }
}
