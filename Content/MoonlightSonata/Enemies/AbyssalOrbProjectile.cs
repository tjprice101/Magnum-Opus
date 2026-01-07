using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.MoonlightSonata.Enemies
{
    /// <summary>
    /// Abyssal Orb - A powerful orb projectile fired by Abyssal Moon Lurker.
    /// </summary>
    public class AbyssalOrbProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Content/MoonlightSonata/Enemies/Projectile1";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 15;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 28;
            Projectile.height = 28;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.penetrate = 2;
            Projectile.timeLeft = 300;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.alpha = 50;
            Projectile.light = 0.6f;
        }

        public override void AI()
        {
            Projectile.rotation += 0.18f;

            // Slight homing
            if (Projectile.ai[0] < 45)
            {
                Player target = Main.player[Player.FindClosest(Projectile.Center, 1, 1)];
                if (target.active && !target.dead)
                {
                    Vector2 direction = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, direction * Projectile.velocity.Length(), 0.025f);
                }
            }
            Projectile.ai[0]++;

            Lighting.AddLight(Projectile.Center, 0.7f, 0.6f, 0.9f);

            // White shimmer particles
            if (Main.rand.NextBool(2))
            {
                Dust shimmer = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.SparksMech, 0f, 0f, 0, Color.White, 1f);
                shimmer.noGravity = true;
                shimmer.velocity *= 0.2f;
            }

            // Purple/blue trail
            if (Main.rand.NextBool(2))
            {
                int dustType = Main.rand.NextBool() ? DustID.PurpleTorch : DustID.IceTorch;
                Dust trail = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, dustType, 0f, 0f, 100, default, 1.3f);
                trail.noGravity = true;
                trail.velocity *= 0.2f;
            }
        }

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 18; i++)
            {
                int dustType = Main.rand.NextBool(3) ? DustID.SparksMech : (Main.rand.NextBool() ? DustID.PurpleTorch : DustID.IceTorch);
                Dust explosion = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, dustType, 0f, 0f, 100, dustType == DustID.SparksMech ? Color.White : default, 1.6f);
                explosion.noGravity = true;
                explosion.velocity = Main.rand.NextVector2Circular(6f, 6f);
            }

            Terraria.Audio.SoundEngine.PlaySound(SoundID.Item10, Projectile.Center);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            Vector2 origin = new Vector2(texture.Width / 2, texture.Height / 2);

            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                Vector2 drawPos = Projectile.oldPos[i] + Projectile.Size / 2 - Main.screenPosition;
                float trailAlpha = (float)(Projectile.oldPos.Length - i) / Projectile.oldPos.Length;
                Color trailColor = i % 2 == 0 ? new Color(255, 255, 255) : new Color(180, 120, 220);
                trailColor *= trailAlpha * 0.5f;
                float trailScale = Projectile.scale * (0.5f + 0.5f * trailAlpha);

                Main.EntitySpriteDraw(texture, drawPos, null, trailColor, Projectile.oldRot[i], origin, trailScale, SpriteEffects.None, 0);
            }

            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.1f) * 0.2f + 0.8f;
            Color glowColor = Color.White * pulse * 0.5f;
            
            for (int i = 0; i < 4; i++)
            {
                Vector2 offset = new Vector2(4f, 0f).RotatedBy(MathHelper.TwoPi * i / 4);
                Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition + offset, null, glowColor, Projectile.rotation, origin, Projectile.scale * 1.1f, SpriteEffects.None, 0);
            }

            return true;
        }

        public override Color? GetAlpha(Color lightColor)
        {
            return new Color(255, 255, 255, 200);
        }
    }
}
