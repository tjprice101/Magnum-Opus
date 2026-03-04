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
using MagnumOpus.Common.Systems.VFX.Trails;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Content.Nachtmusik;
using MagnumOpus.Content.Nachtmusik.Debuffs;
using MagnumOpus.Content.Nachtmusik.Weapons.NocturnalExecutioner.Utilities;
using static MagnumOpus.Common.Systems.Particles.Particle;
using ReLogic.Content;

namespace MagnumOpus.Content.Nachtmusik.Weapons.NocturnalExecutioner.Projectiles
{
    /// <summary>
    /// Swing projectile for Nocturnal Executioner — heavy cosmic greatsword.
    /// 3-phase combo: Shadow Cleave → Cosmic Divide → Stellar Execution.
    /// Phase 1 fires 3-blade fan, Phase 2 fires V-pattern + cosmic dust,
    /// Phase 3 overhead slam with ground impact shockwave.
    /// Uses FBM-distorted cosmic smear arcs with CosmicNebula noise masking.
    /// </summary>
    public sealed class NocturnalExecutionerSwing : MeleeSwingBase
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
        /// Maps to musical dynamics: Pianissimo → Sforzando.
        /// </summary>
        private static readonly Color[] ExecutionerPalette = new Color[]
        {
            NightVoid,          // [0] Pianissimo — deep void
            DeepIndigo,         // [1] Piano — cosmic purple depths
            CosmicBlue,         // [2] Mezzo — cosmic blue body
            StarlightSilver,    // [3] Forte — starlight edges
            MoonPearl,          // [4] Fortissimo — moon pearl shimmer
            StellarWhite        // [5] Sforzando — brilliant stellar core
        };

        #endregion

        #region Combo Phases — Escalating cosmic intensity

        // Phase 0: Shadow Cleave — heavy horizontal sweep, cosmic trailing energy
        private static readonly ComboPhase Phase0_ShadowCleave = new ComboPhase(
            curves: new CurveSegment[]
            {
                new CurveSegment(EasingType.PolyOut, 0f, -0.85f, 0.2f, 2),
                new CurveSegment(EasingType.PolyIn, 0.22f, -0.65f, 1.55f, 3),
                new CurveSegment(EasingType.PolyOut, 0.82f, 0.9f, 0.1f, 2)
            },
            maxAngle: MathHelper.PiOver2 * 1.5f,
            duration: 28,
            bladeLength: 160f,
            flip: false,
            squish: 0.9f,
            damageMult: 0.85f
        );

        // Phase 1: Cosmic Divide — rising uppercut, wider arc, 2 blades in V-pattern
        private static readonly ComboPhase Phase1_CosmicDivide = new ComboPhase(
            curves: new CurveSegment[]
            {
                new CurveSegment(EasingType.PolyOut, 0f, -1.0f, 0.25f, 2),
                new CurveSegment(EasingType.PolyIn, 0.25f, -0.75f, 1.75f, 3),
                new CurveSegment(EasingType.PolyOut, 0.85f, 1.0f, 0.1f, 2)
            },
            maxAngle: MathHelper.PiOver2 * 1.9f,
            duration: 32,
            bladeLength: 165f,
            flip: true,
            squish: 0.86f,
            damageMult: 1.1f
        );

        // Phase 2: Stellar Execution — devastating overhead slam with ground shockwave
        private static readonly ComboPhase Phase2_StellarExecution = new ComboPhase(
            curves: new CurveSegment[]
            {
                new CurveSegment(EasingType.SineOut, 0f, -1.1f, 0.15f, 2),
                new CurveSegment(EasingType.PolyIn, 0.18f, -0.95f, 2.15f, 4),
                new CurveSegment(EasingType.PolyOut, 0.82f, 1.2f, 0.05f, 2)
            },
            maxAngle: MathHelper.PiOver2 * 2.3f,
            duration: 40,
            bladeLength: 175f,
            flip: false,
            squish: 0.80f,
            damageMult: 1.5f
        );

        #endregion

        #region Abstract Overrides

        protected override ComboPhase[] GetAllPhases() => new ComboPhase[]
        {
            Phase0_ShadowCleave,
            Phase1_CosmicDivide,
            Phase2_StellarExecution
        };

        protected override Color[] GetPalette() => ExecutionerPalette;

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
            => ModContent.Request<Texture2D>("MagnumOpus/Content/Nachtmusik/Weapons/NocturnalExecutioner/NocturnalExecutioner", AssetRequestMode.ImmediateLoad).Value;

        protected override SoundStyle GetSwingSound()
            => SoundID.Item71 with { Pitch = -0.3f + ComboStep * 0.2f, Volume = 0.9f };

        protected override int GetInitialDustType() => DustID.PurpleTorch;

        protected override int GetSecondaryDustType() => DustID.Enchanted_Gold;

        protected override Vector3 GetLightColor()
        {
            float intensity = 0.6f + ComboStep * 0.15f;
            Color c = Color.Lerp(DeepIndigo, StarlightSilver, Progression);
            return c.ToVector3() * intensity;
        }

        #endregion

        #region Combo Specials — Phase-specific attack patterns

        protected override void HandleComboSpecials()
        {
            if (hasSpawnedSpecial) return;

            // Phase 0: Spawn NocturnalBladeProjectile forward at 55%
            if (ComboStep == 0 && Progression >= 0.55f)
            {
                hasSpawnedSpecial = true;
                if (Main.myPlayer == Projectile.owner)
                {
                    Vector2 tip = GetBladeTipPosition();
                    Vector2 dir = SwordDirection;
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), tip, dir * 14f,
                        ModContent.ProjectileType<NocturnalBladeProjectile>(),
                        (int)(Projectile.damage * 0.35f), 3f, Projectile.owner);
                }

                Vector2 vfxTip = GetBladeTipPosition();
                NocturnalExecutionerVFX.Phase0AccentVFX(vfxTip);
            }

            // Phase 1: Fire 2 blades in V-pattern + cosmic dust cloud at 50%
            if (ComboStep == 1 && Progression >= 0.50f)
            {
                hasSpawnedSpecial = true;
                if (Main.myPlayer == Projectile.owner)
                {
                    Vector2 tip = GetBladeTipPosition();
                    Vector2 dir = SwordDirection;
                    for (int i = -1; i <= 1; i += 2)
                    {
                        Vector2 vel = dir.RotatedBy(MathHelper.ToRadians(i * 15f)) * 14f;
                        Projectile.NewProjectile(Projectile.GetSource_FromThis(), tip, vel,
                            ModContent.ProjectileType<NocturnalBladeProjectile>(),
                            (int)(Projectile.damage * 0.4f), 3f, Projectile.owner);
                    }
                }

                Vector2 vfxTip = GetBladeTipPosition();
                NocturnalExecutionerVFX.Phase1DivideVFX(vfxTip);
            }

            // Phase 2: 3 blades in fan + ground impact shockwave at 65%
            if (ComboStep == 2 && Progression >= 0.65f)
            {
                hasSpawnedSpecial = true;
                if (Main.myPlayer == Projectile.owner)
                {
                    Vector2 tip = GetBladeTipPosition();
                    Vector2 dir = SwordDirection;

                    // 3 blades in fan
                    for (int i = -1; i <= 1; i++)
                    {
                        Vector2 vel = dir.RotatedBy(MathHelper.ToRadians(i * 12f)) * 16f;
                        Projectile.NewProjectile(Projectile.GetSource_FromThis(), tip, vel,
                            ModContent.ProjectileType<NocturnalBladeProjectile>(),
                            (int)(Projectile.damage * 0.5f), 4f, Projectile.owner);
                    }
                }

                Vector2 vfxTip = GetBladeTipPosition();
                NocturnalExecutionerVFX.Phase2StellarExecutionVFX(vfxTip);
                MagnumScreenEffects.AddScreenShake(7f);
            }
        }

        #endregion

        #region On Hit — Celestial Harmony + Execution Charge

        protected override void OnSwingHitNPC(NPC target, NPC.HitInfo hit, int remainingDamageCount)
        {
            // Apply Celestial Harmony debuff
            target.AddBuff(ModContent.BuffType<CelestialHarmony>(), 480);
            if (target.TryGetGlobalNPC(out CelestialHarmonyNPC harmonyNPC))
                harmonyNPC.AddStack(target, 2);

            // Build execution charge on owning item
            if (Main.myPlayer == Projectile.owner)
            {
                Player player = Main.player[Projectile.owner];
                if (player.HeldItem?.ModItem is NocturnalExecutioner exec)
                {
                    exec.ExecutionCharge += hit.Crit ? 15 : 10;
                }
            }

            // Layered cosmic impact VFX — scales with combo phase
            NocturnalExecutionerVFX.SwingImpactVFX(target.Center, ComboStep);

            // Multi-ring cosmic ripple
            for (int ring = 0; ring < 2 + ComboStep; ring++)
            {
                float p = (float)ring / (2 + ComboStep);
                Color ringColor = Color.Lerp(DeepIndigo, StarlightSilver, p);
                CustomParticles.HaloRing(target.Center, ringColor, 0.3f + ring * 0.1f, 12 + ring * 3);
            }

            // Radial cosmic dust burst — full nocturnal palette
            int dustCount = 8 + ComboStep * 4;
            for (int i = 0; i < dustCount; i++)
            {
                float angle = MathHelper.TwoPi * i / dustCount;
                float dp = (float)i / dustCount;
                Color dc = Color.Lerp(DeepIndigo, StarlightSilver, dp);
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 7f);
                Dust d = Dust.NewDustPerfect(target.Center, DustID.PurpleTorch, vel, 0, dc, 1.4f);
                d.noGravity = true;
            }

            // Stellar sparkle accents
            NachtmusikVFXLibrary.SpawnTwinklingStars(target.Center, 2 + ComboStep, 25f);
            NachtmusikVFXLibrary.SpawnMusicNotes(target.Center, 2 + ComboStep, 25f, 0.7f, 0.9f, 25);

            if (hit.Crit)
            {
                NachtmusikVFXLibrary.SpawnStarBurst(target.Center, 3, 1.0f);
                NachtmusikVFXLibrary.SpawnShatteredStarlight(target.Center, 4, 5f, 0.6f, false);
            }

            Lighting.AddLight(target.Center, CosmicBlue.ToVector3() * 0.8f);
        }

        #endregion

        #region Custom VFX — Shader-driven ExecutionDecree void trail + cosmic bloom

        protected override void DrawCustomVFX(SpriteBatch sb)
        {
            if (Progression < 0.08f || Progression > 0.92f) return;

            Vector2 tipPos = GetBladeTipPosition();
            float phaseIntensity = 1f + ComboStep * 0.2f;
            float time = (float)Main.timeForVisualEffects * 0.03f;

            // ═══════════════════════════════════════════════════════════════
            //  SHADER LAYER 1: ExecutionDecree — heavy void-rip trail overlay
            //  Dark vortex energy unique to the Executioner's heavy cosmic identity
            // ═══════════════════════════════════════════════════════════════
            if (NachtmusikShaderManager.HasExecutionDecree || NachtmusikShaderManager.HasStarTrail)
            {
                int trailCount = BuildTrailPositions();
                if (trailCount > 2)
                {
                    var trailPositions = new Vector2[trailCount];
                    Array.Copy(_trailPosBuffer, trailPositions, trailCount);

                    // Heavy body pass — widened, cosmic void energy
                    float bodyWidth = (14f + ComboStep * 5f) * phaseIntensity;
                    CalamityStyleTrailRenderer.DrawDualLayerTrail(
                        trailPositions, null, CalamityStyleTrailRenderer.TrailStyle.Cosmic,
                        bodyWidth, NachtmusikPalette.CosmicVoid * 0.6f, NachtmusikPalette.Violet * 0.5f,
                        phaseIntensity * 0.6f, bodyOverbright: 3.0f, coreOverbright: 5f, coreWidthRatio: 0.35f);
                }
            }

            // ═══════════════════════════════════════════════════════════════
            //  SHADER LAYER 2: Serenade aura at blade center — ominous void pulse
            //  The executioner's dark cosmic presence radiates from the blade
            // ═══════════════════════════════════════════════════════════════
            if (NachtmusikShaderManager.HasSerenade)
            {
                Texture2D auraTex = MagnumTextureRegistry.GetSoftGlow();
                if (auraTex != null)
                {
                    float auraPulse = 0.8f + 0.2f * MathF.Sin(Progression * MathHelper.Pi * 2.5f);
                    float auraScale = (0.25f + ComboStep * 0.06f) * auraPulse * phaseIntensity;
                    float auraOpacity = MathHelper.Clamp((Progression - 0.1f) / 0.1f, 0f, 1f)
                                      * MathHelper.Clamp((0.9f - Progression) / 0.1f, 0f, 1f);

                    // Blade midpoint aura for cosmic presence
                    Vector2 midPos = Vector2.Lerp(Owner.MountedCenter, tipPos, 0.6f) - Main.screenPosition;

                    NachtmusikShaderManager.BeginShaderAdditive(sb);
                    NachtmusikShaderManager.ApplySerenade(time, NachtmusikPalette.CosmicVoid,
                        NachtmusikPalette.Violet, phase: Progression);

                    Color auraColor = NachtmusikPalette.CosmicVoid with { A = 0 } * auraOpacity * 0.5f;
                    sb.Draw(auraTex, midPos, null, auraColor, SwordRotation * 0.5f,
                        auraTex.Size() / 2f, auraScale, SpriteEffects.None, 0f);

                    // Tip aura — brighter, concentrated
                    Vector2 tipScreen = tipPos - Main.screenPosition;
                    Color tipAura = NachtmusikPalette.Violet with { A = 0 } * auraOpacity * 0.7f;
                    sb.Draw(auraTex, tipScreen, null, tipAura, 0f,
                        auraTex.Size() / 2f, auraScale * 0.6f, SpriteEffects.None, 0f);

                    NachtmusikShaderManager.RestoreSpriteBatch(sb);
                }
            }

            // ═══════════════════════════════════════════════════════════════
            //  PARTICLE LAYER: Dense cosmic nebula dust — heavy, weighty
            // ═══════════════════════════════════════════════════════════════

            // Dense cosmic nebula dust trail along blade
            for (int i = 0; i < 2 + ComboStep; i++)
            {
                float dp = Main.rand.NextFloat();
                Color dc = Color.Lerp(DeepIndigo, StarlightSilver, dp);
                Vector2 dustPos = Vector2.Lerp(Owner.MountedCenter, tipPos, Main.rand.NextFloat(0.4f, 1f));
                Dust d = Dust.NewDustPerfect(dustPos, DustID.PurpleTorch,
                    -SwordDirection * Main.rand.NextFloat(1f, 3f) + Main.rand.NextVector2Circular(1f, 1f),
                    0, dc, 1.3f * phaseIntensity);
                d.noGravity = true;
                d.fadeIn = 1.2f;
            }

            // Star sparkle accents at blade tip
            if (Main.rand.NextBool(3))
            {
                Vector2 sparklePos = tipPos + Main.rand.NextVector2Circular(10f, 10f);
                Dust star = Dust.NewDustPerfect(sparklePos, DustID.GoldFlame,
                    Main.rand.NextVector2Circular(2f, 2f), 0, StellarWhite, 0.8f);
                star.noGravity = true;
            }

            // Music notes — deep cosmic serenade
            if (Main.rand.NextBool(5))
            {
                Vector2 noteVel = -SwordDirection * 1.5f + new Vector2(0, -0.5f);
                MagnumParticleHandler.SpawnParticle(new HueShiftingMusicNoteParticle(
                    tipPos, noteVel,
                    hueMin: 0.58f, hueMax: 0.72f,
                    saturation: 0.7f, luminosity: 0.65f,
                    scale: 0.80f, lifetime: 28, hueSpeed: 0.02f));
            }

            // ═══════════════════════════════════════════════════════════════
            //  BLOOM LAYER: Multi-scale void bloom — 5 layers for cosmic weight
            //  Outer void → cosmic purple → indigo → starlight → white-hot core
            // ═══════════════════════════════════════════════════════════════
            {
                float bloomOpacity = MathHelper.Clamp((Progression - 0.08f) / 0.12f, 0f, 1f)
                                   * MathHelper.Clamp((0.92f - Progression) / 0.12f, 0f, 1f);
                float bloomScale = 0.45f + ComboStep * 0.1f;
                float pulse = 0.85f + 0.15f * MathF.Sin(time * 4f);

                Texture2D bloomTex = MagnumTextureRegistry.GetSoftGlow();
                if (bloomTex != null && bloomOpacity > 0.01f)
                {
                    Vector2 tipScreen = tipPos - Main.screenPosition;
                    Vector2 bloomOrigin = bloomTex.Size() / 2f;

                    SwingShaderSystem.BeginAdditive(sb);

                    // Layer 1: Wide void atmospheric halo
                    sb.Draw(bloomTex, tipScreen, null, NachtmusikPalette.CosmicVoid with { A = 0 } * bloomOpacity * 0.2f,
                        0f, bloomOrigin, bloomScale * 2.8f * pulse, SpriteEffects.None, 0f);

                    // Layer 2: Cosmic purple mid glow
                    sb.Draw(bloomTex, tipScreen, null, NachtmusikPalette.CosmicPurple with { A = 0 } * bloomOpacity * 0.3f,
                        SwordRotation * 0.4f, bloomOrigin, bloomScale * 1.8f * pulse, SpriteEffects.None, 0f);

                    // Layer 3: Deep indigo inner glow
                    sb.Draw(bloomTex, tipScreen, null, DeepIndigo with { A = 0 } * bloomOpacity * 0.45f,
                        0f, bloomOrigin, bloomScale * 1.0f, SpriteEffects.None, 0f);

                    // Layer 4: Starlight silver accent
                    sb.Draw(bloomTex, tipScreen, null, StarlightSilver with { A = 0 } * bloomOpacity * 0.5f,
                        0f, bloomOrigin, bloomScale * 0.5f, SpriteEffects.None, 0f);

                    // Layer 5: Stellar white core
                    sb.Draw(bloomTex, tipScreen, null, StellarWhite with { A = 0 } * bloomOpacity * 0.25f,
                        0f, bloomOrigin, bloomScale * 0.2f, SpriteEffects.None, 0f);

                    // Star flare at tip — cosmic executioner's mark
                    Texture2D flareTex = MagnumTextureRegistry.GetRadialBloom();
                    if (flareTex != null)
                    {
                        Vector2 flareOrigin = flareTex.Size() / 2f;
                        float flareRot = time * 0.3f;
                        sb.Draw(flareTex, tipScreen, null, NachtmusikPalette.Violet with { A = 0 } * bloomOpacity * 0.35f,
                            flareRot, flareOrigin, bloomScale * 0.6f * pulse, SpriteEffects.None, 0f);
                        sb.Draw(flareTex, tipScreen, null, StellarWhite with { A = 0 } * bloomOpacity * 0.15f,
                            flareRot + MathHelper.PiOver4, flareOrigin, bloomScale * 0.35f, SpriteEffects.None, 0f);
                    }

                    SwingShaderSystem.RestoreSpriteBatch(sb);
                }
            }
        }

        #endregion
    }
}
