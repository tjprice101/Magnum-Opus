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
            Item.damage = 1150; // POST-FATE ULTIMATE RANGED - 35%+ above Fate tier
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
            
            // Muzzle flash - trust star burst for core VFX
            NachtmusikCosmicVFX.SpawnStarBurstImpact(position + direction * 25f, 0.6f, 2);
            CustomParticles.GenericFlare(position + direction * 15f, NachtmusikCosmicVFX.Gold, 0.4f, 12);
            
            // Music note on shot
            ThemedParticles.MusicNote(position + direction * 18f, direction * 2f, new Color(100, 60, 180) * 0.7f, 0.65f, 20);
            
            return false;
        }
        
        public override void HoldItem(Player player)
        {
            // Sparse ambient music note
            if (Main.rand.NextBool(25))
            {
                Vector2 notePos = player.Center + Main.rand.NextVector2Circular(30f, 30f);
                Color noteColor = Color.Lerp(new Color(100, 60, 180), new Color(80, 100, 200), Main.rand.NextFloat()) * 0.5f;
                ThemedParticles.MusicNote(notePos, new Vector2(0, -0.4f), noteColor, 0.6f, 30);
            }
            
            Lighting.AddLight(player.Center, NachtmusikCosmicVFX.Gold.ToVector3() * 0.2f);
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
            Item.damage = 1050; // POST-FATE ULTIMATE RANGED - 40%+ above Fate tier
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
            ThemedParticles.MusicNote(position + direction * 15f, direction * 1.5f, new Color(100, 60, 180) * 0.7f, 0.65f, 20);
            
            // Sparse nebula cloud muzzle effect
            for (int i = 0; i < 2; i++)
            {
                Vector2 offset = Main.rand.NextVector2Circular(12f, 12f);
                Color nebulaColor = Color.Lerp(NachtmusikCosmicVFX.NebulaPink, NachtmusikCosmicVFX.Violet, Main.rand.NextFloat());
                var cloud = new GenericGlowParticle(position + offset, velocity * 0.08f, nebulaColor * 0.4f, 0.2f, 15, true);
                MagnumParticleHandler.SpawnParticle(cloud);
            }
            
            return false;
        }
        
        public override void HoldItem(Player player)
        {
            // Sparse ambient music note
            if (Main.rand.NextBool(25))
            {
                Vector2 notePos = player.Center + Main.rand.NextVector2Circular(30f, 30f);
                Color noteColor = Color.Lerp(NachtmusikCosmicVFX.NebulaPink, NachtmusikCosmicVFX.Violet, Main.rand.NextFloat()) * 0.5f;
                ThemedParticles.MusicNote(notePos, new Vector2(0, -0.4f), noteColor, 0.65f, 30);
            }
            
            Lighting.AddLight(player.Center, NachtmusikCosmicVFX.NebulaPink.ToVector3() * 0.2f);
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
            Item.damage = 920; // POST-FATE ULTIMATE RANGED - 36%+ above Fate tier
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
            
            // Serenade burst - trust star burst for core VFX
            Vector2 direction = velocity.SafeNormalize(Vector2.UnitX);
            NachtmusikCosmicVFX.SpawnStarBurstImpact(position + direction * 25f, 0.5f, 2);
            CustomParticles.GenericFlare(position + direction * 20f, NachtmusikCosmicVFX.StarWhite, 0.5f, 12);
            
            // Music note burst (reduced count)
            ThemedParticles.MusicNoteBurst(position + direction * 25f, new Color(100, 60, 180), 3, 3f);
            
            return false;
        }
        
        public override void HoldItem(Player player)
        {
            // Sparse ambient star flare
            if (Main.rand.NextBool(25))
            {
                Vector2 starPos = player.Center + Main.rand.NextVector2Circular(40f, 40f);
                CustomParticles.GenericFlare(starPos, NachtmusikCosmicVFX.StarWhite * 0.5f, 0.12f, 10);
            }
            
            // Sparse music note
            if (Main.rand.NextBool(30))
            {
                Vector2 notePos = player.Center + Main.rand.NextVector2Circular(30f, 30f);
                ThemedParticles.MusicNote(notePos, new Vector2(0, -0.4f), NachtmusikCosmicVFX.Gold * 0.5f, 0.6f, 25);
            }
            
            Lighting.AddLight(player.Center, NachtmusikCosmicVFX.StarWhite.ToVector3() * 0.25f);
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
