using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using Terraria.GameContent;
using MagnumOpus.Content.MoonlightSonata.Debuffs;

namespace MagnumOpus.Content.Eroica.Accessories
{
    /// <summary>
    /// Temporary Heroic Spirit summoned by Sakura's Burning Will every 12 seconds.
    /// Fights for 5 seconds with high damage, uses no minion slots.
    /// Visual: Ghostly sakura warrior with scarlet/pink flame effects and afterimages.
    /// </summary>
    public class HeroicSpiritMinion : ModProjectile
    {
        // Use a vanilla texture as base for afterimages
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.DD2SquireSonicBoom;
        
        private const int LifeDuration = 300; // 5 seconds
        private int targetNPC = -1;
        private int attackCooldown = 0;
        private const int AttackRate = 20; // Attack every ~0.33 seconds
        
        // Animation
        private float glowPulse = 0f;
        
        // Afterimage trail system
        private const int AfterimageCount = 10;
        private Vector2[] afterimagePositions = new Vector2[AfterimageCount];
        private float[] afterimageRotations = new float[AfterimageCount];
        private int afterimageIndex = 0;
        private int afterimageTimer = 0;
        
        public override void SetDefaults()
        {
            Projectile.width = 40;
            Projectile.height = 60;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.penetrate = -1;
            Projectile.timeLeft = LifeDuration;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.alpha = 100;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 15;
        }

        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];
            
            // Get initial target from AI parameter
            if (Projectile.ai[0] >= 0 && targetNPC < 0)
            {
                targetNPC = (int)Projectile.ai[0];
            }
            
            // Update animation pulse
            glowPulse += 0.1f;
            
            // Update afterimage trail
            afterimageTimer++;
            if (afterimageTimer >= 2) // Store position every 2 frames
            {
                afterimageTimer = 0;
                afterimagePositions[afterimageIndex] = Projectile.Center;
                afterimageRotations[afterimageIndex] = Projectile.rotation;
                afterimageIndex = (afterimageIndex + 1) % AfterimageCount;
            }
            
            // Find or update target
            UpdateTarget(owner);
            
            // Movement and attack
            if (targetNPC >= 0 && Main.npc[targetNPC].active)
            {
                AttackTarget(Main.npc[targetNPC]);
            }
            else
            {
                // Idle near player
                IdleMovement(owner);
            }
            
            // Visual effects - ghostly flame aura
            CreateGhostlyEffects();
            
            // Lighting
            float pulse = (float)Math.Sin(glowPulse) * 0.2f + 0.8f;
            Lighting.AddLight(Projectile.Center, 0.8f * pulse, 0.3f * pulse, 0.4f * pulse);
            
            // Facing direction
            if (Math.Abs(Projectile.velocity.X) > 0.5f)
            {
                Projectile.spriteDirection = Projectile.velocity.X > 0 ? 1 : -1;
            }
            
            // Fade out near end of life
            if (Projectile.timeLeft < 60)
            {
                Projectile.alpha = 255 - (int)(Projectile.timeLeft / 60f * 155f);
                
                // Dissipation particles
                if (Main.rand.NextBool(2))
                {
                    Dust fade = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(20f, 30f),
                        DustID.CrimsonTorch, new Vector2(0, -2f), 150, default, 1.5f);
                    fade.noGravity = true;
                }
            }
        }
        
        private void UpdateTarget(Player owner)
        {
            // Check if current target is still valid
            if (targetNPC >= 0)
            {
                NPC target = Main.npc[targetNPC];
                if (!target.active || !target.CanBeChasedBy())
                {
                    targetNPC = -1;
                }
            }
            
            // Find new target if needed
            if (targetNPC < 0)
            {
                float maxDist = 600f;
                
                // Prioritize player's target
                if (owner.HasMinionAttackTargetNPC)
                {
                    NPC playerTarget = Main.npc[owner.MinionAttackTargetNPC];
                    if (playerTarget.CanBeChasedBy() && Vector2.Distance(Projectile.Center, playerTarget.Center) < maxDist)
                    {
                        targetNPC = owner.MinionAttackTargetNPC;
                        return;
                    }
                }
                
                // Find closest enemy
                foreach (NPC npc in Main.ActiveNPCs)
                {
                    if (npc.CanBeChasedBy())
                    {
                        float dist = Vector2.Distance(Projectile.Center, npc.Center);
                        if (dist < maxDist)
                        {
                            maxDist = dist;
                            targetNPC = npc.whoAmI;
                        }
                    }
                }
            }
        }
        
        private void AttackTarget(NPC target)
        {
            // Move toward target
            Vector2 toTarget = target.Center - Projectile.Center;
            float distance = toTarget.Length();
            
            if (distance > 80f)
            {
                // Approach target
                toTarget.Normalize();
                float speed = 12f;
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, toTarget * speed, 0.1f);
            }
            else
            {
                // Circle around target while attacking
                float orbitAngle = Main.GameUpdateCount * 0.08f;
                Vector2 orbitPos = target.Center + new Vector2((float)Math.Cos(orbitAngle), (float)Math.Sin(orbitAngle)) * 60f;
                Vector2 toOrbit = orbitPos - Projectile.Center;
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, toOrbit * 0.1f, 0.2f);
            }
            
            // Attack
            attackCooldown--;
            if (attackCooldown <= 0 && distance < 150f)
            {
                PerformAttack(target);
                attackCooldown = AttackRate;
            }
        }
        
        private void PerformAttack(NPC target)
        {
            // Slash attack - spawn a projectile or deal direct damage
            if (Main.myPlayer == Projectile.owner)
            {
                // Create a slash effect toward the enemy
                Vector2 toTarget = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                
                Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    Projectile.Center,
                    toTarget * 15f,
                    ModContent.ProjectileType<HeroicSpiritSlash>(),
                    Projectile.damage,
                    Projectile.knockBack,
                    Projectile.owner
                );
            }
            
            // Attack visuals
            for (int i = 0; i < 10; i++)
            {
                Vector2 toTarget = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                Dust slash = Dust.NewDustPerfect(Projectile.Center + toTarget * 20f, DustID.CrimsonTorch,
                    toTarget * 8f + Main.rand.NextVector2Circular(2f, 2f), 0, default, 1.5f);
                slash.noGravity = true;
            }
            
            // Sound
            SoundEngine.PlaySound(SoundID.Item71 with { Volume = 0.5f, Pitch = 0.5f }, Projectile.Center);
        }
        
        private void IdleMovement(Player owner)
        {
            // Float near player
            Vector2 targetPos = owner.Center + new Vector2(-50f * owner.direction, -40f);
            Vector2 toTarget = targetPos - Projectile.Center;
            
            Projectile.velocity = Vector2.Lerp(Projectile.velocity, toTarget * 0.05f, 0.1f);
            Projectile.velocity *= 0.95f;
        }
        
        private void CreateGhostlyEffects()
        {
            // Ghostly flame body
            if (Main.rand.NextBool(2))
            {
                // Scarlet core flames
                Dust flame = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(15f, 25f),
                    DustID.CrimsonTorch, new Vector2(0, -2f), 100, default, 1.5f);
                flame.noGravity = true;
            }
            
            // Pink sakura accents
            if (Main.rand.NextBool(3))
            {
                Dust pink = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(20f, 30f),
                    DustID.PinkTorch, new Vector2(Main.rand.NextFloat(-1f, 1f), -1.5f), 80, default, 1.2f);
                pink.noGravity = true;
            }
            
            // Black smoke trail
            if (Main.rand.NextBool(3))
            {
                Dust smoke = Dust.NewDustPerfect(Projectile.Center + new Vector2(0, 20f) + Main.rand.NextVector2Circular(10f, 5f),
                    DustID.Smoke, new Vector2(0, 1f), 150, Color.Black, 1f);
                smoke.noGravity = true;
            }
            
            // Golden sparkles
            if (Main.rand.NextBool(5))
            {
                Dust gold = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(18f, 28f),
                    DustID.GoldCoin, Main.rand.NextVector2Circular(1f, 1f), 0, default, 0.8f);
                gold.noGravity = true;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            
            // End current batch for additive blending
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            // Draw afterimages
            DrawAfterimages(spriteBatch);
            
            // Draw main ghostly form with glow
            DrawGhostlyForm(spriteBatch);
            
            // Restore normal blending
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            return false;
        }
        
        private void DrawAfterimages(SpriteBatch spriteBatch)
        {
            Texture2D glowTex = TextureAssets.Extra[98].Value; // Soft glow texture
            
            // Draw each afterimage with decreasing opacity
            for (int i = 0; i < AfterimageCount; i++)
            {
                int index = (afterimageIndex - i - 1 + AfterimageCount) % AfterimageCount;
                Vector2 pos = afterimagePositions[index];
                
                if (pos == Vector2.Zero) continue; // Skip uninitialized positions
                
                float alpha = (1f - (float)i / AfterimageCount) * 0.4f;
                float scale = 0.8f - i * 0.05f;
                
                // Scarlet ghostly afterimage
                Color afterimageColor = new Color(255, 80, 100) * alpha;
                spriteBatch.Draw(glowTex, pos - Main.screenPosition, null,
                    afterimageColor, afterimageRotations[index], glowTex.Size() / 2f, scale, SpriteEffects.None, 0f);
                
                // Pink accent (every other afterimage)
                if (i % 2 == 0)
                {
                    Color pinkColor = new Color(255, 150, 180) * alpha * 0.5f;
                    spriteBatch.Draw(glowTex, pos - Main.screenPosition, null,
                        pinkColor, afterimageRotations[index], glowTex.Size() / 2f, scale * 0.7f, SpriteEffects.None, 0f);
                }
            }
        }
        
        private void DrawGhostlyForm(SpriteBatch spriteBatch)
        {
            Texture2D glowTex = TextureAssets.Extra[98].Value;
            
            float pulse = (float)Math.Sin(glowPulse) * 0.15f + 1f;
            float fadeAlpha = Projectile.timeLeft < 60 ? Projectile.timeLeft / 60f : 1f;
            
            // Outer scarlet aura
            float outerScale = 1.2f * pulse;
            spriteBatch.Draw(glowTex, Projectile.Center - Main.screenPosition, null,
                new Color(255, 50, 70) * 0.6f * fadeAlpha, 0f, glowTex.Size() / 2f, outerScale, SpriteEffects.None, 0f);
            
            // Middle pink glow
            float midScale = 0.9f * pulse;
            spriteBatch.Draw(glowTex, Projectile.Center - Main.screenPosition, null,
                new Color(255, 120, 150) * 0.7f * fadeAlpha, 0f, glowTex.Size() / 2f, midScale, SpriteEffects.None, 0f);
            
            // Inner golden core
            float innerScale = 0.5f * pulse;
            spriteBatch.Draw(glowTex, Projectile.Center - Main.screenPosition, null,
                new Color(255, 200, 100) * 0.8f * fadeAlpha, 0f, glowTex.Size() / 2f, innerScale, SpriteEffects.None, 0f);
            
            // Bright white center
            float coreScale = 0.25f * pulse;
            spriteBatch.Draw(glowTex, Projectile.Center - Main.screenPosition, null,
                Color.White * 0.5f * fadeAlpha, 0f, glowTex.Size() / 2f, coreScale, SpriteEffects.None, 0f);
        }
        
        public override void OnKill(int timeLeft)
        {
            // Death effect - spirit dissipates
            SoundEngine.PlaySound(SoundID.NPCDeath6 with { Volume = 0.4f, Pitch = 0.5f }, Projectile.Center);
            
            for (int i = 0; i < 30; i++)
            {
                int dustType = Main.rand.NextBool() ? DustID.CrimsonTorch : DustID.PinkTorch;
                Dust death = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(20f, 30f),
                    dustType, Main.rand.NextVector2Circular(5f, 5f), 100, default, 1.5f);
                death.noGravity = true;
            }
        }
    }
    
    /// <summary>
    /// Slash attack projectile from the Heroic Spirit with glowing trail.
    /// </summary>
    public class HeroicSpiritSlash : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.DD2SquireSonicBoom;
        
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 15;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }
        
        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 30;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.penetrate = 3;
            Projectile.timeLeft = 30;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.alpha = 255;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
            
            // Trail particles
            if (Main.rand.NextBool(2))
            {
                Dust trail = Dust.NewDustPerfect(Projectile.Center, DustID.CrimsonTorch,
                    -Projectile.velocity * 0.2f + Main.rand.NextVector2Circular(2f, 2f), 0, default, 1.5f);
                trail.noGravity = true;
            }
            
            // Pink accents
            if (Main.rand.NextBool(3))
            {
                Dust pink = Dust.NewDustPerfect(Projectile.Center, DustID.PinkTorch,
                    Main.rand.NextVector2Circular(2f, 2f), 0, default, 1.2f);
                pink.noGravity = true;
            }
            
            Lighting.AddLight(Projectile.Center, 0.6f, 0.2f, 0.3f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<MusicsDissonance>(), 120);
            
            // Hit effect
            for (int i = 0; i < 8; i++)
            {
                Dust hit2 = Dust.NewDustPerfect(target.Center, DustID.CrimsonTorch,
                    Main.rand.NextVector2Circular(4f, 4f), 0, default, 1.3f);
                hit2.noGravity = true;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            
            // End current batch for additive blending
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            Texture2D glowTex = TextureAssets.Extra[98].Value;
            
            // Draw trail using oldPos
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;
                
                Vector2 drawPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                float progress = (float)i / Projectile.oldPos.Length;
                float alpha = (1f - progress) * 0.6f;
                float scale = (1f - progress) * 0.5f + 0.2f;
                
                // Scarlet trail
                Color trailColor = new Color(255, 80, 100) * alpha;
                spriteBatch.Draw(glowTex, drawPos, null, trailColor, Projectile.oldRot[i], glowTex.Size() / 2f, scale, SpriteEffects.None, 0f);
                
                // Pink highlight
                if (i % 2 == 0)
                {
                    Color pinkColor = new Color(255, 180, 200) * alpha * 0.5f;
                    spriteBatch.Draw(glowTex, drawPos, null, pinkColor, Projectile.oldRot[i], glowTex.Size() / 2f, scale * 0.6f, SpriteEffects.None, 0f);
                }
            }
            
            // Draw main projectile glow
            spriteBatch.Draw(glowTex, Projectile.Center - Main.screenPosition, null,
                new Color(255, 100, 120) * 0.8f, Projectile.rotation, glowTex.Size() / 2f, 0.6f, SpriteEffects.None, 0f);
            spriteBatch.Draw(glowTex, Projectile.Center - Main.screenPosition, null,
                new Color(255, 200, 150) * 0.6f, Projectile.rotation, glowTex.Size() / 2f, 0.35f, SpriteEffects.None, 0f);
            spriteBatch.Draw(glowTex, Projectile.Center - Main.screenPosition, null,
                Color.White * 0.4f, Projectile.rotation, glowTex.Size() / 2f, 0.2f, SpriteEffects.None, 0f);
            
            // Restore normal blending
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            return false;
        }
    }
}
