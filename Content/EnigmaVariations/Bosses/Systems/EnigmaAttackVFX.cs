using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.Bosses;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.VFX.Screen;

namespace MagnumOpus.Content.EnigmaVariations.Bosses.Systems
{
    /// <summary>
    /// Enigma, the Hollow Mystery — full boss VFX choreography.
    /// The player should feel WATCHED by something they cannot understand.
    ///
    /// Phase 1 — The Riddle: faint green symbols drift at vision periphery,
    ///           disappearing when looked at. Attack telegraphs assemble from
    ///           scattered glyph particles into impossibly complex formations.
    ///           Boss flickers between visible and translucent.
    /// Phase 2 — The Unraveling: reality distortion warps screen edges,
    ///           eerie green flame orbits in almost-recognizable patterns.
    ///           Attacks emerge from void-portal tears. Watching eyes in the
    ///           background sky track the player.
    /// Phase 3 — The Revelation: arena becomes void space lit by arcane fire,
    ///           boss splits into fractal copies, constant screen distortion.
    /// Enrage — Total Mystery: screen inverts, boss is void silhouette,
    ///          screaming green particle vortexes, reality tears as rips.
    ///
    /// Palette: void black, deep purple, eerie green flame, unsettling white.
    /// </summary>
    public static class EnigmaAttackVFX
    {
        // Canonical palette — gradient of unknowing
        private static readonly Color VoidBlack = new Color(10, 5, 15);
        private static readonly Color DeepPurple = new Color(80, 20, 140);
        private static readonly Color EerieGreen = new Color(40, 220, 80);
        private static readonly Color ArcaneGreen = new Color(100, 255, 130);
        private static readonly Color UnsettlingWhite = new Color(220, 200, 255);
        private static readonly Color VoidPurple = new Color(50, 10, 80);

        #region Phase 1 — The Riddle

        /// <summary>
        /// VoidLunge telegraph — scattered glyph particles converge into a threat line,
        /// assembling like an impossible equation solving itself.
        /// </summary>
        public static void VoidLungeTelegraph(Vector2 position, Vector2 target)
        {
            Vector2 dir = (target - position).SafeNormalize(Vector2.UnitX);
            TelegraphSystem.ThreatLine(position, dir, 450f, 30, DeepPurple * 0.5f);

            // Scattered glyphs converge toward the line — the riddle assembling
            for (int i = 0; i < 8; i++)
            {
                Vector2 scatterPos = position + Main.rand.NextVector2Circular(120f, 120f);
                Vector2 convergeVel = (position + dir * 100f - scatterPos).SafeNormalize(Vector2.Zero) * 2f;
                MagnumParticleHandler.SpawnParticle(new SparkleParticle(
                    scatterPos, convergeVel, EerieGreen * 0.6f, 0.2f, 25));
            }
            CustomParticles.Glyph(position + dir * 50f, DeepPurple * 0.4f, 0.15f);
        }

        /// <summary>
        /// VoidLunge trail — afterimage flicker, the boss stutters between
        /// existing and not existing as it moves.
        /// </summary>
        public static void VoidLungeTrail(Vector2 position, Vector2 velocity)
        {
            // Flickering void wisps — the boss's passage tears at vision
            if (Main.rand.NextBool(3))
            {
                var smoke = new HeavySmokeParticle(
                    position + Main.rand.NextVector2Circular(20f, 20f),
                    velocity * -0.2f, VoidBlack, 15, 0.15f, 0.3f, 0.02f, false);
                MagnumParticleHandler.SpawnParticle(smoke);
            }
            MagnumParticleHandler.SpawnParticle(new BloomParticle(
                position, velocity * -0.1f, DeepPurple * 0.5f, 0.3f, 12));
        }

        /// <summary>
        /// VoidLunge impact — reality stutters, brief green flash,
        /// glyph fragments scatter from the strike point.
        /// </summary>
        public static void VoidLungeImpact(Vector2 position)
        {
            MagnumScreenEffects.AddScreenShake(8f);
            EnigmaSkySystem.TriggerVoidFlash(0.4f);

            // Glyph fragments scatter — the equation shatters on impact
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f + Main.rand.NextFloat(0.2f);
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 4f);
                MagnumParticleHandler.SpawnParticle(new SparkleParticle(
                    position, vel, EerieGreen, 0.25f, 20));
            }
            CustomParticles.HaloRing(position, DeepPurple * 0.6f, 0.4f, 14);
            MagnumParticleHandler.SpawnParticle(new BloomParticle(
                position, Vector2.Zero, VoidPurple, 0.6f, 18));
        }

        /// <summary>
        /// EyeVolley telegraph — symbols materialize haltingly, as if the boss
        /// is remembering how to form them. Particles snap into formation positions.
        /// </summary>
        public static void EyeVolleyTelegraph(Vector2 position)
        {
            TelegraphSystem.ConvergingRing(position, 80f, 6, EerieGreen * 0.4f);

            // Glyphs stutter into place — assembling like letters spelling a word you can't read
            for (int i = 0; i < 5; i++)
            {
                float angle = MathHelper.TwoPi * i / 5f;
                Vector2 glyphPos = position + angle.ToRotationVector2() * 60f;
                glyphPos += Main.rand.NextVector2Circular(15f, 15f); // Imprecise — still forming
                CustomParticles.Glyph(glyphPos, EerieGreen * 0.5f, 0.15f + Main.rand.NextFloat(0.1f));
            }
        }

        /// <summary>
        /// EyeVolley release — eyes blink open in the glyph formation,
        /// each release spawns a watching eye particle that tracks briefly.
        /// </summary>
        public static void EyeVolleyRelease(Vector2 position, Vector2 direction)
        {
            EnigmaSkySystem.TriggerGreenFlash(0.2f);
            CustomParticles.EnigmaEyeGaze(position, EerieGreen * 0.7f, 0.4f, direction);
            MagnumParticleHandler.SpawnParticle(new BloomParticle(
                position, direction * 0.3f, EerieGreen * 0.5f, 0.35f, 15));
        }

        /// <summary>
        /// ParadoxRing telegraph — concentric glyph rings form,
        /// rotating in opposite directions like a lock mechanism.
        /// </summary>
        public static void ParadoxRingTelegraph(Vector2 center)
        {
            TelegraphSystem.ConvergingRing(center, 120f, 8, DeepPurple * 0.5f);

            // Counter-rotating glyph hints
            for (int ring = 0; ring < 2; ring++)
            {
                float radius = 60f + ring * 40f;
                float direction = ring % 2 == 0 ? 1f : -1f;
                float timeOffset = (float)Main.timeForVisualEffects * 0.02f * direction;
                for (int i = 0; i < 4; i++)
                {
                    float angle = MathHelper.TwoPi * i / 4f + timeOffset;
                    Vector2 pos = center + angle.ToRotationVector2() * radius;
                    Color col = ring == 0 ? DeepPurple * 0.4f : EerieGreen * 0.3f;
                    CustomParticles.GenericFlare(pos, col, 0.15f, 8);
                }
            }
        }

        /// <summary>
        /// ParadoxRing release — the lock snaps open, ring of void energy
        /// expands outward with trailing green fire.
        /// </summary>
        public static void ParadoxRingRelease(Vector2 center, float radius, int ringIndex)
        {
            EnigmaSkySystem.TriggerPurpleFlash(0.25f + ringIndex * 0.05f);
            int count = 10;
            for (int i = 0; i < count; i++)
            {
                float angle = MathHelper.TwoPi * i / count;
                Vector2 pos = center + angle.ToRotationVector2() * radius;
                Color color = Color.Lerp(DeepPurple, EerieGreen, (float)i / count);
                MagnumParticleHandler.SpawnParticle(new SparkleParticle(
                    pos, angle.ToRotationVector2() * 1f, color * 0.6f, 0.2f, 20));
            }
            MagnumParticleHandler.SpawnParticle(new BloomParticle(
                center, Vector2.Zero, DeepPurple * 0.5f, 0.5f, 18));
        }

        /// <summary>
        /// ShadowDash telegraph — void distortion flickers at the destination,
        /// as if space is already wounded where the boss will appear.
        /// </summary>
        public static void ShadowDashTelegraph(Vector2 position, Vector2 target)
        {
            Vector2 dir = (target - position).SafeNormalize(Vector2.UnitX);
            TelegraphSystem.ThreatLine(position, dir, 500f, 30, VoidBlack * 0.7f);

            // Pre-wound: flickering void distortion at destination
            for (int i = 0; i < 4; i++)
            {
                Vector2 destFlicker = target + Main.rand.NextVector2Circular(30f, 30f);
                MagnumParticleHandler.SpawnParticle(new BloomParticle(
                    destFlicker, Vector2.Zero, VoidPurple * 0.4f, 0.2f, 10));
            }
        }

        /// <summary>
        /// ShadowDash impact — the boss materializes with a void implosion,
        /// scattered glyphs briefly visible as if reality is correcting itself.
        /// </summary>
        public static void ShadowDashImpact(Vector2 position)
        {
            MagnumScreenEffects.AddScreenShake(6f);
            EnigmaSkySystem.TriggerVoidFlash(0.4f);

            // Void implosion — reality snapping back
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 startPos = position + angle.ToRotationVector2() * 80f;
                Vector2 implodeVel = (position - startPos).SafeNormalize(Vector2.Zero) * 3f;
                MagnumParticleHandler.SpawnParticle(new SparkleParticle(
                    startPos, implodeVel, DeepPurple * 0.5f, 0.2f, 15));
            }
            CustomParticles.HaloRing(position, VoidPurple, 0.35f, 12);
        }

        /// <summary>
        /// GlyphCircle telegraph — scattered symbol particles drift inward,
        /// assembling into the ritual formation. Impossibly complex.
        /// </summary>
        public static void GlyphCircleTelegraph(Vector2 center)
        {
            // Glyphs assemble from scattered particles
            for (int i = 0; i < 10; i++)
            {
                float targetAngle = MathHelper.TwoPi * i / 10f;
                Vector2 targetPos = center + targetAngle.ToRotationVector2() * 80f;
                Vector2 scatterPos = targetPos + Main.rand.NextVector2Circular(60f, 60f);
                Vector2 assembleVel = (targetPos - scatterPos).SafeNormalize(Vector2.Zero) * 1.5f;
                Color col = i % 2 == 0 ? DeepPurple * 0.5f : EerieGreen * 0.4f;
                MagnumParticleHandler.SpawnParticle(new SparkleParticle(
                    scatterPos, assembleVel, col, 0.15f, 30));
            }
        }

        /// <summary>
        /// GlyphCircle release — each glyph node detonates with a void flash,
        /// releasing an eye that briefly watches the player.
        /// </summary>
        public static void GlyphCircleRelease(Vector2 center, int glyphIndex)
        {
            float angle = MathHelper.TwoPi * glyphIndex / 8f;
            Vector2 pos = center + angle.ToRotationVector2() * 80f;

            if (glyphIndex % 2 == 0)
                EnigmaSkySystem.TriggerPurpleFlash(0.2f);

            MagnumParticleHandler.SpawnParticle(new BloomParticle(
                pos, Vector2.Zero, EerieGreen * 0.4f, 0.35f, 16));
            CustomParticles.Glyph(pos, EerieGreen * 0.6f, 0.2f);
        }

        #endregion

        #region Phase 2 — The Unraveling

        /// <summary>
        /// TendrilRise telegraph — the ground beneath flickers with
        /// eerie green lines, as if something is pressing through from below.
        /// </summary>
        public static void TendrilRiseTelegraph(Vector2 position)
        {
            TelegraphSystem.ImpactPoint(position, 50f, 30);

            // Green fire lines flicker beneath — something pressing through
            for (int i = 0; i < 3; i++)
            {
                Vector2 lineStart = position + new Vector2(Main.rand.NextFloat(-40f, 40f), 10f);
                Vector2 lineVel = new Vector2(0, Main.rand.NextFloat(-1.5f, -0.5f));
                MagnumParticleHandler.SpawnParticle(new SparkleParticle(
                    lineStart, lineVel, EerieGreen * 0.4f, 0.15f, 20));
            }
        }

        /// <summary>
        /// TendrilRise release — void tendrils erupt with green fire trailing,
        /// scattered eye particles blink open along the eruption path.
        /// </summary>
        public static void TendrilRiseRelease(Vector2 position)
        {
            EnigmaSkySystem.TriggerGreenFlash(0.3f);

            // Ascending eruption with eye-flickers along the path
            for (int i = 0; i < 5; i++)
            {
                Vector2 tendrilPos = position + new Vector2(Main.rand.NextFloat(-15f, 15f), i * -30f);
                Color color = Color.Lerp(EerieGreen, ArcaneGreen, i / 5f);
                MagnumParticleHandler.SpawnParticle(new BloomParticle(
                    tendrilPos, new Vector2(0, -1.5f), color * 0.5f, 0.25f + i * 0.04f, 20));
                if (i % 2 == 0)
                    CustomParticles.EnigmaEyeGaze(tendrilPos, EerieGreen * 0.4f, 0.3f);
            }
        }

        /// <summary>
        /// ParadoxWeb telegraph — threat lines radiate from center to nodes,
        /// forming an impossible geometric web. Each node pulses with void light.
        /// </summary>
        public static void ParadoxWebTelegraph(Vector2 center, Vector2[] nodes)
        {
            for (int i = 0; i < nodes.Length; i++)
            {
                TelegraphSystem.ThreatLine(center, (nodes[i] - center).SafeNormalize(Vector2.UnitX),
                    (nodes[i] - center).Length(), 25, DeepPurple * 0.4f);
                // Node pulses — watching
                MagnumParticleHandler.SpawnParticle(new BloomParticle(
                    nodes[i], Vector2.Zero, EerieGreen * 0.3f, 0.2f, 15));
            }
        }

        /// <summary>
        /// ParadoxWeb node activation — the web snaps taut,
        /// void portal briefly tears open at the node.
        /// </summary>
        public static void ParadoxWebActivate(Vector2 nodePos)
        {
            // Void-portal tear at the node
            CustomParticles.HaloRing(nodePos, VoidPurple, 0.3f, 12);
            for (int i = 0; i < 4; i++)
            {
                float angle = MathHelper.TwoPi * i / 4f + Main.rand.NextFloat(0.3f);
                MagnumParticleHandler.SpawnParticle(new SparkleParticle(
                    nodePos, angle.ToRotationVector2() * 2f, EerieGreen * 0.5f, 0.2f, 15));
            }
        }

        /// <summary>
        /// GreenFlameOrbit — eerie green fire orbiting the boss in patterns
        /// that almost form recognizable shapes but never quite resolve.
        /// Called per-frame during Phase 2+ ambient.
        /// </summary>
        public static void GreenFlameOrbit(Vector2 bossCenter, int timer, int difficultyTier)
        {
            int flameCount = 3 + difficultyTier;
            float orbitRadius = 100f + difficultyTier * 20f;

            for (int i = 0; i < flameCount; i++)
            {
                // Almost-recognizable pattern: base orbit + perturbation that prevents regularity
                float baseAngle = MathHelper.TwoPi * i / flameCount + timer * 0.03f;
                float perturbation = (float)Math.Sin(timer * 0.07f + i * 1.7f) * 0.4f;
                float angle = baseAngle + perturbation;
                float radiusWobble = orbitRadius + (float)Math.Sin(timer * 0.05f + i * 2.3f) * 20f;

                Vector2 flamePos = bossCenter + angle.ToRotationVector2() * radiusWobble;
                Color flameColor = Color.Lerp(EerieGreen, ArcaneGreen, (float)Math.Sin(timer * 0.04f + i) * 0.5f + 0.5f);

                MagnumParticleHandler.SpawnParticle(new BloomParticle(
                    flamePos, Vector2.Zero, flameColor * 0.4f, 0.15f, 6));
            }
        }

        /// <summary>
        /// Watching gaze telegraph — eyes materialize in a semicircle facing the player,
        /// each blinking open one by one.
        /// </summary>
        public static void WatchingGazeTelegraph(Vector2 bossCenter, Vector2 playerCenter, int eyeCount)
        {
            float baseAngle = (playerCenter - bossCenter).ToRotation();
            float arcSpan = MathHelper.Pi * 0.6f;
            for (int i = 0; i < eyeCount; i++)
            {
                float t = (float)i / (eyeCount - 1) - 0.5f;
                float angle = baseAngle + t * arcSpan;
                Vector2 eyePos = bossCenter + angle.ToRotationVector2() * 120f;
                CustomParticles.EnigmaEyeGaze(eyePos, EerieGreen * 0.5f, 0.3f, playerCenter);
            }
        }

        /// <summary>
        /// Watching gaze fire — an eye fires toward the player with trailing green afterglow.
        /// </summary>
        public static void WatchingGazeFire(Vector2 eyePos, Vector2 direction)
        {
            MagnumParticleHandler.SpawnParticle(new BloomParticle(
                eyePos, direction * 0.5f, EerieGreen * 0.5f, 0.3f, 12));
            CustomParticles.GenericFlare(eyePos, ArcaneGreen * 0.6f, 0.3f, 10);
        }

        /// <summary>
        /// EntropicSurge telegraph — void energy coalesces with scattered glyphs,
        /// building pressure. Screen edges begin to warp subtly.
        /// </summary>
        public static void EntropicSurgeTelegraph(Vector2 center, float progress)
        {
            int particleCount = (int)(4 + progress * 8);
            float radius = 150f * (1f - progress * 0.5f);
            for (int i = 0; i < particleCount; i++)
            {
                float angle = MathHelper.TwoPi * i / particleCount;
                Vector2 pos = center + angle.ToRotationVector2() * radius;
                Vector2 vel = (center - pos).SafeNormalize(Vector2.Zero) * (1f + progress * 2f);
                Color col = Color.Lerp(DeepPurple, EerieGreen, progress) * (0.3f + progress * 0.3f);
                MagnumParticleHandler.SpawnParticle(new SparkleParticle(
                    pos, vel, col, 0.15f + progress * 0.1f, 15));
            }
        }

        /// <summary>
        /// EntropicSurge release — expanding wave of void distortion.
        /// Green fire trails the wavefront.
        /// </summary>
        public static void EntropicSurgeRelease(Vector2 center)
        {
            EnigmaSkySystem.TriggerGreenFlash(0.4f);
            MagnumScreenEffects.AddScreenShake(10f);

            for (int i = 0; i < 12; i++)
            {
                float angle = MathHelper.TwoPi * i / 12f;
                Vector2 vel = angle.ToRotationVector2() * 3f;
                MagnumParticleHandler.SpawnParticle(new BloomParticle(
                    center, vel, Color.Lerp(DeepPurple, EerieGreen, i / 12f) * 0.5f, 0.3f, 20));
            }
            CustomParticles.HaloRing(center, EerieGreen * 0.5f, 0.5f, 16);
        }

        /// <summary>
        /// SigilSnare telegraph — glyph trap converges on the player.
        /// Symbols drift inward from the edges.
        /// </summary>
        public static void SigilSnareTelegraph(Vector2 center, float convergeRadius)
        {
            BossVFXOptimizer.DangerZoneRing(center, convergeRadius, 14);
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f;
                Vector2 pos = center + angle.ToRotationVector2() * convergeRadius;
                CustomParticles.Glyph(pos, DeepPurple * 0.4f, 0.12f);
            }
        }

        /// <summary>
        /// SigilSnare activate — the trap snaps shut with a void implosion.
        /// </summary>
        public static void SigilSnareActivate(Vector2 center)
        {
            EnigmaSkySystem.TriggerPurpleFlash(0.35f);
            MagnumScreenEffects.AddScreenShake(8f);
            CustomParticles.HaloRing(center, DeepPurple * 0.6f, 0.45f, 14);
            MagnumParticleHandler.SpawnParticle(new BloomParticle(
                center, Vector2.Zero, VoidPurple, 0.6f, 20));
        }

        /// <summary>
        /// VoidBeamPincer telegraph — orbs charge with visible energy lines
        /// connecting them to the boss. Green fire crackles between.
        /// </summary>
        public static void VoidBeamPincerTelegraph(Vector2 orbPos, Vector2 bossCenter)
        {
            MagnumParticleHandler.SpawnParticle(new BloomParticle(
                orbPos, Vector2.Zero, EerieGreen * 0.4f, 0.25f, 12));
            // Crackling energy between orb and boss
            Vector2 midpoint = Vector2.Lerp(orbPos, bossCenter, 0.5f);
            MagnumParticleHandler.SpawnParticle(new SparkleParticle(
                midpoint + Main.rand.NextVector2Circular(15f, 15f),
                Vector2.Zero, ArcaneGreen * 0.3f, 0.1f, 8));
        }

        /// <summary>
        /// VoidBeamPincer fire — beam fires with void distortion trail.
        /// </summary>
        public static void VoidBeamPincerFire(Vector2 orbPos, Vector2 direction)
        {
            EnigmaSkySystem.TriggerGreenFlash(0.3f);
            MagnumParticleHandler.SpawnParticle(new BloomParticle(
                orbPos, direction * 2f, ArcaneGreen * 0.6f, 0.4f, 15));
            CustomParticles.GenericFlare(orbPos, EerieGreen, 0.4f, 12);
        }

        #endregion

        #region Phase 3 — The Revelation That Explains Nothing

        /// <summary>
        /// EyeOfTheVoid telegraph — a massive eye formation assembles from
        /// scattered watching particles. The arena darkens.
        /// </summary>
        public static void EyeOfTheVoidTelegraph(Vector2 center)
        {
            TelegraphSystem.ConvergingRing(center, 180f, 12, EerieGreen * 0.5f);

            // Massive eye assembling — watching particles converge
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 eyePartPos = center + angle.ToRotationVector2() * 120f;
                Vector2 convergeVel = (center - eyePartPos).SafeNormalize(Vector2.Zero) * 1.5f;
                CustomParticles.EnigmaEyeGaze(eyePartPos, EerieGreen * 0.4f, 0.3f, center);
                MagnumParticleHandler.SpawnParticle(new SparkleParticle(
                    eyePartPos, convergeVel, ArcaneGreen * 0.3f, 0.15f, 25));
            }
        }

        /// <summary>
        /// EyeOfTheVoid release — the eye opens fully and fires,
        /// massive green beam with void distortion around it.
        /// </summary>
        public static void EyeOfTheVoidRelease(Vector2 center, Vector2 target)
        {
            MagnumScreenEffects.AddScreenShake(15f);
            EnigmaSkySystem.TriggerGreenFlash(0.6f);

            Vector2 dir = (target - center).SafeNormalize(Vector2.UnitX);
            MagnumParticleHandler.SpawnParticle(new BloomParticle(
                center, Vector2.Zero, ArcaneGreen, 1.0f, 25));

            // Beam path particles
            for (int i = 0; i < 8; i++)
            {
                Vector2 beamPos = center + dir * (i * 80f);
                MagnumParticleHandler.SpawnParticle(new BloomParticle(
                    beamPos, dir * 2f, EerieGreen * 0.5f, 0.3f, 15));
            }
        }

        /// <summary>
        /// ParadoxMirror spawn — a fractal copy materializes with a void tear.
        /// Clone appears from a portal with scattered glyph debris.
        /// </summary>
        public static void ParadoxMirrorSpawn(Vector2 clonePosition)
        {
            EnigmaSkySystem.TriggerVoidFlash(0.3f);

            // Void portal tear at clone spawn
            MagnumParticleHandler.SpawnParticle(new BloomParticle(
                clonePosition, Vector2.Zero, VoidBlack, 0.5f, 18));
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f;
                MagnumParticleHandler.SpawnParticle(new SparkleParticle(
                    clonePosition, angle.ToRotationVector2() * 2f,
                    DeepPurple * 0.5f, 0.2f, 18));
            }
            CustomParticles.Glyph(clonePosition, EerieGreen * 0.4f, 0.2f);
        }

        /// <summary>
        /// ParadoxMirror death — the clone collapses into void dust,
        /// imploding with a brief green flash.
        /// </summary>
        public static void ParadoxMirrorDeath(Vector2 clonePosition)
        {
            EnigmaSkySystem.TriggerPurpleFlash(0.25f);

            // Implosion — particles rush inward
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f;
                Vector2 startPos = clonePosition + angle.ToRotationVector2() * 60f;
                Vector2 implodeVel = (clonePosition - startPos).SafeNormalize(Vector2.Zero) * 3f;
                MagnumParticleHandler.SpawnParticle(new SparkleParticle(
                    startPos, implodeVel, EerieGreen * 0.5f, 0.2f, 12));
            }
            MagnumParticleHandler.SpawnParticle(new BloomParticle(
                clonePosition, Vector2.Zero, VoidPurple * 0.6f, 0.4f, 15));
        }

        /// <summary>
        /// UltimateEnigma telegraph — the aria of unknowing. Converging rings
        /// of green fire, scattered glyphs, reality distortion warnings.
        /// </summary>
        public static void UltimateEnigmaTelegraph(Vector2 center)
        {
            TelegraphSystem.ConvergingRing(center, 300f, 16, EerieGreen * 0.6f);
            BossVFXOptimizer.WarningFlare(center, 1.0f);

            // Scattered glyphs assemble in concentric patterns
            for (int ring = 0; ring < 3; ring++)
            {
                float radius = 80f + ring * 60f;
                for (int i = 0; i < 6; i++)
                {
                    float angle = MathHelper.TwoPi * i / 6f + ring * 0.3f;
                    Vector2 pos = center + angle.ToRotationVector2() * radius;
                    Color col = Color.Lerp(DeepPurple, EerieGreen, ring / 3f) * 0.4f;
                    CustomParticles.Glyph(pos, col, 0.12f + ring * 0.03f);
                }
            }
        }

        /// <summary>
        /// UltimateEnigma release — total revelation. Massive void explosion,
        /// eyes scatter outward, reality cracks everywhere.
        /// </summary>
        public static void UltimateEnigmaRelease(Vector2 center)
        {
            MagnumScreenEffects.AddScreenShake(22f);
            EnigmaSkySystem.TriggerRevelationFlash(0.8f);

            // Void supernova
            MagnumParticleHandler.SpawnParticle(new BloomParticle(
                center, Vector2.Zero, ArcaneGreen, 1.2f, 30));

            for (int i = 0; i < 14; i++)
            {
                float angle = MathHelper.TwoPi * i / 14f;
                Color color = Color.Lerp(DeepPurple, EerieGreen, i / 14f);
                MagnumParticleHandler.SpawnParticle(new BloomParticle(
                    center, angle.ToRotationVector2() * 3.5f, color * 0.5f, 0.4f, 25));
                MagnumParticleHandler.SpawnParticle(new SparkleParticle(
                    center, angle.ToRotationVector2() * 5f, ArcaneGreen * 0.4f, 0.3f, 30));
            }
        }

        /// <summary>
        /// MysteryMaze telegraph — glyph walls hint at the maze pattern.
        /// </summary>
        public static void MysteryMazeTelegraph(Vector2 wallStart, Vector2 wallEnd)
        {
            // Hint particles along the wall path
            int segments = (int)((wallEnd - wallStart).Length() / 40f);
            for (int i = 0; i < segments; i++)
            {
                Vector2 pos = Vector2.Lerp(wallStart, wallEnd, (float)i / segments);
                MagnumParticleHandler.SpawnParticle(new SparkleParticle(
                    pos + Main.rand.NextVector2Circular(5f, 5f),
                    Vector2.Zero, DeepPurple * 0.3f, 0.1f, 12));
            }
        }

        /// <summary>
        /// MysteryMaze wall activate — the maze wall solidifies with green fire.
        /// </summary>
        public static void MysteryMazeWallActivate(Vector2 wallCenter)
        {
            MagnumParticleHandler.SpawnParticle(new BloomParticle(
                wallCenter, Vector2.Zero, EerieGreen * 0.4f, 0.3f, 15));
        }

        /// <summary>
        /// VoidLaserWeb telegraph — rotating laser paths flicker with green fire.
        /// </summary>
        public static void VoidLaserWebTelegraph(Vector2 center, Vector2 end)
        {
            TelegraphSystem.LaserPath(center, end, 20f, 25, EerieGreen * 0.4f);
            MagnumParticleHandler.SpawnParticle(new SparkleParticle(
                Vector2.Lerp(center, end, 0.5f), Vector2.Zero,
                DeepPurple * 0.3f, 0.1f, 10));
        }

        /// <summary>
        /// VoidLaserWeb active beam — green fire trails the sweeping laser.
        /// </summary>
        public static void VoidLaserWebBeam(Vector2 center, Vector2 beamEnd)
        {
            for (int i = 0; i < 4; i++)
            {
                Vector2 pos = Vector2.Lerp(center, beamEnd, Main.rand.NextFloat());
                MagnumParticleHandler.SpawnParticle(new SparkleParticle(
                    pos + Main.rand.NextVector2Circular(5f, 5f),
                    Main.rand.NextVector2Circular(1f, 1f),
                    EerieGreen * 0.4f, 0.12f, 10));
            }
        }

        /// <summary>
        /// RealityZones telegraph — zone perimeters flicker into existence.
        /// </summary>
        public static void RealityZonesTelegraph(Vector2 center, float radius)
        {
            BossVFXOptimizer.DangerZoneRing(center, radius, 12);
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f;
                Vector2 pos = center + angle.ToRotationVector2() * radius;
                MagnumParticleHandler.SpawnParticle(new SparkleParticle(
                    pos, Vector2.Zero, EerieGreen * 0.3f, 0.1f, 15));
            }
        }

        /// <summary>
        /// RealityZones activate — the zone detonates with void energy.
        /// </summary>
        public static void RealityZonesActivate(Vector2 center, float radius)
        {
            EnigmaSkySystem.TriggerGreenFlash(0.3f);
            CustomParticles.HaloRing(center, EerieGreen * 0.5f, 0.4f, 14);
            MagnumParticleHandler.SpawnParticle(new BloomParticle(
                center, Vector2.Zero, VoidPurple, 0.5f, 18));
        }

        /// <summary>
        /// ParadoxJudgment telegraph — the judgment formation assembles.
        /// </summary>
        public static void ParadoxJudgmentTelegraph(Vector2 center)
        {
            TelegraphSystem.ConvergingRing(center, 200f, 12, DeepPurple * 0.5f);
            BossVFXOptimizer.WarningFlare(center, 0.8f);
        }

        /// <summary>
        /// ParadoxJudgment release — judgment wave with void halo.
        /// </summary>
        public static void ParadoxJudgmentRelease(Vector2 center, int wave, int totalWaves)
        {
            float intensity = (float)(wave + 1) / totalWaves;
            EnigmaSkySystem.TriggerPurpleFlash(0.3f + intensity * 0.3f);
            MagnumScreenEffects.AddScreenShake(8f + intensity * 10f);
            CustomParticles.HaloRing(center, Color.Lerp(DeepPurple, EerieGreen, intensity), 0.4f + intensity * 0.3f, 16);
            MagnumParticleHandler.SpawnParticle(new BloomParticle(
                center, Vector2.Zero, Color.Lerp(VoidPurple, ArcaneGreen, intensity), 0.6f + intensity * 0.4f, 20));
        }

        #endregion

        #region Enrage — Total Mystery

        /// <summary>
        /// Screaming green particle vortex — swirling green fire
        /// spirals inward toward the boss, the sound of impossibility.
        /// Called per-frame during enrage.
        /// </summary>
        public static void ScreamingVortex(Vector2 center, float arenaRadius)
        {
            // Green fire vortex — particles spiral inward
            for (int i = 0; i < 3; i++)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                float startRadius = arenaRadius * Main.rand.NextFloat(0.7f, 1.0f);
                Vector2 pos = center + angle.ToRotationVector2() * startRadius;
                // Spiral inward
                float tangent = angle + MathHelper.PiOver2;
                Vector2 vel = (center - pos).SafeNormalize(Vector2.Zero) * 2f
                    + tangent.ToRotationVector2() * 3f;
                Color col = Color.Lerp(EerieGreen, ArcaneGreen, Main.rand.NextFloat()) * 0.5f;
                MagnumParticleHandler.SpawnParticle(new SparkleParticle(
                    pos, vel, col, 0.2f, 25));
            }
            // Central void bloom pulses
            MagnumParticleHandler.SpawnParticle(new BloomParticle(
                center, Vector2.Zero, VoidBlack, 0.3f, 8));
        }

        /// <summary>
        /// Reality tear — a literal rip in the screen rendering.
        /// Jagged line of green fire with void behind it.
        /// </summary>
        public static void RealityTear(Vector2 start, float angle, float length)
        {
            Vector2 end = start + angle.ToRotationVector2() * length;
            EnigmaSkySystem.TriggerGreenFlash(0.4f);

            for (int i = 0; i < 6; i++)
            {
                Vector2 pos = Vector2.Lerp(start, end, i / 5f);
                pos += Main.rand.NextVector2Circular(5f, 5f);
                MagnumParticleHandler.SpawnParticle(new BloomParticle(
                    pos, Main.rand.NextVector2Circular(0.5f, 0.5f),
                    ArcaneGreen * 0.6f, 0.25f, 15));
                // Void behind the tear
                MagnumParticleHandler.SpawnParticle(new BloomParticle(
                    pos, Vector2.Zero, VoidBlack, 0.2f, 12));
            }
        }

        /// <summary>
        /// Enrage ambient consumption — void consumes the arena,
        /// unsettling white flashes punctuate the darkness.
        /// </summary>
        public static void EnrageAmbientConsumption(Vector2 center)
        {
            // Occasional unsettling white flash
            if (Main.rand.NextBool(30))
                EnigmaSkySystem.TriggerRevelationFlash(0.15f);

            // Void smoke billowing from boss
            var smoke = new HeavySmokeParticle(
                center + Main.rand.NextVector2Circular(50f, 50f),
                Main.rand.NextVector2Circular(2f, 2f),
                VoidBlack, Main.rand.Next(20, 35), 0.2f, 0.5f, 0.015f, false);
            MagnumParticleHandler.SpawnParticle(smoke);
        }

        #endregion
    }
}
