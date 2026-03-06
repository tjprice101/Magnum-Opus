using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using MagnumOpus.Content.SwanLake.ResonantWeapons.TheSwansLament.Utilities;
using MagnumOpus.Content.SwanLake.Debuffs;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.VFX.Core;
using MagnumOpus.Content.SwanLake.ResonantWeapons.TheSwansLament.Shaders;
using Terraria.Graphics.Shaders;
using ReLogic.Content;

namespace MagnumOpus.Content.SwanLake.ResonantWeapons.TheSwansLament.Projectiles
{
    /// <summary>
    /// Expanding halo ring projectile for The Swan's Lament.
    /// Ring-shaped collision (edge only), EaseOutQuart expansion 20px → 180px over 2s.
    /// Applies MournfulGaze debuff. Foundation-pattern rendering.
    /// </summary>
    public class DestructionHaloProj : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Textures/InvisibleProjectile";

        public ref float Timer => ref Projectile.ai[0];

        private const float StartRadius = 20f;
        private const float MaxRadius = 180f;
        private const int Duration = 120; // 2 seconds
        private const float RingThickness = 28f; // How thick the damage ring is

        private float Expansion => EaseOutQuart(MathHelper.Clamp(Timer / Duration, 0f, 1f));
        private float CurrentRadius => MathHelper.Lerp(StartRadius, MaxRadius, Expansion);

        private static float EaseOutQuart(float t) => 1f - MathF.Pow(1f - t, 4f);

        public override void SetDefaults()
        {
            Projectile.width = 360;
            Projectile.height = 360;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = -1;
            Projectile.timeLeft = Duration;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1; // Hit once per NPC
            Projectile.alpha = 0;
        }

        public override void AI()
        {
            Timer++;

            // Slow velocity decay (if spawned with velocity)
            Projectile.velocity *= 0.96f;

            // Resize hitbox to encompass the ring
            float radius = CurrentRadius;
            int newSize = (int)(radius * 2 + RingThickness);
            if (Math.Abs(Projectile.width - newSize) > 4)
            {
                Projectile.position += new Vector2((Projectile.width - newSize) / 2f, (Projectile.height - newSize) / 2f);
                Projectile.width = newSize;
                Projectile.height = newSize;
            }

            // Ring edge dust
            if (Timer % 2 == 0)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 ringPos = Projectile.Center + angle.ToRotationVector2() * radius;
                Color c = Color.Lerp(LamentUtils.CatharsisWhite, LamentUtils.GriefGrey, Main.rand.NextFloat(0.3f));
                Dust d = Dust.NewDustPerfect(ringPos, DustID.WhiteTorch,
                    angle.ToRotationVector2() * 1.5f, 0, c, 0.6f);
                d.noGravity = true;
            }

            // Gold accent particles at ring front
            if (Timer % 6 == 0)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 ringPos = Projectile.Center + angle.ToRotationVector2() * radius;
                Dust d = Dust.NewDustPerfect(ringPos, DustID.WhiteTorch,
                    angle.ToRotationVector2() * 0.5f, 0, LamentUtils.RevelationWhite, 0.5f);
                d.noGravity = true;
            }

            Lighting.AddLight(Projectile.Center, 0.4f, 0.4f, 0.5f);
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            // Ring-shaped collision: only hits at the edge of the ring, not inside
            float dist = Vector2.Distance(Projectile.Center, targetHitbox.Center.ToVector2());
            float targetRadius = Math.Max(targetHitbox.Width, targetHitbox.Height) / 2f;
            float radius = CurrentRadius;

            float innerEdge = radius - RingThickness / 2f;
            float outerEdge = radius + RingThickness / 2f;

            return dist - targetRadius < outerEdge && dist + targetRadius > innerEdge;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<MournfulGaze>(), 300);
            target.AddBuff(ModContent.BuffType<SwansMark>(), 180);

            // Impact flash at hit point
            for (int i = 0; i < 4; i++)
            {
                Dust d = Dust.NewDustPerfect(target.Center, DustID.WhiteTorch,
                    Main.rand.NextVector2Circular(3, 3), 0, LamentUtils.CatharsisWhite, 0.8f);
                d.noGravity = true;
            }
        }

        public override void OnKill(int timeLeft)
        {
            // Final flash burst
            for (int i = 0; i < 16; i++)
            {
                float angle = MathHelper.TwoPi / 16f * i;
                Vector2 vel = angle.ToRotationVector2() * 3f;
                Dust d = Dust.NewDustPerfect(Projectile.Center + angle.ToRotationVector2() * CurrentRadius,
                    DustID.WhiteTorch, vel, 0, LamentUtils.CatharsisWhite, 0.5f);
                d.noGravity = true;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float radius = CurrentRadius;
            float lifeProgress = Timer / Duration;

            // Fade: full near start, fading out in last 30%
            float opacity = lifeProgress > 0.7f ? MathHelper.Lerp(1f, 0f, (lifeProgress - 0.7f) / 0.3f) : 1f;

            try
            {
                sb.End();

                Texture2D radial = MagnumTextureRegistry.GetRadialBloom();
                Texture2D bloom = MagnumTextureRegistry.GetSoftGlow();
                Texture2D point = MagnumTextureRegistry.GetPointBloom();
                Texture2D halo = MagnumTextureRegistry.GetHaloRing();
                Texture2D star = MagnumTextureRegistry.GetStar4Soft();

                // ============ SHADER PASS: DestructionRevelation prismatic blast ============
                if (LamentShaderLoader.HasDestructionRevelationShader && radial != null)
                {
                    sb.Begin(SpriteSortMode.Immediate, MagnumBlendStates.ShaderAdditive, SamplerState.LinearClamp,
                        DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

                    var shaderData = GameShaders.Misc["MagnumOpus:DestructionRevelation"];
                    var effect = shaderData.Shader;

                    // Common uniforms
                    effect.Parameters["uTime"]?.SetValue((float)Main.timeForVisualEffects * 0.015f);
                    effect.Parameters["uOpacity"]?.SetValue(opacity);
                    effect.Parameters["uPhase"]?.SetValue(lifeProgress);
                    effect.Parameters["uNoiseScale"]?.SetValue(3f);
                    effect.Parameters["uScrollSpeed"]?.SetValue(0.8f);

                    if (MagnumTextureRegistry.PerlinNoise != null)
                    {
                        shaderData.UseImage1(MagnumTextureRegistry.PerlinNoise);
                        effect.Parameters["uHasSecondaryTex"]?.SetValue(true);
                        effect.Parameters["uSecondaryTexScale"]?.SetValue(2.5f);
                        effect.Parameters["uSecondaryTexScroll"]?.SetValue(0.4f);
                    }

                    // Pass 1: RevelationBlastMain — monochrome-to-rainbow radial
                    effect.Parameters["uColor"]?.SetValue(LamentUtils.GriefGrey.ToVector4());
                    effect.Parameters["uSecondaryColor"]?.SetValue(LamentUtils.CatharsisWhite.ToVector4());
                    effect.Parameters["uIntensity"]?.SetValue(0.9f);
                    effect.Parameters["uOverbrightMult"]?.SetValue(0.15f);
                    effect.Parameters["uDistortionAmt"]?.SetValue(0.06f);
                    effect.CurrentTechnique = effect.Techniques["RevelationBlastMain"];
                    effect.CurrentTechnique.Passes["P0"].Apply();

                    Vector2 rOrigin = radial.Size() * 0.5f;
                    float blastScale = (radius * 1.15f) / (radial.Width * 0.5f);
                    sb.Draw(radial, drawPos, null, LamentUtils.CatharsisWhite * 0.3f * opacity,
                        0f, rOrigin, blastScale, SpriteEffects.None, 0f);

                    // Pass 2: RevelationBlastRing — expanding prismatic shockwave ring
                    if (bloom != null)
                    {
                        effect.Parameters["uIntensity"]?.SetValue(1.5f);
                        effect.Parameters["uOverbrightMult"]?.SetValue(0.35f);
                        effect.CurrentTechnique = effect.Techniques["RevelationBlastRing"];
                        effect.CurrentTechnique.Passes["P0"].Apply();

                        Vector2 bOrigin = bloom.Size() * 0.5f;
                        float ringScale = (radius * 2.3f) / bloom.Width;
                        sb.Draw(bloom, drawPos, null, Color.White * 0.35f * opacity,
                            (float)Main.timeForVisualEffects * 0.012f, bOrigin, ringScale, SpriteEffects.None, 0f);
                    }

                    sb.End();
                }

                // ============ BLOOM LAYERS (enhanced halo rendering) ============
                sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, SamplerState.LinearClamp,
                    DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

                // Layer 1: Soft radial bloom backdrop
                if (radial != null)
                {
                    Vector2 rOrigin2 = radial.Size() * 0.5f;
                    float backdropScale = (radius + 30f) / (radial.Width * 0.5f);
                    Color backdropColor = Color.Lerp(LamentUtils.GriefGrey, LamentUtils.CatharsisWhite, 0.3f);
                    sb.Draw(radial, drawPos, null, backdropColor * 0.12f * opacity, 0f, rOrigin2, backdropScale, SpriteEffects.None, 0f);
                }

                // Layer 2: Halo ring texture or constructed ring
                if (halo != null)
                {
                    Vector2 hOrigin = halo.Size() * 0.5f;
                    float haloScale = (radius * 2f) / halo.Width;
                    float spin = (float)Main.timeForVisualEffects * 0.01f;
                    sb.Draw(halo, drawPos, null, LamentUtils.CatharsisWhite * 0.5f * opacity, spin, hOrigin, haloScale, SpriteEffects.None, 0f);
                    sb.Draw(halo, drawPos, null, LamentUtils.RevelationWhite * 0.18f * opacity, -spin * 0.7f, hOrigin, haloScale * 1.05f, SpriteEffects.None, 0f);
                }
                else if (bloom != null)
                {
                    Vector2 bOrigin = bloom.Size() * 0.5f;
                    int ringSegments = 36;
                    for (int i = 0; i < ringSegments; i++)
                    {
                        float angle = MathHelper.TwoPi / ringSegments * i;
                        Vector2 ringPos = drawPos + angle.ToRotationVector2() * radius;
                        Color c = Color.Lerp(LamentUtils.CatharsisWhite, LamentUtils.GriefGrey, 0.2f);
                        sb.Draw(bloom, ringPos, null, c * 0.35f * opacity, 0f, bOrigin, 0.16f, SpriteEffects.None, 0f);
                    }
                }

                // Layer 3: Center grief-to-catharsis glow
                if (bloom != null)
                {
                    Vector2 bOrigin = bloom.Size() * 0.5f;
                    Color centerColor = Color.Lerp(LamentUtils.GriefGrey, LamentUtils.CatharsisWhite, lifeProgress);
                    sb.Draw(bloom, drawPos, null, centerColor * 0.2f * opacity, 0f, bOrigin, 0.3f, SpriteEffects.None, 0f);
                }

                // Layer 4: Star sparkle at center (rotating)
                if (star != null)
                {
                    Vector2 sOrigin = star.Size() * 0.5f;
                    float starRot = (float)Main.timeForVisualEffects * 0.02f;
                    sb.Draw(star, drawPos, null, LamentUtils.RevelationWhite * 0.2f * opacity, starRot, sOrigin, 0.2f, SpriteEffects.None, 0f);
                }

                // Layer 5: Bright point accents at cardinal positions (rotating)
                if (point != null)
                {
                    Vector2 pOrigin = point.Size() * 0.5f;
                    float rotOffset = (float)Main.timeForVisualEffects * 0.025f;
                    for (int i = 0; i < 6; i++)
                    {
                        float angle = MathHelper.TwoPi / 6f * i + rotOffset;
                        Vector2 accentPos = drawPos + angle.ToRotationVector2() * radius;
                        sb.Draw(point, accentPos, null, Color.White * 0.55f * opacity, 0f, pOrigin, 0.13f, SpriteEffects.None, 0f);
                    }
                }

                // Layer 6: Gold revelation shimmer trailing expansion
                if (bloom != null && lifeProgress < 0.6f)
                {
                    Vector2 bOrigin = bloom.Size() * 0.5f;
                    for (int i = 0; i < 8; i++)
                    {
                        float angle = MathHelper.TwoPi / 8f * i + Timer * 0.08f;
                        Vector2 shimmerPos = drawPos + angle.ToRotationVector2() * (radius * 0.95f);
                        sb.Draw(bloom, shimmerPos, null, LamentUtils.RevelationWhite * 0.2f * opacity, 0f, bOrigin, 0.1f, SpriteEffects.None, 0f);
                    }
                }
            }
            catch { }
            finally
            {
                sb.End();
                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                    DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            }

            return false;
        }
    }
}
