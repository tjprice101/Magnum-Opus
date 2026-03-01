using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.DiesIrae.Weapons.GrimoireOfCondemnation.Utilities;
using MagnumOpus.Content.DiesIrae.Weapons.GrimoireOfCondemnation.Particles;

namespace MagnumOpus.Content.DiesIrae.Weapons.GrimoireOfCondemnation.Projectiles
{
    /// <summary>
    /// Blazing Shard — A spiraling cursed shard that leaves a trail of hellfire.
    /// When 2+ shards are within 200 pixels, chain lightning arcs between them.
    /// On hit: brief sticking phase then detonation after 0.5s.
    /// </summary>
    public class BlazingShardProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/PointBloom";

        private static Asset<Texture2D> bloomTexture;
        private ref float ShardIndex => ref Projectile.ai[0];
        private int timer = 0;
        private bool stuck = false;
        private int stuckTimer = 0;
        private NPC stuckTarget = null;
        private Vector2 stuckOffset;

        // Static: track active shards for chain lightning
        private static readonly List<BlazingShardProjectile> activeShards = new();

        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 2;
            Projectile.timeLeft = 180;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 15;
        }

        public override void OnSpawn(IEntitySource source)
        {
            activeShards.Add(this);
        }

        public override void AI()
        {
            timer++;

            if (stuck && stuckTarget != null && stuckTarget.active)
            {
                Projectile.Center = stuckTarget.Center + stuckOffset;
                Projectile.velocity = Vector2.Zero;
                stuckTimer++;

                if (stuckTimer >= 30) // 0.5s then detonate
                {
                    GrimoireParticleHandler.Spawn(new GrimoireImpactBloom(Projectile.Center, GrimoireUtils.CondemnOrange, 1.5f, 15));
                    for (int i = 0; i < 8; i++)
                    {
                        Vector2 vel = Main.rand.NextVector2Circular(5f, 5f);
                        GrimoireParticleHandler.Spawn(new CursedShardTrailParticle(Projectile.Center, vel,
                            GrimoireUtils.GetGrimoireColor(Main.rand.NextFloat()), 0.15f, 12));
                    }
                    Projectile.Kill();
                    return;
                }
            }
            else if (!stuck)
            {
                // Spiral motion
                float spiralOffset = (float)Math.Sin(timer * 0.15f + ShardIndex * MathHelper.TwoPi / 3f) * 2f;
                Vector2 perp = new Vector2(-Projectile.velocity.Y, Projectile.velocity.X).SafeNormalize(Vector2.Zero);
                Projectile.Center += perp * spiralOffset;

                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;

                // Trail particles
                if (Main.rand.NextBool(2))
                {
                    Vector2 vel = -Projectile.velocity * 0.05f;
                    Color c = GrimoireUtils.GetGrimoireColor(Main.rand.NextFloat(0.3f, 0.7f));
                    GrimoireParticleHandler.Spawn(new CursedShardTrailParticle(Projectile.Center, vel, c, 0.15f, 10));
                }
            }

            // Chain lightning between nearby shards — proper zigzag bolt segments
            if (timer % 8 == 0)
            {
                foreach (var other in activeShards)
                {
                    if (other == this || !other.Projectile.active) continue;
                    float dist = Vector2.Distance(Projectile.Center, other.Projectile.Center);
                    if (dist < 200f && dist > 10f)
                    {
                        Color lightningColor = GrimoireUtils.GetGrimoireColor(Main.rand.NextFloat(0.4f, 0.8f));
                        GrimoireParticleHandler.Spawn(new ChainLightningSegment(
                            Projectile.Center, other.Projectile.Center, lightningColor, 6));

                        // Endpoint glow flashes at both shards
                        GrimoireParticleHandler.Spawn(new GrimoireImpactBloom(
                            Projectile.Center, lightningColor * 0.5f, 0.3f, 5));
                        GrimoireParticleHandler.Spawn(new GrimoireImpactBloom(
                            other.Projectile.Center, lightningColor * 0.5f, 0.3f, 5));
                    }
                }
            }

            Lighting.AddLight(Projectile.Center, GrimoireUtils.CondemnOrange.ToVector3() * 0.3f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.OnFire3, 180);
            if (!stuck)
            {
                stuck = true;
                stuckTarget = target;
                stuckOffset = Projectile.Center - target.Center;
                Projectile.tileCollide = false;
                GrimoireParticleHandler.Spawn(new GrimoireImpactBloom(target.Center, GrimoireUtils.CurseRed, 0.8f, 8));
            }
        }

        public override void OnKill(int timeLeft)
        {
            activeShards.Remove(this);
            for (int i = 0; i < 3; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(2f, 2f);
                GrimoireParticleHandler.Spawn(new GrimoireNoteParticle(Projectile.Center, vel,
                    GrimoireUtils.GetGrimoireColor(Main.rand.NextFloat(0.3f, 0.8f)), 0.35f, 30));
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            bloomTexture ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/PointBloom");
            if (!bloomTexture.IsLoaded) return false;
            var tex = bloomTexture.Value;

            float pulse = 0.8f + 0.2f * (float)Math.Sin(timer * 0.2f + ShardIndex);
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            // Outer glow
            Main.EntitySpriteDraw(tex, drawPos, null,
                GrimoireUtils.Additive(GrimoireUtils.CurseRed, 0.3f * pulse), 0f, tex.Size() / 2f, 0.5f, SpriteEffects.None, 0);
            // Core
            Main.EntitySpriteDraw(tex, drawPos, null,
                GrimoireUtils.Additive(GrimoireUtils.CondemnOrange, 0.5f * pulse), 0f, tex.Size() / 2f, 0.25f, SpriteEffects.None, 0);
            // Hot center
            Main.EntitySpriteDraw(tex, drawPos, null,
                GrimoireUtils.Additive(GrimoireUtils.ParchmentWhite, 0.3f * pulse), 0f, tex.Size() / 2f, 0.1f, SpriteEffects.None, 0);

            // Spiral trail when in flight — velocity-aligned stretch
            if (!stuck && Projectile.velocity.LengthSquared() > 1f)
            {
                float velRot = Projectile.velocity.ToRotation();
                Main.EntitySpriteDraw(tex, drawPos, null,
                    GrimoireUtils.Additive(GrimoireUtils.CondemnOrange, 0.2f), velRot, tex.Size() / 2f,
                    new Vector2(0.6f, 0.08f), SpriteEffects.None, 0);
            }

            // Stuck pulsating glow — ticking time bomb visual
            if (stuck && stuckTarget != null)
            {
                float stuckProgress = stuckTimer / 30f;
                float tickPulse = (float)Math.Abs(Math.Sin(stuckTimer * 0.3f));
                Color tickColor = Color.Lerp(GrimoireUtils.CurseRed, GrimoireUtils.ParchmentWhite, stuckProgress * tickPulse);
                float tickScale = 0.4f + stuckProgress * 0.6f + tickPulse * 0.2f;
                Main.EntitySpriteDraw(tex, drawPos, null,
                    GrimoireUtils.Additive(tickColor, 0.35f * (0.5f + stuckProgress)), 0f, tex.Size() / 2f, tickScale, SpriteEffects.None, 0);

                // Cross-flare at critical moment (last 10 ticks)
                if (stuckTimer >= 20)
                {
                    float urgency = (stuckTimer - 20f) / 10f;
                    Color crossColor = GrimoireUtils.Additive(GrimoireUtils.ParchmentWhite, 0.3f * urgency);
                    Main.EntitySpriteDraw(tex, drawPos, null, crossColor, 0f, tex.Size() / 2f,
                        new Vector2(0.06f, 0.6f * urgency), SpriteEffects.None, 0);
                    Main.EntitySpriteDraw(tex, drawPos, null, crossColor, MathHelper.PiOver2, tex.Size() / 2f,
                        new Vector2(0.06f, 0.6f * urgency), SpriteEffects.None, 0);
                }
            }

            return false;
        }
    }
}
