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
    /// Anamorphic streak VFX spawned at sword hit points.
    /// 15-frame lifetime, renders Horizontal Anamorphic Streak texture
    /// perpendicular to blade direction with 3 additive layers.
    /// Spawned 1-2 per hit.
    /// </summary>
    public class AnamorphicStreakVFX : ModProjectile
    {
        #region Constants

        private const int Lifetime = 15;

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

            float fadeAlpha = 1f - (float)timer / Lifetime;
            Color light = TerraBladeShaderManager.GetPaletteColor(0.5f);
            Lighting.AddLight(Projectile.Center, light.ToVector3() * fadeAlpha * 0.5f);

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
            float storedRotation = Projectile.ai[0]; // SwordRotation + PiOver2 at spawn time
            float fadeAlpha = 1f - (float)timer / Lifetime;

            // Quadratic ease-out
            fadeAlpha = fadeAlpha * fadeAlpha;

            // Quick scale-in
            float scaleRamp = MathHelper.Clamp(timer / 2f, 0f, 1f);

            Texture2D streakTex = SafeRequest("MagnumOpus/Assets/VFX/Blooms/Horizontal Anamorphic Streak");
            if (streakTex == null)
                streakTex = Terraria.GameContent.TextureAssets.Extra[98].Value;

            Vector2 origin = streakTex.Size() * 0.5f;

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null,
                Main.GameViewMatrix.TransformationMatrix);

            float baseScale = 0.35f * scaleRamp;

            // Layer 1: Outer glow (widest, dimmest)
            Color outerColor = TerraBladeShaderManager.GetPaletteColor(0.3f) with { A = 0 };
            sb.Draw(streakTex, drawPos, null, outerColor * fadeAlpha * 0.35f,
                storedRotation, origin, new Vector2(baseScale * 1.3f, baseScale * 0.5f), SpriteEffects.None, 0f);

            // Layer 2: Mid energy
            Color midColor = TerraBladeShaderManager.GetPaletteColor(0.5f) with { A = 0 };
            sb.Draw(streakTex, drawPos, null, midColor * fadeAlpha * 0.55f,
                storedRotation, origin, new Vector2(baseScale, baseScale * 0.4f), SpriteEffects.None, 0f);

            // Layer 3: White-hot core (narrowest, brightest)
            sb.Draw(streakTex, drawPos, null, (Color.White with { A = 0 }) * fadeAlpha * 0.45f,
                storedRotation, origin, new Vector2(baseScale * 0.6f, baseScale * 0.2f), SpriteEffects.None, 0f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                DepthStencilState.None, RasterizerState.CullNone, null,
                Main.GameViewMatrix.TransformationMatrix);

            return false;
        }

        #endregion
    }
}
