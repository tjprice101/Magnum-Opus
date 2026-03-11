using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Content.DiesIrae;
using MagnumOpus.Content.DiesIrae.Weapons.WrathfulContract.Particles;
using MagnumOpus.Content.FoundationWeapons.LaserFoundation;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.Graphics;
using Terraria.ModLoader;

namespace MagnumOpus.Content.DiesIrae.Weapons.WrathfulContract.Utilities
{
    /// <summary>
    /// Static VFX utility for Wrathful Contract.
    /// Demon body rendering (RadialNoiseMaskShader with FBMNoise + VoronoiCell dual-noise),
    /// blood tether beam, blood sacrifice cinematic VFX, frenzy kill burst,
    /// and demon eye rendering.
    /// </summary>
    public static class WrathfulContractUtils
    {
        // ═══════════════════════════════════════════════════════
        //  PALETTE SHORTCUTS
        // ═══════════════════════════════════════════════════════
        public static Color DemonBody => new Color(20, 5, 5);         // Near-black
        public static Color DemonCore => DiesIraePalette.EmberOrange; // Molten cracks
        public static Color DemonFrenzy => new Color(255, 100, 20);   // Brighter ember
        public static Color BreachRed => DiesIraePalette.InfernalRed;
        public static Color BloodCrimson => DiesIraePalette.BloodRed;
        public static Color EyeEmber => new Color(255, 180, 50);      // Ember-gold eyes
        public static Color EyeHostile => new Color(200, 20, 20);     // Red hostile eyes
        public static Color SacrificeGold => DiesIraePalette.JudgmentGold;

        // ═══════════════════════════════════════════════════════
        //  SHADER CACHE
        // ═══════════════════════════════════════════════════════
        private static Effect _maskShader;
        private static Effect _rippleShader;

        // ConvergenceBeamShader pipeline (LaserFoundation pattern)
        private static Effect _convergenceBeamShader;
        private static Effect _flareRainbowShader;
        private static Asset<Texture2D> _diesIraeGradientLUT;

        private static Asset<Texture2D> _diGradient;
        private static Asset<Texture2D> _softCircle;

        public static Effect GetMaskShader()
        {
            if (_maskShader == null)
            {
                try
                {
                    _maskShader = ModContent.Request<Effect>(
                        "MagnumOpus/Content/FoundationWeapons/MaskFoundation/Shaders/RadialNoiseMaskShader",
                        AssetRequestMode.ImmediateLoad).Value;
                }
                catch { }
            }
            return _maskShader;
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

        public static Texture2D GetDIGradient()
        {
            _diGradient ??= ModContent.Request<Texture2D>(
                "MagnumOpus/Assets/VFX Asset Library/ColorGradients/DiesIraeGradientLUTandRAMP",
                AssetRequestMode.AsyncLoad);
            return _diGradient?.IsLoaded == true ? _diGradient.Value : null;
        }

        public static Texture2D GetSoftCircle()
        {
            _softCircle ??= ModContent.Request<Texture2D>(
                "MagnumOpus/Assets/VFX Asset Library/MasksAndShapes/SoftCircle",
                AssetRequestMode.AsyncLoad);
            return _softCircle?.IsLoaded == true ? _softCircle.Value : null;
        }

        // ═══════════════════════════════════════════════════════
        //  EASING HELPERS
        // ═══════════════════════════════════════════════════════
        public static float EaseOutCubic(float t) => 1f - MathF.Pow(1f - t, 3f);
        public static float SinePulse(float timer, float freq = 0.12f) => 0.85f + 0.15f * MathF.Sin(timer * freq);

        // ═══════════════════════════════════════════════════════
        //  DEMON STATE
        // ═══════════════════════════════════════════════════════
        public enum DemonState { Normal, Frenzy, Breach, Sacrifice }

        // ═══════════════════════════════════════════════════════
        //  DEMON BODY RENDERING — RadialNoiseMaskShader
        // ═══════════════════════════════════════════════════════

        /// <summary>
        /// Draws the demon body using RadialNoiseMaskShader with FBMNoise + VoronoiCell double-noise.
        /// State-driven parameters: Normal → Frenzy → Breach.
        /// </summary>
        public static void DrawDemonBody(SpriteBatch sb, Vector2 worldPos, float timer, DemonState state, float seed = 0f)
        {
            Vector2 drawPos = worldPos - Main.screenPosition;
            Effect shader = GetMaskShader();
            Texture2D fbm = DemonTextures.FBMNoise;
            Texture2D voronoi = DemonTextures.VoronoiNoise;
            Texture2D gradient = GetDIGradient();
            Texture2D softCircle = GetSoftCircle();
            Texture2D softGlow = DemonTextures.SoftGlow;
            Texture2D glowOrb = DemonTextures.GlowOrb;

            // State-driven parameters
            float circleRadius, scrollSpeed, intensity;
            Color primary, core;
            GetDemonParams(state, out circleRadius, out scrollSpeed, out intensity, out primary, out core);

            float pulse = SinePulse(timer, 0.1f);

            // Layer 1: Outer smoke/haze
            if (softGlow != null)
            {
                float hazeAlpha = state == DemonState.Frenzy ? 0.18f : 0.1f;
                sb.Draw(softGlow, drawPos, null, primary * hazeAlpha * pulse, 0f, softGlow.Size() / 2f, 0.45f, SpriteEffects.None, 0f);

                // Frenzy aura (capped 300px max)
                if (state == DemonState.Frenzy)
                {
                    sb.Draw(softGlow, drawPos, null, DemonCore * 0.08f, 0f, softGlow.Size() / 2f, MathHelper.Min(circleRadius / 0.32f * 0.4f, 0.58f), SpriteEffects.None, 0f);
                }
            }

            // Layer 2: Shader body — FBMNoise primary
            if (shader != null && fbm != null && softCircle != null)
            {
                sb.End();
                sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, SamplerState.LinearWrap,
                    DepthStencilState.None, RasterizerState.CullNone, shader, Main.GameViewMatrix.ZoomMatrix);

                float time = (float)Main.timeForVisualEffects * 0.015f + seed;

                shader.Parameters["uTime"]?.SetValue(time);
                shader.Parameters["scrollSpeed"]?.SetValue(scrollSpeed);
                shader.Parameters["rotationSpeed"]?.SetValue(0.12f);
                shader.Parameters["circleRadius"]?.SetValue(circleRadius);
                shader.Parameters["edgeSoftness"]?.SetValue(0.06f);
                shader.Parameters["intensity"]?.SetValue(intensity);
                shader.Parameters["primaryColor"]?.SetValue(primary.ToVector3());
                shader.Parameters["coreColor"]?.SetValue(core.ToVector3());
                shader.Parameters["noiseTex"]?.SetValue(fbm);
                if (gradient != null)
                    shader.Parameters["gradientTex"]?.SetValue(gradient);

                Vector2 circOrigin = softCircle.Size() / 2f;
                float drawScale = circleRadius / 0.40f * 0.65f;
                sb.Draw(softCircle, drawPos, null, Color.White, 0f, circOrigin, drawScale, SpriteEffects.None, 0f);

                sb.End();

                // Secondary VoronoiCell layer at 30% intensity (fine surface fractures)
                if (voronoi != null)
                {
                    sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, SamplerState.LinearWrap,
                        DepthStencilState.None, RasterizerState.CullNone, shader, Main.GameViewMatrix.ZoomMatrix);

                    shader.Parameters["intensity"]?.SetValue(intensity * 0.3f);
                    shader.Parameters["rotationSpeed"]?.SetValue(-0.08f);
                    shader.Parameters["scrollSpeed"]?.SetValue(scrollSpeed * 0.7f);
                    shader.Parameters["noiseTex"]?.SetValue(voronoi);

                    sb.Draw(softCircle, drawPos, null, Color.White, 0f, circOrigin, drawScale * 0.95f, SpriteEffects.None, 0f);

                    sb.End();
                }

                sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, SamplerState.LinearClamp,
                    DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);
            }

            // Layer 3: Demon eyes (2x StarFlare)
            DrawDemonEyes(sb, drawPos, state, timer);

            // Layer 4: Core bloom
            if (glowOrb != null)
            {
                Vector2 origin = glowOrb.Size() / 2f;
                sb.Draw(glowOrb, drawPos, null, core * 0.3f * pulse, 0f, origin, 0.12f, SpriteEffects.None, 0f);
            }
        }

        private static void GetDemonParams(DemonState state, out float circleRadius, out float scrollSpeed,
            out float intensity, out Color primary, out Color core)
        {
            switch (state)
            {
                case DemonState.Frenzy:
                    circleRadius = 0.48f; scrollSpeed = 0.35f; intensity = 2.5f;
                    primary = DemonBody; core = DemonFrenzy;
                    break;
                case DemonState.Breach:
                    circleRadius = 0.44f; scrollSpeed = 0.30f; intensity = 3.0f;
                    primary = new Color(40, 5, 5); core = BreachRed;
                    break;
                case DemonState.Sacrifice:
                    circleRadius = 0.46f; scrollSpeed = 0.25f; intensity = 3.5f;
                    primary = DemonBody; core = SacrificeGold;
                    break;
                default: // Normal
                    circleRadius = 0.40f; scrollSpeed = 0.20f; intensity = 1.5f;
                    primary = DemonBody; core = DemonCore;
                    break;
            }
        }

        // ═══════════════════════════════════════════════════════
        //  DEMON EYES — 2x StarFlare offset from center
        // ═══════════════════════════════════════════════════════
        private static void DrawDemonEyes(SpriteBatch sb, Vector2 screenPos, DemonState state, float timer)
        {
            Texture2D eyeTex = DemonTextures.DIStarFlare ?? DemonTextures.StarFlare;
            if (eyeTex == null) return;

            Color eyeColor = state == DemonState.Breach ? EyeHostile : EyeEmber;
            Vector2 origin = eyeTex.Size() / 2f;
            float eyeScale = 0.02f;
            float pulse = 0.9f + 0.1f * MathF.Sin(timer * 0.15f);
            float eyeAlpha = state == DemonState.Breach ? 0.9f : 0.7f;

            // Left eye
            Vector2 leftEye = screenPos + new Vector2(-6f, -4f);
            sb.Draw(eyeTex, leftEye, null, eyeColor * eyeAlpha * pulse, 0f, origin, eyeScale, SpriteEffects.None, 0f);

            // Right eye
            Vector2 rightEye = screenPos + new Vector2(6f, -4f);
            sb.Draw(eyeTex, rightEye, null, eyeColor * eyeAlpha * pulse, 0f, origin, eyeScale, SpriteEffects.None, 0f);

            // Inner white core for each eye
            sb.Draw(eyeTex, leftEye, null, Color.White * eyeAlpha * 0.3f, 0f, origin, eyeScale * 0.4f, SpriteEffects.None, 0f);
            sb.Draw(eyeTex, rightEye, null, Color.White * eyeAlpha * 0.3f, 0f, origin, eyeScale * 0.4f, SpriteEffects.None, 0f);
        }

        // ═══════════════════════════════════════════════════════
        //  BLOOD TETHER — ThinLaserFoundation-style beam
        // ═══════════════════════════════════════════════════════

        /// <summary>
        /// Draws the blood contract tether between player and demon.
        /// VertexStrip + ConvergenceBeamShader (LaserFoundation pipeline) with DiesIrae gradient LUT.
        /// </summary>
        public static void DrawBloodTether(SpriteBatch sb, Vector2 playerPos, Vector2 demonPos,
            DemonState state, bool sacrificeReversed = false)
        {
            Vector2 dir = demonPos - playerPos;
            float dist = dir.Length();
            if (dist < 1f) return;
            float rot = dir.ToRotation();

            // State-driven width and intensity
            float width = 50f;
            float intensity = 0.6f;
            if (state == DemonState.Breach)
            {
                float t = (float)Main.timeForVisualEffects * 0.08f;
                width = 50f + 20f * MathF.Abs(MathF.Sin(t * MathF.PI));
                intensity = 0.9f;
            }
            else if (sacrificeReversed)
            {
                intensity = 0.8f;
            }

            // ── BUILD VERTEX STRIP ──
            Vector2[] positions = { playerPos, demonPos };
            float[] rotations = { rot, rot };

            VertexStrip strip = new VertexStrip();
            strip.PrepareStrip(positions, rotations,
                (float p) => Color.White,
                (float p) => width,
                -Main.screenPosition, includeBacksides: true);

            // ── LOAD & CONFIGURE CONVERGENCE BEAM SHADER ──
            if (_convergenceBeamShader == null)
            {
                _convergenceBeamShader = ModContent.Request<Effect>(
                    "MagnumOpus/Content/FoundationWeapons/LaserFoundation/Shaders/ConvergenceBeamShader",
                    AssetRequestMode.ImmediateLoad).Value;
            }
            if (_diesIraeGradientLUT == null)
            {
                _diesIraeGradientLUT = ModContent.Request<Texture2D>(
                    "MagnumOpus/Assets/VFX Asset Library/ColorGradients/DiesIraeGradientLUTandRAMP",
                    AssetRequestMode.ImmediateLoad);
            }

            Effect shader = _convergenceBeamShader;
            shader.Parameters["WorldViewProjection"].SetValue(
                Main.GameViewMatrix.NormalizedTransformationmatrix);
            shader.Parameters["onTex"].SetValue(LFTextures.BeamAlphaMask.Value);
            shader.Parameters["gradientTex"].SetValue(_diesIraeGradientLUT.Value);
            shader.Parameters["baseColor"].SetValue(Color.White.ToVector3());
            shader.Parameters["satPower"].SetValue(0.8f);

            shader.Parameters["sampleTexture1"].SetValue(LFTextures.DetailThinGlowLine.Value);
            shader.Parameters["sampleTexture2"].SetValue(LFTextures.DetailSpark.Value);
            shader.Parameters["sampleTexture3"].SetValue(LFTextures.DetailExtra.Value);
            shader.Parameters["sampleTexture4"].SetValue(LFTextures.DetailTrailLoop.Value);

            shader.Parameters["grad1Speed"].SetValue(0.66f);
            shader.Parameters["grad2Speed"].SetValue(0.66f);
            shader.Parameters["grad3Speed"].SetValue(1.03f);
            shader.Parameters["grad4Speed"].SetValue(0.77f);

            shader.Parameters["tex1Mult"].SetValue(1.25f);
            shader.Parameters["tex2Mult"].SetValue(1.5f);
            shader.Parameters["tex3Mult"].SetValue(1.15f);
            shader.Parameters["tex4Mult"].SetValue(2.5f);
            shader.Parameters["totalMult"].SetValue(intensity);

            float repVal = dist / 2000f;
            shader.Parameters["gradientReps"].SetValue(0.75f * repVal);
            shader.Parameters["tex1reps"].SetValue(1.15f * repVal);
            shader.Parameters["tex2reps"].SetValue(1.15f * repVal);
            shader.Parameters["tex3reps"].SetValue(1.15f * repVal);
            shader.Parameters["tex4reps"].SetValue(1.15f * repVal);

            shader.Parameters["uTime"].SetValue((float)Main.timeForVisualEffects * -0.024f);

            // ── END CALLER'S SPRITEBATCH, DRAW BEAM BODY ──
            sb.End();
            shader.CurrentTechnique.Passes["MainPS"].Apply();
            strip.DrawTrail();
            Main.pixelShader.CurrentTechnique.Passes[0].Apply();

            // ── ENDPOINT FLARES (FlareRainbowShader) ──
            if (_flareRainbowShader == null)
            {
                _flareRainbowShader = ModContent.Request<Effect>(
                    "MagnumOpus/Content/FoundationWeapons/LaserFoundation/Shaders/FlareRainbowShader",
                    AssetRequestMode.ImmediateLoad).Value;
            }

            float flareRot = (float)Main.timeForVisualEffects * 0.115f;
            _flareRainbowShader.Parameters["rotation"].SetValue(flareRot * 0.075f);
            _flareRainbowShader.Parameters["rainbowRotation"].SetValue(flareRot * 0.025f);
            _flareRainbowShader.Parameters["intensity"].SetValue(intensity);
            _flareRainbowShader.Parameters["fadeStrength"].SetValue(1f);

            Vector2 drawStart = playerPos - Main.screenPosition;
            Vector2 drawEnd = demonPos - Main.screenPosition;

            Texture2D lensFlare = LFTextures.LensFlare.Value;
            Texture2D starFlare = LFTextures.StarFlare.Value;
            Texture2D glowOrb = LFTextures.GlowOrb.Value;
            Texture2D softGlow = LFTextures.SoftGlow.Value;

            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, _flareRainbowShader,
                Main.GameViewMatrix.EffectMatrix);

            // Source flares (player end)
            Vector2 sigilScale = new Vector2(0.2f, 1f) * 0.4f * intensity;
            sb.Draw(softGlow, drawStart, null, Color.White, rot,
                softGlow.Size() / 2f, sigilScale, SpriteEffects.None, 0f);
            sb.Draw(starFlare, drawStart, null, Color.White, rot,
                starFlare.Size() / 2f, sigilScale, SpriteEffects.None, 0f);

            // Demon end flares
            sb.Draw(glowOrb, drawEnd, null, Color.White, flareRot * 0.1f,
                glowOrb.Size() / 2f, 0.4f * intensity, SpriteEffects.None, 0f);
            sb.Draw(lensFlare, drawEnd, null, Color.White, flareRot * 0.02f,
                lensFlare.Size() / 2f, 0.3f * intensity, SpriteEffects.None, 0f);
            sb.Draw(starFlare, drawEnd, null, Color.White, flareRot * 0.05f,
                starFlare.Size() / 2f, 0.35f * intensity, SpriteEffects.None, 0f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                DepthStencilState.None, RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.TransformationMatrix);

            // Blood drain orb particles (6 evenly spaced along tether)
            if (Main.rand.NextBool(8))
            {
                float t = Main.rand.NextFloat();
                Vector2 orbStart = playerPos;
                Vector2 orbEnd = demonPos;
                bool reversed = sacrificeReversed;
                DemonParticleHandler.Spawn(new BloodDrainOrbParticle(orbStart, orbEnd, reversed, 30));
            }
        }

        // ═══════════════════════════════════════════════════════
        //  FRENZY KILL BURST — ExplosionParticlesFoundation 30 sparks
        // ═══════════════════════════════════════════════════════

        /// <summary>
        /// Spawns a radial scatter of 30 sparks at kill position during Frenzy state.
        /// </summary>
        public static void DoFrenzyKillBurst(Vector2 worldPos)
        {
            // === Color-ramped sparkle explosion VFX ===
            DiesIraeVFXLibrary.SpawnColorRampedSparkleExplosion(worldPos, 10, 6f, 0.35f);

            Color[] palette = { DiesIraePalette.EmberOrange, DiesIraePalette.InfernalRed, DiesIraePalette.JudgmentGold };

            for (int i = 0; i < 30; i++)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                float speed = Main.rand.NextFloat(2f, 6f);
                Vector2 vel = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * speed;
                Color sparkColor = palette[Main.rand.Next(palette.Length)];
                float scale = Main.rand.NextFloat(0.02f, 0.04f);

                DemonParticleHandler.Spawn(new FrenzyKillSparkParticle(worldPos, vel, sparkColor, scale, 25));
            }

            // Smoke burst: 10 puffs
            for (int i = 0; i < 10; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(1.5f, 1.5f);
                DemonParticleHandler.Spawn(new DemonSmokePuffParticle(
                    worldPos + vel * 5f, vel * 0.5f, Main.rand.NextFloat(0.06f, 0.1f), 40));
            }
        }

        // ═══════════════════════════════════════════════════════
        //  AMBIENT DEMON SMOKE
        // ═══════════════════════════════════════════════════════

        /// <summary>
        /// Spawns ambient smoke puffs around the demon.
        /// Rate: 5 puffs/s normal, 15 puffs/s Frenzy.
        /// </summary>
        public static void DoAmbientSmoke(Vector2 worldPos, DemonState state)
        {
            int chanceInv = state == DemonState.Frenzy ? 4 : 12;

            if (Main.rand.NextBool(chanceInv))
            {
                Vector2 offset = Main.rand.NextVector2Circular(15f, 15f);
                Vector2 vel = new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), Main.rand.NextFloat(-0.8f, -0.2f));
                float scale = Main.rand.NextFloat(0.04f, 0.08f);
                DemonParticleHandler.Spawn(new DemonSmokePuffParticle(worldPos + offset, vel, scale, 45));
            }

            // Ember sparks
            if (Main.rand.NextBool(chanceInv + 2))
            {
                Vector2 offset = Main.rand.NextVector2Circular(12f, 12f);
                Vector2 vel = new Vector2(Main.rand.NextFloat(-0.7f, 0.7f), Main.rand.NextFloat(-1.2f, -0.3f));
                Color emberColor = Color.Lerp(DiesIraePalette.EmberOrange, DiesIraePalette.InfernalRed, Main.rand.NextFloat());
                DemonParticleHandler.Spawn(new DemonEmberParticle(worldPos + offset, vel, emberColor, 0.015f, 25));
            }
        }

        // ═══════════════════════════════════════════════════════
        //  BLOOD SACRIFICE VFX
        // ═══════════════════════════════════════════════════════

        /// <summary>
        /// Spawns the blood sacrifice visual burst at the demon's position.
        /// </summary>
        public static void DoBloodSacrificeFlash(Vector2 worldPos)
        {
            // Gold flash ring
            DemonParticleHandler.Spawn(new BloodSacrificeFlashParticle(worldPos, 0.4f, 40));

            // Burst sparks in gold
            for (int i = 0; i < 15; i++)
            {
                Vector2 vel = Main.rand.NextVector2CircularEdge(3f, 3f);
                DemonParticleHandler.Spawn(new FrenzyKillSparkParticle(
                    worldPos, vel, DiesIraePalette.JudgmentGold, 0.025f, 20));
            }
        }

        // ═══════════════════════════════════════════════════════
        //  BREACH WARNING VFX
        // ═══════════════════════════════════════════════════════

        /// <summary>
        /// Spawns a rapid pulsing crimson warning ring.
        /// </summary>
        public static void DoBreachWarningPulse(Vector2 worldPos)
        {
            DemonParticleHandler.Spawn(new BreachWarningPulseParticle(worldPos, 0.25f, 30));
        }

        // ─────────── THEME TEXTURE ACCENTS ───────────

        /// <summary>
        /// Draws theme-textured demonic accents. Call under Additive blend.
        /// </summary>
        public static void DrawThemeAccents(SpriteBatch sb, Vector2 worldPos, float scale, float intensity = 1f)
        {
            DiesIraeVFXLibrary.DrawThemeStarFlare(sb, worldPos, scale, intensity * 0.5f);
            float rot = (float)Main.GameUpdateCount * 0.02f;
            DiesIraeVFXLibrary.DrawThemeImpactRing(sb, worldPos, scale * 0.8f, intensity * 0.4f, rot);
        }
    }
}
