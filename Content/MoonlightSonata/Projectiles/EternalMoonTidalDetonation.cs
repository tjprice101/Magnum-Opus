using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.VFX.Core;
using MagnumOpus.Common.Systems.VFX.Bloom;
using MagnumOpus.Common.Systems.VFX.Optimization;
using MagnumOpus.Content.MoonlightSonata;
using MagnumOpus.Content.MoonlightSonata.Debuffs;
using MagnumOpus.Content.MoonlightSonata.Dusts;
using MagnumOpus.Content.MoonlightSonata.Weapons.EternalMoon;
using MagnumOpus.Content.SandboxLastPrism.Systems;

namespace MagnumOpus.Content.MoonlightSonata.Projectiles
{
    /// <summary>
    /// Expanding tidal detonation AoE — Phase 4 ("Full Moon" — Crescendo Tide) finisher.
    /// Stationary expanding circular blast at the blade tip with multiple hit frames.
    /// The full moon rises: a massive tidal ring radiates outward with layered bloom,
    /// crescent dust, and resonant pulse rings that damage everything in the expanding zone.
    ///
    /// Inspired by:
    /// - Calamity Ark of Cosmos: Multi-layered expanding energy detonations
    /// - Coralite NoctiflairStrike: Expanding phase-based AoE mechanics
    /// - IncisorLunarDetonation: Circular collision, expanding radius, sigil overlay
    /// </summary>
    public class EternalMoonTidalDetonation : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_0";

        private int _timer;
        private float _expansionRadius = 10f;

        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.maxPenetrate = -1;
            Projectile.timeLeft = 60;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.alpha = 0;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 18;
        }

        public override bool ShouldUpdatePosition() => false;

        public override void AI()
        {
            _timer++;

            // Expand radius with acceleration
            _expansionRadius += 4f + _timer * 0.3f;
            if (_expansionRadius > 180f)
                _expansionRadius = 180f;

            // === FIRST FRAME BURST ===
            if (_timer == 1)
            {
                // 12 TidalMoonDust starburst radially outward
                for (int i = 0; i < 12; i++)
                {
                    float angle = MathHelper.TwoPi * i / 12f;
                    Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 7f);
                    Color tidalColor = EternalMoonVFX.GetLunarPhaseColor((float)i / 12f, 2);
                    Dust tidal = Dust.NewDustPerfect(Projectile.Center,
                        ModContent.DustType<TidalMoonDust>(),
                        vel, 0, tidalColor, 0.35f);
                    tidal.customData = new TidalMoonBehavior(3f, 25);
                }

                // 4 expanding ResonantPulseDust rings at staggered expansion rates
                for (int i = 0; i < 4; i++)
                {
                    Color ringColor = Color.Lerp(EternalMoonVFX.DeepTide,
                        EternalMoonVFX.TidalFoam, (float)i / 4f);
                    Dust pulse = Dust.NewDustPerfect(Projectile.Center,
                        ModContent.DustType<ResonantPulseDust>(),
                        Vector2.Zero, 0, ringColor,
                        0.25f + i * 0.08f);
                    pulse.customData = new ResonantPulseBehavior(0.04f + i * 0.015f, 16 + i * 4);
                }

                // GenericFlare cascade
                CustomParticles.GenericFlare(Projectile.Center, EternalMoonVFX.DeepTide, 0.8f, 22);
                CustomParticles.GenericFlare(Projectile.Center, MoonlightVFXLibrary.DarkPurple, 0.6f, 20);
                CustomParticles.GenericFlare(Projectile.Center, MoonlightVFXLibrary.Violet, 0.5f, 18);
                CustomParticles.GenericFlare(Projectile.Center, MoonlightVFXLibrary.IceBlue, 0.4f, 16);
                CustomParticles.GenericFlare(Projectile.Center, EternalMoonVFX.CrescentGlow, 0.3f, 14);
                CustomParticles.GenericFlare(Projectile.Center, Color.White, 0.2f, 12);

                // HaloRing
                CustomParticles.HaloRing(Projectile.Center, MoonlightVFXLibrary.IceBlue, 0.5f, 18);

                // God ray burst
                GodRaySystem.CreateBurst(Projectile.Center, MoonlightVFXLibrary.IceBlue,
                    rayCount: 6, radius: 50f, duration: 25,
                    GodRaySystem.GodRayStyle.Explosion,
                    secondaryColor: EternalMoonVFX.CrescentGlow);

                // Screen shake
                MagnumScreenEffects.AddScreenShake(5f);

                // Chromatic aberration flash
                try
                {
                    SLPFlashSystem.SetCAFlashEffect(0.2f, 16, 0.5f, 0.4f, true);
                }
                catch { }
            }

            // === PER-FRAME VFX ===

            // Every frame: 2 TidalMoonDust at random positions on the expanding ring perimeter
            for (int i = 0; i < 2; i++)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 pos = Projectile.Center + angle.ToRotationVector2() * _expansionRadius;
                Vector2 outward = angle.ToRotationVector2() * Main.rand.NextFloat(1f, 3f);
                Vector2 vel = outward + Main.rand.NextVector2Circular(1f, 1f);

                Color dustColor = EternalMoonVFX.GetLunarPhaseColor(Main.rand.NextFloat(), 2);
                Dust tidal = Dust.NewDustPerfect(pos,
                    ModContent.DustType<TidalMoonDust>(),
                    vel, 0, dustColor, 0.35f);
                tidal.customData = new TidalMoonBehavior(3f, 25);
            }

            // Every 4 frames: ResonantPulseDust at center
            if (_timer % 4 == 0)
            {
                float pulseScale = _expansionRadius / 180f;
                Color pulseColor = Color.Lerp(EternalMoonVFX.DeepTide,
                    MoonlightVFXLibrary.Violet, pulseScale);
                Dust pulse = Dust.NewDustPerfect(Projectile.Center,
                    ModContent.DustType<ResonantPulseDust>(),
                    Vector2.Zero, 0, pulseColor,
                    0.2f + pulseScale * 0.3f);
                pulse.customData = new ResonantPulseBehavior(0.04f, 16);
            }

            // Every 6 frames: LunarMote at ring perimeter
            if (_timer % 6 == 0)
            {
                float moteAngle = Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 motePos = Projectile.Center + moteAngle.ToRotationVector2() * _expansionRadius;
                Color moteColor = Color.Lerp(MoonlightVFXLibrary.Violet,
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
            float lightIntensity = MathHelper.Clamp(_expansionRadius / 120f, 0.3f, 1.2f);
            Lighting.AddLight(Projectile.Center, MoonlightVFXLibrary.Violet.ToVector3() * lightIntensity);
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
            float lifeFade = 1f - (float)_timer / 60f;

            // === CENTRAL MULTI-LAYERED BLOOM ===
            var glowTex = MagnumTextureRegistry.GetSoftGlow();
            if (glowTex != null)
            {
                Vector2 glowOrigin = glowTex.Size() * 0.5f;

                // Layer 1: DeepTide outer halo
                sb.Draw(glowTex, drawPos, null,
                    (EternalMoonVFX.DeepTide with { A = 0 }) * 0.15f * lifeFade,
                    0f, glowOrigin, _expansionRadius / 40f,
                    SpriteEffects.None, 0f);

                // Layer 2: DarkPurple mid bloom
                sb.Draw(glowTex, drawPos, null,
                    (MoonlightVFXLibrary.DarkPurple with { A = 0 }) * 0.25f * lifeFade,
                    0f, glowOrigin, _expansionRadius / 55f,
                    SpriteEffects.None, 0f);

                // Layer 3: Violet inner bloom
                sb.Draw(glowTex, drawPos, null,
                    (MoonlightVFXLibrary.Violet with { A = 0 }) * 0.35f * lifeFade,
                    0f, glowOrigin, _expansionRadius / 70f,
                    SpriteEffects.None, 0f);

                // Layer 4: IceBlue core
                sb.Draw(glowTex, drawPos, null,
                    (MoonlightVFXLibrary.IceBlue with { A = 0 }) * 0.45f * lifeFade,
                    0f, glowOrigin, _expansionRadius / 100f,
                    SpriteEffects.None, 0f);

                // === RING OF 8 BLOOM ORBS AT EXPANDING EDGE ===
                for (int i = 0; i < 8; i++)
                {
                    float orbAngle = MathHelper.TwoPi * i / 8f + time * 2f;
                    Vector2 orbPos = drawPos + orbAngle.ToRotationVector2() * _expansionRadius;
                    float orbScale = 0.1f + _expansionRadius / 600f;

                    sb.Draw(glowTex, orbPos, null,
                        (MoonlightVFXLibrary.Violet with { A = 0 }) * 0.3f * lifeFade,
                        0f, glowOrigin, orbScale,
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
                    (EternalMoonVFX.CrescentGlow with { A = 0 }) * 0.4f * lifeFade,
                    starRot, starOrigin, 0.15f,
                    SpriteEffects.None, 0f);
            }

            return false;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Apply MusicsDissonance debuff
            target.AddBuff(ModContent.BuffType<MusicsDissonance>(), 300);

            // 6 TidalMoonDust fan from target center
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 4f);
                Color tidalColor = EternalMoonVFX.GetLunarPhaseColor((float)i / 6f, 2);
                Dust tidal = Dust.NewDustPerfect(target.Center,
                    ModContent.DustType<TidalMoonDust>(),
                    vel, 0, tidalColor, 0.35f);
                tidal.customData = new TidalMoonBehavior(3f, 20);
            }

            // GenericFlare at target
            CustomParticles.GenericFlare(target.Center, MoonlightVFXLibrary.Violet, 0.35f, 14);

            // HaloRing at target
            CustomParticles.HaloRing(target.Center, EternalMoonVFX.CrescentGlow, 0.3f, 12);
        }

        public override void OnKill(int timeLeft)
        {
            // Final burst: 8 TidalMoonDust
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 5f);
                Color tidalColor = Color.Lerp(MoonlightVFXLibrary.Violet,
                    MoonlightVFXLibrary.IceBlue, (float)i / 8f);
                Dust tidal = Dust.NewDustPerfect(Projectile.Center,
                    ModContent.DustType<TidalMoonDust>(),
                    vel, 0, tidalColor, 0.35f);
                tidal.customData = new TidalMoonBehavior(3f, 22);
            }

            // 3 ResonantPulseDust rings
            for (int i = 0; i < 3; i++)
            {
                Color ringColor = Color.Lerp(EternalMoonVFX.DeepTide,
                    EternalMoonVFX.TidalFoam, (float)i / 3f);
                Dust pulse = Dust.NewDustPerfect(Projectile.Center,
                    ModContent.DustType<ResonantPulseDust>(),
                    Vector2.Zero, 0, ringColor,
                    0.25f + i * 0.08f);
                pulse.customData = new ResonantPulseBehavior(0.04f + i * 0.012f, 16 + i * 3);
            }

            // GenericFlare cascade
            CustomParticles.GenericFlare(Projectile.Center, MoonlightVFXLibrary.DarkPurple, 0.5f, 18);
            CustomParticles.GenericFlare(Projectile.Center, MoonlightVFXLibrary.Violet, 0.4f, 16);
            CustomParticles.GenericFlare(Projectile.Center, MoonlightVFXLibrary.IceBlue, 0.3f, 14);

            // HaloRing
            CustomParticles.HaloRing(Projectile.Center, MoonlightVFXLibrary.Violet, 0.4f, 16);
        }
    }
}
