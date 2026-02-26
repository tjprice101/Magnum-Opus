using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using MagnumOpus.Content.MoonlightSonata.Debuffs;
using MagnumOpus.Content.MoonlightSonata.Dusts;
using MagnumOpus.Content.MoonlightSonata.Minions;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.VFX.Core;
using MagnumOpus.Common.Systems.VFX.Trails;

namespace MagnumOpus.Content.MoonlightSonata.Projectiles
{
    /// <summary>
    /// Goliath Moonlight Beam - A small, fast dark purple beam that ricochets.
    /// VFX: CalamityStyleTrailRenderer trail, {A=0} bloom body, GoliathVFX-themed impacts.
    /// </summary>
    public class GoliathMoonlightBeam : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/MagicSparklField6";

        private const int MaxRicochets = 10;
        private const float RicochetRange = 500f;
        private const float BeamSpeed = 20f;

        private int ricochetCount = 0;
        private int lastHitNPC = -1;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.MinionShot[Type] = true;
            ProjectileID.Sets.TrailCacheLength[Type] = 20;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 6;
            Projectile.height = 6;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.penetrate = MaxRicochets + 1;
            Projectile.timeLeft = 300;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.extraUpdates = 3;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();

            // Themed trail dust — GravityWellDust cosmic wake
            if (!Main.dedServ)
            {
                Color dustColor = Color.Lerp(GoliathVFX.GravityWell, GoliathVFX.EnergyTendril, Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(3f, 3f),
                    ModContent.DustType<GravityWellDust>(),
                    -Projectile.velocity * 0.08f, 0, dustColor, 0.18f);
                d.customData = new GravityWellBehavior
                {
                    GravityCenter = Vector2.Zero,
                    PullStrength = 0f,
                    BaseScale = 0.18f,
                    Lifetime = 16,
                    VelocityDecay = 0.95f
                };

                // Music notes (sparse)
                if (Main.rand.NextBool(12))
                {
                    MoonlightVFXLibrary.SpawnMusicNotes(Projectile.Center, 1, 5f, 0.7f, 0.85f, 22);
                }
            }

            Lighting.AddLight(Projectile.Center, GoliathVFX.NebulaPurple.ToVector3() * 0.5f);

            // Slight homing after a bit
            if (Projectile.timeLeft < 250)
            {
                NPC target = FindNearestEnemy();
                if (target != null && target.whoAmI != lastHitNPC)
                {
                    Vector2 toTarget = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity.SafeNormalize(Vector2.Zero), toTarget, 0.03f) * Projectile.velocity.Length();
                }
            }
        }

        private NPC FindNearestEnemy()
        {
            float closestDist = RicochetRange;
            NPC closest = null;

            foreach (NPC npc in Main.ActiveNPCs)
            {
                if (npc.CanBeChasedBy(this) && npc.whoAmI != lastHitNPC)
                {
                    float dist = Vector2.Distance(Projectile.Center, npc.Center);
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        closest = npc;
                    }
                }
            }

            return closest;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<MusicsDissonance>(), 180);

            // Heal the player
            Player owner = Main.player[Projectile.owner];
            if (owner != null && owner.active)
            {
                int healAmount = 10;
                owner.statLife = Math.Min(owner.statLife + healAmount, owner.statLifeMax2);
                owner.HealEffect(healAmount, true);
            }

            // Hit explosion via GoliathVFX
            if (!Main.dedServ)
            {
                GoliathVFX.SmallBeamHitExplosion(target.Center, ricochetCount);
            }

            SoundEngine.PlaySound(SoundID.Item14 with { Volume = 0.4f, Pitch = 0.8f }, target.Center);

            // Ricochet to next target
            ricochetCount++;
            lastHitNPC = target.whoAmI;

            if (ricochetCount <= MaxRicochets)
            {
                NPC nextTarget = FindNearestEnemy();
                if (nextTarget != null)
                {
                    Vector2 newDirection = (nextTarget.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                    Projectile.velocity = newDirection * BeamSpeed;
                    Projectile.netUpdate = true;

                    // Small ricochet dust burst — StarPointDust sparks
                    if (!Main.dedServ)
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            Vector2 vel = newDirection.RotatedByRandom(0.5f) * Main.rand.NextFloat(2f, 5f);
                            Color sparkCol = Color.Lerp(GoliathVFX.StarCore, GoliathVFX.EnergyTendril, Main.rand.NextFloat());
                            Dust spark = Dust.NewDustPerfect(Projectile.Center,
                                ModContent.DustType<StarPointDust>(),
                                vel, 0, sparkCol, 0.18f);
                            spark.customData = new StarPointBehavior
                            {
                                RotationSpeed = 0.15f,
                                TwinkleFrequency = 0.5f,
                                Lifetime = 14,
                                FadeStartTime = 4
                            };
                        }
                    }

                    SoundEngine.PlaySound(SoundID.Item12 with { Volume = 0.25f, Pitch = 0.8f + ricochetCount * 0.05f }, Projectile.Center);
                }
            }
        }

        public override void OnKill(int timeLeft)
        {
            if (!Main.dedServ)
            {
                GoliathVFX.SmallBeamDeath(Projectile.Center);
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;

            // === CalamityStyleTrailRenderer trail ===
            if (Projectile.oldPos.Length > 0 && Projectile.oldPos[0] != Vector2.Zero)
            {
                // Build positions array
                Vector2[] positions = new Vector2[Projectile.oldPos.Length + 1];
                float[] rotations = new float[positions.Length];
                positions[0] = Projectile.Center;
                rotations[0] = Projectile.rotation;

                int validCount = 1;
                for (int i = 0; i < Projectile.oldPos.Length; i++)
                {
                    if (Projectile.oldPos[i] == Vector2.Zero) break;
                    positions[validCount] = Projectile.oldPos[i] + Projectile.Size / 2f;
                    rotations[validCount] = Projectile.oldRot.Length > i ? Projectile.oldRot[i] : Projectile.rotation;
                    validCount++;
                }

                if (validCount > 2)
                {
                    Array.Resize(ref positions, validCount);
                    Array.Resize(ref rotations, validCount);

                    float trailWidth = 8f + ricochetCount * 0.5f;

                    // Cosmic palette trail — shifts from gravity well to star core with ricochets
                    Color primaryTrail = Color.Lerp(GoliathVFX.NebulaPurple, GoliathVFX.EnergyTendril, ricochetCount * 0.08f);
                    Color secondaryTrail = Color.Lerp(GoliathVFX.EnergyTendril, GoliathVFX.StarCore, ricochetCount * 0.08f);

                    CalamityStyleTrailRenderer.DrawTrailWithBloom(
                        positions, rotations,
                        CalamityStyleTrailRenderer.TrailStyle.Ice,
                        trailWidth,
                        primaryTrail,
                        secondaryTrail,
                        0.8f,
                        2.0f);
                }
            }

            // === Body bloom via GoliathVFX ===
            GoliathVFX.DrawSmallBeamBloom(sb, Projectile.Center, ricochetCount);

            return false;
        }
    }
}
