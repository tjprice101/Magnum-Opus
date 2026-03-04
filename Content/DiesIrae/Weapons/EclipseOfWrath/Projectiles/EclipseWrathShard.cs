using MagnumOpus.Common.Systems.VFX.Core;
using MagnumOpus.Content.DiesIrae;
using MagnumOpus.Content.DiesIrae.Weapons.EclipseOfWrath.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.VFX;

namespace MagnumOpus.Content.DiesIrae.Weapons.EclipseOfWrath.Projectiles
{
    /// <summary>
    /// Wrath Shard ? fast piercing fragment from eclipse orb split.
    /// 6 per split (12 on Corona Flare crit). Tight trail, ember glow, ricochet once.
    /// </summary>
    public class EclipseWrathShard : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Textures/InvisibleProjectile";

        private const int TrailLength = 8;
        private Vector2[] trailPositions = new Vector2[TrailLength];
        private int trailHead = 0;
        private bool trailInitialized = false;
        private float timer = 0;
        private int bounceCount = 0;

        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 2;
            Projectile.timeLeft = 120;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.extraUpdates = 1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            // Ricochet once
            if (bounceCount < 1)
            {
                bounceCount++;
                if (Math.Abs(Projectile.velocity.X - oldVelocity.X) > 0.1f)
                    Projectile.velocity.X = -oldVelocity.X;
                if (Math.Abs(Projectile.velocity.Y - oldVelocity.Y) > 0.1f)
                    Projectile.velocity.Y = -oldVelocity.Y;
                return false;
            }
            return true;
        }

        public override void AI()
        {
            timer++;
            Projectile.rotation = Projectile.velocity.ToRotation();

            // Trail
            if (!trailInitialized)
            {
                for (int i = 0; i < TrailLength; i++)
                    trailPositions[i] = Projectile.Center;
                trailInitialized = true;
            }
            trailPositions[trailHead] = Projectile.Center;
            trailHead = (trailHead + 1) % TrailLength;

            // Fire dust
            if (!Main.dedServ && Main.rand.NextBool(3))
            {
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.Torch,
                    -Projectile.velocity * 0.15f + Main.rand.NextVector2Circular(0.5f, 0.5f), 0, default, 0.7f);
                d.noGravity = true;
            }

            Lighting.AddLight(Projectile.Center, 0.4f, 0.12f, 0.03f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.OnFire3, 180);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            Texture2D glow = MagnumTextureRegistry.GetSoftGlow();
            if (glow != null)
            {
                Vector2 origin = glow.Size() / 2f;

                // Trail
                for (int i = 0; i < TrailLength; i++)
                {
                    int idx = (trailHead - 1 - i + TrailLength) % TrailLength;
                    float progress = (float)i / TrailLength;
                    float alpha = (1f - progress) * 0.5f;
                    Color c = Color.Lerp(EclipseOfWrathUtils.CoronaEmber, EclipseOfWrathUtils.ShardCrimson, progress);
                    sb.Draw(glow, trailPositions[idx] - Main.screenPosition, null, c * alpha, 0f, origin,
                        0.018f * (1f - progress * 0.6f), SpriteEffects.None, 0f);
                }
            }

            // Shard body
            EclipseOfWrathUtils.DrawWrathShardBody(sb, Projectile.Center, Projectile.rotation, timer);

            // Dies Irae theme accent layer
            EclipseOfWrathUtils.DrawThemeAccents(sb, Projectile.Center, 1f, 0.6f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            return false;
        }
    }
}