using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.MoonlightSonata.Enemies
{
    /// <summary>
    /// Lunar Blaze - A flaming ball projectile fired by Lunus.
    /// Spread shot with fiery particles.
    /// </summary>
    public class LunarBlazeProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Content/MoonlightSonata/Enemies/LunarBlaze";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
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
            Projectile.ignoreWater = false;
            Projectile.alpha = 50;
            Projectile.light = 0.4f;
        }

        public override void AI()
        {
            // Rotation based on velocity
            Projectile.rotation = Projectile.velocity.ToRotation();

            // Slight gravity
            Projectile.velocity.Y += 0.08f;
            if (Projectile.velocity.Y > 12f)
                Projectile.velocity.Y = 12f;

            // Lighting - blue-purple fire
            Lighting.AddLight(Projectile.Center, 0.4f, 0.3f, 0.8f);

            // Flame particles
            if (Main.rand.NextBool(2))
            {
                Dust flame = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.PurpleTorch, 0f, 0f, 100, default, 1.3f);
                flame.noGravity = true;
                flame.velocity = -Projectile.velocity * 0.2f + Main.rand.NextVector2Circular(1f, 1f);
            }

            // Blue flame particles - light blue ice
            if (Main.rand.NextBool(3))
            {
                Dust blueFlame = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.IceTorch, 0f, 0f, 100, default, 1f);
                blueFlame.noGravity = true;
                blueFlame.velocity = -Projectile.velocity * 0.3f + Main.rand.NextVector2Circular(1.5f, 1.5f);
            }

            // Ember particles
            if (Main.rand.NextBool(5))
            {
                Dust ember = Dust.NewDustDirect(Projectile.Center, 1, 1, DustID.Shadowflame, 0f, 0f, 0, default, 0.6f);
                ember.noGravity = true;
                ember.velocity = -Projectile.velocity * 0.1f + Main.rand.NextVector2Circular(3f, 3f);
                ember.fadeIn = 1f;
            }
        }

        public override void OnKill(int timeLeft)
        {
            // Fiery explosion
            for (int i = 0; i < 12; i++)
            {
                Dust explosion = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.PurpleTorch, 0f, 0f, 100, default, 1.5f);
                explosion.noGravity = true;
                explosion.velocity = Main.rand.NextVector2Circular(6f, 6f);
            }

            for (int i = 0; i < 8; i++)
            {
                Dust blueExplosion = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.BlueTorch, 0f, 0f, 100, default, 1.2f);
                blueExplosion.noGravity = true;
                blueExplosion.velocity = Main.rand.NextVector2Circular(5f, 5f);
            }

            Terraria.Audio.SoundEngine.PlaySound(SoundID.Item14 with { Volume = 0.5f }, Projectile.Center);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            // Draw trail
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            Vector2 origin = new Vector2(texture.Width / 2, texture.Height / 2);

            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                Vector2 drawPos = Projectile.oldPos[i] + Projectile.Size / 2 - Main.screenPosition;
                float trailAlpha = (float)(Projectile.oldPos.Length - i) / Projectile.oldPos.Length;
                Color trailColor = new Color(100, 80, 200) * trailAlpha * 0.4f;
                float trailScale = Projectile.scale * (0.4f + 0.6f * trailAlpha);

                Main.EntitySpriteDraw(texture, drawPos, null, trailColor, Projectile.oldRot[i], origin, trailScale, SpriteEffects.None, 0);
            }

            // Glow effect - fiery
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.15f) * 0.3f + 0.7f;
            Color glowColor = new Color(120, 80, 180) * pulse * 0.5f;
            
            for (int i = 0; i < 4; i++)
            {
                Vector2 offset = new Vector2(2f, 0f).RotatedBy(MathHelper.TwoPi * i / 4 + Main.GameUpdateCount * 0.1f);
                Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition + offset, null, glowColor, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);
            }

            return true;
        }

        public override Color? GetAlpha(Color lightColor)
        {
            return new Color(255, 255, 255, 180);
        }
    }
}
