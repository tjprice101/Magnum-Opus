using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.FoundationWeapons.AttackFoundation
{
    /// <summary>
    /// RangerShotProjectile — Mode 5: Ranger weapon that fires piercing bolts
    /// with special muzzle flash and tracer VFX.
    ///
    /// Behaviour:
    /// - Fast-moving piercing projectile with a bright tracer trail
    /// - Muzzle flash bloom spawns at origin on creation
    /// - Bright glowing bolt body with stretched velocity trail
    /// - Leaves afterimage trail of fading bloom points
    /// - On hit: small directional burst VFX
    /// - Pierces 3 enemies before dying
    ///
    /// ai[0] = mode index (always 4 for RangerShot)
    /// </summary>
    public class RangerShotProjectile : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.RainbowCrystalExplosion;

        private const int MaxLifetime = 180;
        private const float TrailLength = 8;  // Number of afterimage positions stored

        private int timer;
        private float seed;
        private bool muzzleFlashSpawned;
        private Vector2[] trailPositions = new Vector2[(int)TrailLength];
        private int trailIndex;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.DrawScreenCheckFluff[Projectile.type] = 400;
        }

        public override void SetDefaults()
        {
            Projectile.width = 8;
            Projectile.height = 8;
            Projectile.friendly = true;
            Projectile.penetrate = 3;
            Projectile.tileCollide = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.timeLeft = MaxLifetime;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 8;
            Projectile.alpha = 0;
            Projectile.extraUpdates = 1; // Moves twice per frame for smooth look
        }

        public override void AI()
        {
            if (timer == 0)
            {
                seed = Main.rand.NextFloat(100f);

                // Initialize trail positions
                for (int i = 0; i < (int)TrailLength; i++)
                    trailPositions[i] = Projectile.Center;
            }

            timer++;

            // Muzzle flash at spawn
            if (!muzzleFlashSpawned)
            {
                muzzleFlashSpawned = true;
                SpawnMuzzleFlash();
            }

            // Store trail position
            trailPositions[trailIndex % (int)TrailLength] = Projectile.Center;
            trailIndex++;

            // Rotation follows velocity
            Projectile.rotation = Projectile.velocity.ToRotation();

            // Lighting
            Color[] colors = AFTextures.GetModeColors(AttackMode.RangerShot);
            Lighting.AddLight(Projectile.Center, colors[1].ToVector3() * 0.5f);

            // Sparse tracer dust
            if (Main.rand.NextBool(3))
            {
                Color dustColor = colors[Main.rand.Next(colors.Length)];
                Dust dust = Dust.NewDustPerfect(
                    Projectile.Center + Main.rand.NextVector2Circular(3f, 3f),
                    DustID.RainbowMk2,
                    -Projectile.velocity * 0.05f,
                    newColor: dustColor,
                    Scale: Main.rand.NextFloat(0.2f, 0.4f));
                dust.noGravity = true;
                dust.fadeIn = 0.2f;
            }
        }

        private void SpawnMuzzleFlash()
        {
            Color[] colors = AFTextures.GetModeColors(AttackMode.RangerShot);
            Vector2 flashPos = Projectile.Center;

            // Radial muzzle flash dust
            for (int i = 0; i < 10; i++)
            {
                float angle = Projectile.velocity.ToRotation() + Main.rand.NextFloat(-0.8f, 0.8f);
                float speed = Main.rand.NextFloat(2f, 6f);
                Vector2 vel = angle.ToRotationVector2() * speed;
                Color col = colors[Main.rand.Next(colors.Length)];

                Dust dust = Dust.NewDustPerfect(flashPos, DustID.RainbowMk2, vel,
                    newColor: col, Scale: Main.rand.NextFloat(0.3f, 0.7f));
                dust.noGravity = true;
                dust.fadeIn = 0.3f;
            }

            // Forward cone flash dust
            for (int i = 0; i < 5; i++)
            {
                float angle = Projectile.velocity.ToRotation() + Main.rand.NextFloat(-0.2f, 0.2f);
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 10f);

                Dust dust = Dust.NewDustPerfect(flashPos, DustID.RainbowMk2, vel,
                    newColor: colors[2], Scale: Main.rand.NextFloat(0.4f, 0.8f));
                dust.noGravity = true;
                dust.fadeIn = 0.4f;
            }

            SoundEngine.PlaySound(SoundID.Item12 with { Volume = 0.4f, Pitch = 0.5f }, flashPos);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Color[] colors = AFTextures.GetModeColors(AttackMode.RangerShot);

            // Directional impact burst
            float hitAngle = Projectile.velocity.ToRotation();
            for (int i = 0; i < 8; i++)
            {
                float angle = hitAngle + MathHelper.Pi + Main.rand.NextFloat(-0.6f, 0.6f);
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 6f);
                Color col = colors[Main.rand.Next(colors.Length)];

                Dust dust = Dust.NewDustPerfect(target.Center, DustID.RainbowMk2, vel,
                    newColor: col, Scale: Main.rand.NextFloat(0.3f, 0.6f));
                dust.noGravity = true;
                dust.fadeIn = 0.3f;
            }
        }

        public override void OnKill(int timeLeft)
        {
            Color[] colors = AFTextures.GetModeColors(AttackMode.RangerShot);

            for (int i = 0; i < 10; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(4f, 4f);
                Color col = colors[Main.rand.Next(colors.Length)];
                Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.RainbowMk2, vel,
                    newColor: col, Scale: Main.rand.NextFloat(0.3f, 0.6f));
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
            try
            {
            Color[] colors = AFTextures.GetModeColors(AttackMode.RangerShot);
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            float fadeIn = MathHelper.Clamp(timer / 4f, 0f, 1f);
            float pulse = 0.9f + 0.1f * MathF.Sin((float)Main.timeForVisualEffects * 0.15f + seed);

            // ---- ADDITIVE PASS ----
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.TransformationMatrix);

            Texture2D softGlow = AFTextures.SoftGlow.Value;
            Vector2 glowOrigin = softGlow.Size() / 2f;

            // ---- AFTERIMAGE TRAIL ----
            int trailCount = Math.Min(trailIndex, (int)TrailLength);
            for (int i = 0; i < trailCount; i++)
            {
                int idx = (trailIndex - 1 - i) % (int)TrailLength;
                if (idx < 0) idx += (int)TrailLength;
                Vector2 trailPos = trailPositions[idx] - Main.screenPosition;

                float trailAlpha = (1f - (i / TrailLength)) * 0.4f * fadeIn;
                float trailScale = (1f - (i / TrailLength)) * 0.08f;

                sb.Draw(softGlow, trailPos, null, colors[1] * trailAlpha,
                    0f, glowOrigin, trailScale, SpriteEffects.None, 0f);
            }

            // ---- VELOCITY STREAK ----
            float velAngle = Projectile.velocity.ToRotation();
            float speed = Projectile.velocity.Length();
            float streakLength = MathHelper.Clamp(speed * 0.005f, 0.02f, 0.08f);

            // Outer streak (SoftGlow 1024px — target ~80×6px)
            sb.Draw(softGlow, drawPos, null, colors[0] * (0.4f * fadeIn),
                velAngle, glowOrigin,
                new Vector2(streakLength, 0.006f), SpriteEffects.None, 0f);

            // Core streak
            sb.Draw(softGlow, drawPos, null, colors[2] * (0.6f * fadeIn),
                velAngle, glowOrigin,
                new Vector2(streakLength * 0.6f, 0.003f), SpriteEffects.None, 0f);

            // ---- BOLT BODY ----
            // Outer glow (SoftGlow — target ~30px)
            sb.Draw(softGlow, drawPos, null, colors[0] * (0.35f * fadeIn * pulse),
                0f, glowOrigin, 0.03f * pulse, SpriteEffects.None, 0f);

            // Core (target ~15px)
            sb.Draw(softGlow, drawPos, null, colors[2] * (0.7f * fadeIn),
                0f, glowOrigin, 0.015f, SpriteEffects.None, 0f);

            // Star flare at bolt head (StarFlare 1024px — target ~20px)
            Texture2D starFlare = AFTextures.StarFlare.Value;
            Vector2 flareOrigin = starFlare.Size() / 2f;
            sb.Draw(starFlare, drawPos, null, colors[1] * (0.2f * fadeIn),
                velAngle, flareOrigin, 0.02f, SpriteEffects.None, 0f);

            // ---- MUZZLE FLASH BLOOM (first few frames only) ----
            if (timer < 6)
            {
                float flashAlpha = (1f - timer / 6f) * 0.7f;
                Vector2 flashPos = trailPositions[0] - Main.screenPosition;

                // Large muzzle bloom (SoftGlow 1024px — target ~60px)
                sb.Draw(softGlow, flashPos, null, colors[2] * flashAlpha,
                    0f, glowOrigin, 0.06f, SpriteEffects.None, 0f);

                // Directional flash (target ~60×12px)
                sb.Draw(softGlow, flashPos, null, colors[1] * (flashAlpha * 0.6f),
                    velAngle, glowOrigin,
                    new Vector2(0.06f, 0.012f), SpriteEffects.None, 0f);

                // Lens flare at muzzle (LensFlare 1024px — target ~40px)
                Texture2D lensFlare = AFTextures.LensFlare.Value;
                Vector2 lfOrigin = lensFlare.Size() / 2f;
                sb.Draw(lensFlare, flashPos, null, colors[2] * (flashAlpha * 0.4f),
                    velAngle, lfOrigin, 0.04f, SpriteEffects.None, 0f);
            }

            // ---- RESTORE ----
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null,
                Main.GameViewMatrix.TransformationMatrix);

            }
            catch { }
            finally
            {
                try { sb.End(); } catch { }
                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                    DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            }

            return false;
        }

        public override Color? GetAlpha(Color lightColor) => Color.White;
    }
}
