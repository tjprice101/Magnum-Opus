using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.EnigmaVariations.ResonantWeapons.TheUnresolvedCadence.Projectiles
{
    /// <summary>
    /// Detonation projectile for The Unresolved Cadence's charged right-click.
    /// Expanding circle with intense purple/green light, high damage, short lifetime.
    /// </summary>
    public class CadenceSpecialProj : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftGlow";

        private static readonly Color EnigmaPurple = new Color(140, 60, 200);
        private static readonly Color EnigmaGreen = new Color(50, 220, 100);

        public override void SetDefaults()
        {
            Projectile.width = 120;
            Projectile.height = 120;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 30;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }

        public override void AI()
        {
            // Stationary detonation
            Projectile.velocity = Vector2.Zero;

            // Expanding circle
            float progress = 1f - (Projectile.timeLeft / 30f);
            Projectile.scale = MathHelper.Lerp(0.5f, 4.0f, progress);

            // Opacity: bright then fades rapidly
            Projectile.Opacity = progress < 0.3f ? 1f : MathHelper.Lerp(1f, 0f, (progress - 0.3f) / 0.7f);

            // Rotation
            Projectile.rotation += 0.1f;

            // Intense purple/green light that shifts
            float shift = MathF.Sin(Main.GlobalTimeWrappedHourly * 10f) * 0.5f + 0.5f;
            Color lightColor = Color.Lerp(EnigmaPurple, EnigmaGreen, shift);
            Vector3 light = lightColor.ToVector3() * Projectile.Opacity * 1.2f;
            Lighting.AddLight(Projectile.Center, light);

            // Burst dust on first frame
            if (Projectile.localAI[0] == 0f)
            {
                Projectile.localAI[0] = 1f;
                for (int i = 0; i < 20; i++)
                {
                    Vector2 vel = Main.rand.NextVector2CircularEdge(6f, 6f);
                    Dust dust = Dust.NewDustDirect(Projectile.Center, 0, 0,
                        DustID.PurpleTorch, vel.X, vel.Y, 100, default, 1.5f);
                    dust.noGravity = true;
                }
            }
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

                // Purple/green shift matching AI
                float shift = MathF.Sin(Main.GlobalTimeWrappedHourly * 10f) * 0.5f + 0.5f;
                Color baseColor = Color.Lerp(EnigmaPurple, EnigmaGreen, shift);

                // Outer bloom
                sb.Draw(tex, drawPos, null, baseColor * opacity * 0.2f, Projectile.rotation,
                    origin, Projectile.scale * 2.5f, SpriteEffects.None, 0f);
                // Mid bloom
                sb.Draw(tex, drawPos, null, baseColor * opacity * 0.5f, Projectile.rotation * 0.7f,
                    origin, Projectile.scale * 1.4f, SpriteEffects.None, 0f);
                // Core
                sb.Draw(tex, drawPos, null, Color.White * opacity * 0.85f, Projectile.rotation * 1.3f,
                    origin, Projectile.scale * 0.5f, SpriteEffects.None, 0f);
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
