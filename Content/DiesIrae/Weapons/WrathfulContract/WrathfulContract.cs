using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.DiesIrae.Weapons.WrathfulContract.Particles;
using MagnumOpus.Content.DiesIrae.Weapons.WrathfulContract.Utilities;

namespace MagnumOpus.Content.DiesIrae.Weapons.WrathfulContract
{
    /// <summary>
    /// Wrathful Contract — Summons a wrathful demon entity that orbits the player
    /// and dashes at enemies. Every 3rd dash triggers a burst of 6 wrath fireballs.
    ///
    /// Stats: 1650 damage, 40 mana, 35 useTime, 6 KB, Summon, 2 slots.
    /// Theme: Dies Irae — a contract signed in hellfire and fury.
    /// </summary>
    public class WrathfulContract : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 40;
            Item.height = 40;
            Item.damage = 1650;
            Item.DamageType = DamageClass.Summon;
            Item.mana = 40;
            Item.useTime = 35;
            Item.useAnimation = 35;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 6f;
            Item.value = Item.sellPrice(platinum: 2, gold: 25);
            Item.rare = ModContent.RarityType<global::MagnumOpus.Common.DiesIraeRarity>();
            Item.UseSound = SoundID.Item82;
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<Projectiles.WrathDemonMinion>();
            Item.shootSpeed = 0f;
            Item.buffType = ModContent.BuffType<Buffs.WrathfulContractBuff>();
            Item.buffTime = 18000;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            player.AddBuff(Item.buffType, 18000);
            player.SpawnMinionOnCursor(source, player.whoAmI, type, damage, knockback);

            // Summoning VFX — demonic contract flash
            ContractParticleHandler.Spawn(new DemonAuraParticle(Main.MouseWorld, ContractUtils.WrathFlame, 1.2f, 15));
            for (int i = 0; i < 10; i++)
            {
                float angle = MathHelper.TwoPi * i / 10f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 5f);
                ContractParticleHandler.Spawn(new DashTrailParticle(Main.MouseWorld, vel,
                    ContractUtils.GetContractColor(Main.rand.NextFloat()), 0.15f, 15));
            }
            for (int i = 0; i < 3; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(2f, 2f);
                ContractParticleHandler.Spawn(new ContractNoteParticle(Main.MouseWorld, vel,
                    ContractUtils.DemonCrimson, 0.45f, 35));
            }

            return false;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Summons a wrathful demon that orbits and dashes at enemies"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Every 3rd dash unleashes a burst of 6 homing wrath fireballs"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Costs 2 minion slots"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The contract is sealed in hellfire — there is no breaking it'")
            {
                OverrideColor = new Color(200, 50, 30)
            });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<Content.DiesIrae.ResonanceEnergies.ResonantCoreOfDiesIrae>(), 15)
                .AddIngredient(ModContent.ItemType<Content.DiesIrae.ResonanceEnergies.DiesIraeResonantEnergy>(), 35)
                .AddIngredient(ModContent.ItemType<Content.DiesIrae.HarmonicCores.HarmonicCoreOfDiesIrae>(), 3)
                .AddIngredient(ItemID.LunarBar, 25)
                .AddTile(ModContent.TileType<Content.Fate.CraftingStations.FatesCosmicAnvilTile>())
                .Register();
        }
    }
}
