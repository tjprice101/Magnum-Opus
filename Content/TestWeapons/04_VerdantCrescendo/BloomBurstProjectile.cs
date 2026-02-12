using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.VFX;

namespace MagnumOpus.Content.TestWeapons._04_VerdantCrescendo
{
    /// <summary>
    /// 🌸 Bloom Burst Projectile — expanding nature AoE explosion spawned by Step 3 (Overgrowth Cataclysm).
    /// A massive floral eruption that expands outward, damages all enemies in range,
    /// then lingers with drifting petals before fading.
    /// </summary>
    public class BloomBurstProjectile : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.FlowerPetal;

        private const float MaxRadius = 160f;
        private const int ExpandDuration = 22;
        private const int LingerDuration = 50;

        // Nature palette — emerald to golden bloom
        private static readonly Color[] NaturePalette = new Color[]
        {
            new Color(30, 100, 40),    // Deep forest
            new Color(60, 180, 70),    // Vibrant green
            new Color(120, 210, 90),   // Fresh leaf
            new Color(200, 230, 80),   // Spring yellow-green
            new Color(240, 210, 100),  // Golden pollen
            new Color(255, 180, 200),  // Blossom pink
        };

        private float Radius
        {
            get
            {
                int timer = TotalDuration - Projectile.timeLeft;
                if (timer < ExpandDuration)
                {
                    float expandProgress = (float)timer / ExpandDuration;
                    // Elastic-style ease — overshoots slightly then settles
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
            Projectile.localNPCHitCooldown = 18;
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

            // === EXPANDING RING OF LEAVES ===
            if (timer < ExpandDuration + 12)
            {
                int dustCount = (int)(10 + currentRadius * 0.18f);
                for (int i = 0; i < dustCount; i++)
                {
                    float angle = MathHelper.TwoPi * i / dustCount + Main.rand.NextFloat(-0.15f, 0.15f);
                    float dist = currentRadius * Main.rand.NextFloat(0.88f, 1.08f);
                    Vector2 edgePos = Projectile.Center + angle.ToRotationVector2() * dist;
                    Vector2 dustVel = angle.ToRotationVector2() * Main.rand.NextFloat(0.4f, 1.8f);

                    Dust d = Dust.NewDustPerfect(edgePos, DustID.JungleGrass, dustVel, 0, default, 1.5f * fadeOut);
                    d.noGravity = true;
                    d.fadeIn = 1.2f;
                }
            }

            // === INTERIOR SPORE MIST ===
            if (Main.rand.NextBool(2))
            {
                Vector2 randomPos = Projectile.Center + Main.rand.NextVector2Circular(currentRadius * 0.65f, currentRadius * 0.65f);
                int dustType = Main.rand.NextBool(3) ? DustID.GemEmerald : DustID.JungleGrass;
                Dust d = Dust.NewDustPerfect(randomPos, dustType,
                    Main.rand.NextVector2Circular(0.6f, 0.6f) + new Vector2(0, -0.4f),
                    100, default, 0.8f * fadeOut);
                d.noGravity = true;
            }

            // === GLOWING PETAL SPARKLES ===
            if (timer % 4 == 0 && fadeOut > 0.3f)
            {
                for (int i = 0; i < 4; i++)
                {
                    float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                    float dist = Main.rand.NextFloat(0.2f, 0.85f) * currentRadius;
                    Vector2 sparkPos = Projectile.Center + angle.ToRotationVector2() * dist;
                    Vector2 sparkVel = Main.rand.NextVector2Circular(1.2f, 1.2f) + new Vector2(0, -0.8f);

                    float colorProgress = Main.rand.NextFloat();
                    Color sparkColor = GetPaletteColor(colorProgress);
                    var spark = new GlowSparkParticle(sparkPos, sparkVel, sparkColor,
                        Main.rand.NextFloat(0.2f, 0.45f) * fadeOut, Main.rand.Next(12, 22));
                    MagnumParticleHandler.SpawnParticle(spark);
                }
            }

            // === RISING GLOW PARTICLES ===
            if (Main.rand.NextBool(3))
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                float dist = Main.rand.NextFloat(0.3f, 0.7f) * currentRadius;
                Vector2 pos = Projectile.Center + angle.ToRotationVector2() * dist;
                Vector2 vel = new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), Main.rand.NextFloat(-2f, -0.5f));
                Color glowCol = GetPaletteColor(Main.rand.NextFloat()) * 0.6f;
                var glow = new GenericGlowParticle(pos, vel, glowCol,
                    Main.rand.NextFloat(0.15f, 0.3f) * fadeOut, Main.rand.Next(14, 24), true);
                MagnumParticleHandler.SpawnParticle(glow);
            }

            // === BLOOM RING AT FULL EXPANSION ===
            if (timer == ExpandDuration)
            {
                var ring = new BloomRingParticle(Projectile.Center, Vector2.Zero,
                    new Color(120, 220, 80), 1.0f, 25);
                MagnumParticleHandler.SpawnParticle(ring);

                // Eight-point floral burst
                for (int i = 0; i < 8; i++)
                {
                    float angle = MathHelper.TwoPi * i / 8f;
                    Vector2 burstVel = angle.ToRotationVector2() * 5f;
                    Color burstColor = GetPaletteColor((float)i / 8f);
                    var burstGlow = new GenericGlowParticle(
                        Projectile.Center + angle.ToRotationVector2() * currentRadius * 0.5f,
                        burstVel, burstColor, 0.4f, 20, true);
                    MagnumParticleHandler.SpawnParticle(burstGlow);
                }

                // Big emerald dust ring
                for (int i = 0; i < 12; i++)
                {
                    float angle = MathHelper.TwoPi * i / 12f;
                    Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 7f);
                    Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.GemEmerald, vel, 0, default, 1.6f);
                    d.noGravity = true;
                }
            }

            // Lighting — warm green
            float lightIntensity = 0.7f * fadeOut;
            Lighting.AddLight(Projectile.Center, 0.25f * lightIntensity, 0.65f * lightIntensity, 0.15f * lightIntensity);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int remainingDamageCount)
        {
            target.AddBuff(BuffID.Poisoned, 240);

            for (int i = 0; i < 6; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(4f, 4f);
                Dust d = Dust.NewDustPerfect(target.Center, DustID.JungleGrass, vel, 0, default, 1.3f);
                d.noGravity = true;
            }

            var glow = new GenericGlowParticle(target.Center, Main.rand.NextVector2Circular(2f, 2f),
                new Color(100, 220, 80) * 0.6f, 0.3f, 14, true);
            MagnumParticleHandler.SpawnParticle(glow);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Texture2D glow = TextureAssets.Extra[98].Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 glowOrigin = glow.Size() * 0.5f;

            float currentRadius = Radius;
            float fadeOut = Projectile.timeLeft < LingerDuration * 0.5f
                ? Projectile.timeLeft / (LingerDuration * 0.5f) : 1f;

            float pulse = 1f + (float)Math.Sin(Main.GameUpdateCount * 0.12f) * 0.1f;
            float glowScale = currentRadius / (glow.Width * 0.5f) * pulse;

            // Layered nature glow
            Color outerColor = new Color(40, 120, 50, 0) * 0.2f * fadeOut;
            Color midColor = new Color(80, 200, 80, 0) * 0.3f * fadeOut;
            Color innerColor = new Color(160, 240, 100, 0) * 0.4f * fadeOut;
            Color coreColor = new Color(240, 255, 180, 0) * 0.5f * fadeOut;

            SwingShaderSystem.BeginAdditive(sb);

            // Outer forest glow
            sb.Draw(glow, drawPos, null, outerColor, 0f, glowOrigin, glowScale * 1.4f, SpriteEffects.None, 0f);
            // Mid leaf glow
            sb.Draw(glow, drawPos, null, midColor, 0f, glowOrigin, glowScale * 1.0f, SpriteEffects.None, 0f);
            // Inner bloom
            sb.Draw(glow, drawPos, null, innerColor, 0f, glowOrigin, glowScale * 0.65f, SpriteEffects.None, 0f);
            // Bright core
            sb.Draw(glow, drawPos, null, coreColor, 0f, glowOrigin, glowScale * 0.3f, SpriteEffects.None, 0f);

            SwingShaderSystem.RestoreSpriteBatch(sb);

            return false;
        }

        private Color GetPaletteColor(float progress)
        {
            float scaled = progress * (NaturePalette.Length - 1);
            int idx = (int)scaled;
            int next = Math.Min(idx + 1, NaturePalette.Length - 1);
            return Color.Lerp(NaturePalette[idx], NaturePalette[next], scaled - idx);
        }
    }
}
