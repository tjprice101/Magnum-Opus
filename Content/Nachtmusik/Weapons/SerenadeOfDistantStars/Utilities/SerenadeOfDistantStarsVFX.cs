using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Content.Nachtmusik.Weapons.SerenadeOfDistantStars.Utilities
{
    /// <summary>
    /// Static VFX helper for Serenade of Distant Stars — the romantic homing star rifle.
    /// Stellar muzzle flash, rhythm-scaling starlight aura, Star Memory echo flash,
    /// and kill cascade nova with expanding stellar rings.
    /// </summary>
    public static class SerenadeOfDistantStarsVFX
    {
        // =====================================================================
        //  MuzzleFlashVFX — Stellar flash burst with star sparkles
        // =====================================================================
        public static void MuzzleFlashVFX(Vector2 muzzlePos, Vector2 direction)
        {
            // === MELODIC ARC SPRAY — golden stars in musical arc ===
            for (int i = 0; i < 7; i++)
            {
                float arcT = (i / 6f) * 2f - 1f;
                float spread = MathHelper.ToRadians(20f) * arcT;
                Vector2 vel = direction.RotatedBy(spread) * (3.5f + Math.Abs(arcT) * 1.5f);
                Dust d = Dust.NewDustPerfect(muzzlePos, DustID.GoldFlame, vel, 0, default, 0.85f);
                d.noGravity = true;
                d.fadeIn = 0.9f;
            }

            // Blue harmony accent sparks
            for (int i = 0; i < 4; i++)
            {
                Vector2 vel = direction.RotatedBy(Main.rand.NextFloat(-0.3f, 0.3f)) * 2.5f
                    + Main.rand.NextVector2Circular(1f, 1f);
                Dust d = Dust.NewDustPerfect(muzzlePos, DustID.BlueTorch, vel, 0, default, 0.55f);
                d.noGravity = true;
            }

            // Star sparkle accents
            for (int i = 0; i < 3; i++)
            {
                var sparkle = new SparkleParticle(
                    muzzlePos + Main.rand.NextVector2Circular(8f, 8f),
                    direction * Main.rand.NextFloat(1f, 3f),
                    NachtmusikPalette.StarWhite, 0.3f, 14);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }

            NachtmusikVFXLibrary.SpawnMusicNotes(muzzlePos, 2, 16f, 0.5f, 0.8f, 24);
            NachtmusikVFXLibrary.DrawBloom(muzzlePos, 0.4f, 0.7f);
            NachtmusikVFXLibrary.AddPaletteLighting(muzzlePos, 0.5f, 0.55f);
        }

        // =====================================================================
        //  HoldItemVFX — Romantic starlight aura scaling with rhythm stacks
        // =====================================================================
        public static void HoldItemVFX(Player player, int rhythmStacks)
        {
            float time = (float)Main.timeForVisualEffects * 0.035f;
            float stackIntensity = rhythmStacks / 5f; // 0..1

            // === INNER GOLDEN ORRERY ORBIT — scales with stacks ===
            if (Main.rand.NextBool(Math.Max(1, 4 - rhythmStacks)))
            {
                float innerAngle = time * (2f + stackIntensity * 1.5f) + Main.rand.NextFloat() * 0.4f;
                float innerRadius = 12f + (float)Math.Sin(time * 3f) * 3f + stackIntensity * 6f;
                Vector2 innerPos = player.Center + new Vector2(
                    (float)Math.Cos(innerAngle) * innerRadius,
                    (float)Math.Sin(innerAngle) * innerRadius * 0.5f);
                Vector2 tangent = new Vector2(-(float)Math.Sin(innerAngle), (float)Math.Cos(innerAngle)) * 0.4f;

                Dust inner = Dust.NewDustPerfect(innerPos, DustID.GoldFlame, tangent, 0, default,
                    0.4f + stackIntensity * 0.3f);
                inner.noGravity = true;
                inner.fadeIn = 0.7f;
            }

            // === OUTER BLUE ACCENT ORBIT ===
            if (Main.rand.NextBool(Math.Max(1, 5 - rhythmStacks)))
            {
                float outerAngle = time * 0.9f + MathHelper.Pi;
                float outerRadius = 24f + (float)Math.Sin(time * 1.5f) * 5f + stackIntensity * 8f;
                Vector2 outerPos = player.Center + new Vector2(
                    (float)Math.Cos(outerAngle) * outerRadius,
                    (float)Math.Sin(outerAngle) * outerRadius * 0.6f);
                Vector2 tangent = new Vector2(-(float)Math.Sin(outerAngle), (float)Math.Cos(outerAngle)) * 0.25f;

                Dust outer = Dust.NewDustPerfect(outerPos, DustID.BlueTorch, tangent, 0, default,
                    0.3f + stackIntensity * 0.2f);
                outer.noGravity = true;
                outer.fadeIn = 0.6f;
            }

            // === ASCENDING MELODY NOTES — more frequent at higher stacks ===
            if (Main.rand.NextBool(Math.Max(2, 7 - rhythmStacks * 2)))
            {
                Vector2 notePos = player.Center + new Vector2(Main.rand.NextFloat(-20f, 20f), 14f);
                Vector2 noteVel = new Vector2(Main.rand.NextFloat(-0.3f, 0.3f), -0.7f);
                Dust note = Dust.NewDustPerfect(notePos, DustID.GoldFlame, noteVel, 0, default, 0.4f);
                note.noGravity = true;
                note.fadeIn = 0.6f;
            }

            // At max stacks: radiance shimmer ring
            if (rhythmStacks >= 5 && Main.rand.NextBool(3))
            {
                float ringAngle = Main.rand.NextFloat() * MathHelper.TwoPi;
                float ringRadius = 30f + (float)Math.Sin(time * 4f) * 5f;
                Vector2 ringPos = player.Center + ringAngle.ToRotationVector2() * ringRadius;
                Dust ring = Dust.NewDustPerfect(ringPos, DustID.Enchanted_Gold,
                    Vector2.Zero, 0, default, 0.5f);
                ring.noGravity = true;
                ring.fadeIn = 0.5f;
            }

            // Warm starlight twinkle
            if (Main.rand.NextBool(Math.Max(3, 8 - rhythmStacks)))
            {
                NachtmusikVFXLibrary.SpawnTwinklingStars(player.Center, 1, 22f);
            }

            Lighting.AddLight(player.Center,
                NachtmusikPalette.StarGold.ToVector3() * (0.2f + stackIntensity * 0.15f));
        }

        // =====================================================================
        //  StarMemoryEchoVFX — Flash when echo fires
        // =====================================================================
        public static void StarMemoryEchoVFX(Vector2 pos)
        {
            // Quick gold flash
            CustomParticles.GenericFlare(pos, NachtmusikPalette.StarGold, 0.3f, 10);

            // Small directional spark burst
            for (int i = 0; i < 3; i++)
            {
                float angle = Main.rand.NextFloat() * MathHelper.TwoPi;
                Vector2 vel = angle.ToRotationVector2() * (2f + Main.rand.NextFloat() * 2f);
                Dust d = Dust.NewDustPerfect(pos, DustID.GoldFlame, vel, 0, default, 0.5f);
                d.noGravity = true;
            }

            NachtmusikVFXLibrary.AddPaletteLighting(pos, 0.5f, 0.35f);
        }

        // =====================================================================
        //  StarlightSonataNova — Kill cascade nova with expanding stellar rings
        // =====================================================================
        public static void StarlightSonataNova(Vector2 pos)
        {
            // === EXPANDING STELLAR RINGS ===
            for (int ring = 0; ring < 3; ring++)
            {
                float ringRadius = 20f + ring * 25f;
                int ringPoints = 12 + ring * 4;
                Color ringColor = NachtmusikPalette.PaletteLerp(
                    NachtmusikPalette.SerenadeOfDistantStarsShot, ring / 3f);

                for (int i = 0; i < ringPoints; i++)
                {
                    float angle = MathHelper.TwoPi * i / ringPoints;
                    Vector2 ringPos = pos + angle.ToRotationVector2() * ringRadius;
                    Vector2 vel = angle.ToRotationVector2() * (1f + ring * 0.5f);

                    var glow = new GenericGlowParticle(ringPos, vel,
                        ringColor * 0.7f, 0.2f, 20 + ring * 5, true);
                    MagnumParticleHandler.SpawnParticle(glow);
                }

                CustomParticles.HaloRing(pos, ringColor, 0.4f + ring * 0.15f, 14 + ring * 4);
            }

            // Central flash cascade
            CustomParticles.GenericFlare(pos, Color.White, 0.7f, 20);
            CustomParticles.GenericFlare(pos, NachtmusikPalette.StarGold, 0.6f, 18);
            CustomParticles.GenericFlare(pos, NachtmusikPalette.StarlitBlue, 0.5f, 16);

            // Starburst scatter
            NachtmusikVFXLibrary.SpawnStarBurst(pos, 8, 0.5f);
            NachtmusikVFXLibrary.SpawnShatteredStarlight(pos, 6, 5f, 0.8f, true);

            // Music note cascade
            NachtmusikVFXLibrary.SpawnMusicNotes(pos, 5, 30f, 0.6f, 0.9f, 28);
            NachtmusikVFXLibrary.DrawBloom(pos, 0.6f, 0.9f);

            Lighting.AddLight(pos, NachtmusikPalette.StarGold.ToVector3() * 1.0f);
        }
    }
}
