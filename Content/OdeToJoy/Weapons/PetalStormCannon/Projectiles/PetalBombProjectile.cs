using MagnumOpus.Common;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.VFX.Core;
using MagnumOpus.Content.OdeToJoy;
using MagnumOpus.Content.OdeToJoy.Weapons.PetalStormCannon.Dusts;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.OdeToJoy.Weapons.PetalStormCannon.Projectiles
{
    /// <summary>
    /// Petal Bomb — Lobbed by Symphony of Blossoms accessory on petal storm trigger.
    /// Arcs toward target, detonates on contact or after 2s with a radial petal burst.
    /// </summary>
    public class PetalBombProjectile : ModProjectile
    {
        private VertexStrip _vertexStrip;

        public override string Texture => "MagnumOpus/Assets/VFX Asset Library/Theme Specific/Ode to Joy/Projectiles/OJ Rose Petal";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 16;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Generic;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 120;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = false;
        }

        public override void AI()
        {
            Projectile.rotation += Projectile.velocity.X * 0.04f;
            Projectile.velocity.Y += 0.2f; // Gravity arc

            // Petal dust trail — custom storm petal
            if (Main.rand.NextBool(3))
            {
                Color col = Main.rand.NextBool()
                    ? new Color(220, 100, 120)  // PetalPink
                    : new Color(255, 200, 50);  // BloomGold
                Dust dust = Dust.NewDustPerfect(
                    Projectile.Center + Main.rand.NextVector2Circular(4f, 4f),
                    ModContent.DustType<StormPetalDust>(),
                    Projectile.velocity * -0.2f + Main.rand.NextVector2Circular(1f, 1f),
                    Scale: Main.rand.NextFloat(0.3f, 0.6f));
                dust.color = col;
                dust.noGravity = true;
                dust.fadeIn = 0.4f;
            }
        }

        public override void OnKill(int timeLeft)
        {
            // Radial petal burst on detonation — custom storm petal
            for (int i = 0; i < 16; i++)
            {
                float angle = MathHelper.TwoPi / 16f * i;
                float speed = Main.rand.NextFloat(2f, 5f);
                Vector2 vel = angle.ToRotationVector2() * speed;
                Color col = (i % 3) switch
                {
                    0 => new Color(220, 100, 120),  // PetalPink
                    1 => new Color(255, 200, 50),   // BloomGold
                    _ => new Color(255, 250, 200)    // JubilantLight
                };
                Dust dust = Dust.NewDustPerfect(Projectile.Center,
                    ModContent.DustType<StormPetalDust>(), vel,
                    Scale: Main.rand.NextFloat(0.4f, 0.8f));
                dust.color = col;
                dust.noGravity = true;
            }
            OdeToJoyVFXLibrary.SpawnGardenSparkleExplosion(Projectile.Center, 5, 4f, 1f);
            OdeToJoyVFXLibrary.ScreenShake(5f, 10);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            try
            {
                IncisorOrbRenderer.DrawOrbVisuals(sb, Projectile, IncisorOrbRenderer.OdeToJoy, ref _vertexStrip);

                // Petal Bomb accent: rose-pink petal shimmer halo
                sb.End();
                sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                    SamplerState.LinearClamp, DepthStencilState.None,
                    RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

                Vector2 drawPos = Projectile.Center - Main.screenPosition;
                Texture2D glow = MagnumTextureRegistry.GetSoftGlow();
                if (glow != null)
                {
                    Vector2 origin = glow.Size() / 2f;
                    float pulse = 0.8f + 0.2f * MathF.Sin((float)Main.timeForVisualEffects * 0.12f);

                    // Rose-pink bloom
                    sb.Draw(glow, drawPos, null,
                        (OdeToJoyPalette.RosePink with { A = 0 }) * 0.2f * pulse,
                        0f, origin, 0.05f, SpriteEffects.None, 0f);
                }

                sb.End();
            }
            catch { }
            finally
            {
                try { sb.End(); } catch { }
                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                    DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            }

            return false;
        }
    }
}
