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
    /// Celestial Chorus Baton - Summons a Nocturnal Guardian.
    /// The guardian orbits the player and dashes to attack enemies.
    /// DAMAGE: 520 (aggressive melee minion)
    /// </summary>
    public class CelestialChorusBaton : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 44;
            Item.height = 44;
            Item.damage = 720; // POST-FATE SUMMON
            Item.DamageType = DamageClass.Summon;
            Item.mana = 20;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 4f;
            Item.value = Item.sellPrice(gold: 44);
            Item.rare = ModContent.RarityType<NachtmusikRarity>();
            Item.UseSound = SoundID.Item44;
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<NocturnalGuardianMinion>();
            Item.buffType = ModContent.BuffType<CelestialChorusBatonBuff>();
        }
        
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            player.AddBuff(Item.buffType, 2);
            
            // Spawn minion at player position with entrance VFX
            Vector2 spawnPos = Main.MouseWorld;
            
            // Entrance VFX
            NachtmusikCosmicVFX.SpawnCelestialExplosion(spawnPos, 0.8f);
            NachtmusikCosmicVFX.SpawnConstellationCircle(spawnPos, 40f, 6, 0.35f);
            
            // Music note ring for summon entrance
            ThemedParticles.MusicNoteRing(spawnPos, new Color(100, 60, 180), 40f, 6);
            ThemedParticles.MusicNoteBurst(spawnPos, new Color(80, 100, 200), 5, 3f);
            
            // Star sparkle accents
            for (int sparkle = 0; sparkle < 4; sparkle++)
            {
                var starSparkle = new SparkleParticle(spawnPos + Main.rand.NextVector2Circular(15f, 15f),
                    Main.rand.NextVector2Circular(2f, 2f), new Color(255, 250, 240) * 0.6f, 0.22f, 18);
                MagnumParticleHandler.SpawnParticle(starSparkle);
            }
            
            Projectile.NewProjectile(source, spawnPos, Vector2.Zero, type, damage, knockback, player.whoAmI);
            
            return false;
        }
        
        public override void HoldItem(Player player)
        {
            // Conducting particles while holding
            if (Main.rand.NextBool(8))
            {
                float angle = Main.GameUpdateCount * 0.05f;
                Vector2 particlePos = player.Center + angle.ToRotationVector2() * 30f;
                CustomParticles.GenericFlare(particlePos, NachtmusikCosmicVFX.DeepPurple * 0.6f, 0.2f, 12);
            }
            
            // Floating nocturnal melody notes
            if (Main.rand.NextBool(12))
            {
                Vector2 notePos = player.Center + Main.rand.NextVector2Circular(38f, 38f);
                Vector2 noteVel = new Vector2(Main.rand.NextFloat(-0.3f, 0.3f), -Main.rand.NextFloat(0.3f, 0.6f));
                Color noteColor = Color.Lerp(new Color(100, 60, 180), new Color(80, 100, 200), Main.rand.NextFloat()) * 0.6f;
                ThemedParticles.MusicNote(notePos, noteVel, noteColor, 0.68f, 40);
            }
            
            Lighting.AddLight(player.Center, NachtmusikCosmicVFX.DeepPurple.ToVector3() * 0.3f);
        }
        
        public override void ModifyTooltips(System.Collections.Generic.List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Summon", "Summons a Nocturnal Guardian to fight for you"));
            tooltips.Add(new TooltipLine(Mod, "Behavior", "The guardian orbits you and dashes to attack enemies"));
            tooltips.Add(new TooltipLine(Mod, "Debuff", "Guardian attacks inflict Celestial Harmony"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Conduct the symphony of the night'")
            {
                OverrideColor = NachtmusikCosmicVFX.DeepPurple
            });
        }
    }
    
    /// <summary>
    /// Galactic Overture - Summons a Celestial Muse.
    /// The muse hovers near the player and fires musical projectiles.
    /// DAMAGE: 420 (ranged attacks)
    /// </summary>
    public class GalacticOverture : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 40;
            Item.height = 40;
            Item.damage = 620; // POST-FATE SUMMON
            Item.DamageType = DamageClass.Summon;
            Item.mana = 18;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 3f;
            Item.value = Item.sellPrice(gold: 42);
            Item.rare = ModContent.RarityType<NachtmusikRarity>();
            Item.UseSound = SoundID.Item44;
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<CelestialMuseMinion>();
            Item.buffType = ModContent.BuffType<GalacticOvertureBuff>();
        }
        
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            player.AddBuff(Item.buffType, 2);
            
            Vector2 spawnPos = Main.MouseWorld;
            
            // Musical entrance VFX
            NachtmusikCosmicVFX.SpawnMusicNoteBurst(spawnPos, 8, 5f);
            CustomParticles.GenericFlare(spawnPos, NachtmusikCosmicVFX.Gold, 0.7f, 18);
            
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f;
                Vector2 ringPos = spawnPos + angle.ToRotationVector2() * 30f;
                CustomParticles.GenericFlare(ringPos, NachtmusikCosmicVFX.Violet, 0.3f, 15);
            }
            
            // Music note ring for summon entrance
            ThemedParticles.MusicNoteRing(spawnPos, new Color(100, 60, 180), 45f, 8);
            
            // Star sparkle accents
            for (int sparkle = 0; sparkle < 5; sparkle++)
            {
                var starSparkle = new SparkleParticle(spawnPos + Main.rand.NextVector2Circular(18f, 18f),
                    Main.rand.NextVector2Circular(2f, 2f), new Color(255, 250, 240) * 0.6f, 0.22f, 18);
                MagnumParticleHandler.SpawnParticle(starSparkle);
            }
            
            Projectile.NewProjectile(source, spawnPos, Vector2.Zero, type, damage, knockback, player.whoAmI);
            
            return false;
        }
        
        public override void HoldItem(Player player)
        {
            // Musical notes while holding - VISIBLE SCALE 0.75f+
            if (Main.rand.NextBool(10))
            {
                Vector2 notePos = player.Center + Main.rand.NextVector2Circular(25f, 25f);
                float shimmer = 1f + (float)Math.Sin(Main.GameUpdateCount * 0.15f) * 0.1f;
                ThemedParticles.MusicNote(notePos, new Vector2(0, -1f), NachtmusikCosmicVFX.Gold * 0.6f, 0.75f * shimmer, 25);
                
                // Celestial sparkle accent
                var sparkle = new SparkleParticle(notePos, new Vector2(0, -0.8f), NachtmusikCosmicVFX.StarWhite * 0.45f, 0.24f, 18);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
            
            Lighting.AddLight(player.Center, NachtmusikCosmicVFX.Gold.ToVector3() * 0.3f);
        }
        
        public override void ModifyTooltips(System.Collections.Generic.List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Summon", "Summons a Celestial Muse to fight for you"));
            tooltips.Add(new TooltipLine(Mod, "Behavior", "The muse hovers nearby and fires musical projectiles"));
            tooltips.Add(new TooltipLine(Mod, "Debuff", "Attacks inflict Celestial Harmony"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Let the overture begin'")
            {
                OverrideColor = NachtmusikCosmicVFX.Gold
            });
        }
    }
    
    /// <summary>
    /// Conductor of Constellations - The ultimate summon weapon from Nachtmusik.
    /// Summons a Stellar Conductor that commands star attacks.
    /// DAMAGE: 550 (2 minion slots, very powerful attacks)
    /// </summary>
    public class ConductorOfConstellations : ModItem
    {
        // Using placeholder texture since the weapon PNG doesn't exist (only the minion PNG)
        public override string Texture => "Terraria/Images/Item_" + ItemID.RainbowCrystalStaff;
        
        public override void SetDefaults()
        {
            Item.width = 48;
            Item.height = 48;
            Item.damage = 780; // POST-FATE ULTIMATE SUMMON
            Item.DamageType = DamageClass.Summon;
            Item.mana = 35;
            Item.useTime = 35;
            Item.useAnimation = 35;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 5f;
            Item.value = Item.sellPrice(gold: 55);
            Item.rare = ModContent.RarityType<NachtmusikRarity>();
            Item.UseSound = SoundID.Item82;
            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<StellarConductorMinion>();
            Item.buffType = ModContent.BuffType<ConductorOfConstellationsBuff>();
        }
        
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            player.AddBuff(Item.buffType, 2);
            
            Vector2 spawnPos = Main.MouseWorld;
            
            // Grand entrance VFX
            NachtmusikCosmicVFX.SpawnCelestialExplosion(spawnPos, 1.2f);
            NachtmusikCosmicVFX.SpawnConstellationCircle(spawnPos, 60f, 12, 0.5f);
            NachtmusikCosmicVFX.SpawnMusicNoteBurst(spawnPos, 12, 6f);
            NachtmusikCosmicVFX.SpawnGlyphBurst(spawnPos, 8, 5f, 0.4f);
            
            // Star burst
            for (int i = 0; i < 12; i++)
            {
                float angle = MathHelper.TwoPi * i / 12f;
                Vector2 starPos = spawnPos + angle.ToRotationVector2() * 50f;
                CustomParticles.GenericFlare(starPos, NachtmusikCosmicVFX.StarWhite, 0.4f, 18);
            }
            
            // Music note ring and burst for grand entrance
            ThemedParticles.MusicNoteRing(spawnPos, new Color(100, 60, 180), 55f, 10);
            ThemedParticles.MusicNoteBurst(spawnPos, new Color(80, 100, 200), 8, 5f);
            
            // Star sparkle accents
            for (int sparkle = 0; sparkle < 7; sparkle++)
            {
                var starSparkle = new SparkleParticle(spawnPos + Main.rand.NextVector2Circular(25f, 25f),
                    Main.rand.NextVector2Circular(3f, 3f), new Color(255, 250, 240) * 0.6f, 0.25f, 20);
                MagnumParticleHandler.SpawnParticle(starSparkle);
            }
            
            MagnumScreenEffects.AddScreenShake(6f);
            
            Projectile.NewProjectile(source, spawnPos, Vector2.Zero, type, damage, knockback, player.whoAmI);
            
            return false;
        }
        
        public override void HoldItem(Player player)
        {
            // Majestic conductor aura
            if (Main.rand.NextBool(6))
            {
                float angle = Main.GameUpdateCount * 0.03f;
                for (int i = 0; i < 3; i++)
                {
                    float particleAngle = angle + MathHelper.TwoPi * i / 3f;
                    Vector2 particlePos = player.Center + particleAngle.ToRotationVector2() * 40f;
                    Color auraColor = NachtmusikCosmicVFX.GetCelestialGradient((float)i / 3f);
                    CustomParticles.GenericFlare(particlePos, auraColor, 0.2f, 10);
                }
            }
            
            // Glyphs orbiting
            if (Main.rand.NextBool(15))
            {
                NachtmusikCosmicVFX.SpawnGlyphBurst(player.Center, 1, 2f, 0.2f);
            }
            
            // Floating nocturnal melody notes
            if (Main.rand.NextBool(12))
            {
                Vector2 notePos = player.Center + Main.rand.NextVector2Circular(38f, 38f);
                Vector2 noteVel = new Vector2(Main.rand.NextFloat(-0.3f, 0.3f), -Main.rand.NextFloat(0.3f, 0.6f));
                Color noteColor = Color.Lerp(new Color(100, 60, 180), new Color(80, 100, 200), Main.rand.NextFloat()) * 0.6f;
                ThemedParticles.MusicNote(notePos, noteVel, noteColor, 0.68f, 40);
            }
            
            Lighting.AddLight(player.Center, NachtmusikCosmicVFX.Violet.ToVector3() * 0.4f);
        }
        
        public override void ModifyTooltips(System.Collections.Generic.List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Summon", "Summons a Stellar Conductor to command the cosmos"));
            tooltips.Add(new TooltipLine(Mod, "Slots", "Uses 2 minion slots"));
            tooltips.Add(new TooltipLine(Mod, "Behavior", "The conductor fires star barrages and periodic burst attacks"));
            tooltips.Add(new TooltipLine(Mod, "Debuff", "All attacks inflict Celestial Harmony"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Raise your baton and command the stars themselves'")
            {
                OverrideColor = NachtmusikCosmicVFX.StarWhite
            });
        }
    }
}
