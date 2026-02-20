using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.Shaders;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Content.TestWeapons.SandboxWeapons.Shaders;

namespace MagnumOpus.Content.TestWeapons.SandboxWeapons
{
    /// <summary>
    /// Expanding circular explosion zone spawned by CosmicSpinShard on impact.
    /// Brief flash style: 24-frame lifecycle with sqrt(progress) expansion.
    /// 5-layer additive rendering: VoronoiNoise (3 counter-rotating) + shockwave ring
    /// + ripple rings + core bloom + CosmicNebulaClouds shimmer.
    /// Green/white flame-like energy with Terra Blade palette colors.
    /// </summary>
    public class CosmicImpactZone : ModProjectile
    {
        #region Constants

        private const int ExplosionDuration = 24;
        private const float MaxRadius = 120f;

        #endregion

        #region State

        private int timer = 0;
        private float currentRadius = 0f;
        private bool spawnedInitialVFX = false;

        #endregion

        #region Setup

        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.None;

        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.MeleeNoSpeed;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.timeLeft = ExplosionDuration + 5; // small buffer
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1; // hit each NPC once
        }

        public override bool ShouldUpdatePosition() => false;

        #endregion

        #region Collision

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            if (currentRadius <= 0f) return false;

            Vector2 closestPoint = new Vector2(
                MathHelper.Clamp(Projectile.Center.X, targetHitbox.Left, targetHitbox.Right),
                MathHelper.Clamp(Projectile.Center.Y, targetHitbox.Top, targetHitbox.Bottom));

            return Vector2.Distance(Projectile.Center, closestPoint) <= currentRadius;
        }

        #endregion

        #region AI

        public override void AI()
        {
            timer++;

            float progress = (float)timer / ExplosionDuration;
            currentRadius = MaxRadius * MathF.Sqrt(progress);

            // Expand hitbox
            int size = Math.Max((int)(currentRadius * 2), 10);
            Projectile.Resize(size, size);

            // Spawn-frame VFX (first frame only)
            if (!spawnedInitialVFX)
            {
                SpawnInitialVFX();
                spawnedInitialVFX = true;
            }

            // Dynamic lighting
            float fadeAlpha = progress < 0.6f ? 1f : MathHelper.Lerp(1f, 0f, (progress - 0.6f) / 0.4f);
            Color light = TerraBladeShaderManager.GetPaletteColor(0.5f);
            Lighting.AddLight(Projectile.Center, light.ToVector3() * fadeAlpha * 1.2f);

            if (timer >= ExplosionDuration)
            {
                Projectile.Kill();
            }
        }

        private void SpawnInitialVFX()
        {
            Vector2 center = Projectile.Center;

            // Radial dust burst
            for (int i = 0; i < 12; i++)
            {
                float angle = i / 12f * MathHelper.TwoPi;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 6f);
                Color dustColor = TerraBladeShaderManager.GetPaletteColor(Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(center, DustID.GreenTorch, vel, 0, dustColor, 1.5f);
                d.noGravity = true;
            }

            // Gold shimmer sparks
            for (int i = 0; i < 8; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(4f, 4f);
                Dust d = Dust.NewDustPerfect(center, DustID.Enchanted_Gold, vel, 0, Color.White, 0.9f);
                d.noGravity = true;
            }

            // Bloom ring particles at staggered scales
            for (int i = 0; i < 3; i++)
            {
                float ringScale = 0.3f + i * 0.2f;
                Color ringColor = TerraBladeShaderManager.GetPaletteColor(0.4f + i * 0.2f);
                var ring = new BloomRingParticle(center, Vector2.Zero, ringColor * 0.6f, ringScale, 20);
                MagnumParticleHandler.SpawnParticle(ring);
            }

            // Music notes floating upward
            for (int i = 0; i < 3; i++)
            {
                Vector2 noteVel = Main.rand.NextVector2Circular(2f, 2f) + new Vector2(0f, -1.5f);
                Color noteColor = TerraBladeShaderManager.GetPaletteColor(0.3f + Main.rand.NextFloat() * 0.5f);
                ThemedParticles.MusicNote(center, noteVel, noteColor, 0.8f, 35);
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
            float progress = (float)timer / ExplosionDuration;
            float fadeAlpha = progress < 0.6f ? 1f : MathHelper.Lerp(1f, 0f, (progress - 0.6f) / 0.4f);
            float time = Main.GlobalTimeWrappedHourly;

            // Switch to additive blending for all layers
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null,
                Main.GameViewMatrix.TransformationMatrix);

            DrawLayer1_VoronoiNoise(sb, drawPos, fadeAlpha, time);
            DrawLayer2_ShockwaveRing(sb, drawPos, progress, fadeAlpha, time);
            DrawLayer3_RippleRings(sb, drawPos, progress, fadeAlpha, time);
            DrawLayer4_CoreBloom(sb, drawPos, fadeAlpha, time);
            DrawLayer5_CosmicShimmer(sb, drawPos, progress, fadeAlpha, time);

            // Restore normal blending
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                DepthStencilState.None, RasterizerState.CullNone, null,
                Main.GameViewMatrix.TransformationMatrix);

            return false;
        }

        private void DrawLayer1_VoronoiNoise(SpriteBatch sb, Vector2 drawPos, float fadeAlpha, float time)
        {
            Texture2D noiseTex = SandboxVFXHelper.SafeRequest("MagnumOpus/Assets/VFX/Noise/VoronoiNoise");
            if (noiseTex == null)
                noiseTex = SafeRequest("MagnumOpus/Assets/VFX/Noise/VoronoiNoise");
            if (noiseTex == null) return;

            Vector2 noiseOrigin = noiseTex.Size() * 0.5f;
            float texSize = Math.Max(noiseTex.Width, noiseTex.Height);

            // Layer 1: Outer — slow CW
            float outerScale = currentRadius * 2.4f / texSize;
            Color outerColor = TerraBladeShaderManager.GetPaletteColor(0.3f) * 0.35f * fadeAlpha;
            sb.Draw(noiseTex, drawPos, null, outerColor, time * 0.8f, noiseOrigin, outerScale, SpriteEffects.None, 0f);

            // Layer 2: Mid — CCW
            float midScale = currentRadius * 2.0f / texSize;
            Color midColor = TerraBladeShaderManager.GetPaletteColor(0.5f) * 0.50f * fadeAlpha;
            sb.Draw(noiseTex, drawPos, null, midColor, -time * 1.2f, noiseOrigin, midScale, SpriteEffects.None, 0f);

            // Layer 3: Core — fast CW
            float coreScale = currentRadius * 1.4f / texSize;
            Color coreColor = TerraBladeShaderManager.GetPaletteColor(0.7f) * 0.65f * fadeAlpha;
            sb.Draw(noiseTex, drawPos, null, coreColor, time * 2.0f, noiseOrigin, coreScale, SpriteEffects.None, 0f);
        }

        private void DrawLayer2_ShockwaveRing(SpriteBatch sb, Vector2 drawPos, float progress, float fadeAlpha, float time)
        {
            Texture2D ringTex = SafeRequest("MagnumOpus/Assets/VFX/Impacts/Expanding Shockwave Ring");
            if (ringTex == null) return;

            Vector2 ringOrigin = ringTex.Size() * 0.5f;
            float ringTexSize = Math.Max(ringTex.Width, ringTex.Height);
            float ringScale = currentRadius * 2.2f / ringTexSize;

            Color ringColor = Color.White * (1f - progress) * 0.6f * fadeAlpha;
            sb.Draw(ringTex, drawPos, null, ringColor, time * 0.5f, ringOrigin, ringScale, SpriteEffects.None, 0f);
        }

        private void DrawLayer3_RippleRings(SpriteBatch sb, Vector2 drawPos, float progress, float fadeAlpha, float time)
        {
            Texture2D rippleTex = SafeRequest("MagnumOpus/Assets/VFX/Impacts/Concentric Impact Ripple Rings");
            if (rippleTex == null) return;

            Vector2 rippleOrigin = rippleTex.Size() * 0.5f;
            float rippleTexSize = Math.Max(rippleTex.Width, rippleTex.Height);
            float rippleScale = currentRadius * 1.8f / rippleTexSize;

            Color rippleColor = TerraBladeShaderManager.GetPaletteColor(0.4f) * (1f - progress) * 0.4f * fadeAlpha;
            sb.Draw(rippleTex, drawPos, null, rippleColor, -time * 0.7f, rippleOrigin, rippleScale, SpriteEffects.None, 0f);
        }

        private void DrawLayer4_CoreBloom(SpriteBatch sb, Vector2 drawPos, float fadeAlpha, float time)
        {
            Texture2D bloomTex = SafeRequest("MagnumOpus/Assets/VFX/Blooms/Perfect Soft Color Bloom");
            if (bloomTex == null)
            {
                // Fallback to vanilla flare
                bloomTex = Terraria.GameContent.TextureAssets.Extra[98].Value;
            }

            Vector2 bloomOrigin = bloomTex.Size() * 0.5f;
            float pulse = 1f + MathF.Sin(time * 10f) * 0.12f;

            // Outer bloom
            Color outerBloom = TerraBladeShaderManager.GetPaletteColor(0.3f);
            sb.Draw(bloomTex, drawPos, null, outerBloom * 0.3f * fadeAlpha,
                0f, bloomOrigin, 0.5f * pulse, SpriteEffects.None, 0f);

            // Mid bloom
            Color midBloom = TerraBladeShaderManager.GetPaletteColor(0.6f);
            sb.Draw(bloomTex, drawPos, null, midBloom * 0.5f * fadeAlpha,
                0f, bloomOrigin, 0.3f * pulse, SpriteEffects.None, 0f);

            // Core — white hot
            Color coreBloom = Color.White;
            sb.Draw(bloomTex, drawPos, null, coreBloom * 0.7f * fadeAlpha,
                0f, bloomOrigin, 0.15f * pulse, SpriteEffects.None, 0f);
        }

        private void DrawLayer5_CosmicShimmer(SpriteBatch sb, Vector2 drawPos, float progress, float fadeAlpha, float time)
        {
            Texture2D cosmicTex = ShaderLoader.GetNoiseTexture("CosmicNebulaClouds");
            if (cosmicTex == null) return;

            Vector2 cosmicOrigin = cosmicTex.Size() * 0.5f;
            float cosmicTexSize = Math.Max(cosmicTex.Width, cosmicTex.Height);
            float cosmicScale = currentRadius * 1.6f / cosmicTexSize;

            Color cosmicColor = TerraBladeShaderManager.GetPaletteColor(0.4f) * 0.2f * (1f - progress) * fadeAlpha;
            sb.Draw(cosmicTex, drawPos, null, cosmicColor,
                time * 0.3f, cosmicOrigin, cosmicScale, SpriteEffects.None, 0f);
        }

        #endregion

        #region Hit Effects

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Small spark burst on each hit
            for (int i = 0; i < 4; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(3f, 3f);
                Dust d = Dust.NewDustPerfect(target.Center, DustID.GreenTorch, vel, 0,
                    TerraBladeShaderManager.GetPaletteColor(Main.rand.NextFloat(0.3f, 0.7f)), 1.0f);
                d.noGravity = true;
            }
        }

        #endregion
    }
}
