using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.Bosses;

namespace MagnumOpus.Content.LaCampanella.Bosses.Systems
{
    /// <summary>
    /// Custom sky effect for the La Campanella boss fight.
    /// Creates a dark smoky sky with infernal orange fire glow at the horizon,
    /// falling ember particles that react to boss phases,
    /// and bell silhouette shadows during toll attacks.
    /// </summary>
    public class LaCampanellaSky : CustomSky
    {
        private bool _isActive;
        private float _opacity;
        private float _intensity;

        private struct SkyEmber
        {
            public Vector2 Position;
            public float Speed;
            public float Scale;
            public Color Color;
            public float Rotation;
            public float RotSpeed;
            public float Flicker;
        }

        private SkyEmber[] _embers;
        private const int MaxEmbers = 80;

        // Bell silhouette tracking
        private float _bellSilhouetteAlpha;
        private float _bellSilhouetteY;

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
                    Speed = 0.3f + (float)rand.NextDouble() * 1.2f,
                    Scale = 0.2f + (float)rand.NextDouble() * 0.6f,
                    Color = Color.Lerp(new Color(255, 140, 40), new Color(255, 80, 20),
                        (float)rand.NextDouble()),
                    Rotation = (float)rand.NextDouble() * MathHelper.TwoPi,
                    RotSpeed = ((float)rand.NextDouble() - 0.5f) * 0.04f,
                    Flicker = (float)rand.NextDouble() * MathHelper.TwoPi
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

            NPC boss = BossIndexTracker.GetActiveBoss(BossIndexTracker.LaCampanellaChime);
            if (boss != null)
            {
                float hpRatio = boss.life / (float)boss.lifeMax;
                _intensity = 1f - hpRatio;

                // Bell silhouette fades during toll attacks (phase >= 1)
                if (BossIndexTracker.LaCampanellaPhase >= 1)
                {
                    _bellSilhouetteAlpha = Math.Min(1f, _bellSilhouetteAlpha + 0.015f);
                    _bellSilhouetteY = (float)Math.Sin(Main.timeForVisualEffects * 0.008f) * 15f;
                }
                else
                {
                    _bellSilhouetteAlpha = Math.Max(0f, _bellSilhouetteAlpha - 0.03f);
                }

                // Update falling embers
                if (_embers != null)
                {
                    for (int i = 0; i < MaxEmbers; i++)
                    {
                        _embers[i].Position.Y += _embers[i].Speed * (0.8f + _intensity * 1.2f);
                        _embers[i].Position.X += (float)Math.Sin(Main.timeForVisualEffects * 0.012f + i * 0.7f) * 0.4f;
                        _embers[i].Rotation += _embers[i].RotSpeed;
                        _embers[i].Flicker += 0.08f;

                        if (_embers[i].Position.Y > Main.screenHeight + 20)
                        {
                            _embers[i].Position.Y = -20;
                            _embers[i].Position.X = Main.rand.NextFloat() * Main.screenWidth;
                        }
                    }
                }
            }
        }

        public override void Draw(SpriteBatch spriteBatch, float minDepth, float maxDepth)
        {
            if (maxDepth >= 0f && minDepth < 0f)
            {
                // Sky gradient: dark smoky top to infernal orange horizon
                Color topColor = Color.Lerp(new Color(15, 10, 8), new Color(50, 20, 10), _intensity);
                Color midColor = Color.Lerp(new Color(40, 25, 15), new Color(120, 50, 15), _intensity);
                Color bottomColor = Color.Lerp(new Color(80, 45, 15), new Color(200, 100, 25), _intensity);

                topColor *= _opacity * 0.75f;
                midColor *= _opacity * 0.75f;
                bottomColor *= _opacity * 0.75f;

                Texture2D pixel = Terraria.GameContent.TextureAssets.MagicPixel.Value;

                // Draw gradient sky in two halves
                int midpoint = Main.screenHeight / 2;
                for (int y = 0; y < Main.screenHeight; y += 4)
                {
                    float t;
                    Color lineColor;
                    if (y < midpoint)
                    {
                        t = (float)y / midpoint;
                        lineColor = Color.Lerp(topColor, midColor, t);
                    }
                    else
                    {
                        t = (float)(y - midpoint) / (Main.screenHeight - midpoint);
                        lineColor = Color.Lerp(midColor, bottomColor, t);
                    }
                    spriteBatch.Draw(pixel, new Rectangle(0, y, Main.screenWidth, 4), lineColor);
                }

                // Draw bell silhouette shadows in the sky
                if (_bellSilhouetteAlpha > 0.01f)
                {
                    Color bellColor = new Color(10, 5, 5) * _opacity * _bellSilhouetteAlpha * 0.4f;
                    float bellX = Main.screenWidth * 0.5f;
                    float bellY = Main.screenHeight * 0.15f + _bellSilhouetteY;

                    // Simple bell shape: rectangle body + triangle top
                    spriteBatch.Draw(pixel, new Rectangle((int)(bellX - 30), (int)bellY, 60, 50), bellColor);
                    spriteBatch.Draw(pixel, new Rectangle((int)(bellX - 20), (int)(bellY - 20), 40, 25), bellColor);
                    spriteBatch.Draw(pixel, new Rectangle((int)(bellX - 8), (int)(bellY - 35), 16, 20), bellColor);
                }

                // Draw falling ember particles
                if (_embers != null && _opacity > 0.1f)
                {
                    for (int i = 0; i < MaxEmbers; i++)
                    {
                        float flicker = 0.6f + (float)Math.Sin(_embers[i].Flicker) * 0.4f;
                        Color c = _embers[i].Color * _opacity * (0.3f + _intensity * 0.5f) * flicker;
                        c.A = 0; // Additive feel
                        float s = _embers[i].Scale * (1f + _intensity * 0.4f);
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
            Color tint = Color.Lerp(Color.White, new Color(255, 180, 130), _opacity * 0.35f * (1f + _intensity * 0.25f));
            return inColor.MultiplyRGBA(tint);
        }

        public override float GetCloudAlpha() => 1f - _opacity * 0.7f;
    }
}
