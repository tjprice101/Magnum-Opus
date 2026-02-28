/*  COMMENTED OUT — replaced by self-contained SymphonysEnd/ folder VFX
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Content.Fate.ResonantWeapons
{
    /// <summary>
    /// VFX helper for Symphony's End — magic rapid-fire, 500 dmg.
    /// Spawns spectral blades that spiral toward cursor and explode.
    /// Palette: FatePalette.SymphonyEnd
    /// (CosmicVoid -> DarkPink -> BrightCrimson -> StellarCore -> WhiteCelestial -> SupernovaWhite)
    /// </summary>
    public static class SymphonysEndVFX
    {
        // ===== HOLD ITEM VFX =====

        /// <summary>
        /// 5-layer hold effect: (1) blade echoes orbiting, (2) glyph storm mandala,
        /// (3) inner star ring counter-rotating, (4) cosmic dust drifting upward,
        /// (5) music notes rising. Color-shifting aura light.
        /// </summary>
        public static void HoldItemVFX(Player player)
        {
            if (Main.dedServ) return;

            Vector2 center = player.MountedCenter;
            float time = (float)Main.timeForVisualEffects;

            // Layer 1: Blade echoes orbiting — 3 blades at 120 degrees with radius modulation
            float bladeOrbitRadius = 40f + MathF.Sin(time * 0.05f) * 8f;
            for (int i = 0; i < 3; i++)
            {
                float bladeAngle = time * 0.03f + MathHelper.TwoPi * i / 3f;
                Vector2 bladePos = center + bladeAngle.ToRotationVector2() * bladeOrbitRadius;

                if (Main.GameUpdateCount % 3 == 0)
                {
                    Color bladeCol = FatePalette.PaletteLerp(FatePalette.SymphonyEnd, (float)i / 3f + 0.2f);
                    var blade = new GenericGlowParticle(bladePos,
                        bladeAngle.ToRotationVector2().RotatedBy(MathHelper.PiOver2) * 0.8f,
                        bladeCol * 0.5f, 0.2f, 10, true);
                    MagnumParticleHandler.SpawnParticle(blade);
                }
            }

            // Layer 2: Glyph storm pattern — 6-point mandala (1-in-10)
            if (Main.rand.NextBool(10))
            {
                for (int i = 0; i < 6; i++)
                {
                    float mandalaAngle = time * 0.02f + MathHelper.TwoPi * i / 6f;
                    Vector2 glyphPos = center + mandalaAngle.ToRotationVector2() * 35f;
                    Color glyphCol = FatePalette.PaletteLerp(FatePalette.SymphonyEnd, (float)i / 6f);
                    CustomParticles.Glyph(glyphPos, glyphCol * 0.6f, 0.18f, -1);
                }
            }

            // Layer 3: Inner star ring counter-rotating (every 8 frames)
            if (Main.GameUpdateCount % 8 == 0)
            {
                float innerAngle = -time * 0.04f;
                for (int i = 0; i < 4; i++)
                {
                    float starAngle = innerAngle + MathHelper.TwoPi * i / 4f;
                    Vector2 starPos = center + starAngle.ToRotationVector2() * 22f;
                    Color starCol = Main.rand.NextBool(2) ? FatePalette.StellarCore : FatePalette.WhiteCelestial;
                    var star = new GenericGlowParticle(starPos,
                        starAngle.ToRotationVector2().RotatedBy(-MathHelper.PiOver2) * 0.5f,
                        starCol * 0.5f, 0.14f, 12, true);
                    MagnumParticleHandler.SpawnParticle(star);
                }
            }

            // Layer 4: Cosmic dust drifting upward (1-in-5)
            if (Main.rand.NextBool(5))
            {
                Vector2 dustPos = center + Main.rand.NextVector2Circular(30f, 15f) + new Vector2(0, 10f);
                Color dustCol = Color.Lerp(FatePalette.DarkPink, FatePalette.BrightCrimson, Main.rand.NextFloat()) * 0.4f;
                var dust = new GenericGlowParticle(dustPos, new Vector2(Main.rand.NextFloat(-0.3f, 0.3f), -0.8f),
                    dustCol, 0.15f, 20, true);
                MagnumParticleHandler.SpawnParticle(dust);
            }

            // Layer 5: Music notes rising (1-in-12, 0.75f+ scale)
            if (Main.rand.NextBool(12))
                FateVFXLibrary.SpawnMusicNotes(center + new Vector2(0, -15f), 1, 20f, 0.75f, 1.0f, 28);

            // Color-shifting aura light
            float colorShift = MathF.Sin(time * 0.04f) * 0.5f + 0.5f;
            Color auraCol = Color.Lerp(FatePalette.DarkPink, FatePalette.StellarCore, colorShift);
            float auraIntensity = 0.3f + MathF.Sin(time * 0.06f) * 0.1f;
            Lighting.AddLight(center, auraCol.ToVector3() * auraIntensity);
        }

        // ===== PREDRAW IN WORLD BLOOM =====

        /// <summary>
        /// 3-layer additive bloom: outer DarkPink, mid FatePurple,
        /// inner WhiteCelestial-hot core. Manages SpriteBatch state transitions.
        /// </summary>
        public static void PreDrawInWorldBloom(
            SpriteBatch sb, Texture2D tex, Vector2 pos,
            Vector2 origin, float rotation, float scale)
        {
            float time = (float)Main.timeForVisualEffects;
            float pulse = 1f + MathF.Sin(time * 0.04f) * 0.05f;

            // Layer 1: Outer DarkPink aura
            sb.Draw(tex, pos, null,
                FatePalette.Additive(FatePalette.DarkPink, 0.35f),
                rotation, origin, scale * 1.10f * pulse, SpriteEffects.None, 0f);

            // Layer 2: Mid FatePurple glow
            sb.Draw(tex, pos, null,
                FatePalette.Additive(FatePalette.FatePurple, 0.28f),
                rotation, origin, scale * 1.05f * pulse, SpriteEffects.None, 0f);

            // Layer 3: Inner WhiteCelestial hot core
            sb.Draw(tex, pos, null,
                FatePalette.Additive(FatePalette.WhiteCelestial, 0.20f),
                rotation, origin, scale * 1.01f * pulse, SpriteEffects.None, 0f);
        }

        // ===== BLADE SPAWN VFX =====

        /// <summary>
        /// One-shot VFX when a spectral blade projectile spawns and begins spiraling.
        /// Glyph burst, flare, halo ring, 4 cosmic sparks, music note, spawn flash light.
        /// </summary>
        public static void BladeSpawnVFX(Vector2 spawnPos, Vector2 direction)
        {
            if (Main.dedServ) return;

            // Glyph burst at spawn — 4 glyphs
            for (int i = 0; i < 4; i++)
            {
                float angle = MathHelper.TwoPi * i / 4f;
                Vector2 glyphPos = spawnPos + angle.ToRotationVector2() * 15f;
                Color glyphCol = FatePalette.PaletteLerp(FatePalette.SymphonyEnd, (float)i / 4f + 0.2f);
                CustomParticles.Glyph(glyphPos, glyphCol, 0.25f, -1);
            }

            // Central flare
            CustomParticles.GenericFlare(spawnPos, FatePalette.StellarCore, 0.5f, 14);

            // Halo ring at spawn
            CustomParticles.HaloRing(spawnPos, FatePalette.BrightCrimson, 0.3f, 12);

            // 4 cosmic sparks in the direction of travel
            for (int i = 0; i < 4; i++)
            {
                float spreadAngle = (i - 1.5f) * 0.3f;
                Vector2 sparkVel = direction.RotatedBy(spreadAngle) * Main.rand.NextFloat(3f, 5f);
                Color sparkCol = FatePalette.GetCosmicGradient(Main.rand.NextFloat());
                var spark = new GlowSparkParticle(spawnPos, sparkVel, sparkCol, 0.22f, 10);
                MagnumParticleHandler.SpawnParticle(spark);
            }

            // Music note at spawn (1-in-2)
            if (Main.rand.NextBool(2))
                FateVFXLibrary.SpawnMusicNotes(spawnPos, 1, 10f, 0.8f, 1.0f, 25);

            // Spawn flash light
            Lighting.AddLight(spawnPos, FatePalette.StellarCore.ToVector3() * 0.6f);
        }

        // ===== BLADE TRAIL VFX =====

        /// <summary>
        /// Per-frame trail VFX for spiraling spectral blade projectiles.
        /// SymphonyEnd palette trail particles, spiral sparks, occasional glyph accent.
        /// </summary>
        public static void BladeTrailVFX(Vector2 pos, Vector2 velocity)
        {
            if (Main.dedServ) return;

            Vector2 trailDir = -velocity.SafeNormalize(Vector2.Zero);

            // Primary trail particles using SymphonyEnd palette
            Color trailCol = FatePalette.PaletteLerp(FatePalette.SymphonyEnd, Main.rand.NextFloat(0.2f, 0.8f));
            var glow = new GenericGlowParticle(pos + Main.rand.NextVector2Circular(4f, 4f),
                trailDir * 1.0f + Main.rand.NextVector2Circular(0.6f, 0.6f),
                trailCol * 0.7f, 0.2f, 14, true);
            MagnumParticleHandler.SpawnParticle(glow);

            // Spiral sparks — rotating outward from the blade path (1-in-2)
            if (Main.rand.NextBool(2))
            {
                float spiralAngle = (float)Main.timeForVisualEffects * 0.15f;
                Vector2 spiralVel = spiralAngle.ToRotationVector2() * 1.5f + trailDir * 0.5f;
                Color spiralCol = Color.Lerp(FatePalette.BrightCrimson, FatePalette.StellarCore, Main.rand.NextFloat());
                var spiral = new GlowSparkParticle(pos, spiralVel, spiralCol * 0.8f, 0.15f, 10);
                MagnumParticleHandler.SpawnParticle(spiral);
            }

            // Occasional glyph accent (1-in-8)
            if (Main.rand.NextBool(8))
            {
                Color glyphCol = FatePalette.PaletteLerp(FatePalette.SymphonyEnd, Main.rand.NextFloat());
                CustomParticles.Glyph(pos + Main.rand.NextVector2Circular(8f, 8f), glyphCol * 0.6f, 0.18f, -1);
            }

            // Pink torch dust for density
            if (Main.rand.NextBool(3))
            {
                Dust d = Dust.NewDustPerfect(pos, DustID.PinkTorch,
                    trailDir * Main.rand.NextFloat(0.5f, 2f), 0,
                    FatePalette.DarkPink, 1.1f);
                d.noGravity = true;
            }

            Lighting.AddLight(pos, FatePalette.BrightCrimson.ToVector3() * 0.3f);
        }

        // ===== BLADE EXPLOSION VFX =====

        /// <summary>
        /// Explosion VFX when a spiraling spectral blade reaches the cursor and detonates.
        /// FateVFXLibrary.ProjectileImpact at 0.8f, additional halo rings,
        /// glyph burst, and music note explosion.
        /// </summary>
        public static void BladeExplosionVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            // Shared projectile impact at high intensity
            FateVFXLibrary.ProjectileImpact(pos, 0.8f);

            // Additional expanding halo rings in SymphonyEnd gradient
            for (int i = 0; i < 4; i++)
            {
                float progress = (float)i / 4f;
                Color ringCol = FatePalette.PaletteLerp(FatePalette.SymphonyEnd, progress + 0.2f);
                CustomParticles.HaloRing(pos, ringCol, 0.3f + i * 0.1f, 14 + i * 2);
            }

            // Glyph burst — 8 glyphs exploding outward
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 glyphPos = pos + angle.ToRotationVector2() * 20f;
                Color glyphCol = FatePalette.PaletteLerp(FatePalette.SymphonyEnd, (float)i / 8f);
                CustomParticles.Glyph(glyphPos, glyphCol, 0.28f, -1);
            }

            // Music note explosion — the symphony's final chord
            FateVFXLibrary.SpawnMusicNotes(pos, 5, 30f, 0.8f, 1.1f, 30);

            // Star sparkles scattered across the explosion
            FateVFXLibrary.SpawnStarSparkles(pos, 6, 35f, 0.25f);

            // Cosmic cloud burst
            FateVFXLibrary.SpawnCosmicCloudBurst(pos, 0.6f, 12);

            // Radial dust explosion
            FateVFXLibrary.SpawnRadialDustBurst(pos, 14, 6f);

            // Central supernova flash
            CustomParticles.GenericFlare(pos, FatePalette.SupernovaWhite, 0.7f, 18);
            CustomParticles.GenericFlare(pos, FatePalette.StellarCore, 0.6f, 16);

            // Bloom
            FateVFXLibrary.DrawBloom(pos, 0.65f);

            Lighting.AddLight(pos, FatePalette.StellarCore.ToVector3() * 1.0f);
        }
    }
}
*/
