using MagnumOpus.Common;
using MagnumOpus.Content.OdeToJoy.Weapons.ThornboundReckoning.Projectiles;
using MagnumOpus.Content.OdeToJoy.Weapons.ThornboundReckoning.Utilities;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.Fate.CraftingStations;
using MagnumOpus.Content.OdeToJoy.HarmonicCores;
using MagnumOpus.Content.OdeToJoy.ResonanceEnergies;

namespace MagnumOpus.Content.OdeToJoy.Weapons.ThornboundReckoning
{
    /// <summary>
    /// Thornbound Reckoning — The opening chord of Ode to Joy's garden.
    /// A greatsword wreathed in living thorns that channels nature's triumphant fury.
    ///
    /// 3-Phase Botanical Combo:
    ///   Phase 1 — Vine Wave: Horizontal sweep + traveling vine wave projectile
    ///   Phase 2 — Thorn Lash: Rising diagonal + V-pattern thorn lash projectiles
    ///   Phase 3 — Botanical Burst: Overhead slam + thorn wall zone denial
    ///
    /// Reckoning Charge builds through hits. At full charge, Phase 3 creates
    /// double-width thorn wall + golden botanical burst explosion.
    /// </summary>
    public class ThornboundReckoning : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 70;
            Item.height = 70;
            Item.damage = 4200;
            Item.DamageType = DamageClass.Melee;
            Item.useTime = 22;
            Item.useAnimation = 22;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 8f;
            Item.value = Item.sellPrice(platinum: 3, gold: 50);
            Item.rare = ModContent.RarityType<OdeToJoyRarity>();
            Item.UseSound = SoundID.Item71;
            Item.autoReuse = true;
            Item.scale = 1.5f;
            Item.crit = 18;
            Item.shoot = ModContent.ProjectileType<ThornboundSwingProj>();
            Item.shootSpeed = 1f;
            Item.noMelee = true;
            Item.noUseGraphic = true;
        }

        public override bool CanUseItem(Player player)
        {
            // Prevent overlapping swings
            int projType = ModContent.ProjectileType<ThornboundSwingProj>();
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                if (Main.projectile[i].active && Main.projectile[i].type == projType
                    && Main.projectile[i].owner == player.whoAmI)
                    return false;
            }

            // Adjust timing per combo phase
            var tbp = player.GetModPlayer<ThornboundPlayer>();
            switch (tbp.ComboPhase)
            {
                case 0: // Vine Wave — horizontal sweep
                    Item.useTime = 22;
                    Item.useAnimation = 22;
                    break;
                case 1: // Thorn Lash — faster diagonal
                    Item.useTime = 18;
                    Item.useAnimation = 18;
                    break;
                case 2: // Botanical Burst — overhead slam (slower, heavier)
                    Item.useTime = 28;
                    Item.useAnimation = 28;
                    break;
            }

            return true;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source,
            Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            var tbp = player.GetModPlayer<ThornboundPlayer>();
            int phase = tbp.ComboPhase;

            Vector2 aimDir = (Main.MouseWorld - player.MountedCenter).SafeNormalize(Vector2.UnitX);

            // ai[0] = combo phase, ai[1] = reckoning charge (0 or 1 for empowered)
            float empowered = (phase == 2 && tbp.ReckoningCharge >= 100f) ? 1f : 0f;

            Projectile.NewProjectile(source, player.MountedCenter, aimDir, type,
                damage, knockback, player.whoAmI, ai0: phase, ai1: empowered);

            // Spawn sub-projectiles based on combo phase
            if (phase == 0)
            {
                // Phase 1: Vine Wave projectile
                Vector2 waveDir = aimDir * 10f;
                Projectile.NewProjectile(source, player.MountedCenter, waveDir,
                    ModContent.ProjectileType<VineWaveProjectile>(),
                    (int)(damage * 0.6f), knockback * 0.5f, player.whoAmI);
            }
            else if (phase == 1)
            {
                // Phase 2: Thorn Lash V-pattern (2 projectiles)
                float spread = MathHelper.ToRadians(18f);
                for (int i = -1; i <= 1; i += 2)
                {
                    Vector2 lashDir = aimDir.RotatedBy(spread * i) * 12f;
                    Projectile.NewProjectile(source, player.MountedCenter, lashDir,
                        ModContent.ProjectileType<ThornLashProjectile>(),
                        (int)(damage * 0.45f), knockback * 0.3f, player.whoAmI);
                }
            }
            else if (phase == 2)
            {
                // Phase 3: Thorn Wall zone + optional botanical burst
                bool isEmpowered = empowered >= 1f;
                Vector2 wallPos = Main.MouseWorld;

                Projectile.NewProjectile(source, wallPos, Vector2.Zero,
                    ModContent.ProjectileType<ThornWallProjectile>(),
                    (int)(damage * 0.35f), knockback * 0.8f, player.whoAmI,
                    ai0: isEmpowered ? 1f : 0f);

                if (isEmpowered)
                {
                    // Botanical Burst explosion at full charge
                    Projectile.NewProjectile(source, wallPos, Vector2.Zero,
                        ModContent.ProjectileType<BotanicalBurstProjectile>(),
                        (int)(damage * 0.8f), knockback * 1.2f, player.whoAmI);

                    tbp.ReckoningCharge = 0f;

                    SoundEngine.PlaySound(SoundID.Item119 with { Volume = 0.9f, Pitch = -0.2f },
                        player.MountedCenter);
                }
            }

            tbp.AdvanceCombo();
            return false;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<ResonantCoreOfOdeToJoy>(), 20)
                .AddIngredient(ModContent.ItemType<OdeToJoyResonantEnergy>(), 15)
                .AddIngredient(ModContent.ItemType<HarmonicCoreOfOdeToJoy>(), 2)
                .AddIngredient(ItemID.LunarBar, 15)
                .AddTile(ModContent.TileType<FatesCosmicAnvilTile>())
                .Register();
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1",
                "3-phase botanical combo: Vine Wave → Thorn Lash → Botanical Burst"));
            tooltips.Add(new TooltipLine(Mod, "Effect2",
                "Vine waves leave thorn residue that amplifies thorn wall damage by 25%"));
            tooltips.Add(new TooltipLine(Mod, "Effect3",
                "Embedded thorns inflict Rose Thorn Bleed (stacks up to 5x)"));
            tooltips.Add(new TooltipLine(Mod, "Effect4",
                "Reckoning Charge builds through hits — at full charge, unleash a devastating botanical burst"));
            tooltips.Add(new TooltipLine(Mod, "Lore",
                "'The vine does not ask permission to grow. It simply overcomes.'")
            {
                OverrideColor = ThornboundTextures.LoreColor
            });
        }
    }
}