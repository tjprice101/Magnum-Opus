using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.DiesIrae.Weapons.ArbitersSentence.Utilities;
using MagnumOpus.Content.DiesIrae.Weapons.ArbitersSentence.Particles;

namespace MagnumOpus.Content.DiesIrae.Weapons.ArbitersSentence.Projectiles
{
    /// <summary>
    /// Judgment Flame — A rapid-fire flamethrower projectile.
    /// Short-range, wide spread, grows and fades quickly.
    /// Every 15 ticks spawns a lingering Purgatory Ember that deals persistent damage.
    /// </summary>
    public class JudgmentFlameProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/PointBloom";

        private static Asset<Texture2D> bloomTexture;

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
            // Flame grows over time
            float progress = 1f - (Projectile.timeLeft / 35f);
            float scale = 0.3f + progress * 0.7f;

            Projectile.velocity *= 0.97f;
            Projectile.rotation = Projectile.velocity.ToRotation();

            // Spawn flame particles along the stream
            Color flameColor = ArbiterUtils.GetFlameColor(1f - progress);
            ArbiterParticleHandler.Spawn(new JudgmentFlameParticle(
                Projectile.Center + Main.rand.NextVector2Circular(6f, 6f),
                Projectile.velocity * 0.15f + Main.rand.NextVector2Circular(0.5f, 0.5f),
                flameColor, scale * 0.7f, 18));

            // Embers rising from flames
            if (Main.rand.NextBool(4))
            {
                Vector2 emberVel = new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-2f, -0.5f));
                ArbiterParticleHandler.Spawn(new FlameEmberParticle(
                    Projectile.Center, emberVel,
                    Color.Lerp(ArbiterUtils.HellflameOrange, ArbiterUtils.PurgatoryGold, Main.rand.NextFloat()),
                    0.1f, 15));
            }

            // Smoke at the edges
            if (Main.rand.NextBool(5))
            {
                Vector2 smokeVel = Projectile.velocity.RotatedBy(Main.rand.NextFloat(-0.6f, 0.6f)) * 0.1f;
                ArbiterParticleHandler.Spawn(new JudgmentSmokeParticle(
                    Projectile.Center, smokeVel, scale * 0.4f, 25));
            }

            // Every 15 ticks: spawn lingering purgatory ember (a new projectile)
            if (Projectile.timeLeft % 15 == 0 && Main.myPlayer == Projectile.owner)
            {
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center,
                    Vector2.Zero, ModContent.ProjectileType<PurgatoryEmberProjectile>(),
                    Projectile.damage / 3, 0f, Projectile.owner);
            }

            Projectile.alpha = (int)(255 * (1f - MathHelper.Clamp(progress * 4, 0, 1)));
            Lighting.AddLight(Projectile.Center, ArbiterUtils.HellflameOrange.ToVector3() * scale * 0.4f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.OnFire3, 120);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            bloomTexture ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftRadialBloom");
            if (!bloomTexture.IsLoaded) return false;
            var tex = bloomTexture.Value;

            float progress = 1f - (Projectile.timeLeft / 35f);
            float scale = (0.3f + progress * 0.5f) * 0.5f;
            float alpha = (float)Math.Min(progress * 4f, 1f) * (1f - (float)Math.Pow(progress, 2));
            Color c = ArbiterUtils.GetFlameColor(0.3f + progress * 0.4f);

            Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, null,
                ArbiterUtils.Additive(c, alpha * 0.4f), 0f, tex.Size() / 2f, scale, SpriteEffects.None, 0);
            Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, null,
                ArbiterUtils.Additive(ArbiterUtils.SentenceWhite, alpha * 0.2f), 0f, tex.Size() / 2f, scale * 0.3f, SpriteEffects.None, 0);

            return false;
        }
    }

    /// <summary>
    /// Purgatory Ember — A lingering AOE projectile that sits in place dealing damage to nearby enemies.
    /// Pulsates with hellfire glow and fades out over 60 ticks.
    /// </summary>
    public class PurgatoryEmberProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/PointBloom";

        private static Asset<Texture2D> bloomTexture;

        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 30;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 60;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 15;
        }

        public override void AI()
        {
            float life = 1f - (Projectile.timeLeft / 60f);

            // Subtle flicker embers
            if (Main.rand.NextBool(3))
            {
                Vector2 vel = new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), Main.rand.NextFloat(-1f, -0.3f));
                ArbiterParticleHandler.Spawn(new FlameEmberParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(8f, 8f), vel,
                    ArbiterUtils.GetFlameColor(Main.rand.NextFloat(0.3f, 0.7f)),
                    0.08f, 12));
            }

            Lighting.AddLight(Projectile.Center, ArbiterUtils.JudgmentCrimson.ToVector3() * (1f - life) * 0.3f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.OnFire3, 60);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            bloomTexture ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftRadialBloom");
            if (!bloomTexture.IsLoaded) return false;
            var tex = bloomTexture.Value;

            float life = 1f - (Projectile.timeLeft / 60f);
            float pulse = 0.7f + 0.3f * (float)Math.Sin(Main.GameUpdateCount * 0.3f);
            float alpha = (1f - life * life) * pulse;

            Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, null,
                ArbiterUtils.Additive(ArbiterUtils.JudgmentCrimson, alpha * 0.3f), 0f, tex.Size() / 2f, 0.5f, SpriteEffects.None, 0);
            Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, null,
                ArbiterUtils.Additive(ArbiterUtils.HellflameOrange, alpha * 0.2f), 0f, tex.Size() / 2f, 0.25f, SpriteEffects.None, 0);

            return false;
        }
    }
}
