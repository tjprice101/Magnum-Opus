using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.VFX;

namespace MagnumOpus.Content.Eroica.Projectiles
{
    /// <summary>
    /// The devastating beam attack fired by Eroica, God of Valor in Phase 2.
    /// Fires straight down at the player after a countdown.
    /// </summary>
    [AllowLargeHitbox("Boss beam attack requires large hitbox for beam collision")]
    public class EroicasBeam : ModProjectile
    {
        // Use the Energy of Eroica sprite as base
        public override string Texture => "MagnumOpus/Content/Eroica/Projectiles/EnergyOfEroica";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 5;
            ProjectileID.Sets.TrailingMode[Type] = 0;
        }

        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 2000; // Very tall beam
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 60; // 1 second of beam
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.alpha = 0;
            Projectile.light = 1f;
        }

        public override void AI()
        {
            // Intense pink lighting along the beam
            for (int i = 0; i < 20; i++)
            {
                Vector2 lightPos = Projectile.Center + new Vector2(0, i * 100);
                Lighting.AddLight(lightPos, 1f, 0.3f, 0.6f);
            }

            // Beam particles
            for (int i = 0; i < 10; i++)
            {
                Vector2 dustPos = Projectile.position + new Vector2(Main.rand.Next(Projectile.width), Main.rand.Next(Projectile.height));
                Dust dust = Dust.NewDustDirect(dustPos, 1, 1, DustID.PinkTorch, 0f, 0f, 100, default, 2f);
                dust.noGravity = true;
                dust.velocity = new Vector2(Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-1f, 1f));
            }

            // Edge sparkles
            for (int i = 0; i < 3; i++)
            {
                float yPos = Main.rand.Next(Projectile.height);
                Dust sparkle = Dust.NewDustDirect(Projectile.position + new Vector2(0, yPos), Projectile.width, 1, DustID.GoldFlame, 0f, 0f, 0, default, 1.5f);
                sparkle.noGravity = true;
                sparkle.velocity = Main.rand.NextVector2Circular(4f, 2f);
            }
            
            // ☁EMUSICAL NOTATION - Heroic melody along beam
            if (Main.rand.NextBool(8))
            {
                float yPos = Main.rand.Next(Projectile.height);
                Vector2 notePos = Projectile.position + new Vector2(Projectile.width / 2f, yPos);
                Color noteColor = Color.Lerp(new Color(200, 50, 50), new Color(255, 215, 0), Main.rand.NextFloat());
                Vector2 noteVel = new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-0.5f, 0.5f));
                float shimmer = 1f + (float)Math.Sin(Main.GameUpdateCount * 0.15f) * 0.1f;
                ThemedParticles.MusicNote(notePos, noteVel, noteColor, 0.8f * shimmer, 30);
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            try
            {
            float time = (float)Main.timeForVisualEffects * 0.015f;

            // Beam geometry
            Vector2 beamTop = Projectile.position - Main.screenPosition;
            float beamHeight = Projectile.height;
            float beamCenterX = beamTop.X + Projectile.width / 2f;
            float fadeIn = Math.Min(1f, (60 - Projectile.timeLeft) / 8f);
            float fadeOut = Math.Min(1f, Projectile.timeLeft / 10f);
            float lifeAlpha = fadeIn * fadeOut;

            Texture2D beamTex = MagnumTextureRegistry.GetBeamStreak();
            Texture2D bloomTex = MagnumTextureRegistry.GetSoftGlow();
            if (beamTex == null) return false;

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            // ═══ LAYER 1: Shader RequiemBeam body (if available) ═══
            DrawShaderBeamBody(sb, time, lifeAlpha, beamCenterX, beamTop.Y, beamHeight, Projectile.width, beamTex);

            // ═══ LAYER 2: Multi-layer stretched beam glow ═══
            {
                Vector2 drawPos = new Vector2(beamCenterX, beamTop.Y);
                Vector2 stretchOrigin = new Vector2(beamTex.Width / 2f, 0);
                float stretchY = beamHeight / beamTex.Height;

                float outerW = (Projectile.width * 4f) / beamTex.Width;
                sb.Draw(beamTex, drawPos, null, EroicaPalette.Scarlet with { A = 0 } * (0.12f * lifeAlpha),
                    0f, stretchOrigin, new Vector2(outerW, stretchY), SpriteEffects.None, 0f);

                float midW = (Projectile.width * 2.5f) / beamTex.Width;
                sb.Draw(beamTex, drawPos, null, EroicaPalette.Crimson with { A = 0 } * (0.25f * lifeAlpha),
                    0f, stretchOrigin, new Vector2(midW, stretchY), SpriteEffects.None, 0f);

                float innerW = (Projectile.width * 1.5f) / beamTex.Width;
                Color innerColor = Color.Lerp(EroicaPalette.Gold, new Color(255, 100, 180), 0.5f) with { A = 0 };
                sb.Draw(beamTex, drawPos, null, innerColor * (0.4f * lifeAlpha),
                    0f, stretchOrigin, new Vector2(innerW, stretchY), SpriteEffects.None, 0f);

                float coreW = (Projectile.width * 0.6f) / beamTex.Width;
                sb.Draw(beamTex, drawPos, null, Color.White with { A = 0 } * (0.5f * lifeAlpha),
                    0f, stretchOrigin, new Vector2(coreW, stretchY), SpriteEffects.None, 0f);

                float shimmerPulse = 0.6f + 0.4f * (float)Math.Sin(time * 4f);
                float shimmerW = (Projectile.width * 2f) / beamTex.Width;
                sb.Draw(beamTex, drawPos, null, EroicaPalette.Gold with { A = 0 } * (0.08f * lifeAlpha * shimmerPulse),
                    0f, stretchOrigin, new Vector2(shimmerW, stretchY), SpriteEffects.None, 0f);
            }

            // ═══ LAYER 3: Endpoint bloom flares ═══
            if (bloomTex != null)
            {
                Vector2 bloomOrigin = bloomTex.Size() / 2f;

                Vector2 topPos = new Vector2(beamCenterX, beamTop.Y);
                sb.Draw(bloomTex, topPos, null, EroicaPalette.Scarlet with { A = 0 } * (0.4f * lifeAlpha),
                    0f, bloomOrigin, 0.58f, SpriteEffects.None, 0f);
                sb.Draw(bloomTex, topPos, null, Color.White with { A = 0 } * (0.2f * lifeAlpha),
                    0f, bloomOrigin, 0.25f, SpriteEffects.None, 0f);

                Vector2 botPos = new Vector2(beamCenterX, beamTop.Y + beamHeight);
                float endPulse = 0.8f + 0.2f * (float)Math.Sin(time * 6f);
                sb.Draw(bloomTex, botPos, null, EroicaPalette.Gold with { A = 0 } * (0.5f * lifeAlpha * endPulse),
                    0f, bloomOrigin, 0.55f, SpriteEffects.None, 0f);
                sb.Draw(bloomTex, botPos, null, Color.White with { A = 0 } * (0.3f * lifeAlpha),
                    0f, bloomOrigin, 0.35f, SpriteEffects.None, 0f);

                Texture2D flareTex = MagnumTextureRegistry.GetFlare();
                if (flareTex != null)
                {
                    Vector2 flareOrigin = flareTex.Size() / 2f;
                    sb.Draw(flareTex, topPos, null, EroicaPalette.Gold with { A = 0 } * (0.35f * lifeAlpha),
                        time * 0.3f, flareOrigin, MathHelper.Min(0.4f, 0.293f), SpriteEffects.None, 0f);
                }
            }

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.TransformationMatrix);

            // Eroica theme accent
            EroicaVFXLibrary.BeginEroicaAdditive(sb);
            EroicaVFXLibrary.DrawThemeSakuraAccent(sb, Projectile.Center, 1f, 0.5f);
            EroicaVFXLibrary.EndEroicaAdditive(sb);

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

        /// <summary>
        /// Draws beam body using RequiemBeam shader if available.
        /// </summary>
        private void DrawShaderBeamBody(SpriteBatch sb, float time, float lifeAlpha,
            float centerX, float topY, float height, float baseWidth, Texture2D beamTex)
        {
            if (!EroicaShaderManager.HasRequiemBeam) return;

            Texture2D stripTex = EroicaTextures.EmberScatter?.Value ?? EroicaTextures.EnergyTrailUV?.Value;
            if (stripTex == null) return;

            int segments = 12;
            float segHeight = height / segments;
            int texW = stripTex.Width;
            int texH = stripTex.Height;
            float scrollTime = (float)Main.timeForVisualEffects * 0.005f;

            EroicaShaderManager.BeginShaderAdditive(sb);
            try
            {
                EroicaShaderManager.ApplyRequiemBeam(time, EroicaPalette.Scarlet, EroicaPalette.Gold,
                    glowPass: false, scrollSpeed: 1.0f, overbrightMult: 3f, arcFrequency: 6f, arcAmplitude: 0.05f);

                for (int i = 0; i < segments; i++)
                {
                    float segY = topY + i * segHeight;
                    float uStart = ((float)i / segments + scrollTime * 2f) % 1f;
                    int srcX = (int)(uStart * texW) % texW;
                    int srcWidth = Math.Max(1, texW / segments);
                    Rectangle srcRect = new Rectangle(srcX, 0, srcWidth, texH);

                    float scaleX = (baseWidth * 2f) / srcWidth;
                    float scaleY = segHeight / texH;
                    Vector2 pos = new Vector2(centerX, segY);
                    Vector2 drawOrigin = new Vector2(srcWidth / 2f, 0);

                    sb.Draw(stripTex, pos, srcRect, Color.White * (0.5f * lifeAlpha), 0f, drawOrigin,
                        new Vector2(scaleX, scaleY), SpriteEffects.None, 0f);
                }
            }
            finally
            {
                EroicaShaderManager.RestoreSpriteBatch(sb);
            }

            EroicaShaderManager.BeginShaderAdditive(sb);
            try
            {
                EroicaShaderManager.ApplyRequiemBeam(time, EroicaPalette.Crimson, EroicaPalette.Scarlet,
                    glowPass: true, scrollSpeed: 0.8f, overbrightMult: 2f, arcFrequency: 4f, arcAmplitude: 0.03f);

                for (int i = 0; i < segments; i++)
                {
                    float segY = topY + i * segHeight;
                    float uStart = ((float)i / segments + scrollTime * 2f) % 1f;
                    int srcX = (int)(uStart * texW) % texW;
                    int srcWidth = Math.Max(1, texW / segments);
                    Rectangle srcRect = new Rectangle(srcX, 0, srcWidth, texH);

                    float scaleX = (baseWidth * 3.5f) / srcWidth;
                    float scaleY = segHeight / texH;
                    Vector2 pos = new Vector2(centerX, segY);
                    Vector2 drawOrigin = new Vector2(srcWidth / 2f, 0);

                    sb.Draw(stripTex, pos, srcRect, Color.White * (0.2f * lifeAlpha), 0f, drawOrigin,
                        new Vector2(scaleX, scaleY), SpriteEffects.None, 0f);
                }
            }
            finally
            {
                EroicaShaderManager.RestoreSpriteBatch(sb);
            }

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            // Custom collision for the beam
            return projHitbox.Intersects(targetHitbox);
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            // Heavy knockback
            target.velocity.Y = 10f;
        }
    }
}
