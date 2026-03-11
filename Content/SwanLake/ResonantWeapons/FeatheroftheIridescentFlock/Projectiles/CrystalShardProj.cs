using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.SwanLake.ResonantWeapons.FeatheroftheIridescentFlock.Utilities;
using MagnumOpus.Content.SwanLake.ResonantWeapons.FeatheroftheIridescentFlock.Primitives;
using MagnumOpus.Content.SwanLake.Debuffs;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.VFX.Core;
using Terraria.Graphics.Shaders;
using ReLogic.Content;

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
        private FlockPrimitiveRenderer _trailRenderer;
        private VertexStrip _strip;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 16;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

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

            // Swan Lake rainbow sparkles
            IncisorOrbRenderer.SpawnSwanLakeRainbowSparkles(Projectile);
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

            for (int i = 0; i < 6; i++)
            {
                Color c = FlockUtils.GetIridescent(i / 6f);
                Dust d = Dust.NewDustPerfect(target.Center, DustID.WhiteTorch,
                    Main.rand.NextVector2Circular(4, 4), 0, c, 0.7f);
                d.noGravity = true;
            }

            try
            {
                SwanLakeVFXLibrary.SpawnMixedSparkleImpact(target.Center, 0.5f, 3, 3);
                SwanLakeVFXLibrary.SpawnMusicNotes(target.Center, 1, 8f);
            } catch { }
        }

        public override void OnKill(int timeLeft)
        {
            _trailRenderer?.Dispose();
            _trailRenderer = null;

            for (int i = 0; i < 6; i++)
            {
                Color c = FlockUtils.GetIridescent(i / 6f + Timer * 0.01f);
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.WhiteTorch,
                    Main.rand.NextVector2Circular(3, 3), 0, c, 0.5f);
                d.noGravity = true;
            }

            try
            {
                SwanLakeVFXLibrary.SpawnMixedSparkleImpact(Projectile.Center, 0.5f, 3, 3);
                SwanLakeVFXLibrary.SpawnPrismaticSparkles(Projectile.Center, 3, 12f);
            } catch { }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            try
            {
            IncisorOrbRenderer.DrawOrbVisuals(Main.spriteBatch, Projectile, IncisorOrbRenderer.SwanLake, ref _strip);
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
    }
}
