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
    /// 🌀 Void Rift — Expanding dimensional tear AoE spawned by Step 3 (Dimensional Severance).
    /// Opens at the blade tip, expands into a circular rift of void energy,
    /// damages all enemies in its radius, then collapses with an implosion burst.
    /// </summary>
    public class VoidRiftProjectile : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.NebulaBlaze1;

        private const float MaxRadius = 160f;
        private const int ExpandDuration = 24;
        private const int LingerDuration = 50;
        private const int TotalDuration = ExpandDuration + LingerDuration;

        private float Radius
        {
            get
            {
                int timer = (int)Projectile.ai[1];
                if (timer <= ExpandDuration)
                {
                    float t = (float)timer / ExpandDuration;
                    return MaxRadius * (1f - (1f - t) * (1f - t)); // Ease-out quadratic
                }
                return MaxRadius;
            }
        }

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.MeleeNoSpeed;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.timeLeft = TotalDuration;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 18;
            Projectile.alpha = 255;
        }

        public override bool ShouldUpdatePosition() => false;

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float radius = Radius;
            if (radius < 10f) return false;

            // Circular collision
            Vector2 closestPoint = new Vector2(
                MathHelper.Clamp(Projectile.Center.X, targetHitbox.Left, targetHitbox.Right),
                MathHelper.Clamp(Projectile.Center.Y, targetHitbox.Top, targetHitbox.Bottom));
            return Vector2.DistanceSquared(Projectile.Center, closestPoint) <= radius * radius;
        }

        public override void AI()
        {
            Projectile.ai[1]++;
            int timer = (int)Projectile.ai[1];
            float radius = Radius;
            float progress = (float)timer / TotalDuration;

            // Expanding void ring particles
            int ringCount = (int)(6 + radius * 0.12f);
            for (int i = 0; i < ringCount; i++)
            {
                float angle = MathHelper.TwoPi * i / ringCount + Main.GameUpdateCount * 0.04f;
                Vector2 ringPos = Projectile.Center + angle.ToRotationVector2() * radius;
                Dust d = Dust.NewDustPerfect(ringPos, DustID.PurpleTorch, Vector2.Zero, 100, default, 1.3f);
                d.noGravity = true;
                d.fadeIn = 1.2f;
            }

            // Interior void mist
            if (radius > 20f && Main.rand.NextBool(2))
            {
                Vector2 interiorPos = Projectile.Center + Main.rand.NextVector2Circular(radius * 0.7f, radius * 0.7f);
                Color voidColor = Color.Lerp(new Color(40, 10, 60), new Color(120, 40, 180), Main.rand.NextFloat());
                var mist = new GenericGlowParticle(interiorPos, Main.rand.NextVector2Circular(0.6f, 0.6f),
                    voidColor * 0.5f, Main.rand.NextFloat(0.2f, 0.4f), Main.rand.Next(8, 16), true);
                MagnumParticleHandler.SpawnParticle(mist);
            }

            // Cosmic spark crystals every few ticks
            if (timer % 6 == 0 && radius > 30f)
            {
                for (int i = 0; i < 3; i++)
                {
                    float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                    float dist = Main.rand.NextFloat(0.3f, 0.9f) * radius;
                    Vector2 sparkPos = Projectile.Center + angle.ToRotationVector2() * dist;
                    Color sparkColor = Color.Lerp(new Color(160, 60, 220), new Color(255, 180, 255), Main.rand.NextFloat());
                    var spark = new GlowSparkParticle(sparkPos, Main.rand.NextVector2Circular(0.5f, 0.5f),
                        sparkColor, Main.rand.NextFloat(0.15f, 0.3f), Main.rand.Next(8, 14));
                    MagnumParticleHandler.SpawnParticle(spark);
                }
            }

            // Full expansion burst
            if (timer == ExpandDuration)
            {
                SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.2f, Volume = 0.8f }, Projectile.Center);
                var ring = new BloomRingParticle(Projectile.Center, Vector2.Zero, new Color(180, 80, 255), 0.8f, 18);
                MagnumParticleHandler.SpawnParticle(ring);

                for (int i = 0; i < 10; i++)
                {
                    float angle = MathHelper.TwoPi * i / 10f;
                    Vector2 burstPos = Projectile.Center + angle.ToRotationVector2() * MaxRadius * 0.6f;
                    Color burstColor = Color.Lerp(new Color(140, 50, 200), new Color(255, 200, 255), (float)i / 10f);
                    var glow = new GenericGlowParticle(burstPos, angle.ToRotationVector2() * 1.5f,
                        burstColor, 0.3f, 14, true);
                    MagnumParticleHandler.SpawnParticle(glow);
                }
            }

            // Collapse implosion near end
            if (timer > TotalDuration - 10)
            {
                float collapseProgress = (float)(timer - (TotalDuration - 10)) / 10f;
                // Particles pulled inward
                for (int i = 0; i < 4; i++)
                {
                    float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                    Vector2 startPos = Projectile.Center + angle.ToRotationVector2() * MaxRadius * (1f - collapseProgress * 0.5f);
                    Vector2 vel = (Projectile.Center - startPos).SafeNormalize(Vector2.Zero) * 3f;
                    Dust d = Dust.NewDustPerfect(startPos, DustID.PurpleTorch, vel, 60, default, 1.1f);
                    d.noGravity = true;
                }
            }

            Lighting.AddLight(Projectile.Center, 0.4f, 0.15f, 0.6f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Void debuff / visual feedback
            for (int i = 0; i < 4; i++)
            {
                Dust d = Dust.NewDustPerfect(target.Center, DustID.PurpleTorch,
                    Main.rand.NextVector2Circular(3f, 3f), 0, default, 1.2f);
                d.noGravity = true;
            }
        }

        public override void OnKill(int timeLeft)
        {
            // Implosion burst
            SoundEngine.PlaySound(SoundID.Item14 with { Pitch = 0.6f, Volume = 0.6f }, Projectile.Center);
            for (int i = 0; i < 6; i++)
            {
                Vector2 sparkVel = Main.rand.NextVector2Circular(6f, 6f);
                var spark = new GlowSparkParticle(Projectile.Center, sparkVel,
                    new Color(200, 100, 255), 0.35f, Main.rand.Next(10, 18));
                MagnumParticleHandler.SpawnParticle(spark);
            }
            for (int i = 0; i < 12; i++)
            {
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.PurpleTorch,
                    Main.rand.NextVector2Circular(8f, 8f), 0, default, 1.4f);
                d.noGravity = true;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Texture2D tex = Terraria.GameContent.TextureAssets.Extra[98].Value;
            Vector2 origin = tex.Size() * 0.5f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float radius = Radius;

            int timer = (int)Projectile.ai[1];
            float fadeOut = timer > TotalDuration - 15 ? (float)(TotalDuration - timer) / 15f : 1f;
            float pulse = 1f + (float)Math.Sin(Main.GameUpdateCount * 0.12f) * 0.08f;
            float riftScale = radius / 60f;

            SwingShaderSystem.BeginAdditive(sb);

            // Outer void haze
            Color outerColor = new Color(60, 15, 90, 0) * 0.25f * fadeOut;
            sb.Draw(tex, drawPos, null, outerColor, Main.GameUpdateCount * 0.02f, origin, riftScale * 1.3f * pulse, SpriteEffects.None, 0f);

            // Inner void energy
            Color innerColor = new Color(140, 50, 200, 0) * 0.4f * fadeOut;
            sb.Draw(tex, drawPos, null, innerColor, -Main.GameUpdateCount * 0.03f, origin, riftScale * 0.9f * pulse, SpriteEffects.None, 0f);

            // Bright core
            Color coreColor = new Color(220, 160, 255, 0) * 0.55f * fadeOut;
            sb.Draw(tex, drawPos, null, coreColor, Main.GameUpdateCount * 0.05f, origin, riftScale * 0.5f * pulse, SpriteEffects.None, 0f);

            SwingShaderSystem.RestoreSpriteBatch(sb);

            return false;
        }
    }
}
