using Microsoft.Xna.Framework;
using System;
using Terraria;
using MagnumOpus.Content.Fate.ResonantWeapons.TheFinalFermata.Utilities;

namespace MagnumOpus.Content.Fate.ResonantWeapons.TheFinalFermata.Particles
{
    /// <summary>
    /// Factory methods for the 6 Fermata particle types.
    /// Each method spawns a pre-configured particle with appropriate defaults.
    /// </summary>
    public static class FermataParticleTypes
    {
        // === FermataMote: soft drifting temporal mote ===

        /// <summary>
        /// Soft drifting mote with gentle movement. Used for ambient orbit VFX.
        /// </summary>
        public static void SpawnMote(Vector2 pos, Color? color = null, float scale = 0.2f, int lifetime = 30)
        {
            Color col = color ?? FermataUtils.PaletteLerp(Main.rand.NextFloat(0.1f, 0.5f));
            Vector2 vel = new Vector2(
                FermataUtils.RandSpread(0.4f),
                FermataUtils.RandSpread(0.4f));
            FermataParticleHandler.Spawn(pos, vel, col * 0.6f, scale, lifetime,
                FermataParticleType.FermataMote, true, 0.01f);
        }

        /// <summary>
        /// Burst of N motes radiating from a point.
        /// </summary>
        public static void SpawnMoteBurst(Vector2 pos, int count, float radius, Color? color = null)
        {
            for (int i = 0; i < count; i++)
            {
                float angle = MathHelper.TwoPi * i / count + Main.rand.NextFloat(-0.2f, 0.2f);
                Vector2 vel = FermataUtils.AngleToVector(angle) * Main.rand.NextFloat(1f, 3f);
                Color col = color ?? FermataUtils.PaletteLerp(Main.rand.NextFloat());
                FermataParticleHandler.Spawn(
                    pos + Main.rand.NextVector2Circular(radius * 0.3f, radius * 0.3f),
                    vel, col * 0.5f, Main.rand.NextFloat(0.15f, 0.3f), 25,
                    FermataParticleType.FermataMote, true, 0.02f);
            }
        }

        // === FermataSpark: sharp directional spark ===

        /// <summary>
        /// Sharp directional spark. Used for slash impacts and sync attacks.
        /// </summary>
        public static void SpawnSpark(Vector2 pos, Vector2 vel, Color? color = null, float scale = 0.18f, int lifetime = 14)
        {
            Color col = color ?? FermataUtils.FermataCrimson;
            float rot = vel.ToRotation();
            var p = FermataParticleHandler.Spawn(pos, vel, col, scale, lifetime,
                FermataParticleType.FermataSpark, true, 0f);
            if (p != null) p.Rotation = rot;
        }

        /// <summary>
        /// Radiating spark burst from a point.
        /// </summary>
        public static void SpawnSparkBurst(Vector2 pos, int count, float speed, Color? color = null)
        {
            for (int i = 0; i < count; i++)
            {
                float angle = MathHelper.TwoPi * i / count;
                Vector2 vel = FermataUtils.AngleToVector(angle) * Main.rand.NextFloat(speed * 0.6f, speed);
                SpawnSpark(pos, vel, color ?? FermataUtils.PaletteLerp(Main.rand.NextFloat(0.3f, 0.8f)));
            }
        }

        // === FermataTimeShard: frozen crystal-like shard ===

        /// <summary>
        /// Frozen crystal shard that tumbles slowly. Represents frozen time.
        /// </summary>
        public static void SpawnTimeShard(Vector2 pos, Vector2? vel = null, Color? color = null, float scale = 0.22f, int lifetime = 35)
        {
            Color col = color ?? Color.Lerp(FermataUtils.GhostSilver, FermataUtils.TimeGold, Main.rand.NextFloat());
            Vector2 v = vel ?? new Vector2(FermataUtils.RandSpread(0.8f), FermataUtils.RandSpread(0.8f));
            FermataParticleHandler.Spawn(pos, v, col, scale, lifetime,
                FermataParticleType.FermataTimeShard, true, Main.rand.NextFloat(0.01f, 0.04f));
        }

        /// <summary>
        /// Burst of time shards radiating outward — frozen time shattering.
        /// </summary>
        public static void SpawnTimeShardBurst(Vector2 pos, int count, float speed)
        {
            for (int i = 0; i < count; i++)
            {
                float angle = MathHelper.TwoPi * i / count + Main.rand.NextFloat(-0.3f, 0.3f);
                Vector2 vel = FermataUtils.AngleToVector(angle) * Main.rand.NextFloat(speed * 0.5f, speed);
                Color col = Color.Lerp(FermataUtils.GhostSilver, FermataUtils.TimeGold, Main.rand.NextFloat());
                FermataParticleHandler.Spawn(pos, vel, col, Main.rand.NextFloat(0.18f, 0.3f), 40,
                    FermataParticleType.FermataTimeShard, true, Main.rand.NextFloat(0.02f, 0.05f));
            }
        }

        // === FermataGlyph: temporal glyph cross symbol ===

        /// <summary>
        /// Temporal glyph that slowly spins and fades. Marks temporal distortion.
        /// </summary>
        public static void SpawnGlyph(Vector2 pos, Color? color = null, float scale = 0.25f, int lifetime = 40)
        {
            Color col = color ?? FermataUtils.FermataPurple;
            Vector2 vel = new Vector2(FermataUtils.RandSpread(0.2f), FermataUtils.RandSpread(0.2f));
            FermataParticleHandler.Spawn(pos, vel, col * 0.7f, scale, lifetime,
                FermataParticleType.FermataGlyph, true, 0.03f);
        }

        /// <summary>
        /// Ring of glyphs around a point.
        /// </summary>
        public static void SpawnGlyphRing(Vector2 pos, int count, float radius)
        {
            for (int i = 0; i < count; i++)
            {
                float angle = MathHelper.TwoPi * i / count;
                Vector2 glyphPos = pos + FermataUtils.AngleToVector(angle) * radius;
                SpawnGlyph(glyphPos, FermataUtils.PaletteLerp((float)i / count), 0.2f, 35);
            }
        }

        // === FermataBloomFlare: bright bloom flash ===

        /// <summary>
        /// Bright bloom flash — used for impacts, summons, and sync slashes.
        /// </summary>
        public static void SpawnBloomFlare(Vector2 pos, Color? color = null, float scale = 0.4f, int lifetime = 16)
        {
            Color col = color ?? FermataUtils.FlashWhite;
            FermataParticleHandler.Spawn(pos, Vector2.Zero, col, scale, lifetime,
                FermataParticleType.FermataBloomFlare, true, 0f);
        }

        /// <summary>
        /// Dual-color bloom flare cascade (bright core + colored outer).
        /// </summary>
        public static void SpawnFlareImpact(Vector2 pos, Color outerColor)
        {
            SpawnBloomFlare(pos, FermataUtils.FlashWhite, 0.45f, 14);
            SpawnBloomFlare(pos, outerColor, 0.35f, 18);
            SpawnBloomFlare(pos, FermataUtils.FermataPurple * 0.6f, 0.55f, 20);
        }

        // === FermataNebulaWisp: wispy nebula trail ===

        /// <summary>
        /// Wispy nebula particle. Drifts slowly, used for ambient cosmic atmosphere.
        /// </summary>
        public static void SpawnNebulaWisp(Vector2 pos, Vector2? vel = null, Color? color = null, float scale = 0.25f, int lifetime = 40)
        {
            Color col = color ?? Color.Lerp(FermataUtils.FermataPurple, FermataUtils.TemporalVoid, Main.rand.NextFloat());
            Vector2 v = vel ?? new Vector2(FermataUtils.RandSpread(0.3f), FermataUtils.RandSpread(0.3f));
            FermataParticleHandler.Spawn(pos, v, col * 0.5f, scale, lifetime,
                FermataParticleType.FermataNebulaWisp, true, 0.015f);
        }

        /// <summary>
        /// Trail of nebula wisps behind a moving object.
        /// </summary>
        public static void SpawnNebulaTrail(Vector2 pos, Vector2 direction, int count = 3)
        {
            Vector2 backDir = -direction;
            if (backDir != Vector2.Zero)
                backDir.Normalize();

            for (int i = 0; i < count; i++)
            {
                Vector2 offset = Main.rand.NextVector2Circular(6f, 6f);
                Vector2 vel = backDir * Main.rand.NextFloat(0.5f, 1.5f) + Main.rand.NextVector2Circular(0.3f, 0.3f);
                Color col = FermataUtils.PaletteLerp(Main.rand.NextFloat(0.0f, 0.4f));
                FermataParticleHandler.Spawn(pos + offset, vel, col * 0.4f,
                    Main.rand.NextFloat(0.15f, 0.25f), 30,
                    FermataParticleType.FermataNebulaWisp, true, 0.01f);
            }
        }

        // === COMPOSITE VFX HELPERS ===

        /// <summary>
        /// Full temporal distortion burst: time shards + glyphs + bloom flare.
        /// Used when casting the weapon.
        /// </summary>
        public static void TemporalDistortionBurst(Vector2 pos, float intensity = 1f)
        {
            int shardCount = (int)(8 * intensity);
            SpawnTimeShardBurst(pos, shardCount, 4f * intensity);
            SpawnGlyphRing(pos, 6, 30f * intensity);
            SpawnFlareImpact(pos, FermataUtils.FermataPurple);
            SpawnMoteBurst(pos, (int)(10 * intensity), 20f);
        }

        /// <summary>
        /// Sync slash burst VFX: sparks + flare + shards at impact.
        /// </summary>
        public static void SyncSlashImpact(Vector2 pos)
        {
            SpawnSparkBurst(pos, 10, 5f, FermataUtils.FermataCrimson);
            SpawnFlareImpact(pos, FermataUtils.FermataCrimson);
            SpawnTimeShardBurst(pos, 4, 3f);
            SpawnMoteBurst(pos, 6, 15f, FermataUtils.TimeGold);
        }
    }
}
