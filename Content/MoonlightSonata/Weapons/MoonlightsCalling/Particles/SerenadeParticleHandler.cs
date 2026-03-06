using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.MoonlightSonata.Weapons.MoonlightsCalling.Particles
{
    /// <summary>
    /// Self-contained particle management system for Moonlight's Calling.
    /// Two-pass rendering: additive (blooms, glows) then alpha blend (normal particles).
    /// </summary>
    [Autoload(Side = ModSide.Client)]
    public sealed class SerenadeParticleHandler : ModSystem
    {
        private static List<SerenadeParticle> _active;
        private static List<SerenadeParticle> _toRemove;
        private const int ParticleLimit = 600;

        public override void Load()
        {
            _active = new List<SerenadeParticle>();
            _toRemove = new List<SerenadeParticle>();
            On_Main.DrawDust += DrawParticles;
        }

        public override void Unload()
        {
            _active = null;
            _toRemove = null;
        }

        public override void OnWorldUnload()
        {
            _active?.Clear();
            _toRemove?.Clear();
        }

        public static void Spawn(SerenadeParticle particle)
        {
            if (Main.gamePaused || Main.dedServ || _active == null) return;
            if (_active.Count >= ParticleLimit) return;
            _active.Add(particle);
        }

        public static void Remove(SerenadeParticle particle)
        {
            if (!Main.dedServ) _toRemove?.Add(particle);
        }

        public override void PostUpdateEverything()
        {
            if (Main.dedServ || _active == null) return;

            foreach (var p in _active)
            {
                if (p == null) continue;
                p.Position += p.Velocity;
                p.Time++;
                p.Update();
            }

            _active.RemoveAll(p => p.ShouldRemove || _toRemove.Contains(p));
            _toRemove.Clear();
        }

        private static void DrawParticles(On_Main.orig_DrawDust orig, Main self)
        {
            orig(self);
            if (Main.dedServ || _active == null || _active.Count == 0) return;

            // Pass 1: Additive (bloom, glows, prism flares)
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                SamplerState.PointClamp, DepthStencilState.None,
                RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            foreach (var p in _active)
            {
                if (p != null && p.UseAdditiveBlend && p.UseCustomDraw)
                    p.CustomDraw(Main.spriteBatch);
            }

            Main.spriteBatch.End();

            // Pass 2: Alpha blend (opaque particles)
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                SamplerState.PointClamp, DepthStencilState.None,
                RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            foreach (var p in _active)
            {
                if (p != null && !p.UseAdditiveBlend && p.UseCustomDraw)
                    p.CustomDraw(Main.spriteBatch);
            }

            Main.spriteBatch.End();
        }
    }
}
