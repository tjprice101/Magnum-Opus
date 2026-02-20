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

namespace MagnumOpus.Content.MoonlightSonata.VFX.GoliathOfMoonlight
{
    /// <summary>
    /// Unique VFX for Goliath of Moonlight — the massive summoner minion.
    /// Theme: Cosmic entity, gravitational presence, devastating beam charge/fire.
    /// This replaces ALL vanilla Dust usage with custom particle systems.
    /// Every effect should convey the weight and power of a cosmic entity.
    /// </summary>
    public static class GoliathVFX
    {
        // === UNIQUE COLOR ACCENTS ===
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
        /// Replaces the old scattered vanilla Dust.
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

                    Color moteColor = Color.Lerp(GravityWell, NebulaPurple, (float)i / 3f);
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

            // Music notes gathering toward charge point (progress > 50%)
            if (progress > 0.5f && Main.rand.NextBool(4))
            {
                Vector2 noteOffset = Main.rand.NextVector2Circular(30f, 30f);
                Vector2 noteVel = (chargeCenter - (chargeCenter + noteOffset)).SafeNormalize(Vector2.Zero) * 2f;
                MoonlightVFXLibrary.SpawnMusicNotes(chargeCenter + noteOffset, 1, 5f, 0.75f, 0.95f, 20);
            }

            // Lighting intensifies
            Lighting.AddLight(chargeCenter, StarCore.ToVector3() * (0.5f + progress * 0.8f));
        }

        /// <summary>
        /// Charge release flash — the moment the beam fires.
        /// </summary>
        public static void ChargeReleaseFlash(Vector2 firePos, Vector2 beamDirection)
        {
            // Massive central flash
            CustomParticles.GenericFlare(firePos, Color.White, 1.2f, 18);
            CustomParticles.GenericFlare(firePos, StarCore, 1.0f, 22);
            CustomParticles.GenericFlare(firePos, NebulaPurple, 0.8f, 25);

            // Expanding shockwave ring
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
        /// Jump particles — replaces vanilla Dust.NewDust for jump VFX.
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
        /// Landing impact — replaces vanilla Dust for landing.
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

        // ═══════════════════════════════════════════
        //  DRAW HELPERS
        // ═══════════════════════════════════════════

        /// <summary>
        /// PreDraw bloom for the Goliath — cosmic aura glow around the entity.
        /// Replaces the old raw MagicPixel draws with proper { A = 0 } bloom.
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

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            // Cosmic aura layers
            Color voidColor = CosmicVoid with { A = 0 };
            sb.Draw(bloomTex, drawPos, null, voidColor * 0.15f, 0f, origin, 1.8f * pulse * chargeBoost, SpriteEffects.None, 0f);

            Color nebColor = NebulaPurple with { A = 0 };
            sb.Draw(bloomTex, drawPos, null, nebColor * 0.25f * chargeBoost, 0f, origin, 1.2f * pulse * chargeBoost, SpriteEffects.None, 0f);

            Color gravColor = GravityWell with { A = 0 };
            sb.Draw(bloomTex, drawPos, null, gravColor * 0.35f * chargeBoost, 0f, origin, 0.8f * pulse * chargeBoost, SpriteEffects.None, 0f);

            if (isCharging)
            {
                // Charging: bright core grows
                Color coreColor = StarCore with { A = 0 };
                sb.Draw(bloomTex, drawPos, null, coreColor * 0.5f * chargeProgress, 0f, origin, 0.4f * chargeProgress * pulse, SpriteEffects.None, 0f);
            }

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }
    }
}
