using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.VFX.Core;
using MagnumOpus.Content.Nachtmusik;
using MagnumOpus.Content.Nachtmusik.Weapons.SerenadeOfDistantStars.Utilities;

namespace MagnumOpus.Content.Nachtmusik.Weapons.SerenadeOfDistantStars.Projectiles
{
    /// <summary>
    /// Serenade star projectile -- scaffold based on BlackSwanFlareProj pattern.
    /// Homing sub-projectile with IncisorOrbRenderer visuals.
    /// </summary>
    public class SerenadeStarProjectile : ModProjectile
    {
        private const float HomingRange = 350f;
        private const float HomingStrength = 0.08f;
        private const float MaxSpeed = 16f;
        private Player Owner => Main.player[Projectile.owner];
        private bool _initialized;
        private VertexStrip _strip;

        public override string Texture => "MagnumOpus/Content/Nachtmusik/Weapons/SerenadeOfDistantStars/SerenadeOfDistantStars";

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
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 120;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.extraUpdates = 1;
        }

        public override void AI()
        {
            if (!_initialized)
            {
                _initialized = true;
                Projectile.rotation = Projectile.velocity.ToRotation();
            }

            // Rhythm Stacking: accelerating speed and strengthening homing
            float lifeProgress = 1f - (Projectile.timeLeft / 120f);
            float currentHomingStrength = MathHelper.Lerp(0.04f, 0.12f, lifeProgress);
            float currentMaxSpeed = MathHelper.Lerp(12f, 24f, lifeProgress);

            NPC target = SerenadeOfDistantStarsUtils.ClosestNPCAt(Projectile.Center, HomingRange);
            if (target != null)
            {
                Vector2 desiredDir = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, desiredDir * Projectile.velocity.Length(), currentHomingStrength);
            }

            if (Projectile.velocity.Length() < currentMaxSpeed)
                Projectile.velocity *= 1.02f;
            if (Projectile.velocity.Length() > currentMaxSpeed)
                Projectile.velocity = Vector2.Normalize(Projectile.velocity) * currentMaxSpeed;

            Projectile.rotation = Projectile.velocity.ToRotation();

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
            float pulse = 1f + 0.15f * (float)Math.Sin(Projectile.timeLeft * 0.2f);
            Lighting.AddLight(Projectile.Center, new Vector3(0.3f, 0.35f, 0.6f) * 0.35f * pulse);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Vector2 hitPos = target.Center;
            for (int i = 0; i < 6; i++)
            {
                Vector2 sparkVel = Main.rand.NextVector2CircularEdge(4f, 4f);
                Color col = i % 2 == 0 ? new Color(60, 70, 150) : new Color(180, 200, 255);
                Dust d = Dust.NewDustPerfect(hitPos, DustID.WhiteTorch, sparkVel, 0, col, 0.5f);
                d.noGravity = true;
            }
            for (int i = 0; i < 2; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(2f, 2f) + new Vector2(0, -1f);
                Dust d = Dust.NewDustPerfect(hitPos + Main.rand.NextVector2Circular(8f, 8f),
                    DustID.BlueTorch, vel, 0, new Color(60, 70, 150), 0.5f);
                d.noGravity = true;
            }
            try { NachtmusikVFXLibrary.SpawnMusicNotes(hitPos, 1, 12f, 0.4f, 0.7f, 20); } catch { }
            try { NachtmusikVFXLibrary.SpawnMixedSparkleImpact(hitPos, 0.6f, 4, 4); } catch { }
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
    }
}
