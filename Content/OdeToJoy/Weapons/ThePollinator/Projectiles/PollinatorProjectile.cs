using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;
using MagnumOpus.Content.OdeToJoy;

namespace MagnumOpus.Content.OdeToJoy.Weapons.ThePollinator.Projectiles
{
    /// <summary>
    /// Pollinator bullet — dense golden pollen burst with yellow-green bloom trail.
    /// </summary>
    public class PollinatorProjectile : ModProjectile
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
            Projectile.width = 12;
            Projectile.height = 12;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 120;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.extraUpdates = 1;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();

            if (Main.rand.NextBool(4))
            {
                Color dustCol = Main.rand.NextBool() ? OdeToJoyPalette.GoldenPollen : OdeToJoyPalette.SunlightYellow;
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.Flare,
                    -Projectile.velocity * 0.05f, 0, dustCol, 0.50f);
                d.noGravity = true;
            }

            Lighting.AddLight(Projectile.Center, OdeToJoyPalette.SunlightYellow.ToVector3() * 0.20f);
        }

        public override void OnKill(int timeLeft)
        {
            if (Main.dedServ) return;
            for (int i = 0; i < 6; i++)
            {
                Color col = i < 3 ? OdeToJoyPalette.GoldenPollen : OdeToJoyPalette.BudGreen;
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.Flare,
                    Main.rand.NextVector2CircularEdge(2.5f, 2.5f), 0, col, 0.55f);
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
