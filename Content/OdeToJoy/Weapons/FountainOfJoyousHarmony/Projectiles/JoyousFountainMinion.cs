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
using MagnumOpus.Content.OdeToJoy.Weapons.FountainOfJoyousHarmony.Utilities;

namespace MagnumOpus.Content.OdeToJoy.Weapons.FountainOfJoyousHarmony.Projectiles
{
    /// <summary>
    /// Joyous Fountain minion for FountainOfJoyousHarmony.
    /// BlackSwanFlareProj scaffold — minion with IncisorOrb rendering + homing AI.
    /// </summary>
    public class JoyousFountainMinion : ModProjectile
    {
        private const float HomingRange = 700f;
        private Player Owner => Main.player[Projectile.owner];
        private bool _initialized;
        private VertexStrip _strip;

        // Healing Artillery attack system
        private int _attackTimer;
        private int _geyserTimer;

        public override string Texture => "MagnumOpus/Content/OdeToJoy/Weapons/FountainOfJoyousHarmony/FountainOfJoyousHarmony";

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

            // Hover above owner instead of chasing
            HoverAboveOwner(owner);

            // Count active minions for fire rate scaling
            int minionCount = CountActiveMinions(owner);
            int fireInterval = Math.Max(30, 50 - minionCount * 5);

            // Search for targets to fire at
            SearchForTargets(owner, out bool foundTarget, out float distanceFromTarget, out Vector2 targetCenter);

            _attackTimer++;
            _geyserTimer++;

            // Fire arcing FountainProjectile toward nearest enemy
            if (foundTarget && _attackTimer >= fireInterval)
            {
                _attackTimer = 0;
                try
                {
                    FireFountainOrb(targetCenter);
                }
                catch { }
            }

            // Geyser: every 900 frames (15s), fire 5 upward fan spread
            if (foundTarget && _geyserTimer >= 900)
            {
                _geyserTimer = 0;
                try
                {
                    for (int i = 0; i < 5; i++)
                    {
                        float spreadAngle = MathHelper.ToRadians(-90f + (i - 2) * 20f); // Fan from -130 to -50 degrees
                        Vector2 geyserVel = spreadAngle.ToRotationVector2() * 14f;
                        int projType = ModContent.ProjectileType<FountainProjectile>();
                        Projectile.NewProjectile(
                            Projectile.GetSource_FromThis(), Projectile.Center, geyserVel,
                            projType, Projectile.damage, Projectile.knockBack, Projectile.owner);
                    }
                }
                catch { }
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

        /// <summary>Hover above the owner with a gentle sine bob.</summary>
        private void HoverAboveOwner(Player owner)
        {
            Vector2 targetPos = owner.Center + new Vector2(0, -120f);
            // Gentle sine bob
            targetPos.Y += MathF.Sin((float)Main.timeForVisualEffects * 0.03f) * 8f;

            Vector2 diff = targetPos - Projectile.Center;
            float dist = diff.Length();

            if (dist > 600f)
            {
                // Teleport if too far
                Projectile.Center = targetPos;
                Projectile.velocity = Vector2.Zero;
            }
            else if (dist > 5f)
            {
                // Smooth approach
                Projectile.velocity = diff * 0.08f;
            }
            else
            {
                Projectile.velocity *= 0.9f;
            }
        }

        /// <summary>Fire an arcing FountainProjectile toward a target.</summary>
        private void FireFountainOrb(Vector2 targetCenter)
        {
            Vector2 toTarget = (targetCenter - Projectile.Center).SafeNormalize(Vector2.UnitX);
            float distance = Vector2.Distance(Projectile.Center, targetCenter);
            float speed = MathHelper.Clamp(distance * 0.02f, 8f, 16f);
            // Add upward arc component
            Vector2 vel = toTarget * speed + new Vector2(0, -4f);

            int projType = ModContent.ProjectileType<FountainProjectile>();
            Projectile.NewProjectile(
                Projectile.GetSource_FromThis(), Projectile.Center, vel,
                projType, Projectile.damage, Projectile.knockBack, Projectile.owner);
        }

        /// <summary>Count active JoyousFountainMinion projectiles for fire rate scaling.</summary>
        private int CountActiveMinions(Player owner)
        {
            int count = 0;
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile p = Main.projectile[i];
                if (p.active && p.owner == owner.whoAmI && p.type == Projectile.type)
                    count++;
            }
            return count;
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
