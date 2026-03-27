using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;
using MagnumOpus.Content.OdeToJoy;

using MagnumOpus.Content.OdeToJoy.ResonanceEnergies;
using MagnumOpus.Content.OdeToJoy.HarmonicCores;
using MagnumOpus.Content.Fate.CraftingStations;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Content.OdeToJoy.Tools
{
    #region Joy's Drill
    
    /// <summary>
    /// Joy's Drill - Post-Dies Irae tier drill/pickaxe.
    /// 800% pickaxe power, nature-themed VFX
    /// </summary>
    public class JoysDrill : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
            ItemID.Sets.IsDrill[Type] = true;
        }

        public override void SetDefaults()
        {
            Item.damage = 650; // POST-DIES IRAE ULTIMATE DRILL
            Item.DamageType = DamageClass.MeleeNoSpeed;
            Item.width = 44;
            Item.height = 20;
            
            Item.useTime = 1;
            Item.useAnimation = 1;
            
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.channel = true;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            
            Item.knockBack = 7f;
            Item.value = Item.sellPrice(platinum: 3);
            Item.rare = ModContent.RarityType<OdeToJoyRarity>();
            Item.UseSound = SoundID.Item23;
            Item.autoReuse = true;

            Item.pick = 800; // 14% above Dies Irae's 700%
            
            Item.shoot = ModContent.ProjectileType<JoysDrillProjectile>();
            Item.shootSpeed = 34f;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect", "800% pickaxe power") { OverrideColor = OdeToJoyPalette.VerdantGreen });
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Mining powered by nature's boundless energy") { OverrideColor = OdeToJoyPalette.RosePink });
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Dig deep, and let joy bloom forth'") { OverrideColor = OdeToJoyPalette.GoldenPollen });
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
    }

    /// <summary>
    /// Joy's Drill held projectile - vanilla drill AI
    /// </summary>
    public class JoysDrillProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Content/OdeToJoy/Tools/JoysDrill";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.HeldProjDoesNotUsePlayerGfxOffY[Type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = 24;
            Projectile.height = 24;
            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.ownerHitCheck = true;
            Projectile.aiStyle = 20; // Vanilla drill AI
            Projectile.hide = true;
        }

        public override void AI()
        {
            // Nature particles while drilling
            if (Main.rand.NextBool(3))
            {
                Vector2 dustPos = Projectile.Center + Main.rand.NextVector2Circular(12f, 12f);
                Color trailColor = OdeToJoyPalette.GetGradient(Main.rand.NextFloat());
                var trail = new GenericGlowParticle(dustPos, Main.rand.NextVector2Circular(2f, 2f), trailColor * 0.6f, 0.25f, 15, true);
                MagnumParticleHandler.SpawnParticle(trail);
            }
            
            if (Main.rand.NextBool(4))
            {
                Dust leaf = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(15f, 15f),
                    DustID.JungleGrass, Main.rand.NextVector2Circular(2f, 2f), 0, default, 1.2f);
                leaf.noGravity = true;
            }
            
            if (Main.rand.NextBool(5))
            {
                var sparkle = new SparkleParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(15f, 15f),
                    Main.rand.NextVector2Circular(1f, 1f),
                    OdeToJoyPalette.GoldenPollen,
                    0.25f,
                    15
                );
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
            
            Lighting.AddLight(Projectile.Center, OdeToJoyPalette.VerdantGreen.ToVector3() * 0.5f);
        }
    }
    
    #endregion

    #region Joy's Axe
    
    /// <summary>
    /// Joy's Axe - Post-Dies Irae tier axe
    /// 500% axe power
    /// </summary>
    public class JoysAxe : ModItem
    {
        public override void SetDefaults()
        {
            Item.damage = 720;
            Item.DamageType = DamageClass.Melee;
            Item.width = 60;
            Item.height = 54;
            Item.useTime = 6;
            Item.useAnimation = 18;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 8f;
            Item.value = Item.sellPrice(platinum: 3);
            Item.rare = ModContent.RarityType<OdeToJoyRarity>();
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            Item.useTurn = true;
            
            Item.axe = 50; // 500% axe power (displayed value is *10)
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect", "500% axe power") { OverrideColor = OdeToJoyPalette.VerdantGreen });
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Fells forests with a single joyous swing") { OverrideColor = OdeToJoyPalette.RosePink });
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Even the mightiest oak bows to joy'") { OverrideColor = OdeToJoyPalette.GoldenPollen });
        }

        public override void MeleeEffects(Player player, Rectangle hitbox)
        {
            // Dense petal and leaf particles
            if (Main.rand.NextBool(2))
            {
                Vector2 dustPos = new Vector2(
                    Main.rand.Next(hitbox.Left, hitbox.Right),
                    Main.rand.Next(hitbox.Top, hitbox.Bottom)
                );
                
                Color trailColor = OdeToJoyPalette.GetGradient(Main.rand.NextFloat());
                var trail = new GenericGlowParticle(dustPos, player.velocity * 0.2f, trailColor, 0.35f, 15, true);
                MagnumParticleHandler.SpawnParticle(trail);
                
                Dust dust = Dust.NewDustPerfect(dustPos, DustID.JungleGrass, player.velocity * 0.3f, 0, default, 1.4f);
                dust.noGravity = true;
            }
            
            if (Main.rand.NextBool(3))
            {
                Vector2 sparklePos = new Vector2(
                    Main.rand.Next(hitbox.Left, hitbox.Right),
                    Main.rand.Next(hitbox.Top, hitbox.Bottom)
                );
                var sparkle = new SparkleParticle(sparklePos, Main.rand.NextVector2Circular(1f, 1f), 
                    OdeToJoyPalette.GoldenPollen, 0.3f, 15);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
            
            // Music notes
            if (Main.rand.NextBool(6))
            {
                Vector2 notePos = new Vector2(
                    Main.rand.Next(hitbox.Left, hitbox.Right),
                    Main.rand.Next(hitbox.Top, hitbox.Bottom)
                );
            }

            Lighting.AddLight(hitbox.Center.ToVector2(), OdeToJoyPalette.VerdantGreen.ToVector3() * 0.4f);
        }

        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            return false;
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
    }
    
    #endregion

    #region Joy's Hammer
    
    /// <summary>
    /// Joy's Hammer - Post-Dies Irae tier hammer
    /// 260% hammer power
    /// </summary>
    public class JoysHammer : ModItem
    {
        public override void SetDefaults()
        {
            Item.damage = 780;
            Item.DamageType = DamageClass.Melee;
            Item.width = 58;
            Item.height = 58;
            Item.useTime = 8;
            Item.useAnimation = 20;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 12f;
            Item.value = Item.sellPrice(platinum: 3);
            Item.rare = ModContent.RarityType<OdeToJoyRarity>();
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            Item.useTurn = true;
            
            Item.hammer = 260; // 260% hammer power (18% above Dies Irae's 220%)
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect", "260% hammer power") { OverrideColor = OdeToJoyPalette.VerdantGreen });
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Shatters walls with nature's triumphant force") { OverrideColor = OdeToJoyPalette.RosePink });
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Let joy thunder through every barrier'") { OverrideColor = OdeToJoyPalette.GoldenPollen });
        }

        public override void MeleeEffects(Player player, Rectangle hitbox)
        {
            // Dense petal and leaf particles
            if (Main.rand.NextBool(2))
            {
                Vector2 dustPos = new Vector2(
                    Main.rand.Next(hitbox.Left, hitbox.Right),
                    Main.rand.Next(hitbox.Top, hitbox.Bottom)
                );
                
                Color trailColor = OdeToJoyPalette.GetGradient(Main.rand.NextFloat());
                var trail = new GenericGlowParticle(dustPos, player.velocity * 0.2f, trailColor, 0.4f, 18, true);
                MagnumParticleHandler.SpawnParticle(trail);
                
                Dust dust = Dust.NewDustPerfect(dustPos, DustID.JungleGrass, player.velocity * 0.3f, 0, default, 1.5f);
                dust.noGravity = true;
            }
            
            if (Main.rand.NextBool(3))
            {
                Vector2 sparklePos = new Vector2(
                    Main.rand.Next(hitbox.Left, hitbox.Right),
                    Main.rand.Next(hitbox.Top, hitbox.Bottom)
                );
                var sparkle = new SparkleParticle(sparklePos, Main.rand.NextVector2Circular(2f, 2f), 
                    OdeToJoyPalette.GoldenPollen, 0.35f, 18);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
            
            // Heavy pollen smoke for hammer impact
            if (Main.rand.NextBool(4))
            {
                Vector2 smokePos = new Vector2(
                    Main.rand.Next(hitbox.Left, hitbox.Right),
                    Main.rand.Next(hitbox.Top, hitbox.Bottom)
                );
                Dust smoke = Dust.NewDustPerfect(smokePos, DustID.Cloud, -Vector2.UnitY * 1.5f, 150, 
                    OdeToJoyPalette.GoldenPollen * 0.5f, 1.1f);
                smoke.noGravity = true;
            }
            
            // Music notes
            if (Main.rand.NextBool(5))
            {
                Vector2 notePos = new Vector2(
                    Main.rand.Next(hitbox.Left, hitbox.Right),
                    Main.rand.Next(hitbox.Top, hitbox.Bottom)
                );
            }
            
            Lighting.AddLight(hitbox.Center.ToVector2(), OdeToJoyPalette.RosePink.ToVector3() * 0.5f);
        }
        
        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            return false;
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
    }
    
    #endregion
}
