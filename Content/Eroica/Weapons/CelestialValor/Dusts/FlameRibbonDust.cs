using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Eroica.Weapons.CelestialValor.Dusts
{
    /// <summary>
    /// Flame Ribbon — a curving ribbon of fire that expands then dissipates.
    /// Used in swing trails and heroic finisher explosions. Grows to full size
    /// over the first third of its life, then gracefully fades while rotating.
    /// 
    /// Visual: A soft glow that transitions from hot white-gold core to
    /// deep scarlet edge, following the FlameRibbonBehavior pattern.
    /// </summary>
    public class FlameRibbonDust : ModDust
    {
        public override string Texture => "MagnumOpus/Assets/Particles/SoftGlow3";

        public override void OnSpawn(Dust dust)
        {
            dust.noGravity = true;
            dust.noLight = false;
        }

        public override bool Update(Dust dust)
        {
            if (dust.customData is CVDustBehaviorUtil.FlameRibbonBehavior behavior)
            {
                behavior.Update(dust);
            }
            else
            {
                dust.velocity *= 0.94f;
                dust.rotation += 0.03f;
                dust.scale *= 0.97f;
                if (dust.scale < 0.08f)
                    dust.active = false;
            }

            dust.position += dust.velocity;

            float r = dust.color.R / 255f * dust.scale * 0.4f;
            float g = dust.color.G / 255f * dust.scale * 0.25f;
            float b = dust.color.B / 255f * dust.scale * 0.08f;
            Lighting.AddLight(dust.position, r, g, b);

            return false;
        }

        public override bool PreDraw(Dust dust)
        {
            SpriteBatch sb = Main.spriteBatch;
            Texture2D tex = MagnumOpus.Common.Systems.VFX.Core.MagnumTextureRegistry.GetSoftGlow();
            if (tex == null) return false;

            Vector2 drawPos = dust.position - Main.screenPosition;
            Vector2 origin = new Vector2(tex.Width / 2f, tex.Height / 2f);

            float wiggle = (float)Math.Sin(Main.GameUpdateCount * 0.2f + dust.position.Y * 0.02f) * 0.12f;

            // Layer 1: Outer flame glow
            Color outerColor = dust.color with { A = 0 } * 0.25f;
            sb.Draw(tex, drawPos, null, outerColor, dust.rotation + wiggle, origin,
                dust.scale * 2.0f, SpriteEffects.None, 0f);

            // Layer 2: Mid flame body
            Color midColor = Color.Lerp(dust.color, EroicaPalette.Flame, 0.4f) with { A = 0 } * 0.45f;
            sb.Draw(tex, drawPos, null, midColor, dust.rotation, origin,
                dust.scale * 1.2f, SpriteEffects.None, 0f);

            // Layer 3: Hot core
            Color coreColor = EroicaPalette.HotCore with { A = 0 } * 0.6f;
            sb.Draw(tex, drawPos, null, coreColor, dust.rotation - wiggle, origin,
                dust.scale * 0.5f, SpriteEffects.None, 0f);

            return false;
        }
    }
}
