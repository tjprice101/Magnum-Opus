using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.VFX.Core;
using MagnumOpus.Content.OdeToJoy;

namespace MagnumOpus.Content.OdeToJoy.Weapons.TheGardenersFury.Projectiles
{
    /// <summary>
    /// Gravity seed orb for The Gardener's Fury charged right-click.
    /// Phase 0 (ai[0]=0): flies with heavy gravity until hitting a tile.
    /// Phase 1 (ai[0]=1): planted zone — stationary, damages nearby enemies for 1.5s,
    ///     then fires a GenericHomingOrbChild upward toward the nearest enemy and dies.
    /// IncisorOrb rendering with Ode to Joy theme.
    /// </summary>
    public class GardenerFurySpecialProj : ModProjectile
    {
        private VertexStrip _strip;
        private bool _initialized;
        private int _phaseTimer;

        public override string Texture => "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftGlow";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 16;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 300;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.extraUpdates = 1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 20;
        }

        public override void AI()
        {
            if (!_initialized)
            {
                _initialized = true;
                Projectile.rotation = Projectile.velocity.ToRotation();
            }

            // Phase 0: Flying seed with heavy gravity
            if (Projectile.ai[0] == 0f)
            {
                Projectile.velocity.Y += 0.20f;
                Projectile.rotation = Projectile.velocity.ToRotation();

                // Green trail dust while flying
                if (Main.rand.NextBool(2))
                {
                    Color dustColor = Main.rand.NextBool() ? new Color(90, 200, 60) : new Color(255, 210, 60);
                    Dust d = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(4f, 4f),
                        DustID.GreenTorch, -Projectile.velocity * 0.1f, 0, dustColor, 0.8f);
                    d.noGravity = true;
                    d.fadeIn = 0.5f;
                }

                Lighting.AddLight(Projectile.Center, new Vector3(0.3f, 0.5f, 0.1f) * 0.4f);
            }
            // Phase 1: Planted zone
            else if (Projectile.ai[0] == 1f)
            {
                _phaseTimer++;
                Projectile.velocity = Vector2.Zero;

                // Expand hitbox for zone damage
                if (Projectile.width < 60)
                {
                    Projectile.position -= new Vector2(22f, 22f);
                    Projectile.width = 60;
                    Projectile.height = 60;
                }

                // Rising green sparkle dust while planted
                if (Main.rand.NextBool(2))
                {
                    Vector2 dustPos = Projectile.Center + Main.rand.NextVector2Circular(28f, 10f);
                    Color dustColor = Main.rand.NextBool() ? new Color(90, 200, 60) : new Color(255, 210, 60);
                    Dust d = Dust.NewDustPerfect(dustPos, DustID.GreenTorch,
                        new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), -1.5f - Main.rand.NextFloat(0.5f)),
                        0, dustColor, 0.7f);
                    d.noGravity = true;
                    d.fadeIn = 0.4f;
                }

                // Pulsing green-gold light
                float pulse = 0.7f + 0.3f * MathF.Sin(Main.GlobalTimeWrappedHourly * 5f);
                Lighting.AddLight(Projectile.Center, new Vector3(0.3f, 0.55f, 0.15f) * 0.5f * pulse);

                // After 90 frames (1.5s): fire homing child upward toward nearest enemy, then die
                if (_phaseTimer >= 90)
                {
                    // Find nearest enemy for targeting
                    NPC target = null;
                    float closestDist = 600f;
                    for (int i = 0; i < Main.maxNPCs; i++)
                    {
                        NPC npc = Main.npc[i];
                        if (!npc.CanBeChasedBy()) continue;
                        float dist = Vector2.Distance(Projectile.Center, npc.Center);
                        if (dist < closestDist)
                        {
                            closestDist = dist;
                            target = npc;
                        }
                    }

                    Vector2 launchVel;
                    if (target != null)
                    {
                        launchVel = (target.Center - Projectile.Center).SafeNormalize(new Vector2(0, -1)) * 10f;
                    }
                    else
                    {
                        launchVel = new Vector2(0, -10f);
                    }

                    GenericHomingOrbChild.SpawnChild(
                        Projectile.GetSource_FromThis(),
                        Projectile.Center, launchVel,
                        Projectile.damage, Projectile.knockBack, Projectile.owner,
                        homingStrength: 0.08f,
                        behaviorFlags: 0,
                        themeIndex: GenericHomingOrbChild.THEME_ODETOJOY,
                        scaleMult: 1.2f,
                        timeLeft: 120);

                    Projectile.Kill();
                }
            }
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            // Phase 0: transition to planted phase
            if (Projectile.ai[0] == 0f)
            {
                Projectile.ai[0] = 1f;
                Projectile.velocity = Vector2.Zero;
                Projectile.tileCollide = false;
                Projectile.penetrate = -1;

                // Planting dust burst
                for (int i = 0; i < 8; i++)
                {
                    Vector2 sparkVel = Main.rand.NextVector2CircularEdge(3f, 3f);
                    sparkVel.Y = -Math.Abs(sparkVel.Y); // Upward burst
                    Color col = i % 2 == 0 ? new Color(90, 200, 60) : new Color(255, 210, 60);
                    Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.GreenTorch, sparkVel, 0, col, 0.8f);
                    d.noGravity = true;
                }

                try { OdeToJoyVFXLibrary.SpawnMusicNotes(Projectile.Center, 2, 15f, 0.4f, 0.7f, 20); } catch { }

                return false;
            }

            return true;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Vector2 hitPos = target.Center;

            for (int i = 0; i < 5; i++)
            {
                Vector2 sparkVel = Main.rand.NextVector2CircularEdge(3f, 3f);
                Color col = i % 2 == 0 ? new Color(90, 200, 60) : new Color(255, 210, 60);
                Dust d = Dust.NewDustPerfect(hitPos, DustID.GreenTorch, sparkVel, 0, col, 0.5f);
                d.noGravity = true;
            }

            try { OdeToJoyVFXLibrary.SpawnMixedSparkleImpact(hitPos, 0.4f, 3, 3); } catch { }
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
            for (int i = 0; i < 8; i++)
            {
                Vector2 sparkVel = Main.rand.NextVector2CircularEdge(4f, 4f);
                Color col = Main.rand.NextBool() ? new Color(90, 200, 60) : new Color(255, 210, 60);
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.GreenTorch, sparkVel, 0, col, 0.5f);
                d.noGravity = true;
            }

            try { OdeToJoyVFXLibrary.SpawnMusicNotes(Projectile.Center, 2, 15f, 0.5f, 0.8f, 22); } catch { }
            try { OdeToJoyVFXLibrary.SpawnMixedSparkleImpact(Projectile.Center, 0.6f, 4, 4); } catch { }
            try { OdeToJoyVFXLibrary.SpawnJoyousSparkles(Projectile.Center, 4, 20f); } catch { }
        }
    }
}
