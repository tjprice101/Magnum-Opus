using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.VFX;

namespace MagnumOpus.Common
{
    /// <summary>
    /// GlobalItem that applies the MagnumOpus full rotation swing to all mod melee weapons.
    /// Every swing is a full 360° rotation with a large themed particle trail.
    /// The weapon sprite itself rotates around the player.
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
            
            // Skip projectile-based melee weapons (they use held projectiles for animations)
            // These weapons have their own custom swing systems
            if (item.noMelee)
                return false;
            
            return true;
        }
        
        /// <summary>
        /// AppliesToEntity - ensures the GlobalItem only applies to relevant items.
        /// </summary>
        public override bool AppliesToEntity(Item entity, bool lateInstantiation)
        {
            // Only apply to potential melee swing weapons
            if (entity.ModItem == null || entity.ModItem.Mod.Name != "MagnumOpus")
                return false;
            
            // Exclude debug weapons
            if (VFXExclusionHelper.ShouldExcludeItem(entity))
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
        
        public override void HoldItem(Item item, Player player)
        {
            if (!ShouldUseMagnumSwing(item))
            {
                base.HoldItem(item, player);
                return;
            }
            
            var meleePlayer = player.GetModPlayer<MagnumMeleePlayer>();
            MagnumSwingConfig config = GetSwingConfig(item);
            
            // Tell the ModPlayer which config to use for this weapon
            meleePlayer.SetHeldWeaponConfig(item, config);
            
            base.HoldItem(item, player);
        }
        
        /// <summary>
        /// Override the weapon's rotation during swing to make it do a full 360° rotation.
        /// This is called every frame when the player is using the item.
        /// </summary>
        public override void UseStyle(Item item, Player player, Rectangle heldItemFrame)
        {
            if (!ShouldUseMagnumSwing(item))
            {
                base.UseStyle(item, player, heldItemFrame);
                return;
            }
            
            var meleePlayer = player.GetModPlayer<MagnumMeleePlayer>();
            
            if (meleePlayer.IsSwinging)
            {
                // Get the current swing angle from our system
                float swingAngle = meleePlayer.GetCurrentSwingAngle();
                
                // Set the item's rotation to match our swing angle
                // Add Pi/4 (45 degrees) offset because sword sprites are usually diagonal
                player.itemRotation = swingAngle + MathHelper.PiOver4 * player.direction;
                
                // Flip the sprite based on the current angle to look correct during full rotation
                // When the weapon is on the left side of the player, flip it
                float normalizedAngle = MathHelper.WrapAngle(swingAngle);
                bool shouldFlip = normalizedAngle > MathHelper.PiOver2 || normalizedAngle < -MathHelper.PiOver2;
                
                // Adjust item location to orbit around the player
                float weaponLength = Math.Max(item.width, item.height) * 0.6f;
                Vector2 offset = swingAngle.ToRotationVector2() * weaponLength;
                
                // Position the item relative to the player's hand position
                player.itemLocation = player.Center + offset;
                
                // Set the arm to follow the weapon
                float armRotation = swingAngle - MathHelper.PiOver2;
                player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, armRotation);
            }
            
            base.UseStyle(item, player, heldItemFrame);
        }
        
        /// <summary>
        /// Modify the hitbox to follow the weapon's rotation around the player.
        /// </summary>
        public override void UseItemHitbox(Item item, Player player, ref Rectangle hitbox, ref bool noHitbox)
        {
            if (!ShouldUseMagnumSwing(item))
            {
                base.UseItemHitbox(item, player, ref hitbox, ref noHitbox);
                return;
            }
            
            var meleePlayer = player.GetModPlayer<MagnumMeleePlayer>();
            
            if (meleePlayer.IsSwinging)
            {
                // Get the current swing angle
                float swingAngle = meleePlayer.GetCurrentSwingAngle();
                
                // Calculate hitbox position based on swing angle
                float weaponLength = Math.Max(item.width, item.height) * 1.2f;
                Vector2 tipPos = player.Center + swingAngle.ToRotationVector2() * weaponLength;
                
                // Create a hitbox around the weapon tip area
                int hitboxSize = (int)(Math.Max(item.width, item.height) * 0.8f);
                hitbox = new Rectangle(
                    (int)(tipPos.X - hitboxSize / 2),
                    (int)(tipPos.Y - hitboxSize / 2),
                    hitboxSize,
                    hitboxSize
                );
            }
            
            base.UseItemHitbox(item, player, ref hitbox, ref noHitbox);
        }
        
        /// <summary>
        /// Melee effects hook - kept for future use.
        /// The melee smear system (MeleeSmearEffect.cs) handles the visual swing trails.
        /// </summary>
        public override void MeleeEffects(Item item, Player player, Rectangle hitbox)
        {
            if (!ShouldUseMagnumSwing(item))
            {
                base.MeleeEffects(item, player, hitbox);
                return;
            }
            
            // The MeleeSmearEffect system handles visual trails.
            // No additional dust/sparkle effect here to avoid visual clutter.
            
            base.MeleeEffects(item, player, hitbox);
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
                var swingTip = new TooltipLine(Mod, "MagnumSwing", "Full rotation swing with particle trail")
                {
                    OverrideColor = Color.Lerp(Color.White, config.PrimaryColor, 0.5f)
                };
                tooltips.Insert(insertIndex, swingTip);
            }
        }
        
    }
}
