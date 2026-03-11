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
using MagnumOpus.Common.Systems.Shaders;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.VFX.Trails;
using MagnumOpus.Content.Nachtmusik;
using MagnumOpus.Content.Nachtmusik.Debuffs;
using MagnumOpus.Content.Nachtmusik.Weapons.TwilightSeverance.Utilities;
using static MagnumOpus.Common.Systems.Particles.Particle;
using ReLogic.Content;

namespace MagnumOpus.Content.Nachtmusik.Weapons.TwilightSeverance.Projectiles
{
    /// <summary>
    /// Swing projectile for Twilight Severance — ultra-fast Nachtmusik katana.
    /// 3-phase razor combo: Dusk Diagonal → Dawn Reverse → Twilight Horizon.
    /// Fastest melee in Nachtmusik. Tight arcs, precise cuts, minimal wind-up.
    /// Every phase at 50% fires perpendicular slash pair. Phase 2 adds ground impact flash.
    /// Uses ultra-thin indigo trails with silver sparkle accents — speed over weight.
    /// </summary>
    public sealed class TwilightSeveranceSwing : MeleeSwingBase
    {

        #region Combo Phases — Ultra-fast katana precision

        // Phase 0: Dusk Diagonal — fast descending diagonal cut
        private static readonly ComboPhase Phase0_DuskDiagonal = new ComboPhase(
            curves: new CurveSegment[]
            {
                new CurveSegment(EasingType.PolyOut, 0f, -0.7f, 0.15f, 2),
                new CurveSegment(EasingType.PolyIn, 0.18f, -0.55f, 1.35f, 3),
                new CurveSegment(EasingType.PolyOut, 0.85f, 0.8f, 0.08f, 2)
            },
            maxAngle: MathHelper.PiOver2 * 1.3f,
            duration: 16,
            bladeLength: 145f,
            flip: false,
            squish: 0.95f,
            damageMult: 0.9f
        );

        // Phase 1: Dawn Reverse — quick reverse upward slash
        private static readonly ComboPhase Phase1_DawnReverse = new ComboPhase(
            curves: new CurveSegment[]
            {
                new CurveSegment(EasingType.PolyOut, 0f, -0.75f, 0.12f, 2),
                new CurveSegment(EasingType.PolyIn, 0.15f, -0.63f, 1.43f, 3),
                new CurveSegment(EasingType.PolyOut, 0.85f, 0.8f, 0.08f, 2)
            },
            maxAngle: MathHelper.PiOver2 * 1.3f,
            duration: 16,
            bladeLength: 145f,
            flip: true,
            squish: 0.95f,
            damageMult: 1.0f
        );

        // Phase 2: Twilight Horizon — slightly longer horizontal finisher
        private static readonly ComboPhase Phase2_TwilightHorizon = new ComboPhase(
            curves: new CurveSegment[]
            {
                new CurveSegment(EasingType.PolyOut, 0f, -0.8f, 0.12f, 2),
                new CurveSegment(EasingType.PolyIn, 0.15f, -0.68f, 1.58f, 3),
                new CurveSegment(EasingType.PolyOut, 0.85f, 0.9f, 0.06f, 2)
            },
            maxAngle: MathHelper.PiOver2 * 1.35f,
            duration: 20,
            bladeLength: 150f,
            flip: false,
            squish: 0.95f,
            damageMult: 1.15f
        );

        #endregion

        #region Abstract Overrides

        protected override ComboPhase[] GetAllPhases() => new ComboPhase[]
        {
            Phase0_DuskDiagonal,
            Phase1_DawnReverse,
            Phase2_TwilightHorizon
        };

        protected override Color[] GetPalette() => NachtmusikPalette.TwilightSeveranceBlade;

        protected override CalamityStyleTrailRenderer.TrailStyle GetTrailStyle()
            => CalamityStyleTrailRenderer.TrailStyle.Cosmic;

        protected override string GetSmearTexturePath(int comboStep) => comboStep switch
        {
            1 => "MagnumOpus/Assets/VFX Asset Library/MasksAndShapes/VerticalEllipse",
            2 => "MagnumOpus/Assets/VFX Asset Library/ImpactEffects/ImpactEllipse",
            _ => "MagnumOpus/Assets/VFX Asset Library/MasksAndShapes/WideSoftEllipse"
        };

        protected override string GetSmearGradientPath() => "MagnumOpus/Assets/VFX Asset Library/ColorGradients/NachtmusikGradientLUTandRAMP";

        #endregion

        #region Virtual Overrides

        protected override Texture2D GetBladeTexture()
            => ModContent.Request<Texture2D>("MagnumOpus/Content/Nachtmusik/Weapons/TwilightSeverance/TwilightSeverance", AssetRequestMode.ImmediateLoad).Value;

        protected override SoundStyle GetSwingSound()
            => SoundID.Item71 with { Pitch = 0.15f + ComboStep * 0.15f, Volume = 0.75f };

        protected override int GetInitialDustType() => DustID.PurpleTorch;

        protected override int GetSecondaryDustType() => DustID.Enchanted_Gold;

        protected override Vector3 GetLightColor()
        {
            float intensity = 0.4f + ComboStep * 0.12f;
            Color c = Color.Lerp(NachtmusikPalette.DuskViolet, NachtmusikPalette.MoonlitSilver, Progression);
            return c.ToVector3() * intensity;
        }

        #endregion

        #region Combo Specials — Perpendicular blade waves at 50% progression

        protected override void HandleComboSpecials()
        {
            if (hasSpawnedSpecial) return;

            // Every phase at 50%: fire perpendicular TwilightSlashProjectile pair
            if (Progression >= 0.50f)
            {
                hasSpawnedSpecial = true;

                if (Main.myPlayer == Projectile.owner)
                {
                    Vector2 tip = GetBladeTipPosition();
                    Vector2 dir = SwordDirection.SafeNormalize(Vector2.UnitX);

                    for (int side = -1; side <= 1; side += 2)
                    {
                        Vector2 perpVel = dir.RotatedBy(MathHelper.PiOver2 * side) * 12f;
                        Projectile.NewProjectile(Projectile.GetSource_FromThis(), tip, perpVel,
                            ModContent.ProjectileType<TwilightSlashProjectile>(),
                            (int)(Projectile.damage * 0.4f), 2f, Projectile.owner, ai0: 0f);
                    }
                }

                Vector2 vfxTip = GetBladeTipPosition();
                TwilightSeveranceVFX.PerpendicularSlashVFX(vfxTip);

                // Palette-ramped sparkles at slash tip
                NachtmusikVFXLibrary.SpawnGradientSparkles(vfxTip, SwordDirection, 2 + ComboStep, 0.25f + ComboStep * 0.05f, 16, 6f);

                // Phase 2: additional ground impact flash
                if (ComboStep == 2)
                {
                    TwilightSeveranceVFX.SwingImpactVFX(vfxTip, ComboStep);
                    MagnumScreenEffects.AddScreenShake(4f);
                }
            }
        }

        #endregion

        #region On Hit — CelestialHarmony + Twilight Charge build

        protected override void OnSwingHitNPC(NPC target, NPC.HitInfo hit, int remainingDamageCount)
        {
            // Apply Celestial Harmony debuff
            target.AddBuff(ModContent.BuffType<CelestialHarmony>(), 360);
            if (target.TryGetGlobalNPC(out CelestialHarmonyNPC harmonyNPC))
                harmonyNPC.AddStack(target, 1);

            // Build twilight charge on owning item
            if (Main.myPlayer == Projectile.owner)
            {
                Player player = Main.player[Projectile.owner];
                if (player.HeldItem?.ModItem is TwilightSeverance katana)
                {
                    katana.TwilightCharge += hit.Crit ? 8 : 5;
                }
            }

            // Katana-style sharp impact VFX — thin lines, precise
            TwilightSeveranceVFX.SwingImpactVFX(target.Center, ComboStep);

            // Thin star slash marks at impact
            for (int ring = 0; ring < 1 + ComboStep; ring++)
            {
                float p = (float)ring / (1 + ComboStep);
                Color ringColor = Color.Lerp(NachtmusikPalette.DuskViolet, NachtmusikPalette.MoonlitSilver, p);
                CustomParticles.HaloRing(target.Center, ringColor, 0.2f + ring * 0.08f, 10 + ring * 2);
            }

            // Radial cosmic dust — tight, precise burst (katana style)
            int dustCount = 6 + ComboStep * 3;
            for (int i = 0; i < dustCount; i++)
            {
                float angle = MathHelper.TwoPi * i / dustCount;
                float dp = (float)i / dustCount;
                Color dc = Color.Lerp(NachtmusikPalette.DuskViolet, NachtmusikPalette.MoonlitSilver, dp);
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 5f);
                Dust d = Dust.NewDustPerfect(target.Center, DustID.PurpleTorch, vel, 0, dc, 1.0f);
                d.noGravity = true;
            }

            // Silver sparkle accents
            NachtmusikVFXLibrary.SpawnTwinklingStars(target.Center, 1 + ComboStep, 18f);
            NachtmusikVFXLibrary.SpawnMusicNotes(target.Center, 1 + ComboStep, 18f, 0.5f, 0.7f, 22);

            // Palette-ramped sparkle explosion on impact
            NachtmusikVFXLibrary.SpawnGradientSparkleExplosion(target.Center, 6 + ComboStep * 3, 4f + ComboStep, 0.3f);

            Lighting.AddLight(target.Center, NachtmusikPalette.TwilightSeveranceBlade[2].ToVector3() * 0.6f);
        }

        #endregion

        #region Custom VFX — DimensionalRift shader trail + precision bloom

        protected override void DrawCustomVFX(SpriteBatch sb)
        {
            if (Progression < 0.08f || Progression > 0.92f) return;

            Vector2 tipPos = GetBladeTipPosition();
            float phaseIntensity = 1f + ComboStep * 0.15f;
            float time = (float)Main.timeForVisualEffects * 0.03f;

            // Smooth fade envelope
            float vfxOpacity = MathHelper.Clamp((Progression - 0.08f) / 0.1f, 0f, 1f)
                             * MathHelper.Clamp((0.92f - Progression) / 0.1f, 0f, 1f);

            // ═══════════════════════════════════════════════════════════════
            //  LAYER 1: NK-textured Trail Overlay — Harmonic Standing Wave
            //  Uses NKHarmonicRibbon noise to create a ribbed standing-wave
            //  trail pattern unique to Twilight Severance. Thin and sharp.
            // ═══════════════════════════════════════════════════════════════
            {
                int trailCount = BuildTrailPositions();
                if (trailCount > 2)
                {
                    var trailPositions = new Vector2[trailCount];
                    Array.Copy(_trailPosBuffer, trailPositions, trailCount);

                    Texture2D ribbonNoise = NachtmusikThemeTextures.NKHarmonicRibbon?.Value;

                    // Thin trail — katana precision, not greatsword bulk
                    float trailWidth = (8f + ComboStep * 2f) * phaseIntensity;
                    CalamityStyleTrailRenderer.DrawDualLayerTrail(
                        trailPositions, null, CalamityStyleTrailRenderer.TrailStyle.Cosmic,
                        trailWidth,
                        NachtmusikPalette.DuskViolet * 0.6f,
                        NachtmusikPalette.MoonlitSilver * 0.5f,
                        phaseIntensity * 0.6f,
                        bodyOverbright: 3.0f, coreOverbright: 5f, coreWidthRatio: 0.25f,
                        noiseTextureOverride: ribbonNoise);
                }
            }

            // ═══════════════════════════════════════════════════════════════
            //  LAYER 2: DimensionalRift Shader — Tip Shimmer
            //  Per-weapon shader applied to glow sprites at blade tip.
            //  Creates dimensional tear shimmer with twilight flicker.
            // ═══════════════════════════════════════════════════════════════
            if (NachtmusikShaderManager.HasDimensionalRift)
            {
                Texture2D glowTex = MagnumTextureRegistry.GetSoftGlow();
                if (glowTex != null)
                {
                    NachtmusikShaderManager.BeginShaderAdditive(sb);
                    NachtmusikShaderManager.ApplyDimensionalRift(time);

                    Vector2 tipScreen = tipPos - Main.screenPosition;
                    Vector2 glowOrigin = glowTex.Size() / 2f;

                    // Elongated rift along sword direction — the dimensional tear
                    float riftScale = 0.15f + ComboStep * 0.03f;
                    float riftPulse = 0.88f + 0.12f * MathF.Sin(Progression * MathHelper.Pi * 6f);
                    Color riftColor = NachtmusikPalette.DuskViolet with { A = 0 } * vfxOpacity * 0.5f;

                    sb.Draw(glowTex, tipScreen, null, riftColor,
                        SwordRotation, glowOrigin,
                        new Vector2(riftScale * 1.6f, riftScale * 0.5f) * riftPulse,
                        SpriteEffects.None, 0f);

                    // Inner core rift — tighter, brighter
                    Color coreRiftColor = NachtmusikPalette.MoonlitSilver with { A = 0 } * vfxOpacity * 0.35f;
                    sb.Draw(glowTex, tipScreen, null, coreRiftColor,
                        SwordRotation, glowOrigin,
                        new Vector2(riftScale * 0.8f, riftScale * 0.25f) * riftPulse,
                        SpriteEffects.None, 0f);

                    // DimensionalRift glow pass on the NK radial slash texture at 50%+ progression
                    if (Progression > 0.4f && ComboStep >= 1)
                    {
                        NachtmusikShaderManager.ApplyDimensionalRiftGlow(time);
                        Texture2D slashTex = NachtmusikThemeTextures.NKRadialSlashStar?.Value;
                        if (slashTex != null)
                        {
                            float slashOpacity = MathHelper.Clamp((Progression - 0.4f) / 0.15f, 0f, 1f)
                                               * MathHelper.Clamp((0.85f - Progression) / 0.15f, 0f, 1f);
                            float slashScale = 0.06f + ComboStep * 0.015f;
                            Color slashColor = NachtmusikPalette.TwilightSeveranceBlade[3] with { A = 0 } * slashOpacity * 0.3f;
                            sb.Draw(slashTex, tipScreen, null, slashColor,
                                SwordRotation * 0.5f + time, slashTex.Size() / 2f,
                                slashScale, SpriteEffects.None, 0f);
                        }
                    }

                    NachtmusikShaderManager.RestoreSpriteBatch(sb);
                }
            }
            else if (NachtmusikShaderManager.HasSerenade)
            {
                // Fallback: Serenade shader if DimensionalRift unavailable
                Texture2D glowTex = MagnumTextureRegistry.GetSoftGlow();
                if (glowTex != null)
                {
                    float auraPulse = 0.88f + 0.12f * MathF.Sin(Progression * MathHelper.Pi * 6f);
                    float auraScale = (0.12f + ComboStep * 0.03f) * auraPulse;

                    NachtmusikShaderManager.BeginShaderAdditive(sb);
                    NachtmusikShaderManager.ApplySerenade(time, NachtmusikPalette.DuskViolet,
                        NachtmusikPalette.MoonlitSilver, phase: Progression);

                    Vector2 tipScreen = tipPos - Main.screenPosition;
                    Color auraColor = NachtmusikPalette.DuskViolet with { A = 0 } * vfxOpacity * 0.5f;
                    sb.Draw(glowTex, tipScreen, null, auraColor, SwordRotation,
                        glowTex.Size() / 2f, auraScale, SpriteEffects.None, 0f);

                    NachtmusikShaderManager.RestoreSpriteBatch(sb);
                }
            }

            // ═══════════════════════════════════════════════════════════════
            //  LAYER 3: Directional Speed Dust — razor-thin katana identity
            //  Fast backward streaks, not diffuse clouds.
            // ═══════════════════════════════════════════════════════════════
            if (Main.rand.NextBool(3))
            {
                float dp = Main.rand.NextFloat();
                Color lineColor = Color.Lerp(NachtmusikPalette.DuskViolet, NachtmusikPalette.MoonlitSilver, dp);
                Vector2 dustPos = Vector2.Lerp(Owner.MountedCenter, tipPos, Main.rand.NextFloat(0.6f, 1f));
                Vector2 lineVel = -SwordDirection * Main.rand.NextFloat(2f, 4f);
                Dust d = Dust.NewDustPerfect(dustPos, DustID.PurpleTorch, lineVel, 0, lineColor, 0.7f);
                d.noGravity = true;
                d.fadeIn = 0.8f;
            }

            // Music notes — sparse, fast katana tempo
            if (Main.rand.NextBool(8))
            {
                Vector2 noteVel = -SwordDirection * 1.2f + new Vector2(0, -0.4f);
                MagnumParticleHandler.SpawnParticle(new HueShiftingMusicNoteParticle(
                    tipPos, noteVel,
                    hueMin: 0.58f, hueMax: 0.72f,
                    saturation: 0.65f, luminosity: 0.6f,
                    scale: 0.6f, lifetime: 22, hueSpeed: 0.025f));
            }

            // ═══════════════════════════════════════════════════════════════
            //  LAYER 4: Precision Bloom — tight core + NK lens flare
            //  No wide atmospheric padding. Sharp, focused, katana-clean.
            // ═══════════════════════════════════════════════════════════════
            {
                float bloomScale = 0.22f + ComboStep * 0.03f;
                float pulse = 0.92f + 0.08f * MathF.Sin(time * 7f);

                Texture2D bloomTex = MagnumTextureRegistry.GetSoftGlow();
                if (bloomTex != null && vfxOpacity > 0.01f)
                {
                    Vector2 tipScreen = tipPos - Main.screenPosition;
                    Vector2 bloomOrigin = bloomTex.Size() / 2f;

                    SwingShaderSystem.BeginAdditive(sb);

                    // Silver core glow (tight)
                    sb.Draw(bloomTex, tipScreen, null,
                        NachtmusikPalette.MoonlitSilver with { A = 0 } * vfxOpacity * 0.4f,
                        0f, bloomOrigin, bloomScale * 0.3f * pulse, SpriteEffects.None, 0f);

                    // White-hot center (razor point)
                    sb.Draw(bloomTex, tipScreen, null,
                        Color.White with { A = 0 } * vfxOpacity * 0.25f,
                        0f, bloomOrigin, bloomScale * 0.15f, SpriteEffects.None, 0f);

                    // NK Lens Flare — weapon-specific flare instead of generic radial bloom
                    Texture2D flareTex = NachtmusikThemeTextures.NKLensFlare?.Value
                                      ?? MagnumTextureRegistry.GetStar4Soft();
                    if (flareTex != null)
                    {
                        Vector2 flareOrigin = flareTex.Size() / 2f;
                        float flareRot = time * 2f;
                        float flareScale = bloomScale * 0.08f * pulse;
                        sb.Draw(flareTex, tipScreen, null,
                            NachtmusikPalette.MoonlitSilver with { A = 0 } * vfxOpacity * 0.45f,
                            flareRot, flareOrigin, flareScale, SpriteEffects.None, 0f);
                    }

                    SwingShaderSystem.RestoreSpriteBatch(sb);
                }
            }
        }

        #endregion
    }
}
