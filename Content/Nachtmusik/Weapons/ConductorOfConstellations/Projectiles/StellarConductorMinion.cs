using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.VFX.Core;
using MagnumOpus.Content.Nachtmusik;
using MagnumOpus.Content.Nachtmusik.Weapons.ConductorOfConstellations.Buffs;
using MagnumOpus.Content.Nachtmusik.Weapons.ConductorOfConstellations.Utilities;

namespace MagnumOpus.Content.Nachtmusik.Weapons.ConductorOfConstellations.Projectiles
{
    /// <summary>
    /// Stellar Conductor minion -- scaffold based on BlackSwanFlareProj pattern.
    /// Hovers above player, fires ConductorStarProjectile barrages.
    /// 2 minion slots, no contact damage. IncisorOrbRenderer visuals.
    /// </summary>
    public class StellarConductorMinion : ModProjectile
    {
        private const float HomingRange = 900f;
        private Player Owner => Main.player[Projectile.owner];
        private bool _initialized;
        private VertexStrip _strip;

        private float conductAngle;
        private int attackCooldown;
        private int orchestraTimer;

        public override string Texture => "MagnumOpus/Content/Nachtmusik/Weapons/ConductorOfConstellations/StellarConductorMinion";

        public override void SetStaticDefaults()
        {
            Main.projPet[Type] = true;
            ProjectileID.Sets.MinionSacrificable[Type] = true;
            ProjectileID.Sets.CultistIsResistantTo[Type] = true;
            ProjectileID.Sets.MinionTargettingFeature[Type] = true;
            ProjectileID.Sets.TrailCacheLength[Type] = 16;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 36;
            Projectile.height = 36;
            Projectile.friendly = true;
            Projectile.minion = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.minionSlots = 2f;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
        }

        public override bool? CanCutTiles() => false;

        public override bool MinionContactDamage() => false;

        public override void AI()
        {
            if (!CheckActive(Owner))
                return;

            if (!_initialized)
            {
                _initialized = true;
            }

            conductAngle += 0.015f;
            attackCooldown = Math.Max(0, attackCooldown - 1);
            orchestraTimer++;

            // Hover above player with gentle bob
            float hoverY = (float)Math.Sin(conductAngle * 2f) * 15f;
            Vector2 idealPos = Owner.Center + new Vector2(0, -100f + hoverY);
            Vector2 toIdeal = idealPos - Projectile.Center;
            Projectile.velocity = Vector2.Lerp(Projectile.velocity, toIdeal * 0.08f, 0.1f);

            NPC target = FindTarget(Owner, HomingRange);

            // Regular attack: Fire 3 star projectiles with angular spread
            if (target != null && attackCooldown == 0)
            {
                for (int i = 0; i < 3; i++)
                {
                    float angleOffset = MathHelper.ToRadians(-15f + 15f * i);
                    Vector2 baseDir = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX);
                    Vector2 dir = baseDir.RotatedBy(angleOffset);

                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center + dir * 20f,
                        dir * 16f, ModContent.ProjectileType<ConductorStarProjectile>(), Projectile.damage, Projectile.knockBack, Projectile.owner);
                }

                attackCooldown = 25;
                SoundEngine.PlaySound(SoundID.Item25 with { Pitch = 0.5f, Volume = 0.6f }, Projectile.Center);
            }

            // Periodic orchestra burst -- ring of 8 stars
            if (orchestraTimer % 180 == 0 && target != null)
            {
                for (int i = 0; i < 8; i++)
                {
                    float angle = MathHelper.TwoPi * i / 8f;
                    Vector2 dir = angle.ToRotationVector2();
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center,
                        dir * 12f, ModContent.ProjectileType<ConductorStarProjectile>(), Projectile.damage, Projectile.knockBack, Projectile.owner);
                }
            }

            // Trail dust
            if (Main.rand.NextBool(3))
            {
                int dustType = Main.rand.NextBool() ? DustID.WhiteTorch : DustID.BlueTorch;
                Color dustColor = Main.rand.NextBool() ? new Color(180, 200, 255) : new Color(60, 70, 150);
                Dust d = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(6f, 6f),
                    dustType, -Projectile.velocity * 0.15f + Main.rand.NextVector2Circular(0.5f, 0.5f),
                    0, dustColor, 0.8f);
                d.noGravity = true;
                d.fadeIn = 0.6f;
            }

            // Pulsing light
            float pulse = 1f + 0.15f * (float)Math.Sin(Main.GlobalTimeWrappedHourly * 4f);
            Lighting.AddLight(Projectile.Center, new Vector3(0.3f, 0.35f, 0.6f) * 0.6f * pulse);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            try
            {
                IncisorOrbRenderer.DrawOrbVisuals(sb, Projectile, IncisorOrbRenderer.Nachtmusik, ref _strip);
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
                Color col = Main.rand.NextBool() ? new Color(60, 70, 150) : new Color(180, 200, 255);
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.WhiteTorch, sparkVel, 0, col, 0.3f);
                d.noGravity = true;
            }
            try { NachtmusikVFXLibrary.SpawnMusicNotes(Projectile.Center, 1, 12f, 0.5f, 0.7f, 20); } catch { }
            try { NachtmusikVFXLibrary.SpawnMixedSparkleImpact(Projectile.Center, 0.5f, 4, 4); } catch { }
            try { NachtmusikVFXLibrary.SpawnCelestialSparkles(Projectile.Center, 3, 15f); } catch { }
        }

        private bool CheckActive(Player owner)
        {
            if (owner.dead || !owner.active)
            {
                owner.ClearBuff(ModContent.BuffType<ConductorOfConstellationsBuff>());
                return false;
            }

            if (owner.HasBuff(ModContent.BuffType<ConductorOfConstellationsBuff>()))
                Projectile.timeLeft = 2;

            return true;
        }

        private NPC FindTarget(Player owner, float range)
        {
            if (owner.HasMinionAttackTargetNPC)
            {
                NPC manual = Main.npc[owner.MinionAttackTargetNPC];
                if (manual.active && manual.CanBeChasedBy(Projectile) && Vector2.Distance(owner.Center, manual.Center) < range * 1.5f)
                    return manual;
            }

            NPC closest = null;
            float closestDist = range;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && npc.CanBeChasedBy(Projectile))
                {
                    float dist = Vector2.Distance(owner.Center, npc.Center);
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        closest = npc;
                    }
                }
            }
            return closest;
        }
    }
}
