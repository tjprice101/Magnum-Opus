using MagnumOpus.Content.ClairDeLune;
using MagnumOpus.Content.ClairDeLune.Weapons.GearDrivenArbiter.Projectiles;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.ClairDeLune.Weapons.GearDrivenArbiter
{
    public class GearDrivenArbiterItem : ModItem
    {
        public override string Texture => "MagnumOpus/Content/ClairDeLune/Weapons/GearDrivenArbiter/GearDrivenArbiter";

        public override void SetDefaults()
        {
            Item.width = 36;
            Item.height = 36;
            Item.damage = 3000;
            Item.DamageType = DamageClass.Summon;
            Item.mana = 16;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 3.5f;
            Item.value = Item.sellPrice(platinum: 5);
            Item.rare = ModContent.RarityType<ClairDeLuneRarity>();
            Item.UseSound = SoundID.Item44;
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<ArbiterMinionProjectile>();
            Item.buffType = ModContent.BuffType<GearDrivenArbiterBuff>();
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source,
            Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            player.AddBuff(Item.buffType, 2);
            Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
            return false;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Summon", "Summons a Gear-Driven Arbiter to fight for you"));
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Hitting the same enemy stacks Verdicts up to 8"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "At 8 stacks: orbs become 2x larger and deal 5x damage"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'A court of gears that judges in silence.'")
            {
                OverrideColor = ClairDeLunePalette.LoreText
            });
        }
    }

    public class GearDrivenArbiterBuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.buffNoSave[Type] = true;
            Main.buffNoTimeDisplay[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            if (player.ownedProjectileCounts[ModContent.ProjectileType<ArbiterMinionProjectile>()] > 0)
            {
                player.buffTime[buffIndex] = 18000;
            }
            else
            {
                player.DelBuff(buffIndex);
                buffIndex--;
            }
        }
    }
}
