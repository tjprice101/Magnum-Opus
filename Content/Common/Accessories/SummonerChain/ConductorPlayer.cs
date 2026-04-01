using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Prefixes;

namespace MagnumOpus.Content.Common.Accessories.SummonerChain
{
    /// <summary>
    /// Simplified ModPlayer for summoner accessories.
    /// No more Conduct/Scatter/Finale systems - just tracks which accessories are equipped
    /// and applies their simple, static effects.
    /// </summary>
    public class ConductorPlayer : ModPlayer
    {
        // ===== TIER 1-6 (SEASONAL + VIVALDI) FLAGS =====
        public bool HasConductorsWand;                  // +1 minion slot
        public bool HasSpringMaestrosBadge;             // +1 minion slot, 10% summon damage
        public bool HasSolarDirectorsCrest;             // +1 minion slot, 15% summon damage
        public bool HasHarvestBeastlordsHorn;           // +1 minion slot, 5% summon crit
        public bool HasPermafrostCommandersCrown;       // +2 minion slots, 20% summon damage
        public bool HasVivaldisOrchestraBaton;          // +2 minion slots, 25% summon damage

        // ===== T3-T4 RESONANCE SYNERGY FLAGS =====
        public bool HasConductorsBurningCrown;          // T3: +5% minion dmg per burn stack, +50% attack speed at max
        public bool hasResonantBeastlordsHorn;          // T4: Minion crits add 2 stacks, homing at max stacks

        // ===== TIER 5 (THEME VARIANTS) FLAGS =====
        public bool HasMoonlitSymphonyWand;             // +10% summon damage at night
        public bool HasHeroicGeneralsBaton;             // +15% summon damage, +5% crit
        public bool HasInfernalChoirMastersRod;         // Minions inflict burn
        public bool HasEnigmasHivemindLink;             // Minions phase through walls
        public bool HasSwansGracefulDirection;          // Perfect dodge grants minion buff
        public bool HasFatesCosmicDominion;             // +20% summon damage

        // ===== T7-T10 (POST-FATE) FLAGS =====
        public bool HasNocturnalMaestrosBaton;          // +25% summon damage at night
        public bool HasInfernalChoirmastersScepter;     // +30% summon damage during bosses
        public bool HasJubilantOrchestrasStaff;         // Minion hits heal 1 HP
        public bool HasEternalConductorsScepter;        // Minions attack twice

        // ===== FUSION FLAGS =====
        public bool HasStarfallInfernalBaton;           // Nachtmusik + Dies Irae fusion
        public bool HasTriumphantSymphonyBaton;         // 3-theme fusion
        public bool HasScepterOfTheEternalConductor;    // Ultimate: triple attack, +50% damage

        // ===== COOLDOWNS & STATE =====
        public int gracefulDodgeCooldown;  // Swan's Perfect Dodge cooldown
        public int graceBuffTimer;         // Swan's Grace buff timer

        // ===== RESONANCE SYNERGY STATE =====
        public bool resonanceFrenzyActive;              // T3: +50% attack speed active at max stacks
        public int resonanceFrenzyTimer;                // Timer for T3 attack speed buff
        public int temporaryMinionSlots;                // T3: Temporary minion slots from max burn stacks
        public int temporaryMinionSlotTimer;            // Timer for temporary minion slot bonus (8 seconds = 480 ticks)
        public bool resonanceHomingActive;              // T4: Homing active at max stacks
        public int resonanceHomingTimer;                // Timer for T4 homing buff

        // ===== LEGACY COMPATIBILITY STUBS =====
        // These properties exist for backwards compatibility with old code
        public bool IsConducting => false;
        public int ConductedTargetWhoAmI => -1;
        public int ConductDuration => 0;
        public int ConductCooldown => 0;
        public int ConductMaxDuration => 180;
        public bool IsScattering => false;
        public int ScatterDuration => 0;
        public bool IsChargingFinale => false;
        public int FinaleChargeTime => 0;
        public int MinionInvincibilityFrames => 0;
        public bool HasAnyConductorAccessory =>
            HasConductorsWand || HasSpringMaestrosBadge || HasSolarDirectorsCrest ||
            HasHarvestBeastlordsHorn || HasPermafrostCommandersCrown || HasVivaldisOrchestraBaton ||
            HasMoonlitSymphonyWand || HasHeroicGeneralsBaton || HasInfernalChoirMastersRod ||
            HasEnigmasHivemindLink || HasSwansGracefulDirection || HasFatesCosmicDominion ||
            HasNocturnalMaestrosBaton || HasInfernalChoirmastersScepter || HasJubilantOrchestrasStaff ||
            HasEternalConductorsScepter || HasStarfallInfernalBaton || HasTriumphantSymphonyBaton ||
            HasScepterOfTheEternalConductor || HasConductorsBurningCrown || hasResonantBeastlordsHorn;

        public override void Initialize()
        {
            ResonantBurnNPC.OnMaxStacksReached += OnMaxBurnStacksReached;
        }

        public override void Unload()
        {
            ResonantBurnNPC.OnMaxStacksReached -= OnMaxBurnStacksReached;
        }

        /// <summary>
        /// Handles max burn stack triggers for Resonance Synergy accessories.
        /// </summary>
        private void OnMaxBurnStacksReached(NPC npc, Player triggeringPlayer)
        {
            // Only respond to our own burn applications
            if (triggeringPlayer?.whoAmI != Player.whoAmI)
                return;

            // T3 ConductorsBurningCrown: Grant attack speed frenzy + temporary minion slots
            if (HasConductorsBurningCrown)
            {
                resonanceFrenzyActive = true;
                resonanceFrenzyTimer = 300; // 5 seconds

                // Grant +2 temporary minion slots for 8 seconds
                temporaryMinionSlots = 2;
                temporaryMinionSlotTimer = 480; // 8 seconds

                // Visual feedback
                for (int i = 0; i < 10; i++)
                {
                    Dust dust = Dust.NewDustDirect(Player.Center, 1, 1, DustID.Torch);
                    dust.velocity = Vector2.One.RotatedByRandom(MathHelper.TwoPi) * Main.rand.NextFloat(2f, 5f);
                    dust.scale = Main.rand.NextFloat(1.0f, 1.5f);
                    dust.noGravity = true;
                }
            }

            // T4 HarvestBeastlordsHorn: Grant minion homing
            if (hasResonantBeastlordsHorn)
            {
                resonanceHomingActive = true;
                resonanceHomingTimer = 480; // 8 seconds

                // Visual feedback
                for (int i = 0; i < 8; i++)
                {
                    Dust dust = Dust.NewDustDirect(Player.Center, 1, 1, DustID.OrangeTorch);
                    dust.velocity = Vector2.One.RotatedByRandom(MathHelper.TwoPi) * Main.rand.NextFloat(3f, 6f);
                    dust.scale = Main.rand.NextFloat(1.2f, 1.8f);
                    dust.noGravity = true;
                }
            }
        }

        public override void ResetEffects()
        {
            // Reset all accessory flags each frame
            HasConductorsWand = false;
            HasSpringMaestrosBadge = false;
            HasSolarDirectorsCrest = false;
            HasHarvestBeastlordsHorn = false;
            HasPermafrostCommandersCrown = false;
            HasVivaldisOrchestraBaton = false;
            HasConductorsBurningCrown = false;
            hasResonantBeastlordsHorn = false;
            HasMoonlitSymphonyWand = false;
            HasHeroicGeneralsBaton = false;
            HasInfernalChoirMastersRod = false;
            HasEnigmasHivemindLink = false;
            HasSwansGracefulDirection = false;
            HasFatesCosmicDominion = false;
            HasNocturnalMaestrosBaton = false;
            HasInfernalChoirmastersScepter = false;
            HasJubilantOrchestrasStaff = false;
            HasEternalConductorsScepter = false;
            HasStarfallInfernalBaton = false;
            HasTriumphantSymphonyBaton = false;
            HasScepterOfTheEternalConductor = false;
        }

        public override void PostUpdateEquips()
        {
            // === CHAIN INHERITANCE ===
            // Higher-tier accessories inherit all lower-tier effects.
            if (HasVivaldisOrchestraBaton) HasPermafrostCommandersCrown = true;
            if (HasPermafrostCommandersCrown) { HasHarvestBeastlordsHorn = true; hasResonantBeastlordsHorn = true; }
            if (HasHarvestBeastlordsHorn) HasConductorsBurningCrown = true;
            if (HasConductorsBurningCrown) HasSpringMaestrosBadge = true;
            if (HasSpringMaestrosBadge) HasConductorsWand = true;

            // === BASE STATS: Priority system (highest main-chain tier only) ===
            {
                int baseSlots = 0;
                float baseDmg = 0f;
                int baseCrit = 0;
                if (HasVivaldisOrchestraBaton) { baseSlots = 2; baseDmg = 0.25f; }
                else if (HasPermafrostCommandersCrown) { baseSlots = 2; baseDmg = 0.20f; }
                else if (HasHarvestBeastlordsHorn) { baseSlots = 1; baseDmg = 0.12f; baseCrit = 5; }
                else if (HasSpringMaestrosBadge) { baseSlots = 1; baseDmg = 0.10f; }
                else if (HasConductorsWand) { baseSlots = 1; }
                Player.maxMinions += baseSlots;
                Player.GetDamage(DamageClass.Summon) += baseDmg;
                Player.GetCritChance(DamageClass.Summon) += baseCrit;
            }

            // Apply simple static effects from equipped accessories

            // Temporary minion slot bonus management
            if (temporaryMinionSlotTimer > 0)
            {
                Player.maxMinions += temporaryMinionSlots;
                temporaryMinionSlotTimer--;
            }
            else
            {
                temporaryMinionSlots = 0;
            }

            // (Main chain base stats handled by priority system above)

            // ===== RESONANCE SYNERGY: T3 ConductorsBurningCrown =====
            // +5% minion damage per burn stack on any enemy (max +25% at 5 stacks)
            if (HasConductorsBurningCrown)
            {
                int highestStacks = GetHighestBurnStacks();
                if (highestStacks > 0)
                {
                    float damageBonus = 0.05f * highestStacks; // 5% per stack
                    Player.GetDamage(DamageClass.Summon) += damageBonus;
                }

                // Frenzy mode: +50% minion attack speed for 5 seconds at max stacks
                if (resonanceFrenzyActive)
                {
                    // Note: There's no direct minion attack speed stat in Terraria,
                    // so we'll boost damage slightly more during frenzy as a proxy
                    Player.GetDamage(DamageClass.Summon) += 0.25f;

                    if (resonanceFrenzyTimer > 0)
                        resonanceFrenzyTimer--;
                    else
                        resonanceFrenzyActive = false;
                }
            }

            // ===== RESONANCE SYNERGY: T4 HarvestBeastlordsHorn =====
            // +3% crit per burn stack (max +15% at 5 stacks)
            if (hasResonantBeastlordsHorn)
            {
                int highestStacks = GetHighestBurnStacks();
                if (highestStacks > 0)
                {
                    int critBonus = 3 * highestStacks; // 3% per stack
                    Player.GetCritChance(DamageClass.Summon) += critBonus;
                }

                // Homing mode active timer management
                if (resonanceHomingActive && resonanceHomingTimer > 0)
                {
                    resonanceHomingTimer--;
                    if (resonanceHomingTimer <= 0)
                        resonanceHomingActive = false;
                }
            }

            // (Permafrost Commander's Crown and Vivaldi's Orchestra Baton base stats handled by priority system above)

            // Solar Director's Crest: +1 minion slot, +15% summon damage (theme variant, not in main chain)
            if (HasSolarDirectorsCrest)
            {
                Player.maxMinions += 1;
                Player.GetDamage(DamageClass.Summon) += 0.15f;
            }

            // Moonlit Symphony Wand: +10% summon damage at night
            if (HasMoonlitSymphonyWand && !Main.dayTime)
            {
                Player.GetDamage(DamageClass.Summon) += 0.10f;
            }

            // Heroic General's Baton: +15% summon damage, +5% crit
            if (HasHeroicGeneralsBaton)
            {
                Player.GetDamage(DamageClass.Summon) += 0.15f;
                Player.GetCritChance(DamageClass.Summon) += 5;
            }

            // Fate's Cosmic Dominion: +20% summon damage
            if (HasFatesCosmicDominion)
            {
                Player.GetDamage(DamageClass.Summon) += 0.20f;
            }

            // Nocturnal Maestro's Baton: +25% summon damage at night
            if (HasNocturnalMaestrosBaton && !Main.dayTime)
            {
                Player.GetDamage(DamageClass.Summon) += 0.25f;
            }

            // Infernal Choirmaster's Scepter: +30% summon damage during boss fights
            if (HasInfernalChoirmastersScepter && AnyBossAlive())
            {
                Player.GetDamage(DamageClass.Summon) += 0.30f;
            }

            // Scepter of the Eternal Conductor: +50% summon damage
            if (HasScepterOfTheEternalConductor)
            {
                Player.GetDamage(DamageClass.Summon) += 0.50f;
            }

            // Swan's Grace buff timer
            if (graceBuffTimer > 0)
            {
                graceBuffTimer--;
                Player.GetDamage(DamageClass.Summon) += 0.25f;
            }

            // Cooldown management
            if (gracefulDodgeCooldown > 0)
                gracefulDodgeCooldown--;
        }

        public override void OnHurt(Player.HurtInfo info)
        {
            // Swan's Graceful Direction: Perfect dodge grants minion buff
            if (HasSwansGracefulDirection && gracefulDodgeCooldown <= 0 && info.Dodgeable)
            {
                graceBuffTimer = 300; // 5 second buff
                gracefulDodgeCooldown = 1800; // 30 second cooldown
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
        /// Gets the highest burn stack count from any active NPC.
        /// Used for Resonance Synergy accessories.
        /// </summary>
        private int GetHighestBurnStacks()
        {
            int highest = 0;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && !npc.friendly)
                {
                    int stacks = ResonancePrefixHelper.GetBurnStacks(npc);
                    if (stacks > highest)
                        highest = stacks;
                }
            }
            return highest;
        }

        /// <summary>
        /// Gets the number of times minions should attack.
        /// </summary>
        public int GetHitMultiplier()
        {
            if (HasScepterOfTheEternalConductor)
                return 3; // Triple attack

            if (HasEternalConductorsScepter || HasTriumphantSymphonyBaton)
                return 2; // Double attack

            return 1;
        }

        public bool ShouldMinionsPhase() => HasEnigmasHivemindLink || HasSwansGracefulDirection || HasFatesCosmicDominion ||
            HasNocturnalMaestrosBaton || HasInfernalChoirmastersScepter || HasJubilantOrchestrasStaff ||
            HasEternalConductorsScepter || HasStarfallInfernalBaton || HasTriumphantSymphonyBaton ||
            HasScepterOfTheEternalConductor;
    }

    /// <summary>
    /// GlobalProjectile to handle minion behavior modifications (simplified).
    /// </summary>
    public class ConductorMinionGlobalProjectile : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool TileCollideStyle(Projectile projectile, ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac)
        {
            if (!projectile.minion || projectile.owner < 0 || projectile.owner >= Main.maxPlayers)
                return true;

            Player owner = Main.player[projectile.owner];
            ConductorPlayer conductor = owner.GetModPlayer<ConductorPlayer>();

            // Allow phasing through blocks with Enigma ability
            if (conductor != null && conductor.ShouldMinionsPhase())
            {
                projectile.tileCollide = false;
            }

            return true;
        }

        public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!projectile.minion || projectile.owner < 0 || projectile.owner >= Main.maxPlayers)
                return;

            Player owner = Main.player[projectile.owner];
            ConductorPlayer conductor = owner.GetModPlayer<ConductorPlayer>();

            if (conductor == null)
                return;

            // Infernal Choir Master's Rod: Minions inflict burn
            if (conductor.HasInfernalChoirMastersRod)
            {
                target.AddBuff(BuffID.OnFire, 300);
            }

            // Jubilant Orchestra's Staff: Minion hits heal 1 HP
            if (conductor.HasJubilantOrchestrasStaff)
            {
                owner.Heal(1);
            }

            // ===== RESONANCE SYNERGY: T3 ConductorsBurningCrown =====
            // Minion hits apply 1 burn stack so the +5% per stack bonus works
            if (conductor.HasConductorsBurningCrown && !conductor.hasResonantBeastlordsHorn)
            {
                if (ResonancePrefixHelper.IsEnemyBurning(target))
                {
                    var burnNpc = target.GetGlobalNPC<ResonantBurnNPC>();
                    burnNpc.AddStack(target, owner);
                }
                else
                {
                    ResonancePrefixHelper.ApplyBurnDebuff(target, damageDone, owner);
                }
            }

            // ===== RESONANCE SYNERGY: T4 HarvestBeastlordsHorn =====
            // Minion crits add 2 burn stacks to target
            if (conductor.hasResonantBeastlordsHorn && hit.Crit)
            {
                // Add 2 burn stacks (via applying burn with extra stacks)
                if (ResonancePrefixHelper.IsEnemyBurning(target))
                {
                    // Add extra stacks directly
                    var burnNpc = target.GetGlobalNPC<ResonantBurnNPC>();
                    burnNpc.burnStacks = Math.Min(burnNpc.burnStacks + 2, ResonantBurnNPC.MAX_STACKS);
                }
                else
                {
                    // Apply burn with initial stacks
                    ResonancePrefixHelper.ApplyBurnDebuff(target, 300, owner);
                    var burnNpc = target.GetGlobalNPC<ResonantBurnNPC>();
                    burnNpc.burnStacks = Math.Min(burnNpc.burnStacks + 1, ResonantBurnNPC.MAX_STACKS); // +1 more for 2 total
                }
            }

            // T4 Homing behavior: Enhanced targeting when homing is active
            // (Minions automatically target burning enemies - handled via AI modification below)

            // Swan's Graceful Direction: Grant minion buff when perfect dodging
            // Handled via graceBuffTimer in PostUpdateEquips

            // Infernal Choirmaster's Scepter: Extra damage during boss fights
            // Already handled in PostUpdateEquips damage bonus
        }

        public override void AI(Projectile projectile)
        {
            if (!projectile.minion || projectile.owner < 0 || projectile.owner >= Main.maxPlayers)
                return;

            Player owner = Main.player[projectile.owner];
            ConductorPlayer conductor = owner.GetModPlayer<ConductorPlayer>();

            if (conductor == null)
                return;

            // T4 HarvestBeastlordsHorn: Homing to burning enemies when active
            if (conductor.hasResonantBeastlordsHorn && conductor.resonanceHomingActive)
            {
                // Find closest burning enemy
                float closestDist = 600f;
                NPC closestBurning = null;

                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (npc.active && !npc.friendly && !npc.dontTakeDamage && ResonancePrefixHelper.IsEnemyBurning(npc))
                    {
                        float dist = Vector2.Distance(projectile.Center, npc.Center);
                        if (dist < closestDist)
                        {
                            closestDist = dist;
                            closestBurning = npc;
                        }
                    }
                }

                // Gently home toward burning enemy
                if (closestBurning != null)
                {
                    Vector2 direction = closestBurning.Center - projectile.Center;
                    direction.Normalize();
                    projectile.velocity = Vector2.Lerp(projectile.velocity, direction * projectile.velocity.Length(), 0.05f);
                }
            }
        }
    }
}
