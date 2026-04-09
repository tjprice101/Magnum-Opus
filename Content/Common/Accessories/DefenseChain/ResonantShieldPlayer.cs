using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.Nachtmusik.Accessories;

namespace MagnumOpus.Content.Common.Accessories.DefenseChain
{
    /// <summary>
    /// Dynamic defense chain player logic.
    /// Shield absorbs damage before health, regenerates after 5s of not taking damage.
    /// Break effects trigger when shield reaches 0, not on every hit.
    /// </summary>
    public class ResonantShieldPlayer : ModPlayer
    {
        // Tier 1-6
        public bool HasResonantBarrierCore { get; set; }
        public bool HasSpringVitalityShell { get; set; }
        public bool HasSolarFlareAegis { get; set; }
        public bool HasHarvestThornedGuard { get; set; }
        public bool HasPermafrostCrystalWard { get; set; }
        public bool HasVivaldisSeasonalBulwark { get; set; }

        // Post-Moon Lord
        public bool HasMoonlitGuardiansVeil { get; set; }
        public bool HasHeroicValorsAegis { get; set; }
        public bool HasInfernalBellsFortress { get; set; }
        public bool HasEnigmasVoidShell { get; set; }
        public bool HasSwansImmortalGrace { get; set; }
        public bool HasFatesCosmicAegis { get; set; }

        // Post-Fate T7-T10
        public bool HasNocturnalGuardiansWard { get; set; }
        public bool HasInfernalRampartOfDiesIrae { get; set; }
        public bool HasJubilantBulwarkOfJoy { get; set; }
        public bool HasEternalBastionOfTheMoonlight { get; set; }

        // Post-Fate fusions
        public bool HasStarfallInfernalShield { get; set; }
        public bool HasTriumphantJubilantAegis { get; set; }
        public bool HasAegisOfTheEternalBastion { get; set; }

        public float CurrentShield { get; private set; }
        public float MaxShield => Player.statLifeMax2 * ShieldPercent;
        public float ShieldPercent => GetShieldPercent();

        private int lastStandCooldown;
        private int lastStandDuration;
        private int invisibilityDuration;
        private int damageBoostDuration;

        // ===== T7-T9 STATE =====
        private int hymnOfFortitudeTimer;   // T9: Standing still accumulator
        private int hymnOfFortitudeBuff;    // T9: Active buff duration

        // Shield regeneration: damage resets this timer, after 5s at 0, shield recharges
        private int shieldRegenerationCooldown; // Counts up from 0 when shield is depleted
        private const int SHIELD_REGEN_DELAY = 300; // 5 seconds at 60fps
        private bool wasShieldBrokenLastFrame;

        // T1-T6 new mechanic state (replaces shield for these tiers)
        private int noHitTimer;
        private int onHitDefenseTimer;

        private const int LAST_STAND_COOLDOWN = 7200;
        private const int LAST_STAND_DURATION = 180;

        public override void ResetEffects()
        {
            HasResonantBarrierCore = false;
            HasSpringVitalityShell = false;
            HasSolarFlareAegis = false;
            HasHarvestThornedGuard = false;
            HasPermafrostCrystalWard = false;
            HasVivaldisSeasonalBulwark = false;
            HasMoonlitGuardiansVeil = false;
            HasHeroicValorsAegis = false;
            HasInfernalBellsFortress = false;
            HasEnigmasVoidShell = false;
            HasSwansImmortalGrace = false;
            HasFatesCosmicAegis = false;
            HasNocturnalGuardiansWard = false;
            HasInfernalRampartOfDiesIrae = false;
            HasJubilantBulwarkOfJoy = false;
            HasEternalBastionOfTheMoonlight = false;
            HasStarfallInfernalShield = false;
            HasTriumphantJubilantAegis = false;
            HasAegisOfTheEternalBastion = false;
        }

        public bool HasAnyDefenseAccessory()
        {
            return HasResonantBarrierCore || HasSpringVitalityShell || HasSolarFlareAegis ||
                   HasHarvestThornedGuard || HasPermafrostCrystalWard || HasVivaldisSeasonalBulwark ||
                   HasMoonlitGuardiansVeil || HasHeroicValorsAegis || HasInfernalBellsFortress ||
                   HasEnigmasVoidShell || HasSwansImmortalGrace || HasFatesCosmicAegis ||
                   HasNocturnalGuardiansWard || HasInfernalRampartOfDiesIrae || HasJubilantBulwarkOfJoy ||
                   HasEternalBastionOfTheMoonlight || HasStarfallInfernalShield || HasTriumphantJubilantAegis ||
                   HasAegisOfTheEternalBastion;
        }

        private float GetShieldPercent()
        {
            if (HasAegisOfTheEternalBastion) return 1.00f;
            if (HasTriumphantJubilantAegis) return 0.95f;
            if (HasEternalBastionOfTheMoonlight) return 1.00f;
            if (HasStarfallInfernalShield) return 0.85f;
            if (HasJubilantBulwarkOfJoy) return 0.90f;
            if (HasInfernalRampartOfDiesIrae) return 0.80f;
            if (HasNocturnalGuardiansWard) return 0.70f;
            if (HasFatesCosmicAegis) return 0.60f;
            if (HasSwansImmortalGrace) return 0.50f;
            if (HasEnigmasVoidShell) return 0.45f;
            if (HasInfernalBellsFortress) return 0.40f;
            if (HasHeroicValorsAegis) return 0.38f;
            if (HasMoonlitGuardiansVeil) return 0.36f;
            // T1-T6 no longer use shield absorption (replaced with max HP + regen + on-hit defense)
            return 0f;
        }

        public override void PreUpdate()
        {
            if (!HasAnyDefenseAccessory())
            {
                CurrentShield = 0f;
                shieldRegenerationCooldown = 0;
                wasShieldBrokenLastFrame = false;
                return;
            }

            // Shield regeneration logic
            if (CurrentShield <= 0f)
            {
                // Shield is broken - accumulate cooldown timer
                shieldRegenerationCooldown++;
                if (shieldRegenerationCooldown >= SHIELD_REGEN_DELAY)
                {
                    // Regeneration timer elapsed, restore shield to full
                    CurrentShield = MaxShield;
                    shieldRegenerationCooldown = 0;
                }
            }
            else
            {
                // Shield is active - reset regen cooldown
                shieldRegenerationCooldown = 0;
            }

            wasShieldBrokenLastFrame = CurrentShield <= 0f;

            if (lastStandCooldown > 0) lastStandCooldown--;
            if (lastStandDuration > 0) lastStandDuration--;
            if (invisibilityDuration > 0) invisibilityDuration--;
            if (damageBoostDuration > 0) damageBoostDuration--;

            // T1-T6: Track time since last hit for regen mechanic
            noHitTimer++;
            if (onHitDefenseTimer > 0) onHitDefenseTimer--;
        }

        public override void PostUpdate()
        {
            if (invisibilityDuration > 0)
            {
                Player.invis = true;
                Player.aggro -= 1000;
            }

            if (lastStandDuration > 0)
            {
                Player.immune = true;
                Player.immuneTime = 2;
            }

            // Eternal Bastion: +50% faster regen while standing still
            if (HasEternalBastionOfTheMoonlight && Player.velocity.LengthSquared() < 0.1f)
            {
                Player.lifeRegen += 9; // +50% of base 18 regen
            }

            // T9: Hymn of Fortitude - standing still 2s → 6s buff (+8 def, +5 regen, +5% DR)
            if (HasJubilantBulwarkOfJoy)
            {
                if (Player.velocity.LengthSquared() < 0.1f)
                {
                    hymnOfFortitudeTimer++;
                    if (hymnOfFortitudeTimer >= 120) // 2 seconds standing still
                    {
                        hymnOfFortitudeBuff = 360; // 6 seconds
                        hymnOfFortitudeTimer = 120; // Cap accumulator
                    }
                }
                else
                {
                    hymnOfFortitudeTimer = 0;
                }
            }

            if (hymnOfFortitudeBuff > 0)
            {
                hymnOfFortitudeBuff--;
                Player.statDefense += 8;
                Player.lifeRegen += 10; // +5 HP/s
                Player.endurance += 0.05f; // +5% DR
            }

            // Swan's Immortal Grace: +5% dodge when shield is full
            if (HasSwansImmortalGrace && CurrentShield >= MaxShield && MaxShield > 0f)
            {
                Player.blackBelt = true;
            }

            // T3+: Hellfire aura when below 50% HP
            if (Player.statLife < Player.statLifeMax2 / 2)
            {
                if (HasVivaldisSeasonalBulwark)
                {
                    // T6: Multi-debuff aura
                    ApplyNearbyDebuff(BuffID.OnFire3, 120, 200f);
                    ApplyNearbyDebuff(BuffID.Frostburn, 120, 200f);
                    ApplyNearbyDebuff(BuffID.Poisoned, 120, 200f);
                    ApplyNearbyDebuff(BuffID.Bleeding, 120, 200f);
                }
                else if (HasSolarFlareAegis)
                {
                    // T3-T5: Hellfire aura only
                    ApplyNearbyDebuff(BuffID.OnFire3, 120, 200f);
                }
            }
        }

        public override void PostUpdateEquips()
        {
            // === CHAIN INHERITANCE ===
            // Higher-tier accessories inherit all lower-tier effects.

            // --- Fusion chain inheritance ---
            if (HasAegisOfTheEternalBastion) { HasTriumphantJubilantAegis = true; HasEternalBastionOfTheMoonlight = true; }
            if (HasTriumphantJubilantAegis) { HasStarfallInfernalShield = true; HasJubilantBulwarkOfJoy = true; }
            if (HasStarfallInfernalShield) { HasNocturnalGuardiansWard = true; HasInfernalRampartOfDiesIrae = true; }

            // --- Post-Fate T7-T10 linear chain inheritance ---
            if (HasEternalBastionOfTheMoonlight) HasJubilantBulwarkOfJoy = true;
            if (HasJubilantBulwarkOfJoy) HasInfernalRampartOfDiesIrae = true;
            if (HasInfernalRampartOfDiesIrae) HasNocturnalGuardiansWard = true;

            // T7 inherits all theme variants + seasonal chain
            if (HasNocturnalGuardiansWard)
            {
                HasFatesCosmicAegis = true;
                HasSwansImmortalGrace = true;
                HasEnigmasVoidShell = true;
                HasInfernalBellsFortress = true;
                HasHeroicValorsAegis = true;
                HasMoonlitGuardiansVeil = true;
                HasVivaldisSeasonalBulwark = true;
            }

            // --- Seasonal T1-T6 chain inheritance ---
            if (HasVivaldisSeasonalBulwark) HasPermafrostCrystalWard = true;
            if (HasPermafrostCrystalWard) HasHarvestThornedGuard = true;
            if (HasHarvestThornedGuard) HasSolarFlareAegis = true;
            if (HasSolarFlareAegis) HasSpringVitalityShell = true;
            if (HasSpringVitalityShell) HasResonantBarrierCore = true;

            // T1-T6: HP regen after 5 seconds without taking damage
            if (noHitTimer >= SHIELD_REGEN_DELAY)
            {
                int regenAmount = 0;
                if (HasVivaldisSeasonalBulwark) regenAmount = 26;       // +13 HP/s
                else if (HasPermafrostCrystalWard) regenAmount = 22;    // +11 HP/s
                else if (HasHarvestThornedGuard) regenAmount = 18;      // +9 HP/s
                else if (HasSolarFlareAegis) regenAmount = 14;          // +7 HP/s
                else if (HasSpringVitalityShell) regenAmount = 8;       // +4 HP/s
                else if (HasResonantBarrierCore) regenAmount = 4;       // +2 HP/s

                if (regenAmount > 0)
                    Player.lifeRegen += regenAmount;
            }

            // T2+: On-hit defense bonus (active for 1 second after hitting an enemy)
            if (onHitDefenseTimer > 0)
            {
                int defBonus = 0;
                if (HasVivaldisSeasonalBulwark) defBonus = 16;
                else if (HasPermafrostCrystalWard) defBonus = 14;
                else if (HasHarvestThornedGuard) defBonus = 12;
                else if (HasSolarFlareAegis) defBonus = 10;
                else if (HasSpringVitalityShell) defBonus = 8;

                if (defBonus > 0)
                    Player.statDefense += defBonus;
            }
        }

        public override void ModifyHurt(ref Player.HurtModifiers modifiers)
        {
            if (!HasAnyDefenseAccessory())
                return;

            // Dodge chance mechanics (independent of shield)
            if (HasEnigmasVoidShell && Main.rand.NextFloat() < 0.10f)
            {
                modifiers.FinalDamage *= 0f;
                SoundEngine.PlaySound(SoundID.Item8, Player.Center);
                return;
            }

            if (HasSwansImmortalGrace && Main.rand.NextFloat() < 0.05f)
            {
                modifiers.FinalDamage *= 0f;
                SoundEngine.PlaySound(SoundID.Item24, Player.Center);
                return;
            }

            // Shield absorption: if active, reduce damage
            if (CurrentShield > 0f)
            {
                // Shield absorbs a portion of damage
                float damageReduction = (CurrentShield / MaxShield) * 0.5f; // Max 50% reduction when shield is full
                modifiers.FinalDamage *= (1f - damageReduction);

                // Jubilant Bulwark: absorbing hits heals 5% of damage
                if (HasJubilantBulwarkOfJoy)
                {
                    int healAmount = (int)(modifiers.FinalDamage.Multiplicative * 10f * 0.05f);
                    if (healAmount > 0) Player.Heal(healAmount);
                }

                // Triumphant Jubilant Aegis: absorbing hits heals 8% of damage
                if (HasTriumphantJubilantAegis)
                {
                    int healAmount = (int)(modifiers.FinalDamage.Multiplicative * 10f * 0.08f);
                    if (healAmount > 0) Player.Heal(healAmount);
                }

                // Deduct shield based on damage taken
                CurrentShield -= modifiers.FinalDamage.Multiplicative * 10f;
                if (CurrentShield < 0f)
                    CurrentShield = 0f;
            }
        }

        public override void OnHurt(Player.HurtInfo info)
        {
            if (!HasAnyDefenseAccessory())
                return;

            // Reset no-hit regen timer on taking damage
            noHitTimer = 0;

            // Last Stand triggers at low health
            if (HasFatesCosmicAegis && lastStandCooldown <= 0 && Player.statLife <= Player.statLifeMax2 * 0.15f)
            {
                TriggerLastStand();
            }

            // Shield break effects - trigger ONLY when shield breaks (hits 0)
            if (wasShieldBrokenLastFrame && CurrentShield > 0f)
            {
                // Shield was broken last frame but is now restored - don't trigger (regen completed)
            }
            else if (!wasShieldBrokenLastFrame && CurrentShield <= 0f)
            {
                // Shield just broke this frame - trigger break effects
                if (HasSpringVitalityShell || HasVivaldisSeasonalBulwark)
                {
                    Player.Heal(8);
                }

                if (HasMoonlitGuardiansVeil)
                {
                    invisibilityDuration = 120; // 2 seconds
                }

                if (HasHeroicValorsAegis)
                {
                    damageBoostDuration = 300; // 5 seconds
                }

                if (HasSolarFlareAegis || HasInfernalBellsFortress)
                {
                    ApplyNearbyDebuff(BuffID.OnFire3, 120, 180f);
                }

                if (HasPermafrostCrystalWard)
                {
                    ApplyNearbyDebuff(BuffID.Frostburn, 120, 160f);
                }

                // T8: Mors Stupebit on shield break — Feared + -10 def on nearby enemies for 2s
                if (HasInfernalRampartOfDiesIrae)
                {
                    ApplyNearbyDebuff(BuffID.BrokenArmor, 120, 200f); // Proxy for -10 def
                    ApplyNearbyDebuff(BuffID.Confused, 120, 200f);    // Proxy for Feared
                }
            }

            // Damage reflection (triggers on every hit, independent of shield)
            if ((HasHarvestThornedGuard || HasPermafrostCrystalWard || HasVivaldisSeasonalBulwark ||
                 HasMoonlitGuardiansVeil || HasHeroicValorsAegis || HasInfernalBellsFortress ||
                 HasEnigmasVoidShell || HasSwansImmortalGrace || HasFatesCosmicAegis ||
                 HasNocturnalGuardiansWard || HasInfernalRampartOfDiesIrae || HasJubilantBulwarkOfJoy ||
                 HasEternalBastionOfTheMoonlight || HasStarfallInfernalShield ||
                 HasTriumphantJubilantAegis || HasAegisOfTheEternalBastion) &&
                info.DamageSource.TryGetCausingEntity(out Entity attacker) &&
                attacker is NPC npc && npc.active && !npc.friendly && npc.CanBeChasedBy())
            {
                int reflectedDamage = (int)(info.Damage * 0.15f);
                if (reflectedDamage > 0)
                {
                    npc.SimpleStrikeNPC(reflectedDamage, 0, false, 0, null, false, 0, true);
                }

                // Starfall/Infernal Rampart/Aegis: thorns and shield hits inflict fire
                if (HasStarfallInfernalShield || HasInfernalRampartOfDiesIrae || HasAegisOfTheEternalBastion)
                {
                    npc.AddBuff(BuffID.OnFire3, 180); // Hellfire for 3 seconds
                }

                // T7: Sotto Voce at night — slow attacker for 2s
                if (HasNocturnalGuardiansWard && !Main.dayTime)
                {
                    npc.GetGlobalNPC<NachtmusikAccessoryGlobalNPC>().ApplySottoVoce(120);
                }

                // T8: Quantus Tremor — fire DPS + slow atk speed on attackers for 3s
                if (HasInfernalRampartOfDiesIrae)
                {
                    npc.AddBuff(BuffID.OnFire3, 180); // 8 fire DPS proxy
                    npc.AddBuff(BuffID.Slow, 180);    // -8% atk speed proxy
                }
            }
        }

        private void ApplyNearbyDebuff(int buffType, int duration, float radius)
        {
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly || !npc.CanBeChasedBy())
                    continue;

                if (Vector2.Distance(Player.Center, npc.Center) <= radius)
                {
                    npc.AddBuff(buffType, duration);
                }
            }
        }

        private void TriggerLastStand()
        {
            lastStandDuration = LAST_STAND_DURATION;
            lastStandCooldown = LAST_STAND_COOLDOWN;
            SoundEngine.PlaySound(SoundID.Roar with { Volume = 1.2f }, Player.Center);
            Main.NewText("Last Stand Activated!", Color.Gold);
        }

        public override void ModifyHitNPCWithItem(Item item, NPC target, ref NPC.HitModifiers modifiers)
        {
            if (damageBoostDuration > 0)
            {
                modifiers.FinalDamage *= 1.15f;
            }
        }

        public override void ModifyHitNPCWithProj(Projectile proj, NPC target, ref NPC.HitModifiers modifiers)
        {
            if (damageBoostDuration > 0 && proj.owner == Player.whoAmI)
            {
                modifiers.FinalDamage *= 1.15f;
            }
        }

        public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone)
        {
            // T2+: Hitting an enemy grants bonus defense for 1 second
            if (HasSpringVitalityShell || HasSolarFlareAegis || HasHarvestThornedGuard ||
                HasPermafrostCrystalWard || HasVivaldisSeasonalBulwark)
            {
                onHitDefenseTimer = 60;
            }
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (proj.owner != Player.whoAmI) return;

            // T2+: Hitting an enemy grants bonus defense for 1 second
            if (HasSpringVitalityShell || HasSolarFlareAegis || HasHarvestThornedGuard ||
                HasPermafrostCrystalWard || HasVivaldisSeasonalBulwark)
            {
                onHitDefenseTimer = 60;
            }
        }
    }
}