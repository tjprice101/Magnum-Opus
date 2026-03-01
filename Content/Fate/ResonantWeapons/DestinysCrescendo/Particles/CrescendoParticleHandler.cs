using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.VFX;

namespace MagnumOpus.Content.Fate.ResonantWeapons.DestinysCrescendo
{
    /// <summary>
    /// Self-contained particle pool for Destiny's Crescendo.
    /// Updates in PostUpdateDusts, draws in PostDrawTiles (additive two-pass: bloom underlayer + detail).
    /// ZERO shared system references.
    /// </summary>
    public class CrescendoParticleHandler : ModSystem
    {
        private static CrescendoParticle[] _pool;
        private const int PoolSize = 512;

        public override void Load()
        {
            _pool = new CrescendoParticle[PoolSize];
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

        // ─── Draw (PostDrawTiles — additive two-pass) ──────────────
        public override void PostDrawTiles()
        {
            if (Main.dedServ || _pool == null) return;

            SpriteBatch sb = Main.spriteBatch;
            Texture2D glow = MagnumTextureRegistry.GetSoftGlow();
            Texture2D pixel = MagnumTextureRegistry.GetPointBloom();
            if (glow == null || pixel == null) return;

            // Pass 1: Bloom underlayer (larger, softer, lower opacity)
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive,
                SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone,
                null, Main.GameViewMatrix.TransformationMatrix);

            for (int i = 0; i < PoolSize; i++)
            {
                ref CrescendoParticle p = ref _pool[i];
                if (!p.Active) continue;

                Vector2 drawPos = p.Position - Main.screenPosition;
                Color bloomColor = p.Color * p.Opacity * 0.35f;
                float bloomScale = p.Scale * 2.2f;

                if (p.Type == CrescendoParticleType.GlyphCircle)
                    bloomScale *= 1f + p.Progress * 2f;

                Vector2 origin = new Vector2(glow.Width * 0.5f, glow.Height * 0.5f);
                sb.Draw(glow, drawPos, null, bloomColor, p.Rotation, origin, bloomScale, SpriteEffects.None, 0f);
            }

            sb.End();

            // Pass 2: Detail layer (actual particle shape)
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive,
                SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone,
                null, Main.GameViewMatrix.TransformationMatrix);

            for (int i = 0; i < PoolSize; i++)
            {
                ref CrescendoParticle p = ref _pool[i];
                if (!p.Active) continue;

                Vector2 drawPos = p.Position - Main.screenPosition;
                Color drawColor = p.Color * p.Opacity;
                float scale = p.Scale;

                Texture2D tex;
                switch (p.Type)
                {
                    case CrescendoParticleType.OrbGlow:
                    case CrescendoParticleType.BeamFlare:
                    case CrescendoParticleType.AuraWisp:
                        tex = glow;
                        break;
                    case CrescendoParticleType.GlyphCircle:
                        tex = glow;
                        scale *= 1f + p.Progress * 2.5f;
                        break;
                    case CrescendoParticleType.DivineSpark:
                    case CrescendoParticleType.CosmicNote:
                        tex = glow;
                        break;
                    default:
                        tex = pixel;
                        break;
                }

                Vector2 origin = new Vector2(tex.Width * 0.5f, tex.Height * 0.5f);
                sb.Draw(tex, drawPos, null, drawColor, p.Rotation, origin, scale, SpriteEffects.None, 0f);
            }

            sb.End();
        }

        // ─── Public API ───────────────────────────────────────────

        /// <summary>Spawn a single particle. Safe to call from any thread context.</summary>
        public static void Spawn(CrescendoParticle particle)
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
            Color color, CrescendoParticleType type, int lifetime)
        {
            for (int i = 0; i < count; i++)
            {
                float angle = MathHelper.TwoPi * i / count;
                Spawn(new CrescendoParticle
                {
                    Position = center,
                    Velocity = angle.ToRotationVector2() * speed * Main.rand.NextFloat(0.8f, 1.2f),
                    Color = color,
                    Scale = scale * Main.rand.NextFloat(0.8f, 1.2f),
                    Rotation = Main.rand.NextFloat(MathHelper.TwoPi),
                    RotationSpeed = Main.rand.NextFloat(-0.08f, 0.08f),
                    TimeLeft = lifetime,
                    MaxTime = lifetime,
                    Type = type,
                    Opacity = 1f
                });
            }
        }
    }
}
