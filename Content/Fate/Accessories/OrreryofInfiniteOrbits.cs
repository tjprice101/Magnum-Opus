using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;
using MagnumOpus.Content.Fate;
using MagnumOpus.Content.Fate.ResonanceEnergies;
using MagnumOpus.Content.Fate.HarmonicCores;
using MagnumOpus.Content.MoonlightSonata.CraftingStations;
using MagnumOpus.Content.Materials.EnemyDrops;

namespace MagnumOpus.Content.Fate.Accessories
{
    /// <summary>
    /// Orrery of Infinite Orbits - Summon accessory for Fate theme.
    /// A cosmic planetarium that enhances minion capabilities.
    /// Summons orbit around the player and gain cosmic empowerment.
    /// </summary>
    public class OrreryofInfiniteOrbits : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.accessory = true;
            Item.value = Item.buyPrice(platinum: 3);
            Item.rare = ModContent.RarityType<FateRarity>();
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var modPlayer = player.GetModPlayer<OrreryPlayer>();
            modPlayer.hasOrrery = true;
            
            // +22% summon damage
            player.GetDamage(DamageClass.Summon) += 0.22f;
            
            // +1 max minion
            player.maxMinions += 1;
            
            // +10% minion knockback
            player.GetKnockback(DamageClass.Summon) += 0.10f;
        }

        public override void ModifyTooltips(System.Collections.Generic.List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "SummonBoost", "+22% summon damage")
            {
                OverrideColor = new Color(100, 200, 255)
            });

            tooltips.Add(new TooltipLine(Mod, "MinionSlot", "+1 max minion")
            {
                OverrideColor = new Color(120, 220, 255)
            });

            tooltips.Add(new TooltipLine(Mod, "KnockbackBoost", "+10% minion knockback")
            {
                OverrideColor = new Color(140, 200, 255)
            });

            tooltips.Add(new TooltipLine(Mod, "CosmicEmpower", "Every 8s minions gain 'Cosmic Empowerment' for 4s (+25% summon damage)")
            {
                OverrideColor = FatePalette.DarkPink
            });

            tooltips.Add(new TooltipLine(Mod, "Flavor", "'The universe itself serves at your command'")
            {
                OverrideColor = new Color(255, 150, 180)
            });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<FateResonantEnergy>(), 15)
                .AddIngredient(ModContent.ItemType<ResonantCoreOfFate>(), 5)
                .AddIngredient(ModContent.ItemType<FateEssence>(), 5)
                .AddIngredient(ModContent.ItemType<ShardOfFatesTempo>(), 5)
                .AddIngredient(ItemID.SoulofSight, 10)
                .AddIngredient(ItemID.FragmentStardust, 8)
                .AddIngredient(ItemID.LunarBar, 10)
                .AddTile(ModContent.TileType<MoonlightAnvilTile>())
                .Register();
        }
    }

    public class OrreryPlayer : ModPlayer
    {
        public bool hasOrrery = false;
        public int empowermentCooldown = 0;
        public int empowermentTimer = 0;
        
        private const int EmpowermentInterval = 480; // 8 seconds
        private const int EmpowermentDuration = 240; // 4 seconds
        
        public override void ResetEffects()
        {
            hasOrrery = false;
        }

        public override void PostUpdateEquips()
        {
            if (!hasOrrery)
            {
                empowermentCooldown = 0;
                empowermentTimer = 0;
                return;
            }
            
            if (empowermentTimer > 0)
            {
                empowermentTimer--;
                Player.GetDamage(DamageClass.Summon) += 0.25f;
            }
            else
            {
                empowermentCooldown++;
                if (empowermentCooldown >= EmpowermentInterval)
                {
                    empowermentCooldown = 0;
                    empowermentTimer = EmpowermentDuration;
                }
            }
        }
    }
}
