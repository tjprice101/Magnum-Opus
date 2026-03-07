using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using MagnumOpus.Content.LaCampanella.ResonantWeapons.GrandioseChime.Utilities;
using MagnumOpus.Content.LaCampanella.ResonantWeapons.GrandioseChime.Particles;
using MagnumOpus.Content.LaCampanella.ResonantWeapons.GrandioseChime.Primitives;
using MagnumOpus.Content.LaCampanella.ResonantWeapons.GrandioseChime.Shaders;
using MagnumOpus.Content.LaCampanella;
using MagnumOpus.Content.LaCampanella.Debuffs;
using MagnumOpus.Content.FoundationWeapons.ImpactFoundation;
using ReLogic.Content;

namespace MagnumOpus.Content.LaCampanella.ResonantWeapons.GrandioseChime.Projectiles
{
    /// <summary>
    /// Primary golden beam projectile. Fast-moving, pierces 2 enemies.
    /// On NPC kill, triggers Kill Echo Chain (propagate to nearest enemy within 15 tiles, 60% damage, 3 chains max).
    /// ai[0] = 1 for Grandiose Crescendo variant (triple width visual).
    /// </summary>
    public class GrandioseBeamProj : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftGlow";

        private List<Vector2> trailPositions = new List<Vector2>();
        private const int MaxTrailPoints = 14;
        private GrandioseChimePrimitiveRenderer trailRenderer;

        private bool IsGrandiose => Projectile.ai[0] > 0f;

        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 2;
            Projectile.timeLeft = 160;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.extraUpdates = 1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 8;
        }

        public override void AI()
        {
            trailPositions.Insert(0, Projectile.Center);
            if (trailPositions.Count > MaxTrailPoints)
                trailPositions.RemoveAt(trailPositions.Count - 1);

            Projectile.rotation = Projectile.velocity.ToRotation();
            float intensity = IsGrandiose ? 0.8f : 0.5f;
            Lighting.AddLight(Projectile.Center, GrandioseChimeUtils.BeamPalette[2].ToVector3() * intensity);

            // === Ember trail particles — fiery motes scattering from the beam ===
            if (Main.GameUpdateCount % (IsGrandiose ? 1 : 2) == 0)
            {
                Vector2 perpendicular = Projectile.velocity.SafeNormalize(Vector2.Zero).RotatedBy(MathHelper.PiOver2);
                Vector2 offset = perpendicular * Main.rand.NextFloat(-6f, 6f);
                Vector2 emberVel = -Projectile.velocity.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(0.5f, 2f)
                    + perpendicular * Main.rand.NextFloat(-1.5f, 1.5f);
                GrandioseChimeParticleHandler.SpawnParticle(
                    new BeamEmberParticle(Projectile.Center + offset, emberVel, Main.rand.Next(12, 22)));
            }

            // Occasional brighter spark
            if (Main.rand.NextBool(IsGrandiose ? 4 : 8))
            {
                Vector2 sparkVel = Main.rand.NextVector2Circular(2.5f, 2.5f);
                GrandioseChimeParticleHandler.SpawnParticle(
                    new BeamEmberParticle(Projectile.Center, sparkVel, Main.rand.Next(8, 16))
                    { Scale = Main.rand.NextFloat(0.15f, 0.3f) });
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.GetGlobalNPC<ResonantTollNPC>().AddStacks(target, IsGrandiose ? 2 : 1);

            // === FOUNDATION: RippleEffectProjectile — Beam impact zone ring ===
            Projectile.NewProjectile(
                Projectile.GetSource_FromThis(), target.Center, Vector2.Zero,
                ModContent.ProjectileType<RippleEffectProjectile>(),
                0, 0f, Projectile.owner, ai0: 1f);

            // Kill echo chain on enemy death
            if (target.life <= 0)
            {
                TriggerKillEchoChain(target.Center, damageDone, 0);
            }
        }

        private void TriggerKillEchoChain(Vector2 killPos, int killDamage, int chainDepth)
        {
            const int MaxChains = 3;
            const float ChainRange = 240f; // 15 tiles
            const float DamageScale = 0.6f;

            if (chainDepth >= MaxChains) return;

            // Spawn Kill Echo projectile that seeks nearest enemy
            int echoDmg = (int)(killDamage * DamageScale);
            if (echoDmg < 1) return;

            Projectile.NewProjectile(Projectile.GetSource_FromAI(), killPos, Vector2.Zero,
                ModContent.ProjectileType<KillEchoProj>(), echoDmg, 4f, Projectile.owner,
                ai0: ChainRange, ai1: chainDepth);

            // VFX
            GrandioseChimeParticleHandler.SpawnParticle(new KillEchoParticle(killPos, 2f, 15));
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            try
            {

            if (trailPositions.Count >= 2)
            {
                try
                {
                    trailRenderer ??= new GrandioseChimePrimitiveRenderer();
                    float trailWidth = IsGrandiose ? 28f : 10f;

                    // === SHADER-DRIVEN GRANDIOSE BEAM TRAIL ===
                    var beamShader = GrandioseChimeShaderLoader.GetBeamShader();
                    if (beamShader != null)
                    {
                        Color beamCore = GrandioseChimeUtils.BeamPalette[3];
                        Color beamEdge = GrandioseChimeUtils.BeamPalette[1];
                        beamShader.UseColor(beamCore);
                        beamShader.UseSecondaryColor(beamEdge);
                        beamShader.UseOpacity(IsGrandiose ? 0.9f : 0.7f);
                        try
                        {
                            beamShader.Shader.Parameters["uTime"]?.SetValue(Main.GameUpdateCount * 0.03f);
                            beamShader.Shader.Parameters["uIntensity"]?.SetValue(IsGrandiose ? 1.5f : 1.0f);
                            beamShader.Shader.Parameters["uOverbrightMult"]?.SetValue(IsGrandiose ? 1.8f : 1.3f);
                            beamShader.Shader.Parameters["uScrollSpeed"]?.SetValue(2.0f);
                            beamShader.Shader.Parameters["uNoiseScale"]?.SetValue(3.5f);
                            beamShader.Shader.Parameters["uPhase"]?.SetValue(IsGrandiose ? 1.0f : 0.6f);
                            beamShader.Shader.Parameters["uHasSecondaryTex"]?.SetValue(0f);
                        }
                        catch { }

                        // Bind FBM noise texture on sampler 1
                        try
                        {
                            var noiseTex = ModContent.Request<Texture2D>(
                                "MagnumOpus/Assets/VFX Asset Library/NoiseTextures/TileableFBMNoise",
                                AssetRequestMode.ImmediateLoad);
                            if (noiseTex?.Value != null)
                            {
                                Main.graphics.GraphicsDevice.Textures[1] = noiseTex.Value;
                                Main.graphics.GraphicsDevice.SamplerStates[1] = SamplerState.LinearWrap;
                                beamShader.Shader.Parameters["uHasSecondaryTex"]?.SetValue(1f);
                                beamShader.Shader.Parameters["uSecondaryTexScale"]?.SetValue(2.0f);
                            }
                        }
                        catch { }
                    }

                    float beamPulse = 1f + (float)Math.Sin(Main.GameUpdateCount * 0.15f) * 0.08f;
                    var settings = new GrandioseBeamTrailSettings
                    {
                        ColorStart = GrandioseChimeUtils.BeamPalette[3],
                        ColorEnd = GrandioseChimeUtils.BeamPalette[0] * 0.4f,
                        Width = trailWidth,
                        BloomIntensity = IsGrandiose ? 0.7f : 0.35f,
                        Shader = beamShader,
                        Smoothen = true,
                        WidthFunc = t =>
                        {
                            float w = MathHelper.Lerp(trailWidth, 2f, t);
                            return w * beamPulse;
                        },
                        ColorFunc = t =>
                        {
                            Color c = Color.Lerp(GrandioseChimeUtils.BeamPalette[3], GrandioseChimeUtils.BeamPalette[0], t);
                            float alpha = (1f - t * 0.7f) * 0.85f;
                            return c * alpha;
                        }
                    };
                    trailRenderer.DrawTrail(sb, trailPositions, settings, Main.screenPosition);

                    // === Second pass: wider outer glow trail for visual depth ===
                    if (beamShader != null)
                    {
                        try
                        {
                            beamShader.UseOpacity(0.25f);
                            beamShader.Shader.Parameters["uIntensity"]?.SetValue(0.5f);
                        }
                        catch { }
                    }
                    var outerSettings = new GrandioseBeamTrailSettings
                    {
                        ColorStart = GrandioseChimeUtils.BeamPalette[1] * 0.3f,
                        ColorEnd = Color.Transparent,
                        Width = trailWidth * 2.2f,
                        BloomIntensity = 0f,
                        Shader = beamShader,
                        Smoothen = true,
                        WidthFunc = t => MathHelper.Lerp(trailWidth * 2.2f, 4f, t),
                        ColorFunc = t =>
                        {
                            Color c = GrandioseChimeUtils.BeamPalette[1];
                            return c * ((1f - t) * 0.2f);
                        }
                    };
                    trailRenderer.DrawTrail(sb, trailPositions, outerSettings, Main.screenPosition);
                }
                catch { }
            }

            var tex = ModContent.Request<Texture2D>(Texture, AssetRequestMode.ImmediateLoad).Value;
            float pulse = 0.85f + (float)Math.Sin(Main.GameUpdateCount * 0.3f) * 0.15f;
            float coreScale = IsGrandiose ? 0.3f : 0.15f;
            Color coreColor = GrandioseChimeUtils.BeamPalette[3] * pulse;

            // Draw bloom core + LC ring in Additive
            try { sb.End(); } catch { }
            try
            {
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            // Outer ambient glow — wide soft spread
            sb.Draw(tex, Projectile.Center - Main.screenPosition, null,
                GrandioseChimeUtils.BeamPalette[0] * (0.12f * pulse), 0f, tex.Size() / 2f,
                coreScale * 4.5f, SpriteEffects.None, 0f);

            // Mid glow layer — warm orange haze
            sb.Draw(tex, Projectile.Center - Main.screenPosition, null,
                GrandioseChimeUtils.BeamPalette[1] * (0.25f * pulse), 0f, tex.Size() / 2f,
                coreScale * 2.5f, SpriteEffects.None, 0f);

            // Bright gold core
            sb.Draw(tex, Projectile.Center - Main.screenPosition, null,
                coreColor, 0f, tex.Size() / 2f, coreScale, SpriteEffects.None, 0f);

            // Hot white center pinpoint
            sb.Draw(tex, Projectile.Center - Main.screenPosition, null,
                Color.White * (0.6f * pulse * pulse), 0f, tex.Size() / 2f,
                coreScale * 0.4f, SpriteEffects.None, 0f);

            // LC Infernal Beam Ring - fiery halo around beam core
            {
                Vector2 beamScreen = Projectile.Center - Main.screenPosition;
                float ringScale = IsGrandiose ? 0.3f : 0.18f;
                float ringRot = (float)Main.GameUpdateCount * 0.04f;
                LaCampanellaVFXLibrary.DrawInfernalBeamRing(sb, beamScreen,
                    ringScale * pulse, ringRot, 0.25f * pulse,
                    LaCampanellaPalette.InfernalOrange);
            }

            // Theme texture accents
            GrandioseChimeUtils.DrawThemeAccents(sb, Projectile.Center - Main.screenPosition, Projectile.scale);
            }
            catch { }
            finally
            {
                try { sb.End(); } catch { }
                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp,
                    DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            }

            } // end outer try
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

        public override void OnKill(int timeLeft)
        {
            trailRenderer?.Dispose();
            trailRenderer = null;

            // === Death VFX — fiery impact burst ===
            Vector2 deathPos = Projectile.Center;

            // Radial ember burst (8-14 particles)
            int burstCount = IsGrandiose ? 14 : 8;
            for (int i = 0; i < burstCount; i++)
            {
                float angle = MathHelper.TwoPi * i / burstCount + Main.rand.NextFloat(-0.2f, 0.2f);
                float speed = Main.rand.NextFloat(1.5f, 4f) * (IsGrandiose ? 1.5f : 1f);
                Vector2 vel = angle.ToRotationVector2() * speed;
                GrandioseChimeParticleHandler.SpawnParticle(
                    new BeamImpactBurstParticle(deathPos, vel, Main.rand.NextFloat(0.12f, 0.25f), Main.rand.Next(15, 25)));
            }

            // Central flash ember
            GrandioseChimeParticleHandler.SpawnParticle(
                new BeamEmberParticle(deathPos, Vector2.Zero, 12) { Scale = IsGrandiose ? 0.5f : 0.3f });

            // Spawn foundation impact ripple
            Projectile.NewProjectile(
                Projectile.GetSource_FromThis(), deathPos, Vector2.Zero,
                ModContent.ProjectileType<RippleEffectProjectile>(),
                0, 0f, Projectile.owner, ai0: 1f);
        }
    }
}