using System;
using MagnumOpus.Common.BaseClasses;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.DiesIrae.Weapons.WrathsCleaver.Projectiles
{
    /// <summary>
    /// Wrath's Cleaver — Dies Irae theme melee. Exoblade-architecture swing.
    /// Infernal red trail with fire embers and judgment gold highlights.
    /// Combo phase tracking lives here so it advances on hold re-swings.
    /// </summary>
    public class WrathsCleaverSwing : ExobladeStyleSwing
    {
        /// <summary>Wrath combo phase (0-3). Advances each swing including hold re-swings.</summary>
        private int comboPhase = 0;

        protected override float BladeLength => 115f;
        protected override int BaseSwingFrames => 82;
        protected override float TextureDrawScale => 0.93f;
        protected override string GradientLUTPath => "MagnumOpus/Assets/VFX Asset Library/ColorGradients/DiesIraeGradientLUTandRAMP";
        protected override Color SlashPrimaryColor => new Color(255, 60, 30);
        protected override Color SlashSecondaryColor => new Color(80, 10, 10);
        protected override Color SlashAccentColor => new Color(255, 180, 50);

        public override string Texture => "MagnumOpus/Content/DiesIrae/Weapons/WrathsCleaver/WrathsCleaver";

        protected override void OnSwingStart(bool isFirstSwing)
        {
            if (Main.myPlayer != Projectile.owner) return;

            int phase = comboPhase % 4;
            comboPhase++;
        }

        protected override Color GetLensFlareColor(float p)
            => Color.Lerp(new Color(255, 80, 30), new Color(255, 200, 80), (float)Math.Pow(p, 2));

        protected override Color GetSwingDustColor()
        {
            float t = Main.rand.NextFloat();
            return t < 0.4f
                ? Color.Lerp(new Color(255, 50, 20), new Color(180, 10, 10), Main.rand.NextFloat())
                : Color.Lerp(new Color(255, 180, 50), new Color(255, 80, 30), Main.rand.NextFloat());
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
                Dust spark = Dust.NewDustPerfect(tip, DustID.SolarFlare, Main.rand.NextVector2Circular(2f, 2f), 0, default, 0.7f);
                spark.noGravity = true;
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
                Dust spark = Dust.NewDustPerfect(target.Center, DustID.SolarFlare,
                    Main.rand.NextVector2Circular(3f, 3f), 0, default, 0.8f);
                spark.noGravity = true;
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
