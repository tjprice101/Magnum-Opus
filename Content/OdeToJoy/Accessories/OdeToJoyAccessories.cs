using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;
using MagnumOpus.Content.OdeToJoy.ResonanceEnergies;
using MagnumOpus.Content.OdeToJoy.HarmonicCores;
using MagnumOpus.Content.Fate.CraftingStations;
using MagnumOpus.Content.OdeToJoy;
using MagnumOpus.Content.Materials.EnemyDrops;

namespace MagnumOpus.Content.OdeToJoy.Accessories
{
    #region Shared ModPlayer

    public class OdeToJoyAccessoryPlayer : ModPlayer
    {
        // Conductor's Corsage
        public bool corsageActive;
        public int meleeHitCounter;

        // The Flowering Coda
        public bool floweringCodaActive;
        public int magicHitCounter;

        // Anthem's Arbalist
        public bool arbalistActive;
        public int rangedHitCounter;
        public int ovationStacks;
        public int ovationRefreshTimer;

        // The Verdant Refrain
        public bool verdantRefrainActive;
        public int refrainCooldown = 900;

        public override void ResetEffects()
        {
            corsageActive = false;
            floweringCodaActive = false;
            arbalistActive = false;
            verdantRefrainActive = false;
        }

        public override void PostUpdateEquips()
        {
            // Conductor's Corsage: +20 defense while actively swinging
            if (corsageActive && Player.ItemAnimationActive)
            {
                Player.statDefense += 20;
            }

            // Arbalist: Ovation stack timer
            if (ovationStacks > 0 && ovationRefreshTimer > 0)
            {
                ovationRefreshTimer--;
                if (ovationRefreshTimer <= 0)
                    ovationStacks = 0;
            }

            // Verdant Refrain: Refrain cooldown
            if (verdantRefrainActive)
            {
                refrainCooldown--;
                if (refrainCooldown <= 0)
                {
                    Player.AddBuff(ModContent.BuffType<RefrainBuff>(), 180); // 3 seconds
                    refrainCooldown = 900; // 15 seconds
                }
            }
        }

        public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (corsageActive && item.DamageType == DamageClass.Melee)
                ProcessCorsageMeleeHit(target, hit, damageDone);
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (corsageActive && proj.DamageType == DamageClass.Melee && proj.friendly)
                ProcessCorsageMeleeHit(target, hit, damageDone);

            if (floweringCodaActive && proj.DamageType == DamageClass.Magic && proj.friendly)
                ProcessFloweringCodaHit(target, hit, damageDone);

            if (arbalistActive && proj.DamageType == DamageClass.Ranged && proj.friendly)
                ProcessArbalistHit(target, hit, damageDone);

            if (verdantRefrainActive && proj.minion && proj.friendly)
                ProcessVerdantRefrainMinionHit(target, hit, damageDone);
        }

        private void ProcessCorsageMeleeHit(NPC target, NPC.HitInfo hit, int damageDone)
        {
            meleeHitCounter++;
            if (meleeHitCounter >= 8)
            {
                meleeHitCounter = 0;
                Player.AddBuff(ModContent.BuffType<JubilantTempoBuff>(), 300); // 5 seconds
            }

            // 3% lifesteal during Jubilant Tempo (cap 25 HP)
            if (Player.HasBuff(ModContent.BuffType<JubilantTempoBuff>()))
            {
                int healAmount = System.Math.Min((int)(damageDone * 0.03f), 25);
                if (healAmount > 0) Player.Heal(healAmount);
            }
        }

        private void ProcessFloweringCodaHit(NPC target, NPC.HitInfo hit, int damageDone)
        {
            magicHitCounter++;
            if (magicHitCounter >= 10)
            {
                magicHitCounter = 0;
                Player.AddBuff(ModContent.BuffType<JoyousBloomBuff>(), 300); // 5 seconds
            }

            // Mana restore during Joyous Bloom
            if (Player.HasBuff(ModContent.BuffType<JoyousBloomBuff>()))
            {
                Player.statMana = System.Math.Min(Player.statMana + 5, Player.statManaMax2);
            }
        }

        private void ProcessArbalistHit(NPC target, NPC.HitInfo hit, int damageDone)
        {
            rangedHitCounter++;
            if (rangedHitCounter >= 6)
            {
                rangedHitCounter = 0;
                Player.AddBuff(ModContent.BuffType<TriumphantVolleyBuff>(), 240); // 4 seconds
            }

            // Check kill for Ovation
            if (target.life <= 0)
            {
                ovationStacks = System.Math.Min(ovationStacks + 1, 3);
                ovationRefreshTimer = 300; // 5 seconds
                Player.AddBuff(ModContent.BuffType<OvationBuff>(), 300);
            }
        }

        private void ProcessVerdantRefrainMinionHit(NPC target, NPC.HitInfo hit, int damageDone)
        {
            bool hasRefrain = Player.HasBuff(ModContent.BuffType<RefrainBuff>());
            float procChance = hasRefrain ? 0.20f : 0.10f;

            if (Main.rand.NextFloat() < procChance)
            {
                target.GetGlobalNPC<OdeToJoyAccessoryGlobalNPC>().ApplyHymnalAnchor(target);
            }
        }
    }

    #endregion

    #region Conductor's Corsage (Melee)

    public class ConductorsCorsage : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 36;
            Item.height = 36;
            Item.accessory = true;
            Item.value = Item.sellPrice(platinum: 3);
            Item.rare = ModContent.RarityType<OdeToJoyRarity>();
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "+65% melee damage, +40% melee speed, +20 defense while swinging"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Every 8th melee hit grants Jubilant Tempo for 5s (+15% melee damage, +20% attack speed, 3% lifesteal)"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The corsage upon the conductor's heart — every beat of the baton brings joy to the hall'")
            {
                OverrideColor = new Color(255, 200, 50)
            });
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetDamage(DamageClass.Melee) += 0.65f;
            player.GetAttackSpeed(DamageClass.Melee) += 0.40f;
            player.GetModPlayer<OdeToJoyAccessoryPlayer>().corsageActive = true;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<ResonantCoreOfOdeToJoy>(), 18)
                .AddIngredient(ModContent.ItemType<OdeToJoyResonantEnergy>(), 12)
                .AddIngredient(ModContent.ItemType<HarmonicCoreOfOdeToJoy>(), 1)
                .AddIngredient(ModContent.ItemType<JoyEssence>(), 8)
                .AddIngredient(ModContent.ItemType<ShardOfOdeToJoysTempo>(), 5)
                .AddIngredient(ItemID.LunarBar, 15)
                .AddTile(ModContent.TileType<FatesCosmicAnvilTile>())
                .Register();
        }
    }

    #endregion

    #region The Flowering Coda (Magic)

    public class TheFloweringCoda : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 34;
            Item.height = 34;
            Item.accessory = true;
            Item.value = Item.sellPrice(platinum: 3);
            Item.rare = ModContent.RarityType<OdeToJoyRarity>();
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "+60% magic damage, +35% magic crit, -30% mana cost"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Every 10th magic hit grants Joyous Bloom for 5s (+15% magic damage, 5 mana per hit)"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The coda blooms — and every note flowers into golden light'")
            {
                OverrideColor = new Color(255, 200, 50)
            });
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetDamage(DamageClass.Magic) += 0.60f;
            player.GetCritChance(DamageClass.Magic) += 35;
            player.manaCost -= 0.30f;
            player.GetModPlayer<OdeToJoyAccessoryPlayer>().floweringCodaActive = true;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<ResonantCoreOfOdeToJoy>(), 18)
                .AddIngredient(ModContent.ItemType<OdeToJoyResonantEnergy>(), 12)
                .AddIngredient(ModContent.ItemType<HarmonicCoreOfOdeToJoy>(), 1)
                .AddIngredient(ModContent.ItemType<JoyEssence>(), 8)
                .AddIngredient(ModContent.ItemType<ShardOfOdeToJoysTempo>(), 5)
                .AddIngredient(ItemID.LunarBar, 15)
                .AddTile(ModContent.TileType<FatesCosmicAnvilTile>())
                .Register();
        }
    }

    #endregion

    #region Anthem's Arbalist (Ranged) — replaces Symphony of Blossoms

    public class AnthemsArbalist : ModItem
    {
        public override string Texture => "MagnumOpus/Content/OdeToJoy/Accessories/SymphonyOfBlossoms";

        public override void SetDefaults()
        {
            Item.width = 30;
            Item.height = 32;
            Item.accessory = true;
            Item.value = Item.sellPrice(platinum: 3);
            Item.rare = ModContent.RarityType<OdeToJoyRarity>();
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "+60% ranged damage, +25% ranged crit, +20% ranged attack speed"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Every 6th ranged hit grants Triumphant Volley for 4s (+20% ranged damage, +10% attack speed)"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Ranged kills grant Ovation for 5s (+10% all damage, +5% crit; stacks 3x)"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The anthem rings out — and every arrow sings in celebration'")
            {
                OverrideColor = new Color(255, 200, 50)
            });
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetDamage(DamageClass.Ranged) += 0.60f;
            player.GetCritChance(DamageClass.Ranged) += 25;
            player.GetAttackSpeed(DamageClass.Ranged) += 0.20f;
            player.GetModPlayer<OdeToJoyAccessoryPlayer>().arbalistActive = true;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<ResonantCoreOfOdeToJoy>(), 18)
                .AddIngredient(ModContent.ItemType<OdeToJoyResonantEnergy>(), 12)
                .AddIngredient(ModContent.ItemType<HarmonicCoreOfOdeToJoy>(), 1)
                .AddIngredient(ModContent.ItemType<JoyEssence>(), 8)
                .AddIngredient(ModContent.ItemType<ShardOfOdeToJoysTempo>(), 5)
                .AddIngredient(ItemID.LunarBar, 15)
                .AddTile(ModContent.TileType<FatesCosmicAnvilTile>())
                .Register();
        }
    }

    #endregion

    #region The Verdant Refrain (Summoner)

    public class TheVerdantRefrain : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.accessory = true;
            Item.value = Item.sellPrice(platinum: 3);
            Item.rare = ModContent.RarityType<OdeToJoyRarity>();
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "+70% summon damage, +4 max minions, +25% whip speed and range"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Minion hits have 10% chance to apply Hymnal Anchor for 3s (-20% speed, -5 defense)"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Every 15s, gain Refrain for 3s (+25% minion damage, doubled Hymnal Anchor chance)"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The refrain echoes endlessly — and every servant joins the chorus with joy'")
            {
                OverrideColor = new Color(255, 200, 50)
            });
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetDamage(DamageClass.Summon) += 0.70f;
            player.maxMinions += 4;
            player.GetAttackSpeed(DamageClass.SummonMeleeSpeed) += 0.25f;
            player.whipRangeMultiplier += 0.25f;
            player.GetModPlayer<OdeToJoyAccessoryPlayer>().verdantRefrainActive = true;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<ResonantCoreOfOdeToJoy>(), 18)
                .AddIngredient(ModContent.ItemType<OdeToJoyResonantEnergy>(), 12)
                .AddIngredient(ModContent.ItemType<HarmonicCoreOfOdeToJoy>(), 1)
                .AddIngredient(ModContent.ItemType<JoyEssence>(), 8)
                .AddIngredient(ModContent.ItemType<ShardOfOdeToJoysTempo>(), 5)
                .AddIngredient(ItemID.LunarBar, 15)
                .AddTile(ModContent.TileType<FatesCosmicAnvilTile>())
                .Register();
        }
    }

    #endregion
}