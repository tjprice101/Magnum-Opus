using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems;

namespace MagnumOpus.Content.Common.Accessories.RangerChain
{
    /// <summary>
    /// Global NPC that handles the Marked for Death System effects on enemies.
    /// Manages mark state, visual indicators, damage bonuses, death explosions, and special effects.
    /// </summary>
    public class MarkingGlobalNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;
        
        /// <summary>Dictionary of player who am I -> mark duration remaining</summary>
        private Dictionary<int, int> markDurations = new Dictionary<int, int>();
        
        /// <summary>Whether this enemy has the Swan Mark (perfect shot bonus)</summary>
        public bool hasSwanMark;
        
        /// <summary>Whether this enemy is burning from Infernal Executioner's Brand</summary>
        public bool isBurning;
        public int burnDuration;
        
        /// <summary>Seasonal debuff type (0=spring, 1=summer, 2=autumn, 3=winter)</summary>
        public int seasonalDebuffType = -1;
        public int seasonalDebuffDuration;
        
        /// <summary>Whether the first hit auto-crit has been used on this enemy</summary>
        private Dictionary<int, bool> autoCritUsed = new Dictionary<int, bool>();
        
        // Colors
        private static readonly Color MarkBaseRed = new Color(255, 100, 100);
        private static readonly Color SummerOrange = new Color(255, 140, 0);
        private static readonly Color WinterBlue = new Color(150, 220, 255);
        private static readonly Color AutumnBrown = new Color(180, 100, 40);
        private static readonly Color SpringGreen = new Color(144, 238, 144);
        private static readonly Color CampanellaOrange = new Color(255, 140, 40);
        private static readonly Color EnigmaPurple = new Color(140, 60, 200);
        private static readonly Color SwanWhite = new Color(255, 255, 255);
        
        public override void ResetEffects(NPC npc)
        {
            // Decrement mark durations
            var keysToRemove = new List<int>();
            foreach (var kvp in markDurations)
            {
                markDurations[kvp.Key]--;
                if (markDurations[kvp.Key] <= 0)
                    keysToRemove.Add(kvp.Key);
            }
            foreach (var key in keysToRemove)
            {
                markDurations.Remove(key);
                autoCritUsed.Remove(key);
            }
            
            // Handle burn duration
            if (burnDuration > 0)
            {
                burnDuration--;
                if (burnDuration <= 0)
                    isBurning = false;
            }
            
            // Handle seasonal debuff duration
            if (seasonalDebuffDuration > 0)
            {
                seasonalDebuffDuration--;
                if (seasonalDebuffDuration <= 0)
                    seasonalDebuffType = -1;
            }
            
            // Swan Mark decays with the mark
            if (!IsMarkedByAny())
                hasSwanMark = false;
        }
        
        public override void AI(NPC npc)
        {
            // Apply slow effect if marked by a player with Permafrost Hunter's Eye
            if (IsMarkedByAny())
            {
                foreach (var kvp in markDurations)
                {
                    if (kvp.Value > 0)
                    {
                        Player player = Main.player[kvp.Key];
                        if (player.active)
                        {
                            var markingPlayer = player.GetModPlayer<MarkingPlayer>();
                            if (markingPlayer.markedSlowsEnemies)
                            {
                                npc.velocity *= (1f - markingPlayer.markSlowPercent);
                                
                                // Frost particles for slow
                                if (Main.rand.NextBool(20))
                                {
                                    Dust dust = Dust.NewDustDirect(npc.position, npc.width, npc.height, 
                                        DustID.IceTorch, 0f, -1f, 100, WinterBlue, 0.8f);
                                    dust.noGravity = true;
                                }
                            }
                        }
                    }
                }
            }
        }
        
        public override void DrawEffects(NPC npc, ref Color drawColor)
        {
            // Make marked enemies glow
            if (IsMarkedByAny())
            {
                // Get the marking player's color
                Color markColor = MarkBaseRed;
                foreach (var kvp in markDurations)
                {
                    if (kvp.Value > 0 && Main.player[kvp.Key].active)
                    {
                        var markingPlayer = Main.player[kvp.Key].GetModPlayer<MarkingPlayer>();
                        markColor = markingPlayer.GetMarkColor();
                        break;
                    }
                }
                
                // Subtle glow overlay
                float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.1f) * 0.1f + 0.9f;
                drawColor = Color.Lerp(drawColor, markColor, 0.25f * pulse);
                
                // Mark indicator particles
                if (Main.rand.NextBool(12))
                {
                    Vector2 pos = npc.Center + Main.rand.NextVector2Circular(npc.width * 0.5f, npc.height * 0.5f);
                    Vector2 vel = new Vector2(0, -Main.rand.NextFloat(0.5f, 1f));
                    CustomParticles.GenericGlow(pos, vel, markColor * 0.7f, 0.2f, 15, true);
                }
                
                // Swan Mark special indicator
                if (hasSwanMark && Main.rand.NextBool(10))
                {
                    Vector2 pos = npc.Top + new Vector2(Main.rand.NextFloat(-10f, 10f), -10f);
                    Vector2 vel = new Vector2(Main.rand.NextFloat(-0.3f, 0.3f), -Main.rand.NextFloat(0.3f, 0.6f));
                    CustomParticles.GenericGlow(pos, vel, SwanWhite, 0.25f, 18, true);
                }
            }
            
            // Burn visual
            if (isBurning && Main.rand.NextBool(6))
            {
                Vector2 pos = npc.position + new Vector2(Main.rand.Next(npc.width), Main.rand.Next(npc.height));
                Dust dust = Dust.NewDustDirect(pos, 0, 0, DustID.Torch, 0f, -2f, 0, CampanellaOrange, 1.2f);
                dust.noGravity = true;
            }
        }
        
        public override void UpdateLifeRegen(NPC npc, ref int damage)
        {
            // Burn DoT from Infernal Executioner's Brand
            if (isBurning)
            {
                // 15 damage per second
                npc.lifeRegen -= 30;
                if (damage < 8)
                    damage = 8;
            }
            
            // Seasonal debuffs
            if (seasonalDebuffType >= 0)
            {
                switch (seasonalDebuffType)
                {
                    case 1: // Summer - burn
                        npc.lifeRegen -= 20;
                        if (damage < 5)
                            damage = 5;
                        break;
                    case 2: // Autumn - wither (life drain)
                        npc.lifeRegen -= 16;
                        if (damage < 4)
                            damage = 4;
                        break;
                    case 3: // Winter - chill (slow + minor damage)
                        npc.lifeRegen -= 8;
                        if (damage < 2)
                            damage = 2;
                        npc.velocity *= 0.95f;
                        break;
                    // case 0: Spring - bloom (heals player on hit, no DoT)
                }
            }
        }
        
        public override void OnKill(NPC npc)
        {
            // Death explosion from Harvest Reaper's Mark
            if (IsMarkedByAny())
            {
                foreach (var kvp in markDurations)
                {
                    if (kvp.Value > 0)
                    {
                        Player player = Main.player[kvp.Key];
                        if (!player.active) continue;
                        
                        var markingPlayer = player.GetModPlayer<MarkingPlayer>();
                        
                        // Death explosion
                        if (markingPlayer.hasHarvestReapersMark)
                        {
                            float explosionRadius = 100f;
                            
                            // Infernal Executioner's Brand: +50% explosion radius
                            if (markingPlayer.hasInfernalExecutionersBrand)
                                explosionRadius *= 1.5f;
                            
                            // Calculate damage (50% of weapon damage, roughly)
                            int explosionDamage = (int)(player.GetTotalDamage(DamageClass.Ranged).ApplyTo(50));
                            
                            // Deal explosion damage to nearby enemies
                            for (int i = 0; i < Main.maxNPCs; i++)
                            {
                                NPC target = Main.npc[i];
                                if (target.active && !target.friendly && target.whoAmI != npc.whoAmI && 
                                    target.Distance(npc.Center) <= explosionRadius)
                                {
                                    target.SimpleStrikeNPC(explosionDamage, 0, false, 0, DamageClass.Ranged);
                                    
                                    // Chain marking: mark nearby enemies on death
                                    if (target.TryGetGlobalNPC<MarkingGlobalNPC>(out var targetMarkNPC))
                                    {
                                        targetMarkNPC.ApplyMark(player.whoAmI, markingPlayer.baseMarkDuration, markingPlayer);
                                    }
                                }
                            }
                            
                            // Explosion VFX
                            Color explosionColor = markingPlayer.GetMarkColor();
                            CustomParticles.GenericFlare(npc.Center, explosionColor, 0.8f, 20);
                            CustomParticles.HaloRing(npc.Center, explosionColor, 0.5f, 18);
                            CustomParticles.ExplosionBurst(npc.Center, explosionColor, 10, 8f);
                            
                            SoundEngine.PlaySound(SoundID.Item14, npc.Center);
                        }
                        
                        // Refresh marks on nearby enemies (Permafrost Hunter's Eye)
                        if (markingPlayer.hasPermafrostHuntersEye)
                        {
                            float refreshRadius = 200f;
                            for (int i = 0; i < Main.maxNPCs; i++)
                            {
                                NPC target = Main.npc[i];
                                if (target.active && !target.friendly && target.whoAmI != npc.whoAmI && 
                                    target.Distance(npc.Center) <= refreshRadius)
                                {
                                    if (target.TryGetGlobalNPC<MarkingGlobalNPC>(out var targetMarkNPC))
                                    {
                                        if (targetMarkNPC.IsMarkedBy(player.whoAmI))
                                        {
                                            // Refresh the mark duration
                                            targetMarkNPC.ApplyMark(player.whoAmI, markingPlayer.baseMarkDuration, markingPlayer);
                                            
                                            // Visual indicator
                                            CustomParticles.GenericFlare(target.Center, WinterBlue * 0.7f, 0.4f, 12);
                                        }
                                    }
                                }
                            }
                        }
                        
                        break; // Only process for first marking player
                    }
                }
            }
        }
        
        /// <summary>
        /// Checks if this NPC is marked by a specific player.
        /// </summary>
        public bool IsMarkedBy(int playerWhoAmI)
        {
            return markDurations.ContainsKey(playerWhoAmI) && markDurations[playerWhoAmI] > 0;
        }
        
        /// <summary>
        /// Checks if this NPC is marked by any player.
        /// </summary>
        public bool IsMarkedByAny()
        {
            foreach (var kvp in markDurations)
            {
                if (kvp.Value > 0)
                    return true;
            }
            return false;
        }
        
        /// <summary>
        /// Applies a mark to this NPC from the specified player.
        /// </summary>
        public void ApplyMark(int playerWhoAmI, int duration, MarkingPlayer markingPlayer)
        {
            // Check if we're at max marked enemies
            int currentMarked = markingPlayer.CountMarkedEnemies();
            if (!IsMarkedBy(playerWhoAmI) && currentMarked >= markingPlayer.maxMarkedEnemies)
            {
                // At max, don't apply new mark
                return;
            }
            
            // Apply or refresh mark
            markDurations[playerWhoAmI] = duration;
            
            // Apply seasonal debuff if Vivaldi's Seasonal Sight is equipped
            if (markingPlayer.hasVivaldisSeSonalSight)
            {
                seasonalDebuffType = markingPlayer.GetCurrentSeasonalDebuffType();
                seasonalDebuffDuration = duration;
            }
            
            // Apply burn if Infernal Executioner's Brand is equipped
            if (markingPlayer.hasInfernalExecutionersBrand)
            {
                isBurning = true;
                burnDuration = duration;
            }
            
            // Apply Swan Mark if it's a perfect shot
            if (markingPlayer.hasSwansGracefulHunt && markingPlayer.IsPerfectShot)
            {
                hasSwanMark = true;
            }
            
            // Mark spread chance from Enigma's Paradox Mark
            if (markingPlayer.hasEnigmasParadoxMark && Main.rand.NextFloat() < 0.15f)
            {
                // Find a nearby unmarked enemy and mark it too
                float spreadRadius = 150f;
                NPC currentNPC = null;
                
                // Find our NPC
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    if (Main.npc[i].TryGetGlobalNPC<MarkingGlobalNPC>(out var gnpc) && gnpc == this)
                    {
                        currentNPC = Main.npc[i];
                        break;
                    }
                }
                
                if (currentNPC != null)
                {
                    for (int i = 0; i < Main.maxNPCs; i++)
                    {
                        NPC target = Main.npc[i];
                        if (target.active && !target.friendly && target.whoAmI != currentNPC.whoAmI &&
                            target.Distance(currentNPC.Center) <= spreadRadius)
                        {
                            if (target.TryGetGlobalNPC<MarkingGlobalNPC>(out var targetMarkNPC))
                            {
                                if (!targetMarkNPC.IsMarkedBy(playerWhoAmI))
                                {
                                    // Spread mark (without causing another spread)
                                    targetMarkNPC.markDurations[playerWhoAmI] = duration;
                                    
                                    // Visual spread effect
                                    CustomParticles.GenericFlare(target.Center, EnigmaPurple, 0.5f, 15);
                                    CustomParticles.GlyphBurst(target.Center, EnigmaPurple, 4, 3f);
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }
        
        /// <summary>
        /// Checks if auto-crit has been used on this enemy by the specified player.
        /// </summary>
        public bool HasUsedAutoCrit(int playerWhoAmI)
        {
            return autoCritUsed.ContainsKey(playerWhoAmI) && autoCritUsed[playerWhoAmI];
        }
        
        /// <summary>
        /// Marks that auto-crit has been used on this enemy by the specified player.
        /// </summary>
        public void UseAutoCrit(int playerWhoAmI)
        {
            autoCritUsed[playerWhoAmI] = true;
        }
        
        /// <summary>
        /// Gets the remaining mark duration for a specific player.
        /// </summary>
        public int GetMarkDuration(int playerWhoAmI)
        {
            return markDurations.ContainsKey(playerWhoAmI) ? markDurations[playerWhoAmI] : 0;
        }
    }
    
    /// <summary>
    /// Global Projectile that handles applying marks and mark-related effects on hit.
    /// </summary>
    public class MarkingGlobalProjectile : GlobalProjectile
    {
        public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Only process for ranged projectiles from players
            if (!projectile.friendly || projectile.hostile || projectile.owner < 0 || projectile.owner >= Main.maxPlayers)
                return;
            
            if (projectile.DamageType != DamageClass.Ranged && !projectile.DamageType.CountsAsClass(DamageClass.Ranged))
                return;
            
            Player player = Main.player[projectile.owner];
            if (!player.active) return;
            
            var markingPlayer = player.GetModPlayer<MarkingPlayer>();
            
            // If no marking accessory, skip
            if (!markingPlayer.hasResonantSpotter)
                return;
            
            // Apply mark
            if (target.TryGetGlobalNPC<MarkingGlobalNPC>(out var markNPC))
            {
                bool wasAlreadyMarked = markNPC.IsMarkedBy(player.whoAmI);
                
                markNPC.ApplyMark(player.whoAmI, markingPlayer.baseMarkDuration, markingPlayer);
                
                // Spring Hunter's Lens: 10% heart drop on hitting marked enemies
                if (markingPlayer.hasSpringHuntersLens && wasAlreadyMarked && Main.rand.NextFloat() < 0.10f)
                {
                    Item.NewItem(projectile.GetSource_OnHit(target), target.Center, ItemID.Heart);
                    
                    // Visual feedback
                    CustomParticles.GenericFlare(target.Center, MarkingPlayer.SpringGreen, 0.5f, 15);
                }
                
                // Mark application VFX for newly marked enemies
                if (!wasAlreadyMarked)
                {
                    Color markColor = markingPlayer.GetMarkColor();
                    CustomParticles.GenericFlare(target.Center, markColor, 0.6f, 18);
                    CustomParticles.HaloRing(target.Center, markColor * 0.6f, 0.3f, 12);
                }
            }
        }
        
        public override void ModifyHitNPC(Projectile projectile, NPC target, ref NPC.HitModifiers modifiers)
        {
            // Only process for ranged projectiles from players
            if (!projectile.friendly || projectile.hostile || projectile.owner < 0 || projectile.owner >= Main.maxPlayers)
                return;
            
            if (projectile.DamageType != DamageClass.Ranged && !projectile.DamageType.CountsAsClass(DamageClass.Ranged))
                return;
            
            Player player = Main.player[projectile.owner];
            if (!player.active) return;
            
            var markingPlayer = player.GetModPlayer<MarkingPlayer>();
            
            // If no marking accessory, skip
            if (!markingPlayer.hasResonantSpotter)
                return;
            
            // Check if enemy is marked
            if (target.TryGetGlobalNPC<MarkingGlobalNPC>(out var markNPC))
            {
                if (markNPC.IsMarkedBy(player.whoAmI))
                {
                    // Apply damage bonus from marking
                    if (markingPlayer.markedDamageBonus > 0)
                    {
                        modifiers.FinalDamage *= (1f + markingPlayer.markedDamageBonus);
                    }
                    
                    // Swan Mark: +15% crit chance
                    if (markNPC.hasSwanMark)
                    {
                        modifiers.CritDamage += 0.15f;
                    }
                    
                    // Heroic Deadeye: First shot auto-crit
                    if (markingPlayer.hasHeroicDeadeye && !markNPC.HasUsedAutoCrit(player.whoAmI))
                    {
                        modifiers.SetCrit();
                        markNPC.UseAutoCrit(player.whoAmI);
                        
                        // Auto-crit VFX
                        CustomParticles.GenericFlare(target.Center, MarkingPlayer.EroicaGold, 0.7f, 20);
                    }
                }
            }
        }
    }
}
