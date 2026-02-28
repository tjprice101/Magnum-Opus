using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.DiesIrae.Weapons.GrimoireOfCondemnation.Particles;
using MagnumOpus.Content.DiesIrae.Weapons.GrimoireOfCondemnation.Utilities;

namespace MagnumOpus.Content.DiesIrae.Weapons.GrimoireOfCondemnation
{
    /// <summary>
    /// Grimoire of Condemnation — Fires 3 spiraling blazing shards that chain lightning
    /// between each other when within range. Shards stick to enemies briefly then detonate.
    ///
    /// Stats: 1500 damage, 12 mana, 15 useTime, 4 KB, crit 15, Magic.
    /// Theme: Dies Irae — the words of condemnation made manifest.
    /// </summary>
    public class GrimoireOfCondemnation : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.damage = 1500;
            Item.DamageType = DamageClass.Magic;
            Item.mana = 12;
            Item.useTime = 15;
            Item.useAnimation = 15;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 4f;
            Item.value = Item.sellPrice(platinum: 1, gold: 50);
            Item.rare = ModContent.RarityType<global::MagnumOpus.Common.DiesIraeRarity>();
            Item.UseSound = SoundID.Item103;
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.staff[Item.type] = true;
            Item.shoot = ModContent.ProjectileType<Projectiles.BlazingShardProjectile>();
            Item.shootSpeed = 12f;
            Item.crit = 15;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // Fire 3 spiraling shards with slight spread
            for (int i = 0; i < 3; i++)
            {
                float spread = MathHelper.ToRadians(-10 + 10 * i);
                Vector2 vel = velocity.RotatedBy(spread) * Main.rand.NextFloat(0.95f, 1.05f);
                Projectile.NewProjectile(source, position, vel, type, damage, knockback, player.whoAmI, ai0: i);
            }

            // Grimoire casting VFX
            Vector2 castPos = position + velocity.SafeNormalize(Vector2.UnitX) * 40f;
            GrimoireParticleHandler.Spawn(new GrimoireImpactBloom(castPos, GrimoireUtils.CondemnOrange, 0.6f, 8));
            for (int i = 0; i < 4; i++)
            {
                Vector2 sparkVel = velocity.SafeNormalize(Vector2.UnitX).RotatedBy(Main.rand.NextFloat(-0.5f, 0.5f)) * Main.rand.NextFloat(2f, 4f);
                GrimoireParticleHandler.Spawn(new CursedShardTrailParticle(castPos, sparkVel,
                    GrimoireUtils.GetGrimoireColor(Main.rand.NextFloat()), 0.15f, 10));
            }

            return false;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Fires 3 spiraling cursed shards that chain lightning between them"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Shards embed in enemies, then detonate after a brief delay"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Each page condemns another soul to the pyre'")
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
