using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Content.Nachtmusik.ResonantWeapons
{
    /// <summary>
    /// VFX helper for the Starweaver's Grimoire magic weapon.
    /// Intricate star-weaving spellcraft — arcane patterns woven from cosmic
    /// purple threads, violet glyph constellations, serenade glow accents,
    /// and crystalline star-pattern detonations. Each cast weaves a new design.
    /// </summary>
    public static class StarweaversGrimoireVFX
    {
        // =====================================================================
        //  HOLD ITEM VFX
        // =====================================================================

        public static void HoldItemVFX(Player player)
        {
            if (Main.dedServ) return;

            Vector2 center = player.MountedCenter;
            float time = (float)Main.timeForVisualEffects;

            // Arcane star-thread aura
            if (Main.rand.NextBool(6))
            {
                float angle = time * 0.03f + Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 threadPos = center + angle.ToRotationVector2() * Main.rand.NextFloat(15f, 25f);
                Color threadColor = Color.Lerp(NachtmusikPalette.CosmicPurple, NachtmusikPalette.Violet,
                    Main.rand.NextFloat());
                var thread = new GenericGlowParticle(threadPos,
                    (center - threadPos).SafeNormalize(Vector2.Zero) * 0.3f,
                    threadColor * 0.35f, 0.12f, 18, true);
                MagnumParticleHandler.SpawnParticle(thread);
            }

            // Orbiting grimoire glyphs
            if (Main.rand.NextBool(18))
            {
                float glyphAngle = time * 0.025f;
                Vector2 glyphPos = center + glyphAngle.ToRotationVector2() * 20f;
                try { CustomParticles.Glyph(glyphPos, NachtmusikPalette.Violet * 0.4f, 0.25f, -1); } catch { }
            }

            NachtmusikVFXLibrary.AddNachtmusikLight(center, 0.2f);
        }

        // =====================================================================
        //  PREDRAW IN WORLD BLOOM
        // =====================================================================

        public static void PreDrawInWorldBloom(SpriteBatch sb, Texture2D tex,
            Vector2 pos, Vector2 origin, float rotation, float scale)
        {
            float time = (float)Main.timeForVisualEffects;
            float pulse = 1f + MathF.Sin(time * 0.04f) * 0.03f;
            NachtmusikPalette.DrawItemBloom(sb, tex, pos, origin, rotation, scale, pulse);
        }

        // =====================================================================
        //  CAST BURST VFX
        // =====================================================================

        public static void CastBurstVFX(Vector2 castPos)
        {
            if (Main.dedServ) return;

            // Glyph circle burst
            NachtmusikVFXLibrary.SpawnGlyphBurst(castPos, 6, 3.5f, 0.35f);

            // Central arcane flash
            try { CustomParticles.GenericFlare(castPos, NachtmusikPalette.Violet, 0.7f, 14); } catch { }
            try { CustomParticles.GenericFlare(castPos, NachtmusikPalette.SerenadeGlow * 0.6f, 0.5f, 12); } catch { }

            // Star-weaving pattern burst
            var magicBurst = new StarBurstParticle(castPos, Vector2.Zero,
                NachtmusikPalette.Violet, 0.4f, 16);
            MagnumParticleHandler.SpawnParticle(magicBurst);

            // Weaving thread sparks
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 5f);
                var spark = new GlowSparkParticle(castPos, vel,
                    NachtmusikPalette.GetCelestialGradient((float)i / 8f), 0.22f, 14);
                MagnumParticleHandler.SpawnParticle(spark);
            }

            // Music notes
            NachtmusikVFXLibrary.SpawnMusicNotes(castPos, 2, 12f, 0.7f, 0.9f, 22);

            Lighting.AddLight(castPos, NachtmusikPalette.Violet.ToVector3() * 0.7f);
        }

        // =====================================================================
        //  PROJECTILE TRAIL VFX
        // =====================================================================

        public static void ProjectileTrailVFX(Vector2 pos, Vector2 velocity)
        {
            if (Main.dedServ) return;

            // Arcane weave trail
            NachtmusikVFXLibrary.SpawnCloudTrail(pos, velocity, 0.7f);

            // Purple-violet thread dust
            if (Main.rand.NextBool(2))
            {
                Vector2 vel = Main.rand.NextVector2Circular(1f, 1f);
                Color threadColor = Color.Lerp(NachtmusikPalette.CosmicPurple, NachtmusikPalette.Violet,
                    Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(pos, DustID.PurpleTorch, vel, 0, threadColor, 1.2f);
                d.noGravity = true;
            }

            // Serenade glow sparkle (1-in-4)
            if (Main.rand.NextBool(4))
            {
                try { CustomParticles.GenericFlare(pos + Main.rand.NextVector2Circular(5f, 5f),
                    NachtmusikPalette.SerenadeGlow * 0.5f, 0.2f, 12); } catch { }
            }

            // Glyph accent (1-in-12)
            if (Main.rand.NextBool(12))
            {
                try { CustomParticles.Glyph(pos + Main.rand.NextVector2Circular(8f, 8f),
                    NachtmusikPalette.Violet * 0.5f, 0.25f, -1); } catch { }
            }

            // Music note (1-in-7)
            if (Main.rand.NextBool(7))
                NachtmusikVFXLibrary.SpawnMusicNotes(pos, 1, 6f, 0.7f, 0.85f, 20);

            NachtmusikVFXLibrary.AddNachtmusikLight(pos, 0.3f);
        }

        // =====================================================================
        //  HIT VFX
        // =====================================================================

        public static void SmallHitVFX(Vector2 hitPos)
        {
            if (Main.dedServ) return;

            NachtmusikVFXLibrary.SpawnGradientHaloRings(hitPos, 3, 0.2f);
            NachtmusikVFXLibrary.SpawnTwinklingStars(hitPos, 3, 12f);
            NachtmusikVFXLibrary.SpawnRadialDustBurst(hitPos, 8, 4f);
            NachtmusikVFXLibrary.SpawnMusicNotes(hitPos, 2, 15f, 0.7f, 0.9f, 22);

            // Glyph burst at impact
            NachtmusikVFXLibrary.SpawnGlyphBurst(hitPos, 3, 3f, 0.3f);

            // Star-weave pattern flash
            var burst = new StarBurstParticle(hitPos, Vector2.Zero,
                NachtmusikPalette.Violet * 0.6f, 0.3f, 12);
            MagnumParticleHandler.SpawnParticle(burst);

            Lighting.AddLight(hitPos, NachtmusikPalette.Violet.ToVector3() * 0.6f);
        }

        // =====================================================================
        //  COMBO / SPECIAL VFX
        // =====================================================================

        public static void SpecialCastVFX(Vector2 pos, float intensity = 1f)
        {
            if (Main.dedServ) return;

            NachtmusikVFXLibrary.ProjectileImpact(pos, intensity);

            // Unique weaving: glyph orbiting ring
            NachtmusikVFXLibrary.SpawnOrbitingGlyphs(pos, 8, 40f * intensity, Main.GameUpdateCount * 0.03f);

            // Star-pattern constellation
            NachtmusikVFXLibrary.SpawnConstellationCircle(pos, 30f * intensity, 6, Main.rand.NextFloat(MathHelper.TwoPi));
        }

        // =====================================================================
        //  DEATH VFX
        // =====================================================================

        public static void ProjectileDeathVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            NachtmusikVFXLibrary.SpawnRadialDustBurst(pos, 6, 3f);
            NachtmusikVFXLibrary.SpawnGlyphBurst(pos, 3, 2f, 0.25f);
            try { CustomParticles.HaloRing(pos, NachtmusikPalette.Violet * 0.5f, 0.2f, 10); } catch { }
        }
    }
}
