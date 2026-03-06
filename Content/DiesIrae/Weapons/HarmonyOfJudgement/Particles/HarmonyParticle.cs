using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.DiesIrae.Weapons.HarmonyOfJudgement.Particles
{
    /// <summary>
    /// Abstract base particle for Harmony of Judgement VFX.
    /// </summary>
    public abstract class SigilParticle
    {
        public Vector2 Position;
        public Vector2 Velocity;
        public float Scale;
        public float Rotation;
        public Color DrawColor;
        public int Time;
        public int Lifetime;
        public bool Active = true;

        public float Progress => Lifetime > 0 ? (float)Time / Lifetime : 1f;
        public virtual bool UseAdditiveBlend => true;

        public virtual void Update()
        {
            Position += Velocity;
            Time++;
            if (Time >= Lifetime) Active = false;
        }

        public abstract void Draw(SpriteBatch sb);
    }

    /// <summary>
    /// Particle handler for Harmony of Judgement — self-contained per-weapon particle manager.
    /// </summary>
    public class SigilParticleHandler : ModSystem
    {
        private const int MaxParticles = 300;
        private static readonly List<SigilParticle> _particles = new();

        public override void Load()
        {
            if (!Main.dedServ)
                On_Main.DrawDust += DrawParticles;
        }

        public override void Unload()
        {
            if (!Main.dedServ)
                On_Main.DrawDust -= DrawParticles;
            _particles.Clear();
        }

        public static void Spawn(SigilParticle p)
        {
            if (Main.dedServ || p == null) return;
            if (_particles.Count >= MaxParticles)
                _particles.RemoveAt(0);
            _particles.Add(p);
        }

        private static void DrawParticles(On_Main.orig_DrawDust orig, Main self)
        {
            orig(self);
            if (_particles.Count == 0) return;

            SpriteBatch sb = Main.spriteBatch;

            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);

            for (int i = _particles.Count - 1; i >= 0; i--)
            {
                var p = _particles[i];
                p.Update();
                if (!p.Active)
                {
                    _particles.RemoveAt(i);
                    continue;
                }
                if (p.UseAdditiveBlend) p.Draw(sb);
            }

            sb.End();

            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);

            for (int i = 0; i < _particles.Count; i++)
            {
                if (!_particles[i].UseAdditiveBlend && _particles[i].Active)
                    _particles[i].Draw(sb);
            }

            sb.End();
        }
    }
}
