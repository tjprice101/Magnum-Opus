using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Content.Fate.ResonantWeapons.TheFinalFermata.Utilities;

namespace MagnumOpus.Content.Fate.ResonantWeapons.TheFinalFermata.Primitives
{
    /// <summary>
    /// Self-contained trail strip renderer for Fermata projectiles.
    /// Uses SpriteBatch quad drawing — no custom vertex buffers or external shaders required.
    /// </summary>
    public class FermataTrailRenderer
    {
        private readonly Vector2[] _positions;
        private readonly float[] _rotations;
        private int _head;
        private int _count;
        private FermataTrailSettings _settings;

        public FermataTrailRenderer(FermataTrailSettings settings)
        {
            _settings = settings ?? FermataTrailSettings.SwordOrbitTrail();
            _positions = new Vector2[_settings.TrailLength];
            _rotations = new float[_settings.TrailLength];
            _head = 0;
            _count = 0;
        }

        /// <summary>
        /// Record a new trail position. Call every few frames for smooth trails.
        /// </summary>
        public void RecordPosition(Vector2 pos, float rotation)
        {
            _positions[_head] = pos;
            _rotations[_head] = rotation;
            _head = (_head + 1) % _positions.Length;
            if (_count < _positions.Length)
                _count++;
        }

        /// <summary>
        /// Reset all stored positions to a single point.
        /// </summary>
        public void Reset(Vector2 pos)
        {
            for (int i = 0; i < _positions.Length; i++)
            {
                _positions[i] = pos;
                _rotations[i] = 0f;
            }
            _head = 0;
            _count = 0;
        }

        /// <summary>
        /// Draw the trail strip using SpriteBatch (MagicPixel quads).
        /// Handles additive blend mode switching if configured.
        /// </summary>
        public void Draw(SpriteBatch sb, float globalOpacity = 1f)
        {
            if (_count < 2) return;

            Texture2D pixel = MagnumTextureRegistry.GetSoftGlow();
            if (pixel == null) return;

            bool wasAdditive = false;
            if (_settings.Additive)
            {
                sb.End();
                sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, SamplerState.LinearClamp,
                    DepthStencilState.None, RasterizerState.CullNone, null,
                    Main.GameViewMatrix.TransformationMatrix);
                wasAdditive = true;
            }

            // Draw segments from newest to oldest
            for (int i = 0; i < _count - 1; i++)
            {
                int idxA = (_head - 1 - i + _positions.Length * 2) % _positions.Length;
                int idxB = (_head - 2 - i + _positions.Length * 2) % _positions.Length;

                Vector2 posA = _positions[idxA];
                Vector2 posB = _positions[idxB];

                // Progress: 0 = newest, 1 = oldest
                float progressA = (float)i / (_count - 1);
                float progressB = (float)(i + 1) / (_count - 1);

                float widthA = _settings.MaxWidth * _settings.WidthCurve(progressA);
                float widthB = _settings.MaxWidth * _settings.WidthCurve(progressB);

                Color colorA = _settings.ColorGradient(progressA) * _settings.Opacity * globalOpacity;
                Color colorB = _settings.ColorGradient(progressB) * _settings.Opacity * globalOpacity;

                // Draw this segment as a line quad
                DrawSegment(sb, pixel, posA, posB, widthA, widthB, colorA, colorB);
            }

            if (wasAdditive)
            {
                sb.End();
                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                    DepthStencilState.None, RasterizerState.CullNone, null,
                    Main.GameViewMatrix.TransformationMatrix);
            }
        }

        private void DrawSegment(SpriteBatch sb, Texture2D pixel,
            Vector2 posA, Vector2 posB, float widthA, float widthB,
            Color colorA, Color colorB)
        {
            Vector2 screenA = posA - Main.screenPosition;
            Vector2 screenB = posB - Main.screenPosition;

            Vector2 dir = screenB - screenA;
            float length = dir.Length();
            if (length < 0.5f) return;

            float angle = dir.ToRotation();
            float avgWidth = (widthA + widthB) * 0.5f;
            Color avgColor = Color.Lerp(colorA, colorB, 0.5f);

            // Draw as a rotated rectangle from A to B
            sb.Draw(pixel, screenA, new Rectangle(0, 0, 1, 1), avgColor,
                angle, new Vector2(0f, 0.5f),
                new Vector2(length, avgWidth),
                SpriteEffects.None, 0f);
        }

        /// <summary>
        /// Get the most recent recorded position.
        /// </summary>
        public Vector2 GetLatestPosition()
        {
            if (_count == 0) return Vector2.Zero;
            int idx = (_head - 1 + _positions.Length) % _positions.Length;
            return _positions[idx];
        }

        /// <summary>
        /// Provides the current trail settings (for dynamic adjustment).
        /// </summary>
        public FermataTrailSettings Settings
        {
            get => _settings;
            set => _settings = value ?? _settings;
        }
    }
}
