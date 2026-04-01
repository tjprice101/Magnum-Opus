using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;
using MagnumOpus.Content.Eroica.ResonanceEnergies;
using MagnumOpus.Content.Eroica.Enemies;
using MagnumOpus.Content.MoonlightSonata.CraftingStations;
using MagnumOpus.Common.Systems;
using MagnumOpus.Content.Eroica.Accessories.Shared;

namespace MagnumOpus.Content.Eroica.Accessories.FuneralMarchInsignia
{
    /// <summary>
    /// Funeral March Insignia - Mage accessory.
    /// Heroic Encore: Taking fatal damage consumes ALL mana, grants 3s invulnerability + 2x magic damage (180s cooldown).
    /// Mana regeneration triples when below 20% mana.
    /// </summary>
    public class FuneralMarchInsignia : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.accessory = true;
            Item.value = Item.buyPrice(platinum: 3);
            Item.rare = ModContent.RarityType<EroicaRarity>();
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var modPlayer = player.GetModPlayer<EroicaAccessoryPlayer>();
            modPlayer.hasFuneralMarchInsignia = true;
            
            // Heroic Encore active visual - brilliant prismatic gem effect
            if (modPlayer.heroicEncoreActive)
            {
                // Intense golden/red aura
                Lighting.AddLight(player.Center, 1f, 0.6f, 0.2f);
                
                // Eroica themed impact burst for dramatic effect
                if (Main.rand.NextBool(3))
                {
                }
                
                if (Main.rand.NextBool(2))
                {
                    // Protective flame ring
                    float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                    Vector2 ringPos = player.Center + new Vector2((float)System.Math.Cos(angle), (float)System.Math.Sin(angle)) * 40f;
                    Dust ring = Dust.NewDustPerfect(ringPos, DustID.GoldCoin, Vector2.Zero, 0, default, 1.5f);
                    ring.noGravity = true;
                }
            }
        }

        public override void ModifyTooltips(System.Collections.Generic.List<TooltipLine> tooltips)
        {
            var player = Main.LocalPlayer;
            var modPlayer = player.GetModPlayer<EroicaAccessoryPlayer>();
            
            tooltips.Add(new TooltipLine(Mod, "EncoreHeader", "Heroic Encore:")
            {
                OverrideColor = new Color(200, 100, 150)
            });
            
            tooltips.Add(new TooltipLine(Mod, "DeathPrevention", "Fatal damage instead consumes ALL mana")
            {
                OverrideColor = new Color(255, 100, 100)
            });
            
            tooltips.Add(new TooltipLine(Mod, "Invulnerability", "Grants 3 seconds of invulnerability and doubled damage")
            {
                OverrideColor = new Color(255, 200, 100)
            });
            
            tooltips.Add(new TooltipLine(Mod, "Cooldown", "180 second cooldown")
            {
                OverrideColor = new Color(180, 180, 180)
            });
            
            tooltips.Add(new TooltipLine(Mod, "ManaRegen", "Mana regeneration triples when below 20% mana")
            {
                OverrideColor = new Color(150, 150, 255)
            });
            
            // Show current cooldown if active
            if (modPlayer.heroicEncoreCooldown > 0)
            {
                int secondsLeft = modPlayer.heroicEncoreCooldown / 60;
                tooltips.Add(new TooltipLine(Mod, "CooldownTimer", $"Cooldown: {secondsLeft}s remaining")
                {
                    OverrideColor = new Color(255, 100, 100)
                });
            }
            else
            {
                tooltips.Add(new TooltipLine(Mod, "Ready", "Ready!")
                {
                    OverrideColor = new Color(100, 255, 100)
                });
            }
            
            tooltips.Add(new TooltipLine(Mod, "Flavor", "'The march continues, even beyond death'")
            {
                OverrideColor = new Color(120, 80, 100)
            });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<EroicasResonantEnergy>(), 5)
                .AddIngredient(ModContent.ItemType<ResonantCoreOfEroica>(), 5)
                .AddIngredient(ModContent.ItemType<ShardOfTriumphsTempo>(), 5)
                .AddIngredient(ItemID.SoulofFright, 5)
                .AddIngredient(ItemID.SoulofMight, 15)
                .AddTile(ModContent.TileType<MoonlightAnvilTile>())
                .Register();
        }
    }
}
