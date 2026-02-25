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
    /// 15-frame lifetime, renders HIGHLY stretched/squished flare textures layered with
    /// sparkle overlays and fluid shimmer animation perpendicular to blade direction.
    /// Spawned 1-2 per hit.
    /// </summary>
    public class AnamorphicStreakVFX : ModProjectile
    {
        #region Constants

        private const int Lifetime = 15;

        // Stretched flare parameters — extreme horizontal stretch for anamorphic look
        private const float StreakStretchX = 16f;
        private const float StreakSquishY = 0.04f;

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

        public override bool PreDraw(ref Color lightColor)
        {
            if (timer <= 0) return false;

            SpriteBatch sb = Main.spriteBatch;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float storedRotation = Projectile.ai[0];
            float fadeAlpha = 1f - (float)timer / Lifetime;
            float time = Main.GlobalTimeWrappedHourly;

            // Quadratic ease-out
            fadeAlpha = fadeAlpha * fadeAlpha;

            // Quick scale-in with pulse
            float scaleRamp = MathHelper.Clamp(timer / 2f, 0f, 1f);
            float pulse = 1f + MathF.Sin(time * 12f + timer * 0.4f) * 0.10f;
            float stretchMult = scaleRamp * pulse;

            // Load flare textures
            Texture2D flare1 = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/EnergyFlare").Value;
            Texture2D flare2 = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/FlareSparkle").Value;
            Texture2D flare3 = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/ThinSparkleFlare").Value;

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null,
                Main.GameViewMatrix.TransformationMatrix);

            // Layer 1: Wide outer glow — EnergyFlare, maximum stretch for anamorphic smear
            {
                Vector2 origin = flare1.Size() * 0.5f;
                Color outerColor = TerraBladeShaderManager.GetPaletteColor(0.3f) with { A = 0 };
                Vector2 scale = new Vector2(StreakStretchX, StreakSquishY) * stretchMult;
                sb.Draw(flare1, drawPos, null, outerColor * fadeAlpha * 0.35f,
                    storedRotation, origin, scale, SpriteEffects.None, 0f);
            }

            // Layer 2: Secondary glow — EnergyFlare, slightly narrower
            {
                Vector2 origin = flare1.Size() * 0.5f;
                Color midColor = TerraBladeShaderManager.GetPaletteColor(0.45f) with { A = 0 };
                Vector2 scale = new Vector2(StreakStretchX * 0.7f, StreakSquishY * 1.8f) * stretchMult;
                sb.Draw(flare1, drawPos, null, midColor * fadeAlpha * 0.40f,
                    storedRotation, origin, scale, SpriteEffects.None, 0f);
            }

            // Layer 3: FlareSparkle — sparkle texture with slight rotation wobble
            {
                Vector2 origin = flare2.Size() * 0.5f;
                Color sparkleColor = TerraBladeShaderManager.GetPaletteColor(0.6f) with { A = 0 };
                Vector2 scale = new Vector2(StreakStretchX * 0.5f, StreakSquishY * 1.4f) * stretchMult;
                sb.Draw(flare2, drawPos, null, sparkleColor * fadeAlpha * 0.45f,
                    storedRotation + MathF.Sin(time * 8f + timer * 0.3f) * 0.04f, origin, scale, SpriteEffects.None, 0f);
            }

            // Layer 4: ThinSparkleFlare — tight sparkle core
            {
                Vector2 origin = flare3.Size() * 0.5f;
                Color coreColor = TerraBladeShaderManager.GetPaletteColor(0.7f) with { A = 0 };
                Vector2 scale = new Vector2(StreakStretchX * 0.35f, StreakSquishY * 0.8f) * stretchMult;
                sb.Draw(flare3, drawPos, null, coreColor * fadeAlpha * 0.50f,
                    storedRotation, origin, scale, SpriteEffects.None, 0f);
            }

            // Layer 5: White-hot center — extreme stretch, narrowest
            {
                Vector2 origin = flare1.Size() * 0.5f;
                Vector2 scale = new Vector2(StreakStretchX * 0.4f, StreakSquishY * 0.3f) * stretchMult;
                sb.Draw(flare1, drawPos, null, (Color.White with { A = 0 }) * fadeAlpha * 0.50f,
                    storedRotation, origin, scale, SpriteEffects.None, 0f);
            }

            // Layer 6: Counter-rotating sparkle shimmer — dual sparkles
            {
                Vector2 origin = flare2.Size() * 0.5f;
                Color sparkColor = TerraBladeShaderManager.GetPaletteColor(0.55f) with { A = 0 };
                sb.Draw(flare2, drawPos, null, sparkColor * fadeAlpha * 0.20f,
                    -time * 3.5f + timer * 0.15f, origin, 0.12f * stretchMult, SpriteEffects.None, 0f);
                sb.Draw(flare2, drawPos, null, sparkColor * fadeAlpha * 0.12f,
                    time * 2.8f + timer * 0.1f, origin, 0.08f * stretchMult, SpriteEffects.None, 0f);
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
