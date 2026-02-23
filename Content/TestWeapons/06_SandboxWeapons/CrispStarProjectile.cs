using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.Shaders;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.VFX.Bloom;
using MagnumOpus.Common.Systems.VFX.Core;
using MagnumOpus.Common.Systems.VFX.Effects;
using MagnumOpus.Common.Systems.VFX.Screen;
using MagnumOpus.Common.Systems.VFX.Trails;
using MagnumOpus.Content.TestWeapons.SandboxWeapons.Shaders;

namespace MagnumOpus.Content.TestWeapons.SandboxWeapons
{
    /// <summary>
    /// Exoblade-inspired crystal energy bolt spawned during Sandbox TerraBlade left-click swing.
    /// 4 bolts are spawned across 50-85% of the swing arc, staggered in time (Exobeam pattern).
    ///
    /// Movement: Straight outward flight → aggressive homing after staggered delay.
    /// Rendering: Shader trail + motion blur + 4-layer bloom stack + crystal body + flare sparkle.
    /// Impact: ImpactLightBeamVFX flare + GodRay burst + enhanced TerraBladeStarImpact explosion.
    ///
    /// ai[0] = spawn index (0-3) — staggers homing delay so bolts don't all turn at once.
    /// </summary>
    public class CrispStarProjectile : ModProjectile
    {
        #region Constants

        private const int TrailCacheSize = 20;

        // Phase 1: Straight flight
        private const int BaseLaunchFrames = 20;
        private const int LaunchFrameStagger = 4;

        // Phase 2: Homing
        private const float HomingRange = 1200f;
        private const float HomingLerp = 0.07f;
        private const float HomingSpeed = 16f;
        private const float MaxSpeed = 18f;
        private const float SpeedRamp = 0.15f;

        // Impact light beams
        private const int ImpactBeamCount = 4;

        #endregion

        #region State

        private int cachedTargetWhoAmI = -1;
        private int timer = 0;

        private int LaunchFrames => BaseLaunchFrames + (int)(Projectile.ai[0]) * LaunchFrameStagger;

        #endregion

        #region Setup

        public override string Texture => "MagnumOpus/Assets/Particles/CrispStar4";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = TrailCacheSize;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.MeleeNoSpeed;
            Projectile.penetrate = 1;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 300;
            Projectile.extraUpdates = 1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }

        #endregion

        #region AI

        public override void AI()
        {
            timer++;
            Projectile.rotation = Projectile.velocity.ToRotation();

            if (timer <= LaunchFrames)
            {
                // Phase 1: Straight outward flight with gentle deceleration
                Projectile.velocity *= 0.99f;
            }
            else
            {
                // Phase 2: Aggressive homing (Exobeam-style)
                if (cachedTargetWhoAmI < 0)
                    AcquireTarget();

                if (cachedTargetWhoAmI >= 0 && cachedTargetWhoAmI < Main.maxNPCs)
                {
                    NPC target = Main.npc[cachedTargetWhoAmI];
                    if (target.active && !target.friendly && !target.dontTakeDamage)
                    {
                        Vector2 toTarget = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitY);
                        Projectile.velocity = Vector2.Lerp(Projectile.velocity, toTarget * HomingSpeed, HomingLerp);

                        // Speed ramp toward max
                        float speed = Projectile.velocity.Length();
                        if (speed > 0.01f && speed < MaxSpeed)
                        {
                            Projectile.velocity *= (speed + SpeedRamp) / speed;
                        }
                    }
                    else
                    {
                        cachedTargetWhoAmI = -1;
                    }
                }
                else
                {
                    // No target — maintain forward drift
                    Projectile.velocity *= 0.997f;
                }
            }

            // Dense particle trail (Exobeam-inspired)
            EmitTrailParticles();

            // Dynamic lighting
            Color light = TerraBladeShaderManager.GetPaletteColor(0.5f);
            Lighting.AddLight(Projectile.Center, light.ToVector3() * 0.7f);
        }

        private void EmitTrailParticles()
        {
            // Every frame: 1-2 dust particles (like Exobeam's per-frame RainbowMk2 dust)
            int dustCount = timer % 2 == 0 ? 2 : 1;
            for (int i = 0; i < dustCount; i++)
            {
                Vector2 dustVel = -Projectile.velocity * 0.05f + Main.rand.NextVector2Circular(1f, 1f);
                Color dustColor = TerraBladeShaderManager.GetPaletteColor(Main.rand.NextFloat(0.3f, 0.8f));
                Dust d = Dust.NewDustPerfect(
                    Projectile.Center + Main.rand.NextVector2Circular(6f, 6f),
                    DustID.GreenTorch, dustVel, 0, dustColor, 0.9f);
                d.noGravity = true;
                d.fadeIn = 1.0f;
            }

            // Every 3 frames: trailing GlowSpark
            if (timer % 3 == 0)
            {
                Vector2 sparkVel = -Projectile.velocity.SafeNormalize(Vector2.UnitY) * Main.rand.NextFloat(0.5f, 2f);
                sparkVel = sparkVel.RotatedBy(Main.rand.NextFloat(-0.5f, 0.5f));
                Color sparkColor = TerraBladeShaderManager.GetPaletteColor(Main.rand.NextFloat(0.4f, 0.8f));
                var spark = new GlowSparkParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(4f, 4f),
                    sparkVel, sparkColor, 0.12f, 12);
                MagnumParticleHandler.SpawnParticle(spark);
            }
        }

        private void AcquireTarget()
        {
            float bestDist = HomingRange;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly || npc.dontTakeDamage || npc.immortal)
                    continue;
                float dist = Vector2.Distance(Projectile.Center, npc.Center);
                if (dist < bestDist)
                {
                    bestDist = dist;
                    cachedTargetWhoAmI = i;
                }
            }
        }

        #endregion

        #region Hit Effects

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Spawn enhanced impact explosion
            if (Main.myPlayer == Projectile.owner)
            {
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), target.Center, Vector2.Zero,
                    ModContent.ProjectileType<TerraBladeStarImpact>(), 0, 0f, Projectile.owner);
            }

            // --- Lighting Flare Effect ---
            // Spawn 4 ImpactLightBeamVFX at evenly-spaced radial angles
            if (Main.myPlayer == Projectile.owner)
            {
                float baseAngle = Main.rand.NextFloat(MathHelper.TwoPi);
                for (int i = 0; i < ImpactBeamCount; i++)
                {
                    float angle = baseAngle + i * (MathHelper.TwoPi / ImpactBeamCount);
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), target.Center, Vector2.Zero,
                        ModContent.ProjectileType<ImpactLightBeamVFX>(), 0, 0f, Projectile.owner,
                        ai0: angle);
                }
            }

            // GodRay burst
            GodRaySystem.CreateBurst(
                target.Center,
                TerraBladeShaderManager.GetPaletteColor(0.5f),
                rayCount: 8,
                radius: 100f,
                duration: 30,
                style: GodRaySystem.GodRayStyle.Explosion,
                secondaryColor: TerraBladeShaderManager.GetPaletteColor(0.8f));

            // Screen effects
            ScreenDistortionManager.TriggerChromaticBurst(target.Center, intensity: 0.25f, duration: 8);
            ScreenFlashSystem.Instance?.ImpactFlash(0.15f);
            Projectile.ShakeScreen(0.3f);

            // Dense dust burst (12 particles)
            for (int i = 0; i < 12; i++)
            {
                float angle = i / 12f * MathHelper.TwoPi;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 6f);
                Color dustColor = TerraBladeShaderManager.GetPaletteColor(Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(target.Center, DustID.GreenTorch, vel, 0, dustColor, 1.2f);
                d.noGravity = true;
            }

            // GlowSpark burst (6 sparks)
            for (int i = 0; i < 6; i++)
            {
                Vector2 sparkVel = Main.rand.NextVector2Circular(5f, 5f);
                Color sparkColor = TerraBladeShaderManager.GetPaletteColor(Main.rand.NextFloat(0.3f, 0.8f));
                var spark = new GlowSparkParticle(
                    target.Center, sparkVel, sparkColor, 0.15f, 15);
                MagnumParticleHandler.SpawnParticle(spark);
            }

            // Bloom ring
            var ring = new BloomRingParticle(target.Center, Vector2.Zero,
                TerraBladeShaderManager.GetPaletteColor(0.5f) * 0.7f, 0.2f, 15);
            MagnumParticleHandler.SpawnParticle(ring);

            Lighting.AddLight(target.Center, 0.5f, 0.8f, 0.5f);
        }

        public override void OnKill(int timeLeft)
        {
            // Fizzle-out: smaller impact explosion
            if (Main.myPlayer == Projectile.owner)
            {
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero,
                    ModContent.ProjectileType<TerraBladeStarImpact>(), 0, 0f, Projectile.owner,
                    ai0: 0.5f);
            }

            // 2 small light beams at random angles
            if (Main.myPlayer == Projectile.owner)
            {
                for (int i = 0; i < 2; i++)
                {
                    float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero,
                        ModContent.ProjectileType<ImpactLightBeamVFX>(), 0, 0f, Projectile.owner,
                        ai0: angle);
                }
            }

            // Dust fizzle
            for (int i = 0; i < 6; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(3f, 3f);
                Color dustColor = TerraBladeShaderManager.GetPaletteColor(Main.rand.NextFloat(0.3f, 0.7f));
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.GreenTorch, vel, 0, dustColor, 0.8f);
                d.noGravity = true;
            }

            Lighting.AddLight(Projectile.Center, 0.3f, 0.5f, 0.3f);
        }

        #endregion

        #region Rendering

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float time = Main.GlobalTimeWrappedHourly;

            // Layer 1: Shader trail (Nature style, like CosmicSpinShard/TerraBladeEnergyBolt)
            DrawShaderTrail(time);

            // Layer 2: Motion blur afterimages
            DrawMotionBlur(sb);

            // Layer 3: Bloom stack (4 layers at projectile center)
            DrawBloomStack(sb, drawPos, time);

            // Layer 4: Crystal body (CrispStar4, velocity-stretched)
            DrawCrystalBody(sb, drawPos, time);

            // Layer 5: FlareSparkle overlay (counter-rotating)
            DrawFlareSparkle(sb, drawPos, time);

            return false;
        }

        private void DrawShaderTrail(float time)
        {
            try
            {
                float hueShift = MathF.Sin(time * 4f + timer * 0.08f) * 0.15f;
                Color trailPrimary = TerraBladeShaderManager.GetPaletteColor(0.4f + hueShift);
                Color trailSecondary = TerraBladeShaderManager.GetPaletteColor(0.8f - hueShift);

                CalamityStyleTrailRenderer.DrawProjectileTrailWithBloom(
                    Projectile,
                    CalamityStyleTrailRenderer.TrailStyle.Nature,
                    baseWidth: 15f,
                    primaryColor: trailPrimary,
                    secondaryColor: trailSecondary,
                    intensity: 1.2f,
                    bloomMultiplier: 2.0f);
            }
            catch { }
        }

        private void DrawMotionBlur(SpriteBatch sb)
        {
            try
            {
                Texture2D starTex = ModContent.Request<Texture2D>(Texture).Value;
                if (starTex != null)
                {
                    Color mbPrimary = TerraBladeShaderManager.GetPaletteColor(0.4f);
                    Color mbSecondary = TerraBladeShaderManager.GetPaletteColor(0.7f);
                    MotionBlurBloomRenderer.DrawProjectile(sb, starTex, Projectile, mbPrimary, mbSecondary, 0.8f);
                }
            }
            catch { }
        }

        private void DrawBloomStack(SpriteBatch sb, Vector2 drawPos, float time)
        {
            Texture2D bloomTex = TextureAssets.Extra[98].Value;
            if (bloomTex == null) return;

            Vector2 bloomOrigin = bloomTex.Size() * 0.5f;
            float pulse = 0.9f + MathF.Sin(time * 10f + timer * 0.25f) * 0.1f;

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null,
                Main.GameViewMatrix.TransformationMatrix);

            // Outer bloom
            Color outerColor = TerraBladeShaderManager.GetPaletteColor(0.3f) with { A = 0 };
            sb.Draw(bloomTex, drawPos, null, outerColor * 0.30f,
                0f, bloomOrigin, 0.8f * pulse, SpriteEffects.None, 0f);

            // Mid bloom
            Color midColor = TerraBladeShaderManager.GetPaletteColor(0.5f) with { A = 0 };
            sb.Draw(bloomTex, drawPos, null, midColor * 0.50f,
                0f, bloomOrigin, 0.5f * pulse, SpriteEffects.None, 0f);

            // Core bloom
            Color coreColor = TerraBladeShaderManager.GetPaletteColor(0.7f) with { A = 0 };
            sb.Draw(bloomTex, drawPos, null, coreColor * 0.70f,
                0f, bloomOrigin, 0.3f * pulse, SpriteEffects.None, 0f);

            // White-hot center
            sb.Draw(bloomTex, drawPos, null, (Color.White with { A = 0 }) * 0.85f,
                0f, bloomOrigin, 0.15f * pulse, SpriteEffects.None, 0f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                DepthStencilState.None, RasterizerState.CullNone, null,
                Main.GameViewMatrix.TransformationMatrix);
        }

        private void DrawCrystalBody(SpriteBatch sb, Vector2 drawPos, float time)
        {
            Texture2D starTex = ModContent.Request<Texture2D>(Texture).Value;
            if (starTex == null) return;

            Vector2 origin = starTex.Size() * 0.5f;
            float velRot = Projectile.velocity.ToRotation();
            float shimmer = 0.90f + MathF.Sin(time * 12f + timer * 0.2f) * 0.10f;

            // Velocity stretch
            float speed = Projectile.velocity.Length();
            float stretch = 1f + speed * 0.015f;

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null,
                Main.GameViewMatrix.TransformationMatrix);

            // Outer glow
            Color outerColor = TerraBladeShaderManager.GetPaletteColor(0.3f);
            sb.Draw(starTex, drawPos, null, outerColor with { A = 0 } * 0.45f,
                velRot, origin, new Vector2(0.50f * stretch, 0.50f) * shimmer, SpriteEffects.None, 0f);

            // Main body
            Color mainColor = TerraBladeShaderManager.GetPaletteColor(0.5f);
            sb.Draw(starTex, drawPos, null, mainColor with { A = 0 } * 0.9f,
                velRot, origin, new Vector2(0.28f * stretch, 0.28f) * shimmer, SpriteEffects.None, 0f);

            // White-hot core
            sb.Draw(starTex, drawPos, null, (Color.White with { A = 0 }) * 0.7f,
                velRot, origin, new Vector2(0.12f, 0.12f) * shimmer, SpriteEffects.None, 0f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                DepthStencilState.None, RasterizerState.CullNone, null,
                Main.GameViewMatrix.TransformationMatrix);
        }

        private void DrawFlareSparkle(SpriteBatch sb, Vector2 drawPos, float time)
        {
            Texture2D sparkleTex = ParticleTextureGenerator.Sparkle;
            if (sparkleTex == null) return;

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null,
                Main.GameViewMatrix.TransformationMatrix);

            Vector2 sparkleOrigin = sparkleTex.Size() * 0.5f;
            // Counter-rotating sparkle (like CosmicSpinShard)
            float sparkleRot = -time * 8f;

            Color sparkleColor = TerraBladeShaderManager.GetPaletteColor(0.6f) with { A = 0 };
            sb.Draw(sparkleTex, drawPos, null, sparkleColor * 0.5f,
                sparkleRot, sparkleOrigin, 0.30f, SpriteEffects.None, 0f);

            sb.Draw(sparkleTex, drawPos, null, (Color.White with { A = 0 }) * 0.5f,
                sparkleRot, sparkleOrigin, 0.18f, SpriteEffects.None, 0f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                DepthStencilState.None, RasterizerState.CullNone, null,
                Main.GameViewMatrix.TransformationMatrix);
        }

        #endregion

        #region Networking

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(cachedTargetWhoAmI);
            writer.Write(timer);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            cachedTargetWhoAmI = reader.ReadInt32();
            timer = reader.ReadInt32();
        }

        #endregion
    }

    /// <summary>
    /// Enhanced noise-mapped circular explosion spawned by CrispStarProjectile on impact.
    /// Renders a scrolling noise texture through a circular vignette mask via RadialScrollShader,
    /// with a CrispStar4 flash, core bloom, and spawn-frame GodRay burst.
    ///
    /// VFX Layers:
    ///   1. RadialScrollShader noise circle (scrolling energy with circular mask)
    ///   2. CrispStar4 flash (brief star shape that shrinks away)
    ///   3. Core bloom (bright center glow)
    ///
    /// Spawn-frame effects: GodRay burst + radial GlowSpark burst + dense dust explosion.
    /// ai[0] = scale multiplier (default 1.0, set to 0.5 for OnKill mini-explosion)
    /// </summary>
    public class TerraBladeStarImpact : ModProjectile
    {
        #region Constants

        private const int ImpactDuration = 22;
        private const float MaxRadius = 50f;

        #endregion

        #region State

        private int timer = 0;
        private float ScaleMultiplier => Projectile.ai[0] <= 0f ? 1f : Projectile.ai[0];

        #endregion

        #region Setup

        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.None;

        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.timeLeft = ImpactDuration + 2;
            Projectile.ignoreWater = true;
        }

        public override bool ShouldUpdatePosition() => false;

        #endregion

        #region AI

        public override void AI()
        {
            timer++;

            float progress = (float)timer / ImpactDuration;

            // Spawn-frame VFX — enhanced with GodRay burst and radial spark burst
            if (timer == 1)
            {
                // GodRay burst for dramatic light explosion
                GodRaySystem.CreateBurst(
                    Projectile.Center,
                    TerraBladeShaderManager.GetPaletteColor(0.5f),
                    rayCount: 6,
                    radius: 80f * ScaleMultiplier,
                    duration: 25,
                    style: GodRaySystem.GodRayStyle.Explosion,
                    secondaryColor: TerraBladeShaderManager.GetPaletteColor(0.8f));

                // Dense radial dust burst (12 particles)
                for (int i = 0; i < 12; i++)
                {
                    float angle = i / 12f * MathHelper.TwoPi;
                    Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 5f) * ScaleMultiplier;
                    Color dustColor = TerraBladeShaderManager.GetPaletteColor(Main.rand.NextFloat(0.3f, 0.7f));
                    Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.GreenTorch, vel, 0, dustColor, 1.0f);
                    d.noGravity = true;
                    d.fadeIn = 1.0f;
                }

                // Radial GlowSpark burst (8 sparks)
                for (int i = 0; i < 8; i++)
                {
                    float angle = i / 8f * MathHelper.TwoPi + Main.rand.NextFloat(-0.2f, 0.2f);
                    Vector2 sparkVel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 6f) * ScaleMultiplier;
                    Color sparkColor = TerraBladeShaderManager.GetPaletteColor(Main.rand.NextFloat(0.4f, 0.8f));
                    var spark = new GlowSparkParticle(
                        Projectile.Center, sparkVel, sparkColor, 0.14f, 14);
                    MagnumParticleHandler.SpawnParticle(spark);
                }
            }

            // Dynamic lighting — stronger on spawn, fading out
            float fadeAlpha = progress < 0.5f ? 1f : MathHelper.Lerp(1f, 0f, (progress - 0.5f) / 0.5f);
            float lightIntensity = timer <= 3 ? 1.2f : 0.8f;
            Color light = TerraBladeShaderManager.GetPaletteColor(0.5f);
            Lighting.AddLight(Projectile.Center, light.ToVector3() * fadeAlpha * lightIntensity * ScaleMultiplier);

            if (timer >= ImpactDuration)
                Projectile.Kill();
        }

        #endregion

        #region Rendering

        private static Texture2D SafeRequest(string path)
        {
            try
            {
                if (ModContent.HasAsset(path))
                    return ModContent.Request<Texture2D>(path).Value;
            }
            catch { }
            return null;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (timer <= 0) return false;

            SpriteBatch sb = Main.spriteBatch;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float progress = (float)timer / ImpactDuration;
            float fadeAlpha = progress < 0.5f ? 1f : MathHelper.Lerp(1f, 0f, (progress - 0.5f) / 0.5f);
            float time = Main.GlobalTimeWrappedHourly;
            float currentRadius = MaxRadius * ScaleMultiplier * MathF.Sqrt(progress);

            sb.End();

            // Layer 1: Noise circle via RadialScrollShader
            DrawNoiseCircle(sb, drawPos, currentRadius, fadeAlpha, time);

            // Layers 2-3: Star flash + core bloom (additive)
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null,
                Main.GameViewMatrix.TransformationMatrix);

            DrawStarFlash(sb, drawPos, progress, fadeAlpha, time);
            DrawCoreBloom(sb, drawPos, fadeAlpha, time);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                DepthStencilState.None, RasterizerState.CullNone, null,
                Main.GameViewMatrix.TransformationMatrix);

            return false;
        }

        /// <summary>
        /// Draws scrolling noise texture through a circular vignette mask.
        /// Uses RadialScrollShader MultiLayer technique with TerraBlade palette colors.
        /// Falls back to layered bloom circles if shader is unavailable.
        /// </summary>
        private void DrawNoiseCircle(SpriteBatch sb, Vector2 drawPos, float radius, float fadeAlpha, float time)
        {
            Effect radialShader = ShaderLoader.RadialScroll;
            Texture2D noiseTex = ShaderLoader.GetNoiseTexture("UniversalRadialFlowNoise");

            if (noiseTex == null) return;

            Vector2 noiseOrigin = noiseTex.Size() * 0.5f;
            float texSize = Math.Max(noiseTex.Width, noiseTex.Height);
            float noiseScale = radius * 2.4f / texSize;

            if (radialShader != null)
            {
                try
                {
                    radialShader.CurrentTechnique = radialShader.Techniques["MultiLayer"];
                    radialShader.Parameters["uTime"]?.SetValue(time * 2f);
                    radialShader.Parameters["uFlowSpeed"]?.SetValue(2.5f);
                    radialShader.Parameters["uRadialSpeed"]?.SetValue(1.5f);
                    radialShader.Parameters["uZoom"]?.SetValue(1.2f);
                    radialShader.Parameters["uRepeat"]?.SetValue(1.0f);
                    radialShader.Parameters["uVignetteSize"]?.SetValue(0.40f);
                    radialShader.Parameters["uVignetteBlend"]?.SetValue(0.15f);
                    radialShader.Parameters["uOpacity"]?.SetValue(fadeAlpha);
                    radialShader.Parameters["uColor"]?.SetValue(TerraBladeShaderManager.GetPaletteColor(0.5f).ToVector4());
                    radialShader.Parameters["uSecondaryColor"]?.SetValue(TerraBladeShaderManager.GetPaletteColor(0.8f).ToVector4());

                    sb.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.LinearWrap,
                        DepthStencilState.None, RasterizerState.CullNone, radialShader,
                        Main.GameViewMatrix.TransformationMatrix);

                    sb.Draw(noiseTex, drawPos, null, Color.White * 0.85f,
                        0f, noiseOrigin, noiseScale, SpriteEffects.None, 0f);

                    sb.End();
                }
                catch
                {
                    try { sb.End(); } catch { }
                }
            }
            else
            {
                // Fallback: simple layered bloom circles
                sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                    DepthStencilState.None, RasterizerState.CullNone, null,
                    Main.GameViewMatrix.TransformationMatrix);

                Texture2D bloomTex = SafeRequest("MagnumOpus/Assets/VFX/Blooms/Perfect Soft Color Bloom")
                                    ?? TextureAssets.Extra[98].Value;
                Vector2 bloomOrigin = bloomTex.Size() * 0.5f;
                float bloomScale = radius * 2f / Math.Max(bloomTex.Width, bloomTex.Height);

                Color outerColor = TerraBladeShaderManager.GetPaletteColor(0.3f);
                sb.Draw(bloomTex, drawPos, null, outerColor * 0.5f * fadeAlpha,
                    time * 0.5f, bloomOrigin, bloomScale * 1.2f, SpriteEffects.None, 0f);

                Color midColor = TerraBladeShaderManager.GetPaletteColor(0.6f);
                sb.Draw(bloomTex, drawPos, null, midColor * 0.6f * fadeAlpha,
                    0f, bloomOrigin, bloomScale * 0.7f, SpriteEffects.None, 0f);

                sb.Draw(bloomTex, drawPos, null, Color.White * 0.5f * fadeAlpha,
                    0f, bloomOrigin, bloomScale * 0.25f, SpriteEffects.None, 0f);

                sb.End();
            }
        }

        /// <summary>
        /// Brief CrispStar4 flash that appears at impact and quickly shrinks away.
        /// Gives the explosion a star-shaped origin point.
        /// </summary>
        private void DrawStarFlash(SpriteBatch sb, Vector2 drawPos, float progress, float fadeAlpha, float time)
        {
            Texture2D starTex = SafeRequest("MagnumOpus/Assets/Particles/CrispStar4");
            if (starTex == null) return;

            Vector2 starOrigin = starTex.Size() * 0.5f;

            // Star shrinks from 0.6 to 0 over first 60% of lifetime, then gone
            float starProgress = MathHelper.Clamp(progress / 0.6f, 0f, 1f);
            float starScale = (1f - starProgress) * 0.6f * ScaleMultiplier;
            float starAlpha = (1f - starProgress * starProgress);

            if (starScale <= 0.01f) return;

            float rotation = time * 3f;

            // Colored star flash
            Color starColor = TerraBladeShaderManager.GetPaletteColor(0.5f) with { A = 0 };
            sb.Draw(starTex, drawPos, null, starColor * 0.8f * starAlpha * fadeAlpha,
                rotation, starOrigin, starScale, SpriteEffects.None, 0f);

            // White-hot core star
            sb.Draw(starTex, drawPos, null, (Color.White with { A = 0 }) * 0.6f * starAlpha * fadeAlpha,
                rotation, starOrigin, starScale * 0.5f, SpriteEffects.None, 0f);
        }

        /// <summary>
        /// Small bright bloom at the explosion center for punch.
        /// </summary>
        private void DrawCoreBloom(SpriteBatch sb, Vector2 drawPos, float fadeAlpha, float time)
        {
            Texture2D bloomTex = SafeRequest("MagnumOpus/Assets/VFX/Blooms/Perfect Soft Color Bloom")
                                ?? TextureAssets.Extra[98].Value;
            Vector2 bloomOrigin = bloomTex.Size() * 0.5f;
            float pulse = 1f + MathF.Sin(time * 14f) * 0.1f;
            float scale = 0.14f * ScaleMultiplier * pulse;

            Color coreColor = TerraBladeShaderManager.GetPaletteColor(0.6f);
            sb.Draw(bloomTex, drawPos, null, coreColor * 0.5f * fadeAlpha,
                0f, bloomOrigin, scale * 1.5f, SpriteEffects.None, 0f);

            sb.Draw(bloomTex, drawPos, null, (Color.White with { A = 0 }) * 0.5f * fadeAlpha,
                0f, bloomOrigin, scale * 0.6f, SpriteEffects.None, 0f);
        }

        #endregion

        #region Networking

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(timer);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            timer = reader.ReadInt32();
        }

        #endregion
    }
}
