using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Common.Accessories.MobilityChain
{
    /// <summary>
    /// Dynamic mobility chain player logic.
    /// Momentum builds from movement velocity and decays when idle, enabling conditional effect gating.
    /// </summary>
    public class MomentumPlayer : ModPlayer
    {
        // Tier 1-6
        public bool HasVelocityBand { get; set; }
        public bool HasSpringZephyrBoots { get; set; }
        public bool HasSolarBlitzTreads { get; set; }
        public bool HasHarvestPhantomStride { get; set; }
        public bool HasPermafrostAvalancheStep { get; set; }
        public bool HasVivaldisSeasonalSprint { get; set; }

        // Post-Moon Lord
        public bool HasMoonlitPhantomsRush { get; set; }
        public bool HasHeroicChargeBoots { get; set; }
        public bool HasInfernalMeteorStride { get; set; }
        public bool HasEnigmasPhaseShift { get; set; }
        public bool HasSwansEternalGlide { get; set; }
        public bool HasFatesCosmicVelocity { get; set; }

        // Post-Fate T7-T10
        public bool HasNocturnalPhantomTreads { get; set; }
        public bool HasInfernalMeteorTreads { get; set; }
        public bool HasJubilantZephyrTreads { get; set; }
        public bool HasEternalVelocityTreads { get; set; }

        public float CurrentMomentum { get; private set; }
        public float MaxMomentum => GetMaxMomentum();

        private int dashCooldown;
        private int teleportCooldown;

        // Momentum building: builds from movement velocity, decays when idle
        private const float MomentumBuildRate = 1.2f; // Per unit of velocity
        private const float MomentumDecayRate = 0.95f; // Multiplier per frame when idle
        private const float MinimumVelocityForBuilding = 2f; // Must move this fast to build momentum

        public bool HasAnyMobilityAccessory =>
            HasVelocityBand || HasSpringZephyrBoots || HasSolarBlitzTreads ||
            HasHarvestPhantomStride || HasPermafrostAvalancheStep || HasVivaldisSeasonalSprint ||
            HasMoonlitPhantomsRush || HasHeroicChargeBoots || HasInfernalMeteorStride ||
            HasEnigmasPhaseShift || HasSwansEternalGlide || HasFatesCosmicVelocity ||
            HasNocturnalPhantomTreads || HasInfernalMeteorTreads || HasJubilantZephyrTreads ||
            HasEternalVelocityTreads;

        public override void ResetEffects()
        {
            HasVelocityBand = false;
            HasSpringZephyrBoots = false;
            HasSolarBlitzTreads = false;
            HasHarvestPhantomStride = false;
            HasPermafrostAvalancheStep = false;
            HasVivaldisSeasonalSprint = false;
            HasMoonlitPhantomsRush = false;
            HasHeroicChargeBoots = false;
            HasInfernalMeteorStride = false;
            HasEnigmasPhaseShift = false;
            HasSwansEternalGlide = false;
            HasFatesCosmicVelocity = false;
            HasNocturnalPhantomTreads = false;
            HasInfernalMeteorTreads = false;
            HasJubilantZephyrTreads = false;
            HasEternalVelocityTreads = false;
        }

        private float GetMaxMomentum()
        {
            if (HasEternalVelocityTreads) return 250f;
            if (HasJubilantZephyrTreads) return 225f;
            if (HasInfernalMeteorTreads) return 200f;
            if (HasNocturnalPhantomTreads) return 175f;
            if (HasFatesCosmicVelocity) return 150f;
            if (HasVivaldisSeasonalSprint || HasMoonlitPhantomsRush || HasHeroicChargeBoots ||
                HasInfernalMeteorStride || HasEnigmasPhaseShift || HasSwansEternalGlide)
                return 120f;
            return 100f;
        }

        public override void PostUpdate()
        {
            if (!HasAnyMobilityAccessory)
            {
                CurrentMomentum = 0f;
                return;
            }

            // Dynamic momentum: builds from movement velocity, decays when idle
            float playerSpeed = Player.velocity.Length();

            if (playerSpeed >= MinimumVelocityForBuilding)
            {
                // Building phase: gain momentum from movement
                float gainAmount = playerSpeed * MomentumBuildRate;
                CurrentMomentum = CurrentMomentum + gainAmount > MaxMomentum ? MaxMomentum : CurrentMomentum + gainAmount;
            }
            else
            {
                // Decay phase: lose momentum when standing still
                CurrentMomentum *= MomentumDecayRate;
                if (CurrentMomentum < 1f)
                    CurrentMomentum = 0f;
            }

            if (dashCooldown > 0) dashCooldown--;
            if (teleportCooldown > 0) teleportCooldown--;

            ApplyPassiveMobilityEffects();
        }

        private void ApplyPassiveMobilityEffects()
        {
            // Keep a small subset of niche effects always active at tier-appropriate points.
            if (HasHarvestPhantomStride || HasPermafrostAvalancheStep || HasVivaldisSeasonalSprint ||
                HasMoonlitPhantomsRush || HasHeroicChargeBoots || HasInfernalMeteorStride ||
                HasEnigmasPhaseShift || HasSwansEternalGlide || HasFatesCosmicVelocity ||
                HasNocturnalPhantomTreads || HasInfernalMeteorTreads || HasJubilantZephyrTreads ||
                HasEternalVelocityTreads)
            {
                Player.noKnockback = true;
            }

            if (HasMoonlitPhantomsRush || HasHeroicChargeBoots || HasInfernalMeteorStride ||
                HasEnigmasPhaseShift || HasSwansEternalGlide || HasFatesCosmicVelocity ||
                HasNocturnalPhantomTreads || HasInfernalMeteorTreads || HasJubilantZephyrTreads ||
                HasEternalVelocityTreads)
            {
                Player.aggro -= 200;
            }

            if ((HasSwansEternalGlide || HasFatesCosmicVelocity) && CurrentMomentum >= MaxMomentum)
            {
                Player.wingTime = Player.wingTimeMax;
            }
            else if ((HasJubilantZephyrTreads || HasEternalVelocityTreads) && CurrentMomentum >= 175f)
            {
                Player.wingTime = Player.wingTimeMax;
            }

            if (HasFatesCosmicVelocity && CurrentMomentum >= 150f)
            {
                ApplyTimeSlowToNearbyEnemies();
            }
            else if (HasEternalVelocityTreads && CurrentMomentum >= 225f)
            {
                ApplyTimeSlowToNearbyEnemies();
            }
        }

        public void TryHeroicDash()
        {
            if (!HasHeroicChargeBoots && !HasInfernalMeteorStride && !HasEnigmasPhaseShift &&
                !HasSwansEternalGlide && !HasFatesCosmicVelocity && !HasNocturnalPhantomTreads &&
                !HasInfernalMeteorTreads && !HasJubilantZephyrTreads && !HasEternalVelocityTreads)
                return;

            if (dashCooldown > 0)
                return;

            dashCooldown = 45;

            Vector2 dashDir = Player.velocity.SafeNormalize(Player.direction == 1 ? Vector2.UnitX : -Vector2.UnitX);
            Player.velocity = dashDir * 25f;

            Rectangle dashHitbox = Player.Hitbox;
            dashHitbox.Inflate(30, 30);

            int damage = (int)(Player.GetDamage(DamageClass.Generic).ApplyTo(50));
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && !npc.friendly && !npc.dontTakeDamage && dashHitbox.Intersects(npc.Hitbox))
                {
                    int dir = npc.Center.X > Player.Center.X ? 1 : -1;
                    Player.ApplyDamageToNPC(npc, damage, 10f, dir, crit: false);
                }
            }

            SoundEngine.PlaySound(SoundID.Item66, Player.Center);
            Player.immune = true;
            Player.immuneTime = 20;
        }

        public void TryPhaseShift()
        {
            if (!HasEnigmasPhaseShift && !HasSwansEternalGlide && !HasFatesCosmicVelocity &&
                !HasNocturnalPhantomTreads && !HasInfernalMeteorTreads && !HasJubilantZephyrTreads &&
                !HasEternalVelocityTreads)
                return;

            if (teleportCooldown > 0)
                return;

            teleportCooldown = 60;

            Vector2 teleportDir = Player.velocity.SafeNormalize(Player.direction == 1 ? Vector2.UnitX : -Vector2.UnitX);
            Vector2 targetPos = Player.Center + teleportDir * 200f;

            for (int attempt = 0; attempt < 10; attempt++)
            {
                Vector2 testPos = Player.Center + teleportDir * (200f - attempt * 16f);
                Point tilePos = testPos.ToTileCoordinates();

                if (!WorldGen.SolidTile(tilePos.X, tilePos.Y) && !WorldGen.SolidTile(tilePos.X, tilePos.Y + 1))
                {
                    targetPos = testPos;
                    break;
                }
            }

            Player.Teleport(targetPos, TeleportationStyleID.RodOfDiscord);
            SoundEngine.PlaySound(SoundID.Item8, targetPos);
        }

        private void ApplyTimeSlowToNearbyEnemies()
        {
            float slowRadius = 300f;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly || npc.boss)
                    continue;

                float dist = Vector2.Distance(Player.Center, npc.Center);
                if (dist <= slowRadius)
                {
                    npc.AddBuff(BuffID.Slow, 2);
                    npc.velocity *= 0.92f;
                }
            }
        }

        public override void ModifyHurt(ref Player.HurtModifiers modifiers)
        {
            if (HasHarvestPhantomStride || HasPermafrostAvalancheStep || HasVivaldisSeasonalSprint ||
                HasMoonlitPhantomsRush || HasHeroicChargeBoots || HasInfernalMeteorStride ||
                HasEnigmasPhaseShift || HasSwansEternalGlide || HasFatesCosmicVelocity ||
                HasNocturnalPhantomTreads || HasInfernalMeteorTreads || HasJubilantZephyrTreads ||
                HasEternalVelocityTreads)
            {
                modifiers.FinalDamage *= 0.75f;
            }
        }

        public override void ModifyDrawInfo(ref PlayerDrawSet drawInfo)
        {
            if (HasMoonlitPhantomsRush || HasHeroicChargeBoots || HasInfernalMeteorStride ||
                HasEnigmasPhaseShift || HasSwansEternalGlide || HasFatesCosmicVelocity ||
                HasNocturnalPhantomTreads || HasInfernalMeteorTreads || HasJubilantZephyrTreads ||
                HasEternalVelocityTreads)
            {
                const float alpha = 0.75f;
                drawInfo.colorArmorBody *= alpha;
                drawInfo.colorArmorHead *= alpha;
                drawInfo.colorArmorLegs *= alpha;
                drawInfo.colorBodySkin *= alpha;
                drawInfo.colorHead *= alpha;
            }
        }
    }
}