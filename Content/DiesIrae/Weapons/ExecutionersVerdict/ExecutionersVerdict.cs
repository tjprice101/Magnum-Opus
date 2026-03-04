using MagnumOpus.Common;
using MagnumOpus.Common.BaseClasses;
using MagnumOpus.Content.DiesIrae.Weapons.ExecutionersVerdict.Projectiles;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.DiesIrae.HarmonicCores;
using MagnumOpus.Content.DiesIrae.ResonanceEnergies;
using MagnumOpus.Content.Fate.CraftingStations;

namespace MagnumOpus.Content.DiesIrae.Weapons.ExecutionersVerdict
{
    /// <summary>
    /// EXECUTIONER'S VERDICT — The Judicial Greatsword.
    /// Methodical heavy strikes that apply Judgment Marks.
    /// 3-phase combo: Arraignment → Cross-Examination → The Verdict.
    /// At 3 marks: Verdict Execution trigger.
    /// 
    /// Extends MeleeSwingItemBase for proper held-projectile swing behavior
    /// with combo tracking and post-swing stasis gating.
    /// </summary>
    public class ExecutionersVerdict : MeleeSwingItemBase
    {
        protected override int SwingProjectileType => ModContent.ProjectileType<ExecutionersVerdictSwing>();
        protected override int ComboStepCount => 3;
        protected override int ComboResetDelay => 50;

        protected override Color GetLoreColor() => new Color(200, 50, 30);

        protected override void SetWeaponDefaults()
        {
            Item.width = 80;
            Item.height = 80;
            Item.damage = 4200;
            Item.useTime = 32;
            Item.useAnimation = 32;
            Item.knockBack = 12f;
            Item.value = Item.sellPrice(platinum: 2, gold: 50);
            Item.rare = ModContent.RarityType<DiesIraeRarity>();
            Item.crit = 25;
            Item.scale = 1.8f;
        }

        protected override void AddWeaponTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "3-phase judgment combo applies Judgment Marks"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Marked enemies take increased damage per mark"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "At 3 marks: triggers Verdict Execution"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Executes non-boss enemies below 15% health instantly"));
            tooltips.Add(new TooltipLine(Mod, "Effect5", "Deals 50% more damage to enemies below 30% health"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The verdict was written before you were born'")
            {
                OverrideColor = new Color(200, 50, 30)
            });
        }

        protected override void OnShoot(Player player, int projectileIndex)
        {
            // Spawn 3 verdict bolts per swing that track enemies
            int boltCount = 3;
            for (int i = 0; i < boltCount; i++)
            {
                float spread = MathHelper.ToRadians(15f) * (i - 1);
                Vector2 aim = player.MountedCenter.DirectionTo(Main.MouseWorld).RotatedBy(spread);
                Projectile.NewProjectile(player.GetSource_ItemUse(player.HeldItem),
                    player.MountedCenter, aim * 12f,
                    ModContent.ProjectileType<VerdictBolt>(),
                    (int)(Item.damage * 0.35f), Item.knockBack * 0.3f,
                    player.whoAmI);
            }
        }

        public override void AddRecipes()
        {
            CreateRecipe()
            .AddIngredient(ModContent.ItemType<ResonantCoreOfDiesIrae>(), 25)
            .AddIngredient(ModContent.ItemType<DiesIraeResonantEnergy>(), 20)
            .AddIngredient(ModContent.ItemType<HarmonicCoreOfDiesIrae>(), 3)
            .AddIngredient(ItemID.LunarBar, 20)
            .AddTile(ModContent.TileType<FatesCosmicAnvilTile>())
            .Register();
        }
    }
}