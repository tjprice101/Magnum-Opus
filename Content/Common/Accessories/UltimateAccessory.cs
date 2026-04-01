using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Content.Fate;
using MagnumOpus.Content.Fate.ResonantWeapons;
using MagnumOpus.Content.Fate.ResonantWeapons.CodaOfAnnihilation;

namespace MagnumOpus.Content.Common.Accessories
{
    /// <summary>
    /// Coda of Absolute Harmony - THE ULTIMATE ACCESSORY
    /// Phase 5 Ultimate: All themes, all seasons, all Fate power combined
    /// Requires sacrificing the Coda of Annihilation weapon itself
    /// The pinnacle of MagnumOpus accessory progression
    /// </summary>
    public class CodaOfAbsoluteHarmony : ModItem
    {
        // All theme colors
        private static readonly Color MoonlightPurple = new Color(138, 43, 226);
        private static readonly Color MoonlightSilver = new Color(220, 220, 235);
        private static readonly Color EroicaGold = new Color(255, 200, 80);
        private static readonly Color EroicaScarlet = new Color(200, 50, 50);
        private static readonly Color CampanellaOrange = new Color(255, 140, 40);
        private static readonly Color EnigmaPurple = new Color(140, 60, 200);
        private static readonly Color EnigmaGreen = new Color(50, 220, 100);
        private static readonly Color SwanWhite = new Color(255, 255, 255);
        private static readonly Color FateDarkPink = new Color(200, 80, 120);
        private static readonly Color FateBrightRed = new Color(255, 60, 80);
        
        // Season colors
        private static readonly Color SpringPink = new Color(255, 183, 197);
        private static readonly Color SummerGold = new Color(255, 180, 50);
        private static readonly Color AutumnOrange = new Color(200, 100, 30);
        private static readonly Color WinterBlue = new Color(150, 220, 255);
        
        public override void SetDefaults()
        {
            Item.width = 42;
            Item.height = 42;
            Item.accessory = true;
            Item.value = Item.sellPrice(platinum: 10);
            Item.rare = ModContent.RarityType<FateRarity>();
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var modPlayer = player.GetModPlayer<CodaOfAbsoluteHarmonyPlayer>();
            modPlayer.codaEquipped = true;
            
            bool isNight = !Main.dayTime;
            
            // ============================================
            // === THE ULTIMATE ACCESSORY - ALL BONUSES ===
            // ============================================
            
            // === GLOBAL STATS ===
            player.GetDamage(DamageClass.Generic) += 0.40f;
            player.GetCritChance(DamageClass.Generic) += 30;
            player.GetAttackSpeed(DamageClass.Generic) += 0.20f;
            player.statDefense += 35;
            player.lifeRegen += 15;
            player.manaRegen += 10;
            player.endurance += 0.18f;
            player.moveSpeed += 0.30f;
            
            // === MOONLIGHT SONATA (Night power) ===
            if (isNight)
            {
                player.GetDamage(DamageClass.Generic) += 0.25f;
                player.GetCritChance(DamageClass.Generic) += 25;
                player.statDefense += 20;
            }
            
            // === EROICA (Melee mastery) ===
            player.GetDamage(DamageClass.Melee) += 0.25f;
            player.GetAttackSpeed(DamageClass.Melee) += 0.22f;
            player.GetCritChance(DamageClass.Melee) += 15;
            
            // === LA CAMPANELLA (Magic mastery) ===
            player.GetDamage(DamageClass.Magic) += 0.30f;
            player.GetCritChance(DamageClass.Magic) += 15;
            player.manaCost -= 0.25f;
            
            // === ENIGMA (Chaos power) ===
            player.GetDamage(DamageClass.Generic) += 0.22f;
            
            // === SWAN LAKE (Grace) ===
            player.GetDamage(DamageClass.Generic) += 0.22f;
            player.moveSpeed += 0.30f;
            player.maxRunSpeed *= 1.3f;
            
            // === RANGED (Constellation Compass) ===
            player.GetDamage(DamageClass.Ranged) += 0.30f;
            player.GetCritChance(DamageClass.Ranged) += 18;
            
            // === SUMMON (Orrery) ===
            player.maxMinions += 6;
            player.GetDamage(DamageClass.Summon) += 0.30f;
            
            // === MOBILITY (Event Horizon) ===
            player.wingTimeMax += 120;
            player.noFallDmg = true;
            player.runAcceleration *= 1.5f;
            
            // === ELEMENTAL ===
            player.magmaStone = true;
            player.frostBurn = true;
            player.thorns = 2f;
            
            // === IMMUNITIES ===
            player.buffImmune[BuffID.OnFire] = true;
            player.buffImmune[BuffID.Burning] = true;
            player.buffImmune[BuffID.CursedInferno] = true;
            player.buffImmune[BuffID.ShadowFlame] = true;
            player.buffImmune[BuffID.Frozen] = true;
            player.buffImmune[BuffID.Frostburn] = true;
            player.buffImmune[BuffID.Chilled] = true;
            player.buffImmune[BuffID.Confused] = true;
            player.buffImmune[BuffID.Poisoned] = true;
            player.buffImmune[BuffID.Venom] = true;
            player.buffImmune[BuffID.Slow] = true;
            player.buffImmune[BuffID.Darkness] = true;
            player.buffImmune[BuffID.Silenced] = true;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<OpusOfFourMovements>()
                .AddIngredient<CosmicWardensRegalia>()
                .AddIngredient<SpringsMoonlitGarden>()
                .AddIngredient<SummersInfernalPeak>()
                .AddIngredient<WintersEnigmaticSilence>()
                .AddIngredient<CodaOfAnnihilationItem>() // CONSUMED
                .AddTile(TileID.LunarCraftingStation)
                .Register();
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            Color ultimateGold = new Color(255, 215, 0);
            Color cosmicPurple = new Color(200, 100, 255);
            Color softGold = new Color(255, 255, 200);
            
            tooltips.Add(new TooltipLine(Mod, "Ultimate1", "The ULTIMATE musical accessory - the grand finale of all symphonies")
            {
                OverrideColor = ultimateGold
            });
            
            tooltips.Add(new TooltipLine(Mod, "Ultimate2", "Combines Opus of Four Movements, Cosmic Warden's Regalia, all Season-Theme Hybrids, and the Coda of Annihilation")
            {
                OverrideColor = cosmicPurple
            });
            
            tooltips.Add(new TooltipLine(Mod, "Stats1", "+84% all damage, +30 crit chance, +20% attack speed, +35 defense")
            {
                OverrideColor = ultimateGold
            });
            
            tooltips.Add(new TooltipLine(Mod, "Stats2", "+15 life regen, +10 mana regen, +18% damage reduction, +60% movement speed")
            {
                OverrideColor = ultimateGold
            });
            
            tooltips.Add(new TooltipLine(Mod, "ClassBonuses", "Melee: +25% damage, +22% attack speed, +15 crit | Magic: +30% damage, +15 crit, -25% mana cost | Ranged: +30% damage, +18 crit | Summon: +30% damage, +6 minions")
            {
                OverrideColor = cosmicPurple
            });
            
            tooltips.Add(new TooltipLine(Mod, "NightPower", "At night: Additional +25% damage, +25 crit, +20 defense from Moonlight Sonata")
            {
                OverrideColor = new Color(138, 43, 226)
            });
            
            tooltips.Add(new TooltipLine(Mod, "Mobility", "+120 wing time, no fall damage, +50% run acceleration, 30% faster max run speed")
            {
                OverrideColor = cosmicPurple
            });
            
            tooltips.Add(new TooltipLine(Mod, "Effects", "All theme effects: Temporal echoes, bell ring AOE, paradox stacking, feather dodge (18-22%), heroic surge on kill, cosmic bursts")
            {
                OverrideColor = cosmicPurple
            });
            
            tooltips.Add(new TooltipLine(Mod, "Lifesteal", "12% chance to lifesteal 10% of damage (max 30 HP), mana burst when low")
            {
                OverrideColor = ultimateGold
            });
            
            tooltips.Add(new TooltipLine(Mod, "Immunities", "Immunity to all elemental debuffs, status effects, and grants magma stone, frost burn, and 200% thorns")
            {
                OverrideColor = ultimateGold
            });
            
            tooltips.Add(new TooltipLine(Mod, "Lore", "'When all movements converge into one, the symphony transcends mortal limits - this is the Coda of Absolute Harmony'")
            {
                OverrideColor = softGold
            });
        }
    }

    public class CodaOfAbsoluteHarmonyPlayer : ModPlayer
    {
        public bool codaEquipped;
        private int heroicSurgeTimer;
        private int invulnFramesOnKill = 120;
        private int dodgeCooldown;
        private int bellRingCooldown;
        private int temporalEchoCooldown;
        private int cosmicBurstCooldown;
        private int meleeStrikeCount;
        private Dictionary<int, int> paradoxStacks = new Dictionary<int, int>();
        private Dictionary<int, int> paradoxTimers = new Dictionary<int, int>();
        
        // All debuffs from all sources
        private static readonly int[] AllDebuffs = new int[]
        {
            BuffID.Confused, BuffID.Slow, BuffID.CursedInferno,
            BuffID.Ichor, BuffID.ShadowFlame, BuffID.Frostburn,
            BuffID.OnFire, BuffID.Poisoned, BuffID.Venom
        };

        public override void ResetEffects()
        {
            codaEquipped = false;
        }

        public override void PostUpdate()
        {
            if (heroicSurgeTimer > 0)
            {
                heroicSurgeTimer--;
                Player.GetDamage(DamageClass.Generic) += 0.40f;
            }
            
            if (dodgeCooldown > 0) dodgeCooldown--;
            if (bellRingCooldown > 0) bellRingCooldown--;
            if (temporalEchoCooldown > 0) temporalEchoCooldown--;
            if (cosmicBurstCooldown > 0) cosmicBurstCooldown--;
            
            List<int> toRemove = new List<int>();
            foreach (var kvp in paradoxTimers)
            {
                paradoxTimers[kvp.Key]--;
                if (paradoxTimers[kvp.Key] <= 0)
                    toRemove.Add(kvp.Key);
            }
            foreach (int key in toRemove)
            {
                paradoxTimers.Remove(key);
                paradoxStacks.Remove(key);
            }
        }

        public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone)
        {
            HandleCodaHit(target, damageDone, true, DamageClass.Magic.CountsAsClass(item.DamageType));
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (proj.owner == Player.whoAmI)
            {
                bool isMelee = DamageClass.Melee.CountsAsClass(proj.DamageType);
                bool isMagic = DamageClass.Magic.CountsAsClass(proj.DamageType);
                HandleCodaHit(target, damageDone, isMelee, isMagic);
            }
        }

        private void HandleCodaHit(NPC target, int damageDone, bool isMelee, bool isMagic)
        {
            if (!codaEquipped) return;
            
            bool isNight = !Main.dayTime;
            
            // === MOONLIGHT: Blue fire at night ===
            if (isNight && isMagic)
            {
                int bonusDamage = (int)(damageDone * 0.25f);
                target.SimpleStrikeNPC(bonusDamage, 0, false, 0, null, false, 0, true);
            }
            
            // === EROICA: Temporal Echo every 5th melee hit ===
            if (isMelee)
            {
                meleeStrikeCount++;
                if (meleeStrikeCount >= 5)
                {
                    meleeStrikeCount = 0;
                    int echoDamage = (int)(damageDone * 1.0f);
                    target.SimpleStrikeNPC(echoDamage, 0, false, 0, null, false, 0, true);
                    
                }
            }
            
            // === LA CAMPANELLA: Bell ring AOE (20%) ===
            if (bellRingCooldown <= 0 && Main.rand.NextFloat() < 0.20f)
            {
                bellRingCooldown = 15;
                target.AddBuff(BuffID.Confused, 240);
                
                float aoeRadius = 220f;
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (npc.active && !npc.friendly && npc.whoAmI != target.whoAmI && !npc.immortal)
                    {
                        if (Vector2.Distance(npc.Center, target.Center) <= aoeRadius)
                        {
                            int aoeDamage = (int)(damageDone * 0.75f);
                            npc.SimpleStrikeNPC(aoeDamage, 0, false, 0, null, false, 0, true);
                            npc.AddBuff(BuffID.OnFire, 360);
                            npc.AddBuff(BuffID.Frostburn, 300);
                        }
                    }
                }
                
            }
            
            // === ENIGMA: Paradox stacking (25%) ===
            if (Main.rand.NextFloat() < 0.25f)
            {
                int debuffId = AllDebuffs[Main.rand.Next(AllDebuffs.Length)];
                target.AddBuff(debuffId, 480);
                target.AddBuff(BuffID.OnFire, 420);
                target.AddBuff(BuffID.Frostburn, 360);
                
                if (!paradoxStacks.ContainsKey(target.whoAmI))
                    paradoxStacks[target.whoAmI] = 0;
                
                paradoxStacks[target.whoAmI]++;
                paradoxTimers[target.whoAmI] = 540;
                
                // ABSOLUTE HARMONY COLLAPSE at 5 stacks
                if (paradoxStacks[target.whoAmI] >= 5)
                {
                    TriggerAbsoluteHarmonyCollapse(target, damageDone, isNight);
                    paradoxStacks[target.whoAmI] = 0;
                }
            }
            
            // === SWAN LAKE: Rainbow sparkle ===
            if (Main.rand.NextBool(4))
            {
                float hue = Main.rand.NextFloat();
            }
            
            // === SEASONS: All elemental effects ===
            target.AddBuff(BuffID.OnFire, 300);
            target.AddBuff(BuffID.Frostburn, 240);
            target.AddBuff(BuffID.Poisoned, 300);
            
            // === LIFESTEAL (12%) ===
            if (Main.rand.NextFloat() < 0.12f)
            {
                int healAmount = Math.Max(1, Math.Min((int)(damageDone * 0.10f), 30));
                Player.Heal(healAmount);
            }
            
            // === COSMIC MANA BURST ===
            if (Player.statMana < Player.statManaMax2 * 0.3f && cosmicBurstCooldown <= 0)
            {
                cosmicBurstCooldown = 240;
                Player.statMana = Math.Min(Player.statMana + 150, Player.statManaMax2);
                
            }
            
            // Check kill for heroic surge
            if (target.life <= 0 && !target.immortal)
            {
                Player.immune = true;
                Player.immuneTime = Math.Max(Player.immuneTime, invulnFramesOnKill);
                heroicSurgeTimer = 480;
                
                // Kill explosion
            }
        }

        private void TriggerAbsoluteHarmonyCollapse(NPC target, int baseDamage, bool isNight)
        {
            // ================================================
            // === ABSOLUTE HARMONY COLLAPSE - ULTIMATE VFX ===
            // ================================================
            
            // Central flash cascade - ALL colors
            
            Color[] allColors = {
                new Color(255, 183, 197), // Spring
                new Color(255, 180, 50),  // Summer
                new Color(200, 100, 30),  // Autumn
                new Color(150, 220, 255), // Winter
                new Color(138, 43, 226),  // Moonlight
                new Color(255, 200, 80),  // Eroica Gold
                new Color(200, 50, 50),   // Eroica Scarlet
                new Color(255, 140, 40),  // La Campanella
                new Color(140, 60, 200),  // Enigma Purple
                new Color(50, 220, 100),  // Enigma Green
                Color.White,              // Swan Lake White
                new Color(200, 80, 120),  // Fate Pink
                new Color(255, 60, 80),   // Fate Red
            };
            
            // Cascading flares
            
            // Mega halo cascade - 24 rings
            
            // Themed particle bursts
            
            
            
            // Rainbow sparkle explosion
            
            // Multi-color explosion bursts
            
            // Music notes spiraling outward
            
            // ULTIMATE DAMAGE
            if (Main.myPlayer == Player.whoAmI)
            {
                int harmonyDamage = (int)(baseDamage * 7.0f); // 700% damage!
                target.SimpleStrikeNPC(harmonyDamage, 0, false, 0, null, false, 0, true);
                
                float aoeRadius = 450f;
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (npc.active && !npc.friendly && npc.whoAmI != target.whoAmI && !npc.immortal)
                    {
                        if (Vector2.Distance(npc.Center, target.Center) <= aoeRadius)
                        {
                            npc.SimpleStrikeNPC(harmonyDamage / 2, 0, false, 0, null, false, 0, true);
                            
                            // Apply ALL debuffs
                            foreach (int debuff in AllDebuffs)
                                npc.AddBuff(debuff, 600);
                        }
                    }
                }
            }
            
        }

        public override bool FreeDodge(Player.HurtInfo info)
        {
            if (!codaEquipped) return false;
            if (dodgeCooldown > 0) return false;
            
            bool isNight = !Main.dayTime;
            float dodgeChance = isNight ? 0.22f : 0.18f;
            
            if (Main.rand.NextFloat() < dodgeChance)
            {
                dodgeCooldown = 45;
                
                // ULTIMATE DODGE VFX
                
                // Dodge damage
                if (Main.myPlayer == Player.whoAmI)
                {
                    int dodgeDamage = 400 + (int)(Player.GetTotalDamage(DamageClass.Generic).ApplyTo(100) * 0.6f);
                    float damageRadius = 350f;
                    
                    for (int i = 0; i < Main.maxNPCs; i++)
                    {
                        NPC npc = Main.npc[i];
                        if (npc.active && !npc.friendly && !npc.immortal && !npc.dontTakeDamage)
                        {
                            if (Vector2.Distance(npc.Center, Player.Center) <= damageRadius)
                            {
                                npc.SimpleStrikeNPC(dodgeDamage, 0, false, 0, null, false, 0, true);
                                npc.AddBuff(BuffID.OnFire, 360);
                                npc.AddBuff(BuffID.Frostburn, 300);
                            }
                        }
                    }
                }
                
                Player.immune = true;
                Player.immuneTime = 50;
                
                return true;
            }
            
            return false;
        }
    }
}
