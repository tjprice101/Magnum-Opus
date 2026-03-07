using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.BaseClasses;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Shaders;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.VFX.Trails;
using MagnumOpus.Content.EnigmaVariations;
using MagnumOpus.Content.EnigmaVariations.Debuffs;
using MagnumOpus.Content.EnigmaVariations.ResonantWeapons.TheSilentMeasure;
using MagnumOpus.Content.EnigmaVariations.ResonantWeapons.TheUnresolvedCadence.Particles;
using MagnumOpus.Content.EnigmaVariations.ResonantWeapons.TheUnresolvedCadence.Dusts;
using MagnumOpus.Content.EnigmaVariations.ResonantWeapons.TheUnresolvedCadence.Primitives;
using MagnumOpus.Content.EnigmaVariations.ResonantWeapons.TheUnresolvedCadence.Utilities;
using static MagnumOpus.Common.Systems.Particles.Particle;
using ReLogic.Content;

namespace MagnumOpus.Content.EnigmaVariations.ResonantWeapons.TheUnresolvedCadence
{
    /// <summary>
    /// THE UNRESOLVED CADENCE 窶・Swing projectile (held-projectile combo).
    /// 3-phase combo: The Question 竊・The Doubt 竊・The Silence
    /// Each swing builds Inevitability. At 10 stacks, Paradox Collapse tears reality.
    /// Glitch aesthetic: chromatic aberration, scan-line flickers, dimensional tears.
    /// </summary>
    public sealed class TheUnresolvedCadenceSwing : MeleeSwingBase
    {
        #region Theme Colors & Trail Tracking

        private static readonly Color EnigmaBlack = MagnumThemePalettes.EnigmaBlack;
        private static readonly Color EnigmaPurple = MagnumThemePalettes.EnigmaPurple;
        private static readonly Color EnigmaGreen = MagnumThemePalettes.EnigmaGreen;
        private static readonly Color EnigmaDeepPurple = MagnumThemePalettes.EnigmaDeepPurple;
        private static readonly Color EnigmaVoid = MagnumThemePalettes.EnigmaVoid;

        // GPU trail tracking — stores blade tip positions each frame for shader-driven primitive rendering
        private readonly List<Vector2> _tipTrailBuffer = new(40);
        private float _lastSwingIntensity = 0f;

        private static readonly Color[] EnigmaPalette = new Color[]
        {
            MagnumThemePalettes.EnigmaBlack,      // [0] Pianissimo 窶・void darkness
            MagnumThemePalettes.EnigmaDeepPurple,  // [1] Piano 窶・deep arcane
            MagnumThemePalettes.EnigmaPurple,      // [2] Mezzo 窶・enigma purple
            new Color(100, 140, 200),              // [3] Forte 窶・transitional
            MagnumThemePalettes.EnigmaGreen,       // [4] Fortissimo 窶・eerie green
            new Color(180, 255, 180),              // [5] Sforzando 窶・bright green-white
        };

        #endregion

        #region Combo Phases

        // Phase 0: The Question 窶・Quick, probing diagonal slash
        private static readonly ComboPhase Phase0_TheQuestion = new ComboPhase(
            new CurveSegment[]
            {
                new CurveSegment(EasingType.PolyOut, 0f, -0.9f, 0.2f, 2),
                new CurveSegment(EasingType.PolyIn, 0.2f, -0.7f, 1.5f, 3),
                new CurveSegment(EasingType.PolyOut, 0.8f, 0.8f, 0.1f, 2),
            },
            maxAngle: MathHelper.PiOver2 * 1.4f,
            duration: 18,
            bladeLength: 150f,
            flip: false,
            squish: 0.92f,
            damageMult: 0.85f
        );

        // Phase 1: The Doubt 窶・Cross-slash (X pattern), reversed arc
        private static readonly ComboPhase Phase1_TheDoubt = new ComboPhase(
            new CurveSegment[]
            {
                new CurveSegment(EasingType.PolyOut, 0f, -1f, 0.3f, 2),
                new CurveSegment(EasingType.PolyIn, 0.25f, -0.7f, 1.55f, 3),
                new CurveSegment(EasingType.PolyOut, 0.85f, 0.85f, 0.12f, 2),
            },
            maxAngle: MathHelper.PiOver2 * 1.6f,
            duration: 20,
            bladeLength: 155f,
            flip: true,
            squish: 0.9f,
            damageMult: 1.0f
        );

        // Phase 2: The Silence 窶・Heavy downward slam, tears space
        private static readonly ComboPhase Phase2_TheSilence = new ComboPhase(
            new CurveSegment[]
            {
                new CurveSegment(EasingType.PolyOut, 0f, -1.1f, 0.3f, 2),
                new CurveSegment(EasingType.PolyIn, 0.2f, -0.8f, 1.9f, 3),
                new CurveSegment(EasingType.PolyOut, 0.82f, 1.1f, 0.1f, 2),
            },
            maxAngle: MathHelper.PiOver2 * 2.0f,
            duration: 25,
            bladeLength: 168f,
            flip: false,
            squish: 0.85f,
            damageMult: 1.3f
        );

        #endregion

        #region Abstract Overrides

        protected override ComboPhase[] GetAllPhases() => new[]
        {
            Phase0_TheQuestion,
            Phase1_TheDoubt,
            Phase2_TheSilence,
        };

        protected override Color[] GetPalette() => EnigmaPalette;

        protected override CalamityStyleTrailRenderer.TrailStyle GetTrailStyle()
            => CalamityStyleTrailRenderer.TrailStyle.Cosmic;

        protected override string GetSmearTexturePath(int comboStep)
        {
            return comboStep switch
            {
                0 => "MagnumOpus/Assets/VFX Asset Library/SlashArcs/SwordArcSmear",
                1 => "MagnumOpus/Assets/VFX Asset Library/SlashArcs/FlamingSwordArcSmear",
                2 => "MagnumOpus/Assets/VFX Asset Library/SlashArcs/FullCircleSwordArcSlash",
                _ => "MagnumOpus/Assets/VFX Asset Library/ImpactEffects/ImpactEllipse",
            };
        }

        protected override string GetSmearGradientPath() => "MagnumOpus/Assets/VFX Asset Library/ColorGradients/EnigmaGradientLUTandRAMP";

        #endregion

        #region Virtual Overrides

        protected override Texture2D GetBladeTexture()
        {
            if (ModContent.HasAsset("MagnumOpus/Content/EnigmaVariations/ResonantWeapons/TheUnresolvedCadence/TheUnresolvedCadence"))
                return ModContent.Request<Texture2D>("MagnumOpus/Content/EnigmaVariations/ResonantWeapons/TheUnresolvedCadence/TheUnresolvedCadence", AssetRequestMode.ImmediateLoad).Value;
            return base.GetBladeTexture();
        }

        protected override SoundStyle GetSwingSound()
            => SoundID.Item71 with { Pitch = 0.2f + ComboStep * 0.15f, Volume = 0.8f };

        protected override int GetInitialDustType() => DustID.PurpleTorch;

        protected override int GetSecondaryDustType() => DustID.CursedTorch;

        protected override Vector3 GetLightColor()
        {
            float intensity = 0.55f + ComboStep * 0.15f;
            Color c = Color.Lerp(EnigmaPurple, EnigmaGreen, ComboStep / 3f);
            return c.ToVector3() * intensity;
        }

        #endregion

        #region Trail Tracking Overrides

        protected override void InitializeSwing()
        {
            base.InitializeSwing();
            _tipTrailBuffer.Clear();
            _lastSwingIntensity = 0f;
        }

        protected override void DoBehavior_Swinging()
        {
            base.DoBehavior_Swinging();
            // Record blade tip every frame for GPU primitive trail rendering
            Vector2 tip = GetBladeTipPosition();
            _tipTrailBuffer.Add(tip);
            if (_tipTrailBuffer.Count > 35) _tipTrailBuffer.RemoveAt(0);

            // Smooth intensity ramp for VFX
            float targetIntensity = 0.3f + ComboStep * 0.2f + TheUnresolvedCadenceItem.GetInevitabilityStacks() * 0.04f;
            _lastSwingIntensity = MathHelper.Lerp(_lastSwingIntensity, targetIntensity, 0.1f);
        }

        #endregion

        #region Combo Specials

        protected override void HandleComboSpecials()
        {
            if (hasSpawnedSpecial) return;

            // Phase 0 at 55% 窶・no gameplay effect (was VFX only)
            if (ComboStep == 0 && Progression >= 0.55f)
            {
                hasSpawnedSpecial = true;
            }

            // Phase 1 at 60% 窶・dimensional slash sub-projectile
            if (ComboStep == 1 && Progression >= 0.60f)
            {
                hasSpawnedSpecial = true;

                if (Main.myPlayer == Projectile.owner)
                {
                    Vector2 tipPos = GetBladeTipPosition();
                    Vector2 slashVel = SwordDirection * 12f;
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), tipPos, slashVel,
                        ModContent.ProjectileType<DimensionalSlash>(),
                        (int)(Projectile.damage * 0.35f), 2f, Projectile.owner,
                        ai0: Projectile.rotation);
                }
            }

            // Phase 2 at 50% 窶・massive dimensional tear (finisher)
            if (ComboStep == 2 && Progression >= 0.50f)
            {
                hasSpawnedSpecial = true;

                if (Main.myPlayer == Projectile.owner)
                {
                    Vector2 tipPos = GetBladeTipPosition();

                    // Forward slash
                    Vector2 slashVel = SwordDirection * 15f;
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), tipPos, slashVel,
                        ModContent.ProjectileType<DimensionalSlash>(),
                        (int)(Projectile.damage * 0.5f), 3f, Projectile.owner,
                        ai0: Projectile.rotation);

                    // Perpendicular slashes
                    for (int side = -1; side <= 1; side += 2)
                    {
                        Vector2 perpVel = SwordDirection.RotatedBy(MathHelper.PiOver2 * side) * 10f;
                        Projectile.NewProjectile(Projectile.GetSource_FromThis(), tipPos, perpVel,
                            ModContent.ProjectileType<DimensionalSlash>(),
                            (int)(Projectile.damage * 0.25f), 2f, Projectile.owner,
                            ai0: Projectile.rotation + MathHelper.PiOver2 * side);
                    }

                    // Homing question seekers
                    for (int i = 0; i < 4; i++)
                    {
                        float angle = MathHelper.TwoPi * i / 4f + Main.rand.NextFloat(-0.3f, 0.3f);
                        Vector2 seekerVel = angle.ToRotationVector2() * Main.rand.NextFloat(6f, 10f);
                        Projectile.NewProjectile(Projectile.GetSource_FromThis(), tipPos, seekerVel,
                            ModContent.ProjectileType<HomingQuestionSeeker>(),
                            Projectile.damage / 4, 1f, Projectile.owner);
                    }
                }
            }
        }

        #endregion

        #region On Hit

        protected override void OnSwingHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // ParadoxBrand debuff 窶・8 second duration per doc
            target.AddBuff(ModContent.BuffType<ParadoxBrand>(), 480);
            var brandNPC = target.GetGlobalNPC<ParadoxBrandNPC>();
            brandNPC.AddParadoxStack(target, hit.Crit ? 5 : 3);

            // Seeking crystals on crit
            if (hit.Crit && Main.myPlayer == Projectile.owner)
            {
                for (int i = 0; i < 3; i++)
                {
                    Vector2 seekerVel = Main.rand.NextVector2CircularEdge(8f, 8f);
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), target.Center, seekerVel,
                        ModContent.ProjectileType<HomingQuestionSeeker>(),
                        Projectile.damage / 4, 1f, Projectile.owner);
                }
            }

            Lighting.AddLight(target.Center, EnigmaPurple.ToVector3() * 0.9f);

            // Impact VFX
            if (!Main.dedServ)
            {
                CadenceParticleHandler.Spawn(new ParadoxSlashRipple(
                    target.Center, CadenceUtils.CadenceViolet, 0.25f, 25));
                for (int j = 0; j < Main.rand.Next(3, 6); j++)
                {
                    Vector2 burstVel = Main.rand.NextVector2CircularEdge(4f, 4f) * Main.rand.NextFloat(0.5f, 1.5f);
                    CadenceParticleHandler.Spawn(new DimensionalRiftMote(
                        target.Center, burstVel, Main.rand.NextFloat(0.2f, 0.4f), Main.rand.Next(12, 22)));
                }
                // Inevitability glyph on stack increment
                float glyphAngle = Main.rand.NextFloat(MathHelper.TwoPi);
                CadenceParticleHandler.Spawn(new InevitabilityGlyphParticle(
                    target.Center, 35f, glyphAngle,
                    TheUnresolvedCadenceItem.GetInevitabilityStacks(),
                    CadenceUtils.CadenceViolet, 0.35f, 30));
            }
        }

        #endregion

        #region Custom VFX — Full Shader-Driven Pipeline

        protected override void DrawCustomVFX(SpriteBatch sb)
        {
            if (Main.dedServ) return;

            Vector2 tipPos = GetBladeTipPosition();
            int timer = (int)Timer;
            int stacks = TheUnresolvedCadenceItem.GetInevitabilityStacks();
            float stackIntensity = MathHelper.Clamp(stacks / 10f, 0f, 1f);
            float prog = Progression;

            // ═══════════════════════════════════════════════════════
            // LAYER 1: GPU PRIMITIVE TRAIL — CadenceSwingTrail shader
            // Dimensional crack energy rendered as a GPU vertex mesh
            // trailing the blade tip, driven by CadenceSwingTrail.fx
            // ═══════════════════════════════════════════════════════
            if (_tipTrailBuffer.Count >= 3 && prog > 0.05f && prog < 0.95f)
            {
                Effect swingShader = ShaderLoader.CadenceSwingTrail;
                if (swingShader != null)
                {
                    sb.End();

                    var device = Main.graphics.GraphicsDevice;
                    var prevBlend = device.BlendState;
                    device.BlendState = MagnumBlendStates.TrueAdditive;

                    // Bind Voronoi noise for dimensional cracks
                    Texture2D noiseTex = ShaderLoader.GetNoiseTexture("VoronoiNoise");
                    if (noiseTex != null)
                        device.Textures[1] = noiseTex;

                    float time = (float)Main.GameUpdateCount * 0.04f;
                    float phaseBoost = ComboStep * 0.15f;
                    float stackBoost = stackIntensity * 0.25f;

                    swingShader.Parameters["uTime"]?.SetValue(time);
                    swingShader.Parameters["uColor"]?.SetValue(CadenceUtils.CadenceViolet.ToVector3());
                    swingShader.Parameters["uSecondaryColor"]?.SetValue(CadenceUtils.DimensionalGreen.ToVector3());

                    // PASS A: Body trail (CadenceSwingFlow) — jagged dimensional cracks
                    swingShader.Parameters["uOpacity"]?.SetValue(0.55f + phaseBoost + stackBoost);
                    swingShader.Parameters["uIntensity"]?.SetValue(1.1f + phaseBoost + stackBoost);
                    swingShader.CurrentTechnique = swingShader.Techniques["CadenceSwingFlow"];

                    float bodyWidth = 26f + ComboStep * 10f + stackIntensity * 8f;
                    CadencePrimitiveRenderer.RenderTrail(_tipTrailBuffer, new CadencePrimitiveSettings(
                        completion => (1f - completion * 0.65f) * bodyWidth,
                        completion =>
                        {
                            Color c = Color.Lerp(CadenceUtils.DimensionalGreen, CadenceUtils.CadenceViolet, completion);
                            return c * (1f - completion * 0.5f);
                        },
                        swingShader
                    ));

                    // PASS B: Glow trail (CadenceSwingGlow) — soft bloom underlayer
                    swingShader.Parameters["uOpacity"]?.SetValue(0.3f + phaseBoost * 0.5f);
                    swingShader.Parameters["uIntensity"]?.SetValue(0.6f + stackBoost * 0.5f);
                    swingShader.CurrentTechnique = swingShader.Techniques["CadenceSwingGlow"];

                    float glowWidth = bodyWidth * 1.3f;
                    CadencePrimitiveRenderer.RenderTrail(_tipTrailBuffer, new CadencePrimitiveSettings(
                        completion => (1f - completion * 0.5f) * glowWidth,
                        completion =>
                        {
                            Color c = Color.Lerp(CadenceUtils.RiftDeep, CadenceUtils.CadenceViolet, completion);
                            return c * (1f - completion * 0.7f) * 0.45f;
                        },
                        swingShader
                    ));

                    // PASS C: At high stacks, volatile shimmer trail overlay
                    if (stacks >= 5)
                    {
                        swingShader.Parameters["uOpacity"]?.SetValue(stackIntensity * 0.5f);
                        swingShader.Parameters["uIntensity"]?.SetValue(1.8f);
                        swingShader.Parameters["uColor"]?.SetValue(CadenceUtils.SeveranceLime.ToVector3());
                        swingShader.CurrentTechnique = swingShader.Techniques["CadenceSwingFlow"];

                        CadencePrimitiveRenderer.RenderTrail(_tipTrailBuffer, new CadencePrimitiveSettings(
                            completion => (1f - completion * 0.8f) * bodyWidth * 0.5f,
                            completion =>
                            {
                                Color c = Color.Lerp(CadenceUtils.SeveranceLime, CadenceUtils.ParadoxWhite, completion * 0.5f);
                                return c * (1f - completion) * stackIntensity;
                            },
                            swingShader
                        ));
                    }

                    device.Textures[1] = null;
                    device.BlendState = prevBlend;

                    sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                        DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
                }
            }

            // ═══════════════════════════════════════════════════════
            // LAYER 2: SHADER OVERLAY — Dimensional rift aura at arc
            // CadenceSwingTrail shader rendered as screen-space overlay
            // centered on the swing arc midpoint
            // ═══════════════════════════════════════════════════════
            if (prog > 0.1f && prog < 0.9f)
            {
                Effect cadenceShader = ShaderLoader.CadenceSwingTrail;
                if (cadenceShader != null)
                {
                    Texture2D overlayTex = MagnumTextureRegistry.GetSoftGlow();
                    if (overlayTex != null)
                    {
                        Vector2 midArc = Vector2.Lerp(Owner.MountedCenter, tipPos, 0.55f);
                        Vector2 screenMid = midArc - Main.screenPosition;
                        float arcAngle = SwordDirection.ToRotation();
                        float overlayScale = MathHelper.Min((CurrentPhase.BladeLength / 100f) * (0.6f + ComboStep * 0.12f), 0.58f);

                        EnigmaShaderHelper.DrawShaderOverlay(sb, cadenceShader,
                            overlayTex, screenMid, overlayTex.Size() / 2f, overlayScale,
                            CadenceUtils.CadenceViolet.ToVector3(), CadenceUtils.DimensionalGreen.ToVector3(),
                            opacity: 0.35f + stackIntensity * 0.2f,
                            intensity: 1.0f + ComboStep * 0.2f,
                            rotation: arcAngle,
                            noiseTexture: ShaderLoader.GetNoiseTexture("VoronoiNoise"),
                            techniqueName: "CadenceSwingFlow");
                    }
                }
            }

            // ═══════════════════════════════════════════════════════
            // LAYER 3: 6-LAYER BLOOM STACK + Theme Textures
            // Moonlight Sonata-tier multi-layered bloom at blade tip:
            // Void Core -> Rift Deep -> Cadence Violet -> Dimensional
            // Green -> Severance Lime -> Paradox White
            // ═══════════════════════════════════════════════════════
            {
                CadenceUtils.EnterAdditiveShaderRegion(sb);

                Texture2D bloomTex = MagnumTextureRegistry.GetSoftGlow();
                if (bloomTex != null)
                {
                    Vector2 tipScreen = tipPos - Main.screenPosition;
                    Vector2 bOrigin = bloomTex.Size() / 2f;
                    float pulse = 1f + 0.12f * MathF.Sin(Main.GameUpdateCount * 0.12f + ComboStep);
                    float bloomBase = MathHelper.Min((0.3f + ComboStep * 0.1f + stackIntensity * 0.15f) * pulse * 0.65f, 0.39f);

                    // A: Wide outer void glow
                    sb.Draw(bloomTex, tipScreen, null, CadenceUtils.VoidCore * 0.2f, 0f, bOrigin,
                        bloomBase * 0.55f, SpriteEffects.None, 0f);
                    // B: Mid purple haze
                    sb.Draw(bloomTex, tipScreen, null, CadenceUtils.RiftDeep * 0.35f, 0f, bOrigin,
                        bloomBase * 0.38f, SpriteEffects.None, 0f);
                    // C: Inner violet core
                    sb.Draw(bloomTex, tipScreen, null, CadenceUtils.CadenceViolet * 0.5f, 0f, bOrigin,
                        bloomBase * 0.24f, SpriteEffects.None, 0f);
                    // D: Green-hot transitional
                    sb.Draw(bloomTex, tipScreen, null, CadenceUtils.DimensionalGreen * 0.5f, 0f, bOrigin,
                        bloomBase * 0.14f, SpriteEffects.None, 0f);
                    // E: Bright lime flash
                    sb.Draw(bloomTex, tipScreen, null, CadenceUtils.SeveranceLime * 0.45f, 0f, bOrigin,
                        bloomBase * 0.07f, SpriteEffects.None, 0f);
                    // F: White-hot pinpoint
                    sb.Draw(bloomTex, tipScreen, null, CadenceUtils.ParadoxWhite * 0.6f, 0f, bOrigin,
                        bloomBase * 0.03f, SpriteEffects.None, 0f);

                    // EN Star Flare — dual counter-rotating spectral flares
                    Texture2D starFlareTex = EnigmaThemeTextures.ENStarFlare?.Value;
                    if (starFlareTex != null)
                    {
                        Vector2 sfOrigin = starFlareTex.Size() / 2f;
                        float sfRotA = (float)Main.GameUpdateCount * 0.035f;
                        float sfRotB = -(float)Main.GameUpdateCount * 0.025f;
                        float sfScale = bloomBase * 0.55f;
                        sb.Draw(starFlareTex, tipScreen, null, CadenceUtils.DimensionalGreen * 0.55f,
                            sfRotA, sfOrigin, sfScale, SpriteEffects.None, 0f);
                        sb.Draw(starFlareTex, tipScreen, null, CadenceUtils.CadenceViolet * 0.4f,
                            sfRotB, sfOrigin, sfScale * 0.8f, SpriteEffects.None, 0f);
                    }

                    // EN Power Effect Ring — reality-tear rings
                    Texture2D powerRingTex = EnigmaThemeTextures.ENPowerEffectRing?.Value;
                    if (powerRingTex != null)
                    {
                        Vector2 prOrigin = powerRingTex.Size() / 2f;
                        float prRot = (float)Main.GameUpdateCount * 0.028f;
                        float prScale = bloomBase * 0.45f;
                        sb.Draw(powerRingTex, tipScreen, null, CadenceUtils.SeveranceLime * 0.3f,
                            prRot, prOrigin, prScale, SpriteEffects.None, 0f);
                        sb.Draw(powerRingTex, tipScreen, null, CadenceUtils.CadenceViolet * 0.2f,
                            -prRot * 0.6f, prOrigin, prScale * 1.5f, SpriteEffects.None, 0f);
                    }

                    // EN Enigma Eye — the void watches at high Inevitability stacks
                    if (stacks >= 5)
                    {
                        Texture2D eyeTex = EnigmaThemeTextures.ENEnigmaEye?.Value;
                        if (eyeTex != null)
                        {
                            float eyePulse = MathF.Sin(Main.GameUpdateCount * 0.08f) * 0.15f + 0.85f;
                            float eyeAlpha = (stacks - 5f) / 5f * eyePulse;
                            sb.Draw(eyeTex, tipScreen, null, CadenceUtils.DimensionalGreen * eyeAlpha * 0.5f,
                                0f, eyeTex.Size() / 2f, bloomBase * 0.6f, SpriteEffects.None, 0f);
                        }
                    }

                    // Secondary bloom hub at arc center
                    Vector2 midScreen = Vector2.Lerp(Owner.MountedCenter, tipPos, 0.4f) - Main.screenPosition;
                    sb.Draw(bloomTex, midScreen, null, CadenceUtils.RiftDeep * 0.15f, 0f, bOrigin,
                        bloomBase * 1.5f, SpriteEffects.None, 0f);
                    sb.Draw(bloomTex, midScreen, null, CadenceUtils.CadenceViolet * 0.1f, 0f, bOrigin,
                        bloomBase * 0.8f, SpriteEffects.None, 0f);
                }

                CadenceUtils.ExitShaderRegion(sb);
            }

            // ═══════════════════════════════════════════════════════
            // LAYER 4: PARTICLE CHOREOGRAPHY — phase-specific effects
            // ═══════════════════════════════════════════════════════

            // Base: 1-3 DimensionalRiftMotes per frame at blade tip (more per phase)
            int baseMoteCount = 1 + ComboStep;
            for (int i = 0; i < baseMoteCount; i++)
            {
                Vector2 offset = Main.rand.NextVector2Circular(8f + ComboStep * 3f, 8f + ComboStep * 3f);
                Vector2 vel = SwordDirection.RotatedBy(MathHelper.PiOver2 * Direction) * Main.rand.NextFloat(0.5f, 2.5f + ComboStep);
                CadenceParticleHandler.Spawn(new DimensionalRiftMote(
                    tipPos + offset, vel, Main.rand.NextFloat(0.15f, 0.4f), Main.rand.Next(15, 28)));
            }

            // Glitch fragments along blade length
            if (timer % 2 == 0 && Main.rand.NextBool(2 + (2 - ComboStep)))
            {
                Vector2 glitchPos = Vector2.Lerp(Owner.MountedCenter, tipPos, Main.rand.NextFloat(0.2f, 1f));
                glitchPos += Main.rand.NextVector2Circular(14f, 14f);
                Color glitchColor = Main.rand.NextBool() ? CadenceUtils.DimensionalGreen : CadenceUtils.CadenceViolet;
                CadenceParticleHandler.Spawn(new VoidCleaveParticle(
                    glitchPos, Main.rand.NextVector2Circular(1f, 1f), glitchColor * 0.9f,
                    Main.rand.NextFloat(0.15f, 0.35f), Main.rand.Next(3, 7)));
            }

            // Phase 0 (The Question): Probing void cleaves
            if (ComboStep == 0 && timer % 3 == 0)
            {
                Vector2 vel = SwordDirection * Main.rand.NextFloat(3f, 7f);
                CadenceParticleHandler.Spawn(new VoidCleaveParticle(
                    tipPos, vel, CadenceUtils.CadenceViolet, Main.rand.NextFloat(0.35f, 0.7f), Main.rand.Next(12, 22)));
            }

            // Phase 1 (The Doubt): X-pattern crossed particle streams
            if (ComboStep == 1 && timer % 2 == 0)
            {
                for (int side = -1; side <= 1; side += 2)
                {
                    Vector2 crossVel = SwordDirection.RotatedBy(MathHelper.PiOver4 * side) * Main.rand.NextFloat(4f, 8f);
                    CadenceParticleHandler.Spawn(new DimensionalRiftMote(
                        tipPos + Main.rand.NextVector2Circular(6f, 6f), crossVel,
                        Main.rand.NextFloat(0.25f, 0.5f), Main.rand.Next(10, 20)));
                }
                if (timer % 4 == 0)
                {
                    Vector2 perpVel = SwordDirection.RotatedBy(MathHelper.PiOver2 * Direction) * Main.rand.NextFloat(3f, 6f);
                    CadenceParticleHandler.Spawn(new VoidCleaveParticle(
                        tipPos, perpVel, CadenceUtils.DimensionalGreen, Main.rand.NextFloat(0.3f, 0.5f), Main.rand.Next(10, 18)));
                }
            }

            // Phase 2 (The Silence): Heavy paradox ripples + dimensional tears
            if (ComboStep >= 2)
            {
                if (timer % 2 == 0)
                {
                    Color rippleColor = Main.rand.NextBool() ? CadenceUtils.SeveranceLime : CadenceUtils.DimensionalGreen;
                    CadenceParticleHandler.Spawn(new ParadoxSlashRipple(
                        tipPos, rippleColor, Main.rand.NextFloat(0.25f, 0.5f), Main.rand.Next(20, 38)));
                }
                if (timer % 3 == 0)
                {
                    float t = Main.rand.NextFloat(0.3f, 1f);
                    Vector2 arcPos = Vector2.Lerp(Owner.MountedCenter, tipPos, t);
                    Vector2 burstVel = SwordDirection.RotatedByRandom(0.8) * Main.rand.NextFloat(4f, 9f);
                    CadenceParticleHandler.Spawn(new VoidCleaveParticle(
                        arcPos, burstVel, CadenceUtils.SeveranceLime, Main.rand.NextFloat(0.4f, 0.8f), Main.rand.Next(15, 25)));
                }
            }

            // Inevitability escalation: orbiting motes grow denser
            if (stacks >= 3 && timer % 3 == 0)
            {
                int extraMotes = 1 + (int)(stacks * 0.4f);
                for (int i = 0; i < extraMotes; i++)
                {
                    float orbitR = 18f + stacks * 3.5f;
                    Vector2 orbitOffset = Main.rand.NextVector2Circular(orbitR, orbitR);
                    CadenceParticleHandler.Spawn(new DimensionalRiftMote(
                        tipPos + orbitOffset, -orbitOffset * 0.06f,
                        Main.rand.NextFloat(0.12f, 0.3f), Main.rand.Next(18, 35)));
                }
            }

            // Inevitability glyphs at high stacks
            if (stacks >= 6 && timer % 5 == 0)
            {
                float glyphAngle = Main.rand.NextFloat(MathHelper.TwoPi);
                float glyphRadius = 25f + (10 - stacks) * 2f;
                CadenceParticleHandler.Spawn(new InevitabilityGlyphParticle(
                    tipPos, glyphRadius, glyphAngle, stacks,
                    Color.Lerp(CadenceUtils.DimensionalGreen, CadenceUtils.ParadoxWhite, stackIntensity),
                    0.3f + stackIntensity * 0.2f, Main.rand.Next(18, 28)));
            }

            // CadenceRiftDust every 4 frames
            if (timer % 4 == 0)
            {
                Vector2 dustVel = SwordDirection.RotatedByRandom(0.5) * Main.rand.NextFloat(1.5f, 4f);
                Dust.NewDust(tipPos, 0, 0, ModContent.DustType<CadenceRiftDust>(),
                    dustVel.X, dustVel.Y, 0, default, 1f + ComboStep * 0.2f);
            }

            // ═══════════════════════════════════════════════════════
            // LAYER 5: PEAK SWING BURST — multi-layered explosion
            // ═══════════════════════════════════════════════════════
            int attackFrame = (int)(SwingTime * 0.5f);
            if (timer == attackFrame)
            {
                int burstCount = 4 + ComboStep * 3;
                for (int i = 0; i < burstCount; i++)
                {
                    float arcOffset = MathHelper.Lerp(-0.5f, 0.5f, i / (float)Math.Max(burstCount - 1, 1));
                    Vector2 burstDir = SwordDirection.RotatedBy(arcOffset * Direction);
                    Vector2 burstPos = Owner.MountedCenter + burstDir * CurrentPhase.BladeLength * Main.rand.NextFloat(0.4f, 1f);
                    Vector2 burstVel = burstDir * Main.rand.NextFloat(4f, 9f + ComboStep * 2f);
                    Color burstCol = Color.Lerp(CadenceUtils.CadenceViolet, CadenceUtils.SeveranceLime, Main.rand.NextFloat());
                    CadenceParticleHandler.Spawn(new VoidCleaveParticle(
                        burstPos, burstVel, burstCol, Main.rand.NextFloat(0.4f, 0.9f), Main.rand.Next(14, 26)));
                }

                CadenceParticleHandler.Spawn(new ParadoxSlashRipple(
                    tipPos, CadenceUtils.SeveranceLime, 0.4f + ComboStep * 0.15f, 28));

                CadenceParticleHandler.Spawn(new ParadoxCollapseFlash(
                    tipPos, 0.3f + ComboStep * 0.15f, 12 + ComboStep * 3));

                // Screen shake on Phase 2 finisher
                if (ComboStep >= 2)
                {
                    float shakeStr = 3f + stacks * 0.5f;
                    for (int s = 0; s < 3; s++)
                        Owner.velocity += Main.rand.NextVector2Circular(0.3f, 0.3f) * shakeStr * 0.1f;
                }
            }

            // ═══════════════════════════════════════════════════════
            // LAYER 6: ENIGMA THEME ACCENTS
            // ═══════════════════════════════════════════════════════
            if (timer % 8 == 0)
            {
                EnigmaVFXLibrary.AddPulsingLight(tipPos, Main.GameUpdateCount * 0.1f, 0.4f + ComboStep * 0.1f);
            }
        }

        #endregion
    }
}