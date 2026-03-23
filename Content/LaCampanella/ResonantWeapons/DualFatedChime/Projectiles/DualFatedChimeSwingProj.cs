using System;
using MagnumOpus.Common.BaseClasses;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.LaCampanella.ResonantWeapons.DualFatedChime.Projectiles
{
    /// <summary>
    /// Dual Fated Chime — La Campanella theme melee. Exoblade-architecture swing.
    /// Infernal orange/gold trail with black smoke edges and bell-fire highlights.
    /// 5-phase Inferno Waltz toll combo that advances each swing (including hold re-swings).
    /// </summary>
    public class DualFatedChimeSwingProj : ExobladeStyleSwing
    {
        protected override bool SupportsDash => false;

        /// <summary>Inferno Waltz toll phase (0-4). Advances each swing including hold re-swings.</summary>
        private int tollPhase = 0;

        protected override float BladeLength => 105f;
        protected override int BaseSwingFrames => 78;
        protected override Color SlashPrimaryColor => new Color(255, 140, 40);
        protected override Color SlashSecondaryColor => new Color(80, 20, 10);
        protected override Color SlashAccentColor => new Color(255, 240, 150);

        public override string Texture => "MagnumOpus/Content/LaCampanella/ResonantWeapons/DualFatedChime/DualFatedChime";

        protected override Color GetLensFlareColor(float p)
            => Color.Lerp(new Color(255, 160, 40), new Color(255, 240, 140), (float)Math.Pow(p, 2));

        protected override Color GetSwingDustColor()
        {
            float t = Main.rand.NextFloat();
            return t < 0.5f
                ? Color.Lerp(new Color(255, 120, 20), new Color(255, 200, 60), Main.rand.NextFloat())
                : Color.Lerp(new Color(180, 80, 10), new Color(255, 160, 40), Main.rand.NextFloat());
        }

        protected override void OnSwingStart(bool isFirstSwing)
        {
            if (Main.myPlayer != Projectile.owner) return;

            Player player = Owner;
            int damage = Projectile.damage;
            float knockback = Projectile.knockBack;
            Vector2 aimDir = (Main.MouseWorld - player.MountedCenter).SafeNormalize(Vector2.UnitX);
            IEntitySource source = Projectile.GetSource_FromThis();

            int toll = tollPhase % 5;
            tollPhase++;
            int flameType = ModContent.ProjectileType<BellFlameWaveProj>();
            int flameDmg = (int)(damage * 0.35f);

            switch (toll)
            {
                case 0: // Opening Peal — single forward flame wave
                    Projectile.NewProjectile(source, player.MountedCenter, aimDir * 12f,
                        flameType, flameDmg, knockback * 0.5f, player.whoAmI);
                    break;

                case 1: // Answer — 2 waves in narrow spread
                    for (int i = -1; i <= 1; i += 2)
                    {
                        Vector2 flameVel = aimDir.RotatedBy(MathHelper.ToRadians(10 * i)) * 13f;
                        Projectile.NewProjectile(source, player.MountedCenter, flameVel,
                            flameType, flameDmg, knockback * 0.5f, player.whoAmI);
                    }
                    break;

                case 2: // Escalation — 3 waves in wide fan
                    for (int i = -1; i <= 1; i++)
                    {
                        Vector2 flameVel = aimDir.RotatedBy(MathHelper.ToRadians(25 * i)) * 11f;
                        Projectile.NewProjectile(source, player.MountedCenter, flameVel,
                            flameType, (int)(damage * 0.4f), knockback * 0.6f, player.whoAmI);
                    }
                    break;

                case 3: // Resonance — 2 forward + 2 ground-level flanking waves
                    for (int i = -1; i <= 1; i += 2)
                    {
                        Vector2 flameVel = aimDir.RotatedBy(MathHelper.ToRadians(8 * i)) * 14f;
                        Projectile.NewProjectile(source, player.MountedCenter, flameVel,
                            flameType, flameDmg, knockback * 0.5f, player.whoAmI);
                    }
                    for (int i = -1; i <= 1; i += 2)
                    {
                        Vector2 groundVel = new Vector2(i * 10f, 0.5f);
                        Projectile.NewProjectile(source, player.MountedCenter + new Vector2(0, 20),
                            groundVel, flameType, flameDmg, knockback * 0.3f, player.whoAmI);
                    }
                    break;

                case 4: // Grand Toll — 8 directional bell flame waves burst
                    for (int i = 0; i < 8; i++)
                    {
                        float angle = MathHelper.TwoPi / 8f * i;
                        Vector2 flameVel = angle.ToRotationVector2() * 10f;
                        Projectile.NewProjectile(source, player.MountedCenter, flameVel,
                            flameType, (int)(damage * 0.5f), knockback, player.whoAmI);
                    }
                    break;
            }
        }

        protected override void OnSwingFrame()
        {
            // Crackling flame embers along the blade
            if (Progression > 0.25f && Progression < 0.85f && Main.rand.NextFloat() < 0.4f)
            {
                Vector2 pos = Owner.MountedCenter + SwordDirection * BladeLength * Projectile.scale * Main.rand.NextFloat(0.3f, 1f);
                Vector2 vel = SwordDirection.RotatedByRandom(0.8f) * Main.rand.NextFloat(1f, 3f);
                vel.Y -= Main.rand.NextFloat(0.5f, 2f);
                Dust flame = Dust.NewDustPerfect(pos, DustID.Torch, vel, 0, default, Main.rand.NextFloat(0.9f, 1.4f));
                flame.noGravity = true;
            }

            // Black smoke from swing edges
            if (Progression > 0.35f && Main.rand.NextFloat() < 0.2f)
            {
                Vector2 pos = Owner.MountedCenter + SwordDirection * BladeLength * Projectile.scale * Main.rand.NextFloat(0.5f, 0.9f);
                Dust smoke = Dust.NewDustPerfect(pos, DustID.Smoke,
                    SwordDirection.RotatedByRandom(1f) * 0.5f, 120, Color.Black, 0.7f);
                smoke.noGravity = true;
            }
        }

        protected override void OnSwingHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.OnFire, 240);

            for (int i = 0; i < 8; i++)
            {
                Dust fire = Dust.NewDustPerfect(target.Center, DustID.Torch,
                    Main.rand.NextVector2CircularEdge(5f, 5f), 0, default, Main.rand.NextFloat(1f, 1.5f));
                fire.noGravity = true;
            }
            for (int i = 0; i < 3; i++)
            {
                Dust smoke = Dust.NewDustPerfect(target.Center, DustID.Smoke,
                    Main.rand.NextVector2Circular(3f, 3f), 100, Color.Black, 0.8f);
                smoke.noGravity = true;
            }
        }

        protected override void OnDashHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.OnFire3, 300);

            for (int i = 0; i < 14; i++)
            {
                Dust fire = Dust.NewDustPerfect(target.Center, DustID.Torch,
                    Main.rand.NextVector2CircularEdge(7f, 7f), 0, default, Main.rand.NextFloat(1.2f, 1.8f));
                fire.noGravity = true;
            }
        }
    }
}
