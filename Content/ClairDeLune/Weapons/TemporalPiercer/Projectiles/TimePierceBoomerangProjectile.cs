using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.Shaders;
using MagnumOpus.Content.ClairDeLune.Weapons.TemporalPiercer.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.ClairDeLune.Weapons.TemporalPiercer.Projectiles
{
    /// <summary>
    /// Time-Pierce Boomerang — Alt fire for TemporalPiercer.
    /// Rapier launches as a spinning boomerang that pierces through marked enemies.
    /// +30% damage per Temporal Puncture mark. Returns to player.
    /// 3 render passes: (1) CrystalLance CrystalLanceThrust piercing aura,
    /// (2) SparkleTrailShader VertexStrip crystal trail, (3) Multi-scale bloom + spinning rapier sprite.
    /// </summary>
    public class TimePierceBoomerangProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Content/ClairDeLune/Weapons/TemporalPiercer/TemporalPiercer";

        private bool _returning;
        private const float OutSpeed = 14f;
        private const float ReturnSpeed = 16f;
        private const float MaxDistance = 500f;
        private float _distanceTraveled;

        // Trail ring buffer
        private const int TrailLength = 16;
        private Vector2[] _trailPositions = new Vector2[TrailLength];
        private float[] _trailRotations = new float[TrailLength];
        private int _trailIndex;
        private VertexStrip _strip;

        // --- Shader + texture caching ---
        private static Effect _crystalLanceShader;
        private static Effect _sparkleTrailShader;
        private static Asset<Texture2D> _softCircle;
        private static Asset<Texture2D> _softRadialBloom;
        private static Asset<Texture2D> _pointBloom;
        private static Asset<Texture2D> _sparkleHard;
        private static Asset<Texture2D> _glowMask;
        private static Asset<Texture2D> _gradientLUT;

        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 30;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 300;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 15;
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];

            // Spin the rapier sprite
            Projectile.rotation += _returning ? -0.3f : 0.3f;

            // Record trail
            int idx = _trailIndex % TrailLength;
            _trailPositions[idx] = Projectile.Center;
            _trailRotations[idx] = Projectile.velocity.ToRotation();
            _trailIndex++;

            if (!_returning)
            {
                _distanceTraveled += Projectile.velocity.Length();
                if (_distanceTraveled >= MaxDistance)
                    _returning = true;
            }

            if (_returning)
            {
                Vector2 toPlayer = player.Center - Projectile.Center;
                float dist = toPlayer.Length();

                if (dist < 30f)
                {
                    Projectile.Kill();
                    return;
                }

                Projectile.velocity = toPlayer.SafeNormalize(Vector2.UnitX) * ReturnSpeed;
                if (dist < 100f)
                    Projectile.velocity *= 1.5f;
            }

            // Sparkle trail
            if (Main.GameUpdateCount % 2 == 0)
            {
                var sparkle = new SparkleParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(6f, 6f),
                    -Projectile.velocity * 0.1f,
                    ClairDeLunePalette.PearlFrost with { A = 0 } * 0.4f, 0.08f, 8);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }

            Lighting.AddLight(Projectile.Center, ClairDeLunePalette.SoftBlue.ToVector3() * 0.4f);
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            Player player = Main.player[Projectile.owner];
            var piercePlayer = player.GetModPlayer<TemporalPiercerPlayer>();
            int stacks = piercePlayer.GetStacks(target.whoAmI);

            if (stacks > 0)
                modifiers.FinalDamage += 0.3f * stacks;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Player player = Main.player[Projectile.owner];
            var piercePlayer = player.GetModPlayer<TemporalPiercerPlayer>();
            int stacks = piercePlayer.GetStacks(target.whoAmI);

            // Pierce-through VFX: trail of clockwork sparkles
            Vector2 dir = Projectile.velocity.SafeNormalize(Vector2.UnitX);
            for (int i = 0; i < 6; i++)
            {
                Vector2 pos = target.Center + dir * (i * 10f - 25f);
                Color col = Color.Lerp(ClairDeLunePalette.SoftBlue, ClairDeLunePalette.ClockworkBrass, i / 5f) with { A = 0 };
                var pierceSpark = new GenericGlowParticle(pos,
                    dir * 4f + Main.rand.NextVector2Circular(2f, 2f),
                    col * 0.5f, 0.1f, 8, true);
                MagnumParticleHandler.SpawnParticle(pierceSpark);
            }

            if (stacks > 0)
            {
                var stackFlash = new BloomParticle(target.Center, Vector2.Zero,
                    ClairDeLunePalette.ClockworkBrass with { A = 0 } * 0.4f, 0.3f, 6);
                MagnumParticleHandler.SpawnParticle(stackFlash);
            }

            SoundEngine.PlaySound(SoundID.Item30 with { Pitch = 0.2f + stacks * 0.1f, Volume = 0.4f }, target.Center);
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
            try
            {
            Matrix matrix = Main.GameViewMatrix.TransformationMatrix;

            DrawSparkleTrail(sb, matrix);      // Pass 1: SparkleTrailShader VertexStrip crystal trail
            DrawCrystalAura(sb, matrix);       // Pass 2: CrystalLanceThrust piercing aura
            DrawBloomAndSprite(sb, matrix, lightColor); // Pass 3: Bloom + spinning rapier sprite
            ClairDeLuneVFXLibrary.DrawThemeAccents(sb, Projectile.Center, 0.5f, 0.3f);
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

        // ---- PASS 1: SparkleTrailShader VertexStrip crystal trail ----
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
                    ClairDeLunePalette.SoftBlue with { A = 0 }, progress) * (1f - progress * 0.8f),
                (float progress) => MathHelper.Lerp(12f, 1f, progress),
                -Main.screenPosition, count, includeBacksides: true);

            _sparkleTrailShader ??= ModContent.Request<Effect>(
                "MagnumOpus/Content/FoundationWeapons/SparkleProjectileFoundation/Shaders/SparkleTrailShader",
                AssetRequestMode.ImmediateLoad).Value;

            sb.End();

            _sparkleTrailShader.Parameters["WorldViewProjection"]?.SetValue(
                Main.GameViewMatrix.NormalizedTransformationmatrix);
            _sparkleTrailShader.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly * 2.5f);
            _sparkleTrailShader.Parameters["sparkleTex"]?.SetValue(_sparkleHard.Value);
            _sparkleTrailShader.Parameters["gradientTex"]?.SetValue(_gradientLUT.Value);
            _sparkleTrailShader.Parameters["glowMaskTex"]?.SetValue(_glowMask.Value);
            _sparkleTrailShader.Parameters["coreColor"]?.SetValue(ClairDeLunePalette.PearlFrost.ToVector3());
            _sparkleTrailShader.Parameters["outerColor"]?.SetValue(ClairDeLunePalette.SoftBlue.ToVector3());
            _sparkleTrailShader.Parameters["sparkleSpeed"]?.SetValue(4f);
            _sparkleTrailShader.Parameters["sparkleScale"]?.SetValue(0.45f);
            _sparkleTrailShader.Parameters["glitterDensity"]?.SetValue(5.0f);
            _sparkleTrailShader.Parameters["tipFadeStart"]?.SetValue(0.55f);
            _sparkleTrailShader.Parameters["edgeSoftness"]?.SetValue(0.3f);

            _sparkleTrailShader.CurrentTechnique.Passes["SparkleTrailPass"].Apply();
            _strip.DrawTrail();
            Main.pixelShader.CurrentTechnique.Passes[0].Apply();

            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                SamplerState.LinearClamp, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null, matrix);
        }

        // ---- PASS 2: CrystalLance CrystalLanceThrust piercing aura ----
        private void DrawCrystalAura(SpriteBatch sb, Matrix matrix)
        {
            _crystalLanceShader ??= ShaderLoader.CrystalLance;
            if (_crystalLanceShader == null) return;

            sb.End();

            _crystalLanceShader.Parameters["uColor"]?.SetValue(ClairDeLunePalette.PearlFrost.ToVector4());
            _crystalLanceShader.Parameters["uSecondaryColor"]?.SetValue(ClairDeLunePalette.SoftBlue.ToVector4());
            _crystalLanceShader.Parameters["uOpacity"]?.SetValue(0.4f);
            _crystalLanceShader.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly);
            _crystalLanceShader.Parameters["uIntensity"]?.SetValue(_returning ? 1.3f : 1f);
            _crystalLanceShader.Parameters["uOverbrightMult"]?.SetValue(1.0f);
            _crystalLanceShader.Parameters["uScrollSpeed"]?.SetValue(3f);
            _crystalLanceShader.Parameters["uDistortionAmt"]?.SetValue(0.02f);
            _crystalLanceShader.Parameters["uHasSecondaryTex"]?.SetValue(false);

            _crystalLanceShader.CurrentTechnique = _crystalLanceShader.Techniques["CrystalLanceThrust"];

            sb.Begin(SpriteSortMode.Immediate, MagnumBlendStates.ShaderAdditive,
                SamplerState.LinearWrap, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, _crystalLanceShader, matrix);

            Texture2D sc = _softCircle.Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float auraScale = 36f / sc.Width;
            sb.Draw(sc, drawPos, null, Color.White, Projectile.velocity.ToRotation(), sc.Size() * 0.5f, auraScale, SpriteEffects.None, 0f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                SamplerState.LinearClamp, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null, matrix);
        }

        // ---- PASS 3: Multi-scale bloom + spinning rapier sprite ----
        private void DrawBloomAndSprite(SpriteBatch sb, Matrix matrix, Color lightColor)
        {
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Texture2D srb = _softRadialBloom.Value;
            Texture2D pb = _pointBloom.Value;

            // Outer frost haze
            sb.Draw(srb, drawPos, null,
                ClairDeLunePalette.NightMist with { A = 0 } * 0.2f, 0f, srb.Size() * 0.5f,
                30f / srb.Width, SpriteEffects.None, 0f);

            // Mid blue glow
            sb.Draw(srb, drawPos, null,
                ClairDeLunePalette.SoftBlue with { A = 0 } * 0.3f, 0f, srb.Size() * 0.5f,
                18f / srb.Width, SpriteEffects.None, 0f);

            // Core pearl
            sb.Draw(pb, drawPos, null,
                ClairDeLunePalette.PearlFrost with { A = 0 } * 0.2f, 0f, pb.Size() * 0.5f,
                10f / pb.Width, SpriteEffects.None, 0f);

            // Switch to AlphaBlend for the rapier sprite
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null, matrix);

            // Draw spinning rapier sprite
            Texture2D rapierTex = ModContent.Request<Texture2D>(Texture).Value;
            if (rapierTex != null)
            {
                Vector2 rapierOrigin = rapierTex.Size() * 0.5f;
                sb.Draw(rapierTex, drawPos, null, lightColor, Projectile.rotation,
                    rapierOrigin, 0.8f, SpriteEffects.None, 0f);
            }
        }
    }
}
