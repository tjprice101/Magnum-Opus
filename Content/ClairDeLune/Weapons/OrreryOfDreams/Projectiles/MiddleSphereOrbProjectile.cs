using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.Shaders;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.Graphics;
using Terraria.ModLoader;

namespace MagnumOpus.Content.ClairDeLune.Weapons.OrreryOfDreams.Projectiles
{
    /// <summary>
    /// Middle Sphere Orb — Gently homing dream orb from OrreryOfDreams.
    /// Floats toward enemies with a dreamy, drifting quality.
    /// 3 render passes: (1) CelestialOrbit CelestialOrbitCore dreamy body,
    /// (2) SparkleTrailShader VertexStrip dream trail, (3) Multi-scale bloom halo.
    /// </summary>
    public class MiddleSphereOrbProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Textures/InvisibleProjectile";

        private const float HomingRange = 300f;
        private const float HomingStrength = 0.03f;
        private const float MaxSpeed = 7f;

        // Trail ring buffer
        private const int TrailLength = 16;
        private Vector2[] _trailPositions = new Vector2[TrailLength];
        private float[] _trailRotations = new float[TrailLength];
        private int _trailIndex;
        private VertexStrip _strip;

        // --- Shader + texture caching ---
        private static Effect _celestialShader;
        private static Effect _sparkleTrailShader;
        private static Asset<Texture2D> _softCircle;
        private static Asset<Texture2D> _softRadialBloom;
        private static Asset<Texture2D> _pointBloom;
        private static Asset<Texture2D> _sparkleHard;
        private static Asset<Texture2D> _glowMask;
        private static Asset<Texture2D> _gradientLUT;

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 180;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();

            // Record trail
            int idx = _trailIndex % TrailLength;
            _trailPositions[idx] = Projectile.Center;
            _trailRotations[idx] = Projectile.rotation;
            _trailIndex++;

            // Dreamy sine-wave drift
            float drift = MathF.Sin(Main.GameUpdateCount * 0.08f + Projectile.whoAmI) * 0.3f;
            Projectile.velocity = Projectile.velocity.RotatedBy(drift * 0.02f);

            // Gentle homing
            NPC closest = null;
            float closestDist = HomingRange;
            for (int n = 0; n < Main.maxNPCs; n++)
            {
                NPC npc = Main.npc[n];
                if (!npc.active || npc.friendly || npc.dontTakeDamage) continue;
                float dist = Vector2.Distance(Projectile.Center, npc.Center);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closest = npc;
                }
            }

            if (closest != null)
            {
                Vector2 toTarget = (closest.Center - Projectile.Center).SafeNormalize(Vector2.UnitX);
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, toTarget * MaxSpeed, HomingStrength);
            }

            // Dream dust
            if (Main.GameUpdateCount % 3 == 0)
            {
                var dream = new GenericGlowParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(4f, 4f),
                    -Projectile.velocity * 0.03f + Main.rand.NextVector2Circular(0.3f, 0.3f),
                    ClairDeLunePalette.NightMist with { A = 0 } * 0.2f, 0.04f, 8, true);
                MagnumParticleHandler.SpawnParticle(dream);
            }

            Lighting.AddLight(Projectile.Center, ClairDeLunePalette.SoftBlue.ToVector3() * 0.2f);
        }

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 vel = angle.ToRotationVector2() * 2f;
                var burst = new SparkleParticle(Projectile.Center, vel,
                    ClairDeLunePalette.PearlFrost with { A = 0 } * 0.4f, 0.06f, 8);
                MagnumParticleHandler.SpawnParticle(burst);
            }
        }

        private void LoadTextures()
        {
            _softCircle ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/MasksAndShapes/SoftCircle", AssetRequestMode.ImmediateLoad);
            _softRadialBloom ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftRadialBloom", AssetRequestMode.ImmediateLoad);
            _pointBloom ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/PointBloom", AssetRequestMode.ImmediateLoad);
            _sparkleHard ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles Asset Library/Stars/4PointedStarHard", AssetRequestMode.ImmediateLoad);
            _glowMask ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/MasksAndShapes/SoftCircle", AssetRequestMode.ImmediateLoad);
            _gradientLUT ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/ColorGradients/ClairDeLuneGradientLUTandRAMP", AssetRequestMode.ImmediateLoad);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (Main.dedServ) return false;
            LoadTextures();

            SpriteBatch sb = Main.spriteBatch;
            Matrix matrix = Main.GameViewMatrix.TransformationMatrix;

            DrawSparkleTrail(sb, matrix);      // Pass 1: SparkleTrailShader VertexStrip dream trail
            DrawCelestialBody(sb, matrix);     // Pass 2: CelestialOrbitCore dreamy body
            DrawBloomHalo(sb, matrix);         // Pass 3: Multi-scale bloom halo
            return false;
        }

        // ---- PASS 1: SparkleTrailShader VertexStrip dream trail ----
        private void DrawSparkleTrail(SpriteBatch sb, Matrix matrix)
        {
            int count = Math.Min(_trailIndex, TrailLength);
            if (count < 3) return;

            Vector2[] positions = new Vector2[count];
            float[] rotations = new float[count];
            for (int i = 0; i < count; i++)
            {
                int ringIdx = ((_trailIndex - 1 - i) % TrailLength + TrailLength) % TrailLength;
                positions[i] = _trailPositions[ringIdx];
                rotations[i] = _trailRotations[ringIdx];
            }

            _strip ??= new VertexStrip();
            _strip.PrepareStrip(positions, rotations,
                (float progress) => Color.Lerp(
                    ClairDeLunePalette.PearlFrost with { A = 0 },
                    ClairDeLunePalette.NightMist with { A = 0 }, progress) * (1f - progress * 0.85f),
                (float progress) => MathHelper.Lerp(10f, 1f, progress),
                -Main.screenPosition, count, includeBacksides: true);

            _sparkleTrailShader ??= ModContent.Request<Effect>(
                "MagnumOpus/Content/FoundationWeapons/SparkleProjectileFoundation/Shaders/SparkleTrailShader",
                AssetRequestMode.ImmediateLoad).Value;

            sb.End();

            _sparkleTrailShader.Parameters["WorldViewProjection"]?.SetValue(
                Main.GameViewMatrix.NormalizedTransformationmatrix);
            _sparkleTrailShader.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly * 1.5f);
            _sparkleTrailShader.Parameters["sparkleTex"]?.SetValue(_sparkleHard.Value);
            _sparkleTrailShader.Parameters["gradientTex"]?.SetValue(_gradientLUT.Value);
            _sparkleTrailShader.Parameters["glowMaskTex"]?.SetValue(_glowMask.Value);
            _sparkleTrailShader.Parameters["coreColor"]?.SetValue(ClairDeLunePalette.PearlFrost.ToVector3());
            _sparkleTrailShader.Parameters["outerColor"]?.SetValue(ClairDeLunePalette.NightMist.ToVector3());
            _sparkleTrailShader.Parameters["sparkleSpeed"]?.SetValue(2f);
            _sparkleTrailShader.Parameters["sparkleScale"]?.SetValue(0.4f);
            _sparkleTrailShader.Parameters["glitterDensity"]?.SetValue(4.0f);
            _sparkleTrailShader.Parameters["tipFadeStart"]?.SetValue(0.5f);
            _sparkleTrailShader.Parameters["edgeSoftness"]?.SetValue(0.35f);

            _sparkleTrailShader.CurrentTechnique.Passes["SparkleTrailPass"].Apply();
            _strip.DrawTrail();
            Main.pixelShader.CurrentTechnique.Passes[0].Apply();

            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                SamplerState.LinearClamp, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null, matrix);
        }

        // ---- PASS 2: CelestialOrbit CelestialOrbitCore dreamy body ----
        private void DrawCelestialBody(SpriteBatch sb, Matrix matrix)
        {
            _celestialShader ??= ShaderLoader.CelestialOrbit;
            if (_celestialShader == null) return;

            sb.End();

            _celestialShader.Parameters["uColor"]?.SetValue(ClairDeLunePalette.SoftBlue.ToVector4());
            _celestialShader.Parameters["uSecondaryColor"]?.SetValue(ClairDeLunePalette.NightMist.ToVector4());
            _celestialShader.Parameters["uOpacity"]?.SetValue(0.6f);
            _celestialShader.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly);
            _celestialShader.Parameters["uIntensity"]?.SetValue(1f);
            _celestialShader.Parameters["uOverbrightMult"]?.SetValue(1.0f);
            _celestialShader.Parameters["uScrollSpeed"]?.SetValue(1f);
            _celestialShader.Parameters["uDistortionAmt"]?.SetValue(0.02f);
            _celestialShader.Parameters["uHasSecondaryTex"]?.SetValue(false);

            _celestialShader.CurrentTechnique = _celestialShader.Techniques["CelestialOrbitCore"];

            sb.Begin(SpriteSortMode.Immediate, MagnumBlendStates.ShaderAdditive,
                SamplerState.LinearWrap, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, _celestialShader, matrix);

            Texture2D sc = _softCircle.Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float bodyScale = 18f / sc.Width;
            sb.Draw(sc, drawPos, null, Color.White, 0f, sc.Size() * 0.5f, bodyScale, SpriteEffects.None, 0f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                SamplerState.LinearClamp, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null, matrix);
        }

        // ---- PASS 3: Multi-scale bloom halo ----
        private void DrawBloomHalo(SpriteBatch sb, Matrix matrix)
        {
            Vector2 pos = Projectile.Center - Main.screenPosition;
            Texture2D srb = _softRadialBloom.Value;
            Texture2D pb = _pointBloom.Value;
            float dreamPulse = 0.9f + 0.1f * MathF.Sin(Main.GlobalTimeWrappedHourly * 3f + Projectile.whoAmI);

            // Outer dreamy haze
            sb.Draw(srb, pos, null,
                ClairDeLunePalette.NightMist with { A = 0 } * 0.15f * dreamPulse, 0f, srb.Size() * 0.5f,
                22f / srb.Width, SpriteEffects.None, 0f);

            // Mid soft blue
            sb.Draw(srb, pos, null,
                ClairDeLunePalette.SoftBlue with { A = 0 } * 0.2f * dreamPulse, 0f, srb.Size() * 0.5f,
                14f / srb.Width, SpriteEffects.None, 0f);

            // Core pearl
            sb.Draw(pb, pos, null,
                ClairDeLunePalette.PearlFrost with { A = 0 } * 0.2f, 0f, pb.Size() * 0.5f,
                8f / pb.Width, SpriteEffects.None, 0f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null, matrix);
        }
    }
}
