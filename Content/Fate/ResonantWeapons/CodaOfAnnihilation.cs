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
using MagnumOpus.Content.Eroica.Weapons.CelestialValor;
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
    /// The weapon itself swings in a 360-degree arc around the player as part of the attack.
    /// </summary>
    public class CodaOfAnnihilation : ModItem
    {
        public override string Texture => "MagnumOpus/Content/Fate/ResonantWeapons/CodaOfAnnihilation";
        
        // Track for star circle effect
        private int weaponCycleIndex = 0;
        
        // Total number of different weapons to cycle through
        private const int TotalWeaponTypes = 14;
        
        // Track the held swing projectile
        private int heldSwingProjectile = -1;
        
        // Track if we've already spawned a swing this use
        private bool swingSpawnedThisUse = false;
        
        public override void SetDefaults()
        {
            // THE ULTIMATE WEAPON - damage to "fry" the Fate boss
            Item.damage = 1350;
            Item.DamageType = DamageClass.Melee;
            Item.width = 54;
            Item.height = 54;
            Item.useTime = 18;
            Item.useAnimation = 18;
            Item.useStyle = ItemUseStyleID.Shoot; // Use Shoot style for custom weapon positioning
            Item.knockBack = 8f;
            Item.value = Item.sellPrice(platinum: 5);
            Item.rare = ModContent.RarityType<FateRarity>();
            Item.UseSound = SoundID.Item1 with { Pitch = 0.1f, Volume = 0.8f };
            Item.autoReuse = true;
            Item.noMelee = true; // The held swing projectile handles damage
            Item.noUseGraphic = true; // We draw it ourselves via the held projectile
            Item.shoot = ModContent.ProjectileType<CodaZenithSwordProjectile>();
            Item.shootSpeed = 22f;
            Item.crit = 15;
            Item.channel = true; // Allow channeling for swing control
        }
        
        public override void UseStyle(Player player, Rectangle heldItemFrame)
        {
            // Point toward mouse during channel for better control feel
            if (player.channel)
            {
                Vector2 toMouse = Main.MouseWorld - player.Center;
                player.itemRotation = toMouse.ToRotation();
                if (player.direction == -1)
                    player.itemRotation += MathHelper.Pi;
            }
        }
        
        public override void UseItemHitbox(Player player, ref Rectangle hitbox, ref bool noHitbox)
        {
            // No hitbox from the item itself - the held swing projectile handles this
            noHitbox = true;
        }
        
        public override void MeleeEffects(Player player, Rectangle hitbox)
        {
            // Melee effects handled by the CodaHeldSwingProjectile
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
            recipe.AddIngredient(ModContent.ItemType<VariationsOfTheVoidItem>(), 1);
            recipe.AddIngredient(ModContent.ItemType<TheUnresolvedCadenceItem>(), 1);
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
            // Reset spawn flag when animation ends OR when a new animation cycle starts
            // This allows continuous swinging with autoReuse
            if (player.itemAnimation <= 0)
            {
                swingSpawnedThisUse = false;
                heldSwingProjectile = -1;
            }
            // Also reset when animation is at the start of a new cycle (for autoReuse continuity)
            else if (player.itemAnimation == player.itemAnimationMax - 1)
            {
                swingSpawnedThisUse = false;
            }
            
            // Spawn held swing projectile ONCE per animation cycle
            if (player.itemAnimation > 0 && !swingSpawnedThisUse)
            {
                swingSpawnedThisUse = true;
                
                // Spawn the visual spinning sword projectile
                if (Main.myPlayer == player.whoAmI)
                {
                    heldSwingProjectile = Projectile.NewProjectile(
                        player.GetSource_ItemUse(Item),
                        player.Center,
                        Vector2.Zero,
                        ModContent.ProjectileType<CodaHeldSwingProjectile>(),
                        Item.damage,
                        Item.knockBack,
                        player.whoAmI
                    );
                }
            }
            
            // Simple cosmic lighting - no particle clutter
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.06f) * 0.25f + 0.75f;
            Lighting.AddLight(player.Center, FateCosmicVFX.FatePurple.ToVector3() * pulse * 0.6f);
            
            // NOTE: Removed excessive orbit effects - GlobalWeaponVFXOverhaul handles swing VFX
            // The weapon's visual identity comes from its SWING, not its IDLE state
            // Old astralgraph orbit layers 1-6 removed for cleaner visuals
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
