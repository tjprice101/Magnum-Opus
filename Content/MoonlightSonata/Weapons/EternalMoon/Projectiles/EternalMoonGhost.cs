using System;
using MagnumOpus.Content.MoonlightSonata.Weapons.EternalMoon.Particles;
using MagnumOpus.Content.MoonlightSonata.Weapons.EternalMoon.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.MoonlightSonata.Weapons.EternalMoon.Projectiles
{
    /// <summary>
    /// Ghost Reflection — a translucent delayed echo of the player's swing.
    /// Spawned at Half Moon phase (phase 2+), appearing as spectral copies of the blade
    /// offset at ±30° from the main swing. Deals reduced damage and has ethereal VFX.
    /// </summary>
    public class EternalMoonGhost : ModProjectile
    {
        private const float BladeLength = 140f;
        private const int GhostSwingTime = 40;

        public Player Owner => Main.player[Projectile.owner];
        public int LunarPhase => (int)Projectile.ai[0];
        public int GhostSide => (int)Projectile.ai[1]; // -1 or +1

        private float _swingProgress;

        public override string Texture => "MagnumOpus/Content/MoonlightSonata/Weapons/EternalMoon/EternalMoon";

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 70;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.MeleeNoSpeed;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = GhostSwingTime;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 30;
            Projectile.Opacity = 0.35f;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float _ = 0f;
            float angle = Projectile.velocity.ToRotation() + GetSwingAngle(_swingProgress) * GhostSide;
            Vector2 dir = angle.ToRotationVector2();
            Vector2 start = Owner.MountedCenter;
            Vector2 end = start + dir * BladeLength * Projectile.scale;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), start, end, 20f, ref _);
        }

        private float GetSwingAngle(float progress)
        {
            // Ghost swings use a simple sinusoidal arc
            return MathHelper.PiOver2 * 1.4f * ((float)Math.Sin(progress * MathHelper.Pi) * 2f - 1f);
        }

        public override void AI()
        {
            _swingProgress = 1f - (Projectile.timeLeft / (float)GhostSwingTime);

            // Anchor to owner
            Projectile.Center = Owner.MountedCenter;

            // Ethereal dust from ghost blade tip
            if (Main.rand.NextBool(3))
            {
                float angle = Projectile.velocity.ToRotation() + GetSwingAngle(_swingProgress) * GhostSide;
                Vector2 tipPos = Owner.MountedCenter + angle.ToRotationVector2() * BladeLength;
                Dust d = Dust.NewDustPerfect(tipPos, DustID.PurpleTorch, Main.rand.NextVector2Circular(1f, 1f));
                d.noGravity = true;
                d.scale = 0.4f;
                d.alpha = 150;
            }

            // Fade in then out
            float fadeProgress = _swingProgress;
            Projectile.Opacity = fadeProgress < 0.15f ? fadeProgress / 0.15f * 0.35f :
                                 fadeProgress > 0.85f ? (1f - fadeProgress) / 0.15f * 0.35f : 0.35f;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            var texture = Terraria.GameContent.TextureAssets.Projectile[Type].Value;
            float angle = Projectile.velocity.ToRotation() + GetSwingAngle(_swingProgress) * GhostSide;
            Vector2 direction = angle.ToRotationVector2();

            int drawDirection = Math.Sign(Projectile.velocity.X) <= 0 ? -1 : 1;
            SpriteEffects effects = drawDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            // Ghostly tint: translucent lunar colors
            Color ghostColor = Color.Lerp(EternalMoonUtils.Violet, EternalMoonUtils.IceBlue, _swingProgress);
            ghostColor *= Projectile.Opacity;

            float rotation = angle + MathHelper.PiOver4 + (drawDirection == -1 ? MathHelper.Pi : 0f);

            // Draw the ghost blade with reduced opacity
            Main.EntitySpriteDraw(texture, Owner.MountedCenter - Main.screenPosition, null,
                ghostColor, rotation, texture.Size() / 2f, 2.2f * Projectile.scale, effects, 0);

            // Additive glow overlay
            Color glowColor = EternalMoonUtils.IceBlue;
            glowColor.A = 0;
            Main.EntitySpriteDraw(texture, Owner.MountedCenter - Main.screenPosition, null,
                glowColor * Projectile.Opacity * 0.2f, rotation, texture.Size() / 2f,
                2.4f * Projectile.scale, effects, 0);

            return false;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!Main.dedServ)
            {
                // Subtle ghost impact: few sparks
                for (int i = 0; i < 3; i++)
                {
                    Vector2 sparkVel = Main.rand.NextVector2Unit() * Main.rand.NextFloat(2f, 5f);
                    LunarParticleHandler.SpawnParticle(new CrescentSparkParticle(
                        target.Center, sparkVel, Main.rand.NextFloat(0.2f, 0.5f),
                        EternalMoonUtils.Violet, 12));
                }
            }
        }
    }
}
