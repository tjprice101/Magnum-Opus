using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.FoundationWeapons.SwordSmearFoundation
{
    /// <summary>
    /// SwordSmearFoundation — Foundation weapon demonstrating 4 different sword arc
    /// smear overlay textures from Assets/VFX Asset Library/SlashArcs/.
    ///
    /// Left-click: Swing the sword. The arc smear texture is rendered as an additive
    ///             overlay behind the blade during the swing animation.
    /// Right-click: Cycle through the 4 smear styles.
    ///
    /// Each style renders a different arc texture with its own color palette,
    /// showing the visual range of the SlashArcs asset library.
    /// </summary>
    public class SwordSmearFoundation : ModItem
    {
        public override string Texture => "Terraria/Images/Item_" + ItemID.Katana;

        public static int CurrentStyleIndex = 0;

        public override void SetStaticDefaults()
        {
            ItemID.Sets.ItemsThatAllowRepeatedRightClick[Type] = true;
        }

        public override void SetDefaults()
        {
            Item.damage = 70;
            Item.DamageType = DamageClass.Melee;
            Item.width = 40;
            Item.height = 40;
            Item.useTime = 28;
            Item.useAnimation = 28;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 5f;
            Item.value = Item.sellPrice(gold: 5);
            Item.rare = ItemRarityID.Red;
            Item.autoReuse = true;
            Item.UseSound = SoundID.Item1;
            Item.shoot = ModContent.ProjectileType<SmearSwingProjectile>();
            Item.shootSpeed = 1f;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.mana = 0;
        }

        public override bool AltFunctionUse(Player player) => true;

        public override bool CanUseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                Item.useStyle = ItemUseStyleID.HoldUp;
                Item.useTime = 15;
                Item.useAnimation = 15;
                Item.shoot = ProjectileID.None;
                Item.UseSound = SoundID.MenuTick;
            }
            else
            {
                Item.useStyle = ItemUseStyleID.Shoot;
                Item.useTime = 28;
                Item.useAnimation = 28;
                Item.shoot = ModContent.ProjectileType<SmearSwingProjectile>();
                Item.UseSound = SoundID.Item1;

                // Prevent overlapping swing projectiles
                int projType = ModContent.ProjectileType<SmearSwingProjectile>();
                for (int i = 0; i < Main.maxProjectiles; i++)
                {
                    if (Main.projectile[i].active && Main.projectile[i].type == projType
                        && Main.projectile[i].owner == player.whoAmI)
                        return false;
                }
            }
            return true;
        }

        public override bool? UseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                CurrentStyleIndex = (CurrentStyleIndex + 1) % (int)SmearStyle.COUNT;
                SmearStyle newStyle = (SmearStyle)CurrentStyleIndex;

                string styleName = SMFTextures.GetStyleName(newStyle);
                Color[] styleColors = SMFTextures.GetStyleColors(newStyle);
                CombatText.NewText(player.Hitbox, styleColors[0], styleName);

                return true;
            }
            return null;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source,
            Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (player.altFunctionUse == 2)
                return false;

            Vector2 aimDir = (Main.MouseWorld - player.MountedCenter).SafeNormalize(Vector2.UnitX);
            Projectile.NewProjectile(source, player.MountedCenter, aimDir, type,
                damage, knockback, player.whoAmI, ai0: CurrentStyleIndex);

            return false;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            SmearStyle style = (SmearStyle)CurrentStyleIndex;
            string styleName = SMFTextures.GetStyleName(style);
            Color[] styleColors = SMFTextures.GetStyleColors(style);

            tooltips.Add(new TooltipLine(Mod, "Effect1",
                "Swings the blade with a visible arc smear overlay"));
            tooltips.Add(new TooltipLine(Mod, "Effect2",
                "Right-click to cycle through smear visual styles"));
            tooltips.Add(new TooltipLine(Mod, "Style",
                $"Current style: {styleName}")
            {
                OverrideColor = styleColors[0]
            });
            tooltips.Add(new TooltipLine(Mod, "Lore",
                "'The arc of a blade is its signature — each slash writes a different verse'")
            {
                OverrideColor = Main.DiscoColor
            });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.DirtBlock, 1)
                .AddTile(TileID.WorkBenches)
                .Register();
        }
    }
}
