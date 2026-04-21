using MagnumOpus.Content.Nachtmusik;
using MagnumOpus.Content.Nachtmusik.Weapons.TwilightSeverance.Projectiles;
using MagnumOpus.Common.Systems.VFX;
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Nachtmusik.Weapons.TwilightSeverance
{
    public class TwilightSeverance : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.width = 70;
            Item.height = 70;
            Item.scale = 0.09f;
            Item.damage = 280;
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
            Item.value = Item.sellPrice(gold: 40);
            Item.shoot = ModContent.ProjectileType<TwilightSeveranceSwing>();
            Item.shootSpeed = 8f;
            Item.rare = ModContent.RarityType<NachtmusikRarity>();
        }

        public override bool AltFunctionUse(Player player) => true;

        public override bool CanShoot(Player player)
        {
            if (player.altFunctionUse == 2)
                return true;
            return player.ownedProjectileCounts[Item.shoot] <= 0;
        }

        public override bool? CanHitNPC(Player player, NPC target) => false;
        public override bool CanHitPvp(Player player, Player target) => false;

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source,
            Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (player.altFunctionUse == 2)
            {
                // Right-click: spawn amplifier rift zone at cursor
                Vector2 riftPos = Main.MouseWorld;
                Projectile.NewProjectile(source, riftPos, Vector2.Zero,
                    ModContent.ProjectileType<TwilightRiftZone>(),
                    (int)(damage * 0.5f), knockback, player.whoAmI);
                return false;
            }

            Projectile.NewProjectile(source, player.MountedCenter,
                (Main.MouseWorld - player.MountedCenter).SafeNormalize(Vector2.UnitX),
                type, damage, knockback, player.whoAmI, 0f, 0);
            return false;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1",
            "Swings spawn homing starlight orbs on hit"));
            tooltips.Add(new TooltipLine(Mod, "Effect2",
            "Right click to place a twilight rift that amplifies passing orbs"));
            tooltips.Add(new TooltipLine(Mod, "Lore",
            "'Between dusk and starlight, every cut severs what was from what will be.'")
            { OverrideColor = new Color(100, 120, 200) });
        }
    }
}
