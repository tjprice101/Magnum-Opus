using Microsoft.Xna.Framework;
using Terraria;

namespace MagnumOpus.Content.Fate.ResonantWeapons.DestinysCrescendo
{
    // ═══════════ PARTICLE TYPE ENUM ═══════════

    /// <summary>Six visual archetypes for Destiny's Crescendo particles.</summary>
    public enum CrescendoParticleType
    {
        /// <summary>Soft glowing orb — deity core radiance and ambient fills.</summary>
        OrbGlow,
        /// <summary>Sharp divine spark — slash impacts and beam edges.</summary>
        DivineSpark,
        /// <summary>Cosmic music note — the deity's eternal symphony.</summary>
        CosmicNote,
        /// <summary>Expanding glyph circle — summoning sigils and power rings.</summary>
        GlyphCircle,
        /// <summary>Bright beam flare — beam emission and endpoint flashes.</summary>
        BeamFlare,
        /// <summary>Drifting aura wisp — cosmic cloud tendrils around the deity.</summary>
        AuraWisp
    }

    // ═══════════ FACTORY METHODS ═══════════

    /// <summary>
    /// Convenience factory for all 6 particle types + composite spawn helpers.
    /// Every method returns a ready-to-spawn <see cref="CrescendoParticle"/> or
    /// directly spawns via <see cref="CrescendoParticleHandler"/>.
    /// </summary>
    public static class CrescendoParticleFactory
    {
        // ─── Individual Factories ─────────────────────────────────

        public static CrescendoParticle OrbGlow(Vector2 pos, Vector2 vel,
            Color? color = null, float scale = 0.2f, int lifetime = 20)
            => new CrescendoParticle
            {
                Position = pos,
                Velocity = vel,
                Color = color ?? CrescendoUtils.DeityPurple,
                Scale = scale,
                Rotation = 0f,
                RotationSpeed = 0f,
                TimeLeft = lifetime,
                MaxTime = lifetime,
                Type = CrescendoParticleType.OrbGlow,
                Active = true,
                Opacity = 1f
            };

        public static CrescendoParticle DivineSpark(Vector2 pos, Vector2 vel,
            Color? color = null, float scale = 0.12f, int lifetime = 14)
            => new CrescendoParticle
            {
                Position = pos,
                Velocity = vel,
                Color = color ?? CrescendoUtils.DivineCrimson,
                Scale = scale,
                Rotation = vel != Vector2.Zero ? vel.ToRotation() : 0f,
                RotationSpeed = 0f,
                TimeLeft = lifetime,
                MaxTime = lifetime,
                Type = CrescendoParticleType.DivineSpark,
                Active = true,
                Opacity = 1f
            };

        public static CrescendoParticle CosmicNote(Vector2 pos, Vector2 vel,
            Color? color = null, float scale = 0.18f, int lifetime = 30)
            => new CrescendoParticle
            {
                Position = pos,
                Velocity = vel,
                Color = color ?? CrescendoUtils.CrescendoPink,
                Scale = scale,
                Rotation = Main.rand.NextFloat(MathHelper.TwoPi),
                RotationSpeed = Main.rand.NextFloat(-0.04f, 0.04f),
                TimeLeft = lifetime,
                MaxTime = lifetime,
                Type = CrescendoParticleType.CosmicNote,
                Active = true,
                Opacity = 1f
            };

        public static CrescendoParticle GlyphCircle(Vector2 pos,
            Color? color = null, float scale = 0.3f, int lifetime = 25)
            => new CrescendoParticle
            {
                Position = pos,
                Velocity = Vector2.Zero,
                Color = color ?? CrescendoUtils.StarGold,
                Scale = scale,
                Rotation = Main.rand.NextFloat(MathHelper.TwoPi),
                RotationSpeed = 0.05f,
                TimeLeft = lifetime,
                MaxTime = lifetime,
                Type = CrescendoParticleType.GlyphCircle,
                Active = true,
                Opacity = 1f
            };

        public static CrescendoParticle BeamFlare(Vector2 pos, Vector2 vel,
            Color? color = null, float scale = 0.25f, int lifetime = 12)
            => new CrescendoParticle
            {
                Position = pos,
                Velocity = vel,
                Color = color ?? CrescendoUtils.CelestialWhite,
                Scale = scale,
                Rotation = 0f,
                RotationSpeed = 0f,
                TimeLeft = lifetime,
                MaxTime = lifetime,
                Type = CrescendoParticleType.BeamFlare,
                Active = true,
                Opacity = 1f
            };

        public static CrescendoParticle AuraWisp(Vector2 pos, Vector2 vel,
            Color? color = null, float scale = 0.15f, int lifetime = 22)
            => new CrescendoParticle
            {
                Position = pos,
                Velocity = vel,
                Color = color ?? CrescendoUtils.DeityPurple,
                Scale = scale,
                Rotation = Main.rand.NextFloat(MathHelper.TwoPi),
                RotationSpeed = Main.rand.NextFloat(-0.03f, 0.03f),
                TimeLeft = lifetime,
                MaxTime = lifetime,
                Type = CrescendoParticleType.AuraWisp,
                Active = true,
                Opacity = 1f
            };

        // ─── Composite Spawn Helpers ──────────────────────────────

        /// <summary>Spawn orbiting glyph particles around a center position.</summary>
        public static void SpawnOrbitingGlyphs(Vector2 center, int count, float radius, float baseAngle, float scale)
        {
            for (int i = 0; i < count; i++)
            {
                float angle = baseAngle + MathHelper.TwoPi * i / count;
                Vector2 pos = center + CrescendoUtils.HelixOffset(angle, radius);
                Color col = CrescendoUtils.PaletteLerp(0.2f + (float)i / count * 0.6f);
                CrescendoParticleHandler.Spawn(OrbGlow(pos, Main.rand.NextVector2Circular(0.3f, 0.3f), col * 0.6f, scale, 16));
            }
        }

        /// <summary>Spawn a burst of divine sparks in a direction.</summary>
        public static void SpawnSlashSparks(Vector2 pos, Vector2 direction, int count)
        {
            for (int i = 0; i < count; i++)
            {
                float angle = direction.ToRotation() + MathHelper.Lerp(-0.8f, 0.8f, (float)i / (count - 1));
                Vector2 sparkVel = angle.ToRotationVector2() * Main.rand.NextFloat(5f, 12f);
                Color sparkCol = CrescendoUtils.GetCrescendoGradient((float)i / (count - 1));
                CrescendoParticleHandler.Spawn(DivineSpark(pos, sparkVel, sparkCol, 0.2f, 16));
            }
        }

        /// <summary>Spawn cosmic music notes drifting upward.</summary>
        public static void SpawnCosmicNotes(Vector2 center, int count, float spread)
        {
            for (int i = 0; i < count; i++)
            {
                Vector2 pos = center + Main.rand.NextVector2Circular(spread, spread);
                Vector2 vel = new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), Main.rand.NextFloat(-1.2f, -0.4f));
                Color col = Color.Lerp(CrescendoUtils.CrescendoPink, CrescendoUtils.StarGold, Main.rand.NextFloat());
                CrescendoParticleHandler.Spawn(CosmicNote(pos, vel, col, 0.22f, 35));
            }
        }

        /// <summary>Spawn aura wisps around a position for cosmic ambiance.</summary>
        public static void SpawnAuraWisps(Vector2 center, int count, float spread)
        {
            for (int i = 0; i < count; i++)
            {
                Vector2 pos = center + Main.rand.NextVector2Circular(spread, spread);
                Vector2 vel = Main.rand.NextVector2Circular(0.6f, 0.6f);
                Color col = CrescendoUtils.PaletteLerp(Main.rand.NextFloat(0.1f, 0.5f));
                CrescendoParticleHandler.Spawn(AuraWisp(pos, vel, col * 0.5f, 0.18f, 24));
            }
        }

        /// <summary>Spawn a summoning explosion burst of all particle types.</summary>
        public static void SpawnSummonExplosion(Vector2 center)
        {
            // Ring of glyph circles
            CrescendoParticleHandler.SpawnBurst(center, 8, 3f, 0.35f, CrescendoUtils.StarGold, CrescendoParticleType.GlyphCircle, 30);

            // Divine sparks radiating outward
            CrescendoParticleHandler.SpawnBurst(center, 16, 8f, 0.18f, CrescendoUtils.DivineCrimson, CrescendoParticleType.DivineSpark, 18);

            // Orb glow core
            CrescendoParticleHandler.SpawnBurst(center, 6, 2f, 0.4f, CrescendoUtils.CelestialWhite, CrescendoParticleType.OrbGlow, 22);

            // Cosmic notes cascade
            SpawnCosmicNotes(center, 6, 30f);

            // Beam flare at center
            CrescendoParticleHandler.Spawn(BeamFlare(center, Vector2.Zero, CrescendoUtils.CelestialWhite, 0.6f, 15));
        }
    }
}
