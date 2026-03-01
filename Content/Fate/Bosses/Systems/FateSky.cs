using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.Bosses;

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

        public override void Update(GameTime gameTime)
        {
            if (_isActive)
                _opacity = Math.Min(1f, _opacity + 0.015f);
            else
                _opacity = Math.Max(0f, _opacity - 0.015f);

            // Determine awakened state from BossIndexTracker
            _awakening = BossIndexTracker.FateAwakened ? 
                Math.Min(1f, _awakening + 0.01f) : 
                Math.Max(0f, _awakening - 0.01f);

            // Animate stars
            if (_stars != null)
            {
                for (int i = 0; i < MaxStars; i++)
                {
                    float pulse = (float)Math.Sin(Main.timeForVisualEffects * _stars[i].PulseSpeed + _stars[i].PulseOffset);
                    _stars[i].Brightness = 0.3f + pulse * 0.35f + _awakening * 0.3f;

                    // In True Form, stars drift slowly toward center
                    if (_awakening > 0.1f)
                    {
                        Vector2 center = new Vector2(Main.screenWidth * 0.5f, Main.screenHeight * 0.5f);
                        Vector2 toCenter = (center - _stars[i].Position) * 0.0003f * _awakening;
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

                // Draw stars
                if (_stars != null && _opacity > 0.1f)
                {
                    for (int i = 0; i < MaxStars; i++)
                    {
                        Color c = _stars[i].Tint * (_opacity * _stars[i].Brightness);
                        c.A = 0; // Additive
                        float s = _stars[i].Scale * (1f + _awakening * 0.5f);
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
            }
        }

        public override Color OnTileColor(Color inColor)
        {
            Color tint = Color.Lerp(Color.White,
                new Color(200, 180, 220), _opacity * 0.3f * (1f + _awakening * 0.3f));
            return inColor.MultiplyRGBA(tint);
        }

        public override float GetCloudAlpha() => 1f - _opacity * 0.8f;
    }
}
