using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.Bosses;

namespace MagnumOpus.Content.SwanLake.Bosses.Systems
{
    /// <summary>
    /// Custom sky effect for the Swan Lake boss fight.
    /// Transitions between three moods:
    ///   Graceful: elegant silver moonlight, drifting feather motes
    ///   Tempest: stormy prismatic lightning flashes, swirling clouds
    ///   DyingSwan: monochrome fading to white, final rainbow shatter
    /// Uses BossIndexTracker.SwanLakeMood to determine current mood.
    /// </summary>
    public class SwanLakeSky : CustomSky
    {
        private bool _isActive;
        private float _opacity;
        private int _currentMood; // 0=Graceful, 1=Tempest, 2=DyingSwan
        private float _moodBlend; // 0-1 transition progress
        private float _intensity;

        private struct SkyFeather
        {
            public Vector2 Position;
            public float Speed;
            public float Scale;
            public float Rotation;
            public float RotSpeed;
            public float SwayPhase;
            public bool IsBlack; // black vs white feather
        }

        private SkyFeather[] _feathers;
        private const int MaxFeathers = 50;

        // Lightning flash for Tempest mood
        private float _lightningTimer;
        private float _lightningIntensity;

        public override void OnLoad() { }

        public override void Activate(Vector2 position, params object[] args)
        {
            _isActive = true;
            _feathers = new SkyFeather[MaxFeathers];
            Random rand = new Random();

            for (int i = 0; i < MaxFeathers; i++)
            {
                _feathers[i] = new SkyFeather
                {
                    Position = new Vector2(
                        (float)rand.NextDouble() * Main.screenWidth,
                        (float)rand.NextDouble() * Main.screenHeight),
                    Speed = 0.2f + (float)rand.NextDouble() * 0.8f,
                    Scale = 0.3f + (float)rand.NextDouble() * 0.5f,
                    Rotation = (float)rand.NextDouble() * MathHelper.TwoPi,
                    RotSpeed = ((float)rand.NextDouble() - 0.5f) * 0.02f,
                    SwayPhase = (float)rand.NextDouble() * MathHelper.TwoPi,
                    IsBlack = rand.NextDouble() > 0.7 // 30% black feathers
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

            NPC boss = BossIndexTracker.GetActiveBoss(BossIndexTracker.SwanLakeFractal);
            if (boss != null)
            {
                float hpRatio = boss.life / (float)boss.lifeMax;
                _intensity = 1f - hpRatio;

                // Track mood transitions
                int targetMood = BossIndexTracker.SwanLakeMood;
                if (targetMood != _currentMood)
                {
                    _moodBlend += 0.01f;
                    if (_moodBlend >= 1f)
                    {
                        _currentMood = targetMood;
                        _moodBlend = 0f;
                    }
                }

                // Tempest lightning flashes
                if (_currentMood == 1)
                {
                    _lightningTimer -= 1f;
                    if (_lightningTimer <= 0)
                    {
                        _lightningIntensity = 0.5f + Main.rand.NextFloat(0.5f);
                        _lightningTimer = 30f + Main.rand.NextFloat(90f);
                    }
                    _lightningIntensity *= 0.92f;
                }
                else
                {
                    _lightningIntensity *= 0.95f;
                }

                // Update feather drift
                if (_feathers != null)
                {
                    float speedMult = _currentMood == 1 ? 2.5f : (_currentMood == 2 ? 0.5f : 1f);
                    for (int i = 0; i < MaxFeathers; i++)
                    {
                        _feathers[i].Position.Y += _feathers[i].Speed * speedMult;
                        _feathers[i].Position.X += (float)Math.Sin(Main.timeForVisualEffects * 0.008f + _feathers[i].SwayPhase) * 0.5f * speedMult;
                        _feathers[i].Rotation += _feathers[i].RotSpeed;

                        if (_feathers[i].Position.Y > Main.screenHeight + 20)
                        {
                            _feathers[i].Position.Y = -20;
                            _feathers[i].Position.X = Main.rand.NextFloat() * Main.screenWidth;
                        }
                    }
                }
            }
        }

        public override void Draw(SpriteBatch spriteBatch, float minDepth, float maxDepth)
        {
            if (maxDepth >= 0f && minDepth < 0f)
            {
                Texture2D pixel = Terraria.GameContent.TextureAssets.MagicPixel.Value;

                // Mood-dependent sky colors
                Color topColor, bottomColor;

                switch (_currentMood)
                {
                    case 0: // Graceful: silver moonlight
                        topColor = Color.Lerp(new Color(20, 20, 35), new Color(40, 40, 60), _intensity);
                        bottomColor = Color.Lerp(new Color(50, 55, 70), new Color(100, 105, 130), _intensity);
                        break;
                    case 1: // Tempest: stormy dark
                        topColor = Color.Lerp(new Color(15, 15, 25), new Color(30, 20, 40), _intensity);
                        bottomColor = Color.Lerp(new Color(30, 30, 45), new Color(60, 50, 80), _intensity);
                        break;
                    default: // DyingSwan: fading to white
                        float fade = _intensity * 0.5f;
                        topColor = Color.Lerp(new Color(40, 40, 50), new Color(180, 180, 190), fade);
                        bottomColor = Color.Lerp(new Color(60, 60, 70), new Color(220, 220, 230), fade);
                        break;
                }

                topColor *= _opacity * 0.7f;
                bottomColor *= _opacity * 0.7f;

                // Draw gradient
                for (int y = 0; y < Main.screenHeight; y += 4)
                {
                    float t = (float)y / Main.screenHeight;
                    Color lineColor = Color.Lerp(topColor, bottomColor, t);
                    spriteBatch.Draw(pixel, new Rectangle(0, y, Main.screenWidth, 4), lineColor);
                }

                // Lightning flash overlay (Tempest)
                if (_lightningIntensity > 0.01f)
                {
                    float hue = ((float)Main.timeForVisualEffects * 0.01f) % 1f;
                    Color flashColor = Main.hslToRgb(hue, 0.3f, 0.9f) * _opacity * _lightningIntensity * 0.3f;
                    flashColor.A = 0;
                    spriteBatch.Draw(pixel, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), flashColor);
                }

                // DyingSwan white fade overlay
                if (_currentMood == 2 && _intensity > 0.5f)
                {
                    float whiteFade = (_intensity - 0.5f) * 2f * 0.15f;
                    Color whiteOverlay = Color.White * _opacity * whiteFade;
                    whiteOverlay.A = (byte)(whiteOverlay.A * 0.5f);
                    spriteBatch.Draw(pixel, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), whiteOverlay);
                }

                // Draw feather particles
                if (_feathers != null && _opacity > 0.1f)
                {
                    for (int i = 0; i < MaxFeathers; i++)
                    {
                        Color c;
                        if (_currentMood == 2)
                        {
                            // DyingSwan: feathers fade to grey
                            c = new Color(150, 150, 155) * _opacity * 0.4f;
                        }
                        else
                        {
                            c = _feathers[i].IsBlack
                                ? new Color(20, 20, 25) * _opacity * 0.5f
                                : new Color(230, 230, 240) * _opacity * 0.4f;
                        }
                        c.A = 0;
                        float s = _feathers[i].Scale * (1f + _intensity * 0.3f);
                        spriteBatch.Draw(pixel,
                            _feathers[i].Position,
                            new Rectangle(0, 0, 1, 1),
                            c,
                            _feathers[i].Rotation,
                            new Vector2(0.5f),
                            new Vector2(s * 2f, s * 4f), // Elongated like feathers
                            SpriteEffects.None, 0f);
                    }
                }
            }
        }

        public override Color OnTileColor(Color inColor)
        {
            Color tint = _currentMood switch
            {
                0 => Color.Lerp(Color.White, new Color(210, 215, 240), _opacity * 0.25f),
                1 => Color.Lerp(Color.White, new Color(200, 200, 220), _opacity * 0.3f),
                _ => Color.Lerp(Color.White, new Color(230, 230, 235), _opacity * 0.2f)
            };
            return inColor.MultiplyRGBA(tint);
        }

        public override float GetCloudAlpha() => 1f - _opacity * (_currentMood == 1 ? 0.8f : 0.5f);
    }
}
