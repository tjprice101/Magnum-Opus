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
    /// Custom sky for Enigma, the Hollow Mystery — redesigned for phase identity.
    ///
    /// Phase 1 — The Riddle: Arena is subtly wrong. Faint green symbols drift
    ///           at the periphery of vision, disappearing when the player
    ///           looks directly at them. Something is watching but you can't
    ///           quite see it.
    /// Phase 2 — The Unraveling: Watching eyes materialize in the sky and
    ///           TRACK the player. Reality distortion warps screen edges.
    ///           The void deepens, green tinge intensifies.
    /// Phase 3 — The Revelation: Full void space lit by arcane green fire.
    ///           Constant reality fracture lines tear across the sky.
    ///           Everything is wrong. Nothing makes sense.
    /// Enrage — Total Mystery: Screen-tone inversion flicker, reality rips
    ///          as literal tears in the rendering, unsettling white flashes.
    /// </summary>
    public class EnigmaSky : CustomSky
    {
        private bool _isActive;
        private float _opacity;
        private float _windSpeed;

        private static readonly Color VoidBlack = new Color(10, 5, 15);
        private static readonly Color DeepPurple = new Color(80, 20, 140);
        private static readonly Color EerieGreen = new Color(40, 220, 80);
        private static readonly Color ArcaneGreen = new Color(100, 255, 130);
        private static readonly Color UnsettlingWhite = new Color(220, 200, 255);
        private static readonly Color VoidPurple = new Color(50, 10, 80);

        #region Peripheral Symbols (Phase 1)

        /// <summary>
        /// Faint green symbols that drift at the PERIPHERY of vision.
        /// They disappear when the player "looks directly" — i.e., when close
        /// to screen center. This creates the feeling of something wrong
        /// that you can't quite identify.
        /// </summary>
        private struct PeripheralSymbol
        {
            public Vector2 Position;
            public float Scale;
            public float Opacity;
            public float Timer;
            public float Lifetime;
            public float Phase;     // animation phase offset
            public float GlyphType; // which glyph shape (0-1 range, selects variant)
            public bool Active;
        }

        private const int MaxSymbols = 24;
        private PeripheralSymbol[] _symbols = new PeripheralSymbol[MaxSymbols];

        #endregion

        #region Watching Eyes (Phase 2+)

        /// <summary>
        /// Eyes that materialize in the sky and TRACK the player's position.
        /// Phase 2: sparse, half-visible. Phase 3: numerous, staring.
        /// Enrage: everywhere, unblinking.
        /// </summary>
        private struct WatchingEye
        {
            public Vector2 Position;
            public float Scale;
            public float Opacity;
            public float Timer;
            public float Lifetime;
            public float Phase;
            public float PupilAngle; // Tracks toward player
            public bool Active;
        }

        private const int MaxEyes = 40;
        private WatchingEye[] _eyes = new WatchingEye[MaxEyes];

        #endregion

        #region Reality Fractures (Phase 3+)

        /// <summary>
        /// Lines of broken reality that tear across the sky.
        /// Phase 3: occasional. Enrage: constant and severe.
        /// </summary>
        private struct RealityFracture
        {
            public Vector2 Start;
            public Vector2 End;
            public float Opacity;
            public float Timer;
            public float Lifetime;
            public float Width;
            public bool Active;
        }

        private const int MaxFractures = 16;
        private RealityFracture[] _fractures = new RealityFracture[MaxFractures];

        #endregion

        private float _globalTimer;
        private float _inversionFlicker; // Enrage screen inversion

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
            bool isEnraged = BossIndexTracker.EnigmaEnraged;

            UpdatePeripheralSymbols(bossPhase);
            UpdateWatchingEyes(bossPhase, isEnraged);
            UpdateRealityFractures(bossPhase, isEnraged);

            // Enrage: flickering inversion
            if (isEnraged)
            {
                _inversionFlicker = (float)Math.Sin(_globalTimer * 4.5f) * (float)Math.Sin(_globalTimer * 7.3f);
                _inversionFlicker = _inversionFlicker > 0.6f ? 1f : 0f; // Hard flicker, not smooth
            }
            else
            {
                _inversionFlicker = 0f;
            }
        }

        private void UpdatePeripheralSymbols(int bossPhase)
        {
            // Symbols are most active in Phase 1, fade in later phases as eyes take over
            int spawnChance = bossPhase switch
            {
                0 => 20,  // Active in Phase 1
                1 => 50,  // Fading
                _ => 120  // Rare in Phase 3
            };

            for (int i = 0; i < MaxSymbols; i++)
            {
                if (_symbols[i].Active)
                {
                    _symbols[i].Timer += 0.016f;
                    float life = _symbols[i].Timer / _symbols[i].Lifetime;

                    // Fade in, drift, fade out
                    if (life < 0.2f)
                        _symbols[i].Opacity = life / 0.2f;
                    else if (life > 0.7f)
                        _symbols[i].Opacity = 1f - (life - 0.7f) / 0.3f;
                    else
                        _symbols[i].Opacity = 1f;

                    // Drift slowly
                    _symbols[i].Position += new Vector2(_windSpeed * 0.5f, -0.2f);

                    if (_symbols[i].Timer >= _symbols[i].Lifetime)
                        _symbols[i].Active = false;
                }
                else if (Main.rand.NextBool(spawnChance))
                {
                    SpawnPeripheralSymbol(i);
                }
            }
        }

        private void SpawnPeripheralSymbol(int index)
        {
            // Spawn at screen EDGES — never near center
            float edge = Main.rand.NextFloat();
            Vector2 pos;
            if (edge < 0.25f) // Left edge
                pos = new Vector2(Main.rand.NextFloat(0, Main.screenWidth * 0.15f), Main.rand.NextFloat(0, Main.screenHeight));
            else if (edge < 0.5f) // Right edge
                pos = new Vector2(Main.rand.NextFloat(Main.screenWidth * 0.85f, Main.screenWidth), Main.rand.NextFloat(0, Main.screenHeight));
            else if (edge < 0.75f) // Top edge
                pos = new Vector2(Main.rand.NextFloat(0, Main.screenWidth), Main.rand.NextFloat(0, Main.screenHeight * 0.15f));
            else // Bottom edge
                pos = new Vector2(Main.rand.NextFloat(0, Main.screenWidth), Main.rand.NextFloat(Main.screenHeight * 0.85f, Main.screenHeight));

            _symbols[index] = new PeripheralSymbol
            {
                Position = pos,
                Scale = Main.rand.NextFloat(0.4f, 1.0f),
                Opacity = 0f,
                Timer = 0f,
                Lifetime = Main.rand.NextFloat(2f, 5f),
                Phase = Main.rand.NextFloat(MathHelper.TwoPi),
                GlyphType = Main.rand.NextFloat(),
                Active = true
            };
        }

        private void UpdateWatchingEyes(int bossPhase, bool isEnraged)
        {
            // Eyes appear Phase 2+, swarm during enrage
            int spawnChance = bossPhase switch
            {
                0 => 200, // Almost never in Phase 1
                1 => 40,  // Building presence
                _ => 20   // Numerous in Phase 3
            };
            if (isEnraged) spawnChance = 8;

            // Get player screen center for pupil tracking
            Vector2 playerScreenPos = Vector2.Zero;
            if (Main.LocalPlayer != null && Main.LocalPlayer.active)
            {
                playerScreenPos = Main.LocalPlayer.Center - Main.screenPosition;
            }

            for (int i = 0; i < MaxEyes; i++)
            {
                if (_eyes[i].Active)
                {
                    _eyes[i].Timer += 0.016f;
                    float life = _eyes[i].Timer / _eyes[i].Lifetime;

                    // Blink open, stare, blink shut
                    if (life < 0.15f)
                        _eyes[i].Opacity = life / 0.15f;
                    else if (life < 0.7f)
                    {
                        _eyes[i].Opacity = 1f;
                        // Occasional blink
                        float blink = (float)Math.Sin((_globalTimer + _eyes[i].Phase) * 3f);
                        if (blink > 0.95f) _eyes[i].Opacity = 0.2f;
                    }
                    else
                        _eyes[i].Opacity = 1f - (life - 0.7f) / 0.3f;

                    // Track player — pupil angle rotates toward player position
                    Vector2 toPlayer = playerScreenPos - _eyes[i].Position;
                    if (toPlayer.Length() > 1f)
                    {
                        float targetAngle = toPlayer.ToRotation();
                        _eyes[i].PupilAngle = MathHelper.Lerp(_eyes[i].PupilAngle, targetAngle, 0.05f);
                    }

                    if (_eyes[i].Timer >= _eyes[i].Lifetime)
                        _eyes[i].Active = false;
                }
                else if (Main.rand.NextBool(spawnChance))
                {
                    SpawnWatchingEye(i);
                }
            }
        }

        private void SpawnWatchingEye(int index)
        {
            _eyes[index] = new WatchingEye
            {
                Position = new Vector2(
                    Main.rand.NextFloat(0f, Main.screenWidth),
                    Main.rand.NextFloat(0f, Main.screenHeight * 0.7f)),
                Scale = Main.rand.NextFloat(0.3f, 1.2f),
                Opacity = 0f,
                Timer = 0f,
                Lifetime = Main.rand.NextFloat(3f, 7f),
                Phase = Main.rand.NextFloat(MathHelper.TwoPi),
                PupilAngle = Main.rand.NextFloat(MathHelper.TwoPi),
                Active = true
            };
        }

        private void UpdateRealityFractures(int bossPhase, bool isEnraged)
        {
            // Fractures: Phase 3+ only, overwhelming during enrage
            if (bossPhase < 2 && !isEnraged) return;

            int spawnChance = isEnraged ? 15 : 60;

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
                else if (Main.rand.NextBool(spawnChance))
                {
                    SpawnFracture(i);
                }
            }
        }

        private void SpawnFracture(int index)
        {
            Vector2 start = new Vector2(
                Main.rand.NextFloat(0f, Main.screenWidth),
                Main.rand.NextFloat(0f, Main.screenHeight));
            float angle = Main.rand.NextFloat(MathHelper.TwoPi);
            float length = Main.rand.NextFloat(80f, 350f);
            bool isEnraged = BossIndexTracker.EnigmaEnraged;

            _fractures[index] = new RealityFracture
            {
                Start = start,
                End = start + angle.ToRotationVector2() * length,
                Opacity = 0f,
                Timer = 0f,
                Lifetime = isEnraged ? Main.rand.NextFloat(0.2f, 0.8f) : Main.rand.NextFloat(0.4f, 1.5f),
                Width = isEnraged ? Main.rand.NextFloat(2f, 5f) : Main.rand.NextFloat(1f, 3f),
                Active = true
            };
        }

        public override void Draw(SpriteBatch spriteBatch, float minDepth, float maxDepth)
        {
            if (_opacity <= 0f) return;

            if (!(minDepth < float.MinValue / 2f && maxDepth > float.MinValue / 2f))
                return;

            int bossPhase = BossIndexTracker.EnigmaPhase;
            bool isEnraged = BossIndexTracker.EnigmaEnraged;
            Texture2D pixel = Terraria.GameContent.TextureAssets.MagicPixel.Value;

            // ===== VOID GRADIENT SKY =====
            DrawVoidGradient(spriteBatch, pixel, bossPhase, isEnraged);

            // ===== PERIPHERAL SYMBOLS (Phase 1 dominant) =====
            DrawPeripheralSymbols(spriteBatch, pixel);

            // ===== WATCHING EYES (Phase 2+) =====
            DrawWatchingEyes(spriteBatch, pixel, bossPhase);

            // ===== REALITY FRACTURES (Phase 3+) =====
            if (bossPhase >= 2 || isEnraged)
                DrawRealityFractures(spriteBatch, pixel, isEnraged);

            // ===== ARCANE GREEN FIRE (Phase 3 void arena lighting) =====
            if (bossPhase >= 2)
                DrawArcaneFireLighting(spriteBatch, pixel);

            // ===== VOID MIST =====
            DrawVoidMist(spriteBatch, pixel, bossPhase, isEnraged);

            // ===== ENRAGE: INVERSION FLICKER =====
            if (isEnraged && _inversionFlicker > 0.5f)
                DrawInversionOverlay(spriteBatch, pixel);
        }

        private void DrawVoidGradient(SpriteBatch spriteBatch, Texture2D pixel, int bossPhase, bool isEnraged)
        {
            for (int y = 0; y < Main.screenHeight; y += 4)
            {
                float progress = y / (float)Main.screenHeight;
                float wave = (float)Math.Sin(progress * 3f + _globalTimer * 0.3f) * 0.05f;

                Color topColor = VoidBlack;
                Color botColor = VoidBlack;

                // Mid-band color evolves with phase
                Color midColor = bossPhase switch
                {
                    0 => Color.Lerp(DeepPurple * 0.2f, VoidPurple * 0.15f,
                        (float)Math.Sin(_globalTimer * 0.15f) * 0.5f + 0.5f),
                    1 => Color.Lerp(DeepPurple * 0.3f, EerieGreen * 0.15f,
                        (float)Math.Sin(_globalTimer * 0.15f) * 0.5f + 0.5f),
                    _ => Color.Lerp(EerieGreen * 0.2f, ArcaneGreen * 0.1f,
                        (float)Math.Sin(_globalTimer * 0.2f) * 0.5f + 0.5f)
                };

                Color skyColor;
                if (progress < 0.4f)
                    skyColor = Color.Lerp(topColor, midColor, progress / 0.4f + wave);
                else
                    skyColor = Color.Lerp(midColor, botColor, (progress - 0.4f) / 0.6f + wave);

                // Phase escalation
                if (bossPhase >= 1)
                {
                    float greenInfluence = 0.03f + bossPhase * 0.04f;
                    if (isEnraged) greenInfluence *= 2f;
                    skyColor = Color.Lerp(skyColor, EerieGreen * 0.08f, greenInfluence);
                }

                skyColor *= _opacity;
                spriteBatch.Draw(pixel, new Rectangle(0, y, Main.screenWidth, 4), skyColor);
            }
        }

        private void DrawPeripheralSymbols(SpriteBatch spriteBatch, Texture2D pixel)
        {
            // Symbols disappear when close to screen center — the "can't look directly" effect
            Vector2 screenCenter = new Vector2(Main.screenWidth * 0.5f, Main.screenHeight * 0.5f);

            for (int i = 0; i < MaxSymbols; i++)
            {
                if (!_symbols[i].Active || _symbols[i].Opacity <= 0f) continue;

                Vector2 pos = _symbols[i].Position;
                float distFromCenter = Vector2.Distance(pos, screenCenter);
                float maxDist = Math.Max(Main.screenWidth, Main.screenHeight) * 0.5f;

                // Peripheral visibility: stronger at edges, invisible near center
                float peripheralFade = MathHelper.Clamp(distFromCenter / (maxDist * 0.6f), 0f, 1f);
                peripheralFade = peripheralFade * peripheralFade; // Sharper falloff

                float alpha = _symbols[i].Opacity * _opacity * peripheralFade * 0.4f;
                if (alpha < 0.01f) continue;

                float scale = _symbols[i].Scale;
                float pulse = (float)Math.Sin(_globalTimer * 1.5f + _symbols[i].Phase) * 0.3f;
                Color symbolColor = EerieGreen * (alpha * (0.5f + pulse));

                // Simple glyph: cross pattern with varying orientation
                float rotation = _symbols[i].Phase + _globalTimer * 0.3f;
                int size = (int)(6 * scale);

                // Horizontal bar
                spriteBatch.Draw(pixel,
                    new Rectangle((int)pos.X - size, (int)pos.Y - 1, size * 2, 2),
                    null, symbolColor, rotation, Vector2.One, SpriteEffects.None, 0f);
                // Vertical bar
                spriteBatch.Draw(pixel,
                    new Rectangle((int)pos.X - 1, (int)pos.Y - size, 2, size * 2),
                    null, symbolColor, rotation, Vector2.One, SpriteEffects.None, 0f);
            }
        }

        private void DrawWatchingEyes(SpriteBatch spriteBatch, Texture2D pixel, int bossPhase)
        {
            for (int i = 0; i < MaxEyes; i++)
            {
                if (!_eyes[i].Active || _eyes[i].Opacity <= 0f) continue;

                float alpha = _eyes[i].Opacity * _opacity;
                // Phase 2 eyes are dimmer/more ghostly
                if (bossPhase < 2) alpha *= 0.6f;

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

                // Pupil — void black, OFFSET toward player
                float pupilOffset = 2f * scale;
                Vector2 pupilDir = _eyes[i].PupilAngle.ToRotationVector2() * pupilOffset;
                Color pupilColor = VoidBlack * alpha;
                int pupilSize = (int)(3 * scale);
                int px = (int)(pos.X + pupilDir.X);
                int py = (int)(pos.Y + pupilDir.Y);
                spriteBatch.Draw(pixel,
                    new Rectangle(px - pupilSize, py - pupilSize / 3,
                        pupilSize * 2, pupilSize * 2 / 3),
                    pupilColor);
            }
        }

        private void DrawRealityFractures(SpriteBatch spriteBatch, Texture2D pixel, bool isEnraged)
        {
            for (int i = 0; i < MaxFractures; i++)
            {
                if (!_fractures[i].Active || _fractures[i].Opacity <= 0f) continue;

                float alpha = _fractures[i].Opacity * _opacity;
                float width = _fractures[i].Width;

                // Fracture color: green with flickers of white
                Color fractureColor = Color.Lerp(EerieGreen, ArcaneGreen,
                    (float)Math.Sin(_globalTimer * 5f + i) * 0.5f + 0.5f) * alpha;

                // Enrage fractures have void-dark edges
                if (isEnraged)
                    fractureColor = Color.Lerp(fractureColor, UnsettlingWhite, 0.2f);

                Vector2 start = _fractures[i].Start;
                Vector2 end = _fractures[i].End;
                Vector2 diff = end - start;
                float length = diff.Length();
                float rotation = diff.ToRotation();

                // Main fracture line
                spriteBatch.Draw(pixel, start, new Rectangle(0, 0, 1, 1),
                    fractureColor, rotation, Vector2.Zero,
                    new Vector2(length, width), SpriteEffects.None, 0f);

                // Glow around the fracture — green fire
                Color glowColor = EerieGreen * (alpha * 0.3f);
                spriteBatch.Draw(pixel, start, new Rectangle(0, 0, 1, 1),
                    glowColor, rotation, new Vector2(0, 0.5f),
                    new Vector2(length, width * 3f), SpriteEffects.None, 0f);
            }
        }

        private void DrawArcaneFireLighting(SpriteBatch spriteBatch, Texture2D pixel)
        {
            // Phase 3: floating green fire spots that illuminate the void
            float time = _globalTimer;
            for (int i = 0; i < 8; i++)
            {
                float x = Main.screenWidth * (0.1f + 0.8f * ((float)Math.Sin(time * 0.3f + i * 1.7f) * 0.5f + 0.5f));
                float y = Main.screenHeight * (0.2f + 0.5f * ((float)Math.Cos(time * 0.2f + i * 2.3f) * 0.5f + 0.5f));
                float fireSize = 30f + 15f * (float)Math.Sin(time * 2f + i);
                float fireAlpha = 0.06f * _opacity;

                Color fireColor = Color.Lerp(EerieGreen, ArcaneGreen,
                    (float)Math.Sin(time + i) * 0.5f + 0.5f) * fireAlpha;

                spriteBatch.Draw(pixel,
                    new Rectangle((int)(x - fireSize), (int)(y - fireSize),
                        (int)(fireSize * 2), (int)(fireSize * 2)),
                    fireColor);
            }
        }

        private void DrawVoidMist(SpriteBatch spriteBatch, Texture2D pixel, int bossPhase, bool isEnraged)
        {
            float mistIntensity = bossPhase switch
            {
                0 => 0.08f,
                1 => 0.12f,
                _ => 0.18f
            };
            if (isEnraged) mistIntensity = 0.25f;

            for (int x = 0; x < Main.screenWidth; x += 6)
            {
                float mistHeight = 40f + 15f * (float)Math.Sin(x * 0.02f + _globalTimer * 0.4f + _windSpeed);
                if (bossPhase >= 2) mistHeight *= 1.5f;
                float mistAlpha = mistIntensity * _opacity;

                Color mistColor = Color.Lerp(EerieGreen, ArcaneGreen,
                    (float)Math.Sin(x * 0.01f + _globalTimer * 0.3f) * 0.5f + 0.5f) * mistAlpha;

                spriteBatch.Draw(pixel,
                    new Rectangle(x, Main.screenHeight - (int)mistHeight, 6, (int)mistHeight),
                    mistColor);
            }
        }

        private void DrawInversionOverlay(SpriteBatch spriteBatch, Texture2D pixel)
        {
            // Brief unsettling white flash — the screen "inverts" momentarily
            Color invColor = UnsettlingWhite * (_opacity * 0.15f);
            spriteBatch.Draw(pixel, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), invColor);
        }

        public override bool IsActive() => _isActive || _opacity > 0f;

        public override void Reset()
        {
            _isActive = false;
            _opacity = 0f;
            _globalTimer = 0f;
            _inversionFlicker = 0f;
            for (int i = 0; i < MaxSymbols; i++) _symbols[i].Active = false;
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
            int phase = BossIndexTracker.EnigmaPhase;
            Color tintColor = phase switch
            {
                0 => DeepPurple * 0.15f,
                1 => Color.Lerp(DeepPurple, EerieGreen, 0.3f) * 0.2f,
                _ => EerieGreen * 0.15f
            };
            return Color.Lerp(inColor, tintColor, _opacity * 0.4f);
        }
    }
}
