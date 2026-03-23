using System;
using MagnumOpus.Common;
using MagnumOpus.Common.Systems.VFX.Sparkle;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Eroica.Weapons.CelestialValor.Projectiles
{
    /// <summary>
    /// CelestialValorZoneOrb -- Small homing orb projectile spawned from CelestialValorNoiseZone.
    /// After a 15-tick grace period, homes toward nearest NPC within 600px.
    /// Rendered as a 3-layer additive bloom (SoftGlow outer, 4PointStar body, PointBloom core).
    ///
    /// Sparkle explosion on kill is auto-applied by ThemeSparkleGlobalProjectile
    /// since this projectile is in the Eroica namespace.
    /// </summary>
    public class CelestialValorZoneOrb : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Textures/InvisibleProjectile";

        private int timer;

        // Cached textures
        private static Asset<Texture2D> _softGlow;
        private static Asset<Texture2D> _star4Point;
        private static Asset<Texture2D> _pointBloom;

        // Eroica fire palette
        private static readonly Color OrbOuter = new Color(255, 80, 40);
        private static readonly Color OrbBody = new Color(255, 200, 80);

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.DrawScreenCheckFluff[Projectile.type] = 200;
        }

        public override void SetDefaults()
        {
            Projectile.width = 12;
            Projectile.height = 12;
            Projectile.friendly = true;
            Projectile.penetrate = 1;
            Projectile.tileCollide = true;
            Projectile.timeLeft = 120;
            Projectile.DamageType = DamageClass.MeleeNoSpeed;
            Projectile.extraUpdates = 1;
        }

        public override void AI()
        {
            timer++;

            // After 15-tick grace period, home toward nearest NPC
            if (timer > 15)
            {
                NPC target = FindNearestNPC(600f);
                if (target != null)
                {
                    Vector2 dirToTarget = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, dirToTarget * 14f, 0.06f);
                }
            }

            // Clamp speed 8-16
            float speed = Projectile.velocity.Length();
            if (speed > 0.1f)
            {
                if (speed < 8f)
                    Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.Zero) * 8f;
                else if (speed > 16f)
                    Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.Zero) * 16f;
            }

            // Rotation follows velocity
            if (Projectile.velocity.LengthSquared() > 1f)
                Projectile.rotation = Projectile.velocity.ToRotation();

            // Dust trail every 3 ticks
            if (!Main.dedServ && timer % 3 == 0)
            {
                Vector2 dustVel = -Projectile.velocity * 0.05f + Main.rand.NextVector2Circular(0.5f, 0.5f);
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.Torch, dustVel, 0,
                    default, Main.rand.NextFloat(0.5f, 0.9f));
                d.noGravity = true;
            }

            // Lighting
            Lighting.AddLight(Projectile.Center, 0.5f, 0.2f, 0.1f);
        }

        public override void OnKill(int timeLeft)
        {
            if (Main.dedServ) return;

            // 6 Torch dust radial burst
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 4f);
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.Torch, vel, 0,
                    default, Main.rand.NextFloat(0.6f, 1.0f));
                d.noGravity = true;
            }
        }

        private NPC FindNearestNPC(float maxRange)
        {
            NPC closest = null;
            float closestDist = maxRange * maxRange;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly || npc.dontTakeDamage || npc.immortal)
                    continue;

                float distSq = Vector2.DistanceSquared(Projectile.Center, npc.Center);
                if (distSq < closestDist)
                {
                    closestDist = distSq;
                    closest = npc;
                }
            }

            return closest;
        }

        // =====================================================================
        // RENDERING
        // =====================================================================

        public override bool PreDraw(ref Color lightColor)
        {
            if (Main.dedServ) return false;

            SpriteBatch sb = Main.spriteBatch;
            try
            {
                LoadTextures();

                Texture2D softGlow = _softGlow?.Value;
                Texture2D star4Point = _star4Point?.Value;
                Texture2D pointBloom = _pointBloom?.Value;
                if (softGlow == null || star4Point == null || pointBloom == null) return false;

                Vector2 drawPos = Projectile.Center - Main.screenPosition;
                Vector2 glowOrigin = softGlow.Size() / 2f;
                Vector2 starOrigin = star4Point.Size() / 2f;
                Vector2 bloomOrigin = pointBloom.Size() / 2f;

                float starRotation = (float)Main.timeForVisualEffects * 0.08f + Projectile.whoAmI * 1.5f;

                sb.End();
                sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                    SamplerState.LinearClamp, DepthStencilState.None,
                    RasterizerState.CullCounterClockwise, null,
                    Main.GameViewMatrix.TransformationMatrix);

                // Layer 1: SoftGlow outer at 0.08f scale, scarlet-orange
                sb.Draw(softGlow, drawPos, null,
                    (OrbOuter with { A = 0 }) * 0.3f,
                    0f, glowOrigin, 0.08f, SpriteEffects.None, 0f);

                // Layer 2: 4PointStar body at 0.04f scale, rotating, gold
                sb.Draw(star4Point, drawPos, null,
                    (OrbBody with { A = 0 }) * 0.8f,
                    starRotation, starOrigin, 0.04f, SpriteEffects.None, 0f);

                // Layer 3: PointBloom core at 0.02f scale, white
                sb.Draw(pointBloom, drawPos, null,
                    (Color.White with { A = 0 }) * 0.5f,
                    0f, bloomOrigin, 0.02f, SpriteEffects.None, 0f);
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

        private static void LoadTextures()
        {
            const string Bloom = "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/";
            const string Projectiles = "MagnumOpus/Assets/VFX Asset Library/Projectiles/";
            _softGlow ??= ModContent.Request<Texture2D>(Bloom + "SoftGlow", AssetRequestMode.ImmediateLoad);
            _pointBloom ??= ModContent.Request<Texture2D>(Bloom + "PointBloom", AssetRequestMode.ImmediateLoad);
            _star4Point ??= ModContent.Request<Texture2D>(Projectiles + "4PointStarShiningProjectile", AssetRequestMode.ImmediateLoad);
        }
    }
}
