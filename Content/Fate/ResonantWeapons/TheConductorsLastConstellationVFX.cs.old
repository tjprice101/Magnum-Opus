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
    /// VFX helper for The Conductor's Last Constellation — melee + beams, 780 dmg.
    /// Fires 3 homing spectral beams per swing. On hit, 3x cosmic lightning.
    /// Palette: FatePalette.LastConstellation
    /// (CosmicVoid -> FateCyan -> ConstellationSilver -> StarGold -> WhiteCelestial -> SupernovaWhite)
    /// </summary>
    public static class TheConductorsLastConstellationVFX
    {
        // ===== HOLD ITEM VFX =====

        /// <summary>
        /// Per-frame held-item VFX: orbiting glyphs in triple orbit at 45f radius,
        /// star sparkle aura, cosmic cloud wisps while moving, and pulsing cosmic light.
        /// </summary>
        public static void HoldItemVFX(Player player)
        {
            if (Main.dedServ) return;

            Vector2 center = player.MountedCenter;
            float time = (float)Main.timeForVisualEffects;

            // Orbiting glyphs — triple orbit at 45f radius (1-in-8)
            if (Main.rand.NextBool(8))
            {
                for (int i = 0; i < 3; i++)
                {
                    float orbitAngle = time * 0.035f + MathHelper.TwoPi * i / 3f;
                    float radius = 45f + MathF.Sin(time * 0.05f + i * 2f) * 6f;
                    Vector2 glyphPos = center + orbitAngle.ToRotationVector2() * radius;
                    Color glyphCol = FatePalette.PaletteLerp(FatePalette.LastConstellation, (float)i / 3f + 0.15f);
                    CustomParticles.Glyph(glyphPos, glyphCol * 0.6f, 0.2f, -1);
                }
            }

            // Star sparkle aura (1-in-6)
            if (Main.rand.NextBool(6))
            {
                Vector2 sparklePos = center + Main.rand.NextVector2Circular(35f, 35f);
                Color starCol = Main.rand.NextBool(3) ? FatePalette.StarGold : FatePalette.ConstellationSilver;
                var star = new GenericGlowParticle(sparklePos, Main.rand.NextVector2Circular(0.5f, 0.5f),
                    starCol * 0.55f, 0.18f, 16, true);
                MagnumParticleHandler.SpawnParticle(star);
            }

            // Cosmic cloud wisps while moving (velocity > 2f, 1-in-4)
            if (player.velocity.Length() > 2f && Main.rand.NextBool(4))
            {
                Vector2 wispPos = center + Main.rand.NextVector2Circular(20f, 20f);
                Color wispCol = Color.Lerp(FatePalette.CosmicVoid, FatePalette.FateCyan, Main.rand.NextFloat()) * 0.35f;
                var wisp = new GenericGlowParticle(wispPos,
                    -player.velocity * 0.08f + Main.rand.NextVector2Circular(0.8f, 0.8f),
                    wispCol, 0.18f, 18, true);
                MagnumParticleHandler.SpawnParticle(wisp);
            }

            // Pulsing cosmic light
            float pulse = 0.28f + MathF.Sin(time * 0.05f) * 0.1f;
            Color lightCol = Color.Lerp(FatePalette.FateCyan, FatePalette.ConstellationSilver, MathF.Sin(time * 0.03f) * 0.5f + 0.5f);
            Lighting.AddLight(center, lightCol.ToVector3() * pulse);
        }

        // ===== PREDRAW IN WORLD BLOOM =====

        /// <summary>
        /// Standard 3-layer Fate item bloom for the weapon sprite.
        /// </summary>
        public static void PreDrawInWorldBloom(
            SpriteBatch sb, Texture2D tex, Vector2 pos,
            Vector2 origin, float rotation, float scale)
        {
            float time = (float)Main.timeForVisualEffects;
            float pulse = 1f + MathF.Sin(time * 0.04f) * 0.04f;
            FatePalette.DrawItemBloom(sb, tex, pos, origin, rotation, scale, pulse);
        }

        // ===== SWING VFX =====

        /// <summary>
        /// Per-frame swing VFX: cosmic sparks with gradient, music notes,
        /// and an occasional glyph burst along the swing arc.
        /// </summary>
        public static void SwingVFX(Vector2 swingPos, Player player)
        {
            if (Main.dedServ) return;

            // Cosmic sparks (1-in-2) with LastConstellation gradient
            if (Main.rand.NextBool(2))
            {
                Color sparkCol = FatePalette.PaletteLerp(FatePalette.LastConstellation, Main.rand.NextFloat());
                Vector2 sparkVel = Main.rand.NextVector2Circular(3f, 3f);
                var spark = new GlowSparkParticle(swingPos, sparkVel, sparkCol, 0.22f, 10);
                MagnumParticleHandler.SpawnParticle(spark);
            }

            // Music notes along swing (1-in-5)
            if (Main.rand.NextBool(5))
                FateVFXLibrary.SpawnMusicNotes(swingPos, 1, 10f, 0.7f, 0.9f, 22);

            // Glyph burst accent (1-in-8)
            if (Main.rand.NextBool(8))
            {
                Color glyphCol = FatePalette.PaletteLerp(FatePalette.LastConstellation, Main.rand.NextFloat(0.2f, 0.7f));
                CustomParticles.Glyph(swingPos + Main.rand.NextVector2Circular(10f, 10f), glyphCol, 0.22f, -1);
            }

            // Swing dust
            FateVFXLibrary.SpawnFateSwingDust(swingPos, -player.velocity.SafeNormalize(Vector2.UnitX));

            Lighting.AddLight(swingPos, FatePalette.FateCyan.ToVector3() * 0.45f);
        }

        // ===== SPECTRAL BEAM TRAIL VFX =====

        /// <summary>
        /// Per-frame trail VFX for homing spectral beam projectiles.
        /// LastConstellation palette trail: FateCyan -> ConstellationSilver -> StarGold
        /// gradient particles with star sparkle accents along the beam.
        /// </summary>
        public static void SpectralBeamTrailVFX(Vector2 pos, Vector2 velocity)
        {
            if (Main.dedServ) return;

            Vector2 trailDir = -velocity.SafeNormalize(Vector2.Zero);

            // Primary trail: FateCyan -> ConstellationSilver -> StarGold gradient
            float gradientT = Main.rand.NextFloat();
            Color trailCol;
            if (gradientT < 0.33f)
                trailCol = Color.Lerp(FatePalette.FateCyan, FatePalette.ConstellationSilver, gradientT * 3f);
            else if (gradientT < 0.66f)
                trailCol = Color.Lerp(FatePalette.ConstellationSilver, FatePalette.StarGold, (gradientT - 0.33f) * 3f);
            else
                trailCol = Color.Lerp(FatePalette.StarGold, FatePalette.WhiteCelestial, (gradientT - 0.66f) * 3f);

            var glow = new GenericGlowParticle(pos + Main.rand.NextVector2Circular(4f, 4f),
                trailDir * 1.0f + Main.rand.NextVector2Circular(0.5f, 0.5f),
                trailCol * 0.75f, 0.2f, 14, true);
            MagnumParticleHandler.SpawnParticle(glow);

            // Star sparkle accents along beam (1-in-3)
            if (Main.rand.NextBool(3))
            {
                Color starCol = Main.rand.NextBool(2) ? FatePalette.StarGold : FatePalette.WhiteCelestial;
                var star = new GenericGlowParticle(pos + Main.rand.NextVector2Circular(6f, 6f),
                    Main.rand.NextVector2Circular(0.5f, 0.5f),
                    starCol * 0.6f, 0.14f, 12, true);
                MagnumParticleHandler.SpawnParticle(star);
            }

            // Cyan torch dust for beam visibility
            if (Main.rand.NextBool(3))
            {
                Dust d = Dust.NewDustPerfect(pos, DustID.BlueTorch,
                    trailDir * Main.rand.NextFloat(0.5f, 1.5f), 0,
                    FatePalette.FateCyan, 1.0f);
                d.noGravity = true;
            }

            Lighting.AddLight(pos, FatePalette.FateCyan.ToVector3() * 0.3f);
        }

        // ===== SPECTRAL BEAM IMPACT VFX =====

        /// <summary>
        /// Impact VFX when a homing spectral beam hits its target.
        /// FateVFXLibrary.ProjectileImpact, constellation burst, star sparkles.
        /// </summary>
        public static void SpectralBeamImpactVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            // Shared projectile impact
            FateVFXLibrary.ProjectileImpact(pos, 0.5f);

            // Constellation burst — stars forming a pattern at impact
            FateVFXLibrary.SpawnConstellationBurst(pos, 4, 35f, 0.7f);

            // Star sparkles scattering
            FateVFXLibrary.SpawnStarSparkles(pos, 5, 25f, 0.22f);

            // Cyan flare accent
            CustomParticles.GenericFlare(pos, FatePalette.FateCyan, 0.45f, 14);

            Lighting.AddLight(pos, FatePalette.ConstellationSilver.ToVector3() * 0.7f);
        }

        // ===== LIGHTNING STRIKE VFX =====

        /// <summary>
        /// Major cosmic lightning strike VFX from 400px above to the target position.
        /// FateVFXLibrary.DrawCosmicLightning from above, cloud burst at ground,
        /// glyph burst, star sparkles, and strong lighting.
        /// </summary>
        public static void LightningStrikeVFX(Vector2 targetPos, float scale)
        {
            if (Main.dedServ) return;

            // Cosmic lightning bolt from 400px above
            Vector2 strikeStart = targetPos + new Vector2(Main.rand.NextFloat(-25f, 25f), -400f * scale);
            FateVFXLibrary.DrawCosmicLightning(strikeStart, targetPos, 14, 40f * scale,
                FatePalette.FateCyan, FatePalette.WhiteCelestial);

            // Cloud burst at ground impact point
            FateVFXLibrary.SpawnCosmicCloudBurst(targetPos, 0.5f * scale, 10);

            // Glyph burst at strike point
            for (int i = 0; i < 4; i++)
            {
                float angle = MathHelper.TwoPi * i / 4f;
                Vector2 glyphPos = targetPos + angle.ToRotationVector2() * 18f;
                Color glyphCol = FatePalette.PaletteLerp(FatePalette.LastConstellation, (float)i / 4f + 0.2f);
                CustomParticles.Glyph(glyphPos, glyphCol, 0.25f, -1);
            }

            // Star sparkles at impact point
            FateVFXLibrary.SpawnStarSparkles(targetPos, 4, 20f * scale, 0.2f);

            // Ground impact flare
            CustomParticles.GenericFlare(targetPos, FatePalette.WhiteCelestial, 0.55f * scale, 16);
            CustomParticles.HaloRing(targetPos, FatePalette.FateCyan, 0.35f * scale, 14);

            // Radial dust burst at impact
            FateVFXLibrary.SpawnRadialDustBurst(targetPos, 8, 4f * scale);

            // Strong lighting at strike point
            Lighting.AddLight(targetPos, FatePalette.FateCyan.ToVector3() * 1.0f * scale);
        }

        // ===== IMPACT VFX =====

        /// <summary>
        /// Full on-hit impact VFX for The Conductor's Last Constellation.
        /// 3x lightning strikes, 12 gradient spark ring, 8 star particles,
        /// glyph burst, and intense cosmic lighting.
        /// </summary>
        public static void ImpactVFX(Vector2 hitPos)
        {
            if (Main.dedServ) return;

            // 3x lightning strikes from different angles above
            for (int i = 0; i < 3; i++)
            {
                float offsetX = (i - 1) * 40f + Main.rand.NextFloat(-10f, 10f);
                Vector2 strikeStart = hitPos + new Vector2(offsetX, -300f - Main.rand.NextFloat(100f));
                FateVFXLibrary.DrawCosmicLightning(strikeStart, hitPos, 12, 35f,
                    FatePalette.FateCyan, FatePalette.SupernovaWhite);
            }

            // 12 gradient spark ring — constellation pattern
            for (int i = 0; i < 12; i++)
            {
                float angle = MathHelper.TwoPi * i / 12f;
                Vector2 sparkVel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 6f);
                Color sparkCol = FatePalette.PaletteLerp(FatePalette.LastConstellation, (float)i / 12f);
                var spark = new GlowSparkParticle(hitPos, sparkVel, sparkCol, 0.25f, 14);
                MagnumParticleHandler.SpawnParticle(spark);
            }

            // 8 star particles radiating outward
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 starVel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 4f);
                Color starCol = Main.rand.NextBool(3) ? FatePalette.StarGold : FatePalette.WhiteCelestial;
                var star = new GenericGlowParticle(hitPos, starVel, starCol * 0.8f, 0.22f, 18, true);
                MagnumParticleHandler.SpawnParticle(star);
            }

            // Glyph burst at impact
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f;
                Vector2 glyphPos = hitPos + angle.ToRotationVector2() * 22f;
                Color glyphCol = FatePalette.PaletteLerp(FatePalette.LastConstellation, (float)i / 6f);
                CustomParticles.Glyph(glyphPos, glyphCol, 0.26f, -1);
            }

            // Constellation burst
            FateVFXLibrary.SpawnConstellationBurst(hitPos, 5, 50f);

            // Halo ring cascade
            FateVFXLibrary.SpawnGradientHaloRings(hitPos, 4, 0.3f);

            // Music notes
            FateVFXLibrary.SpawnMusicNotes(hitPos, 4, 25f, 0.8f, 1.0f, 28);

            // Central flare cascade
            CustomParticles.GenericFlare(hitPos, FatePalette.SupernovaWhite, 0.7f, 18);
            CustomParticles.GenericFlare(hitPos, FatePalette.FateCyan, 0.55f, 16);

            // Bloom
            FateVFXLibrary.DrawBloom(hitPos, 0.6f);

            // Intense cosmic lighting
            Lighting.AddLight(hitPos, FatePalette.FateCyan.ToVector3() * 1.2f);
        }
    }
}
