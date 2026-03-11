using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.FoundationWeapons.SparkleProjectileFoundation
{
    /// <summary>
    /// SparkleCrystalProjectile — A small homing crystal shard that rotates, sparkles,
    /// and leaves a glittery shader-driven trail behind it.
    /// 
    /// SHADER ARCHITECTURE:
    /// 
    /// 1. SPARKLE TRAIL SHADER (SparkleTrailShader.fx) — VertexStrip pipeline
    ///    - Builds a triangle strip mesh from position history (ring buffer of 24 points)
    ///    - Custom vertex shader transforms world→screen via WorldViewProjection matrix
    ///    - Custom pixel shader creates the glitter effect:
    ///      • Samples a star texture (4PointedStarHard) with UV scrolling for internal sparkle motion
    ///      • Procedural sin-wave interference patterns at multiple frequencies create scattered
    ///        sparkle flash points — raised to high power for sharp peaks (the "glitter" look)
    ///      • Gradient LUT sampling for theme-consistent coloring
    ///      • Smooth edge fade on UV.y and tip fade on UV.x
    ///      • Bright core color + outer theme color layering for depth
    ///    - Trail width tapers from head (8px) to tail (2px)
    ///    - Vertex alpha driven by progress for additional fade control
    /// 
    /// 2. CRYSTAL SHIMMER SHADER (CrystalShimmerShader.fx) — SpriteBatch pipeline
    ///    - Applied as SpriteBatch effect to crystal body sprite draws
    ///    - Creates a dazzling prismatic faceted shimmer effect:
    ///      • 6-facet angular shimmer using sin(angle * 6) for hexagonal crystal look
    ///      • Secondary 4-facet interference pattern for complex light play
    ///      • HSV-based prismatic color at facet boundaries — hue shifts with angle
    ///      • Gradient LUT sampling for theme-consistent prismatic recoloring
    ///      • Sharp sparkle flash points at facet intersections (pow 16 peaks)
    ///      • Pulsing center glow for inner crystal light
    ///    - Sprite luminance drives the color mapping — bright regions get more shimmer
    /// 
    /// 3. ADDITIVE BLOOM LAYER — Standard SpriteBatch (no shader)
    ///    - Multi-scale bloom stacking (outer/mid/inner) using SoftGlow and SoftRadialBloom
    ///    - Provides the ambient glow halo around the crystal
    ///    - Pulsing brightness synced to crystal phase offset
    /// 
    /// 4. SPARKLE ACCENT SPRITES — Standard SpriteBatch additive (no shader)
    ///    - 4 orbiting sparkle points using 4PointedStarHard + ThinTall4PointedStar
    ///    - Cubic sin-wave flash timing for brief bright peaks with long dark gaps
    ///    - Central twinkle pulse with pow(6) peaks for very brief dazzle flashes
    ///    - These supplement the shader effects with discrete accent points
    /// 
    /// BEHAVIOR:
    /// - Spawns from melee swing with 3 crystals in a spread pattern
    /// - 15-frame delay before homing kicks in (lets crystals fan out)
    /// - Smooth angular homing toward nearest enemy (0.06 rad/frame turn speed)
    /// - Continuous rotation with per-crystal phase offset (crystals 120° apart)
    /// - extraUpdates = 1 for smoother movement and denser trail points
    /// - 180-frame lifetime with 30-frame fade-out
    /// - 2 penetrations then death, with sparkle burst on kill
    /// 
    /// ai[0] = Crystal index (0, 1, 2) — phase offset for rotation/sparkle timing
    /// ai[1] = Theme index — determines color palette from SPFTextures.GetThemeColors()
    /// </summary>
    public class SparkleCrystalProjectile : ModProjectile
    {
        // Use vanilla Crystal Bullet sprite as a tiny placeholder
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.CrystalBullet;

        // ---- CONSTANTS ----
        private const int TrailLength = 24;
        private const int MaxLifetime = 180;
        private const int HomingDelay = 15;
        private const float HomingStrength = 0.06f;
        private const float HomingRange = 800f;
        private const float TargetSpeed = 11f;
        private const int FadeOutFrames = 30;
        private const float CrystalBaseScale = 0.04f;
        private const float TrailWidthHead = 14f;
        private const float TrailWidthTail = 1f;

        // ---- BLOOM TRAIL CONSTANTS (Photoviscerator-style) ----
        /// <summary>Base width scale of bloom sprites (perpendicular to velocity).</summary>
        private const float BloomTrailWidthScale = 0.035f;
        /// <summary>How much to stretch blooms along the velocity direction (multiplier on width).</summary>
        private const float BloomTrailStretch = 3.0f;
        /// <summary>How quickly bloom trail shrinks toward tail (per step).</summary>
        private const float BloomTrailShrinkRate = 0.96f;
        /// <summary>How quickly bloom trail fades toward tail (per step).</summary>
        private const float BloomTrailFadeRate = 0.92f;

        // ---- STATE ----
        /// <summary>Ring buffer of past positions for the shader trail VertexStrip.</summary>
        private Vector2[] trailPositions;

        /// <summary>Ring buffer of past rotations for oriented trail mesh construction.</summary>
        private float[] trailRotations;

        private int trailIndex;
        private int trailCount;
        private int timer;

        private int CrystalIndex => (int)Projectile.ai[0];
        private CrystalTheme CurrentTheme => (CrystalTheme)(int)Projectile.ai[1];

        private float bodyRotation;
        private float sparkleSeed;

        // ---- CACHED SHADERS ----
        /// <summary>Cached SparkleTrailShader. Loaded once via ModContent.Request, reused every frame.</summary>
        private Effect trailShader;

        /// <summary>Cached CrystalShimmerShader. Loaded once, applied as SpriteBatch effect.</summary>
        private Effect crystalShader;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 2;
            ProjectileID.Sets.DrawScreenCheckFluff[Projectile.type] = 400;
        }

        public override void SetDefaults()
        {
            Projectile.width = 12;
            Projectile.height = 12;
            Projectile.friendly = true;
            Projectile.penetrate = 2;
            Projectile.tileCollide = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.timeLeft = MaxLifetime;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 15;
            Projectile.hide = false;
            Projectile.alpha = 0;
            Projectile.extraUpdates = 1;
        }

        public override void AI()
        {
            // ---- INIT ----
            if (trailPositions == null)
            {
                trailPositions = new Vector2[TrailLength];
                trailRotations = new float[TrailLength];
                trailIndex = 0;
                trailCount = 0;
                sparkleSeed = Main.rand.NextFloat(100f);
            }

            timer++;

            // ---- RECORD TRAIL POSITION ----
            trailPositions[trailIndex] = Projectile.Center;
            trailRotations[trailIndex] = Projectile.velocity.ToRotation();
            trailIndex = (trailIndex + 1) % TrailLength;
            if (trailCount < TrailLength)
                trailCount++;

            // ---- HOMING ----
            if (timer > HomingDelay * 2) // *2 because extraUpdates doubles AI calls
            {
                NPC target = FindClosestTarget();
                if (target != null)
                {
                    Vector2 toTarget = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX);
                    float targetAngle = toTarget.ToRotation();
                    float currentAngle = Projectile.velocity.ToRotation();
                    float angleDiff = MathHelper.WrapAngle(targetAngle - currentAngle);
                    float clampedTurn = MathHelper.Clamp(angleDiff, -HomingStrength, HomingStrength);
                    Projectile.velocity = (currentAngle + clampedTurn).ToRotationVector2() * Projectile.velocity.Length();
                }

                if (Projectile.velocity.Length() < TargetSpeed)
                {
                    Projectile.velocity *= 1.02f;
                    if (Projectile.velocity.Length() > TargetSpeed)
                        Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.UnitX) * TargetSpeed;
                }
            }

            // ---- ROTATION ----
            float phaseOffset = CrystalIndex * MathHelper.TwoPi / 3f;
            bodyRotation += 0.12f + 0.03f * MathF.Sin((float)Main.timeForVisualEffects * 0.05f + phaseOffset);

            // ---- DYNAMIC LIGHTING ----
            Color[] colors = SPFTextures.GetThemeColors(CurrentTheme);
            float speed = Projectile.velocity.Length();
            float speedPulse = 0.3f + 0.15f * MathF.Sin((float)Main.timeForVisualEffects * 0.1f + sparkleSeed);
            float speedFactor = MathHelper.Clamp(speed / TargetSpeed, 0.3f, 1.2f);
            // Primary light at projectile center — intensity scales with speed
            Lighting.AddLight(Projectile.Center, colors[0].ToVector3() * (speedPulse + 0.15f) * speedFactor);
            // Accent light slightly behind — creates directional glow trail
            if (speed > 2f)
            {
                Vector2 behindPos = Projectile.Center - Projectile.velocity.SafeNormalize(Vector2.Zero) * 12f;
                Lighting.AddLight(behindPos, colors[2].ToVector3() * speedPulse * 0.5f * speedFactor);
            }

            // ---- AMBIENT DUST ----
            if (Main.rand.NextBool(3))
            {
                Color dustColor = colors[Main.rand.Next(colors.Length)];
                Vector2 dustVel = Main.rand.NextVector2Circular(1.5f, 1.5f);
                Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(6f, 6f),
                    DustID.RainbowMk2, dustVel, newColor: dustColor, Scale: Main.rand.NextFloat(0.2f, 0.4f));
                dust.noGravity = true;
                dust.fadeIn = 0.4f;
            }
        }

        private NPC FindClosestTarget()
        {
            NPC closest = null;
            float closestDist = HomingRange;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly || npc.dontTakeDamage || npc.immortal)
                    continue;

                float dist = Vector2.Distance(Projectile.Center, npc.Center);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closest = npc;
                }
            }

            return closest;
        }

        // =====================================================================
        // RENDERING — All visual layers drawn here
        // =====================================================================

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            try
            {
            Color[] themeColors = SPFTextures.GetThemeColors(CurrentTheme);

            float lifeFade = Projectile.timeLeft < FadeOutFrames
                ? Projectile.timeLeft / (float)FadeOutFrames
                : 1f;

            // ---- LAYER 1: SHADER-DRIVEN GLITTER TRAIL (VertexStrip) ----
            DrawShaderTrail(sb, themeColors, lifeFade);

            // ---- LAYER 2: BLOOM TRAIL (Photoviscerator-style shrinking blooms) ----
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.TransformationMatrix);

            DrawBloomTrail(sb, themeColors, lifeFade);

            // ---- LAYER 3: BLOOM HALO at projectile head ----
            DrawCrystalGlowHalo(sb, themeColors, lifeFade);

            // ---- LAYER 4: CRYSTAL BODY (BrightStarProjectile1 drawn directly + shimmer shader) ----
            DrawShaderCrystalBody(sb, themeColors, lifeFade);

            // ---- LAYER 5: SPARKLE ACCENTS (additive sprites, no shader) ----
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.TransformationMatrix);

            DrawSparkleAccents(sb, themeColors, lifeFade);

            // ---- RESTORE ----
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.TransformationMatrix);

            }
            catch { }
            finally
            {
                try { sb.End(); } catch { }
                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                    DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            }

            return false;
        }

        // =====================================================================
        // LAYER 1: SPARKLE TRAIL SHADER — VertexStrip pipeline
        // =====================================================================

        /// <summary>
        /// Draws the glitter trail using a VertexStrip mesh and the SparkleTrailShader.
        /// 
        /// HOW IT WORKS:
        /// 1. Flatten the ring buffer into ordered position/rotation arrays (oldest → newest)
        /// 2. Build a VertexStrip with tapering width and alpha-driven vertex colors
        /// 3. Load and configure SparkleTrailShader with all uniforms:
        ///    - WorldViewProjection matrix for vertex transformation
        ///    - Time for animation
        ///    - Sparkle texture (4PointedStarHard) for internal sparkle pattern
        ///    - Gradient LUT for theme coloring
        ///    - Glow mask (SoftCircle) for cross-section shaping
        ///    - Color uniforms for core and outer colors
        ///    - Sparkle parameters (speed, scale, density, fade, edge softness)
        /// 4. Apply the shader pass and draw the triangle strip
        /// 5. Reset the pixel shader to default
        /// </summary>
        private void DrawShaderTrail(SpriteBatch sb, Color[] themeColors, float lifeFade)
        {
            if (trailCount < 3)
                return;

            // ---- END SPRITEBATCH FOR RAW VERTEX DRAWING ----
            sb.End();

            // ---- BUILD ORDERED ARRAYS FROM RING BUFFER ----
            Vector2[] positions = new Vector2[trailCount];
            float[] rotations = new float[trailCount];

            for (int i = 0; i < trailCount; i++)
            {
                int bufIdx = (trailIndex - trailCount + i + TrailLength * 2) % TrailLength;
                positions[i] = trailPositions[bufIdx];
                rotations[i] = trailRotations[bufIdx];
            }

            // ---- VERTEX STRIP SETUP ----
            // Width tapers from tail to head; alpha fades quadratically
            Color StripColor(float progress)
            {
                float alpha = progress * progress * lifeFade;
                return Color.White * alpha;
            }

            float StripWidth(float progress)
            {
                return MathHelper.Lerp(TrailWidthTail, TrailWidthHead, progress);
            }

            VertexStrip strip = new VertexStrip();
            strip.PrepareStrip(positions, rotations, StripColor, StripWidth,
                -Main.screenPosition, includeBacksides: true);

            // ---- LOAD TRAIL SHADER ----
            if (trailShader == null)
            {
                trailShader = ModContent.Request<Effect>(
                    "MagnumOpus/Content/FoundationWeapons/SparkleProjectileFoundation/Shaders/SparkleTrailShader",
                    AssetRequestMode.ImmediateLoad).Value;
            }

            // ---- CONFIGURE SHADER UNIFORMS ----
            trailShader.Parameters["WorldViewProjection"]?.SetValue(
                Main.GameViewMatrix.NormalizedTransformationmatrix);

            trailShader.Parameters["uTime"]?.SetValue(
                (float)Main.timeForVisualEffects * 0.02f + sparkleSeed);

            // Sparkle pattern texture — the star texture drives the internal glitter detail
            trailShader.Parameters["sparkleTex"]?.SetValue(SPFTextures.SparkleHard.Value);

            // Gradient LUT for theme-consistent coloring
            trailShader.Parameters["gradientTex"]?.SetValue(
                SPFTextures.GetGradientForTheme(CurrentTheme));

            // Glow mask for cross-section shaping — soft circle gives smooth trail edges
            trailShader.Parameters["glowMaskTex"]?.SetValue(SPFTextures.SoftCircle.Value);

            // Color parameters
            trailShader.Parameters["coreColor"]?.SetValue(themeColors[4].ToVector3());
            trailShader.Parameters["outerColor"]?.SetValue(themeColors[0].ToVector3());

            // Sparkle behavior parameters
            trailShader.Parameters["trailIntensity"]?.SetValue(1.5f);
            trailShader.Parameters["sparkleSpeed"]?.SetValue(1.2f);
            trailShader.Parameters["sparkleScale"]?.SetValue(3.0f);
            trailShader.Parameters["glitterDensity"]?.SetValue(2.5f);
            trailShader.Parameters["tipFadeStart"]?.SetValue(0.7f);
            trailShader.Parameters["edgeSoftness"]?.SetValue(0.4f);

            // ---- DRAW WITH SHADER ----
            // Apply pixel shader pass, draw the vertex strip, then reset to default
            trailShader.CurrentTechnique.Passes["SparkleTrailPass"].Apply();
            strip.DrawTrail();

            // Reset to Terraria's default pixel shader
            Main.pixelShader.CurrentTechnique.Passes[0].Apply();

            // ---- RESTART SPRITEBATCH ----
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.TransformationMatrix);
        }

        // =====================================================================
        // LAYER 2: BLOOM TRAIL — Photoviscerator-style shrinking bloom sprites
        // =====================================================================

        /// <summary>
        /// Draws velocity-stretched, fading bloom sprites at historical positions from the ring buffer.
        /// This creates a smooth bright glowing trail behind the projectile similar to Calamity's
        /// Photoviscerator technique.
        /// 
        /// The key to the Photoviscerator look:
        /// - Blooms are STRETCHED along the velocity direction (not circular)
        /// - High opacity that stacks additively into solid white at the head
        /// - White-hot at the head, transitioning to theme-colored at the tail
        /// - Gentle shrink so the trail stays wide for most of its length
        /// 
        /// THREE LAYERS per point (back to front):
        /// - Wide outer glow: theme-colored, ~1.5x width, very soft ambient
        /// - Main bloom body: white→themed color, velocity-stretched
        /// - Tight core: pure white, 50% scale, highest opacity (creates hot center line)
        /// </summary>
        private void DrawBloomTrail(SpriteBatch sb, Color[] themeColors, float lifeFade)
        {
            if (trailCount < 3)
                return;

            Texture2D bloomTex = SPFTextures.SoftGlow.Value;
            Vector2 bloomOrigin = bloomTex.Size() / 2f;

            int pointsToDraw = Math.Min(trailCount, TrailLength);

            for (int i = 0; i < pointsToDraw; i++)
            {
                // Walk backwards from newest to oldest
                int bufIdx = (trailIndex - 1 - i + TrailLength * 2) % TrailLength;
                Vector2 pos = trailPositions[bufIdx] - Main.screenPosition;

                // Get velocity direction from this point to the next (for stretching)
                int nextIdx = (bufIdx + 1) % TrailLength;
                Vector2 delta;
                if (i == 0)
                    delta = Projectile.velocity;
                else if (i < trailCount - 1)
                    delta = trailPositions[nextIdx] - trailPositions[bufIdx];
                else
                    delta = Projectile.velocity;

                float rotation = delta.ToRotation() + MathHelper.PiOver2;

                // Progressive shrink and fade
                float progress = (float)i / pointsToDraw; // 0 = head, 1 = tail
                float widthScale = BloomTrailWidthScale * MathF.Pow(BloomTrailShrinkRate, i);
                float fade = lifeFade * MathF.Pow(BloomTrailFadeRate, i);

                // Cubic fade for smoother tail falloff
                float cubicFade = fade * (1f - progress * progress * progress);

                if (cubicFade < 0.01f || widthScale < 0.002f)
                    break;

                // Velocity stretch: elongated along movement, narrower perpendicular
                // More stretch at the head, less at the tail for a tapering beam look
                float stretchAmount = MathHelper.Lerp(BloomTrailStretch, 1.5f, progress);
                Vector2 drawScale = new Vector2(widthScale, widthScale * stretchAmount);

                // Color: white-hot at head, transitioning to theme color at tail
                // First 40% is mostly white, then fades to theme color
                float colorProgress = MathF.Pow(progress, 0.6f); // slower transition
                Color bodyColor = Color.Lerp(Color.White, themeColors[0], colorProgress);

                // LAYER 1: Wide outer glow — theme colored, soft ambient
                Color outerColor = themeColors[0] * (cubicFade * 0.5f);
                Vector2 outerScale = drawScale * 1.6f;
                sb.Draw(bloomTex, pos, null, outerColor, rotation,
                    bloomOrigin, outerScale, SpriteEffects.None, 0f);

                // LAYER 2: Main bloom body — white→themed, velocity-stretched
                Color mainColor = bodyColor * (cubicFade * 0.75f);
                sb.Draw(bloomTex, pos, null, mainColor, rotation,
                    bloomOrigin, drawScale, SpriteEffects.None, 0f);

                // LAYER 3: Tight hot core — pure white, narrow
                float coreAlpha = cubicFade * (1f - progress * 0.7f); // core fades faster at tail
                Color coreColor = Color.White * (coreAlpha * 0.6f);
                Vector2 coreScale = new Vector2(widthScale * 0.4f, widthScale * stretchAmount * 0.7f);
                sb.Draw(bloomTex, pos, null, coreColor, rotation,
                    bloomOrigin, coreScale, SpriteEffects.None, 0f);
            }
        }

        // =====================================================================
        // LAYER 3: BLOOM HALO — Multi-scale additive bloom stacking at head
        // =====================================================================

        /// <summary>
        /// Draws a soft glow halo around the crystal using multi-scale bloom stacking.
        /// Three layers at different scales create convincing depth:
        /// - Outer: large, dim, theme dark color
        /// - Mid: medium, brighter, theme primary
        /// - Inner: small, intense, highlight white
        /// </summary>
        private void DrawCrystalGlowHalo(SpriteBatch sb, Color[] themeColors, float lifeFade)
        {
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Texture2D softGlow = SPFTextures.SoftGlow.Value;
            Texture2D softRadial = SPFTextures.SoftRadialBloom.Value;
            Vector2 glowOrigin = softGlow.Size() / 2f;
            Vector2 radialOrigin = softRadial.Size() / 2f;

            float pulse = 0.85f + 0.15f * MathF.Sin((float)Main.timeForVisualEffects * 0.08f + sparkleSeed);

            // Outer ambient glow
            sb.Draw(softGlow, drawPos, null, themeColors[3] * (0.2f * lifeFade * pulse), 0f,
                glowOrigin, 0.1f * pulse, SpriteEffects.None, 0f);

            // Mid glow
            sb.Draw(softRadial, drawPos, null, themeColors[0] * (0.35f * lifeFade * pulse), 0f,
                radialOrigin, 0.06f * pulse, SpriteEffects.None, 0f);

            // Inner core glow — brighter and white-hot
            sb.Draw(softGlow, drawPos, null, themeColors[4] * (0.5f * lifeFade * pulse), 0f,
                glowOrigin, 0.035f * pulse, SpriteEffects.None, 0f);
        }

        // =====================================================================
        // LAYER 4: CRYSTAL BODY — BrightStar drawn directly + CrystalShimmer shader
        // =====================================================================

        /// <summary>
        /// Draws the crystal body using BrightStarProjectile1 as the visible star shape.
        /// 
        /// The star is drawn in two passes:
        /// 1. Additive pass WITHOUT shader — draws the actual star sprite with theme tinting
        ///    so the star shape is clearly visible and properly colored.
        /// 2. Additive pass WITH CrystalShimmerShader — draws a smaller overlay for the
        ///    prismatic faceted shimmer effect on top.
        /// 
        /// Additional layers:
        /// - Counter-rotated BrightStarProjectile2 overlay for depth
        /// - Star flare cross gleam through center
        /// </summary>
        private void DrawShaderCrystalBody(SpriteBatch sb, Color[] themeColors, float lifeFade)
        {
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float time = (float)Main.timeForVisualEffects;
            float pulse = 0.9f + 0.1f * MathF.Sin(time * 0.1f + sparkleSeed);

            Texture2D body = SPFTextures.CrystalBody.Value;
            Texture2D overlay = SPFTextures.CrystalOverlay.Value;
            Texture2D starFlare = SPFTextures.StarFlare.Value;

            // ---- PASS 1: DRAW STAR BODY DIRECTLY (no shader, additive) ----
            // Already in additive from bloom halo layer
            float bodyScale = CrystalBaseScale * pulse;

            // Main star body — theme primary color tinted
            sb.Draw(body, drawPos, null, themeColors[0] * (lifeFade * 0.7f), bodyRotation,
                body.Size() / 2f, bodyScale, SpriteEffects.None, 0f);

            // Bright white-hot core version at slightly smaller scale
            sb.Draw(body, drawPos, null, Color.White * (lifeFade * 0.5f), bodyRotation,
                body.Size() / 2f, bodyScale * 0.6f, SpriteEffects.None, 0f);

            // Counter-rotated overlay for depth
            float overlayScale = CrystalBaseScale * 0.55f * pulse;
            sb.Draw(overlay, drawPos, null, themeColors[2] * (lifeFade * 0.35f), -bodyRotation * 0.7f,
                overlay.Size() / 2f, overlayScale, SpriteEffects.None, 0f);

            // Star flare cross gleam
            float flareScale = CrystalBaseScale * 0.5f;
            sb.Draw(starFlare, drawPos, null, themeColors[4] * (lifeFade * 0.3f), bodyRotation * 0.3f,
                starFlare.Size() / 2f, flareScale, SpriteEffects.None, 0f);

            // ---- PASS 2: SHIMMER OVERLAY (with CrystalShimmerShader) ----
            if (crystalShader == null)
            {
                crystalShader = ModContent.Request<Effect>(
                    "MagnumOpus/Content/FoundationWeapons/SparkleProjectileFoundation/Shaders/CrystalShimmerShader",
                    AssetRequestMode.ImmediateLoad).Value;
            }

            crystalShader.Parameters["uTime"]?.SetValue(time * 0.02f);
            crystalShader.Parameters["rotation"]?.SetValue(bodyRotation);
            crystalShader.Parameters["shimmerSpeed"]?.SetValue(2.0f);
            crystalShader.Parameters["flashIntensity"]?.SetValue(1.5f);
            crystalShader.Parameters["baseAlpha"]?.SetValue(lifeFade * 0.6f);
            crystalShader.Parameters["primaryColor"]?.SetValue(themeColors[0].ToVector3());
            crystalShader.Parameters["highlightColor"]?.SetValue(themeColors[4].ToVector3());
            crystalShader.Parameters["gradientTex"]?.SetValue(
                SPFTextures.GetGradientForTheme(CurrentTheme));

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, crystalShader,
                Main.GameViewMatrix.TransformationMatrix);

            // Shimmer overlay — smaller than the main body, adds prismatic flash on top
            float shimmerScale = CrystalBaseScale * 0.8f * pulse;
            sb.Draw(body, drawPos, null, Color.White * lifeFade, bodyRotation,
                body.Size() / 2f, shimmerScale, SpriteEffects.None, 0f);

            // ---- END SHADER BATCH ----
            sb.End();

            // Restart in additive without shader for subsequent layers
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.TransformationMatrix);
        }

        // =====================================================================
        // LAYER 4: SPARKLE ACCENTS — Orbiting sparkle points (no shader)
        // =====================================================================

        /// <summary>
        /// Draws sparkle accent bursts — 4 orbiting sharp 4-pointed stars that flash
        /// around the crystal with randomized cubic sin-wave timing, plus a central
        /// twinkle pulse with very brief pow(6) flash peaks.
        /// 
        /// These supplement the shader effects with discrete accent points that break
        /// the crystal silhouette and add high-frequency visual interest.
        /// </summary>
        private void DrawSparkleAccents(SpriteBatch sb, Color[] themeColors, float lifeFade)
        {
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float time = (float)Main.timeForVisualEffects;

            Texture2D sparkleHard = SPFTextures.SparkleHard.Value;
            Texture2D sparkleThin = SPFTextures.SparkleThin.Value;
            Vector2 hardOrigin = sparkleHard.Size() / 2f;
            Vector2 thinOrigin = sparkleThin.Size() / 2f;

            // 4 sparkle points orbiting the crystal at different phases
            for (int i = 0; i < 4; i++)
            {
                float angle = time * 0.06f + i * MathHelper.PiOver2 + sparkleSeed;
                float radius = 4f + 2f * MathF.Sin(time * 0.1f + i * 1.5f);
                Vector2 offset = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * radius;

                float flash = MathF.Max(0f, MathF.Sin(time * 0.12f + i * 2.3f + sparkleSeed));
                flash = flash * flash * flash; // Cubic peaks

                if (flash < 0.05f)
                    continue;

                float sparkleAlpha = flash * lifeFade;
                float hardScale = 0.03f * (0.5f + flash);
                float hardRot = time * 0.15f + i * 1.1f;

                sb.Draw(sparkleHard, drawPos + offset, null, themeColors[4] * (sparkleAlpha * 0.8f),
                    hardRot, hardOrigin, hardScale, SpriteEffects.None, 0f);

                if (flash > 0.3f)
                {
                    Color thinColor = Color.Lerp(themeColors[0], themeColors[4], flash) * (sparkleAlpha * 0.5f);
                    float thinScale = 0.02f * (0.3f + flash * 0.7f);
                    sb.Draw(sparkleThin, drawPos + offset, null, thinColor, hardRot + MathHelper.PiOver4,
                        thinOrigin, thinScale, SpriteEffects.None, 0f);
                }
            }

            // Central twinkle pulse
            float centralFlash = MathF.Max(0f, MathF.Sin(time * 0.07f + sparkleSeed * 3f));
            centralFlash = MathF.Pow(centralFlash, 6f);
            if (centralFlash > 0.1f)
            {
                sb.Draw(sparkleHard, drawPos, null, themeColors[4] * (centralFlash * lifeFade * 0.6f),
                    bodyRotation * 0.5f, hardOrigin, 0.05f * centralFlash, SpriteEffects.None, 0f);
            }
        }

        // =====================================================================
        // COMBAT
        // =====================================================================

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Color[] themeColors = SPFTextures.GetThemeColors(CurrentTheme);
            for (int i = 0; i < 8; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(6f, 6f);
                Color col = themeColors[Main.rand.Next(themeColors.Length)];
                Dust dust = Dust.NewDustPerfect(target.Center, DustID.RainbowMk2, vel,
                    newColor: col, Scale: Main.rand.NextFloat(0.3f, 0.6f));
                dust.noGravity = true;
                dust.fadeIn = 0.5f;
            }
        }

        public override void OnKill(int timeLeft)
        {
            Color[] themeColors = SPFTextures.GetThemeColors(CurrentTheme);
            for (int i = 0; i < 12; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(5f, 5f);
                Color col = themeColors[Main.rand.Next(themeColors.Length)];
                Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.RainbowMk2, vel,
                    newColor: col, Scale: Main.rand.NextFloat(0.25f, 0.5f));
                dust.noGravity = true;
                dust.fadeIn = 0.6f;
            }
        }

        public override Color? GetAlpha(Color lightColor)
        {
            return Color.White;
        }
    }
}
