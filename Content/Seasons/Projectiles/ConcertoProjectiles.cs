using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Content.Seasons.Projectiles
{
    /// <summary>
    /// Spring Verse - Blooming petal storm with homing capability
    /// </summary>
    public class SpringVerseProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/SoftGlow";
        
        private static readonly Color SpringPink = new Color(255, 183, 197);
        private static readonly Color SpringGreen = new Color(144, 238, 144);
        private static readonly Color SpringWhite = new Color(255, 250, 250);

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 12;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 3;
            Projectile.timeLeft = 180;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.alpha = 30;
        }

        public override void AI()
        {
            Projectile.rotation += 0.2f;

            // Gentle homing
            float homingRange = 300f;
            float homingStrength = 0.025f;

            NPC target = null;
            float closestDist = homingRange;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && !npc.friendly && npc.lifeMax > 5 && !npc.dontTakeDamage)
                {
                    float dist = Vector2.Distance(Projectile.Center, npc.Center);
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        target = npc;
                    }
                }
            }

            if (target != null)
            {
                Vector2 targetDir = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, targetDir * Projectile.velocity.Length(), homingStrength);
            }

            // Petal particles
            if (Main.rand.NextBool(2))
            {
                Vector2 trailPos = Projectile.Center + Main.rand.NextVector2Circular(8f, 8f);
                Vector2 trailVel = -Projectile.velocity * 0.12f + Main.rand.NextVector2Circular(2f, 2f);
                Color trailColor = Color.Lerp(SpringPink, SpringGreen, Main.rand.NextFloat()) * 0.55f;
                var trail = new GenericGlowParticle(trailPos, trailVel, trailColor, 0.25f, 20, true);
                MagnumParticleHandler.SpawnParticle(trail);
            }

            // Spawn secondary petal projectiles periodically
            if (Projectile.timeLeft % 20 == 0 && Main.myPlayer == Projectile.owner)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 petalVel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 5f);
                Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, petalVel,
                    ModContent.ProjectileType<VersePetalProjectile>(), Projectile.damage / 3, Projectile.knockBack * 0.3f, Projectile.owner);
            }

            Lighting.AddLight(Projectile.Center, SpringPink.ToVector3() * 0.5f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Poisoned, 240);

            CustomParticles.GenericFlare(target.Center, SpringPink, 0.65f, 20);
            CustomParticles.HaloRing(target.Center, SpringGreen * 0.5f, 0.4f, 16);

            // Petal burst
            for (int i = 0; i < 10; i++)
            {
                Vector2 burstVel = Main.rand.NextVector2Circular(7f, 7f);
                Color burstColor = Color.Lerp(SpringPink, SpringWhite, Main.rand.NextFloat()) * 0.5f;
                var burst = new GenericGlowParticle(target.Center, burstVel, burstColor, 0.28f, 22, true);
                MagnumParticleHandler.SpawnParticle(burst);
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D texture = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/SoftGlow").Value;
            Vector2 origin = texture.Size() / 2f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            // Trail
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                float progress = (float)i / Projectile.oldPos.Length;
                float alpha = (1f - progress) * 0.5f;
                float trailScale = 0.45f * (1f - progress * 0.5f);
                Color trailColor = Color.Lerp(SpringPink, SpringGreen, progress) * alpha;

                Vector2 trailPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                spriteBatch.Draw(texture, trailPos, null, trailColor, Projectile.oldRot[i], origin, trailScale, SpriteEffects.None, 0f);
            }

            // Main bloom
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.1f) * 0.12f + 1f;
            spriteBatch.Draw(texture, drawPos, null, SpringGreen * 0.35f, Projectile.rotation, origin, 0.6f * pulse, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, drawPos, null, SpringPink * 0.55f, Projectile.rotation, origin, 0.42f * pulse, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, drawPos, null, SpringWhite * 0.7f, Projectile.rotation, origin, 0.22f, SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            return false;
        }

        public override void OnKill(int timeLeft)
        {
            CustomParticles.GenericFlare(Projectile.Center, SpringPink, 0.6f, 18);

            for (int i = 0; i < 12; i++)
            {
                float angle = MathHelper.TwoPi * i / 12f;
                Vector2 burstVel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 7f);
                Color burstColor = Color.Lerp(SpringPink, SpringGreen, Main.rand.NextFloat()) * 0.45f;
                var burst = new GenericGlowParticle(Projectile.Center, burstVel, burstColor, 0.25f, 18, true);
                MagnumParticleHandler.SpawnParticle(burst);
            }
        }
    }

    /// <summary>
    /// Verse Petal - Secondary petal projectile from Spring Verse
    /// </summary>
    public class VersePetalProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/SoftGlow";
        
        private static readonly Color SpringPink = new Color(255, 183, 197);

        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 60;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.alpha = 50;
        }

        public override void AI()
        {
            Projectile.rotation += 0.15f;
            Projectile.velocity *= 0.97f;

            if (Main.rand.NextBool(3))
            {
                var trail = new GenericGlowParticle(Projectile.Center, Vector2.Zero, SpringPink * 0.35f, 0.15f, 12, true);
                MagnumParticleHandler.SpawnParticle(trail);
            }

            Lighting.AddLight(Projectile.Center, SpringPink.ToVector3() * 0.2f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Poisoned, 90);
            CustomParticles.GenericFlare(target.Center, SpringPink, 0.35f, 12);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D texture = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/SoftGlow").Value;
            Vector2 origin = texture.Size() / 2f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            spriteBatch.Draw(texture, drawPos, null, SpringPink * 0.6f, Projectile.rotation, origin, 0.25f, SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            return false;
        }
    }

    /// <summary>
    /// Summer Movement - Solar flare barrage with burning pillars
    /// </summary>
    public class SummerMovementProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/SoftGlow";
        
        private static readonly Color SummerGold = new Color(255, 215, 0);
        private static readonly Color SummerOrange = new Color(255, 140, 0);
        private static readonly Color SummerWhite = new Color(255, 255, 240);

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 15;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 18;
            Projectile.height = 18;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 150;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.alpha = 30;
            Projectile.extraUpdates = 1;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();

            // Solar particles
            if (Main.rand.NextBool(2))
            {
                Vector2 trailPos = Projectile.Center + Main.rand.NextVector2Circular(6f, 6f);
                Vector2 trailVel = -Projectile.velocity * 0.1f + Main.rand.NextVector2Circular(2f, 2f);
                Color trailColor = Color.Lerp(SummerGold, SummerOrange, Main.rand.NextFloat()) * 0.5f;
                var trail = new GenericGlowParticle(trailPos, trailVel, trailColor, 0.28f, 18, true);
                MagnumParticleHandler.SpawnParticle(trail);
            }

            // Leave burning pillars periodically
            if (Projectile.timeLeft % 25 == 0 && Main.myPlayer == Projectile.owner)
            {
                Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, Vector2.Zero,
                    ModContent.ProjectileType<SolarPillarProjectile>(), Projectile.damage / 2, 0f, Projectile.owner);
            }

            Lighting.AddLight(Projectile.Center, SummerGold.ToVector3() * 0.7f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.OnFire3, 300);
            target.AddBuff(BuffID.Daybreak, 180);

            CustomParticles.GenericFlare(target.Center, SummerGold, 0.7f, 22);
            CustomParticles.HaloRing(target.Center, SummerOrange * 0.5f, 0.5f, 18);

            // Solar burst
            for (int i = 0; i < 12; i++)
            {
                Vector2 burstVel = Main.rand.NextVector2Circular(8f, 8f);
                Color burstColor = Color.Lerp(SummerGold, SummerOrange, Main.rand.NextFloat()) * 0.5f;
                var burst = new GenericGlowParticle(target.Center, burstVel, burstColor, 0.3f, 20, true);
                MagnumParticleHandler.SpawnParticle(burst);
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D texture = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/SoftGlow").Value;
            Vector2 origin = texture.Size() / 2f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            // Trail
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                float progress = (float)i / Projectile.oldPos.Length;
                float alpha = (1f - progress) * 0.55f;
                float trailScale = 0.5f * (1f - progress * 0.5f);
                Color trailColor = Color.Lerp(SummerGold, SummerOrange, progress) * alpha;

                Vector2 trailPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                spriteBatch.Draw(texture, trailPos, null, trailColor, Projectile.oldRot[i], origin, trailScale, SpriteEffects.None, 0f);
            }

            // Main bloom
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.12f) * 0.1f + 1f;
            spriteBatch.Draw(texture, drawPos, null, SummerOrange * 0.4f, Projectile.rotation, origin, 0.55f * pulse, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, drawPos, null, SummerGold * 0.6f, Projectile.rotation, origin, 0.4f * pulse, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, drawPos, null, SummerWhite * 0.75f, Projectile.rotation, origin, 0.2f, SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            return false;
        }

        public override void OnKill(int timeLeft)
        {
            CustomParticles.GenericFlare(Projectile.Center, SummerGold, 0.7f, 20);
            CustomParticles.HaloRing(Projectile.Center, SummerOrange * 0.5f, 0.4f, 16);

            for (int i = 0; i < 10; i++)
            {
                float angle = MathHelper.TwoPi * i / 10f;
                Vector2 burstVel = angle.ToRotationVector2() * Main.rand.NextFloat(5f, 8f);
                var burst = new GenericGlowParticle(Projectile.Center, burstVel, SummerGold * 0.5f, 0.28f, 18, true);
                MagnumParticleHandler.SpawnParticle(burst);
            }
        }
    }

    /// <summary>
    /// Solar Pillar - Burning pillar left by Summer Movement
    /// </summary>
    public class SolarPillarProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/SoftGlow";
        
        private static readonly Color SummerGold = new Color(255, 215, 0);
        private static readonly Color SummerOrange = new Color(255, 140, 0);

        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 60;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 120;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.alpha = 50;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 15;
        }

        public override void AI()
        {
            Projectile.velocity = Vector2.Zero;

            // Rising flames
            if (Main.rand.NextBool(2))
            {
                Vector2 particlePos = Projectile.Center + Main.rand.NextVector2Circular(15f, 25f);
                Vector2 particleVel = new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-4f, -2f));
                Color particleColor = Color.Lerp(SummerGold, SummerOrange, Main.rand.NextFloat()) * 0.5f;
                var particle = new GenericGlowParticle(particlePos, particleVel, particleColor, 0.25f, 18, true);
                MagnumParticleHandler.SpawnParticle(particle);
            }

            Lighting.AddLight(Projectile.Center, SummerGold.ToVector3() * 0.5f * (Projectile.timeLeft / 120f));
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.OnFire3, 180);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D texture = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/SoftGlow").Value;
            Vector2 origin = texture.Size() / 2f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            float lifeProgress = Projectile.timeLeft / 120f;
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.15f) * 0.15f + 1f;

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            // Vertical pillar (stretch effect)
            spriteBatch.Draw(texture, drawPos, null, SummerOrange * 0.35f * lifeProgress, 0f, origin, new Vector2(0.4f, 0.9f) * pulse, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, drawPos, null, SummerGold * 0.5f * lifeProgress, 0f, origin, new Vector2(0.25f, 0.7f) * pulse, SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            return false;
        }
    }

    /// <summary>
    /// Autumn Passage - Decaying orbs that drain life
    /// </summary>
    public class AutumnPassageProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/SoftGlow";
        
        private static readonly Color AutumnOrange = new Color(255, 140, 50);
        private static readonly Color AutumnBrown = new Color(139, 90, 43);
        private static readonly Color AutumnRed = new Color(180, 50, 30);

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 22;
            Projectile.height = 22;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 4;
            Projectile.timeLeft = 180;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.alpha = 30;
        }

        public override void AI()
        {
            Projectile.rotation += 0.08f;
            Projectile.velocity *= 0.99f;

            // Decaying particles
            if (Main.rand.NextBool(2))
            {
                Vector2 trailPos = Projectile.Center + Main.rand.NextVector2Circular(8f, 8f);
                Vector2 trailVel = -Projectile.velocity * 0.08f + new Vector2(0, Main.rand.NextFloat(-1f, 0.5f));
                Color trailColor = Color.Lerp(AutumnOrange, AutumnBrown, Main.rand.NextFloat()) * 0.5f;
                var trail = new GenericGlowParticle(trailPos, trailVel, trailColor, 0.25f, 20, true);
                MagnumParticleHandler.SpawnParticle(trail);
            }

            // Leave decay fields
            if (Projectile.timeLeft % 30 == 0 && Main.myPlayer == Projectile.owner)
            {
                Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, Vector2.Zero,
                    ModContent.ProjectileType<DecayFieldProjectile>(), Projectile.damage / 4, 0f, Projectile.owner);
            }

            Lighting.AddLight(Projectile.Center, AutumnOrange.ToVector3() * 0.4f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.CursedInferno, 240);
            target.AddBuff(BuffID.ShadowFlame, 180);

            // Life steal
            Player owner = Main.player[Projectile.owner];
            owner.Heal(Math.Max(1, damageDone / 15));

            CustomParticles.GenericFlare(target.Center, AutumnOrange, 0.6f, 20);
            CustomParticles.HaloRing(target.Center, AutumnBrown * 0.5f, 0.45f, 16);

            // Decay burst
            for (int i = 0; i < 10; i++)
            {
                Vector2 burstVel = Main.rand.NextVector2Circular(6f, 6f);
                Color burstColor = Color.Lerp(AutumnOrange, AutumnRed, Main.rand.NextFloat()) * 0.5f;
                var burst = new GenericGlowParticle(target.Center, burstVel, burstColor, 0.28f, 20, true);
                MagnumParticleHandler.SpawnParticle(burst);
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D texture = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/SoftGlow").Value;
            Vector2 origin = texture.Size() / 2f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            // Trail
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                float progress = (float)i / Projectile.oldPos.Length;
                float alpha = (1f - progress) * 0.5f;
                float trailScale = 0.45f * (1f - progress * 0.5f);
                Color trailColor = Color.Lerp(AutumnOrange, AutumnBrown, progress) * alpha;

                Vector2 trailPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                spriteBatch.Draw(texture, trailPos, null, trailColor, Projectile.oldRot[i], origin, trailScale, SpriteEffects.None, 0f);
            }

            // Main bloom
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.1f) * 0.1f + 1f;
            spriteBatch.Draw(texture, drawPos, null, AutumnBrown * 0.35f, Projectile.rotation, origin, 0.55f * pulse, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, drawPos, null, AutumnOrange * 0.55f, Projectile.rotation, origin, 0.4f * pulse, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, drawPos, null, AutumnRed * 0.7f, Projectile.rotation, origin, 0.2f, SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            return false;
        }

        public override void OnKill(int timeLeft)
        {
            CustomParticles.GenericFlare(Projectile.Center, AutumnOrange, 0.6f, 18);

            for (int i = 0; i < 10; i++)
            {
                float angle = MathHelper.TwoPi * i / 10f;
                Vector2 burstVel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 6f);
                var burst = new GenericGlowParticle(Projectile.Center, burstVel, AutumnOrange * 0.5f, 0.25f, 18, true);
                MagnumParticleHandler.SpawnParticle(burst);
            }
        }
    }

    /// <summary>
    /// Decay Field - Area left by Autumn Passage
    /// </summary>
    public class DecayFieldProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/SoftGlow";
        
        private static readonly Color AutumnOrange = new Color(255, 140, 50);
        private static readonly Color AutumnBrown = new Color(139, 90, 43);

        public override void SetDefaults()
        {
            Projectile.width = 50;
            Projectile.height = 50;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 90;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.alpha = 80;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 20;
        }

        public override void AI()
        {
            Projectile.velocity = Vector2.Zero;
            Projectile.alpha = (int)(80 + (175 * (1f - Projectile.timeLeft / 90f)));

            // Decay wisps
            if (Main.rand.NextBool(3))
            {
                Vector2 particlePos = Projectile.Center + Main.rand.NextVector2Circular(20f, 20f);
                Vector2 particleVel = new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-2f, -0.5f));
                Color particleColor = Color.Lerp(AutumnOrange, AutumnBrown, Main.rand.NextFloat()) * 0.4f;
                var particle = new GenericGlowParticle(particlePos, particleVel, particleColor, 0.2f, 18, true);
                MagnumParticleHandler.SpawnParticle(particle);
            }

            Lighting.AddLight(Projectile.Center, AutumnOrange.ToVector3() * 0.2f * (Projectile.timeLeft / 90f));
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.ShadowFlame, 60);

            Player owner = Main.player[Projectile.owner];
            owner.Heal(Math.Max(1, damageDone / 25));
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D texture = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/SoftGlow").Value;
            Vector2 origin = texture.Size() / 2f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            float lifeProgress = Projectile.timeLeft / 90f;

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            spriteBatch.Draw(texture, drawPos, null, AutumnBrown * 0.3f * lifeProgress, 0f, origin, 0.6f, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, drawPos, null, AutumnOrange * 0.4f * lifeProgress, 0f, origin, 0.4f, SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            return false;
        }
    }

    /// <summary>
    /// Winter Finale - Blizzard burst with freeze chance
    /// </summary>
    public class WinterFinaleProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/SoftGlow";
        
        private static readonly Color WinterBlue = new Color(150, 220, 255);
        private static readonly Color WinterWhite = new Color(240, 250, 255);
        private static readonly Color WinterPurple = new Color(180, 160, 255);

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 12;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 2;
            Projectile.timeLeft = 150;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.alpha = 30;
            Projectile.extraUpdates = 1;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();

            // Ice particles
            if (Main.rand.NextBool(2))
            {
                Vector2 trailPos = Projectile.Center + Main.rand.NextVector2Circular(6f, 6f);
                Vector2 trailVel = -Projectile.velocity * 0.1f + Main.rand.NextVector2Circular(1f, 1f);
                Color trailColor = Color.Lerp(WinterBlue, WinterWhite, Main.rand.NextFloat()) * 0.5f;
                var trail = new GenericGlowParticle(trailPos, trailVel, trailColor, 0.25f, 18, true);
                MagnumParticleHandler.SpawnParticle(trail);
            }

            Lighting.AddLight(Projectile.Center, WinterBlue.ToVector3() * 0.5f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Frostburn2, 300);

            // 35% freeze chance
            if (Main.rand.NextFloat() < 0.35f)
            {
                target.AddBuff(BuffID.Frozen, 90);

                // Shatter damage to nearby enemies
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (npc.active && !npc.friendly && npc.whoAmI != target.whoAmI && !npc.dontTakeDamage)
                    {
                        float dist = Vector2.Distance(target.Center, npc.Center);
                        if (dist < 80f)
                        {
                            npc.SimpleStrikeNPC(Projectile.damage / 4, hit.HitDirection, false, Projectile.knockBack * 0.3f, DamageClass.Magic);
                            npc.AddBuff(BuffID.Frostburn2, 120);
                        }
                    }
                }

                // Shatter VFX
                CustomParticles.GenericFlare(target.Center, WinterWhite, 0.8f, 22);
                for (int i = 0; i < 12; i++)
                {
                    float angle = MathHelper.TwoPi * i / 12f;
                    Vector2 shardVel = angle.ToRotationVector2() * Main.rand.NextFloat(5f, 8f);
                    var shard = new GenericGlowParticle(target.Center, shardVel, WinterBlue * 0.6f, 0.22f, 18, true);
                    MagnumParticleHandler.SpawnParticle(shard);
                }
            }

            CustomParticles.GenericFlare(target.Center, WinterBlue, 0.65f, 20);
            CustomParticles.HaloRing(target.Center, WinterWhite * 0.5f, 0.45f, 16);

            // Ice burst
            for (int i = 0; i < 10; i++)
            {
                Vector2 burstVel = Main.rand.NextVector2Circular(6f, 6f);
                Color burstColor = Color.Lerp(WinterBlue, WinterPurple, Main.rand.NextFloat()) * 0.5f;
                var burst = new GenericGlowParticle(target.Center, burstVel, burstColor, 0.28f, 20, true);
                MagnumParticleHandler.SpawnParticle(burst);
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D texture = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/SoftGlow").Value;
            Vector2 origin = texture.Size() / 2f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            // Trail
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                float progress = (float)i / Projectile.oldPos.Length;
                float alpha = (1f - progress) * 0.55f;
                float trailScale = 0.45f * (1f - progress * 0.5f);
                Color trailColor = Color.Lerp(WinterBlue, WinterPurple, progress) * alpha;

                Vector2 trailPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                spriteBatch.Draw(texture, trailPos, null, trailColor, Projectile.oldRot[i], origin, trailScale, SpriteEffects.None, 0f);
            }

            // Main bloom
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.12f) * 0.1f + 1f;
            spriteBatch.Draw(texture, drawPos, null, WinterPurple * 0.35f, Projectile.rotation, origin, 0.55f * pulse, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, drawPos, null, WinterBlue * 0.55f, Projectile.rotation, origin, 0.4f * pulse, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, drawPos, null, WinterWhite * 0.75f, Projectile.rotation, origin, 0.2f, SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            return false;
        }

        public override void OnKill(int timeLeft)
        {
            CustomParticles.GenericFlare(Projectile.Center, WinterBlue, 0.7f, 20);
            CustomParticles.HaloRing(Projectile.Center, WinterWhite * 0.5f, 0.5f, 16);

            // Spawn ice shards on death
            if (Main.myPlayer == Projectile.owner)
            {
                for (int i = 0; i < 6; i++)
                {
                    float angle = MathHelper.TwoPi * i / 6f;
                    Vector2 shardVel = angle.ToRotationVector2() * Main.rand.NextFloat(5f, 8f);
                    Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, shardVel,
                        ModContent.ProjectileType<FinaleIceShardProjectile>(), Projectile.damage / 3, Projectile.knockBack * 0.3f, Projectile.owner);
                }
            }

            for (int i = 0; i < 12; i++)
            {
                float angle = MathHelper.TwoPi * i / 12f;
                Vector2 burstVel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 7f);
                var burst = new GenericGlowParticle(Projectile.Center, burstVel, WinterBlue * 0.5f, 0.25f, 18, true);
                MagnumParticleHandler.SpawnParticle(burst);
            }
        }
    }

    /// <summary>
    /// Finale Ice Shard - Secondary projectile from Winter Finale
    /// </summary>
    public class FinaleIceShardProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/SoftGlow";
        
        private static readonly Color WinterBlue = new Color(150, 220, 255);

        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 60;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.alpha = 50;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
            Projectile.velocity.Y += 0.08f;

            if (Main.rand.NextBool(3))
            {
                var trail = new GenericGlowParticle(Projectile.Center, -Projectile.velocity * 0.1f, WinterBlue * 0.35f, 0.15f, 12, true);
                MagnumParticleHandler.SpawnParticle(trail);
            }

            Lighting.AddLight(Projectile.Center, WinterBlue.ToVector3() * 0.25f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Frostburn2, 120);
            CustomParticles.GenericFlare(target.Center, WinterBlue, 0.35f, 12);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D texture = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/SoftGlow").Value;
            Vector2 origin = texture.Size() / 2f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            spriteBatch.Draw(texture, drawPos, null, WinterBlue * 0.6f, Projectile.rotation, origin, 0.22f, SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            return false;
        }
    }
}
