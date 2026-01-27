using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Content.SwanLake.ResonanceEnergies;
using MagnumOpus.Content.SwanLake.HarmonicCores;

namespace MagnumOpus.Content.SwanLake.Accessories
{
    /// <summary>
    /// Swan Lake theme color constants - graceful elegance, monochrome with rainbow shimmer
    /// </summary>
    public static class SwanColors
    {
        public static readonly Color White = new Color(255, 255, 255);
        public static readonly Color Black = new Color(20, 20, 30);
        public static readonly Color Silver = new Color(220, 225, 235);
        public static readonly Color IcyBlue = new Color(180, 220, 255);
        
        /// <summary>
        /// Get a rainbow color based on a cycling offset
        /// </summary>
        public static Color GetRainbow(float offset)
        {
            float hue = (Main.GameUpdateCount * 0.01f + offset) % 1f;
            return Main.hslToRgb(hue, 1f, 0.85f);
        }
    }

    #region Plume of Elegance
    /// <summary>
    /// Phase 3 Swan Lake Tier 1 Accessory - Post-Moon Lord
    /// +10% all damage
    /// +15% movement speed
    /// Dodging leaves rainbow afterimages
    /// Graceful feathers drift around player
    /// </summary>
    public class PlumeOfElegance : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.accessory = true;
            Item.value = Item.sellPrice(gold: 25);
            Item.rare = ModContent.RarityType<SwanRarity>();
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            // +10% all damage
            player.GetDamage(DamageClass.Generic) += 0.10f;
            
            // +15% movement speed
            player.moveSpeed += 0.15f;
            player.runAcceleration *= 1.15f;
            
            // Enable rainbow afterimage mechanic
            player.GetModPlayer<PlumeOfElegancePlayer>().plumeEquipped = true;
            
            // Ambient VFX - graceful feathers and prismatic sparkles
            if (!hideVisual)
            {
                // Drifting feathers
                if (Main.rand.NextBool(15))
                {
                    Vector2 offset = new Vector2(Main.rand.NextFloat(-30f, 30f), -20f);
                    Vector2 velocity = new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(0.5f, 1.5f));
                    Color featherColor = Main.rand.NextBool() ? SwanColors.White : SwanColors.Black;
                    
                    CustomParticles.SwanFeatherDrift(player.Center + offset, featherColor, 0.4f);
                }
                
                // Prismatic sparkles
                if (Main.rand.NextBool(12))
                {
                    Vector2 sparklePos = player.Center + Main.rand.NextVector2Circular(35f, 35f);
                    Color rainbowColor = SwanColors.GetRainbow(Main.rand.NextFloat());
                    
                    var sparkle = new SparkleParticle(sparklePos, Main.rand.NextVector2Circular(1f, 1f),
                        rainbowColor, 0.35f, 20);
                    MagnumParticleHandler.SpawnParticle(sparkle);
                }
                
                // Occasional white/black contrast flares
                if (Main.rand.NextBool(20))
                {
                    float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                    Vector2 pos = player.Center + angle.ToRotationVector2() * 25f;
                    Color flareColor = Main.rand.NextBool() ? SwanColors.White : SwanColors.Black;
                    CustomParticles.GenericFlare(pos, flareColor * 0.5f, 0.2f, 12);
                }
            }
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<ResonantCoreOfSwanLake>(3)
                .AddIngredient<SwansResonanceEnergy>(8)
                .AddIngredient(ItemID.LunarBar, 10)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "DamageBoost", "+10% damage")
            {
                OverrideColor = SwanColors.White
            });
            tooltips.Add(new TooltipLine(Mod, "SpeedBoost", "+15% movement speed")
            {
                OverrideColor = SwanColors.IcyBlue
            });
            tooltips.Add(new TooltipLine(Mod, "Afterimage", "Dodging leaves rainbow afterimages")
            {
                OverrideColor = SwanColors.GetRainbow(0f)
            });
            tooltips.Add(new TooltipLine(Mod, "Flavor", "'Grace in motion, elegance personified'")
            {
                OverrideColor = SwanColors.Silver
            });
        }
    }

    public class PlumeOfElegancePlayer : ModPlayer
    {
        public bool plumeEquipped;
        private Vector2 lastPosition;
        private int afterimageTimer;

        public override void ResetEffects()
        {
            plumeEquipped = false;
        }

        public override void PostUpdate()
        {
            if (!plumeEquipped) return;
            
            afterimageTimer++;
            
            // Detect fast movement/dodging (dashing, teleporting, etc.)
            float movementSpeed = Vector2.Distance(Player.Center, lastPosition);
            
            // If moving very fast (dash/dodge), create rainbow afterimages
            if (movementSpeed > 15f && afterimageTimer > 3)
            {
                afterimageTimer = 0;
                CreateRainbowAfterimage();
            }
            
            lastPosition = Player.Center;
        }

        private void CreateRainbowAfterimage()
        {
            // Create a trail of rainbow particles where player was
            for (int i = 0; i < 6; i++)
            {
                float progress = i / 6f;
                Color rainbowColor = SwanColors.GetRainbow(progress);
                Vector2 offset = Main.rand.NextVector2Circular(8f, 8f);
                
                CustomParticles.GenericFlare(Player.Center + offset, rainbowColor, 0.4f, 15);
                
                // Sparkles in the trail
                var sparkle = new SparkleParticle(Player.Center + offset, 
                    -Player.velocity * 0.2f + Main.rand.NextVector2Circular(2f, 2f),
                    rainbowColor, 0.3f, 18);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
            
            // White/black contrast feathers
            CustomParticles.SwanFeatherDrift(Player.Center, SwanColors.White, 0.35f);
            CustomParticles.SwanFeatherDrift(Player.Center, SwanColors.Black, 0.3f);
            
            // Prismatic halo
            CustomParticles.HaloRing(Player.Center, SwanColors.GetRainbow(0f), 0.3f, 12);
        }
    }
    #endregion

    #region Swan's Chromatic Diadem
    /// <summary>
    /// Phase 3 Swan Lake Tier 2 Accessory - Post-Moon Lord (Combination)
    /// Includes all Plume of Elegance benefits maximized:
    /// +16% all damage
    /// +20% movement speed
    /// +10% dodge chance
    /// Perfect dodges trigger "Dying Swan" - a massive prismatic burst that damages nearby enemies
    /// Constant prismatic aura with drifting feathers
    /// </summary>
    public class SwansChromaticDiadem : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 36;
            Item.height = 36;
            Item.accessory = true;
            Item.value = Item.sellPrice(gold: 50);
            Item.rare = ModContent.RarityType<SwanRarity>();
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            // +16% all damage
            player.GetDamage(DamageClass.Generic) += 0.16f;
            
            // +8% crit chance
            player.GetCritChance(DamageClass.Generic) += 8;
            
            // +20% movement speed
            player.moveSpeed += 0.20f;
            player.runAcceleration *= 1.20f;
            
            // +10% dodge chance (through Brain of Confusion-like effect)
            player.GetModPlayer<SwansChromaticDiademPlayer>().diademEquipped = true;
            
            // Elaborate ambient VFX - prismatic aura
            if (!hideVisual)
            {
                // Constant feather waltz
                if (Main.rand.NextBool(8))
                {
                    float angle = Main.GameUpdateCount * 0.03f + Main.rand.NextFloat(MathHelper.TwoPi);
                    float radius = 35f + (float)Math.Sin(Main.GameUpdateCount * 0.05f) * 10f;
                    Vector2 featherPos = player.Center + angle.ToRotationVector2() * radius;
                    
                    // Alternate black and white feathers
                    Color featherColor = (Main.GameUpdateCount / 20) % 2 == 0 ? SwanColors.White : SwanColors.Black;
                    CustomParticles.SwanFeatherDrift(featherPos, featherColor, 0.4f);
                }
                
                // Prismatic sparkle ring
                if (Main.GameUpdateCount % 6 == 0)
                {
                    int sparkleCount = 5;
                    for (int i = 0; i < sparkleCount; i++)
                    {
                        float angle = Main.GameUpdateCount * 0.02f + MathHelper.TwoPi * i / sparkleCount;
                        Vector2 sparklePos = player.Center + angle.ToRotationVector2() * 45f;
                        Color rainbowColor = SwanColors.GetRainbow((float)i / sparkleCount);
                        
                        var sparkle = new SparkleParticle(sparklePos, Vector2.Zero, rainbowColor, 0.35f, 12);
                        MagnumParticleHandler.SpawnParticle(sparkle);
                    }
                }
                
                // Rainbow glow particles flowing around
                if (Main.rand.NextBool(10))
                {
                    Vector2 startPos = player.Center + Main.rand.NextVector2Circular(50f, 50f);
                    Vector2 velocity = (player.Center - startPos).SafeNormalize(Vector2.Zero) * 2f;
                    Color glowColor = SwanColors.GetRainbow(Main.rand.NextFloat());
                    
                    var glow = new GenericGlowParticle(startPos, velocity, glowColor * 0.6f, 0.3f, 20, true);
                    MagnumParticleHandler.SpawnParticle(glow);
                }
                
                // Monochrome contrast flares
                if (Main.rand.NextBool(18))
                {
                    for (int i = 0; i < 2; i++)
                    {
                        float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                        Vector2 pos = player.Center + angle.ToRotationVector2() * Main.rand.NextFloat(20f, 40f);
                        Color flareColor = i == 0 ? SwanColors.White : SwanColors.Black;
                        CustomParticles.GenericFlare(pos, flareColor * 0.4f, 0.22f, 14);
                    }
                }
            }
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<PlumeOfElegance>()
                .AddIngredient<HarmonicCoreOfSwanLake>(2)
                .AddIngredient<SwansResonanceEnergy>(15)
                .AddIngredient<RemnantOfSwansHarmony>(5)
                .AddIngredient(ItemID.FragmentStardust, 10)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "DamageBoost", "+16% damage, +8% critical strike chance")
            {
                OverrideColor = SwanColors.White
            });
            tooltips.Add(new TooltipLine(Mod, "SpeedBoost", "+20% movement speed")
            {
                OverrideColor = SwanColors.IcyBlue
            });
            tooltips.Add(new TooltipLine(Mod, "DodgeChance", "+10% dodge chance")
            {
                OverrideColor = SwanColors.GetRainbow(0.3f)
            });
            tooltips.Add(new TooltipLine(Mod, "DyingSwan", "Perfect dodges trigger 'Dying Swan' - prismatic damage burst")
            {
                OverrideColor = SwanColors.GetRainbow(0.6f)
            });
            tooltips.Add(new TooltipLine(Mod, "Flavor", "'The final dance of the dying swan - beautiful, tragic, eternal'")
            {
                OverrideColor = SwanColors.Silver
            });
        }
    }

    public class SwansChromaticDiademPlayer : ModPlayer
    {
        public bool diademEquipped;
        private int dodgeCooldown;
        private Vector2 lastPosition;
        private int afterimageTimer;

        public override void ResetEffects()
        {
            diademEquipped = false;
        }

        public override void PostUpdate()
        {
            if (!diademEquipped) return;
            
            if (dodgeCooldown > 0)
                dodgeCooldown--;
            
            afterimageTimer++;
            
            // Rainbow afterimages on fast movement
            float movementSpeed = Vector2.Distance(Player.Center, lastPosition);
            if (movementSpeed > 12f && afterimageTimer > 2)
            {
                afterimageTimer = 0;
                CreateEnhancedRainbowAfterimage();
            }
            
            lastPosition = Player.Center;
        }

        public override bool FreeDodge(Player.HurtInfo info)
        {
            if (!diademEquipped) return false;
            if (dodgeCooldown > 0) return false;
            
            // 10% dodge chance
            if (Main.rand.NextFloat() < 0.10f)
            {
                dodgeCooldown = 120; // 2 second cooldown
                TriggerDyingSwanBurst();
                return true;
            }
            
            return false;
        }

        private void CreateEnhancedRainbowAfterimage()
        {
            // Enhanced rainbow trail
            for (int i = 0; i < 8; i++)
            {
                float progress = i / 8f;
                Color rainbowColor = SwanColors.GetRainbow(progress);
                Vector2 offset = Main.rand.NextVector2Circular(10f, 10f);
                
                CustomParticles.GenericFlare(Player.Center + offset, rainbowColor, 0.45f, 16);
                
                var sparkle = new SparkleParticle(Player.Center + offset,
                    -Player.velocity * 0.15f + Main.rand.NextVector2Circular(2f, 2f),
                    rainbowColor, 0.35f, 20);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
            
            // Feather burst
            CustomParticles.SwanFeatherDrift(Player.Center, SwanColors.White, 0.4f);
            CustomParticles.SwanFeatherDrift(Player.Center + Main.rand.NextVector2Circular(10f, 10f), SwanColors.Black, 0.35f);
            
            // Prismatic halo
            CustomParticles.HaloRing(Player.Center, SwanColors.GetRainbow(Main.rand.NextFloat()), 0.35f, 14);
        }

        private void TriggerDyingSwanBurst()
        {
            // "DYING SWAN" - Massive prismatic explosion
            
            // Phase 1: Central white flash
            CustomParticles.GenericFlare(Player.Center, Color.White, 1.8f, 35);
            
            // Phase 2: Rainbow core burst
            for (int i = 0; i < 12; i++)
            {
                float hue = (float)i / 12f;
                Color rainbowColor = Main.hslToRgb(hue, 1f, 0.85f);
                CustomParticles.GenericFlare(Player.Center, rainbowColor, 0.9f - i * 0.05f, 28 - i);
            }
            
            // Phase 3: Cascading prismatic halos
            for (int ring = 0; ring < 8; ring++)
            {
                Color ringColor = SwanColors.GetRainbow(ring / 8f);
                CustomParticles.HaloRing(Player.Center, ringColor, 0.4f + ring * 0.15f, 18 + ring * 3);
            }
            
            // Phase 4: Black and white contrast rings
            for (int i = 0; i < 6; i++)
            {
                Color contrastColor = i % 2 == 0 ? SwanColors.White : SwanColors.Black;
                CustomParticles.HaloRing(Player.Center, contrastColor * 0.7f, 0.3f + i * 0.12f, 15 + i * 2);
            }
            
            // Phase 5: Feather explosion
            for (int i = 0; i < 16; i++)
            {
                float angle = MathHelper.TwoPi * i / 16f;
                Vector2 featherPos = Player.Center + angle.ToRotationVector2() * 30f;
                Color featherColor = i % 2 == 0 ? SwanColors.White : SwanColors.Black;
                CustomParticles.SwanFeatherDrift(featherPos, featherColor, 0.5f);
            }
            
            // Phase 6: Rainbow sparkle spiral
            for (int layer = 0; layer < 3; layer++)
            {
                for (int i = 0; i < 10; i++)
                {
                    float angle = MathHelper.TwoPi * i / 10f + layer * 0.2f;
                    float radius = 40f + layer * 25f;
                    Vector2 sparklePos = Player.Center + angle.ToRotationVector2() * radius;
                    Color sparkleColor = SwanColors.GetRainbow((float)i / 10f + layer * 0.33f);
                    
                    var sparkle = new SparkleParticle(sparklePos,
                        angle.ToRotationVector2() * (3f + layer * 2f),
                        sparkleColor, 0.45f, 22);
                    MagnumParticleHandler.SpawnParticle(sparkle);
                }
            }
            
            // Phase 7: Particle explosion
            CustomParticles.ExplosionBurst(Player.Center, SwanColors.White, 18, 10f);
            CustomParticles.ExplosionBurst(Player.Center, SwanColors.Black, 12, 8f);
            
            // Deal damage to nearby enemies (base 100 + 10% of player's max damage stat)
            if (Main.myPlayer == Player.whoAmI)
            {
                int baseDamage = 100 + (int)(Player.GetTotalDamage(DamageClass.Generic).ApplyTo(100) * 0.5f);
                float damageRadius = 250f;
                
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (npc.active && !npc.friendly && !npc.immortal && !npc.dontTakeDamage)
                    {
                        float distance = Vector2.Distance(npc.Center, Player.Center);
                        if (distance <= damageRadius)
                        {
                            // Damage falls off with distance
                            float damageMult = 1f - (distance / damageRadius) * 0.5f;
                            int finalDamage = (int)(baseDamage * damageMult);
                            
                            npc.SimpleStrikeNPC(finalDamage, 0, false, 0, null, false, 0, true);
                            
                            // VFX on hit
                            CustomParticles.GenericFlare(npc.Center, SwanColors.GetRainbow(Main.rand.NextFloat()), 0.5f, 15);
                            
                            // Feather burst on enemy
                            CustomParticles.SwanFeatherDrift(npc.Center, SwanColors.White, 0.4f);
                        }
                    }
                }
            }
            
            // Brief invincibility frames
            Player.immune = true;
            Player.immuneTime = 30;
            
            // Screen shake
            MagnumScreenEffects.AddScreenShake(10f);
            
            // Sound effect
            Terraria.Audio.SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.5f, Volume = 1.2f }, Player.Center);
        }
    }
    #endregion
}
