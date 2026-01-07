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
    /// Complete Harmonic Core system - appears after Moon Lord kill.
    /// Provides 2 selectable buffs (offensive/defensive) + 4 class stat boosts.
    /// </summary>
    public class HarmonicCoreModPlayer : ModPlayer
    {
        // Core state
        public Item EquippedCore = new Item();
        public bool HasKilledMoonLord = false;
        
        // Buff selection: true = offensive, false = defensive
        public bool UsingOffensiveBuff = true;
        
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
        
        // Class buff percentages per tier (damage boost)
        public static readonly float[] TierDamageBonus = { 0f, 0.04f, 0.06f, 0.08f, 0.10f, 0.12f, 0.15f };
        
        // ========== CALCULATED STATS ==========
        public float MeleeDamageBonus => GetClassBonus();
        public float RangedDamageBonus => GetClassBonus();
        public float MagicDamageBonus => GetClassBonus();
        public float SummonDamageBonus => GetClassBonus();
        
        // Special buff tracking
        private int bellTollCooldown = 0;
        private int fateShieldCooldown = 0;
        private const int FateShieldCooldownMax = 3600; // 1 minute
        
        // === UNIQUE SET BONUS TRACKERS ===
        // Moonlight Sonata
        private int moonlightPhase = 0; // 0-3 for crescent phases
        private int moonlightPhaseTimer = 0;
        private float moonlightStoredDamage = 0f;
        
        // Eroica
        private int eroicaComboCount = 0;
        private int eroicaComboTimer = 0;
        private bool eroicaRallyActive = false;
        private int eroicaRallyTimer = 0;
        
        // Swan Lake
        private bool swanGliding = false;
        private int swanGracePeriod = 0;
        private float swanDodgeChance = 0f;
        
        // La Campanella
        private int campanellaResonance = 0; // Stacks 0-12
        private int campanellaEchoTimer = 0;
        
        // Enigma
        private int enigmaCurrentBonus = 0; // 0-5 random bonus type
        private int enigmaBonusTimer = 0;
        
        // Fate
        private bool fateMarkedEnemy = false;
        private int fateMarkedNPC = -1;
        private int fateDeathsAvoided = 0;
        
        public override void Initialize()
        {
            EquippedCore = new Item();
            HasKilledMoonLord = false;
            UsingOffensiveBuff = true;
            ResetAllBonusTrackers();
        }
        
        private void ResetAllBonusTrackers()
        {
            moonlightPhase = 0;
            moonlightPhaseTimer = 0;
            moonlightStoredDamage = 0f;
            eroicaComboCount = 0;
            eroicaComboTimer = 0;
            eroicaRallyActive = false;
            swanGliding = false;
            swanGracePeriod = 0;
            campanellaResonance = 0;
            enigmaCurrentBonus = 0;
            enigmaBonusTimer = 0;
            fateMarkedNPC = -1;
            fateDeathsAvoided = 0;
        }
        
        public override void SaveData(TagCompound tag)
        {
            tag["HasKilledMoonLord"] = HasKilledMoonLord;
            tag["UsingOffensiveBuff"] = UsingOffensiveBuff;
            tag["FateDeathsAvoided"] = fateDeathsAvoided;
            
            if (EquippedCore != null && !EquippedCore.IsAir)
            {
                tag["EquippedCoreType"] = EquippedCore.type;
            }
        }
        
        public override void LoadData(TagCompound tag)
        {
            HasKilledMoonLord = tag.GetBool("HasKilledMoonLord");
            UsingOffensiveBuff = tag.GetBool("UsingOffensiveBuff");
            fateDeathsAvoided = tag.GetInt("FateDeathsAvoided");
            
            if (tag.ContainsKey("EquippedCoreType"))
            {
                int coreType = tag.GetInt("EquippedCoreType");
                EquippedCore = new Item();
                EquippedCore.SetDefaults(coreType);
            }
        }
        
        public string GetEquippedCoreName()
        {
            if (EquippedCore == null || EquippedCore.IsAir) return "";
            
            string typeName = EquippedCore.ModItem?.GetType().Name ?? "";
            
            if (typeName.Contains("MoonlightSonata")) return "MoonlightSonata";
            if (typeName.Contains("Eroica")) return "Eroica";
            if (typeName.Contains("SwanLake")) return "SwanLake";
            if (typeName.Contains("LaCampanella")) return "LaCampanella";
            if (typeName.Contains("Enigma")) return "Enigma";
            if (typeName.Contains("Fate")) return "Fate";
            
            return "";
        }
        
        public int GetCoreTier()
        {
            string name = GetEquippedCoreName();
            return CoreTiers.ContainsKey(name) ? CoreTiers[name] : 0;
        }
        
        private float GetClassBonus()
        {
            int tier = GetCoreTier();
            if (tier <= 0 || tier > 6) return 0f;
            return TierDamageBonus[tier];
        }
        
        public void EquipCore(Item item)
        {
            EquippedCore = item.Clone();
            
            // Play cha-ching sound
            SoundEngine.PlaySound(SoundID.Coins with { Volume = 0.8f, Pitch = 0.2f });
            SoundEngine.PlaySound(SoundID.Item4 with { Volume = 0.6f });
        }
        
        public void UnequipCore()
        {
            EquippedCore = new Item();
            SoundEngine.PlaySound(SoundID.Grab);
        }
        
        public void ToggleBuffType()
        {
            UsingOffensiveBuff = !UsingOffensiveBuff;
            SoundEngine.PlaySound(SoundID.MenuTick);
        }
        
        // ========== STAT APPLICATION ==========
        public override void ModifyWeaponDamage(Item item, ref StatModifier damage)
        {
            if (EquippedCore == null || EquippedCore.IsAir) return;
            
            float bonus = GetClassBonus();
            string coreName = GetEquippedCoreName();
            
            // Apply class-specific bonus
            if (item.DamageType == DamageClass.Melee)
                damage += bonus;
            else if (item.DamageType == DamageClass.Ranged)
                damage += bonus;
            else if (item.DamageType == DamageClass.Magic)
                damage += bonus;
            else if (item.DamageType == DamageClass.Summon)
                damage += bonus;
            
            // Apply offensive buff bonuses
            if (UsingOffensiveBuff)
            {
                damage += GetOffensiveDamageBonus(coreName);
            }
        }
        
        private float GetOffensiveDamageBonus(string coreName)
        {
            bool isNight = !Main.dayTime;
            bool isLowHP = Player.statLife < Player.statLifeMax2 * 0.5f;
            
            switch (coreName)
            {
                case "MoonlightSonata": // Nocturne's Edge - bonus at night
                    return isNight ? 0.10f : 0.03f;
                    
                case "Eroica": // Heroic Fury - bonus when low HP
                    return isLowHP ? 0.15f : 0.05f;
                    
                case "SwanLake": // Swan's Grace - consistent bonus
                    return 0.12f;
                    
                case "LaCampanella": // Bell's Toll - moderate bonus + crit effect
                    return 0.10f;
                    
                case "Enigma": // Enigma's Chaos - random bonus
                    return 0.08f + Main.rand.NextFloat(0.08f);
                    
                case "Fate": // Fate's Wrath - strong bonus
                    return 0.18f;
                    
                default:
                    return 0f;
            }
        }
        
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            if (EquippedCore == null || EquippedCore.IsAir) return;
            if (!UsingOffensiveBuff) return;
            
            string coreName = GetEquippedCoreName();
            
            // Fate - bonus damage to marked enemy
            if (coreName == "Fate" && target.whoAmI == fateMarkedNPC)
            {
                modifiers.SourceDamage += 0.50f; // 50% more damage to marked target
            }
        }
        
        public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone)
        {
            HandleOnHitEffects(target, hit, damageDone);
        }
        
        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            HandleOnHitEffects(target, hit, damageDone);
        }
        
        private void HandleOnHitEffects(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (EquippedCore == null || EquippedCore.IsAir) return;
            
            string coreName = GetEquippedCoreName();
            
            // ========== OFFENSIVE SET BONUS ON-HIT EFFECTS ==========
            if (UsingOffensiveBuff)
            {
                // Moonlight Sonata - Lunar Crescendo phase building
                if (coreName == "MoonlightSonata")
                {
                    moonlightStoredDamage += damageDone;
                    moonlightPhaseTimer = 0; // Reset decay timer
                    
                    // Every 500 damage builds a phase
                    if (moonlightStoredDamage >= 500 && moonlightPhase < 4)
                    {
                        moonlightPhase++;
                        moonlightStoredDamage = 0;
                        
                        SoundEngine.PlaySound(SoundID.Item29 with { Pitch = 0.1f * moonlightPhase }, Player.Center);
                        
                        // Phase up particles
                        for (int i = 0; i < 10 * moonlightPhase; i++)
                        {
                            Dust dust = Dust.NewDustDirect(Player.position, Player.width, Player.height,
                                DustID.PurpleTorch, Main.rand.NextFloat(-3f, 3f), Main.rand.NextFloat(-3f, 3f), 100, default, 1f + moonlightPhase * 0.3f);
                            dust.noGravity = true;
                        }
                    }
                    
                    // At full moon (phase 4), release devastating moonbeam
                    if (moonlightPhase >= 4)
                    {
                        ReleaseLunarCrescendo(target);
                        moonlightPhase = 0;
                        moonlightStoredDamage = 0;
                    }
                    
                    // Normal slow effect
                    if (Main.rand.NextBool(4))
                    {
                        target.AddBuff(BuffID.Slow, 120);
                    }
                }
                
                // Eroica - Heroic Momentum combo building
                if (coreName == "Eroica")
                {
                    eroicaComboCount++;
                    eroicaComboTimer = 90; // 1.5 second decay timer
                    
                    // At 20 hits, unleash shockwave
                    if (eroicaComboCount >= 20)
                    {
                        ReleaseHeroicShockwave();
                        eroicaComboCount = 0;
                    }
                }
                
                // La Campanella - Bell Resonance stacking
                if (coreName == "LaCampanella")
                {
                    campanellaResonance = Math.Min(12, campanellaResonance + 1);
                    campanellaEchoTimer = 180; // 3 second decay timer
                    
                    // At 12 stacks, next crit echoes
                    if (campanellaResonance >= 12 && hit.Crit)
                    {
                        ReleaseResonantEcho(target, damageDone);
                        campanellaResonance = 0;
                    }
                    else if (hit.Crit && bellTollCooldown <= 0)
                    {
                        // Normal bell toll on crit
                        bellTollCooldown = 30;
                        SoundEngine.PlaySound(SoundID.Item35 with { Pitch = 0.5f }, target.Center);
                        
                        int bellDamage = damageDone / 3;
                        target.SimpleStrikeNPC(bellDamage, 0, false, 0f, null, false, 0f, true);
                        
                        for (int i = 0; i < 15; i++)
                        {
                            float angle = MathHelper.TwoPi * i / 15f;
                            Vector2 vel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 4f;
                            Dust dust = Dust.NewDustPerfect(target.Center, DustID.GoldCoin, vel, 100, default, 1.5f);
                            dust.noGravity = true;
                        }
                    }
                }
                
                // Enigma - random effects + life steal on green variation
                if (coreName == "Enigma")
                {
                    // Green variation = life steal
                    if (enigmaCurrentBonus == 4)
                    {
                        int lifeSteal = Math.Max(1, damageDone / 20);
                        Player.statLife = Math.Min(Player.statLife + lifeSteal, Player.statLifeMax2);
                        Player.HealEffect(lifeSteal, true);
                    }
                    
                    // Normal random debuff
                    if (Main.rand.NextBool(5))
                    {
                        int effect = Main.rand.Next(4);
                        switch (effect)
                        {
                            case 0: target.AddBuff(BuffID.OnFire, 180); break;
                            case 1: target.AddBuff(BuffID.Frostburn, 180); break;
                            case 2: target.AddBuff(BuffID.Venom, 120); break;
                            case 3: target.AddBuff(BuffID.Confused, 120); break;
                        }
                    }
                }
                
                // Fate - Mark of Fate + execute
                if (coreName == "Fate")
                {
                    // Mark enemy on first hit if no one is marked
                    if (fateMarkedNPC < 0 && target.boss)
                    {
                        fateMarkedNPC = target.whoAmI;
                        SoundEngine.PlaySound(SoundID.Item119, target.Center);
                        
                        // Mark particles
                        for (int i = 0; i < 30; i++)
                        {
                            Dust dust = Dust.NewDustDirect(target.position, target.width, target.height,
                                DustID.Shadowflame, Main.rand.NextFloat(-6f, 6f), Main.rand.NextFloat(-6f, 6f), 100, default, 2f);
                            dust.noGravity = true;
                        }
                    }
                    
                    // Execute low HP enemies
                    if (target.life < target.lifeMax * 0.1f && Main.rand.NextBool(3))
                    {
                        target.SimpleStrikeNPC(target.life + 100, 0, true, 0f, null, false, 0f, true);
                        
                        for (int i = 0; i < 20; i++)
                        {
                            Dust dust = Dust.NewDustDirect(target.position, target.width, target.height,
                                DustID.PurpleTorch, Main.rand.NextFloat(-5f, 5f), Main.rand.NextFloat(-5f, 5f), 100, default, 2f);
                            dust.noGravity = true;
                        }
                        
                        SoundEngine.PlaySound(SoundID.NPCDeath6 with { Pitch = -0.3f }, target.Center);
                    }
                }
            }
        }
        
        // ========== SPECIAL ATTACK METHODS ==========
        private void ReleaseLunarCrescendo(NPC target)
        {
            SoundEngine.PlaySound(SoundID.Item122 with { Pitch = -0.2f, Volume = 1.2f }, Player.Center);
            SoundEngine.PlaySound(SoundID.Item162 with { Pitch = 0.3f, Volume = 0.8f }, Player.Center);
            
            // MASSIVE initial flash at player - deep purple and light blue explosion
            for (int ring = 0; ring < 4; ring++)
            {
                for (int i = 0; i < 60; i++)
                {
                    float angle = MathHelper.TwoPi * i / 60f;
                    float speed = 8f + ring * 5f;
                    Vector2 vel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * speed;
                    int dustType = (i + ring) % 2 == 0 ? DustID.PurpleTorch : DustID.IceTorch;
                    Dust dust = Dust.NewDustPerfect(Player.Center, dustType, vel, 0, default, 3f - ring * 0.3f);
                    dust.noGravity = true;
                    dust.fadeIn = 2f;
                }
            }
            
            // Shadowflame burst
            for (int i = 0; i < 40; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(15f, 15f);
                Dust shadow = Dust.NewDustPerfect(Player.Center, DustID.Shadowflame, vel, 100, default, 2.5f);
                shadow.noGravity = true;
            }
            
            // Electric burst
            for (int i = 0; i < 25; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(10f, 10f);
                Dust electric = Dust.NewDustPerfect(Player.Center, DustID.Electric, vel, 100, Color.LightBlue, 1.5f);
                electric.noGravity = true;
            }
            
            // Find all nearby enemies and hit them with VISIBLE moonbeams
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && !npc.friendly && npc.Distance(Player.Center) < 800f)
                {
                    int damage = (int)(Player.GetTotalDamage(DamageClass.Generic).ApplyTo(200));
                    npc.SimpleStrikeNPC(damage, 0, true, 0f, null, false, 0f, true);
                    
                    // MASSIVE moon beam visual from player to enemy - highly visible!
                    Vector2 direction = npc.Center - Player.Center;
                    float distance = direction.Length();
                    direction.Normalize();
                    
                    // Create thick, visible beam with multiple dust layers
                    int beamDensity = (int)(distance / 8f); // More particles for visibility
                    for (int j = 0; j < beamDensity; j++)
                    {
                        float progress = j / (float)beamDensity;
                        Vector2 pos = Player.Center + direction * distance * progress;
                        
                        // Core beam - bright purple
                        for (int k = 0; k < 4; k++)
                        {
                            Vector2 offset = Main.rand.NextVector2Circular(12f, 12f);
                            Dust dust = Dust.NewDustPerfect(pos + offset, DustID.PurpleTorch, direction * 3f, 0, default, 2.8f);
                            dust.noGravity = true;
                            dust.fadeIn = 1.5f;
                        }
                        
                        // Outer beam - light blue
                        for (int k = 0; k < 3; k++)
                        {
                            Vector2 offset = Main.rand.NextVector2Circular(18f, 18f);
                            Dust dust = Dust.NewDustPerfect(pos + offset, DustID.IceTorch, direction * 2f, 0, default, 2.5f);
                            dust.noGravity = true;
                            dust.fadeIn = 1.3f;
                        }
                        
                        // Sparkle layer
                        if (Main.rand.NextBool(3))
                        {
                            Dust sparkle = Dust.NewDustPerfect(pos + Main.rand.NextVector2Circular(8f, 8f), 
                                DustID.SparksMech, direction * 2f, 100, Color.White, 1.5f);
                            sparkle.noGravity = true;
                        }
                        
                        // Add lighting along the beam
                        Lighting.AddLight(pos, 0.6f, 0.3f, 0.8f);
                    }
                    
                    // Explosion at enemy position
                    for (int e = 0; e < 30; e++)
                    {
                        float angle = MathHelper.TwoPi * e / 30f;
                        Vector2 vel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * Main.rand.NextFloat(5f, 10f);
                        int dustType = e % 2 == 0 ? DustID.PurpleTorch : DustID.IceTorch;
                        Dust dust = Dust.NewDustPerfect(npc.Center, dustType, vel, 0, default, 2.5f);
                        dust.noGravity = true;
                        dust.fadeIn = 1.5f;
                    }
                    
                    // Shadowflame explosion at target
                    for (int e = 0; e < 15; e++)
                    {
                        Vector2 vel = Main.rand.NextVector2Circular(8f, 8f);
                        Dust shadow = Dust.NewDustPerfect(npc.Center, DustID.Shadowflame, vel, 100, default, 2f);
                        shadow.noGravity = true;
                    }
                }
            }
            
            // Add strong lighting
            Lighting.AddLight(Player.Center, 1f, 0.5f, 1.2f);
        }
        
        private void ReleaseHeroicShockwave()
        {
            SoundEngine.PlaySound(SoundID.Item62 with { Pitch = -0.3f }, Player.Center);
            
            // Damage all nearby enemies
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && !npc.friendly && npc.Distance(Player.Center) < 600f)
                {
                    int damage = (int)(Player.GetTotalDamage(DamageClass.Generic).ApplyTo(300));
                    npc.SimpleStrikeNPC(damage, Player.direction, true, 10f, null, false, 0f, true);
                }
            }
            
            // Shockwave visual
            for (int ring = 0; ring < 3; ring++)
            {
                for (int i = 0; i < 40; i++)
                {
                    float angle = MathHelper.TwoPi * i / 40f;
                    Vector2 vel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * (6f + ring * 4f);
                    Dust dust = Dust.NewDustPerfect(Player.Center, DustID.PinkTorch, vel, 100, default, 2f - ring * 0.3f);
                    dust.noGravity = true;
                }
            }
        }
        
        private void ReleaseResonantEcho(NPC target, int baseDamage)
        {
            SoundEngine.PlaySound(SoundID.Item35 with { Pitch = -0.2f, Volume = 1.5f }, target.Center);
            SoundEngine.PlaySound(SoundID.Item35 with { Pitch = 0f }, target.Center);
            SoundEngine.PlaySound(SoundID.Item35 with { Pitch = 0.3f }, target.Center);
            
            // 3 echoing hits
            for (int echo = 0; echo < 3; echo++)
            {
                int echoDamage = baseDamage / (echo + 1);
                target.SimpleStrikeNPC(echoDamage, 0, true, 0f, null, false, 0f, true);
            }
            
            // Massive golden explosion
            for (int i = 0; i < 60; i++)
            {
                float angle = MathHelper.TwoPi * i / 60f;
                float speed = 4f + Main.rand.NextFloat(4f);
                Vector2 vel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * speed;
                Dust dust = Dust.NewDustPerfect(target.Center, DustID.GoldCoin, vel, 100, default, 2f);
                dust.noGravity = true;
            }
            
            // AOE damage to nearby enemies
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && !npc.friendly && npc != target && npc.Distance(target.Center) < 300f)
                {
                    npc.SimpleStrikeNPC(baseDamage / 2, 0, false, 0f, null, false, 0f, true);
                }
            }
        }
        
        // ========== DEFENSIVE BUFFS ==========
        public override void UpdateEquips()
        {
            if (EquippedCore == null || EquippedCore.IsAir) return;
            
            string coreName = GetEquippedCoreName();
            
            // Apply base buffs
            if (!UsingOffensiveBuff)
            {
                ApplyDefensiveBuffs(coreName);
            }
            else
            {
                ApplyOffensivePassives(coreName);
            }
            
            // Update unique set bonuses
            UpdateUniqueSetBonus(coreName);
            
            // Cooldown timers
            if (bellTollCooldown > 0) bellTollCooldown--;
            if (fateShieldCooldown > 0) fateShieldCooldown--;
        }
        
        private void UpdateUniqueSetBonus(string coreName)
        {
            switch (coreName)
            {
                case "MoonlightSonata":
                    UpdateMoonlightSetBonus();
                    break;
                case "Eroica":
                    UpdateEroicaSetBonus();
                    break;
                case "SwanLake":
                    UpdateSwanLakeSetBonus();
                    break;
                case "LaCampanella":
                    UpdateCampanellaSetBonus();
                    break;
                case "Enigma":
                    UpdateEnigmaSetBonus();
                    break;
                case "Fate":
                    UpdateFateSetBonus();
                    break;
            }
        }
        
        // ========== MOONLIGHT SONATA SET BONUS ==========
        // OFFENSIVE: "Lunar Crescendo" - Build moon phases by dealing damage, 
        // at full moon release a devastating moonbeam that pierces all enemies
        // DEFENSIVE: "Eclipse Shroud" - Periodically become intangible (dodge all attacks)
        private void UpdateMoonlightSetBonus()
        {
            moonlightPhaseTimer++;
            
            if (UsingOffensiveBuff)
            {
                // Lunar Crescendo - phase builds from damage, decays over time
                if (moonlightPhaseTimer > 120 && moonlightPhase > 0)
                {
                    moonlightPhase--;
                    moonlightPhaseTimer = 0;
                }
                
                // HIGHLY VISIBLE visual indicator of current phase
                if (moonlightPhase > 0)
                {
                    // Constant orbiting moons - very pronounced
                    for (int i = 0; i < moonlightPhase; i++)
                    {
                        float angle = MathHelper.TwoPi * i / 4f + Main.GameUpdateCount * 0.03f;
                        float radius = 50f + moonlightPhase * 5f;
                        Vector2 offset = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * radius;
                        
                        // Large purple orbs
                        Dust purpleOrb = Dust.NewDustPerfect(Player.Center + offset, DustID.PurpleTorch, 
                            new Vector2((float)Math.Cos(angle + MathHelper.PiOver2), (float)Math.Sin(angle + MathHelper.PiOver2)) * 0.5f, 
                            0, default, 2.5f);
                        purpleOrb.noGravity = true;
                        purpleOrb.fadeIn = 1.5f;
                        
                        // Light blue trail
                        Dust blueTrail = Dust.NewDustPerfect(Player.Center + offset * 0.9f, DustID.IceTorch, 
                            Vector2.Zero, 0, default, 2f);
                        blueTrail.noGravity = true;
                        blueTrail.fadeIn = 1.2f;
                    }
                    
                    // Inner glow ring
                    if (Main.GameUpdateCount % 3 == 0)
                    {
                        float innerAngle = Main.rand.NextFloat(MathHelper.TwoPi);
                        Vector2 innerOffset = new Vector2((float)Math.Cos(innerAngle), (float)Math.Sin(innerAngle)) * 25f;
                        int dustType = Main.rand.NextBool() ? DustID.PurpleTorch : DustID.IceTorch;
                        Dust inner = Dust.NewDustPerfect(Player.Center + innerOffset, dustType, Vector2.Zero, 0, default, 1.8f);
                        inner.noGravity = true;
                        inner.fadeIn = 1f;
                    }
                    
                    // Sparkles around player based on phase
                    if (Main.GameUpdateCount % (8 - moonlightPhase) == 0)
                    {
                        Vector2 sparklePos = Player.Center + Main.rand.NextVector2Circular(40f, 60f);
                        Dust sparkle = Dust.NewDustPerfect(sparklePos, DustID.SparksMech, new Vector2(0, -1f), 100, Color.White, 1.2f);
                        sparkle.noGravity = true;
                    }
                    
                    // Rising wisps at higher phases
                    if (moonlightPhase >= 3 && Main.GameUpdateCount % 5 == 0)
                    {
                        Vector2 wispPos = Player.Center + new Vector2(Main.rand.NextFloat(-30f, 30f), Player.height / 2f);
                        Dust wisp = Dust.NewDustPerfect(wispPos, DustID.Shadowflame, new Vector2(0, -3f), 100, default, 1.5f);
                        wisp.noGravity = true;
                    }
                    
                    // Lighting aura
                    float lightIntensity = 0.3f + moonlightPhase * 0.15f;
                    Lighting.AddLight(Player.Center, lightIntensity * 0.6f, lightIntensity * 0.3f, lightIntensity);
                }
            }
            else
            {
                // Eclipse Shroud - every 10 seconds, gain 1 second of invincibility
                if (moonlightPhaseTimer >= 600) // 10 seconds
                {
                    moonlightPhaseTimer = 0;
                    Player.AddBuff(BuffID.ShadowDodge, 60); // 1 second shadow dodge
                    
                    SoundEngine.PlaySound(SoundID.Item72 with { Volume = 0.5f }, Player.Center);
                    
                    // Eclipse effect - MUCH more pronounced
                    for (int ring = 0; ring < 3; ring++)
                    {
                        for (int i = 0; i < 25; i++)
                        {
                            float angle = MathHelper.TwoPi * i / 25f;
                            Vector2 vel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * (4f + ring * 3f);
                            Dust dust = Dust.NewDustPerfect(Player.Center, DustID.Shadowflame, vel, 0, default, 2.2f - ring * 0.3f);
                            dust.noGravity = true;
                            dust.fadeIn = 1.5f;
                        }
                    }
                    
                    // Purple/blue burst
                    for (int i = 0; i < 20; i++)
                    {
                        Vector2 vel = Main.rand.NextVector2Circular(6f, 6f);
                        int dustType = Main.rand.NextBool() ? DustID.PurpleTorch : DustID.IceTorch;
                        Dust dust = Dust.NewDustPerfect(Player.Center, dustType, vel, 0, default, 2f);
                        dust.noGravity = true;
                    }
                }
            }
        }
        
        // ========== EROICA SET BONUS ==========
        // OFFENSIVE: "Heroic Momentum" - Each hit builds combo, at 20 hits unleash a shockwave
        // DEFENSIVE: "Rally Cry" - When hit below 30% HP, gain massive buffs for 5 seconds
        private void UpdateEroicaSetBonus()
        {
            if (UsingOffensiveBuff)
            {
                // Combo decay
                if (eroicaComboTimer > 0) eroicaComboTimer--;
                if (eroicaComboTimer <= 0 && eroicaComboCount > 0)
                {
                    eroicaComboCount = Math.Max(0, eroicaComboCount - 1);
                    eroicaComboTimer = 30;
                }
                
                // Visual combo indicator
                if (eroicaComboCount >= 10 && Main.GameUpdateCount % 10 == 0)
                {
                    float intensity = eroicaComboCount / 20f;
                    Dust dust = Dust.NewDustDirect(Player.position, Player.width, Player.height,
                        DustID.PinkTorch, 0f, -2f, 100, default, 1f + intensity);
                    dust.noGravity = true;
                }
            }
            else
            {
                // Rally timer
                if (eroicaRallyTimer > 0)
                {
                    eroicaRallyTimer--;
                    eroicaRallyActive = true;
                    
                    // Rally buffs
                    Player.GetDamage(DamageClass.Generic) += 0.25f;
                    Player.GetAttackSpeed(DamageClass.Generic) += 0.3f;
                    Player.moveSpeed += 0.3f;
                    Player.statDefense += 20;
                    
                    // Rally particles
                    if (Main.GameUpdateCount % 5 == 0)
                    {
                        Dust dust = Dust.NewDustDirect(Player.position, Player.width, Player.height,
                            DustID.PinkTorch, Main.rand.NextFloat(-1f, 1f), -3f, 100, default, 1.8f);
                        dust.noGravity = true;
                    }
                }
                else
                {
                    eroicaRallyActive = false;
                }
            }
        }
        
        // ========== SWAN LAKE SET BONUS ==========
        // OFFENSIVE: "Dying Swan" - Lower HP = higher crit chance and damage (up to +50% at 1 HP)
        // DEFENSIVE: "Swan's Grace" - While moving, build dodge chance (up to 30%), standing still heals
        private void UpdateSwanLakeSetBonus()
        {
            if (UsingOffensiveBuff)
            {
                // Dying Swan - calculated in damage modifiers
                float hpPercent = (float)Player.statLife / Player.statLifeMax2;
                float bonusCrit = (1f - hpPercent) * 30f; // Up to 30% crit at low HP
                
                Player.GetCritChance(DamageClass.Generic) += (int)bonusCrit;
                
                // Visual at low HP
                if (hpPercent < 0.5f && Main.GameUpdateCount % 20 == 0)
                {
                    Dust dust = Dust.NewDustDirect(Player.position, Player.width, Player.height,
                        DustID.Cloud, 0f, -1f, 200, Color.White, 1f + (1f - hpPercent));
                    dust.noGravity = true;
                }
            }
            else
            {
                // Swan's Grace - moving builds dodge, standing heals
                bool isMoving = Math.Abs(Player.velocity.X) > 1f || Math.Abs(Player.velocity.Y) > 1f;
                
                if (isMoving)
                {
                    swanDodgeChance = Math.Min(0.30f, swanDodgeChance + 0.002f);
                    swanGracePeriod = 0;
                    
                    // Grace particles while moving fast
                    if (swanDodgeChance > 0.15f && Main.rand.NextBool(10))
                    {
                        Dust dust = Dust.NewDustPerfect(Player.Center + Main.rand.NextVector2Circular(20f, 20f),
                            DustID.Cloud, -Player.velocity * 0.1f, 150, Color.White, 1.2f);
                        dust.noGravity = true;
                    }
                }
                else
                {
                    swanDodgeChance = Math.Max(0f, swanDodgeChance - 0.005f);
                    swanGracePeriod++;
                    
                    // Heal while standing still
                    if (swanGracePeriod >= 60 && swanGracePeriod % 30 == 0)
                    {
                        int heal = 3;
                        Player.statLife = Math.Min(Player.statLife + heal, Player.statLifeMax2);
                        Player.HealEffect(heal, true);
                    }
                }
            }
        }
        
        // ========== LA CAMPANELLA SET BONUS ==========
        // OFFENSIVE: "Bell Resonance" - Each hit adds resonance. At 12 stacks, next hit echoes 3 times
        // DEFENSIVE: "Sanctuary Bells" - Taking damage rings bells that damage nearby enemies
        private void UpdateCampanellaSetBonus()
        {
            if (UsingOffensiveBuff)
            {
                // Resonance decay
                if (campanellaEchoTimer > 0)
                {
                    campanellaEchoTimer--;
                }
                else if (campanellaResonance > 0 && Main.GameUpdateCount % 60 == 0)
                {
                    campanellaResonance = Math.Max(0, campanellaResonance - 1);
                }
                
                // Visual resonance stacks
                if (campanellaResonance >= 6 && Main.GameUpdateCount % (15 - campanellaResonance) == 0)
                {
                    float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                    Vector2 offset = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 30f;
                    Dust dust = Dust.NewDustPerfect(Player.Center + offset, DustID.GoldCoin, -offset * 0.05f, 100, default, 1f + campanellaResonance * 0.1f);
                    dust.noGravity = true;
                }
                
                // At max stacks, glow effect
                if (campanellaResonance >= 12)
                {
                    Lighting.AddLight(Player.Center, 1f, 0.9f, 0.5f);
                }
            }
        }
        
        // ========== ENIGMA SET BONUS ==========
        // OFFENSIVE: "Variations" - Every 8 seconds, gain a random powerful buff
        // DEFENSIVE: "Mystery Shield" - 20% chance to reflect projectiles back at enemies
        private void UpdateEnigmaSetBonus()
        {
            enigmaBonusTimer++;
            
            if (UsingOffensiveBuff)
            {
                // Variations - change bonus every 8 seconds
                if (enigmaBonusTimer >= 480)
                {
                    enigmaBonusTimer = 0;
                    enigmaCurrentBonus = Main.rand.Next(6);
                    
                    SoundEngine.PlaySound(SoundID.Item4 with { Pitch = Main.rand.NextFloat(-0.3f, 0.3f) }, Player.Center);
                    
                    // Transformation particles
                    for (int i = 0; i < 20; i++)
                    {
                        int dustType = enigmaCurrentBonus switch
                        {
                            0 => DustID.Torch,
                            1 => DustID.IceTorch,
                            2 => DustID.CursedTorch,
                            3 => DustID.PinkTorch,
                            4 => DustID.GreenTorch,
                            _ => DustID.PurpleTorch
                        };
                        Dust dust = Dust.NewDustDirect(Player.position, Player.width, Player.height,
                            dustType, Main.rand.NextFloat(-4f, 4f), Main.rand.NextFloat(-4f, 4f), 100, default, 1.5f);
                        dust.noGravity = true;
                    }
                }
                
                // Apply current variation bonus
                switch (enigmaCurrentBonus)
                {
                    case 0: // Fire - bonus damage
                        Player.GetDamage(DamageClass.Generic) += 0.20f;
                        break;
                    case 1: // Ice - attack speed
                        Player.GetAttackSpeed(DamageClass.Generic) += 0.25f;
                        break;
                    case 2: // Cursed - armor penetration
                        Player.GetArmorPenetration(DamageClass.Generic) += 20;
                        break;
                    case 3: // Pink - crit chance
                        Player.GetCritChance(DamageClass.Generic) += 15;
                        break;
                    case 4: // Green - life steal (handled in OnHit)
                        break;
                    case 5: // Purple - mana regen
                        Player.manaRegen += 50;
                        Player.statManaMax2 += 50;
                        break;
                }
            }
        }
        
        // ========== FATE SET BONUS ==========
        // OFFENSIVE: "Mark of Fate" - Mark an enemy. They take 50% more damage and drop better loot
        // DEFENSIVE: "Destiny's Weave" - Each death avoided increases your power permanently (resets on death)
        private void UpdateFateSetBonus()
        {
            if (UsingOffensiveBuff)
            {
                // Check if marked enemy is still valid
                if (fateMarkedNPC >= 0 && fateMarkedNPC < Main.maxNPCs)
                {
                    NPC marked = Main.npc[fateMarkedNPC];
                    if (!marked.active || marked.life <= 0)
                    {
                        fateMarkedNPC = -1;
                    }
                    else
                    {
                        // Visual mark on enemy
                        if (Main.GameUpdateCount % 10 == 0)
                        {
                            Dust dust = Dust.NewDustPerfect(marked.Top - new Vector2(0, 20), 
                                DustID.PurpleTorch, Vector2.Zero, 100, default, 2f);
                            dust.noGravity = true;
                            
                            // Skull indicator
                            Dust skull = Dust.NewDustPerfect(marked.Top - new Vector2(0, 30),
                                DustID.Shadowflame, new Vector2(0, -0.5f), 100, default, 1.5f);
                            skull.noGravity = true;
                        }
                    }
                }
            }
            else
            {
                // Destiny's Weave - power from avoided deaths
                if (fateDeathsAvoided > 0)
                {
                    float bonus = Math.Min(fateDeathsAvoided * 0.05f, 0.50f); // Up to 50% bonus
                    Player.GetDamage(DamageClass.Generic) += bonus;
                    Player.statDefense += fateDeathsAvoided * 3;
                    
                    // Destiny aura
                    if (Main.GameUpdateCount % 30 == 0)
                    {
                        for (int i = 0; i < fateDeathsAvoided; i++)
                        {
                            float angle = MathHelper.TwoPi * i / Math.Max(fateDeathsAvoided, 1) + Main.GameUpdateCount * 0.01f;
                            Vector2 offset = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 50f;
                            Dust dust = Dust.NewDustPerfect(Player.Center + offset, DustID.PurpleTorch, Vector2.Zero, 150, default, 1f);
                            dust.noGravity = true;
                        }
                    }
                }
            }
        }
        
        private void ApplyOffensivePassives(string coreName)
        {
            // Dying Swan damage bonus
            if (coreName == "SwanLake")
            {
                float hpPercent = (float)Player.statLife / Player.statLifeMax2;
                float bonusDamage = (1f - hpPercent) * 0.50f; // Up to 50% at 1 HP
                Player.GetDamage(DamageClass.Generic) += bonusDamage;
            }
        }
        
        private void ApplyDefensiveBuffs(string coreName)
        {
            bool isNight = !Main.dayTime;
            bool isLowHP = Player.statLife < Player.statLifeMax2 * 0.5f;
            
            switch (coreName)
            {
                case "MoonlightSonata": // Lunar Veil
                    Player.statDefense += isNight ? 12 : 6;
                    Player.endurance += 0.08f;
                    break;
                    
                case "Eroica": // Heroic Resolve
                    Player.statDefense += isLowHP ? 15 : 8;
                    Player.endurance += 0.10f;
                    break;
                    
                case "SwanLake": // Swan's Elegance
                    Player.statDefense += 12;
                    Player.endurance += 0.12f;
                    Player.moveSpeed += 0.15f;
                    break;
                    
                case "LaCampanella": // Bell's Ward
                    Player.statDefense += 15;
                    Player.endurance += 0.15f;
                    break;
                    
                case "Enigma": // Enigma's Mystery
                    Player.statDefense += 18;
                    Player.endurance += 0.18f;
                    break;
                    
                case "Fate": // Fate's Shield
                    Player.statDefense += 22;
                    Player.endurance += 0.20f;
                    break;
            }
        }
        
        public override void PostUpdate()
        {
            if (EquippedCore == null || EquippedCore.IsAir) return;
            
            string coreName = GetEquippedCoreName();
            
            // La Campanella defensive - periodic healing
            if (coreName == "LaCampanella" && !UsingOffensiveBuff && Main.GameUpdateCount % 120 == 0)
            {
                int healAmount = 8;
                Player.statLife = Math.Min(Player.statLife + healAmount, Player.statLifeMax2);
                Player.HealEffect(healAmount, true);
            }
            
            // Unique cosmetic effects for each core
            UpdateCosmeticEffects(coreName);
        }
        
        private void UpdateCosmeticEffects(string coreName)
        {
            // Vibrant effects every frame for equipped core
            switch (coreName)
            {
                case "MoonlightSonata":
                    // Intense purple and blue ethereal flames
                    if (Main.rand.NextBool(2))
                    {
                        Vector2 offset = new Vector2(Main.rand.NextFloat(-20f, 20f), Main.rand.NextFloat(-10f, 28f));
                        Dust dust = Dust.NewDustPerfect(Player.Center + offset, DustID.PurpleTorch, 
                            new Vector2(Main.rand.NextFloat(-0.8f, 0.8f), Main.rand.NextFloat(-3f, -1.5f)), 80, default, 1.8f);
                        dust.noGravity = true;
                        dust.fadeIn = 1.5f;
                    }
                    // Bright blue accents
                    if (Main.rand.NextBool(2))
                    {
                        Vector2 offset = new Vector2(Main.rand.NextFloat(-18f, 18f), Main.rand.NextFloat(-8f, 24f));
                        Dust dust2 = Dust.NewDustPerfect(Player.Center + offset, DustID.IceTorch, 
                            new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), Main.rand.NextFloat(-2.5f, -1f)), 100, default, 1.5f);
                        dust2.noGravity = true;
                        dust2.fadeIn = 1.4f;
                    }
                    // Occasional LIGHT BEAM - purple/blue ray shooting outward
                    if (Main.rand.NextBool(25))
                    {
                        float beamAngle = Main.rand.NextFloat(MathHelper.TwoPi);
                        Vector2 beamDir = new Vector2((float)Math.Cos(beamAngle), (float)Math.Sin(beamAngle));
                        for (int i = 0; i < 20; i++)
                        {
                            float dist = i * 8f;
                            Vector2 beamPos = Player.Center + beamDir * dist;
                            int dustType = Main.rand.NextBool() ? DustID.PurpleTorch : DustID.IceTorch;
                            Dust beam = Dust.NewDustPerfect(beamPos, dustType, beamDir * 4f, 50, default, 2.2f - i * 0.08f);
                            beam.noGravity = true;
                            beam.fadeIn = 1.8f;
                            Lighting.AddLight(beamPos, 0.4f, 0.25f, 0.6f);
                        }
                        // Flash at center
                        for (int j = 0; j < 12; j++)
                        {
                            Dust flash = Dust.NewDustPerfect(Player.Center, DustID.PurpleTorch, Main.rand.NextVector2Circular(3f, 3f), 0, default, 2.5f);
                            flash.noGravity = true;
                        }
                    }
                    // Intense moonlight glow
                    Lighting.AddLight(Player.Center, 0.5f, 0.35f, 0.8f);
                    break;
                    
                case "Eroica":
                    // Heroic pink/magenta sparks and embers rising vibrantly
                    if (Main.rand.NextBool(2))
                    {
                        Vector2 offset = new Vector2(Main.rand.NextFloat(-22f, 22f), Player.height / 2f);
                        Dust dust = Dust.NewDustPerfect(Player.BottomLeft + offset, DustID.PinkTorch, 
                            new Vector2(Main.rand.NextFloat(-1.2f, 1.2f), Main.rand.NextFloat(-4f, -2f)), 60, default, 1.9f);
                        dust.noGravity = true;
                        dust.fadeIn = 1.4f;
                    }
                    // Frequent bright fairy sparkles
                    if (Main.rand.NextBool(3))
                    {
                        Vector2 sparkPos = Player.Center + Main.rand.NextVector2Circular(25f, 35f);
                        Dust spark = Dust.NewDustPerfect(sparkPos, DustID.PinkFairy, Main.rand.NextVector2Circular(2f, 2f), 0, default, 1.2f);
                        spark.noGravity = true;
                    }
                    // Occasional LIGHT BEAM - pink heroic ray
                    if (Main.rand.NextBool(25))
                    {
                        float beamAngle = Main.rand.NextFloat(-MathHelper.PiOver4, MathHelper.PiOver4) - MathHelper.PiOver2; // Upward bias
                        Vector2 beamDir = new Vector2((float)Math.Cos(beamAngle), (float)Math.Sin(beamAngle));
                        for (int i = 0; i < 18; i++)
                        {
                            float dist = i * 9f;
                            Vector2 beamPos = Player.Center + beamDir * dist;
                            Dust beam = Dust.NewDustPerfect(beamPos, DustID.PinkTorch, beamDir * 5f, 40, default, 2.4f - i * 0.1f);
                            beam.noGravity = true;
                            beam.fadeIn = 2f;
                            Lighting.AddLight(beamPos, 0.6f, 0.2f, 0.4f);
                        }
                        // Heroic burst
                        for (int j = 0; j < 15; j++)
                        {
                            Dust burst = Dust.NewDustPerfect(Player.Center, DustID.PinkFairy, Main.rand.NextVector2Circular(5f, 5f), 0, default, 1.5f);
                            burst.noGravity = true;
                        }
                    }
                    Lighting.AddLight(Player.Center, 0.6f, 0.2f, 0.45f);
                    break;
                    
                case "SwanLake":
                    // Graceful white feathers and intense sparkles
                    if (Main.rand.NextBool(2))
                    {
                        Vector2 offset = new Vector2(Main.rand.NextFloat(-28f, 28f), Main.rand.NextFloat(-15f, 15f));
                        Dust feather = Dust.NewDustPerfect(Player.Center + offset, DustID.Cloud, 
                            new Vector2(Main.rand.NextFloat(-1.5f, 1.5f) + Player.velocity.X * 0.15f, Main.rand.NextFloat(-1f, 0.5f)), 
                            180, Color.White, 1.6f);
                        feather.noGravity = true;
                        feather.fadeIn = 1.2f;
                    }
                    // Bright trailing sparkles
                    if (Main.rand.NextBool(2))
                    {
                        Dust trail = Dust.NewDustPerfect(Player.Center + Main.rand.NextVector2Circular(15f, 15f), DustID.WhiteTorch, 
                            Main.rand.NextVector2Circular(1f, 1f), 120, default, 1.4f);
                        trail.noGravity = true;
                    }
                    // Occasional LIGHT BEAM - white graceful ray
                    if (Main.rand.NextBool(25))
                    {
                        // Two beams like swan wings
                        for (int wing = -1; wing <= 1; wing += 2)
                        {
                            float beamAngle = MathHelper.PiOver4 * wing - MathHelper.PiOver2;
                            Vector2 beamDir = new Vector2((float)Math.Cos(beamAngle), (float)Math.Sin(beamAngle));
                            for (int i = 0; i < 15; i++)
                            {
                                float dist = i * 7f;
                                Vector2 beamPos = Player.Center + beamDir * dist;
                                Dust beam = Dust.NewDustPerfect(beamPos, DustID.WhiteTorch, beamDir * 3f, 80, default, 2f - i * 0.1f);
                                beam.noGravity = true;
                                Lighting.AddLight(beamPos, 0.5f, 0.5f, 0.55f);
                            }
                        }
                        // Feather burst
                        for (int j = 0; j < 20; j++)
                        {
                            Dust feathers = Dust.NewDustPerfect(Player.Center, DustID.Cloud, Main.rand.NextVector2Circular(4f, 4f), 150, Color.White, 1.8f);
                            feathers.noGravity = true;
                        }
                    }
                    Lighting.AddLight(Player.Center, 0.55f, 0.55f, 0.65f);
                    break;
                    
                case "LaCampanella":
                    // Golden musical sparkles and bell-like rings - more intense
                    if (Main.rand.NextBool(2))
                    {
                        float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                        float dist = Main.rand.NextFloat(18f, 30f);
                        Vector2 offset = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * dist;
                        Dust gold = Dust.NewDustPerfect(Player.Center + offset, DustID.GoldCoin, 
                            offset * 0.04f, 80, default, 1.5f);
                        gold.noGravity = true;
                        gold.fadeIn = 1.6f;
                    }
                    // Frequent rising note-like particles
                    if (Main.rand.NextBool(3))
                    {
                        Vector2 notePos = Player.Center + new Vector2(Main.rand.NextFloat(-25f, 25f), 15f);
                        Dust note = Dust.NewDustPerfect(notePos, DustID.YellowTorch, 
                            new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), -2.5f), 60, default, 1.6f);
                        note.noGravity = true;
                        note.fadeIn = 1.3f;
                    }
                    // Occasional LIGHT BEAM - golden bell chime ray
                    if (Main.rand.NextBool(25))
                    {
                        // Radial burst like sound waves
                        for (int ray = 0; ray < 6; ray++)
                        {
                            float beamAngle = MathHelper.TwoPi * ray / 6f + Main.rand.NextFloat(0.2f);
                            Vector2 beamDir = new Vector2((float)Math.Cos(beamAngle), (float)Math.Sin(beamAngle));
                            for (int i = 0; i < 12; i++)
                            {
                                float rayDist = i * 10f;
                                Vector2 beamPos = Player.Center + beamDir * rayDist;
                                int goldDust = Main.rand.NextBool() ? DustID.GoldCoin : DustID.YellowTorch;
                                Dust beam = Dust.NewDustPerfect(beamPos, goldDust, beamDir * 6f, 50, default, 1.8f - i * 0.12f);
                                beam.noGravity = true;
                                Lighting.AddLight(beamPos, 0.6f, 0.5f, 0.2f);
                            }
                        }
                        // Golden explosion
                        for (int j = 0; j < 25; j++)
                        {
                            Dust burst = Dust.NewDustPerfect(Player.Center, DustID.GoldCoin, Main.rand.NextVector2Circular(6f, 6f), 0, default, 2f);
                            burst.noGravity = true;
                        }
                    }
                    Lighting.AddLight(Player.Center, 0.7f, 0.6f, 0.25f);
                    break;
                    
                case "Enigma":
                    // Shifting, random colored mysterious particles - very vibrant
                    if (Main.rand.NextBool(1)) // Every frame!
                    {
                        int dustType = Main.rand.Next(6) switch
                        {
                            0 => DustID.PurpleTorch,
                            1 => DustID.BlueTorch,
                            2 => DustID.GreenTorch,
                            3 => DustID.PinkTorch,
                            4 => DustID.YellowTorch,
                            _ => DustID.Torch
                        };
                        
                        float angle = Main.GameUpdateCount * 0.08f + Main.rand.NextFloat(1f);
                        Vector2 offset = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * Main.rand.NextFloat(12f, 28f);
                        Dust mystery = Dust.NewDustPerfect(Player.Center + offset, dustType, 
                            offset * 0.05f + new Vector2(0, -1f), 80, default, 1.6f);
                        mystery.noGravity = true;
                        mystery.fadeIn = 1.4f;
                    }
                    // Intense swirling rainbow
                    if (Main.rand.NextBool(2))
                    {
                        float swirl = Main.GameUpdateCount * 0.15f;
                        Vector2 swirlPos = Player.Center + new Vector2((float)Math.Cos(swirl) * 22f, (float)Math.Sin(swirl) * 22f);
                        Dust swirlDust = Dust.NewDustPerfect(swirlPos, DustID.RainbowMk2, Vector2.Zero, 100, default, 1.2f);
                        swirlDust.noGravity = true;
                    }
                    // Occasional LIGHT BEAM - rainbow mystery beam
                    if (Main.rand.NextBool(25))
                    {
                        float beamAngle = Main.rand.NextFloat(MathHelper.TwoPi);
                        Vector2 beamDir = new Vector2((float)Math.Cos(beamAngle), (float)Math.Sin(beamAngle));
                        for (int i = 0; i < 22; i++)
                        {
                            float dist = i * 8f;
                            Vector2 beamPos = Player.Center + beamDir * dist;
                            // Rainbow cycling color per segment
                            int rainbowDust = (i % 6) switch
                            {
                                0 => DustID.Torch,
                                1 => DustID.YellowTorch,
                                2 => DustID.GreenTorch,
                                3 => DustID.BlueTorch,
                                4 => DustID.PurpleTorch,
                                _ => DustID.PinkTorch
                            };
                            Dust beam = Dust.NewDustPerfect(beamPos, rainbowDust, beamDir * 5f, 30, default, 2.2f - i * 0.08f);
                            beam.noGravity = true;
                            beam.fadeIn = 1.8f;
                            Lighting.AddLight(beamPos, 0.4f, 0.4f, 0.4f);
                        }
                        // Rainbow burst
                        for (int j = 0; j < 30; j++)
                        {
                            Dust burst = Dust.NewDustPerfect(Player.Center, DustID.RainbowMk2, Main.rand.NextVector2Circular(5f, 5f), 0, default, 1.5f);
                            burst.noGravity = true;
                        }
                    }
                    // Shifting intense light color
                    float r = 0.5f + (float)Math.Sin(Main.GameUpdateCount * 0.04f) * 0.25f;
                    float g = 0.5f + (float)Math.Sin(Main.GameUpdateCount * 0.05f + 2f) * 0.25f;
                    float b = 0.5f + (float)Math.Sin(Main.GameUpdateCount * 0.06f + 4f) * 0.25f;
                    Lighting.AddLight(Player.Center, r, g, b);
                    break;
                    
                case "Fate":
                    // Dark pink and crimson destiny wisps - more intense
                    if (Main.rand.NextBool(2))
                    {
                        Vector2 offset = Main.rand.NextVector2Circular(24f, 30f);
                        Dust shadow = Dust.NewDustPerfect(Player.Center + offset, DustID.CrimsonTorch, 
                            -offset * 0.06f, 80, default, 1.7f);
                        shadow.noGravity = true;
                        shadow.fadeIn = 1.4f;
                    }
                    // Vibrant pink accents
                    if (Main.rand.NextBool(2))
                    {
                        Vector2 offset = Main.rand.NextVector2Circular(22f, 26f);
                        Dust pink = Dust.NewDustPerfect(Player.Center + offset, DustID.PinkTorch, 
                            -offset * 0.05f + new Vector2(0, -1f), 100, default, 1.5f);
                        pink.noGravity = true;
                    }
                    // Fate threads - intense red/pink lines
                    if (Main.rand.NextBool(4))
                    {
                        float threadAngle = Main.rand.NextFloat(MathHelper.TwoPi);
                        for (int i = 0; i < 8; i++)
                        {
                            float dist = 12f + i * 10f;
                            Vector2 threadPos = Player.Center + new Vector2((float)Math.Cos(threadAngle), (float)Math.Sin(threadAngle)) * dist;
                            int threadDust = Main.rand.NextBool() ? DustID.CrimsonTorch : DustID.PinkTorch;
                            Dust thread = Dust.NewDustPerfect(threadPos, threadDust, Vector2.Zero, 150, default, 1.0f - i * 0.08f);
                            thread.noGravity = true;
                        }
                    }
                    // Occasional LIGHT BEAM - fate's crimson thread of destiny
                    if (Main.rand.NextBool(25))
                    {
                        float beamAngle = Main.rand.NextFloat(MathHelper.TwoPi);
                        Vector2 beamDir = new Vector2((float)Math.Cos(beamAngle), (float)Math.Sin(beamAngle));
                        for (int i = 0; i < 25; i++)
                        {
                            float dist = i * 7f;
                            Vector2 beamPos = Player.Center + beamDir * dist;
                            int fateDust = (i % 2 == 0) ? DustID.CrimsonTorch : DustID.PinkTorch;
                            Dust beam = Dust.NewDustPerfect(beamPos, fateDust, beamDir * 4f, 60, default, 2f - i * 0.06f);
                            beam.noGravity = true;
                            beam.fadeIn = 1.6f;
                            Lighting.AddLight(beamPos, 0.5f, 0.15f, 0.3f);
                        }
                        // Destiny burst
                        for (int j = 0; j < 20; j++)
                        {
                            int burstDust = Main.rand.NextBool() ? DustID.CrimsonTorch : DustID.PinkTorch;
                            Dust burst = Dust.NewDustPerfect(Player.Center, burstDust, Main.rand.NextVector2Circular(5f, 5f), 0, default, 2.2f);
                            burst.noGravity = true;
                        }
                    }
                    // Dark pink/crimson glow
                    Lighting.AddLight(Player.Center, 0.6f, 0.18f, 0.35f);
                    break;
            }
        }
        
        public override bool FreeDodge(Player.HurtInfo info)
        {
            if (EquippedCore == null || EquippedCore.IsAir) return false;
            
            string coreName = GetEquippedCoreName();
            
            // Swan's Grace dodge (defensive only)
            if (coreName == "SwanLake" && !UsingOffensiveBuff && swanDodgeChance > 0)
            {
                if (Main.rand.NextFloat() < swanDodgeChance)
                {
                    SoundEngine.PlaySound(SoundID.Item24 with { Pitch = 0.5f, Volume = 0.5f }, Player.Center);
                    
                    // Grace dodge particles
                    for (int i = 0; i < 20; i++)
                    {
                        Dust dust = Dust.NewDustDirect(Player.position, Player.width, Player.height,
                            DustID.Cloud, Main.rand.NextFloat(-4f, 4f), Main.rand.NextFloat(-4f, 4f), 150, Color.White, 1.8f);
                        dust.noGravity = true;
                    }
                    
                    return true;
                }
            }
            
            // Fate's Shield - survive lethal damage (defensive only)
            if (coreName == "Fate" && !UsingOffensiveBuff && fateShieldCooldown <= 0)
            {
                if (Player.statLife - info.Damage <= 0)
                {
                    fateShieldCooldown = FateShieldCooldownMax;
                    fateDeathsAvoided++; // Increase Destiny's Weave power
                    Player.statLife = Player.statLifeMax2 / 4;
                    
                    // Big dramatic effect
                    SoundEngine.PlaySound(SoundID.Item119, Player.Center);
                    
                    for (int i = 0; i < 50; i++)
                    {
                        Dust dust = Dust.NewDustDirect(Player.position, Player.width, Player.height,
                            DustID.PurpleTorch, Main.rand.NextFloat(-8f, 8f), Main.rand.NextFloat(-8f, 8f),
                            100, default, 2.5f);
                        dust.noGravity = true;
                    }
                    
                    Main.NewText($"Fate's Shield activates! (Destiny's Weave: {fateDeathsAvoided} stacks)", 200, 100, 255);
                    return true;
                }
            }
            
            return false;
        }
        
        public override void OnHitByNPC(NPC npc, Player.HurtInfo hurtInfo)
        {
            HandleDefensiveOnHit(hurtInfo.Damage);
        }
        
        public override void OnHitByProjectile(Projectile proj, Player.HurtInfo hurtInfo)
        {
            HandleDefensiveOnHit(hurtInfo.Damage);
        }
        
        private void HandleDefensiveOnHit(int damage)
        {
            if (EquippedCore == null || EquippedCore.IsAir) return;
            
            string coreName = GetEquippedCoreName();
            
            if (UsingOffensiveBuff)
            {
                // Eroica offensive buff - speed boost on taking damage
                if (coreName == "Eroica")
                {
                    Player.AddBuff(BuffID.Swiftness, 180);
                    
                    // Heroic particles
                    for (int i = 0; i < 10; i++)
                    {
                        Dust dust = Dust.NewDustDirect(Player.position, Player.width, Player.height,
                            DustID.PinkTorch, 0f, -2f, 100, default, 1.5f);
                        dust.noGravity = true;
                    }
                }
            }
            else
            {
                // ========== DEFENSIVE SET BONUS ON-HIT EFFECTS ==========
                
                // Eroica - Rally Cry when low HP
                if (coreName == "Eroica")
                {
                    float hpPercent = (float)Player.statLife / Player.statLifeMax2;
                    if (hpPercent < 0.30f && eroicaRallyTimer <= 0)
                    {
                        eroicaRallyTimer = 300; // 5 seconds of rally
                        SoundEngine.PlaySound(SoundID.Roar with { Pitch = 0.3f, Volume = 0.5f }, Player.Center);
                        
                        Main.NewText("Rally Cry activated!", 255, 150, 200);
                        
                        // Rally burst
                        for (int i = 0; i < 30; i++)
                        {
                            float angle = MathHelper.TwoPi * i / 30f;
                            Vector2 vel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 5f;
                            Dust dust = Dust.NewDustPerfect(Player.Center, DustID.PinkTorch, vel, 100, default, 2f);
                            dust.noGravity = true;
                        }
                    }
                }
                
                // La Campanella - Sanctuary Bells
                if (coreName == "LaCampanella")
                {
                    SoundEngine.PlaySound(SoundID.Item35 with { Pitch = 0.2f }, Player.Center);
                    
                    // Damage nearby enemies
                    int bellDamage = damage * 2;
                    for (int i = 0; i < Main.maxNPCs; i++)
                    {
                        NPC npc = Main.npc[i];
                        if (npc.active && !npc.friendly && npc.Distance(Player.Center) < 200f)
                        {
                            npc.SimpleStrikeNPC(bellDamage, 0, false, 0f, null, false, 0f, true);
                        }
                    }
                    
                    // Bell ring visual
                    for (int ring = 0; ring < 3; ring++)
                    {
                        for (int i = 0; i < 20; i++)
                        {
                            float angle = MathHelper.TwoPi * i / 20f;
                            Vector2 vel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * (3f + ring * 2f);
                            Dust dust = Dust.NewDustPerfect(Player.Center, DustID.GoldCoin, vel, 100, default, 1.5f - ring * 0.3f);
                            dust.noGravity = true;
                        }
                    }
                }
                
                // Enigma - reflect projectiles (20% chance)
                if (coreName == "Enigma" && Main.rand.NextBool(5))
                {
                    // Find nearby enemy projectiles and reflect them
                    for (int i = 0; i < Main.maxProjectiles; i++)
                    {
                        Projectile proj = Main.projectile[i];
                        if (proj.active && proj.hostile && proj.Distance(Player.Center) < 150f)
                        {
                            proj.velocity *= -1.5f;
                            proj.hostile = false;
                            proj.friendly = true;
                            
                            SoundEngine.PlaySound(SoundID.Item150, proj.Center);
                            
                            for (int j = 0; j < 10; j++)
                            {
                                Dust dust = Dust.NewDustPerfect(proj.Center, DustID.PurpleTorch, 
                                    Main.rand.NextVector2Circular(3f, 3f), 100, default, 1.5f);
                                dust.noGravity = true;
                            }
                            break; // Only reflect one projectile per hit
                        }
                    }
                }
            }
        }
        
        public override void Kill(double damage, int hitDirection, bool pvp, PlayerDeathReason damageSource)
        {
            // Reset Fate's Destiny Weave on death
            string coreName = GetEquippedCoreName();
            if (coreName == "Fate" && !UsingOffensiveBuff)
            {
                if (fateDeathsAvoided > 0)
                {
                    Main.NewText($"Destiny's Weave broken... ({fateDeathsAvoided} stacks lost)", 100, 50, 150);
                    fateDeathsAvoided = 0;
                }
            }
        }
        
        // ========== BUFF DESCRIPTIONS ==========
        public static string GetOffensiveBuffName(string coreName)
        {
            return coreName switch
            {
                "MoonlightSonata" => "Nocturne's Edge",
                "Eroica" => "Heroic Fury",
                "SwanLake" => "Swan's Grace",
                "LaCampanella" => "Bell's Toll",
                "Enigma" => "Enigma's Chaos",
                "Fate" => "Fate's Wrath",
                _ => "Unknown"
            };
        }
        
        public static string GetDefensiveBuffName(string coreName)
        {
            return coreName switch
            {
                "MoonlightSonata" => "Lunar Veil",
                "Eroica" => "Heroic Resolve",
                "SwanLake" => "Swan's Elegance",
                "LaCampanella" => "Bell's Ward",
                "Enigma" => "Enigma's Mystery",
                "Fate" => "Fate's Shield",
                _ => "Unknown"
            };
        }
        
        public static string GetOffensiveBuffDesc(string coreName)
        {
            return coreName switch
            {
                "MoonlightSonata" => "+10% dmg at night\nLunar Crescendo: Build phases\ndeal damage, full moon = beam burst",
                "Eroica" => "+15% dmg below 50% HP\nHeroic Momentum: 20 hits\nunleashes a massive shockwave",
                "SwanLake" => "Dying Swan: Lower HP =\nmore crit & dmg (up to +50%)",
                "LaCampanella" => "+10% dmg, crits echo\nBell Resonance: 12 stacks =\ntriple echo + AOE blast",
                "Enigma" => "Variations: Random buff\nevery 8s (dmg/speed/crit/pen/life steal/mana)",
                "Fate" => "+18% dmg, execute <10% HP\nMark of Fate: Bosses take\n+50% damage when marked",
                _ => "No effect"
            };
        }
        
        public static string GetDefensiveBuffDesc(string coreName)
        {
            return coreName switch
            {
                "MoonlightSonata" => "+12 def at night, 8% DR\nEclipse Shroud: Every 10s\ngain 1s of invincibility",
                "Eroica" => "+15 def low HP, 10% DR\nRally Cry: <30% HP triggers\n5s of massive buffs",
                "SwanLake" => "+12 def, 12% DR\nSwan's Grace: Moving builds\n30% dodge, still = heal",
                "LaCampanella" => "+15 def, 15% DR\nSanctuary Bells: Taking hit\ndamages all nearby enemies",
                "Enigma" => "+18 def, 18% DR\nMystery Shield: 20% chance\nto reflect projectiles",
                "Fate" => "+22 def, 20% DR\nFate's Shield: Cheat death (5m CD)\nEach save = permanent power",
                _ => "No effect"
            };
        }
        
        public static string GetClassBonusText(int tier)
        {
            if (tier <= 0 || tier > 6) return "No class bonuses";
            float bonus = TierDamageBonus[tier] * 100f;
            return $"+{bonus:0}% to all class damage";
        }
    }
    
    // ========== UI STATE ==========
    public class HarmonicCoreUIState : UIState
    {
        private UIPanel mainPanel;
        private UIPanel collapsedPanel;
        private UIPanel coreSlotPanel;
        private UIPanel offensiveButton;
        private UIPanel defensiveButton;
        private UIText coreNameText;
        private UIText buffNameText;
        private UIText classBonusText;
        private UIText buffDescText;
        private UIText setDescText;
        private UIText buffHeaderText;
        private UIText setHeaderText;
        
        private bool isCollapsed = false;
        private float sparkleTimer = 0f;
        private float pulseTimer = 0f;
        
        public override void OnInitialize()
        {
            // Collapsed panel - sleek minimal button (positioned below minimap area)
            collapsedPanel = new UIPanel();
            collapsedPanel.Width.Set(28f, 0f);
            collapsedPanel.Height.Set(28f, 0f);
            collapsedPanel.Left.Set(20f, 0f);
            collapsedPanel.Top.Set(260f, 0f);
            collapsedPanel.BackgroundColor = new Color(25, 15, 40, 220);
            collapsedPanel.BorderColor = new Color(120, 80, 180);
            collapsedPanel.OnLeftClick += (evt, elem) => { 
                isCollapsed = false;
                UpdatePanelVisibility();
            };
            
            var expandIcon = new UIText("♫", 0.9f);
            expandIcon.HAlign = 0.5f;
            expandIcon.VAlign = 0.5f;
            expandIcon.TextColor = new Color(200, 160, 255);
            collapsedPanel.Append(expandIcon);
            
            // Main panel - expanded to show detailed buff info (positioned below minimap area)
            mainPanel = new UIPanel();
            mainPanel.Width.Set(200f, 0f);
            mainPanel.Height.Set(260f, 0f);
            mainPanel.Left.Set(20f, 0f);
            mainPanel.Top.Set(260f, 0f);
            mainPanel.BackgroundColor = new Color(15, 10, 25, 230);
            mainPanel.BorderColor = new Color(100, 70, 160);
            
            // Collapse button - minimal
            var collapseBtn = new UIPanel();
            collapseBtn.Width.Set(14f, 0f);
            collapseBtn.Height.Set(14f, 0f);
            collapseBtn.Left.Set(-18f, 1f);
            collapseBtn.Top.Set(3f, 0f);
            collapseBtn.BackgroundColor = new Color(50, 25, 25, 180);
            collapseBtn.BorderColor = new Color(100, 50, 50);
            collapseBtn.OnLeftClick += (evt, elem) => { 
                isCollapsed = true;
                UpdatePanelVisibility();
            };
            mainPanel.Append(collapseBtn);
            
            var collapseX = new UIText("−", 0.6f);
            collapseX.HAlign = 0.5f;
            collapseX.VAlign = 0.5f;
            collapseX.TextColor = new Color(180, 100, 100);
            collapseBtn.Append(collapseX);
            
            // Core slot - centered, elegant
            coreSlotPanel = new UIPanel();
            coreSlotPanel.Width.Set(40f, 0f);
            coreSlotPanel.Height.Set(40f, 0f);
            coreSlotPanel.HAlign = 0.5f;
            coreSlotPanel.Top.Set(8f, 0f);
            coreSlotPanel.BackgroundColor = new Color(8, 4, 16, 250);
            coreSlotPanel.BorderColor = new Color(100, 70, 160);
            coreSlotPanel.OnLeftClick += OnSlotClick;
            coreSlotPanel.OnRightClick += OnSlotRightClick;
            mainPanel.Append(coreSlotPanel);
            
            // Core name - below slot
            coreNameText = new UIText("Empty", 0.75f);
            coreNameText.HAlign = 0.5f;
            coreNameText.Top.Set(50f, 0f);
            coreNameText.TextColor = new Color(140, 120, 170);
            mainPanel.Append(coreNameText);
            
            // Buff buttons - side by side
            offensiveButton = new UIPanel();
            offensiveButton.Width.Set(88f, 0f);
            offensiveButton.Height.Set(22f, 0f);
            offensiveButton.Left.Set(10f, 0f);
            offensiveButton.Top.Set(66f, 0f);
            offensiveButton.BackgroundColor = new Color(70, 35, 35, 200);
            offensiveButton.BorderColor = new Color(180, 90, 90);
            offensiveButton.OnLeftClick += (evt, elem) => {
                var player = Main.LocalPlayer.GetModPlayer<HarmonicCoreModPlayer>();
                if (!player.UsingOffensiveBuff) player.ToggleBuffType();
                RefreshDisplay();
            };
            mainPanel.Append(offensiveButton);
            
            var offText = new UIText("Offensive", 0.65f);
            offText.HAlign = 0.5f;
            offText.VAlign = 0.5f;
            offText.TextColor = new Color(255, 140, 140);
            offensiveButton.Append(offText);
            
            defensiveButton = new UIPanel();
            defensiveButton.Width.Set(88f, 0f);
            defensiveButton.Height.Set(22f, 0f);
            defensiveButton.Left.Set(102f, 0f);
            defensiveButton.Top.Set(66f, 0f);
            defensiveButton.BackgroundColor = new Color(35, 35, 70, 200);
            defensiveButton.BorderColor = new Color(90, 90, 180);
            defensiveButton.OnLeftClick += (evt, elem) => {
                var player = Main.LocalPlayer.GetModPlayer<HarmonicCoreModPlayer>();
                if (player.UsingOffensiveBuff) player.ToggleBuffType();
                RefreshDisplay();
            };
            mainPanel.Append(defensiveButton);
            
            var defText = new UIText("Defensive", 0.65f);
            defText.HAlign = 0.5f;
            defText.VAlign = 0.5f;
            defText.TextColor = new Color(140, 140, 255);
            defensiveButton.Append(defText);
            
            // ===== ACTIVE BUFF SECTION =====
            buffHeaderText = new UIText("~ Active Buff ~", 0.55f);
            buffHeaderText.HAlign = 0.5f;
            buffHeaderText.Top.Set(92f, 0f);
            buffHeaderText.TextColor = new Color(180, 180, 140);
            mainPanel.Append(buffHeaderText);
            
            // Current buff name
            buffNameText = new UIText("", 0.65f);
            buffNameText.HAlign = 0.5f;
            buffNameText.Top.Set(106f, 0f);
            buffNameText.TextColor = new Color(180, 160, 200);
            mainPanel.Append(buffNameText);
            
            // Buff description text - shows actual stats
            buffDescText = new UIText("", 0.48f);
            buffDescText.HAlign = 0.5f;
            buffDescText.Top.Set(122f, 0f);
            buffDescText.TextColor = new Color(200, 200, 180);
            mainPanel.Append(buffDescText);
            
            // ===== SET BONUS SECTION =====
            setHeaderText = new UIText("~ Set Bonus ~", 0.55f);
            setHeaderText.HAlign = 0.5f;
            setHeaderText.Top.Set(164f, 0f);
            setHeaderText.TextColor = new Color(255, 200, 100);
            mainPanel.Append(setHeaderText);
            
            // Set bonus description
            setDescText = new UIText("", 0.48f);
            setDescText.HAlign = 0.5f;
            setDescText.Top.Set(180f, 0f);
            setDescText.TextColor = new Color(255, 220, 130);
            mainPanel.Append(setDescText);
            
            // Class bonus text at bottom
            classBonusText = new UIText("", 0.55f);
            classBonusText.HAlign = 0.5f;
            classBonusText.Top.Set(238f, 0f);
            classBonusText.TextColor = new Color(120, 180, 120);
            mainPanel.Append(classBonusText);
            
            // Start with main panel visible
            UpdatePanelVisibility();
        }
        
        private void UpdatePanelVisibility()
        {
            // Remove both panels first
            if (mainPanel.Parent != null)
                mainPanel.Remove();
            if (collapsedPanel.Parent != null)
                collapsedPanel.Remove();
            
            // Append only the appropriate one
            if (isCollapsed)
            {
                Append(collapsedPanel);
            }
            else
            {
                Append(mainPanel);
            }
            
            Recalculate();
        }
        
        private void OnSlotClick(UIMouseEvent evt, UIElement element)
        {
            if (Main.gameMenu || Main.LocalPlayer == null) return;
            
            var player = Main.LocalPlayer.GetModPlayer<HarmonicCoreModPlayer>();
            
            if (Main.mouseItem != null && !Main.mouseItem.IsAir && IsHarmonicCore(Main.mouseItem.type))
            {
                // Swap or equip
                if (player.EquippedCore != null && !player.EquippedCore.IsAir)
                {
                    Item old = player.EquippedCore.Clone();
                    player.EquipCore(Main.mouseItem);
                    Main.mouseItem = old;
                }
                else
                {
                    player.EquipCore(Main.mouseItem);
                    Main.mouseItem = new Item();
                }
                RefreshDisplay();
            }
        }
        
        private void OnSlotRightClick(UIMouseEvent evt, UIElement element)
        {
            if (Main.gameMenu || Main.LocalPlayer == null) return;
            
            var player = Main.LocalPlayer.GetModPlayer<HarmonicCoreModPlayer>();
            
            if (player.EquippedCore != null && !player.EquippedCore.IsAir)
            {
                if (Main.mouseItem.IsAir)
                {
                    Main.mouseItem = player.EquippedCore.Clone();
                    player.UnequipCore();
                }
                else
                {
                    Main.LocalPlayer.QuickSpawnItem(Main.LocalPlayer.GetSource_Misc("HarmonicCore"), 
                        player.EquippedCore.Clone(), 1);
                    player.UnequipCore();
                }
                RefreshDisplay();
            }
        }
        
        private bool IsHarmonicCore(int type)
        {
            return type == ModContent.ItemType<Content.MoonlightSonata.HarmonicCores.HarmonicCoreOfMoonlightSonata>() ||
                   type == ModContent.ItemType<Content.Eroica.HarmonicCores.HarmonicCoreOfEroica>() ||
                   type == ModContent.ItemType<Content.SwanLake.HarmonicCores.HarmonicCoreOfSwanLake>() ||
                   type == ModContent.ItemType<Content.LaCampanella.HarmonicCores.HarmonicCoreOfLaCampanella>() ||
                   type == ModContent.ItemType<Content.EnigmaVariations.HarmonicCores.HarmonicCoreOfEnigma>() ||
                   type == ModContent.ItemType<Content.Fate.HarmonicCores.HarmonicCoreOfFate>();
        }
        
        public void RefreshDisplay()
        {
            if (Main.gameMenu || Main.LocalPlayer == null) return;
            
            var player = Main.LocalPlayer.GetModPlayer<HarmonicCoreModPlayer>();
            string coreName = player.GetEquippedCoreName();
            int tier = player.GetCoreTier();
            
            if (!string.IsNullOrEmpty(coreName))
            {
                coreNameText.SetText(GetDisplayName(coreName));
                coreNameText.TextColor = GetCoreColor(coreName);
                
                // Update button highlights with subtle glow effect
                if (player.UsingOffensiveBuff)
                {
                    offensiveButton.BackgroundColor = new Color(100, 50, 50, 230);
                    offensiveButton.BorderColor = new Color(220, 100, 100);
                    defensiveButton.BackgroundColor = new Color(35, 35, 70, 180);
                    defensiveButton.BorderColor = new Color(70, 70, 140);
                    buffHeaderText.SetText("~ Active Buff ~");
                    buffHeaderText.TextColor = new Color(255, 180, 140);
                }
                else
                {
                    offensiveButton.BackgroundColor = new Color(70, 35, 35, 180);
                    offensiveButton.BorderColor = new Color(140, 70, 70);
                    defensiveButton.BackgroundColor = new Color(50, 50, 100, 230);
                    defensiveButton.BorderColor = new Color(100, 100, 220);
                    buffHeaderText.SetText("~ Active Buff ~");
                    buffHeaderText.TextColor = new Color(140, 180, 255);
                }
                
                // Buff name - compact display
                string buffName = player.UsingOffensiveBuff ? 
                    HarmonicCoreModPlayer.GetOffensiveBuffName(coreName) :
                    HarmonicCoreModPlayer.GetDefensiveBuffName(coreName);
                    
                buffNameText.SetText(buffName);
                buffNameText.TextColor = player.UsingOffensiveBuff ? 
                    new Color(255, 180, 180) : new Color(180, 180, 255);
                
                // Detailed buff description
                string buffDesc = player.UsingOffensiveBuff ?
                    GetOffensiveBuffDetails(coreName) :
                    GetDefensiveBuffDetails(coreName);
                buffDescText.SetText(buffDesc);
                buffDescText.TextColor = player.UsingOffensiveBuff ?
                    new Color(255, 200, 180) : new Color(180, 200, 255);
                
                // Set bonus section header
                setHeaderText.SetText("~ Set Bonus ~");
                setHeaderText.TextColor = new Color(255, 200, 100);
                
                // Set bonus description
                string setDesc = player.UsingOffensiveBuff ?
                    GetOffensiveSetBonusDetails(coreName) :
                    GetDefensiveSetBonusDetails(coreName);
                setDescText.SetText(setDesc);
                setDescText.TextColor = new Color(255, 220, 130);
                
                // Class bonus - all classes
                float bonus = HarmonicCoreModPlayer.TierDamageBonus[tier] * 100f;
                classBonusText.SetText($"All Classes: +{bonus:0}% DMG");
            }
            else
            {
                coreNameText.SetText("Empty");
                coreNameText.TextColor = new Color(100, 90, 120);
                offensiveButton.BackgroundColor = new Color(50, 30, 30, 150);
                defensiveButton.BackgroundColor = new Color(30, 30, 50, 150);
                buffHeaderText.SetText("");
                buffNameText.SetText("");
                buffDescText.SetText("Equip a Harmonic Core");
                setHeaderText.SetText("");
                setDescText.SetText("");
                classBonusText.SetText("");
            }
        }
        
        private string GetDisplayName(string coreName)
        {
            return coreName switch
            {
                "MoonlightSonata" => "Moonlight Sonata",
                "Eroica" => "Eroica",
                "SwanLake" => "Swan Lake",
                "LaCampanella" => "La Campanella",
                "Enigma" => "Enigma",
                "Fate" => "Fate",
                _ => coreName
            };
        }
        
        private Color GetCoreColor(string coreName)
        {
            return coreName switch
            {
                "MoonlightSonata" => new Color(150, 120, 255),
                "Eroica" => new Color(255, 150, 200),
                "SwanLake" => new Color(200, 220, 255),
                "LaCampanella" => new Color(255, 220, 100),
                "Enigma" => new Color(150, 255, 200),
                "Fate" => new Color(255, 100, 100),
                _ => Color.White
            };
        }
        
        private string GetOffensiveBuffDetails(string coreName)
        {
            return coreName switch
            {
                "MoonlightSonata" => "+10% DMG at night, +3% day\n25% chance to Slow enemies",
                "Eroica" => "+15% DMG below 50% HP\n+5% DMG otherwise",
                "SwanLake" => "+12% DMG\nLower HP = more crit (up to +30%)\nand more DMG (up to +50%)",
                "LaCampanella" => "+10% DMG\nCrits deal +33% bonus echo",
                "Enigma" => "+8-16% random DMG\nRandom debuffs on hit",
                "Fate" => "+18% DMG\nExecute enemies below 10% HP\n+50% DMG to marked bosses",
                _ => "No effect"
            };
        }
        
        private string GetDefensiveBuffDetails(string coreName)
        {
            return coreName switch
            {
                "MoonlightSonata" => "+12 DEF at night (+6 day)\n+8% damage reduction",
                "Eroica" => "+15 DEF below 50% HP (+8 else)\n+10% damage reduction",
                "SwanLake" => "+12 DEF, +12% DR\n+15% movement speed",
                "LaCampanella" => "+15 DEF, +15% DR\nHeals 8 HP every 2s",
                "Enigma" => "+18 DEF, +18% DR\n20% projectile reflect",
                "Fate" => "+22 DEF, +20% DR\nCheat death (60s CD)",
                _ => "No effect"
            };
        }
        
        private string GetOffensiveSetBonusDetails(string coreName)
        {
            return coreName switch
            {
                "MoonlightSonata" => "SET: Lunar Crescendo\nDeal damage to build moon phases\nFull moon = piercing moonbeam",
                "Eroica" => "SET: Heroic Momentum\n20 consecutive hits =\nmassive AoE shockwave",
                "SwanLake" => "SET: Dying Swan\nLower HP = higher damage\nUp to +50% DMG and +30% crit",
                "LaCampanella" => "SET: Bell Resonance\n12 hit stacks, then crit =\ntriple damage echo + AoE",
                "Enigma" => "SET: Variations (8s cycle)\n+20% DMG / +25% Speed /\n+20 Pen / +15% Crit / Life Steal",
                "Fate" => "SET: Mark of Fate\nFirst boss hit marks them\nMarked take +50% damage",
                _ => ""
            };
        }
        
        private string GetDefensiveSetBonusDetails(string coreName)
        {
            return coreName switch
            {
                "MoonlightSonata" => "SET: Eclipse Shroud\nEvery 10s gain Shadow Dodge\n(1s of invincibility)",
                "Eroica" => "SET: Rally Cry\nBelow 30% HP when hit:\n+25% DMG, +30% Speed, +20 DEF",
                "SwanLake" => "SET: Swan's Grace\nMoving = up to 30% dodge\nStill = heal 3 HP every 0.5s",
                "LaCampanella" => "SET: Sanctuary Bells\nTaking damage hurts\nall nearby enemies (2x dmg)",
                "Enigma" => "SET: Mystery Shield\n20% chance to reflect\nenemy projectiles",
                "Fate" => "SET: Destiny's Weave\nEach death cheated =\n+5% DMG & +3 DEF (permanent)",
                _ => ""
            };
        }
        
        public override void Draw(SpriteBatch spriteBatch)
        {
            if (Main.gameMenu || Main.LocalPlayer == null) return;
            
            // Animate pulse timer
            pulseTimer += 0.03f;
            
            // Call base to draw the active panel
            base.Draw(spriteBatch);
            
            // If main panel is showing, draw the core item with effects
            if (!isCollapsed)
            {
                var player = Main.LocalPlayer.GetModPlayer<HarmonicCoreModPlayer>();
                var dims = coreSlotPanel.GetDimensions();
                Vector2 center = new Vector2(dims.X + dims.Width / 2, dims.Y + dims.Height / 2);
                
                // Draw equipped core in slot
                if (player.EquippedCore != null && !player.EquippedCore.IsAir)
                {
                    Texture2D tex = TextureAssets.Item[player.EquippedCore.type].Value;
                    float baseScale = Math.Min(32f / tex.Width, 32f / tex.Height);
                    float pulse = 1f + (float)Math.Sin(pulseTimer) * 0.03f;
                    float scale = baseScale * pulse;
                    Vector2 origin = new Vector2(tex.Width / 2f, tex.Height / 2f);
                    
                    // Subtle glow behind item
                    Color glowColor = GetCoreColor(player.GetEquippedCoreName()) * 0.3f;
                    glowColor.A = 0;
                    spriteBatch.Draw(tex, center, null, glowColor, 0f, origin, scale * 1.2f, SpriteEffects.None, 0f);
                    
                    // Main item
                    spriteBatch.Draw(tex, center, null, Color.White, 0f, origin, scale, SpriteEffects.None, 0f);
                    
                    // Occasional subtle sparkle
                    if (Main.rand.NextBool(15))
                    {
                        Vector2 sparklePos = center + Main.rand.NextVector2Circular(18f, 18f);
                        Dust sparkle = Dust.NewDustPerfect(sparklePos, DustID.SparksMech, Vector2.Zero, 200, Color.White, 0.5f);
                        sparkle.noGravity = true;
                        sparkle.fadeIn = 0.3f;
                    }
                }
            }
        }
        
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            
            // Prevent clicking through UI - check whichever panel is visible
            if (isCollapsed)
            {
                if (collapsedPanel.ContainsPoint(Main.MouseScreen))
                {
                    Main.LocalPlayer.mouseInterface = true;
                }
            }
            else
            {
                if (mainPanel.ContainsPoint(Main.MouseScreen))
                {
                    Main.LocalPlayer.mouseInterface = true;
                }
            }
        }
    }
    
    // ========== UI SYSTEM ==========
    public class HarmonicCoreUISystem : ModSystem
    {
        internal HarmonicCoreUIState UIState;
        private UserInterface userInterface;
        
        public override void Load()
        {
            if (!Main.dedServ)
            {
                UIState = new HarmonicCoreUIState();
                UIState.Activate();
                userInterface = new UserInterface();
                userInterface.SetState(UIState);
            }
        }
        
        public override void Unload()
        {
            UIState = null;
            userInterface = null;
        }
        
        public override void UpdateUI(GameTime gameTime)
        {
            if (ShouldShowUI())
            {
                userInterface?.Update(gameTime);
            }
        }
        
        private bool ShouldShowUI()
        {
            if (Main.gameMenu || Main.LocalPlayer == null || !Main.LocalPlayer.active) 
                return false;
                
            var player = Main.LocalPlayer.GetModPlayer<HarmonicCoreModPlayer>();
            
            // Only show when inventory is open AND Moon Lord has been killed
            return Main.playerInventory && player.HasKilledMoonLord;
        }
        
        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            int inventoryIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Inventory"));
            if (inventoryIndex != -1)
            {
                layers.Insert(inventoryIndex + 1, new LegacyGameInterfaceLayer(
                    "MagnumOpus: Harmonic Core",
                    delegate
                    {
                        if (ShouldShowUI())
                        {
                            UIState.RefreshDisplay();
                            userInterface.Draw(Main.spriteBatch, new GameTime());
                        }
                        return true;
                    },
                    InterfaceScaleType.UI));
            }
        }
    }
    
    // ========== MOON LORD KILL TRACKER ==========
    public class MoonLordKillTracker : GlobalNPC
    {
        public override void OnKill(NPC npc)
        {
            if (npc.type == NPCID.MoonLordCore)
            {
                // Mark all players as having killed Moon Lord
                for (int i = 0; i < Main.maxPlayers; i++)
                {
                    if (Main.player[i].active)
                    {
                        var modPlayer = Main.player[i].GetModPlayer<HarmonicCoreModPlayer>();
                        if (!modPlayer.HasKilledMoonLord)
                        {
                            modPlayer.HasKilledMoonLord = true;
                            
                            if (i == Main.myPlayer)
                            {
                                Main.NewText("The Harmonic Core slot has been unlocked!", 200, 150, 255);
                                SoundEngine.PlaySound(SoundID.Item4);
                            }
                        }
                    }
                }
            }
        }
    }
}
