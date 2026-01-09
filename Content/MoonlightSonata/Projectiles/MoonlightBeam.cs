using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent;
using MagnumOpus.Common.Systems;

namespace MagnumOpus.Content.MoonlightSonata.Projectiles
{
    /// <summary>
    /// Moonlight beam projectile - heavy duty, bounces off surfaces, moves fast.
    /// Dark purple center with light purple gradient and sparkles.
    /// Enhanced with additive glow and fractal sparks on bounce.
    /// </summary>
    public class MoonlightBeam : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_0"; // Invisible base

        private int bounceCount = 0;
        private const int MaxBounces = 5;
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
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 8;
            Projectile.timeLeft = 180;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.alpha = 255;
            Projectile.extraUpdates = 3;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 15;
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            bounceCount++;
            
            if (bounceCount >= MaxBounces)
            {
                // Explosive finale with Moonlight-themed fractal sparks
                MagnumVFX.CreateMoonlightSparkBurst(Projectile.Center, 6, 80f);
                MagnumVFX.CreateMusicalBurst(Projectile.Center, new Color(150, 80, 200), Color.White, 2);
                Terraria.Audio.SoundEngine.PlaySound(SoundID.Item10, Projectile.Center);
                return true;
            }

            // Bounce off walls
            if (Projectile.velocity.X != oldVelocity.X)
                Projectile.velocity.X = -oldVelocity.X;
            if (Projectile.velocity.Y != oldVelocity.Y)
                Projectile.velocity.Y = -oldVelocity.Y;

            // Bounce effect with enhanced visuals
            Terraria.Audio.SoundEngine.PlaySound(SoundID.Item10 with { Volume = 0.5f }, Projectile.Center);
            
            // Enhanced bounce impact with ThemedParticles
            ThemedParticles.MoonlightImpact(Projectile.Center, 0.5f);
            
            // Musical notes burst on bounce!
            ThemedParticles.MoonlightMusicNotes(Projectile.Center, 4, 25f);
            
            // Create small Moonlight-themed fractal sparks at bounce point
            for (int i = 0; i < 4; i++)
            {
                float angle = MathHelper.TwoPi * i / 4f + Main.rand.NextFloat(-0.3f, 0.3f);
                Vector2 sparkEnd = Projectile.Center + new Vector2((float)System.Math.Cos(angle), (float)System.Math.Sin(angle)) * Main.rand.NextFloat(30f, 50f);
                MagnumVFX.DrawMoonlightLightning(Projectile.Center, sparkEnd, 4, 15f, 0, 0f);
            }

            // Shockwave ring
            MagnumVFX.CreateShockwaveRing(Projectile.Center, new Color(150, 80, 200), 25f, 2f, 16);

            return false;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
            pulseTimer += 0.2f;
            
            float pulse = 1f + (float)System.Math.Sin(pulseTimer) * 0.3f;
            
            // Enhanced trail using new ThemedParticles system
            ThemedParticles.MoonlightTrail(Projectile.Center, Projectile.velocity);
            
            // Musical note trail - occasional floating notes
            ThemedParticles.MoonlightMusicTrail(Projectile.Center, Projectile.velocity);
            
            // Dark purple core - bigger and pulsing
            Dust core = Dust.NewDustDirect(Projectile.Center - new Vector2(4, 4), 8, 8, DustID.PurpleTorch, 0f, 0f, 50, default, 2.2f * pulse);
            core.noGravity = true;
            core.velocity = Vector2.Zero;
            core.fadeIn = 1.8f;
            
            // Light purple outer glow
            for (int i = 0; i < 2; i++)
            {
                Vector2 offset = Main.rand.NextVector2Circular(10f, 10f);
                Dust glow = Dust.NewDustDirect(Projectile.Center + offset, 1, 1, DustID.PinkTorch, 0f, 0f, 100, default, 1.5f);
                glow.noGravity = true;
                glow.velocity = Projectile.velocity * 0.02f;
            }
            
            // Sparkle effect using ThemedParticles
            if (Main.rand.NextBool(4))
            {
                ThemedParticles.MoonlightSparkles(Projectile.Center, 3, 12f);
            }
            
            // Heavy trail particles
            for (int i = 0; i < 1; i++)
            {
                Vector2 trailPos = Projectile.Center - Projectile.velocity.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(5f, 15f);
                Dust trail = Dust.NewDustDirect(trailPos, 1, 1, DustID.PurpleCrystalShard, 0f, 0f, 150, default, 1.3f);
                trail.noGravity = true;
                trail.velocity *= 0.1f;
            }
            
            // Lighting
            Lighting.AddLight(Projectile.Center, 0.6f, 0.25f, 0.9f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<Debuffs.MusicsDissonance>(), 180);
            MagnumVFX.CreateMusicalBurst(target.Center, new Color(150, 80, 200), Color.White, 1);
            
            // Enhanced hit effect with ThemedParticles
            ThemedParticles.MoonlightSparks(target.Center, target.velocity);
            
            // Musical accidentals on hit
            ThemedParticles.MoonlightAccidentals(target.Center, 2, 15f);
        }

        public override void OnKill(int timeLeft)
        {
            // Enhanced burst using ThemedParticles
            ThemedParticles.MoonlightBloomBurst(Projectile.Center, 0.8f);
            
            // Musical death burst - notes explode outward
            ThemedParticles.MoonlightMusicalImpact(Projectile.Center, 0.6f, false);
            
            // Burst of sparkles with mini fractal sparks
            for (int i = 0; i < 12; i++)
            {
                Vector2 velocity = Main.rand.NextVector2Circular(5f, 5f);
                int dustType = Main.rand.NextBool() ? DustID.PurpleTorch : DustID.PinkFairy;
                Dust dust = Dust.NewDustDirect(Projectile.Center, 1, 1, dustType, velocity.X, velocity.Y, 100, default, 1.6f);
                dust.noGravity = true;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            
            // Switch to additive blending
            MagnumVFX.BeginAdditiveBlend(spriteBatch);
            
            // Draw glowing beam trail
            for (int i = 0; i < Projectile.oldPos.Length - 1; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero || Projectile.oldPos[i + 1] == Vector2.Zero) continue;
                
                float progress = (float)i / Projectile.oldPos.Length;
                Color trailColor = Color.Lerp(new Color(200, 150, 255), new Color(80, 40, 120), progress);
                trailColor *= (1f - progress);
                float width = MathHelper.Lerp(12f, 2f, progress);
                
                Vector2 start = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                Vector2 end = Projectile.oldPos[i + 1] + Projectile.Size / 2f - Main.screenPosition;
                Vector2 direction = end - start;
                float length = direction.Length();
                float rotation = direction.ToRotation();
                
                // Outer glow
                spriteBatch.Draw(pixel, start, new Rectangle(0, 0, 1, 1), trailColor * 0.4f,
                    rotation, new Vector2(0, 0.5f), new Vector2(length, width * 2f), SpriteEffects.None, 0f);
                // Core
                spriteBatch.Draw(pixel, start, new Rectangle(0, 0, 1, 1), trailColor,
                    rotation, new Vector2(0, 0.5f), new Vector2(length, width), SpriteEffects.None, 0f);
                // White center
                spriteBatch.Draw(pixel, start, new Rectangle(0, 0, 1, 1), Color.White * (1f - progress) * 0.8f,
                    rotation, new Vector2(0, 0.5f), new Vector2(length, width * 0.3f), SpriteEffects.None, 0f);
            }
            
            // Draw main projectile glow
            float pulse = MagnumVFX.GetPulse(0.2f, 0.8f, 1.2f);
            Vector2 mainPos = Projectile.Center - Main.screenPosition;
            
            // Outer glow
            spriteBatch.Draw(pixel, mainPos, new Rectangle(0, 0, 1, 1), new Color(150, 80, 200) * 0.5f,
                0f, new Vector2(0.5f, 0.5f), 25f * pulse, SpriteEffects.None, 0f);
            // Core
            spriteBatch.Draw(pixel, mainPos, new Rectangle(0, 0, 1, 1), new Color(200, 150, 255) * 0.7f,
                0f, new Vector2(0.5f, 0.5f), 12f * pulse, SpriteEffects.None, 0f);
            // White center
            spriteBatch.Draw(pixel, mainPos, new Rectangle(0, 0, 1, 1), Color.White * 0.9f,
                0f, new Vector2(0.5f, 0.5f), 5f * pulse, SpriteEffects.None, 0f);
            
            MagnumVFX.EndAdditiveBlend(spriteBatch);
            
            return false;
        }
    }
}
