using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Content.Nachtmusik.Debuffs
{
    /// <summary>
    /// CelestialHarmony - The signature debuff of Nachtmusik weapons.
    /// 
    /// Effects:
    /// - Celestial damage over time that scales with stacks (stronger than Fate)
    /// - "Radiant Chains" slow enemy movement significantly
    /// - Enemies take increased damage from all sources per stack
    /// - At max stacks: "Celestial Crescendo" massive explosion damages all nearby enemies and spreads debuff
    /// </summary>
    public class CelestialHarmony : ModBuff
    {
        public const int MaxStacks = 10;
        public const int BaseDamagePerSecond = 35;    // Higher than Fate's 20
        public const int DamagePerStack = 18;          // Higher than Fate's 12
        public const float SlowPerStack = 0.06f;       // 6% slow per stack
        public const float DamageAmpPerStack = 0.04f;  // 4% damage amp per stack
        public const float CrescendoRadius = 350f;
        
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = true;
            BuffID.Sets.NurseCannotRemoveDebuff[Type] = true;
        }
        
        public override void Update(NPC npc, ref int buffIndex)
        {
            npc.GetGlobalNPC<CelestialHarmonyNPC>().HasCelestialHarmony = true;
        }
    }
    
    public class CelestialHarmonyNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;
        
        public bool HasCelestialHarmony;
        public int HarmonyStacks = 1;
        public int HarmonyTimer;
        public int CrescendoChargeTime;
        public bool IsCrescendoActive;
        
        // Celestial Echo system - delayed bonus damage flares
        private readonly List<CelestialEcho> pendingEchos = new List<CelestialEcho>();
        
        private struct CelestialEcho
        {
            public Vector2 Position;
            public int Damage;
            public int Timer;
            public Color EchoColor;
        }
        
        public override void ResetEffects(NPC npc)
        {
            if (!HasCelestialHarmony)
            {
                HarmonyStacks = 1;
                HarmonyTimer = 0;
                CrescendoChargeTime = 0;
                IsCrescendoActive = false;
            }
            HasCelestialHarmony = false;
        }
        
        public override void UpdateLifeRegen(NPC npc, ref int damage)
        {
            if (!HasCelestialHarmony) return;
            
            // Calculate DoT based on stacks - stronger than Fate
            int totalDamage = CelestialHarmony.BaseDamagePerSecond + 
                              (HarmonyStacks - 1) * CelestialHarmony.DamagePerStack;
            
            // Life regen is in units of 1/2 per second, so multiply by 2
            npc.lifeRegen -= totalDamage * 2;
            
            // Set minimum damage display
            if (damage < totalDamage / 4)
                damage = totalDamage / 4;
        }
        
        public override void ModifyIncomingHit(NPC npc, ref NPC.HitModifiers modifiers)
        {
            if (!HasCelestialHarmony) return;
            
            // Damage amplification per stack
            float damageAmp = 1f + HarmonyStacks * CelestialHarmony.DamageAmpPerStack;
            modifiers.FinalDamage *= damageAmp;
        }
        
        public override void PostAI(NPC npc)
        {
            if (!HasCelestialHarmony) return;
            
            HarmonyTimer++;
            
            // === SLOW EFFECT - Radiant Chains ===
            float slowAmount = HarmonyStacks * CelestialHarmony.SlowPerStack;
            npc.velocity *= (1f - slowAmount);
            
            // === CELESTIAL VFX ===
            SpawnHarmonyParticles(npc);
            
            // === STACK MECHANICS ===
            // Check for max stacks - trigger Celestial Crescendo
            if (HarmonyStacks >= CelestialHarmony.MaxStacks && !IsCrescendoActive)
            {
                CrescendoChargeTime++;
                
                // Charge-up VFX
                if (CrescendoChargeTime % 5 == 0)
                {
                    float chargeProgress = CrescendoChargeTime / 90f;
                    NachtmusikCosmicVFX.SpawnConstellationCircle(npc.Center, 
                        40f + chargeProgress * 30f, 
                        6, 
                        Main.GameUpdateCount * 0.03f);
                }
                
                // Trigger crescendo after charge
                if (CrescendoChargeTime >= 90)
                {
                    TriggerCelestialCrescendo(npc);
                }
            }
            
            // === PROCESS ECHOES ===
            ProcessCelestialEchos(npc);
        }
        
        /// <summary>
        /// Adds a stack to this enemy. Called when hit by Nachtmusik weapons.
        /// </summary>
        public void AddStack(NPC npc, int amount = 1)
        {
            int oldStacks = HarmonyStacks;
            HarmonyStacks = Math.Min(HarmonyStacks + amount, CelestialHarmony.MaxStacks);
            
            // Stack gain VFX
            if (HarmonyStacks > oldStacks)
            {
                // Stack gain particle burst
                for (int i = 0; i < 4; i++)
                {
                    float angle = MathHelper.TwoPi * i / 4f + Main.rand.NextFloat(-0.3f, 0.3f);
                    Vector2 vel = angle.ToRotationVector2() * 4f;
                    Color stackColor = NachtmusikCosmicVFX.GetCelestialGradient((float)HarmonyStacks / CelestialHarmony.MaxStacks);
                    var spark = new GlowSparkParticle(npc.Center, vel, stackColor, 0.25f, 12);
                    MagnumParticleHandler.SpawnParticle(spark);
                }
                
                // Near max stacks warning
                if (HarmonyStacks >= CelestialHarmony.MaxStacks - 2)
                {
                    CustomParticles.GenericFlare(npc.Center, NachtmusikCosmicVFX.Gold, 0.6f, 15);
                    SoundEngine.PlaySound(SoundID.Item29 with { Pitch = 0.4f + HarmonyStacks * 0.05f, Volume = 0.5f }, npc.Center);
                }
            }
        }
        
        /// <summary>
        /// Schedules a celestial echo (delayed damage flare).
        /// </summary>
        public void ScheduleEcho(Vector2 position, int damage, int delay, Color color)
        {
            pendingEchos.Add(new CelestialEcho
            {
                Position = position,
                Damage = damage,
                Timer = delay,
                EchoColor = color
            });
        }
        
        private void ProcessCelestialEchos(NPC npc)
        {
            for (int i = pendingEchos.Count - 1; i >= 0; i--)
            {
                var echo = pendingEchos[i];
                echo.Timer--;
                pendingEchos[i] = echo;
                
                if (echo.Timer <= 0)
                {
                    // Apply echo damage
                    if (npc.active && !npc.dontTakeDamage)
                    {
                        npc.SimpleStrikeNPC(echo.Damage, 0, false, 0f, null, false, 0, false);
                    }
                    
                    // Echo explosion VFX
                    CustomParticles.GenericFlare(echo.Position, echo.EchoColor, 0.6f, 15);
                    CustomParticles.GenericFlare(echo.Position, NachtmusikCosmicVFX.StarWhite, 0.4f, 12);
                    var echoBurst = new StarBurstParticle(echo.Position, Vector2.Zero, echo.EchoColor * 0.7f, 0.3f, 14);
                    MagnumParticleHandler.SpawnParticle(echoBurst);
                    
                    // Star sparkles
                    for (int j = 0; j < 6; j++)
                    {
                        Vector2 offset = Main.rand.NextVector2Circular(15f, 15f);
                        var star = new GenericGlowParticle(echo.Position + offset, Main.rand.NextVector2Circular(2f, 2f),
                            NachtmusikCosmicVFX.StarWhite, 0.2f, 15, true);
                        MagnumParticleHandler.SpawnParticle(star);
                    }
                    
                    pendingEchos.RemoveAt(i);
                }
            }
        }
        
        private void TriggerCelestialCrescendo(NPC npc)
        {
            IsCrescendoActive = true;
            
            // === MASSIVE CELESTIAL EXPLOSION ===
            NachtmusikCosmicVFX.SpawnCelestialExplosion(npc.Center, 2f);
            
            // Additional VFX layers - starburst cascade
            for (int ring = 0; ring < 10; ring++)
            {
                float progress = ring / 10f;
                Color burstColor = NachtmusikCosmicVFX.GetCelestialGradient(progress);
                var crescendoBurst = new StarBurstParticle(npc.Center, Vector2.Zero, burstColor, 0.45f + ring * 0.15f, 20 + ring * 3, ring % 2);
                MagnumParticleHandler.SpawnParticle(crescendoBurst);
                
                // Shattered starlight fragments
                float fragAngle = MathHelper.TwoPi * ring / 10f;
                Vector2 fragVel = fragAngle.ToRotationVector2() * (8f + ring);
                var fragment = new ShatteredStarlightParticle(npc.Center, fragVel, burstColor, 0.35f, 25, true, 0.08f);
                MagnumParticleHandler.SpawnParticle(fragment);
            }
            
            // Lightning strikes in radius
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f;
                Vector2 strikePos = npc.Center + angle.ToRotationVector2() * Main.rand.NextFloat(60f, 120f);
                NachtmusikCosmicVFX.SpawnCelestialLightningStrike(strikePos, 0.8f);
            }
            
            // Screen effects
            MagnumScreenEffects.AddScreenShake(20f);
            
            // Sound
            SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.3f, Volume = 1.2f }, npc.Center);
            SoundEngine.PlaySound(SoundID.Item92 with { Pitch = -0.2f, Volume = 0.8f }, npc.Center);
            
            // === SPREAD TO NEARBY ENEMIES ===
            int explosionDamage = 500 + HarmonyStacks * 100; // Massive damage
            
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC other = Main.npc[i];
                if (other.active && other.CanBeChasedBy() && other.whoAmI != npc.whoAmI)
                {
                    float dist = Vector2.Distance(other.Center, npc.Center);
                    if (dist < CelestialHarmony.CrescendoRadius)
                    {
                        // Apply debuff with starting stacks based on proximity
                        int startStacks = (int)MathHelper.Lerp(5, 1, dist / CelestialHarmony.CrescendoRadius);
                        other.AddBuff(ModContent.BuffType<CelestialHarmony>(), 600);
                        var otherHarmony = other.GetGlobalNPC<CelestialHarmonyNPC>();
                        otherHarmony.HarmonyStacks = Math.Max(otherHarmony.HarmonyStacks, startStacks);
                        
                        // Damage based on distance
                        float damageMult = 1f - (dist / CelestialHarmony.CrescendoRadius) * 0.5f;
                        int damage = (int)(explosionDamage * damageMult);
                        other.SimpleStrikeNPC(damage, 0, false, 0f, null, false, 0, false);
                        
                        // Connecting beam VFX
                        Vector2 direction = (other.Center - npc.Center).SafeNormalize(Vector2.UnitX);
                        for (int j = 0; j < 8; j++)
                        {
                            float lerp = j / 8f;
                            Vector2 beamPos = Vector2.Lerp(npc.Center, other.Center, lerp);
                            Color beamColor = NachtmusikCosmicVFX.GetCelestialGradient(lerp);
                            var beam = new GenericGlowParticle(beamPos, direction * 2f, beamColor, 0.3f, 15, true);
                            MagnumParticleHandler.SpawnParticle(beam);
                        }
                    }
                }
            }
            
            // Reset this enemy's stacks after explosion
            HarmonyStacks = 1;
            CrescendoChargeTime = 0;
            IsCrescendoActive = false;
        }
        
        private void SpawnHarmonyParticles(NPC npc)
        {
            float intensity = (float)HarmonyStacks / CelestialHarmony.MaxStacks;
            
            // Ambient celestial particles - more frequent at higher stacks
            if (Main.rand.NextBool((int)MathHelper.Lerp(12, 3, intensity)))
            {
                Vector2 offset = Main.rand.NextVector2Circular(npc.width * 0.6f, npc.height * 0.6f);
                Color particleColor = NachtmusikCosmicVFX.GetCelestialGradient(Main.rand.NextFloat());
                var particle = new GenericGlowParticle(npc.Center + offset, new Vector2(0, -1.5f), 
                    particleColor * 0.6f, 0.2f + intensity * 0.15f, 20, true);
                MagnumParticleHandler.SpawnParticle(particle);
            }
            
            // Star sparkles at higher stacks
            if (HarmonyStacks >= 5 && Main.rand.NextBool(8))
            {
                Vector2 starOffset = Main.rand.NextVector2Circular(npc.width * 0.5f, npc.height * 0.5f);
                CustomParticles.GenericFlare(npc.Center + starOffset, NachtmusikCosmicVFX.StarWhite, 0.25f, 10);
            }
            
            // Glyph accents at high stacks
            if (HarmonyStacks >= 7 && Main.rand.NextBool(15))
            {
                CustomParticles.Glyph(npc.Center + Main.rand.NextVector2Circular(20f, 20f), 
                    NachtmusikCosmicVFX.Violet, 0.3f, -1);
            }
            
            // Orbiting constellation at max stacks during charge
            if (IsCrescendoActive || CrescendoChargeTime > 0)
            {
                float orbitAngle = Main.GameUpdateCount * 0.05f;
                for (int i = 0; i < 4; i++)
                {
                    float angle = orbitAngle + MathHelper.TwoPi * i / 4f;
                    Vector2 orbitPos = npc.Center + angle.ToRotationVector2() * 35f;
                    var orbit = new GenericGlowParticle(orbitPos, Vector2.Zero, 
                        NachtmusikCosmicVFX.Gold, 0.2f, 5, true);
                    MagnumParticleHandler.SpawnParticle(orbit);
                }
            }
            
            // Dynamic lighting based on stacks
            Vector3 lightColor = Vector3.Lerp(
                NachtmusikCosmicVFX.DeepPurple.ToVector3(),
                NachtmusikCosmicVFX.Gold.ToVector3(),
                intensity
            );
            Lighting.AddLight(npc.Center, lightColor * (0.3f + intensity * 0.4f));
        }
    }
}
