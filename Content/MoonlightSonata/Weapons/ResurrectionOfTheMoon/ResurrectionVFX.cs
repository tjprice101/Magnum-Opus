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

namespace MagnumOpus.Content.MoonlightSonata.Weapons.ResurrectionOfTheMoon
{
    /// <summary>
    /// Unique VFX for Resurrection of the Moon — "The Final Movement".
    /// Theme: Heavy astronomical impact. Comet trails, crater detonations, moonrise flash.
    /// Every shot is a falling star; every ricochet burns brighter.
    ///
    /// Unique identity vs other Moonlight weapons:
    ///   EternalMoon       = flowing water (Cosmic trail, crescent smears)
    ///   Incisor           = surgical precision (thin trails, constellation nodes)
    ///   MoonlightsCalling = musical refraction (prismatic scatter, note cascades)
    ///   Resurrection      = HEAVY IMPACT (comet trails, crater rings, god ray bursts, screen shake)
    /// </summary>
    public static class ResurrectionVFX
    {
        // === UNIQUE COLOR ACCENTS — comet/impact palette ===
        public static readonly Color CometCore = new Color(255, 230, 200);
        public static readonly Color CometTrail = new Color(180, 120, 255);
        public static readonly Color ImpactCrater = new Color(100, 80, 200);
        public static readonly Color MoonriseGold = new Color(255, 210, 150);
        public static readonly Color DeepSpaceViolet = new Color(50, 20, 100);

        /// <summary>
        /// Massive muzzle flash — the gun firing should light up the area.
        /// GodRaySystem burst + screen distortion for sniper-weight impact.
        /// </summary>
        public static void MuzzleFlash(Vector2 firePos, Vector2 direction)
        {
            // Flash cascade — 3 layers outward
            CustomParticles.GenericFlare(firePos, Color.White, 0.9f, 14);
            CustomParticles.GenericFlare(firePos, MoonriseGold, 0.7f, 16);
            CustomParticles.GenericFlare(firePos, CometTrail, 0.55f, 18);

            // Directional blast cone
            for (int i = 0; i < 10; i++)
            {
                Vector2 blastVel = direction.RotatedByRandom(0.35f) * Main.rand.NextFloat(5f, 12f);
                Color blastColor = Color.Lerp(CometCore, MoonlightVFXLibrary.IceBlue, Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(firePos, DustID.MagicMirror, blastVel, 0, blastColor, 1.5f);
                d.noGravity = true;
                d.fadeIn = 1.2f;
            }

            // Perpendicular "barrel flash" sparks
            Vector2 perp = new Vector2(-direction.Y, direction.X);
            for (int side = -1; side <= 1; side += 2)
            {
                for (int j = 0; j < 3; j++)
                {
                    Vector2 sparkVel = perp * side * Main.rand.NextFloat(2f, 5f) + direction * Main.rand.NextFloat(1f, 3f);
                    Dust s = Dust.NewDustPerfect(firePos, DustID.Enchanted_Gold, sparkVel, 0, MoonriseGold, 1.1f);
                    s.noGravity = true;
                }
            }

            // Halo rings
            CustomParticles.HaloRing(firePos, MoonlightVFXLibrary.Violet, 0.5f, 18);
            CustomParticles.HaloRing(firePos, MoonriseGold, 0.4f, 22);

            // GodRaySystem — heavy sniper muzzle flash
            GodRaySystem.CreateBurst(firePos, MoonriseGold, 8, 80f, 25,
                GodRaySystem.GodRayStyle.Explosion, CometTrail);

            // Screen ripple for impact weight
            if (AdaptiveQualityManager.Instance?.CurrentQuality >= AdaptiveQualityManager.QualityLevel.Medium)
            {
                ScreenDistortionManager.TriggerRipple(firePos, CometTrail, 0.5f, 20);
            }

            // Music notes
            MoonlightVFXLibrary.SpawnMusicNotes(firePos, 3, 20f, 0.8f, 1.0f, 30);

            Lighting.AddLight(firePos, CometCore.ToVector3() * 1.5f);
        }

        /// <summary>
        /// Comet trail — dense, heavy trail befitting a sniper round.
        /// Called every frame in projectile AI().
        /// </summary>
        public static void CometTrailFrame(Vector2 projCenter, Vector2 velocity, int ricochetCount)
        {
            float bounceIntensity = 1f + ricochetCount * 0.3f;

            // Dense comet dust — heavy smoky trail
            for (int i = 0; i < 2; i++)
            {
                Vector2 dustVel = -velocity * 0.12f + Main.rand.NextVector2Circular(1f, 1f);
                Color trailColor = Color.Lerp(CometTrail, MoonlightVFXLibrary.IceBlue, Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(projCenter + Main.rand.NextVector2Circular(4f, 4f),
                    DustID.PurpleTorch, dustVel, 0, trailColor, 1.8f * bounceIntensity);
                d.noGravity = true;
                d.fadeIn = 1.3f;
            }

            // Comet ember sparks — golden hot particles
            if (Main.rand.NextBool(2))
            {
                Vector2 emberVel = -velocity * 0.2f + Main.rand.NextVector2Circular(2f, 2f);
                Dust e = Dust.NewDustPerfect(projCenter, DustID.Enchanted_Gold,
                    emberVel, 0, MoonriseGold, 1.2f * bounceIntensity);
                e.noGravity = true;
            }

            // Orbiting music notes (every 8 frames)
            if (Main.rand.NextBool(8))
            {
                float orbitAngle = Main.GameUpdateCount * 0.1f;
                for (int i = 0; i < 2; i++)
                {
                    float noteAngle = orbitAngle + MathHelper.Pi * i;
                    Vector2 notePos = projCenter + noteAngle.ToRotationVector2() * 12f;
                    MoonlightVFXLibrary.SpawnMusicNotes(notePos, 1, 3f, 0.7f, 0.85f, 25);
                }
            }

            // Dynamic lighting
            Lighting.AddLight(projCenter, Color.Lerp(CometTrail, CometCore, 0.3f).ToVector3() * (0.7f + ricochetCount * 0.15f));
        }

        /// <summary>
        /// Ricochet VFX — each bounce burns brighter with escalating god rays.
        /// </summary>
        public static void OnRicochetVFX(Vector2 bouncePos, Vector2 outVel, int ricochetCount)
        {
            float intensity = 0.8f + ricochetCount * 0.3f;

            // Central crater flash
            CustomParticles.GenericFlare(bouncePos, Color.White, 0.6f * intensity, 12);
            CustomParticles.GenericFlare(bouncePos, MoonriseGold, 0.5f * intensity, 15);

            // Crater halo rings
            CustomParticles.HaloRing(bouncePos, ImpactCrater, 0.3f * intensity, 18);
            CustomParticles.HaloRing(bouncePos, CometTrail, 0.25f * intensity, 22);

            // Ricochet spark spray — directional toward outgoing velocity
            int sparkCount = 6 + ricochetCount * 2;
            for (int i = 0; i < sparkCount; i++)
            {
                Vector2 sparkVel = outVel.SafeNormalize(Vector2.Zero).RotatedByRandom(0.8f) * Main.rand.NextFloat(3f, 8f) * intensity;
                Color sparkColor = Color.Lerp(CometCore, CometTrail, Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(bouncePos, DustID.MagicMirror, sparkVel, 0, sparkColor, 1.3f);
                d.noGravity = true;
            }

            // Mini flare burst
            for (int i = 0; i < 4; i++)
            {
                float angle = MathHelper.TwoPi * i / 4f;
                Vector2 flareOffset = angle.ToRotationVector2() * 20f;
                float progress = (float)i / 4f;
                Color fractalColor = Color.Lerp(MoonlightVFXLibrary.Violet, MoonlightVFXLibrary.IceBlue, progress);
                CustomParticles.GenericFlare(bouncePos + flareOffset, fractalColor, 0.35f, 14);
            }

            // GodRaySystem on ricochets 5+
            if (ricochetCount >= 5)
            {
                GodRaySystem.CreateBurst(bouncePos, CometTrail, 4 + (ricochetCount - 5), 40f, 18,
                    GodRaySystem.GodRayStyle.Explosion, MoonriseGold);
            }

            // Music notes
            MoonlightVFXLibrary.SpawnMusicNotes(bouncePos, 2 + ricochetCount / 3, 12f, 0.75f, 1.0f, 30);

            Lighting.AddLight(bouncePos, CometCore.ToVector3() * intensity);
        }

        /// <summary>
        /// Radial explosion on NPC hit — crater detonation with moonbeam lances.
        /// </summary>
        public static void OnHitExplosion(Vector2 impactPos, int ricochetCount, bool crit)
        {
            float intensity = 1f + ricochetCount * 0.15f;

            // Central impact
            MoonlightVFXLibrary.ProjectileImpact(impactPos, intensity);

            // Radial moonbeam lances — 6-point star of dust beams
            int lanceCount = 6;
            for (int i = 0; i < lanceCount; i++)
            {
                float angle = MathHelper.TwoPi * i / lanceCount + Main.rand.NextFloat(-0.1f, 0.1f);
                for (int j = 0; j < 4; j++)
                {
                    float dist = 8f + j * 10f;
                    Vector2 lancePos = impactPos + angle.ToRotationVector2() * dist;
                    Color lanceColor = Color.Lerp(CometCore, MoonlightVFXLibrary.IceBlue, (float)j / 4f);
                    Dust d = Dust.NewDustPerfect(lancePos, DustID.Enchanted_Gold,
                        angle.ToRotationVector2() * (3f + j * 2f), 0, lanceColor, 1.4f - j * 0.15f);
                    d.noGravity = true;
                }
            }

            // Crater ring cascade
            for (int ring = 0; ring < 3 + (crit ? 2 : 0); ring++)
            {
                float progress = ring / (3f + (crit ? 2f : 0f));
                Color craterColor = Color.Lerp(ImpactCrater, DeepSpaceViolet, progress);
                CustomParticles.HaloRing(impactPos, craterColor, 0.35f + ring * 0.15f, 18 + ring * 5);
            }

            // White spark spray
            for (int i = 0; i < 12; i++)
            {
                Vector2 sparkVel = Main.rand.NextVector2Circular(6f, 6f);
                Dust spark = Dust.NewDustPerfect(impactPos, DustID.SilverCoin, sparkVel, 0, Color.White, 1.3f);
                spark.noGravity = true;
            }

            // GodRaySystem on crits
            if (crit)
            {
                GodRaySystem.CreateBurst(impactPos, CometTrail, 6, 60f, 22,
                    GodRaySystem.GodRayStyle.Explosion, MoonriseGold);
            }

            // Music notes
            MoonlightVFXLibrary.SpawnMusicNotes(impactPos, 3, 20f, 0.85f, 1.0f, 30);

            Lighting.AddLight(impactPos, CometCore.ToVector3() * intensity);
        }

        /// <summary>
        /// Wall hit VFX — smaller impact when projectile hits terrain.
        /// </summary>
        public static void WallHitVFX(Vector2 hitPos)
        {
            MoonlightVFXLibrary.ProjectileImpact(hitPos, 0.6f);

            for (int i = 0; i < 16; i++)
            {
                float angle = MathHelper.TwoPi * i / 16f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 6f);
                int dustType = Main.rand.NextBool() ? DustID.PurpleTorch : DustID.IceTorch;
                Dust d = Dust.NewDustPerfect(hitPos, dustType, vel, 100, default, 1.4f);
                d.noGravity = true;
            }

            CustomParticles.HaloRing(hitPos, ImpactCrater, 0.3f, 16);
        }

        /// <summary>
        /// Death VFX — final comet detonation when projectile expires.
        /// </summary>
        public static void DeathVFX(Vector2 deathPos, int totalRicochets)
        {
            float intensity = 0.8f + totalRicochets * 0.1f;

            MoonlightVFXLibrary.ProjectileImpact(deathPos, intensity);

            // Spectral ring cascade
            for (int i = 0; i < 3; i++)
            {
                Color ringColor = Color.Lerp(CometTrail, MoonriseGold, i / 3f);
                CustomParticles.HaloRing(deathPos, ringColor, 0.25f + i * 0.1f, 14 + i * 4);
            }

            // Music note cascade
            MoonlightVFXLibrary.SpawnMusicNotes(deathPos, 4, 25f, 0.8f, 1.0f, 35);
        }

        /// <summary>
        /// Projectile body bloom — comet-core 4-layer bloom stack using {A=0}.
        /// No SpriteBatch restart needed.
        /// </summary>
        public static void DrawProjectileBloom(SpriteBatch sb, Vector2 projWorldPos, float velocityMagnitude, int ricochetCount)
        {
            if (sb == null) return;

            Vector2 drawPos = projWorldPos - Main.screenPosition;
            float speed = MathHelper.Clamp(velocityMagnitude / 20f, 0.5f, 1.5f);
            float pulse = 1f + MathF.Sin(Main.GlobalTimeWrappedHourly * 8f) * 0.08f;
            float bounceScale = 1f + ricochetCount * 0.15f;

            var bloomTex = MagnumTextureRegistry.GetSoftGlow();
            if (bloomTex == null) return;
            Vector2 origin = bloomTex.Size() * 0.5f;

            // All 4 layers use {A=0} — renders additively under AlphaBlend without SpriteBatch restart

            // Layer 1: Outer comet tail glow
            sb.Draw(bloomTex, drawPos, null,
                (CometTrail with { A = 0 }) * 0.25f,
                0f, origin, 0.7f * speed * bounceScale * pulse, SpriteEffects.None, 0f);

            // Layer 2: Mid violet
            sb.Draw(bloomTex, drawPos, null,
                (MoonlightVFXLibrary.Violet with { A = 0 }) * 0.40f,
                0f, origin, 0.45f * speed * bounceScale * pulse, SpriteEffects.None, 0f);

            // Layer 3: Inner gold
            sb.Draw(bloomTex, drawPos, null,
                (MoonriseGold with { A = 0 }) * 0.55f,
                0f, origin, 0.25f * speed * bounceScale * pulse, SpriteEffects.None, 0f);

            // Layer 4: White-hot core
            sb.Draw(bloomTex, drawPos, null,
                (Color.White with { A = 0 }) * 0.75f,
                0f, origin, 0.12f * speed * bounceScale * pulse, SpriteEffects.None, 0f);
        }

        /// <summary>
        /// Reload ready flash — burst when sniper is loaded and ready.
        /// </summary>
        public static void ReadyFlash(Vector2 gunPos)
        {
            // Central flash
            CustomParticles.GenericFlare(gunPos, Color.White, 0.7f, 20);
            CustomParticles.GenericFlare(gunPos, MoonlightVFXLibrary.IceBlue, 0.6f, 18);

            // Fractal burst
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f;
                Vector2 flareOffset = angle.ToRotationVector2() * 20f;
                float progress = (float)i / 6f;
                Color fractalColor = Color.Lerp(MoonlightVFXLibrary.DarkPurple, MoonlightVFXLibrary.MoonWhite, progress);
                CustomParticles.GenericFlare(gunPos + flareOffset, fractalColor, 0.35f, 15);
            }

            // Halo ring
            CustomParticles.HaloRing(gunPos, MoonlightVFXLibrary.IceBlue, 0.4f, 18);

            // Music notes
            MoonlightVFXLibrary.SpawnMusicNotes(gunPos, 4, 25f, 0.8f, 1.0f, 30);

            // Dust burst
            for (int i = 0; i < 10; i++)
            {
                Vector2 dustVel = Main.rand.NextVector2Circular(3f, 3f);
                int dustType = Main.rand.NextBool() ? DustID.PurpleTorch : DustID.SilverCoin;
                Dust d = Dust.NewDustPerfect(gunPos, dustType, dustVel, 100, default, 1.2f);
                d.noGravity = true;
            }

            Lighting.AddLight(gunPos, MoonlightVFXLibrary.MoonWhite.ToVector3() * 1.2f);
        }
    }
}
