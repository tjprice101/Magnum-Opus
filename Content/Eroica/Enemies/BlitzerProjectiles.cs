using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Eroica.Enemies
{
    /// <summary>
    /// Blitzer Projectile 1 - Explodes into black/red flames and lightning
    /// </summary>
    public class BlitzerProjectile1 : ModProjectile
    {
        public override string Texture => "MagnumOpus/Content/Eroica/Enemies/BlitzerProjectile1";

        public override void SetDefaults()
        {
            Projectile.width = 18;
            Projectile.height = 18;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 180;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.alpha = 0;
        }

        public override void AI()
        {
            // Rotation to face velocity
            Projectile.rotation = Projectile.velocity.ToRotation();

            // Dark red flame trail
            if (Main.rand.NextBool(2))
            {
                Dust flame = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Torch, 0f, 0f, 100, Color.DarkRed, 1.4f);
                flame.noGravity = true;
                flame.velocity = -Projectile.velocity * 0.15f;
            }

            // Black smoke
            if (Main.rand.NextBool(3))
            {
                Dust smoke = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Smoke, 0f, 0f, 150, Color.Black, 1f);
                smoke.noGravity = true;
                smoke.velocity = -Projectile.velocity * 0.1f;
            }

            // Electric sparkle
            if (Main.rand.NextBool(5))
            {
                Dust electric = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Electric, 0f, 0f, 100, default, 0.8f);
                electric.noGravity = true;
                electric.velocity *= 0.3f;
            }

            Lighting.AddLight(Projectile.Center, 0.5f, 0.1f, 0.1f);
        }

        public override void OnKill(int timeLeft)
        {
            // Explosive black/red flame and lightning burst
            SoundEngine.PlaySound(SoundID.Item14, Projectile.Center);

            // Black and red flames
            for (int i = 0; i < 25; i++)
            {
                Dust flame = Dust.NewDustDirect(Projectile.position - new Vector2(10f, 10f), Projectile.width + 20, Projectile.height + 20, DustID.Torch, 0f, 0f, 100, Color.DarkRed, 2.5f);
                flame.noGravity = true;
                flame.velocity = Main.rand.NextVector2Circular(10f, 10f);
            }

            // Black smoke
            for (int i = 0; i < 18; i++)
            {
                Dust smoke = Dust.NewDustDirect(Projectile.position - new Vector2(10f, 10f), Projectile.width + 20, Projectile.height + 20, DustID.Smoke, 0f, 0f, 180, Color.Black, 2f);
                smoke.velocity = Main.rand.NextVector2Circular(8f, 8f);
            }

            // Lightning bolts
            for (int i = 0; i < 15; i++)
            {
                Dust lightning = Dust.NewDustDirect(Projectile.Center, 1, 1, DustID.Electric, 0f, 0f, 100, default, 1.5f);
                lightning.noGravity = true;
                lightning.velocity = Main.rand.NextVector2Circular(12f, 12f);
            }

            // Electric sound
            SoundEngine.PlaySound(SoundID.Item93 with { Volume = 0.5f }, Projectile.Center);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            Vector2 origin = texture.Size() / 2f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            // Dark red glow
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.1f) * 0.15f + 0.85f;
            Color glowColor = new Color(180, 30, 30, 0) * 0.5f * pulse;

            for (int i = 0; i < 4; i++)
            {
                Vector2 glowOffset = new Vector2(3f, 0f).RotatedBy(i * MathHelper.PiOver2);
                Main.EntitySpriteDraw(texture, drawPos + glowOffset, null, glowColor, Projectile.rotation,
                    origin, Projectile.scale, SpriteEffects.None, 0);
            }

            Main.EntitySpriteDraw(texture, drawPos, null, lightColor, Projectile.rotation,
                origin, Projectile.scale, SpriteEffects.None, 0);

            return false;
        }
    }

    /// <summary>
    /// Blitzer Projectile 2 - Explodes into black/red flames and lightning (variant)
    /// </summary>
    public class BlitzerProjectile2 : ModProjectile
    {
        public override string Texture => "MagnumOpus/Content/Eroica/Enemies/BlitzerProjectile2";

        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 180;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.alpha = 0;
        }

        public override void AI()
        {
            // Spinning rotation
            Projectile.rotation += 0.2f;

            // Black flame trail
            if (Main.rand.NextBool(2))
            {
                Dust flame = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Smoke, 0f, 0f, 180, Color.Black, 1.5f);
                flame.noGravity = true;
                flame.velocity = -Projectile.velocity * 0.12f;
            }

            // Red flame
            if (Main.rand.NextBool(2))
            {
                Dust red = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Torch, 0f, 0f, 100, Color.DarkRed, 1.3f);
                red.noGravity = true;
                red.velocity = -Projectile.velocity * 0.1f + Main.rand.NextVector2Circular(1f, 1f);
            }

            // Lightning crackle
            if (Main.rand.NextBool(4))
            {
                Dust electric = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Electric, 0f, 0f, 100, default, 0.9f);
                electric.noGravity = true;
                electric.velocity = Main.rand.NextVector2Circular(2f, 2f);
            }

            Lighting.AddLight(Projectile.Center, 0.4f, 0.15f, 0.15f);
        }

        public override void OnKill(int timeLeft)
        {
            // Larger explosive burst
            SoundEngine.PlaySound(SoundID.Item14, Projectile.Center);

            // Red flames
            for (int i = 0; i < 30; i++)
            {
                Dust flame = Dust.NewDustDirect(Projectile.position - new Vector2(15f, 15f), Projectile.width + 30, Projectile.height + 30, DustID.Torch, 0f, 0f, 100, Color.DarkRed, 2.8f);
                flame.noGravity = true;
                flame.velocity = Main.rand.NextVector2Circular(12f, 12f);
            }

            // Black smoke cloud
            for (int i = 0; i < 22; i++)
            {
                Dust smoke = Dust.NewDustDirect(Projectile.position - new Vector2(15f, 15f), Projectile.width + 30, Projectile.height + 30, DustID.Smoke, 0f, 0f, 200, Color.Black, 2.5f);
                smoke.velocity = Main.rand.NextVector2Circular(9f, 9f);
            }

            // Lightning storm
            for (int i = 0; i < 20; i++)
            {
                Dust lightning = Dust.NewDustDirect(Projectile.Center, 1, 1, DustID.Electric, 0f, 0f, 100, default, 1.8f);
                lightning.noGravity = true;
                lightning.velocity = Main.rand.NextVector2Circular(15f, 15f);
            }

            // Thunder sound
            SoundEngine.PlaySound(SoundID.Thunder with { Volume = 0.4f }, Projectile.Center);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            Vector2 origin = texture.Size() / 2f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            // Pulsing glow
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.12f) * 0.2f + 0.8f;
            Color glowColor = new Color(150, 40, 40, 0) * 0.5f * pulse;

            for (int i = 0; i < 4; i++)
            {
                Vector2 glowOffset = new Vector2(3f, 0f).RotatedBy(i * MathHelper.PiOver2);
                Main.EntitySpriteDraw(texture, drawPos + glowOffset, null, glowColor, Projectile.rotation,
                    origin, Projectile.scale, SpriteEffects.None, 0);
            }

            Main.EntitySpriteDraw(texture, drawPos, null, lightColor, Projectile.rotation,
                origin, Projectile.scale, SpriteEffects.None, 0);

            return false;
        }
    }
}
