using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.EnigmaVariations.ResonantWeapons.VariationsOfTheVoid.Projectiles
{
    /// <summary>
    /// Massive energy wave projectile for Variations of the Void's charged right-click.
    /// Grows in scale, fades over its short lifetime, emits intense purple light.
    /// </summary>
    public class VoidSpecialProj : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftGlow";

        private static readonly Color EnigmaPurple = new Color(140, 60, 200);

        public override void SetDefaults()
        {
            Projectile.width = 80;
            Projectile.height = 80;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 60;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 8;
        }

        public override void AI()
        {
            // Slow to a stop
            Projectile.velocity *= 0.96f;

            // Scale grows over lifetime
            float progress = 1f - (Projectile.timeLeft / 60f);
            Projectile.scale = MathHelper.Lerp(0.5f, 3.0f, progress);

            // Opacity fades
            Projectile.Opacity = MathHelper.Lerp(1f, 0f, progress * progress);

            // Rotation
            Projectile.rotation += 0.05f;

            // Intense purple light
            float pulse = 0.8f + 0.2f * MathF.Sin(Main.GlobalTimeWrappedHourly * 8f);
            Vector3 lightColor = EnigmaPurple.ToVector3() * Projectile.Opacity * pulse * 0.8f;
            Lighting.AddLight(Projectile.Center, lightColor);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            try
            {
                sb.End();
                sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                    DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

                Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
                Vector2 drawPos = Projectile.Center - Main.screenPosition;
                Vector2 origin = tex.Size() * 0.5f;
                float opacity = Projectile.Opacity;

                // Outer bloom
                sb.Draw(tex, drawPos, null, EnigmaPurple * opacity * 0.25f, Projectile.rotation,
                    origin, Projectile.scale * 2.5f, SpriteEffects.None, 0f);
                // Mid bloom
                sb.Draw(tex, drawPos, null, EnigmaPurple * opacity * 0.5f, Projectile.rotation * 0.8f,
                    origin, Projectile.scale * 1.5f, SpriteEffects.None, 0f);
                // Core
                sb.Draw(tex, drawPos, null, Color.White * opacity * 0.8f, Projectile.rotation * 1.2f,
                    origin, Projectile.scale * 0.6f, SpriteEffects.None, 0f);
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
