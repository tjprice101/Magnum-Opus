using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.LaCampanella.ResonantWeapons.IgnitionOfTheBell.Particles
{
    /// <summary>
    /// ModSystem handler for IgnitionOfTheBell particles.
    /// Tick, cull, and draw with two passes (additive + alpha blend).
    /// Called from projectile PreDraw rather than via DrawLayer.
    /// </summary>
    [Autoload(Side = ModSide.Client)]
    public class IgnitionOfTheBellParticleHandler : ModSystem
    {
        private static List<IgnitionOfTheBellParticle> _particles = new();
        private const int MaxParticles = 400;

        public static void SpawnParticle(IgnitionOfTheBellParticle p)
        {
            if (_particles.Count >= MaxParticles) return;
            _particles.Add(p);
        }

        public override void PostUpdateEverything()
        {
            for (int i = _particles.Count - 1; i >= 0; i--)
            {
                _particles[i].Update();
                if (_particles[i].ShouldRemove)
                    _particles.RemoveAt(i);
            }
        }

        public void DrawAllParticles(SpriteBatch sb)
        {
            if (_particles.Count == 0) return;

            try
            {
                var additive = _particles.Where(p => p.UseAdditiveBlend).ToList();
                var alpha = _particles.Where(p => !p.UseAdditiveBlend).ToList();

                // Additive pass
                if (additive.Count > 0)
                {
                    sb.End();
                    sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                        DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

                    foreach (var p in additive)
                        p.Draw(sb);
                }

                // Alpha pass
                if (alpha.Count > 0)
                {
                    sb.End();
                    sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                        DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

                    foreach (var p in alpha)
                        p.Draw(sb);
                }
            }
            catch { }
            finally
            {
                // Always restore standard state
                try { sb.End(); } catch { }
                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp,
                    DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            }
        }

        public override void OnWorldUnload()
        {
            _particles.Clear();
        }
    }
}
