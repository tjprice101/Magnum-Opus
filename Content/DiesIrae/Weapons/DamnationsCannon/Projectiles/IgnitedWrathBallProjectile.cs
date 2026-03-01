using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.DiesIrae.Weapons.DamnationsCannon.Utilities;
using MagnumOpus.Content.DiesIrae.Weapons.DamnationsCannon.Particles;

namespace MagnumOpus.Content.DiesIrae.Weapons.DamnationsCannon.Projectiles
{
    /// <summary>
    /// Ignited Wrath Ball — A massive fiery projectile that explodes on impact,
    /// spawning 5 orbiting shrapnel pieces that seek enemies.
    /// Multi-layered glow rendering with smoke trail.
    /// </summary>
    public class IgnitedWrathBallProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/PointBloom";

        private static Asset<Texture2D> bloomTexture;
        private const int TrailLength = 10;
        private Vector2[] trailCache = new Vector2[TrailLength];

        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 30;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 180;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = false;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }

        public override void AI()
        {
            for (int i = TrailLength - 1; i > 0; i--) trailCache[i] = trailCache[i - 1];
            trailCache[0] = Projectile.Center;

            Projectile.rotation += 0.1f;
            Projectile.velocity.Y += 0.08f; // Slight gravity arc

            // Smoke trail
            if (Main.rand.NextBool(2))
            {
                Vector2 vel = -Projectile.velocity * 0.05f + Main.rand.NextVector2Circular(1f, 1f);
                DamnationParticleHandler.Spawn(new CannonSmokeParticle(Projectile.Center, vel, 0.5f, 30));
            }

            // Ember trail
            if (Main.rand.NextBool(2))
            {
                Vector2 vel = -Projectile.velocity * 0.08f + Main.rand.NextVector2Circular(1.5f, 1.5f);
                Color c = DamnationUtils.MulticolorLerp(Main.rand.NextFloat(),
                    DamnationUtils.DamnationRed, DamnationUtils.WrathOrange, DamnationUtils.ExplosionGold);
                DamnationParticleHandler.Spawn(new ShrapnelSparkParticle(Projectile.Center, vel, c, 0.2f, 15));
            }

            Lighting.AddLight(Projectile.Center, DamnationUtils.WrathOrange.ToVector3() * 0.6f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Explode(target.Center);
        }

        public override void OnKill(int timeLeft)
        {
            Explode(Projectile.Center);
        }

        private void Explode(Vector2 pos)
        {
            // Massive explosion bloom
            DamnationParticleHandler.Spawn(new ExplosionBloomParticle(pos, DamnationUtils.WrathOrange, 3f, 25));
            DamnationParticleHandler.Spawn(new ExplosionBloomParticle(pos, DamnationUtils.DetonationWhite, 1.2f, 15));

            // Shrapnel sparks
            for (int i = 0; i < 20; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(8f, 8f);
                Color c = DamnationUtils.GetDamnationColor(Main.rand.NextFloat());
                DamnationParticleHandler.Spawn(new ShrapnelSparkParticle(pos, vel, c, 0.25f, 20));
            }

            // Heavy smoke
            for (int i = 0; i < 8; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(4f, 4f);
                DamnationParticleHandler.Spawn(new CannonSmokeParticle(pos, vel, 0.7f, 40));
            }

            // Music notes
            for (int i = 0; i < 5; i++)
            {
                Vector2 vel = new Vector2(Main.rand.NextFloat(-3f, 3f), Main.rand.NextFloat(-3f, -0.5f));
                DamnationParticleHandler.Spawn(new DamnationNoteParticle(pos, vel,
                    DamnationUtils.MulticolorLerp(Main.rand.NextFloat(), DamnationUtils.DamnationRed, DamnationUtils.ExplosionGold),
                    0.5f, 40));
            }

            // Spawn 5 orbiting shrapnel pieces
            if (Main.myPlayer == Projectile.owner)
            {
                for (int i = 0; i < 5; i++)
                {
                    float angle = MathHelper.TwoPi * i / 5f;
                    Vector2 vel = angle.ToRotationVector2() * 6f;
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), pos, vel,
                        ModContent.ProjectileType<WrathShrapnelProjectile>(), Projectile.damage / 2, 3f, Projectile.owner,
                        ai0: angle);
                }
            }

            SoundEngine.PlaySound(SoundID.Item14 with { Pitch = -0.3f, Volume = 1f }, pos);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            bloomTexture ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftRadialBloom");
            if (!bloomTexture.IsLoaded) return false;
            var tex = bloomTexture.Value;

            // Trail glow — velocity-aligned stretched afterimages
            float velRot = Projectile.velocity.ToRotation();
            for (int i = 1; i < TrailLength; i++)
            {
                if (trailCache[i] == Vector2.Zero) continue;
                float p = i / (float)TrailLength;
                Color c = DamnationUtils.GetDamnationColor(p * 0.5f);
                float trailAlpha = (1f - p) * 0.35f;
                float trailScale = (1f - p) * 0.6f;
                // Elongated along velocity for fiery comet look
                Main.EntitySpriteDraw(tex, trailCache[i] - Main.screenPosition, null,
                    DamnationUtils.Additive(c, trailAlpha), velRot, tex.Size() / 2f,
                    new Vector2(trailScale * 2.2f, trailScale * 0.5f), SpriteEffects.None, 0);
            }

            // Core layers
            float pulse = 0.9f + 0.1f * (float)Math.Sin(Main.GameUpdateCount * 0.2f);
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            // Outer wrath glow
            Main.EntitySpriteDraw(tex, drawPos, null,
                DamnationUtils.Additive(DamnationUtils.WrathOrange, 0.5f * pulse), 0f, tex.Size() / 2f, 1f, SpriteEffects.None, 0);
            // Mid explosion gold
            Main.EntitySpriteDraw(tex, drawPos, null,
                DamnationUtils.Additive(DamnationUtils.ExplosionGold, 0.4f * pulse), 0f, tex.Size() / 2f, 0.6f, SpriteEffects.None, 0);
            // Hot white center
            Main.EntitySpriteDraw(tex, drawPos, null,
                DamnationUtils.Additive(DamnationUtils.DetonationWhite, 0.3f * pulse), 0f, tex.Size() / 2f, 0.3f, SpriteEffects.None, 0);

            // Rotating cross-flare — gives the ball a fiery star appearance
            float flareRot = Projectile.rotation;
            Color flareColor = DamnationUtils.Additive(DamnationUtils.ExplosionGold, 0.25f * pulse);
            Main.EntitySpriteDraw(tex, drawPos, null, flareColor, flareRot, tex.Size() / 2f,
                new Vector2(0.15f, 1.2f * pulse), SpriteEffects.None, 0);
            Main.EntitySpriteDraw(tex, drawPos, null, flareColor, flareRot + MathHelper.PiOver2, tex.Size() / 2f,
                new Vector2(0.15f, 1.2f * pulse), SpriteEffects.None, 0);

            // Dark red underpaint halo — rim glow behind the core
            Main.EntitySpriteDraw(tex, drawPos, null,
                DamnationUtils.Additive(DamnationUtils.DamnationRed, 0.15f), 0f, tex.Size() / 2f, 1.4f, SpriteEffects.None, 0);

            return false;
        }
    }

    /// <summary>
    /// Wrath Shrapnel — Orbiting explosive fragment that seeks enemies.
    /// Starts orbiting the explosion center, then homes to nearby enemies.
    /// </summary>
    public class WrathShrapnelProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/PointBloom";

        private static Asset<Texture2D> bloomTexture;
        private ref float OrbitAngle => ref Projectile.ai[0];
        private int orbitTime = 0;
        private const int OrbitDuration = 30;

        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 2;
            Projectile.timeLeft = 120;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }

        public override void AI()
        {
            orbitTime++;
            Projectile.rotation += 0.15f;

            if (orbitTime < OrbitDuration)
            {
                // Orbit phase: spiral outward
                OrbitAngle += 0.15f;
                float radius = 30f + orbitTime * 1.5f;
                Projectile.velocity = OrbitAngle.ToRotationVector2() * radius * 0.05f;
            }
            else
            {
                // Homing phase
                NPC target = null;
                float closest = 500f * 500f;
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (!npc.active || npc.friendly || npc.dontTakeDamage) continue;
                    float d = Vector2.DistanceSquared(Projectile.Center, npc.Center);
                    if (d < closest) { closest = d; target = npc; }
                }

                if (target != null)
                {
                    Vector2 toTarget = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, toTarget * 14f, 0.08f);
                }
                else
                {
                    Projectile.velocity *= 0.98f;
                }
            }

            // Trail sparks
            if (Main.rand.NextBool(2))
            {
                Vector2 vel = -Projectile.velocity * 0.05f;
                Color c = DamnationUtils.MulticolorLerp(Main.rand.NextFloat(), DamnationUtils.DamnationRed, DamnationUtils.WrathOrange);
                DamnationParticleHandler.Spawn(new ShrapnelSparkParticle(Projectile.Center, vel, c, 0.12f, 10));
            }

            Lighting.AddLight(Projectile.Center, DamnationUtils.WrathOrange.ToVector3() * 0.3f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.OnFire3, 180);
            DamnationParticleHandler.Spawn(new ExplosionBloomParticle(target.Center, DamnationUtils.WrathOrange, 0.8f, 10));
            for (int i = 0; i < 4; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(3f, 3f);
                DamnationParticleHandler.Spawn(new ShrapnelSparkParticle(target.Center, vel, DamnationUtils.ExplosionGold, 0.15f, 12));
            }
        }

        public override void OnKill(int timeLeft)
        {
            DamnationParticleHandler.Spawn(new ExplosionBloomParticle(Projectile.Center, DamnationUtils.DamnationRed, 0.6f, 10));
        }

        public override bool PreDraw(ref Color lightColor)
        {
            bloomTexture ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/PointBloom");
            if (!bloomTexture.IsLoaded) return false;
            var tex = bloomTexture.Value;

            float pulse = 0.8f + 0.2f * (float)Math.Sin(Main.GameUpdateCount * 0.25f + OrbitAngle);
            Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, null,
                DamnationUtils.Additive(DamnationUtils.WrathOrange, 0.5f * pulse), 0f, tex.Size() / 2f, 0.5f, SpriteEffects.None, 0);
            Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, null,
                DamnationUtils.Additive(DamnationUtils.DetonationWhite, 0.3f * pulse), 0f, tex.Size() / 2f, 0.25f, SpriteEffects.None, 0);

            return false;
        }
    }
}
