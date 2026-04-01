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
using MagnumOpus.Common.Systems.Shaders;
using System;

namespace MagnumOpus.Content.Eroica.Projectiles
{
    /// <summary>
    /// Tracking electric beam that seeks enemies and applies Musical Dissonance.
    /// Self-contained VFX: GPU trail, flame afterimages, lightning sparks, impact bloom.
    /// </summary>
    public class FuneralPrayerBeam : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/SandboxLastPrism/Trails/spark_06";

        private int targetNPC = -1;
        private Vector2 beamEnd;
        private const float MaxBeamLength = 200f;
        private bool hasReachedEnd = false;
        private bool hasHitEnemy = false;
        private int shotId = -1;
        private int beamIndex = -1;

        // ── Trail tracking ──
        private const int TrailLength = 18;
        private Vector2[] trailPositions = new Vector2[TrailLength];
        private float[] trailRotations = new float[TrailLength];
        private bool trailInitialized = false;
        private float ageTimer = 0;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 40;
            Projectile.alpha = 255;
            Projectile.light = 0.5f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.extraUpdates = 2;
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            ageTimer++;

            if (shotId == -1)
            {
                shotId = (int)Projectile.ai[0];
                beamIndex = Projectile.whoAmI;
            }

            // ── Trail position tracking ──
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

            beamEnd = Projectile.Center + Projectile.velocity.SafeNormalize(Vector2.UnitX) * MaxBeamLength;

            if (Projectile.timeLeft <= 20 && !hasReachedEnd)
            {
                hasReachedEnd = true;
                FindAndArcToEnemy();
            }

            Projectile.rotation = Projectile.velocity.ToRotation();

            // ── Particle Spawning ──
            SpawnFlightParticles();
        }

        #region Particle Spawning

        private void SpawnFlightParticles()
        {
            // Funeral flame embers along trail
            if (Main.rand.NextBool(3))
            {
                Vector2 perpendicular = Projectile.velocity.SafeNormalize(Vector2.UnitX).RotatedBy(MathHelper.PiOver2);
                Vector2 flameVel = -Projectile.velocity.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(0.5f, 2f)
                    + perpendicular * Main.rand.NextFloatDirection() * 1.5f
                    + new Vector2(0, -Main.rand.NextFloat(0.3f, 0.8f));

                FuneralParticleHandler.SpawnParticle(new FuneralFlameParticle(
                    Projectile.Center + perpendicular * Main.rand.NextFloatDirection() * 4f,
                    flameVel,
                    Color.Lerp(FuneralUtils.DeepCrimson, FuneralUtils.SmolderingAmber, Main.rand.NextFloat()),
                    Main.rand.NextFloat(0.3f, 0.6f),
                    Main.rand.Next(12, 22)
                ));
            }

            // Prayer ash
            if (Main.rand.NextBool(6))
            {
                FuneralParticleHandler.SpawnParticle(new PrayerAshParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(6f, 6f),
                    new Vector2(Main.rand.NextFloatDirection() * 0.3f, -Main.rand.NextFloat(0.2f, 0.5f)),
                    FuneralUtils.AshGray * 0.5f,
                    Main.rand.NextFloat(0.3f, 0.5f),
                    Main.rand.Next(15, 30)
                ));
            }
        }

        private void SpawnImpactParticles(Vector2 position)
        {
            // Convergence bloom
            Color bloomColor = FuneralUtils.PrayerFlame;
            bloomColor.A = 0;
            FuneralParticleHandler.SpawnParticle(new ConvergenceBloomParticle(
                position, Vector2.Zero, bloomColor, 0.7f, 12
            ));

            // Requiem sparks
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f + Main.rand.NextFloatDirection() * 0.2f;
                FuneralParticleHandler.SpawnParticle(new FuneralSparkParticle(
                    position,
                    angle.ToRotationVector2() * Main.rand.NextFloat(3f, 7f),
                    Color.Lerp(FuneralUtils.DeepCrimson, FuneralUtils.PrayerFlame, Main.rand.NextFloat()),
                    Main.rand.NextFloat(0.3f, 0.6f),
                    Main.rand.Next(8, 16)
                ));
            }

            // Funeral note
            FuneralParticleHandler.SpawnParticle(new FuneralNoteParticle(
                position + new Vector2(Main.rand.NextFloatDirection() * 8f, -4f),
                new Vector2(Main.rand.NextFloatDirection() * 0.5f, -Main.rand.NextFloat(0.5f, 1f)),
                Color.Lerp(FuneralUtils.DeepCrimson, FuneralUtils.RequiemViolet, Main.rand.NextFloat()),
                Main.rand.NextFloat(0.35f, 0.55f),
                Main.rand.Next(30, 50)
            ));

            // Dust burst
            for (int i = 0; i < 6; i++)
            {
                Vector2 dustVel = Main.rand.NextVector2CircularEdge(4f, 4f);
                int dust = Dust.NewDust(position - new Vector2(4), 8, 8,
                    ModContent.DustType<FuneralAshDust>(), dustVel.X, dustVel.Y, 0,
                    FuneralUtils.DeepCrimson, Main.rand.NextFloat(0.6f, 1f));
                Main.dust[dust].noGravity = true;
            }
        }

        #endregion

        private void FindAndArcToEnemy()
        {
            int arcTarget = -1;
            float minDistance = 300f;
            bool foundBoss = false;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && !npc.friendly && npc.boss && !npc.dontTakeDamage)
                {
                    float distance = Vector2.Distance(beamEnd, npc.Center);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        arcTarget = i;
                        foundBoss = true;
                    }
                }
            }

            if (!foundBoss)
            {
                minDistance = 300f;
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (npc.active && !npc.friendly && npc.lifeMax > 5 && !npc.dontTakeDamage)
                    {
                        float distance = Vector2.Distance(beamEnd, npc.Center);
                        if (distance < minDistance)
                        {
                            minDistance = distance;
                            arcTarget = i;
                        }
                    }
                }
            }

            if (arcTarget >= 0 && Main.npc[arcTarget].active)
            {
                NPC target = Main.npc[arcTarget];
                targetNPC = arcTarget;

                target.SimpleStrikeNPC(Projectile.damage, 0, false, 0f, null, false, 0f, true);
                target.AddBuff(ModContent.BuffType<MusicsDissonance>(), 240);

                SoundEngine.PlaySound(SoundID.DD2_LightningBugZap, target.position);

                SpawnImpactParticles(target.Center);

                if (!hasHitEnemy && shotId >= 0)
                {
                    hasHitEnemy = true;
                    // RegisterBeamHit removed ? Funeral Prayer is now a channeled beam weapon
                }

                CreateSecondaryArc(target);
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<MusicsDissonance>(), 240);
            SpawnImpactParticles(target.Center);
            SoundEngine.PlaySound(SoundID.DD2_LightningBugZap, Projectile.position);
            CreateSecondaryArc(target);
        }

        private void CreateSecondaryArc(NPC hitTarget)
        {
            int secondaryTarget = -1;
            float minDistance = 300f;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (i == hitTarget.whoAmI) continue;

                if (npc.active && !npc.friendly && npc.lifeMax > 5)
                {
                    float distance = Vector2.Distance(hitTarget.Center, npc.Center);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        secondaryTarget = i;
                    }
                }
            }

            if (secondaryTarget >= 0 && Main.npc[secondaryTarget].active)
            {
                NPC secondary = Main.npc[secondaryTarget];

                int secondaryDamage = (int)(Projectile.damage * 0.5f);
                secondary.SimpleStrikeNPC(secondaryDamage, 0, false, 0f, null, false, 0f, true);
                secondary.AddBuff(ModContent.BuffType<MusicsDissonance>(), 240);

                SpawnImpactParticles(secondary.Center);

                SoundEngine.PlaySound(SoundID.DD2_LightningAuraZap with { Volume = 0.5f, Pitch = 0.3f }, secondary.position);
            }
        }

        #region Rendering

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            try
            {
            Texture2D tex = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 origin = tex.Size() / 2f;
            float time = (float)Main.gameTimeCache.TotalGameTime.TotalSeconds;

            // Layer 1: GPU Funeral Flame Trail
            DrawFlameTrail(sb);

            // Layer 2: Shader Funeral Beam Strip (replaces Foundation beam body)
            DrawShaderBeamBody(sb, time);

            // Layer 3: Shader RequiemBeam pass
            DrawShaderRequiemOverlay(sb, time);

            // Layer 4: Shader Afterimages
            DrawShaderAfterimages(sb, tex, origin, time);

            // Layer 5: Core beam sprite
            DrawBeamCore(sb, tex, origin);

            // Layer 6: Shader Bloom overlay
            DrawShaderBloomOverlay(sb, origin, time);

            // Eroica theme accent
            EroicaVFXLibrary.BeginEroicaAdditive(sb);
            EroicaVFXLibrary.DrawThemeSakuraAccent(sb, Projectile.Center, 1f, 0.4f);
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

        private void DrawFlameTrail(SpriteBatch sb)
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
                completionRatio => MathHelper.Lerp(6f, 1.5f, completionRatio),
                completionRatio =>
                {
                    float fade = (1f - completionRatio);
                    fade = fade * fade;
                    Color baseCol = Color.Lerp(FuneralUtils.PrayerFlame, FuneralUtils.DeepCrimson, completionRatio * 0.7f);
                    return baseCol * fade * 0.7f;
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

        private void DrawShaderBeamBody(SpriteBatch sb, float time)
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

            Texture2D stripTex = EroicaTextures.EmberScatter?.Value ?? EroicaTextures.EnergyTrailUV?.Value;
            if (stripTex == null) return;

            int texW = stripTex.Width;
            int texH = stripTex.Height;
            float scrollTime = (float)Main.timeForVisualEffects * 0.007f;
            const float BaseBeamWidth = 40f;
            int srcWidth = Math.Max(1, texW / validCount);

            bool hasShader = EroicaShaderManager.HasFuneralTrail;

            if (hasShader)
            {
                // PASS 1: FuneralTrail body
                EroicaShaderManager.BeginShaderAdditive(sb);
                try
                {
                    EroicaShaderManager.ApplyFuneralPrayerBeamTrail(time, glowPass: false);

                    for (int i = 0; i < validCount - 1; i++)
                    {
                        float progress = 1f - (float)i / validCount;
                        float fade = progress * progress;
                        if (fade < 0.01f) continue;

                        float width = BaseBeamWidth * (0.3f + 0.7f * progress);
                        Vector2 segDir = trailPositions[i] - trailPositions[i + 1];
                        float segLength = segDir.Length();
                        if (segLength < 0.5f) continue;
                        float segAngle = segDir.ToRotation();

                        float uStart = ((float)i / validCount + scrollTime * 3f) % 1f;
                        int srcX = (int)(uStart * texW) % texW;
                        Rectangle srcRect = new Rectangle(srcX, 0, srcWidth, texH);

                        float scaleX = segLength / (float)srcWidth;
                        float scaleY = width / (float)texH;
                        Vector2 pos = trailPositions[i] - Main.screenPosition;
                        Vector2 drawOrigin = new Vector2(0, texH / 2f);

                        sb.Draw(stripTex, pos, srcRect, Color.White * (fade * 0.5f), segAngle, drawOrigin,
                            new Vector2(scaleX, scaleY), SpriteEffects.None, 0f);
                    }
                }
                finally
                {
                    EroicaShaderManager.RestoreSpriteBatch(sb);
                }

                // PASS 2: FuneralTrail glow ? wider smolder
                EroicaShaderManager.BeginShaderAdditive(sb);
                try
                {
                    EroicaShaderManager.ApplyFuneralPrayerBeamTrail(time, glowPass: true);

                    for (int i = 0; i < validCount - 1; i++)
                    {
                        float progress = 1f - (float)i / validCount;
                        float fade = progress * progress;
                        if (fade < 0.02f) continue;

                        float width = BaseBeamWidth * (0.3f + 0.7f * progress) * 1.5f;
                        Vector2 segDir = trailPositions[i] - trailPositions[i + 1];
                        float segLength = segDir.Length();
                        if (segLength < 0.5f) continue;
                        float segAngle = segDir.ToRotation();

                        float uStart = ((float)i / validCount + scrollTime * 3f) % 1f;
                        int srcX = (int)(uStart * texW) % texW;
                        Rectangle srcRect = new Rectangle(srcX, 0, srcWidth, texH);

                        float scaleX = segLength / (float)srcWidth;
                        float scaleY = width / (float)texH;
                        Vector2 pos = trailPositions[i] - Main.screenPosition;
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
                // Fallback: palette-colored ribbon
                EroicaShaderManager.BeginAdditive(sb);
                try
                {
                    Texture2D fallbackTex = RBFTextures.EnergySurgeBeam?.Value ?? stripTex;
                    int ftW = fallbackTex.Width;
                    int ftH = fallbackTex.Height;
                    int fSrcWidth = Math.Max(1, ftW / validCount);

                    for (int i = 0; i < validCount - 1; i++)
                    {
                        float progress = 1f - (float)i / validCount;
                        float fade = progress * progress;
                        if (fade < 0.01f) continue;

                        float width = BaseBeamWidth * (0.3f + 0.7f * progress);
                        Vector2 segDir = trailPositions[i] - trailPositions[i + 1];
                        float segLength = segDir.Length();
                        if (segLength < 0.5f) continue;
                        float segAngle = segDir.ToRotation();

                        float uStart = ((float)i / validCount + scrollTime * 3f) % 1f;
                        int srcX = (int)(uStart * ftW) % ftW;
                        Rectangle srcRect = new Rectangle(srcX, 0, fSrcWidth, ftH);
                        float scaleX = segLength / (float)fSrcWidth;
                        float scaleY = width / (float)ftH;
                        Vector2 pos = trailPositions[i] - Main.screenPosition;
                        Vector2 drawOrigin = new Vector2(0, ftH / 2f);

                        Color bodyColor = Color.Lerp(FuneralUtils.DeepCrimson, FuneralUtils.RequiemViolet, 1f - progress) with { A = 0 };
                        sb.Draw(fallbackTex, pos, srcRect, bodyColor * (fade * 0.5f), segAngle, drawOrigin,
                            new Vector2(scaleX, scaleY), SpriteEffects.None, 0f);
                    }
                }
                finally
                {
                    EroicaShaderManager.RestoreSpriteBatch(sb);
                }
            }
        }

        private void DrawShaderRequiemOverlay(SpriteBatch sb, float time)
        {
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            if (EroicaShaderManager.HasRequiemBeam)
            {
                EroicaShaderManager.BeginShaderAdditive(sb);
                try
                {
                    EroicaShaderManager.ApplyFuneralPrayerRequiemBeam(time, glowPass: false);

                    Texture2D ringTex = EroicaTextures.HaloRing?.Value ?? EroicaTextures.SoftCircle?.Value;
                    if (ringTex != null)
                    {
                        float pulse = 0.8f + 0.2f * (float)Math.Sin(ageTimer * 0.18f);
                        float ringScale = 0.5f * pulse;
                        sb.Draw(ringTex, drawPos, null, Color.White * 0.35f,
                            time * 0.4f, ringTex.Size() * 0.5f, ringScale, SpriteEffects.None, 0f);
                    }
                }
                finally
                {
                    EroicaShaderManager.RestoreSpriteBatch(sb);
                }

                EroicaShaderManager.BeginShaderAdditive(sb);
                try
                {
                    EroicaShaderManager.ApplyFuneralPrayerRequiemBeam(time, glowPass: true);

                    Texture2D bloomTex = EroicaTextures.BloomOrb?.Value ?? EroicaTextures.SoftGlow?.Value;
                    if (bloomTex != null)
                    {
                        float glowScale = 0.45f;
                        sb.Draw(bloomTex, drawPos, null, Color.White * 0.2f,
                            0f, bloomTex.Size() * 0.5f, glowScale, SpriteEffects.None, 0f);
                    }
                }
                finally
                {
                    EroicaShaderManager.RestoreSpriteBatch(sb);
                }
            }
        }

        private void DrawShaderAfterimages(SpriteBatch sb, Texture2D tex, Vector2 origin, float time)
        {
            int imageCount = 6;

            if (EroicaShaderManager.HasFuneralTrail)
            {
                EroicaShaderManager.BeginShaderAdditive(sb);
                try
                {
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
                        float alpha = fadeFactor * 0.35f;

                        EroicaShaderManager.ApplyFuneralPrayerBeamTrail(time + i * 0.05f, glowPass: true);

                        sb.Draw(tex, pos, null, Color.White * alpha, rot, origin, 1f - progress * 0.2f, SpriteEffects.None, 0f);
                    }
                }
                finally
                {
                    EroicaShaderManager.RestoreSpriteBatch(sb);
                }
            }
            else
            {
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
                    Color drawColor = Color.Lerp(FuneralUtils.PrayerFlame, FuneralUtils.DeepCrimson, progress) * (fadeFactor * 0.35f);
                    drawColor.A = 0;

                    sb.Draw(tex, pos, null, drawColor, rot, origin, 1f - progress * 0.2f, SpriteEffects.None, 0f);
                }
                FuneralUtils.ExitShaderRegion(sb);
            }
        }

        private void DrawBeamCore(SpriteBatch sb, Texture2D tex, Vector2 origin)
        {
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Color coreTint = FuneralUtils.PrayerFlame;

            sb.Draw(tex, drawPos, null, coreTint, Projectile.rotation, origin, 1f, SpriteEffects.None, 0f);

            Color coreGlow = FuneralUtils.SoulWhite;
            coreGlow.A = 0;
            sb.Draw(tex, drawPos, null, coreGlow * 0.4f, Projectile.rotation, origin, 0.8f, SpriteEffects.None, 0f);
        }

        private void DrawShaderBloomOverlay(SpriteBatch sb, Vector2 origin, float time)
        {
            Texture2D tex = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float pulse = (float)Math.Sin(ageTimer * 0.2f) * 0.1f;

            if (EroicaShaderManager.HasFuneralTrail)
            {
                EroicaShaderManager.BeginShaderAdditive(sb);
                try
                {
                    EroicaShaderManager.ApplyFuneralPrayerBeamTrail(time, glowPass: true);
                    sb.Draw(tex, drawPos, null, Color.White * 0.35f, Projectile.rotation, origin,
                        1.5f + pulse, SpriteEffects.None, 0f);
                }
                finally
                {
                    EroicaShaderManager.RestoreSpriteBatch(sb);
                }
            }
            else
            {
                Color bloomColor = FuneralUtils.SmolderingAmber;
                bloomColor.A = 0;
                FuneralUtils.EnterShaderRegion(sb);
                sb.Draw(tex, drawPos, null, bloomColor * 0.35f, Projectile.rotation, origin,
                    1.5f + pulse, SpriteEffects.None, 0f);
                FuneralUtils.ExitShaderRegion(sb);
            }

            Texture2D ringTex = EroicaThemeTextures.ERInfernalBeamRing;
            if (ringTex != null)
            {
                Vector2 ringOrigin = ringTex.Size() * 0.5f;
                float ringPulse = 0.7f + (float)Math.Sin(ageTimer * 0.25f) * 0.3f;
                Color ringCol = FuneralUtils.SmolderingAmber with { A = 0 };
                sb.Draw(ringTex, drawPos, null, ringCol * (0.2f * ringPulse), Projectile.rotation, ringOrigin,
                    0.35f + pulse * 0.1f, SpriteEffects.None, 0f);
            }

            Texture2D waveTex = EroicaThemeTextures.ERHarmonicImpact;
            if (waveTex != null)
            {
                Vector2 waveOrigin = waveTex.Size() * 0.5f;
                float wavePulse = 0.6f + (float)Math.Sin(ageTimer * 0.18f) * 0.4f;
                Color waveCol = Color.Lerp(FuneralUtils.SmolderingAmber, FuneralUtils.PrayerFlame, wavePulse) with { A = 0 };
                sb.Draw(waveTex, drawPos, null, waveCol * (0.15f * wavePulse), Projectile.rotation + MathHelper.PiOver4, waveOrigin,
                    0.4f, SpriteEffects.None, 0f);
            }
        }

        #endregion
    }
}
