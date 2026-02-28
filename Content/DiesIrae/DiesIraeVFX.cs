using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;

namespace MagnumOpus.Content.DiesIrae
{
    /// <summary>
    /// Minimal VFX stub for DiesIraeHeraldOfJudgement boss.
    /// Provides simple vanilla dust effects as a bridge until full VFX are implemented.
    /// </summary>
    public static class DiesIraeVFX
    {
        public static void ChargeUp(Vector2 position, float progress, float scale)
        {
            int count = (int)(4 * scale);
            for (int i = 0; i < count; i++)
            {
                float angle = Main.rand.NextFloat() * MathHelper.TwoPi;
                float dist = (1f - progress) * 80f + 20f;
                Vector2 offset = angle.ToRotationVector2() * dist;
                Vector2 velocity = -offset * 0.05f;
                Dust dust = Dust.NewDustDirect(position + offset, 0, 0, DustID.Torch, velocity.X, velocity.Y, 100, default, 1.5f * scale);
                dust.noGravity = true;
            }
        }

        public static void FireImpact(Vector2 position, float scale)
        {
            int count = (int)(12 * scale);
            for (int i = 0; i < count; i++)
            {
                Vector2 velocity = Main.rand.NextVector2CircularEdge(4f, 4f) * scale;
                int type = Main.rand.NextBool() ? DustID.Torch : DustID.InfernoFork;
                Dust dust = Dust.NewDustDirect(position, 0, 0, type, velocity.X, velocity.Y, 80, default, 2f * scale);
                dust.noGravity = true;
            }
        }

        public static void WarningFlare(Vector2 position, float scale)
        {
            for (int i = 0; i < 6; i++)
            {
                Vector2 velocity = Main.rand.NextVector2Circular(2f, 2f);
                Dust dust = Dust.NewDustDirect(position, 0, 0, DustID.Torch, velocity.X, velocity.Y, 150, default, 1.8f * scale);
                dust.noGravity = true;
                dust.fadeIn = 1.2f;
            }
        }

        public static void FireTrail(Vector2 position, Vector2 velocity, float scale)
        {
            for (int i = 0; i < 3; i++)
            {
                Vector2 dustVel = velocity * 0.3f + Main.rand.NextVector2Circular(1f, 1f);
                Dust dust = Dust.NewDustDirect(position, 0, 0, DustID.Torch, dustVel.X, dustVel.Y, 100, default, 1.2f * scale);
                dust.noGravity = true;
            }
        }

        public static void DeathExplosion(Vector2 position, float scale)
        {
            int count = (int)(20 * scale);
            for (int i = 0; i < count; i++)
            {
                Vector2 velocity = Main.rand.NextVector2CircularEdge(6f, 6f) * scale;
                int type = Main.rand.Next(3) == 0 ? DustID.InfernoFork : DustID.Torch;
                Dust dust = Dust.NewDustDirect(position, 0, 0, type, velocity.X, velocity.Y, 60, default, 2.5f * scale);
                dust.noGravity = true;
                dust.fadeIn = 1.5f;
            }
        }

        public static void SpawnMusicNote(Vector2 position, Vector2 velocity, Color color, float scale)
        {
            for (int i = 0; i < 2; i++)
            {
                Vector2 dustVel = velocity + Main.rand.NextVector2Circular(1.5f, 1.5f);
                Dust dust = Dust.NewDustDirect(position, 0, 0, DustID.Torch, dustVel.X, dustVel.Y, 120, default, 1.5f * scale);
                dust.noGravity = true;
                dust.fadeIn = 1.3f;
            }
        }
    }

    /// <summary>
    /// Stub for Dies Irae color palette used by the boss.
    /// </summary>
    public static class DiesIraeColors
    {
        public static Color Crimson => new Color(139, 0, 0);
        public static Color BloodRed => new Color(120, 20, 20);
        public static Color DarkCrimson => new Color(80, 10, 10);
        public static Color Ember => new Color(200, 80, 40);
        public static Color Ash => new Color(60, 50, 50);
        public static Color Hellfire => new Color(255, 100, 50);
    }
}
