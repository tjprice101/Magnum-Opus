using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Terraria.Audio;
using MagnumOpus.Common;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Content.EnigmaVariations.Debuffs;

namespace MagnumOpus.Content.EnigmaVariations.ResonantWeapons
{
    /// <summary>
    /// DISSONANCE OF SECRETS - Enigma Magic Staff
    /// ===========================================
    /// UNIQUE MECHANICS:
    /// - Fire a slow-moving MYSTERY ORB that grows as it travels
    /// - Orb periodically releases smaller homing projectiles
    /// - Orb has orbiting EYES watching outward + rotating glyph circle
    /// - Enemies near the orb take escalating damage per second (aura damage)
    /// - On death/max size: MASSIVE glyph circle explosion
    /// - Explosion applies heavy Paradox Brand stacks to all nearby enemies
    /// - Can have multiple orbs active at once for overlapping auras
    /// </summary>
    public class DissonanceOfSecrets : ModItem
    {
        private static readonly Color EnigmaBlack = new Color(15, 10, 20);
        private static readonly Color EnigmaPurple = new Color(140, 60, 200);
        private static readonly Color EnigmaGreen = new Color(50, 220, 100);
        
        private Color GetEnigmaGradient(float progress)
        {
            if (progress < 0.5f)
                return Color.Lerp(EnigmaBlack, EnigmaPurple, progress * 2f);
            else
                return Color.Lerp(EnigmaPurple, EnigmaGreen, (progress - 0.5f) * 2f);
        }
        
        public override void SetDefaults()
        {
            Item.damage = 480;
            Item.DamageType = DamageClass.Magic;
            Item.mana = 18;
            Item.width = 28;
            Item.height = 32;
            Item.useTime = 35;
            Item.useAnimation = 35;
            Item.useStyle = ItemUseStyleID.Shoot; // Normal book-style shooting
            Item.knockBack = 3f;
            Item.value = Item.sellPrice(gold: 18);
            Item.rare = ModContent.RarityType<EnigmaRarity>();
            Item.UseSound = SoundID.Item117;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<RiddleCascadeOrb>();
            Item.shootSpeed = 6f;
            Item.noMelee = true;
            Item.scale = 0.765f; // 25% smaller total
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "EnigmaEffect1", "Fire a growing Mystery Orb that damages enemies in range"));
            tooltips.Add(new TooltipLine(Mod, "EnigmaEffect2", "Orb releases homing riddlebolts and has orbiting eyes"));
            tooltips.Add(new TooltipLine(Mod, "EnigmaEffect3", "Explodes with a massive glyph circle on death"));
            tooltips.Add(new TooltipLine(Mod, "EnigmaLore", "'The answer lies within the cascade of riddles'") 
            { 
                OverrideColor = EnigmaPurple 
            });
        }
        
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // Spawn orb
            Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
            
            // Cast VFX
            Vector2 castPos = position + velocity.SafeNormalize(Vector2.Zero) * 30f;
            
            // Glyph circle at cast point
            CustomParticles.GlyphCircle(castPos, EnigmaPurple, count: 6, radius: 35f, rotationSpeed: 0.08f);
            
            // Fractal burst
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 offset = angle.ToRotationVector2() * 30f;
                CustomParticles.GenericFlare(castPos + offset, GetEnigmaGradient((float)i / 8f), 0.5f, 18);
            }
            
            CustomParticles.GenericFlare(castPos, EnigmaGreen, 0.75f, 20);
            CustomParticles.HaloRing(castPos, EnigmaPurple, 0.45f, 16);
            
            // Spiraling sparkle burst on cast - riddles take flight
            for (int i = 0; i < 8; i++)
            {
                float spiralAngle = MathHelper.TwoPi * i / 8f + Main.GameUpdateCount * 0.08f;
                Vector2 spiralVel = spiralAngle.ToRotationVector2() * (3f + i * 0.5f);
                var sparkle = new GenericGlowParticle(castPos, spiralVel, 
                    GetEnigmaGradient((float)i / 8f), 0.4f, 20, true);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
            
            // Music notes spiral outward - the riddle's melody begins
            ThemedParticles.EnigmaMusicNoteBurst(castPos, 8, 4f);
            
            return false;
        }
    }
    
    /// <summary>
    /// Main slow-moving orb that grows and releases homing projectiles
    /// </summary>
    public class RiddleCascadeOrb : ModProjectile
    {
        private static readonly Color EnigmaBlack = new Color(15, 10, 20);
        private static readonly Color EnigmaDeepPurple = new Color(80, 20, 120);
        private static readonly Color EnigmaPurple = new Color(140, 60, 200);
        private static readonly Color EnigmaGreenFlame = new Color(50, 220, 100);
        private static readonly Color EnigmaGreen = new Color(50, 220, 100);
        
        private float currentScale = 0.5f;
        private const float MaxScale = 2.5f;
        private const float GrowthRate = 0.008f;
        private const float AuraRadius = 180f;
        private int auraDamageTimer = 0;
        private int riddleboltTimer = 0;
        
        public override string Texture => "MagnumOpus/Assets/Particles/SoftGlow";
        
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            
            float pulse = 0.8f + (float)Math.Sin(Main.GameUpdateCount * 0.1f) * 0.2f;
            float scale = currentScale * pulse;
            
            // Switch to additive blending for ethereal mystery orb
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, 
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            Texture2D eyeTex = CustomParticleSystem.RandomEnigmaEye().Value;
            Texture2D glyphTex = CustomParticleSystem.RandomGlyph().Value;
            Texture2D sparkleTex = CustomParticleSystem.RandomPrismaticSparkle().Value;
            
            // === DRAW ORBITING GLYPHS (scale with orb growth) ===
            int glyphCount = 6 + (int)(currentScale * 2);
            for (int i = 0; i < glyphCount; i++)
            {
                float orbitAngle = Main.GameUpdateCount * 0.06f + MathHelper.TwoPi * i / glyphCount;
                float orbitRadius = 25f * scale + (float)Math.Sin(Main.GameUpdateCount * 0.08f + i) * 8f;
                Vector2 glyphPos = drawPos + orbitAngle.ToRotationVector2() * orbitRadius;
                Color glyphColor = Color.Lerp(EnigmaPurple, EnigmaGreen, (float)i / glyphCount) * 0.7f;
                spriteBatch.Draw(glyphTex, glyphPos, null, glyphColor, orbitAngle * 2f, glyphTex.Size() / 2f, 0.2f * scale * 0.5f, SpriteEffects.None, 0f);
            }
            
            // === DRAW WATCHING EYES around the orb ===
            for (int i = 0; i < 3; i++)
            {
                float eyeAngle = Main.GameUpdateCount * 0.03f + MathHelper.TwoPi * i / 3f;
                float eyeRadius = 18f * scale;
                Vector2 eyePos = drawPos + eyeAngle.ToRotationVector2() * eyeRadius;
                float lookAngle = (drawPos - eyePos).ToRotation(); // Look inward
                spriteBatch.Draw(eyeTex, eyePos, null, EnigmaGreen * 0.65f, lookAngle, eyeTex.Size() / 2f, 0.25f * scale * 0.6f, SpriteEffects.None, 0f);
            }
            
            // === DRAW CENTRAL ROTATING GLYPH CORE ===
            float glyphRotation = Main.GameUpdateCount * 0.04f;
            spriteBatch.Draw(glyphTex, drawPos, null, EnigmaPurple * 0.85f, glyphRotation, glyphTex.Size() / 2f, scale * 0.5f, SpriteEffects.None, 0f);
            spriteBatch.Draw(glyphTex, drawPos, null, EnigmaGreenFlame * 0.6f, -glyphRotation * 0.7f, glyphTex.Size() / 2f, scale * 0.3f, SpriteEffects.None, 0f);
            
            // === DRAW MYSTERY CORE SPARKLE ===
            spriteBatch.Draw(sparkleTex, drawPos, null, EnigmaGreenFlame * 0.75f, Main.GameUpdateCount * 0.05f, sparkleTex.Size() / 2f, scale * 0.35f, SpriteEffects.None, 0f);
            
            // Restore normal blending
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, 
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            return false;
        }
        
        private Color GetEnigmaGradient(float progress)
        {
            if (progress < 0.5f)
                return Color.Lerp(EnigmaBlack, EnigmaPurple, progress * 2f);
            else
                return Color.Lerp(EnigmaPurple, EnigmaGreen, (progress - 0.5f) * 2f);
        }
        
        public override void SetDefaults()
        {
            Projectile.width = 40;
            Projectile.height = 40;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 420; // 7 seconds
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 30;
        }
        
        public override void AI()
        {
            // Grow the orb
            if (currentScale < MaxScale)
            {
                currentScale += GrowthRate;
                Projectile.scale = currentScale;
            }
            else
            {
                // At max size, explode early
                Projectile.Kill();
                return;
            }
            
            float growthProgress = currentScale / MaxScale;
            
            // Slow down as it grows
            if (Projectile.velocity.Length() > 1.5f)
            {
                Projectile.velocity *= 0.985f;
            }
            
            // Aura damage to nearby enemies
            auraDamageTimer++;
            if (auraDamageTimer >= 15)
            {
                auraDamageTimer = 0;
                DealAuraDamage();
            }
            
            // Release homing riddlebolts
            riddleboltTimer++;
            int riddleboltInterval = (int)MathHelper.Lerp(45, 25, growthProgress);
            if (riddleboltTimer >= riddleboltInterval)
            {
                riddleboltTimer = 0;
                ReleaseRiddlebolt();
            }
            
            // OPTIMIZED: ORBITING SPARKLE CONSTELLATION - reduced frequency from 8 to 15 frames, fewer particles
            int sparkleCount = 2 + (int)(growthProgress * 2);
            if (Main.GameUpdateCount % 15 == 0)
            {
                float baseAngle = Main.GameUpdateCount * 0.03f;
                for (int i = 0; i < sparkleCount; i++)
                {
                    float angle = baseAngle + MathHelper.TwoPi * i / sparkleCount;
                    float radius = 45f * currentScale;
                    Vector2 sparklePos = Projectile.Center + angle.ToRotationVector2() * radius;
                    CustomParticles.GenericFlare(sparklePos, GetEnigmaGradient(growthProgress), 0.4f * currentScale, 16);
                }
            }
            
            // OPTIMIZED: Rotating glyph circle - reduced frequency from 12 to 25 frames, fewer glyphs
            if (Main.GameUpdateCount % 25 == 0)
            {
                int glyphCount = 3 + (int)(growthProgress * 2);
                CustomParticles.GlyphCircle(Projectile.Center, EnigmaPurple, count: glyphCount, 
                    radius: 35f * currentScale, rotationSpeed: 0.04f);
            }
            
            // OPTIMIZED: Core pulsing - reduced from 5 to 12 frames
            if (Main.GameUpdateCount % 12 == 0)
            {
                float pulse = 0.55f + (float)Math.Sin(Main.GameUpdateCount * 0.12f) * 0.15f;
                CustomParticles.GenericFlare(Projectile.Center, GetEnigmaGradient(growthProgress), 
                    pulse * currentScale * 0.55f, 14);
            }
            
            // OPTIMIZED: Swirling particles - reduced from NextBool(3) to every 12 frames
            if (Main.GameUpdateCount % 12 == 0)
            {
                float angle = Main.rand.NextFloat() * MathHelper.TwoPi;
                float radius = 60f * currentScale + Main.rand.NextFloat(30f);
                Vector2 particlePos = Projectile.Center + angle.ToRotationVector2() * radius;
                Vector2 vel = (Projectile.Center - particlePos).SafeNormalize(Vector2.Zero) * 3f;
                
                var glow = new GenericGlowParticle(particlePos, vel, GetEnigmaGradient(Main.rand.NextFloat()) * 0.65f, 
                    0.28f * currentScale, 22, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }
            
            // OPTIMIZED: Ambient trail - removed (was every frame with NextBool(2))
            
            Lighting.AddLight(Projectile.Center, EnigmaPurple.ToVector3() * 0.6f * currentScale);
        }
        
        private void DealAuraDamage()
        {
            float auraRadius = AuraRadius * currentScale;
            int auraDamage = (int)(Projectile.damage * 0.15f * currentScale);
            
            foreach (NPC npc in Main.ActiveNPCs)
            {
                if (npc.friendly) continue;
                
                float dist = Vector2.Distance(npc.Center, Projectile.Center);
                if (dist <= auraRadius)
                {
                    // Damage scales with proximity
                    float proximityMult = 1f + (1f - dist / auraRadius) * 0.5f;
                    npc.SimpleStrikeNPC((int)(auraDamage * proximityMult), 0, false, 1f);
                    npc.AddBuff(ModContent.BuffType<ParadoxBrand>(), 120);
                    npc.GetGlobalNPC<ParadoxBrandNPC>().AddParadoxStack(npc, 1);
                    
                    // Aura damage indicator
                    CustomParticles.GenericFlare(npc.Center, EnigmaPurple * 0.5f, 0.25f, 8);
                    
                    // Draw line from orb to affected enemy occasionally
                    if (Main.rand.NextBool(4))
                    {
                        MagnumVFX.DrawFractalLightning(Projectile.Center, npc.Center, EnigmaGreen * 0.4f, 
                            6, 15f, 2, 0.2f);
                    }
                }
            }
        }
        
        private void ReleaseRiddlebolt()
        {
            // Find target
            NPC target = FindClosestEnemy(400f);
            
            if (target != null)
            {
                SoundEngine.PlaySound(SoundID.Item8 with { Pitch = 0.4f, Volume = 0.5f }, Projectile.Center);
                
                Vector2 direction = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX);
                Vector2 spawnPos = Projectile.Center + direction * 30f * currentScale;
                
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), spawnPos, direction * 10f,
                    ModContent.ProjectileType<Riddlebolt>(), Projectile.damage / 2, Projectile.knockBack * 0.3f, Projectile.owner);
                
                // Release VFX
                CustomParticles.GenericFlare(spawnPos, EnigmaGreen, 0.5f, 14);
                CustomParticles.HaloRing(spawnPos, EnigmaPurple * 0.6f, 0.25f, 10);
                
                // Sparkle targeting line toward target
                for (int sparkle = 0; sparkle < 3; sparkle++)
                {
                    Vector2 sparklePos = Vector2.Lerp(spawnPos, target.Center, (sparkle + 1) * 0.15f);
                    CustomParticles.GenericFlare(sparklePos, GetEnigmaGradient((float)sparkle / 3f) * 0.6f, 0.25f, 10);
                }
            }
        }
        
        private NPC FindClosestEnemy(float range)
        {
            NPC closest = null;
            float closestDist = range;
            
            foreach (NPC npc in Main.ActiveNPCs)
            {
                if (npc.friendly) continue;
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
            target.AddBuff(ModContent.BuffType<ParadoxBrand>(), 300);
            target.GetGlobalNPC<ParadoxBrandNPC>().AddParadoxStack(target, 2);
            
            // === REALITY WARP DISTORTION ===
            FateRealityDistortion.TriggerChromaticAberration(target.Center, 3f, 10);
            
            // === NEW UNIFIED VFX HIT EFFECT ===
            UnifiedVFX.EnigmaVariations.HitEffect(target.Center, 0.9f * currentScale);
            
            // === WATCHING EYE AT IMPACT ===
            CustomParticles.EnigmaEyeImpact(target.Center, target.Center, EnigmaGreen, 0.5f);
            
            // === MUSIC NOTES BURST ===
            ThemedParticles.EnigmaMusicNoteBurst(target.Center, 10, 6f);
            ThemedParticles.EnigmaMusicNotes(target.Center, 5, 35f);
            
            // Radiant sparkle crown above target
            for (int crown = 0; crown < 5; crown++)
            {
                float crownAngle = MathHelper.TwoPi * crown / 5f - MathHelper.PiOver2;
                Vector2 crownPos = target.Center - new Vector2(0, 30f) + crownAngle.ToRotationVector2() * 18f;
                CustomParticles.GenericFlare(crownPos, GetEnigmaGradient((float)crown / 5f), 0.4f, 16);
            }
            
            // === GLYPH CIRCLE FORMATION ===
            CustomParticles.GlyphCircle(target.Center, EnigmaPurple, count: 6, radius: 45f, rotationSpeed: 0.06f);
            
            int stacks = target.GetGlobalNPC<ParadoxBrandNPC>().paradoxStacks;
            if (stacks > 0)
            {
                CustomParticles.GlyphStack(target.Center + new Vector2(0, -25f), EnigmaGreen, stacks, 0.28f);
            }
            
            // === DYNAMIC LIGHTING ===
            Lighting.AddLight(target.Center, EnigmaGreen.ToVector3() * 0.8f);
        }
        
        public override void OnKill(int timeLeft)
        {
            // === MASSIVE REALITY CASCADE ===
            FateRealityDistortion.TriggerChromaticAberration(Projectile.Center, 5f, 18);
            FateRealityDistortion.TriggerInversionPulse(6);
            
            TriggerCascadeExplosion();
        }
        
        private void TriggerCascadeExplosion()
        {
            SoundEngine.PlaySound(SoundID.Item122 with { Pitch = -0.2f, Volume = 1.1f }, Projectile.Center);
            SoundEngine.PlaySound(SoundID.Item14 with { Pitch = -0.4f, Volume = 0.9f }, Projectile.Center);
            
            float explosionRadius = 170f * currentScale; // Reduced by 20% (was 212f)
            
            // Central themed flash
            CustomParticles.GenericFlare(Projectile.Center, EnigmaGreen, 1.5f * currentScale, 30);
            
            // Multiple glyph circles at different radii - signature cascade effect
            for (int circle = 0; circle < 4; circle++)
            {
                float radius = 40f + circle * 50f;
                int glyphCount = 6 + circle * 2;
                float rotSpeed = (circle % 2 == 0 ? 1f : -1f) * (0.05f - circle * 0.01f);
                CustomParticles.GlyphCircle(Projectile.Center, GetEnigmaGradient((float)circle / 4f), 
                    count: glyphCount, radius: radius * currentScale, rotationSpeed: rotSpeed);
            }
            
            // Glyph tower at center
            CustomParticles.GlyphTower(Projectile.Center, EnigmaPurple, layers: 5, baseScale: 0.6f * currentScale);
            
            // Massive glyph burst
            CustomParticles.GlyphBurst(Projectile.Center, EnigmaGreen, count: 16, speed: 8f);
            // Massive fractal explosion
            for (int layer = 0; layer < 4; layer++)
            {
                int points = 10 + layer * 4;
                float radius = 50f + layer * 45f;
                
                for (int i = 0; i < points; i++)
                {
                    float angle = MathHelper.TwoPi * i / points + layer * 0.2f;
                    Vector2 offset = angle.ToRotationVector2() * radius * currentScale;
                    CustomParticles.GenericFlare(Projectile.Center + offset, GetEnigmaGradient((float)i / points), 
                        0.75f * currentScale - layer * 0.1f, 25);
                }
            }
            
            // Particle explosion outward
            for (int i = 0; i < 30; i++)
            {
                float angle = MathHelper.TwoPi * i / 30f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(8f, 16f);
                var glow = new GenericGlowParticle(Projectile.Center, vel, GetEnigmaGradient((float)i / 30f),
                    0.5f * currentScale, Main.rand.Next(30, 50), true);
                MagnumParticleHandler.SpawnParticle(glow);
            }
            
            // === WATCHING EYES burst from explosion - the mystery gazes upon all ===
            CustomParticles.EnigmaEyeExplosion(Projectile.Center, EnigmaPurple, 8, 5f);
            CustomParticles.EnigmaEyeFormation(Projectile.Center, EnigmaGreen, 6, 80f * currentScale);
            
            // Grand sparkle formation - the cascade completes
            for (int ring = 0; ring < 3; ring++)
            {
                int pointsInRing = 8 + ring * 4;
                float ringRadius = (100f + ring * 40f) * currentScale;
                for (int i = 0; i < pointsInRing; i++)
                {
                    float starAngle = MathHelper.TwoPi * i / pointsInRing + ring * 0.2f;
                    Vector2 starVel = starAngle.ToRotationVector2() * (4f + ring * 1.5f);
                    var star = new GenericGlowParticle(Projectile.Center, starVel, 
                        GetEnigmaGradient((ring * pointsInRing + i) / (float)(3 * pointsInRing)), 0.5f - ring * 0.1f, 25, true);
                    MagnumParticleHandler.SpawnParticle(star);
                }
            }
            
            // Music notes explode outward - the riddle's answer rings out
            ThemedParticles.EnigmaMusicNoteBurst(Projectile.Center, 12, 6f);
            ThemedParticles.EnigmaMusicNotes(Projectile.Center, 8, 80f * currentScale);
            
            // Damage and debuff enemies in radius
            foreach (NPC npc in Main.ActiveNPCs)
            {
                if (npc.friendly) continue;
                
                float dist = Vector2.Distance(npc.Center, Projectile.Center);
                if (dist <= explosionRadius)
                {
                    float falloff = 1f - (dist / explosionRadius) * 0.4f;
                    int explosionDamage = (int)(Projectile.damage * 3f * currentScale * falloff);
                    npc.SimpleStrikeNPC(explosionDamage, 0, true, 12f);
                    
                    int stacksToApply = dist < explosionRadius * 0.3f ? 4 : (dist < explosionRadius * 0.6f ? 3 : 2);
                    npc.AddBuff(ModContent.BuffType<ParadoxBrand>(), 360);
                    npc.GetGlobalNPC<ParadoxBrandNPC>().AddParadoxStack(npc, stacksToApply);
                    
                    // Chain lightning to each enemy
                    MagnumVFX.DrawFractalLightning(Projectile.Center, npc.Center, EnigmaGreen, 12, 35f, 4, 0.4f);
                    
                    // Eye at struck enemy
                    // Sparkle burst at aura-damaged enemy
                    CustomParticles.GenericFlare(npc.Center, EnigmaGreen, 0.45f, 14);
                    CustomParticles.HaloRing(npc.Center, EnigmaPurple * 0.5f, 0.25f, 10);
                    
                    // Glyph circle at enemy
                    CustomParticles.GlyphCircle(npc.Center, EnigmaGreen, count: 5, radius: 35f, rotationSpeed: 0.06f);
                }
            }
        }
    }
    
    /// <summary>
    /// Small homing projectile released by the main orb
    /// </summary>
    public class Riddlebolt : ModProjectile
    {
        private static readonly Color EnigmaBlack = new Color(15, 10, 20);
        private static readonly Color EnigmaDeepPurple = new Color(80, 20, 120);
        private static readonly Color EnigmaPurple = new Color(140, 60, 200);
        private static readonly Color EnigmaGreenFlame = new Color(50, 220, 100);
        private static readonly Color EnigmaGreen = new Color(50, 220, 100);
        
        private const float HomingStrength = 0.15f;
        private const float MaxSpeed = 14f;
        
        public override string Texture => "MagnumOpus/Assets/Particles/SoftGlow";
        
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            
            // Switch to additive blending
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, 
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            Texture2D sparkleTex = CustomParticleSystem.RandomPrismaticSparkle().Value;
            Texture2D glyphTex = CustomParticleSystem.RandomGlyph().Value;
            
            // === DRAW SPARKLE TRAIL ===
            if (ProjectileID.Sets.TrailCacheLength[Projectile.type] > 0)
            {
                for (int i = 0; i < Projectile.oldPos.Length; i++)
                {
                    if (Projectile.oldPos[i] == Vector2.Zero) continue;
                    float trailProgress = (float)i / Projectile.oldPos.Length;
                    float trailAlpha = (1f - trailProgress) * 0.6f;
                    float trailScale = (1f - trailProgress * 0.4f) * 0.2f;
                    Vector2 trailPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                    
                    Color trailColor = Color.Lerp(EnigmaGreenFlame, EnigmaPurple, trailProgress);
                    spriteBatch.Draw(sparkleTex, trailPos, null, trailColor * trailAlpha, Projectile.rotation + i * 0.3f, 
                        sparkleTex.Size() / 2f, trailScale, SpriteEffects.None, 0f);
                }
            }
            
            // === DRAW ORBITING MINI GLYPHS ===
            for (int i = 0; i < 4; i++)
            {
                float orbitAngle = Main.GameUpdateCount * 0.15f + MathHelper.TwoPi * i / 4f;
                float orbitRadius = 8f + (float)Math.Sin(Main.GameUpdateCount * 0.1f + i) * 3f;
                Vector2 orbitPos = drawPos + orbitAngle.ToRotationVector2() * orbitRadius;
                Color orbitColor = Color.Lerp(EnigmaPurple, EnigmaGreen, (float)i / 4f) * 0.65f;
                spriteBatch.Draw(glyphTex, orbitPos, null, orbitColor, orbitAngle, glyphTex.Size() / 2f, 0.12f, SpriteEffects.None, 0f);
            }
            
            // === DRAW CENTRAL SPARKLE CORE ===
            spriteBatch.Draw(sparkleTex, drawPos, null, EnigmaGreenFlame * 0.85f, Projectile.rotation, sparkleTex.Size() / 2f, 0.25f, SpriteEffects.None, 0f);
            spriteBatch.Draw(sparkleTex, drawPos, null, EnigmaPurple * 0.5f, -Projectile.rotation, sparkleTex.Size() / 2f, 0.15f, SpriteEffects.None, 0f);
            
            // Restore normal blending
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, 
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            return false;
        }
        
        private Color GetEnigmaGradient(float progress)
        {
            if (progress < 0.5f)
                return Color.Lerp(EnigmaBlack, EnigmaPurple, progress * 2f);
            else
                return Color.Lerp(EnigmaPurple, EnigmaGreen, (progress - 0.5f) * 2f);
        }
        
        public override void SetDefaults()
        {
            Projectile.width = 12;
            Projectile.height = 12;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 180;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.extraUpdates = 1;
        }
        
        public override void AI()
        {
            Projectile.rotation += 0.2f;
            
            // Home toward nearest enemy
            NPC target = FindClosestEnemy(400f);
            if (target != null)
            {
                Vector2 desiredVel = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero) * MaxSpeed;
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, desiredVel, HomingStrength);
            }
            
            // Trail - every 5 frames instead of 50% every frame
            if (Projectile.timeLeft % 5 == 0)
            {
                float progress = Main.rand.NextFloat();
                Color trailColor = GetEnigmaGradient(progress);
                CustomParticles.GenericFlare(Projectile.Center, trailColor * 0.6f, 0.25f, 12);
                
                var glow = new GenericGlowParticle(Projectile.Center, -Projectile.velocity * 0.08f,
                    trailColor * 0.5f, 0.18f, 10, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }
            
            Lighting.AddLight(Projectile.Center, EnigmaGreen.ToVector3() * 0.3f);
        }
        
        private NPC FindClosestEnemy(float range)
        {
            NPC closest = null;
            float closestDist = range;
            
            foreach (NPC npc in Main.ActiveNPCs)
            {
                if (npc.friendly) continue;
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
            target.AddBuff(ModContent.BuffType<ParadoxBrand>(), 240);
            target.GetGlobalNPC<ParadoxBrandNPC>().AddParadoxStack(target, 1);
            
            // === REALITY WARP DISTORTION ===
            FateRealityDistortion.TriggerChromaticAberration(target.Center, 2.5f, 8);
            
            // === NEW UNIFIED VFX HIT EFFECT ===
            UnifiedVFX.EnigmaVariations.HitEffect(target.Center, 1.2f);
            
            // === WATCHING EYE AT IMPACT ===
            CustomParticles.EnigmaEyeImpact(target.Center, target.Center, EnigmaGreen, 0.5f);
            
            // === MUSIC NOTES BURST ===
            ThemedParticles.EnigmaMusicNoteBurst(target.Center, 10, 6f);
            ThemedParticles.EnigmaMusicNotes(target.Center, 5, 35f);
            
            // Impact
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f;
                Vector2 offset = angle.ToRotationVector2() * 20f;
                CustomParticles.GenericFlare(target.Center + offset, GetEnigmaGradient((float)i / 6f), 0.35f, 14);
            }
            
            CustomParticles.HaloRing(target.Center, EnigmaPurple * 0.7f, 0.32f, 14);
            CustomParticles.GlyphImpact(target.Center, EnigmaPurple, EnigmaGreen, 0.4f);
            
            // === GLYPH CIRCLE FORMATION ===
            CustomParticles.GlyphCircle(target.Center, EnigmaPurple, count: 6, radius: 45f, rotationSpeed: 0.06f);
            
            // === DYNAMIC LIGHTING ===
            Lighting.AddLight(target.Center, EnigmaGreen.ToVector3() * 0.8f);
        }
        
        public override void OnKill(int timeLeft)
        {
            // === REALITY WARP ON DEATH ===
            FateRealityDistortion.TriggerChromaticAberration(Projectile.Center, 3f, 12);
            
            for (int i = 0; i < 5; i++)
            {
                float angle = MathHelper.TwoPi * i / 5f;
                Vector2 vel = angle.ToRotationVector2() * 3f;
                var glow = new GenericGlowParticle(Projectile.Center, vel, GetEnigmaGradient((float)i / 5f), 0.25f, 12, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }
            
            // === WATCHING EYE at death point ===
            CustomParticles.EnigmaEyeGaze(Projectile.Center, EnigmaGreen * 0.7f, 0.35f, Projectile.velocity.SafeNormalize(Vector2.UnitX));
        }
    }
}
