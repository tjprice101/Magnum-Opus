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
    /// Snow of the Moon - An icy projectile fired by Waning Deer.
    /// Purple and light blue particles trail behind it.
    /// </summary>
    public class SnowOfTheMoonProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Content/MoonlightSonata/Enemies/SnowOfTheMoon";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 22;
            Projectile.height = 22;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 280;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = false;
            Projectile.alpha = 50;
            Projectile.light = 0.4f;
            Projectile.coldDamage = true; // Ice damage
        }

        public override void AI()
        {
            // Rotation
            Projectile.rotation += 0.12f;

            // Slight gravity
            Projectile.velocity.Y += 0.03f;
            if (Projectile.velocity.Y > 10f)
                Projectile.velocity.Y = 10f;

            // Lighting - icy blue
            Lighting.AddLight(Projectile.Center, 0.4f, 0.5f, 0.8f);

            // Trail particles - alternating purple and light blue
            if (Main.rand.NextBool(2))
            {
                int dustType = Main.rand.NextBool() ? DustID.PurpleTorch : DustID.IceTorch;
                Dust trail = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, dustType, 0f, 0f, 100, default, 1.1f);
                trail.noGravity = true;
                trail.velocity *= 0.2f;
            }

            // Snow particles
            if (Main.rand.NextBool(4))
            {
                Dust snow = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Snow, 0f, 0f, 100, default, 0.8f);
                snow.noGravity = true;
                snow.velocity = -Projectile.velocity * 0.1f + Main.rand.NextVector2Circular(1f, 1f);
            }

            // Sparkle particles
            if (Main.rand.NextBool(6))
            {
                Dust sparkle = Dust.NewDustDirect(Projectile.Center, 1, 1, DustID.BlueFairy, 0f, 0f, 0, default, 0.7f);
                sparkle.noGravity = true;
                sparkle.velocity = Main.rand.NextVector2Circular(2f, 2f);
            }
            
            // ☁EMUSICAL NOTATION - Lunar snow melody (subtle for enemy)
            if (Main.rand.NextBool(12))
            {
                Color noteColor = Color.Lerp(new Color(138, 43, 226), new Color(135, 206, 250), Main.rand.NextFloat()) * 0.6f;
                Vector2 noteVel = new Vector2(Main.rand.NextFloat(-0.4f, 0.4f), -0.8f);
                ThemedParticles.MusicNote(Projectile.Center, noteVel, noteColor, 0.25f, 25);
            }
        }

        public override void OnKill(int timeLeft)
        {
            // Simplified snow projectile death - gentle lunar ripple (enemy, small)
            DynamicParticleEffects.MoonlightDeathLunarRipple(Projectile.Center, 0.5f);
            Terraria.Audio.SoundEngine.PlaySound(SoundID.Item27, Projectile.Center); // Ice shatter sound
        }

        public override bool PreDraw(ref Color lightColor)
        {
            // Draw trail
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            Vector2 origin = new Vector2(texture.Width / 2, texture.Height / 2);

            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                Vector2 drawPos = Projectile.oldPos[i] + Projectile.Size / 2 - Main.screenPosition;
                float trailAlpha = (float)(Projectile.oldPos.Length - i) / Projectile.oldPos.Length;
                // Alternating blue and purple trail
                Color trailColor = i % 2 == 0 ? new Color(120, 180, 240) : new Color(180, 120, 220);
                trailColor *= trailAlpha * 0.5f;
                float trailScale = Projectile.scale * (0.4f + 0.6f * trailAlpha);

                Main.EntitySpriteDraw(texture, drawPos, null, trailColor, Projectile.oldRot[i], origin, trailScale, SpriteEffects.None, 0);
            }

            // Glow effect - icy
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.12f) * 0.25f + 0.75f;
            Color glowColor = new Color(100, 160, 220) * pulse * 0.5f;
            
            for (int i = 0; i < 4; i++)
            {
                Vector2 offset = new Vector2(3f, 0f).RotatedBy(MathHelper.TwoPi * i / 4);
                Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition + offset, null, glowColor, Projectile.rotation, origin, Projectile.scale * 1.1f, SpriteEffects.None, 0);
            }

            return true;
        }

        public override Color? GetAlpha(Color lightColor)
        {
            return new Color(255, 255, 255, 180);
        }
    }
}
