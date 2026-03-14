using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Bosses;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Content.SwanLake.Bosses.Systems;

namespace MagnumOpus.Content.SwanLake.Bosses
{
    /// <summary>
    /// VFX helper for The Monochromatic Fractal boss — Swan Lake's dying grace.
    /// 
    /// Phase design philosophy (Tchaikovsky's tragedy of beauty destroyed):
    ///   Phase 1 — White Swan (100-60%): Pure geometric elegance. Clean white arcs,
    ///             deliberate ballet choreography. Stark white-on-black. Every movement held.
    ///   Phase 2 — Black Swan Emergence (60-45%): Pristine white fractures. Black mirror-particles
    ///             shadow every white one. Prismatic rainbow bleeds at duality seams.
    ///   Phase 3 — Duality War (45-30%): White and black spiral in double helix. Rapid alternation.
    ///             Expanding rings of alternating monochrome on every impact.
    ///   Enrage  — Death of the Swan (30-0%): All color drains. Desperate prismatic rainbow flashes
    ///             only at destruction points. Feathers fall slow-motion. Heartbreaking violence.
    /// 
    /// Palette: pure white, void black, prismatic rainbow edges ONLY at moments of destruction.
    /// </summary>
    public static class MonochromaticFractalVFX
    {
        // Phase-specific pure colors — no prismatic except at destruction seams
        private static readonly Color VoidBlack = new Color(5, 5, 8);
        private static readonly Color PureWhite = new Color(245, 245, 255);
        private static readonly Color SilverMist = new Color(180, 185, 200);
        private static readonly Color GhostWhite = new Color(230, 230, 240);

        /// <summary>
        /// Gets a brief prismatic flash color — used ONLY at points of fracture/destruction.
        /// </summary>
        private static Color GetFractureRainbow(float t)
        {
            float hue = (t + (float)Main.timeForVisualEffects * 0.008f) % 1f;
            return Main.hslToRgb(hue, 0.9f, 0.85f);
        }

        // =====================================================================
        //  PHASE 1 — WHITE SWAN: Pure Geometric Elegance
        // =====================================================================

        /// <summary>
        /// Phase 1 ambient aura: pristine white particles with geometric precision.
        /// Clean, measured, ballet-like. White motes drift in deliberate arcs.
        /// No rainbow, no chaos — pure controlled grace on void black.
        /// </summary>
        public static void WhiteSwanAura(Vector2 center, float hpRatio)
        {
            if (Main.dedServ) return;

            float time = (float)Main.timeForVisualEffects;

            // Precisely orbiting white motes — ballet positions, not random scatter
            if ((int)time % 8 == 0)
            {
                int moteCount = 6;
                for (int i = 0; i < moteCount; i++)
                {
                    float angle = time * 0.015f + MathHelper.TwoPi * i / moteCount;
                    float radius = 55f + (float)Math.Sin(time * 0.01f + i) * 10f;
                    Vector2 motePos = center + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * radius;
                    Vector2 vel = (motePos - center).SafeNormalize(Vector2.Zero).RotatedBy(MathHelper.PiOver2) * 0.3f;

                    var glow = new GenericGlowParticle(motePos, vel, PureWhite * 0.7f, 0.15f, 30, true);
                    MagnumParticleHandler.SpawnParticle(glow);
                }
            }

            // Slow feather drift — singular white feathers descending with purpose
            if (Main.rand.NextBool(12))
            {
                Vector2 featherPos = center + new Vector2(Main.rand.NextFloat(-40f, 40f), -30f);
                Vector2 featherVel = new Vector2(
                    (float)Math.Sin(time * 0.008f + featherPos.X * 0.01f) * 0.3f,
                    Main.rand.NextFloat(0.2f, 0.5f));
                Dust d = Dust.NewDustPerfect(featherPos, DustID.WhiteTorch, featherVel, 0, PureWhite, 0.6f);
                d.noGravity = true;
                d.fadeIn = 1.2f;
            }

            // Geometric light pulse — expanding then contracting ring of white
            float pulse = (float)Math.Sin(time * 0.02f) * 0.5f + 0.5f;
            Lighting.AddLight(center, PureWhite.ToVector3() * (0.5f + pulse * 0.3f));
        }

        /// <summary>
        /// Phase 1 attack arc — clean white arc traced with geometric precision.
        /// Each arc lingers for exactly the right number of frames, like a dance position held.
        /// </summary>
        public static void WhiteSwanAttackArc(Vector2 origin, Vector2 direction, float length)
        {
            if (Main.dedServ) return;

            int arcPoints = 12;
            for (int i = 0; i < arcPoints; i++)
            {
                float t = i / (float)arcPoints;
                Vector2 pos = origin + direction * length * t;
                var glow = new GenericGlowParticle(pos, Vector2.Zero, PureWhite * (1f - t * 0.5f), 0.12f + (1f - t) * 0.08f, 24, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }

            // Terminal bloom at arc endpoint
            Vector2 endpoint = origin + direction * length;
            var bloom = new BloomParticle(endpoint, Vector2.Zero, PureWhite, 0.35f, 18);
            MagnumParticleHandler.SpawnParticle(bloom);

            Lighting.AddLight(origin, PureWhite.ToVector3() * 0.8f);
        }

        /// <summary>
        /// Phase 1 impact — controlled white expansion. No chaos, just precise radial bloom.
        /// </summary>
        public static void WhiteSwanImpact(Vector2 pos, float intensity = 1f)
        {
            if (Main.dedServ) return;

            // Single expanding white ring — geometric perfection
            int ringPoints = 16;
            for (int i = 0; i < ringPoints; i++)
            {
                float angle = MathHelper.TwoPi * i / ringPoints;
                Vector2 vel = angle.ToRotationVector2() * 3f * intensity;
                var glow = new GenericGlowParticle(pos, vel, PureWhite, 0.18f * intensity, 20, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }

            // Central bloom flash
            var bloom = new BloomParticle(pos, Vector2.Zero, PureWhite, 0.4f * intensity, 14);
            MagnumParticleHandler.SpawnParticle(bloom);

            // 2-3 feathers released on impact — gentle, deliberate
            for (int i = 0; i < 3; i++)
            {
                Vector2 vel = new Vector2(Main.rand.NextFloat(-1.5f, 1.5f), Main.rand.NextFloat(-2f, -0.5f));
                Dust d = Dust.NewDustPerfect(pos, DustID.WhiteTorch, vel, 0, PureWhite, 0.8f);
                d.noGravity = true;
                d.fadeIn = 1.3f;
            }

            Lighting.AddLight(pos, PureWhite.ToVector3() * 1.2f * intensity);
        }

        // =====================================================================
        //  PHASE 2 — BLACK SWAN EMERGENCE: The Pristine White Fractures
        // =====================================================================

        /// <summary>
        /// Phase 2 ambient aura: every white mote now has a black mirror twin.
        /// Prismatic rainbow bleeds at the seams where black meets white.
        /// The duality emerges — Odile shadows Odette.
        /// </summary>
        public static void BlackSwanAura(Vector2 center, float hpRatio)
        {
            if (Main.dedServ) return;

            float time = (float)Main.timeForVisualEffects;
            float dualityStrength = MathHelper.Clamp((0.6f - hpRatio) / 0.15f, 0f, 1f);

            // Paired dual motes — white orbits clockwise, black counterclockwise
            if ((int)time % 6 == 0)
            {
                int moteCount = 6;
                for (int i = 0; i < moteCount; i++)
                {
                    float baseAngle = MathHelper.TwoPi * i / moteCount;
                    float radius = 50f + (float)Math.Sin(time * 0.012f + i) * 12f;

                    // White mote — clockwise
                    float whiteAngle = baseAngle + time * 0.018f;
                    Vector2 whitePos = center + new Vector2((float)Math.Cos(whiteAngle), (float)Math.Sin(whiteAngle)) * radius;
                    var whiteGlow = new GenericGlowParticle(whitePos, Vector2.Zero, PureWhite * 0.6f, 0.14f, 26, true);
                    MagnumParticleHandler.SpawnParticle(whiteGlow);

                    // Black mirror mote — counterclockwise, mirrored position
                    float blackAngle = -baseAngle - time * 0.018f;
                    Vector2 blackPos = center + new Vector2((float)Math.Cos(blackAngle), (float)Math.Sin(blackAngle)) * radius;
                    var blackGlow = new GenericGlowParticle(blackPos, Vector2.Zero, VoidBlack * 0.8f, 0.14f, 26, false);
                    MagnumParticleHandler.SpawnParticle(blackGlow);

                    // Prismatic bleed at the seam between paired motes
                    if (dualityStrength > 0.3f)
                    {
                        Vector2 seamPos = Vector2.Lerp(whitePos, blackPos, 0.5f);
                        Color seamColor = GetFractureRainbow(i / (float)moteCount) * dualityStrength * 0.4f;
                        var seamGlow = new GenericGlowParticle(seamPos, Vector2.Zero, seamColor, 0.08f, 16, true);
                        MagnumParticleHandler.SpawnParticle(seamGlow);
                    }
                }
            }

            // Paired feather drift — white down, black rising
            if (Main.rand.NextBool(8))
            {
                // White feather falls
                Vector2 whiteFeatherPos = center + new Vector2(Main.rand.NextFloat(-35f, 35f), -25f);
                Dust wd = Dust.NewDustPerfect(whiteFeatherPos, DustID.WhiteTorch,
                    new Vector2(Main.rand.NextFloat(-0.3f, 0.3f), 0.4f), 0, PureWhite, 0.5f);
                wd.noGravity = true;

                // Black feather rises — the mirror
                Vector2 blackFeatherPos = center + new Vector2(Main.rand.NextFloat(-35f, 35f), 25f);
                Dust bd = Dust.NewDustPerfect(blackFeatherPos, DustID.Shadowflame,
                    new Vector2(Main.rand.NextFloat(-0.3f, 0.3f), -0.4f), 150, VoidBlack, 0.5f);
                bd.noGravity = true;
            }

            SwanLakeVFXLibrary.AddDualPolarityLight(center, time, 0.6f);
        }

        /// <summary>
        /// Phase 2 paired attack — white arc followed by inverted black arc.
        /// Rainbow prismatic shimmer bleeds at the seam between them.
        /// </summary>
        public static void DualArcAttack(Vector2 origin, Vector2 direction, float length)
        {
            if (Main.dedServ) return;

            // White arc first
            int arcPoints = 10;
            for (int i = 0; i < arcPoints; i++)
            {
                float t = i / (float)arcPoints;
                Vector2 pos = origin + direction * length * t;
                var whiteGlow = new GenericGlowParticle(pos, Vector2.Zero, PureWhite * (1f - t * 0.4f), 0.12f, 20, true);
                MagnumParticleHandler.SpawnParticle(whiteGlow);
            }

            // Black arc — inverted direction with slight offset
            Vector2 invertDir = -direction.RotatedBy(0.15f);
            for (int i = 0; i < arcPoints; i++)
            {
                float t = i / (float)arcPoints;
                Vector2 pos = origin + invertDir * length * t;
                Dust d = Dust.NewDustPerfect(pos, DustID.Shadowflame,
                    Vector2.Zero, 120, VoidBlack * (1f - t * 0.3f), 0.7f);
                d.noGravity = true;
            }

            // Rainbow bleed at the origin where arcs diverge
            for (int i = 0; i < 5; i++)
            {
                Color seamColor = GetFractureRainbow(i / 5f);
                Vector2 seamPos = origin + Main.rand.NextVector2Circular(8f, 8f);
                var seamGlow = new GenericGlowParticle(seamPos, Main.rand.NextVector2Circular(1f, 1f), seamColor * 0.5f, 0.1f, 14, true);
                MagnumParticleHandler.SpawnParticle(seamGlow);
            }

            Lighting.AddLight(origin, PureWhite.ToVector3() * 0.6f);
        }

        /// <summary>
        /// Phase 2 impact — dual-polarity burst. White expands, black contracts.
        /// Prismatic seam where they meet.
        /// </summary>
        public static void BlackSwanImpact(Vector2 pos, float intensity = 1f)
        {
            if (Main.dedServ) return;

            int ringPoints = 14;
            for (int i = 0; i < ringPoints; i++)
            {
                float angle = MathHelper.TwoPi * i / ringPoints;

                // White expanding ring
                Vector2 whiteVel = angle.ToRotationVector2() * 4f * intensity;
                var whiteGlow = new GenericGlowParticle(pos, whiteVel, PureWhite * 0.8f, 0.16f * intensity, 18, true);
                MagnumParticleHandler.SpawnParticle(whiteGlow);

                // Black contracting ring (starts far, moves inward)
                Vector2 blackStart = pos + angle.ToRotationVector2() * 40f * intensity;
                Vector2 blackVel = -angle.ToRotationVector2() * 2.5f * intensity;
                Dust bd = Dust.NewDustPerfect(blackStart, DustID.Shadowflame, blackVel, 120, VoidBlack, 0.8f * intensity);
                bd.noGravity = true;
            }

            // Prismatic seam ring at the collision boundary
            for (int i = 0; i < 8; i++)
            {
                float seamAngle = MathHelper.TwoPi * i / 8f;
                Vector2 seamPos = pos + seamAngle.ToRotationVector2() * 20f * intensity;
                Color rainbow = GetFractureRainbow(i / 8f);
                var seamBloom = new BloomParticle(seamPos, Vector2.Zero, rainbow * 0.6f, 0.12f * intensity, 10);
                MagnumParticleHandler.SpawnParticle(seamBloom);
            }

            var whiteBurst = new BloomParticle(pos, Vector2.Zero, PureWhite, 0.4f * intensity, 8);
            MagnumParticleHandler.SpawnParticle(whiteBurst);

            Lighting.AddLight(pos, PureWhite.ToVector3() * intensity);
        }

        // =====================================================================
        //  PHASE 3 — DUALITY WAR: The Double Helix of Destruction
        // =====================================================================

        /// <summary>
        /// Phase 3 ambient: white and black feathers spiral in a double helix around the boss.
        /// Attacks alternate rapidly. The fractal sky rotates and fragments outside.
        /// </summary>
        public static void DualityWarAura(Vector2 center, float hpRatio)
        {
            if (Main.dedServ) return;

            float time = (float)Main.timeForVisualEffects;
            float warIntensity = MathHelper.Clamp((0.45f - hpRatio) / 0.15f, 0f, 1f);

            // Double helix — white and black spiraling in opposite directions
            if ((int)time % 4 == 0)
            {
                int helixPoints = 8;
                float helixSpeed = time * 0.03f;
                float helixRadius = 45f + warIntensity * 15f;

                for (int i = 0; i < helixPoints; i++)
                {
                    float t = i / (float)helixPoints;

                    // White strand of the helix
                    float whiteAngle = helixSpeed + t * MathHelper.TwoPi * 2f;
                    float yOffset = (t - 0.5f) * 80f;
                    Vector2 whitePos = center + new Vector2(
                        (float)Math.Cos(whiteAngle) * helixRadius,
                        yOffset + (float)Math.Sin(whiteAngle) * 15f);
                    var whiteGlow = new GenericGlowParticle(whitePos, Vector2.Zero, PureWhite * 0.7f, 0.12f, 14, true);
                    MagnumParticleHandler.SpawnParticle(whiteGlow);

                    // Black strand — phase-shifted 180 degrees
                    float blackAngle = helixSpeed + t * MathHelper.TwoPi * 2f + MathHelper.Pi;
                    Vector2 blackPos = center + new Vector2(
                        (float)Math.Cos(blackAngle) * helixRadius,
                        yOffset + (float)Math.Sin(blackAngle) * 15f);
                    Dust bd = Dust.NewDustPerfect(blackPos, DustID.Shadowflame, Vector2.Zero, 130, VoidBlack, 0.6f);
                    bd.noGravity = true;
                }
            }

            // Rapid feather alternation — white and black rapidly spawning
            if (Main.rand.NextBool(3))
            {
                bool isWhite = (int)(time * 0.1f) % 2 == 0;
                Vector2 featherPos = center + Main.rand.NextVector2Circular(50f, 50f);
                Vector2 featherVel = (featherPos - center).SafeNormalize(Vector2.Zero) * 1.5f;

                if (isWhite)
                {
                    Dust d = Dust.NewDustPerfect(featherPos, DustID.WhiteTorch, featherVel, 0, PureWhite, 0.7f);
                    d.noGravity = true;
                }
                else
                {
                    Dust d = Dust.NewDustPerfect(featherPos, DustID.Shadowflame, featherVel, 140, VoidBlack, 0.7f);
                    d.noGravity = true;
                }
            }

            float dualPulse = (float)Math.Sin(time * 0.06f);
            float lightVal = 0.4f + dualPulse * 0.3f;
            Lighting.AddLight(center, new Vector3(lightVal, lightVal, lightVal + 0.1f));
        }

        /// <summary>
        /// Phase 3 impact — expanding rings of alternating monochrome.
        /// Each ring alternates white/black. Brief prismatic flash at center.
        /// </summary>
        public static void DualityWarImpact(Vector2 pos, float intensity = 1f)
        {
            if (Main.dedServ) return;

            // Three concentric rings — alternating white, black, white
            for (int ring = 0; ring < 3; ring++)
            {
                bool isWhiteRing = ring % 2 == 0;
                int pointCount = 12 + ring * 4;
                float ringSpeed = (3f + ring * 1.5f) * intensity;

                for (int i = 0; i < pointCount; i++)
                {
                    float angle = MathHelper.TwoPi * i / pointCount;
                    Vector2 vel = angle.ToRotationVector2() * ringSpeed;

                    if (isWhiteRing)
                    {
                        var glow = new GenericGlowParticle(pos, vel, PureWhite * 0.8f, 0.14f * intensity, 16, true);
                        MagnumParticleHandler.SpawnParticle(glow);
                    }
                    else
                    {
                        Dust d = Dust.NewDustPerfect(pos, DustID.Shadowflame, vel, 130, VoidBlack, 0.9f * intensity);
                        d.noGravity = true;
                    }
                }
            }

            // Prismatic flash — brief, only at the point of destruction
            var prismFlash = new BloomParticle(pos, Vector2.Zero, GetFractureRainbow(0f), 0.3f * intensity, 6);
            MagnumParticleHandler.SpawnParticle(prismFlash);

            MagnumScreenEffects.AddScreenShake(4f * intensity);
            Lighting.AddLight(pos, PureWhite.ToVector3() * 1.4f * intensity);
        }

        // =====================================================================
        //  ENRAGE — DEATH OF THE SWAN: Heartbreaking Violence
        // =====================================================================

        /// <summary>
        /// Enrage ambient: all color drains. Feathers fall in slow-motion.
        /// The world becomes grayscale with only desperate prismatic flashes at destruction.
        /// Beauty becoming heartbreakingly violent.
        /// </summary>
        public static void DyingSwanAura(Vector2 center, float hpRatio)
        {
            if (Main.dedServ) return;

            float time = (float)Main.timeForVisualEffects;

            // Slow-motion feathers — drained of color, drifting down with tragic grace
            if (Main.rand.NextBool(4))
            {
                Vector2 featherPos = center + new Vector2(Main.rand.NextFloat(-60f, 60f), Main.rand.NextFloat(-40f, -20f));
                Vector2 featherVel = new Vector2(
                    (float)Math.Sin(time * 0.005f + featherPos.X * 0.02f) * 0.15f,
                    Main.rand.NextFloat(0.1f, 0.25f));

                Color drainedColor = Color.Lerp(SilverMist, GhostWhite, Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(featherPos, DustID.SilverCoin, featherVel, 80, drainedColor, 0.5f);
                d.noGravity = true;
                d.fadeIn = 1.5f;
            }

            // Faint ascending wisps — the swan's life force escaping
            if (Main.rand.NextBool(8))
            {
                Vector2 wispPos = center + Main.rand.NextVector2Circular(30f, 30f);
                Vector2 wispVel = new Vector2(0f, Main.rand.NextFloat(-0.8f, -0.3f));
                var wisp = new SparkleParticle(wispPos, wispVel, GhostWhite * 0.4f, 0.1f, 30);
                MagnumParticleHandler.SpawnParticle(wisp);
            }

            float dimPulse = 0.3f + (float)Math.Sin(time * 0.015f) * 0.1f;
            Lighting.AddLight(center, new Vector3(dimPulse, dimPulse, dimPulse));
        }

        /// <summary>
        /// Enrage impact — the only place color survives. Desperate prismatic rainbow
        /// flash at the point of impact, surrounded by draining monochrome.
        /// </summary>
        public static void DyingSwanImpact(Vector2 pos, float intensity = 1f)
        {
            if (Main.dedServ) return;

            // Draining monochrome ring — gray, lifeless
            int ringPoints = 16;
            for (int i = 0; i < ringPoints; i++)
            {
                float angle = MathHelper.TwoPi * i / ringPoints;
                Vector2 vel = angle.ToRotationVector2() * 2.5f * intensity;
                Color drained = Color.Lerp(SilverMist, GhostWhite, i / (float)ringPoints) * 0.6f;
                var glow = new GenericGlowParticle(pos, vel, drained, 0.12f * intensity, 22, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }

            // DESPERATE prismatic flash — the only color that survives
            for (int i = 0; i < 6; i++)
            {
                float hueT = i / 6f;
                Color rainbow = GetFractureRainbow(hueT);
                Vector2 prismVel = Main.rand.NextVector2Circular(2f, 2f);
                var prismBurst = new BloomParticle(pos + Main.rand.NextVector2Circular(5f, 5f), prismVel, rainbow, 0.25f * intensity, 8);
                MagnumParticleHandler.SpawnParticle(prismBurst);
            }

            var deathFlash = new BloomParticle(pos, Vector2.Zero, PureWhite * 0.8f, 0.35f * intensity, 10);
            MagnumParticleHandler.SpawnParticle(deathFlash);

            // 1-2 slow feathers released — even impacts feel like mourning
            for (int i = 0; i < 2; i++)
            {
                Vector2 featherVel = new Vector2(Main.rand.NextFloat(-0.8f, 0.8f), Main.rand.NextFloat(-1f, -0.3f));
                Dust d = Dust.NewDustPerfect(pos, DustID.SilverCoin, featherVel, 100, GhostWhite, 0.5f);
                d.noGravity = true;
                d.fadeIn = 1.4f;
            }

            MagnumScreenEffects.AddScreenShake(3f * intensity);
            Lighting.AddLight(pos, PureWhite.ToVector3() * 0.8f * intensity);
        }

        // =====================================================================
        //  UNIFIED AMBIENT AURA — Dispatches to phase-specific aura
        // =====================================================================

        /// <summary>
        /// Per-frame boss ambient aura dispatcher. Routes to phase-specific VFX
        /// based on HP ratio (smoother than mood enum for visual transitions).
        /// Call every frame during the boss fight.
        /// </summary>
        public static void AmbientAura(Vector2 center, float hpRatio = 1f)
        {
            if (Main.dedServ) return;

            if (hpRatio > 0.6f)
                WhiteSwanAura(center, hpRatio);
            else if (hpRatio > 0.45f)
                BlackSwanAura(center, hpRatio);
            else if (hpRatio > 0.3f)
                DualityWarAura(center, hpRatio);
            else
                DyingSwanAura(center, hpRatio);

            // Music notes — sparse, appropriate to phase
            if (Main.rand.NextBool(hpRatio < 0.3f ? 20 : 12))
                SwanLakeVFXLibrary.SpawnMusicNotes(center, 1, 40f, hpRatio < 0.3f ? 0.4f : 0.7f, 0.8f, 30);
        }

        // =====================================================================
        //  PHASE TRANSITION — Dramatic duality shift
        // =====================================================================

        /// <summary>
        /// Phase transition VFX: the pristine fractures.
        /// White cracks with black bleeding through, prismatic at the seams.
        /// </summary>
        public static void PhaseTransition(Vector2 pos, bool toBlackPhase)
        {
            if (Main.dedServ) return;

            SwanLakeSkySystem.TriggerPrismaticFlash(12f);
            MagnumScreenEffects.AddScreenShake(toBlackPhase ? 14f : 10f);

            if (toBlackPhase)
            {
                // White shattering — cracks radiating outward
                for (int i = 0; i < 24; i++)
                {
                    float angle = MathHelper.TwoPi * i / 24f;
                    float speed = Main.rand.NextFloat(4f, 10f);
                    Vector2 vel = angle.ToRotationVector2() * speed;

                    Dust wd = Dust.NewDustPerfect(pos, DustID.WhiteTorch, vel, 0, PureWhite, 1.8f);
                    wd.noGravity = true;
                    wd.fadeIn = 1.3f;

                    Vector2 blackVel = vel * 0.7f;
                    Dust bd = Dust.NewDustPerfect(pos, DustID.Shadowflame, blackVel, 140, VoidBlack, 1.5f);
                    bd.noGravity = true;
                }

                // Prismatic seam at the fracture point
                for (int i = 0; i < 8; i++)
                {
                    Color rainbow = GetFractureRainbow(i / 8f);
                    Vector2 seamPos = pos + Main.rand.NextVector2Circular(15f, 15f);
                    Vector2 seamVel = Main.rand.NextVector2Circular(3f, 3f);
                    var seamBurst = new BloomParticle(seamPos, seamVel, rainbow * 0.7f, 0.2f, 12);
                    MagnumParticleHandler.SpawnParticle(seamBurst);
                }
            }
            else
            {
                // Transition to Dying Swan — color draining away
                for (int i = 0; i < 20; i++)
                {
                    float angle = MathHelper.TwoPi * i / 20f;
                    Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 7f);
                    Color draining = Color.Lerp(PureWhite, SilverMist, Main.rand.NextFloat());
                    Dust d = Dust.NewDustPerfect(pos, DustID.SilverCoin, vel, 80, draining, 1.2f);
                    d.noGravity = true;
                    d.fadeIn = 1.4f;
                }

                for (int i = 0; i < 6; i++)
                {
                    Color rainbow = GetFractureRainbow(i / 6f);
                    var flare = new BloomParticle(pos, Main.rand.NextVector2Circular(2f, 2f), rainbow * 0.5f, 0.3f, 10);
                    MagnumParticleHandler.SpawnParticle(flare);
                }
            }

            SwanLakeVFXLibrary.SpawnFeatherBurst(pos, toBlackPhase ? 10 : 6, 0.35f);
            SwanLakeVFXLibrary.SpawnMusicNotes(pos, 5, 50f, 0.8f, 1.1f, 35);
            Lighting.AddLight(pos, PureWhite.ToVector3() * 1.8f);
        }

        // =====================================================================
        //  SWAN SERENADE ATTACK — Phase-aware feather barrage
        // =====================================================================

        /// <summary>SwanSerenade feather launch VFX.</summary>
        public static void SwanSerenadeFeatherLaunch(Vector2 launchPos, Vector2 direction)
        {
            if (Main.dedServ) return;

            try { CustomParticles.GenericFlare(launchPos, PureWhite, 0.35f, 10); } catch { }

            for (int i = 0; i < 3; i++)
            {
                float spreadAngle = Main.rand.NextFloat(-0.15f, 0.15f);
                Vector2 vel = direction.RotatedBy(spreadAngle) * Main.rand.NextFloat(2f, 4f);
                Dust d = Dust.NewDustPerfect(launchPos, DustID.WhiteTorch, vel, 0, PureWhite, 1.0f);
                d.noGravity = true;
            }

            SwanLakeVFXLibrary.SpawnMusicNotes(launchPos, 1, 8f, 0.6f, 0.8f, 18);
        }

        /// <summary>Per-frame feather projectile trail.</summary>
        public static void SwanSerenadeFeatherTrail(Vector2 pos, Vector2 velocity)
        {
            if (Main.dedServ) return;

            Vector2 away = -velocity.SafeNormalize(Vector2.Zero);
            Dust d = Dust.NewDustPerfect(pos, DustID.WhiteTorch, away * 0.5f, 0, PureWhite * 0.6f, 0.4f);
            d.noGravity = true;

            if (Main.rand.NextBool(6))
            {
                var glow = new GenericGlowParticle(pos, away * 0.3f, PureWhite * 0.3f, 0.06f, 12, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }

            SwanLakeVFXLibrary.AddSwanLight(pos, 0.2f);
        }

        // =====================================================================
        //  MONOCHROMATIC APOCALYPSE — Screen-filling VFX
        // =====================================================================

        /// <summary>MonochromaticApocalypse attack VFX — phase-aware intensity.</summary>
        public static void MonochromaticApocalypse(Vector2 pos)
        {
            if (Main.dedServ) return;

            SwanLakeSkySystem.TriggerMonochromeFlash(16f);
            MagnumScreenEffects.AddScreenShake(14f);

            // Massive radial dual-polarity burst
            for (int i = 0; i < 36; i++)
            {
                float angle = MathHelper.TwoPi * i / 36f;
                float speed = Main.rand.NextFloat(5f, 12f);
                Vector2 vel = angle.ToRotationVector2() * speed;
                bool isWhite = i % 2 == 0;

                if (isWhite)
                {
                    Dust wd = Dust.NewDustPerfect(pos, DustID.WhiteTorch, vel, 0, PureWhite, 2.0f);
                    wd.noGravity = true;
                    wd.fadeIn = 1.4f;
                }
                else
                {
                    Dust bd = Dust.NewDustPerfect(pos, DustID.Shadowflame, vel * 0.85f, 140, VoidBlack, 1.8f);
                    bd.noGravity = true;
                    bd.fadeIn = 1.4f;
                }
            }

            // Expanding alternating monochrome rings
            for (int ring = 0; ring < 3; ring++)
            {
                bool isWhiteRing = ring % 2 == 0;
                int count = 16 + ring * 6;
                float ringSpeed = 6f + ring * 3f;
                for (int i = 0; i < count; i++)
                {
                    float angle = MathHelper.TwoPi * i / count;
                    Vector2 vel = angle.ToRotationVector2() * ringSpeed;
                    Color col = isWhiteRing ? PureWhite : VoidBlack;
                    var glow = new GenericGlowParticle(pos, vel, col * 0.7f, 0.15f, 16, isWhiteRing);
                    MagnumParticleHandler.SpawnParticle(glow);
                }
            }

            // Prismatic fracture flash at center
            for (int i = 0; i < 10; i++)
            {
                Color rainbow = GetFractureRainbow(i / 10f);
                var prism = new BloomParticle(pos, Main.rand.NextVector2Circular(3f, 3f), rainbow * 0.6f, 0.2f, 10);
                MagnumParticleHandler.SpawnParticle(prism);
            }

            SwanLakeVFXLibrary.SpawnFeatherBurst(pos, 14, 0.4f);
            SwanLakeVFXLibrary.SpawnMusicNotes(pos, 6, 55f, 0.8f, 1.2f, 40);
            Lighting.AddLight(pos, PureWhite.ToVector3() * 2.2f);
        }

        // =====================================================================
        //  STANDARD ATTACK & PROJECTILE — Phase-dispatched
        // =====================================================================

        /// <summary>Standard boss attack VFX — dispatches based on current mood.</summary>
        public static void StandardAttackVFX(Vector2 pos, Vector2 direction, float intensity = 1f)
        {
            if (Main.dedServ) return;

            int mood = global::MagnumOpus.Common.Systems.Bosses.BossIndexTracker.SwanLakeMood;
            switch (mood)
            {
                case 0: WhiteSwanImpact(pos, intensity); break;
                case 1: BlackSwanImpact(pos, intensity); break;
                default: DyingSwanImpact(pos, intensity); break;
            }
        }

        /// <summary>Boss projectile impact — phase-aware.</summary>
        public static void BossProjectileImpact(Vector2 pos, float intensity = 1f)
        {
            if (Main.dedServ) return;

            int mood = global::MagnumOpus.Common.Systems.Bosses.BossIndexTracker.SwanLakeMood;
            switch (mood)
            {
                case 0: WhiteSwanImpact(pos, intensity * 0.8f); break;
                case 1: BlackSwanImpact(pos, intensity * 0.8f); break;
                default: DyingSwanImpact(pos, intensity * 0.8f); break;
            }
        }

        // =====================================================================
        //  DEATH SEQUENCE — The Final Note
        // =====================================================================

        /// <summary>
        /// Boss death VFX: duality converges, one final prismatic flash, then silence.
        /// </summary>
        public static void DeathSequence(Vector2 pos)
        {
            if (Main.dedServ) return;

            SwanLakeSkySystem.TriggerDeathFlash(20f);
            MagnumScreenEffects.AddScreenShake(18f);

            // Final dual-polarity implosion — all particles converge to center
            for (int i = 0; i < 40; i++)
            {
                float angle = MathHelper.TwoPi * i / 40f;
                float startRadius = Main.rand.NextFloat(60f, 100f);
                Vector2 startPos = pos + angle.ToRotationVector2() * startRadius;
                Vector2 vel = (pos - startPos).SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(2f, 5f);

                bool isWhite = i % 2 == 0;
                if (isWhite)
                {
                    Dust d = Dust.NewDustPerfect(startPos, DustID.WhiteTorch, vel, 0, PureWhite, 1.6f);
                    d.noGravity = true;
                    d.fadeIn = 1.5f;
                }
                else
                {
                    Dust d = Dust.NewDustPerfect(startPos, DustID.Shadowflame, vel, 140, VoidBlack, 1.4f);
                    d.noGravity = true;
                    d.fadeIn = 1.5f;
                }
            }

            // Final prismatic explosion — all the held-back rainbow erupts
            for (int i = 0; i < 16; i++)
            {
                float angle = MathHelper.TwoPi * i / 16f;
                Vector2 vel = angle.ToRotationVector2() * 5f;
                Color rainbow = GetFractureRainbow(i / 16f);
                var prismBurst = new BloomParticle(pos, vel, rainbow, 0.5f, 20);
                MagnumParticleHandler.SpawnParticle(prismBurst);
            }

            // Settling feathers — slow, mournful, grayscale
            for (int i = 0; i < 16; i++)
            {
                Vector2 featherVel = new Vector2(Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-0.5f, 1.5f));
                Color featherCol = Color.Lerp(SilverMist, GhostWhite, Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(pos + Main.rand.NextVector2Circular(40f, 40f),
                    DustID.SilverCoin, featherVel, 80, featherCol, 0.6f);
                d.noGravity = true;
                d.fadeIn = 1.6f;
            }

            // Final ascending sparkle wisps
            for (int i = 0; i < 8; i++)
            {
                Vector2 vel = new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-3f, -1.5f));
                var sparkle = new SparkleParticle(pos + Main.rand.NextVector2Circular(30f, 30f), vel, PureWhite * 0.6f, 0.25f, 35);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }

            SwanLakeVFXLibrary.SpawnMusicNotes(pos, 8, 60f, 0.7f, 1.0f, 50);
            Lighting.AddLight(pos, PureWhite.ToVector3() * 2.5f);
        }
    }
}
