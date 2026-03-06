using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.VFX;

namespace MagnumOpus.Content.Fate.ResonantWeapons.SymphonysEnd
{
    /// <summary>
    /// Self-contained particle pool for Symphony's End.
    /// Updates in PostUpdateDusts, draws in PostDrawTiles (additive, behind entities).
    /// ZERO shared system references.
    /// </summary>
    public class SymphonyParticleHandler : ModSystem
    {
        private static SymphonyParticle[] _pool;
        private const int PoolSize = 512;

        public override void Load()
        {
            _pool = new SymphonyParticle[PoolSize];
        }

        public override void Unload()
        {
            _pool = null;
        }

        // ─── Update ───────────────────────────────────────────────
        public override void PostUpdateDusts()
        {
            if (_pool == null) return;
            for (int i = 0; i < PoolSize; i++)
            {
                if (_pool[i].Active)
                    _pool[i].Update();
            }
        }

        // ─── Draw (PostDrawTiles — additive glow behind entities) ──
        public override void PostDrawTiles()
        {
            if (Main.dedServ || _pool == null) return;

            SpriteBatch sb = Main.spriteBatch;
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone,
                null, Main.GameViewMatrix.TransformationMatrix);

            Texture2D pixel = MagnumTextureRegistry.GetPointBloom();
            Texture2D glow  = MagnumTextureRegistry.GetSoftGlow();
            if (pixel == null || glow == null) return;

            for (int i = 0; i < PoolSize; i++)
            {
                ref SymphonyParticle p = ref _pool[i];
                if (!p.Active) continue;

                Vector2 drawPos = p.Position - Main.screenPosition;
                Color drawColor = p.Color * p.Opacity;

                // Pick texture based on type
                Texture2D tex;
                switch (p.Type)
                {
                    case SymphonyParticleType.Glow:
                    case SymphonyParticleType.Ring:
                    case SymphonyParticleType.Crackle:
                    case SymphonyParticleType.Note:
                        tex = glow;
                        break;
                    default:
                        tex = pixel;
                        break;
                }

                float scale = p.Scale;

                // Ring expands over its lifetime
                if (p.Type == SymphonyParticleType.Ring)
                    scale *= 1f + p.Progress * 2.5f;

                Vector2 origin = new Vector2(tex.Width * 0.5f, tex.Height * 0.5f);
                sb.Draw(tex, drawPos, null, drawColor, p.Rotation, origin, scale, SpriteEffects.None, 0f);
            }

            sb.End();
        }

        // ─── Public API ───────────────────────────────────────────

        /// <summary>Spawn a single particle. Safe to call from any thread context.</summary>
        public static void Spawn(SymphonyParticle particle)
        {
            if (_pool == null) return;
            for (int i = 0; i < PoolSize; i++)
            {
                if (!_pool[i].Active)
                {
                    particle.Active = true;
                    _pool[i] = particle;
                    return;
                }
            }
        }

        /// <summary>Spawn a radial burst of identical particles.</summary>
        public static void SpawnBurst(Vector2 center, int count, float speed, float scale,
            Color color, SymphonyParticleType type, int lifetime)
        {
            for (int i = 0; i < count; i++)
            {
                float angle = MathHelper.TwoPi * i / count;
                Spawn(new SymphonyParticle
                {
                    Position = center,
                    Velocity = angle.ToRotationVector2() * speed * Main.rand.NextFloat(0.8f, 1.2f),
                    Color = color,
                    Scale = scale * Main.rand.NextFloat(0.8f, 1.2f),
                    Rotation = Main.rand.NextFloat(MathHelper.TwoPi),
                    RotationSpeed = Main.rand.NextFloat(-0.1f, 0.1f),
                    TimeLeft = lifetime,
                    MaxTime = lifetime,
                    Type = type,
                    Active = true,
                    Opacity = 1f,
                    Additive = true
                });
            }
        }
    }
}
