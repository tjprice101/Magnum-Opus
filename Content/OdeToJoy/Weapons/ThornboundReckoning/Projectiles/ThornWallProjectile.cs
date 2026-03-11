using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.OdeToJoy;
using MagnumOpus.Content.OdeToJoy.Weapons.ThornboundReckoning.Dusts;

namespace MagnumOpus.Content.OdeToJoy.Weapons.ThornboundReckoning.Projectiles
{
    /// <summary>
    /// Thorn Wall — Phase 3 persistent zone projectile.
    /// Uses RadialNoiseMaskShader with FBM noise for organic thorn growth.
    /// Draws a garden-themed damage zone that persists and catches enemies.
    /// </summary>
    public class ThornWallProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Textures/InvisibleProjectile";

        private const int Duration = 180; // 3 seconds
        private const int FadeInFrames = 20;
        private const int FadeOutFrames = 30;
        private const float MaxRadius = 80f;

        private int timer;
        private float currentRadius;
        private float pulsePhase;

        public override void SetDefaults()
        {
            Projectile.width = 160;
            Projectile.height = 160;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.timeLeft = Duration;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 20;

            // ai[0] = empowered flag (1 = empowered = double width from full Reckoning charge)
        }

        public override void AI()
        {
            timer++;
            bool empowered = Projectile.ai[0] >= 1f;

            // Grow radius
            float targetRadius = empowered ? MaxRadius * 2f : MaxRadius;
            float growRate = 0.08f;
            currentRadius = MathHelper.Lerp(currentRadius, targetRadius,
                Math.Min(timer * growRate, 1f));

            // Update hitbox to match current radius
            int hitboxSize = (int)(currentRadius * 2);
            Projectile.width = hitboxSize;
            Projectile.height = hitboxSize;

            // Don't move
            Projectile.velocity = Vector2.Zero;

            // Pulse
            pulsePhase += 0.05f;

            // Lighting
            float alpha = GetAlpha();
            float lightStrength = empowered ? 0.6f : 0.35f;
            Lighting.AddLight(Projectile.Center,
                ThornboundTextures.BloomGold.ToVector3() * lightStrength * alpha);

            // Ambient botanical particles — VineSapDust rising from the zone
            if (timer % 4 == 0 && alpha > 0.3f)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                float dist = Main.rand.NextFloat(0.3f, 1f) * currentRadius;
                Vector2 spawnPos = Projectile.Center + angle.ToRotationVector2() * dist;
                Vector2 vel = new Vector2(0, -Main.rand.NextFloat(0.3f, 1.2f));

                Color col = Color.Lerp(ThornboundTextures.BloomGold, ThornboundTextures.PetalPink,
                    Main.rand.NextFloat());
                Dust.NewDustPerfect(spawnPos, ModContent.DustType<VineSapDust>(), vel,
                    newColor: col, Scale: Main.rand.NextFloat(0.8f, 1.4f));
            }

            // Empowered: ThornburstDust chips radiating outward
            if (empowered && timer % 6 == 0 && alpha > 0.5f)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 pos = Projectile.Center + angle.ToRotationVector2() * currentRadius * 0.8f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(0.5f, 2f);

                Dust.NewDustPerfect(pos, ModContent.DustType<ThornburstDust>(), vel,
                    newColor: ThornboundTextures.JubilantLight,
                    Scale: Main.rand.NextFloat(1.0f, 1.8f));
            }

            // Harmonic pulse on initial zone creation
            if (timer == 1)
                OdeToJoyVFXLibrary.HarmonicPulseRing(Projectile.Center, currentRadius, 12, OdeToJoyPalette.GoldenPollen, 2f);
        }

        private float GetAlpha()
        {
            float fadeIn = MathHelper.Clamp(timer / (float)FadeInFrames, 0f, 1f);
            float fadeOut = Projectile.timeLeft < FadeOutFrames
                ? Projectile.timeLeft / (float)FadeOutFrames
                : 1f;
            return fadeIn * fadeOut;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float radiusSq = currentRadius * currentRadius;
            Vector2 closest = Vector2.Clamp(Projectile.Center,
                targetHitbox.TopLeft(), targetHitbox.BottomRight());
            return Vector2.DistanceSquared(Projectile.Center, closest) < radiusSq;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            try
            {
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float alpha = GetAlpha();
            bool empowered = Projectile.ai[0] >= 1f;
            float time = (float)Main.timeForVisualEffects * 0.015f;

            sb.End();

            // ── LAYER 0: CelebrationAura FloralSigil shader — thorn zone field ──
            Effect auraShader = OdeToJoyShaders.CelebrationAura;
            if (auraShader != null)
            {
                float ringScale = currentRadius * 2f / Math.Max(ThornboundTextures.OJPowerEffectRing.Value.Width, ThornboundTextures.OJPowerEffectRing.Value.Height);
                OdeToJoyShaders.SetAuraParams(auraShader, time, ThornboundTextures.BloomGold,
                    ThornboundTextures.RadiantAmber, alpha * 0.5f, 1.5f, ringScale * 0.3f, 4f);
                string technique = empowered ? "FloralSigilTechnique" : "CelebrationAuraTechnique";
                OdeToJoyShaders.BeginDeferredShaderBatch(sb, auraShader, technique);
                Texture2D ringTex = ThornboundTextures.OJPowerEffectRing.Value;
                Vector2 ringOrigin = ringTex.Size() / 2f;
                float pulse = 0.85f + 0.15f * (float)Math.Sin(pulsePhase);
                sb.Draw(ringTex, drawPos, null, Color.White * alpha * pulse,
                    pulsePhase * 0.1f, ringOrigin, ringScale, SpriteEffects.None, 0f);
                sb.End();
            }

            // ── LAYER 1: Additive overlays ──
            OdeToJoyShaders.BeginAdditiveBatch(sb);

            float pulse2 = 0.85f + 0.15f * (float)Math.Sin(pulsePhase);

            // Harmonic resonance wave overlay
            Texture2D waveTex = ThornboundTextures.OJHarmonicWaveImpact.Value;
            Vector2 waveOrigin = waveTex.Size() / 2f;
            float waveScale = currentRadius * 1.6f / Math.Max(waveTex.Width, waveTex.Height);
            sb.Draw(waveTex, drawPos, null,
                ThornboundTextures.PetalPink * alpha * 0.35f,
                -pulsePhase * 0.15f, waveOrigin, waveScale * pulse2,
                SpriteEffects.None, 0f);

            // Soft outer glow
            Texture2D softGlow = ThornboundTextures.SoftGlow.Value;
            Vector2 glowOrigin = softGlow.Size() / 2f;
            float glowScale = Math.Min(currentRadius * 2.5f / Math.Max(softGlow.Width, softGlow.Height), 0.293f);
            Color outerGlow = empowered ? ThornboundTextures.JubilantLight : ThornboundTextures.BloomGold;
            sb.Draw(softGlow, drawPos, null,
                outerGlow * alpha * 0.2f, 0f, glowOrigin,
                glowScale, SpriteEffects.None, 0f);

            // Inner core glow
            float coreScale = glowScale * 0.35f;
            sb.Draw(softGlow, drawPos, null,
                ThornboundTextures.RadiantAmber * alpha * 0.4f, 0f, glowOrigin,
                coreScale, SpriteEffects.None, 0f);

            // Empowered floral overlay
            if (empowered)
            {
                Texture2D floralTex = ThornboundTextures.OJFloralImpact.Value;
                Vector2 floralOrigin = floralTex.Size() / 2f;
                float floralScale = currentRadius * 2f / Math.Max(floralTex.Width, floralTex.Height);
                sb.Draw(floralTex, drawPos, null,
                    ThornboundTextures.JubilantLight * alpha * 0.3f * pulse2,
                    pulsePhase * 0.05f, floralOrigin, floralScale,
                    SpriteEffects.None, 0f);
            }

            // Theme blossom sparkle accent
            OdeToJoyVFXLibrary.DrawThemeBlossomSparkle(sb, Projectile.Center, 1f, 0.5f);

            sb.End();
            OdeToJoyShaders.RestoreSpriteBatch(sb);
            }
            catch { }
            finally
            {
                try { sb.End(); } catch { }
                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                    DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            }

            return false;
        }

        public override void OnKill(int timeLeft)
        {
            // Death burst — radial ThornburstDust + VineSapDust
            for (int i = 0; i < 12; i++)
            {
                float angle = MathHelper.TwoPi / 12f * i;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 5f);
                Color col = Color.Lerp(ThornboundTextures.BloomGold,
                    ThornboundTextures.PetalPink, Main.rand.NextFloat());
                int dustType = i % 2 == 0 ? ModContent.DustType<ThornburstDust>() : ModContent.DustType<VineSapDust>();
                Dust.NewDustPerfect(Projectile.Center, dustType, vel,
                    newColor: col, Scale: Main.rand.NextFloat(1.0f, 1.8f));
            }

            // Garden sparkle explosion on zone collapse
            OdeToJoyVFXLibrary.SpawnGardenSparkleExplosion(Projectile.Center, 8, 5f, 0.25f);
        }
    }
}
