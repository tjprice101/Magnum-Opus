using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using MagnumOpus.Content.SwanLake.ResonantWeapons.TheSwansLament.Utilities;
using MagnumOpus.Content.SwanLake.Debuffs;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.VFX.Core;
using MagnumOpus.Content.SwanLake.ResonantWeapons.TheSwansLament.Primitives;
using MagnumOpus.Content.SwanLake.ResonantWeapons.TheSwansLament.Shaders;
using Terraria.Graphics.Shaders;
using ReLogic.Content;

namespace MagnumOpus.Content.SwanLake.ResonantWeapons.TheSwansLament.Projectiles
{
    /// <summary>
    /// Fast white bullet projectile for The Swan's Lament (ranged gun).
    /// Grief-streak trail, feather shrapnel on hit, destruction halo on crit/special.
    /// Foundation-pattern rendering: SpriteBatch bloom trail, no primitives/custom particles.
    /// </summary>
    public class LamentBulletProj : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Textures/InvisibleProjectile";

        public ref float Timer => ref Projectile.ai[0];
        public ref float SpawnMode => ref Projectile.ai[1]; // 0 = normal, 1 = empowered

        private const int TrailLength = 14;
        private Vector2[] oldPos = new Vector2[TrailLength];
        private float[] oldRot = new float[TrailLength];
        private LamentPrimitiveRenderer _trailRenderer;

        private Player Owner => Main.player[Projectile.owner];
        private bool IsEmpowered => SpawnMode == 1f;

        public override void SetStaticDefaults() { ProjectileID.Sets.TrailCacheLength[Projectile.type] = TrailLength; }

        public override void SetDefaults()
        {
            Projectile.width = 8;
            Projectile.height = 8;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 180;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = false;
            Projectile.extraUpdates = 2;
            Projectile.alpha = 0;
        }

        public override void AI()
        {
            Timer++;
            Projectile.rotation = Projectile.velocity.ToRotation();

            // --- Trail recording ---
            for (int i = TrailLength - 1; i > 0; i--)
            {
                oldPos[i] = oldPos[i - 1];
                oldRot[i] = oldRot[i - 1];
            }
            oldPos[0] = Projectile.Center;
            oldRot[0] = Projectile.rotation;

            // --- Grief smoke dust ---
            if (Timer % 3 == 0)
            {
                Dust d = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(4, 4),
                    DustID.Smoke, -Projectile.velocity * 0.15f, 80, LamentUtils.GriefGrey, 0.5f);
                d.noGravity = true;
            }

            // --- White core sparkle ---
            if (Timer % 5 == 0)
            {
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.WhiteTorch,
                    Main.rand.NextVector2Circular(1, 1), 0, LamentUtils.CatharsisWhite, 0.4f);
                d.noGravity = true;
            }

            // Empowered: gold accent dust
            if (IsEmpowered && Timer % 4 == 0)
            {
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.WhiteTorch,
                    Main.rand.NextVector2Circular(2, 2), 0, LamentUtils.RevelationWhite, 0.6f);
                d.noGravity = true;
            }

            Lighting.AddLight(Projectile.Center, 0.6f, 0.6f, 0.7f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<SwansMark>(), 240);

            // Feather shrapnel burst
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi / 8f * i + Main.rand.NextFloat(-0.3f, 0.3f);
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 5f);
                Color c = Color.Lerp(LamentUtils.CatharsisWhite, LamentUtils.GriefGrey, Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(target.Center, DustID.WhiteTorch, vel, 0, c, 0.7f);
                d.noGravity = true;
            }

            // Gold flash on empowered
            if (IsEmpowered)
            {
                for (int i = 0; i < 6; i++)
                {
                    Dust d = Dust.NewDustPerfect(target.Center, DustID.WhiteTorch,
                        Main.rand.NextVector2Circular(3, 3), 0, LamentUtils.RevelationWhite, 0.9f);
                    d.noGravity = true;
                }
            }

            // VFXLibrary impact burst
            try
            {
                SwanLakeVFXLibrary.SpawnRainbowBurst(target.Center, 6, 4f);
                SwanLakeVFXLibrary.SpawnPrismaticSparkles(target.Center, 5, 18f);
                SwanLakeVFXLibrary.SpawnMusicNotes(target.Center, 2, 12f);
                if (IsEmpowered)
                {
                    SwanLakeVFXLibrary.SpawnFeatherBurst(target.Center, 4, 20f);
                }
            } catch { }
        }

        public override void OnKill(int timeLeft)
        {
            _trailRenderer?.Dispose();

            // Small grief puff
            for (int i = 0; i < 6; i++)
            {
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.Smoke,
                    Main.rand.NextVector2Circular(3, 3), 60, LamentUtils.GriefGrey, 0.6f);
                d.noGravity = false;
            }

            // Enhanced death burst
            try
            {
                SwanLakeVFXLibrary.SpawnFeatherDrift(Projectile.Center, 3, 15f);
                SwanLakeVFXLibrary.SpawnPrismaticSparkles(Projectile.Center, 4, 20f);
                SwanLakeVFXLibrary.SpawnMusicNotes(Projectile.Center, 2, 15f);
                if (IsEmpowered)
                {
                    SwanLakeVFXLibrary.SpawnRainbowExplosion(Projectile.Center, 0.8f);
                }
            } catch { }

            // Empowered: spawn Destruction Halo
            if (IsEmpowered && Projectile.owner == Main.myPlayer)
            {
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center,
                    Vector2.Zero, ModContent.ProjectileType<DestructionHaloProj>(),
                    Projectile.damage / 2, 0f, Projectile.owner);
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Vector2 screenPos = Main.screenPosition;

            try
            {
                sb.End();

                // ============ GPU SHADER TRAIL (3-pass grief streak) ============
                if (LamentShaderLoader.HasLamentBulletTrailShader)
                {
                    _trailRenderer ??= new LamentPrimitiveRenderer(Main.graphics.GraphicsDevice);
                    var shaderData = GameShaders.Misc["MagnumOpus:LamentBulletTrail"];
                    var effect = shaderData.Shader;

                    // Build trail point list from oldPos
                    var trailPoints = new List<Vector2>();
                    for (int i = 0; i < TrailLength; i++)
                        if (oldPos[i] != Vector2.Zero) trailPoints.Add(oldPos[i]);

                    if (trailPoints.Count >= 2)
                    {
                        // Common shader uniforms
                        effect.Parameters["uOpacity"]?.SetValue(0.85f);
                        effect.Parameters["uScrollSpeed"]?.SetValue(1.2f);
                        effect.Parameters["uNoiseScale"]?.SetValue(2f);
                        effect.Parameters["uPhase"]?.SetValue(0f);

                        if (MagnumTextureRegistry.PerlinNoise != null)
                        {
                            shaderData.UseImage1(MagnumTextureRegistry.PerlinNoise);
                            effect.Parameters["uHasSecondaryTex"]?.SetValue(1f);
                            effect.Parameters["uSecondaryTexScale"]?.SetValue(1.5f);
                            effect.Parameters["uSecondaryTexScroll"]?.SetValue(0.3f);
                        }

                        float bulletWidth = IsEmpowered ? 8f : 5f;
                        float bulletTaper = IsEmpowered ? 14f : 10f;

                        // Pass 1: Faint grief bloom (LamentTrailGlow @ 3x)
                        effect.Parameters["uColor"]?.SetValue(LamentUtils.GriefGrey.ToVector3());
                        effect.Parameters["uSecondaryColor"]?.SetValue(LamentUtils.MourningBlack.ToVector3());
                        effect.Parameters["uIntensity"]?.SetValue(0.35f);
                        effect.Parameters["uOverbrightMult"]?.SetValue(0f);
                        effect.Parameters["uDistortionAmt"]?.SetValue(0.01f);
                        effect.CurrentTechnique = effect.Techniques["LamentTrailGlow"];

                        var glowSettings = new LamentTrailSettings
                        {
                            WidthFunction = t => (bulletWidth + bulletTaper * (1f - t)) * 3f,
                            ColorFunction = t => Color.Lerp(LamentUtils.GriefGrey, LamentUtils.MourningBlack, t) * (0.25f * (1f - t)),
                            ShaderKey = "MagnumOpus:LamentBulletTrail",
                            TrailLength = 0.9f
                        };
                        _trailRenderer.DrawTrail(trailPoints, glowSettings);

                        // Pass 2: Sharp muted core (LamentTrailMain @ 1x)
                        effect.Parameters["uColor"]?.SetValue(LamentUtils.CatharsisWhite.ToVector3());
                        effect.Parameters["uSecondaryColor"]?.SetValue(LamentUtils.GriefGrey.ToVector3());
                        effect.Parameters["uIntensity"]?.SetValue(1.0f);
                        effect.Parameters["uOverbrightMult"]?.SetValue(0.1f);
                        effect.Parameters["uDistortionAmt"]?.SetValue(0.04f);
                        effect.CurrentTechnique = effect.Techniques["LamentTrailMain"];

                        var mainSettings = new LamentTrailSettings
                        {
                            WidthFunction = t => bulletWidth + bulletTaper * (1f - t),
                            ColorFunction = t => Color.Lerp(LamentUtils.CatharsisWhite, LamentUtils.GriefGrey, t) * (0.7f * (1f - t)),
                            ShaderKey = "MagnumOpus:LamentBulletTrail",
                            TrailLength = 0.9f
                        };
                        _trailRenderer.DrawTrail(trailPoints, mainSettings);

                        // Pass 3: Subtle overbright whisper (LamentTrailGlow @ 1.5x)
                        effect.Parameters["uColor"]?.SetValue(Color.White.ToVector3());
                        effect.Parameters["uSecondaryColor"]?.SetValue(LamentUtils.CatharsisWhite.ToVector3());
                        effect.Parameters["uIntensity"]?.SetValue(1.6f);
                        effect.Parameters["uOverbrightMult"]?.SetValue(0.4f);
                        effect.Parameters["uDistortionAmt"]?.SetValue(0.02f);
                        effect.CurrentTechnique = effect.Techniques["LamentTrailGlow"];

                        var innerSettings = new LamentTrailSettings
                        {
                            WidthFunction = t => (bulletWidth + bulletTaper * (1f - t)) * 1.5f,
                            ColorFunction = t => Color.White * (0.3f * (1f - t)),
                            ShaderKey = "MagnumOpus:LamentBulletTrail",
                            TrailLength = 0.9f
                        };
                        _trailRenderer.DrawTrail(trailPoints, innerSettings);
                    }
                }
                else
                {
                    // Fallback: basic bloom trail
                    sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, SamplerState.LinearClamp,
                        DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
                    Texture2D fb = MagnumTextureRegistry.GetSoftGlow();
                    if (fb != null)
                    {
                        Vector2 fbO = fb.Size() * 0.5f;
                        for (int i = TrailLength - 1; i >= 1; i--)
                        {
                            if (oldPos[i] == Vector2.Zero) continue;
                            float p = 1f - i / (float)TrailLength;
                            Color c = Color.Lerp(LamentUtils.GriefGrey, LamentUtils.CatharsisWhite, p);
                            sb.Draw(fb, oldPos[i] - screenPos, null, c * (p * 0.3f), oldRot[i], fbO, 0.15f + p * 0.1f, SpriteEffects.None, 0f);
                        }
                    }
                    sb.End();
                }

                // ============ BLOOM CORE (5-layer grief/catharsis) ============
                sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, SamplerState.LinearClamp,
                    DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

                Texture2D bloom = MagnumTextureRegistry.GetSoftGlow();
                Texture2D point = MagnumTextureRegistry.GetPointBloom();
                Texture2D star = MagnumTextureRegistry.GetStar4Soft();

                Vector2 drawPos = Projectile.Center - screenPos;
                float pulse = 0.9f + 0.1f * MathF.Sin((float)Main.timeForVisualEffects * 0.1f);
                float empScale = IsEmpowered ? 1.3f : 1f;

                // Layer 1: Outer grief haze
                if (bloom != null)
                {
                    Vector2 bOrigin = bloom.Size() * 0.5f;
                    sb.Draw(bloom, drawPos, null, LamentUtils.GriefGrey * 0.3f * pulse, 0f, bOrigin,
                        0.35f * empScale * pulse, SpriteEffects.None, 0f);
                }

                // Layer 2: Catharsis white mid glow
                if (bloom != null)
                {
                    Vector2 bOrigin = bloom.Size() * 0.5f;
                    sb.Draw(bloom, drawPos, null, LamentUtils.CatharsisWhite * 0.4f * pulse, 0f, bOrigin,
                        0.2f * empScale * pulse, SpriteEffects.None, 0f);
                }

                // Layer 3: Hot white core
                if (point != null)
                {
                    Vector2 pOrigin = point.Size() * 0.5f;
                    sb.Draw(point, drawPos, null, Color.White * 0.85f, 0f, pOrigin,
                        0.08f * empScale * pulse, SpriteEffects.None, 0f);
                }

                // Layer 4: Star accent (subtle, rotating)
                if (star != null)
                {
                    Vector2 sOrigin = star.Size() * 0.5f;
                    float starRot = (float)Main.timeForVisualEffects * 0.03f;
                    sb.Draw(star, drawPos, null, LamentUtils.CatharsisWhite * 0.2f * pulse, starRot,
                        sOrigin, 0.12f * empScale, SpriteEffects.None, 0f);
                }

                // Layer 5: Empowered gold revelation ring
                if (IsEmpowered && bloom != null)
                {
                    Vector2 bOrigin = bloom.Size() * 0.5f;
                    for (int i = 0; i < 6; i++)
                    {
                        float angle = MathHelper.TwoPi / 6f * i + (float)Main.timeForVisualEffects * 0.05f;
                        Vector2 offset = angle.ToRotationVector2() * 6f;
                        sb.Draw(bloom, drawPos + offset, null, LamentUtils.RevelationWhite * 0.2f, 0f, bOrigin, 0.1f, SpriteEffects.None, 0f);
                    }
                }

                // --- Draw bullet sprite ---
                Texture2D tex = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
                if (tex != null)
                {
                    sb.End();
                    sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                        DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

                    Vector2 texOrigin = tex.Size() * 0.5f;
                    sb.Draw(tex, drawPos, null, Projectile.GetAlpha(lightColor), Projectile.rotation + MathHelper.PiOver2,
                        texOrigin, Projectile.scale, SpriteEffects.None, 0f);
                }
            }
            catch { }
            finally
            {
                sb.End();
                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                    DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            }

            // Theme accents (additive)
            try
            {
                sb.End();
                sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, SamplerState.LinearClamp, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
                LamentUtils.DrawThemeAccents(sb, Projectile.Center, 1f, 0.6f);
                sb.End();
                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            }
            catch { }

            return false;
        }
    }
}
