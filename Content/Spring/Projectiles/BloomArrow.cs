using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Content.Spring.Projectiles
{
    /// <summary>
    /// BloomArrow - Arrow that splits into homing petals on hit or after traveling a distance
    /// </summary>
    public class BloomArrow : ModProjectile
    {
        private static readonly Color SpringPink = new Color(255, 183, 197);
        private static readonly Color SpringWhite = new Color(255, 250, 250);
        private static readonly Color SpringGreen = new Color(144, 238, 144);

        private bool hasSplit = false;

        public override string Texture => "MagnumOpus/Assets/Particles/SoftGlow";

        public override void SetDefaults()
        {
            Projectile.width = 12;
            Projectile.height = 12;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 180;
            Projectile.alpha = 0;
            Projectile.light = 0.4f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.arrow = true;
            Projectile.extraUpdates = 1;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            // Trail particles
            if (Main.rand.NextBool(2))
            {
                Vector2 trailPos = Projectile.Center + Main.rand.NextVector2Circular(4f, 4f);
                Color trailColor = Color.Lerp(SpringPink, SpringWhite, Main.rand.NextFloat()) * 0.7f;
                var trail = new GenericGlowParticle(trailPos, -Projectile.velocity * 0.1f, trailColor, 0.3f, 20, true);
                MagnumParticleHandler.SpawnParticle(trail);
            }

            // Split into petals after 40 frames of flight if not hit anything
            if (!hasSplit && Projectile.timeLeft <= 140)
            {
                SplitIntoPetals();
            }

            // Pulsing glow
            float pulse = (float)Math.Sin(Projectile.timeLeft * 0.15f) * 0.15f + 0.5f;
            Lighting.AddLight(Projectile.Center, SpringPink.ToVector3() * pulse);
        }

        private void SplitIntoPetals()
        {
            hasSplit = true;
            
            // Spawn 3 homing petals
            for (int i = 0; i < 3; i++)
            {
                float angle = Projectile.velocity.ToRotation() + MathHelper.ToRadians(-30 + i * 30);
                Vector2 petalVel = angle.ToRotationVector2() * Projectile.velocity.Length() * 0.8f;
                
                Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    Projectile.Center,
                    petalVel,
                    ModContent.ProjectileType<HomingPetal>(),
                    Projectile.damage / 2,
                    Projectile.knockBack * 0.5f,
                    Projectile.owner
                );
            }

            // Split VFX
            CustomParticles.GenericFlare(Projectile.Center, SpringPink, 0.6f, 15);
            CustomParticles.HaloRing(Projectile.Center, SpringWhite * 0.5f, 0.3f, 12);
            
            for (int i = 0; i < 8; i++)
            {
                Vector2 burstVel = Main.rand.NextVector2Circular(4f, 4f);
                var burst = new GenericGlowParticle(Projectile.Center, burstVel, SpringPink * 0.8f, 0.25f, 20, true);
                MagnumParticleHandler.SpawnParticle(burst);
            }

            Projectile.Kill();
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!hasSplit)
            {
                SplitIntoPetals();
            }

            // Pollination: 15% chance to spawn healing flower
            if (Main.rand.NextFloat() < 0.15f)
            {
                Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    target.Center,
                    Vector2.Zero,
                    ModContent.ProjectileType<HealingFlower>(),
                    0,
                    0,
                    Projectile.owner
                );
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = texture.Size() / 2f;

            float pulse = (float)Math.Sin(Projectile.timeLeft * 0.15f) * 0.1f + 1f;

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            // Arrow-shaped glow elongated in direction of travel
            Main.spriteBatch.Draw(texture, drawPos, null, SpringPink * 0.7f, Projectile.rotation, origin, new Vector2(0.3f, 0.6f) * pulse, SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(texture, drawPos, null, SpringWhite * 0.5f, Projectile.rotation, origin, new Vector2(0.2f, 0.4f) * pulse, SpriteEffects.None, 0f);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            return false;
        }

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 6; i++)
            {
                Vector2 dustVel = Main.rand.NextVector2Circular(3f, 3f);
                Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.PinkFairy, dustVel, 100, SpringPink, 0.9f);
                dust.noGravity = true;
            }
        }
    }

    /// <summary>
    /// HomingPetal - Seeks nearby enemies and damages them
    /// </summary>
    public class HomingPetal : ModProjectile
    {
        private static readonly Color SpringPink = new Color(255, 183, 197);
        private static readonly Color SpringWhite = new Color(255, 250, 250);
        private static readonly Color SpringGreen = new Color(144, 238, 144);

        public override string Texture => "MagnumOpus/Assets/Particles/SoftGlow";

        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 120;
            Projectile.alpha = 0;
            Projectile.light = 0.3f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
        }

        public override void AI()
        {
            // Homing behavior
            NPC target = FindClosestNPC(400f);
            if (target != null)
            {
                Vector2 toTarget = target.Center - Projectile.Center;
                toTarget.Normalize();
                float homingStrength = 0.12f;
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, toTarget * 10f, homingStrength);
            }

            // Gentle floating motion
            Projectile.velocity.Y += (float)Math.Sin(Projectile.timeLeft * 0.1f) * 0.05f;

            // Spin
            Projectile.rotation += 0.15f;

            // Trail
            if (Main.rand.NextBool(2))
            {
                Color trailColor = Color.Lerp(SpringPink, SpringWhite, Main.rand.NextFloat()) * 0.6f;
                var trail = new GenericGlowParticle(Projectile.Center, -Projectile.velocity * 0.15f, trailColor, 0.25f, 18, true);
                MagnumParticleHandler.SpawnParticle(trail);
            }

            Lighting.AddLight(Projectile.Center, SpringPink.ToVector3() * 0.3f);
        }

        private NPC FindClosestNPC(float maxDistance)
        {
            NPC closest = null;
            float closestDist = maxDistance;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && !npc.friendly && npc.CanBeChasedBy())
                {
                    float dist = Vector2.Distance(Projectile.Center, npc.Center);
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        closest = npc;
                    }
                }
            }

            return closest;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Life leech - heal player on kill
            if (target.life <= 0)
            {
                Main.player[Projectile.owner].Heal(3);
                
                // Healing VFX
                CustomParticles.GenericFlare(Main.player[Projectile.owner].Center, SpringGreen, 0.5f, 15);
            }

            // Hit VFX
            for (int i = 0; i < 5; i++)
            {
                Vector2 burstVel = Main.rand.NextVector2Circular(3f, 3f);
                var burst = new GenericGlowParticle(target.Center, burstVel, SpringPink, 0.3f, 15, true);
                MagnumParticleHandler.SpawnParticle(burst);
            }

            // Pollination check
            if (Main.rand.NextFloat() < 0.15f)
            {
                Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    target.Center,
                    Vector2.Zero,
                    ModContent.ProjectileType<HealingFlower>(),
                    0,
                    0,
                    Projectile.owner
                );
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = texture.Size() / 2f;

            float pulse = (float)Math.Sin(Projectile.timeLeft * 0.12f) * 0.15f + 1f;
            Color drawColor = Color.Lerp(SpringPink, SpringWhite, (float)Math.Sin(Projectile.timeLeft * 0.1f) * 0.5f + 0.5f);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            Main.spriteBatch.Draw(texture, drawPos, null, drawColor * 0.6f, Projectile.rotation, origin, 0.4f * pulse, SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(texture, drawPos, null, SpringWhite * 0.5f, Projectile.rotation, origin, 0.25f * pulse, SpriteEffects.None, 0f);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            return false;
        }

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 5; i++)
            {
                Vector2 dustVel = Main.rand.NextVector2Circular(2f, 2f);
                Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.PinkFairy, dustVel, 80, SpringPink, 0.8f);
                dust.noGravity = true;
            }
        }
    }

    /// <summary>
    /// HealingFlower - Stationary flower that heals player when touched
    /// </summary>
    public class HealingFlower : ModProjectile
    {
        private static readonly Color SpringPink = new Color(255, 183, 197);
        private static readonly Color SpringGreen = new Color(144, 238, 144);

        public override string Texture => "MagnumOpus/Assets/Particles/SoftGlow";

        public override void SetDefaults()
        {
            Projectile.width = 24;
            Projectile.height = 24;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 300; // 5 seconds
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
        }

        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];

            // Check if player is nearby
            if (Vector2.Distance(owner.Center, Projectile.Center) < 40f)
            {
                owner.Heal(5);
                
                // Heal VFX
                CustomParticles.GenericFlare(owner.Center, SpringGreen, 0.7f, 20);
                for (int i = 0; i < 8; i++)
                {
                    Vector2 healVel = new Vector2(Main.rand.NextFloat(-1f, 1f), -Main.rand.NextFloat(2f, 4f));
                    var heal = new GenericGlowParticle(owner.Center, healVel, SpringGreen, 0.35f, 25, true);
                    MagnumParticleHandler.SpawnParticle(heal);
                }
                
                Projectile.Kill();
                return;
            }

            // Ambient flower particles
            if (Main.rand.NextBool(8))
            {
                Vector2 petalPos = Projectile.Center + Main.rand.NextVector2Circular(15f, 15f);
                Vector2 petalVel = new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), -Main.rand.NextFloat(0.5f, 1f));
                Color petalColor = Main.rand.NextBool() ? SpringPink : SpringGreen;
                var petal = new GenericGlowParticle(petalPos, petalVel, petalColor * 0.7f, 0.25f, 30, true);
                MagnumParticleHandler.SpawnParticle(petal);
            }

            // Pulsing glow
            float pulse = (float)Math.Sin(Projectile.timeLeft * 0.08f) * 0.2f + 0.6f;
            Lighting.AddLight(Projectile.Center, SpringGreen.ToVector3() * pulse);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = texture.Size() / 2f;

            float pulse = (float)Math.Sin(Projectile.timeLeft * 0.1f) * 0.15f + 1f;
            float alpha = Projectile.timeLeft > 60 ? 1f : Projectile.timeLeft / 60f;

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            // Green healing glow
            Main.spriteBatch.Draw(texture, drawPos, null, SpringGreen * 0.5f * alpha, 0f, origin, 0.6f * pulse, SpriteEffects.None, 0f);
            // Pink flower center
            Main.spriteBatch.Draw(texture, drawPos, null, SpringPink * 0.6f * alpha, 0f, origin, 0.35f * pulse, SpriteEffects.None, 0f);
            // White core
            Main.spriteBatch.Draw(texture, drawPos, null, Color.White * 0.4f * alpha, 0f, origin, 0.2f * pulse, SpriteEffects.None, 0f);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            return false;
        }

        public override void OnKill(int timeLeft)
        {
            // Flower dissipation
            for (int i = 0; i < 10; i++)
            {
                Vector2 dustVel = Main.rand.NextVector2Circular(3f, 3f);
                Color dustColor = Main.rand.NextBool() ? SpringPink : SpringGreen;
                Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.PinkFairy, dustVel, 80, dustColor, 0.9f);
                dust.noGravity = true;
            }
        }
    }
}
