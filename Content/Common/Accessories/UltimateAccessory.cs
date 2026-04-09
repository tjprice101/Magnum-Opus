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
using MagnumOpus.Content.Nachtmusik.Accessories;
using MagnumOpus.Content.ClairDeLune.Accessories;
using MagnumOpus.Content.DiesIrae.Accessories;
using MagnumOpus.Content.SwanLake.Debuffs;
using System.Linq;

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
            player.manaRegenBonus += 10;
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
            player.maxMinions += 2;
            
            // === ENIGMA (Chaos power) ===
            player.GetDamage(DamageClass.Generic) += 0.22f;
            
            // === SWAN LAKE (Grace) ===
            player.GetDamage(DamageClass.Generic) += 0.22f;
            player.moveSpeed += 0.30f;
            player.maxRunSpeed *= 1.3f;
            
            // === RANGED (Constellation Compass) ===
            player.GetDamage(DamageClass.Ranged) += 0.30f;
            player.GetCritChance(DamageClass.Ranged) += 18;
            player.ammoCost75 = true; // 25% chance to not consume ammo
            
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
            
            tooltips.Add(new TooltipLine(Mod, "Ultimate1", "The grand finale of all symphonies combined into one")
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
            
            tooltips.Add(new TooltipLine(Mod, "ThemeProcs", "8% Tolling Death echo, 10% Paradox, Moonstruck on magic, Dying Swan's Grace airborne, Heroic Surge on kill")
            {
                OverrideColor = cosmicPurple
            });
            
            tooltips.Add(new TooltipLine(Mod, "ThemeProcs2", "5% Confutatis on crits, auto-execute <10% HP enemies, Eine Kleine every 10s at night, Ovation on kill (3x)")
            {
                OverrideColor = cosmicPurple
            });
            
            tooltips.Add(new TooltipLine(Mod, "Sustain", "3% lifesteal (cap 30 HP), minion hits heal 1 HP (cap 5/s), Prélude +5% dodge (+3 regen at night)")
            {
                OverrideColor = ultimateGold
            });
            
            tooltips.Add(new TooltipLine(Mod, "Harmony", "Harmonic Convergence: 5 theme procs trigger Coda of Unity (+35% damage, +20% dodge, doubled procs, 10s)")
            {
                OverrideColor = ultimateGold
            });
            
            tooltips.Add(new TooltipLine(Mod, "Immunities", "Immunity to all elemental debuffs, magma stone, frost burn, 200% thorns, +80% damage buff effectiveness")
            {
                OverrideColor = ultimateGold
            });
            
            tooltips.Add(new TooltipLine(Mod, "Lore", "'When all movements converge into one, the symphony plays its final note - this is the Coda of Absolute Harmony'")
            {
                OverrideColor = softGold
            });
        }
    }

    public class CodaOfAbsoluteHarmonyPlayer : ModPlayer
    {
        public bool codaEquipped;
        
        // Heroic Surge (Eroica kill trigger)
        private int heroicSurgeTimer;
        
        // Melee temporal echo (Eroica)
        private int meleeStrikeCount;
        
        // Paradox (Enigma)
        private Dictionary<int, int> paradoxStacks = new Dictionary<int, int>();
        private Dictionary<int, int> paradoxTimers = new Dictionary<int, int>();
        
        // Eine Kleine (Nachtmusik)
        private int eineKleineTimer;
        
        // Ovation (Ode to Joy)
        private int ovationStacks;
        private int ovationTimer;
        
        // Minion heal rate cap (Summoner: 1 HP per hit, cap 5/s)
        private int minionHealCount;
        private int minionHealResetTimer;
        
        // Harmonic Convergence → Coda Resonance
        private int harmonicResonanceStacks;
        private int resonanceDecayTimer;
        private int codaOfUnityTimer;
        
        // Dissonance tracking: NPC index → set of theme tags
        private Dictionary<int, HashSet<string>> enemyThemeDebuffs = new Dictionary<int, HashSet<string>>();
        private Dictionary<int, int> dissonanceTimers = new Dictionary<int, int>();
        
        // Dying Swan's Grace cooldowns per NPC
        private Dictionary<int, int> odilesBeautyCooldowns = new Dictionary<int, int>();
        
        // Cooldowns
        private int dodgeCooldown;
        private int cosmicBurstCooldown;
        
        // Swan Lake: +80% damage buff effectiveness
        private const float BuffEffectiveness = 1.80f;
        
        // Coda Resonance constants
        private const int CodaOfUnityBaseDuration = 600; // 10 seconds
        private const int CodaOfUnityMaxDuration = 900; // 15 seconds
        private const int DissonanceDuration = 480; // 8 seconds
        
        private static readonly int[] ParadoxDebuffs = new int[]
        {
            BuffID.Confused, BuffID.Slow, BuffID.CursedInferno,
            BuffID.Ichor, BuffID.ShadowFlame, BuffID.Frostburn
        };

        public override void ResetEffects()
        {
            codaEquipped = false;
        }

        public override void PostUpdate()
        {
            if (!codaEquipped)
            {
                heroicSurgeTimer = 0;
                meleeStrikeCount = 0;
                eineKleineTimer = 0;
                ovationStacks = 0;
                ovationTimer = 0;
                minionHealCount = 0;
                minionHealResetTimer = 0;
                harmonicResonanceStacks = 0;
                resonanceDecayTimer = 0;
                codaOfUnityTimer = 0;
                paradoxStacks.Clear();
                paradoxTimers.Clear();
                enemyThemeDebuffs.Clear();
                dissonanceTimers.Clear();
                odilesBeautyCooldowns.Clear();
                return;
            }
            
            bool isNight = !Main.dayTime;
            float nightPotency = isNight ? 1.10f : 1.0f; // Nachtmusik: +10% buff potency at night
            
            // === HEROIC SURGE: +25% damage (amplified by Swan Lake +80% & Nachtmusik night potency) ===
            if (heroicSurgeTimer > 0)
            {
                heroicSurgeTimer--;
                Player.GetDamage(DamageClass.Generic) += 0.25f * BuffEffectiveness * nightPotency;
            }
            
            // === OVATION: +10% per stack, max 3 stacks (amplified) ===
            if (ovationTimer > 0)
            {
                ovationTimer--;
                Player.GetDamage(DamageClass.Generic) += 0.10f * ovationStacks * BuffEffectiveness * nightPotency;
            }
            else
            {
                ovationStacks = 0;
            }
            
            // === CODA OF UNITY ===
            if (codaOfUnityTimer > 0)
            {
                codaOfUnityTimer--;
                Player.GetDamage(DamageClass.Generic) += 0.35f * BuffEffectiveness * nightPotency;
                if (isNight) Player.GetDamage(DamageClass.Generic) += 0.10f * BuffEffectiveness;
            }
            
            // === CLAIR DE LUNE: PRÉLUDE night life regen ===
            if (isNight) Player.lifeRegen += 3;
            
            // === NACHTMUSIK: EINE KLEINE every 10s at night ===
            if (isNight)
            {
                eineKleineTimer++;
                if (eineKleineTimer >= 600) // 10 seconds
                {
                    eineKleineTimer = 0;
                    Player.AddBuff(ModContent.BuffType<EineKleineBuff>(), 360); // 6 seconds
                }
            }
            else
            {
                eineKleineTimer = 0;
            }
            
            // === MINION HEAL COUNTER RESET (every 1 second) ===
            minionHealResetTimer++;
            if (minionHealResetTimer >= 60)
            {
                minionHealResetTimer = 0;
                minionHealCount = 0;
            }
            
            // === HARMONIC RESONANCE STACK DECAY ===
            if (harmonicResonanceStacks > 0)
            {
                resonanceDecayTimer++;
                int decayInterval = isNight ? 360 : 240; // 6s night, 4s day
                if (resonanceDecayTimer >= decayInterval)
                {
                    harmonicResonanceStacks--;
                    resonanceDecayTimer = 0;
                }
            }
            
            // === COOLDOWN TIMERS ===
            if (dodgeCooldown > 0) dodgeCooldown--;
            if (cosmicBurstCooldown > 0) cosmicBurstCooldown--;
            
            // === PARADOX TIMER DECAY ===
            foreach (int key in paradoxTimers.Keys.ToList())
            {
                paradoxTimers[key]--;
                if (paradoxTimers[key] <= 0)
                {
                    paradoxTimers.Remove(key);
                    paradoxStacks.Remove(key);
                }
            }
            
            // === DISSONANCE TIMER DECAY ===
            foreach (int key in dissonanceTimers.Keys.ToList())
            {
                dissonanceTimers[key]--;
                if (dissonanceTimers[key] <= 0)
                    dissonanceTimers.Remove(key);
            }
            
            // === ODILE'S BEAUTY COOLDOWN DECAY ===
            foreach (int key in odilesBeautyCooldowns.Keys.ToList())
            {
                odilesBeautyCooldowns[key]--;
                if (odilesBeautyCooldowns[key] <= 0)
                    odilesBeautyCooldowns.Remove(key);
            }
            
            // === CLEAN STALE NPC ENTRIES ===
            foreach (int key in enemyThemeDebuffs.Keys.ToList())
            {
                if (key < 0 || key >= Main.maxNPCs || !Main.npc[key].active)
                    enemyThemeDebuffs.Remove(key);
            }
            
            // === DIES IRAE: AUTO-EXECUTE non-boss enemies below 10% HP ===
            if (Main.myPlayer == Player.whoAmI)
            {
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (npc.active && !npc.friendly && !npc.boss && !npc.immortal && !npc.dontTakeDamage
                        && npc.life > 0 && npc.life < npc.lifeMax * 0.10f
                        && Vector2.Distance(npc.Center, Player.Center) <= 600f)
                    {
                        npc.SimpleStrikeNPC(npc.life + npc.defense + 10, 0, false, 0, null, false, 0, true);
                    }
                }
            }
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            if (!codaEquipped) return;
            
            // Dissonance: +20% damage on enemies with 3+ theme debuffs
            if (dissonanceTimers.ContainsKey(target.whoAmI))
                modifiers.FinalDamage *= 1.20f;
        }

        public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!codaEquipped) return;
            HandleCodaHit(target, damageDone, item.DamageType, hit.Crit, false);
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!codaEquipped || proj.owner != Player.whoAmI) return;
            HandleCodaHit(target, damageDone, proj.DamageType, hit.Crit, proj.minion);
        }

        private void HandleCodaHit(NPC target, int damageDone, DamageClass damageType, bool isCrit, bool isMinion)
        {
            bool isMelee = damageType.CountsAsClass(DamageClass.Melee);
            bool isMagic = damageType.CountsAsClass(DamageClass.Magic);
            bool codaActive = codaOfUnityTimer > 0;
            
            // === MOONLIGHT SONATA: MOONSTRUCK on magic attacks (slowed, -15 defense via Ichor) ===
            if (isMagic)
            {
                target.AddBuff(BuffID.Slow, 180); // 3s slow
                target.AddBuff(BuffID.Ichor, 120); // 2s defense reduction
                TrackThemeDebuff(target.whoAmI, "Moonstruck");
                AddResonanceStack();
            }
            
            // === EROICA: Temporal Echo every 5th melee hit ===
            if (isMelee)
            {
                meleeStrikeCount++;
                if (meleeStrikeCount >= 5 && Main.myPlayer == Player.whoAmI)
                {
                    meleeStrikeCount = 0;
                    int echoDamage = (int)(damageDone * 1.0f);
                    target.SimpleStrikeNPC(echoDamage, 0, false, 0, null, false, 0, true);
                }
            }
            
            // === LA CAMPANELLA: 8% TOLLING DEATH (16% during Coda of Unity) ===
            {
                float tollingChance = codaActive ? 0.16f : 0.08f;
                if (Main.rand.NextFloat() < tollingChance && Main.myPlayer == Player.whoAmI)
                {
                    int echoDamage = (int)(damageDone * 0.75f);
                    if (echoDamage > 0)
                        target.SimpleStrikeNPC(echoDamage, 0, false, 0, null, false, 0, true);
                    target.AddBuff(BuffID.WitheredWeapon, 180); // 3s Withered Weapon
                    TrackThemeDebuff(target.whoAmI, "TollingDeath");
                    AddResonanceStack();
                    Terraria.Audio.SoundEngine.PlaySound(SoundID.Item35 with { Pitch = 0.5f, Volume = 0.6f }, target.Center);
                }
            }
            
            // === ENIGMA: 10% PARADOX (20% during Coda of Unity) ===
            {
                float paradoxChance = codaActive ? 0.20f : 0.10f;
                if (Main.rand.NextFloat() < paradoxChance)
                {
                    int debuffId = ParadoxDebuffs[Main.rand.Next(ParadoxDebuffs.Length)];
                    target.AddBuff(debuffId, 300);
                    TrackThemeDebuff(target.whoAmI, "Paradox");
                    AddResonanceStack();
                    
                    if (!paradoxStacks.ContainsKey(target.whoAmI))
                        paradoxStacks[target.whoAmI] = 0;
                    paradoxStacks[target.whoAmI]++;
                    paradoxTimers[target.whoAmI] = 540;
                    
                    // ABSOLUTE HARMONY COLLAPSE at 5 stacks
                    if (paradoxStacks[target.whoAmI] >= 5)
                    {
                        TriggerAbsoluteHarmonyCollapse(target, damageDone, !Main.dayTime);
                        paradoxStacks[target.whoAmI] = 0;
                    }
                }
            }
            
            // === SWAN LAKE: DYING SWAN'S GRACE (airborne → Odile's Beauty) ===
            if (Player.velocity.Y != 0 || Player.wingTime > 0)
            {
                if (!odilesBeautyCooldowns.ContainsKey(target.whoAmI))
                {
                    target.AddBuff(ModContent.BuffType<OdilesBeauty>(), 300); // 5 seconds
                    target.GetGlobalNPC<OdilesBeautyNPC>().SetDamage(damageDone);
                    odilesBeautyCooldowns[target.whoAmI] = 300;
                    TrackThemeDebuff(target.whoAmI, "OdilesBeauty");
                    AddResonanceStack();
                }
            }
            
            // === DIES IRAE: 5% CONFUTATIS on crits (10% during Coda of Unity) ===
            if (isCrit)
            {
                float confutatisChance = codaActive ? 0.10f : 0.05f;
                if (Main.rand.NextFloat() < confutatisChance)
                {
                    target.GetGlobalNPC<DiesIraeAccessoryGlobalNPC>().confutatisTimer = 180; // 3 seconds
                }
            }
            
            // === SEASONS: Elemental debuffs ===
            target.AddBuff(BuffID.OnFire, 300);
            target.AddBuff(BuffID.Frostburn, 240);
            target.AddBuff(BuffID.Poisoned, 300);
            
            // === ODE TO JOY: 3% lifesteal (cap 30 HP) ===
            {
                int healAmount = Math.Max(1, Math.Min((int)(damageDone * 0.03f), 30));
                Player.Heal(healAmount);
            }
            
            // === SUMMONER: Minion heal 1 HP (cap 5/s) ===
            if (isMinion && minionHealCount < 5)
            {
                Player.Heal(1);
                minionHealCount++;
            }
            
            // === CODA OF UNITY: All attacks heal 1% max HP ===
            if (codaActive)
            {
                int codaHeal = Math.Max(1, Player.statLifeMax2 / 100);
                Player.Heal(codaHeal);
            }
            
            // === COSMIC MANA BURST ===
            if (Player.statMana < Player.statManaMax2 * 0.3f && cosmicBurstCooldown <= 0)
            {
                cosmicBurstCooldown = 240;
                Player.statMana = Math.Min(Player.statMana + 150, Player.statManaMax2);
            }
            
            // === CHECK DISSONANCE ===
            CheckDissonance(target.whoAmI);
            
            // === CHECK KILL → HEROIC SURGE + OVATION + CODA EXTENSION ===
            if (target.life <= 0 && !target.immortal)
            {
                // Eroica: Heroic Surge
                heroicSurgeTimer = 300; // 5 seconds
                AddResonanceStack();
                
                // Ode to Joy: Ovation
                ovationStacks = Math.Min(ovationStacks + 1, 3);
                ovationTimer = 300; // 5 seconds
                
                // Coda of Unity extension: +1s per kill, max 15s
                if (codaActive)
                {
                    codaOfUnityTimer = Math.Min(codaOfUnityTimer + 60, CodaOfUnityMaxDuration);
                }
                
                // Kill invulnerability
                Player.immune = true;
                Player.immuneTime = Math.Max(Player.immuneTime, 120);
            }
        }

        private void AddResonanceStack()
        {
            resonanceDecayTimer = 0;
            harmonicResonanceStacks++;
            if (harmonicResonanceStacks >= 5)
            {
                // Trigger Coda of Unity (upgraded Full Harmony)
                codaOfUnityTimer = CodaOfUnityBaseDuration;
                harmonicResonanceStacks = 0;
                Terraria.Audio.SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.3f, Volume = 0.8f }, Player.Center);
            }
        }

        private void TrackThemeDebuff(int npcIndex, string themeTag)
        {
            if (!enemyThemeDebuffs.ContainsKey(npcIndex))
                enemyThemeDebuffs[npcIndex] = new HashSet<string>();
            enemyThemeDebuffs[npcIndex].Add(themeTag);
        }

        private void CheckDissonance(int npcIndex)
        {
            if (!enemyThemeDebuffs.ContainsKey(npcIndex)) return;
            if (enemyThemeDebuffs[npcIndex].Count >= 3)
            {
                bool isNew = !dissonanceTimers.ContainsKey(npcIndex);
                dissonanceTimers[npcIndex] = DissonanceDuration;
                if (isNew && npcIndex >= 0 && npcIndex < Main.maxNPCs && Main.npc[npcIndex].active)
                    Terraria.Audio.SoundEngine.PlaySound(SoundID.Item122 with { Pitch = -0.3f, Volume = 0.7f }, Main.npc[npcIndex].Center);
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
                            
                            // Apply debuffs
                            foreach (int debuff in ParadoxDebuffs)
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
            
            // Clair de Lune Prélude: 5% base dodge + Coda of Unity: +20% dodge
            float dodgeChance = 0.05f;
            if (codaOfUnityTimer > 0) dodgeChance += 0.20f;
            
            if (Main.rand.NextFloat() < dodgeChance)
            {
                dodgeCooldown = 45;
                Player.immune = true;
                Player.immuneTime = 40;
                return true;
            }
            
            return false;
        }

        public override void OnHurt(Player.HurtInfo info)
        {
            if (!codaEquipped) return;
            
            // Clair de Lune: 8% chance to apply Voiles to the nearest hostile NPC (15% miss chance for 2s)
            if (Main.rand.NextFloat() < 0.08f)
            {
                float closestDist = 200f;
                NPC closestNPC = null;
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (npc.active && !npc.friendly && !npc.immortal)
                    {
                        float dist = Vector2.Distance(npc.Center, Player.Center);
                        if (dist < closestDist)
                        {
                            closestDist = dist;
                            closestNPC = npc;
                        }
                    }
                }
                
                if (closestNPC != null)
                {
                    closestNPC.GetGlobalNPC<ClairDeLuneAccessoryGlobalNPC>().ApplyVoiles(120); // 2 seconds
                }
            }
        }
    }
}
