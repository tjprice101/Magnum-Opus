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
    /// Twilight Bolt - Main projectile for Twilight Arbalest
    /// Gains damage over distance traveled (Fading Light mechanic)
    /// </summary>
    public class TwilightBolt : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/SoftGlow";
        
        private static readonly Color TwilightPurple = new Color(120, 60, 140);
        private static readonly Color TwilightOrange = new Color(255, 120, 60);
        private static readonly Color AutumnGold = new Color(218, 165, 32);

        private float distanceTraveled = 0f;
        private const float MaxDistanceBonus = 400f; // Max distance for bonus
        private const float MaxDamageMultiplier = 1.5f; // +50% max

        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 3;
            Projectile.timeLeft = 300;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = false;
            Projectile.arrow = true;
            Projectile.extraUpdates = 1;
            Projectile.alpha = 255;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            // Track distance
            distanceTraveled += Projectile.velocity.Length();

            // Trail effect
            if (Main.rand.NextBool(2))
            {
                Vector2 trailVel = -Projectile.velocity * 0.1f + Main.rand.NextVector2Circular(1f, 1f);
                float distProgress = Math.Min(1f, distanceTraveled / MaxDistanceBonus);
                Color trailColor = Color.Lerp(TwilightPurple, TwilightOrange, distProgress) * 0.5f;
                var trail = new GenericGlowParticle(Projectile.Center, trailVel, trailColor, 0.25f, 18, true);
                MagnumParticleHandler.SpawnParticle(trail);
            }

            // Core glow
            float distProgress2 = Math.Min(1f, distanceTraveled / MaxDistanceBonus);
            Color coreColor = Color.Lerp(TwilightPurple, TwilightOrange, distProgress2);
            CustomParticles.GenericFlare(Projectile.Center, coreColor * 0.35f, 0.2f, 4);

            Lighting.AddLight(Projectile.Center, coreColor.ToVector3() * 0.4f);
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            // Fading Light: Bonus damage based on distance traveled
            float distProgress = Math.Min(1f, distanceTraveled / MaxDistanceBonus);
            float damageMultiplier = MathHelper.Lerp(1f, MaxDamageMultiplier, distProgress);
            modifiers.FinalDamage *= damageMultiplier;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Dusk's Embrace: Crits spawn homing leaf shards
            if (hit.Crit)
            {
                if (Main.myPlayer == Projectile.owner)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        Vector2 leafVel = Main.rand.NextVector2Circular(6f, 6f);
                        Projectile.NewProjectile(
                            Projectile.GetSource_OnHit(target),
                            target.Center,
                            leafVel,
                            ModContent.ProjectileType<HomingLeafShard>(),
                            Projectile.damage / 3,
                            1f,
                            Projectile.owner
                        );
                    }
                }

                // Crit VFX
                CustomParticles.GenericFlare(target.Center, AutumnGold, 0.6f, 18);
                CustomParticles.HaloRing(target.Center, TwilightOrange * 0.5f, 0.4f, 14);
            }

            // Standard hit VFX
            CustomParticles.GenericFlare(target.Center, TwilightPurple, 0.4f, 14);

            for (int i = 0; i < 5; i++)
            {
                Vector2 sparkVel = Main.rand.NextVector2Circular(4f, 4f);
                Color sparkColor = Color.Lerp(TwilightPurple, TwilightOrange, Main.rand.NextFloat()) * 0.5f;
                var spark = new GenericGlowParticle(target.Center, sparkVel, sparkColor, 0.2f, 15, true);
                MagnumParticleHandler.SpawnParticle(spark);
            }
        }

        public override void OnKill(int timeLeft)
        {
            CustomParticles.GenericFlare(Projectile.Center, TwilightOrange, 0.4f, 15);
            CustomParticles.HaloRing(Projectile.Center, TwilightPurple * 0.4f, 0.25f, 12);

            for (int i = 0; i < 6; i++)
            {
                Vector2 burstVel = Main.rand.NextVector2Circular(4f, 4f);
                Color burstColor = Color.Lerp(TwilightPurple, TwilightOrange, Main.rand.NextFloat()) * 0.5f;
                var burst = new GenericGlowParticle(Projectile.Center, burstVel, burstColor, 0.22f, 16, true);
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
            float distProgress = Math.Min(1f, distanceTraveled / MaxDistanceBonus);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            Color mainColor = Color.Lerp(TwilightPurple, TwilightOrange, distProgress);
            spriteBatch.Draw(texture, drawPos, null, mainColor * 0.4f, 0f, origin, 0.35f * pulse * (1f + distProgress * 0.3f), SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, drawPos, null, mainColor * 0.55f, 0f, origin, 0.2f * pulse, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, drawPos, null, Color.White * 0.6f, 0f, origin, 0.1f * pulse, SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            return false;
        }
    }

    /// <summary>
    /// Harvest Moon Bolt - Large seeking projectile fired every 6th shot
    /// </summary>
    public class HarvestMoonBolt : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/SoftGlow";
        
        private static readonly Color MoonSilver = new Color(200, 200, 220);
        private static readonly Color MoonGold = new Color(218, 165, 32);
        private static readonly Color TwilightPurple = new Color(120, 60, 140);

        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 30;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 5;
            Projectile.timeLeft = 240;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.alpha = 255;
        }

        public override void AI()
        {
            // Gentle homing
            NPC target = FindTarget();
            if (target != null)
            {
                Vector2 toTarget = target.Center - Projectile.Center;
                toTarget.Normalize();
                float homingStrength = 0.04f;
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, toTarget * Projectile.velocity.Length(), homingStrength);
            }

            Projectile.rotation += 0.04f;

            // Intense trail
            for (int i = 0; i < 2; i++)
            {
                Vector2 trailVel = -Projectile.velocity * 0.15f + Main.rand.NextVector2Circular(2f, 2f);
                Color trailColor = Color.Lerp(MoonSilver, MoonGold, Main.rand.NextFloat()) * 0.6f;
                var trail = new GenericGlowParticle(Projectile.Center, trailVel, trailColor, 0.35f, 22, true);
                MagnumParticleHandler.SpawnParticle(trail);
            }

            // Core moon glow
            CustomParticles.GenericFlare(Projectile.Center, MoonSilver * 0.4f, 0.35f, 6);

            Lighting.AddLight(Projectile.Center, MoonSilver.ToVector3() * 0.7f);
        }

        private NPC FindTarget()
        {
            float maxDist = 400f;
            NPC closest = null;
            float closestDist = float.MaxValue;

            foreach (NPC npc in Main.npc)
            {
                if (!npc.CanBeChasedBy()) continue;
                
                float dist = Vector2.Distance(Projectile.Center, npc.Center);
                if (dist < maxDist && dist < closestDist)
                {
                    closest = npc;
                    closestDist = dist;
                }
            }

            return closest;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Heavy VFX
            CustomParticles.GenericFlare(target.Center, MoonSilver, 0.7f, 20);
            CustomParticles.HaloRing(target.Center, MoonGold * 0.6f, 0.55f, 18);
            CustomParticles.HaloRing(target.Center, TwilightPurple * 0.4f, 0.35f, 15);

            for (int i = 0; i < 10; i++)
            {
                Vector2 sparkVel = Main.rand.NextVector2Circular(6f, 6f);
                Color sparkColor = Color.Lerp(MoonSilver, MoonGold, Main.rand.NextFloat()) * 0.6f;
                var spark = new GenericGlowParticle(target.Center, sparkVel, sparkColor, 0.28f, 20, true);
                MagnumParticleHandler.SpawnParticle(spark);
            }
        }

        public override void OnKill(int timeLeft)
        {
            // Moon explosion
            CustomParticles.GenericFlare(Projectile.Center, MoonSilver, 0.65f, 22);
            CustomParticles.HaloRing(Projectile.Center, MoonGold * 0.5f, 0.5f, 18);

            for (int i = 0; i < 12; i++)
            {
                float angle = MathHelper.TwoPi * i / 12f;
                Vector2 burstVel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 7f);
                Color burstColor = Color.Lerp(MoonSilver, MoonGold, (float)i / 12f) * 0.5f;
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

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            // Moon layers
            spriteBatch.Draw(texture, drawPos, null, TwilightPurple * 0.25f, Projectile.rotation, origin, 0.65f * pulse, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, drawPos, null, MoonGold * 0.35f, Projectile.rotation, origin, 0.45f * pulse, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, drawPos, null, MoonSilver * 0.5f, Projectile.rotation, origin, 0.3f * pulse, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, drawPos, null, Color.White * 0.6f, Projectile.rotation, origin, 0.15f * pulse, SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            return false;
        }
    }

    /// <summary>
    /// Homing Leaf Shard - Spawned on critical hits
    /// </summary>
    public class HomingLeafShard : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/SoftGlow";
        
        private static readonly Color AutumnOrange = new Color(255, 140, 50);
        private static readonly Color AutumnRed = new Color(178, 34, 34);
        private static readonly Color AutumnGold = new Color(218, 165, 32);

        public override void SetDefaults()
        {
            Projectile.width = 12;
            Projectile.height = 12;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 180;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.alpha = 255;
        }

        public override void AI()
        {
            // Find and home to target
            NPC target = FindTarget();
            if (target != null)
            {
                Vector2 toTarget = target.Center - Projectile.Center;
                toTarget.Normalize();
                float homingStrength = 0.08f;
                float targetSpeed = 14f;
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, toTarget * targetSpeed, homingStrength);
            }

            Projectile.rotation = Projectile.velocity.ToRotation();

            // Leaf trail
            if (Main.rand.NextBool(3))
            {
                Vector2 trailVel = -Projectile.velocity * 0.1f + Main.rand.NextVector2Circular(1f, 1f);
                Color trailColor = Color.Lerp(AutumnOrange, AutumnRed, Main.rand.NextFloat()) * 0.5f;
                var trail = new GenericGlowParticle(Projectile.Center, trailVel, trailColor, 0.18f, 15, true);
                MagnumParticleHandler.SpawnParticle(trail);
            }

            Lighting.AddLight(Projectile.Center, AutumnOrange.ToVector3() * 0.3f);
        }

        private NPC FindTarget()
        {
            float maxDist = 350f;
            NPC closest = null;
            float closestDist = float.MaxValue;

            foreach (NPC npc in Main.npc)
            {
                if (!npc.CanBeChasedBy()) continue;
                
                float dist = Vector2.Distance(Projectile.Center, npc.Center);
                if (dist < maxDist && dist < closestDist)
                {
                    closest = npc;
                    closestDist = dist;
                }
            }

            return closest;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            CustomParticles.GenericFlare(target.Center, AutumnGold, 0.35f, 12);
            
            for (int i = 0; i < 4; i++)
            {
                Vector2 sparkVel = Main.rand.NextVector2Circular(3f, 3f);
                Color sparkColor = Color.Lerp(AutumnOrange, AutumnRed, Main.rand.NextFloat()) * 0.5f;
                var spark = new GenericGlowParticle(target.Center, sparkVel, sparkColor, 0.18f, 14, true);
                MagnumParticleHandler.SpawnParticle(spark);
            }
        }

        public override void OnKill(int timeLeft)
        {
            CustomParticles.GenericFlare(Projectile.Center, AutumnOrange, 0.3f, 12);

            for (int i = 0; i < 4; i++)
            {
                Vector2 burstVel = Main.rand.NextVector2Circular(3f, 3f);
                Color burstColor = Color.Lerp(AutumnOrange, AutumnRed, Main.rand.NextFloat()) * 0.4f;
                var burst = new GenericGlowParticle(Projectile.Center, burstVel, burstColor, 0.15f, 14, true);
                MagnumParticleHandler.SpawnParticle(burst);
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D texture = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/SoftGlow").Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = texture.Size() / 2f;

            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.15f + Projectile.whoAmI) * 0.12f + 1f;

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            spriteBatch.Draw(texture, drawPos, null, AutumnOrange * 0.4f, Projectile.rotation, origin, 0.25f * pulse, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, drawPos, null, AutumnGold * 0.5f, Projectile.rotation, origin, 0.15f * pulse, SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            return false;
        }
    }
}
