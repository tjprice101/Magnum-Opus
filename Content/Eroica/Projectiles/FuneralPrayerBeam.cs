using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using Terraria.GameContent;
using MagnumOpus.Content.MoonlightSonata.Debuffs;
using MagnumOpus.Content.Eroica.Weapons.FuneralPrayer;
using MagnumOpus.Content.Eroica.Weapons.FuneralPrayer.Utilities;
using MagnumOpus.Content.Eroica.Weapons.FuneralPrayer.Particles;
using MagnumOpus.Content.Eroica.Weapons.FuneralPrayer.Primitives;
using MagnumOpus.Content.Eroica.Weapons.FuneralPrayer.Dusts;
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
        private float beamLength = 0f;
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
            beamLength = MaxBeamLength;

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
                FuneralParticleHandler.SpawnParticle(new RequiemSparkParticle(
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
                    FuneralPrayer.RegisterBeamHit(shotId, beamIndex);
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
            Texture2D tex = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 origin = tex.Size() / 2f;

            // ── Layer 1: GPU Funeral Flame Trail ──
            DrawFlameTrail(sb);

            // ── Layer 2: Afterimage chain ──
            DrawAfterimages(sb, tex, origin);

            // ── Layer 3: Core beam sprite with crimson glow ──
            DrawBeamCore(sb, tex, origin);

            // ── Layer 4: Additive bloom ──
            DrawBloomOverlay(sb, origin);

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

            try
            {
                sb.End();
                FuneralTrailRenderer.RenderTrail(positions, settings);
                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                    DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            }
            catch { }
        }

        private void DrawAfterimages(SpriteBatch sb, Texture2D tex, Vector2 origin)
        {
            int imageCount = 6;
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

        private void DrawBeamCore(SpriteBatch sb, Texture2D tex, Vector2 origin)
        {
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Color coreTint = FuneralUtils.PrayerFlame;

            sb.Draw(tex, drawPos, null, coreTint, Projectile.rotation, origin, 1f, SpriteEffects.None, 0f);

            // Bright inner core
            Color coreGlow = FuneralUtils.SoulWhite;
            coreGlow.A = 0;
            sb.Draw(tex, drawPos, null, coreGlow * 0.4f, Projectile.rotation, origin, 0.8f, SpriteEffects.None, 0f);
        }

        private void DrawBloomOverlay(SpriteBatch sb, Vector2 origin)
        {
            Texture2D tex = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            Color bloomColor = FuneralUtils.SmolderingAmber;
            bloomColor.A = 0;
            float pulse = (float)Math.Sin(ageTimer * 0.2f) * 0.1f;

            FuneralUtils.EnterShaderRegion(sb);
            sb.Draw(tex, drawPos, null, bloomColor * 0.35f, Projectile.rotation, origin, 1.5f + pulse, SpriteEffects.None, 0f);
            FuneralUtils.ExitShaderRegion(sb);
        }

        #endregion
    }
}
