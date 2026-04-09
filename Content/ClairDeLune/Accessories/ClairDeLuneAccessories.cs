using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;
using MagnumOpus.Content.ClairDeLune.ResonanceEnergies;
using MagnumOpus.Content.ClairDeLune.HarmonicCores;
using MagnumOpus.Content.Fate.CraftingStations;
using MagnumOpus.Content.Materials.EnemyDrops;

namespace MagnumOpus.Content.ClairDeLune.Accessories
{
    #region Shared Player

    public class ClairDeLuneAccessoryPlayer : ModPlayer
    {
        private static readonly Color LoreColor = new Color(150, 200, 255);

        // Accessory flags
        public bool reverieGauntletActive;
        public bool luminousReveriePendantActive;
        public bool dreambowClaspActive;
        public bool dreamsingerSigilActive;

        // Reverie Gauntlet: melee hit counter for Reverie proc
        public int meleeHitCounter;

        // Luminous Reverie Pendant: magic hit counter for Arabesque proc
        public int magicHitCounter;

        // Dreambow Clasp: ranged crit counter for Reflets dans l'Eau
        public int rangedCritCounter;

        public override void ResetEffects()
        {
            reverieGauntletActive = false;
            luminousReveriePendantActive = false;
            dreambowClaspActive = false;
            dreamsingerSigilActive = false;
        }

        public override bool FreeDodge(Player.HurtInfo info)
        {
            // Reverie: +5% dodge chance
            if (Player.HasBuff(ModContent.BuffType<ReverieBuff>()) && Main.rand.NextFloat() < 0.05f)
                return true;

            return false;
        }

        public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (reverieGauntletActive && item.DamageType == DamageClass.Melee)
                ProcessMeleeHit(target, hit, damageDone);

            // Kill checks
            if (target.life <= 0)
                ProcessKill(target);
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!proj.friendly) return;

            if (reverieGauntletActive && proj.DamageType == DamageClass.Melee)
                ProcessMeleeHit(target, hit, damageDone);

            if (luminousReveriePendantActive && proj.DamageType == DamageClass.Magic)
                ProcessMagicHit(target, hit, damageDone);

            if (dreambowClaspActive && proj.DamageType == DamageClass.Ranged)
                ProcessRangedHit(target, hit, damageDone);

            if (dreamsingerSigilActive && proj.minion)
                ProcessMinionHit(target, hit, damageDone);

            // Kill checks
            if (target.life <= 0)
                ProcessKill(target);
        }

        private void ProcessMeleeHit(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // 20% chance to apply Brumes
            if (Main.rand.NextFloat() < 0.20f)
            {
                var globalNPC = target.GetGlobalNPC<ClairDeLuneAccessoryGlobalNPC>();
                globalNPC.ApplyBrumes(180); // 3 seconds
            }

            // Every 8th hit -> Reverie buff
            meleeHitCounter++;
            if (meleeHitCounter >= 8)
            {
                meleeHitCounter = 0;
                bool isNight = !Main.dayTime;
                int duration = isNight ? 360 : 240; // 6s night, 4s day
                Player.AddBuff(ModContent.BuffType<ReverieBuff>(), duration);
            }
        }

        private void ProcessMagicHit(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // 10% chance to apply Voiles
            if (Main.rand.NextFloat() < 0.10f)
            {
                var globalNPC = target.GetGlobalNPC<ClairDeLuneAccessoryGlobalNPC>();
                globalNPC.ApplyVoiles(240); // 4 seconds
            }

            // Every 10th hit -> Arabesque buff
            magicHitCounter++;
            if (magicHitCounter >= 10)
            {
                magicHitCounter = 0;
                bool isNight = !Main.dayTime;
                int duration = isNight ? 420 : 300; // 7s night, 5s day
                Player.AddBuff(ModContent.BuffType<ArabesqueBuff>(), duration);
            }
        }

        private void ProcessRangedHit(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!hit.Crit) return;

            // Crits apply Pas sur la Neige
            var globalNPC = target.GetGlobalNPC<ClairDeLuneAccessoryGlobalNPC>();
            globalNPC.ApplyPasSurLaNeige(180); // 3 seconds

            // Every 5th crit -> Reflets dans l'Eau
            rangedCritCounter++;
            if (rangedCritCounter >= 5)
            {
                rangedCritCounter = 0;
                bool isNight = !Main.dayTime;
                int duration = isNight ? 360 : 240; // 6s night, 4s day
                Player.AddBuff(ModContent.BuffType<RefletsDansLEauBuff>(), duration);
            }
        }

        private void ProcessMinionHit(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // 10% chance to apply Berceuse
            if (Main.rand.NextFloat() < 0.10f)
            {
                var globalNPC = target.GetGlobalNPC<ClairDeLuneAccessoryGlobalNPC>();
                globalNPC.ApplyBerceuse(180); // 3 seconds
            }
        }

        private void ProcessKill(NPC target)
        {
            // Dreamsinger Sigil: kills grant Clair
            if (dreamsingerSigilActive)
            {
                bool isNight = !Main.dayTime;
                int duration = isNight ? 300 : 180; // 5s night, 3s day
                Player.AddBuff(ModContent.BuffType<ClairBuff>(), duration);
            }
        }
    }

    #endregion

    #region Reverie Gauntlet (Melee)

    public class ReverieGauntlet : ModItem
    {
        private static readonly Color IceBlue = new Color(150, 200, 255);

        public override string Texture => "MagnumOpus/Content/ClairDeLune/Accessories/ChronobladeGauntlet";

        public override void SetDefaults()
        {
            Item.width = 34;
            Item.height = 34;
            Item.accessory = true;
            Item.value = Item.sellPrice(platinum: 4);
            Item.rare = ModContent.RarityType<ClairDeLuneRarity>();
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "+60% melee damage, +35% melee speed, +20% melee crit"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "20% chance on hit to apply Brumes for 3s (-12% speed)"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Every 8th melee hit grants R\u00eaverie for 4s (+15% melee, +5% dodge). Night: 6s"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The gauntlet moves like a dream half-remembered — soft, slow, and impossible to escape'")
            {
                OverrideColor = IceBlue
            });
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetDamage(DamageClass.Melee) += 0.60f;
            player.GetAttackSpeed(DamageClass.Melee) += 0.35f;
            player.GetCritChance(DamageClass.Melee) += 20;
            player.GetModPlayer<ClairDeLuneAccessoryPlayer>().reverieGauntletActive = true;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<ResonantCoreOfClairDeLune>(), 15)
                .AddIngredient(ModContent.ItemType<ClairDeLuneResonantEnergy>(), 10)
                .AddIngredient(ModContent.ItemType<HarmonicCoreOfClairDeLune>(), 1)
                .AddIngredient(ModContent.ItemType<ShardOfClairDeLunesTempo>(), 5)
                .AddIngredient(ItemID.LunarBar, 10)
                .AddTile(ModContent.TileType<FatesCosmicAnvilTile>())
                .Register();
        }
    }

    #endregion

    #region Luminous Reverie Pendant (Magic)

    public class LuminousReveriePendant : ModItem
    {
        private static readonly Color IceBlue = new Color(150, 200, 255);

        public override string Texture => "MagnumOpus/Content/ClairDeLune/Accessories/FracturedHourglassPendant";

        public override void SetDefaults()
        {
            Item.width = 30;
            Item.height = 30;
            Item.accessory = true;
            Item.value = Item.sellPrice(platinum: 4);
            Item.rare = ModContent.RarityType<ClairDeLuneRarity>();
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "+55% magic damage, +30% magic crit, -25% mana cost"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "10% chance on hit to apply Voiles for 4s (15% miss chance)"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Every 10th magic hit grants Arabesque for 5s (+12% magic, -15% mana cost). Night: 7s"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'She wears the moon's light like a pendant — and the night softens wherever she walks'")
            {
                OverrideColor = IceBlue
            });
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetDamage(DamageClass.Magic) += 0.55f;
            player.GetCritChance(DamageClass.Magic) += 30;
            player.manaCost -= 0.25f;
            player.GetModPlayer<ClairDeLuneAccessoryPlayer>().luminousReveriePendantActive = true;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<ResonantCoreOfClairDeLune>(), 15)
                .AddIngredient(ModContent.ItemType<ClairDeLuneResonantEnergy>(), 10)
                .AddIngredient(ModContent.ItemType<HarmonicCoreOfClairDeLune>(), 1)
                .AddIngredient(ModContent.ItemType<LuneEssence>(), 8)
                .AddIngredient(ModContent.ItemType<ShardOfClairDeLunesTempo>(), 5)
                .AddIngredient(ItemID.LunarBar, 10)
                .AddTile(ModContent.TileType<FatesCosmicAnvilTile>())
                .Register();
        }
    }

    #endregion

    #region Dreambow Clasp (Ranged)

    public class DreambowClasp : ModItem
    {
        private static readonly Color IceBlue = new Color(150, 200, 255);

        public override string Texture => "MagnumOpus/Content/ClairDeLune/Accessories/ChronodisruptorOfHarmony";

        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.accessory = true;
            Item.value = Item.sellPrice(platinum: 4);
            Item.rare = ModContent.RarityType<ClairDeLuneRarity>();
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "+55% ranged damage, +30% ranged crit, 20% ammo save"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Ranged crits apply Pas sur la Neige for 3s (-15% speed)"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Every 5th ranged crit grants Reflets dans l'Eau for 4s (+12% ranged, +8% crit). Night: 6s"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The arrow dissolves into moonlight — and the moonlight finds its mark'")
            {
                OverrideColor = IceBlue
            });
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetDamage(DamageClass.Ranged) += 0.55f;
            player.GetCritChance(DamageClass.Ranged) += 30;
            player.ammoCost80 = true;
            player.GetModPlayer<ClairDeLuneAccessoryPlayer>().dreambowClaspActive = true;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<ResonantCoreOfClairDeLune>(), 15)
                .AddIngredient(ModContent.ItemType<ClairDeLuneResonantEnergy>(), 10)
                .AddIngredient(ModContent.ItemType<HarmonicCoreOfClairDeLune>(), 1)
                .AddIngredient(ModContent.ItemType<ShardOfClairDeLunesTempo>(), 5)
                .AddIngredient(ItemID.LunarBar, 10)
                .AddTile(ModContent.TileType<FatesCosmicAnvilTile>())
                .Register();
        }
    }

    #endregion

    #region Dreamsinger Sigil (Summoner)

    public class DreamsingerSigil : ModItem
    {
        private static readonly Color IceBlue = new Color(150, 200, 255);

        public override string Texture => "MagnumOpus/Content/ClairDeLune/Accessories/TimesingerSigil";

        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.accessory = true;
            Item.value = Item.sellPrice(platinum: 4);
            Item.rare = ModContent.RarityType<ClairDeLuneRarity>();
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "+65% summon damage, +4 max minions, +25% whip speed and range"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "10% minion hit chance to apply Berceuse for 3s (-5 defense, -10% speed)"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Kills grant Clair for 3s (+10% summon, +3 regen). Night: 5s"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The sigil hums a lullaby — and the dreaming servants answer in moonlit harmony'")
            {
                OverrideColor = IceBlue
            });
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetDamage(DamageClass.Summon) += 0.65f;
            player.maxMinions += 4;
            player.GetAttackSpeed(DamageClass.SummonMeleeSpeed) += 0.25f;
            player.whipRangeMultiplier += 0.25f;
            player.GetModPlayer<ClairDeLuneAccessoryPlayer>().dreamsingerSigilActive = true;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<ResonantCoreOfClairDeLune>(), 15)
                .AddIngredient(ModContent.ItemType<ClairDeLuneResonantEnergy>(), 10)
                .AddIngredient(ModContent.ItemType<HarmonicCoreOfClairDeLune>(), 1)
                .AddIngredient(ModContent.ItemType<ShardOfClairDeLunesTempo>(), 5)
                .AddIngredient(ItemID.LunarBar, 10)
                .AddTile(ModContent.TileType<FatesCosmicAnvilTile>())
                .Register();
        }
    }

    #endregion
}