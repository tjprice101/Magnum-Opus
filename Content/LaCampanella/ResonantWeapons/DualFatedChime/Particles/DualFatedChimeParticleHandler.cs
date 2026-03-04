using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.LaCampanella.ResonantWeapons.DualFatedChime.Particles
{
    /// <summary>
    /// ModSystem that manages all Dual Fated Chime particles.
    /// Ticks, draws, and culls particles independently from any shared system.
    /// </summary>
    [Autoload(Side = ModSide.Client)]
    public class DualFatedChimeParticleHandler : ModSystem
    {
        private static readonly List<DualFatedChimeParticle> _particles = new List<DualFatedChimeParticle>();
        private const int MaxParticles = 400;

        /// <summary>Spawn a new particle.</summary>
        public static void SpawnParticle(DualFatedChimeParticle particle)
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
        /// Draw all active particles. Call from a draw hook.
        /// Two passes: additive then alpha-blend.
        /// Safe SpriteBatch state management with try/finally.
        /// </summary>
        public static void DrawAllParticles(SpriteBatch spriteBatch)
        {
            if (_particles.Count == 0)
                return;

            try
            {
                // Pass 1: Additive particles (glows, sparks, fire)
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                    DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

                foreach (var p in _particles)
                {
                    if (p.UseAdditiveBlend)
                        p.Draw(spriteBatch);
                }

                // Pass 2: Alpha-blend particles (smoke, embers)
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                    DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

                foreach (var p in _particles)
                {
                    if (!p.UseAdditiveBlend)
                        p.Draw(spriteBatch);
                }
            }
            catch { }
            finally
            {
                // Always restore standard state
                try { spriteBatch.End(); } catch { }
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp,
                    DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            }
        }

        public override void OnWorldUnload()
        {
            _particles.Clear();
        }
    }
}
