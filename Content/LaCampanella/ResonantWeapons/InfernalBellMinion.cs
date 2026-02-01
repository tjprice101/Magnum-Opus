using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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
using MagnumOpus.Content.LaCampanella.Debuffs;

namespace MagnumOpus.Content.LaCampanella.ResonantWeapons
{
    /// <summary>
    /// Infernal Bell Minion - A floating demonic bell that rings with devastating fire and music.
    /// The bell glows with infernal energy, smoke rises from its form, and when it strikes,
    /// flames and musical notes erupt in a symphony of destruction.
    /// 
    /// Visual Identity:
    /// - Floating bell shape with fiery inner glow
    /// - Black smoke constantly rising from the bell
    /// - Orange/gold flames flickering around it
    /// - Musical notes and fire particles on attacks
    /// - Resonant shockwave every 5 hits
    /// 
    /// TEXTURE NEEDED: InfernalBellMinion.png (36x44 pixels recommended)
    /// - A demonic/infernal bell shape
    /// - Dark metallic with orange/gold fiery accents
    /// - Inner glow suggesting flames within
    /// </summary>
    public class InfernalBellMinion : ModProjectile
    {
        // Use InfernalBellFragment as the minion sprite until a dedicated sprite is provided
        // REPLACE THIS when InfernalBellMinion.png is created
        public override string Texture => "MagnumOpus/Content/LaCampanella/SummonItems/InfernalBellFragment";
        
        // Theme colors - La Campanella palette
        private static readonly Color CampanellaOrange = new Color(255, 100, 0);
        private static readonly Color CampanellaGold = new Color(218, 165, 32);
        private static readonly Color CampanellaYellow = new Color(255, 200, 50);
        private static readonly Color CampanellaBlack = new Color(20, 15, 20);
        private static readonly Color CampanellaCrimson = new Color(200, 50, 30);
        
        // State tracking
        private int hitCounter = 0;
        private const int ShockwaveThreshold = 5;
        private float floatTimer = 0f;
        private int attackCooldown = 0;
        private NPC targetNPC = null;
        
        // Animation
        private float bellSwing = 0f; // Bell swinging animation
        private float innerGlow = 0f; // Pulsing inner glow
        private float smokeTimer = 0f;
        
        // Attack state
        private bool isRinging = false;
        private int ringTimer = 0;
        
        public override void SetStaticDefaults()
        {
            Main.projPet[Projectile.type] = true;
            ProjectileID.Sets.MinionSacrificable[Projectile.type] = true;
            ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true;
            ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true;
            
            // Trail for visual effect
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 6;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 36;
            Projectile.height = 44;
            Projectile.friendly = true;
            Projectile.minion = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.penetrate = -1;
            Projectile.minionSlots = 1f;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 20;
        }

        public override bool? CanCutTiles() => false;
        public override bool MinionContactDamage() => true;

        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];
            
            // Check buff
            if (!CheckActive(owner))
                return;
            
            // Update timers
            floatTimer += 0.06f;
            innerGlow = 0.6f + (float)Math.Sin(floatTimer * 2.5f) * 0.3f;
            bellSwing = (float)Math.Sin(floatTimer * 1.5f) * 0.15f;
            smokeTimer += 0.1f;
            attackCooldown = Math.Max(0, attackCooldown - 1);
            
            // Ring animation cooldown
            if (isRinging)
            {
                ringTimer++;
                if (ringTimer > 20)
                {
                    isRinging = false;
                    ringTimer = 0;
                }
            }
            
            // Find target
            FindTarget(owner);
            
            if (targetNPC != null && targetNPC.active)
            {
                // Move toward target
                MoveTowardTarget();
                
                // Attack
                if (attackCooldown <= 0)
                {
                    Attack();
                    attackCooldown = 35;
                }
            }
            else
            {
                // Return to owner
                ReturnToOwner(owner);
            }
            
            // Ambient VFX
            SpawnAmbientParticles();
            
            // Bell swinging rotation
            Projectile.rotation = bellSwing;
            
            // Dynamic lighting - pulsing orange glow
            float lightPulse = innerGlow * 0.7f;
            Lighting.AddLight(Projectile.Center, CampanellaOrange.ToVector3() * lightPulse);
        }

        private bool CheckActive(Player owner)
        {
            if (owner.dead || !owner.active)
            {
                owner.ClearBuff(ModContent.BuffType<CampanellaChoirBuff>());
                return false;
            }
            
            if (owner.HasBuff(ModContent.BuffType<CampanellaChoirBuff>()))
            {
                Projectile.timeLeft = 2;
            }
            
            return true;
        }

        private void FindTarget(Player owner)
        {
            // Check for player-designated target
            if (owner.HasMinionAttackTargetNPC)
            {
                NPC designated = Main.npc[owner.MinionAttackTargetNPC];
                if (designated.active && designated.CanBeChasedBy())
                {
                    targetNPC = designated;
                    return;
                }
            }
            
            // Find nearest enemy
            float nearestDist = 900f;
            targetNPC = null;
            
            foreach (NPC npc in Main.npc)
            {
                if (!npc.active || npc.friendly || !npc.CanBeChasedBy()) continue;
                
                float dist = Vector2.Distance(Projectile.Center, npc.Center);
                if (dist < nearestDist)
                {
                    nearestDist = dist;
                    targetNPC = npc;
                }
            }
        }

        private void MoveTowardTarget()
        {
            Vector2 toTarget = targetNPC.Center - Projectile.Center;
            float distance = toTarget.Length();
            
            // Maintain distance for ranged attacks
            float idealDist = 180f;
            float speed = 14f;
            
            if (distance > idealDist + 60f)
            {
                // Move closer aggressively
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, toTarget.SafeNormalize(Vector2.Zero) * speed, 0.12f);
            }
            else if (distance < idealDist - 40f)
            {
                // Move away
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, -toTarget.SafeNormalize(Vector2.Zero) * speed * 0.5f, 0.1f);
            }
            else
            {
                // Orbit around target
                Vector2 orbitVel = toTarget.SafeNormalize(Vector2.Zero).RotatedBy(MathHelper.PiOver2) * speed * 0.4f;
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, orbitVel, 0.06f);
            }
            
            // Add floating motion
            Projectile.velocity.Y += (float)Math.Sin(floatTimer * 2f) * 0.08f;
        }

        private void ReturnToOwner(Player owner)
        {
            Vector2 targetPos = owner.Center + new Vector2(owner.direction * 50f, -70f);
            Vector2 toTarget = targetPos - Projectile.Center;
            
            if (toTarget.Length() > 700f)
            {
                // Teleport if too far
                Projectile.Center = targetPos;
                TeleportVFX(Projectile.Center);
            }
            else if (toTarget.Length() > 40f)
            {
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, toTarget.SafeNormalize(Vector2.Zero) * 11f, 0.08f);
            }
            else
            {
                Projectile.velocity *= 0.9f;
            }
            
            // Gentle floating animation
            Projectile.velocity.Y += (float)Math.Sin(floatTimer) * 0.15f;
        }

        private void Attack()
        {
            if (targetNPC == null) return;
            
            Vector2 toTarget = (targetNPC.Center - Projectile.Center).SafeNormalize(Vector2.UnitX);
            
            // Trigger ring animation
            isRinging = true;
            ringTimer = 0;
            
            // Fire infernal bell wave projectile
            if (Main.myPlayer == Projectile.owner)
            {
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, toTarget * 14f,
                    ModContent.ProjectileType<InfernalBellWave>(), Projectile.damage, Projectile.knockBack, Projectile.owner, Projectile.whoAmI);
            }
            
            // === BELL RING SOUND - The infernal chime ===
            SoundEngine.PlaySound(SoundID.Item35 with { Pitch = 0.3f + Main.rand.NextFloat(0.2f), Volume = 0.55f }, Projectile.Center);
            SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.5f, Volume = 0.35f }, Projectile.Center);
            
            // === ATTACK VFX ===
            // Central flare
            CustomParticles.GenericFlare(Projectile.Center, CampanellaOrange, 0.6f, 15);
            CustomParticles.GenericFlare(Projectile.Center, CampanellaYellow, 0.4f, 12);
            
            // Fire sparks in attack direction
            for (int i = 0; i < 6; i++)
            {
                float spread = MathHelper.ToRadians(25f);
                Vector2 sparkVel = toTarget.RotatedBy(Main.rand.NextFloat(-spread, spread)) * Main.rand.NextFloat(5f, 10f);
                Color sparkColor = Main.rand.NextBool() ? CampanellaOrange : CampanellaYellow;
                
                var spark = new GlowSparkParticle(Projectile.Center, sparkVel, sparkColor, 0.3f, 18);
                MagnumParticleHandler.SpawnParticle(spark);
            }
            
            // Halo ring on attack
            CustomParticles.HaloRing(Projectile.Center, CampanellaOrange, 0.35f, 14);
            
            // Music notes burst
            ThemedParticles.LaCampanellaMusicNotes(Projectile.Center, 3, 25f);
            
            // Smoke puff
            for (int i = 0; i < 3; i++)
            {
                Vector2 smokeVel = -toTarget.RotatedBy(Main.rand.NextFloat(-0.5f, 0.5f)) * Main.rand.NextFloat(1f, 3f);
                var smoke = new HeavySmokeParticle(
                    Projectile.Center,
                    smokeVel,
                    CampanellaBlack,
                    Main.rand.Next(20, 35),
                    Main.rand.NextFloat(0.2f, 0.35f),
                    Main.rand.NextFloat(0.4f, 0.6f),
                    0.02f,
                    false
                );
                MagnumParticleHandler.SpawnParticle(smoke);
            }
            
            Lighting.AddLight(Projectile.Center, 0.9f, 0.5f, 0.15f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            hitCounter++;
            
            // Apply Resonant Toll debuff
            if (ModContent.TryFind<ModBuff>("MagnumOpus/ResonantToll", out ModBuff resonantToll))
            {
                target.AddBuff(resonantToll.Type, 180);
            }
            
            // Hit VFX
            CustomParticles.GenericFlare(target.Center, CampanellaOrange, 0.5f, 12);
            CustomParticles.HaloRing(target.Center, CampanellaYellow * 0.7f, 0.25f, 10);
            
            // Shockwave every 5 hits
            if (hitCounter >= ShockwaveThreshold)
            {
                hitCounter = 0;
                TriggerShockwave(target.Center);
            }
        }

        private void TriggerShockwave(Vector2 position)
        {
            // === RESONANT SHOCKWAVE - Every 5 hits ===
            SoundEngine.PlaySound(SoundID.Item122 with { Pitch = -0.2f, Volume = 0.7f }, position);
            
            // Massive flare
            CustomParticles.GenericFlare(position, Color.White, 0.9f, 20);
            CustomParticles.GenericFlare(position, CampanellaOrange, 0.7f, 18);
            
            // Cascading halos
            for (int i = 0; i < 5; i++)
            {
                float progress = i / 5f;
                Color ringColor = Color.Lerp(CampanellaOrange, CampanellaGold, progress);
                CustomParticles.HaloRing(position, ringColor, 0.3f + i * 0.15f, 15 + i * 3);
            }
            
            // Glyph burst
            CustomParticles.GlyphBurst(position, CampanellaOrange, 6, 5f);
            
            // Music note explosion
            ThemedParticles.LaCampanellaMusicNotes(position, 8, 50f);
            
            // Fire sparks radial burst
            for (int i = 0; i < 12; i++)
            {
                float angle = MathHelper.TwoPi * i / 12f;
                Vector2 sparkVel = angle.ToRotationVector2() * Main.rand.NextFloat(6f, 12f);
                Color sparkColor = Color.Lerp(CampanellaOrange, CampanellaYellow, Main.rand.NextFloat());
                
                var spark = new GlowSparkParticle(position, sparkVel, sparkColor, 0.35f, 20);
                MagnumParticleHandler.SpawnParticle(spark);
            }
            
            // Damage nearby enemies
            if (Main.myPlayer == Projectile.owner)
            {
                float shockRadius = 150f;
                foreach (NPC npc in Main.npc)
                {
                    if (!npc.active || npc.friendly || !npc.CanBeChasedBy()) continue;
                    
                    float dist = Vector2.Distance(position, npc.Center);
                    if (dist < shockRadius)
                    {
                        int shockDamage = (int)(Projectile.damage * 0.5f);
                        npc.SimpleStrikeNPC(shockDamage, 0, false, 0f, DamageClass.Summon);
                    }
                }
            }
            
            Lighting.AddLight(position, 1.2f, 0.7f, 0.2f);
        }

        private void SpawnAmbientParticles()
        {
            // ☁EMUSICAL PRESENCE - Infernal bell minion ambient melody
            if (Main.rand.NextBool(10))
            {
                Color noteColor = Color.Lerp(new Color(255, 140, 40), new Color(255, 200, 50), Main.rand.NextFloat());
                Vector2 noteVel = new Vector2(Main.rand.NextFloat(-0.25f, 0.25f), -0.6f);
                ThemedParticles.MusicNote(Projectile.Center, noteVel, noteColor, 0.25f, 35);
            }
            
            // Rising smoke from the bell
            if (Main.rand.NextBool(4))
            {
                Vector2 smokePos = Projectile.Center + new Vector2(Main.rand.NextFloat(-12f, 12f), -Projectile.height * 0.3f);
                Vector2 smokeVel = new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), Main.rand.NextFloat(-1.5f, -0.5f));
                
                var smoke = new HeavySmokeParticle(
                    smokePos,
                    smokeVel,
                    CampanellaBlack * 0.7f,
                    Main.rand.Next(30, 50),
                    Main.rand.NextFloat(0.15f, 0.25f),
                    Main.rand.NextFloat(0.3f, 0.5f),
                    0.012f,
                    false
                );
                MagnumParticleHandler.SpawnParticle(smoke);
            }
            
            // Flickering flame particles around the bell
            if (Main.rand.NextBool(6))
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                float radius = Main.rand.NextFloat(15f, 25f);
                Vector2 flamePos = Projectile.Center + angle.ToRotationVector2() * radius;
                Vector2 flameVel = new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-2f, -0.5f));
                
                Color flameColor = Main.rand.NextBool() ? CampanellaOrange : CampanellaYellow;
                var flame = new GenericGlowParticle(flamePos, flameVel, flameColor * 0.7f, 0.2f, 15, true);
                MagnumParticleHandler.SpawnParticle(flame);
            }
            
            // Inner glow flare (pulsing)
            if (Main.rand.NextBool(8))
            {
                Vector2 glowPos = Projectile.Center + Main.rand.NextVector2Circular(8f, 8f);
                CustomParticles.GenericFlare(glowPos, CampanellaOrange * innerGlow, 0.2f, 8);
            }
            
            // Occasional music note
            if (Main.rand.NextBool(25))
            {
                Vector2 notePos = Projectile.Center + Main.rand.NextVector2Circular(20f, 20f);
                ThemedParticles.LaCampanellaMusicNotes(notePos, 1, 15f);
            }
            
            // Spark embers falling
            if (Main.rand.NextBool(12))
            {
                Vector2 emberPos = Projectile.Center + new Vector2(Main.rand.NextFloat(-15f, 15f), Projectile.height * 0.3f);
                Vector2 emberVel = new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), Main.rand.NextFloat(0.5f, 1.5f));
                
                Color emberColor = Color.Lerp(CampanellaOrange, CampanellaCrimson, Main.rand.NextFloat());
                var ember = new GlowSparkParticle(emberPos, emberVel, emberColor * 0.6f, 0.15f, 20);
                MagnumParticleHandler.SpawnParticle(ember);
            }
        }

        private void TeleportVFX(Vector2 position)
        {
            CustomParticles.GenericFlare(position, CampanellaOrange, 0.7f, 18);
            CustomParticles.HaloRing(position, CampanellaOrange, 0.4f, 15);
            
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 sparkVel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 6f);
                Color sparkColor = Main.rand.NextBool() ? CampanellaOrange : CampanellaYellow;
                
                var spark = new GlowSparkParticle(position, sparkVel, sparkColor, 0.25f, 15);
                MagnumParticleHandler.SpawnParticle(spark);
            }
            
            SoundEngine.PlaySound(SoundID.Item8 with { Pitch = 0.3f }, position);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = texture.Size() / 2f;
            
            SpriteEffects effects = Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            
            // Draw afterimage trail
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                float progress = (float)i / Projectile.oldPos.Length;
                float trailAlpha = (1f - progress) * 0.3f;
                Color trailColor = Color.Lerp(CampanellaOrange, CampanellaBlack, progress) * trailAlpha;
                Vector2 trailPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                float trailScale = 1f - progress * 0.3f;
                
                spriteBatch.Draw(texture, trailPos, null, trailColor, Projectile.oldRot[i], origin, trailScale, effects, 0f);
            }
            
            // Additive glow layer
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            // Outer infernal glow
            float pulse = innerGlow;
            Color outerGlow = CampanellaOrange * 0.4f * pulse;
            spriteBatch.Draw(texture, drawPos, null, outerGlow, Projectile.rotation, origin, 1.3f * pulse, effects, 0f);
            
            // Middle glow layer
            Color midGlow = CampanellaYellow * 0.3f * pulse;
            spriteBatch.Draw(texture, drawPos, null, midGlow, Projectile.rotation, origin, 1.15f * pulse, effects, 0f);
            
            // Inner bright glow (when ringing)
            if (isRinging)
            {
                float ringPulse = 1f - (ringTimer / 20f);
                Color ringGlow = Color.White * 0.5f * ringPulse;
                spriteBatch.Draw(texture, drawPos, null, ringGlow, Projectile.rotation, origin, 1.1f + ringPulse * 0.2f, effects, 0f);
            }
            
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            // Main sprite
            spriteBatch.Draw(texture, drawPos, null, lightColor, Projectile.rotation, origin, 1f, effects, 0f);
            
            return false;
        }
    }

    /// <summary>
    /// Infernal Bell Wave - The flame wave projectile fired by the Infernal Bell Minion.
    /// A crescent of fire that damages enemies and applies burning.
    /// </summary>
    public class InfernalBellWave : ModProjectile
    {
        // Use a soft glow texture - the wave is particle-based
        public override string Texture => "MagnumOpus/Assets/Particles/EnergyFlare2";
        
        private static readonly Color CampanellaOrange = new Color(255, 100, 0);
        private static readonly Color CampanellaYellow = new Color(255, 200, 50);
        private static readonly Color CampanellaBlack = new Color(20, 15, 20);

        public override void SetDefaults()
        {
            Projectile.width = 40;
            Projectile.height = 40;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.penetrate = 3;
            Projectile.timeLeft = 50;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 15;
            Projectile.extraUpdates = 1;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
            
            // ☁EMUSICAL NOTATION - Infernal bell wave melodic trail
            if (Main.rand.NextBool(5))
            {
                Color noteColor = Color.Lerp(new Color(255, 140, 40), new Color(255, 200, 50), Main.rand.NextFloat());
                Vector2 noteVel = new Vector2(Main.rand.NextFloat(-0.4f, 0.4f), -0.8f);
                ThemedParticles.MusicNote(Projectile.Center, noteVel, noteColor, 0.3f, 30);
            }
            
            // Wave trail particles
            if (Main.rand.NextBool(2))
            {
                Vector2 offset = Projectile.velocity.SafeNormalize(Vector2.Zero).RotatedBy(MathHelper.PiOver2) * Main.rand.NextFloat(-15f, 15f);
                Vector2 trailPos = Projectile.Center + offset;
                Vector2 trailVel = -Projectile.velocity * 0.15f + Main.rand.NextVector2Circular(1f, 1f);
                
                Color trailColor = Main.rand.NextBool() ? CampanellaOrange : CampanellaYellow;
                var trail = new GenericGlowParticle(trailPos, trailVel, trailColor * 0.7f, 0.3f, 15, true);
                MagnumParticleHandler.SpawnParticle(trail);
            }
            
            // Smoke wisps
            if (Main.rand.NextBool(4))
            {
                Vector2 smokeVel = -Projectile.velocity * 0.1f + new Vector2(0, Main.rand.NextFloat(-1f, 0f));
                var smoke = new HeavySmokeParticle(
                    Projectile.Center,
                    smokeVel,
                    CampanellaBlack * 0.5f,
                    Main.rand.Next(15, 25),
                    Main.rand.NextFloat(0.15f, 0.25f),
                    Main.rand.NextFloat(0.3f, 0.5f),
                    0.02f,
                    false
                );
                MagnumParticleHandler.SpawnParticle(smoke);
            }
            
            // Central glow
            CustomParticles.GenericFlare(Projectile.Center, CampanellaOrange * 0.6f, 0.25f, 4);
            
            Lighting.AddLight(Projectile.Center, CampanellaOrange.ToVector3() * 0.4f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Apply On Fire debuff
            target.AddBuff(BuffID.OnFire, 180);
            
            // ☁EMUSICAL IMPACT - Infernal bell wave impact burst
            ThemedParticles.MusicNoteBurst(target.Center, new Color(255, 140, 40), 4, 3f);
            
            // Hit VFX
            CustomParticles.GenericFlare(target.Center, CampanellaOrange, 0.5f, 12);
            CustomParticles.HaloRing(target.Center, CampanellaYellow * 0.6f, 0.2f, 10);
            
            // Fire sparks
            for (int i = 0; i < 4; i++)
            {
                Vector2 sparkVel = Main.rand.NextVector2Circular(4f, 4f);
                Color sparkColor = Main.rand.NextBool() ? CampanellaOrange : CampanellaYellow;
                
                var spark = new GlowSparkParticle(target.Center, sparkVel, sparkColor, 0.2f, 12);
                MagnumParticleHandler.SpawnParticle(spark);
            }
        }

        public override void OnKill(int timeLeft)
        {
            // ☁EMUSICAL FINALE - Golden notes on bell wave death
            ThemedParticles.MusicNoteBurst(Projectile.Center, new Color(218, 165, 32), 4, 3f);
            
            // Death burst
            CustomParticles.GenericFlare(Projectile.Center, CampanellaOrange, 0.5f, 15);
            CustomParticles.HaloRing(Projectile.Center, CampanellaOrange, 0.3f, 12);
            
            // Dissipating sparks
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f;
                Vector2 sparkVel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 5f);
                Color sparkColor = Color.Lerp(CampanellaOrange, CampanellaYellow, Main.rand.NextFloat());
                
                var spark = new GlowSparkParticle(Projectile.Center, sparkVel, sparkColor * 0.6f, 0.2f, 15);
                MagnumParticleHandler.SpawnParticle(spark);
            }
            
            SoundEngine.PlaySound(SoundID.Item34 with { Pitch = 0.5f, Volume = 0.3f }, Projectile.Center);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = texture.Size() / 2f;
            
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            float pulse = 0.9f + (float)Math.Sin(Main.GameUpdateCount * 0.3f) * 0.1f;
            
            // Outer glow
            spriteBatch.Draw(texture, drawPos, null, CampanellaOrange * 0.4f, Projectile.rotation, origin, 0.6f * pulse, SpriteEffects.None, 0f);
            // Middle glow
            spriteBatch.Draw(texture, drawPos, null, CampanellaYellow * 0.5f, Projectile.rotation, origin, 0.4f * pulse, SpriteEffects.None, 0f);
            // Core
            spriteBatch.Draw(texture, drawPos, null, Color.White * 0.6f, Projectile.rotation, origin, 0.2f * pulse, SpriteEffects.None, 0f);
            
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            return false;
        }
    }
}
