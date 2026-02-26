using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace MagnumOpus.Content.Eroica.Weapons.SakurasBlossom.Dusts
{
    /// <summary>
    /// Bloom energy orbs — soft feathered circles of warm sakura pink glow.
    /// Used at blade tips, impact points, and spawn flashes.
    /// Rapid deceleration, gentle breathing scale pulse, configurable glow intensity.
    /// 3-layer PreDraw: outer diffuse → main body → inner white core.
    /// </summary>
    public class BlossomGlowOrb : ModDust
    {
        public override string Texture => "MagnumOpus/Assets/SandboxLastPrism/Orbs/feather_circle128PMA";

        public override void OnSpawn(Dust dust)
        {
            dust.alpha = 0;
            dust.fadeIn = 0f;
            dust.noGravity = true;
            dust.noLight = false;
            dust.frame = new Rectangle(0, 0, 128, 128);
            dust.scale *= 0.4f;
        }

        public override Color? GetAlpha(Dust dust, Color lightColor)
        {
            return dust.color * dust.fadeIn;
        }

        public override bool Update(Dust dust)
        {
            dust.alpha++;

            if (dust.customData is BlossomGlowOrbBehavior behavior)
            {
                float lifeProgress = (float)dust.alpha / behavior.Lifetime;

                // Color: bloom pink with subtle pulse toward petal white
                float colorPulse = MathF.Sin(dust.alpha * 0.15f) * 0.2f;
                dust.color = Color.Lerp(SBColorInfo.BloomPink, SBColorInfo.PetalWhite, 0.2f + colorPulse);

                // Rapid deceleration
                dust.velocity *= behavior.DecelerationPower;
                dust.position += dust.velocity;

                // Breathing scale pulse
                float pulse = SBEasings.BreathingPulse(dust.alpha, behavior.PulseFrequency, 0.1f);
                float lifetimeScale;
                if (lifeProgress < 0.15f)
                    lifetimeScale = SBEasings.BloomUnfurl(lifeProgress / 0.15f);
                else if (lifeProgress < 0.6f)
                    lifetimeScale = 1f;
                else
                    lifetimeScale = 1f - SBEasings.SmoothStep((lifeProgress - 0.6f) / 0.4f);
                dust.scale = behavior.BaseScale * lifetimeScale * pulse;

                // Slow rotation
                dust.rotation += 0.01f;

                // Fade
                if (dust.alpha < 3)
                    dust.fadeIn = dust.alpha / 3f;
                else if (lifeProgress < 0.6f)
                    dust.fadeIn = behavior.GlowIntensity;
                else
                    dust.fadeIn = behavior.GlowIntensity * (1f - (lifeProgress - 0.6f) / 0.4f);

                if (dust.alpha > behavior.Lifetime || dust.fadeIn < 0.02f)
                    dust.active = false;
            }
            else
            {
                dust.position += dust.velocity;
                dust.velocity *= 0.9f;
                dust.scale *= 0.96f;
                dust.fadeIn = MathHelper.Clamp(1f - (float)dust.alpha / 20f, 0f, 1f);
                if (dust.alpha > 20) dust.active = false;
            }

            if (!dust.noLight)
            {
                float lightIntensity = dust.fadeIn * Math.Min(dust.scale, 1f);
                Lighting.AddLight(dust.position, SBColorInfo.BloomPink.ToVector3() * 0.5f * lightIntensity);
            }

            return false;
        }

        public override bool PreDraw(Dust dust)
        {
            var tex = Texture2D.Value;
            Vector2 drawPos = dust.position - Main.screenPosition;
            Vector2 origin = new Vector2(64, 64);

            // Layer 1: Outer diffuse glow — wide, soft
            Main.EntitySpriteDraw(tex, drawPos, dust.frame,
                (dust.color with { A = 0 }) * 0.20f * dust.fadeIn,
                dust.rotation, origin, dust.scale * 2.2f,
                SpriteEffects.None);

            // Layer 2: Main body glow
            Main.EntitySpriteDraw(tex, drawPos, dust.frame,
                (dust.color with { A = 0 }) * 0.45f * dust.fadeIn,
                dust.rotation, origin, dust.scale * 1.0f,
                SpriteEffects.None);

            // Layer 3: Inner white-hot core
            Color coreColor = Color.Lerp(dust.color, Color.White, 0.7f);
            Main.EntitySpriteDraw(tex, drawPos, dust.frame,
                (coreColor with { A = 0 }) * 0.50f * dust.fadeIn,
                -dust.rotation * 0.3f, origin, dust.scale * 0.4f,
                SpriteEffects.None);

            return false;
        }
    }
}
