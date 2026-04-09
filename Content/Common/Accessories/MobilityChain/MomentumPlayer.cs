using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameInput;
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

        // Key press capture flags — set in ProcessTriggers, consumed in PostUpdate
        // (ProcessTriggers runs BEFORE UpdateAccessory, so accessory flags aren't set yet)
        private bool dashKeyPressed;
        private bool teleportKeyPressed;

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
            // === CHAIN INHERITANCE ===
            // Higher-tier accessories inherit all lower-tier effects.

            // --- Post-Fate T7-T10 linear chain inheritance ---
            if (HasEternalVelocityTreads) HasJubilantZephyrTreads = true;
            if (HasJubilantZephyrTreads) HasInfernalMeteorTreads = true;
            if (HasInfernalMeteorTreads) HasNocturnalPhantomTreads = true;

            // T7 inherits all theme variants + seasonal chain
            if (HasNocturnalPhantomTreads)
            {
                HasFatesCosmicVelocity = true;
                HasSwansEternalGlide = true;
                HasEnigmasPhaseShift = true;
                HasInfernalMeteorStride = true;
                HasHeroicChargeBoots = true;
                HasMoonlitPhantomsRush = true;
                HasVivaldisSeasonalSprint = true;
            }

            // --- Seasonal T1-T6 chain inheritance ---
            if (HasVivaldisSeasonalSprint) HasPermafrostAvalancheStep = true;
            if (HasPermafrostAvalancheStep) HasHarvestPhantomStride = true;
            if (HasHarvestPhantomStride) HasSolarBlitzTreads = true;
            if (HasSolarBlitzTreads) HasSpringZephyrBoots = true;
            if (HasSpringZephyrBoots) HasVelocityBand = true;

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
                // Preserve momentum during boss fights for T10 users
                bool preserveMomentum = HasEternalVelocityTreads && AnyBossAlive();
                if (!preserveMomentum)
                {
                    CurrentMomentum *= MomentumDecayRate;
                    if (CurrentMomentum < 1f)
                        CurrentMomentum = 0f;
                }
            }

            if (dashCooldown > 0) dashCooldown--;
            if (teleportCooldown > 0) teleportCooldown--;

            // Process deferred keybind actions now that accessory flags are available
            if (dashKeyPressed)
            {
                dashKeyPressed = false;
                TryHeroicDash();
            }
            if (teleportKeyPressed)
            {
                teleportKeyPressed = false;
                TryPhaseShift();
            }

            ApplyPassiveMobilityEffects();
            ApplyBlazingTrail();
        }

        /// <summary>
        /// T3+ blazing trail: while moving, leave fire behind and damage nearby enemies.
        /// T6 also adds Frostburn to the trail.
        /// </summary>
        private void ApplyBlazingTrail()
        {
            bool hasTrail = HasSolarBlitzTreads || HasHarvestPhantomStride || HasPermafrostAvalancheStep || HasVivaldisSeasonalSprint;
            if (!hasTrail)
                return;

            float playerSpeed = Player.velocity.Length();
            if (playerSpeed < MinimumVelocityForBuilding)
                return;

            // Fire dust trail
            if (Main.rand.NextBool(3))
            {
                int dustType = DustID.Torch;
                Dust dust = Dust.NewDustDirect(Player.position, Player.width, Player.height, dustType);
                dust.velocity = -Player.velocity * 0.2f;
                dust.noGravity = true;
                dust.scale = 1.2f;
            }

            // T6: also spawn Frostburn dust
            if (HasVivaldisSeasonalSprint && Main.rand.NextBool(3))
            {
                Dust frost = Dust.NewDustDirect(Player.position, Player.width, Player.height, DustID.IceTorch);
                frost.velocity = -Player.velocity * 0.2f;
                frost.noGravity = true;
                frost.scale = 1.0f;
            }

            // Damage nearby enemies with the trail (every 15 frames)
            if (Player.miscCounter % 15 == 0)
            {
                float trailRadius = 80f;
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (!npc.active || npc.friendly || npc.dontTakeDamage)
                        continue;

                    if (Vector2.Distance(Player.Center, npc.Center) <= trailRadius)
                    {
                        npc.AddBuff(BuffID.OnFire, 180);
                        if (HasVivaldisSeasonalSprint)
                            npc.AddBuff(BuffID.Frostburn, 180);
                    }
                }
            }
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

            // T8: Scorched Earth at max momentum (200) — nearby enemies get On Fire! + Slow
            if (HasInfernalMeteorTreads && CurrentMomentum >= 200f)
            {
                ApplyScorchedEarth();
            }

            // T9: No fall damage at 200+ momentum
            if (HasJubilantZephyrTreads && CurrentMomentum >= 200f)
            {
                Player.noFallDmg = true;
            }

            // T9: Jubilant Stride at max momentum (225) — +5% damage, +3% dodge
            if (HasJubilantZephyrTreads && CurrentMomentum >= 225f)
            {
                Player.GetDamage(DamageClass.Generic) += 0.05f;
                Player.endurance += 0.03f; // +3% DR as dodge proxy
            }
        }

        public override void ProcessTriggers(TriggersSet triggersSet)
        {
            // Only capture key presses here — accessory flags aren't set yet
            // (ProcessTriggers runs BEFORE UpdateAccessory in tModLoader's hook order)
            // Actual dash/teleport logic is deferred to PostUpdate where flags are available.
            if (MagnumOpus.DashKeybind?.JustPressed == true)
                dashKeyPressed = true;

            if (MagnumOpus.TeleportKeybind?.JustPressed == true)
                teleportKeyPressed = true;
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

        private static bool AnyBossAlive()
        {
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                if (Main.npc[i].active && Main.npc[i].boss)
                    return true;
            }
            return false;
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

        /// <summary>
        /// T8: Scorched Earth — nearby enemies gain On Fire! + Slow when at max momentum.
        /// </summary>
        private void ApplyScorchedEarth()
        {
            if (Player.miscCounter % 10 != 0) return; // Every 10 frames

            float radius = 120f;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly || npc.dontTakeDamage)
                    continue;

                if (Vector2.Distance(Player.Center, npc.Center) <= radius)
                {
                    npc.AddBuff(BuffID.OnFire3, 120); // 2 seconds
                    npc.AddBuff(BuffID.Slow, 120);    // -10% speed proxy, 2 seconds
                }
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