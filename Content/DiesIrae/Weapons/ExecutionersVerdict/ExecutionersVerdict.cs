using MagnumOpus.Content.DiesIrae;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.DiesIrae.Weapons.ExecutionersVerdict
{
    public class ExecutionersVerdict : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 75;
            Item.height = 75;
            Item.scale = 0.11f;
            Item.damage = 310;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.useTurn = true;
            Item.DamageType = DamageClass.MeleeNoSpeed;
            Item.knockBack = 7.5f;
            Item.autoReuse = true;
            Item.noUseGraphic = true;
            Item.noMelee = true;
            Item.channel = true;
            Item.value = Item.sellPrice(gold: 45);
            Item.shoot = ModContent.ProjectileType<Projectiles.ExecutionersVerdictSwing>();
            Item.shootSpeed = 8f;
            Item.rare = ModContent.RarityType<DiesIraeRarity>();
        }

        public override bool CanShoot(Player player)
        {
            return player.ownedProjectileCounts[Item.shoot] <= 0;
        }

        public override bool? CanHitNPC(Player player, NPC target) => false;
        public override bool CanHitPvp(Player player, Player target) => false;

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source,
            Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Projectile.NewProjectile(source, player.MountedCenter,
                (Main.MouseWorld - player.MountedCenter).SafeNormalize(Vector2.UnitX),
                type, damage, knockback, player.whoAmI, 0f, 0);
            return false;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1",
            "Judicial Crescendo — precise golden strikes that deliver the final sentence"));
            tooltips.Add(new TooltipLine(Mod, "Effect2",
            "Each swing scatters golden judgment sparks with flame accents"));
            tooltips.Add(new TooltipLine(Mod, "Effect3",
            "Hits spawn golden flame bursts at the point of impact"));
            tooltips.Add(new TooltipLine(Mod, "Effect4",
            "Right-click dash attack unleashes a massive golden judgment burst"));
            tooltips.Add(new TooltipLine(Mod, "Lore",
            "'The verdict was written before you were born.'")
            { OverrideColor = DiesIraePalette.LoreText });
        }
    }
}
