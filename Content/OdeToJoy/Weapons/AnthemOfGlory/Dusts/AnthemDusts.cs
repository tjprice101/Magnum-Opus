using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ModLoader;
using MagnumOpus.Content.OdeToJoy.Dusts;

namespace MagnumOpus.Content.OdeToJoy.Weapons.AnthemOfGlory.Dusts
{
    /// <summary>
    /// Radiant crescendo spark — intensifies with channeling progression.
    /// Used for beam sparkles, crescendo aura, victory fanfare rings.
    /// Bright golden core with warm amber edge glow and gentle upward drift.
    /// Respects caller color if provided; defaults to BloomGold/RadiantAmber.
    /// </summary>
    public class CrescendoSparkDust : OdeToJoyDustBase
    {
        public override string Texture => "MagnumOpus/Assets/VFX Asset Library/Theme Specific/Ode to Joy/Particles/OJ Energy Flare";

        protected override float BaseScale => 0.14f;
        protected override Rectangle SpriteFrame => new(0, 0, 128, 128);
        protected override float LightIntensity => 0.3f;

        protected override void OnSpawnExtra(Dust dust)
        {
            if (dust.color == default)
                dust.color = Color.Lerp(OdeToJoyPalette.GoldenPollen, OdeToJoyPalette.WarmAmber, Main.rand.NextFloat());
            dust.rotation = Main.rand.NextFloat(MathHelper.TwoPi);
        }

        protected override void UpdateBehavior(Dust dust)
        {
            dust.velocity *= 0.94f;
            dust.velocity.Y -= 0.02f; // Gentle upward drift like rising notes

            // Harmonic shimmer — slight oscillating brightness via scale
            float shimmer = (float)Math.Sin(dust.alpha * 0.3f + dust.rotation * 2f) * 0.02f;
            dust.scale += shimmer;

            dust.rotation += 0.05f;
            dust.scale *= 0.965f;
            dust.fadeIn *= 0.95f;

            if (dust.scale < 0.03f || dust.alpha > 50)
                dust.active = false;
        }

        protected override void DrawExtraLayers(Dust dust, Texture2D mainTex, Vector2 drawPos, Vector2 origin, float fade)
        {
            // Warm amber outer halo — crescendo radiance
            Main.EntitySpriteDraw(mainTex, drawPos, dust.frame,
                (OdeToJoyPalette.WarmAmber with { A = 0 }) * 0.22f * fade,
                -dust.rotation * 0.3f, origin, dust.scale * 1.5f,
                SpriteEffects.None);
        }
    }

    /// <summary>
    /// Victory fanfare note — jubilant trailing music note particle.
    /// Used for glory notes, complete hymn-style celebrations.
    /// Bright white-gold core with festive petal-pink undertone.
    /// Respects caller color if provided; defaults to NoteColors cycle.
    /// </summary>
    public class FanfareNoteDust : OdeToJoyDustBase
    {
        public override string Texture => "MagnumOpus/Assets/VFX Asset Library/Theme Specific/Ode to Joy/Particles/OJ Music Note";

        protected override float BaseScale => 0.12f;
        protected override Rectangle SpriteFrame => new(0, 0, 128, 128);
        protected override float LightIntensity => 0.2f;

        private static readonly Color[] s_noteColors =
        {
            OdeToJoyPalette.GoldenPollen,
            OdeToJoyPalette.WarmAmber,
            OdeToJoyPalette.PetalPink,
            OdeToJoyPalette.SunlightYellow
        };

        protected override void OnSpawnExtra(Dust dust)
        {
            if (dust.color == default)
                dust.color = s_noteColors[Main.rand.Next(s_noteColors.Length)];
            dust.rotation = Main.rand.NextFloat(MathHelper.TwoPi);
        }

        protected override void UpdateBehavior(Dust dust)
        {
            dust.velocity *= 0.93f;
            dust.velocity.Y -= 0.03f; // Music notes float upward

            // Gentle wobble — like a note drifting on air
            float wobble = (float)Math.Sin(dust.alpha * 0.2f + dust.position.X * 0.01f) * 0.15f;
            dust.velocity.X += wobble;

            dust.rotation += 0.06f * Math.Sign(dust.velocity.X + 0.01f);
            dust.scale *= 0.97f;
            dust.fadeIn *= 0.94f;

            if (dust.scale < 0.025f || dust.alpha > 55)
                dust.active = false;
        }

        protected override void DrawExtraLayers(Dust dust, Texture2D mainTex, Vector2 drawPos, Vector2 origin, float fade)
        {
            // Jubilant golden glow halo
            Main.EntitySpriteDraw(mainTex, drawPos, dust.frame,
                (OdeToJoyPalette.GoldenPollen with { A = 0 }) * 0.18f * fade,
                -dust.rotation * 0.4f, origin, dust.scale * 1.4f,
                SpriteEffects.None);
        }
    }
}
