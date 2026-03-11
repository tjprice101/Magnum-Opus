using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MagnumOpus.Common;
using ReLogic.Content;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.MoonlightSonata.Weapons.ResurrectionOfTheMoon.Dusts
{
    /// <summary>
    /// Comet dust for Resurrection of the Moon — shifts from gold-white to deep violet
    /// over its lifetime to match the comet's cooling gradient.
    /// </summary>
    public class CometDust : ModDust
    {
        // Uses PointBloom for bright comet core — gradient coloring in Update()
        public override string Texture => "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/PointBloom";

        public override void OnSpawn(Dust dust)
        {
            dust.noGravity = true;
            dust.noLight = false;
            dust.scale = 0.9f + Main.rand.NextFloat(0.6f);
            dust.alpha = 60;
        }

        public override bool Update(Dust dust)
        {
            dust.position += dust.velocity;
            dust.velocity *= 0.94f;
            dust.scale -= 0.018f;
            dust.alpha += 3;
            dust.rotation += 0.1f;

            // Cooling gradient: start white-gold, shift through violet to deep space
            float life = dust.alpha / 255f;
            Color goldWhite = new(235, 230, 255);
            Color cometViolet = new(180, 120, 255);
            Color deepSpace = new(50, 20, 100);

            Color current;
            if (life < 0.5f)
                current = Color.Lerp(goldWhite, cometViolet, life * 2f);
            else
                current = Color.Lerp(cometViolet, deepSpace, (life - 0.5f) * 2f);

            dust.color = current;
            Lighting.AddLight(dust.position, current.ToVector3() * 0.35f * dust.scale);

            if (dust.scale < 0.1f || dust.alpha >= 255)
            {
                dust.active = false;
            }

            return false;
        }

        public override bool PreDraw(Dust dust)
        {
            // PointBloom is 2160px — cap draw scale so it never exceeds 300px
            const float MaxDrawScale = 300f / 2160f; // ~0.139
            float drawScale = MathHelper.Min(dust.scale, MaxDrawScale);

            var tex = ModContent.Request<Texture2D>(Texture, AssetRequestMode.ImmediateLoad).Value;
            Vector2 origin = tex.Size() * 0.5f;
            Vector2 pos = dust.position - Main.screenPosition;
            float alpha = (255 - dust.alpha) / 255f;

            // PointBloom is a glow texture on black background — must use additive blending
            // to avoid rendering the black as visible. Switch to TrueAdditive, draw, restore.
            var sb = Main.spriteBatch;
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, SamplerState.LinearClamp,
                DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

            sb.Draw(tex, pos, null, dust.color * alpha, dust.rotation, origin, drawScale, SpriteEffects.None, 0f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

            return false; // Prevent vanilla from drawing at uncapped scale
        }
    }
}
