using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.Bosses;
using MagnumOpus.Common.Systems.VFX;

namespace MagnumOpus.Content.Fate.Bosses.Systems
{
    /// <summary>
    /// Custom sky for the Warden of Melodies — the endgame cosmic encounter.
    /// 4-phase arena that evolves from a void of stars to a collapsing singularity.
    ///
    /// Phase 1 (Awakening):  Deep void starfield with chromatic aberration vignette.
    /// Phase 2 (Convergence): Reality warps and fractures, showing glimpses of other arenas.
    /// Phase 3 (Singularity): Visual black hole — all stars spiral inward toward boss center.
    /// Enrage  (Unraveling):  Arena fractures into floating shards with void between them.
    /// </summary>
    public class FateSky : CustomSky
    {
        private bool _isActive;
        private float _opacity;

        // Boss state
        private float _bossLifeRatio = 1f;
        private Vector2 _bossCenter;
        private bool _bossIsAwakened;
        private int _bossPhase; // 0-3
        private bool _bossIsEnraged;
        private float _phaseTransitionFlash;

        // Phase blend targets (smoothly interpolated)
        private float _singularityStrength;  // 0→1 as phase 3 activates
        private float _convergenceStrength;  // 0→1 as phase 2 activates
        private float _unravelStrength;      // 0→1 during enrage

        // Chromatic aberration edge intensity
        private float _chromaticEdge;

        // Starfield
        private struct CosmicStar
        {
            public Vector2 Position;      // screen-space position
            public Vector2 BasePosition;  // original position for reset
            public float Brightness;
            public float PulseSpeed;
            public float PulseOffset;
            public float Scale;
            public Color Tint;
            public float Depth;           // parallax depth 0..1
        }

        private CosmicStar[] _stars;
        private const int MaxStars = 150;

        // Reality cracks (Phase 2+)
        private struct RealityCrack
        {
            public Vector2 Start;
            public Vector2 End;
            public float Width;
            public float Glow;
            public int ThemeIndex; // which boss arena "shows through" the crack
        }

        private RealityCrack[] _cracks;
        private const int MaxCracks = 18;

        // Floating arena shards (Enrage)
        private struct ArenaShard
        {
            public Vector2 Position;
            public float Rotation;
            public float Size;
            public float Drift;
        }

        private ArenaShard[] _shards;
        private const int MaxShards = 12;

        // 10 theme colors — fleeting echoes of all bosses
        private static readonly Color[] ThemeColors = new Color[]
        {
            new Color(200, 120, 180),  // Spring — pink
            new Color(255, 200, 50),   // Eroica — gold
            new Color(140, 60, 200),   // Enigma — green-purple
            new Color(140, 100, 200),  // Moonlight Sonata — purple
            new Color(240, 240, 255),  // Swan Lake — white
            new Color(255, 140, 40),   // La Campanella — orange
            new Color(200, 50, 30),    // Dies Irae — blood red
            new Color(150, 200, 255),  // Clair de Lune — ice blue
            new Color(100, 120, 200),  // Nachtmusik — indigo
            new Color(255, 200, 50),   // Ode to Joy — warm gold
        };

        public override void OnLoad() { }

        public override void Activate(Vector2 position, params object[] args)
        {
            _isActive = true;
            Random rand = new Random();
            _stars = new CosmicStar[MaxStars];
            _cracks = new RealityCrack[MaxCracks];
            _shards = new ArenaShard[MaxShards];

            for (int i = 0; i < MaxStars; i++)
            {
                float depth = 0.2f + (float)rand.NextDouble() * 0.8f;
                _stars[i] = new CosmicStar
                {
                    Position = new Vector2(
                        (float)rand.NextDouble() * Main.screenWidth,
                        (float)rand.NextDouble() * Main.screenHeight),
                    Brightness = 0.2f + (float)rand.NextDouble() * 0.8f,
                    PulseSpeed = 0.008f + (float)rand.NextDouble() * 0.025f,
                    PulseOffset = (float)rand.NextDouble() * MathHelper.TwoPi,
                    Scale = 0.15f + (float)rand.NextDouble() * 0.6f * depth,
                    Tint = Color.Lerp(new Color(230, 220, 255), new Color(180, 40, 80),
                        (float)rand.NextDouble() * 0.3f),
                    Depth = depth,
                };
                _stars[i].BasePosition = _stars[i].Position;
            }

            for (int i = 0; i < MaxCracks; i++)
            {
                Vector2 center = new Vector2(
                    (float)rand.NextDouble() * Main.screenWidth,
                    (float)rand.NextDouble() * Main.screenHeight);
                float angle = (float)rand.NextDouble() * MathHelper.TwoPi;
                float length = 60f + (float)rand.NextDouble() * 180f;
                _cracks[i] = new RealityCrack
                {
                    Start = center - angle.ToRotationVector2() * length * 0.5f,
                    End = center + angle.ToRotationVector2() * length * 0.5f,
                    Width = 1.5f + (float)rand.NextDouble() * 4f,
                    Glow = 0.4f + (float)rand.NextDouble() * 0.6f,
                    ThemeIndex = rand.Next(10),
                };
            }

            for (int i = 0; i < MaxShards; i++)
            {
                _shards[i] = new ArenaShard
                {
                    Position = new Vector2(
                        (float)rand.NextDouble() * Main.screenWidth,
                        (float)rand.NextDouble() * Main.screenHeight),
                    Rotation = (float)rand.NextDouble() * MathHelper.TwoPi,
                    Size = 40f + (float)rand.NextDouble() * 120f,
                    Drift = 0.2f + (float)rand.NextDouble() * 0.5f,
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

        public override void Update(GameTime gameTime)
        {
            if (_isActive)
                _opacity = Math.Min(1f, _opacity + 0.015f);
            else
                _opacity = Math.Max(0f, _opacity - 0.015f);

            _bossLifeRatio = FateSkySystem.BossLifeRatio;
            _bossCenter = FateSkySystem.BossCenter;
            _bossIsAwakened = FateSkySystem.BossIsAwakened;
            _bossPhase = FateSkySystem.BossPhase;
            _bossIsEnraged = FateSkySystem.BossIsEnraged;

            float hpDrive = 1f - _bossLifeRatio;

            // Smooth phase-blend interpolation
            float convergenceTarget = _bossPhase >= 1 ? 1f : 0f;
            float singularityTarget = _bossPhase >= 2 ? 1f : 0f;
            float unravelTarget = _bossIsEnraged ? 1f : 0f;

            _convergenceStrength += (_convergenceStrength < convergenceTarget ? 0.008f : -0.005f);
            _convergenceStrength = MathHelper.Clamp(_convergenceStrength, 0f, 1f);

            _singularityStrength += (_singularityStrength < singularityTarget ? 0.006f : -0.004f);
            _singularityStrength = MathHelper.Clamp(_singularityStrength, 0f, 1f);

            _unravelStrength += (_unravelStrength < unravelTarget ? 0.01f : -0.006f);
            _unravelStrength = MathHelper.Clamp(_unravelStrength, 0f, 1f);

            // Chromatic aberration at screen edges — always present, intensifies with phase
            float chromaticTarget = 0.15f + _convergenceStrength * 0.25f + _singularityStrength * 0.3f + _unravelStrength * 0.3f;
            _chromaticEdge += (_chromaticEdge < chromaticTarget ? 0.004f : -0.003f);
            _chromaticEdge = MathHelper.Clamp(_chromaticEdge, 0f, 1f);

            // Phase transition flash decay
            _phaseTransitionFlash *= 0.92f;

            // Animate stars
            float time = (float)Main.timeForVisualEffects;
            float pulseMult = 1f + hpDrive * 0.6f + _singularityStrength * 0.5f;
            Vector2 screenCenter = new Vector2(Main.screenWidth * 0.5f, Main.screenHeight * 0.5f);
            Vector2 bossScreenPos = _bossCenter != Vector2.Zero
                ? _bossCenter - Main.screenPosition
                : screenCenter;

            if (_stars != null)
            {
                for (int i = 0; i < MaxStars; i++)
                {
                    float pulse = (float)Math.Sin(time * _stars[i].PulseSpeed * pulseMult + _stars[i].PulseOffset);
                    _stars[i].Brightness = 0.2f + pulse * 0.3f + hpDrive * 0.2f;

                    // Phase 3: Stars spiral inward toward boss (black hole effect)
                    if (_singularityStrength > 0.05f)
                    {
                        Vector2 toBoss = bossScreenPos - _stars[i].Position;
                        float dist = toBoss.Length();
                        if (dist > 5f)
                        {
                            float spiralSpeed = 0.3f + _singularityStrength * 0.8f;
                            float tangent = 0.6f; // spiral ratio
                            Vector2 radial = toBoss / dist;
                            Vector2 perp = new Vector2(-radial.Y, radial.X);
                            _stars[i].Position += (radial + perp * tangent) * spiralSpeed * _stars[i].Depth;
                        }

                        // Respawn stars that reach boss center
                        if (dist < 20f)
                        {
                            _stars[i].Position = new Vector2(
                                Main.rand.Next(Main.screenWidth),
                                Main.rand.Next(Main.screenHeight));
                            // Push to edge
                            float edge = Main.rand.Next(4);
                            if (edge < 1) _stars[i].Position.X = 0;
                            else if (edge < 2) _stars[i].Position.X = Main.screenWidth;
                            else if (edge < 3) _stars[i].Position.Y = 0;
                            else _stars[i].Position.Y = Main.screenHeight;
                        }
                    }
                    // Phase 2: Stars drift slowly, speed increases
                    else if (_convergenceStrength > 0.1f)
                    {
                        float driftSpeed = 0.15f * _convergenceStrength * _stars[i].Depth;
                        _stars[i].Position += new Vector2(
                            (float)Math.Sin(time * 0.001f + i * 0.7f) * driftSpeed,
                            (float)Math.Cos(time * 0.0008f + i * 1.1f) * driftSpeed);
                    }

                    // Wrap stars to screen bounds
                    if (_stars[i].Position.X < -20) _stars[i].Position.X += Main.screenWidth + 40;
                    if (_stars[i].Position.X > Main.screenWidth + 20) _stars[i].Position.X -= Main.screenWidth + 40;
                    if (_stars[i].Position.Y < -20) _stars[i].Position.Y += Main.screenHeight + 40;
                    if (_stars[i].Position.Y > Main.screenHeight + 20) _stars[i].Position.Y -= Main.screenHeight + 40;
                }
            }

            // Animate enrage shards — slow drift and rotation
            if (_shards != null && _unravelStrength > 0.05f)
            {
                for (int i = 0; i < MaxShards; i++)
                {
                    _shards[i].Rotation += _shards[i].Drift * 0.003f;
                    _shards[i].Position += new Vector2(
                        (float)Math.Sin(time * 0.0005f + i * 2.3f) * _shards[i].Drift,
                        (float)Math.Cos(time * 0.0004f + i * 1.7f) * _shards[i].Drift * 0.5f);
                }
            }
        }

        public override void Draw(SpriteBatch spriteBatch, float minDepth, float maxDepth)
        {
            if (maxDepth >= 0f && minDepth < 0f)
            {
                Texture2D pixel = Terraria.GameContent.TextureAssets.MagicPixel.Value;
                float time = (float)Main.timeForVisualEffects;
                float hpDrive = 1f - _bossLifeRatio;

                // === VOID BACKGROUND ===
                // Deep black void — gets slightly more purple in convergence, darker in singularity
                Color topColor = Color.Lerp(new Color(3, 1, 8), new Color(20, 6, 30), _convergenceStrength * 0.5f);
                Color bottomColor = Color.Lerp(new Color(6, 2, 14), new Color(35, 10, 45), _convergenceStrength * 0.5f);
                // Singularity darkens everything toward pure black
                topColor = Color.Lerp(topColor, new Color(2, 0, 4), _singularityStrength * 0.6f);
                bottomColor = Color.Lerp(bottomColor, new Color(4, 1, 8), _singularityStrength * 0.6f);
                topColor *= _opacity;
                bottomColor *= _opacity;

                for (int y = 0; y < Main.screenHeight; y += 4)
                {
                    float t = (float)y / Main.screenHeight;
                    Color lineColor = Color.Lerp(topColor, bottomColor, t);
                    spriteBatch.Draw(pixel, new Rectangle(0, y, Main.screenWidth, 4), lineColor);
                }

                // === ENRAGE: Floating arena shards with void between ===
                if (_shards != null && _unravelStrength > 0.05f)
                {
                    DrawArenaShards(spriteBatch, pixel, time);
                }

                // === STARFIELD ===
                if (_stars != null && _opacity > 0.1f)
                {
                    // During singularity, stars near boss form visible trailing streaks
                    Vector2 bossScreen = _bossCenter != Vector2.Zero
                        ? _bossCenter - Main.screenPosition
                        : new Vector2(Main.screenWidth * 0.5f, Main.screenHeight * 0.5f);

                    for (int i = 0; i < MaxStars; i++)
                    {
                        float brightness = _stars[i].Brightness * _opacity;
                        if (brightness < 0.02f) continue;

                        Color c = _stars[i].Tint * brightness;
                        c.A = 0; // additive

                        float s = _stars[i].Scale * (1f + _singularityStrength * 0.4f);

                        // Singularity: stars near boss stretch into streaks toward center
                        if (_singularityStrength > 0.2f)
                        {
                            Vector2 toBoss = bossScreen - _stars[i].Position;
                            float dist = toBoss.Length();
                            if (dist < 300f && dist > 10f)
                            {
                                float streakFactor = (1f - dist / 300f) * _singularityStrength;
                                float angle = (float)Math.Atan2(toBoss.Y, toBoss.X);
                                float streakLength = s * 4f * (1f + streakFactor * 6f);
                                spriteBatch.Draw(pixel, _stars[i].Position, new Rectangle(0, 0, 1, 1),
                                    c, angle, new Vector2(0.5f), new Vector2(streakLength, s * 1.5f),
                                    SpriteEffects.None, 0f);
                                continue;
                            }
                        }

                        spriteBatch.Draw(pixel, _stars[i].Position, new Rectangle(0, 0, 1, 1),
                            c, 0f, new Vector2(0.5f), s * 2.5f, SpriteEffects.None, 0f);
                    }
                }

                // === PHASE 2+: Reality cracks showing glimpses of other boss arenas ===
                if (_cracks != null && _convergenceStrength > 0.05f && _opacity > 0.1f)
                {
                    DrawRealityCracks(spriteBatch, pixel, time);
                }

                // === SINGULARITY: Dark accretion disk glow around boss center ===
                if (_singularityStrength > 0.1f)
                {
                    DrawAccretionDisk(spriteBatch, pixel, time);
                }

                // === CHROMATIC ABERRATION VIGNETTE (screen edges) ===
                if (_chromaticEdge > 0.02f)
                {
                    DrawChromaticVignette(spriteBatch, pixel);
                }

                // === PHASE TRANSITION "REALITY PUNCH" FLASH ===
                if (_phaseTransitionFlash > 0.01f)
                {
                    Color flashColor = Color.White * (_phaseTransitionFlash * _opacity);
                    spriteBatch.Draw(pixel, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), flashColor);
                }

                // === COSMIC FLASH OVERLAY ===
                var (flashIntensity, flashCol) = FateSkySystem.GetFlashState();
                if (flashIntensity > 0.01f)
                {
                    Color fc = flashCol * (flashIntensity * 0.12f * _opacity);
                    fc.A = 0;
                    spriteBatch.Draw(pixel, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), fc);
                }
            }
        }

        private void DrawArenaShards(SpriteBatch sb, Texture2D pixel, float time)
        {
            // Floating rectangular shards of "arena floor" drifting in the void
            for (int i = 0; i < MaxShards; i++)
            {
                float shardAlpha = _unravelStrength * _opacity * 0.3f;
                float pulse = 0.7f + 0.3f * (float)Math.Sin(time * 0.002f + i * 1.3f);
                // Each shard has a faint theme-color tint on its edge
                Color edgeColor = ThemeColors[i % 10] * (shardAlpha * pulse);
                edgeColor.A = 0;

                Vector2 pos = _shards[i].Position;
                float rot = _shards[i].Rotation;
                float size = _shards[i].Size;

                // Draw as a faint glowing rectangle outline (rotated)
                sb.Draw(pixel, pos, new Rectangle(0, 0, 1, 1), edgeColor, rot,
                    new Vector2(0.5f), new Vector2(size, size * 0.6f), SpriteEffects.None, 0f);

                // Void gap lines between shards — dark crimson hairlines
                Color gapColor = new Color(60, 10, 20) * (shardAlpha * 0.5f);
                gapColor.A = 0;
                sb.Draw(pixel, pos + new Vector2(size * 0.5f, 0).RotatedBy(rot), new Rectangle(0, 0, 1, 1),
                    gapColor, rot, Vector2.Zero, new Vector2(size * 0.3f, 1.5f), SpriteEffects.None, 0f);
            }
        }

        private void DrawRealityCracks(SpriteBatch sb, Texture2D pixel, float time)
        {
            for (int i = 0; i < MaxCracks; i++)
            {
                float crackAlpha = _convergenceStrength * _opacity * _cracks[i].Glow;
                // In singularity, cracks widen and brighten
                crackAlpha *= 1f + _singularityStrength * 0.8f;

                float pulse = (float)Math.Sin(time * 0.025f + i * 0.9f) * 0.3f + 0.7f;

                // Crack edge glows in the theme color of the "arena" showing through
                Color crackEdge = ThemeColors[_cracks[i].ThemeIndex] * (crackAlpha * pulse);
                crackEdge.A = 0;

                // Core of crack is bright crimson/white
                Color crackCore = Color.Lerp(new Color(220, 40, 60), new Color(230, 220, 255), pulse) * crackAlpha;
                crackCore.A = 0;

                Vector2 dir = _cracks[i].End - _cracks[i].Start;
                float length = dir.Length();
                if (length < 1f) continue;
                dir /= length;
                float rotation = (float)Math.Atan2(dir.Y, dir.X);

                float width = _cracks[i].Width * (1f + _convergenceStrength * 0.5f + _singularityStrength);

                // Outer glow (theme color)
                sb.Draw(pixel, _cracks[i].Start, new Rectangle(0, 0, 1, 1), crackEdge, rotation,
                    Vector2.Zero, new Vector2(length, width * 3f), SpriteEffects.None, 0f);
                // Core line (crimson-white)
                sb.Draw(pixel, _cracks[i].Start, new Rectangle(0, 0, 1, 1), crackCore, rotation,
                    Vector2.Zero, new Vector2(length, width), SpriteEffects.None, 0f);
            }
        }

        private void DrawAccretionDisk(SpriteBatch sb, Texture2D pixel, float time)
        {
            Vector2 bossScreen = _bossCenter != Vector2.Zero
                ? _bossCenter - Main.screenPosition
                : new Vector2(Main.screenWidth * 0.5f, Main.screenHeight * 0.5f);

            // Concentric rings of crimson/pink glow spiraling around boss
            int ringCount = 8;
            for (int r = 0; r < ringCount; r++)
            {
                float radius = 30f + r * 25f;
                float ringAlpha = _singularityStrength * _opacity * (1f - r / (float)ringCount) * 0.2f;
                float angleOffset = time * (0.015f + r * 0.003f);
                int segments = 16 + r * 4;

                for (int s = 0; s < segments; s++)
                {
                    float angle = MathHelper.TwoPi * s / segments + angleOffset;
                    Vector2 pos = bossScreen + new Vector2(
                        (float)Math.Cos(angle) * radius,
                        (float)Math.Sin(angle) * radius * 0.4f); // elliptical for perspective

                    float segPulse = 0.6f + 0.4f * (float)Math.Sin(time * 0.03f + angle * 2f);
                    Color diskColor = Color.Lerp(FatePalette.DarkPink, FatePalette.BrightCrimson, segPulse) * ringAlpha;
                    diskColor.A = 0;

                    sb.Draw(pixel, pos, new Rectangle(0, 0, 1, 1), diskColor, angle,
                        new Vector2(0.5f), new Vector2(radius * 0.4f / segments * 2f, 2f + r * 0.3f),
                        SpriteEffects.None, 0f);
                }
            }
        }

        private void DrawChromaticVignette(SpriteBatch sb, Texture2D pixel)
        {
            // Chromatic aberration at edges: red/blue color fringing expanding inward
            int edgeLayers = 10;
            for (int ring = 0; ring < edgeLayers; ring++)
            {
                float t = (float)ring / edgeLayers;
                float alpha = t * t * _chromaticEdge * _opacity * 0.35f;

                int insetX = (int)(Main.screenWidth * 0.5f * (1f - t));
                int insetY = (int)(Main.screenHeight * 0.5f * (1f - t));

                // Red channel — offset outward
                Color redFringe = new Color(220, 30, 60) * (alpha * 0.6f);
                redFringe.A = 0;
                int redOff = (int)(t * 3f * _chromaticEdge);
                sb.Draw(pixel, new Rectangle(-redOff, 0, insetX, Main.screenHeight), redFringe);
                sb.Draw(pixel, new Rectangle(Main.screenWidth - insetX + redOff, 0, insetX, Main.screenHeight), redFringe);

                // Blue channel — offset opposite
                Color blueFringe = new Color(40, 50, 220) * (alpha * 0.4f);
                blueFringe.A = 0;
                sb.Draw(pixel, new Rectangle(0, -redOff, Main.screenWidth, insetY), blueFringe);
                sb.Draw(pixel, new Rectangle(0, Main.screenHeight - insetY + redOff, Main.screenWidth, insetY), blueFringe);

                // Base vignette darkness
                Color vigDark = new Color(5, 2, 10) * (alpha * 1.2f);
                sb.Draw(pixel, new Rectangle(0, 0, Main.screenWidth, insetY), vigDark);
                sb.Draw(pixel, new Rectangle(0, Main.screenHeight - insetY, Main.screenWidth, insetY), vigDark);
                sb.Draw(pixel, new Rectangle(0, 0, insetX, Main.screenHeight), vigDark);
                sb.Draw(pixel, new Rectangle(Main.screenWidth - insetX, 0, insetX, Main.screenHeight), vigDark);
            }
        }

        public override Color OnTileColor(Color inColor)
        {
            float eff = _opacity;
            float darkening = 0.35f + _singularityStrength * 0.2f + _unravelStrength * 0.15f;
            Color tint = Color.Lerp(Color.White, new Color(140, 120, 170), eff * darkening);
            return inColor.MultiplyRGBA(tint);
        }

        public override float GetCloudAlpha() => 1f - _opacity * 0.95f;

        /// <summary>Called by FateSkySystem when a "reality punch" phase transition occurs.</summary>
        public void TriggerRealityPunch() => _phaseTransitionFlash = 1f;
    }

    /// <summary>
    /// Companion ModSystem for FateSky — feeds boss state to sky, provides flash/transition APIs.
    /// </summary>
    public class FateSkySystem : ModSystem
    {
        // Boss state — fed from FateWardenOfMelodies.AI()
        public static float BossLifeRatio { get; set; } = 1f;
        public static Vector2 BossCenter { get; set; }
        public static bool BossIsAwakened { get; set; }
        public static int BossPhase { get; set; }      // 0=Awakening, 1=Convergence, 2=Singularity, 3=CosmicWrath
        public static bool BossIsEnraged { get; set; }

        // Flash system
        private static float _flashIntensity;
        private static Color _flashColor;
        private static float _flashDecay;

        public static void TriggerCosmicFlash(float intensity)
        {
            _flashIntensity = Math.Max(_flashIntensity, intensity);
            _flashColor = new Color(180, 50, 100);
            _flashDecay = 0.92f;
        }

        public static void TriggerCrimsonFlash(float intensity)
        {
            _flashIntensity = Math.Max(_flashIntensity, intensity);
            _flashColor = new Color(255, 60, 80);
            _flashDecay = 0.90f;
        }

        public static void TriggerCelestialFlash(float intensity)
        {
            _flashIntensity = Math.Max(_flashIntensity, intensity);
            _flashColor = new Color(230, 220, 255);
            _flashDecay = 0.88f;
        }

        public static void TriggerSupernovaFlash(float intensity)
        {
            _flashIntensity = Math.Max(_flashIntensity, intensity);
            _flashColor = Color.White;
            _flashDecay = 0.85f;
        }

        /// <summary>"Reality punch" — bright white flash + screen shake for phase transitions.</summary>
        public static void TriggerRealityPunch()
        {
            TriggerSupernovaFlash(20f);
            MagnumScreenEffects.AddScreenShake(28f);

            // Propagate to sky instance
            if (!Main.dedServ && SkyManager.Instance["MagnumOpus:FateSky"] is FateSky sky)
                sky.TriggerRealityPunch();
        }

        public static (float intensity, Color color) GetFlashState() => (_flashIntensity, _flashColor);

        public override void PostUpdateEverything()
        {
            if (_flashIntensity > 0.01f)
                _flashIntensity *= _flashDecay;
            else
                _flashIntensity = 0f;

            // Ambient world dust — cosmic motes around boss
            if (BossCenter != Vector2.Zero && BossLifeRatio > 0f)
            {
                float hpDrive = 1f - BossLifeRatio;
                int moteChance = Math.Max(1, (int)(10 - hpDrive * 7));
                if (Main.rand.NextBool(moteChance))
                {
                    Vector2 offset = Main.rand.NextVector2Circular(200f, 200f);
                    Dust d = Dust.NewDustPerfect(BossCenter + offset, DustID.PinkTorch,
                        new Vector2(0f, Main.rand.NextFloat(-1f, -0.3f)), 0, default, 0.8f);
                    d.noGravity = true;
                    d.fadeIn = 1.2f;
                }

                // Star particle streaming (Phase 2+): constant lines of star dust
                if (BossPhase >= 1 && Main.rand.NextBool(Math.Max(1, 4 - BossPhase)))
                {
                    Vector2 spawnPos = BossCenter + Main.rand.NextVector2Circular(350f, 350f);
                    Vector2 toBoss = (BossCenter - spawnPos).SafeNormalize(Vector2.UnitY);
                    float speed = 1.5f + BossPhase * 0.8f + hpDrive * 2f;
                    Dust d = Dust.NewDustPerfect(spawnPos, DustID.FireworkFountain_Red,
                        toBoss * speed, 0, default, 0.5f + hpDrive * 0.3f);
                    d.noGravity = true;
                    d.fadeIn = 1f;
                }

                if (BossIsAwakened && Main.rand.NextBool(Math.Max(1, (int)(6 - hpDrive * 4))))
                {
                    Vector2 offset = Main.rand.NextVector2Circular(150f, 150f);
                    Dust d = Dust.NewDustPerfect(BossCenter + offset, DustID.FireworkFountain_Red,
                        new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), Main.rand.NextFloat(-1.5f, -0.5f)), 0, default, 0.6f);
                    d.noGravity = true;
                }
            }
        }
    }
}
