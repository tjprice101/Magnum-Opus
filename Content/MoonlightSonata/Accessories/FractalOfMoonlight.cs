using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;
using MagnumOpus.Content.MoonlightSonata.ResonanceEnergies;
using MagnumOpus.Content.MoonlightSonata.Enemies;
using MagnumOpus.Content.MoonlightSonata.CraftingStations;
using MagnumOpus.Common.Systems;

namespace MagnumOpus.Content.MoonlightSonata.Accessories
{
    /// <summary>
    /// Fractal of Moonlight - Summoner accessory.
    /// General buffs: +2 minion slots, +15% minion damage.
    /// Moonlight-specific: +50% damage, +25% attack speed for Moonlight minions.
    /// </summary>
    public class FractalOfMoonlight : ModItem
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
            modPlayer.hasFractalOfMoonlight = true;
            
            // GENERAL SUMMONER BUFFS (applies to ALL minions)
            player.maxMinions += 2; // +2 minion slots
            player.GetDamage(DamageClass.Summon) += 0.15f; // +15% minion damage
            
            // Note: Additional Moonlight-specific minion buffs (+50% damage, +25% attack speed)
            // are handled in the Goliath and Razer minion projectiles themselves
            
            // Enhanced ambient particles using ThemedParticles
            if (!hideVisual)
            {
                // Moonlight aura around player
                ThemedParticles.MoonlightAura(player.Center, 30f);
                
                // Occasional sparkles
                if (Main.rand.NextBool(8))
                {
                    ThemedParticles.MoonlightSparkles(player.Center, 4, 25f);
                }
            }
        }
        
        public override void ModifyTooltips(System.Collections.Generic.List<TooltipLine> tooltips)
        {
            // General summoner buffs
            tooltips.Add(new TooltipLine(Mod, "GeneralHeader", "All Summons:")
            {
                OverrideColor = new Color(100, 200, 100)
            });
            
            tooltips.Add(new TooltipLine(Mod, "MinionSlots", "+2 minion slots")
            {
                OverrideColor = new Color(255, 200, 100)
            });
            
            tooltips.Add(new TooltipLine(Mod, "MinionDamage", "+15% summon damage")
            {
                OverrideColor = new Color(255, 200, 100)
            });
            
            // Moonlight-specific buffs
            tooltips.Add(new TooltipLine(Mod, "MoonlightHeader", "Moonlight Minions:")
            {
                OverrideColor = new Color(180, 120, 255)
            });
            
            tooltips.Add(new TooltipLine(Mod, "MoonlightDamage", "+50% additional damage for Moonlight minions")
            {
                OverrideColor = new Color(200, 150, 255)
            });
            
            tooltips.Add(new TooltipLine(Mod, "MoonlightSpeed", "+25% attack speed for Moonlight minions")
            {
                OverrideColor = new Color(200, 150, 255)
            });
            
            tooltips.Add(new TooltipLine(Mod, "Flavor", "'Infinite reflections of lunar power'")
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
                .AddIngredient(ItemID.SoulofSight, 5)
                .AddTile(ModContent.TileType<MoonlightAnvilTile>())
                .Register();
        }
    }
}
