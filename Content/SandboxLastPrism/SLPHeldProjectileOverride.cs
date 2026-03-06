using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Shaders;
using Terraria.Graphics;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.SandboxLastPrism.Dusts;
using MagnumOpus.Content.SandboxLastPrism.Systems;


namespace MagnumOpus.Content.SandboxLastPrism
{

    //This also handles the combined laser vfx
    public class SLPHeldProjectileOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.LastPrism);
        }

        //How far away from prism does laser start
        private float offsetDistance = 18f;

        float endRot = 0f;
        int timer = 0;
        public override bool PreAI(Projectile projectile)
        {
            if (projectile.ai[0] == 180f)
            {
                GetCombinedLaserInfo();

                drawCombinedLaser = true;

                for (int i = 0; i < 35; i++)
                {
                    Color col = Main.hslToRgb(Main.rand.NextFloat(0f, 1f), 1f, 0.5f);


                    Vector2 vel = projectile.velocity.SafeNormalize(Vector2.UnitX).RotatedByRandom(1.5f) * Main.rand.NextFloat(10f, 30f);

                    Dust p = Dust.NewDustPerfect(projectile.Center, ModContent.DustType<WindLine>(), vel, newColor: col, Scale: Main.rand.NextFloat(0.5f, 0.65f) * 1.5f);
                    p.customData = new WindLineBehavior(VelFadePower: 0.92f, TimeToStartShrink: 11, YScale: 0.5f);
                }

                SLPFlashSystem.SetCAFlashEffect(0.4f, 25, 1f, 0.75f, true);

                Main.player[projectile.owner].GetModPlayer<SLPScreenShakePlayer>().ScreenShakePower = 60f;

                combinedLaserStartBoostPower = 1f;

                //Sound
                SoundStyle style5 = new SoundStyle("Terraria/Sounds/Item_163") with { Volume = 0.5f, Pitch = .7f, MaxInstances = -1 };
                SoundEngine.PlaySound(style5, projectile.Center);
            }

            if (projectile.ai[0] > 180 && projectile.ai[0] < 195)
            {
                float pow = Main.player[projectile.owner].GetModPlayer<SLPScreenShakePlayer>().ScreenShakePower;
                Main.player[projectile.owner].GetModPlayer<SLPScreenShakePlayer>().ScreenShakePower = Math.Clamp(pow, 15f, 60f);
            }

            if (drawCombinedLaser)
            {
                if (timer % 1 == 0)
                {
                    float dist = projectile.ai[2];

                    for (int i = 0; i < dist * 0.75f; i += 125)
                    {
                        Vector2 pos = projectile.Center + new Vector2(i, 0f).RotatedBy(projectile.velocity.ToRotation());
                        float rot = projectile.velocity.ToRotation();


                        int dustColsLength = lpci.dustColors.Count;
                        Color rainbow = lpci.dustColors[Main.rand.Next(0, dustColsLength)];

                        if (Main.rand.NextBool(3))
                        {
                            Vector2 offset = rot.ToRotationVector2().RotatedBy(MathHelper.PiOver2) * Main.rand.NextFloat(-35, 35);
                            Vector2 vel = rot.ToRotationVector2().RotatedByRandom(0.05f) * Main.rand.NextFloat(2, 7) * 1.4f;

                            if (!Main.rand.NextBool(3))
                            {
                                Dust d = Dust.NewDustPerfect(pos + offset, ModContent.DustType<GlowFlare>(), vel * 2f, newColor: rainbow * 1f, Scale: Main.rand.NextFloat(0.5f, 1.5f) * 0.5f);
                                d.noLight = false;
                                d.customData = new GlowFlareBehavior(0.4f, 2.5f);
                            }
                            else
                            {
                                Dust d = Dust.NewDustPerfect(pos + offset, ModContent.DustType<GlowPixelCross>(), vel * 3.5f, newColor: rainbow * 1f, Scale: Main.rand.NextFloat(0.5f, 1.5f) * 0.35f);
                                d.noLight = false;
                                d.customData = SLPDustBehaviorUtil.AssignBehavior_GPCBase(rotPower: 0.17f, postSlowPower:0.84f, velToBeginShrink: 5f, fadePower: 0.9f, shouldFadeColor: false);
                            }
                        }

                    }
                }

                //End point dust
                if (timer % 1 == 0)
                {
                    Vector2 dustPos = projectile.Center + new Vector2(projectile.ai[2], 0f).RotatedBy(projectile.velocity.ToRotation());
                    for (int i = 0; i < 3 + Main.rand.Next(0, 2); i++)
                    {
                        Vector2 vel = Main.rand.NextVector2Circular(22f, 22f);

                        int dustColsLength = lpci.dustColors.Count;
                        Color rainbow = lpci.dustColors[Main.rand.Next(0, dustColsLength)];

                        Dust p = Dust.NewDustPerfect(dustPos, ModContent.DustType<WindLine>(), vel,
                            newColor: rainbow, Scale: Main.rand.NextFloat(0.5f, 0.65f) * 1f);
                        p.customData = new WindLineBehavior(VelFadePower: 0.92f, TimeToStartShrink: 5, YScale: 0.5f);


                        if (i == 0)
                        {
                            Dust p2 = Dust.NewDustPerfect(dustPos + vel * 3f, ModContent.DustType<SoftGlowDust>(), vel * 2f, newColor: rainbow * 0.85f, Scale: Main.rand.NextFloat(0.5f, 0.65f) * 0.35f);
                            p2.customData = SLPDustBehaviorUtil.AssignBehavior_SGDBase(overallAlpha: 0.1f);
                        }

                    }
                }

                //Light at end of laser
                Vector2 lightPos = projectile.Center + new Vector2(projectile.ai[2], 0f).RotatedBy(projectile.velocity.ToRotation());
                Lighting.AddLight(lightPos, Color.White.ToVector3() * 1.1f);

                endRot += 1.15f * (projectile.velocity.X > 0 ? 1f : -1f);

            }

            combinedLaserStartBoostPower = Math.Clamp(MathHelper.Lerp(combinedLaserStartBoostPower, -0.5f, 0.06f), 0f, 10f);

            timer++;

            return base.PreAI(projectile);
        }


        float combinedLaserStartBoostPower = 0f;
        float combinedLaserScale = 1f;
        bool drawCombinedLaser = false;
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            if (drawCombinedLaser)
            {
                #region BlackBorder
                Texture2D black = Mod.Assets.Request<Texture2D>("Assets/SandboxLastPrism/BlackWall").Value;

                if (Main.player[projectile.owner] == Main.LocalPlayer && false)
                {
                    Vector2 pos = projectile.Center - Main.screenPosition + new Vector2(0f, Main.player[projectile.owner].gfxOffY);

                    float opac = 0.06f;

                    Vector2 blackOrigin = new Vector2(black.Width / 2, 0);
                    Vector2 blackScale = new Vector2(5f, 5f) * 1f;

                    Main.EntitySpriteDraw(black, pos, null, Color.Black * opac, projectile.velocity.ToRotation(), blackOrigin, blackScale, 0f);
                    Main.EntitySpriteDraw(black, pos, null, Color.Black * opac, projectile.velocity.ToRotation() + MathHelper.Pi, blackOrigin, blackScale, 0f);
                }

                #endregion


                ModContent.GetInstance<SLPPixelationSystem>().QueueRenderAction(SLPRenderLayer.Dusts, () =>
                {
                    RainbowLaser(projectile);
                    RainbowSigil(projectile);
                });
            }


            Texture2D vanillaTex = TextureAssets.Projectile[projectile.type].Value;

            Vector2 drawPos = projectile.Center - Main.screenPosition + new Vector2(0f, 1f * Main.player[projectile.owner].gfxOffY);
            Rectangle sourceRectangle = vanillaTex.Frame(1, Main.projFrames[projectile.type], frameY: projectile.frame);
            Vector2 TexOrigin = sourceRectangle.Size() / 2f;

            SpriteEffects SE = projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            //Border
            Color[] rainbow = { Color.Red, Color.Orange, Color.Yellow, Color.Green, Color.DodgerBlue, Color.Violet };
            for (int i = 0; i < 6; i++)
            {
                float rot = i * (MathHelper.TwoPi / 6f);

                float prog = Math.Clamp(projectile.ai[0] / 180f, 0f, 1f);
                float dis = MathHelper.Lerp(15f, 3f, SLPEasings.easeOutQuad(prog));
                float scale = MathHelper.Lerp(2f, 1f, prog);

                Vector2 off = new Vector2(dis, 0f).RotatedBy(rot + (float)Main.timeForVisualEffects * 0.15f);
                Main.EntitySpriteDraw(vanillaTex, drawPos + off + new Vector2(0f, 0f), sourceRectangle,
                   rainbow[i] with { A = 0 } * 0.35f * SLPEasings.easeInSine(prog), projectile.rotation, TexOrigin, projectile.scale * 1.07f * scale, SE);
            }

            Main.EntitySpriteDraw(vanillaTex, drawPos + Main.rand.NextVector2CircularEdge(2f, 2f), sourceRectangle, Color.White with { A = 0 } * 0.1f, projectile.rotation, TexOrigin, projectile.scale * 1.1f, SE);
            Main.EntitySpriteDraw(vanillaTex, drawPos, sourceRectangle, Color.White with { A = 180 }, projectile.rotation, TexOrigin, projectile.scale, SE);

            //Not returning true and drawing manually because the last prism apparently doesn't respect gfxOffY
            return false;
        }

        Effect myEffect = null;
        Effect laserEffect = null;
        public void RainbowLaser(Projectile projectile)
        {
            float rot = projectile.velocity.ToRotation();

            Vector2 startPoint = projectile.Center + projectile.velocity.SafeNormalize(Vector2.UnitX) * offsetDistance;
            Vector2 endPoint = startPoint + new Vector2(projectile.ai[2] - offsetDistance, 0f).RotatedBy(rot);

            Vector2[] pos_arr = { startPoint, endPoint };
            float[] rot_arr = { rot, rot };

            Color StripColor(float progress) => Color.White;
            float StripWidth(float progress) => (120f * 1f) * combinedLaserScale + (combinedLaserStartBoostPower * 250f);

            VertexStrip vertexStrip1 = new VertexStrip();
            vertexStrip1.PrepareStrip(pos_arr, rot_arr, StripColor, StripWidth, -Main.screenPosition, includeBacksides: true);


            #region Params
            if (laserEffect == null)
                laserEffect = ModContent.Request<Effect>("MagnumOpus/Effects/SandboxLastPrism/Scroll/ComboLaserVertexGradient", AssetRequestMode.ImmediateLoad).Value;

            laserEffect.Parameters["WorldViewProjection"].SetValue(Main.GameViewMatrix.NormalizedTransformationmatrix);

            String GradLocation = "MagnumOpus/Assets/SandboxLastPrism/Gradients/";

            laserEffect.Parameters["onTex"].SetValue(ModContent.Request<Texture2D>("MagnumOpus/Assets/SandboxLastPrism/Trails/Clear/GlowTrailClear", AssetRequestMode.ImmediateLoad).Value);
            laserEffect.Parameters["gradientTex"].SetValue(ModContent.Request<Texture2D>(GradLocation + lpci.textureLocation, AssetRequestMode.ImmediateLoad).Value);
            laserEffect.Parameters["baseColor"].SetValue(Color.White.ToVector3() * 1f);
            laserEffect.Parameters["satPower"].SetValue(0.8f - (combinedLaserStartBoostPower * 0.8f));

            laserEffect.Parameters["sampleTexture1"].SetValue(SLPCommonTextures.ThinGlowLine.Value);
            laserEffect.Parameters["sampleTexture2"].SetValue(SLPCommonTextures.spark_06.Value);
            laserEffect.Parameters["sampleTexture3"].SetValue(SLPCommonTextures.Extra_196_Black.Value);
            laserEffect.Parameters["sampleTexture4"].SetValue(SLPCommonTextures.Trail5Loop.Value);

            laserEffect.Parameters["grad1Speed"].SetValue(lpci.grad1Speed);
            laserEffect.Parameters["grad2Speed"].SetValue(lpci.grad2Speed);
            laserEffect.Parameters["grad3Speed"].SetValue(lpci.grad3Speed);
            laserEffect.Parameters["grad4Speed"].SetValue(lpci.grad4Speed);

            laserEffect.Parameters["tex1Mult"].SetValue(1.25f);
            laserEffect.Parameters["tex2Mult"].SetValue(1.5f);
            laserEffect.Parameters["tex3Mult"].SetValue(1.15f);
            laserEffect.Parameters["tex4Mult"].SetValue(2.5f);
            laserEffect.Parameters["totalMult"].SetValue(1f);

            float dist = (endPoint - startPoint).Length();
            float repVal = dist / 2000f;
            laserEffect.Parameters["gradientReps"].SetValue(0.75f * repVal);
            laserEffect.Parameters["tex1reps"].SetValue(1.15f * repVal);
            laserEffect.Parameters["tex2reps"].SetValue(1.15f * repVal);
            laserEffect.Parameters["tex3reps"].SetValue(1.15f * repVal);
            laserEffect.Parameters["tex4reps"].SetValue(1.15f * repVal);

            laserEffect.Parameters["uTime"].SetValue((float)Main.timeForVisualEffects * -0.024f);
            #endregion

            laserEffect.CurrentTechnique.Passes["MainPS"].Apply();
            vertexStrip1.DrawTrail();
            Main.pixelShader.CurrentTechnique.Passes[0].Apply();
        }

        public void RainbowSigil(Projectile projectile)
        {
            Texture2D star = Mod.Assets.Request<Texture2D>("Assets/SandboxLastPrism/Flare/Simple Lens Flare_11").Value;
            Texture2D star2 = Mod.Assets.Request<Texture2D>("Assets/SandboxLastPrism/Flare/flare_16").Value;
            Texture2D sigil = Mod.Assets.Request<Texture2D>("Assets/SandboxLastPrism/Orbs/whiteFireEyeA").Value;
            Texture2D orbGlow = Mod.Assets.Request<Texture2D>("Assets/SandboxLastPrism/Orbs/circle_05").Value;

            if (myEffect == null)
                myEffect = ModContent.Request<Effect>("MagnumOpus/Effects/SandboxLastPrism/Radial/RainbowSigil", AssetRequestMode.ImmediateLoad).Value;

            myEffect.Parameters["rotation"].SetValue(endRot * 0.03f * 2.5f);
            myEffect.Parameters["rainbowRotation"].SetValue(endRot * 0.01f * 2.5f);
            myEffect.Parameters["intensity"].SetValue(1f);
            myEffect.Parameters["fadeStrength"].SetValue(1f);

            Vector2 drawPos = projectile.Center - Main.screenPosition + projectile.velocity.SafeNormalize(Vector2.UnitX) * offsetDistance;
            float rot = projectile.velocity.ToRotation();
            Vector2 endPoint = projectile.Center - Main.screenPosition + new Vector2(projectile.ai[2] - offsetDistance, 0f).RotatedBy(rot);


            float sin1 = MathF.Sin((float)Main.timeForVisualEffects * 0.04f);
            float sin2 = MathF.Cos((float)Main.timeForVisualEffects * 0.06f);
            float sin3 = -MathF.Cos(((float)Main.timeForVisualEffects * 0.08f) / 2f) + 1f;

            Vector2 sigilScale1 = new Vector2(0.2f, 1f) * 0.55f * combinedLaserScale * 1f;
            Vector2 sigilScale2 = sigilScale1 * (1.75f + (0.25f * sin1)) * 1f;

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, myEffect, Main.GameViewMatrix.EffectMatrix);

            //Main sigil
            Main.spriteBatch.Draw(sigil, drawPos, null, Color.White, rot, sigil.Size() / 2, sigilScale1, 0, 0f);
            Main.spriteBatch.Draw(sigil, drawPos, null, Color.White, rot, sigil.Size() / 2, sigilScale1, 0, 0f);

            Main.spriteBatch.Draw(star, drawPos + new Vector2(1f, 0f).RotatedBy(rot) * (15f * sin3), null, Color.White, rot, star.Size() / 2, sigilScale2, 0, 0f);

            Main.spriteBatch.Draw(star2, drawPos, null, Color.White, rot, star2.Size() / 2, sigilScale1 * 1f, 0, 0f);
            Main.spriteBatch.Draw(star2, drawPos, null, Color.White, rot, star2.Size() / 2, sigilScale1 * 1f, 0, 0f);


            //Flare at the end of the laser
            Main.spriteBatch.Draw(orbGlow, endPoint + new Vector2(0f, 0f), null, Color.White, endRot * 0.1f, orbGlow.Size() / 2f, 0.5f, 0, 0f);

            float endScale = 0.7f + (combinedLaserStartBoostPower * 0.5f);
            Main.spriteBatch.Draw(star, endPoint, null, Color.White, endRot * 0.02f, star.Size() / 2f, endScale * 0.45f, 0, 0f);
            Main.spriteBatch.Draw(star2, endPoint, null, Color.White, endRot * 0.05f, star2.Size() / 2f, endScale * 0.6f, 0, 0f);
            Main.spriteBatch.Draw(star2, endPoint, null, Color.White, endRot * 0.077f, star2.Size() / 2f, endScale * 0.35f, 0, 0f);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.EffectMatrix);
            Main.graphics.GraphicsDevice.BlendState = BlendState.AlphaBlend;

            //^ Reset with EffectMatrix because otherwise the wind lines draw fucked up for some reason
        }


        SLPColorInfo lpci = null;
        public void GetCombinedLaserInfo()
        {
            // Default to rainbow - no config system in MagnumOpus for this, always use rainbow
            lpci = new SLPColorInfo(SLPColorType.None, "RainbowGrad1", 0.66f, 0.66f, 1.03f, 0.77f);
        }
    }
}
