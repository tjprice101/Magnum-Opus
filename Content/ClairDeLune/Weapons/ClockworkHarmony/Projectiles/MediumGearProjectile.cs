using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.Shaders;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.ClairDeLune.Weapons.ClockworkHarmony.Projectiles
{
    /// <summary>
    /// Medium Gear — Arc-lobbed bouncing gear (32px, 10°/frame, bounces 3x).
    /// Part of ClockworkHarmony's gear mesh system, mid-weight transfer gear.
    /// 3 render passes: (1) GearSwing GearSwingTrail medium gear body,
    /// (2) ClairDeLunePearlGlow PearlShimmer bounce flash, (3) Multi-scale bloom + 12 teeth.
    /// </summary>
    public class MediumGearProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Textures/InvisibleProjectile";

        private const float GearRadius = 16f; // 32px diameter
        private const float SpinRate = MathHelper.Pi / 18f; // 10°/frame
        private const int ToothCount = 12;
        private int _bounceCount;
        private const int MaxBounces = 3;

        // --- Shader + texture caching ---
        private static Effect _gearSwingShader;
        private static Effect _pearlGlowShader;
        private static Asset<Texture2D> _softCircle;
        private static Asset<Texture2D> _softRadialBloom;
        private static Asset<Texture2D> _pointBloom;

        public override void SetDefaults()
        {
            Projectile.width = 32;
            Projectile.height = 32;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 2;
            Projectile.timeLeft = 150;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = false;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }

        public override void AI()
        {
            Projectile.rotation += SpinRate;
            Projectile.velocity.Y += 0.15f; // Arc gravity
            Projectile.velocity *= 0.998f;

            // Trail sparks
            if (Main.GameUpdateCount % 3 == 0)
            {
                float sparkAngle = Projectile.rotation + Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 sparkPos = Projectile.Center + sparkAngle.ToRotationVector2() * GearRadius * 0.8f;
                var spark = new GenericGlowParticle(sparkPos,
                    -Projectile.velocity * 0.1f,
                    ClairDeLunePalette.SoftBlue with { A = 0 } * 0.25f, 0.03f, 5, true);
                MagnumParticleHandler.SpawnParticle(spark);
            }

            Lighting.AddLight(Projectile.Center, ClairDeLunePalette.SoftBlue.ToVector3() * 0.2f);
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (_bounceCount >= MaxBounces)
                return true;

            _bounceCount++;
            SoundEngine.PlaySound(SoundID.Item10 with { Pitch = 0.1f * _bounceCount, Volume = 0.3f }, Projectile.Center);

            // Bounce reflection
            if (Math.Abs(Projectile.velocity.X - oldVelocity.X) > float.Epsilon)
                Projectile.velocity.X = -oldVelocity.X * 0.75f;
            if (Math.Abs(Projectile.velocity.Y - oldVelocity.Y) > float.Epsilon)
                Projectile.velocity.Y = -oldVelocity.Y * 0.75f;

            // Bounce spark burst
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f;
                Vector2 vel = angle.ToRotationVector2() * 2f;
                var bounce = new GenericGlowParticle(Projectile.Center, vel,
                    ClairDeLunePalette.MoonbeamGold with { A = 0 } * 0.4f, 0.05f, 6, true);
                MagnumParticleHandler.SpawnParticle(bounce);
            }

            return false;
        }

        private void LoadTextures()
        {
            _softCircle ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/MasksAndShapes/SoftCircle", AssetRequestMode.ImmediateLoad);
            _softRadialBloom ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftRadialBloom", AssetRequestMode.ImmediateLoad);
            _pointBloom ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/PointBloom", AssetRequestMode.ImmediateLoad);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (Main.dedServ) return false;
            LoadTextures();

            SpriteBatch sb = Main.spriteBatch;
            Matrix matrix = Main.GameViewMatrix.TransformationMatrix;

            DrawGearSwingBody(sb, matrix);     // Pass 1: GearSwingTrail medium gear
            DrawBounceFlash(sb, matrix);       // Pass 2: PearlShimmer bounce-flash overlay
            DrawBloomTeeth(sb, matrix);        // Pass 3: Bloom + 12 teeth
            return false;
        }

        // ---- PASS 1: GearSwing GearSwingTrail medium gear body ----
        private void DrawGearSwingBody(SpriteBatch sb, Matrix matrix)
        {
            _gearSwingShader ??= ShaderLoader.GearSwing;
            if (_gearSwingShader == null) return;

            sb.End();

            _gearSwingShader.Parameters["uColor"]?.SetValue(ClairDeLunePalette.SoftBlue.ToVector4());
            _gearSwingShader.Parameters["uSecondaryColor"]?.SetValue(ClairDeLunePalette.ClockworkBrass.ToVector4());
            _gearSwingShader.Parameters["uOpacity"]?.SetValue(0.6f);
            _gearSwingShader.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly);
            _gearSwingShader.Parameters["uIntensity"]?.SetValue(1f);
            _gearSwingShader.Parameters["uOverbrightMult"]?.SetValue(1.0f);
            _gearSwingShader.Parameters["uScrollSpeed"]?.SetValue(2f);
            _gearSwingShader.Parameters["uDistortionAmt"]?.SetValue(0.015f);
            _gearSwingShader.Parameters["uHasSecondaryTex"]?.SetValue(false);
            _gearSwingShader.Parameters["uPhase"]?.SetValue(Projectile.rotation);

            _gearSwingShader.CurrentTechnique = _gearSwingShader.Techniques["GearSwingTrail"];

            sb.Begin(SpriteSortMode.Immediate, MagnumBlendStates.ShaderAdditive,
                SamplerState.LinearWrap, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, _gearSwingShader, matrix);

            Texture2D sc = _softCircle.Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float bodyScale = GearRadius * 2f / sc.Width;
            sb.Draw(sc, drawPos, null, Color.White, Projectile.rotation, sc.Size() * 0.5f, bodyScale, SpriteEffects.None, 0f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                SamplerState.LinearClamp, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null, matrix);
        }

        // ---- PASS 2: PearlShimmer bounce-flash overlay ----
        private void DrawBounceFlash(SpriteBatch sb, Matrix matrix)
        {
            if (_bounceCount < 1) return;

            _pearlGlowShader ??= ShaderLoader.ClairDeLunePearlGlow;
            if (_pearlGlowShader == null) return;

            float bounceIntensity = _bounceCount / (float)MaxBounces;

            sb.End();

            _pearlGlowShader.Parameters["uColor"]?.SetValue(ClairDeLunePalette.MoonbeamGold.ToVector4());
            _pearlGlowShader.Parameters["uSecondaryColor"]?.SetValue(ClairDeLunePalette.PearlFrost.ToVector4());
            _pearlGlowShader.Parameters["uOpacity"]?.SetValue(0.2f * bounceIntensity);
            _pearlGlowShader.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly);
            _pearlGlowShader.Parameters["uIntensity"]?.SetValue(0.8f + bounceIntensity);
            _pearlGlowShader.Parameters["uOverbrightMult"]?.SetValue(1.1f);
            _pearlGlowShader.Parameters["uScrollSpeed"]?.SetValue(2f);
            _pearlGlowShader.Parameters["uDistortionAmt"]?.SetValue(0.01f);
            _pearlGlowShader.Parameters["uHasSecondaryTex"]?.SetValue(false);

            _pearlGlowShader.CurrentTechnique = _pearlGlowShader.Techniques["PearlShimmer"];

            sb.Begin(SpriteSortMode.Immediate, MagnumBlendStates.ShaderAdditive,
                SamplerState.LinearWrap, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, _pearlGlowShader, matrix);

            Texture2D sc = _softCircle.Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float overlayScale = GearRadius * 2.5f / sc.Width;
            sb.Draw(sc, drawPos, null, Color.White, 0f, sc.Size() * 0.5f, overlayScale, SpriteEffects.None, 0f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                SamplerState.LinearClamp, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null, matrix);
        }

        // ---- PASS 3: Multi-scale bloom + 12 gear teeth ----
        private void DrawBloomTeeth(SpriteBatch sb, Matrix matrix)
        {
            Vector2 pos = Projectile.Center - Main.screenPosition;
            Texture2D srb = _softRadialBloom.Value;
            Texture2D pb = _pointBloom.Value;

            // Outer ambient
            sb.Draw(srb, pos, null,
                ClairDeLunePalette.NightMist with { A = 0 } * 0.12f, 0f, srb.Size() * 0.5f,
                GearRadius * 2f / srb.Width, SpriteEffects.None, 0f);

            // Blue glow
            sb.Draw(srb, pos, null,
                ClairDeLunePalette.SoftBlue with { A = 0 } * 0.2f, 0f, srb.Size() * 0.5f,
                GearRadius * 1.2f / srb.Width, SpriteEffects.None, 0f);

            // Core
            sb.Draw(pb, pos, null,
                ClairDeLunePalette.PearlFrost with { A = 0 } * 0.15f, 0f, pb.Size() * 0.5f,
                GearRadius * 0.5f / pb.Width, SpriteEffects.None, 0f);

            // 12 gear teeth
            for (int t = 0; t < ToothCount; t++)
            {
                float toothAngle = Projectile.rotation + t * MathHelper.TwoPi / ToothCount;
                Vector2 toothPos = pos + toothAngle.ToRotationVector2() * GearRadius;

                sb.Draw(pb, toothPos, null,
                    ClairDeLunePalette.SoftBlue with { A = 0 } * 0.12f, toothAngle, pb.Size() * 0.5f,
                    4f / pb.Width, SpriteEffects.None, 0f);
            }

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null, matrix);
        }
    }
}
