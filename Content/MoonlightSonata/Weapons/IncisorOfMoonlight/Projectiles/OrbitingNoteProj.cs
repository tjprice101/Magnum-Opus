using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using static MagnumOpus.Content.MoonlightSonata.Weapons.IncisorOfMoonlight.Utilities.IncisorUtils;

namespace MagnumOpus.Content.MoonlightSonata.Weapons.IncisorOfMoonlight.Projectiles
{
    /// <summary>
    /// Orbiting Note Projectile 窶・spawned during Movement III (Presto Agitato).
    /// Music notes orbit the player for 3 seconds, then home in on the nearest enemy.
    /// Each slash in the flurry adds another orbiting note, building visual intensity.
    /// "A gathering storm of notes, each one a weapon waiting to strike."
    /// </summary>
    public class OrbitingNoteProj : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles Asset Library/Stars/4PointedStarHard";

        private const int OrbitDuration = 180; // 3 seconds
        private const float OrbitRadius = 60f;
        private const float OrbitSpeed = 0.06f;
        private const float HomingSpeed = 14f;
        private const float HomingTurnSpeed = 0.08f;
        private const float MaxHomeDist = 800f;

        private enum NoteState { Orbiting, Homing, Converging }

        private NoteState State
        {
            get => (NoteState)(int)Projectile.ai[0];
            set => Projectile.ai[0] = (int)value;
        }

        /// <summary>Unique orbit offset angle for this note instance.</summary>
        private ref float OrbitAngle => ref Projectile.ai[1];

        private int orbitTimer = 0;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailingMode[Type] = 2;
            ProjectileID.Sets.TrailCacheLength[Type] = 10;
        }

        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.MeleeNoSpeed;
            Projectile.tileCollide = false;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 360;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }

        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];
            orbitTimer++;

            switch (State)
            {
                case NoteState.Orbiting:
                    DoBehavior_Orbit(owner);
                    break;
                case NoteState.Homing:
                    DoBehavior_Home();
                    break;
                case NoteState.Converging:
                    DoBehavior_Converge();
                    break;
            }

            Projectile.rotation += 0.1f;
            Lighting.AddLight(Projectile.Center, new Vector3(0.3f, 0.28f, 0.55f) * 0.6f);

            // Trailing sparkle
            if (Main.rand.NextBool(4))
            {
                Color sparkColor = MulticolorLerp(Main.rand.NextFloat(),
                    new Color(170, 140, 255), new Color(230, 235, 255));
                var spark = new ConstellationSparkParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(4f, 4f),
                    Main.rand.NextVector2Circular(1f, 1f),
                    false, Main.rand.Next(8, 14), Main.rand.NextFloat(0.05f, 0.12f),
                    sparkColor, new Vector2(0.3f, 1.2f), quickShrink: true);
                IncisorParticleHandler.SpawnParticle(spark);
            }
        }

        private void DoBehavior_Orbit(Player owner)
        {
            OrbitAngle += OrbitSpeed;
            Vector2 targetPos = owner.MountedCenter + OrbitAngle.ToRotationVector2() * OrbitRadius;
            Projectile.Center = Vector2.Lerp(Projectile.Center, targetPos, 0.15f);
            Projectile.velocity = Vector2.Zero;

            // After orbit duration, switch to homing
            if (orbitTimer >= OrbitDuration)
            {
                State = NoteState.Homing;
                SoundEngine.PlaySound(SoundID.Item25 with { Pitch = 0.4f, Volume = 0.3f }, Projectile.Center);
            }
        }

        private void DoBehavior_Home()
        {
            // Find nearest enemy
            NPC target = FindClosestNPC(MaxHomeDist);

            if (target != null)
            {
                Vector2 toTarget = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitY);
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, toTarget * HomingSpeed, HomingTurnSpeed);
            }
            else
            {
                // No target 窶・fly forward and die
                if (Projectile.velocity == Vector2.Zero)
                    Projectile.velocity = Main.rand.NextVector2CircularEdge(6f, 6f);
                Projectile.velocity *= 1.02f;
            }

            // Homing trail particles
            if (Main.rand.NextBool(3))
            {
                var mote = new LunarMoteParticle(
                    Projectile.Center, -Projectile.velocity * 0.1f,
                    Main.rand.NextFloat(0.1f, 0.25f),
                    MulticolorLerp(Main.rand.NextFloat(), IncisorPalette),
                    Main.rand.Next(10, 18), 2f, 2.5f, hueShift: 0.01f);
                IncisorParticleHandler.SpawnParticle(mote);
            }
        }

        private void DoBehavior_Converge()
        {
            // Grand Finale convergence 窶・all notes converge on nearest enemy
            NPC target = FindClosestNPC(1200f);

            if (target != null)
            {
                Vector2 toTarget = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitY);
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, toTarget * HomingSpeed * 1.5f, 0.15f);
            }
            else
            {
                Projectile.velocity *= 0.95f;
                if (Projectile.velocity.Length() < 1f)
                    Projectile.Kill();
            }
        }

        /// <summary>
        /// Called by the Grand Finale to make all orbiting notes converge.
        /// </summary>
        public void TriggerConvergence()
        {
            State = NoteState.Converging;
            Projectile.timeLeft = Math.Min(Projectile.timeLeft, 120);
            Projectile.penetrate = -1; // Unlimited pierce during convergence
        }

        private NPC FindClosestNPC(float maxDist)
        {
            NPC closest = null;
            float closestDist = maxDist;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.CanBeChasedBy()) continue;
                float dist = Vector2.Distance(Projectile.Center, npc.Center);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closest = npc;
                }
            }
            return closest;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            SoundEngine.PlaySound(SoundID.Item29 with { Pitch = 0.5f, Volume = 0.4f }, target.Center);

            if (!Main.dedServ)
            {
                for (int i = 0; i < 3; i++)
                {
                    Color sc = MulticolorLerp(Main.rand.NextFloat(), IncisorPalette);
                    var spark = new ConstellationSparkParticle(
                        target.Center, Main.rand.NextVector2Circular(5f, 5f),
                        true, Main.rand.Next(10, 18), Main.rand.NextFloat(0.1f, 0.2f),
                        sc, new Vector2(0.5f, 1.3f));
                    IncisorParticleHandler.SpawnParticle(spark);
                }
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            var starTex = ModContent.Request<Texture2D>(
                "MagnumOpus/Assets/Particles Asset Library/Stars/4PointedStarHard",
                AssetRequestMode.ImmediateLoad).Value;
            var bloomTex = ModContent.Request<Texture2D>(
                "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftRadialBloom",
                AssetRequestMode.ImmediateLoad).Value;

            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float pulse = 0.8f + 0.2f * MathF.Sin(orbitTimer * 0.12f);
            float scale = 0.18f * pulse;

            // Orbit glow halo
            Color haloColor = MulticolorLerp((orbitTimer * 0.01f) % 1f, IncisorPalette) with { A = 0 };
            Main.spriteBatch.Draw(bloomTex, drawPos, null, haloColor * 0.3f,
                0f, bloomTex.Size() * 0.5f, 0.35f, SpriteEffects.None, 0f);

            // Star body
            Color starColor = Color.Lerp(new Color(200, 180, 255), Color.White, pulse * 0.3f) with { A = 0 };
            Main.spriteBatch.Draw(starTex, drawPos, null, starColor * 0.9f,
                Projectile.rotation, starTex.Size() * 0.5f, scale,
                SpriteEffects.None, 0f);

            // Cross star overlay
            Main.spriteBatch.Draw(starTex, drawPos, null, starColor * 0.5f,
                Projectile.rotation + MathHelper.PiOver4, starTex.Size() * 0.5f,
                scale * 0.7f, SpriteEffects.None, 0f);

            // White core
            Main.spriteBatch.Draw(bloomTex, drawPos, null, Color.White with { A = 0 } * 0.4f,
                0f, bloomTex.Size() * 0.5f, 0.1f, SpriteEffects.None, 0f);

            // Afterimage trail
            for (int i = 1; i < Math.Min(Projectile.oldPos.Length, 6); i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) break;
                float fade = 1f - i / 6f;
                Vector2 oldDraw = Projectile.oldPos[i] + Projectile.Size * 0.5f - Main.screenPosition;
                Main.spriteBatch.Draw(starTex, oldDraw, null, haloColor * fade * 0.2f,
                    Projectile.oldRot[i], starTex.Size() * 0.5f, scale * fade * 0.8f,
                    SpriteEffects.None, 0f);
            }

            return false;
        }

        public override void OnKill(int timeLeft)
        {
            if (Main.dedServ) return;
            for (int i = 0; i < 4; i++)
            {
                Color sc = MulticolorLerp(Main.rand.NextFloat(), IncisorPalette);
                var spark = new ConstellationSparkParticle(
                    Projectile.Center, Main.rand.NextVector2Circular(4f, 4f),
                    true, Main.rand.Next(8, 14), Main.rand.NextFloat(0.08f, 0.18f),
                    sc, new Vector2(0.5f, 1.4f), quickShrink: true);
                IncisorParticleHandler.SpawnParticle(spark);
            }
        }
    }
}
