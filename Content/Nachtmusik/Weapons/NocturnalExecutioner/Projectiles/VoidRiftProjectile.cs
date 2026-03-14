using System;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.VFX.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Nachtmusik.Weapons.NocturnalExecutioner.Projectiles
{
    /// <summary>
    /// Lingering Void Rift — arc-shaped damaging zone spawned by Phase 3 (Stellar Execution).
    /// Deals 40% weapon damage per tick (3 ticks over 1.5s).
    /// Void particles spiral inward constantly. Shrinks over lifetime then implodes.
    /// Gold flash on final collapse.
    /// Only 1 rift active at a time.
    /// ai[0] = swing rotation angle for arc orientation.
    /// </summary>
    public class VoidRiftProjectile : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_0";

        private const int TotalLifetime = 90; // 1.5s at 60fps
        private const int TickInterval = 30;  // damage every 0.5s = 3 ticks total
        private const float RiftRadius = 80f;
        private const float RiftArcWidth = 50f;

        private float SwingRotation => Projectile.ai[0];

        public override void SetDefaults()
        {
            Projectile.width = 120;
            Projectile.height = 120;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.MeleeNoSpeed;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = TotalLifetime;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = TickInterval;
            Projectile.hide = false;
        }

        public override void AI()
        {
            // Kill older rifts from same player
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile p = Main.projectile[i];
                if (p.active && p.type == Type && p.owner == Projectile.owner && p.whoAmI != Projectile.whoAmI)
                {
                    if (p.timeLeft > Projectile.timeLeft)
                        Projectile.Kill();
                    else
                        p.Kill();
                }
            }

            float progress = 1f - (Projectile.timeLeft / (float)TotalLifetime); // 0→1

            // Gravitational particle pull — dust spirals inward to rift center
            if (!Main.dedServ)
            {
                SpawnRiftDust(progress);
            }

            // Ambient lighting
            float lightIntensity = 0.4f * (1f - progress);
            Lighting.AddLight(Projectile.Center,
                NachtmusikPalette.CosmicPurple.ToVector3() * lightIntensity);
        }

        private void SpawnRiftDust(float progress)
        {
            int dustCount = Math.Max(1, (int)(3 * (1f - progress)));
            float currentRadius = RiftRadius * (1f - progress * 0.5f); // Shrinks over time

            for (int i = 0; i < dustCount; i++)
            {
                // Spawn at edge of rift, pulled inward
                float angle = SwingRotation + Main.rand.NextFloat(-0.8f, 0.8f);
                float dist = currentRadius + Main.rand.NextFloat(-10f, 20f);
                Vector2 spawnPos = Projectile.Center + angle.ToRotationVector2() * dist;

                // Velocity spiraling inward
                Vector2 toCenter = (Projectile.Center - spawnPos).SafeNormalize(Vector2.Zero);
                Vector2 tangent = toCenter.RotatedBy(MathHelper.PiOver4) * 0.5f;
                Vector2 vel = (toCenter + tangent) * Main.rand.NextFloat(1.5f, 3.5f);

                Color c = Color.Lerp(NachtmusikPalette.CosmicVoid, NachtmusikPalette.Violet,
                    Main.rand.NextFloat(0.2f, 0.7f));
                Dust d = Dust.NewDustPerfect(spawnPos, DustID.PurpleTorch, vel, 0, c,
                    0.6f + (1f - progress) * 0.3f);
                d.noGravity = true;
                d.fadeIn = 1f;
            }

            // Gold edge sparkles (sparse)
            if (Main.rand.NextBool(4))
            {
                float sparkAngle = SwingRotation + Main.rand.NextFloat(-0.6f, 0.6f);
                Vector2 sparkPos = Projectile.Center + sparkAngle.ToRotationVector2() * currentRadius;
                Dust g = Dust.NewDustPerfect(sparkPos, DustID.GoldFlame,
                    Main.rand.NextVector2Circular(0.5f, 0.5f), 0, NachtmusikPalette.StarGold, 0.4f);
                g.noGravity = true;
            }
        }

        public override void OnKill(int timeLeft)
        {
            if (Main.dedServ) return;

            // Final implosion collapse + gold flash
            int burstCount = 12;
            for (int i = 0; i < burstCount; i++)
            {
                float angle = MathHelper.TwoPi * i / burstCount;

                // Implosion dust (converges)
                Vector2 spawnPos = Projectile.Center + angle.ToRotationVector2() * 30f;
                Vector2 inVel = (Projectile.Center - spawnPos).SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(4f, 7f);
                Dust d = Dust.NewDustPerfect(spawnPos, DustID.PurpleTorch, inVel, 0,
                    NachtmusikPalette.CosmicPurple, 1.0f);
                d.noGravity = true;
            }

            // Gold flash at center
            for (int i = 0; i < 6; i++)
            {
                Dust g = Dust.NewDustPerfect(Projectile.Center, DustID.GoldFlame,
                    Main.rand.NextVector2Circular(4f, 4f), 0,
                    NachtmusikPalette.StarGold, 1.0f);
                g.noGravity = true;
            }

            // Bloom ring on collapse
            var ring = new BloomRingParticle(Projectile.Center, Vector2.Zero,
                NachtmusikPalette.StarGold, 0.4f, 15);
            MagnumParticleHandler.SpawnParticle(ring);

            Lighting.AddLight(Projectile.Center, NachtmusikPalette.StarGold.ToVector3() * 0.6f);
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            // Arc-shaped hitbox centered on the rift
            float progress = 1f - (Projectile.timeLeft / (float)TotalLifetime);
            float currentRadius = RiftRadius * (1f - progress * 0.5f);

            Vector2 targetCenter = targetHitbox.Center.ToVector2();
            float distToCenter = Vector2.Distance(Projectile.Center, targetCenter);

            // Within arc radius
            if (distToCenter > currentRadius + RiftArcWidth * 0.5f) return false;
            if (distToCenter < currentRadius - RiftArcWidth * 0.5f && distToCenter > 30f) return false;

            // Within arc angle
            float angleToTarget = (targetCenter - Projectile.Center).ToRotation();
            float angleDiff = MathHelper.WrapAngle(angleToTarget - SwingRotation);
            return Math.Abs(angleDiff) < 0.8f; // ~46 degree arc on each side
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Texture2D glowTex = MagnumTextureRegistry.GetSoftGlow();
            Texture2D bloomTex = MagnumTextureRegistry.GetRadialBloom();
            if (glowTex == null) return false;

            float progress = 1f - (Projectile.timeLeft / (float)TotalLifetime);
            float currentRadius = RiftRadius * (1f - progress * 0.5f);
            float alpha = 1f - progress; // Fades as it dies
            float time = (float)Main.timeForVisualEffects;
            Vector2 center = Projectile.Center - Main.screenPosition;

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            // Draw arc segments as series of overlapping glow sprites
            int segments = 12;
            Vector2 glowOrigin = glowTex.Size() * 0.5f;

            for (int i = 0; i < segments; i++)
            {
                float arcT = -0.7f + 1.4f * i / (segments - 1);
                float segAngle = SwingRotation + arcT;
                Vector2 segPos = center + segAngle.ToRotationVector2() * currentRadius;

                // Pulsing distortion
                float segPulse = 1f + MathF.Sin(time * 3f + i * 0.5f) * 0.15f * alpha;

                // Void core (dark purple)
                Color voidColor = NachtmusikPalette.Additive(NachtmusikPalette.CosmicPurple, 0.4f * alpha * segPulse);
                sb.Draw(glowTex, segPos, null, voidColor, segAngle, glowOrigin,
                    new Vector2(0.12f, 0.06f) * segPulse, SpriteEffects.None, 0f);

                // Violet edge
                Color violetEdge = NachtmusikPalette.Additive(NachtmusikPalette.Violet, 0.25f * alpha * segPulse);
                sb.Draw(glowTex, segPos, null, violetEdge, segAngle, glowOrigin,
                    new Vector2(0.08f, 0.04f) * segPulse, SpriteEffects.None, 0f);

                // Gold accent on edges of arc
                if (i <= 1 || i >= segments - 2)
                {
                    Color goldAccent = NachtmusikPalette.Additive(NachtmusikPalette.StarGold, 0.2f * alpha);
                    sb.Draw(glowTex, segPos, null, goldAccent, segAngle, glowOrigin,
                        0.05f * segPulse, SpriteEffects.None, 0f);
                }
            }

            // Central void bloom (the gravitational center)
            if (bloomTex != null)
            {
                Vector2 bloomOrigin = bloomTex.Size() * 0.5f;
                float centerPulse = 1f + MathF.Sin(time * 4f) * 0.1f * alpha;

                Color centerVoid = NachtmusikPalette.Additive(NachtmusikPalette.CosmicVoid, 0.3f * alpha);
                sb.Draw(bloomTex, center, null, centerVoid, 0f, bloomOrigin,
                    0.08f * centerPulse, SpriteEffects.None, 0f);

                Color centerPurple = NachtmusikPalette.Additive(NachtmusikPalette.CosmicPurple, 0.2f * alpha);
                sb.Draw(bloomTex, center, null, centerPurple, 0f, bloomOrigin,
                    0.05f * centerPulse, SpriteEffects.None, 0f);
            }

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

            return false;
        }
    }
}
