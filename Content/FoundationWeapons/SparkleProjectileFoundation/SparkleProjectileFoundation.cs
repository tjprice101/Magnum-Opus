using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.FoundationWeapons.SparkleProjectileFoundation
{
    /// <summary>
    /// SparkleProjectileFoundation — Foundation weapon demonstrating homing sparkle crystal projectiles.
    /// 
    /// Left-click: Swing the sword. Each swing spawns 3 small homing crystal projectiles
    /// that rotate, sparkle, and leave glittery trails as they home toward enemies.
    /// 
    /// Right-click: Cycle crystal color theme through all score theme gradient LUTs.
    /// 
    /// Architecture notes:
    /// - Completely self-contained: own textures, own projectile, own VFX rendering
    /// - Does NOT depend on any global VFX systems
    /// - 0 mana cost, dirt crafting recipe for testing purposes
    /// </summary>
    public class SparkleProjectileFoundation : ModItem
    {
        // Uses vanilla Enchanted Sword sprite as placeholder
        public override string Texture => "Terraria/Images/Item_" + ItemID.EnchantedSword;

        /// <summary>
        /// Current crystal theme index. Static so crystal projectiles can read it
        /// in real time. Updated by right-click cycling.
        /// </summary>
        public static int CurrentThemeIndex = 0;

        public override void SetStaticDefaults()
        {
            ItemID.Sets.ItemsThatAllowRepeatedRightClick[Type] = true;
        }

        public override void SetDefaults()
        {
            Item.damage = 55;
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
            Item.UseSound = SoundID.Item1;
            Item.shoot = ModContent.ProjectileType<SparkleCrystalProjectile>();
            Item.shootSpeed = 10f;
        }

        public override bool AltFunctionUse(Player player) => true;

        public override bool CanUseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                // Right-click: cycle theme, don't swing
                Item.useStyle = ItemUseStyleID.HoldUp;
                Item.useTime = 15;
                Item.useAnimation = 15;
                Item.shoot = ProjectileID.None;
                Item.noMelee = true;
            }
            else
            {
                // Left-click: swing and spawn crystals
                Item.useStyle = ItemUseStyleID.Swing;
                Item.useTime = 22;
                Item.useAnimation = 22;
                Item.shoot = ModContent.ProjectileType<SparkleCrystalProjectile>();
                Item.noMelee = false;
            }
            return true;
        }

        public override bool? UseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                // Cycle to next theme
                CurrentThemeIndex = (CurrentThemeIndex + 1) % (int)CrystalTheme.COUNT;
                CrystalTheme newTheme = (CrystalTheme)CurrentThemeIndex;

                string themeName = SPFTextures.GetThemeName(newTheme);
                Color[] themeColors = SPFTextures.GetThemeColors(newTheme);
                CombatText.NewText(player.Hitbox, themeColors[0], themeName);
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

            // Spawn 3 crystals in a spread pattern
            float baseAngle = velocity.ToRotation();
            float spreadAngle = MathHelper.ToRadians(25f);

            for (int i = 0; i < 3; i++)
            {
                // Fan out: -1, 0, +1 offset from center
                float angleOffset = (i - 1) * spreadAngle;
                Vector2 crystalVelocity = (baseAngle + angleOffset).ToRotationVector2() * velocity.Length();

                // Slight speed variation for visual interest
                crystalVelocity *= Main.rand.NextFloat(0.85f, 1.15f);

                // ai[0] = crystal index (0,1,2) — determines phase offset for rotation/sparkle timing
                // ai[1] = current theme index
                Projectile.NewProjectile(source, position, crystalVelocity, type,
                    damage, knockback, player.whoAmI, ai0: i, ai1: CurrentThemeIndex);
            }

            return false; // We handle spawning manually
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            CrystalTheme theme = (CrystalTheme)CurrentThemeIndex;
            string themeName = SPFTextures.GetThemeName(theme);
            Color[] themeColors = SPFTextures.GetThemeColors(theme);

            tooltips.Add(new TooltipLine(Mod, "Effect1",
                "Each swing releases three homing crystal shards"));
            tooltips.Add(new TooltipLine(Mod, "Effect2",
                "Crystals sparkle and leave glittering trails as they seek enemies"));
            tooltips.Add(new TooltipLine(Mod, "Effect3",
                "Right-click to cycle crystal color theme"));
            tooltips.Add(new TooltipLine(Mod, "Theme",
                $"Current theme: {themeName}")
            {
                OverrideColor = themeColors[0]
            });
            tooltips.Add(new TooltipLine(Mod, "Lore",
                "'Fragments of a shattered symphony, each shard still singing'")
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
