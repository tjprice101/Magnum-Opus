using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.VFX;

namespace MagnumOpus.Content.Eroica.Projectiles
{
    /// <summary>
    /// Beam projectile fired by Flames of Valor.
    /// UNIQUE heroic flame beam with sakura petals, golden sparks, and fiery trail.
    /// </summary>
    public class FlameOfValorBeam : ModProjectile
    {
        // Custom invisible texture - we draw everything with particles
        public override string Texture => "MagnumOpus/Assets/Particles/MagicSparklField10";
        
        // === EROICA COLORS ===
        private static readonly Color EroicaScarlet = new Color(180, 50, 50);
        private static readonly Color EroicaCrimson = new Color(220, 80, 60);
        private static readonly Color EroicaGold = new Color(255, 215, 100);
        private static readonly Color EroicaFlame = new Color(255, 140, 60);
        private static readonly Color EroicaSakura = new Color(255, 180, 200);
        
        private float orbitAngle = 0f;
        private float pulseTimer = 0f;
        
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 18;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 180;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.alpha = 50;
            Projectile.light = 0.8f;
            Projectile.extraUpdates = 1;
            Projectile.scale = 0.65f;
        }

        public override void AI()
        {
            orbitAngle += 0.15f;
            pulseTimer += 0.12f;
            
            Projectile.rotation = Projectile.velocity.ToRotation();
            
            // === UNIQUE EFFECT: Orbiting flame sparks ===
            if (Projectile.timeLeft % 5 == 0)
            {
                for (int i = 0; i < 2; i++)
                {
                    float sparkAngle = orbitAngle + MathHelper.Pi * i;
                    Vector2 sparkPos = Projectile.Center + sparkAngle.ToRotationVector2() * 12f;
                    Color sparkColor = i == 0 ? EroicaGold : EroicaCrimson;
                    EnhancedParticles.BloomFlare(sparkPos, sparkColor, 0.2f, 8, 2, 0.55f);
                }
            }
            
            // === UNIQUE EFFECT: Heroic flame trail ===
            if (Main.rand.NextBool(2))
            {
                Vector2 trailOffset = Main.rand.NextVector2Circular(6f, 6f);
                Color trailColor = Color.Lerp(EroicaScarlet, EroicaGold, Main.rand.NextFloat());
                var trail = new GenericGlowParticle(Projectile.Center + trailOffset, -Projectile.velocity * 0.12f + Main.rand.NextVector2Circular(1f, 1f),
                    trailColor * 0.7f, 0.25f, 16, true);
                MagnumParticleHandler.SpawnParticle(trail);
            }
            
            // === UNIQUE EFFECT: Fiery spark particles ===
            if (Main.rand.NextBool(3))
            {
                var spark = new GlowSparkParticle(Projectile.Center + Main.rand.NextVector2Circular(5f, 5f),
                    -Projectile.velocity * 0.15f + Main.rand.NextVector2Circular(2f, 2f),
                    false, 18, 0.22f, EroicaFlame, new Vector2(0.03f, 1.5f));
                MagnumParticleHandler.SpawnParticle(spark);
            }
            
            // === UNIQUE EFFECT: Golden/crimson dust ===
            if (Main.rand.NextBool(3))
            {
                int dustType = Main.rand.NextBool() ? DustID.GoldFlame : DustID.CrimsonTorch;
                Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(6f, 6f), 
                    dustType, -Projectile.velocity * 0.18f + Main.rand.NextVector2Circular(2f, 2f), 0, default, 1.3f);
                dust.noGravity = true;
            }
            
            // === UNIQUE EFFECT: Occasional sakura petal ===
            if (Main.rand.NextBool(12))
            {
                ThemedParticles.SakuraPetals(Projectile.Center, 1, 8f);
            }
            
            // ☁EMUSICAL NOTATION - Heroic melody trail
            if (Main.rand.NextBool(6))
            {
                Color noteColor = Color.Lerp(EroicaScarlet, EroicaGold, Main.rand.NextFloat());
                Vector2 noteVel = new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), -1f);
                ThemedParticles.MusicNote(Projectile.Center, noteVel, noteColor, 0.35f, 35);
            }
            
            // Pulsing light
            float pulse = 0.7f + (float)Math.Sin(pulseTimer) * 0.2f;
            Lighting.AddLight(Projectile.Center, EroicaGold.ToVector3() * pulse * 0.8f + EroicaScarlet.ToVector3() * pulse * 0.4f);
        }

        public override void OnKill(int timeLeft)
        {
            // Fierce crimson spark for flame beam
            DynamicParticleEffects.EroicaDeathCrimsonSpark(Projectile.Center, 0.9f);
            SoundEngine.PlaySound(SoundID.Item10 with { Volume = 0.5f }, Projectile.Center);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D flareTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/MagicSparklField10").Value;
            Texture2D glowTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/MagicSparklField10").Value;
            Vector2 flareOrigin = flareTex.Size() / 2f;
            Vector2 glowOrigin = glowTex.Size() / 2f;
            
            float pulse = 1f + (float)Math.Sin(pulseTimer) * 0.15f;
            
            // === UNIQUE: Flame trail with gradient ===
            for (int k = 0; k < Projectile.oldPos.Length; k++)
            {
                float progress = (float)k / Projectile.oldPos.Length;
                Vector2 drawPos = Projectile.oldPos[k] + Projectile.Size / 2f - Main.screenPosition;
                
                // Gradient from gold to crimson along trail
                Color trailColor = Color.Lerp(EroicaGold, EroicaScarlet, progress) * (1f - progress) * 0.6f;
                trailColor.A = 0;
                float scale = (0.4f + 0.3f * (1f - progress)) * pulse;
                
                // Alternate flare/glow for texture variety
                if (k % 2 == 0)
                {
                    spriteBatch.Draw(flareTex, drawPos, null, trailColor, Projectile.oldRot[k], flareOrigin, 
                        new Vector2(scale * 1.5f, scale * 0.6f), SpriteEffects.None, 0f);
                }
                else
                {
                    spriteBatch.Draw(glowTex, drawPos, null, trailColor * 0.8f, Projectile.oldRot[k], glowOrigin, 
                        scale * 0.8f, SpriteEffects.None, 0f);
                }
            }
            
            // === UNIQUE: Layered flame head ===
            Color outerFlame = EroicaScarlet with { A = 0 };
            Color midFlame = EroicaFlame with { A = 0 };
            Color innerFlame = EroicaGold with { A = 0 };
            Color hotCore = Color.White with { A = 0 };
            
            // Outer scarlet glow
            spriteBatch.Draw(glowTex, Projectile.Center - Main.screenPosition, null, outerFlame * 0.35f, Projectile.rotation, glowOrigin, 1.5f * pulse, SpriteEffects.None, 0f);
            // Middle flame layer
            spriteBatch.Draw(flareTex, Projectile.Center - Main.screenPosition, null, midFlame * 0.5f, Projectile.rotation, flareOrigin, new Vector2(1.3f, 0.6f) * pulse, SpriteEffects.None, 0f);
            // Inner gold core
            spriteBatch.Draw(flareTex, Projectile.Center - Main.screenPosition, null, innerFlame * 0.65f, Projectile.rotation, flareOrigin, new Vector2(0.9f, 0.4f) * pulse, SpriteEffects.None, 0f);
            // White-hot tip
            spriteBatch.Draw(flareTex, Projectile.Center - Main.screenPosition, null, hotCore * 0.8f, Projectile.rotation, flareOrigin, new Vector2(0.5f, 0.25f) * pulse, SpriteEffects.None, 0f);
            
            // Draw orbiting flame sparks
            for (int i = 0; i < 2; i++)
            {
                float sparkAngle = orbitAngle + MathHelper.Pi * i;
                Vector2 sparkPos = Projectile.Center + sparkAngle.ToRotationVector2() * 10f - Main.screenPosition;
                Color sparkColor = (i == 0 ? EroicaGold : EroicaCrimson) with { A = 0 };
                spriteBatch.Draw(flareTex, sparkPos, null, sparkColor * 0.6f, sparkAngle, flareOrigin, 0.2f * pulse, SpriteEffects.None, 0f);
            }

            return false;
        }

        public override Color? GetAlpha(Color lightColor)
        {
            return Color.White;
        }
    }
}
