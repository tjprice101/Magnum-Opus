using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.VFX.Core;
using MagnumOpus.Content.MoonlightSonata;

namespace MagnumOpus.Content.MoonlightSonata.VFX.Accessories
{
    /// <summary>
    /// Unique VFX for Moonlit Engine — the melee-class accessory.
    /// Theme: Mechanical precision, rotating gear particles, engine-rev pulse on proc.
    /// </summary>
    public static class MoonlitEngineVFX
    {
        private static readonly Color EngineViolet = new Color(160, 100, 220);
        private static readonly Color GearSilver = new Color(210, 215, 230);
        private static readonly Color PistonFlash = new Color(220, 180, 255);

        /// <summary>
        /// Ambient VFX — rotating gear motes around the player.
        /// Called from UpdateAccessory when not hidden.
        /// </summary>
        public static void AmbientGearOrbit(Vector2 playerCenter, int timer)
        {
            if (timer % 8 != 0) return;

            float baseAngle = timer * 0.03f;
            for (int i = 0; i < 4; i++)
            {
                float gearAngle = baseAngle + MathHelper.TwoPi * i / 4f;
                float radius = 28f + MathF.Sin(timer * 0.05f + i * 1.2f) * 5f;
                Vector2 gearPos = playerCenter + gearAngle.ToRotationVector2() * radius;

                Color gearColor = Color.Lerp(GearSilver, EngineViolet, (float)i / 4f);
                CustomParticles.MoonlightTrailFlare(gearPos, gearAngle.ToRotationVector2() * 0.3f);
            }
        }

        /// <summary>
        /// Engine-rev shockwave — triggered on every 5th melee hit.
        /// Big burst with mechanical precision sparks.
        /// </summary>
        public static void EngineRevPulse(Vector2 pulseCenter, float intensity = 1f)
        {
            // Central engine flash
            CustomParticles.GenericFlare(pulseCenter, PistonFlash, 0.7f * intensity, 18);
            CustomParticles.GenericFlare(pulseCenter, EngineViolet, 0.5f * intensity, 22);

            // Expanding gear rings
            CustomParticles.MoonlightHalo(pulseCenter, 0.5f * intensity);
            CustomParticles.HaloRing(pulseCenter, EngineViolet, 0.35f * intensity, 20);

            // Radial precision sparks — 8-point mechanical pattern
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 sparkVel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 8f);
                Color sparkColor = Color.Lerp(GearSilver, PistonFlash, (float)i / 8f);
                Dust d = Dust.NewDustPerfect(pulseCenter, DustID.MagicMirror, sparkVel, 0, sparkColor, 1.3f);
                d.noGravity = true;
            }

            // Music notes
            MoonlightVFXLibrary.SpawnMusicNotes(pulseCenter, 3, 15f, 0.7f, 0.9f, 25);

            Lighting.AddLight(pulseCenter, EngineViolet.ToVector3() * intensity);
        }
    }

    /// <summary>
    /// Unique VFX for Moonlit Gyre — the ranger-class accessory.
    /// Theme: Gyroscopic spin, spiral particles, sonic boom on proc.
    /// </summary>
    public static class MoonlitGyreVFX
    {
        private static readonly Color GyroBlue = new Color(120, 180, 255);
        private static readonly Color SonicPulse = new Color(180, 220, 255);
        private static readonly Color SpinTrail = new Color(140, 100, 240);

        /// <summary>
        /// Ambient VFX — rotating crescent ring around player.
        /// </summary>
        public static void AmbientGyroSpin(Vector2 playerCenter, int timer)
        {
            if (timer % 6 != 0) return;

            float spinAngle = timer * 0.06f; // Faster spin than engine
            for (int i = 0; i < 3; i++)
            {
                float angle = spinAngle + MathHelper.TwoPi * i / 3f;
                float radius = 32f + MathF.Sin(timer * 0.04f) * 4f;
                Vector2 gyroPos = playerCenter + angle.ToRotationVector2() * radius;

                Color gyroColor = Color.Lerp(GyroBlue, SpinTrail, (float)i / 3f);
                Dust d = Dust.NewDustPerfect(gyroPos, DustID.MagicMirror, angle.ToRotationVector2() * 0.3f, 0, gyroColor, 0.8f);
                d.noGravity = true;
            }
        }

        /// <summary>
        /// Sonic boom VFX — triggered on gyroscope proc.
        /// Expanding speed-rings with directional blast.
        /// </summary>
        public static void SonicBoomPulse(Vector2 center, Vector2 direction, float intensity = 1f)
        {
            // Central sonic flash
            CustomParticles.GenericFlare(center, SonicPulse, 0.6f * intensity, 15);
            CustomParticles.GenericFlare(center, GyroBlue, 0.45f * intensity, 20);

            // Directional sonic rings
            for (int ring = 0; ring < 3; ring++)
            {
                Vector2 ringPos = center + direction * (15f + ring * 12f);
                CustomParticles.HaloRing(ringPos, SonicPulse, 0.25f + ring * 0.1f, 12 + ring * 3);
            }

            // Speed-blur particles along direction
            for (int i = 0; i < 8; i++)
            {
                Vector2 vel = direction.RotatedByRandom(0.3f) * Main.rand.NextFloat(5f, 10f);
                Dust d = Dust.NewDustPerfect(center, DustID.MagicMirror, vel, 0, SonicPulse, 1.1f);
                d.noGravity = true;
            }

            MoonlightVFXLibrary.SpawnMusicNotes(center, 2, 12f, 0.7f, 0.85f, 25);
            Lighting.AddLight(center, GyroBlue.ToVector3() * 0.8f * intensity);
        }
    }

    /// <summary>
    /// Unique VFX for Fractal of Moonlight — the summoner-class accessory.
    /// Theme: Fractal geometry, branching patterns, recursive visual cascades.
    /// </summary>
    public static class FractalOfMoonlightVFX
    {
        private static readonly Color FractalViolet = new Color(170, 80, 240);
        private static readonly Color BranchSilver = new Color(200, 210, 240);
        private static readonly Color RecursiveGlow = new Color(220, 180, 255);

        /// <summary>
        /// Ambient VFX — orbiting fractal geometry motes.
        /// </summary>
        public static void AmbientFractalOrbit(Vector2 playerCenter, int timer)
        {
            if (timer % 10 != 0) return;

            // Triangular orbit (3-point fractal base)
            float baseAngle = timer * 0.02f;
            for (int i = 0; i < 3; i++)
            {
                float angle = baseAngle + MathHelper.TwoPi * i / 3f;
                float radius = 35f;
                Vector2 vertexPos = playerCenter + angle.ToRotationVector2() * radius;

                // Sub-fractal: each vertex spawns smaller motes
                Color vertexColor = Color.Lerp(FractalViolet, BranchSilver, (float)i / 3f);
                CustomParticles.MoonlightTrailFlare(vertexPos, (angle + MathHelper.PiOver2).ToRotationVector2() * 0.2f);

                // Branch toward next vertex
                int nextI = (i + 1) % 3;
                float nextAngle = baseAngle + MathHelper.TwoPi * nextI / 3f;
                Vector2 nextPos = playerCenter + nextAngle.ToRotationVector2() * radius;
                Vector2 midpoint = (vertexPos + nextPos) * 0.5f;

                Dust d = Dust.NewDustPerfect(midpoint, DustID.Enchanted_Gold, Vector2.Zero, 0, vertexColor * 0.6f, 0.7f);
                d.noGravity = true;
            }
        }

        /// <summary>
        /// Fractal surge VFX — triggered when minion damage boost procs.
        /// Recursive expanding pattern.
        /// </summary>
        public static void FractalSurgePulse(Vector2 center, float intensity = 1f)
        {
            // Central bloom
            CustomParticles.GenericFlare(center, RecursiveGlow, 0.6f * intensity, 18);
            CustomParticles.MoonlightHalo(center, 0.45f * intensity);

            // Fractal burst — 6-point star with recursive sub-bursts
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f;
                Vector2 dir = angle.ToRotationVector2();

                // Primary branch
                for (int j = 0; j < 3; j++)
                {
                    Vector2 branchPos = center + dir * (12f + j * 10f);
                    Color branchColor = Color.Lerp(FractalViolet, BranchSilver, (float)j / 3f);
                    Dust d = Dust.NewDustPerfect(branchPos, DustID.MagicMirror, dir * (2f + j), 0, branchColor, 1.1f);
                    d.noGravity = true;
                }

                // Sub-branch at tip (fractal recursion)
                Vector2 tipPos = center + dir * 32f;
                for (int sub = -1; sub <= 1; sub += 2)
                {
                    Vector2 subDir = dir.RotatedBy(sub * MathHelper.PiOver4 * 0.7f);
                    Dust sd = Dust.NewDustPerfect(tipPos, DustID.Enchanted_Gold, subDir * 3f, 0, RecursiveGlow, 0.9f);
                    sd.noGravity = true;
                }
            }

            MoonlightVFXLibrary.SpawnMusicNotes(center, 3, 18f, 0.75f, 0.95f, 30);
            Lighting.AddLight(center, FractalViolet.ToVector3() * 0.9f * intensity);
        }
    }

    /// <summary>
    /// Unique VFX for Ember of the Moon — the mage-class accessory.
    /// Theme: Lunar embers, soft persistent glow, double-cast flash.
    /// </summary>
    public static class EmberOfTheMoonVFX
    {
        private static readonly Color EmberLavender = new Color(200, 160, 255);
        private static readonly Color EmberCore = new Color(255, 220, 240);
        private static readonly Color DoubleCastFlash = new Color(180, 140, 255);

        /// <summary>
        /// Ambient VFX — floating moon-ember motes.
        /// </summary>
        public static void AmbientEmberFloat(Vector2 playerCenter, int timer)
        {
            if (timer % 12 != 0) return;

            for (int i = 0; i < 2; i++)
            {
                Vector2 offset = Main.rand.NextVector2Circular(25f, 25f);
                Vector2 vel = new Vector2(Main.rand.NextFloat(-0.3f, 0.3f), -0.5f - Main.rand.NextFloat(0.3f));
                Color emberColor = Color.Lerp(EmberLavender, EmberCore, Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(playerCenter + offset, DustID.Enchanted_Pink, vel, 0, emberColor, 1.0f);
                d.noGravity = true;
            }
        }

        /// <summary>
        /// Double-cast flash — triggered when double-cast procs on magic weapons.
        /// Brief bright flash with mage-themed particles.
        /// </summary>
        public static void DoubleCastProcFlash(Vector2 castPos, float intensity = 1f)
        {
            // Bright flash
            CustomParticles.GenericFlare(castPos, EmberCore, 0.55f * intensity, 12);
            CustomParticles.GenericFlare(castPos, DoubleCastFlash, 0.4f * intensity, 16);

            // Ember scatter
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 5f);
                Color scatterColor = Color.Lerp(EmberLavender, EmberCore, Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(castPos, DustID.Enchanted_Pink, vel, 0, scatterColor, 1.2f);
                d.noGravity = true;
            }

            CustomParticles.MoonlightHalo(castPos, 0.3f * intensity);
            MoonlightVFXLibrary.SpawnMusicNotes(castPos, 2, 10f, 0.7f, 0.85f, 20);
            Lighting.AddLight(castPos, EmberLavender.ToVector3() * 0.7f * intensity);
        }
    }

    /// <summary>
    /// Unique VFX for Adagio Pendant — the Tier 1 theme accessory.
    /// Theme: Gentle, slow, meditative. Soft drifting notes and calm lavender aura.
    /// </summary>
    public static class AdagioPendantVFX
    {
        private static readonly Color AdagioLavender = new Color(190, 160, 230);
        private static readonly Color GentleSilver = new Color(220, 215, 240);

        /// <summary>
        /// Ambient VFX — gentle floating aura with slow-drifting music notes.
        /// </summary>
        public static void AmbientAdagioAura(Vector2 playerCenter, int timer)
        {
            // Soft lavender motes (every 8 frames — gentle, not dense)
            if (timer % 8 == 0)
            {
                Vector2 offset = Main.rand.NextVector2Circular(20f, 20f);
                Vector2 vel = new Vector2(Main.rand.NextFloat(-0.2f, 0.2f), -0.3f - Main.rand.NextFloat(0.2f));
                Dust d = Dust.NewDustPerfect(playerCenter + offset, DustID.Enchanted_Pink, vel, 0, AdagioLavender, 0.9f);
                d.noGravity = true;
            }

            // Slow drifting music notes (every 25 frames)
            if (timer % 25 == 0)
            {
                MoonlightVFXLibrary.SpawnMusicNotes(playerCenter + new Vector2(0f, -15f), 1, 18f, 0.65f, 0.8f, 45);
            }

            // Soft ambient light
            float pulse = 0.25f + MathF.Sin(timer * 0.02f) * 0.05f;
            Lighting.AddLight(playerCenter, AdagioLavender.ToVector3() * pulse);
        }

        /// <summary>
        /// Heal/regen proc VFX — soft shimmer when Adagio healing procs.
        /// </summary>
        public static void HealProcShimmer(Vector2 playerCenter)
        {
            CustomParticles.MoonlightFlare(playerCenter, 0.3f);

            for (int i = 0; i < 5; i++)
            {
                Vector2 vel = new Vector2(Main.rand.NextFloat(-1f, 1f), -1f - Main.rand.NextFloat(1f));
                Dust d = Dust.NewDustPerfect(playerCenter + Main.rand.NextVector2Circular(12f, 12f), DustID.Enchanted_Pink, vel, 0, GentleSilver, 1.0f);
                d.noGravity = true;
            }

            MoonlightVFXLibrary.SpawnMusicNotes(playerCenter, 2, 12f, 0.7f, 0.85f, 30);
        }
    }

    /// <summary>
    /// Unique VFX for Sonata's Embrace — the Tier 2 ultimate theme accessory.
    /// Theme: Full orchestral power, multi-layered moonlight aura, Moonstruck application.
    /// The most visually impressive accessory in the Moonlight Sonata theme.
    /// </summary>
    public static class SonatasEmbraceVFX
    {
        private static readonly Color OrchestraViolet = new Color(170, 100, 240);
        private static readonly Color ConductorSilver = new Color(230, 225, 245);
        private static readonly Color MoonstruckFlash = new Color(200, 150, 255);
        private static readonly Color SonataGold = new Color(240, 220, 180);

        /// <summary>
        /// Ambient VFX — full 3-layer orchestral aura.
        /// Layer 1: Inner music notes (close orbit)
        /// Layer 2: Mid sparkle ring (medium orbit)  
        /// Layer 3: Outer cosmic motes (wide orbit)
        /// </summary>
        public static void AmbientOrchestraAura(Vector2 playerCenter, int timer)
        {
            // Layer 1: Inner music notes (every 12 frames)
            if (timer % 12 == 0)
            {
                float innerAngle = timer * 0.04f;
                Vector2 notePos = playerCenter + innerAngle.ToRotationVector2() * 20f;
                MoonlightVFXLibrary.SpawnMusicNotes(notePos, 1, 5f, 0.7f, 0.9f, 30);
            }

            // Layer 2: Mid sparkle ring (every 6 frames)
            if (timer % 6 == 0)
            {
                float midAngle = timer * 0.05f;
                for (int i = 0; i < 2; i++)
                {
                    float angle = midAngle + MathHelper.Pi * i;
                    Vector2 sparkPos = playerCenter + angle.ToRotationVector2() * 32f;
                    Dust d = Dust.NewDustPerfect(sparkPos, DustID.Enchanted_Gold, Vector2.Zero, 0, ConductorSilver, 0.8f);
                    d.noGravity = true;
                }
            }

            // Layer 3: Outer cosmic motes (every 10 frames)
            if (timer % 10 == 0)
            {
                float outerAngle = timer * 0.025f;
                for (int i = 0; i < 3; i++)
                {
                    float angle = outerAngle + MathHelper.TwoPi * i / 3f;
                    float radius = 40f + MathF.Sin(timer * 0.03f + i) * 5f;
                    Vector2 motePos = playerCenter + angle.ToRotationVector2() * radius;

                    Color moteColor = Color.Lerp(OrchestraViolet, SonataGold, (float)i / 3f);
                    CustomParticles.MoonlightTrailFlare(motePos, angle.ToRotationVector2() * 0.2f);
                }
            }

            // Ambient lighting — multi-toned
            float pulse = 0.35f + MathF.Sin(timer * 0.03f) * 0.08f;
            Lighting.AddLight(playerCenter, OrchestraViolet.ToVector3() * pulse);
        }

        /// <summary>
        /// Moonstruck application flash — when Sonata's Embrace applies Moonstruck debuff.
        /// </summary>
        public static void MoonstruckApplicationFlash(Vector2 targetCenter)
        {
            // Dramatic flash
            CustomParticles.GenericFlare(targetCenter, MoonstruckFlash, 0.7f, 18);
            CustomParticles.GenericFlare(targetCenter, Color.White, 0.5f, 12);

            // Debuff rings
            CustomParticles.MoonlightHalo(targetCenter, 0.5f);
            CustomParticles.HaloRing(targetCenter, OrchestraViolet, 0.35f, 20);

            // Radial music note burst
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f;
                Vector2 noteVel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 4f);
                MoonlightVFXLibrary.SpawnMusicNotes(targetCenter, 1, 8f, 0.75f, 1.0f, 30);
            }

            // Sparkle spray
            for (int i = 0; i < 10; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(5f, 5f);
                Color sparkColor = Color.Lerp(OrchestraViolet, ConductorSilver, Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(targetCenter, DustID.MagicMirror, vel, 0, sparkColor, 1.2f);
                d.noGravity = true;
            }

            Lighting.AddLight(targetCenter, MoonstruckFlash.ToVector3() * 1.0f);
        }

        /// <summary>
        /// On-hit enhancement VFX — the extra visual punch Sonata's Embrace adds to all attacks.
        /// </summary>
        public static void OnHitEnhancement(Vector2 hitPos)
        {
            // Subtle overlay flash
            CustomParticles.MoonlightFlare(hitPos, 0.35f);

            // Extra sparkles
            for (int i = 0; i < 4; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(3f, 3f);
                Dust d = Dust.NewDustPerfect(hitPos, DustID.Enchanted_Gold, vel, 0, ConductorSilver, 0.9f);
                d.noGravity = true;
            }

            // Accent music notes
            if (Main.rand.NextBool(3))
            {
                MoonlightVFXLibrary.SpawnMusicNotes(hitPos, 1, 8f, 0.7f, 0.85f, 20);
            }
        }
    }
}
