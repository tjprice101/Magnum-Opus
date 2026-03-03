using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.FoundationWeapons.MaskFoundation
{
    /// <summary>
    /// MaskFoundation — Foundation melee weapon demonstrating radial noise-masked projectiles.
    /// 
    /// Left-click: Swing the sword. Each swing spawns 1 large homing orb projectile
    /// rendered as a vibrant circle with a noise texture radially scrolling through it,
    /// masked to a circular shape via a custom shader.
    /// 
    /// Right-click: Cycle the noise texture mapping through 11 different noise patterns
    /// (Perlin, Voronoi, Cosmic Vortex, FBM, Marble, Nebula, etc.)
    /// 
    /// Architecture notes:
    /// - Completely self-contained: own shader, own texture registry, own projectile
    /// - Does NOT depend on any global VFX systems
    /// - 0 mana cost, dirt crafting recipe for testing purposes
    /// </summary>
    public class MaskFoundation : ModItem
    {
        // Uses vanilla Terra Blade sprite as placeholder
        public override string Texture => "Terraria/Images/Item_" + ItemID.TerraBlade;

        /// <summary>
        /// Current noise mode index. Static so the projectile can read it
        /// in real time. Updated by right-click cycling.
        /// </summary>
        public static int CurrentModeIndex = 0;

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
            Item.knockBack = 6f;
            Item.value = Item.sellPrice(gold: 5);
            Item.rare = ItemRarityID.Red;
            Item.autoReuse = true;
            Item.UseSound = SoundID.Item1;
            Item.shoot = ModContent.ProjectileType<MaskOrbProjectile>();
            Item.shootSpeed = 8f;
        }

        public override bool AltFunctionUse(Player player) => true;

        public override bool CanUseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                // Right-click: cycle noise mode, don't swing
                Item.useStyle = ItemUseStyleID.HoldUp;
                Item.useTime = 15;
                Item.useAnimation = 15;
                Item.shoot = ProjectileID.None;
                Item.noMelee = true;
            }
            else
            {
                // Left-click: swing and spawn orb
                Item.useStyle = ItemUseStyleID.Swing;
                Item.useTime = 28;
                Item.useAnimation = 28;
                Item.shoot = ModContent.ProjectileType<MaskOrbProjectile>();
                Item.noMelee = false;
            }
            return true;
        }

        public override bool? UseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                // Cycle to next noise mode
                CurrentModeIndex = (CurrentModeIndex + 1) % (int)NoiseMode.COUNT;
                NoiseMode newMode = (NoiseMode)CurrentModeIndex;

                string modeName = MFTextures.GetModeName(newMode);
                Color[] modeColors = MFTextures.GetModeColors(newMode);
                CombatText.NewText(player.Hitbox, modeColors[0], modeName);
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

            // Spawn 1 homing orb projectile
            // ai[0] = current noise mode index
            Projectile.NewProjectile(source, position, velocity, type,
                damage, knockback, player.whoAmI, ai0: CurrentModeIndex);

            return false;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            NoiseMode mode = (NoiseMode)CurrentModeIndex;
            string modeName = MFTextures.GetModeName(mode);
            Color[] modeColors = MFTextures.GetModeColors(mode);

            tooltips.Add(new TooltipLine(Mod, "Effect1",
                "Each swing releases a large homing noise-masked orb"));
            tooltips.Add(new TooltipLine(Mod, "Effect2",
                "The orb displays a radially scrolling noise pattern masked to a circle"));
            tooltips.Add(new TooltipLine(Mod, "Effect3",
                "Right-click to cycle noise texture mapping"));
            tooltips.Add(new TooltipLine(Mod, "Mode",
                $"Current mode: {modeName}")
            {
                OverrideColor = modeColors[0]
            });
            tooltips.Add(new TooltipLine(Mod, "Lore",
                "'Every pattern holds a different voice — listen closely'")
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
