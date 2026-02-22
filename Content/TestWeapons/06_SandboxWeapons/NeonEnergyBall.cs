using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.Shaders;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.VFX.Trails;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Content.TestWeapons.SandboxWeapons.Shaders;

namespace MagnumOpus.Content.TestWeapons.SandboxWeapons
{
    /// <summary>
    /// Neon green homing energy ball spawned by the Sandbox Terra Blade swing.
    /// Replaces LightShardProjectile with a more visually impressive projectile.
    ///
    /// AI Phases:
    ///   Phase 1 (0-10 frames): Brief outward deceleration
    ///   Phase 2 (11+): Homes toward nearest enemy (600f range, lerp 0.08, speed 16f)
    ///   On hit: spawns LingeringFlameZone at impact point
    ///   On kill: particle fizzle
    ///
    /// Rendering:
    ///   1. Cosmic trail via CalamityStyleTrailRenderer with CosmicNebulaClouds noise
    ///   2. Noise-textured orb body: SoftCircularCaustics at 3 counter-rotating scales
    ///   3. 4-layer bloom stack (outer → white-hot core)
    ///   4. FlareSparkle counter-rotating at center
    /// </summary>
    public class NeonEnergyBall : ModProjectile
    {
        #region Constants

        private const int TrailCacheSize = 28;
        private const int LaunchFrames = 10;
        private const float HomingRange = 600f;
        private const float HomingLerp = 0.08f;
        private const float HomingSpeed = 16f;
        private const float Deceleration = 0.94f;
        private const float BeamTrailWidth = 24f;
        private const int BeamTrailSegments = 50;

        #endregion

        #region State

        private int cachedTargetWhoAmI = -1;
        private int timer = 0;

        // Beam trail vertex buffers
        private static VertexPositionColorTexture[] _beamVerts = new VertexPositionColorTexture[BeamTrailSegments * 2];
        private static short[] _beamIndices;

        #endregion

        #region Setup

        public override string Texture => "Terraria/Images/Extra_" + 98;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = TrailCacheSize;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 18;
            Projectile.height = 18;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.MeleeNoSpeed;
            Projectile.penetrate = 1;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 200;
            Projectile.extraUpdates = 1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }

        #endregion

        #region AI

        public override void AI()
        {
            timer++;
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

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

            // Sparkle dust trail
            if (Main.rand.NextBool(2))
            {
                Color dustColor = TerraBladeShaderManager.GetPaletteColor(Main.rand.NextFloat(0.3f, 0.7f));
                Dust d = Dust.NewDustPerfect(
                    Projectile.Center + Main.rand.NextVector2Circular(6f, 6f),
                    DustID.GreenTorch,
                    -Projectile.velocity * 0.12f,
                    0, dustColor, 1.2f);
                d.noGravity = true;
                d.fadeIn = 1.2f;
            }

            // GlowSpark particles every few frames
            if (timer % 4 == 0)
            {
                Vector2 sparkVel = -Projectile.velocity.SafeNormalize(Vector2.UnitY) * Main.rand.NextFloat(1f, 3f);
                sparkVel = sparkVel.RotatedBy(Main.rand.NextFloat(-0.5f, 0.5f));
                Color sparkColor = TerraBladeShaderManager.GetPaletteColor(Main.rand.NextFloat(0.4f, 0.8f));
                var spark = new GlowSparkParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(4f, 4f),
                    sparkVel, sparkColor, 0.2f, 15);
                MagnumParticleHandler.SpawnParticle(spark);
            }

            // Dynamic lighting
            Color light = TerraBladeShaderManager.GetPaletteColor(0.5f);
            Lighting.AddLight(Projectile.Center, light.ToVector3() * 0.8f);
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

            // Dust burst
            for (int i = 0; i < 12; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(7f, 7f);
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

            // Music notes
            for (int i = 0; i < 3; i++)
            {
                Vector2 noteVel = Main.rand.NextVector2Circular(3f, 3f) + new Vector2(0f, -1.5f);
                Color noteColor = TerraBladeShaderManager.GetPaletteColor(0.3f + Main.rand.NextFloat() * 0.5f);
                ThemedParticles.MusicNote(target.Center, noteVel, noteColor, 0.9f, 35);
            }

            // Bloom ring particles
            for (int i = 0; i < 2; i++)
            {
                float ringScale = 0.25f + i * 0.15f;
                Color ringColor = TerraBladeShaderManager.GetPaletteColor(0.4f + i * 0.2f);
                var ring = new BloomRingParticle(target.Center, Vector2.Zero, ringColor * 0.6f, ringScale, 18);
                MagnumParticleHandler.SpawnParticle(ring);
            }

            Lighting.AddLight(target.Center, 0.9f, 1.4f, 0.9f);
        }

        public override void OnKill(int timeLeft)
        {
            // Fizzle-out particles
            for (int i = 0; i < 8; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(4f, 4f);
                Color dustColor = TerraBladeShaderManager.GetPaletteColor(Main.rand.NextFloat(0.3f, 0.7f));
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.GreenTorch, vel, 0, dustColor, 1.1f);
                d.noGravity = true;
                d.fadeIn = 1.2f;
            }

            Lighting.AddLight(Projectile.Center, 0.4f, 0.6f, 0.4f);
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
            SpriteBatch sb = Main.spriteBatch;
            Texture2D coreTex = Terraria.GameContent.TextureAssets.Extra[98].Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = coreTex.Size() * 0.5f;
            float time = Main.GlobalTimeWrappedHourly;
            float pulse = 1f + MathF.Sin(time * 8f) * 0.18f;

            // 1. Fluctuating beam trail (Last Prism style)
            DrawFluctuatingBeamTrail(sb);

            // 2. Motion blur
            MotionBlurBloomRenderer.DrawProjectile(
                sb, coreTex, Projectile,
                TerraBladeShaderManager.GetPaletteColor(0.5f),
                TerraBladeShaderManager.GetPaletteColor(0.8f),
                1.0f);

            // Switch to additive for all orb layers
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null,
                Main.GameViewMatrix.TransformationMatrix);

            // 3. Noise-textured orb body (SoftCircularCaustics at 3 counter-rotating scales)
            // Velocity-based stretch (Calamity pattern)
            float speed = Projectile.velocity.Length();
            float stretch = 1f + speed * 0.02f;
            float velocityRot = Projectile.velocity.ToRotation();

            Texture2D noiseTex = ShaderLoader.GetNoiseTexture("SoftCircularCaustics");
            if (noiseTex != null)
            {
                Vector2 noiseOrigin = noiseTex.Size() * 0.5f;
                float orbRadius = 32f;
                float texSize = Math.Max(noiseTex.Width, noiseTex.Height);
                float noiseScale = orbRadius * 2f / texSize;

                // Layer 1: Outer glow (slow CW rotation)
                Color layer1 = TerraBladeShaderManager.GetPaletteColor(0.3f) with { A = 0 } * 0.55f;
                sb.Draw(noiseTex, drawPos, null, layer1,
                    time * 0.8f, noiseOrigin, new Vector2(noiseScale * 1.4f * pulse * stretch, noiseScale * 1.4f * pulse / stretch), SpriteEffects.None, 0f);

                // Layer 2: Mid energy (CCW rotation)
                Color layer2 = TerraBladeShaderManager.GetPaletteColor(0.5f) with { A = 0 } * 0.70f;
                sb.Draw(noiseTex, drawPos, null, layer2,
                    -time * 1.3f, noiseOrigin, noiseScale * 1.0f * pulse, SpriteEffects.None, 0f);

                // Layer 3: Core (fast CW rotation)
                Color layer3 = TerraBladeShaderManager.GetPaletteColor(0.75f) with { A = 0 } * 0.85f;
                sb.Draw(noiseTex, drawPos, null, layer3,
                    time * 2.2f, noiseOrigin, noiseScale * 0.7f * pulse, SpriteEffects.None, 0f);
            }

            // 4. Calamity 4-layer bloom stack with { A = 0 }
            Color outerBloom = TerraBladeShaderManager.GetPaletteColor(0.3f) with { A = 0 };
            sb.Draw(coreTex, drawPos, null, outerBloom * 0.40f,
                0f, origin, 1.2f * pulse, SpriteEffects.None, 0f);

            Color midBloom = TerraBladeShaderManager.GetPaletteColor(0.5f) with { A = 0 };
            sb.Draw(coreTex, drawPos, null, midBloom * 0.65f,
                0f, origin, 0.75f * pulse, SpriteEffects.None, 0f);

            Color innerBloom = TerraBladeShaderManager.GetPaletteColor(0.7f) with { A = 0 };
            sb.Draw(coreTex, drawPos, null, innerBloom * 0.80f,
                0f, origin, 0.45f * pulse, SpriteEffects.None, 0f);

            sb.Draw(coreTex, drawPos, null, (Color.White with { A = 0 }) * 0.95f,
                0f, origin, 0.22f * pulse, SpriteEffects.None, 0f);

            // 5. FlareSparkle counter-rotating at center
            Texture2D sparkleTex = SafeRequest("MagnumOpus/Assets/Particles/FlareSparkle");
            if (sparkleTex != null)
            {
                Vector2 sparkleOrigin = sparkleTex.Size() * 0.5f;
                Color sparkleColor = TerraBladeShaderManager.GetPaletteColor(0.6f) with { A = 0 };
                sb.Draw(sparkleTex, drawPos, null, sparkleColor * 0.6f,
                    -time * 3.5f, sparkleOrigin, 0.22f * pulse, SpriteEffects.None, 0f);
                sb.Draw(sparkleTex, drawPos, null, sparkleColor * 0.3f,
                    time * 2.5f, sparkleOrigin, 0.15f * pulse, SpriteEffects.None, 0f);
            }

            // Restore
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                DepthStencilState.None, RasterizerState.CullNone, null,
                Main.GameViewMatrix.TransformationMatrix);

            return false;
        }

        /// <summary>
        /// Builds a fluctuating beam mesh along the projectile's trail positions (oldPos).
        /// Adapts LightShardBeam's sinusoidal wave pattern for trail-following geometry.
        /// </summary>
        private int BuildBeamTrailMesh(Vector2[] trailPositions, int count,
            float widthMult, Color baseColor, float alpha, float time)
        {
            if (count < 2) return 0;

            int segments = Math.Min(count, BeamTrailSegments);
            for (int i = 0; i < segments; i++)
            {
                float t = (float)i / (segments - 1);
                int posIdx = (int)(t * (count - 1));
                int nextIdx = Math.Min(posIdx + 1, count - 1);
                float subT = t * (count - 1) - posIdx;

                // Interpolated position along trail
                Vector2 pos = Vector2.Lerp(trailPositions[posIdx], trailPositions[nextIdx], subT);

                // Direction at this point (for perpendicular calculation)
                Vector2 dir;
                if (posIdx < count - 1)
                    dir = (trailPositions[nextIdx] - trailPositions[posIdx]).SafeNormalize(Vector2.UnitX);
                else
                    dir = (trailPositions[posIdx] - trailPositions[Math.Max(0, posIdx - 1)]).SafeNormalize(Vector2.UnitX);
                Vector2 perp = new Vector2(-dir.Y, dir.X);

                // 3 superimposed sinusoidal waves for organic fluctuation
                float wave1 = MathF.Sin(t * 14f - time * 6f) * BeamTrailWidth * 0.18f;
                float wave2 = MathF.Sin(t * 9f + time * 4.5f) * BeamTrailWidth * 0.10f;
                float wave3 = MathF.Sin(t * 22f - time * 11f) * BeamTrailWidth * 0.05f;

                float phaseMod = MathF.Sin(time * 1.5f + t * 3f) * 0.5f + 0.5f;
                float totalWave = (wave1 * (0.6f + phaseMod * 0.4f) + wave2 + wave3) * widthMult;

                // Taper at endpoints (head is index 0, tail fades out)
                float headFade = t < 0.05f ? t / 0.05f : 1f;
                float tailFade = t > 0.85f ? (1f - t) / 0.15f : 1f;
                float endFade = headFade * tailFade;

                // Width with pulsing
                float widthPulse = 1f + MathF.Sin(time * 8f + t * 6f) * 0.15f;
                float halfWidth = BeamTrailWidth * 0.5f * widthMult * endFade * widthPulse;

                // Displaced center (screen space)
                Vector2 center = pos + perp * totalWave - Main.screenPosition;

                // Color with trail fadeoff
                Color vertColor = baseColor * (alpha * endFade);

                _beamVerts[i * 2] = new VertexPositionColorTexture(
                    new Vector3(center + perp * halfWidth, 0),
                    vertColor,
                    new Vector2(t, 0f));

                _beamVerts[i * 2 + 1] = new VertexPositionColorTexture(
                    new Vector3(center - perp * halfWidth, 0),
                    vertColor,
                    new Vector2(t, 1f));
            }

            return segments * 2;
        }

        private void DrawFluctuatingBeamTrail(SpriteBatch sb)
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
            if (_beamIndices == null)
            {
                _beamIndices = new short[(BeamTrailSegments - 1) * 6];
                for (int i = 0; i < BeamTrailSegments - 1; i++)
                {
                    int sv = i * 2;
                    int idx = i * 6;
                    _beamIndices[idx + 0] = (short)sv;
                    _beamIndices[idx + 1] = (short)(sv + 1);
                    _beamIndices[idx + 2] = (short)(sv + 2);
                    _beamIndices[idx + 3] = (short)(sv + 2);
                    _beamIndices[idx + 4] = (short)(sv + 1);
                    _beamIndices[idx + 5] = (short)(sv + 3);
                }
            }

            var device = Main.instance.GraphicsDevice;
            Effect trailShader = ShaderLoader.Trail;

            try { sb.End(); } catch { }

            try
            {
                // Bind noise texture for organic flow
                Texture2D noise = ShaderLoader.GetNoiseTexture("TileableFBMNoise");
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
                    trailShader.CurrentTechnique = trailShader.Techniques["NatureTechnique"];
                    trailShader.Parameters["uTime"]?.SetValue(time);
                    trailShader.Parameters["uColor"]?.SetValue(TerraBladeShaderManager.EnergyGreen.ToVector3());
                    trailShader.Parameters["uSecondaryColor"]?.SetValue(TerraBladeShaderManager.BrightCyan.ToVector3());
                    trailShader.Parameters["uOpacity"]?.SetValue(1f);
                    trailShader.Parameters["uProgress"]?.SetValue(0f);
                    trailShader.Parameters["uOverbrightMult"]?.SetValue(5.0f);
                    trailShader.Parameters["uGlowThreshold"]?.SetValue(0.4f);
                    trailShader.Parameters["uGlowIntensity"]?.SetValue(2f);
                    trailShader.Parameters["uHasSecondaryTex"]?.SetValue(noise != null ? 1f : 0f);
                    trailShader.Parameters["uSecondaryTexScale"]?.SetValue(1.5f);
                    trailShader.Parameters["uSecondaryTexScroll"]?.SetValue(0.8f);

                    int primitiveCount = (BeamTrailSegments - 1) * 2;

                    // Pass 1: Outer glow (wide, dim)
                    int vertCount = BuildBeamTrailMesh(positions, validCount, 2.2f,
                        TerraBladeShaderManager.GetPaletteColor(0.2f), 0.25f, time);
                    if (vertCount >= 4)
                    {
                        trailShader.Parameters["uIntensity"]?.SetValue(0.6f);
                        foreach (var p in trailShader.CurrentTechnique.Passes)
                        {
                            p.Apply();
                            device.DrawUserIndexedPrimitives(PrimitiveType.TriangleList,
                                _beamVerts, 0, vertCount, _beamIndices, 0, primitiveCount);
                        }
                    }

                    // Pass 2: Mid energy
                    vertCount = BuildBeamTrailMesh(positions, validCount, 1.4f,
                        TerraBladeShaderManager.GetPaletteColor(0.5f), 0.5f, time);
                    if (vertCount >= 4)
                    {
                        trailShader.Parameters["uIntensity"]?.SetValue(1.2f);
                        foreach (var p in trailShader.CurrentTechnique.Passes)
                        {
                            p.Apply();
                            device.DrawUserIndexedPrimitives(PrimitiveType.TriangleList,
                                _beamVerts, 0, vertCount, _beamIndices, 0, primitiveCount);
                        }
                    }

                    // Pass 3: Main body
                    vertCount = BuildBeamTrailMesh(positions, validCount, 1.0f,
                        TerraBladeShaderManager.GetPaletteColor(0.65f), 0.85f, time);
                    if (vertCount >= 4)
                    {
                        trailShader.Parameters["uIntensity"]?.SetValue(2.0f);
                        foreach (var p in trailShader.CurrentTechnique.Passes)
                        {
                            p.Apply();
                            device.DrawUserIndexedPrimitives(PrimitiveType.TriangleList,
                                _beamVerts, 0, vertCount, _beamIndices, 0, primitiveCount);
                        }
                    }

                    // Pass 4: Hot core (narrow, bright)
                    vertCount = BuildBeamTrailMesh(positions, validCount, 0.35f,
                        TerraBladeShaderManager.GetPaletteColor(0.85f), 0.9f, time);
                    if (vertCount >= 4)
                    {
                        trailShader.Parameters["uIntensity"]?.SetValue(2.5f);
                        foreach (var p in trailShader.CurrentTechnique.Passes)
                        {
                            p.Apply();
                            device.DrawUserIndexedPrimitives(PrimitiveType.TriangleList,
                                _beamVerts, 0, vertCount, _beamIndices, 0, primitiveCount);
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
}
