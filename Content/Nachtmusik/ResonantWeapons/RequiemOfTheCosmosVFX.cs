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
    /// VFX helper for the Requiem of the Cosmos magic weapon.
    /// Grand cosmic finale — somber yet magnificent, from void darkness
    /// to golden supernova brilliance. Cosmic cloud swirls, golden radiance
    /// eruptions, shattered starlight sprays, and celestial implosion effects.
    /// Each cast echoes through the cosmos as a requiem for dying stars.
    /// </summary>
    public static class RequiemOfTheCosmosVFX
    {
        // =====================================================================
        //  HOLD ITEM VFX
        // =====================================================================

        public static void HoldItemVFX(Player player)
        {
            if (Main.dedServ) return;

            Vector2 center = player.MountedCenter;
            float time = (float)Main.timeForVisualEffects;

            // Cosmic void aura — deep, grand
            if (Main.rand.NextBool(5))
            {
                Vector2 offset = Main.rand.NextVector2Circular(25f, 25f);
                Color voidColor = Color.Lerp(NachtmusikPalette.CosmicVoid, NachtmusikPalette.DeepBlue,
                    Main.rand.NextFloat());
                var glow = new GenericGlowParticle(center + offset, Main.rand.NextVector2Circular(0.4f, 0.4f),
                    voidColor * 0.4f, 0.2f, 22, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }

            // Golden radiance pulsing
            if (Main.rand.NextBool(10))
            {
                try { CustomParticles.GenericFlare(center + Main.rand.NextVector2Circular(18f, 18f),
                    NachtmusikPalette.RadianceGold * 0.3f, 0.15f, 12); } catch { }
            }

            NachtmusikVFXLibrary.AddRadianceLight(center, time, 0.25f);
        }

        // =====================================================================
        //  PREDRAW IN WORLD BLOOM
        // =====================================================================

        public static void PreDrawInWorldBloom(SpriteBatch sb, Texture2D tex,
            Vector2 pos, Vector2 origin, float rotation, float scale)
        {
            float time = (float)Main.timeForVisualEffects;
            float pulse = 1f + MathF.Sin(time * 0.035f) * 0.04f;
            NachtmusikPalette.DrawItemBloom(sb, tex, pos, origin, rotation, scale, pulse);
        }

        // =====================================================================
        //  CAST BURST VFX
        // =====================================================================

        public static void CastBurstVFX(Vector2 castPos)
        {
            if (Main.dedServ) return;

            // Cosmic implosion-then-explosion
            try { CustomParticles.GenericFlare(castPos, NachtmusikPalette.RadianceGold, 0.8f, 16); } catch { }
            try { CustomParticles.GenericFlare(castPos, NachtmusikPalette.StarWhite, 0.6f, 14); } catch { }

            // Expanding cosmic ring
            var burstA = new StarBurstParticle(castPos, Vector2.Zero, NachtmusikPalette.DeepBlue, 0.5f, 18);
            MagnumParticleHandler.SpawnParticle(burstA);
            var burstB = new StarBurstParticle(castPos, Vector2.Zero, NachtmusikPalette.RadianceGold * 0.7f, 0.35f, 14, 1);
            MagnumParticleHandler.SpawnParticle(burstB);

            // Radial golden sparks
            for (int i = 0; i < 10; i++)
            {
                float angle = MathHelper.TwoPi * i / 10f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 7f);
                Color sparkColor = Color.Lerp(NachtmusikPalette.StarlitBlue, NachtmusikPalette.RadianceGold, (float)i / 10f);
                var spark = new GlowSparkParticle(castPos, vel, sparkColor, 0.25f, 16);
                MagnumParticleHandler.SpawnParticle(spark);
            }

            // Glyphs
            NachtmusikVFXLibrary.SpawnGlyphBurst(castPos, 5, 4f, 0.4f);

            // Music notes (grand)
            NachtmusikVFXLibrary.SpawnMusicNotes(castPos, 3, 15f, 0.8f, 1.0f, 25);

            Lighting.AddLight(castPos, NachtmusikPalette.RadianceGold.ToVector3() * 0.8f);
        }

        // =====================================================================
        //  PROJECTILE TRAIL VFX
        // =====================================================================

        public static void ProjectileTrailVFX(Vector2 pos, Vector2 velocity)
        {
            if (Main.dedServ) return;

            // Cosmic cloud trail — deep and grand
            NachtmusikVFXLibrary.SpawnCloudTrail(pos, velocity, 1.2f);

            // Deep blue cosmic dust
            Dust d = Dust.NewDustPerfect(pos, DustID.BlueTorch,
                Main.rand.NextVector2Circular(1f, 1f), 0, NachtmusikPalette.DeepBlue, 1.4f);
            d.noGravity = true;

            // Golden radiance sparkle (1-in-3)
            if (Main.rand.NextBool(3))
            {
                try { CustomParticles.GenericFlare(pos + Main.rand.NextVector2Circular(6f, 6f),
                    NachtmusikPalette.RadianceGold * 0.5f, 0.22f, 12); } catch { }
            }

            // Cosmic void wisps (1-in-4)
            if (Main.rand.NextBool(4))
            {
                var wisp = new GenericGlowParticle(pos + Main.rand.NextVector2Circular(8f, 8f),
                    Main.rand.NextVector2Circular(0.5f, 0.5f),
                    NachtmusikPalette.CosmicVoid * 0.5f, 0.18f, 16, true);
                MagnumParticleHandler.SpawnParticle(wisp);
            }

            // Music note (1-in-6)
            if (Main.rand.NextBool(6))
                NachtmusikVFXLibrary.SpawnMusicNotes(pos, 1, 6f, 0.7f, 0.9f, 22);

            NachtmusikVFXLibrary.AddRadianceLight(pos, Main.GameUpdateCount, 0.35f);
        }

        // =====================================================================
        //  HIT VFX
        // =====================================================================

        public static void SmallHitVFX(Vector2 hitPos)
        {
            if (Main.dedServ) return;

            // Golden supernova flash
            NachtmusikVFXLibrary.DrawBloom(hitPos, 0.5f);
            NachtmusikVFXLibrary.SpawnGradientHaloRings(hitPos, 3, 0.25f);
            NachtmusikVFXLibrary.SpawnRadialDustBurst(hitPos, 10, 5f);
            NachtmusikVFXLibrary.SpawnRadianceBurst(hitPos, 6, 4f);
            NachtmusikVFXLibrary.SpawnTwinklingStars(hitPos, 4, 15f);
            NachtmusikVFXLibrary.SpawnMusicNotes(hitPos, 2, 15f, 0.7f, 0.9f, 25);

            // Shattered starlight
            NachtmusikVFXLibrary.SpawnShatteredStarlight(hitPos, 4, 4f, 0.6f, true);

            Lighting.AddLight(hitPos, NachtmusikPalette.RadianceGold.ToVector3() * 0.7f);
        }

        // =====================================================================
        //  SPECIAL / FINISHER VFX
        // =====================================================================

        public static void CosmicFinaleVFX(Vector2 pos, float intensity = 1f)
        {
            if (Main.dedServ) return;

            NachtmusikVFXLibrary.FinisherSlam(pos, intensity);

            // Unique requiem: cosmic implosion -> golden supernova expansion
            // Implosion particles converging
            for (int i = 0; i < 12; i++)
            {
                float angle = MathHelper.TwoPi * i / 12f;
                float dist = 60f * intensity;
                Vector2 startPos = pos + angle.ToRotationVector2() * dist;
                Vector2 vel = (pos - startPos).SafeNormalize(Vector2.Zero) * 4f;
                Color implodeColor = NachtmusikPalette.GetNocturnalGradient((float)i / 12f);
                var converge = new GlowSparkParticle(startPos, vel, implodeColor, 0.3f * intensity, 20);
                MagnumParticleHandler.SpawnParticle(converge);
            }

            // Golden radiance supernova ring
            NachtmusikVFXLibrary.SpawnRadianceHaloRings(pos, 6, 0.3f * intensity);
        }

        // =====================================================================
        //  DEATH VFX
        // =====================================================================

        public static void ProjectileDeathVFX(Vector2 pos)
        {
            if (Main.dedServ) return;

            NachtmusikVFXLibrary.SpawnRadialDustBurst(pos, 8, 4f);
            NachtmusikVFXLibrary.SpawnRadianceBurst(pos, 4, 3f);
            NachtmusikVFXLibrary.SpawnShatteredStarlight(pos, 4, 4f, 0.5f, true);
            try { CustomParticles.HaloRing(pos, NachtmusikPalette.RadianceGold * 0.5f, 0.22f, 10); } catch { }
        }
    }
}
