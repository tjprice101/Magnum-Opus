using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ModLoader;
using MagnumOpus.Content.OdeToJoy.Dusts;

namespace MagnumOpus.Content.OdeToJoy.Weapons.RoseThornChainsaw.Dusts
{
    /// <summary>
    /// Hot metal-cutting sparks flung from the chainsaw blade.
    /// Fast, angular, amber/gold with slight gravity — like grinding metal.
    /// </summary>
    public class ChainsawSparkDust : OdeToJoyDustBase
    {
        public override string Texture => "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftGlow";

        protected override float BaseScale => 0.1f;
        protected override Rectangle SpriteFrame => new(0, 0, 64, 64);
        protected override float LightIntensity => 0.35f;

        protected override void OnSpawnExtra(Dust dust)
        {
            dust.noGravity = false; // Sparks arc downward
            dust.color = Color.Lerp(OdeToJoyPalette.WarmAmber, OdeToJoyPalette.GoldenPollen, Main.rand.NextFloat());
            dust.rotation = Main.rand.NextFloat(MathHelper.TwoPi);
        }

        protected override void UpdateBehavior(Dust dust)
        {
            dust.velocity.X *= 0.95f;
            dust.velocity.Y += 0.08f; // Light gravity — sparks arc

            // Rapid shrink — brief hot spark lifetime
            dust.scale *= 0.93f;
            dust.fadeIn *= 0.9f;

            // Fast jitter rotation — grinding feel
            dust.rotation += dust.velocity.Length() * 0.12f;

            if (dust.scale < 0.02f || dust.alpha > 35)
                dust.active = false;
        }

        protected override void DrawExtraLayers(Dust dust, Texture2D mainTex, Vector2 drawPos, Vector2 origin, float fade)
        {
            // Hot white core glow — spark intensity
            Main.EntitySpriteDraw(mainTex, drawPos, dust.frame,
                (OdeToJoyPalette.WhiteBloom with { A = 0 }) * 0.4f * fade,
                dust.rotation, origin, dust.scale * 0.5f,
                SpriteEffects.None);
        }
    }

    /// <summary>
    /// Shredded rose petal chip — light pink fragments torn by chainsaw teeth.
    /// Flutter-drifts with lateral wobble and slow fade.
    /// </summary>
    public class RosePetalChipDust : OdeToJoyDustBase
    {
        public override string Texture => "MagnumOpus/Assets/VFX Asset Library/Theme Specific/Ode to Joy/Projectiles/OJ Rose Petal";

        protected override float BaseScale => 0.15f;
        protected override Rectangle SpriteFrame => new(0, 0, 128, 128);
        protected override float LightIntensity => 0.2f;

        protected override void OnSpawnExtra(Dust dust)
        {
            dust.rotation = Main.rand.NextFloat(MathHelper.TwoPi);
            dust.color = Color.Lerp(OdeToJoyPalette.RosePink, OdeToJoyPalette.PetalPink, Main.rand.NextFloat());
        }

        protected override void UpdateBehavior(Dust dust)
        {
            dust.velocity *= 0.94f;
            dust.velocity.Y += 0.03f; // Very light gravity — petals drift

            // Lateral flutter wobble — shredded petal tumbling in air
            float life = dust.alpha;
            dust.velocity.X += (float)Math.Sin(life * 0.3f + dust.rotation) * 0.15f;

            // Slow spin — gentle tumble
            dust.rotation += 0.06f * (dust.rotation > MathHelper.Pi ? -1f : 1f);

            dust.scale *= 0.97f;
            dust.fadeIn *= 0.96f;

            if (dust.scale < 0.03f || dust.alpha > 50)
                dust.active = false;
        }

        protected override void DrawExtraLayers(Dust dust, Texture2D mainTex, Vector2 drawPos, Vector2 origin, float fade)
        {
            // Soft pink outer glow
            Main.EntitySpriteDraw(mainTex, drawPos, dust.frame,
                (OdeToJoyPalette.PetalPink with { A = 0 }) * 0.2f * fade,
                -dust.rotation * 0.6f, origin, dust.scale * 1.4f,
                SpriteEffects.None);
        }
    }
}
