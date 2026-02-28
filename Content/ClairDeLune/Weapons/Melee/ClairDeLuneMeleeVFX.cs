using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Content.ClairDeLune.Weapons.Melee
{
    // =========================================================================
    //  ChronologicalityVFX — Temporal Drill
    //  Shader: TemporalDrill.fx (TemporalDrillBore + TemporalDrillGlow)
    //  Identity: Time-rip spiraling bore — the drill tears through reality
    //  itself, leaving crimson temporal fractures in its wake.
    // =========================================================================
    public static class ChronologicalityVFX
    {
        public static void HoldItemVFX(Player player)
        {
            if (Main.rand.NextBool(3))
            {
                Vector2 offset = Main.rand.NextVector2Circular(20f, 20f);
                Vector2 vel = offset * 0.03f;
                Dust d = Dust.NewDustPerfect(player.Center + offset, DustID.FireworkFountain_Red, vel, 0, default, 0.5f);
                d.noGravity = true;
                d.fadeIn = 0.6f;
            }

            if (Main.rand.NextBool(6))
            {
                Vector2 edgeOffset = Main.rand.NextVector2CircularEdge(18f, 18f);
                Dust pearl = Dust.NewDustPerfect(player.Center + edgeOffset, DustID.IceTorch,
                    edgeOffset * 0.015f, 0, default, 0.35f);
                pearl.noGravity = true;
            }

            ClairDeLuneVFXLibrary.AddClairDeLuneLight(player.Center, 0.25f);
        }

        public static void PreDrawInWorldBloom(SpriteBatch sb, Texture2D tex, Vector2 pos,
            Vector2 origin, float rotation, float scale)
        {
            if (ClairDeLuneShaderManager.HasTemporalDrill)
            {
                ClairDeLuneShaderManager.BeginShaderAdditive(sb);
                ClairDeLuneShaderManager.ApplyTemporalDrillGlow((float)Main.timeForVisualEffects * 0.03f);

                sb.Draw(tex, pos - Main.screenPosition, null,
                    ClairDeLunePalette.TemporalCrimson * 0.5f, rotation, origin, scale * 1.04f,
                    SpriteEffects.None, 0f);

                ClairDeLuneShaderManager.RestoreSpriteBatch(sb);
            }
            else
            {
                ClairDeLuneVFXLibrary.DrawClairDeLuneBloomStack(sb, pos,
                    ClairDeLunePalette.TemporalCrimson, ClairDeLunePalette.PearlWhite, scale * 0.3f, 0.4f);
            }
        }

        public static void DrillTrailVFX(Vector2 tipPos, Vector2 drillDirection)
        {
            Vector2 perpendicular = new Vector2(-drillDirection.Y, drillDirection.X);

            Vector2 dustVel = perpendicular * Main.rand.NextFloat(-2f, 2f) + drillDirection * 1.5f;
            Dust d = Dust.NewDustPerfect(tipPos, DustID.FireworkFountain_Red, dustVel, 0, default, 0.8f);
            d.noGravity = true;
            d.fadeIn = 0.9f;

            if (Main.GameUpdateCount % 2 == 0)
            {
                Vector2 pearlVel = drillDirection * 2.5f + Main.rand.NextVector2Circular(0.8f, 0.8f);
                Dust pearl = Dust.NewDustPerfect(tipPos, DustID.IceTorch, pearlVel, 0, default, 0.5f);
                pearl.noGravity = true;
            }

            if (Main.GameUpdateCount % 8 == 0)
                ClairDeLuneVFXLibrary.SpawnMusicNotes(tipPos, 1, 8f, 0.3f, 0.5f, 15);

            ClairDeLuneVFXLibrary.AddPaletteLighting(tipPos, 0.8f, 0.5f);
        }

        public static void DrillImpactVFX(Vector2 hitPos)
        {
            ClairDeLuneVFXLibrary.MeleeImpact(hitPos, 0);

            int dustCount = 14;
            for (int i = 0; i < dustCount; i++)
            {
                float angle = MathHelper.TwoPi * i / dustCount + (float)Main.timeForVisualEffects * 0.1f;
                Vector2 vel = angle.ToRotationVector2() * (5f + Main.rand.NextFloat() * 3f);
                int dustType = (i % 3 == 0) ? DustID.IceTorch : DustID.FireworkFountain_Red;
                Dust d = Dust.NewDustPerfect(hitPos, dustType, vel, 0, default, 1f);
                d.noGravity = true;
                d.fadeIn = 1.1f;
            }

            ClairDeLuneVFXLibrary.SpawnPearlBurst(hitPos, 6, 3.5f, 0.25f);
            ClairDeLuneVFXLibrary.SpawnMusicNotes(hitPos, 2, 12f, 0.4f, 0.7f, 20);
            ClairDeLuneVFXLibrary.DrawBloom(hitPos, 0.4f, 0.7f);
            ClairDeLuneVFXLibrary.AddPaletteLighting(hitPos, 0.8f, 0.7f);
        }

        public static void CriticalDischargeVFX(Vector2 pos)
        {
            ClairDeLuneVFXLibrary.SpawnRadialDustBurst(pos, 20, 7f, DustID.FireworkFountain_Red);
            ClairDeLuneVFXLibrary.SpawnPearlBurst(pos, 12, 6f, 0.4f);
            ClairDeLuneVFXLibrary.SpawnGradientHaloRings(pos, 4, 0.4f);
            ClairDeLuneVFXLibrary.SpawnMusicNotes(pos, 4, 20f, 0.5f, 0.9f, 30);
            ClairDeLuneVFXLibrary.DrawBloom(pos, 0.8f, 1f);
            ClairDeLuneVFXLibrary.AddPaletteLighting(pos, 0.8f, 1.2f);
        }
    }

    // =========================================================================
    //  TemporalPiercerVFX — Crystal Lance
    //  Shader: CrystalLance.fx (CrystalLanceThrust + CrystalLanceShatter)
    //  Identity: Frost-crystal pierce — a frozen lance that crystallizes
    //  the air itself, shattering into ice shards on impact.
    // =========================================================================
    public static class TemporalPiercerVFX
    {
        public static void HoldItemVFX(Player player)
        {
            if (Main.rand.NextBool(4))
            {
                Vector2 offset = Main.rand.NextVector2Circular(22f, 22f);
                Dust d = Dust.NewDustPerfect(player.Center + offset, DustID.IceTorch,
                    Main.rand.NextVector2Circular(0.2f, 0.2f), 0, default, 0.4f);
                d.noGravity = true;
                d.fadeIn = 0.6f;
            }

            if (Main.rand.NextBool(8))
                ClairDeLuneVFXLibrary.SpawnPearlSparkle(player.Center, Main.rand.NextVector2Circular(1f, 1f));

            ClairDeLuneVFXLibrary.AddClairDeLuneLight(player.Center, 0.2f);
        }

        public static void PreDrawInWorldBloom(SpriteBatch sb, Texture2D tex, Vector2 pos,
            Vector2 origin, float rotation, float scale)
        {
            if (ClairDeLuneShaderManager.HasCrystalLance)
            {
                ClairDeLuneShaderManager.BeginShaderAdditive(sb);
                ClairDeLuneShaderManager.ApplyCrystalLanceShatter((float)Main.timeForVisualEffects * 0.03f);

                sb.Draw(tex, pos - Main.screenPosition, null,
                    ClairDeLunePalette.MoonlitFrost * 0.5f, rotation, origin, scale * 1.03f,
                    SpriteEffects.None, 0f);

                ClairDeLuneShaderManager.RestoreSpriteBatch(sb);
            }
            else
            {
                ClairDeLuneVFXLibrary.DrawClairDeLuneBloomStack(sb, pos,
                    ClairDeLunePalette.MoonlitFrost, ClairDeLunePalette.PearlBlue, scale * 0.25f, 0.35f);
            }
        }

        public static void ThrustTrailVFX(Vector2 tipPos, Vector2 thrustDir, bool isCharged)
        {
            float intensity = isCharged ? 1.5f : 1f;

            Vector2 perpendicular = new Vector2(-thrustDir.Y, thrustDir.X);
            Vector2 dustVel = thrustDir * 3f * intensity + perpendicular * Main.rand.NextFloat(-0.5f, 0.5f);
            Dust d = Dust.NewDustPerfect(tipPos, DustID.IceTorch, dustVel, 0, default, 0.7f * intensity);
            d.noGravity = true;
            d.fadeIn = 0.8f;

            if (isCharged && Main.GameUpdateCount % 2 == 0)
                ClairDeLuneVFXLibrary.SpawnPearlSparkle(tipPos, thrustDir);

            if (Main.GameUpdateCount % 6 == 0)
                ClairDeLuneVFXLibrary.SpawnMusicNotes(tipPos, 1, 6f, 0.3f, 0.5f, 12);

            ClairDeLuneVFXLibrary.AddPaletteLighting(tipPos, 0.3f, 0.4f * intensity);
        }

        public static void ThrustImpactVFX(Vector2 hitPos, bool isCharged)
        {
            float intensity = isCharged ? 1.5f : 1f;
            ClairDeLuneVFXLibrary.MeleeImpact(hitPos, isCharged ? 2 : 0);

            int dustCount = (int)(12 * intensity);
            for (int i = 0; i < dustCount; i++)
            {
                float angle = MathHelper.TwoPi * i / dustCount;
                Vector2 vel = angle.ToRotationVector2() * (4f + Main.rand.NextFloat() * 3f) * intensity;
                Dust d = Dust.NewDustPerfect(hitPos, DustID.IceTorch, vel, 0, default, 0.9f * intensity);
                d.noGravity = true;
                d.fadeIn = 1f;
            }

            ClairDeLuneVFXLibrary.SpawnPearlBurst(hitPos, (int)(8 * intensity), 4f * intensity, 0.3f);

            if (isCharged)
            {
                ClairDeLuneVFXLibrary.SpawnGradientHaloRings(hitPos, 3, 0.35f);
                ClairDeLuneVFXLibrary.SpawnStarlitSparkles(hitPos, 6, 30f, 0.25f);
            }

            ClairDeLuneVFXLibrary.SpawnMusicNotes(hitPos, 2, 12f, 0.4f, 0.7f, 20);
            ClairDeLuneVFXLibrary.DrawBloom(hitPos, 0.35f * intensity, 0.7f);
            ClairDeLuneVFXLibrary.AddPaletteLighting(hitPos, 0.3f, 0.6f * intensity);
        }
    }

    // =========================================================================
    //  ClockworkHarmonyVFX — Gear Swing
    //  Shader: GearSwing.fx (GearSwingArc + GearSwingTrail)
    //  Identity: Music box pendulum — a grand brass pendulum that sweeps
    //  in rhythmic arcs, scattering gear-tooth sparks and golden notes.
    // =========================================================================
    public static class ClockworkHarmonyVFX
    {
        public static void HoldItemVFX(Player player)
        {
            if (Main.rand.NextBool(4))
            {
                Vector2 offset = Main.rand.NextVector2Circular(20f, 20f);
                Dust d = Dust.NewDustPerfect(player.Center + offset, DustID.GoldFlame,
                    Main.rand.NextVector2Circular(0.2f, 0.2f), 0, default, 0.4f);
                d.noGravity = true;
                d.fadeIn = 0.5f;
            }

            if (Main.rand.NextBool(10))
                ClairDeLuneVFXLibrary.SpawnPearlShimmer(player.Center, 1, 20f, 0.15f);

            ClairDeLuneVFXLibrary.AddClairDeLuneLight(player.Center, 0.2f);
        }

        public static void PreDrawInWorldBloom(SpriteBatch sb, Texture2D tex, Vector2 pos,
            Vector2 origin, float rotation, float scale)
        {
            if (ClairDeLuneShaderManager.HasGearSwing)
            {
                ClairDeLuneShaderManager.BeginShaderAdditive(sb);
                ClairDeLuneShaderManager.ApplyGearSwingTrail((float)Main.timeForVisualEffects * 0.03f);

                sb.Draw(tex, pos - Main.screenPosition, null,
                    ClairDeLunePalette.ClockworkBrass * 0.5f, rotation, origin, scale * 1.03f,
                    SpriteEffects.None, 0f);

                ClairDeLuneShaderManager.RestoreSpriteBatch(sb);
            }
            else
            {
                ClairDeLuneVFXLibrary.DrawClairDeLuneBloomStack(sb, pos,
                    ClairDeLunePalette.ClockworkBrass, ClairDeLunePalette.MoonbeamGold, scale * 0.25f, 0.35f);
            }
        }

        public static void SwingTrailVFX(Vector2 tipPos, Vector2 swordDirection, int comboStep, int timer)
        {
            Vector2 perpendicular = new Vector2(-swordDirection.Y, swordDirection.X);

            Vector2 dustVel = perpendicular * Main.rand.NextFloat(-1.5f, 1.5f) + swordDirection * 1f;
            Dust d = Dust.NewDustPerfect(tipPos, DustID.GoldFlame, dustVel, 0, default, 0.8f);
            d.noGravity = true;
            d.fadeIn = 0.9f;

            if (timer % 2 == 0)
            {
                Vector2 goldVel = swordDirection * 2f + Main.rand.NextVector2Circular(0.8f, 0.8f);
                Dust gold = Dust.NewDustPerfect(tipPos, DustID.IceTorch, goldVel, 0, default, 0.5f);
                gold.noGravity = true;
            }

            if (timer % 4 == 0)
                ClairDeLuneVFXLibrary.SpawnMusicNotes(tipPos, 1, 8f, 0.35f, 0.6f, 18);

            if (comboStep >= 2 && timer % 6 == 0)
                ClairDeLuneVFXLibrary.SpawnPearlShimmer(tipPos, 1, 15f, 0.2f);

            ClairDeLuneVFXLibrary.AddPaletteLighting(tipPos, 0.6f, 0.4f);
        }

        public static void SwingImpactVFX(Vector2 hitPos, int comboStep = 0)
        {
            float intensity = 1f + comboStep * 0.2f;
            ClairDeLuneVFXLibrary.MeleeImpact(hitPos, comboStep);

            int dustCount = 10 + comboStep * 3;
            for (int i = 0; i < dustCount; i++)
            {
                float angle = MathHelper.TwoPi * i / dustCount;
                Vector2 vel = angle.ToRotationVector2() * (4f + Main.rand.NextFloat() * 2f) * intensity;
                int dustType = (i % 2 == 0) ? DustID.GoldFlame : DustID.IceTorch;
                Dust d = Dust.NewDustPerfect(hitPos, dustType, vel, 0, default, 1f * intensity);
                d.noGravity = true;
                d.fadeIn = 1.1f;
            }

            if (comboStep >= 1)
                ClairDeLuneVFXLibrary.SpawnPearlBurst(hitPos, 4 + comboStep * 2, 3f, 0.25f);

            if (comboStep >= 2)
                ClairDeLuneVFXLibrary.SpawnGradientHaloRings(hitPos, 3, 0.3f * intensity);

            ClairDeLuneVFXLibrary.SpawnMusicNotes(hitPos, 1 + comboStep, 14f, 0.45f, 0.8f, 22);
            ClairDeLuneVFXLibrary.DrawBloom(hitPos, 0.35f * intensity, 0.7f);
            ClairDeLuneVFXLibrary.AddPaletteLighting(hitPos, 0.6f, 0.6f * intensity);
        }

        public static void FinisherVFX(Vector2 pos, float intensity = 1f)
        {
            ClairDeLuneVFXLibrary.FinisherSlam(pos, intensity);
            ClairDeLuneVFXLibrary.SpawnRadialDustBurst(pos, 16, 6f * intensity, DustID.GoldFlame);
            ClairDeLuneVFXLibrary.SpawnPearlBurst(pos, 10, 5f * intensity, 0.35f);
            ClairDeLuneVFXLibrary.SpawnGradientHaloRings(pos, 5, 0.4f * intensity);
            ClairDeLuneVFXLibrary.SpawnMusicNotes(pos, 4, 25f, 0.5f, 1f, 35);
            ClairDeLuneVFXLibrary.DrawBloom(pos, 0.7f * intensity, 1f);
            ClairDeLuneVFXLibrary.AddPaletteLighting(pos, 0.6f, 1f * intensity);
        }
    }
}
