using Microsoft.Xna.Framework;
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
    /// Sin Collector - Infernal sniper rifle. Post-Nachtmusik tier ranged.
    /// On-hit: Chain lightning to nearby enemies + spawns spinning Wrath's Cleaver copies
    /// </summary>
    public class SinCollector : ModItem
    {
        private static readonly Color BloodRed = new Color(139, 0, 0);
        private static readonly Color EmberOrange = new Color(255, 69, 0);

        private int shotCounter = 0;

        public override void SetDefaults()
        {
            Item.width = 70;
            Item.height = 28;
            Item.damage = 2400; // POST-NACHTMUSIK ULTIMATE SNIPER - 109%+ above Nachtmusik (1150)
            Item.DamageType = DamageClass.Ranged;
            Item.useTime = 25;
            Item.useAnimation = 25;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 8f;
            Item.value = Item.sellPrice(platinum: 2, gold: 50);
            Item.rare = ModContent.RarityType<DiesIraeRarity>();
            Item.UseSound = SoundID.Item40 with { Pitch = -0.2f };
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<SinBullet>();
            Item.shootSpeed = 25f;
            Item.useAmmo = AmmoID.Bullet;
            Item.crit = 35;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Converts bullets into sin-seeking rounds"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "On hit, chains lightning to 3 nearby enemies"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Every 5th shot spawns spinning Wrath's Cleaver copies"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Each bullet claims another soul for judgment'") 
            { 
                OverrideColor = BloodRed 
            });
        }

        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            // Always use our custom bullet
            type = ModContent.ProjectileType<SinBullet>();
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            shotCounter++;
            
            Vector2 muzzlePos = position + velocity.SafeNormalize(Vector2.UnitX) * 50f;
            
            // Fire main sin bullet
            Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
            
            // Muzzle flash VFX
            DiesIraeVFX.FireImpact(muzzlePos, 0.6f);
            
            for (int i = 0; i < 3; i++)
            {
                DiesIraeVFX.SpawnMusicNote(muzzlePos, 
                    velocity.SafeNormalize(Vector2.UnitX).RotatedBy(Main.rand.NextFloat(-0.5f, 0.5f)) * 3f, 
                    DiesIraeColors.HellfireGold, 0.8f);
            }
            
            // Every 5th shot spawns spinning cleaver copies
            if (shotCounter >= 5)
            {
                shotCounter = 0;
                
                SoundEngine.PlaySound(SoundID.Item71 with { Pitch = 0.3f, Volume = 0.7f }, position);
                
                // Spawn 3 spinning cleaver copies around the bullet path
                for (int i = 0; i < 3; i++)
                {
                    float angle = velocity.ToRotation() + MathHelper.ToRadians(-45f + i * 45f);
                    Vector2 cleaverVel = angle.ToRotationVector2() * (velocity.Length() * 0.8f);
                    
                    Projectile.NewProjectile(source, position, cleaverVel,
                        ModContent.ProjectileType<SpinningCleaverCopy>(), damage / 2, knockback / 2, player.whoAmI);
                }
                
                DiesIraeVFX.FireImpact(muzzlePos, 1f);
            }
            
            return false;
        }

        public override Vector2? HoldoutOffset()
        {
            return new Vector2(-10f, 0f);
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
    /// Damnation's Cannon - Heavy hellfire cannon. Post-Nachtmusik tier ranged.
    /// Fires exploding wrath balls that create 5 orbiting shrapnel pieces seeking enemies
    /// </summary>
    public class DamnationsCannon : ModItem
    {
        private static readonly Color BloodRed = new Color(139, 0, 0);
        private static readonly Color EmberOrange = new Color(255, 69, 0);

        public override void SetDefaults()
        {
            Item.width = 80;
            Item.height = 36;
            Item.damage = 3500; // POST-NACHTMUSIK ULTIMATE CANNON - Massive explosive damage
            Item.DamageType = DamageClass.Ranged;
            Item.useTime = 45;
            Item.useAnimation = 45;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 10f;
            Item.value = Item.sellPrice(platinum: 2, gold: 50);
            Item.rare = ModContent.RarityType<DiesIraeRarity>();
            Item.UseSound = SoundID.Item38 with { Pitch = -0.3f, Volume = 1.2f };
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<IgnitedWrathBall>();
            Item.shootSpeed = 14f;
            Item.useAmmo = AmmoID.Rocket;
            Item.crit = 20;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Fires massive exploding balls of wrath"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "On explosion, spawns 5 orbiting shrapnel that seek enemies"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Direct hits cause devastating hellfire explosions"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The cannon that delivers damnation itself'") 
            { 
                OverrideColor = BloodRed 
            });
        }

        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            // Always use our custom projectile
            type = ModContent.ProjectileType<IgnitedWrathBall>();
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Vector2 muzzlePos = position + velocity.SafeNormalize(Vector2.UnitX) * 60f;
            
            // Fire main wrath ball
            Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
            
            // Massive muzzle flash
            DiesIraeVFX.FireImpact(muzzlePos, 1.2f);
            
            // Smoke clouds
            for (int i = 0; i < 5; i++)
            {
                Vector2 smokeVel = velocity.SafeNormalize(Vector2.UnitX).RotatedBy(Main.rand.NextFloat(-0.8f, 0.8f)) * Main.rand.NextFloat(2f, 5f);
                var smoke = new GenericGlowParticle(muzzlePos, smokeVel, DiesIraeColors.CharredBlack * 0.5f, 0.6f, 40, true);
                MagnumParticleHandler.SpawnParticle(smoke);
            }
            
            // Music notes
            for (int i = 0; i < 5; i++)
            {
                DiesIraeVFX.SpawnMusicNote(muzzlePos, 
                    Main.rand.NextVector2Circular(5f, 5f), 
                    DiesIraeColors.GetGradient(Main.rand.NextFloat()), 0.85f);
            }
            
            SoundEngine.PlaySound(SoundID.DD2_BetsyFireballShot with { Volume = 0.8f }, position);
            
            return false;
        }

        public override Vector2? HoldoutOffset()
        {
            return new Vector2(-14f, 2f);
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
    /// Arbiter's Sentence - Long-range flamethrower. Post-Nachtmusik tier ranged.
    /// Changed from bow to flamethrower that fires judgment flames that explode on impact
    /// </summary>
    public class ArbitersSentence : ModItem
    {
        private static readonly Color BloodRed = new Color(139, 0, 0);
        private static readonly Color EmberOrange = new Color(255, 69, 0);

        public override void SetDefaults()
        {
            Item.width = 60;
            Item.height = 24;
            Item.damage = 850; // POST-NACHTMUSIK ULTIMATE FLAMETHROWER - Rapid hits
            Item.DamageType = DamageClass.Ranged;
            Item.useTime = 3;
            Item.useAnimation = 9;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 1f;
            Item.value = Item.sellPrice(platinum: 2, gold: 50);
            Item.rare = ModContent.RarityType<DiesIraeRarity>();
            Item.UseSound = SoundID.Item34;
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<JudgmentFlame>();
            Item.shootSpeed = 18f;
            Item.useAmmo = AmmoID.Gel;
            Item.crit = 15;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Fires a stream of judgment flames"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Flames explode on impact, creating hellfire bursts"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Long range for a flamethrower - the arbiter's reach is far"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The arbiter's flames never miss their mark'") 
            { 
                OverrideColor = BloodRed 
            });
        }

        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            type = ModContent.ProjectileType<JudgmentFlame>();
            
            // Add slight spread
            velocity = velocity.RotatedByRandom(MathHelper.ToRadians(5f));
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // Fire the flame
            Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
            
            // Continuous fire trail VFX
            Vector2 muzzlePos = position + velocity.SafeNormalize(Vector2.UnitX) * 40f;
            
            if (Main.rand.NextBool(3))
            {
                DiesIraeVFX.FireTrail(muzzlePos, velocity * 0.5f, 0.5f);
            }
            
            // Occasional music note
            if (Main.rand.NextBool(8))
            {
                DiesIraeVFX.SpawnMusicNote(muzzlePos, velocity * 0.1f, DiesIraeColors.EmberOrange, 0.7f);
            }
            
            // Fire dust
            for (int i = 0; i < 2; i++)
            {
                Dust dust = Dust.NewDustPerfect(muzzlePos, DustID.Torch, 
                    velocity.SafeNormalize(Vector2.UnitX).RotatedBy(Main.rand.NextFloat(-0.3f, 0.3f)) * Main.rand.NextFloat(3f, 6f),
                    0, default, 1.5f);
                dust.noGravity = true;
            }
            
            return false;
        }

        public override Vector2? HoldoutOffset()
        {
            return new Vector2(-8f, 0f);
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
