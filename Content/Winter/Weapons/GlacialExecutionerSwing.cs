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
using MagnumOpus.Content.Winter.Projectiles;
using static MagnumOpus.Common.Systems.Particles.Particle;

namespace MagnumOpus.Content.Winter.Weapons
{
    /// <summary>
    /// Glacial Executioner held-projectile swing — winter's merciless sentence.
    /// 4-phase greataxe combo: Frost Cleave → Rime Backhand → Permafrost Slam → Absolute Zero.
    /// 25% freeze chance on hit; always applies Frostburn2; frozen enemies take 30% bonus;
    /// Phase 3 finisher spawns AvalancheWave; Phase 2 spawns ice bolt sub-projectiles.
    /// </summary>
    public sealed class GlacialExecutionerSwing : MeleeSwingBase
    {
        // ── Theme Colors ──
        private static readonly Color IceBlue = MagnumThemePalettes.WinterIceBlue;
        private static readonly Color FrostWhite = MagnumThemePalettes.WinterFrostPure;
        private static readonly Color DeepBlue = MagnumThemePalettes.WinterDeepBlue;
        private static readonly Color CrystalCyan = MagnumThemePalettes.WinterCrystalCyan;

        // ── 6-Color Palette: pianissimo → sforzando ──
        private static readonly Color[] WinterPalette = new Color[]
        {
            new Color(30, 50, 100),     // [0] Deep ocean shadow
            new Color(60, 100, 180),    // [1] Deep blue
            new Color(150, 220, 255),   // [2] Ice blue
            new Color(100, 255, 255),   // [3] Crystal cyan
            new Color(200, 240, 255),   // [4] Pale frost glow
            new Color(240, 250, 255),   // [5] White-hot frost core
        };

        #region ── Combo Phase Definitions ──

        // Phase 0 — Frost Cleave (heavy horizontal — the first breath of winter)
        private static readonly ComboPhase Phase0_FrostCleave = new ComboPhase(
            curves: new CurveSegment[]
            {
                new CurveSegment(EasingType.PolyOut, 0f, -0.9f, 0.22f, 2),      // Heavy pull-back
                new CurveSegment(EasingType.PolyIn, 0.22f, -0.68f, 1.5f, 3),    // Crushing cleave
                new CurveSegment(EasingType.PolyOut, 0.80f, 0.82f, 0.18f, 2),   // Weight settle
            },
            maxAngle: MathHelper.Pi * 1.3f,
            duration: 32,
            bladeLength: 135f,
            flip: false,
            squish: 0.86f,
            damageMult: 0.95f
        );

        // Phase 1 — Rime Backhand (quick reverse — biting frost wind)
        private static readonly ComboPhase Phase1_RimeBackhand = new ComboPhase(
            curves: new CurveSegment[]
            {
                new CurveSegment(EasingType.PolyOut, 0f, -0.7f, 0.15f, 2),      // Short prep
                new CurveSegment(EasingType.PolyIn, 0.15f, -0.55f, 1.4f, 3),    // Swift backhand
                new CurveSegment(EasingType.PolyOut, 0.70f, 0.85f, 0.15f, 2),   // Snap finish
            },
            maxAngle: MathHelper.Pi * 1.2f,
            duration: 26,
            bladeLength: 125f,
            flip: true,
            squish: 0.90f,
            damageMult: 1.0f
        );

        // Phase 2 — Permafrost Slam (overhead — the ice age descends)
        private static readonly ComboPhase Phase2_PermafrostSlam = new ComboPhase(
            curves: new CurveSegment[]
            {
                new CurveSegment(EasingType.PolyOut, 0f, -1.2f, 0.30f, 2),      // Deep overhead lift
                new CurveSegment(EasingType.PolyIn, 0.28f, -0.9f, 1.8f, 4),     // Devastating slam
                new CurveSegment(EasingType.PolyOut, 0.84f, 0.9f, 0.12f, 2),    // Impact shudder
            },
            maxAngle: MathHelper.Pi * 1.5f,
            duration: 34,
            bladeLength: 145f,
            flip: false,
            squish: 0.82f,
            damageMult: 1.2f
        );

        // Phase 3 — Absolute Zero (massive finisher — the world goes still)
        private static readonly ComboPhase Phase3_AbsoluteZero = new ComboPhase(
            curves: new CurveSegment[]
            {
                new CurveSegment(EasingType.PolyOut, 0f, -1.4f, 0.38f, 2),      // Massive windup
                new CurveSegment(EasingType.PolyIn, 0.32f, -1.02f, 2.1f, 4),    // World-ending arc
                new CurveSegment(EasingType.PolyOut, 0.90f, 1.08f, 0.08f, 2),   // Frozen overshoot
            },
            maxAngle: MathHelper.Pi * 1.9f,
            duration: 40,
            bladeLength: 155f,
            flip: true,
            squish: 0.80f,
            damageMult: 1.4f
        );

        #endregion

        #region ── Abstract Overrides ──

        protected override ComboPhase[] GetAllPhases() => new ComboPhase[]
        {
            Phase0_FrostCleave,
            Phase1_RimeBackhand,
            Phase2_PermafrostSlam,
            Phase3_AbsoluteZero,
        };

        protected override Color[] GetPalette() => WinterPalette;

        protected override CalamityStyleTrailRenderer.TrailStyle GetTrailStyle()
            => CalamityStyleTrailRenderer.TrailStyle.Ice;

        protected override string GetSmearTexturePath(int comboStep) => comboStep switch
        {
            0 => "MagnumOpus/Assets/Particles/SwordArc1",
            1 => "MagnumOpus/Assets/Particles/SimpleArcSwordSlash",
            2 => "MagnumOpus/Assets/Particles/SwordArc6",
            3 => "MagnumOpus/Assets/Particles/SwordArc8",
            _ => "MagnumOpus/Assets/Particles/SwordArc1",
        };

        #endregion

        #region ── Virtual Overrides ──

        protected override SoundStyle GetSwingSound()
        {
            return SoundID.Item1 with
            {
                Pitch = -0.4f + ComboStep * 0.1f,
                Volume = 0.9f,
            };
        }

        protected override int GetInitialDustType() => DustID.IceTorch;

        protected override int GetSecondaryDustType() => DustID.Frost;

        protected override Texture2D GetBladeTexture()
        {
            return ModContent.Request<Texture2D>("MagnumOpus/Content/Winter/Weapons/GlacialExecutioner").Value;
        }

        protected override Vector3 GetLightColor()
        {
            return IceBlue.ToVector3() * (0.45f + ComboStep * 0.10f);
        }

        #endregion

        #region ── Combo Specials ──

        protected override void HandleComboSpecials()
        {
            if (hasSpawnedSpecial) return;

            // Phase 2 at ~70%: spawn icicle bolt sub-projectiles at blade tip
            if (ComboStep == 2 && Progression >= 0.70f)
            {
                hasSpawnedSpecial = true;
                if (Main.myPlayer == Projectile.owner)
                {
                    Vector2 tipPos = GetBladeTipPosition();
                    for (int i = 0; i < 5; i++)
                    {
                        float spread = MathHelper.ToRadians(-40f + i * 20f);
                        Vector2 vel = SwordDirection.RotatedBy(spread) * Main.rand.NextFloat(8f, 12f);
                        Projectile.NewProjectile(
                            Projectile.GetSource_FromThis(),
                            tipPos,
                            vel,
                            ModContent.ProjectileType<IcicleBolt>(),
                            Projectile.damage / 3,
                            Projectile.knockBack * 0.4f,
                            Projectile.owner
                        );
                    }
                }
            }

            // Phase 3 at ~85%: Absolute Zero finisher — spawn AvalancheWave
            if (ComboStep == 3 && Progression >= 0.85f)
            {
                hasSpawnedSpecial = true;
                if (Main.myPlayer == Projectile.owner)
                {
                    Vector2 tipPos = GetBladeTipPosition();
                    Vector2 waveVel = SwordDirection * 16f;

                    Projectile.NewProjectile(
                        Projectile.GetSource_FromThis(),
                        tipPos,
                        waveVel,
                        ModContent.ProjectileType<AvalancheWave>(),
                        (int)(Projectile.damage * 1.5f),
                        Projectile.knockBack,
                        Projectile.owner
                    );

                    // Seeking winter crystals
                    SeekingCrystalHelper.SpawnWinterCrystals(
                        Projectile.GetSource_FromThis(),
                        tipPos,
                        SwordDirection * 6f,
                        (int)(Projectile.damage * 0.35f),
                        Projectile.knockBack * 0.3f,
                        Projectile.owner,
                        count: 4
                    );

                    // Absolute Zero VFX burst
                    SoundEngine.PlaySound(SoundID.Item120 with { Pitch = -0.5f, Volume = 1f }, tipPos);
                }
            }

            // ── Dense dust + frost particles every frame during active swing ──
            if (Progression > 0.10f && Progression < 0.92f)
            {
                Vector2 tipPos = GetBladeTipPosition();
                float bladeLen = CurrentPhase.BladeLength;

                // Ice torch dust — dense, 2 per frame
                for (int i = 0; i < 2; i++)
                {
                    Vector2 dustPos = Owner.MountedCenter + SwordDirection * bladeLen * Main.rand.NextFloat(0.4f, 1f);
                    Dust d = Dust.NewDustPerfect(dustPos, DustID.IceTorch,
                        -SwordDirection * Main.rand.NextFloat(1f, 3f) + Main.rand.NextVector2Circular(1f, 1f),
                        0, IceBlue, 1.3f);
                    d.noGravity = true;
                    d.fadeIn = 1.2f;
                }

                // Contrasting frost sparkle every other frame
                if (Main.GameUpdateCount % 2 == 0)
                {
                    Dust g = Dust.NewDustPerfect(
                        Owner.MountedCenter + SwordDirection * bladeLen * Main.rand.NextFloat(0.5f, 0.9f),
                        DustID.Frost,
                        -SwordDirection * Main.rand.NextFloat(0.5f, 2f),
                        0, FrostWhite, 1.1f);
                    g.noGravity = true;
                }

                // Crystal shard sparkles (1-in-3)
                if (Main.rand.NextBool(3))
                {
                    Vector2 shardPos = Owner.MountedCenter + SwordDirection * bladeLen * Main.rand.NextFloat(0.3f, 1f);
                    Dust shard = Dust.NewDustPerfect(shardPos, DustID.BlueCrystalShard,
                        -SwordDirection * Main.rand.NextFloat(1f, 2.5f) + Main.rand.NextVector2Circular(0.5f, 0.5f),
                        0, CrystalCyan, 0.9f);
                    shard.noGravity = true;
                }

                // Music notes from blade tip (1-in-4 chance, visible scale)
                if (Main.rand.NextBool(4))
                {
                    float noteScale = Main.rand.NextFloat(0.7f, 0.95f);
                    float shimmer = 1f + MathF.Sin(Main.GameUpdateCount * 0.15f) * 0.12f;
                    Color noteColor = Color.Lerp(IceBlue, CrystalCyan, Main.rand.NextFloat());
                    Dust note = Dust.NewDustPerfect(
                        tipPos + Main.rand.NextVector2Circular(6f, 6f),
                        DustID.BlueCrystalShard,
                        -SwordDirection * 1.5f + Main.rand.NextVector2Circular(1f, 1f),
                        0, noteColor, noteScale * shimmer * 1.6f);
                    note.noGravity = true;
                }
            }
        }

        #endregion

        #region ── On Hit NPC ──

        protected override void OnSwingHitNPC(NPC target, NPC.HitInfo hit, int remainingDamageCount)
        {
            Player owner = Main.player[Projectile.owner];

            // Always apply Frostburn2
            target.AddBuff(BuffID.Frostburn2, 240);

            // ── Absolute Zero: 25% freeze chance ──
            if (Main.rand.NextFloat() < 0.25f)
            {
                target.AddBuff(BuffID.Frozen, 90);

                if (Main.myPlayer == Projectile.owner)
                {
                    SeekingCrystalHelper.SpawnWinterCrystals(
                        Projectile.GetSource_FromThis(),
                        target.Center,
                        (target.Center - owner.Center).SafeNormalize(Vector2.Zero) * 5f,
                        (int)(Projectile.damage * 0.35f),
                        Projectile.knockBack * 0.3f,
                        Projectile.owner,
                        count: 5
                    );
                }

                // Freeze VFX flash
                SoundEngine.PlaySound(SoundID.Item27 with { Pitch = 0.3f, Volume = 0.7f }, target.Center);
                for (int i = 0; i < 8; i++)
                {
                    Dust freeze = Dust.NewDustPerfect(target.Center, DustID.Frost,
                        Main.rand.NextVector2Circular(6f, 6f), 0, CrystalCyan, 1.6f);
                    freeze.noGravity = true;
                }
            }

            // ── Gradient halo rings — deep blue → frost white ──
            for (int i = 0; i < 4; i++)
            {
                float progress = i / 4f;
                Color ringColor = Color.Lerp(DeepBlue, FrostWhite, progress);
                for (int j = 0; j < 2; j++)
                {
                    float angle = MathHelper.TwoPi * j / 2f + i * MathHelper.PiOver4;
                    Vector2 offset = angle.ToRotationVector2() * (15f + i * 8f);
                    Dust ring = Dust.NewDustPerfect(target.Center + offset, DustID.IceTorch,
                        offset.SafeNormalize(Vector2.Zero) * 2f, 0, ringColor, 1.3f);
                    ring.noGravity = true;
                }
            }

            // Crystal sparkle flares
            for (int i = 0; i < 3; i++)
            {
                Dust sparkle = Dust.NewDustPerfect(target.Center + Main.rand.NextVector2Circular(14f, 14f),
                    DustID.BlueCrystalShard,
                    Main.rand.NextVector2Circular(3f, 3f), 0, CrystalCyan, 1.0f);
                sparkle.noGravity = true;
            }

            // Radial ice dust burst
            for (int i = 0; i < 10; i++)
            {
                Dust burst = Dust.NewDustPerfect(target.Center, DustID.IceTorch,
                    Main.rand.NextVector2Circular(5f, 5f), 0,
                    Color.Lerp(IceBlue, FrostWhite, Main.rand.NextFloat()), 1.3f);
                burst.noGravity = true;
                burst.fadeIn = 1.2f;
            }

            // Ice shard glow particles
            for (int i = 0; i < 4; i++)
            {
                Vector2 sparkVel = Main.rand.NextVector2Circular(4f, 4f);
                Color sparkColor = Color.Lerp(IceBlue, FrostWhite, Main.rand.NextFloat()) * 0.6f;
                var spark = new GenericGlowParticle(target.Center, sparkVel, sparkColor, 0.30f, 20, true);
                MagnumParticleHandler.SpawnParticle(spark);
            }

            // Music note burst on hit
            for (int n = 0; n < 3; n++)
            {
                float angle = MathHelper.TwoPi * n / 3f + Main.rand.NextFloat(-0.4f, 0.4f);
                Vector2 noteVel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 4f);
                Color noteColor = Color.Lerp(IceBlue, CrystalCyan, Main.rand.NextFloat());
                Dust note = Dust.NewDustPerfect(target.Center, DustID.BlueCrystalShard,
                    noteVel, 0, noteColor, Main.rand.NextFloat(1.4f, 1.8f));
                note.noGravity = true;
            }
        }

        #endregion

        #region ── Custom VFX ──

        protected override void DrawCustomVFX(SpriteBatch sb)
        {
            if (Progression < 0.08f || Progression > 0.95f) return;

            Vector2 tipWorld = GetBladeTipPosition();
            float pulse = 1f + MathF.Sin(Main.GlobalTimeWrappedHourly * 5f) * 0.10f;
            float tipScale = (0.20f + ComboStep * 0.06f) * pulse;

            BloomRenderer.DrawBloomStackAdditive(tipWorld, IceBlue, CrystalCyan, tipScale, 0.85f);

            if (Main.rand.NextBool(4))
            {
                Vector2 noteVel = (Projectile.velocity.SafeNormalize(Vector2.UnitX) * -1.5f).RotatedByRandom(0.4);
                MagnumParticleHandler.SpawnParticle(new HueShiftingMusicNoteParticle(
                    tipWorld, noteVel, 0.52f, 0.68f, 0.80f, 0.65f, 0.75f, 25, 0.025f));
            }
        }

        #endregion
    }
}
