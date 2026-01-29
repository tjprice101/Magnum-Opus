using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.LaCampanella.Debuffs;

namespace MagnumOpus.Content.Common.Accessories.SummonerChain
{
    /// <summary>
    /// ModPlayer for the Conductor's Baton System - Summoner Chain
    /// Right-click to "Conduct": Focus all minions on a single target with enhanced effects
    /// Double-tap for "Scatter": Spread minions to all nearby enemies (with Vivaldi's Orchestra Baton or higher)
    /// </summary>
    public class ConductorPlayer : ModPlayer
    {
        // ========== ACCESSORY FLAGS ==========
        // Tier 1: Resonant Conductor's Wand
        public bool HasConductorsWand { get; set; }
        
        // Tier 2: Spring Maestro's Badge
        public bool HasSpringMaestrosBadge { get; set; }
        
        // Tier 3: Solar Director's Crest
        public bool HasSolarDirectorsCrest { get; set; }
        
        // Tier 4: Harvest Beastlord's Horn
        public bool HasHarvestBeastlordsHorn { get; set; }
        
        // Tier 5: Permafrost Commander's Crown
        public bool HasPermafrostCommandersCrown { get; set; }
        
        // Tier 6: Vivaldi's Orchestra Baton (unlocks Scatter command)
        public bool HasVivaldisOrchestraBaton { get; set; }
        
        // Post-Moon Lord Theme Chain
        public bool HasMoonlitSymphonyWand { get; set; }       // T1 - Night bonus
        public bool HasHeroicGeneralsBaton { get; set; }       // T2 - Minion invincibility
        public bool HasInfernalChoirMastersRod { get; set; }   // T3 - Minion explosions
        public bool HasEnigmasHivemindLink { get; set; }       // T4 - Minions phase through blocks
        public bool HasSwansGracefulDirection { get; set; }    // T5 - Full HP double damage
        public bool HasFatesCosmicDominion { get; set; }       // T6 - 5s cooldown + Finale ability
        
        // ========== CONDUCT STATE ==========
        public bool IsConducting { get; private set; }
        public int ConductedTargetWhoAmI { get; private set; } = -1;
        public int ConductDuration { get; private set; }
        public int ConductCooldown { get; private set; }
        public int ConductMaxDuration { get; private set; } = 180; // 3 seconds at 60fps
        
        // Scatter state (double-tap command)
        public bool IsScattering { get; private set; }
        public int ScatterDuration { get; private set; }
        private int lastRightClickTime;
        private const int DoubleTapWindow = 15; // 0.25 seconds
        
        // Finale state (hold conduct for massive hit)
        public bool IsChargingFinale { get; private set; }
        public int FinaleChargeTime { get; private set; }
        private const int FinaleChargeRequired = 120; // 2 seconds to fully charge
        
        // Minion invincibility tracking
        public int MinionInvincibilityFrames { get; private set; }
        
        // VFX tracking
        private int conductVFXTimer;
        
        // Theme colors
        private static readonly Color ConductorGold = new Color(255, 200, 100);
        private static readonly Color ConductorSilver = new Color(200, 210, 230);
        private static readonly Color MoonlitPurple = new Color(150, 120, 200);
        private static readonly Color HeroicScarlet = new Color(200, 80, 80);
        private static readonly Color InfernalOrange = new Color(255, 140, 50);
        private static readonly Color EnigmaPurple = new Color(140, 60, 200);
        private static readonly Color SwanWhite = new Color(240, 245, 255);
        private static readonly Color FateCrimson = new Color(180, 40, 80);
        
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
        }
        
        /// <summary>
        /// Check if any conductor accessory is equipped
        /// </summary>
        public bool HasAnyConductorAccessory =>
            HasConductorsWand || HasSpringMaestrosBadge || HasSolarDirectorsCrest ||
            HasHarvestBeastlordsHorn || HasPermafrostCommandersCrown || HasVivaldisOrchestraBaton ||
            HasMoonlitSymphonyWand || HasHeroicGeneralsBaton || HasInfernalChoirMastersRod ||
            HasEnigmasHivemindLink || HasSwansGracefulDirection || HasFatesCosmicDominion;
        
        /// <summary>
        /// Get the base cooldown based on equipped accessory
        /// </summary>
        public int GetBaseCooldown()
        {
            if (HasFatesCosmicDominion) return 300;        // 5 seconds
            if (HasPermafrostCommandersCrown) return 480;  // 8 seconds (also inherited by Vivaldi+)
            if (HasSolarDirectorsCrest) return 600;        // 10 seconds
            if (HasSpringMaestrosBadge) return 720;        // 12 seconds
            return 900; // 15 seconds (base tier)
        }
        
        /// <summary>
        /// Get the bonus minion damage during conduct
        /// </summary>
        public float GetConductDamageBonus()
        {
            float bonus = 0.20f; // Base 20%
            
            if (HasHarvestBeastlordsHorn || HasPermafrostCommandersCrown || HasVivaldisOrchestraBaton ||
                HasMoonlitSymphonyWand || HasHeroicGeneralsBaton || HasInfernalChoirMastersRod ||
                HasEnigmasHivemindLink || HasSwansGracefulDirection || HasFatesCosmicDominion)
            {
                bonus = 0.30f; // Harvest tier+ = 30%
            }
            
            // Night bonus from Moonlit Symphony Wand
            if ((HasMoonlitSymphonyWand || HasHeroicGeneralsBaton || HasInfernalChoirMastersRod ||
                 HasEnigmasHivemindLink || HasSwansGracefulDirection || HasFatesCosmicDominion) &&
                !Main.dayTime)
            {
                bonus += 0.10f; // +10% at night
            }
            
            // Perfect conduct from Swan's Graceful Direction
            if ((HasSwansGracefulDirection || HasFatesCosmicDominion) && Player.statLife == Player.statLifeMax2)
            {
                bonus *= 2f; // Double damage at full HP
            }
            
            return bonus;
        }
        
        public override void PostUpdate()
        {
            // Update cooldown
            if (ConductCooldown > 0)
                ConductCooldown--;
            
            // Update conduct duration
            if (IsConducting)
            {
                ConductDuration--;
                
                // VFX while conducting
                if (conductVFXTimer++ % 8 == 0)
                    SpawnConductVFX();
                
                // Validate target still exists and is alive
                if (ConductedTargetWhoAmI >= 0 && ConductedTargetWhoAmI < Main.maxNPCs)
                {
                    NPC target = Main.npc[ConductedTargetWhoAmI];
                    if (!target.active || target.life <= 0 || target.friendly)
                    {
                        EndConduct(killedTarget: true);
                        return;
                    }
                    
                    // Apply Permafrost slow
                    if (HasPermafrostCommandersCrown || HasVivaldisOrchestraBaton ||
                        HasMoonlitSymphonyWand || HasHeroicGeneralsBaton || HasInfernalChoirMastersRod ||
                        HasEnigmasHivemindLink || HasSwansGracefulDirection || HasFatesCosmicDominion)
                    {
                        // Apply slow debuff (Chilled works well for 25% slow)
                        target.AddBuff(BuffID.Chilled, 2);
                    }
                    
                    // Apply "Performed" debuff (defense reduction) from Solar Director's Crest+
                    if (HasSolarDirectorsCrest || HasHarvestBeastlordsHorn || HasPermafrostCommandersCrown ||
                        HasVivaldisOrchestraBaton || HasMoonlitSymphonyWand || HasHeroicGeneralsBaton ||
                        HasInfernalChoirMastersRod || HasEnigmasHivemindLink || HasSwansGracefulDirection ||
                        HasFatesCosmicDominion)
                    {
                        // Apply armor break for defense reduction
                        target.AddBuff(BuffID.BrokenArmor, 2);
                    }
                }
                
                // End conduct when duration expires
                if (ConductDuration <= 0)
                {
                    EndConduct(killedTarget: false);
                }
            }
            
            // Update scatter duration
            if (IsScattering)
            {
                ScatterDuration--;
                if (ScatterDuration <= 0)
                {
                    IsScattering = false;
                }
            }
            
            // Update minion invincibility
            if (MinionInvincibilityFrames > 0)
                MinionInvincibilityFrames--;
            
            // Finale charge handling
            if (IsChargingFinale)
            {
                FinaleChargeTime++;
                
                // VFX while charging
                if (FinaleChargeTime % 5 == 0)
                    SpawnFinaleChargeVFX();
                
                // Release finale if fully charged
                if (FinaleChargeTime >= FinaleChargeRequired)
                {
                    ExecuteFinale();
                }
            }
        }
        
        /// <summary>
        /// Called when player right-clicks - attempt to conduct
        /// </summary>
        public void TryConduct()
        {
            if (!HasAnyConductorAccessory)
                return;
            
            // Check for double-tap (Scatter command)
            if (HasVivaldisOrchestraBaton || HasMoonlitSymphonyWand || HasHeroicGeneralsBaton ||
                HasInfernalChoirMastersRod || HasEnigmasHivemindLink || HasSwansGracefulDirection ||
                HasFatesCosmicDominion)
            {
                int currentTime = (int)Main.GameUpdateCount;
                if (currentTime - lastRightClickTime <= DoubleTapWindow)
                {
                    // Double-tap detected - Scatter!
                    ExecuteScatter();
                    lastRightClickTime = 0; // Reset to prevent triple-tap issues
                    return;
                }
                lastRightClickTime = currentTime;
            }
            
            // Check for Finale (hold conduct with Fate accessory)
            if (HasFatesCosmicDominion && !IsConducting && ConductCooldown <= 0)
            {
                // Start charging finale - will be handled in PostUpdate
                // Player needs to hold right-click
                IsChargingFinale = true;
                FinaleChargeTime = 0;
            }
            
            // Normal conduct
            if (ConductCooldown > 0)
                return;
            
            // Find nearest enemy to player's cursor
            NPC target = FindConductTarget();
            if (target == null)
                return;
            
            StartConduct(target);
        }
        
        /// <summary>
        /// Called when player releases right-click
        /// </summary>
        public void ReleaseConductButton()
        {
            if (IsChargingFinale && FinaleChargeTime < FinaleChargeRequired)
            {
                // Released too early - just do normal conduct
                IsChargingFinale = false;
                FinaleChargeTime = 0;
            }
        }
        
        /// <summary>
        /// Find the nearest enemy to the player's cursor for conducting
        /// </summary>
        private NPC FindConductTarget()
        {
            Vector2 mouseWorld = Main.MouseWorld;
            float maxDist = 800f; // 50 tiles detection range
            NPC bestTarget = null;
            float bestDist = maxDist;
            
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly || npc.dontTakeDamage || npc.immortal)
                    continue;
                
                float dist = Vector2.Distance(mouseWorld, npc.Center);
                if (dist < bestDist)
                {
                    bestDist = dist;
                    bestTarget = npc;
                }
            }
            
            return bestTarget;
        }
        
        /// <summary>
        /// Start conducting - focus all minions on target
        /// </summary>
        private void StartConduct(NPC target)
        {
            IsConducting = true;
            ConductedTargetWhoAmI = target.whoAmI;
            ConductDuration = ConductMaxDuration;
            ConductCooldown = GetBaseCooldown();
            conductVFXTimer = 0;
            
            // Grant minion invincibility if we have Heroic General's Baton+
            if (HasHeroicGeneralsBaton || HasInfernalChoirMastersRod || HasEnigmasHivemindLink ||
                HasSwansGracefulDirection || HasFatesCosmicDominion)
            {
                MinionInvincibilityFrames = 60; // 1 second of invincibility
            }
            
            // VFX on conduct start
            SpawnConductStartVFX(target.Center);
            
            // Sound effect
            SoundEngine.PlaySound(SoundID.Item4 with { Pitch = 0.3f }, Player.Center);
            
            // Combat text
            CombatText.NewText(Player.Hitbox, ConductorGold, "CONDUCT!", true);
        }
        
        /// <summary>
        /// End the conduct state
        /// </summary>
        private void EndConduct(bool killedTarget)
        {
            IsConducting = false;
            
            // If killed target with Harvest Beastlord's Horn+, extend buff
            if (killedTarget && (HasHarvestBeastlordsHorn || HasPermafrostCommandersCrown ||
                HasVivaldisOrchestraBaton || HasMoonlitSymphonyWand || HasHeroicGeneralsBaton ||
                HasInfernalChoirMastersRod || HasEnigmasHivemindLink || HasSwansGracefulDirection ||
                HasFatesCosmicDominion))
            {
                // Grant bonus for 2 seconds
                Player.AddBuff(BuffID.Rage, 120); // Temporary damage buff
            }
            
            ConductedTargetWhoAmI = -1;
            ConductDuration = 0;
            IsChargingFinale = false;
            FinaleChargeTime = 0;
        }
        
        /// <summary>
        /// Execute scatter command - spread minions to all nearby enemies
        /// </summary>
        private void ExecuteScatter()
        {
            IsScattering = true;
            ScatterDuration = 90; // 1.5 seconds
            
            // VFX
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 dustPos = Player.Center + angle.ToRotationVector2() * 30f;
                Dust dust = Dust.NewDustPerfect(dustPos, DustID.MagicMirror, angle.ToRotationVector2() * 4f);
                dust.noGravity = true;
                dust.scale = 1.5f;
                dust.color = ConductorGold;
            }
            
            SoundEngine.PlaySound(SoundID.Item8, Player.Center);
            CombatText.NewText(Player.Hitbox, ConductorSilver, "SCATTER!", true);
        }
        
        /// <summary>
        /// Execute the Finale - sacrifice all minions for massive damage
        /// </summary>
        private void ExecuteFinale()
        {
            IsChargingFinale = false;
            FinaleChargeTime = 0;
            
            // Find target
            NPC target = FindConductTarget();
            if (target == null)
                return;
            
            // Count minions and calculate damage
            int minionCount = 0;
            int totalMinionDamage = 0;
            
            foreach (Projectile proj in Main.projectile)
            {
                if (proj.active && proj.owner == Player.whoAmI && proj.minion)
                {
                    minionCount++;
                    totalMinionDamage += proj.damage;
                    
                    // Kill the minion with VFX
                    SpawnMinionSacrificeVFX(proj.Center);
                    proj.Kill();
                }
            }
            
            if (minionCount == 0)
                return;
            
            // Deal massive damage (sum of all minion damage * multiplier)
            int finaleDamage = (int)(totalMinionDamage * 5f); // 5x total minion damage
            
            // Create the finale strike
            int strikeDir = target.Center.X > Player.Center.X ? 1 : -1;
            Player.ApplyDamageToNPC(target, finaleDamage, 15f, strikeDir, crit: true);
            
            // Massive VFX
            SpawnFinaleImpactVFX(target.Center);
            
            // Sound
            SoundEngine.PlaySound(SoundID.Item119 with { Pitch = -0.3f, Volume = 1.5f }, target.Center);
            
            CombatText.NewText(target.Hitbox, FateCrimson, "FINALE!", true);
            
            // Long cooldown after finale
            ConductCooldown = 600; // 10 seconds
        }
        
        /// <summary>
        /// Called when a minion hits an NPC - apply conductor bonuses
        /// </summary>
        public void OnMinionHitNPC(NPC target, int damage, bool crit, Projectile minion)
        {
            // Healing from Spring Maestro's Badge+
            if (IsConducting && target.whoAmI == ConductedTargetWhoAmI)
            {
                if (HasSpringMaestrosBadge || HasSolarDirectorsCrest || HasHarvestBeastlordsHorn ||
                    HasPermafrostCommandersCrown || HasVivaldisOrchestraBaton || HasMoonlitSymphonyWand ||
                    HasHeroicGeneralsBaton || HasInfernalChoirMastersRod || HasEnigmasHivemindLink ||
                    HasSwansGracefulDirection || HasFatesCosmicDominion)
                {
                    // Heal 1 HP per hit
                    Player.Heal(1);
                }
                
                // Explosion from Infernal Choir Master's Rod+
                if (HasInfernalChoirMastersRod || HasEnigmasHivemindLink || 
                    HasSwansGracefulDirection || HasFatesCosmicDominion)
                {
                    SpawnMinionExplosion(target.Center, damage);
                }
            }
        }
        
        /// <summary>
        /// Check if minions should phase through blocks (Enigma ability)
        /// </summary>
        public bool ShouldMinionsPhase()
        {
            if (!IsConducting) return false;
            return HasEnigmasHivemindLink || HasSwansGracefulDirection || HasFatesCosmicDominion;
        }
        
        // ========== VFX METHODS ==========
        
        private void SpawnConductStartVFX(Vector2 targetPos)
        {
            // Lines from player to target
            Vector2 direction = (targetPos - Player.Center).SafeNormalize(Vector2.UnitX);
            
            for (int i = 0; i < 20; i++)
            {
                float progress = i / 20f;
                Vector2 dustPos = Vector2.Lerp(Player.Center, targetPos, progress);
                dustPos += Main.rand.NextVector2Circular(5f, 5f);
                
                Dust dust = Dust.NewDustPerfect(dustPos, DustID.GoldFlame, direction * 2f);
                dust.noGravity = true;
                dust.scale = 1.2f - progress * 0.5f;
            }
            
            // Burst at target
            for (int i = 0; i < 12; i++)
            {
                float angle = MathHelper.TwoPi * i / 12f;
                Dust dust = Dust.NewDustPerfect(targetPos, DustID.GoldFlame, angle.ToRotationVector2() * 5f);
                dust.noGravity = true;
                dust.scale = 1.5f;
            }
        }
        
        private void SpawnConductVFX()
        {
            if (ConductedTargetWhoAmI < 0 || ConductedTargetWhoAmI >= Main.maxNPCs)
                return;
            
            NPC target = Main.npc[ConductedTargetWhoAmI];
            if (!target.active) return;
            
            // Target indicator ring
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f + Main.GameUpdateCount * 0.05f;
                Vector2 dustPos = target.Center + angle.ToRotationVector2() * (target.width + 20f);
                
                Color dustColor = GetConductorThemeColor();
                Dust dust = Dust.NewDustPerfect(dustPos, DustID.GoldFlame, Vector2.Zero);
                dust.noGravity = true;
                dust.scale = 0.8f;
                dust.color = dustColor;
            }
        }
        
        private void SpawnFinaleChargeVFX()
        {
            float progress = (float)FinaleChargeTime / FinaleChargeRequired;
            int dustCount = 3 + (int)(progress * 8);
            
            for (int i = 0; i < dustCount; i++)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                float dist = 80f * (1f - progress);
                Vector2 dustPos = Player.Center + angle.ToRotationVector2() * dist;
                Vector2 dustVel = (Player.Center - dustPos).SafeNormalize(Vector2.Zero) * 3f;
                
                Dust dust = Dust.NewDustPerfect(dustPos, DustID.PurpleTorch, dustVel);
                dust.noGravity = true;
                dust.scale = 1f + progress;
                dust.color = FateCrimson;
            }
        }
        
        private void SpawnMinionSacrificeVFX(Vector2 position)
        {
            for (int i = 0; i < 10; i++)
            {
                float angle = MathHelper.TwoPi * i / 10f;
                Dust dust = Dust.NewDustPerfect(position, DustID.PurpleTorch, angle.ToRotationVector2() * 4f);
                dust.noGravity = true;
                dust.scale = 1.5f;
                dust.color = FateCrimson;
            }
        }
        
        private void SpawnFinaleImpactVFX(Vector2 position)
        {
            // Massive explosion
            for (int i = 0; i < 40; i++)
            {
                float angle = MathHelper.TwoPi * i / 40f;
                float speed = Main.rand.NextFloat(5f, 15f);
                Dust dust = Dust.NewDustPerfect(position, DustID.PurpleTorch, angle.ToRotationVector2() * speed);
                dust.noGravity = true;
                dust.scale = 2f;
                dust.color = Color.Lerp(FateCrimson, Color.White, Main.rand.NextFloat(0.3f));
            }
            
            // Screen shake
            Player.GetModPlayer<ScreenShakePlayer>()?.AddShake(15f, 20);
        }
        
        private void SpawnMinionExplosion(Vector2 position, int baseDamage)
        {
            // Small AoE explosion
            int explosionDamage = (int)(baseDamage * 0.5f);
            
            // VFX
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Dust dust = Dust.NewDustPerfect(position, DustID.Torch, angle.ToRotationVector2() * 4f);
                dust.noGravity = true;
                dust.scale = 1.2f;
                dust.color = InfernalOrange;
            }
            
            // Damage nearby enemies
            float explosionRadius = 80f;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly || npc.dontTakeDamage)
                    continue;
                
                if (Vector2.Distance(position, npc.Center) <= explosionRadius && npc.whoAmI != ConductedTargetWhoAmI)
                {
                    int dir = npc.Center.X > position.X ? 1 : -1;
                    Player.ApplyDamageToNPC(npc, explosionDamage, 3f, dir, crit: false);
                }
            }
        }
        
        private Color GetConductorThemeColor()
        {
            if (HasFatesCosmicDominion) return FateCrimson;
            if (HasSwansGracefulDirection) return SwanWhite;
            if (HasEnigmasHivemindLink) return EnigmaPurple;
            if (HasInfernalChoirMastersRod) return InfernalOrange;
            if (HasHeroicGeneralsBaton) return HeroicScarlet;
            if (HasMoonlitSymphonyWand) return MoonlitPurple;
            return ConductorGold;
        }
    }
    
    /// <summary>
    /// GlobalProjectile to handle minion behavior modifications
    /// </summary>
    public class ConductorMinionGlobalProjectile : GlobalProjectile
    {
        public override bool InstancePerEntity => true;
        
        public override void AI(Projectile projectile)
        {
            if (!projectile.minion || projectile.owner < 0 || projectile.owner >= Main.maxPlayers)
                return;
            
            Player owner = Main.player[projectile.owner];
            ConductorPlayer conductor = owner.GetModPlayer<ConductorPlayer>();
            
            if (conductor == null || !conductor.HasAnyConductorAccessory)
                return;
            
            // Handle Scatter behavior
            if (conductor.IsScattering)
            {
                // Find all nearby enemies and spread minions
                List<NPC> nearbyEnemies = new List<NPC>();
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (npc.active && !npc.friendly && !npc.dontTakeDamage && 
                        Vector2.Distance(owner.Center, npc.Center) < 600f)
                    {
                        nearbyEnemies.Add(npc);
                    }
                }
                
                if (nearbyEnemies.Count > 0)
                {
                    // Assign this minion to a random nearby enemy
                    NPC assignedTarget = nearbyEnemies[projectile.whoAmI % nearbyEnemies.Count];
                    Vector2 toTarget = (assignedTarget.Center - projectile.Center).SafeNormalize(Vector2.Zero);
                    projectile.velocity = Vector2.Lerp(projectile.velocity, toTarget * 12f, 0.1f);
                }
            }
            // Handle Conduct behavior - focus on single target
            else if (conductor.IsConducting && conductor.ConductedTargetWhoAmI >= 0)
            {
                NPC target = Main.npc[conductor.ConductedTargetWhoAmI];
                if (target.active && !target.friendly)
                {
                    Vector2 toTarget = (target.Center - projectile.Center).SafeNormalize(Vector2.Zero);
                    float speed = Math.Max(projectile.velocity.Length(), 10f);
                    projectile.velocity = Vector2.Lerp(projectile.velocity, toTarget * speed, 0.15f);
                }
            }
        }
        
        public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!projectile.minion || projectile.owner < 0 || projectile.owner >= Main.maxPlayers)
                return;
            
            Player owner = Main.player[projectile.owner];
            ConductorPlayer conductor = owner.GetModPlayer<ConductorPlayer>();
            
            if (conductor != null && conductor.HasAnyConductorAccessory)
            {
                conductor.OnMinionHitNPC(target, damageDone, hit.Crit, projectile);
            }
        }
        
        public override void ModifyHitNPC(Projectile projectile, NPC target, ref NPC.HitModifiers modifiers)
        {
            if (!projectile.minion || projectile.owner < 0 || projectile.owner >= Main.maxPlayers)
                return;
            
            Player owner = Main.player[projectile.owner];
            ConductorPlayer conductor = owner.GetModPlayer<ConductorPlayer>();
            
            if (conductor == null || !conductor.HasAnyConductorAccessory)
                return;
            
            // Apply damage bonus during conduct
            if (conductor.IsConducting && target.whoAmI == conductor.ConductedTargetWhoAmI)
            {
                float damageBonus = conductor.GetConductDamageBonus();
                modifiers.FinalDamage *= 1f + damageBonus;
            }
        }
        
        public override bool? CanHitNPC(Projectile projectile, NPC target)
        {
            // Allow minions to phase through blocks during Enigma conduct
            // This is handled via tile collision, not NPC collision
            return null;
        }
        
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
    }
}
