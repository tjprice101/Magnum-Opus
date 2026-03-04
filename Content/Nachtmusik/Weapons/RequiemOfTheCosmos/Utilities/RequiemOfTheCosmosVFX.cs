using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using System;

namespace MagnumOpus.Content.Nachtmusik.Weapons.RequiemOfTheCosmos.Utilities
{
    public static class RequiemOfTheCosmosVFX
    {
        private static readonly Color NightVoid = new Color(10, 10, 30);
        private static readonly Color DeepIndigo = new Color(40, 30, 100);
        private static readonly Color CosmicBlue = new Color(60, 80, 180);
        private static readonly Color StarlightSilver = new Color(180, 200, 230);
        private static readonly Color MoonPearl = new Color(220, 225, 245);
        private static readonly Color StellarWhite = new Color(240, 245, 255);

        public static void HoldItemVFX(Player player, float chargeProgress)
        {
            if (Main.rand.NextFloat() > 0.2f + chargeProgress * 0.5f) return;
            
            // Cosmic weight aura - intensifies as Event Horizon approaches
            float radius = 25f + chargeProgress * 35f;
            float angle = Main.GameUpdateCount * 0.04f + Main.rand.NextFloat() * MathHelper.TwoPi;
            Vector2 offset = new Vector2((float)Math.Cos(angle) * radius, (float)Math.Sin(angle) * radius);
            
            Color color = Color.Lerp(DeepIndigo, CosmicBlue, chargeProgress);
            int d = Dust.NewDust(player.Center + offset - new Vector2(2), 4, 4, DustID.MagicMirror,
                0f, 0f, 180, color, 0.6f + chargeProgress * 0.6f);
            Main.dust[d].noGravity = true;
            Main.dust[d].velocity = offset * -0.03f; // Slow inward spiral
            
            // Event Horizon warning: void particles pulling inward at high charge
            if (chargeProgress > 0.7f)
            {
                Vector2 voidOffset = Main.rand.NextVector2CircularEdge(50f, 50f);
                int d2 = Dust.NewDust(player.Center + voidOffset - new Vector2(2), 4, 4, DustID.MagicMirror,
                    0f, 0f, 200, NightVoid, 0.8f);
                Main.dust[d2].noGravity = true;
                Main.dust[d2].velocity = (player.Center - (player.Center + voidOffset)) * 0.05f;
            }
            
            Lighting.AddLight(player.Center, 0.1f + chargeProgress * 0.2f, 0.12f + chargeProgress * 0.18f, 0.35f + chargeProgress * 0.35f);
        }

        public static void CastVFX(Vector2 position, Vector2 direction)
        {
            for (int i = 0; i < 10; i++)
            {
                Vector2 vel = direction.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(3f, 8f);
                vel = vel.RotatedByRandom(MathHelper.ToRadians(20));
                Color color = i % 3 == 0 ? StarlightSilver : (i % 2 == 0 ? CosmicBlue : DeepIndigo);
                int d = Dust.NewDust(position, 0, 0, DustID.MagicMirror, vel.X, vel.Y, 100, color, 1f);
                Main.dust[d].noGravity = true;
            }
        }

        public static void OrbImpactVFX(Vector2 position, bool isGravityWell)
        {
            int count = isGravityWell ? 18 : 12;
            for (int i = 0; i < count; i++)
            {
                Vector2 vel = Main.rand.NextVector2CircularEdge(6f, 6f) * (isGravityWell ? 0.5f : 1f);
                if (isGravityWell) vel *= -1; // Particles implode inward
                Color color = Main.rand.NextBool() ? CosmicBlue : DeepIndigo;
                int d = Dust.NewDust(position, 0, 0, DustID.MagicMirror, vel.X, vel.Y, 100, color, 1.1f);
                Main.dust[d].noGravity = true;
            }
            
            // Central flash
            for (int i = 0; i < 3; i++)
            {
                int d = Dust.NewDust(position - new Vector2(4), 8, 8, DustID.PurificationPowder, 0f, 0f, 80, StellarWhite, 1.5f);
                Main.dust[d].noGravity = true;
                Main.dust[d].velocity *= 0.05f;
            }
        }

        public static void EventHorizonCastVFX(Vector2 center)
        {
            // Massive void collapse casting effect
            for (int i = 0; i < 40; i++)
            {
                Vector2 outerPos = center + Main.rand.NextVector2CircularEdge(80f, 80f);
                Vector2 inwardVel = (center - outerPos) * 0.08f;
                Color color = i % 4 == 0 ? StellarWhite : (i % 2 == 0 ? CosmicBlue : NightVoid);
                int d = Dust.NewDust(outerPos, 0, 0, DustID.MagicMirror, inwardVel.X, inwardVel.Y, 100, color, 1.3f);
                Main.dust[d].noGravity = true;
            }
            
            // Cosmic shockwave ring
            for (int i = 0; i < 20; i++)
            {
                float angle = MathHelper.TwoPi / 20f * i;
                Vector2 vel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 8f;
                int d = Dust.NewDust(center, 0, 0, DustID.PurificationPowder, vel.X, vel.Y, 80, MoonPearl, 1.5f);
                Main.dust[d].noGravity = true;
            }
        }

        public static void EventHorizonImpactVFX(Vector2 position)
        {
            // Devastating singularity collapse
            for (int i = 0; i < 35; i++)
            {
                Vector2 vel = Main.rand.NextVector2CircularEdge(8f, 8f) * Main.rand.NextFloat(0.3f, 1.2f);
                Color color = i % 4 == 0 ? StellarWhite : (i % 3 == 0 ? StarlightSilver : (i % 2 == 0 ? CosmicBlue : NightVoid));
                int d = Dust.NewDust(position, 0, 0, DustID.MagicMirror, vel.X, vel.Y, 60, color, 1.5f);
                Main.dust[d].noGravity = true;
            }
            
            // Void flash core
            for (int i = 0; i < 5; i++)
            {
                int d = Dust.NewDust(position - new Vector2(6), 12, 12, DustID.PurificationPowder, 0f, 0f, 50, StellarWhite, 2f);
                Main.dust[d].noGravity = true;
                Main.dust[d].velocity *= 0.02f;
            }
        }
    }
}
