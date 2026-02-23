using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.VFX.Effects;
using MagnumOpus.Common.Systems.VFX.Screen;
using MagnumOpus.Common.Systems.VFX.Trails;
using MagnumOpus.Content.TestWeapons.SandboxWeapons.Shaders;

namespace MagnumOpus.Content.TestWeapons.SandboxWeapons
{
    /// <summary>
    /// Small homing energy bolt spawned during Sandbox TerraBlade swings.
    /// Inspired by Calamity's EonBolt — fires outward from the blade tip,
    /// homes toward the nearest enemy, and creates a short-lived trail.
    /// </summary>
    public class TerraBladeEnergyBolt : ModProjectile
    {
        private const int TrailCacheSize = 20;
        private const float HomingRange = 600f;
        private const float HomingLerp = 0.05f;
        private const float BaseSpeed = 14f;

        private int timer = 0;
        private int cachedTarget = -1;

        public override string Texture => "MagnumOpus/Assets/Particles/CrispStar4";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = TrailCacheSize;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.MeleeNoSpeed;
            Projectile.penetrate = 1;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 80;
            Projectile.extraUpdates = 1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }

        public override void AI()
        {
            timer++;
            Projectile.rotation = Projectile.velocity.ToRotation();

            // Homing
            if (cachedTarget < 0)
                AcquireTarget();

            if (cachedTarget >= 0 && cachedTarget < Main.maxNPCs)
            {
                NPC target = Main.npc[cachedTarget];
                if (target.active && !target.friendly && !target.dontTakeDamage)
                {
                    Vector2 toTarget = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitY);
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, toTarget * BaseSpeed, HomingLerp);
                }
                else
                {
                    cachedTarget = -1;
                }
            }

            // Gentle speed decay
            Projectile.velocity *= 0.995f;

            // Spark particles
            if (timer % 3 == 0)
            {
                Vector2 sparkVel = -Projectile.velocity.SafeNormalize(Vector2.UnitY) * Main.rand.NextFloat(0.5f, 1.5f);
                sparkVel = sparkVel.RotatedBy(Main.rand.NextFloat(-0.4f, 0.4f));
                Color sparkColor = TerraBladeShaderManager.GetPaletteColor(Main.rand.NextFloat(0.4f, 0.8f));
                var spark = new GlowSparkParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(4f, 4f),
                    sparkVel, sparkColor, 0.1f, 10);
                MagnumParticleHandler.SpawnParticle(spark);
            }

            // Lighting
            Color light = TerraBladeShaderManager.GetPaletteColor(0.5f);
            Lighting.AddLight(Projectile.Center, light.ToVector3() * 0.4f);
        }

        private void AcquireTarget()
        {
            float bestDist = HomingRange;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly || npc.dontTakeDamage || npc.immortal)
                    continue;
                float dist = Vector2.Distance(Projectile.Center, npc.Center);
                if (dist < bestDist)
                {
                    bestDist = dist;
                    cachedTarget = i;
                }
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Screen-level impact
            ScreenFlashSystem.Instance?.ImpactFlash(0.2f);
            Projectile.ShakeScreen(0.2f);

            for (int i = 0; i < 5; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(4f, 4f);
                Color dustColor = TerraBladeShaderManager.GetPaletteColor(Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(target.Center, DustID.GreenTorch, vel, 0, dustColor, 1.2f);
                d.noGravity = true;
            }

            var ring = new BloomRingParticle(target.Center, Vector2.Zero,
                TerraBladeShaderManager.GetPaletteColor(0.5f) * 0.6f, 0.15f, 12);
            MagnumParticleHandler.SpawnParticle(ring);

            Lighting.AddLight(target.Center, 0.4f, 0.6f, 0.4f);
        }

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 4; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(3f, 3f);
                Color dustColor = TerraBladeShaderManager.GetPaletteColor(Main.rand.NextFloat(0.3f, 0.7f));
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.GreenTorch, vel, 0, dustColor, 0.8f);
                d.noGravity = true;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float time = Main.GlobalTimeWrappedHourly;

            // Trail
            try
            {
                float hueShift = MathF.Sin(time * 4f + timer * 0.08f) * 0.15f;
                Color trailPrimary = TerraBladeShaderManager.GetPaletteColor(0.4f + hueShift);
                Color trailSecondary = TerraBladeShaderManager.GetPaletteColor(0.8f - hueShift);

                CalamityStyleTrailRenderer.DrawProjectileTrailWithBloom(
                    Projectile,
                    CalamityStyleTrailRenderer.TrailStyle.Nature,
                    baseWidth: 15f,
                    primaryColor: trailPrimary,
                    secondaryColor: trailSecondary,
                    intensity: 1.2f,
                    bloomMultiplier: 2.0f);
            }
            catch { }

            // Body
            Texture2D starTex = ModContent.Request<Texture2D>(Texture).Value;
            if (starTex != null)
            {
                Vector2 origin = starTex.Size() * 0.5f;
                float velRot = Projectile.velocity.ToRotation();
                float pulse = 0.9f + MathF.Sin(time * 12f + timer * 0.3f) * 0.1f;

                sb.End();
                sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                    DepthStencilState.None, RasterizerState.CullNone, null,
                    Main.GameViewMatrix.TransformationMatrix);

                // Outer glow
                Color outerColor = TerraBladeShaderManager.GetPaletteColor(0.3f);
                sb.Draw(starTex, drawPos, null, outerColor with { A = 0 } * 0.4f,
                    velRot, origin, 0.3f * pulse, SpriteEffects.None, 0f);

                // Main body
                Color mainColor = TerraBladeShaderManager.GetPaletteColor(0.5f);
                sb.Draw(starTex, drawPos, null, mainColor with { A = 0 } * 0.8f,
                    velRot, origin, 0.18f * pulse, SpriteEffects.None, 0f);

                // White core
                sb.Draw(starTex, drawPos, null, Color.White with { A = 0 } * 0.6f,
                    velRot, origin, 0.08f * pulse, SpriteEffects.None, 0f);

                sb.End();
                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                    DepthStencilState.None, RasterizerState.CullNone, null,
                    Main.GameViewMatrix.TransformationMatrix);
            }

            return false;
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(cachedTarget);
            writer.Write(timer);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            cachedTarget = reader.ReadInt32();
            timer = reader.ReadInt32();
        }
    }
}
