using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Content.LaCampanella.Debuffs
{
    /// <summary>
    /// Resonant Toll - A stacking debuff from La Campanella weapons.
    /// Effects:
    /// - Deals damage over time scaling with stacks
    /// - Reduces defense per stack
    /// - Periodic bell chime explosions
    /// - At max stacks (10): Brief stun + AoE flame burst that spreads to nearby enemies
    /// </summary>
    public class ResonantToll : ModBuff
    {
        // Use the Dual-Fated Chime as the buff icon base (will show as a bell/chime icon)
        public override string Texture => "MagnumOpus/Content/LaCampanella/ResonantWeapons/DualFatedChime";
        
        public const int MaxStacks = 10;
        public const int StackDuration = 300; // 5 seconds per stack refresh
        
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = true;
            BuffID.Sets.NurseCannotRemoveDebuff[Type] = true;
        }

        public override void Update(NPC npc, ref int buffIndex)
        {
            npc.GetGlobalNPC<ResonantTollNPC>().HasResonantToll = true;
        }
    }

    /// <summary>
    /// Handles the Resonant Toll debuff effects on NPCs.
    /// Creates bell-themed flame visuals with musical particle effects.
    /// </summary>
    public class ResonantTollNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;

        public bool HasResonantToll { get; set; } = false;
        public int Stacks { get; set; } = 0;
        public int StackRefreshTimer { get; set; } = 0;
        
        private int bellChimeTimer = 0;
        private int damageTickTimer = 0;
        private float flameAnimTimer = 0f;
        private bool wasMaxStacks = false;
        private int stunTimer = 0;

        public override void ResetEffects(NPC npc)
        {
            if (!HasResonantToll && Stacks > 0)
            {
                // Debuff expired, decay stacks
                StackRefreshTimer--;
                if (StackRefreshTimer <= 0)
                {
                    Stacks = 0;
                }
            }
            HasResonantToll = false;
        }

        /// <summary>
        /// Add stacks of Resonant Toll to this NPC.
        /// </summary>
        public void AddStacks(NPC npc, int amount = 1)
        {
            int oldStacks = Stacks;
            Stacks = Math.Min(Stacks + amount, ResonantToll.MaxStacks);
            StackRefreshTimer = ResonantToll.StackDuration;
            
            // Apply the debuff
            npc.AddBuff(ModContent.BuffType<ResonantToll>(), ResonantToll.StackDuration);
            
            // Visual feedback for gaining stacks
            SpawnStackGainEffect(npc, Stacks);
            
            // Check for max stack trigger
            if (Stacks >= ResonantToll.MaxStacks && oldStacks < ResonantToll.MaxStacks)
            {
                TriggerMaxStackEffect(npc);
            }
        }

        private void SpawnStackGainEffect(NPC npc, int currentStacks)
        {
            // Bell chime sound on stack gain
            if (Main.rand.NextBool(2))
            {
                SoundEngine.PlaySound(SoundID.Item35 with { Pitch = 0.3f + currentStacks * 0.08f, Volume = 0.4f }, npc.Center);
            }
            
            // BLACK → ORANGE gradient flame burst
            float intensity = currentStacks / (float)ResonantToll.MaxStacks;
            for (int i = 0; i < 3 + currentStacks; i++)
            {
                Vector2 velocity = Main.rand.NextVector2Circular(3f, 3f);
                float progress = (float)i / (3 + currentStacks);
                Color flameColor = Color.Lerp(ThemedParticles.CampanellaBlack, ThemedParticles.CampanellaOrange, progress);
                
                Dust flame = Dust.NewDustPerfect(npc.Center + Main.rand.NextVector2Circular(npc.width * 0.3f, npc.height * 0.3f),
                    DustID.Torch, velocity, 100, flameColor, 1.5f + intensity);
                flame.noGravity = true;
            }
            
            // === GLYPH STACK INDICATOR ===
            if (CustomParticleSystem.TexturesLoaded && currentStacks >= 3)
            {
                for (int i = 0; i < currentStacks / 3; i++)
                {
                    float glyphAngle = Main.GameUpdateCount * 0.04f + MathHelper.TwoPi * i / (currentStacks / 3f);
                    Vector2 glyphPos = npc.Center + glyphAngle.ToRotationVector2() * 25f;
                    Color glyphColor = Color.Lerp(ThemedParticles.CampanellaBlack, ThemedParticles.CampanellaOrange, (float)i / Math.Max(1, currentStacks / 3)) * 0.6f;
                    CustomParticles.Glyph(glyphPos, glyphColor, 0.22f, -1);
                }
            }
            
            // Music note particle
            if (CustomParticleSystem.TexturesLoaded)
            {
                ThemedParticles.LaCampanellaMusicNotes(npc.Center, 1 + currentStacks / 3, 20f);
            }
        }

        private void TriggerMaxStackEffect(NPC npc)
        {
            wasMaxStacks = true;
            stunTimer = 60; // 1 second stun
            
            // Play loud bell gong sound
            SoundEngine.PlaySound(SoundID.Item122 with { Pitch = -0.3f, Volume = 0.8f }, npc.Center);
            SoundEngine.PlaySound(SoundID.Item45 with { Pitch = 0.5f, Volume = 0.6f }, npc.Center);
            
            // Massive bell flame explosion
            SpawnBellFlameExplosion(npc);
            
            // Spread to nearby enemies
            SpreadToNearbyEnemies(npc);
            
            // Reset stacks after max effect
            Stacks = 5; // Keep half stacks after max proc
        }

        private void SpawnBellFlameExplosion(NPC npc)
        {
            // Large bell-flame burst with BLACK → ORANGE gradient
            for (int i = 0; i < 30; i++)
            {
                float angle = MathHelper.TwoPi * i / 30f;
                Vector2 velocity = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 8f);
                
                float progress = (float)i / 30f;
                Color color = Color.Lerp(ThemedParticles.CampanellaBlack, ThemedParticles.CampanellaOrange, progress);
                
                Dust flame = Dust.NewDustPerfect(npc.Center, DustID.Torch, velocity, 80, color, 2.5f);
                flame.noGravity = true;
            }
            
            // === GLYPH EXPLOSION - MAX STACK TRIGGER ===
            if (CustomParticleSystem.TexturesLoaded)
            {
                for (int i = 0; i < 8; i++)
                {
                    float glyphAngle = MathHelper.TwoPi * i / 8f;
                    Vector2 glyphPos = npc.Center + glyphAngle.ToRotationVector2() * 45f;
                    Color glyphColor = Color.Lerp(ThemedParticles.CampanellaBlack, ThemedParticles.CampanellaOrange, (float)i / 8f) * 0.7f;
                    CustomParticles.Glyph(glyphPos, glyphColor, 0.35f, -1);
                }
                
                // Black → Orange gradient halo rings
                for (int ring = 0; ring < 4; ring++)
                {
                    float ringProgress = (float)ring / 4f;
                    Color ringColor = Color.Lerp(ThemedParticles.CampanellaBlack, ThemedParticles.CampanellaOrange, ringProgress);
                    CustomParticles.HaloRing(npc.Center, ringColor, 0.5f + ring * 0.2f, 20 + ring * 4);
                }
            }
            
            // Shockwave ring effect
            if (CustomParticleSystem.TexturesLoaded)
            {
                ThemedParticles.LaCampanellaShockwave(npc.Center, 1.5f);
                ThemedParticles.LaCampanellaImpact(npc.Center, 2f);
            }
            
            // Screen shake removed - weapon debuff effects should not cause screen shake
            
            // Lighting burst
            Lighting.AddLight(npc.Center, 2f, 1.2f, 0.4f);
        }

        private void SpreadToNearbyEnemies(NPC npc)
        {
            float spreadRadius = 300f;
            
            foreach (NPC target in Main.npc)
            {
                if (target.active && !target.friendly && target.whoAmI != npc.whoAmI && target.CanBeChasedBy())
                {
                    float distance = Vector2.Distance(npc.Center, target.Center);
                    if (distance <= spreadRadius)
                    {
                        // Apply 3 stacks to nearby enemies
                        target.GetGlobalNPC<ResonantTollNPC>().AddStacks(target, 3);
                        
                        // Visual connection line
                        SpawnSpreadLine(npc.Center, target.Center);
                    }
                }
            }
        }

        private void SpawnSpreadLine(Vector2 start, Vector2 end)
        {
            Vector2 direction = (end - start).SafeNormalize(Vector2.UnitX);
            float distance = Vector2.Distance(start, end);
            
            int steps = (int)(distance / 15f);
            for (int i = 0; i < steps; i++)
            {
                float d = i * 15f;
                Vector2 pos = start + direction * d;
                float progress = d / distance;
                Color color = Color.Lerp(ThemedParticles.CampanellaBlack, ThemedParticles.CampanellaOrange, progress);
                
                Dust line = Dust.NewDustPerfect(pos, DustID.Torch, Main.rand.NextVector2Circular(0.5f, 0.5f), 100, color, 1.2f);
                line.noGravity = true;
            }
        }

        public override void ModifyIncomingHit(NPC npc, ref NPC.HitModifiers modifiers)
        {
            // Stun effect reduces knockback resistance
            if (stunTimer > 0)
            {
                modifiers.Knockback *= 1.5f;
            }
        }

        public override void UpdateLifeRegen(NPC npc, ref int damage)
        {
            if (!HasResonantToll || Stacks <= 0) return;
            
            // Stop natural regen
            if (npc.lifeRegen > 0)
                npc.lifeRegen = 0;
            
            // DoT scales with stacks: base 5 + 3 per stack
            int dotDamage = 5 + (Stacks * 3);
            npc.lifeRegen -= dotDamage * 2; // lifeRegen is doubled for actual damage
            
            if (damage < dotDamage)
                damage = dotDamage;
        }

        public override void DrawEffects(NPC npc, ref Color drawColor)
        {
            if (!HasResonantToll || Stacks <= 0) return;
            
            // Darken sprite slightly with orange tint
            float intensity = Stacks / (float)ResonantToll.MaxStacks;
            drawColor = Color.Lerp(drawColor, new Color(255, 150, 100), intensity * 0.3f);
        }

        public override void AI(NPC npc)
        {
            if (!HasResonantToll || Stacks <= 0) return;

            flameAnimTimer += 0.1f;
            bellChimeTimer++;
            damageTickTimer++;
            
            // Stun effect - slow enemy
            if (stunTimer > 0)
            {
                stunTimer--;
                npc.velocity *= 0.85f;
            }
            
            // Defense reduction per stack (handled in ModifyHitByProjectile/Item)
            
            // Periodic bell chime explosions (every 2 seconds, scaled by stacks)
            int chimeInterval = Math.Max(60, 180 - Stacks * 12);
            if (bellChimeTimer >= chimeInterval)
            {
                bellChimeTimer = 0;
                SpawnBellChimeExplosion(npc);
            }
            
            // Ambient flame particles
            SpawnAmbientFlames(npc);
            
            // Stack indicator particles
            if (Main.rand.NextBool(20 - Stacks))
            {
                SpawnStackIndicator(npc);
            }
        }

        private void SpawnBellChimeExplosion(NPC npc)
        {
            // Small bell chime burst
            SoundEngine.PlaySound(SoundID.Item35 with { Pitch = 0.5f + Stacks * 0.05f, Volume = 0.3f }, npc.Center);
            
            int particleCount = 5 + Stacks;
            for (int i = 0; i < particleCount; i++)
            {
                float angle = MathHelper.TwoPi * i / particleCount;
                Vector2 velocity = angle.ToRotationVector2() * (2f + Stacks * 0.3f);
                
                // BLACK → ORANGE gradient
                float progress = (float)i / particleCount;
                Color color = Color.Lerp(ThemedParticles.CampanellaBlack, ThemedParticles.CampanellaOrange, progress);
                Dust chime = Dust.NewDustPerfect(npc.Center, DustID.Torch, velocity, 80, color, 1.5f);
                chime.noGravity = true;
            }
            
            // Small AoE damage
            float aoeRadius = 80f + Stacks * 10f;
            int aoeDamage = 10 + Stacks * 5;
            
            foreach (NPC target in Main.npc)
            {
                if (target.active && !target.friendly && target.whoAmI != npc.whoAmI && target.CanBeChasedBy())
                {
                    if (Vector2.Distance(npc.Center, target.Center) <= aoeRadius)
                    {
                        // Apply small damage and 1 stack
                        target.SimpleStrikeNPC(aoeDamage, 0, false, 0f, null, false, 0f, true);
                        target.GetGlobalNPC<ResonantTollNPC>().AddStacks(target, 1);
                    }
                }
            }
            
            Lighting.AddLight(npc.Center, 0.8f, 0.5f, 0.2f);
        }

        private void SpawnAmbientFlames(NPC npc)
        {
            float intensity = Stacks / (float)ResonantToll.MaxStacks;
            
            if (Main.rand.NextFloat() < 0.15f + intensity * 0.3f)
            {
                Vector2 offset = Main.rand.NextVector2Circular(npc.width * 0.5f, npc.height * 0.5f);
                Vector2 velocity = new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), Main.rand.NextFloat(-2f, -0.5f));
                
                // BLACK → ORANGE gradient based on intensity
                Color color = Color.Lerp(ThemedParticles.CampanellaBlack, ThemedParticles.CampanellaOrange, intensity + Main.rand.NextFloat(0.2f));
                int dustType = Main.rand.NextBool() ? DustID.Torch : DustID.Smoke;
                
                Dust flame = Dust.NewDustPerfect(npc.Center + offset, dustType, velocity, 100, color, 1.5f + intensity);
                flame.noGravity = true;
            }
            
            // Occasional music notes
            if (Main.rand.NextBool(30) && CustomParticleSystem.TexturesLoaded)
            {
                ThemedParticles.LaCampanellaMusicNotes(npc.Center + Main.rand.NextVector2Circular(20f, 20f), 1, 15f);
            }
            
            // Lighting
            Lighting.AddLight(npc.Center, 0.4f * intensity, 0.25f * intensity, 0.1f * intensity);
        }

        private void SpawnStackIndicator(NPC npc)
        {
            // Small orbiting flame particles showing stack count with BLACK → ORANGE gradient
            float orbitAngle = flameAnimTimer + Main.rand.NextFloat(MathHelper.TwoPi);
            float orbitRadius = npc.width * 0.4f + 10f;
            Vector2 orbitPos = npc.Center + orbitAngle.ToRotationVector2() * orbitRadius;
            
            float stackProgress = Stacks / (float)ResonantToll.MaxStacks;
            Color color = Color.Lerp(ThemedParticles.CampanellaBlack, ThemedParticles.CampanellaOrange, stackProgress + Main.rand.NextFloat(0.2f));
            Dust indicator = Dust.NewDustPerfect(orbitPos, DustID.Torch, Vector2.Zero, 100, color, 1.2f);
            indicator.noGravity = true;
            indicator.velocity = (npc.Center - orbitPos).SafeNormalize(Vector2.Zero) * 0.5f;
        }

        public override void ModifyHitByItem(NPC npc, Player player, Item item, ref NPC.HitModifiers modifiers)
        {
            if (HasResonantToll && Stacks > 0)
            {
                // Defense reduction: -2 defense per stack
                modifiers.Defense.Flat -= Stacks * 2;
            }
        }

        public override void ModifyHitByProjectile(NPC npc, Projectile projectile, ref NPC.HitModifiers modifiers)
        {
            if (HasResonantToll && Stacks > 0)
            {
                // Defense reduction: -2 defense per stack
                modifiers.Defense.Flat -= Stacks * 2;
            }
        }
    }

    /// <summary>
    /// Simple screen shake player for effects.
    /// </summary>
    public class ScreenShakePlayer : ModPlayer
    {
        private float shakeIntensity = 0f;
        private int shakeDuration = 0;

        public void AddShake(float intensity, int duration)
        {
            if (intensity > shakeIntensity)
            {
                shakeIntensity = intensity;
                shakeDuration = duration;
            }
        }

        public override void ModifyScreenPosition()
        {
            if (shakeDuration > 0 && shakeIntensity > 0)
            {
                Main.screenPosition += Main.rand.NextVector2Circular(shakeIntensity, shakeIntensity);
                shakeIntensity *= 0.9f;
                shakeDuration--;
            }
        }
    }
}
