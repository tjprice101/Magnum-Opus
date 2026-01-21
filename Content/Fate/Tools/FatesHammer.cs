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
    /// Fate's Hammer - A cosmic hammer that shatters the boundaries of worlds.
    /// Features celestial particle effects with glyphs and star sparkles.
    /// Highest tier hammer in the mod.
    /// </summary>
    public class FatesHammer : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.damage = 210; // Higher than Enigma
            Item.DamageType = DamageClass.Melee;
            Item.width = 50;
            Item.height = 50;
            Item.useTime = 4;
            Item.useAnimation = 8;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 12f;
            Item.value = Item.sellPrice(gold: 55);
            Item.rare = ModContent.RarityType<FateRarity>();
            Item.UseSound = SoundID.Item15 with { Pitch = -0.5f, Volume = 0.9f };
            Item.autoReuse = true;
            Item.useTurn = true;

            // Highest hammer power - Tier 6
            Item.hammer = 180; // Highest tier: above Swan Lake (165)
            
            Item.maxStack = 1;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<ResonantCoreOfFate>(), 15)
                .AddIngredient(ModContent.ItemType<FateResonantEnergy>(), 12)
                .AddIngredient(ModContent.ItemType<HarmonicCoreOfFate>(), 1)
                .AddIngredient(ItemID.LunarBar, 8)
                .AddTile(ModContent.TileType<MoonlightAnvilTile>())
                .Register();
        }

        public override void MeleeEffects(Player player, Rectangle hitbox)
        {
            // Shockwave cosmic particles
            if (Main.rand.NextBool(2))
            {
                Dust dust = Dust.NewDustDirect(new Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height,
                    DustID.Clentaminator_Cyan, player.velocity.X * 0.15f, player.velocity.Y * 0.15f, 150, 
                    FateCosmicVFX.FateCyan, 1.5f);
                dust.noGravity = true;
                dust.velocity *= 1.2f;
            }

            // Deep purple cosmic mist
            if (Main.rand.NextBool(3))
            {
                Dust mist = Dust.NewDustDirect(new Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height,
                    DustID.PurpleTorch, 0f, -0.4f, 120, FateCosmicVFX.FatePurple, 1.3f);
                mist.noGravity = true;
                mist.velocity = Main.rand.NextVector2Circular(1.8f, 1.8f);
            }

            // Reality-shattering glyphs
            if (Main.rand.NextBool(5))
            {
                Vector2 glyphPos = hitbox.Center.ToVector2() + Main.rand.NextVector2Circular(20f, 20f);
                CustomParticles.Glyph(glyphPos, FateCosmicVFX.FateBrightRed * 0.6f, 0.32f, -1);
            }

            // Star explosion particles
            if (Main.rand.NextBool(4))
            {
                Vector2 starPos = hitbox.Center.ToVector2() + Main.rand.NextVector2Circular(16f, 16f);
                var star = new GenericGlowParticle(starPos, Main.rand.NextVector2Circular(2f, 2f), 
                    FateCosmicVFX.FateWhite, 0.25f, 16, true);
                MagnumParticleHandler.SpawnParticle(star);
            }

            // Cosmic cloud burst on powerful swings
            if (Main.rand.NextBool(7))
            {
                FateCosmicVFX.SpawnCosmicCloudTrail(hitbox.Center.ToVector2(), 
                    Main.rand.NextVector2Circular(1f, 1f), 0.4f);
            }

            // Impact halo ring occasionally
            if (Main.rand.NextBool(12))
            {
                CustomParticles.HaloRing(hitbox.Center.ToVector2(), FateCosmicVFX.FateDarkPink * 0.5f, 0.25f, 12);
            }

            // Dynamic lighting - brighter for hammer
            Lighting.AddLight(hitbox.Center.ToVector2(), FateCosmicVFX.FateCyan.ToVector3() * 0.55f);
        }
    }
}
