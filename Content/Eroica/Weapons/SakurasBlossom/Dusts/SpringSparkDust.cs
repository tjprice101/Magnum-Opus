using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace MagnumOpus.Content.Eroica.Weapons.SakurasBlossom.Dusts
{
    /// <summary>
    /// 4-point star sparkles — sharp sparks that rapidly decelerate with scale twinkling.
    /// Fast rotation creates a spinning star effect. Random color from sakura palette.
    /// 3-layer PreDraw: outer glow, inner core at counter-rotation, faint halo.
    /// </summary>
    public class SpringSparkDust : ModDust
    {
        public override string Texture => "MagnumOpus/Assets/SandboxLastPrism/Orbs/SoftGlow64";

        public override void OnSpawn(Dust dust)
        {
            dust.alpha = 0;
            dust.fadeIn = 0f;
            dust.noGravity = true;
            dust.noLight = false;
            dust.frame = new Rectangle(0, 0, 64, 64);
            dust.scale *= 0.3f;
            // Random color from sakura palette
            dust.color = SBColorInfo.RandomDustColor();
        }

        public override Color? GetAlpha(Dust dust, Color lightColor)
        {
            return dust.color * dust.fadeIn;
        }

        public override bool Update(Dust dust)
        {
            dust.alpha++;

            if (dust.customData is SpringSparkBehavior behavior)
            {
                float lifeProgress = (float)dust.alpha / behavior.Lifetime;

                // Rapid deceleration
                dust.velocity *= behavior.VelDecay;
                dust.position += dust.velocity;

                // Fast spinning rotation
                dust.rotation += behavior.RotationSpeed;

                // Scale twinkling with lifetime fade
                float twinkle = SBEasings.Twinkle(dust.alpha, behavior.PhaseOffset);
                float lifetimeScale = lifeProgress < 0.2f ? lifeProgress / 0.2f
                    : 1f - (lifeProgress - 0.2f) / 0.8f;
                dust.scale = behavior.BaseScale * lifetimeScale * twinkle;

                // Quick ignite, steady fade
                if (dust.alpha < 2)
                    dust.fadeIn = dust.alpha / 2f;
                else
                    dust.fadeIn = MathHelper.Clamp(1f - lifeProgress, 0f, 1f);

                if (dust.alpha > behavior.Lifetime || dust.fadeIn < 0.02f || dust.scale < 0.03f)
                    dust.active = false;
            }
            else
            {
                dust.position += dust.velocity;
                dust.velocity *= 0.92f;
                dust.rotation += 0.15f;
                dust.scale *= 0.95f;
                dust.fadeIn = MathHelper.Clamp(1f - (float)dust.alpha / 20f, 0f, 1f);
                if (dust.alpha > 20) dust.active = false;
            }

            if (!dust.noLight)
            {
                float lightIntensity = dust.fadeIn * Math.Min(dust.scale * 2f, 1f);
                Lighting.AddLight(dust.position, dust.color.ToVector3() * 0.3f * lightIntensity);
            }

            return false;
        }

        public override bool PreDraw(Dust dust)
        {
            var tex = Texture2D.Value;
            Vector2 drawPos = dust.position - Main.screenPosition;
            Vector2 origin = new Vector2(32, 32);

            // Layer 1: Outer glow halo
            Main.EntitySpriteDraw(tex, drawPos, dust.frame,
                (dust.color with { A = 0 }) * 0.20f * dust.fadeIn,
                dust.rotation * 0.3f, origin, dust.scale * 2.0f,
                SpriteEffects.None);

            // Layer 2: Inner core at counter-rotation for star twinkle depth
            Color coreColor = Color.Lerp(dust.color, Color.White, 0.5f);
            Main.EntitySpriteDraw(tex, drawPos, dust.frame,
                (coreColor with { A = 0 }) * 0.55f * dust.fadeIn,
                -dust.rotation * 0.7f, origin, dust.scale * 0.6f,
                SpriteEffects.None);

            // Layer 3: Main body
            Main.EntitySpriteDraw(tex, drawPos, dust.frame,
                (dust.color with { A = 0 }) * 0.45f * dust.fadeIn,
                dust.rotation, origin, dust.scale * 1.0f,
                SpriteEffects.None);

            return false;
        }
    }
}
