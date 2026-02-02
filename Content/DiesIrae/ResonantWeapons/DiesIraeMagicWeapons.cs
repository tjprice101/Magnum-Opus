using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;
using MagnumOpus.Content.DiesIrae.ResonanceEnergies;
using MagnumOpus.Content.DiesIrae.HarmonicCores;
using MagnumOpus.Content.DiesIrae.Projectiles;
using MagnumOpus.Content.Fate.CraftingStations;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Content.DiesIrae.ResonantWeapons
{
    /// <summary>
    /// Staff of Final Judgement - Summons 5 floating ignitions around the player. Post-Nachtmusik tier magic.
    /// The ignitions orbit briefly, then target and explode with shimmer and sparkle effects
    /// </summary>
    public class StaffOfFinalJudgement : ModItem
    {
        private static readonly Color BloodRed = new Color(139, 0, 0);
        private static readonly Color EmberOrange = new Color(255, 69, 0);

        public override void SetDefaults()
        {
            Item.width = 52;
            Item.height = 52;
            Item.damage = 1950; // POST-NACHTMUSIK ULTIMATE MAGIC - 80%+ above Nachtmusik (1080)
            Item.DamageType = DamageClass.Magic;
            Item.mana = 22;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 5f;
            Item.value = Item.sellPrice(platinum: 2, gold: 50);
            Item.rare = ModContent.RarityType<DiesIraeRarity>();
            Item.UseSound = SoundID.Item72 with { Pitch = -0.2f };
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<FloatingIgnition>();
            Item.shootSpeed = 0f;
            Item.crit = 18;
            Item.staff[Type] = true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Summons 5 floating ignitions around you"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Ignitions orbit briefly, then home to enemies and explode"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Explosions create shimmering cascades of hellfire"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The final word in all matters of judgment'") 
            { 
                OverrideColor = BloodRed 
            });
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // Spawn 5 floating ignitions around the player
            SoundEngine.PlaySound(SoundID.DD2_BetsyFireballShot with { Volume = 0.8f, Pitch = 0.2f }, player.Center);
            
            for (int i = 0; i < 5; i++)
            {
                float angle = MathHelper.TwoPi * i / 5f;
                Vector2 spawnOffset = angle.ToRotationVector2() * 60f;
                
                int proj = Projectile.NewProjectile(source, player.Center + spawnOffset, Vector2.Zero,
                    type, damage, knockback, player.whoAmI, ai0: angle);
                
                // Spawn VFX at each ignition
                DiesIraeVFX.FireImpact(player.Center + spawnOffset, 0.4f);
            }
            
            // Central summoning burst
            DiesIraeVFX.FireImpact(player.Center, 0.8f);
            
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                DiesIraeVFX.SpawnMusicNote(player.Center, angle.ToRotationVector2() * 4f, 
                    DiesIraeColors.GetGradient(i / 8f), 0.9f);
            }
            
            return false;
        }

        public override void HoldItem(Player player)
        {
            // Ambient glow when holding
            if (Main.rand.NextBool(10))
            {
                Vector2 staffTip = player.Center + player.direction * new Vector2(35f, -20f);
                Color glowColor = DiesIraeColors.GetGradient(Main.rand.NextFloat());
                var glow = new GenericGlowParticle(staffTip, Main.rand.NextVector2Circular(1f, 1f), 
                    glowColor, 0.25f, 15, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }
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

    /// <summary>
    /// Eclipse of Wrath - Throwable orb that tracks cursor, explodes on impact.
    /// While airborne, spawns tracking wrath shards. Cursor tracking with click-to-explode.
    /// </summary>
    public class EclipseOfWrath : ModItem
    {
        private static readonly Color BloodRed = new Color(139, 0, 0);
        private static readonly Color EmberOrange = new Color(255, 69, 0);

        public override void SetDefaults()
        {
            Item.width = 44;
            Item.height = 44;
            Item.damage = 1750; // POST-NACHTMUSIK ULTIMATE MAGIC - 62%+ above Nachtmusik
            Item.DamageType = DamageClass.Magic;
            Item.mana = 25;
            Item.useTime = 35;
            Item.useAnimation = 35;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 8f;
            Item.value = Item.sellPrice(platinum: 2, gold: 50);
            Item.rare = ModContent.RarityType<DiesIraeRarity>();
            Item.UseSound = SoundID.Item73 with { Pitch = -0.2f };
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.shoot = ModContent.ProjectileType<EclipseOrb>();
            Item.shootSpeed = 12f;
            Item.crit = 22;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Throws an eclipse orb that tracks your cursor"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "While airborne, spawns blazing wrath shards that seek enemies"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Explodes on impact or when clicking again"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The sun's wrath made manifest'") 
            { 
                OverrideColor = BloodRed 
            });
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // Launch the eclipse orb
            Vector2 launchPos = player.Center;
            Vector2 launchVel = (Main.MouseWorld - player.Center).SafeNormalize(Vector2.UnitX) * Item.shootSpeed;
            
            Projectile.NewProjectile(source, launchPos, launchVel, type, damage, knockback, player.whoAmI);
            
            // Launch VFX
            DiesIraeVFX.FireImpact(launchPos, 0.7f);
            
            for (int i = 0; i < 5; i++)
            {
                DiesIraeVFX.SpawnMusicNote(launchPos, 
                    launchVel.SafeNormalize(Vector2.UnitX).RotatedBy(Main.rand.NextFloat(-0.8f, 0.8f)) * 4f,
                    DiesIraeColors.HellfireGold, 0.85f);
            }
            
            SoundEngine.PlaySound(SoundID.DD2_BetsyFireballShot with { Volume = 0.9f }, player.Center);
            
            return false;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<ResonantCoreOfDiesIrae>(), 20)
                .AddIngredient(ModContent.ItemType<DiesIraeResonantEnergy>(), 15)
                .AddIngredient(ModContent.ItemType<HarmonicCoreOfDiesIrae>(), 2)
                .AddIngredient(ItemID.LunarBar, 15)
                .AddTile(ModContent.TileType<FatesCosmicAnvilTile>())
                .Register();
        }
    }

    /// <summary>
    /// Grimoire of Condemnation - Fires 3 blazing music shards that spiral toward enemies.
    /// On impact, shards chain electricity to nearby enemies. Heavy music note VFX.
    /// </summary>
    public class GrimoireOfCondemnation : ModItem
    {
        private static readonly Color BloodRed = new Color(139, 0, 0);
        private static readonly Color EmberOrange = new Color(255, 69, 0);

        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 38;
            Item.damage = 1500; // POST-NACHTMUSIK ULTIMATE MAGIC - 39%+ above Nachtmusik
            Item.DamageType = DamageClass.Magic;
            Item.mana = 12;
            Item.useTime = 15;
            Item.useAnimation = 15;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 4f;
            Item.value = Item.sellPrice(platinum: 2, gold: 50);
            Item.rare = ModContent.RarityType<DiesIraeRarity>();
            Item.UseSound = SoundID.Item103 with { Pitch = 0.2f };
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<BlazingMusicShard>();
            Item.shootSpeed = 14f;
            Item.crit = 15;
            Item.staff[Type] = true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Fires 3 blazing music shards that spiral toward enemies"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "On impact, shards chain electricity to 2 nearby enemies"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "The written word of fire and symphony"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The written word of absolute condemnation'") 
            { 
                OverrideColor = BloodRed 
            });
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Vector2 mouseDir = (Main.MouseWorld - player.Center).SafeNormalize(Vector2.UnitX);
            
            // Fire 3 spiraling shards
            for (int i = 0; i < 3; i++)
            {
                float angleOffset = MathHelper.ToRadians(-20f + i * 20f);
                Vector2 shardVel = mouseDir.RotatedBy(angleOffset) * Item.shootSpeed;
                
                int proj = Projectile.NewProjectile(source, position, shardVel, type, damage, knockback, player.whoAmI,
                    ai0: i * MathHelper.TwoPi / 3f); // Offset spiral phase
                
                // Per-shard VFX
                DiesIraeVFX.SpawnMusicNote(position, shardVel * 0.3f, 
                    DiesIraeColors.GetGradient(i / 3f), 0.8f);
            }
            
            // Cast VFX at staff position
            Vector2 staffPos = position + mouseDir * 30f;
            DiesIraeVFX.FireImpact(staffPos, 0.5f);
            
            // Music note burst from book/staff
            for (int i = 0; i < 4; i++)
            {
                DiesIraeVFX.SpawnMusicNote(staffPos, 
                    Main.rand.NextVector2Circular(4f, 4f), 
                    DiesIraeColors.HellfireGold, 0.85f);
            }
            
            return false;
        }

        public override void HoldItem(Player player)
        {
            // Book glow effect when holding
            if (Main.rand.NextBool(8))
            {
                Vector2 bookPos = player.Center + player.direction * new Vector2(20f, 0f);
                
                // Small orbiting music notes
                float angle = Main.GameUpdateCount * 0.05f;
                Vector2 notePos = bookPos + angle.ToRotationVector2() * 15f;
                
                DiesIraeVFX.SpawnMusicNote(notePos, Vector2.Zero, DiesIraeColors.EmberOrange * 0.7f, 0.5f);
            }
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<ResonantCoreOfDiesIrae>(), 20)
                .AddIngredient(ModContent.ItemType<DiesIraeResonantEnergy>(), 15)
                .AddIngredient(ModContent.ItemType<HarmonicCoreOfDiesIrae>(), 2)
                .AddIngredient(ItemID.LunarBar, 15)
                .AddTile(ModContent.TileType<FatesCosmicAnvilTile>())
                .Register();
        }
    }
}
