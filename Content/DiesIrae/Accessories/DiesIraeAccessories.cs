using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;
using MagnumOpus.Content.DiesIrae.ResonanceEnergies;
using MagnumOpus.Content.DiesIrae.HarmonicCores;
using MagnumOpus.Content.Fate.CraftingStations;
using MagnumOpus.Content.Materials.EnemyDrops;

namespace MagnumOpus.Content.DiesIrae.Accessories
{
    #region Shared ModPlayer

    public class DiesIraeAccessoryPlayer : ModPlayer
    {
        // Chain of Final Judgment
        public bool chainActive;
        public int meleeKillCounter;

        // Ember of the Condemned
        public bool emberActive;

        // Requiem's Shackle
        public bool shackleActive;

        // Seal of Damnation
        public bool sealActive;

        public override void ResetEffects()
        {
            chainActive = false;
            emberActive = false;
            shackleActive = false;
            sealActive = false;
        }

        public override void PostUpdateEquips()
        {
        }

        public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (chainActive && item.DamageType == DamageClass.Melee)
                ProcessChainMeleeHit(target, hit, damageDone);
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (chainActive && proj.DamageType == DamageClass.Melee && proj.friendly)
                ProcessChainMeleeHit(target, hit, damageDone);

            // Ember: magic crits apply Confutatis
            if (emberActive && proj.DamageType == DamageClass.Magic && proj.friendly && hit.Crit)
                target.GetGlobalNPC<DiesIraeAccessoryGlobalNPC>().ApplyConfutatis(target);

            // Shackle: ranged crits apply Chains of Requiem
            if (shackleActive && proj.DamageType == DamageClass.Ranged && proj.friendly && hit.Crit)
                target.GetGlobalNPC<DiesIraeAccessoryGlobalNPC>().ApplyChainsOfRequiem(target);

            // Seal: 15% minion hit applies Condemned
            if (sealActive && proj.minion && proj.friendly)
            {
                if (Main.rand.NextFloat() < 0.15f)
                    target.GetGlobalNPC<DiesIraeAccessoryGlobalNPC>().ApplyCondemned(target);
            }
        }

        private void ProcessChainMeleeHit(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Execute: crits on non-boss enemies below 15% HP
            if (hit.Crit && !target.boss && target.life < target.lifeMax * 0.15f)
            {
                target.life = 0;
                target.HitEffect();
                target.checkDead();
            }

            // Day of Wrath: inflict Cursed Inferno
            if (Player.HasBuff(ModContent.BuffType<DayOfWrathBuff>()))
                target.AddBuff(BuffID.CursedInferno, 300);

            // Every 10th melee kill grants Day of Wrath
            if (target.life <= 0)
            {
                meleeKillCounter++;
                if (meleeKillCounter >= 10)
                {
                    meleeKillCounter = 0;
                    Player.AddBuff(ModContent.BuffType<DayOfWrathBuff>(), 360); // 6 seconds
                }
            }
        }
    }

    #endregion

    #region Chain of Final Judgment (Melee)

    public class ChainOfFinalJudgment : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 34;
            Item.height = 34;
            Item.accessory = true;
            Item.value = Item.sellPrice(platinum: 2);
            Item.rare = ModContent.RarityType<DiesIraeRarity>();
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "+50% melee damage, +30% melee speed, +15% melee crit"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Crits execute non-boss enemies below 15% HP"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Every 10th melee kill grants Day of Wrath for 6s (+30% melee damage, Cursed Inferno on hit)"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The chain tightens. The verdict is absolute. There is no appeal.'")
            {
                OverrideColor = new Color(200, 50, 30)
            });
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetDamage(DamageClass.Melee) += 0.50f;
            player.GetAttackSpeed(DamageClass.Melee) += 0.30f;
            player.GetCritChance(DamageClass.Melee) += 15;
            player.GetModPlayer<DiesIraeAccessoryPlayer>().chainActive = true;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<ResonantCoreOfDiesIrae>(), 15)
                .AddIngredient(ModContent.ItemType<DiesIraeResonantEnergy>(), 10)
                .AddIngredient(ModContent.ItemType<HarmonicCoreOfDiesIrae>(), 1)
                .AddIngredient(ModContent.ItemType<ShardOfDiesIraesTempo>(), 5)
                .AddIngredient(ItemID.LunarBar, 10)
                .AddTile(ModContent.TileType<FatesCosmicAnvilTile>())
                .Register();
        }
    }

    #endregion

    #region Ember of the Condemned (Magic)

    public class EmberOfTheCondemned : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 30;
            Item.height = 30;
            Item.accessory = true;
            Item.value = Item.sellPrice(platinum: 2);
            Item.rare = ModContent.RarityType<DiesIraeRarity>();
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "+45% magic damage, +25% magic crit, -20% mana cost"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Magic crits apply Confutatis for 4s (-10 defense, +15% damage taken)"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'A cinder from the pyre that burns beneath all things — the fire that remembers every sin'")
            {
                OverrideColor = new Color(200, 50, 30)
            });
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetDamage(DamageClass.Magic) += 0.45f;
            player.GetCritChance(DamageClass.Magic) += 25;
            player.manaCost -= 0.20f;
            player.GetModPlayer<DiesIraeAccessoryPlayer>().emberActive = true;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<ResonantCoreOfDiesIrae>(), 15)
                .AddIngredient(ModContent.ItemType<DiesIraeResonantEnergy>(), 10)
                .AddIngredient(ModContent.ItemType<HarmonicCoreOfDiesIrae>(), 1)
                .AddIngredient(ModContent.ItemType<ShardOfDiesIraesTempo>(), 5)
                .AddIngredient(ItemID.LunarBar, 10)
                .AddTile(ModContent.TileType<FatesCosmicAnvilTile>())
                .Register();
        }
    }

    #endregion

    #region Requiem's Shackle (Ranged)

    public class RequiemsShackle : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 30;
            Item.accessory = true;
            Item.value = Item.sellPrice(platinum: 2);
            Item.rare = ModContent.RarityType<DiesIraeRarity>();
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "+50% ranged damage, +30% ranged crit, 25% chance to save ammo"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Ranged crits apply Chains of Requiem for 4s (-25% speed, +15% damage taken, no regen)"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The shackle remembers every soul it has bound — and it is never satisfied'")
            {
                OverrideColor = new Color(200, 50, 30)
            });
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetDamage(DamageClass.Ranged) += 0.50f;
            player.GetCritChance(DamageClass.Ranged) += 30;
            player.ammoCost75 = true;
            player.GetModPlayer<DiesIraeAccessoryPlayer>().shackleActive = true;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<ResonantCoreOfDiesIrae>(), 15)
                .AddIngredient(ModContent.ItemType<DiesIraeResonantEnergy>(), 10)
                .AddIngredient(ModContent.ItemType<HarmonicCoreOfDiesIrae>(), 1)
                .AddIngredient(ModContent.ItemType<ShardOfDiesIraesTempo>(), 5)
                .AddIngredient(ItemID.LunarBar, 10)
                .AddTile(ModContent.TileType<FatesCosmicAnvilTile>())
                .Register();
        }
    }

    #endregion

    #region Seal of Damnation (Summoner)

    public class SealOfDamnation : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.accessory = true;
            Item.value = Item.sellPrice(platinum: 2);
            Item.rare = ModContent.RarityType<DiesIraeRarity>();
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "+55% summon damage, +3 max minions, +20% whip speed and range"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Minion hits have 15% chance to apply Condemned for 5s (+15% minion damage taken, -5 defense)"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The seal is branded in blood, and the damned serve judgment eternal'")
            {
                OverrideColor = new Color(200, 50, 30)
            });
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetDamage(DamageClass.Summon) += 0.55f;
            player.maxMinions += 3;
            player.GetAttackSpeed(DamageClass.SummonMeleeSpeed) += 0.20f;
            player.whipRangeMultiplier += 0.20f;
            player.GetModPlayer<DiesIraeAccessoryPlayer>().sealActive = true;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<ResonantCoreOfDiesIrae>(), 15)
                .AddIngredient(ModContent.ItemType<DiesIraeResonantEnergy>(), 10)
                .AddIngredient(ModContent.ItemType<HarmonicCoreOfDiesIrae>(), 1)
                .AddIngredient(ModContent.ItemType<WrathEssence>(), 8)
                .AddIngredient(ModContent.ItemType<ShardOfDiesIraesTempo>(), 5)
                .AddIngredient(ItemID.LunarBar, 10)
                .AddTile(ModContent.TileType<FatesCosmicAnvilTile>())
                .Register();
        }
    }

    // Legacy: keep SealOfDamnationGlobalNPC as empty shell for backward compat
    public class SealOfDamnationGlobalNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;
        // All logic moved to DiesIraeAccessoryGlobalNPC.OnKill for Recordare spread
    }

    #endregion

    #region Libera Me Pierce Tracker

    #endregion
}