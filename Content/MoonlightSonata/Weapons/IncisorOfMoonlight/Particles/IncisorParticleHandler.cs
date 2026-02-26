using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.MoonlightSonata.Weapons.IncisorOfMoonlight.Particles
{
    /// <summary>
    /// Self-contained particle system for the Incisor of Moonlight.
    /// Manages spawn, update, and draw of IncisorParticle instances.
    /// Independent of all shared mod particle systems.
    /// </summary>
    [Autoload(Side = ModSide.Client)]
    public sealed class IncisorParticleHandler : ModSystem
    {
        private static List<IncisorParticle> activeParticles;
        private static List<IncisorParticle> particlesToKill;
        private const int ParticleLimit = 500;

        public override void Load()
        {
            activeParticles = new List<IncisorParticle>();
            particlesToKill = new List<IncisorParticle>();
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

        public static void SpawnParticle(IncisorParticle particle)
        {
            if (Main.gamePaused || Main.dedServ || activeParticles == null)
                return;
            if (activeParticles.Count >= ParticleLimit)
                return;
            activeParticles.Add(particle);
        }

        public static void RemoveParticle(IncisorParticle particle)
        {
            if (!Main.dedServ)
                particlesToKill?.Add(particle);
        }

        public override void PostUpdateEverything()
        {
            if (Main.dedServ || activeParticles == null)
                return;

            foreach (IncisorParticle p in activeParticles)
            {
                if (p == null) continue;
                p.Position += p.Velocity;
                p.Time++;
                p.Update();
            }

            activeParticles.RemoveAll(p => p.ShouldRemove || particlesToKill.Contains(p));
            particlesToKill.Clear();
        }

        private static void DrawParticles(On_Main.orig_DrawDust orig, Main self)
        {
            orig(self);

            if (Main.dedServ || activeParticles == null || activeParticles.Count == 0)
                return;

            // Additive-blend particles (glow, sparks)
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive,
                SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone,
                null, Main.GameViewMatrix.TransformationMatrix);
            foreach (IncisorParticle p in activeParticles)
            {
                if (p != null && p.UseAdditiveBlend && p.UseCustomDraw)
                    p.CustomDraw(Main.spriteBatch);
            }
            Main.spriteBatch.End();

            // Alpha-blend particles (smoke, mist)
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone,
                null, Main.GameViewMatrix.TransformationMatrix);
            foreach (IncisorParticle p in activeParticles)
            {
                if (p != null && !p.UseAdditiveBlend && p.UseCustomDraw)
                    p.CustomDraw(Main.spriteBatch);
            }
            Main.spriteBatch.End();
        }
    }
}
