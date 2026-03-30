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
        public bool hasJubilantHuntersSight;    // Ranged kills restore 2 HP
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
            // Apply simple static effects from equipped accessories

            // Solar Tracker's Badge: +5% ranged damage
            if (hasSolarTrackersBadge)
            {
                Player.GetDamage(DamageClass.Ranged) += 0.05f;
            }

            // Vivaldi's Seasonal Sight: +10% ranged damage
            if (hasVivaldisSeasonalSight)
            {
                Player.GetDamage(DamageClass.Ranged) += 0.10f;
            }

            // ===== RESONANCE SYNERGY BONUSES (T3-T4) =====

            // T3 ResonantPiercingLens: +5% crit per burn stack on target
            // (Applied in ModifyHitNPC since it's per-target)

            // T4 EchoingBoltChamber: Homing bolt damage scales with stacks
            // (Applied in OnHitNPC)

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

            // Nocturnal Predator's Sight: +20% ranged damage at night
            if (hasNocturnalPredatorsSight && !Main.dayTime)
            {
                Player.GetDamage(DamageClass.Ranged) += 0.20f;
            }

            // Infernal Executioner's Sight: +25% ranged damage during boss fights
            if (hasInfernalExecutionersSight && AnyBossAlive())
            {
                Player.GetDamage(DamageClass.Ranged) += 0.25f;
            }

            // Scope of the Eternal Verdict: +40% ranged damage
            if (hasScopeOfTheEternalVerdict)
            {
                Player.GetDamage(DamageClass.Ranged) += 0.40f;
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

        public override void ModifyHitNPCWithProj(Projectile proj, NPC target, ref NPC.HitModifiers modifiers)
        {
            if (!proj.DamageType.Equals(DamageClass.Ranged) || proj.owner != Player.whoAmI)
                return;

            // ===== RESONANCE SYNERGY: T3 ResonantPiercingLens =====
            if (hasResonantPiercingLens && ResonancePrefixHelper.IsEnemyBurning(target))
            {
                // +30% damage vs burning enemies
                modifiers.FinalDamage += 0.30f;

                // +5% crit per burn stack
                int stacks = ResonancePrefixHelper.GetBurnStacks(target);
                Player.GetCritChance(DamageClass.Ranged) += stacks * 5;

                // Super crit: 3x damage instead of 2x
                if (resonanceSuperCritReady && modifiers.CritDamage != null)
                {
                    modifiers.CritDamage += 0.50f; // 2x -> 3x
                }
            }
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
            // Only apply ranger-specific effects if this is a ranged item
            if (!item.DamageType.Equals(DamageClass.Ranged))
                return;

            // Spring Hunter's Lens: 10% chance to drop heart on ranged hit
            if (hasSpringHuntersLens && Main.rand.NextFloat() < 0.10f)
            {
                Item.NewItem(null, target.Center, ItemID.Heart);
            }

            // Harvest Reaper's Mark: Ranged kills cause explosions
            if (hasHarvestReapersMark && target.life <= 0)
            {
                for (int i = 0; i < 8; i++)
                {
                    float angle = MathHelper.TwoPi * i / 8f;
                    Vector2 velocity = angle.ToRotationVector2() * 4f;
                    int dustType = DustID.Smoke;
                    Dust dust = Dust.NewDustDirect(target.Center, 1, 1, dustType);
                    dust.velocity = velocity;
                }
            }

            // Permafrost Hunter's Eye: Ranged attacks slow enemies
            if (hasPermafrostHuntersEye)
            {
                target.velocity *= 0.85f;
                target.AddBuff(BuffID.Slow, 120);
            }

            // Vivaldi's Seasonal Sight: Biome-based debuffs
            if (hasVivaldisSeasonalSight)
            {
                if (Player.ZoneSnow)
                {
                    target.AddBuff(BuffID.Frostburn, 300);
                }
                else if (Player.ZoneDesert)
                {
                    target.AddBuff(BuffID.OnFire, 300);
                }
                else if (Player.ZoneJungle)
                {
                    target.AddBuff(BuffID.Poisoned, 300);
                }
                else
                {
                    target.AddBuff(BuffID.Confused, 240);
                }
            }

            // Infernal Executioner's Brand: Ranged attacks inflict burn
            if (hasInfernalExecutionersBrand)
            {
                target.AddBuff(BuffID.OnFire, 300);
            }

            // Enigma's Paradox Mark: 15% chance for bonus projectile (simplified - visual only)
            if (hasEnigmasParadoxMark && Main.rand.NextFloat() < 0.15f)
            {
                // In a full implementation, would spawn additional projectile
                // For now, just a visual particle
                for (int i = 0; i < 3; i++)
                {
                    Vector2 velocity = Vector2.One.RotatedByRandom(MathHelper.TwoPi) * 2f;
                    Dust dust = Dust.NewDustDirect(target.Center, 1, 1, DustID.Shadowflame);
                    dust.velocity = velocity;
                }
            }
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Only apply ranger effects for ranged projectiles
            if (!proj.DamageType.Equals(DamageClass.Ranged) || proj.owner != Player.whoAmI)
                return;

            // ===== RESONANCE SYNERGY EFFECTS =====

            // T3 ResonantPiercingLens: Consume super crit
            if (hasResonantPiercingLens && resonanceSuperCritReady && hit.Crit)
            {
                resonanceSuperCritReady = false;

                // Big impact VFX
                for (int i = 0; i < 10; i++)
                {
                    float angle = MathHelper.TwoPi * i / 10f;
                    float hue = (float)i / 10f;
                    Color rainbowColor = Main.hslToRgb(hue, 0.9f, 0.75f);
                    Vector2 velocity = angle.ToRotationVector2() * 5f;
                    CustomParticles.GenericFlare(target.Center, rainbowColor, 0.5f, 20);
                }

                Terraria.Audio.SoundEngine.PlaySound(SoundID.Item14, target.Center);
            }

            // T4 EchoingBoltChamber: Crit spreads burn to ALL enemies in 300 units
            if (hasEchoingBoltChamber && hit.Crit && ResonancePrefixHelper.IsEnemyBurning(target))
            {
                if (resonanceSpreadBurnOnCrit)
                {
                    // Spread to ALL enemies in 300 units
                    resonanceSpreadBurnOnCrit = false;
                    SpreadBurnToAllNearby(target.Center, damageDone / 2, 300f, target);
                }
                else
                {
                    // Normal: spread to 1 nearby enemy (200 units)
                    ResonancePrefixHelper.SpreadBurnToNearby(target.Center, damageDone / 2, 200f, target);
                }
            }

            // T4 EchoingBoltChamber: 15% homing bolt (damage scales with stacks)
            if (hasEchoingBoltChamber && Main.rand.NextFloat() < 0.15f)
            {
                int stacks = ResonancePrefixHelper.GetBurnStacks(target);
                float stackMultiplier = 1f + (stacks * 0.20f); // +20% per stack
                int boltDamage = (int)(damageDone * 0.5f * stackMultiplier);

                // Spawn homing bolt (simplified - just visual + damage)
                SpawnHomingBolt(target.Center, boltDamage);
            }

            // Spring Hunter's Lens: 10% chance to drop heart on ranged hit
            if (hasSpringHuntersLens && Main.rand.NextFloat() < 0.10f)
            {
                Item.NewItem(null, target.Center, ItemID.Heart);
            }

            // Harvest Reaper's Mark: Ranged kills cause explosions
            if (hasHarvestReapersMark && target.life <= 0)
            {
                for (int i = 0; i < 8; i++)
                {
                    float angle = MathHelper.TwoPi * i / 8f;
                    Vector2 velocity = angle.ToRotationVector2() * 4f;
                    int dustType = DustID.Smoke;
                    Dust dust = Dust.NewDustDirect(target.Center, 1, 1, dustType);
                    dust.velocity = velocity;
                }
            }

            // Permafrost Hunter's Eye: Ranged attacks slow enemies
            if (hasPermafrostHuntersEye)
            {
                target.velocity *= 0.85f;
                target.AddBuff(BuffID.Slow, 120);
            }

            // Vivaldi's Seasonal Sight: Biome-based debuffs
            if (hasVivaldisSeasonalSight)
            {
                if (Player.ZoneSnow)
                {
                    target.AddBuff(BuffID.Frostburn, 300);
                }
                else if (Player.ZoneDesert)
                {
                    target.AddBuff(BuffID.OnFire, 300);
                }
                else if (Player.ZoneJungle)
                {
                    target.AddBuff(BuffID.Poisoned, 300);
                }
                else
                {
                    target.AddBuff(BuffID.Confused, 240);
                }
            }

            // Infernal Executioner's Brand: Ranged attacks inflict burn
            if (hasInfernalExecutionersBrand)
            {
                target.AddBuff(BuffID.OnFire, 300);
            }

            // Enigma's Paradox Mark: 15% chance for bonus projectile
            if (hasEnigmasParadoxMark && Main.rand.NextFloat() < 0.15f)
            {
                for (int i = 0; i < 3; i++)
                {
                    Vector2 velocity = Vector2.One.RotatedByRandom(MathHelper.TwoPi) * 2f;
                    Dust dust = Dust.NewDustDirect(target.Center, 1, 1, DustID.Shadowflame);
                    dust.velocity = velocity;
                }
            }

            // Jubilant Hunter's Sight: Ranged kills restore 2 HP
            if (hasJubilantHuntersSight && target.life <= 0)
            {
                Player.Heal(2);
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
