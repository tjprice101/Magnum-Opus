using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MagnumOpus.Content.Eroica.Weapons.SakurasBlossom.Dusts
{
    /// <summary>
    /// Directional petal wind streaks — elongated sakura-colored speed lines during swings.
    /// Follows SLP WindLine pattern: rotation follows velocity, Y-scale shrinks to create
    /// vanishing streak, velocity decays. Phase-dependent coloring.
    /// PreDraw: Elongated {A=0} additive draw with optional white core.
    /// </summary>
    public class PetalWindLine : ModDust
    {
        public override string Texture => "MagnumOpus/Assets/SandboxLastPrism/Pixel/Flare";

        public override void OnSpawn(Dust dust)
        {
            dust.alpha = 0;
            dust.fadeIn = 1f;
            dust.noGravity = true;
            dust.noLight = true;
            dust.customData = null;
        }

        public override bool Update(Dust dust)
        {
            if (dust.customData == null)
                dust.customData = SBDustBehaviorUtil.WindLine();

            if (dust.alpha == 0 && dust.customData is PetalWindLineBehavior wlb0)
                wlb0.InitialVelLength = dust.velocity.Length();

            dust.rotation = dust.velocity.ToRotation();

            if (dust.customData is PetalWindLineBehavior wlb)
            {
                if (dust.alpha >= wlb.TimeToStartShrink)
                    wlb.Vec2Scale = new Vector2(wlb.Vec2Scale.X, wlb.Vec2Scale.Y * wlb.ShrinkYPower);

                dust.velocity *= wlb.VelFadePower;

                if (wlb.Vec2Scale.Y <= 0.07f || dust.fadeIn <= 0.02f)
                    dust.active = false;

                dust.alpha++;

                if (dust.alpha >= wlb.KillEarlyTime)
                    dust.active = false;
            }

            dust.position += dust.velocity;

            if (!dust.noLight)
                Lighting.AddLight(dust.position, dust.color.ToVector3() * 0.3f * dust.scale);

            return false;
        }

        public override bool PreDraw(Dust dust)
        {
            var tex = Texture2D.Value;
            Vector2 drawPos = dust.position - Main.screenPosition;
            Vector2 origin = tex.Size() / 2f;

            if (dust.customData is PetalWindLineBehavior wlb)
            {
                Vector2 scale = dust.scale * new Vector2(wlb.Vec2Scale.X, wlb.Vec2Scale.Y * 0.5f);

                // Main colored streak
                Main.spriteBatch.Draw(tex, drawPos, null,
                    dust.color with { A = 0 }, dust.rotation, origin, scale,
                    SpriteEffects.None, 0f);

                // White core for brightness
                if (wlb.DrawWhiteCore)
                {
                    Main.spriteBatch.Draw(tex, drawPos, null,
                        Color.White with { A = 0 } * 0.6f, dust.rotation, origin, scale * 0.45f,
                        SpriteEffects.None, 0f);
                }
            }

            return false;
        }
    }
}
