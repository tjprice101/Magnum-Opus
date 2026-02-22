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
    /// 12-frame lifetime, renders Single Directional Light Shaft texture
    /// at a stored rotation with 2 additive layers (colored + white-hot core).
    /// Spawned 3-4 per hit with radially distributed angles.
    /// </summary>
    public class ImpactLightBeamVFX : ModProjectile
    {
        #region Constants

        private const int Lifetime = 12;

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

            // Rotation is stored in ai[0] at spawn time
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

        private static Texture2D SafeRequest(string path)
        {
            try
            {
                if (ModContent.HasAsset(path))
                    return ModContent.Request<Texture2D>(path).Value;
            }
            catch { }
            return null;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (timer <= 0) return false;

            SpriteBatch sb = Main.spriteBatch;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float storedRotation = Projectile.ai[0];
            float fadeAlpha = 1f - (float)timer / Lifetime;

            // Smooth ease-out fade
            fadeAlpha = fadeAlpha * fadeAlpha;

            // Scale: start slightly smaller, reach max at frame 3, then hold
            float scaleRamp = MathHelper.Clamp(timer / 3f, 0f, 1f);

            Texture2D beamTex = SafeRequest("MagnumOpus/Assets/VFX/LightRays/Single Directional Light Shaft");
            if (beamTex == null)
                beamTex = Terraria.GameContent.TextureAssets.Extra[98].Value;

            Vector2 origin = new Vector2(beamTex.Width * 0.5f, beamTex.Height);
            float beamScale = 0.4f * scaleRamp;

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null,
                Main.GameViewMatrix.TransformationMatrix);

            // Layer 1: Colored beam
            Color beamColor = TerraBladeShaderManager.GetPaletteColor(0.5f) with { A = 0 };
            sb.Draw(beamTex, drawPos, null, beamColor * fadeAlpha * 0.7f,
                storedRotation, origin, new Vector2(beamScale * 0.6f, beamScale), SpriteEffects.None, 0f);

            // Layer 2: Mid energy (slightly narrower)
            Color midColor = TerraBladeShaderManager.GetPaletteColor(0.7f) with { A = 0 };
            sb.Draw(beamTex, drawPos, null, midColor * fadeAlpha * 0.5f,
                storedRotation, origin, new Vector2(beamScale * 0.4f, beamScale * 0.95f), SpriteEffects.None, 0f);

            // Layer 3: White-hot core (narrowest)
            sb.Draw(beamTex, drawPos, null, (Color.White with { A = 0 }) * fadeAlpha * 0.5f,
                storedRotation, origin, new Vector2(beamScale * 0.25f, beamScale * 0.9f), SpriteEffects.None, 0f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                DepthStencilState.None, RasterizerState.CullNone, null,
                Main.GameViewMatrix.TransformationMatrix);

            return false;
        }

        #endregion
    }
}
