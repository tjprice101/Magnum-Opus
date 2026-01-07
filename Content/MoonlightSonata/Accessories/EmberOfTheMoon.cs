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
            
            // Ambient particles when equipped - ember-like effect
            if (!hideVisual && Main.rand.NextBool(5))
            {
                int dustType = Main.rand.NextBool(3) ? DustID.IceTorch : DustID.PurpleTorch;
                Vector2 offset = Main.rand.NextVector2Circular(15f, 15f);
                
                Dust dust = Dust.NewDustPerfect(player.Center + offset, dustType, 
                    new Vector2(Main.rand.NextFloat(-0.3f, 0.3f), Main.rand.NextFloat(-2f, -1f)), 0, default, 1.3f);
                dust.noGravity = true;
                dust.fadeIn = 1f;
            }
            
            // Occasional white sparkle
            if (!hideVisual && Main.rand.NextBool(15))
            {
                Vector2 offset = Main.rand.NextVector2Circular(20f, 20f);
                Dust sparkle = Dust.NewDustPerfect(player.Center + offset, DustID.SparksMech, 
                    new Vector2(0, -1.5f), 0, Color.White, 1f);
                sparkle.noGravity = true;
            }
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
                OverrideColor = new Color(120, 120, 180)
            });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<MoonlightsResonantEnergy>(), 5)
                .AddIngredient(ModContent.ItemType<ResonantCoreOfMoonlightSonata>(), 5)
                .AddIngredient(ModContent.ItemType<ShardsOfMoonlitTempo>(), 5)
                .AddIngredient(ItemID.SoulofLight, 5)
                .AddTile(ModContent.TileType<MoonlightAnvilTile>())
                .Register();
        }
    }
}
