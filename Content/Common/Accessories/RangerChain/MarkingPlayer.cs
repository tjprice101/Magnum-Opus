using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

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

        // ===== TIER 1-6 (SEASONAL + VIVALDI) FLAGS =====
        public bool hasResonantSpotter;         // Ranged attacks mark enemies (visual only)
        public bool hasSpringHuntersLens;       // 10% heart drop chance on ranged hit
        public bool hasSolarTrackersBadge;      // +5% ranged damage
        public bool hasHarvestReapersMark;      // Ranged kills cause explosions
        public bool hasPermafrostHuntersEye;    // Ranged attacks slow enemies
        public bool hasVivaldisSeSonalSight;    // +10% ranged damage, biome debuffs

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

        public override void ResetEffects()
        {
            // Reset all accessory flags each frame
            hasResonantSpotter = false;
            hasSpringHuntersLens = false;
            hasSolarTrackersBadge = false;
            hasHarvestReapersMark = false;
            hasPermafrostHuntersEye = false;
            hasVivaldisSeSonalSight = false;
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
            if (hasVivaldisSeSonalSight)
            {
                Player.GetDamage(DamageClass.Ranged) += 0.10f;
            }

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
            if (hasVivaldisSeSonalSight)
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
            if (hasVivaldisSeSonalSight)
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
            if (hasVivaldisSeSonalSight) return GetSeasonalColor();
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
