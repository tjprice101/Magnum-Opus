using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ModLoader;
using MagnumOpus.Content.OdeToJoy.Dusts;

namespace MagnumOpus.Content.OdeToJoy.Weapons.HymnOfTheVictorious.Dusts
{
    /// <summary>
    /// Hymnal verse spark — distinct per-verse identity through caller color.
    /// Used for all 4 verse bolt trails, complete hymn burst, gloria fragments.
    /// Crisp directional spark with subtle harmonic pulse.
    /// Respects caller color (verse-specific tint); defaults to BloomGold.
    /// </summary>
    public class HymnVerseDust : OdeToJoyDustBase
    {
        public override string Texture => "MagnumOpus/Assets/VFX Asset Library/Theme Specific/Ode to Joy/Particles/OJ Energy Flare";

        protected override float BaseScale => 0.13f;
        protected override Rectangle SpriteFrame => new(0, 0, 128, 128);
        protected override float LightIntensity => 0.25f;

        protected override void OnSpawnExtra(Dust dust)
        {
            if (dust.color == default)
                dust.color = OdeToJoyPalette.GoldenPollen;
            dust.rotation = Main.rand.NextFloat(MathHelper.TwoPi);
        }

        protected override void UpdateBehavior(Dust dust)
        {
            dust.velocity *= 0.93f;

            // Harmonic pulse — rhythmic scale oscillation
            float pulse = (float)Math.Sin(dust.alpha * 0.35f) * 0.015f;
            dust.scale += pulse;

            dust.rotation += 0.07f;
            dust.scale *= 0.96f;
            dust.fadeIn *= 0.95f;

            if (dust.scale < 0.025f || dust.alpha > 50)
                dust.active = false;
        }

        protected override void DrawExtraLayers(Dust dust, Texture2D mainTex, Vector2 drawPos, Vector2 origin, float fade)
        {
            // Warm golden halo adapts to verse color
            Main.EntitySpriteDraw(mainTex, drawPos, dust.frame,
                (dust.color with { A = 0 }) * 0.2f * fade,
                -dust.rotation * 0.3f, origin, dust.scale * 1.4f,
                SpriteEffects.None);
        }
    }

    /// <summary>
    /// Jubilant burn ember — warm rising flame particle for debuff visuals.
    /// Used for Jubilant Burn and Hymn Resonance on-NPC effects.
    /// Rises upward with slight flutter, warm gold/amber palette.
    /// </summary>
    public class JubilantEmberDust : OdeToJoyDustBase
    {
        public override string Texture => "MagnumOpus/Assets/VFX Asset Library/Theme Specific/Ode to Joy/Particles/OJ Thorn Fragment";

        protected override float BaseScale => 0.1f;
        protected override Rectangle SpriteFrame => new(0, 0, 128, 128);
        protected override float LightIntensity => 0.2f;

        protected override void OnSpawnExtra(Dust dust)
        {
            if (dust.color == default)
                dust.color = Color.Lerp(OdeToJoyPalette.WarmAmber, OdeToJoyPalette.SunlightYellow, Main.rand.NextFloat());
            dust.rotation = Main.rand.NextFloat(MathHelper.TwoPi);
        }

        protected override void UpdateBehavior(Dust dust)
        {
            dust.velocity *= 0.92f;
            dust.velocity.Y -= 0.04f; // Rising ember

            // Lateral flutter
            float flutter = (float)Math.Sin(dust.alpha * 0.3f + dust.position.X * 0.02f) * 0.1f;
            dust.velocity.X += flutter;

            dust.rotation += 0.04f;
            dust.scale *= 0.955f;
            dust.fadeIn *= 0.94f;

            if (dust.scale < 0.02f || dust.alpha > 55)
                dust.active = false;
        }

        protected override void DrawExtraLayers(Dust dust, Texture2D mainTex, Vector2 drawPos, Vector2 origin, float fade)
        {
            // Warm amber rising glow
            Main.EntitySpriteDraw(mainTex, drawPos, dust.frame,
                (OdeToJoyPalette.WarmAmber with { A = 0 }) * 0.18f * fade,
                -dust.rotation * 0.5f, origin, dust.scale * 1.3f,
                SpriteEffects.None);
        }
    }
}
