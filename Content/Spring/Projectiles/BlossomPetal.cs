using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Spring.Projectiles
{
    /// <summary>
    /// BlossomPetal - Cherry blossom petal projectile from Blossom's Edge
    /// Floats gracefully and damages enemies on contact
    /// </summary>
    public class BlossomPetal : ModProjectile
    {
        private static readonly Color SpringPink = new Color(255, 183, 197);
        private static readonly Color SpringWhite = new Color(255, 250, 250);

        public override string Texture => "MagnumOpus/Assets/Particles/SoftGlow";

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 2;
            Projectile.timeLeft = 120;
            Projectile.alpha = 50;
            Projectile.light = 0.3f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.extraUpdates = 1;
        }

        public override void AI()
        {
            // Gentle floating motion
            Projectile.velocity.Y += 0.03f; // Very slight gravity
            Projectile.velocity.X *= 0.99f; // Air resistance
            
            // Gentle swaying motion
            Projectile.velocity.X += (float)Math.Sin(Projectile.timeLeft * 0.15f) * 0.08f;
            
            // Spin slowly
            Projectile.rotation += Projectile.velocity.X * 0.05f;

            // Trail particles
            if (Main.rand.NextBool(3))
            {
                Vector2 trailPos = Projectile.Center + Main.rand.NextVector2Circular(4f, 4f);
                Color trailColor = Color.Lerp(SpringPink, SpringWhite, Main.rand.NextFloat()) * 0.6f;
                Dust dust = Dust.NewDustPerfect(trailPos, DustID.PinkFairy, -Projectile.velocity * 0.2f, 100, trailColor, 0.7f);
                dust.noGravity = true;
                dust.fadeIn = 0.8f;
            }

            // Pulsing light
            float pulse = (float)Math.Sin(Projectile.timeLeft * 0.1f) * 0.15f + 0.35f;
            Lighting.AddLight(Projectile.Center, SpringPink.ToVector3() * pulse);

            // Fade out near end of life
            if (Projectile.timeLeft < 30)
            {
                Projectile.alpha = (int)(255 * (1f - Projectile.timeLeft / 30f));
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = texture.Size() / 2f;

            float pulse = (float)Math.Sin(Projectile.timeLeft * 0.12f) * 0.15f + 1f;
            float alpha = 1f - Projectile.alpha / 255f;
            Color drawColor = Color.Lerp(SpringPink, SpringWhite, Main.rand.NextFloat(0.3f)) * alpha;

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            // Outer glow
            Main.spriteBatch.Draw(texture, drawPos, null, drawColor * 0.5f, Projectile.rotation, origin, 0.5f * pulse, SpriteEffects.None, 0f);
            // Core
            Main.spriteBatch.Draw(texture, drawPos, null, drawColor * 0.8f, Projectile.rotation, origin, 0.3f * pulse, SpriteEffects.None, 0f);
            // White center
            Main.spriteBatch.Draw(texture, drawPos, null, SpringWhite * 0.4f * alpha, Projectile.rotation, origin, 0.15f, SpriteEffects.None, 0f);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            return false;
        }

        public override void OnKill(int timeLeft)
        {
            // Petal dissipation
            for (int i = 0; i < 6; i++)
            {
                Vector2 dustVel = Main.rand.NextVector2Circular(2f, 2f);
                Color dustColor = Color.Lerp(SpringPink, SpringWhite, Main.rand.NextFloat());
                Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.PinkFairy, dustVel, 100, dustColor, 0.8f);
                dust.noGravity = true;
                dust.fadeIn = 1f;
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Mini bloom burst on hit
            for (int i = 0; i < 4; i++)
            {
                Vector2 burstVel = Main.rand.NextVector2Circular(3f, 3f);
                Dust dust = Dust.NewDustPerfect(target.Center, DustID.PinkFairy, burstVel, 80, SpringPink, 1f);
                dust.noGravity = true;
            }
        }
    }
}
