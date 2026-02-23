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
    /// Renders radial noise via RadialScrollShader MultiLayer technique,
    /// an EclipseRing dark ring, a RippleRing secondary ripple, and a 4-layer bloom stack.
    /// Expanding circular collision hits each NPC once.
    /// </summary>
    public class LightShardExplosion : ModProjectile
    {
        // =====================================================================
        //  Constants
        // =====================================================================

        private const int ExplosionDuration = 28;
        private const float MaxRadius = 70f;

        // =====================================================================
        //  State
        // =====================================================================

        private int timer = 0;

        // =====================================================================
        //  Setup
        // =====================================================================

        public override void SetDefaults()
        {
            Projectile.width = 40;
            Projectile.height = 40;
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

            float currentRadius = MaxRadius * expandScale;

            // --- Noise orb via RadialScrollShader (linear UV scrolling with circle mask) ---
            DrawRadialNoiseExplosion(sb, drawPos, currentRadius, fadeAlpha, time);

            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
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

            // --- Bloom Stack ---
            Texture2D softBloomTex = SandboxVFXHelper.SafeRequest("MagnumOpus/Assets/VFX/Blooms/Perfect Soft Color Bloom");
            Texture2D coreTex = softBloomTex ?? Terraria.GameContent.TextureAssets.Extra[98].Value;
            Vector2 coreOrigin = coreTex.Size() * 0.5f;
            float bloomBase = MaxRadius / 40f * expandScale;
            float pulse = 1f + MathF.Sin(time * 10f) * 0.08f;

            Color bloomColor = TerraBladeShaderManager.GetPaletteColor(0.5f);
            sb.Draw(coreTex, drawPos, null, bloomColor * 0.25f * fadeAlpha, 0f, coreOrigin, bloomBase * 1.6f * pulse, SpriteEffects.None, 0f);
            sb.Draw(coreTex, drawPos, null, bloomColor * 0.30f * fadeAlpha, 0f, coreOrigin, bloomBase * 1.1f * pulse, SpriteEffects.None, 0f);
            sb.Draw(coreTex, drawPos, null, bloomColor * 0.40f * fadeAlpha, 0f, coreOrigin, bloomBase * 0.7f * pulse, SpriteEffects.None, 0f);
            sb.Draw(coreTex, drawPos, null, Color.White * 0.50f * fadeAlpha, 0f, coreOrigin, bloomBase * 0.3f * pulse, SpriteEffects.None, 0f);

            // Restore
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            return false;
        }

        /// <summary>
        /// Draws the explosion noise body using RadialScrollShader MultiLayer technique
        /// with linear UV scrolling and circular masking.
        /// Falls back to scrolling source rectangles if shader is unavailable.
        /// </summary>
        private void DrawRadialNoiseExplosion(SpriteBatch sb, Vector2 drawPos, float radius, float fadeAlpha, float time)
        {
            Effect radialShader = ShaderLoader.RadialScroll;
            Texture2D noiseTex = ShaderLoader.GetNoiseTexture("UniversalRadialFlowNoise");

            if (noiseTex == null) return;

            Vector2 noiseOrigin = noiseTex.Size() * 0.5f;
            float texSize = Math.Max(noiseTex.Width, noiseTex.Height);
            float noiseScale = radius * 2.4f / texSize;

            if (radialShader != null)
            {
                // Shader-based noise scrolling with circular masking
                try
                {
                    radialShader.CurrentTechnique = radialShader.Techniques["MultiLayer"];
                    radialShader.Parameters["uTime"]?.SetValue(time);
                    radialShader.Parameters["uFlowSpeed"]?.SetValue(1.5f);
                    radialShader.Parameters["uRadialSpeed"]?.SetValue(0.8f);
                    radialShader.Parameters["uZoom"]?.SetValue(1.0f);
                    radialShader.Parameters["uRepeat"]?.SetValue(1.0f);
                    radialShader.Parameters["uVignetteSize"]?.SetValue(0.42f);
                    radialShader.Parameters["uVignetteBlend"]?.SetValue(0.12f);
                    radialShader.Parameters["uOpacity"]?.SetValue(fadeAlpha);
                    radialShader.Parameters["uColor"]?.SetValue(TerraBladeShaderManager.GetPaletteColor(0.5f).ToVector4());
                    radialShader.Parameters["uSecondaryColor"]?.SetValue(TerraBladeShaderManager.GetPaletteColor(0.8f).ToVector4());

                    sb.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.LinearWrap,
                        DepthStencilState.None, RasterizerState.CullNone, radialShader,
                        Main.GameViewMatrix.TransformationMatrix);

                    sb.Draw(noiseTex, drawPos, null, Color.White * 0.9f,
                        0f, noiseOrigin, noiseScale, SpriteEffects.None, 0f);

                    sb.End();
                }
                catch
                {
                    try { sb.End(); } catch { }
                }
            }
            else
            {
                // Fallback: vibrant layered energy explosion using shaped VFX textures
                sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                    DepthStencilState.None, RasterizerState.CullNone, null,
                    Main.GameViewMatrix.TransformationMatrix);

                float targetSize = radius * 2f;

                // Radial God Rays — expanding energy burst
                Texture2D godRaysTex = SandboxVFXHelper.SafeRequest("MagnumOpus/Assets/VFX/LightRays/Radial God Rays Full Circle");
                if (godRaysTex != null)
                {
                    Vector2 grOrigin = godRaysTex.Size() * 0.5f;
                    float grScale = targetSize * 1.4f / Math.Max(godRaysTex.Width, godRaysTex.Height);
                    Color grColor = TerraBladeShaderManager.GetPaletteColor(0.35f);
                    sb.Draw(godRaysTex, drawPos, null, grColor * 0.75f * fadeAlpha,
                        time * 0.6f, grOrigin, grScale, SpriteEffects.None, 0f);
                }

                // Energy Flare — central starburst
                Texture2D flareTex = SandboxVFXHelper.SafeRequest("MagnumOpus/Assets/Particles/EnergyFlare");
                if (flareTex != null)
                {
                    Vector2 flOrigin = flareTex.Size() * 0.5f;
                    float flScale = targetSize * 1.1f / Math.Max(flareTex.Width, flareTex.Height);
                    Color flColor = TerraBladeShaderManager.GetPaletteColor(0.5f);
                    sb.Draw(flareTex, drawPos, null, flColor * 0.85f * fadeAlpha,
                        -time * 0.4f, flOrigin, flScale, SpriteEffects.None, 0f);
                }

                // Perfect Soft Color Bloom — structured core glow
                Texture2D bloomTex = SandboxVFXHelper.SafeRequest("MagnumOpus/Assets/VFX/Blooms/Perfect Soft Color Bloom");
                if (bloomTex == null) bloomTex = Terraria.GameContent.TextureAssets.Extra[98].Value;
                Vector2 blOrigin = bloomTex.Size() * 0.5f;
                float blScale = targetSize * 0.7f / Math.Max(bloomTex.Width, bloomTex.Height);
                Color blColor = TerraBladeShaderManager.GetPaletteColor(0.7f);
                sb.Draw(bloomTex, drawPos, null, blColor * 0.7f * fadeAlpha,
                    time * 0.2f, blOrigin, blScale, SpriteEffects.None, 0f);

                // White-hot center
                sb.Draw(bloomTex, drawPos, null, Color.White * 0.6f * fadeAlpha,
                    0f, blOrigin, blScale * 0.3f, SpriteEffects.None, 0f);

                sb.End();
            }
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
