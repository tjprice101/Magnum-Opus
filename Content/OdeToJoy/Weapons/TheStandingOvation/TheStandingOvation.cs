using MagnumOpus.Common;
using MagnumOpus.Content.OdeToJoy.Weapons.TheStandingOvation.Buffs;
using MagnumOpus.Content.OdeToJoy.Weapons.TheStandingOvation.Projectiles;
using MagnumOpus.Content.OdeToJoy.Weapons.TheStandingOvation.Utilities;
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

namespace MagnumOpus.Content.OdeToJoy.Weapons.TheStandingOvation
{
    /// <summary>
    /// The Standing Ovation — Summon weapon.
    /// Summons phantom spectator minions that attack with applause waves,
    /// thrown roses, and standing rushes. Ovation Meter → Standing Ovation Event.
    /// +5% damage per additional crowd member. Cross-summon sync with Triumphant Chorus.
    /// Encore: re-summon within 5s of event = 2 minions for 1 slot.
    /// </summary>
    public class TheStandingOvation : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 46;
            Item.height = 46;
            Item.damage = 2600;
            Item.DamageType = DamageClass.Summon;
            Item.mana = 20;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 3f;
            Item.crit = 4;
            Item.value = Item.sellPrice(platinum: 4);
            Item.rare = ModContent.RarityType<global::MagnumOpus.Common.OdeToJoyRarity>();
            Item.UseSound = SoundID.Item44;
            Item.autoReuse = false;
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<StandingOvationMinion>();
            Item.shootSpeed = 10f;
            Item.buffType = ModContent.BuffType<StandingOvationBuff>();
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            player.AddBuff(Item.buffType, 2);

            OvationPlayer op = player.GetModPlayer<OvationPlayer>();

            // Encore bonus: re-summon during encore window spawns 2 minions for cost of 1
            int count = op.EncoreReady ? 2 : 1;
            for (int i = 0; i < count; i++)
            {
                Vector2 spawnVel = velocity.RotatedByRandom(MathHelper.ToRadians(15f));
                Projectile.NewProjectile(source, position, spawnVel, type, damage, knockback, player.whoAmI);
            }

            // Summoning VFX burst — golden applause sparkles
            for (int i = 0; i < 20; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(4f, 4f);
                Dust d = Dust.NewDustDirect(position, 1, 1, DustID.GoldFlame, vel.X, vel.Y, 80,
                    OvationTextures.ApplauseFlash, 0.8f);
                d.noGravity = true;
                d.fadeIn = 1.2f;
            }

            return false;
        }

        public override void HoldItem(Player player)
        {
            // Ovation Meter ambient VFX — golden particles proportional to meter
            OvationPlayer op = player.GetModPlayer<OvationPlayer>();
            if (op.OvationMeter > 30f && Main.rand.NextFloat() < op.OvationMeter / 200f)
            {
                Vector2 offset = Main.rand.NextVector2Circular(20f, 20f);
                Dust d = Dust.NewDustDirect(player.Center + offset - new Vector2(2), 4, 4, DustID.GoldFlame,
                    0f, -1f, 100, OvationTextures.BloomGold * (op.OvationMeter / 100f), 0.5f);
                d.noGravity = true;
            }
        }

        public override void AddRecipes()
        {
            CreateRecipe()
            .AddIngredient(ModContent.ItemType<ResonantCoreOfOdeToJoy>(), 25)
            .AddIngredient(ModContent.ItemType<OdeToJoyResonantEnergy>(), 20)
            .AddIngredient(ModContent.ItemType<HarmonicCoreOfOdeToJoy>(), 3)
            .AddIngredient(ItemID.LunarBar, 20)
            .AddTile(ModContent.TileType<FatesCosmicAnvilTile>())
            .Register();
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Summons phantom spectators that attack with applause waves, roses, and charges"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Each additional spectator adds +5% crowd damage bonus"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Kills build Ovation Meter toward a devastating Standing Ovation Event"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Re-summon during Encore window to summon 2 spectators for 1"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The audience loved the performance. The audience demands an encore.'")
            {
                OverrideColor = new Color(255, 200, 50)
            });
        }
    }
}