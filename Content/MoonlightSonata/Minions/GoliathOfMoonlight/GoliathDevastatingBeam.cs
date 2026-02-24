using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using MagnumOpus.Content.MoonlightSonata.Debuffs;
using MagnumOpus.Content.MoonlightSonata.Minions;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.VFX.Core;

namespace MagnumOpus.Content.MoonlightSonata.Projectiles
{
    /// <summary>
    /// Goliath Devastating Beam - A Last Prism-style devastating beam.
    /// Fires as a continuous beam that creates ricochet explosions between enemies.
    /// VFX: Bloom-node rendered beam body, GoliathVFX-themed explosions,
    /// MoonlightLightning ricochet chains.
    /// </summary>
    public class GoliathDevastatingBeam : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/GlowingHalo2";

        private const float MaxBeamLength = 2000f;
        private const int BeamDuration = 90;
        private const int ExplosionInterval = 10;

        public float BeamLength
        {
            get => Projectile.ai[0];
            set => Projectile.ai[0] = value;
        }

        public float BeamTimer
        {
            get => Projectile.ai[1];
            set => Projectile.ai[1] = value;
        }

        private int explosionTimer = 0;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.MinionShot[Type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = 18;
            Projectile.height = 18;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.penetrate = -1;
            Projectile.timeLeft = BeamDuration;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 8;
        }

        public override void AI()
        {
            BeamTimer++;

            // Find the owning Goliath minion to track position
            Player owner = Main.player[Projectile.owner];
            Projectile goliath = null;

            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile p = Main.projectile[i];
                if (p.active && p.owner == Projectile.owner && p.type == ModContent.ProjectileType<GoliathOfMoonlight>())
                {
                    goliath = p;
                    break;
                }
            }

            // Update beam origin to follow Goliath
            if (goliath != null && goliath.active)
            {
                Projectile.Center = goliath.Center;
            }

            // Calculate beam length - starts small, grows rapidly
            float progress = Math.Min(BeamTimer / 20f, 1f);
            BeamLength = MaxBeamLength * progress;

            // Beam body VFX via GoliathVFX
            if (!Main.dedServ)
            {
                Vector2 beamEnd = Projectile.Center + Projectile.velocity * BeamLength;
                GoliathVFX.BeamBodyParticles(Projectile.Center, beamEnd);
            }

            // Handle ricochet explosions
            explosionTimer++;
            if (explosionTimer >= ExplosionInterval)
            {
                explosionTimer = 0;
                CreateRicochetExplosions();
            }

            // Beam wobble for visual effect
            float wobble = MathF.Sin(BeamTimer * 0.3f) * 0.02f;
            Projectile.velocity = Projectile.velocity.RotatedBy(wobble);
            Projectile.velocity.Normalize();

            // Intense lighting along beam
            for (float i = 0; i < BeamLength; i += 50f)
            {
                Vector2 lightPos = Projectile.Center + Projectile.velocity * i;
                Lighting.AddLight(lightPos, MoonlightVFXLibrary.Violet.ToVector3() * 0.9f);
            }

            // Sound effects
            if (BeamTimer == 1)
            {
                SoundEngine.PlaySound(SoundID.Item122 with { Volume = 1.2f, Pitch = -0.3f }, Projectile.Center);
            }
            if (BeamTimer % 15 == 0)
            {
                SoundEngine.PlaySound(SoundID.Item15 with { Volume = 0.4f, Pitch = 0.5f }, Projectile.Center);
            }

            // Music notes along beam
            if (!Main.dedServ && BeamTimer % 8 == 0)
            {
                float randomDist = Main.rand.NextFloat(100f, BeamLength * 0.8f);
                Vector2 notePos = Projectile.Center + Projectile.velocity * randomDist;
                MoonlightVFXLibrary.SpawnMusicNotes(notePos, 2, 20f, 0.8f, 1.0f, 30);
            }
        }

        private void CreateRicochetExplosions()
        {
            // Find enemies along the beam path
            List<NPC> enemiesInBeam = new List<NPC>();

            foreach (NPC npc in Main.ActiveNPCs)
            {
                if (!npc.CanBeChasedBy(this))
                    continue;

                Vector2 toNPC = npc.Center - Projectile.Center;
                float distAlongBeam = Vector2.Dot(toNPC, Projectile.velocity);

                if (distAlongBeam > 0 && distAlongBeam < BeamLength)
                {
                    Vector2 closestPointOnBeam = Projectile.Center + Projectile.velocity * distAlongBeam;
                    float distToBeam = Vector2.Distance(npc.Center, closestPointOnBeam);

                    if (distToBeam < 80f)
                    {
                        enemiesInBeam.Add(npc);
                    }
                }
            }

            // Create ricochet explosions between enemies
            if (enemiesInBeam.Count >= 1)
            {
                NPC firstEnemy = enemiesInBeam[0];

                if (!Main.dedServ)
                {
                    GoliathVFX.BeamExplosion(firstEnemy.Center);
                }

                SoundEngine.PlaySound(SoundID.Item14 with { Volume = 0.5f, Pitch = 0.6f }, firstEnemy.Center);

                // Ricochet to other enemies
                for (int i = 1; i < enemiesInBeam.Count && i < 5; i++)
                {
                    NPC prevEnemy = enemiesInBeam[i - 1];
                    NPC currEnemy = enemiesInBeam[i];

                    if (!Main.dedServ)
                    {
                        GoliathVFX.BeamRicochetLine(prevEnemy.Center, currEnemy.Center);
                        GoliathVFX.BeamExplosion(currEnemy.Center);
                    }

                    SoundEngine.PlaySound(SoundID.Item14 with { Volume = 0.5f, Pitch = 0.6f }, currEnemy.Center);
                }

                // If only one enemy, create secondary explosions around it
                if (enemiesInBeam.Count == 1)
                {
                    for (int i = 0; i < 2; i++)
                    {
                        Vector2 randomOffset = Main.rand.NextVector2Circular(100f, 100f);

                        if (!Main.dedServ)
                        {
                            GoliathVFX.BeamExplosion(firstEnemy.Center + randomOffset);
                        }
                    }
                }
            }
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float point = 0f;
            Vector2 beamEnd = Projectile.Center + Projectile.velocity * BeamLength;

            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(),
                Projectile.Center, beamEnd, 30f, ref point);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<MusicsDissonance>(), 300);

            // Heal player
            Player owner = Main.player[Projectile.owner];
            if (owner != null && owner.active)
            {
                int healAmount = 5;
                owner.statLife = Math.Min(owner.statLife + healAmount, owner.statLifeMax2);
                owner.HealEffect(healAmount, true);
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;

            // Render the beam body visually using GoliathVFX bloom nodes
            float widthProgress = Math.Min(BeamTimer / 20f, 1f);
            Vector2 beamEnd = Projectile.Center + Projectile.velocity * BeamLength;

            GoliathVFX.DrawBeamBody(sb, Projectile.Center, beamEnd, widthProgress);

            // Impact point bloom at beam endpoint
            GoliathVFX.BeamImpactPoint(beamEnd);

            return false;
        }
    }
}
