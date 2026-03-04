using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.Shaders;
using MagnumOpus.Content.ClairDeLune.Weapons.TemporalPiercer.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.ClairDeLune.Weapons.TemporalPiercer.Projectiles
{
    /// <summary>
    /// Temporal Thrust — Ultra-precise rapier thrust with CrystalLance.fx shader.
    /// 4 render passes: (1) VertexStrip thrust line with SparkleTrailShader,
    /// (2) CrystalLanceThrust overlay on stretched quad, (3) Multi-layer tip flare,
    /// (4) Rapier sprite. Inflicts Temporal Puncture marks → FrozenMoment at 5 stacks.
    /// </summary>
    public class TemporalThrustProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Content/ClairDeLune/Weapons/TemporalPiercer/TemporalPiercer";

        private int _thrustTimer;
        private float _thrustLength;
        private const float MaxReach = 110f;
        private const int ThrustDuration = 25;

        // Trail recording for thrust line strip
        private const int TrailLength = 16;
        private Vector2[] _trailPositions = new Vector2[TrailLength];
        private float[] _trailRotations = new float[TrailLength];
        private int _trailIndex;

        // --- Shader + texture caching ---
        private static Effect _sparkleTrailShader;
        private static Effect _crystalLanceShader;
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
            Projectile.width = 40;
            Projectile.height = 40;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.timeLeft = ThrustDuration;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.ownerHitCheck = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = ThrustDuration;
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            _thrustTimer++;

            float progress = (float)_thrustTimer / ThrustDuration;
            if (progress < 0.4f)
                _thrustLength = MathHelper.SmoothStep(0f, MaxReach, progress / 0.4f);
            else
                _thrustLength = MathHelper.SmoothStep(MaxReach, 0f, (progress - 0.4f) / 0.6f);

            Projectile.rotation = Projectile.velocity.ToRotation();
            Vector2 tipDir = Projectile.rotation.ToRotationVector2();
            Vector2 tipPos = player.Center + tipDir * _thrustLength;
            Projectile.Center = tipPos;

            // Record trail positions along the thrust line
            _trailPositions[_trailIndex % TrailLength] = tipPos;
            _trailRotations[_trailIndex % TrailLength] = Projectile.rotation;
            _trailIndex++;

            player.ChangeDir(tipDir.X > 0 ? 1 : -1);
            player.heldProj = Projectile.whoAmI;
            player.itemTime = 2;
            player.itemAnimation = 2;

            // Precision sparkle at extended tip
            if (_thrustLength > MaxReach * 0.5f && _thrustTimer % 2 == 0)
            {
                Color sparkColor = ClairDeLunePalette.PearlFrost with { A = 0 } * 0.6f;
                var spark = new SparkleParticle(tipPos + Main.rand.NextVector2Circular(4f, 4f),
                    tipDir * 0.5f, sparkColor, 0.15f, 8);
                MagnumParticleHandler.SpawnParticle(spark);
            }

            // Clockwork gold accent at maximum extension
            if (_thrustTimer == (int)(ThrustDuration * 0.4f))
            {
                var goldFlash = new BloomParticle(tipPos, Vector2.Zero,
                    ClairDeLunePalette.ClockworkBrass with { A = 0 } * 0.5f, 0.2f, 6);
                MagnumParticleHandler.SpawnParticle(goldFlash);
            }

            Lighting.AddLight(tipPos, ClairDeLunePalette.PearlBlue.ToVector3() * (_thrustLength / MaxReach) * 0.7f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Player player = Main.player[Projectile.owner];
            var piercePlayer = player.GetModPlayer<TemporalPiercerPlayer>();
            int stacks = piercePlayer.AddPunctureMark(target.whoAmI, damageDone);

            target.AddBuff(BuffID.Slow, 120);

            var punctureFlash = new BloomParticle(target.Center, Vector2.Zero,
                ClairDeLunePalette.PearlWhite with { A = 0 } * 0.7f, 0.3f, 12);
            MagnumParticleHandler.SpawnParticle(punctureFlash);

            var clockSpark = new SparkleParticle(target.Center,
                Main.rand.NextVector2Circular(3f, 3f),
                ClairDeLunePalette.ClockworkBrass with { A = 0 } * 0.8f, 0.3f, 15);
            MagnumParticleHandler.SpawnParticle(clockSpark);

            for (int i = 0; i < stacks; i++)
            {
                float angle = MathHelper.TwoPi * i / 5f - MathHelper.PiOver2;
                Vector2 markPos = target.Center + angle.ToRotationVector2() * 20f;
                Color markColor = i < stacks - 1
                    ? ClairDeLunePalette.SoftBlue with { A = 0 } * 0.4f
                    : ClairDeLunePalette.ClockworkBrass with { A = 0 } * 0.7f;
                var mark = new GenericGlowParticle(markPos, Vector2.Zero, markColor, 0.08f, 20, true);
                MagnumParticleHandler.SpawnParticle(mark);
            }

            if (stacks >= 5)
            {
                float burstDamage = piercePlayer.ConsumeAllMarks(target.whoAmI);
                int dmg = Math.Max((int)burstDamage, damageDone * 3);
                Projectile.NewProjectile(Projectile.GetSource_FromAI(), target.Center, Vector2.Zero,
                    ModContent.ProjectileType<FrozenMomentProjectile>(),
                    dmg, 12f, Projectile.owner, target.whoAmI);
            }

            if (Main.rand.NextFloat() < 0.15f)
            {
                Vector2 pierceDir = Projectile.rotation.ToRotationVector2();
                Vector2 behindPos = target.Center + pierceDir * (target.width + 16);
                for (int i = 0; i < 5; i++)
                {
                    Vector2 trailPos = Vector2.Lerp(target.Center, behindPos, i / 4f);
                    var pierceSpark = new GenericGlowParticle(trailPos,
                        pierceDir * 2f + Main.rand.NextVector2Circular(1f, 1f),
                        ClairDeLunePalette.PearlWhite with { A = 0 } * 0.5f, 0.1f, 10, true);
                    MagnumParticleHandler.SpawnParticle(pierceSpark);
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
            _starFlare ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/StarFlare", AssetRequestMode.ImmediateLoad);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (Main.dedServ) return false;
            LoadTextures();

            Player player = Main.player[Projectile.owner];
            float thrustNorm = _thrustLength / MaxReach;
            if (thrustNorm < 0.05f) return false;

            SpriteBatch sb = Main.spriteBatch;
            Matrix matrix = Main.GameViewMatrix.TransformationMatrix;

            DrawSparkleTrailLine(sb, matrix, player, thrustNorm); // Pass 1: VertexStrip thrust line
            DrawCrystalLanceOverlay(sb, matrix, player, thrustNorm); // Pass 2: CrystalLance shader overlay
            DrawTipFlare(sb, matrix, thrustNorm);                  // Pass 3: Multi-layer tip bloom
            DrawRapierSprite(sb, matrix, player, thrustNorm, lightColor); // Pass 4: Rapier sprite
            return false;
        }

        // ---- PASS 1: SparkleTrailShader VertexStrip thrust line ----
        private void DrawSparkleTrailLine(SpriteBatch sb, Matrix matrix, Player player, float thrustNorm)
        {
            // Build a 2-point strip from player center to tip
            Vector2 dir = Projectile.rotation.ToRotationVector2();
            Vector2[] positions = { player.Center, player.Center + dir * _thrustLength };
            float[] rotations = { Projectile.rotation, Projectile.rotation };

            _strip ??= new VertexStrip();
            _strip.PrepareStrip(positions, rotations,
                (float progress) =>
                {
                    Color c = Color.Lerp(ClairDeLunePalette.NightMist, ClairDeLunePalette.PearlFrost, progress);
                    return c with { A = 0 } * thrustNorm;
                },
                (float progress) => MathHelper.Lerp(4f, 8f, progress) * thrustNorm,
                -Main.screenPosition, 2, includeBacksides: true);

            _sparkleTrailShader ??= ModContent.Request<Effect>(
                "MagnumOpus/Content/FoundationWeapons/SparkleProjectileFoundation/Shaders/SparkleTrailShader",
                AssetRequestMode.ImmediateLoad).Value;

            sb.End();

            if (_sparkleTrailShader != null)
            {
                _sparkleTrailShader.Parameters["WorldViewProjection"]?.SetValue(
                    Main.GameViewMatrix.NormalizedTransformationmatrix);
                _sparkleTrailShader.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly);
                _sparkleTrailShader.Parameters["trailIntensity"]?.SetValue(1.5f * thrustNorm);
                _sparkleTrailShader.Parameters["sparkleSpeed"]?.SetValue(3f);
                _sparkleTrailShader.Parameters["sparkleScale"]?.SetValue(5f);
                _sparkleTrailShader.Parameters["glitterDensity"]?.SetValue(12f);
                _sparkleTrailShader.Parameters["tipFadeStart"]?.SetValue(0.6f);
                _sparkleTrailShader.Parameters["edgeSoftness"]?.SetValue(0.15f);
                _sparkleTrailShader.Parameters["coreColor"]?.SetValue(ClairDeLunePalette.PearlFrost.ToVector3());
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

        // ---- PASS 2: CrystalLance.fx shader overlay ----
        private void DrawCrystalLanceOverlay(SpriteBatch sb, Matrix matrix, Player player, float thrustNorm)
        {
            _crystalLanceShader ??= ShaderLoader.CrystalLance;
            if (_crystalLanceShader == null) return;

            sb.End();

            _crystalLanceShader.Parameters["uColor"]?.SetValue(ClairDeLunePalette.PearlFrost.ToVector4());
            _crystalLanceShader.Parameters["uSecondaryColor"]?.SetValue(ClairDeLunePalette.ClockworkBrass.ToVector4());
            _crystalLanceShader.Parameters["uOpacity"]?.SetValue(thrustNorm * 0.6f);
            _crystalLanceShader.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly);
            _crystalLanceShader.Parameters["uIntensity"]?.SetValue(1.8f);
            _crystalLanceShader.Parameters["uOverbrightMult"]?.SetValue(1.3f);
            _crystalLanceShader.Parameters["uScrollSpeed"]?.SetValue(4f);
            _crystalLanceShader.Parameters["uDistortionAmt"]?.SetValue(0.02f);
            _crystalLanceShader.Parameters["uHasSecondaryTex"]?.SetValue(false);

            _crystalLanceShader.CurrentTechnique = _crystalLanceShader.Techniques["CrystalLanceThrust"];

            sb.Begin(SpriteSortMode.Immediate, BlendState.Additive,
                SamplerState.LinearWrap, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, _crystalLanceShader, matrix);

            // Stretched SoftCircle quad along thrust line
            Texture2D sc = _softCircle.Value;
            Vector2 midPoint = player.Center + Projectile.rotation.ToRotationVector2() * _thrustLength * 0.5f;
            Vector2 midDraw = midPoint - Main.screenPosition;
            float scaleX = _thrustLength / sc.Width;
            float scaleY = 12f / sc.Height * thrustNorm;
            sb.Draw(sc, midDraw, null, Color.White, Projectile.rotation, sc.Size() * 0.5f,
                new Vector2(scaleX, scaleY), SpriteEffects.None, 0f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive,
                SamplerState.LinearClamp, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null, matrix);
        }

        // ---- PASS 3: Multi-layer tip flare ----
        private void DrawTipFlare(SpriteBatch sb, Matrix matrix, float thrustNorm)
        {
            Vector2 tipDraw = Projectile.Center - Main.screenPosition;
            float time = Main.GlobalTimeWrappedHourly;

            Texture2D srb = _softRadialBloom.Value;
            Texture2D pb = _pointBloom.Value;
            Texture2D sf = _starFlare.Value;

            // Outer glow
            sb.Draw(srb, tipDraw, null, ClairDeLunePalette.NightMist with { A = 0 } * 0.3f * thrustNorm,
                0f, srb.Size() * 0.5f, 30f / srb.Width, SpriteEffects.None, 0f);
            // Mid glow
            sb.Draw(srb, tipDraw, null, ClairDeLunePalette.SoftBlue with { A = 0 } * 0.45f * thrustNorm,
                0f, srb.Size() * 0.5f, 18f / srb.Width, SpriteEffects.None, 0f);
            // Core
            sb.Draw(pb, tipDraw, null, ClairDeLunePalette.PearlWhite with { A = 0 } * 0.6f * thrustNorm,
                0f, pb.Size() * 0.5f, 10f / pb.Width, SpriteEffects.None, 0f);
            // Star flare at tip
            float goldPulse = 0.3f + 0.7f * MathF.Sin(_thrustTimer * 0.3f);
            sb.Draw(sf, tipDraw, null, ClairDeLunePalette.ClockworkBrass with { A = 0 } * 0.25f * thrustNorm * goldPulse,
                time * 3f, sf.Size() * 0.5f, 20f / sf.Width, SpriteEffects.None, 0f);
        }

        // ---- PASS 4: Rapier sprite ----
        private void DrawRapierSprite(SpriteBatch sb, Matrix matrix, Player player, float thrustNorm, Color lightColor)
        {
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null, matrix);

            Texture2D rapierTex = ModContent.Request<Texture2D>(
                "MagnumOpus/Content/ClairDeLune/Weapons/TemporalPiercer/TemporalPiercer").Value;
            if (rapierTex != null)
            {
                Vector2 rapierOrigin = new Vector2(0, rapierTex.Height * 0.5f);
                float scale = _thrustLength / rapierTex.Width;
                scale = MathHelper.Clamp(scale, 0.3f, 1.2f);
                Vector2 rapierPos = player.Center - Main.screenPosition;
                sb.Draw(rapierTex, rapierPos, null, lightColor * thrustNorm, Projectile.rotation,
                    rapierOrigin, scale, SpriteEffects.None, 0f);
            }
        }
    }
}
