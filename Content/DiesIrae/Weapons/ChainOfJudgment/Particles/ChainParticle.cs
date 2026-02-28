using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;

namespace MagnumOpus.Content.DiesIrae.Weapons.ChainOfJudgment.Particles
{
    /// <summary>
    /// Abstract base particle for Chain of Judgment's self-contained particle system.
    /// </summary>
    public abstract class ChainParticle
    {
        public Vector2 Position;
        public Vector2 Velocity;
        public float Scale;
        public float Rotation;
        public Color DrawColor;
        public int Lifetime;
        public int MaxLifetime;
        public bool Active = true;
        public bool IsAdditive;

        public float LifeRatio => MaxLifetime > 0 ? (float)Lifetime / MaxLifetime : 0f;

        public virtual void Update()
        {
            Position += Velocity;
            Lifetime++;
            if (Lifetime >= MaxLifetime) Active = false;
        }

        public abstract void Draw(SpriteBatch sb);
    }
}
