using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Content.Eroica;

namespace MagnumOpus.Content.Eroica.Weapons.SakurasBlossom.Projectiles
{
    /// <summary>
    /// Sakura petal projectile — a drifting, gently homing cherry blossom petal that
    /// floats toward enemies with grace. Drawn programmatically as a soft petal shape
    /// using the existing star texture with sakura pink tints.
    /// ai[0] = mode: 0 = normal petal (gentle homing), 1 = empowered petal (fast homing, larger)
    /// ai[1] = visual variant (0-3, controls color tint and starting rotation)
    /// </summary>
    public class SakuraPetalProj : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/PointBloom";

        private float spin;
        private float drift;
        private VertexStrip _strip;

        private static readonly Color[] PetalColors = new Color[]
        {
            EroicaPalette.Sakura,        // Soft pink
            EroicaPalette.SakuraPale,    // Pale pink-white
            new Color(255, 120, 160),    // Vivid rose
            EroicaPalette.PollenGold,    // Golden pollen accent
        };

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 16;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.MeleeNoSpeed;
            Projectile.penetrate = 1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 180;
            Projectile.extraUpdates = 1;
            Projectile.alpha = 255; // Hide default sprite
            Projectile.localNPCHitCooldown = 10;
            Projectile.usesLocalNPCImmunity = true;
        }

        public override void AI()
        {
            // Initialize on first frame
            if (Projectile.localAI[0] == 0f)
            {
                Projectile.localAI[0] = 1f;
                spin = Projectile.ai[1] * MathHelper.PiOver4;
                drift = Main.rand.NextFloat(-0.5f, 0.5f);
            }

            // Gentle petal spin
            spin += 0.06f;
            Projectile.rotation = spin;

            bool empowered = Projectile.ai[0] == 1f;
            float homeRange = empowered ? 500f : 350f;
            float homeLerp = empowered ? 0.08f : 0.04f;
            float maxSpeed = empowered ? 14f : 9f;

            // Sinusoidal drift perpendicular to motion (petal flutter)
            float time = Projectile.localAI[0]++;
            Vector2 perp = new Vector2(-Projectile.velocity.Y, Projectile.velocity.X);
            if (perp != Vector2.Zero)
                perp.Normalize();
            Projectile.Center += perp * (float)Math.Sin(time * 0.12f + drift) * 0.8f;

            // Homing
            NPC target = FindClosestNPC(homeRange);
            if (target != null)
            {
                Vector2 toTarget = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX);
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, toTarget * maxSpeed, homeLerp);
            }
            else
            {
                // Gentle deceleration when no target
                Projectile.velocity *= 0.98f;
            }

            // Slow fade-in
            if (Projectile.alpha > 0)
                Projectile.alpha = Math.Max(0, Projectile.alpha - 20);

            // Spawn a faint sakura dust trail
            if (Main.rand.NextBool(3))
            {
                Dust d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height,
                    DustID.PinkTorch, 0f, 0f, 100, default, 0.6f);
                d.noGravity = true;
                d.velocity *= 0.3f;
            }
        }

        private NPC FindClosestNPC(float range)
        {
            NPC closest = null;
            float closestDist = range * range;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly || npc.dontTakeDamage || npc.immortal)
                    continue;
                float dist = Vector2.DistanceSquared(Projectile.Center, npc.Center);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closest = npc;
                }
            }
            return closest;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            try
            {
            IncisorOrbRenderer.DrawOrbVisuals(Main.spriteBatch, Projectile, IncisorOrbRenderer.Eroica, ref _strip);
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
            // Scatter a small burst of sakura dust on death
            for (int i = 0; i < 6; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(3f, 3f);
                Dust d = Dust.NewDustDirect(Projectile.Center, 0, 0,
                    DustID.PinkTorch, vel.X, vel.Y, 80, default, 0.8f);
                d.noGravity = true;
            }
        }
    }
}
