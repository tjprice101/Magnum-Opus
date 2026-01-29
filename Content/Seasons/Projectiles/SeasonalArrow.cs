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
    /// Seasonal Arrow - Main projectile for Seasonal Bow
    /// Changes effects based on the season (ai[0])
    /// </summary>
    public class SeasonalArrow : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/SoftGlow";
        
        private static readonly Color SpringPink = new Color(255, 183, 197);
        private static readonly Color SpringGreen = new Color(144, 238, 144);
        private static readonly Color SummerGold = new Color(255, 215, 0);
        private static readonly Color SummerOrange = new Color(255, 140, 0);
        private static readonly Color AutumnOrange = new Color(255, 140, 50);
        private static readonly Color AutumnBrown = new Color(139, 90, 43);
        private static readonly Color WinterBlue = new Color(150, 220, 255);
        private static readonly Color WinterWhite = new Color(240, 250, 255);

        private int SeasonIndex => (int)Projectile.ai[0];

        private Color PrimaryColor => SeasonIndex switch
        {
            0 => SpringPink,
            1 => SummerGold,
            2 => AutumnOrange,
            _ => WinterBlue
        };

        private Color SecondaryColor => SeasonIndex switch
        {
            0 => SpringGreen,
            1 => SummerOrange,
            2 => AutumnBrown,
            _ => WinterWhite
        };

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

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
            Projectile.alpha = 30;
            Projectile.extraUpdates = 1;
            Projectile.arrow = true;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            // Light gravity
            Projectile.velocity.Y += 0.05f;

            // Season-specific trail
            if (Main.rand.NextBool(2))
            {
                Vector2 trailPos = Projectile.Center + Main.rand.NextVector2Circular(5f, 5f);
                Vector2 trailVel = -Projectile.velocity * 0.1f + Main.rand.NextVector2Circular(1f, 1f);
                Color trailColor = Color.Lerp(PrimaryColor, SecondaryColor, Main.rand.NextFloat()) * 0.5f;
                var trail = new GenericGlowParticle(trailPos, trailVel, trailColor, 0.22f, 16, true);
                MagnumParticleHandler.SpawnParticle(trail);
            }

            // Autumn-specific: Leave decay zones
            if (SeasonIndex == 2 && Projectile.timeLeft % 15 == 0)
            {
                Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, Vector2.Zero,
                    ModContent.ProjectileType<DecayZoneProjectile>(), Projectile.damage / 4, 0f, Projectile.owner);
            }

            Lighting.AddLight(Projectile.Center, PrimaryColor.ToVector3() * 0.4f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Season-specific on-hit effects
            switch (SeasonIndex)
            {
                case 0: // Spring - Split into homing petals
                    for (int i = 0; i < 5; i++)
                    {
                        float angle = MathHelper.TwoPi * i / 5f;
                        Vector2 petalVel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 7f);
                        Projectile.NewProjectile(Projectile.GetSource_FromAI(), target.Center, petalVel,
                            ModContent.ProjectileType<HomingPetalProjectile>(), Projectile.damage / 3, Projectile.knockBack * 0.3f, Projectile.owner);
                    }
                    target.AddBuff(BuffID.Poisoned, 180);
                    break;

                case 1: // Summer - Solar explosion
                    for (int i = 0; i < 6; i++)
                    {
                        float angle = MathHelper.TwoPi * i / 6f;
                        Vector2 flareVel = angle.ToRotationVector2() * Main.rand.NextFloat(5f, 8f);
                        Projectile.NewProjectile(Projectile.GetSource_FromAI(), target.Center, flareVel,
                            ModContent.ProjectileType<SummerArrowFlareProjectile>(), Projectile.damage / 4, Projectile.knockBack * 0.2f, Projectile.owner);
                    }
                    target.AddBuff(BuffID.OnFire3, 300);
                    target.AddBuff(BuffID.Daybreak, 120);
                    break;

                case 2: // Autumn - Life steal, debuffs
                    Player owner = Main.player[Projectile.owner];
                    owner.Heal(Math.Max(1, damageDone / 20));
                    target.AddBuff(BuffID.CursedInferno, 240);
                    target.AddBuff(BuffID.ShadowFlame, 180);
                    break;

                case 3: // Winter - Freeze and shatter
                    target.AddBuff(BuffID.Frostburn2, 300);
                    if (Main.rand.NextFloat() < 0.35f)
                    {
                        target.AddBuff(BuffID.Frozen, 90);
                        
                        // Shatter damage to nearby
                        for (int i = 0; i < Main.maxNPCs; i++)
                        {
                            NPC npc = Main.npc[i];
                            if (npc.active && !npc.friendly && npc.whoAmI != target.whoAmI && !npc.dontTakeDamage)
                            {
                                float dist = Vector2.Distance(target.Center, npc.Center);
                                if (dist < 100f)
                                {
                                    npc.SimpleStrikeNPC(Projectile.damage / 3, hit.HitDirection, false, Projectile.knockBack * 0.3f, DamageClass.Ranged);
                                    npc.AddBuff(BuffID.Frostburn2, 120);
                                }
                            }
                        }
                    }
                    break;
            }

            // Impact VFX
            CustomParticles.GenericFlare(target.Center, PrimaryColor, 0.6f, 20);
            CustomParticles.HaloRing(target.Center, SecondaryColor * 0.5f, 0.45f, 16);

            for (int i = 0; i < 8; i++)
            {
                Vector2 burstVel = Main.rand.NextVector2Circular(6f, 6f);
                Color burstColor = Color.Lerp(PrimaryColor, SecondaryColor, Main.rand.NextFloat()) * 0.55f;
                var burst = new GenericGlowParticle(target.Center, burstVel, burstColor, 0.28f, 18, true);
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
                float trailScale = 0.35f * (1f - progress * 0.5f);
                Color trailColor = Color.Lerp(PrimaryColor, SecondaryColor, progress) * alpha;

                Vector2 trailPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                spriteBatch.Draw(texture, trailPos, null, trailColor, Projectile.oldRot[i], origin, trailScale, SpriteEffects.None, 0f);
            }

            // Main glow
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.12f) * 0.1f + 1f;
            spriteBatch.Draw(texture, drawPos, null, SecondaryColor * 0.4f, Projectile.rotation, origin, 0.45f * pulse, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, drawPos, null, PrimaryColor * 0.6f, Projectile.rotation, origin, 0.32f, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, drawPos, null, Color.White * 0.75f, Projectile.rotation, origin, 0.18f, SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            return false;
        }

        public override void OnKill(int timeLeft)
        {
            CustomParticles.GenericFlare(Projectile.Center, PrimaryColor, 0.55f, 18);

            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 burstVel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 6f);
                var burst = new GenericGlowParticle(Projectile.Center, burstVel, PrimaryColor * 0.5f, 0.22f, 16, true);
                MagnumParticleHandler.SpawnParticle(burst);
            }
        }
    }

    /// <summary>
    /// Homing Petal - Spring arrow split projectile
    /// </summary>
    public class HomingPetalProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/SoftGlow";
        
        private static readonly Color SpringPink = new Color(255, 183, 197);
        private static readonly Color SpringGreen = new Color(144, 238, 144);

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 6;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 90;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.alpha = 40;
        }

        public override void AI()
        {
            Projectile.rotation += 0.15f;

            // Homing
            float homingRange = 350f;
            float homingStrength = 0.06f;

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
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, targetDir * 10f, homingStrength);
            }

            // Trail
            if (Main.rand.NextBool(2))
            {
                Vector2 trailVel = -Projectile.velocity * 0.1f;
                var trail = new GenericGlowParticle(Projectile.Center, trailVel, SpringPink * 0.45f, 0.18f, 14, true);
                MagnumParticleHandler.SpawnParticle(trail);
            }

            Lighting.AddLight(Projectile.Center, SpringPink.ToVector3() * 0.3f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Poisoned, 120);
            CustomParticles.GenericFlare(target.Center, SpringPink, 0.4f, 14);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D texture = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/SoftGlow").Value;
            Vector2 origin = texture.Size() / 2f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            spriteBatch.Draw(texture, drawPos, null, SpringPink * 0.55f, Projectile.rotation, origin, 0.3f, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, drawPos, null, SpringGreen * 0.7f, Projectile.rotation, origin, 0.18f, SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            return false;
        }
    }

    /// <summary>
    /// Solar Flare - Summer arrow explosion projectile
    /// </summary>
    public class SummerArrowFlareProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/SoftGlow";
        
        private static readonly Color SummerGold = new Color(255, 215, 0);
        private static readonly Color SummerOrange = new Color(255, 140, 0);

        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 2;
            Projectile.timeLeft = 45;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.alpha = 30;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
            Projectile.velocity *= 0.95f;

            if (Main.rand.NextBool(2))
            {
                var trail = new GenericGlowParticle(Projectile.Center, Main.rand.NextVector2Circular(2f, 2f), SummerOrange * 0.5f, 0.22f, 12, true);
                MagnumParticleHandler.SpawnParticle(trail);
            }

            Lighting.AddLight(Projectile.Center, SummerGold.ToVector3() * 0.5f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.OnFire3, 180);
            CustomParticles.GenericFlare(target.Center, SummerGold, 0.4f, 14);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D texture = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/SoftGlow").Value;
            Vector2 origin = texture.Size() / 2f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            spriteBatch.Draw(texture, drawPos, null, SummerOrange * 0.5f, Projectile.rotation, origin, 0.4f, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, drawPos, null, SummerGold * 0.65f, Projectile.rotation, origin, 0.28f, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, drawPos, null, Color.White * 0.8f, Projectile.rotation, origin, 0.15f, SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            return false;
        }
    }

    /// <summary>
    /// Decay Zone - Autumn arrow damage zone
    /// </summary>
    public class DecayZoneProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/SoftGlow";
        
        private static readonly Color AutumnOrange = new Color(255, 140, 50);
        private static readonly Color AutumnBrown = new Color(139, 90, 43);

        public override void SetDefaults()
        {
            Projectile.width = 60;
            Projectile.height = 60;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 90;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.alpha = 100;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 30;
        }

        public override void AI()
        {
            Projectile.velocity = Vector2.Zero;
            Projectile.alpha = (int)(100 + (155 * (1f - Projectile.timeLeft / 90f)));

            // Decay particles
            if (Main.rand.NextBool(3))
            {
                Vector2 particlePos = Projectile.Center + Main.rand.NextVector2Circular(25f, 25f);
                Vector2 particleVel = new Vector2(0, Main.rand.NextFloat(-2f, -0.5f));
                Color particleColor = Color.Lerp(AutumnOrange, AutumnBrown, Main.rand.NextFloat()) * 0.4f;
                var particle = new GenericGlowParticle(particlePos, particleVel, particleColor, 0.22f, 20, true);
                MagnumParticleHandler.SpawnParticle(particle);
            }

            Lighting.AddLight(Projectile.Center, AutumnOrange.ToVector3() * 0.25f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.CursedInferno, 90);
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

            spriteBatch.Draw(texture, drawPos, null, AutumnBrown * 0.3f * lifeProgress, 0f, origin, 0.7f, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, drawPos, null, AutumnOrange * 0.4f * lifeProgress, 0f, origin, 0.5f, SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            return false;
        }
    }
}
