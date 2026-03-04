using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using System;

namespace MagnumOpus.Content.Nachtmusik.Weapons.StarweaversGrimoire.Utilities
{
    public static class StarweaversGrimoireVFX
    {
        // Nachtmusik palette
        private static readonly Color NightVoid = new Color(10, 10, 30);
        private static readonly Color DeepIndigo = new Color(40, 30, 100);
        private static readonly Color CosmicBlue = new Color(60, 80, 180);
        private static readonly Color StarlightSilver = new Color(180, 200, 230);
        private static readonly Color MoonPearl = new Color(220, 225, 245);
        private static readonly Color StellarWhite = new Color(240, 245, 255);

        public static void HoldItemVFX(Player player, float weaveProgress)
        {
            if (Main.rand.NextFloat() > 0.3f + weaveProgress * 0.4f) return;
            
            // Ambient arcane motes orbit the book
            float angle = Main.GameUpdateCount * 0.03f + Main.rand.NextFloat() * MathHelper.TwoPi;
            float radius = 30f + weaveProgress * 20f;
            Vector2 offset = new Vector2((float)Math.Cos(angle) * radius, (float)Math.Sin(angle) * radius);
            
            int dust = Dust.NewDust(player.Center + offset - new Vector2(2), 4, 4, DustID.PurificationPowder,
                0f, 0f, 200, Color.Lerp(DeepIndigo, CosmicBlue, Main.rand.NextFloat()), 0.5f + weaveProgress * 0.3f);
            Main.dust[dust].noGravity = true;
            Main.dust[dust].velocity = offset * -0.02f; // Drift toward center
            
            // Constellation thread particles at high weave
            if (weaveProgress > 0.5f && Main.rand.NextBool(3))
            {
                Vector2 threadStart = player.Center + Main.rand.NextVector2CircularEdge(40f, 40f);
                Vector2 threadEnd = player.Center + Main.rand.NextVector2CircularEdge(40f, 40f);
                Vector2 midpoint = (threadStart + threadEnd) / 2f;
                
                int d = Dust.NewDust(midpoint - new Vector2(2), 4, 4, DustID.MagicMirror,
                    0f, 0f, 150, StarlightSilver, 0.4f);
                Main.dust[d].noGravity = true;
                Main.dust[d].velocity = Vector2.Zero;
            }
            
            Lighting.AddLight(player.Center, 0.15f + weaveProgress * 0.1f, 0.18f + weaveProgress * 0.12f, 0.4f + weaveProgress * 0.2f);
        }

        public static void CastVFX(Vector2 position, Vector2 direction)
        {
            // Arcane casting burst
            for (int i = 0; i < 8; i++)
            {
                Vector2 vel = direction.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(2f, 6f);
                vel = vel.RotatedByRandom(MathHelper.ToRadians(25));
                Color color = i % 2 == 0 ? CosmicBlue : StarlightSilver;
                int d = Dust.NewDust(position, 0, 0, DustID.MagicMirror, vel.X, vel.Y, 100, color, 0.9f);
                Main.dust[d].noGravity = true;
            }
            
            // Constellation glyph spark
            for (int i = 0; i < 3; i++)
            {
                Vector2 sparkVel = direction.SafeNormalize(Vector2.Zero).RotatedByRandom(MathHelper.ToRadians(40)) * Main.rand.NextFloat(1f, 3f);
                int d = Dust.NewDust(position, 0, 0, DustID.PurificationPowder, sparkVel.X, sparkVel.Y, 180, MoonPearl, 0.6f);
                Main.dust[d].noGravity = true;
            }
        }

        public static void OrbImpactVFX(Vector2 position, Vector2 velocity)
        {
            // Star flash
            for (int i = 0; i < 10; i++)
            {
                Vector2 vel = Main.rand.NextVector2CircularEdge(5f, 5f);
                Color color = Main.rand.NextBool() ? CosmicBlue : DeepIndigo;
                int d = Dust.NewDust(position, 0, 0, DustID.MagicMirror, vel.X, vel.Y, 120, color, 1f);
                Main.dust[d].noGravity = true;
            }
            
            // Central flash
            for (int i = 0; i < 3; i++)
            {
                int d = Dust.NewDust(position - new Vector2(4), 8, 8, DustID.PurificationPowder, 0f, 0f, 100, StellarWhite, 1.2f);
                Main.dust[d].noGravity = true;
                Main.dust[d].velocity *= 0.1f;
            }
        }

        public static void TapestryWeaveVFX(Vector2 center)
        {
            // Massive constellation burst
            for (int i = 0; i < 30; i++)
            {
                float angle = MathHelper.TwoPi / 30f * i;
                Vector2 vel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * Main.rand.NextFloat(4f, 10f);
                Color color = i % 3 == 0 ? StellarWhite : (i % 3 == 1 ? CosmicBlue : DeepIndigo);
                int d = Dust.NewDust(center, 0, 0, DustID.MagicMirror, vel.X, vel.Y, 80, color, 1.2f);
                Main.dust[d].noGravity = true;
            }
            
            // Expanding ring of constellation motes
            for (int i = 0; i < 12; i++)
            {
                float angle = MathHelper.TwoPi / 12f * i;
                Vector2 pos = center + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 60f;
                int d = Dust.NewDust(pos - new Vector2(2), 4, 4, DustID.PurificationPowder, 0f, 0f, 150, StarlightSilver, 0.8f);
                Main.dust[d].noGravity = true;
            }
        }
    }
}
