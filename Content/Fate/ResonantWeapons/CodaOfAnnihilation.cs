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
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Content.Fate.Debuffs;
using MagnumOpus.Content.Fate.Projectiles;
using MagnumOpus.Content.MoonlightSonata.CraftingStations;
// Recipe imports
using MagnumOpus.Content.MoonlightSonata.ResonantWeapons;
using MagnumOpus.Content.MoonlightSonata.Weapons;
using MagnumOpus.Content.MoonlightSonata.ResonanceEnergies;
using MagnumOpus.Content.Eroica.ResonantWeapons;
using MagnumOpus.Content.Eroica.ResonanceEnergies;
using MagnumOpus.Content.LaCampanella.ResonantWeapons;
using MagnumOpus.Content.LaCampanella.ResonanceEnergies;
using MagnumOpus.Content.EnigmaVariations.ResonantWeapons;
using MagnumOpus.Content.EnigmaVariations.ResonanceEnergies;
using MagnumOpus.Content.SwanLake.ResonantWeapons;
using MagnumOpus.Content.SwanLake.ResonanceEnergies;
using MagnumOpus.Content.Fate.ResonanceEnergies;

namespace MagnumOpus.Content.Fate.ResonantWeapons
{
    /// <summary>
    /// Coda of Annihilation - The Zenith of MagnumOpus.
    /// A legendary melee weapon that throws spectral copies of all previous score's melee weapons.
    /// Functions like Terraria's Zenith - rapid sword projectiles that home to enemies.
    /// </summary>
    public class CodaOfAnnihilation : ModItem
    {
        public override string Texture => "MagnumOpus/Content/Fate/ResonantWeapons/CodaOfAnnihilation";
        
        // Total number of different weapons to cycle through
        private const int TotalWeaponTypes = 14;
        
        // Counter for which weapon to spawn next
        private int weaponCycleIndex = 0;
        
        public override void SetDefaults()
        {
            // THE ULTIMATE WEAPON - damage to "fry" the Fate boss
            Item.damage = 1350;
            Item.DamageType = DamageClass.Melee;
            Item.width = 54;
            Item.height = 54;
            Item.useTime = 6;
            Item.useAnimation = 6;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 8f;
            Item.value = Item.sellPrice(platinum: 5);
            Item.rare = ModContent.RarityType<FateRarity>();
            Item.UseSound = SoundID.Item1 with { Pitch = 0.1f, Volume = 0.8f };
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.shoot = ModContent.ProjectileType<CodaZenithSwordProjectile>();
            Item.shootSpeed = 22f;
            Item.crit = 15;
        }
        
        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            
            // === ALL MELEE WEAPONS FROM EVERY SCORE ===
            // Moonlight Sonata (2)
            recipe.AddIngredient(ModContent.ItemType<IncisorOfMoonlight>(), 1);
            recipe.AddIngredient(ModContent.ItemType<EternalMoon>(), 1);
            // Eroica (2)
            recipe.AddIngredient(ModContent.ItemType<SakurasBlossom>(), 1);
            recipe.AddIngredient(ModContent.ItemType<CelestialValor>(), 1);
            // La Campanella (2)
            recipe.AddIngredient(ModContent.ItemType<IgnitionOfTheBell>(), 1);
            recipe.AddIngredient(ModContent.ItemType<DualFatedChime>(), 1);
            // Enigma Variations (2)
            recipe.AddIngredient(ModContent.ItemType<VariationsOfTheVoid>(), 1);
            recipe.AddIngredient(ModContent.ItemType<TheUnresolvedCadence>(), 1);
            // Swan Lake (1)
            recipe.AddIngredient(ModContent.ItemType<CalloftheBlackSwan>(), 1);
            // Fate weapons (4 - NOT including Coda itself)
            recipe.AddIngredient(ModContent.ItemType<TheConductorsLastConstellation>(), 1);
            recipe.AddIngredient(ModContent.ItemType<RequiemOfReality>(), 1);
            recipe.AddIngredient(ModContent.ItemType<OpusUltima>(), 1);
            recipe.AddIngredient(ModContent.ItemType<FractalOfTheStars>(), 1);
            
            // === VANILLA ZENITH ===
            recipe.AddIngredient(ItemID.Zenith, 1);
            
            // === 15 OF EACH RESONANCE ENERGY ===
            recipe.AddIngredient(ModContent.ItemType<MoonlightsResonantEnergy>(), 15);
            recipe.AddIngredient(ModContent.ItemType<EroicasResonantEnergy>(), 15);
            recipe.AddIngredient(ModContent.ItemType<LaCampanellaResonantEnergy>(), 15);
            recipe.AddIngredient(ModContent.ItemType<EnigmaResonantEnergy>(), 15);
            recipe.AddIngredient(ModContent.ItemType<SwansResonanceEnergy>(), 15);
            recipe.AddIngredient(ModContent.ItemType<FateResonantEnergy>(), 15);
            
            // Crafted at the Moonlight Anvil (endgame modded station)
            recipe.AddTile(ModContent.TileType<MoonlightAnvilTile>());
            recipe.Register();
        }
        
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "FateEffect", "Launches spectral echoes of every score's legendary blades"));
            tooltips.Add(new TooltipLine(Mod, "FateSpecial", "Each swing summons a different weapon from the symphony of fate"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The final movement - a symphony of every blade that came before'")
            {
                OverrideColor = FateCosmicVFX.FateBrightRed
            });
        }
        
        public override void HoldItem(Player player)
        {
            // === COSMIC ASTRALGRAPH ORBIT EFFECT ===
            // Multiple layered celestial orbits around the player
            
            float time = Main.GameUpdateCount * 0.035f;
            
            // === LAYER 1: OUTER ASTRALGRAPH RING (6 glyphs orbiting slowly) ===
            for (int i = 0; i < 6; i++)
            {
                float orbitAngle = time * 0.7f + MathHelper.TwoPi * i / 6f;
                float orbitRadius = 75f + (float)Math.Sin(time * 0.5f + i) * 8f;
                Vector2 orbitPos = player.Center + orbitAngle.ToRotationVector2() * orbitRadius;
                
                // Astralgraph glyph at each orbit point
                if (Main.GameUpdateCount % 5 == i % 5)
                {
                    Color glyphColor = FateCosmicVFX.GetCosmicGradient((float)i / 6f);
                    CustomParticles.Glyph(orbitPos, glyphColor, 0.45f, -1);
                }
                
                // Sparkle trail behind each astralgraph
                if (Main.rand.NextBool(3))
                {
                    var sparkle = new GenericGlowParticle(
                        orbitPos + Main.rand.NextVector2Circular(8f, 8f),
                        -orbitAngle.ToRotationVector2() * 1.5f,
                        FateCosmicVFX.FateWhite * 0.8f,
                        0.15f,
                        18,
                        true
                    );
                    MagnumParticleHandler.SpawnParticle(sparkle);
                }
            }
            
            // === LAYER 2: INNER STAR RING (4 bright stars orbiting fast in opposite direction) ===
            for (int i = 0; i < 4; i++)
            {
                float innerAngle = -time * 1.2f + MathHelper.TwoPi * i / 4f;
                float innerRadius = 40f + (float)Math.Cos(time + i * 0.5f) * 5f;
                Vector2 innerPos = player.Center + innerAngle.ToRotationVector2() * innerRadius;
                
                // Bright star flare at each point
                if (Main.GameUpdateCount % 4 == i)
                {
                    CustomParticles.GenericFlare(innerPos, FateCosmicVFX.FateWhite, 0.3f, 10);
                }
            }
            
            // === LAYER 3: FIGURE-8 COSMIC TRAIL ===
            if (Main.rand.NextBool(4))
            {
                float t = time * 1.5f;
                float x = (float)Math.Sin(t) * 55f;
                float y = (float)Math.Sin(t * 2) * 30f;
                Vector2 figurePos = player.Center + new Vector2(x, y);
                
                Color trailColor = Color.Lerp(FateCosmicVFX.FateDarkPink, FateCosmicVFX.FatePurple, 
                    (float)Math.Sin(time) * 0.5f + 0.5f);
                var trail = new GenericGlowParticle(figurePos, Main.rand.NextVector2Circular(1f, 1f), 
                    trailColor * 0.6f, 0.22f, 20, true);
                MagnumParticleHandler.SpawnParticle(trail);
            }
            
            // === LAYER 4: AMBIENT COSMIC SPARKLES (everywhere around player) ===
            if (Main.rand.NextBool(3))
            {
                Vector2 sparklePos = player.Center + Main.rand.NextVector2Circular(85f, 85f);
                Color sparkleColor = Main.rand.NextBool() ? FateCosmicVFX.FateWhite : 
                    FateCosmicVFX.GetCosmicGradient(Main.rand.NextFloat());
                
                var sparkle = new GenericGlowParticle(sparklePos, Main.rand.NextVector2Circular(0.5f, 0.5f),
                    sparkleColor, 0.18f, 22, true);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
            
            // === LAYER 5: CONSTELLATION LINES (faint connecting lines via particles) ===
            if (Main.GameUpdateCount % 8 == 0)
            {
                int p1 = Main.rand.Next(6);
                int p2 = (p1 + Main.rand.Next(1, 3)) % 6;
                
                float angle1 = time * 0.7f + MathHelper.TwoPi * p1 / 6f;
                float angle2 = time * 0.7f + MathHelper.TwoPi * p2 / 6f;
                
                Vector2 pos1 = player.Center + angle1.ToRotationVector2() * 75f;
                Vector2 pos2 = player.Center + angle2.ToRotationVector2() * 75f;
                
                // Spawn particles along the line
                for (int i = 0; i < 4; i++)
                {
                    float lerp = (i + 0.5f) / 4f;
                    Vector2 linePos = Vector2.Lerp(pos1, pos2, lerp);
                    var linePart = new GenericGlowParticle(linePos, Vector2.Zero, 
                        FateCosmicVFX.FatePurple * 0.3f, 0.08f, 12, true);
                    MagnumParticleHandler.SpawnParticle(linePart);
                }
            }
            
            // === LAYER 6: COSMIC DUST MOTES (tiny particles drifting upward) ===
            if (Main.rand.NextBool(5))
            {
                Vector2 dustPos = player.Center + new Vector2(Main.rand.NextFloat(-70f, 70f), 
                    Main.rand.NextFloat(20f, 60f));
                var dust = new GenericGlowParticle(dustPos, new Vector2(Main.rand.NextFloat(-0.3f, 0.3f), -0.8f),
                    FateCosmicVFX.FateDarkPink * 0.4f, 0.1f, 35, true);
                MagnumParticleHandler.SpawnParticle(dust);
            }
            
            // === COSMIC LIGHT AURA ===
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.06f) * 0.25f + 0.75f;
            Lighting.AddLight(player.Center, FateCosmicVFX.FatePurple.ToVector3() * pulse * 0.8f);
            Lighting.AddLight(player.Center + new Vector2(0, -30), FateCosmicVFX.FateDarkPink.ToVector3() * pulse * 0.4f);
        }
        
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // Track for star circle effect
            player.GetModPlayer<FateWeaponEffectPlayer>()?.OnFateWeaponAttack(player.Center);
            
            // Spawn 2-3 sword projectiles per swing
            int swordCount = Main.rand.Next(2, 4);
            
            for (int i = 0; i < swordCount; i++)
            {
                // Zenith-style spawn position - above player with randomness
                Vector2 spawnOffset = new Vector2(
                    Main.rand.NextFloat(-80f, 80f),
                    Main.rand.NextFloat(-100f, -30f)
                );
                Vector2 spawnPos = player.Center + spawnOffset;
                
                // Direction toward mouse with spread
                Vector2 toMouse = Main.MouseWorld - spawnPos;
                toMouse = toMouse.SafeNormalize(Vector2.UnitY);
                toMouse = toMouse.RotatedByRandom(MathHelper.ToRadians(25f));
                
                // Spawn velocity
                float speed = Main.rand.NextFloat(16f, 22f);
                Vector2 projVelocity = toMouse * speed;
                
                // Spawn the sword projectile with the current weapon type
                int proj = Projectile.NewProjectile(
                    source,
                    spawnPos,
                    projVelocity,
                    type,
                    damage,
                    knockback,
                    player.whoAmI,
                    weaponCycleIndex, // ai[0] = weapon index
                    0 // ai[1] = target (0 = no target yet)
                );
                
                // Spawn VFX at spawn location
                Color spawnColor = GetWeaponColor(weaponCycleIndex);
                CustomParticles.GenericFlare(spawnPos, spawnColor, 0.5f, 12);
                
                if (Main.rand.NextBool(2))
                {
                    CustomParticles.HaloRing(spawnPos, spawnColor * 0.6f, 0.25f, 10);
                }
                
                // Cycle to next weapon
                weaponCycleIndex = (weaponCycleIndex + 1) % TotalWeaponTypes;
            }
            
            // Swing sound varies
            if (Main.rand.NextBool(3))
            {
                SoundEngine.PlaySound(SoundID.Item71 with { Pitch = Main.rand.NextFloat(-0.2f, 0.3f), Volume = 0.5f }, player.Center);
            }
            
            return false; // We handled spawning manually
        }
        
        public override void UseAnimation(Player player)
        {
            // Zenith-style animation - weapon held forward during swing
            base.UseAnimation(player);
        }
        
        /// <summary>
        /// Returns the theme color for a given weapon index.
        /// Matches the order in CodaZenithSwordProjectile.WeaponColors
        /// </summary>
        private Color GetWeaponColor(int index)
        {
            Color[] colors = new Color[]
            {
                // Moonlight Sonata
                new Color(138, 43, 226),
                new Color(135, 206, 250),
                // Eroica
                new Color(255, 100, 100),
                new Color(255, 200, 80),
                // La Campanella
                new Color(255, 140, 40),
                new Color(255, 180, 60),
                // Enigma Variations
                new Color(140, 60, 200),
                new Color(50, 180, 100),
                // Swan Lake
                new Color(255, 255, 255),
                // Fate
                new Color(180, 50, 100),
                new Color(200, 60, 80),
                new Color(140, 50, 160),
                new Color(160, 80, 140),
                new Color(220, 80, 120)
            };
            
            if (index >= 0 && index < colors.Length)
                return colors[index];
            return FateCosmicVFX.FateDarkPink;
        }
    }
}
