using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.FoundationWeapons.SmokeFoundation
{
    /// <summary>
    /// SmokeCarrierProjectile — The projectile fired by SmokeFoundation that
    /// travels until it hits an enemy or tile, then spawns a SmokeRingProjectile
    /// at the impact point.
    ///
    /// Behaviour:
    /// - Flies straight with slight gravity
    /// - Glowing tracer effect with style-colored bloom
    /// - On hit/tile: spawns the smoke ring explosion and dies
    ///
    /// ai[0] = SmokeCloudStyle index
    /// </summary>
    public class SmokeCarrierProjectile : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.Bullet;

        private const int MaxLifetime = 180;
        private int timer;
        private float seed;

        private SmokeCloudStyle CurrentStyle => (SmokeCloudStyle)(int)Projectile.ai[0];

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
            Color[] colors = SKFTextures.GetStyleColors(CurrentStyle);
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

        private void SpawnSmokeRing(Vector2 position)
        {
            Projectile.NewProjectile(
                Projectile.GetSource_FromThis(), position, Vector2.Zero,
                ModContent.ProjectileType<SmokeRingProjectile>(),
                Projectile.damage / 2, Projectile.knockBack * 0.5f,
                Projectile.owner, ai0: (float)CurrentStyle);

            SoundEngine.PlaySound(SoundID.Item14 with { Volume = 0.6f, Pitch = -0.3f }, position);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            SpawnSmokeRing(target.Center);
        }

        public override void OnKill(int timeLeft)
        {
            SpawnSmokeRing(Projectile.Center);

            // Small dust burst on death
            Color[] colors = SKFTextures.GetStyleColors(CurrentStyle);
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
            Color[] colors = SKFTextures.GetStyleColors(CurrentStyle);
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float fadeIn = MathHelper.Clamp(timer / 5f, 0f, 1f);
            float pulse = 0.9f + 0.1f * MathF.Sin((float)Main.timeForVisualEffects * 0.12f + seed);

            // ---- ADDITIVE BLOOM ----
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.TransformationMatrix);

            Texture2D softGlow = SKFTextures.SoftGlow.Value;
            Vector2 glowOrigin = softGlow.Size() / 2f;

            // Outer glow
            sb.Draw(softGlow, drawPos, null, colors[0] * (0.3f * fadeIn * pulse),
                0f, glowOrigin, 0.2f * pulse, SpriteEffects.None, 0f);

            // Core glow
            sb.Draw(softGlow, drawPos, null, colors[1] * (0.5f * fadeIn),
                0f, glowOrigin, 0.08f, SpriteEffects.None, 0f);

            // Velocity streak
            float velAngle = Projectile.velocity.ToRotation();
            Texture2D starFlare = SKFTextures.StarFlare.Value;
            sb.Draw(starFlare, drawPos, null, colors[1] * (0.3f * fadeIn),
                velAngle, starFlare.Size() / 2f, new Vector2(0.15f, 0.05f) * pulse,
                SpriteEffects.None, 0f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.TransformationMatrix);

            return false;
        }
    }
}
