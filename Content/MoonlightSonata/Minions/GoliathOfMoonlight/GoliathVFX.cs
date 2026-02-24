using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.VFX.Core;
using MagnumOpus.Common.Systems.VFX.Optimization;
using MagnumOpus.Content.MoonlightSonata;

namespace MagnumOpus.Content.MoonlightSonata.Minions
{
    /// <summary>
    /// Unique VFX for Goliath of Moonlight — the massive summoner minion.
    /// Theme: Cosmic entity, gravitational presence, devastating beam charge/fire.
    /// Every effect conveys the weight and power of a cosmic entity.
    ///
    /// Unique identity vs other Moonlight content:
    ///   Weapons  = player-scale, musical themes
    ///   Goliath  = MASSIVE COSMIC ENTITY (gravitational pull, nebula glow, star-core charge)
    /// </summary>
    public static class GoliathVFX
    {
        // === UNIQUE COLOR ACCENTS — cosmic entity palette ===
        private static readonly Color CosmicVoid = new Color(20, 8, 40);
        private static readonly Color GravityWell = new Color(100, 60, 180);
        private static readonly Color NebulaPurple = new Color(150, 80, 220);
        private static readonly Color StarCore = new Color(255, 240, 220);
        private static readonly Color EnergyTendril = new Color(180, 140, 255);

        // ═══════════════════════════════════════════
        //  AMBIENT EFFECTS — Always-on cosmic aura
        // ═══════════════════════════════════════════

        /// <summary>
        /// Ambient cosmic aura — orbiting motes, gravitational shimmer, passive glow.
        /// Called every frame from Goliath's AI when not attacking.
        /// </summary>
        public static void AmbientAura(Vector2 center, int frameCounter)
        {
            // Orbiting gravity motes — 3 motes in slow orbit
            if (frameCounter % 4 == 0)
            {
                float orbitAngle = frameCounter * 0.035f;
                for (int i = 0; i < 3; i++)
                {
                    float moteAngle = orbitAngle + MathHelper.TwoPi * i / 3f;
                    float radius = 25f + MathF.Sin(frameCounter * 0.02f + i) * 6f;
                    Vector2 motePos = center + moteAngle.ToRotationVector2() * radius;

                    CustomParticles.MoonlightTrailFlare(motePos, moteAngle.ToRotationVector2() * 0.5f);
                }
            }

            // Soft cosmic dust trail below (gravitational wake)
            if (Main.rand.NextBool(3))
            {
                Vector2 dustPos = center + new Vector2(Main.rand.NextFloat(-20f, 20f), Main.rand.NextFloat(5f, 15f));
                Vector2 dustVel = new Vector2(Main.rand.NextFloat(-0.3f, 0.3f), -0.5f - Main.rand.NextFloat(0.5f));
                Color dustColor = Color.Lerp(CosmicVoid, GravityWell, Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(dustPos, DustID.PurpleTorch, dustVel, 0, dustColor, 1.3f);
                d.noGravity = true;
            }

            // Sparse music note float (every 30 frames)
            if (frameCounter % 30 == 0)
            {
                MoonlightVFXLibrary.SpawnMusicNotes(center + new Vector2(0f, -20f), 1, 15f, 0.7f, 0.85f, 40);
            }

            // Cosmic glow
            float pulse = 0.4f + MathF.Sin(frameCounter * 0.04f) * 0.1f;
            Lighting.AddLight(center, MoonlightVFXLibrary.Violet.ToVector3() * pulse);
        }

        // ═══════════════════════════════════════════
        //  CHARGE EFFECTS — Building devastating beam
        // ═══════════════════════════════════════════

        /// <summary>
        /// Charge buildup VFX — converging cosmic particles, growing bloom, energy arcs.
        /// Called every frame during the beam charge sequence.
        /// progress: 0→1 from start to fire.
        /// </summary>
        public static void ChargeBuildup(Vector2 chargeCenter, float progress)
        {
            // Converging particle ring — shrinks toward charge point
            float convergeRadius = 80f * (1f - progress * 0.6f);
            int particleCount = (int)(4 + progress * 8);

            for (int i = 0; i < particleCount; i++)
            {
                float angle = MathHelper.TwoPi * i / particleCount + Main.GameUpdateCount * 0.05f;
                Vector2 particlePos = chargeCenter + angle.ToRotationVector2() * convergeRadius;
                Vector2 toCenter = (chargeCenter - particlePos).SafeNormalize(Vector2.Zero) * (2f + progress * 4f);

                Color chargeColor = Color.Lerp(GravityWell, StarCore, progress);
                Dust d = Dust.NewDustPerfect(particlePos, DustID.MagicMirror, toCenter, 0, chargeColor, 1.2f + progress * 0.8f);
                d.noGravity = true;
            }

            // Growing bloom at center
            float bloomScale = 0.2f + progress * 0.6f;
            CustomParticles.MoonlightFlare(chargeCenter, bloomScale);

            // Energy crackling arcs (random small lines toward center)
            if (progress > 0.3f && Main.rand.NextBool(3))
            {
                float arcAngle = Main.rand.NextFloat(MathHelper.TwoPi);
                float arcDist = Main.rand.NextFloat(20f, 40f) * (1f - progress * 0.5f);
                Vector2 arcStart = chargeCenter + arcAngle.ToRotationVector2() * arcDist;

                for (int j = 0; j < 3; j++)
                {
                    Vector2 arcPos = Vector2.Lerp(arcStart, chargeCenter, (float)j / 3f);
                    arcPos += Main.rand.NextVector2Circular(3f, 3f);
                    Dust a = Dust.NewDustPerfect(arcPos, DustID.Electric, Vector2.Zero, 0, EnergyTendril, 0.8f);
                    a.noGravity = true;
                }
            }

            // GodRaySystem pulse at high charge (progress > 70%)
            if (progress > 0.7f && Main.GameUpdateCount % 20 == 0)
            {
                GodRaySystem.CreateBurst(chargeCenter, NebulaPurple, 4, 30f * progress, 12,
                    GodRaySystem.GodRayStyle.Pulsing, MoonlightVFXLibrary.IceBlue);
            }

            // Music notes gathering toward charge point (progress > 50%)
            if (progress > 0.5f && Main.rand.NextBool(4))
            {
                Vector2 noteOffset = Main.rand.NextVector2Circular(30f, 30f);
                MoonlightVFXLibrary.SpawnMusicNotes(chargeCenter + noteOffset, 1, 5f, 0.75f, 0.95f, 20);
            }

            // Lighting intensifies
            Lighting.AddLight(chargeCenter, StarCore.ToVector3() * (0.5f + progress * 0.8f));
        }

        /// <summary>
        /// Charge release flash — the moment the beam fires.
        /// GodRaySystem + screen distortion + massive particle cascade.
        /// </summary>
        public static void ChargeReleaseFlash(Vector2 firePos, Vector2 beamDirection)
        {
            // Massive central flash
            CustomParticles.GenericFlare(firePos, Color.White, 1.2f, 18);
            CustomParticles.GenericFlare(firePos, StarCore, 1.0f, 22);
            CustomParticles.GenericFlare(firePos, NebulaPurple, 0.8f, 25);

            // Expanding shockwave rings
            CustomParticles.MoonlightHalo(firePos, 1.0f);
            for (int i = 0; i < 3; i++)
            {
                Color ringColor = Color.Lerp(NebulaPurple, MoonlightVFXLibrary.IceBlue, i / 3f);
                CustomParticles.HaloRing(firePos, ringColor, 0.5f + i * 0.2f, 18 + i * 5);
            }

            // Perpendicular blast sparks
            Vector2 perp = new Vector2(-beamDirection.Y, beamDirection.X);
            for (int side = -1; side <= 1; side += 2)
            {
                for (int j = 0; j < 5; j++)
                {
                    Vector2 sparkVel = perp * side * Main.rand.NextFloat(3f, 8f) + beamDirection * Main.rand.NextFloat(1f, 3f);
                    Color sparkCol = Color.Lerp(StarCore, EnergyTendril, Main.rand.NextFloat());
                    Dust d = Dust.NewDustPerfect(firePos, DustID.MagicMirror, sparkVel, 0, sparkCol, 1.5f);
                    d.noGravity = true;
                }
            }

            // GodRaySystem — massive beam release burst
            GodRaySystem.CreateBurst(firePos, NebulaPurple, 8, 100f, 25,
                GodRaySystem.GodRayStyle.Explosion, StarCore);

            // Screen distortion
            if (AdaptiveQualityManager.Instance?.CurrentQuality >= AdaptiveQualityManager.QualityLevel.Medium)
            {
                ScreenDistortionManager.TriggerRipple(firePos, NebulaPurple, 0.6f, 25);
            }

            // Music note burst
            MoonlightVFXLibrary.SpawnMusicNotes(firePos, 5, 20f, 0.85f, 1.1f, 35);

            // Screen shake for the big beam
            MagnumScreenEffects.AddScreenShake(4f);

            Lighting.AddLight(firePos, StarCore.ToVector3() * 2f);
        }

        // ═══════════════════════════════════════════
        //  BEAM EFFECTS — The devastating beam itself
        // ═══════════════════════════════════════════

        /// <summary>
        /// Beam body VFX — particles along an active beam.
        /// Called per frame while the beam is firing.
        /// </summary>
        public static void BeamBodyParticles(Vector2 beamStart, Vector2 beamEnd)
        {
            float beamLength = Vector2.Distance(beamStart, beamEnd);
            Vector2 beamDir = (beamEnd - beamStart).SafeNormalize(Vector2.Zero);
            Vector2 perp = new Vector2(-beamDir.Y, beamDir.X);

            // Particles scattered along beam length
            int particlesPerFrame = Math.Max(3, (int)(beamLength / 60f));
            for (int i = 0; i < particlesPerFrame; i++)
            {
                float t = Main.rand.NextFloat();
                Vector2 pos = Vector2.Lerp(beamStart, beamEnd, t);
                pos += perp * Main.rand.NextFloat(-8f, 8f);

                Vector2 driftVel = perp * Main.rand.NextFloat(-1.5f, 1.5f);
                Color beamColor = Color.Lerp(NebulaPurple, MoonlightVFXLibrary.IceBlue, t);
                Dust d = Dust.NewDustPerfect(pos, DustID.MagicMirror, driftVel, 0, beamColor, 1.2f);
                d.noGravity = true;
            }

            // Music notes along beam (sparse)
            if (Main.rand.NextBool(8))
            {
                float noteDist = Main.rand.NextFloat(100f, beamLength * 0.8f);
                Vector2 notePos = beamStart + beamDir * noteDist;
                MoonlightVFXLibrary.SpawnMusicNotes(notePos, 1, 8f, 0.75f, 0.9f, 25);
            }

            // Lighting along beam
            int lightPoints = Math.Max(2, (int)(beamLength / 100f));
            for (int i = 0; i < lightPoints; i++)
            {
                float t = (float)(i + 1) / (lightPoints + 1);
                Vector2 lightPos = Vector2.Lerp(beamStart, beamEnd, t);
                Lighting.AddLight(lightPos, NebulaPurple.ToVector3() * 0.8f);
            }
        }

        /// <summary>
        /// Beam impact point VFX — where the beam hits the ground/enemy.
        /// </summary>
        public static void BeamImpactPoint(Vector2 impactPos)
        {
            // Impact sparks
            for (int i = 0; i < 4; i++)
            {
                Vector2 sparkVel = Main.rand.NextVector2Circular(4f, 4f);
                Color sparkCol = Color.Lerp(StarCore, NebulaPurple, Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(impactPos, DustID.MagicMirror, sparkVel, 0, sparkCol, 1.3f);
                d.noGravity = true;
            }

            // Impact glow
            if (Main.rand.NextBool(2))
            {
                CustomParticles.MoonlightFlare(impactPos, 0.3f);
            }

            Lighting.AddLight(impactPos, StarCore.ToVector3() * 0.8f);
        }

        /// <summary>
        /// Beam ricochet explosion — massive detonation at each enemy hit by the beam.
        /// Replaces the old raw Dust explosion rings with themed VFX.
        /// </summary>
        public static void BeamExplosion(Vector2 position)
        {
            // Goliath-scale explosion using custom particles
            CustomParticles.MoonlightCrescendo(position, 1.8f);
            CustomParticles.ExplosionBurst(position, NebulaPurple, 20, 9f);

            // Flash cascade
            CustomParticles.GenericFlare(position, Color.White, 0.8f, 16);
            CustomParticles.GenericFlare(position, StarCore, 0.6f, 20);
            CustomParticles.GenericFlare(position, NebulaPurple, 0.5f, 22);

            // Halo rings
            for (int i = 0; i < 3; i++)
            {
                Color ringColor = Color.Lerp(NebulaPurple, MoonlightVFXLibrary.IceBlue, i / 3f);
                CustomParticles.HaloRing(position, ringColor, 0.4f + i * 0.15f, 16 + i * 4);
            }

            // Radial dust burst
            for (int i = 0; i < 16; i++)
            {
                float angle = MathHelper.TwoPi * i / 16f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(5f, 12f);
                Color dustCol = Color.Lerp(GravityWell, MoonlightVFXLibrary.IceBlue, Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(position, DustID.PurpleTorch, vel, 0, dustCol, 2.2f);
                d.noGravity = true;
                d.fadeIn = 1.5f;
            }

            // Music notes
            MoonlightVFXLibrary.SpawnMusicNotes(position, 3, 20f, 0.8f, 1.0f, 30);

            Lighting.AddLight(position, StarCore.ToVector3() * 1.2f);
        }

        /// <summary>
        /// Ricochet line between two enemies — lightning chain visual.
        /// </summary>
        public static void BeamRicochetLine(Vector2 start, Vector2 end)
        {
            MagnumVFX.DrawMoonlightLightning(start, end, 5, 15f, 2, 0.3f);
        }

        // ═══════════════════════════════════════════
        //  MELEE COMBAT EFFECTS
        // ═══════════════════════════════════════════

        /// <summary>
        /// Goliath melee hit VFX — heavy cosmic impact.
        /// </summary>
        public static void MeleeHitImpact(Vector2 hitPos)
        {
            // Central flash
            CustomParticles.GenericFlare(hitPos, Color.White, 0.6f, 15);
            CustomParticles.GenericFlare(hitPos, NebulaPurple, 0.5f, 18);

            // Gradient halo cascade
            MoonlightVFXLibrary.SpawnGradientHaloRings(hitPos, 4, 0.3f);

            // Cosmic burst dust
            for (int i = 0; i < 10; i++)
            {
                float angle = MathHelper.TwoPi * i / 10f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 7f);
                Color dustCol = Color.Lerp(GravityWell, MoonlightVFXLibrary.IceBlue, Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(hitPos, DustID.MagicMirror, vel, 0, dustCol, 1.4f);
                d.noGravity = true;
            }

            // Music notes
            MoonlightVFXLibrary.SpawnMusicNotes(hitPos, 3, 20f, 0.75f, 0.95f, 30);

            Lighting.AddLight(hitPos, NebulaPurple.ToVector3() * 1.0f);
        }

        // ═══════════════════════════════════════════
        //  MOVEMENT EFFECTS
        // ═══════════════════════════════════════════

        /// <summary>
        /// Jump particles — cosmic dust kick-up on jumps.
        /// </summary>
        public static void JumpEffect(Vector2 feetPos)
        {
            for (int i = 0; i < 5; i++)
            {
                Vector2 dustVel = new Vector2(Main.rand.NextFloat(-3f, 3f), Main.rand.NextFloat(-2f, 0.5f));
                Color jumpCol = Color.Lerp(CosmicVoid, GravityWell, Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(feetPos + new Vector2(Main.rand.NextFloat(-10f, 10f), 0f), DustID.PurpleTorch, dustVel, 0, jumpCol, 1.2f);
                d.noGravity = true;
            }
        }

        /// <summary>
        /// Landing impact — cosmic dust burst on landing.
        /// </summary>
        public static void LandingEffect(Vector2 feetPos)
        {
            for (int i = 0; i < 8; i++)
            {
                Vector2 dustVel = new Vector2(Main.rand.NextFloat(-4f, 4f), Main.rand.NextFloat(-3f, -0.5f));
                Color landCol = Color.Lerp(GravityWell, NebulaPurple, Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(feetPos + new Vector2(Main.rand.NextFloat(-12f, 12f), 0f), DustID.PurpleTorch, dustVel, 0, landCol, 1.4f);
                d.noGravity = true;
            }
            CustomParticles.MoonlightHalo(feetPos, 0.25f);
        }

        /// <summary>
        /// Teleport VFX — flash when Goliath teleports to owner.
        /// </summary>
        public static void TeleportFlash(Vector2 position)
        {
            CustomParticles.GenericFlare(position, Color.White, 0.7f, 16);
            CustomParticles.GenericFlare(position, NebulaPurple, 0.5f, 20);

            for (int i = 0; i < 12; i++)
            {
                float angle = MathHelper.TwoPi * i / 12f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 6f);
                Color tpCol = Color.Lerp(GravityWell, MoonlightVFXLibrary.IceBlue, Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(position, DustID.PurpleTorch, vel, 0, tpCol, 1.6f);
                d.noGravity = true;
            }

            CustomParticles.HaloRing(position, NebulaPurple, 0.4f, 18);
            MoonlightVFXLibrary.SpawnMusicNotes(position, 2, 15f, 0.75f, 0.9f, 30);
        }

        // ═══════════════════════════════════════════
        //  DRAW HELPERS
        // ═══════════════════════════════════════════

        /// <summary>
        /// PreDraw bloom for the Goliath — cosmic aura glow around the entity.
        /// Uses {A=0} premultiplied alpha trick — NO SpriteBatch restart needed.
        /// </summary>
        public static void DrawCosmicBloom(SpriteBatch sb, Vector2 goliathCenter, bool isCharging, float chargeProgress)
        {
            if (sb == null) return;

            Vector2 drawPos = goliathCenter - Main.screenPosition;
            float pulse = 1f + MathF.Sin(Main.GlobalTimeWrappedHourly * 4f) * 0.08f;
            float chargeBoost = isCharging ? (1f + chargeProgress * 0.5f) : 1f;

            var bloomTex = MagnumTextureRegistry.GetSoftGlow();
            if (bloomTex == null) return;
            Vector2 origin = bloomTex.Size() * 0.5f;

            // All layers use {A=0} — renders additively under AlphaBlend, no restart needed

            // Layer 1: Outer cosmic void aura
            sb.Draw(bloomTex, drawPos, null,
                (CosmicVoid with { A = 0 }) * 0.15f,
                0f, origin, 1.8f * pulse * chargeBoost, SpriteEffects.None, 0f);

            // Layer 2: Mid nebula purple
            sb.Draw(bloomTex, drawPos, null,
                (NebulaPurple with { A = 0 }) * 0.25f * chargeBoost,
                0f, origin, 1.2f * pulse * chargeBoost, SpriteEffects.None, 0f);

            // Layer 3: Inner gravity well
            sb.Draw(bloomTex, drawPos, null,
                (GravityWell with { A = 0 }) * 0.35f * chargeBoost,
                0f, origin, 0.8f * pulse * chargeBoost, SpriteEffects.None, 0f);

            // Layer 4: Star core (visible when charging)
            if (isCharging)
            {
                sb.Draw(bloomTex, drawPos, null,
                    (StarCore with { A = 0 }) * 0.5f * chargeProgress,
                    0f, origin, 0.4f * chargeProgress * pulse, SpriteEffects.None, 0f);
            }
        }

        /// <summary>
        /// Draw the beam body with proper bloom rendering.
        /// Used by GoliathDevastatingBeam's PreDraw to render the beam visually.
        /// </summary>
        public static void DrawBeamBody(SpriteBatch sb, Vector2 beamStart, Vector2 beamEnd, float widthProgress)
        {
            if (sb == null) return;

            var bloomTex = MagnumTextureRegistry.GetSoftGlow();
            if (bloomTex == null) return;
            Vector2 origin = bloomTex.Size() * 0.5f;

            float beamLength = Vector2.Distance(beamStart, beamEnd);
            Vector2 beamDir = (beamEnd - beamStart).SafeNormalize(Vector2.Zero);
            float beamRotation = beamDir.ToRotation();
            float pulse = 1f + MathF.Sin(Main.GlobalTimeWrappedHourly * 8f) * 0.06f;

            // Draw bloom nodes along the beam
            int nodeCount = Math.Max(3, (int)(beamLength / 30f));
            for (int i = 0; i <= nodeCount; i++)
            {
                float t = (float)i / nodeCount;
                Vector2 nodeWorldPos = Vector2.Lerp(beamStart, beamEnd, t);
                Vector2 nodeDrawPos = nodeWorldPos - Main.screenPosition;

                float beamWidth = widthProgress * pulse;

                // Outer nebula glow
                sb.Draw(bloomTex, nodeDrawPos, null,
                    (NebulaPurple with { A = 0 }) * 0.20f,
                    0f, origin, 0.6f * beamWidth, SpriteEffects.None, 0f);

                // Mid violet
                sb.Draw(bloomTex, nodeDrawPos, null,
                    (MoonlightVFXLibrary.Violet with { A = 0 }) * 0.35f,
                    0f, origin, 0.4f * beamWidth, SpriteEffects.None, 0f);

                // Inner ice blue
                sb.Draw(bloomTex, nodeDrawPos, null,
                    (MoonlightVFXLibrary.IceBlue with { A = 0 }) * 0.50f,
                    0f, origin, 0.25f * beamWidth, SpriteEffects.None, 0f);

                // White-hot core
                sb.Draw(bloomTex, nodeDrawPos, null,
                    (Color.White with { A = 0 }) * 0.65f,
                    0f, origin, 0.12f * beamWidth, SpriteEffects.None, 0f);
            }
        }

        /// <summary>
        /// Small beam projectile bloom — for GoliathMoonlightBeam body rendering.
        /// </summary>
        public static void DrawSmallBeamBloom(SpriteBatch sb, Vector2 projWorldPos, int ricochetCount)
        {
            if (sb == null) return;

            Vector2 drawPos = projWorldPos - Main.screenPosition;
            float pulse = 1f + MathF.Sin(Main.GlobalTimeWrappedHourly * 6f) * 0.08f;
            float bounceScale = 1f + ricochetCount * 0.1f;

            var bloomTex = MagnumTextureRegistry.GetSoftGlow();
            if (bloomTex == null) return;
            Vector2 origin = bloomTex.Size() * 0.5f;

            // Layer 1: Outer purple
            sb.Draw(bloomTex, drawPos, null,
                (GravityWell with { A = 0 }) * 0.25f,
                0f, origin, 0.5f * bounceScale * pulse, SpriteEffects.None, 0f);

            // Layer 2: Mid violet
            sb.Draw(bloomTex, drawPos, null,
                (MoonlightVFXLibrary.Violet with { A = 0 }) * 0.40f,
                0f, origin, 0.35f * bounceScale * pulse, SpriteEffects.None, 0f);

            // Layer 3: Inner ice blue
            sb.Draw(bloomTex, drawPos, null,
                (MoonlightVFXLibrary.IceBlue with { A = 0 }) * 0.55f,
                0f, origin, 0.2f * bounceScale * pulse, SpriteEffects.None, 0f);

            // Layer 4: White core
            sb.Draw(bloomTex, drawPos, null,
                (Color.White with { A = 0 }) * 0.70f,
                0f, origin, 0.1f * bounceScale * pulse, SpriteEffects.None, 0f);
        }

        /// <summary>
        /// Small beam hit explosion — fractal burst + themed dust.
        /// </summary>
        public static void SmallBeamHitExplosion(Vector2 hitPos, int ricochetCount)
        {
            float intensity = 0.7f + ricochetCount * 0.08f;

            // Central flash
            CustomParticles.GenericFlare(hitPos, Color.White, 0.5f * intensity, 14);
            CustomParticles.GenericFlare(hitPos, NebulaPurple, 0.4f * intensity, 16);

            // Fractal flare burst (signature Moonlight pattern)
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f;
                Vector2 flareOffset = angle.ToRotationVector2() * 30f;
                float progress = (float)i / 6f;
                Color fractalColor = Color.Lerp(MoonlightVFXLibrary.DarkPurple, MoonlightVFXLibrary.IceBlue, progress);
                CustomParticles.GenericFlare(hitPos + flareOffset, fractalColor, 0.35f, 16);
            }

            // Dust burst
            for (int i = 0; i < 10; i++)
            {
                float angle = MathHelper.TwoPi * i / 10f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 8f);
                Color dustCol = Color.Lerp(GravityWell, MoonlightVFXLibrary.IceBlue, Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(hitPos, DustID.PurpleTorch, vel, 0, dustCol, 1.8f);
                d.noGravity = true;
                d.fadeIn = 1.3f;
            }

            // Halo rings
            CustomParticles.HaloRing(hitPos, NebulaPurple, 0.3f, 16);
            CustomParticles.HaloRing(hitPos, MoonlightVFXLibrary.IceBlue, 0.25f, 20);

            // Music notes
            MoonlightVFXLibrary.SpawnMusicNotes(hitPos, 2, 15f, 0.75f, 0.9f, 28);

            Lighting.AddLight(hitPos, NebulaPurple.ToVector3() * intensity);
        }

        /// <summary>
        /// Small beam death VFX — final detonation when beam expires.
        /// </summary>
        public static void SmallBeamDeath(Vector2 deathPos)
        {
            MoonlightVFXLibrary.ProjectileImpact(deathPos, 0.8f);

            for (int i = 0; i < 3; i++)
            {
                Color ringColor = Color.Lerp(GravityWell, MoonlightVFXLibrary.MoonWhite, i / 3f);
                CustomParticles.HaloRing(deathPos, ringColor, 0.25f + i * 0.1f, 14 + i * 4);
            }

            MoonlightVFXLibrary.SpawnMusicNotes(deathPos, 3, 20f, 0.8f, 1.0f, 30);
        }
    }
}
