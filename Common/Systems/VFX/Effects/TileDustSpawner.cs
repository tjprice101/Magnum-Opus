using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Common.Systems.VFX.Effects
{
    /// <summary>
    /// Tile/dust interaction system for material-based particle spawning.
    /// 
    /// Interaction Types:
    /// - Collision Dust: Particle burst on tile contact
    /// - Trailing Dust: Continuous spawning along path
    /// - Destruction Debris: Tile fragments when destroyed
    /// - Material-Specific: Different effects per tile type
    /// </summary>
    public static class TileDustSpawner
    {
        #region Material Types

        /// <summary>
        /// Tile material categories.
        /// </summary>
        public enum MaterialType
        {
            Stone,
            Dirt,
            Wood,
            Metal,
            Ice,
            Flesh,
            Crystal,
            Sand,
            Glass,
            Corruption,
            Crimson,
            Hallow,
            Jungle,
            Lava
        }

        /// <summary>
        /// Dust spawn profile per material.
        /// </summary>
        public struct DustProfile
        {
            public int DustType;
            public Color Color;
            public int ParticleCount;
            public float Speed;
            public float Gravity;
            public float Scale;
            public bool NoGravity;
            public bool HasLight;
            public Vector3 LightColor;
        }

        private static readonly Dictionary<MaterialType, DustProfile> DustProfiles = new()
        {
            { MaterialType.Stone, new DustProfile
            {
                DustType = DustID.Stone,
                Color = Color.Gray,
                ParticleCount = 15,
                Speed = 3f,
                Gravity = 0.2f,
                Scale = 1f
            }},
            { MaterialType.Dirt, new DustProfile
            {
                DustType = DustID.Dirt,
                Color = new Color(150, 100, 50),
                ParticleCount = 20,
                Speed = 2f,
                Gravity = 0.25f,
                Scale = 0.8f
            }},
            { MaterialType.Wood, new DustProfile
            {
                DustType = DustID.WoodFurniture,
                Color = new Color(139, 90, 43),
                ParticleCount = 12,
                Speed = 2.5f,
                Gravity = 0.15f,
                Scale = 1.2f
            }},
            { MaterialType.Metal, new DustProfile
            {
                DustType = DustID.Iron,
                Color = Color.LightGray,
                ParticleCount = 10,
                Speed = 4f,
                Gravity = 0.1f,
                Scale = 0.8f,
                HasLight = true,
                LightColor = new Vector3(0.4f, 0.4f, 0.5f)
            }},
            { MaterialType.Ice, new DustProfile
            {
                DustType = DustID.Ice,
                Color = new Color(150, 220, 255),
                ParticleCount = 18,
                Speed = 2f,
                Gravity = 0.15f,
                Scale = 1f,
                NoGravity = true
            }},
            { MaterialType.Crystal, new DustProfile
            {
                DustType = DustID.CrystalSerpent,
                Color = new Color(200, 150, 255),
                ParticleCount = 15,
                Speed = 3f,
                Gravity = 0.1f,
                Scale = 1.2f,
                NoGravity = true,
                HasLight = true,
                LightColor = new Vector3(0.5f, 0.3f, 0.6f)
            }},
            { MaterialType.Sand, new DustProfile
            {
                DustType = DustID.Sand,
                Color = new Color(220, 200, 140),
                ParticleCount = 25,
                Speed = 1.5f,
                Gravity = 0.3f,
                Scale = 0.7f
            }},
            { MaterialType.Corruption, new DustProfile
            {
                DustType = DustID.Corruption,
                Color = new Color(100, 50, 150),
                ParticleCount = 15,
                Speed = 2.5f,
                Gravity = 0.1f,
                Scale = 1f,
                HasLight = true,
                LightColor = new Vector3(0.3f, 0.1f, 0.4f)
            }},
            { MaterialType.Crimson, new DustProfile
            {
                DustType = DustID.Crimson,
                Color = new Color(200, 50, 50),
                ParticleCount = 15,
                Speed = 2.5f,
                Gravity = 0.15f,
                Scale = 1f,
                HasLight = true,
                LightColor = new Vector3(0.5f, 0.1f, 0.1f)
            }},
            { MaterialType.Hallow, new DustProfile
            {
                DustType = DustID.HallowedPlants,
                Color = new Color(255, 200, 255),
                ParticleCount = 15,
                Speed = 2f,
                Gravity = 0.05f,
                Scale = 1.1f,
                NoGravity = true,
                HasLight = true,
                LightColor = new Vector3(0.5f, 0.4f, 0.6f)
            }},
            { MaterialType.Lava, new DustProfile
            {
                DustType = DustID.Lava,
                Color = new Color(255, 150, 50),
                ParticleCount = 12,
                Speed = 3f,
                Gravity = -0.1f,
                Scale = 1.3f,
                NoGravity = true,
                HasLight = true,
                LightColor = new Vector3(0.8f, 0.4f, 0.1f)
            }}
        };

        #endregion

        #region Material Detection

        /// <summary>
        /// Get material type from tile.
        /// </summary>
        public static MaterialType GetMaterialType(Tile tile)
        {
            if (tile == null || !tile.HasTile)
                return MaterialType.Stone;

            int type = tile.TileType;

            // Stone types
            if (type == TileID.Stone || type == TileID.GrayBrick || type == TileID.Granite ||
                type == TileID.Marble || type == TileID.Sandstone)
                return MaterialType.Stone;

            // Dirt types
            if (type == TileID.Dirt || type == TileID.Grass || type == TileID.ClayBlock ||
                type == TileID.Mud)
                return MaterialType.Dirt;

            // Wood types
            if (type >= TileID.WoodBlock && type <= TileID.DynastyWood ||
                type == TileID.LivingWood || type == TileID.LeafBlock)
                return MaterialType.Wood;

            // Metal types
            if (type == TileID.Iron || type == TileID.Lead || type == TileID.Copper ||
                type == TileID.Tin || type == TileID.Silver || type == TileID.Tungsten ||
                type == TileID.Gold || type == TileID.Platinum || type == TileID.Titanium ||
                type == TileID.Adamantite)
                return MaterialType.Metal;

            // Ice types
            if (type == TileID.IceBlock || type == TileID.SnowBlock || type == TileID.BreakableIce)
                return MaterialType.Ice;

            // Crystal types
            if (type == TileID.Crystals || type == TileID.CrystalBlock ||
                type == TileID.Amethyst || type == TileID.Topaz || type == TileID.Sapphire ||
                type == TileID.Emerald || type == TileID.Ruby || type == TileID.Diamond)
                return MaterialType.Crystal;

            // Sand types
            if (type == TileID.Sand || type == TileID.Sandstone || type == TileID.HardenedSand)
                return MaterialType.Sand;

            // Biome-specific
            if (type == TileID.CorruptGrass || type == TileID.Ebonstone || type == TileID.CorruptSandstone)
                return MaterialType.Corruption;

            if (type == TileID.CrimsonGrass || type == TileID.Crimstone || type == TileID.CrimsonSandstone)
                return MaterialType.Crimson;

            if (type == TileID.HallowedGrass || type == TileID.Pearlstone || type == TileID.HallowSandstone)
                return MaterialType.Hallow;

            return MaterialType.Stone;
        }

        /// <summary>
        /// Get material type at world position.
        /// </summary>
        public static MaterialType GetMaterialTypeAt(Vector2 worldPosition)
        {
            Point tileCoords = worldPosition.ToTileCoordinates();

            if (!WorldGen.InWorld(tileCoords.X, tileCoords.Y))
                return MaterialType.Stone;

            return GetMaterialType(Main.tile[tileCoords.X, tileCoords.Y]);
        }

        #endregion

        #region Dust Spawning

        /// <summary>
        /// Spawn impact dust when hitting a tile.
        /// </summary>
        /// <param name="position">Impact position</param>
        /// <param name="normal">Surface normal (direction particles bounce)</param>
        /// <param name="tile">Tile that was hit</param>
        /// <param name="intensity">Effect intensity multiplier</param>
        public static void SpawnImpactDust(Vector2 position, Vector2 normal, Tile tile, float intensity = 1f)
        {
            MaterialType material = GetMaterialType(tile);
            DustProfile profile = GetProfile(material);

            int count = (int)(profile.ParticleCount * intensity);

            for (int i = 0; i < count; i++)
            {
                float angle = normal.ToRotation() +
                    MathHelper.ToRadians(Main.rand.NextFloat(-60f, 60f));
                float speed = Main.rand.NextFloat(profile.Speed * 0.5f, profile.Speed * 1.5f) * intensity;

                Vector2 velocity = angle.ToRotationVector2() * speed;

                int dustIndex = Dust.NewDust(
                    position - new Vector2(4, 4),
                    8, 8,
                    profile.DustType,
                    velocity.X,
                    velocity.Y,
                    0,
                    profile.Color,
                    profile.Scale * Main.rand.NextFloat(0.8f, 1.2f)
                );

                if (dustIndex >= 0 && dustIndex < Main.dust.Length)
                {
                    Dust dust = Main.dust[dustIndex];
                    dust.noGravity = profile.NoGravity;
                    dust.velocity = velocity;

                    if (!profile.NoGravity)
                    {
                        dust.velocity.Y += profile.Gravity;
                    }
                }
            }

            // Special effects per material
            SpawnMaterialSpecificEffects(position, normal, material, intensity);

            // Add light if applicable
            if (profile.HasLight)
            {
                Lighting.AddLight(position, profile.LightColor * intensity);
            }
        }

        /// <summary>
        /// Spawn trailing dust along a beam or projectile path.
        /// </summary>
        public static void SpawnTrailDust(Vector2 position, Tile tile, float intensity = 1f)
        {
            MaterialType material = GetMaterialType(tile);
            DustProfile profile = GetProfile(material);

            if (Main.rand.NextFloat() < 0.3f * intensity)
            {
                Vector2 velocity = Main.rand.NextVector2Circular(1f, 1f);

                int dustIndex = Dust.NewDust(
                    position - new Vector2(2, 2),
                    4, 4,
                    profile.DustType,
                    velocity.X,
                    velocity.Y,
                    0,
                    profile.Color,
                    profile.Scale * 0.6f
                );

                if (dustIndex >= 0 && dustIndex < Main.dust.Length)
                {
                    Main.dust[dustIndex].noGravity = true;
                    Main.dust[dustIndex].fadeIn = 0.5f;
                }
            }
        }

        /// <summary>
        /// Spawn dust along a beam path with tile detection.
        /// </summary>
        public static void SpawnBeamTrailDust(Vector2 start, Vector2 end, float density = 0.1f)
        {
            Vector2 direction = end - start;
            float length = direction.Length();
            if (length < 1f) return;

            direction /= length;
            int steps = (int)(length / 16f);

            for (int i = 0; i < steps; i++)
            {
                if (Main.rand.NextFloat() > density) continue;

                float progress = i / (float)steps;
                Vector2 pos = Vector2.Lerp(start, end, progress);

                Point tileCoords = pos.ToTileCoordinates();
                if (!WorldGen.InWorld(tileCoords.X, tileCoords.Y)) continue;

                Tile tile = Main.tile[tileCoords.X, tileCoords.Y];
                if (tile != null && tile.HasTile)
                {
                    SpawnTrailDust(pos, tile, 0.5f);
                }
            }
        }

        /// <summary>
        /// Spawn debris when destroying a tile.
        /// </summary>
        public static void SpawnDestructionDebris(Vector2 position, Tile tile, int tileX, int tileY)
        {
            MaterialType material = GetMaterialType(tile);
            DustProfile profile = GetProfile(material);

            // Large dust cloud
            for (int i = 0; i < profile.ParticleCount * 2; i++)
            {
                Vector2 velocity = Main.rand.NextVector2CircularEdge(4f, 4f);

                Dust.NewDust(
                    new Vector2(tileX * 16, tileY * 16),
                    16, 16,
                    profile.DustType,
                    velocity.X,
                    velocity.Y,
                    0,
                    profile.Color,
                    Main.rand.NextFloat(0.8f, 1.5f)
                );
            }

            // Screen shake if nearby
            float distance = Vector2.Distance(position, Main.LocalPlayer.Center);
            if (distance < 200f && ScreenShakeManager.Instance != null)
            {
                float intensity = (200f - distance) / 200f * 0.3f;
                ScreenShakeManager.Instance.AddTrauma(intensity);
            }
        }

        #endregion

        #region Material-Specific Effects

        private static void SpawnMaterialSpecificEffects(Vector2 position, Vector2 normal,
            MaterialType material, float intensity)
        {
            switch (material)
            {
                case MaterialType.Metal:
                    SpawnMetalSparks(position, normal, intensity);
                    break;

                case MaterialType.Ice:
                    SpawnIceMist(position, intensity);
                    break;

                case MaterialType.Crystal:
                    SpawnCrystalShimmer(position, normal, intensity);
                    break;

                case MaterialType.Lava:
                    SpawnLavaEmbers(position, normal, intensity);
                    break;

                case MaterialType.Corruption:
                case MaterialType.Crimson:
                    SpawnEvilDust(position, normal, material, intensity);
                    break;

                case MaterialType.Hallow:
                    SpawnHallowSparkles(position, normal, intensity);
                    break;
            }
        }

        private static void SpawnMetalSparks(Vector2 position, Vector2 normal, float intensity)
        {
            int count = (int)(8 * intensity);
            for (int i = 0; i < count; i++)
            {
                float angle = normal.ToRotation() + Main.rand.NextFloat(-1f, 1f);
                Vector2 velocity = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 8f);

                int dust = Dust.NewDust(position, 4, 4, DustID.Electric, velocity.X, velocity.Y);
                if (dust >= 0 && dust < Main.dust.Length)
                {
                    Main.dust[dust].noGravity = true;
                    Main.dust[dust].scale = 1.2f;
                }
            }

            Lighting.AddLight(position, 0.5f, 0.5f, 0.6f);
        }

        private static void SpawnIceMist(Vector2 position, float intensity)
        {
            int count = (int)(5 * intensity);
            for (int i = 0; i < count; i++)
            {
                Vector2 velocity = new Vector2(
                    Main.rand.NextFloat(-1f, 1f),
                    Main.rand.NextFloat(-2f, 0f)
                );

                int dust = Dust.NewDust(position, 8, 8, DustID.IceTorch, velocity.X, velocity.Y);
                if (dust >= 0 && dust < Main.dust.Length)
                {
                    Main.dust[dust].noGravity = true;
                    Main.dust[dust].fadeIn = 1.5f;
                    Main.dust[dust].alpha = 100;
                }
            }

            Lighting.AddLight(position, 0.2f, 0.4f, 0.5f);
        }

        private static void SpawnCrystalShimmer(Vector2 position, Vector2 normal, float intensity)
        {
            int count = (int)(10 * intensity);
            for (int i = 0; i < count; i++)
            {
                float angle = normal.ToRotation() + Main.rand.NextFloat(-0.8f, 0.8f);
                Vector2 velocity = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 5f);

                int dust = Dust.NewDust(position, 4, 4, DustID.CrystalSerpent, velocity.X, velocity.Y);
                if (dust >= 0 && dust < Main.dust.Length)
                {
                    Main.dust[dust].noGravity = true;
                    Main.dust[dust].color = Color.Lerp(Color.Cyan, Color.Purple, Main.rand.NextFloat());
                }
            }

            Lighting.AddLight(position, 0.4f, 0.3f, 0.5f);
        }

        private static void SpawnLavaEmbers(Vector2 position, Vector2 normal, float intensity)
        {
            int count = (int)(6 * intensity);
            for (int i = 0; i < count; i++)
            {
                float angle = normal.ToRotation() + Main.rand.NextFloat(-0.5f, 0.5f);
                Vector2 velocity = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 7f);
                velocity.Y -= 2f; // Rise up

                int dust = Dust.NewDust(position, 4, 4, DustID.Lava, velocity.X, velocity.Y);
                if (dust >= 0 && dust < Main.dust.Length)
                {
                    Main.dust[dust].noGravity = true;
                    Main.dust[dust].scale = 1.5f;
                }
            }

            Lighting.AddLight(position, 0.8f, 0.4f, 0.1f);
        }

        private static void SpawnEvilDust(Vector2 position, Vector2 normal,
            MaterialType material, float intensity)
        {
            int dustType = material == MaterialType.Corruption ? DustID.Corruption : DustID.Crimson;
            Color color = material == MaterialType.Corruption ?
                new Color(100, 50, 150) : new Color(200, 50, 50);

            int count = (int)(8 * intensity);
            for (int i = 0; i < count; i++)
            {
                float angle = normal.ToRotation() + Main.rand.NextFloat(-1f, 1f);
                Vector2 velocity = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 5f);

                int dust = Dust.NewDust(position, 4, 4, dustType, velocity.X, velocity.Y);
                if (dust >= 0 && dust < Main.dust.Length)
                {
                    Main.dust[dust].noGravity = true;
                    Main.dust[dust].color = color;
                }
            }
        }

        private static void SpawnHallowSparkles(Vector2 position, Vector2 normal, float intensity)
        {
            int count = (int)(12 * intensity);
            for (int i = 0; i < count; i++)
            {
                float angle = normal.ToRotation() + Main.rand.NextFloat(-1.2f, 1.2f);
                Vector2 velocity = angle.ToRotationVector2() * Main.rand.NextFloat(1f, 4f);

                int dust = Dust.NewDust(position, 4, 4, DustID.HallowedPlants, velocity.X, velocity.Y);
                if (dust >= 0 && dust < Main.dust.Length)
                {
                    Main.dust[dust].noGravity = true;
                    Main.dust[dust].fadeIn = 1.2f;
                    Main.dust[dust].color = Main.hslToRgb(Main.rand.NextFloat(), 0.8f, 0.8f);
                }
            }

            Lighting.AddLight(position, 0.5f, 0.4f, 0.6f);
        }

        private static DustProfile GetProfile(MaterialType material)
        {
            if (DustProfiles.TryGetValue(material, out var profile))
                return profile;

            return DustProfiles[MaterialType.Stone];
        }

        #endregion

        #region Tile Collision Detection

        /// <summary>
        /// Get tile at world position.
        /// </summary>
        public static Tile GetTileAt(Vector2 position)
        {
            Point tileCoords = position.ToTileCoordinates();

            if (!WorldGen.InWorld(tileCoords.X, tileCoords.Y))
                return default;

            return Main.tile[tileCoords.X, tileCoords.Y];
        }

        /// <summary>
        /// Raycast for tile collision.
        /// </summary>
        /// <returns>True if collision found</returns>
        public static bool TileRaycast(Vector2 start, Vector2 end, out Vector2 hitPoint,
            out Vector2 hitNormal, out Tile hitTile)
        {
            hitPoint = end;
            hitNormal = Vector2.Zero;
            hitTile = default;

            Vector2 direction = end - start;
            float length = direction.Length();
            if (length < 1f) return false;

            direction /= length;

            float stepSize = 8f;
            for (float dist = 0; dist < length; dist += stepSize)
            {
                Vector2 checkPos = start + direction * dist;
                Point tileCoords = checkPos.ToTileCoordinates();

                if (!WorldGen.InWorld(tileCoords.X, tileCoords.Y)) continue;

                Tile tile = Main.tile[tileCoords.X, tileCoords.Y];

                if (tile != null && tile.HasTile && Main.tileSolid[tile.TileType])
                {
                    hitPoint = checkPos;
                    hitTile = tile;
                    hitNormal = CalculateTileNormal(tileCoords);
                    return true;
                }
            }

            return false;
        }

        private static Vector2 CalculateTileNormal(Point tileCoords)
        {
            bool left = WorldGen.SolidTile(tileCoords.X - 1, tileCoords.Y);
            bool right = WorldGen.SolidTile(tileCoords.X + 1, tileCoords.Y);
            bool up = WorldGen.SolidTile(tileCoords.X, tileCoords.Y - 1);
            bool down = WorldGen.SolidTile(tileCoords.X, tileCoords.Y + 1);

            Vector2 normal = Vector2.Zero;

            if (!left) normal.X -= 1;
            if (!right) normal.X += 1;
            if (!up) normal.Y -= 1;
            if (!down) normal.Y += 1;

            if (normal == Vector2.Zero)
                normal = Vector2.UnitY;

            return Vector2.Normalize(normal);
        }

        #endregion
    }
}
