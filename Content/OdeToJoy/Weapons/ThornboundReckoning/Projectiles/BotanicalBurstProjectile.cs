using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.OdeToJoy.Weapons.ThornboundReckoning.Buffs;
using MagnumOpus.Content.OdeToJoy;

namespace MagnumOpus.Content.OdeToJoy.Weapons.ThornboundReckoning.Projectiles
{
    /// <summary>
    /// Botanical Burst — full Reckoning charge explosion.
    /// RadialScatter pattern: 55 sparks of Petal Pink, Bloom Gold, Radiant Amber, Jubilant Light.
    /// 8-tile radius AoE with multi-layered bloom + impact rendering.
    /// </summary>
    public class BotanicalBurstProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Textures/InvisibleProjectile";

        private const int MaxLifetime = 35;
        private const int SparkCount = 55;
        private int timer;
        private float maxExpandRadius;
        private float expandRadius;
        private bool hasExploded;

        public override void SetDefaults()
        {
            Projectile.width = 256; // 8 tiles * 16px * 2
            Projectile.height = 256;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.timeLeft = MaxLifetime;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = MaxLifetime;
        }

        public override void AI()
        {
            timer++;

            // Don't move
            Projectile.velocity = Vector2.Zero;

            // Expand and then contract
            float progress = timer / (float)MaxLifetime;
            if (progress < 0.3f)
            {
                float expandProg = progress / 0.3f;
                expandRadius = MathHelper.SmoothStep(0, 128f, expandProg);
            }
            else
            {
                expandRadius = 128f;
            }

            maxExpandRadius = Math.Max(maxExpandRadius, expandRadius);

            // Fire radial spark burst on first frame
            if (!hasExploded)
            {
                hasExploded = true;
                SpawnRadialSparks();

                // Sound effect
                SoundEngine.PlaySound(SoundID.Item14 with { Pitch = 0.3f, Volume = 0.7f },
                    Projectile.Center);
            }

            // Lighting
            float alpha = GetAlpha();
            Lighting.AddLight(Projectile.Center,
                ThornboundTextures.JubilantLight.ToVector3() * 0.8f * alpha);
        }

        private void SpawnRadialSparks()
        {
            Color[] sparkColors = new[]
            {
                ThornboundTextures.PetalPink,
                ThornboundTextures.BloomGold,
                ThornboundTextures.RadiantAmber,
                ThornboundTextures.JubilantLight
            };

            for (int i = 0; i < SparkCount; i++)
            {
                float angle = MathHelper.TwoPi / SparkCount * i
                    + Main.rand.NextFloat(-0.1f, 0.1f);
                float speed = Main.rand.NextFloat(3f, 8f);
                Vector2 vel = angle.ToRotationVector2() * speed;

                Color col = sparkColors[i % sparkColors.Length];
                col = Color.Lerp(col, Color.White, Main.rand.NextFloat(0.1f));

                Dust dust = Dust.NewDustPerfect(
                    Projectile.Center + Main.rand.NextVector2Circular(8f, 8f),
                    DustID.RainbowMk2, vel,
                    newColor: col,
                    Scale: Main.rand.NextFloat(0.5f, 1.1f));
                dust.noGravity = true;
                dust.fadeIn = 0.5f;
            }

            // Extra large botanical particles
            for (int i = 0; i < 16; i++)
            {
                float angle = MathHelper.TwoPi / 16f * i;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(1f, 3f);
                vel.Y -= Main.rand.NextFloat(0.5f, 1.5f); // upward drift

                Color col = Color.Lerp(ThornboundTextures.BloomGold,
                    ThornboundTextures.PetalPink, Main.rand.NextFloat());

                Dust dust = Dust.NewDustPerfect(
                    Projectile.Center + angle.ToRotationVector2() * Main.rand.NextFloat(20f),
                    DustID.RainbowMk2, vel,
                    newColor: col,
                    Scale: Main.rand.NextFloat(0.8f, 1.4f));
                dust.noGravity = true;
                dust.fadeIn = 0.8f;
            }
        }

        private float GetAlpha()
        {
            float fadeIn = MathHelper.Clamp(timer / 5f, 0f, 1f);
            float fadeOut = Projectile.timeLeft < 15
                ? Projectile.timeLeft / 15f
                : 1f;
            return fadeIn * fadeOut;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float radiusSq = expandRadius * expandRadius;
            Vector2 closest = Vector2.Clamp(Projectile.Center,
                targetHitbox.TopLeft(), targetHitbox.BottomRight());
            return Vector2.DistanceSquared(Projectile.Center, closest) < radiusSq;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Both debuffs
            target.AddBuff(ModContent.BuffType<RoseThornBleedDebuff>(), 300);
            target.AddBuff(ModContent.BuffType<VineRootDebuff>(), 120);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float alpha = GetAlpha();
            float progress = timer / (float)MaxLifetime;
            float time = (float)Main.timeForVisualEffects * 0.015f;

            sb.End();

            // ── LAYER 0: CelebrationAura shader — expanding botanical shockwave ──
            Effect auraShader = OdeToJoyShaders.CelebrationAura;
            if (auraShader != null)
            {
                float auraRadius = expandRadius * 2f / Math.Max(ThornboundTextures.OJPowerEffectRing.Value.Width, ThornboundTextures.OJPowerEffectRing.Value.Height);
                OdeToJoyShaders.SetAuraParams(auraShader, time + progress * 5f, ThornboundTextures.BloomGold,
                    ThornboundTextures.JubilantLight, alpha * 0.6f, 3f, auraRadius * 0.4f, 6f);
                OdeToJoyShaders.BeginShaderBatch(sb, auraShader, "CelebrationAuraTechnique");
                auraShader.CurrentTechnique.Passes["P0"].Apply();
                Texture2D glowTex = ThornboundTextures.SoftGlow.Value;
                Vector2 glowOrig = glowTex.Size() / 2f;
                sb.Draw(glowTex, drawPos, null, Color.White * alpha,
                    0f, glowOrig, auraRadius * 1.5f, SpriteEffects.None, 0f);
                sb.End();
            }

            // ── LAYER 1: GardenBloom JubilantPulse shader — floral burst center ──
            Effect bloomShader = OdeToJoyShaders.GardenBloom;
            if (bloomShader != null)
            {
                Texture2D floralTex = ThornboundTextures.OJFloralImpact.Value;
                Vector2 floralOrigin = floralTex.Size() / 2f;
                float floralScale = expandRadius * 1.5f / Math.Max(floralTex.Width, floralTex.Height);
                OdeToJoyShaders.SetBloomParams(bloomShader, time, ThornboundTextures.PetalPink,
                    ThornboundTextures.BloomGold, alpha * 0.5f, 2f, floralScale);
                bloomShader.Parameters["uPulseSpeed"]?.SetValue(2.5f);
                OdeToJoyShaders.BeginDeferredShaderBatch(sb, bloomShader, "JubilantPulseTechnique");
                sb.Draw(floralTex, drawPos, null, Color.White * alpha,
                    timer * 0.02f, floralOrigin, floralScale, SpriteEffects.None, 0f);
                sb.End();
            }

            // ── LAYER 2: Additive overlays ──
            OdeToJoyShaders.BeginAdditiveBatch(sb);

            // Expanding shockwave ring
            Texture2D ringTex = ThornboundTextures.OJPowerEffectRing.Value;
            Vector2 ringOrigin = ringTex.Size() / 2f;
            float ringScale = expandRadius * 2f / Math.Max(ringTex.Width, ringTex.Height);
            float ringAlpha = alpha * (1f - progress * 0.5f);
            sb.Draw(ringTex, drawPos, null,
                ThornboundTextures.BloomGold * ringAlpha * 0.5f,
                0f, ringOrigin, ringScale, SpriteEffects.None, 0f);

            // Harmonic wave
            Texture2D waveTex = ThornboundTextures.OJHarmonicWaveImpact.Value;
            Vector2 waveOrigin = waveTex.Size() / 2f;
            float waveScale = expandRadius * 1.8f / Math.Max(waveTex.Width, waveTex.Height);
            sb.Draw(waveTex, drawPos, null,
                ThornboundTextures.RadiantAmber * alpha * 0.3f,
                -timer * 0.015f, waveOrigin, waveScale,
                SpriteEffects.None, 0f);

            // Wide outer glow
            Texture2D softGlow = ThornboundTextures.SoftGlow.Value;
            Vector2 glowOrigin = softGlow.Size() / 2f;
            float outerGlowScale = expandRadius * 3f / Math.Max(softGlow.Width, softGlow.Height);
            sb.Draw(softGlow, drawPos, null,
                ThornboundTextures.JubilantLight * alpha * 0.25f,
                0f, glowOrigin, outerGlowScale,
                SpriteEffects.None, 0f);

            // Hot core
            float coreGlowScale = expandRadius * 0.7f / Math.Max(softGlow.Width, softGlow.Height);
            float coreBright = progress < 0.2f ? (progress / 0.2f) : (1f - (progress - 0.2f) / 0.8f);
            sb.Draw(softGlow, drawPos, null,
                ThornboundTextures.PureJoyWhite * alpha * coreBright * 0.6f,
                0f, glowOrigin, coreGlowScale,
                SpriteEffects.None, 0f);

            // Beam surge impact overlay
            Texture2D surgeTex = ThornboundTextures.OJBeamSurgeImpact.Value;
            Vector2 surgeOrigin = surgeTex.Size() / 2f;
            float surgeScale = expandRadius * 1.2f / Math.Max(surgeTex.Width, surgeTex.Height);
            if (progress < 0.5f)
            {
                float surgeAlpha = (1f - progress * 2f);
                sb.Draw(surgeTex, drawPos, null,
                    ThornboundTextures.BloomGold * surgeAlpha * 0.4f,
                    timer * 0.03f, surgeOrigin, surgeScale,
                    SpriteEffects.None, 0f);
            }

            // Theme blossom sparkle accent
            OdeToJoyVFXLibrary.DrawThemeBlossomSparkle(sb, Projectile.Center, 1f, 0.5f);

            sb.End();
            OdeToJoyShaders.RestoreSpriteBatch(sb);
            return false;
        }

        public override void OnKill(int timeLeft)
        {
            // Final burst of golden particles
            for (int i = 0; i < 20; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(6f, 6f);
                Color col = Color.Lerp(ThornboundTextures.BloomGold,
                    ThornboundTextures.JubilantLight, Main.rand.NextFloat());
                Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.RainbowMk2, vel,
                    newColor: col, Scale: Main.rand.NextFloat(0.3f, 0.9f));
                dust.noGravity = true;
            }
        }
    }
}
