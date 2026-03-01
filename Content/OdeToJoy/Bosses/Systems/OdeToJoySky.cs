using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.Bosses;

namespace MagnumOpus.Content.OdeToJoy.Bosses.Systems
{
    /// <summary>
    /// Custom sky effect for the Ode to Joy boss fight.
    /// Creates a warm golden garden atmosphere with floating petals.
    /// Phase 1: Sunlit garden with gentle drifting petals.
    /// Phase 2: Chromatic light show with radiant beams and blooming flower patterns.
    /// </summary>
    public class OdeToJoySky : CustomSky
    {
        private bool _isActive;
        private float _opacity;
        private float _chromaticIntensity; // 0 = Phase 1, 1 = Phase 2

        private struct GardenPetal
        {
            public Vector2 Position;
            public float Speed;
            public float Scale;
            public Color Color;
            public float Rotation;
            public float RotSpeed;
            public float SwayOffset;
        }

        private GardenPetal[] _petals;
        private const int MaxPetals = 50;

        private struct LightBeam
        {
            public float Angle;
            public float Width;
            public float Length;
            public Color Color;
            public float PulseOffset;
        }

        private LightBeam[] _beams;
        private const int MaxBeams = 6;

        public override void OnLoad() { }

        public override void Activate(Vector2 position, params object[] args)
        {
            _isActive = true;
            Random rand = new Random();
            _petals = new GardenPetal[MaxPetals];
            _beams = new LightBeam[MaxBeams];

            Color[] petalColors = {
                new Color(255, 200, 50),   // Gold
                new Color(240, 160, 40),   // Amber
                new Color(230, 130, 150),  // Rose
                new Color(255, 240, 200)   // Light
            };

            for (int i = 0; i < MaxPetals; i++)
            {
                _petals[i] = new GardenPetal
                {
                    Position = new Vector2(
                        (float)rand.NextDouble() * Main.screenWidth,
                        (float)rand.NextDouble() * Main.screenHeight),
                    Speed = 0.3f + (float)rand.NextDouble() * 1.0f,
                    Scale = 0.3f + (float)rand.NextDouble() * 0.6f,
                    Color = petalColors[rand.Next(petalColors.Length)],
                    Rotation = (float)rand.NextDouble() * MathHelper.TwoPi,
                    RotSpeed = ((float)rand.NextDouble() - 0.5f) * 0.04f,
                    SwayOffset = (float)rand.NextDouble() * MathHelper.TwoPi
                };
            }

            for (int i = 0; i < MaxBeams; i++)
            {
                _beams[i] = new LightBeam
                {
                    Angle = (float)rand.NextDouble() * MathHelper.Pi * 0.6f - MathHelper.Pi * 0.3f,
                    Width = 20f + (float)rand.NextDouble() * 40f,
                    Length = Main.screenHeight * 0.8f + (float)rand.NextDouble() * Main.screenHeight * 0.4f,
                    Color = petalColors[rand.Next(petalColors.Length)],
                    PulseOffset = (float)rand.NextDouble() * MathHelper.TwoPi
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
                _opacity = Math.Min(1f, _opacity + 0.02f);
            else
                _opacity = Math.Max(0f, _opacity - 0.02f);

            // Phase 2 chromatic intensity
            float targetChromatic = BossIndexTracker.OdeToJoyPhase >= 2 ? 1f : 0f;
            _chromaticIntensity += (targetChromatic - _chromaticIntensity) * 0.02f;

            // Update petals — gentle descent with horizontal sway
            if (_petals != null)
            {
                Random rand = new Random((int)(Main.timeForVisualEffects * 0.05f));
                for (int i = 0; i < MaxPetals; i++)
                {
                    _petals[i].Position.Y += _petals[i].Speed;
                    _petals[i].Position.X += (float)Math.Sin(
                        Main.timeForVisualEffects * 0.008f + _petals[i].SwayOffset) * 0.5f;
                    _petals[i].Rotation += _petals[i].RotSpeed;

                    if (_petals[i].Position.Y > Main.screenHeight + 20)
                    {
                        _petals[i].Position.Y = -20;
                        _petals[i].Position.X = (float)rand.NextDouble() * Main.screenWidth;
                    }
                }
            }
        }

        public override void Draw(SpriteBatch spriteBatch, float minDepth, float maxDepth)
        {
            if (maxDepth >= 0f && minDepth < 0f)
            {
                Texture2D pixel = Terraria.GameContent.TextureAssets.MagicPixel.Value;

                // Sky gradient — warm golden light overhead, bright horizon
                Color topColor = Color.Lerp(new Color(80, 60, 20), new Color(100, 50, 70), _chromaticIntensity);
                Color bottomColor = Color.Lerp(new Color(120, 90, 30), new Color(150, 100, 80), _chromaticIntensity);
                topColor *= _opacity * 0.6f;
                bottomColor *= _opacity * 0.6f;

                for (int y = 0; y < Main.screenHeight; y += 4)
                {
                    float t = (float)y / Main.screenHeight;
                    Color lineColor = Color.Lerp(topColor, bottomColor, t);
                    spriteBatch.Draw(pixel, new Rectangle(0, y, Main.screenWidth, 4), lineColor);
                }

                // Light beams — gentle in Phase 1, chromatic in Phase 2
                if (_beams != null && _opacity > 0.1f)
                {
                    for (int i = 0; i < MaxBeams; i++)
                    {
                        float pulse = (float)Math.Sin(Main.timeForVisualEffects * 0.01f + _beams[i].PulseOffset);
                        float beamAlpha = _opacity * (0.05f + pulse * 0.03f + _chromaticIntensity * 0.06f);
                        Color beamColor;

                        if (_chromaticIntensity > 0.3f)
                        {
                            float hue = ((float)Main.timeForVisualEffects * 0.002f + i * 0.15f) % 1f;
                            beamColor = Main.hslToRgb(hue, 0.7f, 0.6f) * beamAlpha;
                        }
                        else
                        {
                            beamColor = _beams[i].Color * beamAlpha;
                        }
                        beamColor.A = 0;

                        float x = Main.screenWidth * (0.15f + i * 0.14f);
                        spriteBatch.Draw(pixel,
                            new Vector2(x, 0),
                            new Rectangle(0, 0, 1, 1),
                            beamColor,
                            _beams[i].Angle,
                            Vector2.Zero,
                            new Vector2(_beams[i].Width, _beams[i].Length),
                            SpriteEffects.None, 0f);
                    }
                }

                // Floating petals
                if (_petals != null && _opacity > 0.1f)
                {
                    for (int i = 0; i < MaxPetals; i++)
                    {
                        Color c = _petals[i].Color * _opacity * 0.5f;

                        // Phase 2: rainbow-shift petal colors
                        if (_chromaticIntensity > 0.3f)
                        {
                            float hue = ((float)Main.timeForVisualEffects * 0.003f + i * 0.05f) % 1f;
                            c = Main.hslToRgb(hue, 0.7f, 0.6f) * (_opacity * 0.5f * _chromaticIntensity);
                        }
                        c.A = 0;

                        float s = _petals[i].Scale * (1f + _chromaticIntensity * 0.3f);
                        spriteBatch.Draw(pixel,
                            _petals[i].Position,
                            new Rectangle(0, 0, 1, 1),
                            c,
                            _petals[i].Rotation,
                            new Vector2(0.5f),
                            s * 3f,
                            SpriteEffects.None, 0f);
                    }
                }
            }
        }

        public override Color OnTileColor(Color inColor)
        {
            Color tint = Color.Lerp(Color.White,
                new Color(255, 230, 180), _opacity * 0.2f * (1f + _chromaticIntensity * 0.2f));
            return inColor.MultiplyRGBA(tint);
        }

        public override float GetCloudAlpha() => 1f - _opacity * 0.5f;
    }
}
