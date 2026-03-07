using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using MagnumOpus.Content.SwanLake.ResonantWeapons.IridescentWingspan.Utilities;
using MagnumOpus.Content.SwanLake.Debuffs;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.VFX.Core;
using MagnumOpus.Content.SwanLake.ResonantWeapons.IridescentWingspan.Primitives;
using MagnumOpus.Content.SwanLake.ResonantWeapons.IridescentWingspan.Shaders;
using Terraria.Graphics.Shaders;
using ReLogic.Content;

namespace MagnumOpus.Content.SwanLake.ResonantWeapons.IridescentWingspan.Projectiles
{
    /// <summary>
    /// Spectral wing bolt for Iridescent Wingspan (summoner staff secondary).
    /// 5-bolt fan pattern curves toward cursor. Empowered bolt is 3x larger, pen 5, noclip.
    /// Prismatic Convergence burst when 3+ bolts hit same target area.
    /// Foundation-pattern rendering: bloom trail, no primitives/custom particles.
    /// </summary>
    public class WingspanBoltProj : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Textures/InvisibleProjectile";

        public ref float Timer => ref Projectile.ai[0];
        public ref float BoltIndex => ref Projectile.ai[1]; // 0-4 fan position; -1 = empowered
        public ref float CurveTarget => ref Projectile.localAI[0]; // 0 = normal (curve), 1 = empowered (straight)
        
        private const int TrailLength = 16;
        private Vector2[] oldPos = new Vector2[TrailLength];
        private float[] oldRot = new float[TrailLength];
        private WingspanPrimitiveRenderer _trailRenderer;

        // Hit flash tracking for noise-zone rendering on impact
        private Vector2 _lastHitPos;
        private int _hitFlashTimer;
        private const int HitFlashDuration = 12;

        private Player Owner => Main.player[Projectile.owner];
        private bool IsEmpowered => BoltIndex == -1f;

        public override void SetStaticDefaults() { ProjectileID.Sets.TrailCacheLength[Projectile.type] = TrailLength; }

        public override void SetDefaults()
        {
            Projectile.width = 12;
            Projectile.height = 12;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.penetrate = 2;
            Projectile.timeLeft = 180;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.extraUpdates = 1;
            Projectile.alpha = 50;
        }

        public override void AI()
        {
            Timer++;
            Projectile.rotation = Projectile.velocity.ToRotation();

            // --- Empowered bolt setup ---
            if (IsEmpowered && Timer == 1)
            {
                Projectile.penetrate = 5;
                Projectile.tileCollide = false;
                Projectile.scale = 1.5f;
                Projectile.width = 24;
                Projectile.height = 24;
            }

            // --- Cursor convergence (normal bolts curve toward mouse) ---
            if (!IsEmpowered && Timer > 10 && Timer < 120)
            {
                Vector2 cursorWorld = Main.MouseWorld;
                if (Projectile.owner == Main.myPlayer)
                {
                    Vector2 toCursor = (cursorWorld - Projectile.Center).SafeNormalize(Vector2.UnitY);
                    float curveStrength = 0.035f;
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, toCursor * Projectile.velocity.Length(), curveStrength);
                }
            }

            // --- Trail recording ---
            for (int i = TrailLength - 1; i > 0; i--)
            {
                oldPos[i] = oldPos[i - 1];
                oldRot[i] = oldRot[i - 1];
            }
            oldPos[0] = Projectile.Center;
            oldRot[0] = Projectile.rotation;

            // Hit flash decay
            if (_hitFlashTimer > 0)
                _hitFlashTimer--;

            // --- Spectral wing dust ---
            if (Timer % 2 == 0)
            {
                Color c = WingspanUtils.GetPrismaticEdge(Timer * 0.05f + BoltIndex * 0.3f);
                Dust d = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(4, 4),
                    DustID.WhiteTorch, -Projectile.velocity * 0.2f, 0, c, IsEmpowered ? 0.8f : 0.5f);
                d.noGravity = true;
            }

            // White ethereal sparkle
            if (Timer % 4 == 0)
            {
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.TintableDustLighted,
                    Main.rand.NextVector2Circular(2, 2), 0, WingspanUtils.EtherealWhite * 0.6f, 0.4f);
                d.noGravity = true;
            }

            // Empowered: additional gold feather dust
            if (IsEmpowered && Timer % 3 == 0)
            {
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.WhiteTorch,
                    Main.rand.NextVector2Circular(3, 3), 0, WingspanUtils.WingPrismatic, 0.7f);
                d.noGravity = true;
            }

            Lighting.AddLight(Projectile.Center, 0.5f, 0.5f, 0.7f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<SwansMark>(), 240);

            // Record hit for noise flash rendering
            _lastHitPos = target.Center;
            _hitFlashTimer = HitFlashDuration;

            // Prismatic impact burst
            for (int i = 0; i < 10; i++)
            {
                float angle = MathHelper.TwoPi / 8f * i;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 4f);
                Color c = WingspanUtils.GetPrismaticEdge(i / 8f);
                Dust d = Dust.NewDustPerfect(target.Center, DustID.WhiteTorch, vel, 0, c, 0.7f);
                d.noGravity = true;
            }

            // Convergence tracking: increment player's convergence counter
            try
            {
                var wp = Owner.GetModPlayer<WingspanPlayer>();
                if (wp != null)
                {
                    wp.ConvergenceCount++;
                    // Prismatic convergence burst at 3+ hits
                    if (wp.ConvergenceCount >= 3)
                    {
                        wp.ConvergenceCount = 0;
                        SpawnConvergenceBurst(target.Center);
                    }
                }
            }
            catch { }

            try { SwanLakeVFXLibrary.SpawnRainbowBurst(target.Center, 8, 4.5f); } catch { }
            try { SwanLakeVFXLibrary.SpawnPrismaticSparkles(target.Center, 6, 20f); } catch { }
            try { SwanLakeVFXLibrary.SpawnMusicNotes(target.Center, 2, 15f, 0.6f, 0.9f, 22); } catch { }
        }

        private void SpawnConvergenceBurst(Vector2 pos)
        {
            SoundEngine.PlaySound(SoundID.Item29, pos);

            // Prismatic burst ring
            for (int i = 0; i < 16; i++)
            {
                float angle = MathHelper.TwoPi / 16f * i;
                Vector2 vel = angle.ToRotationVector2() * 5f;
                Color c = WingspanUtils.GetPrismaticEdge(i / 16f);
                Dust d = Dust.NewDustPerfect(pos, DustID.WhiteTorch, vel, 0, c, 1.0f);
                d.noGravity = true;
            }

            // Gold inner burst
            for (int i = 0; i < 12; i++)
            {
                Dust d = Dust.NewDustPerfect(pos, DustID.WhiteTorch,
                    Main.rand.NextVector2Circular(6, 6), 0, WingspanUtils.WingPrismatic, 1.2f);
                d.noGravity = true;
            }

            // Rainbow explosion + feather drift + sparkles
            try { SwanLakeVFXLibrary.SpawnFeatherBurst(pos, 8, 0.35f); } catch { }
            try { SwanLakeVFXLibrary.SpawnRainbowExplosion(pos, 1.2f); } catch { }
            try { SwanLakeVFXLibrary.SpawnPrismaticSparkles(pos, 8, 30f); } catch { }
            try { SwanLakeVFXLibrary.SpawnMusicNotes(pos, 4, 25f, 0.7f, 1.0f, 28); } catch { }
        }

        public override void OnKill(int timeLeft)
        {
            _trailRenderer?.Dispose();

            for (int i = 0; i < 6; i++)
            {
                Color c = WingspanUtils.GetPrismaticEdge(i / 6f);
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.WhiteTorch,
                    Main.rand.NextVector2Circular(3, 3), 0, c, 0.5f);
                d.noGravity = true;
            }

            try { SwanLakeVFXLibrary.SpawnFeatherDrift(Projectile.Center, 2, 12f); } catch { }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Vector2 screenPos = Main.screenPosition;

            try
            {
                sb.End();

                // ============ GPU SHADER TRAIL (3-pass feather-dissolve) ============
                if (WingspanShaderLoader.HasWingspanFlareTrailShader)
                {
                    _trailRenderer ??= new WingspanPrimitiveRenderer();
                    var shaderData = GameShaders.Misc["MagnumOpus:WingspanFlareTrail"];
                    var effect = shaderData.Shader;

                    // Common shader uniforms
                    float huePhase = IsEmpowered ? 0.5f : MathHelper.Clamp(BoltIndex / 5f, 0f, 1f);
                    effect.Parameters["uTime"]?.SetValue((float)Main.timeForVisualEffects * 0.02f);
                    effect.Parameters["uOpacity"]?.SetValue(0.9f);
                    effect.Parameters["uScrollSpeed"]?.SetValue(0.8f);
                    effect.Parameters["uNoiseScale"]?.SetValue(3f);
                    effect.Parameters["uPhase"]?.SetValue(huePhase);

                    if (MagnumTextureRegistry.PerlinNoise != null)
                    {
                        shaderData.UseImage1(MagnumTextureRegistry.PerlinNoise);
                        effect.Parameters["uHasSecondaryTex"]?.SetValue(1f);
                        effect.Parameters["uSecondaryTexScale"]?.SetValue(2f);
                        effect.Parameters["uSecondaryTexScroll"]?.SetValue(0.5f);
                    }

                    float baseWidth = IsEmpowered ? 16f : 10f;
                    float taperWidth = IsEmpowered ? 30f : 22f;

                    // Pass 1: Wide pearlescent bloom (WingspanFlareGlow @ 2.5x)
                    effect.Parameters["uColor"]?.SetValue(WingspanUtils.EtherealWhite.ToVector3());
                    Color secondaryGlow = WingspanUtils.GetPrismaticEdge(huePhase);
                    effect.Parameters["uSecondaryColor"]?.SetValue(secondaryGlow.ToVector3());
                    effect.Parameters["uIntensity"]?.SetValue(0.4f);
                    effect.Parameters["uOverbrightMult"]?.SetValue(0f);
                    effect.Parameters["uDistortionAmt"]?.SetValue(0.02f);
                    effect.CurrentTechnique = effect.Techniques["WingspanFlareGlow"];

                    var glowSettings = new WingspanTrailSettings(
                        t => (baseWidth + taperWidth * (1f - t)) * 2.5f,
                        t => Color.Lerp(secondaryGlow, WingspanUtils.EtherealWhite, 0.5f) * (0.3f * (1f - t)),
                        shaderData
                    );
                    _trailRenderer.RenderTrail(oldPos, glowSettings, TrailLength);

                    // Pass 2: Core feather-dissolve trail (WingspanFlareMain @ 1x)
                    effect.Parameters["uColor"]?.SetValue(Color.White.ToVector3());
                    effect.Parameters["uSecondaryColor"]?.SetValue(secondaryGlow.ToVector3());
                    effect.Parameters["uIntensity"]?.SetValue(1.2f);
                    effect.Parameters["uOverbrightMult"]?.SetValue(0.15f);
                    effect.Parameters["uDistortionAmt"]?.SetValue(0.06f);
                    effect.CurrentTechnique = effect.Techniques["WingspanFlareMain"];

                    var mainSettings = new WingspanTrailSettings(
                        t => baseWidth + taperWidth * (1f - t),
                        t => Color.Lerp(Color.White, secondaryGlow, t * 0.4f) * (0.85f * (1f - t)),
                        shaderData
                    );
                    _trailRenderer.RenderTrail(oldPos, mainSettings, TrailLength);

                    // Pass 3: Overbright inner halo (WingspanFlareGlow @ 1.5x)
                    effect.Parameters["uColor"]?.SetValue(Color.White.ToVector3());
                    effect.Parameters["uSecondaryColor"]?.SetValue(WingspanUtils.EtherealWhite.ToVector3());
                    effect.Parameters["uIntensity"]?.SetValue(1.8f);
                    effect.Parameters["uOverbrightMult"]?.SetValue(0.5f);
                    effect.Parameters["uDistortionAmt"]?.SetValue(0.03f);
                    effect.CurrentTechnique = effect.Techniques["WingspanFlareGlow"];

                    var innerSettings = new WingspanTrailSettings(
                        t => (baseWidth + taperWidth * (1f - t)) * 1.5f,
                        t => Color.White * (0.35f * (1f - t)),
                        shaderData
                    );
                    _trailRenderer.RenderTrail(oldPos, innerSettings, TrailLength);
                }
                else
                {
                    // Fallback: basic bloom trail
                    sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, SamplerState.LinearClamp,
                        DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
                    Texture2D fb = MagnumTextureRegistry.GetSoftGlow();
                    if (fb != null)
                    {
                        Vector2 fbO = fb.Size() * 0.5f;
                        for (int i = TrailLength - 1; i >= 1; i--)
                        {
                            if (oldPos[i] == Vector2.Zero) continue;
                            float p = 1f - i / (float)TrailLength;
                            Color c = WingspanUtils.GetPrismaticEdge(i / (float)TrailLength + (float)Main.timeForVisualEffects * 0.008f);
                            sb.Draw(fb, oldPos[i] - screenPos, null, c * (p * 0.4f), oldRot[i], fbO, 0.2f + p * 0.15f, SpriteEffects.None, 0f);
                        }
                    }
                    sb.End();
                }

                // Restart SpriteBatch for hit flash + bloom core (both use TrueAdditive)
                sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, SamplerState.LinearClamp,
                    DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

                // ============ HIT FLASH SPARKLE BURST (replaces noise zone) ============
                if (_hitFlashTimer > 0 && _lastHitPos != Vector2.Zero)
                {
                    float flashProgress = 1f - (float)_hitFlashTimer / HitFlashDuration;
                    float flashAlpha = (1f - flashProgress) * (1f - flashProgress);
                    float flashRadius = MathHelper.Lerp(8f, IsEmpowered ? 50f : 35f, flashProgress);
                    SwanLakeVFXLibrary.DrawPrismaticSparkleImpact(sb, _lastHitPos, flashRadius,
                        (float)Main.timeForVisualEffects, flashAlpha * 0.8f);
                }

                // ============ BLOOM CORE (5-layer prismatic) ============

                Texture2D bloom = MagnumTextureRegistry.GetSoftGlow();
                Texture2D point = MagnumTextureRegistry.GetPointBloom();
                Texture2D star = MagnumTextureRegistry.GetStar4Soft();

                float boltScale = IsEmpowered ? 1.5f : 1f;
                Vector2 drawPos = Projectile.Center - screenPos;
                float pulse = 0.9f + 0.1f * MathF.Sin((float)Main.timeForVisualEffects * 0.09f + BoltIndex);

                // Layer 1: Outer spectral glow (cap to 300px on 512px SoftGlow)
                if (bloom != null)
                {
                    Vector2 bOrigin = bloom.Size() * 0.5f;
                    Color outerColor = WingspanUtils.GetPrismaticEdge(Timer * 0.02f + BoltIndex * 0.2f);
                    sb.Draw(bloom, drawPos, null, outerColor * 0.35f * pulse, 0f, bOrigin, MathHelper.Min(0.4f * boltScale * pulse, 0.586f), SpriteEffects.None, 0f);
                }

                // Layer 2: Ethereal white mid glow
                if (bloom != null)
                {
                    Vector2 bOrigin = bloom.Size() * 0.5f;
                    sb.Draw(bloom, drawPos, null, WingspanUtils.EtherealWhite * 0.45f * pulse, 0f, bOrigin, 0.25f * boltScale * pulse, SpriteEffects.None, 0f);
                }

                // Layer 3: Hot white core
                if (point != null)
                {
                    Vector2 pOrigin = point.Size() * 0.5f;
                    sb.Draw(point, drawPos, null, Color.White * 0.85f, 0f, pOrigin, 0.07f * boltScale * pulse, SpriteEffects.None, 0f);
                }

                // Layer 4: Star sparkle accent (rotating)
                if (star != null)
                {
                    Vector2 sOrigin = star.Size() * 0.5f;
                    float starRot = (float)Main.timeForVisualEffects * 0.04f + BoltIndex;
                    Color starColor = WingspanUtils.GetPrismaticEdge(Timer * 0.015f + 0.5f);
                    sb.Draw(star, drawPos, null, starColor * 0.3f * pulse, starRot, sOrigin, 0.2f * boltScale, SpriteEffects.None, 0f);
                }

                // Layer 5: Empowered wing-like prismatic side accents
                if (IsEmpowered && bloom != null)
                {
                    Vector2 bOrigin = bloom.Size() * 0.5f;
                    for (int side = -1; side <= 1; side += 2)
                    {
                        Vector2 wingOffset = Projectile.velocity.SafeNormalize(Vector2.UnitY).RotatedBy(MathHelper.PiOver2 * side) * 14f;
                        Color wingColor = WingspanUtils.GetPrismaticEdge(Timer * 0.02f + side * 0.33f);
                        sb.Draw(bloom, drawPos + wingOffset, null, wingColor * 0.25f, 0f, bOrigin, 0.22f, SpriteEffects.None, 0f);
                    }
                    // Additional gold feather accent
                    sb.Draw(bloom, drawPos, null, WingspanUtils.WingPrismatic * 0.15f, 0f, bloom.Size() * 0.5f, 0.5f * pulse, SpriteEffects.None, 0f);
                }
            }
            catch { }
            finally
            {
                sb.End();
                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                    DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            }

            // Theme accents (additive)
            try
            {
                sb.End();
                sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, SamplerState.LinearClamp, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
                WingspanUtils.DrawThemeAccents(sb, Projectile.Center, 1f, 0.6f);
                sb.End();
                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            }
            catch { }

            return false;
        }
    }
}
