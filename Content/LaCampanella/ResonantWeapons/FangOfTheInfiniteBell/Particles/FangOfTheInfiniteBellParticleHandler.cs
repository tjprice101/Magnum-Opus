using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.LaCampanella.ResonantWeapons.FangOfTheInfiniteBell.Particles
{
    [Autoload(Side = ModSide.Client)]
    public class FangOfTheInfiniteBellParticleHandler : ModSystem
    {
        private static List<FangOfTheInfiniteBellParticle> _particles = new();
        private const int MaxParticles = 400;

        public static void SpawnParticle(FangOfTheInfiniteBellParticle p)
        {
            if (_particles.Count >= MaxParticles) return;
            _particles.Add(p);
        }

        public override void PostUpdateEverything()
        {
            for (int i = _particles.Count - 1; i >= 0; i--)
            {
                _particles[i].Update();
                if (_particles[i].ShouldRemove) _particles.RemoveAt(i);
            }
        }

        public void DrawAllParticles(SpriteBatch sb)
        {
            if (_particles.Count == 0) return;
            var add = _particles.Where(p => p.UseAdditiveBlend).ToList();
            var alpha = _particles.Where(p => !p.UseAdditiveBlend).ToList();

            try
            {
                if (add.Count > 0)
                {
                    try { sb.End(); } catch { }
                    sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                        DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
                    foreach (var p in add) p.Draw(sb);
                }
                if (alpha.Count > 0)
                {
                    try { sb.End(); } catch { }
                    sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                        DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
                    foreach (var p in alpha) p.Draw(sb);
                }
            }
            catch { }
            finally
            {
                try { sb.End(); } catch { }
                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp,
                    DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            }
        }

        public override void OnWorldUnload() { _particles.Clear(); }
    }
}
