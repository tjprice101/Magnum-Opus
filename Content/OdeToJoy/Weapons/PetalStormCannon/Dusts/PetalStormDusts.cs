using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ModLoader;
using MagnumOpus.Content.OdeToJoy.Dusts;

namespace MagnumOpus.Content.OdeToJoy.Weapons.PetalStormCannon.Dusts
{
    /// <summary>
    /// Whirling storm petal — caught in wind with curving trajectory and spin.
    /// Used for vortex zones, hurricane debris, cluster trails, petal bomb bursts.
    /// Respects caller color (for seasonal cycling); defaults to PetalPink/BloomGold.
    /// </summary>
    public class StormPetalDust : OdeToJoyDustBase
    {
        public override string Texture => "MagnumOpus/Assets/VFX Asset Library/Theme Specific/Ode to Joy/Particles/OJ Rose Petal";

        protected override float BaseScale => 0.16f;
        protected override Rectangle SpriteFrame => new(0, 0, 128, 128);
        protected override float LightIntensity => 0.25f;

        protected override void OnSpawnExtra(Dust dust)
        {
            if (dust.color == default)
                dust.color = Color.Lerp(OdeToJoyPalette.PetalPink, OdeToJoyPalette.GoldenPollen, Main.rand.NextFloat());
            dust.rotation = Main.rand.NextFloat(MathHelper.TwoPi);
        }

        protected override void UpdateBehavior(Dust dust)
        {
            dust.velocity *= 0.95f;

            // Wind-caught curving — tangential drift
            float life = dust.alpha;
            float curve = (float)Math.Sin(life * 0.25f + dust.rotation) * 0.2f;
            dust.velocity = dust.velocity.RotatedBy(curve * 0.05f);

            // Spin — like a petal caught in a vortex
            dust.rotation += 0.08f * Math.Sign(dust.velocity.X + 0.01f);

            dust.scale *= 0.96f;
            dust.fadeIn *= 0.95f;

            if (dust.scale < 0.03f || dust.alpha > 50)
                dust.active = false;
        }

        protected override void DrawExtraLayers(Dust dust, Texture2D mainTex, Vector2 drawPos, Vector2 origin, float fade)
        {
            // Soft pink/gold outer glow
            Main.EntitySpriteDraw(mainTex, drawPos, dust.frame,
                (OdeToJoyPalette.PetalPink with { A = 0 }) * 0.2f * fade,
                -dust.rotation * 0.5f, origin, dust.scale * 1.3f,
                SpriteEffects.None);
        }
    }
}
