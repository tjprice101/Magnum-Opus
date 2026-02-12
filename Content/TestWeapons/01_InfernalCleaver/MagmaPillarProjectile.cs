using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Content.TestWeapons._01_InfernalCleaver
{
    /// <summary>
    /// Magma Pillar — spawned by the Infernal Cleaver's Step 3 (Massive Slam).
    /// Erupts upward from the ground, dealing area damage in a vertical column.
    /// Lingers briefly with fire particles then dissipates.
    /// </summary>
    public class MagmaPillarProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Content/TestWeapons/01_InfernalCleaver/MagmaEruptionColumn";

        private const int LingerTime = 45;
        private const float PillarHeight = 120f;

        private int LifeTimer
        {
            get => (int)Projectile.ai[0];
            set => Projectile.ai[0] = value;
        }

        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = (int)PillarHeight;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.MeleeNoSpeed;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = LingerTime;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 20;
            Projectile.alpha = 255; // Invisible sprite — drawn via PreDraw
        }

        public override bool ShouldUpdatePosition() => false;

        public override void AI()
        {
            LifeTimer++;
            float progress = (float)LifeTimer / LingerTime;

            // Rise phase (first 30%)
            float riseProgress = Math.Min(1f, progress / 0.3f);

            // Erupt particles — dense fire column
            int particleCount = progress < 0.5f ? 4 : 2;
            for (int i = 0; i < particleCount; i++)
            {
                float yOffset = Main.rand.NextFloat(PillarHeight * riseProgress);
                Vector2 dustPos = Projectile.Center + new Vector2(
                    Main.rand.NextFloat(-12f, 12f),
                    -yOffset);

                Dust d = Dust.NewDustPerfect(dustPos, DustID.Torch,
                    new Vector2(Main.rand.NextFloat(-1f, 1f), -Main.rand.NextFloat(2f, 5f)),
                    0, default, 1.5f + (1f - progress) * 0.8f);
                d.noGravity = true;
                d.fadeIn = 1.2f;
            }

            // Smoke particles
            if (Main.rand.NextBool(2))
            {
                Dust smoke = Dust.NewDustPerfect(
                    Projectile.Center + new Vector2(Main.rand.NextFloat(-15f, 15f), -PillarHeight * riseProgress),
                    DustID.Smoke,
                    new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), -Main.rand.NextFloat(1f, 3f)),
                    100, default, 1.2f);
                smoke.noGravity = false;
            }

            // Ember sparks flying outward
            if (LifeTimer < LingerTime / 2 && Main.rand.NextBool(3))
            {
                var spark = new GlowSparkParticle(
                    Projectile.Center + new Vector2(0, -Main.rand.NextFloat(PillarHeight * 0.5f)),
                    new Vector2(Main.rand.NextFloat(-4f, 4f), -Main.rand.NextFloat(3f, 7f)),
                    new Color(255, Main.rand.Next(80, 180), 20),
                    Main.rand.NextFloat(0.3f, 0.6f),
                    Main.rand.Next(10, 20));
                MagnumParticleHandler.SpawnParticle(spark);
            }

            // Dynamic lighting
            float lightIntensity = (1f - progress) * 1.2f;
            Lighting.AddLight(Projectile.Center, lightIntensity, lightIntensity * 0.4f, lightIntensity * 0.05f);
            Lighting.AddLight(Projectile.Center - new Vector2(0, PillarHeight * 0.5f),
                lightIntensity * 0.8f, lightIntensity * 0.3f, 0f);
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float riseProgress = Math.Min(1f, (float)LifeTimer / (LingerTime * 0.3f));
            float currentHeight = PillarHeight * riseProgress;

            // Vertical column hitbox
            Rectangle pillarBox = new Rectangle(
                (int)(Projectile.Center.X - 20),
                (int)(Projectile.Center.Y - currentHeight),
                40,
                (int)currentHeight);

            return pillarBox.Intersects(targetHitbox);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int remainingDamageCount)
        {
            for (int i = 0; i < 6; i++)
            {
                Dust d = Dust.NewDustPerfect(target.Center, DustID.Torch,
                    Main.rand.NextVector2Circular(5f, 5f), 0, default, 1.4f);
                d.noGravity = true;
            }
        }

        public override void OnKill(int timeLeft)
        {
            // Final smoke puff
            for (int i = 0; i < 8; i++)
            {
                Dust d = Dust.NewDustPerfect(
                    Projectile.Center + new Vector2(Main.rand.NextFloat(-15f, 15f), -Main.rand.NextFloat(PillarHeight)),
                    DustID.Smoke,
                    new Vector2(Main.rand.NextFloat(-2f, 2f), -Main.rand.NextFloat(1f, 3f)),
                    100, default, 1.5f);
                d.noGravity = false;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            float progress = (float)LifeTimer / LingerTime;
            float riseProgress = Math.Min(1f, progress / 0.3f);
            float fadeOut = progress > 0.6f ? 1f - (progress - 0.6f) / 0.4f : 1f;

            // Draw the custom magma eruption column texture
            Texture2D pillarTex = ModContent.Request<Texture2D>(Texture).Value;
            Vector2 pillarOrigin = new Vector2(pillarTex.Width * 0.5f, pillarTex.Height); // Bottom-center origin
            Vector2 basePos = Projectile.Center - Main.screenPosition;

            // Scale pillar height to match rise animation
            float pillarScaleY = (PillarHeight / (float)pillarTex.Height) * riseProgress;
            float pillarScaleX = 0.8f + (float)Math.Sin(Main.GameUpdateCount * 0.1f) * 0.05f; // Slight width pulse

            // Normal pass — the visible pillar
            Color pillarLight = Lighting.GetColor(Projectile.Center.ToTileCoordinates());
            sb.Draw(pillarTex, basePos, null, pillarLight * fadeOut, 0f, pillarOrigin, new Vector2(pillarScaleX, pillarScaleY), SpriteEffects.None, 0f);

            // Additive glow pass — fiery backlight
            Color pillarGlow = new Color(255, 100, 20, 0) * 0.4f * fadeOut;
            sb.Draw(pillarTex, basePos, null, pillarGlow, 0f, pillarOrigin, new Vector2(pillarScaleX * 1.1f, pillarScaleY * 1.05f), SpriteEffects.None, 0f);

            // Additional glow orb layers on top for heat shimmer
            Texture2D glowTex = Terraria.GameContent.TextureAssets.Extra[98].Value;
            Vector2 glowOrigin = glowTex.Size() * 0.5f;

            int segments = 6;
            float currentHeight = PillarHeight * riseProgress;

            for (int s = 0; s < segments; s++)
            {
                float segProgress = (float)s / segments;
                float yOff = currentHeight * segProgress;
                Vector2 segPos = basePos + new Vector2(0, -yOff);

                float segFade = (1f - segProgress * 0.4f) * fadeOut;
                float segScale = (0.15f + (1f - segProgress) * 0.08f);

                // Fire wobble
                float wobble = (float)Math.Sin(Main.GameUpdateCount * 0.15f + s * 1.3f) * 3f;
                segPos.X += wobble;

                // Outer glow
                Color outerColor = Color.Lerp(new Color(200, 50, 10), new Color(255, 120, 20), segProgress);
                outerColor.A = 0;
                sb.Draw(glowTex, segPos, null, outerColor * 0.3f * segFade, 0f, glowOrigin, segScale * 1.5f, SpriteEffects.None, 0f);

                // Inner glow
                Color innerColor = Color.Lerp(new Color(255, 180, 40), new Color(255, 220, 100), segProgress);
                innerColor.A = 0;
                sb.Draw(glowTex, segPos, null, innerColor * 0.4f * segFade, 0f, glowOrigin, segScale, SpriteEffects.None, 0f);
            }

            return false;
        }
    }
}
