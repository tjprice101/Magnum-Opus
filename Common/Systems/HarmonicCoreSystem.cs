using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.UI;

namespace MagnumOpus.Common.Systems
{
    /// <summary>
    /// NEW Harmonic Core system - unlocked by consuming Heart of Music after Moon Lord kill.
    /// Features 3 slots with individual Chromatic (Offensive) / Diatonic (Defensive) toggles.
    /// </summary>
    public class HarmonicCoreModPlayer : ModPlayer
    {
        // 3 Core slots
        public Item[] EquippedCores = new Item[3];
        
        // Per-slot mode: true = Chromatic (Offensive), false = Diatonic (Defensive)
        public bool[] CoreModes = new bool[3] { true, true, true };
        
        // Unlock states
        public bool HasKilledMoonLord = false;
        public bool HasUnlockedHarmonicSlots = false;
        
        // ========== CORE DATA ==========
        public static readonly Dictionary<string, int> CoreTiers = new Dictionary<string, int>
        {
            { "MoonlightSonata", 1 },
            { "Eroica", 2 },
            { "SwanLake", 3 },
            { "LaCampanella", 4 },
            { "Enigma", 5 },
            { "Fate", 6 }
        };
        
        // Core colors for cosmetic effects
        public static readonly Dictionary<string, Color> CoreColors = new Dictionary<string, Color>
        {
            { "MoonlightSonata", new Color(150, 120, 255) },
            { "Eroica", new Color(255, 150, 200) },
            { "SwanLake", new Color(220, 240, 255) },
            { "LaCampanella", new Color(255, 220, 100) },
            { "Enigma", new Color(150, 255, 200) },
            { "Fate", new Color(255, 100, 120) }
        };
        
        // Class buff percentages per tier
        public static readonly float[] TierDamageBonus = { 0f, 0.04f, 0.06f, 0.08f, 0.10f, 0.12f, 0.15f };
        
        // Cooldown timers
        private int bellTollCooldown = 0;
        private int fateShieldCooldown = 0;
        private const int FateShieldCooldownMax = 3600;
        
        // Set bonus trackers (per slot)
        private int[] moonlightPhase = new int[3];
        private int[] moonlightPhaseTimer = new int[3];
        private float[] moonlightStoredDamage = new float[3];
        private int[] eroicaComboCount = new int[3];
        private int[] eroicaComboTimer = new int[3];
        private int[] eroicaRallyTimer = new int[3];
        private float[] swanDodgeChance = new float[3];
        private int[] swanGracePeriod = new int[3];
        private int[] campanellaResonance = new int[3];
        private int[] campanellaEchoTimer = new int[3];
        private int[] enigmaCurrentBonus = new int[3];
        private int[] enigmaBonusTimer = new int[3];
        private int[] fateMarkedNPC = new int[3] { -1, -1, -1 };
        private int fateDeathsAvoided = 0;
        
        // Track last hit target for Lunar Crescendo beam direction
        private int lastHitNPC = -1;
        
        public override void Initialize()
        {
            EquippedCores = new Item[3];
            CoreModes = new bool[3] { true, true, true };
            for (int i = 0; i < 3; i++)
                EquippedCores[i] = new Item();
            HasKilledMoonLord = false;
            HasUnlockedHarmonicSlots = false;
            ResetTrackers();
        }
        
        private void ResetTrackers()
        {
            for (int i = 0; i < 3; i++)
            {
                moonlightPhase[i] = 0;
                moonlightPhaseTimer[i] = 0;
                moonlightStoredDamage[i] = 0f;
                eroicaComboCount[i] = 0;
                eroicaComboTimer[i] = 0;
                eroicaRallyTimer[i] = 0;
                swanDodgeChance[i] = 0f;
                swanGracePeriod[i] = 0;
                campanellaResonance[i] = 0;
                campanellaEchoTimer[i] = 0;
                enigmaCurrentBonus[i] = 0;
                enigmaBonusTimer[i] = 0;
                fateMarkedNPC[i] = -1;
            }
            fateDeathsAvoided = 0;
        }
        
        public override void SaveData(TagCompound tag)
        {
            tag["HasKilledMoonLord"] = HasKilledMoonLord;
            tag["HasUnlockedHarmonicSlots"] = HasUnlockedHarmonicSlots;
            tag["FateDeathsAvoided"] = fateDeathsAvoided;
            
            for (int i = 0; i < 3; i++)
            {
                tag[$"CoreMode_{i}"] = CoreModes[i];
                if (EquippedCores[i] != null && !EquippedCores[i].IsAir)
                    tag[$"EquippedCore_{i}_Type"] = EquippedCores[i].type;
            }
        }
        
        public override void LoadData(TagCompound tag)
        {
            HasKilledMoonLord = tag.GetBool("HasKilledMoonLord");
            HasUnlockedHarmonicSlots = tag.GetBool("HasUnlockedHarmonicSlots");
            fateDeathsAvoided = tag.GetInt("FateDeathsAvoided");
            
            for (int i = 0; i < 3; i++)
            {
                CoreModes[i] = tag.ContainsKey($"CoreMode_{i}") ? tag.GetBool($"CoreMode_{i}") : true;
                if (tag.ContainsKey($"EquippedCore_{i}_Type"))
                {
                    int coreType = tag.GetInt($"EquippedCore_{i}_Type");
                    EquippedCores[i] = new Item();
                    EquippedCores[i].SetDefaults(coreType);
                }
                else
                {
                    EquippedCores[i] = new Item();
                }
            }
            
            // Migration from old single-slot system
            if (tag.ContainsKey("EquippedCoreType") && !tag.ContainsKey("EquippedCore_0_Type"))
            {
                int oldType = tag.GetInt("EquippedCoreType");
                if (oldType > 0)
                {
                    EquippedCores[0] = new Item();
                    EquippedCores[0].SetDefaults(oldType);
                    CoreModes[0] = tag.GetBool("UsingOffensiveBuff");
                    HasUnlockedHarmonicSlots = HasKilledMoonLord;
                }
            }
        }
        
        // ========== HELPER METHODS ==========
        public string GetCoreName(int slot)
        {
            if (slot < 0 || slot >= 3) return "";
            if (EquippedCores[slot] == null || EquippedCores[slot].IsAir) return "";
            
            string typeName = EquippedCores[slot].ModItem?.GetType().Name ?? "";
            
            if (typeName.Contains("MoonlightSonata")) return "MoonlightSonata";
            if (typeName.Contains("Eroica")) return "Eroica";
            if (typeName.Contains("SwanLake")) return "SwanLake";
            if (typeName.Contains("LaCampanella")) return "LaCampanella";
            if (typeName.Contains("Enigma")) return "Enigma";
            if (typeName.Contains("Fate")) return "Fate";
            
            return "";
        }
        
        public int GetCoreTier(int slot)
        {
            string name = GetCoreName(slot);
            return CoreTiers.ContainsKey(name) ? CoreTiers[name] : 0;
        }
        
        public int GetHighestTier()
        {
            int highest = 0;
            for (int i = 0; i < 3; i++)
            {
                int tier = GetCoreTier(i);
                if (tier > highest) highest = tier;
            }
            return highest;
        }
        
        public string GetRightmostCoreName()
        {
            for (int i = 2; i >= 0; i--)
            {
                string name = GetCoreName(i);
                if (!string.IsNullOrEmpty(name)) return name;
            }
            return "";
        }
        
        public Color GetRightmostCoreColor()
        {
            string name = GetRightmostCoreName();
            return CoreColors.ContainsKey(name) ? CoreColors[name] : Color.White;
        }
        
        public int GetEquippedCoreCount()
        {
            int count = 0;
            for (int i = 0; i < 3; i++)
                if (EquippedCores[i] != null && !EquippedCores[i].IsAir)
                    count++;
            return count;
        }
        
        public void EquipCore(int slot, Item item)
        {
            if (slot < 0 || slot >= 3) return;
            EquippedCores[slot] = item.Clone();
            SoundEngine.PlaySound(SoundID.Coins with { Volume = 0.8f, Pitch = 0.2f });
            SoundEngine.PlaySound(SoundID.Item4 with { Volume = 0.6f });
        }
        
        public void UnequipCore(int slot)
        {
            if (slot < 0 || slot >= 3) return;
            EquippedCores[slot] = new Item();
            SoundEngine.PlaySound(SoundID.Grab);
        }
        
        public void ToggleCoreMode(int slot)
        {
            if (slot < 0 || slot >= 3) return;
            CoreModes[slot] = !CoreModes[slot];
            SoundEngine.PlaySound(SoundID.MenuTick);
        }
        
        // ========== BUFF DESCRIPTION HELPERS ==========
        public static string GetChromaticBuffName(string coreName)
        {
            return coreName switch
            {
                "MoonlightSonata" => "Nocturne's Edge",
                "Eroica" => "Heroic Fury",
                "SwanLake" => "Dying Swan",
                "LaCampanella" => "Bell's Toll",
                "Enigma" => "Enigma's Chaos",
                "Fate" => "Fate's Wrath",
                _ => "Unknown"
            };
        }
        
        public static string GetDiatonicBuffName(string coreName)
        {
            return coreName switch
            {
                "MoonlightSonata" => "Lunar Veil",
                "Eroica" => "Heroic Resolve",
                "SwanLake" => "Swan's Grace",
                "LaCampanella" => "Bell's Ward",
                "Enigma" => "Enigma's Mystery",
                "Fate" => "Fate's Shield",
                _ => "Unknown"
            };
        }
        
        public static string GetChromaticBuffDesc(string coreName)
        {
            return coreName switch
            {
                "MoonlightSonata" => "+8% dmg at night\nLunar Crescendo beam",
                "Eroica" => "+12% dmg low HP\nHeroic Shockwave",
                "SwanLake" => "Lower HP = more dmg\nUp to +35% damage",
                "LaCampanella" => "+8% dmg, crit echo\n12 stacks = triple",
                "Enigma" => "Random buffs\nCycle every 10s",
                "Fate" => "+14% dmg, execute\nMark bosses +35%",
                _ => "No effect"
            };
        }
        
        public static string GetDiatonicBuffDesc(string coreName)
        {
            return coreName switch
            {
                "MoonlightSonata" => "+10 DEF night, +6% DR\nInvincibility/12s",
                "Eroica" => "+12 DEF low HP, +8% DR\nRally Cry <30% HP",
                "SwanLake" => "+10 DEF, +10% DR\n25% dodge moving",
                "LaCampanella" => "+12 DEF, +12% DR\nBells + heal 6HP",
                "Enigma" => "+15 DEF, +15% DR\nReflect projectiles",
                "Fate" => "+18 DEF, +16% DR\nCheat death (1m)",
                _ => "No effect"
            };
        }
        
        // Active set bonus effect names
        public static string GetActiveSetBonusName(string coreName, bool isChromatic)
        {
            if (isChromatic)
            {
                return coreName switch
                {
                    "MoonlightSonata" => "► Lunar Crescendo Beam",
                    "Eroica" => "► Heroic Shockwave",
                    "SwanLake" => "► Dying Swan Fury",
                    "LaCampanella" => "► Resonant Echo",
                    "Enigma" => "► Chaotic Variations",
                    "Fate" => "► Mark of Fate",
                    _ => ""
                };
            }
            else
            {
                return coreName switch
                {
                    "MoonlightSonata" => "► Eclipse Shroud",
                    "Eroica" => "► Rally Cry",
                    "SwanLake" => "► Swan's Grace",
                    "LaCampanella" => "► Sanctuary Bells",
                    "Enigma" => "► Mystery Shield",
                    "Fate" => "► Destiny's Weave",
                    _ => ""
                };
            }
        }
        
        // ========== STAT APPLICATION ==========
        public override void ModifyWeaponDamage(Item item, ref StatModifier damage)
        {
            if (!HasUnlockedHarmonicSlots || GetEquippedCoreCount() == 0) return;
            
            // Class bonus from highest tier
            int tier = GetHighestTier();
            if (tier > 0 && tier <= 6)
                damage += TierDamageBonus[tier];
            
            // Offensive bonuses from Chromatic cores
            for (int slot = 0; slot < 3; slot++)
            {
                string coreName = GetCoreName(slot);
                if (string.IsNullOrEmpty(coreName) || !CoreModes[slot]) continue;
                
                bool isNight = !Main.dayTime;
                bool isLowHP = Player.statLife < Player.statLifeMax2 * 0.5f;
                float mult = 0.7f; // Reduced for stacking
                
                float bonus = coreName switch
                {
                    "MoonlightSonata" => (isNight ? 0.08f : 0.02f) * mult,
                    "Eroica" => (isLowHP ? 0.12f : 0.04f) * mult,
                    "SwanLake" => 0.09f * mult,
                    "LaCampanella" => 0.08f * mult,
                    "Enigma" => (0.06f + Main.rand.NextFloat(0.06f)) * mult,
                    "Fate" => 0.14f * mult,
                    _ => 0f
                };
                damage += bonus;
            }
        }
        
        public override void UpdateEquips()
        {
            if (!HasUnlockedHarmonicSlots) return;
            
            if (bellTollCooldown > 0) bellTollCooldown--;
            if (fateShieldCooldown > 0) fateShieldCooldown--;
            
            for (int slot = 0; slot < 3; slot++)
            {
                string coreName = GetCoreName(slot);
                if (string.IsNullOrEmpty(coreName)) continue;
                
                if (!CoreModes[slot]) // Diatonic (Defensive)
                    ApplyDiatonicBuffs(slot, coreName);
                else
                    ApplyChromaticPassives(slot, coreName);
                
                UpdateSetBonus(slot, coreName);
            }
        }
        
        private void ApplyDiatonicBuffs(int slot, string coreName)
        {
            bool isNight = !Main.dayTime;
            bool isLowHP = Player.statLife < Player.statLifeMax2 * 0.5f;
            float mult = 0.65f;
            
            switch (coreName)
            {
                case "MoonlightSonata":
                    Player.statDefense += (int)((isNight ? 10 : 5) * mult);
                    Player.endurance += 0.06f * mult;
                    break;
                case "Eroica":
                    Player.statDefense += (int)((isLowHP ? 12 : 6) * mult);
                    Player.endurance += 0.08f * mult;
                    break;
                case "SwanLake":
                    Player.statDefense += (int)(10 * mult);
                    Player.endurance += 0.10f * mult;
                    Player.moveSpeed += 0.12f * mult;
                    break;
                case "LaCampanella":
                    Player.statDefense += (int)(12 * mult);
                    Player.endurance += 0.12f * mult;
                    break;
                case "Enigma":
                    Player.statDefense += (int)(15 * mult);
                    Player.endurance += 0.15f * mult;
                    break;
                case "Fate":
                    Player.statDefense += (int)(18 * mult);
                    Player.endurance += 0.16f * mult;
                    break;
            }
        }
        
        private void ApplyChromaticPassives(int slot, string coreName)
        {
            if (coreName == "SwanLake")
            {
                float hpPercent = (float)Player.statLife / Player.statLifeMax2;
                Player.GetDamage(DamageClass.Generic) += (1f - hpPercent) * 0.21f;
            }
        }
        
        private void UpdateSetBonus(int slot, string coreName)
        {
            switch (coreName)
            {
                case "MoonlightSonata": UpdateMoonlightBonus(slot); break;
                case "Eroica": UpdateEroicaBonus(slot); break;
                case "SwanLake": UpdateSwanLakeBonus(slot); break;
                case "LaCampanella": UpdateCampanellaBonus(slot); break;
                case "Enigma": UpdateEnigmaBonus(slot); break;
                case "Fate": UpdateFateBonus(slot); break;
            }
        }
        
        private void UpdateMoonlightBonus(int slot)
        {
            moonlightPhaseTimer[slot]++;
            if (CoreModes[slot])
            {
                if (moonlightPhaseTimer[slot] > 150 && moonlightPhase[slot] > 0)
                {
                    moonlightPhase[slot]--;
                    moonlightPhaseTimer[slot] = 0;
                }
            }
            else
            {
                // Diatonic: Lunar Veil - brief invincibility every 12 seconds (Paladin's Shield effect instead of Shadow Dodge)
                if (moonlightPhaseTimer[slot] >= 720)
                {
                    moonlightPhaseTimer[slot] = 0;
                    // Grant brief invincibility frames instead of ShadowDodge (which causes invisibility)
                    Player.immune = true;
                    Player.immuneTime = 45;
                    Player.immuneNoBlink = true; // Don't blink during immunity
                    SoundEngine.PlaySound(SoundID.Item72 with { Volume = 0.4f }, Player.Center);
                    
                    // Visual effect
                    for (int i = 0; i < 15; i++)
                    {
                        Dust dust = Dust.NewDustDirect(Player.position, Player.width, Player.height, 
                            DustID.PurpleTorch, Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-2f, 2f), 100, default, 1.2f);
                        dust.noGravity = true;
                    }
                }
            }
        }
        
        private void UpdateEroicaBonus(int slot)
        {
            if (CoreModes[slot])
            {
                if (eroicaComboTimer[slot] > 0) eroicaComboTimer[slot]--;
                if (eroicaComboTimer[slot] <= 0 && eroicaComboCount[slot] > 0)
                {
                    eroicaComboCount[slot]--;
                    eroicaComboTimer[slot] = 25;
                }
            }
            else if (eroicaRallyTimer[slot] > 0)
            {
                eroicaRallyTimer[slot]--;
                Player.GetDamage(DamageClass.Generic) += 0.18f;
                Player.GetAttackSpeed(DamageClass.Generic) += 0.22f;
                Player.moveSpeed += 0.22f;
                Player.statDefense += 15;
            }
        }
        
        private void UpdateSwanLakeBonus(int slot)
        {
            if (CoreModes[slot])
            {
                float hpPercent = (float)Player.statLife / Player.statLifeMax2;
                Player.GetCritChance(DamageClass.Generic) += (int)((1f - hpPercent) * 22f);
            }
            else
            {
                bool isMoving = Math.Abs(Player.velocity.X) > 1f || Math.Abs(Player.velocity.Y) > 1f;
                if (isMoving)
                {
                    swanDodgeChance[slot] = Math.Min(0.25f, swanDodgeChance[slot] + 0.0015f);
                    swanGracePeriod[slot] = 0;
                }
                else
                {
                    swanDodgeChance[slot] = Math.Max(0f, swanDodgeChance[slot] - 0.004f);
                    swanGracePeriod[slot]++;
                    if (swanGracePeriod[slot] >= 45 && swanGracePeriod[slot] % 25 == 0)
                    {
                        Player.statLife = Math.Min(Player.statLife + 2, Player.statLifeMax2);
                        Player.HealEffect(2, true);
                    }
                }
            }
        }
        
        private void UpdateCampanellaBonus(int slot)
        {
            if (CoreModes[slot])
            {
                if (campanellaEchoTimer[slot] > 0) campanellaEchoTimer[slot]--;
                else if (campanellaResonance[slot] > 0 && Main.GameUpdateCount % 80 == 0)
                    campanellaResonance[slot]--;
            }
        }
        
        private void UpdateEnigmaBonus(int slot)
        {
            enigmaBonusTimer[slot]++;
            if (CoreModes[slot] && enigmaBonusTimer[slot] >= 600)
            {
                enigmaBonusTimer[slot] = 0;
                enigmaCurrentBonus[slot] = Main.rand.Next(6);
                SoundEngine.PlaySound(SoundID.Item4 with { Volume = 0.5f }, Player.Center);
            }
            
            if (CoreModes[slot])
            {
                switch (enigmaCurrentBonus[slot])
                {
                    case 0: Player.GetDamage(DamageClass.Generic) += 0.14f; break;
                    case 1: Player.GetAttackSpeed(DamageClass.Generic) += 0.18f; break;
                    case 2: Player.GetArmorPenetration(DamageClass.Generic) += 14; break;
                    case 3: Player.GetCritChance(DamageClass.Generic) += 10; break;
                    case 5: Player.manaRegen += 35; Player.statManaMax2 += 35; break;
                }
            }
        }
        
        private void UpdateFateBonus(int slot)
        {
            if (CoreModes[slot] && fateMarkedNPC[slot] >= 0)
            {
                NPC marked = Main.npc[fateMarkedNPC[slot]];
                if (!marked.active || marked.life <= 0)
                    fateMarkedNPC[slot] = -1;
            }
            else if (!CoreModes[slot] && fateDeathsAvoided > 0)
            {
                float bonus = Math.Min(fateDeathsAvoided * 0.04f, 0.40f);
                Player.GetDamage(DamageClass.Generic) += bonus;
                Player.statDefense += fateDeathsAvoided * 2;
            }
        }
        
        public override void PostUpdate()
        {
            if (!HasUnlockedHarmonicSlots) return;
            
            // La Campanella healing
            for (int slot = 0; slot < 3; slot++)
            {
                if (GetCoreName(slot) == "LaCampanella" && !CoreModes[slot] && Main.GameUpdateCount % 150 == 0)
                {
                    Player.statLife = Math.Min(Player.statLife + 6, Player.statLifeMax2);
                    Player.HealEffect(6, true);
                }
            }
            
            // Cosmetic effects removed per user request
        }
        
        // ========== ON-HIT EFFECTS ==========
        public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone)
        {
            HandleOnHit(target, hit, damageDone);
        }
        
        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            HandleOnHit(target, hit, damageDone);
        }
        
        private void HandleOnHit(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!HasUnlockedHarmonicSlots) return;
            
            // Track last hit target for beam direction
            lastHitNPC = target.whoAmI;
            
            for (int slot = 0; slot < 3; slot++)
            {
                string coreName = GetCoreName(slot);
                if (string.IsNullOrEmpty(coreName) || !CoreModes[slot]) continue;
                
                switch (coreName)
                {
                    case "MoonlightSonata":
                        moonlightStoredDamage[slot] += damageDone;
                        moonlightPhaseTimer[slot] = 0;
                        if (moonlightStoredDamage[slot] >= 600 && moonlightPhase[slot] < 4)
                        {
                            moonlightPhase[slot]++;
                            moonlightStoredDamage[slot] = 0;
                            SoundEngine.PlaySound(SoundID.Item29 with { Pitch = 0.1f * moonlightPhase[slot], Volume = 0.5f }, Player.Center);
                        }
                        if (moonlightPhase[slot] >= 4)
                        {
                            ReleaseLunarCrescendo();
                            moonlightPhase[slot] = 0;
                            moonlightStoredDamage[slot] = 0;
                        }
                        if (Main.rand.NextBool(5)) target.AddBuff(BuffID.Slow, 90);
                        break;
                        
                    case "Eroica":
                        eroicaComboCount[slot]++;
                        eroicaComboTimer[slot] = 90;
                        if (eroicaComboCount[slot] >= 20)
                        {
                            ReleaseHeroicShockwave();
                            eroicaComboCount[slot] = 0;
                        }
                        break;
                        
                    case "LaCampanella":
                        campanellaResonance[slot] = Math.Min(12, campanellaResonance[slot] + 1);
                        campanellaEchoTimer[slot] = 180;
                        if (campanellaResonance[slot] >= 12 && hit.Crit)
                        {
                            ReleaseResonantEcho(target, damageDone);
                            campanellaResonance[slot] = 0;
                        }
                        else if (hit.Crit && bellTollCooldown <= 0)
                        {
                            bellTollCooldown = 30;
                            SoundEngine.PlaySound(SoundID.Item35 with { Pitch = 0.5f }, target.Center);
                            target.SimpleStrikeNPC(damageDone / 4, 0, false, 0f, null, false, 0f, true);
                        }
                        break;
                        
                    case "Enigma":
                        if (Main.rand.NextBool(6))
                        {
                            int[] debuffs = { BuffID.OnFire, BuffID.Frostburn, BuffID.Venom, BuffID.Confused };
                            target.AddBuff(debuffs[Main.rand.Next(4)], 120);
                        }
                        break;
                        
                    case "Fate":
                        if (fateMarkedNPC[slot] < 0 && target.boss)
                        {
                            fateMarkedNPC[slot] = target.whoAmI;
                            SoundEngine.PlaySound(SoundID.Item119, target.Center);
                        }
                        if (target.life < target.lifeMax * 0.1f && Main.rand.NextBool(4))
                        {
                            target.SimpleStrikeNPC(target.life + 100, 0, true, 0f, null, false, 0f, true);
                            SoundEngine.PlaySound(SoundID.NPCDeath6 with { Pitch = -0.3f }, target.Center);
                        }
                        break;
                }
            }
        }
        
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            if (!HasUnlockedHarmonicSlots) return;
            for (int slot = 0; slot < 3; slot++)
            {
                if (GetCoreName(slot) == "Fate" && CoreModes[slot] && target.whoAmI == fateMarkedNPC[slot])
                    modifiers.SourceDamage += 0.35f;
            }
        }
        
        private void ReleaseLunarCrescendo()
        {
            // Dramatic sound
            SoundEngine.PlaySound(SoundID.Item122 with { Pitch = -0.2f, Volume = 1.2f }, Player.Center);
            SoundEngine.PlaySound(SoundID.Item68 with { Pitch = 0.3f, Volume = 0.8f }, Player.Center);
            
            // Damage all nearby enemies
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && !npc.friendly && npc.Distance(Player.Center) < 700f)
                {
                    int damage = (int)(Player.GetTotalDamage(DamageClass.Generic).ApplyTo(150));
                    npc.SimpleStrikeNPC(damage, 0, true, 0f, null, false, 0f, true);
                }
            }
            
            // DRAMATIC VISUAL: Expanding moon ring
            for (int ring = 0; ring < 3; ring++)
            {
                float ringOffset = ring * 8f;
                for (int i = 0; i < 60; i++)
                {
                    float angle = MathHelper.TwoPi * i / 60f;
                    Vector2 vel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * (8f + ringOffset);
                    Color dustColor = ring == 1 ? new Color(200, 150, 255) : default;
                    Dust dust = Dust.NewDustPerfect(Player.Center, DustID.PurpleTorch, vel, 0, dustColor, 2.5f - ring * 0.4f);
                    dust.noGravity = true;
                    dust.fadeIn = 1.5f;
                }
            }
            
            // Beam of moonlight toward last hit target
            Vector2 beamTarget = Player.Center + new Vector2(0, -300f); // Default up
            if (lastHitNPC >= 0 && lastHitNPC < Main.maxNPCs && Main.npc[lastHitNPC].active)
            {
                beamTarget = Main.npc[lastHitNPC].Center;
            }
            
            Vector2 beamDir = Vector2.Normalize(beamTarget - Player.Center);
            for (int i = 0; i < 80; i++)
            {
                float dist = Main.rand.NextFloat(50f, 500f);
                Vector2 beamPos = Player.Center + beamDir * dist + Main.rand.NextVector2Circular(30f, 30f);
                Dust beam = Dust.NewDustPerfect(beamPos, DustID.PurpleTorch, beamDir * 3f, 0, new Color(180, 120, 255), 2f);
                beam.noGravity = true;
                beam.fadeIn = 1.8f;
            }
            
            // Crescent moon particles
            for (int i = 0; i < 25; i++)
            {
                Vector2 moonPos = Player.Center + Main.rand.NextVector2Circular(200f, 200f);
                Dust moon = Dust.NewDustPerfect(moonPos, DustID.IceTorch, Main.rand.NextVector2Circular(2f, 2f), 100, new Color(220, 200, 255), 1.8f);
                moon.noGravity = true;
            }
            
            // Screen flash effect via lighting
            for (int x = -15; x <= 15; x++)
            {
                for (int y = -15; y <= 15; y++)
                {
                    Vector2 lightPos = Player.Center + new Vector2(x * 32, y * 32);
                    float dist = Vector2.Distance(lightPos, Player.Center) / 500f;
                    float intensity = Math.Max(0, 1f - dist) * 1.5f;
                    Lighting.AddLight(lightPos, 0.6f * intensity, 0.4f * intensity, 1f * intensity);
                }
            }
        }
        
        private void ReleaseHeroicShockwave()
        {
            SoundEngine.PlaySound(SoundID.Item62 with { Pitch = -0.3f }, Player.Center);
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && !npc.friendly && npc.Distance(Player.Center) < 500f)
                {
                    int damage = (int)(Player.GetTotalDamage(DamageClass.Generic).ApplyTo(250));
                    npc.SimpleStrikeNPC(damage, Player.direction, true, 8f, null, false, 0f, true);
                }
            }
            for (int i = 0; i < 35; i++)
            {
                float angle = MathHelper.TwoPi * i / 35f;
                Vector2 vel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 5f;
                Dust dust = Dust.NewDustPerfect(Player.Center, DustID.PinkTorch, vel, 100, default, 1.8f);
                dust.noGravity = true;
            }
        }
        
        private void ReleaseResonantEcho(NPC target, int baseDamage)
        {
            SoundEngine.PlaySound(SoundID.Item35 with { Pitch = -0.2f, Volume = 1.3f }, target.Center);
            for (int echo = 0; echo < 3; echo++)
                target.SimpleStrikeNPC(baseDamage / (echo + 1), 0, true, 0f, null, false, 0f, true);
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && !npc.friendly && npc != target && npc.Distance(target.Center) < 250f)
                    npc.SimpleStrikeNPC(baseDamage / 2, 0, false, 0f, null, false, 0f, true);
            }
        }
        
        // ========== DEFENSIVE ON-HIT ==========
        public override void OnHitByNPC(NPC npc, Player.HurtInfo hurtInfo) => HandleDefensiveHit(hurtInfo.Damage);
        public override void OnHitByProjectile(Projectile proj, Player.HurtInfo hurtInfo) => HandleDefensiveHit(hurtInfo.Damage);
        
        private void HandleDefensiveHit(int damage)
        {
            if (!HasUnlockedHarmonicSlots) return;
            
            for (int slot = 0; slot < 3; slot++)
            {
                string coreName = GetCoreName(slot);
                if (string.IsNullOrEmpty(coreName) || CoreModes[slot]) continue;
                
                if (coreName == "Eroica" && Player.statLife < Player.statLifeMax2 * 0.3f && eroicaRallyTimer[slot] <= 0)
                {
                    eroicaRallyTimer[slot] = 240;
                    SoundEngine.PlaySound(SoundID.Roar with { Pitch = 0.3f, Volume = 0.4f }, Player.Center);
                    Main.NewText("Rally Cry activated!", 255, 150, 200);
                }
                
                if (coreName == "LaCampanella")
                {
                    SoundEngine.PlaySound(SoundID.Item35 with { Pitch = 0.2f, Volume = 0.6f }, Player.Center);
                    for (int i = 0; i < Main.maxNPCs; i++)
                    {
                        NPC npc = Main.npc[i];
                        if (npc.active && !npc.friendly && npc.Distance(Player.Center) < 180f)
                            npc.SimpleStrikeNPC((int)(damage * 1.5f), 0, false, 0f, null, false, 0f, true);
                    }
                }
                
                if (coreName == "Enigma" && Main.rand.NextBool(6))
                {
                    for (int i = 0; i < Main.maxProjectiles; i++)
                    {
                        Projectile proj = Main.projectile[i];
                        if (proj.active && proj.hostile && proj.Distance(Player.Center) < 130f)
                        {
                            proj.velocity *= -1.4f;
                            proj.hostile = false;
                            proj.friendly = true;
                            break;
                        }
                    }
                }
            }
        }
        
        public override bool FreeDodge(Player.HurtInfo info)
        {
            if (!HasUnlockedHarmonicSlots) return false;
            
            for (int slot = 0; slot < 3; slot++)
            {
                string coreName = GetCoreName(slot);
                if (string.IsNullOrEmpty(coreName) || CoreModes[slot]) continue;
                
                if (coreName == "SwanLake" && swanDodgeChance[slot] > 0 && Main.rand.NextFloat() < swanDodgeChance[slot])
                {
                    SoundEngine.PlaySound(SoundID.Item24 with { Pitch = 0.5f, Volume = 0.4f }, Player.Center);
                    return true;
                }
                
                if (coreName == "Fate" && fateShieldCooldown <= 0 && Player.statLife - info.Damage <= 0)
                {
                    fateShieldCooldown = FateShieldCooldownMax;
                    fateDeathsAvoided++;
                    Player.statLife = Player.statLifeMax2 / 4;
                    SoundEngine.PlaySound(SoundID.Item119, Player.Center);
                    Main.NewText($"Fate's Shield activates! (Destiny's Weave: {fateDeathsAvoided} stacks)", 200, 100, 255);
                    return true;
                }
            }
            return false;
        }
        
        public override void Kill(double damage, int hitDirection, bool pvp, PlayerDeathReason damageSource)
        {
            for (int slot = 0; slot < 3; slot++)
            {
                if (GetCoreName(slot) == "Fate" && !CoreModes[slot] && fateDeathsAvoided > 0)
                {
                    Main.NewText($"Destiny's Weave broken... ({fateDeathsAvoided} stacks lost)", 100, 50, 150);
                    fateDeathsAvoided = 0;
                    break;
                }
            }
        }
    }
    
    /// <summary>
    /// Draws a vibrant glowing outline around the player based on their rightmost equipped Harmonic Core.
    /// </summary>
    public class HarmonicCoreOutlineLayer : PlayerDrawLayer
    {
        public override Position GetDefaultPosition() => new BeforeParent(PlayerDrawLayers.Skin);
        
        public override bool GetDefaultVisibility(PlayerDrawSet drawInfo)
        {
            // Cosmetic outline disabled per user request
            return false;
        }
        
        protected override void Draw(ref PlayerDrawSet drawInfo)
        {
            var player = drawInfo.drawPlayer;
            var modPlayer = player.GetModPlayer<HarmonicCoreModPlayer>();
            
            string coreName = modPlayer.GetRightmostCoreName();
            if (string.IsNullOrEmpty(coreName)) return;
            
            Color coreColor = modPlayer.GetRightmostCoreColor();
            
            // Get player texture
            Texture2D playerTexture = TextureAssets.Players[player.skinVariant, 0].Value;
            
            // Vibrant pulsing outline
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.08f) * 0.3f + 0.7f;
            float outerPulse = (float)Math.Sin(Main.GameUpdateCount * 0.06f + 1f) * 0.2f + 0.8f;
            
            // Create vibrant glow color (more saturated and bright)
            Color glowColor = new Color(
                (int)Math.Min(255, coreColor.R * 1.5f),
                (int)Math.Min(255, coreColor.G * 1.5f),
                (int)Math.Min(255, coreColor.B * 1.5f),
                0) * pulse * 0.8f;
            
            Color outerGlow = new Color(
                (int)Math.Min(255, coreColor.R * 1.8f),
                (int)Math.Min(255, coreColor.G * 1.8f),
                (int)Math.Min(255, coreColor.B * 1.8f),
                0) * outerPulse * 0.4f;
            
            Vector2 drawPos = drawInfo.Position - Main.screenPosition + drawInfo.drawPlayer.Size / 2f;
            drawPos = new Vector2((int)drawPos.X, (int)drawPos.Y);
            
            // Draw multiple outline layers for vibrant effect
            float[] offsets = { 4f, 6f, 8f };
            Color[] colors = { glowColor, glowColor * 0.6f, outerGlow };
            
            for (int layer = 0; layer < 3; layer++)
            {
                float offset = offsets[layer];
                Color layerColor = colors[layer];
                
                // 8-directional outline
                Vector2[] directions = {
                    new Vector2(-1, 0), new Vector2(1, 0),
                    new Vector2(0, -1), new Vector2(0, 1),
                    new Vector2(-0.7f, -0.7f), new Vector2(0.7f, -0.7f),
                    new Vector2(-0.7f, 0.7f), new Vector2(0.7f, 0.7f)
                };
                
                foreach (var dir in directions)
                {
                    Vector2 offsetPos = drawPos + dir * offset;
                    
                    DrawData outlineData = new DrawData(
                        playerTexture,
                        offsetPos,
                        drawInfo.drawPlayer.bodyFrame,
                        layerColor,
                        drawInfo.drawPlayer.bodyRotation,
                        drawInfo.bodyVect,
                        1f,
                        drawInfo.playerEffect,
                        0);
                    
                    drawInfo.DrawDataCache.Add(outlineData);
                }
            }
            
            // Add sparkle particles around player occasionally
            if (Main.rand.NextBool(8))
            {
                Vector2 sparklePos = player.Center + Main.rand.NextVector2Circular(24f, 32f);
                int dustType = coreName switch
                {
                    "MoonlightSonata" => DustID.PurpleTorch,
                    "Eroica" => DustID.PinkTorch,
                    "SwanLake" => DustID.IceTorch,
                    "LaCampanella" => DustID.GoldFlame,
                    "Enigma" => DustID.RainbowMk2,
                    "Fate" => DustID.CrimsonTorch,
                    _ => DustID.SparksMech
                };
                
                Dust sparkle = Dust.NewDustPerfect(sparklePos, dustType, 
                    new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), Main.rand.NextFloat(-1.5f, -0.3f)), 
                    100, default, 1.3f);
                sparkle.noGravity = true;
            }
        }
    }
}
