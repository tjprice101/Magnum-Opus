using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.Shaders;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Content.TestWeapons.SandboxWeapons.Shaders;

namespace MagnumOpus.Content.TestWeapons.SandboxWeapons
{
    /// <summary>
    /// Large spinning clockwork gear projectile spawned by the Sandbox Terra Blade swing.
    /// Replaces the 4x NeonEnergyBall spawn with a single high-impact gear.
    ///
    /// AI Phases:
    ///   Phase 1 (0-8 frames): Brief outward deceleration
    ///   Phase 2 (9+): Homes toward nearest enemy (700f range, lerp 0.06, speed 14f)
    ///   On hit: spawns LingeringFlameZone at impact point
    ///   On kill: particle fizzle
    ///
    /// Rendering:
    ///   1. Cosmic ribbon trail (4-pass vertex mesh with CosmicTechnique shader)
    ///   2. Noise-masked gear body via TerraBladeSwingVFX EnergyTrail + ShimmerOverlay
    ///      (gear PNG alpha masks noise texture; noise scrolls visibly through shape)
    ///   3. Bloom glow stack + FlareSparkle center
    /// </summary>
    public class ClockworkGearProjectile : ModProjectile
    {
        #region Constants

        private const int TrailCacheSize = 40;
        private const int LaunchFrames = 8;
        private const float HomingRange = 700f;
        private const float HomingLerp = 0.06f;
        private const float HomingSpeed = 14f;
        private const float Deceleration = 0.92f;
        private const float RibbonTrailWidth = 90f;
        private const int RibbonTrailSegments = 50;

        #endregion

        #region State

        private int cachedTargetWhoAmI = -1;
        private int timer = 0;
        private float spinRotation = 0f;

        // Ribbon trail vertex buffers
        private static VertexPositionColorTexture[] _ribbonVerts = new VertexPositionColorTexture[RibbonTrailSegments * 2];
        private static short[] _ribbonIndices;

        #endregion

        #region Setup

        // TODO: Swap back to ClockworkGearLarge after shader testing
        public override string Texture => "MagnumOpus/Assets/Particles/CircularMask";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = TrailCacheSize;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 120;
            Projectile.height = 120;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.MeleeNoSpeed;
            Projectile.penetrate = 3;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 240;
            Projectile.extraUpdates = 1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 15;
        }

        #endregion

        #region AI

        public override void AI()
        {
            timer++;

            // Spin continuously — speed scales with velocity
            float spinSpeed = 0.15f + Projectile.velocity.Length() * 0.02f;
            spinRotation += spinSpeed;
            Projectile.rotation = spinRotation;

            if (timer <= LaunchFrames)
            {
                // Phase 1: Brief outward deceleration
                Projectile.velocity *= Deceleration;
            }
            else
            {
                // Phase 2: Home toward nearest enemy
                if (cachedTargetWhoAmI < 0)
                    AcquireTarget();

                if (cachedTargetWhoAmI >= 0 && cachedTargetWhoAmI < Main.maxNPCs)
                {
                    NPC target = Main.npc[cachedTargetWhoAmI];
                    if (target.active && !target.friendly && !target.dontTakeDamage)
                    {
                        Vector2 toTarget = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitY);
                        Projectile.velocity = Vector2.Lerp(Projectile.velocity, toTarget * HomingSpeed, HomingLerp);
                    }
                    else
                    {
                        cachedTargetWhoAmI = -1;
                    }
                }
                else
                {
                    Projectile.velocity *= 0.99f;
                }
            }

            // Ambient dust trail
            if (timer % 2 == 0)
            {
                Color dustColor = TerraBladeShaderManager.GetPaletteColor(Main.rand.NextFloat(0.3f, 0.7f));
                Dust d = Dust.NewDustPerfect(
                    Projectile.Center + Main.rand.NextVector2Circular(20f, 20f),
                    DustID.GreenTorch,
                    -Projectile.velocity * 0.1f,
                    0, dustColor, 1.3f);
                d.noGravity = true;
                d.fadeIn = 1.2f;
            }

            // GlowSpark particles
            if (timer % 4 == 0)
            {
                Vector2 sparkVel = -Projectile.velocity.SafeNormalize(Vector2.UnitY) * Main.rand.NextFloat(1f, 3f);
                sparkVel = sparkVel.RotatedBy(Main.rand.NextFloat(-0.6f, 0.6f));
                Color sparkColor = TerraBladeShaderManager.GetPaletteColor(Main.rand.NextFloat(0.4f, 0.8f));
                var spark = new GlowSparkParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(15f, 15f),
                    sparkVel, sparkColor, 0.25f, 18);
                MagnumParticleHandler.SpawnParticle(spark);
            }

            // Dynamic lighting
            Color light = TerraBladeShaderManager.GetPaletteColor(0.5f);
            Lighting.AddLight(Projectile.Center, light.ToVector3() * 0.9f);
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
            // Spawn LingeringFlameZone at impact point
            if (Main.myPlayer == Projectile.owner)
            {
                Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(), target.Center, Vector2.Zero,
                    ModContent.ProjectileType<LingeringFlameZone>(),
                    Projectile.damage / 2, 0f, Projectile.owner);
            }

            // Impact dust burst
            for (int i = 0; i < 15; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(8f, 8f);
                Color dustColor = TerraBladeShaderManager.GetPaletteColor(Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(target.Center, DustID.GreenTorch, vel, 0, dustColor, 1.5f);
                d.noGravity = true;
                d.fadeIn = 1.2f;
            }

            // Gold sparks
            for (int i = 0; i < 6; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(5f, 5f);
                Dust d = Dust.NewDustPerfect(target.Center, DustID.Enchanted_Gold, vel, 0, Color.White, 1.0f);
                d.noGravity = true;
                d.fadeIn = 1.2f;
            }

            // Bloom ring particles
            for (int i = 0; i < 2; i++)
            {
                float ringScale = 0.3f + i * 0.2f;
                Color ringColor = TerraBladeShaderManager.GetPaletteColor(0.4f + i * 0.2f);
                var ring = new BloomRingParticle(target.Center, Vector2.Zero, ringColor * 0.7f, ringScale, 20);
                MagnumParticleHandler.SpawnParticle(ring);
            }

            Lighting.AddLight(target.Center, 0.9f, 1.4f, 0.9f);
        }

        public override void OnKill(int timeLeft)
        {
            // Fizzle-out particles
            for (int i = 0; i < 10; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(5f, 5f);
                Color dustColor = TerraBladeShaderManager.GetPaletteColor(Main.rand.NextFloat(0.3f, 0.7f));
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.GreenTorch, vel, 0, dustColor, 1.2f);
                d.noGravity = true;
                d.fadeIn = 1.2f;
            }

            Lighting.AddLight(Projectile.Center, 0.5f, 0.7f, 0.5f);
        }

        #endregion

        #region Rendering

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float time = Main.GlobalTimeWrappedHourly;

            // 1. Cosmic ribbon trail (vertex mesh + CosmicTechnique shader)
            DrawCosmicRibbonTrail(sb);

            // 2. Noise-masked gear body (EnergyTrail + ShimmerOverlay via TerraBladeSwingVFX)
            DrawNoiseMaskedGear(sb, drawPos, time);

            // 3. Bloom glow stack
            DrawBloomGlow(sb, drawPos, time);

            return false;
        }

        // =================================================================
        // LAYER 1: Cosmic Ribbon Trail
        // =================================================================

        /// <summary>
        /// Builds a flowing ribbon mesh along the projectile's trail positions (oldPos).
        /// Creates a wide, undulating strip with gentle wave displacement for cosmic flow.
        /// </summary>
        private int BuildRibbonTrailMesh(Vector2[] trailPositions, int count,
            float widthMult, Color baseColor, float alpha, float time)
        {
            if (count < 2) return 0;

            int segments = Math.Min(count, RibbonTrailSegments);
            for (int i = 0; i < segments; i++)
            {
                float t = (float)i / (segments - 1);
                int posIdx = (int)(t * (count - 1));
                int nextIdx = Math.Min(posIdx + 1, count - 1);
                float subT = t * (count - 1) - posIdx;

                Vector2 pos = Vector2.Lerp(trailPositions[posIdx], trailPositions[nextIdx], subT);

                Vector2 dir;
                if (posIdx < count - 1)
                    dir = (trailPositions[nextIdx] - trailPositions[posIdx]).SafeNormalize(Vector2.UnitX);
                else
                    dir = (trailPositions[posIdx] - trailPositions[Math.Max(0, posIdx - 1)]).SafeNormalize(Vector2.UnitX);
                Vector2 perp = new Vector2(-dir.Y, dir.X);

                // Flowing ribbon waves for organic cosmic motion
                float wave1 = MathF.Sin(t * 10f - time * 4f) * RibbonTrailWidth * 0.12f;
                float wave2 = MathF.Sin(t * 16f + time * 3f) * RibbonTrailWidth * 0.06f;
                float totalWave = (wave1 + wave2) * widthMult;

                // Taper at head and tail
                float headFade = t < 0.05f ? t / 0.05f : 1f;
                float tailFade = t > 0.7f ? (1f - t) / 0.3f : 1f;
                float endFade = headFade * tailFade;

                float halfWidth = RibbonTrailWidth * 0.5f * widthMult * endFade;

                Vector2 center = pos + perp * totalWave - Main.screenPosition;
                Color vertColor = baseColor * (alpha * endFade);

                _ribbonVerts[i * 2] = new VertexPositionColorTexture(
                    new Vector3(center + perp * halfWidth, 0),
                    vertColor,
                    new Vector2(t, 0f));

                _ribbonVerts[i * 2 + 1] = new VertexPositionColorTexture(
                    new Vector3(center - perp * halfWidth, 0),
                    vertColor,
                    new Vector2(t, 1f));
            }

            return segments * 2;
        }

        private void DrawCosmicRibbonTrail(SpriteBatch sb)
        {
            // Gather valid trail positions
            int validCount = 0;
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] != Vector2.Zero)
                    validCount++;
                else
                    break;
            }

            if (validCount < 3) return;

            Vector2[] positions = new Vector2[validCount];
            for (int i = 0; i < validCount; i++)
            {
                positions[i] = Projectile.oldPos[i] + Projectile.Size * 0.5f;
            }

            float time = Main.GlobalTimeWrappedHourly;

            // Initialize index buffer once
            if (_ribbonIndices == null)
            {
                _ribbonIndices = new short[(RibbonTrailSegments - 1) * 6];
                for (int i = 0; i < RibbonTrailSegments - 1; i++)
                {
                    int sv = i * 2;
                    int idx = i * 6;
                    _ribbonIndices[idx + 0] = (short)sv;
                    _ribbonIndices[idx + 1] = (short)(sv + 1);
                    _ribbonIndices[idx + 2] = (short)(sv + 2);
                    _ribbonIndices[idx + 3] = (short)(sv + 2);
                    _ribbonIndices[idx + 4] = (short)(sv + 1);
                    _ribbonIndices[idx + 5] = (short)(sv + 3);
                }
            }

            var device = Main.instance.GraphicsDevice;
            Effect trailShader = ShaderLoader.Trail;

            try { sb.End(); } catch { }

            try
            {
                // Bind cosmic nebula noise texture
                Texture2D noise = ShaderLoader.GetNoiseTexture("CosmicNebulaClouds");
                if (noise == null) noise = ShaderLoader.GetNoiseTexture("TileableFBMNoise");
                if (noise != null)
                {
                    device.Textures[1] = noise;
                    device.SamplerStates[1] = SamplerState.LinearWrap;
                }

                device.BlendState = BlendState.Additive;
                device.DepthStencilState = DepthStencilState.None;
                device.RasterizerState = RasterizerState.CullNone;
                device.SamplerStates[0] = SamplerState.LinearWrap;
                device.Textures[0] = Terraria.GameContent.TextureAssets.MagicPixel.Value;

                if (trailShader != null)
                {
                    trailShader.CurrentTechnique = trailShader.Techniques["CosmicTechnique"];
                    trailShader.Parameters["uTime"]?.SetValue(time);
                    trailShader.Parameters["uColor"]?.SetValue(TerraBladeShaderManager.EnergyGreen.ToVector3());
                    trailShader.Parameters["uSecondaryColor"]?.SetValue(TerraBladeShaderManager.BrightCyan.ToVector3());
                    trailShader.Parameters["uOpacity"]?.SetValue(1f);
                    trailShader.Parameters["uProgress"]?.SetValue(0f);
                    trailShader.Parameters["uOverbrightMult"]?.SetValue(5.0f);
                    trailShader.Parameters["uGlowThreshold"]?.SetValue(0.4f);
                    trailShader.Parameters["uGlowIntensity"]?.SetValue(2f);
                    trailShader.Parameters["uHasSecondaryTex"]?.SetValue(noise != null ? 1f : 0f);
                    trailShader.Parameters["uSecondaryTexScale"]?.SetValue(1.2f);
                    trailShader.Parameters["uSecondaryTexScroll"]?.SetValue(0.6f);

                    int primitiveCount = (RibbonTrailSegments - 1) * 2;

                    // Pass 1: Wide outer nebula glow
                    int vertCount = BuildRibbonTrailMesh(positions, validCount, 2.5f,
                        TerraBladeShaderManager.GetPaletteColor(0.2f), 0.3f, time);
                    if (vertCount >= 4)
                    {
                        trailShader.Parameters["uIntensity"]?.SetValue(1.0f);
                        foreach (var p in trailShader.CurrentTechnique.Passes)
                        {
                            p.Apply();
                            device.DrawUserIndexedPrimitives(PrimitiveType.TriangleList,
                                _ribbonVerts, 0, vertCount, _ribbonIndices, 0, primitiveCount);
                        }
                    }

                    // Pass 2: Mid ribbon body
                    vertCount = BuildRibbonTrailMesh(positions, validCount, 1.5f,
                        TerraBladeShaderManager.GetPaletteColor(0.5f), 0.55f, time);
                    if (vertCount >= 4)
                    {
                        trailShader.Parameters["uIntensity"]?.SetValue(2.0f);
                        foreach (var p in trailShader.CurrentTechnique.Passes)
                        {
                            p.Apply();
                            device.DrawUserIndexedPrimitives(PrimitiveType.TriangleList,
                                _ribbonVerts, 0, vertCount, _ribbonIndices, 0, primitiveCount);
                        }
                    }

                    // Pass 3: Core ribbon
                    vertCount = BuildRibbonTrailMesh(positions, validCount, 1.0f,
                        TerraBladeShaderManager.GetPaletteColor(0.7f), 0.85f, time);
                    if (vertCount >= 4)
                    {
                        trailShader.Parameters["uIntensity"]?.SetValue(3.5f);
                        foreach (var p in trailShader.CurrentTechnique.Passes)
                        {
                            p.Apply();
                            device.DrawUserIndexedPrimitives(PrimitiveType.TriangleList,
                                _ribbonVerts, 0, vertCount, _ribbonIndices, 0, primitiveCount);
                        }
                    }

                    // Pass 4: Hot inner core
                    vertCount = BuildRibbonTrailMesh(positions, validCount, 0.3f,
                        TerraBladeShaderManager.GetPaletteColor(0.9f), 0.9f, time);
                    if (vertCount >= 4)
                    {
                        trailShader.Parameters["uIntensity"]?.SetValue(5.0f);
                        foreach (var p in trailShader.CurrentTechnique.Passes)
                        {
                            p.Apply();
                            device.DrawUserIndexedPrimitives(PrimitiveType.TriangleList,
                                _ribbonVerts, 0, vertCount, _ribbonIndices, 0, primitiveCount);
                        }
                    }
                }
            }
            finally
            {
                device.Textures[1] = null;

                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                    DepthStencilState.None, RasterizerState.CullNone, null,
                    Main.GameViewMatrix.TransformationMatrix);
            }
        }

        // =================================================================
        // LAYER 2: Noise-Masked Gear Body
        // =================================================================

        /// <summary>
        /// Draws the gear texture with scrolling noise masked onto it.
        /// Uses the VFX+ pattern: RadialScrollShader with PolarScroll technique.
        ///
        /// How it works:
        ///   1. Set causticTexture (noise) and distortTexture via shader parameters
        ///   2. Pass the Effect to sb.Begin() (NOT Immediate mode)
        ///   3. Draw the gear texture — shader reads its alpha from s0 as shape mask,
        ///      scrolls noise from causticTexture in polar coordinates through the shape
        ///   4. Output = colored noise * gear alpha (noise visible only within gear shape)
        ///
        /// Layers (VFX+ InfernoForkVFX style):
        ///   - Black backing (grounds the effect visually)
        ///   - Outer colored glow
        ///   - Shader pass: polar-scrolling noise masked to gear shape
        ///   - Second shader pass: counter-scrolling for depth
        ///   - Core glow on top
        /// </summary>
        private void DrawNoiseMaskedGear(SpriteBatch sb, Vector2 drawPos, float time)
        {
            Texture2D gearTex = ModContent.Request<Texture2D>(Texture).Value;
            if (gearTex == null) return;

            Vector2 origin = gearTex.Size() * 0.5f;
            float gearScale = 2.0f;
            float pulse = 1f + MathF.Sin(time * 6f) * 0.08f;
            float sineScale = 1f + MathF.Cos(time * 3f + timer * 0.05f) * 0.03f;

            // Get bloom texture for backing/glow layers
            Texture2D bloomTex = SandboxVFXHelper.SafeRequest("MagnumOpus/Assets/VFX/Blooms/Perfect Soft Color Bloom");
            if (bloomTex == null) bloomTex = Terraria.GameContent.TextureAssets.Extra[98].Value;
            Vector2 bloomOrigin = bloomTex.Size() * 0.5f;
            float bloomScale = gearScale * (float)Math.Max(gearTex.Width, gearTex.Height) / Math.Max(bloomTex.Width, bloomTex.Height);

            // --- Dark backing layers (VFX+ style: grounds the noise visually) ---
            sb.Draw(bloomTex, drawPos, null, Color.Black * 0.5f,
                0f, bloomOrigin, 0.45f * bloomScale * pulse, SpriteEffects.None, 0f);
            sb.Draw(bloomTex, drawPos, null, Color.Black * 0.75f,
                0f, bloomOrigin, 0.55f * bloomScale * pulse, SpriteEffects.None, 0f);

            // --- Large outer glow ---
            Color outerGlow = TerraBladeShaderManager.GetPaletteColor(0.4f);
            sb.Draw(bloomTex, drawPos, null, outerGlow with { A = 0 } * 0.2f,
                0f, bloomOrigin, 1.0f * bloomScale * pulse * sineScale, SpriteEffects.None, 0f);

            // --- Main shader pass: noise scrolling through gear shape ---
            Effect radialShader = ShaderLoader.RadialScroll;
            Texture2D noiseTex = ShaderLoader.GetNoiseTexture("TileableFBMNoise");
            Texture2D distortNoise = ShaderLoader.GetNoiseTexture("CosmicNebulaClouds");
            if (noiseTex == null) noiseTex = ShaderLoader.GetNoiseTexture("PerlinNoise");
            if (distortNoise == null) distortNoise = noiseTex;

            if (radialShader != null && noiseTex != null)
            {
                try
                {
                    // Set shader parameters (VFX+ pattern: textures via parameters, not device slots)
                    radialShader.Parameters["causticTexture"]?.SetValue(noiseTex);
                    radialShader.Parameters["distortTexture"]?.SetValue(distortNoise ?? noiseTex);
                    radialShader.Parameters["uTime"]?.SetValue(timer * -0.015f);
                    radialShader.Parameters["flowSpeed"]?.SetValue(-1.5f);
                    radialShader.Parameters["distortStrength"]?.SetValue(0.1f);
                    radialShader.Parameters["colorIntensity"]?.SetValue(1.5f);
                    radialShader.Parameters["vignetteSize"]?.SetValue(0.1f);
                    radialShader.Parameters["vignetteBlend"]?.SetValue(0.32f);
                    radialShader.Parameters["uColor"]?.SetValue(TerraBladeShaderManager.EnergyGreen.ToVector3());
                    radialShader.Parameters["uSecondaryColor"]?.SetValue(TerraBladeShaderManager.BrightCyan.ToVector3());
                    radialShader.CurrentTechnique = radialShader.Techniques["PolarScroll"];

                    // Pass Effect to sb.Begin (VFX+ pattern — NOT Immediate mode + Apply)
                    sb.End();
                    sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState,
                        DepthStencilState.None, RasterizerState.CullCounterClockwise, radialShader,
                        Main.GameViewMatrix.TransformationMatrix);

                    // Draw gear — shader reads gear alpha from s0 as shape mask,
                    // scrolls noise through it in polar coordinates
                    sb.Draw(gearTex, drawPos, null, Color.White with { A = 0 },
                        spinRotation, origin, gearScale * pulse * sineScale, SpriteEffects.None, 0f);

                    sb.End();

                    // Second pass: PolarMultiLayer with different timing for depth
                    radialShader.Parameters["uTime"]?.SetValue(timer * 0.01f);
                    radialShader.Parameters["flowSpeed"]?.SetValue(1.0f);
                    radialShader.Parameters["colorIntensity"]?.SetValue(1.0f);
                    radialShader.Parameters["vignetteSize"]?.SetValue(0.12f);
                    radialShader.Parameters["vignetteBlend"]?.SetValue(0.28f);
                    radialShader.CurrentTechnique = radialShader.Techniques["PolarMultiLayer"];

                    sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState,
                        DepthStencilState.None, RasterizerState.CullCounterClockwise, radialShader,
                        Main.GameViewMatrix.TransformationMatrix);

                    // Counter-rotating gear layer for depth
                    sb.Draw(gearTex, drawPos, null, Color.White with { A = 0 },
                        -spinRotation * 0.7f, origin, gearScale * pulse * 0.85f, SpriteEffects.None, 0f);

                    sb.End();
                    sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                        DepthStencilState.None, RasterizerState.CullNone, null,
                        Main.GameViewMatrix.TransformationMatrix);
                }
                catch
                {
                    try { sb.End(); } catch { }
                    sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                        DepthStencilState.None, RasterizerState.CullNone, null,
                        Main.GameViewMatrix.TransformationMatrix);
                }
            }
            else
            {
                // Fallback: multi-layered colored gear draws (no shader available)
                DrawFallbackGear(sb, gearTex, drawPos, origin, gearScale, pulse);
            }

            // --- Core glow on top (VFX+ style) ---
            Color coreGlow = TerraBladeShaderManager.GetPaletteColor(0.6f);
            float corePulse = 1f + MathF.Sin(time * 8f) * 0.07f;
            sb.Draw(bloomTex, drawPos, null, coreGlow with { A = 0 } * 0.9f,
                0f, bloomOrigin, 0.3f * bloomScale * corePulse, SpriteEffects.None, 0f);
        }

        /// <summary>
        /// Fallback gear rendering when RadialScrollShader is unavailable.
        /// Draws the gear texture with palette-tinted layers at different scales/rotations.
        /// </summary>
        private void DrawFallbackGear(SpriteBatch sb, Texture2D gearTex, Vector2 drawPos,
            Vector2 origin, float gearScale, float pulse)
        {
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null,
                Main.GameViewMatrix.TransformationMatrix);

            Color outerGear = TerraBladeShaderManager.GetPaletteColor(0.3f);
            sb.Draw(gearTex, drawPos, null, outerGear * 0.6f,
                spinRotation, origin, gearScale * 1.1f * pulse, SpriteEffects.None, 0f);

            Color midGear = TerraBladeShaderManager.GetPaletteColor(0.5f);
            sb.Draw(gearTex, drawPos, null, midGear * 0.8f,
                spinRotation, origin, gearScale * pulse, SpriteEffects.None, 0f);

            Color innerGear = TerraBladeShaderManager.GetPaletteColor(0.7f);
            sb.Draw(gearTex, drawPos, null, innerGear * 0.7f,
                -spinRotation * 0.7f, origin, gearScale * 0.85f * pulse, SpriteEffects.None, 0f);

            sb.Draw(gearTex, drawPos, null, Color.White * 0.4f,
                spinRotation * 0.5f, origin, gearScale * 0.4f * pulse, SpriteEffects.None, 0f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                DepthStencilState.None, RasterizerState.CullNone, null,
                Main.GameViewMatrix.TransformationMatrix);
        }

        // =================================================================
        // LAYER 3: Bloom Glow Stack
        // =================================================================

        private void DrawBloomGlow(SpriteBatch sb, Vector2 drawPos, float time)
        {
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null,
                Main.GameViewMatrix.TransformationMatrix);

            Texture2D bloomTex = SandboxVFXHelper.SafeRequest("MagnumOpus/Assets/VFX/Blooms/Perfect Soft Color Bloom");
            if (bloomTex == null)
                bloomTex = Terraria.GameContent.TextureAssets.Extra[98].Value;

            Vector2 bloomOrigin = bloomTex.Size() * 0.5f;
            float pulse = 1f + MathF.Sin(time * 8f) * 0.1f;

            // Outer bloom halo
            Color outerBloom = TerraBladeShaderManager.GetPaletteColor(0.3f);
            sb.Draw(bloomTex, drawPos, null, outerBloom * 0.3f,
                0f, bloomOrigin, 1.8f * pulse, SpriteEffects.None, 0f);

            // Mid bloom
            Color midBloom = TerraBladeShaderManager.GetPaletteColor(0.5f);
            sb.Draw(bloomTex, drawPos, null, midBloom * 0.4f,
                0f, bloomOrigin, 1.0f * pulse, SpriteEffects.None, 0f);

            // Inner bloom
            Color coreBloom = TerraBladeShaderManager.GetPaletteColor(0.8f);
            sb.Draw(bloomTex, drawPos, null, coreBloom * 0.35f,
                0f, bloomOrigin, 0.5f * pulse, SpriteEffects.None, 0f);

            // White-hot core
            sb.Draw(bloomTex, drawPos, null, Color.White * 0.3f,
                0f, bloomOrigin, 0.2f * pulse, SpriteEffects.None, 0f);

            // FlareSparkle counter-rotating at center
            Texture2D sparkleTex = SandboxVFXHelper.SafeRequest("MagnumOpus/Assets/Particles/FlareSparkle");
            if (sparkleTex != null)
            {
                Vector2 sparkleOrigin = sparkleTex.Size() * 0.5f;
                Color sparkleColor = TerraBladeShaderManager.GetPaletteColor(0.6f);
                sb.Draw(sparkleTex, drawPos, null, sparkleColor * 0.5f,
                    -time * 2.5f, sparkleOrigin, 0.35f * pulse, SpriteEffects.None, 0f);
            }

            // Restore
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
            writer.Write(spinRotation);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            cachedTargetWhoAmI = reader.ReadInt32();
            timer = reader.ReadInt32();
            spinRotation = reader.ReadSingle();
        }

        #endregion
    }
}
