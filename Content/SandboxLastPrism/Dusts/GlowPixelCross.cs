using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using System;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Graphics.Shaders;
using static Terraria.ModLoader.ModContent;
using MagnumOpus.Content.SandboxLastPrism.Systems;

namespace MagnumOpus.Content.SandboxLastPrism.Dusts
{
    public class GlowPixelCross : ModDust
    {
        public override string Texture => "MagnumOpus/Content/SandboxLastPrism/Dusts/Textures/PixelCrossMain";
        private Texture2D core;


        public override void Load() => core = (Texture2D)ModContent.Request<Texture2D>("MagnumOpus/Content/SandboxLastPrism/Dusts/Textures/PixelCrossInner");

        public override void Unload() => core = null;

        public override void OnSpawn(Dust dust)
        {
            dust.noGravity = true;
            dust.noLight = true;
            dust.frame = new Rectangle(0, 0, 68, 68);
        }

        public override Color? GetAlpha(Dust dust, Color lightColor)
        {
            return dust.color;
        }

        public override bool Update(Dust dust)
        {

            if (dust.customData != null)
            {
                if (dust.customData is GlowPixelCrossBehavior behavior)
                {

                    if (behavior.behaviorToUse == GlowPixelCrossBehavior.Behavior.Base)
                    {
                        dust.position += dust.velocity;
                        dust.rotation += dust.velocity.X * behavior.base_rotPower;

                        dust.velocity *= dust.fadeIn < behavior.base_timeBeforeSlow ? behavior.base_preSlowPower : behavior.base_postSlowPower;
                        if (dust.velocity.Length() < behavior.base_velToBeginShrink)
                        {
                            dust.scale *= behavior.base_fadePower;
                        }

                        if (dust.scale < 0.1f)
                        {
                            dust.active = false;
                        }

                        if (behavior.base_shouldFadeColor)
                            dust.color *= behavior.base_colorFadePower;

                        dust.fadeIn++;
                    }

                    else if (behavior.behaviorToUse == GlowPixelCrossBehavior.Behavior.PlaceHolder1)
                    {

                    }

                    else if (behavior.behaviorToUse == GlowPixelCrossBehavior.Behavior.PlaceHolder2)
                    {

                    }
                }
            }
            else
            {
                dust.position += dust.velocity;
                dust.rotation += dust.velocity.X * 0.15f;


                dust.velocity *= dust.fadeIn < 3 ? 0.99f : 0.92f;
                if (dust.velocity.Length() < 1f)
                {
                    dust.scale *= 0.9f;
                }


                if (dust.scale < 0.1f)
                {
                    dust.active = false;
                }

                dust.fadeIn++;
            }

            if (!dust.noLight)
                Lighting.AddLight(dust.position, dust.color.ToVector3() * 0.5f * dust.scale);

            return false;
        }


        public override bool PreDraw(Dust dust)
        {
            Texture2D Core = (Texture2D)ModContent.Request<Texture2D>("MagnumOpus/Content/SandboxLastPrism/Dusts/Textures/PixelCrossInner");
            Main.spriteBatch.Draw(Texture2D.Value, dust.position - Main.screenPosition, null, dust.color with { A = 0 }, dust.rotation, new Vector2(34f, 34f), dust.scale, SpriteEffects.None, 0f);

            Main.spriteBatch.Draw(Core, dust.position - Main.screenPosition, null, Color.White with { A = 0 }, dust.rotation, new Vector2(34, 34f), dust.scale * 0.5f, SpriteEffects.None, 0f);

            return false;
        }

    }

    public class GlowPixelCrossBehavior
    {
        public Behavior behaviorToUse = Behavior.Base;
        public enum Behavior
        {
            Base = 0,
            PlaceHolder1 = 1,
            PlaceHolder2 = 2,
            PlaceHolder3 = 3,
        }

        //Base
        public float base_rotPower = 0.15f;
        public int base_timeBeforeSlow = 3;
        public float base_preSlowPower = 0.99f;
        public float base_postSlowPower = 0.92f;
        public float base_velToBeginShrink = 1f;
        public float base_fadePower = 0.95f;

        public bool base_shouldFadeColor = false;
        public float base_colorFadePower = 0.93f;
    }
}
