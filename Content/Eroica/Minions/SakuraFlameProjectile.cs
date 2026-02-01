using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using MagnumOpus.Common.Systems;

namespace MagnumOpus.Content.Eroica.Minions
{
    /// <summary>
    /// Sakura Flame Projectile - Vibrant gold and red flame particle.
    /// Fired rapidly by the Sakura of Fate minion as a flamethrower stream.
    /// </summary>
    public class SakuraFlameProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/PrismaticSparkle14";
        
        private float fadeProgress = 0f;
        
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.MinionShot[Type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = 8;
            Projectile.height = 8;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.penetrate = 2;
            Projectile.timeLeft = 45; // Short lived for flamethrower feel
            Projectile.tileCollide = true;
            Projectile.ignoreWater = false;
            Projectile.extraUpdates = 2; // Fast updates for smooth flame
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
            Projectile.alpha = 100;
        }

        public override void AI()
        {
            // Calculate fade based on remaining lifetime
            fadeProgress = 1f - (Projectile.timeLeft / 45f);
            
            // Rotation follows velocity with some wobble
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            Projectile.rotation += (float)Math.Sin(Main.GameUpdateCount * 0.5f + Projectile.whoAmI) * 0.1f;
            
            // Slow down slightly over time (flame dissipates)
            Projectile.velocity *= 0.98f;
            
            // Add slight random drift
            Projectile.velocity += Main.rand.NextVector2Circular(0.15f, 0.15f);
            
            // === VIBRANT GOLD AND RED FLAME PARTICLES ===
            // Gold/yellow core flames
            if (Main.rand.NextBool(2))
            {
                Dust gold = Dust.NewDustPerfect(
                    Projectile.Center + Main.rand.NextVector2Circular(4f, 4f),
                    DustID.Torch, 
                    -Projectile.velocity * 0.2f + Main.rand.NextVector2Circular(1f, 1f),
                    100, 
                    new Color(255, 200, 50), // Bright gold
                    1.5f * (1f - fadeProgress)
                );
                gold.noGravity = true;
                gold.fadeIn = 1.2f;
            }
            
            // Orange/red outer flames
            if (Main.rand.NextBool(2))
            {
                Dust red = Dust.NewDustPerfect(
                    Projectile.Center + Main.rand.NextVector2Circular(6f, 6f),
                    DustID.Torch, 
                    -Projectile.velocity * 0.15f + Main.rand.NextVector2Circular(1.5f, 1.5f),
                    100, 
                    new Color(255, 80, 30), // Deep orange-red
                    1.8f * (1f - fadeProgress * 0.5f)
                );
                red.noGravity = true;
                red.fadeIn = 1.3f;
            }
            
            // Bright yellow sparks
            if (Main.rand.NextBool(4))
            {
                Dust spark = Dust.NewDustPerfect(
                    Projectile.Center,
                    DustID.Firework_Yellow,
                    -Projectile.velocity * 0.1f + Main.rand.NextVector2Circular(2f, 2f),
                    0,
                    default,
                    0.8f * (1f - fadeProgress)
                );
                spark.noGravity = true;
            }
            
            // White-hot center sparks (occasional)
            if (Main.rand.NextBool(6) && fadeProgress < 0.5f)
            {
                Dust white = Dust.NewDustPerfect(
                    Projectile.Center,
                    DustID.SparksMech,
                    Main.rand.NextVector2Circular(1f, 1f),
                    0,
                    Color.White,
                    0.6f
                );
                white.noGravity = true;
            }
            
            // Vibrant warm lighting - gold fading to red
            float lightIntensity = (1f - fadeProgress) * 0.8f;
            float redComponent = 0.9f + fadeProgress * 0.1f;
            float greenComponent = 0.5f - fadeProgress * 0.3f;
            Lighting.AddLight(Projectile.Center, redComponent * lightIntensity, greenComponent * lightIntensity, 0.1f * lightIntensity);
            
            // ☁EMUSICAL NOTATION - Heroic melody trail
            if (Main.rand.NextBool(8))
            {
                Color noteColor = Color.Lerp(new Color(200, 50, 50), new Color(255, 215, 0), Main.rand.NextFloat());
                Vector2 noteVel = new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), -1f);
                ThemedParticles.MusicNote(Projectile.Center, noteVel, noteColor, 0.28f, 30);
            }
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            // Custom additive blend drawing for glow effect
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D pixel = Terraria.GameContent.TextureAssets.MagicPixel.Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            
            float alpha = 1f - fadeProgress;
            float size = 12f * alpha;
            
            // Switch to additive blending
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            // Outer red-orange glow
            Color outerGlow = new Color(255, 60, 20) * alpha * 0.5f;
            spriteBatch.Draw(pixel, drawPos, new Rectangle(0, 0, 1, 1), outerGlow, 0f, new Vector2(0.5f), size * 1.5f, SpriteEffects.None, 0f);
            
            // Mid gold glow
            Color midGlow = new Color(255, 180, 50) * alpha * 0.7f;
            spriteBatch.Draw(pixel, drawPos, new Rectangle(0, 0, 1, 1), midGlow, 0f, new Vector2(0.5f), size, SpriteEffects.None, 0f);
            
            // Inner bright yellow-white core
            Color coreGlow = new Color(255, 240, 150) * alpha * 0.9f;
            spriteBatch.Draw(pixel, drawPos, new Rectangle(0, 0, 1, 1), coreGlow, 0f, new Vector2(0.5f), size * 0.5f, SpriteEffects.None, 0f);
            
            // Restore normal blending
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            return false;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Fiery impact burst - gold and red
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 vel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * Main.rand.NextFloat(2f, 4f);
                
                Color flameColor = i % 2 == 0 ? new Color(255, 200, 50) : new Color(255, 80, 30);
                Dust flame = Dust.NewDustPerfect(target.Center, DustID.Torch, vel, 100, flameColor, 1.4f);
                flame.noGravity = true;
            }
            
            // Brief lighting flash
            Lighting.AddLight(target.Center, 0.8f, 0.4f, 0.1f);
            
            // ☁EMUSICAL IMPACT - Triumphant chord burst
            if (Main.rand.NextBool(3)) // Not every hit to avoid spam
            {
                ThemedParticles.MusicNoteBurst(target.Center, new Color(255, 215, 0), 3, 2.5f);
            }
            
            // === SEEKING CRYSTALS - Sakura flame minion ===
            if (Main.rand.NextBool(4))
            {
                SeekingCrystalHelper.SpawnEroicaCrystals(
                    Projectile.GetSource_FromThis(),
                    target.Center,
                    Projectile.velocity,
                    (int)(damageDone * 0.15f),
                    Projectile.knockBack,
                    Projectile.owner,
                    3
                );
            }
        }
        
        public override void OnKill(int timeLeft)
        {
            // Small flame puff on death
            for (int i = 0; i < 4; i++)
            {
                Color flameColor = Main.rand.NextBool() ? new Color(255, 200, 50) : new Color(255, 80, 30);
                Vector2 vel = Main.rand.NextVector2Circular(2f, 2f);
                Dust flame = Dust.NewDustPerfect(Projectile.Center, DustID.Torch, vel, 100, flameColor, 1f);
                flame.noGravity = true;
            }
        }
    }
}
