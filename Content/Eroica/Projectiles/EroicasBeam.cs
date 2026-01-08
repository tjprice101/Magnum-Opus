using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Eroica.Projectiles
{
    /// <summary>
    /// The devastating beam attack fired by Eroica, God of Valor in Phase 2.
    /// Fires straight down at the player after a countdown.
    /// </summary>
    public class EroicasBeam : ModProjectile
    {
        // Use the Energy of Eroica sprite as base
        public override string Texture => "MagnumOpus/Content/Eroica/Projectiles/EnergyOfEroica";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 5;
            ProjectileID.Sets.TrailingMode[Type] = 0;
        }

        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 2000; // Very tall beam
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 60; // 1 second of beam
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.alpha = 0;
            Projectile.light = 1f;
        }

        public override void AI()
        {
            // Intense pink lighting along the beam
            for (int i = 0; i < 20; i++)
            {
                Vector2 lightPos = Projectile.Center + new Vector2(0, i * 100);
                Lighting.AddLight(lightPos, 1f, 0.3f, 0.6f);
            }

            // Beam particles
            for (int i = 0; i < 10; i++)
            {
                Vector2 dustPos = Projectile.position + new Vector2(Main.rand.Next(Projectile.width), Main.rand.Next(Projectile.height));
                Dust dust = Dust.NewDustDirect(dustPos, 1, 1, DustID.PinkTorch, 0f, 0f, 100, default, 2f);
                dust.noGravity = true;
                dust.velocity = new Vector2(Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-1f, 1f));
            }

            // Edge sparkles
            for (int i = 0; i < 3; i++)
            {
                float yPos = Main.rand.Next(Projectile.height);
                Dust sparkle = Dust.NewDustDirect(Projectile.position + new Vector2(0, yPos), Projectile.width, 1, DustID.GoldFlame, 0f, 0f, 0, default, 1.5f);
                sparkle.noGravity = true;
                sparkle.velocity = Main.rand.NextVector2Circular(4f, 2f);
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            // Draw a custom beam effect using primitives
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D texture = Terraria.GameContent.TextureAssets.MagicPixel.Value;
            
            // Main beam
            Rectangle beamRect = new Rectangle((int)(Projectile.position.X - Main.screenPosition.X), 
                                               (int)(Projectile.position.Y - Main.screenPosition.Y), 
                                               Projectile.width, Projectile.height);
            
            // Core (bright pink)
            Color coreColor = new Color(255, 100, 180, 200);
            spriteBatch.Draw(texture, beamRect, coreColor);
            
            // Inner glow (lighter pink)
            Rectangle innerRect = new Rectangle(beamRect.X - 5, beamRect.Y, beamRect.Width + 10, beamRect.Height);
            Color innerColor = new Color(255, 150, 200, 100);
            spriteBatch.Draw(texture, innerRect, innerColor);
            
            // Outer glow (faint pink)
            Rectangle outerRect = new Rectangle(beamRect.X - 15, beamRect.Y, beamRect.Width + 30, beamRect.Height);
            Color outerColor = new Color(255, 180, 220, 50);
            spriteBatch.Draw(texture, outerRect, outerColor);

            return false; // Don't draw the default sprite
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            // Custom collision for the beam
            return projHitbox.Intersects(targetHitbox);
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            // Heavy knockback
            target.velocity.Y = 10f;
        }
    }
}
