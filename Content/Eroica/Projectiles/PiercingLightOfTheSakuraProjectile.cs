using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using MagnumOpus.Content.Eroica.Weapons.PiercingLightOfTheSakura;

namespace MagnumOpus.Content.Eroica.Projectiles
{
    /// <summary>
    /// Special projectile fired every 10th shot from Piercing Light of the Sakura.
    /// On impact, summons sakura lightning strikes as spiral explosions.
    /// Uses 6×6 sprite sheet animation.  All VFX delegated to PiercingLightOfTheSakuraVFX.
    /// </summary>
    public class PiercingLightOfTheSakuraProjectile : ModProjectile
    {
        // ── Animation ── 6×6 sprite sheet
        private const int FrameColumns = 6;
        private const int FrameRows = 6;
        private const int TotalFrames = 36;
        private const int FrameTime = 2;

        private int frameCounter = 0;
        private int currentFrame = 0;

        // ── AI state accessors ──
        /// <summary>Charge progress (0-1) passed from the item's shotCounter. Drives VFX intensity.</summary>
        private ref float ChargeProgress => ref Projectile.ai[0];
        /// <summary>Tick counter since spawn — used for pulsation and VFX timing.</summary>
        private ref float AgeTimer => ref Projectile.ai[1];

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 14;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 3;
            Projectile.timeLeft = 180;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.alpha = 0;
            Projectile.light = 0f; // Lighting handled by VFX module
            Projectile.extraUpdates = 1;
        }

        public override void AI()
        {
            // Face direction of travel
            Projectile.rotation = Projectile.velocity.ToRotation();

            // Age tracking for VFX progression
            AgeTimer++;

            // Pulsating scale — intensifies with charge
            float scalePulse = (float)Math.Sin(AgeTimer * 0.08f) * 0.06f;
            Projectile.scale = 1.0f + ChargeProgress * 0.15f + scalePulse;

            // Update sprite sheet animation
            frameCounter++;
            if (frameCounter >= FrameTime)
            {
                frameCounter = 0;
                currentFrame++;
                if (currentFrame >= TotalFrames)
                    currentFrame = 0;
            }

            // ═══ ALL VFX delegated to module ═══
            PiercingLightOfTheSakuraVFX.LightningTrailVFX(Projectile);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Delegate hit VFX to module — 8-bolt burst, sakura explosion, bloom cascade
            PiercingLightOfTheSakuraVFX.LightningHitVFX(target.Center);

            // Game logic: spawn 3 sakura lightning spiral explosions
            SpawnLightningExplosions(target.Center);
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            // VFX burst at impact point
            PiercingLightOfTheSakuraVFX.LightningHitVFX(Projectile.Center);

            // Spawn lightning on tile collision too
            SpawnLightningExplosions(Projectile.Center);
            return true;
        }

        public override void OnKill(int timeLeft)
        {
            // Delegate death dissipation VFX to module
            PiercingLightOfTheSakuraVFX.LightningDeathVFX(Projectile.Center);
            SoundEngine.PlaySound(SoundID.Item62 with { Volume = 0.6f }, Projectile.Center);
        }

        /// <summary>
        /// Pure game logic: spawns 3 SakuraLightning explosion projectiles at staggered positions.
        /// All visual effects are handled by LightningHitVFX.
        /// </summary>
        private void SpawnLightningExplosions(Vector2 position)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient) return;

            for (int i = 0; i < 3; i++)
            {
                Vector2 offset = new Vector2(Main.rand.NextFloat(-50f, 50f), Main.rand.NextFloat(-30f, 30f));
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), position + offset, Vector2.Zero,
                    ModContent.ProjectileType<SakuraLightning>(), Projectile.damage, Projectile.knockBack, Projectile.owner);
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;

            // Calculate sprite sheet frame
            int frameWidth = texture.Width / FrameColumns;
            int frameHeight = texture.Height / FrameRows;
            int column = currentFrame % FrameColumns;
            int row = currentFrame / FrameColumns;
            Rectangle sourceRect = new Rectangle(column * frameWidth, row * frameHeight, frameWidth, frameHeight);
            Vector2 drawOrigin = new Vector2(frameWidth / 2f, frameHeight / 2f);

            // Delegate all rendering to VFX module with sprite sheet info
            return PiercingLightOfTheSakuraVFX.DrawLightningProjectile(
                Main.spriteBatch, Projectile, sourceRect, drawOrigin);
        }
    }
}
