using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.MoonlightSonata.Enemies
{
    /// <summary>
    /// Moonlight Flare - A glowing orb projectile fired by Lunus.
    /// Accurate single shot that homes slightly.
    /// </summary>
    public class MoonlightFlareProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Content/MoonlightSonata/Enemies/MoonlightFlare";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 12;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 24;
            Projectile.height = 24;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 300;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.alpha = 50;
            Projectile.light = 0.5f;
        }

        public override void AI()
        {
            // Rotation
            Projectile.rotation += 0.15f;

            // Slight homing towards nearest player
            if (Projectile.ai[0] < 60) // Only home for first second
            {
                Player target = Main.player[Player.FindClosest(Projectile.Center, 1, 1)];
                if (target.active && !target.dead)
                {
                    Vector2 direction = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, direction * Projectile.velocity.Length(), 0.02f);
                }
            }
            Projectile.ai[0]++;

            // Lighting
            Lighting.AddLight(Projectile.Center, 0.6f, 0.5f, 0.9f);

            // Trail particles - purple and light blue
            if (Main.rand.NextBool(2))
            {
                int dustType = Main.rand.NextBool() ? DustID.PurpleTorch : DustID.IceTorch;
                Dust trail = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, dustType, 0f, 0f, 100, default, 1.2f);
                trail.noGravity = true;
                trail.velocity *= 0.2f;
            }

            // Sparkle particles - blue fairy
            if (Main.rand.NextBool(5))
            {
                Dust sparkle = Dust.NewDustDirect(Projectile.Center, 1, 1, DustID.BlueFairy, 0f, 0f, 0, default, 0.8f);
                sparkle.noGravity = true;
                sparkle.velocity = Main.rand.NextVector2Circular(2f, 2f);
            }
        }

        public override void OnKill(int timeLeft)
        {
            // Explosion particles - purple and light blue
            for (int i = 0; i < 12; i++)
            {
                int dustType = Main.rand.NextBool() ? DustID.PurpleTorch : DustID.IceTorch;
                Dust explosion = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, dustType, 0f, 0f, 100, default, 1.5f);
                explosion.noGravity = true;
                explosion.velocity = Main.rand.NextVector2Circular(5f, 5f);
            }

            Terraria.Audio.SoundEngine.PlaySound(SoundID.Item10, Projectile.Center);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            // Draw trail
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            Vector2 origin = new Vector2(texture.Width / 2, texture.Height / 2);

            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                Vector2 drawPos = Projectile.oldPos[i] + Projectile.Size / 2 - Main.screenPosition;
                float trailAlpha = (float)(Projectile.oldPos.Length - i) / Projectile.oldPos.Length;
                Color trailColor = new Color(180, 120, 220) * trailAlpha * 0.5f;
                float trailScale = Projectile.scale * (0.5f + 0.5f * trailAlpha);

                Main.EntitySpriteDraw(texture, drawPos, null, trailColor, Projectile.oldRot[i], origin, trailScale, SpriteEffects.None, 0);
            }

            // Glow effect
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.1f) * 0.2f + 0.8f;
            Color glowColor = new Color(150, 100, 200) * pulse * 0.6f;
            
            for (int i = 0; i < 4; i++)
            {
                Vector2 offset = new Vector2(3f, 0f).RotatedBy(MathHelper.TwoPi * i / 4);
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
