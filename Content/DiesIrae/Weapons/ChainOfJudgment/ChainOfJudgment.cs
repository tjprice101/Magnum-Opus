using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.DiesIrae.Weapons.ChainOfJudgment.Particles;
using MagnumOpus.Content.DiesIrae.Weapons.ChainOfJudgment.Utilities;

namespace MagnumOpus.Content.DiesIrae.Weapons.ChainOfJudgment
{
    /// <summary>
    /// Chain of Judgment — A blazing spectral chain whip that bounces between enemies.
    /// Melee weapon, noMelee/noUseGraphic. Throws a chain that ricochets 4 times, 
    /// exploding at each bounce point, then returns.
    ///
    /// Stats: 2400 damage, 22 useTime, 6 KB, crit 15.
    /// Theme: Dies Irae — chains that bind the damned.
    /// </summary>
    public class ChainOfJudgment : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 50;
            Item.height = 50;
            Item.damage = 2400;
            Item.DamageType = DamageClass.Melee;
            Item.useTime = 22;
            Item.useAnimation = 22;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 6f;
            Item.value = Item.sellPrice(platinum: 2, gold: 50);
            Item.rare = ModContent.RarityType<global::MagnumOpus.Common.DiesIraeRarity>();
            Item.UseSound = SoundID.Item153;
            Item.autoReuse = true;
            Item.useTurn = false;
            Item.scale = 1.3f;
            Item.crit = 15;
            Item.shoot = ModContent.ProjectileType<Projectiles.JudgmentChainProjectile>();
            Item.shootSpeed = 16f;
            Item.noMelee = true;
            Item.noUseGraphic = true;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Vector2 mouseDir = (Main.MouseWorld - player.Center).SafeNormalize(Vector2.UnitX);
            Projectile.NewProjectile(source, player.Center, mouseDir * Item.shootSpeed,
                ModContent.ProjectileType<Projectiles.JudgmentChainProjectile>(), damage, knockback, player.whoAmI);

            // Launch VFX
            Vector2 launchPos = player.Center + mouseDir * 30f;
            ChainParticleHandler.Spawn(new ChainBloomParticle(launchPos, ChainUtils.MoltenLink, 1f, 12));

            for (int i = 0; i < 6; i++)
            {
                Vector2 vel = mouseDir.RotatedBy(Main.rand.NextFloat(-0.6f, 0.6f)) * Main.rand.NextFloat(2f, 5f);
                ChainParticleHandler.Spawn(new ChainSparkParticle(launchPos, vel,
                    ChainUtils.MulticolorLerp(Main.rand.NextFloat(), ChainUtils.MoltenLink, ChainUtils.HellfireChain),
                    0.2f, 15));
            }

            for (int i = 0; i < 3; i++)
            {
                Vector2 vel = mouseDir.RotatedBy(Main.rand.NextFloat(-0.5f, 0.5f)) * 3f;
                ChainParticleHandler.Spawn(new ChainNoteParticle(launchPos, vel, ChainUtils.HellfireChain, 0.4f, 35));
            }

            SoundEngine.PlaySound(SoundID.Item153 with { Volume = 0.8f }, player.Center);

            return false;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Throws a blazing spectral chain that spins and ricochets"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Bounces up to 4 times between enemies, exploding on each hit"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Returns to you after bouncing, ignites struck enemies"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The chains that bind the damned to their fate'")
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
