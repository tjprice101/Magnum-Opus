using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using Terraria.GameContent;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Content.Eroica;
using MagnumOpus.Content.FoundationWeapons.SparkleProjectileFoundation;

namespace MagnumOpus.Content.Eroica.Projectiles
{
    /// <summary>
    /// Triumphant Fractal projectile 窶・homing fractal geometry with lightning flourishes.
    /// Self-contained VFX: GPU trail, lightning arcs, fractal afterimages, impact geometry.
    /// </summary>
    public class TriumphantFractalProjectile : ModProjectile
    {
        private ref float AgeTimer => ref Projectile.ai[1];

        private List<(Vector2 start, Vector2 end, float time)> activeLightning = new();

        // 笏笏 Trail tracking 笏笏
        private const int TrailLength = 22;
        private Vector2[] trailPositions = new Vector2[TrailLength];
        private float[] trailRotations = new float[TrailLength];
        private bool trailInitialized = false;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 14;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
            Main.projFrames[Type] = 1;
        }

        public override void SetDefaults()
        {
            Projectile.width = 32;
            Projectile.height = 32;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 180;
            Projectile.alpha = 0;
            Projectile.light = 1f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
            AgeTimer++;

            // 笏笏 Trail position tracking 笏笏
            if (!trailInitialized)
            {
                for (int i = 0; i < TrailLength; i++)
                {
                    trailPositions[i] = Projectile.Center;
                    trailRotations[i] = Projectile.rotation;
                }
                trailInitialized = true;
            }
            else
            {
                for (int i = TrailLength - 1; i > 0; i--)
                {
                    trailPositions[i] = trailPositions[i - 1];
                    trailRotations[i] = trailRotations[i - 1];
                }
                trailPositions[0] = Projectile.Center;
                trailRotations[0] = Projectile.rotation;
            }

            // 笏笏笏 Homing 笏笏笏
            float homingRange = 400f;
            float homingStrength = 0.045f;
            NPC closestNPC = null;
            float closestDist = homingRange;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && !npc.friendly && npc.CanBeChasedBy())
                {
                    float dist = Vector2.Distance(Projectile.Center, npc.Center);
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        closestNPC = npc;
                    }
                }
            }

            if (closestNPC != null)
            {
                Vector2 toTarget = Vector2.Normalize(closestNPC.Center - Projectile.Center);
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, toTarget * Projectile.velocity.Length(), homingStrength);
            }

            // 笏笏笏 Lightning bolt management 笏笏笏
            if ((int)AgeTimer % 8 == 0)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 lightningEnd = Projectile.Center + angle.ToRotationVector2() * Main.rand.NextFloat(40f, 80f);
                activeLightning.Add((Projectile.Center, lightningEnd, 0f));

                // Spawn lightning arc particle
                TriumphantFractalParticleHandler.SpawnParticle(new LightningArcParticle(
                    Projectile.Center,
                    (lightningEnd - Projectile.Center).SafeNormalize(Vector2.UnitX) * 2f,
                    Color.Lerp(FractalUtils.LightningBlue, FractalUtils.FractalGold, Main.rand.NextFloat(0.3f)),
                    Main.rand.NextFloat(0.4f, 0.7f),
                    Main.rand.Next(6, 12)
                ));
            }

            activeLightning.RemoveAll(l => l.time > 10f);
            for (int i = 0; i < activeLightning.Count; i++)
            {
                var l = activeLightning[i];
                activeLightning[i] = (l.start, l.end, l.time + 1f);
            }

            // Pulsating scale
            float pulse = 1f + MathF.Sin(AgeTimer * 0.3f) * 0.08f;
            Projectile.scale = pulse;

            // 笏笏 Particle Spawning 笏笏
            SpawnFlightParticles();
        }

        #region Particle Spawning

        private void SpawnFlightParticles()
        {
            // Fractal sparks flying off
            if (Main.rand.NextBool(2))
            {
                Vector2 perpendicular = Projectile.velocity.SafeNormalize(Vector2.UnitX).RotatedBy(MathHelper.PiOver2);
                Vector2 sparkVel = -Projectile.velocity.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(1f, 3f)
                    + perpendicular * Main.rand.NextFloatDirection() * 2f;

                TriumphantFractalParticleHandler.SpawnParticle(new FractalSparkParticle(
                    Projectile.Center + perpendicular * Main.rand.NextFloatDirection() * 5f,
                    sparkVel,
                    Color.Lerp(FractalUtils.FractalGold, FractalUtils.CrystalWhite, Main.rand.NextFloat(0.3f)),
                    Main.rand.NextFloat(0.3f, 0.6f),
                    Main.rand.Next(8, 16)
                ));
            }

            // Fractal notes along path
            if (Main.rand.NextBool(10))
            {
                TriumphantFractalParticleHandler.SpawnParticle(new FractalNoteParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(8f, 8f),
                    new Vector2(Main.rand.NextFloatDirection() * 0.5f, -Main.rand.NextFloat(0.4f, 1f)),
                    Color.Lerp(FractalUtils.FractalGold, FractalUtils.FractalViolet, Main.rand.NextFloat()),
                    Main.rand.NextFloat(0.3f, 0.5f),
                    Main.rand.Next(30, 50)
                ));
            }
        }

        private void SpawnImpactParticles()
        {
            // Geometry flash
            Color flashColor = FractalUtils.CrystalWhite;
            flashColor.A = 0;
            TriumphantFractalParticleHandler.SpawnParticle(new GeometryFlashParticle(
                Projectile.Center, Vector2.Zero, flashColor, 1.0f, 12
            ));

            // Radial fractal spark burst
            int sparkCount = 16;
            for (int i = 0; i < sparkCount; i++)
            {
                float angle = MathHelper.TwoPi * i / sparkCount + Main.rand.NextFloatDirection() * 0.15f;
                TriumphantFractalParticleHandler.SpawnParticle(new FractalSparkParticle(
                    Projectile.Center,
                    angle.ToRotationVector2() * Main.rand.NextFloat(4f, 10f),
                    Color.Lerp(FractalUtils.FractalGold, FractalUtils.FractalViolet, Main.rand.NextFloat()),
                    Main.rand.NextFloat(0.4f, 0.8f),
                    Main.rand.Next(10, 20)
                ));
            }

            // Lightning arcs radiating from impact
            for (int i = 0; i < 4; i++)
            {
                float angle = MathHelper.TwoPi * i / 4f + Main.rand.NextFloatDirection() * 0.3f;
                TriumphantFractalParticleHandler.SpawnParticle(new LightningArcParticle(
                    Projectile.Center,
                    angle.ToRotationVector2() * Main.rand.NextFloat(3f, 6f),
                    FractalUtils.LightningBlue,
                    Main.rand.NextFloat(0.5f, 0.8f),
                    Main.rand.Next(8, 14)
                ));
            }

            // Fractal bloom
            Color bloomColor = FractalUtils.FractalGold;
            bloomColor.A = 0;
            TriumphantFractalParticleHandler.SpawnParticle(new FractalBloomParticle(
                Projectile.Center, Vector2.Zero, bloomColor, 0.8f, 14
            ));

            // Music notes from explosion
            for (int i = 0; i < 3; i++)
            {
                TriumphantFractalParticleHandler.SpawnParticle(new FractalNoteParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(12f, 12f),
                    new Vector2(Main.rand.NextFloatDirection() * 1f, -Main.rand.NextFloat(0.6f, 1.5f)),
                    Color.Lerp(FractalUtils.FractalGold, FractalUtils.GeometryPink, Main.rand.NextFloat()),
                    Main.rand.NextFloat(0.4f, 0.65f),
                    Main.rand.Next(35, 55)
                ));
            }

            // Dust ring
            for (int i = 0; i < 10; i++)
            {
                Vector2 dustVel = Main.rand.NextVector2CircularEdge(6f, 6f);
                int dust = Dust.NewDust(Projectile.Center - new Vector2(4), 8, 8,
                    ModContent.DustType<FractalDust>(), dustVel.X, dustVel.Y, 0,
                    FractalUtils.FractalGold, Main.rand.NextFloat(0.7f, 1.3f));
                Main.dust[dust].noGravity = true;
            }
        }

        #endregion

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Main.rand.NextBool(3))
            {
                SeekingCrystalHelper.SpawnEroicaCrystals(
                    Projectile.GetSource_FromThis(),
                    target.Center,
                    Projectile.velocity,
                    (int)(damageDone * 0.20f),
                    Projectile.knockBack,
                    Projectile.owner,
                    4
                );
            }

            CreateMassiveExplosion();
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            CreateMassiveExplosion();
            return true;
        }

        public override void OnKill(int timeLeft)
        {
            CreateMassiveExplosion();
        }

        private void CreateMassiveExplosion()
        {
            if (Projectile.localAI[0] >= 1f) return;
            Projectile.localAI[0] = 1f;

            SoundEngine.PlaySound(SoundID.DD2_ExplosiveTrapExplode, Projectile.position);
            SpawnImpactParticles();
        }

        #region Rendering

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Texture2D tex = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 origin = tex.Size() / 2f;

            // ── Layer 1: GPU Fractal Energy Trail ──
            DrawFractalTrail(sb);

            // ── Layer 2: Lightning arcs ──
            DrawLightningArcs(sb);

            // ── Layer 3: Afterimage chain ──
            DrawAfterimages(sb, tex, origin);

            // ── Layer 3b: Crystal Shimmer overlay (SparkleProjectileFoundation) ──
            DrawCrystalShimmer(sb, origin);

            // ── Layer 4: Core fractal sprite ──
            DrawCore(sb, tex, origin, lightColor);

            // ── Layer 5: Additive bloom ──
            DrawBloomOverlay(sb, origin);

            // Eroica theme accent
            EroicaVFXLibrary.BeginEroicaAdditive(sb);
            EroicaVFXLibrary.DrawThemeSakuraAccent(sb, Projectile.Center, 1f, 0.4f);
            EroicaVFXLibrary.EndEroicaAdditive(sb);

            return false;
        }

        private void DrawFractalTrail(SpriteBatch sb)
        {
            if (AgeTimer < 3) return;

            int validCount = 0;
            for (int i = 0; i < TrailLength; i++)
            {
                if (trailPositions[i] != Vector2.Zero)
                    validCount++;
                else
                    break;
            }
            if (validCount < 3) return;

            Vector2[] positions = new Vector2[validCount];
            Array.Copy(trailPositions, positions, validCount);

            var settings = new FractalTrailSettings(
                completionRatio => MathHelper.Lerp(8f, 2f, completionRatio),
                completionRatio =>
                {
                    float fade = (1f - completionRatio);
                    fade = fade * fade;
                    Color baseCol = Color.Lerp(FractalUtils.FractalGold, FractalUtils.FractalViolet, completionRatio * 0.6f);
                    return baseCol * fade * 0.6f;
                },
                smoothen: true
            );

            sb.End();
            try
            {
                FractalTrailRenderer.RenderTrail(positions, settings);
            }
            finally
            {
                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                    DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.TransformationMatrix);
            }
        }

        private void DrawLightningArcs(SpriteBatch sb)
        {
            if (activeLightning.Count == 0) return;

            FractalUtils.EnterShaderRegion(sb);

            foreach (var (start, end, time) in activeLightning)
            {
                float lifeProgress = time / 10f;
                float alpha = (1f - lifeProgress) * 0.6f;
                if (alpha <= 0) continue;

                Color arcColor = Color.Lerp(FractalUtils.LightningBlue, FractalUtils.FractalGold, lifeProgress * 0.5f);
                arcColor.A = 0;

                // Draw jagged line segments between start and end
                Vector2 direction = end - start;
                float length = direction.Length();
                if (length < 1f) continue;
                direction /= length;
                Vector2 perp = new Vector2(-direction.Y, direction.X);

                int segments = Math.Max(3, (int)(length / 15f));
                Vector2 prevPoint = start;

                for (int s = 1; s <= segments; s++)
                {
                    float t = (float)s / segments;
                    Vector2 basePoint = Vector2.Lerp(start, end, t);
                    if (s < segments)
                        basePoint += perp * Main.rand.NextFloatDirection() * 8f * (1f - lifeProgress);

                    // Draw a small line segment as a stretched pixel
                    Vector2 segDir = basePoint - prevPoint;
                    float segLen = segDir.Length();
                    float segRot = segDir.ToRotation();
                    Vector2 midPoint = (prevPoint + basePoint) / 2f - Main.screenPosition;

                    sb.Draw(MagnumTextureRegistry.GetPointBloom(), midPoint, new Rectangle(0, 0, 1, 1),
                        arcColor * alpha, segRot, new Vector2(0.5f, 0.5f),
                        new Vector2(segLen, 2f * (1f - lifeProgress)), SpriteEffects.None, 0f);

                    prevPoint = basePoint;
                }
            }

            FractalUtils.ExitShaderRegion(sb);
        }

        private void DrawAfterimages(SpriteBatch sb, Texture2D tex, Vector2 origin)
        {
            int imageCount = 8;
            FractalUtils.EnterShaderRegion(sb);

            for (int i = imageCount - 1; i >= 0; i--)
            {
                float progress = (float)i / imageCount;
                float trailIndex = progress * (TrailLength - 1);
                int idx = (int)trailIndex;
                float lerp = trailIndex - idx;

                if (idx + 1 >= TrailLength) continue;
                if (trailPositions[idx] == Vector2.Zero || trailPositions[idx + 1] == Vector2.Zero) continue;

                Vector2 pos = Vector2.Lerp(trailPositions[idx], trailPositions[idx + 1], lerp) - Main.screenPosition;
                float rot = MathHelper.Lerp(trailRotations[idx], trailRotations[idx + 1], lerp);

                float fadeFactor = (1f - progress);
                fadeFactor = fadeFactor * fadeFactor;
                Color drawColor = Color.Lerp(FractalUtils.FractalGold, FractalUtils.FractalViolet, progress * 0.6f) * (fadeFactor * 0.35f);
                drawColor.A = 0;

                float scale = Projectile.scale * (1f - progress * 0.25f);
                sb.Draw(tex, pos, null, drawColor, rot + progress * 0.3f, origin, scale, SpriteEffects.None, 0f);
            }

            FractalUtils.ExitShaderRegion(sb);
        }

        /// <summary>
        /// SparkleProjectileFoundation-style crystal shimmer overlay.
        /// Draws CrystalBody + StarFlare textures from SPF with fractal gold coloring.
        /// </summary>
        private void DrawCrystalShimmer(SpriteBatch sb, Vector2 origin)
        {
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float time = (float)Main.timeForVisualEffects * 0.01f;

            // Crystal body overlay — geometric shimmer from SPF
            Texture2D crystalBody = SPFTextures.CrystalBody.Value;
            Texture2D crystalOverlay = SPFTextures.CrystalOverlay.Value;
            Texture2D starFlare = SPFTextures.StarFlare.Value;

            if (crystalBody != null)
            {
                sb.End();
                sb.Begin(SpriteSortMode.Deferred, BlendState.Additive,
                    Main.DefaultSamplerState, DepthStencilState.None,
                    RasterizerState.CullCounterClockwise, null,
                    Main.GameViewMatrix.TransformationMatrix);
                try
                {

                // Crystal body — fractal gold with rotation
                Color bodyCol = FractalUtils.FractalGold with { A = 0 };
                float shimmerPulse = 0.5f + 0.5f * MathF.Sin(time * 3f);
                sb.Draw(crystalBody, drawPos, null, bodyCol * (0.2f * shimmerPulse),
                    Projectile.rotation + time * 0.5f, crystalBody.Size() * 0.5f,
                    Projectile.scale * 0.35f, SpriteEffects.None, 0f);

                // Crystal overlay — violet counter-rotating
                if (crystalOverlay != null)
                {
                    Color overlayCol = FractalUtils.FractalViolet with { A = 0 };
                    float overlayPulse = 0.4f + 0.6f * MathF.Sin(time * 3f + 1.2f);
                    sb.Draw(crystalOverlay, drawPos, null, overlayCol * (0.15f * overlayPulse),
                        -Projectile.rotation - time * 0.3f, crystalOverlay.Size() * 0.5f,
                        Projectile.scale * 0.3f, SpriteEffects.None, 0f);
                }

                // Star flare — directional sparkle
                if (starFlare != null)
                {
                    Color flareCol = FractalUtils.CrystalWhite with { A = 0 };
                    float flarePulse = 0.3f + 0.7f * MathF.Sin(time * 5f);
                    sb.Draw(starFlare, drawPos, null, flareCol * (0.25f * flarePulse),
                        Projectile.rotation, starFlare.Size() * 0.5f,
                        Projectile.scale * 0.2f, SpriteEffects.None, 0f);
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

        private void DrawCore(SpriteBatch sb, Texture2D tex, Vector2 origin, Color lightColor)
        {
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Color coreTint = Color.Lerp(lightColor, FractalUtils.FractalGold, 0.6f);

            // Base fractal sprite
            sb.Draw(tex, drawPos, null, coreTint, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0f);

            // Inner white-hot core
            Color coreGlow = FractalUtils.CrystalWhite;
            coreGlow.A = 0;
            sb.Draw(tex, drawPos, null, coreGlow * 0.45f, Projectile.rotation, origin,
                Projectile.scale * 0.85f, SpriteEffects.None, 0f);
        }

        private void DrawBloomOverlay(SpriteBatch sb, Vector2 origin)
        {
            Texture2D tex = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            Color bloomColor = FractalUtils.FractalGold;
            bloomColor.A = 0;
            float pulse = (float)Math.Sin(AgeTimer * 0.18f) * 0.12f;
            float bloomScale = Projectile.scale * 1.8f + pulse;

            FractalUtils.EnterShaderRegion(sb);
            sb.Draw(tex, drawPos, null, bloomColor * 0.35f, Projectile.rotation, origin, bloomScale, SpriteEffects.None, 0f);
            FractalUtils.ExitShaderRegion(sb);

            // === THEME-SPECIFIC: ER Gyratory Orb overlay — geometric fractal energy halo ===
            Texture2D orbTex = EroicaThemeTextures.ERGyratoryOrb;
            if (orbTex != null)
            {
                Color orbColor = EroicaVFXLibrary.Gold with { A = 0 };
                float orbPulse = 0.7f + 0.3f * (float)Math.Sin(AgeTimer * 0.12f);
                sb.Draw(orbTex, drawPos, null, orbColor * 0.2f * orbPulse,
                    -AgeTimer * 0.03f, orbTex.Size() * 0.5f, Projectile.scale * 0.45f * orbPulse, SpriteEffects.None, 0f);
            }

            // === THEME-SPECIFIC: ER Power Effect Ring pulsing around fractal bolt ===
            Texture2D ringTex = EroicaThemeTextures.ERPowerEffectRing;
            if (ringTex != null)
            {
                Color ringColor = EroicaVFXLibrary.GetPaletteColor(0.4f) with { A = 0 };
                sb.Draw(ringTex, drawPos, null, ringColor * 0.15f,
                    AgeTimer * 0.015f, ringTex.Size() * 0.5f, Projectile.scale * 0.55f, SpriteEffects.None, 0f);
            }
        }

        #endregion
    }
}