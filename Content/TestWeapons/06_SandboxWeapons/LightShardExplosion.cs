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
    /// Crystal ball explosion spawned at a beam impact point.
    /// Renders 3 counter-rotating VoronoiNoise layers, an EclipseRing dark ring,
    /// a RippleRing secondary ripple, and a 4-layer bloom stack.
    /// Expanding circular collision hits each NPC once.
    /// </summary>
    public class LightShardExplosion : ModProjectile
    {
        // =====================================================================
        //  Constants
        // =====================================================================

        private const int ExplosionDuration = 28;
        private const float MaxRadius = 140f;

        // =====================================================================
        //  State
        // =====================================================================

        private int timer = 0;

        // Disc vertex mesh for circular noise rendering
        private const int RingSegments = 16;
        private VertexPositionColorTexture[] _discVerts;
        private static short[] _discIndices;

        // =====================================================================
        //  Setup
        // =====================================================================

        public override void SetDefaults()
        {
            Projectile.width = 60;
            Projectile.height = 60;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.MeleeNoSpeed;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.timeLeft = ExplosionDuration + 5;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1; // Hit each NPC only once
        }

        public override string Texture => "Terraria/Images/Extra_" + 98;

        // =====================================================================
        //  AI
        // =====================================================================

        public override void AI()
        {
            timer++;

            // Hold position
            Projectile.velocity = Vector2.Zero;

            float progress = timer / (float)ExplosionDuration;

            // Expand hitbox to match visual radius
            float currentRadius = MaxRadius * MathF.Sqrt(progress); // sqrt for fast expand, slow end
            int size = (int)(currentRadius * 2);
            Projectile.Resize(size, size);

            // Spawn particles on first frame
            if (timer == 1)
            {
                SpawnBurstParticles();

                // Vertical energy pillar VFX
                if (Main.myPlayer == Projectile.owner)
                {
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero,
                        ModContent.ProjectileType<VerticalEnergyPillarVFX>(), 0, 0f, Projectile.owner);
                }
            }

            // Ambient sparks during expansion
            if (timer % 3 == 0 && progress < 0.8f)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 sparkPos = Projectile.Center + angle.ToRotationVector2() * currentRadius * Main.rand.NextFloat(0.5f, 1f);
                Color sparkColor = TerraBladeShaderManager.GetPaletteColor(Main.rand.NextFloat(0.3f, 0.8f));
                Dust d = Dust.NewDustPerfect(sparkPos, DustID.GreenTorch,
                    angle.ToRotationVector2() * Main.rand.NextFloat(1f, 3f), 0, sparkColor, 1.1f);
                d.noGravity = true;
                d.fadeIn = 1.2f;
            }

            // Lighting
            Color lc = TerraBladeShaderManager.GetPaletteColor(0.5f);
            float lightIntensity = (1f - progress) * 1.2f;
            Lighting.AddLight(Projectile.Center, lc.ToVector3() * lightIntensity);

            if (timer >= ExplosionDuration)
            {
                Projectile.Kill();
            }
        }

        // =====================================================================
        //  Collision
        // =====================================================================

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float progress = timer / (float)ExplosionDuration;
            float currentRadius = MaxRadius * MathF.Sqrt(progress);

            // Circle vs rectangle collision
            Vector2 closestPoint = new Vector2(
                MathHelper.Clamp(Projectile.Center.X, targetHitbox.Left, targetHitbox.Right),
                MathHelper.Clamp(Projectile.Center.Y, targetHitbox.Top, targetHitbox.Bottom));

            float dist = Vector2.Distance(Projectile.Center, closestPoint);
            return dist <= currentRadius;
        }

        // =====================================================================
        //  Burst Particles (Frame 1)
        // =====================================================================

        private void SpawnBurstParticles()
        {
            // 10 radial GlowSparks
            for (int i = 0; i < 10; i++)
            {
                float angle = MathHelper.TwoPi * i / 10f + Main.rand.NextFloat(-0.2f, 0.2f);
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 8f);
                Color c = TerraBladeShaderManager.GetPaletteColor(0.3f + (float)i / 10f * 0.5f);
                var spark = new GlowSparkParticle(Projectile.Center, vel, c, 0.4f, 20);
                MagnumParticleHandler.SpawnParticle(spark);
            }

            // 12 GreenTorch dust
            for (int i = 0; i < 12; i++)
            {
                Vector2 dustVel = Main.rand.NextVector2Circular(6f, 6f);
                Color dustColor = TerraBladeShaderManager.GetPaletteColor(Main.rand.NextFloat(0.3f, 0.7f));
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.GreenTorch, dustVel, 0, dustColor, 1.4f);
                d.noGravity = true;
                d.fadeIn = 1.2f;
            }

            // 6 Enchanted_Gold shimmer
            for (int i = 0; i < 6; i++)
            {
                Vector2 shimmerVel = Main.rand.NextVector2Circular(4f, 4f);
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.Enchanted_Gold, shimmerVel, 0,
                    TerraBladeShaderManager.GetPaletteColor(0.8f), 1.0f);
                d.noGravity = true;
                d.fadeIn = 1.2f;
            }

            // 3 expanding bloom rings at staggered scales/opacities
            for (int i = 0; i < 3; i++)
            {
                float progress = i / 3f;
                Color ringColor = TerraBladeShaderManager.GetPaletteColor(0.3f + progress * 0.4f);
                float ringScale = 0.3f + i * 0.15f;
                int ringLife = 18 + i * 4;
                float expansion = 0.04f + i * 0.01f;
                var ring = new BloomRingParticle(
                    Projectile.Center, Vector2.Zero,
                    ringColor * (0.7f - progress * 0.15f),
                    ringScale, ringLife, expansion);
                MagnumParticleHandler.SpawnParticle(ring);
            }
        }

        // =====================================================================
        //  Disc Mesh
        // =====================================================================

        private void InitDiscMesh()
        {
            _discVerts = new VertexPositionColorTexture[1 + RingSegments];

            if (_discIndices == null)
            {
                _discIndices = new short[RingSegments * 3];
                for (int i = 0; i < RingSegments; i++)
                {
                    int idx = i * 3;
                    _discIndices[idx + 0] = 0;
                    _discIndices[idx + 1] = (short)(1 + i);
                    _discIndices[idx + 2] = (short)(1 + (i + 1) % RingSegments);
                }
            }
        }

        private int BuildDiscMesh(Vector2 centerScreen, float radius, float fadeAlpha, float time)
        {
            if (_discVerts == null) InitDiscMesh();

            Color centerColor = TerraBladeShaderManager.GetPaletteColor(0.5f) * fadeAlpha;
            _discVerts[0] = new VertexPositionColorTexture(
                new Vector3(centerScreen, 0),
                centerColor,
                new Vector2(0.5f, 0.5f));

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

        // =====================================================================
        //  Rendering
        // =====================================================================

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float progress = timer / (float)ExplosionDuration;

            // Expand then fade
            float expandScale = MathF.Sqrt(progress);
            float fadeAlpha = progress < 0.6f ? 1f : (1f - (progress - 0.6f) / 0.4f);

            float time = Main.GlobalTimeWrappedHourly;

            // Switch to additive
            sb.End();

            // --- Disc mesh with NatureTechnique shader (circular-masked, UV-scrolling) ---
            var device = Main.instance.GraphicsDevice;
            Effect trailShader = ShaderLoader.Trail;
            float currentRadius = MaxRadius * expandScale;

            try
            {
                Texture2D noise = ShaderLoader.GetNoiseTexture("SoftCircularCaustics");
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
                    trailShader.Parameters["uOverbrightMult"]?.SetValue(4.0f);
                    trailShader.Parameters["uGlowThreshold"]?.SetValue(0.4f);
                    trailShader.Parameters["uGlowIntensity"]?.SetValue(2.0f);
                    trailShader.Parameters["uHasSecondaryTex"]?.SetValue(noise != null ? 1f : 0f);
                    trailShader.Parameters["uSecondaryTexScale"]?.SetValue(1.0f);
                    trailShader.Parameters["uSecondaryTexScroll"]?.SetValue(1.2f);

                    int vertCount = BuildDiscMesh(drawPos, currentRadius, fadeAlpha, time);

                    // 3 passes at increasing intensity for layered energy buildup
                    float[] intensities = { 0.4f, 0.8f, 1.4f };
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

            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            // --- EclipseRing: dark ring outline ---
            Texture2D eclipseTex = SandboxVFXHelper.SafeRequest("MagnumOpus/Assets/VFX/Masks/EclipseRing");
            if (eclipseTex != null)
            {
                Vector2 eOrigin = eclipseTex.Size() * 0.5f;
                float ringScale = (MaxRadius * 2.5f) / Math.Max(eclipseTex.Width, eclipseTex.Height) * expandScale;
                Color ringColor = TerraBladeShaderManager.GetPaletteColor(0.3f);
                sb.Draw(eclipseTex, drawPos, null, ringColor * 0.35f * fadeAlpha, time * 0.5f,
                    eOrigin, ringScale, SpriteEffects.None, 0f);
            }

            // --- RippleRing: secondary ripple ---
            Texture2D rippleTex = SandboxVFXHelper.SafeRequest("MagnumOpus/Assets/VFX/Masks/RippleRing");
            if (rippleTex != null)
            {
                Vector2 rOrigin = rippleTex.Size() * 0.5f;
                float rippleScale = (MaxRadius * 3f) / Math.Max(rippleTex.Width, rippleTex.Height) * expandScale;
                Color rippleColor = TerraBladeShaderManager.GetPaletteColor(0.4f);
                sb.Draw(rippleTex, drawPos, null, rippleColor * 0.2f * fadeAlpha, -time * 0.3f,
                    rOrigin, rippleScale, SpriteEffects.None, 0f);
            }

            // --- 4-Layer Bloom Stack ---
            Texture2D coreTex = Terraria.GameContent.TextureAssets.Extra[98].Value;
            Vector2 coreOrigin = coreTex.Size() * 0.5f;
            float bloomBase = MaxRadius / 40f * expandScale;
            float pulse = 1f + MathF.Sin(time * 10f) * 0.08f;

            Color bloomColor = TerraBladeShaderManager.GetPaletteColor(0.5f);
            sb.Draw(coreTex, drawPos, null, bloomColor * 0.30f * fadeAlpha, 0f, coreOrigin, bloomBase * 1.6f * pulse, SpriteEffects.None, 0f);
            sb.Draw(coreTex, drawPos, null, bloomColor * 0.50f * fadeAlpha, 0f, coreOrigin, bloomBase * 1.1f * pulse, SpriteEffects.None, 0f);
            sb.Draw(coreTex, drawPos, null, bloomColor * 0.70f * fadeAlpha, 0f, coreOrigin, bloomBase * 0.7f * pulse, SpriteEffects.None, 0f);
            sb.Draw(coreTex, drawPos, null, Color.White * 0.85f * fadeAlpha, 0f, coreOrigin, bloomBase * 0.3f * pulse, SpriteEffects.None, 0f);

            // Restore
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            return false;
        }

        // =====================================================================
        //  Networking
        // =====================================================================

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(timer);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            timer = reader.ReadInt32();
        }
    }
}
