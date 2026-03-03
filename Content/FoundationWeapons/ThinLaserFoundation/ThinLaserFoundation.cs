using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.FoundationWeapons.LaserFoundation;

namespace MagnumOpus.Content.FoundationWeapons.ThinLaserFoundation
{
    /// <summary>
    /// ThinLaserFoundation — Foundation melee weapon that fires a thin ricocheting beam.
    /// 
    /// Left-click: Swing the weapon and fire a thin beam toward the cursor.
    ///             The beam ricochets off solid tiles up to 3 times.
    /// Right-click: Cycle beam color through all score theme gradient LUTs.
    /// 
    /// Architecture notes:
    /// - Self-contained: own shader (ThinBeamShader), own projectile (ThinLaserBeam)
    /// - Shares BeamTheme enum and VFX Asset Library textures via LFTextures
    /// - Melee damage type, no mana cost
    /// - Own static CurrentThemeIndex (independent from LaserFoundation's)
    /// </summary>
    public class ThinLaserFoundation : ModItem
    {
        // Uses vanilla Terra Blade sprite as placeholder
        public override string Texture => "Terraria/Images/Item_" + ItemID.TerraBlade;

        /// <summary>
        /// Current beam theme index. Static so the beam projectile can read it
        /// live every frame. Independent from LaserFoundation's theme.
        /// </summary>
        public static int CurrentThemeIndex = 0;

        public override void SetStaticDefaults()
        {
            ItemID.Sets.ItemsThatAllowRepeatedRightClick[Type] = true;
        }

        public override void SetDefaults()
        {
            Item.damage = 60;
            Item.DamageType = DamageClass.Melee;
            Item.width = 40;
            Item.height = 40;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.noMelee = true; // No swing hitbox — beam is the damage source
            Item.knockBack = 4f;
            Item.value = Item.sellPrice(gold: 5);
            Item.rare = ItemRarityID.Red;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<ThinLaserBeam>();
            Item.shootSpeed = 1f; // Direction only — beam is instant
        }

        public override bool AltFunctionUse(Player player) => true;

        public override bool CanUseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                // Right-click: cycle theme, don't fire beam
                Item.useTime = 15;
                Item.useAnimation = 15;
                Item.shoot = ProjectileID.None;
            }
            else
            {
                // Left-click: fire ricocheting beam
                Item.useTime = 20;
                Item.useAnimation = 20;
                Item.shoot = ModContent.ProjectileType<ThinLaserBeam>();
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

                string themeName = LFTextures.GetThemeName(newTheme);
                Color[] dustColors = LFTextures.GetDustColorsForTheme(newTheme);
                CombatText.NewText(player.Hitbox, dustColors[0], themeName);
                SoundEngine.PlaySound(SoundID.MenuTick, player.Center);

                return true;
            }
            return null;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source,
            Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // Fire beam toward cursor
            Vector2 direction = (Main.MouseWorld - player.MountedCenter).SafeNormalize(Vector2.UnitX);
            Projectile.NewProjectile(source, player.MountedCenter, direction, type, damage, knockback, player.whoAmI);
            return false;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            BeamTheme theme = (BeamTheme)CurrentThemeIndex;
            string themeName = LFTextures.GetThemeName(theme);
            Color[] dustColors = LFTextures.GetDustColorsForTheme(theme);

            tooltips.Add(new TooltipLine(Mod, "Effect1",
                "Fires a thin beam toward the cursor that ricochets off surfaces"));
            tooltips.Add(new TooltipLine(Mod, "Effect2",
                "Right-click to cycle beam color theme"));
            tooltips.Add(new TooltipLine(Mod, "Theme",
                $"Current theme: {themeName}")
            {
                OverrideColor = dustColors[0]
            });
            tooltips.Add(new TooltipLine(Mod, "Lore",
                "'A thin slice of light, refracted endlessly'")
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
