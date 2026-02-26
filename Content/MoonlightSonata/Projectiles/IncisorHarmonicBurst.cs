using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.VFX.Core;
using MagnumOpus.Common.Systems.VFX.Bloom;
using MagnumOpus.Common.Systems.VFX.Optimization;
using MagnumOpus.Content.MoonlightSonata;
using MagnumOpus.Content.MoonlightSonata.Debuffs;
using MagnumOpus.Content.MoonlightSonata.Dusts;
using MagnumOpus.Content.MoonlightSonata.Weapons.IncisorOfMoonlight;
using MagnumOpus.Content.SandboxLastPrism.Systems;

namespace MagnumOpus.Content.MoonlightSonata.Projectiles
{
    /// <summary>
    /// Harmonic Burst — resonant detonation AoE for the Incisor of Moonlight ("The Stellar Scalpel").
    /// A stationary expanding circular shockwave that pulses with "tuning fork" resonance.
    /// Concentric expanding rings with standing wave node patterns — sound made visible.
    ///
    /// The Incisor's equivalent of EternalMoonTidalDetonation but with a
    /// "resonant frequency" visual identity instead of "tidal water".
    ///
    /// Inspired by:
    /// - EternalMoonTidalDetonation: Expanding radius, circular collision, layered bloom
    /// - IncisorLunarDetonation: Incisor-themed resonant pulse rings, star sigils
    /// - Calamity Ark of Cosmos: Multi-layered expanding energy detonations
    /// - Tuning fork resonance: Standing wave antinode patterns along the ring perimeter
    /// </summary>
    public class IncisorHarmonicBurst : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_0";

        private int _timer;
        private float _expansionRadius = 10f;

        // Standing wave frequency — 4 full wavelengths around the ring perimeter
        private const float StandingWaveFrequency = 4f;

        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.maxPenetrate = -1;
            Projectile.timeLeft = 70;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.alpha = 0;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 20;
        }

        public override bool ShouldUpdatePosition() => false;

        public override void AI()
        {
            _timer++;

            // Expand radius with acceleration, capped at 200f
            _expansionRadius += 3.5f + _timer * 0.25f;
            if (_expansionRadius > 200f)
                _expansionRadius = 200f;

            // === FIRST FRAME BURST ===
            if (_timer == 1)
            {
                // 10 StarPointDust starburst radially outward
                for (int i = 0; i < 10; i++)
                {
                    float angle = MathHelper.TwoPi * i / 10f;
                    Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 7f);
                    Color starColor = IncisorOfMoonlightVFX.GetResonanceColor((float)i / 10f, 2);
                    Dust star = Dust.NewDustPerfect(Projectile.Center,
                        ModContent.DustType<StarPointDust>(),
                        vel, 0, starColor, 0.4f);
                    star.customData = new StarPointBehavior(0.15f, 25);
                }

                // 5 ResonantPulseDust expanding rings at staggered rates
                for (int i = 0; i < 5; i++)
                {
                    Color ringColor = Color.Lerp(IncisorOfMoonlightVFX.DeepResonance,
                        IncisorOfMoonlightVFX.HarmonicWhite, (float)i / 5f);
                    Dust pulse = Dust.NewDustPerfect(Projectile.Center,
                        ModContent.DustType<ResonantPulseDust>(),
                        Vector2.Zero, 0, ringColor,
                        0.25f + i * 0.08f);
                    pulse.customData = new ResonantPulseBehavior(0.04f + i * 0.015f, 16 + i * 4);
                }

                // GenericFlare cascade: DeepResonance → FrequencyPulse → ConstellationBlue → HarmonicWhite → White
                CustomParticles.GenericFlare(Projectile.Center, IncisorOfMoonlightVFX.DeepResonance, 0.9f, 22);
                CustomParticles.GenericFlare(Projectile.Center, IncisorOfMoonlightVFX.FrequencyPulse, 0.7f, 20);
                CustomParticles.GenericFlare(Projectile.Center, IncisorOfMoonlightVFX.ConstellationBlue, 0.5f, 18);
                CustomParticles.GenericFlare(Projectile.Center, IncisorOfMoonlightVFX.HarmonicWhite, 0.3f, 16);
                CustomParticles.GenericFlare(Projectile.Center, Color.White, 0.2f, 14);

                // HaloRing in ConstellationBlue
                CustomParticles.HaloRing(Projectile.Center, IncisorOfMoonlightVFX.ConstellationBlue, 0.5f, 18);

                // God ray burst
                GodRaySystem.CreateBurst(Projectile.Center, MoonlightVFXLibrary.IceBlue,
                    rayCount: 6, radius: 45f, duration: 22,
                    GodRaySystem.GodRayStyle.Explosion,
                    secondaryColor: IncisorOfMoonlightVFX.FrequencyPulse);

                // Screen shake
                MagnumScreenEffects.AddScreenShake(4f);

                // Chromatic aberration flash
                try
                {
                    SLPFlashSystem.SetCAFlashEffect(0.18f, 14, 0.45f, 0.35f, true);
                }
                catch { }
            }

            // === PER-FRAME VFX ===

            // Every frame: 3 StarPointDust at random positions on the expanding ring perimeter
            for (int i = 0; i < 3; i++)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 pos = Projectile.Center + angle.ToRotationVector2() * _expansionRadius;
                Vector2 outward = angle.ToRotationVector2() * Main.rand.NextFloat(1f, 3f);
                Vector2 vel = outward + Main.rand.NextVector2Circular(0.8f, 0.8f);

                Color dustColor = IncisorOfMoonlightVFX.GetResonanceColor(Main.rand.NextFloat(), 2);
                Dust star = Dust.NewDustPerfect(pos,
                    ModContent.DustType<StarPointDust>(),
                    vel, 0, dustColor, 0.35f);
                star.customData = new StarPointBehavior(0.15f, 22);
            }

            // Every 3 frames: ResonantPulseDust at center with scale based on expansion progress
            if (_timer % 3 == 0)
            {
                float pulseScale = _expansionRadius / 200f;
                Color pulseColor = Color.Lerp(IncisorOfMoonlightVFX.DeepResonance,
                    IncisorOfMoonlightVFX.FrequencyPulse, pulseScale);
                Dust pulse = Dust.NewDustPerfect(Projectile.Center,
                    ModContent.DustType<ResonantPulseDust>(),
                    Vector2.Zero, 0, pulseColor,
                    0.2f + pulseScale * 0.3f);
                pulse.customData = new ResonantPulseBehavior(0.04f, 16);
            }

            // Every 5 frames: LunarMote at ring perimeter orbiting outward
            if (_timer % 5 == 0)
            {
                float moteAngle = Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 motePos = Projectile.Center + moteAngle.ToRotationVector2() * _expansionRadius;
                Color moteColor = Color.Lerp(IncisorOfMoonlightVFX.ConstellationBlue,
                    MoonlightVFXLibrary.IceBlue, Main.rand.NextFloat());
                Dust mote = Dust.NewDustPerfect(motePos,
                    ModContent.DustType<LunarMote>(),
                    moteAngle.ToRotationVector2() * 0.5f,
                    0, moteColor, 0.4f);
                mote.customData = new LunarMoteBehavior(Projectile.Center,
                    Main.rand.NextFloat(MathHelper.TwoPi))
                {
                    OrbitRadius = _expansionRadius * 0.6f,
                    OrbitSpeed = 0.07f,
                    Lifetime = 20,
                    FadePower = 0.9f
                };
            }

            // Dynamic lighting
            float intensity = MathHelper.Clamp(_expansionRadius / 130f, 0.3f, 1.2f);
            Lighting.AddLight(Projectile.Center, MoonlightVFXLibrary.IceBlue.ToVector3() * intensity);
        }

        public override void ModifyDamageHitbox(ref Rectangle hitbox)
        {
            int size = (int)(_expansionRadius * 2f);
            size = Math.Max(size, 10);
            hitbox = new Rectangle(
                (int)(Projectile.Center.X - size / 2f),
                (int)(Projectile.Center.Y - size / 2f),
                size, size);
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            // Circular collision — check if NPC center is within expansion radius
            Vector2 targetCenter = targetHitbox.Center.ToVector2();
            return Vector2.Distance(Projectile.Center, targetCenter) <= _expansionRadius;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float time = Main.GlobalTimeWrappedHourly;
            float lifeFade = 1f - (float)_timer / 70f;

            // === CENTRAL MULTI-LAYERED BLOOM ===
            var glowTex = MagnumTextureRegistry.GetSoftGlow();
            if (glowTex != null)
            {
                Vector2 glowOrigin = glowTex.Size() * 0.5f;

                // Layer 1: DeepResonance outer halo
                sb.Draw(glowTex, drawPos, null,
                    (IncisorOfMoonlightVFX.DeepResonance with { A = 0 }) * 0.15f * lifeFade,
                    0f, glowOrigin, _expansionRadius / 45f,
                    SpriteEffects.None, 0f);

                // Layer 2: FrequencyPulse mid bloom
                sb.Draw(glowTex, drawPos, null,
                    (IncisorOfMoonlightVFX.FrequencyPulse with { A = 0 }) * 0.25f * lifeFade,
                    0f, glowOrigin, _expansionRadius / 60f,
                    SpriteEffects.None, 0f);

                // Layer 3: ConstellationBlue inner bloom
                sb.Draw(glowTex, drawPos, null,
                    (IncisorOfMoonlightVFX.ConstellationBlue with { A = 0 }) * 0.35f * lifeFade,
                    0f, glowOrigin, _expansionRadius / 80f,
                    SpriteEffects.None, 0f);

                // Layer 4: HarmonicWhite core
                sb.Draw(glowTex, drawPos, null,
                    (IncisorOfMoonlightVFX.HarmonicWhite with { A = 0 }) * 0.45f * lifeFade,
                    0f, glowOrigin, _expansionRadius / 110f,
                    SpriteEffects.None, 0f);

                // === RING OF 6 BLOOM ORBS AT EXPANDING EDGE (rotating with time) ===
                for (int i = 0; i < 6; i++)
                {
                    float orbAngle = MathHelper.TwoPi * i / 6f + time * 2f;
                    Vector2 orbPos = drawPos + orbAngle.ToRotationVector2() * _expansionRadius;
                    float orbScale = 0.1f + _expansionRadius / 650f;

                    sb.Draw(glowTex, orbPos, null,
                        (IncisorOfMoonlightVFX.FrequencyPulse with { A = 0 }) * 0.3f * lifeFade,
                        0f, glowOrigin, orbScale,
                        SpriteEffects.None, 0f);
                }

                // === STANDING WAVE NODES: 8 brighter bloom spots at antinode positions ===
                for (int i = 0; i < 8; i++)
                {
                    float nodeAngle = MathHelper.TwoPi * i / 8f;
                    // Standing wave modulation — brighter where sin(angle * frequency) peaks
                    float standingWave = MathF.Abs(MathF.Sin(nodeAngle * StandingWaveFrequency + time * 3f));
                    float nodeIntensity = standingWave * standingWave; // Square for sharper peaks

                    if (nodeIntensity < 0.1f)
                        continue;

                    Vector2 nodePos = drawPos + nodeAngle.ToRotationVector2() * _expansionRadius;
                    float nodeScale = 0.08f + nodeIntensity * 0.12f;

                    // Outer glow at antinode
                    sb.Draw(glowTex, nodePos, null,
                        (IncisorOfMoonlightVFX.ConstellationBlue with { A = 0 }) * 0.4f * nodeIntensity * lifeFade,
                        0f, glowOrigin, nodeScale * 2f,
                        SpriteEffects.None, 0f);

                    // Inner bright core at antinode
                    sb.Draw(glowTex, nodePos, null,
                        (IncisorOfMoonlightVFX.HarmonicWhite with { A = 0 }) * 0.5f * nodeIntensity * lifeFade,
                        0f, glowOrigin, nodeScale * 0.8f,
                        SpriteEffects.None, 0f);
                }
            }

            // === 4-POINTED STAR AT CENTER ===
            var starTex = MoonlightSonataTextures.Star4Point?.Value;
            if (starTex != null)
            {
                Vector2 starOrigin = starTex.Size() * 0.5f;
                float starRot = time * 3f;

                sb.Draw(starTex, drawPos, null,
                    (IncisorOfMoonlightVFX.HarmonicWhite with { A = 0 }) * 0.5f * lifeFade,
                    starRot, starOrigin, 0.15f,
                    SpriteEffects.None, 0f);
            }

            return false;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Apply MusicsDissonance debuff for 240 ticks
            target.AddBuff(ModContent.BuffType<MusicsDissonance>(), 240);

            // 4 StarPointDust from target center
            for (int i = 0; i < 4; i++)
            {
                float angle = MathHelper.TwoPi * i / 4f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 4f);
                Color starColor = IncisorOfMoonlightVFX.GetResonanceColor((float)i / 4f, 2);
                Dust star = Dust.NewDustPerfect(target.Center,
                    ModContent.DustType<StarPointDust>(),
                    vel, 0, starColor, 0.35f);
                star.customData = new StarPointBehavior(0.15f, 20);
            }

            // GenericFlare at target (FrequencyPulse)
            CustomParticles.GenericFlare(target.Center, IncisorOfMoonlightVFX.FrequencyPulse, 0.3f, 12);

            // HaloRing at target (ConstellationBlue)
            CustomParticles.HaloRing(target.Center, IncisorOfMoonlightVFX.ConstellationBlue, 0.25f, 10);
        }

        public override void OnKill(int timeLeft)
        {
            // Final burst: 10 StarPointDust
            for (int i = 0; i < 10; i++)
            {
                float angle = MathHelper.TwoPi * i / 10f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 5f);
                Color starColor = Color.Lerp(IncisorOfMoonlightVFX.ConstellationBlue,
                    IncisorOfMoonlightVFX.HarmonicWhite, (float)i / 10f);
                Dust star = Dust.NewDustPerfect(Projectile.Center,
                    ModContent.DustType<StarPointDust>(),
                    vel, 0, starColor, 0.4f);
                star.customData = new StarPointBehavior(0.15f, 25);
            }

            // 4 ResonantPulseDust rings
            for (int i = 0; i < 4; i++)
            {
                Color ringColor = Color.Lerp(IncisorOfMoonlightVFX.DeepResonance,
                    IncisorOfMoonlightVFX.FrequencyPulse, (float)i / 4f);
                Dust pulse = Dust.NewDustPerfect(Projectile.Center,
                    ModContent.DustType<ResonantPulseDust>(),
                    Vector2.Zero, 0, ringColor,
                    0.25f + i * 0.08f);
                pulse.customData = new ResonantPulseBehavior(0.04f + i * 0.012f, 16 + i * 3);
            }

            // GenericFlare cascade: FrequencyPulse → ConstellationBlue → HarmonicWhite
            CustomParticles.GenericFlare(Projectile.Center, IncisorOfMoonlightVFX.FrequencyPulse, 0.5f, 18);
            CustomParticles.GenericFlare(Projectile.Center, IncisorOfMoonlightVFX.ConstellationBlue, 0.4f, 16);
            CustomParticles.GenericFlare(Projectile.Center, IncisorOfMoonlightVFX.HarmonicWhite, 0.3f, 14);

            // HaloRing (FrequencyPulse)
            CustomParticles.HaloRing(Projectile.Center, IncisorOfMoonlightVFX.FrequencyPulse, 0.4f, 16);
        }
    }
}
