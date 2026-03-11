using System;
using MagnumOpus.Common.BaseClasses;
using MagnumOpus.Content.Eroica.Weapons.CelestialValor.Projectiles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Eroica.Weapons.CelestialValor
{
    /// <summary>
    /// Celestial Valor — Eroica theme melee. Exoblade-architecture swing.
    /// Scarlet & crimson trail with heroic gold ember highlights.
    /// Combo phase tracking lives here so it advances on hold re-swings.
    /// </summary>
    public class CelestialValorSwing : ExobladeStyleSwing
    {
        /// <summary>Heroic Crescendo combo phase (0-3). Advances each swing including hold re-swings.</summary>
        private int comboPhase = 0;

        protected override float BladeLength => 110f;
        protected override int BaseSwingFrames => 78;
        protected override float TextureDrawScale => 0.116f;
        protected override Color SlashPrimaryColor => new Color(255, 60, 60);
        protected override Color SlashSecondaryColor => new Color(100, 10, 20);
        protected override Color SlashAccentColor => new Color(255, 220, 100);

        public override string Texture => "MagnumOpus/Content/Eroica/Weapons/CelestialValor/CelestialValor";

        protected override void OnSwingStart(bool isFirstSwing)
        {
            if (Main.myPlayer != Projectile.owner) return;

            Player player = Owner;
            int damage = Projectile.damage;
            float knockback = Projectile.knockBack;
            Vector2 aimDir = (Main.MouseWorld - player.MountedCenter).SafeNormalize(Vector2.UnitX);
            IEntitySource source = Projectile.GetSource_FromThis();

            int phase = comboPhase % 4;
            comboPhase++;

            if (phase == 0 || phase == 1)
            {
                // Phases 0-1: Fire 2 homing valor beams flanking the swing
                int beamDmg = (int)(damage * 0.35f);
                for (int i = -1; i <= 1; i += 2)
                {
                    Vector2 beamVel = aimDir.RotatedBy(MathHelper.ToRadians(18 * i)) * 10f;
                    Projectile.NewProjectile(source, player.MountedCenter, beamVel,
                        ModContent.ProjectileType<ValorBeam>(), beamDmg, knockback * 0.5f, player.whoAmI);
                }
            }
            else if (phase == 2)
            {
                // Phase 2: Wide valor slash arc + flanking beams
                Vector2 slashVel = aimDir * 14f;
                Projectile.NewProjectile(source, player.MountedCenter, slashVel,
                    ModContent.ProjectileType<ValorSlash>(), (int)(damage * 0.55f), knockback, player.whoAmI);

                for (int i = -1; i <= 1; i += 2)
                {
                    Vector2 beamVel = aimDir.RotatedBy(MathHelper.ToRadians(30 * i)) * 8f;
                    Projectile.NewProjectile(source, player.MountedCenter, beamVel,
                        ModContent.ProjectileType<ValorBeam>(), (int)(damage * 0.25f), knockback * 0.3f, player.whoAmI);
                }
            }
            else // phase == 3
            {
                // Finale Fortissimo: Massive ValorBoom at cursor + 4 beams in cardinal spread
                Projectile.NewProjectile(source, Main.MouseWorld, Vector2.Zero,
                    ModContent.ProjectileType<ValorBoom>(), (int)(damage * 0.75f), knockback * 2f, player.whoAmI);

                for (int i = 0; i < 4; i++)
                {
                    Vector2 beamVel = aimDir.RotatedBy(MathHelper.PiOver2 * i) * 11f;
                    Projectile.NewProjectile(source, player.MountedCenter, beamVel,
                        ModContent.ProjectileType<ValorBeam>(), (int)(damage * 0.3f), knockback * 0.5f, player.whoAmI);
                }
            }
        }

        protected override Color GetLensFlareColor(float p)
            => Color.Lerp(new Color(255, 80, 60), new Color(255, 200, 80), (float)Math.Pow(p, 2));

        protected override Color GetSwingDustColor()
        {
            float t = Main.rand.NextFloat();
            return t < 0.4f
                ? Color.Lerp(new Color(255, 50, 30), new Color(200, 20, 20), Main.rand.NextFloat())
                : Color.Lerp(new Color(255, 180, 50), new Color(255, 80, 40), Main.rand.NextFloat());
        }

        protected override void OnSwingFrame()
        {
            if (Progression > 0.3f && Progression < 0.85f && Main.rand.NextFloat() < 0.4f)
            {
                Vector2 pos = Owner.MountedCenter + SwordDirection * BladeLength * Projectile.scale * Main.rand.NextFloat(0.3f, 1f);
                Vector2 vel = SwordDirection.RotatedByRandom(0.6f) * Main.rand.NextFloat(1.5f, 3.5f);
                vel.Y -= Main.rand.NextFloat(0.5f, 2f);
                Dust ember = Dust.NewDustPerfect(pos, DustID.Torch, vel, 0, default, Main.rand.NextFloat(0.8f, 1.3f));
                ember.noGravity = true;
                ember.fadeIn = 0.6f;
            }
            if (Progression > 0.5f && Main.rand.NextFloat() < 0.2f)
            {
                Vector2 tip = Owner.MountedCenter + SwordDirection * BladeLength * Projectile.scale;
                Dust gold = Dust.NewDustPerfect(tip, DustID.GoldFlame, Main.rand.NextVector2Circular(2f, 2f), 0, default, 0.7f);
                gold.noGravity = true;
            }
        }

        protected override void OnSwingHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.OnFire3, 180);

            for (int i = 0; i < 8; i++)
            {
                Dust fire = Dust.NewDustPerfect(target.Center, DustID.Torch,
                    Main.rand.NextVector2CircularEdge(5f, 5f), 0, default, Main.rand.NextFloat(1f, 1.4f));
                fire.noGravity = true;
            }
            for (int i = 0; i < 4; i++)
            {
                Dust gold = Dust.NewDustPerfect(target.Center, DustID.GoldFlame,
                    Main.rand.NextVector2Circular(3f, 3f), 0, default, 0.8f);
                gold.noGravity = true;
            }
        }

        protected override void OnDashHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.OnFire3, 300);

            for (int i = 0; i < 16; i++)
            {
                Dust fire = Dust.NewDustPerfect(target.Center, DustID.Torch,
                    Main.rand.NextVector2CircularEdge(8f, 8f), 0, default, Main.rand.NextFloat(1.2f, 1.8f));
                fire.noGravity = true;
            }
        }
    }
}
