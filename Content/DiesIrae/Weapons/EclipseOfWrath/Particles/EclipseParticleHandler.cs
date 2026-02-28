using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.DiesIrae.Weapons.EclipseOfWrath.Particles
{
    /// <summary>
    /// Self-contained particle handler for Eclipse of Wrath.
    /// 400-particle cap, two-pass rendering.
    /// </summary>
    [Autoload(Side = ModSide.Client)]
    public sealed class EclipseParticleHandler : ModSystem
    {
        private static List<EclipseParticle> activeParticles;
        private static List<EclipseParticle> deadParticles;
        private const int ParticleLimit = 400;

        public override void Load()
        {
            activeParticles = new List<EclipseParticle>(ParticleLimit);
            deadParticles = new List<EclipseParticle>();
            On_Main.DrawDust += DrawAllParticles;
        }

        public override void Unload()
        {
            activeParticles = null;
            deadParticles = null;
        }

        public override void OnWorldUnload()
        {
            activeParticles?.Clear();
            deadParticles?.Clear();
        }

        public static void SpawnParticle(EclipseParticle particle)
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
            if (activeParticles == null || activeParticles.Count == 0) return;

            deadParticles.Clear();
            foreach (var p in activeParticles)
            {
                p.Update();
                if (!p.Active) deadParticles.Add(p);
            }
            foreach (var d in deadParticles)
                activeParticles.Remove(d);
            if (activeParticles.Count == 0) return;

            SpriteBatch sb = Main.spriteBatch;

            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            foreach (var p in activeParticles)
                if (p.IsAdditive && p.Active) p.Draw(sb);
            sb.End();

            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            foreach (var p in activeParticles)
                if (!p.IsAdditive && p.Active) p.Draw(sb);
            sb.End();
        }
    }
}
