using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
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
    /// Fate's Axe - A cosmic axe that cleaves through the fabric of existence.
    /// Features celestial particle effects with glyphs and star sparkles.
    /// Highest tier axe in the mod.
    /// </summary>
    public class FatesAxe : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.damage = 200; // Higher than Enigma
            Item.DamageType = DamageClass.Melee;
            Item.width = 48;
            Item.height = 48;
            Item.useTime = 4;
            Item.useAnimation = 8;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 8f;
            Item.value = Item.sellPrice(gold: 55);
            Item.rare = ModContent.RarityType<FateRarity>();
            Item.UseSound = SoundID.Item15 with { Pitch = -0.3f, Volume = 0.8f };
            Item.autoReuse = true;
            Item.useTurn = true;

            // Highest axe power - Tier 6
            Item.axe = 65; // 325% axe power - highest tier, above Swan Lake (300%)
            
            Item.maxStack = 1;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Each swing cleaves through the threads of destiny'") { OverrideColor = new Color(180, 40, 80) });
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
            // Cosmic crimson sparks
            if (Main.rand.NextBool(2))
            {
                Dust dust = Dust.NewDustDirect(new Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height,
                    DustID.FireworkFountain_Red, player.velocity.X * 0.2f, player.velocity.Y * 0.2f, 150, 
                    FateCosmicVFX.FateBrightRed, 1.4f);
                dust.noGravity = true;
                dust.velocity *= 1.5f;
            }

            // Purple cosmic energy
            if (Main.rand.NextBool(3))
            {
                Dust mist = Dust.NewDustDirect(new Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height,
                    DustID.PurpleTorch, 0f, -0.3f, 120, FateCosmicVFX.FatePurple, 1.1f);
                mist.noGravity = true;
                mist.velocity = Main.rand.NextVector2Circular(2.5f, 2.5f);
            }

            // Glyphs cleaved into existence
            if (Main.rand.NextBool(6))
            {
                Vector2 glyphPos = hitbox.Center.ToVector2() + Main.rand.NextVector2Circular(18f, 18f);
                CustomParticles.Glyph(glyphPos, FateCosmicVFX.FateDarkPink * 0.7f, 0.28f, -1);
            }

            // Star sparks on swing
            if (Main.rand.NextBool(4))
            {
                Vector2 sparkPos = hitbox.Center.ToVector2() + Main.rand.NextVector2Circular(14f, 14f);
                var spark = new GlowSparkParticle(sparkPos, Main.rand.NextVector2Circular(2f, 2f), 
                    FateCosmicVFX.FateStarGold, 0.2f, 12);
                MagnumParticleHandler.SpawnParticle(spark);
            }

            // Cosmic flare burst on hit frames
            if (Main.rand.NextBool(8))
            {
                CustomParticles.GenericFlare(hitbox.Center.ToVector2(), FateCosmicVFX.FateWhite * 0.6f, 0.3f, 10);
            }

            // Dynamic lighting
            Lighting.AddLight(hitbox.Center.ToVector2(), FateCosmicVFX.FateBrightRed.ToVector3() * 0.45f);
        }
    }
}
