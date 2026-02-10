using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.VFX;

namespace MagnumOpus.Common.Systems
{
    /// <summary>
    /// Automatically applies Ark of the Cosmos-style swing trails to MagnumOpus melee weapons.
    /// 
    /// This uses triangle strip mesh rendering with UV-mapped noise textures,
    /// NOT discrete fog particles. The result is buttery smooth, flowing trails
    /// that match the visual quality of Calamity's Ark of the Cosmos.
    /// </summary>
    public class ArkSwingTrailGlobalItem : GlobalItem
    {
        // Track which players have active swings
        private static Dictionary<int, SwingContext> _activeSwings = new Dictionary<int, SwingContext>();
        
        private class SwingContext
        {
            public Item Item;
            public int Timer;
            public float StartAngle;
            public float CurrentAngle;
            public Color PrimaryColor;
            public Color SecondaryColor;
            public float BladeLength;
            public string Theme;
        }
        
        public override bool AppliesToEntity(Item item, bool lateInstantiation)
        {
            // MASTER TOGGLE: When disabled, this global system does nothing
            // Each melee weapon implements its own unique swing trail VFX instead
            if (!VFX.VFXMasterToggle.GlobalSystemsEnabled)
                return false;
            
            // Exclude debug weapons
            if (VFX.VFXExclusionHelper.ShouldExcludeItem(item)) return false;
            
            // Apply to melee weapons
            return item.damage > 0 && item.DamageType == DamageClass.Melee && !item.noMelee;
        }
        
        public override void UseItemFrame(Item item, Player player)
        {
            // Check if this is a MagnumOpus weapon by namespace/mod
            if (item.ModItem == null) return;
            string fullName = item.ModItem.GetType().FullName ?? "";
            if (!fullName.Contains("MagnumOpus")) return;
            
            // Get theme colors based on item's namespace
            var (primary, secondary, theme) = GetThemeColors(fullName);
            
            // Calculate blade length (approximate from item size)
            float bladeLength = MathF.Max(item.width, item.height) * item.scale * 0.8f;
            bladeLength = MathF.Max(bladeLength, 50f); // Minimum length
            
            // Track swing
            int playerIndex = player.whoAmI;
            if (!_activeSwings.TryGetValue(playerIndex, out var ctx))
            {
                ctx = new SwingContext();
                _activeSwings[playerIndex] = ctx;
            }
            
            // Update or initialize swing context
            if (ctx.Item != item)
            {
                ctx.Item = item;
                ctx.Timer = 0;
                ctx.StartAngle = player.itemRotation;
            }
            
            ctx.CurrentAngle = player.itemRotation;
            ctx.PrimaryColor = primary;
            ctx.SecondaryColor = secondary;
            ctx.BladeLength = bladeLength;
            ctx.Theme = theme;
            ctx.Timer++;
            
            // Calculate blade tip position
            Vector2 bladeTip = player.Center + new Vector2(
                MathF.Cos(player.itemRotation) * bladeLength * player.direction,
                MathF.Sin(player.itemRotation) * bladeLength);
            
            // Update swing trail system using PROPER triangle strip rendering
            // ArkSwingTrail now uses BlendState.Additive for all passes - no more black blobs!
            ArkSwingTrail.UpdateSwingTrail(
                player, 
                bladeLength, 
                primary, 
                secondary, 
                width: GetTrailWidth(fullName),
                theme: theme);
        }
        
        public override void UseItemHitbox(Item item, Player player, ref Rectangle hitbox, ref bool noHitbox)
        {
            // When hitbox is checked, trail is active
            // Nothing special needed here
        }
        
        public override void UseAnimation(Item item, Player player)
        {
            // Animation frame - could add extra effects here
        }
        
        /// <summary>
        /// Called when player stops using an item.
        /// </summary>
        public override void HoldItem(Item item, Player player)
        {
            int playerIndex = player.whoAmI;
            
            // Check if swing ended
            if (player.itemAnimation == 0)
            {
                if (_activeSwings.TryGetValue(playerIndex, out var ctx) && ctx.Item == item)
                {
                    // End the ArkSwingTrail (triangle strip rendering)
                    ArkSwingTrail.EndSwingTrail(player);
                    _activeSwings.Remove(playerIndex);
                }
            }
        }
        
        #region Theme Detection
        
        private static (Color primary, Color secondary, string theme) GetThemeColors(string fullName)
        {
            // Check namespace for theme detection
            if (fullName.Contains("LaCampanella"))
            {
                // BRIGHT colors for proper additive blending
                return (
                    new Color(255, 140, 50),   // Bright orange
                    new Color(255, 220, 100),  // Bright gold/yellow (not black!)
                    "LaCampanella"
                );
            }
            if (fullName.Contains("Eroica"))
            {
                return (
                    new Color(200, 50, 50),    // Scarlet
                    new Color(255, 200, 80),   // Gold
                    "Eroica"
                );
            }
            if (fullName.Contains("SwanLake"))
            {
                // Rainbow shifting for Swan Lake
                float hue = (Main.GameUpdateCount * 0.01f) % 1f;
                Color rainbow = Main.hslToRgb(hue, 1f, 0.85f);
                return (
                    Color.White,
                    rainbow,
                    "SwanLake"
                );
            }
            if (fullName.Contains("MoonlightSonata") || fullName.Contains("Moonlight"))
            {
                // BRIGHT colors for proper additive blending
                return (
                    new Color(150, 100, 220),  // Bright purple (additive-friendly)
                    new Color(135, 206, 250),  // Light blue
                    "MoonlightSonata"
                );
            }
            if (fullName.Contains("Enigma"))
            {
                return (
                    new Color(140, 60, 200),   // Purple
                    new Color(50, 220, 100),   // Green flame
                    "EnigmaVariations"
                );
            }
            if (fullName.Contains("Fate"))
            {
                // BRIGHT colors for proper additive blending (no black blobs!)
                return (
                    new Color(255, 120, 180),  // Bright pink (additive-friendly)
                    new Color(180, 100, 220),  // Bright purple (additive-friendly)
                    "Fate"
                );
            }
            if (fullName.Contains("Spring"))
            {
                return (
                    new Color(255, 180, 200),  // Pink
                    new Color(180, 255, 180),  // Pale green
                    "Spring"
                );
            }
            if (fullName.Contains("Summer"))
            {
                return (
                    new Color(255, 140, 50),   // Orange
                    new Color(255, 215, 0),    // Gold
                    "Summer"
                );
            }
            if (fullName.Contains("Autumn"))
            {
                return (
                    new Color(200, 150, 80),   // Amber
                    new Color(180, 50, 50),    // Crimson
                    "Autumn"
                );
            }
            if (fullName.Contains("Winter"))
            {
                return (
                    new Color(150, 200, 255),  // Ice blue
                    Color.White,
                    "Winter"
                );
            }
            
            // Default
            return (
                new Color(200, 200, 255),
                new Color(100, 150, 255),
                null
            );
        }
        
        private static float GetTrailWidth(string fullName)
        {
            // Some themes get wider trails
            if (fullName.Contains("SwanLake")) return 40f;
            if (fullName.Contains("Fate")) return 45f;
            if (fullName.Contains("LaCampanella")) return 35f;
            return 30f;
        }
        
        #endregion
        
        public override void Unload()
        {
            _activeSwings?.Clear();
            _activeSwings = null;
        }
    }
}
