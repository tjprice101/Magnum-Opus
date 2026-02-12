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
using MagnumOpus.Common.Systems.VFX;
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
        
        public override void HoldItem(Player player)
        {
            // === ENHANCED MYSTERY ORB PREVIEW ===
            // Orbiting mini-orbs with eyes watching outward
            float baseAngle = Main.GameUpdateCount * 0.025f;
            
            // === LAYER 1: Three orbiting mini mystery orbs ===
            if (Main.rand.NextBool(8))
            {
                for (int i = 0; i < 3; i++)
                {
                    float angle = baseAngle + MathHelper.TwoPi * i / 3f;
                    float radius = 35f + (float)Math.Sin(Main.GameUpdateCount * 0.04f + i * 1.2f) * 8f;
                    Vector2 orbPos = player.Center + angle.ToRotationVector2() * radius;
                    float progress = (i / 3f + Main.GameUpdateCount * 0.01f) % 1f;
                    var orb = new GenericGlowParticle(orbPos, Vector2.Zero, GetEnigmaGradient(progress), 0.22f, 14, true);
                    MagnumParticleHandler.SpawnParticle(orb);
                }
            }
            
            // === LAYER 2: Watching eye particles ===
            if (Main.rand.NextBool(25))
            {
                float eyeAngle = Main.rand.NextFloat() * MathHelper.TwoPi;
                Vector2 eyePos = player.Center + eyeAngle.ToRotationVector2() * Main.rand.NextFloat(28f, 50f);
                CustomParticles.EnigmaEyeGaze(eyePos, EnigmaPurple * 0.8f, 0.35f, null);
            }
            
            // === LAYER 3: Glyph accents ===
            if (Main.rand.NextBool(30))
            {
                Vector2 glyphOffset = Main.rand.NextVector2Circular(40f, 40f);
                CustomParticles.Glyph(player.Center + glyphOffset, GetEnigmaGradient(Main.rand.NextFloat()), 0.25f, -1);
            }
            
            // === LAYER 4: Green flame wisps ===
            if (Main.rand.NextBool(18))
            {
                Vector2 flamePos = player.Center + Main.rand.NextVector2Circular(25f, 25f);
                Vector2 flameVel = new Vector2(0, -1.5f) + Main.rand.NextVector2Circular(0.5f, 0.5f);
                var flame = new GenericGlowParticle(flamePos, flameVel, EnigmaGreen * 0.6f, 0.2f, 18, true);
                MagnumParticleHandler.SpawnParticle(flame);
            }
            
            // === LAYER 5: Music notes (this is a music mod!) ===
            if (Main.rand.NextBool(35))
            {
                ThemedParticles.EnigmaMusicNotes(player.Center, 1, 35f);
            }
            
            // Pulsing arcane light with color shift
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.06f) * 0.15f + 0.85f;
            float hueShift = (Main.GameUpdateCount * 0.008f) % 1f;
            Color lightColor = Color.Lerp(EnigmaPurple, EnigmaGreen, hueShift * 0.4f);
            Lighting.AddLight(player.Center, lightColor.ToVector3() * pulse * 0.4f);
        }
        
        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            // === ENIGMA MYSTERY GLOW EFFECT ===
            Texture2D texture = Terraria.GameContent.TextureAssets.Item[Item.type].Value;
            Vector2 drawPos = Item.Center - Main.screenPosition;
            Vector2 origin = texture.Size() / 2f;
            
            float time = Main.GameUpdateCount * 0.04f;
            float pulse = 1f + (float)Math.Sin(time * 1.5f) * 0.12f;
            
            // Switch to additive blending for glow
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, 
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            // === LAYER 1: Outer purple mystery glow ===
            Color outerGlow = EnigmaPurple * 0.25f;
            spriteBatch.Draw(texture, drawPos, null, outerGlow, rotation, origin, scale * pulse * 1.4f, SpriteEffects.None, 0f);
            
            // === LAYER 2: Mid green flame shimmer ===
            float greenPulse = (float)Math.Sin(time * 2f + 1f) * 0.1f + 0.9f;
            Color midGlow = EnigmaGreen * 0.2f * greenPulse;
            spriteBatch.Draw(texture, drawPos, null, midGlow, rotation + 0.05f, origin, scale * pulse * 1.25f, SpriteEffects.None, 0f);
            
            // === LAYER 3: Inner core glow ===
            Color coreGlow = Color.Lerp(EnigmaPurple, EnigmaGreen, (float)Math.Sin(time) * 0.5f + 0.5f) * 0.35f;
            spriteBatch.Draw(texture, drawPos, null, coreGlow, rotation, origin, scale * pulse * 1.1f, SpriteEffects.None, 0f);
            
            // Return to normal blending
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, 
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            // Draw the actual item
            spriteBatch.Draw(texture, drawPos, null, lightColor, rotation, origin, scale, SpriteEffects.None, 0f);
            
            // Emit light
            Lighting.AddLight(Item.Center, EnigmaPurple.ToVector3() * 0.5f);
            
            return false;
        }
        
        public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            // === INVENTORY MYSTERY PULSE ===
            Texture2D texture = Terraria.GameContent.TextureAssets.Item[Item.type].Value;
            
            float time = Main.GameUpdateCount * 0.05f;
            float pulse = 1f + (float)Math.Sin(time * 1.8f) * 0.08f;
            
            // Additive glow layer behind the item
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, 
                DepthStencilState.None, RasterizerState.CullNone, null, Main.UIScaleMatrix);
            
            // Purple-green shifting glow
            float colorShift = (float)Math.Sin(time * 0.8f) * 0.5f + 0.5f;
            Color glowColor = Color.Lerp(EnigmaPurple, EnigmaGreen, colorShift) * 0.3f;
            spriteBatch.Draw(texture, position, frame, glowColor, 0f, origin, scale * pulse * 1.15f, SpriteEffects.None, 0f);
            
            // Return to normal blending
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, 
                DepthStencilState.None, RasterizerState.CullNone, null, Main.UIScaleMatrix);
            
            // Draw the actual item
            spriteBatch.Draw(texture, position, frame, drawColor, 0f, origin, scale, SpriteEffects.None, 0f);
            
            return false;
        }
        
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // Spawn orb
            Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
            
            // Cast VFX - clean and focused
            Vector2 castPos = position + velocity.SafeNormalize(Vector2.Zero) * 30f;
            
            // Single glyph circle
            CustomParticles.GlyphCircle(castPos, EnigmaPurple, count: 4, radius: 30f, rotationSpeed: 0.06f);
            
            // Central flare
            EnhancedParticles.BloomFlare(castPos, EnigmaGreen, 0.6f, 18, 3, 0.9f);
            
            // Music notes
            ThemedParticles.EnigmaMusicNoteBurst(castPos, 4, 3f);
            
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
        
        public override string Texture => "MagnumOpus/Assets/Particles/TriangularEye";
        
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
                float corePulse = 0.55f + (float)Math.Sin(Main.GameUpdateCount * 0.12f) * 0.15f;
                CustomParticles.GenericFlare(Projectile.Center, GetEnigmaGradient(growthProgress), 
                    corePulse * currentScale * 0.55f, 14);
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
            
            // === CALAMITY-STANDARD RADIANT TRAIL EFFECTS ===
            // Heavy dust trails (2+ per frame) - void energy stream
            for (int d = 0; d < 2; d++)
            {
                Vector2 dustOffset = Main.rand.NextVector2Circular(10f, 10f) * currentScale;
                Dust dustPurple = Dust.NewDustPerfect(Projectile.Center + dustOffset, DustID.PurpleTorch, 
                    -Projectile.velocity * 0.3f + Main.rand.NextVector2Circular(1f, 1f), 0, EnigmaPurple, 1.3f);
                dustPurple.noGravity = true;
                dustPurple.fadeIn = 1.4f;
                
                Dust dustGreen = Dust.NewDustPerfect(Projectile.Center + dustOffset * 0.5f, DustID.CursedTorch, 
                    -Projectile.velocity * 0.2f + Main.rand.NextVector2Circular(0.8f, 0.8f), 0, EnigmaGreen, 1.1f);
                dustGreen.noGravity = true;
                dustGreen.fadeIn = 1.3f;
            }
            
            // Contrasting sparkles (1-in-2) - mystery shimmer
            if (Main.rand.NextBool(2))
            {
                Vector2 sparkleOffset = Main.rand.NextVector2Circular(15f, 15f) * currentScale;
                var sparkle = new SparkleParticle(Projectile.Center + sparkleOffset, 
                    -Projectile.velocity * 0.1f + Main.rand.NextVector2Circular(0.5f, 0.5f), 
                    EnigmaGreenFlame, 0.45f * currentScale, 20);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
            
            // Enigma shimmer trails (1-in-3) - void hue cycling
            if (Main.rand.NextBool(3))
            {
                float hue = Main.rand.NextFloat(0.28f, 0.45f); // Purple-green void range
                Color shimmerColor = Main.hslToRgb(hue, 0.85f, 0.65f);
                var shimmer = new GenericGlowParticle(Projectile.Center, -Projectile.velocity * 0.15f, 
                    shimmerColor, 0.35f * currentScale, 22, true);
                MagnumParticleHandler.SpawnParticle(shimmer);
            }
            
            // Pearlescent void effect (1-in-4)
            if (Main.rand.NextBool(4))
            {
                float shift = (float)Math.Sin(Main.GameUpdateCount * 0.1f + Projectile.whoAmI) * 0.5f + 0.5f;
                Color pearlColor = Color.Lerp(EnigmaPurple, EnigmaGreenFlame, shift) * 0.8f;
                CustomParticles.GenericFlare(Projectile.Center, pearlColor, 0.4f * currentScale, 16);
            }
            
            // Frequent flares (1-in-2) - arcane radiance
            if (Main.rand.NextBool(2))
            {
                Vector2 flareOffset = Main.rand.NextVector2Circular(8f, 8f) * currentScale;
                CustomParticles.GenericFlare(Projectile.Center + flareOffset, 
                    GetEnigmaGradient(growthProgress), 0.35f * currentScale, 14);
            }
            
            // Music notes orbit the growing orb - the riddle sings (enhanced scale)
            if (Main.rand.NextBool(6))
            {
                Color noteColor = Color.Lerp(EnigmaPurple, EnigmaGreenFlame, Main.rand.NextFloat());
                Vector2 noteOffset = Main.rand.NextVector2Circular(30f, 30f) * currentScale;
                Vector2 noteVel = new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), -1f);
                ThemedParticles.MusicNote(Projectile.Center + noteOffset, noteVel, noteColor, 0.85f * currentScale, 35);
            }
            
            // Pulsing mystery light
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.08f) * 0.15f + 0.85f;
            Lighting.AddLight(Projectile.Center, EnigmaPurple.ToVector3() * 0.7f * currentScale * pulse);
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
            
            // === ENHANCED UNIFIED VFX HIT EFFECT WITH BLOOM ===
            UnifiedVFXBloom.EnigmaVariations.ImpactEnhanced(target.Center, 0.9f * currentScale);
            
            // === WATCHING EYE AT IMPACT ===
            CustomParticles.EnigmaEyeImpact(target.Center, target.Center, EnigmaGreen, 0.5f);
            
            // === ENHANCED MUSIC NOTES BURST ===
            EnhancedThemedParticles.EnigmaMusicNotesEnhanced(target.Center, 8, 6f);
            
            // Radiant sparkle crown above target with bloom
            for (int crown = 0; crown < 5; crown++)
            {
                float crownAngle = MathHelper.TwoPi * crown / 5f - MathHelper.PiOver2;
                Vector2 crownPos = target.Center - new Vector2(0, 30f) + crownAngle.ToRotationVector2() * 18f;
                EnhancedParticles.BloomFlare(crownPos, GetEnigmaGradient((float)crown / 5f), 0.4f, 16, 2, 0.8f);
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
            // UNIQUE DEATH: Mystery Unravel - cascading orb unravels spectacularly
            DynamicParticleEffects.EnigmaDeathMysteryUnravel(Projectile.Center, 1.0f * currentScale);
        }
        
        private void TriggerCascadeExplosion()
        {
            SoundEngine.PlaySound(SoundID.Item122 with { Pitch = -0.2f, Volume = 1.1f }, Projectile.Center);
            SoundEngine.PlaySound(SoundID.Item14 with { Pitch = -0.4f, Volume = 0.9f }, Projectile.Center);
            
            float explosionRadius = 170f * currentScale; // Reduced by 20% (was 212f)
            
            // Central themed flash with enhanced bloom
            EnhancedParticles.BloomFlare(Projectile.Center, EnigmaGreen, 1.5f * currentScale, 30, 4, 1.3f);
            EnhancedThemedParticles.EnigmaBloomBurstEnhanced(Projectile.Center, 1.5f * currentScale);
            
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
            
            // === SPAWN SEEKING CRYSTALS - THE CASCADE RELEASES ITS SECRETS ===
            // Massive burst of seeking crystals that home to all nearby enemies
            int crystalCount = 6 + (int)(currentScale * 4); // More crystals as the orb grew larger
            SeekingCrystalHelper.SpawnEnigmaCrystals(
                Projectile.GetSource_Death(),
                Projectile.Center,
                Vector2.Zero, // Zero velocity - they radiate outward
                (int)(Projectile.damage * 0.35f),
                5f,
                Projectile.owner,
                crystalCount
            );
            
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
        
        public override string Texture => "MagnumOpus/Assets/Particles/MagicSparklField4";
        
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
            
            // Music note trail - the riddle's whisper
            if (Main.rand.NextBool(6))
            {
                Color noteColor = Color.Lerp(new Color(140, 60, 200), new Color(50, 220, 100), Main.rand.NextFloat());
                Vector2 noteVel = new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), -1f);
                ThemedParticles.MusicNote(Projectile.Center, noteVel, noteColor, 0.28f, 28);
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
            
            // === ENHANCED UNIFIED VFX HIT EFFECT WITH BLOOM ===
            UnifiedVFXBloom.EnigmaVariations.ImpactEnhanced(target.Center, 1.2f);
            
            // === WATCHING EYE AT IMPACT ===
            CustomParticles.EnigmaEyeImpact(target.Center, target.Center, EnigmaGreen, 0.5f);
            
            // === ENHANCED MUSIC NOTES BURST ===
            EnhancedThemedParticles.EnigmaMusicNotesEnhanced(target.Center, 8, 6f);
            
            // Impact with enhanced bloom
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f;
                Vector2 offset = angle.ToRotationVector2() * 20f;
                EnhancedParticles.BloomFlare(target.Center + offset, GetEnigmaGradient((float)i / 6f), 0.35f, 14, 2, 0.8f);
            }
            
            EnhancedThemedParticles.EnigmaBloomBurstEnhanced(target.Center, 0.5f);
            CustomParticles.GlyphImpact(target.Center, EnigmaPurple, EnigmaGreen, 0.4f);
            
            // === GLYPH CIRCLE FORMATION ===
            CustomParticles.GlyphCircle(target.Center, EnigmaPurple, count: 6, radius: 45f, rotationSpeed: 0.06f);
            
            // === DYNAMIC LIGHTING ===
            Lighting.AddLight(target.Center, EnigmaGreen.ToVector3() * 0.8f);
        }
        
        public override void OnKill(int timeLeft)
        {
            // UNIQUE DEATH: Void Implode - riddlebolt collapses into void
            DynamicParticleEffects.EnigmaDeathVoidImplode(Projectile.Center, 0.6f);
        }
    }
}
