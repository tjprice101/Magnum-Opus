using MagnumOpus.Common;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.Shaders;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.VFX.Core;
using MagnumOpus.Content.ClairDeLune;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.ClairDeLune.Weapons.ClockworkGrimoire.Projectiles
{
    /// <summary>
    /// Second Bolt — rapid-fire small piercing bolt for Second Mode (20/s).
    /// 6px, Pearl Frost core → Soft Moonblue trail, pierces 1 enemy.
    /// 2 render passes: (1) ClairDeLuneMoonlit MoonlitFlow shimmer body,
    /// (2) Elongated bloom trail + core stacking.
    /// Kept lightweight due to 20/s spawn rate.
    /// </summary>
    public class SecondBoltProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Textures/InvisibleProjectile";

        private VertexStrip _vertexStrip;
        private const int TrailLen = 6;
        private Vector2[] _oldPos = new Vector2[TrailLen];

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 16;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 6;
            Projectile.height = 6;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 2;
            Projectile.timeLeft = 60;
            Projectile.tileCollide = true;
            Projectile.extraUpdates = 2;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();

            for (int i = TrailLen - 1; i > 0; i--)
                _oldPos[i] = _oldPos[i - 1];
            _oldPos[0] = Projectile.Center;

            Lighting.AddLight(Projectile.Center, ClairDeLunePalette.PearlFrost.ToVector3() * 0.15f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            var flash = new GenericGlowParticle(target.Center, Vector2.Zero,
                ClairDeLunePalette.PearlFrost with { A = 0 } * 0.3f, 0.06f, 4, true);
            MagnumParticleHandler.SpawnParticle(flash);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (Main.dedServ) return false;

            SpriteBatch sb = Main.spriteBatch;
            try
            {
                IncisorOrbRenderer.DrawOrbVisuals(sb, Projectile, IncisorOrbRenderer.ClairDeLune, ref _vertexStrip);

                // --- Rapid-fire frost streak accent ---
                sb.End();
                sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, SamplerState.LinearClamp,
                    DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

                var glowTex = MagnumTextureRegistry.GetSoftGlow();
                Vector2 origin = glowTex.Size() / 2f;
                Vector2 pos = Projectile.Center - Main.screenPosition;
                float rot = Projectile.velocity.ToRotation();
                Color frost = (ClairDeLunePalette.MoonlitFrost with { A = 0 }) * 0.55f;
                sb.Draw(glowTex, pos, null, frost, rot, origin, new Vector2(0.06f, 0.02f), SpriteEffects.None, 0f);

                sb.End();
            }
            catch { }
            finally
            {
                try { sb.End(); } catch { }
                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                    DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            }

            return false;
        }
    }
}
