using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.FoundationWeapons.ImpactFoundation
{
    /// <summary>
    /// ImpactFoundation — Foundation weapon demonstrating three kinds of impact effects.
    /// 
    /// Left-click: Fire a homing projectile that spawns an impact effect on hit.
    /// The type of impact depends on the current mode:
    ///   - Ripple: Expanding concentric ring ripple at the impact point
    ///   - Damage Zone: Lasting 5-second noise-masked circle zone that deals damage and sparkles
    ///   - Slash Mark: Fluid directional slash mark rendered at the hit location
    /// 
    /// Right-click: Cycle through the 3 impact modes (Ripple → Damage Zone → Slash Mark).
    /// 
    /// Architecture notes:
    /// - Completely self-contained: own shaders, own texture registry, own projectiles
    /// - Does NOT depend on any global VFX systems
    /// - 0 mana cost, dirt crafting recipe for testing purposes
    /// </summary>
    public class ImpactFoundation : ModItem
    {
        // Uses vanilla Meowmere sprite as placeholder
        public override string Texture => "Terraria/Images/Item_" + ItemID.Meowmere;

        /// <summary>
        /// Current impact mode index. Static so projectiles can read it in real time.
        /// </summary>
        public static int CurrentModeIndex = 0;

        public override void SetStaticDefaults()
        {
            ItemID.Sets.ItemsThatAllowRepeatedRightClick[Type] = true;
        }

        public override void SetDefaults()
        {
            Item.damage = 60;
            Item.DamageType = DamageClass.Magic;
            Item.width = 40;
            Item.height = 40;
            Item.useTime = 24;
            Item.useAnimation = 24;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 5f;
            Item.value = Item.sellPrice(gold: 5);
            Item.rare = ItemRarityID.Red;
            Item.autoReuse = true;
            Item.UseSound = SoundID.Item72;
            Item.shoot = ModContent.ProjectileType<ImpactProjectile>();
            Item.shootSpeed = 14f;
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
                // Left-click: fire projectile
                Item.useStyle = ItemUseStyleID.Shoot;
                Item.useTime = 24;
                Item.useAnimation = 24;
                Item.shoot = ModContent.ProjectileType<ImpactProjectile>();
            }
            return true;
        }

        public override bool? UseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                // Cycle to next impact mode
                CurrentModeIndex = (CurrentModeIndex + 1) % (int)ImpactMode.COUNT;
                ImpactMode newMode = (ImpactMode)CurrentModeIndex;

                string modeName = IFTextures.GetModeName(newMode);
                Color[] modeColors = IFTextures.GetModeColors(newMode);
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

            // ai[0] = current impact mode index
            Projectile.NewProjectile(source, position, velocity, type,
                damage, knockback, player.whoAmI, ai0: CurrentModeIndex);

            return false;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            ImpactMode mode = (ImpactMode)CurrentModeIndex;
            string modeName = IFTextures.GetModeName(mode);
            Color[] modeColors = IFTextures.GetModeColors(mode);

            tooltips.Add(new TooltipLine(Mod, "Effect1",
                "Fires a homing projectile that creates an impact effect on hit"));
            tooltips.Add(new TooltipLine(Mod, "Effect2",
                "Ripple: Expanding concentric ring shockwave"));
            tooltips.Add(new TooltipLine(Mod, "Effect3",
                "Damage Zone: Lasting sparkling area that damages enemies"));
            tooltips.Add(new TooltipLine(Mod, "Effect4",
                "Slash Mark: Fluid directional slash rendered at impact"));
            tooltips.Add(new TooltipLine(Mod, "Effect5",
                "Right-click to cycle impact type"));
            tooltips.Add(new TooltipLine(Mod, "Mode",
                $"Current mode: {modeName}")
            {
                OverrideColor = modeColors[0]
            });
            tooltips.Add(new TooltipLine(Mod, "Lore",
                "'Every strike leaves its mark — some linger, some ripple, some cut deep'")
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
