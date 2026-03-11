using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ModLoader;
using MagnumOpus.Content.OdeToJoy.Dusts;

namespace MagnumOpus.Content.OdeToJoy.Weapons.ElysianVerdict.Dusts
{
    /// <summary>
    /// Elysian judgment mote — prismatic golden particle with weighty descent.
    /// Used for orb shimmer, verdict explosion, elysian mark aura, radiance aura.
    /// Shifts between BloomGold and CrimsonEdge for Paradise Lost mode.
    /// Respects caller color; defaults to BloomGold/PureJoyWhite.
    /// </summary>
    public class ElysianJudgmentDust : OdeToJoyDustBase
    {
        public override string Texture => "MagnumOpus/Assets/VFX Asset Library/Theme Specific/Ode to Joy/Particles/OJ Energy Flare";

        protected override float BaseScale => 0.14f;
        protected override Rectangle SpriteFrame => new(0, 0, 128, 128);
        protected override float LightIntensity => 0.28f;

        protected override void OnSpawnExtra(Dust dust)
        {
            if (dust.color == default)
                dust.color = Color.Lerp(OdeToJoyPalette.GoldenPollen, OdeToJoyPalette.WhiteBloom, Main.rand.NextFloat() * 0.4f);
            dust.rotation = Main.rand.NextFloat(MathHelper.TwoPi);
        }

        protected override void UpdateBehavior(Dust dust)
        {
            dust.velocity *= 0.94f;
            dust.velocity.Y -= 0.015f; // Very gentle float

            // Solemn pulsation — steady, weighty
            float pulse = (float)Math.Sin(dust.alpha * 0.25f) * 0.01f;
            dust.scale += pulse;

            dust.rotation += 0.04f;
            dust.scale *= 0.96f;
            dust.fadeIn *= 0.95f;

            if (dust.scale < 0.025f || dust.alpha > 50)
                dust.active = false;
        }

        protected override void DrawExtraLayers(Dust dust, Texture2D mainTex, Vector2 drawPos, Vector2 origin, float fade)
        {
            // Radiant golden judgment halo
            Main.EntitySpriteDraw(mainTex, drawPos, dust.frame,
                (OdeToJoyPalette.GoldenPollen with { A = 0 }) * 0.22f * fade,
                -dust.rotation * 0.3f, origin, dust.scale * 1.5f,
                SpriteEffects.None);
        }
    }

    /// <summary>
    /// Elysian mark glow — ambient mark indicator that lingers on enemies.
    /// Subtle golden-white mote that rises gently with minimal movement.
    /// Used for ElysianMarkDebuff and ElysianBurnDebuff on-NPC visuals.
    /// </summary>
    public class ElysianMarkGlowDust : OdeToJoyDustBase
    {
        public override string Texture => "MagnumOpus/Assets/VFX Asset Library/Theme Specific/Ode to Joy/Particles/OJ Thorn Fragment";

        protected override float BaseScale => 0.09f;
        protected override Rectangle SpriteFrame => new(0, 0, 128, 128);
        protected override float LightIntensity => 0.18f;

        protected override void OnSpawnExtra(Dust dust)
        {
            if (dust.color == default)
                dust.color = Color.Lerp(new Color(255, 200, 50), new Color(255, 255, 240), Main.rand.NextFloat());
            dust.rotation = Main.rand.NextFloat(MathHelper.TwoPi);
        }

        protected override void UpdateBehavior(Dust dust)
        {
            dust.velocity *= 0.9f;
            dust.velocity.Y -= 0.025f; // Gentle ascent

            dust.rotation += 0.03f;
            dust.scale *= 0.96f;
            dust.fadeIn *= 0.93f;

            if (dust.scale < 0.02f || dust.alpha > 50)
                dust.active = false;
        }

        protected override void DrawExtraLayers(Dust dust, Texture2D mainTex, Vector2 drawPos, Vector2 origin, float fade)
        {
            Main.EntitySpriteDraw(mainTex, drawPos, dust.frame,
                (new Color(255, 230, 150) with { A = 0 }) * 0.15f * fade,
                -dust.rotation * 0.4f, origin, dust.scale * 1.3f,
                SpriteEffects.None);
        }
    }
}
