using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.Bosses;

namespace MagnumOpus.Content.Eroica.Bosses.Systems
{
    /// <summary>
    /// Custom sky effect for the Eroica boss fight.
    /// Creates a scarlet/gold heaven-lit sky with
    /// ascending ember particles that reacts to boss phase and HP.
    /// Similar to Calamity's YSky for Yharon.
    /// </summary>
    public class EroicaSky : CustomSky
    {
        private bool _isActive;
        private float _opacity;
        private float _intensity; // 0 = phase 1, 1 = enraged

        private struct SkyEmber
        {
            public Vector2 Position;
            public float Speed;
            public float Scale;
            public Color Color;
            public float Rotation;
            public float RotSpeed;
        }

        private SkyEmber[] _embers;
        private const int MaxEmbers = 60;

        public override void OnLoad() { }

        public override void Activate(Vector2 position, params object[] args)
        {
            _isActive = true;
            _embers = new SkyEmber[MaxEmbers];
            Random rand = new Random();

            for (int i = 0; i < MaxEmbers; i++)
            {
                _embers[i] = new SkyEmber
                {
                    Position = new Vector2(
                        (float)rand.NextDouble() * Main.screenWidth,
                        (float)rand.NextDouble() * Main.screenHeight),
                    Speed = 0.5f + (float)rand.NextDouble() * 1.5f,
                    Scale = 0.3f + (float)rand.NextDouble() * 0.7f,
                    Color = Color.Lerp(new Color(255, 200, 80), new Color(200, 50, 50),
                        (float)rand.NextDouble()),
                    Rotation = (float)rand.NextDouble() * MathHelper.TwoPi,
                    RotSpeed = ((float)rand.NextDouble() - 0.5f) * 0.05f
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

            // Update intensity based on boss phase
            NPC boss = BossIndexTracker.GetActiveBoss(BossIndexTracker.EroicaRetribution);
            if (boss != null)
            {
                float hpRatio = boss.life / (float)boss.lifeMax;
                _intensity = 1f - hpRatio; // More intense as HP drops

                // Update embers
                if (_embers != null)
                {
                    Random rand = new Random((int)(Main.timeForVisualEffects * 0.1f));
                    for (int i = 0; i < MaxEmbers; i++)
                    {
                        _embers[i].Position.Y -= _embers[i].Speed * (1f + _intensity);
                        _embers[i].Position.X += (float)Math.Sin(Main.timeForVisualEffects * 0.01f + i) * 0.3f;
                        _embers[i].Rotation += _embers[i].RotSpeed;

                        if (_embers[i].Position.Y < -20)
                        {
                            _embers[i].Position.Y = Main.screenHeight + 20;
                            _embers[i].Position.X = (float)rand.NextDouble() * Main.screenWidth;
                        }
                    }
                }
            }
        }

        public override void Draw(SpriteBatch spriteBatch, float minDepth, float maxDepth)
        {
            if (maxDepth >= 0f && minDepth < 0f)
            {
                // Sky gradient: dark scarlet at top, warm gold at horizon
                float time = (float)Main.timeForVisualEffects * 0.002f;
                Color topColor = Color.Lerp(new Color(40, 10, 10), new Color(100, 20, 15), _intensity);
                Color bottomColor = Color.Lerp(new Color(80, 50, 20), new Color(180, 100, 30), _intensity);

                topColor *= _opacity * 0.7f;
                bottomColor *= _opacity * 0.7f;

                Texture2D pixel = Terraria.GameContent.TextureAssets.MagicPixel.Value;

                // Draw gradient sky
                for (int y = 0; y < Main.screenHeight; y += 4)
                {
                    float t = (float)y / Main.screenHeight;
                    Color lineColor = Color.Lerp(topColor, bottomColor, t);
                    spriteBatch.Draw(pixel, new Rectangle(0, y, Main.screenWidth, 4), lineColor);
                }

                // Draw ascending embers
                if (_embers != null && _opacity > 0.1f)
                {
                    for (int i = 0; i < MaxEmbers; i++)
                    {
                        Color c = _embers[i].Color * _opacity * (0.3f + _intensity * 0.5f);
                        c.A = 0; // Additive feel
                        float s = _embers[i].Scale * (1f + _intensity * 0.5f);
                        spriteBatch.Draw(pixel,
                            _embers[i].Position,
                            new Rectangle(0, 0, 1, 1),
                            c,
                            _embers[i].Rotation,
                            new Vector2(0.5f),
                            s * 3f,
                            SpriteEffects.None, 0f);
                    }
                }
            }
        }

        public override Color OnTileColor(Color inColor)
        {
            Color tint = Color.Lerp(Color.White, new Color(255, 200, 160), _opacity * 0.3f * (1f + _intensity * 0.2f));
            return inColor.MultiplyRGBA(tint);
        }

        public override float GetCloudAlpha() => 1f - _opacity * 0.6f;
    }
}
