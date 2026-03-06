using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Fate.ResonantWeapons.RequiemOfReality.Particles
{
    /// <summary>
    /// Self-contained particle handler for Requiem of Reality.
    /// Two-pass rendering: additive glows first, then alpha-blended notes/glyphs.
    /// </summary>
    [Autoload(Side = ModSide.Client)]
    public class RequiemParticleHandler : ModSystem
    {
        private static readonly List<RequiemParticle> _particles = new();
        private const int MaxParticles = 800;

        public static void SpawnParticle(RequiemParticle p)
        {
            if (Main.dedServ || p == null) return;
            if (_particles.Count >= MaxParticles)
            {
                // Remove oldest inactive or first particle
                for (int i = 0; i < _particles.Count; i++)
                {
                    if (!_particles[i].Active) { _particles[i] = p; return; }
                }
                _particles[0] = p;
                return;
            }
            _particles.Add(p);
        }

        public override void PostUpdateDusts()
        {
            for (int i = _particles.Count - 1; i >= 0; i--)
            {
                if (!_particles[i].Update())
                    _particles.RemoveAt(i);
            }
        }

        public override void OnModUnload()
        {
            _particles.Clear();
        }

        public override void PostDrawTiles()
        {
            if (_particles.Count == 0) return;

            SpriteBatch sb = Main.spriteBatch;

            // Pass 1: Additive glow particles
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, SamplerState.LinearClamp,
                DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

            for (int i = 0; i < _particles.Count; i++)
            {
                if (_particles[i].Active && _particles[i].UseAdditiveBlend)
                    _particles[i].Draw(sb);
            }

            sb.End();

            // Pass 2: Alpha-blended particles (notes, glyphs, solid sprites)
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

            for (int i = 0; i < _particles.Count; i++)
            {
                if (_particles[i].Active && !_particles[i].UseAdditiveBlend)
                    _particles[i].Draw(sb);
            }

            sb.End();
        }
    }
}
