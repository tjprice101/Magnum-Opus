using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

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

        // Shield regeneration: damage resets this timer, after 5s at 0, shield recharges
        private int shieldRegenerationCooldown; // Counts up from 0 when shield is depleted
        private const int SHIELD_REGEN_DELAY = 300; // 5 seconds at 60fps
        private bool wasShieldBrokenLastFrame;

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
            if (HasAegisOfTheEternalBastion) return 1.20f;
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
            if (HasVivaldisSeasonalBulwark) return 0.35f;
            if (HasPermafrostCrystalWard) return 0.30f;
            if (HasHarvestThornedGuard) return 0.25f;
            if (HasSolarFlareAegis) return 0.20f;
            if (HasSpringVitalityShell) return 0.15f;
            if (HasResonantBarrierCore) return 0.10f;
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
                    invisibilityDuration = 90;
                }

                if (HasHeroicValorsAegis)
                {
                    damageBoostDuration = 180;
                }

                if (HasSolarFlareAegis || HasInfernalBellsFortress)
                {
                    ApplyNearbyDebuff(BuffID.OnFire3, 120, 180f);
                }

                if (HasPermafrostCrystalWard)
                {
                    ApplyNearbyDebuff(BuffID.Frostburn, 120, 160f);
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
    }
}