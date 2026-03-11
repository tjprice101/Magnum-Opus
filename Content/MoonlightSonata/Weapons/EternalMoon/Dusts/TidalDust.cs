using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MagnumOpus.Common;
using ReLogic.Content;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.MoonlightSonata.Weapons.EternalMoon.Dusts
{
    /// <summary>
    /// Tidal Dust — the primary dust particle for the Eternal Moon weapon.
    /// A soft, luminous mote that drifts with gentle deceleration, colored in the lunar palette.
    /// </summary>
    public class TidalDust : ModDust
    {
        // Uses PointBloom for soft luminous mote — tinted violet-blue in GetAlpha
        public override string Texture => "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/PointBloom";

        public override void OnSpawn(Dust dust)
        {
            dust.noGravity = true;
            dust.noLight = false;
            dust.alpha = 80;
            dust.scale *= 0.6f;
        }

        public override bool Update(Dust dust)
        {
            dust.position += dust.velocity;
            dust.velocity *= 0.95f;
            dust.scale -= 0.015f;
            dust.alpha += 3;
            dust.rotation += dust.velocity.X * 0.04f;

            if (dust.scale < 0.1f || dust.alpha > 250)
            {
                dust.active = false;
            }

            // Moonlight palette lighting: soft violet/blue glow
            float brightness = dust.scale * 0.4f;
            Lighting.AddLight(dust.position, new Vector3(0.35f, 0.15f, 0.55f) * brightness);

            return false;
        }

        public override Color? GetAlpha(Dust dust, Color lightColor)
        {
            return new Color(138, 43, 226, dust.alpha);
        }

        public override bool PreDraw(Dust dust)
        {
            // PointBloom is a glow texture on black background — must use additive blending.
            // Cap draw scale: PointBloom is 2160px, max 300px on screen.
            const float MaxDrawScale = 300f / 2160f; // ~0.139
            float drawScale = MathHelper.Min(dust.scale, MaxDrawScale);

            var tex = ModContent.Request<Texture2D>(Texture, AssetRequestMode.ImmediateLoad).Value;
            if (tex == null) return false;

            Vector2 origin = tex.Size() * 0.5f;
            Vector2 pos = dust.position - Main.screenPosition;
            float alpha = (255 - dust.alpha) / 255f;
            Color drawColor = GetAlpha(dust, Color.White) ?? Color.White;

            var sb = Main.spriteBatch;
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, SamplerState.LinearClamp,
                DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

            sb.Draw(tex, pos, null, drawColor * alpha, dust.rotation, origin, drawScale, SpriteEffects.None, 0f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

            return false;
        }
    }
}
