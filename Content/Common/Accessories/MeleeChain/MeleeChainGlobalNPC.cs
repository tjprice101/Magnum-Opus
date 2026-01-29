using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems;
using System;

namespace MagnumOpus.Content.Common.Accessories.MeleeChain
{
    /// <summary>
    /// Global NPC that handles melee chain accessory effects on enemies.
    /// Applies Scorched debuff, freeze effects, lifesteal, and special burst abilities.
    /// </summary>
    public class MeleeChainGlobalNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;
        
        /// <summary>Scorched debuff stacks (from Solar Crescendo Ring)</summary>
        public int scorchedStacks;
        
        /// <summary>Freeze timer (from Permafrost Cadence Seal)</summary>
        public int freezeTimer;
        
        /// <summary>Paradox debuff active (from Enigma's Dissonance)</summary>
        public bool hasParadox;
        public int paradoxDuration;
        
        private static readonly Color SummerOrange = new Color(255, 140, 0);
        private static readonly Color WinterBlue = new Color(150, 220, 255);
        private static readonly Color EnigmaPurple = new Color(140, 60, 200);
        private static readonly Color EnigmaGreen = new Color(50, 220, 100);
        
        public override void ResetEffects(NPC npc)
        {
            // Decay scorched stacks over time
            if (scorchedStacks > 0 && Main.rand.NextBool(120))
            {
                scorchedStacks--;
            }
            
            // Handle freeze timer
            if (freezeTimer > 0)
            {
                freezeTimer--;
                npc.velocity *= 0.1f; // Nearly frozen
                
                // Frost particles
                if (Main.rand.NextBool(8))
                {
                    Dust dust = Dust.NewDustDirect(npc.position, npc.width, npc.height, DustID.IceTorch, 0f, -1f, 100, default, 1f);
                    dust.noGravity = true;
                }
            }
            
            // Handle paradox duration
            if (paradoxDuration > 0)
            {
                paradoxDuration--;
                if (paradoxDuration <= 0)
                    hasParadox = false;
            }
        }
        
        public override void UpdateLifeRegen(NPC npc, ref int damage)
        {
            // Scorched DoT (stacking fire damage)
            if (scorchedStacks > 0)
            {
                // Each stack deals 4 damage per second
                int scorchedDamage = scorchedStacks * 4;
                npc.lifeRegen -= scorchedDamage * 2; // lifeRegen is in half-HP per second
                
                if (damage < scorchedStacks * 2)
                    damage = scorchedStacks * 2;
                
                // Fire particles
                if (Main.rand.NextBool(6))
                {
                    float intensity = Math.Min(scorchedStacks / 10f, 1f);
                    Color fireColor = Color.Lerp(SummerOrange, Color.Yellow, Main.rand.NextFloat() * intensity);
                    Dust dust = Dust.NewDustDirect(npc.position, npc.width, npc.height, DustID.Torch, 0f, -2f, 0, fireColor, 1.2f + intensity * 0.5f);
                    dust.noGravity = true;
                }
            }
            
            // Paradox DoT (from Enigma's Dissonance)
            if (hasParadox)
            {
                // 25 damage per second
                npc.lifeRegen -= 50;
                if (damage < 12)
                    damage = 12;
                
                // Enigma particles
                if (Main.rand.NextBool(8))
                {
                    Color paradoxColor = Main.rand.NextBool() ? EnigmaPurple : EnigmaGreen;
                    Dust dust = Dust.NewDustDirect(npc.position, npc.width, npc.height, DustID.Shadowflame, 
                        Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-2f, 2f), 0, paradoxColor, 1f);
                    dust.noGravity = true;
                }
            }
        }
        
        public override void OnHitByProjectile(NPC npc, Projectile projectile, NPC.HitInfo hit, int damageDone)
        {
            TryApplyMeleeChainEffects(npc, projectile.owner, hit.Crit, damageDone);
        }
        
        public override void OnHitByItem(NPC npc, Player player, Item item, NPC.HitInfo hit, int damageDone)
        {
            // Only melee weapons
            if (item.DamageType == DamageClass.Melee || item.DamageType == DamageClass.MeleeNoSpeed)
            {
                TryApplyMeleeChainEffects(npc, player.whoAmI, hit.Crit, damageDone);
            }
        }
        
        private void TryApplyMeleeChainEffects(NPC npc, int playerIndex, bool crit, int damageDone)
        {
            if (playerIndex < 0 || playerIndex >= Main.maxPlayers)
                return;
            
            Player player = Main.player[playerIndex];
            if (player == null || !player.active)
                return;
            
            var resonancePlayer = player.GetModPlayer<ResonanceComboPlayer>();
            if (!resonancePlayer.hasResonantRhythmBand)
                return;
            
            // Add resonance stacks
            resonancePlayer.OnMeleeHit(npc, crit);
            
            // Solar Crescendo Ring: Scorched debuff at 15+ stacks
            if (resonancePlayer.hasSolarCrescendoRing && resonancePlayer.resonanceStacks >= 15)
            {
                scorchedStacks = Math.Min(scorchedStacks + 1, 15); // Cap at 15 stacks (60 DPS)
                
                // Fire burst particle
                if (scorchedStacks == 1 || Main.rand.NextBool(3))
                {
                    CustomParticles.GenericFlare(npc.Center, SummerOrange, 0.3f, 12);
                }
            }
            
            // Harvest Rhythm Signet: 1% lifesteal at 20+ stacks
            if (resonancePlayer.hasHarvestRhythmSignet && resonancePlayer.resonanceStacks >= 20)
            {
                int healAmount = Math.Max(1, damageDone / 100); // 1% lifesteal
                player.Heal(healAmount);
                
                // Healing particle
                if (Main.rand.NextBool(4))
                {
                    Vector2 healPos = player.Center + Main.rand.NextVector2Circular(10f, 10f);
                    CustomParticles.GenericGlow(healPos, Vector2.UnitY * -1f, new Color(180, 100, 40) * 0.8f, 0.2f, 15, true);
                }
            }
            
            // Permafrost Cadence Seal: Freeze nearby enemies at 25+ stacks
            if (resonancePlayer.hasPermafrostCadenceSeal && resonancePlayer.resonanceStacks >= 25)
            {
                // Freeze the hit target briefly
                freezeTimer = Math.Max(freezeTimer, 30); // 0.5 seconds
                
                // Freeze particles
                for (int i = 0; i < 3; i++)
                {
                    Vector2 pos = npc.Center + Main.rand.NextVector2Circular(npc.width * 0.5f, npc.height * 0.5f);
                    CustomParticles.GenericFlare(pos, WinterBlue, 0.25f, 15);
                }
            }
            
            // Enigma's Dissonance: Paradox DoT at 45+ stacks
            if (resonancePlayer.hasEnigmasDissonance && resonancePlayer.resonanceStacks >= 45)
            {
                hasParadox = true;
                paradoxDuration = 180; // 3 seconds
                
                // Paradox particles
                if (!hasParadox || Main.rand.NextBool(4))
                {
                    CustomParticles.GlyphBurst(npc.Center, EnigmaPurple, 3, 2f);
                }
            }
        }
        
        /// <summary>
        /// Applies freeze effect to all enemies near the given position.
        /// Used by Permafrost burst abilities.
        /// </summary>
        public static void FreezeNearbyEnemies(Vector2 center, float radius, int duration)
        {
            foreach (NPC npc in Main.npc)
            {
                if (!npc.active || npc.friendly || npc.dontTakeDamage)
                    continue;
                
                if (Vector2.Distance(npc.Center, center) <= radius)
                {
                    var globalNPC = npc.GetGlobalNPC<MeleeChainGlobalNPC>();
                    globalNPC.freezeTimer = Math.Max(globalNPC.freezeTimer, duration);
                    
                    // Freeze burst
                    CustomParticles.GenericFlare(npc.Center, new Color(150, 220, 255), 0.4f, 15);
                }
            }
        }
    }
}
