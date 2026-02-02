using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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
    /// Bullet projectile for Blossom of the Sakura with explosion on impact.
    /// </summary>
    public class BlossomOfTheSakuraBulletProjectile : ModProjectile
    {
        private int targetNPC = -1;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 8;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 12;
            Projectile.height = 12;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 180;
            Projectile.alpha = 0;
            Projectile.light = 0.6f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.extraUpdates = 1;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();

            // Find and home towards nearest enemy (prioritize bosses)
            if (targetNPC < 0 || !Main.npc[targetNPC].active)
            {
                targetNPC = -1;
                float maxDistance = 800f;
                bool foundBoss = false;

                // First pass: look for bosses
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (npc.active && !npc.friendly && npc.boss && !npc.dontTakeDamage)
                    {
                        float distance = Vector2.Distance(Projectile.Center, npc.Center);
                        if (distance < maxDistance)
                        {
                            maxDistance = distance;
                            targetNPC = i;
                            foundBoss = true;
                        }
                    }
                }

                // Second pass: if no boss, target any enemy
                if (!foundBoss)
                {
                    maxDistance = 600f;
                    for (int i = 0; i < Main.maxNPCs; i++)
                    {
                        NPC npc = Main.npc[i];
                        if (npc.active && !npc.friendly && npc.lifeMax > 5 && !npc.dontTakeDamage)
                        {
                            float distance = Vector2.Distance(Projectile.Center, npc.Center);
                            if (distance < maxDistance)
                            {
                                maxDistance = distance;
                                targetNPC = i;
                            }
                        }
                    }
                }
            }

            // Home towards target with moderate tracking
            if (targetNPC >= 0 && Main.npc[targetNPC].active)
            {
                Vector2 direction = Main.npc[targetNPC].Center - Projectile.Center;
                direction.Normalize();
                // Gentle homing so bullets don't make sharp turns
                Projectile.velocity = (Projectile.velocity * 30f + direction * 12f) / 31f;
            }

            // === IRIDESCENT WINGSPAN-STYLE TRAIL VFX ===
            
            // HEAVY DUST TRAILS - scarlet/crimson fire (2+ per frame)
            for (int d = 0; d < 2; d++)
            {
                Dust flame = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(4f, 4f), 
                    DustID.RedTorch, -Projectile.velocity * 0.2f + Main.rand.NextVector2Circular(1f, 1f), 0, default, 1.2f);
                flame.noGravity = true;
                flame.fadeIn = 1.4f;
                
                Dust glow = Dust.NewDustPerfect(Projectile.Center, DustID.GoldCoin, 
                    -Projectile.velocity * 0.15f, 0, Color.White, 0.9f);
                glow.noGravity = true;
                glow.fadeIn = 1.3f;
            }
            
            // CONTRASTING SPARKLES - gold sparkles (1-in-2)
            if (Main.rand.NextBool(2))
            {
                var sparkle = new SparkleParticle(Projectile.Center, -Projectile.velocity * 0.1f + Main.rand.NextVector2Circular(0.5f, 0.5f), 
                    UnifiedVFX.Eroica.Gold, 0.4f, 15);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
            
            // EROICA SHIMMER TRAILS - cycling crimson to gold hues (1-in-3)
            if (Main.rand.NextBool(3))
            {
                // Eroica hues: 0.0-0.08 (red to orange-gold range)
                float hue = Main.rand.NextFloat(0.0f, 0.08f);
                Color shimmerColor = Main.hslToRgb(hue, 1f, 0.6f);
                var shimmer = new GenericGlowParticle(Projectile.Center, -Projectile.velocity * 0.15f + Main.rand.NextVector2Circular(1f, 1f), 
                    shimmerColor, 0.3f, 18, true);
                MagnumParticleHandler.SpawnParticle(shimmer);
            }
            
            // PEARLESCENT SAKURA EFFECTS - color shifting pink/crimson (1-in-4)
            if (Main.rand.NextBool(4))
            {
                float colorShift = (float)System.Math.Sin(Main.GameUpdateCount * 0.2f) * 0.5f + 0.5f;
                Color pearlColor = Color.Lerp(UnifiedVFX.Eroica.Sakura, UnifiedVFX.Eroica.Gold, colorShift) * 0.65f;
                var pearl = new GenericGlowParticle(Projectile.Center, -Projectile.velocity * 0.1f, pearlColor, 0.25f, 16, true);
                MagnumParticleHandler.SpawnParticle(pearl);
            }
            
            // FREQUENT FLARES - scarlet glow (1-in-3)
            if (Main.rand.NextBool(3))
            {
                Color flareColor = Color.Lerp(UnifiedVFX.Eroica.Scarlet, UnifiedVFX.Eroica.Crimson, Main.rand.NextFloat());
                CustomParticles.GenericFlare(Projectile.Center, flareColor, Main.rand.NextFloat(0.2f, 0.35f), 10);
            }
            
            // Custom particle trail
            CustomParticles.EroicaTrail(Projectile.Center, Projectile.velocity, 0.25f);

            // Black smoke wisps (1-in-3)
            if (Main.rand.NextBool(3))
            {
                var smoke = new HeavySmokeParticle(Projectile.Center, -Projectile.velocity * 0.1f + Main.rand.NextVector2Circular(0.5f, 0.5f),
                    Color.Black * 0.5f, Main.rand.Next(15, 25), 0.2f, 0.4f, 0.02f, false);
                MagnumParticleHandler.SpawnParticle(smoke);
            }
            
            // MUSIC NOTES - Eroica melody (1-in-8)
            if (Main.rand.NextBool(8))
            {
                Vector2 noteVel = -Projectile.velocity * 0.05f + new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), Main.rand.NextFloat(-1f, 0f));
                ThemedParticles.MusicNote(Projectile.Center, noteVel, UnifiedVFX.Eroica.Gold, 0.75f, 22);
            }

            Lighting.AddLight(Projectile.Center, 0.6f, 0.2f, 0.15f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // === SEEKING CRYSTALS - Sakura blossom burst ===
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
            
            CreateExplosion();
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            CreateExplosion();
            return true;
        }

        public override void OnKill(int timeLeft)
        {
            CreateExplosion();
        }

        private void CreateExplosion()
        {
            SoundEngine.PlaySound(SoundID.Item14, Projectile.position);

            // === ENHANCED SAKURA EXPLOSION WITH MULTI-LAYER BLOOM ===
            // Central flash with proper bloom stacking
            EnhancedParticles.BloomFlare(Projectile.Center, Color.White, 0.7f, 25, 4, 1.1f);
            EnhancedParticles.BloomFlare(Projectile.Center, ThemedParticles.EroicaSakura, 0.6f, 22, 3, 0.9f);
            
            // Enhanced Eroica themed impact with full bloom
            UnifiedVFXBloom.Eroica.ImpactEnhanced(Projectile.Center, 0.9f);
            
            // Enhanced sakura petal burst with bloom
            EnhancedThemedParticles.SakuraPetalsEnhanced(Projectile.Center, 10, 40f);
            
            // Enhanced music notes with bloom
            EnhancedThemedParticles.EroicaMusicNotesEnhanced(Projectile.Center, 4, 28f);
            
            // Scarlet red explosion
            for (int i = 0; i < 25; i++)
            {
                Dust explosion = Dust.NewDustDirect(Projectile.position - new Vector2(15, 15),
                    Projectile.width + 30, Projectile.height + 30,
                    DustID.RedTorch, 0f, 0f, 100, default, 2.0f);
                explosion.noGravity = true;
                explosion.velocity = Main.rand.NextVector2Circular(6f, 6f);
            }

            // Black smoke explosion
            for (int i = 0; i < 15; i++)
            {
                Dust smoke = Dust.NewDustDirect(Projectile.position - new Vector2(10, 10),
                    Projectile.width + 20, Projectile.height + 20,
                    DustID.Smoke, 0f, 0f, 100, Color.Black, 1.5f);
                smoke.noGravity = true;
                smoke.velocity = Main.rand.NextVector2Circular(4f, 4f);
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
            Vector2 origin = texture.Size() / 2f;

            // Switch to additive blending for prismatic effect
            MagnumVFX.BeginAdditiveBlend(spriteBatch);
            
            // Draw prismatic gem trail
            MagnumVFX.DrawPrismaticGemTrail(spriteBatch, Projectile.oldPos, true, 0.3f, (float)Projectile.timeLeft);
            
            // Draw small prismatic gem at bullet position
            MagnumVFX.DrawEroicaPrismaticGem(spriteBatch, Projectile.Center, 0.35f, 0.8f, (float)Projectile.timeLeft);
            
            MagnumVFX.EndAdditiveBlend(spriteBatch);
            
            // Draw standard trail
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;

                Vector2 drawPos = Projectile.oldPos[i] - Main.screenPosition + Projectile.Size / 2f;
                Color trailColor = new Color(200, 50, 50, 0) * ((float)(Projectile.oldPos.Length - i) / Projectile.oldPos.Length);

                spriteBatch.Draw(texture, drawPos, null, trailColor, Projectile.oldRot[i], origin,
                    Projectile.scale, SpriteEffects.None, 0f);
            }

            return true;
        }
    }
}
