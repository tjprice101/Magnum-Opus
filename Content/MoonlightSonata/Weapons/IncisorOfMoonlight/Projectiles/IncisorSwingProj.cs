using System;
using System.Collections.Generic;
using System.IO;
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
    /// Incisor of Moonlight — Main swing projectile.
    /// "The Stellar Scalpel" — precision cuts that reveal constellations beneath reality.
    ///
    /// Channelled resonance swing — shader-driven slash arc, constellation node sparks,
    /// homing lunar beams fired during the swing arc.
    /// </summary>
    public class IncisorSwingProj : ModProjectile
    {
        public Player Owner => Main.player[Projectile.owner];
        const float BladeLength = 150;
        const float TextureDrawScale = 0.108f;

        // =====================================================================
        // TIMING
        // =====================================================================

        public int GetSwingTime => 72;

        public float Timer => SwingTime - Projectile.timeLeft;
        public float Progression => Timer / SwingTime;

        // =====================================================================
        // STATE
        // =====================================================================

        public ref float SwingTime => ref Projectile.localAI[0];
        public ref float SquishFactor => ref Projectile.localAI[1];

        // =====================================================================
        // ANGLES AND ANIMATION CURVES
        // =====================================================================

        public int Direction => Math.Sign(Projectile.velocity.X) <= 0 ? -1 : 1;
        public float BaseRotation => Projectile.velocity.ToRotation();
        public Vector2 SquishVector => new(1f + (1 - SquishFactor) * 0.6f, SquishFactor);

        public static float MaxSwingAngle = MathHelper.PiOver2 * 1.75f;

        // Musical movement: Grave (slow pull) → Allegro (fast swing) → Diminuendo (settle)
        public CurveSegment GravePull = new(PolyOutEasing, 0f, -1f, 0.28f, 2);
        public CurveSegment AllegroSwing = new(PolyInEasing, 0.25f, -0.72f, 1.62f, 4);
        public CurveSegment DiminuendoSettle = new(PolyOutEasing, 0.83f, 0.9f, 0.1f, 2);

        public float SwingAngleShiftAtProgress(float progress)
            => MaxSwingAngle * PiecewiseAnimation(progress, GravePull, AllegroSwing, DiminuendoSettle);

        public float SwordRotationAtProgress(float progress)
            => BaseRotation + SwingAngleShiftAtProgress(progress) * Direction;

        public float SquishAtProgress(float progress)
            => MathHelper.Lerp(SquishVector.X, SquishVector.Y,
                (float)Math.Abs(Math.Sin(SwingAngleShiftAtProgress(progress))));

        public Vector2 DirectionAtProgress(float progress)
            => SwordRotationAtProgress(progress).ToRotationVector2() * SquishAtProgress(progress);

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
        public override bool ShouldUpdatePosition() => false;

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float _ = 0f;
            Vector2 start = Projectile.Center;
            Vector2 end = start + SwordDirection * (BladeLength + 40) * Projectile.scale;
            float width = Projectile.scale * 26f;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), start, end, width, ref _);
        }

        // =====================================================================
        // INITIALIZATION
        // =====================================================================

        public void InitializationEffects(bool startInit)
        {
            Projectile.velocity = Owner.MountedCenter.DirectionTo(Main.MouseWorld);
            SquishFactor = Main.rand.NextFloat(0.67f, 1f);

            if (startInit)
                Projectile.scale = 0.02f;
            else
                Projectile.scale = 1f;

            SwingTime = GetSwingTime;
            Projectile.timeLeft = (int)SwingTime;
            Projectile.netUpdate = true;
        }

        // =====================================================================
        // MAIN AI
        // =====================================================================

        public override void AI()
        {
            if (Projectile.timeLeft == 0) return;

            if (Projectile.timeLeft >= 9999 || (Projectile.timeLeft == 1 && Owner.channel))
                InitializationEffects(Projectile.timeLeft >= 9999);

            DoBehavior_Swinging();

            // Glue to owner
            Projectile.Center = Owner.RotatedRelativePoint(Owner.MountedCenter, true);
            Owner.heldProj = Projectile.whoAmI;
            Owner.SetDummyItemTime(2);
            Owner.ChangeDir(Direction);

            // Arm rotation
            float armRot = SwordRotation - MathHelper.PiOver2;
            Owner.SetCompositeArmFront(Math.Abs(armRot) > 0.01f, Player.CompositeArmStretchAmount.Full, armRot);
        }

        // =====================================================================
        // SWINGING BEHAVIOR — resonance arc + homing beams + music dusts
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
            if (Projectile.scale < 1f)
                Projectile.scale = MathHelper.Lerp(Projectile.scale, 1f, 0.08f);
            if (!Owner.channel && Progression > 0.7f)
                Projectile.scale = (0.5f + 0.5f * (float)Math.Pow(1 - (Progression - 0.7f) / 0.3f, 0.5));

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

            // Fire homing lunar beams during 60–100% of swing
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
        // DRAWING — Two layers: Slash arc → Blade sprite
        // =====================================================================

        public float SlashWidthFunction(float cr, Vector2 vp)
            => SquishAtProgress(RealProgressionAtTrailCompletion(cr)) * Projectile.scale * 58f;

        public Color SlashColorFunction(float cr, Vector2 vp)
            => new Color(170, 140, 255) * Utils.GetLerpValue(0.95f, 0.3f, cr, true) * Projectile.Opacity;

        public float SlashBloomWidthFunction(float cr, Vector2 vp)
            => SquishAtProgress(RealProgressionAtTrailCompletion(cr)) * Projectile.scale * 85f;

        public Color SlashBloomColorFunction(float cr, Vector2 vp)
            => new Color(120, 80, 200) * Utils.GetLerpValue(0.9f, 0.35f, cr, true) * Projectile.Opacity * 0.3f;

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
            SpriteBatch sb = Main.spriteBatch;
            try
            {
                if (Projectile.Opacity <= 0f) return false;
                DrawSlash();
                DrawBlade();
                DrawConstellationFlare();
            }
            catch { }
            finally
            {
                try { sb.End(); } catch { }
                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                    DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            }

            return false;
        }

        /// <summary>
        /// Constellation star node flare at the blade tip — a sharp 4-pointed star
        /// with a soft glow undertone. Only visible during the active swing arc.
        /// </summary>
        private void DrawConstellationFlare()
        {
            if (Progression < 0.25f || Progression > 0.85f)
                return;

            var starTex = ModContent.Request<Texture2D>(
                "MagnumOpus/Assets/Particles Asset Library/Stars/4PointedStarHard", AssetRequestMode.ImmediateLoad).Value;
            var bloomTex = ModContent.Request<Texture2D>(
                "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftRadialBloom", AssetRequestMode.ImmediateLoad).Value;

            Vector2 tipPos = Owner.MountedCenter + SwordDirection * BladeLength * Projectile.scale
                - Main.screenPosition;

            float starPulse = 0.6f + 0.4f * MathF.Sin(Timer * 0.15f);
            float opacity = MathF.Sin(MathHelper.Pi * (Progression - 0.25f) / 0.6f);

            // Soft glow underlayer — violet constellation nebula
            Color glowColor = MulticolorLerp(Progression, IncisorPalette) with { A = 0 };
            Main.spriteBatch.Draw(bloomTex, tipPos, null, glowColor * opacity * 0.3f,
                0f, bloomTex.Size() * 0.5f, 0.1f * starPulse * Projectile.scale,
                SpriteEffects.None, 0f);

            // Sharp 4-pointed star — constellation node revelation
            Color starColor = Color.Lerp(new Color(230, 235, 255),
                new Color(170, 140, 255), Progression) with { A = 0 };
            Main.spriteBatch.Draw(starTex, tipPos, null, starColor * opacity * 0.8f,
                SwordRotation, starTex.Size() * 0.5f, 0.2f * starPulse * Projectile.scale,
                SpriteEffects.None, 0f);

            // Secondary star at 45° offset — cross pattern
            Main.spriteBatch.Draw(starTex, tipPos, null, starColor * opacity * 0.4f,
                SwordRotation + MathHelper.PiOver4, starTex.Size() * 0.5f,
                0.15f * starPulse * Projectile.scale, SpriteEffects.None, 0f);
        }

        public void DrawSlash()
        {
            if (Progression < 0.3f) return;

            var slashPoints = GenerateSlashPoints();

            // Additive bloom glow pass behind the slash arc
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, MagnumBlendStates.ShaderAdditive,
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
                .SetValue(0.35f);
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
                .SetValue(0.65f);
            GameShaders.Misc["MagnumOpus:IncisorSlash"].Apply();

            IncisorPrimitiveRenderer.RenderTrail(slashPoints,
                new(SlashWidthFunction, SlashColorFunction, (_, _) => Projectile.Center,
                    shader: GameShaders.Misc["MagnumOpus:IncisorSlash"]), 95);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer,
                null, Main.GameViewMatrix.TransformationMatrix);
        }

        public void DrawBlade()
        {
            var texture = Terraria.GameContent.TextureAssets.Projectile[Type].Value;
            SpriteEffects dir = Direction == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            // Swing sprite shader — rotates the square weapon texture
            Effect swingFX = Filters.Scene["MagnumOpus:IncisorSwingSprite"].GetShader().Shader;
            swingFX.Parameters["rotation"].SetValue(SwingAngleShift + MathHelper.PiOver4
                + (Direction == -1 ? MathHelper.Pi : 0f));
            swingFX.Parameters["pommelToOriginPercent"].SetValue(0.05f);
            swingFX.Parameters["color"].SetValue(Color.White.ToVector4());

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend,
                Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer,
                swingFX, Main.GameViewMatrix.TransformationMatrix);

            // Offset blade forward along swing direction to align with smear arc
            Vector2 bladeDrawPos = Owner.MountedCenter + SwordDirection * 14f * Projectile.scale - Main.screenPosition;
            Main.EntitySpriteDraw(texture, bladeDrawPos, null,
                Color.White, BaseRotation, texture.Size() / 2f,
                SquishVector * 3.5f * Projectile.scale * TextureDrawScale, dir, 0);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer,
                null, Main.GameViewMatrix.TransformationMatrix);
        }

        // =====================================================================
        // ON-HIT
        // =====================================================================

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            ItemLoader.OnHitNPC(Owner.HeldItem, Owner, target, hit, damageDone);
            NPCLoader.OnHitByItem(target, Owner, Owner.HeldItem, hit, damageDone);
            PlayerLoader.OnHitNPC(Owner, target, hit, damageDone);
        }

        public override void OnKill(int timeLeft)
        {
            Owner.fullRotation = 0f;
            Owner.Incisor().LungingDown = false;
        }
    }
}
