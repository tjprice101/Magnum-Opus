using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.DiesIrae.Weapons.GrimoireOfCondemnation.Particles
{
    public class GrimoireParticleHandler : ModSystem
    {
        private static readonly List<GrimoireParticle> particles = new();
        private const int MaxParticles = 500;

        public static void Spawn(GrimoireParticle p)
        {
            if (particles.Count >= MaxParticles)
            {
                for (int i = 0; i < particles.Count; i++)
                    if (!particles[i].Active) { particles[i] = p; return; }
                particles[0] = p;
            }
            else particles.Add(p);
        }

        public override void PostUpdateDusts()
        {
            for (int i = particles.Count - 1; i >= 0; i--)
            {
                particles[i].Update();
                if (!particles[i].Active) particles.RemoveAt(i);
            }
        }

        public override void OnWorldUnload() => particles.Clear();
        public override void Load() { On_Main.DrawDust += DrawParticles; }
        public override void Unload() { On_Main.DrawDust -= DrawParticles; particles.Clear(); }

        private static void DrawParticles(On_Main.orig_DrawDust orig, Main self)
        {
            orig(self);
            if (particles.Count == 0) return;
            SpriteBatch sb = Main.spriteBatch;
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            foreach (var p in particles) if (p.Active && !p.IsAdditive) p.Draw(sb);
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            foreach (var p in particles) if (p.Active && p.IsAdditive) p.Draw(sb);
            sb.End();
        }
    }
}
