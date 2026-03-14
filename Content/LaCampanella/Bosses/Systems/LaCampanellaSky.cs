using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.Bosses;
using MagnumOpus.Common.Systems.VFX.Screen;
using SkySystem = global::MagnumOpus.Common.Systems.LaCampanellaSkySystem;

namespace MagnumOpus.Content.LaCampanella.Bosses.Systems
{
    /// <summary>
    /// Custom sky effect for the La Campanella boss fight — phase-aware redesign.
    ///
    /// Phase 1 (First Toll): Dark smoky sky, gentle falling embers, orange fire glow at horizon.
    ///         Bell silhouette looms in the upper sky.
    /// Phase 2 (Accelerando): Embers fall faster and multiply, smoke thickens across the sky,
    ///         fire glow at horizon intensifies. Bell silhouette sways more aggressively.
    /// Phase 3 (Virtuoso Cascade): Dense ember rain, sky dominated by fire glow,
    ///         smoke obscures upper sky almost completely.
    /// Enrage (Bell Cracking): Sky CRACKS — jagged fissures appear with fire bleeding through.
    ///         Embers become violent sparks, smoke consumes everything, bell silhouette fractures.
    ///
    /// Palette: SootBlack, DeepEmber, InfernalOrange, FlameWhite.
    /// </summary>
    public class LaCampanellaSky : CustomSky
    {
        private bool _isActive;
        private float _opacity;
        private float _intensity; // HP-driven 0..1

        // Phase tracking
        private int _phaseIndex; // 0, 1, 2
        private bool _isEnraged;

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
        private const int MaxEmbers = 100; // Up from 80 — need more for later phases

        // Bell silhouette
        private float _bellSilhouetteAlpha;
        private float _bellSilhouetteY;

        // Sky cracks for enrage
        private struct SkyCrack
        {
            public Vector2 Start;
            public Vector2 End;
            public float FireIntensity;
            public float Width;
        }
        private SkyCrack[] _skyCracks;
        private const int MaxCracks = 6;
        private float _crackProgress; // 0..1 — how revealed the cracks are

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
                    Color = Color.Lerp(new Color(255, 100, 0), new Color(255, 180, 40),
                        (float)rand.NextDouble()),
                    Rotation = (float)rand.NextDouble() * MathHelper.TwoPi,
                    RotSpeed = ((float)rand.NextDouble() - 0.5f) * 0.04f,
                    Flicker = (float)rand.NextDouble() * MathHelper.TwoPi
                };
            }

            // Initialize sky cracks
            _skyCracks = new SkyCrack[MaxCracks];
            for (int i = 0; i < MaxCracks; i++)
            {
                float startX = (float)rand.NextDouble() * Main.screenWidth;
                float startY = (float)rand.NextDouble() * Main.screenHeight * 0.6f;
                float angle = ((float)rand.NextDouble() - 0.5f) * 1.2f + MathHelper.PiOver2 * 0.3f;
                float length = 60f + (float)rand.NextDouble() * 120f;
                _skyCracks[i] = new SkyCrack
                {
                    Start = new Vector2(startX, startY),
                    End = new Vector2(startX + (float)Math.Cos(angle) * length,
                                      startY + (float)Math.Sin(angle) * length),
                    FireIntensity = 0.5f + (float)rand.NextDouble() * 0.5f,
                    Width = 2f + (float)rand.NextDouble() * 4f
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

                // Phase detection from LaCampanellaSkySystem state
                _phaseIndex = SkySystem.BossPhaseIndex;
                _isEnraged = SkySystem.BossEnraged;

                // Bell silhouette — always present from Phase 1, more agitated later
                float silTargetAlpha = _phaseIndex >= 0 ? 0.5f + _phaseIndex * 0.15f : 0f;
                if (_isEnraged) silTargetAlpha = 0.8f;
                _bellSilhouetteAlpha = MathHelper.Lerp(_bellSilhouetteAlpha, silTargetAlpha, 0.02f);

                float swaySpeed = 0.008f + _phaseIndex * 0.004f;
                float swayMag = 15f + _phaseIndex * 8f;
                if (_isEnraged) { swaySpeed = 0.025f; swayMag = 30f; }
                _bellSilhouetteY = (float)Math.Sin(Main.timeForVisualEffects * swaySpeed) * swayMag;

                // Sky cracks — only during enrage, progress toward full reveal
                if (_isEnraged)
                    _crackProgress = Math.Min(1f, _crackProgress + 0.008f);
                else
                    _crackProgress = Math.Max(0f, _crackProgress - 0.02f);

                // Update falling embers
                if (_embers != null)
                {
                    // Phase-driven: how many embers are active
                    int activeEmbers = _phaseIndex switch
                    {
                        0 => 50,
                        1 => 75,
                        _ => MaxEmbers
                    };
                    if (_isEnraged) activeEmbers = MaxEmbers;

                    float speedMult = 1f + _phaseIndex * 0.5f + _intensity * 0.8f;
                    if (_isEnraged) speedMult *= 1.5f;

                    for (int i = 0; i < MaxEmbers; i++)
                    {
                        if (i >= activeEmbers)
                        {
                            // Inactive embers stay off-screen
                            _embers[i].Position.Y = -100;
                            continue;
                        }

                        _embers[i].Position.Y += _embers[i].Speed * speedMult;
                        float drift = (float)Math.Sin(Main.timeForVisualEffects * 0.012f + i * 0.7f) * 0.4f;
                        if (_isEnraged) drift *= 3f; // Violent sideways motion
                        _embers[i].Position.X += drift;
                        _embers[i].Rotation += _embers[i].RotSpeed * speedMult;
                        _embers[i].Flicker += 0.08f + _phaseIndex * 0.03f;

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
                Texture2D pixel = Terraria.GameContent.TextureAssets.MagicPixel.Value;

                // === SKY GRADIENT — darkens and reddens with phase ===
                float phaseShift = _phaseIndex * 0.2f;
                if (_isEnraged) phaseShift = 0.8f;

                Color topColor = Color.Lerp(
                    new Color(15, 10, 8),
                    new Color(40, 15, 5), _intensity + phaseShift * 0.5f);
                Color midColor = Color.Lerp(
                    new Color(40, 25, 15),
                    new Color(140, 50, 10), _intensity + phaseShift * 0.3f);
                Color bottomColor = Color.Lerp(
                    new Color(80, 45, 15),
                    new Color(220, 100, 20), _intensity + phaseShift * 0.4f);

                topColor *= _opacity * 0.75f;
                midColor *= _opacity * 0.75f;
                bottomColor *= _opacity * 0.75f;

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

                // === SMOKE LAYER — phase-driven density ===
                // Horizontal smoke bands that thicken with phase
                float smokeDensity = 0.03f + _phaseIndex * 0.02f + _intensity * 0.02f;
                if (_isEnraged) smokeDensity = 0.12f;
                int smokeBands = 3 + _phaseIndex * 2;
                if (_isEnraged) smokeBands = 10;

                for (int band = 0; band < smokeBands; band++)
                {
                    float bandY = Main.screenHeight * (0.1f + band * 0.12f + 
                        (float)Math.Sin(Main.timeForVisualEffects * 0.003f + band * 1.3f) * 0.05f);
                    int bandHeight = 30 + _phaseIndex * 10;
                    if (_isEnraged) bandHeight += 20;
                    Color smokeCol = new Color(20, 15, 20) * _opacity * smokeDensity;
                    spriteBatch.Draw(pixel, new Rectangle(0, (int)bandY, Main.screenWidth, bandHeight), smokeCol);
                }

                // === SKY CRACKS — enrage only, fire bleeds through ===
                if (_crackProgress > 0.01f && _skyCracks != null)
                {
                    for (int i = 0; i < MaxCracks; i++)
                    {
                        SkyCrack crack = _skyCracks[i];
                        float fireFlicker = 0.5f + (float)Math.Sin(Main.timeForVisualEffects * 0.05f + i * 2.1f) * 0.5f;
                        float alpha = _crackProgress * crack.FireIntensity * fireFlicker * _opacity;

                        // Fire glow along the crack — orange light bleeding through
                        Color crackFireColor = Color.Lerp(new Color(255, 100, 0), new Color(255, 230, 200), fireFlicker * 0.4f);
                        crackFireColor *= alpha * 0.6f;
                        crackFireColor.A = 0; // Additive

                        // Draw crack as a thick line using rectangles
                        Vector2 dir = crack.End - crack.Start;
                        float length = dir.Length();
                        float angle = (float)Math.Atan2(dir.Y, dir.X);
                        int crackWidth = (int)(crack.Width * _crackProgress * 2f);

                        // Wide fire glow around crack
                        Color wideGlow = new Color(255, 100, 0) * (alpha * 0.15f);
                        wideGlow.A = 0;
                        spriteBatch.Draw(pixel,
                            crack.Start + dir * 0.5f,
                            new Rectangle(0, 0, 1, 1),
                            wideGlow,
                            angle,
                            new Vector2(0.5f, 0.5f),
                            new Vector2(length, crackWidth * 4f),
                            SpriteEffects.None, 0f);

                        // Bright core
                        spriteBatch.Draw(pixel,
                            crack.Start + dir * 0.5f,
                            new Rectangle(0, 0, 1, 1),
                            crackFireColor,
                            angle,
                            new Vector2(0.5f, 0.5f),
                            new Vector2(length, Math.Max(1, crackWidth)),
                            SpriteEffects.None, 0f);
                    }
                }

                // === BELL SILHOUETTE — looms in upper sky ===
                if (_bellSilhouetteAlpha > 0.01f)
                {
                    Color bellColor = new Color(10, 5, 5) * _opacity * _bellSilhouetteAlpha * 0.4f;
                    float bellX = Main.screenWidth * 0.5f;
                    float bellY = Main.screenHeight * 0.15f + _bellSilhouetteY;

                    // Bell body
                    spriteBatch.Draw(pixel, new Rectangle((int)(bellX - 30), (int)bellY, 60, 50), bellColor);
                    spriteBatch.Draw(pixel, new Rectangle((int)(bellX - 20), (int)(bellY - 20), 40, 25), bellColor);
                    spriteBatch.Draw(pixel, new Rectangle((int)(bellX - 8), (int)(bellY - 35), 16, 20), bellColor);

                    // Enrage: fracture lines across the bell silhouette
                    if (_isEnraged && _crackProgress > 0.1f)
                    {
                        Color fractureColor = new Color(255, 100, 0) * (_opacity * _crackProgress * 0.3f);
                        fractureColor.A = 0;
                        // Diagonal crack across bell body
                        spriteBatch.Draw(pixel, new Rectangle((int)(bellX - 25), (int)(bellY + 5), 50, 2), fractureColor);
                        spriteBatch.Draw(pixel, new Rectangle((int)(bellX - 15), (int)(bellY + 20), 30, 2), fractureColor);
                        spriteBatch.Draw(pixel, new Rectangle((int)(bellX + 5), (int)(bellY - 10), 2, 40), fractureColor);
                    }
                }

                // === FALLING EMBERS ===
                if (_embers != null && _opacity > 0.1f)
                {
                    for (int i = 0; i < MaxEmbers; i++)
                    {
                        if (_embers[i].Position.Y < -50) continue;

                        float flicker = 0.6f + (float)Math.Sin(_embers[i].Flicker) * 0.4f;
                        float phaseAlpha = 0.3f + _intensity * 0.5f + _phaseIndex * 0.1f;
                        if (_isEnraged) phaseAlpha = 1f;
                        Color c = _embers[i].Color * _opacity * phaseAlpha * flicker;
                        c.A = 0; // Additive
                        float s = _embers[i].Scale * (1f + _intensity * 0.4f + _phaseIndex * 0.15f);
                        if (_isEnraged) s *= 1.3f; // Larger, angrier embers
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
            float phaseShift = _phaseIndex * 0.05f;
            if (_isEnraged) phaseShift = 0.15f;
            Color tint = Color.Lerp(Color.White,
                new Color(255, 160, 100),
                _opacity * (0.35f + _intensity * 0.25f + phaseShift));
            return inColor.MultiplyRGBA(tint);
        }

        public override float GetCloudAlpha() => 1f - _opacity * (0.7f + _phaseIndex * 0.1f);
    }
}
