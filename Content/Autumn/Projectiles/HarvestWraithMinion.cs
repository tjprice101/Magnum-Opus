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

namespace MagnumOpus.Content.Autumn.Projectiles
{
    /// <summary>
    /// Harvest Wraith Minion - Spectral reaper that attacks enemies
    /// Features:
    /// - Phasing movement toward targets
    /// - Scythe melee attacks
    /// - Death Toll AoE attack
    /// - Spawns healing orbs on kills
    /// </summary>
    public class HarvestWraithMinion : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/SoftGlow";
        
        private static readonly Color DecayPurple = new Color(100, 50, 120);
        private static readonly Color WraithGreen = new Color(120, 180, 100);
        private static readonly Color SoulWhite = new Color(240, 240, 255);

        private int tollCooldown = 0;
        private const int TollInterval = 180; // 3 seconds
        private float hoverOffset = 0f;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.MinionSacrificable[Projectile.type] = true;
            ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true;
            Main.projPet[Projectile.type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = 40;
            Projectile.height = 50;
            Projectile.netImportant = true;
            Projectile.friendly = true;
            Projectile.minion = true;
            Projectile.minionSlots = 1f;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 2;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.DamageType = DamageClass.Summon;
        }

        public override bool? CanCutTiles() => false;
        public override bool MinionContactDamage() => true;

        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];
            if (!CheckActive(owner)) return;

            int minionIndex = GetMinionIndex(owner);
            int minionCount = owner.ownedProjectileCounts[Projectile.type];

            // Update cooldowns
            if (tollCooldown > 0) tollCooldown--;
            hoverOffset += 0.03f;

            // Find target
            NPC target = FindTarget(owner);

            if (target != null && target.active)
            {
                AttackTarget(owner, target, minionIndex, minionCount);
            }
            else
            {
                IdleMovement(owner, minionIndex, minionCount);
            }

            // Visual effects
            SpawnAmbientEffects(minionIndex);

            // Death Toll
            if (tollCooldown <= 0 && target != null)
            {
                PerformDeathToll();
            }

            // Light
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.06f + minionIndex * 0.5f) * 0.2f + 0.6f;
            Lighting.AddLight(Projectile.Center, WraithGreen.ToVector3() * pulse * 0.5f);
        }

        private bool CheckActive(Player owner)
        {
            if (owner.dead || !owner.active)
            {
                owner.ClearBuff(ModContent.BuffType<Weapons.HarvestWraithBuff>());
                return false;
            }
            
            if (owner.HasBuff(ModContent.BuffType<Weapons.HarvestWraithBuff>()))
            {
                Projectile.timeLeft = 2;
            }
            
            return true;
        }

        private int GetMinionIndex(Player owner)
        {
            int index = 0;
            foreach (Projectile proj in Main.projectile)
            {
                if (proj.active && proj.owner == Projectile.owner && proj.type == Projectile.type)
                {
                    if (proj.whoAmI == Projectile.whoAmI) return index;
                    index++;
                }
            }
            return index;
        }

        private NPC FindTarget(Player owner)
        {
            float maxRange = 700f;
            NPC target = null;
            float closestDist = float.MaxValue;

            if (owner.HasMinionAttackTargetNPC)
            {
                NPC targetNPC = Main.npc[owner.MinionAttackTargetNPC];
                if (targetNPC.CanBeChasedBy() && Vector2.Distance(owner.Center, targetNPC.Center) < maxRange)
                {
                    return targetNPC;
                }
            }

            foreach (NPC npc in Main.npc)
            {
                if (!npc.CanBeChasedBy()) continue;
                
                float dist = Vector2.Distance(owner.Center, npc.Center);
                if (dist < maxRange && dist < closestDist)
                {
                    closestDist = dist;
                    target = npc;
                }
            }

            return target;
        }

        private void AttackTarget(Player owner, NPC target, int minionIndex, int minionCount)
        {
            // Phase toward target
            Vector2 toTarget = target.Center - Projectile.Center;
            float dist = toTarget.Length();
            
            if (dist > 60f)
            {
                toTarget.Normalize();
                float speed = 16f;
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, toTarget * speed, 0.12f);
            }
            else
            {
                // Circle around close targets
                float orbitAngle = Main.GameUpdateCount * 0.06f + minionIndex * MathHelper.TwoPi / minionCount;
                Vector2 orbitPos = target.Center + orbitAngle.ToRotationVector2() * 60f;
                Vector2 toOrbit = orbitPos - Projectile.Center;
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, toOrbit * 0.15f, 0.1f);
            }

            // Face target
            Projectile.rotation = (target.Center - Projectile.Center).ToRotation();
            Projectile.spriteDirection = target.Center.X > Projectile.Center.X ? 1 : -1;
        }

        private void PerformDeathToll()
        {
            tollCooldown = TollInterval;

            // AoE damage
            float tollRadius = 150f;
            foreach (NPC npc in Main.npc)
            {
                if (!npc.CanBeChasedBy()) continue;
                if (Vector2.Distance(Projectile.Center, npc.Center) < tollRadius)
                {
                    // Deal damage
                    if (Main.myPlayer == Projectile.owner)
                    {
                        Main.player[Projectile.owner].ApplyDamageToNPC(npc, Projectile.damage / 2, 0f, 0, false);
                    }
                }
            }

            // Toll VFX
            CustomParticles.GenericFlare(Projectile.Center, DecayPurple, 0.7f, 22);
            CustomParticles.HaloRing(Projectile.Center, WraithGreen * 0.6f, 0.6f, 20);
            CustomParticles.HaloRing(Projectile.Center, DecayPurple * 0.4f, 0.45f, 18);

            // Radial death energy
            for (int i = 0; i < 12; i++)
            {
                float angle = MathHelper.TwoPi * i / 12f;
                Vector2 burstVel = angle.ToRotationVector2() * Main.rand.NextFloat(5f, 9f);
                Color burstColor = Color.Lerp(DecayPurple, WraithGreen, (float)i / 12f) * 0.6f;
                var burst = new GenericGlowParticle(Projectile.Center, burstVel, burstColor, 0.32f, 24, true);
                MagnumParticleHandler.SpawnParticle(burst);
            }
        }

        private void IdleMovement(Player owner, int minionIndex, int minionCount)
        {
            // Float near player
            float angleOffset = MathHelper.TwoPi * minionIndex / minionCount;
            float idleRadius = 80f + minionIndex * 20f;
            float idleAngle = Main.GameUpdateCount * 0.02f + angleOffset;
            
            Vector2 hoverY = new Vector2(0, (float)Math.Sin(hoverOffset + minionIndex) * 10f);
            Vector2 idealPos = owner.Center + idleAngle.ToRotationVector2() * idleRadius + hoverY + new Vector2(0, -60f);
            
            Vector2 toIdeal = idealPos - Projectile.Center;
            Projectile.velocity = Vector2.Lerp(Projectile.velocity, toIdeal * 0.08f, 0.08f);

            Projectile.rotation = Projectile.velocity.X * 0.03f;
            Projectile.spriteDirection = owner.direction;
        }

        private void SpawnAmbientEffects(int minionIndex)
        {
            // Ghostly trail
            if (Main.rand.NextBool(3))
            {
                Vector2 trailVel = -Projectile.velocity * 0.1f + Main.rand.NextVector2Circular(1f, 1f);
                Color trailColor = Color.Lerp(DecayPurple, WraithGreen, Main.rand.NextFloat()) * 0.4f;
                var trail = new GenericGlowParticle(Projectile.Center, trailVel, trailColor, 0.25f, 22, true);
                MagnumParticleHandler.SpawnParticle(trail);
            }

            // Soul wisps
            if (Main.rand.NextBool(12))
            {
                Vector2 wispVel = new Vector2(Main.rand.NextFloat(-1f, 1f), -Main.rand.NextFloat(1f, 3f));
                Color wispColor = Color.Lerp(WraithGreen, SoulWhite, Main.rand.NextFloat()) * 0.4f;
                var wisp = new GenericGlowParticle(Projectile.Center + Main.rand.NextVector2Circular(15f, 15f), wispVel, wispColor, 0.18f, 25, true);
                MagnumParticleHandler.SpawnParticle(wisp);
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Apply decay
            target.AddBuff(BuffID.CursedInferno, 120);

            // Hit VFX
            CustomParticles.GenericFlare(target.Center, WraithGreen, 0.45f, 15);

            for (int i = 0; i < 5; i++)
            {
                Vector2 sparkVel = Main.rand.NextVector2Circular(4f, 4f);
                Color sparkColor = Color.Lerp(DecayPurple, WraithGreen, Main.rand.NextFloat()) * 0.5f;
                var spark = new GenericGlowParticle(target.Center, sparkVel, sparkColor, 0.2f, 16, true);
                MagnumParticleHandler.SpawnParticle(spark);
            }
        }

        public override void OnKill(int timeLeft)
        {
            // Dissipation VFX
            CustomParticles.GenericFlare(Projectile.Center, WraithGreen, 0.55f, 18);
            CustomParticles.HaloRing(Projectile.Center, DecayPurple * 0.5f, 0.4f, 15);

            for (int i = 0; i < 8; i++)
            {
                Vector2 burstVel = Main.rand.NextVector2Circular(5f, 5f) + new Vector2(0, -2f);
                Color burstColor = Color.Lerp(DecayPurple, WraithGreen, Main.rand.NextFloat()) * 0.5f;
                var burst = new GenericGlowParticle(Projectile.Center, burstVel, burstColor, 0.25f, 22, true);
                MagnumParticleHandler.SpawnParticle(burst);
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D texture = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/SoftGlow").Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = texture.Size() / 2f;

            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.08f + Projectile.whoAmI * 0.3f) * 0.15f + 1f;
            float tollProgress = 1f - (float)tollCooldown / TollInterval;

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            // Outer spectral layer
            spriteBatch.Draw(texture, drawPos, null, DecayPurple * 0.25f, 0f, origin, 0.55f * pulse, SpriteEffects.None, 0f);
            // Middle ectoplasm
            spriteBatch.Draw(texture, drawPos, null, WraithGreen * 0.4f, 0f, origin, 0.38f * pulse, SpriteEffects.None, 0f);
            // Core
            spriteBatch.Draw(texture, drawPos, null, SoulWhite * 0.5f, 0f, origin, 0.22f * pulse, SpriteEffects.None, 0f);
            // Hot center
            spriteBatch.Draw(texture, drawPos, null, Color.White * 0.55f, 0f, origin, 0.1f * pulse, SpriteEffects.None, 0f);

            // Toll charge indicator (orbiting particles when near toll)
            if (tollProgress > 0.7f)
            {
                for (int i = 0; i < 3; i++)
                {
                    float orbAngle = Main.GameUpdateCount * 0.1f + MathHelper.TwoPi * i / 3f;
                    float orbDist = 25f * (tollProgress - 0.7f) / 0.3f;
                    Vector2 orbPos = drawPos + orbAngle.ToRotationVector2() * orbDist;
                    spriteBatch.Draw(texture, orbPos, null, DecayPurple * 0.5f, 0f, origin, 0.12f, SpriteEffects.None, 0f);
                }
            }

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            return false;
        }
    }

    /// <summary>
    /// Soul Harvest Orb - Healing pickup spawned on wraith kills
    /// </summary>
    public class SoulHarvestOrb : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/SoftGlow";
        
        private static readonly Color WraithGreen = new Color(120, 180, 100);
        private static readonly Color SoulWhite = new Color(240, 240, 255);

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
            
            // Float and then home to player
            if (Projectile.ai[0] < 45)
            {
                // Initial float
                Projectile.ai[0]++;
                Projectile.velocity *= 0.95f;
                Projectile.velocity.Y -= 0.1f;
            }
            else
            {
                // Home to player
                Vector2 toPlayer = owner.Center - Projectile.Center;
                float dist = toPlayer.Length();
                
                if (dist < 30f)
                {
                    // Heal on contact
                    owner.Heal(15);
                    Projectile.Kill();
                    return;
                }

                toPlayer.Normalize();
                float speed = 12f;
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, toPlayer * speed, 0.1f);
            }

            // Trail
            if (Main.rand.NextBool(2))
            {
                Vector2 trailVel = -Projectile.velocity * 0.1f + Main.rand.NextVector2Circular(1f, 1f);
                Color trailColor = Color.Lerp(WraithGreen, SoulWhite, Main.rand.NextFloat()) * 0.5f;
                var trail = new GenericGlowParticle(Projectile.Center, trailVel, trailColor, 0.2f, 18, true);
                MagnumParticleHandler.SpawnParticle(trail);
            }

            Lighting.AddLight(Projectile.Center, WraithGreen.ToVector3() * 0.4f);
        }

        public override void OnKill(int timeLeft)
        {
            CustomParticles.GenericFlare(Projectile.Center, SoulWhite, 0.4f, 15);
            CustomParticles.HaloRing(Projectile.Center, WraithGreen * 0.4f, 0.3f, 12);

            for (int i = 0; i < 6; i++)
            {
                Vector2 burstVel = Main.rand.NextVector2Circular(3f, 3f);
                Color burstColor = Color.Lerp(WraithGreen, SoulWhite, Main.rand.NextFloat()) * 0.5f;
                var burst = new GenericGlowParticle(Projectile.Center, burstVel, burstColor, 0.18f, 15, true);
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

            spriteBatch.Draw(texture, drawPos, null, WraithGreen * 0.35f, 0f, origin, 0.3f * pulse, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, drawPos, null, SoulWhite * 0.5f, 0f, origin, 0.18f * pulse, SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            return false;
        }
    }
}
