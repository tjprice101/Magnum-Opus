using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.FoundationWeapons.ThinSlashFoundation
{
    /// <summary>
    /// ThinSlashFoundation — Foundation weapon demonstrating razor-thin slash mark VFX.
    /// 
    /// Left-click: Fire a homing projectile that spawns a thin slash mark on hit.
    /// The slash is a crisp, bright, very thin diagonal line that cuts through
    /// the impact point — like a clean sword cut across the screen.
    /// 
    /// Right-click: Cycle through 5 slash color styles:
    ///   Pure White, Ice Cyan, Golden Edge, Violet Cut, Crimson Slice
    /// 
    /// Architecture:
    /// - Self-contained: own shader, own texture registry, own projectiles
    /// - Does NOT depend on any global VFX systems
    /// - 0 mana cost, dirt crafting recipe for testing purposes
    /// </summary>
    public class ThinSlashFoundation : ModItem
    {
        public override string Texture => "Terraria/Images/Item_" + ItemID.EnchantedSword;

        /// <summary>
        /// Current slash style index. Static so projectiles can read it.
        /// </summary>
        public static int CurrentStyleIndex = 0;

        public override void SetStaticDefaults()
        {
            ItemID.Sets.ItemsThatAllowRepeatedRightClick[Type] = true;
        }

        public override void SetDefaults()
        {
            Item.damage = 55;
            Item.DamageType = DamageClass.Magic;
            Item.width = 40;
            Item.height = 40;
            Item.useTime = 18;
            Item.useAnimation = 18;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 4f;
            Item.value = Item.sellPrice(gold: 5);
            Item.rare = ItemRarityID.Red;
            Item.autoReuse = true;
            Item.UseSound = SoundID.Item72;
            Item.shoot = ModContent.ProjectileType<ThinSlashProjectile>();
            Item.shootSpeed = 16f;
            Item.noMelee = true;
            Item.mana = 0; // Foundation test weapon — free to cast
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
                Item.useStyle = ItemUseStyleID.Shoot;
                Item.useTime = 18;
                Item.useAnimation = 18;
                Item.shoot = ModContent.ProjectileType<ThinSlashProjectile>();
            }
            return true;
        }

        public override bool? UseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                CurrentStyleIndex = (CurrentStyleIndex + 1) % (int)SlashStyle.COUNT;
                SlashStyle style = (SlashStyle)CurrentStyleIndex;

                string styleName = TSFTextures.GetStyleName(style);
                Color[] styleColors = TSFTextures.GetStyleColors(style);
                CombatText.NewText(player.Hitbox, styleColors[2], styleName);
                SoundEngine.PlaySound(SoundID.Item4, player.Center);

                return true;
            }
            return null;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source,
            Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (player.altFunctionUse == 2)
                return false;

            // ai[0] = current slash style index
            Projectile.NewProjectile(source, position, velocity, type,
                damage, knockback, player.whoAmI, ai0: CurrentStyleIndex);

            return false;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            SlashStyle style = (SlashStyle)CurrentStyleIndex;
            string styleName = TSFTextures.GetStyleName(style);
            Color[] styleColors = TSFTextures.GetStyleColors(style);

            tooltips.Add(new TooltipLine(Mod, "Effect1",
                "Fires a homing projectile that leaves a razor-thin slash mark on impact"));
            tooltips.Add(new TooltipLine(Mod, "Effect2",
                "Right-click to cycle slash color style"));
            tooltips.Add(new TooltipLine(Mod, "Mode",
                $"Current style: {styleName}")
            {
                OverrideColor = styleColors[2]
            });
            tooltips.Add(new TooltipLine(Mod, "Lore",
                "'A single line — thin as silence, bright as the moment before the note falls'")
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
