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
    /// Moonlit Gyre - Ranger accessory for Moonlight weapons.
    /// Specifically buffs Moonlight rifle weapons (Resurrection of the Moon).
    /// +25% fire rate for Moonlight rifles.
    /// +25% bullet damage for Moonlight rifles.
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
            
            // Note: Moonlight rifle buffs (fire rate, bullet damage) are handled in the weapons themselves
            // +25% fire rate and +25% bullet damage for Moonlight rifle weapons specifically
            
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
            tooltips.Add(new TooltipLine(Mod, "MoonlightRifleBoost", "Moonlight Rifle Weapons:")
            {
                OverrideColor = new Color(180, 120, 255)
            });
            
            tooltips.Add(new TooltipLine(Mod, "FireRateBoost", "+25% fire rate for Moonlight rifles")
            {
                OverrideColor = new Color(255, 200, 100)
            });
            
            tooltips.Add(new TooltipLine(Mod, "BulletDamageBoost", "+25% bullet damage for Moonlight rifles")
            {
                OverrideColor = new Color(150, 180, 220)
            });
            
            tooltips.Add(new TooltipLine(Mod, "Flavor", "'The gyre empowers those who embrace the moon'")
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
