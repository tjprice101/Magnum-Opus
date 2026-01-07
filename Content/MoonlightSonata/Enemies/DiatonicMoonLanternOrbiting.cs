using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.MoonlightSonata.Enemies
{
    /// <summary>
    /// Diatonic Moon Lantern - A tall lantern that floats and orbits around Lunus.
    /// Two of these gently follow the creature, providing ambient lighting.
    /// </summary>
    public class DiatonicMoonLanternOrbiting : ModProjectile
    {
        public override string Texture => "MagnumOpus/Content/MoonlightSonata/Enemies/DiatonicMoonLantern";

        private int ParentNPCIndex => (int)Projectile.ai[0];
        private float OrbitAngle
        {
            get => Projectile.ai[1];
            set => Projectile.ai[1] = value;
        }

        // Orbit parameters
        private const float OrbitRadiusX = 80f;
        private const float OrbitRadiusY = 50f;
        private const float OrbitSpeed = 0.02f;
        private const float BobSpeed = 0.04f;
        private const float BobAmount = 8f;

        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 40;
            Projectile.hostile = false;
            Projectile.friendly = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 60; // Will be refreshed while parent is alive
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.alpha = 0;
            Projectile.light = 0f; // We'll do custom lighting
        }

        public override void AI()
        {
            // Check if parent Lunus still exists
            NPC parent = null;
            if (ParentNPCIndex >= 0 && ParentNPCIndex < Main.maxNPCs)
            {
                NPC potentialParent = Main.npc[ParentNPCIndex];
                if (potentialParent.active && potentialParent.type == ModContent.NPCType<Lunus>())
                {
                    parent = potentialParent;
                }
            }

            // Despawn if parent is gone
            if (parent == null)
            {
                Projectile.Kill();
                return;
            }

            // Stay alive while parent exists
            Projectile.timeLeft = 60;

            // Update orbit angle
            OrbitAngle += OrbitSpeed;
            if (OrbitAngle > MathHelper.TwoPi)
                OrbitAngle -= MathHelper.TwoPi;

            // Calculate target position (elliptical orbit around parent)
            float bobOffset = (float)Math.Sin(Main.GameUpdateCount * BobSpeed) * BobAmount;
            Vector2 orbitOffset = new Vector2(
                (float)Math.Cos(OrbitAngle) * OrbitRadiusX,
                (float)Math.Sin(OrbitAngle) * OrbitRadiusY * 0.5f + bobOffset - 40f // Float above
            );

            Vector2 targetPos = parent.Center + orbitOffset;

            // Smooth movement to target position
            Vector2 direction = targetPos - Projectile.Center;
            float distance = direction.Length();
            
            if (distance > 2f)
            {
                // Gentle, smooth following
                float speed = Math.Min(distance * 0.1f, 8f);
                Projectile.velocity = direction.SafeNormalize(Vector2.Zero) * speed;
            }
            else
            {
                Projectile.velocity *= 0.5f;
            }

            // Gentle swaying rotation
            Projectile.rotation = (float)Math.Sin(Main.GameUpdateCount * 0.03f + OrbitAngle) * 0.1f;

            // Ambient lighting - soft purple/blue glow
            float lightPulse = (float)Math.Sin(Main.GameUpdateCount * 0.05f + OrbitAngle * 2f) * 0.15f + 0.85f;
            Lighting.AddLight(Projectile.Center, 0.5f * lightPulse, 0.4f * lightPulse, 0.8f * lightPulse);

            // Occasional gentle particles
            if (Main.rand.NextBool(15))
            {
                Dust glow = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.PurpleTorch, 0f, 0f, 150, default, 0.6f);
                glow.noGravity = true;
                glow.velocity = new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), Main.rand.NextFloat(-1f, -0.3f));
                glow.fadeIn = 0.5f;
            }

            // Sparkle particles
            if (Main.rand.NextBool(30))
            {
                Dust sparkle = Dust.NewDustDirect(Projectile.Center + Main.rand.NextVector2Circular(10f, 20f), 1, 1, DustID.PinkFairy, 0f, 0f, 0, default, 0.5f);
                sparkle.noGravity = true;
                sparkle.velocity = new Vector2(0f, Main.rand.NextFloat(-0.5f, 0f));
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            Vector2 origin = new Vector2(texture.Width / 2, texture.Height / 2);
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            // Pulsing glow effect
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.06f + OrbitAngle * 2f) * 0.3f + 0.7f;
            
            // Draw glow layers (behind)
            Color glowColor = new Color(140, 100, 200) * pulse * 0.4f;
            for (int i = 0; i < 4; i++)
            {
                Vector2 offset = new Vector2(4f, 0f).RotatedBy(MathHelper.TwoPi * i / 4);
                Main.EntitySpriteDraw(texture, drawPos + offset, null, glowColor, Projectile.rotation, origin, Projectile.scale * 1.05f, SpriteEffects.None, 0);
            }

            // Outer soft glow
            Color outerGlow = new Color(100, 80, 180) * pulse * 0.2f;
            for (int i = 0; i < 6; i++)
            {
                Vector2 offset = new Vector2(8f, 0f).RotatedBy(MathHelper.TwoPi * i / 6);
                Main.EntitySpriteDraw(texture, drawPos + offset, null, outerGlow, Projectile.rotation, origin, Projectile.scale * 1.1f, SpriteEffects.None, 0);
            }

            // Draw main sprite with enhanced brightness
            Color drawColor = new Color(255, 255, 255, 230);
            Main.EntitySpriteDraw(texture, drawPos, null, drawColor, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);

            return false; // Don't draw default
        }

        public override Color? GetAlpha(Color lightColor)
        {
            // Always visible, not affected by darkness
            return new Color(255, 255, 255, 200);
        }

        public override void OnKill(int timeLeft)
        {
            // Fade-out particles
            for (int i = 0; i < 8; i++)
            {
                Dust fade = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.PurpleTorch, 0f, 0f, 100, default, 0.8f);
                fade.noGravity = true;
                fade.velocity = Main.rand.NextVector2Circular(2f, 2f);
                fade.fadeIn = 0.3f;
            }
        }
    }
}
