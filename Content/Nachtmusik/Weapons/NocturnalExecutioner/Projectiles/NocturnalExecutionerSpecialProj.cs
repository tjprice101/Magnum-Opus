using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Graphics;
using MagnumOpus.Common.Systems.VFX;

namespace MagnumOpus.Content.Nachtmusik.Weapons.NocturnalExecutioner.Projectiles
{
    /// <summary>Nocturnal Executioner special: seeking void orb.</summary>
    public class NocturnalExecutionerSpecialProj : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftGlow";

        private VertexStrip _strip;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 16;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 20; Projectile.height = 20;
            Projectile.friendly = true; Projectile.penetrate = 2;
            Projectile.tileCollide = false; Projectile.timeLeft = 180;
            Projectile.DamageType = DamageClass.MeleeNoSpeed;
            Projectile.ignoreWater = true;
            Projectile.extraUpdates = 1;
        }

        public override void AI()
        {
            // Weak homing
            NPC target = Projectile.FindTargetWithinRange(600f);
            if (target != null)
            {
                Vector2 desiredVel = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX) * 12f;
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, desiredVel, 0.04f);
            }
            Projectile.rotation += 0.1f;
            Lighting.AddLight(Projectile.Center, 0.2f, 0.2f, 0.4f);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            try
            {
                IncisorOrbRenderer.DrawOrbVisuals(sb, Projectile, IncisorOrbRenderer.Nachtmusik, ref _strip);
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
