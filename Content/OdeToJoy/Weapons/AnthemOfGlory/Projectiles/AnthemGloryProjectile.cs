using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;
using MagnumOpus.Content.OdeToJoy;

namespace MagnumOpus.Content.OdeToJoy.Weapons.AnthemOfGlory.Projectiles
{
    /// <summary>
    /// Anthem of Glory magic bolt — white-gold spark with verdant bloom trail.
    /// Rapid-fire prismatic notes of the eternal symphony.
    /// </summary>
    public class AnthemGloryProjectile : ModProjectile
    {
        private VertexStrip _strip;
        public override string Texture => "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/PointBloom";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 16;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 120;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.extraUpdates = 1;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();

            if (Main.rand.NextBool(3))
            {
                Color dustCol = Main.rand.NextBool() ? OdeToJoyPalette.GoldenPollen : OdeToJoyPalette.VerdantGreen;
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.Flare,
                    -Projectile.velocity * 0.1f + Main.rand.NextVector2Circular(0.3f, 0.3f),
                    0, dustCol, 0.55f);
                d.noGravity = true;
                d.fadeIn = 0.2f;
            }

            Lighting.AddLight(Projectile.Center, OdeToJoyPalette.GoldenPollen.ToVector3() * 0.25f);
        }

        public override void OnKill(int timeLeft)
        {
            if (Main.dedServ) return;
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 5f);
                Color col = i % 2 == 0 ? OdeToJoyPalette.GoldenPollen : OdeToJoyPalette.RosePink;
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.Flare, vel, 0, col, 0.65f);
                d.noGravity = true;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            IncisorOrbRenderer.DrawOrbVisuals(Main.spriteBatch, Projectile, IncisorOrbRenderer.OdeToJoy, ref _strip);
            return false;
        }
    }
}
