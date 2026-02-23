using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.Shaders;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Content.TestWeapons.SandboxWeapons.Shaders;

namespace MagnumOpus.Content.TestWeapons.SandboxWeapons
{
    /// <summary>
    /// Lingering circular flame zone spawned by NeonEnergyBall on impact.
    /// Duration: 90 frames (1.5 seconds), constant radius, repeated damage.
    ///
    /// Rendering via RadialScrollShader DualPhase technique for seamless
    /// linear noise scrolling with phase-blended animation and circular masking.
    /// Additional soft bloom layers and ambient rising particles.
    /// </summary>
    public class LingeringFlameZone : ModProjectile
    {
        #region Constants

        private const int Duration = 90;
        private const float Radius = 60f;

        #endregion

        #region State

        private int timer = 0;

        #endregion

        #region Setup

        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.None;

        public override void SetDefaults()
        {
            Projectile.width = (int)(Radius * 2);
            Projectile.height = (int)(Radius * 2);
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.MeleeNoSpeed;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.timeLeft = Duration + 5;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 20;
        }

        public override bool ShouldUpdatePosition() => false;

        #endregion

        #region Collision

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            if (timer <= 0) return false;

            Vector2 closestPoint = new Vector2(
                MathHelper.Clamp(Projectile.Center.X, targetHitbox.Left, targetHitbox.Right),
                MathHelper.Clamp(Projectile.Center.Y, targetHitbox.Top, targetHitbox.Bottom));

            return Vector2.Distance(Projectile.Center, closestPoint) <= Radius;
        }

        #endregion

        #region AI

        public override void AI()
        {
            timer++;

            Projectile.velocity = Vector2.Zero;

            float progress = (float)timer / Duration;

            // Fade alpha for last 30 frames
            float fadeAlpha = timer > Duration - 30
                ? (Duration - timer) / 30f
                : 1f;

            // Ambient rising particles
            if (timer % 3 == 0 && fadeAlpha > 0.3f)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                float dist = Main.rand.NextFloat(0.3f, 1f) * Radius;
                Vector2 particlePos = Projectile.Center + angle.ToRotationVector2() * dist;
                Vector2 particleVel = new Vector2(
                    Main.rand.NextFloat(-0.5f, 0.5f),
                    Main.rand.NextFloat(-2.5f, -1f));

                Color dustColor = TerraBladeShaderManager.GetPaletteColor(Main.rand.NextFloat(0.3f, 0.7f));
                Dust d = Dust.NewDustPerfect(particlePos, DustID.GreenTorch, particleVel, 0, dustColor, 1.1f);
                d.noGravity = true;
                d.fadeIn = 1.2f;
            }

            // GlowSpark particles rising from flame
            if (timer % 6 == 0 && fadeAlpha > 0.3f)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                float dist = Main.rand.NextFloat(0.2f, 0.8f) * Radius;
                Vector2 sparkPos = Projectile.Center + angle.ToRotationVector2() * dist;
                Vector2 sparkVel = new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-3f, -1f));
                Color sparkColor = TerraBladeShaderManager.GetPaletteColor(Main.rand.NextFloat(0.4f, 0.8f));
                var spark = new GlowSparkParticle(sparkPos, sparkVel, sparkColor, 0.18f, 20);
                MagnumParticleHandler.SpawnParticle(spark);
            }

            // Dynamic lighting
            Color light = TerraBladeShaderManager.GetPaletteColor(0.5f);
            Lighting.AddLight(Projectile.Center, light.ToVector3() * fadeAlpha * 0.8f);

            if (timer >= Duration)
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

        public override bool PreDraw(ref Color lightColor)
        {
            if (timer <= 0) return false;

            SpriteBatch sb = Main.spriteBatch;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float time = Main.GlobalTimeWrappedHourly;

            float fadeAlpha = timer > Duration - 30
                ? (Duration - timer) / 30f
                : 1f;
            fadeAlpha = MathHelper.Clamp(fadeAlpha, 0f, 1f);

            // --- Noise flame via RadialScrollShader (linear UV scrolling with circle mask) ---
            sb.End();

            DrawRadialNoiseFlame(sb, drawPos, Radius, fadeAlpha, time);

            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null,
                Main.GameViewMatrix.TransformationMatrix);

            // Bloom layers
            Texture2D bloomTex = SafeRequest("MagnumOpus/Assets/VFX/Blooms/Perfect Soft Color Bloom");
            if (bloomTex == null)
                bloomTex = Terraria.GameContent.TextureAssets.Extra[98].Value;

            Vector2 bloomOrigin = bloomTex.Size() * 0.5f;
            float pulse = 1f + MathF.Sin(time * 6f) * 0.08f;

            Color outerBloom = TerraBladeShaderManager.GetPaletteColor(0.3f);
            float bloomScale = Radius / 40f;
            sb.Draw(bloomTex, drawPos, null, outerBloom * 0.25f * fadeAlpha,
                0f, bloomOrigin, bloomScale * 1.2f * pulse, SpriteEffects.None, 0f);

            Color midBloom = TerraBladeShaderManager.GetPaletteColor(0.5f);
            sb.Draw(bloomTex, drawPos, null, midBloom * 0.35f * fadeAlpha,
                0f, bloomOrigin, bloomScale * 0.8f * pulse, SpriteEffects.None, 0f);

            Color coreBloom = Color.White;
            sb.Draw(bloomTex, drawPos, null, coreBloom * 0.30f * fadeAlpha,
                0f, bloomOrigin, bloomScale * 0.3f * pulse, SpriteEffects.None, 0f);

            // Restore
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                DepthStencilState.None, RasterizerState.CullNone, null,
                Main.GameViewMatrix.TransformationMatrix);

            return false;
        }

        /// <summary>
        /// Draws the lingering flame body using RadialScrollShader PolarScroll technique.
        /// CircularMask is drawn as the shape texture on s0 (its alpha masks the output).
        /// Noise is set via causticTexture/distortTexture shader parameters.
        /// Effect is passed to sb.Begin() (VFX+ pattern — NOT Immediate mode).
        /// </summary>
        private void DrawRadialNoiseFlame(SpriteBatch sb, Vector2 drawPos, float radius, float fadeAlpha, float time)
        {
            Effect radialShader = ShaderLoader.RadialScroll;
            Texture2D noiseTex = ShaderLoader.GetNoiseTexture("UniversalRadialFlowNoise");
            Texture2D distortNoise = ShaderLoader.GetNoiseTexture("TileableFBMNoise");

            // CircularMask is the shape texture — its alpha masks the noise output
            Texture2D maskTex = SafeRequest("MagnumOpus/Assets/Particles/CircularMask");
            if (maskTex == null) return;

            Vector2 maskOrigin = maskTex.Size() * 0.5f;
            float maskScale = radius * 2.4f / Math.Max(maskTex.Width, maskTex.Height);

            if (radialShader != null && noiseTex != null)
            {
                // VFX+ pattern: set noise textures via parameters, draw shape on s0
                try
                {
                    radialShader.Parameters["causticTexture"]?.SetValue(noiseTex);
                    radialShader.Parameters["distortTexture"]?.SetValue(distortNoise ?? noiseTex);
                    radialShader.Parameters["uTime"]?.SetValue(time);
                    radialShader.Parameters["flowSpeed"]?.SetValue(-0.8f);
                    radialShader.Parameters["distortStrength"]?.SetValue(0.12f);
                    radialShader.Parameters["colorIntensity"]?.SetValue(1.5f);
                    radialShader.Parameters["vignetteSize"]?.SetValue(0.38f);
                    radialShader.Parameters["vignetteBlend"]?.SetValue(0.15f);
                    radialShader.Parameters["uColor"]?.SetValue(TerraBladeShaderManager.GetPaletteColor(0.5f).ToVector3());
                    radialShader.Parameters["uSecondaryColor"]?.SetValue(TerraBladeShaderManager.GetPaletteColor(0.8f).ToVector3());
                    radialShader.CurrentTechnique = radialShader.Techniques["PolarScroll"];

                    // Pass Effect to sb.Begin (VFX+ pattern — Deferred, not Immediate)
                    sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState,
                        DepthStencilState.None, RasterizerState.CullNone, radialShader,
                        Main.GameViewMatrix.TransformationMatrix);

                    // Draw CircularMask — shader reads its alpha as shape mask,
                    // scrolls noise through it in polar coordinates
                    sb.Draw(maskTex, drawPos, null, Color.White * fadeAlpha,
                        0f, maskOrigin, maskScale, SpriteEffects.None, 0f);

                    sb.End();

                    // Second pass: PolarMultiLayer for volumetric depth
                    radialShader.Parameters["uTime"]?.SetValue(time * 0.7f);
                    radialShader.Parameters["flowSpeed"]?.SetValue(0.5f);
                    radialShader.Parameters["colorIntensity"]?.SetValue(1.0f);
                    radialShader.Parameters["vignetteSize"]?.SetValue(0.35f);
                    radialShader.Parameters["vignetteBlend"]?.SetValue(0.18f);
                    radialShader.CurrentTechnique = radialShader.Techniques["PolarMultiLayer"];

                    sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState,
                        DepthStencilState.None, RasterizerState.CullNone, radialShader,
                        Main.GameViewMatrix.TransformationMatrix);

                    sb.Draw(maskTex, drawPos, null, Color.White * fadeAlpha * 0.6f,
                        0f, maskOrigin, maskScale * 0.9f, SpriteEffects.None, 0f);

                    sb.End();
                }
                catch
                {
                    try { sb.End(); } catch { }
                }
            }
            else
            {
                // Fallback: vibrant layered flame zone using shaped VFX textures
                sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                    DepthStencilState.None, RasterizerState.CullNone, null,
                    Main.GameViewMatrix.TransformationMatrix);

                float targetSize = radius * 2f;

                // Perfect Soft Color Bloom — rotating base flame shape (petals create flame-like rotation)
                Texture2D bloomBase = SafeRequest("MagnumOpus/Assets/VFX/Blooms/Perfect Soft Color Bloom");
                if (bloomBase == null) bloomBase = Terraria.GameContent.TextureAssets.Extra[98].Value;
                Vector2 bbOrigin = bloomBase.Size() * 0.5f;
                float bbScale = targetSize * 1.0f / Math.Max(bloomBase.Width, bloomBase.Height);

                Color outerFlame = TerraBladeShaderManager.GetPaletteColor(0.4f);
                sb.Draw(bloomBase, drawPos, null, outerFlame * 0.65f * fadeAlpha,
                    time * 0.3f, bbOrigin, bbScale * 1.2f, SpriteEffects.None, 0f);

                Color midFlame = TerraBladeShaderManager.GetPaletteColor(0.6f);
                sb.Draw(bloomBase, drawPos, null, midFlame * 0.55f * fadeAlpha,
                    -time * 0.5f, bbOrigin, bbScale * 0.85f, SpriteEffects.None, 0f);

                // Radial God Rays — rotating energy detail overlay
                Texture2D godRaysTex = SafeRequest("MagnumOpus/Assets/VFX/LightRays/Radial God Rays Full Circle");
                if (godRaysTex != null)
                {
                    Vector2 grOrigin = godRaysTex.Size() * 0.5f;
                    float grScale = targetSize * 1.2f / Math.Max(godRaysTex.Width, godRaysTex.Height);
                    Color grColor = TerraBladeShaderManager.GetPaletteColor(0.5f);
                    sb.Draw(godRaysTex, drawPos, null, grColor * 0.5f * fadeAlpha,
                        -time * 0.4f, grOrigin, grScale, SpriteEffects.None, 0f);
                }

                // Energy Flare — pulsing center
                Texture2D flareTex = SafeRequest("MagnumOpus/Assets/Particles/EnergyFlare");
                if (flareTex != null)
                {
                    Vector2 flOrigin = flareTex.Size() * 0.5f;
                    float flScale = targetSize * 0.6f / Math.Max(flareTex.Width, flareTex.Height);
                    Color flColor = TerraBladeShaderManager.GetPaletteColor(0.7f);
                    float flamePulse = 1f + MathF.Sin(time * 6f) * 0.15f;
                    sb.Draw(flareTex, drawPos, null, flColor * 0.7f * fadeAlpha,
                        time * 0.8f, flOrigin, flScale * flamePulse, SpriteEffects.None, 0f);
                }

                // White-hot center
                sb.Draw(bloomBase, drawPos, null, Color.White * 0.4f * fadeAlpha,
                    0f, bbOrigin, bbScale * 0.25f, SpriteEffects.None, 0f);

                sb.End();
            }
        }

        #endregion

        #region Hit Effects

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            for (int i = 0; i < 3; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(3f, 3f);
                Color dustColor = TerraBladeShaderManager.GetPaletteColor(Main.rand.NextFloat(0.3f, 0.7f));
                Dust d = Dust.NewDustPerfect(target.Center, DustID.GreenTorch, vel, 0, dustColor, 0.9f);
                d.noGravity = true;
                d.fadeIn = 1.2f;
            }
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
