using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.Shaders;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.VFX;

namespace MagnumOpus.Content.ClairDeLune.Weapons.AutomatonsTuningFork.Projectiles
{
    /// <summary>
    /// Frequency Zone - shader-driven persistent area effect with one of 4 frequencies.
    /// 3 render passes: (1) RadialNoiseMaskShader zone body, (2) ResonanceFieldPulse standing wave overlay,
    /// (3) Bloom stacking for harmonic nodes + center note core.
    /// ai[0] = frequency type (0=A Attack, 1=C Defense, 2=E Speed, 3=G Damage).
    /// Duration: 5s. Checks for Perfect Resonance (2+ overlapping zone frequencies).
    /// </summary>
    public class FrequencyZoneProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Textures/InvisibleProjectile";

        private const int Duration = 300;
        private const float Radius = 128f;
        private bool _resonanceTriggered;

        private int FrequencyType => (int)Projectile.ai[0];

        private static readonly Color[] FreqColors =
        {
            ClairDeLunePalette.MoonbeamGold,
            ClairDeLunePalette.NightMist,
            ClairDeLunePalette.SoftBlue,
            ClairDeLunePalette.PearlBlue
        };

        // --- Shader + texture caching ---
        private static Effect _radialNoiseShader;
        private static Effect _resonanceShader;
        private static Asset<Texture2D> _softCircle;
        private static Asset<Texture2D> _softRadialBloom;
        private static Asset<Texture2D> _pointBloom;
        private static Asset<Texture2D> _noiseTex;
        private static Asset<Texture2D> _gradientLUT;

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.penetrate = -1;
            Projectile.timeLeft = Duration;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
        }

        public override void AI()
        {
            Projectile.velocity = Vector2.Zero;

            float life = 1f - (Projectile.timeLeft / (float)Duration);
            float fadeIn = Math.Min(life * 5f, 1f);
            float fadeOut = Projectile.timeLeft < 30 ? Projectile.timeLeft / 30f : 1f;
            float alpha = fadeIn * fadeOut;

            Player owner = Main.player[Projectile.owner];

            switch (FrequencyType)
            {
                case 0:
                    for (int i = 0; i < Main.maxNPCs; i++)
                    {
                        NPC npc = Main.npc[i];
                        if (!npc.active || npc.friendly || npc.dontTakeDamage) continue;
                        if (Vector2.Distance(Projectile.Center, npc.Center) <= Radius)
                            npc.AddBuff(BuffID.Ichor, 30);
                    }
                    break;
                case 1:
                    for (int i = 0; i < Main.maxPlayers; i++)
                    {
                        Player p = Main.player[i];
                        if (!p.active || p.dead) continue;
                        if (Vector2.Distance(Projectile.Center, p.Center) <= Radius)
                            p.statDefense += 10;
                    }
                    break;
                case 2:
                    for (int i = 0; i < Main.maxPlayers; i++)
                    {
                        Player p = Main.player[i];
                        if (!p.active || p.dead) continue;
                        if (Vector2.Distance(Projectile.Center, p.Center) <= Radius)
                        {
                            p.moveSpeed += 0.15f;
                            p.maxRunSpeed *= 1.15f;
                        }
                    }
                    break;
                case 3:
                    break;
            }

            // Perfect Resonance check
            if (!_resonanceTriggered)
            {
                bool[] freqPresent = new bool[4];
                freqPresent[FrequencyType] = true;

                for (int i = 0; i < Main.maxProjectiles; i++)
                {
                    Projectile other = Main.projectile[i];
                    if (!other.active || other.whoAmI == Projectile.whoAmI) continue;
                    if (other.type != Projectile.type || other.owner != Projectile.owner) continue;

                    float dist = Vector2.Distance(Projectile.Center, other.Center);
                    if (dist < Radius * 2f)
                    {
                        int otherFreq = (int)other.ai[0];
                        if (otherFreq >= 0 && otherFreq < 4)
                            freqPresent[otherFreq] = true;
                    }
                }

                int count = 0;
                for (int i = 0; i < 4; i++)
                    if (freqPresent[i]) count++;

                if (count >= 2)
                {
                    _resonanceTriggered = true;

                    Vector2 resonancePos = Projectile.Center;
                    int resDmg = (int)(Projectile.damage * 2f);
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), resonancePos, Vector2.Zero,
                        ModContent.ProjectileType<PerfectResonanceProjectile>(),
                        resDmg, 6f, Projectile.owner, count);

                    for (int i = 0; i < 4; i++)
                    {
                        if (!freqPresent[i]) continue;
                        float angle = i * MathHelper.PiOver2;
                        Vector2 vel = angle.ToRotationVector2() * 4f;
                        var flash = new BloomParticle(resonancePos, vel,
                            FreqColors[i] with { A = 0 } * 0.5f, 0.25f, 18);
                        MagnumParticleHandler.SpawnParticle(flash);
                    }
                }
            }

            // Frequency-colored zone particles with standing wave motion
            if (Main.rand.NextBool(4))
            {
                Color color = FreqColors[Math.Clamp(FrequencyType, 0, 3)];
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                float dist = Main.rand.NextFloat(Radius * 0.3f, Radius * 0.8f);
                Vector2 pos = Projectile.Center + angle.ToRotationVector2() * dist;

                float waveSpeed = 0.5f + FrequencyType * 0.2f;
                Vector2 vel = new Vector2(MathF.Cos(angle + MathHelper.PiOver2), MathF.Sin(angle + MathHelper.PiOver2)) * waveSpeed;

                var glow = new GenericGlowParticle(pos, vel,
                    color with { A = 0 } * 0.25f * alpha, 0.06f, 22);
                MagnumParticleHandler.SpawnParticle(glow);
            }

            Color zoneColor = FreqColors[Math.Clamp(FrequencyType, 0, 3)];
            Lighting.AddLight(Projectile.Center, zoneColor.ToVector3() * 0.15f * alpha);
        }

        private void LoadTextures()
        {
            _softCircle ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/MasksAndShapes/SoftCircle", AssetRequestMode.ImmediateLoad);
            _softRadialBloom ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftRadialBloom", AssetRequestMode.ImmediateLoad);
            _pointBloom ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/PointBloom", AssetRequestMode.ImmediateLoad);
            _noiseTex ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/NoiseTextures/PerlinNoise", AssetRequestMode.ImmediateLoad);
            _gradientLUT ??= ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/ColorGradients/ClairDeLuneGradientLUTandRAMP", AssetRequestMode.ImmediateLoad);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (Main.dedServ) return false;
            LoadTextures();

            SpriteBatch sb = Main.spriteBatch;
            Matrix matrix = Main.GameViewMatrix.TransformationMatrix;

            float life = 1f - (Projectile.timeLeft / (float)Duration);
            float fadeIn = Math.Min(life * 5f, 1f);
            float fadeOut = Projectile.timeLeft < 30 ? Projectile.timeLeft / 30f : 1f;
            float alpha = fadeIn * fadeOut;

            Color color = FreqColors[Math.Clamp(FrequencyType, 0, 3)];
            float waveFreq = 0.04f + FrequencyType * 0.015f;
            float wavePulse = 0.85f + 0.15f * MathF.Sin(Main.GameUpdateCount * waveFreq);

            DrawRadialNoiseZone(sb, matrix, alpha, color);              // Pass 1
            DrawResonanceStandingWave(sb, matrix, alpha, color);        // Pass 2
            DrawBloomComposite(sb, matrix, alpha, color, wavePulse);    // Pass 3
            return false;
        }

        // ---- PASS 1: RadialNoiseMaskShader zone body ----
        private void DrawRadialNoiseZone(SpriteBatch sb, Matrix matrix, float alpha, Color freqColor)
        {
            _radialNoiseShader ??= ModContent.Request<Effect>(
                "MagnumOpus/Content/FoundationWeapons/MaskFoundation/Shaders/RadialNoiseMaskShader",
                AssetRequestMode.ImmediateLoad).Value;
            if (_radialNoiseShader == null) return;

            sb.End();

            // Frequency-dependent scroll and rotation speeds
            float scrollBase = 0.3f + FrequencyType * 0.1f;
            float rotBase = 0.2f + FrequencyType * 0.08f;

            _radialNoiseShader.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly);
            _radialNoiseShader.Parameters["scrollSpeed"]?.SetValue(scrollBase);
            _radialNoiseShader.Parameters["rotationSpeed"]?.SetValue(rotBase);
            _radialNoiseShader.Parameters["circleRadius"]?.SetValue(0.44f);
            _radialNoiseShader.Parameters["edgeSoftness"]?.SetValue(0.1f);
            _radialNoiseShader.Parameters["intensity"]?.SetValue(alpha * 0.6f);
            _radialNoiseShader.Parameters["primaryColor"]?.SetValue(freqColor.ToVector3());
            _radialNoiseShader.Parameters["coreColor"]?.SetValue(ClairDeLunePalette.PearlWhite.ToVector3());
            _radialNoiseShader.Parameters["noiseTex"]?.SetValue(_noiseTex.Value);
            _radialNoiseShader.Parameters["gradientTex"]?.SetValue(_gradientLUT.Value);

            _radialNoiseShader.CurrentTechnique = _radialNoiseShader.Techniques["Technique1"];

            sb.Begin(SpriteSortMode.Immediate, MagnumBlendStates.ShaderAdditive,
                SamplerState.LinearWrap, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, _radialNoiseShader, matrix);

            Texture2D sc = _softCircle.Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float diam = Radius * 2f;
            sb.Draw(sc, drawPos, null, Color.White, 0f, sc.Size() * 0.5f,
                diam / sc.Width, SpriteEffects.None, 0f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                SamplerState.LinearClamp, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null, matrix);
        }

        // ---- PASS 2: ResonanceFieldPulse concentric standing wave harmonic overlay ----
        private void DrawResonanceStandingWave(SpriteBatch sb, Matrix matrix, float alpha, Color freqColor)
        {
            _resonanceShader ??= ShaderLoader.ResonanceField;
            if (_resonanceShader == null) return;

            sb.End();

            _resonanceShader.Parameters["uColor"]?.SetValue(freqColor.ToVector4());
            _resonanceShader.Parameters["uSecondaryColor"]?.SetValue(ClairDeLunePalette.PearlWhite.ToVector4());
            _resonanceShader.Parameters["uOpacity"]?.SetValue(alpha * 0.35f);
            _resonanceShader.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly);
            _resonanceShader.Parameters["uIntensity"]?.SetValue(1.0f);
            _resonanceShader.Parameters["uOverbrightMult"]?.SetValue(1.0f);
            // Frequency-dependent scroll speed for different standing wave frequencies
            _resonanceShader.Parameters["uScrollSpeed"]?.SetValue(1.5f + FrequencyType * 0.5f);
            _resonanceShader.Parameters["uDistortionAmt"]?.SetValue(0.01f);
            _resonanceShader.Parameters["uHasSecondaryTex"]?.SetValue(_noiseTex != null);

            if (_noiseTex != null)
            {
                Main.graphics.GraphicsDevice.Textures[1] = _noiseTex.Value;
                _resonanceShader.Parameters["uSecondaryTexScale"]?.SetValue(2.5f);
            }

            _resonanceShader.CurrentTechnique = _resonanceShader.Techniques["ResonanceFieldPulse"];

            sb.Begin(SpriteSortMode.Immediate, MagnumBlendStates.ShaderAdditive,
                SamplerState.LinearWrap, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, _resonanceShader, matrix);

            Texture2D sc = _softCircle.Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float diam = Radius * 2f;
            sb.Draw(sc, drawPos, null, Color.White, 0f, sc.Size() * 0.5f,
                diam / sc.Width, SpriteEffects.None, 0f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                SamplerState.LinearClamp, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null, matrix);
        }

        // ---- PASS 3: Bloom stacking (harmonic nodes, center note core) ----
        private void DrawBloomComposite(SpriteBatch sb, Matrix matrix, float alpha, Color freqColor, float wavePulse)
        {
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Texture2D srb = _softRadialBloom.Value;
            Texture2D pb = _pointBloom.Value;
            float fieldScale = Radius * 2f;

            // Standing wave harmonic node ring blooms
            int harmonics = FrequencyType + 2; // A=2, C=3, E=4, G=5
            float waveFreq = 0.04f + FrequencyType * 0.015f;

            for (int h = 1; h <= harmonics; h++)
            {
                float nodeRadius = Radius * h / (harmonics + 1f);
                float nodeAlpha = 0.04f * (1f - h / (float)(harmonics + 1)) * alpha;
                float nodeWave = 0.8f + 0.2f * MathF.Sin(Main.GameUpdateCount * waveFreq * h);

                // Draw ring of bloom dots at each harmonic radius
                int dots = 6 + h * 2;
                for (int d = 0; d < dots; d++)
                {
                    float angle = MathHelper.TwoPi * d / dots + Main.GameUpdateCount * 0.01f * h;
                    Vector2 dotPos = drawPos + angle.ToRotationVector2() * nodeRadius;
                    sb.Draw(pb, dotPos, null, freqColor with { A = 0 } * nodeAlpha * nodeWave,
                        0f, pb.Size() * 0.5f, 4f / pb.Width, SpriteEffects.None, 0f);
                }
            }

            // Zone ambient halo
            sb.Draw(srb, drawPos, null, freqColor with { A = 0 } * 0.08f * alpha * wavePulse,
                0f, srb.Size() * 0.5f, fieldScale / srb.Width, SpriteEffects.None, 0f);

            // Inner frequency glow
            sb.Draw(srb, drawPos, null, freqColor with { A = 0 } * 0.12f * alpha,
                0f, srb.Size() * 0.5f, fieldScale * 0.4f / srb.Width, SpriteEffects.None, 0f);

            // Center note core
            sb.Draw(pb, drawPos, null, ClairDeLunePalette.PearlWhite with { A = 0 } * 0.2f * alpha,
                0f, pb.Size() * 0.5f, 8f / pb.Width, SpriteEffects.None, 0f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullNone, null, matrix);
        }
    }
}
