using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.Eroica.Weapons.FuneralPrayer;

namespace MagnumOpus.Content.Eroica.Projectiles
{
    /// <summary>
    /// Funeral Prayer projectile — large flaming bolt with red/gold flames using 6×6 sprite sheet.
    /// All VFX delegated to FuneralPrayerVFX module.
    /// </summary>
    public class FuneralPrayerProjectile : ModProjectile
    {
        private const int FrameCount = 36;
        private const int FramesPerRow = 6;
        private const int FrameRows = 6;
        private const int AnimationSpeed = 2;

        private int frameCounter = 0;
        private int currentFrame = 0;

        /// <summary>Tick counter since spawn — drives pulsation and VFX timing.</summary>
        private ref float AgeTimer => ref Projectile.ai[1];

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 12;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 48;
            Projectile.height = 48;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 3;
            Projectile.timeLeft = 180;
            Projectile.alpha = 0;
            Projectile.light = 0f; // Lighting handled by VFX module
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }

        public override void AI()
        {
            AgeTimer++;

            // Pulsating scale — the funeral flame breathes
            float scalePulse = (float)Math.Sin(AgeTimer * 0.07f) * 0.05f;
            Projectile.scale = 1.0f + scalePulse;

            // Animate through 6×6 sprite sheet
            frameCounter++;
            if (frameCounter >= AnimationSpeed)
            {
                frameCounter = 0;
                currentFrame = (currentFrame + 1) % FrameCount;
            }

            // Rotation follows velocity
            Projectile.rotation = Projectile.velocity.ToRotation();

            // ═══ VFX delegated to module ═══
            FuneralPrayerVFX.BeamTrailFrame(Projectile);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;

            int frameWidth = texture.Width / FramesPerRow;
            int frameHeight = texture.Height / FrameRows;
            int frameX = currentFrame % FramesPerRow;
            int frameY = currentFrame / FramesPerRow;
            Rectangle sourceRect = new Rectangle(frameX * frameWidth, frameY * frameHeight, frameWidth, frameHeight);
            Vector2 origin = new Vector2(frameWidth / 2f, frameHeight / 2f);
            SpriteBatch sb = Main.spriteBatch;
            Vector2 projScreen = Projectile.Center - Main.screenPosition;

            // {A=0} afterimage trail — FuneralShadow to DirgeRed gradient
            for (int k = 0; k < Projectile.oldPos.Length; k++)
            {
                if (Projectile.oldPos[k] == Vector2.Zero) continue;
                Vector2 drawPos = Projectile.oldPos[k] - Main.screenPosition + new Vector2(Projectile.width / 2f, Projectile.height / 2f);
                float progress = (Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length;

                Color trailColor = (Color.Lerp(new Color(30, 10, 20), EroicaPalette.Crimson, progress) * progress) with { A = 0 };
                float scale = Projectile.scale * (0.5f + progress * 0.5f);
                sb.Draw(texture, drawPos, sourceRect, trailColor, Projectile.oldRot[k], origin, scale, SpriteEffects.None, 0f);
            }

            // 3-layer {A=0} bloom stack — mourning palette
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.08f) * 0.08f + 1f;
            sb.Draw(texture, projScreen, sourceRect,
                (new Color(180, 80, 40) with { A = 0 }) * 0.35f, Projectile.rotation, origin,
                Projectile.scale * 1.25f * pulse, SpriteEffects.None, 0f);
            sb.Draw(texture, projScreen, sourceRect,
                (EroicaPalette.OrangeGold with { A = 0 }) * 0.30f, Projectile.rotation, origin,
                Projectile.scale * 1.12f * pulse, SpriteEffects.None, 0f);
            sb.Draw(texture, projScreen, sourceRect,
                (new Color(255, 250, 240) with { A = 0 }) * 0.22f, Projectile.rotation, origin,
                Projectile.scale * 1.04f * pulse, SpriteEffects.None, 0f);

            // Main sprite
            sb.Draw(texture, projScreen, sourceRect,
                new Color(255, 240, 225, 210), Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0f);

            return false;
        }

        public override void OnKill(int timeLeft)
        {
            // Delegate death VFX to module — ember scatter, ash, final flare
            FuneralPrayerVFX.BeamDeathVFX(Projectile.Center);
        }
    }
}
