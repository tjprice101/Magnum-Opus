using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;
using MagnumOpus.Common.BaseClasses;
using MagnumOpus.Content.DiesIrae.Weapons.WrathsCleaver.Buffs;
using MagnumOpus.Content.DiesIrae.Weapons.WrathsCleaver.Projectiles;
using MagnumOpus.Content.DiesIrae.HarmonicCores;
using MagnumOpus.Content.DiesIrae.ResonanceEnergies;
using MagnumOpus.Content.Fate.CraftingStations;

namespace MagnumOpus.Content.DiesIrae.Weapons.WrathsCleaver
{
    /// <summary>
    /// WRATH'S CLEAVER — Dies Irae Melee Sword (Item).
    /// Held-projectile swing via MeleeSwingItemBase → WrathsCleaverSwing.
    ///
    /// 3-Phase Wrath Combo:
    ///   Phase 0: Accusation — Heavy horizontal cleave, spawns 2 crystallized flames
    ///   Phase 1: Conviction — Overhead slam, spawns 4 crystallized flames + smoke eruption
    ///   Phase 2: Execution — Spinning 270° cleave, spawns 6 flames in ring + shockwave
    ///
    /// Wrath Meter: Builds on hit (max 100). Scales VFX intensity.
    /// At 100 Wrath → next swing = Wrath Unleashed (massive cleave, 8 flame carpet, eruption).
    /// </summary>
    public class WrathsCleaver : MeleeSwingItemBase
    {
        public override string Texture => "MagnumOpus/Content/DiesIrae/Weapons/WrathsCleaver/WrathsCleaver";

        #region Theme Colors

        private static readonly Color DiesIraeLore = new Color(200, 50, 30);

        #endregion

        #region MeleeSwingItemBase Overrides

        protected override int SwingProjectileType
            => ModContent.ProjectileType<WrathsCleaverSwing>();

        protected override int ComboStepCount => 3;

        protected override Color GetLoreColor() => DiesIraeLore;

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        protected override void SetWeaponDefaults()
        {
            Item.damage = 2800;
            Item.knockBack = 9f;
            Item.useTime = 16;
            Item.useAnimation = 16;
            Item.value = Item.sellPrice(platinum: 2, gold: 50);
            Item.rare = ModContent.RarityType<DiesIraeRarity>();
            Item.crit = 20;
            Item.scale = 1.6f;
        }

        protected override void AddWeaponTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "3-phase wrath combo: Accusation, Conviction, Execution"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Swings spawn crystallized flame projectiles that leave burning ground"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Hits build Wrath — at maximum, triggers Infernal Eruption"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Eruption marks all nearby enemies, increasing damage taken by 25%"));
            tooltips.Add(new TooltipLine(Mod, "Lore",
                "'The first blow of wrath is always the loudest.'")
            {
                OverrideColor = DiesIraeLore
            });
        }

        #endregion

        #region OnShoot — Crystallized Flame Spawning Per Phase

        protected override void OnShoot(Player player, int projectileIndex)
        {
            int justUsedStep = (CurrentComboStep + ComboStepCount - 1) % ComboStepCount;
            Vector2 toCursor = (Main.MouseWorld - player.Center).SafeNormalize(Vector2.UnitX);
            int flameType = ModContent.ProjectileType<WrathCrystallizedFlame>();

            switch (justUsedStep)
            {
                case 0: // Accusation — 2 arcing flames
                    for (int i = -1; i <= 1; i += 2)
                    {
                        Vector2 flameVel = toCursor.RotatedBy(i * 0.3f) * 10f + new Vector2(0, -3f);
                        Projectile.NewProjectile(player.GetSource_ItemUse(Item),
                            player.Center, flameVel, flameType,
                            (int)(Item.damage * 0.4f), 4f, player.whoAmI);
                    }
                    break;

                case 1: // Conviction — 4 spread flames + screen shake
                    for (int i = 0; i < 4; i++)
                    {
                        float angle = toCursor.ToRotation() + MathHelper.ToRadians(-30 + i * 20);
                        Vector2 flameVel = angle.ToRotationVector2() * 11f + new Vector2(0, -4f);
                        Projectile.NewProjectile(player.GetSource_ItemUse(Item),
                            player.Center, flameVel, flameType,
                            (int)(Item.damage * 0.35f), 3f, player.whoAmI);
                    }
                    SoundEngine.PlaySound(SoundID.Item74 with { Pitch = -0.4f, Volume = 0.7f }, player.Center);
                    break;

                case 2: // Execution — 6 flames in ring
                    for (int i = 0; i < 6; i++)
                    {
                        float angle = MathHelper.TwoPi / 6f * i;
                        Vector2 flameVel = angle.ToRotationVector2() * 8f;
                        Projectile.NewProjectile(player.GetSource_ItemUse(Item),
                            player.Center, flameVel, flameType,
                            (int)(Item.damage * 0.5f), 5f, player.whoAmI);
                    }
                    SoundEngine.PlaySound(SoundID.Item45 with { Pitch = -0.5f, Volume = 0.8f }, player.Center);
                    break;
            }
        }

        #endregion

        #region Recipes

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<ResonantCoreOfDiesIrae>(), 20)
                .AddIngredient(ModContent.ItemType<DiesIraeResonantEnergy>(), 15)
                .AddIngredient(ModContent.ItemType<HarmonicCoreOfDiesIrae>(), 2)
                .AddIngredient(ItemID.LunarBar, 15)
                .AddTile(ModContent.TileType<FatesCosmicAnvilTile>())
                .Register();
        }

        #endregion
    }
}