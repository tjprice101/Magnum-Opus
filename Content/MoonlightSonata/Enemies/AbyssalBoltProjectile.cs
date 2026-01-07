using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.MoonlightSonata.Enemies
{
    /// <summary>
    /// Abyssal Bolt - A fast bolt projectile fired by Abyssal Moon Lurker in spreads.
    /// </summary>
    public class AbyssalBoltProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Content/MoonlightSonata/Enemies/Projectile2";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 240;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.alpha = 50;
            Projectile.light = 0.4f;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            Lighting.AddLight(Projectile.Center, 0.5f, 0.4f, 0.7f);

            // White shimmer
            if (Main.rand.NextBool(3))
            {
                Dust shimmer = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.SparksMech, 0f, 0f, 0, Color.White, 0.9f);
                shimmer.noGravity = true;
                shimmer.velocity *= 0.15f;
            }

            // Trail particles
            if (Main.rand.NextBool(2))
            {
                int dustType = Main.rand.NextBool() ? DustID.PurpleTorch : DustID.IceTorch;
                Dust trail = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, dustType, 0f, 0f, 100, default, 1f);
                trail.noGravity = true;
                trail.velocity = -Projectile.velocity * 0.1f;
            }
        }

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 12; i++)
            {
                int dustType = Main.rand.NextBool(3) ? DustID.SparksMech : (Main.rand.NextBool() ? DustID.PurpleTorch : DustID.IceTorch);
                Dust explosion = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, dustType, 0f, 0f, 100, dustType == DustID.SparksMech ? Color.White : default, 1.3f);
                explosion.noGravity = true;
                explosion.velocity = Main.rand.NextVector2Circular(5f, 5f);
            }

            Terraria.Audio.SoundEngine.PlaySound(SoundID.Item10 with { Volume = 0.7f }, Projectile.Center);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            Vector2 origin = new Vector2(texture.Width / 2, texture.Height / 2);

            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                Vector2 drawPos = Projectile.oldPos[i] + Projectile.Size / 2 - Main.screenPosition;
                float trailAlpha = (float)(Projectile.oldPos.Length - i) / Projectile.oldPos.Length;
                Color trailColor = i % 2 == 0 ? new Color(255, 255, 255) : new Color(150, 180, 220);
                trailColor *= trailAlpha * 0.4f;
                float trailScale = Projectile.scale * (0.4f + 0.6f * trailAlpha);

                Main.EntitySpriteDraw(texture, drawPos, null, trailColor, Projectile.oldRot[i], origin, trailScale, SpriteEffects.None, 0);
            }

            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.12f) * 0.2f + 0.8f;
            Color glowColor = Color.White * pulse * 0.4f;
            
            for (int i = 0; i < 4; i++)
            {
                Vector2 offset = new Vector2(3f, 0f).RotatedBy(MathHelper.TwoPi * i / 4);
                Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition + offset, null, glowColor, Projectile.rotation, origin, Projectile.scale * 1.05f, SpriteEffects.None, 0);
            }

            return true;
        }

        public override Color? GetAlpha(Color lightColor)
        {
            return new Color(255, 255, 255, 180);
        }
    }
}
