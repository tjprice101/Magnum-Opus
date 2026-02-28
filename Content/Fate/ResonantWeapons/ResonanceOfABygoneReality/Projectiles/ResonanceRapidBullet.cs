using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Fate.ResonantWeapons.ResonanceOfABygoneReality
{
    /// <summary>
    /// Resonance Rapid Bullet — small 8×8 fast cosmic projectile.
    /// extraUpdates=2, 120-frame life, 1 penetrate.
    /// Every 5th hit (per player via ResonancePlayer) spawns a ResonanceSpectralBlade at 2× damage.
    /// Self-contained VFX through own particle system and renderer — no FateCosmicVFX / FateVFXLibrary.
    /// </summary>
    public class ResonanceRapidBullet : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles Asset Library/Stars/4PointedStarSoft";

        private float pulsePhase;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 8;
            Projectile.height = 8;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 120;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.extraUpdates = 2;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
            pulsePhase += 0.15f;

            if (Main.dedServ) return;

            // === COSMIC TRAIL PARTICLES ===
            if (Main.rand.NextBool(2))
            {
                Color trailCol = ResonanceUtils.GradientLerp(Main.rand.NextFloat(0.2f, 0.9f));
                Vector2 trailVel = -Projectile.velocity * 0.06f + Main.rand.NextVector2Circular(0.8f, 0.8f);
                ResonanceParticleHandler.Spawn(ResonanceParticleType.CosmicTrail,
                    Projectile.Center + Main.rand.NextVector2Circular(3f, 3f),
                    trailVel, trailCol * 0.75f, 0.14f, 12);
            }

            // Bullet glow core
            if (Main.rand.NextBool(3))
            {
                ResonanceParticleHandler.Spawn(ResonanceParticleType.BulletGlow,
                    Projectile.Center, Vector2.Zero,
                    ResonanceUtils.CosmicRose * 0.5f, 0.1f, 6);
            }

            // Trailing sparks
            if (Main.rand.NextBool(4))
            {
                Vector2 sparkVel = -Projectile.velocity.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(1f, 2.5f)
                    + Main.rand.NextVector2Circular(1f, 1f);
                ResonanceParticleHandler.Spawn(ResonanceParticleType.MuzzleSpark,
                    Projectile.Center, sparkVel,
                    ResonanceUtils.StarGold * 0.6f, 0.12f, 8);
            }

            // Nebula mist wisps
            if (Main.rand.NextBool(5))
            {
                Color mistCol = Color.Lerp(ResonanceUtils.NebulaMist, ResonanceUtils.NebulaPurple, Main.rand.NextFloat()) * 0.3f;
                ResonanceParticleHandler.Spawn(ResonanceParticleType.MemoryWisp,
                    Projectile.Center + Main.rand.NextVector2Circular(5f, 5f),
                    Main.rand.NextVector2Circular(0.4f, 0.4f),
                    mistCol, 0.13f, 16);
            }

            // Torch dust for baseline visibility
            if (Main.rand.NextBool(3))
            {
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.PinkTorch,
                    -Projectile.velocity.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(0.5f, 2f),
                    0, default, 1.0f);
                d.noGravity = true;
            }

            Lighting.AddLight(Projectile.Center, ResonanceUtils.NebulaPurple.ToVector3() * 0.3f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Main.myPlayer != Projectile.owner) return;

            Player owner = Main.player[Projectile.owner];
            var rp = owner.Resonance();
            rp.HitCounter++;

            // Impact VFX
            SpawnImpactParticles(target.Center);

            // Every 5th hit spawns spectral blade
            if (rp.HitCounter >= 5)
            {
                rp.HitCounter = 0;

                Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    target.Center + Main.rand.NextVector2Circular(50f, 50f),
                    Vector2.Zero,
                    ModContent.ProjectileType<ResonanceSpectralBlade>(),
                    (int)(Projectile.damage * 2f),
                    0f,
                    Projectile.owner,
                    target.whoAmI
                );

                // Major spawn VFX burst
                SpawnBladeSpawnParticles(target.Center);
                SoundEngine.PlaySound(SoundID.Item29 with { Pitch = 0.3f }, target.Center);
            }
        }

        private void SpawnImpactParticles(Vector2 pos)
        {
            if (Main.dedServ) return;

            // Echo ring
            ResonanceParticleHandler.Spawn(ResonanceParticleType.EchoRing,
                pos, Vector2.Zero, ResonanceUtils.CosmicRose * 0.6f, 0.3f, 14);

            // Radial sparks
            for (int i = 0; i < 5; i++)
            {
                float angle = MathHelper.TwoPi * i / 5f + Main.rand.NextFloat(-0.2f, 0.2f);
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 5f);
                Color col = ResonanceUtils.GradientLerp(Main.rand.NextFloat());
                ResonanceParticleHandler.Spawn(ResonanceParticleType.MuzzleSpark,
                    pos, vel, col, 0.15f, 12);
            }

            // Dust burst
            for (int i = 0; i < 4; i++)
            {
                float angle = MathHelper.TwoPi * i / 4f + Main.rand.NextFloat(-0.2f, 0.2f);
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 4f);
                Dust d = Dust.NewDustPerfect(pos, DustID.PinkTorch, vel, 0, default, 1.2f);
                d.noGravity = true;
            }

            Lighting.AddLight(pos, ResonanceUtils.CosmicRose.ToVector3() * 0.5f);
        }

        private void SpawnBladeSpawnParticles(Vector2 pos)
        {
            if (Main.dedServ) return;

            // Large echo ring
            ResonanceParticleHandler.Spawn(ResonanceParticleType.EchoRing,
                pos, Vector2.Zero, ResonanceUtils.ConstellationSilver, 0.7f, 20);

            // Star burst
            for (int i = 0; i < 12; i++)
            {
                float angle = MathHelper.TwoPi * i / 12f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 7f);
                Color col = ResonanceUtils.GradientLerp((float)i / 12f);
                ResonanceParticleHandler.Spawn(ResonanceParticleType.BulletGlow,
                    pos, vel, col * 0.8f, 0.25f, 18);
            }

            // Memory wisps
            for (int i = 0; i < 6; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(3f, 3f);
                Color col = Color.Lerp(ResonanceUtils.NebulaMist, ResonanceUtils.ConstellationSilver, Main.rand.NextFloat());
                ResonanceParticleHandler.Spawn(ResonanceParticleType.MemoryWisp,
                    pos + Main.rand.NextVector2Circular(20f, 20f), vel, col * 0.5f, 0.2f, 22);
            }

            // Blade arc flash
            ResonanceParticleHandler.Spawn(ResonanceParticleType.BladeArc,
                pos, Vector2.Zero, ResonanceUtils.StarGold, 0.6f, 16);

            Lighting.AddLight(pos, ResonanceUtils.StarGold.ToVector3() * 1.0f);
        }

        public override void OnKill(int timeLeft)
        {
            if (Main.dedServ) return;

            // Death burst
            for (int i = 0; i < 4; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(2f, 2f);
                Color col = ResonanceUtils.GradientLerp(Main.rand.NextFloat());
                ResonanceParticleHandler.Spawn(ResonanceParticleType.MuzzleSpark,
                    Projectile.Center, vel, col * 0.6f, 0.15f, 10);
            }

            ResonanceParticleHandler.Spawn(ResonanceParticleType.EchoRing,
                Projectile.Center, Vector2.Zero, ResonanceUtils.NebulaPurple * 0.4f, 0.2f, 10);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
            Vector2 origin = tex.Size() / 2f;
            float pulse = 1f + MathF.Sin(pulsePhase) * 0.15f;

            // Draw old-position trail
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;
                float progress = (float)i / Projectile.oldPos.Length;
                Color trailColor = ResonanceUtils.GradientLerp(progress * 0.8f + 0.2f) * (1f - progress) * 0.5f;
                Vector2 trailPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                float trailScale = (0.2f - progress * 0.1f) * pulse;
                sb.Draw(tex, trailPos, null, trailColor, Projectile.oldRot[i], origin, trailScale, SpriteEffects.None, 0f);
            }

            // Switch to additive for multi-layer bloom
            ResonanceUtils.BeginAdditive(sb);

            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            // Outer nebula glow
            sb.Draw(tex, drawPos, null, ResonanceUtils.NebulaPurple * 0.3f, Projectile.rotation, origin, 0.4f * pulse, SpriteEffects.None, 0f);
            // Mid cosmic rose layer
            sb.Draw(tex, drawPos, null, ResonanceUtils.CosmicRose * 0.6f, Projectile.rotation, origin, 0.28f * pulse, SpriteEffects.None, 0f);
            // Hot white-silver core
            sb.Draw(tex, drawPos, null, ResonanceUtils.ConstellationSilver * 0.8f, Projectile.rotation, origin, 0.15f * pulse, SpriteEffects.None, 0f);

            // Restore normal blend
            ResonanceUtils.EndAdditive(sb);

            return false;
        }
    }
}
