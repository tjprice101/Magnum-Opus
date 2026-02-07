using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Common.Systems.VFX
{
    /// <summary>
    /// UNIVERSAL ELEMENTAL VFX SYSTEM
    /// 
    /// Provides elemental visual effects usable across ALL content types:
    /// - Weapons (melee trails, ranged muzzle flash, magic channeling)
    /// - Accessories (auras, proc effects)
    /// - Bosses (attack visuals, phase transitions)
    /// - Enemies (death effects, attack patterns)
    /// - Projectiles (trails, impacts, explosions)
    /// 
    /// ============================================
    /// DESIGN PHILOSOPHY - USES EXISTING SYSTEMS
    /// ============================================
    /// 
    /// This system BUILDS UPON the existing VFX infrastructure:
    /// 
    /// - InterpolatedRenderer: For 144Hz+ smooth positions
    /// - EnhancedTrailRenderer: For primitive trail rendering with width/color functions
    /// - BloomRenderer: For multi-layer bloom stacking
    /// - GodRaySystem: For volumetric light rays
    /// - ImpactLightRays: For impact flares with easing
    /// - ProceduralProjectileVFX: For PNG-free procedural shapes
    /// - ShaderRenderer: For shader-based effects
    /// - ScreenDistortionManager: For screen-space distortions
    /// 
    /// NEVER just spawn raw Dust/draw PNGs directly - use these systems for polish!
    /// The only exception is minimal Dust for performance-critical dense particle effects.
    /// </summary>
    public static class UniversalElementalVFX
    {
        #region ========== FLAMES ==========

        /// <summary>
        /// Creates flowing flame trail using primitive rendering.
        /// Uses EnhancedTrailRenderer for smooth, GPU-accelerated trails.
        /// </summary>
        public static void FlowingFlameTrail(
            Vector2[] positions, 
            float[] rotations,
            Color innerColor, 
            Color outerColor, 
            float width = 20f,
            float intensity = 1f)
        {
            if (positions == null || positions.Length < 2) return;
            
            // Use EnhancedTrailRenderer for proper primitive trails
            var settings = new EnhancedTrailRenderer.PrimitiveSettings(
                EnhancedTrailRenderer.QuadraticBumpWidth(width * intensity),
                completionRatio => 
                {
                    Color baseColor = Color.Lerp(innerColor, outerColor, completionRatio);
                    float alpha = (1f - completionRatio) * intensity;
                    return baseColor.WithoutAlpha() * alpha;
                },
                smoothen: true
            );
            
            // Render the trail with bloom settings
            EnhancedTrailRenderer.RenderTrail(positions, settings);
            
            // Add flickering dynamic lighting along trail
            for (int i = 0; i < positions.Length; i += 3)
            {
                float flicker = 0.7f + Main.rand.NextFloat(0.3f);
                float progress = (float)i / positions.Length;
                Lighting.AddLight(positions[i], innerColor.ToVector3() * (1f - progress) * flicker * intensity);
            }
        }

        /// <summary>
        /// Creates infernal eruption effect using GodRays + procedural rendering.
        /// </summary>
        public static void InfernalEruption(
            Vector2 position, 
            Color innerColor, 
            Color outerColor, 
            float scale = 1f,
            bool includeSmoke = true)
        {
            // Use GodRaySystem for volumetric light rays
            GodRaySystem.CreateBurst(
                position,
                innerColor,
                rayCount: (int)(12 * scale),
                radius: 180f * scale,
                duration: 35,
                GodRaySystem.GodRayStyle.Explosion,
                outerColor
            );
            
            // Use ImpactLightRays for the core flash
            ImpactLightRays.SpawnImpactRays(position, innerColor, outerColor, 
                rayCount: 6, scale: scale * 1.2f);
            
            // Use BloomRenderer for the central bloom
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            BloomRenderer.DrawBloomStack(Main.spriteBatch, position, innerColor, outerColor, scale * 1.5f);
            
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            // Smoke uses dust sparingly for performance
            if (includeSmoke)
            {
                for (int i = 0; i < 8 * scale; i++)
                {
                    Vector2 smokeVel = new Vector2(Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-4f, -1f));
                    Dust smoke = Dust.NewDustPerfect(position + Main.rand.NextVector2Circular(20f, 10f) * scale, 
                        DustID.Smoke, smokeVel, 100, Color.Black, 1.5f * scale);
                    smoke.noGravity = true;
                }
            }
            
            // Trigger screen distortion for larger eruptions
            if (scale >= 1.5f)
            {
                ScreenDistortionManager.TriggerRipple(position, innerColor, scale * 0.3f, 15);
            }
            
            Lighting.AddLight(position, innerColor.ToVector3() * 2f * scale);
        }

        /// <summary>
        /// Creates burning aura using procedural rendering with interpolation.
        /// </summary>
        public static void BurningAura(
            Vector2 center, 
            float radius, 
            Color color, 
            float intensity = 1f)
        {
            // Update interpolation for smooth rendering
            InterpolatedRenderer.UpdatePartialTicks();
            float partialTicks = InterpolatedRenderer.PartialTicks;
            float time = Main.GlobalTimeWrappedHourly * 3f + partialTicks * 0.05f;
            
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            // Draw orbiting flame points using BloomRenderer
            int flameCount = (int)(8 * intensity);
            for (int i = 0; i < flameCount; i++)
            {
                float angle = time + MathHelper.TwoPi * i / flameCount;
                float wobble = MathF.Sin(time * 3f + i) * 0.15f;
                float dist = radius * (0.8f + wobble);
                Vector2 flamePos = center + angle.ToRotationVector2() * dist;
                
                // Use BloomRenderer for each flame point
                float flameIntensity = 0.6f + MathF.Sin(time * 5f + i * 1.3f) * 0.2f;
                BloomRenderer.DrawSimpleBloom(Main.spriteBatch, flamePos, color, 
                    0.3f * intensity * flameIntensity, 0.7f);
            }
            
            // Central glow
            BloomRenderer.DrawBreathingBloom(Main.spriteBatch, center, color * 0.5f, 
                radius / 50f * intensity, 1.5f);
            
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            Lighting.AddLight(center, color.ToVector3() * intensity);
        }

        /// <summary>
        /// Simple fire burst using bloom layers - lightweight version.
        /// </summary>
        public static void FireBurst(Vector2 position, Color color, float scale = 1f)
        {
            // Use GodRays for burst
            GodRaySystem.CreateBurst(position, color, 8, 100f * scale, 25, GodRaySystem.GodRayStyle.Explosion);
            
            // Bloom center
            BloomRenderer.DrawBloomStack(Main.spriteBatch, position, color, Color.White, scale);
            
            Lighting.AddLight(position, color.ToVector3() * scale);
        }

        #endregion

        #region ========== LIGHTNING ==========

        /// <summary>
        /// Creates lightning strike using primitive line rendering with interpolation.
        /// </summary>
        public static void LightningStrike(
            Vector2 start, 
            Vector2 end, 
            Color color, 
            float thickness = 3f,
            float jaggedness = 30f,
            int branchCount = 3)
        {
            InterpolatedRenderer.UpdatePartialTicks();
            
            // Generate lightning path with jitter
            List<Vector2> points = GenerateLightningPath(start, end, jaggedness);
            float[] rotations = new float[points.Count];
            for (int i = 0; i < points.Count - 1; i++)
            {
                rotations[i] = (points[i + 1] - points[i]).ToRotation();
            }
            if (points.Count > 0) rotations[points.Count - 1] = rotations[Math.Max(0, points.Count - 2)];
            
            // Use EnhancedTrailRenderer for the main bolt
            var settings = new EnhancedTrailRenderer.PrimitiveSettings(
                EnhancedTrailRenderer.QuadraticBumpWidth(thickness),
                completionRatio => color.WithoutAlpha() * (1f - completionRatio * 0.3f),
                smoothen: false // Lightning should be jagged, not smooth
            );
            
            EnhancedTrailRenderer.RenderTrail(points.ToArray(), settings);
            
            // Add branches
            for (int b = 0; b < branchCount; b++)
            {
                int branchStartIdx = Main.rand.Next(points.Count / 3, points.Count * 2 / 3);
                if (branchStartIdx >= points.Count) continue;
                
                Vector2 branchStart = points[branchStartIdx];
                Vector2 branchEnd = branchStart + Main.rand.NextVector2Unit() * Main.rand.NextFloat(40f, 100f);
                
                LightningBranch(branchStart, branchEnd, color * 0.7f, thickness * 0.5f, jaggedness * 0.6f);
            }
            
            // Impact bloom at end point
            BloomRenderer.DrawBloomStack(Main.spriteBatch, end, color, Color.White, 0.8f);
            
            // Lighting along the bolt
            foreach (var point in points)
            {
                Lighting.AddLight(point, color.ToVector3() * 0.8f);
            }
        }

        /// <summary>
        /// Creates a single lightning branch (no sub-branches).
        /// </summary>
        public static void LightningBranch(
            Vector2 start, 
            Vector2 end, 
            Color color, 
            float thickness = 2f,
            float jaggedness = 20f)
        {
            List<Vector2> points = GenerateLightningPath(start, end, jaggedness, 4);
            float[] rotations = new float[points.Count];
            for (int i = 0; i < points.Count - 1; i++)
            {
                rotations[i] = (points[i + 1] - points[i]).ToRotation();
            }
            if (points.Count > 0) rotations[points.Count - 1] = rotations[Math.Max(0, points.Count - 2)];
            
            var settings = new EnhancedTrailRenderer.PrimitiveSettings(
                EnhancedTrailRenderer.LinearTaper(thickness),
                completionRatio => color.WithoutAlpha() * (1f - completionRatio * 0.5f),
                smoothen: false
            );
            
            EnhancedTrailRenderer.RenderTrail(points.ToArray(), settings);
        }

        /// <summary>
        /// Creates chain lightning between multiple points.
        /// </summary>
        public static void ChainLightning(
            Vector2[] positions, 
            Color color, 
            float thickness = 2.5f)
        {
            if (positions == null || positions.Length < 2) return;
            
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            for (int i = 0; i < positions.Length - 1; i++)
            {
                LightningBranch(positions[i], positions[i + 1], color, thickness, 25f);
                
                // Small bloom at each node
                BloomRenderer.DrawSimpleBloom(Main.spriteBatch, positions[i], color, 0.4f);
            }
            
            // Larger bloom at final target
            BloomRenderer.DrawBloomStack(Main.spriteBatch, positions[positions.Length - 1], color, 0.6f);
            
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }

        /// <summary>
        /// Creates static arcs around a point using procedural rendering.
        /// </summary>
        public static void StaticArcs(
            Vector2 center, 
            float radius, 
            Color color, 
            int arcCount = 4)
        {
            float time = Main.GlobalTimeWrappedHourly;
            
            for (int i = 0; i < arcCount; i++)
            {
                if (!Main.rand.NextBool(3)) continue; // Sporadic appearance
                
                float angle = time * 2f + MathHelper.TwoPi * i / arcCount + Main.rand.NextFloat(-0.5f, 0.5f);
                Vector2 arcStart = center + angle.ToRotationVector2() * radius * 0.3f;
                Vector2 arcEnd = center + angle.ToRotationVector2() * radius;
                
                LightningBranch(arcStart, arcEnd, color, 1.5f, 15f);
            }
        }

        /// <summary>
        /// Simple electrical burst using bloom - lightweight version.
        /// </summary>
        public static void ElectricalBurst(Vector2 position, Color color, float scale = 1f)
        {
            // Use pulsing god rays for electrical feel
            GodRaySystem.CreateBurst(position, color, 10, 80f * scale, 20, GodRaySystem.GodRayStyle.Pulsing);
            
            BloomRenderer.DrawBloomStack(Main.spriteBatch, position, color, Color.White, scale * 0.8f);
            
            Lighting.AddLight(position, color.ToVector3() * 1.2f * scale);
        }

        private static List<Vector2> GenerateLightningPath(Vector2 start, Vector2 end, float jaggedness, int segments = 8)
        {
            List<Vector2> points = new List<Vector2> { start };
            Vector2 direction = end - start;
            Vector2 perpendicular = new Vector2(-direction.Y, direction.X);
            if (perpendicular.Length() > 0) perpendicular.Normalize();
            
            for (int i = 1; i < segments; i++)
            {
                float t = (float)i / segments;
                Vector2 basePoint = Vector2.Lerp(start, end, t);
                float offset = Main.rand.NextFloat(-jaggedness, jaggedness) * (1f - Math.Abs(t - 0.5f) * 2f);
                points.Add(basePoint + perpendicular * offset);
            }
            
            points.Add(end);
            return points;
        }

        #endregion

        #region ========== PETALS / BLOSSOMS ==========

        /// <summary>
        /// Creates petal blossom explosion using GodRays + bloom.
        /// </summary>
        public static void PetalBlossomExplosion(
            Vector2 position, 
            Color[] colors, 
            int count = 12, 
            float scale = 1f)
        {
            if (colors == null || colors.Length == 0) colors = new[] { Color.Pink };
            
            // Use GodRays for the burst effect
            GodRaySystem.CreateBurst(
                position,
                colors[0],
                rayCount: count,
                radius: 120f * scale,
                duration: 40,
                GodRaySystem.GodRayStyle.Explosion,
                colors.Length > 1 ? colors[1] : colors[0]
            );
            
            // Central bloom
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            BloomRenderer.DrawBloomStack(Main.spriteBatch, position, colors[0], scale);
            
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            // Dust petals for added density (sparingly)
            for (int i = 0; i < count / 2; i++)
            {
                Vector2 vel = Main.rand.NextVector2Unit() * Main.rand.NextFloat(3f, 8f) * scale;
                Color petalColor = colors[Main.rand.Next(colors.Length)];
                Dust petal = Dust.NewDustPerfect(position, DustID.PinkFairy, vel, 0, petalColor, 1.2f * scale);
                petal.noGravity = true;
                petal.fadeIn = 1.3f;
            }
            
            Lighting.AddLight(position, colors[0].ToVector3() * scale);
        }

        /// <summary>
        /// Creates petal trail using primitive rendering.
        /// </summary>
        public static void PetalTrail(
            Vector2[] positions,
            float[] rotations,
            Color color, 
            float intensity = 1f)
        {
            if (positions == null || positions.Length < 2) return;
            
            // Soft, flowing trail
            var settings = new EnhancedTrailRenderer.PrimitiveSettings(
                EnhancedTrailRenderer.LinearTaper(12f * intensity),
                completionRatio => 
                {
                    float alpha = MathF.Sin(completionRatio * MathHelper.Pi) * intensity;
                    return color.WithoutAlpha() * alpha * 0.6f;
                },
                smoothen: true
            );
            
            EnhancedTrailRenderer.RenderTrail(positions, settings);
        }

        /// <summary>
        /// Creates petal vortex around a point using procedural drawing.
        /// </summary>
        public static void PetalVortex(
            Vector2 center, 
            float radius, 
            Color color, 
            float rotationSpeed = 0.02f,
            int petalCount = 8)
        {
            InterpolatedRenderer.UpdatePartialTicks();
            float time = Main.GlobalTimeWrappedHourly;
            float rotation = time * rotationSpeed * 60f;
            
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            for (int i = 0; i < petalCount; i++)
            {
                float angle = rotation + MathHelper.TwoPi * i / petalCount;
                float spiralOffset = MathF.Sin(time * 2f + i) * 0.2f;
                float dist = radius * (0.6f + spiralOffset);
                Vector2 petalPos = center + angle.ToRotationVector2() * dist;
                
                // Draw petal as elongated bloom
                float petalScale = 0.2f + MathF.Sin(time * 3f + i * 0.5f) * 0.05f;
                BloomRenderer.DrawSimpleBloom(Main.spriteBatch, petalPos, color, petalScale, 0.6f);
            }
            
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }

        #endregion

        #region ========== LIGHT BEAMS ==========

        /// <summary>
        /// Creates light beam burst using GodRaySystem.
        /// </summary>
        public static void LightBeamBurst(
            Vector2 position, 
            Color color, 
            int rayCount = 8, 
            float maxLength = 200f, 
            float scale = 1f)
        {
            GodRaySystem.CreateBurst(
                position,
                color,
                rayCount: rayCount,
                radius: maxLength * scale,
                duration: 30,
                GodRaySystem.GodRayStyle.Explosion
            );
            
            // Add ImpactLightRays for extra detail
            ImpactLightRays.SpawnImpactRays(position, color, Color.White, rayCount / 2, scale);
        }

        /// <summary>
        /// Creates prismatic refraction effect using multiple colored rays.
        /// </summary>
        public static void PrismaticRefraction(
            Vector2 position, 
            Vector2 direction, 
            float intensity = 1f,
            int rayCount = 6)
        {
            float baseAngle = direction.ToRotation();
            float spread = MathHelper.PiOver4;
            
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            for (int i = 0; i < rayCount; i++)
            {
                float hue = (float)i / rayCount;
                Color rayColor = Main.hslToRgb(hue, 1f, 0.7f);
                float angle = baseAngle + MathHelper.Lerp(-spread, spread, (float)i / (rayCount - 1));
                Vector2 rayEnd = position + angle.ToRotationVector2() * 150f * intensity;
                
                // Draw ray using simple bloom line
                Vector2 mid = Vector2.Lerp(position, rayEnd, 0.5f);
                BloomRenderer.DrawSimpleBloom(Main.spriteBatch, mid, rayColor, 0.3f * intensity);
            }
            
            // Central white flash
            BloomRenderer.DrawBloomStack(Main.spriteBatch, position, Color.White, 0.5f * intensity);
            
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }

        /// <summary>
        /// Creates focused light beam using primitive rendering.
        /// </summary>
        public static void FocusedLightBeam(
            Vector2 start, 
            Vector2 end, 
            Color color, 
            float width = 8f,
            float pulseRate = 2f)
        {
            float time = Main.GlobalTimeWrappedHourly;
            float pulse = 1f + MathF.Sin(time * pulseRate * MathHelper.TwoPi) * 0.15f;
            
            float rotation = (end - start).ToRotation();
            
            // Create simple two-point trail
            Vector2[] positions = { start, end };
            float[] rotations = { rotation, rotation };
            
            var settings = new EnhancedTrailRenderer.PrimitiveSettings(
                EnhancedTrailRenderer.ConstantWithFade(width * pulse, 0.9f),
                completionRatio => color.WithoutAlpha() * (1f - completionRatio * 0.2f),
                smoothen: true
            );
            
            EnhancedTrailRenderer.RenderTrail(positions, settings);
            
            // End point bloom
            BloomRenderer.DrawBloomStack(Main.spriteBatch, end, color, 0.4f * pulse);
            
            Lighting.AddLight(start, color.ToVector3() * 0.5f);
            Lighting.AddLight(end, color.ToVector3() * 0.8f);
        }

        #endregion

        #region ========== FROST / ICE ==========

        /// <summary>
        /// Creates crystalline ice burst using ImpactLightRays + bloom.
        /// </summary>
        public static void CrystallineIceBurst(
            Vector2 position, 
            Color color, 
            float scale = 1f,
            int shardCount = 8)
        {
            // Ice-specific GodRays (sharper, more geometric)
            GodRaySystem.CreateBurst(
                position,
                color,
                rayCount: shardCount,
                radius: 100f * scale,
                duration: 25,
                GodRaySystem.GodRayStyle.Pulsing
            );
            
            // Impact rays for shimmering effect
            ImpactLightRays.SpawnImpactRays(position, color, Color.White, shardCount / 2, scale);
            
            // Central bloom
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            BloomRenderer.DrawBloomStack(Main.spriteBatch, position, color, Color.White, scale * 0.8f);
            
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            // Ice dust for crystalline feel
            for (int i = 0; i < shardCount; i++)
            {
                float angle = MathHelper.TwoPi * i / shardCount + Main.rand.NextFloat(-0.2f, 0.2f);
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 8f) * scale;
                Dust ice = Dust.NewDustPerfect(position, DustID.IceTorch, vel, 0, color, 1f * scale);
                ice.noGravity = true;
            }
            
            Lighting.AddLight(position, color.ToVector3() * scale);
        }

        /// <summary>
        /// Creates frozen trail using primitive rendering.
        /// </summary>
        public static void FrozenTrail(
            Vector2[] positions,
            float[] rotations,
            Color color, 
            float intensity = 1f)
        {
            if (positions == null || positions.Length < 2) return;
            
            // Crystalline, sharp-edged trail
            var settings = new EnhancedTrailRenderer.PrimitiveSettings(
                EnhancedTrailRenderer.InverseLerpBumpWidth(2f, 15f * intensity, 0.15f, 0.85f),
                completionRatio => 
                {
                    Color iceColor = Color.Lerp(color, Color.White, completionRatio * 0.3f);
                    return iceColor.WithoutAlpha() * (1f - completionRatio) * intensity;
                },
                smoothen: false // Sharp, crystalline edges
            );
            
            EnhancedTrailRenderer.RenderTrail(positions, settings);
        }

        /// <summary>
        /// Creates frost aura around a point.
        /// </summary>
        public static void FrostAura(Vector2 center, float radius, Color color, float intensity = 1f)
        {
            InterpolatedRenderer.UpdatePartialTicks();
            float time = Main.GlobalTimeWrappedHourly;
            
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            // Slow-orbiting ice crystals
            int crystalCount = 6;
            for (int i = 0; i < crystalCount; i++)
            {
                float angle = time * 0.5f + MathHelper.TwoPi * i / crystalCount;
                float dist = radius * (0.7f + MathF.Sin(time + i) * 0.2f);
                Vector2 crystalPos = center + angle.ToRotationVector2() * dist;
                
                float shimmer = 0.5f + MathF.Sin(time * 3f + i * 1.5f) * 0.3f;
                BloomRenderer.DrawSimpleBloom(Main.spriteBatch, crystalPos, color, 0.2f * intensity * shimmer);
            }
            
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            Lighting.AddLight(center, color.ToVector3() * 0.5f * intensity);
        }

        #endregion

        #region ========== VOID / COSMIC ==========

        /// <summary>
        /// Creates void energy burst with screen distortion.
        /// </summary>
        public static void VoidEnergyBurst(
            Vector2 position, 
            Color[] colors, 
            float scale = 1f)
        {
            if (colors == null || colors.Length == 0) colors = new[] { new Color(80, 20, 120) };
            
            // Screen distortion for void effect
            ScreenDistortionManager.TriggerRipple(position, colors[0], scale * 0.4f, 20);
            
            // Inward-spiraling god rays for void feel
            GodRaySystem.CreateBurst(
                position,
                colors[0],
                rayCount: 10,
                radius: 140f * scale,
                duration: 35,
                GodRaySystem.GodRayStyle.Spiral,
                colors.Length > 1 ? colors[1] : colors[0]
            );
            
            // Dark core with bright edge
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            BloomRenderer.DrawBloomStack(Main.spriteBatch, position, 
                colors[0], colors.Length > 1 ? colors[1] : Color.White, scale);
            
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            // Void particles (use purple torch for efficiency)
            for (int i = 0; i < 6 * scale; i++)
            {
                Vector2 vel = Main.rand.NextVector2Unit() * Main.rand.NextFloat(2f, 5f) * scale;
                Dust voidDust = Dust.NewDustPerfect(position + Main.rand.NextVector2Circular(20f, 20f) * scale,
                    DustID.PurpleTorch, vel, 0, colors[0], 1.2f * scale);
                voidDust.noGravity = true;
            }
            
            Lighting.AddLight(position, colors[0].ToVector3() * 0.5f * scale);
        }

        /// <summary>
        /// Creates cosmic star field effect using bloom rendering.
        /// </summary>
        public static void CosmicStarField(
            Vector2 center, 
            float radius, 
            int starCount = 20,
            float twinkleSpeed = 2f)
        {
            InterpolatedRenderer.UpdatePartialTicks();
            float time = Main.GlobalTimeWrappedHourly;
            
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            // Use seeded random for consistent star positions
            Random starRandom = new Random(42);
            
            for (int i = 0; i < starCount; i++)
            {
                float angle = (float)(starRandom.NextDouble() * MathHelper.TwoPi);
                float dist = (float)(starRandom.NextDouble() * 0.7f + 0.3f) * radius;
                Vector2 starPos = center + angle.ToRotationVector2() * dist;
                
                // Twinkle effect
                float twinkle = 0.5f + MathF.Sin(time * twinkleSpeed + i * 1.3f) * 0.5f;
                float starScale = 0.1f + (float)starRandom.NextDouble() * 0.15f;
                Color starColor = Color.Lerp(Color.White, new Color(200, 180, 255), (float)starRandom.NextDouble());
                
                BloomRenderer.DrawSimpleBloom(Main.spriteBatch, starPos, starColor, starScale * twinkle);
            }
            
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }

        /// <summary>
        /// Creates nebula cloud effect using layered bloom.
        /// </summary>
        public static void NebulaCloud(
            Vector2 position, 
            Vector2 velocity, 
            Color color, 
            float scale = 1f)
        {
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            // Multiple overlapping clouds for nebula effect
            for (int layer = 0; layer < 3; layer++)
            {
                float layerOffset = layer * 0.3f;
                Vector2 cloudPos = position + velocity * layer * 2f;
                float cloudScale = scale * (1f - layer * 0.2f);
                float cloudAlpha = 0.4f - layer * 0.1f;
                
                Color layerColor = Color.Lerp(color, Color.White, layerOffset);
                BloomRenderer.DrawSimpleBloom(Main.spriteBatch, cloudPos, layerColor, cloudScale, cloudAlpha);
            }
            
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            Lighting.AddLight(position, color.ToVector3() * 0.3f * scale);
        }

        /// <summary>
        /// Creates glyph burst effect for Fate/Enigma themes.
        /// </summary>
        public static void GlyphBurst(Vector2 position, Color color, float scale = 1f, int count = 6)
        {
            // Use spiral god rays for mystical feel
            GodRaySystem.CreateBurst(position, color, count, 100f * scale, 30, GodRaySystem.GodRayStyle.Spiral);
            
            // Central bloom with pulse
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            BloomRenderer.DrawPulsingBloom(Main.spriteBatch, position, color, scale * 0.6f, 2f);
            
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            
            Lighting.AddLight(position, color.ToVector3() * scale);
        }

        #endregion

        #region ========== THEME SHORTCUTS ==========

        /// <summary>La Campanella: Black smoke + orange fire eruption.</summary>
        public static void LaCampanellaFlames(Vector2 position, float scale = 1f)
        {
            InfernalEruption(position, new Color(255, 140, 40), new Color(30, 20, 25), scale, true);
        }

        /// <summary>Eroica: Sakura petal explosion.</summary>
        public static void EroicaPetals(Vector2 position, float scale = 1f)
        {
            Color[] sakuraColors = { new Color(255, 150, 180), new Color(255, 200, 220), new Color(255, 215, 0) };
            PetalBlossomExplosion(position, sakuraColors, 12, scale);
        }

        /// <summary>Fate: Cosmic void burst with stars and glyphs.</summary>
        public static void FateCosmic(Vector2 position, float scale = 1f)
        {
            Color[] fateColors = { new Color(200, 80, 120), new Color(30, 15, 40), Color.White };
            VoidEnergyBurst(position, fateColors, scale);
            CosmicStarField(position, 100f * scale, 15, 3f);
        }

        /// <summary>Enigma: Void lightning with purple-green accents.</summary>
        public static void EnigmaVoidLightning(Vector2 start, Vector2 end, float scale = 1f)
        {
            Color enigmaColor = Color.Lerp(new Color(140, 60, 200), new Color(50, 200, 100), 0.3f);
            LightningStrike(start, end, enigmaColor, 3f * scale, 35f * scale, 4);
        }

        /// <summary>Swan Lake: Prismatic rainbow refraction.</summary>
        public static void SwanLakePrismatic(Vector2 position, Vector2 direction, float scale = 1f)
        {
            PrismaticRefraction(position, direction, scale, 8);
        }

        /// <summary>Moonlight Sonata: Crystalline ice burst with lunar glow.</summary>
        public static void MoonlightFrost(Vector2 position, float scale = 1f)
        {
            CrystallineIceBurst(position, new Color(135, 180, 255), scale, 10);
        }

        /// <summary>Dies Irae: Crimson fire burst.</summary>
        public static void DiesIraeFire(Vector2 position, float scale = 1f)
        {
            InfernalEruption(position, new Color(180, 30, 30), new Color(255, 80, 40), scale, false);
        }

        /// <summary>Clair de Lune: Soft lunar light beams.</summary>
        public static void ClairDeLuneLight(Vector2 position, float scale = 1f)
        {
            LightBeamBurst(position, new Color(200, 220, 255), 6, 150f, scale);
        }

        #endregion
    }
}
