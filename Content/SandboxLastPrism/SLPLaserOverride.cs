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
    public class SLPLaserOverride : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        {
            return lateInstantiation && (entity.type == ProjectileID.LastPrismLaser);
        }

        public override void SetDefaults(Projectile entity)
        {
            entity.hide = true;
            base.SetDefaults(entity);
        }

        public override void DrawBehind(Projectile projectile, int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            overPlayers.Add(index);
            base.DrawBehind(projectile, index, behindNPCsAndTiles, behindNPCs, behindProjectiles, overPlayers, overWiresUI);
        }

        int timer = 0;
        SLPColorInfo lpci = null;
        public override bool PreAI(Projectile projectile)
        {
            if (timer == 0)
            {
                // Default to rainbow
                lpci = new SLPColorInfo(SLPColorType.None, "RainbowGrad1", 0.66f, 0.66f, 1.03f, 0.77f);
            }


            #region vanilla LPL behavior (without dust) && now respects gfxOffYproj
            Vector2? vector133 = null;
            if (projectile.velocity.HasNaNs() || projectile.velocity == Vector2.Zero)
            {
                projectile.velocity = -Vector2.UnitY;
            }

            if (false)
                Main.NewText("PENIS");
            else
            {
                if (projectile.type != 632 || !Main.projectile[(int)projectile.ai[1]].active || Main.projectile[(int)projectile.ai[1]].type != 633)
                {
                    projectile.Kill();
                    return false;
                }
                float num750 = (float)(int)projectile.ai[0] - 2.5f;
                Vector2 vector140 = Vector2.Normalize(Main.projectile[(int)projectile.ai[1]].velocity);
                Projectile projectile2 = Main.projectile[(int)projectile.ai[1]];
                float num751 = num750 * ((float)Math.PI / 6f);
                float num752 = 20f;
                Vector2 zero2 = Vector2.Zero;
                float num753 = 1f;
                float num754 = 15f;
                float num756 = -2f;
                if (projectile2.ai[0] < 180f)
                {
                    num753 = 1f - projectile2.ai[0] / 180f;
                    num754 = 20f - projectile2.ai[0] / 180f * 14f;
                    if (projectile2.ai[0] < 120f)
                    {
                        num752 = 20f - 4f * (projectile2.ai[0] / 120f);
                        projectile.Opacity = projectile2.ai[0] / 120f * 0.4f;
                    }
                    else
                    {
                        num752 = 16f - 10f * ((projectile2.ai[0] - 120f) / 60f);
                        projectile.Opacity = 0.4f + (projectile2.ai[0] - 120f) / 60f * 0.6f;
                    }
                    num756 = -22f + projectile2.ai[0] / 180f * 20f;
                }
                else
                {
                    num753 = 0f;
                    num752 = 1.75f;
                    num754 = 6f;
                    projectile.Opacity = 1f;
                    num756 = -2f;
                }
                float num757 = (projectile2.ai[0] + num750 * num752) / (num752 * 6f) * ((float)Math.PI * 2f);
                num751 = Vector2.UnitY.RotatedBy(num757).Y * ((float)Math.PI / 6f) * num753;
                zero2 = (Vector2.UnitY.RotatedBy(num757) * new Vector2(4f, num754)).RotatedBy(projectile2.velocity.ToRotation());
                projectile.position = projectile2.Center + vector140 * 16f - projectile.Size / 2f + new Vector2(0f, 0f - Main.projectile[(int)projectile.ai[1]].gfxOffY);
                projectile.position += projectile2.velocity.ToRotation().ToRotationVector2() * num756;
                projectile.position += zero2;
                projectile.velocity = Vector2.Normalize(projectile2.velocity).RotatedBy(num751);
                projectile.scale = 1.4f * (1f - num753);
                projectile.damage = projectile2.damage;
                if (projectile2.ai[0] >= 180f)
                {
                    ////////!!!!!!!!!!!!!!!! I added this
                    projectile2.ai[2] = projectile.localAI[1];

                    projectile.damage *= 3;
                    vector133 = projectile2.Center;
                }
                if (!Collision.CanHitLine(Main.player[projectile.owner].Center, 0, 0, projectile2.Center, 0, 0))
                {
                    vector133 = Main.player[projectile.owner].Center;
                }
                projectile.friendly = projectile2.ai[0] > 30f;
            }
            if (projectile.velocity.HasNaNs() || projectile.velocity == Vector2.Zero)
            {
                projectile.velocity = -Vector2.UnitY;
            }
            float num761 = projectile.velocity.ToRotation();

            projectile.rotation = num761 - (float)Math.PI / 2f;
            projectile.velocity = num761.ToRotationVector2();
            float num762 = 0f;
            float num763 = 0f;
            Vector2 samplingPoint = projectile.Center;
            if (vector133.HasValue)
            {
                samplingPoint = vector133.Value;
            }
            if (projectile.type == 632)
            {
                num762 = 2f;
                num763 = 0f;
            }

            float[] array2 = new float[(int)num762];
            Collision.LaserScan(samplingPoint, projectile.velocity, num763 * projectile.scale, 2400f, array2);
            float num764 = 0f;
            for (int num765 = 0; num765 < array2.Length; num765++)
            {
                num764 += array2[num765];
            }
            num764 /= num762;
            float amount = 0.5f;
            if (projectile.type == 455)
            {
                NPC nPC3 = Main.npc[(int)projectile.ai[1]];
                if (nPC3.type == 396)
                {
                    Player player11 = Main.player[nPC3.target];
                    if (!Collision.CanHitLine(nPC3.position, nPC3.width, nPC3.height, player11.position, player11.width, player11.height))
                    {
                        num764 = Math.Min(2400f, Vector2.Distance(nPC3.Center, player11.Center) + 150f);
                        amount = 0.75f;
                    }
                }
            }
            if (projectile.type == 632)
            {
                amount = 0.75f;
            }
            projectile.localAI[1] = MathHelper.Lerp(projectile.localAI[1], num764, amount);

            if (projectile.type != 632 || !(Math.Abs(projectile.localAI[1] - num764) < 100f) || !(projectile.scale > 0.15f))
            {
                return false;
            }

            float laserLuminance = 0.5f;
            float laserAlphaMultiplier = 0f;
            float lastPrismHue = projectile.GetLastPrismHue(projectile.ai[0], ref laserLuminance, ref laserAlphaMultiplier);
            Color color = Main.hslToRgb(lastPrismHue, 1f, laserLuminance);
            color.A = (byte)((float)(int)color.A * laserAlphaMultiplier);
            Color color2 = color;
            Vector2 vector154 = projectile.Center + projectile.velocity * (projectile.localAI[1] - 14.5f * projectile.scale);
            float x5 = Main.rgbToHsl(new Color(Main.DiscoR, Main.DiscoG, Main.DiscoB)).X;
            for (int num791 = 220; num791 < 2; num791++) //!!!!!!!!!!!!!!!!
            {
                float num792 = projectile.velocity.ToRotation() + ((Main.rand.Next(2) == 1) ? (-1f) : 1f) * ((float)Math.PI / 2f);
                float num793 = (float)Main.rand.NextDouble() * 0.8f + 1f;
                Vector2 vector155 = new Vector2((float)Math.Cos(num792) * num793, (float)Math.Sin(num792) * num793);
                int num794 = Dust.NewDust(vector154, 0, 0, 267, vector155.X, vector155.Y);
                Main.dust[num794].color = color;
                Main.dust[num794].scale = 1.2f;
                if (projectile.scale > 1f)
                {
                    Dust dust86 = Main.dust[num794];
                    Dust dust212 = dust86;
                    dust212.velocity *= projectile.scale;
                    dust86 = Main.dust[num794];
                    dust212 = dust86;
                    dust212.scale *= projectile.scale;
                }
                Main.dust[num794].noGravity = true;
                if (projectile.scale != 1.4f && num794 != 6000)
                {
                    Dust dust160 = Dust.CloneDust(num794);
                    dust160.color = Color.White;
                    Dust dust85 = dust160;
                    Dust dust212 = dust85;
                    dust212.scale /= 2f;
                }
                float hue = (x5 + Main.rand.NextFloat() * 0.4f) % 1f;
                Main.dust[num794].color = Color.Lerp(color, Main.hslToRgb(hue, 1f, 0.75f), projectile.scale / 1.4f);
            }
            if (Main.rand.Next(5) == 0 && false) //!!!!!!!!!!!!!!!!!
            {
                Vector2 vector157 = projectile.velocity.RotatedBy(1.5707963705062866) * ((float)Main.rand.NextDouble() - 0.5f) * projectile.width;
                int num795 = Dust.NewDust(vector154 + vector157 - Vector2.One * 4f, 8, 8, 31, 0f, 0f, 100, default(Color), 1.5f);
                Dust dust87 = Main.dust[num795];
                Dust dust212 = dust87;
                dust212.velocity *= 0.5f;
                Main.dust[num795].velocity.Y = 0f - Math.Abs(Main.dust[num795].velocity.Y);
            }
            DelegateMethods.v3_1 = color.ToVector3() * 0.3f;
            float value24 = 0.1f * (float)Math.Sin(Main.GlobalTimeWrappedHourly * 20f);
            Vector2 size2 = new Vector2(projectile.velocity.Length() * projectile.localAI[1], (float)projectile.width * projectile.scale);
            float num796 = projectile.velocity.ToRotation();
            if (Main.netMode != 2)
            {
                ((WaterShaderData)Filters.Scene["WaterDistortion"].GetShader()).QueueRipple(projectile.position + new Vector2(size2.X * 0.5f, 0f).RotatedBy(num796), new Color(0.5f, 0.1f * (float)Math.Sign(value24) + 0.5f, 0f, 1f) * Math.Abs(value24), size2, RippleShape.Square, num796);
            }
            Utils.PlotTileLine(projectile.Center, projectile.Center + projectile.velocity * projectile.localAI[1], (float)projectile.width * projectile.scale, DelegateMethods.CastLight);


            #endregion


            #region tileDust
            Projectile parent = Main.projectile[(int)projectile.ai[1]];

            if (timer % 1 == 0 && parent.ai[0] < 180f)
            {
                for (int i = 0; i < 1; i++)
                {
                    int dustColsLength = lpci.dustColors.Count;
                    Color col = lpci.dustColors[Main.rand.Next(0, dustColsLength)];

                    Vector2 vel = Main.rand.NextVector2Circular(2.75f, 2.75f) * (1f + projectile.scale * 0.5f);

                    Dust p = Dust.NewDustPerfect(projectile.Center + (projectile.velocity * projectile.localAI[1]), ModContent.DustType<PixelGlowOrb>(), vel * Main.rand.NextFloat(0.8f, 1.05f),
                        newColor: col * 1f, Scale: Main.rand.NextFloat(0.35f, 0.4f) * projectile.scale);
                    p.customData = SLPDustBehaviorUtil.AssignBehavior_PGOBase(velToBeginShrink: 2.5f, fadePower: 0.9f);
                }
            }
            #endregion

            timer++;
            return false;
        }


        float overallScale = 1f;
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            Projectile parent = Main.projectile[(int)projectile.ai[1]];

            ModContent.GetInstance<SLPPixelationSystem>().QueueRenderAction(SLPRenderLayer.UnderProjectiles, () =>
            {
                if (parent.ai[0] < 180f)
                    DrawVertexTrailRainbow(projectile, false);
            });

            return false;

        }

        Effect myEffect = null;
        //makes the scrolling of each laser start at a random point
        float randomTimeOffset = Main.rand.NextFloat(0f, 0.15f);
        public void DrawVertexTrailRainbow(Projectile projectile, bool giveUp)
        {
            if (giveUp)
                return;

            float colAlpha = projectile.Opacity * 0.4f;
            float easedAlpha = SLPEasings.easeOutQuad(projectile.Opacity);

            float laserLuminance = 0.5f;
            float laserAlphaMultiplier = 0f;
            Color lastPrismCol = Main.hslToRgb(projectile.GetLastPrismHue(projectile.ai[0], ref laserLuminance, ref laserAlphaMultiplier), 1f, laserLuminance) * colAlpha;

            //Make the color a bit brighter
            lastPrismCol = Color.Lerp(lastPrismCol, Color.White, 0.1f);

            Vector2 startPoint = projectile.Center + new Vector2(0f, Main.player[projectile.owner].gfxOffY);
            Vector2 endPoint = startPoint + (projectile.velocity * projectile.localAI[1]);
            float dist = (endPoint - startPoint).Length();

            //EndPoints
            Texture2D bloomOrb = Mod.Assets.Request<Texture2D>("Assets/SandboxLastPrism/Pixel/PartiGlow").Value;
            float bloomEndSize = 1.3f * colAlpha;
            Main.EntitySpriteDraw(bloomOrb, endPoint - Main.screenPosition, null, Color.White with { A = 0 } * 0.85f, projectile.velocity.ToRotation(), bloomOrb.Size() / 2f, 1f * bloomEndSize, 0, 0);

            Main.EntitySpriteDraw(bloomOrb, startPoint - Main.screenPosition, null, Color.White with { A = 0 } * 0.85f, projectile.velocity.ToRotation(), bloomOrb.Size() / 2f, 1f * bloomEndSize, 0, 0);


            //TRAIL
            if (myEffect == null)
                myEffect = ModContent.Request<Effect>("MagnumOpus/Effects/SandboxLastPrism/Scroll/ComboLaserVertexGradient", AssetRequestMode.ImmediateLoad).Value;

            Texture2D trailTexture1 = SLPCommonTextures.EnergyTex.Value;
            Texture2D trailTexture2 = SLPCommonTextures.ThinGlowLine.Value;

            Vector2[] pos_arr = { startPoint, endPoint };
            float[] rot_arr = { projectile.velocity.ToRotation(), projectile.velocity.ToRotation() };

            float sineWidthMult = 1f + (float)Math.Cos(Main.timeForVisualEffects * 0.3f) * 0.0f;

            Color StripColor(float progress) => Color.White;
            float StripWidth1(float progress) => 40f * overallScale * sineWidthMult * SLPEasings.easeOutCirc(colAlpha);

            VertexStrip vertexStrip1 = new VertexStrip();
            vertexStrip1.PrepareStrip(pos_arr, rot_arr, StripColor, StripWidth1, -Main.screenPosition, includeBacksides: true);

            #region shaderInfo
            String GradLocation = "MagnumOpus/Assets/SandboxLastPrism/Gradients/";

            myEffect.Parameters["WorldViewProjection"].SetValue(Main.GameViewMatrix.NormalizedTransformationmatrix);

            myEffect.Parameters["onTex"].SetValue(ModContent.Request<Texture2D>("MagnumOpus/Assets/SandboxLastPrism/Trails/Clear/ThinLineGlowClear", AssetRequestMode.ImmediateLoad).Value);
            myEffect.Parameters["gradientTex"].SetValue(ModContent.Request<Texture2D>(GradLocation + lpci.textureLocation, AssetRequestMode.ImmediateLoad).Value);
            myEffect.Parameters["baseColor"].SetValue(Color.White.ToVector3());
            myEffect.Parameters["satPower"].SetValue(0.25f);

            myEffect.Parameters["sampleTexture1"].SetValue(SLPCommonTextures.ThinGlowLine.Value);
            myEffect.Parameters["sampleTexture2"].SetValue(SLPCommonTextures.spark_06.Value);
            myEffect.Parameters["sampleTexture3"].SetValue(SLPCommonTextures.Extra_196_Black.Value);
            myEffect.Parameters["sampleTexture4"].SetValue(SLPCommonTextures.Trail5Loop.Value);


            myEffect.Parameters["grad1Speed"].SetValue(lpci.grad1Speed);
            myEffect.Parameters["grad2Speed"].SetValue(lpci.grad2Speed);
            myEffect.Parameters["grad3Speed"].SetValue(lpci.grad3Speed);
            myEffect.Parameters["grad4Speed"].SetValue(lpci.grad4Speed);

            myEffect.Parameters["tex1Mult"].SetValue(2f * easedAlpha);
            myEffect.Parameters["tex2Mult"].SetValue(1f * easedAlpha);
            myEffect.Parameters["tex3Mult"].SetValue(1.15f * easedAlpha);
            myEffect.Parameters["tex4Mult"].SetValue(2.5f * easedAlpha);
            myEffect.Parameters["totalMult"].SetValue(1f);


            //We want the number of repititions to be relative to the number of points
            float repValue = dist / 500;
            myEffect.Parameters["gradientReps"].SetValue(0.35f * repValue);
            myEffect.Parameters["tex1reps"].SetValue(1f * repValue);
            myEffect.Parameters["tex2reps"].SetValue(0.3f * repValue);
            myEffect.Parameters["tex3reps"].SetValue(1f * repValue);
            myEffect.Parameters["tex4reps"].SetValue(0.25f * repValue);

            myEffect.Parameters["uTime"].SetValue(((float)Main.timeForVisualEffects * -0.025f) + randomTimeOffset);

            myEffect.CurrentTechnique.Passes["MainPS"].Apply();
            vertexStrip1.DrawTrail();
            Main.pixelShader.CurrentTechnique.Passes[0].Apply();
            #endregion
        }
    }
}
