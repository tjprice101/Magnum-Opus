using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using MagnumOpus.Content.Fate.ResonantWeapons.TheFinalFermata.Utilities;

namespace MagnumOpus.Content.Fate.ResonantWeapons.TheFinalFermata.Particles
{
    /// <summary>
    /// Self-contained particle pool manager for Fermata.
    /// Handles spawn, update, and draw. No shared system dependencies.
    /// Update/Draw are called from the projectile AI/PreDraw with frame guards.
    /// </summary>
    public static class FermataParticleHandler
    {
        private const int PoolSize = 512;
        private static readonly FermataParticle[] Pool = new FermataParticle[PoolSize];
        private static bool _initialized;

        public static void EnsureInitialized()
        {
            if (_initialized) return;
            for (int i = 0; i < PoolSize; i++)
                Pool[i] = new FermataParticle();
            _initialized = true;
        }

        /// <summary>
        /// Spawn a particle from the pool.
        /// </summary>
        public static FermataParticle Spawn(
            Vector2 pos, Vector2 vel, Color color, float scale,
            int lifetime, FermataParticleType type,
            bool additive = true, float rotSpeed = 0f)
        {
            EnsureInitialized();
            for (int i = 0; i < PoolSize; i++)
            {
                if (!Pool[i].Active)
                {
                    var p = Pool[i];
                    p.Position = pos;
                    p.Velocity = vel;
                    p.DrawColor = color;
                    p.Scale = scale;
                    p.Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
                    p.RotationSpeed = rotSpeed;
                    p.LifeTime = 0;
                    p.MaxLifeTime = lifetime;
                    p.Type = type;
                    p.Active = true;
                    p.Additive = additive;
                    p.Opacity = 1f;
                    return p;
                }
            }
            return null;
        }

        /// <summary>
        /// Update all active particles. Call once per game frame (guarded externally).
        /// </summary>
        public static void UpdateAll()
        {
            if (!_initialized) return;
            for (int i = 0; i < PoolSize; i++)
            {
                if (Pool[i].Active)
                    Pool[i].Update();
            }
        }

        /// <summary>
        /// Draw all active particles to the given SpriteBatch.
        /// Handles blend state switching internally.
        /// </summary>
        public static void DrawAll(SpriteBatch sb)
        {
            if (!_initialized) return;

            Texture2D pixel = Terraria.GameContent.TextureAssets.MagicPixel.Value;
            if (pixel == null) return;

            // Pass 1: alpha-blended particles
            for (int i = 0; i < PoolSize; i++)
            {
                var p = Pool[i];
                if (!p.Active || p.Additive) continue;
                DrawParticle(sb, p, pixel);
            }

            // Pass 2: additive particles
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null,
                Main.GameViewMatrix.TransformationMatrix);

            for (int i = 0; i < PoolSize; i++)
            {
                var p = Pool[i];
                if (!p.Active || !p.Additive) continue;
                DrawParticle(sb, p, pixel);
            }

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null,
                Main.GameViewMatrix.TransformationMatrix);
        }

        private static void DrawParticle(SpriteBatch sb, FermataParticle p, Texture2D pixel)
        {
            Vector2 screenPos = p.Position - Main.screenPosition;
            Color col = p.DrawColor * p.Opacity;
            float s = p.Scale;
            Rectangle src = new Rectangle(0, 0, 1, 1);
            Vector2 origin = new Vector2(0.5f);

            switch (p.Type)
            {
                case FermataParticleType.FermataMote:
                    sb.Draw(pixel, screenPos, src, col * 0.7f, p.Rotation, origin, s * 6f, SpriteEffects.None, 0f);
                    break;

                case FermataParticleType.FermataSpark:
                    sb.Draw(pixel, screenPos, src, col, p.Rotation, origin,
                        new Vector2(s * 14f, s * 2f), SpriteEffects.None, 0f);
                    break;

                case FermataParticleType.FermataTimeShard:
                    // Diamond shard: rotated square + inner highlight
                    sb.Draw(pixel, screenPos, src, col * 0.9f,
                        p.Rotation + MathHelper.PiOver4, origin, s * 5f, SpriteEffects.None, 0f);
                    sb.Draw(pixel, screenPos, src, FermataUtils.FlashWhite * p.Opacity * 0.4f,
                        p.Rotation, origin, s * 2.5f, SpriteEffects.None, 0f);
                    break;

                case FermataParticleType.FermataGlyph:
                    // Cross shape: two perpendicular bars
                    sb.Draw(pixel, screenPos, src, col * 0.8f, p.Rotation, origin,
                        new Vector2(s * 8f, s * 2f), SpriteEffects.None, 0f);
                    sb.Draw(pixel, screenPos, src, col * 0.8f,
                        p.Rotation + MathHelper.PiOver2, origin,
                        new Vector2(s * 8f, s * 2f), SpriteEffects.None, 0f);
                    break;

                case FermataParticleType.FermataBloomFlare:
                    // Multi-layer bloom: outer haze + mid glow + bright core
                    sb.Draw(pixel, screenPos, src, col * 0.2f, 0f, origin, s * 24f, SpriteEffects.None, 0f);
                    sb.Draw(pixel, screenPos, src, col * 0.5f, 0f, origin, s * 10f, SpriteEffects.None, 0f);
                    sb.Draw(pixel, screenPos, src, FermataUtils.FlashWhite * p.Opacity * 0.7f,
                        0f, origin, s * 4f, SpriteEffects.None, 0f);
                    break;

                case FermataParticleType.FermataNebulaWisp:
                    // Wispy trail: several offset blobs
                    for (int j = 0; j < 4; j++)
                    {
                        float offset = j * 2.5f;
                        Vector2 wispOff = new Vector2(
                            MathF.Cos(p.Rotation + j * 1.5f),
                            MathF.Sin(p.Rotation + j * 1.5f)) * offset;
                        float wispAlpha = 1f - j * 0.2f;
                        sb.Draw(pixel, screenPos + wispOff, src,
                            col * wispAlpha * 0.45f, 0f, origin,
                            s * (6f - j * 1.2f), SpriteEffects.None, 0f);
                    }
                    break;
            }
        }

        /// <summary>
        /// Kill all active particles.
        /// </summary>
        public static void ClearAll()
        {
            if (!_initialized) return;
            for (int i = 0; i < PoolSize; i++)
                Pool[i].Active = false;
        }
    }
}
