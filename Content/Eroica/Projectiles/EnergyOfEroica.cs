using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Content.Eroica.Projectiles
{
    /// <summary>
    /// Homing energy projectile spawned by Movement I.
    /// Chases the player dealing significant damage.
    /// </summary>
    public class EnergyOfEroica : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 8;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 24;
            Projectile.height = 24;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 150;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.alpha = 50;
            Projectile.light = 0.5f;
        }

        public override void AI()
        {
            // Pink lighting with pulsing intensity
            float pulse = 0.8f + (float)Math.Sin(Projectile.timeLeft * 0.15f) * 0.2f;
            Lighting.AddLight(Projectile.Center, 0.9f * pulse, 0.4f * pulse, 0.6f * pulse);

            // Find target player
            Player target = Main.player[(int)Projectile.ai[0]];

            if (target.active && !target.dead)
            {
                Vector2 direction = target.Center - Projectile.Center;
                float distance = direction.Length();

                if (distance > 0)
                {
                    direction.Normalize();

                    float homingSpeed = 10f;
                    float turnSpeed = 0.06f;

                    if (distance < 300f)
                    {
                        homingSpeed = 12f;
                        turnSpeed = 0.08f;
                    }

                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, direction * homingSpeed, turnSpeed);
                }
            }

            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            // === Gradient glow particles ===
            if (Projectile.timeLeft % 2 == 0)
            {
                float trailProgress = (float)(150 - Projectile.timeLeft) / 150f;
                Color trailColor = Color.Lerp(EroicaPalette.Sakura, EroicaPalette.Gold, trailProgress);

                var glow = new GenericGlowParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(4f, 4f),
                    -Projectile.velocity * 0.12f + Main.rand.NextVector2Circular(1f, 1f),
                    trailColor,
                    0.35f * pulse,
                    18,
                    true
                );
                MagnumParticleHandler.SpawnParticle(glow);
            }

            // === Orbiting star points ===
            if (Projectile.timeLeft % 5 == 0)
            {
                float orbitAngle = Projectile.timeLeft * 0.15f;
                for (int i = 0; i < 3; i++)
                {
                    float angle = orbitAngle + MathHelper.TwoPi * i / 3f;
                    float radius = 10f + (float)Math.Sin(Projectile.timeLeft * 0.1f + i) * 4f;
                    Vector2 starPos = Projectile.Center + angle.ToRotationVector2() * radius;
                    float progress = (float)i / 3f;
                    Color starColor = Color.Lerp(EroicaPalette.Crimson, EroicaPalette.Gold, progress);
                    EroicaVFXLibrary.BloomFlare(starPos, starColor, 0.22f, 10);
                }
            }

            // Sakura petals + flame trail
            EroicaVFXLibrary.SpawnSakuraPetals(Projectile.Center, 2, 12f);
            EroicaVFXLibrary.SpawnFlameTrailDust(Projectile.Center, Projectile.velocity);

            // === Music notes in trail ===
            if (Main.rand.NextBool(8))
            {
                Vector2 noteVel = -Projectile.velocity.SafeNormalize(Vector2.Zero) * 1.5f;
                noteVel = noteVel.RotatedByRandom(0.4f);
                Color noteColor = Color.Lerp(EroicaPalette.Sakura, EroicaPalette.Gold, Main.rand.NextFloat());
                float shimmer = 1f + (float)Math.Sin(Main.GameUpdateCount * 0.15f) * 0.1f;
                EroicaVFXLibrary.SpawnMusicNote(Projectile.Center, noteVel, noteColor, 0.75f * shimmer, 22);
            }

            // Shimmer dust
            if (Main.rand.NextBool(8))
            {
                Dust sparkle = Dust.NewDustDirect(Projectile.Center, 1, 1, DustID.GoldFlame, 0f, 0f, 0, default, 1.2f);
                sparkle.noGravity = true;
                sparkle.velocity = Main.rand.NextVector2Circular(2f, 2f);
            }
        }

        public override void OnKill(int timeLeft)
        {
            EroicaVFXLibrary.DeathHeroicFlash(Projectile.Center, 0.9f);
            Terraria.Audio.SoundEngine.PlaySound(SoundID.Item110, Projectile.position);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Type].Value;
            Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, texture.Height * 0.5f);

            try
            {
                // Switch to additive blending
                EroicaVFXLibrary.BeginEroicaAdditive(spriteBatch);

                // === Multi-layer trail ===
                // Layer 1: Outer scarlet glow
                for (int k = 0; k < Projectile.oldPos.Length; k++)
                {
                    if (Projectile.oldPos[k] == Vector2.Zero) continue;
                    float progress = (float)k / Projectile.oldPos.Length;
                    Color outerColor = Color.Lerp(EroicaPalette.Scarlet, EroicaPalette.Crimson, progress) * (1f - progress) * 0.35f;
                    float scale = Projectile.scale * (1.4f - progress * 0.4f);
                    Vector2 drawPos = Projectile.oldPos[k] - Main.screenPosition + drawOrigin + new Vector2(0f, Projectile.gfxOffY);
                    Main.EntitySpriteDraw(texture, drawPos, null, outerColor, Projectile.oldRot[k], drawOrigin, scale, SpriteEffects.None, 0);
                }

                // Layer 2: Mid sakura glow
                for (int k = 0; k < Projectile.oldPos.Length; k++)
                {
                    if (Projectile.oldPos[k] == Vector2.Zero) continue;
                    float progress = (float)k / Projectile.oldPos.Length;
                    Color midColor = Color.Lerp(EroicaPalette.Sakura, EroicaPalette.Gold, progress) * (1f - progress) * 0.5f;
                    float scale = Projectile.scale * (1.15f - progress * 0.3f);
                    Vector2 drawPos = Projectile.oldPos[k] - Main.screenPosition + drawOrigin + new Vector2(0f, Projectile.gfxOffY);
                    Main.EntitySpriteDraw(texture, drawPos, null, midColor, Projectile.oldRot[k], drawOrigin, scale, SpriteEffects.None, 0);
                }

                // Layer 3: Core gold
                for (int k = 0; k < Projectile.oldPos.Length; k++)
                {
                    if (Projectile.oldPos[k] == Vector2.Zero) continue;
                    float progress = (float)k / Projectile.oldPos.Length;
                    Color coreColor = Color.Lerp(EroicaPalette.Gold, Color.White, progress * 0.3f) * (1f - progress) * 0.65f;
                    float scale = Projectile.scale * (1f - progress * 0.25f);
                    Vector2 drawPos = Projectile.oldPos[k] - Main.screenPosition + drawOrigin + new Vector2(0f, Projectile.gfxOffY);
                    Main.EntitySpriteDraw(texture, drawPos, null, coreColor, Projectile.oldRot[k], drawOrigin, scale, SpriteEffects.None, 0);
                }

                // Main projectile glow layers
                float pulse = 0.85f + 0.15f * (float)Math.Sin(Main.GameUpdateCount * 0.15f);
                Vector2 mainPos = Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);

                Main.EntitySpriteDraw(texture, mainPos, null, EroicaPalette.Scarlet * 0.35f, Projectile.rotation, drawOrigin, Projectile.scale * 1.5f * pulse, SpriteEffects.None, 0);
                Main.EntitySpriteDraw(texture, mainPos, null, EroicaPalette.Sakura * 0.5f, Projectile.rotation, drawOrigin, Projectile.scale * 1.25f * pulse, SpriteEffects.None, 0);
                Main.EntitySpriteDraw(texture, mainPos, null, EroicaPalette.Gold * 0.6f, Projectile.rotation, drawOrigin, Projectile.scale * 1.1f * pulse, SpriteEffects.None, 0);
            }
            finally
            {
                EroicaVFXLibrary.EndEroicaAdditive(spriteBatch);
            }

            // Eroica theme accent
            EroicaVFXLibrary.BeginEroicaAdditive(spriteBatch);
            EroicaVFXLibrary.DrawThemeSakuraAccent(spriteBatch, Projectile.Center, 1f, 0.5f);
            EroicaVFXLibrary.EndEroicaAdditive(spriteBatch);

            return false;
        }

        public override Color? GetAlpha(Color lightColor)
        {
            return new Color(255, 150, 200, 150);
        }
    }
}
