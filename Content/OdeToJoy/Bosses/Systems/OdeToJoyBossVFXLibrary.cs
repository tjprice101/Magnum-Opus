using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.VFX;

namespace MagnumOpus.Content.OdeToJoy.Bosses.Systems
{
    /// <summary>
    /// ODE TO JOY — CHROMATIC ROSE CONDUCTOR VFX LIBRARY
    /// 
    /// The triumph of universal joy, brotherhood through music — Beethoven's Ninth made visual.
    /// 
    /// Phase 1 — THE SOLO VOICE: Single instrument motifs. Golden trumpet cones, amber violin arcs,
    ///     cymbal-crash rings. Music notes cascade from every strike. Warm candlelight gold arena.
    /// 
    /// Phase 2 — THE CHORUS JOINS: Multiple instrument-trails layer simultaneously.
    ///     Rose petals scatter with each downbeat. Arena brightens as more lights are lit.
    ///     Harmonic resonance rings pulse outward rhythmically.
    /// 
    /// Phase 3 — FULL ORCHESTRA: Arena alive with overlapping visual music.
    ///     Attacks are walls of golden particle waves, fan-shaped chromatic cascades.
    ///     Rose petals form spiraling DNA-helix patterns around the boss.
    /// 
    /// Enrage — FINAL CHORUS (Freude, schöner Götterfunken!): Screen overflows with jubilant golden light.
    ///     Firework-burst particles, confetti-like note cascades. Even being hit feels magnificent.
    /// 
    /// Palette: warm gold, radiant amber, rose pink, jubilant white, celebratory light.
    /// </summary>
    public static class OdeToJoyBossVFXLibrary
    {
        #region Theme Colors — Jubilant Palette

        private static readonly Color WarmGold = new Color(255, 210, 60);
        private static readonly Color RadiantAmber = new Color(255, 170, 40);
        private static readonly Color CandlelightGold = new Color(255, 230, 140);
        private static readonly Color RosePink = new Color(230, 120, 150);
        private static readonly Color JubilantWhite = new Color(255, 250, 235);
        private static readonly Color CelebratoryLight = new Color(255, 240, 200);
        private static readonly Color TrumpetBrass = new Color(255, 190, 50);
        private static readonly Color ViolinAmber = new Color(220, 160, 60);
        private static readonly Color CymbalFlash = new Color(255, 255, 220);
        private static readonly Color ChorusRose = new Color(240, 150, 170);
        private static readonly Color FireworkGold = new Color(255, 220, 100);
        private static readonly Color ConfettiPink = new Color(255, 180, 200);
        private static readonly Color ConfettiGold = new Color(255, 230, 80);

        #endregion

        #region Phase 1 — Solo Voice: Single Instrument Motifs

        /// <summary>
        /// Trumpet-blast cone: A directional fan of golden brass particles flaring outward
        /// like the bell of a trumpet. Single, powerful brass note.
        /// </summary>
        public static void TrumpetBlastCone(Vector2 origin, Vector2 direction, float intensity = 1f)
        {
            float baseAngle = direction.ToRotation();
            float coneSpread = MathHelper.ToRadians(35f);

            // Bright brass core flash at the trumpet bell
            CustomParticles.GenericFlare(origin, JubilantWhite, 1.2f * intensity, 18);
            CustomParticles.GenericFlare(origin, TrumpetBrass, 0.9f * intensity, 22);

            // Cone of golden particles fanning outward
            int particleCount = (int)(12 * intensity);
            for (int i = 0; i < particleCount; i++)
            {
                float angle = baseAngle + Main.rand.NextFloat(-coneSpread, coneSpread);
                float speed = Main.rand.NextFloat(6f, 14f) * intensity;
                Vector2 vel = angle.ToRotationVector2() * speed;

                Color brassColor = Color.Lerp(TrumpetBrass, WarmGold, Main.rand.NextFloat());
                var particle = new GlowSparkParticle(origin, vel, brassColor with { A = 0 }, 0.4f * intensity, 20);
                MagnumParticleHandler.SpawnParticle(particle);
            }

            // Single expanding brass ring — the resonance of the note
            CustomParticles.HaloRing(origin, TrumpetBrass with { A = 0 }, 0.5f * intensity, 22);

            // Music note cascade from the blast
            SpawnMusicNoteCascade(origin, direction * 3f, WarmGold, 3, 30f);

            Lighting.AddLight(origin, TrumpetBrass.ToVector3() * 1.2f * intensity);
        }

        /// <summary>
        /// Violin-bow arc trail: Warm amber sweeping arc particles that linger like a drawn bow.
        /// Smooth, curved trail of amber light with gentle sparkle motes.
        /// </summary>
        public static void ViolinBowArc(Vector2 start, Vector2 end, float intensity = 1f)
        {
            Vector2 direction = end - start;
            float length = direction.Length();
            if (length < 1f) return;
            direction /= length;

            Vector2 perpendicular = new Vector2(-direction.Y, direction.X);
            int segments = Math.Max(6, (int)(length / 20f));

            for (int i = 0; i < segments; i++)
            {
                float t = (float)i / segments;
                // Slight curve — violin bow arcs gently
                float arcOffset = MathF.Sin(t * MathHelper.Pi) * 25f;
                Vector2 pos = Vector2.Lerp(start, end, t) + perpendicular * arcOffset;
                Vector2 vel = perpendicular * Main.rand.NextFloat(-0.5f, 0.5f) + direction * 0.5f;

                Color amberColor = Color.Lerp(ViolinAmber, CandlelightGold, t) with { A = 0 };
                float scale = 0.3f * intensity * (1f - MathF.Abs(t - 0.5f) * 1.2f); // Thickest in middle
                var glow = new GenericGlowParticle(pos, vel, amberColor, scale, 25, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }

            // Sparkle motes along the arc
            for (int i = 0; i < (int)(4 * intensity); i++)
            {
                float t = Main.rand.NextFloat();
                float arcOffset = MathF.Sin(t * MathHelper.Pi) * 25f;
                Vector2 pos = Vector2.Lerp(start, end, t) + perpendicular * arcOffset;
                Vector2 vel = Main.rand.NextVector2Circular(1.5f, 1.5f);

                var sparkle = new SparkleParticle(pos, vel, CandlelightGold with { A = 0 }, 0.2f * intensity, 20);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }

            // Single music note at the apex
            float midArc = MathF.Sin(0.5f * MathHelper.Pi) * 25f;
            Vector2 apex = Vector2.Lerp(start, end, 0.5f) + perpendicular * midArc;
            SpawnMusicNoteCascade(apex, Vector2.UnitY * -1f, ViolinAmber, 1, 15f);
        }

        /// <summary>
        /// Cymbal-crash impact ring: Brilliant white-gold expanding ring with sharp sparkle particles
        /// radiating outward from impact. The percussive accent of the orchestra.
        /// </summary>
        public static void CymbalCrashImpact(Vector2 center, float intensity = 1f)
        {
            // Blinding white core flash
            CustomParticles.GenericFlare(center, Color.White, 1.5f * intensity, 15);
            CustomParticles.GenericFlare(center, CymbalFlash, 1.0f * intensity, 18);
            CustomParticles.GenericFlare(center, WarmGold, 0.7f * intensity, 20);

            // Double expanding flash rings — cymbal resonance
            CustomParticles.HaloRing(center, CymbalFlash with { A = 0 }, 0.6f * intensity, 18);
            CustomParticles.HaloRing(center, WarmGold with { A = 0 }, 0.4f * intensity, 22);

            // Sharp radiating spark particles
            int sparkCount = (int)(10 * intensity);
            for (int i = 0; i < sparkCount; i++)
            {
                float angle = MathHelper.TwoPi * i / sparkCount + Main.rand.NextFloat(-0.2f, 0.2f);
                float speed = Main.rand.NextFloat(5f, 12f);
                Vector2 vel = angle.ToRotationVector2() * speed;

                Color sparkColor = Color.Lerp(CymbalFlash, WarmGold, Main.rand.NextFloat()) with { A = 0 };
                var spark = new GlowSparkParticle(center, vel, sparkColor, 0.35f * intensity, 15);
                MagnumParticleHandler.SpawnParticle(spark);
            }

            // Music note burst radiating from crash
            SpawnMusicNoteCascade(center, Vector2.Zero, WarmGold, 2, 25f);

            Lighting.AddLight(center, CymbalFlash.ToVector3() * 1.5f * intensity);
        }

        /// <summary>
        /// Solo conductor gesture: A graceful sweep from the boss, trailing golden light.
        /// Used during Phase 1 idle/telegraph to establish the conductor identity.
        /// </summary>
        public static void SoloConductorGesture(Vector2 center, float sweepAngle, float intensity = 1f)
        {
            float radius = 70f;
            int trailPoints = 8;

            for (int i = 0; i < trailPoints; i++)
            {
                float t = (float)i / trailPoints;
                float angle = sweepAngle - MathHelper.ToRadians(60f) * t;
                Vector2 pos = center + angle.ToRotationVector2() * radius;
                Vector2 vel = angle.ToRotationVector2() * 0.5f;

                Color color = Color.Lerp(WarmGold, CandlelightGold, t) with { A = 0 };
                float scale = 0.25f * intensity * (1f - t * 0.6f);
                var glow = new GenericGlowParticle(pos, vel, color, scale, 18, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }
        }

        #endregion

        #region Phase 2 — Chorus Joins: Ensemble Compositions

        /// <summary>
        /// Ensemble attack burst: Multiple instrument motifs layered simultaneously.
        /// Trumpet + violin + cymbal overlapping in a single glorious strike.
        /// </summary>
        public static void EnsembleStrike(Vector2 center, Vector2 direction, float intensity = 1f)
        {
            // Trumpet component — directional golden blast
            TrumpetBlastCone(center, direction, intensity * 0.8f);

            // Violin component — perpendicular amber arc
            Vector2 perp = new Vector2(-direction.Y, direction.X);
            if (perp.Length() > 0.01f) perp.Normalize();
            ViolinBowArc(center - perp * 60f, center + perp * 60f, intensity * 0.7f);

            // Cymbal component — impact ring at strike point
            Vector2 impactPoint = center + direction * 40f;
            CymbalCrashImpact(impactPoint, intensity * 0.6f);

            // Layered golden halos — the ensemble's combined resonance
            for (int ring = 0; ring < 3; ring++)
            {
                Color ringColor = Color.Lerp(WarmGold, ChorusRose, ring / 3f) with { A = 0 };
                CustomParticles.HaloRing(center, ringColor, (0.3f + ring * 0.15f) * intensity, 20 + ring * 4);
            }
        }

        /// <summary>
        /// Rose petal downbeat scatter: Rose petals burst outward on each musical downbeat.
        /// The conductor's signature — beauty intertwined with the music.
        /// </summary>
        public static void RosePetalDownbeat(Vector2 center, int count, float speed)
        {
            for (int i = 0; i < count; i++)
            {
                float angle = MathHelper.TwoPi * i / count + Main.rand.NextFloat(-0.15f, 0.15f);
                float petalSpeed = speed * Main.rand.NextFloat(0.7f, 1.3f);
                Vector2 vel = angle.ToRotationVector2() * petalSpeed;
                // Gentle gravity drift
                vel.Y += Main.rand.NextFloat(0.3f, 0.8f);

                Color petalColor = Color.Lerp(RosePink, ChorusRose, Main.rand.NextFloat()) with { A = 0 };
                RoseBudParticle.SpawnBurst(center + Main.rand.NextVector2Circular(15f, 15f), 1, petalSpeed * 0.5f, petalColor, WarmGold, 0.35f, 35);
            }
        }

        /// <summary>
        /// Harmonic resonance ring: A rhythmic pulse ring expanding outward.
        /// Pulses at the beat — visual representation of harmonic resonance.
        /// </summary>
        public static void HarmonicResonancePulse(Vector2 center, float scale, float intensity = 1f)
        {
            // Primary golden ring
            CustomParticles.HaloRing(center, WarmGold with { A = 0 }, scale * intensity, 25);
            // Secondary softer rose ring slightly delayed
            CustomParticles.HaloRing(center, ChorusRose with { A = 0 }, scale * 0.7f * intensity, 30);
            // Inner bright core pulse
            CustomParticles.GenericFlare(center, CandlelightGold with { A = 0 }, scale * 0.4f * intensity, 15);

            // Sparkle motes along the ring perimeter
            int moteCount = (int)(6 * scale * intensity);
            for (int i = 0; i < moteCount; i++)
            {
                float angle = MathHelper.TwoPi * i / moteCount;
                Vector2 pos = center + angle.ToRotationVector2() * (scale * 50f);
                Vector2 vel = angle.ToRotationVector2() * 1.5f;
                var sparkle = new SparkleParticle(pos, vel, CandlelightGold with { A = 0 }, 0.15f * intensity, 18);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
        }

        /// <summary>
        /// Chorus brightening: Arena glow intensification effect as more voices join.
        /// Spawns warm golden light sources spreading across the visible area.
        /// </summary>
        public static void ChorusBrighteningPulse(Vector2 bossCenter, float chorusIntensity)
        {
            // Multiple warm light points around the arena, like candles being lit
            int lightCount = (int)(4 + chorusIntensity * 6);
            for (int i = 0; i < lightCount; i++)
            {
                Vector2 lightPos = bossCenter + Main.rand.NextVector2Circular(400f, 300f);
                var glow = new GenericGlowParticle(lightPos, Vector2.UnitY * -0.3f, CandlelightGold with { A = 0 },
                    0.25f + chorusIntensity * 0.15f, 40, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }
        }

        #endregion

        #region Phase 3 — Full Orchestra: Arena Alive With Music

        /// <summary>
        /// Chromatic cascade fan: A sweeping fan-shaped wave of chromatic particles.
        /// The conductor's grand sweeping gesture launching a wall of golden light.
        /// </summary>
        public static void ChromaticCascadeFan(Vector2 origin, float baseAngle, float arcWidth, float intensity = 1f)
        {
            int waveCount = (int)(20 * intensity);
            float halfArc = arcWidth / 2f;

            for (int i = 0; i < waveCount; i++)
            {
                float t = (float)i / waveCount;
                float angle = baseAngle - halfArc + arcWidth * t;
                float speed = Main.rand.NextFloat(8f, 16f) * intensity;
                Vector2 vel = angle.ToRotationVector2() * speed;

                // Chromatic color shift across the fan
                Color cascadeColor = Color.Lerp(WarmGold, ChorusRose, t);
                cascadeColor = Color.Lerp(cascadeColor, CandlelightGold, Main.rand.NextFloat(0.2f)) with { A = 0 };

                var particle = new GlowSparkParticle(origin, vel, cascadeColor, 0.5f * intensity, 25);
                MagnumParticleHandler.SpawnParticle(particle);
            }

            // Expanding halo rings at origin
            for (int ring = 0; ring < 4; ring++)
            {
                float progress = ring / 4f;
                Color ringColor = Color.Lerp(WarmGold, ChorusRose, progress) with { A = 0 };
                CustomParticles.HaloRing(origin, ringColor, (0.4f + ring * 0.15f) * intensity, 18 + ring * 4);
            }

            // Dense music note shower across the fan
            SpawnMusicNoteCascade(origin, (baseAngle.ToRotationVector2()) * 4f, WarmGold, (int)(5 * intensity), 50f);

            // Core flash
            CustomParticles.GenericFlare(origin, JubilantWhite, 1.5f * intensity, 18);
            CustomParticles.GenericFlare(origin, WarmGold, 1.0f * intensity, 22);
        }

        /// <summary>
        /// Golden particle wave: A sweeping wall of golden light particles.
        /// Full orchestra at maximum intensity — every surface shimmers.
        /// </summary>
        public static void GoldenParticleWave(Vector2 center, Vector2 direction, float width, float intensity = 1f)
        {
            Vector2 perp = new Vector2(-direction.Y, direction.X);
            if (perp.Length() > 0.01f) perp.Normalize();
            if (direction.Length() > 0.01f) direction.Normalize();

            int particleCount = (int)(24 * intensity);
            for (int i = 0; i < particleCount; i++)
            {
                float spread = Main.rand.NextFloat(-width / 2f, width / 2f);
                Vector2 pos = center + perp * spread;
                float speed = Main.rand.NextFloat(6f, 14f);
                Vector2 vel = direction * speed + Main.rand.NextVector2Circular(1f, 1f);

                Color waveColor = Color.Lerp(WarmGold, CandlelightGold, Main.rand.NextFloat()) with { A = 0 };
                var spark = new GlowSparkParticle(pos, vel, waveColor, 0.4f * intensity, 22);
                MagnumParticleHandler.SpawnParticle(spark);
            }

            Lighting.AddLight(center, WarmGold.ToVector3() * intensity);
        }

        /// <summary>
        /// Rose petal DNA helix: Rose petals spiraling in a double-helix pattern around the boss.
        /// The conductor's signature elevated to its ultimate form.
        /// </summary>
        public static void RosePetalHelixOrbit(Vector2 bossCenter, float time, float radius, float intensity = 1f)
        {
            int helixPoints = 12;
            for (int i = 0; i < helixPoints; i++)
            {
                float t = (float)i / helixPoints;
                float angle = time * 2f + t * MathHelper.TwoPi * 2f; // 2 full rotations

                // Helix strand 1
                float yOffset = (t - 0.5f) * 200f;
                Vector2 pos1 = bossCenter + new Vector2(MathF.Cos(angle) * radius, yOffset + MathF.Sin(angle) * 15f);
                // Helix strand 2 (180° offset)
                Vector2 pos2 = bossCenter + new Vector2(MathF.Cos(angle + MathHelper.Pi) * radius, yOffset + MathF.Sin(angle + MathHelper.Pi) * 15f);

                Color petalColor = Color.Lerp(RosePink, ChorusRose, t) with { A = 0 };
                float scale = 0.25f * intensity * (1f - MathF.Abs(t - 0.5f));

                var petal1 = new GenericGlowParticle(pos1, Vector2.Zero, petalColor, scale, 8, true);
                var petal2 = new GenericGlowParticle(pos2, Vector2.Zero, petalColor * 0.8f, scale * 0.85f, 8, true);
                MagnumParticleHandler.SpawnParticle(petal1);
                MagnumParticleHandler.SpawnParticle(petal2);

                // Rose bud at helix nodes
                if (i % 4 == 0)
                {
                    RoseBudParticle.SpawnBurst(pos1, 1, 0.5f, RosePink, WarmGold, 0.2f * intensity, 12);
                }
            }
        }

        /// <summary>
        /// Full orchestra shimmer: Every surface in the arena shimmers with golden light.
        /// Ambient particles across the visible screen area.
        /// </summary>
        public static void FullOrchestraShimmer(Vector2 bossCenter, float intensity = 1f)
        {
            int shimmerCount = (int)(8 * intensity);
            for (int i = 0; i < shimmerCount; i++)
            {
                Vector2 pos = bossCenter + Main.rand.NextVector2Circular(500f, 400f);
                Vector2 vel = new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), Main.rand.NextFloat(-0.8f, 0.2f));

                Color shimmerColor = Main.rand.NextBool(3)
                    ? Color.Lerp(WarmGold, CandlelightGold, Main.rand.NextFloat())
                    : Color.Lerp(RosePink, JubilantWhite, Main.rand.NextFloat());
                shimmerColor.A = 0;

                var sparkle = new SparkleParticle(pos, vel, shimmerColor, 0.15f + Main.rand.NextFloat(0.1f), 30);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
        }

        #endregion

        #region Enrage — Final Chorus: Freude, schöner Götterfunken!

        /// <summary>
        /// Firework burst: Jubilant golden firework explosion. Celebration, not aggression.
        /// Each attack in enrage is a firework — magnificent rather than threatening.
        /// </summary>
        public static void FireworkBurst(Vector2 center, float intensity = 1f)
        {
            // Brilliant white-gold core
            CustomParticles.GenericFlare(center, Color.White, 2f * intensity, 15);
            CustomParticles.GenericFlare(center, FireworkGold, 1.5f * intensity, 20);
            CustomParticles.GenericFlare(center, WarmGold, 1.0f * intensity, 24);

            // Firework trails radiating outward — each a golden streak
            int trailCount = (int)(16 * intensity);
            for (int i = 0; i < trailCount; i++)
            {
                float angle = MathHelper.TwoPi * i / trailCount + Main.rand.NextFloat(-0.1f, 0.1f);
                float speed = Main.rand.NextFloat(7f, 15f);
                Vector2 vel = angle.ToRotationVector2() * speed;

                // Alternate between gold and rose for jubilant variety
                Color trailColor = (i % 3 == 0)
                    ? Color.Lerp(FireworkGold, WarmGold, Main.rand.NextFloat())
                    : (i % 3 == 1)
                        ? Color.Lerp(ConfettiPink, ChorusRose, Main.rand.NextFloat())
                        : CandlelightGold;
                trailColor.A = 0;

                var spark = new GlowSparkParticle(center, vel, trailColor, 0.4f * intensity, 25);
                MagnumParticleHandler.SpawnParticle(spark);
            }

            // Cascading expanding rings — firework rings
            for (int ring = 0; ring < 5; ring++)
            {
                float hue = (float)ring / 5f;
                Color ringColor = Color.Lerp(WarmGold, ChorusRose, hue) with { A = 0 };
                CustomParticles.HaloRing(center, ringColor, (0.4f + ring * 0.2f) * intensity, 18 + ring * 4);
            }

            // Confetti music notes raining
            SpawnConfettiNoteCascade(center, (int)(6 * intensity), 50f);

            WeaponFogVFX.SpawnAttackFog(center, "OdeToJoy", 0.8f * intensity, Vector2.Zero);
            LightBeamImpactVFX.SpawnImpact(center, "OdeToJoy", 1.2f * intensity);
            Lighting.AddLight(center, JubilantWhite.ToVector3() * 2f * intensity);
        }

        /// <summary>
        /// Confetti note cascade: Music notes raining like confetti — jubilant, celebratory.
        /// Notes drift and tumble like confetti at a victory parade.
        /// </summary>
        public static void SpawnConfettiNoteCascade(Vector2 center, int count, float spread)
        {
            for (int i = 0; i < count; i++)
            {
                Vector2 pos = center + Main.rand.NextVector2Circular(spread, spread * 0.5f);
                // Confetti drift: lateral wobble + gentle fall
                Vector2 vel = new Vector2(
                    Main.rand.NextFloat(-3f, 3f),
                    Main.rand.NextFloat(-6f, -1f)
                );

                Color noteColor = Main.rand.Next(4) switch
                {
                    0 => WarmGold,
                    1 => ConfettiPink,
                    2 => ConfettiGold,
                    _ => CandlelightGold
                };
                noteColor.A = 0;

                int noteType = Main.rand.Next(4); // Quarter, Eighth, Sixteenth, Double
                var note = new MusicNoteParticle(pos, vel, noteColor, noteColor, 0.3f, 40, noteType);
                MagnumParticleHandler.SpawnParticle(note);
            }
        }

        /// <summary>
        /// Jubilant golden overflow: The screen overflows with warm golden light.
        /// Used as ambient during enrage — the warmth of a thousand candles.
        /// </summary>
        public static void JubilantGoldenOverflow(Vector2 bossCenter, float intensity = 1f)
        {
            // Dense warm glow particles across the arena
            int glowCount = (int)(12 * intensity);
            for (int i = 0; i < glowCount; i++)
            {
                Vector2 pos = bossCenter + Main.rand.NextVector2Circular(600f, 400f);
                Vector2 vel = new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), Main.rand.NextFloat(-1.5f, -0.3f));

                Color glowColor = Color.Lerp(CandlelightGold, FireworkGold, Main.rand.NextFloat()) with { A = 0 };
                var glow = new GenericGlowParticle(pos, vel, glowColor, 0.2f + Main.rand.NextFloat(0.15f), 35, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }

            // Rose petals drifting everywhere
            if (Main.rand.NextBool(2))
            {
                Vector2 pos = bossCenter + Main.rand.NextVector2Circular(500f, 400f);
                Vector2 vel = new Vector2(Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(0.5f, 2f));
                RoseBudParticle.SpawnBurst(pos, 1, 1f, ConfettiPink, WarmGold, 0.3f, 40);
            }
        }

        /// <summary>
        /// Being-hit celebration: Even taking damage feels magnificent in the final chorus.
        /// Golden sparkle burst on the player when they're hit.
        /// </summary>
        public static void MagnificentHitEffect(Vector2 hitPosition, float intensity = 1f)
        {
            // Warm golden burst rather than painful red
            CustomParticles.GenericFlare(hitPosition, CandlelightGold, 0.7f * intensity, 15);
            CustomParticles.HaloRing(hitPosition, WarmGold with { A = 0 }, 0.3f * intensity, 18);

            // Mini confetti burst
            for (int i = 0; i < 4; i++)
            {
                float angle = MathHelper.TwoPi * i / 4f + Main.rand.NextFloat(-0.3f, 0.3f);
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 6f);
                Color sparkColor = Color.Lerp(WarmGold, ConfettiPink, Main.rand.NextFloat()) with { A = 0 };
                var sparkle = new SparkleParticle(hitPosition, vel, sparkColor, 0.2f * intensity, 15);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }

            // Single music note ascending from impact
            Vector2 noteVel = new Vector2(Main.rand.NextFloat(-1f, 1f), -3f);
            var note = new MusicNoteParticle(hitPosition, noteVel, WarmGold with { A = 0 }, WarmGold with { A = 0 }, 0.25f, 25, Main.rand.Next(4));
            MagnumParticleHandler.SpawnParticle(note);
        }

        #endregion

        #region Phase Transition VFX

        /// <summary>
        /// Rose petal shedding vortex: Petals spiral inward toward the boss during transition.
        /// The solo voice fades — the chorus prepares to join.
        /// </summary>
        public static void PetalSheddingVortex(Vector2 bossCenter, float progress, float intensity = 1f)
        {
            int petalCount = (int)(6 * intensity);
            for (int i = 0; i < petalCount; i++)
            {
                float angle = MathHelper.TwoPi * i / petalCount + progress * MathHelper.TwoPi * 0.3f;
                float radius = 120f - progress * 50f;
                Vector2 pos = bossCenter + angle.ToRotationVector2() * radius;
                // Spiral inward
                Vector2 vel = (bossCenter - pos).SafeNormalize(Vector2.Zero) * (2f + progress * 3f);
                vel += new Vector2(-MathF.Sin(angle), MathF.Cos(angle)) * 3f; // tangential swirl

                Color petalColor = Color.Lerp(RosePink, WarmGold, progress) with { A = 0 };
                RoseBudParticle.SpawnBurst(pos, 1, 0f, petalColor, CandlelightGold, 0.3f * (1f - progress * 0.3f), 20);
            }

            // Brightening core glow as chorus builds
            CustomParticles.GenericFlare(bossCenter, CandlelightGold with { A = 0 }, 0.5f + progress * 0.5f, 10);
        }

        /// <summary>
        /// Chorus awakening explosion: Massive burst at the moment Phase 2 begins.
        /// All instruments join at once — the triumphant climax of the transition.
        /// </summary>
        public static void ChorusAwakeningBurst(Vector2 center, float intensity = 1f)
        {
            // Massive golden core flash
            CustomParticles.GenericFlare(center, Color.White, 1.2f, 20);
            CustomParticles.GenericFlare(center, WarmGold, 1.2f, 25);
            CustomParticles.GenericFlare(center, ChorusRose, 1.2f, 28);

            // 8 expanding halos — the chorus joining
            for (int ring = 0; ring < 8; ring++)
            {
                float progress = ring / 8f;
                Color ringColor = Color.Lerp(WarmGold, ChorusRose, progress) with { A = 0 };
                CustomParticles.HaloRing(center, ringColor, (0.4f + ring * 0.15f) * intensity, 18 + ring * 3);
            }

            // Massive radial rose petal burst
            RosePetalDownbeat(center, 24, 12f * intensity);

            // Music note shower in all directions
            SpawnMusicNoteCascade(center, Vector2.Zero, WarmGold, 8, 80f);

            // Screen effects
            MagnumScreenEffects.AddScreenShake(15f * intensity);
            DynamicSkyboxSystem.TriggerFlash(WarmGold, 0.8f * intensity);

            WeaponFogVFX.SpawnAttackFog(center, "OdeToJoy", 1.5f * intensity, Vector2.Zero);
            LightBeamImpactVFX.SpawnImpact(center, "OdeToJoy", 1.5f * intensity);

            Lighting.AddLight(center, JubilantWhite.ToVector3() * 2.5f * intensity);
        }

        /// <summary>
        /// Triumphant celebration: The climax of a major attack or boss death.
        /// Everything at once — gold, roses, music, light.
        /// </summary>
        public static void TriumphantCelebration(Vector2 center, float intensity = 1f)
        {
            // Core supernova
            CustomParticles.GenericFlare(center, Color.White, 1.2f, 22);
            CustomParticles.GenericFlare(center, WarmGold, 1.2f, 26);

            // 10 cascading halos
            for (int ring = 0; ring < 10; ring++)
            {
                float progress = ring / 10f;
                Color ringColor = Color.Lerp(WarmGold, ChorusRose, progress) with { A = 0 };
                CustomParticles.HaloRing(center, ringColor, (0.5f + ring * 0.18f) * intensity, 20 + ring * 3);
            }

            // Radial golden spark explosion
            int sparkCount = (int)(24 * intensity);
            for (int i = 0; i < sparkCount; i++)
            {
                float angle = MathHelper.TwoPi * i / sparkCount;
                float speed = Main.rand.NextFloat(6f, 16f);
                Vector2 vel = angle.ToRotationVector2() * speed;

                Color sparkColor = Color.Lerp(WarmGold, JubilantWhite, Main.rand.NextFloat()) with { A = 0 };
                var spark = new GlowSparkParticle(center, vel, sparkColor, 0.5f * intensity, 28);
                MagnumParticleHandler.SpawnParticle(spark);
            }

            // Massive music note cascade
            SpawnMusicNoteCascade(center, Vector2.Zero, WarmGold, 10, 100f);

            // Rose petal burst
            RosePetalDownbeat(center, 18, 10f * intensity);

            MagnumScreenEffects.AddScreenShake(20f * intensity);
            DynamicSkyboxSystem.TriggerFlash(JubilantWhite, 1f * intensity);

            WeaponFogVFX.SpawnAttackFog(center, "OdeToJoy", 2f * intensity, Vector2.Zero);
            LightBeamImpactVFX.SpawnImpact(center, "OdeToJoy", 2f * intensity);

            Lighting.AddLight(center, JubilantWhite.ToVector3() * 3f * intensity);
        }

        #endregion

        #region Shared Musical Utilities

        /// <summary>
        /// Spawns a cascade of floating music notes in a direction with spread.
        /// The fundamental musical particle effect — notes ascending from every strike.
        /// </summary>
        public static void SpawnMusicNoteCascade(Vector2 center, Vector2 baseVelocity, Color color, int count, float spread)
        {
            for (int i = 0; i < count; i++)
            {
                Vector2 pos = center + Main.rand.NextVector2Circular(spread * 0.5f, spread * 0.5f);
                Vector2 vel = baseVelocity + new Vector2(
                    Main.rand.NextFloat(-2f, 2f),
                    Main.rand.NextFloat(-3f, -0.5f) // Notes tend to rise
                );

                Color noteColor = Color.Lerp(color, CandlelightGold, Main.rand.NextFloat(0.3f)) with { A = 0 };
                int noteType = Main.rand.Next(4);
                float scale = 0.2f + Main.rand.NextFloat(0.15f);

                var note = new MusicNoteParticle(pos, vel, noteColor, noteColor, scale, 35, noteType);
                MagnumParticleHandler.SpawnParticle(note);
            }
        }

        /// <summary>
        /// Rose petal burst with configurable parameters. Central rose petal spawner.
        /// </summary>
        public static void SpawnRosePetals(Vector2 center, int count, float speed)
        {
            for (int i = 0; i < count; i++)
            {
                float angle = MathHelper.TwoPi * i / count + Main.rand.NextFloat(-0.2f, 0.2f);
                Vector2 vel = angle.ToRotationVector2() * speed * Main.rand.NextFloat(0.5f, 1f);
                vel.Y += Main.rand.NextFloat(0.3f, 1f); // gentle fall

                Color petalColor = Color.Lerp(RosePink, ChorusRose, Main.rand.NextFloat()) with { A = 0 };
                RoseBudParticle.SpawnBurst(center, 1, speed * 0.3f, petalColor, WarmGold, 0.3f, 30);
            }
        }

        /// <summary>
        /// Garden impact: Blooming burst of golden-green-rose particles.
        /// Used for general impacts during the fight.
        /// </summary>
        public static void GardenImpact(Vector2 center, float scale)
        {
            CustomParticles.GenericFlare(center, JubilantWhite, 1.2f * scale, 18);
            CustomParticles.GenericFlare(center, WarmGold, 0.8f * scale, 22);

            // Rose bud bloom burst
            int count = (int)(6 * scale);
            for (int i = 0; i < count; i++)
            {
                float angle = MathHelper.TwoPi * i / count;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 7f);
                RoseBudParticle.SpawnBurst(center + vel * 2f, 1, 2f, RosePink, WarmGold, 0.3f * scale, 22);
            }

            // Golden halos
            CustomParticles.HaloRing(center, WarmGold with { A = 0 }, 0.4f * scale, 20);
            CustomParticles.HaloRing(center, ChorusRose with { A = 0 }, 0.3f * scale, 24);

            Lighting.AddLight(center, WarmGold.ToVector3() * scale);
        }

        /// <summary>
        /// Blossom impact: Smaller, more focused bloom burst.
        /// </summary>
        public static void BlossomImpact(Vector2 center, float scale)
        {
            CustomParticles.GenericFlare(center, CandlelightGold, 0.6f * scale, 15);

            int count = (int)(3 * scale);
            for (int i = 0; i < count; i++)
            {
                float angle = MathHelper.TwoPi * i / count;
                Vector2 pos = center + angle.ToRotationVector2() * 15f;
                Vector2 vel = angle.ToRotationVector2() * 2f;
                RoseBudParticle.SpawnBurst(pos, 1, 1.5f, RosePink, WarmGold, 0.25f * scale, 18);
            }

            Lighting.AddLight(center, CandlelightGold.ToVector3() * 0.6f * scale);
        }

        /// <summary>
        /// Ambient boss glow: Per-frame glow particles around the boss.
        /// Intensity increases with each phase.
        /// </summary>
        public static void BossAmbientGlow(Vector2 bossCenter, int phase, bool isEnraged)
        {
            float baseIntensity = phase switch
            {
                0 => 0.4f,  // Phase 1: Warm candlelight
                1 => 0.7f,  // Phase 2: Brighter, more lights
                2 => 1.0f,  // Phase 3: Full orchestra shimmer
                _ => 0.4f
            };

            if (isEnraged) baseIntensity = 1.3f;

            // Soft golden glow around boss
            if (Main.rand.NextBool(isEnraged ? 2 : 4))
            {
                Vector2 pos = bossCenter + Main.rand.NextVector2Circular(80f, 80f);
                Vector2 vel = new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), Main.rand.NextFloat(-0.8f, 0.2f));
                Color glowColor = Color.Lerp(WarmGold, CandlelightGold, Main.rand.NextFloat()) with { A = 0 };
                var glow = new GenericGlowParticle(pos, vel, glowColor, 0.2f * baseIntensity, 30, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }

            // Rose petals (more in later phases)
            int petalChance = phase switch { 0 => 25, 1 => 15, 2 => 8, _ => 25 };
            if (isEnraged) petalChance = 4;
            if (Main.rand.NextBool(petalChance))
            {
                Vector2 pos = bossCenter + Main.rand.NextVector2Circular(100f, 100f);
                Vector2 vel = new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(0.2f, 1f));
                RoseBudParticle.SpawnBurst(pos, 1, 0.5f, RosePink with { A = 0 }, WarmGold, 0.25f, 35);
            }

            // Music notes floating up (sparse in P1, dense in enrage)
            int noteChance = phase switch { 0 => 40, 1 => 25, 2 => 12, _ => 40 };
            if (isEnraged) noteChance = 6;
            if (Main.rand.NextBool(noteChance))
            {
                Vector2 pos = bossCenter + Main.rand.NextVector2Circular(60f, 60f);
                Vector2 vel = new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-2f, -0.5f));
                Color noteColor = WarmGold with { A = 0 };
                var note = new MusicNoteParticle(pos, vel, noteColor, noteColor, 0.2f, 30, Main.rand.Next(4));
                MagnumParticleHandler.SpawnParticle(note);
            }

            // Phase 3+ helix orbit
            if (phase >= 2 || isEnraged)
            {
                float time = Main.GameUpdateCount * 0.03f;
                RosePetalHelixOrbit(bossCenter, time, 60f + phase * 15f, 0.5f);
            }

            // Lighting scales with phase
            float lightRadius = 0.8f + phase * 0.3f;
            if (isEnraged) lightRadius = 2f;
            Lighting.AddLight(bossCenter, WarmGold.ToVector3() * lightRadius);
        }

        #endregion
    }
}
