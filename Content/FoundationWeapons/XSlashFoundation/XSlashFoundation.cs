using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.FoundationWeapons.XSlashFoundation
{
    /// <summary>
    /// XSlashFoundation — Foundation weapon demonstrating a blazing X-shaped impact effect.
    /// 
    /// Left-click: Fire a homing projectile that spawns a dynamic X-slash impact on hit.
    /// The X impact uses the X-ShapedImpactCross texture rendered with a custom shader
    /// that applies noise-driven fire distortion, UV scrolling, and gradient LUT coloring,
    /// creating a seemingly blazing, fluid cross-shaped slash mark at the impact point.
    /// 
    /// Right-click: Cycle through 6 score-themed color styles.
    /// 
    /// Architecture:
    /// - Self-contained: own shader, own texture registry, own projectiles
    /// - Does NOT depend on any global VFX systems
    /// - 0 mana cost, dirt crafting recipe for testing purposes
    /// </summary>
    public class XSlashFoundation : ModItem
    {
        public override string Texture => "Terraria/Images/Item_" + ItemID.TerraBlade;

        /// <summary>
        /// Current X-slash style index. Static so projectiles can read it.
        /// </summary>
        public static int CurrentStyleIndex = 0;

        public override void SetStaticDefaults()
        {
            ItemID.Sets.ItemsThatAllowRepeatedRightClick[Type] = true;
        }

        public override void SetDefaults()
        {
            Item.damage = 65;
            Item.DamageType = DamageClass.Melee;
            Item.width = 40;
            Item.height = 40;
            Item.useTime = 22;
            Item.useAnimation = 22;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 5f;
            Item.value = Item.sellPrice(gold: 5);
            Item.rare = ItemRarityID.Red;
            Item.autoReuse = true;
            Item.UseSound = SoundID.Item71;
            Item.shoot = ModContent.ProjectileType<XSlashProjectile>();
            Item.shootSpeed = 16f;
            Item.noMelee = true;
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
            }
            else
            {
                Item.useStyle = ItemUseStyleID.Swing;
                Item.useTime = 22;
                Item.useAnimation = 22;
                Item.shoot = ModContent.ProjectileType<XSlashProjectile>();
            }
            return true;
        }

        public override bool? UseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                CurrentStyleIndex = (CurrentStyleIndex + 1) % (int)XSlashStyle.COUNT;
                XSlashStyle newStyle = (XSlashStyle)CurrentStyleIndex;

                string styleName = XSFTextures.GetStyleName(newStyle);
                Color[] styleColors = XSFTextures.GetStyleColors(newStyle);
                CombatText.NewText(player.Hitbox, styleColors[0], styleName);
                SoundEngine.PlaySound(SoundID.MenuTick, player.Center);

                return true;
            }
            return null;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source,
            Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (player.altFunctionUse == 2)
                return false;

            Projectile.NewProjectile(source, position, velocity, type,
                damage, knockback, player.whoAmI, ai0: CurrentStyleIndex);

            return false;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            XSlashStyle style = (XSlashStyle)CurrentStyleIndex;
            string styleName = XSFTextures.GetStyleName(style);
            Color[] styleColors = XSFTextures.GetStyleColors(style);

            tooltips.Add(new TooltipLine(Mod, "Effect1",
                "Fires a homing projectile that creates a blazing X-slash on impact"));
            tooltips.Add(new TooltipLine(Mod, "Effect2",
                "The X impact burns with noise-driven fire and fluid energy"));
            tooltips.Add(new TooltipLine(Mod, "Effect3",
                "Right-click to cycle color theme"));
            tooltips.Add(new TooltipLine(Mod, "Style",
                $"Current style: {styleName}")
            {
                OverrideColor = styleColors[0]
            });
            tooltips.Add(new TooltipLine(Mod, "Lore",
                "'Two cuts — one moment — the mark of finality'")
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
