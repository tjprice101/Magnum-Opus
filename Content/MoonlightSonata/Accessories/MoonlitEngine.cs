using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;
using MagnumOpus.Content.MoonlightSonata.ResonanceEnergies;
using MagnumOpus.Content.MoonlightSonata.Enemies;
using MagnumOpus.Content.MoonlightSonata.CraftingStations;

namespace MagnumOpus.Content.MoonlightSonata.Accessories
{
    /// <summary>
    /// Moonlit Engine - Melee accessory.
    /// Every 5th melee strike creates a devastating shockwave (200% damage).
    /// +18% melee speed.
    /// </summary>
    public class MoonlitEngine : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.accessory = true;
            Item.value = Item.buyPrice(platinum: 2);
            Item.rare = ModContent.RarityType<MoonlightSonataRarity>();
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var modPlayer = player.GetModPlayer<MoonlightAccessoryPlayer>();
            modPlayer.hasMoonlitEngine = true;
            
            // +18% melee speed
            player.GetAttackSpeed(DamageClass.Melee) += 0.18f;
            
            // Ambient particles when equipped
            if (!hideVisual && Main.rand.NextBool(8))
            {
                int dustType = Main.rand.NextBool() ? DustID.PurpleTorch : DustID.IceTorch;
                Vector2 offset = Main.rand.NextVector2Circular(20f, 20f);
                Dust dust = Dust.NewDustPerfect(player.Center + offset + new Vector2(0, -40f), dustType, 
                    new Vector2(0, -1f), 100, default, 1f);
                dust.noGravity = true;
            }
        }

        public override void ModifyTooltips(System.Collections.Generic.List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "MeleeBoost", "+18% melee speed")
            {
                OverrideColor = new Color(255, 200, 100)
            });
            
            tooltips.Add(new TooltipLine(Mod, "ShockwaveEffect", "Every 5th melee strike creates a devastating shockwave")
            {
                OverrideColor = new Color(180, 120, 255)
            });
            
            tooltips.Add(new TooltipLine(Mod, "ShockwaveDamage", "Shockwave deals 200% weapon damage in a large AoE")
            {
                OverrideColor = new Color(150, 100, 200)
            });
            
            tooltips.Add(new TooltipLine(Mod, "Flavor", "'The engine of moonlight drives destruction'")
            {
                OverrideColor = new Color(120, 120, 180)
            });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<MoonlightsResonantEnergy>(), 5)
                .AddIngredient(ModContent.ItemType<ResonantCoreOfMoonlightSonata>(), 5)
                .AddIngredient(ModContent.ItemType<ShardsOfMoonlitTempo>(), 5)
                .AddIngredient(ItemID.SoulofMight, 5)
                .AddTile(ModContent.TileType<MoonlightAnvilTile>())
                .Register();
        }
    }
}
