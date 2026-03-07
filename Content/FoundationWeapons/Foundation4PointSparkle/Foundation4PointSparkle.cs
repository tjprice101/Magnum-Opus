using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.FoundationWeapons.Foundation4PointSparkle
{
    /// <summary>
    /// Foundation4PointSparkle — Foundation weapon demonstrating dazzling 4-point sparkle
    /// explosion effects using star projectile assets.
    ///
    /// Left-click: Fires a glowing star ball projectile. On impact with any tile or enemy,
    /// it detonates into a spectacular display of 4-point sparkles of various sizes,
    /// rotations, and brightness — all using the four star assets.
    ///
    /// Right-click: Toggles "Sparkle Trail" mode. When active, the projectile also
    /// leaves a trail of twinkling, flashing sparkles behind it as it travels —
    /// creating a dazzling ribbon of stars in its wake.
    ///
    /// Architecture notes:
    /// - Completely self-contained: own texture registry, own projectiles
    /// - Does NOT depend on any global VFX systems
    /// - Uses 4 specific star assets: 4PointStarShiningProjectile, 8-Point Starburst Flare,
    ///   BrightStarProjectile1, BrightStarProjectile2
    /// - Ranged weapon, no ammo consumption
    /// - 0 mana cost, dirt crafting recipe for testing purposes
    /// </summary>
    public class Foundation4PointSparkle : ModItem
    {
        // Uses vanilla Star Cannon sprite as placeholder
        public override string Texture => "Terraria/Images/Item_" + ItemID.StarCannon;

        /// <summary>
        /// Current fire mode. Static so projectiles can read it.
        /// 0 = Normal (impact sparkles only), 1 = SparkleTrail (trail + impact sparkles)
        /// </summary>
        public static int CurrentModeIndex = 0;

        public override void SetStaticDefaults()
        {
            ItemID.Sets.ItemsThatAllowRepeatedRightClick[Type] = true;
        }

        public override void SetDefaults()
        {
            Item.damage = 50;
            Item.DamageType = DamageClass.Ranged;
            Item.width = 40;
            Item.height = 40;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 3f;
            Item.value = Item.sellPrice(gold: 5);
            Item.rare = ItemRarityID.Red;
            Item.autoReuse = true;
            Item.UseSound = SoundID.Item9;
            Item.shoot = ModContent.ProjectileType<SparkleStarProjectile>();
            Item.shootSpeed = 14f;
            Item.noMelee = true;
            Item.useAmmo = AmmoID.None; // No ammo consumption
            Item.mana = 0;
        }

        public override bool AltFunctionUse(Player player) => true;

        public override bool CanUseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                // Right-click: toggle trail mode, don't fire
                Item.useStyle = ItemUseStyleID.HoldUp;
                Item.useTime = 15;
                Item.useAnimation = 15;
                Item.shoot = ProjectileID.None;
            }
            else
            {
                // Left-click: fire star projectile
                Item.useStyle = ItemUseStyleID.Shoot;
                Item.useTime = 20;
                Item.useAnimation = 20;
                Item.shoot = ModContent.ProjectileType<SparkleStarProjectile>();
            }
            return true;
        }

        public override bool? UseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                // Toggle sparkle trail mode
                CurrentModeIndex = (CurrentModeIndex + 1) % (int)SparkleFireMode.COUNT;
                SparkleFireMode newMode = (SparkleFireMode)CurrentModeIndex;

                string modeName = F4PSTextures.GetModeName(newMode);
                Color[] colors = F4PSTextures.GetSparkleColors();
                CombatText.NewText(player.Hitbox, colors[0], modeName);
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

            // ai[0] = fire mode index (0 = normal, 1 = sparkle trail)
            Projectile.NewProjectile(source, position, velocity, type,
                damage, knockback, player.whoAmI, ai0: CurrentModeIndex);

            return false;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            SparkleFireMode mode = (SparkleFireMode)CurrentModeIndex;
            string modeName = F4PSTextures.GetModeName(mode);
            Color[] colors = F4PSTextures.GetSparkleColors();

            tooltips.Add(new TooltipLine(Mod, "Effect1",
                "Fires a shining star projectile that detonates into dazzling sparkles on impact"));
            tooltips.Add(new TooltipLine(Mod, "Effect2",
                "Right-click to toggle twinkling trail mode"));
            tooltips.Add(new TooltipLine(Mod, "Mode",
                $"Current mode: {modeName}")
            {
                OverrideColor = colors[0]
            });
            tooltips.Add(new TooltipLine(Mod, "Lore",
                "'A constellation captured in crystal, released in brilliant bursts of light'")
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
