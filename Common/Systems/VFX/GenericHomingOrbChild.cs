using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.ModLoader;
namespace MagnumOpus.Common.Systems.VFX
{
    /// <summary>
    /// Generic reusable child orb projectile for split/spawn mechanics across all themes.
    /// Behavior is configured entirely via ai[] and localAI[] fields:
    ///
    /// ai[0] = Homing strength (0 = straight, 0.04-0.14 = gentle to aggressive)
    /// ai[1] = Behavior flags (bitfield):
    ///         Bit 0 (1):   Accelerate (1.02x per frame, cap at ai[0]*100 or 24f)
    ///         Bit 1 (2):   Decelerate (0.96x per frame, min 1f)
    ///         Bit 2 (4):   Bounce off tiles (max 3)
    ///         Bit 3 (8):   Pierce (penetrate = 3)
    ///         Bit 4 (16):  Gravity arc (0.08f per frame)
    ///         Bit 5 (32):  Sine-wave wobble
    /// localAI[0] = Theme index (maps to IncisorOrbRenderer.ThemeConfig)
    /// localAI[1] = Scale multiplier (0 = default 1.0)
    /// </summary>
    public class GenericHomingOrbChild : ModProjectile
    {
        // Behavior flag constants
        public const int FLAG_ACCELERATE = 1;
        public const int FLAG_DECELERATE = 2;
        public const int FLAG_BOUNCE = 4;
        public const int FLAG_PIERCE = 8;
        public const int FLAG_GRAVITY = 16;
        public const int FLAG_SINEWAVE = 32;
        public const int FLAG_ZONE_ON_KILL = 64;

        private VertexStrip _strip;
        private bool _initialized;
        private int _bounceCount;
        private float _sineTimer;

        public override string Texture => "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/PointBloom";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 12;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Generic;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 90;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.extraUpdates = 1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 15;
        }

        public override void AI()
        {
            int flags = (int)Projectile.ai[1];

            if (!_initialized)
            {
                _initialized = true;
                Projectile.rotation = Projectile.velocity.ToRotation();

                // Apply flag-based defaults
                if ((flags & FLAG_PIERCE) != 0)
                    Projectile.penetrate = 3;

                if ((flags & FLAG_BOUNCE) != 0)
                    Projectile.tileCollide = true;

                // Apply scale
                float scaleMult = Projectile.localAI[1];
                if (scaleMult > 0.01f)
                    Projectile.scale = scaleMult;
            }

            float homingStrength = Projectile.ai[0];

            // Homing
            if (homingStrength > 0.001f)
            {
                NPC target = FindClosestNPC(600f);
                if (target != null)
                {
                    Vector2 idealDir = (target.Center - Projectile.Center).SafeNormalize(Projectile.velocity.SafeNormalize(Vector2.UnitX));
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, idealDir * Projectile.velocity.Length(), homingStrength);
                }
            }

            // Acceleration
            if ((flags & FLAG_ACCELERATE) != 0)
            {
                float maxSpeed = MathHelper.Clamp(homingStrength * 100f, 18f, 30f);
                if (Projectile.velocity.Length() < maxSpeed)
                    Projectile.velocity *= 1.02f;
            }

            // Deceleration
            if ((flags & FLAG_DECELERATE) != 0)
            {
                if (Projectile.velocity.Length() > 1f)
                    Projectile.velocity *= 0.96f;
            }

            // Gravity
            if ((flags & FLAG_GRAVITY) != 0)
                Projectile.velocity.Y += 0.08f;

            // Sine-wave wobble
            if ((flags & FLAG_SINEWAVE) != 0)
            {
                _sineTimer += 0.15f;
                Vector2 perpendicular = new Vector2(-Projectile.velocity.Y, Projectile.velocity.X).SafeNormalize(Vector2.Zero);
                Projectile.Center += perpendicular * MathF.Sin(_sineTimer) * 1.5f;
            }

            Projectile.rotation = Projectile.velocity.ToRotation();

            // Dust trail
            if (Main.rand.NextBool(4))
            {
                Color dustColor = GetThemeColor();
                Dust d = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(4f, 4f),
                    DustID.RainbowMk2, -Projectile.velocity * 0.1f, 0, dustColor, 0.5f);
                d.noGravity = true;
                d.fadeIn = 0.4f;
            }

            // Lighting
            Color lightColor = GetThemeColor();
            Lighting.AddLight(Projectile.Center, lightColor.ToVector3() * 0.3f);
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            int flags = (int)Projectile.ai[1];
            if ((flags & FLAG_BOUNCE) != 0 && _bounceCount < 3)
            {
                _bounceCount++;
                if (Math.Abs(Projectile.velocity.X - oldVelocity.X) > float.Epsilon)
                    Projectile.velocity.X = -oldVelocity.X;
                if (Math.Abs(Projectile.velocity.Y - oldVelocity.Y) > float.Epsilon)
                    Projectile.velocity.Y = -oldVelocity.Y;
                return false;
            }
            return true;
        }

        private NPC FindClosestNPC(float maxDist)
        {
            NPC closest = null;
            float closestDist = maxDist;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.CanBeChasedBy()) continue;
                float dist = Vector2.Distance(Projectile.Center, npc.Center);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closest = npc;
                }
            }
            return closest;
        }

        private IncisorOrbRenderer.ThemeConfig GetThemeConfig()
        {
            int themeIndex = (int)Projectile.localAI[0];
            return themeIndex switch
            {
                0 => IncisorOrbRenderer.Eroica,
                1 => IncisorOrbRenderer.LaCampanella,
                2 => IncisorOrbRenderer.Enigma,
                3 => IncisorOrbRenderer.SwanLake,
                4 => IncisorOrbRenderer.Fate,
                5 => IncisorOrbRenderer.Nachtmusik,
                6 => IncisorOrbRenderer.DiesIrae,
                7 => IncisorOrbRenderer.OdeToJoy,
                8 => IncisorOrbRenderer.ClairDeLune,
                9 => IncisorOrbRenderer.Spring,
                10 => IncisorOrbRenderer.Summer,
                11 => IncisorOrbRenderer.Autumn,
                12 => IncisorOrbRenderer.Winter,
                _ => IncisorOrbRenderer.DiesIrae,
            };
        }

        private Color GetThemeColor()
        {
            var config = GetThemeConfig();
            Vector3 v = config.LightColor;
            return new Color(v.X, v.Y, v.Z);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            try
            {
                var config = GetThemeConfig();
                IncisorOrbRenderer.DrawOrbVisuals(sb, Projectile, config, ref _strip);
            }
            catch { }
            finally
            {
                try { sb.End(); } catch { }
                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                    DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            }
            return false;
        }

        public override void OnKill(int timeLeft)
        {
            // Spawn damage zone on kill if flagged (must run on server too)
            int flags = (int)Projectile.ai[1];
            if ((flags & FLAG_ZONE_ON_KILL) != 0)
            {
                int themeIndex = (int)Projectile.localAI[0];
                GenericDamageZone.SpawnZone(
                    Projectile.GetSource_FromThis(),
                    Projectile.Center, Projectile.damage, Projectile.knockBack, Projectile.owner,
                    GenericDamageZone.FLAG_SLOW, 100f, themeIndex, durationFrames: 90);
            }

            if (Main.dedServ) return;
            Color col = GetThemeColor();
            for (int i = 0; i < 3; i++)
            {
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.RainbowMk2,
                    Main.rand.NextVector2Circular(3f, 3f), 0, col, 0.4f);
                d.noGravity = true;
            }
        }

        // =====================================================================
        // STATIC HELPERS for spawning themed child orbs
        // =====================================================================

        /// <summary>Theme index constants for localAI[0].</summary>
        public const int THEME_EROICA = 0;
        public const int THEME_LACAMPANELLA = 1;
        public const int THEME_ENIGMA = 2;
        public const int THEME_SWANLAKE = 3;
        public const int THEME_FATE = 4;
        public const int THEME_NACHTMUSIK = 5;
        public const int THEME_DIESIRAE = 6;
        public const int THEME_ODETOJOY = 7;
        public const int THEME_CLAIRDELUNE = 8;
        public const int THEME_SPRING = 9;
        public const int THEME_SUMMER = 10;
        public const int THEME_AUTUMN = 11;
        public const int THEME_WINTER = 12;

        /// <summary>
        /// Spawns a themed homing child orb.
        /// </summary>
        public static int SpawnChild(
            Terraria.DataStructures.IEntitySource source,
            Vector2 position, Vector2 velocity,
            int damage, float knockback, int owner,
            float homingStrength, int behaviorFlags, int themeIndex,
            float scaleMult = 1f, int timeLeft = 90)
        {
            int projType = ModContent.ProjectileType<GenericHomingOrbChild>();
            int idx = Terraria.Projectile.NewProjectile(source, position, velocity, projType,
                damage, knockback, owner, ai0: homingStrength, ai1: behaviorFlags);
            if (idx >= 0 && idx < Main.maxProjectiles)
            {
                Main.projectile[idx].localAI[0] = themeIndex;
                Main.projectile[idx].localAI[1] = scaleMult;
                if (timeLeft != 90)
                    Main.projectile[idx].timeLeft = timeLeft;
            }
            return idx;
        }
    }
}
