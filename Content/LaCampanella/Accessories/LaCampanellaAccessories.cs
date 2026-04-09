using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;
using MagnumOpus.Common.Systems;
using MagnumOpus.Content.Common.Accessories;
using MagnumOpus.Content.LaCampanella.Debuffs;
using MagnumOpus.Content.LaCampanella.Bosses;
using MagnumOpus.Content.LaCampanella.ResonanceEnergies;
using MagnumOpus.Content.LaCampanella.HarmonicCores;
using MagnumOpus.Content.MoonlightSonata.CraftingStations;

namespace MagnumOpus.Content.LaCampanella.Accessories
{
    #region Chamber of Bellfire

    /// <summary>
    /// Chamber of Bellfire - La Campanella melee class accessory.
    /// 'Resonance Sliced' Melodic Attunement with fire/lava immunity.
    /// </summary>
    public class ChamberOfBellfire : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.width = 34;
            Item.height = 34;
            Item.accessory = true;
            Item.value = Item.sellPrice(gold: 35);
            Item.rare = ModContent.RarityType<LaCampanellaRarity>();
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var attunement = player.GetModPlayer<MelodicAttunementPlayer>();
            attunement.resonantBurnDmgBonus += 0.35f;
            attunement.critDmgBonusOnBurn += 0.025f;
            attunement.meleeAttunement = true;

            // Fire/lava immunity
            player.buffImmune[BuffID.OnFire] = true;
            player.buffImmune[BuffID.OnFire3] = true;
            player.lavaImmune = true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            Color lore = new Color(255, 140, 40);

            tooltips.Add(new TooltipLine(Mod, "Effect1", "'Resonance Sliced' Melodic Attunement"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "+35% increased Resonant Burn damage"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Hitting an enemy 10 times with melee damage while inflicted with Resonant Burn heals 10% HP"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Critical strike damage on Resonant Burn enemies increased by 2.5%"));
            tooltips.Add(new TooltipLine(Mod, "Effect5", "Immunity to fire debuffs and lava"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The chamber resonates with infernal flames'")
            {
                OverrideColor = lore
            });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<LaCampanellaResonantEnergy>(), 5)
                .AddIngredient(ModContent.ItemType<ResonantCoreOfLaCampanella>(), 5)
                .AddIngredient(ModContent.ItemType<ShardOfTheBurningTempo>(), 5)
                .AddIngredient(ItemID.SoulofMight, 12)
                .AddTile(ModContent.TileType<MoonlightAnvilTile>())
                .Register();
        }
    }

    #endregion

    #region Campanella's Pyre Medallion

    /// <summary>
    /// Campanella's Pyre Medallion - La Campanella summoner class accessory.
    /// 'Resonance Born' Melodic Attunement with 20% universal crit damage and fire/lava immunity.
    /// </summary>
    public class CampanellasPyreMedallion : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.width = 36;
            Item.height = 36;
            Item.accessory = true;
            Item.value = Item.sellPrice(gold: 40);
            Item.rare = ModContent.RarityType<LaCampanellaRarity>();
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var attunement = player.GetModPlayer<MelodicAttunementPlayer>();
            attunement.resonantBurnDmgBonus += 0.35f;
            attunement.critDmgAll += 0.20f;
            attunement.summonAttunement = true;

            // Fire/lava immunity
            player.buffImmune[BuffID.OnFire] = true;
            player.buffImmune[BuffID.OnFire3] = true;
            player.lavaImmune = true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            Color lore = new Color(255, 140, 40);

            tooltips.Add(new TooltipLine(Mod, "Effect1", "'Resonance Born' Melodic Attunement"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "+35% increased Resonant Burn damage"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Hitting an enemy 30 times with summon or whip damage while inflicted with Resonant Burn heals 10% HP"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Increased critical damage against all enemies by 20%"));
            tooltips.Add(new TooltipLine(Mod, "Effect5", "Immunity to fire debuffs and lava"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The pyre burns brightest for those who embrace the inferno'")
            {
                OverrideColor = lore
            });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<LaCampanellaResonantEnergy>(), 5)
                .AddIngredient(ModContent.ItemType<ResonantCoreOfLaCampanella>(), 5)
                .AddIngredient(ModContent.ItemType<ShardOfTheBurningTempo>(), 5)
                .AddIngredient(ItemID.SoulofFright, 5)
                .AddIngredient(ItemID.SoulofMight, 15)
                .AddTile(ModContent.TileType<MoonlightAnvilTile>())
                .Register();
        }
    }

    #endregion

    #region Symphony of the Blazing Sanctuary

    /// <summary>
    /// Symphony of the Blazing Sanctuary - La Campanella magic class accessory.
    /// 'Resonance Seared' Melodic Attunement with fire/lava immunity.
    /// </summary>
    public class SymphonyOfTheBlazingSanctuary : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.width = 40;
            Item.height = 40;
            Item.accessory = true;
            Item.value = Item.sellPrice(gold: 50);
            Item.rare = ModContent.RarityType<LaCampanellaRainbowRarity>();
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var attunement = player.GetModPlayer<MelodicAttunementPlayer>();
            attunement.resonantBurnDmgBonus += 0.35f;
            attunement.critDmgBonusOnBurn += 0.025f;
            attunement.magicAttunement = true;

            // Fire/lava immunity
            player.buffImmune[BuffID.OnFire] = true;
            player.buffImmune[BuffID.OnFire3] = true;
            player.lavaImmune = true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            Color lore = new Color(255, 140, 40);

            tooltips.Add(new TooltipLine(Mod, "Effect1", "'Resonance Seared' Melodic Attunement"));
            tooltips.Add(new TooltipLine(Mod, "Effect2", "+35% increased Resonant Burn damage"));
            tooltips.Add(new TooltipLine(Mod, "Effect3", "Hitting an enemy 15 times with magic damage while inflicted with Resonant Burn heals 10% HP"));
            tooltips.Add(new TooltipLine(Mod, "Effect4", "Critical strike damage on Resonant Burn enemies increased by 2.5%"));
            tooltips.Add(new TooltipLine(Mod, "Effect5", "Immunity to fire debuffs and lava"));
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Within the blazing sanctuary, even the flames sing prayers of protection'")
            {
                OverrideColor = lore
            });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<LaCampanellaResonantEnergy>(), 5)
                .AddIngredient(ModContent.ItemType<ResonantCoreOfLaCampanella>(), 5)
                .AddIngredient(ModContent.ItemType<ShardOfTheBurningTempo>(), 5)
                .AddIngredient(ItemID.SoulofSight, 5)
                .AddIngredient(ItemID.SoulofMight, 10)
                .AddTile(ModContent.TileType<MoonlightAnvilTile>())
                .Register();
        }
    }

    #endregion

    #region Infernal Bell of the Maestro
    
    /// <summary>
    /// Infernal Bell of the Maestro - Tier 4/Ultimate La Campanella accessory.
    /// Combines all previous effects, grants fire mastery, periodic Grand Tolling AOE,
    /// and transforms player into a walking inferno during combat.
    /// </summary>
    public class InfernalBellOfTheMaestro : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.width = 44;
            Item.height = 44;
            Item.accessory = true;
            Item.value = Item.sellPrice(gold: 75);
            Item.rare = ModContent.RarityType<LaCampanellaRainbowRarity>();
            Item.defense = 15;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            InfernalMaestroPlayer modPlayer = player.GetModPlayer<InfernalMaestroPlayer>();
            modPlayer.infernalMaestroEquipped = true;
            
            // All immunities
            player.buffImmune[BuffID.OnFire] = true;
            player.buffImmune[BuffID.OnFire3] = true;
            player.buffImmune[BuffID.Burning] = true;
            player.lavaImmune = true;
            player.fireWalk = true;
            
            // Major stat boosts
            player.GetDamage(DamageClass.Generic) += 0.2f;
            player.GetCritChance(DamageClass.Generic) += 20f;
            player.GetAttackSpeed(DamageClass.Generic) += 0.15f;
            player.statLifeMax2 += 60;
            player.lifeRegen += 8;
            player.moveSpeed += 0.15f;
            
            // Fire mastery - attacks deal fire damage
            modPlayer.fireInfusionActive = true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            Color campanellaOrange = new Color(255, 140, 40);
            Color campanellaGold = new Color(255, 200, 80);
            Color campanellaYellow = new Color(255, 220, 100);

            tooltips.Add(new TooltipLine(Mod, "Tier", "Ultimate La Campanella Accessory")
            {
                OverrideColor = campanellaOrange
            });
            tooltips.Add(new TooltipLine(Mod, "Defense", "+15 defense")
            {
                OverrideColor = campanellaGold
            });
            tooltips.Add(new TooltipLine(Mod, "Damage", "+20% damage")
            {
                OverrideColor = campanellaGold
            });
            tooltips.Add(new TooltipLine(Mod, "Crit", "+20% critical strike chance")
            {
                OverrideColor = campanellaGold
            });
            tooltips.Add(new TooltipLine(Mod, "Speed", "+15% attack speed and movement speed")
            {
                OverrideColor = campanellaGold
            });
            tooltips.Add(new TooltipLine(Mod, "MaxLife", "+60 maximum life")
            {
                OverrideColor = campanellaGold
            });
            tooltips.Add(new TooltipLine(Mod, "Regen", "+8 life regeneration")
            {
                OverrideColor = campanellaGold
            });
            tooltips.Add(new TooltipLine(Mod, "GrandTolling", "Grand Tolling: Powerful bell explosion every 5 seconds")
            {
                OverrideColor = campanellaYellow
            });
            tooltips.Add(new TooltipLine(Mod, "FireInfusion", "Fire Mastery: Critical hits deal 25% bonus fire damage")
            {
                OverrideColor = campanellaYellow
            });
            tooltips.Add(new TooltipLine(Mod, "ResonantToll", "+5% damage per Resonant Toll stack on target")
            {
                OverrideColor = campanellaYellow
            });
            tooltips.Add(new TooltipLine(Mod, "AuraDamage", "Infernal aura damages nearby enemies")
            {
                OverrideColor = campanellaYellow
            });
            tooltips.Add(new TooltipLine(Mod, "Immunities", "Immunity to On Fire!, Burning, and lava")
            {
                OverrideColor = campanellaOrange
            });
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The Maestro commands the inferno itself, conducting symphonies of destruction'")
            {
                OverrideColor = Color.Lerp(campanellaOrange, Color.Black, 0.3f)
            });
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<LaCampanellaResonantEnergy>(), 5)
                .AddIngredient(ModContent.ItemType<ResonantCoreOfLaCampanella>(), 5)
                .AddIngredient(ModContent.ItemType<ShardOfTheBurningTempo>(), 5)
                .AddIngredient(ItemID.SoulofSight, 5)
                .AddIngredient(ItemID.SoulofMight, 8)
                .AddTile(ModContent.TileType<MoonlightAnvilTile>())
                .Register();
        }
    }

    public class InfernalMaestroPlayer : ModPlayer
    {
        public bool infernalMaestroEquipped = false;
        public bool fireInfusionActive = false;
        private int grandTollingTimer = 0;
        private int combatTimer = 0;
        
        public override void ResetEffects()
        {
            infernalMaestroEquipped = false;
            fireInfusionActive = false;
        }

        public override void PostUpdate()
        {
            if (!infernalMaestroEquipped) return;
            
            // Track combat state
            if (combatTimer > 0)
            {
                combatTimer--;
            }
            
            // Grand Tolling timer
            grandTollingTimer++;
            if (grandTollingTimer >= 300) // Every 5 seconds
            {
                grandTollingTimer = 0;
                TriggerGrandTolling();
            }
            
            // Passive aura damage
            if (Main.GameUpdateCount % 15 == 0)
            {
                DamageNearbyEnemies();
            }
        }

        private void TriggerGrandTolling()
        {
            // === EPIC GRAND TOLLING SOUND STACK ===
            SoundEngine.PlaySound(SoundID.Item122 with { Pitch = -0.6f, Volume = 0.85f }, Player.Center);
            SoundEngine.PlaySound(SoundID.Item35 with { Pitch = Main.rand.NextFloat(-0.3f, 0f), Volume = 0.6f }, Player.Center);
            SoundEngine.PlaySound(SoundID.Item34 with { Pitch = Main.rand.NextFloat(0f, 0.3f), Volume = 0.4f }, Player.Center);
            SoundEngine.PlaySound(SoundID.DD2_BetsyFireballImpact with { Pitch = 0.1f, Volume = 0.35f }, Player.Center);
            SoundEngine.PlaySound(SoundID.Item14 with { Pitch = -0.2f, Volume = 0.3f }, Player.Center);
            
            // === MASSIVE GRAND TOLLING VISUAL EFFECTS WITH CUSTOM PARTICLES ===
            
            // === GRAND IMPACT - ULTIMATE EFFECTS ===
            
            // === CRESCENT WAVE EXPLOSION ===
            
            // Multiple massive halo rings
            // Black shadow rings
            
            // Massive radial flare burst with GRADIENT
            
            // Massive radial spark burst
            
            // Music notes explosion
            
            // VFX placeholder — to be replaced with unique effects later
            
            // AOE damage wave
            float waveRadius = 300f;
            foreach (NPC npc in Main.npc)
            {
                if (!npc.active || npc.friendly || !npc.CanBeChasedBy()) continue;
                
                float distance = Vector2.Distance(Player.Center, npc.Center);
                if (distance <= waveRadius)
                {
                    int damage = (int)(100 * (1f - distance / waveRadius)); // Damage falloff with distance
                    npc.SimpleStrikeNPC(damage, 0, false, 0f, null, false, 0f, true);
                    npc.GetGlobalNPC<ResonantTollNPC>().AddStacks(npc, 2);
                    
                    // Effects on each hit enemy
                }
            }
        }

        private void DamageNearbyEnemies()
        {
            float auraRadius = 150f;
            foreach (NPC npc in Main.npc)
            {
                if (!npc.active || npc.friendly || !npc.CanBeChasedBy()) continue;
                
                if (Vector2.Distance(Player.Center, npc.Center) <= auraRadius)
                {
                    npc.SimpleStrikeNPC(30, 0, false, 0f, null, false, 0f, true);
                    npc.GetGlobalNPC<ResonantTollNPC>().AddStacks(npc, 1);
                    
                    // Mark as in combat
                    combatTimer = 180; // 3 seconds
                    
                    // ========================================
                    // AURA BURN EFFECTS - Passive bell damage
                    // ========================================
                    
                    // Bell burn sound
                    SoundEngine.PlaySound(SoundID.Item34 with { Pitch = Main.rand.NextFloat(0.2f, 0.6f), Volume = 0.2f }, npc.Center);
                    
                    // Fire particle burst
                    
                    // Small halo ring
                    
                    // Flame glow particle
                    
                    Lighting.AddLight(npc.Center, 0.8f, 0.4f, 0.1f);
                }
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!infernalMaestroEquipped) return;
            
            // Mark as in combat
            combatTimer = 180;
            
            // Always apply Resonant Toll
            target.GetGlobalNPC<ResonantTollNPC>().AddStacks(target, 1);
            
            // === BLACK SMOKE SPARKLE - ULTIMATE SIGNATURE HIT! ===
            Vector2 hitDir = (target.Center - Player.Center).SafeNormalize(Vector2.UnitX);
            
            // Fire burst on crit
            if (hit.Crit)
            {
                // ========================================
                // CRITICAL HIT - Chainsaw bell crit sound!
                // ========================================
                
                // Layered crit sound
                SoundEngine.PlaySound(SoundID.Item35 with { Pitch = 0.5f, Volume = 0.5f }, target.Center);
                SoundEngine.PlaySound(SoundID.Item34 with { Pitch = Main.rand.NextFloat(0.1f, 0.4f), Volume = 0.35f }, target.Center);
                SoundEngine.PlaySound(SoundID.DD2_BetsyFireballShot with { Pitch = 0.3f, Volume = 0.25f }, target.Center);
                
                // Extra black smoke sparkle on crit!
                
                // Massive bloom burst
                
                // Halo rings on crit
                
                // Radial flares
                
                // Small screen shake on crit
                Player.GetModPlayer<ScreenShakePlayer>()?.AddShake(4f, 6);
                
                Lighting.AddLight(target.Center, 1.4f, 0.7f, 0.2f);
                
                // Extra fire damage
                target.SimpleStrikeNPC(damageDone / 4, 0, false, 0f, null, false, 0f, true);
            }
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            if (!infernalMaestroEquipped) return;
            
            // Bonus damage to enemies with Resonant Toll
            int stacks = target.GetGlobalNPC<ResonantTollNPC>().Stacks;
            if (stacks > 0)
            {
                modifiers.FinalDamage += 0.05f * stacks; // +5% per stack
            }
        }
    }
    
    #endregion
}
