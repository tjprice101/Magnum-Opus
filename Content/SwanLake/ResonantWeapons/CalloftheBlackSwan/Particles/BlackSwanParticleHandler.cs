using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.SwanLake.ResonantWeapons.CalloftheBlackSwan.Particles
{
    /// <summary>
    /// ModSystem that manages all Call of the Black Swan particles.
    /// Ticks, draws, and culls particles independently from any shared system.
    /// </summary>
    [Autoload(Side = ModSide.Client)]
    public class BlackSwanParticleHandler : ModSystem
    {
        private static readonly List<BlackSwanParticle> _particles = new List<BlackSwanParticle>();
        private const int MaxParticles = 400;

        /// <summary>Spawn a new particle.</summary>
        public static void SpawnParticle(BlackSwanParticle particle)
        {
            if (_particles.Count >= MaxParticles)
                return;
            _particles.Add(particle);
        }

        public override void PostUpdateEverything()
        {
            for (int i = _particles.Count - 1; i >= 0; i--)
            {
                var p = _particles[i];
                p.Position += p.Velocity;
                p.Time++;
                p.Update();

                if (p.ShouldRemove)
                    _particles.RemoveAt(i);
            }
        }

        /// <summary>
        /// Call this from a draw hook (e.g., On_Main.DrawDust or a ModSystem.PostDrawDust).
        /// Draws all active particles in additive and alpha-blend passes.
        /// </summary>
        public static void DrawAllParticles(SpriteBatch spriteBatch)
        {
            if (_particles.Count == 0)
                return;

            // Pass 1: Additive particles
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            foreach (var p in _particles)
            {
                if (p.UseAdditiveBlend)
                    p.Draw(spriteBatch);
            }

            // Pass 2: Alpha-blend particles
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            foreach (var p in _particles)
            {
                if (!p.UseAdditiveBlend)
                    p.Draw(spriteBatch);
            }

            // Restore standard state
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }

        public override void OnWorldUnload()
        {
            _particles.Clear();
        }
    }
}
