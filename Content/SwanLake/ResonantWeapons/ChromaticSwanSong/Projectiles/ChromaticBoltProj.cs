using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Graphics.Shaders;
using ReLogic.Content;
using MagnumOpus.Content.SwanLake.ResonantWeapons.ChromaticSwanSong.Utilities;
using MagnumOpus.Content.SwanLake.ResonantWeapons.ChromaticSwanSong.Primitives;
using MagnumOpus.Content.SwanLake.ResonantWeapons.ChromaticSwanSong.Shaders;
using MagnumOpus.Content.SwanLake.Debuffs;
using MagnumOpus.Content.SwanLake;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.VFX.Core;
using MagnumOpus.Common.Systems.VFX.Sparkle;

namespace MagnumOpus.Content.SwanLake.ResonantWeapons.ChromaticSwanSong.Projectiles
{
    /// <summary>
    /// Chromatic Bolt — main projectile for Chromatic Swan Song.
    /// Gentle spiral wobble, rainbow-shifting trail. Color based on Chromatic Scale position.
    /// On impact: triggers Aria Detonation. Foundation-pattern rendering.
    /// ai[0]: 0=normal, 1=harmonic-charged, 2=Opus Detonation.
    /// ai[1]: scale position 0-6 (C-D-E-F-G-A-B).
    /// </summary>
    public class ChromaticBoltProj : ModProjectile
    {
        private float _hueOffset;

        // GPU primitive trail renderer for shader-driven ChromaticTrail
        private ChromaticPrimitiveRenderer _trailRenderer;

        // Combo phase mapped to shader uPhase (0=normal, 0.5=harmonic, 1.0=opus)
        private float ShaderPhase => Math.Clamp(Projectile.ai[0] / 2f, 0f, 1f);

        public override string Texture => "MagnumOpus/Content/SwanLake/ResonantWeapons/ChromaticSwanSong/ChromaticSwanSong";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 20;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 180;
            Projectile.extraUpdates = 1;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.alpha = 255;
        }

        public override void AI()
        {
            Projectile.alpha = Math.Max(Projectile.alpha - 30, 0);
            _hueOffset += 0.02f;

            int scalePos = (int)Projectile.ai[1];

            // Gentle spiral
            float spiral = (float)Math.Sin(Projectile.timeLeft * 0.2f) * 1.2f;
            Projectile.velocity = Projectile.velocity.RotatedBy(MathHelper.ToRadians(spiral * 0.2f));
            Projectile.rotation = Projectile.velocity.ToRotation();

            // Chromatic trail sparks — vanilla Dust colored by scale note
            if (Main.rand.NextBool(3))
            {
                Color sparkCol = ChromaticSwanPlayer.GetScaleColor(scalePos);
                Dust d = Dust.NewDustPerfect(
                    Projectile.Center + Main.rand.NextVector2Circular(3f, 3f),
                    DustID.RainbowTorch,
                    -Projectile.velocity.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(0.5f, 2f)
                        + Main.rand.NextVector2Circular(1f, 1f),
                    0, sparkCol, Main.rand.NextFloat(0.6f, 1.0f));
                d.noGravity = true;
            }

            // Dying Breath: black feather particles
            Player owner = Main.player[Projectile.owner];
            try
            {
                if (owner.active && owner.ChromaticSwan().DyingBreathActive && Main.rand.NextBool(4))
                {
                    Dust d = Dust.NewDustPerfect(
                        Projectile.Center + Main.rand.NextVector2Circular(6f, 6f),
                        DustID.Smoke,
                        -Projectile.velocity.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(0.3f, 1.5f)
                            + Main.rand.NextVector2Circular(0.8f, 0.8f),
                        180, Color.Black, 0.7f);
                    d.noGravity = true;
                }
            }
            catch { }

            // Light — colored by scale note
            Color lightCol = ChromaticSwanPlayer.GetScaleColor(scalePos);
            Lighting.AddLight(Projectile.Center, lightCol.ToVector3() * 0.4f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<SwansMark>(), 300);

            Player owner = Main.player[Projectile.owner];
            ChromaticSwanPlayer csp = null;
            try { csp = owner.ChromaticSwan(); } catch { }
            csp?.RegisterHit(target.whoAmI);

            int scalePos = (int)Projectile.ai[1];
            bool isOpus = Projectile.ai[0] >= 2f;
            bool isHarmonic = Projectile.ai[0] >= 1f;
            bool dyingBreath = csp?.DyingBreathActive ?? false;

            // Aria Detonation on EVERY hit
            float ariaMode = isOpus ? 2f : (isHarmonic ? 1f : 0f);
            float ariaDmgMult = isOpus ? 3f : (isHarmonic ? 2f : 0.5f);
            int ariaDmg = (int)(Projectile.damage * ariaDmgMult);
            float ariaAi1 = scalePos + (dyingBreath ? 100f : 0f);

            Projectile.NewProjectile(Projectile.GetSource_OnHit(target, "AriaDetonation"),
                target.Center, Vector2.Zero, ModContent.ProjectileType<AriaDetonationProj>(),
                ariaDmg, 10f, Projectile.owner, ai0: ariaMode, ai1: ariaAi1);

            if (isHarmonic && !isOpus && csp != null && csp.HarmonicStack >= 5)
                csp.ConsumeHarmonicStack();

            if (isOpus)
                SoundEngine.PlaySound(SoundID.Item29 with { Pitch = 0.2f, Volume = 1.0f }, target.Center);
            else
                SoundEngine.PlaySound(SoundID.Item29 with { Pitch = 0.6f, Volume = 0.7f }, target.Center);

            // Keep dust sparks light — AriaDetonation handles the main impact VFX
            Color noteColor = ChromaticSwanPlayer.GetScaleColor(scalePos);
            for (int i = 0; i < 4; i++)
            {
                Dust d = Dust.NewDustPerfect(
                    target.Center + Main.rand.NextVector2Circular(8f, 8f),
                    DustID.RainbowTorch,
                    Main.rand.NextVector2Circular(3f, 3f),
                    0, noteColor, Main.rand.NextFloat(0.4f, 0.8f));
                d.noGravity = true;
            }

            // Minimal notes — AriaDetonation handles heavier VFX
            try { SwanLakeVFXLibrary.SpawnMusicNotes(target.Center, 1, 10f, 0.4f, 0.7f, 20); } catch { }
        }

        public override void OnKill(int timeLeft)
        {
            // Dispose GPU trail renderer
            _trailRenderer?.Dispose();
            _trailRenderer = null;

            for (int i = 0; i < 8; i++)
            {
                Color col = ChromaticSwanUtils.GetChromatic(Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.RainbowTorch,
                    Main.rand.NextVector2Circular(4f, 4f), 0, col, Main.rand.NextFloat(0.4f, 0.8f));
                d.noGravity = true;
            }

            try { SwanLakeVFXLibrary.SpawnMusicNotes(Projectile.Center, 3, 18f, 0.6f, 0.9f, 26); } catch { }
            try { SwanLakeVFXLibrary.SpawnMixedSparkleImpact(Projectile.Center, 0.6f, 4, 4); } catch { }
            try { SwanLakeVFXLibrary.SpawnPrismaticSparkles(Projectile.Center, 4, 18f); } catch { }
        }

        #region Rendering (3-Pass Shader Trail + 5-Layer Bloom — Chromatic Spectrum Pipeline)

        /// <summary>
        /// OVERHAULED RENDERING PIPELINE:
        /// Pass 1: ChromaticTrailGlow @ 3x width (soft prismatic bloom underlay)
        /// Pass 2: ChromaticTrailMain @ 1x width (sharp spectral-banded core with musical shimmer)
        /// Pass 3: ChromaticTrailGlow @ 1.5x width (overbright halo)
        /// Then: 5-layer chromatic bloom core (spectral halo → note glow → silver → white → star)
        /// Theme accents last.
        /// </summary>
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            try
            {
            float alphaFade = (255 - Projectile.alpha) / 255f;
            if (alphaFade <= 0.01f) return false;

            try
            {
                // ===== GPU SHADER TRAIL (3 passes per ChromaticTrail.fx) =====
                DrawShaderTrail(sb);

                // ===== 5-LAYER CHROMATIC BLOOM CORE =====
                DrawChromaticBloomStack(sb);

                // ===== THEME ACCENTS =====
                sb.End();
                sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, SamplerState.LinearClamp,
                    DepthStencilState.None, Main.Rasterizer, null,
                    Main.GameViewMatrix.TransformationMatrix);
                ChromaticSwanUtils.DrawThemeAccents(sb, Projectile.Center, 1f, 0.6f);
                sb.End();
                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                    DepthStencilState.None, Main.Rasterizer, null,
                    Main.GameViewMatrix.TransformationMatrix);
            }
            catch
            {
                try { sb.End(); } catch { }
                try
                {
                    sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                        DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
                }
                catch { }
            }

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

        /// <summary>
        /// 3-pass GPU shader trail using ChromaticTrail.fx techniques.
        /// Technique switching: ChromaticTrailGlow (wide) → ChromaticTrailMain (core) → ChromaticTrailGlow (overbright).
        /// uPhase drives spectral band escalation — desaturated silver at phase 0, full ROYGBIV at phase 1.
        /// </summary>
        private void DrawShaderTrail(SpriteBatch sb)
        {
            _trailRenderer ??= new ChromaticPrimitiveRenderer();

            // End SpriteBatch — primitive renderer uses raw GPU calls
            sb.End();

            float time = Main.GlobalTimeWrappedHourly;
            float phase = ShaderPhase;
            float alphaFade = (255 - Projectile.alpha) / 255f;
            int scalePos = (int)Projectile.ai[1];
            Color noteColor = ChromaticSwanPlayer.GetScaleColor(scalePos);

            try
            {
                MiscShaderData chromaticShader = null;
                if (ChromaticShaderLoader.HasChromaticTrailShader)
                    chromaticShader = GameShaders.Misc["MagnumOpus:ChromaticTrail"];

                Effect effect = chromaticShader?.Shader;

                if (effect != null)
                {
                    // Configure shared uniforms
                    effect.Parameters["uColor"]?.SetValue(new Vector3(0.75f, 0.75f, 0.82f)); // Silver base
                    effect.Parameters["uSecondaryColor"]?.SetValue(new Vector3(1f, 1f, 1f));  // Pure white core
                    effect.Parameters["uTime"]?.SetValue(time * 2.5f);
                    effect.Parameters["uScrollSpeed"]?.SetValue(1.8f);
                    effect.Parameters["uPhase"]?.SetValue(phase);
                    effect.Parameters["uOverbrightMult"]?.SetValue(1.2f + phase * 0.3f);

                    // Noise distortion for shimmer
                    effect.Parameters["uNoiseScale"]?.SetValue(1.5f);
                    effect.Parameters["uDistortionAmt"]?.SetValue(0.04f + phase * 0.06f);
                    effect.Parameters["uSecondaryTexScale"]?.SetValue(2.0f);
                    effect.Parameters["uSecondaryTexScroll"]?.SetValue(0.8f);

                    // Bind textures
                    if (MagnumTextureRegistry.SoftGlow != null)
                        chromaticShader.UseImage1(MagnumTextureRegistry.SoftGlow);
                    if (MagnumTextureRegistry.PerlinNoise != null)
                    {
                        chromaticShader.UseImage2(MagnumTextureRegistry.PerlinNoise);
                        effect.Parameters["uHasSecondaryTex"]?.SetValue(1f);
                    }

                    // === PASS 1: ChromaticTrailGlow @ 3x width (soft prismatic bloom) ===
                    effect.CurrentTechnique = effect.Techniques["ChromaticTrailGlow"];
                    effect.Parameters["uOpacity"]?.SetValue(0.5f * alphaFade);
                    effect.Parameters["uIntensity"]?.SetValue(0.6f + phase * 0.3f);

                    var glowSettings = new ChromaticTrailSettings(
                        t =>
                        {
                            float baseW = MathHelper.Lerp(36f, 6f, t);
                            return baseW * (1f + phase * 0.5f);
                        },
                        t =>
                        {
                            float fade = (1f - t) * 0.35f * alphaFade;
                            Color col = Color.Lerp(new Color(noteColor.R, noteColor.G, noteColor.B), new Color(200, 200, 220), 0.5f);
                            return col * fade;
                        },
                        shader: chromaticShader
                    );
                    _trailRenderer.RenderTrail(Projectile.oldPos, glowSettings, 20);

                    // === PASS 2: ChromaticTrailMain @ 1x width (sharp spectral bands) ===
                    effect.CurrentTechnique = effect.Techniques["ChromaticTrailMain"];
                    effect.Parameters["uOpacity"]?.SetValue(0.85f * alphaFade);
                    effect.Parameters["uIntensity"]?.SetValue(0.9f + phase * 0.4f);

                    var coreSettings = new ChromaticTrailSettings(
                        t =>
                        {
                            float baseW = MathHelper.Lerp(12f, 2f, t);
                            return baseW * (1f + phase * 0.3f);
                        },
                        t =>
                        {
                            float fade = (1f - t) * 0.8f * alphaFade;
                            Color col = Color.Lerp(new Color(200, 200, 220), Color.White, t * 0.3f);
                            return col * fade;
                        },
                        shader: chromaticShader
                    );
                    _trailRenderer.RenderTrail(Projectile.oldPos, coreSettings, 20);

                    // === PASS 3: ChromaticTrailGlow @ 1.5x width (overbright halo) ===
                    effect.CurrentTechnique = effect.Techniques["ChromaticTrailGlow"];
                    effect.Parameters["uOpacity"]?.SetValue(0.25f * alphaFade);
                    effect.Parameters["uIntensity"]?.SetValue(0.8f + phase * 0.4f);
                    effect.Parameters["uOverbrightMult"]?.SetValue(1.8f);

                    var overbrightSettings = new ChromaticTrailSettings(
                        t =>
                        {
                            float baseW = MathHelper.Lerp(20f, 3f, t);
                            return baseW * (1f + phase * 0.4f);
                        },
                        t =>
                        {
                            float fade = (1f - t) * 0.2f * alphaFade;
                            return new Color(240, 240, 255) * fade;
                        },
                        shader: chromaticShader
                    );
                    _trailRenderer.RenderTrail(Projectile.oldPos, overbrightSettings, 20);
                }
                else
                {
                    // Shader unavailable — no fallback trail
                }
            }
            catch
            {
                // Shader error — no fallback trail
            }

            // Restart SpriteBatch
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                DepthStencilState.None, Main.Rasterizer, null,
                Main.GameViewMatrix.TransformationMatrix);
        }



        /// <summary>
        /// Sparkle-enhanced chromatic bloom core at projectile center.
        /// Replaces 5-layer SoftGlow stack with twinkling prismatic sparkle field.
        /// Note color from scale position, musical pulsing, combo intensity escalation.
        /// </summary>
        private void DrawChromaticBloomStack(SpriteBatch sb)
        {
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float alphaFade = (255 - Projectile.alpha) / 255f;
            float tempoPulse = 0.9f + 0.1f * (float)Math.Sin(Main.GameUpdateCount * (0.12f + ShaderPhase * 0.08f));
            float empScale = 1f + ShaderPhase * 0.4f;
            float time = (float)Main.timeForVisualEffects;

            int scalePos = (int)Projectile.ai[1];
            Color noteColor = ChromaticSwanPlayer.GetScaleColor(scalePos);
            Color shifted = ChromaticSwanUtils.GetChromatic(_hueOffset + 0.33f);
            Color spectrum = ChromaticSwanUtils.GetSpectrumColor(_hueOffset);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null,
                Main.GameViewMatrix.TransformationMatrix);

            // Main sparkle bloom field — prismatic, note-colored, scales with combo
            Color[] sparkleColors = new Color[] {
                noteColor,
                shifted,
                new Color(220, 220, 240),   // Silver core
                spectrum,
                Color.White,
            };
            float sparkleRadius = 20f * empScale;
            int sparkleCount = 6 + (int)(ShaderPhase * 3f);
            SparkleBloomHelper.DrawSparkleBloom(sb, Projectile.Center, SparkleTheme.SwanLake,
                sparkleColors, alphaFade * tempoPulse, sparkleRadius, sparkleCount, time,
                seed: Projectile.identity * 0.53f + _hueOffset, sparkleScale: 0.03f * empScale);

            // Opus Detonation charge: 7 chromatic sparkle positions orbiting center
            if (Projectile.ai[0] >= 2f)
            {
                for (int n = 0; n < 7; n++)
                {
                    Color c = ChromaticSwanPlayer.GetScaleColor(n);
                    float angleOff = MathHelper.TwoPi * n / 7f + Main.GameUpdateCount * 0.05f;
                    Vector2 offset = new Vector2((float)Math.Cos(angleOff), (float)Math.Sin(angleOff)) * 6f;
                    Color[] ringColors = new Color[] { c, Color.Lerp(c, Color.White, 0.5f), Color.White };
                    SparkleBloomHelper.DrawSparkleBloom(sb, Projectile.Center + offset, SparkleTheme.SwanLake,
                        ringColors, 0.4f * alphaFade, 8f, 3, time,
                        seed: n * 1.23f + Projectile.identity * 0.17f, sparkleScale: 0.02f);
                }
            }

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                DepthStencilState.None, Main.Rasterizer, null,
                Main.GameViewMatrix.TransformationMatrix);
        }

        #endregion
    }
}
