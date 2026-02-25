using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Content.Nachtmusik.Accessories
{
    /// <summary>
    /// VFX helper for all Nachtmusik-themed accessories.
    /// Shared ambient utilities and per-accessory unique effects:
    /// ConstellationQuiver, MoonlitSerenadePendant, NocturnesEmbrace,
    /// RadianceOfTheNightQueen, StarweaversSignet.
    /// </summary>
    public static class NachtmusikAccessoryVFX
    {
        // =====================================================================
        //  SHARED AMBIENT UTILITIES
        // =====================================================================

        /// <summary>
        /// Gentle starlit dust drift shared across all Nachtmusik accessories.
        /// </summary>
        public static void AmbientStarlitDrift(Vector2 center)
        {
            if (Main.dedServ) return;

            if (Main.rand.NextBool(8))
            {
                Vector2 offset = Main.rand.NextVector2Circular(30f, 30f);
                Color col = NachtmusikPalette.GetCelestialGradient(Main.rand.NextFloat());
                var glow = new GenericGlowParticle(center + offset,
                    new Vector2(Main.rand.NextFloat(-0.3f, 0.3f), -0.4f),
                    col * 0.3f, 0.12f, 18, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }
        }

        /// <summary>
        /// Shared ambient music note wisps for Nachtmusik accessories.
        /// </summary>
        public static void AmbientMusicNotes(Vector2 center)
        {
            if (Main.dedServ) return;

            if (Main.rand.NextBool(25))
                NachtmusikVFXLibrary.SpawnMusicNotes(center + Main.rand.NextVector2Circular(20f, 20f),
                    1, 8f, 0.7f, 0.8f, 22);
        }

        /// <summary>
        /// Shared ambient pulsing light for Nachtmusik accessories.
        /// </summary>
        public static void AmbientLight(Vector2 center, Color lightColor, float baseIntensity = 0.15f)
        {
            float pulse = baseIntensity + MathF.Sin((float)Main.timeForVisualEffects * 0.06f) * 0.04f;
            Lighting.AddLight(center, lightColor.ToVector3() * pulse);
        }

        /// <summary>
        /// Full ambient VFX combining shared effects — call from accessory PostUpdate.
        /// </summary>
        public static void FullAmbientVFX(Vector2 center, Color lightColor)
        {
            AmbientStarlitDrift(center);
            AmbientMusicNotes(center);
            AmbientLight(center, lightColor);
        }

        /// <summary>
        /// Shared on-hit proc VFX for accessory proc effects.
        /// </summary>
        public static void OnHitProcVFX(Vector2 hitPos, Color procColor, float intensity = 1f)
        {
            if (Main.dedServ) return;

            // Proc flash
            try { CustomParticles.GenericFlare(hitPos, procColor, 0.5f * intensity, 14); } catch { }
            try { CustomParticles.HaloRing(hitPos, procColor * 0.6f, 0.3f * intensity, 12); } catch { }

            // Radial sparkles
            for (int i = 0; i < 5; i++)
            {
                float angle = MathHelper.TwoPi * i / 5f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 4f);
                Color col = NachtmusikPalette.GetCelestialGradient((float)i / 5f);
                var spark = new GlowSparkParticle(hitPos, vel, col * intensity, 0.2f, 12);
                MagnumParticleHandler.SpawnParticle(spark);
            }

            NachtmusikVFXLibrary.SpawnMusicNotes(hitPos, 2, 12f, 0.7f, 0.9f, 22);

            Lighting.AddLight(hitPos, procColor.ToVector3() * 0.5f * intensity);
        }

        /// <summary>
        /// Shared PreDrawInWorld bloom for accessory items.
        /// </summary>
        public static void DrawWorldItemBloom(SpriteBatch sb, Texture2D tex,
            Vector2 pos, Vector2 origin, float rotation, float scale)
        {
            float time = (float)Main.timeForVisualEffects;
            float pulse = 1f + MathF.Sin(time * 0.04f) * 0.04f;
            NachtmusikPalette.DrawItemBloom(sb, tex, pos, origin, rotation, scale, pulse);
        }

        // =====================================================================
        //  CONSTELLATION QUIVER — Ranged Accessory
        // =====================================================================

        /// <summary>
        /// Constellation Quiver ambient: pinpoint constellation dots orbiting,
        /// mini constellation line patterns, star-gold arrow accents.
        /// </summary>
        public static void ConstellationQuiverAmbientVFX(Player player)
        {
            if (Main.dedServ) return;

            Vector2 center = player.MountedCenter;
            float time = (float)Main.timeForVisualEffects;

            // Orbiting constellation star points
            if (Main.rand.NextBool(10))
            {
                float angle = time * 0.04f + Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 starPos = center + angle.ToRotationVector2() * Main.rand.NextFloat(20f, 35f);
                try { CustomParticles.GenericFlare(starPos,
                    NachtmusikPalette.StarGold * 0.4f, 0.12f, 10); } catch { }
            }

            // Mini constellation lines (rare)
            if (Main.rand.NextBool(40))
            {
                Vector2 start = center + Main.rand.NextVector2Circular(25f, 25f);
                Vector2 end = start + Main.rand.NextVector2Circular(18f, 18f);
                NachtmusikVFXLibrary.SpawnConstellationLine(start, end, 2);
            }

            FullAmbientVFX(center, NachtmusikPalette.ConstellationBlue);
        }

        /// <summary>
        /// Constellation Quiver on-hit: constellation web flash at impact.
        /// </summary>
        public static void ConstellationQuiverProcVFX(Vector2 hitPos)
        {
            if (Main.dedServ) return;

            OnHitProcVFX(hitPos, NachtmusikPalette.StarGold);
            NachtmusikVFXLibrary.SpawnConstellationCircle(hitPos, 18f, 4,
                Main.rand.NextFloat(MathHelper.TwoPi));
        }

        // =====================================================================
        //  MOONLIT SERENADE PENDANT — Magic Accessory
        // =====================================================================

        /// <summary>
        /// Moonlit Serenade Pendant ambient: warm serenade glow aura,
        /// moonlit silver orbiting sparkles, ascending golden motes.
        /// </summary>
        public static void MoonlitSerenadePendantAmbientVFX(Player player)
        {
            if (Main.dedServ) return;

            Vector2 center = player.MountedCenter;

            // Warm serenade aura
            if (Main.rand.NextBool(8))
            {
                Vector2 offset = Main.rand.NextVector2Circular(25f, 25f);
                Color auraColor = Color.Lerp(NachtmusikPalette.SerenadeGlow,
                    NachtmusikPalette.MoonlitSilver, Main.rand.NextFloat());
                var glow = new GenericGlowParticle(center + offset,
                    new Vector2(0, -0.3f),
                    auraColor * 0.3f, 0.14f, 18, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }

            // Silver moonlit sparkle (rare)
            if (Main.rand.NextBool(15))
            {
                var sparkle = new SparkleParticle(
                    center + Main.rand.NextVector2Circular(20f, 20f),
                    new Vector2(0, -0.5f),
                    NachtmusikPalette.MoonlitSilver * 0.5f, 0.3f, 16);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }

            FullAmbientVFX(center, NachtmusikPalette.SerenadeGlow);
        }

        /// <summary>
        /// Moonlit Serenade Pendant on-hit: serenade shimmer burst.
        /// </summary>
        public static void MoonlitSerenadePendantProcVFX(Vector2 hitPos)
        {
            if (Main.dedServ) return;

            OnHitProcVFX(hitPos, NachtmusikPalette.SerenadeGlow);
            NachtmusikVFXLibrary.SpawnRadianceBurst(hitPos, 4, 3f);
        }

        // =====================================================================
        //  NOCTURNE'S EMBRACE — Summoner Accessory
        // =====================================================================

        /// <summary>
        /// Nocturne's Embrace ambient: deep blue nocturnal mist,
        /// hovering starlit wisps that orbit slowly, cosmic purple shimmer.
        /// </summary>
        public static void NocturnesEmbraceAmbientVFX(Player player)
        {
            if (Main.dedServ) return;

            Vector2 center = player.MountedCenter;
            float time = (float)Main.timeForVisualEffects;

            // Nocturnal mist aura
            if (Main.rand.NextBool(6))
            {
                Vector2 offset = Main.rand.NextVector2Circular(28f, 28f);
                Color mistColor = Color.Lerp(NachtmusikPalette.MidnightBlue,
                    NachtmusikPalette.DeepBlue, Main.rand.NextFloat());
                var mist = new GenericGlowParticle(center + offset,
                    Main.rand.NextVector2Circular(0.3f, 0.3f),
                    mistColor * 0.35f, 0.15f, 22, true);
                MagnumParticleHandler.SpawnParticle(mist);
            }

            // Orbiting starlit wisps
            if (Main.GameUpdateCount % 18 == 0)
            {
                float baseAngle = time * 0.03f;
                for (int i = 0; i < 2; i++)
                {
                    float angle = baseAngle + MathHelper.Pi * i;
                    Vector2 wispPos = center + angle.ToRotationVector2() * 30f;
                    try { CustomParticles.GenericFlare(wispPos,
                        NachtmusikPalette.StarlitBlue * 0.4f, 0.2f, 12); } catch { }
                }
            }

            FullAmbientVFX(center, NachtmusikPalette.DeepBlue);
        }

        /// <summary>
        /// Nocturne's Embrace on-hit: nocturnal blessing burst.
        /// </summary>
        public static void NocturnesEmbraceProcVFX(Vector2 hitPos)
        {
            if (Main.dedServ) return;

            OnHitProcVFX(hitPos, NachtmusikPalette.DeepBlue);
            NachtmusikVFXLibrary.SpawnTwinklingStars(hitPos, 3, 12f);
        }

        // =====================================================================
        //  RADIANCE OF THE NIGHT QUEEN — Universal Accessory
        // =====================================================================

        /// <summary>
        /// Radiance of the Night Queen ambient: regal golden radiance aura,
        /// cosmic halo orbit, shimmering starlight crown effect,
        /// grand music note accents. The queen's presence illuminates.
        /// </summary>
        public static void RadianceOfTheNightQueenAmbientVFX(Player player)
        {
            if (Main.dedServ) return;

            Vector2 center = player.MountedCenter;
            float time = (float)Main.timeForVisualEffects;

            // Regal golden radiance aura
            if (Main.rand.NextBool(5))
            {
                Vector2 offset = Main.rand.NextVector2Circular(30f, 30f);
                Color radianceColor = Color.Lerp(NachtmusikPalette.RadianceGold,
                    NachtmusikPalette.StarWhite, Main.rand.NextFloat() * 0.5f);
                var glow = new GenericGlowParticle(center + offset,
                    new Vector2(0, -0.5f),
                    radianceColor * 0.35f, 0.18f, 20, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }

            // Crown sparkle above head
            if (Main.rand.NextBool(12))
            {
                Vector2 crownPos = center + new Vector2(Main.rand.NextFloat(-12f, 12f), -35f);
                var sparkle = new SparkleParticle(crownPos,
                    new Vector2(Main.rand.NextFloat(-0.3f, 0.3f), -0.3f),
                    NachtmusikPalette.RadianceGold * 0.6f, 0.35f, 18);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }

            // Cosmic halo orbit (periodic)
            if (Main.GameUpdateCount % 25 == 0)
            {
                float baseAngle = time * 0.03f;
                for (int i = 0; i < 3; i++)
                {
                    float angle = baseAngle + MathHelper.TwoPi * i / 3f;
                    Vector2 haloPos = center + angle.ToRotationVector2() * 35f;
                    try { CustomParticles.GenericFlare(haloPos,
                        NachtmusikPalette.GetStarfieldGradient((float)i / 3f) * 0.5f,
                        0.25f, 14); } catch { }
                }
            }

            FullAmbientVFX(center, NachtmusikPalette.RadianceGold);
        }

        /// <summary>
        /// Radiance of the Night Queen on-hit: grand golden radiance proc.
        /// </summary>
        public static void RadianceOfTheNightQueenProcVFX(Vector2 hitPos)
        {
            if (Main.dedServ) return;

            OnHitProcVFX(hitPos, NachtmusikPalette.RadianceGold, 1.2f);
            NachtmusikVFXLibrary.SpawnRadianceBurst(hitPos, 6, 4f);
            NachtmusikVFXLibrary.SpawnGradientHaloRings(hitPos, 3, 0.25f);
        }

        /// <summary>
        /// Queen's Radiance buff activation VFX — grand burst.
        /// </summary>
        public static void QueensRadianceActivateVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            try { CustomParticles.GenericFlare(pos, NachtmusikPalette.RadianceGold, 0.8f, 18); } catch { }
            try { CustomParticles.GenericFlare(pos, NachtmusikPalette.StarWhite, 0.6f, 16); } catch { }

            NachtmusikVFXLibrary.SpawnGradientHaloRings(pos, 4, 0.35f);
            NachtmusikVFXLibrary.SpawnRadianceHaloRings(pos, 3, 0.3f);
            NachtmusikVFXLibrary.SpawnStarburstCascade(pos, 8, 5f, 0.3f);
            NachtmusikVFXLibrary.SpawnMusicNotes(pos, 4, 25f, 0.8f, 1.0f, 30);

            Lighting.AddLight(pos, NachtmusikPalette.RadianceGold.ToVector3() * 0.9f);
        }

        /// <summary>
        /// Queen's Radiance active buff ambient — subtle golden shimmer.
        /// </summary>
        public static void QueensRadianceActiveVFX(Player player)
        {
            if (Main.dedServ) return;

            Vector2 center = player.MountedCenter;

            // Active radiance shimmer
            if (Main.rand.NextBool(4))
            {
                Vector2 offset = Main.rand.NextVector2Circular(35f, 35f);
                Color shimmer = NachtmusikPalette.GetRadianceShimmer();
                var glow = new GenericGlowParticle(center + offset,
                    new Vector2(0, -0.6f),
                    shimmer * 0.4f, 0.15f, 16, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }

            // Active twinkling
            if (Main.rand.NextBool(10))
                NachtmusikVFXLibrary.SpawnTwinklingStars(center, 1, 25f);

            Lighting.AddLight(center, NachtmusikPalette.RadianceGold.ToVector3() * 0.25f);
        }

        // =====================================================================
        //  STARWEAVER'S SIGNET — Melee Accessory
        // =====================================================================

        /// <summary>
        /// Starweaver's Signet ambient: arcane star-thread patterns orbiting,
        /// violet glyph accents, cosmic purple shimmer around the player.
        /// </summary>
        public static void StarweaversSignetAmbientVFX(Player player)
        {
            if (Main.dedServ) return;

            Vector2 center = player.MountedCenter;
            float time = (float)Main.timeForVisualEffects;

            // Star-thread weaving aura
            if (Main.rand.NextBool(7))
            {
                float angle = time * 0.035f + Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 threadPos = center + angle.ToRotationVector2() * Main.rand.NextFloat(18f, 30f);
                Color threadColor = Color.Lerp(NachtmusikPalette.CosmicPurple,
                    NachtmusikPalette.Violet, Main.rand.NextFloat());
                var thread = new GenericGlowParticle(threadPos,
                    (center - threadPos).SafeNormalize(Vector2.Zero) * 0.4f,
                    threadColor * 0.35f, 0.13f, 18, true);
                MagnumParticleHandler.SpawnParticle(thread);
            }

            // Glyph accent (rare)
            if (Main.rand.NextBool(30))
            {
                float glyphAngle = time * 0.025f;
                Vector2 glyphPos = center + glyphAngle.ToRotationVector2() * 25f;
                try { CustomParticles.Glyph(glyphPos, NachtmusikPalette.Violet * 0.35f, 0.2f, -1); } catch { }
            }

            FullAmbientVFX(center, NachtmusikPalette.CosmicPurple);
        }

        /// <summary>
        /// Starweaver's Signet on-hit: glyph burst melee proc.
        /// </summary>
        public static void StarweaversSignetProcVFX(Vector2 hitPos)
        {
            if (Main.dedServ) return;

            OnHitProcVFX(hitPos, NachtmusikPalette.Violet);
            NachtmusikVFXLibrary.SpawnGlyphBurst(hitPos, 3, 2.5f, 0.25f);
        }
    }
}
