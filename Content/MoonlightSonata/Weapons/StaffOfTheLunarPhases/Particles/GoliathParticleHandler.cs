using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.MoonlightSonata.Weapons.StaffOfTheLunarPhases.Particles
{
    /// <summary>
    /// Self-contained particle management system for Staff of the Lunar Phases.
    /// Two-pass rendering: additive (blooms, glows, beam sparks) then alpha blend (shards, motes).
    /// </summary>
    [Autoload(Side = ModSide.Client)]
    public sealed class GoliathParticleHandler : ModSystem
    {
        private static List<GoliathParticle> _active;
        private static List<GoliathParticle> _toRemove;
        private const int ParticleLimit = 800;

        public override void Load()
        {
            _active = new List<GoliathParticle>();
            _toRemove = new List<GoliathParticle>();
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

        public static void Spawn(GoliathParticle particle)
        {
            if (Main.gamePaused || Main.dedServ || _active == null) return;
            if (_active.Count >= ParticleLimit) return;
            _active.Add(particle);
        }

        public static void Remove(GoliathParticle particle)
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
                if (p.ShouldRemove)
                    _toRemove.Add(p);
            }

            if (_toRemove.Count > 0)
            {
                foreach (var p in _toRemove)
                    _active.Remove(p);
                _toRemove.Clear();
            }
        }

        private static void DrawParticles(On_Main.orig_DrawDust orig, Main self)
        {
            orig(self);
            if (Main.dedServ || _active == null || _active.Count == 0) return;

            SpriteBatch sb = Main.spriteBatch;

            // Pass 1: Additive blend (blooms, glows, beam sparks, rift motes)
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive,
                SamplerState.PointClamp, DepthStencilState.None,
                Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            foreach (var p in _active)
            {
                if (p == null || !p.UseAdditiveBlend) continue;
                if (p.UseCustomDraw) p.CustomDraw(sb);
            }
            sb.End();

            // Pass 2: Alpha blend (normal particles)
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                SamplerState.PointClamp, DepthStencilState.None,
                Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            foreach (var p in _active)
            {
                if (p == null || p.UseAdditiveBlend) continue;
                if (p.UseCustomDraw) p.CustomDraw(sb);
            }
            sb.End();
        }
    }
}
