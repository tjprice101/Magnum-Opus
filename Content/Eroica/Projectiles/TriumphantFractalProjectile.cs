using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.Graphics;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Content.Eroica;
using MagnumOpus.Content.FoundationWeapons.SparkleProjectileFoundation;
using MagnumOpus.Common.Systems.Shaders;

namespace MagnumOpus.Content.Eroica.Projectiles
{
    /// <summary>
    /// Triumphant Fractal projectile  -- homing fractal geometry with lightning flourishes.
    /// Self-contained VFX: GPU trail, lightning arcs, fractal afterimages, impact geometry.
    /// </summary>
    public class TriumphantFractalProjectile : ModProjectile
    {
        private ref float AgeTimer => ref Projectile.ai[1];

        private List<(Vector2 start, Vector2 end, float time)> activeLightning = new();
        private VertexStrip _strip;

        // ── Trail tracking ──
        private const int TrailLength = 22;
        private Vector2[] trailPositions = new Vector2[TrailLength];
        private float[] trailRotations = new float[TrailLength];
        private bool trailInitialized = false;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 16;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
            Main.projFrames[Type] = 1;
        }

        public override void SetDefaults()
        {
            Projectile.width = 8;
            Projectile.height = 8;
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
            float homingRange = 600f;
            float homingStrength = 0.09f;
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
            float pulse = 0.22f + MathF.Sin(AgeTimer * 0.3f) * 0.018f;
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
            try
            {
            IncisorOrbRenderer.DrawOrbVisuals(Main.spriteBatch, Projectile, IncisorOrbRenderer.Eroica, ref _strip);
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

        #endregion
    }
}
