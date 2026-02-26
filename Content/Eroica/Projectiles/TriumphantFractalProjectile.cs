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
using MagnumOpus.Content.Eroica;
using MagnumOpus.Content.Eroica.Weapons.TriumphantFractal;

namespace MagnumOpus.Content.Eroica.Projectiles
{
    /// <summary>
    /// Triumphant Fractal projectile — homing fractal geometry with lightning flourishes.
    /// Game logic (homing, hit, death) lives here; ALL visuals delegated to TriumphantFractalVFX.
    /// </summary>
    public class TriumphantFractalProjectile : ModProjectile
    {
        /// <summary>Frame counter for lightning timing and animation.</summary>
        private ref float AgeTimer => ref Projectile.ai[1];

        private List<(Vector2 start, Vector2 end, float time)> activeLightning = new();

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 15;
            ProjectileID.Sets.TrailingMode[Type] = 2;
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

            // ─── Homing ───
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

            // ─── Lightning bolt management ───
            if ((int)AgeTimer % 8 == 0)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 lightningEnd = Projectile.Center + angle.ToRotationVector2() * Main.rand.NextFloat(40f, 80f);
                activeLightning.Add((Projectile.Center, lightningEnd, 0f));
            }

            activeLightning.RemoveAll(l => l.time > 10f);
            for (int i = 0; i < activeLightning.Count; i++)
            {
                var l = activeLightning[i];
                activeLightning[i] = (l.start, l.end, l.time + 1f);
            }

            // ─── Delegate ALL trail VFX to VFX module ───
            TriumphantFractalVFX.ProjectileTrailVFX(Projectile);

            // Pulsating scale
            float pulse = 1f + MathF.Sin(AgeTimer * 0.3f) * 0.08f;
            Projectile.scale = pulse;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // ─── Delegate hit VFX to VFX module ───
            TriumphantFractalVFX.ProjectileHitVFX(target.Center);

            // ─── Seeking Crystals — game logic (20% chance, 4 crystals) ───
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

            // ─── Delegate death VFX ───
            EroicaVFXLibrary.DeathHeroicFlash(Projectile.Center, 1.8f);
            TriumphantFractalVFX.ProjectileDeathVFX(Projectile.Center);

            // ─── Fractal lightning dust bolts (game-spawned geometry) ───
            for (int i = 0; i < 5; i++)
            {
                float angle = MathHelper.TwoPi * i / 5f;
                Vector2 direction = angle.ToRotationVector2();
                Vector2 lightningEnd = Projectile.Center + direction * Main.rand.NextFloat(80f, 140f);

                // 8-segment zigzag dust chain
                Vector2 step = (lightningEnd - Projectile.Center) / 8f;
                Vector2 perp = new Vector2(-step.Y, step.X);
                perp.Normalize();
                Vector2 current = Projectile.Center;
                for (int s = 0; s < 8; s++)
                {
                    current += step;
                    float offset = (s % 2 == 0 ? 1f : -1f) * Main.rand.NextFloat(14f, 28f);
                    Vector2 pos = current + perp * offset;
                    Color col = Color.Lerp(new Color(255, 210, 80), new Color(255, 150, 100), (float)s / 8f);
                    Dust bolt = Dust.NewDustPerfect(pos, DustID.GoldFlame, step * 0.15f, 0, col, 1.5f - s * 0.1f);
                    bolt.noGravity = true;
                }
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            // ─── Delegate ALL rendering to VFX module ───
            return TriumphantFractalVFX.DrawFractalProjectile(Main.spriteBatch, Projectile, ref lightColor);
        }
    }
}
