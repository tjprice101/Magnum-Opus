using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using Terraria.GameContent;
using MagnumOpus.Content.MoonlightSonata.Debuffs;
using MagnumOpus.Content.Eroica;
using MagnumOpus.Content.Eroica.Weapons.FuneralPrayer;
using MagnumOpus.Content.FoundationWeapons.RibbonFoundation;
using System;
using System.Collections.Generic;

namespace MagnumOpus.Content.Eroica.Projectiles
{
    /// <summary>
    /// Large ricochet beam that bounces between enemies 5-6 times.
    /// Self-contained VFX: GPU trail, flame afterimages, impact blooms, requiem sparks.
    /// </summary>
    public class FuneralPrayerRicochetBeam : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/SandboxLastPrism/Trails/spark_06";

        private int currentTarget = -1;
        private List<int> hitEnemies = new List<int>();
        private int ricochetsRemaining = 5;
        private bool isRicocheting = false;
        private const float MaxRicochetRange = 500f;
        private Vector2 lastHitPosition;

        // 笏笏 Trail tracking 笏笏
        private const int TrailLength = 24;
        private Vector2[] trailPositions = new Vector2[TrailLength];
        private float[] trailRotations = new float[TrailLength];
        private bool trailInitialized = false;
        private float ageTimer = 0;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 14;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 300;
            Projectile.alpha = 255;
            Projectile.light = 0.7f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.extraUpdates = 1;
        }

        public override void AI()
        {
            ageTimer++;

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

            // Initialize ricochet count
            if (Projectile.ai[0] == 0)
            {
                Projectile.ai[0] = 1;
                ricochetsRemaining = Main.rand.Next(5, 7);
            }

            // Find next target if needed
            if (currentTarget < 0 || !Main.npc[currentTarget].active || isRicocheting)
            {
                FindNextTarget();
                isRicocheting = false;
            }

            // Track towards current target
            if (currentTarget >= 0 && Main.npc[currentTarget].active)
            {
                Vector2 direction = (Main.npc[currentTarget].Center - Projectile.Center).SafeNormalize(Vector2.UnitX);
                Projectile.velocity = direction * 20f;

                if (Vector2.Distance(Projectile.Center, Main.npc[currentTarget].Center) < 50f)
                {
                    HitCurrentTarget();
                }
            }
            else if (ricochetsRemaining <= 0)
            {
                Projectile.Kill();
            }

            Projectile.rotation = Projectile.velocity.ToRotation();

            // 笏笏 Particle Spawning 笏笏
            SpawnFlightParticles();
        }

        #region Particle Spawning

        private void SpawnFlightParticles()
        {
            // Intense funeral flames 窶・this is a powerful ricochet beam
            if (Main.rand.NextBool(2))
            {
                Vector2 perpendicular = Projectile.velocity.SafeNormalize(Vector2.UnitX).RotatedBy(MathHelper.PiOver2);
                Vector2 flameVel = -Projectile.velocity.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(1f, 3f)
                    + perpendicular * Main.rand.NextFloatDirection() * 2f
                    + new Vector2(0, -Main.rand.NextFloat(0.5f, 1.2f));

                FuneralParticleHandler.SpawnParticle(new FuneralFlameParticle(
                    Projectile.Center + perpendicular * Main.rand.NextFloatDirection() * 6f,
                    flameVel,
                    Color.Lerp(FuneralUtils.SmolderingAmber, FuneralUtils.PrayerFlame, Main.rand.NextFloat()),
                    Main.rand.NextFloat(0.4f, 0.8f),
                    Main.rand.Next(14, 26)
                ));
            }

            // Requiem sparks trailing behind
            if (Main.rand.NextBool(3))
            {
                Vector2 sparkVel = -Projectile.velocity.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(2f, 5f)
                    + Main.rand.NextVector2Circular(1.5f, 1.5f);
                FuneralParticleHandler.SpawnParticle(new RequiemSparkParticle(
                    Projectile.Center,
                    sparkVel,
                    Color.Lerp(FuneralUtils.PrayerFlame, FuneralUtils.SoulWhite, Main.rand.NextFloat(0.3f)),
                    Main.rand.NextFloat(0.3f, 0.6f),
                    Main.rand.Next(6, 14)
                ));
            }

            // Prayer ash billowing
            if (Main.rand.NextBool(5))
            {
                FuneralParticleHandler.SpawnParticle(new PrayerAshParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(8f, 8f),
                    new Vector2(Main.rand.NextFloatDirection() * 0.4f, -Main.rand.NextFloat(0.3f, 0.6f)),
                    FuneralUtils.AshGray * 0.4f,
                    Main.rand.NextFloat(0.4f, 0.6f),
                    Main.rand.Next(18, 35)
                ));
            }
        }

        private void SpawnRicochetImpactParticles(Vector2 position)
        {
            // Large convergence bloom
            Color bloomColor = FuneralUtils.SoulWhite;
            bloomColor.A = 0;
            FuneralParticleHandler.SpawnParticle(new ConvergenceBloomParticle(
                position, Vector2.Zero, bloomColor, 1.0f, 14
            ));

            // Radial requiem spark burst
            int sparkCount = 14;
            for (int i = 0; i < sparkCount; i++)
            {
                float angle = MathHelper.TwoPi * i / sparkCount + Main.rand.NextFloatDirection() * 0.15f;
                FuneralParticleHandler.SpawnParticle(new RequiemSparkParticle(
                    position,
                    angle.ToRotationVector2() * Main.rand.NextFloat(4f, 9f),
                    Color.Lerp(FuneralUtils.DeepCrimson, FuneralUtils.PrayerFlame, Main.rand.NextFloat()),
                    Main.rand.NextFloat(0.4f, 0.8f),
                    Main.rand.Next(10, 20)
                ));
            }

            // Funeral notes on ricochet 
            for (int i = 0; i < 2; i++)
            {
                FuneralParticleHandler.SpawnParticle(new FuneralNoteParticle(
                    position + new Vector2(Main.rand.NextFloatDirection() * 10f, -6f),
                    new Vector2(Main.rand.NextFloatDirection() * 0.7f, -Main.rand.NextFloat(0.6f, 1.2f)),
                    Color.Lerp(FuneralUtils.RequiemViolet, FuneralUtils.DeepCrimson, Main.rand.NextFloat()),
                    Main.rand.NextFloat(0.4f, 0.6f),
                    Main.rand.Next(35, 55)
                ));
            }

            // Dust burst
            for (int i = 0; i < 8; i++)
            {
                Vector2 dustVel = Main.rand.NextVector2CircularEdge(5f, 5f);
                int dust = Dust.NewDust(position - new Vector2(4), 8, 8,
                    ModContent.DustType<FuneralAshDust>(), dustVel.X, dustVel.Y, 0,
                    FuneralUtils.SmolderingAmber, Main.rand.NextFloat(0.7f, 1.2f));
                Main.dust[dust].noGravity = true;
            }
        }

        #endregion

        private void FindNextTarget()
        {
            if (ricochetsRemaining <= 0)
            {
                currentTarget = -1;
                return;
            }

            int nextTarget = -1;
            float minDistance = MaxRicochetRange;
            Vector2 searchFrom = lastHitPosition != Vector2.Zero ? lastHitPosition : Projectile.Center;

            bool foundBoss = false;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && !npc.friendly && npc.boss && !npc.dontTakeDamage && !hitEnemies.Contains(i))
                {
                    float distance = Vector2.Distance(searchFrom, npc.Center);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        nextTarget = i;
                        foundBoss = true;
                    }
                }
            }

            if (!foundBoss)
            {
                minDistance = MaxRicochetRange;
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (npc.active && !npc.friendly && npc.lifeMax > 5 && !npc.dontTakeDamage && !hitEnemies.Contains(i))
                    {
                        float distance = Vector2.Distance(searchFrom, npc.Center);
                        if (distance < minDistance)
                        {
                            minDistance = distance;
                            nextTarget = i;
                        }
                    }
                }
            }

            currentTarget = nextTarget;
        }

        private void HitCurrentTarget()
        {
            if (currentTarget < 0 || !Main.npc[currentTarget].active)
                return;

            NPC target = Main.npc[currentTarget];

            target.SimpleStrikeNPC(Projectile.damage, 0, false, 0f, null, false, 0f, true);
            target.AddBuff(ModContent.BuffType<MusicsDissonance>(), 360);

            SpawnRicochetImpactParticles(target.Center);

            SoundEngine.PlaySound(SoundID.DD2_BetsyFireballImpact with { Volume = 0.8f, Pitch = -0.2f }, target.position);

            hitEnemies.Add(currentTarget);
            lastHitPosition = target.Center;
            ricochetsRemaining--;
            isRicocheting = true;
            currentTarget = -1;
        }

        #region Rendering

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Texture2D tex = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 origin = tex.Size() / 2f;

            // Alpha decay based on ricochets used (doc: 1.0 → 0.6)
            float bounceAlpha = MathHelper.Lerp(1.0f, 0.6f, 1f - (float)ricochetsRemaining / 5f);

            // ── Layer 1: GPU Requiem Trail ──
            DrawRequiemTrail(sb);

            // ── Layer 1b: ThinLaser Beam Body (ThinLaserFoundation pattern) ──
            DrawThinLaserBody(sb, bounceAlpha);

            // ── Layer 2: Afterimage chain ──
            DrawAfterimages(sb, tex, origin);

            // ── Layer 3: Core beam with soul-fire glow ──
            DrawBeamCore(sb, tex, origin);

            // ── Layer 4: Additive bloom ──
            DrawBloomOverlay(sb, origin);

            // Eroica theme accent
            EroicaVFXLibrary.BeginEroicaAdditive(sb);
            EroicaVFXLibrary.DrawThemeSakuraAccent(sb, Projectile.Center, 1f, 0.4f);
            EroicaVFXLibrary.EndEroicaAdditive(sb);

            return false;
        }

        /// <summary>
        /// ThinLaserFoundation-style beam body — narrow precision beam rendered as
        /// texture strip along trail positions. BaseBeamWidth = 12f per doc spec.
        /// Alpha decays per ricochet bounce.
        /// </summary>
        private void DrawThinLaserBody(SpriteBatch sb, float bounceAlpha)
        {
            if (ageTimer < 2) return;

            int validCount = 0;
            for (int i = 0; i < TrailLength; i++)
            {
                if (trailPositions[i] != Vector2.Zero)
                    validCount++;
                else
                    break;
            }
            if (validCount < 3) return;

            Texture2D stripTex = RBFTextures.EnergySurgeBeam.Value;
            if (stripTex == null) return;

            int texW = stripTex.Width;
            int texH = stripTex.Height;
            float time = (float)Main.timeForVisualEffects * 0.008f;
            const float BaseBeamWidth = 12f;

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive,
                SamplerState.LinearWrap, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.TransformationMatrix);
            try
            {

            int srcWidth = Math.Max(1, texW / validCount);

            for (int i = 0; i < validCount - 1; i++)
            {
                float progress = 1f - (float)i / validCount;
                float fade = progress * progress * bounceAlpha;
                if (fade < 0.01f) continue;

                float width = BaseBeamWidth * (0.4f + 0.6f * progress);

                Vector2 segDir = trailPositions[i] - trailPositions[i + 1];
                float segLength = segDir.Length();
                if (segLength < 0.5f) continue;
                float segAngle = segDir.ToRotation();

                float uStart = ((float)i / validCount + time * 4f) % 1f;
                int srcX = (int)(uStart * texW) % texW;
                Rectangle srcRect = new Rectangle(srcX, 0, srcWidth, texH);

                float scaleX = segLength / (float)srcWidth;
                float scaleY = width / (float)texH;

                Vector2 pos = trailPositions[i] - Main.screenPosition;
                Vector2 drawOrigin = new Vector2(0, texH / 2f);

                // Crimson → Gold gradient for ricochet beam
                Color bodyColor = Color.Lerp(FuneralUtils.DeepCrimson, EroicaPalette.Gold, progress * 0.4f) with { A = 0 };
                sb.Draw(stripTex, pos, srcRect, bodyColor * (fade * 0.6f), segAngle, drawOrigin,
                    new Vector2(scaleX, scaleY), SpriteEffects.None, 0f);

                // Hot white core near head
                if (progress > 0.5f)
                {
                    float coreFade = (progress - 0.5f) / 0.5f;
                    Color coreColor = FuneralUtils.SoulWhite with { A = 0 };
                    sb.Draw(stripTex, pos, srcRect, coreColor * (fade * coreFade * 0.25f), segAngle, drawOrigin,
                        new Vector2(scaleX * 0.3f, scaleY * 0.3f), SpriteEffects.None, 0f);
                }
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

        private void DrawRequiemTrail(SpriteBatch sb)
        {
            if (ageTimer < 2) return;

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

            var settings = new FuneralTrailSettings(
                completionRatio => MathHelper.Lerp(10f, 2f, completionRatio),
                completionRatio =>
                {
                    float fade = (1f - completionRatio);
                    fade = fade * fade;
                    Color baseCol = Color.Lerp(FuneralUtils.SoulWhite, FuneralUtils.DeepCrimson, completionRatio * 0.5f);
                    return baseCol * fade * 0.6f;
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

        private void DrawAfterimages(SpriteBatch sb, Texture2D tex, Vector2 origin)
        {
            int imageCount = 8;
            FuneralUtils.EnterShaderRegion(sb);

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
                Color drawColor = Color.Lerp(FuneralUtils.PrayerFlame, FuneralUtils.RequiemViolet, progress * 0.5f) * (fadeFactor * 0.4f);
                drawColor.A = 0;

                float scale = 1.2f - progress * 0.3f;
                sb.Draw(tex, pos, null, drawColor, rot, origin, scale, SpriteEffects.None, 0f);
            }

            FuneralUtils.ExitShaderRegion(sb);
        }

        private void DrawBeamCore(SpriteBatch sb, Texture2D tex, Vector2 origin)
        {
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            // Outer glow layer
            Color outerColor = FuneralUtils.SmolderingAmber;
            sb.Draw(tex, drawPos, null, outerColor, Projectile.rotation, origin, 1.3f, SpriteEffects.None, 0f);

            // Main beam
            Color coreTint = FuneralUtils.PrayerFlame;
            sb.Draw(tex, drawPos, null, coreTint, Projectile.rotation, origin, 1f, SpriteEffects.None, 0f);

            // White-hot inner core
            Color coreGlow = FuneralUtils.SoulWhite;
            coreGlow.A = 0;
            sb.Draw(tex, drawPos, null, coreGlow * 0.5f, Projectile.rotation, origin, 0.7f, SpriteEffects.None, 0f);
        }

        private void DrawBloomOverlay(SpriteBatch sb, Vector2 origin)
        {
            Texture2D tex = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            Color bloomColor = FuneralUtils.PrayerFlame;
            bloomColor.A = 0;
            float pulse = (float)Math.Sin(ageTimer * 0.15f) * 0.15f;

            FuneralUtils.EnterShaderRegion(sb);
            sb.Draw(tex, drawPos, null, bloomColor * 0.4f, Projectile.rotation, origin, 2f + pulse, SpriteEffects.None, 0f);
            FuneralUtils.ExitShaderRegion(sb);

            // --- ER Crumbling Shatter Burst — explosive shard halo on ricochet ---
            Texture2D shatterTex = EroicaThemeTextures.ERCrumblingShatter;
            if (shatterTex != null)
            {
                Vector2 shatterOrigin = shatterTex.Size() * 0.5f;
                float shatterPulse = 0.5f + (float)Math.Sin(ageTimer * 0.3f) * 0.5f;
                Color shatterCol = FuneralUtils.PrayerFlame with { A = 0 };
                sb.Draw(shatterTex, drawPos, null, shatterCol * (0.25f * shatterPulse), ageTimer * 0.5f, shatterOrigin,
                    0.3f + pulse * 0.05f, SpriteEffects.None, 0f);
            }

            // --- ER Power Effect Ring — concentric energy ring behind ricochet beam ---
            Texture2D ringTex = EroicaThemeTextures.ERPowerEffectRing;
            if (ringTex != null)
            {
                Vector2 ringOrigin = ringTex.Size() * 0.5f;
                float ringPulse = 0.7f + (float)Math.Sin(ageTimer * 0.22f) * 0.3f;
                Color ringCol = Color.Lerp(FuneralUtils.PrayerFlame, FuneralUtils.SmolderingAmber, ringPulse) with { A = 0 };
                sb.Draw(ringTex, drawPos, null, ringCol * (0.18f * ringPulse), Projectile.rotation, ringOrigin,
                    0.45f, SpriteEffects.None, 0f);
            }
        }

        #endregion

        public override void OnKill(int timeLeft)
        {
            // Final death burst
            Color deathColor = FuneralUtils.SoulWhite;
            deathColor.A = 0;
            FuneralParticleHandler.SpawnParticle(new ConvergenceBloomParticle(
                Projectile.Center, Vector2.Zero, deathColor, 0.8f, 10
            ));

            for (int i = 0; i < 10; i++)
            {
                float angle = MathHelper.TwoPi * i / 10f;
                FuneralParticleHandler.SpawnParticle(new FuneralFlameParticle(
                    Projectile.Center,
                    angle.ToRotationVector2() * Main.rand.NextFloat(2f, 5f) + new Vector2(0, -1f),
                    Color.Lerp(FuneralUtils.DeepCrimson, FuneralUtils.SmolderingAmber, Main.rand.NextFloat()),
                    Main.rand.NextFloat(0.4f, 0.7f),
                    Main.rand.Next(15, 28)
                ));
            }
        }
    }
}