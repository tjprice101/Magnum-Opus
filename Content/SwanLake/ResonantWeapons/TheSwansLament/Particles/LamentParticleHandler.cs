using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.SwanLake.ResonantWeapons.TheSwansLament.Particles
{
    [Autoload(Side = ModSide.Client)]
    public class LamentParticleHandler : ModSystem
    {
        public static List<LamentParticle> Particles = new List<LamentParticle>();
        private const int MaxParticles = 400;

        public override void PostUpdateDusts()
        {
            for (int i = Particles.Count - 1; i >= 0; i--)
            {
                Particles[i].Update();
                if (!Particles[i].Active)
                    Particles.RemoveAt(i);
            }
        }

        public static void Spawn(LamentParticle particle, Vector2 position, Vector2 velocity, Color color, float scale, int time)
        {
            if (Particles.Count >= MaxParticles) return;
            particle.Spawn(position, velocity, color, scale, time);
            Particles.Add(particle);
        }

        public static void DrawAll(SpriteBatch spriteBatch)
        {
            if (Particles.Count == 0) return;

            // Pass 1: Additive blend
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, SamplerState.PointClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            foreach (var p in Particles)
                if (p.Active && p.UseAdditiveBlend)
                    p.Draw(spriteBatch);

            // Pass 2: Alpha blend
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            foreach (var p in Particles)
                if (p.Active && !p.UseAdditiveBlend)
                    p.Draw(spriteBatch);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }

        public override void OnWorldUnload() => Particles.Clear();
    }
}
