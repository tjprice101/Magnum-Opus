using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;
using MagnumOpus.Content.Fate.ResonanceEnergies;
using MagnumOpus.Content.Fate.HarmonicCores;
using MagnumOpus.Content.MoonlightSonata.CraftingStations;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems;

namespace MagnumOpus.Content.Fate.Accessories
{
    /// <summary>
    /// Orrery of Infinite Orbits - Summon accessory for Fate theme.
    /// A cosmic planetarium that enhances minion capabilities.
    /// Summons orbit around the player and gain cosmic empowerment.
    /// </summary>
    public class OrreryofInfiniteOrbits : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.accessory = true;
            Item.value = Item.buyPrice(platinum: 3);
            Item.rare = ModContent.RarityType<FateRarity>();
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var modPlayer = player.GetModPlayer<OrreryPlayer>();
            modPlayer.hasOrrery = true;
            
            // +22% summon damage
            player.GetDamage(DamageClass.Summon) += 0.22f;
            
            // +1 max minion
            player.maxMinions += 1;
            
            // +10% minion knockback
            player.GetKnockback(DamageClass.Summon) += 0.10f;
            
            // Cosmic orrery ambient particles - orbiting planets
            if (!hideVisual)
            {
                float orbitSpeed = Main.GameUpdateCount * 0.02f;
                
                // Three orbiting "planets" at different speeds
                for (int planet = 0; planet < 3; planet++)
                {
                    float planetAngle = orbitSpeed * (1f + planet * 0.3f) + MathHelper.TwoPi * planet / 3f;
                    float orbitRadius = 30f + planet * 8f;
                    Vector2 planetPos = player.Center + planetAngle.ToRotationVector2() * orbitRadius;
                    
                    if (Main.rand.NextBool(8))
                    {
                        Color planetColor = planet switch
                        {
                            0 => FateCosmicVFX.FateDarkPink,
                            1 => FateCosmicVFX.FateBrightRed,
                            _ => FateCosmicVFX.FatePurple
                        };
                        
                        Dust dust = Dust.NewDustPerfect(planetPos, DustID.Enchanted_Pink, 
                            Vector2.Zero, 100, planetColor, 0.5f);
                        dust.noGravity = true;
                    }
                }
                
                // Central star glow
                if (Main.rand.NextBool(10))
                {
                    CustomParticles.GenericFlare(player.Center, FateCosmicVFX.FateWhite * 0.5f, 0.15f, 8);
                }
            }
        }

        public override void ModifyTooltips(System.Collections.Generic.List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "SummonBoost", "+22% summon damage")
            {
                OverrideColor = new Color(100, 200, 255)
            });
            
            tooltips.Add(new TooltipLine(Mod, "MinionSlot", "+1 max minion")
            {
                OverrideColor = new Color(120, 220, 255)
            });
            
            tooltips.Add(new TooltipLine(Mod, "KnockbackBoost", "+10% minion knockback")
            {
                OverrideColor = new Color(140, 200, 255)
            });
            
            tooltips.Add(new TooltipLine(Mod, "CosmicEmpower", "Minions periodically gain Cosmic Empowerment")
            {
                OverrideColor = FateCosmicVFX.FateDarkPink
            });
            
            tooltips.Add(new TooltipLine(Mod, "EmpowerEffect", "Empowered minion attacks deal 50% bonus damage")
            {
                OverrideColor = FateCosmicVFX.FateBrightRed
            });
            
            tooltips.Add(new TooltipLine(Mod, "Flavor", "'The universe itself serves at your command'")
            {
                OverrideColor = new Color(255, 150, 180)
            });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<FateResonantEnergy>(), 15)
                .AddIngredient(ModContent.ItemType<ResonantCoreOfFate>(), 5)
                .AddIngredient(ItemID.SoulofSight, 10)
                .AddIngredient(ItemID.FragmentStardust, 8)
                .AddIngredient(ItemID.LunarBar, 10)
                .AddTile(ModContent.TileType<MoonlightAnvilTile>())
                .Register();
        }
    }

    public class OrreryPlayer : ModPlayer
    {
        public bool hasOrrery = false;
        public int empowermentTimer = 0;
        public int empoweredMinionIndex = -1;
        
        private const int EmpowermentInterval = 300; // 5 seconds
        private const int EmpowermentDuration = 120; // 2 seconds of empowerment
        
        public override void ResetEffects()
        {
            hasOrrery = false;
        }

        public override void PostUpdate()
        {
            if (!hasOrrery) 
            {
                empowermentTimer = 0;
                empoweredMinionIndex = -1;
                return;
            }
            
            empowermentTimer++;
            
            // Every 5 seconds, empower a random minion
            if (empowermentTimer >= EmpowermentInterval)
            {
                empowermentTimer = 0;
                EmpowerRandomMinion();
            }
            
            // Show empowerment VFX on empowered minion
            if (empoweredMinionIndex >= 0 && empowermentTimer < EmpowermentDuration)
            {
                Projectile minion = Main.projectile[empoweredMinionIndex];
                if (minion.active && minion.owner == Player.whoAmI && minion.minion)
                {
                    // Cosmic glow around empowered minion
                    if (Main.rand.NextBool(3))
                    {
                        CustomParticles.GenericFlare(minion.Center + Main.rand.NextVector2Circular(15f, 15f),
                            FateCosmicVFX.FateDarkPink * 0.7f, 0.25f, 10);
                    }
                    
                    // Orbiting stars
                    if (Main.GameUpdateCount % 10 == 0)
                    {
                        float starAngle = Main.GameUpdateCount * 0.1f;
                        Vector2 starPos = minion.Center + starAngle.ToRotationVector2() * 20f;
                        CustomParticles.GenericFlare(starPos, FateCosmicVFX.FateWhite, 0.2f, 8);
                    }
                }
            }
        }

        private void EmpowerRandomMinion()
        {
            // Find all player's minions
            System.Collections.Generic.List<int> minionIndices = new();
            
            foreach (Projectile proj in Main.projectile)
            {
                if (proj.active && proj.owner == Player.whoAmI && proj.minion)
                {
                    minionIndices.Add(proj.whoAmI);
                }
            }
            
            if (minionIndices.Count == 0) return;
            
            // Pick random minion
            empoweredMinionIndex = minionIndices[Main.rand.Next(minionIndices.Count)];
            
            // Empowerment VFX burst
            Projectile minion = Main.projectile[empoweredMinionIndex];
            FateCosmicVFX.SpawnCosmicExplosion(minion.Center, 0.5f);
            CustomParticles.GlyphBurst(minion.Center, FateCosmicVFX.FateDarkPink, 3, 2f);
            CustomParticles.HaloRing(minion.Center, FateCosmicVFX.FateBrightRed, 0.3f, 12);
        }
    }

    public class OrreryGlobalProjectile : GlobalProjectile
    {
        public override void ModifyHitNPC(Projectile projectile, NPC target, ref NPC.HitModifiers modifiers)
        {
            if (projectile.owner < 0 || projectile.owner >= Main.maxPlayers) return;
            if (!projectile.minion) return;
            
            Player player = Main.player[projectile.owner];
            var modPlayer = player.GetModPlayer<OrreryPlayer>();
            
            if (!modPlayer.hasOrrery) return;
            
            // Check if this is the empowered minion during empowerment window
            if (projectile.whoAmI == modPlayer.empoweredMinionIndex && 
                modPlayer.empowermentTimer < 120) // Within empowerment duration
            {
                // +50% damage for empowered minion
                modifiers.FinalDamage *= 1.5f;
                
                // Extra VFX on empowered hit
                if (Main.rand.NextBool(2))
                {
                    CustomParticles.GenericFlare(target.Center, FateCosmicVFX.FateBrightRed, 0.4f, 12);
                }
            }
        }
    }
}
