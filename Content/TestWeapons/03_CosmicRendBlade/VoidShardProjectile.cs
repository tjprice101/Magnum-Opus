using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.VFX;

namespace MagnumOpus.Content.TestWeapons._03_CosmicRendBlade
{
    /// <summary>
    /// 🌀 Void Shard — Homing cosmic shard projectile spawned by Step 2 (Rift Tear).
    /// Homes toward nearest enemy with gentle tracking, leaves a purple spark trail,
    /// shatters into void particles on death.
    /// </summary>
    public class VoidShardProjectile : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.NebulaBlaze2;

        private const float HomingStrength = 0.035f;
        private const float MaxSpeed = 16f;
        private const float HomingRange = 450f;

        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.MeleeNoSpeed;
            Projectile.penetrate = 2;
            Projectile.timeLeft = 150;
            Projectile.tileCollide = true;
            Projectile.extraUpdates = 1;
            Projectile.alpha = 80;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 12;
        }

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 12;
            ProjectileID.Sets.DrawScreenCheckFluff[Type] = 200;
        }

        public override void AI()
        {
            // Gentle homing toward nearest NPC
            float bestDist = HomingRange;
            Vector2 bestTarget = Vector2.Zero;
            bool foundTarget = false;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly || npc.dontTakeDamage) continue;
                float dist = Vector2.Distance(Projectile.Center, npc.Center);
                if (dist < bestDist)
                {
                    bestDist = dist;
                    bestTarget = npc.Center;
                    foundTarget = true;
                }
            }

            if (foundTarget)
            {
                Vector2 desired = (bestTarget - Projectile.Center).SafeNormalize(Vector2.Zero) * MaxSpeed;
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, desired, HomingStrength);
            }

            // Cap speed
            if (Projectile.velocity.Length() > MaxSpeed)
                Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.Zero) * MaxSpeed;

            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            // Cosmic dust trail
            if (Main.rand.NextBool(2))
            {
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.PurpleTorch,
                    -Projectile.velocity * 0.15f + Main.rand.NextVector2Circular(1f, 1f), 80, default, 1.1f);
                d.noGravity = true;
            }

            // Glow spark trail
            if (Main.rand.NextBool(3))
            {
                Color sparkColor = Color.Lerp(new Color(140, 60, 200), new Color(220, 160, 255), Main.rand.NextFloat());
                var glow = new GenericGlowParticle(Projectile.Center, -Projectile.velocity * 0.08f,
                    sparkColor * 0.6f, Main.rand.NextFloat(0.15f, 0.25f), Main.rand.Next(6, 12), true);
                MagnumParticleHandler.SpawnParticle(glow);
            }

            Lighting.AddLight(Projectile.Center, 0.25f, 0.08f, 0.4f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Small void burst on hit
            for (int i = 0; i < 4; i++)
            {
                Dust d = Dust.NewDustPerfect(target.Center, DustID.PurpleTorch,
                    Main.rand.NextVector2Circular(4f, 4f), 0, default, 1.2f);
                d.noGravity = true;
            }
        }

        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item10 with { Pitch = 0.4f, Volume = 0.5f }, Projectile.Center);

            // Void shatter burst
            for (int i = 0; i < 4; i++)
            {
                Vector2 sparkVel = Main.rand.NextVector2Circular(4f, 4f);
                Color sparkColor = Color.Lerp(new Color(140, 50, 200), new Color(255, 180, 255), Main.rand.NextFloat());
                var spark = new GlowSparkParticle(Projectile.Center, sparkVel, sparkColor,
                    Main.rand.NextFloat(0.2f, 0.35f), Main.rand.Next(8, 14));
                MagnumParticleHandler.SpawnParticle(spark);
            }

            for (int i = 0; i < 8; i++)
            {
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.PurpleTorch,
                    Main.rand.NextVector2Circular(5f, 5f), 0, default, 1.1f);
                d.noGravity = true;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Texture2D tex = Terraria.GameContent.TextureAssets.Extra[98].Value;
            Vector2 origin = tex.Size() * 0.5f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            // Afterimage trail
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;
                float progress = (float)i / Projectile.oldPos.Length;
                float alpha = (1f - progress) * 0.4f;
                float scale = 0.15f * (1f - progress * 0.5f);
                Color trailColor = Color.Lerp(new Color(200, 100, 255, 0), new Color(100, 30, 160, 0), progress);
                sb.Draw(tex, Projectile.oldPos[i] + Projectile.Size * 0.5f - Main.screenPosition, null,
                    trailColor * alpha, 0f, origin, scale, SpriteEffects.None, 0f);
            }

            // Core glow
            float pulse = 1f + (float)Math.Sin(Main.GameUpdateCount * 0.2f) * 0.1f;
            Color outerGlow = new Color(120, 40, 180, 0) * 0.45f;
            Color coreGlow = new Color(220, 160, 255, 0) * 0.65f;
            sb.Draw(tex, drawPos, null, outerGlow, 0f, origin, 0.22f * pulse, SpriteEffects.None, 0f);
            sb.Draw(tex, drawPos, null, coreGlow, 0f, origin, 0.12f * pulse, SpriteEffects.None, 0f);

            return false;
        }
    }
}
