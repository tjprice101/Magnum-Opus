using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Terraria.Audio;
using Terraria.GameContent;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Content.Fate.Debuffs;

namespace MagnumOpus.Content.Fate
{
    /// <summary>
    /// Fate Astrological Ring - A large kaleidoscopic symbol that draws around the player,
    /// then ignites to deal damage. Inspired by Profaned Guardian's ring attacks.
    /// 
    /// PHASES:
    /// 1. DRAWING (30 frames): Ring segments draw rapidly in sequence
    /// 2. GLOW (15 frames): Ring pulses and glows intensely
    /// 3. IGNITION (20 frames): Ring explodes with flares, dealing damage
    /// 4. FADE (15 frames): Effects dissipate
    /// </summary>
    public class FateAstrologicalRing : ModProjectile
    {
        // DARK PRISMATIC COLOR PALETTE
        private static readonly Color FateBlack = new Color(15, 5, 20);
        private static readonly Color FateDarkPink = new Color(180, 50, 100);
        private static readonly Color FateBrightRed = new Color(255, 60, 80);
        private static readonly Color FatePurple = new Color(120, 30, 140);
        private static readonly Color FateWhite = new Color(255, 255, 255);

        // Ring parameters
        private const float BaseRadius = 200f;
        private const int SymbolSegments = 12; // 12 astrological segments
        private const int InnerRings = 3;
        
        // Phase timings
        private const int DrawPhase = 30;
        private const int GlowPhase = 15;
        private const int IgnitePhase = 20;
        private const int FadePhase = 15;
        private const int TotalLifetime = DrawPhase + GlowPhase + IgnitePhase + FadePhase;

        // Damage tracking
        private bool hasDamaged = false;
        private HashSet<int> hitNPCs = new HashSet<int>();

        public override string Texture => "MagnumOpus/Assets/Particles/SoftGlow";

        private ref float Timer => ref Projectile.ai[0];
        private ref float RingRotation => ref Projectile.ai[1];
        private ref float OwnerIndex => ref Projectile.localAI[0];

        private Color GetFateGradient(float progress)
        {
            if (progress < 0.4f)
                return Color.Lerp(FateBlack, FateDarkPink, progress / 0.4f);
            else if (progress < 0.8f)
                return Color.Lerp(FateDarkPink, FateBrightRed, (progress - 0.4f) / 0.4f);
            else
                return Color.Lerp(FateBrightRed, FateWhite, (progress - 0.8f) / 0.2f);
        }

        public override void SetDefaults()
        {
            Projectile.width = (int)(BaseRadius * 2);
            Projectile.height = (int)(BaseRadius * 2);
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Generic;
            Projectile.penetrate = -1;
            Projectile.timeLeft = TotalLifetime;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1; // Only hit once per NPC
            Projectile.alpha = 0;
        }

        public override void AI()
        {
            Timer++;
            
            // Anchor to owner
            Player owner = Main.player[Projectile.owner];
            Projectile.Center = owner.Center;
            
            // Slow rotation
            RingRotation += 0.015f;

            // Determine current phase
            int phase = GetCurrentPhase();

            switch (phase)
            {
                case 0: // Drawing phase
                    DrawingPhaseEffects();
                    break;
                case 1: // Glow phase
                    GlowPhaseEffects();
                    break;
                case 2: // Ignition phase
                    IgnitionPhaseEffects();
                    break;
                case 3: // Fade phase
                    FadePhaseEffects();
                    break;
            }

            // Add lighting
            float intensity = phase == 2 ? 1.5f : (phase == 1 ? 1.2f : 0.7f);
            Lighting.AddLight(Projectile.Center, FateDarkPink.ToVector3() * intensity);
        }

        private int GetCurrentPhase()
        {
            if (Timer <= DrawPhase) return 0;
            if (Timer <= DrawPhase + GlowPhase) return 1;
            if (Timer <= DrawPhase + GlowPhase + IgnitePhase) return 2;
            return 3;
        }

        private float GetDrawProgress()
        {
            return Math.Clamp(Timer / DrawPhase, 0f, 1f);
        }

        private float GetGlowProgress()
        {
            float t = Timer - DrawPhase;
            return Math.Clamp(t / GlowPhase, 0f, 1f);
        }

        private float GetIgniteProgress()
        {
            float t = Timer - DrawPhase - GlowPhase;
            return Math.Clamp(t / IgnitePhase, 0f, 1f);
        }

        private float GetFadeProgress()
        {
            float t = Timer - DrawPhase - GlowPhase - IgnitePhase;
            return Math.Clamp(t / FadePhase, 0f, 1f);
        }

        /// <summary>
        /// Phase 0: Draw the ring segments rapidly
        /// </summary>
        private void DrawingPhaseEffects()
        {
            float progress = GetDrawProgress();
            int segmentsToShow = (int)(SymbolSegments * progress);

            // Spawn particles for each newly drawn segment
            for (int i = 0; i < segmentsToShow; i++)
            {
                float segmentAngle = RingRotation + MathHelper.TwoPi * i / SymbolSegments;
                
                // Outer ring points
                Vector2 outerPoint = Projectile.Center + segmentAngle.ToRotationVector2() * BaseRadius;
                
                // Spawn drawing sparks at the "pen" position
                if (Main.rand.NextBool(3))
                {
                    float sparkProgress = (float)i / SymbolSegments;
                    Color sparkColor = GetFateGradient(sparkProgress);
                    CustomParticles.GenericFlare(outerPoint, sparkColor, 0.4f, 12);
                    
                    // Trailing glyph
                    if (Main.rand.NextBool(4))
                    {
                        CustomParticles.Glyph(outerPoint, FatePurple * 0.6f, 0.25f, -1);
                    }
                }
            }

            // Inner ring drawing
            for (int ring = 0; ring < InnerRings; ring++)
            {
                float ringRadius = BaseRadius * (0.4f + ring * 0.2f);
                int pointsPerRing = 8 + ring * 4;
                int pointsToShow = (int)(pointsPerRing * progress);

                for (int i = 0; i < pointsToShow; i++)
                {
                    if (!Main.rand.NextBool(5)) continue;
                    
                    float angle = RingRotation * (ring % 2 == 0 ? 1 : -1) + MathHelper.TwoPi * i / pointsPerRing;
                    Vector2 point = Projectile.Center + angle.ToRotationVector2() * ringRadius;
                    Color ringColor = GetFateGradient((float)ring / InnerRings) * 0.7f;
                    
                    var glow = new GenericGlowParticle(point, Vector2.Zero, ringColor, 0.2f, 10, true);
                    MagnumParticleHandler.SpawnParticle(glow);
                }
            }

            // Connecting lines between segments (like astrological constellation)
            if (progress > 0.3f && Main.rand.NextBool(4))
            {
                int seg1 = Main.rand.Next(segmentsToShow);
                int seg2 = (seg1 + Main.rand.Next(1, 4)) % segmentsToShow;
                
                float angle1 = RingRotation + MathHelper.TwoPi * seg1 / SymbolSegments;
                float angle2 = RingRotation + MathHelper.TwoPi * seg2 / SymbolSegments;
                
                Vector2 point1 = Projectile.Center + angle1.ToRotationVector2() * BaseRadius * 0.7f;
                Vector2 point2 = Projectile.Center + angle2.ToRotationVector2() * BaseRadius * 0.7f;
                
                // Draw connecting line particles
                int linePoints = 5;
                for (int i = 0; i < linePoints; i++)
                {
                    Vector2 linePoint = Vector2.Lerp(point1, point2, (float)i / linePoints);
                    CustomParticles.GenericFlare(linePoint, FateDarkPink * 0.5f, 0.15f, 8);
                }
            }

            // Central glyph formation
            if (progress > 0.5f && Main.rand.NextBool(6))
            {
                CustomParticles.GlyphCircle(Projectile.Center, FatePurple * 0.5f, 6, 50f, 0.03f);
            }
        }

        /// <summary>
        /// Phase 1: Ring glows and pulses intensely
        /// </summary>
        private void GlowPhaseEffects()
        {
            float progress = GetGlowProgress();
            float pulse = (float)Math.Sin(progress * MathHelper.TwoPi * 3) * 0.3f + 1f;

            // Full ring glowing
            for (int i = 0; i < SymbolSegments; i++)
            {
                float segmentAngle = RingRotation + MathHelper.TwoPi * i / SymbolSegments;
                float segmentProgress = (float)i / SymbolSegments;
                
                // Outer points glow brightly
                Vector2 outerPoint = Projectile.Center + segmentAngle.ToRotationVector2() * BaseRadius;
                Color glowColor = GetFateGradient(segmentProgress) * pulse;
                
                CustomParticles.GenericFlare(outerPoint, glowColor, 0.5f * pulse, 8);
                
                // Halo rings at segment points
                if (Main.rand.NextBool(8))
                {
                    CustomParticles.HaloRing(outerPoint, FateBrightRed * 0.6f, 0.3f, 12);
                }
            }

            // Pulsing inner rings
            for (int ring = 0; ring < InnerRings; ring++)
            {
                float ringRadius = BaseRadius * (0.4f + ring * 0.2f);
                float ringPulse = (float)Math.Sin(progress * MathHelper.TwoPi * 2 + ring * 0.5f) * 0.2f + 1f;
                
                CustomParticles.HaloRing(Projectile.Center, GetFateGradient((float)ring / InnerRings) * 0.6f * ringPulse, 
                    ringRadius / 100f, 10);
            }

            // Central glyph tower
            if (Main.rand.NextBool(4))
            {
                CustomParticles.GlyphTower(Projectile.Center, FateDarkPink * 0.8f, 3, 0.35f);
            }

            // Music notes orbiting
            if (Main.rand.NextBool(6))
            {
                ThemedParticles.FateMusicNotes(Projectile.Center, 2, BaseRadius * 0.5f);
            }

            // Intense center glow
            float centerIntensity = 0.5f + progress * 0.5f;
            CustomParticles.GenericFlare(Projectile.Center, FateWhite * centerIntensity, 0.6f, 6);
            CustomParticles.GenericFlare(Projectile.Center, FateBrightRed * centerIntensity, 0.8f, 6);
        }

        /// <summary>
        /// Phase 2: Ring ignites and deals damage
        /// </summary>
        private void IgnitionPhaseEffects()
        {
            float progress = GetIgniteProgress();

            // MASSIVE explosion at start
            if (Timer == DrawPhase + GlowPhase + 1)
            {
                // Central explosion
                CustomParticles.GenericFlare(Projectile.Center, FateWhite, 2.0f, 25);
                CustomParticles.GenericFlare(Projectile.Center, FateBrightRed, 1.8f, 22);
                CustomParticles.GenericFlare(Projectile.Center, FateDarkPink, 1.5f, 20);
                
                // Expanding halo cascade
                for (int ring = 0; ring < 8; ring++)
                {
                    float ringProgress = (float)ring / 8f;
                    Color ringColor = GetFateGradient(ringProgress);
                    CustomParticles.HaloRing(Projectile.Center, ringColor, 0.4f + ring * 0.15f, 18 + ring * 3);
                }

                // Explosion at each segment point
                for (int i = 0; i < SymbolSegments; i++)
                {
                    float segmentAngle = RingRotation + MathHelper.TwoPi * i / SymbolSegments;
                    Vector2 segmentPoint = Projectile.Center + segmentAngle.ToRotationVector2() * BaseRadius;
                    
                    // Segment explosion
                    CustomParticles.GenericFlare(segmentPoint, FateBrightRed, 0.9f, 18);
                    CustomParticles.HaloRing(segmentPoint, FateDarkPink, 0.5f, 15);
                    CustomParticles.ExplosionBurst(segmentPoint, GetFateGradient((float)i / SymbolSegments), 8, 6f);
                    
                    // Glyph burst
                    CustomParticles.GlyphBurst(segmentPoint, FatePurple, 4, 3f);
                }

                // Fractal lightning across the ring
                for (int i = 0; i < 6; i++)
                {
                    float startAngle = RingRotation + MathHelper.TwoPi * i / 6f;
                    float endAngle = startAngle + MathHelper.TwoPi / 3f;
                    
                    Vector2 start = Projectile.Center + startAngle.ToRotationVector2() * BaseRadius * 0.5f;
                    Vector2 end = Projectile.Center + endAngle.ToRotationVector2() * BaseRadius;
                    
                    MagnumVFX.DrawFractalLightningCustom(start, end, FateBrightRed, FateWhite, 
                        DustID.FireworkFountain_Red, DustID.PinkTorch, 8, 25f, 2, 0.4f, 5f);
                }

                // Sound effect
                SoundEngine.PlaySound(SoundID.Item122 with { Volume = 0.8f, Pitch = -0.2f }, Projectile.Center);
            }

            // Continuing fire effects during ignition
            float fadeOut = 1f - progress;
            
            // Radial flare waves
            if (Main.rand.NextBool(2))
            {
                float waveAngle = Main.rand.NextFloat() * MathHelper.TwoPi;
                float waveRadius = BaseRadius * (0.3f + progress * 0.7f);
                Vector2 wavePoint = Projectile.Center + waveAngle.ToRotationVector2() * waveRadius;
                
                Color waveColor = GetFateGradient(progress) * fadeOut;
                CustomParticles.GenericFlare(wavePoint, waveColor, 0.4f * fadeOut, 12);
            }

            // Outward sparks
            for (int i = 0; i < 3; i++)
            {
                float sparkAngle = Main.rand.NextFloat() * MathHelper.TwoPi;
                Vector2 sparkPos = Projectile.Center + sparkAngle.ToRotationVector2() * BaseRadius * Main.rand.NextFloat(0.5f, 1f);
                Vector2 sparkVel = sparkAngle.ToRotationVector2() * Main.rand.NextFloat(3f, 8f);
                
                var spark = new GlowSparkParticle(sparkPos, sparkVel, GetFateGradient(Main.rand.NextFloat()) * fadeOut, 
                    0.3f, 20);
                MagnumParticleHandler.SpawnParticle(spark);
            }

            // Deal damage during this phase
            DealDamageToNearbyEnemies();
        }

        /// <summary>
        /// Phase 3: Effects fade away
        /// </summary>
        private void FadePhaseEffects()
        {
            float progress = GetFadeProgress();
            float alpha = 1f - progress;

            // Fading embers
            if (Main.rand.NextBool(3))
            {
                float emberAngle = Main.rand.NextFloat() * MathHelper.TwoPi;
                float emberRadius = BaseRadius * Main.rand.NextFloat(0.2f, 1f);
                Vector2 emberPos = Projectile.Center + emberAngle.ToRotationVector2() * emberRadius;
                
                Vector2 emberVel = new Vector2(0, -Main.rand.NextFloat(1f, 3f));
                var ember = new GenericGlowParticle(emberPos, emberVel, FateDarkPink * alpha * 0.5f, 0.15f, 15, true);
                MagnumParticleHandler.SpawnParticle(ember);
            }

            // Last wisps of glyph energy
            if (Main.rand.NextBool(8))
            {
                CustomParticles.Glyph(Projectile.Center + Main.rand.NextVector2Circular(50f, 50f), 
                    FatePurple * alpha * 0.3f, 0.2f, -1);
            }
        }

        private void DealDamageToNearbyEnemies()
        {
            if (hasDamaged) return;

            float damageRadius = BaseRadius * 1.2f;
            
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly || npc.dontTakeDamage || hitNPCs.Contains(i))
                    continue;

                float distance = Vector2.Distance(Projectile.Center, npc.Center);
                if (distance <= damageRadius)
                {
                    // Apply damage
                    Player owner = Main.player[Projectile.owner];
                    int damage = Projectile.damage;
                    
                    // Create hit info
                    NPC.HitInfo hitInfo = new NPC.HitInfo
                    {
                        Damage = damage,
                        Knockback = 8f,
                        HitDirection = (npc.Center.X > Projectile.Center.X) ? 1 : -1,
                        Crit = Main.rand.NextBool(10)
                    };
                    
                    npc.StrikeNPC(hitInfo);
                    
                    // Apply Paradox debuff
                    npc.AddBuff(ModContent.BuffType<DestinyCollapse>(), 300);
                    
                    // Hit VFX
                    CustomParticles.GenericFlare(npc.Center, FateBrightRed, 0.7f, 15);
                    CustomParticles.HaloRing(npc.Center, FateDarkPink, 0.4f, 12);
                    
                    // Glyph impact
                    CustomParticles.GlyphImpact(npc.Center, FatePurple, FateBrightRed, 0.5f);
                    
                    hitNPCs.Add(i);
                }
            }
        }

        public override bool? CanDamage() => false; // We handle damage manually

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            
            // Switch to additive blending
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, 
                SamplerState.LinearClamp, DepthStencilState.None, 
                RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            DrawAstrologicalRing(spriteBatch);

            // Return to normal blending
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, 
                SamplerState.LinearClamp, DepthStencilState.None, 
                RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            return false;
        }

        private void DrawAstrologicalRing(SpriteBatch spriteBatch)
        {
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            Texture2D glow = TextureAssets.Extra[98].Value;
            
            int phase = GetCurrentPhase();
            float drawProgress = GetDrawProgress();
            float glowProgress = phase >= 1 ? GetGlowProgress() : 0f;
            float igniteProgress = phase >= 2 ? GetIgniteProgress() : 0f;
            float fadeProgress = phase >= 3 ? GetFadeProgress() : 0f;
            
            float overallAlpha = phase == 3 ? (1f - fadeProgress) : 1f;
            float glowIntensity = phase == 1 ? (0.5f + glowProgress * 0.5f) : (phase == 2 ? (1f - igniteProgress * 0.5f) : 0.5f);
            
            Vector2 center = Projectile.Center - Main.screenPosition;

            // === DRAW OUTER RING SEGMENTS ===
            int segmentsToDraw = (int)(SymbolSegments * drawProgress);
            if (phase >= 1) segmentsToDraw = SymbolSegments;

            for (int i = 0; i < segmentsToDraw; i++)
            {
                float segmentAngle = RingRotation + MathHelper.TwoPi * i / SymbolSegments;
                float nextAngle = RingRotation + MathHelper.TwoPi * (i + 1) / SymbolSegments;
                float segmentProgress = (float)i / SymbolSegments;
                
                // Draw arc segment
                DrawRingArc(spriteBatch, pixel, center, BaseRadius, segmentAngle, nextAngle, 
                    GetFateGradient(segmentProgress) * overallAlpha * glowIntensity, 4f);
                
                // Draw segment node
                Vector2 nodePos = center + segmentAngle.ToRotationVector2() * BaseRadius;
                Color nodeColor = GetFateGradient(segmentProgress) * overallAlpha;
                
                // Node glow
                spriteBatch.Draw(glow, nodePos, null, nodeColor * 0.6f * glowIntensity, 0f, 
                    glow.Size() / 2f, 0.15f * glowIntensity, SpriteEffects.None, 0f);
                spriteBatch.Draw(glow, nodePos, null, FateWhite * 0.4f * overallAlpha * glowIntensity, 0f, 
                    glow.Size() / 2f, 0.08f, SpriteEffects.None, 0f);
            }

            // === DRAW INNER CONCENTRIC RINGS ===
            for (int ring = 0; ring < InnerRings; ring++)
            {
                float ringRadius = BaseRadius * (0.4f + ring * 0.2f);
                float ringRotation = RingRotation * (ring % 2 == 0 ? 1 : -1.5f);
                Color ringColor = GetFateGradient((float)ring / InnerRings) * overallAlpha * 0.7f * glowIntensity;
                
                DrawFullRing(spriteBatch, pixel, center, ringRadius, ringColor, 2f, 32);
            }

            // === DRAW CONNECTING CONSTELLATION LINES ===
            if (drawProgress > 0.5f)
            {
                // Star of David pattern (6-pointed)
                for (int i = 0; i < 6; i++)
                {
                    float angle1 = RingRotation + MathHelper.TwoPi * i / 6f;
                    float angle2 = RingRotation + MathHelper.TwoPi * ((i + 2) % 6) / 6f;
                    
                    Vector2 point1 = center + angle1.ToRotationVector2() * BaseRadius * 0.6f;
                    Vector2 point2 = center + angle2.ToRotationVector2() * BaseRadius * 0.6f;
                    
                    DrawLine(spriteBatch, pixel, point1, point2, FateDarkPink * overallAlpha * 0.5f * glowIntensity, 2f);
                }
            }

            // === DRAW CENTRAL SYMBOL ===
            float centralScale = 0.25f * glowIntensity;
            
            // Inner glow
            spriteBatch.Draw(glow, center, null, FateDarkPink * 0.5f * overallAlpha, 0f, 
                glow.Size() / 2f, centralScale * 2f, SpriteEffects.None, 0f);
            spriteBatch.Draw(glow, center, null, FateBrightRed * 0.4f * overallAlpha, 0f, 
                glow.Size() / 2f, centralScale * 1.5f, SpriteEffects.None, 0f);
            spriteBatch.Draw(glow, center, null, FateWhite * 0.6f * overallAlpha, 0f, 
                glow.Size() / 2f, centralScale * 0.8f, SpriteEffects.None, 0f);
        }

        private void DrawRingArc(SpriteBatch spriteBatch, Texture2D tex, Vector2 center, 
            float radius, float startAngle, float endAngle, Color color, float thickness)
        {
            int segments = 8;
            for (int i = 0; i < segments; i++)
            {
                float t1 = (float)i / segments;
                float t2 = (float)(i + 1) / segments;
                float angle1 = MathHelper.Lerp(startAngle, endAngle, t1);
                float angle2 = MathHelper.Lerp(startAngle, endAngle, t2);
                
                Vector2 point1 = center + angle1.ToRotationVector2() * radius;
                Vector2 point2 = center + angle2.ToRotationVector2() * radius;
                
                DrawLine(spriteBatch, tex, point1, point2, color, thickness);
            }
        }

        private void DrawFullRing(SpriteBatch spriteBatch, Texture2D tex, Vector2 center, 
            float radius, Color color, float thickness, int segments)
        {
            for (int i = 0; i < segments; i++)
            {
                float angle1 = MathHelper.TwoPi * i / segments;
                float angle2 = MathHelper.TwoPi * (i + 1) / segments;
                
                Vector2 point1 = center + angle1.ToRotationVector2() * radius;
                Vector2 point2 = center + angle2.ToRotationVector2() * radius;
                
                DrawLine(spriteBatch, tex, point1, point2, color, thickness);
            }
        }

        private void DrawLine(SpriteBatch spriteBatch, Texture2D tex, Vector2 start, Vector2 end, Color color, float thickness)
        {
            Vector2 direction = end - start;
            float length = direction.Length();
            float rotation = direction.ToRotation();
            
            spriteBatch.Draw(tex, start, new Rectangle(0, 0, 1, 1), color, 
                rotation, new Vector2(0, 0.5f), new Vector2(length, thickness), SpriteEffects.None, 0f);
        }
    }
}
