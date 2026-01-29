using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Content.Winter.Projectiles
{
    /// <summary>
    /// Icicle Bolt - Main projectile for Frostbite Repeater
    /// Piercing ice bolt that inflicts Hypothermia stacking debuff
    /// </summary>
    public class IcicleBolt : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/SoftGlow";
        
        private static readonly Color IceBlue = new Color(150, 220, 255);
        private static readonly Color FrostWhite = new Color(240, 250, 255);
        private static readonly Color CrystalCyan = new Color(100, 255, 255);

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 12;
            Projectile.height = 12;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 3;
            Projectile.timeLeft = 180;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.alpha = 30;
            Projectile.extraUpdates = 1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            // Ice crystal trail
            if (Main.rand.NextBool(2))
            {
                Vector2 trailPos = Projectile.Center + Main.rand.NextVector2Circular(6f, 6f);
                Vector2 trailVel = -Projectile.velocity * 0.08f + Main.rand.NextVector2Circular(1f, 1f);
                Color trailColor = Color.Lerp(IceBlue, CrystalCyan, Main.rand.NextFloat()) * 0.5f;
                var trail = new GenericGlowParticle(trailPos, trailVel, trailColor, 0.22f, 15, true);
                MagnumParticleHandler.SpawnParticle(trail);
            }

            Lighting.AddLight(Projectile.Center, IceBlue.ToVector3() * 0.4f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Apply Hypothermia debuff
            target.AddBuff(ModContent.BuffType<HypothermiaBuff>(), 300);
            target.AddBuff(BuffID.Frostburn2, 180);

            // Check for freeze at 5 stacks (handled in buff)
            
            // Impact VFX
            CustomParticles.GenericFlare(target.Center, CrystalCyan, 0.45f, 15);

            for (int i = 0; i < 5; i++)
            {
                Vector2 sparkVel = Main.rand.NextVector2Circular(4f, 4f);
                Color sparkColor = Color.Lerp(IceBlue, FrostWhite, Main.rand.NextFloat()) * 0.5f;
                var spark = new GenericGlowParticle(target.Center, sparkVel, sparkColor, 0.2f, 14, true);
                MagnumParticleHandler.SpawnParticle(spark);
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
                float alpha = (1f - progress) * 0.45f;
                float trailScale = 0.35f * (1f - progress * 0.5f);
                Color trailColor = Color.Lerp(CrystalCyan, IceBlue, progress) * alpha;

                Vector2 trailPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                spriteBatch.Draw(texture, trailPos, null, trailColor, Projectile.oldRot[i], origin, trailScale, SpriteEffects.None, 0f);
            }

            // Main glow
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.15f) * 0.1f + 1f;
            spriteBatch.Draw(texture, drawPos, null, IceBlue * 0.6f, Projectile.rotation, origin, 0.45f * pulse, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, drawPos, null, FrostWhite * 0.8f, Projectile.rotation, origin, 0.25f, SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            return false;
        }

        public override void OnKill(int timeLeft)
        {
            CustomParticles.GenericFlare(Projectile.Center, IceBlue, 0.45f, 16);

            for (int i = 0; i < 6; i++)
            {
                Vector2 burstVel = Main.rand.NextVector2Circular(4f, 4f);
                var burst = new GenericGlowParticle(Projectile.Center, burstVel, IceBlue * 0.5f, 0.2f, 14, true);
                MagnumParticleHandler.SpawnParticle(burst);
            }
        }
    }

    /// <summary>
    /// Blizzard Shard - Homing projectile for Frostbite Repeater's right-click
    /// </summary>
    public class BlizzardShardProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/SoftGlow";
        
        private static readonly Color IceBlue = new Color(150, 220, 255);
        private static readonly Color FrostWhite = new Color(240, 250, 255);
        private static readonly Color CrystalCyan = new Color(100, 255, 255);
        private static readonly Color DeepBlue = new Color(60, 100, 180);

        private bool hasTarget = false;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 12;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 18;
            Projectile.height = 18;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 150;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.alpha = 20;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            // Homing after initial delay
            if (Projectile.timeLeft < 135)
            {
                float homingRange = 400f;
                float homingStrength = 0.08f;

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
                    hasTarget = true;
                    Vector2 targetDir = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, targetDir * Projectile.velocity.Length(), homingStrength);
                }
            }

            // Blizzard particle trail
            if (Main.rand.NextBool(2))
            {
                Vector2 trailPos = Projectile.Center + Main.rand.NextVector2Circular(8f, 8f);
                Vector2 trailVel = -Projectile.velocity * 0.1f + Main.rand.NextVector2Circular(2f, 2f);
                Color trailColor = Color.Lerp(IceBlue, hasTarget ? CrystalCyan : DeepBlue, Main.rand.NextFloat()) * 0.55f;
                var trail = new GenericGlowParticle(trailPos, trailVel, trailColor, 0.25f, 18, true);
                MagnumParticleHandler.SpawnParticle(trail);
            }

            // Snowflake particles
            if (Main.rand.NextBool(4))
            {
                Vector2 snowVel = -Projectile.velocity * 0.2f + new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(0.5f, 1.5f));
                var snow = new GenericGlowParticle(Projectile.Center, snowVel, FrostWhite * 0.4f, 0.15f, 20, true);
                MagnumParticleHandler.SpawnParticle(snow);
            }

            Lighting.AddLight(Projectile.Center, CrystalCyan.ToVector3() * 0.5f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Apply Hypothermia x2
            target.AddBuff(ModContent.BuffType<HypothermiaBuff>(), 300);
            target.AddBuff(ModContent.BuffType<HypothermiaBuff>(), 300);
            target.AddBuff(BuffID.Frostburn2, 240);

            // Impact VFX
            CustomParticles.GenericFlare(target.Center, FrostWhite, 0.65f, 20);
            CustomParticles.HaloRing(target.Center, IceBlue * 0.5f, 0.4f, 16);

            for (int i = 0; i < 8; i++)
            {
                Vector2 burstVel = Main.rand.NextVector2Circular(5f, 5f);
                Color burstColor = Color.Lerp(IceBlue, CrystalCyan, Main.rand.NextFloat()) * 0.55f;
                var burst = new GenericGlowParticle(target.Center, burstVel, burstColor, 0.25f, 16, true);
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
                float trailScale = 0.4f * (1f - progress * 0.6f);
                Color trailColor = Color.Lerp(CrystalCyan, DeepBlue, progress) * alpha;

                Vector2 trailPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                spriteBatch.Draw(texture, trailPos, null, trailColor, Projectile.oldRot[i], origin, trailScale, SpriteEffects.None, 0f);
            }

            // Main glow
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.12f) * 0.12f + 1f;
            spriteBatch.Draw(texture, drawPos, null, DeepBlue * 0.4f, Projectile.rotation, origin, 0.55f * pulse, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, drawPos, null, IceBlue * 0.6f, Projectile.rotation, origin, 0.4f, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, drawPos, null, CrystalCyan * 0.7f, Projectile.rotation, origin, 0.28f, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, drawPos, null, FrostWhite * 0.85f, Projectile.rotation, origin, 0.15f, SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            return false;
        }

        public override void OnKill(int timeLeft)
        {
            CustomParticles.GenericFlare(Projectile.Center, FrostWhite, 0.6f, 18);
            CustomParticles.HaloRing(Projectile.Center, IceBlue * 0.5f, 0.35f, 14);

            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 burstVel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 6f);
                var burst = new GenericGlowParticle(Projectile.Center, burstVel, IceBlue * 0.5f, 0.22f, 16, true);
                MagnumParticleHandler.SpawnParticle(burst);
            }
        }
    }

    /// <summary>
    /// Hypothermia Debuff - Stacking slow, at 5 stacks freezes enemy
    /// </summary>
    public class HypothermiaBuff : ModBuff
    {
        public override string Texture => "Terraria/Images/Buff_" + BuffID.Frostburn;
        
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = true;
        }

        public override void Update(NPC npc, ref int buffIndex)
        {
            // Count stacks (multiple instances of this buff)
            int stacks = 0;
            for (int i = 0; i < npc.buffType.Length; i++)
            {
                if (npc.buffType[i] == Type && npc.buffTime[i] > 0)
                    stacks++;
            }

            // Apply increasing slow
            float slowMult = 1f - (stacks * 0.1f); // 10% slow per stack
            slowMult = Math.Max(0.3f, slowMult);

            // At 5 stacks, freeze
            if (stacks >= 5)
            {
                npc.AddBuff(BuffID.Frozen, 90);
                
                // Clear Hypothermia stacks
                for (int i = 0; i < npc.buffType.Length; i++)
                {
                    if (npc.buffType[i] == Type)
                        npc.buffTime[i] = 0;
                }

                // Freeze VFX
                CustomParticles.GenericFlare(npc.Center, new Color(100, 255, 255), 0.8f, 25);
                CustomParticles.HaloRing(npc.Center, new Color(150, 220, 255), 0.6f, 20);
            }

            // Visual frost particles on debuffed enemies
            if (Main.rand.NextBool(12))
            {
                Vector2 frostPos = npc.Center + Main.rand.NextVector2Circular(npc.width / 2f, npc.height / 2f);
                Vector2 frostVel = new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-1f, 0.5f));
                var frost = new GenericGlowParticle(frostPos, frostVel, new Color(150, 220, 255) * 0.4f, 0.18f, 20, true);
                MagnumParticleHandler.SpawnParticle(frost);
            }
        }
    }
}
