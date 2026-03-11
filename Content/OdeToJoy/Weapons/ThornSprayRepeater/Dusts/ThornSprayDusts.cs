using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ModLoader;
using MagnumOpus.Content.OdeToJoy.Dusts;

namespace MagnumOpus.Content.OdeToJoy.Weapons.ThornSprayRepeater.Dusts
{
    /// <summary>
    /// Sharp crystalline thorn fragment — angular green-gold crystal shard with fast rotation.
    /// Used for thorn projectile trails, detonation shards, embed effects.
    /// </summary>
    public class CrystallineThornSparkDust : OdeToJoyDustBase
    {
        public override string Texture => "MagnumOpus/Assets/VFX Asset Library/Theme Specific/Ode to Joy/Particles/OJ Thorn Fragment";

        protected override float BaseScale => 0.14f;
        protected override Rectangle SpriteFrame => new(0, 0, 128, 128);
        protected override float LightIntensity => 0.3f;

        protected override void OnSpawnExtra(Dust dust)
        {
            if (dust.color == default)
                dust.color = Color.Lerp(OdeToJoyPalette.LeafGreen, OdeToJoyPalette.GoldenPollen, Main.rand.NextFloat());
            dust.rotation = Main.rand.NextFloat(MathHelper.TwoPi);
        }

        protected override void UpdateBehavior(Dust dust)
        {
            dust.velocity *= 0.93f;
            dust.rotation += dust.velocity.Length() * 0.1f;

            dust.scale *= 0.94f;
            dust.fadeIn *= 0.93f;

            if (dust.scale < 0.03f || dust.alpha > 40)
                dust.active = false;
        }

        protected override void DrawExtraLayers(Dust dust, Texture2D mainTex, Vector2 drawPos, Vector2 origin, float fade)
        {
            Main.EntitySpriteDraw(mainTex, drawPos, dust.frame,
                (OdeToJoyPalette.VerdantGreen with { A = 0 }) * 0.25f * fade,
                -dust.rotation * 0.5f, origin, dust.scale * 1.2f,
                SpriteEffects.None);
        }
    }

    /// <summary>
    /// Warm golden bloom burst particle — expanding then fading.
    /// Used for Bloom Reload VFX, accent sparkles, pollen burst moments.
    /// </summary>
    public class ThornBloomBurstDust : OdeToJoyDustBase
    {
        public override string Texture => "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftGlow";

        protected override float BaseScale => 0.15f;
        protected override Rectangle SpriteFrame => new(0, 0, 64, 64);
        protected override float LightIntensity => 0.35f;

        protected override void OnSpawnExtra(Dust dust)
        {
            if (dust.color == default)
                dust.color = Color.Lerp(OdeToJoyPalette.GoldenPollen, OdeToJoyPalette.WarmAmber, Main.rand.NextFloat());
            dust.rotation = Main.rand.NextFloat(MathHelper.TwoPi);
        }

        protected override void UpdateBehavior(Dust dust)
        {
            dust.velocity *= 0.94f;

            // Brief expand then shrink — bloom burst feel
            if (dust.alpha < 6)
                dust.scale *= 1.03f;
            else
            {
                dust.scale *= 0.95f;
                dust.fadeIn *= 0.94f;
            }

            if (dust.scale < 0.03f || dust.alpha > 45)
                dust.active = false;
        }

        protected override void DrawExtraLayers(Dust dust, Texture2D mainTex, Vector2 drawPos, Vector2 origin, float fade)
        {
            // Warm golden outer halo
            Main.EntitySpriteDraw(mainTex, drawPos, dust.frame,
                (OdeToJoyPalette.GoldenPollen with { A = 0 }) * 0.2f * fade,
                0f, origin, dust.scale * 1.6f,
                SpriteEffects.None);
        }
    }
}
