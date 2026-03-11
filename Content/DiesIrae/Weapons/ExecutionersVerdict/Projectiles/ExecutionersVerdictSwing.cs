using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;
using MagnumOpus.Common.BaseClasses;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.VFX.Trails;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using static MagnumOpus.Common.Systems.Particles.Particle;
using MagnumOpus.Content.DiesIrae;
using MagnumOpus.Content.DiesIrae.Weapons.ExecutionersVerdict.Utilities;
using MagnumOpus.Content.DiesIrae.Weapons.ExecutionersVerdict.Buffs;

namespace MagnumOpus.Content.DiesIrae.Weapons.ExecutionersVerdict.Projectiles
{
    /// <summary>
    /// Executioner's Verdict swing projectile (MeleeSwingBase).
    /// 3-Phase Judicial Combo:
    ///   0 - Arraignment:       160 deg overhead strike, deliberate + heavy
    ///   1 - Cross-Examination: 140 deg reversed cross slash, faster
    ///   2 - The Verdict:       200 deg execution slash, devastating
    ///
    /// DrawCustomVFX: 5-layer judicial precision VFX
    ///   L1: Shader-driven guillotine blade aura
    ///   L2: Sharp crimson-gold bloom at blade tip
    ///   L3: Controlled fire dust along blade (not chaotic — precise)
    ///   L4: Judgment impact ring + star flare accents
    ///   L5: Root glow + execution bloom on Phase 2
    /// </summary>
    public sealed class ExecutionersVerdictSwing : MeleeSwingBase
    {
        private static readonly ComboPhase[] _phases = new ComboPhase[]
        {
            // Phase 0: ARRAIGNMENT - Overhead strike, deliberate and heavy
            new ComboPhase(
                curves: new CurveSegment[]
                {
                    new CurveSegment(EasingType.SineIn, 0.0f, -0.18f, 0.18f),
                    new CurveSegment(EasingType.PolyOut, 0.20f, 0.0f, 1.0f, 3),
                    new CurveSegment(EasingType.SineOut, 0.72f, 1.0f, -0.06f),
                },
                maxAngle: MathHelper.ToRadians(160),
                duration: 26,
                bladeLength: 170f,
                flip: false,
                squish: 0.82f,
                damageMult: 1.0f
            ),

            // Phase 1: CROSS-EXAMINATION - Reversed cross slash
            new ComboPhase(
                curves: new CurveSegment[]
                {
                    new CurveSegment(EasingType.SineIn, 0.0f, -0.15f, 0.15f),
                    new CurveSegment(EasingType.PolyOut, 0.15f, 0.0f, 1.0f, 3),
                    new CurveSegment(EasingType.SineOut, 0.68f, 1.0f, -0.07f),
                },
                maxAngle: MathHelper.ToRadians(140),
                duration: 24,
                bladeLength: 165f,
                flip: true,
                squish: 0.84f,
                damageMult: 1.25f
            ),

            // Phase 2: THE VERDICT - Massive horizontal execution slash
            new ComboPhase(
                curves: new CurveSegment[]
                {
                    new CurveSegment(EasingType.SineIn, 0.0f, -0.22f, 0.22f),
                    new CurveSegment(EasingType.PolyOut, 0.25f, 0.0f, 1.0f, 4),
                    new CurveSegment(EasingType.SineOut, 0.75f, 1.0f, -0.04f),
                },
                maxAngle: MathHelper.ToRadians(200),
                duration: 30,
                bladeLength: 180f,
                flip: false,
                squish: 0.78f,
                damageMult: 1.6f
            ),
        };

        // === Abstract overrides ===

        protected override ComboPhase[] GetAllPhases() => _phases;

        protected override Color[] GetPalette() => ExecutionersVerdictUtils.SwingPalette;

        protected override CalamityStyleTrailRenderer.TrailStyle GetTrailStyle()
            => CalamityStyleTrailRenderer.TrailStyle.Flame;

        protected override string GetSmearTexturePath(int comboStep)
        {
            return comboStep switch
            {
                0 => "MagnumOpus/Assets/VFX Asset Library/SlashArcs/SwordArcSmear",
                1 => "MagnumOpus/Assets/VFX Asset Library/SlashArcs/FlamingSwordArcSmear",
                2 => "MagnumOpus/Assets/VFX Asset Library/SlashArcs/FullCircleSwordArcSlash",
                _ => "MagnumOpus/Assets/VFX Asset Library/SlashArcs/SwordArcSmear",
            };
        }

        protected override string GetSmearGradientPath()
            => "MagnumOpus/Assets/VFX Asset Library/ColorGradients/DiesIraeGradientLUTandRAMP";

        // === Virtual overrides ===

        protected override int GetInitialDustType() => DustID.Torch;
        protected override int GetSecondaryDustType() => DustID.GoldFlame;

        protected override SoundStyle GetSwingSound()
        {
            return ComboStep switch
            {
                0 => SoundID.Item71 with { Pitch = -0.4f, Volume = 0.85f },
                1 => SoundID.Item71 with { Pitch = -0.2f, Volume = 0.9f },
                2 => SoundID.Item71 with { Pitch = -0.6f, Volume = 1.0f },
                _ => SoundID.Item71 with { Pitch = -0.4f },
            };
        }

        protected override Texture2D GetBladeTexture()
        {
            return ModContent.Request<Texture2D>(
                "MagnumOpus/Content/DiesIrae/Weapons/ExecutionersVerdict/ExecutionersVerdict").Value;
        }

        protected override Vector3 GetLightColor()
        {
            float intensity = 0.45f + ComboStep * 0.2f;
            return new Vector3(intensity * 0.9f, intensity * 0.15f, intensity * 0.05f);
        }

        // === Combo specials ===

        protected override void HandleComboSpecials()
        {
            float prog = Progression;

            // Phase 1 (Cross-Examination) @ 60%: Judgment spark flash
            if (ComboStep == 1 && !hasSpawnedSpecial && prog > 0.60f)
            {
                hasSpawnedSpecial = true;
                Vector2 tipPos = GetBladeTipPosition();

                DiesIraeVFXLibrary.SpawnColorRampedSparkleExplosion(tipPos, 8, 5f, 0.3f);
                DiesIraeVFXLibrary.SpawnEmberScatter(tipPos, 6, 3f);

                SoundEngine.PlaySound(SoundID.Item73 with { Pitch = 0.1f, Volume = 0.6f }, tipPos);
            }

            // Phase 2 (The Verdict) @ 55%: Execution flash + judgment rings
            if (ComboStep == 2 && !hasSpawnedSpecial && prog > 0.55f)
            {
                hasSpawnedSpecial = true;
                Vector2 tipPos = GetBladeTipPosition();
                Vector2 center = Owner.MountedCenter;

                // Judgment flash at blade tip
                DiesIraeVFXLibrary.SpawnJudgmentRings(tipPos, 3, 0.3f);
                DiesIraeVFXLibrary.SpawnRadialDustBurst(tipPos, 12, 6f, DustID.GoldFlame);
                DiesIraeVFXLibrary.SpawnHellfireStarburst(tipPos, 1.2f);

                MagnumScreenEffects.AddScreenShake(5f);

                SoundEngine.PlaySound(SoundID.Item73 with { Pitch = 0.1f, Volume = 0.7f }, tipPos);
                SoundEngine.PlaySound(SoundID.Item45 with { Pitch = -0.3f, Volume = 0.6f }, center);
            }

            // Continuous controlled dust during swing (more precise than WrathsCleaver)
            if (prog > 0.12f && prog < 0.88f)
            {
                ExecutionersVerdictUtils.SpawnSwingDust(Owner.MountedCenter, SwordDirection,
                    CurrentPhase.BladeLength, ComboStep, prog, Direction);

                // Gold judgment sparkles every 6 frames
                if (Main.GameUpdateCount % 6 == 0)
                {
                    Vector2 tipPos = GetBladeTipPosition();
                    DiesIraeVFXLibrary.SpawnContrastSparkle(tipPos, SwordDirection);
                }

                // Music notes on Phase 2 only (judgment proclamation)
                if (ComboStep == 2 && Main.GameUpdateCount % 10 == 0)
                {
                    Vector2 tipPos = GetBladeTipPosition();
                    DiesIraeVFXLibrary.SpawnMusicNotes(tipPos, 1, 12f, 0.5f, 0.8f, 20);
                }
            }
        }

        // === Hit effects ===

        protected override void OnSwingHitNPC(NPC target, NPC.HitInfo hit, int remainingDamageCount)
        {
            target.AddBuff(ModContent.BuffType<ExecutionBrand>(), 360);
            target.AddBuff(ModContent.BuffType<PyreImmolation>(), 180);

            // Execution threshold: non-boss enemies below 15% HP die instantly
            if (!target.boss && target.life < target.lifeMax * 0.15f)
            {
                target.life = 0;
                target.checkDead();

                // Gold judgment flash on execution kill
                DiesIraeVFXLibrary.SpawnHellfireStarburst(target.Center, 1.5f);
                DiesIraeVFXLibrary.SpawnJudgmentRings(target.Center, 2, 0.4f);
                return;
            }

            // Low HP indicator: extra crimson sparks below 30%
            if (target.life < target.lifeMax * 0.30f)
            {
                DiesIraeVFXLibrary.SpawnDirectionalSparkleExplosion(
                    target.Center,
                    (target.Center - Owner.MountedCenter).SafeNormalize(Vector2.UnitX),
                    8, 6f, 0.35f, 0.7f);
            }

            // Standard multi-layered judgment impact
            DiesIraeVFXLibrary.MeleeImpact(target.Center, ComboStep);

            // Extra gold sparks on heavier phases
            if (ComboStep >= 1)
            {
                Vector2 hitDir = (target.Center - Owner.MountedCenter).SafeNormalize(Vector2.UnitX);
                DiesIraeVFXLibrary.SpawnDirectionalSparkleExplosion(
                    target.Center, hitDir, 4 + ComboStep * 2, 5f, 0.3f, 0.6f);
            }

            // The Verdict: screen shake on execution slash
            if (ComboStep == 2)
                MagnumScreenEffects.AddScreenShake(4f);
        }

        // === DrawCustomVFX: 5-layer composited judicial VFX ===

        protected override void DrawCustomVFX(SpriteBatch sb)
        {
            if (Progression < 0.05f || Progression > 0.95f) return;

            try
            {
            Vector2 tipWorld = GetBladeTipPosition();
            Vector2 tipScreen = tipWorld - Main.screenPosition;
            Vector2 rootScreen = Owner.MountedCenter - Main.screenPosition;

            float time = (float)Main.GameUpdateCount * 0.03f;
            float stepIntensity = 0.65f + ComboStep * 0.18f;
            float swingIntensity = MathF.Sin(Progression * MathF.PI) * stepIntensity;

            // -- LAYER 1: Shader-driven guillotine blade aura --
            bool hasGuillotineShader = false;
            try { hasGuillotineShader = ExecutionersVerdictShaderLoader.GuillotineBladeShader?.Value != null; }
            catch { }

            if (hasGuillotineShader)
            {
                try
                {
                    DiesIraeShaderManager.BeginShaderAdditive(sb);

                    var shader = ExecutionersVerdictShaderLoader.GuillotineBladeShader.Value;
                    shader.Parameters["uColor"]?.SetValue(DiesIraePalette.BloodRed.ToVector3());
                    shader.Parameters["uSecondaryColor"]?.SetValue(DiesIraePalette.JudgmentGold.ToVector3());
                    shader.Parameters["uTime"]?.SetValue(time);
                    shader.Parameters["uOpacity"]?.SetValue(swingIntensity);
                    shader.Parameters["uIntensity"]?.SetValue(1.5f + ComboStep * 0.25f);
                    shader.CurrentTechnique.Passes[0].Apply();

                    Texture2D softGlow = MagnumTextureRegistry.GetSoftGlow();
                    if (softGlow != null)
                    {
                        Vector2 glowOrigin = softGlow.Size() * 0.5f;

                        // Blade midpoint: controlled crimson presence
                        Color midColor = DiesIraePalette.Additive(DiesIraePalette.BloodRed, 0.3f * swingIntensity);
                        Vector2 midScreen = Owner.MountedCenter + SwordDirection * CurrentPhase.BladeLength * 0.5f - Main.screenPosition;
                        sb.Draw(softGlow, midScreen, null, midColor, 0f, glowOrigin,
                            0.05f + ComboStep * 0.008f, SpriteEffects.None, 0f);

                        // Blade tip: focused judgment point
                        Color tipColor = DiesIraePalette.Additive(DiesIraePalette.JudgmentGold, 0.45f * swingIntensity);
                        sb.Draw(softGlow, tipScreen, null, tipColor, 0f, glowOrigin,
                            0.035f + ComboStep * 0.006f, SpriteEffects.None, 0f);
                    }

                    DiesIraeShaderManager.RestoreSpriteBatch(sb);
                }
                catch
                {
                    try { sb.End(); } catch { }
                    sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                        DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
                }
            }

            // -- LAYER 2: Controlled fire dust along blade --
            int dustCount = 1 + ComboStep;
            for (int i = 0; i < dustCount; i++)
            {
                float along = Main.rand.NextFloat(0.4f, 1f);
                Vector2 dustPos = Owner.MountedCenter + SwordDirection * CurrentPhase.BladeLength * along;
                Vector2 perp = SwordDirection.RotatedBy(MathHelper.PiOver2 * Direction);
                Vector2 vel = perp * Main.rand.NextFloat(0.3f, 1f);

                Color col = ExecutionersVerdictUtils.GetPaletteColor(along);
                Dust d = Dust.NewDustPerfect(dustPos, DustID.Torch, vel, 0, col, 1f + ComboStep * 0.15f);
                d.noGravity = true;
                d.fadeIn = 0.8f;
            }

            // -- LAYER 3-5: Additive bloom, judgment rings, root glow --
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone,
                null, Main.GameViewMatrix.TransformationMatrix);

            // Sharp crimson-gold bloom at blade tip
            ExecutionersVerdictUtils.DrawTipBloom(sb, tipScreen, swingIntensity, ComboStep);

            // Root glow at player center
            Texture2D rootGlow = MagnumTextureRegistry.GetSoftGlow();
            if (rootGlow != null)
            {
                Vector2 rootOrigin = rootGlow.Size() * 0.5f;
                float rootAlpha = 0.25f + ComboStep * 0.08f;
                Color rootColor = DiesIraePalette.Additive(DiesIraePalette.BloodRed, rootAlpha * 0.35f * swingIntensity);
                sb.Draw(rootGlow, rootScreen, null, rootColor, 0f, rootOrigin, 0.10f, SpriteEffects.None, 0f);
            }

            // Phase 2 (The Verdict): execution bloom + impact ring
            if (ComboStep == 2 && Progression > 0.20f && Progression < 0.75f)
            {
                ExecutionersVerdictUtils.DrawBloomStack(sb, rootScreen, 0.025f, swingIntensity * 0.5f, ComboStep);

                float ringPhase = Progression * 2f;
                DiesIraeVFXLibrary.DrawThemeImpactRing(sb, tipWorld, 1f + ComboStep * 0.2f, swingIntensity * 0.5f, ringPhase);
            }

            // Theme star flare accents at blade tip
            DiesIraeVFXLibrary.DrawThemeStarFlare(sb, tipWorld, 0.8f + ComboStep * 0.15f, swingIntensity * 0.4f);

            }
            catch { }
            finally
            {
                try { sb.End(); } catch { }
                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                    DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            }
        }
    }
}
