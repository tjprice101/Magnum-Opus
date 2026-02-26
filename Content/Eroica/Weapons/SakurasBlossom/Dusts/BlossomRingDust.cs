using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace MagnumOpus.Content.Eroica.Weapons.SakurasBlossom.Dusts
{
    /// <summary>
    /// Expanding bloom rings — hollow ring dust that expands outward from impact
    /// and combo transition points. Stationary, scale grows linearly, opacity fades.
    /// Phase-dependent coloring. 2-layer PreDraw: outer diffuse overshoot + main ring.
    /// </summary>
    public class BlossomRingDust : ModDust
    {
        public override string Texture => "MagnumOpus/Assets/Particles/GlowingHalo2";

        public override void OnSpawn(Dust dust)
        {
            dust.alpha = 0;
            dust.fadeIn = 0f;
            dust.noGravity = true;
            dust.noLight = false;
            dust.frame = new Rectangle(0, 0, 64, 64);
            dust.scale = 0.1f;
            dust.velocity = Vector2.Zero;
        }

        public override Color? GetAlpha(Dust dust, Color lightColor)
        {
            return dust.color * dust.fadeIn;
        }

        public override bool Update(Dust dust)
        {
            dust.alpha++;

            if (dust.customData is BlossomRingBehavior behavior)
            {
                float lifeProgress = (float)dust.alpha / behavior.Lifetime;

                // Scale grows using bloom unfurl curve
                float expandProgress = SBEasings.BloomUnfurl(lifeProgress);
                dust.scale = behavior.MaxScale * expandProgress;

                // Opacity fades as ring expands
                dust.fadeIn = (1f - lifeProgress) * behavior.FadePower;

                // Slow rotation
                dust.rotation += 0.02f;

                if (dust.alpha > behavior.Lifetime || dust.fadeIn < 0.02f)
                    dust.active = false;
            }
            else
            {
                float lifeProgress = (float)dust.alpha / 25f;
                dust.scale = 1.2f * lifeProgress;
                dust.fadeIn = 1f - lifeProgress;
                dust.rotation += 0.02f;
                if (dust.alpha > 25) dust.active = false;
            }

            if (!dust.noLight)
            {
                float lightIntensity = dust.fadeIn * Math.Min(dust.scale, 1f);
                Lighting.AddLight(dust.position, dust.color.ToVector3() * 0.3f * lightIntensity);
            }

            return false;
        }

        public override bool PreDraw(Dust dust)
        {
            var tex = Texture2D.Value;
            Vector2 drawPos = dust.position - Main.screenPosition;
            Vector2 origin = new Vector2(32, 32);

            // Layer 1: Outer diffuse ring — slightly overshot for bloom effect
            Main.EntitySpriteDraw(tex, drawPos, dust.frame,
                (dust.color with { A = 0 }) * 0.20f * dust.fadeIn,
                dust.rotation, origin, dust.scale * 1.3f,
                SpriteEffects.None);

            // Layer 2: Main ring
            Main.EntitySpriteDraw(tex, drawPos, dust.frame,
                (dust.color with { A = 0 }) * 0.40f * dust.fadeIn,
                dust.rotation, origin, dust.scale,
                SpriteEffects.None);

            return false;
        }
    }
}
