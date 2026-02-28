using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;

namespace MagnumOpus.Content.SwanLake.ResonantWeapons.IridescentWingspan.Particles
{
    public abstract class WingspanParticle
    {
        public Vector2 Position, Velocity;
        public Color DrawColor;
        public float Rotation, Scale;
        public int Time, Lifetime;
        public bool Active = true;
        public float Progress => Lifetime > 0 ? (float)Time / Lifetime : 1f;
        protected virtual int SetLifetime() => 30;
        public virtual bool UseAdditiveBlend => false;

        public void Initialize(Vector2 pos, Vector2 vel, Color col, float scale = 1f)
        {
            Position = pos; Velocity = vel; DrawColor = col; Scale = scale;
            Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
            Time = 0; Lifetime = SetLifetime(); Active = true;
        }

        public virtual void Update() { Position += Velocity; Time++; if (Time >= Lifetime) Active = false; }
        public abstract void Draw(SpriteBatch sb);
    }
}
