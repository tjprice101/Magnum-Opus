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
using MagnumOpus.Common.Systems.VFX.Trails;
using MagnumOpus.Content.Nachtmusik;
using MagnumOpus.Content.Nachtmusik.Projectiles;
using MagnumOpus.Content.Nachtmusik.Debuffs;
using static MagnumOpus.Common.Systems.Particles.Particle;

namespace MagnumOpus.Content.Nachtmusik.ResonantWeapons
{
    /// <summary>
    /// Swing projectile for Twilight Severance — ultra-fast cosmic katana.
    /// 3-phase blazing combo: Dusk Flash → Starlit Edge → Dawn Severance.
    /// Lightning-fast swings with perpendicular slashes on Phase 2.
    /// </summary>
    public sealed class TwilightSeveranceSwing : MeleeSwingBase
    {
        #region Theme Colors

        private static readonly Color DuskViolet = NachtmusikCosmicVFX.DuskViolet;
        private static readonly Color StarGold = NachtmusikCosmicVFX.StarGold;
        private static readonly Color DeepPurple = NachtmusikCosmicVFX.DeepPurple;
        private static readonly Color StarWhite = NachtmusikCosmicVFX.StarWhite;
        private static readonly Color Violet = NachtmusikCosmicVFX.Violet;

        private static readonly Color[] TwilightPalette = new Color[]
        {
            new Color(25, 15, 50),      // [0] Pianissimo — twilight void
            new Color(55, 35, 90),      // [1] Piano — dusk shadow
            new Color(100, 70, 160),    // [2] Mezzo — twilight violet
            new Color(165, 120, 200),   // [3] Forte — bright dusk
            new Color(220, 190, 120),   // [4] Fortissimo — golden dawn
            new Color(255, 248, 230)    // [5] Sforzando — first light
        };

        #endregion

        #region Combo Phases

        // Phase 0: Dusk Flash — ultra-fast horizontal slash
        private static readonly ComboPhase Phase0_DuskFlash = new ComboPhase(
            curves: new CurveSegment[]
            {
                new CurveSegment(EasingType.PolyOut, 0f, -0.7f, 0.15f, 2),
                new CurveSegment(EasingType.PolyIn, 0.15f, -0.55f, 1.3f, 3),
                new CurveSegment(EasingType.PolyOut, 0.78f, 0.75f, 0.18f, 2)
            },
            maxAngle: MathHelper.PiOver2 * 1.3f,
            duration: 16,
            bladeLength: 145f,
            flip: false,
            squish: 0.93f,
            damageMult: 0.8f
        );

        // Phase 1: Starlit Edge — blazing backhand arc
        private static readonly ComboPhase Phase1_StarlitEdge = new ComboPhase(
            curves: new CurveSegment[]
            {
                new CurveSegment(EasingType.PolyOut, 0f, -0.8f, 0.12f, 2),
                new CurveSegment(EasingType.PolyIn, 0.14f, -0.68f, 1.5f, 3),
                new CurveSegment(EasingType.PolyOut, 0.8f, 0.82f, 0.15f, 2)
            },
            maxAngle: MathHelper.PiOver2 * 1.4f,
            duration: 17,
            bladeLength: 148f,
            flip: true,
            squish: 0.90f,
            damageMult: 0.9f
        );

        // Phase 2: Dawn Severance — decisive overhead cut
        private static readonly ComboPhase Phase2_DawnSeverance = new ComboPhase(
            curves: new CurveSegment[]
            {
                new CurveSegment(EasingType.SineOut, 0f, -0.95f, 0.1f, 2),
                new CurveSegment(EasingType.PolyIn, 0.12f, -0.85f, 1.9f, 4),
                new CurveSegment(EasingType.PolyOut, 0.78f, 1.05f, 0.08f, 2)
            },
            maxAngle: MathHelper.PiOver2 * 1.7f,
            duration: 22,
            bladeLength: 155f,
            flip: false,
            squish: 0.85f,
            damageMult: 1.2f
        );

        #endregion

        #region Abstract Overrides

        protected override ComboPhase[] GetAllPhases() => new ComboPhase[]
        {
            Phase0_DuskFlash,
            Phase1_StarlitEdge,
            Phase2_DawnSeverance
        };

        protected override Color[] GetPalette() => TwilightPalette;

        protected override CalamityStyleTrailRenderer.TrailStyle GetTrailStyle()
            => CalamityStyleTrailRenderer.TrailStyle.Cosmic;

        protected override string GetSmearTexturePath(int comboStep) => comboStep switch
        {
            1 => "MagnumOpus/Assets/Particles/SwordArc8",
            2 => "MagnumOpus/Assets/Particles/SimpleArcSwordSlash",
            _ => "MagnumOpus/Assets/Particles/SwordArc1"
        };

        #endregion

        #region Virtual Overrides

        protected override Texture2D GetBladeTexture()
            => ModContent.Request<Texture2D>("MagnumOpus/Content/Nachtmusik/ResonantWeapons/TwilightSeverance").Value;

        protected override SoundStyle GetSwingSound()
            => SoundID.Item71 with { Pitch = 0.4f + ComboStep * 0.1f, Volume = 0.8f };

        protected override int GetInitialDustType() => DustID.PurpleTorch;

        protected override int GetSecondaryDustType() => DustID.GoldFlame;

        protected override Vector3 GetLightColor()
        {
            float intensity = 0.5f + ComboStep * 0.12f;
            Color c = Color.Lerp(DuskViolet, StarGold, Progression);
            return c.ToVector3() * intensity;
        }

        #endregion

        #region Combo Specials

        protected override void HandleComboSpecials()
        {
            if (hasSpawnedSpecial) return;

            // Phase 0: Quick accent at 55%
            if (ComboStep == 0 && Progression >= 0.55f)
            {
                hasSpawnedSpecial = true;
                Vector2 tip = GetBladeTipPosition();
                CustomParticles.GenericFlare(tip, DuskViolet, 0.4f, 10);
                NachtmusikCosmicVFX.SpawnMusicNoteBurst(tip, 2, 12f);
            }

            // Phase 1: Perpendicular slash pair at 50% — signature rapid strikes
            if (ComboStep == 1 && Progression >= 0.50f)
            {
                hasSpawnedSpecial = true;

                if (Main.myPlayer == Projectile.owner)
                {
                    Vector2 tip = GetBladeTipPosition();
                    Vector2 dir = SwordDirection;
                    Vector2 perp1 = dir.RotatedBy(MathHelper.PiOver2) * 13f;
                    Vector2 perp2 = dir.RotatedBy(-MathHelper.PiOver2) * 13f;

                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), tip, perp1,
                        ModContent.ProjectileType<TwilightSlashProjectile>(),
                        (int)(Projectile.damage * 0.35f), 2f, Projectile.owner);
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), tip, perp2,
                        ModContent.ProjectileType<TwilightSlashProjectile>(),
                        (int)(Projectile.damage * 0.35f), 2f, Projectile.owner);
                }

                Vector2 vfxTip = GetBladeTipPosition();
                CustomParticles.GenericFlare(vfxTip, StarWhite, 0.6f, 14);
                CustomParticles.GenericFlare(vfxTip, DuskViolet, 0.45f, 12);
                CustomParticles.HaloRing(vfxTip, Violet, 0.35f, 11);
                NachtmusikCosmicVFX.SpawnMusicNoteBurst(vfxTip, 3, 20f);
            }

            // Phase 2: Dawn Severance finale at 55% — slash fan + seeking crystals
            if (ComboStep == 2 && Progression >= 0.55f)
            {
                hasSpawnedSpecial = true;

                if (Main.myPlayer == Projectile.owner)
                {
                    Vector2 tip = GetBladeTipPosition();
                    Vector2 dir = SwordDirection;

                    // Forward slash
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), tip,
                        dir * 16f,
                        ModContent.ProjectileType<TwilightSlashProjectile>(),
                        (int)(Projectile.damage * 0.45f), 3f, Projectile.owner);

                    // Seeking crystals
                    SeekingCrystalHelper.SpawnNachtmusikCrystals(
                        Projectile.GetSource_FromThis(), tip,
                        dir * 8f, (int)(Projectile.damage * 0.18f),
                        Projectile.knockBack * 0.3f, Projectile.owner, 4);
                }

                Vector2 vfxTip = GetBladeTipPosition();
                NachtmusikCosmicVFX.SpawnCelestialImpact(vfxTip, 1.0f);
                NachtmusikCosmicVFX.SpawnStarBurstImpact(vfxTip, 0.8f, 3);
                for (int i = 0; i < 4; i++)
                {
                    float p = i / 4f;
                    Color rc = Color.Lerp(DuskViolet, StarGold, p);
                    CustomParticles.HaloRing(vfxTip, rc, 0.3f + i * 0.08f, 11 + i * 2);
                }
                NachtmusikCosmicVFX.SpawnMusicNoteBurst(vfxTip, 5, 30f);

                MagnumScreenEffects.AddScreenShake(5f);
            }
        }

        #endregion

        #region On Hit — Celestial Harmony + Twilight Charge

        protected override void OnSwingHitNPC(NPC target, NPC.HitInfo hit, int remainingDamageCount)
        {
            target.AddBuff(ModContent.BuffType<CelestialHarmony>(), 360);

            int stacksToAdd = hit.Crit ? 3 : 1;
            if (target.TryGetGlobalNPC(out CelestialHarmonyNPC harmonyNPC))
                harmonyNPC.AddStack(target, stacksToAdd);

            // Build twilight charge on the owning item
            if (Main.myPlayer == Projectile.owner)
            {
                Player player = Main.player[Projectile.owner];
                if (player.HeldItem?.ModItem is TwilightSeverance ts)
                    ts.TwilightCharge = Math.Min(ts.TwilightCharge + (hit.Crit ? 12 : 5), 100);
            }

            float impactScale = 0.6f + ComboStep * 0.15f;
            NachtmusikCosmicVFX.SpawnCelestialImpact(target.Center, impactScale);

            int dustCount = 5 + ComboStep * 2;
            for (int i = 0; i < dustCount; i++)
            {
                float angle = MathHelper.TwoPi * i / dustCount;
                float dp = (float)i / dustCount;
                Color dc = Color.Lerp(DuskViolet, StarGold, dp);
                int dustType = i % 2 == 0 ? DustID.PurpleTorch : DustID.GoldFlame;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 6f);
                Dust d = Dust.NewDustPerfect(target.Center, dustType, vel, 0, dc, 1.2f);
                d.noGravity = true;
            }

            NachtmusikCosmicVFX.SpawnMusicNoteBurst(target.Center, 2 + ComboStep, 20f);

            if (hit.Crit)
                NachtmusikCosmicVFX.SpawnStarBurstImpact(target.Center, 0.6f, 2);

            Lighting.AddLight(target.Center, DuskViolet.ToVector3() * 0.6f);
        }

        #endregion

        #region Custom VFX

        protected override void DrawCustomVFX(SpriteBatch sb)
        {
            if (Progression < 0.08f || Progression > 0.92f) return;

            Vector2 tipPos = GetBladeTipPosition();

            // Dense twilight dust trail — dusk violet to golden dawn
            for (int i = 0; i < 2; i++)
            {
                float dp = Main.rand.NextFloat();
                Color dc = Color.Lerp(DuskViolet, StarGold, dp);
                int dustType = dp < 0.5f ? DustID.PurpleTorch : DustID.GoldFlame;
                Vector2 dustPos = Vector2.Lerp(Owner.MountedCenter, tipPos, Main.rand.NextFloat(0.5f, 1f));
                Dust d = Dust.NewDustPerfect(dustPos, dustType,
                    -SwordDirection * Main.rand.NextFloat(1f, 3f) + Main.rand.NextVector2Circular(0.8f, 0.8f),
                    0, dc, 1.4f);
                d.noGravity = true;
                d.fadeIn = 1.1f;
            }

            // Quick star sparkle accents — fast katana needs sharp sparkles
            if (Main.rand.NextBool(2))
            {
                Vector2 sparklePos = tipPos + Main.rand.NextVector2Circular(6f, 6f);
                Dust star = Dust.NewDustPerfect(sparklePos, DustID.GoldFlame,
                    Main.rand.NextVector2Circular(2f, 2f), 0, StarWhite, 0.9f);
                star.noGravity = true;
            }

            // Twilight shimmer — hue oscillation between violet and gold
            if (Main.rand.NextBool(3))
            {
                bool isViolet = Main.rand.NextBool();
                float hue = isViolet ? (0.73f + Main.GameUpdateCount * 0.02f % 0.1f) : (0.12f + Main.GameUpdateCount * 0.02f % 0.06f);
                Color shimmer = Main.hslToRgb(hue, 0.85f, 0.75f);
                Dust s = Dust.NewDustPerfect(tipPos, DustID.PurpleTorch,
                    Main.rand.NextVector2Circular(1.5f, 1.5f), 0, shimmer, 1.0f);
                s.noGravity = true;
            }

            // Music notes from blade tip — faster frequency for katana rhythm
            if (Main.rand.NextBool(4))
            {
                float shimmerScale = 1f + MathF.Sin(Main.GameUpdateCount * 0.18f) * 0.1f;
                MagnumParticleHandler.SpawnParticle(new HueShiftingMusicNoteParticle(
                    tipPos,
                    -SwordDirection * 1.2f + new Vector2(0, -0.3f),
                    hueMin: 0.10f, hueMax: 0.78f,
                    saturation: 0.85f, luminosity: 0.65f,
                    scale: 0.75f * shimmerScale,
                    lifetime: 22,
                    hueSpeed: 0.025f
                ));
            }

            // Blade-tip bloom glow — twilight cosmic radiance
            float bloomOpacity = MathHelper.Clamp((Progression - 0.08f) / 0.10f, 0f, 1f)
                               * MathHelper.Clamp((0.92f - Progression) / 0.10f, 0f, 1f);
            if (bloomOpacity > 0f)
            {
                BloomRenderer.DrawBloomStackAdditive(
                    tipPos,
                    NachtmusikCosmicVFX.DuskViolet,
                    NachtmusikCosmicVFX.StarGold,
                    scale: 0.45f + ComboStep * 0.08f,
                    opacity: bloomOpacity);
            }
        }

        #endregion
    }
}
