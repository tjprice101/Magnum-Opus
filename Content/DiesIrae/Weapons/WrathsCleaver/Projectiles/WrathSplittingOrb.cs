using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Content.DiesIrae;

namespace MagnumOpus.Content.DiesIrae.Weapons.WrathsCleaver.Projectiles
{
    /// <summary>
    /// Phase 3 Wrath orb that aggressively homes and splits into 8 children on hit.
    /// The culmination of the Wrath Escalation combo.
    /// </summary>
    public class WrathSplittingOrb : ModProjectile
    {
        private VertexStrip _strip;
        private bool _initialized;

        private const float HomingStrength = 0.14f;
        private const int SplitCount = 8;

        public override string Texture => "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/PointBloom";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 15;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 18;
            Projectile.height = 18;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 120;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.extraUpdates = 1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 15;
            Projectile.scale = 1.3f;
        }

        public override void AI()
        {
            if (!_initialized)
            {
                _initialized = true;
                Projectile.rotation = Projectile.velocity.ToRotation();
            }

            // Aggressive homing
            NPC target = FindClosestNPC(800f);
            if (target != null)
            {
                Vector2 idealDir = (target.Center - Projectile.Center).SafeNormalize(Projectile.velocity.SafeNormalize(Vector2.UnitX));
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, idealDir * Projectile.velocity.Length(), HomingStrength);
            }

            // Slight acceleration
            if (Projectile.velocity.Length() < 22f)
                Projectile.velocity *= 1.015f;

            Projectile.rotation = Projectile.velocity.ToRotation();

            // Intense dust trail
            if (Main.rand.NextBool(2))
            {
                Color dustColor = Color.Lerp(DiesIraePalette.InfernalRed, DiesIraePalette.JudgmentGold, Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(6f, 6f),
                    DustID.Torch, -Projectile.velocity * 0.15f, 0, dustColor, 1.2f);
                d.noGravity = true;
                d.fadeIn = 0.6f;
            }

            // Ember sparks
            if (Main.rand.NextBool(4))
            {
                Dust spark = Dust.NewDustPerfect(Projectile.Center, DustID.SolarFlare,
                    Main.rand.NextVector2Circular(2f, 2f), 0, default, 0.7f);
                spark.noGravity = true;
            }

            // Lighting
            Lighting.AddLight(Projectile.Center, DiesIraePalette.InfernalRed.ToVector3() * 0.6f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Split into 8 children on hit
            if (Main.myPlayer == Projectile.owner)
            {
                for (int i = 0; i < SplitCount; i++)
                {
                    float angle = MathHelper.TwoPi * i / SplitCount + Main.rand.NextFloat(-0.15f, 0.15f);
                    Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(8f, 12f);

                    GenericHomingOrbChild.SpawnChild(
                        Projectile.GetSource_FromThis(),
                        target.Center, vel,
                        (int)(Projectile.damage * 0.5f), Projectile.knockBack * 0.5f, Projectile.owner,
                        homingStrength: 0.08f,
                        behaviorFlags: GenericHomingOrbChild.FLAG_ACCELERATE,
                        themeIndex: GenericHomingOrbChild.THEME_DIESIRAE,
                        scaleMult: 0.8f,
                        timeLeft: 60);
                }
            }

            // Impact VFX
            DiesIraeVFXLibrary.FinisherSlam(target.Center, 0.8f);

            // Apply hellfire
            target.AddBuff(BuffID.OnFire3, 300);
        }

        public override void OnKill(int timeLeft)
        {
            if (Main.dedServ) return;

            // Death burst
            DiesIraeVFXLibrary.SpawnWrathBurst(Projectile.Center, 10, 1f);
            DiesIraeVFXLibrary.SpawnMusicNotes(Projectile.Center, 2, 15f);
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

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            try
            {
                var config = IncisorOrbRenderer.DiesIrae;
                IncisorOrbRenderer.DrawOrbVisuals(sb, Projectile, config, ref _strip);
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
    }
}
