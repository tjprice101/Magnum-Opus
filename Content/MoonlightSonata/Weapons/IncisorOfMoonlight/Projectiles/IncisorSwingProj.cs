using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MagnumOpus.Content.MoonlightSonata.Weapons.IncisorOfMoonlight.Buffs;
using MagnumOpus.Content.MoonlightSonata.Weapons.IncisorOfMoonlight.Primitives;
using MagnumOpus.Content.MoonlightSonata.Weapons.IncisorOfMoonlight.Particles;
using MagnumOpus.Content.MoonlightSonata.Weapons.IncisorOfMoonlight.Utilities;
using MagnumOpus.Content.MoonlightSonata;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using static MagnumOpus.Content.MoonlightSonata.Weapons.IncisorOfMoonlight.Utilities.IncisorUtils;

namespace MagnumOpus.Content.MoonlightSonata.Weapons.IncisorOfMoonlight.Projectiles
{
    /// <summary>
    /// Incisor of Moonlight 窶・Main swing/dash projectile.
    /// "The Stellar Scalpel" 窶・precision cuts that reveal constellations beneath reality.
    ///
    /// Two states:
    ///   Swinging 窶・resonance arc trail, constellation node sparks, homing lunar beams
    ///   LunarDash 窶・dash through enemies with constellation pierce trail
    ///
    /// After dash-hit, enables empowered swing 竊・Lunar Nova explosion + lifesteal.
    /// </summary>
    public class IncisorSwingProj : ModProjectile
    {
        public Player Owner => Main.player[Projectile.owner];
        const float BladeLength = 150;

        // =====================================================================
        // TIMING
        // =====================================================================

        public int GetSwingTime => State == SwingState.LunarDash
            ? IncisorOfMoonlight.DashTime * Projectile.extraUpdates
            : 72;

        public float Timer => SwingTime - Projectile.timeLeft;
        public float Progression => Timer / SwingTime;
        public float LungeProgression => Progression < (1 - IncisorOfMoonlight.LungePercent)
            ? 0 : (Progression - (1 - IncisorOfMoonlight.LungePercent)) / IncisorOfMoonlight.LungePercent;

        // =====================================================================
        // STATE
        // =====================================================================

        public enum SwingState { Swinging, LunarDash }

        public SwingState State
        {
            get => Projectile.ai[0] == 1 ? SwingState.LunarDash : SwingState.Swinging;
            set => Projectile.ai[0] = (int)value;
        }

        public bool PerformingPowerfulSlash => Projectile.ai[0] > 1;
        public bool InPostDashStasis
        {
            get => Projectile.ai[1] > 0;
            set => Projectile.ai[1] = value ? 1 : 0;
        }

        public ref float SwingTime => ref Projectile.localAI[0];
        public ref float SquishFactor => ref Projectile.localAI[1];
        public float IdealSize => PerformingPowerfulSlash ? IncisorOfMoonlight.BigSlashUpscale : 1f;

        // =====================================================================
        // ANGLES AND ANIMATION CURVES
        // =====================================================================

        public int Direction => Math.Sign(Projectile.velocity.X) <= 0 ? -1 : 1;
        public float BaseRotation => Projectile.velocity.ToRotation();
        public Vector2 SquishVector => new(1f + (1 - SquishFactor) * 0.6f, SquishFactor);

        public static float MaxSwingAngle = MathHelper.PiOver2 * 1.75f;

        // Musical movement: Grave (slow pull) 竊・Allegro (fast swing) 竊・Diminuendo (settle)
        public CurveSegment GravePull = new(PolyOutEasing, 0f, -1f, 0.28f, 2);
        public CurveSegment AllegroSwing = new(PolyInEasing, 0.25f, -0.72f, 1.62f, 4);
        public CurveSegment DiminuendoSettle = new(PolyOutEasing, 0.83f, 0.9f, 0.1f, 2);

        public float SwingAngleShiftAtProgress(float progress) => State == SwingState.LunarDash
            ? 0 : MaxSwingAngle * PiecewiseAnimation(progress, GravePull, AllegroSwing, DiminuendoSettle);

        public float SwordRotationAtProgress(float progress) => State == SwingState.LunarDash
            ? BaseRotation : BaseRotation + SwingAngleShiftAtProgress(progress) * Direction;

        public float SquishAtProgress(float progress) => State == SwingState.LunarDash
            ? 1 : MathHelper.Lerp(SquishVector.X, SquishVector.Y,
                (float)Math.Abs(Math.Sin(SwingAngleShiftAtProgress(progress))));

        public Vector2 DirectionAtProgress(float progress) => State == SwingState.LunarDash
            ? Projectile.velocity : SwordRotationAtProgress(progress).ToRotationVector2() * SquishAtProgress(progress);

        public float SwingAngleShift => SwingAngleShiftAtProgress(Progression);
        public float SwordRotation => SwordRotationAtProgress(Progression);
        public float CurrentSquish => SquishAtProgress(Progression);
        public Vector2 SwordDirection => DirectionAtProgress(Progression);

        // =====================================================================
        // TRAIL PRIMITIVES
        // =====================================================================

        public float TrailEndProgression
        {
            get
            {
                float end;
                if (Progression < 0.75f)
                    end = Progression - 0.5f + 0.1f * (Progression / 0.75f);
                else
                    end = Progression - 0.4f * (1 - (Progression - 0.75f) / 0.75f);
                return Math.Clamp(end, 0, 1);
            }
        }

        public float RealProgressionAtTrailCompletion(float completion)
            => MathHelper.Lerp(Progression, TrailEndProgression, completion);

        public Vector2 DirectionAtProgressScuffed(float progress)
        {
            float angleShift = SwingAngleShiftAtProgress(progress);
            Vector2 ap = angleShift.ToRotationVector2();
            ap.X *= SquishVector.X;
            ap.Y *= SquishVector.Y;
            angleShift = ap.ToRotation();
            return (BaseRotation + angleShift * Direction).ToRotationVector2() * SquishAtProgress(progress);
        }

        // Dash displacement curves
        public CurveSegment DashPullBack = new(SineBumpEasing, 0f, -8f, -12f);
        public CurveSegment DashThrust => new(PolyOutEasing, 1 - IncisorOfMoonlight.LungePercent, -8, 10f, 5);
        public float DashDisplace => PiecewiseAnimation(Progression, DashPullBack, DashThrust);

        // Dust spawn density curve
        public float DustDensity
        {
            get
            {
                if (Progression > 0.85f) return 0;
                if (Progression < 0.4f) return (float)Math.Pow(Progression / 0.3f, 2) * 0.2f;
                if (Progression < 0.5f) return 0.2f + 0.7f * (Progression - 0.4f) / 0.1f;
                return 0.9f;
            }
        }

        // =====================================================================
        // SETUP
        // =====================================================================

        public override string Texture => "MagnumOpus/Content/MoonlightSonata/Weapons/IncisorOfMoonlight/IncisorOfMoonlight";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailingMode[Type] = 2;
            ProjectileID.Sets.TrailCacheLength[Type] = 120;
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 80;
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

        public override void SendExtraAI(BinaryWriter writer) { writer.Write(SwingTime); writer.Write(SquishFactor); }
        public override void ReceiveExtraAI(BinaryReader reader) { SwingTime = reader.ReadSingle(); SquishFactor = reader.ReadSingle(); }
        public override bool ShouldUpdatePosition() => State == SwingState.LunarDash && !InPostDashStasis;

        public override bool? CanDamage()
        {
            if (State != SwingState.LunarDash) return null;
            if (InPostDashStasis) return false;
            if (Projectile.timeLeft > SwingTime * IncisorOfMoonlight.LungePercent) return false;
            return null;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float _ = 0f;
            Vector2 start = Projectile.Center;
            Vector2 end = start + SwordDirection * (BladeLength + 40) * Projectile.scale;
            float width = State == SwingState.LunarDash ? Projectile.scale * 40 : Projectile.scale * 26f;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), start, end, width, ref _);
        }

        // =====================================================================
        // INITIALIZATION
        // =====================================================================

        public void InitializationEffects(bool startInit)
        {
            Projectile.velocity = Owner.MountedCenter.DirectionTo(Main.MouseWorld);
            SquishFactor = Main.rand.NextFloat(0.67f, 1f);

            if (startInit && State != SwingState.LunarDash)
                Projectile.scale = 0.02f;
            else
            {
                Projectile.scale = 1f;
                if (PerformingPowerfulSlash)
                    State = SwingState.Swinging;
            }

            if (PerformingPowerfulSlash) SquishFactor = 0.7f;
            SwingTime = GetSwingTime;
            Projectile.timeLeft = (int)SwingTime;
            Projectile.netUpdate = true;
        }

        // =====================================================================
        // MAIN AI
        // =====================================================================

        public override void AI()
        {
            if (InPostDashStasis || Projectile.timeLeft == 0) return;

            if (Projectile.timeLeft >= 9999 || (Projectile.timeLeft == 1 && Owner.channel && State != SwingState.LunarDash))
                InitializationEffects(Projectile.timeLeft >= 9999);

            switch (State)
            {
                case SwingState.Swinging: DoBehavior_Swinging(); break;
                case SwingState.LunarDash: DoBehavior_LunarDash(); break;
            }

            // Glue to owner
            Projectile.Center = Owner.RotatedRelativePoint(Owner.MountedCenter, true);
            Owner.heldProj = Projectile.whoAmI;
            Owner.SetDummyItemTime(2);
            Owner.ChangeDir(Direction);

            // Arm rotation
            float armRot = SwordRotation - MathHelper.PiOver2;
            Owner.SetCompositeArmFront(Math.Abs(armRot) > 0.01f, Player.CompositeArmStretchAmount.Full, armRot);

            // Post-dash stasis
            if (Projectile.timeLeft == 1 && State == SwingState.LunarDash && !InPostDashStasis)
            {
                Projectile.timeLeft = IncisorOfMoonlight.LungeCooldown;
                InPostDashStasis = true;
                Owner.fullRotation = 0f;
                Owner.Incisor().LungingDown = false;
            }
        }

        // =====================================================================
        // SWINGING BEHAVIOR 窶・resonance arc + homing beams + music dusts
        // =====================================================================

        public void DoBehavior_Swinging()
        {
            if (Projectile.timeLeft == (int)(SwingTime / 5))
                SoundEngine.PlaySound(SoundID.Item71 with { PitchVariance = 0.3f, Volume = 0.7f }, Projectile.Center);

            // Moonlight glow from blade
            Vector3 lightColor = new(0.45f, 0.35f, 0.75f);
            Lighting.AddLight(Owner.MountedCenter + SwordDirection * 90, lightColor * 1.4f
                * (float)Math.Sin(Progression * MathHelper.Pi));

            // Scale animation
            if (Projectile.scale < IdealSize)
                Projectile.scale = MathHelper.Lerp(Projectile.scale, IdealSize, 0.08f);
            if (!Owner.channel && Progression > 0.7f)
                Projectile.scale = (0.5f + 0.5f * (float)Math.Pow(1 - (Progression - 0.7f) / 0.3f, 0.5)) * IdealSize;

                // Constellation sparks shed from blade edge during swing
            if (Main.rand.NextFloat() < DustDensity * 0.5f)
            {
                float bladeT = (float)Math.Pow(Main.rand.NextFloat(0.4f, 1f), 0.5f);
                Vector2 sparkPos = Owner.MountedCenter + SwordDirection * BladeLength * Projectile.scale * bladeT;
                Vector2 sparkVel = SwordDirection.RotatedBy(-MathHelper.PiOver2 * Direction) * Main.rand.NextFloat(2f, 5f);
                Color sparkColor = MulticolorLerp(Main.rand.NextFloat(), IncisorPalette);
                var spark = new ConstellationSparkParticle(
                    sparkPos, sparkVel, false,
                    Main.rand.Next(14, 24), Main.rand.NextFloat(0.12f, 0.28f), sparkColor,
                    new Vector2(0.6f, 1.4f), quickShrink: true);
                IncisorParticleHandler.SpawnParticle(spark);
            }

            // Lunar mote particles along the blade during swing
            if (Main.rand.NextFloat() < DustDensity * 0.7f && Progression > 0.25f)
            {
                float bladeT = (float)Math.Pow(Main.rand.NextFloat(0.3f, 1f), 0.5f);
                Vector2 motePos = Owner.MountedCenter + SwordDirection * BladeLength * Projectile.scale * bladeT;
                Vector2 moteVel = SwordDirection.RotatedBy(-MathHelper.PiOver2 * Direction) * Main.rand.NextFloat(2f, 5f);
                Color moteColor = MulticolorLerp(Main.rand.NextFloat(),
                    new Color(170, 140, 255), new Color(135, 206, 250), new Color(220, 230, 255));
                var mote = new LunarMoteParticle(
                    motePos, moteVel, Main.rand.NextFloat(0.2f, 0.5f), moteColor,
                    Main.rand.Next(18, 30), 2.5f, 3.8f, hueShift: 0.01f);
                IncisorParticleHandler.SpawnParticle(mote);
            }

            // Constellation spark bursts at blade tip during fast swing phase
            if (Progression > 0.35f && Progression < 0.8f && Main.rand.NextBool(4))
            {
                Vector2 tipPos = Owner.MountedCenter + SwordDirection * BladeLength * Projectile.scale;
                Vector2 sparkVel = SwordDirection.RotatedByRandom(MathHelper.PiOver4 * 0.5f)
                    * Main.rand.NextFloat(3f, 7f);
                Color sparkColor = MulticolorLerp(Main.rand.NextFloat(), IncisorPalette);
                var spark = new ConstellationSparkParticle(
                    tipPos, sparkVel, true,
                    Main.rand.Next(12, 22), Main.rand.NextFloat(0.15f, 0.35f), sparkColor,
                    new Vector2(1f, 0.4f));
                IncisorParticleHandler.SpawnParticle(spark);
            }

            // Moonlight mist clouds near player during powerful swings
            if (PerformingPowerfulSlash && Progression > 0.3f && Main.rand.NextBool(8))
            {
                Vector2 mistPos = Owner.MountedCenter + SwordDirection * Main.rand.NextFloat(20f, 60f)
                    + Main.rand.NextVector2Circular(30f, 30f);
                Color mistColor = Color.Lerp(new Color(90, 50, 160), new Color(170, 140, 255), Main.rand.NextFloat()) * 0.4f;
                var mist = new MoonlightMistParticle(
                    mistPos, Main.rand.NextVector2Circular(1.5f, 1.5f), mistColor,
                    Main.rand.Next(25, 40), Main.rand.NextFloat(0.3f, 0.6f), 0.35f,
                    Main.rand.NextFloat(-0.02f, 0.02f));
                IncisorParticleHandler.SpawnParticle(mist);
            }

            // Fire homing lunar beams during 60窶・00% of swing
            int beamStart = (int)(SwingTime * 0.6f);
            int beamPeriod = (int)(SwingTime * 0.4f);
            int beamEnd = beamStart + beamPeriod;
            int beamsPerSwing = IncisorOfMoonlight.BeamsPerSwing;
            beamPeriod /= Math.Max(beamsPerSwing - 1, 1);

            if (Main.myPlayer == Projectile.owner && Timer >= beamStart && Timer < beamEnd
                && (Timer - beamStart) % beamPeriod == 0)
            {
                int dmg = (int)(Projectile.damage * IncisorOfMoonlight.NotTrueMeleePenalty);
                Vector2 boltVel = Projectile.velocity.RotatedByRandom(MathHelper.PiOver4 * 0.3);
                boltVel *= Owner.HeldItem.shootSpeed;
                Projectile.NewProjectile(Projectile.GetSource_FromAI(),
                    Projectile.Center + boltVel * 5f, boltVel,
                    ModContent.ProjectileType<LunarBeamProj>(),
                    dmg, Projectile.knockBack / 3f, Projectile.owner);
            }
        }

        // =====================================================================
        // LUNAR DASH BEHAVIOR 窶・constellation pierce through enemies
        // =====================================================================

        public void DoBehavior_LunarDash()
        {
            Owner.mount?.Dismount(Owner);
            Owner.RemoveAllGrapplingHooks();

            if (LungeProgression == 0)
            {
                if (Projectile.timeLeft == 1 + (int)(SwingTime * IncisorOfMoonlight.LungePercent))
                    SoundEngine.PlaySound(SoundID.Item163 with { Volume = 0.6f, PitchVariance = 0.2f }, Projectile.Center);

                Projectile.velocity = Owner.MountedCenter.DirectionTo(Main.MouseWorld);
                Projectile.oldPos = new Vector2[Projectile.oldPos.Length];
                for (int i = 0; i < Projectile.oldPos.Length; ++i)
                    Projectile.oldPos[i] = Projectile.position;
            }
            else
            {
                float rotStrength = MathHelper.PiOver4 * 0.05f * (float)Math.Pow(LungeProgression, 3);
                float curRot = Projectile.velocity.ToRotation();
                float idealRot = Owner.MountedCenter.DirectionTo(Main.MouseWorld).ToRotation();
                Projectile.velocity = curRot.AngleTowards(idealRot, rotStrength).ToRotationVector2();

                Owner.fallStart = (int)(Owner.position.Y / 16f);
                float velPower = (float)Math.Pow(Math.Abs(Math.Sin(MathHelper.Pi * LungeProgression)), 0.6f);
                Vector2 newVel = Projectile.velocity * IncisorOfMoonlight.LungeSpeed * (0.24f + 0.76f * velPower);
                Owner.velocity = newVel;
                Owner.Incisor().LungingDown = true;

                    // Constellation sparks along dash path
                if (Main.rand.NextBool(3))
                {
                    Color sc = MulticolorLerp(Main.rand.NextFloat(), IncisorPalette);
                    Vector2 sparkVel = SwordDirection * -1 * Main.rand.NextFloat(3f, 8f)
                        + Main.rand.NextVector2Circular(2f, 2f);
                    var spark = new ConstellationSparkParticle(
                        Owner.MountedCenter + Main.rand.NextVector2Circular(16f, 16f),
                        sparkVel, false, Main.rand.Next(10, 18),
                        Main.rand.NextFloat(0.15f, 0.3f), sc,
                        new Vector2(0.5f, 1.6f), quickShrink: true);
                    IncisorParticleHandler.SpawnParticle(spark);
                }

                // Lunar mote energy streaks during dash
                if (Main.rand.NextBool(6) && LungeProgression < 0.8f)
                {
                    Vector2 pVel = SwordDirection * -1 * Main.rand.NextFloat(6f, 10f);
                    Color mc = MulticolorLerp(Main.rand.NextFloat(),
                        new Color(170, 140, 255), new Color(135, 206, 250), new Color(220, 230, 255));
                    var mote = new LunarMoteParticle(
                        Owner.MountedCenter + Main.rand.NextVector2Circular(20f, 20f) + Owner.velocity * 5,
                        pVel, Main.rand.NextFloat(0.3f, 0.6f), mc, 30, 3.4f, 4.5f, hueShift: 0.015f);
                    IncisorParticleHandler.SpawnParticle(mote);
                }

                // Trailing motes after dash
                if (Main.rand.NextBool(5) && LungeProgression >= 0.8f)
                {
                    Vector2 pVel = SwordDirection * -1 * Main.rand.NextFloat(6f, 10f);
                    Color mc = MulticolorLerp(Main.rand.NextFloat(), IncisorPalette);
                    var mote = new LunarMoteParticle(
                        Owner.MountedCenter + Main.rand.NextVector2Circular(50f, 50f) + Owner.velocity * 4,
                        pVel, Main.rand.NextFloat(0.3f, 0.6f), mc, 30, 3.4f, 4.5f, hueShift: 0.015f);
                    IncisorParticleHandler.SpawnParticle(mote);
                }
            }

            if (Projectile.timeLeft == 1)
                Owner.velocity *= 0.2f;

            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4 * Direction;
        }

        // =====================================================================
        // DRAWING 窶・Three layers: Slash arc 竊・Pierce trail 竊・Blade sprite
        // =====================================================================

        public float SlashWidthFunction(float cr, Vector2 vp)
            => SquishAtProgress(RealProgressionAtTrailCompletion(cr)) * Projectile.scale * 58f;

        public Color SlashColorFunction(float cr, Vector2 vp)
            => new Color(170, 140, 255) * Utils.GetLerpValue(0.95f, 0.3f, cr, true) * Projectile.Opacity;

        public float SlashBloomWidthFunction(float cr, Vector2 vp)
            => SquishAtProgress(RealProgressionAtTrailCompletion(cr)) * Projectile.scale * 85f;

        public Color SlashBloomColorFunction(float cr, Vector2 vp)
            => new Color(120, 80, 200) * Utils.GetLerpValue(0.9f, 0.35f, cr, true) * Projectile.Opacity * 0.3f;

        public float PierceWidthFunction(float cr, Vector2 vp)
        {
            float w = Utils.GetLerpValue(0f, 0.2f, cr, true) * Projectile.scale * 44f;
            w *= 1 - (float)Math.Pow(LungeProgression, 5);
            return w;
        }

        public Color PierceColorFunction(float cr, Vector2 vp)
            => new Color(170, 140, 255) * Projectile.Opacity;

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
            if (Projectile.Opacity <= 0f || InPostDashStasis) return false;
            DrawSlash();
            DrawPierceTrail();
            DrawBlade();
            DrawConstellationFlare();
            return false;
        }

        /// <summary>
        /// Constellation star node flare at the blade tip 窶・a sharp 4-pointed star
        /// with a soft glow undertone. Only visible during the active swing arc.
        /// "Each cut reveals the stars beneath reality."
        /// </summary>
        private void DrawConstellationFlare()
        {
            if (State != SwingState.Swinging || Progression < 0.25f || Progression > 0.85f)
                return;

            var starTex = ModContent.Request<Texture2D>(
                "MagnumOpus/Assets/Particles Asset Library/Stars/4PointedStarHard", AssetRequestMode.ImmediateLoad).Value;
            var bloomTex = ModContent.Request<Texture2D>(
                "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftRadialBloom", AssetRequestMode.ImmediateLoad).Value;

            Vector2 tipPos = Owner.MountedCenter + SwordDirection * BladeLength * Projectile.scale
                - Main.screenPosition;

            float starPulse = 0.6f + 0.4f * MathF.Sin(Timer * 0.15f);
            float opacity = MathF.Sin(MathHelper.Pi * (Progression - 0.25f) / 0.6f);

            // Soft glow underlayer 窶・violet constellation nebula
            Color glowColor = MulticolorLerp(Progression, IncisorPalette) with { A = 0 };
            Main.spriteBatch.Draw(bloomTex, tipPos, null, glowColor * opacity * 0.3f,
                0f, bloomTex.Size() * 0.5f, 0.4f * starPulse * Projectile.scale,
                SpriteEffects.None, 0f);

            // Sharp 4-pointed star 窶・constellation node revelation
            Color starColor = Color.Lerp(new Color(230, 235, 255),
                new Color(170, 140, 255), Progression) with { A = 0 };
            Main.spriteBatch.Draw(starTex, tipPos, null, starColor * opacity * 0.8f,
                SwordRotation, starTex.Size() * 0.5f, 0.2f * starPulse * Projectile.scale,
                SpriteEffects.None, 0f);

            // Secondary star at 45ﾂｰ offset 窶・cross pattern
            Main.spriteBatch.Draw(starTex, tipPos, null, starColor * opacity * 0.4f,
                SwordRotation + MathHelper.PiOver4, starTex.Size() * 0.5f,
                0.15f * starPulse * Projectile.scale, SpriteEffects.None, 0f);
        }

        public void DrawSlash()
        {
            if (State != SwingState.Swinging || Progression < 0.3f) return;

            var slashPoints = GenerateSlashPoints();

            // Additive bloom glow pass behind the slash arc
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive,
                Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer,
                null, Main.GameViewMatrix.TransformationMatrix);

            GameShaders.Misc["MagnumOpus:IncisorSlash"].UseImage1(
                ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/NoiseTextures/VoronoiNoise"));
            GameShaders.Misc["MagnumOpus:IncisorSlash"].UseColor(new Color(140, 100, 220));
            GameShaders.Misc["MagnumOpus:IncisorSlash"].UseSecondaryColor(new Color(70, 30, 130));
            GameShaders.Misc["MagnumOpus:IncisorSlash"].Shader.Parameters["fireColor"]
                .SetValue(new Color(100, 160, 230).ToVector3());
            GameShaders.Misc["MagnumOpus:IncisorSlash"].Shader.Parameters["flipped"]
                .SetValue(Direction == 1);
            GameShaders.Misc["MagnumOpus:IncisorSlash"].Shader.Parameters["uIntensity"]
                .SetValue(PerformingPowerfulSlash ? 0.7f : 0.35f);
            GameShaders.Misc["MagnumOpus:IncisorSlash"].Apply();

            IncisorPrimitiveRenderer.RenderTrail(slashPoints,
                new(SlashBloomWidthFunction, SlashBloomColorFunction, (_, _) => Projectile.Center,
                    shader: GameShaders.Misc["MagnumOpus:IncisorSlash"]), 95);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend,
                Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer,
                null, Main.GameViewMatrix.TransformationMatrix);

            // Core slash arc pass
            GameShaders.Misc["MagnumOpus:IncisorSlash"].UseImage1(
                ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/NoiseTextures/VoronoiNoise"));
            GameShaders.Misc["MagnumOpus:IncisorSlash"].UseColor(new Color(230, 235, 255));
            GameShaders.Misc["MagnumOpus:IncisorSlash"].UseSecondaryColor(new Color(90, 50, 160));
            GameShaders.Misc["MagnumOpus:IncisorSlash"].Shader.Parameters["fireColor"]
                .SetValue(new Color(135, 206, 250).ToVector3());
            GameShaders.Misc["MagnumOpus:IncisorSlash"].Shader.Parameters["flipped"]
                .SetValue(Direction == 1);
            GameShaders.Misc["MagnumOpus:IncisorSlash"].Shader.Parameters["uIntensity"]
                .SetValue(PerformingPowerfulSlash ? 1f : 0.65f);
            GameShaders.Misc["MagnumOpus:IncisorSlash"].Apply();

            IncisorPrimitiveRenderer.RenderTrail(slashPoints,
                new(SlashWidthFunction, SlashColorFunction, (_, _) => Projectile.Center,
                    shader: GameShaders.Misc["MagnumOpus:IncisorSlash"]), 95);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer,
                null, Main.GameViewMatrix.TransformationMatrix);
        }

        public void DrawPierceTrail()
        {
            if (State != SwingState.LunarDash) return;

            Main.spriteBatch.EnterShaderRegion();

            Color mainColor = MulticolorLerp((Main.GlobalTimeWrappedHourly * 1.5f) % 1, IncisorPalette);
            Color secColor = MulticolorLerp((Main.GlobalTimeWrappedHourly * 1.5f + 0.2f) % 1, IncisorPalette);
            mainColor = Color.Lerp(Color.White, mainColor, 0.4f + 0.6f * (float)Math.Pow(LungeProgression, 0.5f));
            secColor = Color.Lerp(Color.White, secColor, 0.4f + 0.6f * (float)Math.Pow(LungeProgression, 0.5f));

            Vector2 trailOffset = (Projectile.rotation - Direction * MathHelper.PiOver4).ToRotationVector2() * 80f + Projectile.Size * 0.5f;

            GameShaders.Misc["MagnumOpus:IncisorPierce"].UseImage1(
                ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/TrailsAndRibbons/BasicTrail"));
            GameShaders.Misc["MagnumOpus:IncisorPierce"].UseImage2(
                ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/NoiseTextures/PerlinNoise"));
            GameShaders.Misc["MagnumOpus:IncisorPierce"].UseColor(mainColor);
            GameShaders.Misc["MagnumOpus:IncisorPierce"].UseSecondaryColor(secColor);
            GameShaders.Misc["MagnumOpus:IncisorPierce"].Apply();

            var positions = Projectile.oldPos.Take(60).ToArray();
            IncisorPrimitiveRenderer.RenderTrail(positions,
                new(PierceWidthFunction, PierceColorFunction, (_, _) => trailOffset,
                    shader: GameShaders.Misc["MagnumOpus:IncisorPierce"]), 30);

            Main.spriteBatch.ExitShaderRegion();
        }

        public void DrawBlade()
        {
            var texture = Terraria.GameContent.TextureAssets.Projectile[Type].Value;
            SpriteEffects dir = Direction == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            if (State == SwingState.Swinging)
            {
                // Swing sprite shader 窶・rotates the square weapon texture
                Effect swingFX = Filters.Scene["MagnumOpus:IncisorSwingSprite"].GetShader().Shader;
                swingFX.Parameters["rotation"].SetValue(SwingAngleShift + MathHelper.PiOver4
                    + (Direction == -1 ? MathHelper.Pi : 0f));
                swingFX.Parameters["pommelToOriginPercent"].SetValue(0.05f);
                swingFX.Parameters["color"].SetValue(Color.White.ToVector4());

                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend,
                    Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer,
                    swingFX, Main.GameViewMatrix.TransformationMatrix);

                Main.EntitySpriteDraw(texture, Owner.MountedCenter - Main.screenPosition, null,
                    Color.White, BaseRotation, texture.Size() / 2f,
                    SquishVector * 2.2f * Projectile.scale, dir, 0);

                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                    Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer,
                    null, Main.GameViewMatrix.TransformationMatrix);
            }
            else
            {
                // Dash mode: raw sprite + bloom afterimages
                float rotation = BaseRotation + MathHelper.PiOver4;
                Vector2 origin = new(0, texture.Height);
                Vector2 drawPos = Projectile.Center + Projectile.velocity * Projectile.scale * DashDisplace
                    - Main.screenPosition;

                if (Direction == -1)
                {
                    rotation += MathHelper.PiOver2;
                    origin.X = texture.Width;
                }

                Projectile.scale = MathHelper.Lerp(1f, 0.22f, MathF.Pow(LungeProgression, 7));
                Main.EntitySpriteDraw(texture, drawPos, null, Color.White, rotation, origin,
                    Projectile.scale, dir, 0);

                // Purple-silver bloom afterimages
                float energyPower = Utils.GetLerpValue(0f, 0.32f, Progression, true)
                    * Utils.GetLerpValue(1f, 0.85f, Progression, true);
                for (int i = 0; i < 4; i++)
                {
                    Vector2 offset = (MathHelper.TwoPi * i / 4f + BaseRotation).ToRotationVector2()
                        * energyPower * Projectile.scale * 6f;
                    Color afterColor = Color.Lerp(new Color(170, 140, 255), new Color(135, 206, 250), Progression);
                    afterColor.A = 0;
                    Main.spriteBatch.Draw(texture, drawPos + offset, null, afterColor * 0.14f,
                        rotation, origin, Projectile.scale, dir, 0);
                }
            }
        }

        // =====================================================================
        // ON-HIT 窶・Dash freeze + slash chain OR empowered nova + lifesteal
        // =====================================================================

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            ItemLoader.OnHitNPC(Owner.HeldItem, Owner, target, hit, damageDone);
            NPCLoader.OnHitByItem(target, Owner, Owner.HeldItem, hit, damageDone);
            PlayerLoader.OnHitNPC(Owner, target, hit, damageDone);

            if (State == SwingState.LunarDash)
            {
                // Rebound
                Owner.itemAnimation = 0;
                Owner.velocity = Owner.SafeDirectionTo(target.Center) * -IncisorOfMoonlight.ReboundSpeed;
                Projectile.timeLeft = IncisorOfMoonlight.OpportunityForBigSlash + IncisorOfMoonlight.LungeCooldown;
                InPostDashStasis = true;
                Projectile.netUpdate = true;

                SoundEngine.PlaySound(SoundID.Item125 with { Volume = 0.8f }, target.Center);

                // Spawn constellation slash creators
                if (Main.myPlayer == Projectile.owner)
                {
                    int lungeDmg = (int)(Projectile.damage * IncisorOfMoonlight.LungeDamageFactor);
                    for (int i = 0; i < 5; i++)
                    {
                        int slash = Projectile.NewProjectile(Projectile.GetSource_FromAI(),
                            target.Center, Projectile.velocity * 0.1f,
                            ModContent.ProjectileType<ConstellationSlashCreator>(),
                            lungeDmg, 0f, Projectile.owner, target.whoAmI, 100);
                        if (Main.projectile.IndexInRange(slash))
                            Main.projectile[slash].timeLeft -= i * 4;
                    }
                }

                // Moonlit stasis freeze
                target.AddBuff(ModContent.BuffType<MoonlitStasis>(), 60);
            }

            // Empowered swing: Lunar Nova explosion + lifesteal
            if (State == SwingState.Swinging && PerformingPowerfulSlash
                && Owner.ownedProjectileCounts[ModContent.ProjectileType<LunarNova>()] < 1)
            {
                SoundEngine.PlaySound(SoundID.Item122 with { Volume = 0.9f, Pitch = -0.1f }, Projectile.Center);
                if (Main.myPlayer == Projectile.owner)
                {
                    int novaDmg = (int)(Projectile.damage * IncisorOfMoonlight.ExplosionDamageFactor);
                    Projectile.NewProjectile(Projectile.GetSource_FromAI(), target.Center, Vector2.Zero,
                        ModContent.ProjectileType<LunarNova>(), novaDmg, 0f, Projectile.owner);
                }
                Owner.DoLifestealDirect(target, (int)Math.Round(hit.Damage * 0.04), 0.4f);

                // Musical climax 窶・hue-shifting notes erupt from the Lunar Nova
                if (!Main.dedServ)
                    MoonlightVFXLibrary.SpawnMusicNotes(target.Center, count: 6, spread: 35f,
                        minScale: 0.7f, maxScale: 1.1f, lifetime: 50);
            }
        }

        public override void OnKill(int timeLeft)
        {
            Owner.fullRotation = 0f;
            Owner.Incisor().LungingDown = false;
        }
    }
}
