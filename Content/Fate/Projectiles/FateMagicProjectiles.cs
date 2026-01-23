using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Content.Fate.Debuffs;

namespace MagnumOpus.Content.Fate.Projectiles
{
    #region Fate1Magic Projectiles - Cosmic Electricity

    /// <summary>
    /// Held staff that channels cosmic electricity toward the cursor
    /// Renders as The Final Fermata - the last pause before eternal silence
    /// </summary>
    public class CosmicElectricityStaff : ModProjectile
    {
        // Use The Final Fermata texture
        public override string Texture => "MagnumOpus/Content/Fate/ResonantWeapons/TheFinalFermata";

        private HashSet<int> hitEnemies = new HashSet<int>();
        private int zapCooldown = 0;
        private float pulsePhase = 0f;

        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 30;
            Projectile.friendly = false;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
        }

        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];

            if (!owner.active || owner.dead || !owner.channel)
            {
                Projectile.Kill();
                return;
            }

            pulsePhase += 0.1f;
            
            // Position staff at player, aimed toward cursor
            Vector2 aimDir = (Main.MouseWorld - owner.Center).SafeNormalize(Vector2.UnitX);
            Projectile.Center = owner.Center + aimDir * 35f;
            Projectile.rotation = aimDir.ToRotation() + MathHelper.PiOver4;
            owner.ChangeDir(aimDir.X > 0 ? 1 : -1);
            owner.heldProj = Projectile.whoAmI;
            owner.itemTime = 2;
            owner.itemAnimation = 2;
            owner.itemRotation = aimDir.ToRotation();

            zapCooldown--;

            // Find and shock all nearby enemies
            if (zapCooldown <= 0)
            {
                zapCooldown = 8;
                ZapNearbyEnemies(owner);
            }

            // Check for zodiac explosion
            if (hitEnemies.Count >= 3)
            {
                TriggerZodiacExplosion(owner);
                hitEnemies.Clear();
            }

            // === ENHANCED CHANNELING VFX ===
            // Electricity aura around player
            if (Main.rand.NextBool(3))
            {
                FateCosmicVFX.SpawnCosmicElectricity(owner.Center, 2, 60f, 0.6f);
            }
            
            // Orbiting glyphs
            if (Main.rand.NextBool(6))
            {
                float glyphAngle = pulsePhase + Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 glyphPos = Projectile.Center + glyphAngle.ToRotationVector2() * 25f;
                CustomParticles.Glyph(glyphPos, FateCosmicVFX.FateCyan, 0.25f, -1);
            }
            
            // Star sparkles at staff tip
            if (Main.rand.NextBool(5))
            {
                Vector2 tipPos = Projectile.Center + aimDir * 20f;
                FateCosmicVFX.SpawnStarSparkles(tipPos, 1, 15f, 0.2f);
            }

            // Music notes
            if (Main.rand.NextBool(8))
            {
                FateCosmicVFX.SpawnCosmicMusicNotes(owner.Center, 1, 40f, 0.3f);
            }

            Lighting.AddLight(owner.Center, FateCosmicVFX.FateCyan.ToVector3() * 0.8f);
            Lighting.AddLight(Projectile.Center, FateCosmicVFX.FateDarkPink.ToVector3() * 0.5f);
        }

        private void ZapNearbyEnemies(Player owner)
        {
            float range = 350f;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly || npc.dontTakeDamage) continue;

                float dist = Vector2.Distance(owner.Center, npc.Center);
                if (dist < range)
                {
                    // Draw lightning to enemy
                    FateCosmicVFX.DrawCosmicLightning(owner.Center, npc.Center, 10, 30f, 2, 0.3f, FateCosmicVFX.FateCyan, FateCosmicVFX.FateWhite);

                    // Deal damage
                    npc.SimpleStrikeNPC(Projectile.damage, owner.direction, false, 0f);
                    npc.AddBuff(ModContent.BuffType<DestinyCollapse>(), 120);

                    // Track unique enemies hit
                    hitEnemies.Add(npc.whoAmI);

                    // Impact sparks
                    FateCosmicVFX.SpawnStarSparkles(npc.Center, 4, 20f, 0.2f);
                }
            }

            if (hitEnemies.Count > 0)
            {
                SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.3f, Volume = 0.6f }, owner.Center);
            }
        }

        private void TriggerZodiacExplosion(Player owner)
        {
            // Spawn zodiac explosion projectile
            Projectile.NewProjectile(
                Projectile.GetSource_FromThis(),
                owner.Center,
                Vector2.Zero,
                ModContent.ProjectileType<ZodiacExplosion>(),
                (int)(Projectile.damage * 3f),
                10f,
                Projectile.owner
            );

            SoundEngine.PlaySound(SoundID.Item29 with { Pitch = 0.2f, Volume = 1.2f }, owner.Center);
        }

        public override bool? CanDamage() => false;

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
            Vector2 origin = tex.Size() / 2f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            
            float pulse = 1f + (float)Math.Sin(pulsePhase) * 0.1f;
            float glowIntensity = 0.6f + (float)Math.Sin(pulsePhase * 0.7f) * 0.25f;

            // === MULTI-LAYER BLOOM STACK FOR COSMIC ELECTRICITY GLOW ===
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            // Outer cyan electricity glow
            spriteBatch.Draw(tex, drawPos, null, FateCosmicVFX.FateCyan * 0.2f * glowIntensity, Projectile.rotation, origin, 1.6f * pulse, SpriteEffects.None, 0f);
            
            // Middle dark pink cosmic energy
            spriteBatch.Draw(tex, drawPos, null, FateCosmicVFX.FateDarkPink * 0.3f * glowIntensity, Projectile.rotation, origin, 1.3f * pulse, SpriteEffects.None, 0f);
            
            // Inner white core
            spriteBatch.Draw(tex, drawPos, null, FateCosmicVFX.FateWhite * 0.25f * glowIntensity, Projectile.rotation, origin, 1.1f * pulse, SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            // Main staff - The Final Fermata
            spriteBatch.Draw(tex, drawPos, null, Color.White, Projectile.rotation, origin, pulse, SpriteEffects.None, 0f);

            return false;
        }
    }

    /// <summary>
    /// Screen-wide zodiac explosion effect
    /// </summary>
    public class ZodiacExplosion : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/SoftGlow";

        private const int Duration = 60;

        public override void SetDefaults()
        {
            Projectile.width = 50;
            Projectile.height = 50;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = -1;
            Projectile.timeLeft = Duration;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }

        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];
            Projectile.Center = owner.Center;

            float progress = 1f - (float)Projectile.timeLeft / Duration;
            
            // Expanding zodiac circle
            float radius = progress * 600f;
            
            // Update hitbox to match explosion radius
            int size = (int)(radius * 2);
            Projectile.width = Projectile.height = Math.Max(50, size);

            // Zodiac symbols around the circle
            int symbolCount = 12; // 12 zodiac signs
            for (int i = 0; i < symbolCount; i++)
            {
                float angle = MathHelper.TwoPi * i / symbolCount + Main.GameUpdateCount * 0.02f;
                Vector2 symbolPos = owner.Center + angle.ToRotationVector2() * radius;
                
                // Glyph at each zodiac position
                Color glyphColor = FateCosmicVFX.GetCosmicGradient((float)i / symbolCount);
                var glyph = new GenericGlowParticle(symbolPos, angle.ToRotationVector2() * 2f, glyphColor, 0.4f, 8, true);
                MagnumParticleHandler.SpawnParticle(glyph);

                // Star at each position
                var star = new GenericGlowParticle(symbolPos, Vector2.Zero, FateCosmicVFX.FateWhite * 0.8f, 0.3f, 8, true);
                MagnumParticleHandler.SpawnParticle(star);
            }

            // Connecting lines between symbols
            if (Main.GameUpdateCount % 3 == 0)
            {
                for (int i = 0; i < symbolCount; i++)
                {
                    float angle1 = MathHelper.TwoPi * i / symbolCount + Main.GameUpdateCount * 0.02f;
                    float angle2 = MathHelper.TwoPi * ((i + 1) % symbolCount) / symbolCount + Main.GameUpdateCount * 0.02f;
                    Vector2 pos1 = owner.Center + angle1.ToRotationVector2() * radius;
                    Vector2 pos2 = owner.Center + angle2.ToRotationVector2() * radius;
                    
                    FateCosmicVFX.SpawnConstellationLine(pos1, pos2, FateCosmicVFX.FatePurple * 0.5f);
                }
            }

            // Central cosmic explosion building
            float centralIntensity = progress;
            FateCosmicVFX.SpawnCosmicCloudBurst(owner.Center, centralIntensity * 0.5f, 8);
            
            // Music notes bursting outward
            if (Main.rand.NextBool(2))
            {
                float randAngle = Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 notePos = owner.Center + randAngle.ToRotationVector2() * radius * 0.5f;
                FateCosmicVFX.SpawnCosmicMusicNotes(notePos, 2, 20f, 0.35f);
            }

            // Lightning effects
            if (Main.rand.NextBool(4))
            {
                float randAngle = Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 lightningEnd = owner.Center + randAngle.ToRotationVector2() * radius;
                FateCosmicVFX.DrawCosmicLightning(owner.Center, lightningEnd, 12, 40f, 3, 0.4f);
            }

            Lighting.AddLight(owner.Center, FateCosmicVFX.FateDarkPink.ToVector3() * (1f + progress));
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float progress = 1f - (float)Projectile.timeLeft / Duration;
            float radius = progress * 600f;
            float innerRadius = radius * 0.8f;

            Vector2 targetCenter = targetHitbox.Center.ToVector2();
            float dist = Vector2.Distance(Projectile.Center, targetCenter);

            // Damage enemies in the expanding ring
            return dist >= innerRadius && dist <= radius;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<DestinyCollapse>(), 300);
            FateCosmicVFX.SpawnCosmicExplosion(target.Center, 0.5f);
        }

        public override bool PreDraw(ref Color lightColor) => false;
    }

    #endregion

    #region Fate2Magic Projectiles - Spectral Sword Blades

    /// <summary>
    /// Aggressive spectral sword blade that hunts enemies like a dragonfly
    /// Seeks targets, dashes at them rapidly, explodes on contact
    /// Renders as pulsing, glowing copies of the 5 Fate melee weapons
    /// </summary>
    public class SpiralingSpectralBlade : ModProjectile
    {
        // Use Fate melee weapon textures - the actual weapons from the mod!
        private static readonly string[] FateWeaponTextures = new string[] 
        { 
            "MagnumOpus/Content/Fate/ResonantWeapons/TheConductorsLastConstellation",
            "MagnumOpus/Content/Fate/ResonantWeapons/CodaOfAnnihilation",
            "MagnumOpus/Content/Fate/ResonantWeapons/FractalOfTheStars",
            "MagnumOpus/Content/Fate/ResonantWeapons/RequiemOfReality",
            "MagnumOpus/Content/Fate/ResonantWeapons/OpusUltima"
        };

        private int weaponTextureIndex = 0;
        private float pulsePhase = 0f;
        private string currentTexturePath;
        
        // Aggressive hunting AI state
        private int attackPhase = 0;  // 0 = seeking, 1 = charging/dashing, 2 = cooldown
        private int phaseTimer = 0;
        private NPC currentTarget = null;
        private Vector2 dashDirection = Vector2.Zero;
        
        private const float SeekRange = 600f;
        private const float DashSpeed = 28f;
        private const int DashDuration = 15;
        private const int CooldownDuration = 8;

        // Fallback to first weapon texture if load fails
        public override string Texture => "MagnumOpus/Content/Fate/ResonantWeapons/TheConductorsLastConstellation";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 15;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 30;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 3;  // Can hit 3 enemies before dying
            Projectile.timeLeft = 180; // 3 seconds to hunt
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }

        public override void OnSpawn(IEntitySource source)
        {
            // Pick random Fate weapon texture
            weaponTextureIndex = Main.rand.Next(FateWeaponTextures.Length);
            currentTexturePath = FateWeaponTextures[weaponTextureIndex];
            
            // Random starting pulse phase for variety
            pulsePhase = Main.rand.NextFloat(MathHelper.TwoPi);
            
            // Start in seeking mode
            attackPhase = 0;
            phaseTimer = 0;
            
            // Spawn VFX
            FateCosmicVFX.SpawnGlyphBurst(Projectile.Center, 3, 3f, 0.25f);
        }

        public override void AI()
        {
            // Update pulse phase for magical glow
            pulsePhase += 0.15f;
            phaseTimer++;

            switch (attackPhase)
            {
                case 0: // SEEKING - Hunt for target
                    AI_Seeking();
                    break;
                case 1: // DASHING - Fast attack dash
                    AI_Dashing();
                    break;
                case 2: // COOLDOWN - Brief pause before next attack
                    AI_Cooldown();
                    break;
            }
            
            // Rotate blade to face movement
            if (Projectile.velocity.LengthSquared() > 1f)
                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;

            // Spawn trail VFX based on phase
            SpawnTrailEffects();
            
            // Dynamic lighting
            float lightPulse = 0.6f + (float)Math.Sin(pulsePhase) * 0.25f;
            Lighting.AddLight(Projectile.Center, FateCosmicVFX.FateDarkPink.ToVector3() * lightPulse);
        }

        private void AI_Seeking()
        {
            // Find nearest enemy
            currentTarget = FindNearestEnemy();
            
            if (currentTarget != null)
            {
                // Home in on target aggressively
                Vector2 toTarget = currentTarget.Center - Projectile.Center;
                float distance = toTarget.Length();
                
                if (distance < 80f)
                {
                    // Close enough - begin dash attack!
                    attackPhase = 1;
                    phaseTimer = 0;
                    dashDirection = toTarget.SafeNormalize(Vector2.UnitX);
                    Projectile.velocity = dashDirection * DashSpeed;
                    
                    // Dash initiation VFX
                    FateCosmicVFX.SpawnGlyphBurst(Projectile.Center, 4, 5f, 0.3f);
                    SoundEngine.PlaySound(SoundID.Item1 with { Pitch = 0.5f, Volume = 0.6f }, Projectile.Center);
                }
                else
                {
                    // Aggressively home toward target
                    toTarget.Normalize();
                    float homingStrength = 0.25f;
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, toTarget * 18f, homingStrength);
                    
                    // Cap speed during seeking
                    if (Projectile.velocity.Length() > 20f)
                        Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.Zero) * 20f;
                }
            }
            else
            {
                // No target - drift toward cursor or slow down
                Vector2 toCursor = Main.MouseWorld - Projectile.Center;
                if (toCursor.Length() > 50f)
                {
                    toCursor.Normalize();
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, toCursor * 12f, 0.1f);
                }
                else
                {
                    Projectile.velocity *= 0.95f;
                }
            }
        }

        private void AI_Dashing()
        {
            // Maintain dash velocity
            Projectile.velocity = dashDirection * DashSpeed;
            
            // Extra aggressive trail during dash
            if (Main.rand.NextBool(2))
            {
                var spark = new GlowSparkParticle(Projectile.Center, -Projectile.velocity * 0.15f + Main.rand.NextVector2Circular(3f, 3f),
                    FateCosmicVFX.FateWhite, 0.25f, 12);
                MagnumParticleHandler.SpawnParticle(spark);
            }
            
            if (phaseTimer >= DashDuration)
            {
                // Dash complete - enter cooldown
                attackPhase = 2;
                phaseTimer = 0;
            }
        }

        private void AI_Cooldown()
        {
            // Slow down during cooldown
            Projectile.velocity *= 0.85f;
            
            if (phaseTimer >= CooldownDuration)
            {
                // Ready for next attack
                attackPhase = 0;
                phaseTimer = 0;
            }
        }

        private NPC FindNearestEnemy()
        {
            NPC closest = null;
            float closestDist = SeekRange;
            
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && npc.CanBeChasedBy(Projectile) && !npc.friendly)
                {
                    float dist = Vector2.Distance(Projectile.Center, npc.Center);
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        closest = npc;
                    }
                }
            }
            
            return closest;
        }

        private void SpawnTrailEffects()
        {
            // More intense effects during dash
            bool isDashing = attackPhase == 1;
            
            // Cosmic cloud trail
            if (Main.rand.NextBool(isDashing ? 2 : 4))
            {
                FateCosmicVFX.SpawnCosmicCloudTrail(Projectile.Center, Projectile.velocity, isDashing ? 0.8f : 0.5f);
            }
            
            // Star sparkles
            if (Main.rand.NextBool(isDashing ? 2 : 5))
            {
                FateCosmicVFX.SpawnStarSparkles(Projectile.Center, 1, 15f, 0.2f);
            }

            // Music notes - it's a symphony!
            if (Main.rand.NextBool(10))
            {
                FateCosmicVFX.SpawnCosmicMusicNotes(Projectile.Center, 1, 12f, 0.22f);
            }
            
            // Orbiting glyphs
            if (Main.rand.NextBool(12))
            {
                CustomParticles.Glyph(Projectile.Center + Main.rand.NextVector2Circular(12f, 12f), 
                    FateCosmicVFX.FatePurple, 0.2f, -1);
            }
            
            // Cosmic sparks during dash
            if (isDashing && Main.rand.NextBool(3))
            {
                FateCosmicVFX.SpawnCosmicElectricity(Projectile.Center, 1, 12f, 0.35f);
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<DestinyCollapse>(), 180);
            
            // Impact VFX
            FateCosmicVFX.SpawnCosmicExplosion(target.Center, 0.6f);
            FateCosmicVFX.SpawnGlyphBurst(target.Center, 5, 5f, 0.3f);
            FateCosmicVFX.SpawnStarSparkles(target.Center, 4, 25f, 0.25f);
            
            // Reset to seeking for next attack
            attackPhase = 2; // Brief cooldown after hit
            phaseTimer = 0;
            
            SoundEngine.PlaySound(SoundID.Item14 with { Volume = 0.5f, Pitch = 0.4f }, target.Center);
        }

        public override void OnKill(int timeLeft)
        {
            // === SPECTACULAR DEATH EXPLOSION ===
            FateCosmicVFX.SpawnCosmicExplosion(Projectile.Center, 1.0f);
            FateCosmicVFX.SpawnGlyphBurst(Projectile.Center, 8, 6f, 0.4f);
            FateCosmicVFX.SpawnStarSparkles(Projectile.Center, 8, 35f, 0.35f);
            FateCosmicVFX.SpawnCosmicMusicNotes(Projectile.Center, 4, 30f, 0.35f);
            SoundEngine.PlaySound(SoundID.Item14 with { Volume = 0.8f, Pitch = 0.2f }, Projectile.Center);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            
            // Try to load the selected weapon texture
            Texture2D tex;
            try
            {
                tex = ModContent.Request<Texture2D>(currentTexturePath).Value;
            }
            catch
            {
                tex = ModContent.Request<Texture2D>(Texture).Value;
            }
            
            Vector2 origin = tex.Size() / 2f;
            
            // More intense pulsing during dash
            float dashIntensity = attackPhase == 1 ? 1.3f : 1f;
            float pulse = 1f + (float)Math.Sin(pulsePhase) * 0.15f * dashIntensity;
            float glowIntensity = (0.6f + (float)Math.Sin(pulsePhase * 0.8f) * 0.3f) * dashIntensity;

            // Trail with fading weapon copies - longer trail during dash
            int trailCount = attackPhase == 1 ? Projectile.oldPos.Length : Projectile.oldPos.Length / 2;
            for (int i = 0; i < trailCount; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;
                float progress = (float)i / Projectile.oldPos.Length;
                Color trailColor = FateCosmicVFX.GetCosmicGradient(progress) * (1f - progress) * 0.5f;
                Vector2 trailPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                float trailScale = (1f - progress * 0.4f) * 0.7f;
                spriteBatch.Draw(tex, trailPos, null, trailColor, Projectile.oldRot[i], origin, trailScale, SpriteEffects.None, 0f);
            }

            // === MULTI-LAYER BLOOM STACK FOR CELESTIAL PULSING GLOW ===
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            
            // Outermost cosmic nebula glow - purple
            spriteBatch.Draw(tex, drawPos, null, FateCosmicVFX.FatePurple * 0.15f * glowIntensity, Projectile.rotation, origin, 1.9f * pulse, SpriteEffects.None, 0f);
            
            // Outer bright red energy corona
            spriteBatch.Draw(tex, drawPos, null, FateCosmicVFX.FateBrightRed * 0.25f * glowIntensity, Projectile.rotation, origin, 1.5f * pulse, SpriteEffects.None, 0f);
            
            // Middle dark pink energy field
            spriteBatch.Draw(tex, drawPos, null, FateCosmicVFX.FateDarkPink * 0.4f * glowIntensity, Projectile.rotation, origin, 1.25f * pulse, SpriteEffects.None, 0f);
            
            // Inner white-hot celestial core
            spriteBatch.Draw(tex, drawPos, null, FateCosmicVFX.FateWhite * 0.3f * glowIntensity, Projectile.rotation, origin, 1.05f * pulse, SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            // Main weapon blade - slightly tinted with celestial white
            spriteBatch.Draw(tex, drawPos, null, Color.White * 0.95f, Projectile.rotation, origin, 0.85f * pulse, SpriteEffects.None, 0f);

            return false;
        }
    }

    #endregion

    #region Cinematic Star Circle - Player Attack Effect

    /// <summary>
    /// Player ModPlayer to track weapon usage and trigger star circle effect.
    /// Also spawns the massive figure-8 chromatic/pearlescent light effect when holding ANY Fate weapon.
    /// </summary>
    public class FateWeaponEffectPlayer : ModPlayer
    {
        private int attackCounter = 0;
        private const int AttacksForStarCircle = 6;
        
        // Figure-8 effect tracking
        private float figure8Phase = 0f;
        private bool wasHoldingFateWeapon = false;

        /// <summary>
        /// Call this when a Fate weapon attacks. Position parameter for potential future use.
        /// </summary>
        public void OnFateWeaponAttack(Vector2 attackPosition)
        {
            attackCounter++;

            if (attackCounter >= AttacksForStarCircle)
            {
                attackCounter = 0;
                FateCosmicVFX.TriggerStarCircleEffect(Player);
            }
        }
        
        public override void PostUpdate()
        {
            // Check if player is holding a Fate weapon (by rarity)
            bool isHoldingFateWeapon = IsHoldingFateWeapon();
            
            if (isHoldingFateWeapon)
            {
                // Spawn the massive figure-8 chromatic/pearlescent light effect
                SpawnFigure8Effect();
                wasHoldingFateWeapon = true;
            }
            else if (wasHoldingFateWeapon)
            {
                // Was holding but no longer - reset
                wasHoldingFateWeapon = false;
            }
        }
        
        private bool IsHoldingFateWeapon()
        {
            Item heldItem = Player.HeldItem;
            if (heldItem == null || heldItem.IsAir)
                return false;
            
            // Check if the item has FateRarity
            int fateRarityId = ModContent.RarityType<FateRarity>();
            return heldItem.rare == fateRarityId;
        }
        
        private void SpawnFigure8Effect()
        {
            // Advance figure-8 phase
            figure8Phase += 0.035f;
            if (figure8Phase > MathHelper.TwoPi * 4f)
                figure8Phase -= MathHelper.TwoPi * 4f;
            
            // === MASSIVE FIGURE-8 CHROMATIC/PEARLESCENT LIGHT EFFECT ===
            // The figure-8 is created using parametric equations
            // x = sin(t), y = sin(2t) creates a figure-8 (Lissajous curve)
            
            float time = figure8Phase;
            
            // Large figure-8 dimensions
            float xRadius = 100f; // Horizontal extent
            float yRadius = 60f;  // Vertical extent
            
            // === PRIMARY FIGURE-8 PATH - CHROMATIC RAINBOW ===
            // Spawn particles along the figure-8 path
            for (int i = 0; i < 3; i++)
            {
                float pathOffset = i * 0.3f;
                float t = time + pathOffset;
                
                // Figure-8 parametric: x = sin(t), y = sin(2t) / 2
                float x = (float)Math.Sin(t) * xRadius;
                float y = (float)Math.Sin(t * 2f) * yRadius * 0.5f;
                
                Vector2 particlePos = Player.Center + new Vector2(x, y);
                
                // Chromatic rainbow cycling through the path
                float hue = (t * 0.15f + Main.GameUpdateCount * 0.008f) % 1f;
                Color chromaticColor = Main.hslToRgb(hue, 1f, 0.7f);
                
                // Main chromatic glow
                var chromatic = new GenericGlowParticle(particlePos, 
                    new Vector2((float)Math.Cos(t) * 0.8f, (float)Math.Cos(t * 2f) * 0.5f),
                    chromaticColor * 0.7f, 0.35f, 25, true);
                MagnumParticleHandler.SpawnParticle(chromatic);
            }
            
            // === SECONDARY FIGURE-8 PATH - PEARLESCENT (offset and opposite direction) ===
            for (int i = 0; i < 2; i++)
            {
                float pathOffset = i * 0.4f + 0.15f;
                float t = -time * 0.8f + pathOffset; // Opposite direction, slightly slower
                
                float x = (float)Math.Sin(t) * (xRadius * 0.85f);
                float y = (float)Math.Sin(t * 2f) * (yRadius * 0.85f) * 0.5f;
                
                Vector2 particlePos = Player.Center + new Vector2(x, y);
                
                // Pearlescent white/pink/cyan cycling
                float pearlPhase = (t * 0.2f + Main.GameUpdateCount * 0.01f) % 1f;
                Color pearlColor;
                if (pearlPhase < 0.33f)
                    pearlColor = Color.Lerp(Color.White, new Color(255, 200, 220), pearlPhase * 3f); // White to pink
                else if (pearlPhase < 0.66f)
                    pearlColor = Color.Lerp(new Color(255, 200, 220), new Color(200, 230, 255), (pearlPhase - 0.33f) * 3f); // Pink to cyan
                else
                    pearlColor = Color.Lerp(new Color(200, 230, 255), Color.White, (pearlPhase - 0.66f) * 3f); // Cyan to white
                
                var pearl = new GenericGlowParticle(particlePos,
                    new Vector2((float)Math.Cos(t) * -0.6f, (float)Math.Cos(t * 2f) * -0.4f),
                    pearlColor * 0.5f, 0.28f, 22, true);
                MagnumParticleHandler.SpawnParticle(pearl);
            }
            
            // === SPARKLE ACCENTS AT FIGURE-8 CROSSOVER POINTS ===
            // The crossover point is at the center (0,0 of the figure-8)
            if (Main.rand.NextBool(3))
            {
                // Near the center crossover
                Vector2 crossoverPos = Player.Center + Main.rand.NextVector2Circular(15f, 15f);
                float sparkHue = Main.rand.NextFloat();
                Color sparkColor = Main.hslToRgb(sparkHue, 1f, 0.85f);
                
                CustomParticles.GenericFlare(crossoverPos, sparkColor, 0.3f, 15);
            }
            
            // === OUTER CHROMATIC HALO RING (pulsing) ===
            if (Main.GameUpdateCount % 8 == 0)
            {
                float haloHue = (Main.GameUpdateCount * 0.02f) % 1f;
                Color haloColor = Main.hslToRgb(haloHue, 0.8f, 0.6f);
                CustomParticles.HaloRing(Player.Center, haloColor * 0.4f, 0.6f, 18);
            }
            
            // === PEARLESCENT STAR SPARKLES ===
            if (Main.rand.NextBool(4))
            {
                // Random position along the figure-8 path
                float randT = Main.rand.NextFloat(MathHelper.TwoPi * 2f);
                float randX = (float)Math.Sin(randT) * xRadius * Main.rand.NextFloat(0.8f, 1.1f);
                float randY = (float)Math.Sin(randT * 2f) * yRadius * 0.5f * Main.rand.NextFloat(0.8f, 1.1f);
                Vector2 starPos = Player.Center + new Vector2(randX, randY);
                
                Color starColor = Main.rand.NextBool() ? Color.White : new Color(255, 240, 250);
                var star = new GenericGlowParticle(starPos, Main.rand.NextVector2Circular(0.5f, 0.5f),
                    starColor, 0.22f, 20, true);
                MagnumParticleHandler.SpawnParticle(star);
            }
            
            // === GLYPHS ORBITING THE FIGURE-8 ===
            if (Main.GameUpdateCount % 12 == 0)
            {
                float glyphT = time * 0.5f;
                float glyphX = (float)Math.Sin(glyphT) * xRadius * 1.1f;
                float glyphY = (float)Math.Sin(glyphT * 2f) * yRadius * 0.55f;
                Vector2 glyphPos = Player.Center + new Vector2(glyphX, glyphY);
                
                CustomParticles.Glyph(glyphPos, FateCosmicVFX.FateDarkPink * 0.6f, 0.35f, -1);
            }
            
            // === COSMIC DUST MOTES DRIFTING ===
            if (Main.rand.NextBool(6))
            {
                Vector2 dustPos = Player.Center + Main.rand.NextVector2Circular(xRadius * 1.2f, yRadius * 0.8f);
                float dustHue = Main.rand.NextFloat();
                Color dustColor = Main.hslToRgb(dustHue, 0.6f, 0.5f);
                
                var dust = new GenericGlowParticle(dustPos, new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), -0.3f),
                    dustColor * 0.35f, 0.12f, 30, true);
                MagnumParticleHandler.SpawnParticle(dust);
            }
            
            // === LENS FLARE EFFECT - DRAMATIC COSMIC RAYS ===
            // Spawns periodically at the weapon/player position with radiant streaks
            if (Main.GameUpdateCount % 6 == 0)
            {
                // Main lens flare center - bright white core
                float flarePulse = (float)Math.Sin(Main.GameUpdateCount * 0.12f) * 0.3f + 0.7f;
                CustomParticles.GenericFlare(Player.Center, Color.White * flarePulse, 0.45f * flarePulse, 12);
                
                // Chromatic lens flare rays - 6-point star pattern
                for (int ray = 0; ray < 6; ray++)
                {
                    float rayAngle = MathHelper.TwoPi * ray / 6f + Main.GameUpdateCount * 0.015f;
                    float rayLength = 45f + (float)Math.Sin(Main.GameUpdateCount * 0.1f + ray) * 15f;
                    Vector2 rayEnd = Player.Center + rayAngle.ToRotationVector2() * rayLength;
                    
                    // Chromatic color for each ray - shifted hue
                    float rayHue = (ray / 6f + Main.GameUpdateCount * 0.005f) % 1f;
                    Color rayColor = Main.hslToRgb(rayHue, 0.9f, 0.75f);
                    
                    // Ray flare at end point
                    CustomParticles.GenericFlare(rayEnd, rayColor * 0.6f, 0.25f, 10);
                    
                    // Streak particle along the ray
                    Vector2 streakPos = Vector2.Lerp(Player.Center, rayEnd, 0.5f + Main.rand.NextFloat(0.3f));
                    var streak = new GenericGlowParticle(streakPos, rayAngle.ToRotationVector2() * 2f,
                        rayColor * 0.5f, 0.18f, 15, true);
                    MagnumParticleHandler.SpawnParticle(streak);
                }
            }
            
            // === SECONDARY LENS FLARE ARTIFACTS - Pearlescent hexagonal bokeh ===
            if (Main.GameUpdateCount % 10 == 0)
            {
                // Hexagonal bokeh artifacts at random positions (like camera lens artifacts)
                for (int i = 0; i < 3; i++)
                {
                    // Position bokeh along a line from player toward a random direction
                    float bokehAngle = Main.rand.NextFloat(MathHelper.TwoPi);
                    float bokehDist = 30f + Main.rand.NextFloat(60f);
                    Vector2 bokehPos = Player.Center + bokehAngle.ToRotationVector2() * bokehDist;
                    
                    // Pearlescent color cycling
                    float bokehPhase = (Main.GameUpdateCount * 0.02f + i * 0.33f) % 1f;
                    Color bokehColor;
                    if (bokehPhase < 0.33f)
                        bokehColor = Color.Lerp(new Color(255, 220, 240), Color.White, bokehPhase * 3f);
                    else if (bokehPhase < 0.66f)
                        bokehColor = Color.Lerp(Color.White, new Color(220, 240, 255), (bokehPhase - 0.33f) * 3f);
                    else
                        bokehColor = Color.Lerp(new Color(220, 240, 255), new Color(255, 220, 240), (bokehPhase - 0.66f) * 3f);
                    
                    CustomParticles.GenericFlare(bokehPos, bokehColor * 0.4f, 0.2f + Main.rand.NextFloat(0.15f), 15);
                }
            }
            
            // === INFINITY SPARKLE LINES - SCREEN-SPANNING EFFECT ===
            // Dramatic sparkle lines that extend across the screen in an infinity pattern
            if (Main.GameUpdateCount % 3 == 0)
            {
                float infinityPhase = Main.GameUpdateCount * 0.025f;
                
                // Screen dimensions for sparkle line extent
                float lineExtent = Main.screenWidth * 0.6f; // Lines extend well across screen
                
                // Multiple sparkle points along each infinity arm
                for (int arm = 0; arm < 2; arm++)
                {
                    float armOffset = arm * MathHelper.Pi;
                    
                    // Create sparkle points along extending lines
                    for (int point = 0; point < 8; point++)
                    {
                        float pointProgress = (float)point / 8f;
                        float extensionDist = pointProgress * lineExtent;
                        
                        // Infinity pattern angle calculation
                        // Creates sweeping lines that curve in figure-8 pattern
                        float curveAngle = infinityPhase + armOffset;
                        float curveFactor = (float)Math.Sin(curveAngle * 2f) * 0.3f;
                        float lineAngle = curveAngle + curveFactor * pointProgress;
                        
                        Vector2 sparklePos = Player.Center + lineAngle.ToRotationVector2() * extensionDist;
                        
                        // Don't spawn if off-screen
                        if (sparklePos.X < Main.screenPosition.X - 50 || sparklePos.X > Main.screenPosition.X + Main.screenWidth + 50 ||
                            sparklePos.Y < Main.screenPosition.Y - 50 || sparklePos.Y > Main.screenPosition.Y + Main.screenHeight + 50)
                            continue;
                        
                        // Fade based on distance
                        float distanceFade = 1f - pointProgress * 0.7f;
                        
                        // Chromatic color cycling along the line
                        float sparkHue = (infinityPhase * 0.3f + pointProgress * 0.5f + arm * 0.5f) % 1f;
                        Color sparkColor = Main.hslToRgb(sparkHue, 1f, 0.8f);
                        
                        // Main sparkle
                        float sparkScale = 0.2f + (1f - pointProgress) * 0.25f;
                        CustomParticles.GenericFlare(sparklePos, sparkColor * distanceFade * 0.8f, sparkScale, 8);
                        
                        // Trailing glow particle
                        if (point < 6 && Main.rand.NextBool(2))
                        {
                            Vector2 trailVel = lineAngle.ToRotationVector2() * (2f + pointProgress * 3f);
                            var trail = new GenericGlowParticle(sparklePos, trailVel * 0.3f,
                                sparkColor * distanceFade * 0.5f, sparkScale * 0.7f, 15, true);
                            MagnumParticleHandler.SpawnParticle(trail);
                        }
                    }
                }
                
                // === CENTRAL INFINITY CROSSOVER BURST ===
                // Extra sparkles at the center where the infinity lines cross
                float crossBurst = (float)Math.Sin(infinityPhase * 2f) * 0.5f + 0.5f;
                if (crossBurst > 0.8f)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        float burstAngle = MathHelper.TwoPi * i / 4f + infinityPhase;
                        Vector2 burstPos = Player.Center + burstAngle.ToRotationVector2() * 20f;
                        
                        float burstHue = (infinityPhase * 0.5f + i * 0.25f) % 1f;
                        Color burstColor = Main.hslToRgb(burstHue, 1f, 0.9f);
                        
                        CustomParticles.GenericFlare(burstPos, burstColor * 0.7f, 0.35f, 12);
                    }
                }
            }
            
            // === ANAMORPHIC LENS STREAK - Horizontal light streak through center ===
            if (Main.GameUpdateCount % 15 == 0)
            {
                float streakLength = 80f + (float)Math.Sin(Main.GameUpdateCount * 0.05f) * 30f;
                
                // Horizontal streak particles
                for (int s = -3; s <= 3; s++)
                {
                    if (s == 0) continue; // Skip center (already has main flare)
                    
                    float streakX = s * (streakLength / 3f);
                    Vector2 streakPos = Player.Center + new Vector2(streakX, 0);
                    
                    // Fade based on distance from center
                    float distFade = 1f - Math.Abs(s) / 4f;
                    
                    // Chromatic aberration - slightly different colors on each side
                    Color streakColor = s < 0 
                        ? Color.Lerp(new Color(255, 200, 220), Color.White, distFade) // Pink on left
                        : Color.Lerp(new Color(200, 220, 255), Color.White, distFade); // Cyan on right
                    
                    CustomParticles.GenericFlare(streakPos, streakColor * distFade * 0.5f, 0.15f, 12);
                }
            }
            
            // === FIGURE-8 LIGHTING ===
            // Multiple light sources along the figure-8
            for (int i = 0; i < 4; i++)
            {
                float lightT = time + MathHelper.PiOver2 * i;
                float lightX = (float)Math.Sin(lightT) * xRadius * 0.6f;
                float lightY = (float)Math.Sin(lightT * 2f) * yRadius * 0.3f;
                Vector2 lightPos = Player.Center + new Vector2(lightX, lightY);
                
                float lightHue = (lightT * 0.1f) % 1f;
                Color lightColor = Main.hslToRgb(lightHue, 0.7f, 0.5f);
                Lighting.AddLight(lightPos, lightColor.ToVector3() * 0.35f);
            }
            
            // Central cosmic glow
            float centralPulse = (float)Math.Sin(Main.GameUpdateCount * 0.08f) * 0.2f + 0.8f;
            Lighting.AddLight(Player.Center, FateCosmicVFX.FatePurple.ToVector3() * centralPulse * 0.5f);
        }

        public override void ResetEffects()
        {
            // Reset counter if not attacking for a while
            if (Player.itemAnimation == 0)
            {
                // Decay counter slowly
                if (Main.GameUpdateCount % 60 == 0 && attackCounter > 0)
                {
                    attackCounter--;
                }
            }
        }
    }

    #endregion
}
