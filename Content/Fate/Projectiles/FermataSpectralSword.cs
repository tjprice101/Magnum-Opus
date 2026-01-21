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
    /// FermataSpectralSword - Spectral Coda of Annihilation swords spawned by The Final Fermata.
    /// These swords spin around the player, then cast themselves at enemies and slash through twice.
    /// Phase 0: Orbit around player, spinning and building energy
    /// Phase 1: Target acquired, dash to enemy
    /// Phase 2: Slash through enemy
    /// Phase 3: Turn around
    /// Phase 4: Slash back through
    /// Phase 5: Dissipate
    /// </summary>
    public class FermataSpectralSword : ModProjectile
    {
        // Use Coda of Annihilation texture for the spectral sword
        public override string Texture => "MagnumOpus/Content/Fate/ResonantWeapons/CodaOfAnnihilation";
        
        private enum SwordPhase
        {
            Orbiting,       // Phase 0: Spin around player
            Targeting,      // Phase 1: Lock on and charge
            FirstSlash,     // Phase 2: Slash through enemy
            Turnaround,     // Phase 3: Turn around behind enemy
            SecondSlash,    // Phase 4: Slash back through
            Dissipate       // Phase 5: Fade away
        }
        
        private SwordPhase CurrentPhase
        {
            get => (SwordPhase)(int)Projectile.ai[0];
            set => Projectile.ai[0] = (float)value;
        }
        
        // Orbit index (0, 1, or 2 for the three swords)
        public int OrbitIndex
        {
            get => (int)Projectile.ai[1];
            set => Projectile.ai[1] = value;
        }
        
        // Local state
        private int phaseTimer = 0;
        private float orbitAngle = 0f;
        private float orbitRadius = 80f;
        private float spinSpeed = 0.3f;
        private NPC targetEnemy = null;
        private int targetIndex = -1;
        private Vector2 slashStartPos;
        private Vector2 slashEndPos;
        private float spectralAlpha = 0f;
        private bool hasDealtFirstSlash = false;
        private bool hasDealtSecondSlash = false;
        
        // Trail rendering
        private Vector2[] trailPositions = new Vector2[12];
        private float[] trailRotations = new float[12];
        private int trailIdx = 0;
        
        // Timings
        private const int OrbitTime = 60; // Frames to orbit before attacking
        private const int TargetTime = 20; // Frames to charge at target
        private const int SlashTime = 8; // Frames for slash animation
        private const int TurnaroundTime = 15; // Frames to turn around
        private const int DissipateTime = 20; // Frames to fade
        
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 12;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        
        public override void SetDefaults()
        {
            Projectile.width = 50;
            Projectile.height = 50;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 600;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 20;
            Projectile.alpha = 50;
        }
        
        public override void OnSpawn(IEntitySource source)
        {
            // Initialize trail
            for (int i = 0; i < trailPositions.Length; i++)
            {
                trailPositions[i] = Projectile.Center;
                trailRotations[i] = 0f;
            }
            
            // Set initial orbit angle based on index
            orbitAngle = MathHelper.TwoPi * OrbitIndex / 3f;
            spectralAlpha = 0f;
            
            // Spawn VFX
            CustomParticles.GenericFlare(Projectile.Center, FateCosmicVFX.FateWhite, 0.5f, 12);
            FateCosmicVFX.SpawnGlyphBurst(Projectile.Center, 4, 4f, 0.3f);
        }
        
        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];
            
            if (!owner.active || owner.dead)
            {
                Projectile.Kill();
                return;
            }
            
            phaseTimer++;
            
            // Update trail
            if (phaseTimer % 2 == 0)
            {
                trailIdx = (trailIdx + 1) % trailPositions.Length;
                trailPositions[trailIdx] = Projectile.Center;
                trailRotations[trailIdx] = Projectile.rotation;
            }
            
            // Fade in
            if (spectralAlpha < 1f)
                spectralAlpha = Math.Min(1f, spectralAlpha + 0.05f);
            
            switch (CurrentPhase)
            {
                case SwordPhase.Orbiting:
                    AI_Orbiting(owner);
                    break;
                case SwordPhase.Targeting:
                    AI_Targeting(owner);
                    break;
                case SwordPhase.FirstSlash:
                    AI_FirstSlash(owner);
                    break;
                case SwordPhase.Turnaround:
                    AI_Turnaround(owner);
                    break;
                case SwordPhase.SecondSlash:
                    AI_SecondSlash(owner);
                    break;
                case SwordPhase.Dissipate:
                    AI_Dissipate(owner);
                    break;
            }
            
            // Cosmic lighting
            float lightIntensity = spectralAlpha * 0.6f;
            Lighting.AddLight(Projectile.Center, FateCosmicVFX.FateDarkPink.ToVector3() * lightIntensity);
        }
        
        private void AI_Orbiting(Player owner)
        {
            float progress = (float)phaseTimer / OrbitTime;
            
            // Accelerating spin
            float baseSpeed = 0.08f + progress * 0.15f;
            orbitAngle += baseSpeed;
            
            // Shrinking radius as energy builds
            orbitRadius = MathHelper.Lerp(90f, 50f, progress);
            
            // Position on orbit
            Vector2 orbitOffset = orbitAngle.ToRotationVector2() * orbitRadius;
            Projectile.Center = owner.Center + orbitOffset;
            
            // Sword points outward from orbit
            Projectile.rotation = orbitAngle + MathHelper.PiOver4;
            
            // Orbiting VFX
            if (phaseTimer % 3 == 0)
            {
                Color trailColor = FateCosmicVFX.GetCosmicGradient(progress);
                var trail = new GenericGlowParticle(Projectile.Center, -orbitAngle.ToRotationVector2() * 2f,
                    trailColor * 0.6f, 0.25f, 18, true);
                MagnumParticleHandler.SpawnParticle(trail);
            }
            
            // Glyphs on orbit path
            if (phaseTimer % 8 == OrbitIndex * 2)
            {
                CustomParticles.Glyph(Projectile.Center, FateCosmicVFX.FatePurple, 0.35f, -1);
            }
            
            // Star sparkles building
            if (Main.rand.NextBool(4 - (int)(progress * 2)))
            {
                var star = new GenericGlowParticle(Projectile.Center + Main.rand.NextVector2Circular(20f, 20f),
                    Main.rand.NextVector2Circular(1f, 1f), FateCosmicVFX.FateWhite, 0.2f, 15, true);
                MagnumParticleHandler.SpawnParticle(star);
            }
            
            // Music notes occasionally
            if (phaseTimer % 15 == OrbitIndex * 5)
            {
                FateCosmicVFX.SpawnCosmicMusicNotes(Projectile.Center, 1, 15f, 0.22f);
            }
            
            if (phaseTimer >= OrbitTime)
            {
                // Find target
                targetEnemy = FindNearestEnemy(800f);
                if (targetEnemy != null)
                {
                    targetIndex = targetEnemy.whoAmI;
                    CurrentPhase = SwordPhase.Targeting;
                    phaseTimer = 0;
                    
                    // Target lock sound
                    SoundEngine.PlaySound(SoundID.Item29 with { Pitch = 0.3f + OrbitIndex * 0.1f, Volume = 0.5f }, Projectile.Center);
                }
                else
                {
                    // No target, dissipate
                    CurrentPhase = SwordPhase.Dissipate;
                    phaseTimer = 0;
                }
            }
        }
        
        private void AI_Targeting(Player owner)
        {
            // Validate target
            if (!ValidateTarget())
            {
                CurrentPhase = SwordPhase.Dissipate;
                phaseTimer = 0;
                return;
            }
            
            float progress = (float)phaseTimer / TargetTime;
            
            // Pull back slightly then charge
            if (progress < 0.4f)
            {
                // Pull back
                Vector2 pullDir = (owner.Center - targetEnemy.Center).SafeNormalize(Vector2.UnitX);
                Vector2 pullPos = owner.Center + pullDir * 60f;
                Projectile.Center = Vector2.Lerp(Projectile.Center, pullPos, 0.15f);
                
                // Point at target
                Projectile.rotation = (targetEnemy.Center - Projectile.Center).ToRotation() + MathHelper.PiOver4;
            }
            else
            {
                // Charge toward target
                Vector2 toTarget = targetEnemy.Center - Projectile.Center;
                float speed = MathHelper.Lerp(5f, 45f, (progress - 0.4f) / 0.6f);
                Projectile.velocity = toTarget.SafeNormalize(Vector2.UnitX) * speed;
                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;
            }
            
            // Targeting VFX
            if (phaseTimer % 2 == 0)
            {
                FateCosmicVFX.SpawnCosmicCloudTrail(Projectile.Center, Projectile.velocity, 0.5f);
            }
            
            // Check if reached target
            if (Vector2.Distance(Projectile.Center, targetEnemy.Center) < 60f)
            {
                slashStartPos = Projectile.Center;
                slashEndPos = targetEnemy.Center + (targetEnemy.Center - Projectile.Center).SafeNormalize(Vector2.UnitX) * 120f;
                CurrentPhase = SwordPhase.FirstSlash;
                phaseTimer = 0;
                hasDealtFirstSlash = false;
                
                // Slash sound
                SoundEngine.PlaySound(SoundID.Item71 with { Pitch = 0.4f, Volume = 0.9f }, Projectile.Center);
            }
            
            if (phaseTimer >= TargetTime + 30) // Timeout
            {
                CurrentPhase = SwordPhase.FirstSlash;
                phaseTimer = 0;
            }
        }
        
        private void AI_FirstSlash(Player owner)
        {
            if (!ValidateTarget())
            {
                CurrentPhase = SwordPhase.Dissipate;
                phaseTimer = 0;
                return;
            }
            
            float progress = (float)phaseTimer / SlashTime;
            
            // Slash through at high speed
            Vector2 slashDir = (slashEndPos - slashStartPos).SafeNormalize(Vector2.UnitX);
            Projectile.velocity = slashDir * 50f;
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;
            
            // Slash trail VFX
            FateCosmicVFX.SpawnCosmicCloudTrail(Projectile.Center, Projectile.velocity, 0.7f);
            
            if (Main.rand.NextBool(2))
            {
                var star = new GenericGlowParticle(Projectile.Center, -Projectile.velocity * 0.1f,
                    FateCosmicVFX.FateWhite, 0.3f, 12, true);
                MagnumParticleHandler.SpawnParticle(star);
            }
            
            // Impact VFX when passing through target
            if (!hasDealtFirstSlash && Vector2.Distance(Projectile.Center, targetEnemy.Center) < 80f)
            {
                hasDealtFirstSlash = true;
                FateCosmicVFX.SpawnCosmicExplosion(targetEnemy.Center, 0.8f);
                FateCosmicVFX.SpawnCosmicLightningStrike(targetEnemy.Center, 0.6f);
                FateCosmicVFX.SpawnGlyphBurst(targetEnemy.Center, 6, 6f, 0.35f);
            }
            
            if (phaseTimer >= SlashTime)
            {
                CurrentPhase = SwordPhase.Turnaround;
                phaseTimer = 0;
            }
        }
        
        private void AI_Turnaround(Player owner)
        {
            if (!ValidateTarget())
            {
                CurrentPhase = SwordPhase.Dissipate;
                phaseTimer = 0;
                return;
            }
            
            float progress = (float)phaseTimer / TurnaroundTime;
            
            // Slow down and turn
            Projectile.velocity *= 0.85f;
            
            // Arc turn toward target
            Vector2 toTarget = targetEnemy.Center - Projectile.Center;
            float targetAngle = toTarget.ToRotation() + MathHelper.PiOver4;
            Projectile.rotation = MathHelper.Lerp(Projectile.rotation, targetAngle, 0.15f);
            
            // Position adjustment - move to other side of target
            Vector2 oppositePos = targetEnemy.Center - (slashEndPos - slashStartPos).SafeNormalize(Vector2.UnitX) * 80f;
            Projectile.Center = Vector2.Lerp(Projectile.Center, oppositePos, 0.08f);
            
            // Turn VFX
            if (phaseTimer % 3 == 0)
            {
                Color sparkColor = FateCosmicVFX.GetCosmicGradient(progress);
                CustomParticles.GenericFlare(Projectile.Center, sparkColor, 0.3f, 12);
            }
            
            if (phaseTimer >= TurnaroundTime)
            {
                slashStartPos = Projectile.Center;
                slashEndPos = targetEnemy.Center + (targetEnemy.Center - Projectile.Center).SafeNormalize(Vector2.UnitX) * 120f;
                CurrentPhase = SwordPhase.SecondSlash;
                phaseTimer = 0;
                hasDealtSecondSlash = false;
                
                // Second slash sound (higher pitch)
                SoundEngine.PlaySound(SoundID.Item71 with { Pitch = 0.6f, Volume = 0.9f }, Projectile.Center);
            }
        }
        
        private void AI_SecondSlash(Player owner)
        {
            if (!ValidateTarget())
            {
                CurrentPhase = SwordPhase.Dissipate;
                phaseTimer = 0;
                return;
            }
            
            float progress = (float)phaseTimer / SlashTime;
            
            // Slash back through at high speed
            Vector2 slashDir = (slashEndPos - slashStartPos).SafeNormalize(Vector2.UnitX);
            Projectile.velocity = slashDir * 55f; // Slightly faster second slash
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;
            
            // Slash trail VFX - more intense
            FateCosmicVFX.SpawnCosmicCloudTrail(Projectile.Center, Projectile.velocity, 0.8f);
            
            for (int i = 0; i < 2; i++)
            {
                var star = new GenericGlowParticle(Projectile.Center + Main.rand.NextVector2Circular(15f, 15f), 
                    -Projectile.velocity * 0.1f, FateCosmicVFX.FateWhite, 0.35f, 15, true);
                MagnumParticleHandler.SpawnParticle(star);
            }
            
            // Impact VFX when passing through target - more dramatic
            if (!hasDealtSecondSlash && Vector2.Distance(Projectile.Center, targetEnemy.Center) < 80f)
            {
                hasDealtSecondSlash = true;
                FateCosmicVFX.SpawnCosmicExplosion(targetEnemy.Center, 1.0f);
                FateCosmicVFX.SpawnCosmicLightningStrike(targetEnemy.Center, 0.8f);
                FateCosmicVFX.SpawnGlyphBurst(targetEnemy.Center, 8, 8f, 0.4f);
                FateCosmicVFX.SpawnCosmicMusicNotes(targetEnemy.Center, 5, 40f, 0.35f);
                
                // Halo cascade
                for (int i = 0; i < 4; i++)
                {
                    Color haloColor = FateCosmicVFX.GetCosmicGradient((float)i / 4f);
                    CustomParticles.HaloRing(targetEnemy.Center, haloColor, 0.4f + i * 0.1f, 15 + i * 3);
                }
            }
            
            if (phaseTimer >= SlashTime)
            {
                CurrentPhase = SwordPhase.Dissipate;
                phaseTimer = 0;
            }
        }
        
        private void AI_Dissipate(Player owner)
        {
            float progress = (float)phaseTimer / DissipateTime;
            
            // Fade out
            spectralAlpha = 1f - progress;
            Projectile.alpha = (int)(255 * progress);
            
            // Slow down
            Projectile.velocity *= 0.9f;
            
            // Dissipation particles
            if (Main.rand.NextBool(2))
            {
                Color fadeColor = FateCosmicVFX.FateDarkPink * (1f - progress);
                var fade = new GenericGlowParticle(Projectile.Center + Main.rand.NextVector2Circular(15f, 15f),
                    Main.rand.NextVector2Circular(2f, 2f), fadeColor, 0.2f * (1f - progress), 12, true);
                MagnumParticleHandler.SpawnParticle(fade);
            }
            
            if (phaseTimer >= DissipateTime)
            {
                Projectile.Kill();
            }
        }
        
        private bool ValidateTarget()
        {
            if (targetIndex >= 0 && targetIndex < Main.maxNPCs)
            {
                targetEnemy = Main.npc[targetIndex];
                return targetEnemy.active && targetEnemy.life > 0 && !targetEnemy.friendly && targetEnemy.CanBeChasedBy();
            }
            return false;
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
            target.AddBuff(ModContent.BuffType<DestinyCollapse>(), 360);
            
            // Dramatic hit VFX
            Color hitColor = FateCosmicVFX.GetCosmicGradient(Main.rand.NextFloat());
            CustomParticles.GenericFlare(target.Center, hitColor, 0.6f, 18);
            
            for (int i = 0; i < 5; i++)
            {
                var star = new GenericGlowParticle(target.Center + Main.rand.NextVector2Circular(20f, 20f),
                    Main.rand.NextVector2Circular(4f, 4f), FateCosmicVFX.FateWhite, 0.25f, 15, true);
                MagnumParticleHandler.SpawnParticle(star);
            }
        }
        
        public override void OnKill(int timeLeft)
        {
            // Final dissipation burst
            FateCosmicVFX.SpawnGlyphBurst(Projectile.Center, 4, 3f, 0.25f);
            
            for (int i = 0; i < 6; i++)
            {
                var star = new GenericGlowParticle(Projectile.Center + Main.rand.NextVector2Circular(15f, 15f),
                    Main.rand.NextVector2Circular(2f, 2f), FateCosmicVFX.FateDarkPink * 0.6f, 0.2f, 12, true);
                MagnumParticleHandler.SpawnParticle(star);
            }
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = texture.Size() / 2f;
            
            // Spectral color with alpha
            Color spectralColor = Color.White * spectralAlpha;
            
            // Draw trail
            for (int i = 0; i < trailPositions.Length; i++)
            {
                int tIdx = (trailIdx - i + trailPositions.Length) % trailPositions.Length;
                Vector2 trailPos = trailPositions[tIdx] - Main.screenPosition;
                float trailProgress = (float)i / trailPositions.Length;
                float trailAlpha = (1f - trailProgress) * 0.35f * spectralAlpha;
                float trailScale = (1f - trailProgress * 0.4f) * 0.85f;
                
                Color trailColor = FateCosmicVFX.GetCosmicGradient(trailProgress) * trailAlpha;
                spriteBatch.Draw(texture, trailPos, null, trailColor, trailRotations[tIdx], origin, trailScale, SpriteEffects.None, 0f);
            }
            
            // Glow layers (additive)
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.15f + OrbitIndex) * 0.12f + 1f;
            
            // Outer celestial glow
            spriteBatch.Draw(texture, drawPos, null, FateCosmicVFX.FateBrightRed * 0.25f * spectralAlpha, Projectile.rotation, origin, 1.25f * pulse, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, drawPos, null, FateCosmicVFX.FateDarkPink * 0.35f * spectralAlpha, Projectile.rotation, origin, 1.12f * pulse, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, drawPos, null, FateCosmicVFX.FateWhite * 0.2f * spectralAlpha, Projectile.rotation, origin, 1.02f * pulse, SpriteEffects.None, 0f);
            
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            // Main spectral sword
            spriteBatch.Draw(texture, drawPos, null, spectralColor * 0.9f, Projectile.rotation, origin, 0.95f, SpriteEffects.None, 0f);
            
            return false;
        }
    }
}
