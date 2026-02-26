using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.VFX.Core;
using MagnumOpus.Common.Systems.VFX.Bloom;
using MagnumOpus.Common.Systems.VFX.Trails;
using MagnumOpus.Common.Systems.VFX.Optimization;
using MagnumOpus.Common.Systems.Shaders;
using MagnumOpus.Content.MoonlightSonata;
using MagnumOpus.Content.MoonlightSonata.Dusts;
using MagnumOpus.Content.SandboxLastPrism.Systems;

namespace MagnumOpus.Content.MoonlightSonata.Weapons.MoonlightsCalling
{
    /// <summary>
    /// Overhauled VFX for Moonlight's Calling — "The Serenade".
    ///
    /// Visual Identity: Musical refraction — light becomes music, music becomes light.
    /// - Prismatic refraction: Beams split through spectral colors with each bounce
    /// - Staff-line motes: LunarMote crescent notes orbiting the beam path
    /// - Spectral shards: PrismaticShardDust that cycle through rainbow hues
    /// - Resonant pulse rings: Expanding ResonantPulseDust on bounce impacts
    /// - Crescendo cascade: Each bounce is more visually dramatic than the last
    /// - PrismaticBeam.fx: GPU-driven spectral color-splitting beam trail
    /// - RefractionRipple.fx: Prismatic expanding ring effects at bounce points
    /// - Serenade Mode: Charging VFX + PrismaticCrescendo mega-beam release
    ///
    /// Unique vs other Moonlight weapons:
    ///   EternalMoon       = flowing water trails (tidal wake, crescent smears)
    ///   Incisor           = surgical precision (constellation nodes, standing waves)
    ///   MoonlightsCalling = MUSICAL REFRACTION (prismatic scatter, spectral cascade, note storms)
    /// </summary>
    public static class MoonlightsCallingVFX
    {
        // === UNIQUE ACCENT COLORS — prismatic refraction palette ===
        public static readonly Color PrismViolet = new Color(160, 80, 255);
        public static readonly Color RefractedBlue = new Color(100, 200, 255);
        public static readonly Color SpectralCyan = new Color(110, 220, 255);
        public static readonly Color TomeSilver = new Color(190, 200, 255);
        public static readonly Color RefractionLavender = new Color(180, 130, 255);

        /// <summary>
        /// Returns a refraction color that cycles through a spectral range.
        /// More bounces = wider spectral range (more colors visible).
        /// 0 bounces: purple-blue only. Max bounces: full rainbow.
        /// </summary>
        public static Color GetRefractionColor(float progress, int bounceCount)
        {
            float hueRange = MathHelper.Clamp(0.15f + bounceCount * 0.08f, 0.15f, 0.5f);
            float baseHue = 0.7f; // Start at purple
            float hue = (baseHue + progress * hueRange) % 1f;
            return Main.hslToRgb(hue, 0.8f, 0.7f);
        }

        /// <summary>
        /// Trail color function — prismatic gradient that shifts with bounce count.
        /// </summary>
        public static Color BeamTrailColor(float progress, int bounceCount)
        {
            float bounceShift = bounceCount * 0.08f;
            Color start = Color.Lerp(MoonlightVFXLibrary.DarkPurple, PrismViolet, bounceShift);
            Color mid = Color.Lerp(MoonlightVFXLibrary.Violet, RefractedBlue, bounceShift);
            Color end = Color.Lerp(MoonlightVFXLibrary.IceBlue, MoonlightVFXLibrary.MoonWhite, bounceShift);

            Color baseColor;
            if (progress < 0.5f)
                baseColor = Color.Lerp(start, mid, progress * 2f);
            else
                baseColor = Color.Lerp(mid, end, (progress - 0.5f) * 2f);

            return baseColor * (1f - progress * 0.6f);
        }

        /// <summary>
        /// Trail width — wider with more bounces (crescendo effect).
        /// </summary>
        public static float BeamTrailWidth(float progress, int bounceCount)
        {
            float baseWidth = 12f + bounceCount * 2f;
            float taper = 1f - progress * progress;
            return baseWidth * taper;
        }

        // =====================================================================
        //  BEAM TRAIL FRAME VFX (called every frame in beam AI)
        // =====================================================================

        /// <summary>
        /// Per-frame beam trail effects using custom dust types.
        /// PrismaticShardDust for spectral sparkles, LunarMote for crescent notes,
        /// StarPointDust for sharp twinkles.
        /// </summary>
        public static void BeamTrailFrame(Vector2 beamPos, Vector2 velocity, int bounceCount)
        {
            float bounceIntensity = 1f + bounceCount * 0.25f;

            // PRISMATIC SHARD TRAIL — spectral dust cycling through colors
            if (Main.rand.NextBool(2))
            {
                Vector2 shardVel = -velocity * 0.08f + Main.rand.NextVector2Circular(1f, 1f);
                Color shardColor = GetRefractionColor(Main.rand.NextFloat(), bounceCount);
                Dust shard = Dust.NewDustPerfect(
                    beamPos + Main.rand.NextVector2Circular(6f, 6f),
                    ModContent.DustType<PrismaticShardDust>(),
                    shardVel, 0, shardColor, 0.2f * bounceIntensity);
                shard.customData = new PrismaticShardBehavior
                {
                    BaseHue = 0.7f + bounceCount * 0.05f,
                    HueRange = 0.15f + bounceCount * 0.06f,
                    VelocityDecay = 0.94f,
                    RotationSpeed = 0.1f,
                    BaseScale = 0.2f * bounceIntensity,
                    Lifetime = 22 + bounceCount * 3
                };
            }

            // CRESCENT MOTE — LunarMote orbiting behind beam like musical notes
            if (Main.rand.NextBool(5))
            {
                float orbitAngle = Main.GameUpdateCount * 0.1f + Main.rand.NextFloat(MathHelper.TwoPi);
                Color moteColor = Color.Lerp(MoonlightVFXLibrary.Violet,
                    RefractedBlue, Main.rand.NextFloat());
                Dust mote = Dust.NewDustPerfect(beamPos,
                    ModContent.DustType<LunarMote>(),
                    -velocity * 0.03f,
                    0, moteColor, 0.25f * bounceIntensity);
                mote.customData = new LunarMoteBehavior(beamPos, orbitAngle)
                {
                    OrbitRadius = 8f + bounceCount * 2f,
                    OrbitSpeed = 0.12f,
                    Lifetime = 20,
                    FadePower = 0.9f
                };
            }

            // STAR POINT TWINKLE — sharp sparkles along beam path
            if (Main.rand.NextBool(4))
            {
                Color starColor = Color.Lerp(PrismViolet, MoonlightVFXLibrary.MoonWhite,
                    Main.rand.NextFloat(0.5f));
                Dust star = Dust.NewDustPerfect(
                    beamPos + Main.rand.NextVector2Circular(4f, 4f),
                    ModContent.DustType<StarPointDust>(),
                    -velocity * 0.05f, 0, starColor, 0.15f * bounceIntensity);
                star.customData = new StarPointBehavior
                {
                    RotationSpeed = 0.12f,
                    TwinkleFrequency = 0.5f,
                    Lifetime = 18,
                    FadeStartTime = 5
                };
            }

            // Music notes orbiting beam — the defining visual
            if (Main.rand.NextBool(5))
            {
                float angle = Main.GameUpdateCount * 0.1f;
                Vector2 notePos = beamPos + new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * 10f;
                MoonlightVFXLibrary.SpawnMusicNotes(notePos, 1, 3f, 0.7f, 0.85f, 25);
            }

            // Dynamic prismatic lighting
            Vector3 lightVec = Color.Lerp(MoonlightVFXLibrary.Violet, RefractedBlue,
                MathF.Sin(Main.GlobalTimeWrappedHourly * 4f) * 0.5f + 0.5f).ToVector3();
            Lighting.AddLight(beamPos, lightVec * (0.6f + bounceCount * 0.15f));
        }

        // =====================================================================
        //  BOUNCE VFX (escalating prismatic explosion)
        // =====================================================================

        /// <summary>
        /// Bounce VFX — prismatic explosion that grows with each successive bounce.
        /// Uses ResonantPulseDust for expanding rings, PrismaticShardDust for spectral scatter,
        /// LunarMote for crescent flares. GodRaySystem burst on bounces 3+.
        /// </summary>
        public static void OnBounceVFX(Vector2 bouncePos, Vector2 outgoingVelocity, int bounceCount)
        {
            float intensity = 0.7f + bounceCount * 0.3f;

            // Central flash cascade
            CustomParticles.GenericFlare(bouncePos, Color.White, 0.5f * intensity, 15);
            CustomParticles.GenericFlare(bouncePos, PrismViolet, 0.4f * intensity, 18);
            if (bounceCount >= 2)
                CustomParticles.GenericFlare(bouncePos, RefractedBlue, 0.3f * intensity, 16);

            // RESONANT PULSE RINGS — expanding spectral rings
            int ringCount = 2 + bounceCount;
            for (int ring = 0; ring < ringCount; ring++)
            {
                Color ringColor = GetRefractionColor((float)ring / ringCount, bounceCount);
                Dust pulse = Dust.NewDustPerfect(bouncePos,
                    ModContent.DustType<ResonantPulseDust>(),
                    Vector2.Zero, 0, ringColor,
                    0.2f + ring * 0.05f + bounceCount * 0.04f);
                pulse.customData = new ResonantPulseBehavior
                {
                    ExpansionRate = 0.03f + ring * 0.008f,
                    ExpansionDecay = 0.95f,
                    Lifetime = 16 + ring * 3,
                    PulseFrequency = 0.3f
                };
            }

            // PRISMATIC SHARD SCATTER — spectral rays emanating from bounce
            int rayCount = 5 + bounceCount * 2;
            for (int i = 0; i < rayCount; i++)
            {
                float angle = MathHelper.TwoPi * i / rayCount;
                Vector2 rayVel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 6f) * intensity;
                Color rayColor = GetRefractionColor((float)i / rayCount, bounceCount);
                Dust shard = Dust.NewDustPerfect(bouncePos,
                    ModContent.DustType<PrismaticShardDust>(),
                    rayVel, 0, rayColor, 0.25f * intensity);
                shard.customData = new PrismaticShardBehavior
                {
                    BaseHue = 0.7f + (float)i / rayCount * 0.3f,
                    HueRange = 0.1f + bounceCount * 0.05f,
                    RotationSpeed = 0.15f,
                    BaseScale = 0.25f * intensity,
                    Lifetime = 22 + bounceCount * 2
                };
            }

            // LUNAR MOTE CRESCENT FLARES at bounce point
            for (int i = 0; i < 2 + bounceCount; i++)
            {
                float moteAngle = MathHelper.TwoPi * i / (2 + bounceCount);
                Color moteColor = Color.Lerp(PrismViolet, RefractedBlue, (float)i / (2 + bounceCount));
                Dust mote = Dust.NewDustPerfect(bouncePos,
                    ModContent.DustType<LunarMote>(),
                    moteAngle.ToRotationVector2() * 1.5f,
                    0, moteColor, 0.3f * intensity);
                mote.customData = new LunarMoteBehavior(bouncePos, moteAngle)
                {
                    OrbitRadius = 15f + bounceCount * 3f,
                    OrbitSpeed = 0.08f,
                    Lifetime = 25,
                    FadePower = 0.92f
                };
            }

            // Gradient halo rings
            CustomParticles.HaloRing(bouncePos, PrismViolet, 0.3f * intensity, 16);
            CustomParticles.HaloRing(bouncePos, RefractedBlue, 0.25f * intensity, 20);

            // Music note burst — crescendo with bounces
            MoonlightVFXLibrary.SpawnMusicNotes(bouncePos, 2 + bounceCount, 15f * intensity, 0.75f, 1.0f, 30);

            // Refraction splinter trails
            for (int i = 0; i < 3 + bounceCount; i++)
            {
                Vector2 splinterVel = outgoingVelocity.SafeNormalize(Vector2.Zero).RotatedByRandom(0.5f) * Main.rand.NextFloat(2f, 5f);
                Color splinterColor = GetRefractionColor(Main.rand.NextFloat(), bounceCount) with { A = 0 };
                CustomParticles.TrailSegment(bouncePos, splinterVel, splinterColor, 0.25f * intensity);
            }

            // GodRaySystem burst on bounces 3+ — escalating drama
            if (bounceCount >= 3)
            {
                Color godRayColor = Color.Lerp(PrismViolet, RefractedBlue, (bounceCount - 3) * 0.3f);
                GodRaySystem.CreateBurst(bouncePos, godRayColor, 4 + bounceCount, 40f + bounceCount * 10f, 20,
                    GodRaySystem.GodRayStyle.Explosion, MoonlightVFXLibrary.IceBlue);
            }

            Lighting.AddLight(bouncePos, RefractedBlue.ToVector3() * intensity);
        }

        // =====================================================================
        //  GRAND FINALE (all bounces exhausted)
        // =====================================================================

        /// <summary>
        /// Grand finale — full spectral detonation when beam exhausts all bounces.
        /// Massive prismatic explosion + custom dust storms + god rays + screen effects.
        /// </summary>
        public static void OnBeamFinale(Vector2 deathPos, int totalBounces)
        {
            float intensity = 1f + totalBounces * 0.2f;

            // MASSIVE FLARE CASCADE
            CustomParticles.GenericFlare(deathPos, Color.White, 0.9f * intensity, 22);
            CustomParticles.GenericFlare(deathPos, PrismViolet, 0.7f * intensity, 20);
            CustomParticles.GenericFlare(deathPos, RefractedBlue, 0.6f * intensity, 18);
            CustomParticles.GenericFlare(deathPos, SpectralCyan, 0.45f * intensity, 16);
            CustomParticles.GenericFlare(deathPos, TomeSilver, 0.35f * intensity, 14);

            // RESONANT PULSE RING CASCADE — 5 expanding spectral rings
            for (int i = 0; i < 5; i++)
            {
                Color ringColor = GetRefractionColor(i / 5f, totalBounces);
                Dust pulse = Dust.NewDustPerfect(deathPos,
                    ModContent.DustType<ResonantPulseDust>(),
                    Vector2.Zero, 0, ringColor,
                    0.25f + i * 0.08f);
                pulse.customData = new ResonantPulseBehavior
                {
                    ExpansionRate = 0.04f + i * 0.012f,
                    ExpansionDecay = 0.94f,
                    Lifetime = 18 + i * 4,
                    PulseFrequency = 0.2f + i * 0.05f
                };
            }

            // PRISMATIC SHARD STARBURST — 16 shards radiating outward
            for (int i = 0; i < 16; i++)
            {
                float angle = MathHelper.TwoPi * i / 16f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 8f);
                Color shardColor = GetRefractionColor((float)i / 16f, totalBounces);
                Dust shard = Dust.NewDustPerfect(deathPos,
                    ModContent.DustType<PrismaticShardDust>(),
                    vel, 0, shardColor, 0.35f);
                shard.customData = new PrismaticShardBehavior
                {
                    BaseHue = (float)i / 16f,
                    HueRange = 0.5f,
                    RotationSpeed = 0.12f,
                    BaseScale = 0.35f,
                    Lifetime = 35
                };
            }

            // LUNAR MOTE ORBIT — 6 crescent motes spiraling outward
            for (int i = 0; i < 6; i++)
            {
                float moteAngle = MathHelper.TwoPi * i / 6f;
                Color moteColor = Color.Lerp(PrismViolet, MoonlightVFXLibrary.MoonWhite, (float)i / 6f);
                Dust mote = Dust.NewDustPerfect(deathPos + moteAngle.ToRotationVector2() * 10f,
                    ModContent.DustType<LunarMote>(),
                    moteAngle.ToRotationVector2() * 2f,
                    0, moteColor, 0.45f);
                mote.customData = new LunarMoteBehavior(deathPos, moteAngle)
                {
                    OrbitRadius = 25f + i * 5f,
                    OrbitSpeed = 0.06f,
                    Lifetime = 40,
                    FadePower = 0.93f
                };
            }

            // STAR POINT BURST — 10 sharp twinkles
            for (int i = 0; i < 10; i++)
            {
                float angle = MathHelper.TwoPi * i / 10f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 5f);
                Color starColor = Color.Lerp(TomeSilver, RefractionLavender, Main.rand.NextFloat());
                Dust star = Dust.NewDustPerfect(deathPos,
                    ModContent.DustType<StarPointDust>(),
                    vel, 0, starColor, 0.3f);
                star.customData = new StarPointBehavior
                {
                    RotationSpeed = 0.15f,
                    TwinkleFrequency = 0.5f,
                    Lifetime = 30,
                    FadeStartTime = 8
                };
            }

            // Halo ring cascade (spectral rainbow)
            for (int i = 0; i < 5; i++)
            {
                Color haloColor = GetRefractionColor(i / 5f, totalBounces);
                CustomParticles.HaloRing(deathPos, haloColor, 0.3f + i * 0.12f, 16 + i * 5);
            }

            // Moonlight lightning fractals
            for (int i = 0; i < 4; i++)
            {
                float lightningAngle = MathHelper.TwoPi * i / 4f + Main.rand.NextFloat(-0.3f, 0.3f);
                Vector2 lightningEnd = deathPos + lightningAngle.ToRotationVector2() * 70f;
                MagnumVFX.DrawMoonlightLightning(deathPos, lightningEnd, 5, 18f, 2, 0.35f);
            }

            // Music note finale cascade
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 noteOffset = angle.ToRotationVector2() * 8f;
                MoonlightVFXLibrary.SpawnMusicNotes(deathPos + noteOffset, 1, 8f, 0.9f, 1.1f, 40);
            }

            // GOD RAY BURST — grand finale
            GodRaySystem.CreateBurst(deathPos, PrismViolet, 8, 80f, 30,
                GodRaySystem.GodRayStyle.Explosion, RefractedBlue);

            // SCREEN EFFECTS
            if (AdaptiveQualityManager.Instance?.CurrentQuality >= AdaptiveQualityManager.QualityLevel.Medium)
            {
                ScreenDistortionManager.TriggerRipple(deathPos, PrismViolet, 0.5f, 22);
                MagnumScreenEffects.AddScreenShake(4f);
            }

            // CHROMATIC ABERRATION FLASH
            try
            {
                SLPFlashSystem.SetCAFlashEffect(
                    intensity: 0.2f,
                    lifetime: 15,
                    whiteIntensity: 0.5f,
                    distanceMult: 0.4f,
                    moveIn: true);
            }
            catch { }

            // Spectral arc burst
            CustomParticles.SwordArcBurst(deathPos, PrismViolet, 8, 0.5f);

            Lighting.AddLight(deathPos, MoonlightVFXLibrary.MoonWhite.ToVector3() * intensity);
        }

        // =====================================================================
        //  BEAM BODY BLOOM
        // =====================================================================

        /// <summary>
        /// Beam body bloom — prismatic 5-layer bloom stack using {A=0}.
        /// Color cycles through spectral range based on bounce count.
        /// </summary>
        public static void DrawBeamBloom(SpriteBatch sb, Vector2 beamWorldPos, int bounceCount)
        {
            if (sb == null) return;

            Vector2 drawPos = beamWorldPos - Main.screenPosition;
            float pulse = 1f + MathF.Sin(Main.GlobalTimeWrappedHourly * 6f) * 0.1f;
            float bounceScale = 1f + bounceCount * 0.15f;

            var bloomTex = MagnumTextureRegistry.GetSoftGlow();
            if (bloomTex == null) return;
            Vector2 origin = bloomTex.Size() * 0.5f;

            // Layer 1: Outer prismatic glow (color cycles with time + bounces)
            Color outerColor = GetRefractionColor(Main.GlobalTimeWrappedHourly % 1f, bounceCount);
            sb.Draw(bloomTex, drawPos, null,
                (outerColor with { A = 0 }) * 0.2f,
                0f, origin, 0.65f * bounceScale * pulse, SpriteEffects.None, 0f);

            // Layer 2: DarkPurple mid halo
            sb.Draw(bloomTex, drawPos, null,
                (MoonlightVFXLibrary.DarkPurple with { A = 0 }) * 0.25f,
                0f, origin, 0.5f * bounceScale * pulse, SpriteEffects.None, 0f);

            // Layer 3: Prism violet
            sb.Draw(bloomTex, drawPos, null,
                (PrismViolet with { A = 0 }) * 0.40f,
                0f, origin, 0.35f * bounceScale * pulse, SpriteEffects.None, 0f);

            // Layer 4: Refracted blue inner
            sb.Draw(bloomTex, drawPos, null,
                (RefractedBlue with { A = 0 }) * 0.55f,
                0f, origin, 0.22f * bounceScale * pulse, SpriteEffects.None, 0f);

            // Layer 5: White-hot core
            sb.Draw(bloomTex, drawPos, null,
                (Color.White with { A = 0 }) * 0.70f,
                0f, origin, 0.10f * bounceScale * pulse, SpriteEffects.None, 0f);
        }

        // =====================================================================
        //  MUZZLE FLASH
        // =====================================================================

        /// <summary>
        /// Enhanced muzzle flash when firing from the tome.
        /// PrismaticShardDust burst + ResonantPulseDust ring + music notes.
        /// </summary>
        public static void MuzzleFlash(Vector2 firePos, Vector2 direction)
        {
            // Layered central flash
            CustomParticles.GenericFlare(firePos, Color.White * 0.6f, 0.5f, 15);
            CustomParticles.GenericFlare(firePos, PrismViolet, 0.42f, 14);
            CustomParticles.GenericFlare(firePos, MoonlightVFXLibrary.DarkPurple * 0.6f, 0.35f, 12);

            // Directional PrismaticShardDust burst
            for (int i = 0; i < 6; i++)
            {
                Vector2 vel = direction.RotatedByRandom(0.4f) * Main.rand.NextFloat(3f, 7f);
                Color shardColor = GetRefractionColor((float)i / 6f, 0);
                Dust shard = Dust.NewDustPerfect(firePos,
                    ModContent.DustType<PrismaticShardDust>(),
                    vel, 0, shardColor, 0.22f);
                shard.customData = new PrismaticShardBehavior(0.7f + i * 0.03f, 0.15f, 20);
            }

            // ResonantPulseDust ring at muzzle
            Dust pulse = Dust.NewDustPerfect(firePos,
                ModContent.DustType<ResonantPulseDust>(),
                Vector2.Zero, 0, PrismViolet, 0.2f);
            pulse.customData = new ResonantPulseBehavior(0.035f, 12);

            // Cascading halo rings
            for (int i = 0; i < 3; i++)
            {
                Color ringColor = Color.Lerp(MoonlightVFXLibrary.DarkPurple, MoonlightVFXLibrary.IceBlue, i / 3f);
                CustomParticles.HaloRing(firePos, ringColor * (0.5f - i * 0.1f), 0.22f + i * 0.08f, 10 + i * 2);
            }

            // Music notes from the tome
            MoonlightVFXLibrary.SpawnMusicNotes(firePos, 2, 10f, 0.7f, 0.9f, 25);
        }

        // =====================================================================
        //  ON-HIT IMPACT
        // =====================================================================

        /// <summary>
        /// Impact VFX when beam hits an NPC — prismatic shattering using custom dusts.
        /// </summary>
        public static void OnHitImpact(Vector2 hitPos, int bounceCount, bool crit)
        {
            float intensity = 0.7f + bounceCount * 0.15f;

            // Central flash
            CustomParticles.GenericFlare(hitPos, Color.White, 0.6f * intensity, 16);
            CustomParticles.GenericFlare(hitPos, PrismViolet, 0.5f * intensity, 14);

            // Base moonlight impact
            MoonlightVFXLibrary.ProjectileImpact(hitPos, 0.7f * intensity);

            // RESONANT PULSE RINGS on hit
            for (int ring = 0; ring < 2 + (bounceCount / 2); ring++)
            {
                float progress = (float)ring / (2 + bounceCount / 2);
                Color ringColor = Color.Lerp(PrismViolet, RefractedBlue, progress);
                Dust pulse = Dust.NewDustPerfect(hitPos,
                    ModContent.DustType<ResonantPulseDust>(),
                    Vector2.Zero, 0, ringColor,
                    0.2f + ring * 0.05f);
                pulse.customData = new ResonantPulseBehavior
                {
                    ExpansionRate = 0.03f + ring * 0.008f,
                    ExpansionDecay = 0.95f,
                    Lifetime = 14 + ring * 3,
                    PulseFrequency = 0.3f
                };
            }

            // Gradient halo rings
            for (int ring = 0; ring < 3; ring++)
            {
                float progress = ring / 3f;
                Color haloColor = Color.Lerp(PrismViolet, RefractedBlue, progress);
                CustomParticles.HaloRing(hitPos, haloColor, 0.25f + ring * 0.1f, 14 + ring * 4);
            }

            // PRISMATIC SHARD SPRAY
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f + Main.rand.NextFloat(-0.2f, 0.2f);
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 6f);
                Color dustColor = GetRefractionColor((float)i / 6f, bounceCount);
                Dust shard = Dust.NewDustPerfect(hitPos,
                    ModContent.DustType<PrismaticShardDust>(),
                    vel, 0, dustColor, 0.2f * intensity);
                shard.customData = new PrismaticShardBehavior(0.7f + (float)i / 6f * 0.3f, 0.2f, 20);
            }

            // Music notes on hit
            MoonlightVFXLibrary.SpawnMusicNotes(hitPos, 2, 15f, 0.75f, 0.9f, 28);

            // GodRaySystem burst on crits
            if (crit)
            {
                GodRaySystem.CreateBurst(hitPos, PrismViolet, 4 + bounceCount, 40f, 18,
                    GodRaySystem.GodRayStyle.Explosion, RefractedBlue);

                MagnumScreenEffects.AddScreenShake(1.5f + bounceCount * 0.5f);
            }

            Lighting.AddLight(hitPos, RefractedBlue.ToVector3() * intensity);
        }

        // =====================================================================
        //  SPECTRAL SPLIT VFX (when beam spawns child beams)
        // =====================================================================

        /// <summary>
        /// VFX for spectral beam splitting — prism refraction effect.
        /// Called when a beam splits into child spectral beams on later bounces.
        /// </summary>
        public static void SpectralSplitVFX(Vector2 splitPos, int bounceCount)
        {
            float intensity = 0.6f + bounceCount * 0.15f;

            // Central refraction flash
            CustomParticles.GenericFlare(splitPos, Color.White, 0.4f * intensity, 12);
            CustomParticles.GenericFlare(splitPos, PrismViolet, 0.35f * intensity, 14);

            // Expanding refraction ring
            Dust pulse = Dust.NewDustPerfect(splitPos,
                ModContent.DustType<ResonantPulseDust>(),
                Vector2.Zero, 0, TomeSilver, 0.2f);
            pulse.customData = new ResonantPulseBehavior(0.04f, 14);

            // Prismatic shard scatter from split point
            for (int i = 0; i < 5; i++)
            {
                float angle = MathHelper.TwoPi * i / 5f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 4f);
                Color shardColor = GetRefractionColor((float)i / 5f, bounceCount);
                Dust shard = Dust.NewDustPerfect(splitPos,
                    ModContent.DustType<PrismaticShardDust>(),
                    vel, 0, shardColor, 0.2f);
                shard.customData = new PrismaticShardBehavior(
                    (float)i / 5f, 0.3f, 18);
            }

            MoonlightVFXLibrary.SpawnMusicNotes(splitPos, 1, 8f, 0.7f, 0.9f, 20);
        }

        /// <summary>
        /// Dynamic lighting for prismatic beam — pulsing spectral glow.
        /// </summary>
        public static void AddPrismaticLight(Vector2 worldPos, float intensity = 0.7f, int bounceCount = 0)
        {
            float pulse = 0.85f + MathF.Sin(Main.GlobalTimeWrappedHourly * 5f) * 0.15f;
            Color lightColor = Color.Lerp(MoonlightVFXLibrary.Violet, RefractedBlue,
                MathF.Sin(Main.GlobalTimeWrappedHourly * 3f + bounceCount) * 0.5f + 0.5f);
            Lighting.AddLight(worldPos, lightColor.ToVector3() * intensity * pulse);
        }

        // =====================================================================
        //  SHADER-DRIVEN VFX — RefractionRipple.fx at Bounce Points
        // =====================================================================

        /// <summary>
        /// Draws a RefractionRipple.fx shader-driven prismatic expanding ring at a bounce/death point.
        /// Uses the shader for rich spectral ring effects when available, falls back to particle-only.
        /// Called from MoonlightBeam.OnTileCollide at the grand finale.
        /// </summary>
        public static void DrawRefractionRippleBurst(Vector2 burstPos, int bounceCount)
        {
            if (!MoonlightSonataShaderManager.HasRefractionRipple)
            {
                // Fallback: extra particle rings when shader unavailable
                for (int i = 0; i < 3; i++)
                {
                    Color ringColor = GetRefractionColor(i / 3f, bounceCount);
                    CustomParticles.HaloRing(burstPos, ringColor, 0.4f + i * 0.15f, 18 + i * 5);
                }
                return;
            }

            SpriteBatch sb = Main.spriteBatch;
            var glowTex = MoonlightSonataTextures.BloomOrb?.Value
                       ?? MagnumTextureRegistry.GetSoftGlow();
            if (glowTex == null) return;

            Vector2 drawPos = burstPos - Main.screenPosition;
            Vector2 origin = glowTex.Size() * 0.5f;
            float bounceIntensity = MathHelper.Clamp(bounceCount * 0.25f, 0.5f, 2f);

            try
            {
                MoonlightSonataShaderManager.BeginShaderBatch(sb);

                // Main refraction ripple — full prismatic ring
                MoonlightSonataShaderManager.ApplyMoonlightsCallingRefractionRipple(
                    Main.GlobalTimeWrappedHourly, 0.1f, bounceIntensity);

                sb.Draw(glowTex, drawPos, null,
                    Color.White, 0f, origin,
                    0.6f + bounceCount * 0.15f, SpriteEffects.None, 0f);

                // Second ripple ring — slightly larger, offset in time
                MoonlightSonataShaderManager.ApplyMoonlightsCallingRefractionRipple(
                    Main.GlobalTimeWrappedHourly + 0.2f, 0.25f, bounceIntensity * 0.7f);

                sb.Draw(glowTex, drawPos, null,
                    Color.White * 0.6f, 0f, origin,
                    0.8f + bounceCount * 0.2f, SpriteEffects.None, 0f);

                MoonlightSonataShaderManager.RestoreDefaultBatch(sb);
            }
            catch
            {
                try { MoonlightSonataShaderManager.RestoreDefaultBatch(sb); } catch { }

                // Fallback additive bloom
                sb.Draw(glowTex, drawPos, null,
                    MoonlightSonataPalette.Additive(PrismViolet, 0.35f),
                    0f, origin, 0.5f, SpriteEffects.None, 0f);
            }
        }

        // =====================================================================
        //  SERENADE MODE VFX — Charging + Release
        // =====================================================================

        /// <summary>
        /// Per-frame VFX for Serenade Mode charging state.
        /// Orbiting prismatic dust converges toward the tome with escalating intensity.
        /// chargeProgress = 0-1 (0 = just started, 1 = fully charged).
        /// </summary>
        public static void SerenadeChargeVFX(Vector2 tomePos, Vector2 aimDirection, float chargeProgress)
        {
            float intensity = 0.3f + chargeProgress * 0.7f;

            // Converging PrismaticShardDust — orbit inward as charge builds
            if (Main.rand.NextBool(Math.Max(1, 4 - (int)(chargeProgress * 3f))))
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                float radius = 40f * (1f - chargeProgress * 0.6f);
                Vector2 spawnPos = tomePos + angle.ToRotationVector2() * radius;
                Vector2 vel = (tomePos - spawnPos).SafeNormalize(Vector2.Zero) * (2f + chargeProgress * 3f);
                Color shardColor = GetRefractionColor(Main.rand.NextFloat(), (int)(chargeProgress * 5f));
                Dust shard = Dust.NewDustPerfect(spawnPos,
                    ModContent.DustType<PrismaticShardDust>(),
                    vel, 0, shardColor, 0.2f + chargeProgress * 0.15f);
                shard.customData = new PrismaticShardBehavior
                {
                    BaseHue = Main.rand.NextFloat(),
                    HueRange = 0.2f + chargeProgress * 0.3f,
                    VelocityDecay = 0.92f,
                    RotationSpeed = 0.12f,
                    BaseScale = 0.2f + chargeProgress * 0.15f,
                    Lifetime = (int)(15 + chargeProgress * 10f)
                };
            }

            // Orbiting LunarMote crescents — tightening orbit
            if (Main.rand.NextBool(Math.Max(1, 5 - (int)(chargeProgress * 4f))))
            {
                float orbitAngle = Main.GlobalTimeWrappedHourly * 3f + Main.rand.NextFloat(MathHelper.TwoPi);
                Color moteColor = Color.Lerp(PrismViolet, SpectralCyan, Main.rand.NextFloat());
                Dust mote = Dust.NewDustPerfect(tomePos,
                    ModContent.DustType<LunarMote>(),
                    Vector2.Zero, 0, moteColor, 0.25f + chargeProgress * 0.1f);
                mote.customData = new LunarMoteBehavior(tomePos, orbitAngle)
                {
                    OrbitRadius = 20f - chargeProgress * 10f,
                    OrbitSpeed = 0.1f + chargeProgress * 0.15f,
                    Lifetime = 20,
                    FadePower = 0.9f
                };
            }

            // StarPointDust sparkles converging
            if (chargeProgress > 0.3f && Main.rand.NextBool(3))
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                float radius = 25f * (1f - chargeProgress * 0.5f);
                Vector2 starPos = tomePos + angle.ToRotationVector2() * radius;
                Color starColor = Color.Lerp(TomeSilver, MoonlightVFXLibrary.MoonWhite, Main.rand.NextFloat(0.5f));
                Dust star = Dust.NewDustPerfect(starPos,
                    ModContent.DustType<StarPointDust>(),
                    (tomePos - starPos).SafeNormalize(Vector2.Zero) * 1.5f,
                    0, starColor, 0.15f + chargeProgress * 0.08f);
                star.customData = new StarPointBehavior
                {
                    RotationSpeed = 0.15f,
                    TwinkleFrequency = 0.5f,
                    Lifetime = 16,
                    FadeStartTime = 4
                };
            }

            // Music notes spiral inward at high charge
            if (chargeProgress > 0.5f && Main.rand.NextBool(4))
            {
                float noteAngle = Main.GlobalTimeWrappedHourly * 4f;
                Vector2 notePos = tomePos + new Vector2(MathF.Cos(noteAngle), MathF.Sin(noteAngle)) * 15f;
                MoonlightVFXLibrary.SpawnMusicNotes(notePos, 1, 3f, 0.7f + chargeProgress * 0.3f, 0.9f, 25);
            }

            // ResonantPulseDust expanding ring every ~12 frames at high charge
            if (chargeProgress > 0.6f && Main.GameUpdateCount % 12 == 0)
            {
                Color ringColor = GetRefractionColor((Main.GameUpdateCount * 0.03f) % 1f, 3);
                Dust pulse = Dust.NewDustPerfect(tomePos,
                    ModContent.DustType<ResonantPulseDust>(),
                    Vector2.Zero, 0, ringColor,
                    0.15f + chargeProgress * 0.1f);
                pulse.customData = new ResonantPulseBehavior
                {
                    ExpansionRate = 0.025f + chargeProgress * 0.01f,
                    ExpansionDecay = 0.95f,
                    Lifetime = 14,
                    PulseFrequency = 0.3f
                };
            }

            // Directional aim indicator — faint spectral line toward cursor
            if (chargeProgress > 0.4f)
            {
                Vector2 indicatorPos = tomePos + aimDirection * (20f + chargeProgress * 15f);
                Color indicatorColor = Color.Lerp(PrismViolet, RefractedBlue, chargeProgress) with { A = 0 };
                CustomParticles.TrailSegment(tomePos, aimDirection * 15f, indicatorColor, 0.15f * chargeProgress);
            }

            // Prismatic lighting at tome — intensifies with charge
            Lighting.AddLight(tomePos, Color.Lerp(PrismViolet, RefractedBlue,
                MathF.Sin(Main.GlobalTimeWrappedHourly * 4f) * 0.5f + 0.5f).ToVector3()
                * intensity);
        }

        /// <summary>
        /// One-shot VFX burst when Serenade Mode releases the PrismaticCrescendo beam.
        /// Massive prismatic nova at the firing point.
        /// </summary>
        public static void SerenadeReleaseVFX(Vector2 firePos, Vector2 direction)
        {
            // Massive layered flash
            CustomParticles.GenericFlare(firePos, Color.White, 0.8f, 20);
            CustomParticles.GenericFlare(firePos, PrismViolet, 0.65f, 18);
            CustomParticles.GenericFlare(firePos, RefractedBlue, 0.5f, 16);
            CustomParticles.GenericFlare(firePos, SpectralCyan, 0.35f, 14);

            // Directional PrismaticShardDust burst — cone of spectral shards
            for (int i = 0; i < 10; i++)
            {
                Vector2 vel = direction.RotatedByRandom(0.5f) * Main.rand.NextFloat(5f, 10f);
                Color shardColor = GetRefractionColor((float)i / 10f, 5);
                Dust shard = Dust.NewDustPerfect(firePos,
                    ModContent.DustType<PrismaticShardDust>(),
                    vel, 0, shardColor, 0.3f);
                shard.customData = new PrismaticShardBehavior
                {
                    BaseHue = (float)i / 10f,
                    HueRange = 0.4f,
                    RotationSpeed = 0.12f,
                    BaseScale = 0.3f,
                    Lifetime = 25
                };
            }

            // Radial StarPointDust burst
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 6f);
                Color starColor = Color.Lerp(TomeSilver, MoonlightVFXLibrary.MoonWhite, Main.rand.NextFloat());
                Dust star = Dust.NewDustPerfect(firePos,
                    ModContent.DustType<StarPointDust>(),
                    vel, 0, starColor, 0.25f);
                star.customData = new StarPointBehavior
                {
                    RotationSpeed = 0.15f,
                    TwinkleFrequency = 0.5f,
                    Lifetime = 22,
                    FadeStartTime = 6
                };
            }

            // ResonantPulseDust ring cascade
            for (int i = 0; i < 3; i++)
            {
                Color ringColor = GetRefractionColor(i / 3f, 5);
                Dust pulse = Dust.NewDustPerfect(firePos,
                    ModContent.DustType<ResonantPulseDust>(),
                    Vector2.Zero, 0, ringColor,
                    0.25f + i * 0.08f);
                pulse.customData = new ResonantPulseBehavior
                {
                    ExpansionRate = 0.04f + i * 0.01f,
                    ExpansionDecay = 0.94f,
                    Lifetime = 16 + i * 4,
                    PulseFrequency = 0.25f
                };
            }

            // LunarMote crescent burst — spiraling outward
            for (int i = 0; i < 4; i++)
            {
                float moteAngle = MathHelper.TwoPi * i / 4f;
                Color moteColor = Color.Lerp(PrismViolet, SpectralCyan, (float)i / 4f);
                Dust mote = Dust.NewDustPerfect(firePos + moteAngle.ToRotationVector2() * 6f,
                    ModContent.DustType<LunarMote>(),
                    moteAngle.ToRotationVector2() * 2f,
                    0, moteColor, 0.35f);
                mote.customData = new LunarMoteBehavior(firePos, moteAngle)
                {
                    OrbitRadius = 20f + i * 5f,
                    OrbitSpeed = 0.08f,
                    Lifetime = 30,
                    FadePower = 0.92f
                };
            }

            // Halo ring cascade
            for (int i = 0; i < 4; i++)
            {
                Color haloColor = GetRefractionColor(i / 4f, 5);
                CustomParticles.HaloRing(firePos, haloColor, 0.3f + i * 0.1f, 14 + i * 4);
            }

            // Music note crescendo burst
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f;
                Vector2 noteOffset = angle.ToRotationVector2() * 6f;
                MoonlightVFXLibrary.SpawnMusicNotes(firePos + noteOffset, 1, 8f, 0.9f, 1.1f, 35);
            }

            // God ray burst at serenade release
            GodRaySystem.CreateBurst(firePos, PrismViolet, 6, 60f, 24,
                GodRaySystem.GodRayStyle.Explosion, RefractedBlue);

            // Screen effects
            if (AdaptiveQualityManager.Instance?.CurrentQuality >= AdaptiveQualityManager.QualityLevel.Medium)
            {
                ScreenDistortionManager.TriggerRipple(firePos, PrismViolet, 0.4f, 18);
                MagnumScreenEffects.AddScreenShake(3.5f);
            }

            Lighting.AddLight(firePos, MoonlightVFXLibrary.MoonWhite.ToVector3() * 1.5f);
        }

        /// <summary>
        /// Draws a shader-driven prismatic bloom overlay using PrismaticBeam.fx at the beam position.
        /// Renders both main and glow passes for layered spectral bloom.
        /// Called from beam PreDraw when shader is available as an enhancement over the particle bloom.
        /// </summary>
        public static void DrawPrismaticShaderBloom(SpriteBatch sb, Vector2 beamWorldPos, int bounceCount)
        {
            if (!MoonlightSonataShaderManager.HasPrismaticBeam) return;

            var glowTex = MoonlightSonataTextures.BloomOrb?.Value
                       ?? MagnumTextureRegistry.GetSoftGlow();
            if (glowTex == null) return;

            Vector2 drawPos = beamWorldPos - Main.screenPosition;
            Vector2 origin = glowTex.Size() * 0.5f;
            float spectralPhase = (float)bounceCount / 5f;
            float pulse = 1f + MathF.Sin(Main.GlobalTimeWrappedHourly * 5f) * 0.1f;
            float bloomScale = (0.2f + bounceCount * 0.05f) * pulse;

            try
            {
                MoonlightSonataShaderManager.BeginShaderBatch(sb);

                // Main prismatic overlay
                MoonlightSonataShaderManager.ApplyMoonlightsCallingPrismaticBeam(
                    Main.GlobalTimeWrappedHourly, spectralPhase, glowPass: false);

                sb.Draw(glowTex, drawPos, null,
                    Color.White * 0.8f, 0f, origin,
                    bloomScale, SpriteEffects.None, 0f);

                // Glow pass
                MoonlightSonataShaderManager.ApplyMoonlightsCallingPrismaticBeam(
                    Main.GlobalTimeWrappedHourly, spectralPhase, glowPass: true);

                sb.Draw(glowTex, drawPos, null,
                    Color.White * 0.5f, 0f, origin,
                    bloomScale * 1.5f, SpriteEffects.None, 0f);

                MoonlightSonataShaderManager.RestoreDefaultBatch(sb);
            }
            catch
            {
                try { MoonlightSonataShaderManager.RestoreDefaultBatch(sb); } catch { }
            }
        }
    }
}
