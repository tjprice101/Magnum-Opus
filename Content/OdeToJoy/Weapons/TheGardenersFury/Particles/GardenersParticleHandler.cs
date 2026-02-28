using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.OdeToJoy.Weapons.TheGardenersFury.Particles
{
    /// <summary>
    /// Self-contained particle handler for The Gardener's Fury.
    /// Manages its own particle list, update loop, and draw passes.
    /// 500-particle cap, two-pass rendering (additive + alpha blend).
    /// Hooked into On_Main.DrawDust for consistent draw order.
    /// </summary>
    [Autoload(Side = ModSide.Client)]
    public sealed class GardenersParticleHandler : ModSystem
    {
        private static List<GardenersParticle> activeParticles;
        private static List<GardenersParticle> particlesToRemove;
        private const int ParticleLimit = 500;

        public override void Load()
        {
            activeParticles = new List<GardenersParticle>(ParticleLimit);
            particlesToRemove = new List<GardenersParticle>();
            On_Main.DrawDust += DrawAllParticles;
        }

        public override void Unload()
        {
            On_Main.DrawDust -= DrawAllParticles;
            activeParticles = null;
            particlesToRemove = null;
        }

        public override void OnWorldUnload()
        {
            activeParticles?.Clear();
            particlesToRemove?.Clear();
        }

        /// <summary>
        /// Spawns a particle into the handler. Respects the 500-particle cap.
        /// </summary>
        public static void SpawnParticle(GardenersParticle particle)
        {
            if (Main.gamePaused || Main.dedServ || activeParticles == null)
                return;
            if (activeParticles.Count >= ParticleLimit)
                return;
            activeParticles.Add(particle);
        }

        private static void DrawAllParticles(On_Main.orig_DrawDust orig, Main self)
        {
            orig(self);

            if (activeParticles == null || activeParticles.Count == 0)
                return;

            // Update all particles
            particlesToRemove.Clear();
            foreach (var p in activeParticles)
            {
                p.Update();
                if (!p.Active)
                    particlesToRemove.Add(p);
            }
            foreach (var dead in particlesToRemove)
                activeParticles.Remove(dead);

            if (activeParticles.Count == 0)
                return;

            SpriteBatch sb = Main.spriteBatch;

            // Pass 1: Additive blend particles (glows, sparks, bloom)
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            foreach (var p in activeParticles)
            {
                if (p.IsAdditive && p.Active)
                    p.Draw(sb);
            }
            sb.End();

            // Pass 2: Alpha blend particles (petals, mist)
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            foreach (var p in activeParticles)
            {
                if (!p.IsAdditive && p.Active)
                    p.Draw(sb);
            }
            sb.End();
        }
    }
}
