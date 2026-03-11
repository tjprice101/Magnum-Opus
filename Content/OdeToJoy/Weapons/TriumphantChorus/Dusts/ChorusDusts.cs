using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ModLoader;
using MagnumOpus.Content.OdeToJoy.Dusts;

namespace MagnumOpus.Content.OdeToJoy.Weapons.TriumphantChorus.Dusts
{
    /// <summary>
    /// Chorus voice spark — harmonic vocal energy particle.
    /// Used for summoning burst, ambient voice particles, ensemble burst, harmonic blast trail.
    /// Takes voice color from caller (Soprano=Gold, Alto=Rose, Tenor=Amber, Bass=Green).
    /// Respects caller color; defaults to BloomGold.
    /// </summary>
    public class ChorusVoiceDust : OdeToJoyDustBase
    {
        public override string Texture => "MagnumOpus/Assets/Particles Asset Library/MusicNote";

        protected override float BaseScale => 0.12f;
        protected override Rectangle SpriteFrame => new(0, 0, 128, 128);
        protected override float LightIntensity => 0.22f;

        protected override void OnSpawnExtra(Dust dust)
        {
            if (dust.color == default)
                dust.color = OdeToJoyPalette.GoldenPollen;
            dust.rotation = Main.rand.NextFloat(MathHelper.TwoPi);
        }

        protected override void UpdateBehavior(Dust dust)
        {
            dust.velocity *= 0.93f;
            dust.velocity.Y -= 0.02f; // Musical notes float upward

            // Harmonic vibration — rhythmic pulse
            float vibrate = (float)Math.Sin(dust.alpha * 0.3f + dust.rotation * 2f) * 0.012f;
            dust.scale += vibrate;

            dust.rotation += 0.05f;
            dust.scale *= 0.965f;
            dust.fadeIn *= 0.95f;

            if (dust.scale < 0.025f || dust.alpha > 50)
                dust.active = false;
        }

        protected override void DrawExtraLayers(Dust dust, Texture2D mainTex, Vector2 drawPos, Vector2 origin, float fade)
        {
            // Voice-colored harmonic glow
            Main.EntitySpriteDraw(mainTex, drawPos, dust.frame,
                (dust.color with { A = 0 }) * 0.2f * fade,
                -dust.rotation * 0.4f, origin, dust.scale * 1.4f,
                SpriteEffects.None);
        }
    }
}
