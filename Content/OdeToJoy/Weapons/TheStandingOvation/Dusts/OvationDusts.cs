using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ModLoader;
using MagnumOpus.Content.OdeToJoy.Dusts;

namespace MagnumOpus.Content.OdeToJoy.Weapons.TheStandingOvation.Dusts
{
    /// <summary>
    /// Applause flash spark — bright celebratory sparkle for spectator attacks.
    /// Used for summoning burst, applause waves, standing rush, minion death.
    /// Respects caller color; defaults to golden ApplauseFlash.
    /// </summary>
    public class ApplauseSparkDust : OdeToJoyDustBase
    {
        public override string Texture => "MagnumOpus/Assets/VFX Asset Library/Theme Specific/Ode to Joy/Particles/OJ Blossom Sparkle";

        protected override float BaseScale => 0.13f;
        protected override Rectangle SpriteFrame => new(0, 0, 128, 128);
        protected override float LightIntensity => 0.25f;

        protected override void OnSpawnExtra(Dust dust)
        {
            if (dust.color == default)
                dust.color = Color.Lerp(OdeToJoyPalette.GoldenPollen, OdeToJoyPalette.WarmAmber, Main.rand.NextFloat());
            dust.rotation = Main.rand.NextFloat(MathHelper.TwoPi);
        }

        protected override void UpdateBehavior(Dust dust)
        {
            dust.velocity *= 0.94f;

            // Quick flash — fast initial brightness then fade
            dust.rotation += 0.06f;
            dust.scale *= 0.96f;
            dust.fadeIn *= 0.95f;

            if (dust.scale < 0.025f || dust.alpha > 50)
                dust.active = false;
        }

        protected override void DrawExtraLayers(Dust dust, Texture2D mainTex, Vector2 drawPos, Vector2 origin, float fade)
        {
            Main.EntitySpriteDraw(mainTex, drawPos, dust.frame,
                (OdeToJoyPalette.WarmAmber with { A = 0 }) * 0.2f * fade,
                -dust.rotation * 0.3f, origin, dust.scale * 1.4f,
                SpriteEffects.None);
        }
    }

    /// <summary>
    /// Rose petal drift — soft pink petal that flutters gracefully.
    /// Used for ThrownRose petal trail and rose impact burst.
    /// Respects caller color; defaults to PetalPink/RosePink.
    /// </summary>
    public class RosePetalDriftDust : OdeToJoyDustBase
    {
        public override string Texture => "MagnumOpus/Assets/VFX Asset Library/Theme Specific/Ode to Joy/Projectiles/OJ Rose Petal";

        protected override float BaseScale => 0.11f;
        protected override Rectangle SpriteFrame => new(0, 0, 128, 128);
        protected override float LightIntensity => 0.18f;

        protected override void OnSpawnExtra(Dust dust)
        {
            if (dust.color == default)
                dust.color = Color.Lerp(OdeToJoyPalette.PetalPink, OdeToJoyPalette.RosePink, Main.rand.NextFloat());
            dust.rotation = Main.rand.NextFloat(MathHelper.TwoPi);
        }

        protected override void UpdateBehavior(Dust dust)
        {
            dust.velocity *= 0.94f;
            dust.velocity.Y -= 0.02f; // Gentle float

            // Flutter wobble
            float wobble = (float)Math.Sin(dust.alpha * 0.25f + dust.position.X * 0.01f) * 0.12f;
            dust.velocity.X += wobble;

            dust.rotation += 0.05f * Math.Sign(dust.velocity.X + 0.01f);
            dust.scale *= 0.965f;
            dust.fadeIn *= 0.94f;

            if (dust.scale < 0.02f || dust.alpha > 55)
                dust.active = false;
        }

        protected override void DrawExtraLayers(Dust dust, Texture2D mainTex, Vector2 drawPos, Vector2 origin, float fade)
        {
            Main.EntitySpriteDraw(mainTex, drawPos, dust.frame,
                (OdeToJoyPalette.PetalPink with { A = 0 }) * 0.18f * fade,
                -dust.rotation * 0.5f, origin, dust.scale * 1.3f,
                SpriteEffects.None);
        }
    }
}
