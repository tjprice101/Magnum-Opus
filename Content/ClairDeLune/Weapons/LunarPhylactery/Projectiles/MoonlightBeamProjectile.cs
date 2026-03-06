using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.Shaders;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.ClairDeLune.Weapons.LunarPhylactery.Projectiles
{
    /// <summary>
    /// Moonlight Beam — sustained beam projectile fired by Moonlight Sentinel.
    /// Rendering: (1) SparkleTrailShader VertexStrip trail, (2) SoulBeam.fx shimmer overlay,
    /// (3) Multi-scale bloom core, (4) Crossing-burst VFX.
    /// ai[0] = target NPC index. Pierces up to 3 enemies.
    /// </summary>
    public class MoonlightBeamProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Textures/InvisibleProjectile";

        private const int TrailLength = 24;
        private Vector2[] _trailPositions = new Vector2[TrailLength];
        private float[] _trailRotations = new float[TrailLength];
        private int _trailIndex;

        // --- Shader + texture caching ---
        private static Effect _sparkleTrailShader;
        private static Effect _soulBeamShader;
        private static Asset<Texture2D> _sparkleTex;
        private static Asset<Texture2D> _gradientLUT;
        private static Asset<Texture2D> _glowMask;
        private static Asset<Texture2D> _softGlow;
        private static Asset<Texture2D> _softRadialBloom;
        private static Asset<Texture2D> _pointBloom;
        private static Asset<Texture2D> _softCircle;
        private VertexStrip _strip;

        public override void SetDefaults()
        {
            Projectile.width = 8;
            Projectile.height = 8;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.penetrate = 3;
            Projectile.timeLeft = 60;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.extraUpdates = 2;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 15;
        }

        public override void AI()
        {
            // Record trail (ring buffer)
            _trailPositions[_trailIndex % TrailLength] = Projectile.Center;
            _trailRotations[_trailIndex % TrailLength] = Projectile.velocity.ToRotation();
            _trailIndex++;

            Projectile.rotation = Projectile.velocity.ToRotation();

            // Light homing toward target
            int targetIndex = (int)Projectile.ai[0];
            if (targetIndex >= 0 && targetIndex < Main.maxNPCs && Main.npc[targetIndex].active)
            {
                Vector2 toTarget = (Main.npc[targetIndex].Center - Projectile.Center).SafeNormalize(Vector2.UnitX);
                Projectile.velocity = Vector2.Lerp(Projectile.velocity.SafeNormalize(Vector2.UnitX),
                    toTarget, 0.04f) * Projectile.velocity.Length();
            }

            // Beam edge shimmer particles
            if (Main.rand.NextBool(4))
            {
                Vector2 perp = new Vector2(-Projectile.velocity.Y, Projectile.velocity.X).SafeNormalize(Vector2.UnitY);
                Vector2 offset = perp * Main.rand.NextFloat(-4f, 4f);
                var glow = new GenericGlowParticle(
                    Projectile.Center + offset,
                    -Projectile.velocity * 0.03f + perp * Main.rand.NextFloat(-0.3f, 0.3f),
                    ClairDeLunePalette.PearlBlue with { A = 0 } * 0.25f,
                    0.04f, 10);
                MagnumParticleHandler.SpawnParticle(glow);
            }

            Lighting.AddLight(Projectile.Center, ClairDeLunePalette.PearlBlue.ToVector3() * 0.25f);

            // Check for Beam Crossing with other sentinel beams
            CheckBeamCrossing();
        }

        private void CheckBeamCrossing()
        {
            if (Main.rand.NextBool(10)) return;

            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile other = Main.projectile[i];
                if (!other.active || other.whoAmI == Projectile.whoAmI) continue;
                if (other.type != Projectile.type || other.owner != Projectile.owner) continue;

                float dist = Vector2.Distance(Projectile.Center, other.Center);
                if (dist < 32f)
                {
                    Vector2 crossPos = (Projectile.Center + other.Center) * 0.5f;

                    for (int p = 0; p < 6; p++)
                    {
                        float angle = MathHelper.TwoPi * p / 6f;
                        Vector2 vel = angle.ToRotationVector2() * 2.5f;
                        var burst = new SparkleParticle(crossPos, vel,
                            ClairDeLunePalette.WhiteHot with { A = 0 } * 0.5f,
                            0.1f, 15);
                        MagnumParticleHandler.SpawnParticle(burst);
                    }

                    for (int n = 0; n < Main.maxNPCs; n++)
                    {
                        NPC npc = Main.npc[n];
                        if (!npc.active || npc.friendly || npc.dontTakeDamage) continue;
                        if (Vector2.Distance(crossPos, npc.Center) < 48f)
                        {
                            Player owner = Main.player[Projectile.owner];
                            owner.ApplyDamageToNPC(npc, (int)(Projectile.damage * 0.5f), 2f, 0, false);
                        }
                    }
                    break;
                }
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            for (int i = 0; i < 4; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(2f, 2f);
                var spark = new GlowSparkParticle(target.Center, vel,
                    ClairDeLunePalette.PearlBlue with { A = 0 } * 0.5f,
                    0.08f, 12);
                MagnumParticleHandler.SpawnParticle(spark);
            }
        }

        private void LoadTextures()
        {
            _sparkleTex ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/TrailsAndRibbons/SpiralTrail", AssetRequestMode.ImmediateLoad);
            _gradientLUT ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/ColorGradients/ClairDeLuneGradientLUTandRAMP", AssetRequestMode.ImmediateLoad);
            _glowMask ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftGlow", AssetRequestMode.ImmediateLoad);
            _softGlow ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftGlow", AssetRequestMode.ImmediateLoad);
            _softRadialBloom ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftRadialBloom", AssetRequestMode.ImmediateLoad);
            _pointBloom ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/PointBloom", AssetRequestMode.ImmediateLoad);
            _softCircle ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/MasksAndShapes/SoftCircle", AssetRequestMode.ImmediateLoad);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (Main.dedServ) return false;
            LoadTextures();

            SpriteBatch sb = Main.spriteBatch;
            Matrix matrix = Main.GameViewMatrix.TransformationMatrix;

            DrawSparkleTrail(sb, matrix);    // Pass 1: VertexStrip sparkle trail
            DrawSoulBeamOverlay(sb, matrix); // Pass 2: SoulBeam.fx shimmer on SoftCircle quad
            DrawBloomCore(sb, matrix);       // Pass 3: Multi-scale bloom at projectile head
            return false;
        }

        // ---- PASS 1: SparkleTrailShader VertexStrip trail ----
        private void DrawSparkleTrail(SpriteBatch sb, Matrix matrix)
        {
            int count = Math.Min(_trailIndex, TrailLength);
            if (count < 3) return;

            // Build ordered arrays from ring buffer (newest → oldest)
            Vector2[] positions = new Vector2[count];
            float[] rotations = new float[count];
            for (int i = 0; i < count; i++)
            {
                int idx = ((_trailIndex - 1 - i) % TrailLength + TrailLength) % TrailLength;
                positions[i] = _trailPositions[idx];
                rotations[i] = _trailRotations[idx];
            }

            _strip ??= new VertexStrip();
            _strip.PrepareStrip(positions, rotations,
                (float progress) =>
                {
                    Color c = Color.Lerp(ClairDeLunePalette.PearlWhite, ClairDeLunePalette.PearlBlue, progress);
                    return c with { A = 0 } * (1f - progress * 0.8f);
                },
                (float progress) => MathHelper.Lerp(10f, 2f, progress),
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
                _sparkleTrailShader.Parameters["trailIntensity"]?.SetValue(1.2f);
                _sparkleTrailShader.Parameters["sparkleSpeed"]?.SetValue(1.5f);
                _sparkleTrailShader.Parameters["sparkleScale"]?.SetValue(3f);
                _sparkleTrailShader.Parameters["glitterDensity"]?.SetValue(8f);
                _sparkleTrailShader.Parameters["tipFadeStart"]?.SetValue(0.7f);
                _sparkleTrailShader.Parameters["edgeSoftness"]?.SetValue(0.2f);
                _sparkleTrailShader.Parameters["coreColor"]?.SetValue(ClairDeLunePalette.PearlWhite.ToVector3());
                _sparkleTrailShader.Parameters["outerColor"]?.SetValue(ClairDeLunePalette.PearlBlue.ToVector3());
                _sparkleTrailShader.Parameters["sparkleTex"]?.SetValue(_sparkleTex.Value);
                _sparkleTrailShader.Parameters["gradientTex"]?.SetValue(_gradientLUT.Value);
                _sparkleTrailShader.Parameters["glowMaskTex"]?.SetValue(_glowMask.Value);

                _sparkleTrailShader.CurrentTechnique.Passes["SparkleTrailPass"].Apply();
                _strip.DrawTrail();
                Main.pixelShader.CurrentTechnique.Passes[0].Apply();
            }

            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                SamplerState.LinearClamp, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null, matrix);
        }

        // ---- PASS 2: SoulBeam.fx shimmer overlay via SpriteBatch Effect ----
        private void DrawSoulBeamOverlay(SpriteBatch sb, Matrix matrix)
        {
            _soulBeamShader ??= ShaderLoader.SoulBeam;
            if (_soulBeamShader == null) return;

            sb.End();

            // Configure SoulBeam shader
            _soulBeamShader.Parameters["uColor"]?.SetValue(ClairDeLunePalette.PearlBlue.ToVector4());
            _soulBeamShader.Parameters["uSecondaryColor"]?.SetValue(ClairDeLunePalette.SoftBlue.ToVector4());
            _soulBeamShader.Parameters["uOpacity"]?.SetValue(0.35f);
            _soulBeamShader.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly);
            _soulBeamShader.Parameters["uIntensity"]?.SetValue(1.5f);
            _soulBeamShader.Parameters["uOverbrightMult"]?.SetValue(1.2f);
            _soulBeamShader.Parameters["uScrollSpeed"]?.SetValue(2f);
            _soulBeamShader.Parameters["uDistortionAmt"]?.SetValue(0.015f);
            _soulBeamShader.Parameters["uHasSecondaryTex"]?.SetValue(false);

            _soulBeamShader.CurrentTechnique = _soulBeamShader.Techniques["SoulBeamTether"];

            sb.Begin(SpriteSortMode.Immediate, MagnumBlendStates.ShaderAdditive,
                SamplerState.LinearWrap, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, _soulBeamShader, matrix);

            // Draw shimmer on SoftCircle quad along recent trail
            Texture2D sc = _softCircle.Value;
            Vector2 scOrigin = sc.Size() * 0.5f;
            int count = Math.Min(_trailIndex, 6);
            for (int i = 0; i < count; i++)
            {
                int idx = ((_trailIndex - 1 - i) % TrailLength + TrailLength) % TrailLength;
                float fade = 1f - (float)i / count;
                Vector2 pos = _trailPositions[idx] - Main.screenPosition;
                sb.Draw(sc, pos, null, ClairDeLunePalette.PearlBlue with { A = 0 } * 0.3f * fade,
                    0f, scOrigin, 14f / sc.Width, SpriteEffects.None, 0f);
            }

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                SamplerState.LinearClamp, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null, matrix);
        }

        // ---- PASS 3: Multi-scale bloom at projectile head ----
        private void DrawBloomCore(SpriteBatch sb, Matrix matrix)
        {
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float pulse = 0.85f + 0.15f * MathF.Sin(Main.GlobalTimeWrappedHourly * 8f);

            Texture2D srb = _softRadialBloom.Value;
            Texture2D pb = _pointBloom.Value;

            // Wide ambient halo
            sb.Draw(srb, drawPos, null, ClairDeLunePalette.NightMist with { A = 0 } * 0.15f * pulse,
                0f, srb.Size() * 0.5f, 24f / srb.Width, SpriteEffects.None, 0f);
            // Mid glow
            sb.Draw(srb, drawPos, null, ClairDeLunePalette.PearlBlue with { A = 0 } * 0.35f,
                0f, srb.Size() * 0.5f, 14f / srb.Width, SpriteEffects.None, 0f);
            // Bright core
            sb.Draw(pb, drawPos, null, ClairDeLunePalette.WhiteHot with { A = 0 } * 0.6f,
                0f, pb.Size() * 0.5f, 8f / pb.Width, SpriteEffects.None, 0f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                SamplerState.PointClamp, DepthStencilState.None,
                RasterizerState.CullNone, null, matrix);
        }
    }
}