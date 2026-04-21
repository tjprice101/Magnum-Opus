using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;
using MagnumOpus.Content.OdeToJoy;

namespace MagnumOpus.Content.OdeToJoy.Weapons.ThornSprayRepeater.Projectiles
{
    /// <summary>
    /// Thorn Spray projectile — dark forest-green needle with bright emerald tip.
    /// Sharp, small, fast — like a thorn cutting through air.
    /// </summary>
    public class ThornSprayProjectile : ModProjectile
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
            Projectile.width = 10;
            Projectile.height = 10;
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

            // Sparse dark-green dust (thorn cutting through air)
            if (Main.rand.NextBool(5))
            {
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.Grass,
                    -Projectile.velocity * 0.08f, 0, OdeToJoyPalette.LeafGreen, 0.45f);
                d.noGravity = true;
            }

            Lighting.AddLight(Projectile.Center, OdeToJoyPalette.LeafGreen.ToVector3() * 0.15f);
        }

        public override void OnKill(int timeLeft)
        {
            if (Main.dedServ) return;
            for (int i = 0; i < 5; i++)
            {
                Color col = i < 3 ? OdeToJoyPalette.LeafGreen : OdeToJoyPalette.VerdantGreen;
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.Grass,
                    Main.rand.NextVector2CircularEdge(2f, 2f), 0, col, 0.5f);
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
