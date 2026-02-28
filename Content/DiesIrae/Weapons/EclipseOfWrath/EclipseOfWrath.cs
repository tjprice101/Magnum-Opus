using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;
using MagnumOpus.Content.DiesIrae.ResonanceEnergies;
using MagnumOpus.Content.DiesIrae.HarmonicCores;
using MagnumOpus.Content.Fate.CraftingStations;
using MagnumOpus.Content.DiesIrae.Weapons.EclipseOfWrath.Projectiles;
using MagnumOpus.Content.DiesIrae.Weapons.EclipseOfWrath.Particles;
using MagnumOpus.Content.DiesIrae.Weapons.EclipseOfWrath.Utilities;

namespace MagnumOpus.Content.DiesIrae.Weapons.EclipseOfWrath
{
    /// <summary>
    /// Eclipse of Wrath — A dark sun made manifest.
    /// 
    /// MECHANICS (preserved):
    /// - 1750 damage, Magic, 25 mana, 35 useTime, 8 KB, crit 22
    /// - Throws an eclipse orb that tracks cursor
    /// - While airborne, spawns blazing wrath shards that seek enemies
    /// - Explodes on impact or tile collision
    /// 
    /// NEW VFX SYSTEM:
    /// - Self-contained particle system (corona flares, eclipse smoke, solar blooms, wrath embers)
    /// - GPU primitive trail for orb and shards
    /// - 5-layer corona glow rendering (umbra → inner corona → outer → gold → white)
    /// - Orbiting spark points with solar gradient
    /// - Dedicated EclipseOrb shader
    /// </summary>
    public class EclipseOfWrath : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 44;
            Item.height = 44;
            Item.damage = 1750;
            Item.DamageType = DamageClass.Magic;
            Item.mana = 25;
            Item.useTime = 35;
            Item.useAnimation = 35;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 8f;
            Item.value = Item.sellPrice(platinum: 2, gold: 50);
            Item.rare = ModContent.RarityType<DiesIraeRarity>();
            Item.UseSound = SoundID.Item73 with { Pitch = -0.2f };
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.shoot = ModContent.ProjectileType<EclipseOrbProjectile>();
            Item.shootSpeed = 12f;
            Item.crit = 22;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Vector2 launchPos = player.Center;
            Vector2 launchVel = (Main.MouseWorld - player.Center).SafeNormalize(Vector2.UnitX) * Item.shootSpeed;

            Projectile.NewProjectile(source, launchPos, launchVel, type, damage, knockback, player.whoAmI);

            // Launch VFX — solar bloom + corona flares
            EclipseParticleHandler.SpawnParticle(new SolarBloomParticle(launchPos, EclipseUtils.OuterCorona, 1.5f, 18));

            for (int i = 0; i < 6; i++)
            {
                Vector2 noteVel = launchVel.SafeNormalize(Vector2.UnitX).RotatedBy(Main.rand.NextFloat(-0.8f, 0.8f)) * 4f;
                EclipseParticleHandler.SpawnParticle(new EclipseNoteParticle(
                    launchPos, noteVel, EclipseUtils.SolarGold, 0.5f, 35));
            }

            for (int i = 0; i < 4; i++)
            {
                EclipseParticleHandler.SpawnParticle(new CoronaFlareParticle(
                    launchPos, Main.rand.NextVector2Circular(3f, 3f),
                    EclipseUtils.CoronaLerp(Main.rand.NextFloat()), 0.4f, 20));
            }

            SoundEngine.PlaySound(SoundID.DD2_BetsyFireballShot with { Volume = 0.9f }, player.Center);

            return false;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Throws a dark eclipse orb that tracks your cursor"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "While airborne, spawns blazing wrath shards that seek enemies"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Explodes on impact with enemies or tiles"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The sun's wrath made manifest'")
            {
                OverrideColor = new Color(200, 50, 30) // Dies Irae blood red
            });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<ResonantCoreOfDiesIrae>(), 20)
                .AddIngredient(ModContent.ItemType<DiesIraeResonantEnergy>(), 15)
                .AddIngredient(ModContent.ItemType<HarmonicCoreOfDiesIrae>(), 2)
                .AddIngredient(ItemID.LunarBar, 15)
                .AddTile(ModContent.TileType<FatesCosmicAnvilTile>())
                .Register();
        }
    }
}
