using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.FoundationWeapons.ExplosionParticlesFoundation
{
    /// <summary>
    /// SparkCarrierProjectile — The projectile fired by the weapon that travels
    /// until it hits an enemy or tile, then spawns a SparkExplosionProjectile
    /// at the impact point.
    ///
    /// Behaviour:
    /// - Flies straight with slight gravity
    /// - Has a glowing tracer effect
    /// - On hit/tile: spawns the explosion spark field and dies
    ///
    /// ai[0] = SparkMode index
    /// </summary>
    public class SparkCarrierProjectile : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.Bullet;

        private const int MaxLifetime = 180;
        private int timer;
        private float seed;

        private SparkMode CurrentMode => (SparkMode)(int)Projectile.ai[0];

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.DrawScreenCheckFluff[Projectile.type] = 400;
        }

        public override void SetDefaults()
        {
            Projectile.width = 12;
            Projectile.height = 12;
            Projectile.friendly = true;
            Projectile.penetrate = 1;
            Projectile.tileCollide = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.timeLeft = MaxLifetime;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 20;
            Projectile.alpha = 0;
        }

        public override void AI()
        {
            if (timer == 0)
                seed = Main.rand.NextFloat(100f);

            timer++;

            // Slight gravity
            Projectile.velocity.Y += 0.12f;

            // Rotation follows velocity
            Projectile.rotation = Projectile.velocity.ToRotation();

            // Lighting
            Color[] colors = EPFTextures.GetModeColors(CurrentMode);
            Lighting.AddLight(Projectile.Center, colors[1].ToVector3() * 0.4f);

            // Trail dust
            if (Main.rand.NextBool(2))
            {
                Color dustColor = colors[Main.rand.Next(3)];
                Dust dust = Dust.NewDustPerfect(
                    Projectile.Center + Main.rand.NextVector2Circular(4f, 4f),
                    DustID.RainbowMk2,
                    -Projectile.velocity * 0.08f + Main.rand.NextVector2Circular(0.8f, 0.8f),
                    newColor: dustColor,
                    Scale: Main.rand.NextFloat(0.2f, 0.4f));
                dust.noGravity = true;
                dust.fadeIn = 0.3f;
            }
        }

        private void SpawnExplosion(Vector2 position)
        {
            // Spawn the spark explosion projectile at impact point
            Projectile.NewProjectile(
                Projectile.GetSource_FromThis(), position, Vector2.Zero,
                ModContent.ProjectileType<SparkExplosionProjectile>(),
                Projectile.damage / 2, Projectile.knockBack * 0.5f,
                Projectile.owner, ai0: (float)CurrentMode);

            SoundEngine.PlaySound(SoundID.Item14 with { Volume = 0.7f, Pitch = 0.2f }, position);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            SpawnExplosion(target.Center);
        }

        public override void OnKill(int timeLeft)
        {
            // Also spawn on tile collision / death
            SpawnExplosion(Projectile.Center);

            // Small dust burst on death
            Color[] colors = EPFTextures.GetModeColors(CurrentMode);
            for (int i = 0; i < 8; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(4f, 4f);
                Color col = colors[Main.rand.Next(3)];
                Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.RainbowMk2, vel,
                    newColor: col, Scale: Main.rand.NextFloat(0.3f, 0.5f));
                dust.noGravity = true;
                dust.fadeIn = 0.3f;
            }
        }

        // =====================================================================
        // RENDERING
        // =====================================================================

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Color[] colors = EPFTextures.GetModeColors(CurrentMode);
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float fadeIn = MathHelper.Clamp(timer / 5f, 0f, 1f);
            float pulse = 0.9f + 0.1f * MathF.Sin((float)Main.timeForVisualEffects * 0.12f + seed);

            // ---- ADDITIVE BLOOM ----
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.TransformationMatrix);

            Texture2D softGlow = EPFTextures.SoftGlow.Value;
            Vector2 glowOrigin = softGlow.Size() / 2f;

            // Outer glow
            sb.Draw(softGlow, drawPos, null, colors[0] * (0.3f * fadeIn * pulse),
                0f, glowOrigin, 0.2f * pulse, SpriteEffects.None, 0f);

            // Core glow
            sb.Draw(softGlow, drawPos, null, colors[2] * (0.5f * fadeIn),
                0f, glowOrigin, 0.08f, SpriteEffects.None, 0f);

            // Velocity streak
            float velAngle = Projectile.velocity.ToRotation();
            sb.Draw(softGlow, drawPos, null, colors[1] * (0.25f * fadeIn),
                velAngle, glowOrigin,
                new Vector2(0.12f, 0.01f), SpriteEffects.None, 0f);

            // ---- RESTORE ----
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.TransformationMatrix);

            return false;
        }

        public override Color? GetAlpha(Color lightColor) => Color.White;
    }
}
