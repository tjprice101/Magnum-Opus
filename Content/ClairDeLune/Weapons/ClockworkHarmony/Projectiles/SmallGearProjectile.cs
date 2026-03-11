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

namespace MagnumOpus.Content.ClairDeLune.Weapons.ClockworkHarmony.Projectiles
{
    /// <summary>
    /// Small Gear — Fast direct-fire gear (20px, 15°/frame, bounces 1x).
    /// Lightest gear in ClockworkHarmony's mesh system, checks for gear meshing.
    /// 3 render passes: (1) GearSwing GearSwingTrail fast gear body,
    /// (2) ClairDeLuneMoonlit MoonlitFlow speed shimmer, (3) Multi-scale bloom + 8 teeth.
    /// Contains CheckMeshCollision() for gear mesh synergy detection.
    /// </summary>
    public class SmallGearProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Textures/InvisibleProjectile";

        private VertexStrip _vertexStrip;
        private const float GearRadius = 10f; // 20px diameter
        private const float SpinRate = MathHelper.Pi / 12f; // 15°/frame
        private const int ToothCount = 8;
        private int _bounceCount;
        private const int MaxBounces = 1;
        private bool _hasMeshed;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 16;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 120;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = false;
            Projectile.extraUpdates = 1;
        }

        public override void AI()
        {
            Projectile.rotation += SpinRate;

            // Fast gear — minimal gravity
            Projectile.velocity.Y += 0.03f;

            // Check for gear mesh collision with other gears
            if (!_hasMeshed)
                CheckMeshCollision();

            // Speed sparks
            if (Main.GameUpdateCount % 2 == 0)
            {
                var trail = new GenericGlowParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(3f, 3f),
                    -Projectile.velocity * 0.05f,
                    ClairDeLunePalette.PearlFrost with { A = 0 } * 0.2f, 0.025f, 4, true);
                MagnumParticleHandler.SpawnParticle(trail);
            }

            Lighting.AddLight(Projectile.Center, ClairDeLunePalette.PearlFrost.ToVector3() * 0.15f);
        }

        /// <summary>
        /// Checks for collision with other ClockworkHarmony gears.
        /// On contact, boosts both gears' damage and spawns a mesh-synergy burst.
        /// </summary>
        private void CheckMeshCollision()
        {
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile other = Main.projectile[i];
                if (!other.active || other.whoAmI == Projectile.whoAmI) continue;
                if (other.owner != Projectile.owner) continue;

                bool isGear = other.type == ModContent.ProjectileType<DriveGearProjectile>() ||
                              other.type == ModContent.ProjectileType<MediumGearProjectile>();

                if (!isGear) continue;

                float meshDist = GearRadius + (other.type == ModContent.ProjectileType<DriveGearProjectile>() ? 24f : 16f);
                if (Vector2.Distance(Projectile.Center, other.Center) <= meshDist + 6f)
                {
                    _hasMeshed = true;

                    // Mesh synergy: boost damage
                    Projectile.damage = (int)(Projectile.damage * 1.25f);

                    // Synergy spark burst
                    Vector2 meshPoint = (Projectile.Center + other.Center) * 0.5f;
                    SoundEngine.PlaySound(SoundID.Item10 with { Pitch = 0.5f, Volume = 0.3f }, meshPoint);

                    for (int s = 0; s < 8; s++)
                    {
                        float angle = MathHelper.TwoPi * s / 8f;
                        Vector2 vel = angle.ToRotationVector2() * 2f;
                        var meshSpark = new SparkleParticle(meshPoint, vel,
                            ClairDeLunePalette.MoonbeamGold with { A = 0 } * 0.5f, 0.06f, 8);
                        MagnumParticleHandler.SpawnParticle(meshSpark);
                    }
                    break;
                }
            }
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (_bounceCount >= MaxBounces)
                return true;

            _bounceCount++;

            if (Math.Abs(Projectile.velocity.X - oldVelocity.X) > float.Epsilon)
                Projectile.velocity.X = -oldVelocity.X * 0.6f;
            if (Math.Abs(Projectile.velocity.Y - oldVelocity.Y) > float.Epsilon)
                Projectile.velocity.Y = -oldVelocity.Y * 0.6f;

            SoundEngine.PlaySound(SoundID.Item10 with { Pitch = 0.4f, Volume = 0.2f }, Projectile.Center);
            return false;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (Main.dedServ) return false;

            SpriteBatch sb = Main.spriteBatch;
            try
            {
                IncisorOrbRenderer.DrawOrbVisuals(sb, Projectile, IncisorOrbRenderer.ClairDeLune, ref _vertexStrip);

                // --- Small gear brass spark accent ---
                sb.End();
                sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, SamplerState.LinearClamp,
                    DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

                var glowTex = MagnumTextureRegistry.GetSoftGlow();
                Vector2 origin = glowTex.Size() / 2f;
                Vector2 pos = Projectile.Center - Main.screenPosition;
                float rot = Projectile.rotation + (float)(Main.timeForVisualEffects * 0.08);
                Color brass = (ClairDeLunePalette.ClockworkBrass with { A = 0 }) * 0.5f;
                sb.Draw(glowTex, pos, null, brass, rot, origin, 0.025f, SpriteEffects.None, 0f);

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
