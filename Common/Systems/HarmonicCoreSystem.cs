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
using Terraria.ModLoader.IO;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Common.Systems
{
    /// <summary>
    /// NEW Harmonic Core system - unlocked by consuming Heart of Music after Moon Lord kill.
    /// Features 3 slots with unique passive effects per core type.
    /// No more Chromatic/Diatonic toggle - each core has one unique effect.
    /// Enhancement system: Each core can be enhanced up to 5 times with Seed of Universal Melodies.
    /// Enhancement levels are tracked PER CORE TYPE - removing and re-equipping retains the upgrade!
    /// </summary>
    public class HarmonicCoreModPlayer : ModPlayer
    {
        // 3 Core slots
        public Item[] EquippedCores = new Item[3];
        
        // Legacy enhancement levels per slot (0-5) - kept for compatibility but now derived from CoreEnhancementLevels
        public int[] EnhancementLevels = new int[3];
        public const int MaxEnhancementLevel = 5;
        
        // NEW: Persistent enhancement levels tracked by core item TYPE (item.type -> enhancement level)
        // This allows enhancement to persist when cores are unequipped and re-equipped!
        public Dictionary<int, int> CoreEnhancementLevels = new Dictionary<int, int>();
        
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
        
        // Flat damage bonuses per tier (stacks additively)
        public static readonly float[] TierDamageBonus = { 0f, 0.04f, 0.06f, 0.08f, 0.10f, 0.12f, 0.15f };
        
        // ========== UNIQUE EFFECT TRACKERS ==========
        
        // Moonlight Sonata - Lunar Aura
        private int lunarAuraTimer = 0;
        private const float LunarAuraRadius = 180f;
        private const int LunarAuraDamage = 8;
        
        // Eroica - Heroic Rally
        private int heroicRallyHealRadius = 200;
        private const int HeroicRallyHealAmount = 15;
        
        // Swan Lake - Feathered Grace
        private float featherDodgeChance = 0.12f;
        private int featherAttackTimer = 0;
        private const int FeatherDamage = 45;
        
        // La Campanella - Bell's Resonance
        private int bellEchoTimer = 0;
        private int bellAuraTimer = 0;
        private const float BellEchoDamagePercent = 0.25f;
        private const int BellAuraDamage = 35;
        
        // Enigma - Mystery Shield + Prismatic Flares
        private float reflectChance = 0.15f;
        private int prismaticFlareTimer = 0;
        private const int PrismaticFlareDamage = 65;
        
        // Fate - Cosmic Destiny
        private List<CosmicMark> cosmicMarks = new List<CosmicMark>();
        private const int CosmicMarkDelay = 30;
        private const float CosmicMarkDamagePercent = 0.40f;
        
        private struct CosmicMark
        {
            public int NPCIndex;
            public int Timer;
            public int Damage;
            public Vector2 Position;
        }
        
        public override void Initialize()
        {
            EquippedCores = new Item[3];
            EnhancementLevels = new int[3];
            CoreEnhancementLevels = new Dictionary<int, int>();
            for (int i = 0; i < 3; i++)
            {
                EquippedCores[i] = new Item();
                EnhancementLevels[i] = 0;
            }
            HasKilledMoonLord = false;
            HasUnlockedHarmonicSlots = false;
            cosmicMarks.Clear();
        }
        
        /// <summary>
        /// Resets all Harmonic Core effect timers and state every frame.
        /// This ensures effects stop immediately when cores are unequipped.
        /// </summary>
        public override void ResetEffects()
        {
            // Clear cosmic marks if Fate core is not equipped
            if (!HasCore("Fate"))
            {
                cosmicMarks.Clear();
            }
            
            // Reset all effect timers when no cores are equipped
            // This ensures effects stop immediately on unequip
            if (GetEquippedCoreCount() == 0 || !HasUnlockedHarmonicSlots)
            {
                lunarAuraTimer = 0;
                featherAttackTimer = 0;
                prismaticFlareTimer = 0;
                bellAuraTimer = 0;
                bellEchoTimer = 0;
                cosmicMarks.Clear();
            }
        }
        
        public override void SaveData(TagCompound tag)
        {
            tag["HasKilledMoonLord"] = HasKilledMoonLord;
            tag["HasUnlockedHarmonicSlots"] = HasUnlockedHarmonicSlots;
            
            // Save equipped cores
            for (int i = 0; i < 3; i++)
            {
                if (EquippedCores[i] != null && !EquippedCores[i].IsAir)
                    tag[$"EquippedCore_{i}_Type"] = EquippedCores[i].type;
            }
            
            // Save ALL core enhancement levels by item type (persists even when unequipped)
            List<int> coreTypes = new List<int>();
            List<int> enhancementValues = new List<int>();
            foreach (var kvp in CoreEnhancementLevels)
            {
                if (kvp.Value > 0) // Only save non-zero enhancements
                {
                    coreTypes.Add(kvp.Key);
                    enhancementValues.Add(kvp.Value);
                }
            }
            tag["CoreEnhancementTypes"] = coreTypes;
            tag["CoreEnhancementValues"] = enhancementValues;
        }
        
        public override void LoadData(TagCompound tag)
        {
            HasKilledMoonLord = tag.GetBool("HasKilledMoonLord");
            HasUnlockedHarmonicSlots = tag.GetBool("HasUnlockedHarmonicSlots");
            
            // Load persistent core enhancements first
            CoreEnhancementLevels.Clear();
            if (tag.ContainsKey("CoreEnhancementTypes") && tag.ContainsKey("CoreEnhancementValues"))
            {
                var types = tag.GetList<int>("CoreEnhancementTypes");
                var values = tag.GetList<int>("CoreEnhancementValues");
                for (int i = 0; i < Math.Min(types.Count, values.Count); i++)
                {
                    CoreEnhancementLevels[types[i]] = values[i];
                }
            }
            
            // Load equipped cores and sync enhancement levels
            for (int i = 0; i < 3; i++)
            {
                if (tag.ContainsKey($"EquippedCore_{i}_Type"))
                {
                    int coreType = tag.GetInt($"EquippedCore_{i}_Type");
                    EquippedCores[i] = new Item();
                    EquippedCores[i].SetDefaults(coreType);
                    
                    // Sync slot-based enhancement from persistent dictionary
                    EnhancementLevels[i] = CoreEnhancementLevels.TryGetValue(coreType, out int level) ? level : 0;
                }
                else
                {
                    EquippedCores[i] = new Item();
                    EnhancementLevels[i] = 0;
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
        
        public bool HasCore(string coreName)
        {
            for (int i = 0; i < 3; i++)
                if (GetCoreName(i) == coreName) return true;
            return false;
        }
        
        /// <summary>
        /// Gets the slot index containing a specific core, or -1 if not found.
        /// </summary>
        public int GetCoreSlot(string coreName)
        {
            for (int i = 0; i < 3; i++)
                if (GetCoreName(i) == coreName) return i;
            return -1;
        }
        
        /// <summary>
        /// Gets the enhancement multiplier for a specific core type.
        /// </summary>
        public float GetCoreEnhancementMultiplier(string coreName)
        {
            int slot = GetCoreSlot(coreName);
            return slot >= 0 ? GetEnhancementMultiplier(slot) : 1f;
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
            
            // RESTORE enhancement level from persistent dictionary (keeps upgrades when re-equipping)
            int coreType = item.type;
            EnhancementLevels[slot] = CoreEnhancementLevels.TryGetValue(coreType, out int level) ? level : 0;
            
            SoundEngine.PlaySound(SoundID.Coins with { Volume = 0.8f, Pitch = 0.2f });
            SoundEngine.PlaySound(SoundID.Item4 with { Volume = 0.6f });
            
            // Show restoration message if core was previously enhanced
            if (EnhancementLevels[slot] > 0)
            {
                string coreName = GetCoreName(slot);
                Main.NewText($"{GetDisplayName(coreName)} restored at +{EnhancementLevels[slot]}!", new Color(180, 220, 255));
            }
        }
        
        public void UnequipCore(int slot)
        {
            if (slot < 0 || slot >= 3) return;
            
            // Enhancement level is already saved in CoreEnhancementLevels dictionary
            // No need to do anything special - just clear the slot
            
            EquippedCores[slot] = new Item();
            EnhancementLevels[slot] = 0; // Slot is empty, but persistent data remains in dictionary
            SoundEngine.PlaySound(SoundID.Grab);
        }
        
        /// <summary>
        /// Attempts to enhance a core slot. Returns true if successful.
        /// Enhancement is stored persistently by core TYPE, not slot.
        /// </summary>
        public bool TryEnhanceCore(int slot)
        {
            if (slot < 0 || slot >= 3) return false;
            if (EquippedCores[slot] == null || EquippedCores[slot].IsAir) return false;
            if (EnhancementLevels[slot] >= MaxEnhancementLevel) return false;
            
            // Check if player has Seed of Universal Melodies
            int seedType = ModContent.ItemType<Content.Items.SeedOfUniversalMelodies>();
            int seedIndex = -1;
            
            for (int i = 0; i < Player.inventory.Length; i++)
            {
                if (Player.inventory[i].type == seedType && Player.inventory[i].stack > 0)
                {
                    seedIndex = i;
                    break;
                }
            }
            
            if (seedIndex == -1) return false;
            
            // Consume seed and enhance
            Player.inventory[seedIndex].stack--;
            if (Player.inventory[seedIndex].stack <= 0)
                Player.inventory[seedIndex].TurnToAir();
            
            EnhancementLevels[slot]++;
            
            // SAVE enhancement to persistent dictionary by core TYPE
            int coreType = EquippedCores[slot].type;
            CoreEnhancementLevels[coreType] = EnhancementLevels[slot];
            
            // VFX and sound
            SoundEngine.PlaySound(SoundID.Item4 with { Volume = 0.8f, Pitch = 0.5f + EnhancementLevels[slot] * 0.1f });
            SoundEngine.PlaySound(SoundID.Item29 with { Volume = 0.6f });
            
            // Enhancement VFX
            for (int i = 0; i < 12; i++)
            {
                float angle = MathHelper.TwoPi * i / 12f;
                float hue = (float)i / 12f;
                Color sparkColor = Main.hslToRgb(hue, 1f, 0.8f);
                Vector2 pos = Player.Center + angle.ToRotationVector2() * 40f;
                CustomParticles.GenericFlare(pos, sparkColor, 0.5f, 20);
            }
            CustomParticles.HaloRing(Player.Center, Color.White, 0.6f, 25);
            
            string coreName = GetCoreName(slot);
            Main.NewText($"{GetDisplayName(coreName)} enhanced to +{EnhancementLevels[slot]}!", new Color(220, 180, 255));
            
            return true;
        }
        
        /// <summary>
        /// Gets the enhancement level for a slot.
        /// </summary>
        public int GetEnhancementLevel(int slot)
        {
            if (slot < 0 || slot >= 3) return 0;
            return EnhancementLevels[slot];
        }
        
        /// <summary>
        /// Gets the total enhancement multiplier for a slot (1.0 = base, up to 2.0 at +5)
        /// </summary>
        public float GetEnhancementMultiplier(int slot)
        {
            return 1f + GetEnhancementLevel(slot) * 0.2f; // +20% per level, up to 2x at +5
        }
        
        /// <summary>
        /// Gets the damage bonus multiplier including enhancement.
        /// </summary>
        public float GetEnhancedDamageBonus(int slot)
        {
            int tier = GetCoreTier(slot);
            if (tier <= 0 || tier > 6) return 0f;
            float baseBonus = TierDamageBonus[tier];
            return baseBonus * GetEnhancementMultiplier(slot);
        }
        
        private string GetDisplayName(string coreName) => coreName switch
        {
            "MoonlightSonata" => "Moonlight Sonata", "SwanLake" => "Swan Lake",
            "LaCampanella" => "La Campanella", _ => coreName
        };
        
        public Color GetRightmostCoreColor()
        {
            for (int i = 2; i >= 0; i--)
            {
                string name = GetCoreName(i);
                if (CoreColors.ContainsKey(name)) return CoreColors[name];
            }
            return Color.White;
        }
        
        // ========== STAT APPLICATION ==========
        public override void ModifyWeaponDamage(Item item, ref StatModifier damage)
        {
            if (!HasUnlockedHarmonicSlots || GetEquippedCoreCount() == 0) return;
            
            // Flat damage bonus from each equipped core's tier, scaled by enhancement
            for (int slot = 0; slot < 3; slot++)
            {
                int tier = GetCoreTier(slot);
                if (tier > 0 && tier <= 6)
                {
                    float enhancedBonus = GetEnhancedDamageBonus(slot);
                    damage += enhancedBonus;
                }
            }
        }
        
        // ========== UNIQUE EFFECTS UPDATE ==========
        public override void PostUpdate()
        {
            if (!HasUnlockedHarmonicSlots || GetEquippedCoreCount() == 0) return;
            
            // Update timers
            lunarAuraTimer++;
            featherAttackTimer++;
            prismaticFlareTimer++;
            bellAuraTimer++;
            
            // Process each core's unique effect
            if (HasCore("MoonlightSonata")) UpdateLunarAura();
            if (HasCore("SwanLake")) UpdateFeatheredGrace();
            if (HasCore("LaCampanella")) UpdateBellsResonance();
            if (HasCore("Enigma")) UpdateEnigmaticPresence();
            if (HasCore("Fate")) UpdateCosmicMarks();
        }
        
        // === MOONLIGHT SONATA: Lunar Aura ===
        // Soft purple glow, enemies in range take minor DoT
        // COSMETIC: Subtle, non-distracting ambient effect
        private void UpdateLunarAura()
        {
            float enhMult = GetCoreEnhancementMultiplier("MoonlightSonata");
            int enhancedDamage = (int)(LunarAuraDamage * enhMult);
            
            // SUBTLE visual aura particles - much less frequent
            if (lunarAuraTimer % 20 == 0)
            {
                float angle = Main.rand.NextFloat() * MathHelper.TwoPi;
                float radius = Main.rand.NextFloat(30f, LunarAuraRadius * 0.7f);
                Vector2 pos = Player.Center + angle.ToRotationVector2() * radius;
                
                Color auraColor = Color.Lerp(new Color(150, 120, 255), new Color(100, 80, 200), Main.rand.NextFloat());
                CustomParticles.GenericFlare(pos, auraColor * 0.3f, 0.15f, 15); // Smaller, more transparent
            }
            
            // Damage enemies in range every 30 frames
            if (lunarAuraTimer % 30 == 0)
            {
                foreach (NPC npc in Main.ActiveNPCs)
                {
                    if (!npc.friendly && npc.Distance(Player.Center) <= LunarAuraRadius)
                    {
                        npc.SimpleStrikeNPC(enhancedDamage, 0, false, 0f, null, false, 0f, true);
                        
                        // Small hit visual
                        CustomParticles.GenericFlare(npc.Center, new Color(180, 150, 255) * 0.6f, 0.25f, 10);
                    }
                }
            }
            
            // SUBTLE central player glow
            Lighting.AddLight(Player.Center, 0.2f * enhMult, 0.15f * enhMult, 0.3f * enhMult);
        }
        
        // === SWAN LAKE: Feathered Grace ===
        // Passive dodge chance + feathers that orbit and damage enemies
        // COSMETIC: Elegant but subtle orbiting effect
        private void UpdateFeatheredGrace()
        {
            float enhMult = GetCoreEnhancementMultiplier("SwanLake");
            int enhancedFeatherDamage = (int)(FeatherDamage * enhMult);
            
            // SUBTLE orbiting feather particles - less frequent, smaller
            if (featherAttackTimer % 30 == 0)
            {
                float baseAngle = Main.GameUpdateCount * 0.03f;
                for (int i = 0; i < 2; i++) // Reduced from 3 to 2
                {
                    float angle = baseAngle + MathHelper.TwoPi * i / 2f;
                    float radius = 45f + (float)Math.Sin(Main.GameUpdateCount * 0.05f + i) * 8f;
                    Vector2 featherPos = Player.Center + angle.ToRotationVector2() * radius;
                    
                    // Subtle white with slight rainbow shimmer
                    float hue = (Main.GameUpdateCount * 0.01f + i * 0.5f) % 1f;
                    Color featherColor = Color.Lerp(Color.White * 0.7f, Main.hslToRgb(hue, 0.4f, 0.9f), 0.3f);
                    
                    CustomParticles.GenericFlare(featherPos, featherColor * 0.5f, 0.2f, 12);
                }
            }
            
            // Feathers damage nearby enemies every 45 frames
            if (featherAttackTimer % 45 == 0)
            {
                foreach (NPC npc in Main.ActiveNPCs)
                {
                    if (!npc.friendly && npc.Distance(Player.Center) <= 80f)
                    {
                        npc.SimpleStrikeNPC(enhancedFeatherDamage, 0, false, 0f, null, false, 0f, true);
                        
                        // Subtle feather burst on hit
                        for (int i = 0; i < 3; i++)
                        {
                            float hue = Main.rand.NextFloat();
                            Color burstColor = Main.hslToRgb(hue, 0.5f, 0.85f);
                            Vector2 vel = Main.rand.NextVector2Circular(3f, 3f);
                            CustomParticles.GenericFlare(npc.Center + vel * 4f, burstColor * 0.6f, 0.3f, 15);
                        }
                        
                        SoundEngine.PlaySound(SoundID.Item24 with { Volume = 0.3f, Pitch = 0.5f }, npc.Center);
                    }
                }
            }
            
            // Very subtle glow
            Lighting.AddLight(Player.Center, 0.25f, 0.28f, 0.3f);
        }
        
        // === LA CAMPANELLA: Bell's Resonance ===
        // Orbiting infernal flames with smoke that damage enemies
        // COSMETIC: Subtle warm glow, particles only on damage dealt
        private void UpdateBellsResonance()
        {
            float enhMult = GetCoreEnhancementMultiplier("LaCampanella");
            int enhancedDamage = (int)(BellAuraDamage * enhMult);
            
            // Infernal theme colors - black to orange gradient
            Color bellBlack = new Color(20, 15, 20);
            Color bellOrange = new Color(255, 100, 0);
            Color bellYellow = new Color(255, 200, 50);
            Color bellGold = new Color(218, 165, 32);
            
            // === SUBTLE ORBITING FLAMES - just 2 small points ===
            if (bellAuraTimer % 15 == 0)
            {
                float baseAngle = Main.GameUpdateCount * 0.025f;
                for (int i = 0; i < 2; i++)
                {
                    float angle = baseAngle + MathHelper.Pi * i;
                    float radius = 45f + (float)Math.Sin(Main.GameUpdateCount * 0.04f + i * 0.8f) * 8f;
                    Vector2 flamePos = Player.Center + angle.ToRotationVector2() * radius;
                    
                    // Subtle flame flare
                    Color flameColor = Color.Lerp(bellOrange, bellYellow, 0.3f);
                    CustomParticles.GenericFlare(flamePos, flameColor * 0.4f, 0.2f, 10);
                }
            }
            
            // === DAMAGE ENEMIES IN RANGE ===
            if (bellAuraTimer % 40 == 0)
            {
                foreach (NPC npc in Main.ActiveNPCs)
                {
                    if (!npc.friendly && npc.Distance(Player.Center) <= 90f)
                    {
                        npc.SimpleStrikeNPC(enhancedDamage, 0, false, 0f, null, false, 0f, true);
                        
                        // Impact effects - keep these visible for feedback
                        CustomParticles.GenericFlare(npc.Center, bellYellow, 0.5f * enhMult, 12);
                        CustomParticles.GenericFlare(npc.Center, bellOrange, 0.4f * enhMult, 10);
                        CustomParticles.HaloRing(npc.Center, bellGold * 0.6f, 0.3f * enhMult, 12);
                        
                        // Small smoke puff
                        var smoke = new HeavySmokeParticle(
                            npc.Center, Main.rand.NextVector2Circular(1f, 1f), bellBlack,
                            Main.rand.Next(15, 22), 0.2f, 0.35f, 0.02f, false
                        );
                        MagnumParticleHandler.SpawnParticle(smoke);
                        
                        SoundEngine.PlaySound(SoundID.Item35 with { Volume = 0.4f, Pitch = 0.3f }, npc.Center);
                    }
                }
            }
            
            // Subtle warm player glow
            Lighting.AddLight(Player.Center, 0.35f * enhMult, 0.2f * enhMult, 0.08f * enhMult);
        }
        
        // === ENIGMA: Enigmatic Presence ===
        // Watching eyes and arcane glyphs that seek and damage enemies
        // COSMETIC: Subtle mysterious aura, save flashy effects for attacks
        private void UpdateEnigmaticPresence()
        {
            float enhMult = GetCoreEnhancementMultiplier("Enigma");
            
            // Enigma theme colors
            Color enigmaBlack = new Color(15, 10, 20);
            Color enigmaPurple = new Color(140, 60, 200);
            Color enigmaDeepPurple = new Color(80, 20, 120);
            Color enigmaGreenFlame = new Color(50, 220, 100);
            
            // === SUBTLE ORBITING GLYPH - just 1, rarely ===
            if (prismaticFlareTimer % 35 == 0)
            {
                float glyphAngle = Main.GameUpdateCount * 0.02f;
                float radius = 40f + (float)Math.Sin(Main.GameUpdateCount * 0.05f) * 8f;
                Vector2 glyphPos = Player.Center + glyphAngle.ToRotationVector2() * radius;
                
                Color glyphColor = Color.Lerp(enigmaPurple, enigmaGreenFlame, 0.3f);
                CustomParticles.GenericFlare(glyphPos, glyphColor * 0.4f, 0.25f, 12);
            }
            
            // === PERIODIC ENIGMA STRIKE ===
            // Every 60 frames, launch an enigmatic bolt (the main effect)
            if (prismaticFlareTimer % 60 == 0)
            {
                NPC target = null;
                float nearestDist = 400f;
                
                foreach (NPC npc in Main.ActiveNPCs)
                {
                    if (!npc.friendly)
                    {
                        float dist = npc.Distance(Player.Center);
                        if (dist < nearestDist)
                        {
                            nearestDist = dist;
                            target = npc;
                        }
                    }
                }
                
                if (target != null)
                {
                    LaunchEnigmaBolt(target, enhMult);
                }
            }
            
            // Subtle mysterious glow
            Lighting.AddLight(Player.Center, 0.15f * enhMult, 0.1f * enhMult, 0.22f * enhMult);
        }
        
        private void LaunchEnigmaBolt(NPC target, float enhMult = 1f)
        {
            int enhancedDamage = (int)(PrismaticFlareDamage * enhMult);
            
            // Enigma theme colors
            Color enigmaBlack = new Color(15, 10, 20);
            Color enigmaPurple = new Color(140, 60, 200);
            Color enigmaDeepPurple = new Color(80, 20, 120);
            Color enigmaGreenFlame = new Color(50, 220, 100);
            
            Vector2 start = Player.Center;
            Vector2 end = target.Center;
            Vector2 direction = (end - start).SafeNormalize(Vector2.UnitY);
            float distance = Vector2.Distance(start, end);
            
            // === ENIGMATIC BEAM TRAIL ===
            // Draw mysterious bolt with purple->green gradient
            int segments = (int)(distance / 18f);
            for (int i = 0; i <= segments; i++)
            {
                float progress = (float)i / segments;
                Vector2 pos = Vector2.Lerp(start, end, progress);
                
                // Add wave pattern to beam
                float waveOffset = (float)Math.Sin(progress * MathHelper.Pi * 4f + Main.GameUpdateCount * 0.2f) * 8f;
                Vector2 perpendicular = direction.RotatedBy(MathHelper.PiOver2);
                pos += perpendicular * waveOffset;
                
                // Gradient from purple to green flame
                Color beamColor = Color.Lerp(enigmaPurple, enigmaGreenFlame, progress);
                
                // Core beam
                CustomParticles.GenericFlare(pos, beamColor, (0.5f - progress * 0.15f) * enhMult, 12);
                
                // Dark underlayer
                if (i % 2 == 0)
                    CustomParticles.GenericFlare(pos, enigmaDeepPurple * 0.6f, 0.35f * enhMult, 15);
                
                Lighting.AddLight(pos, beamColor.ToVector3() * 0.3f);
            }
            
            // === IMPACT - WATCHING EYE BURST ===
            target.SimpleStrikeNPC(enhancedDamage, 0, true, 0f, null, false, 0f, true);
            
            // Central flash - green flame core
            CustomParticles.GenericFlare(target.Center, enigmaGreenFlame, 0.8f * enhMult, 18);
            CustomParticles.GenericFlare(target.Center, Color.White * 0.7f, 0.4f * enhMult, 12);
            
            // Halo rings with gradient
            for (int ring = 0; ring < 3; ring++)
            {
                float ringProgress = ring / 3f;
                Color ringColor = Color.Lerp(enigmaGreenFlame, enigmaPurple, ringProgress);
                CustomParticles.HaloRing(target.Center, ringColor * 0.8f, (0.35f + ring * 0.15f) * enhMult, 14 + ring * 3);
            }
            
            // 6-point burst with glyph-like positions
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f;
                Vector2 offset = angle.ToRotationVector2() * 28f;
                float progress = (float)i / 6f;
                Color burstColor = Color.Lerp(enigmaPurple, enigmaGreenFlame, progress);
                CustomParticles.GenericFlare(target.Center + offset, burstColor, 0.5f * enhMult, 18);
            }
            
            // Void mist burst
            for (int s = 0; s < 5; s++)
            {
                Vector2 mistVel = Main.rand.NextVector2Circular(2.5f, 2.5f);
                Color mistColor = Color.Lerp(enigmaBlack, enigmaDeepPurple, Main.rand.NextFloat(0.3f, 0.6f));
                var mist = new HeavySmokeParticle(
                    target.Center, mistVel, mistColor,
                    Main.rand.Next(18, 28), 0.3f * enhMult, 0.5f * enhMult, 0.018f, false
                );
                MagnumParticleHandler.SpawnParticle(mist);
            }
            
            // Sparks in green/purple
            for (int sp = 0; sp < 8; sp++)
            {
                float sparkAngle = MathHelper.TwoPi * sp / 8f + Main.rand.NextFloat(-0.2f, 0.2f);
                Vector2 sparkVel = sparkAngle.ToRotationVector2() * Main.rand.NextFloat(5f, 9f);
                Color sparkColor = sp % 2 == 0 ? enigmaGreenFlame : enigmaPurple;
                var spark = new GenericGlowParticle(target.Center, sparkVel, sparkColor, 0.35f, 20, true);
                MagnumParticleHandler.SpawnParticle(spark);
            }
            
            SoundEngine.PlaySound(SoundID.Item125 with { Volume = 0.55f, Pitch = -0.2f }, target.Center);
            Lighting.AddLight(target.Center, enigmaGreenFlame.ToVector3() * 0.8f);
        }
        
        // === FATE: Cosmic Destiny ===
        // Process delayed cosmic marks that deal massive damage
        // Features dark prismatic visuals: black → dark pink → bright red
        // COSMETIC: Subtle ambient effect, visible feedback only on marks/strikes
        private void UpdateCosmicMarks()
        {
            float enhMult = GetCoreEnhancementMultiplier("Fate");
            
            // Fate theme colors - DARK PRISMATIC (black base with pink/red accents)
            Color fateBlack = new Color(15, 5, 20);
            Color fateDarkPink = new Color(180, 50, 100);
            Color fateBrightRed = new Color(255, 60, 80);
            Color fatePurple = new Color(120, 30, 140);
            
            // === SUBTLE AMBIENT COSMIC AURA - less frequent ===
            if (Main.GameUpdateCount % 20 == 0)
            {
                float orbitAngle = Main.GameUpdateCount * 0.018f;
                for (int i = 0; i < 2; i++)
                {
                    float angle = orbitAngle + MathHelper.Pi * i;
                    float radius = 45f + (float)Math.Sin(Main.GameUpdateCount * 0.04f + i * 0.9f) * 10f;
                    Vector2 cosmicPos = Player.Center + angle.ToRotationVector2() * radius;
                    
                    Color cosmicColor = Color.Lerp(fateDarkPink, fateBrightRed, (float)i / 2f);
                    CustomParticles.GenericFlare(cosmicPos, cosmicColor * 0.4f, 0.2f, 10);
                }
            }
            
            // === PROCESS COSMIC MARKS ===
            for (int i = cosmicMarks.Count - 1; i >= 0; i--)
            {
                var mark = cosmicMarks[i];
                mark.Timer--;
                cosmicMarks[i] = mark;
                
                // Get NPC position if still alive
                Vector2 markPos = mark.Position;
                if (mark.NPCIndex >= 0 && mark.NPCIndex < Main.maxNPCs && Main.npc[mark.NPCIndex].active)
                    markPos = Main.npc[mark.NPCIndex].Center;
                
                // === MARK VISUALIZATION - grows as timer counts down ===
                float markProgress = 1f - (float)mark.Timer / CosmicMarkDelay;
                
                // Pulsing ring around marked target
                if (mark.Timer % 6 == 0)
                {
                    float pulseScale = 0.2f + markProgress * 0.4f;
                    Color pulseColor = Color.Lerp(fateDarkPink, fateBrightRed, markProgress) * (0.5f + markProgress * 0.5f);
                    CustomParticles.HaloRing(markPos, pulseColor, pulseScale, 8);
                }
                
                // Converging particles as countdown progresses
                if (mark.Timer % 4 == 0 && markProgress > 0.3f)
                {
                    float convergeAngle = Main.rand.NextFloat() * MathHelper.TwoPi;
                    float convergeRadius = 40f * (1f - markProgress) + 5f;
                    Vector2 particlePos = markPos + convergeAngle.ToRotationVector2() * convergeRadius;
                    Vector2 particleVel = (markPos - particlePos).SafeNormalize(Vector2.Zero) * 3f;
                    
                    Color particleColor = Color.Lerp(fatePurple, fateBrightRed, markProgress);
                    var converge = new GenericGlowParticle(particlePos, particleVel, particleColor, 0.25f, 10, true);
                    MagnumParticleHandler.SpawnParticle(converge);
                }
                
                // Warning flare at 10 frames
                if (mark.Timer == 10)
                {
                    CustomParticles.GenericFlare(markPos, fateBrightRed * 0.8f, 0.6f, 10);
                    CustomParticles.HaloRing(markPos, Color.White * 0.6f, 0.3f, 8);
                }
                
                // Trigger cosmic strike
                if (mark.Timer <= 0)
                {
                    TriggerCosmicStrike(mark);
                    cosmicMarks.RemoveAt(i);
                }
            }
            
            // Subtle dark prismatic player glow
            Lighting.AddLight(Player.Center, 0.28f * enhMult, 0.1f * enhMult, 0.2f * enhMult);
        }
        
        private void TriggerCosmicStrike(CosmicMark mark)
        {
            Vector2 pos = mark.Position;
            
            // Fate theme colors - DARK PRISMATIC
            Color fateBlack = new Color(15, 5, 20);
            Color fateDarkPink = new Color(180, 50, 100);
            Color fateBrightRed = new Color(255, 60, 80);
            Color fatePurple = new Color(120, 30, 140);
            
            // Check if NPC is still there
            if (mark.NPCIndex >= 0 && mark.NPCIndex < Main.maxNPCs)
            {
                NPC npc = Main.npc[mark.NPCIndex];
                if (npc.active && !npc.friendly)
                {
                    pos = npc.Center;
                    npc.SimpleStrikeNPC(mark.Damage, 0, true, 0f, null, false, 0f, true);
                }
            }
            
            // === COSMIC STRIKE VFX - DARK PRISMATIC EXPLOSION ===
            SoundEngine.PlaySound(SoundID.Item125 with { Volume = 0.75f, Pitch = -0.1f }, pos);
            
            // === PHASE 1: VOID CORE ===
            // Central dark core with white flash
            CustomParticles.GenericFlare(pos, Color.White, 1.4f, 18);
            CustomParticles.GenericFlare(pos, fateBrightRed, 1.0f, 20);
            CustomParticles.GenericFlare(pos, fateDarkPink, 0.75f, 22);
            CustomParticles.GenericFlare(pos, fateBlack, 0.5f, 25);
            
            // === PHASE 2: LAYERED HALO RINGS ===
            // Dark prismatic gradient rings expanding outward
            for (int ring = 0; ring < 5; ring++)
            {
                float ringProgress = ring / 5f;
                Color ringColor;
                if (ringProgress < 0.4f)
                    ringColor = Color.Lerp(Color.White, fateDarkPink, ringProgress * 2.5f);
                else if (ringProgress < 0.7f)
                    ringColor = Color.Lerp(fateDarkPink, fateBrightRed, (ringProgress - 0.4f) * 3.3f);
                else
                    ringColor = Color.Lerp(fateBrightRed, fatePurple, (ringProgress - 0.7f) * 3.3f);
                
                CustomParticles.HaloRing(pos, ringColor * 0.85f, 0.25f + ring * 0.18f, 12 + ring * 4);
            }
            
            // === PHASE 3: COSMIC STAR BURST ===
            // 8-point cosmic star with gradient
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                float burstProgress = (float)i / 8f;
                
                // Outer burst
                Vector2 outerOffset = angle.ToRotationVector2() * 40f;
                Color outerColor = Color.Lerp(fateDarkPink, fateBrightRed, burstProgress);
                CustomParticles.GenericFlare(pos + outerOffset, outerColor, 0.55f, 18);
                
                // Inner secondary burst (offset by 22.5 degrees)
                float innerAngle = angle + MathHelper.Pi / 8f;
                Vector2 innerOffset = innerAngle.ToRotationVector2() * 25f;
                Color innerColor = Color.Lerp(fatePurple, fateDarkPink, burstProgress);
                CustomParticles.GenericFlare(pos + innerOffset, innerColor, 0.4f, 16);
            }
            
            // === PHASE 4: RADIAL SPARK STORM ===
            // Dark prismatic sparks bursting outward
            for (int i = 0; i < 14; i++)
            {
                float sparkAngle = MathHelper.TwoPi * i / 14f + Main.rand.NextFloat(-0.15f, 0.15f);
                float speed = Main.rand.NextFloat(6f, 12f);
                Vector2 sparkVel = sparkAngle.ToRotationVector2() * speed;
                
                // Gradient across the burst
                float sparkProgress = (float)i / 14f;
                Color sparkColor;
                if (sparkProgress < 0.33f)
                    sparkColor = Color.Lerp(Color.White, fateDarkPink, Main.rand.NextFloat(0.5f, 1f));
                else if (sparkProgress < 0.66f)
                    sparkColor = Color.Lerp(fateDarkPink, fateBrightRed, Main.rand.NextFloat(0.5f, 1f));
                else
                    sparkColor = Color.Lerp(fateBrightRed, fatePurple, Main.rand.NextFloat(0.5f, 1f));
                
                var spark = new GenericGlowParticle(pos, sparkVel, sparkColor, 0.42f, 24, true);
                MagnumParticleHandler.SpawnParticle(spark);
            }
            
            // === PHASE 5: VOID MIST BURST ===
            // Dark smoke/mist expanding
            for (int m = 0; m < 6; m++)
            {
                Vector2 mistVel = Main.rand.NextVector2Circular(3.5f, 3.5f);
                Color mistColor = Color.Lerp(fateBlack, fatePurple, Main.rand.NextFloat(0.2f, 0.5f));
                var mist = new HeavySmokeParticle(
                    pos, mistVel, mistColor,
                    Main.rand.Next(22, 35), 0.35f, 0.55f, 0.02f, false
                );
                MagnumParticleHandler.SpawnParticle(mist);
            }
            
            // === PHASE 6: AFTERGLOW TRAILS ===
            // Brief echo trails radiating outward
            for (int e = 0; e < 6; e++)
            {
                float echoAngle = MathHelper.TwoPi * e / 6f;
                for (int seg = 1; seg <= 3; seg++)
                {
                    Vector2 echoPos = pos + echoAngle.ToRotationVector2() * (15f * seg);
                    float echoAlpha = 1f - seg * 0.25f;
                    Color echoColor = Color.Lerp(fateBrightRed, fateDarkPink, seg / 3f) * echoAlpha;
                    CustomParticles.GenericFlare(echoPos, echoColor, 0.3f, 12 - seg * 2);
                }
            }
            
            // Dramatic lighting
            Lighting.AddLight(pos, 1.2f, 0.35f, 0.5f);
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
            if (!HasUnlockedHarmonicSlots || GetEquippedCoreCount() == 0) return;
            
            // La Campanella: Bell's Resonance - echo damage on hit
            if (HasCore("LaCampanella"))
            {
                float enhMult = GetCoreEnhancementMultiplier("LaCampanella");
                float enhancedPercent = BellEchoDamagePercent * enhMult;
                int echoDamage = (int)(damageDone * enhancedPercent);
                
                // La Campanella theme colors
                Color bellBlack = new Color(20, 15, 20);
                Color bellOrange = new Color(255, 100, 0);
                Color bellYellow = new Color(255, 200, 50);
                Color bellGold = new Color(218, 165, 32);
                
                // Delayed echo strike
                if (bellEchoTimer <= 0)
                {
                    bellEchoTimer = 20;
                    
                    // Apply echo damage
                    target.SimpleStrikeNPC(echoDamage, 0, false, 0f, null, false, 0f, true);
                    
                    // === BELL CHIME IMPACT VFX ===
                    // Central flash - fiery core
                    CustomParticles.GenericFlare(target.Center, Color.White * 0.9f, 0.8f * enhMult, 15);
                    CustomParticles.GenericFlare(target.Center, bellYellow, 0.65f * enhMult, 18);
                    CustomParticles.GenericFlare(target.Center, bellOrange, 0.5f * enhMult, 20);
                    
                    // Multiple halo rings with gradient
                    for (int ring = 0; ring < 3; ring++)
                    {
                        float ringProgress = ring / 3f;
                        Color ringColor = Color.Lerp(bellYellow, bellOrange, ringProgress);
                        CustomParticles.HaloRing(target.Center, ringColor * 0.8f, (0.3f + ring * 0.15f) * enhMult, 12 + ring * 3);
                    }
                    
                    // 6-point bell chime burst
                    for (int i = 0; i < 6; i++)
                    {
                        float angle = MathHelper.TwoPi * i / 6f;
                        Vector2 offset = angle.ToRotationVector2() * 25f;
                        float progress = (float)i / 6f;
                        Color burstColor = Color.Lerp(bellOrange, bellGold, progress);
                        CustomParticles.GenericFlare(target.Center + offset, burstColor, 0.45f * enhMult, 16);
                    }
                    
                    // Smoke wisps
                    for (int s = 0; s < 3; s++)
                    {
                        Vector2 smokeVel = Main.rand.NextVector2Circular(1.5f, 1.5f) + new Vector2(0, -0.5f);
                        var smoke = new HeavySmokeParticle(
                            target.Center, smokeVel, bellBlack,
                            Main.rand.Next(18, 28), 0.25f * enhMult, 0.4f * enhMult, 0.018f, false
                        );
                        MagnumParticleHandler.SpawnParticle(smoke);
                    }
                    
                    // Sparks burst
                    for (int sp = 0; sp < 8; sp++)
                    {
                        float sparkAngle = MathHelper.TwoPi * sp / 8f + Main.rand.NextFloat(-0.2f, 0.2f);
                        Vector2 sparkVel = sparkAngle.ToRotationVector2() * Main.rand.NextFloat(4f, 8f);
                        Color sparkColor = Color.Lerp(bellOrange, bellYellow, (float)sp / 8f);
                        var spark = new GenericGlowParticle(target.Center, sparkVel, sparkColor, 0.32f, 20, true);
                        MagnumParticleHandler.SpawnParticle(spark);
                    }
                    
                    // Bell chime sound
                    SoundEngine.PlaySound(SoundID.Item35 with { Volume = 0.5f, Pitch = 0.3f }, target.Center);
                    Lighting.AddLight(target.Center, bellOrange.ToVector3() * 0.7f);
                }
            }
            
            // Fate: Cosmic Destiny - queue cosmic mark
            if (HasCore("Fate"))
            {
                float enhMult = GetCoreEnhancementMultiplier("Fate");
                float enhancedPercent = CosmicMarkDamagePercent * enhMult;
                int cosmicDamage = (int)(damageDone * enhancedPercent);
                
                // Fate theme colors
                Color fateDarkPink = new Color(180, 50, 100);
                Color fateBrightRed = new Color(255, 60, 80);
                Color fatePurple = new Color(120, 30, 140);
                
                cosmicMarks.Add(new CosmicMark
                {
                    NPCIndex = target.whoAmI,
                    Timer = CosmicMarkDelay,
                    Damage = cosmicDamage,
                    Position = target.Center
                });
                
                // === MARK APPLIED VISUAL ===
                // Quick flash to show mark applied
                CustomParticles.GenericFlare(target.Center, fateBrightRed * 0.6f, 0.4f * enhMult, 12);
                
                // Mini halo
                CustomParticles.HaloRing(target.Center, fateDarkPink * 0.5f, 0.2f * enhMult, 10);
                
                // 4-point mark indicator
                for (int i = 0; i < 4; i++)
                {
                    float angle = MathHelper.TwoPi * i / 4f + Main.rand.NextFloat(-0.1f, 0.1f);
                    Vector2 offset = angle.ToRotationVector2() * 18f;
                    Color markColor = Color.Lerp(fatePurple, fateDarkPink, (float)i / 4f) * 0.6f;
                    CustomParticles.GenericFlare(target.Center + offset, markColor, 0.25f * enhMult, 15);
                }
                
                Lighting.AddLight(target.Center, fateDarkPink.ToVector3() * 0.3f);
            }
            
            bellEchoTimer--;
        }
        
        // ========== ON-KILL EFFECTS ==========
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!HasUnlockedHarmonicSlots || GetEquippedCoreCount() == 0) return;
            
            // Eroica: Heroic Rally - killing enemies heals nearby
            if (HasCore("Eroica") && target.life <= 0)
            {
                float enhMult = GetCoreEnhancementMultiplier("Eroica");
                int enhancedHeal = (int)(HeroicRallyHealAmount * enhMult);
                
                // Heal player
                Player.statLife = Math.Min(Player.statLife + enhancedHeal, Player.statLifeMax2);
                Player.HealEffect(enhancedHeal, true);
                
                // Healing burst VFX
                CustomParticles.GenericFlare(target.Center, new Color(255, 150, 200), 0.7f * enhMult, 20);
                CustomParticles.HaloRing(target.Center, new Color(255, 180, 200) * 0.6f, 0.5f * enhMult, 18);
                
                // Healing particles toward player
                for (int i = 0; i < 6; i++)
                {
                    Vector2 vel = (Player.Center - target.Center).SafeNormalize(Vector2.Zero) * 8f;
                    vel = vel.RotatedByRandom(0.5f);
                    var healParticle = new GenericGlowParticle(target.Center, vel, new Color(255, 180, 200), 0.35f, 25, true);
                    MagnumParticleHandler.SpawnParticle(healParticle);
                }
                
                SoundEngine.PlaySound(SoundID.Item4 with { Volume = 0.4f, Pitch = 0.3f }, target.Center);
            }
        }
        
        // ========== DEFENSIVE EFFECTS ==========
        public override bool FreeDodge(Player.HurtInfo info)
        {
            if (!HasUnlockedHarmonicSlots) return false;
            
            // Swan Lake: Feathered Grace - dodge chance (scaled by enhancement)
            if (HasCore("SwanLake"))
            {
                float enhMult = GetCoreEnhancementMultiplier("SwanLake");
                float enhancedDodge = featherDodgeChance * enhMult;
                
                if (Main.rand.NextFloat() < enhancedDodge)
                {
                    // Dodge VFX - feather burst
                    for (int i = 0; i < 8; i++)
                    {
                        float hue = Main.rand.NextFloat();
                        Color featherColor = Main.hslToRgb(hue, 0.7f, 0.85f);
                        Vector2 vel = Main.rand.NextVector2Circular(5f, 5f);
                        CustomParticles.GenericFlare(Player.Center + vel * 3f, featherColor, 0.5f * enhMult, 20);
                    }
                    
                    SoundEngine.PlaySound(SoundID.Item24 with { Volume = 0.5f, Pitch = 0.3f }, Player.Center);
                    return true;
                }
            }
            
            // Enigma: Mystery Shield - reflect chance (scaled by enhancement)
            if (HasCore("Enigma"))
            {
                float enhMult = GetCoreEnhancementMultiplier("Enigma");
                float enhancedReflect = reflectChance * enhMult;
                
                if (Main.rand.NextFloat() < enhancedReflect)
                {
                    // Reflect VFX
                    for (int i = 0; i < 6; i++)
                    {
                        float hue = (float)i / 6f;
                        Color shieldColor = Main.hslToRgb(hue, 1f, 0.7f);
                        float angle = MathHelper.TwoPi * i / 6f;
                        Vector2 offset = angle.ToRotationVector2() * 30f * enhMult;
                        CustomParticles.GenericFlare(Player.Center + offset, shieldColor, 0.6f * enhMult, 18);
                    }
                    
                    CustomParticles.HaloRing(Player.Center, new Color(150, 255, 200), 0.6f * enhMult, 15);
                    SoundEngine.PlaySound(SoundID.Item150 with { Volume = 0.5f }, Player.Center);
                    return true;
                }
            }
            
            return false;
        }
        
        // ========== BUFF DESCRIPTION HELPERS (for UI) ==========
        public static string GetCoreEffectName(string coreName)
        {
            return coreName switch
            {
                "MoonlightSonata" => "Lunar Aura",
                "Eroica" => "Heroic Rally",
                "SwanLake" => "Feathered Grace",
                "LaCampanella" => "Bell's Resonance",
                "Enigma" => "Prismatic Flares",
                "Fate" => "Cosmic Destiny",
                _ => "Unknown"
            };
        }
        
        public static string GetCoreEffectDesc(string coreName)
        {
            return GetCoreEffectDescWithEnhancement(coreName, 0);
        }
        
        /// <summary>
        /// Gets the effect description with values scaled by enhancement level.
        /// Matches item tooltips for consistency.
        /// </summary>
        public static string GetCoreEffectDescWithEnhancement(string coreName, int enhancementLevel)
        {
            float mult = 1f + enhancementLevel * 0.2f; // +20% per level
            
            return coreName switch
            {
                "MoonlightSonata" => $"Damages nearby enemies for {(int)(8 * mult)}/hit",
                "Eroica" => $"Restores {(int)(15 * mult)} HP per enemy slain",
                "SwanLake" => $"{(int)(12 * mult)}% dodge, feathers deal {(int)(45 * mult)} damage",
                "LaCampanella" => $"Bell echoes deal {(int)(25 * mult)}% bonus damage",
                "Enigma" => $"{(int)(15 * mult)}% deflect, flares deal {(int)(65 * mult)} damage",
                "Fate" => $"Cosmic marks explode for {(int)(40 * mult)}% bonus damage",
                _ => "No effect"
            };
        }
        
        /// <summary>
        /// Gets the base effect values for a core type (used for calculations).
        /// </summary>
        public static (int damage, float percent, int healing) GetBaseEffectValues(string coreName)
        {
            return coreName switch
            {
                "MoonlightSonata" => (8, 0f, 0),
                "Eroica" => (0, 0f, 15),
                "SwanLake" => (45, 0.12f, 0),
                "LaCampanella" => (0, 0.25f, 0),
                "Enigma" => (65, 0.15f, 0),
                "Fate" => (0, 0.40f, 0),
                _ => (0, 0f, 0)
            };
        }
        
        /// <summary>
        /// Comprehensive stats object for UI display.
        /// </summary>
        public class CoreStats
        {
            public string Name { get; set; }
            public string DisplayName { get; set; }
            public int Tier { get; set; }
            public int EnhancementLevel { get; set; }
            public float DamageBonus { get; set; }
            public string EffectName { get; set; }
            public string EffectDescription { get; set; }
            public Color ThemeColor { get; set; }
        }
        
        /// <summary>
        /// Gets comprehensive stats for all equipped cores for UI display.
        /// </summary>
        public List<CoreStats> GetEquippedCoreStats()
        {
            var stats = new List<CoreStats>();
            
            for (int slot = 0; slot < 3; slot++)
            {
                if (EquippedCores[slot] == null || EquippedCores[slot].IsAir)
                    continue;
                
                string coreName = GetCoreName(slot);
                if (string.IsNullOrEmpty(coreName))
                    continue;
                
                int tier = GetCoreTier(slot);
                int enhancement = GetEnhancementLevel(slot);
                float dmgBonus = GetEnhancedDamageBonus(slot);
                
                stats.Add(new CoreStats
                {
                    Name = coreName,
                    DisplayName = coreName switch
                    {
                        "MoonlightSonata" => "Moonlight Sonata",
                        "SwanLake" => "Swan Lake",
                        "LaCampanella" => "La Campanella",
                        _ => coreName
                    },
                    Tier = tier,
                    EnhancementLevel = enhancement,
                    DamageBonus = dmgBonus,
                    EffectName = GetCoreEffectName(coreName),
                    EffectDescription = GetCoreEffectDescWithEnhancement(coreName, enhancement),
                    ThemeColor = CoreColors.TryGetValue(coreName, out var color) ? color : Color.White
                });
            }
            
            return stats;
        }
        
        /// <summary>
        /// Gets the total damage bonus from all equipped cores.
        /// </summary>
        public float GetTotalDamageBonus()
        {
            float total = 0f;
            for (int slot = 0; slot < 3; slot++)
            {
                total += GetEnhancedDamageBonus(slot);
            }
            return total;
        }
    }
}
