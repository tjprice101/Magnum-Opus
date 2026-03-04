using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.Shaders;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.ClairDeLune.Weapons.OrreryOfDreams.Projectiles
{
    /// <summary>
    /// Inner Sphere Bolt — rapid crystal bolt fired by the Inner Dream Sphere.
    /// Fast, small, direct-fire with short crystal trail.
    /// 3 render passes: (1) CelestialOrbit CelestialOrbitCore for crystal body,
    /// (2) PearlShimmer shimmer overlay, (3) Multi-scale bloom + trail afterimages.
    /// </summary>
    public class InnerSphereBoltProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Textures/InvisibleProjectile";

        private const int TrailLength = 8;
        private Vector2[] _trailPositions = new Vector2[TrailLength];
        private int _trailIndex;

        // --- Texture + shader caching ---
        private static Effect _celestialOrbitShader;
        private static Effect _pearlGlowShader;
        private static Asset<Texture2D> _softCircle;
        private static Asset<Texture2D> _softRadialBloom;
        private static Asset<Texture2D> _pointBloom;

        public override void SetDefaults()
        {
            Projectile.width = 6;
            Projectile.height = 6;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 120;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.extraUpdates = 1;
            Projectile.alpha = 255;
        }

        public override void AI()
        {
            // Fade in
            if (Projectile.alpha > 0) Projectile.alpha -= 40;

            // Record trail
            _trailPositions[_trailIndex % TrailLength] = Projectile.Center;
            _trailIndex++;

            Projectile.rotation = Projectile.velocity.ToRotation();

            // Sparkle particles
            if (Main.rand.NextBool(4))
            {
                var sparkle = new SparkleParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(3, 3),
                    Main.rand.NextVector2Circular(0.3f, 0.3f),
                    ClairDeLunePalette.PearlWhite with { A = 0 } * 0.5f,
                    0.08f, 12);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }

            Lighting.AddLight(Projectile.Center, ClairDeLunePalette.PearlWhite.ToVector3() * 0.3f);
        }

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 4; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(2f, 2f);
                var spark = new GlowSparkParticle(Projectile.Center, vel,
                    ClairDeLunePalette.PearlBlue with { A = 0 } * 0.6f, 0.1f, 15);
                MagnumParticleHandler.SpawnParticle(spark);
            }
        }

        private void LoadTextures()
        {
            _softCircle ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/MasksAndShapes/SoftCircle", AssetRequestMode.ImmediateLoad);
            _softRadialBloom ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftRadialBloom", AssetRequestMode.ImmediateLoad);
            _pointBloom ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/PointBloom", AssetRequestMode.ImmediateLoad);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (Main.dedServ) return false;
            LoadTextures();

            SpriteBatch sb = Main.spriteBatch;
            Matrix matrix = Main.GameViewMatrix.TransformationMatrix;

            DrawCelestialBody(sb, matrix);    // Pass 1: CelestialOrbitCore crystal body
            DrawPearlShimmer(sb, matrix);     // Pass 2: PearlShimmer shimmer overlay
            DrawBloomAndTrail(sb, matrix);    // Pass 3: Bloom trail + core
            return false;
        }

        // ---- PASS 1: CelestialOrbit CelestialOrbitCore crystal body ----
        private void DrawCelestialBody(SpriteBatch sb, Matrix matrix)
        {
            _celestialOrbitShader ??= ShaderLoader.CelestialOrbit;
            if (_celestialOrbitShader == null) return;

            sb.End();

            _celestialOrbitShader.Parameters["uColor"]?.SetValue(ClairDeLunePalette.PearlWhite.ToVector4());
            _celestialOrbitShader.Parameters["uSecondaryColor"]?.SetValue(ClairDeLunePalette.PearlBlue.ToVector4());
            _celestialOrbitShader.Parameters["uOpacity"]?.SetValue(0.4f);
            _celestialOrbitShader.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly);
            _celestialOrbitShader.Parameters["uIntensity"]?.SetValue(1f);
            _celestialOrbitShader.Parameters["uOverbrightMult"]?.SetValue(1.0f);
            _celestialOrbitShader.Parameters["uScrollSpeed"]?.SetValue(3f);
            _celestialOrbitShader.Parameters["uDistortionAmt"]?.SetValue(0.015f);
            _celestialOrbitShader.Parameters["uHasSecondaryTex"]?.SetValue(false);

            _celestialOrbitShader.CurrentTechnique = _celestialOrbitShader.Techniques["CelestialOrbitCore"];

            sb.Begin(SpriteSortMode.Immediate, BlendState.Additive,
                SamplerState.LinearWrap, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, _celestialOrbitShader, matrix);

            Texture2D sc = _softCircle.Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float bodyScale = 10f / sc.Width;
            sb.Draw(sc, drawPos, null, Color.White, Projectile.rotation,
                sc.Size() * 0.5f, bodyScale, SpriteEffects.None, 0f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive,
                SamplerState.LinearClamp, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null, matrix);
        }

        // ---- PASS 2: PearlShimmer shimmer overlay ----
        private void DrawPearlShimmer(SpriteBatch sb, Matrix matrix)
        {
            _pearlGlowShader ??= ShaderLoader.ClairDeLunePearlGlow;
            if (_pearlGlowShader == null) return;

            sb.End();

            _pearlGlowShader.Parameters["uColor"]?.SetValue(ClairDeLunePalette.PearlFrost.ToVector4());
            _pearlGlowShader.Parameters["uSecondaryColor"]?.SetValue(ClairDeLunePalette.SoftBlue.ToVector4());
            _pearlGlowShader.Parameters["uOpacity"]?.SetValue(0.25f);
            _pearlGlowShader.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly);
            _pearlGlowShader.Parameters["uIntensity"]?.SetValue(0.8f);
            _pearlGlowShader.Parameters["uOverbrightMult"]?.SetValue(1.0f);
            _pearlGlowShader.Parameters["uScrollSpeed"]?.SetValue(4f);
            _pearlGlowShader.Parameters["uDistortionAmt"]?.SetValue(0.01f);
            _pearlGlowShader.Parameters["uHasSecondaryTex"]?.SetValue(false);

            _pearlGlowShader.CurrentTechnique = _pearlGlowShader.Techniques["PearlShimmer"];

            sb.Begin(SpriteSortMode.Immediate, BlendState.Additive,
                SamplerState.LinearWrap, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, _pearlGlowShader, matrix);

            Texture2D sc = _softCircle.Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float shimmerScale = 8f / sc.Width;
            sb.Draw(sc, drawPos, null, Color.White, 0f,
                sc.Size() * 0.5f, shimmerScale, SpriteEffects.None, 0f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive,
                SamplerState.LinearClamp, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null, matrix);
        }

        // ---- PASS 3: Bloom trail afterimages + core stacking ----
        private void DrawBloomAndTrail(SpriteBatch sb, Matrix matrix)
        {
            Texture2D srb = _softRadialBloom.Value;
            Texture2D pb = _pointBloom.Value;
            Vector2 srbOrigin = srb.Size() * 0.5f;
            Vector2 pbOrigin = pb.Size() * 0.5f;

            // Trail afterimages
            int count = Math.Min(_trailIndex, TrailLength);
            for (int i = 0; i < count; i++)
            {
                int idx = ((_trailIndex - 1 - i) % TrailLength + TrailLength) % TrailLength;
                float progress = i / (float)count;
                float alpha = (1f - progress) * 0.3f;
                float scale = (1f - progress) * 6f / srb.Width;

                Vector2 pos = _trailPositions[idx] - Main.screenPosition;
                Color trailColor = Color.Lerp(ClairDeLunePalette.PearlWhite, ClairDeLunePalette.SoftBlue, progress) with { A = 0 };
                sb.Draw(srb, pos, null, trailColor * alpha, 0f, srbOrigin, scale, SpriteEffects.None, 0f);
            }

            // Core bloom layers
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            // Outer haze
            sb.Draw(srb, drawPos, null,
                ClairDeLunePalette.NightMist with { A = 0 } * 0.15f, 0f, srbOrigin,
                20f / srb.Width, SpriteEffects.None, 0f);

            // Mid pearl glow
            sb.Draw(srb, drawPos, null,
                ClairDeLunePalette.PearlBlue with { A = 0 } * 0.25f, 0f, srbOrigin,
                12f / srb.Width, SpriteEffects.None, 0f);

            // Hot core
            sb.Draw(pb, drawPos, null,
                ClairDeLunePalette.WhiteHot with { A = 0 } * 0.3f, 0f, pbOrigin,
                5f / pb.Width, SpriteEffects.None, 0f);

            // Restore AlphaBlend
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null, matrix);
        }
    }
}
