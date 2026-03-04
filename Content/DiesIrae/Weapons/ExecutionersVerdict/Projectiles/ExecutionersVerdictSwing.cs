using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.BaseClasses;
using MagnumOpus.Common.Systems.VFX.Trails;
using MagnumOpus.Common.Systems.VFX.Core;
using MagnumOpus.Content.DiesIrae.Weapons.ExecutionersVerdict.Utilities;
using MagnumOpus.Content.DiesIrae.Weapons.ExecutionersVerdict.Buffs;
using MagnumOpus.Common.Systems.VFX;
using static MagnumOpus.Common.Systems.Particles.Particle;

namespace MagnumOpus.Content.DiesIrae.Weapons.ExecutionersVerdict.Projectiles
{
    /// <summary>
    /// EXECUTIONER'S VERDICT ? Melee Swing Projectile.
    /// Extends MeleeSwingBase for the full 6-layer pipeline.
    ///
    /// 3-Phase Judgment Combo:
    ///   Phase 0 (Arraignment) ? Overhead strike, applies first Judgment Mark
    ///   Phase 1 (Cross-Examination) ? Cross slash at �}45��, heavier damage to marked
    ///   Phase 2 (The Verdict) ? Horizontal execution slash, triggers Verdict Execution at 3 marks
    ///
    /// VFX traits: Clean, sharp, deliberate (not chaotic like Wrath's Cleaver).
    /// Controlled distortion, sharper palette transitions.
    /// </summary>
    public class ExecutionersVerdictSwing : MeleeSwingBase
    {
        // ???????????????????????????????????????????????????????????
        //  COMBO PHASES ? Three movements of judicial process
        // ???????????????????????????????????????????????????????????

        private static readonly ComboPhase[] _phases = new ComboPhase[]
        {
            // Phase 0: ARRAIGNMENT ? Overhead strike, deliberate and heavy
            new ComboPhase(
                curves: new CurveSegment[]
                {
                    new CurveSegment(EasingType.SineIn, 0.0f, -0.18f, 0.18f),   // Deliberate windup
                    new CurveSegment(EasingType.PolyOut, 0.20f, 0.0f, 1.0f, 3), // Controlled descent
                    new CurveSegment(EasingType.SineOut, 0.72f, 1.0f, -0.06f),  // Measured follow-through
                },
                maxAngle: MathHelper.ToRadians(160),
                duration: 26,
                bladeLength: 170f,   // Larger weapon
                flip: false,
                squish: 0.82f,
                damageMult: 1.0f
            ),

            // Phase 1: CROSS-EXAMINATION ? Reversed cross slash
            new ComboPhase(
                curves: new CurveSegment[]
                {
                    new CurveSegment(EasingType.SineIn, 0.0f, -0.15f, 0.15f),   // Quick reverse
                    new CurveSegment(EasingType.PolyOut, 0.15f, 0.0f, 1.0f, 3), // Sharp cross cut
                    new CurveSegment(EasingType.SineOut, 0.68f, 1.0f, -0.07f),  // Snap back
                },
                maxAngle: MathHelper.ToRadians(140),
                duration: 24,
                bladeLength: 165f,
                flip: true,
                squish: 0.84f,
                damageMult: 1.25f   // Marked enemies already take +25%, layered
            ),

            // Phase 2: THE VERDICT ? Massive horizontal execution slash
            new ComboPhase(
                curves: new CurveSegment[]
                {
                    new CurveSegment(EasingType.SineIn, 0.0f, -0.22f, 0.22f),   // Grand windup
                    new CurveSegment(EasingType.PolyOut, 0.25f, 0.0f, 1.0f, 4), // Devastating horizontal
                    new CurveSegment(EasingType.SineOut, 0.75f, 1.0f, -0.04f),  // Finality
                },
                maxAngle: MathHelper.ToRadians(200),
                duration: 30,
                bladeLength: 180f,   // Maximum reach for the verdict
                flip: false,
                squish: 0.78f,
                damageMult: 1.6f
            ),
        };

        // ???????????????????????????????????????????????????????????
        //  ABSTRACT OVERRIDES
        // ???????????????????????????????????????????????????????????

        protected override ComboPhase[] GetAllPhases() => _phases;

        protected override Color[] GetPalette() => ExecutionersVerdictUtils.SwingPalette;

        protected override CalamityStyleTrailRenderer.TrailStyle GetTrailStyle()
            => CalamityStyleTrailRenderer.TrailStyle.Flame;

        protected override string GetSmearTexturePath(int comboStep)
        {
            // Controlled flame arcs ? cleaner than Wrath's chaotic fire
            return comboStep switch
            {
                0 => "MagnumOpus/Assets/VFX Asset Library/SlashArcs/SwordArcSmear",
                1 => "MagnumOpus/Assets/VFX Asset Library/SlashArcs/FlamingSwordArcSmear",
                2 => "MagnumOpus/Assets/VFX Asset Library/SlashArcs/FullCircleSwordArcSlash",
                _ => "MagnumOpus/Assets/VFX Asset Library/SlashArcs/SwordArcSmear",
            };
        }

        // ???????????????????????????????????????????????????????????
        //  VIRTUAL OVERRIDES ? Judgment theme
        // ???????????????????????????????????????????????????????????

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
            return new Vector3(
                intensity * 0.9f,   // Blood red
                intensity * 0.15f,  // Very low green
                intensity * 0.05f   // Minimal blue
            );
        }

        // ???????????????????????????????????????????????????????????
        //  COMBO SPECIALS ? Judgment Mark application VFX
        // ???????????????????????????????????????????????????????????

        protected override void HandleComboSpecials()
        {
            float prog = Progression;

            // Phase 2 (The Verdict) ? Gold judgment flash at peak
            if (ComboStep == 2 && !hasSpawnedSpecial && prog > 0.55f)
            {
                hasSpawnedSpecial = true;
                SpawnVerdictFlash();
            }

            // Controlled ember trail during swing
            if (prog > 0.12f && prog < 0.88f)
            {
                ExecutionersVerdictUtils.SpawnSwingDust(Owner.MountedCenter, SwordDirection,
                    CurrentPhase.BladeLength, ComboStep, prog, Direction);
            }
        }

        private void SpawnVerdictFlash()
        {
            Vector2 tipPos = GetBladeTipPosition();

            // Gold judgment flash ? clean, bright, authoritative
            for (int i = 0; i < 12; i++)
            {
                float angle = MathHelper.TwoPi / 12f * i;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 6f);

                Dust d = Dust.NewDustPerfect(tipPos, DustID.GoldFlame, vel, 0,
                    ExecutionersVerdictUtils.JudgmentGold, Main.rand.NextFloat(1.0f, 1.6f));
                d.noGravity = true;
                d.fadeIn = 1.3f;
            }

            // Crimson ring accent
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi / 8f * i + MathHelper.PiOver4;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 4f);

                Dust d = Dust.NewDustPerfect(tipPos, DustID.Torch, vel, 0,
                    ExecutionersVerdictUtils.BloodCrimson, 0.9f);
                d.noGravity = true;
            }

            SoundEngine.PlaySound(SoundID.Item73 with { Pitch = 0.1f, Volume = 0.7f }, tipPos);
        }

        // ???????????????????????????????????????????????????????????
        //  HIT EFFECTS ? Judgment marks + execution mechanics
        // ???????????????????????????????????????????????????????????

        protected override void OnSwingHitNPC(NPC target, NPC.HitInfo hit, int remainingDamageCount)
        {
            // Apply debuffs
            target.AddBuff(ModContent.BuffType<ExecutionBrand>(), 360);
            target.AddBuff(ModContent.BuffType<PyreImmolation>(), 180);

            // Execution threshold ? 15% HP insta-kill for non-bosses
            if (!target.boss && target.life < target.lifeMax * 0.15f)
            {
                target.life = 0;
                target.checkDead();

                // Execution flash
                for (int i = 0; i < 20; i++)
                {
                    Vector2 vel = Main.rand.NextVector2Circular(8f, 8f);
                    Dust d = Dust.NewDustPerfect(target.Center, DustID.GoldFlame, vel, 0,
                        ExecutionersVerdictUtils.JudgmentGold, 1.5f);
                    d.noGravity = true;
                }
                return;
            }

            // +50% damage to enemies below 30% HP (applied via damage modifiers in the item)
            // Visual: extra crimson sparks
            if (target.life < target.lifeMax * 0.30f)
            {
                for (int i = 0; i < 6; i++)
                {
                    Vector2 vel = Main.rand.NextVector2Circular(5f, 5f);
                    Dust d = Dust.NewDustPerfect(target.Center, DustID.Torch, vel, 0,
                        ExecutionersVerdictUtils.CrimsonRed, 1.2f);
                    d.noGravity = true;
                }
            }

            // Standard judgment impact
            ExecutionersVerdictUtils.DoHitImpact(target.Center, ComboStep);
        }

        // ???????????????????????????????????????????????????????????
        //  CUSTOM VFX ? Judicial precision rendering
        // ???????????????????????????????????????????????????????????

        protected override void DrawCustomVFX(SpriteBatch sb)
        {
            if (Progression < 0.05f || Progression > 0.95f) return;

            Vector2 tipWorld = GetBladeTipPosition();
            Vector2 tipScreen = tipWorld - Main.screenPosition;

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive,
                SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone,
                null, Main.GameViewMatrix.TransformationMatrix);

            // Controlled tip bloom ? precise, not wild
            float bloomIntensity = 0.55f + ComboStep * 0.18f;
            ExecutionersVerdictUtils.DrawTipBloom(sb, tipScreen, bloomIntensity, ComboStep);

            // Player root glow ? subtle crimson judicial authority
            Vector2 rootScreen = Owner.MountedCenter - Main.screenPosition;
            Texture2D rootGlow = MagnumTextureRegistry.GetSoftGlow();
            if (rootGlow != null)
            {
                Vector2 rootOrigin = rootGlow.Size() * 0.5f;
                float rootAlpha = 0.25f + ComboStep * 0.08f;
                Color rootColor = ExecutionersVerdictUtils.BloodCrimson * (rootAlpha * 0.35f);
                rootColor.A = 0;
                sb.Draw(rootGlow, rootScreen, null, rootColor, 0f, rootOrigin, 0.10f, SpriteEffects.None, 0f);
            }

            // Verdict phase: intensified judicial aura
            if (ComboStep == 2 && Progression > 0.20f && Progression < 0.75f)
            {
                ExecutionersVerdictUtils.DrawBloomStack(sb, rootScreen, 0.025f, 0.45f, ComboStep);
            }

            // Dies Irae theme accent layer
            ExecutionersVerdictUtils.DrawThemeAccents(sb, tipWorld, 1f, Progression);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone,
                null, Main.GameViewMatrix.TransformationMatrix);
        }
    }
}