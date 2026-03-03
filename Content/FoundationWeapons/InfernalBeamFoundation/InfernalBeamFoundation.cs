using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.FoundationWeapons.InfernalBeamFoundation
{
    /// <summary>
    /// InfernalBeamFoundation — Foundation weapon demonstrating a themed beam
    /// with the InfernalBeamRing spinning at the origin and the SoundWaveBeam
    /// texture composited along the beam body.
    ///
    /// Left-click: Fire a channeled beam that tracks the cursor.
    /// Right-click: Cycle beam color through all score theme gradient LUTs.
    ///
    /// Architecture: Self-contained — own shader, own texture registry, own projectile.
    /// </summary>
    public class InfernalBeamFoundation : ModItem
    {
        public override string Texture => "Terraria/Images/Item_" + ItemID.LastPrism;

        /// <summary>
        /// Current beam theme index. Static so the beam projectile can read it.
        /// </summary>
        public static int CurrentThemeIndex = 0;

        public override void SetStaticDefaults()
        {
            ItemID.Sets.ItemsThatAllowRepeatedRightClick[Type] = true;
        }

        public override void SetDefaults()
        {
            Item.damage = 75;
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
            Item.channel = true;
            Item.noUseGraphic = true;
            Item.shoot = ModContent.ProjectileType<InfernalBeam>();
            Item.shootSpeed = 1f;
        }

        public override bool AltFunctionUse(Player player) => true;

        public override bool CanUseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                Item.channel = false;
                Item.useTime = 15;
                Item.useAnimation = 15;
                Item.shoot = ProjectileID.None;
            }
            else
            {
                Item.channel = true;
                Item.useTime = 10;
                Item.useAnimation = 10;
                Item.shoot = ModContent.ProjectileType<InfernalBeam>();
            }
            return true;
        }

        public override bool? UseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                CurrentThemeIndex = (CurrentThemeIndex + 1) % (int)InfernalBeamTheme.COUNT;
                InfernalBeamTheme newTheme = (InfernalBeamTheme)CurrentThemeIndex;

                string themeName = IBFTextures.GetThemeName(newTheme);
                Color[] dustColors = IBFTextures.GetDustColorsForTheme(newTheme);
                CombatText.NewText(player.Hitbox, dustColors[0], themeName);
                SoundEngine.PlaySound(SoundID.MenuTick, player.Center);

                return true;
            }
            return null;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source,
            Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // Only allow one beam at a time per player
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
            InfernalBeamTheme theme = (InfernalBeamTheme)CurrentThemeIndex;
            string themeName = IBFTextures.GetThemeName(theme);
            Color[] dustColors = IBFTextures.GetDustColorsForTheme(theme);

            tooltips.Add(new TooltipLine(Mod, "Effect1",
                "Fires an infernal convergence beam with a spinning ring at the source"));
            tooltips.Add(new TooltipLine(Mod, "Effect2",
                "The beam body pulses with layered sound-wave energy"));
            tooltips.Add(new TooltipLine(Mod, "Effect3",
                "Right-click to cycle beam color theme"));
            tooltips.Add(new TooltipLine(Mod, "Theme",
                $"Current theme: {themeName}")
            {
                OverrideColor = dustColors[0]
            });
            tooltips.Add(new TooltipLine(Mod, "Lore",
                "'An infernal hymn made visible — the ring tolls, the beam sings'")
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
