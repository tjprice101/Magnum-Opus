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
    /// Medium Gear — Arc-lobbed bouncing gear (32px, 10°/frame, bounces 3x).
    /// Part of ClockworkHarmony's gear mesh system, mid-weight transfer gear.
    /// 3 render passes: (1) GearSwing GearSwingTrail medium gear body,
    /// (2) ClairDeLunePearlGlow PearlShimmer bounce flash, (3) Multi-scale bloom + 12 teeth.
    /// </summary>
    public class MediumGearProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Textures/InvisibleProjectile";

        private VertexStrip _vertexStrip;
        private const float GearRadius = 16f; // 32px diameter
        private const float SpinRate = MathHelper.Pi / 18f; // 10°/frame
        private const int ToothCount = 12;
        private int _bounceCount;
        private const int MaxBounces = 3;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 16;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 32;
            Projectile.height = 32;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 2;
            Projectile.timeLeft = 150;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = false;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }

        public override void AI()
        {
            Projectile.rotation += SpinRate;
            Projectile.velocity.Y += 0.15f; // Arc gravity
            Projectile.velocity *= 0.998f;

            // Trail sparks
            if (Main.GameUpdateCount % 3 == 0)
            {
                float sparkAngle = Projectile.rotation + Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 sparkPos = Projectile.Center + sparkAngle.ToRotationVector2() * GearRadius * 0.8f;
                var spark = new GenericGlowParticle(sparkPos,
                    -Projectile.velocity * 0.1f,
                    ClairDeLunePalette.SoftBlue with { A = 0 } * 0.25f, 0.03f, 5, true);
                MagnumParticleHandler.SpawnParticle(spark);
            }

            Lighting.AddLight(Projectile.Center, ClairDeLunePalette.SoftBlue.ToVector3() * 0.2f);
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (_bounceCount >= MaxBounces)
                return true;

            _bounceCount++;
            SoundEngine.PlaySound(SoundID.Item10 with { Pitch = 0.1f * _bounceCount, Volume = 0.3f }, Projectile.Center);

            // Bounce reflection
            if (Math.Abs(Projectile.velocity.X - oldVelocity.X) > float.Epsilon)
                Projectile.velocity.X = -oldVelocity.X * 0.75f;
            if (Math.Abs(Projectile.velocity.Y - oldVelocity.Y) > float.Epsilon)
                Projectile.velocity.Y = -oldVelocity.Y * 0.75f;

            // Bounce spark burst
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f;
                Vector2 vel = angle.ToRotationVector2() * 2f;
                var bounce = new GenericGlowParticle(Projectile.Center, vel,
                    ClairDeLunePalette.MoonbeamGold with { A = 0 } * 0.4f, 0.05f, 6, true);
                MagnumParticleHandler.SpawnParticle(bounce);
            }

            return false;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (Main.dedServ) return false;

            SpriteBatch sb = Main.spriteBatch;
            try
            {
                IncisorOrbRenderer.DrawOrbVisuals(sb, Projectile, IncisorOrbRenderer.ClairDeLune, ref _vertexStrip);

                // --- Medium gear brass glow accent ---
                sb.End();
                sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, SamplerState.LinearClamp,
                    DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

                var glowTex = MagnumTextureRegistry.GetSoftGlow();
                Vector2 origin = glowTex.Size() / 2f;
                Vector2 pos = Projectile.Center - Main.screenPosition;
                float rot = Projectile.rotation + (float)(Main.timeForVisualEffects * 0.06);
                Color brass = (ClairDeLunePalette.ClockworkBrass with { A = 0 }) * 0.55f;
                sb.Draw(glowTex, pos, null, brass, rot, origin, 0.035f, SpriteEffects.None, 0f);

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
