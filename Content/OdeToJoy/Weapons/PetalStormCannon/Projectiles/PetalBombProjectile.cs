using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.OdeToJoy.Weapons.PetalStormCannon.Projectiles
{
    /// <summary>
    /// Petal Bomb — Lobbed by Symphony of Blossoms accessory on petal storm trigger.
    /// Arcs toward target, detonates on contact or after 2s with a radial petal burst.
    /// </summary>
    public class PetalBombProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/VFX Asset Library/Theme Specific/Ode to Joy/Projectiles/OJ Rose Petal";

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Generic;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 120;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = false;
        }

        public override void AI()
        {
            Projectile.rotation += Projectile.velocity.X * 0.04f;
            Projectile.velocity.Y += 0.2f; // Gravity arc

            // Petal dust trail
            if (Main.rand.NextBool(3))
            {
                Color col = Main.rand.NextBool()
                    ? new Color(220, 100, 120)  // PetalPink
                    : new Color(255, 200, 50);  // BloomGold
                Dust dust = Dust.NewDustPerfect(
                    Projectile.Center + Main.rand.NextVector2Circular(4f, 4f),
                    DustID.RainbowMk2,
                    Projectile.velocity * -0.2f + Main.rand.NextVector2Circular(1f, 1f),
                    newColor: col,
                    Scale: Main.rand.NextFloat(0.3f, 0.6f));
                dust.noGravity = true;
                dust.fadeIn = 0.4f;
            }
        }

        public override void OnKill(int timeLeft)
        {
            // Radial petal burst on detonation
            for (int i = 0; i < 16; i++)
            {
                float angle = MathHelper.TwoPi / 16f * i;
                float speed = Main.rand.NextFloat(2f, 5f);
                Vector2 vel = angle.ToRotationVector2() * speed;
                Color col = (i % 3) switch
                {
                    0 => new Color(220, 100, 120),  // PetalPink
                    1 => new Color(255, 200, 50),   // BloomGold
                    _ => new Color(255, 250, 200)    // JubilantLight
                };
                Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.RainbowMk2, vel,
                    newColor: col, Scale: Main.rand.NextFloat(0.4f, 0.8f));
                dust.noGravity = true;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
            if (tex == null) return true;
            Vector2 origin = tex.Size() / 2f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            // Additive glow behind
            Main.spriteBatch.Draw(tex, drawPos, null,
                new Color(255, 200, 50, 0) * 0.4f, Projectile.rotation, origin, 0.6f, SpriteEffects.None, 0f);
            // Main sprite
            Main.spriteBatch.Draw(tex, drawPos, null,
                lightColor, Projectile.rotation, origin, 0.4f, SpriteEffects.None, 0f);

            // Theme blossom sparkle accent
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState,
                DepthStencilState.None, RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.TransformationMatrix);
            OdeToJoyVFXLibrary.DrawThemeBlossomSparkle(Main.spriteBatch, Projectile.Center, 1f, 0.5f);
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                DepthStencilState.None, RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.TransformationMatrix);

            return false;
        }
    }
}
