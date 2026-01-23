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
    /// RequiemSpectralBladeProjectile - Spectral blade combo that triggers automatically on every 4th swing.
    /// This projectile operates independently of the player's swinging (player continues attacking).
    /// Phase 0: Rise upward (spectral blade flies up)
    /// Phase 1: Spin above player with building energy
    /// Phase 2: Explosion effect and targeting
    /// Phase 3: Seek nearest enemy and slash through twice
    /// Phase 4: Return and fade away
    /// </summary>
    public class RequiemSpectralBladeProjectile : ModProjectile
    {
        // Use a ghostly version texture (falls back to weapon texture)
        public override string Texture => "MagnumOpus/Content/Fate/ResonantWeapons/RequiemOfReality";
        
        // Attack phases (simplified - no held phases)
        private enum AttackPhase
        {
            RiseUp,         // Phase 0: Rise above player
            SpinAbove,      // Phase 1: Spin above player's head
            Explode,        // Phase 2: Explosion + target acquisition
            SeekEnemy,      // Phase 3: Dash to enemy and slash
            Return          // Phase 4: Return and fade
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
        private float spectralAlpha = 0.85f; // Ghostly transparency
        
        // Trail rendering
        private Vector2[] trailPositions = new Vector2[16];
        private float[] trailRotations = new float[16];
        private int trailIndex = 0;
        
        // Phase timings
        private const int RiseUpTime = 20;
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
            
            spinSpeed = 0.15f;
            
            // Initial spawn VFX - spectral appearance
            FateCosmicVFX.SpawnCosmicCloudBurst(Projectile.Center, 0.5f, 10);
            SoundEngine.PlaySound(SoundID.Item8 with { Pitch = 0.3f, Volume = 0.7f }, Projectile.Center);
            
            // Set initial hover position above owner
            Player owner = Main.player[Projectile.owner];
            hoverPosition = owner.Center + new Vector2(0, -120f);
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
            
            // Player does NOT hold this projectile - they continue swinging
            // This is an independent spectral blade
            
            switch (CurrentPhase)
            {
                case AttackPhase.RiseUp:
                    AI_RiseUp(owner);
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
            
            // Spectral cosmic lighting
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.1f) * 0.2f + 0.8f;
            Lighting.AddLight(Projectile.Center, FateCosmicVFX.FateDarkPink.ToVector3() * pulse * 0.6f);
        }
        
        private void AI_RiseUp(Player owner)
        {
            // Spectral blade rises to hover position above player
            float progress = (float)PhaseTimer / RiseUpTime;
            float easedProgress = 1f - (float)Math.Pow(1f - progress, 3); // Ease out
            
            Vector2 startPos = owner.Center + new Vector2(0, -20f);
            hoverPosition = owner.Center + new Vector2(0, -120f);
            Projectile.Center = Vector2.Lerp(startPos, hoverPosition, easedProgress);
            
            // Start spinning
            spinSpeed = MathHelper.Lerp(0.15f, 0.35f, progress);
            spinAngle += spinSpeed;
            Projectile.rotation = spinAngle;
            
            // Rising cosmic trail
            if (PhaseTimer % 2 == 0)
            {
                Color trailColor = FateCosmicVFX.GetCosmicGradient(progress);
                var trail = new GenericGlowParticle(Projectile.Center, new Vector2(Main.rand.NextFloat(-1f, 1f), 2f), 
                    trailColor * 0.6f, 0.3f, 20, true);
                MagnumParticleHandler.SpawnParticle(trail);
                
                // Star sparkles
                if (Main.rand.NextBool(2))
                {
                    var star = new GenericGlowParticle(Projectile.Center + Main.rand.NextVector2Circular(25f, 25f),
                        Main.rand.NextVector2Circular(0.5f, 0.5f), FateCosmicVFX.FateWhite * spectralAlpha, 0.2f, 15, true);
                    MagnumParticleHandler.SpawnParticle(star);
                }
                
                // Spectral glyph trail
                if (Main.rand.NextBool(3))
                    CustomParticles.Glyph(Projectile.Center + Main.rand.NextVector2Circular(20f, 20f), FateCosmicVFX.FatePurple * spectralAlpha, 0.28f, -1);
            }
            
            if (PhaseTimer >= RiseUpTime)
            {
                CurrentPhase = AttackPhase.SpinAbove;
                PhaseTimer = 0;
            }
        }
        private void AI_SpinAbove(Player owner)
        {
            // Hover above player (follows loosely), spinning with building energy
            float progress = (float)PhaseTimer / SpinTime;
            
            // Keep hover position updated relative to player (loose follow)
            hoverPosition = owner.Center + new Vector2(0, -100f);
            Projectile.Center = Vector2.Lerp(Projectile.Center, hoverPosition, 0.1f);
            
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
                FateCosmicVFX.SpawnOrbitingGlyphs(Projectile.Center, glyphCount, 35f + progress * 25f, spinAngle, 0.3f * spectralAlpha);
            }
            
            // Star particles gathering
            if (Main.rand.NextBool(2))
            {
                float gatherAngle = Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 gatherStart = Projectile.Center + gatherAngle.ToRotationVector2() * (80f - progress * 40f);
                Vector2 gatherVel = (Projectile.Center - gatherStart).SafeNormalize(Vector2.Zero) * (2f + progress * 4f);
                
                var star = new GenericGlowParticle(gatherStart, gatherVel, FateCosmicVFX.FateWhite * spectralAlpha, 0.25f, 20, true);
                MagnumParticleHandler.SpawnParticle(star);
            }
            
            // Music notes circling
            if (PhaseTimer % 8 == 0)
            {
                FateCosmicVFX.SpawnCosmicMusicNotes(Projectile.Center + Main.rand.NextVector2Circular(30f, 30f), 2, 20f, 0.25f);
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
            
            // Draw trail (ghostly)
            for (int i = 0; i < trailPositions.Length; i++)
            {
                int trailIdx = (trailIndex - i + trailPositions.Length) % trailPositions.Length;
                Vector2 trailPos = trailPositions[trailIdx] - Main.screenPosition;
                float trailProgress = (float)i / trailPositions.Length;
                float trailAlpha = (1f - trailProgress) * 0.3f * spectralAlpha;
                float trailScale = (1f - trailProgress * 0.5f) * 0.9f;
                
                Color trailColor = FateCosmicVFX.GetCosmicGradient(trailProgress) * trailAlpha;
                spriteBatch.Draw(texture, trailPos, null, trailColor, trailRotations[trailIdx], origin, trailScale, SpriteEffects.None, 0f);
            }
            
            // Glow layers (spectral appearance)
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.12f) * 0.15f + 1f;
            
            // Outer cosmic glow (more intense for spectral look)
            spriteBatch.Draw(texture, drawPos, null, FateCosmicVFX.FateBrightRed * 0.35f * spectralAlpha, Projectile.rotation, origin, 1.4f * pulse, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, drawPos, null, FateCosmicVFX.FateDarkPink * 0.45f * spectralAlpha, Projectile.rotation, origin, 1.2f * pulse, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, drawPos, null, FateCosmicVFX.FateWhite * 0.25f * spectralAlpha, Projectile.rotation, origin, 1.08f * pulse, SpriteEffects.None, 0f);
            
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            // Main sword (semi-transparent spectral)
            Color spectralColor = Color.White * spectralAlpha;
            spectralColor.B = (byte)(spectralColor.B * 1.1f); // Slight blue tint for ghostly feel
            spriteBatch.Draw(texture, drawPos, null, spectralColor, Projectile.rotation, origin, 1f, SpriteEffects.None, 0f);
            
            return false;
        }
    }
}
