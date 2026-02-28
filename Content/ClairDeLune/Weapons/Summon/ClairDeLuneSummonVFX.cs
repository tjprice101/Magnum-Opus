using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Content.ClairDeLune.Weapons.Summon
{
    // =========================================================================
    //  LunarPhylacteryVFX — Soul Beam
    //  Shader: SoulBeam.fx (SoulBeamTether + SoulBeamAura)
    //  Identity: Moonlit soul tether — the phylactery binds lunar souls
    //  that fire ethereal beams of pearl-blue light at enemies.
    // =========================================================================
    public static class LunarPhylacteryVFX
    {
        public static void HoldItemVFX(Player player)
        {
            if (Main.rand.NextBool(3))
            {
                Vector2 offset = Main.rand.NextVector2Circular(22f, 22f);
                Dust d = Dust.NewDustPerfect(player.Center + offset, DustID.IceTorch,
                    new Vector2(0f, -Main.rand.NextFloat(0.3f, 0.7f)), 0, default, 0.4f);
                d.noGravity = true;
                d.fadeIn = 0.6f;
            }

            if (Main.rand.NextBool(6))
                ClairDeLuneVFXLibrary.SpawnMoonlitMist(player.Center, 1, 15f, 0.15f);

            ClairDeLuneVFXLibrary.AddClairDeLuneLight(player.Center, 0.25f);
        }

        public static void PreDrawInWorldBloom(SpriteBatch sb, Texture2D tex, Vector2 pos,
            Vector2 origin, float rotation, float scale)
        {
            if (ClairDeLuneShaderManager.HasSoulBeam)
            {
                ClairDeLuneShaderManager.BeginShaderAdditive(sb);
                ClairDeLuneShaderManager.ApplySoulBeamAura((float)Main.timeForVisualEffects * 0.025f);

                sb.Draw(tex, pos - Main.screenPosition, null,
                    ClairDeLunePalette.PearlBlue * 0.45f, rotation, origin, scale * 1.04f,
                    SpriteEffects.None, 0f);

                ClairDeLuneShaderManager.RestoreSpriteBatch(sb);
            }
            else
            {
                ClairDeLuneVFXLibrary.DrawClairDeLuneBloomStack(sb, pos,
                    ClairDeLunePalette.PearlBlue, ClairDeLunePalette.PearlWhite, scale * 0.25f, 0.4f);
            }
        }

        public static void SummonVFX(Vector2 summonPos)
        {
            int dustCount = 16;
            for (int i = 0; i < dustCount; i++)
            {
                float angle = MathHelper.TwoPi * i / dustCount;
                float radius = 40f;
                Vector2 dustPos = summonPos + angle.ToRotationVector2() * radius;
                Vector2 vel = (summonPos - dustPos) * 0.06f;
                Dust d = Dust.NewDustPerfect(dustPos, DustID.IceTorch, vel, 0, default, 0.8f);
                d.noGravity = true;
                d.fadeIn = 0.9f;
            }

            ClairDeLuneVFXLibrary.SpawnPearlBurst(summonPos, 8, 4f, 0.3f);
            ClairDeLuneVFXLibrary.SpawnGradientHaloRings(summonPos, 3, 0.35f);
            ClairDeLuneVFXLibrary.SpawnMusicNotes(summonPos, 3, 15f, 0.4f, 0.8f, 25);
            ClairDeLuneVFXLibrary.DrawBloom(summonPos, 0.5f, 0.8f);
            ClairDeLuneVFXLibrary.AddPaletteLighting(summonPos, 0.3f, 0.7f);
        }

        public static void MinionAmbientVFX(Vector2 minionPos)
        {
            if (Main.rand.NextBool(3))
            {
                Vector2 offset = Main.rand.NextVector2Circular(12f, 12f);
                Dust d = Dust.NewDustPerfect(minionPos + offset, DustID.IceTorch,
                    new Vector2(0f, -Main.rand.NextFloat(0.2f, 0.5f)), 0, default, 0.3f);
                d.noGravity = true;
                d.fadeIn = 0.4f;
            }

            if (Main.rand.NextBool(12))
                ClairDeLuneVFXLibrary.SpawnPearlSparkle(minionPos, Main.rand.NextVector2Circular(0.5f, 0.5f));

            ClairDeLuneVFXLibrary.AddPaletteLighting(minionPos, 0.3f, 0.2f);
        }

        public static void SoulBeamFireVFX(Vector2 minionPos, Vector2 beamDirection)
        {
            for (int i = 0; i < 6; i++)
            {
                Vector2 vel = beamDirection * (5f + Main.rand.NextFloat() * 3f) +
                    Main.rand.NextVector2Circular(0.8f, 0.8f);
                Dust d = Dust.NewDustPerfect(minionPos, DustID.IceTorch, vel, 0, default, 0.7f);
                d.noGravity = true;
                d.fadeIn = 0.8f;
            }

            ClairDeLuneVFXLibrary.SpawnPearlBurst(minionPos, 3, 2.5f, 0.2f);
            ClairDeLuneVFXLibrary.SpawnMusicNotes(minionPos, 1, 8f, 0.3f, 0.5f, 12);
            ClairDeLuneVFXLibrary.AddPaletteLighting(minionPos, 0.3f, 0.4f);
        }

        public static void SoulBeamImpactVFX(Vector2 hitPos)
        {
            ClairDeLuneVFXLibrary.ProjectileImpact(hitPos, 0);

            int dustCount = 10;
            for (int i = 0; i < dustCount; i++)
            {
                float angle = MathHelper.TwoPi * i / dustCount;
                Vector2 vel = angle.ToRotationVector2() * (3f + Main.rand.NextFloat() * 2f);
                Dust d = Dust.NewDustPerfect(hitPos, DustID.IceTorch, vel, 0, default, 0.8f);
                d.noGravity = true;
                d.fadeIn = 0.9f;
            }

            ClairDeLuneVFXLibrary.SpawnPearlBurst(hitPos, 5, 3f, 0.25f);
            ClairDeLuneVFXLibrary.SpawnMusicNotes(hitPos, 1, 10f, 0.3f, 0.55f, 15);
            ClairDeLuneVFXLibrary.DrawBloom(hitPos, 0.3f, 0.5f);
            ClairDeLuneVFXLibrary.AddPaletteLighting(hitPos, 0.3f, 0.4f);
        }
    }

    // =========================================================================
    //  GearDrivenArbiterVFX — Judgment Mark
    //  Shader: JudgmentMark.fx (JudgmentMarkSigil + JudgmentMarkDetonate)
    //  Identity: Clockwork judgment sigil — the arbiter marks enemies with
    //  rotating gear sigils that detonate in righteous mechanical fury.
    // =========================================================================
    public static class GearDrivenArbiterVFX
    {
        public static void HoldItemVFX(Player player)
        {
            if (Main.rand.NextBool(4))
            {
                Vector2 offset = Main.rand.NextVector2Circular(20f, 20f);
                Dust d = Dust.NewDustPerfect(player.Center + offset, DustID.GoldFlame,
                    Main.rand.NextVector2Circular(0.15f, 0.15f), 0, default, 0.4f);
                d.noGravity = true;
                d.fadeIn = 0.5f;
            }

            if (Main.rand.NextBool(10))
                ClairDeLuneVFXLibrary.SpawnPearlShimmer(player.Center, 1, 20f, 0.15f);

            ClairDeLuneVFXLibrary.AmbientClockworkAura(player.Center, (float)Main.timeForVisualEffects);
            ClairDeLuneVFXLibrary.AddClairDeLuneLight(player.Center, 0.2f);
        }

        public static void PreDrawInWorldBloom(SpriteBatch sb, Texture2D tex, Vector2 pos,
            Vector2 origin, float rotation, float scale)
        {
            if (ClairDeLuneShaderManager.HasJudgmentMark)
            {
                ClairDeLuneShaderManager.BeginShaderAdditive(sb);
                ClairDeLuneShaderManager.ApplyJudgmentMarkDetonate((float)Main.timeForVisualEffects * 0.03f, 0f);

                sb.Draw(tex, pos - Main.screenPosition, null,
                    ClairDeLunePalette.ClockworkBrass * 0.5f, rotation, origin, scale * 1.04f,
                    SpriteEffects.None, 0f);

                ClairDeLuneShaderManager.RestoreSpriteBatch(sb);
            }
            else
            {
                ClairDeLuneVFXLibrary.DrawClairDeLuneBloomStack(sb, pos,
                    ClairDeLunePalette.ClockworkBrass, ClairDeLunePalette.MoonbeamGold, scale * 0.25f, 0.35f);
            }
        }

        public static void SummonVFX(Vector2 summonPos)
        {
            int dustCount = 14;
            for (int i = 0; i < dustCount; i++)
            {
                float angle = MathHelper.TwoPi * i / dustCount;
                float radius = 35f;
                Vector2 dustPos = summonPos + angle.ToRotationVector2() * radius;
                Vector2 vel = (summonPos - dustPos) * 0.05f;
                int dustType = (i % 2 == 0) ? DustID.GoldFlame : DustID.IceTorch;
                Dust d = Dust.NewDustPerfect(dustPos, dustType, vel, 0, default, 0.7f);
                d.noGravity = true;
                d.fadeIn = 0.8f;
            }

            ClairDeLuneVFXLibrary.SpawnPearlBurst(summonPos, 6, 3.5f, 0.25f);
            ClairDeLuneVFXLibrary.SpawnGradientHaloRings(summonPos, 3, 0.3f);
            ClairDeLuneVFXLibrary.SpawnMusicNotes(summonPos, 2, 14f, 0.4f, 0.7f, 22);
            ClairDeLuneVFXLibrary.DrawBloom(summonPos, 0.45f, 0.7f);
            ClairDeLuneVFXLibrary.AddPaletteLighting(summonPos, 0.6f, 0.6f);
        }

        public static void MinionAmbientVFX(Vector2 minionPos)
        {
            if (Main.rand.NextBool(3))
            {
                float ringAngle = (float)Main.timeForVisualEffects * 0.04f;
                Vector2 ringOffset = ringAngle.ToRotationVector2() * 10f;
                Dust d = Dust.NewDustPerfect(minionPos + ringOffset, DustID.GoldFlame,
                    Vector2.Zero, 0, default, 0.25f);
                d.noGravity = true;
            }

            if (Main.rand.NextBool(8))
            {
                Dust pearl = Dust.NewDustPerfect(minionPos + Main.rand.NextVector2Circular(10f, 10f),
                    DustID.IceTorch, Vector2.Zero, 0, default, 0.2f);
                pearl.noGravity = true;
            }

            ClairDeLuneVFXLibrary.AddPaletteLighting(minionPos, 0.6f, 0.2f);
        }

        public static void JudgmentStrikeVFX(Vector2 strikePos)
        {
            int dustCount = 16;
            for (int i = 0; i < dustCount; i++)
            {
                float angle = MathHelper.TwoPi * i / dustCount;
                Vector2 vel = angle.ToRotationVector2() * (5f + Main.rand.NextFloat() * 3f);
                int dustType = (i % 3 == 0) ? DustID.GoldFlame : DustID.IceTorch;
                Dust d = Dust.NewDustPerfect(strikePos, dustType, vel, 0, default, 1f);
                d.noGravity = true;
                d.fadeIn = 1.1f;
            }

            ClairDeLuneVFXLibrary.SpawnRadialDustBurst(strikePos, 12, 6f, DustID.GoldFlame);
            ClairDeLuneVFXLibrary.SpawnPearlBurst(strikePos, 8, 4f, 0.35f);
            ClairDeLuneVFXLibrary.SpawnGradientHaloRings(strikePos, 4, 0.4f);
            ClairDeLuneVFXLibrary.SpawnMusicNotes(strikePos, 3, 16f, 0.45f, 0.8f, 25);
            ClairDeLuneVFXLibrary.DrawBloom(strikePos, 0.6f, 0.9f);
            ClairDeLuneVFXLibrary.AddPaletteLighting(strikePos, 0.6f, 0.8f);
        }

        public static void MarkTargetVFX(Vector2 targetPos, float markProgress = 0f)
        {
            float intensity = MathHelper.Clamp(markProgress, 0f, 1f);
            float ringAngle = (float)Main.timeForVisualEffects * 0.03f;

            for (int i = 0; i < 4; i++)
            {
                float angle = ringAngle + MathHelper.PiOver2 * i;
                float radius = 20f + 8f * intensity;
                Vector2 dustPos = targetPos + angle.ToRotationVector2() * radius;
                Dust d = Dust.NewDustPerfect(dustPos, DustID.GoldFlame,
                    Vector2.Zero, 0, default, 0.4f * (0.5f + intensity * 0.5f));
                d.noGravity = true;
            }

            if (intensity > 0.5f && Main.rand.NextBool(3))
                ClairDeLuneVFXLibrary.SpawnPearlSparkle(targetPos, Main.rand.NextVector2Circular(0.5f, 0.5f));

            ClairDeLuneVFXLibrary.AddPaletteLighting(targetPos, 0.6f, 0.2f * intensity);
        }
    }

    // =========================================================================
    //  AutomatonsTuningForkVFX — Resonance Field
    //  Shader: ResonanceField.fx (ResonanceFieldPulse + ResonanceFieldHarmonic)
    //  Identity: Harmonic resonance pulse — the tuning fork emits expanding
    //  concentric pulses of pure harmonic energy that heal and empower.
    // =========================================================================
    public static class AutomatonsTuningForkVFX
    {
        public static void HoldItemVFX(Player player)
        {
            if (Main.rand.NextBool(3))
            {
                Vector2 offset = Main.rand.NextVector2Circular(24f, 24f);
                Dust d = Dust.NewDustPerfect(player.Center + offset, DustID.IceTorch,
                    offset * 0.01f, 0, default, 0.35f);
                d.noGravity = true;
                d.fadeIn = 0.5f;
            }

            if (Main.rand.NextBool(8))
                ClairDeLuneVFXLibrary.SpawnMoonlitMist(player.Center, 1, 18f, 0.12f);

            ClairDeLuneVFXLibrary.AddClairDeLuneLight(player.Center, 0.22f);
        }

        public static void PreDrawInWorldBloom(SpriteBatch sb, Texture2D tex, Vector2 pos,
            Vector2 origin, float rotation, float scale)
        {
            if (ClairDeLuneShaderManager.HasResonanceField)
            {
                ClairDeLuneShaderManager.BeginShaderAdditive(sb);
                ClairDeLuneShaderManager.ApplyResonanceFieldHarmonic((float)Main.timeForVisualEffects * 0.025f);

                sb.Draw(tex, pos - Main.screenPosition, null,
                    ClairDeLunePalette.SoftBlue * 0.45f, rotation, origin, scale * 1.04f,
                    SpriteEffects.None, 0f);

                ClairDeLuneShaderManager.RestoreSpriteBatch(sb);
            }
            else
            {
                ClairDeLuneVFXLibrary.DrawClairDeLuneBloomStack(sb, pos,
                    ClairDeLunePalette.SoftBlue, ClairDeLunePalette.PearlWhite, scale * 0.25f, 0.35f);
            }
        }

        public static void SummonVFX(Vector2 summonPos)
        {
            int dustCount = 12;
            for (int i = 0; i < dustCount; i++)
            {
                float angle = MathHelper.TwoPi * i / dustCount;
                float radius = 30f;
                Vector2 dustPos = summonPos + angle.ToRotationVector2() * radius;
                Vector2 vel = (summonPos - dustPos) * 0.04f;
                Dust d = Dust.NewDustPerfect(dustPos, DustID.IceTorch, vel, 0, default, 0.7f);
                d.noGravity = true;
                d.fadeIn = 0.8f;
            }

            ClairDeLuneVFXLibrary.SpawnPearlBurst(summonPos, 6, 3.5f, 0.25f);
            ClairDeLuneVFXLibrary.SpawnMusicNotes(summonPos, 3, 14f, 0.4f, 0.8f, 22);
            ClairDeLuneVFXLibrary.DrawBloom(summonPos, 0.4f, 0.7f);
            ClairDeLuneVFXLibrary.AddPaletteLighting(summonPos, 0.3f, 0.6f);
        }

        public static void MinionAmbientVFX(Vector2 minionPos)
        {
            if (Main.rand.NextBool(4))
            {
                float pulse = (float)Math.Sin(Main.timeForVisualEffects * 0.04) * 0.5f + 0.5f;
                float radius = 8f + 6f * pulse;
                float angle = Main.rand.NextFloat() * MathHelper.TwoPi;
                Vector2 dustPos = minionPos + angle.ToRotationVector2() * radius;
                Dust d = Dust.NewDustPerfect(dustPos, DustID.IceTorch,
                    Vector2.Zero, 0, default, 0.25f * (0.5f + pulse * 0.5f));
                d.noGravity = true;
            }

            ClairDeLuneVFXLibrary.AddPaletteLighting(minionPos, 0.3f, 0.2f);
        }

        public static void ResonancePulseVFX(Vector2 pulseCenter, float pulseRadius = 120f)
        {
            int ringCount = 20;
            for (int i = 0; i < ringCount; i++)
            {
                float angle = MathHelper.TwoPi * i / ringCount;
                Vector2 ringPos = pulseCenter + angle.ToRotationVector2() * pulseRadius;
                Vector2 vel = (ringPos - pulseCenter).SafeNormalize(Vector2.Zero) * 2f;
                Dust d = Dust.NewDustPerfect(ringPos, DustID.IceTorch, vel, 0, default, 0.9f);
                d.noGravity = true;
                d.fadeIn = 1f;
            }

            ClairDeLuneVFXLibrary.SpawnRadialDustBurst(pulseCenter, 16, 5f, DustID.IceTorch);
            ClairDeLuneVFXLibrary.SpawnPearlBurst(pulseCenter, 10, 5f, 0.35f);
            ClairDeLuneVFXLibrary.SpawnGradientHaloRings(pulseCenter, 4, 0.4f);
            ClairDeLuneVFXLibrary.SpawnOrbitingNotes(pulseCenter, Vector2.Zero, 4, 40f);
            ClairDeLuneVFXLibrary.SpawnMusicNotes(pulseCenter, 3, 18f, 0.45f, 0.85f, 25);
            ClairDeLuneVFXLibrary.DrawBloom(pulseCenter, 0.6f, 0.9f);
            ClairDeLuneVFXLibrary.AddPaletteLighting(pulseCenter, 0.3f, 0.8f);
        }
    }
}
