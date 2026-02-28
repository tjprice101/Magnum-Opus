using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.DiesIrae.Weapons.DamnationsCannon.Particles;
using MagnumOpus.Content.DiesIrae.Weapons.DamnationsCannon.Utilities;

namespace MagnumOpus.Content.DiesIrae.Weapons.DamnationsCannon
{
    /// <summary>
    /// Damnation's Cannon — A heavy hellfire cannon that fires massive exploding wrath balls.
    /// On explosion: spawns 5 orbiting shrapnel pieces that seek enemies.
    ///
    /// Stats: 3500 damage, 45 useTime, 10 KB, crit 20, uses Rockets.
    /// Theme: Dies Irae — delivers damnation itself.
    /// </summary>
    public class DamnationsCannon : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 80;
            Item.height = 36;
            Item.damage = 3500;
            Item.DamageType = DamageClass.Ranged;
            Item.useTime = 45;
            Item.useAnimation = 45;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 10f;
            Item.value = Item.sellPrice(platinum: 2, gold: 50);
            Item.rare = ModContent.RarityType<global::MagnumOpus.Common.DiesIraeRarity>();
            Item.UseSound = SoundID.Item38 with { Pitch = -0.3f, Volume = 1.2f };
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<Projectiles.IgnitedWrathBallProjectile>();
            Item.shootSpeed = 14f;
            Item.useAmmo = AmmoID.Rocket;
            Item.crit = 20;
        }

        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            type = ModContent.ProjectileType<Projectiles.IgnitedWrathBallProjectile>();
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Vector2 muzzlePos = position + velocity.SafeNormalize(Vector2.UnitX) * 60f;
            Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);

            // Massive muzzle flash
            DamnationParticleHandler.Spawn(new CannonFlashParticle(muzzlePos, velocity.ToRotation(), DamnationUtils.ExplosionGold, 2f, 8));

            // Heavy smoke clouds
            for (int i = 0; i < 6; i++)
            {
                Vector2 smokeVel = velocity.SafeNormalize(Vector2.UnitX).RotatedBy(Main.rand.NextFloat(-0.8f, 0.8f)) * Main.rand.NextFloat(2f, 5f);
                DamnationParticleHandler.Spawn(new CannonSmokeParticle(muzzlePos, smokeVel, 0.6f, 35));
            }

            // Ember sparks
            for (int i = 0; i < 8; i++)
            {
                Vector2 sparkVel = velocity.SafeNormalize(Vector2.UnitX).RotatedBy(Main.rand.NextFloat(-0.6f, 0.6f)) * Main.rand.NextFloat(3f, 7f);
                Color c = DamnationUtils.GetDamnationColor(Main.rand.NextFloat());
                DamnationParticleHandler.Spawn(new ShrapnelSparkParticle(muzzlePos, sparkVel, c, 0.2f, 15));
            }

            // Music notes
            for (int i = 0; i < 4; i++)
            {
                Vector2 noteVel = Main.rand.NextVector2Circular(3f, 3f);
                DamnationParticleHandler.Spawn(new DamnationNoteParticle(muzzlePos, noteVel,
                    DamnationUtils.MulticolorLerp(Main.rand.NextFloat(), DamnationUtils.DamnationRed, DamnationUtils.ExplosionGold), 0.5f, 40));
            }

            SoundEngine.PlaySound(SoundID.DD2_BetsyFireballShot with { Volume = 0.8f }, position);

            return false;
        }

        public override Vector2? HoldoutOffset() => new Vector2(-14f, 2f);

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Fires massive exploding balls of wrath"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "On explosion, spawns 5 orbiting shrapnel that seek enemies"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Direct hits cause devastating hellfire explosions"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The cannon that delivers damnation itself'")
            {
                OverrideColor = new Color(200, 50, 30)
            });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<Content.DiesIrae.ResonanceEnergies.ResonantCoreOfDiesIrae>(), 25)
                .AddIngredient(ModContent.ItemType<Content.DiesIrae.ResonanceEnergies.DiesIraeResonantEnergy>(), 20)
                .AddIngredient(ModContent.ItemType<Content.DiesIrae.HarmonicCores.HarmonicCoreOfDiesIrae>(), 3)
                .AddIngredient(ItemID.LunarBar, 20)
                .AddTile(ModContent.TileType<Content.Fate.CraftingStations.FatesCosmicAnvilTile>())
                .Register();
        }
    }
}
