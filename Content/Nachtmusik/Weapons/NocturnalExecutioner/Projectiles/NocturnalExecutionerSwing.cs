using System;
using MagnumOpus.Common.BaseClasses;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Nachtmusik.Weapons.NocturnalExecutioner.Projectiles
{
    /// <summary>
    /// Nocturnal Executioner — Nachtmusik theme melee. Exoblade-architecture swing.
    /// Deep indigo trail with starlight silver highlights and void-edged accents.
    /// Combo phase tracking lives here so it advances on hold re-swings.
    /// </summary>
    public class NocturnalExecutionerSwing : ExobladeStyleSwing
    {
        protected override bool SupportsDash => false;

        protected override float BladeLength => 115f;
        protected override int BaseSwingFrames => 82;
        protected override float TextureDrawScale => 0.81f;
        protected override string GradientLUTPath => "MagnumOpus/Assets/VFX Asset Library/ColorGradients/NachtmusikGradientLUTandRAMP";
        protected override Color SlashPrimaryColor => new Color(100, 120, 220);
        protected override Color SlashSecondaryColor => new Color(20, 15, 50);
        protected override Color SlashAccentColor => new Color(180, 200, 255);

        public override string Texture => "MagnumOpus/Content/Nachtmusik/Weapons/NocturnalExecutioner/NocturnalExecutioner";

        protected override void OnSwingStart(bool isFirstSwing)
        {
            if (Main.myPlayer != Projectile.owner) return;

            Player player = Owner;
            int damage = Projectile.damage;
            float knockback = Projectile.knockBack;
            Vector2 aimDir = (Main.MouseWorld - player.MountedCenter).SafeNormalize(Vector2.UnitX);
            IEntitySource source = Projectile.GetSource_FromThis();

            if (Main.dayTime)
            {
                // Daytime: 5 orbs in spread, gentle homing (default 0.04), short timeLeft (60f)
                int bladeDmg = (int)(damage * 0.3f);
                for (int i = 0; i < 5; i++)
                {
                    float spread = MathHelper.ToRadians(-20 + 10 * i);
                    Vector2 bladeVel = aimDir.RotatedBy(spread) * 12f;
                    int idx = Projectile.NewProjectile(source, player.MountedCenter, bladeVel,
                        ModContent.ProjectileType<NocturnalBladeProjectile>(),
                        bladeDmg, knockback * 0.4f, player.whoAmI, 0f, 0f);
                    if (idx >= 0 && idx < Main.maxProjectiles)
                        Main.projectile[idx].timeLeft = 60;
                }
            }
            else
            {
                // Nighttime: 1 orb, aggressive homing (0.14), long timeLeft (240f), night mode flag
                int bladeDmg = (int)(damage * 0.5f);
                int idx = Projectile.NewProjectile(source, player.MountedCenter, aimDir * 14f,
                    ModContent.ProjectileType<NocturnalBladeProjectile>(),
                    bladeDmg, knockback * 0.6f, player.whoAmI, 0.14f, 1f);
                if (idx >= 0 && idx < Main.maxProjectiles)
                    Main.projectile[idx].timeLeft = 240;
            }
        }

        protected override Color GetLensFlareColor(float p)
            => Color.Lerp(new Color(80, 100, 200), new Color(200, 220, 255), (float)Math.Pow(p, 2));

        protected override Color GetSwingDustColor()
        {
            float t = Main.rand.NextFloat();
            return t < 0.4f
                ? Color.Lerp(new Color(20, 15, 50), new Color(100, 120, 220), Main.rand.NextFloat())
                : Color.Lerp(new Color(80, 100, 200), new Color(180, 200, 255), Main.rand.NextFloat());
        }

        protected override void OnSwingFrame()
        {
            if (Progression > 0.3f && Progression < 0.85f && Main.rand.NextFloat() < 0.4f)
            {
                Vector2 pos = Owner.MountedCenter + SwordDirection * BladeLength * Projectile.scale * Main.rand.NextFloat(0.3f, 1f);
                Vector2 vel = SwordDirection.RotatedByRandom(0.6f) * Main.rand.NextFloat(1.5f, 3.5f);
                vel.Y -= Main.rand.NextFloat(0.5f, 2f);
                Dust ember = Dust.NewDustPerfect(pos, DustID.BlueTorch, vel, 0, default, Main.rand.NextFloat(0.8f, 1.3f));
                ember.noGravity = true;
                ember.fadeIn = 0.6f;
            }
            if (Progression > 0.5f && Main.rand.NextFloat() < 0.2f)
            {
                Vector2 tip = Owner.MountedCenter + SwordDirection * BladeLength * Projectile.scale;
                Dust star = Dust.NewDustPerfect(tip, DustID.WhiteTorch, Main.rand.NextVector2Circular(2f, 2f), 0, default, 0.7f);
                star.noGravity = true;
            }
        }

        protected override void OnSwingHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Frostburn, 180);

            for (int i = 0; i < 8; i++)
            {
                Dust frost = Dust.NewDustPerfect(target.Center, DustID.BlueTorch,
                    Main.rand.NextVector2CircularEdge(5f, 5f), 0, default, Main.rand.NextFloat(1f, 1.4f));
                frost.noGravity = true;
            }
            for (int i = 0; i < 4; i++)
            {
                Dust star = Dust.NewDustPerfect(target.Center, DustID.WhiteTorch,
                    Main.rand.NextVector2Circular(3f, 3f), 0, default, 0.8f);
                star.noGravity = true;
            }
        }

        protected override void OnDashHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Frostburn, 300);

            for (int i = 0; i < 16; i++)
            {
                Dust frost = Dust.NewDustPerfect(target.Center, DustID.BlueTorch,
                    Main.rand.NextVector2CircularEdge(8f, 8f), 0, default, Main.rand.NextFloat(1.2f, 1.8f));
                frost.noGravity = true;
            }
        }
    }
}
