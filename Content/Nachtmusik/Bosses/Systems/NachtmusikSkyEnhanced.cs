using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.Bosses;

namespace MagnumOpus.Content.Nachtmusik.Bosses.Systems
{
    /// <summary>
    /// Enhanced sky effect for the Nachtmusik boss fight.
    /// Provides more dramatic visuals than the base NachtmusikCelestialSky,
    /// with Phase 1 serene starlit night and Phase 2 violent cosmic storm.
    /// Uses BossIndexTracker.NachtmusikPhase to drive visual intensity.
    /// </summary>
    public class NachtmusikSkyEnhanced : CustomSky
    {
        private bool _isActive;
        private float _opacity;
        private float _stormIntensity; // 0 = serene, 1 = cosmic storm

        private struct NightStar
        {
            public Vector2 Position;
            public float Brightness;
            public float PulseSpeed;
            public float PulseOffset;
            public float Scale;
            public Color Tint;
        }

        private NightStar[] _stars;
        private const int MaxStars = 70;

        private struct ShootingStar
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Life;
            public float MaxLife;
            public Color Color;
        }

        private ShootingStar[] _shootingStars;
        private const int MaxShootingStars = 8;

        public override void OnLoad() { }

        public override void Activate(Vector2 position, params object[] args)
        {
            _isActive = true;
            Random rand = new Random();
            _stars = new NightStar[MaxStars];
            _shootingStars = new ShootingStar[MaxShootingStars];

            for (int i = 0; i < MaxStars; i++)
            {
                _stars[i] = new NightStar
                {
                    Position = new Vector2(
                        (float)rand.NextDouble() * Main.screenWidth,
                        (float)rand.NextDouble() * Main.screenHeight),
                    Brightness = 0.2f + (float)rand.NextDouble() * 0.8f,
                    PulseSpeed = 0.008f + (float)rand.NextDouble() * 0.02f,
                    PulseOffset = (float)rand.NextDouble() * MathHelper.TwoPi,
                    Scale = 0.2f + (float)rand.NextDouble() * 0.7f,
                    Tint = Color.Lerp(new Color(200, 210, 230), new Color(80, 120, 200),
                        (float)rand.NextDouble() * 0.5f)
                };
            }

            for (int i = 0; i < MaxShootingStars; i++)
                ResetShootingStar(ref _shootingStars[i], rand);
        }

        private void ResetShootingStar(ref ShootingStar star, Random rand)
        {
            star.Position = new Vector2(
                (float)rand.NextDouble() * Main.screenWidth,
                (float)rand.NextDouble() * Main.screenHeight * 0.5f);
            float angle = MathHelper.ToRadians(120f + (float)rand.NextDouble() * 60f);
            float speed = 2f + (float)rand.NextDouble() * 4f + _stormIntensity * 6f;
            star.Velocity = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * speed;
            star.MaxLife = 30f + (float)rand.NextDouble() * 40f;
            star.Life = star.MaxLife;
            star.Color = Color.Lerp(new Color(200, 210, 230), new Color(220, 180, 100),
                (float)rand.NextDouble() * _stormIntensity);
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

            // Phase 2 = storm intensity
            float targetStorm = BossIndexTracker.NachtmusikPhase >= 2 ? 1f : 0f;
            _stormIntensity += (targetStorm - _stormIntensity) * 0.02f;

            // Update stars
            if (_stars != null)
            {
                for (int i = 0; i < MaxStars; i++)
                {
                    float pulse = (float)Math.Sin(Main.timeForVisualEffects * _stars[i].PulseSpeed + _stars[i].PulseOffset);
                    _stars[i].Brightness = 0.2f + (pulse * 0.3f + 0.3f) + _stormIntensity * 0.3f;

                    // Storm: stars flicker rapidly
                    if (_stormIntensity > 0.5f)
                    {
                        float flicker = (float)Math.Sin(Main.timeForVisualEffects * 0.2f + i * 1.3f);
                        _stars[i].Brightness *= 0.7f + flicker * 0.3f;
                    }
                }
            }

            // Update shooting stars
            if (_shootingStars != null)
            {
                Random rand = new Random((int)(Main.timeForVisualEffects * 0.01f) + 42);
                for (int i = 0; i < MaxShootingStars; i++)
                {
                    _shootingStars[i].Position += _shootingStars[i].Velocity;
                    _shootingStars[i].Life -= 1f;
                    if (_shootingStars[i].Life <= 0 ||
                        _shootingStars[i].Position.X < -50 || _shootingStars[i].Position.X > Main.screenWidth + 50 ||
                        _shootingStars[i].Position.Y > Main.screenHeight + 50)
                    {
                        ResetShootingStar(ref _shootingStars[i], rand);
                    }
                }
            }
        }

        public override void Draw(SpriteBatch spriteBatch, float minDepth, float maxDepth)
        {
            if (maxDepth >= 0f && minDepth < 0f)
            {
                Texture2D pixel = Terraria.GameContent.TextureAssets.MagicPixel.Value;

                // Sky gradient — deep indigo night, intensifying to nebula purple in Phase 2
                Color topColor = Color.Lerp(new Color(10, 8, 30), new Color(40, 15, 60), _stormIntensity);
                Color bottomColor = Color.Lerp(new Color(20, 15, 50), new Color(60, 30, 90), _stormIntensity);
                topColor *= _opacity * 0.8f;
                bottomColor *= _opacity * 0.8f;

                for (int y = 0; y < Main.screenHeight; y += 4)
                {
                    float t = (float)y / Main.screenHeight;
                    Color lineColor = Color.Lerp(topColor, bottomColor, t);
                    spriteBatch.Draw(pixel, new Rectangle(0, y, Main.screenWidth, 4), lineColor);
                }

                float alpha = _opacity;

                // Stars
                if (_stars != null && alpha > 0.1f)
                {
                    for (int i = 0; i < MaxStars; i++)
                    {
                        Color c = _stars[i].Tint * (alpha * _stars[i].Brightness);
                        c.A = 0;
                        float s = _stars[i].Scale * (1f + _stormIntensity * 0.3f);
                        spriteBatch.Draw(pixel,
                            _stars[i].Position,
                            new Rectangle(0, 0, 1, 1),
                            c, 0f, new Vector2(0.5f),
                            s * 2.5f, SpriteEffects.None, 0f);
                    }
                }

                // Shooting stars
                if (_shootingStars != null && alpha > 0.1f)
                {
                    for (int i = 0; i < MaxShootingStars; i++)
                    {
                        float lifeRatio = _shootingStars[i].Life / _shootingStars[i].MaxLife;
                        if (lifeRatio <= 0) continue;
                        Color c = _shootingStars[i].Color * (alpha * lifeRatio * 0.8f);
                        c.A = 0;
                        Vector2 dir = _shootingStars[i].Velocity;
                        dir.Normalize();
                        float rot = (float)Math.Atan2(dir.Y, dir.X);
                        float tailLen = 8f + (1f - lifeRatio) * 15f + _stormIntensity * 10f;

                        spriteBatch.Draw(pixel,
                            _shootingStars[i].Position,
                            new Rectangle(0, 0, 1, 1),
                            c, rot, Vector2.Zero,
                            new Vector2(tailLen, 1.5f),
                            SpriteEffects.None, 0f);
                    }
                }

                // Phase 2: nebula cloud overlay
                if (_stormIntensity > 0.1f && alpha > 0.1f)
                {
                    float nebulaAlpha = _stormIntensity * alpha * 0.15f;
                    float time = (float)Main.timeForVisualEffects * 0.001f;

                    for (int i = 0; i < 5; i++)
                    {
                        float x = (float)Math.Sin(time + i * 1.2f) * Main.screenWidth * 0.3f + Main.screenWidth * 0.5f;
                        float y = (float)Math.Cos(time * 0.7f + i * 0.9f) * Main.screenHeight * 0.2f + Main.screenHeight * 0.3f;
                        Color nebulaColor = Color.Lerp(new Color(80, 120, 200), new Color(220, 180, 100),
                            (float)Math.Sin(time + i) * 0.5f + 0.5f) * nebulaAlpha;
                        nebulaColor.A = 0;

                        spriteBatch.Draw(pixel,
                            new Vector2(x, y),
                            new Rectangle(0, 0, 1, 1),
                            nebulaColor, time * 0.5f + i,
                            new Vector2(0.5f),
                            80f + i * 20f,
                            SpriteEffects.None, 0f);
                    }
                }
            }
        }

        public override Color OnTileColor(Color inColor)
        {
            Color tint = Color.Lerp(Color.White,
                Color.Lerp(new Color(180, 190, 220), new Color(160, 140, 200), _stormIntensity),
                _opacity * 0.25f);
            return inColor.MultiplyRGBA(tint);
        }

        public override float GetCloudAlpha() => 1f - _opacity * 0.7f;
    }
}
