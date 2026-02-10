using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Common.Systems.VFX
{
    /// <summary>
    /// CALAMITY-STYLE VFX SYSTEM - The New Standard
    /// 
    /// This system completely replaces the old "TRUE VFX STANDARD" with buttery-smooth,
    /// shader-enhanced, interpolated visual effects inspired by Calamity Mod.
    /// 
    /// Core Features:
    /// - Sub-pixel interpolation for 144Hz+ smoothness
    /// - Bézier curve projectile paths
    /// - Multi-pass primitive trail rendering
    /// - Dynamic bloom with shader styles
    /// - Screen distortion effects
    /// - Theme-based color cycling
    /// - Music note integration (we're still a music mod!)
    /// 
    /// THIS REPLACES:
    /// - Old "slap a flare" approach
    /// - Static single-frame particles
    /// - Rigid straight-line trails
    /// - Generic puff impacts
    /// </summary>
    public static class CalamityStyleVFX
    {
        #region Melee Swing Variations
        
        /// <summary>
        /// Parameters that control how a melee swing looks and feels.
        /// Each weapon gets unique variations based on its identity.
        /// </summary>
        public struct MeleeSwingVariation
        {
            // === WHIPPING/SNAP FEEL ===
            public float AnticipationTime;   // 0.05-0.25: How long the slow start lasts (lower = snappier)
            public float FastPhaseStart;      // 0.15-0.35: When the fast phase begins
            public float FastPhaseEnd;        // 0.65-0.85: When the fast phase ends
            public float PeakVelocityBoost;   // 0.8-1.5: Multiplier for max velocity (higher = more whippy)
            
            // === BLADE STRETCHING ===
            public float StretchMultiplier;   // 0.3-0.8: How much the blade stretches at peak (0.5 = 50% longer)
            public float SineWobbleAmount;    // 5-20: Sine wave wobble intensity during swing
            
            // === TRAIL APPEARANCE ===
            public float TrailWidthBase;      // 12-25: Base width of the trail
            public float TrailWidthVelocityBonus; // 8-20: Extra width during fast swing
            public float BloomIntensity;      // 2.0-4.0: How bright the bloom is
            
            // === ENERGY BLUR ===
            public float EnergyBlurIntensity; // 0.4-1.0: Intensity of the motion blur offset effect
            public int EnergyBlurPoints;      // 3-6: Number of ghost flares for motion blur
            
            // === PARTICLES ===
            public int ParticlesPerFrameBase; // 1-3: Minimum particles per frame
            public int ParticlesPerFrameBonus; // 2-5: Extra particles during fast swing
            public float SparkleChance;       // 0.3-0.8: Chance for whip-crack sparkles (at high velocity)
            public float MusicNoteChance;     // 0.1-0.3: Chance for music note spawns
            
            // === ARC SHAPE ===
            public float ArcLength;           // 50-100: Total arc angle in degrees
            public int ArcPoints;             // 12-24: Smoothness of the arc curve
            
            /// <summary>
            /// Creates a default balanced swing variation.
            /// </summary>
            public static MeleeSwingVariation Default => new MeleeSwingVariation
            {
                AnticipationTime = 0.15f,
                FastPhaseStart = 0.25f,
                FastPhaseEnd = 0.75f,
                PeakVelocityBoost = 1.0f,
                StretchMultiplier = 0.5f,
                SineWobbleAmount = 10f,
                TrailWidthBase = 18f,
                TrailWidthVelocityBonus = 12f,
                BloomIntensity = 2.5f,
                EnergyBlurIntensity = 0.7f,
                EnergyBlurPoints = 4,
                ParticlesPerFrameBase = 2,
                ParticlesPerFrameBonus = 3,
                SparkleChance = 0.5f,
                MusicNoteChance = 0.15f,
                ArcLength = 75f,
                ArcPoints = 16
            };
            
            /// <summary>
            /// Heavy, slow, powerful weapons (greatswords, hammers)
            /// </summary>
            public static MeleeSwingVariation Heavy => new MeleeSwingVariation
            {
                AnticipationTime = 0.22f,  // Longer windup
                FastPhaseStart = 0.30f,
                FastPhaseEnd = 0.70f,
                PeakVelocityBoost = 1.3f,  // POWERFUL snap
                StretchMultiplier = 0.7f,  // Stretches more
                SineWobbleAmount = 15f,
                TrailWidthBase = 24f,       // Thick trail
                TrailWidthVelocityBonus = 16f,
                BloomIntensity = 3.2f,      // More bloom
                EnergyBlurIntensity = 0.9f,
                EnergyBlurPoints = 5,
                ParticlesPerFrameBase = 3,
                ParticlesPerFrameBonus = 4,
                SparkleChance = 0.6f,
                MusicNoteChance = 0.12f,
                ArcLength = 90f,            // Wider arc
                ArcPoints = 20
            };
            
            /// <summary>
            /// Fast, precise weapons (rapiers, short swords, daggers)
            /// </summary>
            public static MeleeSwingVariation Swift => new MeleeSwingVariation
            {
                AnticipationTime = 0.08f,  // Very quick start
                FastPhaseStart = 0.15f,    // Fast phase starts early
                FastPhaseEnd = 0.80f,      // Stays fast longer
                PeakVelocityBoost = 0.85f, // Less raw power, more speed
                StretchMultiplier = 0.35f, // Less stretch
                SineWobbleAmount = 6f,
                TrailWidthBase = 14f,      // Thinner trail
                TrailWidthVelocityBonus = 8f,
                BloomIntensity = 2.2f,
                EnergyBlurIntensity = 0.5f,
                EnergyBlurPoints = 3,
                ParticlesPerFrameBase = 2,
                ParticlesPerFrameBonus = 2,
                SparkleChance = 0.4f,
                MusicNoteChance = 0.18f,
                ArcLength = 55f,           // Tighter arc
                ArcPoints = 12
            };
            
            /// <summary>
            /// Magical/ethereal weapons (enchanted swords, staves used as melee)
            /// </summary>
            public static MeleeSwingVariation Ethereal => new MeleeSwingVariation
            {
                AnticipationTime = 0.18f,
                FastPhaseStart = 0.22f,
                FastPhaseEnd = 0.78f,
                PeakVelocityBoost = 1.1f,
                StretchMultiplier = 0.6f,
                SineWobbleAmount = 18f,    // More wobble (ethereal feel)
                TrailWidthBase = 20f,
                TrailWidthVelocityBonus = 14f,
                BloomIntensity = 3.5f,     // HIGH bloom
                EnergyBlurIntensity = 0.85f,
                EnergyBlurPoints = 6,      // More blur points
                ParticlesPerFrameBase = 3,
                ParticlesPerFrameBonus = 4,
                SparkleChance = 0.7f,      // More sparkles
                MusicNoteChance = 0.22f,   // More music notes
                ArcLength = 70f,
                ArcPoints = 18
            };
            
            /// <summary>
            /// Tools (pickaxes, axes, hammers) - practical, less flashy
            /// </summary>
            public static MeleeSwingVariation Tool => new MeleeSwingVariation
            {
                AnticipationTime = 0.12f,
                FastPhaseStart = 0.20f,
                FastPhaseEnd = 0.72f,
                PeakVelocityBoost = 0.9f,
                StretchMultiplier = 0.4f,
                SineWobbleAmount = 8f,
                TrailWidthBase = 15f,
                TrailWidthVelocityBonus = 10f,
                BloomIntensity = 2.0f,     // Less bloom
                EnergyBlurIntensity = 0.4f,
                EnergyBlurPoints = 3,
                ParticlesPerFrameBase = 1,
                ParticlesPerFrameBonus = 2,
                SparkleChance = 0.25f,     // Fewer sparkles
                MusicNoteChance = 0.08f,   // Fewer notes
                ArcLength = 65f,
                ArcPoints = 12
            };
            
            /// <summary>
            /// Epic/legendary endgame weapons - maximum spectacle
            /// </summary>
            public static MeleeSwingVariation Legendary => new MeleeSwingVariation
            {
                AnticipationTime = 0.14f,
                FastPhaseStart = 0.20f,
                FastPhaseEnd = 0.80f,
                PeakVelocityBoost = 1.4f,  // MAXIMUM snap
                StretchMultiplier = 0.8f,  // MAXIMUM stretch
                SineWobbleAmount = 14f,
                TrailWidthBase = 26f,      // Thick trail
                TrailWidthVelocityBonus = 18f,
                BloomIntensity = 4.0f,     // MAXIMUM bloom
                EnergyBlurIntensity = 1.0f,
                EnergyBlurPoints = 6,
                ParticlesPerFrameBase = 4,
                ParticlesPerFrameBonus = 5,
                SparkleChance = 0.8f,      // Lots of sparkles
                MusicNoteChance = 0.25f,   // Lots of notes
                ArcLength = 85f,
                ArcPoints = 22
            };
            
            /// <summary>
            /// Creates a variation with random subtle tweaks based on a seed.
            /// Ensures each weapon feels slightly different even within the same category.
            /// </summary>
            public static MeleeSwingVariation WithRandomTweaks(MeleeSwingVariation baseVariation, int seed)
            {
                var rand = new Random(seed);
                float Tweak(float val, float range) => val * (1f + (float)(rand.NextDouble() * 2 - 1) * range);
                
                return new MeleeSwingVariation
                {
                    AnticipationTime = Tweak(baseVariation.AnticipationTime, 0.15f),
                    FastPhaseStart = Tweak(baseVariation.FastPhaseStart, 0.1f),
                    FastPhaseEnd = Tweak(baseVariation.FastPhaseEnd, 0.08f),
                    PeakVelocityBoost = Tweak(baseVariation.PeakVelocityBoost, 0.12f),
                    StretchMultiplier = Tweak(baseVariation.StretchMultiplier, 0.15f),
                    SineWobbleAmount = Tweak(baseVariation.SineWobbleAmount, 0.2f),
                    TrailWidthBase = Tweak(baseVariation.TrailWidthBase, 0.1f),
                    TrailWidthVelocityBonus = Tweak(baseVariation.TrailWidthVelocityBonus, 0.15f),
                    BloomIntensity = Tweak(baseVariation.BloomIntensity, 0.1f),
                    EnergyBlurIntensity = Tweak(baseVariation.EnergyBlurIntensity, 0.12f),
                    EnergyBlurPoints = baseVariation.EnergyBlurPoints + rand.Next(-1, 2),
                    ParticlesPerFrameBase = Math.Max(1, baseVariation.ParticlesPerFrameBase + rand.Next(-1, 2)),
                    ParticlesPerFrameBonus = Math.Max(1, baseVariation.ParticlesPerFrameBonus + rand.Next(-1, 2)),
                    SparkleChance = MathHelper.Clamp(Tweak(baseVariation.SparkleChance, 0.15f), 0.1f, 0.9f),
                    MusicNoteChance = MathHelper.Clamp(Tweak(baseVariation.MusicNoteChance, 0.2f), 0.05f, 0.35f),
                    ArcLength = Tweak(baseVariation.ArcLength, 0.1f),
                    ArcPoints = Math.Max(10, baseVariation.ArcPoints + rand.Next(-2, 3))
                };
            }
        }
        
        /// <summary>
        /// Gets the appropriate swing variation for a weapon based on its properties.
        /// </summary>
        public static MeleeSwingVariation GetVariationForItem(Item item)
        {
            if (item == null) return MeleeSwingVariation.Default;
            
            // Use item type as seed for consistent per-weapon randomization
            int seed = item.type * 31337;
            
            // Determine base variation from item properties
            MeleeSwingVariation baseVar;
            
            string itemName = item.Name?.ToLowerInvariant() ?? "";
            string className = item.GetType().Name.ToLowerInvariant();
            
            // Check for legendary/endgame weapons (Fate, Coda, Opus, etc.)
            if (itemName.Contains("coda") || itemName.Contains("opus") || itemName.Contains("requiem") ||
                itemName.Contains("constellation") || itemName.Contains("fractal") ||
                className.Contains("coda") || className.Contains("opus"))
            {
                baseVar = MeleeSwingVariation.Legendary;
            }
            // Check for tools (pickaxe, axe, hammer in name or class)
            else if (item.pick > 0 || item.axe > 0 || item.hammer > 0 ||
                     itemName.Contains("pickaxe") || itemName.Contains("hammer") || itemName.Contains("axe"))
            {
                baseVar = MeleeSwingVariation.Tool;
            }
            // Check for heavy weapons (slow attack speed or big size)
            else if (item.useTime >= 35 || item.width >= 60 || item.height >= 60 ||
                     itemName.Contains("great") || itemName.Contains("colossal") ||
                     itemName.Contains("executioner") || itemName.Contains("cleaver"))
            {
                baseVar = MeleeSwingVariation.Heavy;
            }
            // Check for swift weapons (fast attack speed or small size)
            else if (item.useTime <= 15 || (item.width <= 35 && item.height <= 35) ||
                     itemName.Contains("rapier") || itemName.Contains("dagger") ||
                     itemName.Contains("knife") || itemName.Contains("swift"))
            {
                baseVar = MeleeSwingVariation.Swift;
            }
            // Check for ethereal/magical weapons
            else if (itemName.Contains("eternal") || itemName.Contains("ethereal") ||
                     itemName.Contains("moon") || itemName.Contains("star") ||
                     itemName.Contains("cosmic") || itemName.Contains("celestial") ||
                     itemName.Contains("enigma") || itemName.Contains("void"))
            {
                baseVar = MeleeSwingVariation.Ethereal;
            }
            else
            {
                baseVar = MeleeSwingVariation.Default;
            }
            
            // Apply random tweaks for uniqueness
            return MeleeSwingVariation.WithRandomTweaks(baseVar, seed);
        }
        
        #endregion
        
        #region Weapon Swing Effects

        /// <summary>
        /// Creates a buttery-smooth melee swing arc with primitive trails.
        /// Uses interpolation for 144Hz+ smoothness.
        /// 
        /// ENHANCED: Now includes Exo Blade-style whipping motion and blade stretching!
        /// - SlowStart → FastSwing → EaseOut creates a "whip crack" feel
        /// - Blade STRETCHES during peak velocity (expands outward)
        /// - Energy blur offset during fast phase
        /// - Motion blur particles follow swing velocity
        /// 
        /// Each weapon has UNIQUE variations for a distinct feel!
        /// </summary>
        public static void SmoothMeleeSwing(Player player, string theme, float swingProgress, Vector2 swingDirection, float arcLength = 60f)
        {
            // Use default variation for backwards compatibility
            SmoothMeleeSwing(player, theme, swingProgress, swingDirection, MeleeSwingVariation.Default, arcLength);
        }
        
        /// <summary>
        /// Creates a buttery-smooth melee swing arc with weapon-specific variations.
        /// </summary>
        public static void SmoothMeleeSwing(Player player, string theme, float swingProgress, Vector2 swingDirection, MeleeSwingVariation variation, float arcLengthOverride = 0f)
        {
            var palette = MagnumThemePalettes.GetThemePalette(theme);
            if (palette == null || palette.Length < 2) return;
            
            Color primary = palette[0];
            Color secondary = palette.Length > 1 ? palette[1] : primary;
            Color tertiary = palette.Length > 2 ? palette[2] : secondary;
            
            // Use variation's arc length unless override provided
            float arcLength = arcLengthOverride > 0f ? arcLengthOverride : variation.ArcLength;
            
            // Calculate swing arc positions using smooth interpolation
            float swingAngle = swingDirection.ToRotation();
            float startAngle = swingAngle - MathHelper.ToRadians(arcLength / 2f);
            float currentAngle = startAngle + MathHelper.ToRadians(arcLength) * swingProgress;
            
            // === EXO BLADE WHIPPING EFFECT ===
            // Calculate instantaneous swing velocity using variation parameters
            float swingVelocity = GetSwingVelocityVaried(swingProgress, variation) * variation.PeakVelocityBoost;
            
            // === BLADE STRETCHING EFFECT ===
            // Blade extends OUTWARD during peak velocity (like a whip stretching)
            float baseLength = 60f;
            float stretchFactor = swingVelocity * variation.StretchMultiplier;
            float sineWobble = (float)Math.Sin(swingProgress * MathHelper.Pi) * variation.SineWobbleAmount;
            float bladeLength = baseLength * (1f + stretchFactor) + sineWobble;
            
            Vector2 tipPos = player.Center + currentAngle.ToRotationVector2() * bladeLength;
            
            // === PRIMITIVE TRAIL ARC WITH DYNAMIC WIDTH ===
            int arcPoints = variation.ArcPoints;
            Vector2[] arcPositions = new Vector2[arcPoints];
            for (int i = 0; i < arcPoints; i++)
            {
                float t = i / (float)(arcPoints - 1) * swingProgress;
                float angle = startAngle + MathHelper.ToRadians(arcLength) * t;
                
                // Length varies along trail with stretching
                float trailVelocity = GetSwingVelocityVaried(t, variation);
                float trailStretch = trailVelocity * variation.StretchMultiplier * 0.8f;
                float trailLength = baseLength * (1f + trailStretch) + (float)Math.Sin(t * MathHelper.Pi) * variation.SineWobbleAmount;
                
                arcPositions[i] = player.Center + angle.ToRotationVector2() * trailLength;
            }
            
            // Dynamic trail width based on velocity
            float trailWidth = variation.TrailWidthBase + swingVelocity * variation.TrailWidthVelocityBonus;
            
            // Render multi-pass trail with dynamic width (reduced bloom for subtler effect)
            EnhancedTrailRenderer.RenderMultiPassTrail(
                arcPositions,
                EnhancedTrailRenderer.QuadraticBumpWidth(trailWidth * 0.7f),
                EnhancedTrailRenderer.GradientColor(primary, secondary, 0.6f),
                bloomMultiplier: variation.BloomIntensity * 0.4f + swingVelocity * 0.3f,
                coreMultiplier: 0.25f
            );
            
            // === FOG EFFECT (replaces bright flares) ===
            // Spawn fog along the swing arc for atmospheric effect
            if (swingProgress > 0.1f && swingProgress < 0.9f)
            {
                WeaponFogVFX.SpawnSwingFog(player, swingProgress, theme, 0.8f);
            }
            
            // === GOD RAY LIGHT EFFECT ===
            // Subtle light rays at weapon tip during peak velocity
            if (swingVelocity > 0.6f && swingProgress > 0.3f && swingProgress < 0.7f && Main.rand.NextBool(8))
            {
                GodRaySystem.CreateBurst(
                    tipPos,
                    primary * 0.6f,
                    rayCount: 6,
                    radius: 40f + swingVelocity * 30f,
                    duration: 12,
                    style: GodRaySystem.GodRayStyle.Explosion,
                    secondaryColor: secondary * 0.4f
                );
            }
            
            // === SUBTLE LIGHTING (replaces flare spam) ===
            // Just add dynamic lighting to the blade tip
            Lighting.AddLight(tipPos, primary.ToVector3() * (0.3f + swingVelocity * 0.4f));
            
            // === MUSIC NOTES (Still a music mod!) ===
            bool inSlowPhase = swingProgress < 0.2f || swingProgress > 0.85f;
            float noteChance = variation.MusicNoteChance * (inSlowPhase ? 2f : 1f);
            if (Main.rand.NextFloat() < noteChance)
            {
                Vector2 notePos = tipPos + Main.rand.NextVector2Circular(10f, 10f);
                Vector2 noteVel = currentAngle.ToRotationVector2() * 2f + new Vector2(0, -1f);
                ThemedParticles.MusicNote(notePos, noteVel, tertiary, 0.75f, 30);
            }
        }
        
        /// <summary>
        /// Calculates swing velocity with variation-specific timing parameters.
        /// </summary>
        private static float GetSwingVelocityVaried(float progress, MeleeSwingVariation variation)
        {
            float antic = variation.AnticipationTime;
            float fastStart = variation.FastPhaseStart;
            float fastEnd = variation.FastPhaseEnd;
            
            if (progress < antic * 0.6f)
            {
                // Slow anticipation - very low velocity
                return progress / (antic * 0.6f) * 0.2f;
            }
            else if (progress < fastStart)
            {
                // Ramping up
                float rampProgress = (progress - antic * 0.6f) / (fastStart - antic * 0.6f);
                return 0.2f + rampProgress * 0.3f;
            }
            else if (progress < fastEnd)
            {
                // Peak velocity zone - sine for smooth peak
                float midProgress = (progress - fastStart) / (fastEnd - fastStart);
                return 0.5f + (float)Math.Sin(midProgress * MathHelper.Pi) * 0.5f;
            }
            else if (progress < 0.9f)
            {
                // Decelerating
                float decelProgress = (progress - fastEnd) / (0.9f - fastEnd);
                return 0.5f * (1f - decelProgress);
            }
            else
            {
                // Final stop
                return 0.1f * (1f - (progress - 0.9f) / 0.1f);
            }
        }
        
        /// <summary>
        /// Calculates the instantaneous swing velocity based on eased progress.
        /// Returns 0-1 where 1 is maximum velocity (during the fast phase).
        /// Uses derivative of the piecewise animation curve.
        /// </summary>
        private static float GetSwingVelocity(float progress)
        {
            // The swing curve is: SlowStart (0-25%), FastSwing (25-75%), EaseOut (75-100%)
            // Velocity peaks during FastSwing phase and is low at start/end
            
            // Approximate velocity by sampling progress difference
            // In the fast phase (0.25-0.75), velocity is high
            // At edges, velocity is low
            
            if (progress < 0.15f)
            {
                // Slow anticipation - very low velocity
                return progress / 0.15f * 0.2f;
            }
            else if (progress < 0.25f)
            {
                // Ramping up
                return 0.2f + (progress - 0.15f) / 0.1f * 0.3f;
            }
            else if (progress < 0.75f)
            {
                // Peak velocity zone - use sine for smooth peak in middle
                float midProgress = (progress - 0.25f) / 0.5f;
                return 0.5f + (float)Math.Sin(midProgress * MathHelper.Pi) * 0.5f;
            }
            else if (progress < 0.9f)
            {
                // Decelerating
                float decelProgress = (progress - 0.75f) / 0.15f;
                return 0.5f * (1f - decelProgress);
            }
            else
            {
                // Final stop
                return 0.1f * (1f - (progress - 0.9f) / 0.1f);
            }
        }

        /// <summary>
        /// Creates a dramatic wave projectile effect using SwordArc textures with bloom layers.
        /// </summary>
        public static void WaveProjectileEffect(Projectile projectile, string theme, float scale = 1f)
        {
            var palette = MagnumThemePalettes.GetThemePalette(theme);
            if (palette == null || palette.Length < 2) return;
            
            Color primary = palette[0];
            Color secondary = palette[1];
            
            // Use interpolated position for smoothness
            Vector2 drawPos = InterpolatedRenderer.GetInterpolatedCenter(projectile);
            float rotation = InterpolatedRenderer.GetInterpolatedRotation(projectile);
            
            // === MULTI-LAYER ARC BLOOM ===
            // Outer glow layer
            float pulse = 1f + (float)Math.Sin(Main.GameUpdateCount * 0.15f) * 0.1f;
            
            // Core arc
            CustomParticles.GenericFlare(drawPos, primary, 0.5f * scale * pulse, 8);
            
            // Gradient trail particles
            for (int i = 0; i < 4; i++)
            {
                float t = i / 4f;
                Vector2 trailPos = drawPos - projectile.velocity.SafeNormalize(Vector2.Zero) * (i * 8f);
                Color trailColor = Color.Lerp(primary, secondary, t) * (1f - t * 0.3f);
                CustomParticles.GenericFlare(trailPos, trailColor, 0.35f * scale * (1f - t * 0.5f), 6);
            }
            
            // Leading edge sparkle
            if (Main.rand.NextBool(3))
            {
                var sparkle = new SparkleParticle(
                    drawPos + projectile.velocity.SafeNormalize(Vector2.Zero) * 10f,
                    projectile.velocity * 0.1f,
                    Color.White * 0.8f,
                    0.3f * scale,
                    15
                );
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
        }

        #endregion

        #region Projectile Trail Systems

        /// <summary>
        /// Creates a Calamity-style primitive trail for any projectile.
        /// Uses the new EnhancedTrailRenderer with multi-pass bloom.
        /// </summary>
        public static void ProjectilePrimitiveTrail(Projectile projectile, string theme, float width = 25f)
        {
            if (projectile.oldPos == null || projectile.oldPos.Length < 2) return;
            
            var palette = MagnumThemePalettes.GetThemePalette(theme);
            if (palette == null || palette.Length < 2) return;
            
            // Filter valid positions
            List<Vector2> validPos = new List<Vector2>();
            foreach (var pos in projectile.oldPos)
            {
                if (pos != Vector2.Zero)
                    validPos.Add(pos + projectile.Size * 0.5f);
            }
            if (validPos.Count < 2) return;
            
            Color primary = palette[0];
            Color secondary = palette.Length > 1 ? palette[1] : primary;
            
            // Render buttery-smooth multi-pass trail
            EnhancedTrailRenderer.RenderMultiPassTrail(
                validPos.ToArray(),
                EnhancedTrailRenderer.InverseLerpBumpWidth(width * 0.2f, width),
                EnhancedTrailRenderer.PaletteColor(palette, projectile.Opacity),
                bloomMultiplier: 2.7f,
                coreMultiplier: 0.4f,
                segmentCount: 40
            );
        }

        /// <summary>
        /// Creates a Bézier curve homing projectile path with smooth interpolated movement.
        /// </summary>
        public static void BezierHomingPath(Projectile projectile, NPC target, string theme, ref BezierProjectileSystem.BezierState state)
        {
            if (!state.Initialized && target != null && target.active)
            {
                // Initialize Bézier path
                Vector2[] path = BezierProjectileSystem.GenerateHomingArc(
                    projectile.Center,
                    target.Center,
                    arcHeight: 0.4f,
                    curveDirection: Main.rand.NextBool() ? -1f : 1f
                );
                
                state = BezierProjectileSystem.BezierState.Create(path, 0.015f, target.whoAmI, updateTarget: true);
            }
            
            if (state.Initialized)
            {
                BezierProjectileSystem.UpdateBezierProjectile(projectile, ref state);
            }
        }

        /// <summary>
        /// Creates an S-curve snaking projectile effect.
        /// </summary>
        public static void SnakingProjectilePath(Projectile projectile, Vector2 target, string theme, ref BezierProjectileSystem.BezierState state)
        {
            if (!state.Initialized)
            {
                Vector2[] path = BezierProjectileSystem.GenerateSnakingPath(
                    projectile.Center,
                    target,
                    amplitude: 80f
                );
                
                state = BezierProjectileSystem.BezierState.Create(path, 0.02f);
            }
            
            if (state.Initialized)
            {
                BezierProjectileSystem.UpdateBezierProjectile(projectile, ref state);
            }
        }

        #endregion

        #region Impact & Death Effects

        /// <summary>
        /// Creates a spectacular death effect with screen distortion and cascading particles.
        /// Replaces the old "puff" style deaths.
        /// </summary>
        public static void SpectacularDeath(Vector2 position, string theme, float scale = 1f)
        {
            var palette = MagnumThemePalettes.GetThemePalette(theme);
            if (palette == null || palette.Length < 2) return;
            
            Color primary = palette[0];
            Color secondary = palette.Length > 1 ? palette[1] : primary;
            Color tertiary = palette.Length > 2 ? palette[2] : secondary;
            
            // === SCREEN DISTORTION ===
            ScreenDistortionManager.TriggerThemeEffect(theme, position, scale * 0.4f, 20);
            
            // === CENTRAL NOVA FLASH ===
            CustomParticles.GenericFlare(position, Color.White, 1.5f * scale, 22);
            CustomParticles.GenericFlare(position, primary, 1.2f * scale, 20);
            CustomParticles.GenericFlare(position, secondary, 0.9f * scale, 18);
            
            // === CASCADING HALO RINGS (8 layers!) ===
            for (int i = 0; i < 8; i++)
            {
                float progress = i / 8f;
                Color ringColor = VFXUtilities.PaletteLerp(palette, progress);
                float ringScale = (0.2f + i * 0.12f) * scale;
                int ringLife = 15 + i * 4;
                CustomParticles.HaloRing(position, ringColor * (1f - progress * 0.4f), ringScale, ringLife);
            }
            
            // === RADIAL SPARKLE BURST ===
            for (int i = 0; i < 16; i++)
            {
                float angle = MathHelper.TwoPi * i / 16f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(6f, 12f) * scale;
                Color sparkleColor = Color.Lerp(primary, secondary, (float)i / 16f);
                
                var sparkle = new SparkleParticle(position, vel, sparkleColor, 0.4f * scale, 25);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
            
            // === GLOW PARTICLE EXPLOSION ===
            for (int i = 0; i < 12; i++)
            {
                float angle = MathHelper.TwoPi * i / 12f + Main.rand.NextFloat(-0.2f, 0.2f);
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 8f) * scale;
                Color glowColor = VFXUtilities.PaletteLerp(palette, Main.rand.NextFloat());
                
                var glow = new GenericGlowParticle(position, vel, glowColor * 0.85f, 0.45f * scale, 28, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }
            
            // === MUSIC NOTE FINALE ===
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 noteVel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 6f);
                ThemedParticles.MusicNote(position, noteVel, tertiary, 0.8f, 35);
            }
            
            // === BRIGHT LIGHTING PULSE ===
            Lighting.AddLight(position, primary.ToVector3() * 2f * scale);
        }

        /// <summary>
        /// Creates a glimmer cascade impact (replaces old basic hit effects).
        /// </summary>
        public static void GlimmerCascadeImpact(Vector2 position, string theme, float scale = 1f)
        {
            var palette = MagnumThemePalettes.GetThemePalette(theme);
            if (palette == null || palette.Length < 2) return;
            
            Color primary = palette[0];
            Color secondary = palette.Length > 1 ? palette[1] : primary;
            
            // === MULTI-LAYER SPINNING FLARES ===
            float time = Main.GameUpdateCount * 0.08f;
            for (int layer = 0; layer < 4; layer++)
            {
                float layerRot = time + layer * MathHelper.PiOver4;
                float layerScale = (0.3f + layer * 0.1f) * scale;
                float layerAlpha = 0.8f - layer * 0.15f;
                Color layerColor = Color.Lerp(Color.White, primary, layer / 4f);
                
                Vector2 offset = layerRot.ToRotationVector2() * (layer * 5f);
                CustomParticles.GenericFlare(position + offset, layerColor * layerAlpha, layerScale, 12 - layer);
            }
            
            // === EXPANDING GLOW RINGS ===
            for (int i = 0; i < 3; i++)
            {
                Color ringColor = Color.Lerp(primary, secondary, i / 3f);
                CustomParticles.HaloRing(position, ringColor, (0.2f + i * 0.08f) * scale, 10 + i * 2);
            }
            
            // === CONTRASTING SPARKLES ===
            for (int i = 0; i < 6; i++)
            {
                Vector2 sparkleOffset = Main.rand.NextVector2Circular(15f, 15f) * scale;
                var sparkle = new SparkleParticle(
                    position + sparkleOffset,
                    Main.rand.NextVector2Circular(2f, 2f),
                    Color.White * 0.7f,
                    0.25f * scale,
                    15
                );
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
        }

        #endregion

        #region Ambient & Hold Effects

        /// <summary>
        /// Creates smooth orbiting particles around a held weapon.
        /// Uses interpolation for buttery-smooth orbits at any frame rate.
        /// </summary>
        public static void OrbitingAuraEffect(Player player, string theme, float radius = 35f, int particleCount = 5)
        {
            var palette = MagnumThemePalettes.GetThemePalette(theme);
            if (palette == null || palette.Length < 2) return;
            
            Color primary = palette[0];
            Color secondary = palette.Length > 1 ? palette[1] : primary;
            
            float baseAngle = Main.GameUpdateCount * 0.04f;
            float pulseRadius = radius + (float)Math.Sin(Main.GameUpdateCount * 0.08f) * 5f;
            
            for (int i = 0; i < particleCount; i++)
            {
                float angle = baseAngle + MathHelper.TwoPi * i / particleCount;
                float particleRadius = pulseRadius + (float)Math.Sin(Main.GameUpdateCount * 0.1f + i) * 3f;
                Vector2 orbitPos = player.Center + angle.ToRotationVector2() * particleRadius;
                
                // Color cycles through palette
                float colorProgress = (i / (float)particleCount + Main.GameUpdateCount * 0.01f) % 1f;
                Color orbitColor = VFXUtilities.PaletteLerp(palette, colorProgress);
                
                // Spawn orbiting glow
                if (Main.rand.NextBool(3))
                {
                    CustomParticles.GenericFlare(orbitPos, orbitColor, 0.25f, 8);
                }
                
                // Trailing sparkle
                if (Main.rand.NextBool(8))
                {
                    Vector2 trailVel = angle.ToRotationVector2().RotatedBy(MathHelper.PiOver2) * 1.5f;
                    var sparkle = new SparkleParticle(orbitPos, trailVel, orbitColor * 0.7f, 0.2f, 12);
                    MagnumParticleHandler.SpawnParticle(sparkle);
                }
            }
            
            // Central soft glow
            float centralPulse = (float)Math.Sin(Main.GameUpdateCount * 0.06f) * 0.15f + 0.85f;
            Lighting.AddLight(player.Center, primary.ToVector3() * centralPulse * 0.5f);
        }

        /// <summary>
        /// Creates ethereal wing silhouettes behind the player.
        /// </summary>
        public static void EtherealWingEffect(Player player, string theme, float wingSpread = 60f)
        {
            var palette = MagnumThemePalettes.GetThemePalette(theme);
            if (palette == null || palette.Length < 2) return;
            
            Color primary = palette[0];
            Color secondary = palette.Length > 1 ? palette[1] : primary;
            
            float time = Main.GameUpdateCount * 0.03f;
            float wingPulse = (float)Math.Sin(time * 2f) * 0.15f + 0.85f;
            
            // Two wings
            for (int wing = 0; wing < 2; wing++)
            {
                float wingDirection = wing == 0 ? -1f : 1f;
                
                // Each wing has feathers spreading outward
                for (int feather = 0; feather < 7; feather++)
                {
                    float featherAngle = MathHelper.PiOver2 * wingDirection;
                    featherAngle += (feather - 3) * 0.12f * wingDirection;
                    featherAngle += MathHelper.Pi * 0.15f;
                    
                    float featherLength = (20f + feather * 8f) * wingPulse;
                    Vector2 featherTip = player.Center + featherAngle.ToRotationVector2() * featherLength;
                    featherTip.Y -= 5f;
                    
                    // Rainbow cycling per feather
                    float hue = (feather / 7f + time * 0.5f) % 1f;
                    Color featherColor = Main.hslToRgb(hue, 0.6f, 0.75f);
                    featherColor = Color.Lerp(primary, featherColor, 0.4f);
                    
                    if (Main.rand.NextBool(4))
                    {
                        CustomParticles.GenericFlare(featherTip, featherColor, 0.2f + feather * 0.03f, 8);
                    }
                }
            }
        }

        #endregion

        #region Boss Attack VFX

        /// <summary>
        /// Creates a Calamity-style boss attack windup with screen distortion buildup.
        /// </summary>
        public static void BossAttackWindup(NPC boss, string theme, float progress, float radius = 150f)
        {
            var palette = MagnumThemePalettes.GetThemePalette(theme);
            if (palette == null || palette.Length < 2) return;
            
            Color primary = palette[0];
            Color secondary = palette.Length > 1 ? palette[1] : primary;
            
            Vector2 bossCenter = InterpolatedRenderer.GetInterpolatedCenter(boss);
            
            // === CONVERGING PARTICLE RING ===
            int particleCount = (int)(6 + progress * 12);
            float currentRadius = radius * (1f - progress * 0.6f);
            
            for (int i = 0; i < particleCount; i++)
            {
                float angle = MathHelper.TwoPi * i / particleCount + Main.GameUpdateCount * 0.03f;
                Vector2 pos = bossCenter + angle.ToRotationVector2() * currentRadius;
                Color particleColor = Color.Lerp(primary, secondary, (float)i / particleCount);
                
                CustomParticles.GenericFlare(pos, particleColor, 0.25f + progress * 0.35f, 10);
            }
            
            // === SCREEN DISTORTION BUILDUP ===
            if (progress > 0.5f)
            {
                float distortionIntensity = (progress - 0.5f) * 0.6f;
                ScreenDistortionManager.TriggerThemeEffect(theme, bossCenter, distortionIntensity, 5);
            }
            
            // === CENTRAL GLOW INTENSIFYING ===
            CustomParticles.GenericFlare(bossCenter, Color.Lerp(primary, Color.White, progress * 0.6f), 0.4f + progress * 0.6f, 8);
            
            // === MUSIC NOTES ORBITING ===
            if (Main.rand.NextBool(5) && progress > 0.3f)
            {
                float noteAngle = Main.GameUpdateCount * 0.1f;
                for (int i = 0; i < 3; i++)
                {
                    float angle = noteAngle + MathHelper.TwoPi * i / 3f;
                    Vector2 notePos = bossCenter + angle.ToRotationVector2() * (40f + progress * 30f);
                    ThemedParticles.MusicNote(notePos, angle.ToRotationVector2() * 2f, primary, 0.7f + progress * 0.3f, 25);
                }
            }
        }

        /// <summary>
        /// Creates a spectacular boss attack release with full VFX burst.
        /// </summary>
        public static void BossAttackRelease(NPC boss, string theme, float scale = 1f)
        {
            Vector2 bossCenter = InterpolatedRenderer.GetInterpolatedCenter(boss);
            
            // Use the spectacular death effect for maximum impact
            SpectacularDeath(bossCenter, theme, scale);
            
            // Additional screen flash
            var palette = MagnumThemePalettes.GetThemePalette(theme);
            if (palette != null && palette.Length > 0)
            {
                DynamicSkyboxSystem.TriggerFlash(palette[0], scale * 0.8f);
            }
        }

        /// <summary>
        /// Creates a boss dash trail using primitive rendering.
        /// Returns trail ID for updating during dash.
        /// </summary>
        public static int BossDashStart(NPC boss, string theme, float width = 35f)
        {
            Vector2 startPos = InterpolatedRenderer.GetInterpolatedCenter(boss);
            
            var palette = MagnumThemePalettes.GetThemePalette(theme);
            Color primary = palette != null && palette.Length > 0 ? palette[0] : Color.White;
            Color secondary = palette != null && palette.Length > 1 ? palette[1] : primary;
            
            // Departure burst
            CustomParticles.GenericFlare(startPos, primary, 0.8f, 15);
            CustomParticles.HaloRing(startPos, primary, 0.4f, 12);
            
            // Create trail
            return AdvancedTrailSystem.CreateThemeTrail(theme, width, maxPoints: 25, intensity: 1.2f);
        }

        /// <summary>
        /// Updates boss dash trail with current position.
        /// </summary>
        public static void BossDashUpdate(int trailId, NPC boss)
        {
            Vector2 pos = InterpolatedRenderer.GetInterpolatedCenter(boss);
            float rotation = boss.rotation;
            AdvancedTrailSystem.UpdateTrail(trailId, pos, rotation);
        }

        /// <summary>
        /// Ends boss dash with impact effect.
        /// </summary>
        public static void BossDashEnd(int trailId, NPC boss, string theme)
        {
            AdvancedTrailSystem.EndTrail(trailId);
            
            Vector2 endPos = InterpolatedRenderer.GetInterpolatedCenter(boss);
            GlimmerCascadeImpact(endPos, theme, 1.2f);
        }

        /// <summary>
        /// Creates a boss phase transition with spectacular cascading effects.
        /// </summary>
        public static void BossPhaseTransition(NPC boss, string theme, float scale = 1.5f)
        {
            Vector2 bossCenter = InterpolatedRenderer.GetInterpolatedCenter(boss);
            
            var palette = MagnumThemePalettes.GetThemePalette(theme);
            if (palette == null || palette.Length < 2) return;
            
            Color primary = palette[0];
            Color secondary = palette.Length > 1 ? palette[1] : primary;
            
            // === MASSIVE SCREEN DISTORTION ===
            ScreenDistortionManager.TriggerThemeEffect(theme, bossCenter, scale * 0.7f, 40);
            
            // === TRIPLE NOVA FLASH ===
            CustomParticles.GenericFlare(bossCenter, Color.White, 2f * scale, 30);
            CustomParticles.GenericFlare(bossCenter, primary, 1.6f * scale, 28);
            CustomParticles.GenericFlare(bossCenter, secondary, 1.2f * scale, 25);
            
            // === CASCADING SHOCKWAVE SEQUENCE (12 rings!) ===
            for (int phase = 0; phase < 3; phase++)
            {
                for (int i = 0; i < 4; i++)
                {
                    float ringScale = scale * (0.3f + i * 0.2f + phase * 0.35f);
                    Color ringColor = VFXUtilities.PaletteLerp(palette, (phase * 4 + i) / 12f);
                    int ringLife = 22 + phase * 8 + i * 3;
                    CustomParticles.HaloRing(bossCenter, ringColor * (1f - phase * 0.2f), ringScale, ringLife);
                }
            }
            
            // === MASSIVE PARTICLE EXPLOSION ===
            for (int i = 0; i < 30; i++)
            {
                float angle = MathHelper.TwoPi * i / 30f + Main.rand.NextFloat(-0.1f, 0.1f);
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(10f, 20f) * scale;
                Color particleColor = VFXUtilities.PaletteLerp(palette, Main.rand.NextFloat());
                
                var glow = new GenericGlowParticle(bossCenter, vel, particleColor * 0.9f, 0.6f * scale, 35, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }
            
            // === MUSIC NOTE CASCADE ===
            for (int i = 0; i < 12; i++)
            {
                float angle = MathHelper.TwoPi * i / 12f;
                Vector2 noteVel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 8f);
                ThemedParticles.MusicNote(bossCenter, noteVel, primary, 0.9f, 40);
            }
            
            // === SKY FLASH ===
            DynamicSkyboxSystem.TriggerFlash(primary, scale);
        }

        #endregion

        #region Color Utilities

        /// <summary>
        /// Gets a smoothly cycling theme color.
        /// </summary>
        public static Color GetCyclingThemeColor(string theme, float offset = 0f)
        {
            return VFXIntegration.GetThemeColor(theme, offset);
        }

        /// <summary>
        /// Gets rainbow color with theme-specific hue constraints.
        /// </summary>
        public static Color GetThemedRainbow(string theme, float offset = 0f)
        {
            return theme.ToLower() switch
            {
                "eroica" => RainbowGradientSystem.GetEroicaFlame(offset),
                "lacampanella" or "campanella" => RainbowGradientSystem.GetCampanellaInferno(offset),
                "swanlake" or "swan" => RainbowGradientSystem.GetSwanLakeShimmer(offset),
                "moonlight" or "moonlightsonata" => RainbowGradientSystem.GetMoonlightGlow(offset),
                "enigma" or "enigmavariations" => RainbowGradientSystem.GetEnigmaVoid(offset),
                "fate" => RainbowGradientSystem.GetFateCosmic(offset),
                _ => RainbowGradientSystem.GetRainbowColor(offset)
            };
        }

        #endregion
    }
}
