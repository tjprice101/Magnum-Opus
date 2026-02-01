using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Content.Eroica.Projectiles
{
    /// <summary>
    /// Splitting Valor Orb - Large slow projectile that splits into smaller EnergyOfEroica projectiles.
    /// Inspired by Yharon's splitting fireballs from Calamity.
    /// </summary>
    public class SplittingValorOrb : ModProjectile
    {
        // Use invisible texture - projectile is entirely particle-based
        public override string Texture => "MagnumOpus/Assets/Particles/MagicSparklField11";
        
        private const int SplitCount = 8;
        private const int SplitDelay = 90; // Splits after 1.5 seconds
        private bool hasSplit = false;
        
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 12;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 40;
            Projectile.height = 40;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 180;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.alpha = 0;
            Projectile.light = 0.8f;
        }

        public override void AI()
        {
            Projectile.ai[1]++;
            
            // Pulsing scale
            float pulse = 1f + (float)Math.Sin(Projectile.ai[1] * 0.1f) * 0.15f;
            
            // Lighting with pulsing intensity
            Lighting.AddLight(Projectile.Center, 1.2f * pulse, 0.5f * pulse, 0.3f * pulse);
            
            // Slow rotation
            Projectile.rotation += 0.08f;
            
            // === MAIN ORB VISUAL - Large pulsing core ===
            float progress = Projectile.ai[1] / SplitDelay;
            Color coreColor = Color.Lerp(ThemedParticles.EroicaScarlet, ThemedParticles.EroicaGold, progress);
            CustomParticles.GenericFlare(Projectile.Center, coreColor, 0.7f * pulse, 8);
            CustomParticles.GenericFlare(Projectile.Center, Color.White * 0.8f, 0.4f * pulse, 6);
            
            // === ORBITING GLOWS ===
            if (Projectile.ai[1] % 3 == 0)
            {
                for (int i = 0; i < 4; i++)
                {
                    float angle = Projectile.ai[1] * 0.12f + MathHelper.TwoPi * i / 4f;
                    float radius = 25f + (float)Math.Sin(Projectile.ai[1] * 0.08f + i) * 8f;
                    Vector2 orbitPos = Projectile.Center + angle.ToRotationVector2() * radius;
                    Color orbitColor = Color.Lerp(ThemedParticles.EroicaSakura, ThemedParticles.EroicaGold, (float)i / 4f);
                    CustomParticles.GenericFlare(orbitPos, orbitColor * 0.7f, 0.3f, 10);
                }
            }
            
            // === TRAIL PARTICLES ===
            if (Projectile.ai[1] % 2 == 0)
            {
                var trail = new GenericGlowParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(8f, 8f),
                    -Projectile.velocity * 0.15f + Main.rand.NextVector2Circular(2f, 2f),
                    coreColor,
                    0.45f,
                    22,
                    true
                );
                MagnumParticleHandler.SpawnParticle(trail);
                
                // Sakura petals trail
                if (Main.rand.NextBool(4))
                    ThemedParticles.SakuraPetals(Projectile.Center, 1, 15f);
            }
            
            // === HALO RINGS ===
            if (Projectile.ai[1] % 15 == 0)
            {
                CustomParticles.HaloRing(Projectile.Center, coreColor * 0.5f, 0.4f + progress * 0.3f, 20);
            }
            
            // === MUSIC NOTES (it's a music mod!) ===
            if (Projectile.ai[1] % 20 == 0)
            {
                ThemedParticles.EroicaMusicNotes(Projectile.Center, 2, 25f);
            }
            
            // === SPLIT LOGIC ===
            if (Projectile.ai[1] >= SplitDelay && !hasSplit)
            {
                Split();
            }
            
            // Pre-split warning - intensifying glow
            if (Projectile.ai[1] >= SplitDelay - 30 && !hasSplit)
            {
                float warningProgress = (Projectile.ai[1] - (SplitDelay - 30)) / 30f;
                
                // Intensifying flare
                CustomParticles.GenericFlare(Projectile.Center, Color.Lerp(coreColor, Color.White, warningProgress * 0.5f), 
                    0.5f + warningProgress * 0.4f, 5);
                
                // Warning particles gathering
                if (Projectile.ai[1] % 3 == 0)
                {
                    for (int i = 0; i < SplitCount; i++)
                    {
                        float angle = MathHelper.TwoPi * i / SplitCount + Projectile.ai[1] * 0.05f;
                        float dist = (1f - warningProgress) * 60f + 20f;
                        Vector2 particlePos = Projectile.Center + angle.ToRotationVector2() * dist;
                        Dust warning = Dust.NewDustPerfect(particlePos, DustID.GoldFlame, -angle.ToRotationVector2() * warningProgress * 3f, 100, default, 1.5f);
                        warning.noGravity = true;
                    }
                }
            }
        }

        private void Split()
        {
            hasSplit = true;
            
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                // Target player from ai[0]
                int targetPlayer = (int)Projectile.ai[0];
                Player target = Main.player[targetPlayer];
                
                // Split into smaller EnergyOfEroica projectiles
                for (int i = 0; i < SplitCount; i++)
                {
                    float angle = MathHelper.TwoPi * i / SplitCount;
                    Vector2 splitVelocity = angle.ToRotationVector2() * 6f;
                    
                    // Add some velocity toward player for the ones facing that direction
                    Vector2 toPlayer = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                    float dot = Vector2.Dot(splitVelocity.SafeNormalize(Vector2.Zero), toPlayer);
                    if (dot > 0.3f)
                    {
                        splitVelocity += toPlayer * 3f;
                    }
                    
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, splitVelocity,
                        ModContent.ProjectileType<EnergyOfEroica>(), Projectile.damage / 2, 2f, Main.myPlayer, targetPlayer);
                }
            }
            
            // === SPLIT EXPLOSION VFX ===
            SoundEngine.PlaySound(SoundID.Item14 with { Pitch = 0.2f, Volume = 0.9f }, Projectile.Center);
            
            // Themed explosion
            UnifiedVFX.Eroica.Explosion(Projectile.Center, 1.5f);
            
            // Fractal burst pattern
            for (int i = 0; i < SplitCount; i++)
            {
                float angle = MathHelper.TwoPi * i / SplitCount;
                Vector2 burstPos = Projectile.Center + angle.ToRotationVector2() * 35f;
                Color burstColor = Color.Lerp(ThemedParticles.EroicaScarlet, ThemedParticles.EroicaGold, (float)i / SplitCount);
                CustomParticles.GenericFlare(burstPos, burstColor, 0.6f, 20);
            }
            
            // Central white flash
            CustomParticles.GenericFlare(Projectile.Center, Color.White, 1.2f, 15);
            
            // Expanding halos
            for (int ring = 0; ring < 4; ring++)
            {
                float ringProgress = (float)ring / 4f;
                Color ringColor = Color.Lerp(ThemedParticles.EroicaScarlet, ThemedParticles.EroicaGold, ringProgress);
                CustomParticles.HaloRing(Projectile.Center, ringColor * 0.7f, 0.5f + ring * 0.2f, 18 + ring * 4);
            }
            
            // Sakura petals scatter
            ThemedParticles.SakuraPetals(Projectile.Center, 12, 80f);
            
            // Music notes burst
            ThemedParticles.EroicaMusicNotes(Projectile.Center, 8, 60f);
            
            // Kill the parent projectile
            Projectile.Kill();
        }

        public override void OnKill(int timeLeft)
        {
            // If killed without splitting (hit something), still do split
            if (!hasSplit)
            {
                Split();
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            // Draw multiple layered glows as the "projectile" visual
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D glowTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/MagicSparklField11").Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = glowTex.Size() / 2f;
            
            float pulse = 1f + (float)Math.Sin(Projectile.ai[1] * 0.1f) * 0.15f;
            float progress = Math.Min(Projectile.ai[1] / SplitDelay, 1f);
            
            // Outer crimson glow
            Color outerColor = ThemedParticles.EroicaScarlet * 0.4f;
            spriteBatch.Draw(glowTex, drawPos, null, outerColor, Projectile.rotation, origin, 1.8f * pulse, SpriteEffects.None, 0f);
            
            // Middle gold glow
            Color middleColor = Color.Lerp(ThemedParticles.EroicaCrimson, ThemedParticles.EroicaGold, progress) * 0.5f;
            spriteBatch.Draw(glowTex, drawPos, null, middleColor, Projectile.rotation * 0.7f, origin, 1.2f * pulse, SpriteEffects.None, 0f);
            
            // Inner core
            Color innerColor = Color.Lerp(ThemedParticles.EroicaGold, Color.White, 0.4f) * 0.7f;
            spriteBatch.Draw(glowTex, drawPos, null, innerColor, -Projectile.rotation, origin, 0.7f * pulse, SpriteEffects.None, 0f);
            
            // Central white hot core
            spriteBatch.Draw(glowTex, drawPos, null, Color.White * 0.8f, 0f, origin, 0.35f * pulse, SpriteEffects.None, 0f);
            
            return false; // Don't draw default sprite
        }

        public override Color? GetAlpha(Color lightColor)
        {
            return Color.White;
        }
    }
}
