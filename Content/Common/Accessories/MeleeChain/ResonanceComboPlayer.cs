using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Prefixes;

namespace MagnumOpus.Content.Common.Accessories.MeleeChain
{
    /// <summary>
    /// Simplified ModPlayer for melee accessories.
    /// No more stacking system - just tracks which accessories are equipped
    /// and applies their simple, static effects.
    /// </summary>
    public class ResonanceComboPlayer : ModPlayer
    {
        // ===== TIER 1-4 (SEASONAL) FLAGS =====
        public bool hasResonantRhythmBand;      // +5% damage, +3% speed
        public bool hasSpringTempoCharm;        // +10% speed, chance for healing petal
        public bool hasResonantCleaversEdge;    // Resonance Sliced synergy: +50% burn DoT, 2% heal vs burning
        public bool hasInfernoTempoSignet;      // +4% speed per burning enemy, extend burn on hit
        public bool hasPermafrostCadenceSeal;   // 10% freeze chance
        public bool hasVivaldisTempoMaster;     // +12% damage, biome debuffs

        // ===== TIER 3-4 ACCESSORY FLAGS (for GlobalNPC effects) =====
        public bool hasSolarCrescendoRing;      // Inflicts Scorched stacks
        public bool hasHarvestRhythmSignet;     // 2% lifesteal

        // ===== TIER 5 (THEME VARIANTS) FLAGS =====
        public bool hasMoonlitSonataBand;       // Crits spawn moonbeams
        public bool hasHeroicCrescendo;         // +15% damage, +10% crit
        public bool hasInfernalFortissimo;      // Kills cause explosions
        public bool hasEnigmasDissonance;       // Delayed burst damage
        public bool hasSwansPerfectMeasure;     // Perfect dodge invuln
        public bool hasFatesCosmicSymphony;     // Day/night scaling damage

        // ===== T7-T10 (POST-FATE) FLAGS =====
        public bool hasNocturnalSymphonyBand;   // +20% night damage, constellation trails
        public bool hasInfernalFortissimoBandT8;// Judgment Burn, +25% boss damage
        public bool hasJubilantCrescendoBand;   // 3% lifesteal, kill healing
        public bool hasEternalResonanceBand;    // Double hit (50% second)

        // ===== FUSION FLAGS =====
        public bool hasStarfallJudgmentGauntlet;      // Nachtmusik + Dies Irae
        public bool hasTriumphantCosmosGauntlet;      // 3-theme fusion
        public bool hasGauntletOfTheEternalSymphony;  // Ultimate: triple hit, +40% damage

        // ===== LEGACY RESONANCE SYNERGY STATE (kept for T7+ compatibility) =====
        public bool resonanceSynergyBonusDamageReady;   // T7+: Ready for +200% hit
        public int resonanceSynergySpeedBoostTimer;     // T7+: 2-second speed boost timer

        // ===== T7-T9 STATE =====
        public int notturnoTimer;               // T7: Notturno buff timer (crits grant +8% dmg, +5% crit)
        public int wrathfireHitCooldown;        // T8: Prevents spamming wrathfire application
        public int meleeKillCounter;            // T9: Tracks kills for Triumphant Crescendo
        public int triumphantCrescendoTimer;    // T9: Triumphant Crescendo buff timer
        public int lifestealCooldown;           // T9: Lifesteal HP cap tracking (resets every second)
        public int lifestealThisSecond;         // T9: HP healed this second via lifesteal

        // ===== COOLDOWNS =====
        public int perfectDodgeCooldown;        // Swan's Perfect Measure cooldown

        // ===== LEGACY COMPATIBILITY STUBS =====
        // These properties exist for backwards compatibility with old UI/tooltip code
        // The stacking system has been simplified, so these return constant values
        public int resonanceStacks => 0;
        public int maxResonance => 100;
        public bool IsBurstOnCooldown => true; // Bursts removed, always "on cooldown"
        public int BurstCooldown => 0;
        public bool IsGraceful => false;
        public float GetResonancePercent() => 0f;
        public Color GetResonanceColor() => GetThemeColor();
        // ===== COLORS =====
        private static readonly Color MoonlightPurple = new Color(138, 43, 226);
        private static readonly Color EroicaGold = new Color(255, 200, 80);
        private static readonly Color CampanellaOrange = new Color(255, 140, 40);
        private static readonly Color EnigmaPurple = new Color(140, 60, 200);
        private static readonly Color SwanWhite = new Color(255, 255, 255);
        private static readonly Color FateCrimson = new Color(200, 80, 120);
        private static readonly Color NachtmusikGold = new Color(255, 215, 0);
        private static readonly Color DiesIraeCrimson = new Color(180, 40, 40);
        private static readonly Color OdeToJoyRose = new Color(255, 200, 220);

        public override void Initialize()
        {
            // Subscribe to the max burn stacks event for Resonance Synergy
            ResonantBurnNPC.OnMaxStacksReached += OnMaxBurnStacksReached;
        }

        public override void Unload()
        {
            ResonantBurnNPC.OnMaxStacksReached -= OnMaxBurnStacksReached;
        }

        /// <summary>
        /// Called when Resonant Burn reaches 5 stacks on an enemy.
        /// Triggers max-stack bonuses for T3-T4 accessories.
        /// </summary>
        private void OnMaxBurnStacksReached(NPC npc, Player triggerPlayer)
        {
            if (triggerPlayer.whoAmI != Player.whoAmI)
                return;

            // T3 ResonantCleaversEdge: Ready +200% hit, consumes stacks
            if (hasResonantCleaversEdge)
            {
                resonanceSynergyBonusDamageReady = true;

                // Visual cue
                for (int i = 0; i < 6; i++)
                {
                    float angle = MathHelper.TwoPi * i / 6f;
                    Vector2 offset = angle.ToRotationVector2() * 20f;
                    CustomParticles.GenericFlare(Player.Center + offset, CampanellaOrange, 0.4f, 15);
                }
            }

            // T4 InfernoTempoSignet: +30% attack speed for 2 seconds
            if (hasInfernoTempoSignet)
            {
                resonanceSynergySpeedBoostTimer = 120; // 2 seconds

                // Visual cue
                for (int i = 0; i < 8; i++)
                {
                    float angle = MathHelper.TwoPi * i / 8f;
                    float hue = (float)i / 8f;
                    Color rainbowColor = Main.hslToRgb(hue, 0.9f, 0.7f);
                    Vector2 velocity = angle.ToRotationVector2() * 4f;
                    CustomParticles.GenericGlow(Player.Center, velocity, rainbowColor * 0.6f, 0.3f, 20, true);
                }

                Terraria.Audio.SoundEngine.PlaySound(SoundID.Item4, Player.Center);
            }
        }

        public override void ResetEffects()
        {
            // Reset all accessory flags each frame
            hasResonantRhythmBand = false;
            hasSpringTempoCharm = false;
            hasResonantCleaversEdge = false;
            hasInfernoTempoSignet = false;
            hasPermafrostCadenceSeal = false;
            hasVivaldisTempoMaster = false;
            hasSolarCrescendoRing = false;
            hasHarvestRhythmSignet = false;
            hasMoonlitSonataBand = false;
            hasHeroicCrescendo = false;
            hasInfernalFortissimo = false;
            hasEnigmasDissonance = false;
            hasSwansPerfectMeasure = false;
            hasFatesCosmicSymphony = false;
            hasNocturnalSymphonyBand = false;
            hasInfernalFortissimoBandT8 = false;
            hasJubilantCrescendoBand = false;
            hasEternalResonanceBand = false;
            hasStarfallJudgmentGauntlet = false;
            hasTriumphantCosmosGauntlet = false;
            hasGauntletOfTheEternalSymphony = false;
        }

        public override void PostUpdateEquips()
        {
            // === CHAIN INHERITANCE ===
            // Higher-tier accessories inherit all lower-tier effects.
            // When a player crafts T2 from T1, T1 is consumed — T2 must include T1's effects.

            // --- Fusion chain inheritance ---
            if (hasGauntletOfTheEternalSymphony) { hasTriumphantCosmosGauntlet = true; hasEternalResonanceBand = true; }
            if (hasTriumphantCosmosGauntlet) { hasStarfallJudgmentGauntlet = true; hasJubilantCrescendoBand = true; }
            if (hasStarfallJudgmentGauntlet) { hasNocturnalSymphonyBand = true; hasInfernalFortissimoBandT8 = true; }

            // --- Post-Fate T7-T10 linear chain inheritance ---
            if (hasEternalResonanceBand) hasJubilantCrescendoBand = true;
            if (hasJubilantCrescendoBand) hasInfernalFortissimoBandT8 = true;
            if (hasInfernalFortissimoBandT8) hasNocturnalSymphonyBand = true;

            // T7 inherits all theme variants + seasonal chain
            if (hasNocturnalSymphonyBand)
            {
                hasFatesCosmicSymphony = true;
                hasSwansPerfectMeasure = true;
                hasEnigmasDissonance = true;
                hasInfernalFortissimo = true;
                hasHeroicCrescendo = true;
                hasMoonlitSonataBand = true;
                hasVivaldisTempoMaster = true;
            }

            // --- Seasonal T1-T6 chain inheritance ---
            if (hasVivaldisTempoMaster) hasPermafrostCadenceSeal = true;
            if (hasPermafrostCadenceSeal) hasInfernoTempoSignet = true;
            if (hasInfernoTempoSignet) hasResonantCleaversEdge = true;
            if (hasResonantCleaversEdge) hasSpringTempoCharm = true;
            if (hasSpringTempoCharm) hasResonantRhythmBand = true;

            // T1-T6 stats are now applied directly in UpdateAccessory on each item.
            // Only T7+ stats are applied here.

            // Resonance Synergy speed boost (from max stacks trigger, T7+ only)
            if (resonanceSynergySpeedBoostTimer > 0)
            {
                resonanceSynergySpeedBoostTimer--;
                Player.GetAttackSpeed(DamageClass.Melee) += 0.30f;
            }

            // Heroic Crescendo: +15% damage, +10% crit
            if (hasHeroicCrescendo)
            {
                Player.GetDamage(DamageClass.Melee) += 0.15f;
                Player.GetCritChance(DamageClass.Melee) += 10;
            }

            // Nocturnal Symphony Band: +20% damage at night, +10% during day
            if (hasNocturnalSymphonyBand)
            {
                if (!Main.dayTime)
                    Player.GetDamage(DamageClass.Melee) += 0.20f;
                else
                    Player.GetDamage(DamageClass.Melee) += 0.10f;
            }

            // Nocturnal Symphony Band: Notturno buff (crits grant +8% dmg, +5% crit for 2s)
            if (hasNocturnalSymphonyBand && notturnoTimer > 0)
            {
                notturnoTimer--;
                Player.GetDamage(DamageClass.Melee) += 0.08f;
                Player.GetCritChance(DamageClass.Melee) += 5;
            }

            // Infernal Fortissimo T8: +25% damage during boss fights, +12% otherwise
            if (hasInfernalFortissimoBandT8)
            {
                if (AnyBossAlive())
                    Player.GetDamage(DamageClass.Melee) += 0.25f;
                else
                    Player.GetDamage(DamageClass.Melee) += 0.12f;
            }

            // Jubilant Crescendo Band: +15% melee damage
            if (hasJubilantCrescendoBand)
            {
                Player.GetDamage(DamageClass.Melee) += 0.15f;
            }

            // Jubilant Crescendo Band: Triumphant Crescendo buff
            if (hasJubilantCrescendoBand && triumphantCrescendoTimer > 0)
            {
                triumphantCrescendoTimer--;
                Player.GetDamage(DamageClass.Melee) += 0.12f;
                Player.GetCritChance(DamageClass.Melee) += 8;
            }

            // Lifesteal cap reset (once per second)
            if (hasJubilantCrescendoBand || hasTriumphantCosmosGauntlet || hasGauntletOfTheEternalSymphony)
            {
                lifestealCooldown++;
                if (lifestealCooldown >= 60)
                {
                    lifestealCooldown = 0;
                    lifestealThisSecond = 0;
                }
            }

            // Cooldown management
            if (perfectDodgeCooldown > 0)
                perfectDodgeCooldown--;
        }

        /// <summary>
        /// Gets the highest burn stack count on any nearby enemy.
        /// </summary>
        private int GetHighestBurnStacks()
        {
            int maxStacks = 0;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && !npc.friendly && Vector2.Distance(Player.Center, npc.Center) < 2000f)
                {
                    int stacks = ResonancePrefixHelper.GetBurnStacks(npc);
                    if (stacks > maxStacks)
                        maxStacks = stacks;
                }
            }
            return maxStacks;
        }

        public override void OnHurt(Player.HurtInfo info)
        {
            // Swan's Perfect Measure: Perfect dodge grants invulnerability
            // "Perfect dodge" = dodge within very short window, handled via accessory effect
            // For simplicity: when hit while wearing this and cooldown is 0, trigger invuln
            if (hasSwansPerfectMeasure && perfectDodgeCooldown <= 0 && info.Dodgeable)
            {
                // Grant 2 seconds of invulnerability
                Player.immune = true;
                Player.immuneTime = 120;
                perfectDodgeCooldown = 1800; // 30 second cooldown

                // VFX
                for (int i = 0; i < 12; i++)
                {
                    float angle = MathHelper.TwoPi * i / 12f;
                    Vector2 offset = angle.ToRotationVector2() * 30f;
                    CustomParticles.GenericFlare(Player.Center + offset, SwanWhite, 0.5f, 25);
                }
            }
        }

        public override void ModifyHitNPCWithItem(Item item, NPC target, ref NPC.HitModifiers modifiers)
        {
            // T7+ bonus damage logic (resonance synergy)
            if (resonanceSynergyBonusDamageReady && (hasHeroicCrescendo || hasInfernalFortissimo || hasEnigmasDissonance ||
                hasSwansPerfectMeasure || hasFatesCosmicSymphony || hasNocturnalSymphonyBand ||
                hasInfernalFortissimoBandT8 || hasJubilantCrescendoBand || hasEternalResonanceBand ||
                hasStarfallJudgmentGauntlet || hasTriumphantCosmosGauntlet || hasGauntletOfTheEternalSymphony))
            {
                modifiers.FinalDamage *= 2f;
            }

            // Lifesteal: T7+ accessories (capped at 20 HP/s)
            float lifestealPercent = GetLifestealPercent();
            if (lifestealPercent > 0f)
            {
                int healAmount = (int)(modifiers.FinalDamage.Multiplicative * lifestealPercent);
                int lifestealCap = 20;
                if (healAmount > 0 && lifestealThisSecond < lifestealCap)
                {
                    healAmount = Math.Min(healAmount, lifestealCap - lifestealThisSecond);
                    if (healAmount > 0)
                    {
                        Player.Heal(healAmount);
                        lifestealThisSecond += healAmount;
                    }
                }
            }
        }

        public override void ModifyHitNPCWithProj(Projectile proj, NPC target, ref NPC.HitModifiers modifiers)
        {
            if (proj.owner != Player.whoAmI || !proj.DamageType.Equals(DamageClass.Melee))
                return;

            // T7+ bonus damage logic
            if (resonanceSynergyBonusDamageReady && (hasHeroicCrescendo || hasInfernalFortissimo || hasEnigmasDissonance ||
                hasSwansPerfectMeasure || hasFatesCosmicSymphony || hasNocturnalSymphonyBand ||
                hasInfernalFortissimoBandT8 || hasJubilantCrescendoBand || hasEternalResonanceBand ||
                hasStarfallJudgmentGauntlet || hasTriumphantCosmosGauntlet || hasGauntletOfTheEternalSymphony))
            {
                modifiers.FinalDamage *= 2f;
            }
        }

        public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone)
        {
            // ===== T1-T6 MELEE CHAIN COMBAT EFFECTS =====
            ApplyMeleeChainOnHit(target, hit, damageDone);

            // ===== T7+ EFFECTS =====

            // Solar Crescendo Ring: Inflict On Fire! for 5 seconds
            if (hasSolarCrescendoRing)
                target.AddBuff(BuffID.OnFire, 300);

            // Moonlit Sonata Band: Crits spawn moonbeams
            if (hasMoonlitSonataBand && hit.Crit)
            {
                for (int i = 0; i < 3; i++)
                    CustomParticles.GenericFlare(target.Center, MoonlightPurple, 0.6f, 20);
            }

            // Infernal Fortissimo: Kills cause explosions
            if (hasInfernalFortissimo && target.life <= 0)
            {
                for (int i = 0; i < 8; i++)
                {
                    float angle = MathHelper.TwoPi * i / 8f;
                    Vector2 velocity = angle.ToRotationVector2() * 4f;
                    Dust dust = Dust.NewDustDirect(target.Center, 1, 1, DustID.Torch);
                    dust.velocity = velocity;
                }
            }

            // T7: Nocturnal Symphony Band - crits grant Notturno
            if (hasNocturnalSymphonyBand && hit.Crit)
            {
                notturnoTimer = 120; // 2 seconds
            }

            // T8: Infernal Fortissimo Band - crits inflict Wrathfire (On Fire! + defense debuff via Ichor)
            if (hasInfernalFortissimoBandT8 && hit.Crit)
            {
                target.AddBuff(BuffID.OnFire3, 180); // Hellfire 3s (4 DPS equivalent)
                target.AddBuff(BuffID.Ichor, 180);    // -15 defense (closest vanilla equivalent to -5 def)
            }

            // T9: Jubilant Crescendo Band - kills toward Triumphant Crescendo
            if (hasJubilantCrescendoBand && target.life <= 0)
            {
                meleeKillCounter++;
                if (meleeKillCounter >= 10)
                {
                    meleeKillCounter = 0;
                    triumphantCrescendoTimer = 300; // 5 seconds
                    // Heal 8% max HP on trigger
                    int healAmount = (int)(Player.statLifeMax2 * 0.08f);
                    if (healAmount > 0) Player.Heal(healAmount);
                }
            }
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (proj.owner != Player.whoAmI)
                return;

            if (!proj.DamageType.Equals(DamageClass.Melee))
                return;

            // ===== T1-T6 MELEE CHAIN COMBAT EFFECTS =====
            ApplyMeleeChainOnHit(target, hit, damageDone);

            // ===== T7+ EFFECTS =====

            // Lifesteal for melee projectiles (T7+)
            float lifestealPercent = GetLifestealPercent();
            if (lifestealPercent > 0f)
            {
                int healAmount = (int)(hit.Damage * lifestealPercent);
                int lifestealCap = 20; // T9 spec: cap 20 HP
                if (healAmount > 0 && lifestealThisSecond < lifestealCap)
                {
                    healAmount = Math.Min(healAmount, lifestealCap - lifestealThisSecond);
                    if (healAmount > 0)
                    {
                        Player.Heal(healAmount);
                        lifestealThisSecond += healAmount;
                    }
                }
            }

            // Solar Crescendo Ring: Inflict On Fire!
            if (hasSolarCrescendoRing)
                target.AddBuff(BuffID.OnFire, 300);

            // Moonlit Sonata Band: Crit sparkles
            if (hasMoonlitSonataBand && hit.Crit)
            {
                for (int i = 0; i < 3; i++)
                    CustomParticles.GenericFlare(target.Center, MoonlightPurple, 0.6f, 20);
            }

            // Infernal Fortissimo: Kills cause explosions
            if (hasInfernalFortissimo && target.life <= 0)
            {
                for (int i = 0; i < 8; i++)
                {
                    float angle = MathHelper.TwoPi * i / 8f;
                    Vector2 velocity = angle.ToRotationVector2() * 4f;
                    Dust dust = Dust.NewDustDirect(target.Center, 1, 1, DustID.Torch);
                    dust.velocity = velocity;
                }
            }

            // T7: Nocturnal Symphony Band - crits grant Notturno
            if (hasNocturnalSymphonyBand && hit.Crit)
            {
                notturnoTimer = 120; // 2 seconds
            }

            // T8: Infernal Fortissimo Band - crits inflict Wrathfire
            if (hasInfernalFortissimoBandT8 && hit.Crit)
            {
                target.AddBuff(BuffID.OnFire3, 180);
                target.AddBuff(BuffID.Ichor, 180);
            }

            // T9: Jubilant Crescendo Band - kills toward Triumphant Crescendo
            if (hasJubilantCrescendoBand && target.life <= 0)
            {
                meleeKillCounter++;
                if (meleeKillCounter >= 10)
                {
                    meleeKillCounter = 0;
                    triumphantCrescendoTimer = 300;
                    int healAmount = (int)(Player.statLifeMax2 * 0.08f);
                    if (healAmount > 0) Player.Heal(healAmount);
                }
            }
        }

        /// <summary>
        /// Shared T1-T6 melee chain on-hit effects: burn, heal-vs-burning, extend burn, freeze, multi-debuff.
        /// </summary>
        private void ApplyMeleeChainOnHit(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // T6: Multi-debuff (OnFire, Frostburn, Poisoned, Bleeding)
            if (hasVivaldisTempoMaster)
            {
                target.AddBuff(BuffID.OnFire, 180);
                target.AddBuff(BuffID.Frostburn, 180);
                target.AddBuff(BuffID.Poisoned, 180);
                target.AddBuff(BuffID.Bleeding, 180);
            }
            // T3-T5: Burn on hit
            else if (hasResonantCleaversEdge)
            {
                target.AddBuff(BuffID.OnFire, 180); // 3 seconds
            }

            // T4+: Extend existing burn by 2 seconds
            if (hasInfernoTempoSignet && target.HasBuff(BuffID.OnFire))
            {
                target.AddBuff(BuffID.OnFire, 120); // extend
            }

            // Heal vs burning enemies (tier-dependent percentage)
            if (hasResonantCleaversEdge && target.HasBuff(BuffID.OnFire))
            {
                float healPercent;
                if (hasVivaldisTempoMaster) healPercent = 0.07f;
                else if (hasPermafrostCadenceSeal) healPercent = 0.05f;
                else if (hasInfernoTempoSignet) healPercent = 0.03f;
                else healPercent = 0.02f;

                int healAmount = Math.Max(1, (int)(damageDone * healPercent));
                Player.Heal(healAmount);
            }

            // T6: 3% freeze chance
            if (hasVivaldisTempoMaster && Main.rand.NextFloat() < 0.03f)
            {
                target.AddBuff(BuffID.Frozen, 60); // 1 second freeze
            }
            // T5: 2% freeze chance
            else if (hasPermafrostCadenceSeal && Main.rand.NextFloat() < 0.02f)
            {
                target.AddBuff(BuffID.Frozen, 60);
            }
        }

        /// <summary>
        /// Checks if any boss is currently alive.
        /// </summary>
        private bool AnyBossAlive()
        {
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && npc.boss)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Gets lifesteal percentage based on equipped accessories.
        /// </summary>
        public float GetLifestealPercent()
        {
            float lifesteal = 0f;

            if (hasHarvestRhythmSignet)
                lifesteal += 0.02f;

            if (hasJubilantCrescendoBand)
                lifesteal += 0.03f;

            if (hasTriumphantCosmosGauntlet)
                lifesteal += 0.03f;

            if (hasGauntletOfTheEternalSymphony)
                lifesteal += 0.02f;

            return lifesteal;
        }

        /// <summary>
        /// Gets the number of times melee attacks should hit.
        /// </summary>
        public int GetHitMultiplier()
        {
            if (hasGauntletOfTheEternalSymphony)
                return 3; // Triple hit

            if (hasEternalResonanceBand || hasTriumphantCosmosGauntlet)
                return 2; // Double hit

            return 1;
        }

        /// <summary>
        /// Gets the color associated with the highest tier accessory equipped.
        /// Used for VFX theming.
        /// </summary>
        public Color GetThemeColor()
        {
            if (hasGauntletOfTheEternalSymphony) return Color.Lerp(OdeToJoyRose, NachtmusikGold, 0.5f);
            if (hasTriumphantCosmosGauntlet) return OdeToJoyRose;
            if (hasStarfallJudgmentGauntlet) return Color.Lerp(NachtmusikGold, DiesIraeCrimson, 0.5f);
            if (hasEternalResonanceBand) return new Color(200, 170, 100);
            if (hasJubilantCrescendoBand) return OdeToJoyRose;
            if (hasInfernalFortissimoBandT8) return DiesIraeCrimson;
            if (hasNocturnalSymphonyBand) return NachtmusikGold;
            if (hasFatesCosmicSymphony) return FateCrimson;
            if (hasSwansPerfectMeasure) return SwanWhite;
            if (hasEnigmasDissonance) return EnigmaPurple;
            if (hasInfernalFortissimo) return CampanellaOrange;
            if (hasHeroicCrescendo) return EroicaGold;
            if (hasMoonlitSonataBand) return MoonlightPurple;
            return new Color(180, 130, 255); // Default purple
        }
    }
}
