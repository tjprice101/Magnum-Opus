using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.VFX.Bloom;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Content.ClairDeLune.Weapons.Ranged
{
    // ═══════════════════════════════════════════════════════════════════════════
    //  STARFALL WHISPER — VFX
    //  Identity: Temporal Sniper — crystalline bolts that pierce through time.
    //  Precise, distant, starlit. Every shot is a falling star.
    //  Starlight silver collides with pearl white in each critical discharge.
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// VFX helper for the Starfall Whisper sniper rifle.
    /// Handles hold-item starlight shimmer, world item bloom, charge-up particles,
    /// muzzle flash, crystal bolt trail, bolt impact, chain lightning arcs,
    /// and starfall rift detonation VFX.
    /// </summary>
    public static class StarfallWhisperVFX
    {
        /// <summary>
        /// Starlit shimmer around the player while holding the sniper.
        /// Silver dust orbits with pearl sparkle accents and ambient dreamy aura.
        /// </summary>
        public static void HoldItemVFX(Player player, Item item)
        {
            if (Main.dedServ) return;

            Vector2 center = player.MountedCenter;
            float time = (float)Main.timeForVisualEffects;

            // Starlight silver orbiting motes
            for (int i = 0; i < 2; i++)
            {
                float angle = time * 0.04f + MathHelper.Pi * i;
                float radius = 16f + MathF.Sin(time * 0.06f + i * 0.9f) * 3f;
                Vector2 motePos = center + angle.ToRotationVector2() * radius;

                if (Main.rand.NextBool(4))
                {
                    Color col = ClairDeLunePalette.PaletteLerp(ClairDeLunePalette.StarfallWhisperShot, 0.3f + (float)i / 2f * 0.4f);
                    Dust d = Dust.NewDustPerfect(motePos, DustID.IceTorch, Vector2.Zero, 0, col, 0.7f);
                    d.noGravity = true;
                    d.fadeIn = 0.5f;
                }
            }

            // Starlit sparkle accents
            if (Main.rand.NextBool(7))
            {
                Vector2 sparkPos = center + Main.rand.NextVector2Circular(20f, 20f);
                Color col = Color.Lerp(ClairDeLunePalette.StarlightSilver, ClairDeLunePalette.PearlWhite, Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(sparkPos, DustID.GemDiamond,
                    new Vector2(Main.rand.NextFloat(-0.2f, 0.2f), -0.3f), 0, col, 0.6f);
                d.noGravity = true;
            }

            ClairDeLuneVFXLibrary.AmbientDreamyAura(center, time, 26f);

            if (Main.rand.NextBool(30))
                ClairDeLuneVFXLibrary.SpawnMusicNotes(center, 1, 18f, 0.7f, 0.85f, 28);

            float pulse = 0.35f + MathF.Sin(time * 0.05f) * 0.1f;
            Lighting.AddLight(center, ClairDeLunePalette.StarlightSilver.ToVector3() * pulse);
        }

        /// <summary>
        /// 3-layer PreDrawInWorld bloom for the Starfall Whisper sniper.
        /// </summary>
        public static void PreDrawInWorldBloom(
            Microsoft.Xna.Framework.Graphics.SpriteBatch sb,
            Microsoft.Xna.Framework.Graphics.Texture2D tex,
            Vector2 pos, Vector2 origin, float rotation, float scale)
        {
            float time = (float)Main.timeForVisualEffects;
            float pulse = 1f + MathF.Sin(time * 0.05f) * 0.03f;
            ClairDeLunePalette.DrawItemBloom(sb, tex, pos, origin, rotation, scale, pulse);
        }

        /// <summary>
        /// Charge-up convergence VFX at the muzzle while holding fire.
        /// Crystal and starlight particles converge toward the barrel tip.
        /// </summary>
        public static void ChargeUpVFX(Vector2 muzzlePos, float progress)
        {
            if (Main.dedServ) return;

            int dustCount = 2 + (int)(progress * 5);
            for (int i = 0; i < dustCount; i++)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                float radius = 50f * (1f - progress * 0.5f) + Main.rand.NextFloat(15f);
                Vector2 dustPos = muzzlePos + angle.ToRotationVector2() * radius;
                Vector2 toCenter = (muzzlePos - dustPos).SafeNormalize(Vector2.Zero) * (2f + progress * 4f);
                Color col = Color.Lerp(ClairDeLunePalette.StarlightSilver, ClairDeLunePalette.WhiteHot, progress);
                Dust d = Dust.NewDustPerfect(dustPos, DustID.IceTorch, toCenter, 0, col, 0.7f + progress * 0.5f);
                d.noGravity = true;
            }

            if (progress > 0.5f)
                ClairDeLuneVFXLibrary.SpawnPearlShimmer(muzzlePos, 1, 15f * (1f - progress), 0.2f);

            Lighting.AddLight(muzzlePos, ClairDeLunePalette.StarlightSilver.ToVector3() * (0.3f + progress * 0.5f));
        }

        /// <summary>
        /// Muzzle flash VFX on fire — starlit burst with pearl shimmer.
        /// Enhanced version for charged shots.
        /// </summary>
        public static void MuzzleFlashVFX(Vector2 pos, Vector2 direction, bool isCharged)
        {
            if (Main.dedServ) return;

            float intensity = isCharged ? 1.5f : 1f;
            int dustCount = isCharged ? 10 : 6;

            ClairDeLuneVFXLibrary.DrawBloom(pos, 0.4f * intensity);

            for (int i = 0; i < dustCount; i++)
            {
                Vector2 vel = direction * Main.rand.NextFloat(3f, 7f) * intensity
                    + Main.rand.NextVector2Circular(1.5f, 1.5f);
                Color col = ClairDeLunePalette.PaletteLerp(ClairDeLunePalette.StarfallWhisperShot, Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(pos, DustID.IceTorch, vel, 0, col, 1.2f * intensity);
                d.noGravity = true;
            }

            ClairDeLuneVFXLibrary.SpawnPearlShimmer(pos, isCharged ? 4 : 2, 20f, 0.25f);
            ClairDeLuneVFXLibrary.SpawnMusicNotes(pos, isCharged ? 3 : 1, 15f, 0.75f, 1.0f, 22);

            if (isCharged)
            {
                ClairDeLuneVFXLibrary.SpawnStarlitSparkles(pos, 5, 25f, 0.22f);
                MagnumScreenEffects.AddScreenShake(3f);
            }

            Lighting.AddLight(pos, ClairDeLunePalette.WhiteHot.ToVector3() * (0.8f * intensity));
        }

        /// <summary>
        /// Crystal bolt trail VFX — starlight silver to pearl gradient.
        /// </summary>
        public static void BoltTrailVFX(Vector2 pos, Vector2 velocity)
        {
            if (Main.dedServ) return;

            Vector2 away = -velocity.SafeNormalize(Vector2.Zero);

            // Starlight crystal dust
            {
                Vector2 vel = away * Main.rand.NextFloat(1f, 2.5f) + Main.rand.NextVector2Circular(0.4f, 0.4f);
                Color col = ClairDeLunePalette.PaletteLerp(ClairDeLunePalette.StarfallWhisperShot, Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(pos, DustID.IceTorch, vel, 0, col, 1.1f);
                d.noGravity = true;
            }

            // Pearl sparkle accent
            if (Main.rand.NextBool(2))
            {
                Vector2 vel = away * Main.rand.NextFloat(0.5f, 1.5f);
                Color col = ClairDeLunePalette.GetPearlGradient(Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(pos, DustID.GemDiamond, vel, 0, col, 0.8f);
                d.noGravity = true;
            }

            ClairDeLuneVFXLibrary.SpawnPearlSparkle(pos, away);
            Lighting.AddLight(pos, ClairDeLunePalette.StarlightSilver.ToVector3() * 0.4f);
        }

        /// <summary>
        /// Crystal bolt impact VFX.
        /// </summary>
        public static void BoltImpactVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            ClairDeLuneVFXLibrary.ProjectileImpact(pos, 0.6f);

            // Starlit shatter burst
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 5f);
                Color col = ClairDeLunePalette.PaletteLerp(ClairDeLunePalette.StarfallWhisperShot, (float)i / 8f);
                Dust d = Dust.NewDustPerfect(pos, DustID.IceTorch, vel, 0, col, 1.2f);
                d.noGravity = true;
            }

            ClairDeLuneVFXLibrary.SpawnPearlBurst(pos, 6, 4f, 0.25f);
            ClairDeLuneVFXLibrary.SpawnMusicNotes(pos, 2, 18f, 0.75f, 1.0f, 25);
            Lighting.AddLight(pos, ClairDeLunePalette.PearlWhite.ToVector3() * 0.8f);
        }

        /// <summary>
        /// Chain lightning arc VFX between enemies on critical hit.
        /// Electric arcs with starlit shimmer at each node.
        /// </summary>
        public static void ChainLightningVFX(Vector2 from, Vector2 to)
        {
            if (Main.dedServ) return;

            int segments = (int)(Vector2.Distance(from, to) / 15f);
            segments = Math.Max(segments, 4);

            for (int i = 0; i < segments; i++)
            {
                float t = (float)i / segments;
                Vector2 pos = Vector2.Lerp(from, to, t);
                Vector2 jitter = Main.rand.NextVector2Circular(8f, 8f);
                Color col = ClairDeLunePalette.GetPearlGradient(t);
                Dust d = Dust.NewDustPerfect(pos + jitter, DustID.Electric,
                    Main.rand.NextVector2Circular(0.5f, 0.5f), 0, col, 0.9f);
                d.noGravity = true;
            }

            Lighting.AddLight(Vector2.Lerp(from, to, 0.5f), ClairDeLunePalette.PearlBlue.ToVector3() * 0.6f);
        }

        /// <summary>
        /// Starfall rift detonation VFX — massive starlit explosion
        /// when a fully charged shot creates a rift.
        /// </summary>
        public static void StarfallRiftVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            ClairDeLuneVFXLibrary.DrawBloom(pos, 0.9f);

            // 20-point radial starlit burst
            for (int i = 0; i < 20; i++)
            {
                float angle = MathHelper.TwoPi * i / 20f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(6f, 10f);
                Color col = ClairDeLunePalette.PaletteLerp(ClairDeLunePalette.StarfallWhisperShot, (float)i / 20f);
                int dustType = i % 3 == 0 ? DustID.Electric : DustID.IceTorch;
                Dust d = Dust.NewDustPerfect(pos, dustType, vel, 0, col, 1.6f);
                d.noGravity = true;
            }

            ClairDeLuneVFXLibrary.SpawnPearlBurst(pos, 12, 6f, 0.35f);
            ClairDeLuneVFXLibrary.SpawnGradientHaloRings(pos, 6, 0.35f);
            ClairDeLuneVFXLibrary.SpawnStarlitSparkles(pos, 10, 45f, 0.25f);
            ClairDeLuneVFXLibrary.SpawnMoonlitMist(pos, 6, 55f, 0.5f);
            ClairDeLuneVFXLibrary.SpawnMusicNotes(pos, 8, 40f, 0.8f, 1.2f, 40);

            MagnumScreenEffects.AddScreenShake(7f);
            Lighting.AddLight(pos, ClairDeLunePalette.WhiteHot.ToVector3() * 1.8f);
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════
    //  MIDNIGHT MECHANISM — VFX
    //  Identity: Clockwork Gatling — spinning brass mechanism in the dark.
    //  Relentless, mechanical, intensifying. Every bullet is a clock tick.
    //  Brass and gold interleave with temporal crimson on special attacks.
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// VFX helper for the Midnight Mechanism clockwork gatling gun.
    /// Handles hold-item brass shimmer, world item bloom, spin-up particles,
    /// bullet trail, bullet impact, and gear barrage detonation VFX.
    /// </summary>
    public static class MidnightMechanismVFX
    {
        /// <summary>
        /// Brass clockwork shimmer around the player while holding the gatling.
        /// Gear motes orbit with amber sparks and ambient clockwork aura.
        /// </summary>
        public static void HoldItemVFX(Player player, Item item)
        {
            if (Main.dedServ) return;

            Vector2 center = player.MountedCenter;
            float time = (float)Main.timeForVisualEffects;

            // Brass gear motes — 2-point subtle orbit
            for (int i = 0; i < 2; i++)
            {
                float angle = time * 0.05f + MathHelper.Pi * i;
                float radius = 18f + MathF.Sin(time * 0.07f) * 3f;
                Vector2 motePos = center + angle.ToRotationVector2() * radius;

                if (Main.rand.NextBool(4))
                {
                    Color col = ClairDeLunePalette.PaletteLerp(ClairDeLunePalette.MidnightMechanismBurst, 0.3f + (float)i / 2f * 0.4f);
                    Dust d = Dust.NewDustPerfect(motePos, DustID.GemDiamond, Vector2.Zero, 0, col, 0.7f);
                    d.noGravity = true;
                }
            }

            if (Main.rand.NextBool(6))
            {
                Vector2 sparkPos = center + Main.rand.NextVector2Circular(20f, 20f);
                Color col = ClairDeLunePalette.GetClockworkGradient(Main.rand.NextFloat(0.4f, 0.9f));
                Dust d = Dust.NewDustPerfect(sparkPos, DustID.GemDiamond,
                    new Vector2(Main.rand.NextFloat(-0.2f, 0.2f), -0.3f), 0, col, 0.65f);
                d.noGravity = true;
            }

            ClairDeLuneVFXLibrary.AmbientClockworkAura(center, time, 28f);

            if (Main.rand.NextBool(28))
                ClairDeLuneVFXLibrary.SpawnMusicNotes(center, 1, 18f, 0.7f, 0.85f, 28);

            float pulse = 0.35f + MathF.Sin(time * 0.06f) * 0.1f;
            Lighting.AddLight(center, ClairDeLunePalette.ClockworkBrass.ToVector3() * pulse);
        }

        /// <summary>
        /// 3-layer PreDrawInWorld bloom for the Midnight Mechanism gatling.
        /// </summary>
        public static void PreDrawInWorldBloom(
            Microsoft.Xna.Framework.Graphics.SpriteBatch sb,
            Microsoft.Xna.Framework.Graphics.Texture2D tex,
            Vector2 pos, Vector2 origin, float rotation, float scale)
        {
            float time = (float)Main.timeForVisualEffects;
            float pulse = 1f + MathF.Sin(time * 0.05f) * 0.03f;
            ClairDeLunePalette.DrawItemBloom(sb, tex, pos, origin, rotation, scale, pulse);
        }

        /// <summary>
        /// Spin-up ambient particles — increasing intensity as fire rate ramps.
        /// </summary>
        public static void SpinUpVFX(Vector2 muzzlePos, float spinProgress)
        {
            if (Main.dedServ) return;

            int dustCount = 1 + (int)(spinProgress * 4);
            for (int i = 0; i < dustCount; i++)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 dustPos = muzzlePos + angle.ToRotationVector2() * Main.rand.NextFloat(5f, 15f);
                Color col = ClairDeLunePalette.GetClockworkGradient(Main.rand.NextFloat(0.3f, 0.8f));
                Dust d = Dust.NewDustPerfect(dustPos, DustID.GemDiamond,
                    Main.rand.NextVector2Circular(0.5f, 0.5f), 0, col, 0.5f + spinProgress * 0.3f);
                d.noGravity = true;
            }

            Lighting.AddLight(muzzlePos, ClairDeLunePalette.ClockworkBrass.ToVector3() * (0.2f + spinProgress * 0.3f));
        }

        /// <summary>
        /// Bullet trail VFX — brass-gold streaks behind each bullet.
        /// </summary>
        public static void BulletTrailVFX(Vector2 pos, Vector2 velocity)
        {
            if (Main.dedServ) return;

            Vector2 away = -velocity.SafeNormalize(Vector2.Zero);

            Vector2 vel = away * Main.rand.NextFloat(0.5f, 1.5f) + Main.rand.NextVector2Circular(0.3f, 0.3f);
            Color col = Color.Lerp(ClairDeLunePalette.ClockworkBrass, ClairDeLunePalette.MoonbeamGold, Main.rand.NextFloat());
            Dust d = Dust.NewDustPerfect(pos, DustID.GemDiamond, vel, 0, col, 0.9f);
            d.noGravity = true;

            Lighting.AddLight(pos, ClairDeLunePalette.ClockworkBrass.ToVector3() * 0.3f);
        }

        /// <summary>
        /// Bullet impact VFX — small brass burst.
        /// </summary>
        public static void BulletImpactVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            for (int i = 0; i < 5; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(3f, 3f);
                Color col = ClairDeLunePalette.PaletteLerp(ClairDeLunePalette.MidnightMechanismBurst, Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(pos, DustID.GemDiamond, vel, 0, col, 1.0f);
                d.noGravity = true;
            }

            ClairDeLuneVFXLibrary.SpawnMusicNotes(pos, 1, 10f, 0.7f, 0.9f, 18);
            Lighting.AddLight(pos, ClairDeLunePalette.MoonbeamGold.ToVector3() * 0.5f);
        }

        /// <summary>
        /// Gear barrage detonation VFX — massive clockwork explosion
        /// triggered every 50th shot.
        /// </summary>
        public static void GearBarrageVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            ClairDeLuneVFXLibrary.DrawBloom(pos, 0.8f);

            // 18-point radial brass + crimson burst
            for (int i = 0; i < 18; i++)
            {
                float angle = MathHelper.TwoPi * i / 18f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(5f, 9f);
                Color col = i % 3 == 0
                    ? ClairDeLunePalette.GetTemporalGradient((float)i / 18f)
                    : ClairDeLunePalette.GetClockworkGradient((float)i / 18f);
                Dust d = Dust.NewDustPerfect(pos, DustID.GemDiamond, vel, 0, col, 1.5f);
                d.noGravity = true;
            }

            ClairDeLuneVFXLibrary.SpawnPearlBurst(pos, 10, 5f, 0.3f);
            ClairDeLuneVFXLibrary.SpawnGradientHaloRings(pos, 5, 0.35f);
            ClairDeLuneVFXLibrary.SpawnMusicNotes(pos, 6, 35f, 0.8f, 1.1f, 35);
            ClairDeLuneVFXLibrary.SpawnConvergingMist(pos, 6, 55f, 0.5f);

            MagnumScreenEffects.AddScreenShake(5f);
            Lighting.AddLight(pos, ClairDeLunePalette.WhiteHot.ToVector3() * 1.5f);
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════
    //  COG AND HAMMER — VFX
    //  Identity: Clockwork Launcher — explosive clockwork bombs across moonlit sky.
    //  Heavy, explosive, mechanical. Every launch is a cannon's report.
    //  Deep night to brass with volcanic-style distortion on detonation.
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// VFX helper for the Cog and Hammer clockwork launcher.
    /// Handles hold-item heavy brass ambiance, world item bloom, charge-up particles,
    /// bomb trail, bomb detonation, gear shrapnel trail, and temporal singularity VFX.
    /// </summary>
    public static class CogAndHammerVFX
    {
        /// <summary>
        /// Heavy brass clockwork ambiance while holding the launcher.
        /// </summary>
        public static void HoldItemVFX(Player player, Item item)
        {
            if (Main.dedServ) return;

            Vector2 center = player.MountedCenter;
            float time = (float)Main.timeForVisualEffects;

            // Heavy brass gear shimmer
            if (Main.rand.NextBool(5))
            {
                Vector2 sparkPos = center + Main.rand.NextVector2Circular(22f, 22f);
                Color col = ClairDeLunePalette.GetClockworkGradient(Main.rand.NextFloat(0.3f, 0.9f));
                Dust d = Dust.NewDustPerfect(sparkPos, DustID.GemDiamond,
                    new Vector2(Main.rand.NextFloat(-0.3f, 0.3f), -0.4f), 0, col, 0.8f);
                d.noGravity = true;
            }

            // Night mist undertone
            if (Main.rand.NextBool(8))
            {
                Vector2 mistPos = center + Main.rand.NextVector2Circular(25f, 25f);
                Color col = ClairDeLunePalette.NightMist * 0.5f;
                var glow = new GenericGlowParticle(mistPos,
                    new Vector2(Main.rand.NextFloat(-0.3f, 0.3f), -0.2f),
                    col, 0.2f, 25, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }

            ClairDeLuneVFXLibrary.AmbientClockworkAura(center, time, 30f);

            if (Main.rand.NextBool(25))
                ClairDeLuneVFXLibrary.SpawnMusicNotes(center, 1, 20f, 0.7f, 0.9f, 30);

            float pulse = 0.4f + MathF.Sin(time * 0.04f) * 0.12f;
            Lighting.AddLight(center, ClairDeLunePalette.ClockworkBrass.ToVector3() * pulse);
        }

        /// <summary>
        /// 3-layer PreDrawInWorld bloom for the Cog and Hammer launcher.
        /// </summary>
        public static void PreDrawInWorldBloom(
            Microsoft.Xna.Framework.Graphics.SpriteBatch sb,
            Microsoft.Xna.Framework.Graphics.Texture2D tex,
            Vector2 pos, Vector2 origin, float rotation, float scale)
        {
            float time = (float)Main.timeForVisualEffects;
            float pulse = 1f + MathF.Sin(time * 0.035f) * 0.03f;
            ClairDeLunePalette.DrawItemBloom(sb, tex, pos, origin, rotation, scale, pulse);
        }

        /// <summary>
        /// Charge-up convergence VFX — heavy clockwork particles converging.
        /// </summary>
        public static void ChargeUpVFX(Vector2 pos, float progress)
        {
            if (Main.dedServ) return;

            int dustCount = 3 + (int)(progress * 6);
            for (int i = 0; i < dustCount; i++)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                float radius = 45f * (1f - progress * 0.4f) + Main.rand.NextFloat(15f);
                Vector2 dustPos = pos + angle.ToRotationVector2() * radius;
                Vector2 toCenter = (pos - dustPos).SafeNormalize(Vector2.Zero) * (2f + progress * 3f);
                Color col = Color.Lerp(ClairDeLunePalette.NightMist, ClairDeLunePalette.ClockworkBrass, progress);
                Dust d = Dust.NewDustPerfect(dustPos, DustID.GemDiamond, toCenter, 0, col, 0.8f + progress * 0.4f);
                d.noGravity = true;
            }

            Lighting.AddLight(pos, ClairDeLunePalette.ClockworkBrass.ToVector3() * (0.3f + progress * 0.4f));
        }

        /// <summary>
        /// Bomb/gear shrapnel trail VFX — clockwork debris following projectile.
        /// </summary>
        public static void BombTrailVFX(Vector2 pos, Vector2 velocity)
        {
            if (Main.dedServ) return;

            Vector2 away = -velocity.SafeNormalize(Vector2.Zero);

            for (int i = 0; i < 2; i++)
            {
                Vector2 vel = away * Main.rand.NextFloat(1f, 2.5f) + Main.rand.NextVector2Circular(0.5f, 0.5f);
                Color col = ClairDeLunePalette.PaletteLerp(ClairDeLunePalette.CogAndHammerLaunch, Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(pos, DustID.GemDiamond, vel, 0, col, 1.1f);
                d.noGravity = true;
            }

            // Smoke trail
            if (Main.rand.NextBool(3))
            {
                var smoke = new GenericGlowParticle(pos, away * 0.5f,
                    ClairDeLunePalette.NightMist * 0.4f, 0.25f, 20, true);
                MagnumParticleHandler.SpawnParticle(smoke);
            }

            Lighting.AddLight(pos, ClairDeLunePalette.ClockworkBrass.ToVector3() * 0.35f);
        }

        /// <summary>
        /// Standard bomb detonation VFX — gear shrapnel explosion.
        /// </summary>
        public static void BombDetonationVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            ClairDeLuneVFXLibrary.ProjectileImpact(pos, 0.9f);

            // 14-point radial brass gear shrapnel
            for (int i = 0; i < 14; i++)
            {
                float angle = MathHelper.TwoPi * i / 14f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 7f);
                Color col = ClairDeLunePalette.GetClockworkGradient((float)i / 14f);
                Dust d = Dust.NewDustPerfect(pos, DustID.GemDiamond, vel, 0, col, 1.4f);
                d.noGravity = true;
            }

            ClairDeLuneVFXLibrary.SpawnPearlBurst(pos, 8, 5f, 0.3f);
            ClairDeLuneVFXLibrary.SpawnMusicNotes(pos, 4, 25f, 0.8f, 1.0f, 28);
            ClairDeLuneVFXLibrary.SpawnMoonlitMist(pos, 3, 35f, 0.45f);

            MagnumScreenEffects.AddScreenShake(4f);
            Lighting.AddLight(pos, ClairDeLunePalette.MoonbeamGold.ToVector3() * 1.0f);
        }

        /// <summary>
        /// Temporal singularity VFX — massive charged shot detonation.
        /// Deep implosion followed by massive brass-pearl explosion.
        /// </summary>
        public static void TemporalSingularityVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            ClairDeLuneVFXLibrary.DrawBloom(pos, 1.0f);

            // 24-point radial mega-burst — alternating brass and pearl
            for (int i = 0; i < 24; i++)
            {
                float angle = MathHelper.TwoPi * i / 24f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(7f, 12f);
                Color col = i % 2 == 0
                    ? ClairDeLunePalette.GetClockworkGradient((float)i / 24f)
                    : ClairDeLunePalette.GetPearlGradient((float)i / 24f);
                int dustType = i % 3 == 0 ? DustID.IceTorch : DustID.GemDiamond;
                Dust d = Dust.NewDustPerfect(pos, dustType, vel, 0, col, 1.7f);
                d.noGravity = true;
            }

            ClairDeLuneVFXLibrary.SpawnPearlBurst(pos, 14, 7f, 0.35f);
            ClairDeLuneVFXLibrary.SpawnGradientHaloRings(pos, 7, 0.4f);
            ClairDeLuneVFXLibrary.SpawnConvergingMist(pos, 8, 70f, 0.55f);
            ClairDeLuneVFXLibrary.SpawnMoonlitMist(pos, 6, 60f, 0.5f);
            ClairDeLuneVFXLibrary.SpawnStarlitSparkles(pos, 8, 45f, 0.25f);
            ClairDeLuneVFXLibrary.SpawnMusicNotes(pos, 10, 45f, 0.85f, 1.2f, 45);

            MagnumScreenEffects.AddScreenShake(9f);
            Lighting.AddLight(pos, ClairDeLunePalette.WhiteHot.ToVector3() * 2.0f);
        }
    }
}
