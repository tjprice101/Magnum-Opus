using MagnumOpus.Common;
using MagnumOpus.Content.OdeToJoy.Weapons.AnthemOfGlory.Projectiles;
using MagnumOpus.Content.OdeToJoy.Weapons.AnthemOfGlory.Utilities;
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

namespace MagnumOpus.Content.OdeToJoy.Weapons.AnthemOfGlory
{
    /// <summary>
    /// Anthem of Glory — Channeled golden beam weapon (LaserFoundation primary).
    /// Hold to fire a continuous beam that scales with Crescendo (1x→2x over 5s).
    /// Spawns Glory Notes every 2s during channel. Victory Fanfare on 3+ kills.
    /// </summary>
    public class AnthemOfGlory : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 42;
            Item.height = 42;
            Item.damage = 2800;
            Item.DamageType = DamageClass.Magic;
            Item.useTime = 6;
            Item.useAnimation = 6;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 3f;
            Item.value = Item.sellPrice(platinum: 3, gold: 50);
            Item.rare = ModContent.RarityType<global::MagnumOpus.Common.OdeToJoyRarity>();
            Item.UseSound = SoundID.Item21;
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.channel = true;
            Item.mana = 15;
            Item.crit = 12;
            Item.shoot = ModContent.ProjectileType<AnthemBeamProjectile>();
            Item.shootSpeed = 1f; // Beam doesn't need speed
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // Only spawn one beam — it persists while channeling
            bool beamExists = false;
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                if (Main.projectile[i].active && Main.projectile[i].owner == player.whoAmI &&
                    Main.projectile[i].type == ModContent.ProjectileType<AnthemBeamProjectile>())
                {
                    beamExists = true;
                    break;
                }
            }

            if (!beamExists)
            {
                Vector2 dir = (Main.MouseWorld - player.Center).SafeNormalize(Vector2.UnitX);
                Projectile.NewProjectile(source, player.Center, dir, type, damage, knockback, player.whoAmI);
            }

            return false;
        }

        public override void HoldItem(Player player)
        {
            // Crescendo aura VFX while channeling
            AnthemPlayer ap = player.GetModPlayer<AnthemPlayer>();
            if (ap.IsChanneling && ap.CrescendoProgress > 0.1f)
            {
                // Golden aura particles around caster — intensity scales with crescendo
                if (Main.rand.NextBool((int)Math.Max(1, 6 - ap.CrescendoProgress * 4)))
                {
                    float angle = Main.rand.NextFloat() * MathHelper.TwoPi;
                    float dist = 30f + 20f * ap.CrescendoProgress;
                    Vector2 pos = player.Center + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * dist;
                    Dust d = Dust.NewDustDirect(pos, 1, 1, DustID.GoldFlame, 0f, -0.5f, 120, AnthemTextures.BloomGold, 0.5f + ap.CrescendoProgress * 0.4f);
                    d.noGravity = true;
                    d.fadeIn = 1.2f;
                }
            }
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
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Hold to channel a continuous golden beam that grows in power"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Crescendo: beam damage and width scale from 1x to 2x over 5 seconds"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Spawns homing Glory Notes every 2 seconds while channeling"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Victory Fanfare: 3 kills during a channel triggers a golden shockwave"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Let every note ring with triumph — for this is the anthem that crowns the victorious'")
            {
                OverrideColor = new Color(255, 200, 50)
            });
        }
    }
}