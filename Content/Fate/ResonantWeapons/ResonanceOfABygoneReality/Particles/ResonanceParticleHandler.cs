using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Fate.ResonantWeapons.ResonanceOfABygoneReality
{
    /// <summary>
    /// ModSystem that manages and renders Resonance-specific particles.
    /// Pool of 512 value-type particles with additive two-pass drawing.
    /// Self-contained — no dependency on global particle systems.
    /// </summary>
    public class ResonanceParticleHandler : ModSystem
    {
        public const int MaxParticles = 512;
        private static ResonanceParticle[] _particles;
        private static int _nextSlot;

        public override void Load()
        {
            _particles = new ResonanceParticle[MaxParticles];
            _nextSlot = 0;
        }

        public override void Unload()
        {
            _particles = null;
        }

        /// <summary>
        /// Spawn a particle. Overwrites oldest slot when pool is full.
        /// </summary>
        public static void Spawn(ResonanceParticleType type, Vector2 pos, Vector2 vel,
            Color color, float scale, int life, float rotSpeed = 0f)
        {
            if (_particles == null || Main.dedServ) return;

            ref var p = ref _particles[_nextSlot % MaxParticles];
            p.Active = true;
            p.Type = type;
            p.Position = pos;
            p.Velocity = vel;
            p.Color = color;
            p.Scale = scale;
            p.Life = life;
            p.MaxLife = life;
            p.Opacity = 1f;
            p.Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
            p.RotationSpeed = rotSpeed;
            _nextSlot++;
        }

        public override void PostUpdateDusts()
        {
            if (_particles == null) return;

            for (int i = 0; i < MaxParticles; i++)
            {
                _particles[i].Update();
            }
        }

        /// <summary>
        /// Draw all active particles with two-pass rendering.
        /// Pass 1: Additive bloom halos.  Pass 2: Alpha-blended cores.
        /// Call from PreDraw or a DrawLayer.
        /// </summary>
        public static void DrawAll(SpriteBatch sb)
        {
            if (_particles == null || Main.dedServ) return;

            Texture2D glow = Terraria.GameContent.TextureAssets.Extra[98].Value;
            if (glow == null) return;

            // Pass 1: Additive bloom layer
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            Vector2 glowOrigin = glow.Size() / 2f;

            for (int i = 0; i < MaxParticles; i++)
            {
                ref var p = ref _particles[i];
                if (!p.Active) continue;

                Vector2 drawPos = p.Position - Main.screenPosition;
                Color bloomColor = p.Color * p.Opacity * 0.4f;
                float bloomScale = p.Scale * ResonanceParticleTypes.GetBloomScale(p.Type);
                sb.Draw(glow, drawPos, null, bloomColor, p.Rotation, glowOrigin, bloomScale, SpriteEffects.None, 0f);
            }

            // Pass 2: Normal alpha-blend core
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            for (int i = 0; i < MaxParticles; i++)
            {
                ref var p = ref _particles[i];
                if (!p.Active) continue;

                Vector2 drawPos = p.Position - Main.screenPosition;
                Color coreColor = p.Color * p.Opacity;
                float coreScale = p.Scale;
                sb.Draw(glow, drawPos, null, coreColor, p.Rotation, glowOrigin, coreScale, SpriteEffects.None, 0f);
            }
        }

        /// <summary>
        /// Number of currently active particles.
        /// </summary>
        public static int ActiveCount
        {
            get
            {
                if (_particles == null) return 0;
                int count = 0;
                for (int i = 0; i < MaxParticles; i++)
                    if (_particles[i].Active) count++;
                return count;
            }
        }
    }
}
