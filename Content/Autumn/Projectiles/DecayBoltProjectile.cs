using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Content.Autumn.Projectiles
{
    /// <summary>
    /// Decay Bolt - Main projectile for Withering Grimoire
    /// Creates entropic fields on impact
    /// </summary>
    public class DecayBoltProjectile : ModProjectile
    {
        private static readonly Color DecayPurple = new Color(100, 50, 120);
        private static readonly Color DeathGreen = new Color(80, 120, 60);
        private static readonly Color WitherBrown = new Color(90, 60, 40);

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 3;
            Projectile.timeLeft = 240;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.alpha = 255;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();

            // Decay trail
            if (Main.rand.NextBool(2))
            {
                Vector2 trailVel = -Projectile.velocity * 0.12f + Main.rand.NextVector2Circular(1.5f, 1.5f);
                Color trailColor = Color.Lerp(DecayPurple, DeathGreen, Main.rand.NextFloat()) * 0.5f;
                var trail = new GenericGlowParticle(Projectile.Center, trailVel, trailColor, 0.28f, 20, true);
                MagnumParticleHandler.SpawnParticle(trail);
            }

            // Core glow
            CustomParticles.GenericFlare(Projectile.Center, DecayPurple * 0.35f, 0.22f, 5);

            Lighting.AddLight(Projectile.Center, DecayPurple.ToVector3() * 0.45f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Life Drain: 5% heal
            Player owner = Main.player[Projectile.owner];
            int healAmount = Math.Max(1, damageDone / 20);
            owner.Heal(healAmount);

            // Apply withering debuff
            target.AddBuff(BuffID.CursedInferno, 180);

            // Create entropic field
            if (Main.myPlayer == Projectile.owner && Projectile.penetrate <= 1)
            {
                Projectile.NewProjectile(
                    Projectile.GetSource_OnHit(target),
                    target.Center,
                    Vector2.Zero,
                    ModContent.ProjectileType<EntropicField>(),
                    Projectile.damage / 4,
                    0f,
                    Projectile.owner
                );
            }

            // Hit VFX
            CustomParticles.GenericFlare(target.Center, DeathGreen, 0.5f, 16);
            CustomParticles.HaloRing(target.Center, DecayPurple * 0.5f, 0.35f, 14);

            for (int i = 0; i < 6; i++)
            {
                Vector2 sparkVel = Main.rand.NextVector2Circular(5f, 5f);
                Color sparkColor = Color.Lerp(DecayPurple, DeathGreen, Main.rand.NextFloat()) * 0.5f;
                var spark = new GenericGlowParticle(target.Center, sparkVel, sparkColor, 0.22f, 18, true);
                MagnumParticleHandler.SpawnParticle(spark);
            }
        }

        public override void OnKill(int timeLeft)
        {
            CustomParticles.GenericFlare(Projectile.Center, DecayPurple, 0.45f, 16);
            CustomParticles.HaloRing(Projectile.Center, DeathGreen * 0.4f, 0.3f, 14);

            for (int i = 0; i < 8; i++)
            {
                Vector2 burstVel = Main.rand.NextVector2Circular(4f, 4f);
                Color burstColor = Color.Lerp(DecayPurple, DeathGreen, Main.rand.NextFloat()) * 0.5f;
                var burst = new GenericGlowParticle(Projectile.Center, burstVel, burstColor, 0.22f, 18, true);
                MagnumParticleHandler.SpawnParticle(burst);
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D texture = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/SoftGlow").Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = texture.Size() / 2f;

            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.12f) * 0.1f + 1f;

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            spriteBatch.Draw(texture, drawPos, null, DecayPurple * 0.4f, 0f, origin, 0.35f * pulse, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, drawPos, null, DeathGreen * 0.5f, 0f, origin, 0.22f * pulse, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, drawPos, null, Color.White * 0.5f, 0f, origin, 0.1f * pulse, SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            return false;
        }
    }

    /// <summary>
    /// Entropic Field - Damaging zone created on bolt impact
    /// </summary>
    public class EntropicField : ModProjectile
    {
        private static readonly Color DecayPurple = new Color(100, 50, 120);
        private static readonly Color DeathGreen = new Color(80, 120, 60);

        public override void SetDefaults()
        {
            Projectile.width = 80;
            Projectile.height = 80;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 180;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.alpha = 255;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 20;
        }

        public override void AI()
        {
            float lifeProgress = 1f - (float)Projectile.timeLeft / 180f;
            float fadeOut = Projectile.timeLeft < 30 ? (float)Projectile.timeLeft / 30f : 1f;

            // Swirling decay particles
            if (Main.rand.NextBool(3))
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                float dist = Main.rand.NextFloat(10f, 40f);
                Vector2 pos = Projectile.Center + angle.ToRotationVector2() * dist;
                Vector2 vel = (angle + MathHelper.PiOver2).ToRotationVector2() * Main.rand.NextFloat(1f, 3f);
                vel += new Vector2(0, -0.5f);
                Color color = Color.Lerp(DecayPurple, DeathGreen, Main.rand.NextFloat()) * 0.5f * fadeOut;
                var particle = new GenericGlowParticle(pos, vel, color, 0.25f, 25, true);
                MagnumParticleHandler.SpawnParticle(particle);
            }

            // Central glow
            CustomParticles.GenericFlare(Projectile.Center, DecayPurple * 0.3f * fadeOut, 0.3f, 6);

            Lighting.AddLight(Projectile.Center, DecayPurple.ToVector3() * 0.35f * fadeOut);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Life Drain: 5% heal
            Player owner = Main.player[Projectile.owner];
            int healAmount = Math.Max(1, damageDone / 20);
            owner.Heal(healAmount);

            target.AddBuff(BuffID.CursedInferno, 60);

            // Small hit VFX
            for (int i = 0; i < 3; i++)
            {
                Vector2 sparkVel = Main.rand.NextVector2Circular(3f, 3f);
                Color sparkColor = Color.Lerp(DecayPurple, DeathGreen, Main.rand.NextFloat()) * 0.4f;
                var spark = new GenericGlowParticle(target.Center, sparkVel, sparkColor, 0.18f, 12, true);
                MagnumParticleHandler.SpawnParticle(spark);
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D texture = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/SoftGlow").Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = texture.Size() / 2f;

            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.08f) * 0.1f + 1f;
            float fadeOut = Projectile.timeLeft < 30 ? (float)Projectile.timeLeft / 30f : 1f;

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            spriteBatch.Draw(texture, drawPos, null, DecayPurple * 0.2f * fadeOut, 0f, origin, 0.8f * pulse, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, drawPos, null, DeathGreen * 0.15f * fadeOut, 0f, origin, 0.6f * pulse, SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            return false;
        }
    }

    /// <summary>
    /// Withering Wave - Charged attack projectile
    /// </summary>
    public class WitheringWave : ModProjectile
    {
        private static readonly Color DecayPurple = new Color(100, 50, 120);
        private static readonly Color DeathGreen = new Color(80, 120, 60);
        private static readonly Color WitherBrown = new Color(90, 60, 40);

        public override void SetDefaults()
        {
            Projectile.width = 80;
            Projectile.height = 40;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 120;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.alpha = 255;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 15;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
            Projectile.velocity *= 0.97f;

            // Expand hitbox as it travels
            if (Projectile.ai[0] < 30)
            {
                Projectile.ai[0]++;
                Projectile.width = (int)MathHelper.Lerp(80, 150, Projectile.ai[0] / 30f);
                Projectile.height = (int)MathHelper.Lerp(40, 80, Projectile.ai[0] / 30f);
            }

            float fadeOut = Projectile.timeLeft < 30 ? (float)Projectile.timeLeft / 30f : 1f;

            // Intense wave particles
            for (int i = 0; i < 4; i++)
            {
                Vector2 offset = Main.rand.NextVector2Circular(Projectile.width / 3f, Projectile.height / 3f);
                Vector2 particleVel = -Projectile.velocity * 0.15f + Main.rand.NextVector2Circular(2f, 2f);
                Color color = Color.Lerp(DecayPurple, DeathGreen, Main.rand.NextFloat()) * 0.5f * fadeOut;
                var particle = new GenericGlowParticle(Projectile.Center + offset, particleVel, color, 0.35f, 22, true);
                MagnumParticleHandler.SpawnParticle(particle);
            }

            // Core wave glow
            CustomParticles.GenericFlare(Projectile.Center, DecayPurple * 0.4f * fadeOut, 0.4f, 6);

            Lighting.AddLight(Projectile.Center, DeathGreen.ToVector3() * 0.6f * fadeOut);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Life Drain: 5% heal
            Player owner = Main.player[Projectile.owner];
            int healAmount = Math.Max(1, damageDone / 20);
            owner.Heal(healAmount);

            // Heavy debuffs
            target.AddBuff(BuffID.CursedInferno, 300);
            target.AddBuff(BuffID.Ichor, 240);

            // Create entropic field
            if (Main.myPlayer == Projectile.owner)
            {
                Projectile.NewProjectile(
                    Projectile.GetSource_OnHit(target),
                    target.Center,
                    Vector2.Zero,
                    ModContent.ProjectileType<EntropicField>(),
                    Projectile.damage / 3,
                    0f,
                    Projectile.owner
                );
            }

            // Heavy hit VFX
            CustomParticles.GenericFlare(target.Center, DeathGreen, 0.7f, 20);
            CustomParticles.HaloRing(target.Center, DecayPurple * 0.6f, 0.5f, 16);
            CustomParticles.HaloRing(target.Center, WitherBrown * 0.4f, 0.35f, 14);

            for (int i = 0; i < 10; i++)
            {
                Vector2 sparkVel = Main.rand.NextVector2Circular(6f, 6f);
                Color sparkColor = Color.Lerp(DecayPurple, DeathGreen, Main.rand.NextFloat()) * 0.6f;
                var spark = new GenericGlowParticle(target.Center, sparkVel, sparkColor, 0.28f, 20, true);
                MagnumParticleHandler.SpawnParticle(spark);
            }
        }

        public override void OnKill(int timeLeft)
        {
            // Dissipation VFX
            CustomParticles.GenericFlare(Projectile.Center, DecayPurple, 0.6f, 20);
            CustomParticles.HaloRing(Projectile.Center, DeathGreen * 0.5f, 0.45f, 16);

            for (int i = 0; i < 14; i++)
            {
                float angle = MathHelper.TwoPi * i / 14f;
                Vector2 burstVel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 8f);
                Color burstColor = Color.Lerp(DecayPurple, DeathGreen, (float)i / 14f) * 0.5f;
                var burst = new GenericGlowParticle(Projectile.Center, burstVel, burstColor, 0.28f, 22, true);
                MagnumParticleHandler.SpawnParticle(burst);
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D texture = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/SoftGlow").Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = texture.Size() / 2f;

            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.1f) * 0.12f + 1f;
            float fadeOut = Projectile.timeLeft < 30 ? (float)Projectile.timeLeft / 30f : 1f;
            float expansion = Projectile.ai[0] / 30f;

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            // Wave shape - stretched horizontally
            float stretch = 2.5f + expansion;
            spriteBatch.Draw(texture, drawPos, null, DecayPurple * 0.4f * fadeOut, Projectile.rotation, origin, new Vector2(0.5f * stretch, 0.25f) * pulse, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, drawPos, null, DeathGreen * 0.35f * fadeOut, Projectile.rotation, origin, new Vector2(0.4f * stretch, 0.2f) * pulse, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, drawPos, null, Color.White * 0.4f * fadeOut, Projectile.rotation, origin, new Vector2(0.25f * stretch, 0.12f) * pulse, SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            return false;
        }
    }
}
