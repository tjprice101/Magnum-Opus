using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Content.DiesIrae;
using MagnumOpus.Content.DiesIrae.Weapons.DeathTollingBell.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.DiesIrae.Weapons.DeathTollingBell.Utilities
{
    /// <summary>
    /// Static VFX utility for Death Tolling Bell.
    /// Spectral bell aesthetic — concentric toll wave rings, state-driven bell glow,
    /// funeral march smoke burst, standing wave tether, noise-masked body rendering.
    /// Integrates with BellToll.fx shader and the particle system.
    /// </summary>
    public static class DeathTollingBellUtils
    {
        // ═══════════════════════════════════════════════════════
        //  PALETTE SHORTCUTS
        // ═══════════════════════════════════════════════════════
        public static Color TollCrimson => DiesIraePalette.BloodRed;
        public static Color TollEmber => DiesIraePalette.EmberOrange;
        public static Color TollGold => DiesIraePalette.JudgmentGold;
        public static Color FlashWhite => DiesIraePalette.WrathWhite;
        public static Color BellBody => new Color(100, 30, 15);
        public static Color FuneralBlack => DiesIraePalette.CharcoalBlack;

        // ═══════════════════════════════════════════════════════
        //  SHADER CACHE
        // ═══════════════════════════════════════════════════════
        private static Effect _bellTollShader;
        private static Effect _rippleShader;

        public static Effect GetBellTollShader()
        {
            if (_bellTollShader == null)
            {
                try
                {
                    _bellTollShader = ModContent.Request<Effect>(
                        "MagnumOpus/Effects/DiesIrae/DeathTollingBell/BellToll",
                        AssetRequestMode.ImmediateLoad).Value;
                }
                catch { }
            }
            return _bellTollShader;
        }

        public static Effect GetRippleShader()
        {
            if (_rippleShader == null)
            {
                try
                {
                    _rippleShader = ModContent.Request<Effect>(
                        "MagnumOpus/Content/FoundationWeapons/ImpactFoundation/Shaders/RippleShader",
                        AssetRequestMode.ImmediateLoad).Value;
                }
                catch { }
            }
            return _rippleShader;
        }

        // ═══════════════════════════════════════════════════════
        //  EASING HELPERS
        // ═══════════════════════════════════════════════════════
        public static float EaseOutCubic(float t) => 1f - MathF.Pow(1f - t, 3f);
        public static float EaseOutQuad(float t) => 1f - (1f - t) * (1f - t);
        public static float SinePulse(float timer, float freq = 0.2f) => 0.85f + 0.15f * MathF.Sin(timer * freq);

        // ═══════════════════════════════════════════════════════
        //  BELL BODY RENDERING — Multi-layer state-driven glow
        // ═══════════════════════════════════════════════════════

        /// <summary>
        /// Draws the bell body with state-driven layered glow.
        /// States: 0=idle, 1=charging, 2=toll moment, 3=funeral march flash.
        /// 4-layer rendering: bell sprite → inner fire glow → state bloom → outer haze.
        /// </summary>
        public static void DrawBellBody(SpriteBatch sb, Vector2 worldPos, int state, float timer, Texture2D bellTexture = null)
        {
            Vector2 drawPos = worldPos - Main.screenPosition;

            // Layer 0: Bell sprite with state-driven coloring
            if (bellTexture != null)
            {
                Vector2 origin = bellTexture.Size() / 2f;
                float swing = MathF.Sin(timer * 0.08f) * 0.05f;

                Color tint = state switch
                {
                    0 => Color.White * 0.9f,
                    1 => Color.Lerp(Color.White, TollEmber, 0.3f),
                    2 => Color.Lerp(Color.White, TollGold, 0.6f),
                    3 => Color.Lerp(Color.White, FlashWhite, 0.8f),
                    _ => Color.White,
                };

                // Subtle scale pulse during charging/toll
                float spriteScale = 1f;
                if (state == 1) spriteScale = 1f + 0.02f * MathF.Sin(timer * 0.4f);
                else if (state >= 2) spriteScale = 1f + 0.05f * MathF.Sin(timer * 0.8f);

                sb.Draw(bellTexture, drawPos, null, tint, swing, origin, spriteScale, SpriteEffects.None, 0f);
            }

            // State properties
            float intensity = state switch { 0 => 1f, 1 => 1.5f, 2 => 2.5f, 3 => 3f, _ => 1f };
            float pulse = SinePulse(timer);
            Color stateColor = state switch
            {
                0 => TollCrimson,
                1 => TollEmber,
                2 => TollGold,
                3 => FlashWhite,
                _ => TollCrimson,
            };

            // Layer 1: Inner fire glow — tight around the bell using CosmicVortex-like swirling
            Texture2D glow = BellTextures.SoftGlow;
            if (glow == null) return;
            Vector2 glowOrigin = glow.Size() / 2f;

            float baseScale = 0.04f + (intensity - 1f) * 0.018f;

            // Inner core — bright, tight
            sb.Draw(glow, drawPos, null, FlashWhite * 0.35f * pulse * (intensity / 2.5f), 0f, glowOrigin,
                baseScale * 0.4f, SpriteEffects.None, 0f);

            // Layer 2: State bloom — mid-range glow
            Color midColor = Color.Lerp(stateColor, TollGold, 0.3f);
            sb.Draw(glow, drawPos, null, midColor * 0.45f * pulse, 0f, glowOrigin,
                baseScale, SpriteEffects.None, 0f);

            // Layer 3: Outer haze — wide, soft
            sb.Draw(glow, drawPos, null, stateColor * 0.2f * pulse, 0f, glowOrigin,
                baseScale * 2.2f, SpriteEffects.None, 0f);

            // Layer 4: Star flare accent during toll/funeral march
            if (state >= 2)
            {
                Texture2D flare = BellTextures.DIStarFlare ?? BellTextures.StarFlare;
                if (flare != null)
                {
                    Vector2 flareOrigin = flare.Size() / 2f;
                    float flareAlpha = (state == 3) ? 0.6f : 0.35f;
                    float flareScale = baseScale * 0.8f * (1f + 0.15f * MathF.Sin(timer * 0.5f));
                    float flareRot = timer * 0.02f;
                    sb.Draw(flare, drawPos, null, TollGold * flareAlpha * pulse, flareRot, flareOrigin,
                        flareScale, SpriteEffects.None, 0f);
                }
            }

            // Lighting
            float lightBrightness = 0.3f + intensity * 0.15f;
            Lighting.AddLight(worldPos, stateColor.ToVector3() * lightBrightness);
        }

        // ═══════════════════════════════════════════════════════
        //  TOLL WAVE RING DRAWING — Shader-driven expanding rings
        // ═══════════════════════════════════════════════════════

        /// <summary>
        /// Draws expanding toll wave using RippleShader from ImpactFoundation.
        /// The shader renders concentric rings with noise distortion on a SoftCircle quad.
        /// </summary>
        public static void DrawTollWaveShader(SpriteBatch sb, Vector2 center, float progress,
            float fadeAlpha, float expandScale, int ringCount, bool isFuneral, float seed)
        {
            Effect shader = GetRippleShader();
            if (shader == null)
            {
                // Fallback to non-shader rings
                float radius = expandScale * 300f;
                DrawTollWaveRingFallback(sb, center, radius, fadeAlpha, isFuneral);
                return;
            }

            float time = (float)Main.timeForVisualEffects;

            // Configure shader
            shader.Parameters["uTime"]?.SetValue(time * 0.02f + seed);
            shader.Parameters["progress"]?.SetValue(progress);
            shader.Parameters["ringCount"]?.SetValue((float)ringCount);
            shader.Parameters["ringThickness"]?.SetValue(isFuneral ? 0.08f : 0.05f);
            shader.Parameters["primaryColor"]?.SetValue(TollCrimson.ToVector3());
            shader.Parameters["secondaryColor"]?.SetValue(TollEmber.ToVector3());
            shader.Parameters["coreColor"]?.SetValue(TollGold.ToVector3());
            shader.Parameters["fadeAlpha"]?.SetValue(fadeAlpha);

            // Set noise texture
            Texture2D noise = BellTextures.PerlinNoise;
            if (noise != null)
                shader.Parameters["noiseTex"]?.SetValue(noise);

            // Draw with shader
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearWrap,
                DepthStencilState.None, RasterizerState.CullNone, shader,
                Main.GameViewMatrix.ZoomMatrix);

            Texture2D circle = BellTextures.SoftCircle;
            if (circle != null)
            {
                Vector2 origin = circle.Size() / 2f;
                Vector2 pos = center - Main.screenPosition;
                sb.Draw(circle, pos, null, Color.White * fadeAlpha, 0f, origin, expandScale, SpriteEffects.None, 0f);
            }

            // End shader batch, return to additive
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null,
                Main.GameViewMatrix.ZoomMatrix);
        }

        /// <summary>
        /// Non-shader fallback — expanding crimson-gold ring layers using sprite stacking.
        /// </summary>
        public static void DrawTollWaveRingFallback(SpriteBatch sb, Vector2 center, float radius, float alpha, bool isFuneralMarch)
        {
            Texture2D ring = BellTextures.DIPowerEffectRing ?? BellTextures.SoftCircle;
            if (ring == null) return;
            Vector2 origin = ring.Size() / 2f;
            Vector2 pos = center - Main.screenPosition;

            float thickness = isFuneralMarch ? 1.5f : 1f;
            float scale = radius / (ring.Width * 0.5f) * thickness;

            // 3-layer ring stack: outer crimson → mid ember → inner gold
            sb.Draw(ring, pos, null, TollCrimson * alpha * 0.45f, 0f, origin, scale * 1.1f, SpriteEffects.None, 0f);
            sb.Draw(ring, pos, null, TollEmber * alpha * 0.55f, 0f, origin, scale, SpriteEffects.None, 0f);
            sb.Draw(ring, pos, null, TollGold * alpha * 0.35f, 0f, origin, scale * 0.9f, SpriteEffects.None, 0f);

            // Harmonic resonance wave overlay (DI-specific)
            Texture2D harmonic = BellTextures.DIHarmonicWaveImpact;
            if (harmonic != null)
            {
                Vector2 hOrigin = harmonic.Size() / 2f;
                float hScale = radius / (harmonic.Width * 0.5f);
                sb.Draw(harmonic, pos, null, TollGold * alpha * 0.25f, 0f, hOrigin, hScale, SpriteEffects.None, 0f);
            }
        }

        // ═══════════════════════════════════════════════════════
        //  STANDING WAVE TETHER — Harmonic sinusoidal connection
        // ═══════════════════════════════════════════════════════

        /// <summary>
        /// Draws a standing wave tether between bell and player.
        /// Uses sinusoidal perpendicular offsets for harmonic wave visualization.
        /// Color oscillates between crimson and gold.
        /// </summary>
        public static void DrawHarmonicTether(SpriteBatch sb, Vector2 bellPos, Vector2 playerPos, float timer)
        {
            Texture2D beam = BellTextures.DIEnergySurgeBeam ?? MagnumTextureRegistry.GetBeamStreak();
            if (beam == null) return;

            Vector2 start = bellPos - Main.screenPosition;
            Vector2 end = playerPos - Main.screenPosition;
            Vector2 diff = end - start;
            float length = diff.Length();
            float rotation = diff.ToRotation();
            Vector2 origin = new Vector2(0, beam.Height / 2f);
            Vector2 perp = new Vector2(-MathF.Sin(rotation), MathF.Cos(rotation));

            if (length < 4f) return;

            int segments = Math.Max(1, (int)(length / 6f));
            float segLength = length / segments;

            // Color oscillation at 2Hz
            float colorOsc = MathF.Sin(timer * 0.12f) * 0.5f + 0.5f;

            for (int i = 0; i < segments; i++)
            {
                float t = (float)i / segments;
                Vector2 segStart = Vector2.Lerp(start, end, t);

                // Standing wave: 4 nodes, amplitude modulated by distance from ends
                float waveEnvelope = MathF.Sin(t * MathF.PI); // Envelope: 0 at ends, 1 at center
                float wave = MathF.Sin(t * MathF.PI * 4f + timer * 0.1f) * 4f * waveEnvelope;
                segStart += perp * wave;

                Color color = Color.Lerp(TollCrimson, TollGold, t * 0.7f + colorOsc * 0.3f);
                float alpha = 0.35f * (1f - t * 0.2f) * waveEnvelope;

                // Main beam
                sb.Draw(beam, segStart, null, color * alpha, rotation, origin,
                    new Vector2(segLength / beam.Width, 0.3f), SpriteEffects.None, 0f);

                // Soft glow sub-layer
                Texture2D glow = BellTextures.SoftGlow;
                if (glow != null && i % 3 == 0)
                {
                    Vector2 glowOrigin = glow.Size() / 2f;
                    sb.Draw(glow, segStart, null, color * alpha * 0.3f, 0f, glowOrigin, 0.02f, SpriteEffects.None, 0f);
                }
            }
        }

        // ═══════════════════════════════════════════════════════
        //  TOLL WAVE DUST & PARTICLES
        // ═══════════════════════════════════════════════════════

        /// <summary>
        /// Toll wave radial ember burst — fire sparks expand outward from center.
        /// Spawns both vanilla dust and custom ember particles.
        /// </summary>
        public static void DoTollWaveDust(Vector2 center, float radius, bool isFuneral = false)
        {
            if (Main.dedServ) return;

            int count = isFuneral ? 24 : 16;
            for (int i = 0; i < count; i++)
            {
                float angle = MathHelper.TwoPi / count * i + Main.rand.NextFloat(-0.1f, 0.1f);
                Vector2 dir = angle.ToRotationVector2();
                Vector2 pos = center + dir * radius;
                float speed = isFuneral ? 3.5f : 2f;
                Vector2 vel = dir * (speed + Main.rand.NextFloat() * 1.5f);

                // Vanilla dust
                Dust d = Dust.NewDustPerfect(pos, DustID.Torch, vel, 0, default, isFuneral ? 1.3f : 0.9f);
                d.noGravity = true;
                d.fadeIn = 0.4f;

                // Custom ember particle
                Color emberColor = Color.Lerp(TollEmber, TollGold, Main.rand.NextFloat());
                BellParticleHandler.Spawn(new TollEmberParticle(
                    pos, vel * 0.8f, emberColor, 0.15f + Main.rand.NextFloat() * 0.1f, 25 + Main.rand.Next(15)));
            }

            // Toll ring bloom particle
            Color ringColor = isFuneral ? TollGold : TollEmber;
            float endScale = isFuneral ? 0.6f : 0.4f;
            BellParticleHandler.Spawn(new TollRingBloomParticle(
                center, ringColor, 0.05f, endScale, isFuneral ? 60 : 45));
        }

        /// <summary>
        /// Funeral March enhanced toll — massive smoke burst with crimson veil.
        /// 15-puff smoke ring + gold fire flash + ash flakes.
        /// </summary>
        public static void DoFuneralMarch(Vector2 center)
        {
            if (Main.dedServ) return;

            // Crimson-black smoke burst (15 puffs)
            for (int i = 0; i < 15; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(4f, 4f);
                Color smokeColor = Color.Lerp(FuneralBlack, TollCrimson, Main.rand.NextFloat() * 0.4f);
                BellParticleHandler.Spawn(new FuneralSmokeParticle(
                    center + Main.rand.NextVector2Circular(16f, 16f),
                    vel, smokeColor, 0.3f + Main.rand.NextFloat() * 0.2f, 50 + Main.rand.Next(25)));
            }

            // Gold fire flash ring
            for (int i = 0; i < 24; i++)
            {
                float angle = MathHelper.TwoPi / 24 * i;
                Vector2 vel = angle.ToRotationVector2() * (3f + Main.rand.NextFloat() * 4f);
                Dust d = Dust.NewDustPerfect(center, DustID.GoldFlame, vel, 0, default, 1.8f);
                d.noGravity = true;

                // Extra ember particles
                Color col = Color.Lerp(TollGold, FlashWhite, Main.rand.NextFloat() * 0.3f);
                BellParticleHandler.Spawn(new TollEmberParticle(
                    center, vel * 0.6f, col, 0.2f + Main.rand.NextFloat() * 0.15f, 30 + Main.rand.Next(20)));
            }

            // Ash flakes drifting upward
            for (int i = 0; i < 10; i++)
            {
                Vector2 vel = new Vector2(Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-3f, -1f));
                BellParticleHandler.Spawn(new AshFlakeParticle(
                    center + Main.rand.NextVector2Circular(30f, 30f),
                    vel, 0.3f + Main.rand.NextFloat() * 0.2f, 60 + Main.rand.Next(40)));
            }

            // Central flash particle
            BellParticleHandler.Spawn(new BellGlowPulseParticle(center, FlashWhite, 0.15f, 20));
        }

        /// <summary>
        /// Death-Mark application flash — dramatic gold-white burst on enemy.
        /// </summary>
        public static void DoDeathMarkFlash(Vector2 center)
        {
            if (Main.dedServ) return;

            BellParticleHandler.Spawn(new DeathMarkFlashParticle(center, 0.3f, 30));

            for (int i = 0; i < 12; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(3f, 3f);
                Color col = Color.Lerp(TollGold, FlashWhite, Main.rand.NextFloat() * 0.5f);
                BellParticleHandler.Spawn(new TollEmberParticle(
                    center, vel, col, 0.12f + Main.rand.NextFloat() * 0.08f, 20 + Main.rand.Next(10)));
            }
        }
    }
}
