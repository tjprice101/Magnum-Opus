using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems;
using MagnumOpus.Content.Common.Accessories;
using MagnumOpus.Content.Eroica.HarmonicCores;
using MagnumOpus.Content.Eroica.ResonanceEnergies;
using MagnumOpus.Content.MoonlightSonata.CraftingStations;
using System;
using System.Collections.Generic;
using MagnumOpus.Content.Materials.EnemyDrops;

namespace MagnumOpus.Content.Eroica.Accessories.Shared
{
    #region Badge of Valor

    /// <summary>
    /// Badge of Valor - Eroica Tier 1 Theme Accessory.
    /// 20% for melee attacks to deal double damage on impact.
    /// 5% chance for next 10 melee swings to deal 2x critical hits.
    /// </summary>
    public class BadgeOfValor : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 28;
            Item.accessory = true;
            Item.value = Item.buyPrice(platinum: 1);
            Item.rare = ModContent.RarityType<EroicaRarity>();
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<BadgeOfValorPlayer>().hasBadgeOfValor = true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "DoubleDamage", "20% chance for melee attacks to deal double damage")
            {
                OverrideColor = EroicaPalette.Crimson
            });

            tooltips.Add(new TooltipLine(Mod, "RedCrit", "5% chance for next 10 melee swings to deal 2x critical hits")
            {
                OverrideColor = EroicaPalette.Gold
            });

            tooltips.Add(new TooltipLine(Mod, "Flavor", "'Worn by those who charge into battle without hesitation'")
            {
                OverrideColor = new Color(200, 50, 50)
            });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<ResonantCoreOfEroica>(), 15)
                .AddIngredient(ModContent.ItemType<MelodicCharm>(), 1)
                .AddIngredient(ModContent.ItemType<EroicasResonantEnergy>(), 1)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }

    public class BadgeOfValorPlayer : ModPlayer
    {
        public bool hasBadgeOfValor;
        public int redCritSwingsRemaining;

        public override void ResetEffects()
        {
            hasBadgeOfValor = false;
        }

        public override void ModifyHitNPCWithItem(Item item, NPC target, ref NPC.HitModifiers modifiers)
        {
            if (!hasBadgeOfValor) return;
            if (item.DamageType != DamageClass.Melee) return;
            ApplyMeleeBonuses(ref modifiers);
        }

        public override void ModifyHitNPCWithProj(Projectile proj, NPC target, ref NPC.HitModifiers modifiers)
        {
            if (!hasBadgeOfValor || proj.owner != Player.whoAmI) return;
            if (proj.DamageType != DamageClass.Melee) return;
            ApplyMeleeBonuses(ref modifiers);
        }

        private void ApplyMeleeBonuses(ref NPC.HitModifiers modifiers)
        {
            // Red crit mode: 2x damage
            if (redCritSwingsRemaining > 0)
            {
                modifiers.FinalDamage *= 2f;
                redCritSwingsRemaining--;
                return;
            }

            // 20% chance for double damage  
            if (Main.rand.NextFloat() < 0.20f)
            {
                modifiers.FinalDamage *= 2f;
            }
        }

        public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone)
        {
            TryTriggerRedCrit(item.DamageType);
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (proj.owner == Player.whoAmI)
                TryTriggerRedCrit(proj.DamageType);
        }

        private void TryTriggerRedCrit(DamageClass damageType)
        {
            if (!hasBadgeOfValor || damageType != DamageClass.Melee) return;
            if (redCritSwingsRemaining > 0) return; // Don't re-trigger while active
            
            // 5% chance to activate red crit mode
            if (Main.rand.NextFloat() < 0.05f)
            {
                redCritSwingsRemaining = 10;
            }
        }
    }

    #endregion

    #region Hero's Symphony

    /// <summary>
    /// Hero's Symphony - Eroica Tier 2 Theme Accessory (Ultimate).
    /// The triumphant power of Eroica crystallized into wearable form.
    /// All Eroica bonuses maximized, kills trigger "Heroic Surge" (+25% damage for 5s).
    /// </summary>
    public class HerosSymphony : ModItem
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
            var modPlayer = player.GetModPlayer<HerosSymphonyPlayer>();
            modPlayer.hasHerosSymphony = true;

            // +23% melee damage
            player.GetDamage(DamageClass.Melee) += 0.23f;

            // +17% melee speed
            player.GetAttackSpeed(DamageClass.Melee) += 0.17f;

            // +12% melee crit
            player.GetCritChance(DamageClass.Melee) += 12;

            // Heroic Surge damage bonus (when active)
            if (modPlayer.heroicSurgeDuration > 0)
            {
                player.GetDamage(DamageClass.Generic) += 0.25f;
            }
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "DoubleDamage", "25% chance for melee attacks to deal double damage")
            {
                OverrideColor = EroicaPalette.Crimson
            });

            tooltips.Add(new TooltipLine(Mod, "RedCrit", "10% chance for next 10 melee swings to deal 2x critical hits")
            {
                OverrideColor = EroicaPalette.Gold
            });

            tooltips.Add(new TooltipLine(Mod, "MeleeDamage", "+23% melee damage")
            {
                OverrideColor = EroicaPalette.Crimson
            });

            tooltips.Add(new TooltipLine(Mod, "MeleeSpeed", "+17% melee speed")
            {
                OverrideColor = EroicaPalette.Gold
            });

            tooltips.Add(new TooltipLine(Mod, "MeleeCrit", "+12% melee critical strike chance")
            {
                OverrideColor = EroicaPalette.Sakura
            });

            tooltips.Add(new TooltipLine(Mod, "HeroicMoment", "Killing an enemy grants 1 second of invulnerability")
            {
                OverrideColor = EroicaPalette.Sakura
            });

            tooltips.Add(new TooltipLine(Mod, "HeroicSurge", "Kills trigger 'Heroic Surge': +25% damage for 5 seconds")
            {
                OverrideColor = EroicaPalette.Gold
            });

            tooltips.Add(new TooltipLine(Mod, "Flavor", "'The symphony of heroes echoes through eternity'")
            {
                OverrideColor = new Color(200, 50, 50)
            });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<BadgeOfValor>(), 1)
                .AddIngredient(ModContent.ItemType<ResonantCoreOfEroica>(), 25)
                .AddIngredient(ModContent.ItemType<EroicasResonantEnergy>(), 10)
                .AddIngredient(ModContent.ItemType<ValorEssence>(), 10)
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }
    }

    public class HerosSymphonyPlayer : ModPlayer
    {
        public bool hasHerosSymphony;
        public int invulnFramesRemaining;
        public int heroicSurgeDuration;
        public int redCritSwingsRemaining;

        public override void ResetEffects()
        {
            hasHerosSymphony = false;
        }

        public override void PostUpdate()
        {
            if (invulnFramesRemaining > 0)
            {
                Player.immune = true;
                Player.immuneTime = 2;
                invulnFramesRemaining--;
            }

            if (heroicSurgeDuration > 0)
                heroicSurgeDuration--;
        }

        public override void ModifyHitNPCWithItem(Item item, NPC target, ref NPC.HitModifiers modifiers)
        {
            if (!hasHerosSymphony || item.DamageType != DamageClass.Melee) return;
            ApplyMeleeBonuses(ref modifiers);
        }

        public override void ModifyHitNPCWithProj(Projectile proj, NPC target, ref NPC.HitModifiers modifiers)
        {
            if (!hasHerosSymphony || proj.owner != Player.whoAmI || proj.DamageType != DamageClass.Melee) return;
            ApplyMeleeBonuses(ref modifiers);
        }

        private void ApplyMeleeBonuses(ref NPC.HitModifiers modifiers)
        {
            if (redCritSwingsRemaining > 0)
            {
                modifiers.FinalDamage *= 2f;
                redCritSwingsRemaining--;
                return;
            }

            // 25% chance for double damage
            if (Main.rand.NextFloat() < 0.25f)
            {
                modifiers.FinalDamage *= 2f;
            }
        }

        public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone)
        {
            CheckForKillAndRedCrit(target, item.DamageType);
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (proj.owner == Player.whoAmI)
                CheckForKillAndRedCrit(target, proj.DamageType);
        }

        private void CheckForKillAndRedCrit(NPC target, DamageClass damageType)
        {
            if (!hasHerosSymphony) return;

            // Red crit trigger on melee hit
            if (damageType == DamageClass.Melee && redCritSwingsRemaining <= 0 && Main.rand.NextFloat() < 0.10f)
            {
                redCritSwingsRemaining = 10;
            }

            // Kill rewards
            if (target.life <= 0 && !target.friendly && target.lifeMax > 5)
            {
                invulnFramesRemaining = 60;
                heroicSurgeDuration = 300;
            }
        }
    }

    #endregion
}
