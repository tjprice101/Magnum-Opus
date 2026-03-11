using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ModLoader;
using MagnumOpus.Content.OdeToJoy.Dusts;

namespace MagnumOpus.Content.OdeToJoy.Weapons.FountainOfJoyousHarmony.Dusts
{
    /// <summary>
    /// Golden droplet spray — water-like particle with gentle gravity fallback.
    /// Used for fountain spray, geyser burst, droplet trails, splash impacts.
    /// Falls like water (noGravity = false by default). Caller can override.
    /// Respects caller color; defaults to DropletGold/BloomGold blend.
    /// </summary>
    public class FountainDropletDust : OdeToJoyDustBase
    {
        public override string Texture => "MagnumOpus/Assets/VFX Asset Library/Theme Specific/Ode to Joy/Particles/OJ Energy Flare";

        protected override float BaseScale => 0.12f;
        protected override Rectangle SpriteFrame => new(0, 0, 128, 128);
        protected override float LightIntensity => 0.22f;

        protected override void OnSpawnExtra(Dust dust)
        {
            if (dust.color == default)
                dust.color = Color.Lerp(OdeToJoyPalette.GoldenPollen, OdeToJoyPalette.SunlightYellow, Main.rand.NextFloat() * 0.5f);
            dust.rotation = Main.rand.NextFloat(MathHelper.TwoPi);
            dust.noGravity = false; // Falls like water by default
        }

        protected override void UpdateBehavior(Dust dust)
        {
            dust.velocity *= 0.96f;

            // Shimmer — slight sparkle wobble
            float shimmer = (float)Math.Sin(dust.alpha * 0.4f + dust.rotation) * 0.01f;
            dust.scale += shimmer;

            dust.rotation += 0.03f;
            dust.scale *= 0.965f;
            dust.fadeIn *= 0.95f;

            if (dust.scale < 0.025f || dust.alpha > 50)
                dust.active = false;
        }

        protected override void DrawExtraLayers(Dust dust, Texture2D mainTex, Vector2 drawPos, Vector2 origin, float fade)
        {
            // Warm golden water glow
            Main.EntitySpriteDraw(mainTex, drawPos, dust.frame,
                (OdeToJoyPalette.GoldenPollen with { A = 0 }) * 0.2f * fade,
                -dust.rotation * 0.3f, origin, dust.scale * 1.4f,
                SpriteEffects.None);
        }
    }

    /// <summary>
    /// Harmony field sparkle — gentle ambient mote for Harmony Zone aura.
    /// Floats gently upward with very slow fade. Used for tier glow and field sparkles.
    /// </summary>
    public class HarmonyFieldDust : OdeToJoyDustBase
    {
        public override string Texture => "MagnumOpus/Assets/VFX Asset Library/Theme Specific/Ode to Joy/Particles/OJ Thorn Fragment";

        protected override float BaseScale => 0.08f;
        protected override Rectangle SpriteFrame => new(0, 0, 128, 128);
        protected override float LightIntensity => 0.15f;

        protected override void OnSpawnExtra(Dust dust)
        {
            if (dust.color == default)
                dust.color = Color.Lerp(OdeToJoyPalette.SunlightYellow, OdeToJoyPalette.WhiteBloom, Main.rand.NextFloat() * 0.4f);
            dust.rotation = Main.rand.NextFloat(MathHelper.TwoPi);
        }

        protected override void UpdateBehavior(Dust dust)
        {
            dust.velocity *= 0.92f;
            dust.velocity.Y -= 0.015f; // Gentle float

            dust.rotation += 0.025f;
            dust.scale *= 0.97f;
            dust.fadeIn *= 0.94f;

            if (dust.scale < 0.02f || dust.alpha > 55)
                dust.active = false;
        }

        protected override void DrawExtraLayers(Dust dust, Texture2D mainTex, Vector2 drawPos, Vector2 origin, float fade)
        {
            Main.EntitySpriteDraw(mainTex, drawPos, dust.frame,
                (OdeToJoyPalette.SunlightYellow with { A = 0 }) * 0.15f * fade,
                -dust.rotation * 0.4f, origin, dust.scale * 1.3f,
                SpriteEffects.None);
        }
    }
}
