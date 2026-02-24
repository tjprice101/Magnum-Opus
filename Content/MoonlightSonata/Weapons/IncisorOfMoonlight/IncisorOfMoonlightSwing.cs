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
using MagnumOpus.Content.MoonlightSonata.Projectiles;
using MagnumOpus.Content.MoonlightSonata.Debuffs;
using static MagnumOpus.Common.Systems.Particles.Particle;

namespace MagnumOpus.Content.MoonlightSonata.Weapons.IncisorOfMoonlight
{
    /// <summary>
    /// Swing projectile for Incisor of Moonlight — "The Stellar Scalpel".
    /// 4-phase lunar combo: Lunar Arc → Crescent Edge → Silver Surge → Moonlit Crescendo.
    /// Each phase fires expanding crescent wave projectiles (MoonlightWaveProjectile).
    ///
    /// VFX pipeline (sealed in MeleeSwingBase):
    ///   Trail (CalamityStyleTrailRenderer.Cosmic) → Smear → Blade → Glow → LensFlare → MotionBlur → CustomVFX
    ///
    /// Custom VFX layer adds:
    ///   - Resonant edge bloom (constellation starpoints along blade, not crescent)
    ///   - Precision spark trails (tight tuning-fork pattern)
    ///   - God ray bursts + screen distortion on crescendo
    /// </summary>
    public sealed class IncisorOfMoonlightSwing : MeleeSwingBase
    {
        #region Palette (canonical MoonlightVFXLibrary references)

        private int _crystalCooldown;

        // 6-color Moonlight palette — dusk to moonbeam
        private static readonly Color[] MoonlightPalette = new Color[]
        {
            MoonlightVFXLibrary.NightPurple,   // [0] Pianissimo — deep night purple
            MoonlightVFXLibrary.DarkPurple,    // [1] Piano — indigo shadow
            MoonlightVFXLibrary.Violet,        // [2] Mezzo — violet body
            MoonlightVFXLibrary.IceBlue,       // [3] Forte — moonlit blue
            MoonlightVFXLibrary.Lavender,      // [4] Fortissimo — lavender glow
            MoonlightVFXLibrary.MoonWhite      // [5] Sforzando — pure moonbeam
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

        // Phase 1: Crescent Edge — quick reverse arc
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

        // Phase 3: Moonlit Crescendo — massive overhead slam
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
            => ModContent.Request<Texture2D>("MagnumOpus/Content/MoonlightSonata/Weapons/IncisorOfMoonlight/IncisorOfMoonlight").Value;

        protected override SoundStyle GetSwingSound()
            => SoundID.Item71 with { Pitch = -0.25f + ComboStep * 0.12f, Volume = 0.85f };

        protected override int GetInitialDustType() => DustID.PurpleTorch;

        protected override int GetSecondaryDustType() => DustID.Enchanted_Pink;

        protected override Vector3 GetLightColor()
        {
            float intensity = 0.55f + ComboStep * 0.12f;
            float pulse = 1f + MathF.Sin(Main.GlobalTimeWrappedHourly * 4f) * 0.1f;
            Color c = Color.Lerp(MoonlightVFXLibrary.DarkPurple, MoonlightVFXLibrary.IceBlue, Progression);
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
                CustomParticles.GenericFlare(vfxTip, MoonlightVFXLibrary.IceBlue, 0.5f, 14);
                CustomParticles.HaloRing(vfxTip, MoonlightVFXLibrary.Violet, 0.3f, 12);
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
                CustomParticles.GenericFlare(vfxTip, MoonlightVFXLibrary.Violet, 0.45f, 14);
                CustomParticles.HaloRing(vfxTip, MoonlightVFXLibrary.DarkPurple, 0.35f, 13);
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
                CustomParticles.GenericFlare(vfxTip, MoonlightVFXLibrary.IceBlue, 0.7f, 18);
                CustomParticles.GenericFlare(vfxTip, MoonlightVFXLibrary.Lavender, 0.55f, 16);
                for (int i = 0; i < 3; i++)
                {
                    float progress = i / 3f;
                    Color ringColor = Color.Lerp(MoonlightVFXLibrary.DarkPurple, MoonlightVFXLibrary.IceBlue, progress);
                    CustomParticles.HaloRing(vfxTip, ringColor, 0.3f + i * 0.08f, 13 + i * 2);
                }
                MoonlightVFXLibrary.SpawnMusicNotes(vfxTip, 3, 25f, 0.8f, 1.0f, 30);
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

                // Crescendo finisher — the night sky trembles
                Vector2 vfxTip = GetBladeTipPosition();
                IncisorOfMoonlightVFX.CrescendoFinisherVFX(vfxTip);
            }
        }

        #endregion

        #region On Hit — MusicsDissonance + Seeking Crystals

        protected override void OnSwingHitNPC(NPC target, NPC.HitInfo hit, int remainingDamageCount)
        {
            // Apply MusicsDissonance debuff
            target.AddBuff(ModContent.BuffType<MusicsDissonance>(), 240);

            // Incisor-unique impact VFX — resonant shockwave with tuning-fork pattern
            IncisorOfMoonlightVFX.OnHitImpact(target.Center, ComboStep, hit.Crit);

            // Gradient halo rings — purple to silver
            for (int ring = 0; ring < 2 + ComboStep; ring++)
            {
                float progress = (float)ring / (2 + ComboStep);
                Color ringColor = Color.Lerp(MoonlightVFXLibrary.DarkPurple, MoonlightVFXLibrary.Silver, progress);
                CustomParticles.HaloRing(target.Center, ringColor, 0.3f + ring * 0.1f, 12 + ring * 2);
            }

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

            Lighting.AddLight(target.Center, MoonlightVFXLibrary.Violet.ToVector3() * (0.7f + ComboStep * 0.15f));
        }

        #endregion

        #region Custom VFX — Stellar Scalpel

        protected override void DrawCustomVFX(SpriteBatch sb)
        {
            if (Progression <= 0.08f || Progression >= 0.92f) return;

            Vector2 tipPos = GetBladeTipPosition();

            // Incisor-unique per-frame effects — precision sparks, resonance pulses, music notes
            IncisorOfMoonlightVFX.SwingFrameEffects(Owner.MountedCenter, tipPos, SwordDirection, ComboStep, Projectile.timeLeft);

            // Resonant edge bloom — constellation starpoints along the blade ({A=0}, no batch restart)
            IncisorOfMoonlightVFX.DrawResonantEdgeBloom(sb, Owner.MountedCenter, tipPos, ComboStep, Progression);

            // Pulsing resonant light at the blade tip
            IncisorOfMoonlightVFX.AddResonantLight(tipPos, 0.5f + ComboStep * 0.15f);

            // Crystal shard accents on higher phases
            if (ComboStep >= 2 && Main.rand.NextBool(3))
            {
                float bladeProgress = Main.rand.NextFloat(0.4f, 1f);
                Vector2 bladePos = Owner.MountedCenter + SwordDirection * CurrentPhase.BladeLength * bladeProgress;
                Dust glow = Dust.NewDustPerfect(bladePos, DustID.PurpleCrystalShard,
                    -SwordDirection * Main.rand.NextFloat(1f, 3f), 0, MoonlightVFXLibrary.IceBlue, 1.4f);
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
