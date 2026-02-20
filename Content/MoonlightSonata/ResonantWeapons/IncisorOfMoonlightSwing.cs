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
using MagnumOpus.Content.MoonlightSonata;
using MagnumOpus.Content.MoonlightSonata.ResonantWeapons;
using MagnumOpus.Content.MoonlightSonata.Debuffs;
using MagnumOpus.Content.MoonlightSonata.VFX.IncisorOfMoonlight;
using static MagnumOpus.Common.Systems.Particles.Particle;

namespace MagnumOpus.Content.MoonlightSonata.ResonantWeapons
{
    /// <summary>
    /// Swing projectile for Incisor of Moonlight — Moonlight Sonata's crescent blade.
    /// 4-phase lunar combo: Lunar Arc → Crescent Edge → Silver Surge → Moonlit Crescendo.
    /// Each phase fires expanding crescent wave projectiles (MoonlightWaveProjectile).
    /// The sword channels crystallized moonlight — each swing a movement in the nocturne.
    /// </summary>
    public sealed class IncisorOfMoonlightSwing : MeleeSwingBase
    {
        #region Theme Colors

        private static readonly Color DarkPurple = MagnumThemePalettes.MoonlightDarkPurple;
        private static readonly Color MediumPurple = MagnumThemePalettes.MoonlightViolet;
        private static readonly Color LightBlue = MagnumThemePalettes.MoonlightIceBlue;
        private static readonly Color Silver = MagnumThemePalettes.MoonlightSilver;
        private static readonly Color Lavender = MagnumThemePalettes.MoonlightWeaponLavender;
        private static readonly Color LightPurple = MagnumThemePalettes.MoonlightLightPurple;

        private int _crystalCooldown;

        // 6-color Moonlight palette — dusk to moonbeam
        private static readonly Color[] MoonlightPalette = new Color[]
        {
            new Color(40, 0, 80),                        // [0] Pianissimo — deep night purple
            MagnumThemePalettes.MoonlightDarkPurple,     // [1] Piano — indigo shadow
            MagnumThemePalettes.MoonlightViolet,         // [2] Mezzo — violet body
            MagnumThemePalettes.MoonlightIceBlue,        // [3] Forte — moonlit blue
            MagnumThemePalettes.MoonlightWeaponLavender, // [4] Fortissimo — lavender glow
            MagnumThemePalettes.MoonlightMoonWhite       // [5] Sforzando — pure moonbeam
        };

        #endregion

        #region Combo Phases

        // Phase 0: Lunar Arc — smooth sweeping crescent
        private static readonly ComboPhase Phase0_LunarArc = new ComboPhase(
            curves: new CurveSegment[]
            {
                new CurveSegment(EasingType.PolyOut, 0f, -0.9f, 0.2f, 2),
                new CurveSegment(EasingType.PolyIn, 0.2f, -0.7f, 1.5f, 3),
                new CurveSegment(EasingType.PolyOut, 0.82f, 0.8f, 0.12f, 2)
            },
            maxAngle: MathHelper.PiOver2 * 1.4f,
            duration: 22,
            bladeLength: 148f,
            flip: false,
            squish: 0.93f,
            damageMult: 0.85f
        );

        // Phase 1: Crescent Edge — quick reverse arc, blade traces a crescent shape
        private static readonly ComboPhase Phase1_CrescentEdge = new ComboPhase(
            curves: new CurveSegment[]
            {
                new CurveSegment(EasingType.PolyOut, 0f, 0.85f, -0.18f, 2),
                new CurveSegment(EasingType.PolyIn, 0.18f, 0.67f, -1.55f, 3),
                new CurveSegment(EasingType.PolyOut, 0.78f, -0.88f, -0.1f, 2)
            },
            maxAngle: MathHelper.PiOver2 * 1.5f,
            duration: 24,
            bladeLength: 150f,
            flip: true,
            squish: 0.91f,
            damageMult: 0.95f
        );

        // Phase 2: Silver Surge — rising arc, wider and more luminous
        private static readonly ComboPhase Phase2_SilverSurge = new ComboPhase(
            curves: new CurveSegment[]
            {
                new CurveSegment(EasingType.SineOut, 0f, -1.0f, 0.22f, 2),
                new CurveSegment(EasingType.PolyIn, 0.22f, -0.78f, 1.75f, 3),
                new CurveSegment(EasingType.PolyOut, 0.83f, 0.97f, 0.08f, 2)
            },
            maxAngle: MathHelper.PiOver2 * 1.8f,
            duration: 28,
            bladeLength: 158f,
            flip: false,
            squish: 0.87f,
            damageMult: 1.1f
        );

        // Phase 3: Moonlit Crescendo — massive overhead slam, full lunar spectacle
        private static readonly ComboPhase Phase3_MoonlitCrescendo = new ComboPhase(
            curves: new CurveSegment[]
            {
                new CurveSegment(EasingType.SineOut, 0f, -1.15f, 0.2f, 2),
                new CurveSegment(EasingType.PolyIn, 0.22f, -0.95f, 2.15f, 4),
                new CurveSegment(EasingType.PolyOut, 0.84f, 1.2f, 0.05f, 2)
            },
            maxAngle: MathHelper.PiOver2 * 2.2f,
            duration: 36,
            bladeLength: 168f,
            flip: true,
            squish: 0.82f,
            damageMult: 1.45f
        );

        #endregion

        #region Abstract Overrides

        protected override ComboPhase[] GetAllPhases() => new ComboPhase[]
        {
            Phase0_LunarArc,
            Phase1_CrescentEdge,
            Phase2_SilverSurge,
            Phase3_MoonlitCrescendo
        };

        protected override Color[] GetPalette() => MoonlightPalette;

        protected override CalamityStyleTrailRenderer.TrailStyle GetTrailStyle()
            => CalamityStyleTrailRenderer.TrailStyle.Cosmic;

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
            => ModContent.Request<Texture2D>("MagnumOpus/Content/MoonlightSonata/ResonantWeapons/IncisorOfMoonlight").Value;

        protected override SoundStyle GetSwingSound()
            => SoundID.Item71 with { Pitch = -0.25f + ComboStep * 0.12f, Volume = 0.85f };

        protected override int GetInitialDustType() => DustID.PurpleTorch;

        protected override int GetSecondaryDustType() => DustID.Enchanted_Pink;

        protected override Vector3 GetLightColor()
        {
            float intensity = 0.55f + ComboStep * 0.12f;
            float pulse = 1f + MathF.Sin(Main.GlobalTimeWrappedHourly * 4f) * 0.1f;
            Color c = Color.Lerp(DarkPurple, LightBlue, Progression);
            return c.ToVector3() * intensity * pulse;
        }

        #endregion

        #region Combo Specials — Escalating Crescent Waves

        protected override void HandleComboSpecials()
        {
            if (_crystalCooldown > 0) _crystalCooldown--;
            if (hasSpawnedSpecial) return;

            // Phase 0 (Lunar Arc): 1 crescent wave at 60%
            if (ComboStep == 0 && Progression >= 0.60f)
            {
                hasSpawnedSpecial = true;
                if (Main.myPlayer == Projectile.owner)
                {
                    Vector2 tipPos = GetBladeTipPosition();
                    Vector2 vel = SwordDirection * 12f;
                    Projectile.NewProjectile(
                        Projectile.GetSource_FromThis(), tipPos, vel,
                        ModContent.ProjectileType<MoonlightWaveProjectile>(),
                        (int)(Projectile.damage * 0.6f), 2f, Projectile.owner);
                }

                Vector2 vfxTip = GetBladeTipPosition();
                CustomParticles.GenericFlare(vfxTip, LightBlue, 0.5f, 14);
                CustomParticles.HaloRing(vfxTip, MediumPurple, 0.3f, 12);
            }

            // Phase 1 (Crescent Edge): 1 crescent wave at 55%
            if (ComboStep == 1 && Progression >= 0.55f)
            {
                hasSpawnedSpecial = true;
                if (Main.myPlayer == Projectile.owner)
                {
                    Vector2 tipPos = GetBladeTipPosition();
                    Vector2 vel = SwordDirection * 13f;
                    Projectile.NewProjectile(
                        Projectile.GetSource_FromThis(), tipPos, vel,
                        ModContent.ProjectileType<MoonlightWaveProjectile>(),
                        (int)(Projectile.damage * 0.65f), 2.5f, Projectile.owner);
                }

                Vector2 vfxTip = GetBladeTipPosition();
                CustomParticles.GenericFlare(vfxTip, Color.White, 0.6f, 16);
                CustomParticles.GenericFlare(vfxTip, MediumPurple, 0.45f, 14);
                CustomParticles.HaloRing(vfxTip, DarkPurple, 0.35f, 13);
                CustomParticles.PrismaticSparkle(vfxTip, Silver, 0.35f);
            }

            // Phase 2 (Silver Surge): 2 crescent waves at 60% with spread
            if (ComboStep == 2 && Progression >= 0.60f)
            {
                hasSpawnedSpecial = true;
                if (Main.myPlayer == Projectile.owner)
                {
                    Vector2 tipPos = GetBladeTipPosition();
                    float spread = MathHelper.ToRadians(10f);
                    for (int i = -1; i <= 1; i += 2)
                    {
                        Vector2 vel = SwordDirection.RotatedBy(spread * i) * 13f;
                        Projectile.NewProjectile(
                            Projectile.GetSource_FromThis(), tipPos, vel,
                            ModContent.ProjectileType<MoonlightWaveProjectile>(),
                            (int)(Projectile.damage * 0.65f), 2.5f, Projectile.owner);
                    }
                }

                Vector2 vfxTip = GetBladeTipPosition();
                CustomParticles.GenericFlare(vfxTip, LightBlue, 0.7f, 18);
                CustomParticles.GenericFlare(vfxTip, Lavender, 0.55f, 16);
                for (int i = 0; i < 3; i++)
                {
                    float progress = i / 3f;
                    Color ringColor = Color.Lerp(DarkPurple, LightBlue, progress);
                    CustomParticles.HaloRing(vfxTip, ringColor, 0.3f + i * 0.08f, 13 + i * 2);
                }
                ThemedParticles.MoonlightSparks(vfxTip, SwordDirection, 6, 5f);
                CustomParticles.MoonlightMusicNotes(vfxTip, 3, 25f);
            }

            // Phase 3 (Moonlit Crescendo): 3 crescent waves at 55% — full spectacle
            if (ComboStep == 3 && Progression >= 0.55f)
            {
                hasSpawnedSpecial = true;
                if (Main.myPlayer == Projectile.owner)
                {
                    Vector2 tipPos = GetBladeTipPosition();
                    float spread = MathHelper.ToRadians(12f);
                    for (int i = -1; i <= 1; i++)
                    {
                        Vector2 vel = SwordDirection.RotatedBy(spread * i) * 14f;
                        Projectile.NewProjectile(
                            Projectile.GetSource_FromThis(), tipPos, vel,
                            ModContent.ProjectileType<MoonlightWaveProjectile>(),
                            (int)(Projectile.damage * 0.7f), 3f, Projectile.owner);
                    }
                }

                // VFX: Full moonlit crescendo — the night sky trembles
                Vector2 vfxTip = GetBladeTipPosition();
                UnifiedVFX.MoonlightSonata.Impact(vfxTip, 1.3f);
                CustomParticles.GenericFlare(vfxTip, Color.White, 1.0f, 22);
                CustomParticles.GenericFlare(vfxTip, LightBlue, 0.8f, 20);
                CustomParticles.GenericFlare(vfxTip, MediumPurple, 0.6f, 18);
                for (int i = 0; i < 5; i++)
                {
                    float progress = i / 5f;
                    Color ringColor = Color.Lerp(DarkPurple, Silver, progress);
                    CustomParticles.HaloRing(vfxTip, ringColor, 0.35f + i * 0.1f, 14 + i * 2);
                }
                CustomParticles.MoonlightMusicNotes(vfxTip, 6, 40f);

                // Lunar crystal shard burst — unique Incisor finisher treatment
                IncisorOfMoonlightVFX.CrescendoFinisherVFX(vfxTip);
            }
        }

        #endregion

        #region On Hit — MusicsDissonance + Seeking Crystals

        protected override void OnSwingHitNPC(NPC target, NPC.HitInfo hit, int remainingDamageCount)
        {
            // Apply MusicsDissonance debuff
            target.AddBuff(ModContent.BuffType<MusicsDissonance>(), 240);

            // Impact VFX scales with combo step
            float impactScale = 0.7f + ComboStep * 0.2f;
            UnifiedVFX.MoonlightSonata.Impact(target.Center, impactScale);

            // Gradient halo rings — purple to silver
            for (int ring = 0; ring < 2 + ComboStep; ring++)
            {
                float progress = (float)ring / (2 + ComboStep);
                Color ringColor = Color.Lerp(DarkPurple, Silver, progress);
                CustomParticles.HaloRing(target.Center, ringColor, 0.3f + ring * 0.1f, 12 + ring * 2);
            }

            // Moonlight impact VFX — unique Incisor resonance treatment
            IncisorOfMoonlightVFX.OnHitImpact(target.Center, ComboStep, hit.Crit);

            // Seeking crystals on hit — 3 normally, 5 on crit (30-frame cooldown)
            if (_crystalCooldown <= 0)
            {
                int crystalCount = hit.Crit ? 5 : 3;
                if (Main.myPlayer == Projectile.owner)
                {
                    SeekingCrystalHelper.SpawnMoonlightCrystals(
                        Projectile.GetSource_FromThis(),
                        target.Center,
                        (target.Center - Owner.Center).SafeNormalize(Vector2.UnitX) * 7f,
                        (int)(Projectile.damage * 0.35f),
                        2.5f,
                        Projectile.owner,
                        crystalCount);
                }
                _crystalCooldown = 30;
            }

            Lighting.AddLight(target.Center, MediumPurple.ToVector3() * (0.7f + ComboStep * 0.15f));
        }

        #endregion

        #region Custom VFX — Moonlight Shimmer

        protected override void DrawCustomVFX(SpriteBatch sb)
        {
            if (Progression <= 0.08f || Progression >= 0.92f) return;

            Vector2 tipPos = GetBladeTipPosition();

            // Unified swing-frame VFX — dense dust, sparkles, notes, lighting
            MoonlightVFXLibrary.SwingFrameVFX(tipPos, SwordDirection, ComboStep, Projectile.timeLeft);

            // Unique Incisor per-frame effects — silver sparks, resonance pulses, music notes
            IncisorOfMoonlightVFX.SwingFrameEffects(Owner.MountedCenter, tipPos, SwordDirection, ComboStep, Projectile.timeLeft);

            // Resonant edge bloom — silver glow points along the blade
            IncisorOfMoonlightVFX.DrawResonantEdgeBloom(sb, Owner.MountedCenter, tipPos, ComboStep, Progression);

            // Pulsing resonant light at the blade tip
            IncisorOfMoonlightVFX.AddResonantLight(tipPos, 0.5f + ComboStep * 0.15f);

            // Crystal shard accents on higher phases
            if (ComboStep >= 2 && Main.rand.NextBool(3))
            {
                float bladeProgress = Main.rand.NextFloat(0.4f, 1f);
                Vector2 bladePos = Owner.MountedCenter + SwordDirection * CurrentPhase.BladeLength * bladeProgress;
                Dust glow = Dust.NewDustPerfect(bladePos, DustID.PurpleCrystalShard,
                    -SwordDirection * Main.rand.NextFloat(1f, 3f), 0, LightBlue, 1.4f);
                glow.noGravity = true;
            }

            // Blade-tip bloom — moonlit glow (combo-aware)
            {
                float bloomOpacity = MathHelper.Clamp((Progression - 0.08f) / 0.12f, 0f, 1f)
                                   * MathHelper.Clamp((0.92f - Progression) / 0.12f, 0f, 1f);
                MoonlightVFXLibrary.DrawComboBloom(tipPos, ComboStep, 0.4f, bloomOpacity);
            }
        }

        #endregion
    }
}
