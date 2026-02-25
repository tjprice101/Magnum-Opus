using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.VFX.Bloom;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Content.ClairDeLune.Weapons.Melee
{
    // ═══════════════════════════════════════════════════════════════════════════
    //  CHRONOLOGICALITY — VFX
    //  Identity: Temporal Drill — time-rending mechanism, relentless and absolute.
    //  Every thrust tears through the fabric of time itself.
    //  Crimson temporal energy collides with clockwork brass precision.
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// VFX helper for the Chronologicality temporal drill weapon.
    /// Handles hold-item clockwork ambiance, world item bloom,
    /// drill trail particles, drill impacts, critical discharge explosions,
    /// temporal rift projectile trails and rift impact VFX.
    /// </summary>
    public static class ChronologicalityVFX
    {
        /// <summary>
        /// Ambient clockwork energy around the player while holding the drill.
        /// Temporal crimson sparks orbit with brass gear hints an ambient smoke.
        /// </summary>
        public static void HoldItemVFX(Player player, Item item)
        {
            if (Main.dedServ) return;

            Vector2 center = player.MountedCenter;
            float time = (float)Main.timeForVisualEffects;

            // 3-point orbiting temporal sparks — crimson and brass interleaved
            for (int i = 0; i < 3; i++)
            {
                float angle = time * 0.05f + MathHelper.TwoPi * i / 3f;
                float radius = 20f + MathF.Sin(time * 0.07f + i * 1.1f) * 4f;
                Vector2 motePos = center + angle.ToRotationVector2() * radius;

                if (Main.rand.NextBool(3))
                {
                    float progress = 0.3f + (float)i / 3f * 0.5f;
                    Color col = ClairDeLunePalette.PaletteLerp(ClairDeLunePalette.ChronologicalityBlade, progress);
                    int dustType = i % 2 == 0 ? DustID.Electric : DustID.IceTorch;
                    Dust d = Dust.NewDustPerfect(motePos, dustType, Vector2.Zero, 0, col, 0.8f);
                    d.noGravity = true;
                    d.fadeIn = 0.6f;
                }
            }

            // Brass gear hint — occasional clockwork sparkle
            if (Main.rand.NextBool(6))
            {
                Vector2 gearPos = center + Main.rand.NextVector2Circular(18f, 18f);
                Vector2 gearVel = new Vector2(Main.rand.NextFloat(-0.2f, 0.2f), -0.3f - Main.rand.NextFloat(0.2f));
                Color brassCol = ClairDeLunePalette.GetClockworkGradient(Main.rand.NextFloat(0.4f, 0.8f));
                Dust d = Dust.NewDustPerfect(gearPos, DustID.GemDiamond, gearVel, 0, brassCol, 0.7f);
                d.noGravity = true;
            }

            // Temporal crimson mote drift
            if (Main.rand.NextBool(5))
            {
                Vector2 crimsonPos = center + Main.rand.NextVector2Circular(22f, 22f);
                Vector2 crimsonVel = new Vector2(Main.rand.NextFloat(-0.3f, 0.3f), -0.5f - Main.rand.NextFloat(0.3f));
                Color col = ClairDeLunePalette.GetTemporalGradient(Main.rand.NextFloat(0.3f, 0.7f));
                Dust d = Dust.NewDustPerfect(crimsonPos, DustID.Electric, crimsonVel, 0, col, 0.9f);
                d.noGravity = true;
            }

            // Ambient clockwork aura
            ClairDeLuneVFXLibrary.AmbientClockworkAura(center, time, 30f);

            // Rare music note
            if (Main.rand.NextBool(25))
                ClairDeLuneVFXLibrary.SpawnMusicNotes(center, 1, 20f, 0.7f, 0.9f, 30);

            float pulse = 0.5f + MathF.Sin(time * 0.05f) * 0.15f;
            Lighting.AddLight(center, ClairDeLunePalette.TemporalCrimson.ToVector3() * pulse);
        }

        /// <summary>
        /// 3-layer PreDrawInWorld bloom for the Chronologicality drill.
        /// Temporal crimson outer, clockwork brass mid, pearl white inner.
        /// </summary>
        public static void PreDrawInWorldBloom(
            Microsoft.Xna.Framework.Graphics.SpriteBatch sb,
            Microsoft.Xna.Framework.Graphics.Texture2D tex,
            Vector2 pos, Vector2 origin, float rotation, float scale)
        {
            float time = (float)Main.timeForVisualEffects;
            float pulse = 1f + MathF.Sin(time * 0.04f) * 0.03f;
            float colorShift = MathF.Sin(time * 0.03f) * 0.5f + 0.5f;

            Color outerCol = Color.Lerp(ClairDeLunePalette.TemporalCrimson, ClairDeLunePalette.MidnightBlue, colorShift * 0.4f);
            Color midCol = Color.Lerp(ClairDeLunePalette.ClockworkBrass, ClairDeLunePalette.SoftBlue, colorShift * 0.3f);

            // Layer 1: Outer temporal crimson aura
            sb.Draw(tex, pos, null, ClairDeLunePalette.Additive(outerCol, 0.38f), rotation, origin,
                scale * 1.10f * pulse, Microsoft.Xna.Framework.Graphics.SpriteEffects.None, 0f);
            // Layer 2: Middle clockwork brass glow
            sb.Draw(tex, pos, null, ClairDeLunePalette.Additive(midCol, 0.28f), rotation, origin,
                scale * 1.05f * pulse, Microsoft.Xna.Framework.Graphics.SpriteEffects.None, 0f);
            // Layer 3: Inner pearl white core
            sb.Draw(tex, pos, null, ClairDeLunePalette.Additive(ClairDeLunePalette.PearlWhite, 0.22f), rotation, origin,
                scale * 1.01f * pulse, Microsoft.Xna.Framework.Graphics.SpriteEffects.None, 0f);
        }

        /// <summary>
        /// Dense temporal particle trail behind the drill during contact.
        /// Crimson, brass, and electric blue dust interleaved with music notes.
        /// </summary>
        public static void DrillTrailVFX(Vector2 pos, Vector2 velocity)
        {
            if (Main.dedServ) return;

            Vector2 away = -velocity.SafeNormalize(Vector2.Zero);

            // Temporal crimson dust (2 particles)
            for (int i = 0; i < 2; i++)
            {
                Vector2 vel = away * Main.rand.NextFloat(2f, 4f) + Main.rand.NextVector2Circular(0.8f, 0.8f);
                Color col = ClairDeLunePalette.GetTemporalGradient(Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(pos, DustID.Electric, vel, 0, col, 1.4f);
                d.noGravity = true;
            }

            // Clockwork brass dust
            {
                Vector2 vel = away * Main.rand.NextFloat(1.5f, 3f) + Main.rand.NextVector2Circular(0.6f, 0.6f);
                Color col = ClairDeLunePalette.GetClockworkGradient(Main.rand.NextFloat(0.3f, 0.9f));
                Dust d = Dust.NewDustPerfect(pos, DustID.GemDiamond, vel, 0, col, 1.2f);
                d.noGravity = true;
            }

            // Electric blue accent
            {
                Vector2 vel = away * Main.rand.NextFloat(1f, 2.5f) + Main.rand.NextVector2Circular(0.5f, 0.5f);
                Color col = ClairDeLunePalette.GetPearlGradient(Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(pos, DustID.IceTorch, vel, 0, col, 1.3f);
                d.noGravity = true;
            }

            // Periodic music notes along trail
            if (Main.rand.NextBool(4))
                ClairDeLuneVFXLibrary.SpawnMusicNotes(pos, 1, 8f, 0.7f, 0.9f, 22);

            Lighting.AddLight(pos, ClairDeLunePalette.TemporalCrimson.ToVector3() * 0.6f);
        }

        /// <summary>
        /// Temporal drill impact burst with clockwork gear scatter,
        /// pearl burst, and screen shake.
        /// </summary>
        public static void DrillImpactVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            ClairDeLuneVFXLibrary.MeleeImpact(pos, 1);

            // Clockwork gear scatter — 10-point radial brass burst
            for (int i = 0; i < 10; i++)
            {
                float angle = MathHelper.TwoPi * i / 10f;
                float progress = (float)i / 10f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 6f);
                Color col = ClairDeLunePalette.GetClockworkGradient(progress);
                Dust d = Dust.NewDustPerfect(pos, DustID.GemDiamond, vel, 0, col, 1.3f);
                d.noGravity = true;
            }

            // Pearl burst at impact center
            ClairDeLuneVFXLibrary.SpawnPearlBurst(pos, 8, 4f, 0.3f);

            // Music note scatter
            ClairDeLuneVFXLibrary.SpawnMusicNotes(pos, 3, 25f, 0.75f, 1.0f, 30);

            MagnumScreenEffects.AddScreenShake(3f);

            Lighting.AddLight(pos, ClairDeLunePalette.ClockworkBrass.ToVector3() * 0.9f);
        }

        /// <summary>
        /// Massive lightning-style critical discharge explosion.
        /// Crimson and brass radial burst, heavy music notes, screen shake.
        /// The temporal drill's ultimate payoff on critical hits.
        /// </summary>
        public static void CriticalDischargeVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            ClairDeLuneVFXLibrary.DrawBloom(pos, 0.9f);

            // 20-point radial crimson + brass burst
            for (int i = 0; i < 20; i++)
            {
                float angle = MathHelper.TwoPi * i / 20f;
                float progress = (float)i / 20f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(6f, 10f);

                // Alternate between temporal crimson and clockwork brass gradient
                Color col = i % 2 == 0
                    ? ClairDeLunePalette.GetTemporalGradient(progress)
                    : ClairDeLunePalette.GetClockworkGradient(progress);

                int dustType = i % 3 == 0 ? DustID.Electric : DustID.IceTorch;
                Dust d = Dust.NewDustPerfect(pos, dustType, vel, 0, col, 1.7f);
                d.noGravity = true;
            }

            // Lightning arc dust — 8 electric arcs radiating outward
            for (int i = 0; i < 8; i++)
            {
                float baseAngle = MathHelper.TwoPi * i / 8f;
                for (int j = 0; j < 4; j++)
                {
                    float jitter = Main.rand.NextFloat(-0.3f, 0.3f);
                    float dist = 8f + j * 10f;
                    Vector2 arcPos = pos + (baseAngle + jitter).ToRotationVector2() * dist;
                    Color col = ClairDeLunePalette.GetTemporalGradient(0.5f + (float)j / 8f);
                    Dust d = Dust.NewDustPerfect(arcPos, DustID.Electric,
                        Main.rand.NextVector2Circular(1f, 1f), 0, col, 1.1f);
                    d.noGravity = true;
                }
            }

            // Pearl burst
            ClairDeLuneVFXLibrary.SpawnPearlBurst(pos, 12, 6f, 0.35f);

            // Gradient halo rings
            ClairDeLuneVFXLibrary.SpawnGradientHaloRings(pos, 6, 0.35f);

            // Heavy music note shower
            ClairDeLuneVFXLibrary.SpawnMusicNotes(pos, 8, 40f, 0.8f, 1.2f, 40);

            // Converging mist for aftermath
            ClairDeLuneVFXLibrary.SpawnConvergingMist(pos, 6, 60f, 0.5f);

            MagnumScreenEffects.AddScreenShake(7f);

            Lighting.AddLight(pos, ClairDeLunePalette.WhiteHot.ToVector3() * 1.8f);
        }

        /// <summary>
        /// Clockwork gear trail for temporal rift projectiles.
        /// Brass and pearl gradient dust following the rift's path.
        /// </summary>
        public static void TemporalRiftTrailVFX(Vector2 pos, Vector2 velocity)
        {
            if (Main.dedServ) return;

            Vector2 away = -velocity.SafeNormalize(Vector2.Zero);

            // Brass gear dust
            {
                Vector2 vel = away * Main.rand.NextFloat(1f, 2.5f) + Main.rand.NextVector2Circular(0.5f, 0.5f);
                Color col = ClairDeLunePalette.GetClockworkGradient(Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(pos, DustID.GemDiamond, vel, 0, col, 1.1f);
                d.noGravity = true;
            }

            // Temporal shimmer accent
            if (Main.rand.NextBool(2))
            {
                Vector2 vel = away * Main.rand.NextFloat(0.5f, 1.5f) + Main.rand.NextVector2Circular(0.4f, 0.4f);
                Color col = ClairDeLunePalette.GetTemporalGradient(Main.rand.NextFloat(0.3f, 0.8f));
                Dust d = Dust.NewDustPerfect(pos, DustID.Electric, vel, 0, col, 0.9f);
                d.noGravity = true;
            }

            // Pearl sparkle every other frame
            ClairDeLuneVFXLibrary.SpawnPearlSparkle(pos, away);

            Lighting.AddLight(pos, ClairDeLunePalette.ClockworkBrass.ToVector3() * 0.45f);
        }

        /// <summary>
        /// Temporal rift impact VFX with pearl burst and lightning arc dust.
        /// The rift collapses in a shower of clockwork debris and moonlit energy.
        /// </summary>
        public static void TemporalRiftImpactVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            ClairDeLuneVFXLibrary.ProjectileImpact(pos, 0.7f);

            // Pearl burst at rift collapse
            ClairDeLuneVFXLibrary.SpawnPearlBurst(pos, 10, 5f, 0.3f);

            // Lightning arc dust ring — 6 arcs
            for (int i = 0; i < 6; i++)
            {
                float baseAngle = MathHelper.TwoPi * i / 6f;
                for (int j = 0; j < 3; j++)
                {
                    float jitter = Main.rand.NextFloat(-0.4f, 0.4f);
                    float dist = 6f + j * 8f;
                    Vector2 arcPos = pos + (baseAngle + jitter).ToRotationVector2() * dist;
                    Color col = ClairDeLunePalette.GetTemporalGradient((float)j / 6f + 0.3f);
                    Dust d = Dust.NewDustPerfect(arcPos, DustID.Electric,
                        Main.rand.NextVector2Circular(0.8f, 0.8f), 0, col, 1.0f);
                    d.noGravity = true;
                }
            }

            // Music notes on rift collapse
            ClairDeLuneVFXLibrary.SpawnMusicNotes(pos, 3, 20f, 0.75f, 1.0f, 28);

            Lighting.AddLight(pos, ClairDeLunePalette.PearlBlue.ToVector3() * 0.8f);
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════
    //  TEMPORAL PIERCER — VFX
    //  Identity: Time-shattering lance of crystalline frost.
    //  Precise, piercing, crystalline. Every thrust is a needle through eternity.
    //  Moonlit frost collides with pearl brilliance in each strike.
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// VFX helper for the Temporal Piercer lance weapon.
    /// Handles hold-item frost shimmer, world item bloom, thrust trail particles,
    /// thrust impacts, and the massive time fracture detonation.
    /// </summary>
    public static class TemporalPiercerVFX
    {
        /// <summary>
        /// Crystal frost shimmer around the player while holding the lance.
        /// Pearl sparkle accents with ambient moonlit aura.
        /// </summary>
        public static void HoldItemVFX(Player player, Item item)
        {
            if (Main.dedServ) return;

            Vector2 center = player.MountedCenter;
            float time = (float)Main.timeForVisualEffects;

            // Crystal frost shimmer — orbiting frost motes
            for (int i = 0; i < 2; i++)
            {
                float angle = time * 0.04f + MathHelper.Pi * i;
                float radius = 17f + MathF.Sin(time * 0.06f + i * 0.8f) * 3f;
                Vector2 motePos = center + angle.ToRotationVector2() * radius;

                if (Main.rand.NextBool(3))
                {
                    float progress = (float)i / 2f;
                    Color col = ClairDeLunePalette.PaletteLerp(ClairDeLunePalette.TemporalPiercerLance, 0.3f + progress * 0.4f);
                    Dust d = Dust.NewDustPerfect(motePos, DustID.IceTorch, Vector2.Zero, 0, col, 0.75f);
                    d.noGravity = true;
                    d.fadeIn = 0.5f;
                }
            }

            // Pearl sparkle accents
            if (Main.rand.NextBool(7))
            {
                Vector2 sparkPos = center + Main.rand.NextVector2Circular(20f, 20f);
                Vector2 sparkVel = new Vector2(Main.rand.NextFloat(-0.2f, 0.2f), -0.3f - Main.rand.NextFloat(0.2f));
                Color col = ClairDeLunePalette.GetPearlGradient(Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(sparkPos, DustID.GemDiamond, sparkVel, 0, col, 0.7f);
                d.noGravity = true;
            }

            // Ambient dreamy aura
            ClairDeLuneVFXLibrary.AmbientDreamyAura(center, time, 28f);

            // Rare music note
            if (Main.rand.NextBool(28))
                ClairDeLuneVFXLibrary.SpawnMusicNotes(center, 1, 18f, 0.7f, 0.85f, 28);

            float pulse = 0.4f + MathF.Sin(time * 0.05f) * 0.12f;
            Lighting.AddLight(center, ClairDeLunePalette.MoonlitFrost.ToVector3() * pulse);
        }

        /// <summary>
        /// 3-layer PreDrawInWorld bloom for the Temporal Piercer lance.
        /// Frost-tinted layers shifting between moonlit frost and pearl blue.
        /// </summary>
        public static void PreDrawInWorldBloom(
            Microsoft.Xna.Framework.Graphics.SpriteBatch sb,
            Microsoft.Xna.Framework.Graphics.Texture2D tex,
            Vector2 pos, Vector2 origin, float rotation, float scale)
        {
            float time = (float)Main.timeForVisualEffects;
            float pulse = 1f + MathF.Sin(time * 0.05f) * 0.03f;
            float colorShift = MathF.Sin(time * 0.04f) * 0.5f + 0.5f;

            Color outerCol = Color.Lerp(ClairDeLunePalette.MidnightBlue, ClairDeLunePalette.MoonlitFrost, colorShift * 0.3f);
            Color midCol = Color.Lerp(ClairDeLunePalette.SoftBlue, ClairDeLunePalette.PearlBlue, colorShift * 0.4f);

            // Layer 1: Outer frost-midnight aura
            sb.Draw(tex, pos, null, ClairDeLunePalette.Additive(outerCol, 0.38f), rotation, origin,
                scale * 1.09f * pulse, Microsoft.Xna.Framework.Graphics.SpriteEffects.None, 0f);
            // Layer 2: Middle pearl-frost glow
            sb.Draw(tex, pos, null, ClairDeLunePalette.Additive(midCol, 0.30f), rotation, origin,
                scale * 1.04f * pulse, Microsoft.Xna.Framework.Graphics.SpriteEffects.None, 0f);
            // Layer 3: Inner crystal white core
            sb.Draw(tex, pos, null, ClairDeLunePalette.Additive(ClairDeLunePalette.WhiteHot, 0.20f), rotation, origin,
                scale * 1.01f * pulse, Microsoft.Xna.Framework.Graphics.SpriteEffects.None, 0f);
        }

        /// <summary>
        /// Crystal trail behind the lance during thrust.
        /// Frost dust with pearl sparks, intensified for power thrusts.
        /// </summary>
        public static void ThrustTrailVFX(Vector2 pos, Vector2 velocity, bool isPowerThrust)
        {
            if (Main.dedServ) return;

            Vector2 away = -velocity.SafeNormalize(Vector2.Zero);
            int dustCount = isPowerThrust ? 3 : 2;
            float speedMul = isPowerThrust ? 1.5f : 1f;
            float scaleMul = isPowerThrust ? 1.4f : 1f;

            // Crystal frost dust
            for (int i = 0; i < dustCount; i++)
            {
                Vector2 vel = away * Main.rand.NextFloat(1.5f, 3.5f) * speedMul
                    + Main.rand.NextVector2Circular(0.6f, 0.6f);
                Color col = ClairDeLunePalette.PaletteLerp(ClairDeLunePalette.TemporalPiercerLance, Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(pos, DustID.IceTorch, vel, 0, col, 1.3f * scaleMul);
                d.noGravity = true;
            }

            // Pearl diamond sparkle
            {
                Vector2 vel = away * Main.rand.NextFloat(1f, 2f) * speedMul
                    + Main.rand.NextVector2Circular(0.4f, 0.4f);
                Color col = ClairDeLunePalette.GetPearlGradient(Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(pos, DustID.GemDiamond, vel, 0, col, 1.0f * scaleMul);
                d.noGravity = true;
            }

            // Power thrust: extra electric accents
            if (isPowerThrust && Main.rand.NextBool(2))
            {
                Vector2 vel = away * Main.rand.NextFloat(2f, 4f) + Main.rand.NextVector2Circular(1f, 1f);
                Color col = ClairDeLunePalette.GetMoonlitGradient(Main.rand.NextFloat(0.5f, 1f));
                Dust d = Dust.NewDustPerfect(pos, DustID.Electric, vel, 0, col, 1.1f);
                d.noGravity = true;
            }

            // Music notes during trail
            if (Main.rand.NextBool(isPowerThrust ? 3 : 5))
                ClairDeLuneVFXLibrary.SpawnMusicNotes(pos, 1, 8f, 0.7f, 0.9f, 22);

            // Pearl sparkle trail
            ClairDeLuneVFXLibrary.SpawnPearlSparkle(pos, away);

            Color lightCol = isPowerThrust ? ClairDeLunePalette.PearlWhite : ClairDeLunePalette.MoonlitFrost;
            Lighting.AddLight(pos, lightCol.ToVector3() * (isPowerThrust ? 0.7f : 0.5f));
        }

        /// <summary>
        /// Crystal shatter burst on thrust impact.
        /// Pearl shimmer cascade with optional screen shake on power thrusts.
        /// </summary>
        public static void ThrustImpactVFX(Vector2 pos, bool isPowerThrust)
        {
            if (Main.dedServ) return;

            int comboStep = isPowerThrust ? 2 : 1;
            ClairDeLuneVFXLibrary.MeleeImpact(pos, comboStep);

            // Crystal shatter — radial frost burst
            int shardCount = isPowerThrust ? 14 : 8;
            for (int i = 0; i < shardCount; i++)
            {
                float angle = MathHelper.TwoPi * i / shardCount;
                float progress = (float)i / shardCount;
                float speed = isPowerThrust ? Main.rand.NextFloat(5f, 8f) : Main.rand.NextFloat(3f, 6f);
                Vector2 vel = angle.ToRotationVector2() * speed;
                Color col = ClairDeLunePalette.PaletteLerp(ClairDeLunePalette.TemporalPiercerLance, progress);
                Dust d = Dust.NewDustPerfect(pos, DustID.IceTorch, vel, 0, col, 1.4f);
                d.noGravity = true;
            }

            // Pearl shimmer burst
            ClairDeLuneVFXLibrary.SpawnPearlShimmer(pos, isPowerThrust ? 5 : 3, 30f, 0.28f);

            // Music notes
            int noteCount = isPowerThrust ? 5 : 3;
            ClairDeLuneVFXLibrary.SpawnMusicNotes(pos, noteCount, 25f, 0.75f, 1.0f, 30);

            // Power thrust: extra bloom, starlit sparkles, screen shake
            if (isPowerThrust)
            {
                ClairDeLuneVFXLibrary.DrawBloom(pos, 0.7f);
                ClairDeLuneVFXLibrary.SpawnStarlitSparkles(pos, 5, 25f, 0.22f);
                MagnumScreenEffects.AddScreenShake(4f);
            }

            Color lightCol = isPowerThrust ? ClairDeLunePalette.PearlWhite : ClairDeLunePalette.MoonlitFrost;
            Lighting.AddLight(pos, lightCol.ToVector3() * (isPowerThrust ? 1.2f : 0.8f));
        }

        /// <summary>
        /// Massive temporal explosion — the lance shatters time itself.
        /// Radial frost burst, converging mist, heavy music notes, and starlit cascade.
        /// The ultimate time-pierce detonation.
        /// </summary>
        public static void TimeFractureVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            ClairDeLuneVFXLibrary.DrawBloom(pos, 1.0f);

            // 24-point radial frost + pearl burst
            for (int i = 0; i < 24; i++)
            {
                float angle = MathHelper.TwoPi * i / 24f;
                float progress = (float)i / 24f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(7f, 12f);
                Color col = ClairDeLunePalette.PaletteLerp(ClairDeLunePalette.TemporalPiercerLance, progress);
                int dustType = i % 3 == 0 ? DustID.Electric : DustID.IceTorch;
                Dust d = Dust.NewDustPerfect(pos, dustType, vel, 0, col, 1.8f);
                d.noGravity = true;
            }

            // Inner pearl shatter ring
            for (int i = 0; i < 12; i++)
            {
                float angle = MathHelper.TwoPi * i / 12f + MathHelper.Pi / 12f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 7f);
                Color col = ClairDeLunePalette.GetPearlGradient((float)i / 12f);
                Dust d = Dust.NewDustPerfect(pos, DustID.GemDiamond, vel, 0, col, 1.5f);
                d.noGravity = true;
            }

            // Pearl burst
            ClairDeLuneVFXLibrary.SpawnPearlBurst(pos, 14, 7f, 0.35f);

            // Gradient halo rings
            ClairDeLuneVFXLibrary.SpawnGradientHaloRings(pos, 7, 0.4f);

            // Converging mist — temporal implosion
            ClairDeLuneVFXLibrary.SpawnConvergingMist(pos, 10, 80f, 0.6f);

            // Moonlit mist aftermath
            ClairDeLuneVFXLibrary.SpawnMoonlitMist(pos, 6, 60f, 0.55f);

            // Starlit sparkle cascade
            ClairDeLuneVFXLibrary.SpawnStarlitSparkles(pos, 10, 50f, 0.25f);

            // Heavy music note shower
            ClairDeLuneVFXLibrary.SpawnMusicNotes(pos, 10, 45f, 0.85f, 1.2f, 45);

            MagnumScreenEffects.AddScreenShake(9f);

            Lighting.AddLight(pos, ClairDeLunePalette.WhiteHot.ToVector3() * 2.0f);
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════
    //  CLOCKWORK HARMONY — VFX
    //  Identity: Mechanical greatsword, brass clockwork meets moonlit steel.
    //  Heavy, rhythmic, mechanical. Every swing is a clockwork pendulum.
    //  The culminating mechanism — every 5th swing unleashes synchronized chaos.
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// VFX helper for the Clockwork Harmony mechanical greatsword.
    /// Handles hold-item brass ambiance, world item bloom, swing trail particles,
    /// swing impacts, synchronized explosion (every 5th swing), gear wave
    /// projectile trails, and gear wave impact VFX.
    /// </summary>
    public static class ClockworkHarmonyVFX
    {
        /// <summary>
        /// Brass clockwork ambiance around the player while holding the greatsword.
        /// Gear shimmer with ambient clockwork aura and dreamy undertones.
        /// </summary>
        public static void HoldItemVFX(Player player, Item item)
        {
            if (Main.dedServ) return;

            Vector2 center = player.MountedCenter;
            float time = (float)Main.timeForVisualEffects;

            // 3-point orbiting brass gear motes
            for (int i = 0; i < 3; i++)
            {
                float angle = time * 0.035f + MathHelper.TwoPi * i / 3f;
                float radius = 22f + MathF.Sin(time * 0.05f + i * 1.3f) * 5f;
                Vector2 motePos = center + angle.ToRotationVector2() * radius;

                if (Main.rand.NextBool(4))
                {
                    float progress = 0.2f + (float)i / 3f * 0.5f;
                    Color col = ClairDeLunePalette.PaletteLerp(ClairDeLunePalette.ClockworkHarmonyBlade, progress);
                    Dust d = Dust.NewDustPerfect(motePos, DustID.GemDiamond, Vector2.Zero, 0, col, 0.8f);
                    d.noGravity = true;
                    d.fadeIn = 0.6f;
                }
            }

            // Gear shimmer — occasional brass sparkle with gold undertones
            if (Main.rand.NextBool(5))
            {
                Vector2 sparkPos = center + Main.rand.NextVector2Circular(24f, 24f);
                Vector2 sparkVel = new Vector2(Main.rand.NextFloat(-0.3f, 0.3f), -0.4f - Main.rand.NextFloat(0.3f));
                Color col = ClairDeLunePalette.GetClockworkGradient(Main.rand.NextFloat(0.3f, 0.9f));
                Dust d = Dust.NewDustPerfect(sparkPos, DustID.GemDiamond, sparkVel, 0, col, 0.85f);
                d.noGravity = true;
            }

            // Ambient clockwork aura
            ClairDeLuneVFXLibrary.AmbientClockworkAura(center, time, 35f);

            // Rare music note
            if (Main.rand.NextBool(22))
                ClairDeLuneVFXLibrary.SpawnMusicNotes(center, 1, 22f, 0.7f, 0.9f, 32);

            float pulse = 0.45f + MathF.Sin(time * 0.04f) * 0.12f;
            Lighting.AddLight(center, ClairDeLunePalette.ClockworkBrass.ToVector3() * pulse);
        }

        /// <summary>
        /// 3-layer PreDrawInWorld bloom for the Clockwork Harmony greatsword.
        /// Brass-gold outer layers shifting into moonbeam white core.
        /// </summary>
        public static void PreDrawInWorldBloom(
            Microsoft.Xna.Framework.Graphics.SpriteBatch sb,
            Microsoft.Xna.Framework.Graphics.Texture2D tex,
            Vector2 pos, Vector2 origin, float rotation, float scale)
        {
            float time = (float)Main.timeForVisualEffects;
            float pulse = 1f + MathF.Sin(time * 0.035f) * 0.03f;
            float colorShift = MathF.Sin(time * 0.03f) * 0.5f + 0.5f;

            Color outerCol = Color.Lerp(ClairDeLunePalette.NightMist, ClairDeLunePalette.ClockworkBrass, 0.4f + colorShift * 0.2f);
            Color midCol = Color.Lerp(ClairDeLunePalette.ClockworkBrass, ClairDeLunePalette.MoonbeamGold, colorShift * 0.4f);

            // Layer 1: Outer brass-night aura
            sb.Draw(tex, pos, null, ClairDeLunePalette.Additive(outerCol, 0.36f), rotation, origin,
                scale * 1.10f * pulse, Microsoft.Xna.Framework.Graphics.SpriteEffects.None, 0f);
            // Layer 2: Middle brass-gold glow
            sb.Draw(tex, pos, null, ClairDeLunePalette.Additive(midCol, 0.28f), rotation, origin,
                scale * 1.05f * pulse, Microsoft.Xna.Framework.Graphics.SpriteEffects.None, 0f);
            // Layer 3: Inner moonbeam white core
            sb.Draw(tex, pos, null, ClairDeLunePalette.Additive(ClairDeLunePalette.PearlWhite, 0.20f), rotation, origin,
                scale * 1.01f * pulse, Microsoft.Xna.Framework.Graphics.SpriteEffects.None, 0f);
        }

        /// <summary>
        /// Heavy brass-pearl trail during the greatsword swing.
        /// Gear particles interleave with dreamy moonlit dust and trailing music notes.
        /// </summary>
        public static void SwingTrailVFX(Vector2 pos, Vector2 velocity)
        {
            if (Main.dedServ) return;

            Vector2 away = -velocity.SafeNormalize(Vector2.Zero);

            // Heavy brass dust (2 particles)
            for (int i = 0; i < 2; i++)
            {
                Vector2 vel = away * Main.rand.NextFloat(2f, 4f) + Main.rand.NextVector2Circular(0.7f, 0.7f);
                Color col = ClairDeLunePalette.PaletteLerp(ClairDeLunePalette.ClockworkHarmonyBlade, Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(pos, DustID.GemDiamond, vel, 0, col, 1.4f);
                d.noGravity = true;
            }

            // Pearl shimmer accent
            {
                Vector2 vel = away * Main.rand.NextFloat(1f, 2.5f) + Main.rand.NextVector2Circular(0.5f, 0.5f);
                Color col = ClairDeLunePalette.GetPearlGradient(Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(pos, DustID.IceTorch, vel, 0, col, 1.2f);
                d.noGravity = true;
            }

            // Gear particle — occasional brass sparkle
            if (Main.rand.NextBool(3))
            {
                Vector2 vel = away * Main.rand.NextFloat(1.5f, 3f) + Main.rand.NextVector2Circular(0.8f, 0.8f);
                Color col = ClairDeLunePalette.GetClockworkGradient(Main.rand.NextFloat(0.4f, 1f));
                Dust d = Dust.NewDustPerfect(pos, DustID.GemDiamond, vel, 0, col, 1.1f);
                d.noGravity = true;
            }

            // Moonlit swing dust
            ClairDeLuneVFXLibrary.SpawnMoonlitSwingDust(pos, away);

            // Periodic music notes
            if (Main.rand.NextBool(4))
                ClairDeLuneVFXLibrary.SpawnMusicNotes(pos, 1, 10f, 0.7f, 0.9f, 24);

            Lighting.AddLight(pos, ClairDeLunePalette.ClockworkBrass.ToVector3() * 0.55f);
        }

        /// <summary>
        /// Clockwork impact burst with gear cascade.
        /// Radial brass gradient burst with pearl shimmer and music notes.
        /// </summary>
        public static void SwingImpactVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            ClairDeLuneVFXLibrary.MeleeImpact(pos, 1);

            // Gear cascade — 12-point radial brass gradient burst
            for (int i = 0; i < 12; i++)
            {
                float angle = MathHelper.TwoPi * i / 12f;
                float progress = (float)i / 12f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 7f);
                Color col = ClairDeLunePalette.GetClockworkGradient(progress);
                Dust d = Dust.NewDustPerfect(pos, DustID.GemDiamond, vel, 0, col, 1.4f);
                d.noGravity = true;
            }

            // Pearl shimmer at impact center
            ClairDeLuneVFXLibrary.SpawnPearlShimmer(pos, 4, 28f, 0.26f);

            // Music notes
            ClairDeLuneVFXLibrary.SpawnMusicNotes(pos, 4, 25f, 0.75f, 1.0f, 28);

            Lighting.AddLight(pos, ClairDeLunePalette.MoonbeamGold.ToVector3() * 0.9f);
        }

        /// <summary>
        /// Massive clockwork explosionon every 5th swing.
        /// Huge radial brass-pearl burst, heavy screen shake, bloom,
        /// cascading music note spirals, converging mist, and starlit sparks.
        /// The greatsword's ultimate synchronized detonation.
        /// </summary>
        public static void SynchronizedExplosionVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            ClairDeLuneVFXLibrary.DrawBloom(pos, 1.1f);

            // 24-point radial brass + pearl mega-burst
            for (int i = 0; i < 24; i++)
            {
                float angle = MathHelper.TwoPi * i / 24f;
                float progress = (float)i / 24f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(8f, 13f);

                // Alternate brass and pearl gradient
                Color col = i % 2 == 0
                    ? ClairDeLunePalette.GetClockworkGradient(progress)
                    : ClairDeLunePalette.GetPearlGradient(progress);

                int dustType = i % 3 == 0 ? DustID.IceTorch : DustID.GemDiamond;
                Dust d = Dust.NewDustPerfect(pos, dustType, vel, 0, col, 1.8f);
                d.noGravity = true;
            }

            // Inner moonbeam gold ring
            for (int i = 0; i < 12; i++)
            {
                float angle = MathHelper.TwoPi * i / 12f + MathHelper.Pi / 12f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(5f, 8f);
                Color col = Color.Lerp(ClairDeLunePalette.ClockworkBrass, ClairDeLunePalette.MoonbeamGold, (float)i / 12f);
                Dust d = Dust.NewDustPerfect(pos, DustID.GemDiamond, vel, 0, col, 1.6f);
                d.noGravity = true;
            }

            // Pearl burst cascade
            ClairDeLuneVFXLibrary.SpawnPearlBurst(pos, 16, 8f, 0.4f);

            // Gradient halo rings — maximum cascade
            ClairDeLuneVFXLibrary.SpawnGradientHaloRings(pos, 8, 0.4f);

            // Cascading music note spirals — 12 notes spiraling outward
            ClairDeLuneVFXLibrary.SpawnMusicNotes(pos, 12, 50f, 0.85f, 1.3f, 50);

            // Converging mist implosion
            ClairDeLuneVFXLibrary.SpawnConvergingMist(pos, 10, 90f, 0.6f);

            // Moonlit mist aftermath
            ClairDeLuneVFXLibrary.SpawnMoonlitMist(pos, 8, 70f, 0.55f);

            // Starlit sparkle cascade
            ClairDeLuneVFXLibrary.SpawnStarlitSparkles(pos, 12, 55f, 0.28f);

            // Pearl shimmer shower
            ClairDeLuneVFXLibrary.SpawnPearlShimmer(pos, 8, 45f, 0.3f);

            MagnumScreenEffects.AddScreenShake(10f);

            Lighting.AddLight(pos, ClairDeLunePalette.WhiteHot.ToVector3() * 2.5f);
        }

        /// <summary>
        /// Brass gear trail for gear wave projectiles launched by the greatsword.
        /// Clockwork gradient dust with pearl accent and dreamy shimmer.
        /// </summary>
        public static void GearWaveTrailVFX(Vector2 pos, Vector2 velocity)
        {
            if (Main.dedServ) return;

            Vector2 away = -velocity.SafeNormalize(Vector2.Zero);

            // Brass gear dust
            {
                Vector2 vel = away * Main.rand.NextFloat(1f, 2.5f) + Main.rand.NextVector2Circular(0.5f, 0.5f);
                Color col = ClairDeLunePalette.GetClockworkGradient(Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(pos, DustID.GemDiamond, vel, 0, col, 1.2f);
                d.noGravity = true;
            }

            // Pearl shimmer accent
            if (Main.rand.NextBool(2))
            {
                Vector2 vel = away * Main.rand.NextFloat(0.5f, 1.5f) + Main.rand.NextVector2Circular(0.4f, 0.4f);
                Color col = ClairDeLunePalette.GetPearlGradient(Main.rand.NextFloat(0.3f, 0.8f));
                Dust d = Dust.NewDustPerfect(pos, DustID.IceTorch, vel, 0, col, 1.0f);
                d.noGravity = true;
            }

            // Moonlit swing dust for the dreamy undertone
            ClairDeLuneVFXLibrary.SpawnPearlSparkle(pos, away);

            Lighting.AddLight(pos, ClairDeLunePalette.ClockworkBrass.ToVector3() * 0.4f);
        }

        /// <summary>
        /// Gear wave impact VFX when the projectile hits an enemy.
        /// Clockwork radial burst with pearl burst cascade.
        /// </summary>
        public static void GearWaveImpactVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            ClairDeLuneVFXLibrary.ProjectileImpact(pos, 0.6f);

            // Clockwork gear scatter — 8-point radial brass burst
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                float progress = (float)i / 8f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 5f);
                Color col = ClairDeLunePalette.GetClockworkGradient(progress);
                Dust d = Dust.NewDustPerfect(pos, DustID.GemDiamond, vel, 0, col, 1.2f);
                d.noGravity = true;
            }

            // Pearl burst
            ClairDeLuneVFXLibrary.SpawnPearlBurst(pos, 8, 4f, 0.28f);

            // Music notes on impact
            ClairDeLuneVFXLibrary.SpawnMusicNotes(pos, 3, 20f, 0.75f, 1.0f, 26);

            Lighting.AddLight(pos, ClairDeLunePalette.MoonbeamGold.ToVector3() * 0.7f);
        }
    }
}
