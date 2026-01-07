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
    /// Moonlit Gyre - Ranger accessory.
    /// Bullets/arrows that miss ricochet up to 3 times.
    /// +25% ranged crit chance.
    /// Crits create sonic booms that pierce 5 enemies.
    /// 40% chance not to consume ammo.
    /// </summary>
    public class MoonlitGyre : ModItem
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
            modPlayer.hasMoonlitGyre = true;
            
            // +25% ranged crit chance
            player.GetCritChance(DamageClass.Ranged) += 25f;
            
            // Ambient particles when equipped
            if (!hideVisual && Main.rand.NextBool(8))
            {
                int dustType = Main.rand.NextBool() ? DustID.Shadowflame : DustID.IceTorch;
                Vector2 offset = Main.rand.NextVector2Circular(25f, 25f);
                Dust dust = Dust.NewDustPerfect(player.Center + offset + new Vector2(0, -35f), dustType, 
                    new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), -1.5f), 100, default, 1f);
                dust.noGravity = true;
            }
        }

        public override void ModifyTooltips(System.Collections.Generic.List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "CritBoost", "+25% ranged critical strike chance")
            {
                OverrideColor = new Color(255, 200, 100)
            });
            
            tooltips.Add(new TooltipLine(Mod, "RicochetEffect", "Missed projectiles ricochet up to 3 times toward enemies")
            {
                OverrideColor = new Color(150, 180, 220)
            });
            
            tooltips.Add(new TooltipLine(Mod, "SonicBoom", "Critical hits create sonic booms that pierce through 5 enemies")
            {
                OverrideColor = new Color(120, 80, 180)
            });
            
            tooltips.Add(new TooltipLine(Mod, "AmmoSave", "40% chance not to consume ammo")
            {
                OverrideColor = new Color(100, 200, 100)
            });
            
            tooltips.Add(new TooltipLine(Mod, "Flavor", "'Spinning through the lunar vortex'")
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
                .AddIngredient(ItemID.SoulofNight, 5)
                .AddTile(ModContent.TileType<MoonlightAnvilTile>())
                .Register();
        }
    }
}
