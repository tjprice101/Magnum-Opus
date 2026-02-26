using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace MagnumOpus.Content.Eroica.Weapons.SakurasBlossom.Dusts
{
    /// <summary>
    /// Burning petal fragments — small ember-like particles that start bright sakura
    /// and cool to crimson. Fast initial velocity from swing direction, slight gravity,
    /// tumbling rotation. Color: PetalWhite → BloomPink → BudCrimson (cooling ember).
    /// 2-layer PreDraw: outer ember glow + hot core with white push.
    /// </summary>
    public class SakuraEmberDust : ModDust
    {
        public override string Texture => "MagnumOpus/Assets/SandboxLastPrism/Orbs/SoftGlow";

        public override void OnSpawn(Dust dust)
        {
            dust.alpha = 0;
            dust.fadeIn = 0f;
            dust.noGravity = true;
            dust.noLight = false;
            dust.frame = new Rectangle(0, 0, 32, 32);
            dust.scale *= 0.3f;
        }

        public override Color? GetAlpha(Dust dust, Color lightColor)
        {
            return dust.color * dust.fadeIn;
        }

        public override bool Update(Dust dust)
        {
            dust.alpha++;

            if (dust.customData is SakuraEmberBehavior behavior)
            {
                float lifeProgress = (float)dust.alpha / behavior.Lifetime;
                float coolProgress = SBEasings.EmberCool(lifeProgress);

                // Color: hot PetalWhite → BloomPink → cold BudCrimson
                if (coolProgress > 0.6f)
                    dust.color = Color.Lerp(SBColorInfo.BloomPink, SBColorInfo.PetalWhite, (coolProgress - 0.6f) / 0.4f);
                else if (coolProgress > 0.2f)
                    dust.color = Color.Lerp(SBColorInfo.BudCrimson, SBColorInfo.BloomPink, (coolProgress - 0.2f) / 0.4f);
                else
                    dust.color = SBColorInfo.BudCrimson * (0.5f + coolProgress * 2.5f);

                // Slight gravity
                dust.velocity.Y += behavior.Gravity;
                dust.velocity *= behavior.VelDecay;
                dust.position += dust.velocity;

                // Tumbling rotation
                dust.rotation += behavior.RotationSpeed * (dust.velocity.X > 0 ? 1f : -1f);

                // Scale: starts at full, shrinks in last 40%
                float lifetimeScale = lifeProgress < 0.6f ? 1f :
                    1f - (lifeProgress - 0.6f) / 0.4f;
                dust.scale = behavior.BaseScale * lifetimeScale;

                // Quick ignite, then fade
                if (dust.alpha < 2)
                    dust.fadeIn = dust.alpha / 2f;
                else if (lifeProgress < 0.6f)
                    dust.fadeIn = 1f;
                else
                    dust.fadeIn = 1f - (lifeProgress - 0.6f) / 0.4f;

                if (dust.alpha > behavior.Lifetime || dust.fadeIn < 0.02f)
                    dust.active = false;
            }
            else
            {
                dust.position += dust.velocity;
                dust.velocity.Y += 0.05f;
                dust.velocity *= 0.95f;
                dust.rotation += 0.1f;
                dust.scale *= 0.97f;
                dust.fadeIn = MathHelper.Clamp(1f - (float)dust.alpha / 25f, 0f, 1f);
                if (dust.alpha > 25) dust.active = false;
            }

            if (!dust.noLight)
            {
                float lightIntensity = dust.fadeIn * Math.Min(dust.scale * 2f, 1f);
                Lighting.AddLight(dust.position, dust.color.ToVector3() * 0.35f * lightIntensity);
            }

            return false;
        }

        public override bool PreDraw(Dust dust)
        {
            var tex = Texture2D.Value;
            Vector2 drawPos = dust.position - Main.screenPosition;
            Vector2 origin = tex.Size() / 2f;

            // Layer 1: Outer ember glow
            Main.EntitySpriteDraw(tex, drawPos, null,
                (dust.color with { A = 0 }) * 0.35f * dust.fadeIn,
                dust.rotation, origin, dust.scale * 1.8f,
                SpriteEffects.None);

            // Layer 2: Hot core with white push
            Color coreColor = Color.Lerp(dust.color, Color.White, 0.45f);
            Main.EntitySpriteDraw(tex, drawPos, null,
                (coreColor with { A = 0 }) * 0.55f * dust.fadeIn,
                dust.rotation * 0.8f, origin, dust.scale * 0.7f,
                SpriteEffects.None);

            return false;
        }
    }
}
