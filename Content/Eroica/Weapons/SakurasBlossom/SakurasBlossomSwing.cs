using System;
using MagnumOpus.Common.BaseClasses;
using MagnumOpus.Content.Eroica.Weapons.SakurasBlossom.Projectiles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Eroica.Weapons.SakurasBlossom
{
    /// <summary>
    /// Sakura's Blossom — Eroica theme melee. Exoblade-architecture swing.
    /// Sakura pink trail with scattered cherry blossom petals and warm gold highlights.
    /// 3-phase Petal Dance combo that advances each swing (including hold re-swings).
    /// </summary>
    public class SakurasBlossomSwing : ExobladeStyleSwing
    {
        /// <summary>Petal Dance combo phase (0-2). Advances each swing including hold re-swings.</summary>
        private int comboPhase = 0;

        protected override bool SupportsDash => false;
        protected override float BladeLength => 100f;
        protected override int BaseSwingFrames => 78;
        protected override float TextureDrawScale => 0.12f;
        protected override Color SlashPrimaryColor => new Color(255, 120, 140);
        protected override Color SlashSecondaryColor => new Color(120, 20, 40);
        protected override Color SlashAccentColor => new Color(255, 200, 150);

        public override string Texture => "MagnumOpus/Content/Eroica/Weapons/SakurasBlossom/SakurasBlossom";

        protected override Color GetLensFlareColor(float p)
            => Color.Lerp(new Color(255, 150, 180), new Color(255, 220, 160), (float)Math.Pow(p, 2));

        protected override Color GetSwingDustColor()
        {
            float t = Main.rand.NextFloat();
            return t < 0.5f
                ? Color.Lerp(new Color(255, 180, 200), new Color(255, 220, 230), Main.rand.NextFloat())
                : Color.Lerp(new Color(255, 140, 160), new Color(255, 200, 150), Main.rand.NextFloat());
        }

        protected override void OnSwingStart(bool isFirstSwing)
        {
            if (Main.myPlayer != Projectile.owner) return;

            Player player = Owner;
            int damage = Projectile.damage;
            float knockback = Projectile.knockBack;
            Vector2 aimDir = (Main.MouseWorld - player.MountedCenter).SafeNormalize(Vector2.UnitX);
            IEntitySource source = Projectile.GetSource_FromThis();

            int phase = comboPhase % 3;
            comboPhase++;
            int petalType = ModContent.ProjectileType<SakuraPetalProj>();
            int petalDmg = (int)(damage * 0.3f);

            if (phase == 0)
            {
                // 3 petals in narrow fan
                for (int i = -1; i <= 1; i++)
                {
                    Vector2 petalVel = aimDir.RotatedBy(MathHelper.ToRadians(20 * i)) * Main.rand.NextFloat(6f, 9f);
                    petalVel += Main.rand.NextVector2Circular(1f, 1f);
                    Projectile.NewProjectile(source, player.MountedCenter, petalVel,
                        petalType, petalDmg, knockback * 0.3f, player.whoAmI,
                        0f, Main.rand.Next(4));
                }
            }
            else if (phase == 1)
            {
                // 4 petals in wider fan + 1 homing petal
                for (int i = 0; i < 4; i++)
                {
                    float angle = MathHelper.ToRadians(-30 + 20 * i);
                    Vector2 petalVel = aimDir.RotatedBy(angle) * Main.rand.NextFloat(7f, 10f);
                    Projectile.NewProjectile(source, player.MountedCenter, petalVel,
                        petalType, petalDmg, knockback * 0.3f, player.whoAmI,
                        0f, Main.rand.Next(4));
                }
                Projectile.NewProjectile(source, player.MountedCenter, aimDir * 11f,
                    petalType, (int)(damage * 0.45f), knockback * 0.5f, player.whoAmI,
                    1f, Main.rand.Next(4));
            }
            else
            {
                // Final Bloom: 8 petals in full 360-degree burst
                for (int i = 0; i < 8; i++)
                {
                    float angle = MathHelper.TwoPi / 8f * i;
                    Vector2 petalVel = aimDir.RotatedBy(angle) * Main.rand.NextFloat(5f, 8f);
                    Projectile.NewProjectile(source, player.MountedCenter, petalVel,
                        petalType, (int)(damage * 0.4f), knockback * 0.5f, player.whoAmI,
                        1f, (float)(i % 4));
                }
            }
        }

        protected override void OnSwingFrame()
        {
            if (Progression > 0.3f && Progression < 0.85f && Main.rand.NextFloat() < 0.35f)
            {
                Vector2 pos = Owner.MountedCenter + SwordDirection * BladeLength * Projectile.scale * Main.rand.NextFloat(0.4f, 1f);
                Vector2 vel = SwordDirection.RotatedBy(MathHelper.PiOver2 * Direction * Main.rand.NextFloat(0.3f, 1.2f)) * Main.rand.NextFloat(1f, 3f);
                vel.Y -= Main.rand.NextFloat(0.5f, 1.5f);
                Dust petal = Dust.NewDustPerfect(pos, DustID.PinkFairy, vel, 80, default, Main.rand.NextFloat(0.6f, 1f));
                petal.noGravity = true;
                petal.fadeIn = 0.8f;
            }
            if (Progression > 0.4f && Main.rand.NextFloat() < 0.15f)
            {
                Vector2 tip = Owner.MountedCenter + SwordDirection * BladeLength * Projectile.scale;
                Dust spark = Dust.NewDustPerfect(tip, DustID.GoldFlame, Main.rand.NextVector2Circular(1f, 1f), 0, default, 0.5f);
                spark.noGravity = true;
            }
        }

        protected override void OnSwingHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            for (int i = 0; i < 6; i++)
            {
                Dust petal = Dust.NewDustPerfect(target.Center, DustID.PinkFairy,
                    Main.rand.NextVector2CircularEdge(4f, 4f), 60, default, Main.rand.NextFloat(0.8f, 1.2f));
                petal.noGravity = true;
                petal.fadeIn = 1f;
            }
            for (int i = 0; i < 3; i++)
            {
                Vector2 vel = (target.Center - Owner.Center).SafeNormalize(Vector2.UnitX).RotatedByRandom(0.5f) * Main.rand.NextFloat(3f, 6f);
                Dust spark = Dust.NewDustPerfect(target.Center, DustID.GoldFlame, vel, 0, default, 0.6f);
                spark.noGravity = true;
            }
        }

        protected override void OnDashHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            for (int i = 0; i < 12; i++)
            {
                Dust petal = Dust.NewDustPerfect(target.Center, DustID.PinkFairy,
                    Main.rand.NextVector2CircularEdge(6f, 6f), 40, default, Main.rand.NextFloat(1f, 1.5f));
                petal.noGravity = true;
            }
        }
    }
}
