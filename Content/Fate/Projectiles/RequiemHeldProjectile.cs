using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Content.Fate.Debuffs;

namespace MagnumOpus.Content.Fate.Projectiles
{
    /// <summary>
    /// RequiemHeldProjectile - Multi-phase melee combo attack for Requiem of Reality.
    /// Phase 0: Swing Down (held, visual swing animation)
    /// Phase 1: Swing Up (held, visual swing animation) 
    /// Phase 2: Throw upward (projectile leaves player, spins above head)
    /// Phase 3: Spin above head with building energy
    /// Phase 4: Explosion effect and targeting
    /// Phase 5: Seek nearest enemy and slash through
    /// Phase 6: Return to player (boomerang-style)
    /// </summary>
    public class RequiemHeldProjectile : ModProjectile
    {
        // Use the weapon's texture for the held sword
        public override string Texture => "MagnumOpus/Content/Fate/ResonantWeapons/RequiemOfReality";
        
        // Attack phases
        private enum AttackPhase
        {
            SwingDown,      // Phase 0: Held swing down
            SwingUp,        // Phase 1: Held swing up  
            ThrowUp,        // Phase 2: Throw upward
            SpinAbove,      // Phase 3: Spin above player's head
            Explode,        // Phase 4: Explosion + target acquisition
            SeekEnemy,      // Phase 5: Dash to enemy and slash
            Return          // Phase 6: Return to player
        }
        
        private AttackPhase CurrentPhase
        {
            get => (AttackPhase)(int)Projectile.ai[0];
            set => Projectile.ai[0] = (float)value;
        }
        
        private int PhaseTimer
        {
            get => (int)Projectile.ai[1];
            set => Projectile.ai[1] = value;
        }
        
        // Local state
        private float spinAngle = 0f;
        private float spinSpeed = 0f;
        private NPC targetEnemy = null;
        private int targetIndex = -1;
        private Vector2 hoverPosition;
        private bool hasSlashedThrough = false;
        private Vector2 slashDirection;
        private int slashCount = 0;
        private const int MaxSlashes = 2; // Slash through, then back
        
        // Trail rendering
        private Vector2[] trailPositions = new Vector2[16];
        private float[] trailRotations = new float[16];
        private int trailIndex = 0;
        
        // Phase timings
        private const int SwingDownTime = 12;
        private const int SwingUpTime = 12;
        private const int ThrowUpTime = 18;
        private const int SpinTime = 45;
        private const int ExplodeTime = 15;
        private const int SeekTime = 60;
        private const int ReturnTime = 90;
        
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 16;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        
        public override void SetDefaults()
        {
            Projectile.width = 50;
            Projectile.height = 50;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 600;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 15;
            Projectile.ownerHitCheck = false;
        }
        
        public override void OnSpawn(IEntitySource source)
        {
            // Initialize trail
            for (int i = 0; i < trailPositions.Length; i++)
            {
                trailPositions[i] = Projectile.Center;
                trailRotations[i] = 0f;
            }
            
            spinSpeed = 0.1f;
            
            // Initial spawn VFX
            FateCosmicVFX.SpawnCosmicCloudBurst(Projectile.Center, 0.4f, 8);
            SoundEngine.PlaySound(SoundID.Item1 with { Pitch = -0.2f, Volume = 0.8f }, Projectile.Center);
        }
        
        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];
            
            // Keep projectile alive while owner is valid
            if (!owner.active || owner.dead)
            {
                Projectile.Kill();
                return;
            }
            
            PhaseTimer++;
            
            // Update trail
            if (PhaseTimer % 2 == 0)
            {
                trailIndex = (trailIndex + 1) % trailPositions.Length;
                trailPositions[trailIndex] = Projectile.Center;
                trailRotations[trailIndex] = Projectile.rotation;
            }
            
            switch (CurrentPhase)
            {
                case AttackPhase.SwingDown:
                    AI_SwingDown(owner);
                    break;
                case AttackPhase.SwingUp:
                    AI_SwingUp(owner);
                    break;
                case AttackPhase.ThrowUp:
                    AI_ThrowUp(owner);
                    break;
                case AttackPhase.SpinAbove:
                    AI_SpinAbove(owner);
                    break;
                case AttackPhase.Explode:
                    AI_Explode(owner);
                    break;
                case AttackPhase.SeekEnemy:
                    AI_SeekEnemy(owner);
                    break;
                case AttackPhase.Return:
                    AI_Return(owner);
                    break;
            }
            
            // Cosmic lighting
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.1f) * 0.2f + 0.8f;
            Lighting.AddLight(Projectile.Center, FateCosmicVFX.FateDarkPink.ToVector3() * pulse * 0.7f);
        }
        
        private void AI_SwingDown(Player owner)
        {
            // Lock to owner, animate swing down
            float progress = (float)PhaseTimer / SwingDownTime;
            float swingAngle = MathHelper.Lerp(-MathHelper.PiOver4, MathHelper.PiOver4 + MathHelper.PiOver2, progress);
            
            Projectile.Center = owner.Center + new Vector2(owner.direction * 35f, -10f);
            Projectile.rotation = swingAngle * owner.direction;
            
            // Spawn swing particles
            if (PhaseTimer % 2 == 0)
            {
                Vector2 tipPos = Projectile.Center + Projectile.rotation.ToRotationVector2() * 45f;
                Color sparkColor = FateCosmicVFX.GetCosmicGradient(progress);
                var spark = new GlowSparkParticle(tipPos, Main.rand.NextVector2Circular(2f, 2f), sparkColor, 0.2f, 12);
                MagnumParticleHandler.SpawnParticle(spark);
                
                // Music notes in swing
                if (Main.rand.NextBool(3))
                    FateCosmicVFX.SpawnCosmicMusicNotes(tipPos, 1, 12f, 0.2f);
            }
            
            // Direction locking
            owner.ChangeDir(Main.MouseWorld.X > owner.Center.X ? 1 : -1);
            owner.heldProj = Projectile.whoAmI;
            owner.itemAnimation = 2;
            owner.itemTime = 2;
            
            if (PhaseTimer >= SwingDownTime)
            {
                CurrentPhase = AttackPhase.SwingUp;
                PhaseTimer = 0;
                SoundEngine.PlaySound(SoundID.Item71 with { Pitch = 0.2f, Volume = 0.6f }, Projectile.Center);
            }
        }
        
        private void AI_SwingUp(Player owner)
        {
            // Swing back up, preparing to throw
            float progress = (float)PhaseTimer / SwingUpTime;
            float swingAngle = MathHelper.Lerp(MathHelper.PiOver4 + MathHelper.PiOver2, -MathHelper.PiOver2 - MathHelper.PiOver4, progress);
            
            Projectile.Center = owner.Center + new Vector2(owner.direction * 35f, -10f - progress * 30f);
            Projectile.rotation = swingAngle * owner.direction;
            
            // More intense particles as throw approaches
            if (PhaseTimer % 2 == 0)
            {
                Vector2 tipPos = Projectile.Center + Projectile.rotation.ToRotationVector2() * 45f;
                Color sparkColor = FateCosmicVFX.GetCosmicGradient(progress);
                
                for (int i = 0; i < 2; i++)
                {
                    var spark = new GlowSparkParticle(tipPos + Main.rand.NextVector2Circular(10f, 10f), 
                        Main.rand.NextVector2Circular(3f, 3f), sparkColor, 0.25f, 15);
                    MagnumParticleHandler.SpawnParticle(spark);
                }
                
                // Glyphs gathering
                if (progress > 0.5f && Main.rand.NextBool(2))
                    CustomParticles.Glyph(tipPos + Main.rand.NextVector2Circular(20f, 20f), FateCosmicVFX.FatePurple, 0.3f, -1);
            }
            
            owner.heldProj = Projectile.whoAmI;
            owner.itemAnimation = 2;
            owner.itemTime = 2;
            
            if (PhaseTimer >= SwingUpTime)
            {
                CurrentPhase = AttackPhase.ThrowUp;
                PhaseTimer = 0;
                hoverPosition = owner.Center + new Vector2(0, -120f);
                
                // Throw sound and VFX
                SoundEngine.PlaySound(SoundID.Item1 with { Pitch = 0.4f, Volume = 0.9f }, Projectile.Center);
                FateCosmicVFX.SpawnGlyphBurst(Projectile.Center, 6, 6f, 0.4f);
            }
        }
        
        private void AI_ThrowUp(Player owner)
        {
            // Sword flies upward to hover position
            float progress = (float)PhaseTimer / ThrowUpTime;
            float easedProgress = 1f - (float)Math.Pow(1f - progress, 3); // Ease out
            
            Vector2 startPos = owner.Center + new Vector2(0, -40f);
            Projectile.Center = Vector2.Lerp(startPos, hoverPosition, easedProgress);
            
            // Start spinning
            spinSpeed = MathHelper.Lerp(0.1f, 0.35f, progress);
            spinAngle += spinSpeed;
            Projectile.rotation = spinAngle;
            
            // Rising cosmic trail
            if (PhaseTimer % 2 == 0)
            {
                Color trailColor = FateCosmicVFX.GetCosmicGradient(progress);
                var trail = new GenericGlowParticle(Projectile.Center, new Vector2(Main.rand.NextFloat(-1f, 1f), 2f), 
                    trailColor * 0.7f, 0.3f, 20, true);
                MagnumParticleHandler.SpawnParticle(trail);
                
                // Star sparkles
                if (Main.rand.NextBool(2))
                {
                    var star = new GenericGlowParticle(Projectile.Center + Main.rand.NextVector2Circular(25f, 25f),
                        Main.rand.NextVector2Circular(0.5f, 0.5f), FateCosmicVFX.FateWhite, 0.2f, 15, true);
                    MagnumParticleHandler.SpawnParticle(star);
                }
            }
            
            if (PhaseTimer >= ThrowUpTime)
            {
                CurrentPhase = AttackPhase.SpinAbove;
                PhaseTimer = 0;
                hoverPosition = owner.Center + new Vector2(0, -100f);
            }
        }
        
        private void AI_SpinAbove(Player owner)
        {
            // Hover above player, spinning with building energy
            float progress = (float)PhaseTimer / SpinTime;
            
            // Keep hover position updated relative to player
            hoverPosition = owner.Center + new Vector2(0, -100f);
            Projectile.Center = Vector2.Lerp(Projectile.Center, hoverPosition, 0.15f);
            
            // Accelerating spin
            spinSpeed = MathHelper.Lerp(0.35f, 0.8f, progress);
            spinAngle += spinSpeed;
            Projectile.rotation = spinAngle;
            
            // Cosmic cloud energy building
            if (PhaseTimer % 3 == 0)
            {
                FateCosmicVFX.SpawnCosmicCloudTrail(Projectile.Center, Vector2.Zero, 0.5f + progress * 0.5f);
            }
            
            // Orbiting glyphs
            int glyphCount = 3 + (int)(progress * 4);
            if (PhaseTimer % 4 == 0)
            {
                FateCosmicVFX.SpawnOrbitingGlyphs(Projectile.Center, glyphCount, 35f + progress * 25f, spinAngle, 0.35f);
            }
            
            // Star particles gathering
            if (Main.rand.NextBool(2))
            {
                float gatherAngle = Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 gatherStart = Projectile.Center + gatherAngle.ToRotationVector2() * (80f - progress * 40f);
                Vector2 gatherVel = (Projectile.Center - gatherStart).SafeNormalize(Vector2.Zero) * (2f + progress * 4f);
                
                var star = new GenericGlowParticle(gatherStart, gatherVel, FateCosmicVFX.FateWhite, 0.25f, 20, true);
                MagnumParticleHandler.SpawnParticle(star);
            }
            
            // Music notes circling
            if (PhaseTimer % 8 == 0)
            {
                FateCosmicVFX.SpawnCosmicMusicNotes(Projectile.Center + Main.rand.NextVector2Circular(30f, 30f), 2, 20f, 0.28f);
            }
            
            // Pulsing hum sound
            if (PhaseTimer == SpinTime / 2)
            {
                SoundEngine.PlaySound(SoundID.Item29 with { Pitch = -0.3f, Volume = 0.5f }, Projectile.Center);
            }
            
            if (PhaseTimer >= SpinTime)
            {
                CurrentPhase = AttackPhase.Explode;
                PhaseTimer = 0;
            }
        }
        
        private void AI_Explode(Player owner)
        {
            // Explosion and target acquisition
            if (PhaseTimer == 1)
            {
                // MASSIVE EXPLOSION VFX
                SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.1f, Volume = 1.0f }, Projectile.Center);
                SoundEngine.PlaySound(SoundID.Item105 with { Pitch = -0.3f, Volume = 0.8f }, Projectile.Center);
                
                // Cosmic explosion
                FateCosmicVFX.SpawnCosmicExplosion(Projectile.Center, 1.5f);
                FateCosmicVFX.SpawnCosmicCloudBurst(Projectile.Center, 1.2f, 24);
                FateCosmicVFX.SpawnGlyphBurst(Projectile.Center, 12, 10f, 0.5f);
                FateCosmicVFX.SpawnStarSparkles(Projectile.Center, 20, 60f, 0.35f);
                FateCosmicVFX.SpawnCosmicMusicNotes(Projectile.Center, 8, 50f, 0.4f);
                
                // Halo rings cascade
                for (int i = 0; i < 6; i++)
                {
                    Color ringColor = FateCosmicVFX.GetCosmicGradient((float)i / 6f);
                    CustomParticles.HaloRing(Projectile.Center, ringColor, 0.5f + i * 0.15f, 20 + i * 3);
                }
                
                // Find target
                targetEnemy = FindNearestEnemy(600f);
                if (targetEnemy != null)
                {
                    targetIndex = targetEnemy.whoAmI;
                    slashDirection = (targetEnemy.Center - Projectile.Center).SafeNormalize(Vector2.UnitX);
                }
                
                // Screen shake
                if (owner.whoAmI == Main.myPlayer)
                {
                    MagnumScreenEffects.AddScreenShake(12f);
                }
            }
            
            // Continue spinning during explosion
            spinAngle += 0.6f;
            Projectile.rotation = spinAngle;
            
            // Lingering explosion particles
            if (Main.rand.NextBool(2))
            {
                Color burstColor = FateCosmicVFX.GetCosmicGradient(Main.rand.NextFloat());
                CustomParticles.GenericFlare(Projectile.Center + Main.rand.NextVector2Circular(40f, 40f), burstColor, 0.4f, 15);
            }
            
            if (PhaseTimer >= ExplodeTime)
            {
                if (targetEnemy != null && targetEnemy.active)
                {
                    CurrentPhase = AttackPhase.SeekEnemy;
                    hasSlashedThrough = false;
                    slashCount = 0;
                }
                else
                {
                    CurrentPhase = AttackPhase.Return;
                }
                PhaseTimer = 0;
            }
        }
        
        private void AI_SeekEnemy(Player owner)
        {
            // Validate target
            if (targetIndex >= 0 && targetIndex < Main.maxNPCs)
            {
                targetEnemy = Main.npc[targetIndex];
                if (!targetEnemy.active || targetEnemy.life <= 0)
                {
                    targetEnemy = FindNearestEnemy(800f);
                    if (targetEnemy != null)
                        targetIndex = targetEnemy.whoAmI;
                }
            }
            
            if (targetEnemy == null || !targetEnemy.active)
            {
                CurrentPhase = AttackPhase.Return;
                PhaseTimer = 0;
                return;
            }
            
            // Calculate slash behavior
            if (!hasSlashedThrough)
            {
                // Dash TO enemy
                Vector2 toTarget = targetEnemy.Center - Projectile.Center;
                float dist = toTarget.Length();
                
                if (dist < 40f)
                {
                    // Slashed through!
                    hasSlashedThrough = true;
                    slashCount++;
                    slashDirection = toTarget.SafeNormalize(Vector2.UnitX);
                    
                    // Slash VFX and damage dealt by collision
                    SoundEngine.PlaySound(SoundID.Item71 with { Pitch = 0.3f, Volume = 0.9f }, Projectile.Center);
                    FateCosmicVFX.SpawnCosmicExplosion(targetEnemy.Center, 0.8f);
                    FateCosmicVFX.SpawnCosmicLightningStrike(targetEnemy.Center, 0.7f);
                    
                    // Continue through enemy
                    Projectile.velocity = slashDirection * 35f;
                }
                else
                {
                    // Home toward target
                    toTarget.Normalize();
                    float speed = Math.Min(30f, 15f + PhaseTimer * 0.5f);
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, toTarget * speed, 0.2f);
                }
            }
            else
            {
                // Continue through, then turn around
                if (PhaseTimer > 15 && slashCount < MaxSlashes)
                {
                    // Turn around for another pass
                    hasSlashedThrough = false;
                    PhaseTimer = 0;
                }
                else if (slashCount >= MaxSlashes)
                {
                    // Done slashing, return
                    CurrentPhase = AttackPhase.Return;
                    PhaseTimer = 0;
                    return;
                }
                
                // Slow down after slash through
                Projectile.velocity *= 0.92f;
            }
            
            // Rotation follows velocity
            if (Projectile.velocity.Length() > 1f)
            {
                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;
            }
            else
            {
                spinAngle += 0.4f;
                Projectile.rotation = spinAngle;
            }
            
            // Trail VFX
            if (PhaseTimer % 2 == 0)
            {
                FateCosmicVFX.SpawnCosmicCloudTrail(Projectile.Center, Projectile.velocity, 0.6f);
                
                if (Main.rand.NextBool(2))
                {
                    var star = new GenericGlowParticle(Projectile.Center, -Projectile.velocity * 0.1f + Main.rand.NextVector2Circular(2f, 2f),
                        FateCosmicVFX.FateWhite, 0.25f, 15, true);
                    MagnumParticleHandler.SpawnParticle(star);
                }
            }
            
            if (PhaseTimer >= SeekTime)
            {
                CurrentPhase = AttackPhase.Return;
                PhaseTimer = 0;
            }
        }
        
        private void AI_Return(Player owner)
        {
            // Return to player
            Vector2 toPlayer = owner.Center - Projectile.Center;
            float dist = toPlayer.Length();
            
            if (dist < 40f)
            {
                // Reached player - end projectile
                Projectile.Kill();
                return;
            }
            
            // Accelerate toward player
            toPlayer.Normalize();
            float returnSpeed = MathHelper.Lerp(12f, 28f, Math.Min(PhaseTimer / 30f, 1f));
            Projectile.velocity = Vector2.Lerp(Projectile.velocity, toPlayer * returnSpeed, 0.12f);
            
            // Rotation follows velocity
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;
            
            // Return trail
            if (PhaseTimer % 3 == 0)
            {
                Color trailColor = FateCosmicVFX.GetCosmicGradient((PhaseTimer * 0.02f) % 1f);
                var trail = new GenericGlowParticle(Projectile.Center, -Projectile.velocity * 0.1f, 
                    trailColor * 0.6f, 0.25f, 18, true);
                MagnumParticleHandler.SpawnParticle(trail);
            }
            
            if (PhaseTimer >= ReturnTime)
            {
                Projectile.Kill();
            }
        }
        
        private NPC FindNearestEnemy(float maxDist)
        {
            NPC closest = null;
            float closestDist = maxDist;
            
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly || !npc.CanBeChasedBy())
                    continue;
                
                float dist = Vector2.Distance(Projectile.Center, npc.Center);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closest = npc;
                }
            }
            
            return closest;
        }
        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Apply debuff
            target.AddBuff(ModContent.BuffType<DestinyCollapse>(), 300);
            
            // Hit VFX
            FateCosmicVFX.SpawnCosmicExplosion(target.Center, 0.7f);
            FateCosmicVFX.SpawnCosmicMusicNotes(target.Center, 4, 30f, 0.3f);
            FateCosmicVFX.SpawnGlyphBurst(target.Center, 4, 5f, 0.3f);
            
            // Star sparkles
            for (int i = 0; i < 6; i++)
            {
                var star = new GenericGlowParticle(target.Center + Main.rand.NextVector2Circular(25f, 25f),
                    Main.rand.NextVector2Circular(3f, 3f), FateCosmicVFX.FateWhite, 0.25f, 18, true);
                MagnumParticleHandler.SpawnParticle(star);
            }
        }
        
        public override void OnKill(int timeLeft)
        {
            // Return VFX
            FateCosmicVFX.SpawnGlyphBurst(Projectile.Center, 6, 4f, 0.35f);
            CustomParticles.GenericFlare(Projectile.Center, FateCosmicVFX.FateWhite, 0.6f, 15);
            
            for (int i = 0; i < 8; i++)
            {
                var star = new GenericGlowParticle(Projectile.Center + Main.rand.NextVector2Circular(20f, 20f),
                    Main.rand.NextVector2Circular(2f, 2f), FateCosmicVFX.FateDarkPink, 0.2f, 15, true);
                MagnumParticleHandler.SpawnParticle(star);
            }
            
            SoundEngine.PlaySound(SoundID.Item4 with { Pitch = 0.2f, Volume = 0.5f }, Projectile.Center);
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = texture.Size() / 2f;
            
            // Draw trail
            for (int i = 0; i < trailPositions.Length; i++)
            {
                int trailIdx = (trailIndex - i + trailPositions.Length) % trailPositions.Length;
                Vector2 trailPos = trailPositions[trailIdx] - Main.screenPosition;
                float trailProgress = (float)i / trailPositions.Length;
                float trailAlpha = (1f - trailProgress) * 0.4f;
                float trailScale = (1f - trailProgress * 0.5f) * 0.9f;
                
                Color trailColor = FateCosmicVFX.GetCosmicGradient(trailProgress) * trailAlpha;
                spriteBatch.Draw(texture, trailPos, null, trailColor, trailRotations[trailIdx], origin, trailScale, SpriteEffects.None, 0f);
            }
            
            // Glow layers
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.12f) * 0.15f + 1f;
            
            // Outer cosmic glow
            spriteBatch.Draw(texture, drawPos, null, FateCosmicVFX.FateBrightRed * 0.3f, Projectile.rotation, origin, 1.3f * pulse, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, drawPos, null, FateCosmicVFX.FateDarkPink * 0.4f, Projectile.rotation, origin, 1.15f * pulse, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, drawPos, null, FateCosmicVFX.FateWhite * 0.2f, Projectile.rotation, origin, 1.05f * pulse, SpriteEffects.None, 0f);
            
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            // Main sword
            spriteBatch.Draw(texture, drawPos, null, Color.White, Projectile.rotation, origin, 1f, SpriteEffects.None, 0f);
            
            return false;
        }
    }
}
