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
using MagnumOpus.Content.Nachtmusik.Debuffs;
using MagnumOpus.Content.Nachtmusik.Projectiles;

namespace MagnumOpus.Content.Nachtmusik.ResonantWeapons
{
    /// <summary>
    /// Constellation Piercer - A bow that fires star-chaining arrows.
    /// Arrows chain between enemies, marking them with constellations.
    /// DAMAGE: 520 (chains make it much higher effective DPS)
    /// </summary>
    public class ConstellationPiercer : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 34;
            Item.height = 66;
            Item.damage = 520;
            Item.DamageType = DamageClass.Ranged;
            Item.useTime = 14;
            Item.useAnimation = 14;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 4f;
            Item.value = Item.sellPrice(gold: 45);
            Item.rare = ModContent.RarityType<NachtmusikRarity>();
            Item.UseSound = SoundID.Item5;
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<ConstellationBoltProjectile>();
            Item.shootSpeed = 18f;
            Item.useAmmo = AmmoID.Arrow;
            Item.crit = 20;
        }
        
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // Always fire constellation bolts, ignore arrow type
            Vector2 direction = velocity.SafeNormalize(Vector2.UnitX);
            
            // Fire main bolt
            Projectile.NewProjectile(source, position, velocity, ModContent.ProjectileType<ConstellationBoltProjectile>(), damage, knockback, player.whoAmI);
            
            // Fire two side bolts at slight angles
            for (int i = -1; i <= 1; i += 2)
            {
                float angleOffset = MathHelper.ToRadians(8f * i);
                Vector2 sideVel = velocity.RotatedBy(angleOffset);
                Projectile.NewProjectile(source, position, sideVel, ModContent.ProjectileType<ConstellationBoltProjectile>(), (int)(damage * 0.7f), knockback * 0.5f, player.whoAmI);
            }
            
            // Enhanced muzzle VFX with star burst
            NachtmusikCosmicVFX.SpawnStarBurstImpact(position + direction * 25f, 0.7f, 2);
            NachtmusikCosmicVFX.SpawnConstellationCircle(position + direction * 30f, 25f, 4, 0.25f);
            NachtmusikCosmicVFX.SpawnShatteredStarlightBurst(position + direction * 20f, 6, 5f, 0.4f, false);
            CustomParticles.GenericFlare(position + direction * 15f, NachtmusikCosmicVFX.Gold, 0.5f, 12);
            
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
            
            Lighting.AddLight(player.Center, NachtmusikCosmicVFX.Gold.ToVector3() * 0.3f);
        }
        
        public override Vector2? HoldoutOffset() => new Vector2(-5f, 0f);
        
        public override void ModifyTooltips(System.Collections.Generic.List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Triple", "Fires three constellation bolts per shot"));
            tooltips.Add(new TooltipLine(Mod, "Chain", "Arrows chain to up to 4 nearby enemies"));
            tooltips.Add(new TooltipLine(Mod, "Debuff", "Inflicts Celestial Harmony on all chained targets"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Each arrow draws another star in the sky'")
            {
                OverrideColor = NachtmusikCosmicVFX.Gold
            });
        }
    }
    
    /// <summary>
    /// Nebula's Whisper - A mystical bow that fires splitting nebula arrows.
    /// Arrows split on hit, creating a cloud of damage.
    /// DAMAGE: 480 (splits make effective damage much higher)
    /// </summary>
    public class NebulasWhisper : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 30;
            Item.height = 58;
            Item.damage = 480;
            Item.DamageType = DamageClass.Ranged;
            Item.useTime = 18;
            Item.useAnimation = 18;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 3f;
            Item.value = Item.sellPrice(gold: 42);
            Item.rare = ModContent.RarityType<NachtmusikRarity>();
            Item.UseSound = SoundID.Item5;
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<NebulaArrowProjectile>();
            Item.shootSpeed = 16f;
            Item.useAmmo = AmmoID.Arrow;
            Item.crit = 16;
        }
        
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // Fire nebula arrow
            Projectile.NewProjectile(source, position, velocity, ModContent.ProjectileType<NebulaArrowProjectile>(), damage, knockback, player.whoAmI);
            
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
            
            Lighting.AddLight(player.Center, NachtmusikCosmicVFX.NebulaPink.ToVector3() * 0.25f);
        }
        
        public override Vector2? HoldoutOffset() => new Vector2(-3f, 0f);
        
        public override void ModifyTooltips(System.Collections.Generic.List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Split", "Arrows split into 4 fragments on first hit"));
            tooltips.Add(new TooltipLine(Mod, "Nebula", "Fragments spread nebula damage across an area"));
            tooltips.Add(new TooltipLine(Mod, "Debuff", "Inflicts Celestial Harmony"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'A whisper from the depths of space'")
            {
                OverrideColor = NachtmusikCosmicVFX.NebulaPink
            });
        }
    }
    
    /// <summary>
    /// Serenade of Distant Stars - A bow that fires homing star projectiles.
    /// Each shot releases multiple stars that seek out enemies.
    /// DAMAGE: 390 per star, fires 4-5 stars
    /// </summary>
    public class SerenadeOfDistantStars : ModItem
    {
        private int shotCounter = 0;
        
        public override void SetDefaults()
        {
            Item.width = 36;
            Item.height = 70;
            Item.damage = 390;
            Item.DamageType = DamageClass.Ranged;
            Item.useTime = 22;
            Item.useAnimation = 22;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 3.5f;
            Item.value = Item.sellPrice(gold: 48);
            Item.rare = ModContent.RarityType<NachtmusikRarity>();
            Item.UseSound = SoundID.Item5;
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<SerenadeStarProjectile>();
            Item.shootSpeed = 14f;
            Item.useAmmo = AmmoID.Arrow;
            Item.crit = 18;
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
            
            // Subtle music notes
            if (Main.rand.NextBool(20))
            {
                Vector2 notePos = player.Center + Main.rand.NextVector2Circular(35f, 35f);
                ThemedParticles.MusicNote(notePos, new Vector2(0, -0.5f), NachtmusikCosmicVFX.Gold * 0.5f, 0.2f, 20);
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
