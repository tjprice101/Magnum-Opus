using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.VFX.Core;
using MagnumOpus.Content.OdeToJoy;

namespace MagnumOpus.Content.OdeToJoy.Weapons.RoseThornChainsaw.Projectiles
{
    /// <summary>
    /// Short-lived rapid-fire orb projectile for Rose Thorn Chainsaw.
    /// Straight-shot, no homing. When empowered (ai[0] >= 1), gains penetrate=2.
    /// IncisorOrb rendering with Ode to Joy theme.
    /// ai[0] = empowered flag (1 = empowered, 0 = normal)
    /// </summary>
    public class RoseThornChainsawSpecialProj : ModProjectile
    {
        private VertexStrip _strip;
        private bool _initialized;

        public override string Texture => "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftGlow";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 12;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 40;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.extraUpdates = 1;
        }

        public override void AI()
        {
            if (!_initialized)
            {
                _initialized = true;
                Projectile.rotation = Projectile.velocity.ToRotation();

                // Empowered mode: gain extra pierce
                if (Projectile.ai[0] >= 1f)
                    Projectile.penetrate = 2;
            }

            // Straight shot — no homing, pure directional speed
            Projectile.rotation = Projectile.velocity.ToRotation();

            // Ode to Joy dust trail
            if (Main.rand.NextBool(3))
            {
                int dustType = Main.rand.NextBool() ? DustID.GreenTorch : DustID.GoldFlame;
                Color dustColor = Main.rand.NextBool() ? new Color(90, 200, 60) : new Color(255, 210, 60);
                Dust d = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(4f, 4f),
                    dustType, -Projectile.velocity * 0.12f + Main.rand.NextVector2Circular(0.4f, 0.4f),
                    0, dustColor, 0.7f);
                d.noGravity = true;
                d.fadeIn = 0.5f;
            }

            // Golden-green light
            float pulse = 1f + 0.15f * MathF.Sin(Projectile.timeLeft * 0.25f);
            Lighting.AddLight(Projectile.Center, new Vector3(0.4f, 0.5f, 0.15f) * 0.35f * pulse);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Vector2 hitPos = target.Center;

            for (int i = 0; i < 5; i++)
            {
                Vector2 sparkVel = Main.rand.NextVector2CircularEdge(3.5f, 3.5f);
                Color col = i % 2 == 0 ? new Color(90, 200, 60) : new Color(255, 210, 60);
                Dust d = Dust.NewDustPerfect(hitPos, DustID.GreenTorch, sparkVel, 0, col, 0.5f);
                d.noGravity = true;
            }
            for (int i = 0; i < 2; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(2f, 2f) + new Vector2(0, -0.8f);
                Dust d = Dust.NewDustPerfect(hitPos + Main.rand.NextVector2Circular(6f, 6f),
                    DustID.GoldFlame, vel, 0, new Color(255, 210, 60), 0.4f);
                d.noGravity = true;
            }

            try { OdeToJoyVFXLibrary.SpawnMusicNotes(hitPos, 1, 10f, 0.3f, 0.6f, 18); } catch { }
            try { OdeToJoyVFXLibrary.SpawnMixedSparkleImpact(hitPos, 0.4f, 3, 3); } catch { }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            try
            {
                IncisorOrbRenderer.DrawOrbVisuals(sb, Projectile, IncisorOrbRenderer.OdeToJoy, ref _strip);
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

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 4; i++)
            {
                Vector2 sparkVel = Main.rand.NextVector2CircularEdge(2.5f, 2.5f);
                Color col = Main.rand.NextBool() ? new Color(90, 200, 60) : new Color(255, 210, 60);
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.GreenTorch, sparkVel, 0, col, 0.3f);
                d.noGravity = true;
            }

            try { OdeToJoyVFXLibrary.SpawnMusicNotes(Projectile.Center, 1, 10f, 0.4f, 0.6f, 18); } catch { }
            try { OdeToJoyVFXLibrary.SpawnMixedSparkleImpact(Projectile.Center, 0.4f, 3, 3); } catch { }
        }
    }
}
