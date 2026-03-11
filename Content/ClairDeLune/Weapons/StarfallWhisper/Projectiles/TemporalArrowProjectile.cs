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
using Terraria.Audio;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.ClairDeLune.Weapons.StarfallWhisper.Projectiles
{
    /// <summary>
    /// Temporal Arrow — Crystal arrow with GPU-driven SparkleTrail (SparkleProjectileFoundation),
    /// multi-layer bloom trail, 5-layer crystal head, and StarFlare/4PointedStar accents.
    /// Renders 5 layers: (1) SparkleTrail shader, (2) Bloom trail, (3) Bloom halo,
    /// (4) Crystal body + CrystalShimmer shader, (5) Sparkle accents.
    /// </summary>
    public class TemporalArrowProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Textures/InvisibleProjectile";

        private VertexStrip _vertexStrip;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 16;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 120;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.arrow = true;
            Projectile.extraUpdates = 1;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();

            // Temporal shimmer — clock-tick flash every 8 frames
            if (Projectile.timeLeft % 8 == 0)
            {
                var shimmer = new SparkleParticle(
                    Projectile.Center, -Projectile.velocity * 0.05f,
                    ClairDeLunePalette.PearlFrost with { A = 0 } * 0.6f, 0.12f, 6);
                MagnumParticleHandler.SpawnParticle(shimmer);
            }

            // Subtle crystal trail dust
            if (Main.GameUpdateCount % 2 == 0)
            {
                var trail = new GenericGlowParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(3f, 3f),
                    -Projectile.velocity * 0.02f,
                    ClairDeLunePalette.SoftBlue with { A = 0 } * 0.3f, 0.06f, 8, true);
                MagnumParticleHandler.SpawnParticle(trail);
            }

            Lighting.AddLight(Projectile.Center, ClairDeLunePalette.PearlFrost.ToVector3() * 0.3f);
        }

        public override void OnKill(int timeLeft)
        {
            Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, Vector2.Zero,
                ModContent.ProjectileType<TemporalFractureProjectile>(),
                (int)(Projectile.damage * 0.4f), 0f, Projectile.owner);

            SoundEngine.PlaySound(SoundID.Item27 with { Pitch = 0.3f, Volume = 0.4f }, Projectile.Center);

            // Impact burst — 8 sparkles + bloom flash
            var flash = new BloomParticle(Projectile.Center, Vector2.Zero,
                ClairDeLunePalette.PearlFrost with { A = 0 } * 0.6f, 0.3f, 8);
            MagnumParticleHandler.SpawnParticle(flash);

            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 vel = angle.ToRotationVector2() * 3f;
                var shard = new SparkleParticle(Projectile.Center, vel,
                    ClairDeLunePalette.SoftBlue with { A = 0 } * 0.4f, 0.08f, 10);
                MagnumParticleHandler.SpawnParticle(shard);
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            try
            {
                IncisorOrbRenderer.DrawOrbVisuals(sb, Projectile, IncisorOrbRenderer.ClairDeLune, ref _vertexStrip);

                // Temporal arrow: moonbeam crystal directional streak
                sb.End();
                sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                    SamplerState.LinearClamp, DepthStencilState.None,
                    RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

                Vector2 drawPos = Projectile.Center - Main.screenPosition;
                Texture2D glow = MagnumTextureRegistry.GetSoftGlow();
                if (glow != null)
                {
                    Vector2 origin = glow.Size() / 2f;
                    float rot = Projectile.velocity.ToRotation();

                    sb.Draw(glow, drawPos, null,
                        (ClairDeLunePalette.StarlightSilver with { A = 0 }) * 0.2f,
                        rot, origin, new Vector2(0.06f, 0.022f), SpriteEffects.None, 0f);
                }

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
