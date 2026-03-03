using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.FoundationWeapons.MagicOrbFoundation
{
    /// <summary>
    /// MagicOrbFoundation — Foundation weapon demonstrating noise-textured floating orbs
    /// that fire bloom-based shiny projectiles at nearby enemies.
    /// 
    /// Left-click: Casts a single floating orb that drifts slowly toward the cursor
    ///   direction. The orb uses a noise texture radial shader and periodically
    ///   fires shiny bloom bolt sub-projectiles at enemies within radius.
    /// 
    /// Right-click: Casts 3 orbs in a spread that move faster and each explode
    ///   after a shorter lifetime, dealing area damage with burst VFX.
    /// 
    /// Architecture:
    /// - Self-contained: reuses RadialNoiseMaskShader from MaskFoundation, own texture
    ///   registry, own projectiles
    /// - 0 mana cost, dirt crafting recipe for testing
    /// </summary>
    public class MagicOrbFoundation : ModItem
    {
        public override string Texture => "Terraria/Images/Item_" + ItemID.NebulaBlaze;

        public override void SetStaticDefaults()
        {
            ItemID.Sets.ItemsThatAllowRepeatedRightClick[Type] = true;
        }

        public override void SetDefaults()
        {
            Item.damage = 45;
            Item.DamageType = DamageClass.Magic;
            Item.width = 40;
            Item.height = 40;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 3f;
            Item.value = Item.sellPrice(gold: 5);
            Item.rare = ItemRarityID.Red;
            Item.autoReuse = true;
            Item.UseSound = SoundID.Item117;
            Item.shoot = ModContent.ProjectileType<MagicOrb>();
            Item.shootSpeed = 4f;
            Item.noMelee = true;
            Item.mana = 0;
        }

        public override bool AltFunctionUse(Player player) => true;

        public override bool CanUseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                // Right-click: burst mode — 3 faster orbs
                Item.useTime = 40;
                Item.useAnimation = 40;
                Item.shootSpeed = 7f;
                Item.UseSound = SoundID.Item122;
            }
            else
            {
                // Left-click: single slow orb
                Item.useTime = 30;
                Item.useAnimation = 30;
                Item.shootSpeed = 4f;
                Item.UseSound = SoundID.Item117;
            }
            return true;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source,
            Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (player.altFunctionUse == 2)
            {
                // Right-click: spawn 3 orbs in a spread, with burst flag ai[1] = 1
                float baseAngle = velocity.ToRotation();
                float spread = 0.3f; // ~17 degrees

                for (int i = -1; i <= 1; i++)
                {
                    float angle = baseAngle + i * spread;
                    Vector2 vel = angle.ToRotationVector2() * velocity.Length();
                    // ai[0] = random noise style, ai[1] = 1 (burst mode)
                    int noiseStyle = Main.rand.Next((int)OrbNoiseStyle.COUNT);
                    Projectile.NewProjectile(source, position, vel, type,
                        damage, knockback, player.whoAmI,
                        ai0: noiseStyle, ai1: 1f);
                }
                return false;
            }
            else
            {
                // Left-click: single slow orb
                // ai[0] = random noise style, ai[1] = 0 (normal mode)
                int noiseStyle = Main.rand.Next((int)OrbNoiseStyle.COUNT);
                Projectile.NewProjectile(source, position, velocity, type,
                    damage, knockback, player.whoAmI,
                    ai0: noiseStyle, ai1: 0f);
                return false;
            }
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1",
                "Casts a floating noise orb that fires shiny bolts at nearby enemies"));
            tooltips.Add(new TooltipLine(Mod, "Effect2",
                "Right-click to launch 3 fast orbs that explode on expiry"));
            tooltips.Add(new TooltipLine(Mod, "Lore",
                "'The orb remembers the shape of every sound it has ever heard'")
            {
                OverrideColor = new Color(160, 80, 220)
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
