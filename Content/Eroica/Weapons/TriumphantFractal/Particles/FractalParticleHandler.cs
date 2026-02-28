using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Eroica.Weapons.TriumphantFractal.Particles
{
    [Autoload(Side = ModSide.Client)]
    public class FractalParticleHandler : ModSystem
    {
        private static readonly List<FractalParticle> _particles = new();
        private const int MaxParticles = 600;

        public override void OnModLoad()
        {
            On_Main.DrawDust += DrawParticles;
        }

        public override void OnModUnload()
        {
            On_Main.DrawDust -= DrawParticles;
            _particles.Clear();
        }

        public static void SpawnParticle(FractalParticle particle)
        {
            if (Main.dedServ || _particles.Count >= MaxParticles) return;
            _particles.Add(particle);
        }

        private void DrawParticles(On_Main.orig_DrawDust orig, Main self)
        {
            orig(self);
            if (_particles.Count == 0) return;

            SpriteBatch sb = Main.spriteBatch;

            // Pass 1: Additive particles (sparks, bloom, geometry flashes)
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            for (int i = _particles.Count - 1; i >= 0; i--)
            {
                _particles[i].Update();
                _particles[i].StandardUpdate();

                if (_particles[i].ShouldRemove())
                {
                    _particles.RemoveAt(i);
                    continue;
                }

                if (_particles[i].UseAdditiveBlend)
                    _particles[i].CustomDraw(sb);
            }

            sb.End();

            // Pass 2: Alpha blend particles (lightning arcs, notes)
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            for (int i = 0; i < _particles.Count; i++)
            {
                if (!_particles[i].UseAdditiveBlend)
                    _particles[i].CustomDraw(sb);
            }

            sb.End();
        }
    }
}
