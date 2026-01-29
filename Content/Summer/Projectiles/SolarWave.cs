using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Content.Summer.Projectiles
{
    /// <summary>
    /// Solar Wave - Standard energy wave from Zenith Cleaver swings
    /// </summary>
    public class SolarWave : ModProjectile
    {
        private static readonly Color SunGold = new Color(255, 215, 0);
        private static readonly Color SunOrange = new Color(255, 140, 0);
        private static readonly Color SunWhite = new Color(255, 250, 240);

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 24;
            Projectile.height = 24;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 3;
            Projectile.timeLeft = 60;
            Projectile.light = 0.5f;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.alpha = 0;
        }

        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.SolarWhipSwordExplosion;

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            // Trail particles
            if (Main.rand.NextBool(2))
            {
                Vector2 trailPos = Projectile.Center + Main.rand.NextVector2Circular(8f, 8f);
                Vector2 trailVel = -Projectile.velocity * 0.08f + Main.rand.NextVector2Circular(1.5f, 1.5f);
                Color trailColor = Color.Lerp(SunGold, SunOrange, Main.rand.NextFloat()) * 0.7f;
                var trail = new GenericGlowParticle(trailPos, trailVel, trailColor, 0.3f, 18, true);
                MagnumParticleHandler.SpawnParticle(trail);
            }

            // Fire dust
            if (Main.rand.NextBool(3))
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.SolarFlare, -Projectile.velocity * 0.1f, 0, SunOrange, 1.0f);
                dust.noGravity = true;
            }

            // Slowing down slightly
            Projectile.velocity *= 0.98f;

            Lighting.AddLight(Projectile.Center, SunGold.ToVector3() * 0.5f);
        }

        public override void OnKill(int timeLeft)
        {
            CustomParticles.GenericFlare(Projectile.Center, SunGold, 0.5f, 16);
            
            for (int i = 0; i < 6; i++)
            {
                Vector2 burstVel = Main.rand.NextVector2Circular(5f, 5f);
                Color burstColor = Color.Lerp(SunGold, SunOrange, Main.rand.NextFloat());
                var burst = new GenericGlowParticle(Projectile.Center, burstVel, burstColor * 0.65f, 0.26f, 18, true);
                MagnumParticleHandler.SpawnParticle(burst);
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 origin = texture.Size() / 2f;

            SpriteBatch spriteBatch = Main.spriteBatch;

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            // Trail
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;
                float progress = (float)i / Projectile.oldPos.Length;
                float trailAlpha = 1f - progress;
                float trailScale = (1f - progress * 0.5f) * 0.8f;
                Color trailColor = Color.Lerp(SunGold, SunOrange, progress) * trailAlpha * 0.5f;
                Vector2 trailPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                spriteBatch.Draw(texture, trailPos, null, trailColor, Projectile.oldRot[i], origin, trailScale, SpriteEffects.None, 0f);
            }

            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.1f) * 0.1f + 1f;

            spriteBatch.Draw(texture, drawPos, null, SunGold * 0.35f, Projectile.rotation, origin, 0.5f * pulse, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, drawPos, null, SunOrange * 0.45f, Projectile.rotation, origin, 0.35f * pulse, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, drawPos, null, SunWhite * 0.55f, Projectile.rotation, origin, 0.22f * pulse, SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            return false;
        }
    }

    /// <summary>
    /// Zenith Flare - Massive solar projectile from Zenith Strike
    /// </summary>
    public class ZenithFlare : ModProjectile
    {
        private static readonly Color SunGold = new Color(255, 215, 0);
        private static readonly Color SunOrange = new Color(255, 140, 0);
        private static readonly Color SunWhite = new Color(255, 250, 240);
        private static readonly Color SunRed = new Color(255, 100, 50);

        private float orbitAngle = 0f;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 15;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 32;
            Projectile.height = 32;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 5;
            Projectile.timeLeft = 120;
            Projectile.light = 0.8f;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.alpha = 0;
        }

        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.SolarWhipSwordExplosion;

        public override void AI()
        {
            orbitAngle += 0.15f;
            Projectile.rotation += 0.12f;

            // Intense trail
            for (int i = 0; i < 2; i++)
            {
                Vector2 trailPos = Projectile.Center + Main.rand.NextVector2Circular(10f, 10f);
                Vector2 trailVel = -Projectile.velocity * 0.1f + Main.rand.NextVector2Circular(2f, 2f);
                Color trailColor = Color.Lerp(SunGold, SunRed, Main.rand.NextFloat()) * 0.75f;
                var trail = new GenericGlowParticle(trailPos, trailVel, trailColor, 0.4f, 22, true);
                MagnumParticleHandler.SpawnParticle(trail);
            }

            // Orbiting flare points
            if (Projectile.timeLeft % 5 == 0)
            {
                for (int i = 0; i < 4; i++)
                {
                    float sparkAngle = orbitAngle + MathHelper.PiOver2 * i;
                    Vector2 sparkPos = Projectile.Center + sparkAngle.ToRotationVector2() * 20f;
                    Color sparkColor = i % 2 == 0 ? SunGold : SunOrange;
                    CustomParticles.GenericFlare(sparkPos, sparkColor * 0.7f, 0.28f, 10);
                }
            }

            // Fire dust constantly
            Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(15f, 15f), DustID.SolarFlare, -Projectile.velocity * 0.15f, 0, SunOrange, 1.2f);
            dust.noGravity = true;

            // Dynamic lighting
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.12f) * 0.2f + 1f;
            Lighting.AddLight(Projectile.Center, SunGold.ToVector3() * pulse * 0.9f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Apply heavy burning
            target.AddBuff(BuffID.OnFire3, 300);
            target.AddBuff(BuffID.Daybreak, 180);

            // Big impact VFX
            CustomParticles.GenericFlare(target.Center, SunWhite, 0.9f, 22);
            CustomParticles.GenericFlare(target.Center, SunGold, 0.75f, 20);
            CustomParticles.HaloRing(target.Center, SunOrange * 0.7f, 0.5f, 18);

            for (int i = 0; i < 10; i++)
            {
                Vector2 emberVel = Main.rand.NextVector2Circular(8f, 8f);
                Color emberColor = Color.Lerp(SunGold, SunRed, Main.rand.NextFloat());
                var ember = new GenericGlowParticle(target.Center, emberVel, emberColor * 0.8f, 0.35f, 22, true);
                MagnumParticleHandler.SpawnParticle(ember);
            }
        }

        public override void OnKill(int timeLeft)
        {
            // Big solar explosion
            CustomParticles.GenericFlare(Projectile.Center, Color.White, 1.1f, 25);
            CustomParticles.GenericFlare(Projectile.Center, SunGold, 0.9f, 22);
            CustomParticles.HaloRing(Projectile.Center, SunOrange * 0.7f, 0.6f, 20);
            CustomParticles.HaloRing(Projectile.Center, SunRed * 0.5f, 0.45f, 18);

            // Radial burst
            for (int i = 0; i < 16; i++)
            {
                float angle = MathHelper.TwoPi * i / 16f;
                Vector2 burstVel = angle.ToRotationVector2() * Main.rand.NextFloat(5f, 12f);
                Color burstColor = Color.Lerp(SunGold, SunRed, (float)i / 16f);
                var burst = new GenericGlowParticle(Projectile.Center, burstVel, burstColor * 0.75f, 0.38f, 25, true);
                MagnumParticleHandler.SpawnParticle(burst);
            }

            // Fire dust explosion
            for (int i = 0; i < 12; i++)
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.SolarFlare, Main.rand.NextVector2Circular(8f, 8f), 0, SunOrange, 1.3f);
                dust.noGravity = true;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 origin = texture.Size() / 2f;

            SpriteBatch spriteBatch = Main.spriteBatch;

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            // Intense trail
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;
                float progress = (float)i / Projectile.oldPos.Length;
                float trailAlpha = 1f - progress;
                float trailScale = (1f - progress * 0.4f);
                Color trailColor = Color.Lerp(SunGold, SunRed, progress) * trailAlpha * 0.55f;
                Vector2 trailPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                spriteBatch.Draw(texture, trailPos, null, trailColor, Projectile.oldRot[i], origin, trailScale, SpriteEffects.None, 0f);
            }

            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.12f) * 0.12f + 1f;

            // Multi-layer bloom
            spriteBatch.Draw(texture, drawPos, null, SunRed * 0.3f, Projectile.rotation, origin, 0.7f * pulse, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, drawPos, null, SunOrange * 0.4f, Projectile.rotation, origin, 0.55f * pulse, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, drawPos, null, SunGold * 0.5f, Projectile.rotation, origin, 0.4f * pulse, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, drawPos, null, SunWhite * 0.6f, Projectile.rotation, origin, 0.25f * pulse, SpriteEffects.None, 0f);

            // Orbiting points
            for (int i = 0; i < 4; i++)
            {
                float sparkAngle = orbitAngle + MathHelper.PiOver2 * i;
                Vector2 sparkOffset = sparkAngle.ToRotationVector2() * 16f;
                Color sparkColor = i % 2 == 0 ? SunGold : SunOrange;
                spriteBatch.Draw(texture, drawPos + sparkOffset, null, sparkColor * 0.5f, 0f, origin, 0.15f * pulse, SpriteEffects.None, 0f);
            }

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            return false;
        }
    }
}
