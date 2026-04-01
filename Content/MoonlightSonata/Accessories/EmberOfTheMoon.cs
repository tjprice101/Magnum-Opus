using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;
using MagnumOpus.Content.MoonlightSonata.ResonanceEnergies;
using MagnumOpus.Content.MoonlightSonata.Enemies;
using MagnumOpus.Content.MoonlightSonata.CraftingStations;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Content.MoonlightSonata.VFX.Accessories;

namespace MagnumOpus.Content.MoonlightSonata.Accessories
{
    /// <summary>
    /// Ember of the Moon - Mage accessory.
    /// -30% mana cost.
    /// +25% magic damage.
    /// 15% chance to cast spells twice.
    /// When mana drops below 20%, automatically restores 100 mana (120 second cooldown).
    /// </summary>
    public class EmberOfTheMoon : ModItem
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
            modPlayer.hasEmberOfTheMoon = true;
            
            // Note: -30% mana cost, +25% magic damage, and double cast handled in MoonlightAccessoryPlayer
        }

        public override void ModifyTooltips(System.Collections.Generic.List<TooltipLine> tooltips)
        {
            // Get current cooldown status
            Player player = Main.LocalPlayer;
            var modPlayer = player.GetModPlayer<MoonlightAccessoryPlayer>();
            
            tooltips.Add(new TooltipLine(Mod, "ManaCost", "-30% mana cost")
            {
                OverrideColor = new Color(100, 200, 255)
            });
            
            tooltips.Add(new TooltipLine(Mod, "DamageBoost", "+25% magic damage")
            {
                OverrideColor = new Color(255, 200, 100)
            });
            
            tooltips.Add(new TooltipLine(Mod, "DoubleCast", "15% chance to cast spells twice")
            {
                OverrideColor = new Color(180, 120, 255)
            });
            
            tooltips.Add(new TooltipLine(Mod, "ManaRestore", "Automatically restores 100 mana when below 20%")
            {
                OverrideColor = new Color(100, 180, 255)
            });
            
            // Show cooldown status
            if (modPlayer.manaRestoreCooldown > 0)
            {
                int secondsLeft = modPlayer.manaRestoreCooldown / 60;
                tooltips.Add(new TooltipLine(Mod, "Cooldown", $"Mana restore on cooldown: {secondsLeft}s")
                {
                    OverrideColor = new Color(255, 100, 100)
                });
            }
            else
            {
                tooltips.Add(new TooltipLine(Mod, "CooldownReady", "Mana restore ready!")
                {
                    OverrideColor = new Color(100, 255, 100)
                });
            }
            
            tooltips.Add(new TooltipLine(Mod, "Flavor", "'A dying star's last breath of magic'")
            {
                OverrideColor = new Color(140, 100, 200)
            });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<MoonlightsResonantEnergy>(), 5)
                .AddIngredient(ModContent.ItemType<ResonantCoreOfMoonlightSonata>(), 5)
                .AddIngredient(ModContent.ItemType<ShardsOfMoonlitTempo>(), 5)
                .AddIngredient(ItemID.SoulofLight, 5)
                .AddIngredient(ItemID.SoulofNight, 8)
                .AddTile(ModContent.TileType<MoonlightAnvilTile>())
                .Register();
        }
    }
}
