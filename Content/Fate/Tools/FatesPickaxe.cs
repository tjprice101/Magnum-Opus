using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.Fate.ResonanceEnergies;
using MagnumOpus.Content.Fate.HarmonicCores;
using MagnumOpus.Content.MoonlightSonata.CraftingStations;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common;

namespace MagnumOpus.Content.Fate.Tools
{
    /// <summary>
    /// Fate's Pickaxe - A cosmic pickaxe that tears through reality itself.
    /// Features celestial particle effects with glyphs and star sparkles.
    /// Highest tier tool in the mod.
    /// </summary>
    public class FatesPickaxe : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.damage = 220; // Higher than Enigma (195)
            Item.DamageType = DamageClass.Melee;
            Item.width = 44;
            Item.height = 44;
            Item.useTime = 2;
            Item.useAnimation = 4;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 9f;
            Item.value = Item.sellPrice(gold: 60);
            Item.rare = ModContent.RarityType<FateRarity>();
            Item.UseSound = SoundID.Item15 with { Pitch = -0.4f, Volume = 0.8f };
            Item.autoReuse = true;
            Item.useTurn = true;

            // Highest pickaxe power - can mine anything
            Item.pick = 550;
            
            Item.maxStack = 1;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<ResonantCoreOfFate>(), 20)
                .AddIngredient(ModContent.ItemType<FateResonantEnergy>(), 15)
                .AddIngredient(ModContent.ItemType<HarmonicCoreOfFate>(), 2)
                .AddIngredient(ItemID.LunarBar, 10)
                .AddTile(ModContent.TileType<MoonlightAnvilTile>())
                .Register();
        }

        public override void MeleeEffects(Player player, Rectangle hitbox)
        {
            // Cosmic star particles
            if (Main.rand.NextBool(2))
            {
                Dust dust = Dust.NewDustDirect(new Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height,
                    DustID.PinkFairy, player.velocity.X * 0.2f, player.velocity.Y * 0.2f, 150, 
                    FateCosmicVFX.FateWhite, 1.3f);
                dust.noGravity = true;
                dust.velocity *= 1.4f;
            }

            // Dark pink cosmic mist
            if (Main.rand.NextBool(3))
            {
                Dust mist = Dust.NewDustDirect(new Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height,
                    DustID.PinkTorch, 0f, -0.5f, 120, FateCosmicVFX.FateDarkPink, 1.2f);
                mist.noGravity = true;
                mist.velocity = Main.rand.NextVector2Circular(2f, 2f);
            }

            // Orbiting glyphs during swing
            if (Main.rand.NextBool(8))
            {
                Vector2 glyphPos = hitbox.Center.ToVector2() + Main.rand.NextVector2Circular(15f, 15f);
                CustomParticles.Glyph(glyphPos, FateCosmicVFX.FatePurple * 0.6f, 0.25f, -1);
            }

            // Star sparkles
            if (Main.rand.NextBool(5))
            {
                Vector2 starPos = hitbox.Center.ToVector2() + Main.rand.NextVector2Circular(12f, 12f);
                var star = new GenericGlowParticle(starPos, Main.rand.NextVector2Circular(1f, 1f), 
                    FateCosmicVFX.FateWhite, 0.2f, 15, true);
                MagnumParticleHandler.SpawnParticle(star);
            }

            // Cosmic cloud trail
            if (Main.rand.NextBool(10))
            {
                FateCosmicVFX.SpawnCosmicCloudTrail(hitbox.Center.ToVector2(), player.velocity * 0.2f, 0.3f);
            }

            // Dynamic lighting
            Lighting.AddLight(hitbox.Center.ToVector2(), FateCosmicVFX.FateDarkPink.ToVector3() * 0.5f);
        }
    }
}
