using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.Shaders;
using MagnumOpus.Content.ClairDeLune;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.ClairDeLune.Weapons.StarfallWhisper.Projectiles
{
    /// <summary>
    /// Temporal Arrow — Crystal arrow with GPU-driven SparkleTrail (SparkleProjectileFoundation),
    /// multi-layer bloom trail, 5-layer crystal head, and StarFlare/4PointedStar accents.
    /// Renders 5 layers: (1) SparkleTrail shader, (2) Bloom trail, (3) Bloom halo,
    /// (4) Crystal body + CrystalShimmer shader, (5) Sparkle accents.
    /// </summary>
    public class TemporalArrowProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Textures/InvisibleProjectile";

        // --- Trail ring buffer ---
        private const int TrailLength = 24;
        private Vector2[] _trailPositions = new Vector2[TrailLength];
        private float[] _trailRotations = new float[TrailLength];
        private int _trailIndex;

        // --- Shader caching ---
        private static Effect _sparkleTrailShader;
        private static Effect _crystalShimmerShader;
        private static Asset<Texture2D> _softGlow;
        private static Asset<Texture2D> _softRadialBloom;
        private static Asset<Texture2D> _pointBloom;
        private static Asset<Texture2D> _starFlare;
        private static Asset<Texture2D> _4ptStar;
        private static Asset<Texture2D> _softCircle;
        private static Asset<Texture2D> _sparkleHard;
        private static Asset<Texture2D> _glowMask;
        private static Asset<Texture2D> _gradientLUT;
        private VertexStrip _strip;

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 120;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.arrow = true;
            Projectile.extraUpdates = 1;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();

            // Record trail in ring buffer
            int idx = _trailIndex % TrailLength;
            _trailPositions[idx] = Projectile.Center;
            _trailRotations[idx] = Projectile.rotation;
            _trailIndex++;

            // Temporal shimmer — clock-tick flash every 8 frames
            if (Projectile.timeLeft % 8 == 0)
            {
                var shimmer = new SparkleParticle(
                    Projectile.Center, -Projectile.velocity * 0.05f,
                    ClairDeLunePalette.PearlFrost with { A = 0 } * 0.6f, 0.12f, 6);
                MagnumParticleHandler.SpawnParticle(shimmer);
            }

            // Subtle crystal trail dust
            if (Main.GameUpdateCount % 2 == 0)
            {
                var trail = new GenericGlowParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(3f, 3f),
                    -Projectile.velocity * 0.02f,
                    ClairDeLunePalette.SoftBlue with { A = 0 } * 0.3f, 0.06f, 8, true);
                MagnumParticleHandler.SpawnParticle(trail);
            }

            Lighting.AddLight(Projectile.Center, ClairDeLunePalette.PearlFrost.ToVector3() * 0.3f);
        }

        public override void OnKill(int timeLeft)
        {
            Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, Vector2.Zero,
                ModContent.ProjectileType<TemporalFractureProjectile>(),
                (int)(Projectile.damage * 0.4f), 0f, Projectile.owner);

            SoundEngine.PlaySound(SoundID.Item27 with { Pitch = 0.3f, Volume = 0.4f }, Projectile.Center);

            // Impact burst — 8 sparkles + bloom flash
            var flash = new BloomParticle(Projectile.Center, Vector2.Zero,
                ClairDeLunePalette.PearlFrost with { A = 0 } * 0.6f, 0.3f, 8);
            MagnumParticleHandler.SpawnParticle(flash);

            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 vel = angle.ToRotationVector2() * 3f;
                var shard = new SparkleParticle(Projectile.Center, vel,
                    ClairDeLunePalette.SoftBlue with { A = 0 } * 0.4f, 0.08f, 10);
                MagnumParticleHandler.SpawnParticle(shard);
            }
        }

        private void LoadTextures()
        {
            _softGlow ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftGlow", AssetRequestMode.ImmediateLoad);
            _softRadialBloom ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftRadialBloom", AssetRequestMode.ImmediateLoad);
            _pointBloom ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/PointBloom", AssetRequestMode.ImmediateLoad);
            _starFlare ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/StarFlare", AssetRequestMode.ImmediateLoad);
            _4ptStar ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles Asset Library/Stars/4PointedStarHard", AssetRequestMode.ImmediateLoad);
            _softCircle ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/MasksAndShapes/SoftCircle", AssetRequestMode.ImmediateLoad);
            _sparkleHard ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles Asset Library/Stars/4PointedStarHard", AssetRequestMode.ImmediateLoad);
            _glowMask ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/MasksAndShapes/SoftCircle", AssetRequestMode.ImmediateLoad);
            _gradientLUT ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/ColorGradients/ClairDeLuneGradientLUTandRAMP", AssetRequestMode.ImmediateLoad);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (Main.dedServ) return false;
            LoadTextures();

            SpriteBatch sb = Main.spriteBatch;
            Matrix matrix = Main.GameViewMatrix.TransformationMatrix;

            DrawSparkleTrail(sb, matrix);    // Layer 1: GPU SparkleTrail shader (VertexStrip)
            DrawBloomTrail(sb, matrix);      // Layer 2: Per-position velocity-stretched bloom
            DrawBloomHalo(sb, matrix);       // Layer 3: Multi-scale bloom stacking at head
            DrawCrystalBody(sb, matrix);     // Layer 4: Crystal body with CrystalShimmer shader overlay
            DrawSparkleAccents(sb, matrix);  // Layer 5: Orbiting 4-pointed stars + central twinkle
            ClairDeLuneVFXLibrary.DrawThemeAccents(sb, Projectile.Center, 0.5f, 0.3f);
            return false;
        }

        // ---- LAYER 1: SparkleTrail shader via VertexStrip (SparkleProjectileFoundation pattern) ----
        private void DrawSparkleTrail(SpriteBatch sb, Matrix matrix)
        {
            int count = Math.Min(_trailIndex, TrailLength);
            if (count < 3) return;

            // Build ordered arrays newest→oldest
            Vector2[] positions = new Vector2[count];
            float[] rotations = new float[count];
            for (int i = 0; i < count; i++)
            {
                int ringIdx = ((_trailIndex - 1 - i) % TrailLength + TrailLength) % TrailLength;
                positions[i] = _trailPositions[ringIdx];
                rotations[i] = _trailRotations[ringIdx];
            }

            _strip ??= new VertexStrip();
            _strip.PrepareStrip(positions, rotations,
                (float progress) => Color.Lerp(ClairDeLunePalette.PearlWhite with { A = 0 },
                    ClairDeLunePalette.SoftBlue with { A = 0 }, progress) * (1f - progress * 0.8f),
                (float progress) => MathHelper.Lerp(14f, 1f, progress),
                -Main.screenPosition, count, includeBacksides: true);

            // Load shader
            _sparkleTrailShader ??= ModContent.Request<Effect>(
                "MagnumOpus/Content/FoundationWeapons/SparkleProjectileFoundation/Shaders/SparkleTrailShader",
                AssetRequestMode.ImmediateLoad).Value;

            sb.End(); // End SpriteBatch for raw vertex drawing

            // Orthographic projection for VertexStrip
            var device = Main.graphics.GraphicsDevice;
            var viewProj = Matrix.CreateOrthographicOffCenter(0, device.Viewport.Width, device.Viewport.Height, 0, -1, 1)
                * Main.GameViewMatrix.TransformationMatrix;

            _sparkleTrailShader.Parameters["WorldViewProjection"]?.SetValue(
                Main.GameViewMatrix.NormalizedTransformationmatrix);
            _sparkleTrailShader.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly * 2f);
            _sparkleTrailShader.Parameters["sparkleTex"]?.SetValue(_sparkleHard.Value);
            _sparkleTrailShader.Parameters["gradientTex"]?.SetValue(_gradientLUT.Value);
            _sparkleTrailShader.Parameters["glowMaskTex"]?.SetValue(_glowMask.Value);
            _sparkleTrailShader.Parameters["coreColor"]?.SetValue(ClairDeLunePalette.PearlWhite.ToVector3());
            _sparkleTrailShader.Parameters["outerColor"]?.SetValue(ClairDeLunePalette.SoftBlue.ToVector3());
            _sparkleTrailShader.Parameters["sparkleSpeed"]?.SetValue(3.5f);
            _sparkleTrailShader.Parameters["sparkleScale"]?.SetValue(0.5f);
            _sparkleTrailShader.Parameters["glitterDensity"]?.SetValue(5.0f);
            _sparkleTrailShader.Parameters["tipFadeStart"]?.SetValue(0.6f);
            _sparkleTrailShader.Parameters["edgeSoftness"]?.SetValue(0.3f);

            _sparkleTrailShader.CurrentTechnique.Passes["SparkleTrailPass"].Apply();
            _strip.DrawTrail();
            Main.pixelShader.CurrentTechnique.Passes[0].Apply(); // Reset pixel shader

            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive,
                SamplerState.LinearClamp, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null, matrix);
        }

        // ---- LAYER 2: Bloom trail — velocity-stretched per-position (Photoviscerator pattern) ----
        private void DrawBloomTrail(SpriteBatch sb, Matrix matrix)
        {
            int count = Math.Min(_trailIndex, TrailLength);
            if (count < 2) return;

            Texture2D glow = _softGlow.Value;
            Vector2 glowOrigin = glow.Size() * 0.5f;
            float texW = glow.Width;

            for (int i = 0; i < count; i++)
            {
                int ringIdx = ((_trailIndex - 1 - i) % TrailLength + TrailLength) % TrailLength;
                Vector2 pos = _trailPositions[ringIdx] - Main.screenPosition;
                float t = (float)i / count; // 0 = head, 1 = tail
                float fade = 1f - t;
                float rot = _trailRotations[ringIdx];

                // Layer A: Outer glow — wide, dim
                float outerSize = MathHelper.Lerp(50f, 8f, t);
                Color outerCol = Color.Lerp(ClairDeLunePalette.NightMist, ClairDeLunePalette.SoftBlue, t) with { A = 0 };
                sb.Draw(glow, pos, null, outerCol * 0.2f * fade, rot, glowOrigin,
                    new Vector2(outerSize / texW * 3f, outerSize / texW), SpriteEffects.None, 0f);

                // Layer B: Core body — velocity stretched
                float bodySize = MathHelper.Lerp(30f, 4f, t);
                Color bodyCol = Color.Lerp(ClairDeLunePalette.PearlFrost, ClairDeLunePalette.SoftBlue, t) with { A = 0 };
                sb.Draw(glow, pos, null, bodyCol * 0.35f * fade, rot, glowOrigin,
                    new Vector2(bodySize / texW * 4f, bodySize / texW), SpriteEffects.None, 0f);

                // Layer C: Hot core (head 40% only)
                if (t < 0.4f)
                {
                    float coreSize = MathHelper.Lerp(16f, 2f, t / 0.4f);
                    Color coreCol = ClairDeLunePalette.WhiteHot with { A = 0 };
                    sb.Draw(glow, pos, null, coreCol * 0.5f * (1f - t / 0.4f), rot, glowOrigin,
                        new Vector2(coreSize / texW * 5f, coreSize / texW), SpriteEffects.None, 0f);
                }
            }

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null, matrix);
        }

        // ---- LAYER 3: Bloom halo at head — 3-scale stacking ----
        private void DrawBloomHalo(SpriteBatch sb, Matrix matrix)
        {
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive,
                SamplerState.LinearClamp, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null, matrix);

            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Texture2D srb = _softRadialBloom.Value;
            Texture2D pb = _pointBloom.Value;
            Vector2 srbOrigin = srb.Size() * 0.5f;
            Vector2 pbOrigin = pb.Size() * 0.5f;

            float pulse = 0.85f + 0.15f * MathF.Sin(Main.GlobalTimeWrappedHourly * 4f);

            // Outer halo — NightMist, wide
            sb.Draw(srb, drawPos, null, ClairDeLunePalette.NightMist with { A = 0 } * 0.2f * pulse,
                0f, srbOrigin, 80f / srb.Width, SpriteEffects.None, 0f);
            // Mid glow — SoftBlue
            sb.Draw(srb, drawPos, null, ClairDeLunePalette.SoftBlue with { A = 0 } * 0.4f * pulse,
                0f, srbOrigin, 45f / srb.Width, SpriteEffects.None, 0f);
            // Inner glow — PearlFrost
            sb.Draw(pb, drawPos, null, ClairDeLunePalette.PearlFrost with { A = 0 } * 0.55f,
                0f, pbOrigin, 25f / pb.Width, SpriteEffects.None, 0f);
            // White-hot core
            sb.Draw(pb, drawPos, null, ClairDeLunePalette.WhiteHot with { A = 0 } * 0.7f,
                0f, pbOrigin, 10f / pb.Width, SpriteEffects.None, 0f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null, matrix);
        }

        // ---- LAYER 4: Crystal body with CrystalShimmer shader overlay ----
        private void DrawCrystalBody(SpriteBatch sb, Matrix matrix)
        {
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            // Normal draw — bright star sprite as crystal
            Texture2D star = _4ptStar.Value;
            Vector2 starOrigin = star.Size() * 0.5f;
            float bodyScale = 20f / star.Width;
            sb.Draw(star, drawPos, null, ClairDeLunePalette.PearlWhite * 0.8f,
                Projectile.rotation + MathHelper.PiOver4, starOrigin, bodyScale, SpriteEffects.None, 0f);

            // CrystalShimmer shader overlay pass
            _crystalShimmerShader ??= ModContent.Request<Effect>(
                "MagnumOpus/Content/FoundationWeapons/SparkleProjectileFoundation/Shaders/CrystalShimmerShader",
                AssetRequestMode.ImmediateLoad).Value;

            if (_crystalShimmerShader != null && _softCircle != null)
            {
                sb.End();
                sb.Begin(SpriteSortMode.Deferred, BlendState.Additive,
                    SamplerState.LinearWrap, DepthStencilState.None,
                    RasterizerState.CullCounterClockwise, _crystalShimmerShader, matrix);

                _crystalShimmerShader.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly * 1.5f);
                _crystalShimmerShader.Parameters["facetCount"]?.SetValue(6f);
                _crystalShimmerShader.Parameters["shimmerIntensity"]?.SetValue(0.8f);
                _crystalShimmerShader.Parameters["hueShiftSpeed"]?.SetValue(0.3f);
                _crystalShimmerShader.Parameters["gradientTex"]?.SetValue(_gradientLUT.Value);
                _crystalShimmerShader.Parameters["coreColor"]?.SetValue(ClairDeLunePalette.PearlWhite.ToVector3());
                _crystalShimmerShader.Parameters["edgeColor"]?.SetValue(ClairDeLunePalette.SoftBlue.ToVector3());
                _crystalShimmerShader.CurrentTechnique.Passes[0].Apply();

                float shimmerScale = 24f / _softCircle.Value.Width;
                sb.Draw(_softCircle.Value, drawPos, null, Color.White,
                    Projectile.rotation, _softCircle.Value.Size() * 0.5f, shimmerScale, SpriteEffects.None, 0f);

                sb.End();
                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                    Main.DefaultSamplerState, DepthStencilState.None,
                    RasterizerState.CullCounterClockwise, null, matrix);
            }
        }

        // ---- LAYER 5: Orbiting sparkle accents + central twinkle ----
        private void DrawSparkleAccents(SpriteBatch sb, Matrix matrix)
        {
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive,
                SamplerState.LinearClamp, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null, matrix);

            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Texture2D star = _4ptStar.Value;
            Vector2 starOrigin = star.Size() * 0.5f;
            float time = Main.GlobalTimeWrappedHourly;

            // 4 orbiting sparkle points — cubic sin-wave flash timing
            for (int i = 0; i < 4; i++)
            {
                float orbitAngle = time * 3f + i * MathHelper.PiOver2;
                float orbitRadius = 14f;
                Vector2 sparklePos = drawPos + new Vector2(
                    MathF.Cos(orbitAngle) * orbitRadius,
                    MathF.Sin(orbitAngle) * orbitRadius);

                // Cubic sin-wave flash: pow(sin, 6) for sharp twinkle
                float flash = MathF.Pow(Math.Max(0, MathF.Sin(time * 5f + i * 1.5f)), 6f);
                float sparkleScale = 8f / star.Width * (0.3f + 0.7f * flash);
                Color sparkleCol = Color.Lerp(ClairDeLunePalette.SoftBlue, ClairDeLunePalette.PearlWhite, flash) with { A = 0 };

                sb.Draw(star, sparklePos, null, sparkleCol * (0.2f + 0.6f * flash),
                    orbitAngle, starOrigin, sparkleScale, SpriteEffects.None, 0f);
            }

            // Central twinkle — pow(sin, 8) sharp pulse
            float centralFlash = MathF.Pow(Math.Max(0, MathF.Sin(time * 4f)), 8f);
            Texture2D sfTex = _starFlare.Value;
            Vector2 sfOrigin = sfTex.Size() * 0.5f;
            float sfScale = 20f / sfTex.Width * (0.4f + 0.6f * centralFlash);
            sb.Draw(sfTex, drawPos, null, ClairDeLunePalette.PearlFrost with { A = 0 } * 0.4f * centralFlash,
                time * 0.5f, sfOrigin, sfScale, SpriteEffects.None, 0f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null, matrix);
        }
    }
}
