using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Graphics;
using Terraria.ModLoader;
using MagnumOpus.Content.Fate.Debuffs;
using ReLogic.Content;

namespace MagnumOpus.Content.Fate.ResonantWeapons.DestinysCrescendo
{
    /// <summary>
    /// Cosmic beam fired by the CrescendoDeityMinion.
    /// 3 penetrate, 90 life, gradient trail, multi-layer bloom PreDraw.
    /// Applies DestinyCollapse debuff on hit.
    /// 
    /// Texture: MagnumOpus/Assets/Particles Asset Library/Stars/4PointedStarHard
    /// ZERO shared VFX system references.
    /// </summary>
    public class CrescendoCosmicBeam : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles Asset Library/Stars/4PointedStarHard";

        private float pulsePhase = 0f;

        // --- InfernalBeamFoundation scaffolding: shader + texture caching ---
        private static Effect _beamShader;
        private static Asset<Texture2D> _beamAlphaMask;
        private static Asset<Texture2D> _gradientLUT;
        private static Asset<Texture2D> _bodyTex;
        private static Asset<Texture2D> _detailTex1;
        private static Asset<Texture2D> _detailTex2;
        private static Asset<Texture2D> _noiseTex;
        private static Asset<Texture2D> _softGlow;
        private static Asset<Texture2D> _pointBloom;
        private VertexStrip _strip;

        /// <summary>Escalation Phase (0-3) passed from CrescendoDeityMinion via ai[0].</summary>
        private int Phase => (int)Projectile.ai[0];

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 25;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.penetrate = 3;
            Projectile.timeLeft = 90;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 15;
        }

        /// <summary>Scale penetration with phase after defaults are set (called from AI first tick).</summary>
        private bool _phaseApplied = false;
        private void ApplyPhaseScaling()
        {
            if (_phaseApplied) return;
            _phaseApplied = true;
            // Phase 0: 3 pen, Phase 1: 4, Phase 2: 5, Phase 3: 7
            Projectile.penetrate += Phase;
            if (Phase >= 3) Projectile.penetrate += 1;
            // Wider hitbox at higher phases
            int sizeBonus = Phase * 4;
            Projectile.width += sizeBonus;
            Projectile.height += sizeBonus;
        }

        public override void AI()
        {
            ApplyPhaseScaling();

            Projectile.rotation = Projectile.velocity.ToRotation();
            pulsePhase += 0.18f;

            if (Main.dedServ) return;

            float phaseIntensity = 1f + Phase * 0.35f; // VFX density multiplier

            // === COSMIC BEAM TRAIL VFX (scaled with phase) ===

            // Gradient glow particles trailing behind the beam
            if (Main.rand.NextFloat() < 0.5f * phaseIntensity)
            {
                Color trailColor = CrescendoUtils.GetCrescendoGradient(Main.rand.NextFloat(0.3f, 0.8f));
                Vector2 vel = -Projectile.velocity * 0.08f + Main.rand.NextVector2Circular(1f, 1f);
                CrescendoParticleHandler.Spawn(CrescendoParticleFactory.OrbGlow(Projectile.Center, vel, trailColor * (0.5f + Phase * 0.1f), 0.16f + Phase * 0.04f, 14));
            }

            // Star sparkles in trail 遯ｶ繝ｻmore at higher phases
            if (Main.rand.NextFloat() < 0.33f * phaseIntensity)
            {
                Vector2 sparkPos = Projectile.Center + Main.rand.NextVector2Circular(12f + Phase * 4f, 12f + Phase * 4f);
                Color sparkCol = Main.rand.NextBool(2) ? CrescendoUtils.StarGold : CrescendoUtils.CelestialWhite;
                CrescendoParticleHandler.Spawn(CrescendoParticleFactory.DivineSpark(sparkPos,
                    Main.rand.NextVector2Circular(1.5f, 1.5f), sparkCol * (0.4f + Phase * 0.1f), 0.12f + Phase * 0.03f, 12));
            }

            // Cosmic cloud wisps along beam path
            if (Main.rand.NextFloat() < 0.33f * phaseIntensity * 0.4f)
            {
                CrescendoParticleFactory.SpawnAuraWisps(Projectile.Center, 1, 10f + Phase * 3f);
            }

            // Glyph accents 遯ｶ繝ｻmore frequent at higher phases
            if (Main.rand.NextFloat() < (0.125f + Phase * 0.05f))
            {
                CrescendoParticleHandler.Spawn(CrescendoParticleFactory.GlyphCircle(Projectile.Center, CrescendoUtils.DeityPurple * (0.5f + Phase * 0.1f), 0.2f + Phase * 0.05f, 16));
            }

            // Cosmic music notes 遯ｶ繝ｻthe beam sings louder at higher phases
            if (Main.rand.NextFloat() < (0.17f + Phase * 0.06f))
            {
                CrescendoParticleFactory.SpawnCosmicNotes(Projectile.Center, 1, 8f + Phase * 2f);
            }

            Lighting.AddLight(Projectile.Center, CrescendoUtils.CelestialWhite.ToVector3() * (1.3f + Phase * 0.3f));
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<DestinyCollapse>(), 180);

            // Enhanced impact burst
            CrescendoParticleHandler.SpawnBurst(target.Center, 10, 5f, 0.2f, CrescendoUtils.DivineCrimson, CrescendoParticleType.DivineSpark, 16);
            CrescendoParticleHandler.SpawnBurst(target.Center, 4, 3f, 0.25f, CrescendoUtils.StarGold, CrescendoParticleType.GlyphCircle, 20);
            CrescendoParticleFactory.SpawnCosmicNotes(target.Center, 3, 18f);

            // Flash
            CrescendoParticleHandler.Spawn(CrescendoParticleFactory.BeamFlare(target.Center, Vector2.Zero, CrescendoUtils.CelestialWhite, 0.5f, 12));
        }

        public override void OnKill(int timeLeft)
        {
            if (Main.dedServ) return;

            // Death explosion
            CrescendoParticleHandler.SpawnBurst(Projectile.Center, 12, 6f, 0.22f, CrescendoUtils.CrescendoPink, CrescendoParticleType.DivineSpark, 18);
            CrescendoParticleHandler.SpawnBurst(Projectile.Center, 5, 3f, 0.3f, CrescendoUtils.StarGold, CrescendoParticleType.GlyphCircle, 22);
            CrescendoParticleFactory.SpawnCosmicNotes(Projectile.Center, 4, 20f);

            // Central flash
            CrescendoParticleHandler.Spawn(CrescendoParticleFactory.BeamFlare(Projectile.Center, Vector2.Zero, CrescendoUtils.CelestialWhite, 0.6f, 14));

            SoundEngine.PlaySound(SoundID.Item10 with { Pitch = 0.5f, Volume = 0.5f }, Projectile.Center);
        }

        private void LoadBeamTextures()
        {
            const string ThemeBeams = "MagnumOpus/Assets/VFX Asset Library/Theme Specific/Fate/Beam Textures/";
            const string Beams = "MagnumOpus/Assets/VFX Asset Library/BeamTextures/";
            const string Trails = "MagnumOpus/Assets/VFX Asset Library/TrailsAndRibbons/";
            const string Bloom = "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/";
            const string Noise = "MagnumOpus/Assets/VFX Asset Library/NoiseTextures/";
            const string Gradients = "MagnumOpus/Assets/VFX Asset Library/ColorGradients/";

            _beamAlphaMask ??= ModContent.Request<Texture2D>(Trails + "BasicTrail", AssetRequestMode.ImmediateLoad);
            _gradientLUT ??= ModContent.Request<Texture2D>(Gradients + "FateGradientLUTandRAMP", AssetRequestMode.ImmediateLoad);
            _bodyTex ??= ModContent.Request<Texture2D>(Beams + "SoundWaveBeam", AssetRequestMode.ImmediateLoad);
            _detailTex1 ??= ModContent.Request<Texture2D>(Beams + "EnergyMotion", AssetRequestMode.ImmediateLoad);
            _detailTex2 ??= ModContent.Request<Texture2D>(ThemeBeams + "FA Energy Surge Beam", AssetRequestMode.ImmediateLoad);
            _noiseTex ??= ModContent.Request<Texture2D>(Noise + "TileableFBMNoise", AssetRequestMode.ImmediateLoad);
            _softGlow ??= ModContent.Request<Texture2D>(Bloom + "SoftGlow", AssetRequestMode.ImmediateLoad);
            _pointBloom ??= ModContent.Request<Texture2D>(Bloom + "PointBloom", AssetRequestMode.ImmediateLoad);
        }

        // ═══════════════════════════════════════════════════════════════════
        //  PREDRAW — INFERNAL BEAM FOUNDATION SHADER TRAIL + MULTI-LAYER BLOOM
        // ═══════════════════════════════════════════════════════════════════

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;

            try
            {
                LoadBeamTextures();

                float pulse = 1f + MathF.Sin(pulsePhase) * 0.15f;
                float phaseScale = 1f + Phase * 0.2f;
                float phaseGlow = 1f + Phase * 0.15f;
                float beamWidth = MathHelper.Lerp(20f, 35f, Phase / 3f) * pulse;

                // Build VertexStrip from trail cache (oldPos[0] = newest/head)
                int count = 0;
                for (int i = 0; i < Projectile.oldPos.Length; i++)
                {
                    if (Projectile.oldPos[i] == Vector2.Zero) break;
                    count++;
                }

                spriteBatch.End();

                // === LAYER 1: Shader-driven beam body via VertexStrip (InfernalBeamFoundation) ===
                if (count >= 2)
                {
                    Vector2[] positions = new Vector2[count];
                    float[] rotations = new float[count];
                    float totalLength = 0f;

                    for (int i = 0; i < count; i++)
                    {
                        positions[i] = Projectile.oldPos[i] + Projectile.Size / 2f;
                        rotations[i] = Projectile.oldRot[i];
                        if (i > 0) totalLength += Vector2.Distance(positions[i - 1], positions[i]);
                    }

                    _strip ??= new VertexStrip();
                    _strip.PrepareStrip(positions, rotations,
                        (float progress) => Color.White * (1f - progress * 0.8f) * phaseGlow,
                        (float progress) => MathHelper.Lerp(beamWidth, 3f, progress),
                        -Main.screenPosition, includeBacksides: true);

                    _beamShader ??= ModContent.Request<Effect>(
                        "MagnumOpus/Content/FoundationWeapons/InfernalBeamFoundation/Shaders/InfernalBeamBodyShader",
                        AssetRequestMode.ImmediateLoad).Value;

                    if (_beamShader != null)
                    {
                        float repVal = MathHelper.Max(totalLength / 600f, 0.3f);
                        float time = (float)Main.timeForVisualEffects * -0.028f;

                        _beamShader.Parameters["WorldViewProjection"].SetValue(
                            Main.GameViewMatrix.NormalizedTransformationmatrix);
                        _beamShader.Parameters["onTex"].SetValue(_beamAlphaMask.Value);
                        _beamShader.Parameters["gradientTex"].SetValue(_gradientLUT.Value);
                        _beamShader.Parameters["bodyTex"].SetValue(_bodyTex.Value);
                        _beamShader.Parameters["detailTex1"].SetValue(_detailTex1.Value);
                        _beamShader.Parameters["detailTex2"].SetValue(_detailTex2.Value);
                        _beamShader.Parameters["noiseTex"].SetValue(_noiseTex.Value);

                        _beamShader.Parameters["bodyReps"].SetValue(1.8f * repVal);
                        _beamShader.Parameters["detail1Reps"].SetValue(2.2f * repVal);
                        _beamShader.Parameters["detail2Reps"].SetValue(1.4f * repVal);
                        _beamShader.Parameters["gradientReps"].SetValue(0.9f * repVal);
                        _beamShader.Parameters["bodyScrollSpeed"].SetValue(1.0f);
                        _beamShader.Parameters["detail1ScrollSpeed"].SetValue(1.4f);
                        _beamShader.Parameters["detail2ScrollSpeed"].SetValue(-0.7f);
                        _beamShader.Parameters["noiseDistortion"].SetValue(0.04f);
                        _beamShader.Parameters["totalMult"].SetValue(1.2f + Phase * 0.2f);
                        _beamShader.Parameters["uTime"].SetValue(time);

                        _beamShader.CurrentTechnique.Passes["MainPS"].Apply();
                        _strip.DrawTrail();
                        Main.pixelShader.CurrentTechnique.Passes[0].Apply();
                    }
                }

                // === LAYER 2: Multi-layer additive bloom (Fate palette, phase-scaled) ===
                spriteBatch.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                    SamplerState.LinearClamp, DepthStencilState.None,
                    RasterizerState.CullCounterClockwise, null,
                    Main.GameViewMatrix.TransformationMatrix);

                Vector2 drawPos = Projectile.Center - Main.screenPosition;
                Texture2D glowTex = _softGlow?.Value;
                Texture2D bloomTex = _pointBloom?.Value;

                if (glowTex != null && bloomTex != null)
                {
                    // Layer 1: Deity purple outer glow
                    spriteBatch.Draw(glowTex, drawPos, null, CrescendoUtils.DeityPurple * (0.35f * phaseGlow),
                        0f, glowTex.Size() / 2f, 0.4f * phaseScale, SpriteEffects.None, 0f);
                    // Layer 2: Divine crimson mid bloom
                    spriteBatch.Draw(bloomTex, drawPos, null, CrescendoUtils.DivineCrimson * (0.45f * phaseGlow),
                        0f, bloomTex.Size() / 2f, 0.2f * phaseScale, SpriteEffects.None, 0f);
                    // Layer 3: Crescendo pink inner
                    spriteBatch.Draw(bloomTex, drawPos, null, CrescendoUtils.CrescendoPink * (0.55f * phaseGlow),
                        0f, bloomTex.Size() / 2f, 0.12f * phaseScale, SpriteEffects.None, 0f);
                    // Layer 4: Celestial white hot core
                    spriteBatch.Draw(bloomTex, drawPos, null, CrescendoUtils.CelestialWhite * (0.8f * phaseGlow),
                        0f, bloomTex.Size() / 2f, 0.06f * phaseScale, SpriteEffects.None, 0f);
                }

                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                    SamplerState.LinearClamp, DepthStencilState.None,
                    Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            }
            catch
            {
                try
                {
                    spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                        DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
                }
                catch { }
            }

            return false;
        }
    }
}