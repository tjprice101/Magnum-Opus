using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Content.ClairDeLune.ResonanceEnergies;
using MagnumOpus.Content.ClairDeLune.HarmonicCores;
using MagnumOpus.Content.Fate.CraftingStations;

namespace MagnumOpus.Content.ClairDeLune.Tools
{
    #region Clock of Time's Drill

    /// <summary>
    /// Clock of Time's Drill — Tier 10 ultimate drill/pickaxe.
    /// Temporal clockwork theme with soft blue and pearl white particles.
    /// </summary>
    public class ClockOfTimesDrill : ModItem
    {
        private static readonly Color MistBlue = new Color(100, 140, 200);
        private static readonly Color PearlWhite = new Color(220, 225, 240);
        private static readonly Color Brass = new Color(205, 127, 50);
        private static readonly Color IceBlue = new Color(150, 200, 255);

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
            ItemID.Sets.IsDrill[Type] = true;
        }

        public override void SetDefaults()
        {
            Item.damage = 780;
            Item.DamageType = DamageClass.MeleeNoSpeed;
            Item.width = 44;
            Item.height = 20;
            Item.useTime = 1;
            Item.useAnimation = 1;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.channel = true;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.knockBack = 8f;
            Item.value = Item.sellPrice(platinum: 4);
            Item.rare = ModContent.RarityType<ClairDeLuneRarity>();
            Item.UseSound = SoundID.Item23;
            Item.autoReuse = true;

            Item.pick = 950; // 19% above Ode to Joy's 800%

            Item.shoot = ModContent.ProjectileType<ClockOfTimesDrillProjectile>();
            Item.shootSpeed = 36f;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect", "950% pickaxe power")
            {
                OverrideColor = Brass
            });
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Drills through time itself, phasing past all resistance")
            {
                OverrideColor = MistBlue
            });
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The gears turn, and stone remembers it was once dust'")
            {
                OverrideColor = IceBlue
            });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<ResonantCoreOfClairDeLune>(), 25)
                .AddIngredient(ModContent.ItemType<ClairDeLuneResonantEnergy>(), 20)
                .AddIngredient(ModContent.ItemType<HarmonicCoreOfClairDeLune>(), 3)
                .AddIngredient(ItemID.LunarBar, 25)
                .AddTile(ModContent.TileType<FatesCosmicAnvilTile>())
                .Register();
        }
    }

    /// <summary>
    /// Clock of Time's Drill held projectile — vanilla drill AI with temporal particles.
    /// </summary>
    public class ClockOfTimesDrillProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Content/ClairDeLune/Tools/ClockOfTimesDrill";

        private static readonly Color MistBlue = new Color(100, 140, 200);
        private static readonly Color PearlWhite = new Color(220, 225, 240);
        private static readonly Color Brass = new Color(205, 127, 50);

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
            Projectile.aiStyle = 20;
            Projectile.hide = true;
        }

        public override void AI()
        {
            if (Main.rand.NextBool(3))
            {
                Vector2 dustPos = Projectile.Center + Main.rand.NextVector2Circular(12f, 12f);
                Color color = Color.Lerp(MistBlue, PearlWhite, Main.rand.NextFloat());
                var glow = new GenericGlowParticle(dustPos, Main.rand.NextVector2Circular(1.5f, 1.5f), color * 0.5f, 0.2f, 12, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }

            if (Main.rand.NextBool(5))
            {
                var sparkle = new SparkleParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(15f, 15f),
                    Main.rand.NextVector2Circular(0.8f, 0.8f),
                    Brass,
                    0.2f,
                    12
                );
                MagnumParticleHandler.SpawnParticle(sparkle);
            }

            Lighting.AddLight(Projectile.Center, MistBlue.ToVector3() * 0.4f);
        }
    }

    #endregion

    #region Clock of Time's Axe

    /// <summary>
    /// Clock of Time's Axe — Tier 10 ultimate axe.
    /// Temporal clockwork theme with gentle luminescent swing particles.
    /// </summary>
    public class ClockOfTimesAxe : ModItem
    {
        private static readonly Color MistBlue = new Color(100, 140, 200);
        private static readonly Color PearlWhite = new Color(220, 225, 240);
        private static readonly Color Brass = new Color(205, 127, 50);
        private static readonly Color IceBlue = new Color(150, 200, 255);

        public override void SetDefaults()
        {
            Item.damage = 860;
            Item.DamageType = DamageClass.Melee;
            Item.width = 60;
            Item.height = 54;
            Item.useTime = 5;
            Item.useAnimation = 16;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 9f;
            Item.value = Item.sellPrice(platinum: 4);
            Item.rare = ModContent.RarityType<ClairDeLuneRarity>();
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            Item.useTurn = true;

            Item.axe = 120; // 600% axe power (120 * 5)
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect", "600% axe power")
            {
                OverrideColor = Brass
            });
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Each swing reverses the growth of centuries")
            {
                OverrideColor = MistBlue
            });
            tooltips.Add(new TooltipLine(Mod, "Lore", "'In the clock's silence, even ancient wood forgets its roots'")
            {
                OverrideColor = IceBlue
            });
        }

        public override void MeleeEffects(Player player, Rectangle hitbox)
        {
            if (Main.rand.NextBool(2))
            {
                Vector2 dustPos = new Vector2(
                    Main.rand.Next(hitbox.Left, hitbox.Right),
                    Main.rand.Next(hitbox.Top, hitbox.Bottom)
                );

                Color color = Color.Lerp(MistBlue, PearlWhite, Main.rand.NextFloat());
                var glow = new GenericGlowParticle(dustPos, player.velocity * 0.2f, color * 0.5f, 0.3f, 14, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }

            if (Main.rand.NextBool(4))
            {
                Vector2 sparklePos = new Vector2(
                    Main.rand.Next(hitbox.Left, hitbox.Right),
                    Main.rand.Next(hitbox.Top, hitbox.Bottom)
                );
                var sparkle = new SparkleParticle(sparklePos, Main.rand.NextVector2Circular(0.8f, 0.8f),
                    Brass, 0.25f, 12);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }

            Lighting.AddLight(hitbox.Center.ToVector2(), MistBlue.ToVector3() * 0.3f);
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<ResonantCoreOfClairDeLune>(), 25)
                .AddIngredient(ModContent.ItemType<ClairDeLuneResonantEnergy>(), 20)
                .AddIngredient(ModContent.ItemType<HarmonicCoreOfClairDeLune>(), 3)
                .AddIngredient(ItemID.LunarBar, 25)
                .AddTile(ModContent.TileType<FatesCosmicAnvilTile>())
                .Register();
        }
    }

    #endregion

    #region Clock of Time's Hammer

    /// <summary>
    /// Clock of Time's Hammer — Tier 10 ultimate hammer.
    /// Temporal clockwork theme with chiming impact particles.
    /// </summary>
    public class ClockOfTimesHammer : ModItem
    {
        private static readonly Color MistBlue = new Color(100, 140, 200);
        private static readonly Color PearlWhite = new Color(220, 225, 240);
        private static readonly Color Brass = new Color(205, 127, 50);
        private static readonly Color IceBlue = new Color(150, 200, 255);

        public override void SetDefaults()
        {
            Item.damage = 950;
            Item.DamageType = DamageClass.Melee;
            Item.width = 58;
            Item.height = 58;
            Item.useTime = 7;
            Item.useAnimation = 18;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 14f;
            Item.value = Item.sellPrice(platinum: 4);
            Item.rare = ModContent.RarityType<ClairDeLuneRarity>();
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            Item.useTurn = true;

            Item.hammer = 310; // 310% hammer power (19% above Ode to Joy's 260%)
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect", "310% hammer power")
            {
                OverrideColor = Brass
            });
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Strikes echo through time, shattering walls before they were built")
            {
                OverrideColor = MistBlue
            });
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The pendulum swings, and all barriers crumble to memory'")
            {
                OverrideColor = IceBlue
            });
        }

        public override void MeleeEffects(Player player, Rectangle hitbox)
        {
            if (Main.rand.NextBool(2))
            {
                Vector2 dustPos = new Vector2(
                    Main.rand.Next(hitbox.Left, hitbox.Right),
                    Main.rand.Next(hitbox.Top, hitbox.Bottom)
                );

                Color color = Color.Lerp(MistBlue, PearlWhite, Main.rand.NextFloat());
                var glow = new GenericGlowParticle(dustPos, player.velocity * 0.2f, color * 0.5f, 0.35f, 14, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }

            if (Main.rand.NextBool(4))
            {
                Vector2 sparklePos = new Vector2(
                    Main.rand.Next(hitbox.Left, hitbox.Right),
                    Main.rand.Next(hitbox.Top, hitbox.Bottom)
                );
                var sparkle = new SparkleParticle(sparklePos, Main.rand.NextVector2Circular(0.8f, 0.8f),
                    Brass, 0.3f, 12);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }

            Lighting.AddLight(hitbox.Center.ToVector2(), MistBlue.ToVector3() * 0.35f);
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<ResonantCoreOfClairDeLune>(), 25)
                .AddIngredient(ModContent.ItemType<ClairDeLuneResonantEnergy>(), 20)
                .AddIngredient(ModContent.ItemType<HarmonicCoreOfClairDeLune>(), 3)
                .AddIngredient(ItemID.LunarBar, 25)
                .AddTile(ModContent.TileType<FatesCosmicAnvilTile>())
                .Register();
        }
    }

    #endregion
}
