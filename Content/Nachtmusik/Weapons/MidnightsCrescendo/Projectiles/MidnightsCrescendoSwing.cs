using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.BaseClasses;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.VFX.Bloom;
using MagnumOpus.Common.Systems.VFX.Trails;
using MagnumOpus.Common.Systems.Shaders;
using MagnumOpus.Content.Nachtmusik;
using MagnumOpus.Content.Nachtmusik.Debuffs;
using MagnumOpus.Content.Nachtmusik.Weapons.MidnightsCrescendo.Utilities;
using static MagnumOpus.Common.Systems.Particles.Particle;
using ReLogic.Content;

namespace MagnumOpus.Content.Nachtmusik.Weapons.MidnightsCrescendo.Projectiles
{
    /// <summary>
    /// Swing projectile for Midnight's Crescendo —rapid crescendo-building sword.
    /// 3-phase combo: fast left-right-overhead (22, 22, 26 frames).
    /// VFX intensity scales dynamically with the weapon's crescendo stacks.
    /// At 15 stacks, the blade becomes a blinding storm of cosmic starlight.
    /// </summary>
    public sealed class MidnightsCrescendoSwing : MeleeSwingBase
    {

        #region Combo Phases —Fast crescendo rhythm (22, 22, 26)

        // Phase 0: Quick left sweep —opening allegro
        private static readonly ComboPhase Phase0_LeftSweep = new ComboPhase(
            curves: new CurveSegment[]
            {
                new CurveSegment(EasingType.PolyOut, 0f, -0.80f, 0.18f, 2),
                new CurveSegment(EasingType.PolyIn, 0.20f, -0.62f, 1.52f, 3),
                new CurveSegment(EasingType.PolyOut, 0.82f, 0.90f, 0.10f, 2)
            },
            maxAngle: MathHelper.PiOver2 * 1.4f,
            duration: 22,
            bladeLength: 145f,
            flip: false,
            squish: 0.92f,
            damageMult: 0.80f
        );

        // Phase 1: Quick right return —vivace counterstroke
        private static readonly ComboPhase Phase1_RightReturn = new ComboPhase(
            curves: new CurveSegment[]
            {
                new CurveSegment(EasingType.PolyOut, 0f, -0.85f, 0.18f, 2),
                new CurveSegment(EasingType.PolyIn, 0.22f, -0.67f, 1.57f, 3),
                new CurveSegment(EasingType.PolyOut, 0.84f, 0.90f, 0.10f, 2)
            },
            maxAngle: MathHelper.PiOver2 * 1.5f,
            duration: 22,
            bladeLength: 148f,
            flip: true,
            squish: 0.90f,
            damageMult: 0.90f
        );

        // Phase 2: Overhead crescendo slam —fortissimo finisher
        private static readonly ComboPhase Phase2_CrescendoSlam = new ComboPhase(
            curves: new CurveSegment[]
            {
                new CurveSegment(EasingType.SineOut, 0f, -1.0f, 0.14f, 2),
                new CurveSegment(EasingType.PolyIn, 0.16f, -0.86f, 2.06f, 4),
                new CurveSegment(EasingType.PolyOut, 0.82f, 1.20f, 0.05f, 2)
            },
            maxAngle: MathHelper.PiOver2 * 2.0f,
            duration: 26,
            bladeLength: 155f,
            flip: false,
            squish: 0.85f,
            damageMult: 1.15f
        );

        #endregion

        #region Abstract Overrides

        protected override ComboPhase[] GetAllPhases() => new ComboPhase[]
        {
            Phase0_LeftSweep,
            Phase1_RightReturn,
            Phase2_CrescendoSlam
        };

        protected override Color[] GetPalette() => NachtmusikPalette.MidnightsCrescendoBlade;

        protected override CalamityStyleTrailRenderer.TrailStyle GetTrailStyle()
            => CalamityStyleTrailRenderer.TrailStyle.Cosmic;

        protected override string GetSmearTexturePath(int comboStep) => comboStep switch
        {
            2 => "MagnumOpus/Assets/VFX Asset Library/MasksAndShapes/VerticalEllipse",
            _ => "MagnumOpus/Assets/VFX Asset Library/MasksAndShapes/WideSoftEllipse"
        };

        protected override string GetSmearGradientPath() => "MagnumOpus/Assets/VFX Asset Library/ColorGradients/NachtmusikGradientLUTandRAMP";

        #endregion

        #region Virtual Overrides

        protected override Texture2D GetBladeTexture()
            => ModContent.Request<Texture2D>("MagnumOpus/Content/Nachtmusik/Weapons/MidnightsCrescendo/MidnightsCrescendo", AssetRequestMode.ImmediateLoad).Value;

        protected override SoundStyle GetSwingSound()
            => SoundID.Item71 with { Pitch = 0.1f + ComboStep * 0.15f, Volume = 0.85f };

        protected override int GetInitialDustType() => DustID.PurpleTorch;

        protected override int GetSecondaryDustType() => DustID.Enchanted_Gold;

        protected override Vector3 GetLightColor()
        {
            float stackProgress = GetCrescendoStackProgress();
            float intensity = 0.4f + ComboStep * 0.1f + stackProgress * 0.4f;
            Color c = Color.Lerp(NachtmusikPalette.MidnightsCrescendoBlade[1], NachtmusikPalette.MidnightsCrescendoBlade[4], Progression * 0.5f + stackProgress * 0.5f);
            return c.ToVector3() * intensity;
        }

        #endregion

        #region Crescendo Stack Helpers

        /// <summary>
        /// Read crescendo stack progress (0-1) from the owning player's held item.
        /// </summary>
        private float GetCrescendoStackProgress()
        {
            Player player = Owner;
            if (player?.HeldItem?.ModItem is MidnightsCrescendo mc)
                return mc.CrescendoStacks / 15f;
            return 0f;
        }

        /// <summary>
        /// Read raw crescendo stack count from the owning player's held item.
        /// </summary>
        private int GetCrescendoStacks()
        {
            Player player = Owner;
            if (player?.HeldItem?.ModItem is MidnightsCrescendo mc)
                return mc.CrescendoStacks;
            return 0;
        }

        #endregion

        #region Combo Specials —Phase-specific accent VFX

        protected override void HandleComboSpecials()
        {
            if (hasSpawnedSpecial) return;

            float stackProgress = GetCrescendoStackProgress();

            // Phase 0: Accent VFX at 55% progression
            if (ComboStep == 0 && Progression >= 0.55f)
            {
                hasSpawnedSpecial = true;
                Vector2 tipPos = GetBladeTipPosition();

                CustomParticles.GenericFlare(tipPos, Color.Lerp(NachtmusikPalette.MidnightsCrescendoBlade[2], NachtmusikPalette.MidnightsCrescendoBlade[3], stackProgress), 0.4f + stackProgress * 0.2f, 14);
                CustomParticles.HaloRing(tipPos, NachtmusikPalette.MidnightsCrescendoBlade[1], 0.25f + stackProgress * 0.1f, 12);
                NachtmusikVFXLibrary.SpawnMusicNotes(tipPos, 2, 15f, 0.6f, 0.9f, 25);
                NachtmusikVFXLibrary.SpawnTwinklingStars(tipPos, 1 + (int)(stackProgress * 2), 12f);
            }

            // Phase 1: Accent VFX at 55% progression —slightly bigger
            if (ComboStep == 1 && Progression >= 0.55f)
            {
                hasSpawnedSpecial = true;
                Vector2 tipPos = GetBladeTipPosition();

                CustomParticles.GenericFlare(tipPos, Color.Lerp(NachtmusikPalette.MidnightsCrescendoBlade[3], NachtmusikPalette.MidnightsCrescendoBlade[5], stackProgress), 0.5f + stackProgress * 0.2f, 16);
                CustomParticles.HaloRing(tipPos, NachtmusikPalette.MidnightsCrescendoBlade[2], 0.3f + stackProgress * 0.12f, 14);
                NachtmusikVFXLibrary.SpawnMusicNotes(tipPos, 3, 20f, 0.7f, 0.9f, 25);
                NachtmusikVFXLibrary.SpawnTwinklingStars(tipPos, 2 + (int)(stackProgress * 2), 15f);

                if (stackProgress > 0.5f)
                    NachtmusikVFXLibrary.SpawnShatteredStarlight(tipPos, 3, 3f, 0.4f, false);
            }

            // Phase 2: Crescendo slam at 65% —extra cosmic dust burst
            if (ComboStep == 2 && Progression >= 0.65f)
            {
                hasSpawnedSpecial = true;
                Vector2 tipPos = GetBladeTipPosition();

                // Crescendo slam flare cascade
                CustomParticles.GenericFlare(tipPos, NachtmusikPalette.MidnightsCrescendoBlade[5], 0.6f + stackProgress * 0.3f, 18);
                CustomParticles.GenericFlare(tipPos, NachtmusikPalette.MidnightsCrescendoBlade[2], 0.45f + stackProgress * 0.2f, 16);

                for (int ring = 0; ring < 3; ring++)
                {
                    float p = ring / 3f;
                    Color ringColor = Color.Lerp(NachtmusikPalette.MidnightsCrescendoBlade[1], NachtmusikPalette.MidnightsCrescendoBlade[3], p + stackProgress * 0.3f);
                    CustomParticles.HaloRing(tipPos, ringColor, 0.3f + ring * 0.1f + stackProgress * 0.05f, 14 + ring * 2);
                }

                // Dense cosmic dust burst —the crescendo slam's signature
                int dustCount = 10 + (int)(stackProgress * 8);
                for (int i = 0; i < dustCount; i++)
                {
                    float angle = MathHelper.TwoPi * i / dustCount;
                    float dp = (float)i / dustCount;
                    Color dc = Color.Lerp(NachtmusikPalette.MidnightsCrescendoBlade[1], NachtmusikPalette.MidnightsCrescendoBlade[5], dp * (0.6f + stackProgress * 0.4f));
                    Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 7f) * (1f + stackProgress * 0.5f);
                    Dust d = Dust.NewDustPerfect(tipPos, DustID.PurpleTorch, vel, 0, dc, 1.3f + stackProgress * 0.4f);
                    d.noGravity = true;
                }

                NachtmusikVFXLibrary.SpawnMusicNotes(tipPos, 4 + (int)(stackProgress * 3), 25f, 0.7f, 1.0f, 28);
                NachtmusikVFXLibrary.SpawnShatteredStarlight(tipPos, 4 + (int)(stackProgress * 3), 5f, 0.5f + stackProgress * 0.2f, false);
                NachtmusikVFXLibrary.SpawnConstellationCircle(tipPos, 40f + stackProgress * 20f, 6, Main.rand.NextFloat() * MathHelper.TwoPi);
                NachtmusikVFXLibrary.SpawnGradientSparkles(tipPos, SwordDirection, 4 + (int)(stackProgress * 4), 0.3f + stackProgress * 0.15f, 20, 10f);

                if (stackProgress > 0.5f)
                    MagnumScreenEffects.AddScreenShake(3f + stackProgress * 4f);
            }
        }

        #endregion

        #region On Hit —Celestial Harmony + Crescendo Stack Building

        protected override void OnSwingHitNPC(NPC target, NPC.HitInfo hit, int remainingDamageCount)
        {
            // Apply Celestial Harmony debuff
            target.AddBuff(ModContent.BuffType<CelestialHarmony>(), 480);
            if (target.TryGetGlobalNPC(out CelestialHarmonyNPC harmonyNPC))
                harmonyNPC.AddStack(target, 1);

            // Build crescendo stacks on the weapon item
            if (Main.myPlayer == Projectile.owner)
            {
                Player player = Main.player[Projectile.owner];
                if (player.HeldItem?.ModItem is MidnightsCrescendo mc)
                {
                    mc.CrescendoStacks++;
                    mc.ResetDecayTimer();
                }
            }

            float stackProgress = GetCrescendoStackProgress();
            int stacks = GetCrescendoStacks();

            // Layered cosmic impact VFX —scales with both combo and stacks
            MidnightsCrescendoVFX.SwingImpactVFX(target.Center, ComboStep, stackProgress);

            // Cosmic ripple rings —more rings at higher stacks
            int ringCount = 2 + ComboStep + (stacks >= 8 ? 1 : 0);
            for (int ring = 0; ring < ringCount; ring++)
            {
                float p = (float)ring / ringCount;
                Color ringColor = Color.Lerp(NachtmusikPalette.MidnightsCrescendoBlade[1], NachtmusikPalette.MidnightsCrescendoBlade[3], p + stackProgress * 0.2f);
                CustomParticles.HaloRing(target.Center, ringColor, 0.25f + ring * 0.08f, 12 + ring * 2);
            }

            // Radial cosmic dust burst —palette and density scale with stacks
            int dustCount = 6 + ComboStep * 3 + (int)(stackProgress * 6);
            for (int i = 0; i < dustCount; i++)
            {
                float angle = MathHelper.TwoPi * i / dustCount;
                float dp = (float)i / dustCount;
                Color dc = Color.Lerp(NachtmusikPalette.MidnightsCrescendoBlade[1], Color.Lerp(NachtmusikPalette.MidnightsCrescendoBlade[3], NachtmusikPalette.MidnightsCrescendoBlade[5], stackProgress), dp);
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 6f) * (1f + stackProgress * 0.3f);
                Dust d = Dust.NewDustPerfect(target.Center, DustID.PurpleTorch, vel, 0, dc, 1.2f + stackProgress * 0.3f);
                d.noGravity = true;
            }

            // Stellar sparkle accents
            NachtmusikVFXLibrary.SpawnTwinklingStars(target.Center, 2 + ComboStep, 20f + stackProgress * 10f);
            NachtmusikVFXLibrary.SpawnMusicNotes(target.Center, 2 + ComboStep, 20f, 0.6f, 0.9f, 25);

            // Palette-ramped sparkle explosion on impact (scales with crescendo stacks)
            NachtmusikVFXLibrary.SpawnGradientSparkleExplosion(target.Center, 6 + ComboStep * 3 + (int)(stackProgress * 4), 4f + ComboStep + stackProgress * 2f, 0.3f + stackProgress * 0.1f);

            if (hit.Crit)
            {
                NachtmusikVFXLibrary.SpawnStarBurst(target.Center, 3 + (int)(stackProgress * 3), 0.8f + stackProgress * 0.4f);
                NachtmusikVFXLibrary.SpawnShatteredStarlight(target.Center, 3, 4f, 0.5f, false);
            }

            // At max stacks, every hit is explosive
            if (stacks >= 15)
            {
                NachtmusikVFXLibrary.SpawnStarburstCascade(target.Center, 3, 0.8f);
                MagnumScreenEffects.AddScreenShake(2f);
            }

            Lighting.AddLight(target.Center, Color.Lerp(NachtmusikPalette.MidnightsCrescendoBlade[2], NachtmusikPalette.MidnightsCrescendoBlade[5], stackProgress).ToVector3() * (0.5f + stackProgress * 0.4f));
        }

        #endregion

        #region Custom VFX -- Shader-driven CrescendoRise trail + crescendo-scaling bloom

        protected override void DrawCustomVFX(SpriteBatch sb)
        {
            if (Progression < 0.08f || Progression > 0.92f) return;

            Vector2 tipPos = GetBladeTipPosition();
            float stackProgress = GetCrescendoStackProgress();
            int stacks = GetCrescendoStacks();
            float phaseIntensity = 1f + ComboStep * 0.15f + stackProgress * 0.5f;
            float time = (float)Main.timeForVisualEffects * 0.03f;

            // ==============================================================
            //  LAYER 1: NK Constellation Noise trail -- crescendo-scaling
            //  Uses NKConstellationNoise for star-field texture overlay.
            //  Width and brightness rise from whisper (0 stacks) to storm (15).
            // ==============================================================
            {
                int trailCount = BuildTrailPositions();
                if (trailCount > 2)
                {
                    var trailPositions = new Vector2[trailCount];
                    Array.Copy(_trailPosBuffer, trailPositions, trailCount);

                    Texture2D constellationNoise = NachtmusikThemeTextures.NKConstellationNoise?.Value;

                    float baseWidth = 8f + ComboStep * 2f;
                    float stackWidth = stackProgress * 10f;
                    float bodyWidth = (baseWidth + stackWidth) * phaseIntensity;

                    Color bodyColor = Color.Lerp(NachtmusikPalette.DeepBlue, NachtmusikPalette.Violet, stackProgress) * (0.4f + stackProgress * 0.3f);
                    Color coreColor = Color.Lerp(NachtmusikPalette.Violet, NachtmusikPalette.StarWhite, stackProgress) * (0.3f + stackProgress * 0.4f);

                    CalamityStyleTrailRenderer.DrawDualLayerTrail(
                        trailPositions, null, CalamityStyleTrailRenderer.TrailStyle.Cosmic,
                        bodyWidth, bodyColor, coreColor,
                        phaseIntensity * (0.5f + stackProgress * 0.4f),
                        bodyOverbright: 2.5f + stackProgress * 2f,
                        coreOverbright: 4f + stackProgress * 3f,
                        coreWidthRatio: 0.3f + stackProgress * 0.1f,
                        noiseTextureOverride: constellationNoise);
                }
            }

            // ==============================================================
            //  LAYER 2: CrescendoRise shader -- intensity aura at blade tip
            //  Per-weapon shader driven by stackProgress as crescendoLevel.
            //  Invisible at 0 stacks, blazing stellar aura at max.
            // ==============================================================
            if (NachtmusikShaderManager.HasCrescendoRise && stackProgress > 0.1f)
            {
                Texture2D glowTex = MagnumTextureRegistry.GetSoftGlow();
                if (glowTex != null)
                {
                    float auraPulse = 0.7f + 0.3f * MathF.Sin(Progression * MathHelper.Pi * 3f + stacks * 0.3f);
                    float auraScale = (0.15f + stackProgress * 0.25f + ComboStep * 0.04f) * auraPulse * phaseIntensity;
                    float auraOpacity = MathHelper.Clamp((Progression - 0.1f) / 0.1f, 0f, 1f)
                                      * MathHelper.Clamp((0.9f - Progression) / 0.1f, 0f, 1f)
                                      * stackProgress;

                    Vector2 tipScreen = tipPos - Main.screenPosition;
                    Vector2 glowOrigin = glowTex.Size() / 2f;

                    NachtmusikShaderManager.BeginShaderAdditive(sb);

                    // Main crescendo aura at tip -- shader intensity follows stacks
                    NachtmusikShaderManager.ApplyCrescendoRise(time, stackProgress);
                    Color auraColor = Color.Lerp(NachtmusikPalette.MidnightsCrescendoBlade[1], NachtmusikPalette.MidnightsCrescendoBlade[3], stackProgress) with { A = 0 } * auraOpacity * 0.6f;
                    sb.Draw(glowTex, tipScreen, null, auraColor, 0f,
                        glowOrigin, auraScale, SpriteEffects.None, 0f);

                    // Inner core glow pass -- tighter, hotter
                    NachtmusikShaderManager.ApplyCrescendoRiseGlow(time, stackProgress);
                    Color coreGlow = Color.Lerp(NachtmusikPalette.MidnightsCrescendoBlade[3], NachtmusikPalette.MidnightsCrescendoBlade[5], stackProgress) with { A = 0 } * auraOpacity * 0.4f;
                    sb.Draw(glowTex, tipScreen, null, coreGlow, 0f,
                        glowOrigin, auraScale * 0.5f, SpriteEffects.None, 0f);

                    // At high stacks, NKEnergySurgeBeam directional flare along blade
                    if (stackProgress > 0.5f)
                    {
                        Texture2D surgeTex = NachtmusikThemeTextures.NKEnergySurgeBeam?.Value;
                        if (surgeTex != null)
                        {
                            Vector2 surgeOrigin = new Vector2(surgeTex.Width * 0.5f, surgeTex.Height * 0.5f);
                            float surgeAlpha = (stackProgress - 0.5f) * 2f * auraOpacity;
                            float surgeScale = 0.12f + stackProgress * 0.08f;
                            Color surgeColor = NachtmusikPalette.MidnightsCrescendoBlade[2] with { A = 0 } * surgeAlpha * 0.35f;
                            sb.Draw(surgeTex, tipScreen, null, surgeColor, SwordRotation,
                                surgeOrigin, new Vector2(surgeScale * 1.5f, surgeScale * 0.4f), SpriteEffects.None, 0f);
                        }

                        // Second aura ring at blade midpoint
                        Vector2 midPos = Vector2.Lerp(Owner.MountedCenter, tipPos, 0.5f) - Main.screenPosition;
                        Color midAura = NachtmusikPalette.MidnightsCrescendoBlade[2] with { A = 0 } * auraOpacity * 0.25f * (stackProgress - 0.5f) * 2f;
                        NachtmusikShaderManager.ApplyCrescendoRiseGlow(time, stackProgress * 0.7f);
                        sb.Draw(glowTex, midPos, null, midAura, SwordRotation * 0.3f,
                            glowOrigin, auraScale * 0.6f, SpriteEffects.None, 0f);
                    }

                    NachtmusikShaderManager.RestoreSpriteBatch(sb);
                }
            }

            // ==============================================================
            //  LAYER 3: Particles -- crescendo-scaling dust + sparkles
            //  Sparse at 0 stacks, dense cosmic storm at 15.
            // ==============================================================

            int trailDustCount = 1 + ComboStep + (int)(stackProgress * 4);
            for (int i = 0; i < trailDustCount; i++)
            {
                float dp = Main.rand.NextFloat();
                Color dc = Color.Lerp(NachtmusikPalette.MidnightsCrescendoBlade[1], Color.Lerp(NachtmusikPalette.MidnightsCrescendoBlade[3], NachtmusikPalette.MidnightsCrescendoBlade[5], stackProgress), dp);
                float bladeProg = Main.rand.NextFloat(0.3f + stackProgress * 0.1f, 1f);
                Vector2 dustPos = Vector2.Lerp(Owner.MountedCenter, tipPos, bladeProg);
                float spreadWidth = 1f + stackProgress * 2f;
                Dust d = Dust.NewDustPerfect(dustPos, DustID.PurpleTorch,
                    -SwordDirection * Main.rand.NextFloat(1f, 2.5f) + Main.rand.NextVector2Circular(spreadWidth, spreadWidth),
                    0, dc, (1.1f + stackProgress * 0.5f) * phaseIntensity);
                d.noGravity = true;
                d.fadeIn = 1.2f;
            }

            // Star sparkle accents -- frequency rises with stacks
            int sparkleChance = Math.Max(1, 4 - (int)(stackProgress * 3));
            if (Main.rand.NextBool(sparkleChance))
            {
                float sparkleRadius = 8f + stackProgress * 8f;
                Vector2 sparklePos = tipPos + Main.rand.NextVector2Circular(sparkleRadius, sparkleRadius);
                Color sparkleColor = Color.Lerp(NachtmusikPalette.MidnightsCrescendoBlade[3], NachtmusikPalette.MidnightsCrescendoBlade[5], stackProgress);
                Dust star = Dust.NewDustPerfect(sparklePos, DustID.GoldFlame,
                    Main.rand.NextVector2Circular(2f, 2f), 0, sparkleColor, 0.7f + stackProgress * 0.4f);
                star.noGravity = true;
            }

            // Music notes -- crescendo serenade
            int noteChance = Math.Max(2, 6 - (int)(stackProgress * 4));
            if (Main.rand.NextBool(noteChance))
            {
                Vector2 noteVel = -SwordDirection * 1.5f + new Vector2(0, -0.5f);
                MagnumParticleHandler.SpawnParticle(new HueShiftingMusicNoteParticle(
                    tipPos, noteVel,
                    hueMin: 0.58f, hueMax: 0.72f + stackProgress * 0.05f,
                    saturation: 0.7f, luminosity: 0.6f + stackProgress * 0.15f,
                    scale: 0.75f + stackProgress * 0.15f, lifetime: 26, hueSpeed: 0.025f));
            }

            // Stellar sparkle storm at max stacks (15)
            if (stacks >= 15)
            {
                for (int i = 0; i < 3; i++)
                {
                    float bladeProg = Main.rand.NextFloat(0.2f, 1f);
                    Vector2 stormPos = Vector2.Lerp(Owner.MountedCenter, tipPos, bladeProg);
                    Vector2 stormVel = Main.rand.NextVector2Circular(4f, 4f) + -SwordDirection * 2f;
                    Color stormColor = Color.Lerp(NachtmusikPalette.MidnightsCrescendoBlade[4], NachtmusikPalette.MidnightsCrescendoBlade[5], Main.rand.NextFloat());
                    Dust storm = Dust.NewDustPerfect(stormPos, DustID.GoldFlame, stormVel, 0, stormColor, 1.1f);
                    storm.noGravity = true;
                }

                if (Main.rand.NextBool(2))
                    NachtmusikVFXLibrary.SpawnTwinklingStars(tipPos, 1, 20f);

                if (Main.rand.NextBool(3))
                {
                    Vector2 cascadeVel = -SwordDirection * 2f + new Vector2(Main.rand.NextFloat(-1f, 1f), -1.5f);
                    MagnumParticleHandler.SpawnParticle(new HueShiftingMusicNoteParticle(
                        tipPos, cascadeVel,
                        hueMin: 0.55f, hueMax: 0.75f,
                        saturation: 0.8f, luminosity: 0.8f,
                        scale: 0.9f, lifetime: 30, hueSpeed: 0.03f));
                }
            }

            // ==============================================================
            //  LAYER 4: Crescendo bloom -- 3-layer + NK Lens Flare accent
            //  Scales from faint indigo glow to blinding stellar core.
            // ==============================================================
            {
                float bloomOpacity = MathHelper.Clamp((Progression - 0.08f) / 0.12f, 0f, 1f)
                                   * MathHelper.Clamp((0.92f - Progression) / 0.12f, 0f, 1f);
                float bloomScale = 0.35f + ComboStep * 0.08f + stackProgress * 0.25f;
                float pulse = 0.85f + 0.15f * MathF.Sin(time * 5f + stacks * 0.2f);

                Texture2D bloomTex = MagnumTextureRegistry.GetSoftGlow();
                if (bloomTex != null && bloomOpacity > 0.01f)
                {
                    Vector2 tipScreen = tipPos - Main.screenPosition;
                    Vector2 bloomOrigin = bloomTex.Size() / 2f;

                    SwingShaderSystem.BeginAdditive(sb);

                    // Outer indigo halo -- widens with stacks
                    Color outerColor = Color.Lerp(NachtmusikPalette.MidnightsCrescendoBlade[0], NachtmusikPalette.MidnightsCrescendoBlade[1], stackProgress) with { A = 0 };
                    sb.Draw(bloomTex, tipScreen, null, outerColor * bloomOpacity * (0.15f + stackProgress * 0.15f),
                        0f, bloomOrigin, bloomScale * (0.42f + stackProgress * 0.18f) * pulse, SpriteEffects.None, 0f);

                    // Mid cosmic blue glow
                    Color midColor = Color.Lerp(NachtmusikPalette.MidnightsCrescendoBlade[1], NachtmusikPalette.MidnightsCrescendoBlade[2], stackProgress) with { A = 0 };
                    sb.Draw(bloomTex, tipScreen, null, midColor * bloomOpacity * (0.25f + stackProgress * 0.15f),
                        SwordRotation * 0.3f, bloomOrigin, bloomScale * (0.3f + stackProgress * 0.1f) * pulse, SpriteEffects.None, 0f);

                    // Inner starlight core -- white-hot at max
                    Color innerColor = Color.Lerp(NachtmusikPalette.MidnightsCrescendoBlade[3], NachtmusikPalette.MidnightsCrescendoBlade[5], stackProgress) with { A = 0 };
                    sb.Draw(bloomTex, tipScreen, null, innerColor * bloomOpacity * (0.2f + stackProgress * 0.25f),
                        0f, bloomOrigin, bloomScale * (0.25f + stackProgress * 0.1f), SpriteEffects.None, 0f);

                    // NK Lens Flare accent -- visible above 20% stacks
                    if (stackProgress > 0.2f)
                    {
                        Texture2D flareTex = NachtmusikThemeTextures.NKLensFlare?.Value;
                        if (flareTex != null)
                        {
                            Vector2 flareOrigin = flareTex.Size() / 2f;
                            float flareRot = time * 0.5f;
                            float flareAlpha = (stackProgress - 0.2f) / 0.8f;
                            sb.Draw(flareTex, tipScreen, null,
                                Color.Lerp(NachtmusikPalette.MidnightsCrescendoBlade[2], NachtmusikPalette.MidnightsCrescendoBlade[4], stackProgress) with { A = 0 } * bloomOpacity * flareAlpha * 0.35f,
                                flareRot, flareOrigin, bloomScale * (0.06f + stackProgress * 0.06f) * pulse, SpriteEffects.None, 0f);
                        }
                    }

                    // NK Power Ring on combo finisher phases
                    if (ComboStep >= 1 && stackProgress > 0.3f)
                    {
                        Texture2D ringTex = NachtmusikThemeTextures.NKPowerEffectRing?.Value;
                        if (ringTex != null)
                        {
                            Vector2 ringOrigin = ringTex.Size() / 2f;
                            float ringAlpha = (stackProgress - 0.3f) / 0.7f * MathHelper.Clamp((Progression - 0.15f) / 0.15f, 0f, 1f);
                            float ringScale = bloomScale * (0.1f + stackProgress * 0.06f) * pulse;
                            Color ringColor = NachtmusikPalette.MidnightsCrescendoBlade[3] with { A = 0 } * bloomOpacity * ringAlpha * 0.3f;
                            sb.Draw(ringTex, tipScreen, null, ringColor, -time * 0.8f,
                                ringOrigin, ringScale, SpriteEffects.None, 0f);
                        }
                    }

                    SwingShaderSystem.RestoreSpriteBatch(sb);
                }
            }
        }

        #endregion
    }
}
