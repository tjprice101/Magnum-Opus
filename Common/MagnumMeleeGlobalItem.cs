using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Common
{
    /// <summary>
    /// GlobalItem that applies the unique MagnumOpus swing pattern to all mod melee weapons.
    /// Pattern: Swing Down → Swing Up → 360° Spin with Particle Burst
    /// 
    /// This automatically detects the weapon's theme (based on rarity or namespace) and applies
    /// the appropriate themed swing config.
    /// </summary>
    public class MagnumMeleeGlobalItem : GlobalItem
    {
        // Cache theme configs by item type
        private static Dictionary<int, MagnumSwingConfig> cachedConfigs = new Dictionary<int, MagnumSwingConfig>();
        
        /// <summary>
        /// Determines if this item should use the MagnumOpus swing system.
        /// Returns true for all mod melee weapons that use the swing style.
        /// </summary>
        private static bool ShouldUseMagnumSwing(Item item)
        {
            // Must be from our mod
            if (item.ModItem == null || item.ModItem.Mod.Name != "MagnumOpus")
                return false;
            
            // Must be melee
            if (item.DamageType != DamageClass.Melee && !item.DamageType.CountsAsClass(DamageClass.Melee))
                return false;
            
            // Must use swing style
            if (item.useStyle != ItemUseStyleID.Swing)
                return false;
            
            // Skip tools
            if (item.pick > 0 || item.axe > 0 || item.hammer > 0)
                return false;
            
            return true;
        }
        
        /// <summary>
        /// Gets or creates the appropriate swing config for the item based on its theme.
        /// </summary>
        private static MagnumSwingConfig GetSwingConfig(Item item)
        {
            if (cachedConfigs.TryGetValue(item.type, out var cached))
                return cached;
            
            MagnumSwingConfig config = DetectThemeConfig(item);
            cachedConfigs[item.type] = config;
            return config;
        }
        
        /// <summary>
        /// Detects the item's theme based on namespace or rarity and returns the appropriate config.
        /// </summary>
        private static MagnumSwingConfig DetectThemeConfig(Item item)
        {
            if (item.ModItem == null)
                return MagnumSwingConfigs.SwanLake; // Default fallback
            
            string fullName = item.ModItem.GetType().FullName ?? "";
            
            // Check namespace for theme
            if (fullName.Contains("SwanLake"))
                return MagnumSwingConfigs.SwanLake;
            if (fullName.Contains("LaCampanella"))
                return MagnumSwingConfigs.LaCampanella;
            if (fullName.Contains("Eroica"))
                return MagnumSwingConfigs.Eroica;
            if (fullName.Contains("MoonlightSonata") || fullName.Contains("Moonlight"))
                return MagnumSwingConfigs.MoonlightSonata;
            if (fullName.Contains("EnigmaVariations") || fullName.Contains("Enigma"))
                return MagnumSwingConfigs.EnigmaVariations;
            if (fullName.Contains("Fate"))
                return MagnumSwingConfigs.Fate;
            
            // Check rarity as fallback
            if (item.rare == ModContent.RarityType<SwanRarity>())
                return MagnumSwingConfigs.SwanLake;
            if (item.rare == ModContent.RarityType<LaCampanellaRarity>())
                return MagnumSwingConfigs.LaCampanella;
            if (item.rare == ModContent.RarityType<EroicaRarity>())
                return MagnumSwingConfigs.Eroica;
            if (item.rare == ModContent.RarityType<MoonlightSonataRarity>())
                return MagnumSwingConfigs.MoonlightSonata;
            if (item.rare == ModContent.RarityType<EnigmaVariationsRarity>())
                return MagnumSwingConfigs.EnigmaVariations;
            if (item.rare == ModContent.RarityType<FateRarity>())
                return MagnumSwingConfigs.Fate;
            
            // Default to Swan Lake (most neutral visually)
            return MagnumSwingConfigs.SwanLake;
        }
        
        public override bool? UseItem(Item item, Player player)
        {
            if (!ShouldUseMagnumSwing(item))
                return base.UseItem(item, player);
            
            var meleePlayer = player.GetModPlayer<MagnumMeleePlayer>();
            
            // If we're at the start of a swing (player.itemAnimation just started)
            if (player.itemAnimation == player.itemAnimationMax - 1)
            {
                // Start the MagnumOpus swing pattern
                MagnumSwingConfig config = GetSwingConfig(item);
                meleePlayer.StartSwing(item, config, player.itemAnimationMax);
            }
            
            return base.UseItem(item, player);
        }
        
        public override void UseItemFrame(Item item, Player player)
        {
            if (!ShouldUseMagnumSwing(item))
            {
                base.UseItemFrame(item, player);
                return;
            }
            
            var meleePlayer = player.GetModPlayer<MagnumMeleePlayer>();
            
            // Update the swing visual effects
            if (meleePlayer.IsSwinging)
            {
                meleePlayer.UpdateSwing();
                
                // Modify player arm rotation based on our swing angle
                float swingAngle = meleePlayer.GetCurrentSwingAngle();
                
                // Convert swing angle to arm composite rotation
                // This creates the custom swing animation
                float armAngle = swingAngle;
                if (player.direction == -1)
                {
                    armAngle = MathHelper.Pi - armAngle;
                }
                
                // Apply custom rotation to player composite arms
                player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, armAngle - MathHelper.PiOver2);
            }
            
            base.UseItemFrame(item, player);
        }
        
        public override void HoldItem(Item item, Player player)
        {
            if (!ShouldUseMagnumSwing(item))
            {
                base.HoldItem(item, player);
                return;
            }
            
            var meleePlayer = player.GetModPlayer<MagnumMeleePlayer>();
            
            // Continue updating swing if in progress
            if (meleePlayer.IsSwinging && player.itemAnimation > 0)
            {
                meleePlayer.UpdateSwing();
            }
            
            // Add subtle ambient particles while holding to indicate combo state
            if (meleePlayer.GetSwingDirection() > 0 && Main.rand.NextBool(30))
            {
                // Show visual feedback that we're in a combo
                MagnumSwingConfig config = GetSwingConfig(item);
                Color comboColor = Color.Lerp(config.PrimaryColor, config.SecondaryColor, Main.rand.NextFloat());
                
                Vector2 particlePos = player.Center + Main.rand.NextVector2Circular(20f, 30f);
                CustomParticles.GenericFlare(particlePos, comboColor * 0.5f, 0.2f, 12);
            }
            
            base.HoldItem(item, player);
        }
        
        public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
        {
            if (!ShouldUseMagnumSwing(item))
            {
                base.ModifyTooltips(item, tooltips);
                return;
            }
            
            // Add tooltip explaining the swing pattern
            int insertIndex = tooltips.FindIndex(t => t.Name == "Damage") + 1;
            if (insertIndex > 0)
            {
                var config = GetSwingConfig(item);
                var swingTip = new TooltipLine(Mod, "MagnumSwing", "Unique 3-swing combo: Down → Up → 360° Burst")
                {
                    OverrideColor = Color.Lerp(Color.White, config.PrimaryColor, 0.5f)
                };
                tooltips.Insert(insertIndex, swingTip);
            }
        }
        
    }
}
