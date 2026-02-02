using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.Nachtmusik.ResonanceEnergies;
using MagnumOpus.Content.Nachtmusik.HarmonicCores;
using MagnumOpus.Content.Fate.CraftingStations;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common;

namespace MagnumOpus.Content.Nachtmusik.Tools
{
    /// <summary>
    /// Night's Melody Pickaxe - POST-FATE ULTIMATE tier pickaxe.
    /// Superior to Fate's pickaxe with celestial starlight effects.
    /// </summary>
    public class NightsMelodyPickaxe : ModItem
    {
        // Nachtmusik colors
        private static readonly Color DeepPurple = new Color(45, 27, 78);
        private static readonly Color Gold = new Color(255, 215, 0);
        private static readonly Color Violet = new Color(123, 104, 238);
        private static readonly Color StarWhite = new Color(255, 255, 255);

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.damage = 280; // Higher than Fate (220)
            Item.DamageType = DamageClass.Melee;
            Item.width = 44;
            Item.height = 44;
            Item.useTime = 2;
            Item.useAnimation = 3; // Faster than Fate
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 10f;
            Item.value = Item.sellPrice(platinum: 1);
            Item.rare = ModContent.RarityType<NachtmusikRarity>();
            Item.UseSound = SoundID.Item15 with { Pitch = 0.2f, Volume = 0.9f };
            Item.autoReuse = true;
            Item.useTurn = true;

            // POST-FATE ULTIMATE pickaxe power
            Item.pick = 650; // Higher than Fate (550)
            
            Item.maxStack = 1;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect", "650% pickaxe power") { OverrideColor = Gold });
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The stars themselves yield to your melody'") { OverrideColor = Violet });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<NachtmusikResonantCore>(), 25)
                .AddIngredient(ModContent.ItemType<NachtmusikResonantEnergy>(), 20)
                .AddIngredient(ModContent.ItemType<HarmonicCoreOfNachtmusik>(), 3)
                .AddIngredient(ItemID.LunarBar, 15)
                .AddTile(ModContent.TileType<FatesCosmicAnvilTile>())
                .Register();
        }

        public override void MeleeEffects(Player player, Rectangle hitbox)
        {
            // Celestial star particles
            if (Main.rand.NextBool(2))
            {
                Dust dust = Dust.NewDustDirect(new Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height,
                    DustID.PurpleTorch, player.velocity.X * 0.2f, player.velocity.Y * 0.2f, 150, 
                    Violet, 1.4f);
                dust.noGravity = true;
                dust.velocity *= 1.5f;
            }

            // Golden starlight
            if (Main.rand.NextBool(3))
            {
                Dust gold = Dust.NewDustDirect(new Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height,
                    DustID.GoldFlame, 0f, -0.5f, 100, Gold, 1.3f);
                gold.noGravity = true;
                gold.velocity = Main.rand.NextVector2Circular(2f, 2f);
            }

            // Musical notes during swing
            if (Main.rand.NextBool(10))
            {
                Vector2 notePos = hitbox.Center.ToVector2() + Main.rand.NextVector2Circular(15f, 15f);
                ThemedParticles.MusicNote(notePos, Main.rand.NextVector2Circular(1f, 1f), Violet, 0.5f, 20);
            }

            // Star sparkles
            if (Main.rand.NextBool(6))
            {
                Vector2 starPos = hitbox.Center.ToVector2() + Main.rand.NextVector2Circular(12f, 12f);
                var star = new GenericGlowParticle(starPos, Main.rand.NextVector2Circular(1f, 1f), 
                    StarWhite, 0.25f, 18, true);
                MagnumParticleHandler.SpawnParticle(star);
            }

            // Lighting
            Lighting.AddLight(hitbox.Center.ToVector2(), Violet.ToVector3() * 0.4f);
        }
    }

    /// <summary>
    /// Night's Melody Axe - POST-FATE ULTIMATE tier axe.
    /// Superior to Fate's axe with celestial starlight effects.
    /// </summary>
    public class NightsMelodyAxe : ModItem
    {
        private static readonly Color DeepPurple = new Color(45, 27, 78);
        private static readonly Color Gold = new Color(255, 215, 0);
        private static readonly Color Violet = new Color(123, 104, 238);
        private static readonly Color StarWhite = new Color(255, 255, 255);

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.damage = 300; // Higher than Fate
            Item.DamageType = DamageClass.Melee;
            Item.width = 48;
            Item.height = 48;
            Item.useTime = 2;
            Item.useAnimation = 3;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 12f;
            Item.value = Item.sellPrice(platinum: 1);
            Item.rare = ModContent.RarityType<NachtmusikRarity>();
            Item.UseSound = SoundID.Item15 with { Pitch = 0.1f, Volume = 0.9f };
            Item.autoReuse = true;
            Item.useTurn = true;

            // POST-FATE ULTIMATE axe power
            Item.axe = 80; // Very high (displayed as 400%)
            
            Item.maxStack = 1;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect", "400% axe power") { OverrideColor = Gold });
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Every tree falls to the night's symphony'") { OverrideColor = Violet });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<NachtmusikResonantCore>(), 25)
                .AddIngredient(ModContent.ItemType<NachtmusikResonantEnergy>(), 20)
                .AddIngredient(ModContent.ItemType<HarmonicCoreOfNachtmusik>(), 3)
                .AddIngredient(ItemID.LunarBar, 15)
                .AddTile(ModContent.TileType<FatesCosmicAnvilTile>())
                .Register();
        }

        public override void MeleeEffects(Player player, Rectangle hitbox)
        {
            if (Main.rand.NextBool(2))
            {
                Dust dust = Dust.NewDustDirect(new Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height,
                    DustID.PurpleTorch, player.velocity.X * 0.2f, player.velocity.Y * 0.2f, 150, 
                    Violet, 1.4f);
                dust.noGravity = true;
                dust.velocity *= 1.5f;
            }

            if (Main.rand.NextBool(3))
            {
                Dust gold = Dust.NewDustDirect(new Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height,
                    DustID.GoldFlame, 0f, -0.5f, 100, Gold, 1.3f);
                gold.noGravity = true;
            }

            if (Main.rand.NextBool(6))
            {
                Vector2 starPos = hitbox.Center.ToVector2() + Main.rand.NextVector2Circular(12f, 12f);
                var star = new GenericGlowParticle(starPos, Main.rand.NextVector2Circular(1f, 1f), 
                    StarWhite, 0.25f, 18, true);
                MagnumParticleHandler.SpawnParticle(star);
            }

            Lighting.AddLight(hitbox.Center.ToVector2(), Gold.ToVector3() * 0.4f);
        }
    }

    /// <summary>
    /// Night's Melody Hammer - POST-FATE ULTIMATE tier hammer.
    /// Superior to Fate's hammer with celestial starlight effects.
    /// </summary>
    public class NightsMelodyHammer : ModItem
    {
        private static readonly Color DeepPurple = new Color(45, 27, 78);
        private static readonly Color Gold = new Color(255, 215, 0);
        private static readonly Color Violet = new Color(123, 104, 238);
        private static readonly Color StarWhite = new Color(255, 255, 255);

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.damage = 320; // Higher than Fate
            Item.DamageType = DamageClass.Melee;
            Item.width = 48;
            Item.height = 48;
            Item.useTime = 2;
            Item.useAnimation = 4;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 14f;
            Item.value = Item.sellPrice(platinum: 1);
            Item.rare = ModContent.RarityType<NachtmusikRarity>();
            Item.UseSound = SoundID.Item15 with { Pitch = -0.2f, Volume = 1.0f };
            Item.autoReuse = true;
            Item.useTurn = true;

            // POST-FATE ULTIMATE hammer power
            Item.hammer = 200; // Very high
            
            Item.maxStack = 1;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect", "200% hammer power") { OverrideColor = Gold });
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The percussion of creation and destruction'") { OverrideColor = Violet });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<NachtmusikResonantCore>(), 25)
                .AddIngredient(ModContent.ItemType<NachtmusikResonantEnergy>(), 20)
                .AddIngredient(ModContent.ItemType<HarmonicCoreOfNachtmusik>(), 3)
                .AddIngredient(ItemID.LunarBar, 15)
                .AddTile(ModContent.TileType<FatesCosmicAnvilTile>())
                .Register();
        }

        public override void MeleeEffects(Player player, Rectangle hitbox)
        {
            if (Main.rand.NextBool(2))
            {
                Dust dust = Dust.NewDustDirect(new Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height,
                    DustID.PurpleTorch, player.velocity.X * 0.2f, player.velocity.Y * 0.2f, 150, 
                    DeepPurple, 1.5f);
                dust.noGravity = true;
                dust.velocity *= 1.5f;
            }

            if (Main.rand.NextBool(3))
            {
                Dust gold = Dust.NewDustDirect(new Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height,
                    DustID.GoldFlame, 0f, -0.8f, 100, Gold, 1.4f);
                gold.noGravity = true;
            }

            // Shockwave particles on swing
            if (Main.rand.NextBool(4))
            {
                CustomParticles.HaloRing(hitbox.Center.ToVector2(), Violet * 0.5f, 0.2f, 10);
            }

            if (Main.rand.NextBool(6))
            {
                Vector2 starPos = hitbox.Center.ToVector2() + Main.rand.NextVector2Circular(15f, 15f);
                var star = new GenericGlowParticle(starPos, Main.rand.NextVector2Circular(1.5f, 1.5f), 
                    StarWhite, 0.3f, 20, true);
                MagnumParticleHandler.SpawnParticle(star);
            }

            Lighting.AddLight(hitbox.Center.ToVector2(), DeepPurple.ToVector3() * 0.5f);
        }
    }
}
