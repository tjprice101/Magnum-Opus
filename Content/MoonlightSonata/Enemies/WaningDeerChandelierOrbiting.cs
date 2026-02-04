using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems;

namespace MagnumOpus.Content.MoonlightSonata.Enemies
{
    /// <summary>
    /// Waning Deer Chandelier - A floating chandelier that orbits around the Waning Deer.
    /// Two to three of these gently follow the creature, providing ambient lighting.
    /// </summary>
    public class WaningDeerChandelierOrbiting : ModProjectile
    {
        public override string Texture => "MagnumOpus/Content/MoonlightSonata/Enemies/WaningDeerFloat";

        private int ParentNPCIndex => (int)Projectile.ai[0];
        private float OrbitAngle
        {
            get => Projectile.ai[1];
            set => Projectile.ai[1] = value;
        }

        // Orbit parameters - slightly different from Lunus lanterns
        private const float OrbitRadiusX = 90f;
        private const float OrbitRadiusY = 55f;
        private const float OrbitSpeed = 0.018f;
        private const float BobSpeed = 0.035f;
        private const float BobAmount = 10f;

        public override void SetDefaults()
        {
            Projectile.width = 24;
            Projectile.height = 40;
            Projectile.hostile = false;
            Projectile.friendly = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 60; // Will be refreshed while parent is alive
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.alpha = 0;
            Projectile.light = 0f; // Custom lighting
        }

        public override void AI()
        {
            // Check if parent Waning Deer still exists
            NPC parent = null;
            if (ParentNPCIndex >= 0 && ParentNPCIndex < Main.maxNPCs)
            {
                NPC potentialParent = Main.npc[ParentNPCIndex];
                if (potentialParent.active && potentialParent.type == ModContent.NPCType<WaningDeer>())
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
            float bobOffset = (float)Math.Sin(Main.GameUpdateCount * BobSpeed + OrbitAngle) * BobAmount;
            Vector2 orbitOffset = new Vector2(
                (float)Math.Cos(OrbitAngle) * OrbitRadiusX,
                (float)Math.Sin(OrbitAngle) * OrbitRadiusY * 0.4f + bobOffset - 50f // Float above
            );

            Vector2 targetPos = parent.Center + orbitOffset;

            // Smooth movement to target position
            Vector2 direction = targetPos - Projectile.Center;
            float distance = direction.Length();
            
            if (distance > 2f)
            {
                // Gentle, smooth following
                float speed = Math.Min(distance * 0.08f, 7f);
                Projectile.velocity = direction.SafeNormalize(Vector2.Zero) * speed;
            }
            else
            {
                Projectile.velocity *= 0.5f;
            }

            // Gentle swaying rotation
            Projectile.rotation = (float)Math.Sin(Main.GameUpdateCount * 0.025f + OrbitAngle) * 0.08f;

            // Ambient lighting - soft icy blue and purple
            float lightPulse = (float)Math.Sin(Main.GameUpdateCount * 0.04f + OrbitAngle * 2f) * 0.15f + 0.85f;
            Lighting.AddLight(Projectile.Center, 0.4f * lightPulse, 0.5f * lightPulse, 0.8f * lightPulse);

            // Occasional gentle particles - purple and light blue
            if (Main.rand.NextBool(18))
            {
                int dustType = Main.rand.NextBool() ? DustID.PurpleTorch : DustID.IceTorch;
                Dust glow = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, dustType, 0f, 0f, 150, default, 0.6f);
                glow.noGravity = true;
                glow.velocity = new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), Main.rand.NextFloat(-1f, -0.3f));
                glow.fadeIn = 0.5f;
            }

            // Snow sparkle particles
            if (Main.rand.NextBool(25))
            {
                Dust sparkle = Dust.NewDustDirect(Projectile.Center + Main.rand.NextVector2Circular(12f, 24f), 1, 1, DustID.BlueFairy, 0f, 0f, 0, default, 0.5f);
                sparkle.noGravity = true;
                sparkle.velocity = new Vector2(0f, Main.rand.NextFloat(-0.5f, 0f));
            }

            // Occasional snowflake
            if (Main.rand.NextBool(40))
            {
                Dust snow = Dust.NewDustDirect(Projectile.Center, 1, 1, DustID.Snow, 0f, 0f, 100, default, 0.6f);
                snow.noGravity = true;
                snow.velocity = new Vector2(Main.rand.NextFloat(-0.3f, 0.3f), Main.rand.NextFloat(0.2f, 0.5f));
            }
            
            // ☁EMUSICAL NOTATION - Ethereal chandelier melody (subtle for enemy)
            if (Main.rand.NextBool(30))
            {
                Color noteColor = Color.Lerp(new Color(138, 43, 226), new Color(135, 206, 250), Main.rand.NextFloat()) * 0.7f;
                Vector2 noteVel = new Vector2(Main.rand.NextFloat(-0.3f, 0.3f), -0.6f);
                ThemedParticles.MusicNote(Projectile.Center, noteVel, noteColor, 0.22f, 28);
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            Vector2 origin = new Vector2(texture.Width / 2, texture.Height / 2);
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            // Pulsing glow effect - icy
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.05f + OrbitAngle * 2f) * 0.3f + 0.7f;
            
            // Draw glow layers (behind) - alternating blue and purple
            Color blueGlow = new Color(100, 160, 220) * pulse * 0.4f;
            Color purpleGlow = new Color(150, 100, 200) * pulse * 0.3f;
            
            for (int i = 0; i < 4; i++)
            {
                Vector2 offset = new Vector2(4f, 0f).RotatedBy(MathHelper.TwoPi * i / 4);
                Color glowColor = i % 2 == 0 ? blueGlow : purpleGlow;
                Main.EntitySpriteDraw(texture, drawPos + offset, null, glowColor, Projectile.rotation, origin, Projectile.scale * 1.05f, SpriteEffects.None, 0);
            }

            // Outer soft glow
            Color outerGlow = new Color(80, 120, 180) * pulse * 0.2f;
            for (int i = 0; i < 6; i++)
            {
                Vector2 offset = new Vector2(8f, 0f).RotatedBy(MathHelper.TwoPi * i / 6);
                Main.EntitySpriteDraw(texture, drawPos + offset, null, outerGlow, Projectile.rotation, origin, Projectile.scale * 1.1f, SpriteEffects.None, 0);
            }

            // Draw main sprite with enhanced brightness
            Color drawColor = new Color(255, 255, 255, 220);
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
            // Simplified chandelier death - silver mist dissipation (enemy, small)
            DynamicParticleEffects.MoonlightDeathSilverMist(Projectile.Center, 0.5f);
        }
    }
}
