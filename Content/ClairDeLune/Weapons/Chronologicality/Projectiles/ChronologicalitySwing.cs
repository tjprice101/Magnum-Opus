using MagnumOpus.Common.BaseClasses;
using MagnumOpus.Content.ClairDeLune.Weapons.Chronologicality.Utilities;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.ClairDeLune.Weapons.Chronologicality.Projectiles
{
    /// <summary>
    /// ChronologicalitySwing — ExobladeStyleSwing implementation for the Chronologicality broadsword.
    ///
    /// 3-phase clock-hand combo tracked via ChronologicalityPlayer:
    ///  - Hour Hand (Phase 0): Heavy cleave, spawns TimeSlowField on hit, screen shake
    ///  - Minute Hand (Phase 1): Mid sweep, spawns TemporalEcho on hit
    ///  - Second Hand (Phase 2): Fast flurry, extra pearl burst on hit
    ///
    /// Dash triggers Clockwork Overflow detonation when a perfect H->M->S cycle has been completed.
    /// GPU-driven slash trail, dash mechanics, lens flare, and motion blur provided by ExobladeStyleSwing.
    /// </summary>
    public class ChronologicalitySwing : ExobladeStyleSwing
    {
        public override string Texture => "MagnumOpus/Content/ClairDeLune/Weapons/Chronologicality/Chronologicality";

        // ===============================================
        //  REQUIRED OVERRIDES
        // ===============================================

        protected override float BladeLength => 108f;
        protected override int BaseSwingFrames => 78;

        // CdL palette: night mist blue -> pearl white with clockwork brass accents
        protected override Color SlashPrimaryColor => ClairDeLunePalette.SoftBlue;
        protected override Color SlashSecondaryColor => ClairDeLunePalette.NightMist;
        protected override Color SlashAccentColor => ClairDeLunePalette.PearlWhite;

        // ===============================================
        //  VISUAL OVERRIDES
        // ===============================================

        protected override Color GetLensFlareColor(float progression)
            => Color.Lerp(ClairDeLunePalette.SoftBlue, ClairDeLunePalette.PearlWhite, (float)Math.Pow(progression, 2.5));

        protected override Color GetSwingDustColor()
        {
            float roll = Main.rand.NextFloat();
            if (roll < 0.35f)
                return ClairDeLunePalette.SoftBlue;
            if (roll < 0.65f)
                return ClairDeLunePalette.ClockworkBrass;
            return ClairDeLunePalette.PearlWhite;
        }

        protected override Color GetLightColor(float progression)
            => Color.Lerp(ClairDeLunePalette.MidnightBlue, ClairDeLunePalette.PearlBlue, (float)Math.Pow(progression, 2));

        protected override SoundStyle SwingSoundStyle => SoundID.Item71 with { Volume = 0.75f, PitchVariance = 0.3f };
        protected override SoundStyle DashHitSoundStyle => SoundID.Item62 with { Volume = 1f, Pitch = -0.2f };

        // ===============================================
        //  BEHAVIOR HOOKS
        // ===============================================

        private int ComboPhase => Owner.ChronologicalityState().ComboPhase;

        protected override void OnSwingStart(bool isFirstSwing)
        {
            // Play clockwork chime on phase transitions
            if (!isFirstSwing)
                SoundEngine.PlaySound(SoundID.Item35 with { Volume = 0.4f, Pitch = 0.3f + ComboPhase * 0.15f }, Projectile.Center);
        }

        protected override void OnSwingFrame()
        {
            Vector2 tipPos = Owner.MountedCenter + SwordDirection * BladeLength * Projectile.scale;

            // Moonlit shimmer dust at blade tip
            if (Main.rand.NextBool(3))
            {
                int dustType = Main.rand.NextBool() ? DustID.IceTorch : DustID.GemDiamond;
                Color col = dustType == DustID.IceTorch ? ClairDeLunePalette.SoftBlue : ClairDeLunePalette.PearlWhite;
                Dust d = Dust.NewDustPerfect(tipPos, dustType,
                    SwordDirection.RotatedBy(-MathHelper.PiOver2 * Direction) * 1.5f + Main.rand.NextVector2Circular(0.5f, 0.5f),
                    0, col, 1.4f);
                d.noGravity = true;
                d.fadeIn = 1.2f;
            }

            // Clockwork brass sparks (gold ticks along arc)
            if (Main.rand.NextBool(5))
            {
                float along = Main.rand.NextFloat(0.4f, 1f);
                Vector2 sparkPos = Owner.MountedCenter + SwordDirection * BladeLength * along * Projectile.scale;
                Dust g = Dust.NewDustPerfect(sparkPos, DustID.GoldFlame,
                    Main.rand.NextVector2Circular(1f, 1f), 0, ClairDeLunePalette.ClockworkBrass, 0.8f);
                g.noGravity = true;
            }

            // Periodic pearl sparkle via VFX library
            if (Timer % 6 == 0)
                ClairDeLuneVFXLibrary.SpawnPearlSparkle(tipPos, -SwordDirection);

            // LUT color-ramped 4-point star sparkles along blade tip
            ClairDeLuneVFXLibrary.SpawnLUTSwingSparkles(tipPos, SwordDirection, Direction, Progression);

            // Music notes every 8 ticks
            if (Timer % 8 == 0)
                ClairDeLuneVFXLibrary.SpawnMusicNotes(tipPos, 1, 12f, 0.7f, 0.9f, 25);
        }

        protected override void OnSwingHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            var mp = Owner.ChronologicalityState();
            int phase = mp.ComboPhase;

            // Full Clair de Lune melee impact VFX (scales with combo)
            ClairDeLuneVFXLibrary.MeleeImpact(target.Center, phase);

            // Phase-specific on-hit effects
            switch (phase)
            {
                case 0: // Hour Hand -- heavy, spawns Time Slow Field
                    if (Owner.ownedProjectileCounts[ModContent.ProjectileType<TimeSlowFieldProjectile>()] < 2)
                    {
                        Projectile.NewProjectile(Projectile.GetSource_FromThis(), target.Center, Vector2.Zero,
                            ModContent.ProjectileType<TimeSlowFieldProjectile>(),
                            0, 0f, Owner.whoAmI);
                    }
                    break;

                case 1: // Minute Hand -- spawns Temporal Echo for 30% damage replay
                    float echoDir = (target.Center - Owner.MountedCenter).ToRotation();
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), target.Center,
                        Vector2.Zero, ModContent.ProjectileType<TemporalEchoProjectile>(),
                        (int)(damageDone * 0.3f), 0f, Owner.whoAmI,
                        echoDir, SwordRotation);
                    break;

                case 2: // Second Hand -- extra pearl sparkle burst
                    ClairDeLuneVFXLibrary.SpawnPearlBurst(target.Center, 6, 4f, 0.25f);
                    break;
            }

            // Advance combo
            mp.AdvanceCombo();
        }

        protected override void OnDashHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            var mp = Owner.ChronologicalityState();

            // If overflow is available, trigger Clockwork Overflow detonation
            if (mp.CanTriggerOverflow)
            {
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), target.Center,
                    Vector2.Zero, ModContent.ProjectileType<ClockworkOverflowProjectile>(),
                    (int)(damageDone * 1.5f), 12f, Owner.whoAmI);
                mp.ConsumeOverflow();
            }

            // Enhanced impact VFX for dash hit
            ClairDeLuneVFXLibrary.FinisherSlam(target.Center, 0.8f);

            // Radial clockwork brass sparks
            for (int i = 0; i < 12; i++)
            {
                float angle = MathHelper.TwoPi * i / 12f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 7f);
                Dust d = Dust.NewDustPerfect(target.Center, DustID.GoldFlame, vel, 0, ClairDeLunePalette.ClockworkBrass, 1.2f);
                d.noGravity = true;
            }
        }

        protected override void OnDashFrame()
        {
            // Temporal trail particles during dash lunge
            if (Main.rand.NextBool(2))
            {
                Vector2 dustPos = Owner.MountedCenter + Main.rand.NextVector2Circular(15f, 15f);
                Dust d = Dust.NewDustPerfect(dustPos, DustID.IceTorch,
                    -Projectile.velocity * 0.5f + Main.rand.NextVector2Circular(1f, 1f),
                    0, ClairDeLunePalette.PearlBlue, 1.3f);
                d.noGravity = true;
            }
        }
    }
}