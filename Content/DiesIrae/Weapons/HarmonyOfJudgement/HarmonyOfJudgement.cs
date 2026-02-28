using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.DiesIrae.Weapons.HarmonyOfJudgement.Particles;
using MagnumOpus.Content.DiesIrae.Weapons.HarmonyOfJudgement.Utilities;

namespace MagnumOpus.Content.DiesIrae.Weapons.HarmonyOfJudgement
{
    /// <summary>
    /// Harmony of Judgement — Summons a floating judgment sigil that fires piercing rays of judgment.
    ///
    /// Stats: 1300 damage, 20 mana, 30 useTime, 3.5 KB, Summon, 1 slot.
    /// Theme: Dies Irae — divine harmony turned to righteous fury.
    /// </summary>
    public class HarmonyOfJudgement : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 40;
            Item.height = 40;
            Item.damage = 1300;
            Item.DamageType = DamageClass.Summon;
            Item.mana = 20;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 3.5f;
            Item.value = Item.sellPrice(platinum: 1, gold: 75);
            Item.rare = ModContent.RarityType<global::MagnumOpus.Common.DiesIraeRarity>();
            Item.UseSound = SoundID.Item44;
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<Projectiles.JudgmentSigilMinion>();
            Item.shootSpeed = 0f;
            Item.buffType = ModContent.BuffType<Buffs.HarmonyOfJudgementBuff>();
            Item.buffTime = 18000;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            player.AddBuff(Item.buffType, 18000);
            player.SpawnMinionOnCursor(source, player.whoAmI, type, damage, knockback);

            // Summoning VFX
            HarmonyParticleHandler.Spawn(new SigilGlowParticle(Main.MouseWorld, HarmonyUtils.SigilGold, 1f, 15));
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 4f);
                HarmonyParticleHandler.Spawn(new HarmonyEmberParticle(Main.MouseWorld, vel,
                    HarmonyUtils.GetHarmonyColor(Main.rand.NextFloat()), 0.1f, 15));
            }

            return false;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Summons a judgment sigil that fires piercing rays of judgment fire"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Rays pierce multiple enemies, burning all they touch"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'In harmony, heaven and hell converge to deliver judgment'")
            {
                OverrideColor = new Color(200, 50, 30)
            });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<Content.DiesIrae.ResonanceEnergies.ResonantCoreOfDiesIrae>(), 10)
                .AddIngredient(ModContent.ItemType<Content.DiesIrae.ResonanceEnergies.DiesIraeResonantEnergy>(), 28)
                .AddIngredient(ModContent.ItemType<Content.DiesIrae.HarmonicCores.HarmonicCoreOfDiesIrae>(), 2)
                .AddIngredient(ItemID.LunarBar, 18)
                .AddTile(ModContent.TileType<Content.Fate.CraftingStations.FatesCosmicAnvilTile>())
                .Register();
        }
    }
}
