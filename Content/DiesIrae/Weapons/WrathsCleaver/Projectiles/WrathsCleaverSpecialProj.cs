using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MagnumOpus.Common.Systems.VFX;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.DiesIrae.Weapons.WrathsCleaver.Projectiles
{
    /// <summary>
    /// Wrath's Cleaver special orb projectile — "Wrath Escalation" combo system.
    /// ai[0] = combo phase (0-3) or -1 for right-click dash orb.
    ///
    /// Phase 0: 1 orb, straight shot, no homing — the warning
    /// Phase 1: 2 orbs, mild homing (0.06) — judgment approaches
    /// Phase 2: 3 orbs, standard homing (0.08), pierce 1 — judgment weighs
    /// Phase 3: 1 orb, aggressive homing (0.14), on-hit splits into 8 child orbs — judgment rendered
    /// Phase -1: dash orb, pierce all, short life
    /// </summary>
    public class WrathsCleaverSpecialProj : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftGlow";

        private VertexStrip _strip;
        private bool _initialized;
        private int Phase => (int)Projectile.ai[0];

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 16;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 30;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.penetrate = 1;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 120;
            Projectile.DamageType = DamageClass.MeleeNoSpeed;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 15;
        }

        public override void AI()
        {
            if (!_initialized)
            {
                _initialized = true;

                // Apply phase-specific penetrate
                switch (Phase)
                {
                    case 0: // Phase 1: single hit
                    case 1: // Phase 2: single hit
                    case 3: // Phase 4: single hit (splits on hit)
                        Projectile.penetrate = 1;
                        break;
                    case 2: // Phase 3: pierce 1
                        Projectile.penetrate = 2;
                        break;
                    // Phase -1 (dash): penetrate already set to -1 by weapon Shoot()
                }
            }

            // Rotation
            Projectile.rotation += 0.15f;

            // Homing behavior based on phase
            float homingStrength = Phase switch
            {
                1 => 0.06f,
                2 => 0.08f,
                3 => 0.14f,
                _ => 0f // Phase 0 and -1: no homing
            };

            if (homingStrength > 0f)
            {
                NPC target = FindClosestNPC(600f);
                if (target != null)
                {
                    Vector2 idealDir = (target.Center - Projectile.Center).SafeNormalize(
                        Projectile.velocity.SafeNormalize(Vector2.UnitX));
                    Projectile.velocity = Vector2.Lerp(
                        Projectile.velocity,
                        idealDir * Projectile.velocity.Length(),
                        homingStrength);
                }
            }

            // Slight deceleration (original behavior) — but not for dash orbs
            if (Phase != -1)
                Projectile.velocity *= 0.98f;

            // Lighting — brighter for later phases
            float lightIntensity = Phase switch
            {
                3 => 0.8f,
                2 => 0.6f,
                1 => 0.5f,
                -1 => 0.7f,
                _ => 0.4f
            };
            Lighting.AddLight(Projectile.Center, lightIntensity, lightIntensity * 0.4f, lightIntensity * 0.2f);

            // Dust trail — frequency scales with phase
            int dustChance = Phase switch
            {
                3 => 1,   // Every frame
                2 => 2,
                -1 => 2,
                _ => 3
            };
            if (Main.rand.NextBool(dustChance))
            {
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.Torch,
                    Main.rand.NextVector2Circular(2f, 2f));
                d.noGravity = true;
                d.scale = Phase >= 2 ? 1.2f : 0.9f;
            }

            // Phase 3 orb gets extra fire particles for visual intensity
            if (Phase == 3 && Main.rand.NextBool(2))
            {
                Dust fire = Dust.NewDustPerfect(
                    Projectile.Center + Main.rand.NextVector2Circular(8f, 8f),
                    DustID.SolarFlare, -Projectile.velocity * 0.15f, 0, default, 0.7f);
                fire.noGravity = true;
            }

            // Dash orb gets ember trail
            if (Phase == -1 && Main.rand.NextBool(2))
            {
                Dust ember = Dust.NewDustPerfect(Projectile.Center, DustID.Torch,
                    -Projectile.velocity * 0.3f + Main.rand.NextVector2Circular(1.5f, 1.5f),
                    0, default, 1.1f);
                ember.noGravity = true;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            try
            {
                IncisorOrbRenderer.DrawOrbVisuals(sb, Projectile, IncisorOrbRenderer.DiesIrae, ref _strip);
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

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Phase 4 (ai[0]=3): on hit, split into 8 child orbs — judgment rendered
            if (Phase == 3 && Projectile.owner == Main.myPlayer)
            {
                SoundEngine.PlaySound(SoundID.Item45 with { Pitch = 0.2f, Volume = 0.6f }, Projectile.Center);

                var source = Projectile.GetSource_FromThis();
                int childDamage = (int)(Projectile.damage * 0.6f);

                for (int i = 0; i < 8; i++)
                {
                    float angle = MathHelper.TwoPi * i / 8f + Main.rand.NextFloat(-0.15f, 0.15f);
                    Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(7f, 10f);

                    GenericHomingOrbChild.SpawnChild(
                        source,
                        Projectile.Center, vel,
                        childDamage, Projectile.knockBack * 0.5f, Projectile.owner,
                        homingStrength: 0.08f,
                        behaviorFlags: GenericHomingOrbChild.FLAG_ACCELERATE,
                        themeIndex: GenericHomingOrbChild.THEME_DIESIRAE,
                        scaleMult: 0.8f,
                        timeLeft: 60);
                }

                // Impact burst
                for (int i = 0; i < 12; i++)
                {
                    Dust fire = Dust.NewDustPerfect(Projectile.Center, DustID.Torch,
                        Main.rand.NextVector2CircularEdge(6f, 6f), 0, default,
                        Main.rand.NextFloat(1.0f, 1.5f));
                    fire.noGravity = true;
                }
            }
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
    }
}
