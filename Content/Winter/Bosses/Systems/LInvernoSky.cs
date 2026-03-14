using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.Graphics.Effects;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.Bosses;
using MagnumOpus.Common.Systems.Shaders;

namespace MagnumOpus.Content.Winter.Bosses.Systems
{
    /// <summary>
    /// Phase-aware custom sky for L'Inverno boss fight.
    /// Phase 1 (100-70%): First Frost — gentle snow, crystalline shimmer
    /// Phase 2 (70-40%): Frozen Expanse — heavy snow, oppressive cold
    /// Phase 3 (40-15%): Blizzard — whiteout, wind-driven chaos
    /// Phase 4 (&lt;15%/Enrage): Absolute Zero — frozen stillness, frost creep
    /// </summary>
    public class LInvernoSky : CustomSky
    {
        private bool _isActive;
        private float _opacity;
        private float _intensity;

        // Boss state (fed from LInverno.cs)
        public static float BossLifeRatio = 1f;
        public static Vector2 BossCenter = Vector2.Zero;
        public static bool BossIsEnraged = false;

        // Phase colors
        private static readonly Color IceBlue = new Color(168, 216, 234);
        private static readonly Color FrostWhite = new Color(232, 244, 248);
        private static readonly Color DeepGlacialBlue = new Color(27, 79, 114);
        private static readonly Color CrystalCyan = new Color(0, 229, 255);
        private static readonly Color BlizzardWhite = new Color(240, 248, 255);
        private static readonly Color GlacialPurple = new Color(123, 104, 174);
        private static readonly Color PaleSilverBlue = new Color(190, 210, 230);

        /// <summary>
        /// Returns VFX phase 1-4 based on boss HP ratio.
        /// </summary>
        public static int GetVFXPhase()
        {
            if (BossIsEnraged) return 4;
            if (BossLifeRatio > 0.7f) return 1;
            if (BossLifeRatio > 0.4f) return 2;
            if (BossLifeRatio > 0.15f) return 3;
            return 4;
        }

        #region Snowflakes

        private struct Snowflake
        {
            public Vector2 Position;
            public float FallSpeed;
            public float DriftPhase;
            public float DriftAmplitude;
            public float Scale;
            public float Alpha;
            public float Rotation;
            public float RotationSpeed;
            public bool IsFrozen;
        }

        private Snowflake[] _snowflakes;
        private const int MaxSnowflakes = 300;

        #endregion

        #region Breath Crystals

        private struct BreathCrystal
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Scale;
            public float Alpha;
            public float Life;
            public float MaxLife;
            public float Rotation;
        }

        private BreathCrystal[] _breathCrystals;
        private const int MaxBreathCrystals = 40;
        private int _nextCrystalSlot;

        #endregion

        public override void OnLoad() { }

        public override void Activate(Vector2 position, params object[] args)
        {
            _isActive = true;
            _snowflakes = new Snowflake[MaxSnowflakes];
            _breathCrystals = new BreathCrystal[MaxBreathCrystals];
            _nextCrystalSlot = 0;
            Random rand = new Random();

            for (int i = 0; i < MaxSnowflakes; i++)
            {
                _snowflakes[i] = new Snowflake
                {
                    Position = new Vector2(
                        (float)rand.NextDouble() * Main.screenWidth,
                        (float)rand.NextDouble() * Main.screenHeight),
                    FallSpeed = 0.2f + (float)rand.NextDouble() * 2.0f,
                    DriftPhase = (float)rand.NextDouble() * MathHelper.TwoPi,
                    DriftAmplitude = 8f + (float)rand.NextDouble() * 30f,
                    Scale = 0.15f + (float)rand.NextDouble() * 0.85f,
                    Alpha = 0.3f + (float)rand.NextDouble() * 0.7f,
                    Rotation = (float)rand.NextDouble() * MathHelper.TwoPi,
                    RotationSpeed = (float)(rand.NextDouble() - 0.5) * 0.03f,
                    IsFrozen = false
                };
            }
        }

        public override void Deactivate(params object[] args)
        {
            _isActive = false;
        }

        public override void Reset()
        {
            _isActive = false;
            _opacity = 0f;
        }

        public override bool IsActive() => _isActive || _opacity > 0.001f;

        private float GetEffectiveIntensity()
        {
            if (Main.LocalPlayer == null) return _opacity;
            float dist = Vector2.Distance(Main.LocalPlayer.Center, BossCenter);
            float proximity = MathHelper.SmoothStep(1f, 0f, MathHelper.Clamp(dist / 3500f, 0f, 1f));
            return _opacity * proximity;
        }

        public override void Update(GameTime gameTime)
        {
            if (_isActive)
                _opacity = Math.Min(1f, _opacity + 0.02f);
            else
                _opacity = Math.Max(0f, _opacity - 0.02f);

            NPC boss = BossIndexTracker.GetActiveBoss(BossIndexTracker.LInverno);
            if (boss == null) return;

            float hpRatio = boss.life / (float)boss.lifeMax;
            _intensity = 1f - hpRatio;
            int phase = GetVFXPhase();

            // Update snowflakes with phase-scaled behavior
            if (_snowflakes != null)
            {
                float time = (float)Main.timeForVisualEffects;
                float speedMult, windStrength;
                int activeCount;

                switch (phase)
                {
                    case 1: // First Frost — gentle, crystalline
                        speedMult = 0.6f + _intensity * 0.4f;
                        windStrength = 0.1f + _intensity * 0.3f;
                        activeCount = 80;
                        break;
                    case 2: // Frozen Expanse — heavier snowfall
                        speedMult = 1.0f + _intensity * 0.6f;
                        windStrength = 0.5f + _intensity * 0.8f;
                        activeCount = 180;
                        break;
                    case 3: // Blizzard — storm
                        speedMult = 1.8f + _intensity * 1.2f;
                        windStrength = 2.0f + _intensity * 1.5f;
                        activeCount = 280;
                        break;
                    default: // Absolute Zero — frozen stillness
                        speedMult = 0.02f;
                        windStrength = 0f;
                        activeCount = MaxSnowflakes;
                        break;
                }

                for (int i = 0; i < MaxSnowflakes; i++)
                {
                    // Absolute Zero: freeze particles in place
                    if (phase == 4 && !_snowflakes[i].IsFrozen)
                    {
                        _snowflakes[i].IsFrozen = true;
                    }
                    else if (phase != 4)
                    {
                        _snowflakes[i].IsFrozen = false;
                    }

                    if (_snowflakes[i].IsFrozen)
                    {
                        _snowflakes[i].Alpha = Math.Max(0.1f, _snowflakes[i].Alpha - 0.001f);
                        continue;
                    }

                    bool visible = i < activeCount;
                    if (!visible)
                    {
                        _snowflakes[i].Alpha = Math.Max(0f, _snowflakes[i].Alpha - 0.02f);
                        continue;
                    }
                    else
                    {
                        _snowflakes[i].Alpha = Math.Min(
                            0.3f + 0.7f * ((float)i / MaxSnowflakes),
                            _snowflakes[i].Alpha + 0.01f);
                    }

                    _snowflakes[i].Position.Y += _snowflakes[i].FallSpeed * speedMult;
                    _snowflakes[i].Position.X += (float)Math.Sin(time * 0.005f + _snowflakes[i].DriftPhase)
                        * _snowflakes[i].DriftAmplitude * 0.01f;
                    _snowflakes[i].Position.X -= windStrength;
                    _snowflakes[i].Rotation += _snowflakes[i].RotationSpeed * speedMult;

                    if (_snowflakes[i].Position.Y > Main.screenHeight + 20)
                    {
                        _snowflakes[i].Position.Y = -10;
                        _snowflakes[i].Position.X = Main.rand.NextFloat() * Main.screenWidth;
                    }
                    if (_snowflakes[i].Position.X < -30)
                        _snowflakes[i].Position.X = Main.screenWidth + 20;
                    if (_snowflakes[i].Position.X > Main.screenWidth + 30)
                        _snowflakes[i].Position.X = -20;
                }
            }

            // Breath crystals — spawn near boss position, drift upward
            if (_breathCrystals != null && phase >= 2)
            {
                float spawnRate = phase == 2 ? 0.3f : (phase == 3 ? 0.6f : 0.1f);
                if (Main.rand.NextFloat() < spawnRate)
                {
                    Vector2 bossScreen = BossCenter - Main.screenPosition;
                    Vector2 spawnPos = bossScreen + Main.rand.NextVector2Circular(200f, 120f);
                    _breathCrystals[_nextCrystalSlot] = new BreathCrystal
                    {
                        Position = spawnPos,
                        Velocity = new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), -Main.rand.NextFloat(0.3f, 1.2f)),
                        Scale = 0.3f + Main.rand.NextFloat() * 0.5f,
                        Alpha = 0.8f,
                        Life = 0,
                        MaxLife = 60 + Main.rand.Next(60),
                        Rotation = Main.rand.NextFloat() * MathHelper.TwoPi
                    };
                    _nextCrystalSlot = (_nextCrystalSlot + 1) % MaxBreathCrystals;
                }

                for (int i = 0; i < MaxBreathCrystals; i++)
                {
                    if (_breathCrystals[i].MaxLife <= 0) continue;
                    _breathCrystals[i].Life++;
                    _breathCrystals[i].Position += _breathCrystals[i].Velocity;
                    float lifeProgress = _breathCrystals[i].Life / _breathCrystals[i].MaxLife;
                    _breathCrystals[i].Alpha = (1f - lifeProgress) * 0.8f;
                    if (_breathCrystals[i].Life >= _breathCrystals[i].MaxLife)
                        _breathCrystals[i].MaxLife = 0;
                }
            }
        }

        public override Color OnTileColor(Color inColor)
        {
            float eff = GetEffectiveIntensity();
            int phase = GetVFXPhase();
            Color tint;
            float tintStr;

            switch (phase)
            {
                case 1: tint = PaleSilverBlue; tintStr = 0.2f; break;
                case 2: tint = IceBlue; tintStr = 0.35f; break;
                case 3: tint = DeepGlacialBlue; tintStr = 0.5f; break;
                default: tint = new Color(180, 200, 220); tintStr = 0.6f; break;
            }

            return Color.Lerp(inColor, tint, eff * tintStr);
        }

        public override void Draw(SpriteBatch spriteBatch, float minDepth, float maxDepth)
        {
            if (maxDepth >= 0f && minDepth < 0f)
            {
                float eff = GetEffectiveIntensity();
                if (eff < 0.01f) return;
                int phase = GetVFXPhase();

                Texture2D pixel = Terraria.GameContent.TextureAssets.MagicPixel.Value;

                // ===== SKY GRADIENT (phase-specific) =====
                Color topColor, bottomColor;
                switch (phase)
                {
                    case 1: // First Frost — pale, gentle
                        topColor = Color.Lerp(new Color(18, 25, 50), new Color(35, 50, 80), _intensity);
                        bottomColor = Color.Lerp(new Color(55, 70, 100), new Color(90, 110, 145), _intensity);
                        break;
                    case 2: // Frozen Expanse — darker, heavier
                        topColor = Color.Lerp(new Color(12, 18, 40), new Color(20, 35, 65), _intensity);
                        bottomColor = Color.Lerp(new Color(40, 55, 85), new Color(70, 95, 130), _intensity);
                        break;
                    case 3: // Blizzard — storm gray
                        topColor = Color.Lerp(new Color(25, 30, 45), new Color(50, 55, 70), _intensity);
                        bottomColor = Color.Lerp(new Color(70, 80, 100), new Color(120, 130, 150), _intensity);
                        break;
                    default: // Absolute Zero — near-monochrome blue-black
                        topColor = new Color(8, 12, 25);
                        bottomColor = new Color(25, 35, 55);
                        break;
                }

                topColor *= eff * 0.85f;
                bottomColor *= eff * 0.85f;

                for (int y = 0; y < Main.screenHeight; y += 4)
                {
                    float t = (float)y / Main.screenHeight;
                    Color lineColor = Color.Lerp(topColor, bottomColor, t);
                    spriteBatch.Draw(pixel, new Rectangle(0, y, Main.screenWidth, 4), lineColor);
                }

                // ===== FROST EDGES (all phases, scales up) =====
                float edgeBase, edgeAlphaBase;
                switch (phase)
                {
                    case 1: edgeBase = 12f + _intensity * 25f; edgeAlphaBase = 0.06f + _intensity * 0.08f; break;
                    case 2: edgeBase = 25f + _intensity * 50f; edgeAlphaBase = 0.10f + _intensity * 0.12f; break;
                    case 3: edgeBase = 40f + _intensity * 70f; edgeAlphaBase = 0.15f + _intensity * 0.15f; break;
                    default:
                        edgeBase = 80f + (float)Math.Sin((float)Main.timeForVisualEffects * 0.008f) * 15f;
                        edgeAlphaBase = 0.25f;
                        break;
                }

                Color frostEdge = (phase == 4 ? BlizzardWhite : IceBlue) * eff * edgeAlphaBase;
                frostEdge.A = 0;

                float time_fe = (float)Main.timeForVisualEffects;

                // Top edge
                for (int x = 0; x < Main.screenWidth; x += 4)
                {
                    float variation = (float)Math.Sin(x * 0.02f + time_fe * 0.001f) * 0.4f + 0.6f;
                    int height = (int)(edgeBase * variation);
                    Color c = frostEdge * variation;
                    spriteBatch.Draw(pixel, new Rectangle(x, 0, 4, height), c);
                }

                // Bottom edge
                for (int x = 0; x < Main.screenWidth; x += 4)
                {
                    float variation = (float)Math.Sin(x * 0.025f + 2f + time_fe * 0.0008f) * 0.4f + 0.6f;
                    int height = (int)(edgeBase * 0.7f * variation);
                    Color c = frostEdge * variation * 0.7f;
                    spriteBatch.Draw(pixel, new Rectangle(x, Main.screenHeight - height, 4, height), c);
                }

                // Side edges
                for (int y = 0; y < Main.screenHeight; y += 4)
                {
                    float variation = (float)Math.Sin(y * 0.03f + time_fe * 0.0012f) * 0.4f + 0.6f;
                    int width = (int)(edgeBase * 0.5f * variation);
                    Color c = frostEdge * variation * 0.5f;
                    spriteBatch.Draw(pixel, new Rectangle(0, y, width, 4), c);
                    spriteBatch.Draw(pixel, new Rectangle(Main.screenWidth - width, y, width, 4), c);
                }

                // ===== FROST FLOOR SPREAD (Phase 2+) — shader-driven =====
                if (phase >= 2 && eff > 0.2f)
                {
                    Effect frostFloorShader = ShaderLoader.GetShader(BossShaderManager.InvernoFrostFloor);
                    Texture2D floorNoiseTex = ShaderLoader.GetNoiseTexture("PerlinNoise");
                    if (frostFloorShader != null && floorNoiseTex != null)
                    {
                        // Boss position normalized to screen UV (0-1 range)
                        Vector2 bossScreen = BossCenter - Main.screenPosition;
                        Vector2 bossUV = new Vector2(
                            MathHelper.Clamp(bossScreen.X / Main.screenWidth, 0f, 1f),
                            MathHelper.Clamp(bossScreen.Y / Main.screenHeight, 0f, 1f));

                        float spreadProgress = phase switch
                        {
                            2 => 0.2f + _intensity * 0.15f,
                            3 => 0.4f + _intensity * 0.2f,
                            _ => BossIsEnraged ? 0.9f : 0.6f + _intensity * 0.15f
                        };

                        float floorIntensity = phase switch
                        {
                            2 => eff * 0.25f,
                            3 => eff * 0.35f,
                            _ => eff * 0.5f
                        };

                        frostFloorShader.Parameters["uColor"]?.SetValue(IceBlue.ToVector4());
                        frostFloorShader.Parameters["uTime"]?.SetValue(time_fe * 0.01f);
                        frostFloorShader.Parameters["uIntensity"]?.SetValue(floorIntensity);
                        frostFloorShader.Parameters["uSpreadProgress"]?.SetValue(spreadProgress);
                        frostFloorShader.Parameters["uOrigin"]?.SetValue(bossUV);

                        var floorDevice = Main.graphics.GraphicsDevice;
                        spriteBatch.End();
                        spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive,
                            SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone,
                            frostFloorShader, Main.GameViewMatrix.TransformationMatrix);
                        floorDevice.Textures[1] = floorNoiseTex;
                        floorDevice.SamplerStates[1] = SamplerState.LinearWrap;

                        spriteBatch.Draw(pixel, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.White);

                        spriteBatch.End();
                        floorDevice.Textures[1] = null;
                        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                            DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
                    }
                }

                // ===== FROST CREEP from edges (Phase 4 / Absolute Zero) — shader-driven =====
                if (phase == 4 && eff > 0.3f)
                {
                    Effect frostCreepShader = ShaderLoader.GetShader(BossShaderManager.InvernoFrostCreep);
                    Texture2D noiseTex = ShaderLoader.GetNoiseTexture("PerlinNoise");
                    if (frostCreepShader != null && noiseTex != null)
                    {
                        float creepNorm = BossIsEnraged ? 0.85f : (0.3f + (float)Math.Sin(time_fe * 0.003f) * 0.1f);
                        frostCreepShader.Parameters["uColor"]?.SetValue(BlizzardWhite.ToVector4());
                        frostCreepShader.Parameters["uTime"]?.SetValue(time_fe * 0.01f);
                        frostCreepShader.Parameters["uIntensity"]?.SetValue(eff * 0.7f);
                        frostCreepShader.Parameters["uCreepDepth"]?.SetValue(creepNorm);

                        var device = Main.graphics.GraphicsDevice;
                        spriteBatch.End();
                        spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive,
                            SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone,
                            frostCreepShader, Main.GameViewMatrix.TransformationMatrix);
                        device.Textures[1] = noiseTex;
                        device.SamplerStates[1] = SamplerState.LinearWrap;

                        spriteBatch.Draw(pixel, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.White);

                        spriteBatch.End();
                        device.Textures[1] = null;
                        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                            DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
                    }
                }

                // ===== SNOWFLAKES =====
                if (_snowflakes != null && eff > 0.1f)
                {
                    float snowAlphaBase;
                    switch (phase)
                    {
                        case 1: snowAlphaBase = 0.25f; break;
                        case 2: snowAlphaBase = 0.4f; break;
                        case 3: snowAlphaBase = 0.55f; break;
                        default: snowAlphaBase = 0.3f; break;
                    }

                    for (int i = 0; i < MaxSnowflakes; i++)
                    {
                        if (_snowflakes[i].Alpha < 0.01f) continue;

                        float alpha = _snowflakes[i].Alpha * eff * snowAlphaBase;
                        Color baseColor = phase == 4 ? BlizzardWhite
                            : Color.Lerp(IceBlue, FrostWhite, _snowflakes[i].Scale);
                        Color c = baseColor * alpha;
                        c.A = 0;

                        float s = _snowflakes[i].Scale * (1f + _intensity * 0.3f);

                        if (phase == 3)
                        {
                            // Blizzard: elongated wind-streaks
                            float stretch = 1f + s * 2f;
                            spriteBatch.Draw(pixel, _snowflakes[i].Position, new Rectangle(0, 0, 1, 1),
                                c, -0.3f, new Vector2(0.5f), new Vector2(s * stretch, s * 0.5f),
                                SpriteEffects.None, 0f);
                        }
                        else
                        {
                            spriteBatch.Draw(pixel, _snowflakes[i].Position, new Rectangle(0, 0, 1, 1),
                                c, _snowflakes[i].Rotation, new Vector2(0.5f), s * 2.5f,
                                SpriteEffects.None, 0f);
                        }
                    }
                }

                // ===== BREATH CRYSTALS =====
                if (_breathCrystals != null && phase >= 2 && eff > 0.2f)
                {
                    for (int i = 0; i < MaxBreathCrystals; i++)
                    {
                        if (_breathCrystals[i].MaxLife <= 0 || _breathCrystals[i].Alpha < 0.01f) continue;
                        Color crystalColor = Color.Lerp(CrystalCyan, FrostWhite,
                            _breathCrystals[i].Life / _breathCrystals[i].MaxLife) * _breathCrystals[i].Alpha * eff;
                        crystalColor.A = 0;
                        spriteBatch.Draw(pixel, _breathCrystals[i].Position, new Rectangle(0, 0, 1, 1),
                            crystalColor, _breathCrystals[i].Rotation, new Vector2(0.5f),
                            _breathCrystals[i].Scale * 3f, SpriteEffects.None, 0f);
                    }
                }

                // ===== WHITEOUT OVERLAY (Phase 3) — shader-driven =====
                if (phase == 3 && eff > 0.3f)
                {
                    Effect whiteoutShader = ShaderLoader.GetShader(BossShaderManager.InvernoWhiteout);
                    Texture2D noiseTex = ShaderLoader.GetNoiseTexture("PerlinNoise");
                    if (whiteoutShader != null && noiseTex != null)
                    {
                        float intensityFactor = MathHelper.Clamp((_intensity - 0.6f) / 0.4f, 0f, 1f);
                        whiteoutShader.Parameters["uColor"]?.SetValue(BlizzardWhite.ToVector4());
                        whiteoutShader.Parameters["uTime"]?.SetValue(time_fe * 0.01f);
                        whiteoutShader.Parameters["uIntensity"]?.SetValue(eff * 0.18f * intensityFactor);
                        whiteoutShader.Parameters["uWindDirection"]?.SetValue(-0.3f);

                        var device = Main.graphics.GraphicsDevice;
                        spriteBatch.End();
                        spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive,
                            SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone,
                            whiteoutShader, Main.GameViewMatrix.TransformationMatrix);
                        device.Textures[1] = noiseTex;
                        device.SamplerStates[1] = SamplerState.LinearWrap;

                        spriteBatch.Draw(pixel, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.White);

                        spriteBatch.End();
                        device.Textures[1] = null;
                        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                            DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
                    }
                    else
                    {
                        // Fallback: simple pixel whiteout if shader unavailable
                        float whiteoutPulse = (float)Math.Sin(time_fe * 0.012f) * 0.3f + 0.5f;
                        float fallbackFactor = MathHelper.Clamp((_intensity - 0.6f) / 0.4f, 0f, 1f);
                        float whiteoutAlpha = MathHelper.Clamp(eff * 0.08f * whiteoutPulse * fallbackFactor, 0f, 0.15f);
                        Color whiteout = BlizzardWhite * whiteoutAlpha;
                        whiteout.A = 0;
                        spriteBatch.Draw(pixel, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), whiteout);
                    }
                }

                // ===== VIGNETTE (all phases, adaptive) =====
                if (eff > 0.15f)
                {
                    float vigBase;
                    int ringCount;
                    Color vigColor;

                    switch (phase)
                    {
                        case 1:
                            vigBase = 0.08f + _intensity * 0.1f;
                            ringCount = 6;
                            vigColor = PaleSilverBlue;
                            break;
                        case 2:
                            vigBase = 0.12f + _intensity * 0.15f;
                            ringCount = 8;
                            vigColor = IceBlue;
                            break;
                        case 3:
                            vigBase = 0.2f + _intensity * 0.2f;
                            ringCount = 10;
                            vigColor = DeepGlacialBlue;
                            break;
                        default:
                            vigBase = 0.35f + (BossIsEnraged ? 0.25f : 0.1f);
                            ringCount = 14;
                            vigColor = new Color(15, 25, 50);
                            break;
                    }

                    for (int r = 0; r < ringCount; r++)
                    {
                        float ringT = (float)r / ringCount;
                        float ringAlpha = eff * vigBase * ringT * ringT;
                        Color rc = vigColor * ringAlpha;
                        rc.A = 0;

                        int inset = (int)((1f - ringT) * Math.Min(Main.screenWidth, Main.screenHeight) * 0.5f);
                        if (inset < 1) inset = 1;

                        spriteBatch.Draw(pixel, new Rectangle(0, 0, Main.screenWidth, inset), rc);
                        spriteBatch.Draw(pixel, new Rectangle(0, Main.screenHeight - inset, Main.screenWidth, inset), rc);
                        spriteBatch.Draw(pixel, new Rectangle(0, inset, inset, Main.screenHeight - inset * 2), rc);
                        spriteBatch.Draw(pixel, new Rectangle(Main.screenWidth - inset, inset, inset, Main.screenHeight - inset * 2), rc);
                    }
                }
            }
        }

        public override float GetCloudAlpha()
        {
            int phase = GetVFXPhase();
            float suppress;
            switch (phase)
            {
                case 1: suppress = 0.3f; break;
                case 2: suppress = 0.5f; break;
                case 3: suppress = 0.8f; break;
                default: suppress = 0.9f; break;
            }
            return 1f - _opacity * suppress;
        }
    }

    /// <summary>
    /// Companion ModSystem for LInvernoSky.
    /// Phase-aware flash APIs and ambient world particles.
    /// Flash uses subtractive decay for more natural falloff.
    /// </summary>
    public class LInvernoSkySystem : ModSystem
    {
        private static float _flashIntensity;
        private static Color _flashColor;

        private static void TriggerFlash(Color color, float intensity)
        {
            _flashIntensity = Math.Max(_flashIntensity, intensity);
            _flashColor = color;
        }

        /// <summary>Frost flash — icy blue burst.</summary>
        public static void TriggerFrostFlash(float intensity = 1f)
            => TriggerFlash(new Color(168, 216, 234), intensity);

        /// <summary>Blizzard flash — deep glacial burst.</summary>
        public static void TriggerBlizzardFlash(float intensity = 1f)
            => TriggerFlash(new Color(27, 79, 114), intensity);

        /// <summary>Crystal flash — brilliant frost white.</summary>
        public static void TriggerCrystalFlash(float intensity = 1f)
            => TriggerFlash(new Color(232, 244, 248), intensity);

        /// <summary>Absolute zero flash — blinding white for death climax.</summary>
        public static void TriggerAbsoluteZeroFlash(float intensity = 1f)
            => TriggerFlash(Color.White, intensity);

        public override void PostUpdateEverything()
        {
            // Subtractive decay — more natural falloff
            if (_flashIntensity > 0.01f)
                _flashIntensity = Math.Max(0f, _flashIntensity - 0.12f);
            else
                _flashIntensity = 0f;

            if (Main.netMode == NetmodeID.Server) return;
            NPC boss = BossIndexTracker.GetActiveBoss(BossIndexTracker.LInverno);
            if (boss == null) return;

            float hpDrive = 1f - (boss.life / (float)boss.lifeMax);
            int phase = LInvernoSky.GetVFXPhase();

            // Phase-scaled ambient frost motes
            int moteChance;
            switch (phase)
            {
                case 1: moteChance = 10; break;
                case 2: moteChance = 6; break;
                case 3: moteChance = 3; break;
                default: moteChance = 15; break;
            }

            if (Main.rand.NextBool(Math.Max(1, moteChance)))
            {
                Vector2 pos = Main.LocalPlayer.Center + Main.rand.NextVector2Circular(600f, 400f);
                int dustType = phase >= 3 ? DustID.BlueTorch : DustID.IceTorch;
                int d = Dust.NewDust(pos, 4, 4, dustType, Main.rand.NextFloat(-0.5f, 0.5f), 0.8f, 100, default, 1.0f);
                Main.dust[d].noGravity = true;
                Main.dust[d].fadeIn = 1.1f;
            }

            // Snow wisps at Phase 2+
            if (phase >= 2 && Main.rand.NextBool(Math.Max(1, (int)(6 - hpDrive * 3))))
            {
                Vector2 pos = Main.LocalPlayer.Center + Main.rand.NextVector2Circular(500f, 350f);
                int d = Dust.NewDust(pos, 4, 4, DustID.BlueTorch, -1f, Main.rand.NextFloat(0.3f, 1.2f), 120, default, 0.8f);
                Main.dust[d].noGravity = true;
            }

            // Phase 4: Frozen dust motes hover in place
            if (phase == 4 && Main.rand.NextBool(8))
            {
                Vector2 pos = Main.LocalPlayer.Center + Main.rand.NextVector2Circular(400f, 300f);
                int d = Dust.NewDust(pos, 2, 2, DustID.IceTorch, 0f, 0f, 150, default, 0.6f);
                Main.dust[d].noGravity = true;
                Main.dust[d].velocity = Vector2.Zero;
            }
        }

        public static float GetFlashIntensity() => _flashIntensity;
        public static Color GetFlashColor() => _flashColor;
    }
}