using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.OdeToJoy.Weapons.RoseThornChainsaw.Particles
{
    /// <summary>
    /// Self-contained particle handler for Rose Thorn Chainsaw.
    /// 600-particle cap, two-pass rendering.
    /// </summary>
    [Autoload(Side = ModSide.Client)]
    public sealed class ThornParticleHandler : ModSystem
    {
        private static List<ThornParticle> particles;
        private static List<ThornParticle> dead;
        private const int Cap = 600;

        public override void Load()
        {
            particles = new List<ThornParticle>(Cap);
            dead = new List<ThornParticle>();
            On_Main.DrawDust += DrawAll;
        }

        public override void Unload()
        {
            particles = null;
            dead = null;
        }

        public override void OnWorldUnload()
        {
            particles?.Clear();
            dead?.Clear();
        }

        public static void Spawn(ThornParticle p)
        {
            if (Main.gamePaused || Main.dedServ || particles == null) return;
            if (particles.Count >= Cap) return;
            particles.Add(p);
        }

        private static void DrawAll(On_Main.orig_DrawDust orig, Main self)
        {
            orig(self);
            if (particles == null || particles.Count == 0) return;

            dead.Clear();
            foreach (var p in particles) { p.Update(); if (!p.Active) dead.Add(p); }
            foreach (var d in dead) particles.Remove(d);
            if (particles.Count == 0) return;

            SpriteBatch sb = Main.spriteBatch;

            // Additive pass
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            foreach (var p in particles) { if (p.IsAdditive && p.Active) p.Draw(sb); }
            sb.End();

            // Alpha blend pass
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            foreach (var p in particles) { if (!p.IsAdditive && p.Active) p.Draw(sb); }
            sb.End();
        }
    }
}
