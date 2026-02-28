using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;

namespace MagnumOpus.Content.SwanLake.ResonantWeapons.ChromaticSwanSong.Particles
{
    public abstract class ChromaticParticle
    {
        public Vector2 Position;
        public Vector2 Velocity;
        public Color DrawColor;
        public float Rotation;
        public float Scale;
        public int Time;
        public int Lifetime;
        public bool Active = true;

        public float Progress => Lifetime > 0 ? (float)Time / Lifetime : 1f;
        protected virtual int SetLifetime() => 30;
        public virtual bool UseAdditiveBlend => false;

        public void Initialize(Vector2 position, Vector2 velocity, Color color, float scale = 1f)
        {
            Position = position; Velocity = velocity; DrawColor = color; Scale = scale;
            Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
            Time = 0; Lifetime = SetLifetime(); Active = true;
        }

        public virtual void Update()
        {
            Position += Velocity;
            Time++;
            if (Time >= Lifetime) Active = false;
        }

        public abstract void Draw(SpriteBatch spriteBatch);
    }
}
