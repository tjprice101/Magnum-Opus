using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.OdeToJoy.Weapons.FountainOfJoyousHarmony.Utilities;
using MagnumOpus.Content.OdeToJoy.Weapons.FountainOfJoyousHarmony.Particles;

namespace MagnumOpus.Content.OdeToJoy.Weapons.FountainOfJoyousHarmony.Projectiles
{
    // ═══════════════════════════════════════════════════════════
    // JoyousFountainMinion — stationary fountain sentry (uses minion slots).
    // Sits on the ground where placed. Heals owner 3 HP every 60 frames
    // within 500px. Fires FountainWaterBolt every 30 frames at nearest
    // enemy within 600px. Spawns continuous fountain spray particles.
    // ═══════════════════════════════════════════════════════════
    public class JoyousFountainMinion : ModProjectile
    {
        public override string Texture => "MagnumOpus/Content/OdeToJoy/Weapons/Summon/FountainOfJoyousHarmonyMinion";

        private ref float AttackTimer => ref Projectile.ai[0];
        private ref float HealTimer => ref Projectile.ai[1];

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 1;
            Main.projPet[Projectile.type] = true;
            ProjectileID.Sets.MinionSacrificable[Projectile.type] = true;
            ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true;
            ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = 40;
            Projectile.height = 60;
            Projectile.friendly = true;
            Projectile.minion = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.minionSlots = 1f;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
        }

        public override bool? CanCutTiles() => false;
        public override bool MinionContactDamage() => false;

        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];

            if (!CheckActive(owner))
                return;

            // Stationary — lock in place
            Projectile.velocity = Vector2.Zero;

            // ── HEALING AURA ──
            HealTimer++;
            if (HealTimer >= 60)
            {
                HealTimer = 0;

                float distToOwner = Vector2.Distance(Projectile.Center, owner.Center);
                if (distToOwner <= 500f)
                {
                    owner.statLife += 3;
                    owner.HealEffect(3);
                }

                // Healing VFX — expanding green ring
                if (!Main.dedServ)
                {
                    FountainParticleHandler.SpawnParticle(new HealingAuraParticle(
                        Projectile.Center, 0.3f, 30));

                    // Scatter small healing motes
                    for (int i = 0; i < 4; i++)
                    {
                        float angle = MathHelper.TwoPi * i / 4f;
                        Vector2 moteVel = angle.ToRotationVector2() * Main.rand.NextFloat(1f, 2.5f);
                        FountainParticleHandler.SpawnParticle(new FountainSprayParticle(
                            Projectile.Center + Main.rand.NextVector2Circular(8f, 8f),
                            moteVel,
                            Main.rand.NextFloat(0.1f, 0.18f), 20));
                    }
                }
            }

            // ── ATTACK ──
            AttackTimer++;
            if (AttackTimer >= 30)
            {
                NPC target = FountainUtils.ClosestNPC(Projectile.Center, 600f);
                if (target != null)
                {
                    AttackTimer = 0;

                    Vector2 attackDir = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX);

                    Projectile.NewProjectile(
                        Projectile.GetSource_FromThis(),
                        Projectile.Center + new Vector2(0, -20f), // fire from top of fountain
                        attackDir * 10f,
                        ModContent.ProjectileType<FountainWaterBolt>(),
                        (int)(Projectile.damage * 0.5f),
                        Projectile.knockBack,
                        Projectile.owner
                    );

                    SoundEngine.PlaySound(SoundID.Item21 with { Volume = 0.5f, Pitch = 0.3f }, Projectile.Center);
                }
            }

            // ── CONTINUOUS FOUNTAIN SPRAY VFX ──
            if (!Main.dedServ && Main.GameUpdateCount % 3 == 0)
            {
                // Upward water spray from fountain top
                Vector2 sprayPos = Projectile.Center + new Vector2(Main.rand.NextFloat(-8f, 8f), -25f);
                Vector2 sprayVel = new Vector2(Main.rand.NextFloat(-0.8f, 0.8f), Main.rand.NextFloat(-4f, -2f));
                FountainParticleHandler.SpawnParticle(new FountainSprayParticle(
                    sprayPos, sprayVel, Main.rand.NextFloat(0.15f, 0.28f), Main.rand.Next(25, 40)));

                // Occasional music notes rising
                if (Main.rand.NextBool(4))
                {
                    Vector2 noteVel = new Vector2(Main.rand.NextFloat(-0.6f, 0.6f), -1.5f);
                    FountainParticleHandler.SpawnParticle(new FountainNoteParticle(
                        Projectile.Center + new Vector2(Main.rand.NextFloat(-12f, 12f), -30f),
                        noteVel, Main.rand.NextFloat(0.25f, 0.4f), 50));
                }
            }

            // Golden glow light at fountain base
            Lighting.AddLight(Projectile.Center, FountainUtils.GoldenSpray.ToVector3() * 0.5f);
        }

        private bool CheckActive(Player owner)
        {
            if (owner.dead || !owner.active)
            {
                owner.ClearBuff(ModContent.BuffType<Buffs.JoyousFountainBuff>());
                Projectile.Kill();
                return false;
            }

            if (owner.HasBuff(ModContent.BuffType<Buffs.JoyousFountainBuff>()))
                Projectile.timeLeft = 2;

            return true;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = tex.Size() / 2f;

            // Additive golden glow behind fountain
            SpriteBatch sb = Main.spriteBatch;
            sb.End();
            FountainUtils.BeginAdditive(sb);

            float pulse = 1f + (float)Math.Sin(Main.GameUpdateCount * 0.06f) * 0.06f;
            Color glow = FountainUtils.Additive(FountainUtils.GoldenSpray, 0.3f);
            sb.Draw(tex, drawPos, null, glow, 0f, origin, Projectile.scale * pulse * 1.2f, SpriteEffects.None, 0f);

            sb.End();
            FountainUtils.BeginDefault(sb);

            // Normal draw with lighting
            sb.Draw(tex, drawPos, null, lightColor, 0f, origin, Projectile.scale, SpriteEffects.None, 0f);

            return false;
        }
    }

    // ═══════════════════════════════════════════════════════════
    // FountainWaterBolt — homing water bolt projectile.
    // 16x16, pen 1, timeLeft 180, mild homing 0.04.
    // Applies Poisoned 60 + Wet 300. On hit: spawn 2 PetalSplashProjectile.
    // ═══════════════════════════════════════════════════════════
    public class FountainWaterBolt : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/PointBloom";

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 180;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
        }

        public override void AI()
        {
            // Homing toward nearest enemy
            NPC target = FountainUtils.ClosestNPC(Projectile.Center, 600f);
            if (target != null)
            {
                Vector2 toTarget = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, toTarget * Projectile.velocity.Length(), 0.04f);
            }

            Projectile.rotation = Projectile.velocity.ToRotation();

            // Trail particles
            if (!Main.dedServ && Main.GameUpdateCount % 2 == 0)
            {
                FountainParticleHandler.SpawnParticle(new WaterBoltTrailParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(4f, 4f),
                    -Projectile.velocity * 0.15f + Main.rand.NextVector2Circular(0.5f, 0.5f),
                    Main.rand.NextFloat(0.15f, 0.25f), Main.rand.Next(10, 18)));
            }

            // Lighting
            Lighting.AddLight(Projectile.Center, FountainUtils.AquaGlow.ToVector3() * 0.4f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Poisoned, 60);
            target.AddBuff(BuffID.Wet, 300);

            // Spawn 2 petal splash sub-projectiles
            for (int i = 0; i < 2; i++)
            {
                Vector2 splashVel = Main.rand.NextVector2CircularEdge(4f, 4f);
                Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    Projectile.Center,
                    splashVel,
                    ModContent.ProjectileType<PetalSplashProjectile>(),
                    (int)(Projectile.damage * 0.5f), // 1/4 weapon damage (bolt is already 1/2)
                    Projectile.knockBack * 0.5f,
                    Projectile.owner
                );
            }

            // Impact VFX — petal splash burst
            if (!Main.dedServ)
            {
                for (int i = 0; i < 6; i++)
                {
                    Vector2 petalVel = Main.rand.NextVector2CircularEdge(3f, 3f);
                    FountainParticleHandler.SpawnParticle(new PetalSplashParticle(
                        Projectile.Center, petalVel, Main.rand.NextFloat(0.2f, 0.35f), Main.rand.Next(20, 35)));
                }

                // Water splash glow
                FountainParticleHandler.SpawnParticle(new FountainSprayParticle(
                    Projectile.Center, Vector2.Zero, 0.5f, 15));
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = tex.Size() / 2f;

            SpriteBatch sb = Main.spriteBatch;
            sb.End();
            FountainUtils.BeginAdditive(sb);

            float pulse = 1f + (float)Math.Sin(Main.GameUpdateCount * 0.14f) * 0.1f;

            // Outer aqua glow
            Color outerGlow = FountainUtils.Additive(FountainUtils.AquaGlow, 0.5f);
            sb.Draw(tex, drawPos, null, outerGlow, Projectile.rotation, origin,
                0.5f * pulse, SpriteEffects.None, 0f);

            // Golden core
            Color coreGlow = FountainUtils.Additive(FountainUtils.GoldenSpray, 0.8f);
            sb.Draw(tex, drawPos, null, coreGlow, Projectile.rotation, origin,
                0.25f * pulse, SpriteEffects.None, 0f);

            sb.End();
            FountainUtils.BeginDefault(sb);

            return false;
        }
    }

    // ═══════════════════════════════════════════════════════════
    // PetalSplashProjectile — secondary petal projectile on bolt hit.
    // 8x8, pen 1, timeLeft 60, tileCollide false, mild homing 0.03.
    // 1/4 weapon damage. Applies Poisoned 30.
    // ═══════════════════════════════════════════════════════════
    public class PetalSplashProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/PointBloom";

        public override void SetDefaults()
        {
            Projectile.width = 8;
            Projectile.height = 8;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 60;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
        }

        public override void AI()
        {
            // Mild homing
            NPC target = FountainUtils.ClosestNPC(Projectile.Center, 400f);
            if (target != null)
            {
                Vector2 toTarget = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, toTarget * Projectile.velocity.Length(), 0.03f);
            }

            Projectile.rotation += 0.15f;
            Projectile.velocity *= 0.99f;

            // Trail particles
            if (!Main.dedServ && Main.rand.NextBool(3))
            {
                FountainParticleHandler.SpawnParticle(new PetalSplashParticle(
                    Projectile.Center,
                    -Projectile.velocity * 0.08f + Main.rand.NextVector2Circular(0.3f, 0.3f),
                    Main.rand.NextFloat(0.08f, 0.15f), Main.rand.Next(12, 22)));
            }

            Lighting.AddLight(Projectile.Center, FountainUtils.RoseSplash.ToVector3() * 0.2f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Poisoned, 30);

            // Small petal burst on hit
            if (!Main.dedServ)
            {
                for (int i = 0; i < 3; i++)
                {
                    Vector2 burstVel = Main.rand.NextVector2CircularEdge(2f, 2f);
                    FountainParticleHandler.SpawnParticle(new PetalSplashParticle(
                        Projectile.Center, burstVel, Main.rand.NextFloat(0.1f, 0.2f), Main.rand.Next(15, 25)));
                }
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = tex.Size() / 2f;

            SpriteBatch sb = Main.spriteBatch;
            sb.End();
            FountainUtils.BeginAdditive(sb);

            float fade = Projectile.timeLeft / 60f;
            Color col = FountainUtils.Additive(FountainUtils.RoseSplash, fade * 0.7f);
            sb.Draw(tex, drawPos, null, col, Projectile.rotation, origin,
                0.25f, SpriteEffects.None, 0f);

            // Inner bright core
            Color core = FountainUtils.Additive(FountainUtils.FountainWhite, fade * 0.3f);
            sb.Draw(tex, drawPos, null, core, Projectile.rotation, origin,
                0.12f, SpriteEffects.None, 0f);

            sb.End();
            FountainUtils.BeginDefault(sb);

            return false;
        }
    }
}
