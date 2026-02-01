using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using static MagnumOpus.Common.Systems.ThemedParticles;
using MagnumOpus.Content.Nachtmusik.Debuffs;
using MagnumOpus.Content.Nachtmusik.Projectiles;

namespace MagnumOpus.Content.Nachtmusik.ResonantWeapons
{
    /// <summary>
    /// Constellation Piercer - A celestial rifle that fires star-chaining energy bolts.
    /// Bolts chain between enemies, marking them with constellations.
    /// DAMAGE: 750 (chains make it much higher effective DPS)
    /// </summary>
    public class ConstellationPiercer : ModItem
    {
        private int crystalCounter = 0;
        
        public override void SetDefaults()
        {
            Item.width = 34;
            Item.height = 66;
            Item.damage = 750; // POST-FATE RANGED
            Item.DamageType = DamageClass.Ranged;
            Item.useTime = 12;
            Item.useAnimation = 12;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 4.5f;
            Item.value = Item.sellPrice(gold: 45);
            Item.rare = ModContent.RarityType<NachtmusikRarity>();
            Item.UseSound = SoundID.Item41 with { Pitch = -0.2f, Volume = 0.9f }; // Gun sound
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<ConstellationBoltProjectile>();
            Item.shootSpeed = 22f;
            Item.useAmmo = AmmoID.Bullet;
            Item.crit = 22;
        }
        
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // Always fire constellation bolts, ignore arrow type
            Vector2 direction = velocity.SafeNormalize(Vector2.UnitX);
            
            // Track shots for crystal burst
            crystalCounter++;
            
            // Fire main bolt
            Projectile.NewProjectile(source, position, velocity, ModContent.ProjectileType<ConstellationBoltProjectile>(), damage, knockback, player.whoAmI);
            
            // Fire two side bolts at slight angles
            for (int i = -1; i <= 1; i += 2)
            {
                float angleOffset = MathHelper.ToRadians(8f * i);
                Vector2 sideVel = velocity.RotatedBy(angleOffset);
                Projectile.NewProjectile(source, position, sideVel, ModContent.ProjectileType<ConstellationBoltProjectile>(), (int)(damage * 0.7f), knockback * 0.5f, player.whoAmI);
            }
            
            // === SPAWN SEEKING CRYSTALS EVERY 5 SHOTS ===
            if (crystalCounter >= 5)
            {
                crystalCounter = 0;
                SeekingCrystalHelper.SpawnNachtmusikCrystals(
                    source,
                    position + direction * 30f,
                    velocity * 0.8f,
                    (int)(damage * 0.5f),
                    knockback,
                    player.whoAmI,
                    4
                );
                SoundEngine.PlaySound(SoundID.Item25 with { Pitch = 0.4f, Volume = 0.7f }, position);
            }
            
            // Enhanced muzzle VFX with star burst
            NachtmusikCosmicVFX.SpawnStarBurstImpact(position + direction * 25f, 0.7f, 2);
            NachtmusikCosmicVFX.SpawnConstellationCircle(position + direction * 30f, 25f, 4, 0.25f);
            NachtmusikCosmicVFX.SpawnShatteredStarlightBurst(position + direction * 20f, 6, 5f, 0.4f, false);
            CustomParticles.GenericFlare(position + direction * 15f, NachtmusikCosmicVFX.Gold, 0.5f, 12);
            
            // Music note on shot
            ThemedParticles.MusicNote(position + direction * 18f, direction * 2f, new Color(100, 60, 180) * 0.8f, 0.7f, 25);
            
            // Star sparkle accents
            for (int sparkle = 0; sparkle < 3; sparkle++)
            {
                var starSparkle = new SparkleParticle(position + direction * 20f + Main.rand.NextVector2Circular(10f, 10f),
                    direction * 1.5f + Main.rand.NextVector2Circular(1f, 1f), new Color(255, 250, 240) * 0.5f, 0.2f, 16);
                MagnumParticleHandler.SpawnParticle(starSparkle);
            }
            
            return false;
        }
        
        public override void HoldItem(Player player)
        {
            // Subtle star particles while holding
            if (Main.rand.NextBool(8))
            {
                Vector2 bowTip = player.Center + new Vector2(player.direction * 25f, -5f);
                var star = new GenericGlowParticle(bowTip + Main.rand.NextVector2Circular(10f, 10f),
                    new Vector2(0, -0.5f), NachtmusikCosmicVFX.StarWhite * 0.5f, 0.15f, 15, true);
                MagnumParticleHandler.SpawnParticle(star);
            }
            
            // Floating nocturnal melody notes
            if (Main.rand.NextBool(12))
            {
                Vector2 notePos = player.Center + Main.rand.NextVector2Circular(38f, 38f);
                Vector2 noteVel = new Vector2(Main.rand.NextFloat(-0.3f, 0.3f), -Main.rand.NextFloat(0.3f, 0.6f));
                Color noteColor = Color.Lerp(new Color(100, 60, 180), new Color(80, 100, 200), Main.rand.NextFloat()) * 0.6f;
                ThemedParticles.MusicNote(notePos, noteVel, noteColor, 0.68f, 40);
            }
            
            Lighting.AddLight(player.Center, NachtmusikCosmicVFX.Gold.ToVector3() * 0.3f);
        }
        
        public override Vector2? HoldoutOffset() => new Vector2(-5f, 0f);
        
        public override void ModifyTooltips(System.Collections.Generic.List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Triple", "Fires three constellation bolts per shot"));
            tooltips.Add(new TooltipLine(Mod, "Chain", "Bolts chain to up to 4 nearby enemies"));
            tooltips.Add(new TooltipLine(Mod, "Debuff", "Inflicts Celestial Harmony on all chained targets"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Each shot etches another star into the cosmos'")
            {
                OverrideColor = NachtmusikCosmicVFX.Gold
            });
        }
    }
    
    /// <summary>
    /// Nebula's Whisper - A cosmic cannon that fires splitting nebula blasts.
    /// Shots split on hit, creating a cloud of cosmic damage.
    /// DAMAGE: 680 (splits make effective damage much higher)
    /// </summary>
    public class NebulasWhisper : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 30;
            Item.height = 58;
            Item.damage = 680; // POST-FATE RANGED
            Item.DamageType = DamageClass.Ranged;
            Item.useTime = 16;
            Item.useAnimation = 16;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 3.5f;
            Item.value = Item.sellPrice(gold: 42);
            Item.rare = ModContent.RarityType<NachtmusikRarity>();
            Item.UseSound = SoundID.Item38 with { Pitch = 0.1f, Volume = 0.85f }; // Cosmic gun sound
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<NebulaArrowProjectile>();
            Item.shootSpeed = 20f;
            Item.useAmmo = AmmoID.Bullet;
            Item.crit = 18;
        }
        
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // Fire nebula arrow
            Projectile.NewProjectile(source, position, velocity, ModContent.ProjectileType<NebulaArrowProjectile>(), damage, knockback, player.whoAmI);
            
            // Music note on shot
            Vector2 direction = velocity.SafeNormalize(Vector2.UnitX);
            ThemedParticles.MusicNote(position + direction * 15f, direction * 1.5f, new Color(100, 60, 180) * 0.8f, 0.7f, 25);
            
            // Nebula cloud muzzle effect
            for (int i = 0; i < 6; i++)
            {
                Vector2 offset = Main.rand.NextVector2Circular(15f, 15f);
                Color nebulaColor = Color.Lerp(NachtmusikCosmicVFX.NebulaPink, NachtmusikCosmicVFX.Violet, Main.rand.NextFloat());
                var cloud = new GenericGlowParticle(position + offset, velocity * 0.1f + Main.rand.NextVector2Circular(1f, 1f),
                    nebulaColor * 0.5f, 0.25f, 18, true);
                MagnumParticleHandler.SpawnParticle(cloud);
            }
            
            return false;
        }
        
        public override void HoldItem(Player player)
        {
            // Nebula wisps while holding
            if (Main.rand.NextBool(10))
            {
                Vector2 wispPos = player.Center + Main.rand.NextVector2Circular(30f, 30f);
                Color wispColor = Color.Lerp(NachtmusikCosmicVFX.NebulaPink, NachtmusikCosmicVFX.DeepPurple, Main.rand.NextFloat());
                var wisp = new GenericGlowParticle(wispPos, Main.rand.NextVector2Circular(0.5f, 0.5f),
                    wispColor * 0.4f, 0.2f, 20, true);
                MagnumParticleHandler.SpawnParticle(wisp);
            }
            
            // Floating nocturnal melody notes - VISIBLE SCALE 0.75f+
            if (Main.rand.NextBool(12))
            {
                Vector2 notePos = player.Center + Main.rand.NextVector2Circular(38f, 38f);
                Vector2 noteVel = new Vector2(Main.rand.NextFloat(-0.3f, 0.3f), -Main.rand.NextFloat(0.3f, 0.6f));
                Color noteColor = Color.Lerp(NachtmusikCosmicVFX.NebulaPink, NachtmusikCosmicVFX.Violet, Main.rand.NextFloat()) * 0.7f;
                float shimmer = 1f + (float)Math.Sin(Main.GameUpdateCount * 0.15f) * 0.1f;
                ThemedParticles.MusicNote(notePos, noteVel, noteColor, 0.75f * shimmer, 40);
                
                // Nebula sparkle companion
                var sparkle = new SparkleParticle(notePos, noteVel * 0.8f, NachtmusikCosmicVFX.StarWhite * 0.4f, 0.22f, 18);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
            
            Lighting.AddLight(player.Center, NachtmusikCosmicVFX.NebulaPink.ToVector3() * 0.25f);
        }
        
        public override Vector2? HoldoutOffset() => new Vector2(-3f, 0f);
        
        public override void ModifyTooltips(System.Collections.Generic.List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Split", "Shots split into 4 nebula fragments on first hit"));
            tooltips.Add(new TooltipLine(Mod, "Nebula", "Fragments spread cosmic damage across an area"));
            tooltips.Add(new TooltipLine(Mod, "Debuff", "Inflicts Celestial Harmony"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'A whisper from the depths of space'")
            {
                OverrideColor = NachtmusikCosmicVFX.NebulaPink
            });
        }
    }
    
    /// <summary>
    /// Serenade of Distant Stars - A cosmic rifle that fires homing star projectiles.
    /// Each shot releases multiple stars that seek out enemies.
    /// DAMAGE: 580 per star, fires 4-5 stars
    /// </summary>
    public class SerenadeOfDistantStars : ModItem
    {
        private int shotCounter = 0;
        
        public override void SetDefaults()
        {
            Item.width = 36;
            Item.height = 70;
            Item.damage = 580; // POST-FATE RANGED
            Item.DamageType = DamageClass.Ranged;
            Item.useTime = 18;
            Item.useAnimation = 18;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 4f;
            Item.value = Item.sellPrice(gold: 48);
            Item.rare = ModContent.RarityType<NachtmusikRarity>();
            Item.UseSound = SoundID.Item91 with { Pitch = 0.2f, Volume = 0.8f }; // Cosmic gun sound
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<SerenadeStarProjectile>();
            Item.shootSpeed = 16f;
            Item.useAmmo = AmmoID.Bullet;
            Item.crit = 20;
        }
        
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            shotCounter++;
            
            // Fire 4-5 homing stars
            int starCount = 4 + (shotCounter % 3 == 0 ? 1 : 0);
            
            for (int i = 0; i < starCount; i++)
            {
                float angleOffset = MathHelper.ToRadians(-20f + 10f * i);
                Vector2 starVel = velocity.RotatedBy(angleOffset + Main.rand.NextFloat(-0.05f, 0.05f));
                
                // Stagger spawn positions slightly
                Vector2 starPos = position + Main.rand.NextVector2Circular(8f, 8f);
                
                Projectile.NewProjectile(source, starPos, starVel, ModContent.ProjectileType<SerenadeStarProjectile>(), damage, knockback, player.whoAmI);
            }
            
            // Enhanced serenade burst VFX with star particles
            Vector2 direction = velocity.SafeNormalize(Vector2.UnitX);
            NachtmusikCosmicVFX.SpawnStarBurstImpact(position + direction * 25f, 0.6f, 2);
            CustomParticles.GenericFlare(position + direction * 20f, NachtmusikCosmicVFX.StarWhite, 0.6f, 15);
            NachtmusikCosmicVFX.SpawnMusicNoteBurst(position + direction * 30f, 4, 4f);
            NachtmusikCosmicVFX.SpawnShatteredStarlightBurst(position + direction * 18f, 4, 3f, 0.35f, false);
            
            // Music note ring for serenade effect
            ThemedParticles.MusicNoteRing(position + direction * 25f, new Color(100, 60, 180), 35f, 6);
            
            // Star sparkle accents
            for (int sparkle = 0; sparkle < 4; sparkle++)
            {
                var starSparkle = new SparkleParticle(position + direction * 22f + Main.rand.NextVector2Circular(12f, 12f),
                    direction * 2f + Main.rand.NextVector2Circular(1.5f, 1.5f), new Color(255, 250, 240) * 0.6f, 0.22f, 18);
                MagnumParticleHandler.SpawnParticle(starSparkle);
            }
            
            // Play a musical sound
            SoundEngine.PlaySound(SoundID.Item25 with { Pitch = 0.3f + Main.rand.NextFloat(-0.1f, 0.1f), Volume = 0.6f }, position);
            
            return false;
        }
        
        public override void HoldItem(Player player)
        {
            // Distant star sparkles
            if (Main.rand.NextBool(12))
            {
                Vector2 starPos = player.Center + Main.rand.NextVector2Circular(50f, 50f);
                CustomParticles.GenericFlare(starPos, NachtmusikCosmicVFX.StarWhite * 0.6f, 0.15f, 12);
            }
            
            // Subtle music notes - VISIBLE SCALE 0.75f+
            if (Main.rand.NextBool(16))
            {
                Vector2 notePos = player.Center + Main.rand.NextVector2Circular(35f, 35f);
                float shimmer = 1f + (float)Math.Sin(Main.GameUpdateCount * 0.15f) * 0.1f;
                ThemedParticles.MusicNote(notePos, new Vector2(0, -0.5f), NachtmusikCosmicVFX.Gold * 0.6f, 0.75f * shimmer, 25);
                
                // Star sparkle accent
                var sparkle = new SparkleParticle(notePos, new Vector2(0, -0.4f), NachtmusikCosmicVFX.StarWhite * 0.45f, 0.22f, 18);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
            
            Lighting.AddLight(player.Center, NachtmusikCosmicVFX.StarWhite.ToVector3() * 0.35f);
        }
        
        public override Vector2? HoldoutOffset() => new Vector2(-6f, 0f);
        
        public override void ModifyTooltips(System.Collections.Generic.List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Stars", "Fires 4-5 homing star projectiles per shot"));
            tooltips.Add(new TooltipLine(Mod, "Homing", "Stars aggressively home in on enemies"));
            tooltips.Add(new TooltipLine(Mod, "Debuff", "Inflicts stacking Celestial Harmony"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'A love song sung by the stars themselves'")
            {
                OverrideColor = NachtmusikCosmicVFX.StarWhite
            });
        }
    }
}
