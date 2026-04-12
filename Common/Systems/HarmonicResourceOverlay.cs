using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using MagnumOpus.Content.Common.Consumables;
using MagnumOpus.Content.Nachtmusik.Accessories;
using MagnumOpus.Content.DiesIrae.Accessories;
using MagnumOpus.Content.OdeToJoy.Accessories;
using MagnumOpus.Content.ClairDeLune.Accessories;

namespace MagnumOpus.Common.Systems
{
    /// <summary>
    /// Custom resource overlay that transforms hearts to light blue with purple glow shimmer
    /// and mana stars to pale light blue with deep purple overlay for players who have used
    /// CrystallizedHarmony and ArcaneHarmonicPrism consumables.
    /// </summary>
    public class HarmonicResourceOverlay : ModResourceOverlay
    {
        private Dictionary<string, Asset<Texture2D>> vanillaAssetCache = new();

        public override bool PreDrawResource(ResourceOverlayDrawContext context)
        {
            if (context.texture == null) return true;

            Player player = Main.LocalPlayer;
            Asset<Texture2D> asset = context.texture;
            string fancyFolder = "Images/UI/PlayerResourceSets/FancyClassic/";

            var harmonyPlayer = player.GetModPlayer<CrystallizedHarmonyPlayer>();
            int transformedHearts = harmonyPlayer.crystallizedHarmonyUses;

            var prismPlayer = player.GetModPlayer<ArcaneHarmonicPrismPlayer>();
            int transformedStars = prismPlayer.arcaneHarmonicPrismUses;

            // --- HEARTS: Tint base heart to light blue ---
            bool isHeart = asset == TextureAssets.Heart || asset == TextureAssets.Heart2
                || CompareAssets(asset, fancyFolder + "Heart_Fill")
                || CompareAssets(asset, fancyFolder + "Heart_Fill_B");

            if (isHeart && transformedHearts > 0 && context.resourceNumber < transformedHearts)
            {
                // Draw the base heart tinted light blue instead of red
                SpriteBatch sb = context.SpriteBatch;
                Texture2D tex = context.texture.Value;

                // Light blue tint for the heart base
                Color lightBlue = new Color(130, 200, 255);
                float pulse = 0.9f + 0.1f * MathF.Sin(Main.GlobalTimeWrappedHourly * 4f + context.resourceNumber * 0.7f);
                Color tintedColor = lightBlue * pulse;

                sb.Draw(tex, context.position, context.source, tintedColor, context.rotation,
                    context.origin, context.scale, context.effects, 0f);

                return false; // Skip vanilla draw — we drew it ourselves
            }

            // --- HEARTS: Wing Amplification tint ---
            var wingColors = GetWingAmplifyHeartColors(player);
            if (isHeart && wingColors.HasValue)
            {
                SpriteBatch sb = context.SpriteBatch;
                Texture2D tex = context.texture.Value;
                var (baseColor, shimmerColor) = wingColors.Value;

                float pulse = 0.9f + 0.1f * MathF.Sin(Main.GlobalTimeWrappedHourly * 4f + context.resourceNumber * 0.7f);
                Color tintedColor = baseColor * pulse;

                sb.Draw(tex, context.position, context.source, tintedColor, context.rotation,
                    context.origin, context.scale, context.effects, 0f);

                return false;
            }

            // --- MANA STARS: Tint base star to pale light blue ---
            bool isManaStar = asset == TextureAssets.Mana
                || CompareAssets(asset, fancyFolder + "Star_Fill");

            if (isManaStar && transformedStars > 0 && context.resourceNumber < transformedStars)
            {
                SpriteBatch sb = context.SpriteBatch;
                Texture2D tex = context.texture.Value;

                // Pale light blue for mana star base
                Color paleBlue = new Color(160, 210, 245);
                float pulse = 0.9f + 0.1f * MathF.Sin(Main.GlobalTimeWrappedHourly * 3.5f + context.resourceNumber * 0.6f);
                Color tintedColor = paleBlue * pulse;

                sb.Draw(tex, context.position, context.source, tintedColor, context.rotation,
                    context.origin, context.scale, context.effects, 0f);

                return false;
            }

            return true;
        }

        public override void PostDrawResource(ResourceOverlayDrawContext context)
        {
            if (context.texture == null) return;

            Player player = Main.LocalPlayer;
            Asset<Texture2D> asset = context.texture;
            string fancyFolder = "Images/UI/PlayerResourceSets/FancyClassic/";

            var harmonyPlayer = player.GetModPlayer<CrystallizedHarmonyPlayer>();
            int transformedHearts = harmonyPlayer.crystallizedHarmonyUses;

            var prismPlayer = player.GetModPlayer<ArcaneHarmonicPrismPlayer>();
            int transformedStars = prismPlayer.arcaneHarmonicPrismUses;

            // --- HEARTS: Purple glow shimmer overlay ---
            bool isHeart = asset == TextureAssets.Heart || asset == TextureAssets.Heart2
                || CompareAssets(asset, fancyFolder + "Heart_Fill")
                || CompareAssets(asset, fancyFolder + "Heart_Fill_B");

            if (isHeart && transformedHearts > 0 && context.resourceNumber < transformedHearts)
            {
                DrawPurpleGlowHeartOverlay(context);
            }

            // --- HEARTS: Wing Amplification shimmer overlay ---
            var wingColors = GetWingAmplifyHeartColors(player);
            if (isHeart && wingColors.HasValue)
            {
                DrawWingAmplifyHeartOverlay(context, wingColors.Value.shimmer);
            }

            // --- MANA STARS: Deep purple overlay ---
            bool isManaStar = asset == TextureAssets.Mana
                || CompareAssets(asset, fancyFolder + "Star_Fill");

            if (isManaStar && transformedStars > 0 && context.resourceNumber < transformedStars)
            {
                DrawDeepPurpleManaOverlay(context);
            }
        }

        private bool CompareAssets(Asset<Texture2D> existingAsset, string compareAssetPath)
        {
            if (!vanillaAssetCache.TryGetValue(compareAssetPath, out var asset))
                asset = vanillaAssetCache[compareAssetPath] = Main.Assets.Request<Texture2D>(compareAssetPath);
            return existingAsset == asset;
        }

        /// <summary>
        /// Draws a purple glowing hue shimmer overlay on light-blue tinted hearts.
        /// Three additive glow layers that pulse and shift between violet and purple.
        /// </summary>
        private void DrawPurpleGlowHeartOverlay(ResourceOverlayDrawContext context)
        {
            SpriteBatch sb = context.SpriteBatch;
            Texture2D tex = context.texture.Value;
            float t = Main.GlobalTimeWrappedHourly;
            int idx = context.resourceNumber;

            // Shimmer cycle: oscillates between blue-purple and magenta-purple
            float shimmer = 0.5f + 0.5f * MathF.Sin(t * 3f + idx * 0.8f);
            Color purple1 = new Color(120, 60, 200);  // Blue-violet
            Color purple2 = new Color(180, 80, 220);  // Magenta-purple
            Color glowColor = Color.Lerp(purple1, purple2, shimmer);

            // Pulse intensity
            float pulseA = 0.85f + 0.15f * MathF.Sin(t * 5f + idx * 1.1f);
            float pulseB = 0.9f + 0.1f * MathF.Sin(t * 7f + idx * 0.6f + 1.5f);

            // Outer diffuse glow (large, soft)
            Color outerGlow = glowColor * (0.30f * pulseA);
            outerGlow.A = 0;
            sb.Draw(tex, context.position + new Vector2(-3f, -3f), context.source, outerGlow,
                context.rotation, context.origin, context.scale * 1.3f * pulseA, context.effects, 0f);

            // Mid glow
            Color midGlow = glowColor * (0.45f * pulseB);
            midGlow.A = 0;
            sb.Draw(tex, context.position + new Vector2(-1.5f, -1.5f), context.source, midGlow,
                context.rotation, context.origin, context.scale * 1.15f * pulseB, context.effects, 0f);

            // Inner bright shimmer
            Color innerGlow = glowColor * (0.55f * pulseA * pulseB);
            innerGlow.A = 0;
            sb.Draw(tex, context.position, context.source, innerGlow,
                context.rotation, context.origin, context.scale, context.effects, 0f);

            // Sparkle dust
            if (Main.rand.NextBool(90))
            {
                Vector2 sparklePos = context.position + new Vector2(Main.rand.NextFloat(-4, 14), Main.rand.NextFloat(-4, 14));
                Dust d = Dust.NewDustDirect(sparklePos, 2, 2, Terraria.ID.DustID.PurpleTorch,
                    0f, -0.4f, 120, Color.White, 0.45f);
                d.noGravity = true;
                d.velocity *= 0.2f;
            }
        }

        /// <summary>
        /// Draws a deep purple overlay on pale light-blue tinted mana stars.
        /// Three additive glow layers with a slower, more mysterious pulse.
        /// </summary>
        private void DrawDeepPurpleManaOverlay(ResourceOverlayDrawContext context)
        {
            SpriteBatch sb = context.SpriteBatch;
            Texture2D tex = context.texture.Value;
            float t = Main.GlobalTimeWrappedHourly;
            int idx = context.resourceNumber;

            // Deep purple shimmer — darker and more saturated than hearts
            float shimmer = 0.5f + 0.5f * MathF.Sin(t * 2.5f + idx * 0.7f + 2f);
            Color deepPurple1 = new Color(80, 30, 160);   // Deep violet
            Color deepPurple2 = new Color(130, 50, 180);   // Rich purple
            Color glowColor = Color.Lerp(deepPurple1, deepPurple2, shimmer);

            float pulseA = 0.88f + 0.12f * MathF.Sin(t * 4f + idx * 0.9f);
            float pulseB = 0.92f + 0.08f * MathF.Sin(t * 6f + idx * 0.5f + 1f);

            // Outer deep glow
            Color outerGlow = glowColor * (0.28f * pulseA);
            outerGlow.A = 0;
            sb.Draw(tex, context.position + new Vector2(-2.5f, -2.5f), context.source, outerGlow,
                context.rotation, context.origin, context.scale * 1.25f * pulseA, context.effects, 0f);

            // Mid glow
            Color midGlow = glowColor * (0.42f * pulseB);
            midGlow.A = 0;
            sb.Draw(tex, context.position + new Vector2(-1f, -1f), context.source, midGlow,
                context.rotation, context.origin, context.scale * 1.12f * pulseB, context.effects, 0f);

            // Inner vivid glow
            Color innerGlow = glowColor * (0.50f * pulseA * pulseB);
            innerGlow.A = 0;
            sb.Draw(tex, context.position, context.source, innerGlow,
                context.rotation, context.origin, context.scale, context.effects, 0f);

            // Sparkle dust
            if (Main.rand.NextBool(80))
            {
                Vector2 sparklePos = context.position + new Vector2(Main.rand.NextFloat(-4, 14), Main.rand.NextFloat(-4, 14));
                Dust d = Dust.NewDustDirect(sparklePos, 2, 2, Terraria.ID.DustID.PurpleTorch,
                    0f, -0.3f, 100, Color.White, 0.4f);
                d.noGravity = true;
                d.velocity *= 0.25f;
            }
        }

        /// <summary>
        /// Returns (baseColor, shimmerColor) for the active wing amplification buff, or null if none active.
        /// </summary>
        private (Color baseColor, Color shimmer)? GetWingAmplifyHeartColors(Player player)
        {
            if (player.HasBuff(ModContent.BuffType<MoonlightWingAmplifyBuff>()))
                return (new Color(60, 40, 100), new Color(100, 70, 180));
            if (player.HasBuff(ModContent.BuffType<EroicaWingAmplifyBuff>()))
                return (new Color(120, 90, 30), new Color(255, 180, 200));
            if (player.HasBuff(ModContent.BuffType<LaCampanellaWingAmplifyBuff>()))
                return (new Color(80, 80, 80), new Color(255, 160, 60));
            if (player.HasBuff(ModContent.BuffType<EnigmaWingAmplifyBuff>()))
                return (new Color(70, 40, 100), new Color(50, 180, 80));
            if (player.HasBuff(ModContent.BuffType<SwanLakeWingAmplifyBuff>()))
                return (new Color(40, 40, 40), new Color(255, 255, 255));
            if (player.HasBuff(ModContent.BuffType<FateWingAmplifyBuff>()))
                return (new Color(120, 30, 60), new Color(180, 100, 200));
            if (player.HasBuff(ModContent.BuffType<NachtmusikWingAmplifyBuff>()))
                return (new Color(30, 35, 80), new Color(200, 210, 230));
            if (player.HasBuff(ModContent.BuffType<DiesIraeWingAmplifyBuff>()))
                return (new Color(100, 20, 20), new Color(255, 140, 50));
            if (player.HasBuff(ModContent.BuffType<OdeToJoyWingAmplifyBuff>()))
                return (new Color(130, 110, 40), new Color(255, 200, 80));
            if (player.HasBuff(ModContent.BuffType<ClairDeLuneWingAmplifyBuff>()))
                return (new Color(70, 70, 70), new Color(200, 180, 220));
            return null;
        }

        /// <summary>
        /// Draws a theme-colored shimmer overlay on hearts during wing amplification.
        /// </summary>
        private void DrawWingAmplifyHeartOverlay(ResourceOverlayDrawContext context, Color shimmerColor)
        {
            SpriteBatch sb = context.SpriteBatch;
            Texture2D tex = context.texture.Value;
            float t = Main.GlobalTimeWrappedHourly;
            int idx = context.resourceNumber;

            float shimmer = 0.5f + 0.5f * MathF.Sin(t * 4f + idx * 0.9f);
            Color glowColor = Color.Lerp(shimmerColor * 0.6f, shimmerColor, shimmer);

            float pulseA = 0.85f + 0.15f * MathF.Sin(t * 5f + idx * 1.1f);
            float pulseB = 0.9f + 0.1f * MathF.Sin(t * 7f + idx * 0.6f + 1.5f);

            // Outer diffuse glow
            Color outerGlow = glowColor * (0.30f * pulseA);
            outerGlow.A = 0;
            sb.Draw(tex, context.position + new Vector2(-3f, -3f), context.source, outerGlow,
                context.rotation, context.origin, context.scale * 1.3f * pulseA, context.effects, 0f);

            // Mid glow
            Color midGlow = glowColor * (0.45f * pulseB);
            midGlow.A = 0;
            sb.Draw(tex, context.position + new Vector2(-1.5f, -1.5f), context.source, midGlow,
                context.rotation, context.origin, context.scale * 1.15f * pulseB, context.effects, 0f);

            // Inner shimmer
            Color innerGlow = glowColor * (0.55f * pulseA * pulseB);
            innerGlow.A = 0;
            sb.Draw(tex, context.position, context.source, innerGlow,
                context.rotation, context.origin, context.scale, context.effects, 0f);
        }
    }
}
