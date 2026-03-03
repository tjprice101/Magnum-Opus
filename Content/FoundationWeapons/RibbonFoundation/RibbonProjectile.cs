using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.FoundationWeapons.RibbonFoundation
{
    /// <summary>
    /// RibbonProjectile — A music note orb projectile that leaves one of 10 different
    /// ribbon trail styles behind it, demonstrating the full range of trail rendering
    /// techniques available in the VFX Asset Library.
    /// 
    /// RENDERING ARCHITECTURE (no custom shaders — all CPU-side SpriteBatch):
    /// 
    /// 1. RIBBON TRAIL — The main visual. 10 different rendering approaches:
    ///    Modes 1-2: Bloom sprite stacking along position history (additive)
    ///    Modes 3-10: Texture-strip rendering — draws a UV-mapped ribbon by placing
    ///    texture samples at each position history point, stretched and oriented
    ///    along the velocity direction with width tapering toward the tail.
    /// 
    /// 2. PROJECTILE BODY — Pulsating Music Note Orb drawn at projectile center,
    ///    scaled down from 1024x1024 to ~32px display size.
    /// 
    /// 3. BLOOM HALO — Soft glow around the projectile head for luminous presence.
    /// 
    /// POSITION HISTORY:
    /// - Ring buffer of 40 positions recorded each AI tick
    /// - extraUpdates = 1 doubles recording density for smoother ribbons
    /// - Positions are unwound into ordered arrays for rendering
    /// 
    /// ai[0] = Ribbon mode index (0-9)
    /// </summary>
    public class RibbonProjectile : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.RainbowRodBullet;

        // ---- CONSTANTS ----
        private const int TrailLength = 40;
        private const int MaxLifetime = 240;
        private const int FadeOutFrames = 30;

        /// <summary>Display scale for the 1024x1024 orb texture. 0.035 ≈ 36px on screen.</summary>
        private const float OrbDisplayScale = 0.035f;

        /// <summary>Head width of the ribbon in world pixels.</summary>
        private const float RibbonWidthHead = 20f;

        /// <summary>Tail width of the ribbon in world pixels.</summary>
        private const float RibbonWidthTail = 2f;

        // ---- STATE ----
        private Vector2[] trailPositions;
        private float[] trailRotations;
        private int trailIndex;
        private int trailCount;
        private int timer;
        private float seed;

        private RibbonMode CurrentMode => (RibbonMode)(int)Projectile.ai[0];

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.DrawScreenCheckFluff[Projectile.type] = 600;
        }

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.penetrate = 3;
            Projectile.tileCollide = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.timeLeft = MaxLifetime;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 15;
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
                seed = Main.rand.NextFloat(100f);
            }

            timer++;

            // ---- RECORD TRAIL ----
            trailPositions[trailIndex] = Projectile.Center;
            trailRotations[trailIndex] = Projectile.velocity.ToRotation();
            trailIndex = (trailIndex + 1) % TrailLength;
            if (trailCount < TrailLength)
                trailCount++;

            // ---- ROTATION ----
            Projectile.rotation = Projectile.velocity.ToRotation();

            // ---- GENTLE GRAVITY (slight arc) ----
            Projectile.velocity.Y += 0.02f;

            // ---- LIGHTING ----
            Color[] colors = RBFTextures.GetModeColors(CurrentMode);
            float pulse = 0.4f + 0.1f * MathF.Sin((float)Main.timeForVisualEffects * 0.08f + seed);
            Lighting.AddLight(Projectile.Center, colors[0].ToVector3() * pulse);
        }

        // =====================================================================
        // RENDERING
        // =====================================================================

        public override bool PreDraw(ref Color lightColor)
        {
            if (trailCount < 3)
                return false;

            SpriteBatch sb = Main.spriteBatch;
            Color[] modeColors = RBFTextures.GetModeColors(CurrentMode);

            float lifeFade = Projectile.timeLeft < FadeOutFrames
                ? Projectile.timeLeft / (float)FadeOutFrames
                : 1f;

            // ---- UNWIND RING BUFFER (oldest → newest) ----
            Vector2[] positions = new Vector2[trailCount];
            float[] rotations = new float[trailCount];
            for (int i = 0; i < trailCount; i++)
            {
                int bufIdx = (trailIndex - trailCount + i + TrailLength * 2) % TrailLength;
                positions[i] = trailPositions[bufIdx];
                rotations[i] = trailRotations[bufIdx];
            }

            // ---- DRAW RIBBON (switch into additive) ----
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive,
                SamplerState.LinearWrap, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.TransformationMatrix);

            switch (CurrentMode)
            {
                case RibbonMode.PureBloom:
                    DrawPureBloomRibbon(sb, positions, modeColors, lifeFade);
                    break;
                case RibbonMode.BloomNoiseFade:
                    DrawBloomNoiseFadeRibbon(sb, positions, modeColors, lifeFade);
                    break;
                case RibbonMode.BasicTrailStrip:
                    DrawTextureStripRibbon(sb, positions, rotations, RBFTextures.BasicTrail.Value, modeColors, lifeFade);
                    break;
                case RibbonMode.HarmonicWave:
                    DrawTextureStripRibbon(sb, positions, rotations, RBFTextures.HarmonicWaveRibbon.Value, modeColors, lifeFade);
                    break;
                case RibbonMode.SpiralingVortex:
                    DrawTextureStripRibbon(sb, positions, rotations, RBFTextures.SpiralingVortexStrip.Value, modeColors, lifeFade);
                    break;
                case RibbonMode.EnergySurge:
                    DrawTextureStripRibbon(sb, positions, rotations, RBFTextures.EnergySurgeBeam.Value, modeColors, lifeFade);
                    break;
                case RibbonMode.CosmicNebula:
                    DrawTextureStripRibbon(sb, positions, rotations, RBFTextures.CosmicNebulaClouds.Value, modeColors, lifeFade);
                    break;
                case RibbonMode.MusicalWave:
                    DrawTextureStripRibbon(sb, positions, rotations, RBFTextures.MusicalWavePattern.Value, modeColors, lifeFade);
                    break;
                case RibbonMode.MarbleFlow:
                    DrawTextureStripRibbon(sb, positions, rotations, RBFTextures.TileableMarbleNoise.Value, modeColors, lifeFade);
                    break;
                case RibbonMode.LightningRibbon:
                    DrawLightningRibbon(sb, positions, rotations, modeColors, lifeFade);
                    break;
            }

            // ---- DRAW PROJECTILE HEAD BLOOM ----
            DrawHeadBloom(sb, modeColors, lifeFade);

            // ---- DRAW ORB BODY ----
            DrawOrbBody(sb, modeColors, lifeFade);

            // ---- RESTORE ----
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.TransformationMatrix);

            return false;
        }

        // =====================================================================
        // MODE 1: PURE BLOOM — Bright bloom sprites stacked along trail
        // =====================================================================

        /// <summary>
        /// Renders the ribbon as overlapping bloom sprites at each recorded position.
        /// Three layers per point (outer glow, main body, hot core) create depth.
        /// The blooms are velocity-stretched for a smooth flowing appearance.
        /// </summary>
        private void DrawPureBloomRibbon(SpriteBatch sb, Vector2[] positions, Color[] colors, float lifeFade)
        {
            Texture2D bloomTex = RBFTextures.SoftGlowBright.Value;
            Texture2D coreTex = RBFTextures.PointBloom.Value;
            Vector2 bloomOrigin = bloomTex.Size() / 2f;
            Vector2 coreOrigin = coreTex.Size() / 2f;

            for (int i = 0; i < positions.Length; i++)
            {
                float progress = (float)i / positions.Length; // 0 = oldest, 1 = newest
                float reverseProg = 1f - progress; // 1 = oldest (tail), 0 = newest (head)

                // Fade: strong at head, fading at tail (cubic falloff)
                float fade = progress * progress * lifeFade;
                if (fade < 0.01f) continue;

                // Width scales from tail to head
                float width = MathHelper.Lerp(RibbonWidthTail, RibbonWidthHead, progress);
                float scale = width / bloomTex.Width;

                // Velocity direction for stretching
                Vector2 vel;
                if (i < positions.Length - 1)
                    vel = positions[i + 1] - positions[i];
                else
                    vel = Projectile.velocity;
                float rot = vel.ToRotation() + MathHelper.PiOver2;

                Vector2 pos = positions[i] - Main.screenPosition;

                // Stretch along velocity
                float stretchX = scale;
                float stretchY = scale * 2.5f;

                // Outer glow — large, soft, theme-colored
                Color outerColor = colors[1] * (fade * 0.35f);
                sb.Draw(bloomTex, pos, null, outerColor, rot, bloomOrigin,
                    new Vector2(stretchX * 1.8f, stretchY * 1.4f), SpriteEffects.None, 0f);

                // Main body — brighter, white→themed transition
                Color bodyColor = Color.Lerp(Color.White, colors[0], reverseProg) * (fade * 0.6f);
                sb.Draw(bloomTex, pos, null, bodyColor, rot, bloomOrigin,
                    new Vector2(stretchX, stretchY), SpriteEffects.None, 0f);

                // Hot core — pure white, tight
                Color coreColor = Color.White * (fade * 0.5f * progress);
                sb.Draw(coreTex, pos, null, coreColor, rot, coreOrigin,
                    new Vector2(stretchX * 0.4f, stretchY * 0.6f), SpriteEffects.None, 0f);
            }
        }

        // =====================================================================
        // MODE 2: BLOOM + NOISE FADE — Blooms with noise texture erosion
        // =====================================================================

        /// <summary>
        /// Like PureBloom but uses a noise texture to modulate each bloom point's opacity,
        /// creating an organic breaking-apart effect at the tail. The noise is sampled
        /// based on position and time for animated erosion.
        /// </summary>
        private void DrawBloomNoiseFadeRibbon(SpriteBatch sb, Vector2[] positions, Color[] colors, float lifeFade)
        {
            Texture2D bloomTex = RBFTextures.SoftGlow.Value;
            Texture2D noiseTex = RBFTextures.PerlinNoise.Value;
            Vector2 bloomOrigin = bloomTex.Size() / 2f;

            float time = (float)Main.timeForVisualEffects * 0.01f;

            // Pre-sample noise values by reading pixel data
            // We approximate a noise lookup by using position-based hash
            for (int i = 0; i < positions.Length; i++)
            {
                float progress = (float)i / positions.Length;
                float fade = progress * progress * lifeFade;
                if (fade < 0.01f) continue;

                // Noise erosion: sample based on index and time
                // As we approach the tail (progress → 0), raise the erosion threshold
                float erosionThreshold = (1f - progress) * 0.9f; // Higher threshold at tail
                float noisePhase = (i * 0.3f + time + seed) % 6.28f;
                float noiseValue = 0.5f + 0.5f * MathF.Sin(noisePhase * 3.7f)
                                       * MathF.Cos(noisePhase * 2.3f + i * 0.5f);

                // If noise is below threshold, skip this point (eroded away)
                if (noiseValue < erosionThreshold)
                    continue;

                // Surviving points get extra brightness boost from noise
                float noiseBrightness = MathHelper.Clamp((noiseValue - erosionThreshold) * 3f, 0f, 1f);

                float width = MathHelper.Lerp(RibbonWidthTail, RibbonWidthHead, progress);
                float scale = width / bloomTex.Width;

                Vector2 vel;
                if (i < positions.Length - 1)
                    vel = positions[i + 1] - positions[i];
                else
                    vel = Projectile.velocity;
                float rot = vel.ToRotation() + MathHelper.PiOver2;

                Vector2 pos = positions[i] - Main.screenPosition;

                float effFade = fade * noiseBrightness;

                // Second noise layer for variety
                float noise2 = 0.5f + 0.5f * MathF.Sin(noisePhase * 5.1f + 1.7f);

                // Outer — uses FBM-like secondary noise to vary halo size
                float outerScale = scale * (1.4f + noise2 * 0.6f);
                Color outerColor = colors[1] * (effFade * 0.4f);
                sb.Draw(bloomTex, pos, null, outerColor, rot, bloomOrigin,
                    new Vector2(outerScale, outerScale * 2f), SpriteEffects.None, 0f);

                // Core — pulsing brightness
                float corePulse = 0.7f + 0.3f * MathF.Sin(time * 5f + i * 0.8f);
                Color coreColor = Color.Lerp(colors[0], Color.White, corePulse * 0.5f) * (effFade * 0.7f);
                sb.Draw(bloomTex, pos, null, coreColor, rot, bloomOrigin,
                    new Vector2(scale * 0.6f, scale * 1.8f), SpriteEffects.None, 0f);
            }
        }

        // =====================================================================
        // MODES 3-9: TEXTURE STRIP RIBBON — UV-oriented texture along trail
        // =====================================================================

        /// <summary>
        /// Generic texture-strip ribbon renderer. Takes any texture and draws it as
        /// a UV-mapped ribbon along the position history.
        /// 
        /// Each position in the trail gets a small quad of the texture, oriented
        /// perpendicular to the trail direction, with UV.x mapping along trail
        /// progress (0 at tail, 1 at head) and UV.y mapping across the width.
        /// 
        /// The texture is drawn as overlapping sprites with source rectangles that
        /// sample sequential horizontal slices, creating the illusion of a continuous
        /// UV-mapped strip without needing a custom vertex mesh.
        /// </summary>
        private void DrawTextureStripRibbon(SpriteBatch sb, Vector2[] positions, float[] rotations,
            Texture2D stripTex, Color[] colors, float lifeFade)
        {
            int texW = stripTex.Width;
            int texH = stripTex.Height;
            float time = (float)Main.timeForVisualEffects * 0.005f;

            // We need at least 2 points to draw segments
            if (positions.Length < 2) return;

            int srcWidth = Math.Max(1, texW / positions.Length);

            for (int i = 0; i < positions.Length - 1; i++)
            {
                float progress = (float)i / positions.Length; // 0 = oldest (tail), 1 = newest (head)
                float fade = progress * progress * lifeFade;
                if (fade < 0.01f) continue;

                // Width of the ribbon at this point
                float width = MathHelper.Lerp(RibbonWidthTail, RibbonWidthHead, progress);

                // Direction and length of this trail segment
                Vector2 segDir = positions[i + 1] - positions[i];
                float segLength = segDir.Length();
                if (segLength < 0.5f) continue;
                float segAngle = segDir.ToRotation();

                // UV.x — maps along the texture width, scrolling over time
                float uStart = (progress + time * 2f) % 1f;
                int srcX = (int)(uStart * texW) % texW;

                // Source rectangle: a vertical slice of the texture
                Rectangle srcRect = new Rectangle(srcX, 0, srcWidth, texH);

                // ScaleX: stretch source slice to cover the segment distance in world pixels
                float scaleX = segLength / (float)srcWidth;
                // ScaleY: match ribbon width in world pixels
                float scaleY = width / (float)texH;

                Vector2 pos = positions[i] - Main.screenPosition;
                // Origin at left-center so sprite stretches from pos[i] toward pos[i+1]
                Vector2 origin = new Vector2(0, texH / 2f);

                // Draw main body — theme tinted
                Color bodyColor = Color.Lerp(colors[1], colors[0], progress) * (fade * 0.8f);
                sb.Draw(stripTex, pos, srcRect, bodyColor, segAngle, origin,
                    new Vector2(scaleX, scaleY), SpriteEffects.None, 0f);

                // Draw hot core at higher progress (near head) — white-tinted
                if (progress > 0.4f)
                {
                    float coreFade = (progress - 0.4f) / 0.6f;
                    Color coreColor = Color.Lerp(colors[2], Color.White, coreFade * 0.5f) * (fade * coreFade * 0.4f);
                    sb.Draw(stripTex, pos, srcRect, coreColor, segAngle, origin,
                        new Vector2(scaleX * 0.5f, scaleY * 0.5f), SpriteEffects.None, 0f);
                }
            }

            // Draw a soft bloom overlay along the trail for glow
            DrawRibbonBloomOverlay(sb, positions, colors, lifeFade, 0.4f);
        }

        // =====================================================================
        // MODE 10: LIGHTNING RIBBON — Electric bolt + bloom hybrid
        // =====================================================================

        /// <summary>
        /// Combines the LightningSurge texture drawn along the trail with aggressive
        /// bloom stacking and random jitter offsets for an electric crackling appearance.
        /// </summary>
        private void DrawLightningRibbon(SpriteBatch sb, Vector2[] positions, float[] rotations,
            Color[] colors, float lifeFade)
        {
            Texture2D lightningTex = RBFTextures.LightningSurge.Value;
            Texture2D bloomTex = RBFTextures.SoftGlow.Value;
            Vector2 bloomOrigin = bloomTex.Size() / 2f;

            int texW = lightningTex.Width;
            int texH = lightningTex.Height;
            float time = (float)Main.timeForVisualEffects * 0.008f;

            int srcWidth = Math.Max(1, texW / positions.Length);

            for (int i = 0; i < positions.Length - 1; i++)
            {
                float progress = (float)i / positions.Length;
                float fade = progress * progress * lifeFade;
                if (fade < 0.01f) continue;

                float width = MathHelper.Lerp(RibbonWidthTail * 1.5f, RibbonWidthHead * 1.2f, progress);

                // Direction and length of this trail segment
                Vector2 segDir = positions[i + 1] - positions[i];
                float segLength = segDir.Length();
                if (segLength < 0.5f) continue;
                float segAngle = segDir.ToRotation();

                // UV scrolling with faster speed for electric energy feel
                float uStart = (progress + time * 4f) % 1f;
                int srcX = (int)(uStart * texW) % texW;
                Rectangle srcRect = new Rectangle(srcX, 0, srcWidth, texH);

                // ScaleX: stretch source slice to cover segment distance
                float scaleX = segLength / (float)srcWidth;
                float scaleY = width / (float)texH;

                // Random jitter for electric flickering
                float jitterX = MathF.Sin(time * 20f + i * 3.7f) * 2f * (1f - progress);
                float jitterY = MathF.Cos(time * 17f + i * 2.9f) * 2f * (1f - progress);
                Vector2 jitter = new Vector2(jitterX, jitterY);

                Vector2 pos = positions[i] - Main.screenPosition + jitter;
                // Origin at left-center so sprite stretches from pos[i] toward pos[i+1]
                Vector2 origin = new Vector2(0, texH / 2f);

                // Lightning texture — bright
                Color lightningColor = colors[0] * (fade * 0.9f);
                sb.Draw(lightningTex, pos, srcRect, lightningColor, segAngle, origin,
                    new Vector2(scaleX, scaleY), SpriteEffects.None, 0f);

                // Electric bloom at every few points — sharp bright flashes
                if (i % 3 == 0)
                {
                    float flash = 0.5f + 0.5f * MathF.Sin(time * 15f + i * 4.2f);
                    flash = flash * flash; // Square for sharp peaks
                    Color bloomColor = Color.White * (fade * flash * 0.5f);
                    float bloomScale = width / bloomTex.Width * 2f;
                    sb.Draw(bloomTex, pos, null, bloomColor, 0f, bloomOrigin,
                        bloomScale, SpriteEffects.None, 0f);
                }
            }

            // Extra aggressive bloom overlay for the electric glow
            DrawRibbonBloomOverlay(sb, positions, colors, lifeFade, 0.6f);
        }

        // =====================================================================
        // SHARED: Bloom overlay along trail (used by texture strip modes)
        // =====================================================================

        /// <summary>
        /// Draws a soft bloom glow along the trail at a subset of points
        /// to give the ribbon an ambient luminous quality.
        /// </summary>
        private void DrawRibbonBloomOverlay(SpriteBatch sb, Vector2[] positions, Color[] colors,
            float lifeFade, float intensity)
        {
            Texture2D bloomTex = RBFTextures.SoftGlow.Value;
            Vector2 bloomOrigin = bloomTex.Size() / 2f;

            int step = Math.Max(1, positions.Length / 15); // ~15 bloom points max

            for (int i = 0; i < positions.Length; i += step)
            {
                float progress = (float)i / positions.Length;
                float fade = progress * progress * lifeFade * intensity;
                if (fade < 0.01f) continue;

                float width = MathHelper.Lerp(RibbonWidthTail, RibbonWidthHead, progress);
                float scale = width / bloomTex.Width * 1.5f;

                Vector2 pos = positions[i] - Main.screenPosition;

                Color glowColor = colors[0] * (fade * 0.35f);
                sb.Draw(bloomTex, pos, null, glowColor, 0f, bloomOrigin, scale, SpriteEffects.None, 0f);
            }
        }

        // =====================================================================
        // HEAD BLOOM — Glow halo at projectile position
        // =====================================================================

        /// <summary>
        /// Draws a multi-layer bloom halo at the projectile head for luminous presence.
        /// </summary>
        private void DrawHeadBloom(SpriteBatch sb, Color[] colors, float lifeFade)
        {
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float time = (float)Main.timeForVisualEffects;
            float pulse = 0.85f + 0.15f * MathF.Sin(time * 0.08f + seed);

            Texture2D softGlow = RBFTextures.SoftGlow.Value;
            Texture2D starFlare = RBFTextures.StarFlare.Value;
            Vector2 glowOrigin = softGlow.Size() / 2f;
            Vector2 flareOrigin = starFlare.Size() / 2f;

            // Outer ambient glow
            sb.Draw(softGlow, drawPos, null, colors[1] * (0.25f * lifeFade * pulse), 0f,
                glowOrigin, 0.08f * pulse, SpriteEffects.None, 0f);

            // Mid glow
            sb.Draw(softGlow, drawPos, null, colors[0] * (0.4f * lifeFade * pulse), 0f,
                glowOrigin, 0.05f * pulse, SpriteEffects.None, 0f);

            // Inner core
            sb.Draw(softGlow, drawPos, null, Color.White * (0.35f * lifeFade * pulse), 0f,
                glowOrigin, 0.025f * pulse, SpriteEffects.None, 0f);

            // Star flare cross
            sb.Draw(starFlare, drawPos, null, colors[2] * (0.2f * lifeFade * pulse), time * 0.01f,
                flareOrigin, 0.03f * pulse, SpriteEffects.None, 0f);
        }

        // =====================================================================
        // ORB BODY — Pulsating Music Note Orb scaled down
        // =====================================================================

        /// <summary>
        /// Draws the Pulsating Music Note Orb texture scaled down from 1024x1024
        /// to approximately 36px on screen, with theme color tinting.
        /// </summary>
        private void DrawOrbBody(SpriteBatch sb, Color[] colors, float lifeFade)
        {
            Texture2D orbTex = RBFTextures.MusicNoteOrb.Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = orbTex.Size() / 2f;

            float time = (float)Main.timeForVisualEffects;
            float pulse = 0.95f + 0.05f * MathF.Sin(time * 0.1f + seed);
            float scale = OrbDisplayScale * pulse;

            // Main orb — theme primary tint
            Color orbColor = colors[0] * (lifeFade * 0.85f);
            sb.Draw(orbTex, drawPos, null, orbColor, 0f, origin, scale, SpriteEffects.None, 0f);

            // White-hot inner overlay — slightly smaller
            Color innerColor = Color.White * (lifeFade * 0.4f * pulse);
            sb.Draw(orbTex, drawPos, null, innerColor, 0f, origin, scale * 0.7f, SpriteEffects.None, 0f);
        }

        // =====================================================================
        // COMBAT
        // =====================================================================

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Color[] colors = RBFTextures.GetModeColors(CurrentMode);
            for (int i = 0; i < 6; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(5f, 5f);
                Color col = colors[Main.rand.Next(colors.Length)];
                Dust dust = Dust.NewDustPerfect(target.Center, DustID.RainbowMk2, vel,
                    newColor: col, Scale: Main.rand.NextFloat(0.2f, 0.5f));
                dust.noGravity = true;
                dust.fadeIn = 0.4f;
            }
        }

        public override void OnKill(int timeLeft)
        {
            Color[] colors = RBFTextures.GetModeColors(CurrentMode);
            for (int i = 0; i < 10; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(4f, 4f);
                Color col = colors[Main.rand.Next(colors.Length)];
                Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.RainbowMk2, vel,
                    newColor: col, Scale: Main.rand.NextFloat(0.2f, 0.45f));
                dust.noGravity = true;
                dust.fadeIn = 0.5f;
            }
        }

        public override Color? GetAlpha(Color lightColor) => Color.White;
    }
}
