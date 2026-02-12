using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Content.TestWeapons._01_InfernalCleaver
{
    /// <summary>
    /// Bouncing ember shard — spawned by the Infernal Cleaver's Step 2 (Rising Uppercut).
    /// Arcs through the air with gravity, bounces off tiles 2 times, then dies.
    /// Leaves small fire-dust trail and deals contact damage.
    /// </summary>
    public class EmberShardProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Content/TestWeapons/01_InfernalCleaver/EmberShardFragment";

        private const int MaxBounces = 2;

        private int BounceCount
        {
            get => (int)Projectile.ai[0];
            set => Projectile.ai[0] = value;
        }

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailingMode[Type] = 0;
            ProjectileID.Sets.TrailCacheLength[Type] = 12;
        }

        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.MeleeNoSpeed;
            Projectile.tileCollide = true;
            Projectile.penetrate = 3;
            Projectile.timeLeft = 180;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 12;
            Projectile.alpha = 255; // Invisible sprite — drawn via PreDraw
        }

        public override void AI()
        {
            // Gravity
            Projectile.velocity.Y += 0.25f;
            if (Projectile.velocity.Y > 16f) Projectile.velocity.Y = 16f;

            // Rotation follows velocity
            Projectile.rotation = Projectile.velocity.ToRotation();

            // Fire dust trail
            if (Main.rand.NextBool(2))
            {
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.Torch,
                    -Projectile.velocity * 0.2f + Main.rand.NextVector2Circular(1f, 1f),
                    0, default, 1.3f);
                d.noGravity = true;
                d.fadeIn = 1.1f;
            }

            // Sparks
            if (Main.rand.NextBool(3))
            {
                Dust d2 = Dust.NewDustPerfect(Projectile.Center, DustID.Enchanted_Gold,
                    Main.rand.NextVector2Circular(2f, 2f), 0, default, 0.7f);
                d2.noGravity = true;
            }

            // Lighting
            Lighting.AddLight(Projectile.Center, 0.6f, 0.3f, 0.05f);
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            BounceCount++;
            if (BounceCount >= MaxBounces)
                return true; // Kill projectile

            // Bounce
            if (Math.Abs(Projectile.velocity.X - oldVelocity.X) > float.Epsilon)
                Projectile.velocity.X = -oldVelocity.X * 0.7f;
            if (Math.Abs(Projectile.velocity.Y - oldVelocity.Y) > float.Epsilon)
                Projectile.velocity.Y = -oldVelocity.Y * 0.6f;

            // Bounce impact particles
            for (int i = 0; i < 6; i++)
            {
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.Torch,
                    Main.rand.NextVector2Circular(4f, 4f), 0, default, 1.2f);
                d.noGravity = true;
            }

            Terraria.Audio.SoundEngine.PlaySound(SoundID.Item10 with { Pitch = 0.5f, Volume = 0.5f }, Projectile.Center);
            return false;
        }

        public override void OnKill(int timeLeft)
        {
            // Death burst
            for (int i = 0; i < 10; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(5f, 5f);
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.Torch, vel, 0, default, 1.5f);
                d.noGravity = true;
            }

            for (int i = 0; i < 4; i++)
            {
                var spark = new GlowSparkParticle(Projectile.Center,
                    Main.rand.NextVector2Circular(6f, 6f),
                    new Color(255, 120, 20), Main.rand.NextFloat(0.3f, 0.5f), Main.rand.Next(12, 20));
                MagnumParticleHandler.SpawnParticle(spark);
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int remainingDamageCount)
        {
            for (int i = 0; i < 5; i++)
            {
                Dust d = Dust.NewDustPerfect(target.Center, DustID.Torch,
                    Main.rand.NextVector2Circular(5f, 5f), 0, default, 1.3f);
                d.noGravity = true;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;

            // Draw afterimage trail
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) break;
                float progress = (float)i / Projectile.oldPos.Length;
                float alpha = 1f - progress;
                Color trailColor = Color.Lerp(new Color(255, 180, 40), new Color(200, 50, 10), progress);
                trailColor *= alpha * 0.6f;
                trailColor.A = 0;

                Texture2D glowTex = Terraria.GameContent.TextureAssets.Extra[98].Value;
                Vector2 drawPos = Projectile.oldPos[i] + Projectile.Size * 0.5f - Main.screenPosition;
                float scale = (1f - progress) * 0.15f;

                sb.Draw(glowTex, drawPos, null, trailColor, 0f, glowTex.Size() * 0.5f, scale, SpriteEffects.None, 0f);
            }

            // Draw the actual ember shard fragment sprite
            Texture2D shardTex = ModContent.Request<Texture2D>(Texture).Value;
            Vector2 shardPos = Projectile.Center - Main.screenPosition;
            Vector2 shardOrigin = shardTex.Size() * 0.5f;
            float shardRot = Projectile.rotation;
            float shardScale = Projectile.scale;

            // Normal pass — the visible shard
            sb.Draw(shardTex, shardPos, null, lightColor, shardRot, shardOrigin, shardScale, SpriteEffects.None, 0f);

            // Additive glow pass — fiery hot glow behind the shard
            Color glowColor = new Color(255, 140, 30, 0) * 0.5f;
            sb.Draw(shardTex, shardPos, null, glowColor, shardRot, shardOrigin, shardScale * 1.15f, SpriteEffects.None, 0f);

            // Core glow at current position
            Texture2D coreTex = Terraria.GameContent.TextureAssets.Extra[98].Value;
            float pulse = 1f + (float)Math.Sin(Main.GameUpdateCount * 0.2f) * 0.1f;
            Color coreColor = new Color(255, 140, 20, 0) * 0.8f;

            sb.Draw(coreTex, shardPos, null, coreColor, 0f, coreTex.Size() * 0.5f, 0.2f * pulse, SpriteEffects.None, 0f);
            sb.Draw(coreTex, shardPos, null, Color.White * 0.5f, 0f, coreTex.Size() * 0.5f, 0.08f * pulse, SpriteEffects.None, 0f);

            return false;
        }
    }
}
