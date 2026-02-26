using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.VFX;

namespace MagnumOpus.Content.Eroica.Weapons.CelestialValor.Dusts
{
    /// <summary>
    /// Valor Ember — a rising flame particle that shifts from deep scarlet
    /// through crimson to brilliant gold as it ascends. The signature ambient
    /// particle of Celestial Valor's heroic fire aura.
    /// 
    /// Drawn as a multi-layered soft glow with {A=0} additive bloom,
    /// following the SandboxLastPrism PixelGlowOrb pattern.
    /// </summary>
    public class ValorEmberDust : ModDust
    {
        public override string Texture => "MagnumOpus/Assets/Particles/SoftGlow2";

        public override void OnSpawn(Dust dust)
        {
            dust.noGravity = true;
            dust.noLight = false;
            dust.frame = new Rectangle(0, 0, 1, 1); // Will be overridden in PreDraw
        }

        public override bool Update(Dust dust)
        {
            if (dust.customData is CVDustBehaviorUtil.HeroicEmberBehavior behavior)
            {
                behavior.Update(dust);
            }
            else
            {
                // Default behavior if no customData
                dust.velocity *= 0.96f;
                dust.velocity.Y -= 0.04f; // Rise
                dust.scale *= 0.97f;
                if (dust.scale < 0.1f)
                    dust.active = false;
            }

            dust.position += dust.velocity;

            // Emit warm heroic light
            float r = dust.color.R / 255f * dust.scale * 0.5f;
            float g = dust.color.G / 255f * dust.scale * 0.3f;
            float b = dust.color.B / 255f * dust.scale * 0.1f;
            Lighting.AddLight(dust.position, r, g, b);

            return false; // Custom Update
        }

        public override bool PreDraw(Dust dust)
        {
            SpriteBatch sb = Main.spriteBatch;
            Texture2D tex = MagnumTextureRegistry.GetSoftGlow();
            if (tex == null) return false;

            Vector2 drawPos = dust.position - Main.screenPosition;
            Vector2 origin = new Vector2(tex.Width / 2f, tex.Height / 2f);

            float pulse = 1f + (float)Math.Sin(Main.GameUpdateCount * 0.15f + dust.position.X * 0.01f) * 0.1f;

            // Layer 1: Outer glow (deep scarlet -> gold, large, soft)
            Color outerColor = dust.color with { A = 0 } * 0.3f;
            sb.Draw(tex, drawPos, null, outerColor, dust.rotation, origin,
                dust.scale * 1.6f * pulse, SpriteEffects.None, 0f);

            // Layer 2: Mid glow (warmer)
            Color midColor = Color.Lerp(dust.color, EroicaPalette.Gold, 0.3f) with { A = 0 } * 0.5f;
            sb.Draw(tex, drawPos, null, midColor, dust.rotation, origin,
                dust.scale * 1.0f * pulse, SpriteEffects.None, 0f);

            // Layer 3: Core (bright hot center)
            Color coreColor = Color.Lerp(dust.color, EroicaPalette.HotCore, 0.5f) with { A = 0 } * 0.7f;
            sb.Draw(tex, drawPos, null, coreColor, dust.rotation, origin,
                dust.scale * 0.5f * pulse, SpriteEffects.None, 0f);

            return false; // Custom PreDraw
        }
    }
}
