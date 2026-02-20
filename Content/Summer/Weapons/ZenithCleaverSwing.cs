using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.BaseClasses;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.VFX.Trails;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Content.Summer.Projectiles;
using static MagnumOpus.Common.Systems.Particles.Particle;

namespace MagnumOpus.Content.Summer.Weapons
{
    /// <summary>
    /// Zenith Cleaver held-projectile swing — the blazing second movement of summer.
    /// 3-phase solar combo: Scorching Slash → Solar Arc → Zenith Slam.
    /// Every swing fires a SolarWave; every 7th hit unleashes Zenith Strike
    /// (ZenithFlare + 8 radial SolarWaves). Hits apply Sunstroke debuffs
    /// and spawn seeking SummerCrystals.
    /// </summary>
    public sealed class ZenithCleaverSwing : MeleeSwingBase
    {
        // ── Theme Colors ──
        private static readonly Color SunGold = MagnumThemePalettes.SunGold;
        private static readonly Color SunOrange = MagnumThemePalettes.SunOrange;
        private static readonly Color SunWhite = MagnumThemePalettes.SunWhite;
        private static readonly Color SunRed = MagnumThemePalettes.SunRed;

        private int _crystalCooldown;

        // ── Swing Counter (stored in ai[2]) — Zenith Strike triggers on 7th ──
        private int SwingCounter
        {
            get => (int)Projectile.ai[2];
            set => Projectile.ai[2] = value;
        }

        // ── 6-Color Palette: pianissimo → sforzando (solar heat gradient) ──
        private static readonly Color[] SummerPalette = new Color[]
        {
            new Color(120, 50, 10),     // [0] Deep amber shadow
            new Color(200, 90, 20),     // [1] Warm ember
            new Color(255, 140, 0),     // [2] Sun orange (primary)
            new Color(255, 215, 0),     // [3] Sun gold (hot)
            new Color(255, 240, 140),   // [4] Bright solar glow
            new Color(255, 250, 240),   // [5] White-hot solar core
        };

        #region ── Combo Phase Definitions ──

        // Phase 0 — Scorching Slash (a quick cutting beam of summer heat)
        private static readonly ComboPhase Phase0_ScorchingSlash = new ComboPhase(
            curves: new CurveSegment[]
            {
                new CurveSegment(EasingType.PolyOut, 0f, -0.9f, 0.2f, 2),       // Tight windup
                new CurveSegment(EasingType.PolyIn, 0.18f, -0.7f, 1.5f, 3),     // Fast scorching cut
                new CurveSegment(EasingType.PolyOut, 0.72f, 0.8f, 0.2f, 2),     // Brief follow-through
            },
            maxAngle: MathHelper.Pi * 1.2f,
            duration: 20,
            bladeLength: 100f,
            flip: false,
            squish: 0.90f,
            damageMult: 0.85f
        );

        // Phase 1 — Solar Arc (the sun's arc across the zenith sky)
        private static readonly ComboPhase Phase1_SolarArc = new ComboPhase(
            curves: new CurveSegment[]
            {
                new CurveSegment(EasingType.PolyOut, 0f, -1.1f, 0.3f, 2),       // Moderate windup
                new CurveSegment(EasingType.PolyIn, 0.22f, -0.8f, 1.7f, 3),     // Wide sweeping arc
                new CurveSegment(EasingType.PolyOut, 0.78f, 0.9f, 0.15f, 2),    // Slow heat trail
            },
            maxAngle: MathHelper.Pi * 1.5f,
            duration: 24,
            bladeLength: 110f,
            flip: true,
            squish: 0.86f,
            damageMult: 1.0f
        );

        // Phase 2 — Zenith Slam (the sun at its peak — maximum devastation)
        private static readonly ComboPhase Phase2_ZenithSlam = new ComboPhase(
            curves: new CurveSegment[]
            {
                new CurveSegment(EasingType.PolyOut, 0f, -1.3f, 0.4f, 2),       // Heavy windup
                new CurveSegment(EasingType.PolyIn, 0.28f, -0.9f, 2.0f, 4),     // Devastating slam
                new CurveSegment(EasingType.PolyOut, 0.82f, 1.1f, 0.1f, 2),     // Smoldering finish
            },
            maxAngle: MathHelper.Pi * 1.7f,
            duration: 28,
            bladeLength: 120f,
            flip: false,
            squish: 0.82f,
            damageMult: 1.3f
        );

        #endregion

        #region ── Abstract Overrides ──

        protected override ComboPhase[] GetAllPhases() => new ComboPhase[]
        {
            Phase0_ScorchingSlash,
            Phase1_SolarArc,
            Phase2_ZenithSlam,
        };

        protected override Color[] GetPalette() => SummerPalette;

        protected override CalamityStyleTrailRenderer.TrailStyle GetTrailStyle()
            => CalamityStyleTrailRenderer.TrailStyle.Flame;

        protected override string GetSmearTexturePath(int comboStep) => comboStep switch
        {
            0 => "MagnumOpus/Assets/Particles/FlamingArcSwordSlash",
            1 => "MagnumOpus/Assets/Particles/SwordArc6",
            2 => "MagnumOpus/Assets/Particles/SwordArc3",
            _ => "MagnumOpus/Assets/Particles/FlamingArcSwordSlash",
        };

        #endregion

        #region ── Virtual Overrides ──

        protected override SoundStyle GetSwingSound()
        {
            return SoundID.Item60 with
            {
                Pitch = -0.3f + ComboStep * 0.2f,
                Volume = 0.9f,
            };
        }

        protected override int GetInitialDustType() => DustID.SolarFlare;

        protected override int GetSecondaryDustType() => DustID.Enchanted_Gold;

        protected override Texture2D GetBladeTexture()
        {
            return ModContent.Request<Texture2D>("MagnumOpus/Content/Summer/Weapons/ZenithCleaver").Value;
        }

        protected override Vector3 GetLightColor()
        {
            return SunGold.ToVector3() * (0.45f + ComboStep * 0.15f);
        }

        #endregion

        #region ── Combo Specials ──

        protected override void HandleComboSpecials()
        {
            if (_crystalCooldown > 0) _crystalCooldown--;
            if (hasSpawnedSpecial) return;

            // ── Every swing fires a SolarWave at ~55% progress ──
            if (Progression >= 0.55f && !hasSpawnedSpecial)
            {
                hasSpawnedSpecial = true;

                if (Main.myPlayer == Projectile.owner)
                {
                    Vector2 tipPos = GetBladeTipPosition();
                    Vector2 waveVel = SwordDirection * 14f;

                    // Standard SolarWave — half damage
                    Projectile.NewProjectile(
                        Projectile.GetSource_FromThis(),
                        tipPos,
                        waveVel,
                        ModContent.ProjectileType<SolarWave>(),
                        Projectile.damage / 2,
                        Projectile.knockBack * 0.5f,
                        Projectile.owner
                    );

                    // Spawn flash VFX at blade tip
                    for (int i = 0; i < 4; i++)
                    {
                        Dust flare = Dust.NewDustPerfect(
                            tipPos + Main.rand.NextVector2Circular(8f, 8f),
                            DustID.SolarFlare,
                            waveVel.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(2f, 5f),
                            0, SunGold, 1.5f);
                        flare.noGravity = true;
                    }

                    // ── Zenith Strike check — every 7th swing ──
                    SwingCounter++;
                    if (SwingCounter >= 7)
                    {
                        SwingCounter = 0;
                        TriggerZenithStrike(tipPos);
                    }
                }
            }

            // ── Dense solar dust + embers every frame during active swing ──
            if (Progression > 0.10f && Progression < 0.92f)
            {
                Vector2 tipPos = GetBladeTipPosition();
                float bladeLen = CurrentPhase.BladeLength;

                // Solar flare dust — 2 per frame (dense, fiery)
                for (int i = 0; i < 2; i++)
                {
                    Vector2 dustPos = Owner.MountedCenter + SwordDirection * bladeLen * Main.rand.NextFloat(0.35f, 1f);
                    Dust d = Dust.NewDustPerfect(dustPos, DustID.SolarFlare,
                        -SwordDirection * Main.rand.NextFloat(1f, 3.5f) + Main.rand.NextVector2Circular(1.2f, 1.2f),
                        0, SunOrange, 1.5f);
                    d.noGravity = true;
                    d.fadeIn = 1.2f;
                }

                // Contrasting gold sparkle every other frame
                if (Main.GameUpdateCount % 2 == 0)
                {
                    Dust g = Dust.NewDustPerfect(
                        Owner.MountedCenter + SwordDirection * bladeLen * Main.rand.NextFloat(0.45f, 0.95f),
                        DustID.Enchanted_Gold,
                        -SwordDirection * Main.rand.NextFloat(0.5f, 2.5f),
                        0, SunGold, 1.3f);
                    g.noGravity = true;
                }

                // White-hot sparkles (1-in-3)
                if (Main.rand.NextBool(3))
                {
                    Dust sparkle = Dust.NewDustPerfect(
                        Owner.MountedCenter + SwordDirection * bladeLen * Main.rand.NextFloat(0.6f, 1f),
                        DustID.FireworkFountain_Yellow,
                        -SwordDirection * Main.rand.NextFloat(1f, 4f) + Main.rand.NextVector2Circular(1.5f, 1.5f),
                        0, SunWhite, 1.2f);
                    sparkle.noGravity = true;
                }

                // Solar shimmer trail — hslToRgb for iridescent heat
                if (Main.rand.NextBool(3))
                {
                    float hue = Main.rand.NextFloat(0.10f, 0.16f);
                    Color shimmerColor = Main.hslToRgb(hue, 0.95f, 0.7f);
                    Dust shimmer = Dust.NewDustPerfect(
                        Owner.MountedCenter + SwordDirection * bladeLen * Main.rand.NextFloat(0.3f, 0.85f),
                        DustID.SolarFlare,
                        -SwordDirection * Main.rand.NextFloat(0.5f, 2f) + Main.rand.NextVector2Circular(0.8f, 0.8f),
                        0, shimmerColor, 1.4f);
                    shimmer.noGravity = true;
                }

                // Heat embers drifting upward (1-in-4)
                if (Main.rand.NextBool(4))
                {
                    Color emberColor = Color.Lerp(SunOrange, SunRed, Main.rand.NextFloat());
                    Dust ember = Dust.NewDustPerfect(
                        tipPos + Main.rand.NextVector2Circular(12f, 12f),
                        DustID.Torch,
                        new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-3f, -1f)),
                        0, emberColor, 1.3f);
                    ember.noGravity = true;
                }

                // Music notes from blade tip (1-in-6, visible scale)
                if (Main.rand.NextBool(6))
                {
                    float noteScale = Main.rand.NextFloat(0.7f, 0.95f);
                    float shimmer = 1f + MathF.Sin(Main.GameUpdateCount * 0.15f) * 0.12f;
                    Dust note = Dust.NewDustPerfect(
                        tipPos + Main.rand.NextVector2Circular(6f, 6f),
                        DustID.Enchanted_Gold,
                        -SwordDirection * 1.5f + new Vector2(0, -Main.rand.NextFloat(0.5f, 2f)),
                        0, SunGold, noteScale * shimmer * 1.6f);
                    note.noGravity = true;
                }

                // Dynamic pulsing light
                Lighting.AddLight(tipPos, SunGold.ToVector3() * (0.5f + ComboStep * 0.15f));
            }
        }

        /// <summary>
        /// Zenith Strike: fires ZenithFlare + 8 radial SolarWaves — the climactic chord.
        /// </summary>
        private void TriggerZenithStrike(Vector2 tipPos)
        {
            SoundEngine.PlaySound(SoundID.Item45 with { Pitch = 0.2f, Volume = 1.1f }, tipPos);

            // Central ZenithFlare projectile — double damage
            Projectile.NewProjectile(
                Projectile.GetSource_FromThis(),
                tipPos,
                SwordDirection * 16f,
                ModContent.ProjectileType<ZenithFlare>(),
                Projectile.damage * 2,
                Projectile.knockBack * 2f,
                Projectile.owner
            );

            // 8 radial SolarWaves — third damage
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(10f, 14f);
                Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    tipPos,
                    vel,
                    ModContent.ProjectileType<SolarWave>(),
                    Projectile.damage / 3,
                    Projectile.knockBack * 0.3f,
                    Projectile.owner
                );
            }

            // ── Zenith Strike VFX ──
            // Central white flash
            for (int i = 0; i < 6; i++)
            {
                Dust flash = Dust.NewDustPerfect(tipPos, DustID.SolarFlare,
                    Main.rand.NextVector2Circular(6f, 6f), 0, SunWhite, 2.0f);
                flash.noGravity = true;
            }

            // Golden burst
            for (int i = 0; i < 10; i++)
            {
                float angle = MathHelper.TwoPi * i / 10f;
                Vector2 burstVel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 8f);
                Dust burst = Dust.NewDustPerfect(tipPos, DustID.Enchanted_Gold,
                    burstVel, 0, SunGold, 1.8f);
                burst.noGravity = true;
            }

            // Halo rings — gold to red gradient
            for (int ring = 0; ring < 5; ring++)
            {
                float progress = ring / 5f;
                Color ringColor = Color.Lerp(SunGold, SunRed, progress);
                for (int j = 0; j < 4; j++)
                {
                    float angle = MathHelper.TwoPi * j / 4f + ring * MathHelper.PiOver4 * 0.5f;
                    Vector2 offset = angle.ToRotationVector2() * (20f + ring * 12f);
                    Dust halo = Dust.NewDustPerfect(tipPos + offset, DustID.SolarFlare,
                        offset.SafeNormalize(Vector2.Zero) * 2.5f, 0, ringColor, 1.4f);
                    halo.noGravity = true;
                }
            }
        }

        #endregion

        #region ── On Hit NPC ──

        protected override void OnSwingHitNPC(NPC target, NPC.HitInfo hit, int remainingDamageCount)
        {
            Player owner = Main.player[Projectile.owner];

            // ── Sunstroke Debuffs ──
            target.AddBuff(BuffID.OnFire3, 180);   // Hellfire 3 seconds
            target.AddBuff(BuffID.Daybreak, 120);   // Daybreak 2 seconds

            // ── Seeking Summer Crystals — 2-3 crystals at 30% damage (30-frame cooldown) ──
            if (_crystalCooldown <= 0)
            {
                if (Main.myPlayer == Projectile.owner)
                {
                    SeekingCrystalHelper.SpawnSummerCrystals(
                        Projectile.GetSource_FromThis(),
                        target.Center,
                        (target.Center - owner.Center).SafeNormalize(Vector2.UnitY) * 5f,
                        (int)(Projectile.damage * 0.3f),
                        Projectile.knockBack * 0.3f,
                        Projectile.owner,
                        count: 2 + Main.rand.Next(2)
                    );
                }
                _crystalCooldown = 30;
            }

            // ── Impact VFX Layers ──

            // Gradient halo rings — SunGold → SunRed
            for (int i = 0; i < 4; i++)
            {
                float progress = i / 4f;
                Color ringColor = Color.Lerp(SunGold, SunRed, progress);
                for (int j = 0; j < 3; j++)
                {
                    float angle = MathHelper.TwoPi * j / 3f + i * MathHelper.PiOver4;
                    Vector2 offset = angle.ToRotationVector2() * (12f + i * 10f);
                    Dust ring = Dust.NewDustPerfect(target.Center + offset, DustID.SolarFlare,
                        offset.SafeNormalize(Vector2.Zero) * 2f, 0, ringColor, 1.4f);
                    ring.noGravity = true;
                }
            }

            // Solar shimmer flares — hslToRgb 0.08-0.18
            for (int i = 0; i < 6; i++)
            {
                float progress = i / 6f;
                float hue = 0.08f + progress * 0.10f;
                Color shimmerColor = Main.hslToRgb(hue, 1f, 0.75f);
                float angle = MathHelper.TwoPi * i / 6f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 6f);
                Dust shimmer = Dust.NewDustPerfect(target.Center + Main.rand.NextVector2Circular(8f, 8f),
                    DustID.Enchanted_Gold, vel, 0, shimmerColor, 1.5f);
                shimmer.noGravity = true;
            }

            // Dust explosion — SolarFlare + Enchanted_Gold radial burst
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 7f);
                int dustType = i % 2 == 0 ? DustID.SolarFlare : DustID.Enchanted_Gold;
                Dust burst = Dust.NewDustPerfect(target.Center, dustType,
                    vel, 0, Color.Lerp(SunGold, SunOrange, Main.rand.NextFloat()), 1.5f);
                burst.noGravity = true;
            }

            // White-hot sparkles
            for (int i = 0; i < 4; i++)
            {
                Dust sparkle = Dust.NewDustPerfect(
                    target.Center + Main.rand.NextVector2Circular(10f, 10f),
                    DustID.FireworkFountain_Yellow,
                    Main.rand.NextVector2Circular(4f, 4f),
                    0, SunWhite, 1.4f);
                sparkle.noGravity = true;
            }

            // Ember burst
            for (int i = 0; i < 5; i++)
            {
                Color emberColor = Color.Lerp(SunOrange, SunRed, Main.rand.NextFloat());
                Dust ember = Dust.NewDustPerfect(target.Center, DustID.Torch,
                    Main.rand.NextVector2Circular(5f, 5f) + new Vector2(0, -Main.rand.NextFloat(1f, 3f)),
                    0, emberColor, 1.3f);
                ember.noGravity = true;
            }

            // Music notes — scattered from impact (1-in-2 per hit, visible)
            for (int i = 0; i < 3; i++)
            {
                float noteAngle = MathHelper.TwoPi * i / 3f + Main.rand.NextFloat(-0.3f, 0.3f);
                Vector2 noteVel = noteAngle.ToRotationVector2() * Main.rand.NextFloat(2f, 4f);
                Dust note = Dust.NewDustPerfect(
                    target.Center + Main.rand.NextVector2Circular(8f, 8f),
                    DustID.Enchanted_Gold,
                    noteVel + new Vector2(0, -Main.rand.NextFloat(0.5f, 2f)),
                    0, SunGold, Main.rand.NextFloat(0.8f, 1.1f) * 1.5f);
                note.noGravity = true;
            }

            // Bright lighting on impact
            Lighting.AddLight(target.Center, SunGold.ToVector3() * 1.2f);
        }

        #endregion

        #region ── Custom VFX ──

        protected override void DrawCustomVFX(SpriteBatch sb)
        {
            if (Progression < 0.08f || Progression > 0.95f) return;

            Vector2 tipWorld = GetBladeTipPosition();
            float pulse = 1f + MathF.Sin(Main.GlobalTimeWrappedHourly * 6f) * 0.10f;
            float tipScale = (0.20f + ComboStep * 0.07f) * pulse;

            BloomRenderer.DrawBloomStackAdditive(tipWorld, SunGold, SunRed, tipScale, 0.90f);

            if (Main.rand.NextBool(4))
            {
                Vector2 noteVel = -SwordDirection * Main.rand.NextFloat(0.5f, 1.5f);
                MagnumParticleHandler.SpawnParticle(new HueShiftingMusicNoteParticle(
                    tipWorld, noteVel,
                    hueMin: 0.04f, hueMax: 0.14f,
                    saturation: 0.95f, luminosity: 0.55f,
                    scale: 0.75f, lifetime: 25, hueSpeed: 0.025f));
            }
        }

        #endregion
    }
}
