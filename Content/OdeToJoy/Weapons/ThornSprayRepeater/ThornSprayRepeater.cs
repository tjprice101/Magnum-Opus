using MagnumOpus.Common;
using MagnumOpus.Content.OdeToJoy.Weapons.ThornSprayRepeater.Projectiles;
using MagnumOpus.Content.OdeToJoy.Weapons.ThornSprayRepeater.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.Fate.CraftingStations;
using MagnumOpus.Content.OdeToJoy.HarmonicCores;
using MagnumOpus.Content.OdeToJoy.ResonanceEnergies;

namespace MagnumOpus.Content.OdeToJoy.Weapons.ThornSprayRepeater
{
    /// <summary>
    /// Thorn Spray Repeater — rapid-fire crystalline thorn repeater.
    /// Fires 12 thorns/s with widening spread. Thorn Accumulation stacks to 25 = detonation.
    /// Bloom Reload after 36 shots: heals 15 HP + petal burst + 6 enhanced Bloom Thorns.
    /// Precision Spray: crouching tightens spread and boosts velocity 20%.
    /// </summary>
    public class ThornSprayRepeater : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 52;
            Item.height = 24;
            Item.damage = 2400;
            Item.DamageType = DamageClass.Ranged;
            Item.useTime = 5;
            Item.useAnimation = 5;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 2f;
            Item.value = Item.sellPrice(platinum: 3, gold: 50);
            Item.rare = ModContent.RarityType<global::MagnumOpus.Common.OdeToJoyRarity>();
            Item.UseSound = SoundID.Item5;
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.crit = 12;
            Item.shoot = ModContent.ProjectileType<CrystallineThornProjectile>();
            Item.shootSpeed = 16f;
            Item.useAmmo = AmmoID.Arrow;
        }

        public override bool CanUseItem(Player player)
        {
            var sp = player.GetModPlayer<ThornSprayPlayer>();
            if (sp.TickBloomReload())
            {
                // During Bloom Reload: weapon is paused but we spawn pollen VFX
                if (sp.BloomReloadTimer == 55) // Near start of reload
                {
                    // Heal player
                    player.Heal(15);

                    // Pollen burst VFX
                    for (int i = 0; i < 20; i++)
                    {
                        Vector2 dustVel = Main.rand.NextVector2Circular(4f, 4f);
                        Dust d = Dust.NewDustDirect(player.Center - new Vector2(4), 8, 8, DustID.GoldFlame, dustVel.X, dustVel.Y, 150, ThornSprayTextures.BloomGold, 1.2f);
                        d.noGravity = true;
                        d.fadeIn = 1.5f;
                    }
                    // Blossom sparkle accents
                    for (int i = 0; i < 8; i++)
                    {
                        Vector2 dustVel = Main.rand.NextVector2Circular(6f, 6f);
                        Dust d = Dust.NewDustDirect(player.Center - new Vector2(4), 8, 8, DustID.YellowTorch, dustVel.X, dustVel.Y, 100, ThornSprayTextures.JubilantLight, 0.8f);
                        d.noGravity = true;
                    }
                }
                return false;
            }
            return base.CanUseItem(player);
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            var sp = player.GetModPlayer<ThornSprayPlayer>();
            bool isBloom = sp.RegisterShot();

            // Override arrow ammo to our thorn projectile
            int projType = isBloom
                ? ModContent.ProjectileType<BloomThornProjectile>()
                : ModContent.ProjectileType<CrystallineThornProjectile>();

            float spread = sp.GetSpreadAngle();
            float speedBonus = player.velocity.Length() < 0.5f ? 1.2f : 1f; // Precision Spray
            int bloomDamage = isBloom ? (int)(damage * 1.5f) : damage;

            Vector2 perturbedVel = velocity.RotatedByRandom(spread) * speedBonus;
            Projectile.NewProjectile(source, position, perturbedVel, projType, bloomDamage, knockback, player.whoAmI);

            return false;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Converts arrows into rapid-fire crystalline thorns at 12 shots per second"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Thorns stack Thorn Accumulation on enemies — at 25 stacks, all thorns detonate simultaneously"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "After 36 shots, enters Bloom Reload — heals 15 HP and next 6 shots deal 50% bonus damage"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Standing still tightens spread and increases thorn velocity by 20%"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'A thousand thorns. A thousand tiny joys. A thousand reasons to stay down.'")
            {
                OverrideColor = new Color(255, 200, 50)
            });
        }
    }
}