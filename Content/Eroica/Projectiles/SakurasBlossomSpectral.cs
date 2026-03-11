using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using MagnumOpus.Common.Systems;
using MagnumOpus.Content.Eroica;
using MagnumOpus.Content.FoundationWeapons.SparkleProjectileFoundation;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Content.FoundationWeapons.RibbonFoundation;
using ReLogic.Content;

namespace MagnumOpus.Content.Eroica.Projectiles
{
    /// <summary>
    /// Spectral copy of Sakura's Blossom — homing phantom blade that seeks enemies
    /// and detonates in petal bursts. Enhanced with acceleration-based homing,
    /// pulsating visual scale, and full VFX module integration.
    /// 
    /// Trail cache: 20 positions for dramatic arc sweep rendering.
    /// Rendering: Delegated to SakurasBlossomVFX.DrawSpectralCopy for
    /// consistent 5-layer bloom + afterimage + perpendicular shimmer pipeline.
    /// </summary>
    public class SakurasBlossomSpectral : ModProjectile
    {
        public override string Texture => "MagnumOpus/Content/Eroica/Weapons/SakurasBlossom/SakurasBlossom";

        // AI state tracking
        private ref float HomingAccel => ref Projectile.ai[0];
        private ref float AgeTimer => ref Projectile.ai[1];

        private int targetNPC = -1;

        // ── Trail tracking for Pure Bloom ribbon ──
        private const int TrailLength = 16;
        private Vector2[] trailPositions = new Vector2[TrailLength];
        private bool trailInitialized = false;

        public override void SetStaticDefaults()
        {
        }

        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 30;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 180;
            Projectile.alpha = 80;
            Projectile.light = 0.6f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
        }

        public override void AI()
        {
            AgeTimer++;

            // Align rotation with velocity direction
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;

            // Pulsating visual scale — breathing blossom effect
            float scaleBase = 1.0f;
            float scalePulse = (float)Math.Sin(AgeTimer * 0.12f) * 0.08f;
            Projectile.scale = scaleBase + scalePulse;

            // ── ENHANCED HOMING — acceleration curve ──
            // Homing strengthens over time: starts gentle, becomes aggressive
            float ageSeconds = AgeTimer / 60f;
            HomingAccel = MathHelper.Lerp(0.020f, 0.048f, MathHelper.Clamp(ageSeconds, 0f, 1f));

            // Find target — prioritize bosses
            if (targetNPC < 0 || !Main.npc[targetNPC].active)
            {
                targetNPC = -1;
                float maxDistance = 850f;
                bool foundBoss = false;

                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (npc.active && !npc.friendly && npc.boss && !npc.dontTakeDamage)
                    {
                        float distance = Vector2.Distance(Projectile.Center, npc.Center);
                        if (distance < maxDistance)
                        {
                            maxDistance = distance;
                            targetNPC = i;
                            foundBoss = true;
                        }
                    }
                }

                if (!foundBoss)
                {
                    maxDistance = 650f;
                    for (int i = 0; i < Main.maxNPCs; i++)
                    {
                        NPC npc = Main.npc[i];
                        if (npc.active && !npc.friendly && npc.lifeMax > 5 && !npc.dontTakeDamage)
                        {
                            float distance = Vector2.Distance(Projectile.Center, npc.Center);
                            if (distance < maxDistance)
                            {
                                maxDistance = distance;
                                targetNPC = i;
                            }
                        }
                    }
                }
            }

            // Apply homing with acceleration curve + rotation smoothing
            if (targetNPC >= 0 && Main.npc[targetNPC].active)
            {
                Vector2 direction = (Main.npc[targetNPC].Center - Projectile.Center).SafeNormalize(Vector2.UnitX);
                float speed = Projectile.velocity.Length();
                float turnWeight = 18f - HomingAccel * 80f; // tighter turns as age increases
                turnWeight = MathHelper.Clamp(turnWeight, 12f, 20f);
                Projectile.velocity = (Projectile.velocity * turnWeight + direction * speed) / (turnWeight + 1f);
            }

            // Slight speed decay after 50 ticks — gives a weighted feel
            if (AgeTimer > 50)
            {
                Projectile.velocity *= 0.998f;
            }

            // ── Trail position tracking ──
            if (!trailInitialized)
            {
                for (int i = 0; i < TrailLength; i++)
                    trailPositions[i] = Projectile.Center;
                trailInitialized = true;
            }
            else
            {
                for (int i = TrailLength - 1; i > 0; i--)
                    trailPositions[i] = trailPositions[i - 1];
                trailPositions[0] = Projectile.Center;
            }

        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Seeking crystals — 33% chance
            if (Main.rand.NextBool(3) && Main.myPlayer == Projectile.owner)
            {
                SeekingCrystalHelper.SpawnEroicaCrystals(
                    Projectile.GetSource_FromThis(),
                    target.Center,
                    Projectile.velocity,
                    (int)(damageDone * 0.18f),
                    Projectile.knockBack,
                    Projectile.owner,
                    4
                );
            }

            SoundEngine.PlaySound(SoundID.DD2_ExplosiveTrapExplode with
            {
                Pitch = 0.15f,
                Volume = 0.7f
            }, Projectile.position);
        }

        public override void OnKill(int timeLeft)
        {
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            try
            {
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            // === GPU Primitive Ribbon Trail (EnhancedTrailRenderer) ===
            EnhancedTrailRenderer.RenderMultiPassTrail(
                trailPositions,
                EnhancedTrailRenderer.LinearTaper(16f),
                EnhancedTrailRenderer.GradientColor(
                    EroicaPalette.Sakura with { A = 0 },
                    EroicaPalette.Gold with { A = 0 } * 0.15f,
                    0.7f),
                bloomMultiplier: 1.2f, coreMultiplier: 0.5f);

            // ── Layer 1: Pure Bloom Ribbon trail (RibbonFoundation Mode 1) ──
            DrawPureBloomTrail(sb);

            // ── Layer 2: Bloom afterimages along trail ──
            DrawBloomAfterimages(sb);

            // ── Layer 3: Noise-masked circular bloom core ──
            DrawNoiseMaskedCore(sb, drawPos);

            // ── Layer 4: Crystal shimmer overlay (SparkleProjectileFoundation) ──
            DrawCrystalShimmer(sb, drawPos);

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
        /// RibbonFoundation Mode 1 (Pure Bloom) — velocity-stretched sakura bloom sprites.
        /// Sakura pink → petal white, ghostly translucent appearance.
        /// </summary>
        private void DrawPureBloomTrail(SpriteBatch sb)
        {
            if (AgeTimer < 3) return;

            int validCount = 0;
            for (int i = 0; i < TrailLength; i++)
            {
                if (trailPositions[i] != Vector2.Zero) validCount++;
                else break;
            }
            if (validCount < 4) return;

            Texture2D bloomTex = RBFTextures.SoftGlowBright.Value;
            Texture2D coreTex = RBFTextures.PointBloom.Value;
            if (bloomTex == null || coreTex == null) return;

            Vector2 bloomOrigin = bloomTex.Size() / 2f;
            Vector2 coreOrigin = coreTex.Size() / 2f;

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                SamplerState.LinearClamp, DepthStencilState.None,
                RasterizerState.CullNone, null,
                Main.GameViewMatrix.TransformationMatrix);
            try
            {

            for (int i = 0; i < validCount; i++)
            {
                float progress = (float)i / validCount;
                float headStrength = 1f - progress;
                float fade = headStrength * headStrength;
                if (fade < 0.01f) continue;

                float width = MathHelper.Lerp(1.5f, 8f, headStrength);
                float scale = width / bloomTex.Width;

                Vector2 vel = i + 1 < validCount ? trailPositions[i] - trailPositions[i + 1] : Projectile.velocity;
                float rot = vel.ToRotation() + MathHelper.PiOver2;

                Vector2 pos = trailPositions[i] - Main.screenPosition;
                float stretchX = scale;
                float stretchY = scale * 2f;

                // Outer sakura haze
                Color outerColor = EroicaPalette.Sakura with { A = 0 } * (fade * 0.25f);
                sb.Draw(bloomTex, pos, null, outerColor, rot, bloomOrigin,
                    new Vector2(stretchX * 1.5f, stretchY * 1.2f), SpriteEffects.None, 0f);

                // Body — sakura → white
                Color bodyColor = Color.Lerp(Color.White, EroicaPalette.Sakura, progress * 0.7f) with { A = 0 } * (fade * 0.35f);
                sb.Draw(bloomTex, pos, null, bodyColor, rot, bloomOrigin,
                    new Vector2(stretchX, stretchY), SpriteEffects.None, 0f);

                // Hot core
                Color coreColor = Color.White with { A = 0 } * (fade * 0.25f * headStrength);
                sb.Draw(coreTex, pos, null, coreColor, rot, coreOrigin,
                    new Vector2(stretchX * 0.3f, stretchY * 0.5f), SpriteEffects.None, 0f);
            }

            }
            finally
            {
                sb.End();
                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                    Main.DefaultSamplerState, DepthStencilState.None,
                    RasterizerState.CullCounterClockwise, null,
                    Main.GameViewMatrix.TransformationMatrix);
            }
        }

        /// <summary>
        /// Bloom afterimages — soft bloom orbs along trail.
        /// Clean sakura petal glow without noise textures.
        /// </summary>
        private void DrawBloomAfterimages(SpriteBatch sb)
        {
            int imageCount = 5;
            Texture2D bloomTex = RBFTextures.SoftGlow.Value;
            if (bloomTex == null) return;

            Vector2 bloomOrigin = bloomTex.Size() / 2f;
            float baseScale = 0.18f * Projectile.scale;

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                SamplerState.LinearClamp, DepthStencilState.None,
                RasterizerState.CullNone, null,
                Main.GameViewMatrix.TransformationMatrix);
            try
            {

            for (int i = imageCount - 1; i >= 0; i--)
            {
                float progress = (float)i / imageCount;
                float trailIndex = progress * (TrailLength - 1);
                int idx = (int)trailIndex;
                float frac = trailIndex - idx;

                if (idx + 1 >= TrailLength) continue;
                if (trailPositions[idx] == Vector2.Zero || trailPositions[idx + 1] == Vector2.Zero) continue;

                Vector2 pos = Vector2.Lerp(trailPositions[idx], trailPositions[idx + 1], frac) - Main.screenPosition;

                float fadeFactor = (1f - progress);
                fadeFactor *= fadeFactor;
                float afterScale = baseScale * (1f - progress * 0.3f);

                // Soft bloom orb
                Color afterColor = Color.Lerp(EroicaPalette.Sakura, EroicaPalette.Gold, progress * 0.3f) with { A = 0 } * (fadeFactor * 0.25f);
                sb.Draw(bloomTex, pos, null, afterColor, 0f, bloomOrigin, afterScale, SpriteEffects.None, 0f);
            }

            }
            finally
            {
                sb.End();
                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                    Main.DefaultSamplerState, DepthStencilState.None,
                    RasterizerState.CullCounterClockwise, null,
                    Main.GameViewMatrix.TransformationMatrix);
            }
        }

        /// <summary>
        /// Pure bloom core — soft sakura glow without noise textures.
        /// Multi-scale bloom stack: outer sakura haze → mid glow → bright core → white-hot center.
        /// </summary>
        private void DrawNoiseMaskedCore(SpriteBatch sb, Vector2 drawPos)
        {
            Texture2D bloomTex = RBFTextures.SoftGlowBright.Value;
            if (bloomTex == null) return;

            Vector2 bloomOrigin = bloomTex.Size() / 2f;

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                SamplerState.LinearClamp, DepthStencilState.None,
                RasterizerState.CullNone, null,
                Main.GameViewMatrix.TransformationMatrix);
            try
            {

            float pulse = 0.85f + 0.15f * (float)Math.Sin(AgeTimer * 0.1f);
            float bloomScale = 0.14f * Projectile.scale;

            // Outer sakura haze — tighter ambient glow
            Color outerColor = EroicaPalette.Sakura with { A = 0 };
            sb.Draw(bloomTex, drawPos, null, outerColor * 0.12f * pulse,
                0f, bloomOrigin, bloomScale * 0.45f, SpriteEffects.None, 0f);

            // Mid sakura-gold glow body
            Color midColor = Color.Lerp(EroicaPalette.Sakura, EroicaPalette.Gold, 0.25f) with { A = 0 };
            sb.Draw(bloomTex, drawPos, null, midColor * 0.2f * pulse,
                0f, bloomOrigin, bloomScale * 0.3f, SpriteEffects.None, 0f);

            // Bright sakura-white bloom body
            Color bodyColor = Color.Lerp(EroicaPalette.Sakura, Color.White, 0.35f) with { A = 0 };
            sb.Draw(bloomTex, drawPos, null, bodyColor * 0.35f * pulse,
                0f, bloomOrigin, bloomScale * 0.2f, SpriteEffects.None, 0f);

            // Hot white core — sharper definition
            Color coreColor = Color.White with { A = 0 };
            sb.Draw(bloomTex, drawPos, null, coreColor * 0.4f * pulse,
                0f, bloomOrigin, bloomScale * 0.08f, SpriteEffects.None, 0f);

            }
            finally
            {
                sb.End();
                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                    Main.DefaultSamplerState, DepthStencilState.None,
                    RasterizerState.CullCounterClockwise, null,
                    Main.GameViewMatrix.TransformationMatrix);
            }
        }

        /// <summary>
        /// SparkleProjectileFoundation crystal shimmer — sakura-pink crystal facets.
        /// </summary>
        private void DrawCrystalShimmer(SpriteBatch sb, Vector2 drawPos)
        {
            Texture2D crystalBody = SPFTextures.CrystalBody.Value;
            Texture2D starFlare = SPFTextures.StarFlare.Value;
            if (crystalBody == null || starFlare == null) return;

            float time = (float)Main.timeForVisualEffects * 0.025f;

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                SamplerState.LinearClamp, DepthStencilState.None,
                RasterizerState.CullNone, null,
                Main.GameViewMatrix.TransformationMatrix);
            try
            {

            float pulse = 0.8f + 0.2f * (float)Math.Sin(AgeTimer * 0.12f);
            float shimmerScale = Projectile.scale * 0.35f;

            if (crystalBody != null)
            {
                Color bodyColor = EroicaPalette.Sakura with { A = 0 };
                sb.Draw(crystalBody, drawPos, null, bodyColor * 0.2f * pulse,
                    time * 0.4f, crystalBody.Size() * 0.5f, shimmerScale, SpriteEffects.None, 0f);
            }

            if (starFlare != null)
            {
                Color flareColor = Color.White with { A = 0 };
                float flarePulse = 0.5f + 0.5f * (float)Math.Sin(AgeTimer * 0.18f + 0.5f);
                sb.Draw(starFlare, drawPos, null, flareColor * 0.15f * flarePulse,
                    Projectile.rotation, starFlare.Size() * 0.5f, shimmerScale * 0.6f, SpriteEffects.None, 0f);
            }

            }
            finally
            {
                sb.End();
                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                    Main.DefaultSamplerState, DepthStencilState.None,
                    RasterizerState.CullCounterClockwise, null,
                    Main.GameViewMatrix.TransformationMatrix);
            }
        }
    }
}
