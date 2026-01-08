using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Eroica.Enemies
{
    /// <summary>
    /// Stolen Valor Flame - Black and red flaming projectile shot by Stolen Valor Minions.
    /// Purely dust-based visual effect with no texture.
    /// </summary>
    public class StolenValorFlame : ModProjectile
    {
        // Use a vanilla texture since we're dust-only (won't actually be drawn)
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.Bullet;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 300;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.alpha = 255; // Invisible sprite - dust only
        }

        public override void AI()
        {
            // Rotation based on velocity
            Projectile.rotation = Projectile.velocity.ToRotation();

            // Dark red glow with hint of gold
            Lighting.AddLight(Projectile.Center, 0.6f, 0.15f, 0.1f);

            // Core - Dense black flames
            if (Main.rand.NextBool(1))
            {
                Dust blackFlame = Dust.NewDustDirect(Projectile.Center - new Vector2(8, 8), 16, 16, DustID.Torch, 0f, 0f, 120, Color.Black, 1.8f);
                blackFlame.noGravity = true;
                blackFlame.velocity = Projectile.velocity * 0.15f + Main.rand.NextVector2Circular(0.5f, 0.5f);
                blackFlame.fadeIn = 1.2f;
            }

            // Mid layer - Dark red flames
            if (Main.rand.NextBool(1))
            {
                Dust redFlame = Dust.NewDustDirect(Projectile.Center - new Vector2(10, 10), 20, 20, DustID.Torch, 0f, 0f, 100, new Color(180, 0, 0), 1.5f);
                redFlame.noGravity = true;
                redFlame.velocity = Projectile.velocity * 0.2f + Main.rand.NextVector2Circular(0.8f, 0.8f);
                redFlame.fadeIn = 1.0f;
            }

            // Outer layer - Bright red/orange flames
            if (Main.rand.NextBool(2))
            {
                Dust outerFlame = Dust.NewDustDirect(Projectile.Center - new Vector2(12, 12), 24, 24, DustID.Torch, 0f, 0f, 80, new Color(255, 50, 0), 1.2f);
                outerFlame.noGravity = true;
                outerFlame.velocity = Projectile.velocity * 0.25f + Main.rand.NextVector2Circular(1.2f, 1.2f);
            }

            // Golden sparkles - triumphant but corrupted
            if (Main.rand.NextBool(4))
            {
                Dust sparkle = Dust.NewDustDirect(Projectile.Center - new Vector2(6, 6), 12, 12, DustID.GoldFlame, 0f, 0f, 0, Color.Gold, 0.9f);
                sparkle.noGravity = true;
                sparkle.velocity = Main.rand.NextVector2Circular(2f, 2f);
                sparkle.fadeIn = 0.8f;
            }

            // Enchanted gold shimmer for extra flair
            if (Main.rand.NextBool(6))
            {
                Dust gold = Dust.NewDustDirect(Projectile.Center - new Vector2(8, 8), 16, 16, DustID.Enchanted_Gold, 0f, 0f, 100, default, 0.7f);
                gold.noGravity = true;
                gold.velocity = Projectile.velocity * 0.1f + Main.rand.NextVector2Circular(1f, 1f);
            }

            // Black smoke trails
            if (Main.rand.NextBool(3))
            {
                Dust smoke = Dust.NewDustDirect(Projectile.Center - new Vector2(10, 10), 20, 20, DustID.Smoke, 0f, 0f, 150, new Color(40, 40, 40), 1.3f);
                smoke.velocity = Projectile.velocity * 0.1f + Main.rand.NextVector2Circular(0.5f, 0.5f);
                smoke.alpha = 100;
            }

            // Trail effect - leave burning embers
            if (Main.rand.NextBool(3))
            {
                Dust trail = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Torch, 0f, 0f, 100, Color.DarkRed, 1.0f);
                trail.noGravity = true;
                trail.velocity = Main.rand.NextVector2Circular(0.5f, 0.5f);
                trail.fadeIn = 0.5f;
            }

            // Slight homing toward nearest player
            Player target = Main.player[Player.FindClosest(Projectile.position, Projectile.width, Projectile.height)];
            if (target != null && target.active && !target.dead)
            {
                Vector2 toTarget = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, toTarget * Projectile.velocity.Length(), 0.03f);
            }
        }

        public override void OnKill(int timeLeft)
        {
            // Dramatic explosion effect
            SoundEngine.PlaySound(SoundID.Item14, Projectile.position);

            // Black core explosion
            for (int i = 0; i < 15; i++)
            {
                Dust blackBurst = Dust.NewDustDirect(Projectile.Center - new Vector2(15, 15), 30, 30, DustID.Torch, 0f, 0f, 120, Color.Black, 2.2f);
                blackBurst.noGravity = true;
                blackBurst.velocity = Main.rand.NextVector2Circular(8f, 8f);
                blackBurst.fadeIn = 1.5f;
            }

            // Red flame burst
            for (int i = 0; i < 20; i++)
            {
                Dust redBurst = Dust.NewDustDirect(Projectile.Center - new Vector2(20, 20), 40, 40, DustID.Torch, 0f, 0f, 100, new Color(200, 0, 0), 1.8f);
                redBurst.noGravity = true;
                redBurst.velocity = Main.rand.NextVector2Circular(10f, 10f);
            }

            // Golden sparkle explosion
            for (int i = 0; i < 12; i++)
            {
                Dust goldBurst = Dust.NewDustDirect(Projectile.Center - new Vector2(12, 12), 24, 24, DustID.GoldFlame, 0f, 0f, 0, Color.Gold, 1.2f);
                goldBurst.noGravity = true;
                goldBurst.velocity = Main.rand.NextVector2Circular(6f, 6f);
            }

            // Enchanted gold shimmer ring
            for (int i = 0; i < 8; i++)
            {
                Dust shimmer = Dust.NewDustDirect(Projectile.Center - new Vector2(10, 10), 20, 20, DustID.Enchanted_Gold, 0f, 0f, 100, default, 1.0f);
                shimmer.noGravity = true;
                shimmer.velocity = Main.rand.NextVector2Circular(5f, 5f);
            }

            // Black smoke cloud
            for (int i = 0; i < 10; i++)
            {
                Dust smoke = Dust.NewDustDirect(Projectile.Center - new Vector2(25, 25), 50, 50, DustID.Smoke, 0f, 0f, 150, new Color(50, 50, 50), 1.8f);
                smoke.velocity = Main.rand.NextVector2Circular(4f, 4f);
            }
        }
    }
}
