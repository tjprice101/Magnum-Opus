using MagnumOpus.Content.DiesIrae;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.DiesIrae.Weapons.StaffOfFinalJudgment
{
    public class StaffOfFinalJudgment : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 48;
            Item.height = 48;
            Item.damage = 1950;
            Item.DamageType = DamageClass.Magic;
            Item.mana = 22;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 5f;
            Item.value = Item.sellPrice(platinum: 2);
            Item.rare = ModContent.RarityType<DiesIraeRarity>();
            Item.UseSound = SoundID.Item117;
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<Projectiles.FloatingIgnitionProjectile>();
            Item.shootSpeed = 4f;
            Item.crit = 18;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Projectile.NewProjectile(source, player.MountedCenter, velocity, type, damage, knockback, player.whoAmI);
            return false;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Fires floating fire mines that auto-arm after a short delay"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Armed mines detonate when enemies approach, releasing 3 homing child orbs"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Maximum of 5 active mines at once"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "3+ mines detonating within 1 second triggers Judgment Storm, firing 5 child orbs each"));
            tooltips.Add(new TooltipLine(Mod, "Effect5", "Right click fires a direct homing shot"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Judgment does not chase. Judgment waits.'")
            {
                OverrideColor = DiesIraePalette.LoreText
            });
        }
    }
}
