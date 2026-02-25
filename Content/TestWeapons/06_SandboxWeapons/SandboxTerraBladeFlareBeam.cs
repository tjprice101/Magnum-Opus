using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.Shaders;
using MagnumOpus.Common.Systems.VFX.Effects;
using MagnumOpus.Content.TestWeapons.SandboxWeapons.Shaders;

namespace MagnumOpus.Content.TestWeapons.SandboxWeapons
{
    /// <summary>
    /// Shader-driven flare beam projectile for the Sandbox TerraBlade swing.
    /// Renders a vertex-mesh beam from trail positions with the TerraBladeFlareBeamShader,
    /// featuring heavy wave distortion and directional motion-blur streaking.
    ///
    /// Rendering pipeline (3-pass + accent flares):
    ///   Pass 1: Wide bloom — large, soft-edged additive glow for motion blur halo
    ///   Pass 2: Main beam — flowing energy with wave distortion (alpha blend)
    ///   Pass 3: White-hot core — narrow, intense center streak (additive)
    ///   Accent: Head flare sprites for bright leading edge
    /// </summary>
    public class SandboxTerraBladeFlareBeam : ModProjectile
    {
        #region Constants

        private const int TrailCacheSize = 16;
        private const float BaseSpeed = 22f;
        private const int MaxLifetime = 45;

        // Beam mesh parameters
        private const float BeamBaseWidth = 40f;
        private const int MeshSegments = 20;
        private const float ScrollSpeed = 4.0f;

        // Fluid wobble — 3-frequency for organic water-like beam undulation
        private const float WobbleFreq1 = 4.5f;
        private const float WobbleFreq2 = 7.3f;
        private const float WobbleFreq3 = 10.1f;
        private const float WobbleAmp1 = 2.2f;
        private const float WobbleAmp2 = 1.3f;
        private const float WobbleAmp3 = 0.6f;

        // Shader beam vertex pool (shared static to avoid per-frame allocation)
        private const int VertexCap = 128;
        private const int IndexCap = 384;
        private static VertexPositionColorTexture[] _vertices;
        private static short[] _indices;
        private static bool _poolReady;

        #endregion

        #region State

        private int timer = 0;
        private float wobblePhase;

        #endregion

        #region Setup

        public override string Texture => "MagnumOpus/Assets/Particles/EnergyFlare";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = TrailCacheSize;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 30;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.MeleeNoSpeed;
            Projectile.penetrate = 3;
            Projectile.tileCollide = false;
            Projectile.timeLeft = MaxLifetime;
            Projectile.extraUpdates = 1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 8;
            Projectile.ignoreWater = true;
            Projectile.alpha = 255;

            wobblePhase = Main.rand.NextFloat(MathHelper.TwoPi);
        }

        #endregion

        #region AI

        public override void AI()
        {
            timer++;
            Projectile.rotation = Projectile.velocity.ToRotation();

            // Slight deceleration for a "fading beam" feel
            Projectile.velocity *= 0.98f;

            // Fluid wobble — 3-frequency organic beam undulation perpendicular to velocity
            float t = timer * 0.1f + wobblePhase;
            Vector2 dir = Projectile.velocity.SafeNormalize(Vector2.UnitX);
            Vector2 perp = new Vector2(-dir.Y, dir.X);
            float wobble = MathF.Sin(t * WobbleFreq1) * WobbleAmp1
                         + MathF.Sin(t * WobbleFreq2 + 2.5f) * WobbleAmp2
                         + MathF.Sin(t * WobbleFreq3 + 4.8f) * WobbleAmp3;
            Projectile.position += perp * wobble * 0.2f;

            // Spark dust along the beam
            if (Main.rand.NextBool(2))
            {
                Vector2 offset = Main.rand.NextVector2Circular(8f, 8f);
                Color dustColor = TerraBladeShaderManager.GetPaletteColor(Main.rand.NextFloat(0.3f, 0.8f));
                Dust d = Dust.NewDustPerfect(Projectile.Center + offset, DustID.GreenTorch,
                    -Projectile.velocity * 0.1f + Main.rand.NextVector2Circular(1f, 1f),
                    0, dustColor, 1.0f);
                d.noGravity = true;
                d.fadeIn = 1.1f;
            }

            // GlowSpark particles — sparkle trail
            if (timer % 3 == 0)
            {
                Vector2 sparkVel = -Projectile.velocity.SafeNormalize(Vector2.UnitY) * Main.rand.NextFloat(0.5f, 1.5f);
                sparkVel = sparkVel.RotatedBy(Main.rand.NextFloat(-0.4f, 0.4f));
                Color sparkColor = TerraBladeShaderManager.GetPaletteColor(Main.rand.NextFloat(0.4f, 0.8f));
                var spark = new GlowSparkParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(6f, 6f),
                    sparkVel, sparkColor, 0.12f, 10);
                MagnumParticleHandler.SpawnParticle(spark);
            }

            // Sparkle flicker particles
            if (timer % 4 == 0)
            {
                Color flickerColor = TerraBladeShaderManager.GetPaletteColor(Main.rand.NextFloat(0.5f, 0.9f));
                var sparkle = new SparkleParticle(
                    Projectile.Center, Main.rand.NextVector2Circular(1f, 1f),
                    flickerColor, flickerColor * 0.5f,
                    Main.rand.NextFloat(0.15f, 0.3f), 8);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }

            // Lighting
            Color light = TerraBladeShaderManager.GetPaletteColor(0.5f);
            float lifeRatio = (float)Projectile.timeLeft / (float)MaxLifetime;
            Lighting.AddLight(Projectile.Center, light.ToVector3() * 0.7f * lifeRatio);
        }

        #endregion

        #region Hit Effects

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            for (int i = 0; i < 8; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(5f, 5f);
                Color dustColor = TerraBladeShaderManager.GetPaletteColor(Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(target.Center, DustID.GreenTorch, vel, 0, dustColor, 1.3f);
                d.noGravity = true;
            }

            // GlowSpark burst
            for (int i = 0; i < 4; i++)
            {
                Vector2 sparkVel = Main.rand.NextVector2Circular(4f, 4f);
                Color sparkColor = TerraBladeShaderManager.GetPaletteColor(Main.rand.NextFloat(0.3f, 0.8f));
                var spark = new GlowSparkParticle(
                    target.Center, sparkVel, sparkColor, 0.13f, 12);
                MagnumParticleHandler.SpawnParticle(spark);
            }

            var ring = new BloomRingParticle(target.Center, Vector2.Zero,
                TerraBladeShaderManager.GetPaletteColor(0.5f) * 0.6f, 0.18f, 14);
            MagnumParticleHandler.SpawnParticle(ring);

            Lighting.AddLight(target.Center, 0.5f, 0.9f, 0.5f);
        }

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 5f);
                Color dustColor = TerraBladeShaderManager.GetPaletteColor(Main.rand.NextFloat(0.3f, 0.8f));
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.GreenTorch, vel, 0, dustColor, 1.0f);
                d.noGravity = true;
            }

            Lighting.AddLight(Projectile.Center, 0.3f, 0.5f, 0.3f);
        }

        #endregion

        #region Rendering

        public override bool PreDraw(ref Color lightColor)
        {
            float lifeRatio = (float)Projectile.timeLeft / (float)MaxLifetime;
            float fadeIn = MathHelper.Clamp(timer / 5f, 0f, 1f);
            float opacity = fadeIn * lifeRatio;

            if (opacity <= 0f)
                return false;

            // Try shader beam pipeline first; fall back to sprite rendering
            Effect shader = ShaderLoader.TerraBladeFlareBeam;
            if (shader != null)
            {
                DrawShaderBeam(shader, opacity);
            }
            else
            {
                DrawFallbackSprites(opacity);
            }

            DrawHeadFlare(opacity);

            return false;
        }

        /// <summary>
        /// Shader beam pipeline: builds a vertex mesh from trail positions and renders
        /// with 3-pass wave-distorted motion-blur shader.
        /// </summary>
        private void DrawShaderBeam(Effect shader, float opacity)
        {
            EnsureVertexPool();
            if (_vertices == null)
                return;

            // Collect trail control points: head (current) → tail (oldest)
            Vector2[] points = CollectTrailPoints(out int pointCount);
            if (pointCount < 2)
                return;

            float time = Main.GlobalTimeWrappedHourly;
            float uvScroll = time * ScrollSpeed;

            Texture2D noise1 = ShaderLoader.GetNoiseTexture("SoftCircularCaustics");
            Texture2D noise2 = ShaderLoader.GetNoiseTexture("TileableFBMNoise");
            Color primary = TerraBladeShaderManager.GetPaletteColor(0.40f);
            Color secondary = TerraBladeShaderManager.GetPaletteColor(0.65f);

            Matrix viewProj = Matrix.CreateLookAt(Vector3.Backward, Vector3.Zero, Vector3.Up)
                            * Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

            try { Main.spriteBatch.End(); } catch { }

            try
            {
                var device = Main.instance.GraphicsDevice;
                var prevBlend = device.BlendState;
                var prevRaster = device.RasterizerState;
                var prevDepth = device.DepthStencilState;

                device.RasterizerState = RasterizerState.CullNone;
                device.DepthStencilState = DepthStencilState.None;

                // Shared shader parameters
                shader.Parameters["uWorldViewProjection"]?.SetValue(viewProj);
                shader.Parameters["uTime"]?.SetValue(time);
                shader.Parameters["uNoiseSpeed1"]?.SetValue(-2.0f);
                shader.Parameters["uNoiseSpeed2"]?.SetValue(1.2f);
                shader.Parameters["uNoiseScale1"]?.SetValue(3.0f);
                shader.Parameters["uNoiseScale2"]?.SetValue(4.5f);
                shader.Parameters["uPulseSpeed"]?.SetValue(5.0f);

                // Wave distortion — heavy visible waves
                shader.Parameters["uWaveAmplitude"]?.SetValue(0.10f);
                shader.Parameters["uWaveFrequency"]?.SetValue(6.0f);

                // Bind noise textures
                if (noise1 != null) device.Textures[0] = noise1;
                if (noise2 != null) device.Textures[1] = noise2;
                device.SamplerStates[0] = SamplerState.LinearWrap;
                device.SamplerStates[1] = SamplerState.LinearWrap;

                int vc, tc;

                // Pass 1: Wide bloom glow (heavy motion blur halo)
                FillBeamMesh(points, pointCount, uvScroll, BeamBaseWidth, 3.5f, opacity, out vc, out tc);
                if (vc > 0)
                {
                    SetShaderPass(shader, primary, secondary, 0.25f * opacity, 0.8f, 0.45f, 3.0f);
                    device.BlendState = BlendState.Additive;
                    ApplyAndDraw(shader, device, vc, tc);
                }

                // Pass 2: Main flowing beam with wave distortion
                FillBeamMesh(points, pointCount, uvScroll, BeamBaseWidth, 1.0f, opacity, out vc, out tc);
                if (vc > 0)
                {
                    SetShaderPass(shader, primary, secondary, 0.9f * opacity, 1.3f, 0.18f, 1.8f);
                    device.BlendState = BlendState.AlphaBlend;
                    ApplyAndDraw(shader, device, vc, tc);
                }

                // Pass 3: White-hot core (narrow, intense, additive)
                FillBeamMesh(points, pointCount, uvScroll, BeamBaseWidth, 0.2f, opacity, out vc, out tc);
                if (vc > 0)
                {
                    SetShaderPass(shader, Color.White, new Color(200, 255, 230), 1.0f * opacity, 1.6f, 0.08f, 3.5f);
                    device.BlendState = BlendState.Additive;
                    ApplyAndDraw(shader, device, vc, tc);
                }

                device.BlendState = prevBlend;
                device.RasterizerState = prevRaster;
                device.DepthStencilState = prevDepth;
            }
            finally
            {
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                    SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone,
                    null, Main.GameViewMatrix.TransformationMatrix);
            }
        }

        /// <summary>
        /// Collects valid trail positions from current center (head) backward through
        /// old positions (tail), adding wave wobble to interior points.
        /// </summary>
        private Vector2[] CollectTrailPoints(out int count)
        {
            // Head first, then old positions going backward (comet-like motion blur)
            Vector2[] raw = new Vector2[TrailCacheSize + 1];
            raw[0] = Projectile.Center;
            int rawCount = 1;

            for (int i = 0; i < TrailCacheSize; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero)
                    break;
                raw[rawCount++] = Projectile.oldPos[i] + Projectile.Size * 0.5f;
            }

            if (rawCount < 2)
            {
                count = 0;
                return raw;
            }

            // Add wave wobble perpendicular to beam direction at interior points
            float time = Main.GlobalTimeWrappedHourly;
            Vector2 beamDir = (raw[0] - raw[rawCount - 1]).SafeNormalize(Vector2.UnitX);
            Vector2 beamPerp = new Vector2(-beamDir.Y, beamDir.X);

            for (int i = 1; i < rawCount - 1; i++)
            {
                float t = (float)i / (rawCount - 1);
                float wobble = MathF.Sin(t * MathHelper.TwoPi * 2.5f + time * 4.0f) * 6f
                             + MathF.Sin(t * MathHelper.TwoPi * 4.2f - time * 6.5f) * 3f;
                // Taper wave at endpoints
                float taper = MathF.Sin(t * MathHelper.Pi);
                raw[i] += beamPerp * wobble * taper;
            }

            count = rawCount;
            return raw;
        }

        /// <summary>
        /// Fills vertex/index buffers with a comet-shaped beam strip.
        /// Width tapers from wide at head (index 0) to thin at tail (last index).
        /// Alpha fades linearly from head to tail for natural motion blur.
        /// </summary>
        private static void FillBeamMesh(Vector2[] points, int pointCount, float uvScroll,
            float baseWidth, float widthMult, float alphaFade, out int vertexCount, out int triangleCount)
        {
            vertexCount = pointCount * 2;
            triangleCount = (pointCount - 1) * 2;

            if (vertexCount > VertexCap || triangleCount * 3 > IndexCap)
            {
                vertexCount = 0;
                triangleCount = 0;
                return;
            }

            for (int i = 0; i < pointCount; i++)
            {
                float ratio = (float)i / (pointCount - 1);

                // Comet taper: wide at head (ratio=0), thin at tail (ratio=1)
                // Smoothstep cubic for satisfying falloff shape
                float taperSmooth = ratio * ratio * (3f - 2f * ratio);
                float width = MathHelper.Lerp(baseWidth, baseWidth * 0.05f, taperSmooth) * widthMult;

                // Alpha: bright at head, fading toward tail
                float headFade = 1f - ratio * ratio; // Quadratic falloff
                // Soft fade at the very front tip too
                float tipFade = MathHelper.Clamp(ratio * 8f, 0f, 1f);
                Color vertColor = Color.White * (headFade * tipFade * alphaFade);

                // Direction for perpendicular calculation
                Vector2 dir;
                if (i == 0 && pointCount > 1)
                    dir = (points[0] - points[1]).SafeNormalize(Vector2.UnitY);
                else if (i == pointCount - 1)
                    dir = (points[i - 1] - points[i]).SafeNormalize(Vector2.UnitY);
                else
                    dir = (points[i - 1] - points[i + 1]).SafeNormalize(Vector2.UnitY);

                Vector2 perp = new Vector2(-dir.Y, dir.X);
                Vector2 screenPos = points[i] - Main.screenPosition;
                float u = ratio + uvScroll;

                Vector2 topPos = screenPos + perp * width * 0.5f;
                Vector2 bottomPos = screenPos - perp * width * 0.5f;

                _vertices[i * 2] = new VertexPositionColorTexture(
                    new Vector3(topPos.X, topPos.Y, 0), vertColor, new Vector2(u, 0));
                _vertices[i * 2 + 1] = new VertexPositionColorTexture(
                    new Vector3(bottomPos.X, bottomPos.Y, 0), vertColor, new Vector2(u, 1));
            }

            int idx = 0;
            for (int i = 0; i < pointCount - 1; i++)
            {
                int b = i * 2;
                _indices[idx++] = (short)b;
                _indices[idx++] = (short)(b + 1);
                _indices[idx++] = (short)(b + 2);
                _indices[idx++] = (short)(b + 1);
                _indices[idx++] = (short)(b + 3);
                _indices[idx++] = (short)(b + 2);
            }
        }

        private static void SetShaderPass(Effect shader, Color primary, Color secondary,
            float opacity, float intensity, float edgeSoftness, float overbrightMult)
        {
            shader.Parameters["uColor"]?.SetValue(primary.ToVector3());
            shader.Parameters["uSecondaryColor"]?.SetValue(secondary.ToVector3());
            shader.Parameters["uOpacity"]?.SetValue(opacity);
            shader.Parameters["uIntensity"]?.SetValue(intensity);
            shader.Parameters["uEdgeSoftness"]?.SetValue(edgeSoftness);
            shader.Parameters["uOverbrightMult"]?.SetValue(overbrightMult);
        }

        private static void ApplyAndDraw(Effect shader, GraphicsDevice device, int vertexCount, int triangleCount)
        {
            foreach (var pass in shader.CurrentTechnique.Passes)
                pass.Apply();
            device.DrawUserIndexedPrimitives(PrimitiveType.TriangleList,
                _vertices, 0, vertexCount, _indices, 0, triangleCount);
        }

        private static void EnsureVertexPool()
        {
            if (_poolReady || Main.dedServ) return;
            _vertices = new VertexPositionColorTexture[VertexCap];
            _indices = new short[IndexCap];
            _poolReady = true;
        }

        /// <summary>
        /// Bright flare sprites at the projectile head for a hot leading edge.
        /// </summary>
        private void DrawHeadFlare(float opacity)
        {
            SpriteBatch sb = Main.spriteBatch;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float time = Main.GlobalTimeWrappedHourly;
            float velRot = Projectile.velocity.ToRotation();
            float pulse = 1f + MathF.Sin(time * 10f + timer * 0.3f) * 0.10f;

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null,
                Main.GameViewMatrix.TransformationMatrix);

            Texture2D bloomTex = SandboxVFXHelper.SafeRequest("MagnumOpus/Assets/VFX/Blooms/Perfect Soft Color Bloom")
                                ?? Terraria.GameContent.TextureAssets.Extra[98].Value;
            Vector2 origin = bloomTex.Size() * 0.5f;

            // Outer bloom at head
            Color outerColor = TerraBladeShaderManager.GetPaletteColor(0.35f) with { A = 0 };
            sb.Draw(bloomTex, drawPos, null, outerColor * 0.5f * opacity,
                0f, origin, 0.25f * pulse, SpriteEffects.None, 0f);

            // Inner bright bloom
            Color innerColor = TerraBladeShaderManager.GetPaletteColor(0.65f) with { A = 0 };
            sb.Draw(bloomTex, drawPos, null, innerColor * 0.65f * opacity,
                0f, origin, 0.10f * pulse, SpriteEffects.None, 0f);

            // White-hot core dot
            sb.Draw(bloomTex, drawPos, null, (Color.White with { A = 0 }) * 0.55f * opacity,
                0f, origin, 0.04f * pulse, SpriteEffects.None, 0f);

            // Anamorphic streak along velocity for motion blur accent
            Texture2D streakTex = SandboxVFXHelper.SafeRequest("MagnumOpus/Assets/VFX/Blooms/Horizontal Anamorphic Streak");
            if (streakTex != null)
            {
                Vector2 streakOrigin = streakTex.Size() * 0.5f;
                Color streakColor = TerraBladeShaderManager.GetPaletteColor(0.55f) with { A = 0 };
                sb.Draw(streakTex, drawPos, null, streakColor * 0.4f * opacity,
                    velRot, streakOrigin, new Vector2(0.25f, 0.03f) * pulse,
                    SpriteEffects.None, 0f);
            }

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                DepthStencilState.None, RasterizerState.CullNone, null,
                Main.GameViewMatrix.TransformationMatrix);
        }

        /// <summary>
        /// Fallback sprite rendering when shaders are unavailable.
        /// Uses stretched flare textures with afterimage trail.
        /// </summary>
        private void DrawFallbackSprites(float opacity)
        {
            SpriteBatch sb = Main.spriteBatch;
            float velRotation = Projectile.velocity.ToRotation();
            float time = Main.GlobalTimeWrappedHourly;
            float pulse = 1f + MathF.Sin(time * 10f + timer * 0.3f) * 0.10f;
            float speed = Projectile.velocity.Length();
            float dynamicStretch = 1f + speed * 0.04f;

            Texture2D flare1 = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/EnergyFlare").Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null,
                Main.GameViewMatrix.TransformationMatrix);

            // Outer glow
            {
                Vector2 origin = flare1.Size() * 0.5f;
                Color outerColor = TerraBladeShaderManager.GetPaletteColor(0.25f) with { A = 0 };
                Vector2 stretchScale = new Vector2(24f * dynamicStretch, 0.10f) * pulse;
                sb.Draw(flare1, drawPos, null, outerColor * 0.28f * opacity,
                    velRotation, origin, stretchScale, SpriteEffects.None, 0f);
            }

            // White-hot center
            {
                Vector2 origin = flare1.Size() * 0.5f;
                Vector2 stretchScale = new Vector2(7f * dynamicStretch, 0.012f) * pulse;
                sb.Draw(flare1, drawPos, null, (Color.White with { A = 0 }) * 0.58f * opacity,
                    velRotation, origin, stretchScale, SpriteEffects.None, 0f);
            }

            // Afterimage trail
            Vector2 flareOrigin = flare1.Size() * 0.5f;
            for (int i = 1; i < TrailCacheSize; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;
                float trailProgress = (float)i / TrailCacheSize;
                float trailAlpha = (1f - trailProgress) * 0.28f * opacity;
                Vector2 trailDrawPos = Projectile.oldPos[i] + Projectile.Size * 0.5f - Main.screenPosition;
                float trailRot = Projectile.oldRot[i];
                float trailStretch = MathHelper.Lerp(12f, 2.8f, trailProgress);
                float trailSquish = MathHelper.Lerp(0.07f, 0.025f, trailProgress);
                Color trailColor = TerraBladeShaderManager.GetPaletteColor(0.3f + trailProgress * 0.4f) with { A = 0 };
                sb.Draw(flare1, trailDrawPos, null, trailColor * trailAlpha,
                    trailRot, flareOrigin, new Vector2(trailStretch, trailSquish), SpriteEffects.None, 0f);
            }

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                DepthStencilState.None, RasterizerState.CullNone, null,
                Main.GameViewMatrix.TransformationMatrix);
        }

        #endregion

        #region Networking

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(timer);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            timer = reader.ReadInt32();
        }

        #endregion
    }
}
