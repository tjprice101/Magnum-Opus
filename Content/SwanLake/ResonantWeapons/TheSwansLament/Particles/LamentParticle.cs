using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;

namespace MagnumOpus.Content.SwanLake.ResonantWeapons.TheSwansLament.Particles
{
    public abstract class LamentParticle
    {
        public Vector2 Position;
        public Vector2 Velocity;
        public float Rotation;
        public float RotationSpeed;
        public float Scale;
        public float Opacity;
        public Color DrawColor;
        public int TimeLeft;
        public int MaxTime;
        public bool Active;

        public float LifeRatio => MaxTime > 0 ? (float)TimeLeft / MaxTime : 0f;

        public virtual void Spawn(Vector2 position, Vector2 velocity, Color color, float scale, int time)
        {
            Position = position;
            Velocity = velocity;
            DrawColor = color;
            Scale = scale;
            TimeLeft = time;
            MaxTime = time;
            Opacity = 1f;
            Active = true;
            Rotation = Main.rand.NextFloat() * MathHelper.TwoPi;
            RotationSpeed = Main.rand.NextFloat(-0.08f, 0.08f);
        }

        public virtual void Update()
        {
            if (!Active) return;
            TimeLeft--;
            if (TimeLeft <= 0)
            {
                Active = false;
                return;
            }
            Position += Velocity;
            Rotation += RotationSpeed;
        }

        public abstract void Draw(SpriteBatch spriteBatch);

        /// <summary>Whether this particle draws in additive blend mode (pass 1) or alpha blend (pass 2).</summary>
        public virtual bool UseAdditiveBlend => false;
    }
}
