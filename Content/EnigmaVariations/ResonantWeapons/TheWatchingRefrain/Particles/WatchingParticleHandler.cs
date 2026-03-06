using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.EnigmaVariations.ResonantWeapons.TheWatchingRefrain.Particles
{
    /// <summary>
    /// ModSystem that manages TheWatchingRefrain-exclusive particles.
    /// Hooks into Main.DrawDust for dual-pass rendering (additive + alpha).
    /// </summary>
    [Autoload(Side = ModSide.Client)]
    public sealed class WatchingParticleHandler : ModSystem
    {
        private static readonly List<WatchingParticle> _particles = new(500);
        private const int MaxParticles = 500;

        public override void OnModLoad()
        {
            On_Main.DrawDust += DrawWatchingParticles;
        }

        public override void OnModUnload()
        {
            On_Main.DrawDust -= DrawWatchingParticles;
            _particles.Clear();
        }

        public static void Spawn(WatchingParticle particle)
        {
            if (_particles.Count >= MaxParticles) return;
            _particles.Add(particle);
        }

        public static void Clear() => _particles.Clear();

        private void DrawWatchingParticles(On_Main.orig_DrawDust orig, Main self)
        {
            orig(self);
            if (Main.dedServ || _particles.Count == 0) return;

            SpriteBatch sb = Main.spriteBatch;

            // Pass 1: Additive blend
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, SamplerState.LinearClamp,
                DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

            for (int i = _particles.Count - 1; i >= 0; i--)
            {
                var p = _particles[i];
                p.Update();
                p.Time++;

                if (p.ShouldRemove)
                {
                    _particles.RemoveAt(i);
                    continue;
                }

                if (p.UseAdditiveBlend)
                {
                    if (p.UseCustomDraw)
                        p.CustomDraw(sb);
                }
            }

            sb.End();

            // Pass 2: Alpha blend
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

            foreach (var p in _particles)
            {
                if (!p.UseAdditiveBlend && p.UseCustomDraw)
                    p.CustomDraw(sb);
            }

            sb.End();
        }
    }
}
