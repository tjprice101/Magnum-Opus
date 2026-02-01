using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;

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
        private float petalFlutter;

        public override string Texture => "MagnumOpus/Assets/Particles/PrismaticSparkle8";

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

            // ☁ELAYERED TRAIL - Mix of dust, sparkles, and glows for visual richness
            if (Main.rand.NextBool(3))
            {
                Vector2 trailPos = Projectile.Center + Main.rand.NextVector2Circular(4f, 4f);
                Color trailColor = Color.Lerp(SpringPink, SpringWhite, Main.rand.NextFloat()) * 0.6f;
                
                // Vanilla dust for density
                Dust dust = Dust.NewDustPerfect(trailPos, DustID.PinkFairy, -Projectile.velocity * 0.2f, 100, trailColor, 0.7f);
                dust.noGravity = true;
                dust.fadeIn = 0.8f;
                
                // Magic sparkle field for shimmer (use variant 3 for petal-like sparkle)
                var sparkle = new SparkleParticle(trailPos, -Projectile.velocity * 0.15f, SpringPink * 0.8f, 0.35f, 25);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
            
            // Prismatic sparkle accents (use variant 8 for spring feel)
            if (Main.rand.NextBool(5))
            {
                Color prismColor = Color.Lerp(SpringPink, SpringWhite, Main.rand.NextFloat());
                var prism = new SparkleParticle(Projectile.Center + Main.rand.NextVector2Circular(6f, 6f), 
                    Main.rand.NextVector2Circular(1f, 1f), prismColor * 0.7f, 0.3f, 20);
                MagnumParticleHandler.SpawnParticle(prism);
            }

            // ☁EMUSICAL NOTATION - VISIBLE notes (0.7f+ scale!) drift with the petals!
            if (Main.rand.NextBool(6))
            {
                Vector2 noteVel = new Vector2(Main.rand.NextFloat(-0.3f, 0.3f), Main.rand.NextFloat(-0.8f, -0.3f));
                // Scale 0.7f makes notes VISIBLE! Use different note variants (1-6)
                ThemedParticles.MusicNote(Projectile.Center, noteVel, SpringPink * 0.9f, 0.7f, 45);
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
            // ☁ELAYERED PETAL DISSIPATION - Multiple particle types!
            
            // Central bloom flash with different flare variants
            CustomParticles.GenericFlare(Projectile.Center, SpringWhite, 0.5f, 15);
            CustomParticles.GenericFlare(Projectile.Center, SpringPink, 0.4f, 12);
            
            // Sparkle burst ring
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 sparkPos = Projectile.Center + angle.ToRotationVector2() * 12f;
                var sparkle = new SparkleParticle(sparkPos, angle.ToRotationVector2() * 2f, 
                    Color.Lerp(SpringPink, SpringWhite, i / 8f) * 0.8f, 0.35f, 20);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
            
            // Dust for density
            for (int i = 0; i < 6; i++)
            {
                Vector2 dustVel = Main.rand.NextVector2Circular(2f, 2f);
                Color dustColor = Color.Lerp(SpringPink, SpringWhite, Main.rand.NextFloat());
                Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.PinkFairy, dustVel, 100, dustColor, 0.8f);
                dust.noGravity = true;
                dust.fadeIn = 1f;
            }
            
            // VISIBLE music note farewell (scale 0.75f)
            if (Main.rand.NextBool(2))
            {
                ThemedParticles.MusicNote(Projectile.Center, new Vector2(0, -1f), SpringPink, 0.75f, 35);
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // ☁EMUSICAL IMPACT - VISIBLE notes sing on contact! (scale 0.8f)
            for (int i = 0; i < 5; i++)
            {
                float angle = MathHelper.TwoPi * i / 5f;
                Vector2 noteVel = angle.ToRotationVector2() * 2.5f;
                Color noteColor = Color.Lerp(SpringPink, SpringWhite, i / 5f);
                ThemedParticles.MusicNote(target.Center, noteVel, noteColor, 0.8f, 35);
            }
            
            // Central flash with layered flares
            CustomParticles.GenericFlare(target.Center, SpringWhite, 0.6f, 18);
            CustomParticles.GenericFlare(target.Center, SpringPink, 0.5f, 15);
            
            // Sparkle burst ring for magical feel
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f;
                Vector2 sparkVel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 5f);
                var sparkle = new SparkleParticle(target.Center, sparkVel, 
                    Color.Lerp(SpringPink, SpringWhite, Main.rand.NextFloat()) * 0.9f, 0.4f, 22);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
            
            // Dust for density
            for (int i = 0; i < 4; i++)
            {
                Vector2 burstVel = Main.rand.NextVector2Circular(3f, 3f);
                Dust dust = Dust.NewDustPerfect(target.Center, DustID.PinkFairy, burstVel, 80, SpringPink, 1f);
                dust.noGravity = true;
            }
        }
    }
}
