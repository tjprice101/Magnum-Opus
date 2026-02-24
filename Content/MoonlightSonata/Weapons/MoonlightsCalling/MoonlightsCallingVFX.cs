using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.VFX.Core;
using MagnumOpus.Content.MoonlightSonata;

namespace MagnumOpus.Content.MoonlightSonata.VFX.MoonlightsCalling
{
    /// <summary>
    /// Unique VFX for Moonlight's Calling — the magic tome.
    /// Theme: Ethereal beam refraction, prismatic bounce cascade, channeling circles.
    /// Each bounce intensifies the visual effects — light splitting into spectral colors.
    /// </summary>
    public static class MoonlightsCallingVFX
    {
        // === UNIQUE COLOR ACCENTS ===
        private static readonly Color PrismViolet = new Color(160, 80, 255);
        private static readonly Color RefractedBlue = new Color(100, 200, 255);
        private static readonly Color SpectralGreen = new Color(120, 255, 180);
        private static readonly Color TomeGold = new Color(255, 220, 140);

        /// <summary>
        /// Beam trail VFX — ethereal mist trail with prismatic refraction particles.
        /// Called every frame in the beam projectile's AI().
        /// </summary>
        public static void BeamTrailFrame(Vector2 beamPos, Vector2 velocity, int bounceCount)
        {
            float bounceIntensity = 1f + bounceCount * 0.3f; // Intensifies with bounces

            // Primary mist trail — soft purple glow particles
            if (Main.rand.NextBool(2))
            {
                Vector2 mistVel = -velocity * 0.08f + Main.rand.NextVector2Circular(0.8f, 0.8f);
                Color mistColor = Color.Lerp(MoonlightVFXLibrary.DarkPurple, PrismViolet, Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(beamPos + Main.rand.NextVector2Circular(6f, 6f), DustID.PurpleTorch, mistVel, 0, mistColor, 1.4f * bounceIntensity);
                d.noGravity = true;
                d.fadeIn = 1.0f;
            }

            // Refraction sparkles — prismatic spectral colors based on bounce count
            if (Main.rand.NextBool(3))
            {
                Color refractionColor = GetRefractionColor(Main.rand.NextFloat(), bounceCount);
                Vector2 sparkVel = -velocity * 0.05f + Main.rand.NextVector2Circular(1.5f, 1.5f);
                Dust d = Dust.NewDustPerfect(beamPos, DustID.MagicMirror, sparkVel, 0, refractionColor, 1.0f * bounceIntensity);
                d.noGravity = true;
            }

            // Music notes orbiting beam
            if (Main.rand.NextBool(6))
            {
                float orbitAngle = Main.GameUpdateCount * 0.1f;
                Vector2 notePos = beamPos + new Vector2(MathF.Cos(orbitAngle), MathF.Sin(orbitAngle)) * 10f;
                MoonlightVFXLibrary.SpawnMusicNotes(notePos, 1, 3f, 0.7f, 0.85f, 25);
            }

            // Dynamic lighting — prismatic glow intensifies with bounces
            Vector3 lightVec = Color.Lerp(MoonlightVFXLibrary.Violet, RefractedBlue, MathF.Sin(Main.GlobalTimeWrappedHourly * 4f) * 0.5f + 0.5f).ToVector3();
            Lighting.AddLight(beamPos, lightVec * (0.6f + bounceCount * 0.15f));
        }

        /// <summary>
        /// Returns a refraction color that cycles through a spectral range.
        /// More bounces = wider spectral range (more colors visible).
        /// </summary>
        public static Color GetRefractionColor(float progress, int bounceCount)
        {
            // Base: purple-blue only. Each bounce widens the spectrum.
            float hueRange = MathHelper.Clamp(0.15f + bounceCount * 0.08f, 0.15f, 0.5f);
            float baseHue = 0.7f; // Start at purple
            float hue = (baseHue + progress * hueRange) % 1f;
            return Main.hslToRgb(hue, 0.8f, 0.7f);
        }

        /// <summary>
        /// Bounce VFX — triggered each time the beam bounces off a wall.
        /// Bigger, more prismatic with each successive bounce.
        /// </summary>
        public static void OnBounceVFX(Vector2 bouncePos, Vector2 incomingVelocity, Vector2 outgoingVelocity, int bounceCount)
        {
            float intensity = 0.7f + bounceCount * 0.3f;

            // Central flash
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

            // Halo ring — expands bigger with more bounces
            CustomParticles.MoonlightHalo(bouncePos, 0.35f * intensity);
            CustomParticles.HaloRing(bouncePos, RefractedBlue, 0.3f * intensity, 18);

            // Music note burst on bounce
            MoonlightVFXLibrary.SpawnMusicNotes(bouncePos, 2 + bounceCount, 15f * intensity, 0.75f, 1.0f, 30);

            // Refraction "splinter" particles — unique to this weapon
            for (int i = 0; i < 3 + bounceCount; i++)
            {
                Vector2 splinterVel = outgoingVelocity.SafeNormalize(Vector2.Zero).RotatedByRandom(0.5f) * Main.rand.NextFloat(2f, 5f);
                Color splinterColor = GetRefractionColor(Main.rand.NextFloat(), bounceCount) with { A = 0 };
                CustomParticles.TrailSegment(bouncePos, splinterVel, splinterColor, 0.25f * intensity);
            }

            Lighting.AddLight(bouncePos, RefractedBlue.ToVector3() * intensity);
        }

        /// <summary>
        /// Beam death/kill VFX — prismatic explosion.
        /// </summary>
        public static void OnBeamKill(Vector2 deathPos, int totalBounces)
        {
            float intensity = 1f + totalBounces * 0.2f;

            // Prismatic explosion — full spectrum scatter
            MoonlightVFXLibrary.ProjectileImpact(deathPos, intensity);

            // UNIQUE: Spectral ring cascade
            for (int i = 0; i < 4; i++)
            {
                Color ringColor = GetRefractionColor(i / 4f, totalBounces);
                CustomParticles.HaloRing(deathPos, ringColor, 0.25f + i * 0.12f, 15 + i * 4);
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
        }

        /// <summary>
        /// Beam PreDraw bloom — prismatic 4-layer bloom stack with spectral color cycling.
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

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            // Layer 1: Outer prismatic glow
            Color outerColor = GetRefractionColor(Main.GlobalTimeWrappedHourly % 1f, bounceCount) with { A = 0 };
            sb.Draw(bloomTex, drawPos, null, outerColor * 0.25f, 0f, origin, 0.6f * bounceScale * pulse, SpriteEffects.None, 0f);

            // Layer 2: Mid violet
            Color midColor = PrismViolet with { A = 0 };
            sb.Draw(bloomTex, drawPos, null, midColor * 0.40f, 0f, origin, 0.4f * bounceScale * pulse, SpriteEffects.None, 0f);

            // Layer 3: Inner blue
            Color innerColor = RefractedBlue with { A = 0 };
            sb.Draw(bloomTex, drawPos, null, innerColor * 0.55f, 0f, origin, 0.25f * bounceScale * pulse, SpriteEffects.None, 0f);

            // Layer 4: White-hot core
            sb.Draw(bloomTex, drawPos, null, Color.White with { A = 0 } * 0.70f, 0f, origin, 0.12f * bounceScale * pulse, SpriteEffects.None, 0f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }

        /// <summary>
        /// Muzzle flash when firing from the tome.
        /// </summary>
        public static void MuzzleFlash(Vector2 firePos, Vector2 direction)
        {
            CustomParticles.GenericFlare(firePos, PrismViolet, 0.6f, 15);
            CustomParticles.MoonlightFlare(firePos, 0.5f);

            // Directional sparkle burst
            for (int i = 0; i < 6; i++)
            {
                Vector2 vel = direction.RotatedByRandom(0.4f) * Main.rand.NextFloat(3f, 7f);
                Dust d = Dust.NewDustPerfect(firePos, DustID.Enchanted_Gold, vel, 0, RefractedBlue, 1.1f);
                d.noGravity = true;
            }

            // Music notes from the book
            MoonlightVFXLibrary.SpawnMusicNotes(firePos, 2, 10f, 0.7f, 0.9f, 25);
        }
    }
}
