using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.FoundationWeapons.LaserFoundation
{
    /// <summary>
    /// LaserFoundation — Foundation weapon demonstrating a convergence beam.
    /// 
    /// Left-click: Fire a channeled convergence beam that tracks the cursor.
    /// Right-click: Cycle beam color through all score theme gradient LUTs.
    /// 
    /// The current theme index is stored on the item and passed to the beam
    /// projectile via Projectile.ai[1]. The beam reads this to pick which
    /// gradient LUT texture to feed the shader.
    /// 
    /// Architecture notes:
    /// - Completely self-contained: own shader, own texture registry, own projectile
    /// - Does NOT depend on any global VFX systems
    /// - 0 mana cost for testing purposes
    /// </summary>
    public class LaserFoundation : ModItem
    {
        // Uses vanilla Last Prism sprite as placeholder
        public override string Texture => "Terraria/Images/Item_" + ItemID.LastPrism;

        /// <summary>
        /// Current beam theme index. Static so the beam projectile can read it
        /// every frame without needing ai[] sync. Updated by right-click cycling.
        /// </summary>
        public static int CurrentThemeIndex = 0;

        public override void SetStaticDefaults()
        {
            // Allow right-click to be used repeatedly
            ItemID.Sets.ItemsThatAllowRepeatedRightClick[Type] = true;
        }

        public override void SetDefaults()
        {
            Item.damage = 80;
            Item.DamageType = DamageClass.Magic;
            Item.mana = 0; // 0 mana for testing
            Item.width = 28;
            Item.height = 30;
            Item.useTime = 10;
            Item.useAnimation = 10;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.knockBack = 2f;
            Item.value = Item.sellPrice(gold: 5);
            Item.rare = ItemRarityID.Red;
            Item.autoReuse = true;
            Item.channel = true; // Channeled weapon — held while mouse is pressed
            Item.noUseGraphic = true; // Don't draw the item sprite during use
            Item.shoot = ModContent.ProjectileType<LaserFoundationBeam>();
            Item.shootSpeed = 1f; // Direction only — beam is instant-hit
        }

        public override bool AltFunctionUse(Player player) => true;

        public override bool CanUseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                // Right-click: cycle theme, don't fire
                Item.channel = false;
                Item.useTime = 15;
                Item.useAnimation = 15;
                Item.shoot = ProjectileID.None;
            }
            else
            {
                // Left-click: fire beam
                Item.channel = true;
                Item.useTime = 10;
                Item.useAnimation = 10;
                Item.shoot = ModContent.ProjectileType<LaserFoundationBeam>();
            }
            return true;
        }

        public override bool? UseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                // Right-click: cycle to next theme
                CurrentThemeIndex = (CurrentThemeIndex + 1) % (int)BeamTheme.COUNT;
                BeamTheme newTheme = (BeamTheme)CurrentThemeIndex;

                // Show theme name as combat text
                string themeName = LFTextures.GetThemeName(newTheme);
                Color[] dustColors = LFTextures.GetDustColorsForTheme(newTheme);
                CombatText.NewText(player.Hitbox, dustColors[0], themeName);

                // Play a small sound for feedback
                SoundEngine.PlaySound(SoundID.MenuTick, player.Center);

                return true;
            }

            return null; // Let normal use proceed
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source,
            Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // Only allow one beam projectile at a time per player
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile p = Main.projectile[i];
                if (p.active && p.owner == player.whoAmI && p.type == type)
                    return false;
            }

            Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
            return false;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            BeamTheme theme = (BeamTheme)CurrentThemeIndex;
            string themeName = LFTextures.GetThemeName(theme);
            Color[] dustColors = LFTextures.GetDustColorsForTheme(theme);

            tooltips.Add(new TooltipLine(Mod, "Effect1",
                "Fires a concentrated convergence beam that tracks the cursor"));
            tooltips.Add(new TooltipLine(Mod, "Effect2",
                "Right-click to cycle beam color theme"));
            tooltips.Add(new TooltipLine(Mod, "Theme",
                $"Current theme: {themeName}")
            {
                OverrideColor = dustColors[0]
            });
            tooltips.Add(new TooltipLine(Mod, "Lore",
                "'The foundation upon which all future devastation is built'")
            {
                OverrideColor = Main.DiscoColor
            });
        }

        public override void AddRecipes()
        {
            // Craftable from dirt for easy testing
            CreateRecipe()
                .AddIngredient(ItemID.DirtBlock, 1)
                .AddTile(TileID.WorkBenches)
                .Register();
        }
    }
}
