using Microsoft.Xna.Framework;
using Terraria;

namespace MagnumOpus.Content.Fate.ResonantWeapons.SymphonysEnd
{
    // ═══════════ PARTICLE TYPE ENUM ═══════════

    /// <summary>Six visual archetypes for Symphony's End particles.</summary>
    public enum SymphonyParticleType
    {
        /// <summary>Tiny fast spark — trailing blade edges.</summary>
        Spark,
        /// <summary>Soft glow circle — ambient fills and bloom accents.</summary>
        Glow,
        /// <summary>Music note motif — rendered as a tinted glow.</summary>
        Note,
        /// <summary>Expanding ring — shatter impact halos.</summary>
        Ring,
        /// <summary>Tiny blade shard — shatter debris.</summary>
        Shard,
        /// <summary>Electrical crackle — wand-tip energy.</summary>
        Crackle
    }

    // ═══════════ FACTORY METHODS ═══════════

    /// <summary>
    /// Convenience factory for all 6 particle types + composite spawn helpers.
    /// Every method returns a ready-to-spawn <see cref="SymphonyParticle"/> or
    /// directly spawns via <see cref="SymphonyParticleHandler"/>.
    /// </summary>
    public static class SymphonyParticleFactory
    {
        // ─── Individual Factories ─────────────────────────────────

        public static SymphonyParticle Spark(Vector2 pos, Vector2 vel,
            Color? color = null, float scale = 0.08f, int lifetime = 15)
            => new SymphonyParticle
            {
                Position = pos,
                Velocity = vel,
                Color = color ?? SymphonyUtils.SymphonyPink,
                Scale = scale,
                Rotation = vel.ToRotation(),
                RotationSpeed = 0f,
                TimeLeft = lifetime,
                MaxTime = lifetime,
                Type = SymphonyParticleType.Spark,
                Active = true,
                Opacity = 1f,
                Additive = true
            };

        public static SymphonyParticle Glow(Vector2 pos, Vector2 vel,
            Color? color = null, float scale = 0.15f, int lifetime = 20)
            => new SymphonyParticle
            {
                Position = pos,
                Velocity = vel,
                Color = color ?? SymphonyUtils.HarmonyBlue,
                Scale = scale,
                Rotation = 0f,
                RotationSpeed = 0f,
                TimeLeft = lifetime,
                MaxTime = lifetime,
                Type = SymphonyParticleType.Glow,
                Active = true,
                Opacity = 1f,
                Additive = true
            };

        public static SymphonyParticle Note(Vector2 pos, Vector2 vel,
            Color? color = null, float scale = 0.2f, int lifetime = 25)
            => new SymphonyParticle
            {
                Position = pos,
                Velocity = vel,
                Color = color ?? SymphonyUtils.SymphonyViolet,
                Scale = scale,
                Rotation = Main.rand.NextFloat(MathHelper.TwoPi),
                RotationSpeed = Main.rand.NextFloat(-0.05f, 0.05f),
                TimeLeft = lifetime,
                MaxTime = lifetime,
                Type = SymphonyParticleType.Note,
                Active = true,
                Opacity = 1f,
                Additive = true
            };

        public static SymphonyParticle Ring(Vector2 pos,
            Color? color = null, float scale = 0.1f, int lifetime = 18)
            => new SymphonyParticle
            {
                Position = pos,
                Velocity = Vector2.Zero,
                Color = color ?? SymphonyUtils.FinalWhite,
                Scale = scale,
                Rotation = 0f,
                RotationSpeed = 0f,
                TimeLeft = lifetime,
                MaxTime = lifetime,
                Type = SymphonyParticleType.Ring,
                Active = true,
                Opacity = 1f,
                Additive = true
            };

        public static SymphonyParticle Shard(Vector2 pos, Vector2 vel,
            Color? color = null, float scale = 0.06f, int lifetime = 12)
            => new SymphonyParticle
            {
                Position = pos,
                Velocity = vel,
                Color = color ?? SymphonyUtils.DiscordRed,
                Scale = scale,
                Rotation = vel.ToRotation(),
                RotationSpeed = Main.rand.NextFloat(-0.15f, 0.15f),
                TimeLeft = lifetime,
                MaxTime = lifetime,
                Type = SymphonyParticleType.Shard,
                Active = true,
                Opacity = 1f,
                Additive = true
            };

        public static SymphonyParticle Crackle(Vector2 pos, Vector2 vel,
            Color? color = null, float scale = 0.12f, int lifetime = 10)
            => new SymphonyParticle
            {
                Position = pos,
                Velocity = vel,
                Color = color ?? SymphonyUtils.SymphonyPink,
                Scale = scale,
                Rotation = Main.rand.NextFloat(MathHelper.TwoPi),
                RotationSpeed = Main.rand.NextFloat(-0.2f, 0.2f),
                TimeLeft = lifetime,
                MaxTime = lifetime,
                Type = SymphonyParticleType.Crackle,
                Active = true,
                Opacity = 1f,
                Additive = true
            };

        // ─── Composite Spawn Helpers ──────────────────────────────

        /// <summary>Trail particles behind a spiraling blade (called per-frame).</summary>
        public static void SpawnSpiralTrailParticles(Vector2 pos, Vector2 vel)
        {
            Vector2 trailDir = -vel.SafeNormalize(Vector2.UnitX);

            // Glow in Symphony gradient
            SymphonyParticleHandler.Spawn(Glow(
                pos + Main.rand.NextVector2Circular(4f, 4f),
                trailDir * 1.2f + Main.rand.NextVector2Circular(0.5f, 0.5f),
                SymphonyUtils.GetSymphonyGradient(Main.rand.NextFloat()),
                0.12f, 16));

            // Alternating sparks
            if (Main.rand.NextBool(2))
            {
                float sparkAngle = vel.ToRotation() + Main.rand.NextFloat(-0.8f, 0.8f);
                SymphonyParticleHandler.Spawn(Spark(
                    pos,
                    sparkAngle.ToRotationVector2() * Main.rand.NextFloat(2f, 4f),
                    SymphonyUtils.RandomPaletteColor(),
                    0.06f, 12));
            }
        }

        /// <summary>Multi-layered shatter burst (rings + shards + glows + notes).</summary>
        public static void SpawnShatterBurst(Vector2 pos, int shardCount = 12)
        {
            // Expanding halos
            SymphonyParticleHandler.Spawn(Ring(pos, SymphonyUtils.FinalWhite, 0.15f, 20));
            SymphonyParticleHandler.Spawn(Ring(pos, SymphonyUtils.SymphonyPink * 0.8f, 0.1f, 24));

            // Radiating shards
            for (int i = 0; i < shardCount; i++)
            {
                float angle = MathHelper.TwoPi * i / shardCount + Main.rand.NextFloat(-0.2f, 0.2f);
                float speed = Main.rand.NextFloat(3f, 7f);
                Vector2 vel = angle.ToRotationVector2() * speed;
                Color col = SymphonyUtils.GetSymphonyGradient((float)i / shardCount);
                SymphonyParticleHandler.Spawn(Shard(pos, vel, col, 0.08f, 16));
            }

            // Ambient glows
            for (int i = 0; i < 6; i++)
            {
                SymphonyParticleHandler.Spawn(Glow(
                    pos + Main.rand.NextVector2Circular(8f, 8f),
                    Main.rand.NextVector2Circular(2f, 2f),
                    SymphonyUtils.FinalWhite * 0.7f,
                    0.2f, 18));
            }

            // Music notes — the final chord
            for (int i = 0; i < 3; i++)
            {
                SymphonyParticleHandler.Spawn(Note(
                    pos,
                    Main.rand.NextVector2Circular(3f, 3f) + new Vector2(0, -1f),
                    SymphonyUtils.HarmonyBlue,
                    0.18f, 30));
            }
        }

        /// <summary>Wand-tip crackle aura (called per-frame while holding).</summary>
        public static void SpawnCrackleAura(Vector2 pos, float intensity)
        {
            if (Main.rand.NextFloat() > intensity * 0.5f) return;

            float angle = Main.rand.NextFloat(MathHelper.TwoPi);
            float dist = Main.rand.NextFloat(8f, 20f);
            Vector2 cracklePos = pos + angle.ToRotationVector2() * dist;

            SymphonyParticleHandler.Spawn(Crackle(
                cracklePos,
                (cracklePos - pos).SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(0.5f, 2f),
                Color.Lerp(SymphonyUtils.SymphonyViolet, SymphonyUtils.SymphonyPink, Main.rand.NextFloat()),
                0.1f * intensity, 8));

            if (Main.rand.NextBool(3))
            {
                SymphonyParticleHandler.Spawn(Spark(
                    cracklePos,
                    Main.rand.NextVector2Circular(3f, 3f),
                    SymphonyUtils.FinalWhite * 0.6f,
                    0.04f, 8));
            }
        }
    }
}
