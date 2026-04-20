using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.VFX.Core;
using MagnumOpus.Content.OdeToJoy;

namespace MagnumOpus.Content.OdeToJoy.Weapons.ThornboundReckoning.Projectiles
{
    /// <summary>
    /// Bouncing blade projectile for Thornbound Reckoning's charged right-click.
    /// Weak homing toward nearby enemies, bounces off tiles up to 3 times with
    /// escalating damage (1.2x per bounce). Gold flame dust trail with IncisorOrb rendering.
    /// Multi-orb collision bonus: 2+ orbs hitting same NPC within 10 frames = 1.5x damage.
    /// ai[0] = bounce count
    /// </summary>
    public class ThornboundReckoningSpecialProj : ModProjectile
    {
        private VertexStrip _strip;

        // Multi-orb collision tracking: NPC whoAmI -> (hitCount, lastHitFrame)
        private static readonly Dictionary<int, (int hitCount, int lastFrame)> _multiOrbHits = new();

        public override string Texture => "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftGlow";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 12;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 5;
            Projectile.timeLeft = 180;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }

        public override void AI()
        {
            // Weak homing toward nearest enemy
            float homingStrength = 0.04f;
            float detectionRange = 600f;
            NPC closest = null;
            float closestDist = detectionRange;

            foreach (NPC npc in Main.ActiveNPCs)
            {
                if (npc.CanBeChasedBy())
                {
                    float dist = Vector2.Distance(Projectile.Center, npc.Center);
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        closest = npc;
                    }
                }
            }

            if (closest != null)
            {
                Vector2 desired = (closest.Center - Projectile.Center).SafeNormalize(Vector2.UnitX);
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, desired * Projectile.velocity.Length(), homingStrength);
            }

            // Rotation follows velocity
            Projectile.rotation += 0.3f * Projectile.direction;

            // Gold flame dust trail
            if (Main.rand.NextBool(2))
            {
                int dustType = Main.rand.NextBool() ? DustID.GreenTorch : DustID.GoldFlame;
                Color dustColor = Main.rand.NextBool() ? new Color(90, 200, 60) : new Color(255, 210, 60);
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height,
                    dustType, 0f, 0f, 0, dustColor, 1.2f);
                dust.noGravity = true;
                dust.velocity *= 0.3f;
            }

            // Subtle golden-green light, brightens with bounces
            float bounceIntensity = 1f + Projectile.ai[0] * 0.15f;
            Lighting.AddLight(Projectile.Center, new Vector3(0.4f, 0.55f, 0.1f) * 0.5f * bounceIntensity);
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (Projectile.ai[0] < 3)
            {
                Projectile.ai[0]++;

                // Reflect velocity
                if (Math.Abs(Projectile.velocity.X - oldVelocity.X) > float.Epsilon)
                    Projectile.velocity.X = -oldVelocity.X;
                if (Math.Abs(Projectile.velocity.Y - oldVelocity.Y) > float.Epsilon)
                    Projectile.velocity.Y = -oldVelocity.Y;

                // Escalate damage per bounce
                Projectile.damage = (int)(Projectile.damage * 1.2f);

                // Bounce dust burst
                for (int i = 0; i < 6; i++)
                {
                    Vector2 sparkVel = Main.rand.NextVector2CircularEdge(3f, 3f);
                    Color col = i % 2 == 0 ? new Color(90, 200, 60) : new Color(255, 210, 60);
                    Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.GreenTorch, sparkVel, 0, col, 0.8f);
                    d.noGravity = true;
                }

                return false;
            }

            return true;
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            // Multi-orb collision bonus: if 2+ orbs hit same NPC within 10 frames, 1.5x damage
            if (_multiOrbHits.TryGetValue(target.whoAmI, out var entry) &&
                (int)Main.GameUpdateCount - entry.lastFrame <= 10 && entry.hitCount >= 1)
            {
                modifiers.FinalDamage *= 1.5f;
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Track multi-orb hits
            int currentFrame = (int)Main.GameUpdateCount;
            if (_multiOrbHits.TryGetValue(target.whoAmI, out var existing) &&
                currentFrame - existing.lastFrame <= 10)
            {
                _multiOrbHits[target.whoAmI] = (existing.hitCount + 1, currentFrame);
            }
            else
            {
                _multiOrbHits[target.whoAmI] = (1, currentFrame);
            }

            Vector2 hitPos = target.Center;

            // Spark VFX
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
            for (int i = 0; i < 6; i++)
            {
                Vector2 sparkVel = Main.rand.NextVector2CircularEdge(3f, 3f);
                Color col = Main.rand.NextBool() ? new Color(90, 200, 60) : new Color(255, 210, 60);
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.GreenTorch, sparkVel, 0, col, 0.4f);
                d.noGravity = true;
            }

            try { OdeToJoyVFXLibrary.SpawnMusicNotes(Projectile.Center, 1, 12f, 0.5f, 0.7f, 20); } catch { }
            try { OdeToJoyVFXLibrary.SpawnMixedSparkleImpact(Projectile.Center, 0.5f, 4, 4); } catch { }
            try { OdeToJoyVFXLibrary.SpawnJoyousSparkles(Projectile.Center, 3, 15f); } catch { }
        }
    }
}
