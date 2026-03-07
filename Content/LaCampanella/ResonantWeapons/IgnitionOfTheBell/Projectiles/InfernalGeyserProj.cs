using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.LaCampanella.ResonantWeapons.IgnitionOfTheBell.Utilities;
using MagnumOpus.Content.LaCampanella.ResonantWeapons.IgnitionOfTheBell.Particles;
using MagnumOpus.Content.LaCampanella.ResonantWeapons.IgnitionOfTheBell.Shaders;
using MagnumOpus.Content.LaCampanella.Debuffs;
using Terraria.Graphics.Shaders;
using MagnumOpus.Content.FoundationWeapons.ImpactFoundation;
using MagnumOpus.Content.FoundationWeapons.ExplosionParticlesFoundation;

namespace MagnumOpus.Content.LaCampanella.ResonantWeapons.IgnitionOfTheBell.Projectiles
{
    /// <summary>
    /// InfernalGeyserProj - Ground fire pillar that erupts vertically from a point.
    /// Spawned on-hit by Ignition Strike (full-size) and Tolling Frenzy (smaller).
    /// ai[0] = 0 for full geyser, 1 for small variant.
    /// Deals AoE damage in vertical column + leaves lingering fire damage.
    /// </summary>
    public class InfernalGeyserProj : ModProjectile
    {
        private const int FullDuration = 35;
        private const int SmallDuration = 22;
        private const float FullHeight = 180f;
        private const float SmallHeight = 100f;
        private const float FullWidth = 50f;
        private const float SmallWidth = 30f;

        private bool IsSmall => Projectile.ai[0] >= 1f;
        private int Duration => IsSmall ? SmallDuration : FullDuration;
        private float GeyserHeight => IsSmall ? SmallHeight : FullHeight;
        private float GeyserWidth => IsSmall ? SmallWidth : FullWidth;

        private bool _initialized;
        private int _timer;
        private Vector2 _groundPos; // Bottom of geyser

        public override string Texture => "MagnumOpus/Content/LaCampanella/ResonantWeapons/IgnitionOfTheBell/IgnitionOfTheBell";

        public override void SetDefaults()
        {
            Projectile.width = 50;
            Projectile.height = 180;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
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
                _groundPos = Projectile.Center;
                Projectile.timeLeft = Duration;

                // Eruption sound
                SoundEngine.PlaySound(SoundID.Item45 with
                {
                    Pitch = IsSmall ? 0.3f : -0.1f,
                    Volume = IsSmall ? 0.5f : 0.8f
                }, _groundPos);

                // Initial eruption burst
                int burstCount = IsSmall ? 6 : 12;
                for (int i = 0; i < burstCount; i++)
                {
                    float angle = MathHelper.TwoPi * i / burstCount;
                    Vector2 burstVel = new Vector2((float)Math.Cos(angle) * 2f, -Main.rand.NextFloat(3f, 6f));
                    IgnitionOfTheBellParticleHandler.SpawnParticle(
                        new FlameJetParticle(_groundPos, burstVel, Main.rand.NextFloat(0.5f, 1f), 18, 0.6f));
                }

                IgnitionOfTheBellParticleHandler.SpawnParticle(
                    new BellIgnitionFlashParticle(_groundPos, IsSmall ? 8 : 14, IsSmall ? 1.2f : 2f));

                // === FOUNDATION: RippleEffectProjectile — Geyser eruption ring at base ===
                if (!IsSmall)
                {
                    Projectile.NewProjectile(
                        Projectile.GetSource_FromThis(), _groundPos, Vector2.Zero,
                        ModContent.ProjectileType<RippleEffectProjectile>(),
                        0, 0f, Projectile.owner, ai0: 1f);
                }

                // === FOUNDATION: SparkExplosionProjectile — Eruption burst sparks ===
                Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(), _groundPos, Vector2.Zero,
                    ModContent.ProjectileType<SparkExplosionProjectile>(),
                    0, 0f, Projectile.owner,
                    ai0: (float)SparkMode.FountainCascade);
            }

            _timer++;
            float progress = (float)_timer / Duration;

            // Pillar rises then fades
            float heightMult;
            if (progress < 0.3f)
                heightMult = progress / 0.3f; // Rising
            else if (progress < 0.7f)
                heightMult = 1f; // Full height
            else
                heightMult = 1f - (progress - 0.7f) / 0.3f; // Fading

            float currentHeight = GeyserHeight * heightMult;

            // Position hitbox as vertical column
            Projectile.width = (int)GeyserWidth;
            Projectile.height = (int)Math.Max(currentHeight, 10);
            Projectile.Center = _groundPos - new Vector2(0, currentHeight * 0.5f);

            // Rising flame particles
            if (heightMult > 0.2f)
            {
                int particleCount = IsSmall ? 2 : 4;
                for (int i = 0; i < particleCount; i++)
                {
                    float xOffset = Main.rand.NextFloat(-GeyserWidth * 0.4f, GeyserWidth * 0.4f);
                    float yOffset = Main.rand.NextFloat(0, -currentHeight);
                    Vector2 pos = _groundPos + new Vector2(xOffset, yOffset);
                    Vector2 vel = new Vector2(
                        Main.rand.NextFloat(-0.5f, 0.5f),
                        -Main.rand.NextFloat(2f, 5f) * heightMult);

                    IgnitionOfTheBellParticleHandler.SpawnParticle(
                        new FlameJetParticle(pos, vel, Main.rand.NextFloat(0.4f, 0.9f),
                            Main.rand.Next(10, 18), 0.5f * heightMult));
                }
            }

            // Embers scattering from top
            if (heightMult > 0.5f && Main.rand.NextBool(2))
            {
                Vector2 topPos = _groundPos - new Vector2(0, currentHeight);
                Vector2 emberVel = new Vector2(Main.rand.NextFloat(-2f, 2f), -Main.rand.NextFloat(1f, 3f));
                IgnitionOfTheBellParticleHandler.SpawnParticle(
                    new ThrustEmberParticle(topPos + Main.rand.NextVector2Circular(10f, 5f),
                        emberVel, Main.rand.NextFloat(0.4f, 0.8f), 20, 0.3f));
            }

            // Base sparks
            if (Main.rand.NextBool(3))
            {
                float angle = Main.rand.NextFloat() * MathHelper.Pi; // Upward half-circle
                Vector2 sparkVel = new Vector2((float)Math.Cos(angle) * 3f, -(float)Math.Sin(angle) * 2f);
                Dust d = Dust.NewDustPerfect(_groundPos + Main.rand.NextVector2Circular(GeyserWidth * 0.3f, 5f),
                    DustID.Torch, sparkVel, 0,
                    IgnitionOfTheBellUtils.GetMagmaFlicker(Main.rand.NextFloat()), 1.2f);
                d.noGravity = true;
            }

            // Lighting
            float lightIntensity = 0.6f * heightMult;
            Lighting.AddLight(_groundPos - new Vector2(0, currentHeight * 0.5f),
                new Vector3(0.6f, 0.25f, 0.03f) * lightIntensity);
            Lighting.AddLight(_groundPos - new Vector2(0, currentHeight),
                new Vector3(0.5f, 0.2f, 0.02f) * lightIntensity * 0.7f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.GetGlobalNPC<ResonantTollNPC>().AddStacks(target, IsSmall ? 1 : 2);

            // Fire impact sparks
            for (int i = 0; i < 4; i++)
            {
                Vector2 sparkVel = new Vector2(Main.rand.NextFloat(-3f, 3f), -Main.rand.NextFloat(2f, 5f));
                IgnitionOfTheBellParticleHandler.SpawnParticle(
                    new ThrustEmberParticle(target.Center, sparkVel,
                        Main.rand.NextFloat(0.5f, 1f), 15, 0.3f));
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            try
            {
                DrawGeyserPillar(sb);
            }
            catch
            {
                try
                {
                    sb.End();
                    sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp,
                        DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
                }
                catch { }
            }
            return false;
        }

        private void DrawGeyserPillar(SpriteBatch sb)
        {
            Texture2D bloomTex = null;
            try
            {
                bloomTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftGlow",
                    ReLogic.Content.AssetRequestMode.ImmediateLoad)?.Value;
            }
            catch { }
            if (bloomTex == null) return;

            float progress = (float)_timer / Duration;
            float heightMult;
            if (progress < 0.3f)
                heightMult = progress / 0.3f;
            else if (progress < 0.7f)
                heightMult = 1f;
            else
                heightMult = 1f - (progress - 0.7f) / 0.3f;

            float currentHeight = GeyserHeight * heightMult;
            Vector2 origin = new Vector2(bloomTex.Width / 2f, bloomTex.Height / 2f);
            float pulse = 0.8f + 0.2f * (float)Math.Sin(_timer * 0.4f);

            // === SHADER-DRIVEN GEYSER PILLAR ===
            MiscShaderData geyserShader = IgnitionOfTheBellShaderLoader.GetGeyserShader();

            // Shader overlay pass first (Immediate mode for pixel shader)
            if (geyserShader != null && heightMult > 0.1f)
            {
                try
                {
                    geyserShader.UseColor(new Color(255, 140, 30));
                    geyserShader.UseSecondaryColor(new Color(255, 240, 210));
                    geyserShader.UseOpacity(heightMult * 0.7f);
                    geyserShader.Shader.Parameters["uTime"]?.SetValue(Main.GameUpdateCount * 0.05f);
                    geyserShader.Shader.Parameters["uIntensity"]?.SetValue(heightMult * 1.5f);
                    geyserShader.Shader.Parameters["uOverbrightMult"]?.SetValue(1.6f);
                    geyserShader.Shader.Parameters["uScrollSpeed"]?.SetValue(3.0f);
                    geyserShader.Shader.Parameters["uNoiseScale"]?.SetValue(5.0f);
                    geyserShader.Shader.Parameters["uPhase"]?.SetValue(progress);
                }
                catch { }

                try { sb.End(); } catch { }
                sb.Begin(SpriteSortMode.Immediate, MagnumBlendStates.ShaderAdditive, SamplerState.LinearClamp,
                    DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
                geyserShader.Apply();

                // Shader-driven geyser column - fewer layers needed with shader distortion
                int shaderLayers = IsSmall ? 3 : 5;
                for (int i = 0; i < shaderLayers; i++)
                {
                    float t = i / (float)(shaderLayers - 1);
                    Vector2 layerPos = _groundPos - new Vector2(0, currentHeight * t) - Main.screenPosition;
                    float widthScale = MathHelper.Min((1f - t * 0.4f) * (IsSmall ? 0.2f : 0.35f), 0.293f);
                    float alphaFade = (1f - t * 0.2f) * heightMult;

                    sb.Draw(bloomTex, layerPos, null,
                        IgnitionOfTheBellUtils.Additive(new Color(255, 140, 30), 0.45f * alphaFade * pulse),
                        0f, origin, widthScale, SpriteEffects.None, 0f);
                }

                sb.End();
            }

            try { sb.End(); } catch { }
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            // Non-shader bloom layers for depth and glow
            int layerCount = IsSmall ? 4 : 7;
            for (int i = 0; i < layerCount; i++)
            {
                float t = i / (float)(layerCount - 1);
                Vector2 layerPos = _groundPos - new Vector2(0, currentHeight * t) - Main.screenPosition;

                float widthScale = (1f - t * 0.5f) * (IsSmall ? 0.18f : 0.28f);
                float alphaFade = (1f - t * 0.3f) * heightMult;

                // Deep ember outer
                sb.Draw(bloomTex, layerPos, null,
                    IgnitionOfTheBellUtils.Additive(new Color(200, 50, 0), 0.2f * alphaFade * pulse),
                    0f, origin, widthScale * 0.5f, SpriteEffects.None, 0f);

                // Bright orange mid
                sb.Draw(bloomTex, layerPos, null,
                    IgnitionOfTheBellUtils.Additive(new Color(255, 140, 30), 0.35f * alphaFade * pulse),
                    0f, origin, widthScale, SpriteEffects.None, 0f);

                // White-hot core
                sb.Draw(bloomTex, layerPos, null,
                    IgnitionOfTheBellUtils.Additive(new Color(255, 240, 210), 0.5f * alphaFade),
                    0f, origin, widthScale * 0.3f, SpriteEffects.None, 0f);

                // Additional flicker at each layer for visual richness
                float flicker = (float)Math.Sin(_timer * 0.5f + t * 5f) * 0.1f + 0.9f;
                sb.Draw(bloomTex, layerPos + new Vector2((float)Math.Sin(_timer * 0.3f + t * 3f) * 3f, 0), null,
                    IgnitionOfTheBellUtils.Additive(new Color(255, 200, 100), 0.15f * alphaFade * flicker),
                    0f, origin, widthScale * 0.3f, SpriteEffects.None, 0f);
            }

            // Base eruption bloom
            Vector2 basePos = _groundPos - Main.screenPosition;
            sb.Draw(bloomTex, basePos, null,
                IgnitionOfTheBellUtils.Additive(new Color(255, 100, 0), 0.4f * heightMult * pulse),
                0f, origin, IsSmall ? 0.12f : 0.18f, SpriteEffects.None, 0f);

            // Tip glow at top of geyser
            Vector2 tipPos = _groundPos - new Vector2(0, currentHeight) - Main.screenPosition;
            float tipPulse = 0.7f + 0.3f * (float)Math.Sin(_timer * 0.6f);
            sb.Draw(bloomTex, tipPos, null,
                IgnitionOfTheBellUtils.Additive(new Color(255, 220, 150), 0.35f * heightMult * tipPulse),
                0f, origin, (IsSmall ? 0.06f : 0.1f) * tipPulse, SpriteEffects.None, 0f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }

        public override void OnKill(int timeLeft)
        {
            // Final burst at base
            int burstCount = IsSmall ? 4 : 8;
            for (int i = 0; i < burstCount; i++)
            {
                Vector2 burstVel = Main.rand.NextVector2CircularEdge(3f, 3f);
                burstVel.Y = -Math.Abs(burstVel.Y); // Force upward
                IgnitionOfTheBellParticleHandler.SpawnParticle(
                    new ThrustEmberParticle(_groundPos, burstVel,
                        Main.rand.NextFloat(0.5f, 1f), 20, 0.35f));
            }
        }
    }
}
