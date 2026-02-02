using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.DiesIrae.ResonanceEnergies;
using MagnumOpus.Content.DiesIrae.HarmonicCores;
using MagnumOpus.Content.Fate.CraftingStations;
using MagnumOpus.Common;

namespace MagnumOpus.Content.DiesIrae.Tools
{
    /// <summary>
    /// Wrath's Pickaxe - Post-Nachtmusik tier pickaxe.
    /// Superior to Nachtmusik's pickaxe with infernal hellfire effects.
    /// </summary>
    public class WrathsPickaxe : ModItem
    {
        // Use the axe texture as a placeholder until proper pickaxe texture is available
        public override string Texture => "MagnumOpus/Content/DiesIrae/Tools/WrathsAxe";

        // Dies Irae colors
        private static readonly Color BloodRed = new Color(139, 0, 0);
        private static readonly Color EmberOrange = new Color(255, 69, 0);
        private static readonly Color CharredBlack = new Color(25, 20, 15);
        private static readonly Color CrimsonFire = new Color(200, 30, 30);

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.damage = 450; // POST-NACHTMUSIK ULTIMATE - 61%+ above Nachtmusik (280)
            Item.DamageType = DamageClass.Melee;
            Item.width = 44;
            Item.height = 44;
            Item.useTime = 2;
            Item.useAnimation = 2; // Faster than Nachtmusik
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 11f;
            Item.value = Item.sellPrice(platinum: 2);
            Item.rare = ModContent.RarityType<DiesIraeRarity>();
            Item.UseSound = SoundID.Item15 with { Pitch = -0.3f, Volume = 1.0f };
            Item.autoReuse = true;
            Item.useTurn = true;

            // Post-Nachtmusik pickaxe power
            Item.pick = 700; // Higher than Nachtmusik (650)
            
            Item.maxStack = 1;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect", "700% pickaxe power") { OverrideColor = EmberOrange });
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Carves through stone as wrath carves through souls'") { OverrideColor = BloodRed });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<ResonantCoreOfDiesIrae>(), 25)
                .AddIngredient(ModContent.ItemType<DiesIraeResonantEnergy>(), 20)
                .AddIngredient(ModContent.ItemType<HarmonicCoreOfDiesIrae>(), 3)
                .AddIngredient(ItemID.LunarBar, 20)
                .AddTile(ModContent.TileType<FatesCosmicAnvilTile>())
                .Register();
        }

        public override void MeleeEffects(Player player, Rectangle hitbox)
        {
            // Infernal fire particles
            if (Main.rand.NextBool(2))
            {
                Dust dust = Dust.NewDustDirect(new Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height,
                    DustID.Torch, player.velocity.X * 0.2f, player.velocity.Y * 0.2f, 100, 
                    BloodRed, 1.5f);
                dust.noGravity = true;
                dust.velocity *= 1.5f;
            }

            // Ember sparks
            if (Main.rand.NextBool(3))
            {
                Dust ember = Dust.NewDustDirect(new Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height,
                    DustID.Torch, 0f, -0.5f, 100, EmberOrange, 1.3f);
                ember.noGravity = true;
                ember.velocity = Main.rand.NextVector2Circular(2f, 2f);
            }

            // Smoke wisps
            if (Main.rand.NextBool(6))
            {
                Dust smoke = Dust.NewDustDirect(new Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height,
                    DustID.Smoke, 0f, -1f, 150, CharredBlack, 0.9f);
                smoke.noGravity = true;
            }

            // Lighting
            Lighting.AddLight(hitbox.Center.ToVector2(), CrimsonFire.ToVector3() * 0.5f);
        }
    }

    /// <summary>
    /// Wrath's Axe - Post-Nachtmusik tier axe.
    /// Superior to Nachtmusik's axe with infernal hellfire effects.
    /// </summary>
    public class WrathsAxe : ModItem
    {
        private static readonly Color BloodRed = new Color(139, 0, 0);
        private static readonly Color EmberOrange = new Color(255, 69, 0);
        private static readonly Color CharredBlack = new Color(25, 20, 15);
        private static readonly Color CrimsonFire = new Color(200, 30, 30);

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.damage = 480; // POST-NACHTMUSIK ULTIMATE - 60%+ above Nachtmusik (300)
            Item.DamageType = DamageClass.Melee;
            Item.width = 48;
            Item.height = 48;
            Item.useTime = 2;
            Item.useAnimation = 2;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 14f;
            Item.value = Item.sellPrice(platinum: 2);
            Item.rare = ModContent.RarityType<DiesIraeRarity>();
            Item.UseSound = SoundID.Item15 with { Pitch = -0.2f, Volume = 1.0f };
            Item.autoReuse = true;
            Item.useTurn = true;

            // Post-Nachtmusik axe power
            Item.axe = 85; // Higher than Nachtmusik (80) - displayed as 425%
            
            Item.maxStack = 1;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect", "425% axe power") { OverrideColor = EmberOrange });
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Every tree falls before the inferno of judgment'") { OverrideColor = BloodRed });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<ResonantCoreOfDiesIrae>(), 25)
                .AddIngredient(ModContent.ItemType<DiesIraeResonantEnergy>(), 20)
                .AddIngredient(ModContent.ItemType<HarmonicCoreOfDiesIrae>(), 3)
                .AddIngredient(ItemID.LunarBar, 20)
                .AddTile(ModContent.TileType<FatesCosmicAnvilTile>())
                .Register();
        }

        public override void MeleeEffects(Player player, Rectangle hitbox)
        {
            if (Main.rand.NextBool(2))
            {
                Dust dust = Dust.NewDustDirect(new Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height,
                    DustID.Torch, player.velocity.X * 0.2f, player.velocity.Y * 0.2f, 100, 
                    BloodRed, 1.5f);
                dust.noGravity = true;
                dust.velocity *= 1.5f;
            }

            if (Main.rand.NextBool(3))
            {
                Dust ember = Dust.NewDustDirect(new Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height,
                    DustID.Torch, 0f, -0.5f, 100, EmberOrange, 1.4f);
                ember.noGravity = true;
            }

            if (Main.rand.NextBool(5))
            {
                Dust smoke = Dust.NewDustDirect(new Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height,
                    DustID.Smoke, 0f, -1f, 150, CharredBlack, 0.8f);
                smoke.noGravity = true;
            }

            Lighting.AddLight(hitbox.Center.ToVector2(), CrimsonFire.ToVector3() * 0.5f);
        }
    }

    /// <summary>
    /// Wrath's Hammer - Post-Nachtmusik tier hammer.
    /// Superior to Nachtmusik's hammer with infernal hellfire effects.
    /// </summary>
    public class WrathsHammer : ModItem
    {
        private static readonly Color BloodRed = new Color(139, 0, 0);
        private static readonly Color EmberOrange = new Color(255, 69, 0);
        private static readonly Color CharredBlack = new Color(25, 20, 15);
        private static readonly Color CrimsonFire = new Color(200, 30, 30);

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.damage = 520; // POST-NACHTMUSIK ULTIMATE - 62%+ above Nachtmusik (320)
            Item.DamageType = DamageClass.Melee;
            Item.width = 48;
            Item.height = 48;
            Item.useTime = 2;
            Item.useAnimation = 3;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 16f;
            Item.value = Item.sellPrice(platinum: 2);
            Item.rare = ModContent.RarityType<DiesIraeRarity>();
            Item.UseSound = SoundID.Item15 with { Pitch = -0.4f, Volume = 1.1f };
            Item.autoReuse = true;
            Item.useTurn = true;

            // Post-Nachtmusik hammer power
            Item.hammer = 220; // Higher than Nachtmusik (200)
            
            Item.maxStack = 1;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect", "220% hammer power") { OverrideColor = EmberOrange });
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The percussion of damnation echoes eternally'") { OverrideColor = BloodRed });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<ResonantCoreOfDiesIrae>(), 25)
                .AddIngredient(ModContent.ItemType<DiesIraeResonantEnergy>(), 20)
                .AddIngredient(ModContent.ItemType<HarmonicCoreOfDiesIrae>(), 3)
                .AddIngredient(ItemID.LunarBar, 20)
                .AddTile(ModContent.TileType<FatesCosmicAnvilTile>())
                .Register();
        }

        public override void MeleeEffects(Player player, Rectangle hitbox)
        {
            if (Main.rand.NextBool(2))
            {
                Dust dust = Dust.NewDustDirect(new Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height,
                    DustID.Torch, player.velocity.X * 0.2f, player.velocity.Y * 0.2f, 100, 
                    CrimsonFire, 1.6f);
                dust.noGravity = true;
                dust.velocity *= 1.5f;
            }

            if (Main.rand.NextBool(3))
            {
                Dust ember = Dust.NewDustDirect(new Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height,
                    DustID.Torch, 0f, -0.8f, 100, EmberOrange, 1.5f);
                ember.noGravity = true;
            }

            // Heavy smoke from powerful hammer strikes
            if (Main.rand.NextBool(4))
            {
                Dust smoke = Dust.NewDustDirect(new Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height,
                    DustID.Smoke, 0f, -1.5f, 180, CharredBlack, 1.2f);
                smoke.noGravity = true;
            }

            Lighting.AddLight(hitbox.Center.ToVector2(), CrimsonFire.ToVector3() * 0.6f);
        }
    }

    /// <summary>
    /// Wrath's Drill - Post-Nachtmusik tier drill.
    /// Vanilla-style drill that vibrates, points at cursor, and plays chainsaw sound.
    /// Superior mining speed to Nachtmusik's pickaxe.
    /// </summary>
    public class WrathsDrill : ModItem
    {
        private static readonly Color BloodRed = new Color(139, 0, 0);
        private static readonly Color EmberOrange = new Color(255, 69, 0);

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
            // Mark this as a drill for proper behavior
            ItemID.Sets.IsDrill[Type] = true;
        }

        public override void SetDefaults()
        {
            Item.damage = 420; // POST-NACHTMUSIK ULTIMATE DRILL
            // Drills use MeleeNoSpeed so attack speed bonuses don't affect mining
            Item.DamageType = DamageClass.MeleeNoSpeed;
            Item.width = 38;
            Item.height = 18;
            
            // Drill use times are typically 60% of pickaxe values
            Item.useTime = 1; // Very fast drill
            Item.useAnimation = 1;
            
            // CRITICAL: Drill-specific settings
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.channel = true; // Important - projectile checks this
            Item.noMelee = true;
            Item.noUseGraphic = true; // Hide the item, show the projectile
            
            Item.knockBack = 8f;
            Item.value = Item.sellPrice(platinum: 2);
            Item.rare = ModContent.RarityType<DiesIraeRarity>();
            Item.UseSound = SoundID.Item23; // Chainsaw sound
            Item.autoReuse = true;

            // Post-Nachtmusik pickaxe power
            Item.pick = 700; // Same as pickaxe version

            // Shoot the drill projectile
            Item.shoot = ModContent.ProjectileType<WrathsDrillProjectile>();
            Item.shootSpeed = 32f; // Controls holdout distance
            
            Item.maxStack = 1;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect", "700% pickaxe power") { OverrideColor = EmberOrange });
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Mining enhanced by infernal fury") { OverrideColor = BloodRed });
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The drill that pierces through to Hell itself'") { OverrideColor = BloodRed });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<ResonantCoreOfDiesIrae>(), 25)
                .AddIngredient(ModContent.ItemType<DiesIraeResonantEnergy>(), 20)
                .AddIngredient(ModContent.ItemType<HarmonicCoreOfDiesIrae>(), 3)
                .AddIngredient(ItemID.LunarBar, 20)
                .AddTile(ModContent.TileType<FatesCosmicAnvilTile>())
                .Register();
        }
    }
}
