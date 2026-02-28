using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.DiesIrae.Weapons.ArbitersSentence.Particles;
using MagnumOpus.Content.DiesIrae.Weapons.ArbitersSentence.Utilities;

namespace MagnumOpus.Content.DiesIrae.Weapons.ArbitersSentence
{
    /// <summary>
    /// Arbiter's Sentence — A flamethrower that unleashes a stream of judgment fire.
    /// Rapid-fire (3 useTime), uses Gel.
    /// Every 15 ticks drops a lingering Purgatory Ember AOE.
    /// Fires scattered music notes along the flame stream.
    ///
    /// Stats: 850 damage, 3 useTime/9 useAnimation, 1 KB, crit 15, uses Gel.
    /// Theme: Dies Irae — the sentence is pronounced in fire.
    /// </summary>
    public class ArbitersSentence : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 64;
            Item.height = 24;
            Item.damage = 850;
            Item.DamageType = DamageClass.Ranged;
            Item.useTime = 3;
            Item.useAnimation = 9;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 1f;
            Item.value = Item.sellPrice(platinum: 1, gold: 50);
            Item.rare = ModContent.RarityType<global::MagnumOpus.Common.DiesIraeRarity>();
            Item.UseSound = SoundID.Item34 with { Pitch = 0.15f, Volume = 0.6f };
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<Projectiles.JudgmentFlameProjectile>();
            Item.shootSpeed = 11f;
            Item.useAmmo = AmmoID.Gel;
            Item.crit = 15;
        }

        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            type = ModContent.ProjectileType<Projectiles.JudgmentFlameProjectile>();
            velocity = velocity.RotatedByRandom(MathHelper.ToRadians(8));
            velocity *= Main.rand.NextFloat(0.9f, 1.1f);
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Vector2 muzzlePos = position + velocity.SafeNormalize(Vector2.UnitX) * 40f;

            Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);

            // Nozzle flash (every shot)
            ArbiterParticleHandler.Spawn(new NozzleFlashParticle(muzzlePos, velocity.ToRotation(),
                ArbiterUtils.PurgatoryGold, 1f, 4));

            // Occasional music notes in the fire stream
            if (Main.rand.NextBool(5))
            {
                Vector2 noteVel = velocity.SafeNormalize(Vector2.UnitX) * Main.rand.NextFloat(2f, 5f) + Main.rand.NextVector2Circular(1f, 1f);
                ArbiterParticleHandler.Spawn(new ArbiterNoteParticle(muzzlePos, noteVel,
                    ArbiterUtils.GetFlameColor(Main.rand.NextFloat(0.3f, 0.8f)), 0.35f, 30));
            }

            return false;
        }

        public override Vector2? HoldoutOffset() => new Vector2(-10f, 0f);

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Unleashes a continuous stream of judgment fire"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Leaves lingering purgatory embers that burn enemies"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The sentence is written in fire, and none may appeal'")
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
