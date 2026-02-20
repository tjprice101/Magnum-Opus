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
using static MagnumOpus.Common.Systems.Particles.Particle;

namespace MagnumOpus.Content.Spring.Weapons
{
    /// <summary>
    /// Blossom's Edge held-projectile swing — the gentle first movement of spring.
    /// 3-phase cherry blossom combo: Quick Slash → Wide Arc → Flourishing Thrust.
    /// Each swing scatters petals; every 5th hit triggers Renewal Strike (8 HP heal);
    /// crits trigger Spring Bloom (seeking crystals + AoE petal burst).
    /// </summary>
    public sealed class BlossomsEdgeSwing : MeleeSwingBase
    {
        // ── Theme Colors ──
        private static readonly Color SpringPink = MagnumThemePalettes.SpringPink;
        private static readonly Color SpringWhite = MagnumThemePalettes.SpringWhite;
        private static readonly Color SpringGreen = MagnumThemePalettes.SpringLightGreen;
        private static readonly Color CherryBlossom = MagnumThemePalettes.SpringPink;

        // ── Hit Counter (stored in ai[2]) ──
        private int HitCounter
        {
            get => (int)Projectile.ai[2];
            set => Projectile.ai[2] = value;
        }

        // ── 6-Color Palette: pianissimo → sforzando ──
        private static readonly Color[] SpringPalette = new Color[]
        {
            new Color(120, 80, 100),    // [0] Deep rose shadow
            new Color(180, 120, 150),   // [1] Dusk rose
            new Color(255, 183, 197),   // [2] Cherry blossom pink
            new Color(200, 255, 200),   // [3] Fresh spring green
            new Color(255, 230, 210),   // [4] Warm petal glow
            new Color(255, 250, 250),   // [5] White-hot petal core
        };

        #region ── Combo Phase Definitions ──

        // Phase 0 — Quick Slash (the breath of new petals)
        private static readonly ComboPhase Phase0_QuickSlash = new ComboPhase(
            curves: new CurveSegment[]
            {
                new CurveSegment(EasingType.PolyOut, 0f, -0.8f, 0.2f, 2),       // Small windup
                new CurveSegment(EasingType.PolyIn, 0.20f, -0.6f, 1.4f, 3),     // Quick slash
                new CurveSegment(EasingType.PolyOut, 0.75f, 0.8f, 0.2f, 2),     // Gentle follow-through
            },
            maxAngle: MathHelper.Pi * 1.1f,
            duration: 22,
            bladeLength: 90f,
            flip: false,
            squish: 0.92f,
            damageMult: 0.9f
        );

        // Phase 1 — Wide Arc (the blossom in full bloom)
        private static readonly ComboPhase Phase1_WideArc = new ComboPhase(
            curves: new CurveSegment[]
            {
                new CurveSegment(EasingType.PolyOut, 0f, -1f, 0.3f, 2),         // Moderate windup
                new CurveSegment(EasingType.PolyIn, 0.25f, -0.7f, 1.6f, 3),     // Sweeping arc
                new CurveSegment(EasingType.PolyOut, 0.80f, 0.9f, 0.1f, 2),     // Slow finish
            },
            maxAngle: MathHelper.Pi * 1.4f,
            duration: 26,
            bladeLength: 100f,
            flip: true,
            squish: 0.88f,
            damageMult: 1.0f
        );

        // Phase 2 — Flourishing Thrust (petals scatter in the wind)
        private static readonly ComboPhase Phase2_Flourish = new ComboPhase(
            curves: new CurveSegment[]
            {
                new CurveSegment(EasingType.PolyOut, 0f, -1.2f, 0.35f, 2),      // Deep windup
                new CurveSegment(EasingType.PolyIn, 0.30f, -0.85f, 1.85f, 4),   // Powerful thrust
                new CurveSegment(EasingType.PolyOut, 0.85f, 1.0f, 0.15f, 2),    // Lingering finish
            },
            maxAngle: MathHelper.Pi * 1.6f,
            duration: 28,
            bladeLength: 110f,
            flip: false,
            squish: 0.85f,
            damageMult: 1.2f
        );

        #endregion

        #region ── Abstract Overrides ──

        protected override ComboPhase[] GetAllPhases() => new ComboPhase[]
        {
            Phase0_QuickSlash,
            Phase1_WideArc,
            Phase2_Flourish,
        };

        protected override Color[] GetPalette() => SpringPalette;

        protected override CalamityStyleTrailRenderer.TrailStyle GetTrailStyle()
            => CalamityStyleTrailRenderer.TrailStyle.Nature;

        protected override string GetSmearTexturePath(int comboStep) => comboStep switch
        {
            0 => "MagnumOpus/Assets/Particles/SimpleArcSwordSlash",
            1 => "MagnumOpus/Assets/Particles/CurvedSwordSlash",
            2 => "MagnumOpus/Assets/Particles/SwordArc2",
            _ => "MagnumOpus/Assets/Particles/SimpleArcSwordSlash",
        };

        #endregion

        #region ── Virtual Overrides ──

        protected override Terraria.Audio.SoundStyle GetSwingSound()
        {
            return SoundID.Item71 with
            {
                Pitch = -0.2f + ComboStep * 0.15f,
                Volume = 0.8f,
            };
        }

        protected override int GetInitialDustType() => DustID.PinkFairy;

        protected override int GetSecondaryDustType() => DustID.GreenFairy;

        protected override Microsoft.Xna.Framework.Graphics.Texture2D GetBladeTexture()
        {
            return ModContent.Request<Texture2D>("MagnumOpus/Content/Spring/Weapons/BlossomsEdge").Value;
        }

        protected override Vector3 GetLightColor()
        {
            return SpringPink.ToVector3() * (0.35f + ComboStep * 0.12f);
        }

        #endregion

        #region ── Combo Specials ──

        protected override void HandleComboSpecials()
        {
            if (hasSpawnedSpecial) return;

            // Phase 1 at ~65%: scatter BlossomPetal projectiles at blade tip
            if (ComboStep == 1 && Progression >= 0.65f)
            {
                hasSpawnedSpecial = true;
                if (Main.myPlayer == Projectile.owner)
                {
                    Vector2 tipPos = GetBladeTipPosition();
                    int petalCount = 2 + Main.rand.Next(2); // 2-3
                    for (int i = 0; i < petalCount; i++)
                    {
                        float spread = MathHelper.ToRadians(-25f + i * (50f / Math.Max(petalCount - 1, 1)));
                        Vector2 vel = SwordDirection.RotatedBy(spread) * Main.rand.NextFloat(6f, 9f);
                        Projectile.NewProjectile(
                            Projectile.GetSource_FromThis(),
                            tipPos,
                            vel,
                            ModContent.ProjectileType<Content.Spring.Projectiles.BlossomPetal>(),
                            Projectile.damage / 3,
                            Projectile.knockBack * 0.5f,
                            Projectile.owner
                        );
                    }
                }
            }

            // Phase 2 at ~75%: larger petal burst + seeking crystals
            if (ComboStep == 2 && Progression >= 0.75f)
            {
                hasSpawnedSpecial = true;
                if (Main.myPlayer == Projectile.owner)
                {
                    Vector2 tipPos = GetBladeTipPosition();

                    // Petal burst
                    for (int i = 0; i < 4; i++)
                    {
                        float angle = MathHelper.ToRadians(-40f + i * 27f);
                        Vector2 vel = SwordDirection.RotatedBy(angle) * Main.rand.NextFloat(7f, 11f);
                        Projectile.NewProjectile(
                            Projectile.GetSource_FromThis(),
                            tipPos,
                            vel,
                            ModContent.ProjectileType<Content.Spring.Projectiles.BlossomPetal>(),
                            Projectile.damage / 3,
                            Projectile.knockBack * 0.5f,
                            Projectile.owner
                        );
                    }

                    // Seeking spring crystals
                    SeekingCrystalHelper.SpawnSpringCrystals(
                        Projectile.GetSource_FromThis(),
                        tipPos,
                        SwordDirection * 8f,
                        (int)(Projectile.damage * 0.35f),
                        Projectile.knockBack * 0.3f,
                        Projectile.owner,
                        count: 3
                    );
                }
            }

            // ── Dense dust + petal particles every frame during active swing ──
            if (Progression > 0.10f && Progression < 0.92f)
            {
                Vector2 tipPos = GetBladeTipPosition();
                float bladeLen = CurrentPhase.BladeLength;

                // Pink petal dust — dense, 2 per frame
                for (int i = 0; i < 2; i++)
                {
                    Vector2 dustPos = Owner.MountedCenter + SwordDirection * bladeLen * Main.rand.NextFloat(0.4f, 1f);
                    Dust d = Dust.NewDustPerfect(dustPos, DustID.PinkFairy,
                        -SwordDirection * Main.rand.NextFloat(1f, 3f) + Main.rand.NextVector2Circular(1f, 1f),
                        0, SpringPink, 1.4f);
                    d.noGravity = true;
                    d.fadeIn = 1.1f;
                }

                // Contrasting green sparkle every other frame
                if (Main.GameUpdateCount % 2 == 0)
                {
                    Dust g = Dust.NewDustPerfect(
                        Owner.MountedCenter + SwordDirection * bladeLen * Main.rand.NextFloat(0.5f, 0.9f),
                        DustID.GreenFairy,
                        -SwordDirection * Main.rand.NextFloat(0.5f, 2f),
                        0, SpringGreen, 1.1f);
                    g.noGravity = true;
                }

                // Music notes from blade tip (1-in-4 chance, visible scale)
                if (Main.rand.NextBool(4))
                {
                    float noteScale = Main.rand.NextFloat(0.7f, 0.95f);
                    float shimmer = 1f + MathF.Sin(Main.GameUpdateCount * 0.15f) * 0.12f;
                    Color noteColor = Color.Lerp(SpringPink, SpringGreen, Main.rand.NextFloat());
                    // Simple dust-based music note since ThemedParticles may vary
                    Dust note = Dust.NewDustPerfect(
                        tipPos + Main.rand.NextVector2Circular(6f, 6f),
                        DustID.Enchanted_Pink,
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

            // ── Increment hit counter ──
            HitCounter++;

            // ── Visual impact layers ──
            // Halo rings — pink → green gradient
            for (int i = 0; i < 4; i++)
            {
                float progress = i / 4f;
                Color ringColor = Color.Lerp(SpringPink, SpringGreen, progress);
                // GenericFlare or HaloRing via dust
                for (int j = 0; j < 2; j++)
                {
                    float angle = MathHelper.TwoPi * j / 2f + i * MathHelper.PiOver4;
                    Vector2 offset = angle.ToRotationVector2() * (15f + i * 8f);
                    Dust ring = Dust.NewDustPerfect(target.Center + offset, DustID.PinkFairy,
                        offset.SafeNormalize(Vector2.Zero) * 2f, 0, ringColor, 1.3f);
                    ring.noGravity = true;
                }
            }

            // Shimmer flares
            for (int i = 0; i < 3; i++)
            {
                Dust shimmer = Dust.NewDustPerfect(target.Center + Main.rand.NextVector2Circular(12f, 12f),
                    DustID.Enchanted_Pink,
                    Main.rand.NextVector2Circular(3f, 3f), 0, SpringWhite, 1.5f);
                shimmer.noGravity = true;
            }

            // Dust explosion
            for (int i = 0; i < 6; i++)
            {
                Dust burst = Dust.NewDustPerfect(target.Center, DustID.PinkFairy,
                    Main.rand.NextVector2Circular(5f, 5f), 0,
                    Color.Lerp(SpringPink, SpringGreen, Main.rand.NextFloat()), 1.4f);
                burst.noGravity = true;
            }

            // Scatter petals on every hit
            if (Main.rand.NextBool(3))
            {
                Dust petal = Dust.NewDustPerfect(target.Center + Main.rand.NextVector2Circular(10f, 10f),
                    DustID.PinkFairy,
                    Main.rand.NextVector2Circular(4f, 4f) + new Vector2(0, -2f),
                    0, CherryBlossom, 1.6f);
                petal.noGravity = true;
            }

            // ── RENEWAL STRIKE — every 5th hit heals 8 HP ──
            if (HitCounter >= 5)
            {
                HitCounter = 0;

                if (Main.myPlayer == Projectile.owner)
                {
                    owner.Heal(8);
                    CombatText.NewText(owner.getRect(), new Color(100, 255, 130), "Renewal!", true, false);
                }

                // Green healing VFX burst
                SoundEngine.PlaySound(SoundID.Item4 with { Pitch = 0.4f, Volume = 0.6f }, target.Center);
                for (int i = 0; i < 10; i++)
                {
                    float angle = MathHelper.TwoPi * i / 10f;
                    Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 6f);
                    Dust heal = Dust.NewDustPerfect(owner.Center, DustID.GreenFairy,
                        vel, 0, SpringGreen, 1.8f);
                    heal.noGravity = true;
                }
                for (int i = 0; i < 6; i++)
                {
                    Dust sparkle = Dust.NewDustPerfect(owner.Center + Main.rand.NextVector2Circular(20f, 20f),
                        DustID.Enchanted_Gold,
                        new Vector2(0, -Main.rand.NextFloat(1f, 3f)),
                        0, new Color(180, 255, 180), 1.3f);
                    sparkle.noGravity = true;
                }
            }

            // ── SPRING BLOOM — on crit: seeking crystals + AoE ──
            if (hit.Crit)
            {
                // Crit flash
                SoundEngine.PlaySound(SoundID.Item29 with { Pitch = 0.3f, Volume = 0.7f }, target.Center);
                for (int i = 0; i < 8; i++)
                {
                    Dust critDust = Dust.NewDustPerfect(target.Center, DustID.PinkFairy,
                        Main.rand.NextVector2Circular(7f, 7f), 0, Color.White, 1.6f);
                    critDust.noGravity = true;
                }

                // Petal burst
                for (int i = 0; i < 12; i++)
                {
                    float angle = MathHelper.TwoPi * i / 12f;
                    Vector2 petalVel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 8f);
                    Dust petal = Dust.NewDustPerfect(target.Center, DustID.PinkFairy,
                        petalVel, 0, Color.Lerp(SpringPink, CherryBlossom, Main.rand.NextFloat()), 1.8f);
                    petal.noGravity = true;
                }

                if (Main.myPlayer == Projectile.owner)
                {
                    // Seeking crystals (40% damage, 4 crystals)
                    SeekingCrystalHelper.SpawnSpringCrystals(
                        Projectile.GetSource_FromThis(),
                        target.Center,
                        (target.Center - owner.Center).SafeNormalize(Vector2.UnitY) * 6f,
                        (int)(Projectile.damage * 0.4f),
                        Projectile.knockBack * 0.3f,
                        Projectile.owner,
                        count: 4
                    );

                    // AoE — 50% damage to nearby enemies within 100 tiles
                    for (int i = 0; i < Main.maxNPCs; i++)
                    {
                        NPC npc = Main.npc[i];
                        if (npc.active && !npc.friendly && npc.whoAmI != target.whoAmI &&
                            Vector2.DistanceSquared(npc.Center, target.Center) < 10000f)
                        {
                            npc.SimpleStrikeNPC(Projectile.damage / 2, hit.HitDirection, hit.Crit, 0f, null, false, 0f, true);
                        }
                    }
                }
            }
        }

        #endregion

        #region ── Custom VFX ──

        protected override void DrawCustomVFX(SpriteBatch sb)
        {
            if (Progression < 0.08f || Progression > 0.95f) return;

            Vector2 tipWorld = GetBladeTipPosition();
            float pulse = 1f + MathF.Sin(Main.GlobalTimeWrappedHourly * 5f) * 0.08f;
            float tipScale = (0.15f + ComboStep * 0.05f) * pulse;

            BloomRenderer.DrawBloomStackAdditive(tipWorld, SpringPink, SpringGreen, tipScale, 0.85f);

            if (Main.rand.NextBool(4))
            {
                Vector2 noteVel = -SwordDirection * Main.rand.NextFloat(0.5f, 1.5f);
                MagnumParticleHandler.SpawnParticle(new HueShiftingMusicNoteParticle(
                    tipWorld, noteVel,
                    hueMin: 0.25f, hueMax: 0.42f,
                    saturation: 0.85f, luminosity: 0.65f,
                    scale: 0.75f, lifetime: 25, hueSpeed: 0.025f));
            }
        }

        #endregion
    }
}
