using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.VFX.Bloom;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Content.ClairDeLune.Accessories
{
    // =========================================================================
    //  TEMPORAL WRATH GAUNTLET  (Melee Accessory) -- VFX
    //  Identity: Time-rending gauntlet of absolute fury.
    //  Melee attacks freeze time around targets, critical strikes unleash
    //  temporal shockwaves, and every 5th consecutive hit triggers a massive
    //  time fracture that shatters reality in a sphere of clockwork devastation.
    //  Temporal crimson fury married with pearl-white brilliance.
    // =========================================================================

    /// <summary>
    /// VFX helper for the Temporal Wrath Gauntlet melee accessory.
    /// Handles ambient temporal aura, per-hit freeze accents,
    /// critical shockwave bursts, and the every-5th-hit time fracture detonation.
    /// </summary>
    public static class TemporalWrathGauntletVFX
    {
        /// <summary>
        /// Ambient temporal fury aura around the player.
        /// Orbiting crimson sparks with clockwork brass accents and periodic music notes.
        /// </summary>
        public static void AmbientAura(Vector2 playerCenter, float timer)
        {
            if (Main.dedServ) return;

            // 3-point orbiting temporal crimson sparks
            for (int i = 0; i < 3; i++)
            {
                float angle = timer * 0.05f + MathHelper.TwoPi * i / 3f;
                float radius = 24f + MathF.Sin(timer * 0.06f + i * 1.2f) * 4f;
                Vector2 motePos = playerCenter + angle.ToRotationVector2() * radius;

                if (Main.rand.NextBool(4))
                {
                    Color col = ClairDeLunePalette.GetTemporalGradient(0.3f + (float)i / 3f * 0.4f);
                    Dust d = Dust.NewDustPerfect(motePos, DustID.Electric, Vector2.Zero, 0, col, 0.85f);
                    d.noGravity = true;
                    d.fadeIn = 0.5f;
                }
            }

            // Clockwork brass accent drift
            if (Main.rand.NextBool(6))
            {
                Vector2 pos = playerCenter + Main.rand.NextVector2Circular(20f, 20f);
                Vector2 vel = new Vector2(Main.rand.NextFloat(-0.2f, 0.2f), -0.3f - Main.rand.NextFloat(0.2f));
                Color col = ClairDeLunePalette.GetClockworkGradient(Main.rand.NextFloat(0.4f, 0.8f));
                Dust d = Dust.NewDustPerfect(pos, DustID.GemDiamond, vel, 0, col, 0.7f);
                d.noGravity = true;
            }

            // Ambient clockwork aura (delegates to library: mist + pearl + clockwork sparkle)
            ClairDeLuneVFXLibrary.AmbientClockworkAura(playerCenter, timer, 30f);

            // Rare music note
            if ((int)timer % 25 == 0)
                ClairDeLuneVFXLibrary.SpawnMusicNotes(playerCenter, 1, 20f, 0.7f, 0.9f, 30);

            float pulse = 0.35f + MathF.Sin(timer * 0.04f) * 0.1f;
            Lighting.AddLight(playerCenter, ClairDeLunePalette.TemporalCrimson.ToVector3() * pulse);
        }

        /// <summary>
        /// Per-hit temporal freeze accent at the target position.
        /// Crimson and frost dust with pearl sparkle shimmer.
        /// </summary>
        public static void HitVFX(Vector2 hitPos)
        {
            if (Main.dedServ) return;

            // Temporal crimson dust ring (4 particles)
            for (int i = 0; i < 4; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(3f, 3f);
                Color col = ClairDeLunePalette.GetTemporalGradient(Main.rand.NextFloat(0.3f, 0.8f));
                Dust d = Dust.NewDustPerfect(hitPos, DustID.Electric, vel, 0, col, 1.2f);
                d.noGravity = true;
            }

            // Frost freeze accent
            {
                Vector2 vel = Main.rand.NextVector2Circular(2f, 2f);
                Color col = ClairDeLunePalette.MoonlitFrost;
                Dust d = Dust.NewDustPerfect(hitPos, DustID.IceTorch, vel, 0, col, 1.0f);
                d.noGravity = true;
            }

            ClairDeLuneVFXLibrary.SpawnPearlSparkle(hitPos, Main.rand.NextVector2Unit());

            Lighting.AddLight(hitPos, ClairDeLunePalette.TemporalCrimson.ToVector3() * 0.5f);
        }

        /// <summary>
        /// Critical strike shockwave — 12-point radial crimson burst,
        /// pearl shimmer, screen shake, and music note scatter.
        /// </summary>
        public static void CritShockwaveVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            ClairDeLuneVFXLibrary.DrawBloom(pos, 0.6f);

            // 12-point radial crimson + brass burst
            for (int i = 0; i < 12; i++)
            {
                float angle = MathHelper.TwoPi * i / 12f;
                float progress = (float)i / 12f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(5f, 8f);
                Color col = i % 2 == 0
                    ? ClairDeLunePalette.GetTemporalGradient(progress)
                    : ClairDeLunePalette.GetClockworkGradient(progress);
                int dustType = i % 3 == 0 ? DustID.Electric : DustID.IceTorch;
                Dust d = Dust.NewDustPerfect(pos, dustType, vel, 0, col, 1.5f);
                d.noGravity = true;
            }

            ClairDeLuneVFXLibrary.SpawnPearlBurst(pos, 8, 5f, 0.3f);
            ClairDeLuneVFXLibrary.SpawnGradientHaloRings(pos, 4, 0.3f);
            ClairDeLuneVFXLibrary.SpawnMusicNotes(pos, 3, 25f, 0.75f, 1.0f, 28);

            MagnumScreenEffects.AddScreenShake(4f);

            Lighting.AddLight(pos, ClairDeLunePalette.WhiteHot.ToVector3() * 1.2f);
        }

        /// <summary>
        /// Every-5th-hit time fracture detonation — massive 20-point radial burst,
        /// lightning arcs, heavy bloom, converging mist, music note shower, and screen shake.
        /// The ultimate temporal devastation proc for the supreme melee gauntlet.
        /// </summary>
        public static void TimeFractureVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            ClairDeLuneVFXLibrary.DrawBloom(pos, 1.0f);

            // 20-point radial crimson + pearl mega-burst
            for (int i = 0; i < 20; i++)
            {
                float angle = MathHelper.TwoPi * i / 20f;
                float progress = (float)i / 20f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(7f, 12f);
                Color col = i % 2 == 0
                    ? ClairDeLunePalette.GetTemporalGradient(progress)
                    : ClairDeLunePalette.GetPearlGradient(progress);
                int dustType = i % 3 == 0 ? DustID.Electric : DustID.IceTorch;
                Dust d = Dust.NewDustPerfect(pos, dustType, vel, 0, col, 1.8f);
                d.noGravity = true;
            }

            // 6 lightning arcs radiating outward
            for (int i = 0; i < 6; i++)
            {
                float baseAngle = MathHelper.TwoPi * i / 6f;
                for (int j = 0; j < 4; j++)
                {
                    float jitter = Main.rand.NextFloat(-0.3f, 0.3f);
                    float dist = 10f + j * 12f;
                    Vector2 arcPos = pos + (baseAngle + jitter).ToRotationVector2() * dist;
                    Color col = ClairDeLunePalette.GetTemporalGradient(0.4f + (float)j / 8f);
                    Dust d = Dust.NewDustPerfect(arcPos, DustID.Electric,
                        Main.rand.NextVector2Circular(1f, 1f), 0, col, 1.1f);
                    d.noGravity = true;
                }
            }

            ClairDeLuneVFXLibrary.SpawnPearlBurst(pos, 14, 7f, 0.35f);
            ClairDeLuneVFXLibrary.SpawnGradientHaloRings(pos, 7, 0.4f);
            ClairDeLuneVFXLibrary.SpawnConvergingMist(pos, 8, 70f, 0.55f);
            ClairDeLuneVFXLibrary.SpawnStarlitSparkles(pos, 8, 45f, 0.25f);
            ClairDeLuneVFXLibrary.SpawnMusicNotes(pos, 8, 40f, 0.85f, 1.2f, 40);

            MagnumScreenEffects.AddScreenShake(8f);

            Lighting.AddLight(pos, ClairDeLunePalette.WhiteHot.ToVector3() * 2.0f);
        }
    }

    // =========================================================================
    //  CLOCKWORK TARGETING MODULE  (Ranged Accessory) -- VFX
    //  Identity: Precision clockwork optics locking onto targets.
    //  Ranged projectiles gain subtle homing, critical strikes fire temporal
    //  bolts, and headshots unleash devastating brass-gold precision flares.
    //  Clockwork brass precision married with starlight silver clarity.
    // =========================================================================

    /// <summary>
    /// VFX helper for the Clockwork Targeting Module ranged accessory.
    /// Handles ambient clockwork precision aura, per-hit gear cascade accents,
    /// critical temporal bolt firing flash, and headshot precision flare.
    /// </summary>
    public static class ClockworkTargetingModuleVFX
    {
        /// <summary>
        /// Ambient clockwork precision aura around the player.
        /// Brass gear shimmer with starlight accents and periodic music notes.
        /// </summary>
        public static void AmbientAura(Vector2 playerCenter, float timer)
        {
            if (Main.dedServ) return;

            // 2-point orbiting brass precision motes
            for (int i = 0; i < 2; i++)
            {
                float angle = timer * 0.04f + MathHelper.Pi * i;
                float radius = 22f + MathF.Sin(timer * 0.05f + i * 1.5f) * 3f;
                Vector2 motePos = playerCenter + angle.ToRotationVector2() * radius;

                if (Main.rand.NextBool(4))
                {
                    Color col = ClairDeLunePalette.GetClockworkGradient(0.4f + (float)i / 2f * 0.4f);
                    Dust d = Dust.NewDustPerfect(motePos, DustID.GemDiamond, Vector2.Zero, 0, col, 0.75f);
                    d.noGravity = true;
                    d.fadeIn = 0.5f;
                }
            }

            // Starlight silver sparkle accent
            if (Main.rand.NextBool(7))
            {
                Vector2 pos = playerCenter + Main.rand.NextVector2Circular(18f, 18f);
                Vector2 vel = new Vector2(Main.rand.NextFloat(-0.2f, 0.2f), -0.25f - Main.rand.NextFloat(0.2f));
                Color col = Color.Lerp(ClairDeLunePalette.StarlightSilver, ClairDeLunePalette.PearlWhite, Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(pos, DustID.GemDiamond, vel, 0, col, 0.65f);
                d.noGravity = true;
            }

            ClairDeLuneVFXLibrary.AmbientClockworkAura(playerCenter, timer, 28f);

            if ((int)timer % 28 == 0)
                ClairDeLuneVFXLibrary.SpawnMusicNotes(playerCenter, 1, 18f, 0.65f, 0.85f, 28);

            float pulse = 0.3f + MathF.Sin(timer * 0.045f) * 0.08f;
            Lighting.AddLight(playerCenter, ClairDeLunePalette.ClockworkBrass.ToVector3() * pulse);
        }

        /// <summary>
        /// Per-hit gear cascade accent at the target position.
        /// Small clockwork gear dust with brass-gold shimmer.
        /// </summary>
        public static void HitVFX(Vector2 hitPos)
        {
            if (Main.dedServ) return;

            // Clockwork gear dust (3 particles)
            for (int i = 0; i < 3; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(2.5f, 2.5f);
                Color col = ClairDeLunePalette.GetClockworkGradient(Main.rand.NextFloat(0.3f, 0.9f));
                Dust d = Dust.NewDustPerfect(hitPos, DustID.GemDiamond, vel, 0, col, 1.0f);
                d.noGravity = true;
            }

            // Starlight accent
            if (Main.rand.NextBool(2))
            {
                Color col = ClairDeLunePalette.StarlightSilver;
                Dust d = Dust.NewDustPerfect(hitPos, DustID.IceTorch,
                    Main.rand.NextVector2Circular(1.5f, 1.5f), 0, col, 0.9f);
                d.noGravity = true;
            }

            Lighting.AddLight(hitPos, ClairDeLunePalette.ClockworkBrass.ToVector3() * 0.4f);
        }

        /// <summary>
        /// Critical temporal bolt firing flash — 8-point radial brass burst,
        /// pearl shimmer, and music note accent at the crit location.
        /// </summary>
        public static void ProcVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            ClairDeLuneVFXLibrary.DrawBloom(pos, 0.5f);

            // 8-point radial brass + temporal burst
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                float progress = (float)i / 8f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 6f);
                Color col = i % 2 == 0
                    ? ClairDeLunePalette.GetClockworkGradient(progress)
                    : ClairDeLunePalette.GetTemporalGradient(progress);
                Dust d = Dust.NewDustPerfect(pos, DustID.GemDiamond, vel, 0, col, 1.3f);
                d.noGravity = true;
            }

            ClairDeLuneVFXLibrary.SpawnPearlShimmer(pos, 3, 20f, 0.25f);
            ClairDeLuneVFXLibrary.SpawnMusicNotes(pos, 2, 18f, 0.7f, 0.9f, 25);

            Lighting.AddLight(pos, ClairDeLunePalette.MoonbeamGold.ToVector3() * 0.8f);
        }

        /// <summary>
        /// Headshot precision flare — concentrated brass-gold bloom,
        /// starlit sparkle cascade, converging gear dust, and screen shake.
        /// The triumphant payoff for precision marksmanship.
        /// </summary>
        public static void HeadshotVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            ClairDeLuneVFXLibrary.DrawBloom(pos, 0.7f);

            // 10-point radial gold precision burst
            for (int i = 0; i < 10; i++)
            {
                float angle = MathHelper.TwoPi * i / 10f;
                float progress = (float)i / 10f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 7f);
                Color col = Color.Lerp(ClairDeLunePalette.ClockworkBrass, ClairDeLunePalette.MoonbeamGold, progress);
                Dust d = Dust.NewDustPerfect(pos, DustID.GemDiamond, vel, 0, col, 1.5f);
                d.noGravity = true;
            }

            ClairDeLuneVFXLibrary.SpawnPearlBurst(pos, 6, 4f, 0.28f);
            ClairDeLuneVFXLibrary.SpawnStarlitSparkles(pos, 5, 25f, 0.22f);
            ClairDeLuneVFXLibrary.SpawnGradientHaloRings(pos, 3, 0.28f);
            ClairDeLuneVFXLibrary.SpawnMusicNotes(pos, 3, 22f, 0.75f, 1.0f, 30);

            MagnumScreenEffects.AddScreenShake(3f);

            Lighting.AddLight(pos, ClairDeLunePalette.MoonbeamGold.ToVector3() * 1.2f);
        }
    }

    // =========================================================================
    //  RESONANT CHRONOSPHERE  (Magic Accessory) -- VFX
    //  Identity: A sphere that suspends all moments in perfect stillness.
    //  Magic attacks create temporal echoes, low mana triggers the Temporal
    //  Clarity buff with crystalline dream haze, and every spell leaves
    //  shimmering pearl residue. Dream haze impressionism meets arcane power.
    // =========================================================================

    /// <summary>
    /// VFX helper for the Resonant Chronosphere magic accessory.
    /// Handles ambient dreamy aura, per-hit crystal shatter accents,
    /// temporal echo proc VFX, and Temporal Clarity buff visuals.
    /// </summary>
    public static class ResonantChronosphereVFX
    {
        /// <summary>
        /// Ambient dreamy chronosphere aura around the player.
        /// Dream haze mist with pearl shimmer and periodic music notes.
        /// </summary>
        public static void AmbientAura(Vector2 playerCenter, float timer)
        {
            if (Main.dedServ) return;

            // 2-point orbiting dream haze motes
            for (int i = 0; i < 2; i++)
            {
                float angle = timer * 0.035f + MathHelper.Pi * i;
                float radius = 20f + MathF.Sin(timer * 0.05f + i * 1.0f) * 4f;
                Vector2 motePos = playerCenter + angle.ToRotationVector2() * radius;

                if (Main.rand.NextBool(3))
                {
                    Color col = ClairDeLunePalette.GetMoonlitGradient(0.3f + (float)i / 2f * 0.4f);
                    Dust d = Dust.NewDustPerfect(motePos, DustID.IceTorch, Vector2.Zero, 0, col, 0.8f);
                    d.noGravity = true;
                    d.fadeIn = 0.6f;
                }
            }

            // Dream haze particle drift
            if (Main.rand.NextBool(5))
            {
                Vector2 pos = playerCenter + Main.rand.NextVector2Circular(22f, 22f);
                Vector2 vel = new Vector2(Main.rand.NextFloat(-0.3f, 0.3f), -0.4f - Main.rand.NextFloat(0.3f));
                Color col = ClairDeLunePalette.DreamHaze;
                Dust d = Dust.NewDustPerfect(pos, DustID.IceTorch, vel, 0, col, 0.85f);
                d.noGravity = true;
            }

            ClairDeLuneVFXLibrary.AmbientDreamyAura(playerCenter, timer, 32f);

            if ((int)timer % 26 == 0)
                ClairDeLuneVFXLibrary.SpawnMusicNotes(playerCenter, 1, 18f, 0.7f, 0.9f, 30);

            float pulse = 0.3f + MathF.Sin(timer * 0.04f) * 0.1f;
            Lighting.AddLight(playerCenter, ClairDeLunePalette.DreamHaze.ToVector3() * pulse);
        }

        /// <summary>
        /// Per-hit crystal shatter accent at the target position.
        /// Frost and pearl dust with dream haze undertone.
        /// </summary>
        public static void HitVFX(Vector2 hitPos)
        {
            if (Main.dedServ) return;

            // Crystal shatter dust (4 particles)
            for (int i = 0; i < 4; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(3f, 3f);
                Color col = ClairDeLunePalette.GetPearlGradient(Main.rand.NextFloat());
                int dustType = i % 2 == 0 ? DustID.IceTorch : DustID.GemDiamond;
                Dust d = Dust.NewDustPerfect(hitPos, dustType, vel, 0, col, 1.1f);
                d.noGravity = true;
            }

            // Dream haze accent
            {
                Color col = ClairDeLunePalette.DreamHaze;
                Dust d = Dust.NewDustPerfect(hitPos, DustID.IceTorch,
                    Main.rand.NextVector2Circular(1.5f, 1.5f), 0, col, 0.9f);
                d.noGravity = true;
            }

            Lighting.AddLight(hitPos, ClairDeLunePalette.PearlBlue.ToVector3() * 0.45f);
        }

        /// <summary>
        /// Temporal echo proc — the ghost of a spell repeats itself.
        /// 10-point radial pearl-dream burst, converging mist, music notes, bloom flash.
        /// Fires when the echo projectile is created.
        /// </summary>
        public static void ProcVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            ClairDeLuneVFXLibrary.DrawBloom(pos, 0.6f);

            // 10-point radial pearl + dream haze burst
            for (int i = 0; i < 10; i++)
            {
                float angle = MathHelper.TwoPi * i / 10f;
                float progress = (float)i / 10f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 7f);
                Color col = i % 2 == 0
                    ? ClairDeLunePalette.GetPearlGradient(progress)
                    : ClairDeLunePalette.GetMoonlitGradient(progress);
                Dust d = Dust.NewDustPerfect(pos, DustID.IceTorch, vel, 0, col, 1.4f);
                d.noGravity = true;
            }

            ClairDeLuneVFXLibrary.SpawnPearlBurst(pos, 6, 4f, 0.28f);
            ClairDeLuneVFXLibrary.SpawnConvergingMist(pos, 4, 40f, 0.45f);
            ClairDeLuneVFXLibrary.SpawnMusicNotes(pos, 3, 20f, 0.75f, 1.0f, 28);

            Lighting.AddLight(pos, ClairDeLunePalette.PearlBlue.ToVector3() * 0.9f);
        }

        /// <summary>
        /// Temporal Clarity buff ambient VFX — active when mana drops below 20%.
        /// Intensified dream haze aura with rising crystal dust and pulsing pearl ring.
        /// </summary>
        public static void TemporalClarityVFX(Vector2 playerCenter, float timer)
        {
            if (Main.dedServ) return;

            // Rising crystal dust every 3 frames
            if ((int)timer % 3 == 0)
            {
                Vector2 vel = new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), -0.8f - Main.rand.NextFloat(0.5f));
                Color col = ClairDeLunePalette.GetPearlGradient(Main.rand.NextFloat(0.4f, 1f));
                Dust d = Dust.NewDustPerfect(
                    playerCenter + Main.rand.NextVector2Circular(20f, 20f),
                    DustID.GemDiamond, vel, 0, col, 1.1f);
                d.noGravity = true;
            }

            // Dream haze intensified motes every 5 frames
            if ((int)timer % 5 == 0)
            {
                Vector2 vel = new Vector2(Main.rand.NextFloat(-0.6f, 0.6f), -0.5f - Main.rand.NextFloat(0.4f));
                Color col = ClairDeLunePalette.DreamHaze;
                Dust d = Dust.NewDustPerfect(
                    playerCenter + Main.rand.NextVector2Circular(25f, 25f),
                    DustID.IceTorch, vel, 0, col, 1.0f);
                d.noGravity = true;
            }

            // Pearl shimmer every 8 frames
            if ((int)timer % 8 == 0)
                ClairDeLuneVFXLibrary.SpawnPearlShimmer(playerCenter, 1, 22f, 0.22f);

            // Music notes every 15 frames
            if ((int)timer % 15 == 0)
                ClairDeLuneVFXLibrary.SpawnMusicNotes(playerCenter, 1, 15f, 0.7f, 0.9f, 25);

            float pulse = 0.5f + MathF.Sin(timer * 0.08f) * 0.15f;
            Lighting.AddLight(playerCenter, ClairDeLunePalette.PearlWhite.ToVector3() * pulse);
        }
    }

    // =========================================================================
    //  CONDUCTOR'S TEMPORAL BATON  (Summon Accessory) -- VFX
    //  Identity: The conductor commands the orchestra of time itself.
    //  Minion attacks spawn temporal clones, every 10th hit triggers a
    //  devastating temporal storm burst with lightning arcs, and minions
    //  shimmer with moonlit silver energy. Pearl white elegance meets
    //  clockwork temporal devastation.
    // =========================================================================

    /// <summary>
    /// VFX helper for the Conductor's Temporal Baton summon accessory.
    /// Handles ambient moonlit conductor aura, per-hit temporal trail accents,
    /// temporal clone proc flash, and the every-10th-hit storm burst detonation.
    /// </summary>
    public static class ConductorsTemporalBatonVFX
    {
        /// <summary>
        /// Ambient moonlit conductor aura around the player.
        /// Pearl shimmer orbits with starlight silver accents and periodic music notes.
        /// </summary>
        public static void AmbientAura(Vector2 playerCenter, float timer)
        {
            if (Main.dedServ) return;

            // 3-point orbiting pearl-silver conductor motes
            for (int i = 0; i < 3; i++)
            {
                float angle = timer * 0.04f + MathHelper.TwoPi * i / 3f;
                float radius = 26f + MathF.Sin(timer * 0.05f + i * 1.3f) * 5f;
                Vector2 motePos = playerCenter + angle.ToRotationVector2() * radius;

                if (Main.rand.NextBool(4))
                {
                    Color col = Color.Lerp(ClairDeLunePalette.StarlightSilver, ClairDeLunePalette.PearlWhite, (float)i / 3f);
                    Dust d = Dust.NewDustPerfect(motePos, DustID.GemDiamond, Vector2.Zero, 0, col, 0.8f);
                    d.noGravity = true;
                    d.fadeIn = 0.5f;
                }
            }

            // Moonlit mist drift accent
            if (Main.rand.NextBool(6))
            {
                Vector2 pos = playerCenter + Main.rand.NextVector2Circular(24f, 24f);
                Vector2 vel = new Vector2(Main.rand.NextFloat(-0.3f, 0.3f), -0.3f - Main.rand.NextFloat(0.2f));
                Color col = ClairDeLunePalette.GetMoonlitGradient(Main.rand.NextFloat(0.4f, 0.9f));
                Dust d = Dust.NewDustPerfect(pos, DustID.IceTorch, vel, 0, col, 0.75f);
                d.noGravity = true;
            }

            ClairDeLuneVFXLibrary.AmbientDreamyAura(playerCenter, timer, 34f);

            if ((int)timer % 25 == 0)
                ClairDeLuneVFXLibrary.SpawnMusicNotes(playerCenter, 1, 22f, 0.7f, 0.9f, 32);

            float pulse = 0.3f + MathF.Sin(timer * 0.04f) * 0.08f;
            Lighting.AddLight(playerCenter, ClairDeLunePalette.StarlightSilver.ToVector3() * pulse);
        }

        /// <summary>
        /// Per-hit temporal trail accent at the target position.
        /// Silver-pearl dust with clockwork brass shimmer.
        /// </summary>
        public static void HitVFX(Vector2 hitPos)
        {
            if (Main.dedServ) return;

            // Moonlit trail dust (3 particles)
            for (int i = 0; i < 3; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(2.5f, 2.5f);
                Color col = ClairDeLunePalette.GetMoonlitGradient(Main.rand.NextFloat(0.3f, 0.9f));
                Dust d = Dust.NewDustPerfect(hitPos, DustID.IceTorch, vel, 0, col, 1.0f);
                d.noGravity = true;
            }

            // Clockwork brass accent
            if (Main.rand.NextBool(2))
            {
                Color col = ClairDeLunePalette.GetClockworkGradient(Main.rand.NextFloat(0.4f, 0.8f));
                Dust d = Dust.NewDustPerfect(hitPos, DustID.GemDiamond,
                    Main.rand.NextVector2Circular(1.5f, 1.5f), 0, col, 0.85f);
                d.noGravity = true;
            }

            Lighting.AddLight(hitPos, ClairDeLunePalette.StarlightSilver.ToVector3() * 0.35f);
        }

        /// <summary>
        /// Temporal clone proc flash — a ghost of the minion strikes again.
        /// Small pearl burst with diamond dust scatter and music note.
        /// Fires on the 20% chance temporal clone trigger.
        /// </summary>
        public static void ProcVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            // 6-point radial pearl flash
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f;
                float progress = (float)i / 6f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 5f);
                Color col = ClairDeLunePalette.GetPearlGradient(progress);
                Dust d = Dust.NewDustPerfect(pos, DustID.GemDiamond, vel, 0, col, 1.2f);
                d.noGravity = true;
            }

            ClairDeLuneVFXLibrary.SpawnPearlShimmer(pos, 2, 18f, 0.22f);
            ClairDeLuneVFXLibrary.SpawnMusicNotes(pos, 1, 12f, 0.7f, 0.9f, 22);

            Lighting.AddLight(pos, ClairDeLunePalette.PearlWhite.ToVector3() * 0.6f);
        }

        /// <summary>
        /// Every-10th-hit temporal storm burst — massive clockwork-pearl detonation.
        /// 16-point radial burst, lightning arc dust, heavy bloom, converging mist,
        /// starlit cascade, music note shower, and screen shake.
        /// The conductor's ultimate orchestral climax.
        /// </summary>
        public static void TemporalStormBurstVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            ClairDeLuneVFXLibrary.DrawBloom(pos, 0.9f);

            // 16-point radial silver + brass storm burst
            for (int i = 0; i < 16; i++)
            {
                float angle = MathHelper.TwoPi * i / 16f;
                float progress = (float)i / 16f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(6f, 10f);
                Color col = i % 2 == 0
                    ? ClairDeLunePalette.GetPearlGradient(progress)
                    : ClairDeLunePalette.GetClockworkGradient(progress);
                int dustType = i % 3 == 0 ? DustID.Electric : DustID.GemDiamond;
                Dust d = Dust.NewDustPerfect(pos, dustType, vel, 0, col, 1.6f);
                d.noGravity = true;
            }

            // 8 lightning arcs for the temporal storm
            for (int i = 0; i < 8; i++)
            {
                float baseAngle = MathHelper.TwoPi * i / 8f;
                for (int j = 0; j < 3; j++)
                {
                    float jitter = Main.rand.NextFloat(-0.35f, 0.35f);
                    float dist = 8f + j * 10f;
                    Vector2 arcPos = pos + (baseAngle + jitter).ToRotationVector2() * dist;
                    Color col = ClairDeLunePalette.GetTemporalGradient(0.3f + (float)j / 6f);
                    Dust d = Dust.NewDustPerfect(arcPos, DustID.Electric,
                        Main.rand.NextVector2Circular(0.8f, 0.8f), 0, col, 1.0f);
                    d.noGravity = true;
                }
            }

            ClairDeLuneVFXLibrary.SpawnPearlBurst(pos, 12, 6f, 0.32f);
            ClairDeLuneVFXLibrary.SpawnGradientHaloRings(pos, 6, 0.35f);
            ClairDeLuneVFXLibrary.SpawnConvergingMist(pos, 6, 60f, 0.5f);
            ClairDeLuneVFXLibrary.SpawnStarlitSparkles(pos, 8, 40f, 0.25f);
            ClairDeLuneVFXLibrary.SpawnMoonlitMist(pos, 5, 50f, 0.5f);
            ClairDeLuneVFXLibrary.SpawnMusicNotes(pos, 6, 35f, 0.8f, 1.2f, 38);

            MagnumScreenEffects.AddScreenShake(6f);

            Lighting.AddLight(pos, ClairDeLunePalette.WhiteHot.ToVector3() * 1.6f);
        }
    }
}
