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
    /// VARIATIONS OF THE VOID - Enigma Melee Sword
    /// ============================================
    /// UNIQUE MECHANICS:
    /// - Every swing spawns 5 spiral projectiles that orbit outward in a fractal pattern
    /// - Projectiles leave "paradox rifts" that damage enemies who touch them
    /// - Every 3rd hit creates a massive REALITY TEAR - a line that slices through reality
    /// - Reality tears have eyes at endpoints watching inward + along the tear line
    /// - Enemies hit get marked with rotating glyph circles (Paradox Brand)
    /// - At 5 Paradox stacks, enemy EXPLODES with paradox energy hitting nearby foes
    /// - During combo, player gains orbiting eye/glyph aura
    /// </summary>
    public class VariationsOfTheVoid : ModItem
    {
        // Enigma color palette with proper gradients
        private static readonly Color EnigmaBlack = new Color(15, 10, 20);
        private static readonly Color EnigmaPurple = new Color(140, 60, 200);
        private static readonly Color EnigmaGreen = new Color(50, 220, 100);
        private static readonly Color EnigmaDeepPurple = new Color(80, 20, 120);
        
        private int strikeCounter = 0;
        private int comboTimer = 0;
        private const int ComboResetTime = 120;
        
        // NEW: Void Collapse system - builds with hits, explodes at max
        private int voidChargeStacks = 0;
        private const int MaxVoidChargeStacks = 8;
        
        public static int GetVoidChargeStacks(Player player)
        {
            if (player.HeldItem?.ModItem is VariationsOfTheVoid voidSword)
                return voidSword.voidChargeStacks;
            return 0;
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
            Item.damage = 550;
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
        
        public override bool AltFunctionUse(Player player) => true;
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "EnigmaEffect1", "Swings spawn spiraling paradox shards that leave rifts"));
            tooltips.Add(new TooltipLine(Mod, "EnigmaEffect2", "Every 3rd hit creates a reality tear dealing massive line damage"));
            tooltips.Add(new TooltipLine(Mod, "EnigmaEffect3", "Enemies at 5 Paradox stacks explode affecting nearby foes"));
            tooltips.Add(new TooltipLine(Mod, "EnigmaEffect4", $"Void Charge: {voidChargeStacks}/{MaxVoidChargeStacks} - at max, triggers Void Collapse"));
            tooltips.Add(new TooltipLine(Mod, "EnigmaEffect5", "Right-click fires converging void beams that ignite into a devastating burst"));
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
                {
                    strikeCounter = 0;
                    voidChargeStacks = 0; // Void charge decays when combo breaks
                }
            }
            
            // === VOID CHARGE VISUAL INDICATOR ===
            if (voidChargeStacks > 0)
            {
                float voidIntensity = (float)voidChargeStacks / MaxVoidChargeStacks;
                
                // Orbiting void fragments showing charge level
                if (Main.GameUpdateCount % 10 == 0)
                {
                    int fragmentCount = Math.Min(voidChargeStacks, 6);
                    float baseAngle = Main.GameUpdateCount * 0.05f;
                    for (int i = 0; i < fragmentCount; i++)
                    {
                        float angle = baseAngle + MathHelper.TwoPi * i / fragmentCount;
                        float radius = 50f + voidIntensity * 15f;
                        Vector2 fragPos = player.Center + angle.ToRotationVector2() * radius;
                        Color fragColor = Color.Lerp(EnigmaPurple, EnigmaGreen, voidIntensity) * 0.7f;
                        CustomParticles.GenericFlare(fragPos, fragColor, 0.35f + voidIntensity * 0.15f, 12);
                    }
                }
                
                // Pulsing glyph stack above player showing charge
                if (Main.GameUpdateCount % 30 == 0)
                {
                    CustomParticles.GlyphStack(player.Center - new Vector2(0, 55f), GetEnigmaGradient(voidIntensity), voidChargeStacks, 0.25f);
                }
                
                // Warning flash when near max
                if (voidChargeStacks >= MaxVoidChargeStacks - 1 && Main.GameUpdateCount % 8 == 0)
                {
                    CustomParticles.GenericFlare(player.Center, EnigmaGreen, 0.5f + (float)Math.Sin(Main.GameUpdateCount * 0.2f) * 0.2f, 10);
                }
            }
            
            // === AMBIENT MYSTERY AURA (always active) ===
            // Orbiting mystery flares - the enigma pulses with arcane energy
            if (Main.GameUpdateCount % 20 == 0)
            {
                float baseAngle = Main.GameUpdateCount * 0.035f;
                for (int i = 0; i < 2; i++)
                {
                    float angle = baseAngle + MathHelper.TwoPi * i / 2f;
                    float radius = 42f + (float)Math.Sin(Main.GameUpdateCount * 0.08f + i * 0.8f) * 12f;
                    Vector2 flarePos = player.Center + angle.ToRotationVector2() * radius;
                    Color flareColor = GetEnigmaGradient((float)i / 2f + Main.GameUpdateCount * 0.01f % 1f) * 0.6f;
                    CustomParticles.GenericFlare(flarePos, flareColor, 0.32f, 16);
                }
            }
            
            // Watching eye - the mystery observes
            if (Main.GameUpdateCount % 120 == 0)
            {
                float eyeAngle = Main.rand.NextFloat() * MathHelper.TwoPi;
                Vector2 eyePos = player.Center + eyeAngle.ToRotationVector2() * 55f;
                CustomParticles.EnigmaEyeGaze(eyePos, EnigmaGreen * 0.5f, 0.35f, (player.Center - eyePos).SafeNormalize(Vector2.UnitX));
            }
            
            // Ambient music notes - the melody of mystery
            if (Main.GameUpdateCount % 100 == 0)
            {
                ThemedParticles.EnigmaMusicNotes(player.Center + Main.rand.NextVector2Circular(40f, 40f), 1, 18f);
            }
            
            // === COMBO VISUAL - orbiting eyes and glyphs ===
            if (strikeCounter > 0)
            {
                float comboIntensity = (float)strikeCounter / 3f;
                
                if (Main.GameUpdateCount % 24 == 0)
                {
                    float comboPulse = Main.GameUpdateCount * 0.04f;
                    for (int i = 0; i < Math.Min(strikeCounter, 2); i++)
                    {
                        float angle = comboPulse + MathHelper.TwoPi * i / strikeCounter;
                        float radius = 45f + (float)Math.Sin(Main.GameUpdateCount * 0.08f + i) * 10f;
                        Vector2 glyphPos = player.Center + angle.ToRotationVector2() * radius;
                        CustomParticles.Glyph(glyphPos, GetEnigmaGradient(comboIntensity), 0.28f, -1);
                    }
                }
                
                // Prismatic sparkle ring with gradient - combo power visualization
                if (Main.GameUpdateCount % 32 == 0)
                {
                    float sparkleAngle = Main.rand.NextFloat() * MathHelper.TwoPi;
                    Vector2 sparklePos = player.Center + sparkleAngle.ToRotationVector2() * 50f;
                    CustomParticles.GenericFlare(sparklePos, GetEnigmaGradient(comboIntensity), 0.4f + comboIntensity * 0.18f, 16);
                }
                
                // Watching eyes intensify with combo
                if (Main.GameUpdateCount % 60 == 0)
                {
                    float eyeAngle = Main.rand.NextFloat() * MathHelper.TwoPi;
                    Vector2 eyePos = player.Center + eyeAngle.ToRotationVector2() * 60f;
                    CustomParticles.EnigmaEyeGaze(eyePos, GetEnigmaGradient(comboIntensity), 0.45f, (player.Center - eyePos).SafeNormalize(Vector2.UnitX));
                }
                
                if (Main.GameUpdateCount % 20 == 0)
                    CustomParticles.GlyphAura(player.Center, GetEnigmaGradient(comboIntensity), 55f, 1);
                
                // Enhanced combo lighting
                float intensity = strikeCounter / 3f * 0.5f;
                Lighting.AddLight(player.Center, EnigmaPurple.ToVector3() * intensity);
            }
            
            // Base mystical lighting
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.06f) * 0.1f + 0.9f;
            Lighting.AddLight(player.Center, EnigmaGreen.ToVector3() * 0.35f * pulse);
        }
        
        public override bool CanUseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                Item.useStyle = ItemUseStyleID.Shoot;
                Item.useTime = 35;
                Item.useAnimation = 35;
                Item.noMelee = true;
                Item.noUseGraphic = true;
            }
            else
            {
                Item.useStyle = ItemUseStyleID.Swing;
                Item.useTime = 18;
                Item.useAnimation = 18;
                Item.noMelee = false;
                Item.noUseGraphic = false;
            }
            return base.CanUseItem(player);
        }
        
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // === ALT FIRE: TRI-BEAM CONVERGENCE ===
            if (player.altFunctionUse == 2)
            {
                Vector2 targetPos = Main.MouseWorld;
                float baseAngle = (targetPos - player.Center).ToRotation();
                
                SoundEngine.PlaySound(SoundID.Item72 with { Pitch = -0.4f, Volume = 0.9f }, player.Center);
                SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.2f, Volume = 0.7f }, player.Center);
                
                // Fire 3 converging void beams
                for (int i = 0; i < 3; i++)
                {
                    // Beams spread outward then converge
                    float spreadAngle = baseAngle + (i - 1) * MathHelper.ToRadians(25f);
                    Vector2 beamVel = spreadAngle.ToRotationVector2() * 12f;
                    
                    Projectile.NewProjectile(source, player.Center, beamVel,
                        ModContent.ProjectileType<VoidConvergenceBeam>(), damage * 2, knockback, player.whoAmI,
                        ai0: targetPos.X, ai1: targetPos.Y);
                    
                    // Spawn VFX at origin
                    CustomParticles.GenericFlare(player.Center, GetEnigmaGradient((float)i / 3f), 0.6f, 18);
                }
                
                // Spawn the convergence point projectile that will create the explosion
                Projectile.NewProjectile(source, targetPos, Vector2.Zero,
                    ModContent.ProjectileType<VoidConvergencePoint>(), damage * 5, knockback * 2f, player.whoAmI);
                
                // Origin burst VFX
                UnifiedVFX.EnigmaVariations.Impact(player.Center, 1.0f);
                CustomParticles.GlyphBurst(player.Center, EnigmaPurple, 6, 4f);
                ThemedParticles.EnigmaMusicNoteBurst(player.Center, 4, 3f);
                
                return false;
            }
            
            // === NORMAL ATTACK: Spiral projectiles ===
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
            if (Main.rand.NextBool(8))
            {
                Vector2 notePos = hitbox.Center.ToVector2() + Main.rand.NextVector2Circular(15f, 15f);
                ThemedParticles.EnigmaMusicNotes(notePos, 1, 20f);
            }
            
            if (Main.rand.NextBool(4))
            {
                Vector2 pos = new Vector2(
                    Main.rand.Next(hitbox.Left, hitbox.Right),
                    Main.rand.Next(hitbox.Top, hitbox.Bottom));
                    
                float progress = Main.rand.NextFloat();
                Color trailColor = GetEnigmaGradient(progress);
                CustomParticles.GenericFlare(pos, trailColor * 0.8f, 0.35f, 14);
                
                if (Main.rand.NextBool(10))
                    CustomParticles.Glyph(pos, EnigmaPurple * 0.6f, 0.22f, -1);
            }
        }
        
        public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<ParadoxBrand>(), 360);
            var brandNPC = target.GetGlobalNPC<ParadoxBrandNPC>();
            brandNPC.AddParadoxStack(target, 1);
            
            strikeCounter++;
            comboTimer = ComboResetTime;
            
            // === BUILD VOID CHARGE ===
            voidChargeStacks = Math.Min(voidChargeStacks + 1, MaxVoidChargeStacks);
            
            // Check for Void Collapse trigger
            if (voidChargeStacks >= MaxVoidChargeStacks)
            {
                TriggerVoidCollapse(player, target);
                voidChargeStacks = 0;
            }
            
            // === TONED DOWN HIT EFFECT - Still mysterious but not overwhelming ===
            // Simple fractal burst - reduced count and brightness
            for (int i = 0; i < 5; i++)
            {
                float angle = MathHelper.TwoPi * i / 5f;
                Vector2 offset = angle.ToRotationVector2() * 28f;
                CustomParticles.GenericFlare(target.Center + offset, GetEnigmaGradient((float)i / 5f) * 0.75f, 0.4f, 16);
            }
            
            // Central flash
            CustomParticles.GenericFlare(target.Center, EnigmaGreen * 0.7f, 0.5f, 14);
            
            // Simple halo
            CustomParticles.HaloRing(target.Center, EnigmaPurple * 0.6f, 0.35f, 14);
            
            // Single glyph accent (instead of circle)
            if (Main.rand.NextBool(2))
                CustomParticles.Glyph(target.Center + new Vector2(0, -20f), EnigmaPurple * 0.6f, 0.3f, -1);
            
            // Show paradox stack count
            int stacks = brandNPC.paradoxStacks;
            if (stacks > 0)
            {
                CustomParticles.GlyphStack(target.Center + new Vector2(0, -35f), EnigmaGreen * 0.6f, stacks, 0.3f);
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
        
        /// <summary>
        /// VOID COLLAPSE - Massive explosion when Void Charge reaches maximum!
        /// Similar to TheUnresolvedCadence's Paradox Collapse but with unique Void visuals.
        /// </summary>
        private void TriggerVoidCollapse(Player player, NPC triggerTarget)
        {
            SoundEngine.PlaySound(SoundID.Item122 with { Pitch = -0.3f, Volume = 1.2f }, player.Center);
            SoundEngine.PlaySound(SoundID.Item14 with { Pitch = -0.6f, Volume = 0.9f }, player.Center);
            SoundEngine.PlaySound(SoundID.Item71 with { Pitch = -0.4f, Volume = 0.8f }, player.Center);
            
            float collapseRadius = 350f; // Damage radius
            int collapseDamage = Item.damage * 4;
            
            // === PHASE 1: CENTRAL VOID IMPLOSION ===
            CustomParticles.GenericFlare(player.Center, Color.White, 1.4f, 30);
            CustomParticles.GenericFlare(player.Center, EnigmaGreen, 1.1f, 25);
            CustomParticles.GenericFlare(player.Center, EnigmaPurple, 0.9f, 22);
            
            // === PHASE 2: EXPANDING VOID HALOS ===
            for (int ring = 0; ring < 5; ring++)
            {
                float progress = ring / 5f;
                Color ringColor = Color.Lerp(EnigmaPurple, EnigmaGreen, progress) * 0.8f;
                CustomParticles.HaloRing(player.Center, ringColor, 0.5f + ring * 0.25f, 20 + ring * 5);
            }
            
            // === PHASE 3: SPIRAL GALAXY BURST ===
            for (int arm = 0; arm < 6; arm++)
            {
                float armAngle = MathHelper.TwoPi * arm / 6f;
                for (int point = 0; point < 6; point++)
                {
                    float spiralAngle = armAngle + point * 0.35f;
                    float spiralRadius = 30f + point * 25f;
                    Vector2 spiralPos = player.Center + spiralAngle.ToRotationVector2() * spiralRadius;
                    float gradientProgress = (arm * 6 + point) / 36f;
                    CustomParticles.GenericFlare(spiralPos, GetEnigmaGradient(gradientProgress), 0.5f, 22);
                }
            }
            
            // === PHASE 4: GLYPH CIRCLES AT MULTIPLE RADII ===
            CustomParticles.GlyphCircle(player.Center, EnigmaPurple, count: 8, radius: 60f, rotationSpeed: 0.1f);
            CustomParticles.GlyphCircle(player.Center, EnigmaGreen, count: 10, radius: 100f, rotationSpeed: -0.08f);
            CustomParticles.GlyphCircle(player.Center, EnigmaDeepPurple, count: 12, radius: 140f, rotationSpeed: 0.06f);
            
            // === PHASE 5: GLYPH TOWER + BURST ===
            CustomParticles.GlyphTower(player.Center, EnigmaPurple, layers: 4, baseScale: 0.55f);
            CustomParticles.GlyphBurst(player.Center, EnigmaGreen, count: 12, speed: 7f);
            
            // === PHASE 6: WATCHING EYES EXPLODE OUTWARD ===
            CustomParticles.EnigmaEyeExplosion(player.Center, EnigmaPurple, 6, 5f);
            
            // === PHASE 7: PARTICLE EXPLOSION ===
            for (int i = 0; i < 24; i++)
            {
                float angle = MathHelper.TwoPi * i / 24f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(8f, 14f);
                var glow = new GenericGlowParticle(player.Center, vel, GetEnigmaGradient((float)i / 24f),
                    0.5f, Main.rand.Next(25, 40), true);
                MagnumParticleHandler.SpawnParticle(glow);
            }
            
            // === PHASE 8: MUSIC NOTES CASCADE ===
            ThemedParticles.EnigmaMusicNoteBurst(player.Center, 10, 6f);
            
            // === DEAL DAMAGE TO ALL ENEMIES IN RADIUS ===
            foreach (NPC npc in Main.ActiveNPCs)
            {
                if (npc.friendly) continue;
                
                float dist = Vector2.Distance(npc.Center, player.Center);
                if (dist <= collapseRadius)
                {
                    float falloff = 1f - (dist / collapseRadius) * 0.4f;
                    int finalDamage = (int)(collapseDamage * falloff);
                    npc.SimpleStrikeNPC(finalDamage, player.direction, true, 15f);
                    npc.AddBuff(ModContent.BuffType<ParadoxBrand>(), 480);
                    npc.GetGlobalNPC<ParadoxBrandNPC>().AddParadoxStack(npc, 3);
                    
                    // Lightning to each enemy
                    MagnumVFX.DrawFractalLightning(player.Center, npc.Center, EnigmaGreen, 10, 25f, 3, 0.35f);
                    
                    // Impact at each enemy
                    CustomParticles.GenericFlare(npc.Center, EnigmaGreen, 0.6f, 18);
                    CustomParticles.HaloRing(npc.Center, EnigmaPurple * 0.7f, 0.4f, 15);
                }
            }
            
            // Lighting
            Lighting.AddLight(player.Center, EnigmaGreen.ToVector3() * 1.5f);
        }
        
        private void TriggerParadoxDetonation(Player player, NPC target, ParadoxBrandNPC brandNPC)
        {
            SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.2f, Volume = 0.8f }, target.Center);
            SoundEngine.PlaySound(SoundID.Item14 with { Pitch = -0.5f, Volume = 0.6f }, target.Center);
            
            brandNPC.paradoxStacks = 0;
            
            float explosionRadius = 50f; // Reduced from 200f (75% reduction)
            
            // === TONED DOWN EXPLOSION - Still impactful but not blinding ===
            // Central flares - reduced scale and brightness
            CustomParticles.GenericFlare(target.Center, EnigmaGreen * 0.8f, 0.7f, 20);
            CustomParticles.GenericFlare(target.Center, EnigmaPurple * 0.7f, 0.6f, 18);
            
            // === SIMPLE SPIRAL BURST - 4 arms instead of 8, fewer points ===
            for (int arm = 0; arm < 4; arm++)
            {
                float armAngle = MathHelper.TwoPi * arm / 4f + Main.GameUpdateCount * 0.04f;
                for (int point = 0; point < 4; point++)
                {
                    float spiralAngle = armAngle + point * 0.4f;
                    float spiralRadius = 10f + point * 10f; // Reduced from 25f + point * 20f
                    Vector2 spiralPos = target.Center + spiralAngle.ToRotationVector2() * spiralRadius;
                    float gradientProgress = (arm * 4 + point) / 16f;
                    CustomParticles.GenericFlare(spiralPos, GetEnigmaGradient(gradientProgress) * 0.8f, 0.35f, 18);
                }
            }
            
            // === EXPANDING HALOS - Cleaner visual ===
            for (int ring = 0; ring < 3; ring++)
            {
                Color ringColor = Color.Lerp(EnigmaPurple, EnigmaGreen, ring / 3f) * 0.6f;
                CustomParticles.HaloRing(target.Center, ringColor, 0.4f + ring * 0.2f, 18 + ring * 5);
            }
            
            // === WATCHING EYES - Just 3, meaningfully placed ===
            CustomParticles.EnigmaEyeFormation(target.Center, EnigmaGreen * 0.8f, 3, 25f); // Reduced from 60f
            
            // === GLYPHS - Simple burst ===
            CustomParticles.GlyphBurst(target.Center, EnigmaPurple * 0.7f, count: 4, speed: 2f); // Reduced count and speed
            
            // === MUSIC NOTES - Moderate cascade ===
            ThemedParticles.EnigmaMusicNoteBurst(target.Center, 5, 3f); // Reduced from 8, 5f
            
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
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            
            // Switch to additive blending for ethereal effects
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, 
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            // Get particle textures for unique visuals
            Texture2D eyeTex = CustomParticleSystem.RandomEnigmaEye().Value;
            Texture2D glyphTex = CustomParticleSystem.RandomGlyph().Value;
            Texture2D sparkleTex = CustomParticleSystem.RandomPrismaticSparkle().Value;
            
            // === DRAW SPIRAL TRAIL with glyphs and sparkles ===
            if (ProjectileID.Sets.TrailCacheLength[Projectile.type] > 0)
            {
                for (int i = 0; i < Projectile.oldPos.Length; i++)
                {
                    if (Projectile.oldPos[i] == Vector2.Zero) continue;
                    float trailProgress = (float)i / Projectile.oldPos.Length;
                    float trailAlpha = (1f - trailProgress) * 0.7f;
                    float trailScale = (1f - trailProgress * 0.5f) * 0.35f;
                    Vector2 trailPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                    
                    Color trailColor = Color.Lerp(EnigmaGreenFlame, EnigmaPurple, trailProgress);
                    spriteBatch.Draw(sparkleTex, trailPos, null, trailColor * trailAlpha, Projectile.rotation + i * 0.3f, 
                        sparkleTex.Size() / 2f, trailScale, SpriteEffects.None, 0f);
                    
                    // Add glyph at trail intervals
                    if (i % 3 == 0)
                    {
                        spriteBatch.Draw(glyphTex, trailPos, null, EnigmaPurple * trailAlpha * 0.6f, 
                            Main.GameUpdateCount * 0.05f + i, glyphTex.Size() / 2f, trailScale * 0.7f, SpriteEffects.None, 0f);
                    }
                }
            }
            
            // === DRAW ORBITING SPARKLES around core ===
            for (int i = 0; i < 5; i++)
            {
                float orbitAngle = Main.GameUpdateCount * 0.12f + MathHelper.TwoPi * i / 5f;
                float orbitRadius = 14f + (float)Math.Sin(Main.GameUpdateCount * 0.1f + i) * 5f;
                Vector2 orbitPos = drawPos + orbitAngle.ToRotationVector2() * orbitRadius;
                Color orbitColor = Color.Lerp(EnigmaPurple, EnigmaGreen, (float)i / 5f) * 0.75f;
                float orbitScale = 0.2f + (float)Math.Sin(Main.GameUpdateCount * 0.15f + i * 0.8f) * 0.05f;
                spriteBatch.Draw(sparkleTex, orbitPos, null, orbitColor, orbitAngle, sparkleTex.Size() / 2f, orbitScale, SpriteEffects.None, 0f);
            }
            
            // === DRAW CENTRAL GLYPH (rotating arcane symbol) ===
            float glyphRotation = Main.GameUpdateCount * 0.06f;
            spriteBatch.Draw(glyphTex, drawPos, null, EnigmaPurple * 0.85f, glyphRotation, glyphTex.Size() / 2f, 0.4f, SpriteEffects.None, 0f);
            spriteBatch.Draw(glyphTex, drawPos, null, EnigmaGreen * 0.6f, -glyphRotation * 0.7f, glyphTex.Size() / 2f, 0.25f, SpriteEffects.None, 0f);
            
            // === DRAW MYSTERIOUS CORE with sparkle texture ===
            spriteBatch.Draw(sparkleTex, drawPos, null, EnigmaGreenFlame * 0.9f, Projectile.rotation, sparkleTex.Size() / 2f, 0.35f, SpriteEffects.None, 0f);
            
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
            
            // Trail particles - every 4 frames instead of 50% every frame
            if (Projectile.timeLeft % 4 == 0)
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
            
            // === REALITY WARP DISTORTION ===
            FateRealityDistortion.TriggerChromaticAberration(target.Center, 2.5f, 8);
            
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
            // === REALITY WARP ON DEATH ===
            FateRealityDistortion.TriggerChromaticAberration(Projectile.Center, 3.5f, 12);
            
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
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            
            float lifeProgress = 1f - (Projectile.timeLeft / 60f);
            float pulse = 0.8f + (float)Math.Sin(Main.GameUpdateCount * 0.15f) * 0.4f;
            float opacity = 1f - lifeProgress * 0.4f;
            float baseScale = 0.4f; // Reduced from 1.5f (75% reduction)
            
            // Switch to additive blending
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, 
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            Texture2D eyeTex = CustomParticleSystem.RandomEnigmaEye().Value;
            Texture2D glyphTex = CustomParticleSystem.RandomGlyph().Value;
            Texture2D sparkleTex = CustomParticleSystem.RandomPrismaticSparkle().Value;
            Texture2D flareTex = CustomParticleSystem.EnergyFlares[0].Value;
            
            // === MASSIVE OUTER GLYPH RING (reality warping boundary) ===
            int outerGlyphCount = 12;
            for (int i = 0; i < outerGlyphCount; i++)
            {
                float angle = Main.GameUpdateCount * 0.05f + MathHelper.TwoPi * i / outerGlyphCount;
                float radius = 80f * pulse * baseScale;
                Vector2 glyphPos = drawPos + angle.ToRotationVector2() * radius;
                Color glyphColor = Color.Lerp(EnigmaPurple, EnigmaGreen, (float)i / outerGlyphCount) * opacity * 0.8f;
                spriteBatch.Draw(glyphTex, glyphPos, null, glyphColor, -angle * 2f + Main.GameUpdateCount * 0.03f, 
                    glyphTex.Size() / 2f, 0.5f * pulse * baseScale, SpriteEffects.None, 0f);
            }
            
            // === INNER ROTATING GLYPH CIRCLE (faster, opposite direction) ===
            int innerGlyphCount = 8;
            for (int i = 0; i < innerGlyphCount; i++)
            {
                float angle = -Main.GameUpdateCount * 0.08f + MathHelper.TwoPi * i / innerGlyphCount;
                float radius = 45f * pulse * baseScale;
                Vector2 glyphPos = drawPos + angle.ToRotationVector2() * radius;
                Color glyphColor = Color.Lerp(EnigmaGreen, EnigmaPurple, (float)i / innerGlyphCount) * opacity * 0.7f;
                spriteBatch.Draw(glyphTex, glyphPos, null, glyphColor, angle * 1.5f, 
                    glyphTex.Size() / 2f, 0.4f * pulse * baseScale, SpriteEffects.None, 0f);
            }
            
            // === WATCHING EYES RING (all watching the center) ===
            int eyeCount = 6;
            for (int i = 0; i < eyeCount; i++)
            {
                float angle = Main.GameUpdateCount * 0.03f + MathHelper.TwoPi * i / eyeCount;
                float radius = 55f * pulse * baseScale;
                Vector2 eyePos = drawPos + angle.ToRotationVector2() * radius;
                Color eyeColor = Color.Lerp(EnigmaPurple, EnigmaGreenFlame, (float)i / eyeCount) * opacity * 0.9f;
                // Eyes look inward toward center
                float lookAngle = angle + MathHelper.Pi;
                spriteBatch.Draw(eyeTex, eyePos, null, eyeColor, lookAngle, 
                    eyeTex.Size() / 2f, 0.6f * baseScale, SpriteEffects.None, 0f);
            }
            
            // === SPARKLE ACCENTS ===
            for (int i = 0; i < 8; i++)
            {
                float angle = Main.GameUpdateCount * 0.1f + MathHelper.TwoPi * i / 8f;
                float radius = 25f * pulse * baseScale;
                Vector2 sparkPos = drawPos + angle.ToRotationVector2() * radius;
                Color sparkColor = Color.Lerp(EnigmaGreen, Color.White, 0.3f) * opacity * 0.65f;
                spriteBatch.Draw(sparkleTex, sparkPos, null, sparkColor, angle * 3f, 
                    sparkleTex.Size() / 2f, 0.35f * baseScale, SpriteEffects.None, 0f);
            }
            
            // === CENTRAL MASSIVE VOID EYE (the rift watching) ===
            spriteBatch.Draw(eyeTex, drawPos, null, EnigmaBlack * opacity, Main.GameUpdateCount * 0.01f, 
                eyeTex.Size() / 2f, 1.2f * pulse * baseScale, SpriteEffects.None, 0f);
            spriteBatch.Draw(eyeTex, drawPos, null, EnigmaPurple * opacity * 0.8f, -Main.GameUpdateCount * 0.02f, 
                eyeTex.Size() / 2f, 0.9f * pulse * baseScale, SpriteEffects.None, 0f);
            
            // === BRIGHT CENTRAL CORE ===
            spriteBatch.Draw(flareTex, drawPos, null, EnigmaGreenFlame * opacity * 0.9f, Main.GameUpdateCount * 0.04f, 
                flareTex.Size() / 2f, 0.7f * pulse * baseScale, SpriteEffects.None, 0f);
            spriteBatch.Draw(flareTex, drawPos, null, Color.White * opacity * 0.5f, -Main.GameUpdateCount * 0.05f, 
                flareTex.Size() / 2f, 0.4f * pulse * baseScale, SpriteEffects.None, 0f);
            
            // Restore normal blending
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, 
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
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
            
            // === PARADOX RIFT REALITY WARP ===
            FateRealityDistortion.TriggerChromaticAberration(target.Center, 4f, 15);
            
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
            
            float progress = 1f - (Projectile.timeLeft / 40f);
            float opacity = 1f - progress * 0.5f;
            float pulse = 1f + (float)Math.Sin(Main.GameUpdateCount * 0.2f) * 0.2f;
            
            // Switch to additive blending
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, 
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            Texture2D eyeTex = CustomParticleSystem.RandomEnigmaEye().Value;
            Texture2D glyphTex = CustomParticleSystem.RandomGlyph().Value;
            Texture2D sparkleTex = CustomParticleSystem.RandomPrismaticSparkle().Value;
            Texture2D flareTex = CustomParticleSystem.EnergyFlares[0].Value;
            
            Vector2 startPos = Projectile.Center - Main.screenPosition;
            Vector2 endPos = TearEnd - Main.screenPosition;
            Vector2 midPos = Vector2.Lerp(startPos, endPos, 0.5f);
            float tearLength = Vector2.Distance(startPos, endPos);
            float tearAngle = (endPos - startPos).ToRotation();
            
            // === DRAW THE TEAR LINE ITSELF (stretched flare) ===
            Vector2 tearScale = new Vector2(tearLength / flareTex.Width * 1.5f, 0.8f * pulse);
            spriteBatch.Draw(flareTex, midPos, null, EnigmaGreenFlame * opacity * 0.9f, tearAngle, 
                flareTex.Size() / 2f, tearScale, SpriteEffects.None, 0f);
            spriteBatch.Draw(flareTex, midPos, null, EnigmaPurple * opacity * 0.7f, tearAngle, 
                flareTex.Size() / 2f, tearScale * 0.6f, SpriteEffects.None, 0f);
            spriteBatch.Draw(flareTex, midPos, null, Color.White * opacity * 0.5f, tearAngle, 
                flareTex.Size() / 2f, tearScale * 0.3f, SpriteEffects.None, 0f);
            
            // === MASSIVE WATCHING EYES AT ENDPOINTS ===
            Vector2 startToEnd = (endPos - startPos).SafeNormalize(Vector2.Zero);
            float startEyeRot = startToEnd.ToRotation();
            float endEyeRot = (-startToEnd).ToRotation();
            
            // Big eyes at both ends
            spriteBatch.Draw(eyeTex, startPos, null, EnigmaGreen * opacity, startEyeRot, eyeTex.Size() / 2f, 1.0f * pulse, SpriteEffects.None, 0f);
            spriteBatch.Draw(eyeTex, endPos, null, EnigmaPurple * opacity, endEyeRot, eyeTex.Size() / 2f, 1.0f * pulse, SpriteEffects.None, 0f);
            
            // Glow halos around the eyes
            spriteBatch.Draw(flareTex, startPos, null, EnigmaGreen * opacity * 0.5f, 0f, flareTex.Size() / 2f, 0.8f * pulse, SpriteEffects.None, 0f);
            spriteBatch.Draw(flareTex, endPos, null, EnigmaPurple * opacity * 0.5f, 0f, flareTex.Size() / 2f, 0.8f * pulse, SpriteEffects.None, 0f);
            
            // === GLYPHS ALONG THE TEAR LINE (MORE AND LARGER) ===
            int glyphCount = 8;
            for (int i = 0; i < glyphCount; i++)
            {
                float t = (float)(i + 1) / (glyphCount + 1);
                Vector2 glyphPos = Vector2.Lerp(startPos, endPos, t);
                // Perpendicular offset for "torn" appearance
                Vector2 perpendicular = new Vector2(-startToEnd.Y, startToEnd.X);
                float offset = (float)Math.Sin(Main.GameUpdateCount * 0.15f + i * 1.2f) * 15f;
                glyphPos += perpendicular * offset;
                
                Color glyphColor = Color.Lerp(EnigmaGreen, EnigmaPurple, t) * opacity * 0.85f;
                float glyphRot = Main.GameUpdateCount * 0.06f + i * 0.8f;
                spriteBatch.Draw(glyphTex, glyphPos, null, glyphColor, glyphRot, glyphTex.Size() / 2f, 0.5f * pulse, SpriteEffects.None, 0f);
            }
            
            // === SPARKLES SCATTERED ALONG THE TEAR ===
            for (int i = 0; i < 12; i++)
            {
                float t = (float)i / 11f;
                Vector2 sparkPos = Vector2.Lerp(startPos, endPos, t);
                Vector2 perpendicular = new Vector2(-startToEnd.Y, startToEnd.X);
                float offset = (float)Math.Sin(Main.GameUpdateCount * 0.2f + i * 0.7f) * 20f;
                sparkPos += perpendicular * offset;
                
                Color sparkColor = Color.Lerp(EnigmaGreen, EnigmaPurple, t) * opacity * 0.6f;
                spriteBatch.Draw(sparkleTex, sparkPos, null, sparkColor, Main.GameUpdateCount * 0.1f + i, 
                    sparkleTex.Size() / 2f, 0.4f * pulse, SpriteEffects.None, 0f);
            }
            
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
    
    /// <summary>
    /// VOID CONVERGENCE BEAM - One of three beams that slowly converge on a target point
    /// Creates a trailing void beam effect as it travels toward the convergence point
    /// </summary>
    public class VoidConvergenceBeam : ModProjectile
    {
        private static readonly Color EnigmaBlack = new Color(15, 10, 20);
        private static readonly Color EnigmaDeepPurple = new Color(80, 20, 120);
        private static readonly Color EnigmaPurple = new Color(140, 60, 200);
        private static readonly Color EnigmaGreen = new Color(50, 220, 100);
        
        private Vector2 TargetPoint => new Vector2(Projectile.ai[0], Projectile.ai[1]);
        private List<Vector2> trailPositions = new List<Vector2>();
        private const int MaxTrailLength = 20;
        
        public override string Texture => "MagnumOpus/Assets/Particles/SoftGlow";
        
        private Color GetEnigmaGradient(float progress)
        {
            if (progress < 0.5f)
                return Color.Lerp(EnigmaBlack, EnigmaPurple, progress * 2f);
            else
                return Color.Lerp(EnigmaPurple, EnigmaGreen, (progress - 0.5f) * 2f);
        }
        
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 25;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        
        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 80;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 15;
            Projectile.extraUpdates = 1;
        }
        
        public override void AI()
        {
            // Store trail positions
            trailPositions.Insert(0, Projectile.Center);
            if (trailPositions.Count > MaxTrailLength)
                trailPositions.RemoveAt(trailPositions.Count - 1);
            
            // Slowly curve toward the target convergence point
            Vector2 toTarget = TargetPoint - Projectile.Center;
            float distToTarget = toTarget.Length();
            
            if (distToTarget < 30f)
            {
                // Reached convergence point
                Projectile.Kill();
                return;
            }
            
            // Gradually increase homing as we get closer
            float homingStrength = MathHelper.Lerp(0.03f, 0.15f, 1f - Math.Min(distToTarget / 600f, 1f));
            Vector2 targetDirection = toTarget.SafeNormalize(Vector2.UnitX);
            Vector2 currentDirection = Projectile.velocity.SafeNormalize(Vector2.UnitX);
            
            // Smoothly rotate toward target
            float currentAngle = currentDirection.ToRotation();
            float targetAngle = targetDirection.ToRotation();
            float newAngle = MathHelper.Lerp(currentAngle, targetAngle, homingStrength);
            
            float speed = Projectile.velocity.Length();
            Projectile.velocity = newAngle.ToRotationVector2() * speed;
            Projectile.rotation = Projectile.velocity.ToRotation();
            
            // === TRAIL VFX - Dense void energy ===
            if (Main.GameUpdateCount % 2 == 0)
            {
                float lifeProgress = 1f - (Projectile.timeLeft / 80f);
                Color trailColor = GetEnigmaGradient(lifeProgress);
                
                // Main glow trail
                CustomParticles.GenericFlare(Projectile.Center, trailColor * 0.9f, 0.45f, 14);
                
                // Wispy side particles
                Vector2 perpendicular = Projectile.velocity.SafeNormalize(Vector2.Zero).RotatedBy(MathHelper.PiOver2);
                Vector2 sideOffset = perpendicular * Main.rand.NextFloat(-12f, 12f);
                var glow = new GenericGlowParticle(Projectile.Center + sideOffset, -Projectile.velocity * 0.1f,
                    trailColor * 0.6f, 0.3f, 12, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }
            
            // Occasional glyphs in trail
            if (Main.GameUpdateCount % 8 == 0)
            {
                CustomParticles.GlyphTrail(Projectile.Center, Projectile.velocity, EnigmaPurple * 0.7f, 0.3f);
            }
            
            // Occasional eyes watching along the beam
            if (Main.GameUpdateCount % 15 == 0)
            {
                Vector2 eyeDir = (TargetPoint - Projectile.Center).SafeNormalize(Vector2.UnitX);
                CustomParticles.EnigmaEyeGaze(Projectile.Center, EnigmaGreen * 0.7f, 0.35f, eyeDir);
            }
            
            Lighting.AddLight(Projectile.Center, GetEnigmaGradient(0.5f).ToVector3() * 0.6f);
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            Texture2D flareTex = CustomParticleSystem.EnergyFlares[0].Value;
            Texture2D glyphTex = CustomParticleSystem.RandomGlyph().Value;
            
            // Draw trail
            if (Projectile.oldPos.Length > 0)
            {
                for (int i = 0; i < Projectile.oldPos.Length; i++)
                {
                    if (Projectile.oldPos[i] == Vector2.Zero) continue;
                    
                    float trailProgress = (float)i / Projectile.oldPos.Length;
                    float trailAlpha = (1f - trailProgress) * 0.8f;
                    float trailScale = (1f - trailProgress * 0.6f) * 0.5f;
                    Vector2 trailPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                    
                    Color trailColor = GetEnigmaGradient(trailProgress);
                    spriteBatch.Draw(flareTex, trailPos, null, trailColor * trailAlpha,
                        Projectile.oldRot[i], flareTex.Size() / 2f, trailScale, SpriteEffects.None, 0f);
                    
                    // Glyph accents along trail
                    if (i % 5 == 0)
                    {
                        spriteBatch.Draw(glyphTex, trailPos, null, EnigmaPurple * trailAlpha * 0.5f,
                            Main.GameUpdateCount * 0.05f + i, glyphTex.Size() / 2f, trailScale * 0.6f, SpriteEffects.None, 0f);
                    }
                }
            }
            
            // Draw main projectile
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            spriteBatch.Draw(flareTex, drawPos, null, EnigmaGreen * 0.9f, Projectile.rotation, flareTex.Size() / 2f, 0.6f, SpriteEffects.None, 0f);
            spriteBatch.Draw(flareTex, drawPos, null, Color.White * 0.6f, Projectile.rotation, flareTex.Size() / 2f, 0.3f, SpriteEffects.None, 0f);
            
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            return false;
        }
        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<ParadoxBrand>(), 300);
            target.GetGlobalNPC<ParadoxBrandNPC>().AddParadoxStack(target, 1);
            
            CustomParticles.GenericFlare(target.Center, EnigmaGreen, 0.5f, 14);
            CustomParticles.GlyphImpact(target.Center, EnigmaPurple, EnigmaGreen, 0.4f);
        }
        
        public override void OnKill(int timeLeft)
        {
            // Small dissipation effect
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f;
                Vector2 vel = angle.ToRotationVector2() * 3f;
                var glow = new GenericGlowParticle(Projectile.Center, vel, GetEnigmaGradient((float)i / 6f), 0.35f, 15, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }
        }
    }
    
    /// <summary>
    /// VOID CONVERGENCE POINT - The brilliant explosion when tri-beams converge
    /// Waits for beams to arrive, then ignites with massive VFX and spawns homing glyphs/eyes
    /// </summary>
    public class VoidConvergencePoint : ModProjectile
    {
        private static readonly Color EnigmaBlack = new Color(15, 10, 20);
        private static readonly Color EnigmaDeepPurple = new Color(80, 20, 120);
        private static readonly Color EnigmaPurple = new Color(140, 60, 200);
        private static readonly Color EnigmaGreen = new Color(50, 220, 100);
        
        private int chargeTimer = 0;
        private const int ChargeTime = 40; // Time for beams to arrive
        private bool hasIgnited = false;
        
        public override string Texture => "MagnumOpus/Assets/Particles/SoftGlow";
        
        private Color GetEnigmaGradient(float progress)
        {
            if (progress < 0.5f)
                return Color.Lerp(EnigmaBlack, EnigmaPurple, progress * 2f);
            else
                return Color.Lerp(EnigmaPurple, EnigmaGreen, (progress - 0.5f) * 2f);
        }
        
        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 30;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 180;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }
        
        public override void AI()
        {
            chargeTimer++;
            float chargeProgress = Math.Min(chargeTimer / (float)ChargeTime, 1f);
            
            // === PRE-IGNITION: Building energy at convergence point ===
            if (!hasIgnited)
            {
                // Pulsing target indicator
                if (Main.GameUpdateCount % 3 == 0)
                {
                    float pulse = 0.3f + chargeProgress * 0.7f;
                    CustomParticles.GenericFlare(Projectile.Center, EnigmaPurple * chargeProgress, pulse * 0.5f, 10);
                    
                    // Rotating glyph circle building up
                    if (chargeProgress > 0.3f)
                    {
                        float glyphAngle = Main.GameUpdateCount * 0.08f;
                        int glyphCount = (int)(4 + chargeProgress * 4);
                        for (int i = 0; i < glyphCount; i++)
                        {
                            float angle = glyphAngle + MathHelper.TwoPi * i / glyphCount;
                            float radius = 30f + chargeProgress * 20f;
                            Vector2 glyphPos = Projectile.Center + angle.ToRotationVector2() * radius;
                            CustomParticles.Glyph(glyphPos, GetEnigmaGradient(chargeProgress) * 0.6f, 0.25f + chargeProgress * 0.1f, -1);
                        }
                    }
                }
                
                // Particles converging inward
                if (Main.GameUpdateCount % 4 == 0 && chargeProgress > 0.2f)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        float angle = Main.rand.NextFloat() * MathHelper.TwoPi;
                        float dist = 60f + Main.rand.NextFloat(40f);
                        Vector2 startPos = Projectile.Center + angle.ToRotationVector2() * dist;
                        Vector2 vel = (Projectile.Center - startPos).SafeNormalize(Vector2.Zero) * 4f;
                        var converge = new GenericGlowParticle(startPos, vel, GetEnigmaGradient(Main.rand.NextFloat()) * 0.7f, 0.3f, 15, true);
                        MagnumParticleHandler.SpawnParticle(converge);
                    }
                }
                
                // Ignite when charge is complete
                if (chargeTimer >= ChargeTime)
                {
                    TriggerIgnition();
                    hasIgnited = true;
                }
                
                Lighting.AddLight(Projectile.Center, GetEnigmaGradient(chargeProgress).ToVector3() * chargeProgress * 0.8f);
            }
            else
            {
                // Post-ignition: lingering effect
                float postProgress = (chargeTimer - ChargeTime) / 60f;
                if (postProgress > 1f)
                {
                    Projectile.Kill();
                    return;
                }
                
                // Fading residual glow
                if (Main.GameUpdateCount % 4 == 0)
                {
                    float fade = 1f - postProgress;
                    CustomParticles.GenericFlare(Projectile.Center, EnigmaGreen * fade * 0.5f, 0.4f * fade, 10);
                }
                
                Lighting.AddLight(Projectile.Center, EnigmaGreen.ToVector3() * (1f - postProgress) * 0.5f);
            }
        }
        
        private void TriggerIgnition()
        {
            Player owner = Main.player[Projectile.owner];
            
            SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.3f, Volume = 1.1f }, Projectile.Center);
            SoundEngine.PlaySound(SoundID.Item14 with { Pitch = -0.4f, Volume = 0.9f }, Projectile.Center);
            SoundEngine.PlaySound(SoundID.Item71 with { Pitch = 0.1f, Volume = 0.8f }, Projectile.Center);
            
            // === PHASE 1: BRILLIANT CENTRAL FLASH ===
            CustomParticles.GenericFlare(Projectile.Center, Color.White, 1.8f, 30);
            CustomParticles.GenericFlare(Projectile.Center, EnigmaGreen, 1.5f, 28);
            CustomParticles.GenericFlare(Projectile.Center, EnigmaPurple, 1.2f, 25);
            
            // === PHASE 2: EXPANDING HALOS ===
            for (int ring = 0; ring < 6; ring++)
            {
                float progress = ring / 6f;
                Color ringColor = Color.Lerp(EnigmaGreen, EnigmaPurple, progress) * 0.85f;
                CustomParticles.HaloRing(Projectile.Center, ringColor, 0.5f + ring * 0.2f, 18 + ring * 4);
            }
            
            // === PHASE 3: FRACTAL BURST PATTERN ===
            for (int i = 0; i < 12; i++)
            {
                float angle = MathHelper.TwoPi * i / 12f;
                Vector2 offset = angle.ToRotationVector2() * 40f;
                CustomParticles.GenericFlare(Projectile.Center + offset, GetEnigmaGradient((float)i / 12f), 0.6f, 20);
                
                // Outer ring
                Vector2 outerOffset = angle.ToRotationVector2() * 70f;
                CustomParticles.GenericFlare(Projectile.Center + outerOffset, GetEnigmaGradient((float)i / 12f) * 0.7f, 0.45f, 18);
            }
            
            // === PHASE 4: GLYPH EXPLOSION ===
            CustomParticles.GlyphBurst(Projectile.Center, EnigmaPurple, 10, 6f);
            CustomParticles.GlyphBurst(Projectile.Center, EnigmaGreen, 8, 4f);
            CustomParticles.GlyphCircle(Projectile.Center, EnigmaDeepPurple, 12, 80f, 0.06f);
            
            // === PHASE 5: EYE EXPLOSION ===
            CustomParticles.EnigmaEyeExplosion(Projectile.Center, EnigmaGreen, 6, 5f);
            
            // === PHASE 6: PARTICLE STORM ===
            for (int i = 0; i < 30; i++)
            {
                float angle = MathHelper.TwoPi * i / 30f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(6f, 12f);
                var glow = new GenericGlowParticle(Projectile.Center, vel, GetEnigmaGradient((float)i / 30f), 0.45f, 25, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }
            
            // === PHASE 7: MUSIC NOTES CASCADE ===
            ThemedParticles.EnigmaMusicNoteBurst(Projectile.Center, 12, 7f);
            
            // === PHASE 8: SPAWN HOMING GLYPHS AND EYES ===
            Vector2 cursorTarget = Main.MouseWorld;
            
            // Spawn homing void seekers that swirl toward cursor
            for (int i = 0; i < 8; i++)
            {
                float spawnAngle = MathHelper.TwoPi * i / 8f;
                Vector2 spawnOffset = spawnAngle.ToRotationVector2() * 35f;
                Vector2 spawnVel = spawnAngle.ToRotationVector2() * 4f;
                
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center + spawnOffset, spawnVel,
                    ModContent.ProjectileType<VoidSeekerGlyph>(), Projectile.damage / 3, 2f, Projectile.owner,
                    ai0: cursorTarget.X, ai1: cursorTarget.Y);
            }
            
            // Spawn homing eye projectiles
            for (int i = 0; i < 4; i++)
            {
                float spawnAngle = MathHelper.TwoPi * i / 4f + MathHelper.PiOver4;
                Vector2 spawnOffset = spawnAngle.ToRotationVector2() * 50f;
                Vector2 spawnVel = spawnAngle.ToRotationVector2() * 3f;
                
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center + spawnOffset, spawnVel,
                    ModContent.ProjectileType<VoidSeekerEye>(), Projectile.damage / 2, 3f, Projectile.owner,
                    ai0: cursorTarget.X, ai1: cursorTarget.Y);
            }
            
            // === DEAL DAMAGE IN AREA ===
            float damageRadius = 150f;
            foreach (NPC npc in Main.ActiveNPCs)
            {
                if (npc.friendly || npc.immortal) continue;
                
                float dist = Vector2.Distance(npc.Center, Projectile.Center);
                if (dist <= damageRadius)
                {
                    float falloff = 1f - (dist / damageRadius) * 0.4f;
                    int finalDamage = (int)(Projectile.damage * falloff);
                    npc.SimpleStrikeNPC(finalDamage, owner.direction, true, 12f);
                    npc.AddBuff(ModContent.BuffType<ParadoxBrand>(), 420);
                    npc.GetGlobalNPC<ParadoxBrandNPC>().AddParadoxStack(npc, 2);
                    
                    // Lightning to each enemy
                    MagnumVFX.DrawFractalLightning(Projectile.Center, npc.Center, EnigmaGreen, 8, 20f, 3, 0.4f);
                    
                    CustomParticles.GenericFlare(npc.Center, EnigmaGreen, 0.6f, 16);
                    CustomParticles.HaloRing(npc.Center, EnigmaPurple * 0.7f, 0.4f, 14);
                }
            }
            
            // Reality distortion effect
            FateRealityDistortion.TriggerChromaticAberration(Projectile.Center, 6f, 20);
            
            Lighting.AddLight(Projectile.Center, EnigmaGreen.ToVector3() * 2f);
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            if (!hasIgnited)
            {
                SpriteBatch spriteBatch = Main.spriteBatch;
                float chargeProgress = Math.Min(chargeTimer / (float)ChargeTime, 1f);
                
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                    DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
                
                Texture2D flareTex = CustomParticleSystem.EnergyFlares[0].Value;
                Vector2 drawPos = Projectile.Center - Main.screenPosition;
                float pulse = 1f + (float)Math.Sin(Main.GameUpdateCount * 0.15f) * 0.2f;
                
                // Pulsing convergence point
                spriteBatch.Draw(flareTex, drawPos, null, EnigmaPurple * chargeProgress * 0.7f, 0f, flareTex.Size() / 2f, 0.8f * pulse * chargeProgress, SpriteEffects.None, 0f);
                spriteBatch.Draw(flareTex, drawPos, null, EnigmaGreen * chargeProgress * 0.5f, 0f, flareTex.Size() / 2f, 0.5f * pulse * chargeProgress, SpriteEffects.None, 0f);
                
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                    DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            }
            
            return false;
        }
        
        public override bool? CanDamage() => false; // Damage is handled in TriggerIgnition
    }
    
    /// <summary>
    /// VOID SEEKER GLYPH - Homing glyph projectile that swirls toward cursor position
    /// </summary>
    public class VoidSeekerGlyph : ModProjectile
    {
        private static readonly Color EnigmaBlack = new Color(15, 10, 20);
        private static readonly Color EnigmaPurple = new Color(140, 60, 200);
        private static readonly Color EnigmaGreen = new Color(50, 220, 100);
        
        private Vector2 TargetPos => new Vector2(Projectile.ai[0], Projectile.ai[1]);
        private float orbitAngle = 0f;
        private float orbitRadius = 0f;
        private int phase = 0; // 0 = spiral out, 1 = home to target
        
        public override string Texture => "MagnumOpus/Assets/Particles/SoftGlow";
        
        private Color GetEnigmaGradient(float progress)
        {
            if (progress < 0.5f)
                return Color.Lerp(EnigmaBlack, EnigmaPurple, progress * 2f);
            else
                return Color.Lerp(EnigmaPurple, EnigmaGreen, (progress - 0.5f) * 2f);
        }
        
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 15;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        
        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 2;
            Projectile.timeLeft = 180;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 20;
        }
        
        public override void OnSpawn(IEntitySource source)
        {
            orbitAngle = Projectile.velocity.ToRotation();
            orbitRadius = 20f;
        }
        
        public override void AI()
        {
            // Phase 0: Spiral outward briefly
            if (phase == 0)
            {
                orbitAngle += 0.15f;
                orbitRadius += 2f;
                
                if (orbitRadius > 80f)
                {
                    phase = 1;
                }
                
                // Spiral movement
                Vector2 spiralDir = orbitAngle.ToRotationVector2();
                Projectile.velocity = spiralDir * 6f;
            }
            // Phase 1: Home toward target/cursor
            else
            {
                Vector2 toTarget = TargetPos - Projectile.Center;
                float distToTarget = toTarget.Length();
                
                if (distToTarget > 20f)
                {
                    // Strong homing with slight spiral
                    float targetAngle = toTarget.ToRotation();
                    float currentAngle = Projectile.velocity.ToRotation();
                    
                    // Add slight spiral motion while homing
                    float spiralOffset = (float)Math.Sin(Main.GameUpdateCount * 0.2f + Projectile.whoAmI) * 0.1f;
                    float newAngle = MathHelper.Lerp(currentAngle, targetAngle + spiralOffset, 0.12f);
                    
                    float speed = Math.Min(Projectile.velocity.Length() + 0.3f, 14f);
                    Projectile.velocity = newAngle.ToRotationVector2() * speed;
                }
            }
            
            Projectile.rotation = Main.GameUpdateCount * 0.1f;
            
            // Trail particles
            if (Main.GameUpdateCount % 3 == 0)
            {
                float progress = 1f - (Projectile.timeLeft / 180f);
                CustomParticles.GenericFlare(Projectile.Center, GetEnigmaGradient(progress) * 0.7f, 0.3f, 10);
            }
            
            Lighting.AddLight(Projectile.Center, EnigmaPurple.ToVector3() * 0.4f);
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            Texture2D glyphTex = CustomParticleSystem.RandomGlyph().Value;
            Texture2D flareTex = CustomParticleSystem.EnergyFlares[0].Value;
            
            // Draw trail
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;
                
                float trailProgress = (float)i / Projectile.oldPos.Length;
                float trailAlpha = (1f - trailProgress) * 0.7f;
                float trailScale = (1f - trailProgress * 0.5f) * 0.3f;
                Vector2 trailPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                
                Color trailColor = GetEnigmaGradient(trailProgress);
                spriteBatch.Draw(flareTex, trailPos, null, trailColor * trailAlpha,
                    Projectile.rotation + i * 0.2f, flareTex.Size() / 2f, trailScale, SpriteEffects.None, 0f);
            }
            
            // Draw main glyph
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            spriteBatch.Draw(glyphTex, drawPos, null, EnigmaPurple * 0.9f, Projectile.rotation, glyphTex.Size() / 2f, 0.5f, SpriteEffects.None, 0f);
            spriteBatch.Draw(flareTex, drawPos, null, EnigmaGreen * 0.5f, 0f, flareTex.Size() / 2f, 0.35f, SpriteEffects.None, 0f);
            
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            return false;
        }
        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<ParadoxBrand>(), 240);
            target.GetGlobalNPC<ParadoxBrandNPC>().AddParadoxStack(target, 1);
            
            CustomParticles.GenericFlare(target.Center, EnigmaGreen, 0.5f, 14);
            CustomParticles.GlyphImpact(target.Center, EnigmaPurple, EnigmaGreen, 0.4f);
        }
        
        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 5; i++)
            {
                float angle = MathHelper.TwoPi * i / 5f;
                Vector2 vel = angle.ToRotationVector2() * 2.5f;
                var glow = new GenericGlowParticle(Projectile.Center, vel, GetEnigmaGradient((float)i / 5f), 0.3f, 12, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }
            CustomParticles.GlyphBurst(Projectile.Center, EnigmaPurple, 3, 2f);
        }
    }
    
    /// <summary>
    /// VOID SEEKER EYE - Homing eye projectile that watches and hunts toward cursor position
    /// </summary>
    public class VoidSeekerEye : ModProjectile
    {
        private static readonly Color EnigmaBlack = new Color(15, 10, 20);
        private static readonly Color EnigmaPurple = new Color(140, 60, 200);
        private static readonly Color EnigmaGreen = new Color(50, 220, 100);
        
        private Vector2 TargetPos => new Vector2(Projectile.ai[0], Projectile.ai[1]);
        private float spiralPhase = 0f;
        
        public override string Texture => "MagnumOpus/Assets/Particles/SoftGlow";
        
        private Color GetEnigmaGradient(float progress)
        {
            if (progress < 0.5f)
                return Color.Lerp(EnigmaBlack, EnigmaPurple, progress * 2f);
            else
                return Color.Lerp(EnigmaPurple, EnigmaGreen, (progress - 0.5f) * 2f);
        }
        
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 12;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        
        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 3;
            Projectile.timeLeft = 200;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 25;
        }
        
        public override void AI()
        {
            spiralPhase += 0.08f;
            
            Vector2 toTarget = TargetPos - Projectile.Center;
            float distToTarget = toTarget.Length();
            
            if (distToTarget > 30f)
            {
                // Home toward target with swirling motion
                float targetAngle = toTarget.ToRotation();
                float currentAngle = Projectile.velocity.ToRotation();
                
                // Swirl offset creates the swirling motion
                float swirlOffset = (float)Math.Sin(spiralPhase) * 0.3f;
                float newAngle = MathHelper.Lerp(currentAngle, targetAngle + swirlOffset, 0.08f);
                
                float speed = Math.Min(Projectile.velocity.Length() + 0.2f, 12f);
                Projectile.velocity = newAngle.ToRotationVector2() * speed;
            }
            
            // Eye always looks toward target
            Projectile.rotation = (TargetPos - Projectile.Center).ToRotation();
            
            // Trail particles
            if (Main.GameUpdateCount % 4 == 0)
            {
                CustomParticles.GenericFlare(Projectile.Center, EnigmaGreen * 0.6f, 0.35f, 12);
                
                // Occasional glyph accent
                if (Main.rand.NextBool(3))
                {
                    CustomParticles.Glyph(Projectile.Center, EnigmaPurple * 0.5f, 0.2f, -1);
                }
            }
            
            Lighting.AddLight(Projectile.Center, EnigmaGreen.ToVector3() * 0.5f);
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            Texture2D eyeTex = CustomParticleSystem.RandomEnigmaEye().Value;
            Texture2D flareTex = CustomParticleSystem.EnergyFlares[0].Value;
            
            // Draw trail
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;
                
                float trailProgress = (float)i / Projectile.oldPos.Length;
                float trailAlpha = (1f - trailProgress) * 0.6f;
                float trailScale = (1f - trailProgress * 0.4f) * 0.4f;
                Vector2 trailPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                
                Color trailColor = GetEnigmaGradient(trailProgress);
                spriteBatch.Draw(flareTex, trailPos, null, trailColor * trailAlpha,
                    0f, flareTex.Size() / 2f, trailScale, SpriteEffects.None, 0f);
            }
            
            // Draw main eye - looking toward target
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            spriteBatch.Draw(eyeTex, drawPos, null, EnigmaGreen * 0.9f, Projectile.rotation, eyeTex.Size() / 2f, 0.6f, SpriteEffects.None, 0f);
            
            // Glow around eye
            spriteBatch.Draw(flareTex, drawPos, null, EnigmaPurple * 0.4f, 0f, flareTex.Size() / 2f, 0.5f, SpriteEffects.None, 0f);
            
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            return false;
        }
        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<ParadoxBrand>(), 300);
            target.GetGlobalNPC<ParadoxBrandNPC>().AddParadoxStack(target, 1);
            
            CustomParticles.GenericFlare(target.Center, EnigmaGreen, 0.6f, 16);
            CustomParticles.EnigmaEyeImpact(target.Center, target.Center, EnigmaPurple, 0.5f);
            CustomParticles.GlyphImpact(target.Center, EnigmaPurple, EnigmaGreen, 0.45f);
        }
        
        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f;
                Vector2 vel = angle.ToRotationVector2() * 3f;
                var glow = new GenericGlowParticle(Projectile.Center, vel, GetEnigmaGradient((float)i / 6f), 0.35f, 14, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }
            
            // Eye dispersal effect
            CustomParticles.EnigmaEyeExplosion(Projectile.Center, EnigmaGreen * 0.7f, 3, 3f);
        }
    }
}
