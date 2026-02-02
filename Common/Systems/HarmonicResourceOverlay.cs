using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using MagnumOpus.Content.Common.Consumables;

namespace MagnumOpus.Common.Systems
{
    /// <summary>
    /// Custom resource overlay that draws rainbow-shimmering hearts and arcane mana stars
    /// for players who have used CrystallizedHarmony and ArcaneHarmonicPrism consumables.
    /// Based on ExampleMod's VanillaLifeOverlay and VanillaManaOverlay patterns.
    /// </summary>
    public class HarmonicResourceOverlay : ModResourceOverlay
    {
        // Cache for comparing vanilla assets
        private Dictionary<string, Asset<Texture2D>> vanillaAssetCache = new();

        /// <summary>
        /// Intercepts heart/mana drawing and overlays rainbow/arcane effects on transformed resources.
        /// </summary>
        public override void PostDrawResource(ResourceOverlayDrawContext context)
        {
            if (context.texture == null) return;

            Player player = Main.LocalPlayer;
            Asset<Texture2D> asset = context.texture;

            // Check for health hearts (Classic and Fancy styles)
            string fancyFolder = "Images/UI/PlayerResourceSets/FancyClassic/";
            
            // Get transformed counts
            var harmonyPlayer = player.GetModPlayer<CrystallizedHarmonyPlayer>();
            int transformedHearts = harmonyPlayer.crystallizedHarmonyUses;
            
            var prismPlayer = player.GetModPlayer<ArcaneHarmonicPrismPlayer>();
            int transformedStars = prismPlayer.arcaneHarmonicPrismUses;

            // Health resources are drawn in groups of two in some modes
            // For Classic hearts: resourceNumber maps directly to heart index
            // Hearts 0-19 represent health, we transform based on uses (1 use = 1 heart transformed)
            
            // Check if this is a heart being drawn
            if (asset == TextureAssets.Heart || asset == TextureAssets.Heart2)
            {
                // Classic hearts - resourceNumber is the heart index (0-based)
                if (transformedHearts > 0 && context.resourceNumber < transformedHearts)
                {
                    DrawRainbowHeartOverlay(context);
                }
            }
            else if (CompareAssets(asset, fancyFolder + "Heart_Fill") || CompareAssets(asset, fancyFolder + "Heart_Fill_B"))
            {
                // Fancy hearts
                if (transformedHearts > 0 && context.resourceNumber < transformedHearts * 2) // Fancy uses 2 resources per heart
                {
                    DrawRainbowHeartOverlay(context);
                }
            }
            
            // Check if this is a mana star being drawn
            if (asset == TextureAssets.Mana)
            {
                // Classic mana stars
                if (transformedStars > 0 && context.resourceNumber < transformedStars)
                {
                    DrawArcaneManaOverlay(context);
                }
            }
            else if (CompareAssets(asset, fancyFolder + "Star_Fill"))
            {
                // Fancy mana stars
                if (transformedStars > 0 && context.resourceNumber < transformedStars)
                {
                    DrawArcaneManaOverlay(context);
                }
            }
        }

        /// <summary>
        /// Helper method to compare asset paths (from ExampleMod pattern).
        /// </summary>
        private bool CompareAssets(Asset<Texture2D> existingAsset, string compareAssetPath)
        {
            if (!vanillaAssetCache.TryGetValue(compareAssetPath, out var asset))
                asset = vanillaAssetCache[compareAssetPath] = Main.Assets.Request<Texture2D>(compareAssetPath);

            return existingAsset == asset;
        }

        /// <summary>
        /// Draws a rainbow-shimmering overlay on transformed hearts.
        /// </summary>
        private void DrawRainbowHeartOverlay(ResourceOverlayDrawContext context)
        {
            SpriteBatch spriteBatch = context.SpriteBatch;
            Texture2D texture = context.texture.Value;
            
            // Calculate rainbow hue based on time and heart position for wave effect
            float timeOffset = Main.GameUpdateCount * 0.03f;
            float positionOffset = context.resourceNumber * 0.15f;
            float hue = (timeOffset + positionOffset) % 1f;
            
            // Create shimmering rainbow color
            Color rainbowColor = Main.hslToRgb(hue, 0.85f, 0.65f);
            
            // Add subtle pulsing
            float pulse = 1f + (float)Math.Sin(Main.GameUpdateCount * 0.1f + context.resourceNumber * 0.5f) * 0.08f;
            
            // Draw additive glow layer behind
            Color glowColor = rainbowColor * 0.4f;
            glowColor.A = 0; // Additive blending
            
            Vector2 glowOffset = new Vector2(-2, -2);
            spriteBatch.Draw(
                texture,
                context.position + glowOffset,
                context.source,
                glowColor,
                context.rotation,
                context.origin,
                context.scale * pulse * 1.15f,
                context.effects,
                0f
            );
            
            // Draw rainbow-tinted heart overlay with additive blend
            Color overlayColor = rainbowColor * 0.6f;
            overlayColor.A = 0; // For additive blending effect
            
            spriteBatch.Draw(
                texture,
                context.position,
                context.source,
                overlayColor,
                context.rotation,
                context.origin,
                context.scale * pulse,
                context.effects,
                0f
            );
            
            // Occasional sparkle effect
            if (Main.rand.NextBool(120))
            {
                Vector2 sparklePos = context.position + new Vector2(
                    Main.rand.NextFloat(-5, 15),
                    Main.rand.NextFloat(-5, 15)
                );
                
                // Spawn a tiny dust sparkle
                Dust dust = Dust.NewDustDirect(
                    sparklePos, 
                    2, 2, 
                    Terraria.ID.DustID.RainbowMk2, 
                    0f, -0.5f, 
                    100, 
                    rainbowColor, 
                    0.4f
                );
                dust.noGravity = true;
                dust.velocity *= 0.3f;
            }
        }

        /// <summary>
        /// Draws an arcane-shimmering overlay on transformed mana stars.
        /// </summary>
        private void DrawArcaneManaOverlay(ResourceOverlayDrawContext context)
        {
            SpriteBatch spriteBatch = context.SpriteBatch;
            Texture2D texture = context.texture.Value;
            
            // Calculate arcane blue-violet hue based on time and star position
            float timeOffset = Main.GameUpdateCount * 0.025f;
            float positionOffset = context.resourceNumber * 0.12f;
            
            // Blue to violet range (0.55 to 0.8 in hue)
            float hue = 0.55f + (float)Math.Sin(timeOffset + positionOffset) * 0.12f + 0.12f;
            
            // Create shimmering arcane color
            Color arcaneColor = Main.hslToRgb(hue, 0.8f, 0.7f);
            
            // Add subtle pulsing
            float pulse = 1f + (float)Math.Sin(Main.GameUpdateCount * 0.08f + context.resourceNumber * 0.4f) * 0.1f;
            
            // Draw additive glow layer behind
            Color glowColor = arcaneColor * 0.35f;
            glowColor.A = 0; // Additive blending
            
            Vector2 glowOffset = new Vector2(-1.5f, -1.5f);
            spriteBatch.Draw(
                texture,
                context.position + glowOffset,
                context.source,
                glowColor,
                context.rotation,
                context.origin,
                context.scale * pulse * 1.12f,
                context.effects,
                0f
            );
            
            // Draw arcane-tinted overlay with additive blend
            Color overlayColor = arcaneColor * 0.55f;
            overlayColor.A = 0; // For additive blending effect
            
            spriteBatch.Draw(
                texture,
                context.position,
                context.source,
                overlayColor,
                context.rotation,
                context.origin,
                context.scale * pulse,
                context.effects,
                0f
            );
            
            // Occasional arcane sparkle effect
            if (Main.rand.NextBool(100))
            {
                Vector2 sparklePos = context.position + new Vector2(
                    Main.rand.NextFloat(-5, 15),
                    Main.rand.NextFloat(-5, 15)
                );
                
                // Spawn arcane dust sparkle
                Dust dust = Dust.NewDustDirect(
                    sparklePos, 
                    2, 2, 
                    Terraria.ID.DustID.MagicMirror, 
                    0f, -0.3f, 
                    100, 
                    arcaneColor, 
                    0.5f
                );
                dust.noGravity = true;
                dust.velocity *= 0.4f;
            }
        }
    }
}
