using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.FoundationWeapons.AttackAnimationFoundation
{
    /// <summary>
    /// AttackAnimationFoundation — Foundation weapon demonstrating a full-screen
    /// cinematic attack animation.
    ///
    /// Left-click: Initiates a cinematic attack sequence:
    ///   1. Camera pans to the cursor position
    ///   2. Player rapidly slashes through the screen center 5 times from random angles
    ///   3. Each slash adds progressive blur and brightness
    ///   4. A circular noise zone builds on struck enemies
    ///   5. Final slash comes from the top with B&amp;W impact frame + screen shake
    ///   6. Camera smoothly returns to the player
    ///
    /// Architecture:
    /// - Self-contained: own ModPlayer, own projectiles, own texture registry
    /// - Does NOT depend on any global VFX systems
    /// - 0 mana cost, dirt crafting recipe for testing purposes
    /// </summary>
    public class AttackAnimationFoundation : ModItem
    {
        public override string Texture => "Terraria/Images/Item_" + ItemID.Katana;

        public override void SetDefaults()
        {
            Item.damage = 80;
            Item.DamageType = DamageClass.Melee;
            Item.width = 40;
            Item.height = 40;
            Item.useTime = 90; // Long use time — entire animation plays out
            Item.useAnimation = 90;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 6f;
            Item.value = Item.sellPrice(gold: 5);
            Item.rare = ItemRarityID.Red;
            Item.autoReuse = false; // Don't repeat — one cinematic at a time
            Item.UseSound = SoundID.Item71;
            Item.shoot = ModContent.ProjectileType<AttackAnimationProjectile>();
            Item.shootSpeed = 1f; // Speed irrelevant — projectile is orchestrator
            Item.noMelee = true;
            Item.noUseGraphic = true; // Player sprite handled by the projectile
            Item.channel = false;
            Item.mana = 0; // Foundation test weapon — free to use
        }

        public override bool CanUseItem(Player player)
        {
            // Only allow if no existing attack animation is active
            int projType = ModContent.ProjectileType<AttackAnimationProjectile>();
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                if (Main.projectile[i].active && Main.projectile[i].type == projType
                    && Main.projectile[i].owner == player.whoAmI)
                    return false;
            }
            return true;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source,
            Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // The cursor position in world coordinates is the camera target
            Vector2 cursorWorld = Main.MouseWorld;

            // ai[0] and ai[1] = cursor world position
            Projectile.NewProjectile(source, player.Center, Vector2.Zero, type,
                damage, knockback, player.whoAmI,
                ai0: cursorWorld.X, ai1: cursorWorld.Y);

            return false;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1",
                "Launches a cinematic multi-slash attack at the cursor position"));
            tooltips.Add(new TooltipLine(Mod, "Effect2",
                "The player dashes through enemies with 5 rapid slashes"));
            tooltips.Add(new TooltipLine(Mod, "Effect3",
                "Each hit builds a noise zone and screen intensity"));
            tooltips.Add(new TooltipLine(Mod, "Effect4",
                "The final slash creates a black-and-white impact frame"));
            tooltips.Add(new TooltipLine(Mod, "Lore",
                "'A blade so swift it cuts between frames of reality'")
            {
                OverrideColor = new Color(180, 200, 255)
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
