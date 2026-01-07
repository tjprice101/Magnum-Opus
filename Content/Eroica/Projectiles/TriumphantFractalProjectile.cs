using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;

namespace MagnumOpus.Content.Eroica.Projectiles
{
    /// <summary>
    /// Triumphant Fractal projectile with massive explosion.
    /// </summary>
    public class TriumphantFractalProjectile : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 12;
            ProjectileID.Sets.TrailingMode[Type] = 2;
            Main.projFrames[Type] = 1;
        }

        public override void SetDefaults()
        {
            Projectile.width = 32;
            Projectile.height = 32;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 200;
            Projectile.alpha = 0;
            Projectile.light = 1f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
        }

        public override void AI()
        {
            // Set rotation to match velocity direction (no spinning)
            Projectile.rotation = Projectile.velocity.ToRotation();

            // Intense red and black flame trail
            for (int i = 0; i < 2; i++)
            {
                Dust flame = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height,
                    DustID.RedTorch, 0f, 0f, 100, default, 1.8f);
                flame.noGravity = true;
                flame.velocity = Projectile.velocity * 0.3f;
            }

            if (Main.rand.NextBool(2))
            {
                Dust smoke = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height,
                    DustID.Smoke, 0f, 0f, 100, Color.Black, 1.3f);
                smoke.noGravity = true;
                smoke.velocity *= 0.4f;
            }

            Lighting.AddLight(Projectile.Center, 0.9f, 0.2f, 0.1f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            CreateMassiveExplosion();
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            CreateMassiveExplosion();
            return true;
        }

        public override void OnKill(int timeLeft)
        {
            CreateMassiveExplosion();
        }

        private void CreateMassiveExplosion()
        {
            SoundEngine.PlaySound(SoundID.DD2_ExplosiveTrapExplode, Projectile.position);
            SoundEngine.PlaySound(SoundID.DD2_BetsyFireballImpact, Projectile.position);

            // MASSIVE red flames
            for (int i = 0; i < 60; i++)
            {
                Dust explosion = Dust.NewDustDirect(Projectile.position - new Vector2(30, 30),
                    Projectile.width + 60, Projectile.height + 60,
                    DustID.RedTorch, 0f, 0f, 100, default, 3.0f);
                explosion.noGravity = true;
                explosion.velocity = Main.rand.NextVector2Circular(12f, 12f);
            }

            // MASSIVE black smoke
            for (int i = 0; i < 40; i++)
            {
                Dust smoke = Dust.NewDustDirect(Projectile.position - new Vector2(30, 30),
                    Projectile.width + 60, Projectile.height + 60,
                    DustID.Smoke, 0f, 0f, 100, Color.Black, 2.5f);
                smoke.noGravity = true;
                smoke.velocity = Main.rand.NextVector2Circular(10f, 10f);
            }

            // Fire particles
            for (int i = 0; i < 30; i++)
            {
                Dust fire = Dust.NewDustDirect(Projectile.position - new Vector2(20, 20),
                    Projectile.width + 40, Projectile.height + 40,
                    DustID.Torch, 0f, 0f, 100, default, 2.0f);
                fire.noGravity = true;
                fire.velocity = Main.rand.NextVector2Circular(8f, 8f);
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
            Vector2 origin = texture.Size() / 2f;

            // Draw glowing trail
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;

                Vector2 drawPos = Projectile.oldPos[i] - Main.screenPosition + Projectile.Size / 2f;
                Color trailColor = new Color(255, 100, 50, 0) * ((float)(Projectile.oldPos.Length - i) / Projectile.oldPos.Length) * 0.8f;

                spriteBatch.Draw(texture, drawPos, null, trailColor, Projectile.oldRot[i], origin,
                    Projectile.scale * 0.9f, SpriteEffects.None, 0f);
            }

            return true;
        }
    }
}
