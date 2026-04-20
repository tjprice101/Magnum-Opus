using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.VFX.Core;
using MagnumOpus.Content.Nachtmusik;
using MagnumOpus.Content.Nachtmusik.Weapons.SerenadeOfDistantStars.Utilities;

namespace MagnumOpus.Content.Nachtmusik.Weapons.SerenadeOfDistantStars.Projectiles
{
    /// <summary>
    /// Serenade star projectile — Rhythm Stacking behavior.
    /// Reads rhythm stack count from ai[0] (0-5) and scales behavior accordingly:
    ///   Stack 0: homing 0.04, speed 12
    ///   Stack 1: homing 0.05, speed 13
    ///   Stack 2: homing 0.06, speed 15
    ///   Stack 3: pierce 1, homing 0.07, speed 16
    ///   Stack 4: pierce 1, homing 0.08, speed 17, on-hit spawn 1 child (half damage)
    ///   Stack 5: aggressive homing 0.12, speed 18, 1.3x scale, on-hit spawn 1 child
    ///
    /// ai[0] = rhythm stack level (0-5)
    /// </summary>
    public class SerenadeStarProjectile : ModProjectile
    {
        // Stack-based homing and speed tables
        private static readonly float[] StackHoming = { 0.04f, 0.05f, 0.06f, 0.07f, 0.08f, 0.12f };
        private static readonly float[] StackSpeed = { 12f, 13f, 15f, 16f, 17f, 18f };

        private const float HomingRange = 350f;
        private Player Owner => Main.player[Projectile.owner];
        private bool _initialized;
        private VertexStrip _strip;

        private int StackLevel => Math.Clamp((int)Projectile.ai[0], 0, 5);

        public override string Texture => "MagnumOpus/Content/Nachtmusik/Weapons/SerenadeOfDistantStars/SerenadeOfDistantStars";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 16;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 120;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.extraUpdates = 1;
        }

        public override void AI()
        {
            int stack = StackLevel;

            if (!_initialized)
            {
                _initialized = true;
                Projectile.rotation = Projectile.velocity.ToRotation();

                // Stack 3+: grant 1 pierce
                if (stack >= 3)
                    Projectile.penetrate = 2; // 1 base + 1 pierce = hits 2 enemies

                // Stack 5: scale up
                if (stack >= 5)
                    Projectile.scale = 1.3f;
            }

            float homingStrength = StackHoming[stack];
            float maxSpeed = StackSpeed[stack];

            // Homing behavior
            NPC target = SerenadeOfDistantStarsUtils.ClosestNPCAt(Projectile.Center, HomingRange);
            if (target != null)
            {
                Vector2 desiredDir = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, desiredDir * Projectile.velocity.Length(), homingStrength);
            }

            // Accelerate toward max speed
            if (Projectile.velocity.Length() < maxSpeed)
                Projectile.velocity *= 1.02f;
            if (Projectile.velocity.Length() > maxSpeed)
                Projectile.velocity = Vector2.Normalize(Projectile.velocity) * maxSpeed;

            Projectile.rotation = Projectile.velocity.ToRotation();

            // Trail dust — more intense at higher stacks
            int dustChance = stack >= 3 ? 2 : 3;
            if (Main.rand.NextBool(dustChance))
            {
                int dustType = Main.rand.NextBool() ? DustID.WhiteTorch : DustID.BlueTorch;
                Color dustColor = Main.rand.NextBool() ? new Color(180, 200, 255) : new Color(60, 70, 150);
                float dustScale = 0.8f + stack * 0.06f;
                Dust d = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(6f, 6f),
                    dustType, -Projectile.velocity * 0.15f + Main.rand.NextVector2Circular(0.5f, 0.5f),
                    0, dustColor, dustScale);
                d.noGravity = true;
                d.fadeIn = 0.6f;
            }

            // Pulsing light — brighter at higher stacks
            float pulse = 1f + 0.15f * (float)Math.Sin(Projectile.timeLeft * 0.2f);
            float lightMult = 0.35f + stack * 0.04f;
            Lighting.AddLight(Projectile.Center, new Vector3(0.3f, 0.35f, 0.6f) * lightMult * pulse);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Vector2 hitPos = target.Center;
            int stack = StackLevel;

            // Standard impact VFX
            for (int i = 0; i < 6; i++)
            {
                Vector2 sparkVel = Main.rand.NextVector2CircularEdge(4f, 4f);
                Color col = i % 2 == 0 ? new Color(60, 70, 150) : new Color(180, 200, 255);
                Dust d = Dust.NewDustPerfect(hitPos, DustID.WhiteTorch, sparkVel, 0, col, 0.5f);
                d.noGravity = true;
            }
            for (int i = 0; i < 2; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(2f, 2f) + new Vector2(0, -1f);
                Dust d = Dust.NewDustPerfect(hitPos + Main.rand.NextVector2Circular(8f, 8f),
                    DustID.BlueTorch, vel, 0, new Color(60, 70, 150), 0.5f);
                d.noGravity = true;
            }
            try { NachtmusikVFXLibrary.SpawnMusicNotes(hitPos, 1, 12f, 0.4f, 0.7f, 20); } catch { }
            try { NachtmusikVFXLibrary.SpawnMixedSparkleImpact(hitPos, 0.6f, 4, 4); } catch { }

            // Stack 4+: spawn 1 child orb at half damage
            if (stack >= 4)
            {
                Vector2 childVel = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX).RotatedByRandom(MathHelper.ToRadians(30)) * 10f;
                GenericHomingOrbChild.SpawnChild(
                    Projectile.GetSource_FromThis(),
                    Projectile.Center, childVel,
                    Projectile.damage / 2, Projectile.knockBack * 0.5f, Projectile.owner,
                    0.08f, GenericHomingOrbChild.FLAG_ACCELERATE, GenericHomingOrbChild.THEME_NACHTMUSIK,
                    0.8f, 60);

                // Extra child spawn VFX
                for (int i = 0; i < 4; i++)
                {
                    Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.WhiteTorch,
                        Main.rand.NextVector2CircularEdge(2f, 2f), 0, new Color(200, 180, 100), 0.4f);
                    d.noGravity = true;
                }
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            try
            {
                IncisorOrbRenderer.DrawOrbVisuals(sb, Projectile, IncisorOrbRenderer.Nachtmusik, ref _strip);
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
            for (int i = 0; i < 4; i++)
            {
                Vector2 sparkVel = Main.rand.NextVector2CircularEdge(3f, 3f);
                Color col = Main.rand.NextBool() ? new Color(60, 70, 150) : new Color(180, 200, 255);
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.WhiteTorch, sparkVel, 0, col, 0.3f);
                d.noGravity = true;
            }
            try { NachtmusikVFXLibrary.SpawnMusicNotes(Projectile.Center, 1, 12f, 0.5f, 0.7f, 20); } catch { }
            try { NachtmusikVFXLibrary.SpawnMixedSparkleImpact(Projectile.Center, 0.5f, 4, 4); } catch { }
            try { NachtmusikVFXLibrary.SpawnCelestialSparkles(Projectile.Center, 3, 15f); } catch { }
        }
    }
}
