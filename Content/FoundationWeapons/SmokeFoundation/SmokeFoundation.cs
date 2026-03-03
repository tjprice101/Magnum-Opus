using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.FoundationWeapons.SmokeFoundation
{
    /// <summary>
    /// SmokeFoundation — Foundation weapon demonstrating an expanding smoke ring
    /// explosion inspired by Calamity's Supernova heavy smoke particles.
    ///
    /// Left-click: Swing fires a carrier projectile. On collision with an enemy
    ///             or tile, it detonates into a ring of heavy smoke puff particles
    ///             using the 3×6 SmokeRender grid spritesheet. Each puff expands,
    ///             color-shifts from hot core to dark soot, and fades with a
    ///             non-linear (cubic) ease — lingering visibly before rapid dissipation.
    ///
    /// Right-click: Cycle through 6 smoke cloud color styles.
    ///
    /// Architecture notes:
    /// - Completely self-contained: own texture registry, own projectiles
    /// - Does NOT depend on any global VFX systems or shaders
    /// - 0 mana cost, dirt crafting recipe for testing purposes
    /// </summary>
    public class SmokeFoundation : ModItem
    {
        public override string Texture => "Terraria/Images/Item_" + ItemID.Katana;

        /// <summary>
        /// Current smoke style index. Static so projectiles can read it.
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
            Item.useTime = 28;
            Item.useAnimation = 28;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 5f;
            Item.value = Item.sellPrice(gold: 5);
            Item.rare = ItemRarityID.Red;
            Item.autoReuse = true;
            Item.UseSound = SoundID.Item1;
            Item.shoot = ModContent.ProjectileType<SmokeCarrierProjectile>();
            Item.shootSpeed = 14f;
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
                Item.UseSound = SoundID.MenuTick;
            }
            else
            {
                Item.useStyle = ItemUseStyleID.Swing;
                Item.useTime = 28;
                Item.useAnimation = 28;
                Item.shoot = ModContent.ProjectileType<SmokeCarrierProjectile>();
            }
            return true;
        }

        public override bool? UseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                CurrentStyleIndex = (CurrentStyleIndex + 1) % (int)SmokeCloudStyle.COUNT;
                SmokeCloudStyle newStyle = (SmokeCloudStyle)CurrentStyleIndex;

                string styleName = SKFTextures.GetStyleName(newStyle);
                Color[] styleColors = SKFTextures.GetStyleColors(newStyle);
                CombatText.NewText(player.Hitbox, styleColors[1], styleName);
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

            Projectile.NewProjectile(source, position, velocity, type,
                damage, knockback, player.whoAmI, ai0: CurrentStyleIndex);

            return false;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            SmokeCloudStyle style = (SmokeCloudStyle)CurrentStyleIndex;
            string styleName = SKFTextures.GetStyleName(style);
            Color[] styleColors = SKFTextures.GetStyleColors(style);

            tooltips.Add(new TooltipLine(Mod, "Effect1",
                "Fires a projectile that detonates into a ring of heavy smoke clouds"));
            tooltips.Add(new TooltipLine(Mod, "Effect2",
                "Smoke expands and color-shifts before dissipating"));
            tooltips.Add(new TooltipLine(Mod, "Effect3",
                "Right-click to cycle smoke cloud color styles"));
            tooltips.Add(new TooltipLine(Mod, "Style",
                $"Current style: {styleName}")
            {
                OverrideColor = styleColors[1]
            });
            tooltips.Add(new TooltipLine(Mod, "Lore",
                "'Where the blade passes, smoke remembers the melody it once carried'")
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
