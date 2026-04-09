using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Prefixes;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Content.Common.Accessories.RangerChain
{
    /// <summary>
    /// Simplified ModPlayer for ranger accessories.
    /// No more Mark duration system - just tracks which accessories are equipped
    /// and applies their simple, static effects.
    /// </summary>
    public class MarkingPlayer : ModPlayer
    {
        public static readonly Color SpringGreen = new Color(144, 238, 144);
        public static readonly Color EroicaGold = new Color(255, 200, 80);
        public static readonly Color SummerOrange = new Color(255, 140, 0);

        // ===== TIER 1-6 (SEASONAL + VIVALDI) FLAGS =====
        public bool hasResonantSpotter;         // Ranged attacks mark enemies (visual only)
        public bool hasSpringHuntersLens;       // 10% heart drop chance on ranged hit
        public bool hasResonantPiercingLens;    // T3: +30% damage vs burning, doubled armor pen vs burning
        public bool hasEchoingBoltChamber;      // T4: Homing bolt proc, crit spreads burn
        public bool hasSolarTrackersBadge;      // +5% ranged damage
        public bool hasHarvestReapersMark;      // Ranged kills cause explosions
        public bool hasPermafrostHuntersEye;    // Ranged attacks slow enemies
        public bool hasVivaldisSeasonalSight;    // +10% ranged damage, biome debuffs

        // ===== TIER 5 (THEME VARIANTS) FLAGS =====
        public bool hasMoonlitPredatorsGaze;    // See marked enemies through walls
        public bool hasHeroicDeadeye;           // +12% ranged damage, +8% crit
        public bool hasInfernalExecutionersBrand; // Ranged attacks inflict burn
        public bool hasEnigmasParadoxMark;      // 15% chance for bonus projectile
        public bool hasSwansGracefulHunt;       // Perfect dodge grants damage buff
        public bool hasFatesCosmicVerdict;      // +15% ranged damage

        // ===== T7-T10 (POST-FATE) FLAGS =====
        public bool hasNocturnalPredatorsSight; // +20% ranged damage at night
        public bool hasInfernalExecutionersSight; // +25% ranged damage during bosses
        public bool hasJubilantHuntersSight;    // Ranged kills restore 4 HP
        public bool hasEternalVerdictSight;     // Ranged attacks hit twice

        // ===== FUSION FLAGS =====
        public bool hasStarfallExecutionersScope;  // Nachtmusik + Dies Irae fusion
        public bool hasTriumphantVerdictScope;     // 3-theme fusion
        public bool hasScopeOfTheEternalVerdict;   // Ultimate: triple hit, +40% damage

        // ===== RESONANCE SYNERGY STATE =====
        public bool resonanceSuperCritReady;       // T3: Next crit deals 3x instead of 2x
        public bool resonanceSpreadBurnOnCrit;     // T4: Next crit spreads burn to ALL enemies

        // ===== COOLDOWNS & STATE =====
        public int gracefulDodgeCooldown;  // Swan's Perfect Dodge cooldown
        public int graceBuffTimer;         // Swan's Grace buff timer
        public int rangedShotCounter;      // Tracks shots for every-3rd-shot mechanic

        // ===== T7-T9 STATE =====
        public int executionerFocusTimer;       // T8: Executioner's Focus buff timer
        public int hunterJubilationTimer;       // T9: Hunter's Jubilation buff timer

        // ===== LEGACY COMPATIBILITY STUBS =====
        // These properties exist for backwards compatibility with old code
        public int baseMarkDuration => 300; // 5 seconds baseline
        public int maxMarkedEnemies => 3;
        public float markedDamageBonus => 0f;
        public bool markedSlowsEnemies => hasPermafrostHuntersEye;
        public float markSlowPercent => hasPermafrostHuntersEye ? 0.15f : 0f;
        public bool IsPerfectShot => false; // Simplified

        public override void Initialize()
        {
            ResonantBurnNPC.OnMaxStacksReached += OnMaxBurnStacksReached;
        }

        public override void Unload()
        {
            ResonantBurnNPC.OnMaxStacksReached -= OnMaxBurnStacksReached;
        }

        private void OnMaxBurnStacksReached(NPC npc, Player triggerPlayer)
        {
            if (triggerPlayer.whoAmI != Player.whoAmI)
                return;

            // T3 ResonantPiercingLens: Next crit deals 3x damage
            if (hasResonantPiercingLens)
            {
                resonanceSuperCritReady = true;

                // Visual cue
                for (int i = 0; i < 6; i++)
                {
                    float angle = MathHelper.TwoPi * i / 6f;
                    Vector2 offset = angle.ToRotationVector2() * 25f;
                    CustomParticles.GenericFlare(Player.Center + offset, SummerOrange, 0.4f, 15);
                }
            }

            // T4 EchoingBoltChamber: Spread burn to ALL enemies in 300 units
            if (hasEchoingBoltChamber)
            {
                resonanceSpreadBurnOnCrit = true;

                // Visual cue
                float hue = (Main.GameUpdateCount * 0.02f) % 1f;
                Color rainbowColor = Main.hslToRgb(hue, 0.9f, 0.7f);
                for (int i = 0; i < 8; i++)
                {
                    float angle = MathHelper.TwoPi * i / 8f;
                    Vector2 velocity = angle.ToRotationVector2() * 5f;
                    CustomParticles.GenericGlow(Player.Center, velocity, rainbowColor * 0.6f, 0.3f, 20, true);
                }

                Terraria.Audio.SoundEngine.PlaySound(SoundID.Item4, Player.Center);
            }
        }

        public override void ResetEffects()
        {
            // Reset all accessory flags each frame
            hasResonantSpotter = false;
            hasSpringHuntersLens = false;
            hasResonantPiercingLens = false;
            hasEchoingBoltChamber = false;
            hasSolarTrackersBadge = false;
            hasHarvestReapersMark = false;
            hasPermafrostHuntersEye = false;
            hasVivaldisSeasonalSight = false;
            hasMoonlitPredatorsGaze = false;
            hasHeroicDeadeye = false;
            hasInfernalExecutionersBrand = false;
            hasEnigmasParadoxMark = false;
            hasSwansGracefulHunt = false;
            hasFatesCosmicVerdict = false;
            hasNocturnalPredatorsSight = false;
            hasInfernalExecutionersSight = false;
            hasJubilantHuntersSight = false;
            hasEternalVerdictSight = false;
            hasStarfallExecutionersScope = false;
            hasTriumphantVerdictScope = false;
            hasScopeOfTheEternalVerdict = false;
        }

        public override void PostUpdateEquips()
        {
            // === CHAIN INHERITANCE ===
            // Higher-tier accessories inherit all lower-tier effects.

            // --- Fusion chain inheritance ---
            if (hasScopeOfTheEternalVerdict) { hasTriumphantVerdictScope = true; hasEternalVerdictSight = true; }
            if (hasTriumphantVerdictScope) { hasStarfallExecutionersScope = true; hasJubilantHuntersSight = true; }
            if (hasStarfallExecutionersScope) { hasNocturnalPredatorsSight = true; hasInfernalExecutionersSight = true; }

            // --- Post-Fate T7-T10 linear chain inheritance ---
            if (hasEternalVerdictSight) hasJubilantHuntersSight = true;
            if (hasJubilantHuntersSight) hasInfernalExecutionersSight = true;
            if (hasInfernalExecutionersSight) hasNocturnalPredatorsSight = true;

            // T7 inherits all theme variants + seasonal chain
            if (hasNocturnalPredatorsSight)
            {
                hasFatesCosmicVerdict = true;
                hasSwansGracefulHunt = true;
                hasEnigmasParadoxMark = true;
                hasInfernalExecutionersBrand = true;
                hasHeroicDeadeye = true;
                hasMoonlitPredatorsGaze = true;
                hasVivaldisSeasonalSight = true;
            }

            // --- Seasonal T1-T6 chain inheritance ---
            if (hasVivaldisSeasonalSight) hasPermafrostHuntersEye = true;
            if (hasPermafrostHuntersEye) hasEchoingBoltChamber = true;
            if (hasEchoingBoltChamber) hasResonantPiercingLens = true;
            if (hasResonantPiercingLens) hasSpringHuntersLens = true;
            if (hasSpringHuntersLens) hasResonantSpotter = true;

            // T1-T6 stats are now applied directly in UpdateAccessory on each item.
            // Only T7+ stats and special effects are applied here.

            // Heroic Deadeye: +12% ranged damage, +8% crit
            if (hasHeroicDeadeye)
            {
                Player.GetDamage(DamageClass.Ranged) += 0.12f;
                Player.GetCritChance(DamageClass.Ranged) += 8;
            }

            // Fate's Cosmic Verdict: +15% ranged damage
            if (hasFatesCosmicVerdict)
            {
                Player.GetDamage(DamageClass.Ranged) += 0.15f;
            }

            // Nocturnal Predator's Sight: +20% ranged damage at night, +10% during day, +8% crit at night, night vision
            if (hasNocturnalPredatorsSight)
            {
                if (!Main.dayTime)
                {
                    Player.GetDamage(DamageClass.Ranged) += 0.20f;
                    Player.GetCritChance(DamageClass.Ranged) += 8;
                }
                else
                {
                    Player.GetDamage(DamageClass.Ranged) += 0.10f;
                }
                Player.nightVision = true;
            }

            // Infernal Executioner's Sight: +25% ranged damage during boss fights, +12% otherwise
            if (hasInfernalExecutionersSight)
            {
                if (AnyBossAlive())
                {
                    Player.GetDamage(DamageClass.Ranged) += 0.25f;
                    Player.ammoCost80 = true; // 20% ammo save during bosses
                }
                else
                {
                    Player.GetDamage(DamageClass.Ranged) += 0.12f;
                }
            }

            // Infernal Executioner's Sight: Executioner's Focus buff
            if (hasInfernalExecutionersSight && executionerFocusTimer > 0)
            {
                executionerFocusTimer--;
                Player.GetDamage(DamageClass.Ranged) += 0.15f;
            }

            // Jubilant Hunter's Sight: +15% ranged damage
            if (hasJubilantHuntersSight)
            {
                Player.GetDamage(DamageClass.Ranged) += 0.15f;
            }

            // Jubilant Hunter's Sight: Hunter's Jubilation buff
            if (hasJubilantHuntersSight && hunterJubilationTimer > 0)
            {
                hunterJubilationTimer--;
                Player.GetCritChance(DamageClass.Ranged) += 8;
                Player.GetDamage(DamageClass.Ranged) += 0.05f;
            }

            // Scope of the Eternal Verdict: +30% ranged damage
            if (hasScopeOfTheEternalVerdict)
            {
                Player.GetDamage(DamageClass.Ranged) += 0.30f;
            }

            // Swan's Grace buff timer
            if (graceBuffTimer > 0)
            {
                graceBuffTimer--;
                Player.GetDamage(DamageClass.Ranged) += 0.20f;
            }

            // Cooldown management
            if (gracefulDodgeCooldown > 0)
                gracefulDodgeCooldown--;
        }

        public override void ModifyHitNPCWithItem(Item item, NPC target, ref NPC.HitModifiers modifiers)
        {
            if (!item.DamageType.Equals(DamageClass.Ranged))
                return;

            // Every 3rd shot bonus damage (T1-T6)
            ApplyThirdShotBonus(ref modifiers);
        }

        public override void ModifyHitNPCWithProj(Projectile proj, NPC target, ref NPC.HitModifiers modifiers)
        {
            if (!proj.DamageType.Equals(DamageClass.Ranged) || proj.owner != Player.whoAmI)
                return;

            // Every 3rd shot bonus damage (T1-T6)
            ApplyThirdShotBonus(ref modifiers);
        }

        /// <summary>
        /// Applies the every-3rd-shot bonus damage based on highest tier.
        /// </summary>
        private void ApplyThirdShotBonus(ref NPC.HitModifiers modifiers)
        {
            if (!hasResonantSpotter)
                return;

            rangedShotCounter++;
            if (rangedShotCounter % 3 != 0)
                return;

            float bonusDamage;
            if (hasVivaldisSeasonalSight) bonusDamage = 0.25f;
            else if (hasPermafrostHuntersEye) bonusDamage = 0.20f;
            else if (hasEchoingBoltChamber) bonusDamage = 0.15f;
            else if (hasResonantPiercingLens) bonusDamage = 0.12f;
            else if (hasSpringHuntersLens) bonusDamage = 0.08f;
            else bonusDamage = 0.05f;

            modifiers.FinalDamage += bonusDamage;
        }

        public override void OnHurt(Player.HurtInfo info)
        {
            // Swan's Graceful Hunt: Perfect dodge grants damage buff
            if (hasSwansGracefulHunt && gracefulDodgeCooldown <= 0 && info.Dodgeable)
            {
                graceBuffTimer = 300; // 5 second buff
                gracefulDodgeCooldown = 1800; // 30 second cooldown
            }
        }

        public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!item.DamageType.Equals(DamageClass.Ranged))
                return;

            // ===== T1-T6 RANGER CHAIN COMBAT EFFECTS =====
            ApplyRangerChainOnHit(target, hit, damageDone);

            // ===== T7+ EFFECTS =====

            // Infernal Executioner's Brand: burn
            if (hasInfernalExecutionersBrand)
                target.AddBuff(BuffID.OnFire, 300);

            // Enigma's Paradox Mark: 15% chance homing bolt
            if (hasEnigmasParadoxMark && Main.rand.NextFloat() < 0.15f)
                SpawnHomingBolt(target.Center, Math.Max(1, damageDone / 3));
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!proj.DamageType.Equals(DamageClass.Ranged) || proj.owner != Player.whoAmI)
                return;

            // ===== T1-T6 RANGER CHAIN COMBAT EFFECTS =====
            ApplyRangerChainOnHit(target, hit, damageDone);

            // ===== T7+ EFFECTS =====

            // Infernal Executioner's Brand: burn
            if (hasInfernalExecutionersBrand)
                target.AddBuff(BuffID.OnFire, 300);

            // Enigma's Paradox Mark: 15% chance homing bolt
            if (hasEnigmasParadoxMark && Main.rand.NextFloat() < 0.15f)
                SpawnHomingBolt(target.Center, Math.Max(1, damageDone / 3));

            // Harvest Reaper's Mark: Ranged kills cause explosions
            if (hasHarvestReapersMark && target.life <= 0)
            {
                for (int i = 0; i < 8; i++)
                {
                    float angle = MathHelper.TwoPi * i / 8f;
                    Vector2 velocity = angle.ToRotationVector2() * 4f;
                    Dust dust = Dust.NewDustDirect(target.Center, 1, 1, DustID.Smoke);
                    dust.velocity = velocity;
                }
            }

            // Jubilant Hunter's Sight: Ranged kills restore 4 HP, crit kills grant Hunter's Jubilation
            if (hasJubilantHuntersSight && target.life <= 0)
            {
                Player.Heal(4);
                if (hit.Crit)
                    hunterJubilationTimer = 180; // 3 seconds
            }

            // T8: Executioner's Focus - crit on boss grants +15% ranged damage for 2s
            if (hasInfernalExecutionersSight && hit.Crit && target.boss)
            {
                executionerFocusTimer = 120; // 2 seconds
            }
        }

        /// <summary>
        /// Shared T1-T6 ranger chain on-hit effects: hearts, burn, slow, spread, multi-debuff.
        /// </summary>
        private void ApplyRangerChainOnHit(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // T6: Multi-debuff
            if (hasVivaldisSeasonalSight)
            {
                target.AddBuff(BuffID.OnFire, 180);
                target.AddBuff(BuffID.Frostburn, 180);
                target.AddBuff(BuffID.Poisoned, 180);
                target.AddBuff(BuffID.Bleeding, 180);
            }
            // T3-T5: Burn on hit
            else if (hasResonantPiercingLens)
            {
                target.AddBuff(BuffID.OnFire, 180);
            }

            // Heart drop chances (tier-dependent)
            if (hasSpringHuntersLens)
            {
                float heartChance;
                if (hasVivaldisSeasonalSight) heartChance = 0.05f;
                else if (hasPermafrostHuntersEye) heartChance = 0.03f;
                else if (hasEchoingBoltChamber) heartChance = 0.025f;
                else heartChance = 0.02f;

                if (Main.rand.NextFloat() < heartChance)
                    Item.NewItem(null, target.Center, ItemID.Heart);
            }

            // Slow chance
            if (hasVivaldisSeasonalSight && Main.rand.NextFloat() < 0.08f)
            {
                target.AddBuff(BuffID.Slow, 60);
            }
            else if (hasPermafrostHuntersEye && Main.rand.NextFloat() < 0.05f)
            {
                target.AddBuff(BuffID.Slow, 60);
            }

            // T4+: Critical hits spread burn to nearby enemies
            if (hasEchoingBoltChamber && hit.Crit && target.HasBuff(BuffID.OnFire))
            {
                float spreadRadius = hasVivaldisSeasonalSight ? 300f : 200f;
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (!npc.active || npc.friendly || npc.dontTakeDamage || npc.whoAmI == target.whoAmI)
                        continue;

                    if (Vector2.Distance(target.Center, npc.Center) <= spreadRadius)
                    {
                        npc.AddBuff(BuffID.OnFire, 180);
                        if (hasVivaldisSeasonalSight)
                        {
                            npc.AddBuff(BuffID.Frostburn, 180);
                            npc.AddBuff(BuffID.Poisoned, 180);
                            npc.AddBuff(BuffID.Bleeding, 180);
                        }
                        if (hasPermafrostHuntersEye)
                            npc.AddBuff(BuffID.Slow, 60);
                    }
                }
            }
        }

        /// <summary>
        /// Spreads Resonant Burn to ALL enemies within range.
        /// </summary>
        private void SpreadBurnToAllNearby(Vector2 center, int damage, float range, NPC source)
        {
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly || npc.dontTakeDamage || npc == source)
                    continue;

                if (!ResonancePrefixHelper.IsEnemyBurning(npc))
                {
                    float dist = Vector2.Distance(center, npc.Center);
                    if (dist < range)
                    {
                        ResonancePrefixHelper.ApplyBurnDebuff(npc, damage, Player);
                        ResonancePrefixHelper.SpawnHitVFX(npc.Center);
                    }
                }
            }

            // Big spread VFX
            for (int i = 0; i < 16; i++)
            {
                float angle = MathHelper.TwoPi * i / 16f;
                float hue = (float)i / 16f;
                Color rainbowColor = Main.hslToRgb(hue, 0.9f, 0.7f);
                Vector2 velocity = angle.ToRotationVector2() * 8f;
                CustomParticles.GenericGlow(center, velocity, rainbowColor * 0.5f, 0.4f, 30, true);
            }

            Terraria.Audio.SoundEngine.PlaySound(SoundID.Item45, center);
        }

        /// <summary>
        /// Spawns a homing bolt that deals damage to the nearest enemy.
        /// </summary>
        private void SpawnHomingBolt(Vector2 center, int damage)
        {
            // Find nearest enemy
            NPC closestNPC = null;
            float closestDist = 400f;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly || npc.dontTakeDamage)
                    continue;

                float dist = Vector2.Distance(center, npc.Center);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closestNPC = npc;
                }
            }

            if (closestNPC != null)
            {
                // Visual bolt trail
                Vector2 start = center;
                Vector2 end = closestNPC.Center;
                int segments = (int)(closestDist / 15f);
                for (int i = 0; i < segments; i++)
                {
                    float t = (float)i / segments;
                    Vector2 pos = Vector2.Lerp(start, end, t);
                    pos += Main.rand.NextVector2Circular(5f, 5f);
                    float hue = Main.rand.NextFloat();
                    CustomParticles.GenericGlow(pos, Vector2.Zero, Main.hslToRgb(hue, 0.8f, 0.7f) * 0.6f, 0.2f, 15, true);
                }

                // Deal damage
                Player.ApplyDamageToNPC(closestNPC, damage, 0f, 0, false);

                // Hit effect
                CustomParticles.GenericFlare(closestNPC.Center, SummerOrange, 0.5f, 15);
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
        /// Gets the number of times ranged attacks should hit.
        /// </summary>
        public int GetHitMultiplier()
        {
            if (hasScopeOfTheEternalVerdict)
                return 3; // Triple hit

            if (hasEternalVerdictSight || hasTriumphantVerdictScope)
                return 2; // Double hit

            return 1;
        }

        /// <summary>
        /// Gets the mark color for visual effects.
        /// </summary>
        public Color GetMarkColor()
        {
            if (hasScopeOfTheEternalVerdict) return new Color(200, 170, 100);
            if (hasTriumphantVerdictScope) return new Color(255, 180, 200);
            if (hasStarfallExecutionersScope) return Color.Lerp(new Color(255, 215, 140), new Color(200, 50, 50), 0.5f);
            if (hasEternalVerdictSight) return new Color(200, 170, 100);
            if (hasJubilantHuntersSight) return new Color(220, 200, 255);
            if (hasInfernalExecutionersSight) return new Color(200, 50, 50);
            if (hasNocturnalPredatorsSight) return new Color(255, 215, 140);
            if (hasFatesCosmicVerdict) return new Color(200, 80, 120);
            if (hasSwansGracefulHunt) return new Color(255, 255, 255);
            if (hasEnigmasParadoxMark) return new Color(140, 60, 200);
            if (hasInfernalExecutionersBrand) return new Color(255, 140, 40);
            if (hasHeroicDeadeye) return new Color(255, 200, 80);
            if (hasMoonlitPredatorsGaze) return new Color(138, 43, 226);
            if (hasVivaldisSeasonalSight) return GetSeasonalColor();
            if (hasPermafrostHuntersEye) return new Color(150, 220, 255);
            if (hasHarvestReapersMark) return new Color(180, 100, 40);
            if (hasSolarTrackersBadge) return new Color(255, 140, 0);
            if (hasSpringHuntersLens) return new Color(144, 238, 144);
            return new Color(255, 100, 100); // Default red
        }

        private Color GetSeasonalColor()
        {
            int seasonIndex = (int)(Main.GameUpdateCount / 600) % 4;
            return seasonIndex switch
            {
                0 => new Color(144, 238, 144), // Spring green
                1 => new Color(255, 140, 0),   // Summer orange
                2 => new Color(180, 100, 40),  // Autumn brown
                3 => new Color(150, 220, 255), // Winter blue
                _ => new Color(255, 100, 100)
            };
        }

        /// <summary>
        /// Counts marked enemies (simplified - always returns 0 since we removed the system).
        /// </summary>
        public int CountMarkedEnemies()
        {
            return 0; // Mark system simplified - visual only now
        }

        /// <summary>
        /// Gets the current seasonal debuff type (for compatibility).
        /// </summary>
        public int GetCurrentSeasonalDebuffType()
        {
            return (int)(Main.GameUpdateCount / 600) % 4;
        }

        /// <summary>
        /// Auto-crit compatibility stub (simplified).
        /// </summary>
        public bool TryUseAutoCrit()
        {
            return false;
        }

        /// <summary>
        /// Auto-crit reset compatibility stub (simplified).
        /// </summary>
        public void ResetAutoCritCooldown()
        {
            // No-op
        }
    }
}
