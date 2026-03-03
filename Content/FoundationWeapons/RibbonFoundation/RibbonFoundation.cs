using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.FoundationWeapons.RibbonFoundation
{
    /// <summary>
    /// RibbonFoundation — Foundation weapon demonstrating 10 different ribbon/trail
    /// rendering techniques using textures from the VFX Asset Library.
    /// 
    /// Left-click: Fires a projectile rendered as a scaled-down Pulsating Music Note Orb.
    ///   The projectile leaves a ribbon trail behind it. The ribbon style depends on the
    ///   currently selected mode.
    /// 
    /// Right-click: Cycle through 10 ribbon rendering modes:
    ///   1. Pure Bloom — Bright bloom sprites stacked along the trail
    ///   2. Bloom + Noise Fade — Bloom with perlin noise erosion
    ///   3. Basic Trail — BasicTrail.png UV-mapped strip
    ///   4. Harmonic Wave — Harmonic Standing Wave Ribbon strip
    ///   5. Spiraling Vortex — Spiraling Vortex Energy Strip
    ///   6. Energy Surge — EnergySurgeBeam as ribbon fill
    ///   7. Cosmic Nebula — CosmicNebulaClouds noise as cloud ribbon
    ///   8. Musical Wave — MusicalWavePattern noise ribbon
    ///   9. Marble Flow — TileableMarbleNoise as veined ribbon
    ///  10. Lightning Ribbon — LightningSurge + bloom electric ribbon
    /// 
    /// Architecture:
    /// - Completely self-contained: own texture registry, own projectile
    /// - Does NOT depend on any global VFX systems or shaders
    /// - 0 mana cost, dirt crafting recipe for testing purposes
    /// </summary>
    public class RibbonFoundation : ModItem
    {
        // Uses vanilla Rainbow Rod sprite as placeholder
        public override string Texture => "Terraria/Images/Item_" + ItemID.RainbowRod;

        /// <summary>
        /// Current ribbon mode index. Static so the projectile can read it.
        /// </summary>
        public static int CurrentModeIndex = 0;

        public override void SetStaticDefaults()
        {
            ItemID.Sets.ItemsThatAllowRepeatedRightClick[Type] = true;
        }

        public override void SetDefaults()
        {
            Item.damage = 50;
            Item.DamageType = DamageClass.Magic;
            Item.width = 40;
            Item.height = 40;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 4f;
            Item.value = Item.sellPrice(gold: 5);
            Item.rare = ItemRarityID.Red;
            Item.autoReuse = true;
            Item.UseSound = SoundID.Item117;
            Item.shoot = ModContent.ProjectileType<RibbonProjectile>();
            Item.shootSpeed = 12f;
            Item.noMelee = true;
            Item.mana = 0;
        }

        public override bool AltFunctionUse(Player player) => true;

        public override bool CanUseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                // Right-click: cycle mode, don't shoot
                Item.useStyle = ItemUseStyleID.HoldUp;
                Item.useTime = 15;
                Item.useAnimation = 15;
                Item.shoot = ProjectileID.None;
                Item.noMelee = true;
            }
            else
            {
                // Left-click: fire orb projectile
                Item.useStyle = ItemUseStyleID.Shoot;
                Item.useTime = 20;
                Item.useAnimation = 20;
                Item.shoot = ModContent.ProjectileType<RibbonProjectile>();
                Item.noMelee = true;
            }
            return true;
        }

        public override bool? UseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                // Cycle to next ribbon mode
                CurrentModeIndex = (CurrentModeIndex + 1) % (int)RibbonMode.COUNT;
                RibbonMode newMode = (RibbonMode)CurrentModeIndex;

                string modeName = RBFTextures.GetModeName(newMode);
                Color modeColor = RBFTextures.GetModeColor(newMode);
                CombatText.NewText(player.Hitbox, modeColor, modeName);
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

            // ai[0] = current ribbon mode index
            Projectile.NewProjectile(source, position, velocity, type,
                damage, knockback, player.whoAmI, ai0: CurrentModeIndex);

            return false;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            RibbonMode mode = (RibbonMode)CurrentModeIndex;
            string modeName = RBFTextures.GetModeName(mode);
            Color modeColor = RBFTextures.GetModeColor(mode);

            tooltips.Add(new TooltipLine(Mod, "Effect1",
                "Fires a music note orb trailing a ribbon of light"));
            tooltips.Add(new TooltipLine(Mod, "Effect2",
                "Right-click to cycle through 10 ribbon styles"));
            tooltips.Add(new TooltipLine(Mod, "Mode",
                $"Current ribbon: {modeName}")
            {
                OverrideColor = modeColor
            });
            tooltips.Add(new TooltipLine(Mod, "Lore",
                "'Each ribbon sings a different verse of the same eternal melody'")
            {
                OverrideColor = new Color(200, 160, 255)
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
