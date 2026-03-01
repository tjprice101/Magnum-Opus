using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Content.ClairDeLune.Weapons.Ranged
{
    // =========================================================================
    //  StarfallWhisperVFX — Starfall Trail
    //  Shader: StarfallTrail.fx (StarfallBolt + StarfallWake)
    //  Identity: FALLING STAR COMET — each shot is a miniature meteor with a
    //  dual-tail comet wake: inner core of starlight silver + outer veil of
    //  twinkling sparkle stardust. On impact, a stellar shower cascades outward
    //  like a star fragmenting. Charge builds a converging constellation halo.
    //  Distinct from Mechanism (brass clockwork tracers) and Cog (gravity vortex).
    // =========================================================================
    public static class StarfallWhisperVFX
    {
        public static void HoldItemVFX(Player player)
        {
            float time = (float)Main.timeForVisualEffects;

            // SIGNATURE: Drifting starfield motes — tiny stars fall gently downward like snow
            if (Main.rand.NextBool(3))
            {
                Vector2 starPos = player.Center + Main.rand.NextVector2Circular(28f, 15f) + Vector2.UnitY * -12f;
                Vector2 driftVel = new Vector2(Main.rand.NextFloat(-0.2f, 0.2f), Main.rand.NextFloat(0.3f, 0.8f));
                Color starCol = ClairDeLunePalette.PaletteLerp(
                    ClairDeLunePalette.StarfallWhisperShot,
                    0.3f + (float)Math.Sin(time * 0.04f) * 0.2f);
                var star = new SparkleParticle(starPos, driftVel, starCol * 0.4f, 0.12f, 18);
                MagnumParticleHandler.SpawnParticle(star);
            }

            // Occasional starlit sparkle
            if (Main.rand.NextBool(8))
                ClairDeLuneVFXLibrary.SpawnStarlitSparkles(player.Center, 1, 25f, 0.12f);

            ClairDeLuneVFXLibrary.AddClairDeLuneLight(player.Center, 0.2f);
        }

        public static void PreDrawInWorldBloom(SpriteBatch sb, Texture2D tex, Vector2 pos,
            Vector2 origin, float rotation, float scale)
        {
            if (ClairDeLuneShaderManager.HasStarfallTrail)
            {
                ClairDeLuneShaderManager.BeginShaderAdditive(sb);
                ClairDeLuneShaderManager.ApplyStarfallWake((float)Main.timeForVisualEffects * 0.03f);

                sb.Draw(tex, pos - Main.screenPosition, null,
                    ClairDeLunePalette.StarlightSilver * 0.5f, rotation, origin, scale * 1.03f,
                    SpriteEffects.None, 0f);

                ClairDeLuneShaderManager.RestoreSpriteBatch(sb);
            }
            else
            {
                ClairDeLuneVFXLibrary.DrawClairDeLuneBloomStack(sb, pos,
                    ClairDeLunePalette.StarlightSilver, ClairDeLunePalette.PearlBlue, scale * 0.25f, 0.35f);
            }
        }

        /// <summary>
        /// SIGNATURE BOLT TRAIL: Dual-tail comet wake — inner starlight core streak
        /// with an outer veil of twinkling sparkle stardust that fans out behind the bolt.
        /// The trail widens toward the back (comet tail shape), not uniform.
        /// </summary>
        public static void BoltTrailVFX(Vector2 boltPos, Vector2 boltVelocity)
        {
            float time = (float)Main.timeForVisualEffects;
            Vector2 perpendicular = new Vector2(-boltVelocity.Y, boltVelocity.X);
            if (perpendicular != Vector2.Zero) perpendicular.Normalize();

            // INNER CORE: Tight starlight silver streak along bolt axis
            Dust d = Dust.NewDustPerfect(boltPos, DustID.SilverFlame,
                boltVelocity * -0.1f + perpendicular * Main.rand.NextFloat(-0.3f, 0.3f),
                0, default, 0.65f);
            d.noGravity = true;
            d.fadeIn = 0.8f;

            // OUTER VEIL: Sparkle stardust fanning outward — wider toward the tail
            for (int i = 0; i < 2; i++)
            {
                float trailDist = Main.rand.NextFloat(5f, 25f);
                float fanWidth = 2f + trailDist * 0.15f; // Widens toward back
                Vector2 wakePos = boltPos - boltVelocity.SafeNormalize(Vector2.Zero) * trailDist;
                Vector2 fanOffset = perpendicular * Main.rand.NextFloat(-fanWidth, fanWidth);

                Color wakeCol = ClairDeLunePalette.PaletteLerp(
                    ClairDeLunePalette.StarfallWhisperShot,
                    0.3f + trailDist * 0.02f);
                var sparkle = new SparkleParticle(
                    wakePos + fanOffset,
                    perpendicular * Main.rand.NextFloat(-0.3f, 0.3f) + Vector2.UnitY * 0.2f,
                    wakeCol * 0.55f, 0.14f, 14);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }

            // Periodic ice-blue trailing glow
            if (Main.GameUpdateCount % 2 == 0)
            {
                var trailGlow = new GenericGlowParticle(
                    boltPos, boltVelocity * -0.08f,
                    ClairDeLunePalette.StarlightSilver * 0.4f, 0.2f, 10, true);
                MagnumParticleHandler.SpawnParticle(trailGlow);
            }

            if (Main.GameUpdateCount % 6 == 0)
                ClairDeLuneVFXLibrary.SpawnMusicNotes(boltPos, 1, 6f, 0.25f, 0.45f, 12);

            ClairDeLuneVFXLibrary.AddPaletteLighting(boltPos, 0.3f, 0.35f);
        }

        /// <summary>
        /// BOLT IMPACT: Stellar shower cascade — star fragments radiate outward
        /// in a spray pattern (NOT radial uniform), each trailing tiny sparkles.
        /// Like a star bursting into fragments on impact.
        /// </summary>
        public static void BoltImpactVFX(Vector2 hitPos)
        {
            ClairDeLuneVFXLibrary.ProjectileImpact(hitPos, 0);

            // SIGNATURE: Star-fragment spray — fragments fly in varied speeds with sparkle trails
            int fragmentCount = 16;
            for (int i = 0; i < fragmentCount; i++)
            {
                float angle = MathHelper.TwoPi * i / fragmentCount + Main.rand.NextFloat(-0.15f, 0.15f);
                float speed = 3.5f + Main.rand.NextFloat() * 4f; // Varied speeds for natural look
                Vector2 vel = angle.ToRotationVector2() * speed;

                Color fragCol = ClairDeLunePalette.PaletteLerp(
                    ClairDeLunePalette.StarfallWhisperShot, (float)i / fragmentCount);
                int dustType = (i % 3 == 0) ? DustID.SilverFlame : DustID.IceTorch;
                Dust d = Dust.NewDustPerfect(hitPos, dustType, vel, 0, default, 0.9f);
                d.noGravity = true;
                d.fadeIn = 1f;

                // Star sparkle particle on every other fragment
                if (i % 2 == 0)
                {
                    var frag = new SparkleParticle(hitPos, vel * 0.6f, fragCol * 0.7f, 0.22f, 16);
                    MagnumParticleHandler.SpawnParticle(frag);
                }
            }

            // Central stellar flash
            var coreFlash = new GenericGlowParticle(hitPos, Vector2.Zero,
                ClairDeLunePalette.PearlWhite * 0.8f, 0.5f, 8, true);
            MagnumParticleHandler.SpawnParticle(coreFlash);

            ClairDeLuneVFXLibrary.SpawnStarlitSparkles(hitPos, 6, 30f, 0.22f);
            ClairDeLuneVFXLibrary.SpawnPearlBurst(hitPos, 8, 4f, 0.3f);
            ClairDeLuneVFXLibrary.SpawnMusicNotes(hitPos, 2, 14f, 0.4f, 0.7f, 20);
            ClairDeLuneVFXLibrary.DrawBloom(hitPos, 0.45f, 0.75f);
            ClairDeLuneVFXLibrary.AddPaletteLighting(hitPos, 0.3f, 0.6f);
        }

        /// <summary>
        /// CHARGE UP: Converging constellation — starlit motes converge inward
        /// along geometric lines (like constellation connections), growing brighter
        /// as charge builds. At high charge, a halo ring pulses at the muzzle.
        /// </summary>
        public static void ChargeUpVFX(Vector2 muzzlePos, float chargeProgress)
        {
            float intensity = MathHelper.Clamp(chargeProgress, 0f, 1f);
            float time = (float)Main.timeForVisualEffects;
            int dustCount = (int)(4 * intensity) + 1;

            // Converging star motes along geometric radial lines
            for (int i = 0; i < dustCount; i++)
            {
                float lineAngle = MathHelper.TwoPi * i / Math.Max(dustCount, 1) + time * 0.02f;
                float radius = 35f * (1f - intensity * 0.7f) + 5f;
                Vector2 dustPos = muzzlePos + lineAngle.ToRotationVector2() * radius;
                Vector2 vel = (muzzlePos - dustPos) * 0.09f * intensity;

                Color chargeCol = ClairDeLunePalette.PaletteLerp(
                    ClairDeLunePalette.StarfallWhisperShot, 0.2f + intensity * 0.6f);
                var converge = new SparkleParticle(dustPos, vel, chargeCol * (0.3f + intensity * 0.5f),
                    0.16f * intensity, 12);
                MagnumParticleHandler.SpawnParticle(converge);
            }

            // Core converging dust
            if (Main.rand.NextBool(Math.Max(1, 4 - (int)(3 * intensity))))
            {
                float angle = Main.rand.NextFloat() * MathHelper.TwoPi;
                float rad = 25f * (1f - intensity) + 8f;
                Vector2 pos = muzzlePos + angle.ToRotationVector2() * rad;
                Dust d = Dust.NewDustPerfect(pos, DustID.IceTorch, (muzzlePos - pos) * 0.08f * intensity,
                    0, default, 0.5f * intensity);
                d.noGravity = true;
                d.fadeIn = 0.6f;
            }

            // High charge: pulsing halo ring at muzzle
            if (intensity > 0.6f && Main.GameUpdateCount % 8 == 0)
                CustomParticles.HaloRing(muzzlePos, ClairDeLunePalette.StarlightSilver * (intensity * 0.5f),
                    0.15f * intensity, 10);

            if (intensity > 0.7f)
                ClairDeLuneVFXLibrary.SpawnPearlSparkle(muzzlePos, Vector2.Zero);

            ClairDeLuneVFXLibrary.AddPaletteLighting(muzzlePos, 0.3f, 0.35f * intensity);
        }
    }

    // =========================================================================
    //  MidnightMechanismVFX — Gatling Blur
    //  Shader: GatlingBlur.fx (GatlingBarrelBlur + GatlingMuzzle)
    //  Identity: CLOCKWORK TRACER STORM — rapid-fire brassgold tracers with
    //  alternating brass/crimson shell signatures. Each bullet leaves a short,
    //  bright tracer line (not a soft trail). Muzzle flash creates staccato
    //  brass-white bursts. Spin-up builds rotating clockwork gear sparks.
    //  Distinct from Starfall (soft starlit comet) and Cog (gravity vortex).
    // =========================================================================
    public static class MidnightMechanismVFX
    {
        private static float _barrelAngle = 0f;

        public static void HoldItemVFX(Player player)
        {
            float time = (float)Main.timeForVisualEffects;

            // SIGNATURE: Ticking clockwork idle — gear-like dust orbiting at barrel position
            _barrelAngle += 0.02f;
            if (Main.rand.NextBool(4))
            {
                float tickAngle = _barrelAngle + Main.rand.NextFloat(MathHelper.TwoPi);
                float tickRadius = 14f + (float)Math.Sin(time * 0.05f) * 3f;
                Vector2 tickPos = player.Center + new Vector2(player.direction * 12f, -4f)
                    + tickAngle.ToRotationVector2() * tickRadius;
                Dust d = Dust.NewDustPerfect(tickPos, DustID.GoldFlame,
                    Vector2.Zero, 0, default, 0.3f);
                d.noGravity = true;
                d.fadeIn = 0.4f;
            }

            ClairDeLuneVFXLibrary.AmbientClockworkAura(player.Center, time);
            ClairDeLuneVFXLibrary.AddClairDeLuneLight(player.Center, 0.2f);
        }

        public static void PreDrawInWorldBloom(SpriteBatch sb, Texture2D tex, Vector2 pos,
            Vector2 origin, float rotation, float scale)
        {
            if (ClairDeLuneShaderManager.HasGatlingBlur)
            {
                ClairDeLuneShaderManager.BeginShaderAdditive(sb);
                ClairDeLuneShaderManager.ApplyGatlingMuzzle((float)Main.timeForVisualEffects * 0.04f);

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

        /// <summary>
        /// SIGNATURE BULLET TRAIL: Short bright tracer line — brass-gold with alternating
        /// crimson tracers every few bullets. NOT soft trailing dust — hard, linear, fast.
        /// Perpendicular casing sparks eject sideways like shell casings.
        /// </summary>
        public static void BulletTrailVFX(Vector2 bulletPos, Vector2 bulletVelocity)
        {
            Vector2 perpendicular = new Vector2(-bulletVelocity.Y, bulletVelocity.X);
            if (perpendicular != Vector2.Zero) perpendicular.Normalize();

            // CORE TRACER: Hard brass-gold line along bullet path
            Dust d = Dust.NewDustPerfect(bulletPos, DustID.GoldFlame,
                bulletVelocity * -0.06f, 0, default, 0.55f);
            d.noGravity = true;
            d.fadeIn = 0.7f;

            // Alternating crimson tracer every 3rd bullet (tracked by game time)
            bool isCrimsonTracer = (Main.GameUpdateCount % 6 < 2);
            if (isCrimsonTracer)
            {
                Dust crimson = Dust.NewDustPerfect(bulletPos, DustID.FireworkFountain_Red,
                    bulletVelocity * -0.04f, 0, default, 0.4f);
                crimson.noGravity = true;
            }

            // CASING EJECT: Perpendicular spark burst (like brass shell casing ejection)
            if (Main.GameUpdateCount % 3 == 0)
            {
                float ejectSide = Main.rand.NextBool() ? 1f : -1f;
                Vector2 ejectVel = perpendicular * ejectSide * Main.rand.NextFloat(1.5f, 3f)
                    + Vector2.UnitY * 0.5f; // Slight gravity on casings
                Color casingCol = ClairDeLunePalette.PaletteLerp(
                    ClairDeLunePalette.MidnightMechanismBurst, 0.4f);
                var casing = new SparkleParticle(bulletPos, ejectVel, casingCol * 0.5f, 0.1f, 8);
                MagnumParticleHandler.SpawnParticle(casing);
            }

            // Inline glow — palette-colored tracer core behind bullet
            if (Main.GameUpdateCount % 2 == 0)
            {
                Color tracerCol = ClairDeLunePalette.PaletteLerp(
                    ClairDeLunePalette.MidnightMechanismBurst,
                    isCrimsonTracer ? 0.6f : 0.3f);
                var tracerGlow = new GenericGlowParticle(
                    bulletPos, bulletVelocity * -0.05f, tracerCol * 0.5f, 0.15f, 6, true);
                MagnumParticleHandler.SpawnParticle(tracerGlow);
            }

            ClairDeLuneVFXLibrary.AddPaletteLighting(bulletPos, 0.6f, 0.28f);
        }

        /// <summary>
        /// BULLET IMPACT: Staccato clockwork detonation — a tight, fast burst
        /// of brass sparks radiating outward with angular precision, not a soft bloom.
        /// Distinct from Starfall (soft star cascade) and Cog (gravity implosion).
        /// </summary>
        public static void BulletImpactVFX(Vector2 hitPos)
        {
            ClairDeLuneVFXLibrary.ProjectileImpact(hitPos, 0);

            // SIGNATURE: Angular gear-spark spray — 8-directional with alternating brass/pearl
            int dustCount = 12;
            for (int i = 0; i < dustCount; i++)
            {
                // Snap to 8 directions for mechanical precision feel
                float baseAngle = MathHelper.TwoPi * i / dustCount;
                float snapAngle = (float)Math.Round(baseAngle / (MathHelper.PiOver4)) * MathHelper.PiOver4;
                float angle = MathHelper.Lerp(baseAngle, snapAngle, 0.3f);
                float speed = 3.5f + Main.rand.NextFloat() * 2.5f;
                Vector2 vel = angle.ToRotationVector2() * speed;

                Color burstCol = ClairDeLunePalette.PaletteLerp(
                    ClairDeLunePalette.MidnightMechanismBurst, (float)i / dustCount);
                int dustType = (i % 2 == 0) ? DustID.GoldFlame : DustID.IceTorch;
                Dust d = Dust.NewDustPerfect(hitPos, dustType, vel, 0, default, 0.75f);
                d.noGravity = true;
                d.fadeIn = 0.9f;
            }

            ClairDeLuneVFXLibrary.SpawnPearlBurst(hitPos, 4, 2.5f, 0.2f);
            ClairDeLuneVFXLibrary.SpawnMusicNotes(hitPos, 1, 8f, 0.3f, 0.5f, 12);
            ClairDeLuneVFXLibrary.DrawBloom(hitPos, 0.3f, 0.55f);
            ClairDeLuneVFXLibrary.AddPaletteLighting(hitPos, 0.6f, 0.45f);
        }

        /// <summary>
        /// SPIN-UP: Clockwork mechanism building momentum — gears rotate around
        /// the barrel with increasing speed and spark intensity. At full spin,
        /// the barrel glows with brass-gold heat.
        /// </summary>
        public static void SpinUpVFX(Vector2 barrelPos, float spinProgress)
        {
            float intensity = MathHelper.Clamp(spinProgress, 0f, 1f);
            float time = (float)Main.timeForVisualEffects;

            // Rotating gear sparks — increasing count and speed with spin
            _barrelAngle += 0.04f + intensity * 0.12f;
            int sparkCount = 1 + (int)(3 * intensity);
            for (int i = 0; i < sparkCount; i++)
            {
                float sparkAngle = _barrelAngle + MathHelper.TwoPi * i / sparkCount;
                float sparkRadius = 8f + 5f * intensity;
                Vector2 sparkPos = barrelPos + sparkAngle.ToRotationVector2() * sparkRadius;
                Vector2 tangent = new Vector2(-(float)Math.Sin(sparkAngle), (float)Math.Cos(sparkAngle));
                Vector2 vel = tangent * (1f + intensity * 2f);

                if (Main.rand.NextBool(Math.Max(1, 4 - (int)(3 * intensity))))
                {
                    Dust d = Dust.NewDustPerfect(sparkPos, DustID.GoldFlame, vel, 0, default,
                        0.35f + intensity * 0.3f);
                    d.noGravity = true;
                }
            }

            // High spin: brass heat glow and pearl shimmer
            if (intensity > 0.5f && Main.GameUpdateCount % 4 == 0)
            {
                Color heatCol = Color.Lerp(ClairDeLunePalette.ClockworkBrass,
                    ClairDeLunePalette.MoonbeamGold, intensity);
                var heat = new GenericGlowParticle(barrelPos, Vector2.Zero,
                    heatCol * 0.3f * intensity, 0.2f, 8, true);
                MagnumParticleHandler.SpawnParticle(heat);
            }

            if (intensity > 0.7f && Main.GameUpdateCount % 6 == 0)
                ClairDeLuneVFXLibrary.SpawnMusicNotes(barrelPos, 1, 8f, 0.3f * intensity, 0.5f, 12);

            ClairDeLuneVFXLibrary.AddPaletteLighting(barrelPos, 0.6f, 0.3f * intensity);
        }
    }

    // =========================================================================
    //  CogAndHammerVFX — Singularity Pull
    //  Shader: SingularityPull.fx (SingularityVortex + SingularityCore)
    //  Identity: GRAVITATIONAL SINGULARITY — bombs create visible gravity wells
    //  as they fly, with particles spiraling INWARD toward the bomb in a vortex.
    //  On detonation: massive implosion-then-explosion with concentric shockwave
    //  rings. The only weapon in Clair de Lune with INWARD particle flow.
    //  Distinct from Starfall (outward starlight) and Mechanism (linear tracers).
    // =========================================================================
    public static class CogAndHammerVFX
    {
        public static void HoldItemVFX(Player player)
        {
            float time = (float)Main.timeForVisualEffects;

            // SIGNATURE: Gravity well idle — particles slowly converge toward the player's hand
            if (Main.rand.NextBool(3))
            {
                float pullAngle = Main.rand.NextFloat(MathHelper.TwoPi);
                float pullRadius = 18f + Main.rand.NextFloat(8f);
                Vector2 pullPos = player.Center + pullAngle.ToRotationVector2() * pullRadius;
                Vector2 inwardVel = (player.Center - pullPos) * 0.025f;

                Color pullCol = ClairDeLunePalette.PaletteLerp(
                    ClairDeLunePalette.CogAndHammerLaunch,
                    0.3f + (float)Math.Sin(time * 0.04f) * 0.15f);
                Dust d = Dust.NewDustPerfect(pullPos, DustID.IceTorch, inwardVel, 0, default, 0.35f);
                d.noGravity = true;
                d.fadeIn = 0.5f;
            }

            // Occasional brass clockwork accent
            if (Main.rand.NextBool(8))
            {
                Dust brass = Dust.NewDustPerfect(player.Center + Main.rand.NextVector2Circular(16f, 16f),
                    DustID.GoldFlame, Vector2.Zero, 0, default, 0.25f);
                brass.noGravity = true;
            }

            ClairDeLuneVFXLibrary.AddClairDeLuneLight(player.Center, 0.22f);
        }

        public static void PreDrawInWorldBloom(SpriteBatch sb, Texture2D tex, Vector2 pos,
            Vector2 origin, float rotation, float scale)
        {
            if (ClairDeLuneShaderManager.HasSingularityPull)
            {
                ClairDeLuneShaderManager.BeginShaderAdditive(sb);
                ClairDeLuneShaderManager.ApplySingularityCore((float)Main.timeForVisualEffects * 0.03f);

                sb.Draw(tex, pos - Main.screenPosition, null,
                    ClairDeLunePalette.DeepNight * 0.5f, rotation, origin, scale * 1.05f,
                    SpriteEffects.None, 0f);

                ClairDeLuneShaderManager.RestoreSpriteBatch(sb);
            }
            else
            {
                ClairDeLuneVFXLibrary.DrawClairDeLuneBloomStack(sb, pos,
                    ClairDeLunePalette.DeepNight, ClairDeLunePalette.PearlBlue, scale * 0.3f, 0.4f);
            }
        }

        /// <summary>
        /// SIGNATURE BOMB TRAIL: Gravity vortex — particles spiral INWARD toward the bomb
        /// in a rotating vortex, creating a visible gravity well around the projectile.
        /// This is the ONLY Clair de Lune weapon with inward particle flow.
        /// </summary>
        public static void BombTrailVFX(Vector2 bombPos, Vector2 bombVelocity)
        {
            float time = (float)Main.timeForVisualEffects;

            // SIGNATURE: Inward-spiraling vortex around the bomb
            for (int i = 0; i < 2; i++)
            {
                float vortexAngle = time * 0.1f + i * MathHelper.Pi;
                float vortexRadius = 15f + Main.rand.NextFloat(8f);
                Vector2 vortexPos = bombPos + vortexAngle.ToRotationVector2() * vortexRadius;
                Vector2 inwardVel = (bombPos - vortexPos) * 0.07f
                    + new Vector2(-(float)Math.Sin(vortexAngle), (float)Math.Cos(vortexAngle)) * 1.5f;

                Color vortexCol = ClairDeLunePalette.PaletteLerp(
                    ClairDeLunePalette.CogAndHammerLaunch,
                    0.3f + (float)Math.Sin(time * 0.06f + i) * 0.2f);
                Dust d = Dust.NewDustPerfect(vortexPos, DustID.IceTorch, inwardVel, 0, default, 0.5f);
                d.noGravity = true;
                d.fadeIn = 0.7f;
            }

            // Core trail behind the bomb
            if (Main.GameUpdateCount % 2 == 0)
            {
                Dust trail = Dust.NewDustPerfect(bombPos, DustID.GoldFlame,
                    bombVelocity * -0.1f, 0, default, 0.4f);
                trail.noGravity = true;
            }

            // Gravity well glow particles converging
            if (Main.GameUpdateCount % 3 == 0)
            {
                float pullAngle = Main.rand.NextFloat(MathHelper.TwoPi);
                float pullDist = 20f + Main.rand.NextFloat(10f);
                Vector2 pullPos = bombPos + pullAngle.ToRotationVector2() * pullDist;
                var pullGlow = new GenericGlowParticle(
                    pullPos, (bombPos - pullPos) * 0.06f,
                    ClairDeLunePalette.DeepNight * 0.5f, 0.18f, 10, true);
                MagnumParticleHandler.SpawnParticle(pullGlow);
            }

            if (Main.GameUpdateCount % 8 == 0)
                ClairDeLuneVFXLibrary.SpawnMusicNotes(bombPos, 1, 6f, 0.25f, 0.4f, 10);

            ClairDeLuneVFXLibrary.AddPaletteLighting(bombPos, 0.3f, 0.35f);
        }

        /// <summary>
        /// BOMB DETONATION: Gravitational implosion-then-explosion — a two-phase
        /// detonation where particles first rush INWARD (singularity collapse), then
        /// explode OUTWARD in concentric shockwave rings with palette-colored debris.
        /// </summary>
        public static void BombDetonationVFX(Vector2 detonationPos, float radius = 100f)
        {
            // PHASE 1: IMPLOSION — particles from edge converge inward
            int implodeCount = 24;
            for (int i = 0; i < implodeCount; i++)
            {
                float angle = MathHelper.TwoPi * i / implodeCount;
                Vector2 edgePos = detonationPos + angle.ToRotationVector2() * radius;
                Vector2 inwardVel = (detonationPos - edgePos) * 0.06f;

                Color implodeCol = ClairDeLunePalette.PaletteLerp(
                    ClairDeLunePalette.CogAndHammerLaunch, (float)i / implodeCount);
                Dust d = Dust.NewDustPerfect(edgePos, DustID.IceTorch, inwardVel, 0, default, 1.1f);
                d.noGravity = true;
                d.fadeIn = 1.2f;

                // Glow converging particles
                if (i % 2 == 0)
                {
                    var converge = new GenericGlowParticle(edgePos, inwardVel * 1.2f,
                        implodeCol * 0.7f, 0.3f, 14, true);
                    MagnumParticleHandler.SpawnParticle(converge);
                }
            }

            // PHASE 2: EXPLOSION — outward shockwave in 3 concentric rings
            for (int ring = 0; ring < 3; ring++)
            {
                int ringCount = 10 + ring * 4;
                float ringSpeed = 5f + ring * 2.5f;
                float ringDelay = ring * 0.1f; // Visual delay through varying lifetime
                for (int i = 0; i < ringCount; i++)
                {
                    float angle = MathHelper.TwoPi * i / ringCount + ring * 0.2f;
                    Vector2 vel = angle.ToRotationVector2() * ringSpeed;
                    int dustType = (ring == 1) ? DustID.GoldFlame : DustID.IceTorch;
                    float dustScale = 1.1f - ring * 0.15f;
                    Dust d = Dust.NewDustPerfect(detonationPos, dustType, vel, 0, default, dustScale);
                    d.noGravity = true;
                    d.fadeIn = 1.1f;
                }
            }

            // Central gravity-well flash
            var coreFlash = new GenericGlowParticle(detonationPos, Vector2.Zero,
                ClairDeLunePalette.PearlWhite * 0.9f, 0.6f, 10, true);
            MagnumParticleHandler.SpawnParticle(coreFlash);

            ClairDeLuneVFXLibrary.SpawnGradientHaloRings(detonationPos, 5, 0.5f);
            ClairDeLuneVFXLibrary.SpawnConvergingMist(detonationPos, 8, radius * 0.7f, 0.5f);
            ClairDeLuneVFXLibrary.SpawnPearlBurst(detonationPos, 12, 6f, 0.4f);
            ClairDeLuneVFXLibrary.SpawnMusicNotes(detonationPos, 4, 22f, 0.5f, 1f, 30);
            ClairDeLuneVFXLibrary.DrawBloom(detonationPos, 0.85f, 1f);
            ClairDeLuneVFXLibrary.AddPaletteLighting(detonationPos, 0.3f, 1.2f);
        }
    }
}