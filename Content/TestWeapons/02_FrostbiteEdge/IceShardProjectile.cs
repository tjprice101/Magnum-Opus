using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Content.TestWeapons._02_FrostbiteEdge
{
    /// <summary>
    /// ❄️ Ice Shard Projectile — bouncing ice shards spawned by Step 2 (Shatter Strike).
    /// Launches in a fan, bounces off tiles 2 times, scatters frost dust.
    /// </summary>
    public class IceShardProjectile : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.IceBolt;

        private int MaxBounces => 2;
        private int BounceCount { get => (int)Projectile.ai[0]; set => Projectile.ai[0] = value; }

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailingMode[Type] = 2;
            ProjectileID.Sets.TrailCacheLength[Type] = 10;
        }

        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.MeleeNoSpeed;
            Projectile.penetrate = 3;
            Projectile.timeLeft = 180;
            Projectile.tileCollide = true;
            Projectile.alpha = 0;
            Projectile.extraUpdates = 1;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;
            Projectile.velocity.Y += 0.18f;

            // Ice dust trail
            if (Main.rand.NextBool(2))
            {
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.IceTorch,
                    -Projectile.velocity * 0.15f + Main.rand.NextVector2Circular(0.5f, 0.5f),
                    0, default, 0.8f);
                d.noGravity = true;
            }

            if (Main.rand.NextBool(4))
            {
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.Frost,
                    -Projectile.velocity * 0.1f, 100, default, 0.5f);
                d.noGravity = true;
            }

            Lighting.AddLight(Projectile.Center, 0.15f, 0.35f, 0.6f);
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (BounceCount >= MaxBounces)
                return true;

            BounceCount++;

            if (Math.Abs(Projectile.velocity.X - oldVelocity.X) > float.Epsilon)
                Projectile.velocity.X = -oldVelocity.X * 0.75f;
            if (Math.Abs(Projectile.velocity.Y - oldVelocity.Y) > float.Epsilon)
                Projectile.velocity.Y = -oldVelocity.Y * 0.75f;

            // Bounce burst
            for (int i = 0; i < 5; i++)
            {
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.IceTorch,
                    Main.rand.NextVector2Circular(3f, 3f), 0, default, 1f);
                d.noGravity = true;
            }

            Terraria.Audio.SoundEngine.PlaySound(SoundID.Item50 with { Volume = 0.4f, Pitch = 0.3f },
                Projectile.Center);

            return false;
        }

        public override void OnKill(int timeLeft)
        {
            // Death burst with ice sparkles
            for (int i = 0; i < 4; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(4f, 4f);
                Color sparkColor = Color.Lerp(new Color(100, 200, 255), Color.White, Main.rand.NextFloat(0.5f));
                var spark = new GlowSparkParticle(Projectile.Center + Main.rand.NextVector2Circular(4f, 4f),
                    vel, sparkColor, Main.rand.NextFloat(0.25f, 0.45f), Main.rand.Next(10, 18));
                MagnumParticleHandler.SpawnParticle(spark);
            }

            for (int i = 0; i < 8; i++)
            {
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.IceTorch,
                    Main.rand.NextVector2Circular(5f, 5f), 0, default, 1.1f);
                d.noGravity = true;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Texture2D tex = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            Vector2 origin = tex.Size() * 0.5f;

            // Afterimage trail
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;
                float progress = (float)i / Projectile.oldPos.Length;
                float alpha = 1f - progress;
                Color trailColor = Color.Lerp(new Color(100, 200, 255), new Color(200, 240, 255), progress) * alpha * 0.5f;
                trailColor.A = 0;
                Vector2 trailPos = Projectile.oldPos[i] + Projectile.Size * 0.5f - Main.screenPosition;
                sb.Draw(tex, trailPos, null, trailColor, Projectile.oldRot[i], origin, Projectile.scale * (1f - progress * 0.3f), SpriteEffects.None, 0f);
            }

            // Core glow
            Texture2D glow = Terraria.GameContent.TextureAssets.Extra[98].Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float pulse = 1f + (float)Math.Sin(Main.GameUpdateCount * 0.15f) * 0.08f;
            Color coreColor = new Color(120, 210, 255, 0) * 0.6f;
            sb.Draw(glow, drawPos, null, coreColor, 0f, glow.Size() * 0.5f, 0.18f * pulse, SpriteEffects.None, 0f);

            // Sprite
            sb.Draw(tex, drawPos, null, lightColor, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0f);

            return false;
        }
    }
}
