using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.VFX.Bloom;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Content.OdeToJoy.Bosses
{
    /// <summary>
    /// VFX helper for the Ode to Joy theme boss: Chromatic Rose Conductor.
    /// Two-phase fight — Phase 1 is graceful botanical elegance,
    /// Phase 2 is chromatic fury and triumphant celebration.
    /// Uses OdeToJoyPalette and OdeToJoyVFXLibrary for canonical colours
    /// and shared effects.
    /// </summary>
    public static class OdeToJoyEnemyVFX
    {
        // =====================================================================
        //  AMBIENT VFX
        // =====================================================================

        /// <summary>
        /// Per-frame ambient particles for the Chromatic Rose Conductor.
        /// Petal particles orbiting, rose buds, vine wisps.
        /// Phase 2 adds chromatic hue shifting and intensified particle density.
        /// </summary>
        public static void ConductorAmbientVFX(Vector2 center, float width, float height, bool isPhase2)
        {
            if (Main.dedServ) return;

            float time = (float)Main.timeForVisualEffects;

            // Orbiting petal glow particles (1-in-3)
            if (Main.rand.NextBool(3))
            {
                Vector2 offset = Main.rand.NextVector2Circular(width * 0.5f, height * 0.5f);
                Color col;
                if (isPhase2)
                {
                    // Chromatic shifting in phase 2
                    col = OdeToJoyPalette.GetBlossomGradient((time * 0.02f) % 1f) * 0.7f;
                }
                else
                {
                    col = Main.rand.NextBool() ? OdeToJoyPalette.RosePink : OdeToJoyPalette.VerdantGreen;
                    col *= 0.6f;
                }
                var particle = new GenericGlowParticle(center + offset,
                    Main.rand.NextVector2Circular(0.8f, 0.8f), col, 0.25f, 22, true);
                MagnumParticleHandler.SpawnParticle(particle);
            }

            // Rose bud sparkles (1-in-6)
            if (Main.rand.NextBool(6))
            {
                Vector2 budPos = center + Main.rand.NextVector2Circular(50f, 50f);
                Color budCol = Color.Lerp(OdeToJoyPalette.RosePink, OdeToJoyPalette.PetalPink, Main.rand.NextFloat());
                CustomParticles.GenericFlare(budPos, budCol * 0.5f, 0.18f, 12);
            }

            // Vine wisps — trailing leaf particles (1-in-10)
            if (Main.rand.NextBool(10))
            {
                Vector2 vinePos = center + Main.rand.NextVector2Circular(60f, 60f);
                OdeToJoyVFXLibrary.SpawnVineTrailDust(vinePos, new Vector2(0, -0.5f));
            }

            // Golden pollen sparkles (1-in-8)
            if (Main.rand.NextBool(8))
            {
                Vector2 pollenPos = center + Main.rand.NextVector2Circular(45f, 45f);
                CustomParticles.GenericFlare(pollenPos, OdeToJoyPalette.GoldenPollen * 0.4f, 0.15f, 10);
            }

            // Phase 2: additional chromatic particles (1-in-4)
            if (isPhase2 && Main.rand.NextBool(4))
            {
                Vector2 chromaPos = center + Main.rand.NextVector2Circular(55f, 55f);
                Color chromaCol = OdeToJoyPalette.GetBlossomGradient(Main.rand.NextFloat()) * 0.8f;
                var chromaParticle = new GenericGlowParticle(chromaPos,
                    Main.rand.NextVector2Circular(1.5f, 1.5f), chromaCol, 0.3f, 18, true);
                MagnumParticleHandler.SpawnParticle(chromaParticle);
            }

            // Music notes (1-in-15, 1-in-10 in phase 2)
            if (Main.rand.NextBool(isPhase2 ? 10 : 15))
            {
                OdeToJoyVFXLibrary.SpawnPetalMusicNotes(center, 1, 25f);
            }

            Lighting.AddLight(center, OdeToJoyPalette.VerdantGreen.ToVector3() * (isPhase2 ? 0.5f : 0.3f));
        }

        /// <summary>
        /// Orbiting petal ring around the Conductor — decorative rotating petal crown.
        /// </summary>
        public static void ConductorOrbitingPetals(Vector2 center, float rotation)
        {
            if (Main.dedServ) return;

            if (Main.rand.NextBool(8))
            {
                float angle = rotation + Main.rand.NextFloat(MathHelper.TwoPi);
                float radius = 55f + Main.rand.NextFloat(15f);
                Vector2 petalPos = center + angle.ToRotationVector2() * radius;
                Color col = Color.Lerp(OdeToJoyPalette.RosePink, OdeToJoyPalette.PetalPink, Main.rand.NextFloat());
                CustomParticles.GenericFlare(petalPos, col * 0.7f, 0.2f, 14);
            }

            // Pollen sparkle accent on the ring (1-in-12)
            if (Main.rand.NextBool(12))
            {
                float sparkleAngle = rotation * 1.3f + Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 sparklePos = center + sparkleAngle.ToRotationVector2() * 60f;
                CustomParticles.GenericFlare(sparklePos, OdeToJoyPalette.GoldenPollen * 0.5f, 0.12f, 8);
            }
        }

        // =====================================================================
        //  PHASE 1 ATTACK VFX
        // =====================================================================

        /// <summary>
        /// Petal Storm — sweeping petal arcs radiating outward.
        /// Particles fan out in curved arcs with rose-to-gold gradient.
        /// </summary>
        public static void PetalStormVFX(Vector2 position, float scale)
        {
            if (Main.dedServ) return;

            // Central flash
            CustomParticles.GenericFlare(position, OdeToJoyPalette.RosePink, 0.8f * scale, 18);
            CustomParticles.GenericFlare(position, OdeToJoyPalette.WhiteBloom, 0.5f * scale, 15);

            // Sweeping petal arcs — particles radiate in curved patterns
            for (int i = 0; i < 14; i++)
            {
                float angle = MathHelper.TwoPi * i / 14f;
                Vector2 vel = angle.ToRotationVector2() * (6f * scale) + Main.rand.NextVector2Circular(1f, 1f);
                Color col = OdeToJoyPalette.GetPetalGradient((float)i / 14f);
                var particle = new GenericGlowParticle(position, vel, col, 0.35f * scale, 22, true);
                MagnumParticleHandler.SpawnParticle(particle);
            }

            // Rose petal dust
            OdeToJoyVFXLibrary.SpawnRosePetals(position, (int)(6 * scale), 35f * scale);

            // Petal halo rings
            OdeToJoyVFXLibrary.SpawnPetalHaloRings(position, 3, 0.25f * scale);

            // Music note accent
            OdeToJoyVFXLibrary.SpawnPetalMusicNotes(position, 2, 20f * scale);

            Lighting.AddLight(position, OdeToJoyPalette.RosePink.ToVector3() * 0.8f * scale);
        }

        /// <summary>
        /// Vine Whip — vine lash trail between two points.
        /// Spawns leaf particles along the line with a green-gold gradient.
        /// </summary>
        public static void VineWhipVFX(Vector2 from, Vector2 to)
        {
            if (Main.dedServ) return;

            Vector2 direction = to - from;
            float distance = direction.Length();
            if (distance < 1f) return;
            Vector2 unit = direction / distance;

            // Vine trail dust along the lash path
            int segmentCount = (int)(distance / 16f);
            for (int i = 0; i < segmentCount; i++)
            {
                float progress = (float)i / segmentCount;
                Vector2 pos = Vector2.Lerp(from, to, progress);
                Vector2 perpendicular = new Vector2(-unit.Y, unit.X);
                pos += perpendicular * MathF.Sin(progress * MathHelper.TwoPi * 2f) * 8f;

                Color col = Color.Lerp(OdeToJoyPalette.LeafGreen, OdeToJoyPalette.VerdantGreen, progress);
                var particle = new GenericGlowParticle(pos,
                    perpendicular * Main.rand.NextFloat(-1f, 1f),
                    col * 0.7f, 0.2f, 15, true);
                MagnumParticleHandler.SpawnParticle(particle);
            }

            // Leaf dust along the vine
            for (int i = 0; i < 4; i++)
            {
                float t = Main.rand.NextFloat();
                Vector2 dustPos = Vector2.Lerp(from, to, t);
                OdeToJoyVFXLibrary.SpawnVineTrailDust(dustPos, unit * 2f);
            }

            // Impact flare at the tip
            CustomParticles.GenericFlare(to, OdeToJoyPalette.VerdantGreen, 0.5f, 12);

            Lighting.AddLight(to, OdeToJoyPalette.VerdantGreen.ToVector3() * 0.6f);
        }

        /// <summary>
        /// Rose Bud Volley — burst VFX at the launch origin.
        /// Concentric rose flares and scattered petal particles.
        /// </summary>
        public static void RoseBudVolleyVFX(Vector2 position)
        {
            if (Main.dedServ) return;

            // Central rose flash
            CustomParticles.GenericFlare(position, OdeToJoyPalette.PetalPink, 0.7f, 16);
            CustomParticles.GenericFlare(position, OdeToJoyPalette.RosePink, 0.5f, 14);

            // Radial bud scatter
            for (int i = 0; i < 10; i++)
            {
                float angle = MathHelper.TwoPi * i / 10f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 7f);
                Color col = Color.Lerp(OdeToJoyPalette.BudGreen, OdeToJoyPalette.RosePink, Main.rand.NextFloat());
                var particle = new GenericGlowParticle(position, vel, col, 0.25f, 18, true);
                MagnumParticleHandler.SpawnParticle(particle);
            }

            // Petal dust burst
            OdeToJoyVFXLibrary.SpawnRosePetals(position, 4, 25f);

            // Music note
            OdeToJoyVFXLibrary.SpawnMusicNotes(position, 1, 15f);

            Lighting.AddLight(position, OdeToJoyPalette.PetalPink.ToVector3() * 0.7f);
        }

        /// <summary>
        /// Pollen Cloud — golden pollen area denial VFX.
        /// Fills a circular area with floating golden particles.
        /// </summary>
        public static void PollenCloudVFX(Vector2 position, float radius)
        {
            if (Main.dedServ) return;

            // Golden pollen sparkles filling the area
            int particleCount = (int)(radius / 5f);
            for (int i = 0; i < particleCount; i++)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                float dist = Main.rand.NextFloat(radius);
                Vector2 spawnPos = position + angle.ToRotationVector2() * dist;
                Vector2 vel = new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), -Main.rand.NextFloat(0.3f, 0.8f));

                Color col = Color.Lerp(OdeToJoyPalette.GoldenPollen, OdeToJoyPalette.PollenGold, Main.rand.NextFloat());
                var particle = new GenericGlowParticle(spawnPos, vel, col * 0.6f, 0.2f, 25, true);
                MagnumParticleHandler.SpawnParticle(particle);
            }

            // Occasional central glow pulse
            if (Main.rand.NextBool(3))
            {
                CustomParticles.GenericFlare(position, OdeToJoyPalette.GoldenPollen * 0.4f,
                    0.3f + MathF.Sin((float)Main.timeForVisualEffects * 0.1f) * 0.1f, 8);
            }

            // Pollen dust
            OdeToJoyVFXLibrary.SpawnPollenSparkles(position, 3, radius * 0.6f);

            Lighting.AddLight(position, OdeToJoyPalette.GoldenPollen.ToVector3() * 0.4f);
        }

        /// <summary>
        /// Harmonic Bloom — radial flower burst expanding outward.
        /// Concentric rings of petals and golden pollen.
        /// </summary>
        public static void HarmonicBloomVFX(Vector2 position, float scale)
        {
            if (Main.dedServ) return;

            // Central sunlight flash
            CustomParticles.GenericFlare(position, OdeToJoyPalette.SunlightYellow, 1.0f * scale, 22);
            CustomParticles.GenericFlare(position, OdeToJoyPalette.WhiteBloom, 0.6f * scale, 18);

            // Radial petal rings — 3 concentric bursts
            for (int ring = 0; ring < 3; ring++)
            {
                int count = 8 + ring * 4;
                for (int i = 0; i < count; i++)
                {
                    float angle = MathHelper.TwoPi * i / count + ring * 0.2f;
                    float speed = (4f + ring * 3f) * scale;
                    Vector2 vel = angle.ToRotationVector2() * speed;
                    Color col = OdeToJoyPalette.GetGardenGradient((float)ring / 3f);
                    var particle = new GenericGlowParticle(position, vel, col, 0.3f * scale, 20 + ring * 5, true);
                    MagnumParticleHandler.SpawnParticle(particle);
                }
            }

            // Halo rings
            OdeToJoyVFXLibrary.SpawnGradientHaloRings(position, 4, 0.3f * scale);

            // Rose petals
            OdeToJoyVFXLibrary.SpawnRosePetals(position, (int)(5 * scale), 40f * scale);

            // Pollen sparkles
            OdeToJoyVFXLibrary.SpawnPollenSparkles(position, (int)(4 * scale), 30f * scale);

            // Music notes
            OdeToJoyVFXLibrary.SpawnMusicNotes(position, 3, 25f * scale);

            Lighting.AddLight(position, OdeToJoyPalette.SunlightYellow.ToVector3() * scale);
        }

        // =====================================================================
        //  PHASE 2 ATTACK VFX
        // =====================================================================

        /// <summary>
        /// Chromatic Cascade — intense multi-colored petal storm.
        /// A Phase 2 version of Petal Storm with full chromatic hue cycling.
        /// </summary>
        public static void ChromaticCascadeVFX(Vector2 position, float scale)
        {
            if (Main.dedServ) return;

            // Intense white-hot core flash
            CustomParticles.GenericFlare(position, OdeToJoyPalette.WhiteBloom, 1.2f * scale, 22);
            CustomParticles.GenericFlare(position, OdeToJoyPalette.GoldenPollen, 0.8f * scale, 18);

            // Chromatic petal storm — particles with full blossom gradient cycling
            for (int i = 0; i < 20; i++)
            {
                float angle = MathHelper.TwoPi * i / 20f;
                float speed = Main.rand.NextFloat(5f, 12f) * scale;
                Vector2 vel = angle.ToRotationVector2() * speed;
                Color col = OdeToJoyPalette.GetBlossomGradient((float)i / 20f);
                var particle = new GenericGlowParticle(position, vel, col, 0.4f * scale, 25, true);
                MagnumParticleHandler.SpawnParticle(particle);
            }

            // Secondary wave — smaller, faster, offset rotation
            for (int i = 0; i < 12; i++)
            {
                float angle = MathHelper.TwoPi * i / 12f + 0.15f;
                Vector2 vel = angle.ToRotationVector2() * (8f * scale);
                Color col = OdeToJoyPalette.GetPetalGradient((float)i / 12f);
                var particle = new GenericGlowParticle(position, vel, col * 0.8f, 0.3f * scale, 20, true);
                MagnumParticleHandler.SpawnParticle(particle);
            }

            // Cascading halo rings
            OdeToJoyVFXLibrary.SpawnGradientHaloRings(position, 5, 0.35f * scale);
            OdeToJoyVFXLibrary.SpawnPetalHaloRings(position, 4, 0.3f * scale);

            // Rose petals and pollen
            OdeToJoyVFXLibrary.SpawnRosePetals(position, (int)(8 * scale), 45f * scale);
            OdeToJoyVFXLibrary.SpawnPollenSparkles(position, (int)(6 * scale), 35f * scale);

            // Music note burst
            OdeToJoyVFXLibrary.SpawnMusicNotes(position, 4, 30f * scale);
            OdeToJoyVFXLibrary.SpawnPetalMusicNotes(position, 3, 25f * scale);

            Lighting.AddLight(position, OdeToJoyPalette.WhiteBloom.ToVector3() * 1.2f * scale);
        }

        /// <summary>
        /// Thorny Embrace — converging vine cage closing inward.
        /// Particles spiral inward from the radius toward the center.
        /// </summary>
        public static void ThornyEmbraceVFX(Vector2 position, float radius)
        {
            if (Main.dedServ) return;

            // Converging vine particles from outer radius toward center
            int vineCount = (int)(radius / 8f);
            for (int i = 0; i < vineCount; i++)
            {
                float angle = MathHelper.TwoPi * i / vineCount;
                float dist = radius * Main.rand.NextFloat(0.6f, 1f);
                Vector2 spawnPos = position + angle.ToRotationVector2() * dist;
                Vector2 vel = (position - spawnPos).SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(3f, 7f);

                Color col = Color.Lerp(OdeToJoyPalette.LeafGreen, OdeToJoyPalette.MossShadow, Main.rand.NextFloat());
                var particle = new GenericGlowParticle(spawnPos, vel, col * 0.7f, 0.25f, 20, true);
                MagnumParticleHandler.SpawnParticle(particle);
            }

            // Thorn accent flares along the cage edge
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f + (float)Main.timeForVisualEffects * 0.03f;
                Vector2 thornPos = position + angle.ToRotationVector2() * radius;
                CustomParticles.GenericFlare(thornPos, OdeToJoyPalette.DeepForest, 0.3f, 10);
            }

            // Vine trail dust at convergence points
            for (int i = 0; i < 3; i++)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 dustPos = position + angle.ToRotationVector2() * (radius * 0.4f);
                OdeToJoyVFXLibrary.SpawnVineTrailDust(dustPos, (position - dustPos).SafeNormalize(Vector2.Zero) * 2f);
            }

            Lighting.AddLight(position, OdeToJoyPalette.VerdantGreen.ToVector3() * 0.5f);
        }

        /// <summary>
        /// Garden Symphony — safe-arc radial burst with musical accents.
        /// Particles burst in defined arcs leaving deliberate safe gaps.
        /// </summary>
        public static void GardenSymphonyVFX(Vector2 position, float scale)
        {
            if (Main.dedServ) return;

            // Central golden flash
            CustomParticles.GenericFlare(position, OdeToJoyPalette.GoldenPollen, 1.0f * scale, 20);
            CustomParticles.GenericFlare(position, OdeToJoyPalette.SunlightYellow, 0.7f * scale, 16);

            // Radial arc burst — 5 arcs with gaps between them
            int arcCount = 5;
            float arcWidth = MathHelper.TwoPi / arcCount * 0.6f;
            for (int arc = 0; arc < arcCount; arc++)
            {
                float arcCenter = MathHelper.TwoPi * arc / arcCount;
                int particlesPerArc = 6;
                for (int i = 0; i < particlesPerArc; i++)
                {
                    float angleOffset = (i - particlesPerArc * 0.5f) / particlesPerArc * arcWidth;
                    float angle = arcCenter + angleOffset;
                    float speed = Main.rand.NextFloat(6f, 10f) * scale;
                    Vector2 vel = angle.ToRotationVector2() * speed;
                    Color col = OdeToJoyPalette.GetGardenGradient((float)arc / arcCount);
                    var particle = new GenericGlowParticle(position, vel, col, 0.35f * scale, 22, true);
                    MagnumParticleHandler.SpawnParticle(particle);
                }
            }

            // Garden aura at center
            OdeToJoyVFXLibrary.SpawnGardenAura(position, 30f * scale);

            // Gradient halo rings
            OdeToJoyVFXLibrary.SpawnGradientHaloRings(position, 4, 0.3f * scale);

            // Music notes — a structured musical burst
            OdeToJoyVFXLibrary.SpawnMusicNotes(position, 5, 30f * scale);

            Lighting.AddLight(position, OdeToJoyPalette.GoldenPollen.ToVector3() * scale);
        }

        /// <summary>
        /// Eternal Bloom — massive floral explosion.
        /// Concentric rings of garden particles with intense bloom flash.
        /// </summary>
        public static void EternalBloomVFX(Vector2 position, float scale)
        {
            if (Main.dedServ) return;

            // Massive white-golden core flash
            CustomParticles.GenericFlare(position, OdeToJoyPalette.WhiteBloom, 1.8f * scale, 28);
            CustomParticles.GenericFlare(position, OdeToJoyPalette.GoldenPollen, 1.3f * scale, 24);
            CustomParticles.GenericFlare(position, OdeToJoyPalette.RosePink, 1.0f * scale, 20);

            // 5 concentric rings of garden particles
            for (int ring = 0; ring < 5; ring++)
            {
                float progress = (float)ring / 5f;
                int count = 10 + ring * 3;
                for (int i = 0; i < count; i++)
                {
                    float angle = MathHelper.TwoPi * i / count + ring * 0.15f;
                    float speed = (5f + ring * 3f) * scale;
                    Vector2 vel = angle.ToRotationVector2() * speed;
                    Color col = OdeToJoyPalette.GetGardenGradient(progress);
                    var particle = new GenericGlowParticle(position, vel, col, 0.4f * scale, 25 + ring * 3, true);
                    MagnumParticleHandler.SpawnParticle(particle);
                }
            }

            // Halo cascade
            OdeToJoyVFXLibrary.SpawnGradientHaloRings(position, 6, 0.4f * scale);
            OdeToJoyVFXLibrary.SpawnPetalHaloRings(position, 5, 0.35f * scale);

            // Rose petals and pollen
            OdeToJoyVFXLibrary.SpawnRosePetals(position, (int)(10 * scale), 55f * scale);
            OdeToJoyVFXLibrary.SpawnPollenSparkles(position, (int)(8 * scale), 40f * scale);

            // Music note burst
            OdeToJoyVFXLibrary.MusicNoteBurst(position, OdeToJoyPalette.GoldenPollen, (int)(8 * scale), 6f * scale);

            Lighting.AddLight(position, OdeToJoyPalette.WhiteBloom.ToVector3() * 1.5f * scale);
        }

        /// <summary>
        /// Jubilant Finale — ultimate attack VFX.
        /// The Ode to Joy signature: a triumphant eruption of petals, pollen,
        /// golden light, and musical celebration.
        /// </summary>
        public static void JubilantFinaleVFX(Vector2 position, float scale)
        {
            if (Main.dedServ) return;

            // Blinding core flash — layered white → gold → rose → green
            CustomParticles.GenericFlare(position, OdeToJoyPalette.WhiteBloom, 2.2f * scale, 32);
            CustomParticles.GenericFlare(position, OdeToJoyPalette.GoldenPollen, 1.6f * scale, 28);
            CustomParticles.GenericFlare(position, OdeToJoyPalette.RosePink, 1.2f * scale, 24);
            CustomParticles.GenericFlare(position, OdeToJoyPalette.VerdantGreen, 0.9f * scale, 20);

            // Massive radial garden storm — 30 particles in full blossom gradient
            for (int i = 0; i < 30; i++)
            {
                float angle = MathHelper.TwoPi * i / 30f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(8f, 18f) * scale;
                Color col = OdeToJoyPalette.GetBlossomGradient((float)i / 30f);
                var particle = new GenericGlowParticle(position, vel, col, 0.5f * scale, 30, true);
                MagnumParticleHandler.SpawnParticle(particle);
            }

            // Secondary petal wave — upward-biased for jubilant lift
            for (int i = 0; i < 16; i++)
            {
                float angle = MathHelper.TwoPi * i / 16f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(5f, 10f) * scale;
                vel.Y -= 3f * scale; // Upward bias — joy rises
                Color col = OdeToJoyPalette.GetPetalGradient((float)i / 16f);
                var particle = new GenericGlowParticle(position, vel, col * 0.8f, 0.35f * scale, 25, true);
                MagnumParticleHandler.SpawnParticle(particle);
            }

            // Massive halo cascade
            OdeToJoyVFXLibrary.SpawnGradientHaloRings(position, 8, 0.5f * scale);
            OdeToJoyVFXLibrary.SpawnPetalHaloRings(position, 6, 0.4f * scale);

            // Rose petals and pollen explosion
            OdeToJoyVFXLibrary.SpawnRosePetals(position, (int)(14 * scale), 70f * scale);
            OdeToJoyVFXLibrary.SpawnPollenSparkles(position, (int)(10 * scale), 50f * scale);

            // Music note cascade — large jubilant burst
            OdeToJoyVFXLibrary.MusicNoteBurst(position, OdeToJoyPalette.GoldenPollen, (int)(12 * scale), 7f * scale);
            OdeToJoyVFXLibrary.SpawnPetalMusicNotes(position, 4, 40f * scale);

            // Garden aura
            OdeToJoyVFXLibrary.SpawnGardenAura(position, 50f * scale);

            Lighting.AddLight(position, OdeToJoyPalette.WhiteBloom.ToVector3() * 2.0f * scale);
        }

        // =====================================================================
        //  PHASE TRANSITION VFX
        // =====================================================================

        /// <summary>
        /// Phase transition — petal shedding and energy surging as the Conductor
        /// breaks from Phase 1 into Phase 2. Called per-frame with progress 0→1.
        /// Petals shed outward, vine energy converges, golden light intensifies.
        /// </summary>
        public static void PhaseTransitionVFX(Vector2 position, float progress)
        {
            if (Main.dedServ) return;

            // Shedding petals — increase count as progress advances
            int petalCount = (int)(4 + progress * 12);
            for (int i = 0; i < petalCount; i++)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                float speed = Main.rand.NextFloat(3f, 8f) * (0.5f + progress * 0.5f);
                Vector2 vel = angle.ToRotationVector2() * speed;
                Color col = OdeToJoyPalette.GetPetalGradient(Main.rand.NextFloat());
                var particle = new GenericGlowParticle(position, vel, col * (0.5f + progress * 0.5f),
                    0.3f, 20, true);
                MagnumParticleHandler.SpawnParticle(particle);
            }

            // Converging vine energy from the edges
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f;
                float radius = 180f * (1f - progress * 0.5f);
                Vector2 convergePos = position + angle.ToRotationVector2() * radius;
                Vector2 vel = (position - convergePos).SafeNormalize(Vector2.Zero) * (4f + progress * 8f);
                Color col = Color.Lerp(OdeToJoyPalette.VerdantGreen, OdeToJoyPalette.GoldenPollen, progress);
                var particle = new GenericGlowParticle(convergePos, vel, col, 0.3f + progress * 0.2f, 18, true);
                MagnumParticleHandler.SpawnParticle(particle);
            }

            // Intensifying central glow
            float glowScale = 0.4f + progress * 0.8f;
            CustomParticles.GenericFlare(position,
                Color.Lerp(OdeToJoyPalette.RosePink, OdeToJoyPalette.GoldenPollen, progress),
                glowScale, 8);

            // Pollen sparkles increase with progress
            if (Main.rand.NextBool(Math.Max(1, 6 - (int)(progress * 5))))
                OdeToJoyVFXLibrary.SpawnPollenSparkles(position, 2, 40f);

            Lighting.AddLight(position, OdeToJoyPalette.GoldenPollen.ToVector3() * (0.5f + progress * 1.0f));
        }

        /// <summary>
        /// Phase 2 Awakening — chromatic energy explosion as the Conductor
        /// ascends into the Garden of Eternal Roses form.
        /// Called per-frame with progress 0→1 during the awakening animation.
        /// </summary>
        public static void Phase2AwakeningVFX(Vector2 position, float progress)
        {
            if (Main.dedServ) return;

            // Spiraling chromatic energy
            int spiralCount = 3;
            float time = (float)Main.timeForVisualEffects;
            for (int arm = 0; arm < spiralCount; arm++)
            {
                float baseAngle = time * 0.1f + MathHelper.TwoPi * arm / spiralCount;
                for (int point = 0; point < 6; point++)
                {
                    float spiralAngle = baseAngle + point * 0.35f;
                    float spiralRadius = 25f + point * 18f;
                    Vector2 spiralPos = position + spiralAngle.ToRotationVector2() * spiralRadius;
                    Color col = OdeToJoyPalette.GetBlossomGradient((float)point / 6f + progress);
                    CustomParticles.GenericFlare(spiralPos, col, 0.4f + progress * 0.2f, 12);
                }
            }

            // Radial burst intensifying with progress
            if (progress > 0.3f)
            {
                int burstCount = (int)(6 + progress * 12);
                for (int i = 0; i < burstCount; i++)
                {
                    Vector2 vel = Main.rand.NextVector2Unit() * Main.rand.NextFloat(4f, 10f) * progress;
                    Color col = OdeToJoyPalette.GetGardenGradient(Main.rand.NextFloat());
                    var particle = new GenericGlowParticle(position, vel, col * 0.8f, 0.35f, 22, true);
                    MagnumParticleHandler.SpawnParticle(particle);
                }
            }

            // Petal and pollen storm at high progress
            if (progress > 0.5f)
            {
                OdeToJoyVFXLibrary.SpawnRosePetals(position, (int)(progress * 6), 50f * progress);
                OdeToJoyVFXLibrary.SpawnPollenSparkles(position, (int)(progress * 4), 35f);
            }

            // Music note cascade at high progress
            if (progress > 0.7f && Main.rand.NextBool(3))
            {
                OdeToJoyVFXLibrary.SpawnMusicNotes(position, 2, 30f);
                OdeToJoyVFXLibrary.SpawnPetalMusicNotes(position, 1, 25f);
            }

            // Central glow building
            float coreScale = 0.5f + progress * 1.5f;
            CustomParticles.GenericFlare(position, OdeToJoyPalette.WhiteBloom * progress, coreScale, 6);

            Lighting.AddLight(position, OdeToJoyPalette.WhiteBloom.ToVector3() * (0.5f + progress * 1.5f));
        }

        // =====================================================================
        //  BOSS EVENTS — SPAWN, HIT, DEATH
        // =====================================================================

        /// <summary>
        /// Conductor spawn burst — the initial appearance of the boss.
        /// A garden erupts: radial petals, pollen explosion, music note cascade.
        /// </summary>
        public static void ConductorSpawnVFX(Vector2 position)
        {
            if (Main.dedServ) return;

            // White-hot central flash
            CustomParticles.GenericFlare(position, OdeToJoyPalette.WhiteBloom, 1.8f, 28);
            CustomParticles.GenericFlare(position, OdeToJoyPalette.GoldenPollen, 1.3f, 24);
            CustomParticles.GenericFlare(position, OdeToJoyPalette.RosePink, 0.9f, 20);

            // Radial garden burst — full blossom gradient
            for (int i = 0; i < 20; i++)
            {
                float angle = MathHelper.TwoPi * i / 20f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(6f, 14f);
                Color col = OdeToJoyPalette.GetBlossomGradient((float)i / 20f);
                var particle = new GenericGlowParticle(position, vel, col, 0.45f, 28, true);
                MagnumParticleHandler.SpawnParticle(particle);
            }

            // Halo cascade
            OdeToJoyVFXLibrary.SpawnGradientHaloRings(position, 6, 0.35f);
            OdeToJoyVFXLibrary.SpawnPetalHaloRings(position, 4, 0.3f);

            // Rose petals erupting
            OdeToJoyVFXLibrary.SpawnRosePetals(position, 8, 50f);

            // Pollen sparkle explosion
            OdeToJoyVFXLibrary.SpawnPollenSparkles(position, 10, 40f);

            // Garden aura establishing
            OdeToJoyVFXLibrary.SpawnGardenAura(position, 45f);

            // Music note fanfare
            OdeToJoyVFXLibrary.MusicNoteBurst(position, OdeToJoyPalette.GoldenPollen, 8, 5f);

            // Garden impact
            OdeToJoyVFXLibrary.GardenImpact(position, 1.2f);

            Lighting.AddLight(position, OdeToJoyPalette.WhiteBloom.ToVector3() * 1.8f);
        }

        /// <summary>
        /// Conductor on-hit reaction — quick floral flash when the boss takes damage.
        /// </summary>
        public static void ConductorHitVFX(Vector2 position)
        {
            if (Main.dedServ) return;

            // Quick petal flash
            CustomParticles.GenericFlare(position, OdeToJoyPalette.RosePink, 0.5f, 10);
            CustomParticles.GenericFlare(position, OdeToJoyPalette.WhiteBloom, 0.3f, 8);

            // Scattered petals
            for (int i = 0; i < 5; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(4f, 4f);
                Color col = Color.Lerp(OdeToJoyPalette.RosePink, OdeToJoyPalette.PetalPink, Main.rand.NextFloat());
                var particle = new GenericGlowParticle(position, vel, col * 0.6f, 0.2f, 12, true);
                MagnumParticleHandler.SpawnParticle(particle);
            }

            // Single halo ring
            CustomParticles.HaloRing(position, OdeToJoyPalette.RosePink, 0.25f, 10);

            // Petal dust
            OdeToJoyVFXLibrary.SpawnRosePetals(position, 2, 15f);

            Lighting.AddLight(position, OdeToJoyPalette.RosePink.ToVector3() * 0.5f);
        }

        /// <summary>
        /// Conductor death animation — progressive VFX called per-frame
        /// as progress goes from 0 to 1 during the death sequence.
        /// Garden wilts, energy disperses, petals shed.
        /// </summary>
        public static void ConductorDeathVFX(Vector2 position, float progress)
        {
            if (Main.dedServ) return;

            // Flickering energy — unstable flares
            if (Main.rand.NextBool(Math.Max(1, 4 - (int)(progress * 3))))
            {
                Vector2 flickerPos = position + Main.rand.NextVector2Circular(30f + progress * 20f, 30f + progress * 20f);
                Color flickerCol = OdeToJoyPalette.GetGardenGradient(Main.rand.NextFloat());
                CustomParticles.GenericFlare(flickerPos, flickerCol, 0.3f + progress * 0.3f, 8);
            }

            // Shedding petals — accelerating with progress
            int shedCount = (int)(2 + progress * 10);
            for (int i = 0; i < shedCount; i++)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                float speed = Main.rand.NextFloat(2f, 6f) * (0.5f + progress);
                Vector2 vel = angle.ToRotationVector2() * speed;
                vel.Y += 1f; // Slight downward — wilting
                Color col = OdeToJoyPalette.GetPetalGradient(Main.rand.NextFloat());
                col *= (0.4f + progress * 0.6f);
                var particle = new GenericGlowParticle(position, vel, col, 0.25f, 18, true);
                MagnumParticleHandler.SpawnParticle(particle);
            }

            // Converging energy particles at high progress
            if (progress > 0.4f)
            {
                for (int i = 0; i < 4; i++)
                {
                    float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                    float radius = 120f * (1f - progress * 0.5f);
                    Vector2 convergePos = position + angle.ToRotationVector2() * radius;
                    Vector2 vel = (position - convergePos).SafeNormalize(Vector2.Zero) * (3f + progress * 6f);
                    Color col = Color.Lerp(OdeToJoyPalette.VerdantGreen, OdeToJoyPalette.GoldenPollen, progress);
                    var particle = new GenericGlowParticle(convergePos, vel, col, 0.3f, 15, true);
                    MagnumParticleHandler.SpawnParticle(particle);
                }
            }

            // Building core glow
            float coreGlow = 0.3f + progress * 1.2f;
            CustomParticles.GenericFlare(position,
                Color.Lerp(OdeToJoyPalette.RosePink, OdeToJoyPalette.WhiteBloom, progress),
                coreGlow, 6);

            // Music notes fading away (early progress only)
            if (progress < 0.6f && Main.rand.NextBool(5))
            {
                OdeToJoyVFXLibrary.SpawnMusicNotes(position, 1, 20f);
            }

            Lighting.AddLight(position, OdeToJoyPalette.GoldenPollen.ToVector3() * (0.5f + progress * 1.0f));
        }

        /// <summary>
        /// Conductor final death explosion — the ultimate climactic burst
        /// when the boss is defeated. Massive garden detonation.
        /// </summary>
        public static void ConductorFinalDeathExplosion(Vector2 position)
        {
            if (Main.dedServ) return;

            // Blinding layered flash — white → gold → rose → green → deep forest
            CustomParticles.GenericFlare(position, OdeToJoyPalette.WhiteBloom, 3.0f, 40);
            CustomParticles.GenericFlare(position, OdeToJoyPalette.SunlightYellow, 2.4f, 36);
            CustomParticles.GenericFlare(position, OdeToJoyPalette.GoldenPollen, 1.8f, 32);
            CustomParticles.GenericFlare(position, OdeToJoyPalette.RosePink, 1.4f, 28);
            CustomParticles.GenericFlare(position, OdeToJoyPalette.VerdantGreen, 1.0f, 24);

            // Massive radial garden storm — 40 particles with full blossom gradient
            for (int i = 0; i < 40; i++)
            {
                float angle = MathHelper.TwoPi * i / 40f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(10f, 22f);
                Color col = OdeToJoyPalette.GetBlossomGradient((float)i / 40f);
                var particle = new GenericGlowParticle(position, vel, col, 0.55f, 35, true);
                MagnumParticleHandler.SpawnParticle(particle);
            }

            // Secondary upward-biased petal storm — joy ascending
            for (int i = 0; i < 20; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(12f, 12f);
                vel.Y -= 4f; // Upward bias
                Color col = OdeToJoyPalette.GetPetalGradient(Main.rand.NextFloat());
                var particle = new GenericGlowParticle(position, vel, col * 0.8f, 0.4f, 30, true);
                MagnumParticleHandler.SpawnParticle(particle);
            }

            // Massive halo cascade
            OdeToJoyVFXLibrary.SpawnGradientHaloRings(position, 10, 0.5f);
            OdeToJoyVFXLibrary.SpawnPetalHaloRings(position, 8, 0.4f);

            // Rose petals erupting everywhere
            OdeToJoyVFXLibrary.SpawnRosePetals(position, 16, 80f);

            // Golden pollen explosion
            OdeToJoyVFXLibrary.SpawnPollenSparkles(position, 14, 60f);

            // Music note grand finale burst
            OdeToJoyVFXLibrary.MusicNoteBurst(position, OdeToJoyPalette.GoldenPollen, 14, 8f);
            OdeToJoyVFXLibrary.SpawnPetalMusicNotes(position, 6, 50f);

            // Triumphant celebration — the signature Ode to Joy effect
            OdeToJoyVFXLibrary.TriumphantCelebration(position, 1.5f);

            // Death garden flash
            OdeToJoyVFXLibrary.DeathGardenFlash(position, 1.5f);

            Lighting.AddLight(position, OdeToJoyPalette.WhiteBloom.ToVector3() * 2.5f);
        }

        // =====================================================================
        //  PREDRAW HELPERS
        // =====================================================================

        /// <summary>
        /// Get afterimage trail color for Conductor PreDraw afterimages.
        /// Phase 2 uses chromatic cycling; Phase 1 uses petal gradient.
        /// </summary>
        public static Color ConductorAfterimageColor(float progress, bool isPhase2)
        {
            Color col;
            if (isPhase2)
                col = OdeToJoyPalette.GetBlossomGradient(progress) * ((1f - progress) * 0.45f);
            else
                col = OdeToJoyPalette.GetPetalGradient(progress) * ((1f - progress) * 0.35f);
            return col with { A = 0 };
        }

        /// <summary>
        /// Get additive glow underlay color for Conductor PreDraw.
        /// </summary>
        public static Color ConductorGlowColor(bool isPhase2)
        {
            Color glowColor = isPhase2
                ? OdeToJoyPalette.GoldenPollen * 0.35f
                : OdeToJoyPalette.RosePink * 0.25f;
            return glowColor with { A = 0 };
        }
    }
}
