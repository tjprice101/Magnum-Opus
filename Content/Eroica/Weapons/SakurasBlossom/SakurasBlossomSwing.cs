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
using MagnumOpus.Content.Eroica.Projectiles;
using MagnumOpus.Content.MoonlightSonata.Debuffs;
using static MagnumOpus.Common.Systems.Particles.Particle;

namespace MagnumOpus.Content.Eroica.ResonantWeapons
{
    /// <summary>
    /// Swing projectile for Sakura's Blossom — Eroica's blooming sword of spring.
    /// 4-phase sakura combo: Petal Slash → Crimson Scatter → Blossom Bloom → Storm of Petals.
    /// Each phase spawns increasing numbers of spectral homing copies (SakurasBlossomSpectral).
    /// The blade literally blooms with petals — a flower unfurling across four movements.
    /// </summary>
    public sealed class SakurasBlossomSwing : MeleeSwingBase
    {
        #region Theme Colors

        private static readonly Color SakuraPink = MagnumThemePalettes.EroicaSakura;
        private static readonly Color SakuraPale = MagnumThemePalettes.SakuraPale;
        private static readonly Color EroicaScarlet = MagnumThemePalettes.EroicaBladeScarlet;
        private static readonly Color EroicaCrimson = MagnumThemePalettes.EroicaBladeCrimson;
        private static readonly Color EroicaGold = MagnumThemePalettes.EroicaGold;
        private static readonly Color PollenGold = MagnumThemePalettes.SakuraPollenGold;

        // 6-color Sakura palette — bud to full bloom
        private static readonly Color[] SakuraPalette = new Color[]
        {
            new Color(100, 20, 35),     // [0] Pianissimo — deep bud crimson
            new Color(180, 50, 70),     // [1] Piano — opening blossom
            new Color(255, 120, 150),   // [2] Mezzo — sakura pink body
            new Color(255, 170, 190),   // [3] Forte — pale petal glow
            new Color(255, 210, 140),   // [4] Fortissimo — golden pollen
            new Color(255, 245, 220)    // [5] Sforzando — white-hot bloom center
        };

        #endregion

        #region Combo Phases

        // Phase 0: Petal Slash — quick horizontal opener, petals scatter
        private static readonly ComboPhase Phase0_PetalSlash = new ComboPhase(
            curves: new CurveSegment[]
            {
                new CurveSegment(EasingType.PolyOut, 0f, -0.85f, 0.18f, 2),
                new CurveSegment(EasingType.PolyIn, 0.18f, -0.67f, 1.45f, 3),
                new CurveSegment(EasingType.PolyOut, 0.80f, 0.78f, 0.12f, 2)
            },
            maxAngle: MathHelper.PiOver2 * 1.4f,
            duration: 26,
            bladeLength: 155f,
            flip: false,
            squish: 0.92f,
            damageMult: 0.85f
        );

        // Phase 1: Crimson Scatter — backhand that tosses spectral copies wide
        private static readonly ComboPhase Phase1_CrimsonScatter = new ComboPhase(
            curves: new CurveSegment[]
            {
                new CurveSegment(EasingType.PolyOut, 0f, 0.9f, -0.2f, 2),
                new CurveSegment(EasingType.PolyIn, 0.2f, 0.7f, -1.6f, 3),
                new CurveSegment(EasingType.PolyOut, 0.82f, -0.9f, -0.1f, 2)
            },
            maxAngle: MathHelper.PiOver2 * 1.6f,
            duration: 28,
            bladeLength: 158f,
            flip: true,
            squish: 0.90f,
            damageMult: 1.0f
        );

        // Phase 2: Blossom Bloom — rising arc, pollen explodes from blade
        private static readonly ComboPhase Phase2_BlossomBloom = new ComboPhase(
            curves: new CurveSegment[]
            {
                new CurveSegment(EasingType.SineOut, 0f, -1.0f, 0.25f, 2),
                new CurveSegment(EasingType.PolyIn, 0.25f, -0.75f, 1.8f, 3),
                new CurveSegment(EasingType.PolyOut, 0.84f, 1.05f, 0.08f, 2)
            },
            maxAngle: MathHelper.PiOver2 * 1.9f,
            duration: 32,
            bladeLength: 165f,
            flip: false,
            squish: 0.86f,
            damageMult: 1.15f
        );

        // Phase 3: Storm of Petals — massive slam, sakura storm erupts
        private static readonly ComboPhase Phase3_StormOfPetals = new ComboPhase(
            curves: new CurveSegment[]
            {
                new CurveSegment(EasingType.SineOut, 0f, -1.15f, 0.18f, 2),
                new CurveSegment(EasingType.PolyIn, 0.2f, -0.97f, 2.2f, 4),
                new CurveSegment(EasingType.PolyOut, 0.82f, 1.23f, 0.05f, 2)
            },
            maxAngle: MathHelper.PiOver2 * 2.3f,
            duration: 40,
            bladeLength: 175f,
            flip: true,
            squish: 0.80f,
            damageMult: 1.5f
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
            => ModContent.Request<Texture2D>("MagnumOpus/Content/Eroica/ResonantWeapons/SakurasBlossom").Value;

        protected override SoundStyle GetSwingSound()
            => SoundID.Item71 with { Pitch = -0.15f + ComboStep * 0.15f, Volume = 0.9f };

        protected override int GetInitialDustType() => DustID.RedTorch;

        protected override int GetSecondaryDustType() => DustID.PinkTorch;

        protected override Vector3 GetLightColor()
        {
            float intensity = 0.55f + ComboStep * 0.12f;
            Color c = Color.Lerp(SakuraPink, EroicaGold, Progression);
            return c.ToVector3() * intensity;
        }

        #endregion

        #region Combo Specials — Escalating Spectral Copies

        protected override void HandleComboSpecials()
        {
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
                CustomParticles.GenericFlare(vfxTip, SakuraPink, 0.55f, 14);
                CustomParticles.HaloRing(vfxTip, EroicaScarlet, 0.3f, 12);
                ThemedParticles.SakuraPetals(vfxTip, 3, 25f);
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
                CustomParticles.GenericFlare(vfxTip, Color.White, 0.65f, 16);
                CustomParticles.GenericFlare(vfxTip, EroicaScarlet, 0.5f, 14);
                CustomParticles.HaloRing(vfxTip, SakuraPink, 0.38f, 14);
                ThemedParticles.SakuraPetals(vfxTip, 5, 30f);
                ThemedParticles.EroicaSparks(vfxTip, SwordDirection, 6, 5f);
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

                // VFX: Pollen explosion — golden motes scatter from bloom
                Vector2 vfxTip = GetBladeTipPosition();
                CustomParticles.GenericFlare(vfxTip, PollenGold, 0.75f, 18);
                CustomParticles.GenericFlare(vfxTip, SakuraPink, 0.6f, 16);
                for (int i = 0; i < 3; i++)
                {
                    float ring_progress = i / 3f;
                    Color ringColor = Color.Lerp(SakuraPink, EroicaGold, ring_progress);
                    CustomParticles.HaloRing(vfxTip, ringColor, 0.3f + i * 0.08f, 13 + i * 2);
                }
                ThemedParticles.SakuraPetals(vfxTip, 8, 40f);
                ThemedParticles.EroicaMusicNotes(vfxTip, 3, 25f);
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
                UnifiedVFX.Eroica.Impact(vfxTip, 1.4f);
                CustomParticles.GenericFlare(vfxTip, Color.White, 1.0f, 22);
                CustomParticles.GenericFlare(vfxTip, SakuraPink, 0.8f, 20);
                CustomParticles.GenericFlare(vfxTip, EroicaGold, 0.6f, 18);
                for (int i = 0; i < 5; i++)
                {
                    float progress = i / 5f;
                    Color ringColor = Color.Lerp(EroicaScarlet, PollenGold, progress);
                    CustomParticles.HaloRing(vfxTip, ringColor, 0.35f + i * 0.1f, 14 + i * 2);
                }
                ThemedParticles.SakuraPetals(vfxTip, 12, 55f);
                ThemedParticles.EroicaMusicNotes(vfxTip, 6, 40f);

                MagnumScreenEffects.AddScreenShake(7f);
            }
        }

        #endregion

        #region On Hit — MusicsDissonance + Seeking Crystals

        protected override void OnSwingHitNPC(NPC target, NPC.HitInfo hit, int remainingDamageCount)
        {
            // Apply MusicsDissonance debuff
            target.AddBuff(ModContent.BuffType<MusicsDissonance>(), 240);

            // Impact VFX scales with combo step
            float impactScale = 0.8f + ComboStep * 0.2f;
            UnifiedVFX.Eroica.Impact(target.Center, impactScale);

            // Gradient halo rings — sakura to gold
            for (int ring = 0; ring < 2 + ComboStep; ring++)
            {
                float progress = (float)ring / (2 + ComboStep);
                Color ringColor = Color.Lerp(SakuraPink, EroicaGold, progress);
                CustomParticles.HaloRing(target.Center, ringColor, 0.3f + ring * 0.1f, 13 + ring * 2);
            }

            // Radial sakura/scarlet dust burst
            int dustCount = 8 + ComboStep * 3;
            for (int i = 0; i < dustCount; i++)
            {
                float angle = MathHelper.TwoPi * i / dustCount;
                float dustProgress = (float)i / dustCount;
                int dustType = i % 2 == 0 ? DustID.RedTorch : DustID.PinkTorch;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 7f);
                Color dustColor = Color.Lerp(EroicaScarlet, SakuraPink, dustProgress);
                Dust d = Dust.NewDustPerfect(target.Center, dustType, vel, 0, dustColor, 1.5f);
                d.noGravity = true;
            }

            // Sakura petals burst on hit
            ThemedParticles.SakuraPetals(target.Center, 3 + ComboStep, 30f);

            // Music notes — the song of spring
            ThemedParticles.EroicaMusicNotes(target.Center, 2 + ComboStep, 25f);

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

            Lighting.AddLight(target.Center, 0.9f, 0.4f, 0.45f);
        }

        #endregion

        #region Custom VFX — Blooming Sakura Aura

        protected override void DrawCustomVFX(SpriteBatch sb)
        {
            if (Progression <= 0.08f || Progression >= 0.92f) return;

            Vector2 tipPos = GetBladeTipPosition();

            // Dense sakura petal trail — petals unfurl from the blade edge
            for (int i = 0; i < 2; i++)
            {
                float dustProgress = Main.rand.NextFloat();
                Color petalColor = Color.Lerp(SakuraPink, SakuraPale, dustProgress);
                int dustType = dustProgress < 0.5f ? DustID.RedTorch : DustID.PinkTorch;
                Vector2 dustPos = Vector2.Lerp(Owner.MountedCenter, tipPos, Main.rand.NextFloat(0.4f, 1f));
                Dust d = Dust.NewDustPerfect(dustPos, dustType,
                    -SwordDirection * Main.rand.NextFloat(1f, 3f) + Main.rand.NextVector2Circular(1f, 1f),
                    0, petalColor, 1.5f);
                d.noGravity = true;
                d.fadeIn = 1.2f;
            }

            // Golden pollen motes drifting upward
            if (Main.rand.NextBool(3))
            {
                Vector2 pollenPos = tipPos + Main.rand.NextVector2Circular(10f, 10f);
                Dust pollen = Dust.NewDustPerfect(pollenPos, DustID.GoldFlame,
                    new Vector2(Main.rand.NextFloat(-1f, 1f), -Main.rand.NextFloat(0.5f, 2f)),
                    0, PollenGold, 1.1f);
                pollen.noGravity = true;
            }

            // Scarlet ember accents from blade
            if (Main.rand.NextBool(4))
            {
                float bladeProgress = Main.rand.NextFloat(0.5f, 1f);
                Vector2 bladePos = Owner.MountedCenter + SwordDirection * CurrentPhase.BladeLength * bladeProgress;
                Dust ember = Dust.NewDustPerfect(bladePos, DustID.RedTorch,
                    -SwordDirection * Main.rand.NextFloat(1f, 2.5f), 0, EroicaScarlet, 1.3f);
                ember.noGravity = true;
            }

            // Sakura blossom shimmer — hue oscillation in pink-gold range
            if (Main.rand.NextBool(3))
            {
                float sakuraHue = 0.95f + Main.rand.NextFloat() * 0.08f;
                Color shimmer = Main.hslToRgb(sakuraHue % 1f, 0.85f, 0.7f);
                Dust s = Dust.NewDustPerfect(tipPos, DustID.PinkFairy,
                    Main.rand.NextVector2Circular(1.5f, 1.5f), 0, shimmer, 1.2f);
                s.noGravity = true;
            }

            // Music notes — song of blossoming spring
            if (Main.rand.NextBool(5))
            {
                float shimmer = 1f + MathF.Sin(Main.GameUpdateCount * 0.15f) * 0.12f;
                MagnumParticleHandler.SpawnParticle(new HueShiftingMusicNoteParticle(
                    tipPos,
                    -SwordDirection * 1.5f + new Vector2(0, -0.8f),
                    hueMin: 0.90f, hueMax: 0.98f,
                    saturation: 0.90f, luminosity: 0.65f,
                    scale: 0.75f * shimmer,
                    lifetime: 28,
                    hueSpeed: 0.02f
                ));
            }

            // Blade-tip bloom glow — sakura blossom radiance
            float bloomOpacity = MathHelper.Clamp((Progression - 0.08f) / 0.10f, 0f, 1f)
                               * MathHelper.Clamp((0.92f - Progression) / 0.10f, 0f, 1f);
            if (bloomOpacity > 0f)
            {
                BloomRenderer.DrawBloomStackAdditive(
                    tipPos,
                    SakuraPink,
                    EroicaGold,
                    scale: 0.45f + ComboStep * 0.08f,
                    opacity: bloomOpacity);
            }
        }

        #endregion
    }
}
