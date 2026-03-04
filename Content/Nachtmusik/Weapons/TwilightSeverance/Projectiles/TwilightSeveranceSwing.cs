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
        #region Theme Colors — Night Void → Deep Indigo → Cosmic Blue → Starlight Silver

        private static readonly Color NightVoid = new Color(10, 10, 30);
        private static readonly Color DeepIndigo = new Color(40, 30, 100);
        private static readonly Color CosmicBlue = new Color(60, 80, 180);
        private static readonly Color StarlightSilver = new Color(180, 200, 230);
        private static readonly Color MoonPearl = new Color(220, 225, 245);
        private static readonly Color StellarWhite = new Color(240, 245, 255);

        /// <summary>
        /// 6-stop palette: Night Void → Deep Indigo → Cosmic Blue → Starlight Silver → Moon Pearl → Stellar White.
        /// Katana precision — thinner, sharper gradient than Executioner's heavy cosmic palette.
        /// </summary>
        private static readonly Color[] SeverancePalette = new Color[]
        {
            NightVoid,          // [0] Pianissimo — void edge
            DeepIndigo,         // [1] Piano — dimensional violet
            CosmicBlue,         // [2] Mezzo — cosmic blue core
            StarlightSilver,    // [3] Forte — starlight edge
            MoonPearl,          // [4] Fortissimo — moon pearl flash
            StellarWhite        // [5] Sforzando — stellar white peak
        };

        #endregion

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

        protected override Color[] GetPalette() => SeverancePalette;

        protected override CalamityStyleTrailRenderer.TrailStyle GetTrailStyle()
            => CalamityStyleTrailRenderer.TrailStyle.Cosmic;

        protected override string GetSmearTexturePath(int comboStep) => comboStep switch
        {
            1 => "MagnumOpus/Assets/VFX Asset Library/MasksAndShapes/VerticalEllipse",
            2 => "MagnumOpus/Assets/VFX Asset Library/ImpactEffects/ImpactEllipse",
            _ => "MagnumOpus/Assets/VFX Asset Library/MasksAndShapes/WideSoftEllipse"
        };

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
            Color c = Color.Lerp(DeepIndigo, StarlightSilver, Progression);
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
                Color ringColor = Color.Lerp(DeepIndigo, StarlightSilver, p);
                CustomParticles.HaloRing(target.Center, ringColor, 0.2f + ring * 0.08f, 10 + ring * 2);
            }

            // Radial cosmic dust — tight, precise burst (katana style)
            int dustCount = 6 + ComboStep * 3;
            for (int i = 0; i < dustCount; i++)
            {
                float angle = MathHelper.TwoPi * i / dustCount;
                float dp = (float)i / dustCount;
                Color dc = Color.Lerp(DeepIndigo, StarlightSilver, dp);
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 5f);
                Dust d = Dust.NewDustPerfect(target.Center, DustID.PurpleTorch, vel, 0, dc, 1.0f);
                d.noGravity = true;
            }

            // Silver sparkle accents
            NachtmusikVFXLibrary.SpawnTwinklingStars(target.Center, 1 + ComboStep, 18f);
            NachtmusikVFXLibrary.SpawnMusicNotes(target.Center, 1 + ComboStep, 18f, 0.5f, 0.7f, 22);

            Lighting.AddLight(target.Center, CosmicBlue.ToVector3() * 0.6f);
        }

        #endregion

        #region Custom VFX — Shader-driven dimensional rift trail + precision bloom

        protected override void DrawCustomVFX(SpriteBatch sb)
        {
            if (Progression < 0.08f || Progression > 0.92f) return;

            Vector2 tipPos = GetBladeTipPosition();
            float phaseIntensity = 1f + ComboStep * 0.15f;
            float time = (float)Main.timeForVisualEffects * 0.03f;

            // ═══════════════════════════════════════════════════════════════
            //  SHADER LAYER 1: DimensionalRift — per-weapon trail overlay
            //  Ultra-sharp dimensional tear VFX unique to Twilight Severance
            // ═══════════════════════════════════════════════════════════════
            if (NachtmusikShaderManager.HasDimensionalRift || NachtmusikShaderManager.HasStarTrail)
            {
                int trailCount = BuildTrailPositions();
                if (trailCount > 2)
                {
                    var trailPositions = new Vector2[trailCount];
                    Array.Copy(_trailPosBuffer, trailPositions, trailCount);

                    // Glow underlayer — wide, soft dimensional shimmer
                    float glowWidth = (10f + ComboStep * 3f) * phaseIntensity;
                    CalamityStyleTrailRenderer.DrawDualLayerTrail(
                        trailPositions, null, CalamityStyleTrailRenderer.TrailStyle.Cosmic,
                        glowWidth, NachtmusikPalette.DuskViolet * 0.5f, NachtmusikPalette.MoonlitSilver * 0.4f,
                        phaseIntensity * 0.5f, bodyOverbright: 2.5f, coreOverbright: 4f, coreWidthRatio: 0.3f);
                }
            }

            // ═══════════════════════════════════════════════════════════════
            //  SHADER LAYER 2: NachtmusikSerenade aura at blade tip
            //  Harmonic wave shimmer unique to the katana's speed identity
            // ═══════════════════════════════════════════════════════════════
            if (NachtmusikShaderManager.HasSerenade)
            {
                Texture2D auraTex = MagnumTextureRegistry.GetSoftGlow();
                if (auraTex != null)
                {
                    float auraPulse = 0.85f + 0.15f * MathF.Sin(Progression * MathHelper.Pi * 4f);
                    float auraScale = (0.18f + ComboStep * 0.04f) * auraPulse;
                    float auraOpacity = MathHelper.Clamp((Progression - 0.1f) / 0.1f, 0f, 1f)
                                      * MathHelper.Clamp((0.9f - Progression) / 0.1f, 0f, 1f);

                    NachtmusikShaderManager.BeginShaderAdditive(sb);
                    NachtmusikShaderManager.ApplySerenade(time, NachtmusikPalette.DuskViolet,
                        NachtmusikPalette.MoonlitSilver, phase: Progression);

                    Vector2 tipScreen = tipPos - Main.screenPosition;
                    Color auraColor = NachtmusikPalette.DuskViolet with { A = 0 } * auraOpacity * 0.6f;
                    sb.Draw(auraTex, tipScreen, null, auraColor, SwordRotation,
                        auraTex.Size() / 2f, auraScale, SpriteEffects.None, 0f);

                    NachtmusikShaderManager.RestoreSpriteBatch(sb);
                }
            }

            // ═══════════════════════════════════════════════════════════════
            //  PARTICLE LAYER: Precision katana dust — minimal, razor-thin
            // ═══════════════════════════════════════════════════════════════

            // Ultra-thin indigo dust trail along blade — katana precision
            if (Main.rand.NextBool(2))
            {
                float dp = Main.rand.NextFloat();
                Color dc = Color.Lerp(DeepIndigo, CosmicBlue, dp);
                Vector2 dustPos = Vector2.Lerp(Owner.MountedCenter, tipPos, Main.rand.NextFloat(0.5f, 1f));
                Dust d = Dust.NewDustPerfect(dustPos, DustID.PurpleTorch,
                    -SwordDirection * Main.rand.NextFloat(0.5f, 2f) + Main.rand.NextVector2Circular(0.5f, 0.5f),
                    0, dc, 0.9f * phaseIntensity);
                d.noGravity = true;
                d.fadeIn = 1f;
            }

            // Precise silver sparkle at blade tip — the katana's signature edge
            if (Main.rand.NextBool(2))
            {
                Vector2 sparklePos = tipPos + Main.rand.NextVector2Circular(5f, 5f);
                Dust star = Dust.NewDustPerfect(sparklePos, DustID.Enchanted_Gold,
                    Main.rand.NextVector2Circular(1.5f, 1.5f), 0, StarlightSilver, 0.65f);
                star.noGravity = true;
            }

            // Music notes — sparse, fast (katana tempo)
            if (Main.rand.NextBool(7))
            {
                Vector2 noteVel = -SwordDirection * 1.2f + new Vector2(0, -0.4f);
                MagnumParticleHandler.SpawnParticle(new HueShiftingMusicNoteParticle(
                    tipPos, noteVel,
                    hueMin: 0.58f, hueMax: 0.72f,
                    saturation: 0.65f, luminosity: 0.6f,
                    scale: 0.6f, lifetime: 22, hueSpeed: 0.025f));
            }

            // ═══════════════════════════════════════════════════════════════
            //  BLOOM LAYER: Multi-scale bloom stack at blade tip
            //  4-layer: outer dusk → mid cosmic → inner starlight → core white
            // ═══════════════════════════════════════════════════════════════
            {
                float bloomOpacity = MathHelper.Clamp((Progression - 0.08f) / 0.12f, 0f, 1f)
                                   * MathHelper.Clamp((0.92f - Progression) / 0.12f, 0f, 1f);
                float bloomScale = 0.3f + ComboStep * 0.06f;
                float pulse = 0.9f + 0.1f * MathF.Sin(time * 5f);

                Texture2D bloomTex = MagnumTextureRegistry.GetSoftGlow();
                if (bloomTex != null && bloomOpacity > 0.01f)
                {
                    Vector2 tipScreen = tipPos - Main.screenPosition;
                    Vector2 bloomOrigin = bloomTex.Size() / 2f;

                    SwingShaderSystem.BeginAdditive(sb);

                    // Layer 1: Wide dusk atmospheric halo
                    sb.Draw(bloomTex, tipScreen, null, NachtmusikPalette.DuskViolet with { A = 0 } * bloomOpacity * 0.2f,
                        0f, bloomOrigin, bloomScale * 2.2f * pulse, SpriteEffects.None, 0f);

                    // Layer 2: Mid cosmic blue glow
                    sb.Draw(bloomTex, tipScreen, null, CosmicBlue with { A = 0 } * bloomOpacity * 0.35f,
                        SwordRotation * 0.3f, bloomOrigin, bloomScale * 1.4f * pulse, SpriteEffects.None, 0f);

                    // Layer 3: Inner starlight silver
                    sb.Draw(bloomTex, tipScreen, null, StarlightSilver with { A = 0 } * bloomOpacity * 0.5f,
                        0f, bloomOrigin, bloomScale * 0.7f, SpriteEffects.None, 0f);

                    // Layer 4: White-hot core
                    sb.Draw(bloomTex, tipScreen, null, StellarWhite with { A = 0 } * bloomOpacity * 0.3f,
                        0f, bloomOrigin, bloomScale * 0.3f, SpriteEffects.None, 0f);

                    // Star flare accent (rotation gives 4-pointed star look)
                    Texture2D flareTex = MagnumTextureRegistry.GetRadialBloom();
                    if (flareTex != null)
                    {
                        Vector2 flareOrigin = flareTex.Size() / 2f;
                        float flareRot = time * 0.5f;
                        sb.Draw(flareTex, tipScreen, null, MoonPearl with { A = 0 } * bloomOpacity * 0.3f,
                            flareRot, flareOrigin, bloomScale * 0.5f * pulse, SpriteEffects.None, 0f);
                        sb.Draw(flareTex, tipScreen, null, StellarWhite with { A = 0 } * bloomOpacity * 0.15f,
                            flareRot + MathHelper.PiOver4, flareOrigin, bloomScale * 0.3f, SpriteEffects.None, 0f);
                    }

                    SwingShaderSystem.RestoreSpriteBatch(sb);
                }
            }
        }

        #endregion
    }
}
