using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Content.DiesIrae;

namespace MagnumOpus.Content.DiesIrae.Weapons.SinCollector.Projectiles
{
    /// <summary>
    /// Sin Expenditure projectile — spawned on right-click when sins are consumed.
    /// ai[0] = tier (1=Penance, 2=Absolution, 3=Damnation)
    /// Tier 1 (Penance, 10+ sins): pierce 3, 1.5x damage, slight homing (0.06)
    /// Tier 2 (Absolution, 20+ sins): pierce -1 (all), 2x damage, accelerating (8→30)
    /// Tier 3 (Damnation, 30 sins): 3x scale, 3x damage, aggressive homing (0.14),
    ///         on-hit spawns GenericDamageZone (3s, 150px, continuous damage)
    /// </summary>
    public class SinExpendProjectile : ModProjectile
    {
        private VertexStrip _strip;
        private bool _initialized;
        private int Tier => (int)Projectile.ai[0];

        public override string Texture => "MagnumOpus/Content/DiesIrae/Weapons/SinCollector/SinCollector";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 20;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 180;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.extraUpdates = 1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }

        public override void AI()
        {
            if (!_initialized)
            {
                _initialized = true;

                switch (Tier)
                {
                    case 1: // Penance: pierce 3, slight homing
                        Projectile.penetrate = 3;
                        break;
                    case 2: // Absolution: pierce all, accelerating
                        Projectile.penetrate = -1;
                        // Start at reduced speed, will accelerate
                        if (Projectile.velocity.Length() > 8f)
                            Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.UnitX) * 8f;
                        break;
                    case 3: // Damnation: massive, aggressive homing
                        Projectile.penetrate = 1;
                        Projectile.scale = 3f;
                        Projectile.width = 48;
                        Projectile.height = 48;
                        break;
                }
            }

            // Homing behavior based on tier
            float homingStrength = Tier switch
            {
                1 => 0.06f,
                2 => 0f, // Absolution: no homing, pure straight accelerating
                3 => 0.14f,
                _ => 0.04f
            };

            float homingRange = Tier switch
            {
                3 => 600f,
                _ => 400f
            };

            if (homingStrength > 0.001f)
            {
                NPC target = FindClosestNPC(homingRange);
                if (target != null)
                {
                    Vector2 idealDir = (target.Center - Projectile.Center).SafeNormalize(Projectile.velocity.SafeNormalize(Vector2.UnitX));
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, idealDir * Projectile.velocity.Length(), homingStrength);
                }
            }

            // Absolution: accelerate from 8 toward 30
            if (Tier == 2)
            {
                if (Projectile.velocity.Length() < 30f)
                    Projectile.velocity *= 1.025f;
            }

            Projectile.rotation = Projectile.velocity.ToRotation();

            // Enhanced dust trail
            if (Main.rand.NextBool(2))
            {
                int dustType = Main.rand.NextBool() ? DustID.Torch : DustID.SolarFlare;
                float dustScale = Tier >= 3 ? 1.2f : (Tier >= 2 ? 1.0f : 0.8f);
                Color dustColor = Main.rand.NextBool() ? new Color(255, 180, 50) : new Color(200, 40, 20);
                Dust d = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(8f, 8f),
                    dustType, -Projectile.velocity * 0.15f + Main.rand.NextVector2Circular(1f, 1f),
                    0, dustColor, dustScale);
                d.noGravity = true;
                d.fadeIn = 0.6f;
            }

            float pulse = 1f + 0.2f * MathF.Sin(Projectile.timeLeft * 0.2f);
            float lightMult = 0.35f + Tier * 0.15f;
            Lighting.AddLight(Projectile.Center, new Vector3(0.8f, 0.3f, 0.1f) * lightMult * pulse);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Damnation tier: spawn lingering pyre zone on hit
            if (Tier == 3)
            {
                GenericDamageZone.SpawnZone(
                    Projectile.GetSource_FromThis(),
                    target.Center, Projectile.damage / 3, Projectile.knockBack, Projectile.owner,
                    modeFlags: 0, radius: 150f,
                    themeIndex: GenericHomingOrbChild.THEME_DIESIRAE,
                    durationFrames: 180); // 3 seconds
            }

            // Impact VFX
            Vector2 hitPos = target.Center;
            int sparkCount = 6 + Tier * 3;
            for (int i = 0; i < sparkCount; i++)
            {
                Vector2 sparkVel = Main.rand.NextVector2CircularEdge(5f + Tier, 5f + Tier);
                Color col = i % 2 == 0 ? new Color(200, 40, 20) : new Color(255, 180, 50);
                Dust d = Dust.NewDustPerfect(hitPos, DustID.Torch, sparkVel, 0, col, 0.6f + Tier * 0.1f);
                d.noGravity = true;
            }

            try { DiesIraeVFXLibrary.SpawnMusicNotes(hitPos, Tier, 15f, 0.5f, 0.9f, 25); } catch { }
            try { DiesIraeVFXLibrary.SpawnMixedSparkleImpact(hitPos, 0.8f, 5, 5); } catch { }
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

        public override void OnKill(int timeLeft)
        {
            if (Main.dedServ) return;
            int burstCount = 4 + Tier * 2;
            for (int i = 0; i < burstCount; i++)
            {
                Vector2 sparkVel = Main.rand.NextVector2CircularEdge(4f, 4f);
                Color col = Main.rand.NextBool() ? new Color(200, 40, 20) : new Color(255, 180, 50);
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.Torch, sparkVel, 0, col, 0.5f);
                d.noGravity = true;
            }
            try { DiesIraeVFXLibrary.SpawnInfernalSparkles(Projectile.Center, Tier + 2, 18f); } catch { }
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
                if (dist < closestDist) { closestDist = dist; closest = npc; }
            }
            return closest;
        }
    }
}
