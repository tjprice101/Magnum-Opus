using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework;
using MagnumOpus.Content.Autumn.Materials;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems;
using static MagnumOpus.Common.Systems.ThemedParticles;

namespace MagnumOpus.Content.Autumn.Accessories
{
    /// <summary>
    /// Reaper's Charm - Life Steal Autumn accessory
    /// Post-Wall of Flesh tier, provides life steal on hits
    /// </summary>
    public class ReapersCharm : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 26;
            Item.height = 26;
            Item.accessory = true;
            Item.rare = ItemRarityID.Lime;
            Item.value = Item.sellPrice(gold: 7);
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetDamage(DamageClass.Generic) += 0.07f;
            
            // Life steal handled via OnHit
            player.GetModPlayer<ReapersCharmPlayer>().reapersCharmEquipped = true;
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            Color autumnOrange = new Color(255, 100, 30);
            Color autumnBrown = new Color(139, 69, 19);
            
            tooltips.Add(new TooltipLine(Mod, "Season", "Autumn Accessory") { OverrideColor = autumnOrange });
            tooltips.Add(new TooltipLine(Mod, "Damage", "+7% damage") { OverrideColor = autumnBrown });
            tooltips.Add(new TooltipLine(Mod, "LifeSteal", "20% chance to life steal 4% of damage dealt (max 25 HP)") { OverrideColor = autumnOrange });
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The reaper claims a portion of every soul'") { OverrideColor = Color.Lerp(autumnBrown, Color.Black, 0.3f) });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<AutumnResonantEnergy>(), 2)
                .AddIngredient(ModContent.ItemType<DecayEssence>(), 4)
                .AddIngredient(ModContent.ItemType<LeafOfEnding>(), 12)
                .AddIngredient(ModContent.ItemType<DeathsNote>(), 1)
                .AddIngredient(ItemID.Megashark, 1)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
    
    public class ReapersCharmPlayer : ModPlayer
    {
        public bool reapersCharmEquipped = false;
        
        public override void ResetEffects()
        {
            reapersCharmEquipped = false;
        }
        
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (reapersCharmEquipped && Main.rand.NextBool(5)) // 20% chance
            {
                int healAmount = (int)(damageDone * 0.04f); // 4% lifesteal
                healAmount = System.Math.Max(1, System.Math.Min(healAmount, 25)); // Cap at 25 HP
                Player.Heal(healAmount);
                
                // Minimal lifesteal dust
                for (int i = 0; i < 2; i++)
                {
                    Dust d = Dust.NewDustDirect(target.Center + Main.rand.NextVector2Circular(10f, 10f),
                        0, 0, DustID.Torch, 0f, -1f, 100, default, 0.8f);
                    d.noGravity = true;
                }
            }
        }
    }
    
    /// <summary>
    /// Twilight Ring - Crit-based Autumn accessory
    /// Post-Wall of Flesh tier, boosts crit chance and crit damage
    /// </summary>
    public class TwilightRing : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.accessory = true;
            Item.rare = ItemRarityID.Lime;
            Item.value = Item.sellPrice(gold: 7);
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetCritChance(DamageClass.Generic) += 10;
            player.GetDamage(DamageClass.Generic) += 0.10f;
            
            // Boosted to +20% total during night
            if (!Main.dayTime)
                player.GetDamage(DamageClass.Generic) += 0.10f;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            Color autumnOrange = new Color(255, 100, 30);
            Color twilightPurple = new Color(128, 64, 96);
            
            tooltips.Add(new TooltipLine(Mod, "Season", "Autumn Accessory") { OverrideColor = autumnOrange });
            tooltips.Add(new TooltipLine(Mod, "Crit", "+10% critical strike chance") { OverrideColor = twilightPurple });
            tooltips.Add(new TooltipLine(Mod, "Damage", "+10% damage (boosted to +20% during night)") { OverrideColor = twilightPurple });
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The boundary between day and night holds great power'") { OverrideColor = Color.Lerp(autumnOrange, twilightPurple, 0.5f) });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<AutumnResonantEnergy>(), 2)
                .AddIngredient(ModContent.ItemType<DecayEssence>(), 4)
                .AddIngredient(ModContent.ItemType<LeafOfEnding>(), 12)
                .AddIngredient(ModContent.ItemType<TwilightWingFragment>(), 1)
                .AddIngredient(ItemID.MoonStone, 1)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
    
    /// <summary>
    /// Harvest Mantle - Tank Autumn accessory
    /// Post-Wall of Flesh tier, provides significant defense and thorns
    /// </summary>
    public class HarvestMantle : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 30;
            Item.height = 30;
            Item.accessory = true;
            Item.rare = ItemRarityID.Yellow;
            Item.value = Item.sellPrice(gold: 8);
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.statDefense += 12;
            player.endurance += 0.08f; // 8% damage reduction
            player.thorns = 1f; // Thorns damage
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            Color autumnOrange = new Color(255, 100, 30);
            Color harvestGold = new Color(218, 165, 32);

            tooltips.Add(new TooltipLine(Mod, "Season", "Autumn Accessory") { OverrideColor = autumnOrange });
            tooltips.Add(new TooltipLine(Mod, "Defense", "+12 defense") { OverrideColor = harvestGold });
            tooltips.Add(new TooltipLine(Mod, "DR", "+8% damage reduction") { OverrideColor = harvestGold });
            tooltips.Add(new TooltipLine(Mod, "Thorns", "Reflects 100% of contact damage back to attackers") { OverrideColor = autumnOrange });
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The harvest provides both bounty and protection'") { OverrideColor = Color.Lerp(harvestGold, Color.White, 0.3f) });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<AutumnResonantEnergy>(), 3)
                .AddIngredient(ModContent.ItemType<DecayEssence>(), 6)
                .AddIngredient(ModContent.ItemType<LeafOfEnding>(), 15)
                .AddIngredient(ItemID.PaladinsShield, 1)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }

    /// <summary>
    /// Autumn accessories VFX player hook for TwilightRing and HarvestMantle
    /// </summary>
    public class AutumnAccessoriesPlayer : ModPlayer
    {
        public bool twilightRingEquipped = false;
        public bool harvestMantleEquipped = false;

        public override void ResetEffects()
        {
            twilightRingEquipped = false;
            harvestMantleEquipped = false;
        }

        public override void UpdateEquips()
        {
            if (Player.HasItem(ModContent.ItemType<TwilightRing>()))
                twilightRingEquipped = true;
            if (Player.HasItem(ModContent.ItemType<HarvestMantle>()))
                harvestMantleEquipped = true;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (twilightRingEquipped)
            {
                for (int i = 0; i < 2; i++)
                {
                    Dust d = Dust.NewDustDirect(target.Center + Main.rand.NextVector2Circular(10f, 10f),
                        0, 0, DustID.PurpleTorch, 0f, -1f, 100, default, 0.8f);
                    d.noGravity = true;
                }
            }

            if (harvestMantleEquipped)
            {
                for (int i = 0; i < 2; i++)
                {
                    Dust d = Dust.NewDustDirect(target.Center + Main.rand.NextVector2Circular(10f, 10f),
                        0, 0, DustID.Copper, 0f, -1f, 100, default, 0.8f);
                    d.noGravity = true;
                }
            }
        }
    }
}
