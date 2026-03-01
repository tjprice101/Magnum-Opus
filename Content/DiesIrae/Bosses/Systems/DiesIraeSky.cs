using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.Bosses;

namespace MagnumOpus.Content.DiesIrae.Bosses.Systems
{
    /// <summary>
    /// Custom sky effect for the Dies Irae boss fight.
    /// Creates an apocalyptic hellfire atmosphere with blood-red sky,
    /// falling ash particles, and crimson lightning flashes.
    /// Escalates in intensity with HP tiers — darker, more violent,
    /// more ash, more lightning as the Herald's wrath builds.
    /// </summary>
    public class DiesIraeSky : CustomSky
    {
        private bool _isActive;
        private float _opacity;
        private float _wrathIntensity; // 0 = start, 1 = maximum wrath

        private struct AshParticle
        {
            public Vector2 Position;
            public float Speed;
            public float Scale;
            public float Rotation;
            public float RotSpeed;
            public float Drift;
        }

        private AshParticle[] _ash;
        private const int MaxAsh = 80;

        // Lightning flash
        private float _lightningTimer;
        private float _lightningBrightness;
        private float _lightningDecay;

        public override void OnLoad() { }

        public override void Activate(Vector2 position, params object[] args)
        {
            _isActive = true;
            Random rand = new Random();
            _ash = new AshParticle[MaxAsh];

            for (int i = 0; i < MaxAsh; i++)
            {
                _ash[i] = new AshParticle
                {
                    Position = new Vector2(
                        (float)rand.NextDouble() * Main.screenWidth,
                        (float)rand.NextDouble() * Main.screenHeight),
                    Speed = 0.4f + (float)rand.NextDouble() * 1.2f,
                    Scale = 0.2f + (float)rand.NextDouble() * 0.5f,
                    Rotation = (float)rand.NextDouble() * MathHelper.TwoPi,
                    RotSpeed = ((float)rand.NextDouble() - 0.5f) * 0.06f,
                    Drift = ((float)rand.NextDouble() - 0.5f) * 0.8f
                };
            }

            _lightningTimer = 0f;
            _lightningBrightness = 0f;
            _lightningDecay = 0.05f;
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

            // Wrath intensity from boss HP tier
            NPC boss = BossIndexTracker.GetActiveBoss(BossIndexTracker.DiesIraeHerald);
            if (boss != null)
            {
                float hpRatio = boss.life / (float)boss.lifeMax;
                float targetWrath = 1f - hpRatio;
                _wrathIntensity += (targetWrath - _wrathIntensity) * 0.015f;
            }

            // Update ash particles — fall with drift
            if (_ash != null)
            {
                Random rand = new Random((int)(Main.timeForVisualEffects * 0.05f));
                for (int i = 0; i < MaxAsh; i++)
                {
                    _ash[i].Position.Y += _ash[i].Speed * (1f + _wrathIntensity * 0.5f);
                    _ash[i].Position.X += _ash[i].Drift + (float)Math.Sin(Main.timeForVisualEffects * 0.01f + i * 0.5f) * 0.3f;
                    _ash[i].Rotation += _ash[i].RotSpeed;

                    if (_ash[i].Position.Y > Main.screenHeight + 20)
                    {
                        _ash[i].Position.Y = -20;
                        _ash[i].Position.X = (float)rand.NextDouble() * Main.screenWidth;
                    }
                    if (_ash[i].Position.X < -20)
                        _ash[i].Position.X = Main.screenWidth + 20;
                    if (_ash[i].Position.X > Main.screenWidth + 20)
                        _ash[i].Position.X = -20;
                }
            }

            // Lightning flashes — more frequent at higher wrath
            _lightningBrightness = Math.Max(0f, _lightningBrightness - _lightningDecay);
            _lightningTimer += 1f;
            float flashChance = 200f - _wrathIntensity * 150f; // 200 frames → 50 frames
            if (_lightningTimer > flashChance && Main.rand.NextBool((int)Math.Max(1, flashChance * 0.5f)))
            {
                _lightningBrightness = 0.3f + _wrathIntensity * 0.5f;
                _lightningDecay = 0.02f + _wrathIntensity * 0.03f;
                _lightningTimer = 0f;
            }
        }

        public override void Draw(SpriteBatch spriteBatch, float minDepth, float maxDepth)
        {
            if (maxDepth >= 0f && minDepth < 0f)
            {
                Texture2D pixel = Terraria.GameContent.TextureAssets.MagicPixel.Value;

                // Sky gradient — blood red overhead darkening to ashen black
                Color topColor = Color.Lerp(new Color(60, 10, 8), new Color(30, 5, 5), _wrathIntensity);
                Color bottomColor = Color.Lerp(new Color(25, 10, 5), new Color(15, 5, 3), _wrathIntensity);
                topColor *= _opacity * 0.8f;
                bottomColor *= _opacity * 0.8f;

                for (int y = 0; y < Main.screenHeight; y += 4)
                {
                    float t = (float)y / Main.screenHeight;
                    Color lineColor = Color.Lerp(topColor, bottomColor, t);
                    spriteBatch.Draw(pixel, new Rectangle(0, y, Main.screenWidth, 4), lineColor);
                }

                // Lightning flash overlay
                if (_lightningBrightness > 0.01f && _opacity > 0.1f)
                {
                    Color flashColor = new Color(200, 50, 30) * (_lightningBrightness * _opacity);
                    flashColor.A = 0;
                    spriteBatch.Draw(pixel,
                        new Rectangle(0, 0, Main.screenWidth, Main.screenHeight),
                        flashColor);
                }

                // Falling ash
                if (_ash != null && _opacity > 0.1f)
                {
                    for (int i = 0; i < MaxAsh; i++)
                    {
                        float ashAlpha = _opacity * (0.2f + _wrathIntensity * 0.4f);
                        Color ashColor = Color.Lerp(
                            new Color(80, 60, 50),
                            new Color(120, 40, 20), _wrathIntensity) * ashAlpha;

                        // Some ash glows ember orange at high wrath
                        if (_wrathIntensity > 0.5f && i % 5 == 0)
                        {
                            float glowPulse = (float)Math.Sin(Main.timeForVisualEffects * 0.03f + i) * 0.5f + 0.5f;
                            ashColor = Color.Lerp(ashColor, new Color(220, 100, 30) * ashAlpha, glowPulse * _wrathIntensity);
                            ashColor.A = 0; // Additive for glowing embers
                        }

                        float s = _ash[i].Scale * (1f + _wrathIntensity * 0.3f);
                        spriteBatch.Draw(pixel,
                            _ash[i].Position,
                            new Rectangle(0, 0, 1, 1),
                            ashColor,
                            _ash[i].Rotation,
                            new Vector2(0.5f),
                            s * 2.5f,
                            SpriteEffects.None, 0f);
                    }
                }

                // Hellfire glow at the horizon — intensifies with wrath
                if (_wrathIntensity > 0.2f && _opacity > 0.1f)
                {
                    float glowAlpha = _wrathIntensity * _opacity * 0.2f;
                    float pulse = (float)Math.Sin(Main.timeForVisualEffects * 0.008f) * 0.3f + 0.7f;
                    Color glowColor = new Color(200, 30, 20) * (glowAlpha * pulse);
                    glowColor.A = 0;

                    int horizonY = (int)(Main.screenHeight * 0.7f);
                    for (int y = horizonY; y < Main.screenHeight; y += 4)
                    {
                        float t = (float)(y - horizonY) / (Main.screenHeight - horizonY);
                        Color lineGlow = glowColor * (1f - t * 0.5f);
                        spriteBatch.Draw(pixel, new Rectangle(0, y, Main.screenWidth, 4), lineGlow);
                    }
                }
            }
        }

        public override Color OnTileColor(Color inColor)
        {
            Color tint = Color.Lerp(Color.White,
                Color.Lerp(new Color(200, 160, 140), new Color(180, 100, 80), _wrathIntensity),
                _opacity * 0.3f);
            return inColor.MultiplyRGBA(tint);
        }

        public override float GetCloudAlpha() => 1f - _opacity * 0.9f;
    }
}
