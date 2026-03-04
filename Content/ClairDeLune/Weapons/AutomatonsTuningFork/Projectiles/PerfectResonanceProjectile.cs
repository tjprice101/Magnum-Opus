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

namespace MagnumOpus.Content.ClairDeLune.Weapons.AutomatonsTuningFork.Projectiles
{
    /// <summary>
    /// Perfect Resonance — Triggered when 2+ frequency zones overlap.
    /// VoronoiCell-style fractal burst dealing 2x damage in AoE.
    /// ai[0] = number of overlapping frequencies (2-4).
    /// 3 render passes: (1) ResonanceField ResonanceFieldHarmonic burst,
    /// (2) RadialNoiseMaskShader fractal zone, (3) Multi-scale bloom + frequency facets.
    /// </summary>
    public class PerfectResonanceProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Textures/InvisibleProjectile";

        private int _timer;
        private const int LifeTime = 40;

        private int FrequencyCount => Math.Max(2, (int)Projectile.ai[0]);
        private float Radius => 64f + (FrequencyCount - 2) * 32f; // 64-128px

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
            Projectile.timeLeft = LifeTime;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = LifeTime;
        }

        public override void AI()
        {
            Projectile.velocity = Vector2.Zero;
            _timer++;

            if (_timer == 1)
            {
                SoundEngine.PlaySound(SoundID.Item29 with {
                    Pitch = 0.1f * FrequencyCount, Volume = 0.6f + FrequencyCount * 0.1f },
                    Projectile.Center);

                // AoE damage
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (!npc.active || npc.friendly || npc.dontTakeDamage) continue;
                    if (Vector2.Distance(Projectile.Center, npc.Center) <= Radius)
                    {
                        Player owner = Main.player[Projectile.owner];
                        owner.ApplyDamageToNPC(npc, Projectile.damage, Projectile.knockBack, 0, false);
                    }
                }

                // Multi-frequency burst particles
                for (int f = 0; f < FrequencyCount; f++)
                {
                    Color color = FreqColors[f % 4];
                    float angleOffset = f * MathHelper.TwoPi / FrequencyCount;
                    for (int p = 0; p < 8; p++)
                    {
                        float angle = angleOffset + p * MathHelper.TwoPi / 8f;
                        float speed = 3f + f * 1.5f;
                        Vector2 vel = angle.ToRotationVector2() * speed;
                        var burst = new BloomParticle(Projectile.Center, vel,
                            color with { A = 0 } * 0.5f, 0.1f + f * 0.03f, 18);
                        MagnumParticleHandler.SpawnParticle(burst);
                    }
                }

                // Central resonance flash
                var coreFlash = new BloomParticle(Projectile.Center, Vector2.Zero,
                    ClairDeLunePalette.WhiteHot with { A = 0 } * 0.6f,
                    0.35f + FrequencyCount * 0.08f, 10);
                MagnumParticleHandler.SpawnParticle(coreFlash);

                // Crystalline shatter sparks
                for (int s = 0; s < 10 + FrequencyCount * 3; s++)
                {
                    float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                    float speed = Main.rand.NextFloat(2f, 5f);
                    Vector2 vel = angle.ToRotationVector2() * speed;
                    Color sparkCol = FreqColors[Main.rand.Next(FrequencyCount) % 4];
                    var spark = new SparkleParticle(Projectile.Center, vel,
                        sparkCol with { A = 0 } * 0.5f, 0.06f, 14);
                    MagnumParticleHandler.SpawnParticle(spark);
                }
            }

            Lighting.AddLight(Projectile.Center, ClairDeLunePalette.PearlWhite.ToVector3() *
                0.4f * (1f - _timer / (float)LifeTime));
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
            Matrix matrix = Main.GameViewMatrix.TransformationMatrix;

            float progress = _timer / (float)LifeTime;
            float fadeOut = 1f - progress;

            DrawResonanceBurst(sb, matrix, progress, fadeOut);   // Pass 1: ResonanceFieldHarmonic
            DrawRadialZone(sb, matrix, progress, fadeOut);       // Pass 2: RadialNoiseMask fractal zone
            DrawBloomFacets(sb, matrix, progress, fadeOut);      // Pass 3: Bloom + frequency facets
            return false;
        }

        // ---- PASS 1: ResonanceField ResonanceFieldHarmonic burst ----
        private void DrawResonanceBurst(SpriteBatch sb, Matrix matrix, float progress, float fadeOut)
        {
            _resonanceShader ??= ShaderLoader.ResonanceField;
            if (_resonanceShader == null) return;

            sb.End();

            _resonanceShader.Parameters["uColor"]?.SetValue(ClairDeLunePalette.MoonbeamGold.ToVector4());
            _resonanceShader.Parameters["uSecondaryColor"]?.SetValue(ClairDeLunePalette.PearlFrost.ToVector4());
            _resonanceShader.Parameters["uOpacity"]?.SetValue(0.6f * fadeOut);
            _resonanceShader.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly * 2f);
            _resonanceShader.Parameters["uIntensity"]?.SetValue(1.5f + FrequencyCount * 0.3f);
            _resonanceShader.Parameters["uOverbrightMult"]?.SetValue(1.2f);
            _resonanceShader.Parameters["uScrollSpeed"]?.SetValue(3f + FrequencyCount);
            _resonanceShader.Parameters["uDistortionAmt"]?.SetValue(0.03f);
            _resonanceShader.Parameters["uHasSecondaryTex"]?.SetValue(false);

            _resonanceShader.CurrentTechnique = _resonanceShader.Techniques["ResonanceFieldHarmonic"];

            sb.Begin(SpriteSortMode.Immediate, BlendState.Additive,
                SamplerState.LinearWrap, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, _resonanceShader, matrix);

            Texture2D sc = _softCircle.Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float burstScale = Radius * MathF.Min(progress * 3f, 1f) * 2f / sc.Width;
            sb.Draw(sc, drawPos, null, Color.White, 0f, sc.Size() * 0.5f, burstScale, SpriteEffects.None, 0f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive,
                SamplerState.LinearClamp, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null, matrix);
        }

        // ---- PASS 2: RadialNoiseMask fractal zone ----
        private void DrawRadialZone(SpriteBatch sb, Matrix matrix, float progress, float fadeOut)
        {
            if (progress < 0.1f) return; // Wait for initial burst

            _radialNoiseShader ??= ModContent.Request<Effect>(
                "MagnumOpus/Content/FoundationWeapons/MaskFoundation/Shaders/RadialNoiseMaskShader",
                AssetRequestMode.ImmediateLoad).Value;
            if (_radialNoiseShader == null) return;

            sb.End();

            _radialNoiseShader.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly * 3f);
            _radialNoiseShader.Parameters["scrollSpeed"]?.SetValue(4f);
            _radialNoiseShader.Parameters["rotationSpeed"]?.SetValue(2f + FrequencyCount * 0.5f);
            _radialNoiseShader.Parameters["circleRadius"]?.SetValue(0.42f);
            _radialNoiseShader.Parameters["edgeSoftness"]?.SetValue(0.12f);
            _radialNoiseShader.Parameters["intensity"]?.SetValue(0.8f * fadeOut);
            _radialNoiseShader.Parameters["primaryColor"]?.SetValue(ClairDeLunePalette.SoftBlue.ToVector3());
            _radialNoiseShader.Parameters["coreColor"]?.SetValue(ClairDeLunePalette.MoonbeamGold.ToVector3());

            Main.graphics.GraphicsDevice.Textures[1] = _noiseTex.Value;
            Main.graphics.GraphicsDevice.Textures[2] = _gradientLUT.Value;

            _radialNoiseShader.CurrentTechnique = _radialNoiseShader.Techniques["Technique1"];

            sb.Begin(SpriteSortMode.Immediate, BlendState.Additive,
                SamplerState.LinearWrap, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, _radialNoiseShader, matrix);

            Texture2D sc = _softCircle.Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float zoneScale = Radius * 2.2f / sc.Width;
            sb.Draw(sc, drawPos, null, Color.White, 0f, sc.Size() * 0.5f, zoneScale, SpriteEffects.None, 0f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive,
                SamplerState.LinearClamp, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null, matrix);
        }

        // ---- PASS 3: Multi-scale bloom + frequency facets ----
        private void DrawBloomFacets(SpriteBatch sb, Matrix matrix, float progress, float fadeOut)
        {
            Vector2 pos = Projectile.Center - Main.screenPosition;
            Texture2D srb = _softRadialBloom.Value;
            Texture2D pb = _pointBloom.Value;
            Texture2D sf = _starFlare.Value;

            // Expanding multi-frequency rings
            for (int f = 0; f < FrequencyCount; f++)
            {
                Color color = FreqColors[f % 4];
                float ringProgress = MathF.Min(progress * (1.5f + f * 0.3f), 1f);
                float ringRadius = Radius * ringProgress;
                float ringAlpha = (1f - ringProgress) * 0.2f * fadeOut;

                sb.Draw(srb, pos, null, color with { A = 0 } * ringAlpha,
                    f * 0.5f, srb.Size() * 0.5f, ringRadius * 2f / srb.Width, SpriteEffects.None, 0f);
            }

            // Voronoi-like facet dots
            float cellAngle = Main.GlobalTimeWrappedHourly;
            int facets = FrequencyCount * 3;
            for (int i = 0; i < facets; i++)
            {
                float angle = cellAngle + i * MathHelper.TwoPi / facets;
                float dist = Radius * 0.4f * fadeOut;
                Vector2 facetPos = pos + angle.ToRotationVector2() * dist;
                Color facetColor = FreqColors[i % FrequencyCount % 4];

                sb.Draw(pb, facetPos, null, facetColor with { A = 0 } * 0.1f * fadeOut,
                    0f, pb.Size() * 0.5f, 4f / pb.Width, SpriteEffects.None, 0f);
            }

            // Central glow
            sb.Draw(pb, pos, null,
                ClairDeLunePalette.WhiteHot with { A = 0 } * 0.25f * fadeOut,
                0f, pb.Size() * 0.5f, 12f / pb.Width, SpriteEffects.None, 0f);

            // Star flare
            if (progress < 0.5f)
            {
                float flareAlpha = (1f - progress * 2f) * 0.2f;
                sb.Draw(sf, pos, null,
                    ClairDeLunePalette.MoonbeamGold with { A = 0 } * flareAlpha,
                    Main.GlobalTimeWrappedHourly * 4f, sf.Size() * 0.5f,
                    Radius * 0.3f / sf.Width, SpriteEffects.None, 0f);
            }

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullCounterClockwise, null, matrix);
        }
    }
}
