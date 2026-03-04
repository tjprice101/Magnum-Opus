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
    /// Thorn Lash — V-pattern projectile from Phase 2.
    /// Embeds in enemies, dealing Rose Thorn Bleed DoT.
    /// VFX: CellularCrack-textured thorn sprite with Petal Pink trailing sparks.
    /// </summary>
    public class ThornLashProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Textures/InvisibleProjectile";

        private const int MaxLifetime = 50;
        private const int FadeInFrames = 5;
        private const int FadeOutFrames = 10;
        private int timer;
        private float seed;

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 2;
            Projectile.timeLeft = MaxLifetime;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 15;
        }

        public override void AI()
        {
            if (timer == 0)
                seed = Main.rand.NextFloat(100f);

            timer++;
            Projectile.rotation = Projectile.velocity.ToRotation();

            // Slight deceleration
            if (Projectile.velocity.Length() > 4f)
                Projectile.velocity *= 0.97f;

            // Lighting
            float alpha = GetAlpha();
            Lighting.AddLight(Projectile.Center,
                ThornboundTextures.RadiantAmber.ToVector3() * 0.4f * alpha);

            // Trailing sparks
            if (timer % 2 == 0)
            {
                Vector2 vel = -Projectile.velocity * 0.1f + Main.rand.NextVector2Circular(1f, 1f);
                Color col = Color.Lerp(ThornboundTextures.PetalPink, ThornboundTextures.RadiantAmber,
                    Main.rand.NextFloat());
                Dust dust = Dust.NewDustPerfect(
                    Projectile.Center + Main.rand.NextVector2Circular(4f, 4f),
                    DustID.RainbowMk2, vel, newColor: col,
                    Scale: Main.rand.NextFloat(0.2f, 0.45f));
                dust.noGravity = true;
                dust.fadeIn = 0.4f;
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
            // Rose Thorn Bleed — embeds, stacks
            target.AddBuff(ModContent.BuffType<RoseThornBleedDebuff>(), 240);

            // Build Reckoning Charge for thorn embed
            Player owner = Main.player[Projectile.owner];
            var tbp = owner.GetModPlayer<ThornboundPlayer>();
            tbp.AddThornEmbedCharge();

            // Embed impact burst
            for (int i = 0; i < 6; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(4f, 4f);
                Color col = Color.Lerp(ThornboundTextures.RoseShadow, ThornboundTextures.RadiantAmber,
                    Main.rand.NextFloat());
                Dust dust = Dust.NewDustPerfect(target.Center, DustID.RainbowMk2, vel,
                    newColor: col, Scale: Main.rand.NextFloat(0.4f, 0.7f));
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

            // ── LAYER 0: VerdantSlash ThornImpact shader — thorn body ──
            Effect slashShader = OdeToJoyShaders.VerdantSlash;
            if (slashShader != null)
            {
                OdeToJoyShaders.SetSlashParams(slashShader, time + seed * 0.3f,
                    ThornboundTextures.RadiantAmber, ThornboundTextures.RoseShadow,
                    alpha * 0.55f, 1.5f, 0f);
                OdeToJoyShaders.BeginDeferredShaderBatch(sb, slashShader, "ThornImpactTechnique");
                Texture2D thornTex = ThornboundTextures.OJThornFragment.Value;
                Vector2 thornOrigin = thornTex.Size() / 2f;
                sb.Draw(thornTex, drawPos, null, Color.White * alpha,
                    Projectile.rotation, thornOrigin, 0.6f, SpriteEffects.None, 0f);
                sb.End();
            }

            // ── LAYER 1: Additive bloom ──
            OdeToJoyShaders.BeginAdditiveBatch(sb);

            Texture2D softGlow = ThornboundTextures.SoftGlow.Value;
            Vector2 glowOrigin = softGlow.Size() / 2f;
            sb.Draw(softGlow, drawPos, null,
                ThornboundTextures.PetalPink * alpha * 0.3f,
                0f, glowOrigin, 0.08f, SpriteEffects.None, 0f);

            // Theme blossom sparkle accent
            OdeToJoyVFXLibrary.DrawThemeBlossomSparkle(sb, Projectile.Center, 1f, 0.5f);

            sb.End();
            OdeToJoyShaders.RestoreSpriteBatch(sb);
            return false;
        }

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 8; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(3f, 3f);
                Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.RainbowMk2, vel,
                    newColor: ThornboundTextures.RoseShadow,
                    Scale: Main.rand.NextFloat(0.2f, 0.5f));
                dust.noGravity = true;
            }
        }
    }
}
