using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.BaseClasses;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.VFX.Trails;
using MagnumOpus.Content.Eroica.Projectiles;
using MagnumOpus.Content.MoonlightSonata.Debuffs;

namespace MagnumOpus.Content.Eroica.Weapons.SakurasBlossom
{
    /// <summary>
    /// Swing projectile for Sakura's Blossom — Eroica's blooming sword of spring.
    /// 4-phase sakura combo: Petal Slash → Crimson Scatter → Blossom Bloom → Storm of Petals.
    /// Each phase spawns increasing numbers of spectral homing copies (SakurasBlossomSpectral).
    /// The blade literally blooms with petals — a flower unfurling across four movements.
    /// </summary>
    public sealed class SakurasBlossomSwing : MeleeSwingBase
    {
        #region Theme Colors

        // 6-color Sakura palette — bud to full bloom (delegates to EroicaPalette)
        private static readonly Color[] SakuraPalette = EroicaPalette.SakurasBlossomBlade;

        #endregion

        #region Combo Phases

        // Phase 0: Petal Slash — quick horizontal opener, petals scatter
        private static readonly ComboPhase Phase0_PetalSlash = new ComboPhase(
            curves: new CurveSegment[]
            {
                new CurveSegment(EasingType.PolyOut, 0f, -0.85f, 0.18f, 2),
                new CurveSegment(EasingType.PolyIn, 0.18f, -0.67f, 1.45f, 3),
                new CurveSegment(EasingType.PolyOut, 0.80f, 0.78f, 0.12f, 2)
            },
            maxAngle: MathHelper.PiOver2 * 1.4f,
            duration: 26,
            bladeLength: 155f,
            flip: false,
            squish: 0.92f,
            damageMult: 0.85f
        );

        // Phase 1: Crimson Scatter — backhand that tosses spectral copies wide
        private static readonly ComboPhase Phase1_CrimsonScatter = new ComboPhase(
            curves: new CurveSegment[]
            {
                new CurveSegment(EasingType.PolyOut, 0f, 0.9f, -0.2f, 2),
                new CurveSegment(EasingType.PolyIn, 0.2f, 0.7f, -1.6f, 3),
                new CurveSegment(EasingType.PolyOut, 0.82f, -0.9f, -0.1f, 2)
            },
            maxAngle: MathHelper.PiOver2 * 1.6f,
            duration: 28,
            bladeLength: 158f,
            flip: true,
            squish: 0.90f,
            damageMult: 1.0f
        );

        // Phase 2: Blossom Bloom — rising arc, pollen explodes from blade
        private static readonly ComboPhase Phase2_BlossomBloom = new ComboPhase(
            curves: new CurveSegment[]
            {
                new CurveSegment(EasingType.SineOut, 0f, -1.0f, 0.25f, 2),
                new CurveSegment(EasingType.PolyIn, 0.25f, -0.75f, 1.8f, 3),
                new CurveSegment(EasingType.PolyOut, 0.84f, 1.05f, 0.08f, 2)
            },
            maxAngle: MathHelper.PiOver2 * 1.9f,
            duration: 32,
            bladeLength: 165f,
            flip: false,
            squish: 0.86f,
            damageMult: 1.15f
        );

        // Phase 3: Storm of Petals — massive slam, sakura storm erupts
        private static readonly ComboPhase Phase3_StormOfPetals = new ComboPhase(
            curves: new CurveSegment[]
            {
                new CurveSegment(EasingType.SineOut, 0f, -1.15f, 0.18f, 2),
                new CurveSegment(EasingType.PolyIn, 0.2f, -0.97f, 2.2f, 4),
                new CurveSegment(EasingType.PolyOut, 0.82f, 1.23f, 0.05f, 2)
            },
            maxAngle: MathHelper.PiOver2 * 2.3f,
            duration: 40,
            bladeLength: 175f,
            flip: true,
            squish: 0.80f,
            damageMult: 1.5f
        );

        #endregion

        #region Abstract Overrides

        protected override ComboPhase[] GetAllPhases() => new ComboPhase[]
        {
            Phase0_PetalSlash,
            Phase1_CrimsonScatter,
            Phase2_BlossomBloom,
            Phase3_StormOfPetals
        };

        protected override Color[] GetPalette() => SakuraPalette;

        protected override CalamityStyleTrailRenderer.TrailStyle GetTrailStyle()
            => CalamityStyleTrailRenderer.TrailStyle.Flame;

        protected override string GetSmearTexturePath(int comboStep) => comboStep switch
        {
            1 => "MagnumOpus/Assets/Particles/SwordArc3",
            2 => "MagnumOpus/Assets/Particles/CurvedSwordSlash",
            3 => "MagnumOpus/Assets/Particles/FlamingArcSwordSlash",
            _ => "MagnumOpus/Assets/Particles/SwordArc2"
        };

        #endregion

        #region Virtual Overrides

        protected override Texture2D GetBladeTexture()
            => ModContent.Request<Texture2D>("MagnumOpus/Content/Eroica/ResonantWeapons/SakurasBlossom").Value;

        protected override SoundStyle GetSwingSound()
            => SoundID.Item71 with { Pitch = -0.15f + ComboStep * 0.15f, Volume = 0.9f };

        protected override int GetInitialDustType() => DustID.RedTorch;

        protected override int GetSecondaryDustType() => DustID.PinkTorch;

        protected override Vector3 GetLightColor()
        {
            float intensity = 0.55f + ComboStep * 0.12f;
            Color c = Color.Lerp(EroicaPalette.Sakura, EroicaPalette.Gold, Progression);
            return c.ToVector3() * intensity;
        }

        #endregion

        #region Combo Specials — Escalating Spectral Copies

        protected override void HandleComboSpecials()
        {
            if (hasSpawnedSpecial) return;

            // Phase 0 (Petal Slash): 1 spectral copy at 60%
            if (ComboStep == 0 && Progression >= 0.60f)
            {
                hasSpawnedSpecial = true;
                if (Main.myPlayer == Projectile.owner)
                {
                    Vector2 tipPos = GetBladeTipPosition();
                    Vector2 vel = SwordDirection * 15f;
                    Projectile.NewProjectile(
                        Projectile.GetSource_FromThis(), tipPos, vel,
                        ModContent.ProjectileType<SakurasBlossomSpectral>(),
                        (int)(Projectile.damage * 0.7f), 3f, Projectile.owner);
                }

                Vector2 vfxTip = GetBladeTipPosition();
                SakurasBlossomVFX.PetalSlashVFX(vfxTip, SwordDirection);
            }

            // Phase 1 (Crimson Scatter): 2 spectral copies at 55% with spread
            if (ComboStep == 1 && Progression >= 0.55f)
            {
                hasSpawnedSpecial = true;
                if (Main.myPlayer == Projectile.owner)
                {
                    Vector2 tipPos = GetBladeTipPosition();
                    float spread = MathHelper.ToRadians(25f);
                    for (int i = -1; i <= 1; i += 2)
                    {
                        Vector2 vel = SwordDirection.RotatedBy(spread * i) * 14f;
                        Projectile.NewProjectile(
                            Projectile.GetSource_FromThis(), tipPos, vel,
                            ModContent.ProjectileType<SakurasBlossomSpectral>(),
                            (int)(Projectile.damage * 0.7f), 3f, Projectile.owner);
                    }
                }

                Vector2 vfxTip = GetBladeTipPosition();
                SakurasBlossomVFX.CrimsonScatterVFX(vfxTip, SwordDirection);
            }

            // Phase 2 (Blossom Bloom): 3 spectral copies at 65% in fan
            if (ComboStep == 2 && Progression >= 0.65f)
            {
                hasSpawnedSpecial = true;
                if (Main.myPlayer == Projectile.owner)
                {
                    Vector2 tipPos = GetBladeTipPosition();
                    float spread = MathHelper.ToRadians(20f);
                    for (int i = -1; i <= 1; i++)
                    {
                        Vector2 vel = SwordDirection.RotatedBy(spread * i) * 15f;
                        Projectile.NewProjectile(
                            Projectile.GetSource_FromThis(), tipPos, vel,
                            ModContent.ProjectileType<SakurasBlossomSpectral>(),
                            (int)(Projectile.damage * 0.75f), 3f, Projectile.owner);
                    }
                }

                // VFX: Pollen explosion — golden motes scatter from bloom
                Vector2 vfxTip = GetBladeTipPosition();
                SakurasBlossomVFX.BlossomBloomVFX(vfxTip);
            }

            // Phase 3 (Storm of Petals): 4 spectral copies at 55% + spectacular VFX
            if (ComboStep == 3 && Progression >= 0.55f)
            {
                hasSpawnedSpecial = true;
                if (Main.myPlayer == Projectile.owner)
                {
                    Vector2 tipPos = GetBladeTipPosition();
                    float spread = MathHelper.ToRadians(18f);
                    for (int i = 0; i < 4; i++)
                    {
                        float angle = spread * (i - 1.5f);
                        Vector2 vel = SwordDirection.RotatedBy(angle) * 16f;
                        Projectile.NewProjectile(
                            Projectile.GetSource_FromThis(), tipPos, vel,
                            ModContent.ProjectileType<SakurasBlossomSpectral>(),
                            (int)(Projectile.damage * 0.8f), 4f, Projectile.owner);
                    }
                }

                // VFX: Full sakura storm — the climactic blooming
                Vector2 vfxTip = GetBladeTipPosition();
                SakurasBlossomVFX.StormOfPetalsVFX(vfxTip);
            }
        }

        #endregion

        #region On Hit — MusicsDissonance + Seeking Crystals

        protected override void OnSwingHitNPC(NPC target, NPC.HitInfo hit, int remainingDamageCount)
        {
            // Apply MusicsDissonance debuff
            target.AddBuff(ModContent.BuffType<MusicsDissonance>(), 240);

            // Impact VFX — delegated to VFX module
            SakurasBlossomVFX.SwingHitImpact(target.Center, ComboStep);

            // Seeking crystals on every third hit (33% chance)
            if (Main.rand.NextBool(3) && Main.myPlayer == Projectile.owner)
            {
                SeekingCrystalHelper.SpawnEroicaCrystals(
                    Projectile.GetSource_FromThis(),
                    target.Center,
                    (Main.MouseWorld - target.Center).SafeNormalize(Vector2.UnitX) * 8f,
                    (int)(Projectile.damage * 0.2f),
                    Projectile.knockBack * 0.4f,
                    Projectile.owner,
                    4);
            }
        }

        #endregion

        #region Custom VFX — Blooming Sakura Aura

        protected override void DrawCustomVFX(SpriteBatch sb)
        {
            if (Progression <= 0.08f || Progression >= 0.92f) return;

            Vector2 tipPos = GetBladeTipPosition();

            SakurasBlossomVFX.DrawSwingTrailVFX(tipPos, Owner.MountedCenter, SwordDirection, Progression, ComboStep);
        }

        #endregion
    }
}
