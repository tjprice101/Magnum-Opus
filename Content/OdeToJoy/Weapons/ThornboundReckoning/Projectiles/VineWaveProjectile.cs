using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.OdeToJoy.Weapons.ThornboundReckoning.Buffs;
using MagnumOpus.Content.OdeToJoy.Weapons.ThornboundReckoning.Utilities;
using MagnumOpus.Content.OdeToJoy;

namespace MagnumOpus.Content.OdeToJoy.Weapons.ThornboundReckoning.Projectiles
{
    /// <summary>
    /// Vine Wave — Traveling vine wave from Phase 1 swing.
    /// Damages and slows enemies. Leaves thorn residue trail.
    ///
    /// VFX: ImpactFoundation RippleShader concentric rings + bloom layers + botanical dust.
    /// </summary>
    public class VineWaveProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Textures/InvisibleProjectile";

        private const int MaxLifetime = 75;
        private const int FadeInFrames = 8;
        private const int FadeOutFrames = 15;
        private int timer;
        private float seed;

        public override void SetDefaults()
        {
            Projectile.width = 80;
            Projectile.height = 40;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 6;
            Projectile.timeLeft = MaxLifetime;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 12;
        }

        public override void AI()
        {
            if (timer == 0)
                seed = Main.rand.NextFloat(100f);

            timer++;
            Projectile.rotation = Projectile.velocity.ToRotation();

            // Slow down gradually for creeping vine feel
            if (Projectile.velocity.Length() > 3f)
                Projectile.velocity *= 0.985f;

            // Gentle sine bob perpendicular to travel
            Vector2 perpDir = new Vector2(-Projectile.velocity.Y, Projectile.velocity.X).SafeNormalize(Vector2.UnitY);
            Projectile.position += perpDir * MathF.Sin(timer * 0.08f + seed) * 0.6f;

            // Lighting
            float alpha = GetAlpha();
            Lighting.AddLight(Projectile.Center,
                ThornboundTextures.BloomGold.ToVector3() * 0.5f * alpha);

            // Vine trail dust
            if (timer % 3 == 0)
            {
                Color col = ThornboundTextures.GetBotanicalGradient(Main.rand.NextFloat());
                Vector2 vel = Main.rand.NextVector2Circular(1.5f, 1.5f);
                Dust dust = Dust.NewDustPerfect(
                    Projectile.Center + Main.rand.NextVector2Circular(20f, 12f),
                    DustID.RainbowMk2, vel, newColor: col,
                    Scale: Main.rand.NextFloat(0.3f, 0.6f));
                dust.noGravity = true;
                dust.fadeIn = 0.5f;
            }
        }

        private float GetAlpha()
        {
            float fadeIn = MathHelper.Clamp(timer / (float)FadeInFrames, 0f, 1f);
            float fadeOut = Projectile.timeLeft < FadeOutFrames
                ? Projectile.timeLeft / (float)FadeOutFrames
                : 1f;
            return fadeIn * fadeOut;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Apply Vine Root slow debuff
            target.AddBuff(ModContent.BuffType<VineRootDebuff>(), 120);

            // Build charge on the player
            Player owner = Main.player[Projectile.owner];
            var tbp = owner.GetModPlayer<ThornboundPlayer>();
            tbp.AddVineWaveCharge();

            // Impact dust burst
            for (int i = 0; i < 10; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(5f, 5f);
                Color col = ThornboundTextures.GetBotanicalGradient(Main.rand.NextFloat());
                Dust dust = Dust.NewDustPerfect(target.Center, DustID.RainbowMk2, vel,
                    newColor: col, Scale: Main.rand.NextFloat(0.4f, 0.8f));
                dust.noGravity = true;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float alpha = GetAlpha();
            float time = (float)Main.timeForVisualEffects * 0.015f;

            sb.End();

            // ── LAYER 0: VerdantSlash VerdantSlashTechnique shader — vine ripple ──
            Effect slashShader = OdeToJoyShaders.VerdantSlash;
            if (slashShader != null)
            {
                OdeToJoyShaders.SetSlashParams(slashShader, time + seed * 0.5f,
                    ThornboundTextures.BloomGold, ThornboundTextures.PetalPink,
                    alpha * 0.5f, 1.8f, 0.3f);
                OdeToJoyShaders.BeginDeferredShaderBatch(sb, slashShader, "VerdantSlashTechnique");
                Texture2D impactTex = ThornboundTextures.OJHarmonicImpact.Value;
                Vector2 impactOrigin = impactTex.Size() / 2f;
                float ringScale = 0.12f + timer * 0.003f;
                sb.Draw(impactTex, drawPos, null, Color.White * alpha,
                    Projectile.rotation, impactOrigin, ringScale, SpriteEffects.None, 0f);
                sb.End();
            }

            // ── LAYER 1: Additive bloom overlays ──
            OdeToJoyShaders.BeginAdditiveBatch(sb);

            float pulse = 0.85f + 0.15f * MathF.Sin(time * 4f + seed);
            float ringScale2 = 0.12f + timer * 0.003f;
            Texture2D impactTex2 = ThornboundTextures.OJHarmonicImpact.Value;
            Vector2 impactOrigin2 = impactTex2.Size() / 2f;

            sb.Draw(impactTex2, drawPos, null,
                ThornboundTextures.RoseShadow * alpha * 0.3f * pulse,
                Projectile.rotation, impactOrigin2,
                ringScale2 * 1.2f, SpriteEffects.None, 0f);
            sb.Draw(impactTex2, drawPos, null,
                ThornboundTextures.PetalPink * alpha * 0.4f * pulse,
                Projectile.rotation, impactOrigin2,
                ringScale2, SpriteEffects.None, 0f);

            Texture2D softGlow = ThornboundTextures.SoftGlow.Value;
            Vector2 glowOrigin = softGlow.Size() / 2f;
            sb.Draw(softGlow, drawPos, null,
                ThornboundTextures.BloomGold * alpha * 0.35f * pulse,
                0f, glowOrigin, 0.18f * pulse, SpriteEffects.None, 0f);
            sb.Draw(softGlow, drawPos, null,
                ThornboundTextures.JubilantLight * alpha * 0.2f,
                0f, glowOrigin, 0.1f * pulse, SpriteEffects.None, 0f);

            // Theme blossom sparkle accent
            OdeToJoyVFXLibrary.DrawThemeBlossomSparkle(sb, Projectile.Center, 1f, 0.5f);

            sb.End();
            OdeToJoyShaders.RestoreSpriteBatch(sb);
            return false;
        }

        public override void OnKill(int timeLeft)
        {
            // Death burst — petal scatter
            for (int i = 0; i < 15; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(4f, 4f);
                Color col = ThornboundTextures.GetBotanicalGradient(Main.rand.NextFloat());
                Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.RainbowMk2, vel,
                    newColor: col, Scale: Main.rand.NextFloat(0.3f, 0.6f));
                dust.noGravity = true;
                dust.fadeIn = 0.4f;
            }
        }
    }
}