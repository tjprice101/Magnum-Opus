using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.Shaders;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.ClairDeLune.Weapons.MidnightMechanism.Projectiles
{
    /// <summary>
    /// Midnight Strike Shot — 10x damage massive projectile fired when all 12 tick marks are consumed.
    /// 4 render passes: (1) SparkleTrailShader VertexStrip comet trail,
    /// (2) GatlingBlur.fx GatlingMuzzle overlay on stretched body,
    /// (3) ClairDeLunePearlGlow.fx PearlBloom halo, (4) Multi-scale bloom core.
    /// </summary>
    public class MidnightStrikeShotProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Textures/InvisibleProjectile";

        private const int TrailLen = 24;
        private Vector2[] _trailPositions = new Vector2[TrailLen];
        private float[] _trailRotations = new float[TrailLen];
        private int _trailIndex;

        // --- Shader + texture caching ---
        private static Effect _sparkleTrailShader;
        private static Effect _gatlingBlurShader;
        private static Effect _pearlGlowShader;
        private static Asset<Texture2D> _sparkleTex;
        private static Asset<Texture2D> _gradientLUT;
        private static Asset<Texture2D> _glowMask;
        private static Asset<Texture2D> _softCircle;
        private static Asset<Texture2D> _softRadialBloom;
        private static Asset<Texture2D> _pointBloom;
        private static Asset<Texture2D> _starFlare;
        private VertexStrip _strip;

        public override void SetDefaults()
        {
            Projectile.width = 24;
            Projectile.height = 24;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 3;
            Projectile.timeLeft = 120;
            Projectile.tileCollide = true;
            Projectile.extraUpdates = 1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();

            // Ring-buffer trail recording
            _trailPositions[_trailIndex % TrailLen] = Projectile.Center;
            _trailRotations[_trailIndex % TrailLen] = Projectile.rotation;
            _trailIndex++;

            // Intense trailing dust
            for (int i = 0; i < 2; i++)
            {
                Vector2 dustPos = Projectile.Center + Main.rand.NextVector2Circular(6f, 6f);
                Color dustCol = Color.Lerp(ClairDeLunePalette.MoonbeamGold, ClairDeLunePalette.PearlWhite, Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(dustPos, DustID.GoldFlame,
                    -Projectile.velocity * 0.15f + Main.rand.NextVector2Circular(1f, 1f),
                    0, dustCol, 1.2f);
                d.noGravity = true;
            }

            float pulse = 0.8f + 0.2f * MathF.Sin((float)Projectile.ai[1]++ * 0.15f);
            Lighting.AddLight(Projectile.Center, ClairDeLunePalette.MoonbeamGold.ToVector3() * 0.8f * pulse);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            SoundEngine.PlaySound(SoundID.Item14 with { Pitch = -0.8f, Volume = 1.0f }, target.Center);

            for (int ring = 0; ring < 12; ring++)
            {
                float ringProgress = ring / 11f;
                Color ringCol = Color.Lerp(ClairDeLunePalette.MoonbeamGold, ClairDeLunePalette.PearlWhite, ringProgress);
                int points = 20 + ring * 2;
                float radius = 8f + ring * 6f;

                for (int p = 0; p < points; p++)
                {
                    float angle = MathHelper.TwoPi * p / points + ring * 0.15f;
                    Vector2 vel = angle.ToRotationVector2() * (radius / 8f);
                    var dot = new GenericGlowParticle(target.Center, vel,
                        ringCol with { A = 0 } * (0.5f - ring * 0.03f), 0.06f, 12 + ring, true);
                    MagnumParticleHandler.SpawnParticle(dot);
                }
            }

            var flash = new BloomParticle(target.Center, Vector2.Zero,
                ClairDeLunePalette.PearlWhite with { A = 0 } * 0.8f, 0.8f, 12);
            MagnumParticleHandler.SpawnParticle(flash);

            for (int t = 0; t < 12; t++)
            {
                float angle = MathHelper.TwoPi * t / 12f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(6f, 12f);
                var tickShard = new GenericGlowParticle(target.Center, vel,
                    ClairDeLunePalette.MoonbeamGold with { A = 0 } * 0.6f, 0.1f, 20, true);
                MagnumParticleHandler.SpawnParticle(tickShard);
            }
        }

        public override void OnKill(int timeLeft)
        {
            var residual = new BloomParticle(Projectile.Center, Vector2.Zero,
                ClairDeLunePalette.MoonbeamGold with { A = 0 } * 0.5f, 0.5f, 10);
            MagnumParticleHandler.SpawnParticle(residual);
        }

        private void LoadTextures()
        {
            _sparkleTex ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/TrailsAndRibbons/CometTrail", AssetRequestMode.ImmediateLoad);
            _gradientLUT ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/ColorGradients/ClairDeLuneGradientLUTandRAMP", AssetRequestMode.ImmediateLoad);
            _glowMask ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftGlow", AssetRequestMode.ImmediateLoad);
            _softCircle ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/MasksAndShapes/SoftCircle", AssetRequestMode.ImmediateLoad);
            _softRadialBloom ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftRadialBloom", AssetRequestMode.ImmediateLoad);
            _pointBloom ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/PointBloom", AssetRequestMode.ImmediateLoad);
            _starFlare ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/StarFlare", AssetRequestMode.ImmediateLoad);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (Main.dedServ) return false;
            LoadTextures();

            SpriteBatch sb = Main.spriteBatch;
            Matrix matrix = Main.GameViewMatrix.TransformationMatrix;

            DrawSparkleTrail(sb, matrix);       // Pass 1: VertexStrip comet trail
            DrawGatlingBlurBody(sb, matrix);     // Pass 2: GatlingBlur shader on stretched body
            DrawPearlGlowHalo(sb, matrix);       // Pass 3: PearlBloom shader halo
            DrawBloomCore(sb, matrix);           // Pass 4: Multi-scale bloom + star flare
            ClairDeLuneVFXLibrary.DrawThemeAccents(sb, Projectile.Center, 0.5f, 0.3f);
            return false;
        }

        // ---- PASS 1: SparkleTrailShader VertexStrip ----
        private void DrawSparkleTrail(SpriteBatch sb, Matrix matrix)
        {
            int count = Math.Min(_trailIndex, TrailLen);
            if (count < 3) return;

            Vector2[] positions = new Vector2[count];
            float[] rotations = new float[count];
            for (int i = 0; i < count; i++)
            {
                int idx = ((_trailIndex - 1 - i) % TrailLen + TrailLen) % TrailLen;
                positions[i] = _trailPositions[idx];
                rotations[i] = _trailRotations[idx];
            }

            _strip ??= new VertexStrip();
            _strip.PrepareStrip(positions, rotations,
                (float p) =>
                {
                    Color c = Color.Lerp(ClairDeLunePalette.PearlWhite, ClairDeLunePalette.MoonbeamGold, p);
                    return c with { A = 0 } * (1f - p * 0.8f);
                },
                (float p) => MathHelper.Lerp(14f, 2f, p),
                -Main.screenPosition, count, includeBacksides: true);

            _sparkleTrailShader ??= ModContent.Request<Effect>(
                "MagnumOpus/Content/FoundationWeapons/SparkleProjectileFoundation/Shaders/SparkleTrailShader",
                AssetRequestMode.ImmediateLoad).Value;

            sb.End();

            if (_sparkleTrailShader != null)
            {
                _sparkleTrailShader.Parameters["WorldViewProjection"]?.SetValue(
                    Main.GameViewMatrix.NormalizedTransformationmatrix);
                _sparkleTrailShader.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly);
                _sparkleTrailShader.Parameters["trailIntensity"]?.SetValue(1.4f);
                _sparkleTrailShader.Parameters["sparkleSpeed"]?.SetValue(5f);
                _sparkleTrailShader.Parameters["sparkleScale"]?.SetValue(4f);
                _sparkleTrailShader.Parameters["glitterDensity"]?.SetValue(14f);
                _sparkleTrailShader.Parameters["tipFadeStart"]?.SetValue(0.5f);
                _sparkleTrailShader.Parameters["edgeSoftness"]?.SetValue(0.15f);
                _sparkleTrailShader.Parameters["coreColor"]?.SetValue(ClairDeLunePalette.PearlWhite.ToVector3());
                _sparkleTrailShader.Parameters["outerColor"]?.SetValue(ClairDeLunePalette.MoonbeamGold.ToVector3());
                _sparkleTrailShader.Parameters["sparkleTex"]?.SetValue(_sparkleTex.Value);
                _sparkleTrailShader.Parameters["gradientTex"]?.SetValue(_gradientLUT.Value);
                _sparkleTrailShader.Parameters["glowMaskTex"]?.SetValue(_glowMask.Value);

                _sparkleTrailShader.CurrentTechnique.Passes["SparkleTrailPass"].Apply();
                _strip.DrawTrail();
                Main.pixelShader.CurrentTechnique.Passes[0].Apply();
            }

            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive,
                SamplerState.LinearClamp, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null, matrix);
        }

        // ---- PASS 2: GatlingBlur.fx body shimmer ----
        private void DrawGatlingBlurBody(SpriteBatch sb, Matrix matrix)
        {
            _gatlingBlurShader ??= ShaderLoader.GatlingBlur;
            if (_gatlingBlurShader == null) return;

            sb.End();

            _gatlingBlurShader.Parameters["uColor"]?.SetValue(ClairDeLunePalette.MoonbeamGold.ToVector4());
            _gatlingBlurShader.Parameters["uSecondaryColor"]?.SetValue(ClairDeLunePalette.PearlWhite.ToVector4());
            _gatlingBlurShader.Parameters["uOpacity"]?.SetValue(0.7f);
            _gatlingBlurShader.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly);
            _gatlingBlurShader.Parameters["uIntensity"]?.SetValue(2f);
            _gatlingBlurShader.Parameters["uOverbrightMult"]?.SetValue(1.3f);
            _gatlingBlurShader.Parameters["uScrollSpeed"]?.SetValue(8f);
            _gatlingBlurShader.Parameters["uDistortionAmt"]?.SetValue(0.03f);
            _gatlingBlurShader.Parameters["uHasSecondaryTex"]?.SetValue(false);

            _gatlingBlurShader.CurrentTechnique = _gatlingBlurShader.Techniques["GatlingMuzzle"];

            sb.Begin(SpriteSortMode.Immediate, BlendState.Additive,
                SamplerState.LinearWrap, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, _gatlingBlurShader, matrix);

            Texture2D sc = _softCircle.Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float bodyScale = 28f / sc.Width;
            sb.Draw(sc, drawPos, null, Color.White, Projectile.rotation, sc.Size() * 0.5f,
                new Vector2(bodyScale * 2.5f, bodyScale), SpriteEffects.None, 0f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive,
                SamplerState.LinearClamp, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null, matrix);
        }

        // ---- PASS 3: PearlBloom halo ----
        private void DrawPearlGlowHalo(SpriteBatch sb, Matrix matrix)
        {
            _pearlGlowShader ??= ShaderLoader.ClairDeLunePearlGlow;
            if (_pearlGlowShader == null) return;

            sb.End();

            _pearlGlowShader.Parameters["uColor"]?.SetValue(ClairDeLunePalette.PearlWhite.ToVector4());
            _pearlGlowShader.Parameters["uSecondaryColor"]?.SetValue(ClairDeLunePalette.MoonbeamGold.ToVector4());
            _pearlGlowShader.Parameters["uOpacity"]?.SetValue(0.5f);
            _pearlGlowShader.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly);
            _pearlGlowShader.Parameters["uIntensity"]?.SetValue(1.5f);
            _pearlGlowShader.Parameters["uOverbrightMult"]?.SetValue(1.2f);
            _pearlGlowShader.Parameters["uScrollSpeed"]?.SetValue(2f);
            _pearlGlowShader.Parameters["uDistortionAmt"]?.SetValue(0.01f);
            _pearlGlowShader.Parameters["uHasSecondaryTex"]?.SetValue(false);

            _pearlGlowShader.CurrentTechnique = _pearlGlowShader.Techniques["PearlBloom"];

            sb.Begin(SpriteSortMode.Immediate, BlendState.Additive,
                SamplerState.LinearWrap, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, _pearlGlowShader, matrix);

            Texture2D sc = _softCircle.Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float haloScale = 48f / sc.Width;
            sb.Draw(sc, drawPos, null, Color.White, 0f, sc.Size() * 0.5f, haloScale, SpriteEffects.None, 0f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive,
                SamplerState.LinearClamp, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null, matrix);
        }

        // ---- PASS 4: Multi-scale bloom core + star flare ----
        private void DrawBloomCore(SpriteBatch sb, Matrix matrix)
        {
            Vector2 pos = Projectile.Center - Main.screenPosition;
            float pulse = 0.85f + 0.15f * MathF.Sin((float)Projectile.ai[1] * 0.15f);

            Texture2D srb = _softRadialBloom.Value;
            Texture2D pb = _pointBloom.Value;
            Texture2D sf = _starFlare.Value;

            // Wide gold haze
            sb.Draw(srb, pos, null, ClairDeLunePalette.MoonbeamGold with { A = 0 } * 0.3f,
                0f, srb.Size() * 0.5f, 48f * pulse / srb.Width, SpriteEffects.None, 0f);

            // Pearl frost mid glow
            sb.Draw(srb, pos, null, ClairDeLunePalette.PearlFrost with { A = 0 } * 0.4f,
                0f, srb.Size() * 0.5f, 28f * pulse / srb.Width, SpriteEffects.None, 0f);

            // Elongated gold body
            sb.Draw(pb, pos, null, ClairDeLunePalette.MoonbeamGold with { A = 0 } * 0.5f,
                Projectile.rotation, pb.Size() * 0.5f,
                new Vector2(24f * pulse / pb.Width, 12f * pulse / pb.Height), SpriteEffects.None, 0f);

            // White-hot core
            sb.Draw(pb, pos, null, ClairDeLunePalette.PearlWhite with { A = 0 } * 0.7f,
                Projectile.rotation, pb.Size() * 0.5f,
                new Vector2(12f / pb.Width, 6f / pb.Height), SpriteEffects.None, 0f);

            // Star flare accent (rotating)
            float flareRot = Main.GlobalTimeWrappedHourly * 2f;
            sb.Draw(sf, pos, null, ClairDeLunePalette.PearlWhite with { A = 0 } * 0.3f,
                flareRot, sf.Size() * 0.5f, 20f / sf.Width, SpriteEffects.None, 0f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null, matrix);
        }
    }
}
