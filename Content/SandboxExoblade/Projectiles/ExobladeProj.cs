using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MagnumOpus.Content.SandboxExoblade.Buffs;
using MagnumOpus.Content.SandboxExoblade.Dusts;
using MagnumOpus.Content.SandboxExoblade.Primitives;
using MagnumOpus.Content.SandboxExoblade.Particles;
using MagnumOpus.Content.SandboxExoblade.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using static MagnumOpus.Content.SandboxExoblade.Utilities.ExobladeUtils;

namespace MagnumOpus.Content.SandboxExoblade.Projectiles
{
    public class ExobladeProj : ModProjectile
    {
        public Player Owner => Main.player[Projectile.owner];

        const float BladeLength = 180;

        public int GetSwingTime
        {
            get
            {
                if (State == SwingState.BonkDash)
                    return Exoblade.DashTime * Projectile.extraUpdates;
                else
                    return 78;
            }
        }

        public float Timer => SwingTime - Projectile.timeLeft;
        public float Progression => Timer / (float)SwingTime;
        public float LungeProgression => Progression < (1 - Exoblade.PercentageOfAnimationSpentLunging) ? 0 : (Progression - (1 - Exoblade.PercentageOfAnimationSpentLunging)) / Exoblade.PercentageOfAnimationSpentLunging;

        public enum SwingState
        {
            Swinging,
            BonkDash
        }

        public SwingState State
        {
            get
            {
                if (Projectile.ai[0] == 1)
                    return SwingState.BonkDash;
                return SwingState.Swinging;
            }
            set
            {
                Projectile.ai[0] = (int)value;
            }
        }

        public bool PerformingPowerfulSlash
        {
            get => Projectile.ai[0] > 1;
        }

        public bool InPostBonkStasis
        {
            get => Projectile.ai[1] > 0;
            set => Projectile.ai[1] = value ? 1 : 0;
        }

        public ref float SwingTime => ref Projectile.localAI[0];
        public ref float SquishFactor => ref Projectile.localAI[1];

        public float IdealSize => PerformingPowerfulSlash ? Exoblade.BigSlashUpscaleFactor : 1f;

        #region Angles and animation curves
        public int Direction => Math.Sign(Projectile.velocity.X) <= 0 ? -1 : 1;
        public float BaseRotation => Projectile.velocity.ToRotation();
        public Vector2 SquishVector => new Vector2(1f + (1 - SquishFactor) * 0.6f, SquishFactor);

        public static float MaxSwingAngle = MathHelper.PiOver2 * 1.8f;
        public CurveSegment SlowStart = new(PolyOutEasing, 0f, -1f, 0.3f, 2);
        public CurveSegment SwingFast = new(PolyInEasing, 0.27f, -0.7f, 1.6f, 4);
        public CurveSegment EndSwing = new(PolyOutEasing, 0.85f, 0.9f, 0.1f, 2);
        public float SwingAngleShiftAtProgress(float progress) => State == SwingState.BonkDash ? 0 : MaxSwingAngle * PiecewiseAnimation(progress, new CurveSegment[] { SlowStart, SwingFast, EndSwing });
        public float SwordRotationAtProgress(float progress) => State == SwingState.BonkDash ? BaseRotation : BaseRotation + SwingAngleShiftAtProgress(progress) * Direction;
        public float SquishAtProgress(float progress) => State == SwingState.BonkDash ? 1 : MathHelper.Lerp(SquishVector.X, SquishVector.Y, (float)Math.Abs(Math.Sin(SwingAngleShiftAtProgress(progress))));
        public Vector2 DirectionAtProgress(float progress) => State == SwingState.BonkDash ? Projectile.velocity : SwordRotationAtProgress(progress).ToRotationVector2() * SquishAtProgress(progress);

        public float SwingAngleShift => SwingAngleShiftAtProgress(Progression);
        public float SwordRotation => SwordRotationAtProgress(Progression);
        public float CurrentSquish => SquishAtProgress(Progression);
        public Vector2 SwordDirection => DirectionAtProgress(Progression);
        #endregion

        #region Trail primitives
        public float TrailEndProgression
        {
            get
            {
                float endProgression;
                if (Progression < 0.75f)
                    endProgression = Progression - 0.5f + 0.1f * (Progression / 0.75f);
                else
                    endProgression = Progression - 0.4f * (1 - (Progression - 0.75f) / 0.75f);
                return Math.Clamp(endProgression, 0, 1);
            }
        }

        public float RealProgressionAtTrailCompletion(float completion) => MathHelper.Lerp(Progression, TrailEndProgression, completion);

        public Vector2 DirectionAtProgressScuffed(float progress)
        {
            float angleShift = SwingAngleShiftAtProgress(progress);
            Vector2 anglePoint = angleShift.ToRotationVector2();
            anglePoint.X *= SquishVector.X;
            anglePoint.Y *= SquishVector.Y;
            angleShift = anglePoint.ToRotation();
            return (BaseRotation + angleShift * Direction).ToRotationVector2() * SquishAtProgress(progress);
        }
        #endregion

        public CurveSegment GoBack = new(SineBumpEasing, 0f, -10f, -14f);
        public CurveSegment AndThrust => new(PolyOutEasing, 1 - Exoblade.PercentageOfAnimationSpentLunging, -10, 12f, 5);
        public float DashDisplace => PiecewiseAnimation(Progression, new CurveSegment[] { GoBack, AndThrust });

        public float RiskOfDust
        {
            get
            {
                if (Progression > 0.85f) return 0;
                if (Progression < 0.4f) return (float)Math.Pow(Progression / 0.3f, 2) * 0.2f;
                if (Progression < 0.5f) return 0.2f + 0.7f * (Progression - 0.4f) / 0.1f;
                return 0.9f;
            }
        }

        public override string Texture => "MagnumOpus/Content/SandboxExoblade/ExobladeSquare";
        public static Asset<Texture2D> LensFlare;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailingMode[Type] = 2;
            ProjectileID.Sets.TrailCacheLength[Type] = 120;
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 98;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.MeleeNoSpeed;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 9999;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.MaxUpdates = 3;
            Projectile.localNPCHitCooldown = Projectile.MaxUpdates * 8;
            Projectile.noEnchantmentVisuals = true;
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(SwingTime);
            writer.Write(SquishFactor);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            SwingTime = reader.ReadSingle();
            SquishFactor = reader.ReadSingle();
        }

        public override bool ShouldUpdatePosition() => State == SwingState.BonkDash && !InPostBonkStasis;

        public override bool? CanDamage()
        {
            if (State != SwingState.BonkDash)
                return null;
            if (InPostBonkStasis)
                return false;
            if (Projectile.timeLeft > SwingTime * Exoblade.PercentageOfAnimationSpentLunging)
                return false;
            return null;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float _ = 0f;
            Vector2 start = Projectile.Center;
            Vector2 end = start + SwordDirection * (BladeLength + 50) * Projectile.scale;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), start, end, State == SwingState.BonkDash ? Projectile.scale * 45 : Projectile.scale * 30f, ref _);
        }

        public void InitializationEffects(bool startInitialization)
        {
            Projectile.velocity = Owner.MountedCenter.DirectionTo(Main.MouseWorld);
            SquishFactor = Main.rand.NextFloat(0.67f, 1f);

            if (startInitialization && State != SwingState.BonkDash)
                Projectile.scale = 0.02f;
            else
            {
                Projectile.scale = 1f;
                if (PerformingPowerfulSlash)
                    State = SwingState.Swinging;
            }

            if (PerformingPowerfulSlash)
                SquishFactor = 0.7f;

            SwingTime = GetSwingTime;
            Projectile.timeLeft = (int)SwingTime;
            Projectile.netUpdate = true;
        }

        public override void AI()
        {
            if (InPostBonkStasis || Projectile.timeLeft == 0)
                return;

            if (Projectile.timeLeft >= 9999 || (Projectile.timeLeft == 1 && Owner.channel && State != SwingState.BonkDash))
                InitializationEffects(Projectile.timeLeft >= 9999);

            switch (State)
            {
                case SwingState.Swinging:
                    DoBehavior_Swinging();
                    break;
                case SwingState.BonkDash:
                    DoBehavior_BonkDash();
                    break;
            }

            // Glue the sword to its owner.
            Projectile.Center = Owner.RotatedRelativePoint(Owner.MountedCenter, true);
            Owner.heldProj = Projectile.whoAmI;
            Owner.SetDummyItemTime(2);
            Owner.ChangeDir(Direction);

            // Decide the arm rotation for the owner.
            float armRotation = SwordRotation - MathHelper.PiOver2;
            Owner.SetCompositeArmFront(Math.Abs(armRotation) > 0.01f, Player.CompositeArmStretchAmount.Full, armRotation);

            // Freeze the projectile on its last frame for cooldown
            if (Projectile.timeLeft == 1 && State == SwingState.BonkDash && !InPostBonkStasis)
            {
                Projectile.timeLeft = Exoblade.LungeCooldown;
                InPostBonkStasis = true;
                Owner.fullRotation = 0f;
                Owner.ExoBlade().LungingDown = false;
            }
        }

        public void DoBehavior_Swinging()
        {
            if (Projectile.timeLeft == (int)(SwingTime / 5))
                SoundEngine.PlaySound(PerformingPowerfulSlash ? Exoblade.BigSwingSound : Exoblade.SwingSound, Projectile.Center);

            Lighting.AddLight(Owner.MountedCenter + SwordDirection * 100, Color.Lerp(Color.GreenYellow, Color.DeepPink, (float)Math.Pow(Progression, 3)).ToVector3() * 1.6f * (float)Math.Sin(Progression * MathHelper.Pi));

            // Decide the scale of the sword.
            if (Projectile.scale < IdealSize)
                Projectile.scale = MathHelper.Lerp(Projectile.scale, IdealSize, 0.08f);

            // Make the sword get smaller near the end of the slash
            if (!Owner.channel && Progression > 0.7f)
                Projectile.scale = (0.5f + 0.5f * (float)Math.Pow(1 - (Progression - 0.7f) / 0.3f, 0.5)) * IdealSize;

            if (Main.rand.NextFloat() * 3f < RiskOfDust)
            {
                Dust auricDust = Dust.NewDustPerfect(Owner.MountedCenter + SwordDirection * BladeLength * Projectile.scale * (float)Math.Pow(Main.rand.NextFloat(0.5f, 1f), 0.5f), ModContent.DustType<AuricBarDust>(), SwordDirection.RotatedBy(-MathHelper.PiOver2 * Direction) * 2f);
                auricDust.noGravity = true;
                auricDust.alpha = 10;
                auricDust.scale = 0.5f;
            }

            if (Main.rand.NextFloat() < RiskOfDust)
            {
                Color dustColor = Main.hslToRgb(Main.rand.NextFloat(), 1f, 0.9f);
                Dust must = Dust.NewDustPerfect(Owner.MountedCenter + SwordDirection * BladeLength * Projectile.scale * (float)Math.Pow(Main.rand.NextFloat(0.2f, 1f), 0.5f), DustID.RainbowMk2, SwordDirection.RotatedBy(MathHelper.PiOver2 * Direction) * 2.6f, 0, dustColor);
                must.scale = 0.3f;
                must.fadeIn = Main.rand.NextFloat() * 1.2f;
                must.noGravity = true;
            }

            // Create homing beams.
            int beamShootStart = (int)(SwingTime * 0.6f);
            int beamShootPeriod = (int)(SwingTime * 0.4f);
            int beamShootEnd = beamShootStart + beamShootPeriod;
            beamShootPeriod /= (Exoblade.BeamsPerSwing - 1);

            if (Main.myPlayer == Projectile.owner && Timer >= beamShootStart && Timer < beamShootEnd && (Timer - beamShootStart) % beamShootPeriod == 0)
            {
                int boltDamage = (int)(Projectile.damage * Exoblade.NotTrueMeleeDamagePenalty);
                Vector2 boltVelocity = Projectile.velocity.RotatedByRandom(MathHelper.PiOver4 * 0.3);
                boltVelocity *= Owner.HeldItem.shootSpeed;

                Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center + boltVelocity * 5f, boltVelocity, ModContent.ProjectileType<Exobeam>(), boltDamage, Projectile.knockBack / 3f, Projectile.owner);
            }
        }

        public void DoBehavior_BonkDash()
        {
            Owner.mount?.Dismount(Owner);
            Owner.RemoveAllGrapplingHooks();

            if (LungeProgression == 0)
            {
                // Play a charge sound right before the dash.
                if (Projectile.timeLeft == 1 + (int)(SwingTime * (Exoblade.PercentageOfAnimationSpentLunging)))
                    SoundEngine.PlaySound(Exoblade.DashSound, Projectile.Center);

                Projectile.velocity = Owner.MountedCenter.DirectionTo(Main.MouseWorld);
                Projectile.oldPos = new Vector2[Projectile.oldPos.Length];

                for (int i = 0; i < Projectile.oldPos.Length; ++i)
                    Projectile.oldPos[i] = Projectile.position;
            }
            else
            {
                float rotationStrength = MathHelper.PiOver4 * 0.05f * (float)Math.Pow(LungeProgression, 3);
                float currentRotation = Projectile.velocity.ToRotation();
                float idealRotation = Owner.MountedCenter.DirectionTo(Main.MouseWorld).ToRotation();
                Projectile.velocity = currentRotation.AngleTowards(idealRotation, rotationStrength).ToRotationVector2();

                Owner.fallStart = (int)(Owner.position.Y / 16f);

                float velocityPower = (float)Math.Sin(MathHelper.Pi * LungeProgression);
                velocityPower = (float)Math.Pow(Math.Abs(velocityPower), 0.6f);
                Vector2 newVelocity = Projectile.velocity * Exoblade.LungeSpeed * (0.24f + 0.76f * velocityPower);
                Owner.velocity = newVelocity;
                Owner.ExoBlade().LungingDown = true;

                if (Main.rand.NextBool())
                {
                    Color dustColor = Main.hslToRgb(Main.rand.NextFloat(), 1f, 0.9f);
                    Dust must = Dust.NewDustPerfect(Owner.MountedCenter + Main.rand.NextVector2Circular(20f, 20f), DustID.RainbowMk2, SwordDirection * -2.6f, 0, dustColor);
                    must.scale = 0.3f;
                    must.fadeIn = Main.rand.NextFloat() * 1.2f;
                    must.noGravity = true;
                }

                // Streaks during the dash
                if (Main.rand.NextBool(6) && LungeProgression < 0.8f)
                {
                    Vector2 particleSpeed = SwordDirection * -1 * Main.rand.NextFloat(6f, 10f);
                    ExoSquishyLightParticle energyLeak = new(Owner.MountedCenter + Main.rand.NextVector2Circular(20f, 20f) + Owner.velocity * 5, particleSpeed, Main.rand.NextFloat(0.3f, 0.6f), Color.GreenYellow, 30, 3.4f, 4.5f, hueShift: 0.02f);
                    ExoParticleHandler.SpawnParticle(energyLeak);
                }

                // Streaks after the dash
                if (Main.rand.NextBool(5) && LungeProgression >= 0.8f)
                {
                    Vector2 particleSpeed = SwordDirection * -1 * Main.rand.NextFloat(6f, 10f);
                    ExoSquishyLightParticle energyLeak = new(Owner.MountedCenter + Main.rand.NextVector2Circular(50f, 50f) + Owner.velocity * 4, particleSpeed, Main.rand.NextFloat(0.3f, 0.6f), Color.GreenYellow, 30, 3.4f, 4.5f, hueShift: 0.02f);
                    ExoParticleHandler.SpawnParticle(energyLeak);
                }
            }

            // Stop the dash on the last frame.
            if (Projectile.timeLeft == 1)
                Owner.velocity *= 0.2f;

            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4 * Direction;
        }

        public float SlashWidthFunction(float completionRatio, Vector2 vertexPos) => SquishAtProgress(RealProgressionAtTrailCompletion(completionRatio)) * Projectile.scale * 60.5f;
        public Color SlashColorFunction(float completionRatio, Vector2 vertexPos) => Color.Lime * Utils.GetLerpValue(0.9f, 0.4f, completionRatio, true) * Projectile.Opacity;

        public float PierceWidthFunction(float completionRatio, Vector2 vertexPos)
        {
            float width = Utils.GetLerpValue(0f, 0.2f, completionRatio, true) * Projectile.scale * 50f;
            width *= (1 - (float)Math.Pow(LungeProgression, 5));
            return width;
        }

        public Color PierceColorFunction(float completionRatio, Vector2 vertexPos) => Color.Lime * Projectile.Opacity;

        public List<Vector2> GenerateSlashPoints()
        {
            List<Vector2> result = new();
            for (int i = 0; i < 40; i++)
            {
                float progress = MathHelper.Lerp(Progression, TrailEndProgression, i / 40f);
                result.Add(DirectionAtProgressScuffed(progress) * (BladeLength - 6f) * Projectile.scale);
            }
            return result;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (Projectile.Opacity <= 0f || InPostBonkStasis)
                return false;

            DrawSlash();
            DrawPierceTrail();
            DrawBlade();
            return false;
        }

        public void DrawSlash()
        {
            if (State != SwingState.Swinging || Progression < 0.45f)
                return;

            Main.spriteBatch.EnterShaderRegion();
            GameShaders.Misc["MagnumOpus:ExobladeSlash"].UseImage1(ModContent.Request<Texture2D>("MagnumOpus/Content/SandboxExoblade/Textures/VoronoiShapes"));
            GameShaders.Misc["MagnumOpus:ExobladeSlash"].UseColor(new Color(105, 240, 220));
            GameShaders.Misc["MagnumOpus:ExobladeSlash"].UseSecondaryColor(new Color(57, 46, 115));
            GameShaders.Misc["MagnumOpus:ExobladeSlash"].Shader.Parameters["fireColor"].SetValue(new Color(242, 112, 72).ToVector3());
            GameShaders.Misc["MagnumOpus:ExobladeSlash"].Shader.Parameters["flipped"].SetValue(Direction == 1);
            GameShaders.Misc["MagnumOpus:ExobladeSlash"].Apply();

            PrimitiveRenderer.RenderTrail(GenerateSlashPoints(), new(SlashWidthFunction, SlashColorFunction, (_, _) => Projectile.Center, shader: GameShaders.Misc["MagnumOpus:ExobladeSlash"]), 95);

            Main.spriteBatch.ExitShaderRegion();
        }

        public void DrawPierceTrail()
        {
            if (State != SwingState.BonkDash)
                return;

            Main.spriteBatch.EnterShaderRegion();

            Color mainColor = MulticolorLerp((Main.GlobalTimeWrappedHourly * 2f) % 1, Color.Cyan, Color.Lime, Color.GreenYellow, Color.Goldenrod, Color.Orange);
            Color secondaryColor = MulticolorLerp((Main.GlobalTimeWrappedHourly * 2f + 0.2f) % 1, Color.Cyan, Color.Lime, Color.GreenYellow, Color.Goldenrod, Color.Orange);

            mainColor = Color.Lerp(Color.White, mainColor, 0.4f + 0.6f * (float)Math.Pow(LungeProgression, 0.5f));
            secondaryColor = Color.Lerp(Color.White, secondaryColor, 0.4f + 0.6f * (float)Math.Pow(LungeProgression, 0.5f));

            Vector2 trailOffset = (Projectile.rotation - Direction * MathHelper.PiOver4).ToRotationVector2() * 98f + Projectile.Size * 0.5f;
            GameShaders.Misc["MagnumOpus:ExobladePierce"].UseImage1(ModContent.Request<Texture2D>("MagnumOpus/Content/SandboxExoblade/Textures/EternityStreak"));
            GameShaders.Misc["MagnumOpus:ExobladePierce"].UseImage2("Images/Extra_189");
            GameShaders.Misc["MagnumOpus:ExobladePierce"].UseColor(mainColor);
            GameShaders.Misc["MagnumOpus:ExobladePierce"].UseSecondaryColor(secondaryColor);
            GameShaders.Misc["MagnumOpus:ExobladePierce"].Apply();

            int numPointsRendered = 30;
            int numPointsProvided = 60;
            var positionsToUse = Projectile.oldPos.Take(numPointsProvided).ToArray();
            PrimitiveRenderer.RenderTrail(positionsToUse, new(PierceWidthFunction, PierceColorFunction, (_, _) => trailOffset, shader: GameShaders.Misc["MagnumOpus:ExobladePierce"]), numPointsRendered);

            Main.spriteBatch.ExitShaderRegion();
        }

        public void DrawBlade()
        {
            var texture = Terraria.GameContent.TextureAssets.Projectile[Type].Value;
            SpriteEffects direction = Direction == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            if (State == SwingState.Swinging)
            {
                Effect swingFX = Filters.Scene["MagnumOpus:ExobladeSwingSprite"].GetShader().Shader;
                swingFX.Parameters["rotation"].SetValue(SwingAngleShift + MathHelper.PiOver4 + (Direction == -1 ? MathHelper.Pi : 0f));
                swingFX.Parameters["pommelToOriginPercent"].SetValue(0.05f);
                swingFX.Parameters["color"].SetValue(Color.White.ToVector4());

                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, swingFX, Main.GameViewMatrix.TransformationMatrix);

                Main.EntitySpriteDraw(texture, Owner.MountedCenter - Main.screenPosition, null, Color.White, BaseRotation, texture.Size() / 2f, SquishVector * 3f * Projectile.scale, direction, 0);

                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

                if (LensFlare == null)
                    LensFlare = ModContent.Request<Texture2D>("MagnumOpus/Content/SandboxExoblade/Textures/HalfStar");
                Texture2D shineTex = LensFlare.Value;
                Vector2 shineScale = new Vector2(1f, 3f);

                float lensFlareOpacity = (Progression < 0.3f ? 0f : 0.2f + 0.8f * (float)Math.Sin(MathHelper.Pi * (Progression - 0.3f) / 0.7f)) * 0.6f;
                Color lensFlareColor = Color.Lerp(Color.LimeGreen, Color.Plum, (float)Math.Pow(Progression, 3));
                lensFlareColor.A = 0;
                Main.EntitySpriteDraw(shineTex, Owner.MountedCenter + DirectionAtProgressScuffed(Progression) * Projectile.scale * BladeLength - Main.screenPosition, null, lensFlareColor * lensFlareOpacity, MathHelper.PiOver2, shineTex.Size() / 2f, shineScale * Projectile.scale, 0, 0);
            }
            else
            {
                float rotation = BaseRotation + MathHelper.PiOver4;
                Vector2 origin = new Vector2(0, texture.Height);
                Vector2 drawPosition = Projectile.Center + Projectile.velocity * Projectile.scale * DashDisplace - Main.screenPosition;

                if (Direction == -1)
                {
                    rotation += MathHelper.PiOver2;
                    origin.X = texture.Width;
                }

                Projectile.scale = MathHelper.Lerp(1f, 0.22f, MathF.Pow(LungeProgression, 7));

                Main.EntitySpriteDraw(texture, drawPosition, null, Color.White, rotation, origin, Projectile.scale, direction, 0);

                float energyPower = Utils.GetLerpValue(0f, 0.32f, Progression, true) * Utils.GetLerpValue(1f, 0.85f, Progression, true);
                for (int i = 0; i < 4; i++)
                {
                    Vector2 drawOffset = (MathHelper.TwoPi * i / 4f + BaseRotation).ToRotationVector2() * energyPower * Projectile.scale * 7f;
                    Main.spriteBatch.Draw(texture, drawPosition + drawOffset, null, Color.Lerp(Color.Goldenrod, Color.MediumTurquoise, Progression) with { A = 0 } * 0.16f, rotation, origin, Projectile.scale, direction, 0);
                }
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            ItemLoader.OnHitNPC(Owner.HeldItem, Owner, target, hit, damageDone);
            NPCLoader.OnHitByItem(target, Owner, Owner.HeldItem, hit, damageDone);
            PlayerLoader.OnHitNPC(Owner, target, hit, damageDone);

            if (State == SwingState.BonkDash)
            {
                Owner.itemAnimation = 0;
                Owner.velocity = Owner.SafeDirectionTo(target.Center) * -Exoblade.ReboundSpeed;
                Projectile.timeLeft = Exoblade.OpportunityForBigSlash + Exoblade.LungeCooldown;
                InPostBonkStasis = true;

                Projectile.netUpdate = true;

                SoundEngine.PlaySound(Exoblade.DashHitSound, target.Center);
                SoundEngine.PlaySound(Exoblade.BeamHitSound with { Volume = Exoblade.BeamHitSound.Volume * 1.2f }, target.Center);
                if (Main.myPlayer == Projectile.owner)
                {
                    int lungeHitDamage = (int)(Projectile.damage * Exoblade.LungeDamageFactor);
                    for (int i = 0; i < 5; i++)
                    {
                        int slash = Projectile.NewProjectile(Projectile.GetSource_FromAI(), target.Center, Projectile.velocity * 0.1f, ModContent.ProjectileType<ExobeamSlashCreator>(), lungeHitDamage, 0f, Projectile.owner, target.whoAmI, 100);
                        if (Main.projectile.IndexInRange(slash))
                            Main.projectile[slash].timeLeft -= i * 4;
                    }
                }

                // Freeze the target briefly
                target.AddBuff(ModContent.BuffType<ExoGlacialState>(), 60);
            }

            if (State == SwingState.Swinging && PerformingPowerfulSlash && Owner.ownedProjectileCounts[ModContent.ProjectileType<Exoboom>()] < 1)
            {
                SoundEngine.PlaySound(Exoblade.BigHitSound, Projectile.Center);
                if (Main.myPlayer == Projectile.owner)
                {
                    int explosionDamage = (int)(Projectile.damage * Exoblade.ExplosionDamageFactor);
                    Projectile.NewProjectile(Projectile.GetSource_FromAI(), target.Center, Vector2.Zero, ModContent.ProjectileType<Exoboom>(), explosionDamage, 0f, Projectile.owner);
                }

                Owner.DoLifestealDirect(target, (int)Math.Round(hit.Damage * 0.04), 0.4f);
            }
        }

        public override void OnKill(int timeLeft)
        {
            Owner.fullRotation = 0f;
            Owner.ExoBlade().LungingDown = false;
        }
    }
}
