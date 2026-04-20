using MagnumOpus.Content.DiesIrae;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.DiesIrae.Weapons.ArbitersSentence
{
    public class ArbitersSentence : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 64;
            Item.height = 24;
            Item.damage = 400;
            Item.DamageType = DamageClass.Ranged;
            Item.useTime = 3;
            Item.useAnimation = 9;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 1f;
            Item.value = Item.sellPrice(platinum: 1, gold: 50);
            Item.rare = ModContent.RarityType<DiesIraeRarity>();
            Item.UseSound = SoundID.Item34 with { Pitch = 0.15f, Volume = 0.6f };
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<Projectiles.JudgmentFlameProjectile>();
            Item.shootSpeed = 11f;
            Item.useAmmo = AmmoID.Gel;
            Item.crit = 15;
        }

        public override Vector2? HoldoutOffset() => new Vector2(-10f, 0f);

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Projectile.NewProjectile(source, player.MountedCenter, velocity, type, damage, knockback, player.whoAmI);
            return false;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Precision flamethrower that applies stacking Judgment Flame (15 damage/s per stack)"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "At 5 stacks: Sentence Cage roots enemy, next hit deals double damage"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "5 consecutive hits on same target activates Arbiter's Focus — 3 precision shots with +40% damage"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Killing sentenced enemies transfers flames to nearby foes"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The arbiter does not miss. The arbiter does not forgive.'")
            {
                OverrideColor = DiesIraePalette.LoreText
            });
        }
    }
}
