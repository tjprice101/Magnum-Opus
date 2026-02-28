using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;

namespace MagnumOpus.Content.OdeToJoy
{
    /// <summary>
    /// Shared Ode to Joy VFX library used by bosses, accessories, and tools.
    /// Weapons have their own per-weapon particle systems in their self-contained folders.
    /// These methods use vanilla dust as a lightweight shared system for non-weapon code.
    /// </summary>
    public static class OdeToJoyVFXLibrary
    {
        public static void SpawnRosePetals(Vector2 position, int count, float speed)
        {
            for (int i = 0; i < count; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(speed, speed);
                Color col = Color.Lerp(OdeToJoyPalette.RosePink, OdeToJoyPalette.PetalPink, Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(position, DustID.PinkFairy, vel, 0, col, Main.rand.NextFloat(0.8f, 1.4f));
                d.noGravity = true;
                d.fadeIn = 1.2f;
            }
        }

        public static void SpawnMusicNotes(Vector2 position, int count, float speed, float scale = 1f)
        {
            for (int i = 0; i < count; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(speed, speed) + new Vector2(0, -2f);
                Color col = Color.Lerp(OdeToJoyPalette.GoldenPollen, OdeToJoyPalette.SunlightYellow, Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(position, DustID.YellowStarDust, vel, 0, col, scale * Main.rand.NextFloat(0.6f, 1f));
                d.noGravity = true;
                d.fadeIn = 1.1f;
            }
        }

        public static void SpawnVineTrailDust(Vector2 position, Vector2 velocity)
        {
            Color col = Color.Lerp(OdeToJoyPalette.LeafGreen, OdeToJoyPalette.VerdantGreen, Main.rand.NextFloat());
            Dust d = Dust.NewDustPerfect(position, DustID.GreenFairy, velocity, 0, col, 0.8f);
            d.noGravity = true;
        }

        public static void SpawnPetalMusicNotes(Vector2 position, int count, float speed)
        {
            for (int i = 0; i < count; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(speed, speed) + new Vector2(0, -3f);
                Color col = Color.Lerp(OdeToJoyPalette.RosePink, OdeToJoyPalette.GoldenPollen, Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(position, DustID.PinkFairy, vel, 0, col, Main.rand.NextFloat(0.5f, 0.9f));
                d.noGravity = true;
                d.fadeIn = 0.9f;
            }
        }

        public static void SpawnPetalHaloRings(Vector2 position, int rings, float scale)
        {
            for (int r = 0; r < rings; r++)
            {
                float radius = 30f * (r + 1) * scale;
                int dustCount = 8 + r * 4;
                for (int i = 0; i < dustCount; i++)
                {
                    float angle = MathHelper.TwoPi * i / dustCount;
                    Vector2 offset = angle.ToRotationVector2() * radius;
                    Color col = OdeToJoyPalette.GetBlossomGradient((float)i / dustCount);
                    Dust d = Dust.NewDustPerfect(position + offset, DustID.PinkFairy, offset * 0.02f, 0, col, 0.6f * scale);
                    d.noGravity = true;
                }
            }
        }

        public static void SpawnPollenSparkles(Vector2 position, int count, float radius)
        {
            for (int i = 0; i < count; i++)
            {
                Vector2 offset = Main.rand.NextVector2Circular(radius, radius);
                Dust d = Dust.NewDustPerfect(position + offset, DustID.YellowStarDust, Vector2.Zero, 0, OdeToJoyPalette.GoldenPollen, 0.5f);
                d.noGravity = true;
                d.fadeIn = 0.8f;
            }
        }

        public static void SpawnGradientHaloRings(Vector2 position, int count, float scale)
        {
            for (int r = 0; r < count; r++)
            {
                float radius = 40f * (r + 1) * scale;
                int dustCount = 10 + r * 4;
                for (int i = 0; i < dustCount; i++)
                {
                    float angle = MathHelper.TwoPi * i / dustCount;
                    Vector2 offset = angle.ToRotationVector2() * radius;
                    Color col = OdeToJoyPalette.GetGardenGradient((float)i / dustCount);
                    Dust d = Dust.NewDustPerfect(position + offset, DustID.GreenFairy, offset * 0.01f, 0, col, 0.5f * scale);
                    d.noGravity = true;
                }
            }
        }

        public static void SpawnGardenAura(Vector2 position, float radius)
        {
            int count = (int)(radius / 5f);
            for (int i = 0; i < count; i++)
            {
                Vector2 offset = Main.rand.NextVector2Circular(radius, radius);
                Color col = OdeToJoyPalette.GetGardenGradient(Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(position + offset, DustID.GreenFairy, new Vector2(0, -0.5f), 0, col, 0.4f);
                d.noGravity = true;
                d.fadeIn = 0.7f;
            }
        }

        public static void GardenImpact(Vector2 position, float scale)
        {
            int count = (int)(10 * scale);
            for (int i = 0; i < count; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(6f * scale, 6f * scale);
                Color col = OdeToJoyPalette.GetGardenGradient(Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(position, DustID.GreenFairy, vel, 0, col, scale * 0.8f);
                d.noGravity = true;
            }
            SpawnPollenSparkles(position, (int)(4 * scale), 20f * scale);
        }

        public static void BlossomImpact(Vector2 position, float scale)
        {
            int count = (int)(12 * scale);
            for (int i = 0; i < count; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(8f * scale, 8f * scale);
                Color col = OdeToJoyPalette.GetBlossomGradient(Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(position, DustID.PinkFairy, vel, 0, col, scale * 0.9f);
                d.noGravity = true;
                d.fadeIn = 1.1f;
            }
            SpawnPetalMusicNotes(position, (int)(3 * scale), 4f * scale);
        }

        public static void BloomBurst(Vector2 position, float scale)
        {
            int count = (int)(16 * scale);
            for (int i = 0; i < count; i++)
            {
                float angle = MathHelper.TwoPi * i / count;
                Vector2 vel = angle.ToRotationVector2() * (6f * scale);
                Color col = Color.Lerp(OdeToJoyPalette.GoldenPollen, OdeToJoyPalette.WhiteBloom, Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(position, DustID.YellowStarDust, vel, 0, col, scale * 1.1f);
                d.noGravity = true;
            }
            BlossomImpact(position, scale * 0.6f);
        }

        public static void TriumphantCelebration(Vector2 position, float scale)
        {
            BloomBurst(position, scale);
            SpawnRosePetals(position, (int)(20 * scale), 12f * scale);
            SpawnMusicNotes(position, (int)(8 * scale), 8f * scale);
            SpawnPetalHaloRings(position, 3, scale * 0.5f);

            for (int i = 0; i < (int)(8 * scale); i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(10f * scale, 10f * scale);
                Dust d = Dust.NewDustPerfect(position, DustID.GoldFlame, vel, 0, OdeToJoyPalette.GoldenPollen, scale);
                d.noGravity = true;
            }
        }

        public static void DeathGardenFlash(Vector2 position, float scale)
        {
            TriumphantCelebration(position, scale * 1.5f);
            for (int i = 0; i < (int)(24 * scale); i++)
            {
                float angle = MathHelper.TwoPi * i / (int)(24 * scale);
                Vector2 vel = angle.ToRotationVector2() * (14f * scale);
                Dust d = Dust.NewDustPerfect(position, DustID.GoldFlame, vel, 0, OdeToJoyPalette.WhiteBloom, scale * 1.5f);
                d.noGravity = true;
            }
        }

        public static void MusicNoteBurst(Vector2 position, Color color, int count, float speed)
        {
            for (int i = 0; i < count; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(speed, speed) + new Vector2(0, -2f);
                Dust d = Dust.NewDustPerfect(position, DustID.YellowStarDust, vel, 0, color, Main.rand.NextFloat(0.5f, 1f));
                d.noGravity = true;
                d.fadeIn = 1f;
            }
        }

        public static void MeleeImpact(Vector2 position, int variant)
        {
            float scale = 0.8f + variant * 0.2f;
            GardenImpact(position, scale);
            SpawnRosePetals(position, 4 + variant * 2, 5f * scale);
        }

        public static void MusicalImpact(Vector2 position, float scale, bool withNotes)
        {
            BlossomImpact(position, scale);
            if (withNotes)
                SpawnMusicNotes(position, (int)(4 * scale), 6f * scale);
        }

        public static void ProjectileImpact(Vector2 position, float scale)
        {
            GardenImpact(position, scale * 0.7f);
            SpawnPollenSparkles(position, (int)(5 * scale), 15f * scale);
        }

        public static void FinisherSlam(Vector2 position, float scale)
        {
            TriumphantCelebration(position, scale);
            for (int i = 0; i < (int)(12 * scale); i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(8f * scale, 8f * scale);
                Color col = OdeToJoyPalette.GetPetalGradient(Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(position, DustID.PinkFairy, vel, 0, col, scale * 1.2f);
                d.noGravity = true;
            }
        }

        public static Color GetPaletteColor(float t)
        {
            return OdeToJoyPalette.GetGradient(t);
        }
    }
}
