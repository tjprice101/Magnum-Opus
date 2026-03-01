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
    //  Identity: ETHEREAL SOUL VESSEL — the phylactery is a dreamy moonlit
    //  container overflowing with captured lunar souls. Wisps of pearl-blue
    //  soul energy drift upward like escaping spirits. On beam fire, a
    //  luminous soul-streak lances outward with pulsing energy nodes.
    //  Distinct from Arbiter (brass clockwork) and Fork (harmonic waves).
    // =========================================================================
    public static class LunarPhylacteryVFX
    {
        public static void HoldItemVFX(Player player)
        {
            float time = (float)Main.timeForVisualEffects;

            // SIGNATURE: Soul wisps rising upward — ethereal moonlit spirits escaping the vessel
            if (Main.rand.NextBool(3))
            {
                Vector2 wispPos = player.Center + Main.rand.NextVector2Circular(18f, 10f);
                // Wisps drift upward with gentle sinusoidal sway
                float sway = (float)Math.Sin(time * 0.06f + wispPos.X * 0.02f) * 0.4f;
                Vector2 wispVel = new Vector2(sway, -Main.rand.NextFloat(0.4f, 1f));

                Color wispCol = ClairDeLunePalette.PaletteLerp(
                    ClairDeLunePalette.LunarPhylacterySummon,
                    0.3f + (float)Math.Sin(time * 0.04f) * 0.2f);
                var wisp = new GenericGlowParticle(wispPos, wispVel, wispCol * 0.4f, 0.16f, 18, true);
                MagnumParticleHandler.SpawnParticle(wisp);
            }

            // Moonlit mist pooling at feet
            if (Main.rand.NextBool(6))
                ClairDeLuneVFXLibrary.SpawnMoonlitMist(player.Center + Vector2.UnitY * 10f, 1, 12f, 0.12f);

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

        /// <summary>
        /// SUMMON RITUAL: Moonlit soul circle — souls converge from afar in a lunar ritual,
        /// forming a pearl-blue convergence ring before coalescing into the phylactery.
        /// </summary>
        public static void SummonVFX(Vector2 summonPos)
        {
            float time = (float)Main.timeForVisualEffects;

            // SIGNATURE: Converging soul wisps from outer circle
            int soulCount = 18;
            for (int i = 0; i < soulCount; i++)
            {
                float angle = MathHelper.TwoPi * i / soulCount;
                float radius = 45f;
                Vector2 soulPos = summonPos + angle.ToRotationVector2() * radius;
                Vector2 vel = (summonPos - soulPos) * 0.07f;

                Color soulCol = ClairDeLunePalette.PaletteLerp(
                    ClairDeLunePalette.LunarPhylacterySummon, (float)i / soulCount);

                // Alternating: dust + glow particle for layered soul appearance
                Dust d = Dust.NewDustPerfect(soulPos, DustID.IceTorch, vel, 0, default, 0.8f);
                d.noGravity = true;
                d.fadeIn = 0.9f;

                if (i % 2 == 0)
                {
                    var soulGlow = new GenericGlowParticle(soulPos, vel * 0.8f,
                        soulCol * 0.6f, 0.25f, 16, true);
                    MagnumParticleHandler.SpawnParticle(soulGlow);
                }
            }

            ClairDeLuneVFXLibrary.SpawnPearlBurst(summonPos, 8, 4f, 0.3f);
            ClairDeLuneVFXLibrary.SpawnGradientHaloRings(summonPos, 3, 0.35f);
            ClairDeLuneVFXLibrary.SpawnMusicNotes(summonPos, 3, 15f, 0.4f, 0.8f, 25);
            ClairDeLuneVFXLibrary.DrawBloom(summonPos, 0.5f, 0.8f);
            ClairDeLuneVFXLibrary.AddPaletteLighting(summonPos, 0.3f, 0.7f);
        }

        /// <summary>
        /// MINION AMBIENT: Ethereal soul halo — the phylactery minion has a gentle
        /// orbiting halo of soul wisps, with occasional pearl sparkle twinkle.
        /// Different from Arbiter (clockwork gear ring) and Fork (pulsing wave).
        /// </summary>
        public static void MinionAmbientVFX(Vector2 minionPos)
        {
            float time = (float)Main.timeForVisualEffects;

            // SIGNATURE: Orbiting soul wisp halo — 2 wisps circling gently
            if (Main.rand.NextBool(2))
            {
                for (int i = 0; i < 2; i++)
                {
                    float orbitAngle = time * 0.03f + i * MathHelper.Pi;
                    float orbitRadius = 10f + (float)Math.Sin(time * 0.02f + i) * 3f;
                    Vector2 orbitPos = minionPos + orbitAngle.ToRotationVector2() * orbitRadius;
                    Color soulCol = (i == 0)
                        ? ClairDeLunePalette.PearlBlue * 0.4f
                        : ClairDeLunePalette.PearlWhite * 0.3f;
                    Dust d = Dust.NewDustPerfect(orbitPos, DustID.IceTorch,
                        Vector2.Zero, 0, default, 0.22f);
                    d.noGravity = true;
                }
            }

            // Rising soul wisp
            if (Main.rand.NextBool(6))
            {
                var wisp = new GenericGlowParticle(
                    minionPos + Main.rand.NextVector2Circular(8f, 8f),
                    new Vector2(0f, -0.4f),
                    ClairDeLunePalette.PearlBlue * 0.3f, 0.1f, 12, true);
                MagnumParticleHandler.SpawnParticle(wisp);
            }

            if (Main.rand.NextBool(15))
                ClairDeLuneVFXLibrary.SpawnPearlSparkle(minionPos, Main.rand.NextVector2Circular(0.5f, 0.5f));

            ClairDeLuneVFXLibrary.AddPaletteLighting(minionPos, 0.3f, 0.2f);
        }

        /// <summary>
        /// SOUL BEAM FIRE: Luminous soul-streak — pearl-blue energy lances forward with
        /// pulsing soul-energy nodes along the beam axis. A burst of soul wisps scatters
        /// perpendicular to the beam at the fire point.
        /// </summary>
        public static void SoulBeamFireVFX(Vector2 minionPos, Vector2 beamDirection)
        {
            Vector2 perpendicular = new Vector2(-beamDirection.Y, beamDirection.X);

            // SIGNATURE: Forward soul lance with pulsing nodes
            for (int i = 0; i < 8; i++)
            {
                float dist = i * 4f;
                Vector2 nodePos = minionPos + beamDirection * dist;
                Vector2 vel = beamDirection * (6f + Main.rand.NextFloat(2f))
                    + perpendicular * Main.rand.NextFloat(-0.4f, 0.4f);

                Color beamCol = ClairDeLunePalette.PaletteLerp(
                    ClairDeLunePalette.LunarPhylacterySummon,
                    0.3f + (float)i / 8f * 0.4f);
                Dust d = Dust.NewDustPerfect(nodePos, DustID.IceTorch, vel, 0, default, 0.75f);
                d.noGravity = true;
                d.fadeIn = 0.8f;

                // Pulsing energy nodes every 3rd step
                if (i % 3 == 0)
                {
                    var node = new GenericGlowParticle(nodePos, vel * 0.5f,
                        beamCol * 0.7f, 0.22f, 10, true);
                    MagnumParticleHandler.SpawnParticle(node);
                }
            }

            // Perpendicular soul scatter at muzzle
            for (int i = 0; i < 3; i++)
            {
                float side = (i == 0) ? 1f : (i == 1) ? -1f : 0f;
                Vector2 scatterVel = perpendicular * side * Main.rand.NextFloat(1.5f, 3f) + beamDirection * 0.5f;
                var scatter = new SparkleParticle(minionPos, scatterVel,
                    ClairDeLunePalette.PearlWhite * 0.5f, 0.15f, 10);
                MagnumParticleHandler.SpawnParticle(scatter);
            }

            ClairDeLuneVFXLibrary.SpawnPearlBurst(minionPos, 4, 3f, 0.22f);
            ClairDeLuneVFXLibrary.SpawnMusicNotes(minionPos, 1, 8f, 0.3f, 0.5f, 12);
            ClairDeLuneVFXLibrary.AddPaletteLighting(minionPos, 0.3f, 0.45f);
        }

        /// <summary>
        /// SOUL BEAM IMPACT: Moonlit soul scatter — on hit, captured souls scatter
        /// outward in a soft bloom pattern with pearl-blue trails.
        /// </summary>
        public static void SoulBeamImpactVFX(Vector2 hitPos)
        {
            ClairDeLuneVFXLibrary.ProjectileImpact(hitPos, 0);

            // SIGNATURE: Soul scatter — palette-colored glow particles outward
            int soulCount = 12;
            for (int i = 0; i < soulCount; i++)
            {
                float angle = MathHelper.TwoPi * i / soulCount;
                float speed = 3f + Main.rand.NextFloat() * 2.5f;
                Vector2 vel = angle.ToRotationVector2() * speed;

                Color soulCol = ClairDeLunePalette.PaletteLerp(
                    ClairDeLunePalette.LunarPhylacterySummon, (float)i / soulCount);
                Dust d = Dust.NewDustPerfect(hitPos, DustID.IceTorch, vel, 0, default, 0.8f);
                d.noGravity = true;
                d.fadeIn = 0.9f;

                // Soul glow particles for a ghostly layered effect
                if (i % 2 == 0)
                {
                    var soulGlow = new GenericGlowParticle(hitPos, vel * 0.6f,
                        soulCol * 0.6f, 0.2f, 12, true);
                    MagnumParticleHandler.SpawnParticle(soulGlow);
                }
            }

            ClairDeLuneVFXLibrary.SpawnPearlBurst(hitPos, 6, 3.5f, 0.28f);
            ClairDeLuneVFXLibrary.SpawnMusicNotes(hitPos, 2, 12f, 0.35f, 0.6f, 16);
            ClairDeLuneVFXLibrary.DrawBloom(hitPos, 0.35f, 0.55f);
            ClairDeLuneVFXLibrary.AddPaletteLighting(hitPos, 0.3f, 0.45f);
        }
    }

    // =========================================================================
    //  GearDrivenArbiterVFX — Judgment Mark
    //  Shader: JudgmentMark.fx (JudgmentMarkSigil + JudgmentMarkDetonate)
    //  Identity: CLOCKWORK TRIBUNAL — the arbiter is a mechanical judge
    //  with interlocking brass gear orbits. Its judgment mark creates
    //  a visible rotating gear sigil on targets. Detonation is a cascading
    //  clockwork chain of counter-rotating gear rings expanding outward.
    //  Distinct from Phylactery (ethereal souls) and Fork (harmonic waves).
    // =========================================================================
    public static class GearDrivenArbiterVFX
    {
        private static float _gearPhase = 0f;

        public static void HoldItemVFX(Player player)
        {
            float time = (float)Main.timeForVisualEffects;
            _gearPhase += 0.03f;

            // SIGNATURE: Interlocking gear orbit — 2 counter-rotating dust rings
            if (Main.rand.NextBool(3))
            {
                for (int ring = 0; ring < 2; ring++)
                {
                    float dir = (ring == 0) ? 1f : -1f;
                    float ringAngle = _gearPhase * dir + ring * MathHelper.PiOver4;
                    float ringRadius = 16f + ring * 5f;
                    Vector2 gearPos = player.Center + ringAngle.ToRotationVector2() * ringRadius;

                    Color gearCol = (ring == 0)
                        ? ClairDeLunePalette.ClockworkBrass
                        : ClairDeLunePalette.MoonbeamGold;
                    Dust d = Dust.NewDustPerfect(gearPos, DustID.GoldFlame,
                        Vector2.Zero, 0, default, 0.3f + ring * 0.1f);
                    d.noGravity = true;
                    d.fadeIn = 0.4f;
                }
            }

            if (Main.rand.NextBool(12))
                ClairDeLuneVFXLibrary.SpawnPearlShimmer(player.Center, 1, 20f, 0.15f);

            ClairDeLuneVFXLibrary.AmbientClockworkAura(player.Center, time);
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

        /// <summary>
        /// SUMMON RITUAL: Clockwork assembly — gears spiral inward from the periphery
        /// and interlock at the center, like a mechanism assembling itself from spare parts.
        /// </summary>
        public static void SummonVFX(Vector2 summonPos)
        {
            float time = (float)Main.timeForVisualEffects;

            // SIGNATURE: Spiral gear assembly — gears converge with rotation
            int gearCount = 16;
            for (int i = 0; i < gearCount; i++)
            {
                float spiralAngle = MathHelper.TwoPi * i / gearCount + i * 0.2f;
                float radius = 40f;
                Vector2 gearPos = summonPos + spiralAngle.ToRotationVector2() * radius;
                // Spiral inward with rotation
                Vector2 inwardVel = (summonPos - gearPos) * 0.06f;
                Vector2 tangentVel = new Vector2(-(float)Math.Sin(spiralAngle), (float)Math.Cos(spiralAngle)) * 1.5f;
                Vector2 vel = inwardVel + tangentVel;

                Color gearCol = ClairDeLunePalette.PaletteLerp(
                    ClairDeLunePalette.GearDrivenArbiterSummon, (float)i / gearCount);
                int dustType = (i % 2 == 0) ? DustID.GoldFlame : DustID.IceTorch;
                Dust d = Dust.NewDustPerfect(gearPos, dustType, vel, 0, default, 0.75f);
                d.noGravity = true;
                d.fadeIn = 0.8f;
            }

            ClairDeLuneVFXLibrary.SpawnPearlBurst(summonPos, 6, 3.5f, 0.25f);
            ClairDeLuneVFXLibrary.SpawnGradientHaloRings(summonPos, 3, 0.3f);
            ClairDeLuneVFXLibrary.SpawnMusicNotes(summonPos, 2, 14f, 0.4f, 0.7f, 22);
            ClairDeLuneVFXLibrary.DrawBloom(summonPos, 0.45f, 0.7f);
            ClairDeLuneVFXLibrary.AddPaletteLighting(summonPos, 0.6f, 0.6f);
        }

        /// <summary>
        /// MINION AMBIENT: Clockwork orbit mechanism — the arbiter has visibly rotating
        /// counter-rotating gear dust rings, like a mechanical entity hovering in place.
        /// Different from Phylactery (gentle soul wisps) and Fork (pulsing radial waves).
        /// </summary>
        public static void MinionAmbientVFX(Vector2 minionPos)
        {
            float time = (float)Main.timeForVisualEffects;
            _gearPhase += 0.04f;

            // SIGNATURE: Counter-rotating dual gear rings
            for (int ring = 0; ring < 2; ring++)
            {
                float dir = (ring == 0) ? 1f : -1f;
                float ringAngle = time * 0.04f * dir;
                float ringRadius = 10f + ring * 4f;

                if (Main.rand.NextBool(2))
                {
                    Vector2 ringOffset = ringAngle.ToRotationVector2() * ringRadius;
                    Color gearCol = (ring == 0)
                        ? ClairDeLunePalette.ClockworkBrass
                        : ClairDeLunePalette.MoonbeamGold;
                    Dust d = Dust.NewDustPerfect(minionPos + ringOffset, DustID.GoldFlame,
                        Vector2.Zero, 0, default, 0.22f + ring * 0.05f);
                    d.noGravity = true;
                }
            }

            // Occasional brass sparkle at ring intersections
            if (Main.rand.NextBool(10))
            {
                var sparkle = new SparkleParticle(
                    minionPos + Main.rand.NextVector2Circular(8f, 8f), Vector2.Zero,
                    ClairDeLunePalette.MoonbeamGold * 0.4f, 0.1f, 6);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }

            ClairDeLuneVFXLibrary.AddPaletteLighting(minionPos, 0.6f, 0.2f);
        }

        /// <summary>
        /// JUDGMENT STRIKE: Cascading gear detonation — counter-rotating gear rings
        /// expanding outward in a mechanical chain reaction, with crimson judgment sparks.
        /// </summary>
        public static void JudgmentStrikeVFX(Vector2 strikePos)
        {
            // SIGNATURE: Triple counter-rotating gear ring cascade
            for (int ring = 0; ring < 3; ring++)
            {
                float dir = (ring % 2 == 0) ? 1f : -1f;
                int gearCount = 10 + ring * 4;
                float ringSpeed = 4f + ring * 2f;
                float ringOffset = ring * 0.2f;

                for (int i = 0; i < gearCount; i++)
                {
                    float angle = MathHelper.TwoPi * i / gearCount + ringOffset;
                    // Slight spiral for gear-meshing appearance
                    Vector2 vel = (angle + dir * 0.12f).ToRotationVector2() * ringSpeed;

                    Color gearCol = ClairDeLunePalette.PaletteLerp(
                        ClairDeLunePalette.GearDrivenArbiterSummon, (float)i / gearCount);
                    int dustType = (ring == 1) ? DustID.FireworkFountain_Red : DustID.GoldFlame;
                    Dust d = Dust.NewDustPerfect(strikePos, dustType, vel, 0, default, 1f - ring * 0.1f);
                    d.noGravity = true;
                    d.fadeIn = 1.1f;

                    // Outer ring gets glow particles
                    if (ring == 0 && i % 2 == 0)
                    {
                        var glow = new GenericGlowParticle(strikePos, vel * 0.7f,
                            gearCol * 0.7f, 0.25f, 14, true);
                        MagnumParticleHandler.SpawnParticle(glow);
                    }
                }
            }

            // Crimson judgment flash at center
            var flash = new GenericGlowParticle(strikePos, Vector2.Zero,
                ClairDeLunePalette.TemporalCrimson * 0.8f, 0.5f, 10, true);
            MagnumParticleHandler.SpawnParticle(flash);

            ClairDeLuneVFXLibrary.SpawnRadialDustBurst(strikePos, 14, 6.5f, DustID.GoldFlame);
            ClairDeLuneVFXLibrary.SpawnPearlBurst(strikePos, 8, 4f, 0.35f);
            ClairDeLuneVFXLibrary.SpawnGradientHaloRings(strikePos, 4, 0.4f);
            ClairDeLuneVFXLibrary.SpawnMusicNotes(strikePos, 3, 16f, 0.45f, 0.8f, 25);
            ClairDeLuneVFXLibrary.DrawBloom(strikePos, 0.6f, 0.9f);
            ClairDeLuneVFXLibrary.AddPaletteLighting(strikePos, 0.6f, 0.8f);
        }

        /// <summary>
        /// MARK TARGET: Rotating gear sigil — 4 gear-tooth dust points orbit the target
        /// with a cross-hair pattern, tightening as mark builds. Distinct from
        /// Phylactery (no mark) and Fork (radial wave, not targeted).
        /// </summary>
        public static void MarkTargetVFX(Vector2 targetPos, float markProgress = 0f)
        {
            float intensity = MathHelper.Clamp(markProgress, 0f, 1f);
            float time = (float)Main.timeForVisualEffects;
            float rotSpeed = 0.035f + intensity * 0.025f;

            // SIGNATURE: Rotating crosshair gear teeth — 4 points tightening
            for (int i = 0; i < 4; i++)
            {
                float angle = time * rotSpeed + MathHelper.PiOver2 * i;
                float radius = 22f - 10f * intensity; // Tightens as mark builds
                Vector2 toothPos = targetPos + angle.ToRotationVector2() * radius;

                Color toothCol = ClairDeLunePalette.PaletteLerp(
                    ClairDeLunePalette.GearDrivenArbiterSummon,
                    0.3f + intensity * 0.4f) * (0.4f + intensity * 0.5f);
                Dust d = Dust.NewDustPerfect(toothPos, DustID.GoldFlame,
                    Vector2.Zero, 0, default, 0.35f * (0.5f + intensity * 0.5f));
                d.noGravity = true;

                // At high charge, connecting lines between teeth (glow particles)
                if (intensity > 0.5f && i % 2 == 0)
                {
                    float nextAngle = time * rotSpeed + MathHelper.PiOver2 * (i + 1);
                    Vector2 nextPos = targetPos + nextAngle.ToRotationVector2() * radius;
                    Vector2 midpoint = (toothPos + nextPos) * 0.5f;
                    var link = new SparkleParticle(midpoint, Vector2.Zero,
                        ClairDeLunePalette.MoonbeamGold * 0.3f * intensity, 0.08f, 4);
                    MagnumParticleHandler.SpawnParticle(link);
                }
            }

            // Central mark glow intensifies
            if (intensity > 0.3f && Main.rand.NextBool(3))
            {
                var centerGlow = new GenericGlowParticle(targetPos, Vector2.Zero,
                    ClairDeLunePalette.ClockworkBrass * 0.2f * intensity, 0.12f, 6, true);
                MagnumParticleHandler.SpawnParticle(centerGlow);
            }

            ClairDeLuneVFXLibrary.AddPaletteLighting(targetPos, 0.6f, 0.2f * intensity);
        }
    }

    // =========================================================================
    //  AutomatonsTuningForkVFX — Resonance Field
    //  Shader: ResonanceField.fx (ResonanceFieldPulse + ResonanceFieldHarmonic)
    //  Identity: HARMONIC STANDING WAVE — the tuning fork emits visible concentric
    //  wave rings that pulse outward at regular intervals, like sound waves
    //  made visible. Music notes ride the crests of the wave peaks, creating
    //  a rhythmic visual heartbeat. The ONLY weapon with regular periodic pulses.
    //  Distinct from Phylactery (drifting souls) and Arbiter (gear mechanism).
    // =========================================================================
    public static class AutomatonsTuningForkVFX
    {
        private static float _pulseTimer = 0f;
        private static float _wavePhase = 0f;

        public static void HoldItemVFX(Player player)
        {
            float time = (float)Main.timeForVisualEffects;
            _pulseTimer += 1f;

            // SIGNATURE: Rhythmic concentric wave pulses — visible standing waves
            float pulseCycle = (float)Math.Sin(_pulseTimer * 0.05f);
            if (Main.rand.NextBool(3))
            {
                float waveRadius = 15f + pulseCycle * 5f;
                float ringAngle = Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 wavePos = player.Center + ringAngle.ToRotationVector2() * waveRadius;
                Vector2 outwardVel = (wavePos - player.Center).SafeNormalize(Vector2.Zero) * 0.3f;

                Color waveCol = ClairDeLunePalette.PaletteLerp(
                    ClairDeLunePalette.AutomatonTuningForkSummon,
                    0.3f + pulseCycle * 0.15f);
                Dust d = Dust.NewDustPerfect(wavePos, DustID.IceTorch, outwardVel, 0, default, 0.3f);
                d.noGravity = true;
                d.fadeIn = 0.5f;
            }

            // Beat indicator — gentle moonlit mist on pulse peaks
            if (pulseCycle > 0.8f && Main.rand.NextBool(4))
                ClairDeLuneVFXLibrary.SpawnMoonlitMist(player.Center, 1, 18f, 0.1f);

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

        /// <summary>
        /// SUMMON RITUAL: Harmonic convergence — concentric rings contract inward
        /// at musical intervals, like sound waves focusing to a point.
        /// </summary>
        public static void SummonVFX(Vector2 summonPos)
        {
            // SIGNATURE: 3 contracting concentric rings — each at different radius
            for (int ring = 0; ring < 3; ring++)
            {
                int ringCount = 10 + ring * 2;
                float ringRadius = 30f + ring * 15f;
                for (int i = 0; i < ringCount; i++)
                {
                    float angle = MathHelper.TwoPi * i / ringCount;
                    Vector2 ringPos = summonPos + angle.ToRotationVector2() * ringRadius;
                    Vector2 inwardVel = (summonPos - ringPos) * (0.04f + ring * 0.01f);

                    Color waveCol = ClairDeLunePalette.PaletteLerp(
                        ClairDeLunePalette.AutomatonTuningForkSummon, (float)ring / 3f);
                    Dust d = Dust.NewDustPerfect(ringPos, DustID.IceTorch, inwardVel,
                        0, default, 0.7f - ring * 0.1f);
                    d.noGravity = true;
                    d.fadeIn = 0.8f;
                }
            }

            ClairDeLuneVFXLibrary.SpawnPearlBurst(summonPos, 6, 3.5f, 0.25f);
            ClairDeLuneVFXLibrary.SpawnMusicNotes(summonPos, 3, 14f, 0.4f, 0.8f, 22);
            ClairDeLuneVFXLibrary.DrawBloom(summonPos, 0.4f, 0.7f);
            ClairDeLuneVFXLibrary.AddPaletteLighting(summonPos, 0.3f, 0.6f);
        }

        /// <summary>
        /// MINION AMBIENT: Rhythmic wave pulsation — the tuning fork minion has
        /// a visible pulsating radius that expands and contracts at regular intervals,
        /// like a heartbeat. Particles concentrate at the wave crest.
        /// Different from Phylactery (orbiting wisps) and Arbiter (gear rings).
        /// </summary>
        public static void MinionAmbientVFX(Vector2 minionPos)
        {
            float time = (float)Main.timeForVisualEffects;
            _wavePhase += 0.04f;

            // SIGNATURE: Pulsating wave crest — radius oscillates, particles ride the crest
            float pulse = (float)Math.Sin(_wavePhase) * 0.5f + 0.5f;
            float waveRadius = 8f + 8f * pulse;

            if (Main.rand.NextBool(2))
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 crestPos = minionPos + angle.ToRotationVector2() * waveRadius;
                Vector2 outVel = (crestPos - minionPos).SafeNormalize(Vector2.Zero) * pulse * 0.5f;

                Color crestCol = ClairDeLunePalette.PaletteLerp(
                    ClairDeLunePalette.AutomatonTuningForkSummon,
                    0.3f + pulse * 0.3f) * (0.3f + pulse * 0.3f);
                Dust d = Dust.NewDustPerfect(crestPos, DustID.IceTorch,
                    outVel, 0, default, 0.2f + pulse * 0.15f);
                d.noGravity = true;
            }

            // Peak pulse: brief music note at wave crest
            if (pulse > 0.85f && Main.rand.NextBool(6))
            {
                var note = new HueShiftingMusicNoteParticle(
                    minionPos + Main.rand.NextVector2Circular(waveRadius, waveRadius),
                    Vector2.UnitY * -0.5f, 0.55f, 0.65f, 0.5f, 0.7f, 0.35f, 18);
                MagnumParticleHandler.SpawnParticle(note);
            }

            ClairDeLuneVFXLibrary.AddPaletteLighting(minionPos, 0.3f, 0.15f + pulse * 0.1f);
        }

        /// <summary>
        /// RESONANCE PULSE: Expanding harmonic wave — concentric frequency bands
        /// radiate outward at different speeds (overtones). Music notes ride
        /// the crests of each wave. The visual signature of this weapon: rhythmic
        /// circular shockwaves with embedded musical motifs.
        /// </summary>
        public static void ResonancePulseVFX(Vector2 pulseCenter, float pulseRadius = 120f)
        {
            // SIGNATURE: 4 frequency bands expanding outward — fundamental + 3 overtones
            float[] bandSpeeds = { 1f, 1.5f, 2f, 3f };
            float[] bandRadii = { pulseRadius, pulseRadius * 0.75f, pulseRadius * 0.5f, pulseRadius * 0.3f };
            Color[] bandColors = {
                ClairDeLunePalette.SoftBlue,
                ClairDeLunePalette.PearlBlue,
                ClairDeLunePalette.PearlShimmer,
                ClairDeLunePalette.PearlWhite
            };

            for (int band = 0; band < 4; band++)
            {
                int waveCount = 14 + band * 4;
                float radius = bandRadii[band];
                for (int i = 0; i < waveCount; i++)
                {
                    float angle = MathHelper.TwoPi * i / waveCount + band * 0.15f;
                    Vector2 ringPos = pulseCenter + angle.ToRotationVector2() * radius;
                    Vector2 outVel = (ringPos - pulseCenter).SafeNormalize(Vector2.Zero) * (2f * bandSpeeds[band]);

                    Dust d = Dust.NewDustPerfect(ringPos, DustID.IceTorch, outVel,
                        0, default, 0.85f - band * 0.1f);
                    d.noGravity = true;
                    d.fadeIn = 1f;

                    // Glow particles on fundamental band
                    if (band == 0 && i % 2 == 0)
                    {
                        var glow = new GenericGlowParticle(ringPos, outVel * 0.5f,
                            bandColors[band] * 0.6f, 0.25f, 14, true);
                        MagnumParticleHandler.SpawnParticle(glow);
                    }
                }
            }

            // Music notes riding the wave crests
            for (int i = 0; i < 5; i++)
            {
                float noteAngle = Main.rand.NextFloat(MathHelper.TwoPi);
                float noteRadius = pulseRadius * Main.rand.NextFloat(0.3f, 0.8f);
                Vector2 notePos = pulseCenter + noteAngle.ToRotationVector2() * noteRadius;
                Vector2 noteVel = (notePos - pulseCenter).SafeNormalize(Vector2.Zero) * 2f + Vector2.UnitY * -1.5f;
                var note = new HueShiftingMusicNoteParticle(
                    notePos, noteVel, 0.55f, 0.65f, 0.5f, 0.7f,
                    Main.rand.NextFloat(0.5f, 0.8f), 25);
                MagnumParticleHandler.SpawnParticle(note);
            }

            ClairDeLuneVFXLibrary.SpawnOrbitingNotes(pulseCenter, Vector2.Zero, 4, 40f);
            ClairDeLuneVFXLibrary.SpawnPearlBurst(pulseCenter, 10, 5f, 0.35f);
            ClairDeLuneVFXLibrary.SpawnGradientHaloRings(pulseCenter, 5, 0.45f);
            ClairDeLuneVFXLibrary.DrawBloom(pulseCenter, 0.65f, 0.9f);
            ClairDeLuneVFXLibrary.AddPaletteLighting(pulseCenter, 0.3f, 0.85f);
        }
    }
}