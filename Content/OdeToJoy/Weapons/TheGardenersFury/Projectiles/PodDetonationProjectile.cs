using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.OdeToJoy;
using MagnumOpus.Content.OdeToJoy.Weapons.TheGardenersFury.Buffs;
using MagnumOpus.Content.OdeToJoy.Weapons.ThornboundReckoning.Buffs;

namespace MagnumOpus.Content.OdeToJoy.Weapons.TheGardenersFury.Projectiles
{
    /// <summary>
    /// Pod Detonation — AoE explosion from seed pod detonation.
    /// Visual style depends on pod type:
    ///   Bloom (0): Rose petal burst + brief blind
    ///   Thorn (1): Thorn shrapnel spray + bleed
    ///   Pollen (2): Golden pollen cloud + slow
    /// ai[0] = pod type, ai[1] = radius
    /// </summary>
    public class PodDetonationProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Textures/InvisibleProjectile";

        private const int MaxLifetime = 30;
        private int timer;

        private int PodType => (int)Projectile.ai[0];
        private float Radius => Projectile.ai[1] > 0 ? Projectile.ai[1] : 64f;

        public override void SetDefaults()
        {
            Projectile.width = 128;
            Projectile.height = 128;
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
            Projectile.velocity = Vector2.Zero;

            // Update hitbox to match radius
            int size = (int)(Radius * 2);
            Projectile.width = size;
            Projectile.height = size;

            float alpha = GetAlpha();
            Color glowColor = GardenerFuryTextures.GetPodColor(PodType);
            Lighting.AddLight(Projectile.Center, glowColor.ToVector3() * 0.6f * alpha);
        }

        private float GetAlpha()
        {
            float progress = timer / (float)MaxLifetime;
            return progress < 0.15f
                ? progress / 0.15f
                : 1f - (progress - 0.15f) / 0.85f;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            Vector2 closest = Vector2.Clamp(Projectile.Center,
                targetHitbox.TopLeft(), targetHitbox.BottomRight());
            return Vector2.DistanceSquared(Projectile.Center, closest) < Radius * Radius;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            switch (PodType)
            {
                case 0: // Bloom — brief confusion (visual blind)
                    target.AddBuff(BuffID.Confused, 90);
                    break;
                case 1: // Thorn — bleed
                    target.AddBuff(ModContent.BuffType<RoseThornBleedDebuff>(), 240);
                    break;
                case 2: // Pollen — slow
                    target.AddBuff(ModContent.BuffType<PollenSlowDebuff>(), 180);
                    break;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float alpha = GetAlpha();
            float progress = timer / (float)MaxLifetime;
            Color podColor = GardenerFuryTextures.GetPodColor(PodType);

            float expandProgress = MathHelper.SmoothStep(0f, 1f, Math.Min(progress / 0.3f, 1f));

            // ---- Layer 1: CelebrationAura shader expanding detonation field ----
            Effect celebShader = OdeToJoyShaders.CelebrationAura;
            if (celebShader != null)
            {
                OdeToJoyShaders.SetAuraParams(celebShader, (float)Main.gameTimeCache.TotalGameTime.TotalSeconds, podColor, GardenerFuryTextures.BloomGold, alpha * 0.55f, 1.2f * expandProgress, 1.0f, 4f, timer * 0.02f);
                OdeToJoyShaders.BeginShaderBatch(sb, celebShader, "CelebrationAuraTechnique");

                Texture2D ringTex = GardenerFuryTextures.OJPowerEffectRing.Value;
                Vector2 ringOrigin = ringTex.Size() / 2f;
                float ringScale = Radius * 2f * expandProgress / Math.Max(ringTex.Width, ringTex.Height);

                sb.Draw(ringTex, drawPos, null, podColor * alpha * 0.6f,
                    timer * 0.03f, ringOrigin, ringScale, SpriteEffects.None, 0f);

                OdeToJoyShaders.BeginAdditiveBatch(sb);
            }
            else
            {
                sb.End();
                sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                    Main.DefaultSamplerState, DepthStencilState.None,
                    RasterizerState.CullCounterClockwise, null,
                    Main.GameViewMatrix.TransformationMatrix);

                // Fallback expanding ring
                Texture2D ringTex = GardenerFuryTextures.OJPowerEffectRing.Value;
                Vector2 ringOrigin = ringTex.Size() / 2f;
                float ringScale = Radius * 2f * expandProgress / Math.Max(ringTex.Width, ringTex.Height);
                sb.Draw(ringTex, drawPos, null, podColor * alpha * 0.6f,
                    timer * 0.03f, ringOrigin, ringScale, SpriteEffects.None, 0f);
            }

            // ---- Layer 2: Pod-type-specific overlay ----
            Texture2D overlayTex;
            Color overlayColor;

            switch (PodType)
            {
                case 0: // Bloom — floral pattern
                    overlayTex = GardenerFuryTextures.OJFloralImpact.Value;
                    overlayColor = GardenerFuryTextures.PetalPink;
                    break;
                case 1: // Thorn — harmonic impact (sharp)
                    overlayTex = GardenerFuryTextures.OJHarmonicImpact.Value;
                    overlayColor = GardenerFuryTextures.RoseShadow;
                    break;
                case 2: // Pollen — beam surge (golden)
                    overlayTex = GardenerFuryTextures.OJBeamSurgeImpact.Value;
                    overlayColor = GardenerFuryTextures.BloomGold;
                    break;
                default:
                    overlayTex = GardenerFuryTextures.OJFloralImpact.Value;
                    overlayColor = GardenerFuryTextures.BloomGold;
                    break;
            }

            Vector2 overlayOrigin = overlayTex.Size() / 2f;
            float overlayScale = Radius * 1.5f * expandProgress / Math.Max(overlayTex.Width, overlayTex.Height);

            sb.Draw(overlayTex, drawPos, null, overlayColor * alpha * 0.5f,
                -timer * 0.02f, overlayOrigin, overlayScale, SpriteEffects.None, 0f);

            // ---- Layer 3: PollenDrift BloomDetonation shader burst ----
            Effect pollenShader = OdeToJoyShaders.PollenDrift;
            if (pollenShader != null)
            {
                float detonationRadius = Radius * expandProgress / 64f;
                OdeToJoyShaders.SetPollenParams(pollenShader, (float)Main.gameTimeCache.TotalGameTime.TotalSeconds, podColor, GardenerFuryTextures.BloomGold, alpha * 0.45f, 1.0f, detonationRadius);
                OdeToJoyShaders.BeginShaderBatch(sb, pollenShader, "BloomDetonationTechnique");

                Texture2D burstGlow = GardenerFuryTextures.SoftGlow.Value;
                Vector2 burstOrigin = burstGlow.Size() / 2f;
                float burstScale = Radius * 2.5f * expandProgress / Math.Max(burstGlow.Width, burstGlow.Height);
                sb.Draw(burstGlow, drawPos, null, podColor * alpha * 0.4f,
                    0f, burstOrigin, burstScale, SpriteEffects.None, 0f);

                OdeToJoyShaders.BeginAdditiveBatch(sb);
            }

            // ---- Layer 4: Outer ambient glow ----
            Texture2D softGlow = GardenerFuryTextures.SoftGlow.Value;
            Vector2 glowOrigin = softGlow.Size() / 2f;
            float outerScale = Radius * 3f * expandProgress / Math.Max(softGlow.Width, softGlow.Height);

            sb.Draw(softGlow, drawPos, null, podColor * alpha * 0.25f,
                0f, glowOrigin, outerScale, SpriteEffects.None, 0f);

            // ---- Layer 5: Hot core flash ----
            float coreBright = progress < 0.15f ? progress / 0.15f : Math.Max(0, 1f - (progress - 0.15f) / 0.5f);
            float coreScale = Radius * 0.8f * expandProgress / Math.Max(softGlow.Width, softGlow.Height);

            sb.Draw(softGlow, drawPos, null,
                GardenerFuryTextures.PureJoyWhite * coreBright * 0.7f,
                0f, glowOrigin, coreScale, SpriteEffects.None, 0f);

            // Theme blossom sparkle accent
            OdeToJoyVFXLibrary.DrawThemeBlossomSparkle(sb, Projectile.Center, 1f, 0.5f);

            OdeToJoyShaders.RestoreSpriteBatch(sb);

            return false;
        }

        public override void OnKill(int timeLeft)
        {
            Color podColor = GardenerFuryTextures.GetPodColor(PodType);
            for (int i = 0; i < 10; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(4f, 4f);
                Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.RainbowMk2, vel,
                    newColor: podColor, Scale: Main.rand.NextFloat(0.3f, 0.7f));
                dust.noGravity = true;
            }
        }
    }
}
