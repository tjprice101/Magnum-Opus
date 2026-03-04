using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.SwanLake.ResonantWeapons.FeatheroftheIridescentFlock.Utilities;
using MagnumOpus.Content.SwanLake.Debuffs;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.VFX;

namespace MagnumOpus.Content.SwanLake.ResonantWeapons.FeatheroftheIridescentFlock.Projectiles
{
    /// <summary>
    /// Small homing crystal shard fired by IridescentCrystalProj in 3-burst volleys.
    /// Gentle homing, iridescent shimmer trail.
    /// Foundation-pattern rendering: 2-layer bloom + vanilla dust.
    /// </summary>
    public class CrystalShardProj : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Textures/InvisibleProjectile";

        public ref float Timer => ref Projectile.ai[0];

        private const int TrailLength = 10;
        private Vector2[] oldPos = new Vector2[TrailLength];

        public override void SetStaticDefaults() { ProjectileID.Sets.TrailCacheLength[Projectile.type] = TrailLength; }

        public override void SetDefaults()
        {
            Projectile.width = 8;
            Projectile.height = 8;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 120;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.extraUpdates = 1;
            Projectile.alpha = 50;
        }

        public override void AI()
        {
            Timer++;
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;

            // --- Gentle homing ---
            if (Timer > 8)
            {
                NPC target = FindClosestNPC(400f);
                if (target != null)
                {
                    Vector2 toTarget = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitY);
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, toTarget * Projectile.velocity.Length(), 0.03f);
                }
            }

            // --- Trail recording ---
            for (int i = TrailLength - 1; i > 0; i--)
                oldPos[i] = oldPos[i - 1];
            oldPos[0] = Projectile.Center;

            // --- Iridescent shimmer dust ---
            if (Timer % 3 == 0)
            {
                Color c = FlockUtils.GetIridescent(Timer * 0.03f);
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.WhiteTorch,
                    -Projectile.velocity * 0.15f + Main.rand.NextVector2Circular(1, 1), 0, c, 0.4f);
                d.noGravity = true;
            }

            Lighting.AddLight(Projectile.Center, 0.3f, 0.3f, 0.4f);
        }

        private NPC FindClosestNPC(float range)
        {
            NPC closest = null;
            float bestDist = range;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly || npc.dontTakeDamage) continue;
                float dist = Vector2.Distance(Projectile.Center, npc.Center);
                if (dist < bestDist)
                {
                    bestDist = dist;
                    closest = npc;
                }
            }
            return closest;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<SwansMark>(), 180);

            for (int i = 0; i < 4; i++)
            {
                Color c = FlockUtils.GetIridescent(i / 4f);
                Dust d = Dust.NewDustPerfect(target.Center, DustID.WhiteTorch,
                    Main.rand.NextVector2Circular(3, 3), 0, c, 0.6f);
                d.noGravity = true;
            }
        }

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 4; i++)
            {
                Color c = FlockUtils.GetIridescent(i / 4f + Timer * 0.01f);
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.WhiteTorch,
                    Main.rand.NextVector2Circular(2, 2), 0, c, 0.4f);
                d.noGravity = true;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Vector2 screenPos = Main.screenPosition;

            try
            {
                sb.End();
                sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                    DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

                Texture2D bloom = MagnumTextureRegistry.GetSoftGlow();
                Texture2D point = MagnumTextureRegistry.GetPointBloom();

                // --- Iridescent bloom trail ---
                if (bloom != null)
                {
                    Vector2 bOrigin = bloom.Size() * 0.5f;
                    for (int i = TrailLength - 1; i >= 1; i--)
                    {
                        if (oldPos[i] == Vector2.Zero) continue;
                        float progress = 1f - i / (float)TrailLength;
                        Color trailColor = FlockUtils.GetIridescent(
                            i / (float)TrailLength + (float)Main.timeForVisualEffects * 0.01f);
                        sb.Draw(bloom, oldPos[i] - screenPos, null, trailColor * (progress * 0.25f),
                            0f, bOrigin, 0.08f + progress * 0.06f, SpriteEffects.None, 0f);
                    }
                }

                // --- Core bloom (2 layers) ---
                Vector2 drawPos = Projectile.Center - screenPos;

                // Outer iridescent glow
                if (bloom != null)
                {
                    Vector2 bOrigin = bloom.Size() * 0.5f;
                    Color outerColor = FlockUtils.GetIridescent(Timer * 0.02f);
                    sb.Draw(bloom, drawPos, null, outerColor * 0.3f, 0f, bOrigin, 0.15f, SpriteEffects.None, 0f);
                }

                // White core
                if (point != null)
                {
                    Vector2 pOrigin = point.Size() * 0.5f;
                    sb.Draw(point, drawPos, null, Color.White * 0.6f, 0f, pOrigin, 0.08f, SpriteEffects.None, 0f);
                }

                // --- Draw shard sprite ---
                Texture2D tex = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
                if (tex != null)
                {
                    sb.End();
                    sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                        DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

                    Vector2 origin = tex.Size() * 0.5f;
                    Color tint = FlockUtils.GetIridescent(Timer * 0.015f);
                    Color drawColor = Color.Lerp(Projectile.GetAlpha(lightColor), tint, 0.2f);
                    sb.Draw(tex, drawPos, null, drawColor, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0f);
                }
            }
            catch { }
            finally
            {
                sb.End();
                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                    DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            }

            return false;
        }
    }
}
