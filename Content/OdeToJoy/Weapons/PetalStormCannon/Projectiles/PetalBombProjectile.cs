using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;
using MagnumOpus.Content.OdeToJoy;

namespace MagnumOpus.Content.OdeToJoy.Weapons.PetalStormCannon.Projectiles
{
    /// <summary>
    /// Petal Bomb — soft rose-pink arcing projectile that detonates into a petal storm.
    /// A glass rose hurtling through air, bursting into prismatic petals.
    /// </summary>
    public class PetalBombProjectile : ModProjectile
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
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 120;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
        }

        public override void AI()
        {
            // Gravity arc — this is a bomb, not a bullet
            Projectile.velocity.Y += 0.10f;
            Projectile.rotation += 0.04f;

            if (Main.rand.NextBool(3))
            {
                Color dustCol = Main.rand.NextBool() ? OdeToJoyPalette.RosePink : OdeToJoyPalette.PetalPink;
                Dust d = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(5f, 5f),
                    DustID.Flare, -Projectile.velocity * 0.08f + Main.rand.NextVector2Circular(0.5f, 0.5f),
                    0, dustCol, 0.60f);
                d.noGravity = true;
                d.fadeIn = 0.3f;
            }

            Lighting.AddLight(Projectile.Center, OdeToJoyPalette.RosePink.ToVector3() * 0.25f);
        }

        public override void OnKill(int timeLeft)
        {
            if (Main.dedServ) return;
            // Large petal burst explosion
            for (int i = 0; i < 16; i++)
            {
                float angle = MathHelper.TwoPi * i / 16f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 7f);
                Color col = i % 3 == 0 ? OdeToJoyPalette.WhiteBloom
                    : i % 3 == 1 ? OdeToJoyPalette.RosePink
                    : OdeToJoyPalette.PetalPink;
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.Flare, vel, 0, col, 0.75f);
                d.noGravity = true;
            }
            // Pollen scatter
            for (int i = 0; i < 6; i++)
            {
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.Flare,
                    Main.rand.NextVector2Circular(4f, 4f), 0, OdeToJoyPalette.GoldenPollen, 0.55f);
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
