using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.VFX.Bloom;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Content.ClairDeLune.Weapons.Magic
{
    // =============================================================================
    //  CLOCKWORK GRIMOIRE VFX
    //  Identity: Temporal Tome with 4 spell modes (Lightning, Crystal, Gear, Time
    //  Fracture). Arcane pages of temporal knowledge — mystical, channeled, building.
    //  Every spell is a page turning in eternity.
    // =============================================================================

    /// <summary>
    /// VFX helper for the Clockwork Grimoire temporal tome weapon.
    /// Handles hold-item ambient, world item bloom, mode switching flash,
    /// and per-mode cast / trail / impact VFX across 4 spell modes:
    /// 0 = Lightning, 1 = Crystal, 2 = Gear, 3 = Time Fracture.
    /// Call from ClockworkGrimoire and its projectile classes.
    /// </summary>
    public static class ClockworkGrimoireVFX
    {
        // =====================================================================
        //  HOLD ITEM VFX
        // =====================================================================

        /// <summary>
        /// Per-frame held-item VFX: dreamy arcane ambiance with soft blue mist,
        /// occasional music notes, and pearl sparkle orbiting the player.
        /// Call from HoldItem().
        /// </summary>
        public static void HoldItemVFX(Player player, Item item)
        {
            if (Main.dedServ) return;

            Vector2 center = player.MountedCenter;
            float time = (float)Main.timeForVisualEffects;

            // 3-point orbiting arcane motes
            for (int i = 0; i < 3; i++)
            {
                float angle = time * 0.03f + MathHelper.TwoPi * i / 3f;
                float radius = 20f + MathF.Sin(time * 0.05f + i) * 5f;
                Vector2 motePos = center + angle.ToRotationVector2() * radius;

                if (Main.rand.NextBool(4))
                {
                    float progress = 0.3f + (float)i / 3f * 0.4f;
                    Color col = ClairDeLunePalette.PaletteLerp(ClairDeLunePalette.ClockworkGrimoireCast, progress);
                    Dust d = Dust.NewDustPerfect(motePos, DustID.IceTorch, Vector2.Zero, 0, col, 0.7f);
                    d.noGravity = true;
                    d.fadeIn = 0.5f;
                }
            }

            // Soft blue mist drift
            if (Main.rand.NextBool(6))
                ClairDeLuneVFXLibrary.SpawnMoonlitMist(center, 1, 30f, 0.3f);

            // Pearl sparkle accent
            if (Main.rand.NextBool(5))
                ClairDeLuneVFXLibrary.SpawnPearlShimmer(center, 1, 22f, 0.2f);

            // Periodic music notes
            if (Main.rand.NextBool(25))
                ClairDeLuneVFXLibrary.SpawnMusicNotes(center, 1, 20f, 0.7f, 0.9f, 30);

            // Pulsing dreamy glow
            float pulse = 0.4f + MathF.Sin(time * 0.05f) * 0.12f;
            Lighting.AddLight(center, ClairDeLunePalette.SoftBlue.ToVector3() * pulse);
        }

        // =====================================================================
        //  PREDRAW IN WORLD BLOOM
        // =====================================================================

        /// <summary>
        /// 3-layer bloom for Clockwork Grimoire when lying in the world.
        /// Soft blue tints with gentle pulsing moonlit aura.
        /// Call from PreDrawInWorld with additive SpriteBatch.
        /// </summary>
        public static void PreDrawInWorldBloom(
            Microsoft.Xna.Framework.Graphics.SpriteBatch sb,
            Microsoft.Xna.Framework.Graphics.Texture2D tex,
            Vector2 pos, Vector2 origin, float rotation, float scale)
        {
            float time = (float)Main.timeForVisualEffects;
            float pulse = 1f + MathF.Sin(time * 0.04f) * 0.03f;
            ClairDeLunePalette.DrawItemBloom(sb, tex, pos, origin, rotation, scale, pulse);
        }

        // =====================================================================
        //  SPELL MODE SWITCH VFX
        // =====================================================================

        /// <summary>
        /// Mode change flash with gear orbit dust and mode-specific color burst.
        /// Mode 0 = electric blue, 1 = crystal frost, 2 = clockwork brass, 3 = temporal crimson.
        /// Call when the player cycles between spell modes.
        /// </summary>
        public static void SpellModeSwitchVFX(Vector2 pos, int newMode)
        {
            if (Main.dedServ) return;

            // Central flash
            ClairDeLuneVFXLibrary.DrawBloom(pos, 0.4f);

            // Mode-specific accent color
            Color modeColor = newMode switch
            {
                0 => ClairDeLunePalette.SoftBlue,             // Electric
                1 => ClairDeLunePalette.MoonlitFrost,         // Crystal
                2 => ClairDeLunePalette.ClockworkBrass,       // Gear
                3 => ClairDeLunePalette.TemporalCrimson,      // Time Fracture
                _ => ClairDeLunePalette.PearlBlue,
            };

            // Gear orbit dust ring (8 points)
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                float progress = (float)i / 8f;
                Vector2 dustPos = pos + angle.ToRotationVector2() * 25f;
                Vector2 vel = angle.ToRotationVector2() * 2f;
                Color col = Color.Lerp(modeColor, ClairDeLunePalette.PearlWhite, progress * 0.4f);
                Dust d = Dust.NewDustPerfect(dustPos, DustID.IceTorch, vel, 0, col, 1.3f);
                d.noGravity = true;
            }

            // Pearl burst at switch
            ClairDeLuneVFXLibrary.SpawnPearlBurst(pos, 6, 3f, 0.25f);

            // Music notes for mode transition
            ClairDeLuneVFXLibrary.SpawnMusicNotes(pos, 2, 18f, 0.7f, 0.9f, 25);

            Lighting.AddLight(pos, modeColor.ToVector3() * 0.7f);
        }

        // =====================================================================
        //  MODE 0: LIGHTNING — Electric blue bolt VFX
        // =====================================================================

        /// <summary>
        /// Electric blue bolt cast VFX with pearl shimmer burst at the cast origin.
        /// Call when the lightning projectile is spawned.
        /// </summary>
        public static void LightningCastVFX(Vector2 pos, Vector2 direction)
        {
            if (Main.dedServ) return;

            ClairDeLuneVFXLibrary.DrawBloom(pos, 0.45f);

            // Directional electric burst
            for (int i = 0; i < 6; i++)
            {
                Vector2 vel = direction * Main.rand.NextFloat(3f, 6f)
                    + Main.rand.NextVector2Circular(2f, 2f);
                float progress = (float)i / 6f;
                Color col = ClairDeLunePalette.GetGradient(progress);
                Dust d = Dust.NewDustPerfect(pos, DustID.Electric, vel, 0, col, 1.2f);
                d.noGravity = true;
            }

            // Pearl shimmer at origin
            ClairDeLuneVFXLibrary.SpawnPearlShimmer(pos, 3, 15f, 0.25f);

            // Music notes
            ClairDeLuneVFXLibrary.SpawnMusicNotes(pos, 2, 15f, 0.7f, 0.9f, 25);

            Lighting.AddLight(pos, ClairDeLunePalette.SoftBlue.ToVector3() * 0.8f);
        }

        /// <summary>
        /// Electric blue dust trail with sparkle accents for the lightning bolt projectile.
        /// Call every frame from the projectile AI.
        /// </summary>
        public static void LightningTrailVFX(Vector2 pos, Vector2 velocity)
        {
            if (Main.dedServ) return;

            Vector2 away = -velocity.SafeNormalize(Vector2.Zero);

            // Electric trail dust
            for (int i = 0; i < 2; i++)
            {
                Vector2 vel = away * Main.rand.NextFloat(1f, 2.5f) + Main.rand.NextVector2Circular(0.5f, 0.5f);
                float progress = Main.rand.NextFloat();
                Color col = ClairDeLunePalette.GetGradient(progress);
                Dust d = Dust.NewDustPerfect(pos + Main.rand.NextVector2Circular(4f, 4f),
                    DustID.Electric, vel, 0, col, 1.3f);
                d.noGravity = true;
                d.fadeIn = 1.1f;
            }

            // Sparkle accent
            if (Main.rand.NextBool(3))
            {
                Color sparkCol = ClairDeLunePalette.GetPearlGradient(Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(pos + Main.rand.NextVector2Circular(6f, 6f),
                    DustID.GemDiamond, away * 0.5f, 0, sparkCol, 0.8f);
                d.noGravity = true;
            }

            Lighting.AddLight(pos, ClairDeLunePalette.SoftBlue.ToVector3() * 0.5f);
        }

        /// <summary>
        /// Electric burst impact VFX with pearl shimmer and music notes.
        /// Call when the lightning bolt hits a target or expires.
        /// </summary>
        public static void LightningImpactVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            ClairDeLuneVFXLibrary.DrawBloom(pos, 0.5f);

            // Electric radial burst
            for (int i = 0; i < 10; i++)
            {
                float angle = MathHelper.TwoPi * i / 10f;
                float progress = (float)i / 10f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 6f);
                Color col = ClairDeLunePalette.GetGradient(progress);
                Dust d = Dust.NewDustPerfect(pos, DustID.Electric, vel, 0, col, 1.4f);
                d.noGravity = true;
            }

            // Pearl shimmer burst
            ClairDeLuneVFXLibrary.SpawnPearlShimmer(pos, 4, 20f, 0.28f);

            // Music note scatter
            ClairDeLuneVFXLibrary.SpawnMusicNotes(pos, 3, 20f, 0.7f, 1.0f, 28);

            // Starlit sparkle accent
            ClairDeLuneVFXLibrary.SpawnStarlitSparkles(pos, 3, 18f, 0.2f);

            Lighting.AddLight(pos, ClairDeLunePalette.PearlBlue.ToVector3() * 0.9f);
        }

        // =====================================================================
        //  MODE 1: CRYSTAL — Crystal shard spray VFX
        // =====================================================================

        /// <summary>
        /// Crystal shard spray cast VFX with moonlit frost dust at the cast origin.
        /// Call when crystal shard projectiles are spawned.
        /// </summary>
        public static void CrystalCastVFX(Vector2 pos, Vector2 direction)
        {
            if (Main.dedServ) return;

            ClairDeLuneVFXLibrary.DrawBloom(pos, 0.4f);

            // Crystal frost spray
            for (int i = 0; i < 8; i++)
            {
                Vector2 vel = direction * Main.rand.NextFloat(2f, 5f)
                    + Main.rand.NextVector2Circular(2.5f, 2.5f);
                float progress = (float)i / 8f;
                Color col = ClairDeLunePalette.GetPearlGradient(progress);
                Dust d = Dust.NewDustPerfect(pos, DustID.GemDiamond, vel, 0, col, 1.1f);
                d.noGravity = true;
                d.fadeIn = 1.0f;
            }

            // Moonlit frost accent
            for (int i = 0; i < 3; i++)
            {
                Vector2 vel = direction * Main.rand.NextFloat(1f, 3f) + Main.rand.NextVector2Circular(1f, 1f);
                Color col = Color.Lerp(ClairDeLunePalette.MoonlitFrost, ClairDeLunePalette.PearlWhite,
                    Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(pos, DustID.IceTorch, vel, 0, col, 0.9f);
                d.noGravity = true;
            }

            ClairDeLuneVFXLibrary.SpawnMusicNotes(pos, 2, 12f, 0.7f, 0.9f, 25);

            Lighting.AddLight(pos, ClairDeLunePalette.MoonlitFrost.ToVector3() * 0.7f);
        }

        /// <summary>
        /// Crystal dust trail with moonlit frost accents for crystal shard projectiles.
        /// Call every frame from the projectile AI.
        /// </summary>
        public static void CrystalTrailVFX(Vector2 pos, Vector2 velocity)
        {
            if (Main.dedServ) return;

            Vector2 away = -velocity.SafeNormalize(Vector2.Zero);

            // Crystal dust trail
            Vector2 vel = away * Main.rand.NextFloat(1f, 2f) + Main.rand.NextVector2Circular(0.4f, 0.4f);
            float progress = Main.rand.NextFloat();
            Color col = ClairDeLunePalette.GetPearlGradient(progress);
            Dust d = Dust.NewDustPerfect(pos + Main.rand.NextVector2Circular(3f, 3f),
                DustID.GemDiamond, vel, 0, col, 1.0f);
            d.noGravity = true;

            // Frost accent (1-in-3)
            if (Main.rand.NextBool(3))
            {
                Color frostCol = Color.Lerp(ClairDeLunePalette.MoonlitFrost, ClairDeLunePalette.PearlWhite,
                    Main.rand.NextFloat());
                Dust f = Dust.NewDustPerfect(pos + Main.rand.NextVector2Circular(5f, 5f),
                    DustID.IceTorch, away * 0.5f, 0, frostCol, 0.7f);
                f.noGravity = true;
            }

            Lighting.AddLight(pos, ClairDeLunePalette.MoonlitFrost.ToVector3() * 0.4f);
        }

        /// <summary>
        /// Crystal shatter burst VFX with pearl burst on impact.
        /// Call when a crystal shard hits a target or expires.
        /// </summary>
        public static void CrystalImpactVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            ClairDeLuneVFXLibrary.DrawBloom(pos, 0.45f);

            // Crystal shatter burst
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                float progress = (float)i / 8f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 5f);
                Color col = ClairDeLunePalette.GetPearlGradient(progress);
                Dust d = Dust.NewDustPerfect(pos, DustID.GemDiamond, vel, 0, col, 1.3f);
                d.noGravity = true;
            }

            // Pearl burst
            ClairDeLuneVFXLibrary.SpawnPearlBurst(pos, 6, 4f, 0.25f);

            // Frost ring
            ClairDeLuneVFXLibrary.SpawnGradientHaloRings(pos, 3, 0.25f);

            // Music notes
            ClairDeLuneVFXLibrary.SpawnMusicNotes(pos, 2, 18f, 0.7f, 1.0f, 26);

            Lighting.AddLight(pos, ClairDeLunePalette.PearlWhite.ToVector3() * 0.8f);
        }

        // =====================================================================
        //  MODE 2: GEAR — Clockwork gear spray VFX
        // =====================================================================

        /// <summary>
        /// Clockwork gear spray cast VFX with brass dust at the cast origin.
        /// Call when gear projectiles are spawned.
        /// </summary>
        public static void GearCastVFX(Vector2 pos, Vector2 direction)
        {
            if (Main.dedServ) return;

            ClairDeLuneVFXLibrary.DrawBloom(pos, 0.4f);

            // Brass gear dust spray
            for (int i = 0; i < 6; i++)
            {
                Vector2 vel = direction * Main.rand.NextFloat(2f, 5f)
                    + Main.rand.NextVector2Circular(2f, 2f);
                float progress = (float)i / 6f;
                Color col = ClairDeLunePalette.GetClockworkGradient(progress);
                Dust d = Dust.NewDustPerfect(pos, DustID.GemDiamond, vel, 0, col, 1.2f);
                d.noGravity = true;
            }

            // Clockwork shimmer accents
            for (int i = 0; i < 3; i++)
            {
                Vector2 vel = direction * Main.rand.NextFloat(1f, 3f) + Main.rand.NextVector2Circular(1f, 1f);
                Color col = Color.Lerp(ClairDeLunePalette.ClockworkBrass, ClairDeLunePalette.MoonbeamGold,
                    Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(pos, DustID.IceTorch, vel, 0, col, 0.9f);
                d.noGravity = true;
            }

            ClairDeLuneVFXLibrary.SpawnMusicNotes(pos, 2, 15f, 0.7f, 0.9f, 25);

            Lighting.AddLight(pos, ClairDeLunePalette.ClockworkBrass.ToVector3() * 0.7f);
        }

        /// <summary>
        /// Brass gear dust trail for clockwork gear projectiles.
        /// Call every frame from the projectile AI.
        /// </summary>
        public static void GearTrailVFX(Vector2 pos, Vector2 velocity)
        {
            if (Main.dedServ) return;

            Vector2 away = -velocity.SafeNormalize(Vector2.Zero);

            // Brass trail dust
            Vector2 vel = away * Main.rand.NextFloat(1f, 2f) + Main.rand.NextVector2Circular(0.4f, 0.4f);
            float progress = Main.rand.NextFloat();
            Color col = ClairDeLunePalette.GetClockworkGradient(progress);
            Dust d = Dust.NewDustPerfect(pos + Main.rand.NextVector2Circular(4f, 4f),
                DustID.GemDiamond, vel, 0, col, 1.0f);
            d.noGravity = true;

            // Moonbeam gold accent (1-in-4)
            if (Main.rand.NextBool(4))
            {
                Color goldCol = Color.Lerp(ClairDeLunePalette.ClockworkBrass, ClairDeLunePalette.MoonbeamGold,
                    Main.rand.NextFloat());
                Dust g = Dust.NewDustPerfect(pos + Main.rand.NextVector2Circular(5f, 5f),
                    DustID.IceTorch, away * 0.3f, 0, goldCol, 0.7f);
                g.noGravity = true;
            }

            Lighting.AddLight(pos, ClairDeLunePalette.ClockworkBrass.ToVector3() * 0.4f);
        }

        /// <summary>
        /// Clockwork burst impact VFX with pearl shimmer on gear hit.
        /// Call when a gear projectile hits a target or expires.
        /// </summary>
        public static void GearImpactVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            ClairDeLuneVFXLibrary.DrawBloom(pos, 0.45f);

            // Clockwork burst
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                float progress = (float)i / 8f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 5f);
                Color col = ClairDeLunePalette.GetClockworkGradient(progress);
                Dust d = Dust.NewDustPerfect(pos, DustID.GemDiamond, vel, 0, col, 1.3f);
                d.noGravity = true;
            }

            // Pearl shimmer
            ClairDeLuneVFXLibrary.SpawnPearlShimmer(pos, 3, 18f, 0.25f);

            // Halo rings
            ClairDeLuneVFXLibrary.SpawnGradientHaloRings(pos, 3, 0.25f);

            // Music notes
            ClairDeLuneVFXLibrary.SpawnMusicNotes(pos, 2, 18f, 0.7f, 1.0f, 26);

            Lighting.AddLight(pos, ClairDeLunePalette.MoonbeamGold.ToVector3() * 0.8f);
        }

        // =====================================================================
        //  MODE 3: TIME FRACTURE — Temporal crimson energy VFX
        // =====================================================================

        /// <summary>
        /// Temporal crimson energy release cast VFX with heavy bloom at the cast origin.
        /// The most powerful and dramatic spell mode. Call when time fracture is cast.
        /// </summary>
        public static void TimeFractureCastVFX(Vector2 pos, Vector2 direction)
        {
            if (Main.dedServ) return;

            // Heavy bloom flash
            ClairDeLuneVFXLibrary.DrawBloom(pos, 0.6f);

            // Temporal crimson energy directional burst
            for (int i = 0; i < 8; i++)
            {
                Vector2 vel = direction * Main.rand.NextFloat(3f, 7f)
                    + Main.rand.NextVector2Circular(2.5f, 2.5f);
                float progress = (float)i / 8f;
                Color col = ClairDeLunePalette.GetTemporalGradient(progress);
                Dust d = Dust.NewDustPerfect(pos, DustID.Electric, vel, 0, col, 1.4f);
                d.noGravity = true;
                d.fadeIn = 1.2f;
            }

            // Pearl white accent burst
            for (int i = 0; i < 4; i++)
            {
                Vector2 vel = direction * Main.rand.NextFloat(2f, 4f) + Main.rand.NextVector2Circular(1.5f, 1.5f);
                Color col = ClairDeLunePalette.GetPearlGradient(Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(pos, DustID.GemDiamond, vel, 0, col, 1.0f);
                d.noGravity = true;
            }

            // Pearl shimmer
            ClairDeLuneVFXLibrary.SpawnPearlShimmer(pos, 4, 20f, 0.28f);

            // Music notes
            ClairDeLuneVFXLibrary.SpawnMusicNotes(pos, 3, 18f, 0.8f, 1.0f, 28);

            MagnumScreenEffects.AddScreenShake(2f);

            Lighting.AddLight(pos, ClairDeLunePalette.TemporalCrimson.ToVector3() * 0.9f);
        }

        /// <summary>
        /// Crimson and pearl temporal trail VFX for time fracture projectiles.
        /// Call every frame from the projectile AI.
        /// </summary>
        public static void TimeFractureTrailVFX(Vector2 pos, Vector2 velocity)
        {
            if (Main.dedServ) return;

            Vector2 away = -velocity.SafeNormalize(Vector2.Zero);

            // Temporal crimson trail
            for (int i = 0; i < 2; i++)
            {
                Vector2 vel = away * Main.rand.NextFloat(1f, 2.5f) + Main.rand.NextVector2Circular(0.6f, 0.6f);
                float progress = Main.rand.NextFloat();
                Color col = ClairDeLunePalette.GetTemporalGradient(progress);
                Dust d = Dust.NewDustPerfect(pos + Main.rand.NextVector2Circular(5f, 5f),
                    DustID.Electric, vel, 0, col, 1.3f);
                d.noGravity = true;
                d.fadeIn = 1.1f;
            }

            // Pearl accent (1-in-3)
            if (Main.rand.NextBool(3))
            {
                Color pearlCol = ClairDeLunePalette.GetPearlGradient(Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(pos + Main.rand.NextVector2Circular(6f, 6f),
                    DustID.GemDiamond, away * 0.5f, 0, pearlCol, 0.8f);
                d.noGravity = true;
            }

            Lighting.AddLight(pos, ClairDeLunePalette.TemporalCrimson.ToVector3() * 0.5f);
        }

        /// <summary>
        /// Temporal explosion VFX with converging mist, heavy music note scatter,
        /// and cascading halo rings. The most dramatic impact in the grimoire's arsenal.
        /// Call when a time fracture projectile hits a target or expires.
        /// </summary>
        public static void TimeFractureImpactVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            // Heavy bloom
            ClairDeLuneVFXLibrary.DrawBloom(pos, 0.7f);

            // Temporal crimson radial burst
            for (int i = 0; i < 12; i++)
            {
                float angle = MathHelper.TwoPi * i / 12f;
                float progress = (float)i / 12f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 7f);
                Color col = ClairDeLunePalette.GetTemporalGradient(progress);
                Dust d = Dust.NewDustPerfect(pos, DustID.Electric, vel, 0, col, 1.5f);
                d.noGravity = true;
            }

            // Pearl burst cascade
            ClairDeLuneVFXLibrary.SpawnPearlBurst(pos, 8, 5f, 0.3f);

            // Converging mist
            ClairDeLuneVFXLibrary.SpawnConvergingMist(pos, 6, 50f, 0.5f);

            // Cascading halo rings
            ClairDeLuneVFXLibrary.SpawnGradientHaloRings(pos, 5, 0.3f);

            // Heavy music note shower
            ClairDeLuneVFXLibrary.SpawnMusicNotes(pos, 5, 30f, 0.8f, 1.1f, 35);

            // Starlit sparkles
            ClairDeLuneVFXLibrary.SpawnStarlitSparkles(pos, 5, 25f, 0.22f);

            MagnumScreenEffects.AddScreenShake(4f);

            Lighting.AddLight(pos, ClairDeLunePalette.WhiteHot.ToVector3() * 1.0f);
        }
    }

    // =============================================================================
    //  ORRERY OF DREAMS VFX
    //  Identity: Celestial Staff with dreamy orbiting spheres. Cosmic, orbiting,
    //  dreamlike. Every sphere is a dream in motion across the moonlit sky.
    //  Starlight silver trails, dream haze mist, moonbeam gold accents.
    // =============================================================================

    /// <summary>
    /// VFX helper for the Orrery of Dreams celestial staff weapon.
    /// Handles hold-item ambient, world item bloom, sphere cast, orbit trail,
    /// ambient orbit effects, sphere impact, and on-kill enhanced detonation.
    /// Call from OrreryOfDreams and its orb projectile classes.
    /// </summary>
    public static class OrreryOfDreamsVFX
    {
        // =====================================================================
        //  HOLD ITEM VFX
        // =====================================================================

        /// <summary>
        /// Per-frame held-item VFX: dreamy cosmic ambiance with dream haze mist,
        /// starlight sparkles, pearl shimmer, and occasional music notes.
        /// Call from HoldItem().
        /// </summary>
        public static void HoldItemVFX(Player player, Item item)
        {
            if (Main.dedServ) return;

            Vector2 center = player.MountedCenter;
            float time = (float)Main.timeForVisualEffects;

            // 4-point orbiting celestial motes
            for (int i = 0; i < 4; i++)
            {
                float angle = time * 0.025f + MathHelper.TwoPi * i / 4f;
                float radius = 22f + MathF.Sin(time * 0.04f + i * 0.9f) * 6f;
                Vector2 motePos = center + angle.ToRotationVector2() * radius;

                if (Main.rand.NextBool(4))
                {
                    float progress = (float)i / 4f;
                    Color col = ClairDeLunePalette.PaletteLerp(ClairDeLunePalette.OrreryOfDreamsOrbit, progress);
                    Dust d = Dust.NewDustPerfect(motePos, DustID.IceTorch, Vector2.Zero, 0, col, 0.6f);
                    d.noGravity = true;
                    d.fadeIn = 0.5f;
                }
            }

            // Dream haze mist drift
            if (Main.rand.NextBool(5))
                ClairDeLuneVFXLibrary.SpawnMoonlitMist(center, 1, 35f, 0.25f);

            // Starlight sparkles
            if (Main.rand.NextBool(5))
                ClairDeLuneVFXLibrary.SpawnStarlitSparkles(center, 1, 28f, 0.18f);

            // Pearl shimmer
            if (Main.rand.NextBool(6))
                ClairDeLuneVFXLibrary.SpawnPearlShimmer(center, 1, 25f, 0.2f);

            // Periodic music notes
            if (Main.rand.NextBool(22))
                ClairDeLuneVFXLibrary.SpawnMusicNotes(center, 1, 22f, 0.7f, 0.9f, 32);

            // Pulsing dream haze glow
            float pulse = 0.35f + MathF.Sin(time * 0.04f) * 0.1f;
            Lighting.AddLight(center, ClairDeLunePalette.DreamHaze.ToVector3() * pulse);
        }

        // =====================================================================
        //  PREDRAW IN WORLD BLOOM
        // =====================================================================

        /// <summary>
        /// 3-layer bloom for Orrery of Dreams when lying in the world.
        /// Dream haze tints with gentle cosmic pulsing aura.
        /// Call from PreDrawInWorld with additive SpriteBatch.
        /// </summary>
        public static void PreDrawInWorldBloom(
            Microsoft.Xna.Framework.Graphics.SpriteBatch sb,
            Microsoft.Xna.Framework.Graphics.Texture2D tex,
            Vector2 pos, Vector2 origin, float rotation, float scale)
        {
            float time = (float)Main.timeForVisualEffects;
            float pulse = 1f + MathF.Sin(time * 0.04f) * 0.03f;

            // Layer 1: Outer midnight blue aura
            sb.Draw(tex, pos, null, ClairDeLunePalette.Additive(ClairDeLunePalette.MidnightBlue, 0.38f),
                rotation, origin, scale * 1.08f * pulse, Microsoft.Xna.Framework.Graphics.SpriteEffects.None, 0f);

            // Layer 2: Middle dream haze glow
            sb.Draw(tex, pos, null, ClairDeLunePalette.Additive(ClairDeLunePalette.DreamHaze, 0.30f),
                rotation, origin, scale * 1.04f * pulse, Microsoft.Xna.Framework.Graphics.SpriteEffects.None, 0f);

            // Layer 3: Inner starlight silver core
            sb.Draw(tex, pos, null, ClairDeLunePalette.Additive(ClairDeLunePalette.StarlightSilver, 0.22f),
                rotation, origin, scale * 1.01f * pulse, Microsoft.Xna.Framework.Graphics.SpriteEffects.None, 0f);

            Lighting.AddLight(pos + Main.screenPosition, ClairDeLunePalette.DreamHaze.ToVector3() * 0.35f);
        }

        // =====================================================================
        //  CAST VFX
        // =====================================================================

        /// <summary>
        /// Celestial sphere launch VFX with starlight burst, moonbeam gold accent,
        /// and music notes at the cast origin. Call when the orb projectile is spawned.
        /// </summary>
        public static void CastVFX(Vector2 pos, Vector2 direction)
        {
            if (Main.dedServ) return;

            ClairDeLuneVFXLibrary.DrawBloom(pos, 0.5f);

            // Starlight burst in cast direction
            for (int i = 0; i < 8; i++)
            {
                Vector2 vel = direction * Main.rand.NextFloat(2f, 5f)
                    + Main.rand.NextVector2Circular(2f, 2f);
                float progress = (float)i / 8f;
                Color col = ClairDeLunePalette.PaletteLerp(ClairDeLunePalette.OrreryOfDreamsOrbit, progress);
                Dust d = Dust.NewDustPerfect(pos, DustID.IceTorch, vel, 0, col, 1.3f);
                d.noGravity = true;
                d.fadeIn = 1.1f;
            }

            // Moonbeam gold accent sparks
            for (int i = 0; i < 4; i++)
            {
                Vector2 vel = direction * Main.rand.NextFloat(1.5f, 3.5f) + Main.rand.NextVector2Circular(1.5f, 1.5f);
                Color col = Color.Lerp(ClairDeLunePalette.MoonbeamGold, ClairDeLunePalette.PearlWhite,
                    Main.rand.NextFloat(0.4f));
                Dust d = Dust.NewDustPerfect(pos, DustID.GemDiamond, vel, 0, col, 0.9f);
                d.noGravity = true;
            }

            // Starlight sparkle burst at origin
            ClairDeLuneVFXLibrary.SpawnStarlitSparkles(pos, 4, 18f, 0.22f);

            // Pearl shimmer
            ClairDeLuneVFXLibrary.SpawnPearlShimmer(pos, 3, 16f, 0.25f);

            // Music notes
            ClairDeLuneVFXLibrary.SpawnMusicNotes(pos, 3, 20f, 0.7f, 1.0f, 28);

            Lighting.AddLight(pos, ClairDeLunePalette.StarlightSilver.ToVector3() * 0.8f);
        }

        // =====================================================================
        //  ORB TRAIL VFX
        // =====================================================================

        /// <summary>
        /// Dreamy haze trail with starlight silver dust and occasional pearl sparkle.
        /// Call every frame from the orbiting sphere projectile AI.
        /// </summary>
        public static void OrbTrailVFX(Vector2 pos, Vector2 velocity)
        {
            if (Main.dedServ) return;

            Vector2 away = -velocity.SafeNormalize(Vector2.Zero);

            // Dreamy haze trail dust
            for (int i = 0; i < 2; i++)
            {
                Vector2 vel = away * Main.rand.NextFloat(0.8f, 2f) + Main.rand.NextVector2Circular(0.5f, 0.5f);
                float progress = Main.rand.NextFloat();
                Color col = Color.Lerp(ClairDeLunePalette.DreamHaze, ClairDeLunePalette.StarlightSilver, progress);
                Dust d = Dust.NewDustPerfect(pos + Main.rand.NextVector2Circular(4f, 4f),
                    DustID.IceTorch, vel, 0, col, 1.1f);
                d.noGravity = true;
                d.fadeIn = 1.0f;
            }

            // Starlight silver sparkle accent (1-in-3)
            if (Main.rand.NextBool(3))
            {
                Color sparkCol = Color.Lerp(ClairDeLunePalette.StarlightSilver, ClairDeLunePalette.PearlWhite,
                    Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(pos + Main.rand.NextVector2Circular(6f, 6f),
                    DustID.GemDiamond, away * 0.4f, 0, sparkCol, 0.7f);
                d.noGravity = true;
            }

            // Occasional pearl sparkle (1-in-6)
            if (Main.rand.NextBool(6))
                ClairDeLuneVFXLibrary.SpawnPearlShimmer(pos, 1, 8f, 0.18f);

            Lighting.AddLight(pos, ClairDeLunePalette.DreamHaze.ToVector3() * 0.45f);
        }

        // =====================================================================
        //  ORB AMBIENT VFX
        // =====================================================================

        /// <summary>
        /// Orbiting starlight dust ring (6-point) with dream haze mist drift.
        /// Call every frame from the orbiting sphere's ambient AI phase.
        /// </summary>
        public static void OrbAmbientVFX(Vector2 center)
        {
            if (Main.dedServ) return;

            float time = (float)Main.timeForVisualEffects;

            // 6-point orbiting starlight ring
            for (int i = 0; i < 6; i++)
            {
                float angle = time * 0.035f + MathHelper.TwoPi * i / 6f;
                float radius = 14f + MathF.Sin(time * 0.06f + i * 1.2f) * 4f;
                Vector2 dustPos = center + angle.ToRotationVector2() * radius;

                if (Main.rand.NextBool(3))
                {
                    float progress = (float)i / 6f;
                    Color col = Color.Lerp(ClairDeLunePalette.StarlightSilver, ClairDeLunePalette.PearlWhite, progress);
                    Dust d = Dust.NewDustPerfect(dustPos, DustID.GemDiamond, Vector2.Zero, 0, col, 0.6f);
                    d.noGravity = true;
                }
            }

            // Dream haze mist drift (1-in-8)
            if (Main.rand.NextBool(8))
                ClairDeLuneVFXLibrary.SpawnMoonlitMist(center, 1, 20f, 0.2f);

            // Pulsing ambient glow
            float pulse = 0.35f + MathF.Sin(time * 0.05f) * 0.1f;
            Lighting.AddLight(center, ClairDeLunePalette.DreamHaze.ToVector3() * pulse);
        }

        // =====================================================================
        //  ORB IMPACT VFX
        // =====================================================================

        /// <summary>
        /// Celestial detonation VFX with starlight burst, pearl shimmer,
        /// moonbeam gold accents, and music notes. Standard orb impact.
        /// Call when an orbiting sphere hits a target.
        /// </summary>
        public static void OrbImpactVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            // Celestial bloom flash
            ClairDeLuneVFXLibrary.DrawBloom(pos, 0.55f);

            // Starlight radial burst
            for (int i = 0; i < 10; i++)
            {
                float angle = MathHelper.TwoPi * i / 10f;
                float progress = (float)i / 10f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 6f);
                Color col = ClairDeLunePalette.PaletteLerp(ClairDeLunePalette.OrreryOfDreamsOrbit, progress);
                Dust d = Dust.NewDustPerfect(pos, DustID.IceTorch, vel, 0, col, 1.4f);
                d.noGravity = true;
            }

            // Moonbeam gold accent sparkles
            for (int i = 0; i < 4; i++)
            {
                float angle = MathHelper.TwoPi * i / 4f + Main.rand.NextFloat(-0.3f, 0.3f);
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 4f);
                Color col = Color.Lerp(ClairDeLunePalette.MoonbeamGold, ClairDeLunePalette.PearlWhite,
                    Main.rand.NextFloat(0.4f));
                Dust d = Dust.NewDustPerfect(pos, DustID.GemDiamond, vel, 0, col, 0.9f);
                d.noGravity = true;
            }

            // Pearl shimmer
            ClairDeLuneVFXLibrary.SpawnPearlShimmer(pos, 4, 22f, 0.28f);

            // Halo rings
            ClairDeLuneVFXLibrary.SpawnGradientHaloRings(pos, 4, 0.28f);

            // Starlight sparkle accent
            ClairDeLuneVFXLibrary.SpawnStarlitSparkles(pos, 4, 20f, 0.2f);

            // Music notes
            ClairDeLuneVFXLibrary.SpawnMusicNotes(pos, 3, 22f, 0.7f, 1.0f, 28);

            Lighting.AddLight(pos, ClairDeLunePalette.StarlightSilver.ToVector3() * 0.9f);
        }

        // =====================================================================
        //  ORB KILL VFX
        // =====================================================================

        /// <summary>
        /// Enhanced celestial explosion VFX with extra music notes, pearl burst,
        /// converging mist, and screen shake. Triggered on killing blow with an orb.
        /// </summary>
        public static void OrbKillVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            // Heavy bloom
            ClairDeLuneVFXLibrary.DrawBloom(pos, 0.7f);

            // Enhanced starlight radial burst
            for (int i = 0; i < 14; i++)
            {
                float angle = MathHelper.TwoPi * i / 14f;
                float progress = (float)i / 14f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 8f);
                Color col = ClairDeLunePalette.PaletteLerp(ClairDeLunePalette.OrreryOfDreamsOrbit, progress);
                Dust d = Dust.NewDustPerfect(pos, DustID.IceTorch, vel, 0, col, 1.6f);
                d.noGravity = true;
            }

            // Moonbeam gold accent sparkle ring
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 5f);
                Color col = Color.Lerp(ClairDeLunePalette.MoonbeamGold, ClairDeLunePalette.PearlWhite,
                    Main.rand.NextFloat(0.5f));
                Dust d = Dust.NewDustPerfect(pos, DustID.GemDiamond, vel, 0, col, 1.0f);
                d.noGravity = true;
            }

            // Pearl burst cascade
            ClairDeLuneVFXLibrary.SpawnPearlBurst(pos, 10, 6f, 0.32f);

            // Converging mist
            ClairDeLuneVFXLibrary.SpawnConvergingMist(pos, 6, 55f, 0.5f);

            // Cascading halo rings
            ClairDeLuneVFXLibrary.SpawnGradientHaloRings(pos, 6, 0.3f);

            // Heavy music note shower
            ClairDeLuneVFXLibrary.SpawnMusicNotes(pos, 6, 35f, 0.8f, 1.2f, 35);

            // Starlit sparkle scatter
            ClairDeLuneVFXLibrary.SpawnStarlitSparkles(pos, 6, 30f, 0.25f);

            MagnumScreenEffects.AddScreenShake(3f);

            Lighting.AddLight(pos, ClairDeLunePalette.WhiteHot.ToVector3() * 1.1f);
        }
    }

    // =============================================================================
    //  REQUIEM OF TIME VFX
    //  Identity: Time-Freeze Magic Sword with temporal crimson blade. Sweeping,
    //  freezing, absolute. Every arc stops the clock. Temporal crimson energy
    //  married with pearl blue moonlit shimmer for the supreme final boss tier.
    // =============================================================================

    /// <summary>
    /// VFX helper for the Requiem of Time time-freeze magic sword weapon.
    /// Handles hold-item ambient, world item bloom, swing trail, swing impact,
    /// time freeze charge / release, and projectile trail / impact VFX.
    /// Call from RequiemOfTime, RequiemOfTimeSwing, and time-blade projectile classes.
    /// </summary>
    public static class RequiemOfTimeVFX
    {
        // =====================================================================
        //  HOLD ITEM VFX
        // =====================================================================

        /// <summary>
        /// Per-frame held-item VFX: temporal crimson energy with pearl blue accents,
        /// ambient moonlit mist, and occasional music notes.
        /// Call from HoldItem().
        /// </summary>
        public static void HoldItemVFX(Player player, Item item)
        {
            if (Main.dedServ) return;

            Vector2 center = player.MountedCenter;
            float time = (float)Main.timeForVisualEffects;

            // 3-point orbiting temporal motes
            for (int i = 0; i < 3; i++)
            {
                float angle = time * 0.035f + MathHelper.TwoPi * i / 3f;
                float radius = 18f + MathF.Sin(time * 0.055f + i) * 5f;
                Vector2 motePos = center + angle.ToRotationVector2() * radius;

                if (Main.rand.NextBool(4))
                {
                    float progress = (float)i / 3f;
                    Color col = ClairDeLunePalette.PaletteLerp(ClairDeLunePalette.RequiemOfTimeSweep, progress);
                    Dust d = Dust.NewDustPerfect(motePos, DustID.IceTorch, Vector2.Zero, 0, col, 0.7f);
                    d.noGravity = true;
                    d.fadeIn = 0.5f;
                }
            }

            // Temporal crimson wisps near blade
            if (Main.rand.NextBool(6))
            {
                Vector2 tipOffset = new Vector2(player.direction * 22f, -6f);
                Vector2 tipPos = center + tipOffset + Main.rand.NextVector2Circular(8f, 8f);
                Color col = ClairDeLunePalette.GetTemporalGradient(Main.rand.NextFloat(0.3f, 0.7f));
                Dust d = Dust.NewDustPerfect(tipPos, DustID.Electric, new Vector2(0, -0.3f), 0, col, 0.6f);
                d.noGravity = true;
            }

            // Pearl blue accent sparkle
            if (Main.rand.NextBool(7))
            {
                Vector2 sparkPos = center + Main.rand.NextVector2Circular(22f, 22f);
                Color sparkCol = ClairDeLunePalette.GetPearlGradient(Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(sparkPos, DustID.GemDiamond, Vector2.Zero, 0, sparkCol, 0.5f);
                d.noGravity = true;
            }

            // Ambient moonlit mist
            if (Main.rand.NextBool(8))
                ClairDeLuneVFXLibrary.SpawnMoonlitMist(center, 1, 28f, 0.25f);

            // Periodic music notes
            if (Main.rand.NextBool(25))
                ClairDeLuneVFXLibrary.SpawnMusicNotes(center, 1, 18f, 0.7f, 0.9f, 30);

            // Pulsing temporal glow
            float pulse = 0.35f + MathF.Sin(time * 0.045f) * 0.12f;
            Lighting.AddLight(center, ClairDeLunePalette.TemporalCrimson.ToVector3() * pulse);
        }

        // =====================================================================
        //  PREDRAW IN WORLD BLOOM
        // =====================================================================

        /// <summary>
        /// 3-layer bloom for Requiem of Time when lying in the world.
        /// Crimson-pearl tints with temporal pulsing aura.
        /// Call from PreDrawInWorld with additive SpriteBatch.
        /// </summary>
        public static void PreDrawInWorldBloom(
            Microsoft.Xna.Framework.Graphics.SpriteBatch sb,
            Microsoft.Xna.Framework.Graphics.Texture2D tex,
            Vector2 pos, Vector2 origin, float rotation, float scale)
        {
            float time = (float)Main.timeForVisualEffects;
            float pulse = 1f + MathF.Sin(time * 0.04f) * 0.03f;

            // Layer 1: Outer midnight blue aura
            sb.Draw(tex, pos, null, ClairDeLunePalette.Additive(ClairDeLunePalette.MidnightBlue, 0.38f),
                rotation, origin, scale * 1.08f * pulse, Microsoft.Xna.Framework.Graphics.SpriteEffects.None, 0f);

            // Layer 2: Middle temporal crimson glow
            sb.Draw(tex, pos, null, ClairDeLunePalette.Additive(ClairDeLunePalette.TemporalCrimson, 0.28f),
                rotation, origin, scale * 1.04f * pulse, Microsoft.Xna.Framework.Graphics.SpriteEffects.None, 0f);

            // Layer 3: Inner pearl blue core
            sb.Draw(tex, pos, null, ClairDeLunePalette.Additive(ClairDeLunePalette.PearlBlue, 0.22f),
                rotation, origin, scale * 1.01f * pulse, Microsoft.Xna.Framework.Graphics.SpriteEffects.None, 0f);

            Lighting.AddLight(pos + Main.screenPosition, ClairDeLunePalette.TemporalCrimson.ToVector3() * 0.35f);
        }

        // =====================================================================
        //  SWING TRAIL VFX
        // =====================================================================

        /// <summary>
        /// Per-frame swing VFX: temporal crimson swing dust with pearl sparkle
        /// and periodic music notes. Intensity scales with combo step.
        /// Call from the swing projectile's AI() every frame.
        /// </summary>
        public static void SwingTrailVFX(Vector2 tipPos, Vector2 swordDirection, int comboStep, int timer)
        {
            if (Main.dedServ) return;

            Vector2 away = -swordDirection;

            // Temporal crimson swing dust
            for (int i = 0; i < 2; i++)
            {
                Vector2 vel = away * Main.rand.NextFloat(1f, 3f) + Main.rand.NextVector2Circular(0.5f, 0.5f);
                float progress = Main.rand.NextFloat();
                Color col = ClairDeLunePalette.GetTemporalGradient(progress);
                Dust d = Dust.NewDustPerfect(tipPos, DustID.Electric, vel, 0, col, 1.4f);
                d.noGravity = true;
                d.fadeIn = 1.2f;
            }

            // Pearl blue accent dust
            if (Main.rand.NextBool(2))
            {
                Vector2 vel = away * Main.rand.NextFloat(0.5f, 2f) + Main.rand.NextVector2Circular(0.4f, 0.4f);
                Color col = ClairDeLunePalette.GetPearlGradient(Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(tipPos, DustID.GemDiamond, vel, 0, col, 0.9f);
                d.noGravity = true;
            }

            // Pearl sparkle (intensifies with combo)
            if (timer % (4 - Math.Min(comboStep, 2)) == 0)
                ClairDeLuneVFXLibrary.SpawnPearlShimmer(tipPos, 1, 10f, 0.2f);

            // Periodic music notes
            if (timer % 5 == 0)
                ClairDeLuneVFXLibrary.SpawnMusicNotes(tipPos, 1, 10f, 0.7f, 0.9f, 25);

            // Dynamic light
            Color lightCol = ClairDeLunePalette.PaletteLerp(ClairDeLunePalette.RequiemOfTimeSweep,
                0.4f + comboStep * 0.15f);
            Lighting.AddLight(tipPos, lightCol.ToVector3() * 0.6f);
        }

        // =====================================================================
        //  SWING IMPACT VFX
        // =====================================================================

        /// <summary>
        /// Temporal melee impact VFX with crimson burst and pearl shimmer.
        /// Scales with combo step for heavier hits. Call from OnHitNPC in the swing projectile.
        /// </summary>
        public static void SwingImpactVFX(Vector2 pos, int comboStep)
        {
            if (Main.dedServ) return;

            // Base melee impact (bloom, halos, radial dust, music notes)
            ClairDeLuneVFXLibrary.MeleeImpact(pos, comboStep);

            // Extra temporal crimson burst
            for (int i = 0; i < 4 + comboStep * 2; i++)
            {
                float angle = MathHelper.TwoPi * i / (4 + comboStep * 2);
                float progress = (float)i / (4 + comboStep * 2);
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 4f);
                Color col = ClairDeLunePalette.GetTemporalGradient(progress);
                Dust d = Dust.NewDustPerfect(pos, DustID.Electric, vel, 0, col, 1.2f);
                d.noGravity = true;
            }

            // Pearl shimmer accent
            ClairDeLuneVFXLibrary.SpawnPearlShimmer(pos, 2 + comboStep, 20f, 0.25f);

            // Starlit sparkles on later combos
            if (comboStep >= 1)
                ClairDeLuneVFXLibrary.SpawnStarlitSparkles(pos, 3, 18f, 0.2f);

            Lighting.AddLight(pos, ClairDeLunePalette.TemporalCrimson.ToVector3() * (0.7f + comboStep * 0.15f));
        }

        // =====================================================================
        //  TIME FREEZE CHARGE VFX
        // =====================================================================

        /// <summary>
        /// Converging temporal crimson particles channeling toward the sword for
        /// the time freeze special ability. Progress 0.0 to 1.0 controls intensity.
        /// Call every frame during the time freeze charge-up.
        /// </summary>
        public static void TimeFreezeChargeVFX(Vector2 pos, float progress)
        {
            if (Main.dedServ) return;

            float time = (float)Main.timeForVisualEffects;
            float intensity = MathHelper.Clamp(progress, 0f, 1f);

            // Converging temporal crimson particles
            int particleCount = (int)(4 + intensity * 8);
            for (int i = 0; i < particleCount; i++)
            {
                float angle = Main.rand.NextFloat() * MathHelper.TwoPi;
                float dist = 50f + Main.rand.NextFloat(30f) * (1f - intensity * 0.5f);
                Vector2 particlePos = pos + angle.ToRotationVector2() * dist;
                Vector2 vel = (pos - particlePos).SafeNormalize(Vector2.Zero) * (2f + intensity * 4f);

                float gradientT = Main.rand.NextFloat();
                Color col = ClairDeLunePalette.GetTemporalGradient(gradientT) * (0.4f + intensity * 0.5f);
                Dust d = Dust.NewDustPerfect(particlePos, DustID.Electric, vel, 0, col, 0.8f + intensity * 0.6f);
                d.noGravity = true;
                d.fadeIn = 1.0f;
            }

            // Pearl blue converging accent (1-in-3)
            if (Main.rand.NextBool(3))
            {
                float angle = Main.rand.NextFloat() * MathHelper.TwoPi;
                float dist = 40f + Main.rand.NextFloat(25f);
                Vector2 particlePos = pos + angle.ToRotationVector2() * dist;
                Vector2 vel = (pos - particlePos).SafeNormalize(Vector2.Zero) * 3f;
                Color col = ClairDeLunePalette.GetPearlGradient(Main.rand.NextFloat()) * 0.5f;
                Dust d = Dust.NewDustPerfect(particlePos, DustID.GemDiamond, vel, 0, col, 0.7f);
                d.noGravity = true;
            }

            // Converging mist (scales with progress)
            if (Main.rand.NextBool(4) && intensity > 0.3f)
                ClairDeLuneVFXLibrary.SpawnConvergingMist(pos, 2, 45f * (1.5f - intensity), 0.35f + intensity * 0.2f);

            // Central glow intensifies
            float pulse = 0.3f + intensity * 0.5f + MathF.Sin(time * 0.08f) * 0.1f;
            Lighting.AddLight(pos, ClairDeLunePalette.TemporalCrimson.ToVector3() * pulse);
        }

        // =====================================================================
        //  TIME FREEZE RELEASE VFX
        // =====================================================================

        /// <summary>
        /// Massive time freeze explosion with heavy radial burst, pearl cascade,
        /// screen shake, bloom, and music note shower. The ultimate Requiem of Time
        /// moment — when time itself shatters.
        /// Call when the time freeze ability triggers.
        /// </summary>
        public static void TimeFreezeReleaseVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            // Massive bloom flash
            ClairDeLuneVFXLibrary.DrawBloom(pos, 0.9f);

            // Heavy temporal crimson radial burst
            for (int i = 0; i < 18; i++)
            {
                float angle = MathHelper.TwoPi * i / 18f;
                float progress = (float)i / 18f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(5f, 10f);
                Color col = ClairDeLunePalette.GetTemporalGradient(progress);
                Dust d = Dust.NewDustPerfect(pos, DustID.Electric, vel, 0, col, 1.8f);
                d.noGravity = true;
                d.fadeIn = 1.4f;
            }

            // Pearl white radial ring
            for (int i = 0; i < 12; i++)
            {
                float angle = MathHelper.TwoPi * i / 12f;
                float progress = (float)i / 12f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 7f);
                Color col = ClairDeLunePalette.GetPearlGradient(progress);
                Dust d = Dust.NewDustPerfect(pos, DustID.GemDiamond, vel, 0, col, 1.5f);
                d.noGravity = true;
            }

            // Pearl cascade burst
            ClairDeLuneVFXLibrary.SpawnPearlBurst(pos, 14, 7f, 0.35f);

            // Converging mist
            ClairDeLuneVFXLibrary.SpawnConvergingMist(pos, 10, 80f, 0.6f);

            // Moonlit mist explosion
            ClairDeLuneVFXLibrary.SpawnMoonlitMist(pos, 8, 60f, 0.55f);

            // Cascading gradient halo rings
            ClairDeLuneVFXLibrary.SpawnGradientHaloRings(pos, 7, 0.35f);

            // Starlit sparkle scatter
            ClairDeLuneVFXLibrary.SpawnStarlitSparkles(pos, 10, 45f, 0.28f);

            // Heavy music note shower
            ClairDeLuneVFXLibrary.SpawnMusicNotes(pos, 8, 45f, 0.8f, 1.2f, 40);

            // Screen shake — time shatters
            MagnumScreenEffects.AddScreenShake(8f);

            Lighting.AddLight(pos, ClairDeLunePalette.WhiteHot.ToVector3() * 1.5f);
        }

        // =====================================================================
        //  PROJECTILE TRAIL VFX
        // =====================================================================

        /// <summary>
        /// Temporal crimson dust trail for spawned time-blade projectiles.
        /// Call every frame from the time-blade projectile AI.
        /// </summary>
        public static void ProjectileTrailVFX(Vector2 pos, Vector2 velocity)
        {
            if (Main.dedServ) return;

            Vector2 away = -velocity.SafeNormalize(Vector2.Zero);

            // Temporal crimson trail
            for (int i = 0; i < 2; i++)
            {
                Vector2 vel = away * Main.rand.NextFloat(1f, 2.5f) + Main.rand.NextVector2Circular(0.5f, 0.5f);
                float progress = Main.rand.NextFloat();
                Color col = ClairDeLunePalette.GetTemporalGradient(progress);
                Dust d = Dust.NewDustPerfect(pos + Main.rand.NextVector2Circular(4f, 4f),
                    DustID.Electric, vel, 0, col, 1.2f);
                d.noGravity = true;
                d.fadeIn = 1.0f;
            }

            // Pearl accent (1-in-3)
            if (Main.rand.NextBool(3))
            {
                Color pearlCol = ClairDeLunePalette.GetPearlGradient(Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(pos + Main.rand.NextVector2Circular(5f, 5f),
                    DustID.GemDiamond, away * 0.4f, 0, pearlCol, 0.7f);
                d.noGravity = true;
            }

            // Occasional music note (1-in-8)
            if (Main.rand.NextBool(8))
                ClairDeLuneVFXLibrary.SpawnMusicNotes(pos, 1, 6f, 0.7f, 0.85f, 22);

            Lighting.AddLight(pos, ClairDeLunePalette.TemporalCrimson.ToVector3() * 0.5f);
        }

        // =====================================================================
        //  PROJECTILE IMPACT VFX
        // =====================================================================

        /// <summary>
        /// Temporal impact VFX with pearl burst for time-blade projectile hits.
        /// Call when a time-blade projectile hits a target or expires.
        /// </summary>
        public static void ProjectileImpactVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            // Bloom flash
            ClairDeLuneVFXLibrary.DrawBloom(pos, 0.5f);

            // Temporal crimson burst
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                float progress = (float)i / 8f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 5f);
                Color col = ClairDeLunePalette.GetTemporalGradient(progress);
                Dust d = Dust.NewDustPerfect(pos, DustID.Electric, vel, 0, col, 1.3f);
                d.noGravity = true;
            }

            // Pearl burst
            ClairDeLuneVFXLibrary.SpawnPearlBurst(pos, 6, 4f, 0.25f);

            // Pearl shimmer
            ClairDeLuneVFXLibrary.SpawnPearlShimmer(pos, 3, 18f, 0.25f);

            // Halo rings
            ClairDeLuneVFXLibrary.SpawnGradientHaloRings(pos, 3, 0.25f);

            // Music notes
            ClairDeLuneVFXLibrary.SpawnMusicNotes(pos, 3, 20f, 0.7f, 1.0f, 28);

            Lighting.AddLight(pos, ClairDeLunePalette.TemporalCrimson.ToVector3() * 0.9f);
        }
    }
}
