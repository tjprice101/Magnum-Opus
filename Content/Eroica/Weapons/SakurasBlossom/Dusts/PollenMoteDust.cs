using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace MagnumOpus.Content.Eroica.Weapons.SakurasBlossom.Dusts
{
    /// <summary>
    /// Golden rising pollen — tiny soft golden dots that rise upward with twinkling.
    /// Constant gentle upward velocity, horizontal sinusoidal drift, scale twinkling.
    /// 2-layer PreDraw: outer golden glow at 2x, bright core at 0.7x.
    /// Color: PollenGold shifting to SunlitPetal at end of life.
    /// </summary>
    public class PollenMoteDust : ModDust
    {
        public override string Texture => "MagnumOpus/Assets/SandboxLastPrism/Orbs/SoftGlow";

        public override void OnSpawn(Dust dust)
        {
            dust.alpha = 0;
            dust.fadeIn = 0f;
            dust.noGravity = true;
            dust.noLight = false;
            dust.frame = new Rectangle(0, 0, 32, 32);
            dust.scale *= 0.2f;
        }

        public override Color? GetAlpha(Dust dust, Color lightColor)
        {
            return dust.color * dust.fadeIn;
        }

        public override bool Update(Dust dust)
        {
            dust.alpha++;

            if (dust.customData is PollenMoteBehavior behavior)
            {
                float lifeProgress = (float)dust.alpha / behavior.Lifetime;

                // Color: PollenGold → SunlitPetal
                dust.color = Color.Lerp(SBColorInfo.PollenGold, SBColorInfo.SunlitPetal, lifeProgress);

                // Constant upward velocity with gradual slowdown
                float riseMult = SBEasings.PollenRise(lifeProgress);
                dust.velocity.Y = -behavior.RiseSpeed * riseMult;

                // Horizontal sinusoidal drift
                float drift = MathF.Sin(dust.alpha * 0.08f + dust.position.X * 0.01f) * behavior.DriftAmplitude;
                dust.velocity.X = drift * (1f - lifeProgress);

                dust.position += dust.velocity;

                // Scale twinkling
                float twinkle = SBEasings.Twinkle(dust.alpha, dust.position.X);
                float lifetimeScale = lifeProgress < 0.1f ? lifeProgress / 0.1f
                    : lifeProgress < 0.7f ? 1f
                    : 1f - (lifeProgress - 0.7f) / 0.3f;
                dust.scale = behavior.BaseScale * lifetimeScale * twinkle;

                // Fade
                if (dust.alpha < 3)
                    dust.fadeIn = dust.alpha / 3f;
                else if (lifeProgress < 0.7f)
                    dust.fadeIn = 1f;
                else
                    dust.fadeIn = MathHelper.Clamp(1f - (lifeProgress - 0.7f) / 0.3f, 0f, 1f);

                if (dust.alpha > behavior.Lifetime || dust.fadeIn < 0.02f)
                    dust.active = false;
            }
            else
            {
                dust.position += dust.velocity;
                dust.velocity.Y -= 0.02f;
                dust.velocity *= 0.98f;
                dust.scale *= 0.97f;
                dust.fadeIn = MathHelper.Clamp(1f - (float)dust.alpha / 30f, 0f, 1f);
                if (dust.alpha > 30) dust.active = false;
            }

            if (!dust.noLight)
            {
                float lightIntensity = dust.fadeIn * Math.Min(dust.scale * 2f, 1f);
                Lighting.AddLight(dust.position, SBColorInfo.PollenGold.ToVector3() * 0.25f * lightIntensity);
            }

            return false;
        }

        public override bool PreDraw(Dust dust)
        {
            var tex = Texture2D.Value;
            Vector2 drawPos = dust.position - Main.screenPosition;
            Vector2 origin = tex.Size() / 2f;

            // Layer 1: Outer golden glow (wide, diffuse)
            Main.EntitySpriteDraw(tex, drawPos, null,
                (dust.color with { A = 0 }) * 0.3f * dust.fadeIn,
                0f, origin, dust.scale * 2.0f,
                SpriteEffects.None);

            // Layer 2: Bright golden core
            Color coreColor = Color.Lerp(dust.color, Color.White, 0.3f);
            Main.EntitySpriteDraw(tex, drawPos, null,
                (coreColor with { A = 0 }) * 0.6f * dust.fadeIn,
                0f, origin, dust.scale * 0.7f,
                SpriteEffects.None);

            return false;
        }
    }
}
