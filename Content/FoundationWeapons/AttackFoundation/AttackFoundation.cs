using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.FoundationWeapons.AttackFoundation
{
    /// <summary>
    /// AttackFoundation — Foundation weapon demonstrating five distinct attack modes,
    /// each with a unique animation and weapon class feel.
    ///
    /// Left-click: Fires the current attack mode's projectile.
    /// Right-click: Cycles through the 5 attack modes.
    ///
    /// Modes:
    ///   1. Throw Slam — Throws the sword upward; it spins and slams the nearest enemy (Melee)
    ///   2. Combo Swing — Swings down, back up, then spins the blade toward the cursor (Melee)
    ///   3. Astralgraph — Summons an arcane astralgraph circle around the player (Magic)
    ///   4. Flaming Ring — Creates a flaming ring that orbits and damages enemies (Summoner)
    ///   5. Ranger Shot — Fires piercing bolts with muzzle flash and tracer VFX (Ranged)
    ///
    /// Architecture notes:
    /// - Completely self-contained: own texture registry, own projectiles
    /// - Does NOT depend on any global VFX systems
    /// - 0 mana cost, dirt crafting recipe for testing purposes
    /// </summary>
    public class AttackFoundation : ModItem
    {
        // Uses vanilla Meowmere sprite as placeholder
        public override string Texture => "Terraria/Images/Item_" + ItemID.Meowmere;

        /// <summary>
        /// Current attack mode index. Static so projectiles can read it in real time.
        /// </summary>
        public static int CurrentModeIndex = 0;

        public override void SetStaticDefaults()
        {
            ItemID.Sets.ItemsThatAllowRepeatedRightClick[Type] = true;
        }

        public override void SetDefaults()
        {
            Item.damage = 70;
            Item.DamageType = DamageClass.Melee;
            Item.width = 40;
            Item.height = 40;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 5f;
            Item.value = Item.sellPrice(gold: 5);
            Item.rare = ItemRarityID.Red;
            Item.autoReuse = true;
            Item.UseSound = SoundID.Item1;
            Item.shoot = ModContent.ProjectileType<ThrowSlamProjectile>();
            Item.shootSpeed = 12f;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.mana = 0; // Foundation test weapon — free to cast
        }

        public override bool AltFunctionUse(Player player) => true;

        public override bool CanUseItem(Player player)
        {
            AttackMode mode = (AttackMode)CurrentModeIndex;

            if (player.altFunctionUse == 2)
            {
                // Right-click: cycle mode, don't fire
                Item.useStyle = ItemUseStyleID.HoldUp;
                Item.useTime = 15;
                Item.useAnimation = 15;
                Item.shoot = ProjectileID.None;
                Item.noMelee = true;
                Item.noUseGraphic = true;
                return true;
            }

            // Left-click: configure based on current mode
            switch (mode)
            {
                case AttackMode.ThrowSlam:
                    Item.useStyle = ItemUseStyleID.Swing;
                    Item.useTime = 40;
                    Item.useAnimation = 40;
                    Item.shoot = ModContent.ProjectileType<ThrowSlamProjectile>();
                    Item.shootSpeed = 12f;
                    Item.DamageType = DamageClass.Melee;
                    Item.UseSound = SoundID.Item1;
                    Item.noMelee = true;
                    Item.noUseGraphic = true;
                    Item.autoReuse = true;
                    Item.channel = false;
                    break;

                case AttackMode.ComboSwing:
                    Item.useStyle = ItemUseStyleID.Shoot;
                    Item.useTime = 50;
                    Item.useAnimation = 50;
                    Item.shoot = ModContent.ProjectileType<ComboSwingProjectile>();
                    Item.shootSpeed = 1f;
                    Item.DamageType = DamageClass.Melee;
                    Item.UseSound = SoundID.Item71;
                    Item.noMelee = true;
                    Item.noUseGraphic = true;
                    Item.autoReuse = true;
                    Item.channel = false;
                    break;

                case AttackMode.Astralgraph:
                    Item.useStyle = ItemUseStyleID.Shoot;
                    Item.useTime = 35;
                    Item.useAnimation = 35;
                    Item.shoot = ModContent.ProjectileType<AstralgraphProjectile>();
                    Item.shootSpeed = 1f;
                    Item.DamageType = DamageClass.Magic;
                    Item.UseSound = SoundID.Item8;
                    Item.noMelee = true;
                    Item.noUseGraphic = true;
                    Item.autoReuse = true;
                    Item.channel = false;
                    break;

                case AttackMode.FlamingRing:
                    Item.useStyle = ItemUseStyleID.Shoot;
                    Item.useTime = 45;
                    Item.useAnimation = 45;
                    Item.shoot = ModContent.ProjectileType<FlamingRingProjectile>();
                    Item.shootSpeed = 1f;
                    Item.DamageType = DamageClass.Summon;
                    Item.UseSound = SoundID.Item46;
                    Item.noMelee = true;
                    Item.noUseGraphic = true;
                    Item.autoReuse = true;
                    Item.channel = false;
                    break;

                case AttackMode.RangerShot:
                    Item.useStyle = ItemUseStyleID.Shoot;
                    Item.useTime = 12;
                    Item.useAnimation = 12;
                    Item.shoot = ModContent.ProjectileType<RangerShotProjectile>();
                    Item.shootSpeed = 18f;
                    Item.DamageType = DamageClass.Ranged;
                    Item.UseSound = SoundID.Item12;
                    Item.noMelee = true;
                    Item.noUseGraphic = true;
                    Item.autoReuse = true;
                    Item.channel = false;
                    break;
            }

            // Don't allow if the same projectile type is already active (for single-instance modes)
            if (mode == AttackMode.FlamingRing || mode == AttackMode.Astralgraph)
            {
                for (int i = 0; i < Main.maxProjectiles; i++)
                {
                    if (Main.projectile[i].active && Main.projectile[i].type == Item.shoot
                        && Main.projectile[i].owner == player.whoAmI)
                        return false;
                }
            }

            return true;
        }

        public override bool? UseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                // Cycle to next attack mode
                CurrentModeIndex = (CurrentModeIndex + 1) % (int)AttackMode.COUNT;
                AttackMode newMode = (AttackMode)CurrentModeIndex;

                string modeName = AFTextures.GetModeName(newMode);
                Color[] modeColors = AFTextures.GetModeColors(newMode);
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

            AttackMode mode = (AttackMode)CurrentModeIndex;

            // ai[0] = current mode index for all projectiles
            Projectile.NewProjectile(source, position, velocity, type,
                damage, knockback, player.whoAmI, ai0: CurrentModeIndex);

            return false;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            AttackMode mode = (AttackMode)CurrentModeIndex;
            string modeName = AFTextures.GetModeName(mode);
            Color[] modeColors = AFTextures.GetModeColors(mode);

            tooltips.Add(new TooltipLine(Mod, "Effect1",
                "A multi-form weapon with five distinct attack animations"));
            tooltips.Add(new TooltipLine(Mod, "Effect2",
                "Throw Slam: Hurls the blade skyward to slam the nearest foe"));
            tooltips.Add(new TooltipLine(Mod, "Effect3",
                "Combo Swing: Three-part melee combo ending with a spinning throw"));
            tooltips.Add(new TooltipLine(Mod, "Effect4",
                "Astralgraph: Conjures an arcane star circle around the player"));
            tooltips.Add(new TooltipLine(Mod, "Effect5",
                "Flaming Ring: Summons a ring of fire orbiting the player"));
            tooltips.Add(new TooltipLine(Mod, "Effect6",
                "Ranger Shot: Fires piercing bolts with tracer VFX"));
            tooltips.Add(new TooltipLine(Mod, "Effect7",
                "Right-click to cycle attack mode"));
            tooltips.Add(new TooltipLine(Mod, "Mode",
                $"Current mode: {modeName}")
            {
                OverrideColor = modeColors[0]
            });
            tooltips.Add(new TooltipLine(Mod, "Lore",
                "'Five voices, one instrument — the symphony demands versatility'")
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
