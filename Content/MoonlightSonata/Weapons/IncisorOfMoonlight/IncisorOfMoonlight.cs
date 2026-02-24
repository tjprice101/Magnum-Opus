using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent;
using MagnumOpus.Common;
using MagnumOpus.Common.BaseClasses;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Content.MoonlightSonata.ResonanceEnergies;
using MagnumOpus.Content.MoonlightSonata.CraftingStations;

namespace MagnumOpus.Content.MoonlightSonata.ResonantWeapons
{
    /// <summary>
    /// Incisor of Moonlight — Moonlight Sonata endgame melee weapon using held-projectile swing system.
    /// A crescent blade forged from crystallized moonlight — 4-phase lunar combo with escalating wave projectiles.
    /// Crafted at Moonlight Anvil.
    /// </summary>
    public class IncisorOfMoonlight : MeleeSwingItemBase
    {
        #region Theme Colors

        private static readonly Color DarkPurple = new Color(75, 0, 130);
        private static readonly Color MediumPurple = new Color(138, 43, 226);
        private static readonly Color LightBlue = new Color(135, 206, 250);
        private static readonly Color Silver = new Color(220, 220, 235);

        #endregion

        #region Abstract Overrides (MeleeSwingItemBase)

        protected override int SwingProjectileType => ModContent.ProjectileType<IncisorOfMoonlightSwing>();
        protected override int ComboStepCount => 4;

        #endregion

        #region Virtual Overrides

        protected override Color GetLoreColor() => new Color(140, 100, 200);

        protected override void SetWeaponDefaults()
        {
            Item.width = 60;
            Item.height = 60;
            Item.damage = 280;
            Item.DamageType = DamageClass.MeleeNoSpeed;
            Item.useTime = 12;
            Item.useAnimation = 12;
            Item.knockBack = 6.5f;
            Item.value = Item.sellPrice(gold: 25);
            Item.rare = ModContent.RarityType<EroicaRainbowRarity>();
            Item.UseSound = SoundID.Item1;
        }

        protected override void AddWeaponTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "LunarCombo",
                "4-phase lunar combo fires escalating crescent wave projectiles")
            { OverrideColor = LightBlue });
            tooltips.Add(new TooltipLine(Mod, "SeekingCrystals",
                "Hits unleash seeking moonlight crystals on critical strikes")
            { OverrideColor = Silver });
            tooltips.Add(new TooltipLine(Mod, "MoonlightAura",
                "Ethereal moonlight aura while held")
            { OverrideColor = MediumPurple });
            tooltips.Add(new TooltipLine(Mod, "Lore",
                "'A blade forged from crystallized moonlight'")
            { OverrideColor = GetLoreColor() });
        }

        #endregion

        #region Recipe

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<MoonlightsResonantEnergy>(), 20)
                .AddIngredient(ModContent.ItemType<ResonantCoreOfMoonlightSonata>(), 10)
                .AddIngredient(ModContent.ItemType<Enemies.ShardsOfMoonlitTempo>(), 25)
                .AddTile(ModContent.TileType<MoonlightAnvilTile>())
                .Register();
        }

        #endregion

        #region Visual Effects

        public override void HoldItem(Player player)
        {
            base.HoldItem(player);

            // === UnifiedVFX MOONLIGHT SONATA AURA ===
            UnifiedVFX.MoonlightSonata.Aura(player.Center, 32f, 0.28f);

            // Subtle ambient sparkle (reduced frequency for cleaner look)
            if (Main.rand.NextBool(15))
            {
                Vector2 offset = Main.rand.NextVector2Circular(25f, 25f);
                CustomParticles.PrismaticSparkle(player.Center + offset, UnifiedVFX.MoonlightSonata.Silver * 0.6f, 0.2f);
            }

            // Soft gradient lighting with pulse
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.05f) * 0.08f + 0.92f;
            Vector3 lightColor = Color.Lerp(UnifiedVFX.MoonlightSonata.DarkPurple, UnifiedVFX.MoonlightSonata.LightBlue, 0.4f).ToVector3();
            Lighting.AddLight(player.Center, lightColor * pulse * 0.5f);
        }

        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            // Draw glowing backlight effect when dropped in world
            Texture2D texture = TextureAssets.Item[Item.type].Value;
            Vector2 position = Item.Center - Main.screenPosition;
            Vector2 origin = texture.Size() / 2f;

            // Calculate pulse - slow and ethereal for moonlight theme
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.04f) * 0.12f + 1f;

            // Begin additive blending for glow
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            // Outer deep purple aura
            spriteBatch.Draw(texture, position, null, new Color(75, 0, 130) * 0.45f, rotation, origin, scale * pulse * 1.35f, SpriteEffects.None, 0f);

            // Middle blue-purple glow
            spriteBatch.Draw(texture, position, null, new Color(138, 43, 226) * 0.35f, rotation, origin, scale * pulse * 1.2f, SpriteEffects.None, 0f);

            // Inner silver/lavender glow
            spriteBatch.Draw(texture, position, null, new Color(200, 180, 255) * 0.25f, rotation, origin, scale * pulse * 1.08f, SpriteEffects.None, 0f);

            // Return to normal blending
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            // Add lighting
            Lighting.AddLight(Item.Center, 0.4f, 0.3f, 0.7f);

            return true; // Draw the normal sprite too
        }

        public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            // === MOONLIGHT INVENTORY GLOW ===
            Texture2D texture = TextureAssets.Item[Item.type].Value;

            float time = Main.GameUpdateCount * 0.04f;
            float pulse = 1f + (float)Math.Sin(time * 1.5f) * 0.08f;

            // Additive glow layer
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.UIScaleMatrix);

            // Ethereal purple-blue glow
            Color glowColor = Color.Lerp(UnifiedVFX.MoonlightSonata.DarkPurple, UnifiedVFX.MoonlightSonata.LightBlue,
                (float)Math.Sin(time * 0.7f) * 0.5f + 0.5f) * 0.25f;
            spriteBatch.Draw(texture, position, frame, glowColor, 0f, origin, scale * pulse * 1.12f, SpriteEffects.None, 0f);

            // Return to normal blending
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.UIScaleMatrix);

            // Draw the actual item
            spriteBatch.Draw(texture, position, frame, drawColor, 0f, origin, scale, SpriteEffects.None, 0f);

            return false;
        }

        #endregion
    }
}
