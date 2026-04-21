using System;
using MagnumOpus.Common.BaseClasses;
using MagnumOpus.Content.ClairDeLune;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.ClairDeLune.Weapons.ClockworkHarmony.Projectiles
{
    /// <summary>
    /// Clockwork Harmony — Clair de Lune melee greatsword. Exoblade-architecture swing.
    /// Unique mechanic: Every 3 hits on same NPC, trigger a cascading harmonic burst.
    /// Clockwork-brass trail with pearl-blue accents and gear particle effects.
    /// </summary>
    public class ClockworkHarmonySwing : ExobladeStyleSwing
    {
        protected override bool SupportsDash => true;

        protected override float BladeLength => 100f;
        protected override int BaseSwingFrames => 76;
        protected override float TextureDrawScale => 0.89f;
        protected override string GradientLUTPath => "MagnumOpus/Assets/VFX Asset Library/ColorGradients/ClairDeLuneGradientLUTandRAMP";
        protected override Color SlashPrimaryColor => ClairDeLunePalette.ClockworkBrass;
        protected override Color SlashSecondaryColor => ClairDeLunePalette.DeepNight;
        protected override Color SlashAccentColor => ClairDeLunePalette.PearlWhite;

        public override string Texture => "MagnumOpus/Content/ClairDeLune/Weapons/ClockworkHarmony/ClockworkHarmony";

        protected override Color GetLensFlareColor(float p)
            => Color.Lerp(ClairDeLunePalette.ClockworkBrass, ClairDeLunePalette.WhiteHot, (float)Math.Pow(p, 2));

        protected override Color GetSwingDustColor()
        {
            float t = Main.rand.NextFloat();
            return t < 0.5f
                ? Color.Lerp(ClairDeLunePalette.ClockworkBrass, ClairDeLunePalette.MoonbeamGold, Main.rand.NextFloat())
                : Color.Lerp(ClairDeLunePalette.PearlBlue, ClairDeLunePalette.PearlWhite, Main.rand.NextFloat());
        }

        protected override void OnSwingFrame()
        {
            // Clockwork mechanic: Periodic gear sparkles
            if (Progression > 0.2f && Main.rand.NextBool(5))
                ClairDeLuneVFXLibrary.SpawnClockworkSparkle(
                    Owner.MountedCenter + SwordDirection * BladeLength * Projectile.scale * Main.rand.NextFloat(0.3f, 1f), 1);

            ClairDeLuneVFXLibrary.SwingFrameVFX(
                Owner.MountedCenter + SwordDirection * BladeLength * Projectile.scale * 0.7f,
                -SwordDirection, (int)Progression, Projectile.timeLeft);
        }

        protected override void OnSwingHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            ClairDeLuneVFXLibrary.MeleeImpact(target.Center, (int)Progression);

            // Harmonic resonance counter (stored in target's global data)
            var globalNPC = target.GetGlobalNPC<ClairDeLuneGlobalNPC>();
            globalNPC.AutomatonActiveFrequencies++;

            // Every 3 hits = cascading harmonic burst
            if (globalNPC.AutomatonActiveFrequencies % 3 == 0)
            {
                ClairDeLuneVFXLibrary.SpawnPearlExplosion(target.Center, 1.5f);
                Lighting.AddLight(target.Center, ClairDeLunePalette.MoonbeamGold.ToVector3() * 1.2f);
            }

            target.AddBuff(BuffID.Frostburn, 180);
        }

        protected override void OnDashHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Right-click dash: massive harmonic cascade
            ClairDeLuneVFXLibrary.FinisherSlam(target.Center, 2.0f);

            // Force trigger cascading burst
            var globalNPC = target.GetGlobalNPC<ClairDeLuneGlobalNPC>();
            globalNPC.AutomatonActiveFrequencies = 3;

            target.AddBuff(BuffID.Frostburn, 300);
        }
    }
}
