using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.VFX;

namespace MagnumOpus.Content.Eroica.Weapons.CelestialValor.Dusts
{
    /// <summary>
    /// Valor Crest — a brief 4-pointed star/cross that pulses once then fades.
    /// Used as accent for critical hits, phase transitions, and combo finishers.
    /// Bright gold-white core with scarlet arms.
    ///
    /// Follows SandboxLastPrism GlowPixelCross dual-layer pattern.
    /// </summary>
    public class ValorCrestDust : ModDust
    {
        public override string Texture => "MagnumOpus/Assets/Particles/CrispStar4";

        public override void OnSpawn(Dust dust)
        {
            dust.noGravity = true;
            dust.noLight = false;
            dust.frame = new Rectangle(0, 0, 1, 1);
        }

        public override bool Update(Dust dust)
        {
            dust.position += dust.velocity;
            dust.velocity *= 0.88f;

            // Quick flash lifecycle: grow fast (0-20%), hold (20-50%), shrink (50-100%)
            int age = (int)(dust.dustIndex * 0.1f) % 100; // Approximate age via index hack
            dust.rotation += 0.02f;
            dust.scale *= 0.96f;

            float intensity = dust.scale * 0.7f;
            Lighting.AddLight(dust.position, intensity * 1f, intensity * 0.9f, intensity * 0.4f);

            if (dust.scale < 0.08f)
                dust.active = false;

            return false;
        }

        public override bool PreDraw(Dust dust)
        {
            SpriteBatch sb = Main.spriteBatch;

            // Use CrispStar4 texture
            Texture2D starTex = null;
            try
            {
                var asset = EroicaTextures.Star4Point;
                starTex = asset?.Value;
            }
            catch { }

            if (starTex == null)
                starTex = MagnumTextureRegistry.GetSoftGlow();
            if (starTex == null) return false;

            Vector2 drawPos = dust.position - Main.screenPosition;
            Vector2 origin = new Vector2(starTex.Width / 2f, starTex.Height / 2f);

            float pulse = 1f + (float)Math.Sin(Main.GameUpdateCount * 0.25f) * 0.15f;

            // Layer 1: Outer scarlet glow (large, soft)
            Color outerColor = EroicaPalette.Scarlet with { A = 0 } * 0.3f;
            sb.Draw(starTex, drawPos, null, outerColor, dust.rotation, origin,
                dust.scale * 1.8f * pulse, SpriteEffects.None, 0f);

            // Layer 2: Gold mid layer
            Color goldColor = EroicaPalette.Gold with { A = 0 } * 0.5f;
            sb.Draw(starTex, drawPos, null, goldColor, dust.rotation + MathHelper.PiOver4 * 0.5f, origin,
                dust.scale * 1.2f * pulse, SpriteEffects.None, 0f);

            // Layer 3: White-hot core
            Color coreColor = EroicaPalette.HotCore with { A = 0 } * 0.8f;
            sb.Draw(starTex, drawPos, null, coreColor, dust.rotation, origin,
                dust.scale * 0.5f, SpriteEffects.None, 0f);

            return false;
        }
    }
}
