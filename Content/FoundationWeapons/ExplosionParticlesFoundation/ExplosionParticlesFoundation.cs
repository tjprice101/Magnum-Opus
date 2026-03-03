using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.FoundationWeapons.ExplosionParticlesFoundation
{
    /// <summary>
    /// ExplosionParticlesFoundation — Foundation weapon demonstrating three kinds
    /// of explosion spark particle fields.
    ///
    /// Left-click: Swing fires a projectile that travels to the hit point and
    /// detonates into a scattering field of elongated sparks/debris particles.
    ///
    /// Right-click: Cycle through the 3 spark explosion modes:
    ///   - Radial Scatter: Classic uniform outward burst of hot sparks (orange/gold)
    ///   - Fountain Cascade: Sparks shoot upward then arc with gravity (blue/cyan)
    ///   - Spiral Shrapnel: Sparks spin outward in a spiral pattern (purple/pink)
    ///
    /// Architecture notes:
    /// - Completely self-contained: own texture registry, own projectiles
    /// - Does NOT depend on any global VFX systems
    /// - 0 mana cost, dirt crafting recipe for testing purposes
    /// </summary>
    public class ExplosionParticlesFoundation : ModItem
    {
        // Uses vanilla Meowmere sprite as placeholder
        public override string Texture => "Terraria/Images/Item_" + ItemID.Meowmere;

        /// <summary>
        /// Current spark mode index. Static so projectiles can read it in real time.
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
            Item.knockBack = 5f;
            Item.value = Item.sellPrice(gold: 5);
            Item.rare = ItemRarityID.Red;
            Item.autoReuse = true;
            Item.UseSound = SoundID.Item1;
            Item.shoot = ModContent.ProjectileType<SparkCarrierProjectile>();
            Item.shootSpeed = 16f;
            Item.noMelee = true;
            Item.mana = 0; // Foundation test weapon — free to cast
        }

        public override bool AltFunctionUse(Player player) => true;

        public override bool CanUseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                // Right-click: cycle mode, don't fire
                Item.useStyle = ItemUseStyleID.HoldUp;
                Item.useTime = 15;
                Item.useAnimation = 15;
                Item.shoot = ProjectileID.None;
            }
            else
            {
                // Left-click: swing fires projectile
                Item.useStyle = ItemUseStyleID.Swing;
                Item.useTime = 28;
                Item.useAnimation = 28;
                Item.shoot = ModContent.ProjectileType<SparkCarrierProjectile>();
            }
            return true;
        }

        public override bool? UseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                // Cycle to next spark mode
                CurrentModeIndex = (CurrentModeIndex + 1) % (int)SparkMode.COUNT;
                SparkMode newMode = (SparkMode)CurrentModeIndex;

                string modeName = EPFTextures.GetModeName(newMode);
                Color[] modeColors = EPFTextures.GetModeColors(newMode);
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

            // ai[0] = current spark mode index
            Projectile.NewProjectile(source, position, velocity, type,
                damage, knockback, player.whoAmI, ai0: CurrentModeIndex);

            return false;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            SparkMode mode = (SparkMode)CurrentModeIndex;
            string modeName = EPFTextures.GetModeName(mode);
            Color[] modeColors = EPFTextures.GetModeColors(mode);

            tooltips.Add(new TooltipLine(Mod, "Effect1",
                "Fires a projectile that detonates into a field of scattering sparks"));
            tooltips.Add(new TooltipLine(Mod, "Effect2",
                "Radial Scatter: Classic outward burst of hot orange sparks"));
            tooltips.Add(new TooltipLine(Mod, "Effect3",
                "Fountain Cascade: Upward spray of sparks that arc with gravity"));
            tooltips.Add(new TooltipLine(Mod, "Effect4",
                "Spiral Shrapnel: Spinning spiral of shrapnel fragments"));
            tooltips.Add(new TooltipLine(Mod, "Effect5",
                "Right-click to cycle explosion type"));
            tooltips.Add(new TooltipLine(Mod, "Mode",
                $"Current mode: {modeName}")
            {
                OverrideColor = modeColors[0]
            });
            tooltips.Add(new TooltipLine(Mod, "Lore",
                "'From one note, a thousand fragments — each carrying the echo of impact'")
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
