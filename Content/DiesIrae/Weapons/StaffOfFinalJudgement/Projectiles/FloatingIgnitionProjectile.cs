using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.DiesIrae.Weapons.StaffOfFinalJudgement.Utilities;
using MagnumOpus.Content.DiesIrae.Weapons.StaffOfFinalJudgement.Particles;

namespace MagnumOpus.Content.DiesIrae.Weapons.StaffOfFinalJudgement.Projectiles
{
    /// <summary>
    /// Floating Ignition — 5 of these orbit the cursor position, then converge on a target and detonate.
    /// Phase 1 (0-60 ticks): Orbit cursor, track enemies
    /// Phase 2 (60+): Home to nearest enemy and detonate on contact
    /// Massive multi-layered explosion on death.
    /// </summary>
    public class FloatingIgnitionProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/PointBloom";

        private static Asset<Texture2D> bloomTexture;
        private ref float OrbitAngle => ref Projectile.ai[0];
        private ref float OrbIndex => ref Projectile.ai[1];
        private int timer = 0;
        private const int OrbitPhase = 60;
        private Vector2 cursorTarget;

        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 240;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }

        public override void AI()
        {
            timer++;
            Player owner = Main.player[Projectile.owner];
            cursorTarget = (Projectile.owner == Main.myPlayer) ? Main.MouseWorld : owner.Center;

            if (timer < OrbitPhase)
            {
                // Orbit phase: circle around cursor position
                OrbitAngle += 0.08f;
                float indexOffset = OrbIndex * (MathHelper.TwoPi / 5f);
                float radius = 60f + 10f * (float)Math.Sin(timer * 0.05f);
                Vector2 targetPos = cursorTarget + (OrbitAngle + indexOffset).ToRotationVector2() * radius;
                Projectile.velocity = (targetPos - Projectile.Center) * 0.15f;

                // Ambient particles
                if (Main.rand.NextBool(4))
                {
                    Vector2 vel = Main.rand.NextVector2Circular(0.5f, 0.5f);
                    JudgementParticleHandler.Spawn(new IgnitionOrbParticle(Projectile.Center, vel,
                        JudgementUtils.GetJudgmentColor(Main.rand.NextFloat(0.3f, 0.7f)), 0.2f, 10));
                }
            }
            else
            {
                // Homing phase: seek nearest enemy
                NPC target = null;
                float closest = 800f * 800f;
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (!npc.active || npc.friendly || npc.dontTakeDamage) continue;
                    float d = Vector2.DistanceSquared(Projectile.Center, npc.Center);
                    if (d < closest) { closest = d; target = npc; }
                }

                if (target != null)
                {
                    Vector2 dir = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, dir * 16f, 0.1f);
                }
                else
                {
                    Projectile.velocity *= 0.98f;
                }

                // Trailing embers when homing
                if (Main.rand.NextBool(2))
                {
                    Vector2 vel = -Projectile.velocity * 0.05f + Main.rand.NextVector2Circular(1f, 1f);
                    Color c = JudgementUtils.GetJudgmentColor(Main.rand.NextFloat());
                    JudgementParticleHandler.Spawn(new JudgmentEmberParticle(Projectile.Center, vel, c, 0.12f, 12));
                }
            }

            Projectile.rotation += 0.08f;
            Lighting.AddLight(Projectile.Center, JudgementUtils.JudgmentFlame.ToVector3() * 0.4f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.OnFire3, 240);
            Detonate(target.Center);
        }

        public override void OnKill(int timeLeft)
        {
            Detonate(Projectile.Center);
        }

        private void Detonate(Vector2 pos)
        {
            // Large bloom
            JudgementParticleHandler.Spawn(new JudgmentDetonationParticle(pos, JudgementUtils.JudgmentFlame, 2f, 20));
            JudgementParticleHandler.Spawn(new JudgmentDetonationParticle(pos, JudgementUtils.DetonationGold, 1f, 12));

            // Spark shower
            for (int i = 0; i < 12; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(6f, 6f);
                Color c = JudgementUtils.GetJudgmentColor(Main.rand.NextFloat());
                JudgementParticleHandler.Spawn(new JudgmentEmberParticle(pos, vel, c, 0.18f, 18));
            }

            // Smoke
            for (int i = 0; i < 4; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(2f, 2f);
                JudgementParticleHandler.Spawn(new JudgmentSmokeParticle(pos, vel, 0.5f, 25));
            }

            // Music notes
            for (int i = 0; i < 3; i++)
            {
                Vector2 vel = new Vector2(Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-2f, -0.5f));
                JudgementParticleHandler.Spawn(new JudgmentNoteParticle(pos, vel,
                    JudgementUtils.GetJudgmentColor(Main.rand.NextFloat(0.4f, 0.8f)), 0.4f, 35));
            }

            SoundEngine.PlaySound(SoundID.Item45 with { Pitch = 0.2f }, pos);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            bloomTexture ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftRadialBloom");
            if (!bloomTexture.IsLoaded) return false;
            var tex = bloomTexture.Value;

            float pulse = 0.8f + 0.2f * (float)Math.Sin(Main.GameUpdateCount * 0.2f + OrbIndex * 1.3f);
            float scale = timer < OrbitPhase ? 0.4f : 0.5f;

            // Outer glow
            Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, null,
                JudgementUtils.Additive(JudgementUtils.WrathCrimson, 0.3f * pulse), 0f, tex.Size() / 2f, scale * 1.5f, SpriteEffects.None, 0);
            // Mid glow
            Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, null,
                JudgementUtils.Additive(JudgementUtils.JudgmentFlame, 0.5f * pulse), 0f, tex.Size() / 2f, scale, SpriteEffects.None, 0);
            // Core
            Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, null,
                JudgementUtils.Additive(JudgementUtils.DivineWhite, 0.3f * pulse), 0f, tex.Size() / 2f, scale * 0.3f, SpriteEffects.None, 0);

            return false;
        }
    }
}
