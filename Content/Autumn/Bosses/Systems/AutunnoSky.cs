using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.Bosses;

namespace MagnumOpus.Content.Autumn.Bosses.Systems
{
    /// <summary>
    /// Custom sky effect for the Autunno boss fight.
    /// Creates an amber-orange evening sky with falling leaf particles
    /// and wind-blown debris that scatters during attacks.
    /// </summary>
    public class AutunnoSky : CustomSky
    {
        private bool _isActive;
        private float _opacity;
        private float _intensity;

        private struct FallingLeaf
        {
            public Vector2 Position;
            public float FallSpeed;
            public float SwayAmplitude;
            public float SwayPhase;
            public float Scale;
            public Color Color;
            public float Rotation;
            public float RotSpeed;
        }

        private FallingLeaf[] _leaves;
        private const int MaxLeaves = 80;

        public override void OnLoad() { }

        public override void Activate(Vector2 position, params object[] args)
        {
            _isActive = true;
            _leaves = new FallingLeaf[MaxLeaves];
            Random rand = new Random();

            for (int i = 0; i < MaxLeaves; i++)
            {
                _leaves[i] = new FallingLeaf
                {
                    Position = new Vector2(
                        (float)rand.NextDouble() * Main.screenWidth,
                        (float)rand.NextDouble() * Main.screenHeight),
                    FallSpeed = 0.4f + (float)rand.NextDouble() * 1.2f,
                    SwayAmplitude = 15f + (float)rand.NextDouble() * 30f,
                    SwayPhase = (float)rand.NextDouble() * MathHelper.TwoPi,
                    Scale = 0.3f + (float)rand.NextDouble() * 0.8f,
                    Color = Color.Lerp(new Color(200, 120, 40), new Color(180, 160, 60),
                        (float)rand.NextDouble()),
                    Rotation = (float)rand.NextDouble() * MathHelper.TwoPi,
                    RotSpeed = ((float)rand.NextDouble() - 0.5f) * 0.08f
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

            NPC boss = BossIndexTracker.GetActiveBoss(BossIndexTracker.Autunno);
            if (boss != null)
            {
                float hpRatio = boss.life / (float)boss.lifeMax;
                _intensity = 1f - hpRatio;

                if (_leaves != null)
                {
                    float time = (float)Main.timeForVisualEffects;
                    for (int i = 0; i < MaxLeaves; i++)
                    {
                        // Leaves fall downward with sinusoidal sway
                        _leaves[i].Position.Y += _leaves[i].FallSpeed * (1f + _intensity * 0.8f);
                        _leaves[i].Position.X += (float)Math.Sin(time * 0.008f + _leaves[i].SwayPhase)
                            * _leaves[i].SwayAmplitude * 0.02f;

                        // Wind gusts push leaves sideways during high intensity
                        _leaves[i].Position.X += _intensity * 0.5f *
                            (float)Math.Sin(time * 0.003f + i * 0.7f);

                        _leaves[i].Rotation += _leaves[i].RotSpeed * (1f + _intensity);

                        if (_leaves[i].Position.Y > Main.screenHeight + 20)
                        {
                            _leaves[i].Position.Y = -20;
                            _leaves[i].Position.X = Main.rand.NextFloat() * Main.screenWidth;
                        }
                        if (_leaves[i].Position.X < -30)
                            _leaves[i].Position.X = Main.screenWidth + 20;
                        if (_leaves[i].Position.X > Main.screenWidth + 30)
                            _leaves[i].Position.X = -20;
                    }
                }
            }
        }

        public override void Draw(SpriteBatch spriteBatch, float minDepth, float maxDepth)
        {
            if (maxDepth >= 0f && minDepth < 0f)
            {
                // Sky gradient: deep amber at top, warm gold-brown at horizon
                Color topColor = Color.Lerp(new Color(50, 25, 10), new Color(90, 40, 15), _intensity);
                Color bottomColor = Color.Lerp(new Color(120, 70, 25), new Color(200, 120, 40), _intensity);

                topColor *= _opacity * 0.7f;
                bottomColor *= _opacity * 0.7f;

                Texture2D pixel = Terraria.GameContent.TextureAssets.MagicPixel.Value;

                for (int y = 0; y < Main.screenHeight; y += 4)
                {
                    float t = (float)y / Main.screenHeight;
                    Color lineColor = Color.Lerp(topColor, bottomColor, t);
                    spriteBatch.Draw(pixel, new Rectangle(0, y, Main.screenWidth, 4), lineColor);
                }

                // Draw falling leaves
                if (_leaves != null && _opacity > 0.1f)
                {
                    for (int i = 0; i < MaxLeaves; i++)
                    {
                        Color c = _leaves[i].Color * _opacity * (0.3f + _intensity * 0.5f);
                        c.A = 0;
                        float s = _leaves[i].Scale * (1f + _intensity * 0.4f);
                        spriteBatch.Draw(pixel,
                            _leaves[i].Position,
                            new Rectangle(0, 0, 1, 1),
                            c,
                            _leaves[i].Rotation,
                            new Vector2(0.5f),
                            new Vector2(s * 4f, s * 2f), // Elongated leaf shape
                            SpriteEffects.None, 0f);
                    }
                }
            }
        }

        public override Color OnTileColor(Color inColor)
        {
            Color tint = Color.Lerp(Color.White, new Color(240, 200, 140),
                _opacity * 0.35f * (1f + _intensity * 0.2f));
            return inColor.MultiplyRGBA(tint);
        }

        public override float GetCloudAlpha() => 1f - _opacity * 0.5f;
    }
}
