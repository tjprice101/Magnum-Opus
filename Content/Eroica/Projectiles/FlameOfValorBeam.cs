using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Content.Eroica.Projectiles
{
    /// <summary>
    /// Beam projectile fired by Flames of Valor.
    /// UNIQUE heroic flame beam with GPU trail, shader-driven body, sakura petals, and cosmic fire.
    /// </summary>
    public class FlameOfValorBeam : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles Asset Library/Stars/4PointedStarSoft";
        
        private float orbitAngle = 0f;
        private float pulseTimer = 0f;
        
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 18;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 180;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.alpha = 50;
            Projectile.light = 0.8f;
            Projectile.extraUpdates = 1;
            Projectile.scale = 0.65f;
        }

        public override void AI()
        {
            orbitAngle += 0.15f;
            pulseTimer += 0.12f;
            
            Projectile.rotation = Projectile.velocity.ToRotation();
            
            // === UNIQUE EFFECT: Orbiting flame sparks ===
            if (Projectile.timeLeft % 5 == 0)
            {
                for (int i = 0; i < 2; i++)
                {
                    float sparkAngle = orbitAngle + MathHelper.Pi * i;
                    Vector2 sparkPos = Projectile.Center + sparkAngle.ToRotationVector2() * 12f;
                    Color sparkColor = i == 0 ? EroicaPalette.Gold : EroicaPalette.Crimson;
                    EroicaVFXLibrary.BloomFlare(sparkPos, sparkColor, 0.2f, 8, 2, 0.55f);
                }
            }
            
            // === UNIQUE EFFECT: Heroic flame trail ===
            if (Main.rand.NextBool(2))
            {
                Vector2 trailOffset = Main.rand.NextVector2Circular(6f, 6f);
                Color trailColor = Color.Lerp(EroicaPalette.Scarlet, EroicaPalette.Gold, Main.rand.NextFloat());
                var trail = new GenericGlowParticle(Projectile.Center + trailOffset, -Projectile.velocity * 0.12f + Main.rand.NextVector2Circular(1f, 1f),
                    trailColor * 0.7f, 0.25f, 16, true);
                MagnumParticleHandler.SpawnParticle(trail);
            }
            
            // === UNIQUE EFFECT: Fiery spark particles ===
            if (Main.rand.NextBool(3))
            {
                var spark = new GlowSparkParticle(Projectile.Center + Main.rand.NextVector2Circular(5f, 5f),
                    -Projectile.velocity * 0.15f + Main.rand.NextVector2Circular(2f, 2f),
                    false, 18, 0.22f, EroicaPalette.Flame, new Vector2(0.03f, 1.5f));
                MagnumParticleHandler.SpawnParticle(spark);
            }
            
            // === UNIQUE EFFECT: Golden/crimson dust ===
            if (Main.rand.NextBool(3))
            {
                int dustType = Main.rand.NextBool() ? DustID.GoldFlame : DustID.CrimsonTorch;
                Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(6f, 6f), 
                    dustType, -Projectile.velocity * 0.18f + Main.rand.NextVector2Circular(2f, 2f), 0, default, 1.3f);
                dust.noGravity = true;
            }
            
            // === UNIQUE EFFECT: Occasional sakura petal ===
            if (Main.rand.NextBool(12))
            {
                EroicaVFXLibrary.SpawnSakuraPetals(Projectile.Center, 1, 8f);
            }
            
            // MUSICAL NOTATION - Heroic melody trail
            if (Main.rand.NextBool(6))
            {
                Color noteColor = Color.Lerp(EroicaPalette.Scarlet, EroicaPalette.Gold, Main.rand.NextFloat());
                Vector2 noteVel = new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), -1f);
                EroicaVFXLibrary.SpawnMusicNote(Projectile.Center, noteVel, noteColor, 0.35f, 35);
            }
            
            // Pulsing light
            float pulse = 0.7f + (float)Math.Sin(pulseTimer) * 0.2f;
            Lighting.AddLight(Projectile.Center, EroicaPalette.Gold.ToVector3() * pulse * 0.8f + EroicaPalette.Scarlet.ToVector3() * pulse * 0.4f);
        }

        public override void OnKill(int timeLeft)
        {
            EroicaVFXLibrary.DeathHeroicFlash(Projectile.Center, 0.9f);
            SoundEngine.PlaySound(SoundID.Item10 with { Volume = 0.5f }, Projectile.Center);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            float time = (float)Main.timeForVisualEffects * 0.015f;
            float pulse = 1f + (float)Math.Sin(pulseTimer) * 0.15f;

            // ═══════════════════════════════════════════
            //  LAYER 1: GPU Bloom Trail (FuneralTrailRenderer)
            // ═══════════════════════════════════════════
            DrawBloomTrail(sb);

            // ═══════════════════════════════════════════
            //  LAYER 2: Shader Flame Beam Body
            // ═══════════════════════════════════════════
            DrawShaderFlameBody(sb, time);

            // ═══════════════════════════════════════════
            //  LAYER 3: Layered flame head
            // ═══════════════════════════════════════════
            DrawFlameHead(sb, pulse);

            // ═══════════════════════════════════════════
            //  LAYER 4: Orbiting flame sparks
            // ═══════════════════════════════════════════
            DrawOrbitingSparks(sb, pulse);

            // Eroica theme accent
            EroicaVFXLibrary.BeginEroicaAdditive(sb);
            EroicaVFXLibrary.DrawThemeSakuraAccent(sb, Projectile.Center, 1f, 0.5f);
            EroicaVFXLibrary.EndEroicaAdditive(sb);

            return false;
        }

        private void DrawBloomTrail(SpriteBatch sb)
        {
            int validCount = 0;
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] != Vector2.Zero) validCount++;
                else break;
            }
            if (validCount < 3) return;

            Vector2[] positions = new Vector2[validCount];
            for (int i = 0; i < validCount; i++)
                positions[i] = Projectile.oldPos[i] + Projectile.Size / 2f;

            var settings = new FuneralTrailSettings(
                completionRatio => MathHelper.Lerp(6f, 1.5f, completionRatio),
                completionRatio =>
                {
                    float fade = (1f - completionRatio);
                    fade = fade * fade;
                    return Color.Lerp(EroicaPalette.Gold, EroicaPalette.Scarlet, completionRatio * 0.7f) * fade * 0.7f;
                },
                smoothen: true
            );

            sb.End();
            try
            {
                FuneralTrailRenderer.RenderTrail(positions, settings);
            }
            finally
            {
                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                    DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.TransformationMatrix);
            }
        }

        private void DrawShaderFlameBody(SpriteBatch sb, float time)
        {
            int validCount = 0;
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] != Vector2.Zero) validCount++;
                else break;
            }
            if (validCount < 3) return;

            Texture2D stripTex = EroicaTextures.EmberScatter?.Value ?? EroicaTextures.EnergyTrailUV?.Value;
            if (stripTex == null) return;

            int texW = stripTex.Width;
            int texH = stripTex.Height;
            float scrollTime = (float)Main.timeForVisualEffects * 0.007f;
            const float BaseBeamWidth = 20f;
            int srcWidth = Math.Max(1, texW / validCount);

            bool hasShader = EroicaShaderManager.HasHeroicFlameTrail;

            if (hasShader)
            {
                // PASS 1: Heroic flame trail body
                EroicaShaderManager.BeginShaderAdditive(sb);
                try
                {
                    EroicaShaderManager.ApplyHeroicFlameTrail(time, EroicaPalette.Gold, EroicaPalette.Scarlet, glowPass: false);

                    for (int i = 0; i < validCount - 1; i++)
                    {
                        float progress = 1f - (float)i / validCount;
                        float fade = progress * progress;
                        if (fade < 0.01f) continue;

                        float width = BaseBeamWidth * (0.3f + 0.7f * progress);
                        Vector2 segStart = Projectile.oldPos[i] + Projectile.Size / 2f;
                        Vector2 segEnd = Projectile.oldPos[i + 1] + Projectile.Size / 2f;
                        Vector2 segDir = segStart - segEnd;
                        float segLength = segDir.Length();
                        if (segLength < 0.5f) continue;
                        float segAngle = segDir.ToRotation();

                        float uStart = ((float)i / validCount + scrollTime * 3f) % 1f;
                        int srcX = (int)(uStart * texW) % texW;
                        Rectangle srcRect = new Rectangle(srcX, 0, srcWidth, texH);

                        float scaleX = segLength / (float)srcWidth;
                        float scaleY = width / (float)texH;
                        Vector2 pos = segStart - Main.screenPosition;
                        Vector2 drawOrigin = new Vector2(0, texH / 2f);

                        sb.Draw(stripTex, pos, srcRect, Color.White * (fade * 0.5f), segAngle, drawOrigin,
                            new Vector2(scaleX, scaleY), SpriteEffects.None, 0f);
                    }
                }
                finally
                {
                    EroicaShaderManager.RestoreSpriteBatch(sb);
                }

                // PASS 2: Heroic flame glow — wider
                EroicaShaderManager.BeginShaderAdditive(sb);
                try
                {
                    EroicaShaderManager.ApplyHeroicFlameTrail(time, EroicaPalette.Scarlet, EroicaPalette.Gold, glowPass: true);

                    for (int i = 0; i < validCount - 1; i++)
                    {
                        float progress = 1f - (float)i / validCount;
                        float fade = progress * progress;
                        if (fade < 0.02f) continue;

                        float width = BaseBeamWidth * (0.3f + 0.7f * progress) * 1.5f;
                        Vector2 segStart = Projectile.oldPos[i] + Projectile.Size / 2f;
                        Vector2 segEnd = Projectile.oldPos[i + 1] + Projectile.Size / 2f;
                        Vector2 segDir = segStart - segEnd;
                        float segLength = segDir.Length();
                        if (segLength < 0.5f) continue;
                        float segAngle = segDir.ToRotation();

                        float uStart = ((float)i / validCount + scrollTime * 3f) % 1f;
                        int srcX = (int)(uStart * texW) % texW;
                        Rectangle srcRect = new Rectangle(srcX, 0, srcWidth, texH);

                        float scaleX = segLength / (float)srcWidth;
                        float scaleY = width / (float)texH;
                        Vector2 pos = segStart - Main.screenPosition;
                        Vector2 drawOrigin = new Vector2(0, texH / 2f);

                        sb.Draw(stripTex, pos, srcRect, Color.White * (fade * 0.2f), segAngle, drawOrigin,
                            new Vector2(scaleX, scaleY), SpriteEffects.None, 0f);
                    }
                }
                finally
                {
                    EroicaShaderManager.RestoreSpriteBatch(sb);
                }
            }
            else
            {
                // Fallback: palette-colored trail segments
                sb.End();
                sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, SamplerState.LinearClamp,
                    DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
                try
                {
                    for (int i = 0; i < validCount - 1; i++)
                    {
                        float progress = 1f - (float)i / validCount;
                        float fade = progress * progress;
                        if (fade < 0.01f) continue;

                        float width = BaseBeamWidth * (0.3f + 0.7f * progress);
                        Vector2 segStart = Projectile.oldPos[i] + Projectile.Size / 2f;
                        Vector2 segEnd = Projectile.oldPos[i + 1] + Projectile.Size / 2f;
                        Vector2 segDir = segStart - segEnd;
                        float segLength = segDir.Length();
                        if (segLength < 0.5f) continue;
                        float segAngle = segDir.ToRotation();

                        int srcX = (int)(((float)i / validCount + scrollTime * 3f) % 1f * texW) % texW;
                        Rectangle srcRect = new Rectangle(srcX, 0, srcWidth, texH);
                        float scaleX = segLength / (float)srcWidth;
                        float scaleY = width / (float)texH;
                        Vector2 pos = segStart - Main.screenPosition;
                        Vector2 drawOrigin = new Vector2(0, texH / 2f);

                        Color bodyColor = Color.Lerp(EroicaPalette.Gold, EroicaPalette.Scarlet, 1f - progress) with { A = 0 };
                        sb.Draw(stripTex, pos, srcRect, bodyColor * (fade * 0.5f), segAngle, drawOrigin,
                            new Vector2(scaleX, scaleY), SpriteEffects.None, 0f);
                    }
                }
                finally
                {
                    sb.End();
                    sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                        DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.TransformationMatrix);
                }
            }
        }

        private void DrawFlameHead(SpriteBatch sb, float pulse)
        {
            Texture2D glowTex = MagnumTextureRegistry.GetSoftGlow();
            if (glowTex == null) return;

            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 glowOrigin = glowTex.Size() / 2f;

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            // Outer scarlet glow
            sb.Draw(glowTex, drawPos, null, EroicaPalette.Scarlet with { A = 0 } * 0.35f,
                Projectile.rotation, glowOrigin, 0.5f * pulse, SpriteEffects.None, 0f);
            // Middle flame layer
            sb.Draw(glowTex, drawPos, null, EroicaPalette.Flame with { A = 0 } * 0.5f,
                Projectile.rotation, glowOrigin, 0.35f * pulse, SpriteEffects.None, 0f);
            // Inner gold core
            sb.Draw(glowTex, drawPos, null, EroicaPalette.Gold with { A = 0 } * 0.65f,
                Projectile.rotation, glowOrigin, 0.2f * pulse, SpriteEffects.None, 0f);
            // White-hot tip
            sb.Draw(glowTex, drawPos, null, Color.White with { A = 0 } * 0.8f,
                Projectile.rotation, glowOrigin, 0.1f * pulse, SpriteEffects.None, 0f);

            // Directional streak
            Texture2D streak = MagnumTextureRegistry.GetBeamStreak();
            if (streak != null)
            {
                Vector2 streakOrigin = new Vector2(streak.Width * 0.5f, streak.Height * 0.5f);
                sb.Draw(streak, drawPos, null, EroicaPalette.Gold with { A = 0 } * 0.4f,
                    Projectile.rotation, streakOrigin, new Vector2(0.5f, 0.15f) * pulse, SpriteEffects.None, 0f);
            }

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.TransformationMatrix);
        }

        private void DrawOrbitingSparks(SpriteBatch sb, float pulse)
        {
            Texture2D flareTex = MagnumTextureRegistry.GetFlare();
            if (flareTex == null) return;

            Vector2 flareOrigin = flareTex.Size() / 2f;

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            for (int i = 0; i < 2; i++)
            {
                float sparkAngle = orbitAngle + MathHelper.Pi * i;
                Vector2 sparkPos = Projectile.Center + sparkAngle.ToRotationVector2() * 10f - Main.screenPosition;
                Color sparkColor = (i == 0 ? EroicaPalette.Gold : EroicaPalette.Crimson) with { A = 0 };
                sb.Draw(flareTex, sparkPos, null, sparkColor * 0.6f, sparkAngle, flareOrigin, 0.15f * pulse, SpriteEffects.None, 0f);
            }

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.TransformationMatrix);
        }

        public override Color? GetAlpha(Color lightColor)
        {
            return Color.White;
        }
    }
}