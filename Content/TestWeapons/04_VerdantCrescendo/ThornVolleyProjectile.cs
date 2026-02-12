using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Content.TestWeapons._04_VerdantCrescendo
{
    /// <summary>
    /// 🌿 Thorn Volley Projectile — piercing thorn shards spawned by Step 2 (Thorn Eruption).
    /// Five shards launch in a spread from the blade tip. Each shard arcs through the air,
    /// pierces 2 enemies, and bursts into leaf particles on death.
    /// </summary>
    public class ThornVolleyProjectile : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.SeedPlantera;

        private bool hasHitTile = false;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailingMode[Type] = 0;
            ProjectileID.Sets.TrailCacheLength[Type] = 10;
        }

        public override void SetDefaults()
        {
            Projectile.width = 12;
            Projectile.height = 12;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.MeleeNoSpeed;
            Projectile.tileCollide = true;
            Projectile.penetrate = 3;
            Projectile.timeLeft = 150;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
            Projectile.alpha = 255; // Invisible sprite — fully custom drawn
        }

        public override void AI()
        {
            // Gentle gravity arc
            Projectile.velocity.Y += 0.18f;
            if (Projectile.velocity.Y > 14f) Projectile.velocity.Y = 14f;

            // Spin rotation
            Projectile.rotation += Projectile.velocity.X * 0.04f;

            // Leaf dust trail — 2 per frame for density
            for (int i = 0; i < 2; i++)
            {
                Vector2 dustOffset = Main.rand.NextVector2Circular(6f, 6f);
                Vector2 dustVel = -Projectile.velocity * 0.15f + Main.rand.NextVector2Circular(0.5f, 0.5f);
                Dust d = Dust.NewDustPerfect(Projectile.Center + dustOffset, DustID.JungleGrass, dustVel,
                    0, default, Main.rand.NextFloat(1.0f, 1.4f));
                d.noGravity = true;
                d.fadeIn = 1.1f;
            }

            // Green sparkles — 1-in-2
            if (Main.rand.NextBool(2))
            {
                Dust gem = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(8f, 8f),
                    DustID.GemEmerald, -Projectile.velocity * 0.1f, 0, default, 0.7f);
                gem.noGravity = true;
            }

            // Glow particle trail
            if (Main.rand.NextBool(3))
            {
                Color glowColor = Color.Lerp(new Color(60, 180, 70), new Color(180, 230, 80), Main.rand.NextFloat());
                var glow = new GenericGlowParticle(Projectile.Center, -Projectile.velocity * 0.1f,
                    glowColor * 0.5f, Main.rand.NextFloat(0.12f, 0.22f), Main.rand.Next(8, 14), true);
                MagnumParticleHandler.SpawnParticle(glow);
            }

            // Lighting
            Lighting.AddLight(Projectile.Center, 0.15f, 0.4f, 0.1f);
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            // Stick into ground briefly then die
            if (!hasHitTile)
            {
                hasHitTile = true;
                Projectile.velocity = Vector2.Zero;
                Projectile.tileCollide = false;
                Projectile.timeLeft = Math.Min(Projectile.timeLeft, 20);
                SoundEngine.PlaySound(SoundID.Dig with { Pitch = 0.4f, Volume = 0.4f }, Projectile.Center);

                // Impact dust
                for (int i = 0; i < 4; i++)
                {
                    Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.JungleGrass,
                        Main.rand.NextVector2Circular(3f, 3f), 0, default, 1.1f);
                    d.noGravity = true;
                }
                return false;
            }
            return true;
        }

        public override void OnKill(int timeLeft)
        {
            // Leaf burst
            for (int i = 0; i < 8; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(4f, 4f);
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.JungleGrass, vel, 0, default, 1.4f);
                d.noGravity = true;
            }

            // Green glow sparks
            for (int i = 0; i < 3; i++)
            {
                var spark = new GlowSparkParticle(Projectile.Center,
                    Main.rand.NextVector2Circular(5f, 5f),
                    Color.Lerp(new Color(60, 200, 80), new Color(200, 240, 100), Main.rand.NextFloat()),
                    Main.rand.NextFloat(0.2f, 0.4f), Main.rand.Next(10, 18));
                MagnumParticleHandler.SpawnParticle(spark);
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int remainingDamageCount)
        {
            // Poison on hit
            target.AddBuff(BuffID.Poisoned, 180);

            // Impact leaves
            for (int i = 0; i < 4; i++)
            {
                Dust d = Dust.NewDustPerfect(target.Center, DustID.JungleGrass,
                    Main.rand.NextVector2Circular(4f, 4f), 0, default, 1.2f);
                d.noGravity = true;
            }

            // Green spark on hit
            var glow = new GenericGlowParticle(target.Center, Main.rand.NextVector2Circular(3f, 3f),
                new Color(80, 220, 90) * 0.7f, 0.25f, 12, true);
            MagnumParticleHandler.SpawnParticle(glow);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;

            // Afterimage trail
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) break;
                float progress = (float)i / Projectile.oldPos.Length;
                float alpha = (1f - progress) * 0.5f;
                Color trailColor = Color.Lerp(new Color(100, 220, 80), new Color(40, 120, 40), progress);
                trailColor.A = 0;

                Texture2D glowTex = TextureAssets.Extra[98].Value;
                Vector2 drawPos = Projectile.oldPos[i] + Projectile.Size * 0.5f - Main.screenPosition;
                float scale = (1f - progress) * 0.12f;

                sb.Draw(glowTex, drawPos, null, trailColor * alpha, 0f, glowTex.Size() * 0.5f, scale, SpriteEffects.None, 0f);
            }

            // Core glow
            Texture2D coreTex = TextureAssets.Extra[98].Value;
            Vector2 corePos = Projectile.Center - Main.screenPosition;
            float pulse = 1f + (float)Math.Sin(Main.GameUpdateCount * 0.2f) * 0.1f;

            // Outer green glow
            Color outerGlow = new Color(60, 180, 70, 0) * 0.5f;
            sb.Draw(coreTex, corePos, null, outerGlow, Projectile.rotation, coreTex.Size() * 0.5f,
                0.18f * pulse, SpriteEffects.None, 0f);

            // Inner bright core
            Color innerGlow = new Color(180, 240, 120, 0) * 0.7f;
            sb.Draw(coreTex, corePos, null, innerGlow, Projectile.rotation, coreTex.Size() * 0.5f,
                0.09f * pulse, SpriteEffects.None, 0f);

            // White-hot center
            sb.Draw(coreTex, corePos, null, Color.White * 0.4f, 0f, coreTex.Size() * 0.5f,
                0.04f * pulse, SpriteEffects.None, 0f);

            return false;
        }
    }
}
