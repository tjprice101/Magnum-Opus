using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.SwanLake.ResonantWeapons.FeatheroftheIridescentFlock.Particles
{
    [Autoload(Side = ModSide.Client)]
    public class FlockParticleHandler : ModSystem
    {
        private static readonly List<FlockParticle> _particles = new List<FlockParticle>();
        private const int MaxParticles = 400;

        public static void Spawn(FlockParticle particle)
        {
            if (_particles.Count >= MaxParticles)
            {
                for (int i = 0; i < _particles.Count; i++)
                    if (!_particles[i].Active) { _particles[i] = particle; return; }
                _particles.RemoveAt(0);
            }
            _particles.Add(particle);
        }

        public override void PostUpdateEverything()
        {
            for (int i = _particles.Count - 1; i >= 0; i--)
            {
                _particles[i].Update();
                if (!_particles[i].Active) _particles.RemoveAt(i);
            }
        }

        public static void DrawAllParticles(SpriteBatch spriteBatch)
        {
            if (_particles.Count == 0) return;

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, SamplerState.PointClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            foreach (var p in _particles) if (p.Active && p.UseAdditiveBlend) p.Draw(spriteBatch);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            foreach (var p in _particles) if (p.Active && !p.UseAdditiveBlend) p.Draw(spriteBatch);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }

        public override void OnWorldUnload() => _particles.Clear();
    }
}
