using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.LaCampanella.ResonantWeapons.IgnitionOfTheBell.Utilities;
using MagnumOpus.Content.LaCampanella.ResonantWeapons.IgnitionOfTheBell.Particles;
using MagnumOpus.Content.LaCampanella.Debuffs;

namespace MagnumOpus.Content.LaCampanella.ResonantWeapons.IgnitionOfTheBell.Projectiles
{
    /// <summary>
    /// ChimeCycloneProj  ETriggered when IgnitionOfTheBell hits the same enemy 3 times.
    /// An AoE fire cyclone centered on the enemy, dealing damage in a ring and
    /// applying Resonant Toll stacks. Spiraling flame particles with cherry-crimson tones.
    /// </summary>
    public class ChimeCycloneProj : ModProjectile
    {
        private const int Duration = 45;
        private const float Radius = 100f;
        private float _spinAngle;
        private bool _initialized;

        public override string Texture => "MagnumOpus/Content/LaCampanella/ResonantWeapons/IgnitionOfTheBell/IgnitionOfTheBell";

        public override void SetDefaults()
        {
            Projectile.width = 200;
            Projectile.height = 200;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.timeLeft = Duration;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 15;
        }

        public override void AI()
        {
            if (!_initialized)
            {
                _initialized = true;
                _spinAngle = Main.rand.NextFloat() * MathHelper.TwoPi;

                // Initial flash burst
                IgnitionOfTheBellParticleHandler.SpawnParticle(
                    new BellIgnitionFlashParticle(Projectile.Center, 12, 2f));
            }

            float progress = 1f - (float)Projectile.timeLeft / Duration;
            _spinAngle += 0.25f + progress * 0.3f;

            // Spiraling cyclone flame particles
            int particleCount = (int)(4 + progress * 4);
            for (int i = 0; i < particleCount; i++)
            {
                float angle = _spinAngle + i * MathHelper.TwoPi / particleCount;
                float currentRadius = Radius * (0.3f + 0.7f * progress);

                IgnitionOfTheBellParticleHandler.SpawnParticle(
                    new CycloneFlameParticle(
                        Projectile.Center,
                        angle,
                        0.15f + Main.rand.NextFloat(0.05f),
                        currentRadius * Main.rand.NextFloat(0.6f, 1f),
                        Main.rand.NextFloat(0.5f, 2f),
                        Main.rand.Next(15, 25),
                        Main.rand.NextFloat(0.4f, 0.8f)));
            }

            // Outer ember scattering
            if (Main.rand.NextBool(2))
            {
                float emberAngle = Main.rand.NextFloat() * MathHelper.TwoPi;
                Vector2 emberPos = Projectile.Center + emberAngle.ToRotationVector2() * Radius * progress;
                Vector2 emberVel = (emberAngle + MathHelper.PiOver2).ToRotationVector2() * Main.rand.NextFloat(2f, 4f);

                IgnitionOfTheBellParticleHandler.SpawnParticle(
                    new ThrustEmberParticle(emberPos, emberVel, Main.rand.NextFloat(0.5f, 1f), 20, 0.35f));
            }

            // Vanilla dust ring
            for (int i = 0; i < 2; i++)
            {
                float dustAngle = _spinAngle + Main.rand.NextFloat() * MathHelper.TwoPi;
                float dustRadius = Radius * progress * Main.rand.NextFloat(0.5f, 1f);
                Vector2 dustPos = Projectile.Center + dustAngle.ToRotationVector2() * dustRadius;

                Dust d = Dust.NewDustPerfect(dustPos, DustID.Torch,
                    (dustAngle + MathHelper.PiOver2).ToRotationVector2() * 2f,
                    0, IgnitionOfTheBellUtils.GetCycloneGradient(Main.rand.NextFloat()), 1.1f);
                d.noGravity = true;
            }

            // Pulsing light
            float intensity = 0.5f + 0.3f * (float)Math.Sin(progress * Math.PI);
            Lighting.AddLight(Projectile.Center, new Vector3(0.5f, 0.15f, 0.02f) * intensity);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.GetGlobalNPC<ResonantTollNPC>().AddStacks(target, 2);

            for (int i = 0; i < 4; i++)
            {
                Vector2 sparkVel = Main.rand.NextVector2CircularEdge(4f, 4f);
                IgnitionOfTheBellParticleHandler.SpawnParticle(
                    new ThrustEmberParticle(target.Center, sparkVel, Main.rand.NextFloat(0.5f, 1f), 15, 0.3f));
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            DrawCycloneAura(sb);
            DrawCycloneParticles(sb);
            return false;
        }

        private void DrawCycloneAura(SpriteBatch sb)
        {
            Texture2D bloomTex = null;
            try
            {
                bloomTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/SandboxLastPrism/Orbs/SoftGlow",
                    ReLogic.Content.AssetRequestMode.ImmediateLoad)?.Value;
            }
            catch { }
            if (bloomTex == null) return;

            float progress = 1f - (float)Projectile.timeLeft / Duration;
            Vector2 screenPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = new Vector2(bloomTex.Width / 2f, bloomTex.Height / 2f);

            float pulse = 0.7f + 0.3f * (float)Math.Sin(progress * Math.PI * 3f);
            float fade = (float)Math.Sin(progress * Math.PI);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            // Outer crimson cyclone glow
            float outerScale = 2f + progress * 2f;
            sb.Draw(bloomTex, screenPos, null,
                IgnitionOfTheBellUtils.Additive(new Color(140, 20, 0), 0.2f * fade * pulse),
                _spinAngle, origin, outerScale, SpriteEffects.None, 0f);

            // Mid cherry ring
            float midScale = 1.2f + progress * 1.5f;
            sb.Draw(bloomTex, screenPos, null,
                IgnitionOfTheBellUtils.Additive(new Color(220, 60, 10), 0.3f * fade * pulse),
                -_spinAngle * 0.5f, origin, midScale, SpriteEffects.None, 0f);

            // Hot core
            sb.Draw(bloomTex, screenPos, null,
                IgnitionOfTheBellUtils.Additive(new Color(255, 200, 80), 0.4f * fade),
                0f, origin, 0.6f, SpriteEffects.None, 0f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }

        private void DrawCycloneParticles(SpriteBatch sb)
        {
            IgnitionOfTheBellParticleHandler handler = ModContent.GetInstance<IgnitionOfTheBellParticleHandler>();
            handler?.DrawAllParticles(sb);
        }

        public override void OnKill(int timeLeft)
        {
            // Final burst
            for (int i = 0; i < 12; i++)
            {
                float angle = i / 12f * MathHelper.TwoPi;
                Vector2 burstVel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 6f);
                IgnitionOfTheBellParticleHandler.SpawnParticle(
                    new ThrustEmberParticle(Projectile.Center, burstVel, Main.rand.NextFloat(0.6f, 1f), 25, 0.4f));
            }

            IgnitionOfTheBellParticleHandler.SpawnParticle(
                new BellIgnitionFlashParticle(Projectile.Center, 10, 1.8f));
        }
    }
}
