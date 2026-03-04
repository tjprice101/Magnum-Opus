using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.Shaders;
using MagnumOpus.Content.ClairDeLune.Weapons.MidnightMechanism.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.ClairDeLune.Weapons.MidnightMechanism.Projectiles
{
    /// <summary>
    /// Mechanism Bullet — rapid-fire projectile with phase-scaled VFX.
    /// 3 render passes: (1) SparkleTrailShader VertexStrip trail (Phase 3+),
    /// (2) GatlingBlur.fx muzzle overlay on bullet body, (3) Multi-scale bloom core.
    /// ai[0] = phase (1-5). Visual complexity scales with phase.
    /// </summary>
    public class MechanismBulletProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Textures/InvisibleProjectile";

        private const int TrailLen = 16;
        private Vector2[] _trailPositions = new Vector2[TrailLen];
        private float[] _trailRotations = new float[TrailLen];
        private int _trailIndex;
        private int Phase => (int)Projectile.ai[0];

        // --- Shader + texture caching ---
        private static Effect _sparkleTrailShader;
        private static Effect _gatlingBlurShader;
        private static Asset<Texture2D> _sparkleTex;
        private static Asset<Texture2D> _gradientLUT;
        private static Asset<Texture2D> _glowMask;
        private static Asset<Texture2D> _softCircle;
        private static Asset<Texture2D> _softRadialBloom;
        private static Asset<Texture2D> _pointBloom;
        private VertexStrip _strip;

        public override void SetDefaults()
        {
            Projectile.width = 8;
            Projectile.height = 8;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 90;
            Projectile.tileCollide = true;
            Projectile.extraUpdates = 2;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();

            // Trail recording (ring buffer)
            _trailPositions[_trailIndex % TrailLen] = Projectile.Center;
            _trailRotations[_trailIndex % TrailLen] = Projectile.rotation;
            _trailIndex++;

            if (Phase >= 3)
                Projectile.width = Projectile.height = 10;
            if (Phase >= 5)
                Projectile.width = Projectile.height = 12;

            // Clockwork tracer dust
            if (Main.rand.NextBool(Math.Max(5 - Phase, 1)))
            {
                Color dustCol = Phase >= 4 ? ClairDeLunePalette.PearlFrost : ClairDeLunePalette.SoftBlue;
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.BlueFairy,
                    -Projectile.velocity * 0.05f, 0, dustCol, 0.4f + Phase * 0.1f);
                d.noGravity = true;
            }

            Lighting.AddLight(Projectile.Center, ClairDeLunePalette.SoftBlue.ToVector3() * (0.2f + Phase * 0.08f));
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Player player = Main.player[Projectile.owner];
            var mp = player.GetModPlayer<MidnightMechanismPlayer>();
            mp.RegisterHit();

            float flashScale = 0.12f + Phase * 0.04f;
            var flash = new BloomParticle(target.Center, Vector2.Zero,
                ClairDeLunePalette.PearlWhite with { A = 0 } * 0.4f, flashScale, 5);
            MagnumParticleHandler.SpawnParticle(flash);

            if (Phase >= 4)
            {
                for (int i = 0; i < Phase - 2; i++)
                {
                    Vector2 vel = Main.rand.NextVector2Circular(3f, 3f);
                    var spark = new GenericGlowParticle(target.Center, vel,
                        ClairDeLunePalette.MoonbeamGold with { A = 0 } * 0.4f, 0.06f, 6, true);
                    MagnumParticleHandler.SpawnParticle(spark);
                }
            }
        }

        private void LoadTextures()
        {
            _sparkleTex ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/TrailsAndRibbons/SpiralTrail", AssetRequestMode.ImmediateLoad);
            _gradientLUT ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/ColorGradients/ClairDeLuneGradientLUTandRAMP", AssetRequestMode.ImmediateLoad);
            _glowMask ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftGlow", AssetRequestMode.ImmediateLoad);
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

            if (Phase >= 3) DrawSparkleTrail(sb, matrix);  // Pass 1: VertexStrip trail (Phase 3+)
            DrawGatlingBlurOverlay(sb, matrix);              // Pass 2: GatlingBlur shader on bullet
            DrawBloomCore(sb, matrix);                       // Pass 3: Multi-scale bloom body
            return false;
        }

        // ---- PASS 1: SparkleTrailShader VertexStrip trail (Phase 3+) ----
        private void DrawSparkleTrail(SpriteBatch sb, Matrix matrix)
        {
            int count = Math.Min(_trailIndex, TrailLen);
            if (count < 3) return;

            // Ordered arrays from ring buffer
            Vector2[] positions = new Vector2[count];
            float[] rotations = new float[count];
            for (int i = 0; i < count; i++)
            {
                int idx = ((_trailIndex - 1 - i) % TrailLen + TrailLen) % TrailLen;
                positions[i] = _trailPositions[idx];
                rotations[i] = _trailRotations[idx];
            }

            float trailWidth = 3f + (Phase - 3) * 2f;
            _strip ??= new VertexStrip();
            _strip.PrepareStrip(positions, rotations,
                (float progress) =>
                {
                    Color c = Color.Lerp(ClairDeLunePalette.MoonbeamGold, ClairDeLunePalette.NightMist, progress);
                    return c with { A = 0 } * (1f - progress * 0.85f);
                },
                (float progress) => MathHelper.Lerp(trailWidth, 1f, progress),
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
                _sparkleTrailShader.Parameters["trailIntensity"]?.SetValue(0.6f + Phase * 0.15f);
                _sparkleTrailShader.Parameters["sparkleSpeed"]?.SetValue(4f);
                _sparkleTrailShader.Parameters["sparkleScale"]?.SetValue(6f);
                _sparkleTrailShader.Parameters["glitterDensity"]?.SetValue(10f);
                _sparkleTrailShader.Parameters["tipFadeStart"]?.SetValue(0.6f);
                _sparkleTrailShader.Parameters["edgeSoftness"]?.SetValue(0.2f);
                _sparkleTrailShader.Parameters["coreColor"]?.SetValue(ClairDeLunePalette.MoonbeamGold.ToVector3());
                _sparkleTrailShader.Parameters["outerColor"]?.SetValue(ClairDeLunePalette.SoftBlue.ToVector3());
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

        // ---- PASS 2: GatlingBlur.fx shader overlay on bullet body ----
        private void DrawGatlingBlurOverlay(SpriteBatch sb, Matrix matrix)
        {
            if (Phase < 2) return; // Only Phase 2+ gets shader overlay

            _gatlingBlurShader ??= ShaderLoader.GatlingBlur;
            if (_gatlingBlurShader == null) return;

            sb.End();

            _gatlingBlurShader.Parameters["uColor"]?.SetValue(ClairDeLunePalette.MoonbeamGold.ToVector4());
            _gatlingBlurShader.Parameters["uSecondaryColor"]?.SetValue(ClairDeLunePalette.PearlFrost.ToVector4());
            _gatlingBlurShader.Parameters["uOpacity"]?.SetValue(0.3f + Phase * 0.1f);
            _gatlingBlurShader.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly);
            _gatlingBlurShader.Parameters["uIntensity"]?.SetValue(1f + Phase * 0.2f);
            _gatlingBlurShader.Parameters["uOverbrightMult"]?.SetValue(1.1f);
            _gatlingBlurShader.Parameters["uScrollSpeed"]?.SetValue(6f);
            _gatlingBlurShader.Parameters["uDistortionAmt"]?.SetValue(0.01f * Phase);
            _gatlingBlurShader.Parameters["uHasSecondaryTex"]?.SetValue(false);

            _gatlingBlurShader.CurrentTechnique = _gatlingBlurShader.Techniques["GatlingMuzzle"];

            sb.Begin(SpriteSortMode.Immediate, BlendState.Additive,
                SamplerState.LinearWrap, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, _gatlingBlurShader, matrix);

            Texture2D sc = _softCircle.Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float bulletScale = (8f + Phase * 3f) / sc.Width;
            sb.Draw(sc, drawPos, null, Color.White, Projectile.rotation, sc.Size() * 0.5f,
                new Vector2(bulletScale * 2f, bulletScale), SpriteEffects.None, 0f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive,
                SamplerState.LinearClamp, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null, matrix);
        }

        // ---- PASS 3: Multi-scale bloom body ----
        private void DrawBloomCore(SpriteBatch sb, Matrix matrix)
        {
            Vector2 pos = Projectile.Center - Main.screenPosition;
            Texture2D srb = _softRadialBloom.Value;
            Texture2D pb = _pointBloom.Value;

            // Phase-scaled layers
            float phaseScale = 1f + Phase * 0.15f;

            // Always: Gold core bullet
            sb.Draw(pb, pos, null, ClairDeLunePalette.MoonbeamGold with { A = 0 } * 0.5f,
                Projectile.rotation, pb.Size() * 0.5f,
                new Vector2(12f * phaseScale / pb.Width, 6f * phaseScale / pb.Height), SpriteEffects.None, 0f);

            // Phase 2+: Pearl frost glow
            if (Phase >= 2)
                sb.Draw(srb, pos, null, ClairDeLunePalette.PearlFrost with { A = 0 } * 0.3f,
                    0f, srb.Size() * 0.5f, 10f * phaseScale / srb.Width, SpriteEffects.None, 0f);

            // Phase 4+: Soft blue ambient
            if (Phase >= 4)
                sb.Draw(srb, pos, null, ClairDeLunePalette.SoftBlue with { A = 0 } * 0.2f,
                    0f, srb.Size() * 0.5f, 16f * phaseScale / srb.Width, SpriteEffects.None, 0f);

            // Phase 5: Pearl white hot core
            if (Phase >= 5)
                sb.Draw(pb, pos, null, ClairDeLunePalette.PearlWhite with { A = 0 } * 0.4f,
                    0f, pb.Size() * 0.5f, 5f / pb.Width, SpriteEffects.None, 0f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null, matrix);
        }
    }
}
