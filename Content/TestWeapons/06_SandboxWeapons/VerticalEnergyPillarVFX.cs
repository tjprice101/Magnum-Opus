using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.Shaders;
using MagnumOpus.Content.TestWeapons.SandboxWeapons.Shaders;

namespace MagnumOpus.Content.TestWeapons.SandboxWeapons
{
    /// <summary>
    /// Vertical energy pillar VFX spawned from explosions (LightShardExplosion, CosmicImpactZone).
    /// Exoblade-style vertical stretch effect.
    ///
    /// Animation: stretch vertically (0-10 frames), hold (10-20 frames), fade (20-30 frames).
    /// Rendered via vertex quad with NatureTechnique shader + bloom layers.
    /// </summary>
    public class VerticalEnergyPillarVFX : ModProjectile
    {
        #region Constants

        private const int Lifetime = 30;
        private const float MaxHeight = 400f;
        private const float PillarWidth = 40f;

        #endregion

        #region State

        private int timer = 0;

        // Quad vertex mesh
        private VertexPositionColorTexture[] _quadVerts = new VertexPositionColorTexture[4];
        private static readonly short[] _quadIndices = { 0, 1, 2, 2, 1, 3 };

        #endregion

        #region Setup

        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.None;

        public override void SetDefaults()
        {
            Projectile.width = 4;
            Projectile.height = 4;
            Projectile.friendly = false;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = Lifetime + 2;
            Projectile.ignoreWater = true;
        }

        public override bool? CanDamage() => false;
        public override bool ShouldUpdatePosition() => false;

        #endregion

        #region AI

        public override void AI()
        {
            timer++;
            Projectile.velocity = Vector2.Zero;

            float fadeAlpha = timer <= 20 ? 1f : (Lifetime - timer) / 10f;
            Color light = TerraBladeShaderManager.GetPaletteColor(0.5f);
            Lighting.AddLight(Projectile.Center, light.ToVector3() * MathHelper.Clamp(fadeAlpha, 0f, 1f) * 0.8f);

            if (timer >= Lifetime)
            {
                Projectile.Kill();
            }
        }

        #endregion

        #region Rendering

        private static Texture2D SafeRequest(string path)
        {
            try
            {
                if (ModContent.HasAsset(path))
                    return ModContent.Request<Texture2D>(path).Value;
            }
            catch { }
            return null;
        }

        private int BuildVerticalQuad(Vector2 centerScreen, float width, float height, float alpha, float time)
        {
            float halfW = width * 0.5f;
            float halfH = height * 0.5f;

            Color topColor = TerraBladeShaderManager.GetPaletteColor(0.7f) * alpha * 0f; // fade at edges
            Color midColor = TerraBladeShaderManager.GetPaletteColor(0.5f) * alpha;

            // Top-left, Top-right (faded)
            _quadVerts[0] = new VertexPositionColorTexture(
                new Vector3(centerScreen.X - halfW, centerScreen.Y - halfH, 0),
                topColor, new Vector2(0f + time * 0.5f, 0f));
            _quadVerts[1] = new VertexPositionColorTexture(
                new Vector3(centerScreen.X + halfW, centerScreen.Y - halfH, 0),
                topColor, new Vector2(1f + time * 0.5f, 0f));

            // Bottom-left, Bottom-right (faded)
            _quadVerts[2] = new VertexPositionColorTexture(
                new Vector3(centerScreen.X - halfW, centerScreen.Y + halfH, 0),
                topColor, new Vector2(0f + time * 0.5f, 1f));
            _quadVerts[3] = new VertexPositionColorTexture(
                new Vector3(centerScreen.X + halfW, centerScreen.Y + halfH, 0),
                topColor, new Vector2(1f + time * 0.5f, 1f));

            // Override center vertices to be brighter — use center alpha for mid rows
            // Since we only have 4 verts (a quad), the center brightness comes from
            // the bloom layers drawn separately

            return 4;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (timer <= 0) return false;

            SpriteBatch sb = Main.spriteBatch;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float time = Main.GlobalTimeWrappedHourly;

            // Animation: stretch up (0-10), hold (10-20), fade (20-30)
            float stretchProgress = MathHelper.Clamp(timer / 10f, 0f, 1f);
            float fadeAlpha = timer <= 20 ? 1f : MathHelper.Clamp((Lifetime - timer) / 10f, 0f, 1f);

            // Ease the stretch (SineOut-like)
            float easedStretch = 1f - (1f - stretchProgress) * (1f - stretchProgress);
            float currentHeight = MaxHeight * easedStretch;
            float currentWidth = PillarWidth * MathHelper.Clamp(stretchProgress * 3f, 0f, 1f);

            var device = Main.instance.GraphicsDevice;
            Effect trailShader = ShaderLoader.Trail;

            // --- Vertex quad with shader ---
            try { sb.End(); } catch { }

            try
            {
                Texture2D noise = ShaderLoader.GetNoiseTexture("SparklyNoiseTexture");
                if (noise != null)
                {
                    device.Textures[1] = noise;
                    device.SamplerStates[1] = SamplerState.LinearWrap;
                }

                device.BlendState = BlendState.Additive;
                device.DepthStencilState = DepthStencilState.None;
                device.RasterizerState = RasterizerState.CullNone;
                device.SamplerStates[0] = SamplerState.LinearWrap;
                device.Textures[0] = Terraria.GameContent.TextureAssets.MagicPixel.Value;

                if (trailShader != null)
                {
                    trailShader.CurrentTechnique = trailShader.Techniques["NatureTechnique"];
                    trailShader.Parameters["uTime"]?.SetValue(time);
                    trailShader.Parameters["uColor"]?.SetValue(TerraBladeShaderManager.EnergyGreen.ToVector3());
                    trailShader.Parameters["uSecondaryColor"]?.SetValue(TerraBladeShaderManager.BrightCyan.ToVector3());
                    trailShader.Parameters["uOpacity"]?.SetValue(fadeAlpha);
                    trailShader.Parameters["uProgress"]?.SetValue(0f);
                    trailShader.Parameters["uOverbrightMult"]?.SetValue(3.5f);
                    trailShader.Parameters["uGlowThreshold"]?.SetValue(0.4f);
                    trailShader.Parameters["uGlowIntensity"]?.SetValue(2.0f);
                    trailShader.Parameters["uIntensity"]?.SetValue(1.5f);
                    trailShader.Parameters["uHasSecondaryTex"]?.SetValue(noise != null ? 1f : 0f);
                    trailShader.Parameters["uSecondaryTexScale"]?.SetValue(1.2f);
                    trailShader.Parameters["uSecondaryTexScroll"]?.SetValue(1.0f);

                    int vertCount = BuildVerticalQuad(drawPos, currentWidth, currentHeight, fadeAlpha, time);

                    foreach (var p in trailShader.CurrentTechnique.Passes)
                    {
                        p.Apply();
                        device.DrawUserIndexedPrimitives(
                            PrimitiveType.TriangleList,
                            _quadVerts, 0, vertCount,
                            _quadIndices, 0, 2);
                    }
                }
            }
            finally
            {
                device.Textures[1] = null;
            }

            // --- Bloom layers (SpriteBatch) ---
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null,
                Main.GameViewMatrix.TransformationMatrix);

            // Light shaft texture drawn vertically
            Texture2D shaftTex = SafeRequest("MagnumOpus/Assets/VFX/LightRays/Single Directional Light Shaft");
            if (shaftTex != null)
            {
                Vector2 shaftOrigin = new Vector2(shaftTex.Width * 0.5f, shaftTex.Height * 0.5f);
                float shaftScaleX = currentWidth / shaftTex.Width * 1.5f;
                float shaftScaleY = currentHeight / shaftTex.Height;

                Color shaftColor = TerraBladeShaderManager.GetPaletteColor(0.5f);
                sb.Draw(shaftTex, drawPos, null, shaftColor * 0.4f * fadeAlpha,
                    0f, shaftOrigin, new Vector2(shaftScaleX, shaftScaleY), SpriteEffects.None, 0f);
                sb.Draw(shaftTex, drawPos, null, Color.White * 0.3f * fadeAlpha,
                    0f, shaftOrigin, new Vector2(shaftScaleX * 0.4f, shaftScaleY * 0.9f), SpriteEffects.None, 0f);
            }

            // Center bloom
            Texture2D bloomTex = SafeRequest("MagnumOpus/Assets/VFX/Blooms/Perfect Soft Color Bloom");
            if (bloomTex == null)
                bloomTex = Terraria.GameContent.TextureAssets.Extra[98].Value;

            Vector2 bloomOrigin = bloomTex.Size() * 0.5f;
            float pulse = 1f + MathF.Sin(time * 8f) * 0.1f;

            Color coreColor = TerraBladeShaderManager.GetPaletteColor(0.6f);
            sb.Draw(bloomTex, drawPos, null, coreColor * 0.5f * fadeAlpha,
                0f, bloomOrigin, 0.3f * pulse * stretchProgress, SpriteEffects.None, 0f);
            sb.Draw(bloomTex, drawPos, null, Color.White * 0.4f * fadeAlpha,
                0f, bloomOrigin, 0.12f * pulse * stretchProgress, SpriteEffects.None, 0f);

            // Restore
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                DepthStencilState.None, RasterizerState.CullNone, null,
                Main.GameViewMatrix.TransformationMatrix);

            return false;
        }

        #endregion
    }
}
