using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.Fate.Debuffs;

namespace MagnumOpus.Content.Fate.ResonantWeapons.ResonanceOfABygoneReality
{
    /// <summary>
    /// Spectral Slashing Blade — summoned on every 5th Resonance bullet hit.
    /// 3-phase AI: approach (Phase 0) → slash through at 35f (Phase 1) → explode (Phase 2).
    /// 60-frame life, 3 penetrate, localNPCHitCooldown=5, applies DestinyCollapse.
    /// Self-contained VFX — no global VFX system references.
    /// </summary>
    public class ResonanceSpectralBlade : ModProjectile
    {
        public override string Texture => "MagnumOpus/Content/Fate/ResonantWeapons/CodaOfAnnihilation";

        public ref float TargetNPC => ref Projectile.ai[0];
        public ref float Phase => ref Projectile.ai[1];

        private Vector2 slashDirection;
        private Vector2 targetCenter;
        private float pulsePhase;
        private bool hasHitTarget;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 12;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 40;
            Projectile.height = 40;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 3;
            Projectile.timeLeft = 60;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 5;
        }

        public override void AI()
        {
            NPC target = null;
            int targetIdx = (int)TargetNPC;
            if (targetIdx >= 0 && targetIdx < Main.maxNPCs)
                target = Main.npc[targetIdx];

            pulsePhase += 0.18f;

            // === Phase 0: Approach target ===
            if (Phase == 0)
            {
                if (target == null || !target.active || target.dontTakeDamage)
                {
                    Phase = 2;
                    Projectile.timeLeft = 1;
                    return;
                }

                targetCenter = target.Center;
                slashDirection = (targetCenter - Projectile.Center).SafeNormalize(Vector2.UnitX);

                Vector2 approachPos = target.Center - slashDirection * 100f;
                Projectile.velocity = (approachPos - Projectile.Center) * 0.25f;

                if (Vector2.Distance(Projectile.Center, approachPos) < 40f || Projectile.timeLeft < 50)
                {
                    Phase = 1;
                    slashDirection = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX);
                    Projectile.velocity = slashDirection * 35f;

                    SoundEngine.PlaySound(SoundID.Item71 with { Pitch = 0.6f }, Projectile.Center);

                    if (!Main.dedServ)
                    {
                        ResonanceParticleHandler.Spawn(ResonanceParticleType.EchoRing,
                            Projectile.Center, Vector2.Zero, ResonanceUtils.ConstellationSilver, 0.5f, 14);
                    }
                }

                // Approach VFX — building energy
                if (!Main.dedServ && Main.rand.NextBool(3))
                {
                    Color col = ResonanceUtils.GradientLerp(Main.rand.NextFloat());
                    ResonanceParticleHandler.Spawn(ResonanceParticleType.BulletGlow,
                        Projectile.Center, Main.rand.NextVector2Circular(2f, 2f),
                        col * 0.6f, 0.22f, 12);
                }

                Projectile.rotation = slashDirection.ToRotation() + MathHelper.PiOver4;
            }
            // === Phase 1: Slash through at high speed ===
            else if (Phase == 1)
            {
                Projectile.velocity = slashDirection * 35f;
                Projectile.rotation = slashDirection.ToRotation() + MathHelper.PiOver4;

                float distPastTarget = Vector2.Dot(Projectile.Center - targetCenter, slashDirection);

                if (!Main.dedServ)
                {
                    // Heavy slash trail particles
                    for (int i = 0; i < 2; i++)
                    {
                        Vector2 perp = new Vector2(-slashDirection.Y, slashDirection.X);
                        Vector2 offset = perp * Main.rand.NextFloat(-10f, 10f);
                        Color sparkCol = ResonanceUtils.GradientLerp(Main.rand.NextFloat());
                        ResonanceParticleHandler.Spawn(ResonanceParticleType.BladeArc,
                            Projectile.Center + offset, -slashDirection * 4f,
                            sparkCol, 0.28f, 12);
                    }

                    // Memory wisps along slash path
                    if (Main.rand.NextBool(2))
                    {
                        Color wisp = Color.Lerp(ResonanceUtils.NebulaPurple, ResonanceUtils.ConstellationSilver, Main.rand.NextFloat());
                        ResonanceParticleHandler.Spawn(ResonanceParticleType.MemoryWisp,
                            Projectile.Center, Main.rand.NextVector2Circular(1.5f, 1.5f),
                            wisp * 0.5f, 0.2f, 16);
                    }
                }

                // Once 80 units past target, transition to explode
                if (distPastTarget > 80f)
                {
                    Phase = 2;
                    Projectile.timeLeft = 1;
                }
            }
            // Phase 2: Explosion handled in OnKill

            Lighting.AddLight(Projectile.Center, ResonanceUtils.ConstellationSilver.ToVector3() * 0.8f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            hasHitTarget = true;
            target.AddBuff(ModContent.BuffType<DestinyCollapse>(), 90);

            if (Main.dedServ) return;

            // Slash impact VFX
            ResonanceParticleHandler.Spawn(ResonanceParticleType.EchoRing,
                target.Center, Vector2.Zero, ResonanceUtils.CosmicRose, 0.4f, 16);

            for (int i = 0; i < 8; i++)
            {
                float angle = slashDirection.ToRotation() + MathHelper.Lerp(-0.5f, 0.5f, (float)i / 7f);
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 8f);
                Color col = ResonanceUtils.GradientLerp((float)i / 7f);
                ResonanceParticleHandler.Spawn(ResonanceParticleType.MuzzleSpark,
                    target.Center, vel, col, 0.2f, 14);
            }
        }

        public override void OnKill(int timeLeft)
        {
            if (Main.dedServ) return;

            float intensity = hasHitTarget ? 1.0f : 0.6f;

            // Large echo ring
            ResonanceParticleHandler.Spawn(ResonanceParticleType.EchoRing,
                Projectile.Center, Vector2.Zero, ResonanceUtils.ConstellationSilver * intensity, 0.8f, 20);

            // Radial star burst
            int sparkCount = hasHitTarget ? 16 : 10;
            for (int i = 0; i < sparkCount; i++)
            {
                float angle = MathHelper.TwoPi * i / sparkCount;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(5f, 10f);
                Color col = ResonanceUtils.GradientLerp((float)i / sparkCount);
                ResonanceParticleHandler.Spawn(ResonanceParticleType.BulletGlow,
                    Projectile.Center, vel, col * intensity, 0.25f, 18);
            }

            // Memory wisps explosion
            for (int i = 0; i < 8; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(4f, 4f);
                Color col = Color.Lerp(ResonanceUtils.NebulaMist, ResonanceUtils.StarGold, Main.rand.NextFloat());
                ResonanceParticleHandler.Spawn(ResonanceParticleType.MemoryWisp,
                    Projectile.Center, vel, col * 0.6f, 0.2f, 22);
            }

            // Blade arc flash
            ResonanceParticleHandler.Spawn(ResonanceParticleType.BladeArc,
                Projectile.Center, Vector2.Zero, ResonanceUtils.StarGold * intensity, 0.6f, 14);

            SoundEngine.PlaySound(SoundID.Item14 with { Pitch = 0.3f, Volume = 0.7f }, Projectile.Center);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
            Vector2 origin = tex.Size() / 2f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float pulse = 1f + MathF.Sin(pulsePhase) * 0.12f;

            // Trail during slash phase
            if (Phase == 1)
            {
                for (int i = 0; i < Projectile.oldPos.Length; i++)
                {
                    if (Projectile.oldPos[i] == Vector2.Zero) continue;
                    float progress = (float)i / Projectile.oldPos.Length;
                    Color trailColor = ResonanceUtils.GradientLerp(progress) * (1f - progress) * 0.5f;
                    Vector2 trailPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                    sb.Draw(tex, trailPos, null, trailColor, Projectile.oldRot[i], origin, (1f - progress * 0.4f) * pulse, SpriteEffects.None, 0f);
                }
            }

            // Multi-layer celestial bloom
            ResonanceUtils.BeginAdditive(sb);

            float intensityMult = Phase == 1 ? 1.3f : 1f;

            // Outer nebula glow
            sb.Draw(tex, drawPos, null, ResonanceUtils.NebulaPurple * 0.25f * intensityMult, Projectile.rotation, origin, 1.8f * pulse, SpriteEffects.None, 0f);
            // Mid cosmic rose energy
            sb.Draw(tex, drawPos, null, ResonanceUtils.CosmicRose * 0.35f * intensityMult, Projectile.rotation, origin, 1.5f * pulse, SpriteEffects.None, 0f);
            // Inner star gold field
            sb.Draw(tex, drawPos, null, ResonanceUtils.StarGold * 0.3f * intensityMult, Projectile.rotation, origin, 1.25f * pulse, SpriteEffects.None, 0f);

            ResonanceUtils.EndAdditive(sb);

            // Core sword sprite — ghostly tinted
            float ghostAlpha = Phase == 1 ? 0.95f : 0.7f;
            sb.Draw(tex, drawPos, null, Color.White * ghostAlpha, Projectile.rotation, origin, 1f * pulse, SpriteEffects.None, 0f);

            return false;
        }
    }
}
