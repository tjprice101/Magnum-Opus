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
    /// Solar Orb Projectile - Rapid fire orb from Solstice Tome
    /// Explodes on impact
    /// </summary>
    public class SolarOrbProjectile : ModProjectile
    {
        private static readonly Color SunGold = new Color(255, 215, 0);
        private static readonly Color SunOrange = new Color(255, 140, 0);
        private static readonly Color SunWhite = new Color(255, 250, 240);
        private static readonly Color SunRed = new Color(255, 80, 40);

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 120;
            Projectile.light = 0.4f;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.alpha = 0;
        }

        public override string Texture => "MagnumOpus/Assets/Particles/GlowingHalo4";

        private float orbitTimer = 0f;

        public override void AI()
        {
            Projectile.rotation += 0.15f;
            orbitTimer += 0.12f;

            // === VFX VARIATION #13: SOLAR CORONA ORBIT ===
            // Fiery wisps orbit the orb like a sun's corona
            if (Main.GameUpdateCount % 4 == 0)
            {
                for (int c = 0; c < 4; c++)
                {
                    float coronaAngle = orbitTimer + MathHelper.TwoPi * c / 4f;
                    float coronaRadius = 12f + (float)Math.Sin(Main.GameUpdateCount * 0.15f + c) * 3f;
                    Vector2 coronaPos = Projectile.Center + coronaAngle.ToRotationVector2() * coronaRadius;
                    Vector2 coronaVel = coronaAngle.ToRotationVector2().RotatedBy(MathHelper.PiOver2) * 0.8f;
                    Color coronaColor = Color.Lerp(SunGold, SunRed, (float)c / 4f) * 0.6f;
                    var corona = new GenericGlowParticle(coronaPos, coronaVel, coronaColor, 0.2f, 12, true);
                    MagnumParticleHandler.SpawnParticle(corona);
                }
            }

            // === VFX VARIATION #14: RADIANT PULSE RINGS ===
            // Expanding rings pulse outward periodically
            if (Main.GameUpdateCount % 15 == 0)
            {
                for (int r = 0; r < 6; r++)
                {
                    float ringAngle = MathHelper.TwoPi * r / 6f;
                    Vector2 ringVel = ringAngle.ToRotationVector2() * 3.5f;
                    Color ringColor = Color.Lerp(SunGold, SunOrange, (float)r / 6f) * 0.5f;
                    var ring = new GenericGlowParticle(Projectile.Center, ringVel, ringColor, 0.18f, 16, true);
                    MagnumParticleHandler.SpawnParticle(ring);
                }
            }

            // Layered trail with sparkles
            if (Main.rand.NextBool(2))
            {
                Vector2 trailPos = Projectile.Center + Main.rand.NextVector2Circular(6f, 6f);
                Vector2 trailVel = -Projectile.velocity * 0.09f + Main.rand.NextVector2Circular(1.8f, 1.8f);
                Color trailColor = Color.Lerp(SunGold, SunOrange, Main.rand.NextFloat()) * 0.7f;
                var trail = new GenericGlowParticle(trailPos, trailVel, trailColor, 0.3f, 20, true);
                MagnumParticleHandler.SpawnParticle(trail);
                
                // Sparkle accents for magical shimmer
                var sparkle = new SparkleParticle(trailPos, trailVel * 1.3f, SunWhite * 0.55f, 0.24f, 16);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }

            // Fire dust
            if (Main.rand.NextBool(4))
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.SolarFlare, -Projectile.velocity * 0.12f, 0, SunOrange, 1.0f);
                dust.noGravity = true;
            }

            // Musical notes blaze - VISIBLE (scale 0.78f)
            if (Main.rand.NextBool(5))
            {
                Vector2 noteVel = -Projectile.velocity * 0.05f + new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), Main.rand.NextFloat(-1.2f, -0.4f));
                Color noteColor = Color.Lerp(SunGold, SunOrange, Main.rand.NextFloat());
                ThemedParticles.MusicNote(Projectile.Center + Main.rand.NextVector2Circular(8f, 8f), noteVel, noteColor, 0.78f, 40);
            }

            Lighting.AddLight(Projectile.Center, SunGold.ToVector3() * 0.5f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // ☁EMUSICAL IMPACT - VISIBLE notes burst! (scale 0.75f)
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f;
                Vector2 noteVel = angle.ToRotationVector2() * 3.5f;
                Color noteColor = Color.Lerp(SunGold, SunOrange, i / 6f);
                ThemedParticles.MusicNote(target.Center, noteVel, noteColor, 0.75f, 35);
            }
            
            // Sparkle ring for magical feel
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f;
                Vector2 sparkVel = angle.ToRotationVector2() * 4f;
                var sparkle = new SparkleParticle(target.Center, sparkVel, SunWhite * 0.8f, 0.3f, 18);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
            
            target.AddBuff(BuffID.OnFire3, 90);
        }

        public override void OnKill(int timeLeft)
        {
            // ☁EMUSICAL FINALE - VISIBLE notes scatter! (scale 0.8f)
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 noteVel = angle.ToRotationVector2() * 4f;
                Color noteColor = Color.Lerp(SunGold, SunOrange, i / 8f);
                ThemedParticles.MusicNote(Projectile.Center, noteVel, noteColor, 0.8f, 40);
            }
            
            // Explosion VFX - layered bloom
            CustomParticles.GenericFlare(Projectile.Center, SunWhite, 0.7f, 20);
            CustomParticles.GenericFlare(Projectile.Center, SunGold, 0.6f, 18);
            CustomParticles.GenericFlare(Projectile.Center, SunOrange * 0.5f, 0.4f, 14);
            CustomParticles.GenericFlare(Projectile.Center, SunOrange * 0.3f, 0.28f, 12);
            
            // Sparkle ring
            for (int ray = 0; ray < 6; ray++)
            {
                float rayAngle = MathHelper.TwoPi * ray / 6f;
                Vector2 rayPos = Projectile.Center + rayAngle.ToRotationVector2() * 15f;
                var sparkle = new SparkleParticle(rayPos, rayAngle.ToRotationVector2() * 2f, SunWhite * 0.7f, 0.28f, 15);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }

            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 burstVel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 6f);
                Color burstColor = Color.Lerp(SunGold, SunOrange, Main.rand.NextFloat());
                var burst = new GenericGlowParticle(Projectile.Center, burstVel, burstColor * 0.65f, 0.28f, 18, true);
                MagnumParticleHandler.SpawnParticle(burst);
            }

            // Fire dust burst
            for (int i = 0; i < 6; i++)
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.SolarFlare, Main.rand.NextVector2Circular(5f, 5f), 0, SunOrange, 1.1f);
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

            // Trail
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;
                float progress = (float)i / Projectile.oldPos.Length;
                float trailAlpha = 1f - progress;
                Color trailColor = Color.Lerp(SunGold, SunOrange, progress) * trailAlpha * 0.5f;
                Vector2 trailPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                spriteBatch.Draw(texture, trailPos, null, trailColor, Projectile.oldRot[i], origin, 0.35f * (1f - progress * 0.4f), SpriteEffects.None, 0f);
            }

            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.1f) * 0.08f + 1f;

            spriteBatch.Draw(texture, drawPos, null, SunGold * 0.4f, Projectile.rotation, origin, 0.4f * pulse, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, drawPos, null, SunOrange * 0.5f, Projectile.rotation, origin, 0.28f * pulse, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, drawPos, null, SunWhite * 0.6f, Projectile.rotation, origin, 0.16f * pulse, SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            return false;
        }
    }

    /// <summary>
    /// Sunbeam Projectile - Charged beam from Solstice Tome
    /// </summary>
    public class SunbeamProjectile : ModProjectile
    {
        private static readonly Color SunGold = new Color(255, 215, 0);
        private static readonly Color SunOrange = new Color(255, 140, 0);
        private static readonly Color SunWhite = new Color(255, 250, 240);
        private static readonly Color SunRed = new Color(255, 100, 50);

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 20;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 24;
            Projectile.height = 24;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 10;
            Projectile.timeLeft = 90;
            Projectile.light = 0.9f;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.alpha = 0;
            Projectile.extraUpdates = 2;
        }

        public override string Texture => "MagnumOpus/Assets/Particles/MagicSparklField11";

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            // Intense trail
            for (int i = 0; i < 2; i++)
            {
                Vector2 trailPos = Projectile.Center + Main.rand.NextVector2Circular(8f, 8f);
                Vector2 trailVel = -Projectile.velocity * 0.05f + Main.rand.NextVector2Circular(2f, 2f);
                Color trailColor = Color.Lerp(SunGold, SunWhite, Main.rand.NextFloat()) * 0.7f;
                var trail = new GenericGlowParticle(trailPos, trailVel, trailColor, 0.4f, 20, true);
                MagnumParticleHandler.SpawnParticle(trail);
            }

            // Fire dust
            Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.SolarFlare, -Projectile.velocity * 0.08f, 0, SunOrange, 1.3f);
            dust.noGravity = true;

            // Side sparks
            if (Projectile.timeLeft % 3 == 0)
            {
                for (int side = -1; side <= 1; side += 2)
                {
                    Vector2 perpDir = Projectile.velocity.RotatedBy(MathHelper.PiOver2 * side).SafeNormalize(Vector2.Zero);
                    Vector2 sparkPos = Projectile.Center + perpDir * 10f;
                    CustomParticles.GenericFlare(sparkPos, SunGold * 0.6f, 0.22f, 8);
                }
            }

            // ☁EMUSICAL NOTATION - Blazing notes stream from sunbeam! - VISIBLE SCALE 0.75f+
            if (Main.rand.NextBool(3))
            {
                Vector2 noteVel = -Projectile.velocity * 0.03f + new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), Main.rand.NextFloat(-1.2f, -0.3f));
                ThemedParticles.MusicNote(Projectile.Center + Main.rand.NextVector2Circular(10f, 10f), noteVel, SunGold, 0.75f, 40);
                
                // Solar sparkle
                var sparkle = new SparkleParticle(Projectile.Center, noteVel * 0.5f, SunOrange * 0.4f, 0.22f, 16);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }

            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.15f) * 0.2f + 1f;
            Lighting.AddLight(Projectile.Center, SunGold.ToVector3() * pulse);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // ☁EMUSICAL IMPACT - Solar symphony on hit!
            ThemedParticles.MusicNoteBurst(target.Center, SunGold, 8, 4f);
            
            // Heavy burning
            target.AddBuff(BuffID.OnFire3, 300);
            target.AddBuff(BuffID.Daybreak, 180);

            // Hit VFX - layered bloom
            CustomParticles.GenericFlare(target.Center, SunWhite, 0.75f, 18);
            CustomParticles.GenericFlare(target.Center, SunGold * 0.6f, 0.5f, 16);
            CustomParticles.GenericFlare(target.Center, SunGold * 0.4f, 0.35f, 14);
            // Solar ray burst
            for (int ray = 0; ray < 4; ray++)
            {
                float rayAngle = MathHelper.TwoPi * ray / 4f;
                Vector2 rayPos = target.Center + rayAngle.ToRotationVector2() * 14f;
                CustomParticles.GenericFlare(rayPos, SunGold * 0.65f, 0.18f, 10);
            }

            for (int i = 0; i < 6; i++)
            {
                Vector2 sparkVel = Main.rand.NextVector2Circular(6f, 6f);
                Color sparkColor = Color.Lerp(SunGold, SunRed, Main.rand.NextFloat());
                var spark = new GenericGlowParticle(target.Center, sparkVel, sparkColor * 0.7f, 0.32f, 18, true);
                MagnumParticleHandler.SpawnParticle(spark);
            }
        }

        public override void OnKill(int timeLeft)
        {
            // Big explosion - layered bloom cascade
            CustomParticles.GenericFlare(Projectile.Center, Color.White, 1.1f, 25);
            CustomParticles.GenericFlare(Projectile.Center, SunGold, 0.9f, 22);
            CustomParticles.GenericFlare(Projectile.Center, SunOrange * 0.7f, 0.65f, 20);
            CustomParticles.GenericFlare(Projectile.Center, SunOrange * 0.5f, 0.5f, 18);
            // Solar ray burst - 6-point star
            for (int ray = 0; ray < 6; ray++)
            {
                float rayAngle = MathHelper.TwoPi * ray / 6f;
                Vector2 rayPos = Projectile.Center + rayAngle.ToRotationVector2() * 18f;
                Color rayColor = ray % 2 == 0 ? SunGold : SunOrange;
                CustomParticles.GenericFlare(rayPos, rayColor * 0.75f, 0.22f, 12);
            }

            // Radial burst
            for (int i = 0; i < 14; i++)
            {
                float angle = MathHelper.TwoPi * i / 14f;
                Vector2 burstVel = angle.ToRotationVector2() * Main.rand.NextFloat(5f, 10f);
                Color burstColor = Color.Lerp(SunGold, SunRed, (float)i / 14f);
                var burst = new GenericGlowParticle(Projectile.Center, burstVel, burstColor * 0.7f, 0.38f, 24, true);
                MagnumParticleHandler.SpawnParticle(burst);
            }

            // Fire dust explosion
            for (int i = 0; i < 10; i++)
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.SolarFlare, Main.rand.NextVector2Circular(7f, 7f), 0, SunOrange, 1.3f);
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

            // Long intense trail
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;
                float progress = (float)i / Projectile.oldPos.Length;
                float trailAlpha = 1f - progress;
                float trailScale = (1f - progress * 0.3f);
                Color trailColor = Color.Lerp(SunWhite, SunOrange, progress) * trailAlpha * 0.6f;
                Vector2 trailPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                spriteBatch.Draw(texture, trailPos, null, trailColor, Projectile.oldRot[i], origin, trailScale * 0.5f, SpriteEffects.None, 0f);
            }

            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.15f) * 0.1f + 1f;

            // Multi-layer bloom
            spriteBatch.Draw(texture, drawPos, null, SunOrange * 0.35f, Projectile.rotation, origin, 0.65f * pulse, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, drawPos, null, SunGold * 0.5f, Projectile.rotation, origin, 0.48f * pulse, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, drawPos, null, SunWhite * 0.65f, Projectile.rotation, origin, 0.3f * pulse, SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            return false;
        }
    }
}
