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
    /// Movement-aware custom sky for Eroica's Retribution — Beethoven's Third Symphony.
    /// M1 (Call to Arms): Dawn-red glory, ascending embers, bright gold horizon.
    /// M2 (Funeral March): Overcast darkness, falling embers, golden light breaks through rarely.
    /// M3 (Scherzo): Electric flickering, rapid color shifts, erratic embers.
    /// M4 (Apotheosis): Blazing gold-white sky, furious ascending embers, maximum intensity.
    /// </summary>
    public class EroicaSky : CustomSky
    {
        private bool _isActive;
        private float _opacity;
        private float _movement = 1f; // 1-4
        private float _smoothMovement = 1f; // Smoothly interpolated
        private float _heroIntensity;

        // Palette
        private static readonly Color ValorGold = new Color(255, 200, 80);
        private static readonly Color ValorScarlet = new Color(200, 50, 50);
        private static readonly Color FuneralCrimson = new Color(180, 30, 60);
        private static readonly Color FuneralAsh = new Color(80, 60, 50);
        private static readonly Color PhoenixWhite = new Color(255, 240, 220);
        private static readonly Color EmberOrange = new Color(255, 140, 30);

        private struct SkyEmber
        {
            public Vector2 Position;
            public float Speed;
            public float Scale;
            public Color BaseColor;
            public float Rotation;
            public float RotSpeed;
            public float Phase; // For per-ember variation
        }

        private SkyEmber[] _embers;
        private const int MaxEmbers = 80;

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
                    BaseColor = Color.Lerp(ValorGold, ValorScarlet, (float)rand.NextDouble()),
                    Rotation = (float)rand.NextDouble() * MathHelper.TwoPi,
                    RotSpeed = ((float)rand.NextDouble() - 0.5f) * 0.05f,
                    Phase = (float)rand.NextDouble() * MathHelper.TwoPi
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

            // Derive movement from BossIndexTracker
            int phase = BossIndexTracker.EroicaPhase;
            bool enraged = BossIndexTracker.EroicaEnraged;
            _movement = enraged ? 4f : Math.Max(1f, phase);
            _smoothMovement = MathHelper.Lerp(_smoothMovement, _movement, 0.03f);

            NPC boss = BossIndexTracker.GetActiveBoss(BossIndexTracker.EroicaRetribution);
            if (boss != null)
            {
                float hpRatio = boss.life / (float)boss.lifeMax;
                _heroIntensity = enraged ? 1f : MathHelper.Clamp((1f - hpRatio) * 0.8f + (phase - 1) * 0.15f, 0f, 1f);

                if (_embers != null)
                {
                    float time = (float)Main.timeForVisualEffects;

                    for (int i = 0; i < MaxEmbers; i++)
                    {
                        ref SkyEmber ember = ref _embers[i];
                        float mv = _smoothMovement;

                        if (mv < 1.5f)
                        {
                            // M1: Steady ascending embers — heroic and warm
                            ember.Position.Y -= ember.Speed * 1.2f;
                            ember.Position.X += (float)Math.Sin(time * 0.01f + ember.Phase) * 0.4f;
                        }
                        else if (mv < 2.5f)
                        {
                            // M2: Falling embers — funeral descent, slow lateral drift
                            ember.Position.Y += ember.Speed * 0.6f;
                            ember.Position.X += (float)Math.Sin(time * 0.005f + ember.Phase) * 0.8f;
                        }
                        else if (mv < 3.5f)
                        {
                            // M3: Erratic embers — chaotic flicker, rapid direction changes
                            float erratic = (float)Math.Sin(time * 0.05f + ember.Phase * 3f);
                            ember.Position.Y -= ember.Speed * (0.5f + erratic * 1.5f);
                            ember.Position.X += (float)Math.Sin(time * 0.03f + ember.Phase) * 2f;
                        }
                        else
                        {
                            // M4: Furious ascending columns — fast, bright, intense
                            ember.Position.Y -= ember.Speed * 2.5f;
                            ember.Position.X += (float)Math.Sin(time * 0.015f + ember.Phase) * 0.5f;
                        }

                        ember.Rotation += ember.RotSpeed * (mv < 3.5f ? 1f : 2f);

                        // Wrap embers based on movement direction
                        if (mv >= 1.5f && mv < 2.5f)
                        {
                            // M2: Falling — wrap at bottom
                            if (ember.Position.Y > Main.screenHeight + 20)
                            {
                                ember.Position.Y = -20;
                                ember.Position.X = Main.rand.NextFloat() * Main.screenWidth;
                            }
                        }
                        else
                        {
                            // M1/M3/M4: Rising — wrap at top
                            if (ember.Position.Y < -20)
                            {
                                ember.Position.Y = Main.screenHeight + 20;
                                ember.Position.X = Main.rand.NextFloat() * Main.screenWidth;
                            }
                        }

                        // Horizontal wrap
                        if (ember.Position.X < -20)
                            ember.Position.X = Main.screenWidth + 20;
                        else if (ember.Position.X > Main.screenWidth + 20)
                            ember.Position.X = -20;
                    }
                }
            }
        }

        public override void Draw(SpriteBatch spriteBatch, float minDepth, float maxDepth)
        {
            if (maxDepth >= 0f && minDepth < 0f)
            {
                float time = (float)Main.timeForVisualEffects * 0.002f;
                float mv = _smoothMovement;

                // Movement-driven sky gradient
                Color topColor, bottomColor;

                if (mv < 1.5f)
                {
                    // M1: Dawn-red glory — dark scarlet zenith, warm gold horizon
                    topColor = Color.Lerp(new Color(40, 10, 10), new Color(90, 25, 15), _heroIntensity);
                    bottomColor = Color.Lerp(new Color(80, 50, 20), new Color(180, 120, 40), _heroIntensity);
                }
                else if (mv < 2.5f)
                {
                    // M2: Overcast funeral — near-black zenith, ashen dark horizon
                    topColor = Color.Lerp(new Color(15, 8, 8), new Color(30, 15, 15), _heroIntensity);
                    bottomColor = Color.Lerp(new Color(40, 30, 25), new Color(60, 40, 30), _heroIntensity);

                    // Golden breakthrough: rare pulses of warmth from below
                    float breakthrough = (float)Math.Sin(time * 0.35f + 1.7f);
                    if (breakthrough > 0.75f)
                    {
                        float btAmount = (breakthrough - 0.75f) / 0.25f * 0.25f;
                        bottomColor = Color.Lerp(bottomColor, new Color(120, 80, 30), btAmount);
                    }
                }
                else if (mv < 3.5f)
                {
                    // M3: Electric flickering — rapid shifts between scarlet and dark
                    float flicker = (float)Math.Sin(time * 6f) * 0.5f + 0.5f;
                    topColor = Color.Lerp(new Color(30, 8, 8), new Color(80, 20, 15), flicker * _heroIntensity);
                    bottomColor = Color.Lerp(new Color(50, 30, 15), new Color(150, 80, 25), flicker);
                }
                else
                {
                    // M4: Blazing apotheosis — lower half funeral dark, upper erupting gold-white
                    topColor = Color.Lerp(new Color(100, 60, 20), new Color(200, 140, 60), _heroIntensity);
                    bottomColor = Color.Lerp(new Color(20, 10, 8), new Color(80, 30, 15), _heroIntensity);

                    // Pulsing white-hot core at horizon center
                    float pulse = (float)Math.Sin(time * 2.5f) * 0.2f + 0.8f;
                    topColor = Color.Lerp(topColor, PhoenixWhite, _heroIntensity * 0.3f * pulse);
                }

                topColor *= _opacity * 0.75f;
                bottomColor *= _opacity * 0.75f;

                Texture2D pixel = Terraria.GameContent.TextureAssets.MagicPixel.Value;

                // Draw gradient sky
                for (int y = 0; y < Main.screenHeight; y += 4)
                {
                    float t = (float)y / Main.screenHeight;
                    Color lineColor = Color.Lerp(topColor, bottomColor, t);
                    spriteBatch.Draw(pixel, new Rectangle(0, y, Main.screenWidth, 4), lineColor);
                }

                // Draw embers with movement-aware coloring
                if (_embers != null && _opacity > 0.1f)
                {
                    for (int i = 0; i < MaxEmbers; i++)
                    {
                        ref SkyEmber ember = ref _embers[i];

                        Color c;
                        float scaleMult;

                        if (mv < 1.5f)
                        {
                            // M1: Gold-scarlet embers, warm and steady
                            c = ember.BaseColor * _opacity * (0.35f + _heroIntensity * 0.4f);
                            scaleMult = 1f + _heroIntensity * 0.3f;
                        }
                        else if (mv < 2.5f)
                        {
                            // M2: Dim crimson-ash, barely visible — funeral embers
                            Color funeralColor = Color.Lerp(FuneralAsh, FuneralCrimson, (float)Math.Sin(ember.Phase) * 0.5f + 0.5f);
                            c = funeralColor * _opacity * (0.15f + _heroIntensity * 0.15f);
                            scaleMult = 0.7f;

                            // Occasional golden ember (a memory of glory)
                            if (i % 12 == 0)
                            {
                                float goldPulse = (float)Math.Sin(time * 0.5f + ember.Phase);
                                if (goldPulse > 0.6f)
                                    c = ValorGold * _opacity * 0.2f * ((goldPulse - 0.6f) / 0.4f);
                            }
                        }
                        else if (mv < 3.5f)
                        {
                            // M3: Flickering between gold and scarlet, erratic brightness
                            float flicker = (float)Math.Sin(time * 8f + ember.Phase * 5f);
                            Color flickerColor = flicker > 0 ? ValorGold : ValorScarlet;
                            c = flickerColor * _opacity * (0.2f + Math.Abs(flicker) * 0.5f);
                            scaleMult = 0.8f + Math.Abs(flicker) * 0.5f;
                        }
                        else
                        {
                            // M4: White-hot cores with gold-scarlet halos, large, bright
                            float pulse = (float)Math.Sin(time * 3f + ember.Phase) * 0.3f + 0.7f;
                            Color coreColor = Color.Lerp(ember.BaseColor, PhoenixWhite, _heroIntensity * 0.6f);
                            c = coreColor * _opacity * (0.5f + _heroIntensity * 0.5f) * pulse;
                            scaleMult = 1.5f + _heroIntensity * 0.8f;
                        }

                        c.A = 0; // Additive feel
                        float s = ember.Scale * scaleMult;

                        spriteBatch.Draw(pixel,
                            ember.Position,
                            new Rectangle(0, 0, 1, 1),
                            c,
                            ember.Rotation,
                            new Vector2(0.5f),
                            s * 3f,
                            SpriteEffects.None, 0f);
                    }
                }
            }
        }

        public override Color OnTileColor(Color inColor)
        {
            float mv = _smoothMovement;
            Color tint;

            if (mv < 1.5f)
            {
                // M1: Warm golden tint
                tint = Color.Lerp(Color.White, new Color(255, 210, 170), _opacity * 0.3f);
            }
            else if (mv < 2.5f)
            {
                // M2: Cold, dim — funeral pallor
                tint = Color.Lerp(Color.White, new Color(160, 140, 140), _opacity * 0.4f);
            }
            else if (mv < 3.5f)
            {
                // M3: Flickering warm/cool
                float flicker = (float)Math.Sin(Main.timeForVisualEffects * 0.008f) * 0.5f + 0.5f;
                Color warmTint = new Color(255, 200, 160);
                Color coolTint = new Color(200, 160, 160);
                tint = Color.Lerp(Color.White, Color.Lerp(coolTint, warmTint, flicker), _opacity * 0.3f);
            }
            else
            {
                // M4: Bright golden-white, heroic illumination
                tint = Color.Lerp(Color.White, new Color(255, 230, 200), _opacity * 0.35f * (1f + _heroIntensity * 0.3f));
            }

            return inColor.MultiplyRGBA(tint);
        }

        public override float GetCloudAlpha()
        {
            float mv = _smoothMovement;
            if (mv < 2.5f && mv >= 1.5f)
                return 1f - _opacity * 0.3f; // M2: Some clouds remain (overcast)
            return 1f - _opacity * 0.7f; // Other movements: mostly clear
        }
    }
}
