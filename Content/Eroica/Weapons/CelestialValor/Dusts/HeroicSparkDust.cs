using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.VFX;

namespace MagnumOpus.Content.Eroica.Weapons.CelestialValor.Dusts
{
    /// <summary>
    /// Heroic Spark — a fast-moving velocity-aligned spark that leaves
    /// a brief trail of gold-white light. Used on hit impacts and
    /// combo specials. Elongates along its velocity for a dynamic streaking effect.
    /// 
    /// Follows the SandboxLastPrism LineSpark pattern: draws as an elongated
    /// line aligned to velocity with gradient fade.
    /// </summary>
    public class HeroicSparkDust : ModDust
    {
        public override string Texture => "MagnumOpus/Assets/Particles/ThinSparkleFlare";

        public override void OnSpawn(Dust dust)
        {
            dust.noGravity = true;
            dust.noLight = false;
        }

        public override bool Update(Dust dust)
        {
            if (dust.customData is CVDustBehaviorUtil.ValorSparkBehavior behavior)
            {
                behavior.Update(dust);
            }
            else
            {
                dust.velocity *= 0.92f;
                dust.scale *= 0.95f;
                if (dust.scale < 0.05f)
                    dust.active = false;
            }

            dust.position += dust.velocity;

            // Bright white-gold light
            float intensity = dust.scale * 0.6f;
            Lighting.AddLight(dust.position, intensity * 1.0f, intensity * 0.85f, intensity * 0.3f);

            return false;
        }

        public override bool PreDraw(Dust dust)
        {
            SpriteBatch sb = Main.spriteBatch;
            Texture2D tex = MagnumTextureRegistry.GetSoftGlow();
            if (tex == null) return false;

            Vector2 drawPos = dust.position - Main.screenPosition;
            Vector2 origin = new Vector2(tex.Width / 2f, tex.Height / 2f);

            // Align rotation to velocity
            float rot = dust.velocity.LengthSquared() > 0.25f
                ? dust.velocity.ToRotation()
                : dust.rotation;

            // Elongation factor based on speed
            float speed = dust.velocity.Length();
            float elongation = MathHelper.Clamp(speed * 0.3f, 1f, 3f);

            // Layer 1: Outer trail glow (elongated, colored)
            Color outerColor = Color.Lerp(EroicaPalette.Scarlet, EroicaPalette.Gold, 0.3f) with { A = 0 } * 0.4f;
            sb.Draw(tex, drawPos, null, outerColor, rot, origin,
                new Vector2(dust.scale * elongation * 1.5f, dust.scale * 0.4f), SpriteEffects.None, 0f);

            // Layer 2: Core bright line
            Color coreColor = EroicaPalette.HotCore with { A = 0 } * 0.8f;
            sb.Draw(tex, drawPos, null, coreColor, rot, origin,
                new Vector2(dust.scale * elongation * 0.8f, dust.scale * 0.2f), SpriteEffects.None, 0f);

            return false;
        }
    }
}
