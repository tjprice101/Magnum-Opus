using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Content.Eroica.Projectiles
{
    /// <summary>
    /// Pink flaming bolt shot by Movement III.
    /// Fast moving, straight trajectory with enhanced fractal lightning effects.
    /// </summary>
    public class PinkFlamingBolt : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_0";

        private float pulseTimer = 0f;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 15;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 180;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.alpha = 255;
            Projectile.light = 0.6f;
        }

        public override void AI()
        {
            pulseTimer += 0.15f;
            float pulse = 1f + (float)System.Math.Sin(pulseTimer) * 0.2f;
            
            // Enhanced Eroica lighting with sakura pink
            Vector3 sakuraLight = UnifiedVFX.Eroica.Sakura.ToVector3() * pulse * 1.2f;
            Lighting.AddLight(Projectile.Center, sakuraLight);

            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            // === Core flare every frame ===
            CustomParticles.GenericFlare(Projectile.Center, UnifiedVFX.Eroica.Sakura * pulse, 0.4f, 6);
            
            // === Dense gradient glow particle trail ===
            for (int i = 0; i < 3; i++)
            {
                float gradientProgress = Main.rand.NextFloat();
                Color particleColor = Color.Lerp(UnifiedVFX.Eroica.Sakura, UnifiedVFX.Eroica.Crimson, gradientProgress);
                Vector2 randomOffset = Main.rand.NextVector2Circular(6f, 6f);
                Vector2 vel = -Projectile.velocity * 0.15f + Main.rand.NextVector2Circular(1.5f, 1.5f);
                
                var glow = new GenericGlowParticle(Projectile.Center + randomOffset, vel, particleColor, 
                    0.28f + Main.rand.NextFloat(0.1f), 16, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }
            
            // === Orbiting sakura sparkles - 3-point formation ===
            if (pulseTimer % 0.6f < 0.15f)
            {
                for (int i = 0; i < 3; i++)
                {
                    float orbitAngle = MathHelper.TwoPi * i / 3f + pulseTimer * 2f;
                    float orbitRadius = 18f + pulse * 5f;
                    Vector2 orbitPos = Projectile.Center + orbitAngle.ToRotationVector2() * orbitRadius;
                    Color orbitColor = Color.Lerp(UnifiedVFX.Eroica.Sakura, UnifiedVFX.Eroica.Gold, (float)i / 3f);
                    CustomParticles.GenericFlare(orbitPos, orbitColor, 0.25f, 8);
                }
            }
            
            // === Sakura petals in trail ===
            if (Main.rand.NextBool(4))
            {
                ThemedParticles.SakuraPetals(Projectile.Center, 1, 20f);
            }
            
            // === Music notes occasionally ===
            if (Main.rand.NextBool(8))
            {
                float noteProgress = Main.rand.NextFloat();
                Color noteColor = Color.Lerp(UnifiedVFX.Eroica.Sakura, UnifiedVFX.Eroica.Gold, noteProgress);
                Vector2 noteVel = -Projectile.velocity * 0.08f + Main.rand.NextVector2Circular(1f, 1f);
                ThemedParticles.MusicNote(Projectile.Center, noteVel, noteColor, 0.28f, 22);
            }
        }

        public override void OnKill(int timeLeft)
        {
            // Elegant sakura scatter for sakura bolt
            DynamicParticleEffects.EroicaDeathSakuraScatter(Projectile.Center, 0.9f);

            // Sound effect
            Terraria.Audio.SoundEngine.PlaySound(SoundID.Item74, Projectile.position);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            Texture2D glowTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/PrismaticSparkle11").Value;
            Vector2 glowOrigin = glowTex.Size() / 2f;
            
            // Switch to additive blending for glow
            MagnumVFX.BeginAdditiveBlend(spriteBatch);
            
            // === Multi-layer glowing trail with Eroica gradient ===
            for (int i = 0; i < Projectile.oldPos.Length - 1; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero || Projectile.oldPos[i + 1] == Vector2.Zero) continue;
                
                float progress = (float)i / Projectile.oldPos.Length;
                float alpha = 1f - progress;
                float width = MathHelper.Lerp(12f, 3f, progress);
                
                Vector2 start = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                Vector2 end = Projectile.oldPos[i + 1] + Projectile.Size / 2f - Main.screenPosition;
                Vector2 direction = end - start;
                float length = direction.Length();
                float rotation = direction.ToRotation();
                
                // Layer 1: Outer crimson glow
                Color outerColor = UnifiedVFX.Eroica.Crimson * alpha * 0.4f;
                spriteBatch.Draw(pixel, start, new Rectangle(0, 0, 1, 1), outerColor,
                    rotation, new Vector2(0, 0.5f), new Vector2(length, width * 2.5f), SpriteEffects.None, 0f);
                
                // Layer 2: Mid sakura glow
                Color midColor = UnifiedVFX.Eroica.Sakura * alpha * 0.6f;
                spriteBatch.Draw(pixel, start, new Rectangle(0, 0, 1, 1), midColor,
                    rotation, new Vector2(0, 0.5f), new Vector2(length, width * 1.5f), SpriteEffects.None, 0f);
                
                // Layer 3: Core gold/white
                Color coreColor = Color.Lerp(UnifiedVFX.Eroica.Gold, Color.White, 0.5f) * alpha * 0.7f;
                spriteBatch.Draw(pixel, start, new Rectangle(0, 0, 1, 1), coreColor,
                    rotation, new Vector2(0, 0.5f), new Vector2(length, width * 0.5f), SpriteEffects.None, 0f);
                
                // Glow orbs at trail points every 3rd position
                if (i % 3 == 0)
                {
                    Color glowColor = Color.Lerp(UnifiedVFX.Eroica.Sakura, UnifiedVFX.Eroica.Crimson, progress) * alpha * 0.5f;
                    spriteBatch.Draw(glowTex, start, null, glowColor, 0f, glowOrigin, width * 0.15f, SpriteEffects.None, 0f);
                }
            }
            
            // === Enhanced main projectile glow ===
            float pulse = MagnumVFX.GetPulse(0.15f, 0.9f, 1.1f);
            Vector2 mainPos = Projectile.Center - Main.screenPosition;
            
            // Outer crimson bloom
            spriteBatch.Draw(glowTex, mainPos, null, UnifiedVFX.Eroica.Crimson * 0.35f,
                0f, glowOrigin, 2.2f * pulse, SpriteEffects.None, 0f);
            
            // Mid sakura bloom
            spriteBatch.Draw(glowTex, mainPos, null, UnifiedVFX.Eroica.Sakura * 0.5f,
                0f, glowOrigin, 1.4f * pulse, SpriteEffects.None, 0f);
            
            // Inner gold bloom
            spriteBatch.Draw(glowTex, mainPos, null, UnifiedVFX.Eroica.Gold * 0.6f,
                0f, glowOrigin, 0.9f * pulse, SpriteEffects.None, 0f);
            
            // White hot core
            spriteBatch.Draw(glowTex, mainPos, null, Color.White * 0.8f,
                0f, glowOrigin, 0.5f * pulse, SpriteEffects.None, 0f);
            
            MagnumVFX.EndAdditiveBlend(spriteBatch);
            
            return false;
        }
    }
}
