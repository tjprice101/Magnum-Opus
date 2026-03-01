using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.Bosses;

namespace MagnumOpus.Content.Winter.Bosses.Systems
{
    /// <summary>
    /// Custom sky effect for the L'Inverno boss fight.
    /// Creates a cold winter sky with falling snow particles,
    /// frost crystallization at screen edges, and blizzard
    /// intensity that increases with phases.
    /// </summary>
    public class LInvernoSky : CustomSky
    {
        private bool _isActive;
        private float _opacity;
        private float _intensity;

        private struct Snowflake
        {
            public Vector2 Position;
            public float FallSpeed;
            public float DriftPhase;
            public float DriftAmplitude;
            public float Scale;
            public float Alpha;
        }

        private Snowflake[] _snowflakes;
        private const int MaxSnowflakes = 100;

        public override void OnLoad() { }

        public override void Activate(Vector2 position, params object[] args)
        {
            _isActive = true;
            _snowflakes = new Snowflake[MaxSnowflakes];
            Random rand = new Random();

            for (int i = 0; i < MaxSnowflakes; i++)
            {
                _snowflakes[i] = new Snowflake
                {
                    Position = new Vector2(
                        (float)rand.NextDouble() * Main.screenWidth,
                        (float)rand.NextDouble() * Main.screenHeight),
                    FallSpeed = 0.3f + (float)rand.NextDouble() * 1.8f,
                    DriftPhase = (float)rand.NextDouble() * MathHelper.TwoPi,
                    DriftAmplitude = 10f + (float)rand.NextDouble() * 25f,
                    Scale = 0.2f + (float)rand.NextDouble() * 0.8f,
                    Alpha = 0.4f + (float)rand.NextDouble() * 0.6f
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

            NPC boss = BossIndexTracker.GetActiveBoss(BossIndexTracker.LInverno);
            if (boss != null)
            {
                float hpRatio = boss.life / (float)boss.lifeMax;
                _intensity = 1f - hpRatio;

                if (_snowflakes != null)
                {
                    float time = (float)Main.timeForVisualEffects;
                    // Blizzard wind increases with intensity
                    float windStrength = 0.2f + _intensity * 1.5f;

                    for (int i = 0; i < MaxSnowflakes; i++)
                    {
                        // Snow falls with increasing blizzard speed
                        _snowflakes[i].Position.Y += _snowflakes[i].FallSpeed * (1f + _intensity * 1.5f);

                        // Sideways drift with blizzard wind
                        _snowflakes[i].Position.X += (float)Math.Sin(time * 0.005f + _snowflakes[i].DriftPhase)
                            * _snowflakes[i].DriftAmplitude * 0.01f;
                        _snowflakes[i].Position.X -= windStrength;

                        if (_snowflakes[i].Position.Y > Main.screenHeight + 20)
                        {
                            _snowflakes[i].Position.Y = -10;
                            _snowflakes[i].Position.X = Main.rand.NextFloat() * Main.screenWidth;
                        }
                        if (_snowflakes[i].Position.X < -30)
                            _snowflakes[i].Position.X = Main.screenWidth + 20;
                    }
                }
            }
        }

        public override void Draw(SpriteBatch spriteBatch, float minDepth, float maxDepth)
        {
            if (maxDepth >= 0f && minDepth < 0f)
            {
                // Sky gradient: deep blue-indigo at top, pale frost at horizon
                Color topColor = Color.Lerp(new Color(15, 20, 45), new Color(30, 40, 70), _intensity);
                Color bottomColor = Color.Lerp(new Color(50, 60, 90), new Color(100, 120, 160), _intensity);

                topColor *= _opacity * 0.8f;
                bottomColor *= _opacity * 0.8f;

                Texture2D pixel = Terraria.GameContent.TextureAssets.MagicPixel.Value;

                for (int y = 0; y < Main.screenHeight; y += 4)
                {
                    float t = (float)y / Main.screenHeight;
                    Color lineColor = Color.Lerp(topColor, bottomColor, t);
                    spriteBatch.Draw(pixel, new Rectangle(0, y, Main.screenWidth, 4), lineColor);
                }

                // Frost crystallization at screen edges — gets thicker with intensity
                if (_opacity > 0.2f)
                {
                    float edgeThickness = 20f + _intensity * 60f;
                    Color frostEdge = new Color(180, 210, 240) * _opacity * (0.1f + _intensity * 0.15f);
                    frostEdge.A = 0;

                    // Top edge frost
                    for (int x = 0; x < Main.screenWidth; x += 6)
                    {
                        float variation = (float)Math.Sin(x * 0.02f + (float)Main.timeForVisualEffects * 0.001f) * 0.4f + 0.6f;
                        int height = (int)(edgeThickness * variation);
                        Color c = frostEdge * variation;
                        spriteBatch.Draw(pixel, new Rectangle(x, 0, 6, height), c);
                    }

                    // Bottom edge frost
                    for (int x = 0; x < Main.screenWidth; x += 6)
                    {
                        float variation = (float)Math.Sin(x * 0.025f + 2f + (float)Main.timeForVisualEffects * 0.0008f) * 0.4f + 0.6f;
                        int height = (int)(edgeThickness * 0.7f * variation);
                        Color c = frostEdge * variation * 0.7f;
                        spriteBatch.Draw(pixel, new Rectangle(x, Main.screenHeight - height, 6, height), c);
                    }

                    // Side edge frost
                    for (int y = 0; y < Main.screenHeight; y += 6)
                    {
                        float variation = (float)Math.Sin(y * 0.03f + (float)Main.timeForVisualEffects * 0.0012f) * 0.4f + 0.6f;
                        int width = (int)(edgeThickness * 0.5f * variation);
                        Color c = frostEdge * variation * 0.5f;
                        spriteBatch.Draw(pixel, new Rectangle(0, y, width, 6), c);
                        spriteBatch.Draw(pixel, new Rectangle(Main.screenWidth - width, y, width, 6), c);
                    }
                }

                // Draw falling snowflakes
                if (_snowflakes != null && _opacity > 0.1f)
                {
                    for (int i = 0; i < MaxSnowflakes; i++)
                    {
                        Color c = Color.White * _opacity * _snowflakes[i].Alpha * (0.3f + _intensity * 0.5f);
                        c.A = 0;
                        float s = _snowflakes[i].Scale * (1f + _intensity * 0.3f);
                        spriteBatch.Draw(pixel,
                            _snowflakes[i].Position,
                            new Rectangle(0, 0, 1, 1),
                            c, 0f,
                            new Vector2(0.5f),
                            s * 2.5f,
                            SpriteEffects.None, 0f);
                    }
                }
            }
        }

        public override Color OnTileColor(Color inColor)
        {
            Color tint = Color.Lerp(Color.White, new Color(200, 220, 255),
                _opacity * 0.35f * (1f + _intensity * 0.25f));
            return inColor.MultiplyRGBA(tint);
        }

        public override float GetCloudAlpha() => 1f - _opacity * 0.6f;
    }
}
