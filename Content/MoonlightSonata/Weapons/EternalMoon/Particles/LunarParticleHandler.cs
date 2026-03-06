using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.MoonlightSonata.Weapons.EternalMoon.Particles
{
    /// <summary>
    /// Self-contained particle management system for Eternal Moon.
    /// Handles spawning, updating, and two-pass rendering (additive + alpha blend).
    /// </summary>
    [Autoload(Side = ModSide.Client)]
    public sealed class LunarParticleHandler : ModSystem
    {
        private static List<LunarParticle> _activeParticles;
        private static List<LunarParticle> _particlesToKill;
        private const int ParticleLimit = 600;

        public override void Load()
        {
            _activeParticles = new List<LunarParticle>();
            _particlesToKill = new List<LunarParticle>();
            On_Main.DrawDust += DrawParticles;
        }

        public override void Unload()
        {
            _activeParticles = null;
            _particlesToKill = null;
        }

        public override void OnWorldUnload()
        {
            _activeParticles?.Clear();
            _particlesToKill?.Clear();
        }

        public static void SpawnParticle(LunarParticle particle)
        {
            if (Main.gamePaused || Main.dedServ || _activeParticles == null) return;
            if (_activeParticles.Count >= ParticleLimit) return;
            _activeParticles.Add(particle);
        }

        public static void RemoveParticle(LunarParticle particle)
        {
            if (!Main.dedServ) _particlesToKill?.Add(particle);
        }

        public override void PostUpdateEverything()
        {
            if (Main.dedServ || _activeParticles == null) return;

            foreach (LunarParticle particle in _activeParticles)
            {
                if (particle == null) continue;
                particle.Position += particle.Velocity;
                particle.Time++;
                particle.Update();
            }

            _activeParticles.RemoveAll(p => p.ShouldRemove || _particlesToKill.Contains(p));
            _particlesToKill.Clear();
        }

        private static void DrawParticles(On_Main.orig_DrawDust orig, Main self)
        {
            orig(self);
            if (Main.dedServ || _activeParticles == null || _activeParticles.Count == 0) return;

            // Pass 1: Additive blend (bloom, glows, flares)
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                SamplerState.PointClamp, DepthStencilState.None,
                RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            foreach (LunarParticle particle in _activeParticles)
            {
                if (particle != null && particle.UseAdditiveBlend && particle.UseCustomDraw)
                    particle.CustomDraw(Main.spriteBatch);
            }

            Main.spriteBatch.End();

            // Pass 2: Alpha blend (normal particles)
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                SamplerState.PointClamp, DepthStencilState.None,
                RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            foreach (LunarParticle particle in _activeParticles)
            {
                if (particle != null && !particle.UseAdditiveBlend && particle.UseCustomDraw)
                    particle.CustomDraw(Main.spriteBatch);
            }

            Main.spriteBatch.End();
        }
    }
}
