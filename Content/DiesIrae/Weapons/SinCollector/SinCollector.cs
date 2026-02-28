using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.DiesIrae.Weapons.SinCollector.Particles;
using MagnumOpus.Content.DiesIrae.Weapons.SinCollector.Utilities;

namespace MagnumOpus.Content.DiesIrae.Weapons.SinCollector
{
    /// <summary>
    /// Sin Collector — An infernal sniper rifle that fires sin-seeking rounds.
    /// On hit: chains lightning to 3 nearby enemies.
    /// Every 5th shot: spawns 3 spinning phantom cleaver copies.
    ///
    /// Stats: 2400 damage, 25 useTime, 8 KB, crit 35, uses Bullets.
    /// Theme: Dies Irae — each bullet claims another soul for judgment.
    /// </summary>
    public class SinCollector : ModItem
    {
        private int shotCounter = 0;

        public override void SetDefaults()
        {
            Item.width = 70;
            Item.height = 28;
            Item.damage = 2400;
            Item.DamageType = DamageClass.Ranged;
            Item.useTime = 25;
            Item.useAnimation = 25;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 8f;
            Item.value = Item.sellPrice(platinum: 2, gold: 50);
            Item.rare = ModContent.RarityType<global::MagnumOpus.Common.DiesIraeRarity>();
            Item.UseSound = SoundID.Item40 with { Pitch = -0.2f };
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<Projectiles.SinBulletProjectile>();
            Item.shootSpeed = 25f;
            Item.useAmmo = AmmoID.Bullet;
            Item.crit = 35;
        }

        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            type = ModContent.ProjectileType<Projectiles.SinBulletProjectile>();
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            shotCounter++;
            Vector2 muzzlePos = position + velocity.SafeNormalize(Vector2.UnitX) * 50f;

            // Fire main sin bullet
            Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);

            // Muzzle flash VFX
            SinParticleHandler.Spawn(new MuzzleFlashParticle(muzzlePos, velocity.ToRotation(), SinUtils.MuzzleGold, 1.2f, 6));

            for (int i = 0; i < 4; i++)
            {
                Vector2 vel = velocity.SafeNormalize(Vector2.UnitX).RotatedBy(Main.rand.NextFloat(-0.4f, 0.4f)) * Main.rand.NextFloat(2f, 5f);
                SinParticleHandler.Spawn(new SinBulletTrailParticle(muzzlePos, vel,
                    SinUtils.MulticolorLerp(Main.rand.NextFloat(), SinUtils.TrackingEmber, SinUtils.MuzzleGold), 0.2f, 12));
            }

            for (int i = 0; i < 3; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(2f, 2f);
                SinParticleHandler.Spawn(new SinSmokeParticle(muzzlePos, vel, 0.3f, 20));
            }

            // Every 5th shot — spawn 3 spinning cleaver copies
            if (shotCounter >= 5)
            {
                shotCounter = 0;
                SoundEngine.PlaySound(SoundID.Item71 with { Pitch = 0.3f, Volume = 0.7f }, position);

                for (int i = 0; i < 3; i++)
                {
                    float angle = velocity.ToRotation() + MathHelper.ToRadians(-45f + i * 45f);
                    Vector2 cleaverVel = angle.ToRotationVector2() * (velocity.Length() * 0.7f);

                    Projectile.NewProjectile(source, position, cleaverVel,
                        ModContent.ProjectileType<Projectiles.SpinningCleaverCopyProjectile>(), damage / 2, knockback / 2, player.whoAmI);
                }

                // Extra VFX for cleaver launch
                SinParticleHandler.Spawn(new SinImpactBloomParticle(muzzlePos, SinUtils.SinCrimson, 1.5f, 12));

                for (int i = 0; i < 4; i++)
                {
                    Vector2 vel = new Vector2(Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-2f, -0.5f));
                    SinParticleHandler.Spawn(new SinNoteParticle(muzzlePos, vel, SinUtils.MuzzleGold, 0.5f, 35));
                }
            }

            return false;
        }

        public override Vector2? HoldoutOffset() => new Vector2(-10f, 0f);

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Converts bullets into sin-seeking rounds"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "On hit, chains lightning to 3 nearby enemies"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Every 5th shot spawns 3 spinning phantom cleaver copies"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Each bullet claims another soul for judgment'")
            {
                OverrideColor = new Color(200, 50, 30)
            });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<Content.DiesIrae.ResonanceEnergies.ResonantCoreOfDiesIrae>(), 20)
                .AddIngredient(ModContent.ItemType<Content.DiesIrae.ResonanceEnergies.DiesIraeResonantEnergy>(), 15)
                .AddIngredient(ModContent.ItemType<Content.DiesIrae.HarmonicCores.HarmonicCoreOfDiesIrae>(), 2)
                .AddIngredient(ItemID.LunarBar, 15)
                .AddTile(ModContent.TileType<Content.Fate.CraftingStations.FatesCosmicAnvilTile>())
                .Register();
        }
    }
}
