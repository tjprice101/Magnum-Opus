using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace MagnumOpus.Content.Eroica.Weapons.SakurasBlossom.Dusts
{
    /// <summary>
    /// The signature sakura dust — cherry blossom petals that tumble and drift downward.
    /// Sinusoidal lateral flutter, gentle gravity, slow rotation, breathing scale pulse.
    /// 3-layer PreDraw: outer glow → main body → inner white core at counter-rotation.
    /// Color shifts from PetalWhite at spawn → BloomPink body → BudCrimson at death.
    /// </summary>
    public class SakuraPetalDust : ModDust
    {
        public override string Texture => "MagnumOpus/Assets/SandboxLastPrism/Orbs/SoftGlow64";

        public override void OnSpawn(Dust dust)
        {
            dust.alpha = 0;
            dust.fadeIn = 0f;
            dust.noGravity = true;
            dust.noLight = false;
            dust.frame = new Rectangle(0, 0, 64, 64);
            dust.scale *= 0.35f;
        }

        public override Color? GetAlpha(Dust dust, Color lightColor)
        {
            return dust.color * dust.fadeIn;
        }

        public override bool Update(Dust dust)
        {
            dust.alpha++;

            if (dust.customData is SakuraPetalBehavior behavior)
            {
                float lifeProgress = (float)dust.alpha / behavior.Lifetime;

                // Color shift: PetalWhite → BloomPink → BudCrimson
                if (lifeProgress < 0.3f)
                {
                    dust.color = Color.Lerp(SBColorInfo.PetalWhite, SBColorInfo.BloomPink, lifeProgress / 0.3f);
                }
                else if (lifeProgress < 0.7f)
                {
                    dust.color = Color.Lerp(SBColorInfo.BloomPink, SBColorInfo.BlossomCore, (lifeProgress - 0.3f) / 0.4f);
                }
                else
                {
                    dust.color = Color.Lerp(SBColorInfo.BlossomCore, SBColorInfo.BudCrimson, (lifeProgress - 0.7f) / 0.3f);
                }

                // Sinusoidal lateral flutter — perpendicular to velocity
                float drift = SBEasings.PetalDriftCurve(lifeProgress, behavior.DriftFrequency) * behavior.DriftAmplitude;
                Vector2 perpendicular = new Vector2(-dust.velocity.Y, dust.velocity.X);
                if (perpendicular != Vector2.Zero)
                    perpendicular.Normalize();
                dust.position += perpendicular * drift;

                // Gentle gravity — petals fall like real cherry blossoms
                dust.velocity.Y += behavior.Gravity;
                dust.velocity *= 0.98f;

                dust.position += dust.velocity;

                // Slow tumbling rotation
                dust.rotation += behavior.RotationSpeed * (1f + MathF.Sin(dust.alpha * 0.1f) * 0.3f);

                // Breathing scale pulse
                float pulse = SBEasings.BreathingPulse(dust.alpha, 0.15f, 0.1f);
                float lifetimeScale;
                if (lifeProgress < 0.15f)
                    lifetimeScale = lifeProgress / 0.15f; // Grow in
                else if (lifeProgress < 0.7f)
                    lifetimeScale = 1f; // Hold
                else
                    lifetimeScale = 1f - (lifeProgress - 0.7f) / 0.3f; // Shrink out
                dust.scale = behavior.BaseScale * lifetimeScale * pulse;

                // Fade control
                if (dust.alpha < 4)
                    dust.fadeIn = dust.alpha / 4f;
                else if (lifeProgress < 0.75f)
                    dust.fadeIn = 1f;
                else
                    dust.fadeIn = MathHelper.Clamp(1f - (lifeProgress - 0.75f) / 0.25f, 0f, 1f);

                if (dust.alpha > behavior.Lifetime || dust.fadeIn < 0.02f)
                    dust.active = false;
            }
            else
            {
                // Default fallback
                dust.position += dust.velocity;
                dust.velocity *= 0.97f;
                dust.velocity.Y += 0.03f;
                dust.rotation += 0.03f;
                dust.scale *= 0.98f;
                dust.fadeIn = MathHelper.Clamp(1f - (float)dust.alpha / 40f, 0f, 1f);
                if (dust.alpha > 40) dust.active = false;
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

            // Layer 1: Outer glow — diffuse sakura haze
            Main.EntitySpriteDraw(tex, drawPos, dust.frame,
                (dust.color with { A = 0 }) * 0.25f * dust.fadeIn,
                dust.rotation, origin, dust.scale * 2.0f,
                SpriteEffects.None);

            // Layer 2: Main petal body
            Main.EntitySpriteDraw(tex, drawPos, dust.frame,
                (dust.color with { A = 0 }) * 0.55f * dust.fadeIn,
                dust.rotation, origin, dust.scale * 1.0f,
                SpriteEffects.None);

            // Layer 3: Inner white core vein — counter-rotation for depth
            Color coreColor = Color.Lerp(dust.color, Color.White, 0.65f);
            Main.EntitySpriteDraw(tex, drawPos, dust.frame,
                (coreColor with { A = 0 }) * 0.45f * dust.fadeIn,
                -dust.rotation * 0.6f, origin, dust.scale * 0.45f,
                SpriteEffects.None);

            return false;
        }
    }
}
