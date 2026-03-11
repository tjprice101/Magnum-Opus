using MagnumOpus.Common.BaseClasses;
using MagnumOpus.Content.ClairDeLune.Weapons.TemporalPiercer.Utilities;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.ClairDeLune.Weapons.TemporalPiercer.Projectiles
{
    /// <summary>
    /// TemporalThrustProjectile -- ExobladeStyleSwing implementation for the Temporal Piercer rapier.
    ///
    /// Short blade, fast swing for rapier precision feel.
    /// Each hit inflicts Temporal Puncture marks tracked via TemporalPiercerPlayer.
    /// At 5 marks on a target, triggers Frozen Moment (stun + massive burst).
    /// Dash replaces the old boomerang alt-fire as a Time-Pierce Lunge.
    ///
    /// GPU-driven slash trail, dash, lens flare from ExobladeStyleSwing.
    /// Crystal frost palette: pearl blue -> moonlit frost -> absolute white.
    /// </summary>
    public class TemporalThrustProjectile : ExobladeStyleSwing
    {
        public override string Texture => "MagnumOpus/Content/ClairDeLune/Weapons/TemporalPiercer/TemporalPiercer";

        // ===============================================
        //  REQUIRED OVERRIDES
        // ===============================================

        protected override float BladeLength => 80f;    // Short rapier reach
        protected override int BaseSwingFrames => 54;   // Fast rapier swing

        // Crystal frost palette
        protected override Color SlashPrimaryColor => ClairDeLunePalette.MoonlitFrost;
        protected override Color SlashSecondaryColor => ClairDeLunePalette.MidnightBlue;
        protected override Color SlashAccentColor => new Color(255, 255, 255); // Absolute crystal white

        // ===============================================
        //  VISUAL OVERRIDES
        // ===============================================

        protected override Color GetLensFlareColor(float progression)
            => Color.Lerp(ClairDeLunePalette.PearlBlue, new Color(240, 248, 255), (float)Math.Pow(progression, 2));

        protected override Color GetSwingDustColor()
        {
            float roll = Main.rand.NextFloat();
            if (roll < 0.4f)
                return ClairDeLunePalette.MoonlitFrost;
            if (roll < 0.7f)
                return ClairDeLunePalette.PearlBlue;
            return ClairDeLunePalette.PearlWhite;
        }

        protected override Color GetLightColor(float progression)
            => Color.Lerp(ClairDeLunePalette.SoftBlue, ClairDeLunePalette.MoonlitFrost, (float)Math.Pow(progression, 2));

        // Rapier feel: narrower arc, quick and precise
        protected override float SwingArcMultiplier => 1.4f;

        protected override SoundStyle SwingSoundStyle => SoundID.Item71 with { Volume = 0.6f, Pitch = 0.3f, PitchVariance = 0.2f };
        protected override SoundStyle DashSoundStyle => SoundID.Item73 with { Volume = 0.7f, Pitch = 0.2f };

        // Faster, shorter dash for rapier lunge
        protected override int DashTimeFrames => 36;
        protected override float DashLungeSpeed => 50f;

        // ===============================================
        //  BEHAVIOR HOOKS
        // ===============================================

        protected override void OnSwingStart(bool isFirstSwing)
        {
            // Crystalline chime on each swing start
            if (!isFirstSwing)
                SoundEngine.PlaySound(SoundID.Item28 with { Volume = 0.3f, Pitch = 0.5f }, Projectile.Center);
        }

        protected override void OnSwingFrame()
        {
            Vector2 tipPos = Owner.MountedCenter + SwordDirection * BladeLength * Projectile.scale;

            // Crystal frost sparkles
            if (Main.rand.NextBool(3))
            {
                Dust d = Dust.NewDustPerfect(tipPos, DustID.GemDiamond,
                    SwordDirection.RotatedBy(-MathHelper.PiOver2 * Direction) * 1.2f + Main.rand.NextVector2Circular(0.4f, 0.4f),
                    0, ClairDeLunePalette.MoonlitFrost, 1.2f);
                d.noGravity = true;
                d.fadeIn = 1.1f;
            }

            // Ice shimmer along blade
            if (Main.rand.NextBool(4))
            {
                float along = Main.rand.NextFloat(0.3f, 1f);
                Vector2 icePos = Owner.MountedCenter + SwordDirection * BladeLength * along * Projectile.scale;
                Dust ice = Dust.NewDustPerfect(icePos, DustID.IceTorch,
                    Main.rand.NextVector2Circular(0.8f, 0.8f), 0, ClairDeLunePalette.PearlBlue, 0.9f);
                ice.noGravity = true;
            }

            // Pearl sparkle at tip
            if (Timer % 5 == 0)
                ClairDeLuneVFXLibrary.SpawnPearlSparkle(tipPos, -SwordDirection);

            // LUT color-ramped 4-point star sparkles at rapier tip
            ClairDeLuneVFXLibrary.SpawnLUTSwingSparkles(tipPos, SwordDirection, Direction, Progression);

            // Starlit sparkles periodically
            if (Timer % 10 == 0)
                ClairDeLuneVFXLibrary.SpawnStarlitSparkles(tipPos, 2, 15f, 0.18f);
        }

        protected override void OnSwingHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            var pp = Owner.GetModPlayer<TemporalPiercerPlayer>();
            int stacks = pp.AddPunctureMark(target.whoAmI, damageDone);

            // Crystal impact VFX
            ClairDeLuneVFXLibrary.MeleeImpact(target.Center, Math.Min(stacks, 3));

            // Visual mark indicator: pearl sparkle burst scales with stacks
            ClairDeLuneVFXLibrary.SpawnPearlBurst(target.Center, 3 + stacks, 3f + stacks * 0.5f, 0.2f);

            // At 5 marks -> trigger Frozen Moment
            if (stacks >= 5)
            {
                float burstDamage = pp.ConsumeAllMarks(target.whoAmI);
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), target.Center,
                    Vector2.Zero, ModContent.ProjectileType<FrozenMomentProjectile>(),
                    (int)burstDamage, 0f, Owner.whoAmI, target.whoAmI);

                // Grand crystal detonation VFX
                ClairDeLuneVFXLibrary.FinisherSlam(target.Center, 1f);
                SoundEngine.PlaySound(SoundID.Item27 with { Volume = 1f, Pitch = -0.3f }, target.Center);
            }
        }

        protected override void OnDashHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            var pp = Owner.GetModPlayer<TemporalPiercerPlayer>();

            // Dash always adds 2 puncture marks (Time-Pierce Lunge)
            pp.AddPunctureMark(target.whoAmI, damageDone);
            int stacks = pp.AddPunctureMark(target.whoAmI, damageDone);

            // Enhanced VFX for lunge hit
            ClairDeLuneVFXLibrary.FinisherSlam(target.Center, 0.6f);

            // Crystal shard burst
            for (int i = 0; i < 10; i++)
            {
                float angle = MathHelper.TwoPi * i / 10f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 6f);
                Dust d = Dust.NewDustPerfect(target.Center, DustID.GemDiamond, vel, 0, ClairDeLunePalette.MoonlitFrost, 1.4f);
                d.noGravity = true;
            }

            // Check for Frozen Moment trigger after double-stack
            if (stacks >= 5)
            {
                float burstDamage = pp.ConsumeAllMarks(target.whoAmI);
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), target.Center,
                    Vector2.Zero, ModContent.ProjectileType<FrozenMomentProjectile>(),
                    (int)burstDamage, 0f, Owner.whoAmI, target.whoAmI);

                ClairDeLuneVFXLibrary.FinisherSlam(target.Center, 1.2f);
                SoundEngine.PlaySound(SoundID.Item27 with { Volume = 1f, Pitch = -0.3f }, target.Center);
            }
        }

        protected override void OnDashFrame()
        {
            // Crystal frost trail during lunge
            if (Main.rand.NextBool(2))
            {
                Vector2 dustPos = Owner.MountedCenter + Main.rand.NextVector2Circular(10f, 10f);
                Dust d = Dust.NewDustPerfect(dustPos, DustID.GemDiamond,
                    -Projectile.velocity * 0.4f + Main.rand.NextVector2Circular(0.8f, 0.8f),
                    0, ClairDeLunePalette.PearlWhite, 1.1f);
                d.noGravity = true;
            }
        }
    }
}