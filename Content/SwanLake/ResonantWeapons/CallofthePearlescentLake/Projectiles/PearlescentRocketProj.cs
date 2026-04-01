using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using MagnumOpus.Content.SwanLake.ResonantWeapons.CallofthePearlescentLake.Utilities;
using MagnumOpus.Content.SwanLake.Debuffs;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.VFX.Core;
using MagnumOpus.Content.SwanLake.ResonantWeapons.CallofthePearlescentLake.Primitives;
using MagnumOpus.Content.SwanLake.ResonantWeapons.CallofthePearlescentLake.Shaders;
using Terraria.Graphics.Shaders;
using ReLogic.Content;

namespace MagnumOpus.Content.SwanLake.ResonantWeapons.CallofthePearlescentLake.Projectiles
{
    /// <summary>
    /// Ranged rocket projectile for Call of the Pearlescent Lake.
    /// Sine-wave wobble flight, homing for tidal/still-waters variants.
    /// On-kill spawns SplashZoneProj + concentric ripple VFX.
    /// Foundation-pattern rendering: SpriteBatch bloom trail, no primitives, no custom particles.
    /// </summary>
    public class PearlescentRocketProj : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Textures/InvisibleProjectile";

        // --- AI Slots ---
        public ref float Timer => ref Projectile.ai[0];
        public ref float WobblePhase => ref Projectile.ai[1];
        public ref float HomingStrength => ref Projectile.ai[2];

        // --- Trail ---
        private const int TrailLength = 18;
        private Vector2[] oldPos = new Vector2[TrailLength];
        private float[] oldRot = new float[TrailLength];
        private VertexStrip _strip;

        // --- Variant flags (set by weapon on spawn via Projectile.localAI) ---
        public bool IsTidalWaters => Projectile.localAI[0] == 1f;
        public bool IsStillWaters => Projectile.localAI[0] == 2f;

        private Player Owner => Main.player[Projectile.owner];
        private float LifeProgress => Timer / 300f;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 16;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 300;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.extraUpdates = 1;
            Projectile.alpha = 0;
        }

        public override void AI()
        {
            Timer++;
            Projectile.rotation = Projectile.velocity.ToRotation();

            // --- Sine-wave wobble ---
            WobblePhase += 0.12f;
            float wobbleOffset = MathF.Sin(WobblePhase) * 3.5f;
            Vector2 perp = Projectile.velocity.SafeNormalize(Vector2.UnitY).RotatedBy(MathHelper.PiOver2);
            Projectile.position += perp * wobbleOffset;

            // --- Homing (tidal / still waters variants) ---
            if (IsTidalWaters || IsStillWaters)
            {
                float homingRange = IsTidalWaters ? 350f : 250f;
                float homingFactor = IsTidalWaters ? 0.04f : 0.025f;

                NPC target = FindClosestNPC(homingRange);
                if (target != null)
                {
                    Vector2 toTarget = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitY);
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, toTarget * Projectile.velocity.Length(), homingFactor);
                }
            }

            // --- Trail recording ---
            for (int i = TrailLength - 1; i > 0; i--)
            {
                oldPos[i] = oldPos[i - 1];
                oldRot[i] = oldRot[i - 1];
            }
            oldPos[0] = Projectile.Center;
            oldRot[0] = Projectile.rotation;

            // --- Ambient dust ---
            if (Timer % 2 == 0)
            {
                Color dustColor = PearlescentUtils.GetRainbow(Timer * 0.02f);
                Dust d = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(6, 6),
                    DustID.WhiteTorch, -Projectile.velocity * 0.3f, 0, dustColor, 0.6f);
                d.noGravity = true;
            }

            // --- Pearlescent shimmer dust (white core sparkle) ---
            if (Timer % 4 == 0)
            {
                Dust s = Dust.NewDustPerfect(Projectile.Center, DustID.TintableDustLighted,
                    Main.rand.NextVector2Circular(2, 2), 0, Color.White, 0.5f);
                s.noGravity = true;
            }

            // --- Lighting ---
            Lighting.AddLight(Projectile.Center, 0.8f, 0.8f, 0.9f);

            // Swan Lake rainbow sparkles
            IncisorOrbRenderer.SpawnSwanLakeRainbowSparkles(Projectile);
        }

        private NPC FindClosestNPC(float range)
        {
            NPC closest = null;
            float bestDist = range;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly || npc.dontTakeDamage) continue;
                float dist = Vector2.Distance(Projectile.Center, npc.Center);
                if (dist < bestDist)
                {
                    bestDist = dist;
                    closest = npc;
                }
            }
            return closest;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<SwansMark>(), 300);

            // Rainbow impact burst
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi / 8f * i;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 6f);
                Color c = PearlescentUtils.GetRainbow(i / 8f);
                Dust d = Dust.NewDustPerfect(target.Center, DustID.WhiteTorch, vel, 0, c, 1.0f);
                d.noGravity = true;
            }

            // VFXLibrary integration — mixed sparkle + reduced counts
            try { SwanLakeVFXLibrary.SpawnMixedSparkleImpact(target.Center, 0.8f, 5, 5); } catch { }
            try { SwanLakeVFXLibrary.SpawnMusicNotes(target.Center, 1, 12f, 0.5f, 0.8f, 20); } catch { }
        }

        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item14, Projectile.Center);

            // Spawn SplashZone AoE
            if (Projectile.owner == Main.myPlayer)
            {
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center,
                    Vector2.Zero, ModContent.ProjectileType<SplashZoneProj>(),
                    Projectile.damage / 3, 0f, Projectile.owner);
            }

            // Concentric ripple dust burst (3 rings)
            for (int ring = 0; ring < 3; ring++)
            {
                int count = 10 + ring * 6;
                float ringSpeed = 2f + ring * 2.5f;
                for (int i = 0; i < count; i++)
                {
                    float angle = MathHelper.TwoPi / count * i;
                    Vector2 vel = angle.ToRotationVector2() * ringSpeed;
                    Color c = Color.Lerp(PearlescentUtils.PearlWhite, PearlescentUtils.GetRainbow(i / (float)count + ring * 0.33f), 0.4f);
                    Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.WhiteTorch, vel, 0, c, 0.8f - ring * 0.15f);
                    d.noGravity = true;
                }
            }

            // Enhanced VFX
            try { SwanLakeVFXLibrary.SpawnRainbowExplosion(Projectile.Center, 1f); } catch { }
            try { SwanLakeVFXLibrary.SpawnPrismaticSparkles(Projectile.Center, 8, 30f); } catch { }
            try { SwanLakeVFXLibrary.SpawnMixedSparkleImpact(Projectile.Center, 0.8f, 6, 6); } catch { }
            try { SwanLakeVFXLibrary.SpawnMusicNotes(Projectile.Center, 4, 20f, 0.7f, 1.0f, 28); } catch { }

            // Screen impact
            for (int i = 0; i < 12; i++)
            {
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.Smoke,
                    Main.rand.NextVector2Circular(5, 5), 100, Color.LightGray, 1.2f);
                d.noGravity = false;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            try
            {
            IncisorOrbRenderer.DrawOrbVisuals(Main.spriteBatch, Projectile, IncisorOrbRenderer.SwanLake, ref _strip);
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
    }
}
