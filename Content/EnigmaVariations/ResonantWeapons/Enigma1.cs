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
    /// PARADOX SPIRALBLADE - Enigma Melee Sword
    /// =========================================
    /// UNIQUE MECHANICS:
    /// - Every swing spawns 5 spiral projectiles that orbit outward in a fractal pattern
    /// - Projectiles leave "paradox rifts" that damage enemies who touch them
    /// - Every 3rd hit creates a massive REALITY TEAR - a line that slices through reality
    /// - Reality tears have eyes at endpoints watching inward + along the tear line
    /// - Enemies hit get marked with rotating glyph circles (Paradox Brand)
    /// - At 5 Paradox stacks, enemy EXPLODES with paradox energy hitting nearby foes
    /// - During combo, player gains orbiting eye/glyph aura
    /// </summary>
    public class Enigma1 : ModItem
    {
        // Enigma color palette with proper gradients
        private static readonly Color EnigmaBlack = new Color(15, 10, 20);
        private static readonly Color EnigmaPurple = new Color(140, 60, 200);
        private static readonly Color EnigmaGreen = new Color(50, 220, 100);
        private static readonly Color EnigmaDeepPurple = new Color(80, 20, 120);
        
        private int strikeCounter = 0;
        private int comboTimer = 0;
        private const int ComboResetTime = 120;
        
        public override string Texture => "Terraria/Images/Item_" + ItemID.TerraBlade;
        
        private Color GetEnigmaGradient(float progress)
        {
            if (progress < 0.5f)
                return Color.Lerp(EnigmaBlack, EnigmaPurple, progress * 2f);
            else
                return Color.Lerp(EnigmaPurple, EnigmaGreen, (progress - 0.5f) * 2f);
        }
        
        public override void SetDefaults()
        {
            Item.damage = 520;
            Item.DamageType = DamageClass.Melee;
            Item.width = 60;
            Item.height = 60;
            Item.useTime = 18;
            Item.useAnimation = 18;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 6f;
            Item.value = Item.sellPrice(gold: 18);
            Item.rare = ModContent.RarityType<EnigmaRarity>();
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<ParadoxSpiralProjectile>();
            Item.shootSpeed = 10f;
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "EnigmaEffect1", "Swings spawn spiraling paradox shards that leave rifts"));
            tooltips.Add(new TooltipLine(Mod, "EnigmaEffect2", "Every 3rd hit creates a REALITY TEAR dealing massive line damage"));
            tooltips.Add(new TooltipLine(Mod, "EnigmaEffect3", "Enemies at 5 Paradox stacks EXPLODE affecting nearby foes"));
            tooltips.Add(new TooltipLine(Mod, "EnigmaLore", "'The blade spirals through dimensions, questioning existence itself'") 
            { 
                OverrideColor = EnigmaPurple 
            });
        }
        
        public override void HoldItem(Player player)
        {
            if (comboTimer > 0)
            {
                comboTimer--;
                if (comboTimer <= 0)
                    strikeCounter = 0;
            }
            
            // === AMBIENT MYSTERY AURA (always active) ===
            // Orbiting mystery flares - the enigma pulses with arcane energy
            if (Main.rand.NextBool(5))
            {
                float baseAngle = Main.GameUpdateCount * 0.035f;
                for (int i = 0; i < 4; i++)
                {
                    float angle = baseAngle + MathHelper.TwoPi * i / 4f;
                    float radius = 42f + (float)Math.Sin(Main.GameUpdateCount * 0.08f + i * 0.8f) * 12f;
                    Vector2 flarePos = player.Center + angle.ToRotationVector2() * radius;
                    Color flareColor = GetEnigmaGradient((float)i / 4f + Main.GameUpdateCount * 0.01f % 1f) * 0.6f;
                    CustomParticles.GenericFlare(flarePos, flareColor, 0.32f, 16);
                }
            }
            
            // Watching eye - the mystery observes
            if (Main.rand.NextBool(30))
            {
                float eyeAngle = Main.rand.NextFloat() * MathHelper.TwoPi;
                Vector2 eyePos = player.Center + eyeAngle.ToRotationVector2() * 55f;
                CustomParticles.EnigmaEyeGaze(eyePos, EnigmaGreen * 0.5f, 0.35f, (player.Center - eyePos).SafeNormalize(Vector2.UnitX));
            }
            
            // Ambient music notes - the melody of mystery
            if (Main.rand.NextBool(25))
            {
                ThemedParticles.EnigmaMusicNotes(player.Center + Main.rand.NextVector2Circular(40f, 40f), 1, 18f);
            }
            
            // === COMBO VISUAL - orbiting eyes and glyphs ===
            if (strikeCounter > 0)
            {
                float comboIntensity = (float)strikeCounter / 3f;
                
                if (Main.rand.NextBool(6 - strikeCounter))
                {
                    float comboPulse = Main.GameUpdateCount * 0.04f;
                    for (int i = 0; i < strikeCounter; i++)
                    {
                        float angle = comboPulse + MathHelper.TwoPi * i / strikeCounter;
                        float radius = 45f + (float)Math.Sin(Main.GameUpdateCount * 0.08f + i) * 10f;
                        Vector2 glyphPos = player.Center + angle.ToRotationVector2() * radius;
                        CustomParticles.Glyph(glyphPos, GetEnigmaGradient(comboIntensity), 0.28f, -1);
                    }
                }
                
                // Prismatic sparkle ring with gradient - combo power visualization
                if (Main.rand.NextBool(8))
                {
                    float sparkleAngle = Main.rand.NextFloat() * MathHelper.TwoPi;
                    Vector2 sparklePos = player.Center + sparkleAngle.ToRotationVector2() * 50f;
                    CustomParticles.GenericFlare(sparklePos, GetEnigmaGradient(comboIntensity), 0.4f + comboIntensity * 0.18f, 16);
                    ThemedParticles.EnigmaMusicNotes(sparklePos, 1, 10f);
                }
                
                // Watching eyes intensify with combo
                if (Main.rand.NextBool(15 - strikeCounter * 3))
                {
                    float eyeAngle = Main.rand.NextFloat() * MathHelper.TwoPi;
                    Vector2 eyePos = player.Center + eyeAngle.ToRotationVector2() * 60f;
                    CustomParticles.EnigmaEyeGaze(eyePos, GetEnigmaGradient(comboIntensity), 0.45f, (player.Center - eyePos).SafeNormalize(Vector2.UnitX));
                }
                
                CustomParticles.GlyphAura(player.Center, GetEnigmaGradient(comboIntensity), 55f, 1);
                
                // Enhanced combo lighting
                float intensity = strikeCounter / 3f * 0.5f;
                Lighting.AddLight(player.Center, EnigmaPurple.ToVector3() * intensity);
            }
            
            // Base mystical lighting
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.06f) * 0.1f + 0.9f;
            Lighting.AddLight(player.Center, EnigmaGreen.ToVector3() * 0.35f * pulse);
        }
        
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            float baseAngle = velocity.ToRotation();
            
            for (int i = 0; i < 5; i++)
            {
                float offsetAngle = baseAngle + MathHelper.TwoPi * i / 5f * 0.4f - MathHelper.Pi * 0.4f;
                Vector2 spiralVel = offsetAngle.ToRotationVector2() * 8f;
                Projectile.NewProjectile(source, player.Center, spiralVel, type, damage / 2, knockback * 0.5f, player.whoAmI, ai0: i, ai1: baseAngle);
                
                CustomParticles.GenericFlare(player.Center, GetEnigmaGradient((float)i / 5f), 0.45f, 16);
            }
            
            for (int i = 0; i < 8; i++)
            {
                float angle = baseAngle + MathHelper.PiOver2 * ((float)i / 8f - 0.5f);
                Vector2 offset = angle.ToRotationVector2() * (30f + i * 5f);
                CustomParticles.GenericFlare(player.Center + offset, GetEnigmaGradient((float)i / 8f), 0.4f, 15);
            }
            
            CustomParticles.HaloRing(player.Center, EnigmaPurple * 0.7f, 0.35f, 12);
            
            // Music notes burst on attack
            ThemedParticles.EnigmaMusicNoteBurst(player.Center, 6, 4f);
            
            return false;
        }
        
        public override void MeleeEffects(Player player, Rectangle hitbox)
        {
            // Music notes in swing trail - THIS IS A MUSIC MOD!
            if (Main.rand.NextBool(4))
            {
                Vector2 notePos = hitbox.Center.ToVector2() + Main.rand.NextVector2Circular(15f, 15f);
                ThemedParticles.EnigmaMusicNotes(notePos, 2, 20f);
            }
            
            if (Main.rand.NextBool(2))
            {
                Vector2 pos = new Vector2(
                    Main.rand.Next(hitbox.Left, hitbox.Right),
                    Main.rand.Next(hitbox.Top, hitbox.Bottom));
                    
                float progress = Main.rand.NextFloat();
                Color trailColor = GetEnigmaGradient(progress);
                CustomParticles.GenericFlare(pos, trailColor * 0.8f, 0.35f, 14);
                
                if (Main.rand.NextBool(5))
                    CustomParticles.Glyph(pos, EnigmaPurple * 0.6f, 0.22f, -1);
                
                // Floating sparkle motes in swing trail
                if (Main.rand.NextBool(8))
                {
                    var sparkle = new GenericGlowParticle(pos, Main.rand.NextVector2Circular(1.5f, 1.5f), 
                        GetEnigmaGradient(Main.rand.NextFloat()) * 0.6f, 0.25f, 16, true);
                    MagnumParticleHandler.SpawnParticle(sparkle);
                }
            }
        }
        
        public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<ParadoxBrand>(), 360);
            var brandNPC = target.GetGlobalNPC<ParadoxBrandNPC>();
            brandNPC.AddParadoxStack(target, 1);
            
            strikeCounter++;
            comboTimer = ComboResetTime;
            
            // === USE THE SPECTACULAR NEW UNIFIED VFX ===
            UnifiedVFX.EnigmaVariations.HitEffect(target.Center, 1.2f);
            
            // === ADDITIONAL FRACTAL BURST for extra elegance ===
            for (int i = 0; i < 12; i++)
            {
                float angle = MathHelper.TwoPi * i / 12f;
                Vector2 offset = angle.ToRotationVector2() * 35f;
                CustomParticles.GenericFlare(target.Center + offset, GetEnigmaGradient((float)i / 12f), 0.55f, 20);
            }
            
            // === WATCHING EYES at impact point ===
            CustomParticles.EnigmaEyeImpact(target.Center, target.Center, EnigmaGreen, 0.5f);
            
            // === ORBITING GLYPH FORMATION ===
            CustomParticles.GlyphCircle(target.Center, EnigmaPurple, count: 6, radius: 45f, rotationSpeed: 0.06f);
            
            // === CASCADING MUSIC NOTES - the mystery sings ===
            ThemedParticles.EnigmaMusicNoteBurst(target.Center, 10, 6f);
            ThemedParticles.EnigmaMusicNotes(target.Center, 5, 35f);
            
            // === SPIRAL SPARKLE FORMATION ===
            for (int arm = 0; arm < 4; arm++)
            {
                float armAngle = MathHelper.TwoPi * arm / 4f + Main.GameUpdateCount * 0.03f;
                for (int point = 0; point < 3; point++)
                {
                    float spiralAngle = armAngle + point * 0.3f;
                    Vector2 spiralPos = target.Center + spiralAngle.ToRotationVector2() * (20f + point * 15f);
                    var sparkle = new GenericGlowParticle(spiralPos, spiralAngle.ToRotationVector2() * 2f, 
                        GetEnigmaGradient((arm * 3 + point) / 12f), 0.35f, 18, true);
                    MagnumParticleHandler.SpawnParticle(sparkle);
                }
            }
            
            int stacks = brandNPC.paradoxStacks;
            if (stacks > 0)
            {
                CustomParticles.GlyphStack(target.Center + new Vector2(0, -35f), EnigmaGreen, stacks, 0.35f);
            }
            
            if (stacks >= 5)
            {
                TriggerParadoxDetonation(player, target, brandNPC);
            }
            
            if (strikeCounter >= 3)
            {
                strikeCounter = 0;
                TriggerRealityTear(player, target);
                brandNPC.AddParadoxStack(target, 2);
            }
            
            Lighting.AddLight(target.Center, EnigmaGreen.ToVector3() * 0.8f);
        }
        
        private void TriggerParadoxDetonation(Player player, NPC target, ParadoxBrandNPC brandNPC)
        {
            SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.2f, Volume = 1.0f }, target.Center);
            SoundEngine.PlaySound(SoundID.Item14 with { Pitch = -0.5f, Volume = 0.8f }, target.Center);
            
            brandNPC.paradoxStacks = 0;
            
            float explosionRadius = 200f;
            
            // === USE THE NEW SPECTACULAR UNIFIED VFX EXPLOSION ===
            UnifiedVFX.EnigmaVariations.Explosion(target.Center, 1.5f);
            
            // === ADDITIONAL DOUBLE SPIRAL GALAXY - THE MYSTERY UNRAVELS ===
            for (int arm = 0; arm < 8; arm++)
            {
                float armAngle = MathHelper.TwoPi * arm / 8f + Main.GameUpdateCount * 0.04f;
                for (int point = 0; point < 6; point++)
                {
                    float spiralAngle = armAngle + point * 0.35f;
                    float spiralRadius = 30f + point * 25f;
                    Vector2 spiralPos = target.Center + spiralAngle.ToRotationVector2() * spiralRadius;
                    float gradientProgress = (arm * 6 + point) / 48f;
                    CustomParticles.GenericFlare(spiralPos, GetEnigmaGradient(gradientProgress), 0.6f + point * 0.05f, 25);
                    
                    // Trailing sparkles behind each spiral point
                    if (point > 0)
                    {
                        var sparkle = new GenericGlowParticle(spiralPos, -spiralAngle.ToRotationVector2() * 2f, 
                            GetEnigmaGradient(gradientProgress) * 0.8f, 0.35f, 20, true);
                        MagnumParticleHandler.SpawnParticle(sparkle);
                    }
                }
            }
            
            // === WATCHING EYES - THE MYSTERY GAZES UPON DESTRUCTION ===
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f;
                Vector2 eyePos = target.Center + angle.ToRotationVector2() * 80f;
                CustomParticles.EnigmaEyeGaze(eyePos, EnigmaGreen, 0.6f, (target.Center - eyePos).SafeNormalize(Vector2.UnitX));
            }
            
            // === CASCADING GLYPH TOWERS - ARCANE PILLARS OF PARADOX ===
            for (int tower = 0; tower < 4; tower++)
            {
                float towerAngle = MathHelper.TwoPi * tower / 4f + MathHelper.PiOver4;
                Vector2 towerPos = target.Center + towerAngle.ToRotationVector2() * 60f;
                CustomParticles.GlyphTower(towerPos, GetEnigmaGradient((float)tower / 4f), layers: 4, baseScale: 0.45f);
            }
            
            // === MUSIC NOTES CASCADE - THE SYMPHONY OF PARADOX ===
            ThemedParticles.EnigmaMusicNoteBurst(target.Center, 16, 8f);
            for (int wave = 0; wave < 3; wave++)
            {
                float waveRadius = 40f + wave * 35f;
                for (int note = 0; note < 6; note++)
                {
                    float noteAngle = MathHelper.TwoPi * note / 6f + wave * 0.3f;
                    Vector2 notePos = target.Center + noteAngle.ToRotationVector2() * waveRadius;
                    ThemedParticles.EnigmaMusicNotes(notePos, 2, 15f);
                }
            }
            
            foreach (NPC npc in Main.ActiveNPCs)
            {
                if (npc.friendly || npc.whoAmI == target.whoAmI) continue;
                
                float dist = Vector2.Distance(npc.Center, target.Center);
                if (dist <= explosionRadius)
                {
                    float falloff = 1f - (dist / explosionRadius) * 0.5f;
                    int explosionDamage = (int)(Item.damage * 2.5f * falloff);
                    npc.SimpleStrikeNPC(explosionDamage, player.direction, true, 12f);
                    npc.AddBuff(ModContent.BuffType<ParadoxBrand>(), 300);
                    npc.GetGlobalNPC<ParadoxBrandNPC>().AddParadoxStack(npc, 2);
                    
                    MagnumVFX.DrawFractalLightning(target.Center, npc.Center, EnigmaGreen, 10, 30f, 4, 0.4f);
                    // Cascading sparkle wave at chained target
                    CustomParticles.GenericFlare(npc.Center, EnigmaGreen, 0.55f, 18);
                    CustomParticles.HaloRing(npc.Center, EnigmaPurple * 0.7f, 0.35f, 14);
                }
            }
        }
        
        private void TriggerRealityTear(Player player, NPC target)
        {
            SoundEngine.PlaySound(SoundID.Item122 with { Pitch = -0.3f, Volume = 0.9f }, target.Center);
            SoundEngine.PlaySound(SoundID.Item71 with { Pitch = -0.6f, Volume = 0.7f }, target.Center);
            
            Vector2 tearDirection = (target.Center - player.Center).SafeNormalize(Vector2.UnitX);
            Vector2 tearStart = player.Center + tearDirection * 30f;
            Vector2 tearEnd = player.Center + tearDirection * 450f;
            
            Projectile.NewProjectile(player.GetSource_ItemUse(Item), tearStart, tearDirection, 
                ModContent.ProjectileType<RealityTearLine>(), Item.damage * 3, Item.knockBack * 2f, player.whoAmI,
                ai0: tearEnd.X, ai1: tearEnd.Y);
            
            // === PHASE 1: REALITY FRACTURE ENDPOINTS ===
            // Start point - the origin of the tear
            UnifiedVFX.EnigmaVariations.Impact(tearStart, 1.0f);
            // End point - where reality finishes breaking
            UnifiedVFX.EnigmaVariations.Impact(tearEnd, 1.2f);
            
            // === PHASE 2: WATCHING EYES ALONG THE TEAR - THE VOID OBSERVES ===
            for (int i = 0; i < 8; i++)
            {
                float t = (float)(i + 1) / 9f;
                Vector2 eyePos = Vector2.Lerp(tearStart, tearEnd, t);
                // Eyes alternate sides of the tear
                Vector2 perpendicular = new Vector2(-tearDirection.Y, tearDirection.X);
                eyePos += perpendicular * (i % 2 == 0 ? 30f : -30f);
                CustomParticles.EnigmaEyeGaze(eyePos, GetEnigmaGradient(t), 0.45f, tearDirection);
            }
            
            // === PHASE 3: DENSE SPARKLE CASCADE ALONG THE TEAR ===
            int sparkleCount = 20;
            for (int i = 0; i < sparkleCount; i++)
            {
                float t = (float)i / sparkleCount;
                Vector2 sparklePos = Vector2.Lerp(tearStart, tearEnd, t);
                
                // Central tear particles
                CustomParticles.GenericFlare(sparklePos, GetEnigmaGradient(t), 0.65f - t * 0.2f, 25);
                
                // Perpendicular fractured edges
                Vector2 perpendicular = new Vector2(-tearDirection.Y, tearDirection.X);
                for (int side = -1; side <= 1; side += 2)
                {
                    float offset = 15f + Main.rand.NextFloat(10f);
                    Vector2 sidePos = sparklePos + perpendicular * offset * side;
                    var sideSparkle = new GenericGlowParticle(sidePos, perpendicular * side * 1.5f, 
                        GetEnigmaGradient(t) * 0.8f, 0.3f, 18, true);
                    MagnumParticleHandler.SpawnParticle(sideSparkle);
                }
                
                // Music notes scattered along tear - the melody of reality breaking
                if (i % 4 == 0)
                    ThemedParticles.EnigmaMusicNotes(sparklePos, 2, 15f);
            }
            
            // === PHASE 4: GLYPH FORMATIONS AT KEY POINTS ===
            CustomParticles.GlyphCircle(tearStart, EnigmaPurple, count: 8, radius: 45f, rotationSpeed: 0.08f);
            CustomParticles.GlyphCircle(tearEnd, EnigmaGreen, count: 8, radius: 45f, rotationSpeed: -0.08f);
            CustomParticles.GlyphCircle(Vector2.Lerp(tearStart, tearEnd, 0.5f), GetEnigmaGradient(0.5f), count: 12, radius: 60f, rotationSpeed: 0.05f);
            
            // === PHASE 5: DENSE TEAR PARTICLE TRAIL ===
            int tearParticles = 50;
            for (int i = 0; i <= tearParticles; i++)
            {
                float t = (float)i / tearParticles;
                Vector2 tearPos = Vector2.Lerp(tearStart, tearEnd, t);
                
                // Main tear glow
                CustomParticles.GenericFlare(tearPos, GetEnigmaGradient(t), 0.7f - t * 0.15f, 30);
                
                // Fractal side particles
                if (i % 2 == 0)
                {
                    Vector2 perpendicular = new Vector2(-tearDirection.Y, tearDirection.X);
                    float sideOffset = 22f + (float)Math.Sin(i * 0.5f) * 8f;
                    CustomParticles.GenericFlare(tearPos + perpendicular * sideOffset, EnigmaPurple * 0.75f, 0.4f, 22);
                    CustomParticles.GenericFlare(tearPos - perpendicular * sideOffset, EnigmaPurple * 0.75f, 0.4f, 22);
                }
            }
            
            // === PHASE 6: LAYERED HALOS AT ENDPOINTS ===
            for (int ring = 0; ring < 6; ring++)
            {
                float ringProgress = ring / 6f;
                Color ringColor = GetEnigmaGradient(ringProgress);
                float scale = 0.35f + ring * 0.15f;
                int lifetime = 20 + ring * 4;
                CustomParticles.HaloRing(tearStart, ringColor, scale, lifetime);
                CustomParticles.HaloRing(tearEnd, ringColor, scale, lifetime);
            }
            
            // === PHASE 7: GLYPH BURST FROM CENTER ===
            Vector2 tearCenter = Vector2.Lerp(tearStart, tearEnd, 0.5f);
            CustomParticles.GlyphBurst(tearCenter, EnigmaPurple, count: 16, speed: 8f);
            CustomParticles.GlyphBurst(target.Center, EnigmaGreen, count: 12, speed: 6f);
            
            // === PHASE 8: FINAL EXPLOSION AT TARGET ===
            UnifiedVFX.EnigmaVariations.HitEffect(target.Center, 1.4f);
            CustomParticles.GenericFlare(target.Center, Color.White, 1.3f, 25);
            CustomParticles.ExplosionBurst(target.Center, EnigmaGreen, 25, 16f);
            CustomParticles.ExplosionBurst(target.Center, EnigmaPurple, 20, 12f);
            
            // Intense lighting
            Lighting.AddLight(tearStart, EnigmaGreen.ToVector3() * 1.2f);
            Lighting.AddLight(tearEnd, EnigmaGreen.ToVector3() * 1.2f);
            Lighting.AddLight(target.Center, EnigmaGreen.ToVector3() * 1.5f);
        }
    }
    
    public class ParadoxSpiralProjectile : ModProjectile
    {
        private static readonly Color EnigmaBlack = new Color(15, 10, 20);
        private static readonly Color EnigmaDeepPurple = new Color(80, 20, 120);
        private static readonly Color EnigmaPurple = new Color(140, 60, 200);
        private static readonly Color EnigmaGreenFlame = new Color(50, 220, 100);
        private static readonly Color EnigmaGreen = new Color(50, 220, 100);
        
        private float spiralAngle = 0f;
        private float spiralRadius = 25f;
        private Vector2 orbitCenter;
        private float baseDirection;
        
        public override string Texture => "MagnumOpus/Assets/Particles/SoftGlow";
        
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D glowTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/SoftGlow").Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = glowTex.Size() / 2f;
            
            // Draw eerie trail
            if (ProjectileID.Sets.TrailCacheLength[Projectile.type] > 0)
            {
                for (int i = 0; i < Projectile.oldPos.Length; i++)
                {
                    if (Projectile.oldPos[i] == Vector2.Zero) continue;
                    float trailProgress = (float)i / Projectile.oldPos.Length;
                    float trailAlpha = (1f - trailProgress) * 0.55f;
                    float trailScale = (1f - trailProgress * 0.4f) * 0.45f;
                    Vector2 trailPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                    
                    Color trailColor = Color.Lerp(EnigmaGreenFlame, EnigmaPurple, trailProgress);
                    spriteBatch.Draw(glowTex, trailPos, null, trailColor * trailAlpha, 0f, origin, trailScale, SpriteEffects.None, 0f);
                }
            }
            
            // Draw layered glow core - mysterious void
            spriteBatch.Draw(glowTex, drawPos, null, EnigmaBlack * 0.9f, 0f, origin, 0.9f, SpriteEffects.None, 0f);
            spriteBatch.Draw(glowTex, drawPos, null, EnigmaDeepPurple * 0.7f, 0f, origin, 0.65f, SpriteEffects.None, 0f);
            spriteBatch.Draw(glowTex, drawPos, null, EnigmaPurple * 0.8f, 0f, origin, 0.4f, SpriteEffects.None, 0f);
            spriteBatch.Draw(glowTex, drawPos, null, EnigmaGreenFlame * 0.6f, 0f, origin, 0.2f, SpriteEffects.None, 0f);
            
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
            Projectile.width = 18;
            Projectile.height = 18;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 4;
            Projectile.timeLeft = 100;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.extraUpdates = 1;
        }
        
        public override void OnSpawn(IEntitySource source)
        {
            spiralAngle = Projectile.ai[0] * MathHelper.TwoPi / 5f;
            baseDirection = Projectile.ai[1];
            orbitCenter = Projectile.Center;
        }
        
        public override void AI()
        {
            spiralAngle += 0.18f;
            spiralRadius += 2f;
            
            orbitCenter += baseDirection.ToRotationVector2() * 3f;
            
            Vector2 spiralOffset = new Vector2((float)Math.Cos(spiralAngle), (float)Math.Sin(spiralAngle)) * spiralRadius;
            Vector2 targetPos = orbitCenter + spiralOffset;
            Projectile.Center = Vector2.Lerp(Projectile.Center, targetPos, 0.35f);
            
            Projectile.rotation += 0.25f;
            
            if (Main.rand.NextBool(2))
            {
                float lifeProgress = 1f - (Projectile.timeLeft / 100f);
                Color trailColor = GetEnigmaGradient(lifeProgress);
                CustomParticles.GenericFlare(Projectile.Center, trailColor * 0.85f, 0.38f, 16);
                
                var glow = new GenericGlowParticle(Projectile.Center, -Projectile.velocity * 0.12f + Main.rand.NextVector2Circular(1f, 1f),
                    trailColor * 0.65f, 0.28f, 14, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }
            
            if (Projectile.timeLeft % 12 == 0)
            {
                CustomParticles.GlyphTrail(Projectile.Center, Projectile.velocity, EnigmaPurple, 0.28f);
            }
            
            if (Projectile.timeLeft % 20 == 0 && Projectile.timeLeft < 80)
            {
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero,
                    ModContent.ProjectileType<ParadoxRift>(), Projectile.damage / 3, 0f, Projectile.owner);
            }
            
            Lighting.AddLight(Projectile.Center, GetEnigmaGradient(spiralAngle / MathHelper.TwoPi).ToVector3() * 0.5f);
        }
        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<ParadoxBrand>(), 240);
            var brandNPC = target.GetGlobalNPC<ParadoxBrandNPC>();
            brandNPC.AddParadoxStack(target, 1);
            
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 offset = angle.ToRotationVector2() * 25f;
                CustomParticles.GenericFlare(target.Center + offset, GetEnigmaGradient((float)i / 8f), 0.45f, 16);
            }
            
            // Sparkle burst instead of eye
            CustomParticles.GenericFlare(target.Center - new Vector2(0, 25f), EnigmaPurple, 0.6f, 18);
            for (int i = 0; i < 6; i++)
            {
                float sparkAngle = MathHelper.TwoPi * i / 6f;
                Vector2 sparkVel = sparkAngle.ToRotationVector2() * 3.5f;
                var sparkle = new GenericGlowParticle(target.Center - new Vector2(0, 25f), sparkVel, GetEnigmaGradient((float)i / 6f), 0.35f, 18, true);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
            CustomParticles.GlyphImpact(target.Center, EnigmaPurple, EnigmaGreen, 0.45f);
        }
        
        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 10; i++)
            {
                float angle = MathHelper.TwoPi * i / 10f;
                Vector2 vel = angle.ToRotationVector2() * 5f;
                var glow = new GenericGlowParticle(Projectile.Center, vel, GetEnigmaGradient((float)i / 10f), 0.4f, 20, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }
            
            CustomParticles.HaloRing(Projectile.Center, EnigmaPurple * 0.75f, 0.4f, 16);
            CustomParticles.GlyphBurst(Projectile.Center, EnigmaGreen, count: 4, speed: 2f);
        }
    }
    
    public class ParadoxRift : ModProjectile
    {
        private static readonly Color EnigmaBlack = new Color(15, 10, 20);
        private static readonly Color EnigmaDeepPurple = new Color(80, 20, 120);
        private static readonly Color EnigmaPurple = new Color(140, 60, 200);
        private static readonly Color EnigmaGreenFlame = new Color(50, 220, 100);
        private static readonly Color EnigmaGreen = new Color(50, 220, 100);
        
        public override string Texture => "MagnumOpus/Assets/Particles/SoftGlow";
        
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D glowTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/SoftGlow").Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = glowTex.Size() / 2f;
            
            float lifeProgress = 1f - (Projectile.timeLeft / 60f);
            float pulse = 0.7f + (float)Math.Sin(Main.GameUpdateCount * 0.15f) * 0.3f;
            float opacity = 1f - lifeProgress * 0.6f;
            
            // Draw pulsing rift glow
            spriteBatch.Draw(glowTex, drawPos, null, EnigmaBlack * opacity * pulse, 0f, origin, 0.8f * pulse, SpriteEffects.None, 0f);
            spriteBatch.Draw(glowTex, drawPos, null, EnigmaDeepPurple * 0.6f * opacity, 0f, origin, 0.55f * pulse, SpriteEffects.None, 0f);
            spriteBatch.Draw(glowTex, drawPos, null, EnigmaPurple * 0.7f * opacity, 0f, origin, 0.35f * pulse, SpriteEffects.None, 0f);
            spriteBatch.Draw(glowTex, drawPos, null, EnigmaGreenFlame * 0.5f * opacity, 0f, origin, 0.18f * pulse, SpriteEffects.None, 0f);
            
            return false;
        }
        
        public override void SetDefaults()
        {
            Projectile.width = 35;
            Projectile.height = 35;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 60;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 20;
        }
        
        public override void AI()
        {
            float lifeProgress = 1f - (Projectile.timeLeft / 60f);
            float opacity = 1f - lifeProgress * 0.6f;
            
            if (Main.GameUpdateCount % 3 == 0)
            {
                int spiralCount = 4;
                for (int i = 0; i < spiralCount; i++)
                {
                    float angle = Main.GameUpdateCount * 0.12f + MathHelper.TwoPi * i / spiralCount;
                    float radius = 18f + (float)Math.Sin(Main.GameUpdateCount * 0.15f + i) * 8f;
                    Vector2 particlePos = Projectile.Center + angle.ToRotationVector2() * radius;
                    Vector2 vel = (Projectile.Center - particlePos).SafeNormalize(Vector2.Zero) * 2f;
                    
                    Color spiralColor = Color.Lerp(EnigmaPurple, EnigmaGreen, (float)i / spiralCount) * opacity;
                    var glow = new GenericGlowParticle(particlePos, vel, spiralColor * 0.6f, 0.25f, 12, true);
                    MagnumParticleHandler.SpawnParticle(glow);
                }
            }
            
            if (Main.GameUpdateCount % 10 == 0)
            {
                CustomParticles.GenericFlare(Projectile.Center, EnigmaBlack * opacity, 0.35f * opacity, 10);
            }
            
            Lighting.AddLight(Projectile.Center, EnigmaPurple.ToVector3() * 0.3f * opacity);
        }
        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<ParadoxBrand>(), 180);
            target.GetGlobalNPC<ParadoxBrandNPC>().AddParadoxStack(target, 1);
            
            CustomParticles.GenericFlare(target.Center, EnigmaPurple, 0.4f, 12);
            CustomParticles.GlyphImpact(target.Center, EnigmaPurple, EnigmaGreen, 0.35f);
        }
    }
    
    public class RealityTearLine : ModProjectile
    {
        private static readonly Color EnigmaBlack = new Color(15, 10, 20);
        private static readonly Color EnigmaDeepPurple = new Color(80, 20, 120);
        private static readonly Color EnigmaPurple = new Color(140, 60, 200);
        private static readonly Color EnigmaGreenFlame = new Color(50, 220, 100);
        private static readonly Color EnigmaGreen = new Color(50, 220, 100);
        
        private Vector2 TearEnd => new Vector2(Projectile.ai[0], Projectile.ai[1]);
        private bool hasDealtDamage = false;
        
        public override string Texture => "MagnumOpus/Assets/Particles/SoftGlow";
        
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D glowTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/SoftGlow").Value;
            Vector2 origin = glowTex.Size() / 2f;
            
            float progress = 1f - (Projectile.timeLeft / 40f);
            float opacity = 1f - progress * 0.7f;
            
            // Draw glow at both endpoints
            Vector2 startPos = Projectile.Center - Main.screenPosition;
            Vector2 endPos = TearEnd - Main.screenPosition;
            
            spriteBatch.Draw(glowTex, startPos, null, EnigmaBlack * opacity * 0.9f, 0f, origin, 0.7f, SpriteEffects.None, 0f);
            spriteBatch.Draw(glowTex, startPos, null, EnigmaPurple * opacity * 0.7f, 0f, origin, 0.45f, SpriteEffects.None, 0f);
            spriteBatch.Draw(glowTex, startPos, null, EnigmaGreenFlame * opacity * 0.5f, 0f, origin, 0.25f, SpriteEffects.None, 0f);
            
            spriteBatch.Draw(glowTex, endPos, null, EnigmaBlack * opacity * 0.9f, 0f, origin, 0.7f, SpriteEffects.None, 0f);
            spriteBatch.Draw(glowTex, endPos, null, EnigmaPurple * opacity * 0.7f, 0f, origin, 0.45f, SpriteEffects.None, 0f);
            spriteBatch.Draw(glowTex, endPos, null, EnigmaGreenFlame * opacity * 0.5f, 0f, origin, 0.25f, SpriteEffects.None, 0f);
            
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
            Projectile.width = 35;
            Projectile.height = 35;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 40;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }
        
        public override void AI()
        {
            if (!hasDealtDamage)
            {
                hasDealtDamage = true;
                DealLineDamage();
            }
            
            if (Projectile.timeLeft % 4 == 0)
            {
                float progress = 1f - (Projectile.timeLeft / 40f);
                float opacity = 1f - progress * 0.7f;
                int particleCount = (int)MathHelper.Lerp(20, 8, progress);
                
                for (int i = 0; i < particleCount; i++)
                {
                    float t = Main.rand.NextFloat();
                    Vector2 pos = Vector2.Lerp(Projectile.Center, TearEnd, t);
                    pos += Main.rand.NextVector2Circular(18f, 18f) * (1f + progress);
                    
                    CustomParticles.GenericFlare(pos, GetEnigmaGradient(t) * opacity, 0.35f, 14);
                }
            }
            
            Lighting.AddLight(Vector2.Lerp(Projectile.Center, TearEnd, 0.5f), EnigmaGreen.ToVector3() * (Projectile.timeLeft / 40f));
        }
        
        private void DealLineDamage()
        {
            Vector2 start = Projectile.Center;
            Vector2 end = TearEnd;
            float tearWidth = 50f;
            
            foreach (NPC npc in Main.ActiveNPCs)
            {
                if (npc.friendly || npc.immortal) continue;
                
                float distToLine = DistanceToLine(npc.Center, start, end);
                if (distToLine <= tearWidth + npc.width / 2f)
                {
                    npc.SimpleStrikeNPC(Projectile.damage, Projectile.direction, true, 14f);
                    npc.AddBuff(ModContent.BuffType<ParadoxBrand>(), 360);
                    npc.GetGlobalNPC<ParadoxBrandNPC>().AddParadoxStack(npc, 3);
                    
                    CustomParticles.GenericFlare(npc.Center, EnigmaGreen, 0.7f, 20);
                    // Sparkle burst above target
                    CustomParticles.GenericFlare(npc.Center + new Vector2(0, -30f), EnigmaPurple, 0.65f, 20);
                    CustomParticles.HaloRing(npc.Center + new Vector2(0, -30f), EnigmaGreen * 0.8f, 0.35f, 15);
                    for (int s = 0; s < 5; s++)
                    {
                        float sAngle = MathHelper.TwoPi * s / 5f;
                        Vector2 sVel = sAngle.ToRotationVector2() * 2.5f;
                        var sparkle = new GenericGlowParticle(npc.Center + new Vector2(0, -30f), sVel, GetEnigmaGradient((float)s / 5f), 0.3f, 16, true);
                        MagnumParticleHandler.SpawnParticle(sparkle);
                    }
                    CustomParticles.GlyphCircle(npc.Center, EnigmaPurple, 6, 35f, 0.06f);
                    
                    int stacks = npc.GetGlobalNPC<ParadoxBrandNPC>().paradoxStacks;
                    CustomParticles.GlyphStack(npc.Center + new Vector2(0, -25f), EnigmaGreen, stacks, 0.28f);
                }
            }
        }
        
        private float DistanceToLine(Vector2 point, Vector2 lineStart, Vector2 lineEnd)
        {
            Vector2 line = lineEnd - lineStart;
            float lineLength = line.Length();
            if (lineLength == 0) return Vector2.Distance(point, lineStart);
            
            float t = Math.Clamp(Vector2.Dot(point - lineStart, line) / (lineLength * lineLength), 0f, 1f);
            Vector2 projection = lineStart + line * t;
            return Vector2.Distance(point, projection);
        }
        
        public override bool? CanDamage() => false;
    }
}
