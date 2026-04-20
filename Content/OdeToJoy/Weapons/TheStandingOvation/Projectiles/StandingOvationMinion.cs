using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.VFX.Core;
using MagnumOpus.Content.OdeToJoy;
using MagnumOpus.Content.OdeToJoy.Weapons.TheStandingOvation.Utilities;

namespace MagnumOpus.Content.OdeToJoy.Weapons.TheStandingOvation.Projectiles
{
    /// <summary>
    /// Standing Ovation minion for TheStandingOvation.
    /// BlackSwanFlareProj scaffold — minion with IncisorOrb rendering + homing AI.
    /// </summary>
    public class StandingOvationMinion : ModProjectile
    {
        private const float HomingRange = 700f;
        private Player Owner => Main.player[Projectile.owner];
        private bool _initialized;
        private VertexStrip _strip;

        // Audience Meter attack system
        private int _attackTimer;
        private int _encoreTimer;

        public override string Texture => "MagnumOpus/Content/OdeToJoy/Weapons/TheStandingOvation/TheStandingOvation";

        public override void SetStaticDefaults()
        {
            Main.projPet[Type] = true;
            ProjectileID.Sets.MinionSacrificable[Type] = true;
            ProjectileID.Sets.MinionTargettingFeature[Type] = true;
            ProjectileID.Sets.TrailCacheLength[Type] = 16;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 28;
            Projectile.height = 28;
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

            if (!_initialized)
            {
                _initialized = true;
            }

            SearchForTargets(owner, out bool foundTarget, out float distanceFromTarget, out Vector2 targetCenter);
            Movement(foundTarget, distanceFromTarget, targetCenter, owner);

            // Mark player as active for ovation system
            var ovationPlayer = owner.GetModPlayer<TheStandingOvationPlayer>();
            ovationPlayer.isActive = true;

            // Audience Meter attack system
            _attackTimer++;
            if (_encoreTimer > 0)
                _encoreTimer--;

            int ovationLevel = ovationPlayer.ovationLevel;

            if (foundTarget)
            {
                int fireInterval = GetFireInterval(ovationLevel);

                // Encore mode: halve the fire rate
                if (_encoreTimer > 0)
                    fireInterval = Math.Max(15, fireInterval / 2);

                if (_attackTimer >= fireInterval)
                {
                    _attackTimer = 0;

                    try
                    {
                        if (ovationLevel >= 10)
                        {
                            // Level 10: fire 3 orbs at once + trigger Encore
                            for (int i = 0; i < 3; i++)
                            {
                                float offsetAngle = (i - 1) * 0.2f;
                                Vector2 toTarget = (targetCenter - Projectile.Center).SafeNormalize(Vector2.UnitX);
                                Vector2 vel = toTarget.RotatedBy(offsetAngle) * 12f;
                                GenericHomingOrbChild.SpawnChild(
                                    Projectile.GetSource_FromThis(), Projectile.Center, vel,
                                    Projectile.damage, Projectile.knockBack, Projectile.owner,
                                    homingStrength: 0.10f,
                                    behaviorFlags: GenericHomingOrbChild.FLAG_PIERCE,
                                    themeIndex: GenericHomingOrbChild.THEME_ODETOJOY,
                                    scaleMult: 1.2f, timeLeft: 150);
                            }
                            _encoreTimer = 300; // 5 seconds of doubled fire rate
                            ovationPlayer.TriggerEncore();
                        }
                        else
                        {
                            FireOvationOrb(targetCenter, ovationLevel);
                        }
                    }
                    catch { }
                }
            }

            Projectile.rotation = Projectile.velocity.ToRotation();

            // Trail dust
            if (Main.rand.NextBool(3))
            {
                int dustType = Main.rand.NextBool() ? DustID.GreenTorch : DustID.GoldFlame;
                Color dustColor = Main.rand.NextBool() ? new Color(90, 200, 60) : new Color(255, 210, 60);
                Dust d = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(6f, 6f),
                    dustType, -Projectile.velocity * 0.15f + Main.rand.NextVector2Circular(0.5f, 0.5f),
                    0, dustColor, 0.8f);
                d.noGravity = true;
                d.fadeIn = 0.6f;
            }

            float pulse = 1f + 0.15f * (float)Math.Sin(Main.GlobalTimeWrappedHourly * 3f);
            Lighting.AddLight(Projectile.Center, new Vector3(0.4f, 0.55f, 0.2f) * 0.35f * pulse);
        }

        /// <summary>Get fire interval based on ovation level.</summary>
        private int GetFireInterval(int ovationLevel)
        {
            if (ovationLevel <= 1) return 60;       // Level 0-1: base
            if (ovationLevel <= 3) return 45;       // Level 2-3: 25%
            if (ovationLevel <= 5) return 45;       // Level 4-5: 50%
            if (ovationLevel <= 8) return 45;       // Level 7-8: 75%
            return 45;                               // Level 9-10: same interval, more features
        }

        /// <summary>Fire orb based on current ovation level.</summary>
        private void FireOvationOrb(Vector2 targetCenter, int ovationLevel)
        {
            Vector2 toTarget = (targetCenter - Projectile.Center).SafeNormalize(Vector2.UnitX);

            float homing = 0.06f;
            int flags = 0;
            float speed = 10f;
            float scaleMult = 0.9f;
            int timeLeft = 100;

            if (ovationLevel >= 4)
            {
                // Level 4-5: add pierce
                flags |= GenericHomingOrbChild.FLAG_PIERCE;
            }

            if (ovationLevel >= 7)
            {
                // Level 7-8: aggressive homing + 30% more speed
                homing = 0.10f;
                speed = 14f;
                scaleMult = 1.0f;
            }

            Vector2 vel = toTarget * speed;
            GenericHomingOrbChild.SpawnChild(
                Projectile.GetSource_FromThis(), Projectile.Center, vel,
                Projectile.damage, Projectile.knockBack, Projectile.owner,
                homingStrength: homing, behaviorFlags: flags,
                themeIndex: GenericHomingOrbChild.THEME_ODETOJOY,
                scaleMult: scaleMult, timeLeft: timeLeft);
        }

        private bool CheckActive(Player owner)
        {
            if (owner.dead || !owner.active)
            {
                owner.ClearBuff(ModContent.BuffType<Buffs.StandingOvationBuff>());
                Projectile.Kill();
                return false;
            }

            if (owner.HasBuff(ModContent.BuffType<Buffs.StandingOvationBuff>()))
                Projectile.timeLeft = 2;

            return true;
        }

        private void SearchForTargets(Player owner, out bool foundTarget, out float distanceFromTarget, out Vector2 targetCenter)
        {
            distanceFromTarget = HomingRange;
            targetCenter = Projectile.position;
            foundTarget = false;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.CanBeChasedBy())
                {
                    float dist = Vector2.Distance(npc.Center, Projectile.Center);
                    if (dist < distanceFromTarget)
                    {
                        distanceFromTarget = dist;
                        targetCenter = npc.Center;
                        foundTarget = true;
                    }
                }
            }
        }

        private void Movement(bool foundTarget, float distanceFromTarget, Vector2 targetCenter, Player owner)
        {
            float speed = 8f;
            float inertia = 20f;

            if (foundTarget)
            {
                Vector2 direction = targetCenter - Projectile.Center;
                direction.Normalize();
                direction *= speed;
                Projectile.velocity = (Projectile.velocity * (inertia - 1) + direction) / inertia;
            }
            else
            {
                float distToOwner = Vector2.Distance(owner.Center, Projectile.Center);
                if (distToOwner > 600f)
                {
                    Projectile.Center = owner.Center;
                }
                else if (distToOwner > 200f)
                {
                    Vector2 direction = owner.Center - Projectile.Center;
                    direction.Normalize();
                    direction *= speed;
                    Projectile.velocity = (Projectile.velocity * (inertia - 1) + direction) / inertia;
                }
                else
                {
                    Projectile.velocity *= 0.95f;
                }
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Vector2 hitPos = target.Center;
            for (int i = 0; i < 6; i++)
            {
                Vector2 sparkVel = Main.rand.NextVector2CircularEdge(4f, 4f);
                Color col = i % 2 == 0 ? new Color(90, 200, 60) : new Color(255, 210, 60);
                Dust d = Dust.NewDustPerfect(hitPos, DustID.GreenTorch, sparkVel, 0, col, 0.5f);
                d.noGravity = true;
            }
            for (int i = 0; i < 2; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(2f, 2f) + new Vector2(0, -1f);
                Dust d = Dust.NewDustPerfect(hitPos + Main.rand.NextVector2Circular(8f, 8f),
                    DustID.GoldFlame, vel, 0, new Color(255, 210, 60), 0.5f);
                d.noGravity = true;
            }
            try { OdeToJoyVFXLibrary.SpawnMusicNotes(hitPos, 1, 12f, 0.4f, 0.7f, 20); } catch { }
            try { OdeToJoyVFXLibrary.SpawnMixedSparkleImpact(hitPos, 0.6f, 4, 4); } catch { }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            try
            {
                IncisorOrbRenderer.DrawOrbVisuals(sb, Projectile, IncisorOrbRenderer.OdeToJoy, ref _strip);
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

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 4; i++)
            {
                Vector2 sparkVel = Main.rand.NextVector2CircularEdge(3f, 3f);
                Color col = Main.rand.NextBool() ? new Color(90, 200, 60) : new Color(255, 210, 60);
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.GreenTorch, sparkVel, 0, col, 0.3f);
                d.noGravity = true;
            }
            try { OdeToJoyVFXLibrary.SpawnMusicNotes(Projectile.Center, 1, 12f, 0.5f, 0.7f, 20); } catch { }
            try { OdeToJoyVFXLibrary.SpawnMixedSparkleImpact(Projectile.Center, 0.5f, 4, 4); } catch { }
            try { OdeToJoyVFXLibrary.SpawnJoyousSparkles(Projectile.Center, 3, 15f); } catch { }
        }
    }
}
