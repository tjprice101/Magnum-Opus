using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Eroica.Weapons.CelestialValor.Particles
{
    /// <summary>
    /// Self-contained particle system for Celestial Valor.
    /// Mirrors ExoParticleHandler — manages, updates, and draws ValorParticles
    /// via an On_Main.DrawDust hook with proper blend state management.
    /// </summary>
    [Autoload(Side = ModSide.Client)]
    public sealed class ValorParticleHandler : ModSystem
    {
        private static List<ValorParticle> _active;
        private static List<ValorParticle> _toKill;
        private const int ParticleLimit = 600;

        public override void Load()
        {
            _active = new List<ValorParticle>();
            _toKill = new List<ValorParticle>();
            On_Main.DrawDust += DrawAllParticles;
        }

        public override void Unload()
        {
            _active = null;
            _toKill = null;
        }

        public override void OnWorldUnload()
        {
            _active?.Clear();
            _toKill?.Clear();
        }

        public static void SpawnParticle(ValorParticle particle)
        {
            if (Main.gamePaused || Main.dedServ || _active == null) return;
            if (_active.Count >= ParticleLimit) return;
            _active.Add(particle);
        }

        public static void RemoveParticle(ValorParticle particle)
        {
            if (!Main.dedServ) _toKill?.Add(particle);
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

            _active.RemoveAll(p => p.ShouldRemove || _toKill.Contains(p));
            _toKill.Clear();
        }

        private static void DrawAllParticles(On_Main.orig_DrawDust orig, Main self)
        {
            orig(self);

            if (Main.dedServ || _active == null || _active.Count == 0) return;

            // Additive blend pass
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive,
                SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone,
                null, Main.GameViewMatrix.TransformationMatrix);

            foreach (var p in _active)
            {
                if (p is { UseAdditiveBlend: true, UseCustomDraw: true })
                    p.CustomDraw(Main.spriteBatch);
            }

            Main.spriteBatch.End();

            // Alpha blend pass
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone,
                null, Main.GameViewMatrix.TransformationMatrix);

            foreach (var p in _active)
            {
                if (p is { UseAdditiveBlend: false, UseCustomDraw: true })
                    p.CustomDraw(Main.spriteBatch);
            }

            Main.spriteBatch.End();
        }
    }
}
