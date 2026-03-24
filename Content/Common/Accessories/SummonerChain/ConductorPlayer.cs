using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

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
            HasScepterOfTheEternalConductor;

        public override void ResetEffects()
        {
            // Reset all accessory flags each frame
            HasConductorsWand = false;
            HasSpringMaestrosBadge = false;
            HasSolarDirectorsCrest = false;
            HasHarvestBeastlordsHorn = false;
            HasPermafrostCommandersCrown = false;
            HasVivaldisOrchestraBaton = false;
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
            // Apply simple static effects from equipped accessories

            // Conductor's Wand: +1 minion slot
            if (HasConductorsWand)
            {
                Player.maxMinions += 1;
            }

            // Spring Maestro's Badge: +1 minion slot, +10% summon damage
            if (HasSpringMaestrosBadge)
            {
                Player.maxMinions += 1;
                Player.GetDamage(DamageClass.Summon) += 0.10f;
            }

            // Solar Director's Crest: +1 minion slot, +15% summon damage
            if (HasSolarDirectorsCrest)
            {
                Player.maxMinions += 1;
                Player.GetDamage(DamageClass.Summon) += 0.15f;
            }

            // Harvest Beastlord's Horn: +1 minion slot, +5% summon crit
            if (HasHarvestBeastlordsHorn)
            {
                Player.maxMinions += 1;
                Player.GetCritChance(DamageClass.Summon) += 5;
            }

            // Permafrost Commander's Crown: +2 minion slots, +20% summon damage
            if (HasPermafrostCommandersCrown)
            {
                Player.maxMinions += 2;
                Player.GetDamage(DamageClass.Summon) += 0.20f;
            }

            // Vivaldi's Orchestra Baton: +2 minion slots, +25% summon damage
            if (HasVivaldisOrchestraBaton)
            {
                Player.maxMinions += 2;
                Player.GetDamage(DamageClass.Summon) += 0.25f;
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

        /// <summary>
        /// Legacy method compatibility stubs (simplified).
        /// </summary>
        public int GetBaseCooldown() => 300;
        public float GetConductDamageBonus() => 0f;
        public int GetMinionKnockbackBonus() => 0;
        public bool CanUseScatter() => false;
        public void StartConduct(int targetWhoAmI) { }
        public void StartScatter() { }
        public void StopConduct() { }
        public void TryConduct() { }
        public void ReleaseConductButton() { }
        public void OnMinionHitNPC(NPC target, int damage, bool crit, Projectile minion) { }
        public bool ShouldMinionsPhase() => HasEnigmasHivemindLink || HasSwansGracefulDirection || HasFatesCosmicDominion ||
            HasNocturnalMaestrosBaton || HasInfernalChoirmastersScepter || HasJubilantOrchestrasStaff ||
            HasEternalConductorsScepter || HasStarfallInfernalBaton || HasTriumphantSymphonyBaton ||
            HasScepterOfTheEternalConductor;
        public bool IsMinionConducted(Projectile minion) => false;
        public bool ShouldMinionTargetConducted(Projectile minion) => false;
        public Color GetConductColor() => new Color(255, 200, 100);
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

            // Swan's Graceful Direction: Grant minion buff when perfect dodging
            // Handled via graceBuffTimer in PostUpdateEquips

            // Infernal Choirmaster's Scepter: Extra damage during boss fights
            // Already handled in PostUpdateEquips damage bonus
        }
    }
}
