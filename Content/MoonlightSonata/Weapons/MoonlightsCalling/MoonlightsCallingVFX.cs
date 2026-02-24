using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.VFX.Core;
using MagnumOpus.Common.Systems.VFX.Bloom;
using MagnumOpus.Common.Systems.VFX.Trails;
using MagnumOpus.Common.Systems.VFX.Optimization;
using MagnumOpus.Common.Systems.VFX.Screen;
using MagnumOpus.Content.MoonlightSonata;

namespace MagnumOpus.Content.MoonlightSonata.Weapons.MoonlightsCalling
{
    /// <summary>
    /// Unique VFX for Moonlight's Calling — "The Serenade".
    /// Theme: Musical notation made visible. Prismatic beam refraction, spectral bounce cascade.
    /// Each bounce widens the spectral range — light splitting into more colors.
    ///
    /// Unique identity vs other Moonlight weapons:
    ///   EternalMoon  = flowing water trails (Cosmic trail, crescent smears)
    ///   Incisor      = surgical precision (thin trails, constellation nodes)
    ///   MoonlightsCalling = MUSICAL REFRACTION (prismatic scatter, staff-line trails, note cascades)
    /// </summary>
    public static class MoonlightsCallingVFX
    {
        // === UNIQUE COLOR ACCENTS — prismatic refraction palette ===
        public static readonly Color PrismViolet = new Color(160, 80, 255);
        public static readonly Color RefractedBlue = new Color(100, 200, 255);
        public static readonly Color SpectralGreen = new Color(120, 255, 180);
        public static readonly Color TomeGold = new Color(255, 220, 140);

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
        /// Trail color function for CalamityStyleTrailRenderer.
        /// Returns prismatic color that shifts based on trail position and bounce count.
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
        /// Trail width function for CalamityStyleTrailRenderer.
        /// Wider with more bounces (crescendo effect).
        /// </summary>
        public static float BeamTrailWidth(float progress, int bounceCount)
        {
            float baseWidth = 12f + bounceCount * 2f;
            float taper = 1f - progress * progress; // Quadratic taper
            return baseWidth * taper;
        }

        /// <summary>
        /// Beam trail per-frame VFX — prismatic mist + orbiting music notes.
        /// Called every frame in beam AI().
        /// </summary>
        public static void BeamTrailFrame(Vector2 beamPos, Vector2 velocity, int bounceCount)
        {
            float bounceIntensity = 1f + bounceCount * 0.25f;

            // Primary mist trail — soft purple glow dust
            if (Main.rand.NextBool(2))
            {
                Vector2 mistVel = -velocity * 0.08f + Main.rand.NextVector2Circular(0.8f, 0.8f);
                Color mistColor = Color.Lerp(MoonlightVFXLibrary.DarkPurple, PrismViolet, Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(beamPos + Main.rand.NextVector2Circular(6f, 6f),
                    DustID.PurpleTorch, mistVel, 0, mistColor, 1.4f * bounceIntensity);
                d.noGravity = true;
                d.fadeIn = 1.0f;
            }

            // Refraction sparkles — spectral colors based on bounce count
            if (Main.rand.NextBool(3))
            {
                Color refractionColor = GetRefractionColor(Main.rand.NextFloat(), bounceCount);
                Vector2 sparkVel = -velocity * 0.05f + Main.rand.NextVector2Circular(1.5f, 1.5f);
                Dust d = Dust.NewDustPerfect(beamPos, DustID.MagicMirror,
                    sparkVel, 0, refractionColor, 1.0f * bounceIntensity);
                d.noGravity = true;
            }

            // Music notes orbiting beam — the defining visual
            if (Main.rand.NextBool(5))
            {
                float orbitAngle = Main.GameUpdateCount * 0.1f;
                Vector2 notePos = beamPos + new Vector2(MathF.Cos(orbitAngle), MathF.Sin(orbitAngle)) * 10f;
                MoonlightVFXLibrary.SpawnMusicNotes(notePos, 1, 3f, 0.7f, 0.85f, 25);
            }

            // Dynamic lighting — prismatic glow intensifies with bounces
            Vector3 lightVec = Color.Lerp(MoonlightVFXLibrary.Violet, RefractedBlue,
                MathF.Sin(Main.GlobalTimeWrappedHourly * 4f) * 0.5f + 0.5f).ToVector3();
            Lighting.AddLight(beamPos, lightVec * (0.6f + bounceCount * 0.15f));
        }

        /// <summary>
        /// Bounce VFX — prismatic explosion that grows with each successive bounce.
        /// Each bounce is visually larger and more colorful (crescendo).
        /// GodRaySystem burst on bounces 3+.
        /// </summary>
        public static void OnBounceVFX(Vector2 bouncePos, Vector2 outgoingVelocity, int bounceCount)
        {
            float intensity = 0.7f + bounceCount * 0.3f;

            // Central flash — layered
            CustomParticles.GenericFlare(bouncePos, Color.White, 0.5f * intensity, 15);
            CustomParticles.GenericFlare(bouncePos, PrismViolet, 0.4f * intensity, 18);

            // Prismatic scatter — spectral rays emanating from bounce point
            int rayCount = 5 + bounceCount * 2;
            for (int i = 0; i < rayCount; i++)
            {
                float angle = MathHelper.TwoPi * i / rayCount;
                Vector2 rayVel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 6f) * intensity;
                Color rayColor = GetRefractionColor((float)i / rayCount, bounceCount);
                Dust d = Dust.NewDustPerfect(bouncePos, DustID.MagicMirror, rayVel, 0, rayColor, 1.2f);
                d.noGravity = true;
            }

            // Halo rings — gradient cascade
            CustomParticles.HaloRing(bouncePos, PrismViolet, 0.3f * intensity, 16);
            CustomParticles.HaloRing(bouncePos, RefractedBlue, 0.25f * intensity, 20);

            // Music note burst — crescendo with bounces
            MoonlightVFXLibrary.SpawnMusicNotes(bouncePos, 2 + bounceCount, 15f * intensity, 0.75f, 1.0f, 30);

            // Refraction splinter particles
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

        /// <summary>
        /// Grand finale VFX — when beam exhausts all bounces.
        /// Full spectral explosion + god rays + screen effects + music note cascade.
        /// </summary>
        public static void OnBeamFinale(Vector2 deathPos, int totalBounces)
        {
            float intensity = 1f + totalBounces * 0.2f;

            // Massive central flash
            CustomParticles.GenericFlare(deathPos, Color.White, 0.9f * intensity, 22);
            CustomParticles.GenericFlare(deathPos, PrismViolet, 0.7f * intensity, 20);
            CustomParticles.GenericFlare(deathPos, RefractedBlue, 0.6f * intensity, 18);

            // Spectral ring cascade — full rainbow
            for (int i = 0; i < 5; i++)
            {
                Color ringColor = GetRefractionColor(i / 5f, totalBounces);
                CustomParticles.HaloRing(deathPos, ringColor, 0.3f + i * 0.12f, 16 + i * 5);
            }

            // Fractal burst — 8-point prismatic star
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 flareOffset = angle.ToRotationVector2() * 35f;
                Color fractalColor = GetRefractionColor((float)i / 8f, totalBounces);
                CustomParticles.GenericFlare(deathPos + flareOffset, fractalColor, 0.55f, 20);
            }

            // Dense prismatic dust burst
            for (int i = 0; i < 12; i++)
            {
                float angle = MathHelper.TwoPi * i / 12f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 8f);
                Color dustColor = GetRefractionColor((float)i / 12f, totalBounces);
                Dust d = Dust.NewDustPerfect(deathPos, DustID.MagicMirror, vel, 0, dustColor, 1.3f);
                d.noGravity = true;
            }

            // Moonlight lightning fractals
            for (int i = 0; i < 4; i++)
            {
                float lightningAngle = MathHelper.TwoPi * i / 4f + Main.rand.NextFloat(-0.3f, 0.3f);
                Vector2 lightningEnd = deathPos + lightningAngle.ToRotationVector2() * 70f;
                MagnumVFX.DrawMoonlightLightning(deathPos, lightningEnd, 5, 18f, 2, 0.35f);
            }

            // Music notes finale cascade
            MoonlightVFXLibrary.SpawnMusicNotes(deathPos, 6, 40f, 0.9f, 1.1f, 40);

            // GodRaySystem grand finale burst
            GodRaySystem.CreateBurst(deathPos, PrismViolet, 8, 80f, 30,
                GodRaySystem.GodRayStyle.Explosion, RefractedBlue);

            // Screen effects on finale
            if (AdaptiveQualityManager.Instance?.CurrentQuality >= AdaptiveQualityManager.QualityLevel.Medium)
            {
                ScreenDistortionManager.TriggerRipple(deathPos, PrismViolet, 0.4f, 20);
            }

            Lighting.AddLight(deathPos, MoonlightVFXLibrary.MoonWhite.ToVector3() * intensity);
        }

        /// <summary>
        /// Beam body bloom — prismatic 4-layer bloom stack using {A=0} (no SpriteBatch restart).
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
                (outerColor with { A = 0 }) * 0.25f,
                0f, origin, 0.6f * bounceScale * pulse, SpriteEffects.None, 0f);

            // Layer 2: Mid violet
            sb.Draw(bloomTex, drawPos, null,
                (PrismViolet with { A = 0 }) * 0.40f,
                0f, origin, 0.4f * bounceScale * pulse, SpriteEffects.None, 0f);

            // Layer 3: Inner blue
            sb.Draw(bloomTex, drawPos, null,
                (RefractedBlue with { A = 0 }) * 0.55f,
                0f, origin, 0.25f * bounceScale * pulse, SpriteEffects.None, 0f);

            // Layer 4: White-hot core
            sb.Draw(bloomTex, drawPos, null,
                (Color.White with { A = 0 }) * 0.70f,
                0f, origin, 0.12f * bounceScale * pulse, SpriteEffects.None, 0f);
        }

        /// <summary>
        /// Muzzle flash when firing from the tome.
        /// Prismatic burst with music note scatter.
        /// </summary>
        public static void MuzzleFlash(Vector2 firePos, Vector2 direction)
        {
            // Layered central flash
            CustomParticles.GenericFlare(firePos, Color.White * 0.6f, 0.5f, 15);
            CustomParticles.GenericFlare(firePos, PrismViolet, 0.42f, 14);
            CustomParticles.GenericFlare(firePos, MoonlightVFXLibrary.DarkPurple * 0.6f, 0.35f, 12);

            // Directional sparkle burst
            for (int i = 0; i < 6; i++)
            {
                Vector2 vel = direction.RotatedByRandom(0.4f) * Main.rand.NextFloat(3f, 7f);
                Dust d = Dust.NewDustPerfect(firePos, DustID.Enchanted_Gold, vel, 0, RefractedBlue, 1.1f);
                d.noGravity = true;
            }

            // Cascading halo rings
            for (int i = 0; i < 3; i++)
            {
                Color ringColor = Color.Lerp(MoonlightVFXLibrary.DarkPurple, MoonlightVFXLibrary.IceBlue, i / 3f);
                CustomParticles.HaloRing(firePos, ringColor * (0.5f - i * 0.1f), 0.22f + i * 0.08f, 10 + i * 2);
            }

            // Music notes from the tome
            MoonlightVFXLibrary.SpawnMusicNotes(firePos, 2, 10f, 0.7f, 0.9f, 25);
        }

        /// <summary>
        /// Impact VFX when beam hits an NPC — prismatic shattering.
        /// </summary>
        public static void OnHitImpact(Vector2 hitPos, int bounceCount, bool crit)
        {
            float intensity = 0.7f + bounceCount * 0.15f;

            // Central flash
            CustomParticles.GenericFlare(hitPos, Color.White, 0.6f * intensity, 16);
            CustomParticles.GenericFlare(hitPos, PrismViolet, 0.5f * intensity, 14);

            // Prismatic impact burst
            MoonlightVFXLibrary.ProjectileImpact(hitPos, 0.7f * intensity);

            // Gradient halo rings
            for (int ring = 0; ring < 3; ring++)
            {
                float progress = ring / 3f;
                Color ringColor = Color.Lerp(PrismViolet, RefractedBlue, progress);
                CustomParticles.HaloRing(hitPos, ringColor, 0.25f + ring * 0.1f, 14 + ring * 4);
            }

            // Spectral dust spray
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f + Main.rand.NextFloat(-0.2f, 0.2f);
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 6f);
                Color dustColor = GetRefractionColor((float)i / 6f, bounceCount);
                Dust d = Dust.NewDustPerfect(hitPos, DustID.MagicMirror, vel, 0, dustColor, 1.1f);
                d.noGravity = true;
            }

            // Music notes on hit
            MoonlightVFXLibrary.SpawnMusicNotes(hitPos, 2, 15f, 0.75f, 0.9f, 28);

            // GodRaySystem on crits
            if (crit)
            {
                GodRaySystem.CreateBurst(hitPos, PrismViolet, 4, 40f, 18,
                    GodRaySystem.GodRayStyle.Explosion, RefractedBlue);
            }

            Lighting.AddLight(hitPos, RefractedBlue.ToVector3() * intensity);
        }
    }
}
