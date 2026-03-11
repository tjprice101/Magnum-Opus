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

namespace MagnumOpus.Content.ClairDeLune.Weapons.OrreryOfDreams.Projectiles
{
    /// <summary>
    /// Middle Sphere Orb — Gently homing dream orb from OrreryOfDreams.
    /// Floats toward enemies with a dreamy, drifting quality.
    /// 3 render passes: (1) CelestialOrbit CelestialOrbitCore dreamy body,
    /// (2) SparkleTrailShader VertexStrip dream trail, (3) Multi-scale bloom halo.
    /// </summary>
    public class MiddleSphereOrbProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Textures/InvisibleProjectile";

        private const float HomingRange = 300f;
        private const float HomingStrength = 0.03f;
        private const float MaxSpeed = 7f;
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
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 180;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();

            // Dreamy sine-wave drift
            float drift = MathF.Sin(Main.GameUpdateCount * 0.08f + Projectile.whoAmI) * 0.3f;
            Projectile.velocity = Projectile.velocity.RotatedBy(drift * 0.02f);

            // Gentle homing
            NPC closest = null;
            float closestDist = HomingRange;
            for (int n = 0; n < Main.maxNPCs; n++)
            {
                NPC npc = Main.npc[n];
                if (!npc.active || npc.friendly || npc.dontTakeDamage) continue;
                float dist = Vector2.Distance(Projectile.Center, npc.Center);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closest = npc;
                }
            }

            if (closest != null)
            {
                Vector2 toTarget = (closest.Center - Projectile.Center).SafeNormalize(Vector2.UnitX);
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, toTarget * MaxSpeed, HomingStrength);
            }

            // Dream dust
            if (Main.GameUpdateCount % 3 == 0)
            {
                var dream = new GenericGlowParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(4f, 4f),
                    -Projectile.velocity * 0.03f + Main.rand.NextVector2Circular(0.3f, 0.3f),
                    ClairDeLunePalette.NightMist with { A = 0 } * 0.2f, 0.04f, 8, true);
                MagnumParticleHandler.SpawnParticle(dream);
            }

            Lighting.AddLight(Projectile.Center, ClairDeLunePalette.SoftBlue.ToVector3() * 0.2f);
        }

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 vel = angle.ToRotationVector2() * 2f;
                var burst = new SparkleParticle(Projectile.Center, vel,
                    ClairDeLunePalette.PearlFrost with { A = 0 } * 0.4f, 0.06f, 8);
                MagnumParticleHandler.SpawnParticle(burst);
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            try
            {
                IncisorOrbRenderer.DrawOrbVisuals(sb, Projectile, IncisorOrbRenderer.ClairDeLune, ref _vertexStrip);

                // Dream orb: dreamy blue orbital halo
                sb.End();
                sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                    SamplerState.LinearClamp, DepthStencilState.None,
                    RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

                Vector2 drawPos = Projectile.Center - Main.screenPosition;
                Texture2D glow = MagnumTextureRegistry.GetSoftGlow();
                if (glow != null)
                {
                    Vector2 origin = glow.Size() / 2f;
                    float pulse = 0.8f + 0.2f * MathF.Sin((float)Main.timeForVisualEffects * 0.1f + Projectile.whoAmI);

                    sb.Draw(glow, drawPos, null,
                        (ClairDeLunePalette.DreamBlue with { A = 0 }) * 0.18f * pulse,
                        0f, origin, 0.045f, SpriteEffects.None, 0f);
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
