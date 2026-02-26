using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using System;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Graphics.Shaders;
using ReLogic.Content;
using static Terraria.ModLoader.ModContent;
using MagnumOpus.Content.SandboxLastPrism.Systems;

namespace MagnumOpus.Content.SandboxLastPrism.Dusts
{

    public class LineSpark : ModDust
    {
        public override string Texture => "MagnumOpus/Content/SandboxLastPrism/Dusts/Textures/GlowLine1Black";


        public override void OnSpawn(Dust dust)
        {
            dust.noGravity = true;
            dust.alpha = 255;
            dust.frame = new Rectangle(0, 0, 128, 27);
        }

        public override Color? GetAlpha(Dust dust, Color lightColor)
        {
            return dust.color;
        }

        public override bool Update(Dust dust)
        {

            if (dust.customData != null)
            {
                if (dust.customData is LineSparkBehavior behavior)
                {

                    dust.rotation = dust.velocity.ToRotation();

                    dust.velocity *= behavior.base_velFadePower;
                    dust.position += dust.velocity;

                    if (dust.fadeIn > behavior.base_killEarlyTime)
                        dust.active = false;

                    if (dust.fadeIn < behavior.base_timeToStartShrink)
                    {
                        dust.scale *= behavior.base_preShrinkPower;
                    }
                    else
                    {
                        dust.scale *= behavior.base_postShrinkPower;
                    }


                    if (behavior.base_shouldFadeColor)
                        dust.alpha = (int)(dust.alpha * behavior.base_colorFadePower);


                    if (dust.scale < 0.03f || dust.scale < 0.03)
                    {
                        dust.active = false;
                    }

                    dust.fadeIn++;
                }
            }
            else
            {
                dust.rotation = dust.velocity.ToRotation();

                dust.velocity *= 0.97f;
                dust.position += dust.velocity;

                if (dust.noLight == false)
                {
                    if (dust.fadeIn > 60)
                        dust.active = false;
                }
                else
                {
                    if (dust.fadeIn < 40)
                    {
                        dust.scale *= 0.99f;
                    }
                    else
                    {
                        dust.scale *= 0.97f;
                    }

                    if (dust.scale < 0.03f)
                    {
                        dust.active = false;
                    }
                }

                dust.fadeIn++;
            }

            return false;
        }


        public override bool PreDraw(Dust dust)
        {
            float opacity = (dust.alpha / 255f);

            Color White = Color.White with { A = 0 } * opacity;
            Texture2D tex = Texture2D.Value;

            if (dust.customData != null)
            {
                if (dust.customData is LineSparkBehavior behavior)
                {
                    Vector2 scale = behavior.Vector2DrawScale * dust.scale;

                    Main.spriteBatch.Draw(tex, dust.position - Main.screenPosition, null, dust.color with { A = 0 } * opacity, dust.rotation, tex.Size() / 2f, scale * 1f, SpriteEffects.None, 0f);

                    if (behavior.DrawWhiteCore)
                        Main.spriteBatch.Draw(tex, dust.position - Main.screenPosition, null, White with { A = 0 } * 1f, dust.rotation, tex.Size() / 2f, scale * 0.5f, SpriteEffects.None, 0f);
                }
            }
            else
            {
                Main.spriteBatch.Draw(tex, dust.position - Main.screenPosition, null, dust.color with { A = 0 } * opacity, dust.rotation, tex.Size() / 2f, dust.scale * 1f, SpriteEffects.None, 0f);
                Main.spriteBatch.Draw(tex, dust.position - Main.screenPosition, null, White with { A = 0 }, dust.rotation, tex.Size() / 2f, dust.scale * 0.65f, SpriteEffects.None, 0f);
            }
            return false;
        }

    }

    public class LineSparkBehavior
    {
        public Behavior behaviorToUse = Behavior.Base;
        public enum Behavior
        {
            Base = 0,
            PlaceHolder1 = 1,
            PlaceHolder2 = 2,
            PlaceHolder3 = 3,
        }

        public bool DrawWhiteCore = true;
        public Vector2 Vector2DrawScale = new Vector2(1f, 1f);

        //Base
        public float base_velFadePower = 0.97f;
        public float base_preShrinkPower = 0.99f;
        public float base_postShrinkPower = 0.92f;
        public int base_timeToStartShrink = 40;
        public int base_killEarlyTime = 60;

        public bool base_shouldFadeColor = false;
        public float base_colorFadePower = 0.93f;
    }

}
