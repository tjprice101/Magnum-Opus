using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;
using MagnumOpus.Content.Eroica.ResonanceEnergies;
using MagnumOpus.Content.Eroica.Enemies;
using MagnumOpus.Content.MoonlightSonata.CraftingStations;

namespace MagnumOpus.Content.Eroica.Accessories
{
    /// <summary>
    /// Pyre of the Fallen Hero - Melee accessory.
    /// Triumphant Berserker: Melee hits build "Fury" stacks (max 12). 
    /// At max, release a devastating 360° sakura slash wave dealing 400% damage.
    /// Taking damage grants +25% attack speed for 2 seconds.
    /// </summary>
    public class PyreOfTheFallenHero : ModItem
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
            modPlayer.hasPyreOfTheFallenHero = true;
            
            // +25% attack speed when recently damaged
            if (modPlayer.damageBoostTimer > 0)
            {
                player.GetAttackSpeed(DamageClass.Melee) += 0.25f;
            }
            
            // Ambient particles - deep scarlet and black flames
            if (!hideVisual && Main.rand.NextBool(6))
            {
                // Scarlet flame
                Vector2 offset = Main.rand.NextVector2Circular(25f, 25f);
                Dust flame = Dust.NewDustPerfect(player.Center + offset, DustID.CrimsonTorch, 
                    new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), -2f), 100, default, 1.3f);
                flame.noGravity = true;
            }
            
            if (!hideVisual && Main.rand.NextBool(8))
            {
                // Black smoke
                Vector2 offset = Main.rand.NextVector2Circular(20f, 20f);
                Dust smoke = Dust.NewDustPerfect(player.Center + offset, DustID.Smoke, 
                    new Vector2(0, -1.5f), 180, Color.Black, 1.2f);
                smoke.noGravity = true;
            }
            
            // Fury stack visual indicator
            if (!hideVisual && modPlayer.furyStacks > 0)
            {
                // More intense particles as stacks build
                float intensity = (float)modPlayer.furyStacks / 12f;
                if (Main.rand.NextFloat() < intensity * 0.5f)
                {
                    Dust fury = Dust.NewDustPerfect(player.Center + Main.rand.NextVector2Circular(30f, 30f),
                        DustID.Torch, Main.rand.NextVector2Circular(2f, 2f), 50, new Color(255, 100 + (int)(155 * intensity), 50), 1f + intensity);
                    fury.noGravity = true;
                }
            }
        }

        public override void ModifyTooltips(System.Collections.Generic.List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "FuryHeader", "Triumphant Berserker:")
            {
                OverrideColor = new Color(255, 100, 100)
            });
            
            tooltips.Add(new TooltipLine(Mod, "FuryEffect", "Melee hits build Fury stacks (max 12)")
            {
                OverrideColor = new Color(255, 180, 100)
            });
            
            tooltips.Add(new TooltipLine(Mod, "FuryRelease", "At max stacks, release a 360° sakura slash wave (400% damage)")
            {
                OverrideColor = new Color(255, 150, 150)
            });
            
            tooltips.Add(new TooltipLine(Mod, "DamageBoost", "Taking damage grants +25% melee attack speed for 2 seconds")
            {
                OverrideColor = new Color(255, 200, 100)
            });
            
            tooltips.Add(new TooltipLine(Mod, "Flavor", "'Rise from the flames, stronger than before'")
            {
                OverrideColor = new Color(180, 80, 80)
            });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<EroicasResonantEnergy>(), 5)
                .AddIngredient(ModContent.ItemType<ResonantCoreOfEroica>(), 5)
                .AddIngredient(ModContent.ItemType<ShardOfTriumphsTempo>(), 5)
                .AddIngredient(ItemID.SoulofMight, 5)
                .AddTile(ModContent.TileType<MoonlightAnvilTile>())
                .Register();
        }
    }
}
