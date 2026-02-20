using System;
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
    /// Laser beam connecting the LightShardProjectile's origin to a target NPC.
    /// Renders a thick sinusoidal wave beam using vertex mesh strips with
    /// NatureTechnique shader and 3-layer rendering (outer glow, main, core).
    /// Muzzle flare at origin, impact splash at target.
    /// On hit, spawns a LightShardExplosion at the target's center.
    /// </summary>
    public class LightShardBeam : ModProjectile
    {
        // =====================================================================
        //  Constants
        // =====================================================================

        private const int BeamLifetime = 30;
        private const float BeamWidth = 80f;
        private const float CollisionWidth = 40f;
        private const int BeamSegments = 50;

        // =====================================================================
        //  AI Slot Accessors
        // =====================================================================

        /// <summary>Target NPC whoAmI index.</summary>
        private ref float TargetWhoAmI => ref Projectile.ai[0];

        // =====================================================================
        //  Vertex Buffers
        // =====================================================================

        private static VertexPositionColorTexture[] _beamVerts = new VertexPositionColorTexture[BeamSegments * 2];
        private static short[] _beamIndices;

        // =====================================================================
        //  Setup
        // =====================================================================

        public override void SetDefaults()
        {
            Projectile.width = 8;
            Projectile.height = 8;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.MeleeNoSpeed;
            Projectile.penetrate = 3;
            Projectile.tileCollide = false;
            Projectile.extraUpdates = 2;
            Projectile.timeLeft = BeamLifetime * 3 + 10; // account for extraUpdates
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }

        public override string Texture => "Terraria/Images/Extra_" + 98;

        // =====================================================================
        //  AI
        // =====================================================================

        public override void AI()
        {
            // Face target
            int targetIdx = (int)TargetWhoAmI;
            if (targetIdx < 0 || targetIdx >= Main.maxNPCs || !Main.npc[targetIdx].active)
            {
                Projectile.Kill();
                return;
            }

            NPC target = Main.npc[targetIdx];
            Vector2 toTarget = target.Center - Projectile.Center;
            Projectile.rotation = toTarget.ToRotation();

            // Hold position (beam is drawn from Center to target)
            Projectile.velocity = Vector2.Zero;

            // Spark particles along beam body
            if (Main.rand.NextBool(2))
            {
                float t = Main.rand.NextFloat();
                Vector2 sparkPos = Vector2.Lerp(Projectile.Center, target.Center, t);
                Vector2 perp = toTarget.SafeNormalize(Vector2.UnitY).RotatedBy(MathHelper.PiOver2);
                sparkPos += perp * Main.rand.NextFloat(-BeamWidth * 0.4f, BeamWidth * 0.4f);

                Color sparkColor = TerraBladeShaderManager.GetPaletteColor(Main.rand.NextFloat(0.4f, 0.8f));
                Dust d = Dust.NewDustPerfect(sparkPos, DustID.GreenTorch,
                    perp * Main.rand.NextFloat(-2f, 2f), 0, sparkColor, 1.2f);
                d.noGravity = true;
            }

            // Lighting along beam
            int lightSteps = Math.Max(1, (int)(toTarget.Length() / 60f));
            for (int i = 0; i <= lightSteps; i++)
            {
                float t = (float)i / lightSteps;
                Vector2 lightPos = Vector2.Lerp(Projectile.Center, target.Center, t);
                Color lc = TerraBladeShaderManager.GetPaletteColor(0.5f);
                Lighting.AddLight(lightPos, lc.ToVector3() * 0.7f);
            }
        }

        // =====================================================================
        //  Collision (Line-based)
        // =====================================================================

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            int targetIdx = (int)TargetWhoAmI;
            if (targetIdx < 0 || targetIdx >= Main.maxNPCs || !Main.npc[targetIdx].active)
                return false;

            NPC target = Main.npc[targetIdx];
            float point = 0f;
            return Collision.CheckAABBvLineCollision(
                targetHitbox.TopLeft(), targetHitbox.Size(),
                Projectile.Center, target.Center,
                CollisionWidth * 0.5f, ref point);
        }

        // =====================================================================
        //  On Hit → Spawn Explosion
        // =====================================================================

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Main.myPlayer == Projectile.owner)
            {
                Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    target.Center,
                    Vector2.Zero,
                    ModContent.ProjectileType<LightShardExplosion>(),
                    Projectile.damage / 2,
                    0f,
                    Projectile.owner);
            }
        }

        // =====================================================================
        //  Wave Beam Mesh Building
        // =====================================================================

        /// <summary>
        /// Builds a vertex mesh strip along the beam with sinusoidal wave displacement.
        /// 3 superimposed sine waves create organic, morphing undulation.
        /// </summary>
        private int BuildWaveBeamMesh(Vector2 startWorld, Vector2 endWorld,
            float widthMult, Color baseColor, float alpha, float time)
        {
            Vector2 delta = endWorld - startWorld;
            float beamLength = delta.Length();
            if (beamLength < 1f) return 0;

            Vector2 dir = delta / beamLength;
            Vector2 perp = new Vector2(-dir.Y, dir.X);

            for (int i = 0; i < BeamSegments; i++)
            {
                float t = (float)i / (BeamSegments - 1);

                // Position along beam (world space)
                Vector2 pos = Vector2.Lerp(startWorld, endWorld, t);

                // 3 superimposed sinusoidal waves that phase in/out and morph
                float wave1 = MathF.Sin(t * 14f - time * 6f) * BeamWidth * 0.22f;
                float wave2 = MathF.Sin(t * 9f + time * 4.5f) * BeamWidth * 0.12f;
                float wave3 = MathF.Sin(t * 22f - time * 11f) * BeamWidth * 0.06f;

                // Phase modulation: waves morph over time
                float phaseMod = MathF.Sin(time * 1.5f + t * 3f) * 0.5f + 0.5f;
                float totalWave = (wave1 * (0.6f + phaseMod * 0.4f) + wave2 + wave3) * widthMult;

                // Width envelope: taper at beam endpoints
                float endFade = 1f;
                if (t < 0.08f) endFade = t / 0.08f;
                else if (t > 0.92f) endFade = (1f - t) / 0.08f;

                // Pulsing width
                float widthPulse = 1f + MathF.Sin(time * 8f + t * 6f) * 0.08f;
                float halfWidth = BeamWidth * 0.5f * widthMult * endFade * widthPulse;

                // Displaced center (screen space)
                Vector2 center = pos + perp * totalWave - Main.screenPosition;

                // Vertex color with gradient along beam
                Color vertColor = baseColor;
                vertColor *= alpha * endFade;

                // Upper vertex
                _beamVerts[i * 2] = new VertexPositionColorTexture(
                    new Vector3(center + perp * halfWidth, 0),
                    vertColor,
                    new Vector2(t, 0f));

                // Lower vertex
                _beamVerts[i * 2 + 1] = new VertexPositionColorTexture(
                    new Vector3(center - perp * halfWidth, 0),
                    vertColor,
                    new Vector2(t, 1f));
            }

            return BeamSegments * 2;
        }

        // =====================================================================
        //  Rendering
        // =====================================================================

        public override bool PreDraw(ref Color lightColor)
        {
            int targetIdx = (int)TargetWhoAmI;
            if (targetIdx < 0 || targetIdx >= Main.maxNPCs || !Main.npc[targetIdx].active)
                return false;

            SpriteBatch sb = Main.spriteBatch;
            NPC target = Main.npc[targetIdx];
            Vector2 startWorld = Projectile.Center;
            Vector2 endWorld = target.Center;
            Vector2 startScreen = startWorld - Main.screenPosition;
            Vector2 endScreen = endWorld - Main.screenPosition;
            float beamLength = Vector2.Distance(startWorld, endWorld);

            if (beamLength < 1f) return false;

            float lifetimeProgress = 1f - (Projectile.timeLeft / (float)(BeamLifetime * 3 + 10));
            float fadeAlpha = lifetimeProgress < 0.7f ? 1f : (1f - (lifetimeProgress - 0.7f) / 0.3f);
            float time = Main.GlobalTimeWrappedHourly;
            float pulse = 1f + MathF.Sin(time * 12f) * 0.06f;

            // Initialize index buffer once
            if (_beamIndices == null)
            {
                _beamIndices = new short[(BeamSegments - 1) * 6];
                for (int i = 0; i < BeamSegments - 1; i++)
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

            // ── Wave Beam Body (shader-based vertex mesh) ──
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

                // Device state for primitive rendering
                device.BlendState = BlendState.Additive;
                device.DepthStencilState = DepthStencilState.None;
                device.RasterizerState = RasterizerState.CullNone;
                device.SamplerStates[0] = SamplerState.LinearWrap;
                device.Textures[0] = Terraria.GameContent.TextureAssets.MagicPixel.Value;

                if (trailShader != null)
                {
                    // Shader-based rendering with NatureTechnique
                    trailShader.CurrentTechnique = trailShader.Techniques["NatureTechnique"];
                    trailShader.Parameters["uTime"]?.SetValue(time);
                    trailShader.Parameters["uColor"]?.SetValue(TerraBladeShaderManager.EnergyGreen.ToVector3());
                    trailShader.Parameters["uSecondaryColor"]?.SetValue(TerraBladeShaderManager.BrightCyan.ToVector3());
                    trailShader.Parameters["uOpacity"]?.SetValue(1f);
                    trailShader.Parameters["uProgress"]?.SetValue(0f);
                    trailShader.Parameters["uOverbrightMult"]?.SetValue(3.5f);
                    trailShader.Parameters["uGlowThreshold"]?.SetValue(0.4f);
                    trailShader.Parameters["uGlowIntensity"]?.SetValue(2f);
                    trailShader.Parameters["uHasSecondaryTex"]?.SetValue(noise != null ? 1f : 0f);
                    trailShader.Parameters["uSecondaryTexScale"]?.SetValue(1.5f);
                    trailShader.Parameters["uSecondaryTexScroll"]?.SetValue(0.8f);

                    int primitiveCount = (BeamSegments - 1) * 2;

                    // Pass 1: Outer glow (wide, dim)
                    int vertCount = BuildWaveBeamMesh(startWorld, endWorld, 1.8f,
                        TerraBladeShaderManager.GetPaletteColor(0.2f), fadeAlpha * 0.3f, time);
                    if (vertCount >= 4)
                    {
                        trailShader.Parameters["uIntensity"]?.SetValue(0.8f);
                        foreach (var p in trailShader.CurrentTechnique.Passes)
                        {
                            p.Apply();
                            device.DrawUserIndexedPrimitives(PrimitiveType.TriangleList,
                                _beamVerts, 0, vertCount, _beamIndices, 0, primitiveCount);
                        }
                    }

                    // Pass 2: Main beam body
                    vertCount = BuildWaveBeamMesh(startWorld, endWorld, 1.0f,
                        TerraBladeShaderManager.GetPaletteColor(0.5f), fadeAlpha * 0.65f, time);
                    if (vertCount >= 4)
                    {
                        trailShader.Parameters["uIntensity"]?.SetValue(1.4f);
                        foreach (var p in trailShader.CurrentTechnique.Passes)
                        {
                            p.Apply();
                            device.DrawUserIndexedPrimitives(PrimitiveType.TriangleList,
                                _beamVerts, 0, vertCount, _beamIndices, 0, primitiveCount);
                        }
                    }

                    // Pass 3: Hot core (narrow, bright)
                    vertCount = BuildWaveBeamMesh(startWorld, endWorld, 0.35f,
                        TerraBladeShaderManager.GetPaletteColor(0.85f), fadeAlpha * 0.9f, time);
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
                }
            }
            finally
            {
                device.Textures[1] = null;
            }

            // ── Muzzle Flare + Impact Splash (SpriteBatch) ──
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            // Muzzle flare at start
            Texture2D muzzleTex = SandboxVFXHelper.SafeRequest("MagnumOpus/Assets/VFX/Beams/Beam Muzzle Flare Origin");
            Texture2D muzzle = muzzleTex ?? Terraria.GameContent.TextureAssets.Extra[98].Value;
            Vector2 muzzleOrigin = muzzle.Size() * 0.5f;
            Color muzzleColor = TerraBladeShaderManager.GetPaletteColor(0.7f);
            float muzzleScale = 0.6f * pulse;
            sb.Draw(muzzle, startScreen, null, muzzleColor * 0.7f * fadeAlpha, time * 4f,
                muzzleOrigin, muzzleScale, SpriteEffects.None, 0f);
            sb.Draw(muzzle, startScreen, null, Color.White * 0.5f * fadeAlpha, 0f,
                muzzleOrigin, muzzleScale * 0.4f, SpriteEffects.None, 0f);

            // Impact splash at end
            Texture2D impactTex = SandboxVFXHelper.SafeRequest("MagnumOpus/Assets/VFX/Beams/Beam Impact Splash");
            Texture2D impact = impactTex ?? muzzle;
            Vector2 impactOrigin = impact.Size() * 0.5f;
            Color impactColor = TerraBladeShaderManager.GetPaletteColor(0.6f);
            float impactScale = 0.7f * pulse;
            sb.Draw(impact, endScreen, null, impactColor * 0.6f * fadeAlpha, -time * 3f,
                impactOrigin, impactScale, SpriteEffects.None, 0f);
            sb.Draw(impact, endScreen, null, Color.White * 0.45f * fadeAlpha, 0f,
                impactOrigin, impactScale * 0.35f, SpriteEffects.None, 0f);

            // Restore
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            return false;
        }
    }
}
