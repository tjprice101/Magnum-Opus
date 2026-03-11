using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.FoundationWeapons.FoundationIncisorOrbs
{
    /// <summary>
    /// FoundationIncisorOrbs — Foundation weapon demonstrating the Incisor of Moonlight's
    /// "orb projectiles on swing" mechanic.
    ///
    /// This is a 1-to-1 skeleton of how the Incisor of Moonlight fires homing orb
    /// projectiles (LunarBeamProj) during its channelled swing arc.
    ///
    /// Architecture (mirrors IncisorOfMoonlight):
    /// - Left-click: Channelled swing via IncisorOrbSwingProj
    ///   → During the swing arc (60–100% of swing time), fires IncisorOrbProj
    ///     at regular intervals (BeamsPerSwing).
    ///   → IncisorOrbProj: flies straight initially, then homes on nearest NPC.
    ///   → IncisorOrbProj renders with shader-driven VertexStrip trail +
    ///     multi-layer bloom head (identical pipeline to LunarBeamProj).
    /// - Swing uses CurveSegment piecewise animation (Grave → Allegro → Diminuendo).
    /// - 0 mana cost, dirt crafting recipe for testing.
    ///
    /// Key systems demonstrated:
    /// 1. Channelled swing projectile with CurveSegment easing
    /// 2. Timed sub-projectile spawning during swing arc
    /// 3. Homing orb AI with delayed homing activation
    /// 4. VertexStrip trail with InfernalBeamBodyShader
    /// 5. Multi-layer additive bloom head rendering
    /// </summary>
    public class FoundationIncisorOrbs : ModItem
    {
        // =====================================================================
        // TUNING CONSTANTS — mirrors IncisorOfMoonlight
        // =====================================================================

        /// <summary>Damage multiplier for the orb projectiles (not true melee).</summary>
        public const float OrbDamagePenalty = 0.45f;

        /// <summary>Number of homing orbs fired per swing.</summary>
        public const int BeamsPerSwing = 3;

        // Sounds
        private static readonly SoundStyle SwingSound = SoundID.Item71 with { PitchVariance = 0.3f, Volume = 0.65f };

        public override string Texture => "Terraria/Images/Item_" + ItemID.Katana;

        // =====================================================================
        // SETUP
        // =====================================================================

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.width = 80;
            Item.height = 80;
            Item.damage = 100;
            Item.DamageType = DamageClass.MeleeNoSpeed;
            Item.useTime = 12;
            Item.useAnimation = 12;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 6.5f;
            Item.value = Item.sellPrice(gold: 5);
            Item.rare = ItemRarityID.Red;
            Item.autoReuse = true;
            Item.shootSpeed = 8f;

            // Channel-held swing — projectile-based, not vanilla swing
            Item.channel = true;
            Item.noUseGraphic = true;
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<IncisorOrbSwingProj>();
            Item.mana = 0;
        }

        // =====================================================================
        // TOOLTIPS
        // =====================================================================

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1",
                "Channelled swing fires homing orb projectiles"));
            tooltips.Add(new TooltipLine(Mod, "Effect2",
                "Orbs fly straight initially, then home on nearby enemies"));
            tooltips.Add(new TooltipLine(Mod, "Lore",
                "'A skeleton of the Incisor of Moonlight's orb-on-swing mechanic'")
            {
                OverrideColor = new Color(170, 140, 255)
            });
        }

        // =====================================================================
        // USAGE CONTROL — mirrors IncisorOfMoonlight
        // =====================================================================

        public override bool CanUseItem(Player player)
        {
            // Block use if swing projectile already exists
            if (player.ownedProjectileCounts[ModContent.ProjectileType<IncisorOrbSwingProj>()] > 0)
            {
                var proj = Main.projectile.FirstOrDefault(p =>
                    p.active && p.owner == player.whoAmI && p.type == Item.shoot);
                if (proj != default && proj.active)
                    return false;
            }
            return true;
        }

        public override bool CanShoot(Player player)
        {
            // Prevent spawning duplicates — swing projectile is channelled
            return player.ownedProjectileCounts[ModContent.ProjectileType<IncisorOrbSwingProj>()] <= 0;
        }

        // =====================================================================
        // SHOOT — spawns the swing projectile
        // =====================================================================

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source,
            Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Projectile.NewProjectile(source, position, velocity, type, damage, knockback,
                player.whoAmI, 0f, 0f);
            return false;
        }

        // =====================================================================
        // NO DIRECT HIT — damage is dealt by projectile
        // =====================================================================

        public override bool? CanHitNPC(Player player, NPC target) => false;
        public override bool CanHitPvp(Player player, Player target) => false;

        // =====================================================================
        // RECIPE — dirt for testing
        // =====================================================================

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.DirtBlock, 1)
                .AddTile(TileID.WorkBenches)
                .Register();
        }
    }
}
