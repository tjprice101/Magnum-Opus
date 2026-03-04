using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.LaCampanella.ResonantWeapons.PiercingBellsResonance.Particles
{
    [Autoload(Side = ModSide.Client)]
    public class PiercingBellsParticleHandler : ModSystem
    {
        public static List<PiercingBellsParticle> Particles = new List<PiercingBellsParticle>();
        private const int MaxParticles = 400;

        public override void OnWorldUnload() => Particles.Clear();

        public static void SpawnParticle(PiercingBellsParticle p)
        {
            if (Particles.Count < MaxParticles)
                Particles.Add(p);
        }

        public override void PostUpdateEverything()
        {
            for (int i = Particles.Count - 1; i >= 0; i--)
            {
                Particles[i].Update();
                if (Particles[i].ShouldRemove)
                    Particles.RemoveAt(i);
            }
        }

        public static void DrawAllParticles(SpriteBatch sb)
        {
            if (Particles.Count == 0) return;

            try
            {
                try { sb.End(); } catch { }
                sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.PointClamp,
                    DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
                foreach (var p in Particles)
                    if (p.UseAdditiveBlend) p.Draw(sb);

                try { sb.End(); } catch { }
                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp,
                    DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
                foreach (var p in Particles)
                    if (!p.UseAdditiveBlend) p.Draw(sb);
            }
            catch { }
            finally
            {
                try { sb.End(); } catch { }
                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp,
                    DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            }
        }
    }
}
