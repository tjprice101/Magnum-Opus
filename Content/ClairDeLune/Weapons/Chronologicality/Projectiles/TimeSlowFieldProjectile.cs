using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.Shaders;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.ClairDeLune.Weapons.Chronologicality.Projectiles
{
    /// <summary>
    /// Time Slow Field  EPersistent AoE zone left by Chronologicality's Hour Hand hits.
    /// Slows enemies within range for 3 seconds with clockwork VFX.
    /// 3 render passes: (1) RadialNoiseMaskShader frozen time zone,
    /// (2) ClairDeLuneMoonlit.fx MoonlitGlow frost overlay, (3) Multi-scale bloom layers.
    /// </summary>
    public class TimeSlowFieldProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Textures/InvisibleProjectile";

        private const int Duration = 180;
        private const float Radius = 48f;
        private const int FadeOutFrames = 30;
        private float _seed;

        // --- Shader + texture caching ---
        private static Effect _radialNoiseShader;
        private static Effect _moonlitShader;
        private static Asset<Texture2D> _softCircle;
        private static Asset<Texture2D> _softRadialBloom;
        private static Asset<Texture2D> _noiseTex;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.DrawScreenCheckFluff[Projectile.type] = 400;
        }

        public override void SetDefaults()
        {
            Projectile.width = 96;
            Projectile.height = 96;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = Duration;
            Projectile.ignoreWater = true;
        }

        public override void AI()
        {
            if (_seed == 0f) _seed = Main.rand.NextFloat(100f);

            float fadeOut = Projectile.timeLeft < FadeOutFrames ? Projectile.timeLeft / (float)FadeOutFrames : 1f;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && !npc.friendly && npc.Distance(Projectile.Center) < Radius)
                    npc.AddBuff(BuffID.Slow, 10);
            }

            if (Main.GameUpdateCount % 4 == 0)
            {
                float angle = Main.GameUpdateCount * 0.04f;
                Vector2 edgePos = Projectile.Center + angle.ToRotationVector2() * Radius * 0.8f;
                float progress = 1f - (float)Projectile.timeLeft / Duration;
                Color zoneColor = ClairDeLunePalette.GetClockworkGradient(progress) with { A = 0 } * fadeOut * 0.5f;
                var glow = new GenericGlowParticle(edgePos, Vector2.Zero, zoneColor, 0.2f, 8, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }

            if (Main.GameUpdateCount % 10 == 0)
            {
                float tickAngle = Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 tickPos = Projectile.Center + tickAngle.ToRotationVector2() * Radius * 0.9f;
                var tickSpark = new SparkleParticle(tickPos, Vector2.Zero,
                    ClairDeLunePalette.ClockworkBrass with { A = 0 } * fadeOut * 0.4f, 0.15f, 10);
                MagnumParticleHandler.SpawnParticle(tickSpark);
            }

            Lighting.AddLight(Projectile.Center, ClairDeLunePalette.SoftBlue.ToVector3() * 0.3f * fadeOut);
        }

        private void LoadTextures()
        {
            _softCircle ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/MasksAndShapes/SoftCircle", AssetRequestMode.ImmediateLoad);
            _softRadialBloom ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftRadialBloom", AssetRequestMode.ImmediateLoad);
            _noiseTex ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/NoiseTextures/VornoiEdgeNoise", AssetRequestMode.ImmediateLoad);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (Main.dedServ) return false;
            LoadTextures();

            SpriteBatch sb = Main.spriteBatch;
            try
            {
            Matrix matrix = Main.GameViewMatrix.TransformationMatrix;

            float fadeOut = Projectile.timeLeft < FadeOutFrames ? Projectile.timeLeft / (float)FadeOutFrames : 1f;
            float fadeIn = MathHelper.Clamp((Duration - Projectile.timeLeft) / 15f, 0f, 1f);
            float alpha = fadeOut * fadeIn;

            DrawRadialNoiseZone(sb, matrix, alpha);   // Pass 1: RadialNoiseMask frozen time
            DrawMoonlitOverlay(sb, matrix, alpha);    // Pass 2: MoonlitGlow frost overlay
            DrawBloomLayers(sb, matrix, alpha);       // Pass 3: Multi-scale bloom
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

        // ---- PASS 1: RadialNoiseMaskShader frozen time zone ----
        private void DrawRadialNoiseZone(SpriteBatch sb, Matrix matrix, float alpha)
        {
            _radialNoiseShader ??= ModContent.Request<Effect>(
                "MagnumOpus/Content/FoundationWeapons/MaskFoundation/Shaders/RadialNoiseMaskShader",
                AssetRequestMode.ImmediateLoad).Value;
            if (_radialNoiseShader == null) return;

            sb.End();

            float time = (float)Main.timeForVisualEffects * 0.005f + _seed;
            _radialNoiseShader.Parameters["uTime"]?.SetValue(time);
            _radialNoiseShader.Parameters["scrollSpeed"]?.SetValue(0.08f);
            _radialNoiseShader.Parameters["rotationSpeed"]?.SetValue(0.03f);
            _radialNoiseShader.Parameters["circleRadius"]?.SetValue(0.38f);
            _radialNoiseShader.Parameters["edgeSoftness"]?.SetValue(0.10f);
            _radialNoiseShader.Parameters["intensity"]?.SetValue(1.4f * alpha);
            _radialNoiseShader.Parameters["primaryColor"]?.SetValue(ClairDeLunePalette.NightMist.ToVector3());
            _radialNoiseShader.Parameters["coreColor"]?.SetValue(ClairDeLunePalette.SoftBlue.ToVector3());

            Main.graphics.GraphicsDevice.Textures[1] = _noiseTex.Value;
            _radialNoiseShader.CurrentTechnique = _radialNoiseShader.Techniques["Technique1"];

            sb.Begin(SpriteSortMode.Immediate, MagnumBlendStates.ShaderAdditive,
                SamplerState.LinearWrap, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, _radialNoiseShader, matrix);

            Texture2D sc = _softCircle.Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float orbScale = Radius * 2.5f / sc.Width;
            sb.Draw(sc, drawPos, null, Color.White * alpha, 0f, sc.Size() * 0.5f, orbScale, SpriteEffects.None, 0f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                SamplerState.LinearClamp, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null, matrix);
        }

        // ---- PASS 2: ClairDeLuneMoonlit.fx MoonlitGlow frost overlay ----
        private void DrawMoonlitOverlay(SpriteBatch sb, Matrix matrix, float alpha)
        {
            _moonlitShader ??= ShaderLoader.ClairDeLuneMoonlit;
            if (_moonlitShader == null) return;

            sb.End();

            _moonlitShader.Parameters["uColor"]?.SetValue(ClairDeLunePalette.SoftBlue.ToVector4());
            _moonlitShader.Parameters["uSecondaryColor"]?.SetValue(ClairDeLunePalette.PearlBlue.ToVector4());
            _moonlitShader.Parameters["uOpacity"]?.SetValue(0.2f * alpha);
            _moonlitShader.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly);
            _moonlitShader.Parameters["uIntensity"]?.SetValue(0.8f);
            _moonlitShader.Parameters["uOverbrightMult"]?.SetValue(1f);
            _moonlitShader.Parameters["uScrollSpeed"]?.SetValue(0.5f);
            _moonlitShader.Parameters["uDistortionAmt"]?.SetValue(0.01f);
            _moonlitShader.Parameters["uHasSecondaryTex"]?.SetValue(false);

            _moonlitShader.CurrentTechnique = _moonlitShader.Techniques["MoonlitGlow"];

            sb.Begin(SpriteSortMode.Immediate, MagnumBlendStates.ShaderAdditive,
                SamplerState.LinearWrap, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, _moonlitShader, matrix);

            Texture2D sc = _softCircle.Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float overlayScale = Radius * 3f / sc.Width;
            sb.Draw(sc, drawPos, null, Color.White, 0f, sc.Size() * 0.5f, overlayScale, SpriteEffects.None, 0f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                SamplerState.LinearClamp, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null, matrix);
        }

        // ---- PASS 3: Multi-scale bloom layers ----
        private void DrawBloomLayers(SpriteBatch sb, Matrix matrix, float alpha)
        {
            Vector2 pos = Projectile.Center - Main.screenPosition;
            Texture2D srb = _softRadialBloom.Value;
            float rotation = Main.GameUpdateCount * 0.01f;

            // NightMist outer glow
            sb.Draw(srb, pos, null,
                ClairDeLunePalette.NightMist with { A = 0 } * 0.15f * alpha, rotation, srb.Size() * 0.5f,
                Radius * 2f / srb.Width, SpriteEffects.None, 0f);

            // SoftBlue mid field
            sb.Draw(srb, pos, null,
                ClairDeLunePalette.SoftBlue with { A = 0 } * 0.12f * alpha, -rotation * 0.7f, srb.Size() * 0.5f,
                Radius * 1.2f / srb.Width, SpriteEffects.None, 0f);

            // PearlBlue inner core
            sb.Draw(srb, pos, null,
                ClairDeLunePalette.PearlBlue with { A = 0 } * 0.08f * alpha, 0f, srb.Size() * 0.5f,
                Radius * 0.5f / srb.Width, SpriteEffects.None, 0f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null, matrix);
        }
    }
}
