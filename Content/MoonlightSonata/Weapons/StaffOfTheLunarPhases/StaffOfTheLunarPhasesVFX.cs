using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.VFX.Bloom;
using MagnumOpus.Common.Systems.VFX.Optimization;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Content.MoonlightSonata.Weapons.StaffOfTheLunarPhases
{
    /// <summary>
    /// VFX helper for Staff of the Lunar Phases — "The Conductor's Baton".
    /// Provides all visual effects: hold aura, summon ritual, bloom, and ambient particles.
    /// Call from the weapon's HoldItem, PreDrawInWorld, and Shoot methods.
    /// </summary>
    public static class StaffOfTheLunarPhasesVFX
    {
        // Per-weapon accent colors
        private static readonly Color BatonGlow = new Color(190, 160, 255);    // Conductor's baton lavender
        private static readonly Color RitualFlash = new Color(220, 200, 255);  // Summoning circle flash
        private static readonly Color PhaseShift = new Color(160, 120, 240);   // Lunar phase transition

        // =====================================================================
        //  HOLD ITEM VFX — orbiting motes, sparkles, music notes, ambient dust
        // =====================================================================

        /// <summary>
        /// Per-frame VFX while the staff is held. Produces orbiting lunar motes,
        /// prismatic sparkles, conductor's baton music notes, and ambient dust.
        /// </summary>
        public static void HoldItemVFX(Player player)
        {
            if (Main.dedServ) return;

            float time = Main.GameUpdateCount * 0.04f;

            // Orbiting lunar motes — 3 crescent points cycling
            if (Main.rand.NextBool(6))
            {
                for (int i = 0; i < 3; i++)
                {
                    float angle = time + MathHelper.TwoPi * i / 3f;
                    float radius = 30f + MathF.Sin(Main.GameUpdateCount * 0.05f + i * 0.7f) * 6f;
                    Vector2 orbitPos = player.Center + angle.ToRotationVector2() * radius;
                    float progress = (float)i / 3f;
                    Color orbitColor = Color.Lerp(MoonlightSonataPalette.DarkPurple, MoonlightSonataPalette.IceBlue, progress);
                    CustomParticles.GenericFlare(orbitPos, orbitColor * 0.6f, 0.25f, 12);
                }
            }

            // Prismatic sparkle aura
            if (Main.rand.NextBool(4))
            {
                Vector2 offset = Main.rand.NextVector2Circular(28f, 28f);
                Color gradientColor = Color.Lerp(MoonlightSonataPalette.Violet, MoonlightSonataPalette.IceBlue, Main.rand.NextFloat());
                CustomParticles.PrismaticSparkle(player.Center + offset, gradientColor * 0.6f, 0.22f);

                var sparkle = new SparkleParticle(player.Center + offset, Main.rand.NextVector2Circular(0.5f, 0.5f),
                    MoonlightSonataPalette.MoonWhite * 0.4f, 0.18f, 20);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }

            // Conductor's baton orbiting music notes
            if (Main.rand.NextBool(8))
            {
                float noteOrbit = Main.GameUpdateCount * 0.06f;

                for (int i = 0; i < 2; i++)
                {
                    float noteAngle = noteOrbit + MathHelper.Pi * i;
                    Vector2 notePos = player.Center + noteAngle.ToRotationVector2() * 22f;
                    MoonlightVFXLibrary.SpawnMusicNotes(notePos, 1, 2f, 0.75f, 0.9f, 35);

                    // Sparkle companion for visibility
                    CustomParticles.PrismaticSparkle(notePos + Main.rand.NextVector2Circular(4f, 4f),
                        MoonlightSonataPalette.MoonWhite * 0.4f, 0.15f);
                }
            }

            // Dense lunar dust
            if (Main.rand.NextBool(3))
            {
                Vector2 dustPos = player.Center + Main.rand.NextVector2Circular(35f, 35f);
                Dust dust = Dust.NewDustPerfect(dustPos, DustID.PurpleTorch,
                    Main.rand.NextVector2Circular(0.8f, 0.8f), 80, default, 1.1f);
                dust.noGravity = true;
                dust.fadeIn = 1.2f;
            }

            // Pulsing mystical glow
            float pulse = MathF.Sin(Main.GameUpdateCount * 0.05f) * 0.15f + 0.95f;
            Lighting.AddLight(player.Center, MoonlightSonataPalette.Violet.ToVector3() * pulse * 0.6f);
        }

        // =====================================================================
        //  PREDRAW BLOOM — 4-layer {A=0} bloom for world item rendering
        // =====================================================================

        /// <summary>
        /// 4-layer PreDrawInWorld bloom for the staff item lying in the world.
        /// Uses {A=0} alpha trick for additive rendering under AlphaBlend.
        /// </summary>
        public static void DrawWorldItemBloom(SpriteBatch sb, Texture2D texture,
            Vector2 position, Vector2 origin, float rotation, float scale)
        {
            float pulse = 1f + MathF.Sin(Main.GameUpdateCount * 0.04f) * 0.12f;

            // Layer 1: Outer deep indigo aura
            sb.Draw(texture, position, null,
                MoonlightSonataPalette.Additive(MoonlightSonataPalette.NightPurple, 0.35f),
                rotation, origin, scale * pulse * 1.35f, SpriteEffects.None, 0f);

            // Layer 2: Mid violet glow
            sb.Draw(texture, position, null,
                MoonlightSonataPalette.Additive(MoonlightSonataPalette.Violet, 0.30f),
                rotation, origin, scale * pulse * 1.18f, SpriteEffects.None, 0f);

            // Layer 3: Inner ice blue
            sb.Draw(texture, position, null,
                MoonlightSonataPalette.Additive(MoonlightSonataPalette.IceBlue, 0.25f),
                rotation, origin, scale * pulse * 1.06f, SpriteEffects.None, 0f);

            // Layer 4: White core
            sb.Draw(texture, position, null,
                MoonlightSonataPalette.Additive(Color.White, 0.18f),
                rotation, origin, scale * pulse, SpriteEffects.None, 0f);

            Lighting.AddLight(position + Main.screenPosition, MoonlightSonataPalette.Violet.ToVector3() * 0.45f);
        }

        // =====================================================================
        //  SUMMONING RITUAL VFX — grand ceremony when Goliath is summoned
        // =====================================================================

        /// <summary>
        /// Grand summoning ritual VFX at the spawn position.
        /// Produces central flash cascade, magic circle, lunar phase symbols,
        /// halo ring cascade, radial spark burst, GodRay burst, screen distortion,
        /// and a music note scatter.
        /// </summary>
        public static void SummoningRitualVFX(Vector2 position)
        {
            if (Main.dedServ) return;

            // === Central flash cascade ===
            CustomParticles.GenericFlare(position, Color.White, 1.0f, 22);
            CustomParticles.GenericFlare(position, MoonlightSonataPalette.MoonWhite, 0.8f, 20);
            CustomParticles.GenericFlare(position, MoonlightSonataPalette.Violet, 0.6f, 18);

            // === Magic circle — 6 glyph flares in expanding ring ===
            float magicCircleAngle = Main.GameUpdateCount * 0.05f;
            for (int i = 0; i < 6; i++)
            {
                float glyphAngle = magicCircleAngle + MathHelper.TwoPi * i / 6f;
                Vector2 glyphPos = position + glyphAngle.ToRotationVector2() * 50f;
                Color glyphColor = Color.Lerp(MoonlightSonataPalette.DarkPurple, MoonlightSonataPalette.IceBlue, (float)i / 6f);
                CustomParticles.GenericFlare(glyphPos, glyphColor, 0.4f, 18);
            }

            // === 8 lunar phase symbols — crescent flares at different orientations ===
            for (int i = 0; i < 8; i++)
            {
                float phaseAngle = MathHelper.TwoPi * i / 8f;
                Vector2 phasePos = position + phaseAngle.ToRotationVector2() * 35f;
                Color phaseColor = Color.Lerp(MoonlightSonataPalette.Lavender, MoonlightSonataPalette.MoonWhite, (float)i / 8f);
                CustomParticles.GenericFlare(phasePos, phaseColor, 0.3f, 16);
            }

            // === Halo ring cascade ===
            for (int ring = 0; ring < 3; ring++)
            {
                Color ringColor = Color.Lerp(MoonlightSonataPalette.DarkPurple, MoonlightSonataPalette.MoonWhite, ring / 3f);
                CustomParticles.HaloRing(position, ringColor, 0.5f + ring * 0.2f, 20 + ring * 5);
            }

            // === Radial spark burst ===
            for (int i = 0; i < 12; i++)
            {
                float angle = MathHelper.TwoPi * i / 12f;
                Vector2 sparkVel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 8f);
                Color sparkColor = Color.Lerp(MoonlightSonataPalette.Violet, MoonlightSonataPalette.IceBlue, (float)i / 12f);
                var spark = new SparkleParticle(position, sparkVel, sparkColor, 0.3f, 20);
                MagnumParticleHandler.SpawnParticle(spark);
            }

            // === GodRay burst — summoning completion ===
            GodRaySystem.CreateBurst(position, MoonlightSonataPalette.Violet, 6, 60f, 22,
                GodRaySystem.GodRayStyle.Explosion, MoonlightSonataPalette.IceBlue);

            // === Screen distortion ===
            if (AdaptiveQualityManager.Instance?.CurrentQuality >= AdaptiveQualityManager.QualityLevel.Medium)
            {
                ScreenDistortionManager.TriggerRipple(position, MoonlightSonataPalette.Violet, 0.4f, 20);
            }

            // === Music notes — the summoning song ===
            MoonlightVFXLibrary.SpawnMusicNotes(position, 8, 50f, 0.8f, 1.1f, 35);
        }
    }
}
