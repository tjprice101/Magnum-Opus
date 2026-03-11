using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ModLoader;
using MagnumOpus.Content.OdeToJoy.Dusts;

namespace MagnumOpus.Content.OdeToJoy.Weapons.ThePollinator.Dusts
{
    /// <summary>
    /// Soft golden pollen cloud mote — gentle upward drift with horizontal wander.
    /// Used for pollen trails, pollination aura, golden field ambience, mass bloom burst.
    /// Respects caller color if provided; defaults to golden pollen palette.
    /// </summary>
    public class PollenCloudDust : OdeToJoyDustBase
    {
        public override string Texture => "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftGlow";

        protected override float BaseScale => 0.12f;
        protected override Rectangle SpriteFrame => new(0, 0, 64, 64);
        protected override float LightIntensity => 0.25f;

        protected override void OnSpawnExtra(Dust dust)
        {
            if (dust.color == default)
                dust.color = Color.Lerp(OdeToJoyPalette.GoldenPollen, OdeToJoyPalette.WarmAmber, Main.rand.NextFloat());
        }

        protected override void UpdateBehavior(Dust dust)
        {
            dust.velocity *= 0.96f;
            dust.velocity.Y -= 0.02f; // Gentle upward drift

            // Slight horizontal wander
            float life = dust.alpha;
            dust.velocity.X += (float)Math.Sin(life * 0.2f + dust.position.X * 0.01f) * 0.04f;

            // Expand then fade
            if (dust.alpha < 8)
                dust.scale *= 1.02f;
            else
            {
                dust.scale *= 0.96f;
                dust.fadeIn *= 0.95f;
            }

            if (dust.scale < 0.03f || dust.alpha > 50)
                dust.active = false;
        }

        protected override void DrawExtraLayers(Dust dust, Texture2D mainTex, Vector2 drawPos, Vector2 origin, float fade)
        {
            // Wide soft outer glow — pollen haze effect
            Main.EntitySpriteDraw(mainTex, drawPos, dust.frame,
                (OdeToJoyPalette.GoldenPollen with { A = 0 }) * 0.15f * fade,
                0f, origin, dust.scale * 2.0f,
                SpriteEffects.None);
        }
    }
}
