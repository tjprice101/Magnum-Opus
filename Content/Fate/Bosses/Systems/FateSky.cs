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
    /// Custom sky effect for the Fate boss fight — the Warden of Melodies.
    /// Creates a cosmic void with constellation backdrop that reacts to boss state.
    /// Phase 1: Dark void with distant, slowly pulsing stars.
    /// True Form: Cracking reality with cosmic energy bleeding through fractures.
    /// </summary>
    public class FateSky : CustomSky
    {
        private bool _isActive;
        private float _opacity;
        private float _awakening; // 0 = phase 1, 1 = True Form

        // Boss state tracking for HP-driven intensity
        private float _bossLifeRatio = 1f;
        private Vector2 _bossCenter;
        private bool _bossIsAwakened;
        private float _vignetteStrength;

        private struct CosmicStar
        {
            public Vector2 Position;
            public float Brightness;
            public float PulseSpeed;
            public float PulseOffset;
            public float Scale;
            public Color Tint;
        }

        private CosmicStar[] _stars;
        private const int MaxStars = 80;

        private struct RealityCrack
        {
            public Vector2 Start;
            public Vector2 End;
            public float Width;
            public float Glow;
        }

        private RealityCrack[] _cracks;
        private const int MaxCracks = 12;

        public override void OnLoad() { }

        public override void Activate(Vector2 position, params object[] args)
        {
            _isActive = true;
            Random rand = new Random();
            _stars = new CosmicStar[MaxStars];
            _cracks = new RealityCrack[MaxCracks];

            for (int i = 0; i < MaxStars; i++)
            {
                _stars[i] = new CosmicStar
                {
                    Position = new Vector2(
                        (float)rand.NextDouble() * Main.screenWidth,
                        (float)rand.NextDouble() * Main.screenHeight),
                    Brightness = 0.3f + (float)rand.NextDouble() * 0.7f,
                    PulseSpeed = 0.01f + (float)rand.NextDouble() * 0.03f,
                    PulseOffset = (float)rand.NextDouble() * MathHelper.TwoPi,
                    Scale = 0.2f + (float)rand.NextDouble() * 0.8f,
                    Tint = Color.Lerp(new Color(230, 220, 255), new Color(180, 40, 80),
                        (float)rand.NextDouble() * 0.4f)
                };
            }

            for (int i = 0; i < MaxCracks; i++)
            {
                Vector2 center = new Vector2(
                    (float)rand.NextDouble() * Main.screenWidth,
                    (float)rand.NextDouble() * Main.screenHeight);
                float angle = (float)rand.NextDouble() * MathHelper.TwoPi;
                float length = 40f + (float)rand.NextDouble() * 120f;
                _cracks[i] = new RealityCrack
                {
                    Start = center - angle.ToRotationVector2() * length * 0.5f,
                    End = center + angle.ToRotationVector2() * length * 0.5f,
                    Width = 1f + (float)rand.NextDouble() * 3f,
                    Glow = 0.5f + (float)rand.NextDouble() * 0.5f
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

        private void UpdateBossState()
        {
            _bossLifeRatio = FateSkySystem.BossLifeRatio;
            _bossCenter = FateSkySystem.BossCenter;
            _bossIsAwakened = FateSkySystem.BossIsAwakened;
        }

        private float GetEffectiveIntensity()
        {
            if (_bossCenter == Vector2.Zero) return _opacity;
            Vector2 playerCenter = Main.LocalPlayer.Center;
            float dist = Vector2.Distance(playerCenter, _bossCenter);
            float falloff = MathHelper.SmoothStep(1f, 0.25f, MathHelper.Clamp(dist / 3500f, 0f, 1f));
            return _opacity * falloff;
        }

        public override void Update(GameTime gameTime)
        {
            if (_isActive)
                _opacity = Math.Min(1f, _opacity + 0.015f);
            else
                _opacity = Math.Max(0f, _opacity - 0.015f);

            UpdateBossState();

            // Determine awakened state from BossIndexTracker
            _awakening = BossIndexTracker.FateAwakened ? 
                Math.Min(1f, _awakening + 0.01f) : 
                Math.Max(0f, _awakening - 0.01f);

            // HP-driven vignette: intensifies as boss weakens
            float targetVignette = _bossIsAwakened ? 0.4f : 0.15f;
            float hpDrive = 1f - _bossLifeRatio;
            targetVignette += hpDrive * 0.25f;
            _vignetteStrength += (_vignetteStrength < targetVignette ? 0.003f : -0.003f);
            _vignetteStrength = MathHelper.Clamp(_vignetteStrength, 0f, 0.7f);

            // Animate stars — HP-driven pulse speed
            float pulseMult = 1f + hpDrive * 0.8f;
            if (_stars != null)
            {
                for (int i = 0; i < MaxStars; i++)
                {
                    float pulse = (float)Math.Sin(Main.timeForVisualEffects * _stars[i].PulseSpeed * pulseMult + _stars[i].PulseOffset);
                    _stars[i].Brightness = 0.3f + pulse * 0.35f + _awakening * 0.3f + hpDrive * 0.2f;

                    // In True Form, stars drift toward center — faster at low HP
                    if (_awakening > 0.1f)
                    {
                        Vector2 center = new Vector2(Main.screenWidth * 0.5f, Main.screenHeight * 0.5f);
                        float driftSpeed = 0.0003f + hpDrive * 0.0004f;
                        Vector2 toCenter = (center - _stars[i].Position) * driftSpeed * _awakening;
                        _stars[i].Position += toCenter;
                    }
                }
            }
        }

        public override void Draw(SpriteBatch spriteBatch, float minDepth, float maxDepth)
        {
            if (maxDepth >= 0f && minDepth < 0f)
            {
                Texture2D pixel = Terraria.GameContent.TextureAssets.MagicPixel.Value;

                // Cosmic void gradient — deep black with faint purple undertone
                Color topColor = Color.Lerp(new Color(5, 2, 10), new Color(30, 10, 40), _awakening);
                Color bottomColor = Color.Lerp(new Color(10, 5, 20), new Color(60, 15, 50), _awakening);
                topColor *= _opacity * 0.85f;
                bottomColor *= _opacity * 0.85f;

                for (int y = 0; y < Main.screenHeight; y += 4)
                {
                    float t = (float)y / Main.screenHeight;
                    Color lineColor = Color.Lerp(topColor, bottomColor, t);
                    spriteBatch.Draw(pixel, new Rectangle(0, y, Main.screenWidth, 4), lineColor);
                }

                // Draw stars — HP-driven visibility (more visible at low HP)
                float hpDrive = 1f - _bossLifeRatio;
                int visibleStars = (int)(MaxStars * (0.6f + hpDrive * 0.4f));
                if (_stars != null && _opacity > 0.1f)
                {
                    for (int i = 0; i < visibleStars; i++)
                    {
                        Color c = _stars[i].Tint * (_opacity * _stars[i].Brightness);
                        c.A = 0; // Additive
                        float s = _stars[i].Scale * (1f + _awakening * 0.5f + hpDrive * 0.3f);
                        spriteBatch.Draw(pixel,
                            _stars[i].Position,
                            new Rectangle(0, 0, 1, 1),
                            c,
                            0f,
                            new Vector2(0.5f),
                            s * 2.5f,
                            SpriteEffects.None, 0f);
                    }
                }

                // Reality cracks — only visible during True Form
                if (_cracks != null && _awakening > 0.1f && _opacity > 0.1f)
                {
                    for (int i = 0; i < MaxCracks; i++)
                    {
                        float crackAlpha = _awakening * _opacity * _cracks[i].Glow;
                        float pulse = (float)Math.Sin(Main.timeForVisualEffects * 0.03f + i * 0.7f) * 0.3f + 0.7f;
                        Color crackColor = Color.Lerp(new Color(220, 40, 60), new Color(230, 220, 255), pulse) * crackAlpha;
                        crackColor.A = 0;

                        Vector2 dir = (_cracks[i].End - _cracks[i].Start);
                        float length = dir.Length();
                        dir.Normalize();
                        float rotation = (float)Math.Atan2(dir.Y, dir.X);

                        spriteBatch.Draw(pixel,
                            _cracks[i].Start,
                            new Rectangle(0, 0, 1, 1),
                            crackColor,
                            rotation,
                            Vector2.Zero,
                            new Vector2(length, _cracks[i].Width * (1f + _awakening)),
                            SpriteEffects.None, 0f);
                    }
                }

                // Vignette rendering — cosmic void creeping from edges
                if (_vignetteStrength > 0.01f)
                {
                    float eff = GetEffectiveIntensity();
                    for (int ring = 0; ring < 12; ring++)
                    {
                        float t = ring / 12f;
                        float alpha = t * t * _vignetteStrength * eff;
                        Color vigColor = Color.Lerp(new Color(15, 5, 20), new Color(180, 50, 100), _awakening * 0.3f) * alpha;
                        int inset = (int)(Main.screenWidth * 0.5f * (1f - t));
                        int insetY = (int)(Main.screenHeight * 0.5f * (1f - t));
                        spriteBatch.Draw(pixel, new Rectangle(0, 0, Main.screenWidth, insetY), vigColor);
                        spriteBatch.Draw(pixel, new Rectangle(0, Main.screenHeight - insetY, Main.screenWidth, insetY), vigColor);
                        spriteBatch.Draw(pixel, new Rectangle(0, 0, inset, Main.screenHeight), vigColor);
                        spriteBatch.Draw(pixel, new Rectangle(Main.screenWidth - inset, 0, inset, Main.screenHeight), vigColor);
                    }
                }
            }
        }

        public override Color OnTileColor(Color inColor)
        {
            float eff = GetEffectiveIntensity();
            float tintStrength = _bossIsAwakened ? 0.5f : 0.35f;
            Color tint = Color.Lerp(Color.White,
                new Color(180, 160, 200), eff * tintStrength);
            return inColor.MultiplyRGBA(tint);
        }

        public override float GetCloudAlpha() => 1f - _opacity * 0.8f;
    }

    /// <summary>
    /// Companion ModSystem for FateSky — feeds boss state and provides cosmic flash APIs.
    /// </summary>
    public class FateSkySystem : ModSystem
    {
        // Boss state fed from main boss AI
        public static float BossLifeRatio { get; set; } = 1f;
        public static Vector2 BossCenter { get; set; }
        public static bool BossIsAwakened { get; set; }

        // Flash system
        private static float _flashIntensity;
        private static Color _flashColor;
        private static float _flashDecay;

        public static void TriggerCosmicFlash(float intensity)
        {
            _flashIntensity = intensity;
            _flashColor = new Color(180, 50, 100); // Dark pink cosmic
            _flashDecay = 0.92f;
        }

        public static void TriggerCrimsonFlash(float intensity)
        {
            _flashIntensity = intensity;
            _flashColor = new Color(255, 60, 80); // Bright crimson
            _flashDecay = 0.90f;
        }

        public static void TriggerCelestialFlash(float intensity)
        {
            _flashIntensity = intensity;
            _flashColor = new Color(230, 220, 255); // Celestial white
            _flashDecay = 0.88f;
        }

        public static void TriggerSupernovaFlash(float intensity)
        {
            _flashIntensity = intensity;
            _flashColor = Color.White;
            _flashDecay = 0.85f;
        }

        public static (float intensity, Color color) GetFlashState() => (_flashIntensity, _flashColor);

        public override void PostUpdateEverything()
        {
            // Decay flash
            if (_flashIntensity > 0.01f)
                _flashIntensity *= _flashDecay;
            else
                _flashIntensity = 0f;

            // Ambient world dust around boss — cosmic motes + crimson sparks
            if (BossCenter != Vector2.Zero && BossLifeRatio > 0f)
            {
                float hpDrive = 1f - BossLifeRatio;
                int moteChance = Math.Max(1, (int)(12 - hpDrive * 8));
                if (Main.rand.NextBool(moteChance))
                {
                    Vector2 offset = Main.rand.NextVector2Circular(200f, 200f);
                    Dust d = Dust.NewDustPerfect(BossCenter + offset, DustID.PinkTorch,
                        new Vector2(0f, Main.rand.NextFloat(-1f, -0.3f)), 0, default, 0.8f);
                    d.noGravity = true;
                    d.fadeIn = 1.2f;
                }

                if (BossIsAwakened && Main.rand.NextBool(Math.Max(1, (int)(8 - hpDrive * 5))))
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
