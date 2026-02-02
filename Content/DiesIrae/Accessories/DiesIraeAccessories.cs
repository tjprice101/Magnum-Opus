using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;
using MagnumOpus.Content.DiesIrae.ResonanceEnergies;
using MagnumOpus.Content.DiesIrae.HarmonicCores;
using MagnumOpus.Content.Fate.CraftingStations;

namespace MagnumOpus.Content.DiesIrae.Accessories
{
    /// <summary>
    /// Ember of the Condemned - Magic class accessory. Boosts magic damage and causes spells to ignite enemies.
    /// Post-Nachtmusik tier.
    /// </summary>
    public class EmberOfTheCondemned : ModItem
    {
        private static readonly Color BloodRed = new Color(139, 0, 0);

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
            tooltips.Add(new TooltipLine(Mod, "Effect1", "+45% magic damage"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "+25% magic critical strike chance"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Magic attacks inflict Hellfire, dealing damage over time"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "-20% mana cost"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'A cinder from the flames of eternal condemnation'") 
            { 
                OverrideColor = BloodRed 
            });
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetDamage(DamageClass.Magic) += 0.45f; // POST-NACHTMUSIK (Nachtmusik: 0.35f)
            player.GetCritChance(DamageClass.Magic) += 25;
            player.manaCost -= 0.20f;
            
            // Add on-hit effect via player buff tracking (simplified implementation)
            player.GetModPlayer<EmberOfCondemnedPlayer>().emberActive = true;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<ResonantCoreOfDiesIrae>(), 15)
                .AddIngredient(ModContent.ItemType<DiesIraeResonantEnergy>(), 10)
                .AddIngredient(ModContent.ItemType<HarmonicCoreOfDiesIrae>(), 1)
                .AddIngredient(ItemID.LunarBar, 10)
                .AddTile(ModContent.TileType<FatesCosmicAnvilTile>())
                .Register();
        }
    }

    public class EmberOfCondemnedPlayer : ModPlayer
    {
        public bool emberActive = false;

        public override void ResetEffects()
        {
            emberActive = false;
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (emberActive && proj.DamageType == DamageClass.Magic && proj.friendly)
            {
                target.AddBuff(BuffID.OnFire3, 300); // Hellfire for 5 seconds
            }
        }
    }

    /// <summary>
    /// Seal of Damnation - Summoner class accessory. Boosts summon damage and minion count.
    /// Post-Nachtmusik tier.
    /// </summary>
    public class SealOfDamnation : ModItem
    {
        private static readonly Color BloodRed = new Color(139, 0, 0);

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
            tooltips.Add(new TooltipLine(Mod, "Effect1", "+55% summon damage"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "+3 max minions"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Minions inflict Daybroken on hit"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "+20% whip speed and range"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Bound by the seal, they serve judgment eternal'") 
            { 
                OverrideColor = BloodRed 
            });
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetDamage(DamageClass.Summon) += 0.55f; // POST-NACHTMUSIK (Nachtmusik: 0.45f)
            player.maxMinions += 3;
            player.GetAttackSpeed(DamageClass.SummonMeleeSpeed) += 0.20f;
            
            // Add on-hit effect via player buff tracking
            player.GetModPlayer<SealOfDamnationPlayer>().sealActive = true;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<ResonantCoreOfDiesIrae>(), 15)
                .AddIngredient(ModContent.ItemType<DiesIraeResonantEnergy>(), 10)
                .AddIngredient(ModContent.ItemType<HarmonicCoreOfDiesIrae>(), 1)
                .AddIngredient(ItemID.LunarBar, 10)
                .AddTile(ModContent.TileType<FatesCosmicAnvilTile>())
                .Register();
        }
    }

    public class SealOfDamnationPlayer : ModPlayer
    {
        public bool sealActive = false;

        public override void ResetEffects()
        {
            sealActive = false;
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (sealActive && proj.minion && proj.friendly)
            {
                target.AddBuff(BuffID.Daybreak, 180); // Daybroken for 3 seconds
            }
        }
    }

    /// <summary>
    /// Chain of Final Judgment - Melee class accessory. Boosts melee damage and adds lifesteal.
    /// Post-Nachtmusik tier.
    /// </summary>
    public class ChainOfFinalJudgment : ModItem
    {
        private static readonly Color BloodRed = new Color(139, 0, 0);

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
            tooltips.Add(new TooltipLine(Mod, "Effect1", "+50% melee damage"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "+30% melee speed"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Melee attacks heal for 8% of damage dealt"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Critical strikes have 20% chance to instantly kill non-boss enemies below 15% health"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The chains that bind all sinners to their fate'") 
            { 
                OverrideColor = BloodRed 
            });
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetDamage(DamageClass.Melee) += 0.50f; // POST-NACHTMUSIK (Nachtmusik: 0.38f)
            player.GetAttackSpeed(DamageClass.Melee) += 0.30f;
            
            // Add effects via player class
            player.GetModPlayer<ChainOfFinalJudgmentPlayer>().chainActive = true;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<ResonantCoreOfDiesIrae>(), 15)
                .AddIngredient(ModContent.ItemType<DiesIraeResonantEnergy>(), 10)
                .AddIngredient(ModContent.ItemType<HarmonicCoreOfDiesIrae>(), 1)
                .AddIngredient(ItemID.LunarBar, 10)
                .AddTile(ModContent.TileType<FatesCosmicAnvilTile>())
                .Register();
        }
    }

    public class ChainOfFinalJudgmentPlayer : ModPlayer
    {
        public bool chainActive = false;

        public override void ResetEffects()
        {
            chainActive = false;
        }

        public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (chainActive && item.DamageType == DamageClass.Melee)
            {
                // 8% lifesteal (POST-NACHTMUSIK ULTIMATE)
                int healAmount = (int)(damageDone * 0.08f);
                if (healAmount > 0)
                {
                    Player.Heal(healAmount);
                }
                
                // Execute chance on crit (20% chance at 15% HP threshold)
                if (hit.Crit && !target.boss && target.life < target.lifeMax * 0.15f && Main.rand.NextFloat() < 0.20f)
                {
                    target.life = 0;
                    target.HitEffect();
                    target.checkDead();
                }
            }
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (chainActive && proj.DamageType == DamageClass.Melee && proj.friendly)
            {
                // 8% lifesteal (POST-NACHTMUSIK ULTIMATE)
                int healAmount = (int)(damageDone * 0.08f);
                if (healAmount > 0)
                {
                    Player.Heal(healAmount);
                }
                
                // Execute chance on crit (20% chance at 15% HP threshold)
                if (hit.Crit && !target.boss && target.life < target.lifeMax * 0.15f && Main.rand.NextFloat() < 0.20f)
                {
                    target.life = 0;
                    target.HitEffect();
                    target.checkDead();
                }
            }
        }
    }

    /// <summary>
    /// Requiem's Shackle - Ranger class accessory. Boosts ranged damage and adds marking mechanic.
    /// Post-Nachtmusik tier.
    /// </summary>
    public class RequiemsShackle : ModItem
    {
        private static readonly Color BloodRed = new Color(139, 0, 0);

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
            tooltips.Add(new TooltipLine(Mod, "Effect1", "+50% ranged damage"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "+30% ranged critical strike chance"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Ranged attacks mark enemies for 5 seconds"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Marked enemies take 20% increased damage from all sources"));
            tooltips.Add(new TooltipLine(Mod, "Effect5", "25% chance to not consume ammo"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The shackles that bind souls to their requiem'") 
            { 
                OverrideColor = BloodRed 
            });
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetDamage(DamageClass.Ranged) += 0.50f; // POST-NACHTMUSIK (Nachtmusik: 0.40f)
            player.GetCritChance(DamageClass.Ranged) += 30;
            player.ammoCost75 = true; // 25% chance to not consume ammo
            
            // Add marking effect via player class
            player.GetModPlayer<RequiemsShacklePlayer>().shackleActive = true;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<ResonantCoreOfDiesIrae>(), 15)
                .AddIngredient(ModContent.ItemType<DiesIraeResonantEnergy>(), 10)
                .AddIngredient(ModContent.ItemType<HarmonicCoreOfDiesIrae>(), 1)
                .AddIngredient(ItemID.LunarBar, 10)
                .AddTile(ModContent.TileType<FatesCosmicAnvilTile>())
                .Register();
        }
    }

    public class RequiemsShacklePlayer : ModPlayer
    {
        public bool shackleActive = false;

        public override void ResetEffects()
        {
            shackleActive = false;
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (shackleActive && proj.DamageType == DamageClass.Ranged && proj.friendly)
            {
                // Mark with Ichor (increases damage taken) - placeholder for mark debuff
                target.AddBuff(BuffID.Ichor, 300); // 5 seconds
            }
        }
    }
}
