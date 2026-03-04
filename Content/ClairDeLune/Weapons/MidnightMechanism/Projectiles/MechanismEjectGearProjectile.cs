using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.Shaders;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.ClairDeLune.Weapons.MidnightMechanism.Projectiles
{
    /// <summary>
    /// Mechanism Eject Gear — shrapnel ejected on Gear Jam.
    /// Spinning brass gear fragments that deal 50% weapon damage and bounce once.
    /// 3 render passes: (1) GatlingBlur GatlingBarrelBlur for motion blur,
    /// (2) PearlShimmer bounce flash, (3) Bloom teeth + core stacking.
    /// </summary>
    public class MechanismEjectGearProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Textures/InvisibleProjectile";

        private int _bounces;
        private const int MaxBounces = 1;
        private float _bounceFlash; // 0→1 on bounce, decays

        // --- Texture + shader caching ---
        private static Effect _gatlingBlurShader;
        private static Effect _pearlGlowShader;
        private static Asset<Texture2D> _softCircle;
        private static Asset<Texture2D> _softRadialBloom;
        private static Asset<Texture2D> _pointBloom;

        public override void SetDefaults()
        {
            Projectile.width = 12;
            Projectile.height = 12;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 2;
            Projectile.timeLeft = 45;
            Projectile.tileCollide = true;
        }

        public override void AI()
        {
            Projectile.rotation += 0.35f;
            Projectile.velocity *= 0.97f;
            Projectile.velocity.Y += 0.15f;

            // Decay bounce flash
            if (_bounceFlash > 0f)
                _bounceFlash *= 0.88f;

            // Gear spark trail
            if (Main.rand.NextBool(2))
            {
                Color sparkCol = Main.rand.NextBool(4)
                    ? ClairDeLunePalette.NightMist
                    : ClairDeLunePalette.MoonbeamGold;
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.GoldFlame,
                    -Projectile.velocity * 0.1f + Main.rand.NextVector2Circular(1f, 1f),
                    0, sparkCol, 0.6f);
                d.noGravity = true;
            }

            Lighting.AddLight(Projectile.Center, ClairDeLunePalette.MoonbeamGold.ToVector3() * 0.2f);
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (_bounces < MaxBounces)
            {
                _bounces++;
                _bounceFlash = 1f;

                if (Projectile.velocity.X != oldVelocity.X)
                    Projectile.velocity.X = -oldVelocity.X * 0.6f;
                if (Projectile.velocity.Y != oldVelocity.Y)
                    Projectile.velocity.Y = -oldVelocity.Y * 0.6f;

                // Bounce spark particles
                for (int i = 0; i < 3; i++)
                {
                    var spark = new GenericGlowParticle(Projectile.Center,
                        Main.rand.NextVector2Circular(3f, 3f),
                        ClairDeLunePalette.MoonbeamGold with { A = 0 } * 0.4f, 0.08f, 6, true);
                    MagnumParticleHandler.SpawnParticle(spark);
                }

                return false;
            }
            return true;
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

            DrawGatlingBlur(sb, matrix);      // Pass 1: GatlingBarrelBlur motion blur
            DrawBounceFlash(sb, matrix);      // Pass 2: PearlShimmer bounce flash (if active)
            DrawBloomAndTeeth(sb, matrix);    // Pass 3: Bloom teeth + core
            return false;
        }

        // ---- PASS 1: GatlingBlur GatlingBarrelBlur motion blur ----
        private void DrawGatlingBlur(SpriteBatch sb, Matrix matrix)
        {
            _gatlingBlurShader ??= ShaderLoader.GatlingBlur;
            if (_gatlingBlurShader == null) return;

            sb.End();

            _gatlingBlurShader.Parameters["uColor"]?.SetValue(ClairDeLunePalette.ClockworkBrass.ToVector4());
            _gatlingBlurShader.Parameters["uSecondaryColor"]?.SetValue(ClairDeLunePalette.MoonbeamGold.ToVector4());
            _gatlingBlurShader.Parameters["uOpacity"]?.SetValue(0.35f);
            _gatlingBlurShader.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly);
            _gatlingBlurShader.Parameters["uIntensity"]?.SetValue(1f);
            _gatlingBlurShader.Parameters["uOverbrightMult"]?.SetValue(1.0f);
            _gatlingBlurShader.Parameters["uScrollSpeed"]?.SetValue(5f);
            _gatlingBlurShader.Parameters["uDistortionAmt"]?.SetValue(0.02f);
            _gatlingBlurShader.Parameters["uHasSecondaryTex"]?.SetValue(false);

            _gatlingBlurShader.CurrentTechnique = _gatlingBlurShader.Techniques["GatlingBarrelBlur"];

            sb.Begin(SpriteSortMode.Immediate, BlendState.Additive,
                SamplerState.LinearWrap, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, _gatlingBlurShader, matrix);

            Texture2D sc = _softCircle.Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float blurScale = 16f / sc.Width;
            sb.Draw(sc, drawPos, null, Color.White, Projectile.rotation,
                sc.Size() * 0.5f, blurScale, SpriteEffects.None, 0f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive,
                SamplerState.LinearClamp, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null, matrix);
        }

        // ---- PASS 2: PearlShimmer bounce flash (active after bounce) ----
        private void DrawBounceFlash(SpriteBatch sb, Matrix matrix)
        {
            if (_bounceFlash < 0.05f) return;

            _pearlGlowShader ??= ShaderLoader.ClairDeLunePearlGlow;
            if (_pearlGlowShader == null) return;

            sb.End();

            _pearlGlowShader.Parameters["uColor"]?.SetValue(ClairDeLunePalette.PearlFrost.ToVector4());
            _pearlGlowShader.Parameters["uSecondaryColor"]?.SetValue(ClairDeLunePalette.MoonbeamGold.ToVector4());
            _pearlGlowShader.Parameters["uOpacity"]?.SetValue(0.5f * _bounceFlash);
            _pearlGlowShader.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly);
            _pearlGlowShader.Parameters["uIntensity"]?.SetValue(1.2f);
            _pearlGlowShader.Parameters["uOverbrightMult"]?.SetValue(1.2f);
            _pearlGlowShader.Parameters["uScrollSpeed"]?.SetValue(3f);
            _pearlGlowShader.Parameters["uDistortionAmt"]?.SetValue(0.01f);
            _pearlGlowShader.Parameters["uHasSecondaryTex"]?.SetValue(false);

            _pearlGlowShader.CurrentTechnique = _pearlGlowShader.Techniques["PearlShimmer"];

            sb.Begin(SpriteSortMode.Immediate, BlendState.Additive,
                SamplerState.LinearWrap, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, _pearlGlowShader, matrix);

            Texture2D sc = _softCircle.Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float flashScale = MathHelper.Lerp(16f, 28f, _bounceFlash) / sc.Width;
            sb.Draw(sc, drawPos, null, Color.White, 0f,
                sc.Size() * 0.5f, flashScale, SpriteEffects.None, 0f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive,
                SamplerState.LinearClamp, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null, matrix);
        }

        // ---- PASS 3: Bloom teeth + core stacking ----
        private void DrawBloomAndTeeth(SpriteBatch sb, Matrix matrix)
        {
            Vector2 pos = Projectile.Center - Main.screenPosition;
            float life = (float)Projectile.timeLeft / 45f;
            Texture2D srb = _softRadialBloom.Value;
            Texture2D pb = _pointBloom.Value;

            // Gear teeth — 6 small bloom dots
            int teeth = 6;
            float gearRadius = 8f;
            for (int t = 0; t < teeth; t++)
            {
                float angle = Projectile.rotation + MathHelper.TwoPi * t / teeth;
                Vector2 toothPos = pos + angle.ToRotationVector2() * gearRadius;
                sb.Draw(pb, toothPos, null,
                    ClairDeLunePalette.ClockworkBrass with { A = 0 } * 0.3f * life, 0f,
                    pb.Size() * 0.5f, 4f / pb.Width, SpriteEffects.None, 0f);
            }

            // Outer soft haze
            sb.Draw(srb, pos, null,
                ClairDeLunePalette.NightMist with { A = 0 } * 0.12f * life, 0f,
                srb.Size() * 0.5f, 24f / srb.Width, SpriteEffects.None, 0f);

            // Core
            sb.Draw(pb, pos, null,
                ClairDeLunePalette.MoonbeamGold with { A = 0 } * 0.4f * life, 0f,
                pb.Size() * 0.5f, 8f / pb.Width, SpriteEffects.None, 0f);

            // Restore AlphaBlend
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null, matrix);
        }
    }
}
