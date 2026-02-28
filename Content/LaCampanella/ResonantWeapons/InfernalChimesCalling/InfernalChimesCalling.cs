using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;
using MagnumOpus.Content.LaCampanella.ResonantWeapons.InfernalChimesCalling.Projectiles;

namespace MagnumOpus.Content.LaCampanella.ResonantWeapons.InfernalChimesCalling
{
    public class InfernalChimesCallingItem : ModItem
    {
        public override string Texture => "MagnumOpus/Content/LaCampanella/ResonantWeapons/InfernalChimesCalling/InfernalChimesCalling";
        public override string Name => "InfernalChimesCalling";

        public override void SetStaticDefaults()
        {
            ItemID.Sets.GamepadWholeScreenUseRange[Item.type] = true;
            ItemID.Sets.LockOnIgnoresCollision[Item.type] = true;
        }

        public override void SetDefaults()
        {
            Item.damage = 145;
            Item.DamageType = DamageClass.Summon;
            Item.mana = 20;
            Item.width = 40;
            Item.height = 40;
            Item.useTime = 36;
            Item.useAnimation = 36;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 3f;
            Item.value = Item.sellPrice(gold: 50);
            Item.rare = ModContent.RarityType<LaCampanellaRainbowRarity>();
            Item.UseSound = SoundID.Item44;
            Item.shoot = ModContent.ProjectileType<CampanellaChoirMinion>();
            Item.buffType = ModContent.BuffType<CampanellaChoirBuff>();
            Item.noMelee = true;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            player.AddBuff(Item.buffType, 18000);
            position = Main.MouseWorld;
            Projectile.NewProjectile(source, position, Vector2.Zero, type, damage, knockback, player.whoAmI);
            return false;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Summons an infernal bell choir entity to fight for you"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "The choir attacks with fiery slams, flame breath, and resonant bell rings"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Every 5th hit triggers a devastating musical shockwave"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The bells ring in unholy chorus, each toll a hymn of annihilation'")
            {
                OverrideColor = new Color(255, 140, 40)
            });
        }
    }
}
