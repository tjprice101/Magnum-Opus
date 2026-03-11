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
using MagnumOpus.Common.Systems.VFX;

namespace MagnumOpus.Content.ClairDeLune.Weapons.AutomatonsTuningFork.Projectiles
{
    /// <summary>
    /// Conductor's Final Note - shader-driven 4-phase cascade + combined harmonic finale.
    /// 3 render passes: (1) ResonanceFieldHarmonic phase cascade rings,
    /// (2) RadialNoiseMaskShader finale explosion zone, (3) Bloom stacking for phase echoes + core.
    /// 4-phase cascade (A/C/E/G) then combined harmonic finale with AoE damage.
    /// </summary>
    public class ConductorFinalNoteProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Textures/InvisibleProjectile";

        private int _timer;
        private int _phase;
        private const int PhaseInterval = 10;
        private const float FinalRadius = 240f;
        private const int TotalDuration = PhaseInterval * 4 + 30;

        private static readonly Color[] FreqColors =
        {
            ClairDeLunePalette.MoonbeamGold,
            ClairDeLunePalette.NightMist,
            ClairDeLunePalette.SoftBlue,
            ClairDeLunePalette.PearlBlue
        };

        // --- Shader + texture caching ---
        private static Effect _resonanceShader;
        private static Effect _radialNoiseShader;
        private static Asset<Texture2D> _softCircle;
        private static Asset<Texture2D> _softRadialBloom;
        private static Asset<Texture2D> _pointBloom;
        private static Asset<Texture2D> _starFlare;
        private static Asset<Texture2D> _noiseTex;
        private static Asset<Texture2D> _gradientLUT;

        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.penetrate = -1;
            Projectile.timeLeft = TotalDuration;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 15;
        }

        public override void AI()
        {
            Projectile.velocity = Vector2.Zero;
            _timer++;

            int newPhase = Math.Min(_timer / PhaseInterval, 4);

            if (newPhase != _phase && newPhase <= 4)
            {
                _phase = newPhase;

                if (_phase < 4)
                {
                    float phaseRadius = FinalRadius * (_phase + 1) / 5f;
                    Color phaseColor = FreqColors[_phase];

                    SoundEngine.PlaySound(SoundID.Item4 with
                    {
                        Pitch = -0.2f + _phase * 0.2f,
                        Volume = 0.4f + _phase * 0.1f
                    }, Projectile.Center);

                    DealPhaseDamage(phaseRadius, 0.5f + _phase * 0.15f);

                    int ringParticles = 10 + _phase * 3;
                    for (int i = 0; i < ringParticles; i++)
                    {
                        float angle = MathHelper.TwoPi * i / ringParticles;
                        float speed = 2f + _phase * 1.5f;
                        Vector2 vel = angle.ToRotationVector2() * speed;
                        var ring = new BloomParticle(Projectile.Center, vel,
                            phaseColor with { A = 0 } * 0.4f, 0.1f + _phase * 0.03f, 18);
                        MagnumParticleHandler.SpawnParticle(ring);
                    }
                }
                else
                {
                    // COMBINED FINALE
                    SoundEngine.PlaySound(SoundID.Item29 with { Pitch = -0.3f, Volume = 1f }, Projectile.Center);
                    SoundEngine.PlaySound(SoundID.Item14 with { Pitch = 0.3f, Volume = 0.6f }, Projectile.Center);

                    DealPhaseDamage(FinalRadius, 1.5f);

                    for (int f = 0; f < 4; f++)
                    {
                        float freqAngleBase = f * MathHelper.PiOver2;
                        for (int p = 0; p < 12; p++)
                        {
                            float angle = freqAngleBase + (p - 6) * 0.12f;
                            float speed = Main.rand.NextFloat(4f, 8f);
                            Vector2 vel = angle.ToRotationVector2() * speed;
                            var burst = new BloomParticle(Projectile.Center, vel,
                                FreqColors[f] with { A = 0 } * 0.5f, 0.12f, 22);
                            MagnumParticleHandler.SpawnParticle(burst);
                        }
                    }

                    var whiteFlash = new BloomParticle(Projectile.Center, Vector2.Zero,
                        ClairDeLunePalette.WhiteHot with { A = 0 } * 0.8f, 0.8f, 15);
                    MagnumParticleHandler.SpawnParticle(whiteFlash);

                    for (int i = 0; i < 24; i++)
                    {
                        float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                        float speed = Main.rand.NextFloat(3f, 7f);
                        Vector2 vel = angle.ToRotationVector2() * speed;
                        Color sparkColor = FreqColors[Main.rand.Next(4)];
                        var sparkle = new SparkleParticle(Projectile.Center, vel,
                            sparkColor with { A = 0 } * 0.6f, 0.09f, 25);
                        MagnumParticleHandler.SpawnParticle(sparkle);
                    }
                }
            }

            float intensity = _phase < 4 ? _phase / 4f : 1f;
            Lighting.AddLight(Projectile.Center, ClairDeLunePalette.PearlWhite.ToVector3() *
                0.3f * intensity * (Projectile.timeLeft > 10 ? 1f : Projectile.timeLeft / 10f));
        }

        private void DealPhaseDamage(float radius, float multiplier)
        {
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly || npc.dontTakeDamage) continue;
                if (Vector2.Distance(Projectile.Center, npc.Center) <= radius)
                {
                    int dmg = (int)(Projectile.damage * multiplier);
                    Player owner = Main.player[Projectile.owner];
                    owner.ApplyDamageToNPC(npc, dmg, Projectile.knockBack, 0, false);
                }
            }
        }

        private void LoadTextures()
        {
            _softCircle ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/MasksAndShapes/SoftCircle", AssetRequestMode.ImmediateLoad);
            _softRadialBloom ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftRadialBloom", AssetRequestMode.ImmediateLoad);
            _pointBloom ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/PointBloom", AssetRequestMode.ImmediateLoad);
            _starFlare ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/StarFlare", AssetRequestMode.ImmediateLoad);
            _noiseTex ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/NoiseTextures/PerlinNoise", AssetRequestMode.ImmediateLoad);
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
            float fadeOut = Projectile.timeLeft < 20 ? Projectile.timeLeft / 20f : 1f;

            DrawResonanceCascadeRings(sb, matrix, fadeOut); // Pass 1
            DrawFinaleNoiseZone(sb, matrix, fadeOut);       // Pass 2
            DrawBloomComposite(sb, matrix, fadeOut);        // Pass 3
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

        // ---- PASS 1: ResonanceFieldHarmonic expanding cascade rings per phase ----
        private void DrawResonanceCascadeRings(SpriteBatch sb, Matrix matrix, float fadeOut)
        {
            _resonanceShader ??= ShaderLoader.ResonanceField;
            if (_resonanceShader == null) return;

            sb.End();

            for (int p = 0; p <= Math.Min(_phase, 3); p++)
            {
                float phaseElapsed = (_timer - p * PhaseInterval) / (float)(TotalDuration - p * PhaseInterval);
                float ringRadius = FinalRadius * (p + 1) / 5f * Math.Min(phaseElapsed * 3f, 1f);
                float ringAlpha = (1f - phaseElapsed * 0.5f) * fadeOut;

                if (ringAlpha <= 0.01f || ringRadius < 4f) continue;

                _resonanceShader.Parameters["uColor"]?.SetValue(FreqColors[p].ToVector4());
                _resonanceShader.Parameters["uSecondaryColor"]?.SetValue(ClairDeLunePalette.PearlWhite.ToVector4());
                _resonanceShader.Parameters["uOpacity"]?.SetValue(ringAlpha * 0.35f);
                _resonanceShader.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly + p * 0.5f);
                _resonanceShader.Parameters["uIntensity"]?.SetValue(1.0f + p * 0.15f);
                _resonanceShader.Parameters["uOverbrightMult"]?.SetValue(1.2f);
                _resonanceShader.Parameters["uScrollSpeed"]?.SetValue(2.0f + p * 0.5f);
                _resonanceShader.Parameters["uDistortionAmt"]?.SetValue(0.01f);
                _resonanceShader.Parameters["uHasSecondaryTex"]?.SetValue(false);

                _resonanceShader.CurrentTechnique = _resonanceShader.Techniques["ResonanceFieldHarmonic"];

                sb.Begin(SpriteSortMode.Immediate, MagnumBlendStates.ShaderAdditive,
                    SamplerState.LinearWrap, DepthStencilState.None,
                    RasterizerState.CullCounterClockwise, _resonanceShader, matrix);

                Texture2D sc = _softCircle.Value;
                Vector2 drawPos = Projectile.Center - Main.screenPosition;
                float diam = ringRadius * 2f;
                sb.Draw(sc, drawPos, null, Color.White, p * 0.3f, sc.Size() * 0.5f,
                    diam / sc.Width, SpriteEffects.None, 0f);

                sb.End();
            }

            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                SamplerState.LinearClamp, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null, matrix);
        }

        // ---- PASS 2: RadialNoiseMaskShader for the finale combined explosion ----
        private void DrawFinaleNoiseZone(SpriteBatch sb, Matrix matrix, float fadeOut)
        {
            if (_phase < 4) return;

            _radialNoiseShader ??= ModContent.Request<Effect>(
                "MagnumOpus/Content/FoundationWeapons/MaskFoundation/Shaders/RadialNoiseMaskShader",
                AssetRequestMode.ImmediateLoad).Value;
            if (_radialNoiseShader == null) return;

            float finaleProgress = (_timer - 4 * PhaseInterval) / 30f;
            float finaleRadius = FinalRadius * Math.Min(finaleProgress * 2f, 1f);
            float finaleAlpha = (1f - finaleProgress) * fadeOut;

            if (finaleAlpha <= 0.01f) return;

            sb.End();

            _radialNoiseShader.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly);
            _radialNoiseShader.Parameters["scrollSpeed"]?.SetValue(0.8f);
            _radialNoiseShader.Parameters["rotationSpeed"]?.SetValue(0.6f);
            _radialNoiseShader.Parameters["circleRadius"]?.SetValue(0.42f);
            _radialNoiseShader.Parameters["edgeSoftness"]?.SetValue(0.15f);
            _radialNoiseShader.Parameters["intensity"]?.SetValue(finaleAlpha * 0.7f);
            _radialNoiseShader.Parameters["primaryColor"]?.SetValue(ClairDeLunePalette.PearlWhite.ToVector3());
            _radialNoiseShader.Parameters["coreColor"]?.SetValue(ClairDeLunePalette.MoonbeamGold.ToVector3());
            _radialNoiseShader.Parameters["noiseTex"]?.SetValue(_noiseTex.Value);
            _radialNoiseShader.Parameters["gradientTex"]?.SetValue(_gradientLUT.Value);

            _radialNoiseShader.CurrentTechnique = _radialNoiseShader.Techniques["Technique1"];

            sb.Begin(SpriteSortMode.Immediate, MagnumBlendStates.ShaderAdditive,
                SamplerState.LinearWrap, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, _radialNoiseShader, matrix);

            Texture2D sc = _softCircle.Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float diam = finaleRadius * 2f;
            sb.Draw(sc, drawPos, null, Color.White, 0f, sc.Size() * 0.5f,
                diam / sc.Width, SpriteEffects.None, 0f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                SamplerState.LinearClamp, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null, matrix);
        }

        // ---- PASS 3: Bloom composite (phase ring echoes, finale halo, central core) ----
        private void DrawBloomComposite(SpriteBatch sb, Matrix matrix, float fadeOut)
        {
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Texture2D srb = _softRadialBloom.Value;
            Texture2D pb = _pointBloom.Value;
            Texture2D sf = _starFlare.Value;

            // Completed phase ring bloom echoes
            for (int p = 0; p <= Math.Min(_phase, 3); p++)
            {
                float phaseElapsed = (_timer - p * PhaseInterval) / (float)(TotalDuration - p * PhaseInterval);
                float ringRadius = FinalRadius * (p + 1) / 5f * Math.Min(phaseElapsed * 3f, 1f);
                float ringAlpha = 0.06f * (1f - phaseElapsed * 0.5f) * fadeOut;

                if (ringAlpha > 0.01f)
                {
                    sb.Draw(srb, drawPos, null, FreqColors[p] with { A = 0 } * ringAlpha,
                        0f, srb.Size() * 0.5f, ringRadius * 2f / srb.Width, SpriteEffects.None, 0f);
                }
            }

            // Finale combined multi-color rotating halo
            if (_phase >= 4)
            {
                float finaleProgress = (_timer - 4 * PhaseInterval) / 30f;
                float finaleAlpha = (1f - finaleProgress) * 0.1f * fadeOut;

                for (int f = 0; f < 4; f++)
                {
                    float rotOffset = Main.GameUpdateCount * 0.05f + f * MathHelper.PiOver2;
                    Vector2 rotPos = drawPos + new Vector2(MathF.Cos(rotOffset), MathF.Sin(rotOffset)) * 6f;
                    sb.Draw(sf, rotPos, null, FreqColors[f] with { A = 0 } * finaleAlpha,
                        rotOffset, sf.Size() * 0.5f, 24f / sf.Width, SpriteEffects.None, 0f);
                }
            }

            // Central conductor glow
            float centerIntensity = Math.Min(_phase / 4f, 1f);
            sb.Draw(srb, drawPos, null, ClairDeLunePalette.PearlWhite with { A = 0 } * 0.15f * centerIntensity * fadeOut,
                0f, srb.Size() * 0.5f, 24f / srb.Width, SpriteEffects.None, 0f);
            sb.Draw(pb, drawPos, null, ClairDeLunePalette.WhiteHot with { A = 0 } * 0.3f * centerIntensity * fadeOut,
                0f, pb.Size() * 0.5f, 8f / pb.Width, SpriteEffects.None, 0f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullNone, null, matrix);
        }
    }
}
