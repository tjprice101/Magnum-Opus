using MagnumOpus.Common.Systems.VFX.Core;
using MagnumOpus.Content.DiesIrae;
using MagnumOpus.Content.DiesIrae.Weapons.ArbitersSentence.Buffs;
using MagnumOpus.Content.DiesIrae.Weapons.ArbitersSentence.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.VFX;

namespace MagnumOpus.Content.DiesIrae.Weapons.ArbitersSentence.Projectiles
{
    /// <summary>
    /// Judgment Flame ? precision fire bullet with tight trail and focused bloom.
    /// Short-range flamethrower shot. On hit: applies Judgment Flame stacking debuff.
    /// At 5 stacks �� Sentence Cage (root + 2x next hit).
    /// </summary>
    public class JudgmentFlameProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Textures/InvisibleProjectile";

        private const int TrailLength = 10;
        private Vector2[] trailPositions = new Vector2[TrailLength];
        private float[] trailRotations = new float[TrailLength];
        private int trailHead = 0;
        private bool trailInitialized = false;

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 3;
            Projectile.timeLeft = 35;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = false;
            Projectile.alpha = 255;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 6;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();

            // Record trail
            if (!trailInitialized)
            {
                for (int i = 0; i < TrailLength; i++)
                {
                    trailPositions[i] = Projectile.Center;
                    trailRotations[i] = Projectile.rotation;
                }
                trailInitialized = true;
            }
            trailPositions[trailHead] = Projectile.Center;
            trailRotations[trailHead] = Projectile.rotation;
            trailHead = (trailHead + 1) % TrailLength;

            // Precision fire dust trail ? tight, focused
            if (Main.rand.NextBool(2))
            {
                Vector2 perpendicular = Projectile.velocity.SafeNormalize(Vector2.UnitX).RotatedBy(MathHelper.PiOver2);
                Vector2 dustPos = Projectile.Center + perpendicular * Main.rand.NextFloat(-3f, 3f);
                Dust d = Dust.NewDustPerfect(dustPos, DustID.Torch, -Projectile.velocity * 0.2f, 0, default, 0.8f);
                d.noGravity = true;
                d.fadeIn = 0.3f;
            }

            // Emit light
            Lighting.AddLight(Projectile.Center, 0.6f, 0.15f, 0.05f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.OnFire3, 120);

            // Apply Judgment Flame stacking debuff
            var globalNPC = target.GetGlobalNPC<ArbitersSentenceGlobalNPC>();
            globalNPC.IncrementFlameStack(target);
            globalNPC.TrackConsecutiveHit(target);

            // Precision impact VFX
            ArbitersSentenceUtils.DoFlameImpact(target.Center, globalNPC.JudgmentFlameStacks);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            Texture2D glow = MagnumTextureRegistry.GetSoftGlow();
            if (glow != null)
            {
                Vector2 origin = glow.Size() / 2f;

                // Draw trail ? tight precision trail
                for (int i = 0; i < TrailLength; i++)
                {
                    int idx = (trailHead - 1 - i + TrailLength) % TrailLength;
                    float progress = (float)i / TrailLength;
                    float alpha = (1f - progress) * 0.5f;
                    float scale = (1f - progress) * 0.025f;

                    Color trailColor = Color.Lerp(ArbitersSentenceUtils.JudgmentEmber, ArbitersSentenceUtils.PrecisionCrimson, progress) * alpha;
                    Vector2 pos = trailPositions[idx] - Main.screenPosition;
                    sb.Draw(glow, pos, null, trailColor, 0f, origin, scale, SpriteEffects.None, 0f);
                }
            }

            // Draw bullet body
            ArbitersSentenceUtils.DrawFlameBulletBody(sb, Projectile.Center, Projectile.rotation, Projectile.ai[0]++);

            // Dies Irae theme accent layer
            ArbitersSentenceUtils.DrawThemeAccents(sb, Projectile.Center, 1f, 0.6f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            return false;
        }
    }

    /// <summary>
    /// Purgatory Ember ? lingering ground fire. Spawned occasionally by the flamethrower stream.
    /// Sits on the ground, damages enemies that walk through, slowly fades.
    /// </summary>
    public class PurgatoryEmberProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Textures/InvisibleProjectile";

        private const int MaxLifetime = 180; // 3 seconds

        public override void SetDefaults()
        {
            Projectile.width = 40;
            Projectile.height = 20;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = -1;
            Projectile.timeLeft = MaxLifetime;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 20;
        }

        public override void AI()
        {
            // Decelerate quickly and stick to ground
            Projectile.velocity *= 0.92f;
            if (Projectile.velocity.Length() < 0.1f)
                Projectile.velocity = Vector2.Zero;

            // Ember dust
            ArbitersSentenceUtils.SpawnPurgatoryEmberDust(Projectile.Center, 20f);

            // Lighting
            float life = (float)Projectile.timeLeft / MaxLifetime;
            Lighting.AddLight(Projectile.Center, 0.4f * life, 0.1f * life, 0.02f * life);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.OnFire3, 90);

            // Also apply Judgment Flame stacking
            var globalNPC = target.GetGlobalNPC<ArbitersSentenceGlobalNPC>();
            globalNPC.IncrementFlameStack(target);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            Texture2D glow = MagnumTextureRegistry.GetSoftGlow();
            if (glow != null)
            {
                Vector2 origin = glow.Size() / 2f;
                Vector2 pos = Projectile.Center - Main.screenPosition;
                float life = (float)Projectile.timeLeft / MaxLifetime;
                float pulse = 0.7f + 0.3f * (float)Math.Sin(Projectile.ai[0] * 0.15f);
                Projectile.ai[0]++;

                // Ground fire glow ? crimson outer
                sb.Draw(glow, pos, null, ArbitersSentenceUtils.PrecisionCrimson * 0.3f * life * pulse, 0f, origin,
                    new Vector2(0.06f, 0.025f), SpriteEffects.None, 0f);
                // Ember mid
                sb.Draw(glow, pos, null, ArbitersSentenceUtils.JudgmentEmber * 0.4f * life * pulse, 0f, origin,
                    new Vector2(0.035f, 0.015f), SpriteEffects.None, 0f);
                // Hot core
                sb.Draw(glow, pos, null, ArbitersSentenceUtils.FocusWhite * 0.5f * life, 0f, origin,
                    new Vector2(0.015f, 0.008f), SpriteEffects.None, 0f);
            }

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            return false;
        }
    }
}