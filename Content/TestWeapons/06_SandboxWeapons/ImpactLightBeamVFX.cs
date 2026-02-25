using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.TestWeapons.SandboxWeapons.Shaders;

namespace MagnumOpus.Content.TestWeapons.SandboxWeapons
{
    /// <summary>
    /// Visual-only impact light beam spawned at sword hit points.
    /// 12-frame lifetime, renders stretched/squished flare textures with sparkle overlays
    /// at a stored rotation with layered additive rendering.
    /// Spawned 3-4 per hit with radially distributed angles.
    /// </summary>
    public class ImpactLightBeamVFX : ModProjectile
    {
        #region Constants

        private const int Lifetime = 12;

        // Stretched flare parameters for beam-like appearance
        private const float BeamStretchX = 10f;
        private const float BeamSquishY = 0.08f;

        #endregion

        #region State

        private int timer = 0;

        #endregion

        #region Setup

        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.None;

        public override void SetDefaults()
        {
            Projectile.width = 4;
            Projectile.height = 4;
            Projectile.friendly = false;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = Lifetime + 2;
            Projectile.ignoreWater = true;
        }

        public override bool? CanDamage() => false;
        public override bool ShouldUpdatePosition() => false;

        #endregion

        #region AI

        public override void AI()
        {
            timer++;

            Projectile.velocity = Vector2.Zero;

            Color light = TerraBladeShaderManager.GetPaletteColor(0.5f);
            float fadeAlpha = 1f - (float)timer / Lifetime;
            Lighting.AddLight(Projectile.Center, light.ToVector3() * fadeAlpha * 0.6f);

            if (timer >= Lifetime)
            {
                Projectile.Kill();
            }
        }

        #endregion

        #region Rendering

        public override bool PreDraw(ref Color lightColor)
        {
            if (timer <= 0) return false;

            SpriteBatch sb = Main.spriteBatch;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float storedRotation = Projectile.ai[0];
            float fadeAlpha = 1f - (float)timer / Lifetime;
            float time = Main.GlobalTimeWrappedHourly;

            // Smooth ease-out fade
            fadeAlpha = fadeAlpha * fadeAlpha;

            // Scale ramp
            float scaleRamp = MathHelper.Clamp(timer / 3f, 0f, 1f);
            float pulse = 1f + MathF.Sin(time * 10f) * 0.06f;

            // Load flare textures
            Texture2D flare1 = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/EnergyFlare").Value;
            Texture2D flare2 = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/ThinSparkleFlare").Value;
            Texture2D flare3 = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/FlareSparkle").Value;

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null,
                Main.GameViewMatrix.TransformationMatrix);

            float stretchMult = scaleRamp * pulse;

            // Layer 1: Wide outer glow — EnergyFlare, highly stretched along beam direction
            {
                Vector2 origin = flare1.Size() * 0.5f;
                Color beamColor = TerraBladeShaderManager.GetPaletteColor(0.4f) with { A = 0 };
                Vector2 stretchScale = new Vector2(BeamStretchX, BeamSquishY) * stretchMult;
                sb.Draw(flare1, drawPos, null, beamColor * fadeAlpha * 0.45f,
                    storedRotation, origin, stretchScale, SpriteEffects.None, 0f);
            }

            // Layer 2: ThinSparkleFlare — medium stretch for sparkly beam core
            {
                Vector2 origin = flare2.Size() * 0.5f;
                Color midColor = TerraBladeShaderManager.GetPaletteColor(0.6f) with { A = 0 };
                Vector2 stretchScale = new Vector2(BeamStretchX * 0.7f, BeamSquishY * 0.6f) * stretchMult;
                sb.Draw(flare2, drawPos, null, midColor * fadeAlpha * 0.55f,
                    storedRotation, origin, stretchScale, SpriteEffects.None, 0f);
            }

            // Layer 3: FlareSparkle overlay — adds sparkle texture to the beam
            {
                Vector2 origin = flare3.Size() * 0.5f;
                Color sparkleColor = TerraBladeShaderManager.GetPaletteColor(0.7f) with { A = 0 };
                Vector2 stretchScale = new Vector2(BeamStretchX * 0.5f, BeamSquishY * 1.5f) * stretchMult;
                sb.Draw(flare3, drawPos, null, sparkleColor * fadeAlpha * 0.35f,
                    storedRotation + 0.02f, origin, stretchScale, SpriteEffects.None, 0f);
            }

            // Layer 4: White-hot core — extreme stretch, narrowest
            {
                Vector2 origin = flare1.Size() * 0.5f;
                Vector2 stretchScale = new Vector2(BeamStretchX * 0.4f, BeamSquishY * 0.3f) * stretchMult;
                sb.Draw(flare1, drawPos, null, (Color.White with { A = 0 }) * fadeAlpha * 0.50f,
                    storedRotation, origin, stretchScale, SpriteEffects.None, 0f);
            }

            // Layer 5: Counter-rotating sparkle shimmer at impact center
            {
                Vector2 origin = flare3.Size() * 0.5f;
                Color sparkColor = TerraBladeShaderManager.GetPaletteColor(0.6f) with { A = 0 };
                float sparkRot = -time * 4f;
                sb.Draw(flare3, drawPos, null, sparkColor * fadeAlpha * 0.25f,
                    sparkRot, origin, 0.14f * stretchMult, SpriteEffects.None, 0f);
            }

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                DepthStencilState.None, RasterizerState.CullNone, null,
                Main.GameViewMatrix.TransformationMatrix);

            return false;
        }

        #endregion
    }
}
