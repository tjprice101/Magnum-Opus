using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using MagnumOpus.Common;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Content.EnigmaVariations.Debuffs;

namespace MagnumOpus.Content.EnigmaVariations.Accessories
{
    /// <summary>
    /// Alchemical Paradox - Ranger Accessory
    /// 
    /// "Paradox Shots" - Every 4th ranged projectile becomes a "Paradox Bolt" that:
    /// - Splits into 2-3 smaller projectiles on hit
    /// - Applies "Paradox" debuff to enemies
    /// - Enemies with Paradox take damage over time
    /// - Enemies explode on death, damaging nearby foes
    /// Additionally, +8% ranged critical strike chance.
    /// 
    /// Theme: The contradictory nature of existence,
    /// where destruction begets more destruction.
    /// </summary>
    public class AlchemicalParadox : ModItem
    {
        // Enigma color palette
        private static readonly Color EnigmaPurple = new Color(140, 60, 200);
        private static readonly Color EnigmaGreenFlame = new Color(50, 220, 100);
        private static readonly Color EnigmaDeepPurple = new Color(80, 20, 120);
        
        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.accessory = true;
            Item.value = Item.buyPrice(platinum: 3);
            Item.rare = ModContent.RarityType<EnigmaVariationsRarity>();
        }
        
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var modPlayer = player.GetModPlayer<EnigmaAccessoryPlayer>();
            modPlayer.hasAlchemicalParadox = true;
            
            // Ambient visual effects
            if (!hideVisual)
            {
                // Alchemical bubbles/particles around the player - every 40 frames
                if (Main.GameUpdateCount % 40 == 0)
                {
                    Vector2 offset = Main.rand.NextVector2Circular(30f, 35f);
                    float progress = Main.rand.NextFloat();
                    Color bubbleColor = EnigmaAccessoryPlayer.GetEnigmaGradient(progress);
                    
                    // Rising bubbles
                    Vector2 vel = new Vector2(Main.rand.NextFloat(-0.2f, 0.2f), -0.8f - Main.rand.NextFloat(0.4f));
                    CustomParticles.GenericGlow(player.Center + offset, bubbleColor * 0.4f, 0.18f, 25);
                }
                
                // Shot counter visual - building intensity
                int shotsMade = modPlayer.rangedShotCounter;
                if (shotsMade > 0)
                {
                    // Intensity increases as we approach the 4th shot
                    float intensity = (float)shotsMade / 4f;
                    
                    // Orbiting paradox particles - frame-based, every 25 frames
                    if (Main.GameUpdateCount % 25 == 0)
                    {
                        float orbitAngle = Main.GameUpdateCount * 0.04f;
                        for (int i = 0; i < shotsMade; i++)
                        {
                            float particleAngle = orbitAngle + MathHelper.TwoPi * i / 4f;
                            Vector2 particlePos = player.Center + particleAngle.ToRotationVector2() * (25f + intensity * 10f);
                            
                            Color particleColor = EnigmaAccessoryPlayer.GetEnigmaGradient((float)i / 4f);
                            CustomParticles.GenericFlare(particlePos, particleColor * 0.6f, 0.2f + intensity * 0.15f, 10);
                        }
                    }
                    
                    // At 3 shots, start showing more dramatic effects - every 20 frames
                    if (shotsMade >= 3 && Main.GameUpdateCount % 20 == 0)
                    {
                        // Swirling void energy
                        Vector2 voidPos = player.Center + Main.rand.NextVector2Circular(25f, 25f);
                        CustomParticles.GenericFlare(voidPos, EnigmaGreenFlame * 0.7f, 0.3f, 12);
                        
                        // Ready indicator glyph
                        CustomParticles.Glyph(player.Center + Main.rand.NextVector2Circular(30f, 30f), 
                            EnigmaPurple * 0.5f, 0.22f, -1);
                    }
                    
                    // Pulsing light
                    Lighting.AddLight(player.Center, EnigmaAccessoryPlayer.GetEnigmaGradient(intensity).ToVector3() * 0.25f * intensity);
                }
                
                // Mark ranged projectiles with paradox visuals when trigger is ready
                if (modPlayer.paradoxProcCooldown > 0)
                {
                    // Just fired a paradox shot - trail all ranged projectiles briefly
                    foreach (Projectile proj in Main.projectile)
                    {
                        if (proj.active && proj.owner == player.whoAmI && proj.DamageType == DamageClass.Ranged 
                            && proj.timeLeft > proj.extraUpdates * 60 - 10) // Only very new projectiles
                        {
                            // Frame-based instead of random every frame
                            if (proj.timeLeft % 5 == 0)
                            {
                                CustomParticles.GlyphTrail(proj.Center, proj.velocity, EnigmaGreenFlame * 0.6f, 0.25f);
                                CustomParticles.GenericFlare(proj.Center, EnigmaPurple * 0.5f, 0.2f, 8);
                            }
                        }
                    }
                }
                
                // Enigma aura
                ThemedParticles.EnigmaAura(player.Center, 30f, 0.25f);
            }
        }
        
        public override void ModifyTooltips(System.Collections.Generic.List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "ParadoxHeader", "Paradox Shots:")
            {
                OverrideColor = EnigmaGreenFlame
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect1", "Every 4th ranged attack becomes a Paradox Bolt"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "Paradox Bolts split into 2-3 projectiles on hit"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Applies Paradox debuff (damage over time)"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Afflicted enemies explode on death"));
            tooltips.Add(new TooltipLine(Mod, "Crit", "+8% ranged critical strike chance"));
            
            tooltips.Add(new TooltipLine(Mod, "Lore", "'In contradiction, truth unravels'")
            {
                OverrideColor = EnigmaPurple
            });
        }
        
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<ResonanceEnergies.EnigmaResonantEnergy>(), 15)
                .AddIngredient(ModContent.ItemType<HarmonicCores.HarmonicCoreOfEnigma>(), 1)
                .AddIngredient(ItemID.RangerEmblem)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
    
    /// <summary>
    /// Global projectile to handle Paradox Bolt behavior
    /// </summary>
    public class AlchemicalParadoxGlobalProjectile : GlobalProjectile
    {
        public override bool InstancePerEntity => true;
        
        public bool isParadoxBolt = false;
        
        private static readonly Color EnigmaPurple = new Color(140, 60, 200);
        private static readonly Color EnigmaGreenFlame = new Color(50, 220, 100);
        
        public override void OnSpawn(Projectile projectile, IEntitySource source)
        {
            if (source is EntitySource_ItemUse_WithAmmo itemSource)
            {
                Player player = Main.player[projectile.owner];
                var modPlayer = player.GetModPlayer<EnigmaAccessoryPlayer>();
                
                // Mark as paradox bolt if we just triggered the effect
                if (modPlayer.hasAlchemicalParadox && modPlayer.paradoxProcCooldown > 0 && 
                    projectile.DamageType == DamageClass.Ranged)
                {
                    isParadoxBolt = true;
                }
            }
        }
        
        public override void AI(Projectile projectile)
        {
            if (isParadoxBolt && projectile.active)
            {
                // Paradox trail effect
                if (Main.rand.NextBool(2))
                {
                    float progress = Main.rand.NextFloat();
                    Color trailColor = EnigmaAccessoryPlayer.GetEnigmaGradient(progress);
                    CustomParticles.GenericGlow(projectile.Center, trailColor * 0.5f, 0.2f, 12);
                }
                
                // Occasional glyph in trail
                if (Main.rand.NextBool(6))
                {
                    CustomParticles.GlyphTrail(projectile.Center, projectile.velocity, EnigmaPurple * 0.5f, 0.18f);
                }
                
                // Green flame sparkles
                if (Main.rand.NextBool(5))
                {
                    CustomParticles.GenericFlare(projectile.Center + Main.rand.NextVector2Circular(8f, 8f), 
                        EnigmaGreenFlame * 0.6f, 0.2f, 10);
                }
                
                Lighting.AddLight(projectile.Center, EnigmaGreenFlame.ToVector3() * 0.3f);
            }
        }
        
        public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (isParadoxBolt && projectile.owner >= 0 && projectile.owner < Main.maxPlayers)
            {
                Player player = Main.player[projectile.owner];
                
                // Apply Paradox debuff
                var paradoxNPC = target.GetGlobalNPC<ParadoxBrandNPC>();
                paradoxNPC.AddParadoxStack(target, 2);
                
                // Split into smaller projectiles
                int splitCount = Main.rand.Next(2, 4); // 2-3 projectiles
                for (int i = 0; i < splitCount; i++)
                {
                    float splitAngle = MathHelper.TwoPi * i / splitCount + Main.rand.NextFloat(-0.3f, 0.3f);
                    Vector2 splitVel = splitAngle.ToRotationVector2() * Main.rand.NextFloat(8f, 12f);
                    
                    // Find a nearby enemy to home toward (slight homing)
                    NPC nearestEnemy = null;
                    float nearestDist = 300f;
                    foreach (NPC npc in Main.npc)
                    {
                        if (npc.active && !npc.friendly && npc.whoAmI != target.whoAmI)
                        {
                            float dist = npc.Distance(target.Center);
                            if (dist < nearestDist)
                            {
                                nearestDist = dist;
                                nearestEnemy = npc;
                            }
                        }
                    }
                    
                    if (nearestEnemy != null)
                    {
                        Vector2 toEnemy = (nearestEnemy.Center - target.Center).SafeNormalize(Vector2.UnitX);
                        splitVel = Vector2.Lerp(splitVel, toEnemy * 10f, 0.5f);
                    }
                    
                    // Spawn split projectile (using generic magic bolt)
                    Projectile splitProj = Projectile.NewProjectileDirect(
                        projectile.GetSource_FromThis(),
                        target.Center,
                        splitVel,
                        ProjectileID.CursedFlameFriendly, // Visual placeholder
                        projectile.damage / 3,
                        projectile.knockBack / 2f,
                        player.whoAmI
                    );
                    
                    // Make it Enigma-themed
                    splitProj.DamageType = DamageClass.Ranged;
                    splitProj.friendly = true;
                    splitProj.hostile = false;
                    splitProj.tileCollide = true;
                    splitProj.timeLeft = 120;
                    
                    // Visual for split
                    CustomParticles.GenericFlare(target.Center, EnigmaGreenFlame, 0.4f, 12);
                }
                
                // Impact burst
                CustomParticles.GlyphBurst(target.Center, EnigmaPurple, 6, 4f);
                CustomParticles.GenericFlare(target.Center, EnigmaGreenFlame, 0.6f, 18);
                ThemedParticles.EnigmaMusicNotes(target.Center, 3, 20f);
                
                // Don't split again (only original paradox bolts split)
                isParadoxBolt = false;
            }
        }
    }
}
