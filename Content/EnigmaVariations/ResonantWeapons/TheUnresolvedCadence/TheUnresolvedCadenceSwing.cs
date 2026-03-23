using System;
using MagnumOpus.Common.BaseClasses;
using MagnumOpus.Content.EnigmaVariations.ResonantWeapons.TheUnresolvedCadence.Utilities;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.EnigmaVariations.ResonantWeapons.TheUnresolvedCadence
{
    /// <summary>
    /// The Unresolved Cadence — Enigma Variations theme melee. Exoblade-architecture swing.
    /// Glitchy green/purple trail with dimensional instability sparks.
    /// 3-phase combo: 0=The Question (1 slash), 1=The Doubt (2 flanking slashes), 2=The Silence (3 wide slashes).
    /// Combo advances automatically on each swing (including hold).
    /// </summary>
    public class TheUnresolvedCadenceSwing : ExobladeStyleSwing
    {
        protected override bool SupportsDash => false;

        protected override float BladeLength => 104f;
        protected override int BaseSwingFrames => 76;
        protected override Color SlashPrimaryColor => new Color(80, 230, 120);
        protected override Color SlashSecondaryColor => new Color(100, 30, 160);
        protected override Color SlashAccentColor => new Color(220, 160, 255);

        public override string Texture => "MagnumOpus/Content/EnigmaVariations/ResonantWeapons/TheUnresolvedCadence/TheUnresolvedCadence";

        protected override Color GetLensFlareColor(float p)
            => Color.Lerp(new Color(100, 200, 130), new Color(200, 140, 255), (float)Math.Pow(p, 2));

        protected override Color GetSwingDustColor()
        {
            float t = Main.rand.NextFloat();
            return t < 0.5f
                ? Color.Lerp(new Color(60, 220, 100), new Color(140, 255, 180), Main.rand.NextFloat())
                : Color.Lerp(new Color(160, 80, 220), new Color(200, 140, 255), Main.rand.NextFloat());
        }

        protected override void OnSwingStart(bool isFirstSwing)
        {
            if (Main.myPlayer != Projectile.owner) return;

            Player player = Owner;
            CadencePlayer cadence = player.Cadence();
            int phase = cadence.ComboPhase % 3;
            cadence.ComboPhase++;

            // Ramp cadence intensity with each swing
            cadence.CadenceIntensity = MathHelper.Clamp(cadence.CadenceIntensity + 0.2f, 0f, 1f);

            int damage = Projectile.damage;
            float knockback = Projectile.knockBack;
            Vector2 aimDir = (Main.MouseWorld - player.MountedCenter).SafeNormalize(Vector2.UnitX);
            IEntitySource source = Projectile.GetSource_FromThis();

            if (phase == 0)
            {
                // Phase 0 — The Question: Single dimensional slash forward
                int slashDmg = (int)(damage * 0.35f);
                Vector2 slashVel = aimDir * 12f;
                Projectile.NewProjectile(source, player.MountedCenter, slashVel,
                    ModContent.ProjectileType<DimensionalSlash>(), slashDmg, knockback * 0.5f, player.whoAmI);
            }
            else if (phase == 1)
            {
                // Phase 1 — The Doubt: Two flanking dimensional slashes
                int slashDmg = (int)(damage * 0.3f);
                for (int i = -1; i <= 1; i += 2)
                {
                    Vector2 slashVel = aimDir.RotatedBy(MathHelper.ToRadians(20 * i)) * 11f;
                    Projectile.NewProjectile(source, player.MountedCenter, slashVel,
                        ModContent.ProjectileType<DimensionalSlash>(), slashDmg, knockback * 0.4f, player.whoAmI);
                }
            }
            else // phase == 2
            {
                // Phase 2 — The Silence: Three wide dimensional slashes
                int slashDmg = (int)(damage * 0.25f);
                for (int i = -1; i <= 1; i++)
                {
                    Vector2 slashVel = aimDir.RotatedBy(MathHelper.ToRadians(25 * i)) * 13f;
                    Projectile.NewProjectile(source, player.MountedCenter, slashVel,
                        ModContent.ProjectileType<DimensionalSlash>(), slashDmg, knockback * 0.3f, player.whoAmI);
                }
            }
        }

        protected override void OnSwingFrame()
        {
            // Unstable dimensional sparks — alternating green and purple
            if (Progression > 0.3f && Progression < 0.85f && Main.rand.NextFloat() < 0.35f)
            {
                Vector2 pos = Owner.MountedCenter + SwordDirection * BladeLength * Projectile.scale * Main.rand.NextFloat(0.3f, 1f);
                Vector2 vel = SwordDirection.RotatedByRandom(0.8f) * Main.rand.NextFloat(1f, 3.5f);
                int dustType = Main.rand.NextBool() ? DustID.CursedTorch : DustID.ShadowbeamStaff;
                Dust spark = Dust.NewDustPerfect(pos, dustType, vel, 60, default, Main.rand.NextFloat(0.5f, 0.9f));
                spark.noGravity = true;
                spark.fadeIn = 0.6f;
            }

            // Glitchy afterimage-like dust at random positions along blade
            if (Progression > 0.35f && Main.rand.NextFloat() < 0.1f)
            {
                float offset = Main.rand.NextFloat(-0.2f, 0.2f);
                Vector2 pos = Owner.MountedCenter + SwordDirection * BladeLength * Projectile.scale * 0.8f;
                pos += SwordDirection.RotatedBy(MathHelper.PiOver2) * BladeLength * offset;
                Dust glitch = Dust.NewDustPerfect(pos, DustID.PurpleTorch,
                    Vector2.Zero, 80, default, 0.4f);
                glitch.noGravity = true;
            }
        }

        protected override void OnSwingHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.CursedInferno, 180);

            for (int i = 0; i < 8; i++)
            {
                int dustType = i % 2 == 0 ? DustID.CursedTorch : DustID.ShadowbeamStaff;
                Dust burst = Dust.NewDustPerfect(target.Center, dustType,
                    Main.rand.NextVector2CircularEdge(5f, 5f), 40, default, Main.rand.NextFloat(0.8f, 1.2f));
                burst.noGravity = true;
            }
        }

        protected override void OnDashHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.CursedInferno, 300);
            target.AddBuff(BuffID.ShadowFlame, 300);

            for (int i = 0; i < 14; i++)
            {
                int dustType = Main.rand.NextBool() ? DustID.CursedTorch : DustID.ShadowbeamStaff;
                Dust burst = Dust.NewDustPerfect(target.Center, dustType,
                    Main.rand.NextVector2CircularEdge(8f, 8f), 20, default, Main.rand.NextFloat(1.2f, 1.8f));
                burst.noGravity = true;
            }
        }
    }
}
