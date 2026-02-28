/* COMMENTED OUT — replaced by self-contained TheFinalFermata system
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
    /// VFX helper for The Final Fermata — magic, 520 dmg.
    /// Spawns 3 spectral Coda swords that orbit, lock onto enemies, slash through twice.
    /// Palette: FatePalette.FinalFermata
    /// (CosmicVoid -> CosmicRose -> BrightCrimson -> DarkPink -> StarGold -> SupernovaWhite)
    /// </summary>
    public static class TheFinalFermataVFX
    {
        // ===== HOLD ITEM VFX =====

        /// <summary>
        /// Per-frame held-item VFX: faint spectral swords orbiting the player,
        /// zodiac glyphs floating, star particles, and a cosmic glow with purple tint.
        /// </summary>
        public static void HoldItemVFX(Player player)
        {
            if (Main.dedServ) return;

            Vector2 center = player.MountedCenter;
            float time = (float)Main.timeForVisualEffects;

            // Faint spectral swords orbiting (1-in-8, orbit at 50-70f)
            if (Main.rand.NextBool(8))
            {
                float swordAngle = time * 0.03f + Main.rand.NextFloat(MathHelper.TwoPi);
                float swordRadius = Main.rand.NextFloat(50f, 70f);
                Vector2 swordPos = center + swordAngle.ToRotationVector2() * swordRadius;
                Color swordCol = FatePalette.PaletteLerp(FatePalette.FinalFermata, Main.rand.NextFloat(0.2f, 0.6f));
                var sword = new GenericGlowParticle(swordPos,
                    swordAngle.ToRotationVector2().RotatedBy(MathHelper.PiOver2) * 0.6f,
                    swordCol * 0.45f, 0.2f, 14, true);
                MagnumParticleHandler.SpawnParticle(sword);
            }

            // Zodiac glyphs floating (1-in-10)
            if (Main.rand.NextBool(10))
            {
                Vector2 glyphPos = center + Main.rand.NextVector2Circular(45f, 45f);
                Color glyphCol = FatePalette.PaletteLerp(FatePalette.FinalFermata, Main.rand.NextFloat(0.1f, 0.5f));
                CustomParticles.Glyph(glyphPos, glyphCol * 0.5f, 0.2f, -1);
            }

            // Star particles (1-in-7)
            if (Main.rand.NextBool(7))
            {
                Vector2 starPos = center + Main.rand.NextVector2Circular(40f, 40f);
                Color starCol = Main.rand.NextBool(3) ? FatePalette.StarGold : FatePalette.SupernovaWhite;
                var star = new GenericGlowParticle(starPos, Main.rand.NextVector2Circular(0.4f, 0.4f),
                    starCol * 0.5f, 0.16f, 16, true);
                MagnumParticleHandler.SpawnParticle(star);
            }

            // Cosmic glow with purple tint
            float pulse = 0.25f + MathF.Sin(time * 0.05f) * 0.1f;
            Color lightCol = Color.Lerp(FatePalette.FatePurple, FatePalette.CosmicRose, MathF.Sin(time * 0.04f) * 0.5f + 0.5f);
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

        // ===== SWORD SUMMON VFX =====

        /// <summary>
        /// One-shot VFX when the 3 spectral Coda swords are summoned.
        /// Per-sword: flare + glyph burst.
        /// Central: cosmic cloud burst, music notes, 4 halo rings with gradient.
        /// </summary>
        public static void SwordSummonVFX(Vector2 summonPos)
        {
            if (Main.dedServ) return;

            // Per-sword spawn effects — 3 swords at 120-degree intervals
            for (int s = 0; s < 3; s++)
            {
                float swordAngle = MathHelper.TwoPi * s / 3f;
                Vector2 swordPos = summonPos + swordAngle.ToRotationVector2() * 40f;

                // Flare at each sword spawn
                Color flareCol = FatePalette.PaletteLerp(FatePalette.FinalFermata, (float)s / 3f + 0.2f);
                CustomParticles.GenericFlare(swordPos, flareCol, 0.45f, 14);

                // Glyph burst at each sword
                for (int g = 0; g < 3; g++)
                {
                    float glyphAngle = swordAngle + MathHelper.TwoPi * g / 3f;
                    Vector2 glyphPos = swordPos + glyphAngle.ToRotationVector2() * 12f;
                    Color glyphCol = FatePalette.PaletteLerp(FatePalette.FinalFermata, Main.rand.NextFloat());
                    CustomParticles.Glyph(glyphPos, glyphCol, 0.22f, -1);
                }
            }

            // Central: cosmic cloud burst
            FateVFXLibrary.SpawnCosmicCloudBurst(summonPos, 0.6f, 10);

            // Central: music notes — the fermata holds
            FateVFXLibrary.SpawnMusicNotes(summonPos, 4, 25f, 0.8f, 1.0f, 30);

            // Central: 4 halo rings with FinalFermata gradient
            for (int i = 0; i < 4; i++)
            {
                float progress = (float)i / 4f;
                Color ringCol = FatePalette.PaletteLerp(FatePalette.FinalFermata, progress + 0.15f);
                CustomParticles.HaloRing(summonPos, ringCol, 0.3f + i * 0.1f, 14 + i * 2);
            }

            // Central flare cascade
            CustomParticles.GenericFlare(summonPos, FatePalette.SupernovaWhite, 0.6f, 18);
            CustomParticles.GenericFlare(summonPos, FatePalette.CosmicRose, 0.5f, 16);

            // Star sparkles
            FateVFXLibrary.SpawnStarSparkles(summonPos, 5, 30f, 0.22f);

            // Bloom
            FateVFXLibrary.DrawBloom(summonPos, 0.55f);

            Lighting.AddLight(summonPos, FatePalette.BrightCrimson.ToVector3() * 0.9f);
        }

        // ===== SWORD ORBIT VFX =====

        /// <summary>
        /// Per-frame VFX for each spectral sword while orbiting the player.
        /// Gentle trail particles in FinalFermata palette, subtle glow,
        /// periodic star sparkle.
        /// </summary>
        public static void SwordOrbitVFX(Vector2 swordPos, float orbitAngle)
        {
            if (Main.dedServ) return;

            // Gentle trail particles using FinalFermata palette
            Color trailCol = FatePalette.PaletteLerp(FatePalette.FinalFermata, Main.rand.NextFloat(0.2f, 0.7f));
            Vector2 trailVel = orbitAngle.ToRotationVector2().RotatedBy(MathHelper.PiOver2) * 0.5f;
            var glow = new GenericGlowParticle(swordPos + Main.rand.NextVector2Circular(4f, 4f),
                trailVel + Main.rand.NextVector2Circular(0.3f, 0.3f),
                trailCol * 0.4f, 0.15f, 14, true);
            MagnumParticleHandler.SpawnParticle(glow);

            // Subtle sword glow dust (1-in-3)
            if (Main.rand.NextBool(3))
            {
                Dust d = Dust.NewDustPerfect(swordPos + Main.rand.NextVector2Circular(6f, 6f),
                    DustID.PinkTorch, Main.rand.NextVector2Circular(0.5f, 0.5f), 0,
                    FatePalette.CosmicRose * 0.6f, 0.8f);
                d.noGravity = true;
            }

            // Periodic star sparkle (1-in-6)
            if (Main.rand.NextBool(6))
            {
                Color starCol = Main.rand.NextBool(2) ? FatePalette.StarGold : FatePalette.SupernovaWhite;
                var star = new GenericGlowParticle(swordPos + Main.rand.NextVector2Circular(8f, 8f),
                    Main.rand.NextVector2Circular(0.3f, 0.3f),
                    starCol * 0.45f, 0.12f, 12, true);
                MagnumParticleHandler.SpawnParticle(star);
            }

            Lighting.AddLight(swordPos, FatePalette.CosmicRose.ToVector3() * 0.2f);
        }

        // ===== SWORD DASH VFX =====

        /// <summary>
        /// Per-frame trail VFX when a spectral sword dashes toward a locked enemy.
        /// Fast trail: BrightCrimson -> SupernovaWhite gradient sparks,
        /// GenericGlowParticle, and cosmic dust.
        /// </summary>
        public static void SwordDashVFX(Vector2 pos, Vector2 velocity)
        {
            if (Main.dedServ) return;

            Vector2 trailDir = -velocity.SafeNormalize(Vector2.Zero);

            // Fast gradient sparks: BrightCrimson -> SupernovaWhite
            for (int i = 0; i < 2; i++)
            {
                Color sparkCol = Color.Lerp(FatePalette.BrightCrimson, FatePalette.SupernovaWhite, Main.rand.NextFloat());
                Vector2 sparkVel = trailDir * Main.rand.NextFloat(2f, 4f) + Main.rand.NextVector2Circular(1.5f, 1.5f);
                var spark = new GlowSparkParticle(pos, sparkVel, sparkCol * 0.85f, 0.2f, 10);
                MagnumParticleHandler.SpawnParticle(spark);
            }

            // Primary trail glow
            Color trailCol = FatePalette.PaletteLerp(FatePalette.FinalFermata, Main.rand.NextFloat(0.3f, 0.9f));
            var glow = new GenericGlowParticle(pos + Main.rand.NextVector2Circular(3f, 3f),
                trailDir * 1.2f + Main.rand.NextVector2Circular(0.5f, 0.5f),
                trailCol * 0.8f, 0.2f, 12, true);
            MagnumParticleHandler.SpawnParticle(glow);

            // Cosmic dust trailing (1-in-2)
            if (Main.rand.NextBool(2))
            {
                Color dustCol = Color.Lerp(FatePalette.DarkPink, FatePalette.CosmicRose, Main.rand.NextFloat()) * 0.5f;
                var dust = new GenericGlowParticle(pos + Main.rand.NextVector2Circular(5f, 5f),
                    trailDir * 0.5f + Main.rand.NextVector2Circular(0.8f, 0.8f),
                    dustCol, 0.14f, 16, true);
                MagnumParticleHandler.SpawnParticle(dust);
            }

            // Pink torch dust for density
            Dust d = Dust.NewDustPerfect(pos, DustID.PinkTorch,
                trailDir * Main.rand.NextFloat(1f, 3f), 0,
                FatePalette.BrightCrimson, 1.3f);
            d.noGravity = true;

            Lighting.AddLight(pos, FatePalette.BrightCrimson.ToVector3() * 0.4f);
        }

        // ===== SWORD SLASH IMPACT VFX =====

        /// <summary>
        /// Impact VFX when a dashing spectral sword slashes through an enemy.
        /// FateVFXLibrary.MeleeImpact at scale 1, cosmic lightning sparks,
        /// extra star burst, and music notes.
        /// </summary>
        public static void SwordSlashImpactVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            // Full melee impact via shared library
            FateVFXLibrary.MeleeImpact(pos, 1);

            // Cosmic lightning sparks — short electrical bursts
            for (int i = 0; i < 3; i++)
            {
                Vector2 sparkEnd = pos + Main.rand.NextVector2Circular(30f, 30f);
                FateVFXLibrary.DrawCosmicLightning(pos, sparkEnd, 5, 10f,
                    FatePalette.CosmicRose, FatePalette.SupernovaWhite);
            }

            // Extra star burst — 6 stars radiating
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f;
                Vector2 starVel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 5f);
                Color starCol = Main.rand.NextBool(3) ? FatePalette.StarGold : FatePalette.SupernovaWhite;
                var star = new GenericGlowParticle(pos, starVel, starCol * 0.8f, 0.22f, 18, true);
                MagnumParticleHandler.SpawnParticle(star);
            }

            // Music notes — the fermata's resonance on impact
            FateVFXLibrary.SpawnMusicNotes(pos, 3, 20f, 0.8f, 1.0f, 25);

            // Glyph accent
            FateVFXLibrary.SpawnGlyphAccent(pos, 0.28f);

            // Halo ring
            CustomParticles.HaloRing(pos, FatePalette.CosmicRose, 0.35f, 14);

            // Flare cascade
            CustomParticles.GenericFlare(pos, FatePalette.SupernovaWhite, 0.55f, 16);
            CustomParticles.GenericFlare(pos, FatePalette.BrightCrimson, 0.45f, 14);

            Lighting.AddLight(pos, FatePalette.BrightCrimson.ToVector3() * 0.9f);
        }

        // ===== SWORD RETURN TRAIL VFX =====

        /// <summary>
        /// Per-frame trail VFX when a spectral sword returns to the player after slashing.
        /// Fading trail using DarkPink -> StarGold, with reduced particle density.
        /// </summary>
        public static void SwordReturnTrailVFX(Vector2 pos, Vector2 velocity)
        {
            if (Main.dedServ) return;

            Vector2 trailDir = -velocity.SafeNormalize(Vector2.Zero);

            // Fading return trail: DarkPink -> StarGold gradient
            Color returnCol = Color.Lerp(FatePalette.DarkPink, FatePalette.StarGold, Main.rand.NextFloat());
            var glow = new GenericGlowParticle(pos + Main.rand.NextVector2Circular(3f, 3f),
                trailDir * 0.6f + Main.rand.NextVector2Circular(0.4f, 0.4f),
                returnCol * 0.45f, 0.14f, 14, true);
            MagnumParticleHandler.SpawnParticle(glow);

            // Reduced density sparks (1-in-3)
            if (Main.rand.NextBool(3))
            {
                Color sparkCol = Color.Lerp(FatePalette.DarkPink, FatePalette.StarGold, Main.rand.NextFloat());
                Vector2 sparkVel = trailDir * Main.rand.NextFloat(0.5f, 1.5f) + Main.rand.NextVector2Circular(0.8f, 0.8f);
                var spark = new GlowSparkParticle(pos, sparkVel, sparkCol * 0.5f, 0.12f, 10);
                MagnumParticleHandler.SpawnParticle(spark);
            }

            // Light fading dust (1-in-4)
            if (Main.rand.NextBool(4))
            {
                Dust d = Dust.NewDustPerfect(pos, DustID.PinkTorch,
                    trailDir * Main.rand.NextFloat(0.3f, 1f), 0,
                    FatePalette.DarkPink * 0.6f, 0.8f);
                d.noGravity = true;
            }

            Lighting.AddLight(pos, FatePalette.DarkPink.ToVector3() * 0.2f);
        }
    }
}
*/
