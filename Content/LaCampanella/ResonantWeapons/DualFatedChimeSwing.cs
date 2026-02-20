using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.BaseClasses;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.VFX.Trails;
using MagnumOpus.Content.LaCampanella.Debuffs;
using static MagnumOpus.Common.Systems.Particles.Particle;

namespace MagnumOpus.Content.LaCampanella.ResonantWeapons
{
    public sealed class DualFatedChimeSwing : MeleeSwingBase
    {
        #region Theme Colors

        private static readonly Color InfernalBlack = MagnumThemePalettes.InfernalBlack;
        private static readonly Color InfernalDeepOrange = MagnumThemePalettes.InfernalDeepOrange;
        private static readonly Color InfernalOrange = MagnumThemePalettes.InfernalOrange;
        private static readonly Color InfernalBright = MagnumThemePalettes.InfernalBright;
        private static readonly Color InfernalGold = MagnumThemePalettes.InfernalGold;
        private static readonly Color InfernalWhiteHot = MagnumThemePalettes.InfernalWhiteHot;

        private static readonly Color[] InfernalPalette = new Color[]
        {
            InfernalBlack,
            InfernalDeepOrange,
            InfernalOrange,
            InfernalBright,
            InfernalGold,
            InfernalWhiteHot
        };

        #endregion

        #region Combo Phases

        // Phase 0: Swift bell chime strike — fast opener
        private static readonly ComboPhase Phase0_BellStrike = new ComboPhase(
            new CurveSegment[]
            {
                new CurveSegment(EasingType.SineOut, 0f, -0.9f, 0.2f, 2),
                new CurveSegment(EasingType.PolyIn, 0.20f, -0.7f, 1.55f, 3),
                new CurveSegment(EasingType.PolyOut, 0.80f, 0.85f, 0.15f, 2),
            },
            maxAngle: MathHelper.PiOver2 * 1.4f,
            duration: 18,
            bladeLength: 155f,
            flip: false,
            squish: 0.88f,
            damageMult: 0.9f
        );

        // Phase 1: Tolling sweep — wider arc, alternating direction
        private static readonly ComboPhase Phase1_TollSweep = new ComboPhase(
            new CurveSegment[]
            {
                new CurveSegment(EasingType.PolyOut, 0f, -1f, 0.25f, 2),
                new CurveSegment(EasingType.PolyIn, 0.22f, -0.75f, 1.7f, 3),
                new CurveSegment(EasingType.PolyOut, 0.82f, 0.95f, 0.05f, 2),
            },
            maxAngle: MathHelper.PiOver2 * 1.6f,
            duration: 22,
            bladeLength: 160f,
            flip: true,
            squish: 0.85f,
            damageMult: 1.1f
        );

        // Phase 2: Grand toll — heavy finisher with wide arc and long blade
        private static readonly ComboPhase Phase2_GrandToll = new ComboPhase(
            new CurveSegment[]
            {
                new CurveSegment(EasingType.PolyOut, 0f, -1.1f, 0.3f, 2),
                new CurveSegment(EasingType.PolyIn, 0.28f, -0.8f, 1.85f, 3),
                new CurveSegment(EasingType.PolyOut, 0.85f, 1.05f, -0.05f, 2),
            },
            maxAngle: MathHelper.PiOver2 * 2.0f,
            duration: 26,
            bladeLength: 170f,
            flip: false,
            squish: 0.82f,
            damageMult: 1.35f
        );

        #endregion

        #region Abstract Overrides

        protected override ComboPhase[] GetAllPhases() => new[]
        {
            Phase0_BellStrike,
            Phase1_TollSweep,
            Phase2_GrandToll
        };

        protected override Color[] GetPalette() => InfernalPalette;

        protected override CalamityStyleTrailRenderer.TrailStyle GetTrailStyle()
            => CalamityStyleTrailRenderer.TrailStyle.Flame;

        protected override string GetSmearTexturePath(int comboStep)
        {
            return comboStep switch
            {
                2 => "MagnumOpus/Assets/Particles/FlamingArcSwordSlash",
                _ => "MagnumOpus/Assets/Particles/SwordArc3"
            };
        }

        #endregion

        #region Virtual Overrides

        protected override Texture2D GetBladeTexture()
            => Terraria.GameContent.TextureAssets.Item[ItemID.BreakerBlade].Value;

        protected override SoundStyle GetSwingSound()
            => SoundID.Item71 with { Pitch = -0.1f + ComboStep * 0.08f, Volume = 0.7f };

        protected override int GetInitialDustType() => DustID.Torch;

        protected override int GetSecondaryDustType() => DustID.Smoke;

        protected override Vector3 GetLightColor()
        {
            float intensity = 0.5f + ComboStep * 0.15f;
            return InfernalOrange.ToVector3() * intensity;
        }

        #endregion

        #region Combo Specials

        protected override void HandleComboSpecials()
        {
            if (hasSpawnedSpecial) return;

            // Phase 1 at 65%: Ring of ember sparks from blade tip
            if (ComboStep == 1 && Progression >= 0.65f)
            {
                hasSpawnedSpecial = true;
                Vector2 tipPos = GetBladeTipPosition();

                for (int i = 0; i < 6; i++)
                {
                    float angle = MathHelper.TwoPi * i / 6f;
                    Vector2 sparkVel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 6f);
                    Dust d = Dust.NewDustPerfect(tipPos, DustID.Torch, sparkVel, 0, InfernalOrange, 1.6f);
                    d.noGravity = true;
                }

                try { UnifiedVFX.LaCampanella.Impact(tipPos, 0.6f); } catch { }
                try { ThemedParticles.LaCampanellaMusicNotes(tipPos, 2, 20f); } catch { }
            }

            // Phase 2 at 70%: Fire wave burst + bell chime particles
            if (ComboStep == 2 && Progression >= 0.70f)
            {
                hasSpawnedSpecial = true;
                Vector2 tipPos = GetBladeTipPosition();

                if (Main.myPlayer == Projectile.owner)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        float angle = MathHelper.ToRadians(-30 + i * 30);
                        Vector2 vel = (SwordRotation + angle).ToRotationVector2() * Main.rand.NextFloat(10f, 16f);
                        Projectile.NewProjectile(Projectile.GetSource_FromThis(), tipPos, vel,
                            ModContent.ProjectileType<BellFlameWave>(),
                            Projectile.damage / 3, 2f, Projectile.owner);
                    }
                }

                try { UnifiedVFX.LaCampanella.Explosion(tipPos, 0.8f); } catch { }
                try { ThemedParticles.LaCampanellaBellChime(tipPos, 1.0f); } catch { }
                try { ThemedParticles.LaCampanellaMusicNotes(tipPos, 3, 25f); } catch { }

                for (int i = 0; i < 10; i++)
                {
                    Vector2 dustVel = Main.rand.NextVector2Circular(5f, 5f);
                    Dust d = Dust.NewDustPerfect(tipPos, DustID.Torch, dustVel, 0, InfernalBright, 1.8f);
                    d.noGravity = true;
                }
            }
        }

        #endregion

        #region On Hit

        protected override void OnSwingHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Charge the Inferno Waltz bar
            if (Owner.HeldItem?.ModItem is DualFatedChime chime)
                chime.AddCharge(DualFatedChime.ChargePerHit);

            // Resonant Toll debuff stacks
            try { target.GetGlobalNPC<ResonantTollNPC>().AddStacks(target, 1); } catch { }

            // Chime Cyclone hit tracking
            try { Owner.GetModPlayer<ChimeCyclonePlayer>().AddHit(target); } catch { }

            // === VFX ===
            Vector2 hitPos = target.Center;

            // Gradient halo rings
            for (int i = 0; i < 4 + ComboStep; i++)
            {
                float progress = (float)i / (4 + ComboStep);
                Color ringColor = Color.Lerp(InfernalOrange, InfernalGold, progress);
                try { CustomParticles.HaloRing(hitPos, ringColor, 0.25f + i * 0.08f, 12 + i * 2); } catch { }
            }

            // Radial dust burst
            int dustCount = 8 + ComboStep * 4;
            for (int i = 0; i < dustCount; i++)
            {
                float angle = MathHelper.TwoPi * i / dustCount;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 7f);
                Dust d = Dust.NewDustPerfect(hitPos, DustID.Torch, vel, 0, InfernalOrange, 1.4f);
                d.noGravity = true;
            }

            // Smoke wisps
            for (int i = 0; i < 3; i++)
            {
                Vector2 smokeVel = new Vector2(Main.rand.NextFloat(-2f, 2f), -Main.rand.NextFloat(1f, 3f));
                Dust smoke = Dust.NewDustPerfect(hitPos, DustID.Smoke, smokeVel, 100, InfernalBlack, 1.5f);
                smoke.noGravity = true;
            }

            // Themed particles
            try { UnifiedVFX.LaCampanella.Impact(hitPos, 0.8f + ComboStep * 0.2f); } catch { }
            try { ThemedParticles.LaCampanellaSparks(hitPos, SwordDirection, 3 + ComboStep, 5f); } catch { }
            try { ThemedParticles.LaCampanellaMusicNotes(hitPos, 1 + ComboStep, 15f); } catch { }

            // Seeking crystals on crit
            if (hit.Crit && Main.rand.NextBool(3) && Main.myPlayer == Projectile.owner)
            {
                try
                {
                    SeekingCrystalHelper.SpawnLaCampanellaCrystals(
                        Projectile.GetSource_FromThis(), hitPos, Projectile.velocity,
                        Projectile.damage / 2, Projectile.knockBack, Projectile.owner, 4);
                }
                catch { }
            }

            Lighting.AddLight(hitPos, InfernalOrange.ToVector3() * (0.6f + ComboStep * 0.15f));
        }

        #endregion

        #region Custom VFX

        protected override void DrawCustomVFX(SpriteBatch sb)
        {
            if (Progression <= 0.08f || Progression >= 0.92f) return;

            Vector2 tipPos = GetBladeTipPosition();

            // Dense fire dust trail along blade
            for (int i = 0; i < 2; i++)
            {
                float t = Main.rand.NextFloat(0.4f, 1f);
                Vector2 dustPos = Vector2.Lerp(Owner.MountedCenter, tipPos, t);
                Dust d = Dust.NewDustPerfect(dustPos, DustID.Torch,
                    -SwordDirection * Main.rand.NextFloat(1f, 3f), 0,
                    GetElementColor(Main.rand.NextFloat(0.3f, 0.8f)), 1.6f);
                d.noGravity = true;
            }

            // Smoke wisps along blade
            if (Main.rand.NextBool(3))
            {
                float t = Main.rand.NextFloat(0.5f, 0.9f);
                Vector2 smokePos = Vector2.Lerp(Owner.MountedCenter, tipPos, t);
                Dust smoke = Dust.NewDustPerfect(smokePos, DustID.Smoke,
                    new Vector2(Main.rand.NextFloat(-1f, 1f), -Main.rand.NextFloat(0.5f, 2f)),
                    80, InfernalBlack, 1.3f);
                smoke.noGravity = true;
            }

            // Ember sparkle accents
            if (Main.rand.NextBool(4))
            {
                Vector2 emberPos = tipPos + Main.rand.NextVector2Circular(12f, 12f);
                Dust ember = Dust.NewDustPerfect(emberPos, DustID.FireworkFountain_Yellow,
                    -SwordDirection * Main.rand.NextFloat(0.5f, 2f), 0, InfernalGold, 1.0f);
                ember.noGravity = true;
            }

            // Music notes — infernal chime spectrum (hue-shifting)
            if (Main.rand.NextBool(5))
            {
                Vector2 noteVel = -SwordDirection * 1.2f + new Vector2(0, -0.5f);
                MagnumParticleHandler.SpawnParticle(new HueShiftingMusicNoteParticle(
                    tipPos, noteVel,
                    hueMin: 0.04f, hueMax: 0.14f,
                    saturation: 0.95f, luminosity: 0.55f,
                    scale: 0.75f, lifetime: 25, hueSpeed: 0.02f));
            }

            // Blade-tip bloom — infernal glow
            {
                float bloomOpacity = MathHelper.Clamp((Progression - 0.08f) / 0.12f, 0f, 1f)
                                   * MathHelper.Clamp((0.92f - Progression) / 0.12f, 0f, 1f);
                float bloomScale = 0.45f + ComboStep * 0.08f;
                BloomRenderer.DrawBloomStackAdditive(tipPos, InfernalOrange, InfernalGold, bloomScale, bloomOpacity);
            }
        }

        #endregion
    }
}
