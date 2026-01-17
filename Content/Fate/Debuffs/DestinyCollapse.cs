using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Content.Fate.Debuffs
{
    /// <summary>
    /// DestinyCollapse - The signature debuff of Fate weapons.
    /// 
    /// Effects:
    /// - Cosmic damage over time that scales with stacks
    /// - Micro-teleports: enemy jitters/shifts slightly
    /// - "Fate Fractures" slow enemy and reduce their damage
    /// - At max stacks: Singularity pulls in enemies, then supernova explosion spreads debuff
    /// </summary>
    public class DestinyCollapse : ModBuff
    {
        public const int MaxStacks = 8;
        public const int BaseDamagePerSecond = 20;
        public const int DamagePerStack = 12;
        public const float SlowPerStack = 0.04f; // 4% slow per stack
        public const float SingularityRadius = 250f;
        
        // Use placeholder texture
        public override string Texture => "Terraria/Images/Buff_" + BuffID.Obstructed;
        
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = true;
            BuffID.Sets.NurseCannotRemoveDebuff[Type] = true;
        }
        
        public override void Update(NPC npc, ref int buffIndex)
        {
            npc.GetGlobalNPC<DestinyCollapseNPC>().HasDestinyCollapse = true;
        }
    }
    
    public class DestinyCollapseNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;
        
        public bool HasDestinyCollapse;
        public int CollapseStacks = 1;
        public int CollapseTimer;
        public int FractureTimer;
        public int SingularityChargeTime;
        public bool IsSingularityActive;
        
        // Cosmic Revisit system - delayed damage flares
        private readonly List<CosmicRevisit> pendingRevisits = new List<CosmicRevisit>();
        
        private struct CosmicRevisit
        {
            public int Timer;
            public int Damage;
            public Vector2 SourcePosition;
            public float Scale;
        }
        
        // Fate theme colors
        private static readonly Color FateWhite = new Color(255, 255, 255);
        private static readonly Color FateDarkPink = new Color(200, 80, 120);
        private static readonly Color FatePurple = new Color(140, 50, 160);
        private static readonly Color FateCrimson = new Color(180, 30, 60);
        private static readonly Color FateBlack = new Color(10, 5, 15);
        private static readonly Color FateBrightRed = new Color(255, 60, 80);
        
        /// <summary>
        /// Gets the cosmic gradient color based on progress (0-1).
        /// </summary>
        private Color GetCosmicGradient(float progress)
        {
            // White -> DarkPink -> Purple -> Crimson
            if (progress < 0.33f)
                return Color.Lerp(FateWhite, FateDarkPink, progress / 0.33f);
            else if (progress < 0.66f)
                return Color.Lerp(FateDarkPink, FatePurple, (progress - 0.33f) / 0.33f);
            else
                return Color.Lerp(FatePurple, FateCrimson, (progress - 0.66f) / 0.34f);
        }
        
        public override void ResetEffects(NPC npc)
        {
            if (!HasDestinyCollapse)
            {
                CollapseStacks = 1;
                CollapseTimer = 0;
                FractureTimer = 0;
                IsSingularityActive = false;
                SingularityChargeTime = 0;
            }
            HasDestinyCollapse = false;
        }
        
        public override void UpdateLifeRegen(NPC npc, ref int damage)
        {
            if (HasDestinyCollapse)
            {
                // Cosmic DoT - scales with stacks
                int dps = DestinyCollapse.BaseDamagePerSecond + (CollapseStacks - 1) * DestinyCollapse.DamagePerStack;
                npc.lifeRegen -= dps * 2;
                damage = Math.Max(damage, dps / 3);
            }
        }
        
        public override void AI(NPC npc)
        {
            // Process Cosmic Revisits (delayed damage flares) - always processed
            ProcessCosmicRevisits(npc);
            
            if (!HasDestinyCollapse) return;
            
            CollapseTimer++;
            FractureTimer++;
            
            // Apply slow (Fate Fractures effect)
            float slowAmount = CollapseStacks * DestinyCollapse.SlowPerStack;
            npc.velocity *= (1f - slowAmount);
            
            // Visual effects - cosmic particles
            if (CollapseTimer % 6 == 0)
            {
                SpawnCosmicParticles(npc);
            }
            
            // Micro-teleports (jitter effect)
            if (FractureTimer >= 30 && !IsSingularityActive)
            {
                FractureTimer = 0;
                int jitterChance = 10 + CollapseStacks * 8;
                
                if (Main.rand.Next(100) < jitterChance)
                {
                    PerformMicroTeleport(npc);
                }
            }
            
            // Max stack singularity
            if (CollapseStacks >= DestinyCollapse.MaxStacks && !IsSingularityActive)
            {
                IsSingularityActive = true;
                SingularityChargeTime = 0;
            }
            
            // Singularity building up
            if (IsSingularityActive)
            {
                SingularityChargeTime++;
                UpdateSingularity(npc);
                
                if (SingularityChargeTime >= 90) // 1.5 seconds of pull, then explode
                {
                    TriggerSupernova(npc);
                    IsSingularityActive = false;
                    CollapseStacks = 0;
                    int buffIndex = npc.FindBuffIndex(ModContent.BuffType<DestinyCollapse>());
                    if (buffIndex >= 0)
                        npc.DelBuff(buffIndex);
                }
            }
        }
        
        private void SpawnCosmicParticles(NPC npc)
        {
            Vector2 pos = npc.Center + Main.rand.NextVector2Circular(npc.width * 0.7f, npc.height * 0.7f);
            
            float progress = Main.rand.NextFloat();
            Color particleColor = GetCosmicGradient(progress);
            
            // Orbital particles that spiral inward
            float angle = Main.rand.NextFloat() * MathHelper.TwoPi;
            float spiralSpeed = 1.5f + CollapseStacks * 0.3f;
            Vector2 velocity = angle.ToRotationVector2() * spiralSpeed;
            velocity = velocity.RotatedBy(MathHelper.PiOver4); // Spiral inward
            
            var glow = new GenericGlowParticle(pos, velocity, particleColor, 0.25f + CollapseStacks * 0.05f, 25, true);
            MagnumParticleHandler.SpawnParticle(glow);
            
            // Cosmic flares at higher stacks
            if (CollapseStacks >= 3 && Main.rand.NextBool(3))
            {
                float hue = ((Main.GameUpdateCount * 0.01f) + Main.rand.NextFloat()) % 1f;
                // Bias toward pink/purple range
                hue = 0.8f + hue * 0.2f;
                if (hue > 1f) hue -= 1f;
                Color cosmicColor = Main.hslToRgb(hue, 0.8f, 0.7f);
                CustomParticles.GenericFlare(pos, cosmicColor, 0.4f, 18);
            }
            
            // Reality tear particles at high stacks
            if (CollapseStacks >= 5 && Main.rand.NextBool(4))
            {
                // Chromatic aberration-style offset particles
                CustomParticles.GenericFlare(pos + new Vector2(-3, 0), Color.Red * 0.5f, 0.3f, 10);
                CustomParticles.GenericFlare(pos, Color.Green * 0.5f, 0.3f, 10);
                CustomParticles.GenericFlare(pos + new Vector2(3, 0), Color.Blue * 0.5f, 0.3f, 10);
            }
        }
        
        /// <summary>
        /// Process pending cosmic revisits - delayed damage flares that strike the enemy again.
        /// </summary>
        private void ProcessCosmicRevisits(NPC npc)
        {
            if (pendingRevisits.Count == 0) return;
            
            for (int i = pendingRevisits.Count - 1; i >= 0; i--)
            {
                var revisit = pendingRevisits[i];
                revisit.Timer--;
                pendingRevisits[i] = revisit;
                
                // Pre-flash warning at 10 frames before impact
                if (revisit.Timer == 10)
                {
                    // Small warning glow
                    CustomParticles.GenericFlare(npc.Center, FateDarkPink * 0.6f, revisit.Scale * 0.4f, 10);
                }
                
                // Trigger the cosmic revisit
                if (revisit.Timer <= 0)
                {
                    TriggerCosmicRevisit(npc, revisit);
                    pendingRevisits.RemoveAt(i);
                }
            }
        }
        
        /// <summary>
        /// The cosmic flare strikes - a delayed burst of cosmic light dealing massive damage.
        /// </summary>
        private void TriggerCosmicRevisit(NPC npc, CosmicRevisit revisit)
        {
            Vector2 center = npc.Center;
            
            // Sound effect - ethereal cosmic strike
            SoundEngine.PlaySound(SoundID.Item125 with { Pitch = 0.3f, Volume = 0.8f }, center);
            
            // === PHASE 1: COSMIC BEAM FROM SOURCE ===
            // Draw a quick beam of light from source to target
            if (revisit.SourcePosition != Vector2.Zero)
            {
                DrawCosmicBeam(revisit.SourcePosition, center);
            }
            
            // === PHASE 2: CENTRAL COSMIC FLASH ===
            // Bright white core flash
            CustomParticles.GenericFlare(center, FateWhite, revisit.Scale * 1.5f, 20);
            // Dark pink secondary
            CustomParticles.GenericFlare(center, FateDarkPink, revisit.Scale * 1.2f, 18);
            // Bright red tertiary
            CustomParticles.GenericFlare(center, FateBrightRed, revisit.Scale * 0.9f, 15);
            
            // === PHASE 3: COSMIC HALO ===
            CustomParticles.HaloRing(center, FateBrightRed * 0.9f, 0.5f * revisit.Scale, 18);
            
            // === PHASE 4: CHROMATIC BURST PATTERN ===
            // 6-point star burst with RGB separation
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f;
                Vector2 offset = angle.ToRotationVector2() * 25f * revisit.Scale;
                float progress = (float)i / 6f;
                Color burstColor = GetCosmicGradient(progress);
                
                // Core particle
                CustomParticles.GenericFlare(center + offset, burstColor, 0.45f * revisit.Scale, 15);
                
                // Chromatic aberration
                CustomParticles.GenericFlare(center + offset + new Vector2(-2, 0), Color.Red * 0.4f, 0.3f * revisit.Scale, 10);
                CustomParticles.GenericFlare(center + offset + new Vector2(2, 0), Color.Cyan * 0.4f, 0.3f * revisit.Scale, 10);
            }
            
            // === PHASE 5: RADIAL SPARK EXPLOSION ===
            int sparkCount = (int)(12 * revisit.Scale);
            for (int i = 0; i < sparkCount; i++)
            {
                float angle = MathHelper.TwoPi * i / sparkCount;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(6f, 12f);
                Color sparkColor = GetCosmicGradient((float)i / sparkCount);
                
                var spark = new GenericGlowParticle(center, vel, sparkColor, 
                    Main.rand.NextFloat(0.35f, 0.55f), Main.rand.Next(18, 28), true);
                MagnumParticleHandler.SpawnParticle(spark);
            }
            
            // === PHASE 6: MUSIC NOTE COSMIC ECHO ===
            ThemedParticles.FateMusicNotes(center, 4, 35f);
            
            // === PHASE 7: GLYPH IMPACT ===
            CustomParticles.GlyphImpact(center, FateDarkPink, FateBrightRed, 0.5f * revisit.Scale);
            
            // === DEAL THE DAMAGE ===
            if (npc.active && !npc.friendly)
            {
                npc.SimpleStrikeNPC(revisit.Damage, 0, true, 0f, null, false, 0f, true);
            }
            
            // Dynamic lighting
            Lighting.AddLight(center, FateBrightRed.ToVector3() * 1.2f);
        }
        
        /// <summary>
        /// Draw a cosmic beam of light between two points.
        /// </summary>
        private void DrawCosmicBeam(Vector2 start, Vector2 end)
        {
            Vector2 direction = end - start;
            float length = direction.Length();
            int segments = Math.Max(3, (int)(length / 25f));
            
            for (int i = 0; i <= segments; i++)
            {
                float t = (float)i / segments;
                Vector2 pos = Vector2.Lerp(start, end, t);
                
                // Add slight wave
                Vector2 perpendicular = new Vector2(-direction.Y, direction.X);
                perpendicular.Normalize();
                float wave = (float)Math.Sin(t * MathHelper.TwoPi * 2) * 5f;
                pos += perpendicular * wave;
                
                Color beamColor = GetCosmicGradient(t);
                float scale = 0.3f + (1f - Math.Abs(t - 0.5f) * 2f) * 0.2f; // Thicker in middle
                
                CustomParticles.GenericFlare(pos, beamColor, scale, 8);
                
                // Chromatic edges
                if (i % 2 == 0)
                {
                    CustomParticles.GenericFlare(pos + perpendicular * 3f, Color.Cyan * 0.3f, 0.2f, 6);
                    CustomParticles.GenericFlare(pos - perpendicular * 3f, Color.Red * 0.3f, 0.2f, 6);
                }
            }
        }
        
        /// <summary>
        /// Queue a cosmic revisit - called when a Fate weapon hits an enemy.
        /// After the delay, a cosmic flare will strike the enemy dealing bonus damage.
        /// </summary>
        /// <param name="npc">The target NPC</param>
        /// <param name="damage">The bonus damage to deal</param>
        /// <param name="delay">Delay in frames before the revisit strikes (default 25 = ~0.4 seconds)</param>
        /// <param name="sourcePosition">Where the attack originated from (for beam effect)</param>
        /// <param name="scale">Scale of the visual effect (default 1.0)</param>
        public void QueueCosmicRevisit(NPC npc, int damage, int delay = 25, Vector2 sourcePosition = default, float scale = 1f)
        {
            pendingRevisits.Add(new CosmicRevisit
            {
                Timer = delay,
                Damage = damage,
                SourcePosition = sourcePosition,
                Scale = scale
            });
            
            // Queue visual indicator - small cosmic mark appears
            float markAngle = Main.rand.NextFloat() * MathHelper.TwoPi;
            Vector2 markOffset = markAngle.ToRotationVector2() * 15f;
            CustomParticles.GenericFlare(npc.Center + markOffset, FateDarkPink * 0.5f, 0.25f * scale, delay);
        }
        
        private void PerformMicroTeleport(NPC npc)
        {
            // Small jitter - not a full teleport
            Vector2 jitter = Main.rand.NextVector2Circular(8f + CollapseStacks * 2f, 8f + CollapseStacks * 2f);
            npc.position += jitter;
            npc.netUpdate = true;
            
            // Glitch visual effect
            for (int i = 0; i < 4; i++)
            {
                float progress = (float)i / 4f;
                Color glitchColor = GetCosmicGradient(progress);
                Vector2 offset = Main.rand.NextVector2Circular(15f, 15f);
                CustomParticles.GenericFlare(npc.Center + offset, glitchColor * 0.7f, 0.3f, 8);
            }
        }
        
        private void UpdateSingularity(NPC npc)
        {
            float chargeProgress = SingularityChargeTime / 90f;
            Vector2 center = npc.Center;
            
            // Singularity visual - swirling vortex
            if (SingularityChargeTime % 3 == 0)
            {
                // Spiral particles being pulled in
                for (int i = 0; i < 4; i++)
                {
                    float angle = Main.GameUpdateCount * 0.1f + MathHelper.TwoPi * i / 4f;
                    float radius = 100f * (1f - chargeProgress);
                    Vector2 particlePos = center + angle.ToRotationVector2() * radius;
                    Vector2 velocity = (center - particlePos).SafeNormalize(Vector2.Zero) * 5f;
                    
                    float progress = (float)i / 4f;
                    Color spiralColor = GetCosmicGradient(progress);
                    
                    var glow = new GenericGlowParticle(particlePos, velocity, spiralColor, 0.4f, 20, true);
                    MagnumParticleHandler.SpawnParticle(glow);
                }
                
                // === DARK COSMIC SMOKE - amorphous singularity energy ===
                float smokeAngle = Main.rand.NextFloat(MathHelper.TwoPi);
                float smokeRadius = Main.rand.NextFloat(30f, 80f) * (1f - chargeProgress * 0.5f);
                Vector2 smokePos = center + smokeAngle.ToRotationVector2() * smokeRadius;
                Vector2 smokeVel = (center - smokePos).SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(1f, 2f);
                var smoke = new HeavySmokeParticle(
                    smokePos,
                    smokeVel,
                    Color.Lerp(FateBlack, FatePurple, Main.rand.NextFloat(0.4f)),
                    Main.rand.Next(35, 55), 0.4f * (0.5f + chargeProgress), 0.55f, 0.02f, false);
                MagnumParticleHandler.SpawnParticle(smoke);
            }
            
            // Central buildup
            if (SingularityChargeTime % 8 == 0)
            {
                float pulse = 0.3f + chargeProgress * 0.5f;
                CustomParticles.GenericFlare(center, FateWhite, pulse, 15);
                CustomParticles.HaloRing(center, FatePurple * chargeProgress, 0.3f, 12);
            }
            
            // Pull in nearby enemies
            float pullRadius = DestinyCollapse.SingularityRadius * chargeProgress;
            float pullStrength = 2f + chargeProgress * 4f;
            
            foreach (NPC target in Main.ActiveNPCs)
            {
                if (target.whoAmI != npc.whoAmI && !target.friendly && target.knockBackResist > 0f)
                {
                    float distance = Vector2.Distance(target.Center, center);
                    if (distance <= pullRadius && distance > 20f)
                    {
                        Vector2 pullDirection = (center - target.Center).SafeNormalize(Vector2.Zero);
                        float pullMagnitude = pullStrength * (1f - distance / pullRadius) * target.knockBackResist;
                        target.velocity += pullDirection * pullMagnitude * 0.1f;
                    }
                }
            }
            
            // Screen shake buildup (removed - weapon effects shouldn't shake screen)
            // if (Main.LocalPlayer.Distance(center) < 600f)
            // {
            //     MagnumScreenEffects.AddScreenShake(chargeProgress * 3f);
            // }
        }
        
        private void TriggerSupernova(NPC npc)
        {
            Vector2 center = npc.Center;
            
            SoundEngine.PlaySound(SoundID.Item14 with { Pitch = -0.5f, Volume = 1.2f }, center);
            SoundEngine.PlaySound(SoundID.Item162 with { Pitch = 0.3f, Volume = 1f }, center);
            
            // MASSIVE cosmic supernova explosion
            
            // Central flash
            CustomParticles.GenericFlare(center, FateWhite, 2f, 30);
            
            // Single expanding shockwave
            CustomParticles.HaloRing(center, FateBrightRed, 0.8f, 25);
            
            // Fractal burst pattern - signature MagnumOpus look
            for (int layer = 0; layer < 3; layer++)
            {
                int points = 6 + layer * 2;
                float radius = 40f + layer * 35f;
                
                for (int i = 0; i < points; i++)
                {
                    float angle = MathHelper.TwoPi * i / points + layer * 0.2f;
                    Vector2 offset = angle.ToRotationVector2() * radius;
                    float gradientProgress = ((float)i / points + layer * 0.3f) % 1f;
                    Color burstColor = GetCosmicGradient(gradientProgress);
                    float flareScale = 0.7f - layer * 0.1f;
                    CustomParticles.GenericFlare(center + offset, burstColor, flareScale, 25);
                }
            }
            
            // Radial particle explosion
            for (int i = 0; i < 40; i++)
            {
                float angle = MathHelper.TwoPi * i / 40f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(10f, 20f);
                float progress = (float)i / 40f;
                Color burstColor = GetCosmicGradient(progress);
                
                var glow = new GenericGlowParticle(center, vel, burstColor, 
                    Main.rand.NextFloat(0.5f, 0.9f), Main.rand.Next(35, 55), true);
                MagnumParticleHandler.SpawnParticle(glow);
            }
            
            // === MASSIVE COSMIC SMOKE BURST - supernova aftermath ===
            for (int i = 0; i < 16; i++)
            {
                float angle = MathHelper.TwoPi * i / 16f;
                Vector2 smokeVel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 8f) + new Vector2(0, -1f);
                var smoke = new HeavySmokeParticle(
                    center + Main.rand.NextVector2Circular(20f, 20f),
                    smokeVel,
                    Color.Lerp(FateBlack, FatePurple, Main.rand.NextFloat(0.5f)),
                    Main.rand.Next(50, 80), 0.6f, 0.8f, 0.012f, false);
                MagnumParticleHandler.SpawnParticle(smoke);
            }
            
            // Glyph explosion - cosmic runes scatter
            CustomParticles.GlyphBurst(center, FateDarkPink, 12, 8f);
            CustomParticles.GlyphCircle(center, FateBrightRed, 10, 80f, 0.06f);
            
            // Chromatic aberration burst (reality tear effect)
            for (int i = 0; i < 12; i++)
            {
                float angle = MathHelper.TwoPi * i / 12f;
                Vector2 baseOffset = angle.ToRotationVector2() * 60f;
                
                // RGB separation
                CustomParticles.GenericFlare(center + baseOffset + new Vector2(-4, 0), Color.Red * 0.6f, 0.45f, 18);
                CustomParticles.GenericFlare(center + baseOffset, Color.Green * 0.6f, 0.45f, 18);
                CustomParticles.GenericFlare(center + baseOffset + new Vector2(4, 0), Color.Blue * 0.6f, 0.45f, 18);
            }
            
            // Spread debuff to nearby enemies
            foreach (NPC target in Main.ActiveNPCs)
            {
                if (target.whoAmI != npc.whoAmI && !target.friendly && target.lifeMax > 5)
                {
                    float distance = Vector2.Distance(target.Center, center);
                    if (distance <= DestinyCollapse.SingularityRadius)
                    {
                        // Apply Destiny Collapse
                        target.AddBuff(ModContent.BuffType<DestinyCollapse>(), 300);
                        var collapseNPC = target.GetGlobalNPC<DestinyCollapseNPC>();
                        collapseNPC.AddStack(target, 3);
                        
                        // Connecting cosmic lightning
                        DrawCosmicLightning(center, target.Center);
                        
                        // Impact at target
                        CustomParticles.GenericFlare(target.Center, FateDarkPink, 0.6f, 18);
                        CustomParticles.HaloRing(target.Center, FatePurple, 0.4f, 15);
                    }
                }
            }
            
            // Damage burst
            foreach (NPC target in Main.ActiveNPCs)
            {
                if (!target.friendly)
                {
                    float distance = Vector2.Distance(target.Center, center);
                    if (distance <= DestinyCollapse.SingularityRadius)
                    {
                        int damage = (int)(250 * (1f - distance / DestinyCollapse.SingularityRadius * 0.5f));
                        target.SimpleStrikeNPC(damage, 0, false, 0f, null, false, 0f, true);
                    }
                }
            }
            
            // Screen shake (removed - weapon effects shouldn't shake screen)
            // if (Main.LocalPlayer.Distance(center) < 1000f)
            // {
            //     MagnumScreenEffects.AddScreenShake(15f);
            // }
        }
        
        private void DrawCosmicLightning(Vector2 start, Vector2 end)
        {
            Vector2 direction = end - start;
            float length = direction.Length();
            int segments = (int)(length / 15f);
            direction.Normalize();
            Vector2 perpendicular = new Vector2(-direction.Y, direction.X);
            
            Vector2 lastPoint = start;
            for (int i = 1; i <= segments; i++)
            {
                float t = (float)i / segments;
                Vector2 basePoint = Vector2.Lerp(start, end, t);
                
                if (i < segments)
                {
                    float offset = Main.rand.NextFloat(-20f, 20f);
                    basePoint += perpendicular * offset;
                }
                
                // Draw lightning segment particles with cosmic gradient
                for (int j = 0; j < 2; j++)
                {
                    float segT = j / 2f;
                    Vector2 segPos = Vector2.Lerp(lastPoint, basePoint, segT);
                    Color lightningColor = GetCosmicGradient(t);
                    CustomParticles.GenericFlare(segPos, lightningColor, 0.2f, 8);
                }
                
                lastPoint = basePoint;
            }
        }
        
        public override void DrawEffects(NPC npc, ref Color drawColor)
        {
            if (HasDestinyCollapse)
            {
                // Cosmic tint
                float intensity = 0.1f + CollapseStacks * 0.08f;
                Color cosmicTint = GetCosmicGradient((Main.GameUpdateCount * 0.02f) % 1f);
                drawColor = Color.Lerp(drawColor, cosmicTint, intensity);
                
                // Slight transparency during singularity
                if (IsSingularityActive)
                {
                    float collapseAlpha = 1f - (SingularityChargeTime / 90f) * 0.3f;
                    drawColor *= collapseAlpha;
                }
            }
        }
        
        /// <summary>
        /// Add a stack of Destiny Collapse. Called by weapons on hit.
        /// </summary>
        public void AddStack(NPC npc, int amount = 1, int duration = 300)
        {
            int oldStacks = CollapseStacks;
            CollapseStacks = Math.Min(CollapseStacks + amount, DestinyCollapse.MaxStacks);
            
            if (CollapseStacks > oldStacks)
            {
                // Stack gain VFX
                for (int i = 0; i < CollapseStacks; i++)
                {
                    float angle = MathHelper.TwoPi * i / CollapseStacks;
                    Vector2 offset = angle.ToRotationVector2() * 20f;
                    Color stackColor = GetCosmicGradient((float)i / CollapseStacks);
                    CustomParticles.GenericFlare(npc.Center + offset, stackColor, 0.4f, 15);
                }
                
                CustomParticles.HaloRing(npc.Center, FateDarkPink, 0.35f, 12);
            }
            
            // Refresh duration
            int buffIndex = npc.FindBuffIndex(ModContent.BuffType<DestinyCollapse>());
            if (buffIndex >= 0)
            {
                npc.buffTime[buffIndex] = Math.Max(npc.buffTime[buffIndex], duration);
            }
        }
        
        /// <summary>
        /// Get the current stack count of Destiny Collapse on this NPC.
        /// </summary>
        public int GetStacks(NPC npc)
        {
            if (!HasDestinyCollapse)
                return 0;
            return CollapseStacks;
        }
    }
}
