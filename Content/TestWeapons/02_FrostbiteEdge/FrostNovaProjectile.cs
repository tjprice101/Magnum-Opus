using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.VFX;

namespace MagnumOpus.Content.TestWeapons._02_FrostbiteEdge
{
    /// <summary>
    /// ❄️ Frost Nova Projectile — expanding ring of ice that damages enemies in its radius.
    /// Spawned by Step 3 (Glacial Cataclysm). Expands outward, then lingers before fading.
    /// </summary>
    public class FrostNovaProjectile : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.FrostBlastFriendly;

        private const float MaxRadius = 140f;
        private const int ExpandDuration = 20;
        private const int LingerDuration = 40;

        private float Radius
        {
            get
            {
                int timer = TotalDuration - Projectile.timeLeft;
                if (timer < ExpandDuration)
                {
                    float expandProgress = (float)timer / ExpandDuration;
                    float eased = 1f - (1f - expandProgress) * (1f - expandProgress);
                    return MaxRadius * eased;
                }
                return MaxRadius;
            }
        }

        private int TotalDuration => ExpandDuration + LingerDuration;
        private float LifeProgress => 1f - (float)Projectile.timeLeft / TotalDuration;

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.MeleeNoSpeed;
            Projectile.penetrate = -1;
            Projectile.timeLeft = ExpandDuration + LingerDuration;
            Projectile.tileCollide = false;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 15;
            Projectile.alpha = 255;
        }

        public override bool ShouldUpdatePosition() => false;

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float currentRadius = Radius;
            Vector2 closestPoint = new Vector2(
                MathHelper.Clamp(Projectile.Center.X, targetHitbox.Left, targetHitbox.Right),
                MathHelper.Clamp(Projectile.Center.Y, targetHitbox.Top, targetHitbox.Bottom));
            return Vector2.Distance(Projectile.Center, closestPoint) <= currentRadius;
        }

        public override void AI()
        {
            float currentRadius = Radius;
            int timer = TotalDuration - Projectile.timeLeft;
            float fadeOut = Projectile.timeLeft < LingerDuration * 0.5f
                ? Projectile.timeLeft / (LingerDuration * 0.5f) : 1f;

            // Ring of ice dust at the expanding edge
            if (timer < ExpandDuration + 10)
            {
                int dustCount = (int)(8 + currentRadius * 0.15f);
                for (int i = 0; i < dustCount; i++)
                {
                    float angle = MathHelper.TwoPi * i / dustCount + Main.rand.NextFloat(-0.2f, 0.2f);
                    Vector2 edgePos = Projectile.Center + angle.ToRotationVector2() * (currentRadius * Main.rand.NextFloat(0.85f, 1.05f));
                    Vector2 dustVel = angle.ToRotationVector2() * Main.rand.NextFloat(0.3f, 1.5f);

                    Dust d = Dust.NewDustPerfect(edgePos, DustID.IceTorch, dustVel, 0, default, 1.3f * fadeOut);
                    d.noGravity = true;
                }
            }

            // Interior frost mist
            if (Main.rand.NextBool(2))
            {
                Vector2 randomPos = Projectile.Center + Main.rand.NextVector2Circular(currentRadius * 0.7f, currentRadius * 0.7f);
                Dust d = Dust.NewDustPerfect(randomPos, DustID.Frost,
                    Main.rand.NextVector2Circular(0.5f, 0.5f), 120, default, 0.7f * fadeOut);
                d.noGravity = true;
            }

            // Snowflake crystals
            if (timer % 5 == 0 && fadeOut > 0.3f)
            {
                for (int i = 0; i < 3; i++)
                {
                    float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                    float dist = Main.rand.NextFloat(0.3f, 0.9f) * currentRadius;
                    Vector2 sparkPos = Projectile.Center + angle.ToRotationVector2() * dist;
                    Vector2 sparkVel = Main.rand.NextVector2Circular(1.5f, 1.5f) + new Vector2(0, -0.5f);
                    Color sparkColor = Color.Lerp(new Color(130, 210, 255), Color.White, Main.rand.NextFloat(0.6f));
                    var spark = new GlowSparkParticle(sparkPos, sparkVel, sparkColor,
                        Main.rand.NextFloat(0.2f, 0.4f) * fadeOut, Main.rand.Next(10, 20));
                    MagnumParticleHandler.SpawnParticle(spark);
                }
            }

            // Bloom ring at boundary — one-time at full expansion
            if (timer == ExpandDuration)
            {
                var ring = new BloomRingParticle(Projectile.Center, Vector2.Zero,
                    new Color(100, 200, 255), 0.8f, 22);
                MagnumParticleHandler.SpawnParticle(ring);

                // Eight-point ice flare burst
                for (int i = 0; i < 8; i++)
                {
                    float angle = MathHelper.TwoPi * i / 8f;
                    Vector2 burstVel = angle.ToRotationVector2() * 6f;
                    var glow = new GenericGlowParticle(Projectile.Center + angle.ToRotationVector2() * currentRadius * 0.6f,
                        burstVel, new Color(140, 220, 255), 0.35f, 18, true);
                    MagnumParticleHandler.SpawnParticle(glow);
                }
            }

            // Lighting
            float lightIntensity = 0.6f * fadeOut;
            Lighting.AddLight(Projectile.Center, 0.2f * lightIntensity, 0.5f * lightIntensity, 0.9f * lightIntensity);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int remainingDamageCount)
        {
            target.AddBuff(BuffID.Frostburn2, 180);

            for (int i = 0; i < 5; i++)
            {
                Dust d = Dust.NewDustPerfect(target.Center, DustID.IceTorch,
                    Main.rand.NextVector2Circular(4f, 4f), 0, default, 1.2f);
                d.noGravity = true;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Texture2D glow = Terraria.GameContent.TextureAssets.Extra[98].Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 glowOrigin = glow.Size() * 0.5f;

            float currentRadius = Radius;
            float fadeOut = Projectile.timeLeft < LingerDuration * 0.5f
                ? Projectile.timeLeft / (LingerDuration * 0.5f) : 1f;

            // Central glow layers
            float pulse = 1f + (float)Math.Sin(Main.GameUpdateCount * 0.1f) * 0.08f;
            float glowScale = currentRadius / (glow.Width * 0.5f) * pulse;

            Color outerColor = new Color(40, 100, 180, 0) * 0.2f * fadeOut;
            Color innerColor = new Color(100, 200, 255, 0) * 0.35f * fadeOut;
            Color coreColor = new Color(200, 240, 255, 0) * 0.5f * fadeOut;

            SwingShaderSystem.BeginAdditive(sb);
            sb.Draw(glow, drawPos, null, outerColor, 0f, glowOrigin, glowScale * 1.3f, SpriteEffects.None, 0f);
            sb.Draw(glow, drawPos, null, innerColor, 0f, glowOrigin, glowScale * 0.9f, SpriteEffects.None, 0f);
            sb.Draw(glow, drawPos, null, coreColor, 0f, glowOrigin, glowScale * 0.5f, SpriteEffects.None, 0f);
            SwingShaderSystem.RestoreSpriteBatch(sb);

            return false;
        }
    }
}
