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
using WrathsCleaverItem = MagnumOpus.Content.DiesIrae.Weapons.WrathsCleaver.WrathsCleaver;
using MagnumOpus.Content.DiesIrae.Weapons.WrathsCleaver.Utilities;
using MagnumOpus.Content.DiesIrae.Weapons.WrathsCleaver.Buffs;

namespace MagnumOpus.Content.DiesIrae.Weapons.WrathsCleaver.Projectiles
{
    /// <summary>
    /// Wrath's Cleaver swing projectile (MeleeSwingBase).
    /// 4-Phase Wrath Combo:
    ///   0 - Accusation:  150 deg horizontal cleave
    ///   1 - Conviction:  170 deg overhead slam
    ///   2 - Execution:   270 deg spinning cleave
    ///   3 - Damnation:   360 deg infernal eruption
    ///
    /// DrawCustomVFX: 5-layer composited VFX pipeline
    ///   L1: Shader-driven trail (DiesIraeShaderManager)
    ///   L2: WrathSlash shader aura at blade midpoint + tip
    ///   L3: Dense hellfire dust + gold star accents + music notes
    ///   L4: 5-sublayer infernal bloom (Blood-Infernal-Ember-Gold-White)
    ///   L5: Judgment impact ring on finisher phases
    /// </summary>
    public sealed class WrathsCleaverSwing : MeleeSwingBase
    {
        private static readonly ComboPhase[] _phases = new ComboPhase[]
        {
            // Phase 0: ACCUSATION - Wide horizontal cleave
            new ComboPhase(
                curves: new CurveSegment[]
                {
                    new CurveSegment(EasingType.SineIn, 0.0f, -0.15f, 0.15f),
                    new CurveSegment(EasingType.PolyOut, 0.15f, 0.0f, 1.0f, 3),
                    new CurveSegment(EasingType.SineOut, 0.75f, 1.0f, -0.08f),
                },
                maxAngle: MathHelper.ToRadians(150),
                duration: 22,
                bladeLength: 140f,
                flip: false,
                squish: 0.85f,
                damageMult: 1.0f
            ),

            // Phase 1: CONVICTION - Overhead slam with heavy contact
            new ComboPhase(
                curves: new CurveSegment[]
                {
                    new CurveSegment(EasingType.SineIn, 0.0f, -0.20f, 0.20f),
                    new CurveSegment(EasingType.PolyOut, 0.25f, 0.0f, 1.0f, 4),
                    new CurveSegment(EasingType.SineOut, 0.70f, 1.0f, -0.05f),
                },
                maxAngle: MathHelper.ToRadians(170),
                duration: 28,
                bladeLength: 155f,
                flip: true,
                squish: 0.80f,
                damageMult: 1.3f
            ),

            // Phase 2: EXECUTION - Spinning 270 deg cleave
            new ComboPhase(
                curves: new CurveSegment[]
                {
                    new CurveSegment(EasingType.SineIn, 0.0f, -0.10f, 0.10f),
                    new CurveSegment(EasingType.PolyOut, 0.12f, 0.0f, 1.0f, 2),
                    new CurveSegment(EasingType.SineOut, 0.80f, 1.0f, -0.03f),
                },
                maxAngle: MathHelper.ToRadians(270),
                duration: 26,
                bladeLength: 160f,
                flip: false,
                squish: 0.90f,
                damageMult: 1.5f
            ),

            // Phase 3: DAMNATION - 360 deg infernal eruption
            new ComboPhase(
                curves: new CurveSegment[]
                {
                    new CurveSegment(EasingType.SineOut, 0.0f, -0.12f, 0.12f),
                    new CurveSegment(EasingType.PolyIn, 0.10f, 0.0f, 1.0f, 5),
                    new CurveSegment(EasingType.SineOut, 0.85f, 1.0f, -0.02f),
                },
                maxAngle: MathHelper.TwoPi,
                duration: 34,
                bladeLength: 170f,
                flip: false,
                squish: 0.75f,
                damageMult: 2.0f
            ),
        };

        // === Abstract overrides ===

        protected override ComboPhase[] GetAllPhases() => _phases;

        protected override Color[] GetPalette() => WrathsCleaverUtils.SwingPalette;

        protected override CalamityStyleTrailRenderer.TrailStyle GetTrailStyle()
            => CalamityStyleTrailRenderer.TrailStyle.Flame;

        protected override string GetSmearTexturePath(int comboStep)
        {
            return comboStep switch
            {
                0 => "MagnumOpus/Assets/VFX Asset Library/SlashArcs/FlamingSwordArcSmear",
                1 => "MagnumOpus/Assets/VFX Asset Library/SlashArcs/FlamingSwordArcSmear2",
                2 => "MagnumOpus/Assets/VFX Asset Library/SlashArcs/FullCircleSwordArcSlash",
                3 => "MagnumOpus/Assets/VFX Asset Library/SlashArcs/FullCircleSwordArcSlash",
                _ => "MagnumOpus/Assets/VFX Asset Library/SlashArcs/FlamingSwordArcSmear",
            };
        }

        protected override string GetSmearGradientPath()
            => "MagnumOpus/Assets/VFX Asset Library/ColorGradients/DiesIraeGradientLUTandRAMP";

        // === Virtual overrides ===

        protected override int GetInitialDustType() => DustID.Torch;
        protected override int GetSecondaryDustType() => DustID.Enchanted_Gold;

        protected override SoundStyle GetSwingSound()
        {
            return ComboStep switch
            {
                0 => SoundID.Item71 with { Pitch = -0.3f, Volume = 0.8f },
                1 => SoundID.Item71 with { Pitch = -0.5f, Volume = 0.9f },
                2 => SoundID.Item71 with { Pitch = -0.2f, Volume = 1.0f },
                3 => SoundID.Item45 with { Pitch = -0.6f, Volume = 1.0f },
                _ => SoundID.Item71,
            };
        }

        protected override Texture2D GetBladeTexture()
        {
            return ModContent.Request<Texture2D>(
                "MagnumOpus/Content/DiesIrae/Weapons/WrathsCleaver/WrathsCleaver").Value;
        }

        protected override Vector3 GetLightColor()
        {
            float intensity = 0.4f + ComboStep * 0.2f;
            return new Vector3(intensity, intensity * 0.3f, intensity * 0.05f);
        }

        // === Combo specials ===

        protected override void HandleComboSpecials()
        {
            float prog = Progression;

            // Phase 1 (Conviction) @ 65%: Smoke eruption at slam impact
            if (ComboStep == 1 && !hasSpawnedSpecial && prog > 0.65f)
            {
                hasSpawnedSpecial = true;
                Vector2 slamPos = GetBladeTipPosition();

                DiesIraeVFXLibrary.SpawnHeavySmoke(slamPos, 6, 1.2f, 3f, 50);
                DiesIraeVFXLibrary.SpawnEmberScatter(slamPos, 10, 4f);
                DiesIraeVFXLibrary.SpawnBoneAshScatter(slamPos, 4, 2f);

                SoundEngine.PlaySound(SoundID.Item74 with { Pitch = -0.4f, Volume = 0.7f }, slamPos);
            }

            // Phase 2 (Execution) @ 50%: Shockwave ring
            if (ComboStep == 2 && !hasSpawnedSpecial && prog > 0.50f)
            {
                hasSpawnedSpecial = true;
                Vector2 center = Owner.MountedCenter;

                DiesIraeVFXLibrary.SpawnWrathPulseRings(center, 3, 0.4f);
                DiesIraeVFXLibrary.SpawnRadialDustBurst(center, 20, 7f, DustID.Torch);
                DiesIraeVFXLibrary.SpawnColorRampedSparkleExplosion(center, 12, 6f, 0.35f);

                SoundEngine.PlaySound(SoundID.Item45 with { Pitch = -0.5f, Volume = 0.8f }, center);
            }

            // Phase 3 (Damnation) @ 45%: Infernal eruption
            if (ComboStep == 3 && !hasSpawnedSpecial && prog > 0.45f)
            {
                hasSpawnedSpecial = true;
                Vector2 center = Owner.MountedCenter;

                DiesIraeVFXLibrary.WrathShockwaveImpact(center, 1.5f);
                DiesIraeVFXLibrary.SpawnHeavySmoke(center, 10, 1.5f, 4f, 70);
                DiesIraeVFXLibrary.SpawnHellfireStarburst(center, 1.8f);

                MagnumScreenEffects.AddScreenShake(10f);

                SoundEngine.PlaySound(SoundID.Item45 with { Pitch = -0.7f, Volume = 1.0f }, center);
                SoundEngine.PlaySound(SoundID.Item74 with { Pitch = -0.2f, Volume = 0.9f }, center);
            }

            // Continuous ember VFX during active swing
            if (prog > 0.08f && prog < 0.92f)
            {
                WrathsCleaverUtils.SpawnSwingDust(Owner.MountedCenter, SwordDirection,
                    CurrentPhase.BladeLength, ComboStep, prog, Direction);

                // Gold star accents every 5 frames
                if (Main.GameUpdateCount % 5 == 0)
                {
                    Vector2 tipPos = GetBladeTipPosition();
                    DiesIraeVFXLibrary.SpawnContrastSparkle(tipPos, SwordDirection);
                }

                // Music notes every 8 frames on Phase 2+
                if (ComboStep >= 2 && Main.GameUpdateCount % 8 == 0)
                {
                    Vector2 tipPos = GetBladeTipPosition();
                    DiesIraeVFXLibrary.SpawnMusicNotes(tipPos, 1, 15f, 0.6f, 0.9f, 25);
                }
            }
        }

        // === Hit effects ===

        protected override void OnSwingHitNPC(NPC target, NPC.HitInfo hit, int remainingDamageCount)
        {
            target.AddBuff(ModContent.BuffType<HellfireImmolation>(), 180 + ComboStep * 60);
            target.AddBuff(BuffID.OnFire3, 240);

            if (ComboStep >= 2)
                target.AddBuff(ModContent.BuffType<WrathMark>(), 300);

            // Build Wrath Meter on the parent item
            if (Owner.HeldItem?.ModItem is WrathsCleaverItem cleaver)
            {
                float wrathGain = hit.Crit ? 12f : 8f;
                cleaver.AddWrath(wrathGain);
            }

            // Multi-layered wrath impact VFX
            DiesIraeVFXLibrary.MeleeImpact(target.Center, ComboStep);

            if (ComboStep >= 1)
            {
                Vector2 hitDir = (target.Center - Owner.MountedCenter).SafeNormalize(Vector2.UnitX);
                DiesIraeVFXLibrary.SpawnDirectionalSparkleExplosion(
                    target.Center, hitDir, 6 + ComboStep * 3, 5f + ComboStep, 0.3f, 0.6f);
            }

            // Damnation phase: extra eruption burst
            if (ComboStep == 3)
            {
                DiesIraeVFXLibrary.SpawnHellfireStarburst(target.Center, 1.2f);
                MagnumScreenEffects.AddScreenShake(6f);
            }
        }

        // === DrawCustomVFX: 5-layer composited VFX ===

        protected override void DrawCustomVFX(SpriteBatch sb)
        {
            if (Progression < 0.05f || Progression > 0.95f) return;

            try
            {
            Vector2 tipWorld = GetBladeTipPosition();
            Vector2 tipScreen = tipWorld - Main.screenPosition;
            Vector2 midWorld = Owner.MountedCenter + SwordDirection * CurrentPhase.BladeLength * 0.5f;
            Vector2 midScreen = midWorld - Main.screenPosition;
            Vector2 rootScreen = Owner.MountedCenter - Main.screenPosition;

            float time = (float)Main.GameUpdateCount * 0.03f;
            float stepIntensity = 0.7f + ComboStep * 0.15f;
            float swingIntensity = MathF.Sin(Progression * MathF.PI) * stepIntensity;

            // -- LAYER 1: Shader-driven inferno trail via DiesIraeShaderManager --
            if (ComboStep >= 1 && DiesIraeShaderManager.HasFlameTrail)
            {
                try
                {
                    DiesIraeShaderManager.BeginShaderAdditive(sb);
                    DiesIraeShaderManager.BindSmokeNoiseTexture(Main.graphics.GraphicsDevice);
                    DiesIraeShaderManager.ApplyWrathsCleaverTrail(time);

                    Texture2D softGlow = MagnumTextureRegistry.GetSoftGlow();
                    if (softGlow != null)
                    {
                        Vector2 glowOrigin = softGlow.Size() * 0.5f;
                        Color bodyColor = DiesIraePalette.Additive(DiesIraePalette.InfernalRed, 0.4f * swingIntensity);
                        float bodyScale = (16f + ComboStep * 5f) * 0.005f;
                        sb.Draw(softGlow, midScreen, null, bodyColor, 0f, glowOrigin, bodyScale, SpriteEffects.None, 0f);
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

            // -- LAYER 2: WrathSlash shader aura (per-weapon shader effect) --
            bool hasWrathShader = false;
            try { hasWrathShader = WrathsCleaverShaderLoader.WrathSlashShader?.Value != null; }
            catch { }

            if (hasWrathShader)
            {
                try
                {
                    DiesIraeShaderManager.BeginShaderAdditive(sb);

                    var shader = WrathsCleaverShaderLoader.WrathSlashShader.Value;
                    shader.Parameters["uColor"]?.SetValue(DiesIraePalette.InfernalRed.ToVector3());
                    shader.Parameters["uSecondaryColor"]?.SetValue(DiesIraePalette.JudgmentGold.ToVector3());
                    shader.Parameters["uTime"]?.SetValue(time);
                    shader.Parameters["uOpacity"]?.SetValue(swingIntensity);
                    shader.Parameters["uIntensity"]?.SetValue(1.8f + ComboStep * 0.3f);
                    shader.CurrentTechnique = shader.Techniques["WrathSlashMain"];
                    shader.CurrentTechnique.Passes[0].Apply();

                    Texture2D softGlow = MagnumTextureRegistry.GetSoftGlow();
                    if (softGlow != null)
                    {
                        Vector2 glowOrigin = softGlow.Size() * 0.5f;

                        // Blade midpoint: wide hellfire presence
                        Color midColor = DiesIraePalette.Additive(DiesIraePalette.BloodRed, 0.35f * swingIntensity);
                        sb.Draw(softGlow, midScreen, null, midColor, 0f, glowOrigin,
                            0.06f + ComboStep * 0.01f, SpriteEffects.None, 0f);

                        // Blade tip: concentrated wrath authority
                        Color tipColor = DiesIraePalette.Additive(DiesIraePalette.EmberOrange, 0.5f * swingIntensity);
                        sb.Draw(softGlow, tipScreen, null, tipColor, 0f, glowOrigin,
                            0.04f + ComboStep * 0.008f, SpriteEffects.None, 0f);
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

            // -- LAYER 3: Dense hellfire dust with gold star accents --
            int dustCount = 2 + ComboStep;
            for (int i = 0; i < dustCount; i++)
            {
                float along = Main.rand.NextFloat(0.3f, 1f);
                Vector2 dustPos = Owner.MountedCenter + SwordDirection * CurrentPhase.BladeLength * along;
                Vector2 perp = SwordDirection.RotatedBy(MathHelper.PiOver2 * Direction);
                Vector2 vel = perp * Main.rand.NextFloat(0.5f, 1.5f);

                Color col = WrathsCleaverUtils.GetPaletteColor(along);
                Dust d = Dust.NewDustPerfect(dustPos, DustID.Torch, vel, 0, col, 1.2f + ComboStep * 0.2f);
                d.noGravity = true;
                d.fadeIn = 1.0f;
            }

            if (Main.rand.NextBool(3))
            {
                Vector2 sparkPos = Owner.MountedCenter + SwordDirection * CurrentPhase.BladeLength * Main.rand.NextFloat(0.6f, 1f);
                Dust s = Dust.NewDustPerfect(sparkPos, DustID.Enchanted_Gold,
                    Main.rand.NextVector2Circular(1.5f, 1.5f), 0, DiesIraePalette.JudgmentGold, 0.8f);
                s.noGravity = true;
            }

            // -- LAYER 4: 5-sublayer infernal bloom at blade tip --
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone,
                null, Main.GameViewMatrix.TransformationMatrix);

            WrathsCleaverUtils.DrawTipBloom(sb, tipScreen, swingIntensity, ComboStep);
            WrathsCleaverUtils.DrawHellfireStarFlare(sb, tipScreen, 1f + ComboStep * 0.3f, swingIntensity * 0.8f);

            // Root glow: ember aura at player center
            Texture2D rootGlow = MagnumTextureRegistry.GetSoftGlow();
            if (rootGlow != null)
            {
                Vector2 rootOrigin = rootGlow.Size() * 0.5f;
                float rootIntensity = 0.3f + ComboStep * 0.1f;
                Color rootColor = DiesIraePalette.Additive(DiesIraePalette.BloodRed, rootIntensity * 0.4f * swingIntensity);
                sb.Draw(rootGlow, rootScreen, null, rootColor, 0f, rootOrigin, 0.12f, SpriteEffects.None, 0f);
            }

            // -- LAYER 5: Judgment ring on Phase 2+ and radial slash on Phase 3 --
            if (ComboStep >= 2)
            {
                float ringPhase = Progression * 2f;
                WrathsCleaverUtils.DrawJudgmentImpactRing(sb, tipScreen,
                    1f + ComboStep * 0.3f, swingIntensity * 0.7f, ringPhase);
            }

            if (ComboStep == 3 && Progression > 0.3f)
            {
                float slashRot = SwordDirection.ToRotation();
                WrathsCleaverUtils.DrawRadialSlashBurst(sb, midScreen, 1.5f, swingIntensity, slashRot);
            }

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
