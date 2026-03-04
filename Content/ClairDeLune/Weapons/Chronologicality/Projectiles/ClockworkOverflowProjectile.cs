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

namespace MagnumOpus.Content.ClairDeLune.Weapons.Chronologicality.Projectiles
{
    /// <summary>
    /// Clockwork Overflow — 8-tile temporal detonation after a perfect 3-phase combo.
    /// 4 phases: Trigger -> Freeze -> Detonation -> Resume.
    /// 3 render passes: (1) RadialNoiseMaskShader zone disc,
    /// (2) TemporalDrill.fx phase overlay, (3) Multi-scale bloom per phase.
    /// </summary>
    public class ClockworkOverflowProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Textures/InvisibleProjectile";

        private const int TriggerDuration = 10;
        private const int FreezeDuration = 20;
        private const int DetonationDuration = 15;
        private const int ResumeDuration = 15;
        private const int TotalLife = TriggerDuration + FreezeDuration + DetonationDuration + ResumeDuration;
        private const float DetonationRadius = 128f;

        private int _timer;
        private bool _hasDealtDamage;

        private enum OverflowPhase { Trigger, Freeze, Detonation, Resume }

        private OverflowPhase CurrentPhase
        {
            get
            {
                if (_timer < TriggerDuration) return OverflowPhase.Trigger;
                if (_timer < TriggerDuration + FreezeDuration) return OverflowPhase.Freeze;
                if (_timer < TriggerDuration + FreezeDuration + DetonationDuration) return OverflowPhase.Detonation;
                return OverflowPhase.Resume;
            }
        }

        // --- Shader + texture caching ---
        private static Effect _radialMaskShader;
        private static Effect _temporalDrillShader;
        private static Effect _pearlGlowShader;
        private static Asset<Texture2D> _noiseTex;
        private static Asset<Texture2D> _gradientLUT;
        private static Asset<Texture2D> _softCircle;
        private static Asset<Texture2D> _softRadialBloom;
        private static Asset<Texture2D> _starFlare;

        public override void SetDefaults()
        {
            Projectile.width = (int)(DetonationRadius * 2);
            Projectile.height = (int)(DetonationRadius * 2);
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.timeLeft = TotalLife;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }

        public override void AI()
        {
            _timer++;
            Projectile.Center = Main.player[Projectile.owner].Center;

            var phase = CurrentPhase;
            switch (phase)
            {
                case OverflowPhase.Trigger: TriggerPhase(); break;
                case OverflowPhase.Freeze: FreezePhase(); break;
                case OverflowPhase.Detonation: DetonationPhase(); break;
                case OverflowPhase.Resume: ResumePhase(); break;
            }

            float lightMult = phase == OverflowPhase.Detonation ? 1.5f : 0.8f;
            Lighting.AddLight(Projectile.Center, ClairDeLunePalette.PearlWhite.ToVector3() * lightMult);
        }

        private void TriggerPhase()
        {
            float triggerProgress = (float)_timer / TriggerDuration;

            if (_timer == 1)
                SoundEngine.PlaySound(SoundID.Item29 with { Pitch = -0.3f, Volume = 0.6f }, Projectile.Center);

            if (_timer % 3 == 0)
            {
                int numTicks = 12;
                for (int i = 0; i < numTicks; i++)
                {
                    float angle = MathHelper.TwoPi * i / numTicks;
                    float dist = DetonationRadius * 0.5f * triggerProgress;
                    Vector2 pos = Projectile.Center + angle.ToRotationVector2() * dist;

                    var tick = new GenericGlowParticle(pos, Vector2.Zero,
                        ClairDeLunePalette.ClockworkBrass with { A = 0 } * 0.6f,
                        0.1f, 8, true);
                    MagnumParticleHandler.SpawnParticle(tick);
                }
            }
        }

        private void FreezePhase()
        {
            int localTimer = _timer - TriggerDuration;

            if (localTimer % 5 == 0)
            {
                for (int i = 0; i < 6; i++)
                {
                    Vector2 offset = Main.rand.NextVector2Circular(DetonationRadius, DetonationRadius);
                    Vector2 slowDrift = Main.rand.NextVector2Circular(0.3f, 0.3f);
                    var frost = new GenericGlowParticle(
                        Projectile.Center + offset, slowDrift,
                        ClairDeLunePalette.PearlFrost with { A = 0 } * 0.3f,
                        0.08f + Main.rand.NextFloat() * 0.05f, 15, true);
                    MagnumParticleHandler.SpawnParticle(frost);
                }
            }
        }

        private void DetonationPhase()
        {
            int localTimer = _timer - TriggerDuration - FreezeDuration;
            Projectile.friendly = true;

            if (!_hasDealtDamage && localTimer == 0)
            {
                _hasDealtDamage = true;
                SoundEngine.PlaySound(SoundID.Item14 with { Pitch = -0.5f, Volume = 0.8f }, Projectile.Center);

                for (int ring = 0; ring < 10; ring++)
                {
                    float ringRadius = DetonationRadius * (ring + 1) / 10f;
                    Color ringColor = GetRingColor(ring);
                    int numPoints = 24 + ring * 4;
                    for (int p = 0; p < numPoints; p++)
                    {
                        float angle = MathHelper.TwoPi * p / numPoints + ring * 0.15f;
                        Vector2 vel = angle.ToRotationVector2() * (ringRadius / 10f);
                        var ringParticle = new GenericGlowParticle(
                            Projectile.Center, vel, ringColor with { A = 0 } * 0.6f,
                            0.12f - ring * 0.008f, 10 + ring, true);
                        MagnumParticleHandler.SpawnParticle(ringParticle);
                    }
                }

                for (int i = 0; i < 16; i++)
                {
                    float angle = MathHelper.TwoPi * i / 16f;
                    Vector2 vel = angle.ToRotationVector2() * (6f + Main.rand.NextFloat() * 4f);
                    Color gearColor = Main.rand.NextBool()
                        ? ClairDeLunePalette.ClockworkBrass
                        : ClairDeLunePalette.MoonbeamGold;
                    var gear = new SparkleParticle(
                        Projectile.Center, vel, gearColor with { A = 0 } * 0.7f,
                        0.15f, 20);
                    MagnumParticleHandler.SpawnParticle(gear);
                }

                var flash = new BloomParticle(Projectile.Center, Vector2.Zero,
                    ClairDeLunePalette.PearlWhite with { A = 0 } * 0.9f, 1.5f, 12);
                MagnumParticleHandler.SpawnParticle(flash);
            }

            if (localTimer % 3 == 0 && localTimer > 0)
            {
                float detProgress = (float)localTimer / DetonationDuration;
                float currentRadius = DetonationRadius * detProgress;
                int numPoints = 32;
                for (int p = 0; p < numPoints; p++)
                {
                    float angle = MathHelper.TwoPi * p / numPoints;
                    Vector2 edgePos = Projectile.Center + angle.ToRotationVector2() * currentRadius;
                    var edgeGlow = new GenericGlowParticle(
                        edgePos, Vector2.Zero,
                        ClairDeLunePalette.SoftBlue with { A = 0 } * 0.3f * (1f - detProgress),
                        0.08f, 6, true);
                    MagnumParticleHandler.SpawnParticle(edgeGlow);
                }
            }
        }

        private void ResumePhase()
        {
            Projectile.friendly = false;
            int localTimer = _timer - TriggerDuration - FreezeDuration - DetonationDuration;
            float resumeProgress = (float)localTimer / ResumeDuration;

            if (localTimer == 1)
            {
                for (int i = 0; i < 12; i++)
                {
                    float angle = MathHelper.TwoPi * i / 12f;
                    Vector2 vel = angle.ToRotationVector2() * (3f + Main.rand.NextFloat() * 5f);
                    var shard = new SparkleParticle(
                        Projectile.Center, vel,
                        ClairDeLunePalette.ClockworkBrass with { A = 0 } * 0.5f,
                        0.1f + Main.rand.NextFloat() * 0.08f, 25);
                    MagnumParticleHandler.SpawnParticle(shard);
                }

                SoundEngine.PlaySound(SoundID.Shatter with { Pitch = 0.2f, Volume = 0.5f }, Projectile.Center);
            }

            if (localTimer % 4 == 0)
            {
                float fade = 1f - resumeProgress;
                var ambient = new BloomParticle(Projectile.Center, Vector2.Zero,
                    ClairDeLunePalette.NightMist with { A = 0 } * 0.2f * fade,
                    0.8f * fade, 5);
                MagnumParticleHandler.SpawnParticle(ambient);
            }
        }

        private static Color GetRingColor(int ringIndex)
        {
            float t = ringIndex / 9f;
            if (t < 0.33f)
                return Color.Lerp(ClairDeLunePalette.NightMist, ClairDeLunePalette.SoftBlue, t / 0.33f);
            if (t < 0.66f)
                return Color.Lerp(ClairDeLunePalette.SoftBlue, ClairDeLunePalette.PearlFrost, (t - 0.33f) / 0.33f);
            return Color.Lerp(ClairDeLunePalette.PearlFrost, ClairDeLunePalette.ClockworkBrass, (t - 0.66f) / 0.34f);
        }

        private void LoadTextures()
        {
            _noiseTex ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/NoiseAndDistortion/PerlinNoise", AssetRequestMode.ImmediateLoad);
            _gradientLUT ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/ColorGradients/ClairDeLuneGradientLUTandRAMP", AssetRequestMode.ImmediateLoad);
            _softCircle ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/MasksAndShapes/SoftCircle", AssetRequestMode.ImmediateLoad);
            _softRadialBloom ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftRadialBloom", AssetRequestMode.ImmediateLoad);
            _starFlare ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/StarFlare", AssetRequestMode.ImmediateLoad);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (Main.dedServ) return false;
            LoadTextures();

            SpriteBatch sb = Main.spriteBatch;
            Matrix matrix = Main.GameViewMatrix.TransformationMatrix;
            var phase = CurrentPhase;

            DrawRadialZone(sb, matrix, phase);       // Pass 1: RadialNoiseMask zone
            DrawTemporalDrillOverlay(sb, matrix, phase); // Pass 2: TemporalDrill.fx overlay
            DrawPhaseBloom(sb, matrix, phase);       // Pass 3: Multi-scale bloom
            return false;
        }

        // ---- PASS 1: RadialNoiseMaskShader zone disc ----
        private void DrawRadialZone(SpriteBatch sb, Matrix matrix, OverflowPhase phase)
        {
            if (phase == OverflowPhase.Resume) return;

            _radialMaskShader ??= ModContent.Request<Effect>(
                "MagnumOpus/Content/FoundationWeapons/MaskFoundation/Shaders/RadialNoiseMaskShader",
                AssetRequestMode.ImmediateLoad).Value;
            if (_radialMaskShader == null) return;

            float zoneRadius = phase switch
            {
                OverflowPhase.Trigger => DetonationRadius * 0.5f * ((float)_timer / TriggerDuration),
                OverflowPhase.Freeze => DetonationRadius,
                OverflowPhase.Detonation => DetonationRadius,
                _ => 0f
            };
            if (zoneRadius <= 0f) return;

            float phaseIntensity = phase switch
            {
                OverflowPhase.Trigger => 0.6f,
                OverflowPhase.Freeze => 0.8f,
                OverflowPhase.Detonation => 1.4f,
                _ => 0f
            };

            Color primaryCol = phase == OverflowPhase.Detonation ? ClairDeLunePalette.MoonbeamGold : ClairDeLunePalette.PearlFrost;

            sb.End();

            _radialMaskShader.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly);
            _radialMaskShader.Parameters["scrollSpeed"]?.SetValue(phase == OverflowPhase.Freeze ? 0.05f : 0.4f);
            _radialMaskShader.Parameters["rotationSpeed"]?.SetValue(phase == OverflowPhase.Freeze ? 0.1f : 0.6f);
            _radialMaskShader.Parameters["circleRadius"]?.SetValue(0.44f);
            _radialMaskShader.Parameters["edgeSoftness"]?.SetValue(0.12f);
            _radialMaskShader.Parameters["intensity"]?.SetValue(phaseIntensity);
            _radialMaskShader.Parameters["primaryColor"]?.SetValue(primaryCol.ToVector3());
            _radialMaskShader.Parameters["coreColor"]?.SetValue(ClairDeLunePalette.PearlWhite.ToVector3());
            _radialMaskShader.Parameters["noiseTex"]?.SetValue(_noiseTex.Value);
            _radialMaskShader.Parameters["gradientTex"]?.SetValue(_gradientLUT.Value);

            _radialMaskShader.CurrentTechnique.Passes["RadialNoiseMaskPass"].Apply();

            sb.Begin(SpriteSortMode.Immediate, BlendState.Additive,
                SamplerState.LinearWrap, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, _radialMaskShader, matrix);

            Texture2D sc = _softCircle.Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float zoneScale = zoneRadius * 2f / sc.Width;
            sb.Draw(sc, drawPos, null, Color.White, 0f, sc.Size() * 0.5f, zoneScale, SpriteEffects.None, 0f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive,
                SamplerState.LinearClamp, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null, matrix);
        }

        // ---- PASS 2: TemporalDrill.fx phase overlay ----
        private void DrawTemporalDrillOverlay(SpriteBatch sb, Matrix matrix, OverflowPhase phase)
        {
            if (phase != OverflowPhase.Detonation && phase != OverflowPhase.Freeze) return;

            _temporalDrillShader ??= ShaderLoader.TemporalDrill;
            if (_temporalDrillShader == null) return;

            sb.End();

            bool isDetonating = phase == OverflowPhase.Detonation;
            float detProgress = isDetonating
                ? (float)(_timer - TriggerDuration - FreezeDuration) / DetonationDuration
                : 0f;

            _temporalDrillShader.Parameters["uColor"]?.SetValue(
                (isDetonating ? ClairDeLunePalette.MoonbeamGold : ClairDeLunePalette.PearlFrost).ToVector4());
            _temporalDrillShader.Parameters["uSecondaryColor"]?.SetValue(ClairDeLunePalette.ClockworkBrass.ToVector4());
            _temporalDrillShader.Parameters["uOpacity"]?.SetValue(isDetonating ? 0.6f * (1f - detProgress) : 0.3f);
            _temporalDrillShader.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly);
            _temporalDrillShader.Parameters["uIntensity"]?.SetValue(isDetonating ? 2f : 1f);
            _temporalDrillShader.Parameters["uOverbrightMult"]?.SetValue(1.2f);
            _temporalDrillShader.Parameters["uScrollSpeed"]?.SetValue(isDetonating ? 4f : 1f);
            _temporalDrillShader.Parameters["uDistortionAmt"]?.SetValue(isDetonating ? 0.03f : 0.01f);
            _temporalDrillShader.Parameters["uHasSecondaryTex"]?.SetValue(false);

            string technique = isDetonating ? "TemporalDrillBore" : "TemporalDrillGlow";
            _temporalDrillShader.CurrentTechnique = _temporalDrillShader.Techniques[technique];

            sb.Begin(SpriteSortMode.Immediate, BlendState.Additive,
                SamplerState.LinearWrap, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, _temporalDrillShader, matrix);

            Texture2D sc = _softCircle.Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float overlayScale = DetonationRadius * 2.2f / sc.Width;
            sb.Draw(sc, drawPos, null, Color.White, 0f, sc.Size() * 0.5f, overlayScale, SpriteEffects.None, 0f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive,
                SamplerState.LinearClamp, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null, matrix);
        }

        // ---- PASS 3: Multi-scale bloom per phase ----
        private void DrawPhaseBloom(SpriteBatch sb, Matrix matrix, OverflowPhase phase)
        {
            Texture2D srb = _softRadialBloom.Value;
            Texture2D sf = _starFlare.Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            switch (phase)
            {
                case OverflowPhase.Trigger:
                {
                    float t = (float)_timer / TriggerDuration;
                    sb.Draw(srb, drawPos, null,
                        ClairDeLunePalette.ClockworkBrass with { A = 0 } * 0.35f * t, 0f, srb.Size() * 0.5f,
                        DetonationRadius * t / srb.Width, SpriteEffects.None, 0f);
                    sb.Draw(srb, drawPos, null,
                        ClairDeLunePalette.PearlWhite with { A = 0 } * 0.2f * t, 0f, srb.Size() * 0.5f,
                        DetonationRadius * t * 0.4f / srb.Width, SpriteEffects.None, 0f);
                    break;
                }
                case OverflowPhase.Freeze:
                {
                    float t = (float)(_timer - TriggerDuration) / FreezeDuration;
                    float pulse = 0.8f + 0.2f * MathF.Sin(t * MathHelper.Pi * 6f);
                    sb.Draw(srb, drawPos, null,
                        ClairDeLunePalette.PearlFrost with { A = 0 } * 0.12f * pulse, 0f, srb.Size() * 0.5f,
                        DetonationRadius * 2f / srb.Width, SpriteEffects.None, 0f);
                    sb.Draw(srb, drawPos, null,
                        ClairDeLunePalette.PearlWhite with { A = 0 } * 0.3f * pulse, 0f, srb.Size() * 0.5f,
                        DetonationRadius * 0.4f / srb.Width, SpriteEffects.None, 0f);
                    break;
                }
                case OverflowPhase.Detonation:
                {
                    float t = (float)(_timer - TriggerDuration - FreezeDuration) / DetonationDuration;
                    float fadeOut = 1f - t;
                    for (int r = 0; r < 3; r++)
                    {
                        float ringOffset = r * 0.15f;
                        float ringT = MathHelper.Clamp(t - ringOffset, 0f, 1f);
                        float ringScale = DetonationRadius * 2f * ringT / srb.Width;
                        Color ringColor = r switch
                        {
                            0 => ClairDeLunePalette.PearlWhite,
                            1 => ClairDeLunePalette.SoftBlue,
                            _ => ClairDeLunePalette.NightMist
                        };
                        sb.Draw(srb, drawPos, null,
                            ringColor with { A = 0 } * (0.35f * fadeOut / (r + 1)), 0f, srb.Size() * 0.5f,
                            ringScale, SpriteEffects.None, 0f);
                    }
                    sb.Draw(srb, drawPos, null,
                        ClairDeLunePalette.ClockworkBrass with { A = 0 } * 0.5f * fadeOut * fadeOut, 0f,
                        srb.Size() * 0.5f, 30f / srb.Width, SpriteEffects.None, 0f);
                    sb.Draw(sf, drawPos, null,
                        ClairDeLunePalette.PearlWhite with { A = 0 } * 0.3f * fadeOut,
                        Main.GlobalTimeWrappedHourly * 2f, sf.Size() * 0.5f, 20f / sf.Width, SpriteEffects.None, 0f);
                    break;
                }
                case OverflowPhase.Resume:
                {
                    float t = (float)(_timer - TriggerDuration - FreezeDuration - DetonationDuration) / ResumeDuration;
                    float fadeOut = (1f - t) * (1f - t);
                    sb.Draw(srb, drawPos, null,
                        ClairDeLunePalette.NightMist with { A = 0 } * 0.15f * fadeOut, 0f, srb.Size() * 0.5f,
                        DetonationRadius * 2f * (1f - fadeOut * 0.5f) / srb.Width, SpriteEffects.None, 0f);
                    break;
                }
            }

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null, matrix);
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            if (CurrentPhase != OverflowPhase.Detonation) return false;
            float dist = Vector2.Distance(Projectile.Center, targetHitbox.Center.ToVector2());
            return dist < DetonationRadius;
        }
    }
}
