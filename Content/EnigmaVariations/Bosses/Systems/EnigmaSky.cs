using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.Bosses;

namespace MagnumOpus.Content.EnigmaVariations.Bosses.Systems
{
    /// <summary>
    /// Custom sky for the Enigma Variations boss encounter.
    /// A void-drenched atmosphere of unknowable dread. Watching eyes
    /// materialize and vanish, reality fracture lines tear across the sky
    /// during Phase 3, and an eerie green-tinged void mist rolls endlessly.
    /// </summary>
    public class EnigmaSky : CustomSky
    {
        private bool _isActive;
        private float _opacity;
        private float _windSpeed;

        private static readonly Color VoidBlack = new Color(15, 5, 25);
        private static readonly Color DeepPurple = new Color(100, 30, 150);
        private static readonly Color EerieGreen = new Color(80, 200, 100);
        private static readonly Color MysteryWhite = new Color(200, 180, 220);

        private struct VoidEye
        {
            public Vector2 Position;
            public float Scale;
            public float Opacity;
            public float Timer;
            public float Lifetime;
            public float Phase;
            public bool Active;
        }

        private const int MaxEyes = 40;
        private VoidEye[] _eyes = new VoidEye[MaxEyes];
        private float _globalTimer;

        private struct RealityFracture
        {
            public Vector2 Start;
            public Vector2 End;
            public float Opacity;
            public float Timer;
            public float Lifetime;
            public bool Active;
        }

        private const int MaxFractures = 12;
        private RealityFracture[] _fractures = new RealityFracture[MaxFractures];

        public override void OnLoad() { }

        public override void Update(GameTime gameTime)
        {
            if (!_isActive)
            {
                _opacity = Math.Max(0f, _opacity - 0.01f);
                return;
            }

            _opacity = Math.Min(1f, _opacity + 0.02f);
            _globalTimer += 0.016f;
            _windSpeed = (float)Math.Sin(_globalTimer * 0.2f) * 0.3f;

            int bossPhase = BossIndexTracker.EnigmaPhase;

            // Update void eyes
            for (int i = 0; i < MaxEyes; i++)
            {
                if (_eyes[i].Active)
                {
                    _eyes[i].Timer += 0.016f;
                    float life = _eyes[i].Timer / _eyes[i].Lifetime;

                    // Eyes pulse open and shut, then vanish
                    if (life < 0.15f)
                        _eyes[i].Opacity = life / 0.15f;
                    else if (life < 0.6f)
                        _eyes[i].Opacity = 1f - 0.3f * (float)Math.Sin((life - 0.15f) * MathHelper.TwoPi * 2f);
                    else if (life < 0.85f)
                        _eyes[i].Opacity = 1f - (life - 0.6f) / 0.25f;
                    else
                        _eyes[i].Active = false;

                    if (_eyes[i].Timer >= _eyes[i].Lifetime)
                        _eyes[i].Active = false;
                }
                else if (Main.rand.NextBool(bossPhase >= 2 ? 30 : 60))
                {
                    SpawnEye(i);
                }
            }

            // Phase 3: Reality fracture lines
            if (bossPhase >= 2)
            {
                for (int i = 0; i < MaxFractures; i++)
                {
                    if (_fractures[i].Active)
                    {
                        _fractures[i].Timer += 0.016f;
                        float life = _fractures[i].Timer / _fractures[i].Lifetime;
                        if (life < 0.1f)
                            _fractures[i].Opacity = life / 0.1f;
                        else if (life > 0.7f)
                            _fractures[i].Opacity = 1f - (life - 0.7f) / 0.3f;
                        else
                            _fractures[i].Opacity = 1f;

                        if (_fractures[i].Timer >= _fractures[i].Lifetime)
                            _fractures[i].Active = false;
                    }
                    else if (Main.rand.NextBool(80))
                    {
                        SpawnFracture(i);
                    }
                }
            }
        }

        private void SpawnEye(int index)
        {
            _eyes[index] = new VoidEye
            {
                Position = new Vector2(
                    Main.rand.NextFloat(0f, Main.screenWidth),
                    Main.rand.NextFloat(0f, Main.screenHeight * 0.7f)),
                Scale = Main.rand.NextFloat(0.3f, 1.2f),
                Opacity = 0f,
                Timer = 0f,
                Lifetime = Main.rand.NextFloat(2f, 6f),
                Phase = Main.rand.NextFloat(MathHelper.TwoPi),
                Active = true
            };
        }

        private void SpawnFracture(int index)
        {
            Vector2 start = new Vector2(
                Main.rand.NextFloat(0f, Main.screenWidth),
                Main.rand.NextFloat(0f, Main.screenHeight));
            float angle = Main.rand.NextFloat(MathHelper.TwoPi);
            float length = Main.rand.NextFloat(80f, 300f);
            _fractures[index] = new RealityFracture
            {
                Start = start,
                End = start + angle.ToRotationVector2() * length,
                Opacity = 0f,
                Timer = 0f,
                Lifetime = Main.rand.NextFloat(0.4f, 1.5f),
                Active = true
            };
        }

        public override void Draw(SpriteBatch spriteBatch, float minDepth, float maxDepth)
        {
            if (_opacity <= 0f)
                return;

            if (minDepth < float.MinValue / 2f && maxDepth > float.MinValue / 2f)
            {
                // Draw void gradient sky
                float time = _globalTimer;
                int bossPhase = BossIndexTracker.EnigmaPhase;

                for (int y = 0; y < Main.screenHeight; y += 4)
                {
                    float progress = y / (float)Main.screenHeight;
                    float wave = (float)Math.Sin(progress * 3f + time * 0.3f) * 0.05f;

                    Color topColor = VoidBlack;
                    Color midColor = Color.Lerp(DeepPurple * 0.3f, EerieGreen * 0.1f,
                        (float)Math.Sin(time * 0.15f) * 0.5f + 0.5f);
                    Color botColor = VoidBlack;

                    Color skyColor;
                    if (progress < 0.4f)
                        skyColor = Color.Lerp(topColor, midColor, progress / 0.4f + wave);
                    else
                        skyColor = Color.Lerp(midColor, botColor, (progress - 0.4f) / 0.6f + wave);

                    // Phase escalation: green tinge deepens
                    if (bossPhase >= 1)
                    {
                        float greenInfluence = 0.05f + bossPhase * 0.03f;
                        skyColor = Color.Lerp(skyColor, EerieGreen * 0.1f, greenInfluence);
                    }

                    skyColor *= _opacity;

                    spriteBatch.Draw(
                        Terraria.GameContent.TextureAssets.MagicPixel.Value,
                        new Rectangle(0, y, Main.screenWidth, 4),
                        skyColor);
                }

                // Draw void eyes
                Texture2D pixel = Terraria.GameContent.TextureAssets.MagicPixel.Value;
                for (int i = 0; i < MaxEyes; i++)
                {
                    if (!_eyes[i].Active || _eyes[i].Opacity <= 0f)
                        continue;

                    float alpha = _eyes[i].Opacity * _opacity;
                    float scale = _eyes[i].Scale;
                    Vector2 pos = _eyes[i].Position;
                    float pulse = (float)Math.Sin(_globalTimer * 2f + _eyes[i].Phase) * 0.15f;

                    // Outer glow — deep purple
                    Color outerColor = DeepPurple * (alpha * (0.3f + pulse));
                    int outerSize = (int)(16 * scale);
                    spriteBatch.Draw(pixel,
                        new Rectangle((int)pos.X - outerSize, (int)pos.Y - outerSize / 3,
                            outerSize * 2, outerSize * 2 / 3),
                        outerColor);

                    // Inner iris — eerie green
                    Color irisColor = EerieGreen * (alpha * (0.6f + pulse));
                    int innerSize = (int)(8 * scale);
                    spriteBatch.Draw(pixel,
                        new Rectangle((int)pos.X - innerSize, (int)pos.Y - innerSize / 3,
                            innerSize * 2, innerSize * 2 / 3),
                        irisColor);

                    // Pupil — void black
                    Color pupilColor = VoidBlack * alpha;
                    int pupilSize = (int)(3 * scale);
                    spriteBatch.Draw(pixel,
                        new Rectangle((int)pos.X - pupilSize, (int)pos.Y - pupilSize / 3,
                            pupilSize * 2, pupilSize * 2 / 3),
                        pupilColor);
                }

                // Phase 3: reality fracture lines
                if (bossPhase >= 2)
                {
                    for (int i = 0; i < MaxFractures; i++)
                    {
                        if (!_fractures[i].Active || _fractures[i].Opacity <= 0f)
                            continue;

                        float alpha = _fractures[i].Opacity * _opacity;
                        Color fractureColor = Color.Lerp(EerieGreen, MysteryWhite, Main.rand.NextFloat(0.3f)) * alpha;

                        Vector2 start = _fractures[i].Start;
                        Vector2 end = _fractures[i].End;
                        Vector2 diff = end - start;
                        float length = diff.Length();
                        float rotation = diff.ToRotation();

                        spriteBatch.Draw(pixel,
                            start,
                            new Rectangle(0, 0, 1, 1),
                            fractureColor,
                            rotation,
                            Vector2.Zero,
                            new Vector2(length, 2f),
                            SpriteEffects.None,
                            0f);
                    }
                }

                // Void mist — rolling green haze at the bottom
                for (int x = 0; x < Main.screenWidth; x += 6)
                {
                    float mistHeight = 40f + 15f * (float)Math.Sin(x * 0.02f + _globalTimer * 0.4f + _windSpeed);
                    float mistAlpha = 0.12f * _opacity * (1f + bossPhase * 0.05f);
                    Color mistColor = EerieGreen * mistAlpha;

                    spriteBatch.Draw(pixel,
                        new Rectangle(x, Main.screenHeight - (int)mistHeight, 6, (int)mistHeight),
                        mistColor);
                }
            }
        }

        public override bool IsActive() => _isActive || _opacity > 0f;

        public override void Reset()
        {
            _isActive = false;
            _opacity = 0f;
            _globalTimer = 0f;
            for (int i = 0; i < MaxEyes; i++) _eyes[i].Active = false;
            for (int i = 0; i < MaxFractures; i++) _fractures[i].Active = false;
        }

        public override void Activate(Vector2 position, params object[] args)
        {
            _isActive = true;
        }

        public override void Deactivate(params object[] args)
        {
            _isActive = false;
        }

        public override float GetCloudAlpha() => 0f;

        public override Color OnTileColor(Color inColor)
        {
            return Color.Lerp(inColor, DeepPurple * 0.2f, _opacity * 0.4f);
        }
    }
}
