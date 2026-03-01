using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.Bosses;

namespace MagnumOpus.Content.Spring.Bosses.Systems
{
    /// <summary>
    /// Custom sky effect for the Primavera boss fight.
    /// Creates a bright spring morning sky with blossom petals drifting,
    /// gentle green/pink tint that intensifies during Phase 2.
    /// </summary>
    public class PrimaveraSky : CustomSky
    {
        private bool _isActive;
        private float _opacity;
        private float _intensity;

        private struct BlossomPetal
        {
            public Vector2 Position;
            public float DriftSpeed;
            public float SwayAmplitude;
            public float SwayPhase;
            public float Scale;
            public Color Color;
            public float Rotation;
            public float RotSpeed;
        }

        private BlossomPetal[] _petals;
        private const int MaxPetals = 70;

        public override void OnLoad() { }

        public override void Activate(Vector2 position, params object[] args)
        {
            _isActive = true;
            _petals = new BlossomPetal[MaxPetals];
            Random rand = new Random();

            for (int i = 0; i < MaxPetals; i++)
            {
                _petals[i] = new BlossomPetal
                {
                    Position = new Vector2(
                        (float)rand.NextDouble() * Main.screenWidth,
                        (float)rand.NextDouble() * Main.screenHeight),
                    DriftSpeed = 0.3f + (float)rand.NextDouble() * 1.0f,
                    SwayAmplitude = 20f + (float)rand.NextDouble() * 40f,
                    SwayPhase = (float)rand.NextDouble() * MathHelper.TwoPi,
                    Scale = 0.2f + (float)rand.NextDouble() * 0.6f,
                    Color = Color.Lerp(new Color(240, 160, 180), new Color(255, 230, 240),
                        (float)rand.NextDouble()),
                    Rotation = (float)rand.NextDouble() * MathHelper.TwoPi,
                    RotSpeed = ((float)rand.NextDouble() - 0.5f) * 0.04f
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

            NPC boss = BossIndexTracker.GetActiveBoss(BossIndexTracker.Primavera);
            if (boss != null)
            {
                float hpRatio = boss.life / (float)boss.lifeMax;
                _intensity = 1f - hpRatio;

                if (_petals != null)
                {
                    float time = (float)Main.timeForVisualEffects;
                    for (int i = 0; i < MaxPetals; i++)
                    {
                        // Petals drift gently downward with wide sway
                        _petals[i].Position.Y += _petals[i].DriftSpeed * (0.8f + _intensity * 0.6f);
                        _petals[i].Position.X += (float)Math.Sin(time * 0.006f + _petals[i].SwayPhase)
                            * _petals[i].SwayAmplitude * 0.015f;

                        // Light breeze shifts petals sideways
                        _petals[i].Position.X += 0.3f + _intensity * 0.2f;

                        _petals[i].Rotation += _petals[i].RotSpeed;

                        if (_petals[i].Position.Y > Main.screenHeight + 20)
                        {
                            _petals[i].Position.Y = -20;
                            _petals[i].Position.X = Main.rand.NextFloat() * Main.screenWidth;
                        }
                        if (_petals[i].Position.X > Main.screenWidth + 30)
                            _petals[i].Position.X = -20;
                    }
                }
            }
        }

        public override void Draw(SpriteBatch spriteBatch, float minDepth, float maxDepth)
        {
            if (maxDepth >= 0f && minDepth < 0f)
            {
                // Sky gradient: soft lavender-pink at top, warm spring green at horizon
                Color topColor = Color.Lerp(new Color(60, 40, 55), new Color(100, 70, 90), _intensity);
                Color bottomColor = Color.Lerp(new Color(80, 100, 50), new Color(140, 180, 80), _intensity);

                topColor *= _opacity * 0.6f;
                bottomColor *= _opacity * 0.6f;

                Texture2D pixel = Terraria.GameContent.TextureAssets.MagicPixel.Value;

                for (int y = 0; y < Main.screenHeight; y += 4)
                {
                    float t = (float)y / Main.screenHeight;
                    Color lineColor = Color.Lerp(topColor, bottomColor, t);
                    spriteBatch.Draw(pixel, new Rectangle(0, y, Main.screenWidth, 4), lineColor);
                }

                // Draw drifting blossom petals
                if (_petals != null && _opacity > 0.1f)
                {
                    for (int i = 0; i < MaxPetals; i++)
                    {
                        Color c = _petals[i].Color * _opacity * (0.35f + _intensity * 0.45f);
                        c.A = 0;
                        float s = _petals[i].Scale * (1f + _intensity * 0.3f);
                        spriteBatch.Draw(pixel,
                            _petals[i].Position,
                            new Rectangle(0, 0, 1, 1),
                            c,
                            _petals[i].Rotation,
                            new Vector2(0.5f),
                            new Vector2(s * 3f, s * 1.5f),
                            SpriteEffects.None, 0f);
                    }
                }
            }
        }

        public override Color OnTileColor(Color inColor)
        {
            Color tint = Color.Lerp(Color.White, new Color(230, 255, 220),
                _opacity * 0.3f * (1f + _intensity * 0.25f));
            return inColor.MultiplyRGBA(tint);
        }

        public override float GetCloudAlpha() => 1f - _opacity * 0.4f;
    }
}
