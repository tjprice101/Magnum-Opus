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
    /// Rendering via disc vertex mesh (center + 16 ring vertices) with
    /// NatureTechnique shader and NoiseSmoke texture for UV-scrolling effect.
    /// Additional soft bloom layers and ambient rising particles.
    /// </summary>
    public class LingeringFlameZone : ModProjectile
    {
        #region Constants

        private const int Duration = 90;
        private const float Radius = 60f;
        private const int RingSegments = 16;

        #endregion

        #region State

        private int timer = 0;

        // Disc vertex mesh
        private VertexPositionColorTexture[] _discVerts;
        private static short[] _discIndices;

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

        #region Disc Mesh

        private void InitDiscMesh()
        {
            _discVerts = new VertexPositionColorTexture[1 + RingSegments];

            if (_discIndices == null)
            {
                _discIndices = new short[RingSegments * 3];
                for (int i = 0; i < RingSegments; i++)
                {
                    int idx = i * 3;
                    _discIndices[idx + 0] = 0; // center
                    _discIndices[idx + 1] = (short)(1 + i);
                    _discIndices[idx + 2] = (short)(1 + (i + 1) % RingSegments);
                }
            }
        }

        private int BuildDiscMesh(Vector2 centerScreen, float radius, float fadeAlpha, float time)
        {
            if (_discVerts == null) InitDiscMesh();

            // Center vertex: full alpha, UV at texture center
            Color centerColor = TerraBladeShaderManager.GetPaletteColor(0.5f) * fadeAlpha;
            _discVerts[0] = new VertexPositionColorTexture(
                new Vector3(centerScreen, 0),
                centerColor,
                new Vector2(0.5f, 0.5f));

            // Ring vertices: radial outward scroll + angular rotation for amorphous flow
            for (int i = 0; i < RingSegments; i++)
            {
                float angle = i / (float)RingSegments * MathHelper.TwoPi;
                Vector2 offset = angle.ToRotationVector2() * radius;
                Vector2 pos = centerScreen + offset;

                // Radial UV scrolling: rotate angle over time + scroll outward
                float scrollAngle = angle + time * 0.8f;
                float radialScroll = time * 0.5f;
                float u = 0.5f + MathF.Cos(scrollAngle) * (0.5f + radialScroll);
                float v = 0.5f + MathF.Sin(scrollAngle) * (0.5f + radialScroll);

                Color edgeColor = TerraBladeShaderManager.GetPaletteColor(0.3f) * fadeAlpha * 0.08f;
                _discVerts[1 + i] = new VertexPositionColorTexture(
                    new Vector3(pos, 0),
                    edgeColor,
                    new Vector2(u, v));
            }

            return 1 + RingSegments;
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

            var device = Main.instance.GraphicsDevice;
            Effect trailShader = ShaderLoader.Trail;

            // --- Disc mesh with NatureTechnique shader ---
            try { sb.End(); } catch { }

            try
            {
                Texture2D noise = ShaderLoader.GetNoiseTexture("UniversalRadialFlowNoise");
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
                    trailShader.Parameters["uOverbrightMult"]?.SetValue(3.0f);
                    trailShader.Parameters["uGlowThreshold"]?.SetValue(0.4f);
                    trailShader.Parameters["uGlowIntensity"]?.SetValue(1.5f);
                    trailShader.Parameters["uHasSecondaryTex"]?.SetValue(noise != null ? 1f : 0f);
                    trailShader.Parameters["uSecondaryTexScale"]?.SetValue(1.0f);
                    trailShader.Parameters["uSecondaryTexScroll"]?.SetValue(1.2f);

                    int vertCount = BuildDiscMesh(drawPos, Radius, fadeAlpha, time);

                    // Multi-pass for intensity buildup
                    float[] intensities = { 0.6f, 1.0f, 1.6f };
                    for (int pass = 0; pass < intensities.Length; pass++)
                    {
                        trailShader.Parameters["uIntensity"]?.SetValue(intensities[pass]);

                        foreach (var p in trailShader.CurrentTechnique.Passes)
                        {
                            p.Apply();
                            device.DrawUserIndexedPrimitives(
                                PrimitiveType.TriangleList,
                                _discVerts, 0, vertCount,
                                _discIndices, 0, RingSegments);
                        }
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
            sb.Draw(bloomTex, drawPos, null, midBloom * 0.40f * fadeAlpha,
                0f, bloomOrigin, bloomScale * 0.8f * pulse, SpriteEffects.None, 0f);

            Color coreBloom = Color.White;
            sb.Draw(bloomTex, drawPos, null, coreBloom * 0.35f * fadeAlpha,
                0f, bloomOrigin, bloomScale * 0.3f * pulse, SpriteEffects.None, 0f);

            // Restore
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                DepthStencilState.None, RasterizerState.CullNone, null,
                Main.GameViewMatrix.TransformationMatrix);

            return false;
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
