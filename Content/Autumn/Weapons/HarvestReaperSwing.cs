using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.BaseClasses;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.VFX.Trails;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Content.Autumn.Projectiles;
using static MagnumOpus.Common.Systems.Particles.Particle;
using ReLogic.Content;

namespace MagnumOpus.Content.Autumn.Weapons
{
    /// <summary>
    /// Harvest Reaper held-projectile swing 窶・the slow dirge of autumn's decay.
    /// 4-phase scythe combo: Reaping Sweep 竊・Decay Slash 竊・Soul Rend 竊・Twilight Judgement.
    /// Every 5th hit applies Autumn's Decay (Ichor + seeking crystals);
    /// kills spawn soul wisps; every 8th COMBO triggers Twilight Slash (DecayCrescentWave).
    /// </summary>
    public sealed class HarvestReaperSwing : MeleeSwingBase
    {
        // 笏笏 Theme Colors 笏笏
        private static readonly Color AutumnOrange = MagnumThemePalettes.AutumnOrange;
        private static readonly Color AutumnBrown = MagnumThemePalettes.AutumnBrown;
        private static readonly Color AutumnRed = MagnumThemePalettes.AutumnRed;
        private static readonly Color AutumnGold = MagnumThemePalettes.AutumnHarvestGold;
        private static readonly Color DecayPurple = MagnumThemePalettes.AutumnDecayPurple;

        // 笏笏 Counters (stored in ai[2]) 笏笏
        private int HitCounter
        {
            get => (int)Projectile.ai[2];
            set => Projectile.ai[2] = value;
        }

        // 笏笏 6-Color Palette: pianissimo 竊・sforzando 笏笏
        private static readonly Color[] AutumnPalette = new Color[]
        {
            new Color(80, 40, 20),      // [0] Dark bark shadow
            new Color(139, 90, 43),     // [1] Autumn brown
            new Color(255, 140, 50),    // [2] Autumn orange
            new Color(218, 165, 32),    // [3] Harvest gold
            new Color(255, 200, 100),   // [4] Bright amber
            new Color(255, 245, 220),   // [5] White-hot harvest core
        };

        #region 笏笏 Combo Phase Definitions 笏笏

        // Phase 0 窶・Reaping Sweep (slow, wide horizontal arc 窶・the first leaves falling)
        private static readonly ComboPhase Phase0_ReapingSweep = new ComboPhase(
            curves: new CurveSegment[]
            {
                new CurveSegment(EasingType.PolyOut, 0f, -0.9f, 0.2f, 2),       // Pull back
                new CurveSegment(EasingType.PolyIn, 0.20f, -0.7f, 1.5f, 3),     // Wide reap
                new CurveSegment(EasingType.PolyOut, 0.78f, 0.8f, 0.2f, 2),     // Settle
            },
            maxAngle: MathHelper.Pi * 1.3f,
            duration: 30,
            bladeLength: 130f,
            flip: false,
            squish: 0.88f,
            damageMult: 0.9f
        );

        // Phase 1 窶・Decay Slash (quick backhand 窶・the wind scattering dead leaves)
        private static readonly ComboPhase Phase1_DecaySlash = new ComboPhase(
            curves: new CurveSegment[]
            {
                new CurveSegment(EasingType.PolyOut, 0f, -0.7f, 0.15f, 2),      // Tiny windup
                new CurveSegment(EasingType.PolyIn, 0.15f, -0.55f, 1.45f, 3),   // Fast reverse cut
                new CurveSegment(EasingType.PolyOut, 0.72f, 0.9f, 0.1f, 2),     // Quick end
            },
            maxAngle: MathHelper.Pi * 1.2f,
            duration: 24,
            bladeLength: 120f,
            flip: true,
            squish: 0.90f,
            damageMult: 1.0f
        );

        // Phase 2 窶・Soul Rend (overhead slam 窶・the weight of autumn's sorrow)
        private static readonly ComboPhase Phase2_SoulRend = new ComboPhase(
            curves: new CurveSegment[]
            {
                new CurveSegment(EasingType.PolyOut, 0f, -1.1f, 0.3f, 2),       // Deep overhead pull
                new CurveSegment(EasingType.PolyIn, 0.28f, -0.8f, 1.7f, 4),     // Heavy slam
                new CurveSegment(EasingType.PolyOut, 0.82f, 0.9f, 0.15f, 2),    // Impact settle
            },
            maxAngle: MathHelper.Pi * 1.5f,
            duration: 32,
            bladeLength: 140f,
            flip: false,
            squish: 0.84f,
            damageMult: 1.15f
        );

        // Phase 3 窶・Twilight Judgement (massive finisher 窶・the last light fading)
        private static readonly ComboPhase Phase3_TwilightJudgement = new ComboPhase(
            curves: new CurveSegment[]
            {
                new CurveSegment(EasingType.PolyOut, 0f, -1.3f, 0.35f, 2),      // Long wind-back
                new CurveSegment(EasingType.PolyIn, 0.30f, -0.95f, 2.0f, 4),    // Devastating arc
                new CurveSegment(EasingType.PolyOut, 0.88f, 1.05f, 0.1f, 2),    // Overshoot
            },
            maxAngle: MathHelper.Pi * 1.8f,
            duration: 38,
            bladeLength: 150f,
            flip: true,
            squish: 0.82f,
            damageMult: 1.35f
        );

        #endregion

        #region 笏笏 Abstract Overrides 笏笏

        protected override ComboPhase[] GetAllPhases() => new ComboPhase[]
        {
            Phase0_ReapingSweep,
            Phase1_DecaySlash,
            Phase2_SoulRend,
            Phase3_TwilightJudgement,
        };

        protected override Color[] GetPalette() => AutumnPalette;

        protected override CalamityStyleTrailRenderer.TrailStyle GetTrailStyle()
            => CalamityStyleTrailRenderer.TrailStyle.Nature;

        protected override string GetSmearTexturePath(int comboStep) => comboStep switch
        {
            0 => "MagnumOpus/Assets/VFX Asset Library/MasksAndShapes/VerticalEllipse",
            1 => "MagnumOpus/Assets/VFX Asset Library/ImpactEffects/ImpactEllipse",
            2 => "MagnumOpus/Assets/VFX Asset Library/MasksAndShapes/VerticalEllipse",
            3 => "MagnumOpus/Assets/VFX Asset Library/ImpactEffects/ImpactEllipse",
            _ => "MagnumOpus/Assets/VFX Asset Library/MasksAndShapes/VerticalEllipse",
        };
        protected override string GetSmearGradientPath() => "MagnumOpus/Assets/VFX Asset Library/ColorGradients/LaCampanellaGradientLUTandRAMP";
        #endregion

        #region 笏笏 Virtual Overrides 笏笏

        protected override SoundStyle GetSwingSound()
        {
            return SoundID.Item71 with
            {
                Pitch = -0.3f + ComboStep * 0.12f,
                Volume = 0.85f,
            };
        }

        protected override int GetInitialDustType() => DustID.Torch;

        protected override int GetSecondaryDustType() => DustID.GoldCoin;

        protected override Texture2D GetBladeTexture()
        {
            return ModContent.Request<Texture2D>("MagnumOpus/Content/Autumn/Weapons/HarvestReaper", AssetRequestMode.ImmediateLoad).Value;
        }

        protected override Vector3 GetLightColor()
        {
            return AutumnOrange.ToVector3() * (0.40f + ComboStep * 0.10f);
        }

        #endregion

        #region 笏笏 Combo Specials 笏笏

        protected override void HandleComboSpecials()
        {
            if (hasSpawnedSpecial) return;

            // Phase 2 at ~70%: spawn decay bolt sub-projectiles at blade tip
            if (ComboStep == 2 && Progression >= 0.70f)
            {
                hasSpawnedSpecial = true;
                if (Main.myPlayer == Projectile.owner)
                {
                    Vector2 tipPos = GetBladeTipPosition();
                    for (int i = 0; i < 4; i++)
                    {
                        float spread = MathHelper.ToRadians(-35f + i * 23f);
                        Vector2 vel = SwordDirection.RotatedBy(spread) * Main.rand.NextFloat(7f, 10f);
                        Projectile.NewProjectile(
                            Projectile.GetSource_FromThis(),
                            tipPos,
                            vel,
                            ModContent.ProjectileType<DecayBoltProjectile>(),
                            Projectile.damage / 3,
                            Projectile.knockBack * 0.4f,
                            Projectile.owner
                        );
                    }
                }
            }

            // Phase 3 at ~85%: Twilight Slash finisher 窶・spawn DecayCrescentWave
            if (ComboStep == 3 && Progression >= 0.85f)
            {
                hasSpawnedSpecial = true;
                if (Main.myPlayer == Projectile.owner)
                {
                    Vector2 tipPos = GetBladeTipPosition();
                    Vector2 waveVel = SwordDirection * 14f;

                    Projectile.NewProjectile(
                        Projectile.GetSource_FromThis(),
                        tipPos,
                        waveVel,
                        ModContent.ProjectileType<DecayCrescentWave>(),
                        (int)(Projectile.damage * 1.6f),
                        Projectile.knockBack,
                        Projectile.owner
                    );

                    // Seeking autumn crystals
                    SeekingCrystalHelper.SpawnAutumnCrystals(
                        Projectile.GetSource_FromThis(),
                        tipPos,
                        SwordDirection * 6f,
                        (int)(Projectile.damage * 0.35f),
                        Projectile.knockBack * 0.3f,
                        Projectile.owner,
                        count: 4
                    );

                    // Twilight Slash VFX burst
                    SoundEngine.PlaySound(SoundID.Item73 with { Pitch = -0.4f, Volume = 1f }, tipPos);
                }
            }

            // 笏笏 Dense dust + leaf particles every frame during active swing 笏笏
            if (Progression > 0.10f && Progression < 0.92f)
            {
                Vector2 tipPos = GetBladeTipPosition();
                float bladeLen = CurrentPhase.BladeLength;

                // Autumn torch dust 窶・dense, 2 per frame
                for (int i = 0; i < 2; i++)
                {
                    Vector2 dustPos = Owner.MountedCenter + SwordDirection * bladeLen * Main.rand.NextFloat(0.4f, 1f);
                    Dust d = Dust.NewDustPerfect(dustPos, DustID.Torch,
                        -SwordDirection * Main.rand.NextFloat(1f, 3f) + Main.rand.NextVector2Circular(1f, 1f),
                        0, AutumnOrange, 1.3f);
                    d.noGravity = true;
                    d.fadeIn = 1.2f;
                }

                // Contrasting gold coin sparkle every other frame
                if (Main.GameUpdateCount % 2 == 0)
                {
                    Dust g = Dust.NewDustPerfect(
                        Owner.MountedCenter + SwordDirection * bladeLen * Main.rand.NextFloat(0.5f, 0.9f),
                        DustID.GoldCoin,
                        -SwordDirection * Main.rand.NextFloat(0.5f, 2f),
                        0, AutumnGold, 1.0f);
                    g.noGravity = true;
                }

                // Falling leaf particles (1-in-3)
                if (Main.rand.NextBool(3))
                {
                    Vector2 leafPos = Owner.MountedCenter + SwordDirection * bladeLen * Main.rand.NextFloat(0.3f, 1f);
                    Vector2 leafVel = new Vector2(Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(0.5f, 2f));
                    Color leafColor = Main.rand.NextBool() ? AutumnOrange : AutumnRed;
                    var leaf = new GenericGlowParticle(leafPos, leafVel, leafColor * 0.65f, 0.28f, 30, true);
                    MagnumParticleHandler.SpawnParticle(leaf);
                }

                // Music notes from blade tip (1-in-4 chance, visible scale)
                if (Main.rand.NextBool(4))
                {
                    float noteScale = Main.rand.NextFloat(0.7f, 0.95f);
                    float shimmer = 1f + MathF.Sin(Main.GameUpdateCount * 0.15f) * 0.12f;
                    Color noteColor = Color.Lerp(AutumnOrange, AutumnGold, Main.rand.NextFloat());
                    Dust note = Dust.NewDustPerfect(
                        tipPos + Main.rand.NextVector2Circular(6f, 6f),
                        DustID.Enchanted_Gold,
                        -SwordDirection * 1.5f + Main.rand.NextVector2Circular(1f, 1f),
                        0, noteColor, noteScale * shimmer * 1.6f);
                    note.noGravity = true;
                }
            }
        }

        #endregion

        #region 笏笏 On Hit NPC 笏笏

        protected override void OnSwingHitNPC(NPC target, NPC.HitInfo hit, int remainingDamageCount)
        {
            Player owner = Main.player[Projectile.owner];

            HitCounter++;

            // 笏笏 Gradient halo rings 窶・orange 竊・purple 笏笏
            for (int i = 0; i < 4; i++)
            {
                float progress = i / 4f;
                Color ringColor = Color.Lerp(AutumnOrange, DecayPurple, progress);
                for (int j = 0; j < 2; j++)
                {
                    float angle = MathHelper.TwoPi * j / 2f + i * MathHelper.PiOver4;
                    Vector2 offset = angle.ToRotationVector2() * (15f + i * 8f);
                    Dust ring = Dust.NewDustPerfect(target.Center + offset, DustID.Torch,
                        offset.SafeNormalize(Vector2.Zero) * 2f, 0, ringColor, 1.2f);
                    ring.noGravity = true;
                }
            }

            // Shimmer flares
            for (int i = 0; i < 3; i++)
            {
                Dust shimmer = Dust.NewDustPerfect(target.Center + Main.rand.NextVector2Circular(12f, 12f),
                    DustID.Enchanted_Gold,
                    Main.rand.NextVector2Circular(3f, 3f), 0, AutumnGold, 1.4f);
                shimmer.noGravity = true;
            }

            // Radial dust burst
            for (int i = 0; i < 10; i++)
            {
                Dust burst = Dust.NewDustPerfect(target.Center,
                    Main.rand.NextBool() ? DustID.Torch : DustID.GoldCoin,
                    Main.rand.NextVector2Circular(5f, 5f), 0,
                    Color.Lerp(AutumnOrange, AutumnRed, Main.rand.NextFloat()), 1.3f);
                burst.noGravity = true;
                burst.fadeIn = 1.2f;
            }

            // Falling leaves on hit
            for (int i = 0; i < 4; i++)
            {
                Vector2 leafVel = new Vector2(Main.rand.NextFloat(-3f, 3f), -Main.rand.NextFloat(2f, 4f));
                Color leafColor = Main.rand.NextBool() ? AutumnOrange : AutumnRed;
                var leaf = new GenericGlowParticle(target.Center, leafVel, leafColor * 0.7f, 0.28f, 30, true);
                MagnumParticleHandler.SpawnParticle(leaf);
            }

            // 笏笏 AUTUMN'S DECAY 窶・every 5th hit: Ichor + seeking crystals 笏笏
            if (HitCounter >= 5)
            {
                HitCounter = 0;

                target.AddBuff(BuffID.Ichor, 300);

                if (Main.myPlayer == Projectile.owner)
                {
                    SeekingCrystalHelper.SpawnAutumnCrystals(
                        Projectile.GetSource_FromThis(),
                        target.Center,
                        (target.Center - owner.Center).SafeNormalize(Vector2.Zero) * 4f,
                        (int)(Projectile.damage * 0.4f),
                        Projectile.knockBack * 0.3f,
                        Projectile.owner,
                        count: 5
                    );
                }

                // Decay VFX burst
                SoundEngine.PlaySound(SoundID.Item103 with { Pitch = -0.3f, Volume = 0.6f }, target.Center);
                for (int i = 0; i < 8; i++)
                {
                    float angle = MathHelper.TwoPi * i / 8f;
                    Vector2 decayVel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 7f);
                    Color decayColor = Color.Lerp(DecayPurple, AutumnBrown, Main.rand.NextFloat()) * 0.6f;
                    Dust decay = Dust.NewDustPerfect(target.Center, DustID.PurpleTorch,
                        decayVel, 0, decayColor, 1.5f);
                    decay.noGravity = true;
                }
            }

            // 笏笏 SOUL HARVEST 窶・kills spawn healing wisp 笏笏
            if (target.life <= 0)
            {
                if (Main.myPlayer == Projectile.owner && Main.netMode != NetmodeID.MultiplayerClient)
                {
                    Projectile.NewProjectile(
                        Projectile.GetSource_FromThis(),
                        target.Center,
                        Vector2.Zero,
                        ModContent.ProjectileType<SoulWisp>(),
                        0, 0, Projectile.owner
                    );
                }

                // Soul release VFX
                for (int i = 0; i < 6; i++)
                {
                    Vector2 soulVel = Main.rand.NextVector2Circular(4f, 4f) + new Vector2(0, -2f);
                    Color soulColor = Color.Lerp(AutumnGold, Color.White, Main.rand.NextFloat()) * 0.6f;
                    Dust soul = Dust.NewDustPerfect(target.Center, DustID.Enchanted_Gold,
                        soulVel, 0, soulColor, 1.6f);
                    soul.noGravity = true;
                }
            }

            // Music note burst on hit
            for (int n = 0; n < 3; n++)
            {
                float angle = MathHelper.TwoPi * n / 3f + Main.rand.NextFloat(-0.4f, 0.4f);
                Vector2 noteVel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 4f);
                Color noteColor = Color.Lerp(AutumnOrange, AutumnGold, Main.rand.NextFloat());
                Dust note = Dust.NewDustPerfect(target.Center, DustID.Enchanted_Gold,
                    noteVel, 0, noteColor, Main.rand.NextFloat(1.4f, 1.8f));
                note.noGravity = true;
            }
        }

        #endregion

        #region 笏笏 Custom VFX 笏笏

        protected override void DrawCustomVFX(SpriteBatch sb)
        {
            if (Progression < 0.08f || Progression > 0.95f) return;

            Vector2 tipWorld = GetBladeTipPosition();
            Vector2 tipScreen = tipWorld - Main.screenPosition;
            Vector2 rootScreen = Owner.MountedCenter - Main.screenPosition;
            float phaseIntensity = 1f + ComboStep * 0.12f;
            float pulse = 1f + MathF.Sin(Main.GlobalTimeWrappedHourly * 4f) * 0.12f;
            float swingFade = MathHelper.Clamp((Progression - 0.08f) / 0.08f, 0f, 1f)
                            * MathHelper.Clamp((0.95f - Progression) / 0.08f, 0f, 1f);

            // ═══ Switch to additive for all glow layers ═══
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                SamplerState.LinearClamp, DepthStencilState.None,
                RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            Texture2D glowTex = MagnumTextureRegistry.GetSoftGlow();
            Texture2D starTex = MagnumTextureRegistry.GetStar4Soft();
            if (glowTex != null)
            {
                Vector2 glowOrigin = glowTex.Size() / 2f;
                float baseScale = MathHelper.Min((0.18f + ComboStep * 0.06f) * pulse * phaseIntensity, 0.335f);

                // Layer 1: Wide decay haze — the creeping autumn shadow (capped 300px max)
                sb.Draw(glowTex, tipScreen, null,
                    DecayPurple with { A = 0 } * 0.2f * swingFade, 0f,
                    glowOrigin, baseScale * 1.75f, SpriteEffects.None, 0f);

                // Layer 2: Warm autumn orange glow
                sb.Draw(glowTex, tipScreen, null,
                    AutumnOrange with { A = 0 } * 0.45f * swingFade, 0f,
                    glowOrigin, baseScale * 1.6f, SpriteEffects.None, 0f);

                // Layer 3: Inner harvest gold — the ripened core
                sb.Draw(glowTex, tipScreen, null,
                    AutumnGold with { A = 0 } * 0.6f * swingFade, 0f,
                    glowOrigin, baseScale * 0.9f, SpriteEffects.None, 0f);

                // Layer 4: Bright harvest ember core
                Color hotCore = Color.Lerp(AutumnGold, Color.White, 0.5f);
                sb.Draw(glowTex, tipScreen, null,
                    hotCore with { A = 0 } * 0.75f * swingFade, 0f,
                    glowOrigin, baseScale * 0.35f, SpriteEffects.None, 0f);

                // Layer 5: Root glow — warm ember at sword base
                float rootPulse = 0.6f + 0.4f * MathF.Sin(Main.GlobalTimeWrappedHourly * 2.5f);
                sb.Draw(glowTex, rootScreen, null,
                    AutumnBrown with { A = 0 } * 0.25f * swingFade * rootPulse, 0f,
                    glowOrigin, 0.12f * phaseIntensity, SpriteEffects.None, 0f);

                // Layer 6: Trailing decay echo at 35% blade length
                Vector2 echoScreen = Vector2.Lerp(rootScreen, tipScreen, 0.35f);
                sb.Draw(glowTex, echoScreen, null,
                    DecayPurple with { A = 0 } * 0.18f * swingFade, 0f,
                    glowOrigin, baseScale * 0.5f, SpriteEffects.None, 0f);
            }

            // Layer 7: Ember star flare at tip — the reaper's lantern
            if (starTex != null)
            {
                Vector2 starOrigin = starTex.Size() / 2f;
                float starRot = Main.GlobalTimeWrappedHourly * 2.5f;
                float starScale = (0.07f + ComboStep * 0.025f) * pulse;
                sb.Draw(starTex, tipScreen, null,
                    AutumnOrange with { A = 0 } * 0.65f * swingFade, starRot,
                    starOrigin, starScale, SpriteEffects.None, 0f);
                sb.Draw(starTex, tipScreen, null,
                    AutumnGold with { A = 0 } * 0.4f * swingFade, -starRot * 0.6f,
                    starOrigin, starScale * 0.55f, SpriteEffects.None, 0f);
            }

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                SamplerState.LinearClamp, DepthStencilState.None,
                RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            // ═══ Falling leaf dust along blade ═══
            if (Progression > 0.12f && Progression < 0.88f)
            {
                int dustCount = 1 + ComboStep;
                for (int i = 0; i < dustCount; i++)
                {
                    float t = Main.rand.NextFloat(0.25f, 0.85f);
                    Vector2 dustPos = Vector2.Lerp(Owner.MountedCenter, tipWorld, t);
                    Vector2 drift = new Vector2(
                        Main.rand.NextFloat(-1f, 1f),
                        Main.rand.NextFloat(0.3f, 1.5f));
                    Color leafColor = Color.Lerp(AutumnOrange, AutumnRed, Main.rand.NextFloat());
                    Dust leaf = Dust.NewDustPerfect(dustPos, DustID.AmberBolt, drift, 0, leafColor, 0.7f);
                    leaf.noGravity = true;
                    leaf.fadeIn = 0.5f;
                }
            }

            // ═══ Rising embers from tip (upward drift) ═══
            if (Main.rand.NextBool(3))
            {
                Vector2 emberVel = new Vector2(Main.rand.NextFloat(-0.4f, 0.4f), Main.rand.NextFloat(-2f, -0.8f));
                Dust ember = Dust.NewDustPerfect(tipWorld, DustID.Torch, emberVel, 0, AutumnGold, 0.6f);
                ember.noGravity = true;
            }

            // ═══ Music note particles ═══
            if (Main.rand.NextBool(4))
            {
                Vector2 noteVel = (Projectile.velocity.SafeNormalize(Vector2.UnitX) * -1.5f).RotatedByRandom(0.4);
                MagnumParticleHandler.SpawnParticle(new HueShiftingMusicNoteParticle(
                    tipWorld, noteVel, 0.06f, 0.12f, 0.90f, 0.55f, 0.75f, 25, 0.025f));
            }
        }

        #endregion
    }
}