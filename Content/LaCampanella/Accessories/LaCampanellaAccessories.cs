using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Content.LaCampanella.Debuffs;
using MagnumOpus.Content.LaCampanella.Bosses;
using MagnumOpus.Content.LaCampanella.ResonanceEnergies;
using MagnumOpus.Content.LaCampanella.HarmonicCores;
using MagnumOpus.Content.MoonlightSonata.CraftingStations;

namespace MagnumOpus.Content.LaCampanella.Accessories
{
    #region Chamber of Bellfire
    
    /// <summary>
    /// Chamber of Bellfire - Tier 1 La Campanella accessory.
    /// Grants fire resistance, bellfire aura that damages nearby enemies,
    /// and causes attacks to occasionally trigger bell explosions.
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
            Item.defense = 6;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            ChamberOfBellfirePlayer modPlayer = player.GetModPlayer<ChamberOfBellfirePlayer>();
            modPlayer.chamberOfBellfireEquipped = true;
            
            // Fire resistance
            player.buffImmune[BuffID.OnFire] = true;
            player.buffImmune[BuffID.OnFire3] = true;
            player.lavaImmune = true;
            
            // Damage boost to burning enemies
            player.GetDamage(DamageClass.Generic) += 0.12f;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            Color flameOrange = new Color(255, 140, 40);
            Color smokyBlack = new Color(50, 40, 45);
            
            tooltips.Add(new TooltipLine(Mod, "Stats", "+12% all damage, +6 defense")
            {
                OverrideColor = flameOrange
            });
            
            tooltips.Add(new TooltipLine(Mod, "Aura", "Bellfire aura damages nearby enemies (25 damage every 0.5s)")
            {
                OverrideColor = flameOrange
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effect", "Every 10 hits triggers a bell explosion")
            {
                OverrideColor = smokyBlack
            });
            
            tooltips.Add(new TooltipLine(Mod, "Immunities", "Immunity to fire debuffs and lava")
            {
                OverrideColor = flameOrange
            });
            
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The chamber resonates with infernal flames'")
            {
                OverrideColor = new Color(200, 150, 100)
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

    public class ChamberOfBellfirePlayer : ModPlayer
    {
        public bool chamberOfBellfireEquipped = false;
        private int bellExplosionTimer = 0;
        private int auraDamageTimer = 0;
        
        public override void ResetEffects()
        {
            chamberOfBellfireEquipped = false;
        }

        public override void PostUpdate()
        {
            if (!chamberOfBellfireEquipped) return;
            
            // Bellfire aura damage
            auraDamageTimer++;
            if (auraDamageTimer >= 30) // Every 0.5 seconds
            {
                auraDamageTimer = 0;
                DamageNearbyEnemies();
            }
        }

        private void DamageNearbyEnemies()
        {
            float auraRadius = 120f;
            foreach (NPC npc in Main.npc)
            {
                if (!npc.active || npc.friendly || !npc.CanBeChasedBy()) continue;
                
                if (Vector2.Distance(Player.Center, npc.Center) <= auraRadius)
                {
                    // Small fire damage
                    npc.SimpleStrikeNPC(25, 0, false, 0f, null, false, 0f, true);
                    npc.GetGlobalNPC<ResonantTollNPC>().AddStacks(npc, 1);
                    
                    // Fire effect
                    for (int i = 0; i < 3; i++)
                    {
                        Dust flame = Dust.NewDustPerfect(npc.Center + Main.rand.NextVector2Circular(npc.width * 0.3f, npc.height * 0.3f),
                            DustID.Torch, Main.rand.NextVector2Circular(2f, 2f), 100, new Color(255, 100, 0), 1.2f);
                        flame.noGravity = true;
                    }
                }
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!chamberOfBellfireEquipped) return;
            
            // === BLACK SMOKE SPARKLE - SIGNATURE HIT ON ACCESSORY! ===
            Vector2 hitDir = (target.Center - Player.Center).SafeNormalize(Vector2.UnitX);
            
            // Chance for bell explosion
            bellExplosionTimer++;
            if (bellExplosionTimer >= 10) // Every 10 hits
            {
                bellExplosionTimer = 0;
                TriggerBellExplosion(target.Center);
            }
        }

        private void TriggerBellExplosion(Vector2 position)
        {
            // === GUTURAL BELL EXPLOSION SOUNDS ===
            SoundEngine.PlaySound(SoundID.Item35 with { Pitch = Main.rand.NextFloat(0.2f, 0.5f), Volume = 0.55f }, position);
            SoundEngine.PlaySound(SoundID.Item34 with { Pitch = Main.rand.NextFloat(0.1f, 0.4f), Volume = 0.35f }, position);
            
            // === MASSIVE BELL EXPLOSION EFFECTS WITH CUSTOM PARTICLES ===
            
            // === RADIAL FLARE BURST with GRADIENT ===
            
            // === PRISMATIC SPARKLES ===
            
            // Halo rings
            
            // Radial flares
            
            // Music notes
            
            // Screen shake
            Player.GetModPlayer<ScreenShakePlayer>()?.AddShake(4f, 8);
            
            Lighting.AddLight(position, 1f, 0.5f, 0.15f);
            
            // AOE damage
            float explosionRadius = 100f;
            foreach (NPC npc in Main.npc)
            {
                if (!npc.active || npc.friendly || !npc.CanBeChasedBy()) continue;
                
                if (Vector2.Distance(position, npc.Center) <= explosionRadius)
                {
                    npc.SimpleStrikeNPC(75, 0, false, 0f, null, false, 0f, true);
                    npc.GetGlobalNPC<ResonantTollNPC>().AddStacks(npc, 1);
                    
                    // Hit effect on each enemy with flares
                    // Flares around enemy
                }
            }
        }
    }
    
    #endregion

    #region Campanella's Pyre Medallion
    
    /// <summary>
    /// Campanella's Pyre Medallion - La Campanella accessory.
    /// Enhances Resonant Toll debuff, grants crit chance against burning enemies,
    /// and leaves a trail of flame when dashing.
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
            CampanellasPyreMedallionPlayer modPlayer = player.GetModPlayer<CampanellasPyreMedallionPlayer>();
            modPlayer.pyreMedallionEquipped = true;
            
            // Crit chance boost
            player.GetCritChance(DamageClass.Generic) += 15f;
            
            // Attack speed
            player.GetAttackSpeed(DamageClass.Generic) += 0.08f;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            Color campanellaOrange = new Color(255, 140, 40);
            Color campanellaGold = new Color(255, 200, 80);
            Color campanellaYellow = new Color(255, 220, 100);

            tooltips.Add(new TooltipLine(Mod, "Tier", "Tier 2 La Campanella Accessory")
            {
                OverrideColor = campanellaOrange
            });
            tooltips.Add(new TooltipLine(Mod, "Stats", "+15% critical strike chance")
            {
                OverrideColor = campanellaGold
            });
            tooltips.Add(new TooltipLine(Mod, "Speed", "+8% attack speed")
            {
                OverrideColor = campanellaGold
            });
            tooltips.Add(new TooltipLine(Mod, "DamageBonus", "+20% damage against enemies with Resonant Toll stacks")
            {
                OverrideColor = campanellaYellow
            });
            tooltips.Add(new TooltipLine(Mod, "FlameTrail", "Leaves a trail of fire when dashing")
            {
                OverrideColor = campanellaOrange
            });
            tooltips.Add(new TooltipLine(Mod, "Lore", "'The pyre burns brightest for those who embrace the inferno'")
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
                .AddIngredient(ItemID.SoulofFright, 5)
                .AddIngredient(ItemID.SoulofMight, 15)
                .AddTile(ModContent.TileType<MoonlightAnvilTile>())
                .Register();
        }
    }

    public class CampanellasPyreMedallionPlayer : ModPlayer
    {
        public bool pyreMedallionEquipped = false;
        private Vector2 lastPosition;
        
        public override void ResetEffects()
        {
            pyreMedallionEquipped = false;
        }

        public override void PostUpdate()
        {
            if (!pyreMedallionEquipped)
            {
                lastPosition = Player.Center;
                return;
            }
            
            // Flame trail when moving fast (dashing)
            float speed = Vector2.Distance(Player.Center, lastPosition);
            if (speed > 10f)
            {
                for (int i = 0; i < 2; i++)
                {
                    Vector2 trailPos = Vector2.Lerp(lastPosition, Player.Center, i / 2f);
                    
                    Dust flame = Dust.NewDustPerfect(trailPos + Main.rand.NextVector2Circular(5f, 5f),
                        DustID.Torch, Vector2.Zero, 100, new Color(255, 100, 0), 1.5f);
                    flame.noGravity = true;
                    flame.fadeIn = 0.3f;
                }
            }
            
            lastPosition = Player.Center;
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            if (!pyreMedallionEquipped) return;
            
            // Extra damage to enemies with Resonant Toll
            if (target.GetGlobalNPC<ResonantTollNPC>().Stacks > 0)
            {
                modifiers.FinalDamage += 0.2f; // +20% damage to afflicted enemies
            }
        }
    }
    
    #endregion

    #region Symphony of the Blazing Sanctuary
    
    /// <summary>
    /// Symphony of the Blazing Sanctuary - Tier 3 La Campanella accessory.
    /// Creates protective bell barrier on low health, grants regen near fire,
    /// and killing enemies creates healing fire pillars.
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
            Item.defense = 10;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            BlazingSanctuaryPlayer modPlayer = player.GetModPlayer<BlazingSanctuaryPlayer>();
            modPlayer.blazingSanctuaryEquipped = true;
            
            // Life regen near fire sources (lava, fire blocks, etc.)
            player.lifeRegen += 4;
            
            // Max life boost
            player.statLifeMax2 += 40;
            
            // Defense when below half health
            if (player.statLife < player.statLifeMax2 / 2)
            {
                player.statDefense += 15;
            }
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            Color campanellaOrange = new Color(255, 140, 40);
            Color campanellaGold = new Color(255, 200, 80);
            Color campanellaYellow = new Color(255, 220, 100);

            tooltips.Add(new TooltipLine(Mod, "Tier", "Tier 3 La Campanella Accessory")
            {
                OverrideColor = campanellaOrange
            });
            tooltips.Add(new TooltipLine(Mod, "Defense", "+10 defense (+15 when below 50% health)")
            {
                OverrideColor = campanellaGold
            });
            tooltips.Add(new TooltipLine(Mod, "MaxLife", "+40 maximum life")
            {
                OverrideColor = campanellaGold
            });
            tooltips.Add(new TooltipLine(Mod, "Regen", "+4 life regeneration")
            {
                OverrideColor = campanellaGold
            });
            tooltips.Add(new TooltipLine(Mod, "BellBarrier", "Protective bell barrier when below 30% health (70% damage reduction)")
            {
                OverrideColor = campanellaYellow
            });
            tooltips.Add(new TooltipLine(Mod, "Cooldown", "Bell barrier has a 30 second cooldown")
            {
                OverrideColor = Color.Gray
            });
            tooltips.Add(new TooltipLine(Mod, "HealingPillar", "Killing enemies creates healing fire pillars (+15 HP when nearby)")
            {
                OverrideColor = campanellaYellow
            });
            tooltips.Add(new TooltipLine(Mod, "BellKnockback", "Bell barrier knocks back enemies and applies Resonant Toll")
            {
                OverrideColor = campanellaYellow
            });
            tooltips.Add(new TooltipLine(Mod, "Lore", "'Within the blazing sanctuary, even the flames sing prayers of protection'")
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
                .AddIngredient(ItemID.SoulofMight, 10)
                .AddTile(ModContent.TileType<MoonlightAnvilTile>())
                .Register();
        }
    }

    public class BlazingSanctuaryPlayer : ModPlayer
    {
        public bool blazingSanctuaryEquipped = false;
        private int barrierCooldown = 0;
        private bool barrierTriggeredThisHit = false;
        
        public override void ResetEffects()
        {
            blazingSanctuaryEquipped = false;
            barrierTriggeredThisHit = false;
        }

        public override void PostUpdate()
        {
            if (barrierCooldown > 0)
                barrierCooldown--;
        }

        public override void ModifyHurt(ref Player.HurtModifiers modifiers)
        {
            if (!blazingSanctuaryEquipped) return;
            
            // Bell barrier on low health - 70% damage reduction
            if (Player.statLife < Player.statLifeMax2 * 0.3f && barrierCooldown <= 0)
            {
                modifiers.FinalDamage *= 0.3f; // 70% damage reduction
                barrierTriggeredThisHit = true;
            }
        }

        public override void OnHurt(Player.HurtInfo info)
        {
            if (!blazingSanctuaryEquipped) return;
            
            // Trigger barrier visual/effects after damage is calculated
            if (barrierTriggeredThisHit && barrierCooldown <= 0)
            {
                TriggerBellBarrier();
                barrierCooldown = 1800; // 30 second cooldown
            }
        }

        private void TriggerBellBarrier()
        {
            // === EPIC BELL BARRIER SOUND STACK ===
            SoundEngine.PlaySound(SoundID.Item122 with { Pitch = -0.4f, Volume = 0.85f }, Player.Center);
            SoundEngine.PlaySound(SoundID.Item35 with { Pitch = Main.rand.NextFloat(-0.1f, 0.2f), Volume = 0.65f }, Player.Center);
            SoundEngine.PlaySound(SoundID.Item34 with { Pitch = Main.rand.NextFloat(0.1f, 0.4f), Volume = 0.45f }, Player.Center);
            SoundEngine.PlaySound(SoundID.DD2_BetsyFireballImpact with { Pitch = 0.2f, Volume = 0.35f }, Player.Center);
            
            // === MASSIVE BARRIER VISUAL EFFECTS WITH CUSTOM PARTICLES ===
            
            // === GRAND IMPACT WITH ALL EFFECTS ===
            
            // === CRESCENT WAVE BARRIER ===
            
            // Multiple massive halo rings
            // Black shadow rings
            
            // Radial flare burst with GRADIENT
            
            // Ring of fire around player
            for (int i = 0; i < 36; i++)
            {
                float angle = MathHelper.TwoPi * i / 36f;
                Vector2 pos = Player.Center + angle.ToRotationVector2() * 80f;
                
                
                Dust flame = Dust.NewDustPerfect(pos, DustID.Torch,
                    angle.ToRotationVector2().RotatedBy(MathHelper.PiOver2) * 3f, 100, new Color(255, 100, 0), 2f);
                flame.noGravity = true;
            }
            
            // Music notes explosion
            
            // Screen shake
            Player.GetModPlayer<ScreenShakePlayer>()?.AddShake(10f, 18);
            
            Lighting.AddLight(Player.Center, 2f, 1f, 0.35f);
            
            // Knockback nearby enemies
            foreach (NPC npc in Main.npc)
            {
                if (!npc.active || npc.friendly || !npc.CanBeChasedBy()) continue;
                
                if (Vector2.Distance(Player.Center, npc.Center) <= 150f)
                {
                    Vector2 knockback = (npc.Center - Player.Center).SafeNormalize(Vector2.UnitX) * 15f;
                    npc.velocity += knockback;
                    npc.GetGlobalNPC<ResonantTollNPC>().AddStacks(npc, 3);
                    
                    // Hit effects on each enemy with all effects
                }
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!blazingSanctuaryEquipped) return;
            
            // === BLACK SMOKE SPARKLE - SIGNATURE HIT ON ACCESSORY! ===
            Vector2 hitDir = (target.Center - Player.Center).SafeNormalize(Vector2.UnitX);
            
            // Create healing fire pillar on kill
            if (target.life <= 0 && !target.SpawnedFromStatue)
            {
                SpawnHealingPillar(target.Center);
            }
        }

        private void SpawnHealingPillar(Vector2 position)
        {
            // === BELL CHIME SOUND ===
            SoundEngine.PlaySound(SoundID.Item35 with { Pitch = Main.rand.NextFloat(0.3f, 0.6f), Volume = 0.45f }, position);
            SoundEngine.PlaySound(SoundID.Item34 with { Pitch = Main.rand.NextFloat(0.2f, 0.5f), Volume = 0.3f }, position);
            
            // === FIRE PILLAR EFFECTS ===
            
            // Visual pillar with custom particles
            for (int i = 0; i < 20; i++)
            {
                Vector2 pillarPos = position + new Vector2(0, -i * 10f);
                
                Dust flame = Dust.NewDustPerfect(pillarPos,
                    DustID.Torch, new Vector2(Main.rand.NextFloat(-1f, 1f), -2f), 100, new Color(255, 150, 50), 2f);
                flame.noGravity = true;
            }
            
            // Halo rings at base
            
            // Sparks rising
            
            // Heal player if nearby
            if (Vector2.Distance(Player.Center, position) <= 200f)
            {
                Player.Heal(15);
                
                // Healing effect
                CombatText.NewText(Player.Hitbox, new Color(255, 150, 100), 15, false, true);
            }
            
            Lighting.AddLight(position, 0.8f, 0.4f, 0.1f);
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
