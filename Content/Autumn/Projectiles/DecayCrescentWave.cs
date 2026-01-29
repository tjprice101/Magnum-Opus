using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Content.Autumn.Projectiles
{
    /// <summary>
    /// Decay Crescent Wave - Twilight Slash projectile from Harvest Reaper
    /// </summary>
    public class DecayCrescentWave : ModProjectile
    {
        private static readonly Color AutumnOrange = new Color(255, 140, 50);
        private static readonly Color AutumnRed = new Color(178, 34, 34);
        private static readonly Color DecayPurple = new Color(100, 50, 120);
        private static readonly Color AutumnGold = new Color(218, 165, 32);

        public override void SetDefaults()
        {
            Projectile.width = 60;
            Projectile.height = 30;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 5;
            Projectile.timeLeft = 90;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.alpha = 255;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
            Projectile.velocity *= 0.98f; // Slight slowdown

            // Intense crescent trail
            for (int i = 0; i < 3; i++)
            {
                Vector2 offset = Main.rand.NextVector2Circular(10f, 5f);
                Vector2 trailVel = -Projectile.velocity * 0.15f + Main.rand.NextVector2Circular(1.5f, 1.5f);
                Color trailColor = Color.Lerp(AutumnOrange, DecayPurple, Main.rand.NextFloat()) * 0.6f;
                var trail = new GenericGlowParticle(Projectile.Center + offset, trailVel, trailColor, 0.35f, 22, true);
                MagnumParticleHandler.SpawnParticle(trail);
            }

            // Core glow
            if (Main.rand.NextBool(2))
            {
                CustomParticles.GenericFlare(Projectile.Center, AutumnGold * 0.5f, 0.35f, 8);
            }

            // Falling leaves in trail
            if (Main.rand.NextBool(4))
            {
                Vector2 leafVel = new Vector2(Main.rand.NextFloat(-3f, 3f), Main.rand.NextFloat(1f, 4f));
                Color leafColor = Main.rand.NextBool() ? AutumnOrange : AutumnRed;
                var leaf = new GenericGlowParticle(Projectile.Center, leafVel, leafColor * 0.5f, 0.22f, 35, true);
                MagnumParticleHandler.SpawnParticle(leaf);
            }

            Lighting.AddLight(Projectile.Center, AutumnOrange.ToVector3() * 0.7f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Apply decay
            target.AddBuff(BuffID.Ichor, 240);

            // Hit VFX
            CustomParticles.GenericFlare(target.Center, AutumnGold, 0.6f, 18);
            CustomParticles.HaloRing(target.Center, DecayPurple * 0.6f, 0.45f, 15);

            for (int i = 0; i < 10; i++)
            {
                Vector2 sparkVel = Main.rand.NextVector2Circular(7f, 7f);
                Color sparkColor = Color.Lerp(AutumnOrange, DecayPurple, Main.rand.NextFloat()) * 0.6f;
                var spark = new GenericGlowParticle(target.Center, sparkVel, sparkColor, 0.28f, 18, true);
                MagnumParticleHandler.SpawnParticle(spark);
            }
        }

        public override void OnKill(int timeLeft)
        {
            // Dissipation VFX
            CustomParticles.GenericFlare(Projectile.Center, AutumnOrange, 0.5f, 18);
            CustomParticles.HaloRing(Projectile.Center, DecayPurple * 0.5f, 0.4f, 15);

            // Leaf scatter
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 leafVel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 6f);
                Color leafColor = Color.Lerp(AutumnOrange, AutumnRed, Main.rand.NextFloat()) * 0.5f;
                var leaf = new GenericGlowParticle(Projectile.Center, leafVel, leafColor, 0.25f, 30, true);
                MagnumParticleHandler.SpawnParticle(leaf);
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D texture = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/SoftGlow").Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = texture.Size() / 2f;

            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.12f) * 0.15f + 1f;
            float lifeProgress = 1f - (float)Projectile.timeLeft / 90f;

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            // Crescent shape - draw stretched
            float stretch = 2.5f;
            spriteBatch.Draw(texture, drawPos, null, AutumnOrange * 0.5f * (1f - lifeProgress * 0.5f), Projectile.rotation, origin, new Vector2(0.6f * stretch, 0.3f) * pulse, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, drawPos, null, DecayPurple * 0.4f * (1f - lifeProgress * 0.5f), Projectile.rotation, origin, new Vector2(0.45f * stretch, 0.22f) * pulse, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, drawPos, null, AutumnGold * 0.55f * (1f - lifeProgress * 0.5f), Projectile.rotation, origin, new Vector2(0.3f * stretch, 0.15f) * pulse, SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            return false;
        }
    }

    /// <summary>
    /// Soul Wisp - Healing projectile generated on kills by Harvest Reaper
    /// </summary>
    public class SoulWisp : ModProjectile
    {
        private static readonly Color AutumnGold = new Color(218, 165, 32);
        private static readonly Color SoulWhite = new Color(255, 250, 230);

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 300;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.alpha = 255;
        }

        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];
            
            // Home toward player
            Vector2 toPlayer = owner.Center - Projectile.Center;
            float dist = toPlayer.Length();
            
            if (dist < 30f)
            {
                // Heal on contact
                owner.Heal(12);
                Projectile.Kill();
                return;
            }

            // Accelerating homing
            float speed = MathHelper.Lerp(4f, 16f, 1f - (float)Projectile.timeLeft / 300f);
            toPlayer.Normalize();
            Projectile.velocity = Vector2.Lerp(Projectile.velocity, toPlayer * speed, 0.08f);

            // Trail effect
            if (Main.rand.NextBool(2))
            {
                Vector2 trailVel = -Projectile.velocity * 0.1f + Main.rand.NextVector2Circular(1f, 1f);
                Color trailColor = Color.Lerp(AutumnGold, SoulWhite, Main.rand.NextFloat()) * 0.5f;
                var trail = new GenericGlowParticle(Projectile.Center, trailVel, trailColor, 0.22f, 20, true);
                MagnumParticleHandler.SpawnParticle(trail);
            }

            Lighting.AddLight(Projectile.Center, AutumnGold.ToVector3() * 0.5f);
        }

        public override void OnKill(int timeLeft)
        {
            // Absorption VFX
            CustomParticles.GenericFlare(Projectile.Center, SoulWhite, 0.45f, 15);
            CustomParticles.HaloRing(Projectile.Center, AutumnGold * 0.5f, 0.3f, 12);

            for (int i = 0; i < 6; i++)
            {
                Vector2 burstVel = Main.rand.NextVector2Circular(3f, 3f);
                Color burstColor = Color.Lerp(AutumnGold, SoulWhite, Main.rand.NextFloat()) * 0.6f;
                var burst = new GenericGlowParticle(Projectile.Center, burstVel, burstColor, 0.2f, 15, true);
                MagnumParticleHandler.SpawnParticle(burst);
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D texture = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/SoftGlow").Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = texture.Size() / 2f;

            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.15f + Projectile.whoAmI) * 0.15f + 1f;

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            spriteBatch.Draw(texture, drawPos, null, AutumnGold * 0.35f, 0f, origin, 0.35f * pulse, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, drawPos, null, SoulWhite * 0.5f, 0f, origin, 0.2f * pulse, SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            return false;
        }
    }
}
