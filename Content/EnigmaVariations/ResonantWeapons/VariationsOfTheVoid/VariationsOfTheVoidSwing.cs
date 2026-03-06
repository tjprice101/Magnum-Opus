using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
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
using MagnumOpus.Content.EnigmaVariations.ResonantWeapons.TheUnresolvedCadence;
using MagnumOpus.Content.EnigmaVariations.ResonantWeapons.VariationsOfTheVoid.Particles;
using MagnumOpus.Content.EnigmaVariations.ResonantWeapons.VariationsOfTheVoid.Dusts;
using MagnumOpus.Content.EnigmaVariations.ResonantWeapons.VariationsOfTheVoid.Primitives;
using MagnumOpus.Content.EnigmaVariations.ResonantWeapons.VariationsOfTheVoid.Utilities;
using static MagnumOpus.Common.Systems.Particles.Particle;
using ReLogic.Content;

namespace MagnumOpus.Content.EnigmaVariations.ResonantWeapons.VariationsOfTheVoid
{
    /// <summary>
    /// VARIATIONS OF THE VOID — Enigma Melee Sword (Swing Projectile).
    /// Held-projectile swing via MeleeSwingBase.
    /// 
    /// 3-Phase combo — each a variation of the void's voice:
    ///   Phase 0: HorizontalSweep — fast sweep, no sub-projectiles
    ///   Phase 1: DiagonalSlash — upward diagonal + 1 DimensionalSlash (33% damage)
    ///   Phase 2: HeavySlamFinisher — heavy slam + 3 DimensionalSlash + 3 HomingQuestionSeeker
    /// Every third strike (after Phase 2) spawns VoidConvergenceBeamSet tri-beam.
    /// Beams converge over 120 frames → Void Resonance Explosion (3x damage, 100→300 AoE).
    /// ParadoxBrand on hit (8s), seeking crystals on crit.
    /// </summary>
    public sealed class VariationsOfTheVoidSwing : MeleeSwingBase
    {
        #region Theme Colors, Palette & Trail Tracking

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
            MagnumThemePalettes.EnigmaBlack,  // [0] void darkness
            new Color(60, 20, 100),            // [1] deep arcane
            MagnumThemePalettes.EnigmaPurple,  // [2] primary purple
            MagnumThemePalettes.EnigmaGreen,   // [3] eerie green
            new Color(180, 140, 230),          // [4] bright arcane
            new Color(230, 250, 220)           // [5] whitehot
        };

        #endregion

        #region Combo Phase Definitions

        private static readonly ComboPhase Phase0_HorizontalSweep = new ComboPhase(
            new CurveSegment[]
            {
                new CurveSegment(EasingType.PolyOut, 0f, -0.8f, 0.15f, 2),
                new CurveSegment(EasingType.PolyIn, 0.2f, -0.65f, 1.55f, 3),
                new CurveSegment(EasingType.PolyOut, 0.8f, 0.9f, 0.1f, 2)
            },
            maxAngle: MathHelper.PiOver2 * 1.4f,
            duration: 16,
            bladeLength: 145f,
            flip: false,
            squish: 0.92f,
            damageMult: 0.85f
        );

        private static readonly ComboPhase Phase1_DiagonalSlash = new ComboPhase(
            new CurveSegment[]
            {
                new CurveSegment(EasingType.PolyOut, 0f, -0.9f, 0.2f, 2),
                new CurveSegment(EasingType.PolyIn, 0.22f, -0.7f, 1.6f, 3),
                new CurveSegment(EasingType.PolyOut, 0.82f, 0.9f, 0.1f, 2)
            },
            maxAngle: MathHelper.PiOver2 * 1.6f,
            duration: 18,
            bladeLength: 150f,
            flip: true,
            squish: 0.90f,
            damageMult: 1.0f
        );

        private static readonly ComboPhase Phase2_HeavySlamFinisher = new ComboPhase(
            new CurveSegment[]
            {
                new CurveSegment(EasingType.PolyOut, 0f, -1.0f, 0.3f, 2),
                new CurveSegment(EasingType.PolyIn, 0.28f, -0.7f, 1.7f, 3),
                new CurveSegment(EasingType.PolyOut, 0.85f, 1.0f, 0.05f, 2)
            },
            maxAngle: MathHelper.PiOver2 * 2.0f,
            duration: 24,
            bladeLength: 165f,
            flip: false,
            squish: 0.85f,
            damageMult: 1.25f
        );

        #endregion

        #region Abstract Overrides

        protected override ComboPhase[] GetAllPhases() => new[]
        {
            Phase0_HorizontalSweep,
            Phase1_DiagonalSlash,
            Phase2_HeavySlamFinisher
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
                _ => "MagnumOpus/Assets/VFX Asset Library/SlashArcs/SwordArcSmear"
            };
        }

        protected override string GetSmearGradientPath() => "MagnumOpus/Assets/VFX Asset Library/ColorGradients/EnigmaGradientLUTandRAMP";

        #endregion

        #region Virtual Overrides

        protected override Texture2D GetBladeTexture()
        {
            if (ModContent.HasAsset("MagnumOpus/Content/EnigmaVariations/ResonantWeapons/VariationsOfTheVoid/VariationsOfTheVoid"))
                return ModContent.Request<Texture2D>(
                    "MagnumOpus/Content/EnigmaVariations/ResonantWeapons/VariationsOfTheVoid/VariationsOfTheVoid", AssetRequestMode.ImmediateLoad).Value;
            return base.GetBladeTexture();
        }

        protected override SoundStyle GetSwingSound()
            => SoundID.Item71 with { Pitch = 0.15f + ComboStep * 0.12f, Volume = 0.6f };

        protected override int GetInitialDustType() => DustID.PurpleTorch;
        protected override int GetSecondaryDustType() => DustID.CursedTorch;

        protected override Vector3 GetLightColor()
        {
            Color c = Color.Lerp(EnigmaDeepPurple, EnigmaGreen, Progression * 0.5f);
            return c.ToVector3() * 0.7f;
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

            // Smooth intensity ramp for VFX — scales with combo phase
            float targetIntensity = 0.3f + ComboStep * 0.25f;
            _lastSwingIntensity = MathHelper.Lerp(_lastSwingIntensity, targetIntensity, 0.1f);
        }

        #endregion

        #region HandleComboSpecials

        protected override void HandleComboSpecials()
        {
            if (hasSpawnedSpecial) return;

            // Phase 0 at 55% 窶・no gameplay effect (was VFX only)
            if (ComboStep == 0 && Progression >= 0.55f)
            {
                hasSpawnedSpecial = true;
            }

            // Phase 1 at 60% 窶・dimensional slash sub-projectile
            if (ComboStep == 1 && Progression >= 0.6f)
            {
                hasSpawnedSpecial = true;

                if (Main.myPlayer == Projectile.owner)
                {
                    Vector2 tipPos = GetBladeTipPosition();
                    Vector2 slashVel = SwordDirection * 10f;
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), tipPos, slashVel,
                        ModContent.ProjectileType<DimensionalSlash>(),
                        Projectile.damage / 3, 2f, Projectile.owner,
                        ai0: Projectile.rotation);
                }
            }

            // Phase 2 at 50% 窶・heavy finisher with sub-projectiles
            if (ComboStep == 2 && Progression >= 0.5f)
            {
                hasSpawnedSpecial = true;

                if (Main.myPlayer == Projectile.owner)
                {
                    Vector2 tipPos = GetBladeTipPosition();

                    // Forward slash
                    Vector2 fwdVel = SwordDirection * 12f;
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), tipPos, fwdVel,
                        ModContent.ProjectileType<DimensionalSlash>(),
                        (int)(Projectile.damage * 0.45f), 3f, Projectile.owner,
                        ai0: Projectile.rotation);

                    // Two perpendicular slashes
                    for (int side = -1; side <= 1; side += 2)
                    {
                        Vector2 sideVel = SwordDirection.RotatedBy(side * MathHelper.PiOver4) * 9f;
                        Projectile.NewProjectile(Projectile.GetSource_FromThis(), tipPos, sideVel,
                            ModContent.ProjectileType<DimensionalSlash>(),
                            Projectile.damage / 4, 2f, Projectile.owner,
                            ai0: Projectile.rotation + side * 0.5f);
                    }

                    // Homing question seekers
                    for (int i = 0; i < 3; i++)
                    {
                        float angle = Projectile.rotation + MathHelper.ToRadians(-30 + i * 30);
                        Vector2 crystalVel = angle.ToRotationVector2() * Main.rand.NextFloat(6f, 10f);
                        Projectile.NewProjectile(Projectile.GetSource_FromThis(), tipPos, crystalVel,
                            ModContent.ProjectileType<HomingQuestionSeeker>(),
                            Projectile.damage / 5, 1f, Projectile.owner);
                    }
                }
            }
        }

        #endregion

        #region OnSwingHitNPC

        protected override void OnSwingHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // ParadoxBrand: 8 seconds per doc (480 frames)
            target.AddBuff(ModContent.BuffType<ParadoxBrand>(), 480);
            int stacks = hit.Crit ? 4 : 2;
            for (int s = 0; s < stacks; s++)
                target.AddBuff(ModContent.BuffType<ParadoxBrand>(), 480);

            if (hit.Crit && Main.myPlayer == Projectile.owner)
            {
                for (int i = 0; i < 3; i++)
                {
                    Vector2 crystalVel = Main.rand.NextVector2CircularEdge(7f, 7f);
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), target.Center, crystalVel,
                        ModContent.ProjectileType<HomingQuestionSeeker>(),
                        Projectile.damage / 5, 1f, Projectile.owner);
                }
            }

            Lighting.AddLight(target.Center, EnigmaPurple.ToVector3() * 0.8f);

            // === VFX: AbyssalEchoRing at target ===
            VoidVariationParticleHandler.Spawn(new AbyssalEchoRing(
                target.Center, VoidVariationUtils.VariationViolet, 0.3f, 30));

            // === VFX: 3-5 RiftSunderSpark burst from hit point ===
            for (int sp = 0; sp < Main.rand.Next(3, 6); sp++)
            {
                Vector2 sparkVel = Main.rand.NextVector2CircularEdge(5f, 5f);
                VoidVariationParticleHandler.Spawn(new RiftSunderSpark(
                    target.Center, sparkVel, Main.rand.NextFloat(0.15f, 0.25f), Main.rand.Next(15, 25)));
            }

            // === VFX: VoidWhisperMote at target ===
            VoidVariationParticleHandler.Spawn(new VoidWhisperMote(
                target.Center, Main.rand.NextVector2Circular(1f, 1f), VoidVariationUtils.VoidSurge, 0.3f, 35));
        }

        #endregion

        #region DrawCustomVFX

        protected override void DrawCustomVFX(SpriteBatch sb)
        {
            if (Progression <= 0f || Progression >= 1f) return;

            Vector2 tipPos = GetBladeTipPosition();
            int timer = (int)Main.GameUpdateCount;
            float prog = Progression;
            float phaseBoost = ComboStep * 0.15f;
            float swingIntensity = _lastSwingIntensity;

            // ═══════════════════════════════════════════════════════
            // LAYER 1: GPU PRIMITIVE TRAIL — VoidVariation shader-driven
            // 3 passes: Body (VoidVariationSwingFlow), Glow (VoidVariationSwingGlow),
            // Deep void shimmer on Phase 2 finisher
            // ═══════════════════════════════════════════════════════
            if (_tipTrailBuffer.Count >= 3)
            {
                Effect voidShader = ShaderLoader.VoidSwingTrail;
                if (voidShader != null)
                {
                    sb.End();

                    var device = Main.graphics.GraphicsDevice;
                    var prevBlend = device.BlendState;
                    device.BlendState = MagnumBlendStates.TrueAdditive;

                    // Bind noise texture for dimensional fracture patterns
                    Texture2D noiseTex = ShaderLoader.GetNoiseTexture("CosmicEnergyVortex");
                    if (noiseTex != null)
                        device.Textures[1] = noiseTex;

                    float timeVal = (float)Main.GameUpdateCount * 0.04f;
                    voidShader.Parameters["uTime"]?.SetValue(timeVal);
                    voidShader.Parameters["uColor"]?.SetValue(VoidVariationUtils.VariationViolet.ToVector3());
                    voidShader.Parameters["uSecondaryColor"]?.SetValue(VoidVariationUtils.RiftTeal.ToVector3());

                    // PASS A: Body trail (VoidVariationSwingFlow) — primary rift energy
                    float bodyWidth = 28f + ComboStep * 8f;
                    voidShader.Parameters["uOpacity"]?.SetValue(0.85f + phaseBoost);
                    voidShader.Parameters["uIntensity"]?.SetValue(1.0f + swingIntensity);
                    voidShader.CurrentTechnique = voidShader.Techniques["VoidVariationSwingFlow"];

                    VoidVariationPrimitiveRenderer.RenderTrail(_tipTrailBuffer, new VoidVariationPrimitiveSettings(
                        completion => (1f - completion * 0.6f) * bodyWidth,
                        completion =>
                        {
                            Color c = Color.Lerp(VoidVariationUtils.RiftTeal, VoidVariationUtils.VariationViolet, completion);
                            return c * (1f - completion * 0.5f);
                        },
                        voidShader
                    ));

                    // PASS B: Glow trail (VoidVariationSwingGlow) — soft abyssal bloom underlayer
                    voidShader.Parameters["uOpacity"]?.SetValue(0.3f + phaseBoost * 0.6f);
                    voidShader.Parameters["uIntensity"]?.SetValue(0.6f + swingIntensity * 0.5f);
                    voidShader.CurrentTechnique = voidShader.Techniques["VoidVariationSwingGlow"];

                    float glowWidth = bodyWidth * 1.4f;
                    VoidVariationPrimitiveRenderer.RenderTrail(_tipTrailBuffer, new VoidVariationPrimitiveSettings(
                        completion => (1f - completion * 0.5f) * glowWidth,
                        completion =>
                        {
                            Color c = Color.Lerp(VoidVariationUtils.AbyssPurple, VoidVariationUtils.VariationViolet, completion);
                            return c * (1f - completion * 0.7f) * 0.45f;
                        },
                        voidShader
                    ));

                    // PASS C: Phase 2 finisher — deep void shimmer overlay for the heavy slam
                    if (ComboStep >= 2)
                    {
                        voidShader.Parameters["uOpacity"]?.SetValue(swingIntensity * 0.6f);
                        voidShader.Parameters["uIntensity"]?.SetValue(1.8f);
                        voidShader.Parameters["uColor"]?.SetValue(VoidVariationUtils.VoidSurge.ToVector3());
                        voidShader.CurrentTechnique = voidShader.Techniques["VoidVariationSwingFlow"];

                        VoidVariationPrimitiveRenderer.RenderTrail(_tipTrailBuffer, new VoidVariationPrimitiveSettings(
                            completion => (1f - completion * 0.8f) * bodyWidth * 0.55f,
                            completion =>
                            {
                                Color c = Color.Lerp(VoidVariationUtils.VoidSurge, VoidVariationUtils.SunderingWhite, completion * 0.5f);
                                return c * (1f - completion) * swingIntensity;
                            },
                            voidShader
                        ));
                    }

                    device.Textures[1] = null;
                    device.BlendState = prevBlend;

                    sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                        DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
                }
            }

            // ═══════════════════════════════════════════════════════
            // LAYER 2: SHADER OVERLAY — Abyssal rift aura at arc center
            // VoidSwingTrail shader in screen-space, voronoi-disrupted
            // ═══════════════════════════════════════════════════════
            if (prog > 0.1f && prog < 0.9f)
            {
                Effect voidOverlay = ShaderLoader.VoidSwingTrail;
                if (voidOverlay != null)
                {
                    Texture2D overlayTex = MagnumTextureRegistry.GetSoftGlow();
                    if (overlayTex != null)
                    {
                        Vector2 midArc = Vector2.Lerp(Owner.MountedCenter, tipPos, 0.55f);
                        Vector2 screenMid = midArc - Main.screenPosition;
                        float arcAngle = SwordDirection.ToRotation();
                        float overlayScale = (CurrentPhase.BladeLength / 100f) * (0.6f + ComboStep * 0.12f);

                        EnigmaShaderHelper.DrawShaderOverlay(sb, voidOverlay,
                            overlayTex, screenMid, overlayTex.Size() / 2f, overlayScale,
                            VoidVariationUtils.VariationViolet.ToVector3(), VoidVariationUtils.RiftTeal.ToVector3(),
                            opacity: 0.35f + swingIntensity * 0.2f,
                            intensity: 1.0f + ComboStep * 0.2f,
                            rotation: arcAngle,
                            noiseTexture: ShaderLoader.GetNoiseTexture("VoronoiNoise"),
                            techniqueName: "VoidVariationSwingFlow");
                    }
                }
            }

            // ═══════════════════════════════════════════════════════
            // LAYER 3: 6-LAYER BLOOM STACK + Theme Textures
            // True Void -> Abyss Purple -> Variation Violet -> Rift Teal
            // -> Void Surge -> Sundering White
            // ═══════════════════════════════════════════════════════
            {
                VoidVariationUtils.EnterAdditiveShaderRegion(sb);

                Texture2D bloomTex = MagnumTextureRegistry.GetSoftGlow();
                if (bloomTex != null)
                {
                    Vector2 tipScreen = tipPos - Main.screenPosition;
                    Vector2 bOrigin = bloomTex.Size() / 2f;
                    float pulse = 1f + 0.12f * MathF.Sin(Main.GameUpdateCount * 0.1f + ComboStep);
                    float bloomBase = (0.3f + ComboStep * 0.1f + swingIntensity * 0.15f) * pulse * 0.65f;

                    // A: Wide outer void glow — the emptiness radiating outward
                    sb.Draw(bloomTex, tipScreen, null, VoidVariationUtils.TrueVoid * 0.2f, 0f, bOrigin,
                        bloomBase * 3.5f, SpriteEffects.None, 0f);
                    // B: Mid abyss purple haze — the resonance depth
                    sb.Draw(bloomTex, tipScreen, null, VoidVariationUtils.AbyssPurple * 0.35f, 0f, bOrigin,
                        bloomBase * 2.2f, SpriteEffects.None, 0f);
                    // C: Inner violet core — the shifting variation made visible
                    sb.Draw(bloomTex, tipScreen, null, VoidVariationUtils.VariationViolet * 0.5f, 0f, bOrigin,
                        bloomBase * 1.3f, SpriteEffects.None, 0f);
                    // D: Rift teal transitional — where reality fractures
                    sb.Draw(bloomTex, tipScreen, null, VoidVariationUtils.RiftTeal * 0.5f, 0f, bOrigin,
                        bloomBase * 0.7f, SpriteEffects.None, 0f);
                    // E: Void surge flash — abyssal energy breaking free
                    sb.Draw(bloomTex, tipScreen, null, VoidVariationUtils.VoidSurge * 0.45f, 0f, bOrigin,
                        bloomBase * 0.35f, SpriteEffects.None, 0f);
                    // F: Sundering white-hot pinpoint — the rift torn open
                    sb.Draw(bloomTex, tipScreen, null, VoidVariationUtils.SunderingWhite * 0.6f, 0f, bOrigin,
                        bloomBase * 0.12f, SpriteEffects.None, 0f);

                    // EN Star Flare — dual counter-rotating dimensional rift flares
                    Texture2D starFlareTex = EnigmaThemeTextures.ENStarFlare?.Value;
                    if (starFlareTex != null)
                    {
                        Vector2 sfOrigin = starFlareTex.Size() / 2f;
                        float sfRotA = (float)Main.GameUpdateCount * 0.03f;
                        float sfRotB = -(float)Main.GameUpdateCount * 0.022f;
                        float sfScale = bloomBase * 0.55f;
                        sb.Draw(starFlareTex, tipScreen, null, VoidVariationUtils.RiftTeal * 0.55f,
                            sfRotA, sfOrigin, sfScale, SpriteEffects.None, 0f);
                        sb.Draw(starFlareTex, tipScreen, null, VoidVariationUtils.VariationViolet * 0.4f,
                            sfRotB, sfOrigin, sfScale * 0.8f, SpriteEffects.None, 0f);
                    }

                    // EN Power Effect Ring — expanding abyssal resonance rings
                    Texture2D powerRingTex = EnigmaThemeTextures.ENPowerEffectRing?.Value;
                    if (powerRingTex != null)
                    {
                        Vector2 prOrigin = powerRingTex.Size() / 2f;
                        float prRot = (float)Main.GameUpdateCount * 0.025f;
                        float prScale = bloomBase * 0.45f;
                        sb.Draw(powerRingTex, tipScreen, null, VoidVariationUtils.VoidSurge * 0.3f,
                            prRot, prOrigin, prScale, SpriteEffects.None, 0f);
                        sb.Draw(powerRingTex, tipScreen, null, VoidVariationUtils.VariationViolet * 0.2f,
                            -prRot * 0.6f, prOrigin, prScale * 1.5f, SpriteEffects.None, 0f);
                    }

                    // EN Enigma Eye — the void watches during the heavy slam finisher
                    if (ComboStep >= 2)
                    {
                        Texture2D eyeTex = EnigmaThemeTextures.ENEnigmaEye?.Value;
                        if (eyeTex != null)
                        {
                            float eyePulse = MathF.Sin(Main.GameUpdateCount * 0.08f) * 0.15f + 0.85f;
                            float eyeAlpha = (prog > 0.3f && prog < 0.7f) ? eyePulse * 0.6f : eyePulse * 0.2f;
                            sb.Draw(eyeTex, tipScreen, null, VoidVariationUtils.RiftTeal * eyeAlpha,
                                0f, eyeTex.Size() / 2f, bloomBase * 0.6f, SpriteEffects.None, 0f);
                        }
                    }

                    // Secondary bloom hub at arc center — ambient void resonance
                    Vector2 midScreen = Vector2.Lerp(Owner.MountedCenter, tipPos, 0.4f) - Main.screenPosition;
                    sb.Draw(bloomTex, midScreen, null, VoidVariationUtils.AbyssPurple * 0.15f, 0f, bOrigin,
                        bloomBase * 1.5f, SpriteEffects.None, 0f);
                    sb.Draw(bloomTex, midScreen, null, VoidVariationUtils.VariationViolet * 0.1f, 0f, bOrigin,
                        bloomBase * 0.8f, SpriteEffects.None, 0f);
                }

                VoidVariationUtils.ExitShaderRegion(sb);
            }

            // ═══════════════════════════════════════════════════════
            // LAYER 4: PARTICLE CHOREOGRAPHY — phase-specific void effects
            // Each phase has its own distinctive void variation pattern
            // ═══════════════════════════════════════════════════════

            // Base: 1-3 VoidWhisperMotes per frame at blade tip (density scales with phase)
            int baseMoteCount = 1 + ComboStep;
            for (int i = 0; i < baseMoteCount; i++)
            {
                Vector2 offset = Main.rand.NextVector2Circular(8f + ComboStep * 3f, 8f + ComboStep * 3f);
                Vector2 vel = SwordDirection.RotatedBy(MathHelper.PiOver2 * Direction) * Main.rand.NextFloat(0.5f, 2.5f + ComboStep);
                Color whisperColor = Main.rand.NextBool() ? VoidVariationUtils.VariationViolet : VoidVariationUtils.RiftTeal;
                VoidVariationParticleHandler.Spawn(new VoidWhisperMote(
                    tipPos + offset, vel, whisperColor,
                    Main.rand.NextFloat(0.1f, 0.25f), Main.rand.Next(18, 35)));
            }

            // Void echo afterimage — dimensional flicker fragments along blade
            if (timer % 2 == 0 && Main.rand.NextBool(2 + (2 - ComboStep)))
            {
                Vector2 echoPos = Vector2.Lerp(Owner.MountedCenter, tipPos, Main.rand.NextFloat(0.2f, 1f));
                echoPos += Main.rand.NextVector2Circular(14f, 14f);
                VoidVariationParticleHandler.Spawn(new RiftSunderSpark(
                    echoPos, Main.rand.NextVector2Circular(1f, 1f),
                    Main.rand.NextFloat(0.15f, 0.35f), Main.rand.Next(3, 7)));
            }

            // Phase 0 (Horizontal Sweep): Perpendicular rift sparks — reality shearing
            if (ComboStep == 0 && timer % 3 == 0)
            {
                Vector2 sparkVel = SwordDirection.RotatedBy(MathHelper.PiOver2 * Direction) * Main.rand.NextFloat(3f, 7f);
                VoidVariationParticleHandler.Spawn(new RiftSunderSpark(
                    tipPos, sparkVel, Main.rand.NextFloat(0.2f, 0.4f), Main.rand.Next(12, 22)));
            }

            // Phase 1 (Diagonal Slash): X-pattern crossed rift streams
            if (ComboStep == 1 && timer % 2 == 0)
            {
                for (int side = -1; side <= 1; side += 2)
                {
                    Vector2 crossVel = SwordDirection.RotatedBy(MathHelper.PiOver4 * side) * Main.rand.NextFloat(4f, 8f);
                    VoidVariationParticleHandler.Spawn(new RiftSunderSpark(
                        tipPos + Main.rand.NextVector2Circular(6f, 6f), crossVel,
                        Main.rand.NextFloat(0.25f, 0.5f), Main.rand.Next(10, 20)));
                }
                // Extra abyssal echo ring every 4 frames
                if (timer % 4 == 0)
                {
                    VoidVariationParticleHandler.Spawn(new AbyssalEchoRing(
                        tipPos, VoidVariationUtils.VariationViolet, 0.15f, Main.rand.Next(18, 30)));
                }
            }

            // Phase 2 (Heavy Slam): Dense void fracture — reality shattering
            if (ComboStep >= 2)
            {
                if (timer % 2 == 0)
                {
                    Color ringColor = Main.rand.NextBool() ? VoidVariationUtils.VoidSurge : VoidVariationUtils.RiftTeal;
                    VoidVariationParticleHandler.Spawn(new AbyssalEchoRing(
                        tipPos, ringColor, Main.rand.NextFloat(0.2f, 0.4f), Main.rand.Next(20, 38)));
                }
                if (timer % 3 == 0)
                {
                    float t = Main.rand.NextFloat(0.3f, 1f);
                    Vector2 arcPos = Vector2.Lerp(Owner.MountedCenter, tipPos, t);
                    Vector2 burstVel = SwordDirection.RotatedByRandom(0.8) * Main.rand.NextFloat(4f, 9f);
                    VoidVariationParticleHandler.Spawn(new RiftSunderSpark(
                        arcPos, burstVel, Main.rand.NextFloat(0.4f, 0.8f), Main.rand.Next(15, 25)));
                }
                // Heavy slam convergence motes spiraling toward tip
                if (timer % 4 == 0)
                {
                    for (int i = 0; i < 2; i++)
                    {
                        Vector2 spiralOffset = Main.rand.NextVector2Circular(40f, 40f);
                        VoidVariationParticleHandler.Spawn(new TriBeamConvergenceMote(
                            tipPos + spiralOffset, -spiralOffset * 0.04f, VoidVariationUtils.VoidSurge,
                            Main.rand.NextFloat(0.12f, 0.22f), Main.rand.Next(18, 30)));
                    }
                }
            }

            // VoidVariationDust every 4 frames
            if (timer % 4 == 0)
            {
                Vector2 dustVel = SwordDirection.RotatedByRandom(0.5) * Main.rand.NextFloat(1.5f, 4f);
                Dust.NewDust(tipPos, 0, 0, ModContent.DustType<VoidVariationDust>(),
                    dustVel.X, dustVel.Y, 0, default, 1f + ComboStep * 0.2f);
            }

            // ═══════════════════════════════════════════════════════
            // LAYER 5: PEAK SWING BURST — multi-layered void detonation
            // ═══════════════════════════════════════════════════════
            int attackFrame = (int)(SwingTime * 0.5f);
            if ((int)Timer == attackFrame)
            {
                // Radial burst of rift sparks along the swing arc
                int burstCount = 4 + ComboStep * 3;
                for (int i = 0; i < burstCount; i++)
                {
                    float arcOffset = MathHelper.Lerp(-0.5f, 0.5f, i / (float)Math.Max(burstCount - 1, 1));
                    Vector2 burstDir = SwordDirection.RotatedBy(arcOffset * Direction);
                    Vector2 burstPos = Owner.MountedCenter + burstDir * CurrentPhase.BladeLength * Main.rand.NextFloat(0.4f, 1f);
                    Vector2 burstVel = burstDir * Main.rand.NextFloat(4f, 9f + ComboStep * 2f);
                    Color burstCol = Color.Lerp(VoidVariationUtils.VariationViolet, VoidVariationUtils.VoidSurge, Main.rand.NextFloat());
                    VoidVariationParticleHandler.Spawn(new RiftSunderSpark(
                        burstPos, burstVel, Main.rand.NextFloat(0.4f, 0.9f), Main.rand.Next(14, 26)));
                }

                // Abyssal echo ring at tip — expanding void resonance
                VoidVariationParticleHandler.Spawn(new AbyssalEchoRing(
                    tipPos, VoidVariationUtils.VoidSurge, 0.35f + ComboStep * 0.15f, 30));

                // Radial whisper burst — void motes scattered from impact
                for (int i = 0; i < Main.rand.Next(4, 8); i++)
                {
                    Vector2 radialVel = Main.rand.NextVector2CircularEdge(5f, 5f);
                    VoidVariationParticleHandler.Spawn(new VoidWhisperMote(
                        tipPos, radialVel, VoidVariationUtils.SunderingWhite,
                        Main.rand.NextFloat(0.15f, 0.3f), Main.rand.Next(18, 30)));
                }

                // Screen shake on Phase 2 heavy slam finisher
                if (ComboStep >= 2)
                {
                    float shakeStr = 3.5f;
                    for (int s = 0; s < 3; s++)
                        Owner.velocity += Main.rand.NextVector2Circular(0.3f, 0.3f) * shakeStr * 0.1f;
                }
            }

            // ═══════════════════════════════════════════════════════
            // LAYER 6: ENIGMA THEME ACCENTS — ambient void pulsing
            // ═══════════════════════════════════════════════════════
            if (timer % 8 == 0)
            {
                EnigmaVFXLibrary.AddPulsingLight(tipPos, Main.GameUpdateCount * 0.1f, 0.4f + ComboStep * 0.1f);
            }
        }

        #endregion
    }
}
