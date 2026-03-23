using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Eroica.Weapons.SakurasBlossom.Projectiles
{
    /// <summary>
    /// Fast sparkling laser projectile fired by <see cref="SakuraSpectralTurret"/>.
    /// Mildly homes toward nearby enemies after a brief delay, leaving a sakura-pink
    /// afterimage trail with bloom at the head.
    /// </summary>
    public class SakuraSparkLaser : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/PointBloom";

        // Cached texture references
        private static Asset<Texture2D> _softGlowTex;
        private static Asset<Texture2D> _starTex;
        private static Asset<Texture2D> _pointBloomTex;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 8;
            ProjectileID.Sets.TrailingMode[Type] = 2;

            _softGlowTex = ModContent.Request<Texture2D>(
                "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftGlow");
            _starTex = ModContent.Request<Texture2D>(
                "MagnumOpus/Assets/VFX Asset Library/Projectiles/4PointStarShiningProjectile");
            _pointBloomTex = ModContent.Request<Texture2D>(
                "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/PointBloom");
        }

        public override void SetDefaults()
        {
            Projectile.width = 8;
            Projectile.height = 8;
            Projectile.friendly = true;
            Projectile.penetrate = 1;
            Projectile.tileCollide = true;
            Projectile.timeLeft = 90;
            Projectile.DamageType = DamageClass.MeleeNoSpeed;
            Projectile.extraUpdates = 1;
            Projectile.alpha = 255; // Hide default sprite
        }

        public override void AI()
        {
            Projectile.localAI[0]++;
            float timer = Projectile.localAI[0];

            // Face velocity direction
            Projectile.rotation = Projectile.velocity.ToRotation();

            // Mild homing after 10 ticks
            if (timer > 10f)
            {
                NPC target = FindNearestEnemy(400f);
                if (target != null)
                {
                    Vector2 dirToTarget = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX);
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, dirToTarget * 18f, 0.04f);
                }
            }

            // Speed clamp: keep between 14 and 20
            float speed = Projectile.velocity.Length();
            if (speed < 14f)
            {
                Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.UnitX) * 14f;
            }
            else if (speed > 20f)
            {
                Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.UnitX) * 20f;
            }

            // Dust trail every 2 ticks
            if (timer % 2 == 0)
            {
                Dust d = Dust.NewDustDirect(
                    Projectile.Center,
                    0, 0,
                    DustID.PinkFairy,
                    Main.rand.NextFloat(-0.5f, 0.5f),
                    Main.rand.NextFloat(-0.5f, 0.5f),
                    100,
                    default,
                    0.6f);
                d.noGravity = true;
            }

            // Lighting
            Lighting.AddLight(Projectile.Center, 0.4f, 0.2f, 0.3f);
        }

        /// <summary>
        /// Finds the nearest active, targetable NPC within the given range.
        /// </summary>
        private NPC FindNearestEnemy(float maxDist)
        {
            NPC closest = null;
            float closestDistSq = maxDist * maxDist;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || !npc.CanBeChasedBy())
                    continue;

                float distSq = Vector2.DistanceSquared(Projectile.Center, npc.Center);
                if (distSq < closestDistSq)
                {
                    closestDistSq = distSq;
                    closest = npc;
                }
            }

            return closest;
        }

        public override void OnKill(int timeLeft)
        {
            // Radial burst of 6 pink fairy dust
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi / 6f * i;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 4f);
                Dust d = Dust.NewDustDirect(
                    Projectile.Center,
                    0, 0,
                    DustID.PinkFairy,
                    vel.X, vel.Y,
                    80,
                    default,
                    0.8f);
                d.noGravity = true;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            Texture2D glowTex = _softGlowTex?.Value;
            Texture2D starTex = _starTex?.Value;
            Texture2D bloomTex = _pointBloomTex?.Value;

            if (glowTex == null || starTex == null || bloomTex == null)
                return false;

            Vector2 glowOrigin = glowTex.Size() / 2f;
            Vector2 starOrigin = starTex.Size() / 2f;
            Vector2 bloomOrigin = bloomTex.Size() / 2f;

            // --- Additive pass: afterimage trail + head bloom ---
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            // Trail: afterimage from oldPos
            Color trailTint = new Color(255, 150, 180);
            for (int i = 1; i < Projectile.oldPos.Length; i++)
            {
                Vector2 oldCenter = Projectile.oldPos[i];
                if (oldCenter == Vector2.Zero)
                    continue;

                oldCenter += Projectile.Size / 2f - Main.screenPosition;
                float progress = 1f - i / (float)Projectile.oldPos.Length;
                Color trailColor = trailTint * (progress * 0.3f);
                trailColor.A = 0;

                spriteBatch.Draw(glowTex, oldCenter, null, trailColor, 0f, glowOrigin, 0.04f, SpriteEffects.None, 0f);
            }

            // Head: SoftGlow
            Color headGlowColor = new Color(255, 150, 180) * 0.4f;
            headGlowColor.A = 0;
            spriteBatch.Draw(glowTex, drawPos, null, headGlowColor, 0f, glowOrigin, 0.08f, SpriteEffects.None, 0f);

            // Head: 4-point star, slowly rotating
            float starRot = Main.GlobalTimeWrappedHourly * 3f + Projectile.whoAmI;
            Color starColor = new Color(255, 200, 220);
            starColor.A = 0;
            spriteBatch.Draw(starTex, drawPos, null, starColor, starRot, starOrigin, 0.035f, SpriteEffects.None, 0f);

            // Head: PointBloom bright core
            Color coreColor = Color.White * 0.5f;
            coreColor.A = 0;
            spriteBatch.Draw(bloomTex, drawPos, null, coreColor, 0f, bloomOrigin, 0.02f, SpriteEffects.None, 0f);

            spriteBatch.End();

            // --- Restore default SpriteBatch state ---
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

            return false;
        }
    }
}
