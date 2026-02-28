using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.DiesIrae.Weapons.DeathTollingBell.Utilities;
using MagnumOpus.Content.DiesIrae.Weapons.DeathTollingBell.Particles;

namespace MagnumOpus.Content.DiesIrae.Weapons.DeathTollingBell.Projectiles
{
    /// <summary>
    /// Bell Toll Wave — A devastating expanding shockwave ring released by the Bell Minion.
    /// Released in 3 concentric rings of 12 projectiles each at different speeds.
    /// Each toll wave appears as a fading ring of crimson/gold energy that damages on contact.
    ///
    /// VFX: Glowing radial ring + ember trail + fading bloom core.
    /// ai[0] = ring index (0-2, affects color and scale).
    /// </summary>
    public class BellTollWaveProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/PointBloom";

        private static Asset<Texture2D> bloomTexture;
        private static Asset<Texture2D> trailTexture;

        // ─── Trail cache for short tail ───
        private const int TrailLength = 6;
        private Vector2[] trailCache = new Vector2[TrailLength];

        private ref float RingIndex => ref Projectile.ai[0];

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = 24;
            Projectile.height = 24;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.penetrate = 3;
            Projectile.timeLeft = 60;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 12;
            Projectile.alpha = 0;
        }

        public override void AI()
        {
            // Update trail
            UpdateTrailCache();

            // Fade out near end of life
            if (Projectile.timeLeft < 15)
                Projectile.alpha = (int)MathHelper.Lerp(0, 255, 1f - Projectile.timeLeft / 15f);

            // Slow deceleration
            Projectile.velocity *= 0.985f;

            // Rotation follows velocity
            Projectile.rotation = Projectile.velocity.ToRotation();

            // Ambient VFX
            SpawnTrailParticles();
        }

        private void UpdateTrailCache()
        {
            for (int i = TrailLength - 1; i > 0; i--)
                trailCache[i] = trailCache[i - 1];
            trailCache[0] = Projectile.Center;
        }

        private void SpawnTrailParticles()
        {
            if (Main.rand.NextBool(3))
            {
                Vector2 off = Main.rand.NextVector2Circular(6f, 6f);
                Vector2 vel = -Projectile.velocity.SafeNormalize(Vector2.Zero) * 0.5f;
                Color c = GetRingColor(RingIndex, Main.rand.NextFloat());
                BellParticleHandler.SpawnParticle(new BellEmberParticle(Projectile.Center + off, vel, 0.15f, 15));
            }
        }

        private static Color GetRingColor(float ringIndex, float t)
        {
            return ringIndex switch
            {
                0 => Color.Lerp(BellUtils.TollCrimson, BellUtils.BurningResonance, t),
                1 => Color.Lerp(BellUtils.BurningResonance, BellUtils.EmberOrange, t),
                _ => Color.Lerp(BellUtils.EmberOrange, BellUtils.HellfireGold, t),
            };
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Impact VFX
            Vector2 hitPos = target.Center;

            // Mini bloom
            BellParticleHandler.SpawnParticle(new BellBloomParticle(hitPos, GetRingColor(RingIndex, 0.5f), 0.8f, 12));

            // Spark burst
            for (int i = 0; i < 4; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(2f, 2f);
                BellParticleHandler.SpawnParticle(new BellEmberParticle(hitPos, vel, 0.2f, 15));
            }

            // Music note on hit
            if (Main.rand.NextBool(2))
            {
                Vector2 noteVel = new Vector2(Main.rand.NextFloat(-1f, 1f), -1.5f);
                BellParticleHandler.SpawnParticle(new BellNoteParticle(hitPos, noteVel, GetRingColor(RingIndex, 0.8f), 0.4f, 35));
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            DrawTrailGlow();
            DrawCore();
            return false;
        }

        private void DrawCore()
        {
            bloomTexture ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftRadialBloom");
            if (!bloomTexture.IsLoaded) return;
            var tex = bloomTexture.Value;

            float alpha = 1f - Projectile.alpha / 255f;
            float lifeRatio = Projectile.timeLeft / 60f;
            Color coreColor = GetRingColor(RingIndex, 0.5f);
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            // Outer glow
            float pulseScale = 0.8f + 0.15f * (float)Math.Sin(Main.GameUpdateCount * 0.2f + RingIndex);
            Main.EntitySpriteDraw(tex, drawPos, null, coreColor * alpha * 0.5f, 0f, tex.Size() / 2f,
                pulseScale, SpriteEffects.None, 0);

            // Inner bright core
            Main.EntitySpriteDraw(tex, drawPos, null, BellUtils.BellWhite * alpha * 0.3f, 0f, tex.Size() / 2f,
                pulseScale * 0.35f, SpriteEffects.None, 0);
        }

        private void DrawTrailGlow()
        {
            bloomTexture ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/PointBloom");
            if (!bloomTexture.IsLoaded) return;
            var tex = bloomTexture.Value;

            float alpha = 1f - Projectile.alpha / 255f;
            Color baseColor = GetRingColor(RingIndex, 0.3f);

            for (int i = 1; i < TrailLength; i++)
            {
                if (trailCache[i] == Vector2.Zero) continue;
                float progress = i / (float)TrailLength;
                float trailAlpha = (1f - progress) * alpha * 0.4f;
                float scale = (1f - progress) * 0.4f;
                Main.EntitySpriteDraw(tex, trailCache[i] - Main.screenPosition, null, baseColor * trailAlpha,
                    0f, tex.Size() / 2f, scale, SpriteEffects.None, 0);
            }
        }

        public override Color? GetAlpha(Color lightColor)
        {
            return Color.White * (1f - Projectile.alpha / 255f);
        }
    }
}
