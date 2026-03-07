using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Graphics.Shaders;
using ReLogic.Content;
using MagnumOpus.Content.SwanLake.ResonantWeapons.CalloftheBlackSwan.Utilities;
using MagnumOpus.Content.SwanLake.ResonantWeapons.CalloftheBlackSwan.Primitives;
using MagnumOpus.Content.SwanLake.ResonantWeapons.CalloftheBlackSwan.Shaders;
using MagnumOpus.Content.SwanLake.Debuffs;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.VFX.Core;

namespace MagnumOpus.Content.SwanLake.ResonantWeapons.CalloftheBlackSwan.Projectiles
{
    /// <summary>
    /// Black Swan Flare — Homing sub-projectile fired during swing phases.
    /// Dual-polarity: randomly black or white on spawn. Tracks enemies.
    /// ai[0] = 0: normal, 1: empowered (rainbow aura), 2: grand jeté shockwave seed.
    /// ai[1] = polarity (0 = white, 1 = black).
    /// Foundation-pattern rendering: safe SpriteBatch, MagnumTextureRegistry textures.
    /// </summary>
    public class BlackSwanFlareProj : ModProjectile
    {
        #region Properties

        public bool IsEmpowered => Projectile.ai[0] >= 1f;
        public bool IsGrandJete => Projectile.ai[0] >= 2f;
        public bool IsBlack => Projectile.ai[1] >= 1f;

        private const float HomingRange = 350f;
        private const float HomingStrength = 0.08f;
        private const float MaxSpeed = 16f;

        private Player Owner => Main.player[Projectile.owner];
        private bool _initialized;

        // GPU primitive trail renderer for shader-driven trail
        private BlackSwanPrimitiveRenderer _trailRenderer;

        #endregion

        public override string Texture => "MagnumOpus/Content/SwanLake/ResonantWeapons/CalloftheBlackSwan/CalloftheBlackSwan";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 20;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.MeleeNoSpeed;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 120;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.extraUpdates = 1;
        }

        public override void AI()
        {
            if (!_initialized)
            {
                _initialized = true;
                Projectile.ai[1] = Main.rand.NextBool() ? 1f : 0f;
                Projectile.rotation = Projectile.velocity.ToRotation();
            }

            // Homing AI
            NPC target = BlackSwanUtils.ClosestNPCAt(Projectile.Center, HomingRange);
            if (target != null)
            {
                Vector2 desiredDir = Projectile.Center.SafeDirectionTo(target.Center);
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, desiredDir * Projectile.velocity.Length(), HomingStrength);
            }

            if (Projectile.velocity.Length() > MaxSpeed)
                Projectile.velocity = Vector2.Normalize(Projectile.velocity) * MaxSpeed;

            Projectile.rotation = Projectile.velocity.ToRotation();

            // Trail dust — dual polarity
            if (Main.rand.NextBool(3))
            {
                int dustType = IsBlack ? DustID.Shadowflame : DustID.WhiteTorch;
                Color dustColor = IsBlack ? new Color(30, 30, 45) : new Color(240, 240, 255);
                Dust d = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(6f, 6f),
                    dustType, -Projectile.velocity * 0.15f + Main.rand.NextVector2Circular(0.5f, 0.5f),
                    0, dustColor, 0.8f);
                d.noGravity = true;
                d.fadeIn = 0.6f;
            }

            // Empowered rainbow sparkle
            if (IsEmpowered && Main.rand.NextBool(4))
            {
                float hue = Main.rand.NextFloat();
                Color rainbow = Main.hslToRgb(hue, 0.85f, 0.8f);
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.RainbowTorch,
                    Main.rand.NextVector2Circular(1f, 1f), 0, rainbow, 0.5f);
                d.noGravity = true;
            }

            // Pulsing light
            float intensity = IsEmpowered ? 0.6f : 0.35f;
            float pulse = 1f + 0.15f * (float)Math.Sin(Projectile.timeLeft * 0.2f);
            Vector3 lightColor = IsBlack
                ? new Vector3(0.15f, 0.15f, 0.25f)
                : new Vector3(0.5f, 0.5f, 0.6f);
            Lighting.AddLight(Projectile.Center, lightColor * intensity * pulse);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            try { Owner.GetModPlayer<BlackSwanPlayer>().RegisterHit(); } catch { }
            try { Owner.GetModPlayer<BlackSwanPlayer>().RegisterFlareHit(); } catch { }

            target.AddBuff(ModContent.BuffType<SwansMark>(), 300);

            Vector2 hitPos = target.Center;

            // Impact sparks — dual polarity
            for (int i = 0; i < 6; i++)
            {
                Vector2 sparkVel = Main.rand.NextVector2CircularEdge(4f, 4f);
                bool isBlack = i % 2 == 0;
                Color col = isBlack ? new Color(30, 30, 45) : new Color(240, 240, 255);
                Dust d = Dust.NewDustPerfect(hitPos, DustID.RainbowTorch, sparkVel, 0, col, 0.5f);
                d.noGravity = true;
            }

            // Feather on impact
            for (int i = 0; i < 2; i++)
            {
                Vector2 featherVel = Main.rand.NextVector2Circular(2f, 2f) + new Vector2(0, -1f);
                bool isBlack = Main.rand.NextBool();
                Dust d = Dust.NewDustPerfect(hitPos + Main.rand.NextVector2Circular(8f, 8f),
                    isBlack ? DustID.Shadowflame : DustID.WhiteTorch, featherVel, 0,
                    isBlack ? new Color(30, 30, 40) : new Color(248, 245, 255), 0.5f);
                d.noGravity = true;
            }

            try { SwanLakeVFXLibrary.SpawnMusicNotes(hitPos, 2, 15f, 0.5f, 0.8f, 22); } catch { }

            // Empowered rainbow burst
            if (IsEmpowered)
            {
                for (int i = 0; i < 8; i++)
                {
                    float hue = (float)i / 8f;
                    Color rainbow = Main.hslToRgb(hue, 0.85f, 0.8f);
                    Vector2 burstVel = Main.rand.NextVector2CircularEdge(6f, 6f);
                    Dust d = Dust.NewDustPerfect(hitPos, DustID.RainbowTorch, burstVel, 0, rainbow, 0.7f);
                    d.noGravity = true;
                }
            }
        }

        #region Rendering (3-Pass Shader Trail + 5-Layer Bloom — Moonlight Sonata Tier)

        /// <summary>
        /// OVERHAULED RENDERING PIPELINE:
        /// Pass 1: SwanFlareGlow @ 3x width (barely-visible bloom underlay)
        /// Pass 2: SwanFlareMain @ 1x width (sharp polarity razor core)
        /// Pass 3: SwanFlareGlow @ 1.5x width (subtle overbright whisper)
        /// Then: 5-layer bloom core (atmospheric → polarity glow → silver → white-hot → star accent)
        /// Plus: empowered rainbow aura when IsEmpowered
        /// </summary>
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            float alphaFade = (255 - Projectile.alpha) / 255f;
            if (alphaFade <= 0.01f) return false;

            try
            {
                // ===== GPU SHADER TRAIL (3 passes as defined by SwanFlareTrail.fx) =====
                DrawShaderTrail(sb);

                // ===== 5-LAYER BLOOM CORE =====
                DrawFlareBloomStack(sb);

                // ===== THEME ACCENTS =====
                sb.End();
                sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, SamplerState.LinearClamp,
                    DepthStencilState.None, RasterizerState.CullCounterClockwise, null,
                    Main.GameViewMatrix.TransformationMatrix);
                BlackSwanUtils.DrawThemeAccents(sb, Projectile.Center, IsEmpowered ? 0.8f : 0.5f, 0.4f);
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

            return false;
        }

        /// <summary>
        /// 3-pass GPU shader trail using SwanFlareTrail.fx techniques.
        /// Technique switching: SwanFlareGlow (wide) → SwanFlareMain (core) → SwanFlareGlow (overbright).
        /// </summary>
        private void DrawShaderTrail(SpriteBatch sb)
        {
            _trailRenderer ??= new BlackSwanPrimitiveRenderer();

            // End SpriteBatch — primitive renderer uses raw GPU calls
            sb.End();

            float time = Main.GlobalTimeWrappedHourly;
            float phase = IsBlack ? 0f : 1f;
            float intensity = IsEmpowered ? 1.2f : 0.9f;
            float waltzPulse = 0.88f + 0.12f * (float)Math.Sin(Main.GameUpdateCount * 0.1047f);

            // Primary/secondary colors based on polarity
            Color primaryColor = IsBlack ? new Color(15, 15, 30) : new Color(255, 255, 255);
            Color secondaryColor = IsBlack ? new Color(180, 180, 200) : new Color(50, 50, 70);

            try
            {
                MiscShaderData flareShader = BlackSwanShaderLoader.GetFlareTrailShader();
                Effect effect = flareShader?.Shader;

                if (effect != null)
                {
                    // Configure all shader uniforms (shared across glow/main passes)
                    effect.Parameters["uColor"]?.SetValue(primaryColor.ToVector3());
                    effect.Parameters["uSecondaryColor"]?.SetValue(secondaryColor.ToVector3());
                    effect.Parameters["uTime"]?.SetValue(time * 3f);
                    effect.Parameters["uScrollSpeed"]?.SetValue(2.5f);
                    effect.Parameters["uPhase"]?.SetValue(phase);
                    effect.Parameters["uOverbrightMult"]?.SetValue(1.3f);

                    // Bind trail body texture via UseImage1
                    if (MagnumTextureRegistry.SoftGlow != null)
                        flareShader.UseImage1(MagnumTextureRegistry.SoftGlow);

                    // === PASS 1: SwanFlareGlow @ 3x width (barely-visible bloom underlay) ===
                    effect.CurrentTechnique = effect.Techniques["SwanFlareGlow"];
                    effect.Parameters["uOpacity"]?.SetValue(0.4f * waltzPulse);
                    effect.Parameters["uIntensity"]?.SetValue(intensity * 0.5f);

                    var glowSettings = new BlackSwanTrailSettings(
                        t =>
                        {
                            float baseW = MathHelper.Lerp(28f, 4f, t);
                            return baseW * (IsEmpowered ? 1.4f : 1f);
                        },
                        t =>
                        {
                            float fade = (1f - t) * 0.35f;
                            Color col = IsBlack
                                ? Color.Lerp(new Color(30, 30, 55), new Color(100, 100, 140), t)
                                : Color.Lerp(new Color(180, 180, 210), new Color(230, 235, 250), t);
                            return col * fade;
                        },
                        shader: flareShader, smoothen: true
                    );
                    _trailRenderer.RenderTrail(Projectile.oldPos, glowSettings, 20);

                    // === PASS 2: SwanFlareMain @ 1x width (sharp polarity razor core) ===
                    effect.CurrentTechnique = effect.Techniques["SwanFlareMain"];
                    effect.Parameters["uOpacity"]?.SetValue(0.85f * waltzPulse);
                    effect.Parameters["uIntensity"]?.SetValue(intensity);

                    var coreSettings = new BlackSwanTrailSettings(
                        t =>
                        {
                            float baseW = MathHelper.Lerp(10f, 1.5f, t);
                            return baseW * (IsEmpowered ? 1.2f : 1f);
                        },
                        t =>
                        {
                            float fade = (1f - t) * 0.8f;
                            Color col = IsBlack
                                ? Color.Lerp(new Color(80, 80, 110), new Color(200, 200, 220), t)
                                : Color.Lerp(new Color(220, 220, 240), new Color(255, 255, 255), t);
                            return col * fade;
                        },
                        shader: flareShader, smoothen: true
                    );
                    _trailRenderer.RenderTrail(Projectile.oldPos, coreSettings, 20);

                    // === PASS 3: SwanFlareGlow @ 1.5x width (subtle overbright whisper) ===
                    effect.CurrentTechnique = effect.Techniques["SwanFlareGlow"];
                    effect.Parameters["uOpacity"]?.SetValue(0.2f);
                    effect.Parameters["uIntensity"]?.SetValue(intensity * 0.7f);
                    effect.Parameters["uOverbrightMult"]?.SetValue(1.8f);

                    var overbrightSettings = new BlackSwanTrailSettings(
                        t =>
                        {
                            float baseW = MathHelper.Lerp(16f, 2f, t);
                            return baseW * (IsEmpowered ? 1.3f : 1f);
                        },
                        t =>
                        {
                            float fade = (1f - t) * 0.2f;
                            Color col = Color.Lerp(new Color(200, 200, 220), new Color(255, 255, 255), t);
                            return col * fade;
                        },
                        shader: flareShader, smoothen: true
                    );
                    _trailRenderer.RenderTrail(Projectile.oldPos, overbrightSettings, 20);
                }
                else
                {
                    // Shader unavailable fallback — basic bloom dot trail
                    DrawFallbackTrail(sb);
                }
            }
            catch
            {
                // Shader error fallback
                DrawFallbackTrail(sb);
            }

            // Restart SpriteBatch for subsequent layers
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                DepthStencilState.None, Main.Rasterizer, null,
                Main.GameViewMatrix.TransformationMatrix);
        }

        /// <summary>
        /// Fallback trail when shader is unavailable — sprite-based bloom trail.
        /// </summary>
        private void DrawFallbackTrail(SpriteBatch sb)
        {
            Texture2D bloom = MagnumTextureRegistry.GetSoftGlow();
            if (bloom == null) return;

            // Need SpriteBatch active for this fallback
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            Vector2 origin = bloom.Size() * 0.5f;
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;
                float t = (float)i / Projectile.oldPos.Length;
                Vector2 drawPos = Projectile.oldPos[i] + Projectile.Size * 0.5f - Main.screenPosition;
                float trailAlpha = (1f - t) * 0.5f;
                float trailScale = MathHelper.Lerp(0.12f, 0.03f, t) * (IsEmpowered ? 1.3f : 1f);
                Color trailCol = IsBlack ? new Color(40, 40, 60, 0) : new Color(200, 200, 220, 0);
                sb.Draw(bloom, drawPos, null, trailCol * trailAlpha, 0f, origin, trailScale, SpriteEffects.None, 0f);
                sb.Draw(bloom, drawPos, null, new Color(255, 255, 255, 0) * trailAlpha * 0.3f, 0f, origin, trailScale * 0.4f, SpriteEffects.None, 0f);
            }

            sb.End();
        }

        /// <summary>
        /// 5-layer bloom core at projectile center (Swan Lake polarity + waltz rhythm).
        /// </summary>
        private void DrawFlareBloomStack(SpriteBatch sb)
        {
            Texture2D softRadial = MagnumTextureRegistry.SoftRadialBloom?.Value;
            Texture2D bloom = MagnumTextureRegistry.SoftGlow?.Value;
            Texture2D pointBloom = MagnumTextureRegistry.PointBloom?.Value;
            Texture2D star = MagnumTextureRegistry.GetStar4Soft();
            Texture2D glowOrb = MagnumTextureRegistry.BloomCircle?.Value;
            if (bloom == null && pointBloom == null) return;

            Vector2 screenPos = Projectile.Center - Main.screenPosition;
            float pulse = 0.8f + 0.2f * (float)Math.Sin(Projectile.timeLeft * 0.15f);
            float waltzPulse = 0.85f + 0.15f * (float)Math.Sin(Main.GameUpdateCount * 0.1047f);
            float baseScale = IsEmpowered ? 0.3f : 0.2f;
            float empScale = IsEmpowered ? 1.1f : 1f;

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null,
                Main.GameViewMatrix.TransformationMatrix);

            // Sub-layer 1: Wide atmospheric halo (polarity-colored)
            Texture2D wideBloom = softRadial ?? bloom;
            if (wideBloom != null)
            {
                Vector2 origin = wideBloom.Size() * 0.5f;
                Color outerColor = IsBlack ? new Color(25, 25, 50, 0) : new Color(210, 215, 235, 0);
                sb.Draw(wideBloom, screenPos, null, outerColor * 0.15f * waltzPulse * empScale,
                    0f, origin, baseScale * 0.25f, SpriteEffects.None, 0f);
            }

            // Sub-layer 2: Mid polarity glow
            if (bloom != null)
            {
                Vector2 origin = bloom.Size() * 0.5f;
                Color midColor = IsBlack ? new Color(60, 60, 90, 0) : new Color(200, 205, 235, 0);
                sb.Draw(bloom, screenPos, null, midColor * 0.22f * pulse * empScale,
                    0f, origin, baseScale * 0.55f, SpriteEffects.None, 0f);
            }

            // Sub-layer 3: Silver core bloom
            if (bloom != null)
            {
                Vector2 origin = bloom.Size() * 0.5f;
                sb.Draw(bloom, screenPos, null, new Color(200, 200, 225, 0) * 0.3f * pulse,
                    0f, origin, baseScale * 0.8f, SpriteEffects.None, 0f);
            }

            // Sub-layer 4: White-hot core point
            if (pointBloom != null)
            {
                Vector2 pbOrigin = pointBloom.Size() * 0.5f;
                sb.Draw(pointBloom, screenPos, null, new Color(255, 255, 255, 0) * 0.65f * waltzPulse,
                    0f, pbOrigin, baseScale * 0.05f, SpriteEffects.None, 0f);
            }

            // Sub-layer 5: Rotating star accent (polarity-tinted)
            if (star != null)
            {
                Vector2 starOrigin = star.Size() * 0.5f;
                Color starCol = IsBlack ? new Color(100, 100, 145, 0) : new Color(242, 242, 255, 0);
                float starRot = Main.GameUpdateCount * 0.08f;
                sb.Draw(star, screenPos, null, starCol * 0.35f * pulse,
                    starRot, starOrigin, baseScale * 0.35f, SpriteEffects.None, 0f);
            }

            // Empowered: pulsating rainbow prismatic aura
            if (IsEmpowered)
            {
                Texture2D auraBloom = glowOrb ?? softRadial ?? bloom;
                if (auraBloom != null)
                {
                    Vector2 auraOrigin = auraBloom.Size() * 0.5f;
                    float hue = (Main.GameUpdateCount * 0.02f) % 1f;
                    Color rainbow = Main.hslToRgb(hue, 0.9f, 0.85f);
                    sb.Draw(auraBloom, screenPos, null, new Color(rainbow.R, rainbow.G, rainbow.B, 0) * 0.15f * waltzPulse,
                        0f, auraOrigin, baseScale * 0.25f, SpriteEffects.None, 0f);

                    // Counter-rotating prismatic ring
                    Color rainbow2 = Main.hslToRgb((hue + 0.5f) % 1f, 0.85f, 0.8f);
                    sb.Draw(auraBloom, screenPos, null, new Color(rainbow2.R, rainbow2.G, rainbow2.B, 0) * 0.15f,
                        0f, auraOrigin, baseScale * 0.2f, SpriteEffects.None, 0f);
                }
            }

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                DepthStencilState.None, Main.Rasterizer, null,
                Main.GameViewMatrix.TransformationMatrix);
        }

        #endregion

        public override void OnKill(int timeLeft)
        {
            // Dispose GPU trail renderer
            _trailRenderer?.Dispose();
            _trailRenderer = null;

            // Death VFX — dual-polarity spark burst
            for (int i = 0; i < 4; i++)
            {
                Vector2 sparkVel = Main.rand.NextVector2CircularEdge(3f, 3f);
                bool isBlack = Main.rand.NextBool();
                Color col = isBlack ? new Color(30, 30, 45) : new Color(240, 240, 255);
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.RainbowTorch, sparkVel, 0, col, 0.3f);
                d.noGravity = true;
            }

            try { SwanLakeVFXLibrary.SpawnMusicNotes(Projectile.Center, 1, 12f, 0.5f, 0.7f, 20); } catch { }
            try { SwanLakeVFXLibrary.SpawnFeatherDrift(Projectile.Center, 2, 15f, 0.2f); } catch { }
        }
    }
}
