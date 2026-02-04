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
                // Fancy hearts - resourceNumber is still the heart index (0-based), same as classic
                if (transformedHearts > 0 && context.resourceNumber < transformedHearts)
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
        /// Draws a cosmic dark purple/pink shimmering overlay on transformed hearts.
        /// Creates vibrant wavy pulsing cosmic effect.
        /// </summary>
        private void DrawRainbowHeartOverlay(ResourceOverlayDrawContext context)
        {
            SpriteBatch spriteBatch = context.SpriteBatch;
            Texture2D texture = context.texture.Value;
            
            // Cosmic dark purple to dark pink color range (0.75 to 0.92 hue range)
            float timeOffset = Main.GameUpdateCount * 0.04f;
            float positionOffset = context.resourceNumber * 0.2f;
            
            // Create wavy motion - multiple sine waves for organic feel
            float wave1 = (float)Math.Sin(Main.GameUpdateCount * 0.08f + context.resourceNumber * 0.6f) * 0.5f;
            float wave2 = (float)Math.Sin(Main.GameUpdateCount * 0.12f + context.resourceNumber * 0.4f + 1.5f) * 0.3f;
            float waveOffset = wave1 + wave2;
            
            // Hue oscillates between dark purple (0.75) and dark pink/magenta (0.92)
            float hueBase = 0.75f + (float)Math.Sin(timeOffset + positionOffset + waveOffset) * 0.085f + 0.085f;
            
            // Create vibrant cosmic color with high saturation
            Color cosmicColor = Main.hslToRgb(hueBase, 0.95f, 0.55f);
            
            // Enhanced pulsing - multiple pulse frequencies for dynamic effect
            float pulse1 = (float)Math.Sin(Main.GameUpdateCount * 0.1f + context.resourceNumber * 0.5f) * 0.12f;
            float pulse2 = (float)Math.Sin(Main.GameUpdateCount * 0.16f + context.resourceNumber * 0.3f + 2f) * 0.08f;
            float pulse = 1f + pulse1 + pulse2;
            
            // Outer cosmic glow layer (large, diffuse)
            Color outerGlow = cosmicColor * 0.35f;
            outerGlow.A = 0; // Additive blending
            
            Vector2 outerOffset = new Vector2(-3, -3);
            spriteBatch.Draw(
                texture,
                context.position + outerOffset,
                context.source,
                outerGlow,
                context.rotation,
                context.origin,
                context.scale * pulse * 1.25f,
                context.effects,
                0f
            );
            
            // Middle intensity glow layer
            Color middleGlow = cosmicColor * 0.5f;
            middleGlow.A = 0;
            
            Vector2 middleOffset = new Vector2(-1.5f, -1.5f);
            spriteBatch.Draw(
                texture,
                context.position + middleOffset,
                context.source,
                middleGlow,
                context.rotation,
                context.origin,
                context.scale * pulse * 1.12f,
                context.effects,
                0f
            );
            
            // Inner vibrant overlay layer
            Color innerOverlay = cosmicColor * 0.7f;
            innerOverlay.A = 0;
            
            spriteBatch.Draw(
                texture,
                context.position,
                context.source,
                innerOverlay,
                context.rotation,
                context.origin,
                context.scale * pulse,
                context.effects,
                0f
            );
            
            // Occasional cosmic sparkle effect
            if (Main.rand.NextBool(80))
            {
                Vector2 sparklePos = context.position + new Vector2(
                    Main.rand.NextFloat(-5, 15),
                    Main.rand.NextFloat(-5, 15)
                );
                
                // Cosmic dust in purple-pink range
                float sparkleHue = Main.rand.NextFloat(0.75f, 0.95f);
                Color sparkleColor = Main.hslToRgb(sparkleHue, 1f, 0.7f);
                
                Dust dust = Dust.NewDustDirect(
                    sparklePos, 
                    2, 2, 
                    Terraria.ID.DustID.PurpleTorch, 
                    0f, -0.5f, 
                    100, 
                    sparkleColor, 
                    0.5f
                );
                dust.noGravity = true;
                dust.velocity *= 0.3f;
            }
        }

        /// <summary>
        /// Draws a cosmic dark purple/pink shimmering overlay on transformed mana stars.
        /// Creates vibrant wavy pulsing cosmic effect matching the hearts.
        /// </summary>
        private void DrawArcaneManaOverlay(ResourceOverlayDrawContext context)
        {
            SpriteBatch spriteBatch = context.SpriteBatch;
            Texture2D texture = context.texture.Value;
            
            // Cosmic dark purple to dark pink color range - slightly different phase from hearts
            float timeOffset = Main.GameUpdateCount * 0.035f;
            float positionOffset = context.resourceNumber * 0.18f;
            
            // Create wavy motion - multiple sine waves for organic cosmic feel
            float wave1 = (float)Math.Sin(Main.GameUpdateCount * 0.07f + context.resourceNumber * 0.5f) * 0.5f;
            float wave2 = (float)Math.Sin(Main.GameUpdateCount * 0.11f + context.resourceNumber * 0.35f + 1.2f) * 0.3f;
            float waveOffset = wave1 + wave2;
            
            // Hue oscillates between dark purple (0.76) and dark pink/magenta (0.90) - slightly shifted from hearts
            float hueBase = 0.76f + (float)Math.Sin(timeOffset + positionOffset + waveOffset) * 0.07f + 0.07f;
            
            // Create vibrant cosmic color with high saturation
            Color cosmicColor = Main.hslToRgb(hueBase, 0.92f, 0.58f);
            
            // Enhanced pulsing - multiple pulse frequencies
            float pulse1 = (float)Math.Sin(Main.GameUpdateCount * 0.09f + context.resourceNumber * 0.45f) * 0.1f;
            float pulse2 = (float)Math.Sin(Main.GameUpdateCount * 0.14f + context.resourceNumber * 0.28f + 1.8f) * 0.06f;
            float pulse = 1f + pulse1 + pulse2;
            
            // Outer cosmic glow layer
            Color outerGlow = cosmicColor * 0.32f;
            outerGlow.A = 0;
            
            Vector2 outerOffset = new Vector2(-2.5f, -2.5f);
            spriteBatch.Draw(
                texture,
                context.position + outerOffset,
                context.source,
                outerGlow,
                context.rotation,
                context.origin,
                context.scale * pulse * 1.2f,
                context.effects,
                0f
            );
            
            // Middle intensity glow layer
            Color middleGlow = cosmicColor * 0.48f;
            middleGlow.A = 0;
            
            Vector2 middleOffset = new Vector2(-1.2f, -1.2f);
            spriteBatch.Draw(
                texture,
                context.position + middleOffset,
                context.source,
                middleGlow,
                context.rotation,
                context.origin,
                context.scale * pulse * 1.1f,
                context.effects,
                0f
            );
            
            // Inner vibrant overlay layer
            Color innerOverlay = cosmicColor * 0.65f;
            innerOverlay.A = 0;
            
            spriteBatch.Draw(
                texture,
                context.position,
                context.source,
                innerOverlay,
                context.rotation,
                context.origin,
                context.scale * pulse,
                context.effects,
                0f
            );
            
            // Occasional cosmic sparkle effect
            if (Main.rand.NextBool(70))
            {
                Vector2 sparklePos = context.position + new Vector2(
                    Main.rand.NextFloat(-5, 15),
                    Main.rand.NextFloat(-5, 15)
                );
                
                // Cosmic dust in purple-pink range
                float sparkleHue = Main.rand.NextFloat(0.76f, 0.93f);
                Color sparkleColor = Main.hslToRgb(sparkleHue, 1f, 0.65f);
                
                Dust dust = Dust.NewDustDirect(
                    sparklePos, 
                    2, 2, 
                    Terraria.ID.DustID.PinkTorch, 
                    0f, -0.4f, 
                    100, 
                    sparkleColor, 
                    0.45f
                );
                dust.noGravity = true;
                dust.velocity *= 0.35f;
            }
        }
    }
}
