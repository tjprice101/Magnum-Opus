using MagnumOpus.Content.DiesIrae;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.DiesIrae.Weapons.HarmonyOfJudgment
{
    public class HarmonyOfJudgment : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 40;
            Item.height = 40;
            Item.damage = 1650;
            Item.DamageType = DamageClass.Summon;
            Item.mana = 20;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 3.5f;
            Item.value = Item.sellPrice(platinum: 1, gold: 75);
            Item.rare = ModContent.RarityType<DiesIraeRarity>();
            Item.UseSound = SoundID.Item44;
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<Projectiles.JudgmentSigilMinion>();
            Item.shootSpeed = 0f;
            Item.buffType = ModContent.BuffType<Buffs.HarmonyOfJudgmentBuff>();
            Item.buffTime = 18000;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            player.AddBuff(Item.buffType, 2);
            Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
            return false;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Summons a judgment sigil that autonomously processes targets"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Sigils cycle through Scan, Judge, and Execute phases"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Multiple sigils targeting the same enemy trigger Collective Judgment for double damage"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Five rapid executions within 10 seconds enters Harmonized Verdict state"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'When many voices speak as one, there is no appeal'")
            {
                OverrideColor = DiesIraePalette.LoreText
            });
        }
    }
}
