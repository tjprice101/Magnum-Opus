using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ModLoader;
using MagnumOpus.Content.OdeToJoy.Dusts;

namespace MagnumOpus.Content.OdeToJoy.Weapons.ThornboundReckoning.Dusts
{
    /// <summary>
    /// Sharp thorn fragment dust with angular tumble, green-gold gradient.
    /// Spawns on melee impacts and swing arcs for Thornbound Reckoning.
    /// </summary>
    public class ThornburstDust : OdeToJoyDustBase
    {
        public override string Texture => "MagnumOpus/Assets/VFX Asset Library/Theme Specific/Ode to Joy/Particles/OJ Thorn Fragment";

        protected override float BaseScale => 0.18f;
        protected override Rectangle SpriteFrame => new(0, 0, 128, 128);
        protected override float LightIntensity => 0.3f;

        protected override void OnSpawnExtra(Dust dust)
        {
            // Give each thorn a random initial rotation for angular tumble
            dust.rotation = Main.rand.NextFloat(MathHelper.TwoPi);
        }

        protected override void UpdateBehavior(Dust dust)
        {
            dust.velocity *= 0.92f;

            // Fast angular tumble — sharp debris feel
            dust.rotation += dust.velocity.Length() * 0.08f * (dust.rotation > MathHelper.Pi ? -1f : 1f);

            if (dust.alpha > 8)
            {
                dust.scale *= 0.95f;
                dust.fadeIn *= 0.94f;
            }

            if (dust.scale < 0.04f || dust.alpha > 40)
                dust.active = false;
        }

        protected override void DrawExtraLayers(Dust dust, Texture2D mainTex, Vector2 drawPos, Vector2 origin, float fade)
        {
            // Counter-rotating outer glow for depth
            Main.EntitySpriteDraw(mainTex, drawPos, dust.frame,
                (OdeToJoyPalette.LeafGreen with { A = 0 }) * 0.25f * fade,
                -dust.rotation * 0.4f, origin, dust.scale * 1.3f,
                SpriteEffects.None);
        }
    }

    /// <summary>
    /// Liquid sap droplet dust — emerald green glow with gravity.
    /// Drips from blade during vine-themed swings.
    /// </summary>
    public class VineSapDust : OdeToJoyDustBase
    {
        public override string Texture => "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftGlow";

        protected override float BaseScale => 0.12f;
        protected override Rectangle SpriteFrame => new(0, 0, 64, 64);
        protected override float LightIntensity => 0.25f;

        protected override void OnSpawnExtra(Dust dust)
        {
            dust.noGravity = false; // Sap drips with gravity
            dust.color = Color.Lerp(OdeToJoyPalette.DeepForest, OdeToJoyPalette.VerdantGreen, Main.rand.NextFloat());
        }

        protected override void UpdateBehavior(Dust dust)
        {
            dust.velocity.X *= 0.96f;
            dust.velocity.Y += 0.12f; // Gravity pull

            // Expand briefly then shrink (liquid drop behavior)
            if (dust.alpha < 8)
                dust.scale *= 1.02f;
            else
            {
                dust.scale *= 0.96f;
                dust.fadeIn *= 0.95f;
            }

            if (dust.scale < 0.03f || dust.alpha > 45)
                dust.active = false;
        }
    }
}
