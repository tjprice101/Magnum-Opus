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

            // Ethereal lunar particles from ghost blade tip
            if (!Main.dedServ)
            {
                float angle = Projectile.velocity.ToRotation() + GetSwingAngle(_swingProgress) * GhostSide;
                Vector2 tipPos = Owner.MountedCenter + angle.ToRotationVector2() * BladeLength;

                // Tidal mote trail along ghost blade
                if (Main.rand.NextBool(3))
                {
                    Vector2 moteVel = angle.ToRotationVector2().RotatedByRandom(0.4f) * Main.rand.NextFloat(1f, 2.5f);
                    Color moteColor = Color.Lerp(EternalMoonUtils.Violet, EternalMoonUtils.IceBlue, Main.rand.NextFloat()) * 0.6f;
                    LunarParticleHandler.SpawnParticle(new TidalMoteParticle(
                        tipPos + Main.rand.NextVector2Circular(8f, 8f), moteVel,
                        Main.rand.NextFloat(0.2f, 0.4f), moteColor, Main.rand.Next(15, 30)));
                }

                // Moon glint sparkles at tip
                if (Main.rand.NextBool(7))
                {
                    LunarParticleHandler.SpawnParticle(new MoonGlintParticle(
                        tipPos + Main.rand.NextVector2Circular(5f, 5f),
                        Main.rand.NextFloat(0.15f, 0.3f),
                        EternalMoonUtils.MoonWhite * 0.5f, Main.rand.Next(10, 18)));
                }

                // Subtle tidal droplets falling from ghost blade
                if (Main.rand.NextBool(8))
                {
                    float bladePos = Main.rand.NextFloat(0.3f, 1f);
                    Vector2 dropPos = Owner.MountedCenter + angle.ToRotationVector2() * BladeLength * bladePos;
                    Vector2 dropVel = new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), Main.rand.NextFloat(-0.3f, 0.3f));
                    LunarParticleHandler.SpawnParticle(new TidalDropletParticle(
                        dropPos, dropVel, Main.rand.NextFloat(0.15f, 0.3f),
                        EternalMoonUtils.IceBlue * 0.4f, Main.rand.Next(15, 25)));
                }
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

            // Ghostly tint: translucent lunar colors — phase-dependent intensity
            float phaseGlow = MathHelper.Lerp(0.3f, 0.5f, LunarPhase / 4f);
            Color ghostColor = Color.Lerp(EternalMoonUtils.Violet, EternalMoonUtils.IceBlue, _swingProgress);
            ghostColor *= Projectile.Opacity;

            float rotation = angle + MathHelper.PiOver4 + (drawDirection == -1 ? MathHelper.Pi : 0f);

            // Layer 1: Wide outer glow (additive bloom under the ghost blade)
            Color outerGlow = EternalMoonUtils.DarkPurple;
            outerGlow.A = 0;
            Main.EntitySpriteDraw(texture, Owner.MountedCenter - Main.screenPosition, null,
                outerGlow * Projectile.Opacity * 0.1f, rotation, texture.Size() / 2f,
                2.8f * Projectile.scale, effects, 0);

            // Layer 2: Ghost blade body
            Main.EntitySpriteDraw(texture, Owner.MountedCenter - Main.screenPosition, null,
                ghostColor, rotation, texture.Size() / 2f, 2.2f * Projectile.scale, effects, 0);

            // Layer 3: Core glow overlay (additive)
            Color glowColor = EternalMoonUtils.IceBlue;
            glowColor.A = 0;
            Main.EntitySpriteDraw(texture, Owner.MountedCenter - Main.screenPosition, null,
                glowColor * Projectile.Opacity * phaseGlow, rotation, texture.Size() / 2f,
                2.4f * Projectile.scale, effects, 0);

            // Add moonlight
            Vector2 tipPos = Owner.MountedCenter + direction * BladeLength * 0.5f;
            Lighting.AddLight(tipPos, EternalMoonUtils.IceBlue.ToVector3() * Projectile.Opacity * 0.3f);

            return false;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!Main.dedServ)
            {
                // Ghost impact: crescent sparks + moon glint + subtle bloom
                for (int i = 0; i < 5; i++)
                {
                    Vector2 sparkVel = Main.rand.NextVector2Unit() * Main.rand.NextFloat(2f, 6f);
                    Color sparkColor = Color.Lerp(EternalMoonUtils.Violet, EternalMoonUtils.IceBlue, Main.rand.NextFloat());
                    LunarParticleHandler.SpawnParticle(new CrescentSparkParticle(
                        target.Center, sparkVel, Main.rand.NextFloat(0.2f, 0.5f),
                        sparkColor, Main.rand.Next(10, 18)));
                }

                // Moon glint at impact point
                LunarParticleHandler.SpawnParticle(new MoonGlintParticle(
                    target.Center, Main.rand.NextFloat(0.2f, 0.4f),
                    EternalMoonUtils.MoonWhite * 0.7f, 12));

                // Subtle ghost impact bloom
                LunarParticleHandler.SpawnParticle(new LunarBloomParticle(
                    target.Center, 0.3f, EternalMoonUtils.Violet * 0.5f, 12, 0.03f));
            }
        }
    }
}
