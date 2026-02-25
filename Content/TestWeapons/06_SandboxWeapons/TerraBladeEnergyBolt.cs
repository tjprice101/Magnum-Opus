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
    /// Rendered as HIGHLY stretched/squished flare textures with fluid water-like wobble movement.
    /// Fires outward from the blade tip, homes toward the nearest enemy with organic motion.
    /// </summary>
    public class TerraBladeEnergyBolt : ModProjectile
    {
        private const int TrailCacheSize = 20;
        private const float HomingRange = 600f;
        private const float HomingLerp = 0.05f;
        private const float BaseSpeed = 14f;

        // Fluid wobble — multi-frequency for organic water-like flow
        private const float WobbleFreq1 = 4.2f;
        private const float WobbleFreq2 = 6.8f;
        private const float WobbleFreq3 = 9.1f;
        private const float WobbleAmp1 = 2.2f;
        private const float WobbleAmp2 = 1.3f;
        private const float WobbleAmp3 = 0.7f;

        // Stretched flare parameters
        private const float FlareStretchX = 6f;
        private const float FlareSquishY = 0.12f;

        private int timer = 0;
        private int cachedTarget = -1;
        private float wobblePhase;

        public override string Texture => "MagnumOpus/Assets/Particles/EnergyFlare";

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
            Projectile.alpha = 255;

            wobblePhase = Main.rand.NextFloat(MathHelper.TwoPi);
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

            // Fluid water-like wobble
            float t = timer * 0.09f + wobblePhase;
            Vector2 dir = Projectile.velocity.SafeNormalize(Vector2.UnitX);
            Vector2 perp = new Vector2(-dir.Y, dir.X);
            float wobble = MathF.Sin(t * WobbleFreq1) * WobbleAmp1
                         + MathF.Sin(t * WobbleFreq2 + 2.1f) * WobbleAmp2
                         + MathF.Sin(t * WobbleFreq3 + 4.3f) * WobbleAmp3;
            Projectile.position += perp * wobble * 0.25f;

            // Sparkle particles
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

            // Sparkle flicker
            if (timer % 4 == 0)
            {
                Color flickerColor = TerraBladeShaderManager.GetPaletteColor(Main.rand.NextFloat(0.5f, 0.9f));
                var sparkle = new SparkleParticle(
                    Projectile.Center, Main.rand.NextVector2Circular(1f, 1f),
                    flickerColor, flickerColor * 0.5f,
                    Main.rand.NextFloat(0.15f, 0.3f), 8);
                MagnumParticleHandler.SpawnParticle(sparkle);
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
            float velRot = Projectile.velocity.ToRotation();
            float pulse = 0.9f + MathF.Sin(time * 12f + timer * 0.3f) * 0.1f;
            float speed = Projectile.velocity.Length();
            float dynamicStretch = 1f + speed * 0.05f;

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

            // Stretched flare body
            Texture2D flare1 = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/EnergyFlare").Value;
            Texture2D flare2 = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/FlareSparkle").Value;
            Texture2D flare3 = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/ThinSparkleFlare").Value;

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null,
                Main.GameViewMatrix.TransformationMatrix);

            // Layer 1: Outer glow — wide stretched flare
            {
                Vector2 origin = flare1.Size() * 0.5f;
                Color outerColor = TerraBladeShaderManager.GetPaletteColor(0.3f) with { A = 0 };
                Vector2 stretchScale = new Vector2(FlareStretchX * dynamicStretch, FlareSquishY) * pulse;
                sb.Draw(flare1, drawPos, null, outerColor * 0.30f,
                    velRot, origin, stretchScale, SpriteEffects.None, 0f);
            }

            // Layer 2: Sparkle overlay — FlareSparkle with rotation wobble
            {
                Vector2 origin = flare2.Size() * 0.5f;
                Color sparkleColor = TerraBladeShaderManager.GetPaletteColor(0.55f) with { A = 0 };
                Vector2 stretchScale = new Vector2(FlareStretchX * 0.7f * dynamicStretch, FlareSquishY * 0.8f) * pulse;
                sb.Draw(flare2, drawPos, null, sparkleColor * 0.40f,
                    velRot + MathF.Sin(time * 5f) * 0.04f, origin, stretchScale, SpriteEffects.None, 0f);
            }

            // Layer 3: Thin core sparkle — tight stretch
            {
                Vector2 origin = flare3.Size() * 0.5f;
                Color coreColor = TerraBladeShaderManager.GetPaletteColor(0.7f) with { A = 0 };
                Vector2 stretchScale = new Vector2(FlareStretchX * 0.5f * dynamicStretch, FlareSquishY * 0.4f) * pulse;
                sb.Draw(flare3, drawPos, null, coreColor * 0.50f,
                    velRot, origin, stretchScale, SpriteEffects.None, 0f);
            }

            // Layer 4: White-hot center
            {
                Vector2 origin = flare1.Size() * 0.5f;
                Vector2 stretchScale = new Vector2(FlareStretchX * 0.3f * dynamicStretch, FlareSquishY * 0.3f) * pulse;
                sb.Draw(flare1, drawPos, null, (Color.White with { A = 0 }) * 0.55f,
                    velRot, origin, stretchScale, SpriteEffects.None, 0f);
            }

            // Afterimage flares at old positions
            Vector2 flareOrigin = flare1.Size() * 0.5f;
            for (int i = 1; i < TrailCacheSize; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;
                float trailProgress = (float)i / TrailCacheSize;
                float trailAlpha = (1f - trailProgress) * 0.20f;
                Vector2 trailPos = Projectile.oldPos[i] + Projectile.Size * 0.5f - Main.screenPosition;
                float trailRot = Projectile.oldRot[i];
                float trailStretch = MathHelper.Lerp(FlareStretchX * 0.4f, FlareStretchX * 0.1f, trailProgress);
                float trailSquish = MathHelper.Lerp(FlareSquishY * 0.7f, FlareSquishY * 0.2f, trailProgress);
                Color trailColor = TerraBladeShaderManager.GetPaletteColor(0.3f + trailProgress * 0.4f) with { A = 0 };
                sb.Draw(flare1, trailPos, null, trailColor * trailAlpha,
                    trailRot, flareOrigin, new Vector2(trailStretch, trailSquish), SpriteEffects.None, 0f);
            }

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                DepthStencilState.None, RasterizerState.CullNone, null,
                Main.GameViewMatrix.TransformationMatrix);

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
