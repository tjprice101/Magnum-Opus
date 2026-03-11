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
    /// Drive Gear — Heavy slow gear (48px, 5°/frame spin, 2x damage).
    /// The main driving gear of ClockworkHarmony's gear mesh system.
    /// 3 render passes: (1) GearSwing GearSwingArc heavy gear body,
    /// (2) ClairDeLuneMoonlit MoonlitGlow ambient aura, (3) Multi-scale bloom + 16 gear teeth.
    /// </summary>
    public class DriveGearProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Textures/InvisibleProjectile";

        private VertexStrip _vertexStrip;
        private const float GearRadius = 24f; // 48px diameter
        private const float SpinRate = MathHelper.Pi / 36f; // 5°/frame
        private const int ToothCount = 16;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 16;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 48;
            Projectile.height = 48;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 3;
            Projectile.timeLeft = 180;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = false;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 12;
        }

        public override void AI()
        {
            Projectile.rotation += SpinRate;
            Projectile.velocity.Y += 0.06f; // Subtle gravity — it's heavy
            Projectile.velocity *= 0.995f;

            // Grinding sparks
            if (Main.GameUpdateCount % 4 == 0)
            {
                float sparkAngle = Projectile.rotation + Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 sparkPos = Projectile.Center + sparkAngle.ToRotationVector2() * GearRadius;
                var spark = new GenericGlowParticle(sparkPos,
                    sparkAngle.ToRotationVector2() * 1.5f,
                    ClairDeLunePalette.ClockworkBrass with { A = 0 } * 0.3f, 0.04f, 6, true);
                MagnumParticleHandler.SpawnParticle(spark);
            }

            Lighting.AddLight(Projectile.Center, ClairDeLunePalette.ClockworkBrass.ToVector3() * 0.25f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            SoundEngine.PlaySound(SoundID.Item10 with { Pitch = -0.3f, Volume = 0.4f }, Projectile.Center);

            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 vel = angle.ToRotationVector2() * 2.5f;
                var spark = new GenericGlowParticle(target.Center, vel,
                    ClairDeLunePalette.MoonbeamGold with { A = 0 } * 0.4f, 0.05f, 8, true);
                MagnumParticleHandler.SpawnParticle(spark);
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (Main.dedServ) return false;

            SpriteBatch sb = Main.spriteBatch;
            try
            {
                IncisorOrbRenderer.DrawOrbVisuals(sb, Projectile, IncisorOrbRenderer.ClairDeLune, ref _vertexStrip);

                // --- Drive gear brass corona accent ---
                sb.End();
                sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, SamplerState.LinearClamp,
                    DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

                var glowTex = MagnumTextureRegistry.GetSoftGlow();
                Vector2 origin = glowTex.Size() / 2f;
                Vector2 pos = Projectile.Center - Main.screenPosition;
                float rot = Projectile.rotation + (float)(Main.timeForVisualEffects * 0.04);
                float pulse = 0.9f + 0.1f * (float)Math.Sin(Main.timeForVisualEffects * 0.05);
                Color brass = (ClairDeLunePalette.ClockworkBrass with { A = 0 }) * 0.6f * pulse;
                sb.Draw(glowTex, pos, null, brass, rot, origin, 0.05f, SpriteEffects.None, 0f);

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
