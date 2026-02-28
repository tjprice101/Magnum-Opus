using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Fate.ResonantWeapons.CodaOfAnnihilation.Particles
{
    /// <summary>
    /// ModSystem that manages all Coda of Annihilation particles.
    /// Higher pool (800) for this ultimate weapon's intense VFX.
    /// Two-pass rendering: additive then alpha-blend.
    /// </summary>
    [Autoload(Side = ModSide.Client)]
    public class CodaParticleHandler : ModSystem
    {
        private static readonly List<CodaParticle> _particles = new List<CodaParticle>();
        private const int MaxParticles = 800;

        /// <summary>Spawn a new particle if pool not full.</summary>
        public static void SpawnParticle(CodaParticle particle)
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
        /// Draw all active particles. 
        /// Two passes: additive (glows, sparks, flares) then alpha-blend (smoke, debris).
        /// </summary>
        public static void DrawAllParticles(SpriteBatch spriteBatch)
        {
            if (_particles.Count == 0)
                return;

            // Pass 1: Additive particles
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
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
