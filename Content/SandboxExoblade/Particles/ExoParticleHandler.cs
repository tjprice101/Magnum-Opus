using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.SandboxExoblade.Particles
{
    [Autoload(Side = ModSide.Client)]
    public sealed class ExoParticleHandler : ModSystem
    {
        private static List<ExoParticle> activeParticles;
        private static List<ExoParticle> particlesToKill;
        private const int ParticleLimit = 500;

        public override void Load()
        {
            activeParticles = new List<ExoParticle>();
            particlesToKill = new List<ExoParticle>();
            On_Main.DrawDust += DrawParticles;
        }

        public override void Unload()
        {
            activeParticles = null;
            particlesToKill = null;
        }

        public override void OnWorldUnload()
        {
            activeParticles?.Clear();
            particlesToKill?.Clear();
        }

        public static void SpawnParticle(ExoParticle particle)
        {
            if (Main.gamePaused || Main.dedServ || activeParticles == null)
                return;
            if (activeParticles.Count >= ParticleLimit)
                return;
            activeParticles.Add(particle);
        }

        public static void RemoveParticle(ExoParticle particle)
        {
            if (!Main.dedServ)
                particlesToKill?.Add(particle);
        }

        public override void PostUpdateEverything()
        {
            if (Main.dedServ || activeParticles == null)
                return;

            foreach (ExoParticle particle in activeParticles)
            {
                if (particle == null) continue;
                particle.Position += particle.Velocity;
                particle.Time++;
                particle.Update();
            }

            activeParticles.RemoveAll(p => p.ShouldRemove || particlesToKill.Contains(p));
            particlesToKill.Clear();
        }

        private static void DrawParticles(On_Main.orig_DrawDust orig, Main self)
        {
            orig(self);

            if (Main.dedServ || activeParticles == null || activeParticles.Count == 0)
                return;

            // Draw additive particles
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            foreach (ExoParticle particle in activeParticles)
            {
                if (particle != null && particle.UseAdditiveBlend && particle.UseCustomDraw)
                    particle.CustomDraw(Main.spriteBatch);
            }
            Main.spriteBatch.End();

            // Draw normal particles
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            foreach (ExoParticle particle in activeParticles)
            {
                if (particle != null && !particle.UseAdditiveBlend && particle.UseCustomDraw)
                    particle.CustomDraw(Main.spriteBatch);
            }
            Main.spriteBatch.End();
        }
    }
}
