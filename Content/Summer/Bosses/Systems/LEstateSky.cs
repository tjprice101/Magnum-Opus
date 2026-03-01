using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.Bosses;

namespace MagnumOpus.Content.Summer.Bosses.Systems
{
    /// <summary>
    /// Custom sky effect for the L'Estate boss fight.
    /// Creates a blazing hot summer sky with solar glare,
    /// heat haze distortion at edges, and intensity phases
    /// (normal → scorching → supernova).
    /// </summary>
    public class LEstateSky : CustomSky
    {
        private bool _isActive;
        private float _opacity;
        private float _intensity;

        private struct HeatMote
        {
            public Vector2 Position;
            public float RiseSpeed;
            public float WobblePhase;
            public float Scale;
            public Color Color;
            public float Shimmer;
        }

        private HeatMote[] _motes;
        private const int MaxMotes = 50;

        public override void OnLoad() { }

        public override void Activate(Vector2 position, params object[] args)
        {
            _isActive = true;
            _motes = new HeatMote[MaxMotes];
            Random rand = new Random();

            for (int i = 0; i < MaxMotes; i++)
            {
                _motes[i] = new HeatMote
                {
                    Position = new Vector2(
                        (float)rand.NextDouble() * Main.screenWidth,
                        (float)rand.NextDouble() * Main.screenHeight),
                    RiseSpeed = 0.3f + (float)rand.NextDouble() * 1.5f,
                    WobblePhase = (float)rand.NextDouble() * MathHelper.TwoPi,
                    Scale = 0.4f + (float)rand.NextDouble() * 1.0f,
                    Color = Color.Lerp(new Color(255, 200, 50), new Color(240, 130, 30),
                        (float)rand.NextDouble()),
                    Shimmer = (float)rand.NextDouble()
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

            NPC boss = BossIndexTracker.GetActiveBoss(BossIndexTracker.LEstate);
            if (boss != null)
            {
                float hpRatio = boss.life / (float)boss.lifeMax;
                _intensity = 1f - hpRatio;

                if (_motes != null)
                {
                    float time = (float)Main.timeForVisualEffects;
                    for (int i = 0; i < MaxMotes; i++)
                    {
                        // Heat motes rise upward with shimmer wobble
                        _motes[i].Position.Y -= _motes[i].RiseSpeed * (1f + _intensity * 1.2f);
                        _motes[i].Position.X += (float)Math.Sin(time * 0.01f + _motes[i].WobblePhase) * 0.4f;

                        // Shimmer pulsing
                        _motes[i].Shimmer = (float)Math.Sin(time * 0.02f + i * 0.5f) * 0.5f + 0.5f;

                        if (_motes[i].Position.Y < -20)
                        {
                            _motes[i].Position.Y = Main.screenHeight + 20;
                            _motes[i].Position.X = Main.rand.NextFloat() * Main.screenWidth;
                        }
                    }
                }
            }
        }

        public override void Draw(SpriteBatch spriteBatch, float minDepth, float maxDepth)
        {
            if (maxDepth >= 0f && minDepth < 0f)
            {
                // Sky gradient: blazing white-gold at top, scorching orange at horizon
                // Intensifies: normal → scorching → supernova
                float phase = _intensity;
                Color topColor = Color.Lerp(
                    new Color(80, 50, 15),
                    Color.Lerp(new Color(140, 80, 20), new Color(180, 100, 30), phase),
                    phase);
                Color bottomColor = Color.Lerp(
                    new Color(100, 70, 20),
                    Color.Lerp(new Color(200, 120, 25), new Color(255, 160, 40), phase),
                    phase);

                topColor *= _opacity * 0.75f;
                bottomColor *= _opacity * 0.75f;

                Texture2D pixel = Terraria.GameContent.TextureAssets.MagicPixel.Value;

                for (int y = 0; y < Main.screenHeight; y += 4)
                {
                    float t = (float)y / Main.screenHeight;
                    Color lineColor = Color.Lerp(topColor, bottomColor, t);
                    spriteBatch.Draw(pixel, new Rectangle(0, y, Main.screenWidth, 4), lineColor);
                }

                // Solar glare overlay at center-top
                if (_opacity > 0.3f)
                {
                    float glareIntensity = _opacity * (0.15f + _intensity * 0.25f);
                    Color glareColor = new Color(255, 240, 180) * glareIntensity;
                    glareColor.A = 0;
                    Vector2 glarePos = new Vector2(Main.screenWidth * 0.5f, Main.screenHeight * 0.15f);
                    float glareScale = 80f + _intensity * 60f;
                    spriteBatch.Draw(pixel, glarePos, new Rectangle(0, 0, 1, 1),
                        glareColor, 0f, new Vector2(0.5f), glareScale, SpriteEffects.None, 0f);
                }

                // Draw rising heat motes
                if (_motes != null && _opacity > 0.1f)
                {
                    for (int i = 0; i < MaxMotes; i++)
                    {
                        float shimmerAlpha = 0.3f + _motes[i].Shimmer * 0.4f;
                        Color c = _motes[i].Color * _opacity * shimmerAlpha * (0.4f + _intensity * 0.5f);
                        c.A = 0;
                        float s = _motes[i].Scale * (1f + _intensity * 0.6f);
                        spriteBatch.Draw(pixel,
                            _motes[i].Position,
                            new Rectangle(0, 0, 1, 1),
                            c, 0f,
                            new Vector2(0.5f),
                            s * 3f,
                            SpriteEffects.None, 0f);
                    }
                }
            }
        }

        public override Color OnTileColor(Color inColor)
        {
            Color tint = Color.Lerp(Color.White, new Color(255, 230, 180),
                _opacity * 0.4f * (1f + _intensity * 0.3f));
            return inColor.MultiplyRGBA(tint);
        }

        public override float GetCloudAlpha() => 1f - _opacity * 0.7f;
    }
}
