using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.BaseClasses;
using MagnumOpus.Common.Systems.VFX.Trails;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.Particles;
using static MagnumOpus.Common.Systems.Particles.Particle;
using MagnumOpus.Content.DiesIrae.Weapons.WrathsCleaver.Utilities;
using MagnumOpus.Content.DiesIrae.Weapons.WrathsCleaver.Buffs;

namespace MagnumOpus.Content.DiesIrae.Weapons.WrathsCleaver.Projectiles
{
    /// <summary>
    /// WRATH'S CLEAVER ? Melee Swing Projectile.
    /// Extends MeleeSwingBase for full 6-layer rendering pipeline:
    ///   Trail �� Smear overlay �� Blade sprite �� Glow + Lens flare �� Motion blur �� Custom VFX
    ///
    /// 3-Phase Wrath Combo:
    ///   Phase 0 (Accusation) ? Wide horizontal cleave, moderate speed
    ///   Phase 1 (Conviction) ? Overhead slam, slow windup + heavy contact
    ///   Phase 2 (Execution) ? Spinning 270�� cleave, devastating whirlwind
    /// </summary>
    public class WrathsCleaverSwing : MeleeSwingBase
    {
        // ???????????????????????????????????????????????????????????
        //  COMBO PHASES ? Three movements of wrath
        // ???????????????????????????????????????????????????????????

        private static readonly ComboPhase[] _phases = new ComboPhase[]
        {
            // Phase 0: ACCUSATION ? Heavy horizontal cleave
            new ComboPhase(
                curves: new CurveSegment[]
                {
                    new CurveSegment(EasingType.SineIn, 0.0f, -0.15f, 0.15f),   // Windup pull-back
                    new CurveSegment(EasingType.PolyOut, 0.15f, 0.0f, 1.0f, 3), // Main sweep
                    new CurveSegment(EasingType.SineOut, 0.75f, 1.0f, -0.08f),  // Follow-through
                },
                maxAngle: MathHelper.ToRadians(150),
                duration: 22,
                bladeLength: 140f,
                flip: false,
                squish: 0.85f,
                damageMult: 1.0f
            ),

            // Phase 1: CONVICTION ? Overhead slam (reversed direction)
            new ComboPhase(
                curves: new CurveSegment[]
                {
                    new CurveSegment(EasingType.SineIn, 0.0f, -0.20f, 0.20f),   // Extended windup
                    new CurveSegment(EasingType.PolyOut, 0.25f, 0.0f, 1.0f, 4), // Heavy slam
                    new CurveSegment(EasingType.SineOut, 0.70f, 1.0f, -0.05f),  // Ground bounce
                },
                maxAngle: MathHelper.ToRadians(170),
                duration: 28,
                bladeLength: 155f,
                flip: true,
                squish: 0.80f,
                damageMult: 1.3f
            ),

            // Phase 2: EXECUTION ? Spinning 270�� cleave
            new ComboPhase(
                curves: new CurveSegment[]
                {
                    new CurveSegment(EasingType.SineIn, 0.0f, -0.10f, 0.10f),   // Brief windup
                    new CurveSegment(EasingType.PolyOut, 0.12f, 0.0f, 1.0f, 2), // Explosive spin
                    new CurveSegment(EasingType.SineOut, 0.80f, 1.0f, -0.03f),  // Momentum carry
                },
                maxAngle: MathHelper.ToRadians(270),
                duration: 26,
                bladeLength: 160f,
                flip: false,
                squish: 0.90f,
                damageMult: 1.5f
            ),
        };

        // ???????????????????????????????????????????????????????????
        //  ABSTRACT OVERRIDES ? Required by MeleeSwingBase
        // ???????????????????????????????????????????????????????????

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
                _ => "MagnumOpus/Assets/VFX Asset Library/SlashArcs/FlamingSwordArcSmear",
            };
        }

        protected override string GetSmearGradientPath() => "MagnumOpus/Assets/VFX Asset Library/ColorGradients/DiesIraeGradientLUTandRAMP";

        // ???????????????????????????????????????????????????????????
        //  VIRTUAL OVERRIDES ? Dies Irae wrath theme
        // ???????????????????????????????????????????????????????????

        protected override int GetInitialDustType() => DustID.Torch;

        protected override int GetSecondaryDustType() => DustID.Torch;

        protected override SoundStyle GetSwingSound()
        {
            return ComboStep switch
            {
                0 => SoundID.Item71 with { Pitch = -0.3f, Volume = 0.8f },
                1 => SoundID.Item71 with { Pitch = -0.5f, Volume = 0.9f },
                2 => SoundID.Item71 with { Pitch = -0.2f, Volume = 1.0f },
                _ => SoundID.Item71 with { Pitch = -0.3f },
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
            return new Vector3(
                intensity * 1.0f,   // Strong red
                intensity * 0.3f,   // Low green
                intensity * 0.05f   // Minimal blue ? fire light
            );
        }

        // ???????????????????????????????????????????????????????????
        //  COMBO SPECIALS ? Per-phase unique mechanics
        // ???????????????????????????????????????????????????????????

        protected override void HandleComboSpecials()
        {
            float prog = Progression;

            // Phase 1 (Conviction) ? Smoke eruption at slam point
            if (ComboStep == 1 && !hasSpawnedSpecial && prog > 0.65f)
            {
                hasSpawnedSpecial = true;
                SpawnConvictionSmoke();
            }

            // Phase 2 (Execution) ? Shockwave ring at peak spin
            if (ComboStep == 2 && !hasSpawnedSpecial && prog > 0.50f)
            {
                hasSpawnedSpecial = true;
                SpawnExecutionShockwave();
            }

            // Continuous ember VFX during active swing
            if (prog > 0.1f && prog < 0.9f)
            {
                WrathsCleaverUtils.SpawnSwingDust(Owner.MountedCenter, SwordDirection,
                    CurrentPhase.BladeLength, ComboStep, prog, Direction);
            }
        }

        private void SpawnConvictionSmoke()
        {
            Vector2 slamPos = GetBladeTipPosition();

            // Heavy smoke burst ? dark hellsmoke puffs
            for (int i = 0; i < 15; i++)
            {
                float angle = MathHelper.TwoPi / 15f * i + Main.rand.NextFloat(-0.2f, 0.2f);
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 5f);

                Dust d = Dust.NewDustPerfect(slamPos + Main.rand.NextVector2Circular(10f, 10f),
                    DustID.Smoke, vel, 150,
                    DiesIraePalette.CharcoalBlack, Main.rand.NextFloat(1.5f, 2.5f));
                d.noGravity = false;
                d.fadeIn = 1.5f;
            }

            // Ember cores within the smoke
            for (int i = 0; i < 8; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(4f, 4f) + new Vector2(0, -3f);
                Dust d = Dust.NewDustPerfect(slamPos, DustID.Torch, vel, 0,
                    WrathsCleaverUtils.EmberOrange, Main.rand.NextFloat(0.8f, 1.4f));
                d.noGravity = true;
            }

            SoundEngine.PlaySound(SoundID.Item74 with { Pitch = -0.4f, Volume = 0.7f }, slamPos);
        }

        private void SpawnExecutionShockwave()
        {
            Vector2 center = Owner.MountedCenter;

            // Expanding ring of fire dust
            for (int i = 0; i < 20; i++)
            {
                float angle = MathHelper.TwoPi / 20f * i;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 8f);

                Dust d = Dust.NewDustPerfect(center, DustID.Torch, vel, 0,
                    WrathsCleaverUtils.GetPaletteColor(Main.rand.NextFloat(0.3f, 0.8f)),
                    Main.rand.NextFloat(1.2f, 2.0f));
                d.noGravity = true;
                d.fadeIn = 1.4f;
            }

            // Inner gold ring
            for (int i = 0; i < 12; i++)
            {
                float angle = MathHelper.TwoPi / 12f * i;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 4f);

                Dust d = Dust.NewDustPerfect(center, DustID.GoldFlame, vel, 0,
                    WrathsCleaverUtils.JudgmentGold, Main.rand.NextFloat(0.6f, 1.0f));
                d.noGravity = true;
            }

            SoundEngine.PlaySound(SoundID.Item45 with { Pitch = -0.5f, Volume = 0.8f }, center);
        }

        // ???????????????????????????????????????????????????????????
        //  HIT EFFECTS ? Dies Irae wrath-themed impacts
        // ???????????????????????????????????????????????????????????

        protected override void OnSwingHitNPC(NPC target, NPC.HitInfo hit, int remainingDamageCount)
        {
            // Apply debuffs ? Hellfire + vanilla fire
            target.AddBuff(ModContent.BuffType<HellfireImmolation>(), 180);
            target.AddBuff(BuffID.OnFire3, 240);

            // Wrath mark on execution phase for +25% damage amp
            if (ComboStep == 2)
                target.AddBuff(ModContent.BuffType<WrathMark>(), 300);

            // Multi-layered wrath impact VFX
            WrathsCleaverUtils.DoHitImpact(target.Center, ComboStep);

            // Extra impact sparks on heavy hits
            if (ComboStep >= 1)
            {
                int sparkCount = ComboStep == 2 ? 12 : 6;
                for (int i = 0; i < sparkCount; i++)
                {
                    Vector2 vel = Main.rand.NextVector2Circular(6f, 6f);
                    Dust d = Dust.NewDustPerfect(target.Center, DustID.Torch, vel, 0,
                        WrathsCleaverUtils.JudgmentGold, 0.5f);
                    d.noGravity = true;
                }
            }
        }

        // ???????????????????????????????????????????????????????????
        //  CUSTOM VFX ? Drawn after the standard pipeline
        // ???????????????????????????????????????????????????????????

        protected override void DrawCustomVFX(SpriteBatch sb)
        {
            if (Progression < 0.05f || Progression > 0.95f) return;

            Vector2 tipWorld = GetBladeTipPosition();
            Vector2 tipScreen = tipWorld - Main.screenPosition;

            // Switch to additive for glow rendering
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone,
                null, Main.GameViewMatrix.TransformationMatrix);

            // ── Layer 1: Tip bloom — theme star flares + bloom scales with combo
            float bloomIntensity = 0.6f + ComboStep * 0.15f;
            WrathsCleaverUtils.DrawTipBloom(sb, tipScreen, bloomIntensity, ComboStep);

            // ── Layer 2: Hellfire star flare accent at blade tip (theme texture)
            WrathsCleaverUtils.DrawHellfireStarFlare(sb, tipScreen, 1f + ComboStep * 0.3f, bloomIntensity * 0.8f);

            // ── Layer 3: Root glow at player center (ember aura)
            Vector2 rootScreen = Owner.MountedCenter - Main.screenPosition;
            float rootIntensity = 0.3f + ComboStep * 0.1f;
            Texture2D rootGlow = MagnumTextureRegistry.GetSoftGlow();
            if (rootGlow != null)
            {
                Vector2 rootOrigin = rootGlow.Size() * 0.5f;
                Color rootColor = WrathsCleaverUtils.BloodRed * (rootIntensity * 0.4f);
                rootColor.A = 0;
                sb.Draw(rootGlow, rootScreen, null, rootColor, 0f, rootOrigin, 0.12f, SpriteEffects.None, 0f);
            }

            // ── Layer 4: Execution phase — wrath aura + judgment impact ring
            if (ComboStep == 2 && Progression > 0.15f && Progression < 0.80f)
            {
                WrathsCleaverUtils.DrawBloomStack(sb, rootScreen, 0.03f, 0.5f, ComboStep);

                // Judgment impact ring expanding from player center
                float ringRotation = Main.GameUpdateCount * 0.02f;
                float ringIntensity = MathHelper.SmoothStep(0f, 1f, (Progression - 0.15f) / 0.45f);
                WrathsCleaverUtils.DrawJudgmentImpactRing(sb, rootScreen, 1f + ringIntensity * 2f, ringIntensity * 0.7f, ringRotation);
            }

            // ── Layer 5: Trailing slash burst at blade midpoint during active swing
            if (Progression > 0.15f && Progression < 0.85f)
            {
                Vector2 midBlade = Owner.MountedCenter + SwordDirection * CurrentPhase.BladeLength * 0.5f;
                Vector2 midScreen = midBlade - Main.screenPosition;
                float slashRot = SwordDirection.ToRotation();
                float slashIntensity = (float)Math.Sin((Progression - 0.15f) / 0.7f * MathHelper.Pi) * 0.6f;
                WrathsCleaverUtils.DrawRadialSlashBurst(sb, midScreen, 0.8f + ComboStep * 0.2f, slashIntensity, slashRot);
            }

            // Restore to normal blend
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone,
                null, Main.GameViewMatrix.TransformationMatrix);
        }
    }
}