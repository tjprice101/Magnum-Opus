using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.VFX.Core;
using MagnumOpus.Common.Systems.VFX.Bloom;
using MagnumOpus.Common.Systems.VFX.Trails;
using MagnumOpus.Common.Systems.VFX.Optimization;
using MagnumOpus.Content.MoonlightSonata;
using MagnumOpus.Content.MoonlightSonata.Dusts;
using MagnumOpus.Content.MoonlightSonata.Debuffs;
using MagnumOpus.Content.MoonlightSonata.Weapons.MoonlightsCalling;
using MagnumOpus.Content.SandboxLastPrism.Systems;

namespace MagnumOpus.Content.MoonlightSonata.Projectiles
{
    /// <summary>
    /// Prismatic Crescendo — "The Serenade" Serenade Mode alt-fire mega-beam.
    ///
    /// A massive prismatic beam that travels through walls and enemies with infinite pierce,
    /// leaving dense spectral particle storms and escalating musical VFX in its wake.
    /// The culmination of the Moonlight's Calling weapon — a charged beam that channels
    /// every spectral color simultaneously in a blinding crescendo of light and sound.
    ///
    /// Visual Identity: Full-spectrum prismatic overload
    /// - Dense PrismaticShardDust trail cycling through the entire spectral palette
    /// - LunarMote crescents orbiting the beam path like swirling sheet music
    /// - StarPointDust twinkles scattered along the trajectory
    /// - ResonantPulseDust expanding rings pulsing outward at regular intervals
    /// - Music note storms trailing behind the beam
    /// - Massive spectral detonation on death with god rays and screen distortion
    /// </summary>
    public class PrismaticCrescendo : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/SoftGlow3";

        /// <summary>Internal frame counter for timed VFX intervals.</summary>
        private ref float FrameCounter => ref Projectile.ai[0];

        // ai[1] is unused — reserved for future expansion.

        /// <summary>Maximum bounce count equivalent for VFX intensity scaling (always 5 = max).</summary>
        private const int VFXIntensityTier = 5;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 30;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 32;
            Projectile.height = 32;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 120;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.alpha = 255;
            Projectile.extraUpdates = 2;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 8;
            Projectile.scale = 1.5f;
        }

        public override void AI()
        {
            int frame = (int)FrameCounter;

            // Rotation tracks velocity direction
            Projectile.rotation = Projectile.velocity.ToRotation();

            // Gradual deceleration — maintains most speed but slowly fades over lifetime
            Projectile.velocity *= 0.995f;

            // ===================================================================
            //  EVERY-FRAME VFX — dense spectral particle trail
            // ===================================================================

            // PRISMATIC SHARD TRAIL — dense spectral dust cycling through all colors
            {
                float hueProgress = (Main.GlobalTimeWrappedHourly * 2f + frame * 0.05f) % 1f;
                Vector2 shardVel = -Projectile.velocity * 0.08f + Main.rand.NextVector2Circular(1.5f, 1.5f);
                Color shardColor = MoonlightsCallingVFX.GetRefractionColor(hueProgress, VFXIntensityTier);
                Dust shard = Dust.NewDustPerfect(
                    Projectile.Center + Main.rand.NextVector2Circular(8f, 8f),
                    ModContent.DustType<PrismaticShardDust>(),
                    shardVel, 0, shardColor, 0.35f);
                shard.customData = new PrismaticShardBehavior
                {
                    BaseHue = hueProgress,
                    HueRange = 0.4f,
                    VelocityDecay = 0.93f,
                    RotationSpeed = 0.12f,
                    BaseScale = 0.35f,
                    Lifetime = 28
                };
            }

            // LUNAR MOTE CRESCENTS — orbiting the beam path like swirling sheet music
            if (Main.rand.NextBool(3))
            {
                float orbitAngle = Main.GameUpdateCount * 0.12f + Main.rand.NextFloat(MathHelper.TwoPi);
                Color moteColor = Color.Lerp(MoonlightsCallingVFX.PrismViolet,
                    MoonlightsCallingVFX.SpectralCyan, Main.rand.NextFloat());
                Dust mote = Dust.NewDustPerfect(Projectile.Center,
                    ModContent.DustType<LunarMote>(),
                    -Projectile.velocity * 0.04f,
                    0, moteColor, 0.35f);
                mote.customData = new LunarMoteBehavior(Projectile.Center, orbitAngle)
                {
                    OrbitRadius = 12f,
                    OrbitSpeed = 0.14f,
                    Lifetime = 24,
                    FadePower = 0.91f
                };
            }

            // STAR POINT TWINKLES — sharp sparkles scattered along trajectory
            if (Main.rand.NextBool(3))
            {
                Color starColor = Color.Lerp(MoonlightsCallingVFX.PrismViolet,
                    MoonlightVFXLibrary.MoonWhite, Main.rand.NextFloat(0.6f));
                Dust star = Dust.NewDustPerfect(
                    Projectile.Center + Main.rand.NextVector2Circular(6f, 6f),
                    ModContent.DustType<StarPointDust>(),
                    -Projectile.velocity * 0.06f, 0, starColor, 0.2f);
                star.customData = new StarPointBehavior
                {
                    RotationSpeed = 0.14f,
                    TwinkleFrequency = 0.5f,
                    Lifetime = 20,
                    FadeStartTime = 5
                };
            }

            // ===================================================================
            //  INTERVAL VFX — timed spectral bursts
            // ===================================================================

            // Music note storm — every 3 frames, 2 notes at velocity 8f
            if (frame % 3 == 0)
            {
                MoonlightVFXLibrary.SpawnMusicNotes(Projectile.Center, 2, 8f, 0.8f, 1.0f, 30);
            }

            // RESONANT PULSE RING — expanding spectral ring every 6 frames
            if (frame % 6 == 0)
            {
                Color ringColor = MoonlightsCallingVFX.GetRefractionColor(
                    (frame * 0.04f) % 1f, VFXIntensityTier);
                Dust pulse = Dust.NewDustPerfect(Projectile.Center,
                    ModContent.DustType<ResonantPulseDust>(),
                    Vector2.Zero, 0, ringColor, 0.3f);
                pulse.customData = new ResonantPulseBehavior
                {
                    ExpansionRate = 0.04f,
                    ExpansionDecay = 0.94f,
                    Lifetime = 18,
                    PulseFrequency = 0.25f
                };
            }

            // SCREEN SHAKE — periodic rumble while the beam is alive
            if (frame % 8 == 0)
            {
                MagnumScreenEffects.AddScreenShake(0.5f);
            }

            // ===================================================================
            //  DYNAMIC PRISMATIC LIGHTING
            // ===================================================================

            float lifetimeRatio = Projectile.timeLeft / 120f;
            float lightPulse = 0.85f + MathF.Sin(Main.GlobalTimeWrappedHourly * 5f) * 0.15f;
            Color lightColor = Color.Lerp(MoonlightsCallingVFX.PrismViolet,
                MoonlightsCallingVFX.RefractedBlue,
                MathF.Sin(Main.GlobalTimeWrappedHourly * 4f) * 0.5f + 0.5f);
            Lighting.AddLight(Projectile.Center, lightColor.ToVector3() * (1.0f + lifetimeRatio * 0.5f) * lightPulse);

            FrameCounter++;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Extended Musical Dissonance debuff — 360 frames (6 seconds)
            target.AddBuff(ModContent.BuffType<MusicsDissonance>(), 360);

            // Maximum-intensity prismatic impact VFX
            MoonlightsCallingVFX.OnHitImpact(target.Center, VFXIntensityTier, hit.Crit);

            // Spawn 3 seeking moonlight crystals on every hit
            if (Main.myPlayer == Projectile.owner)
            {
                SeekingCrystalHelper.SpawnMoonlightCrystals(
                    Projectile.GetSource_FromThis(),
                    target.Center,
                    Projectile.velocity,
                    (int)(Projectile.damage * 0.15f),
                    Projectile.knockBack,
                    Projectile.owner,
                    3);
            }
        }

        public override void OnKill(int timeLeft)
        {
            Vector2 deathPos = Projectile.Center;

            // === GRAND SPECTRAL DETONATION via MoonlightsCallingVFX finale ===
            MoonlightsCallingVFX.OnBeamFinale(deathPos, VFXIntensityTier);

            // === 12-RAY PRISMATIC SHARD STARBURST ===
            for (int i = 0; i < 12; i++)
            {
                float angle = MathHelper.TwoPi * i / 12f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(5f, 9f);
                Color shardColor = MoonlightsCallingVFX.GetRefractionColor((float)i / 12f, VFXIntensityTier);
                Dust shard = Dust.NewDustPerfect(deathPos,
                    ModContent.DustType<PrismaticShardDust>(),
                    vel, 0, shardColor, 0.4f);
                shard.customData = new PrismaticShardBehavior
                {
                    BaseHue = (float)i / 12f,
                    HueRange = 0.5f,
                    RotationSpeed = 0.14f,
                    BaseScale = 0.4f,
                    Lifetime = 35
                };
            }

            // === 5 EXPANDING RESONANT PULSE SPECTRAL RINGS ===
            for (int i = 0; i < 5; i++)
            {
                Color ringColor = MoonlightsCallingVFX.GetRefractionColor(i / 5f, VFXIntensityTier);
                Dust pulse = Dust.NewDustPerfect(deathPos,
                    ModContent.DustType<ResonantPulseDust>(),
                    Vector2.Zero, 0, ringColor,
                    0.3f + i * 0.08f);
                pulse.customData = new ResonantPulseBehavior
                {
                    ExpansionRate = 0.05f + i * 0.015f,
                    ExpansionDecay = 0.93f,
                    Lifetime = 20 + i * 5,
                    PulseFrequency = 0.2f + i * 0.05f
                };
            }

            // === 4 GOD RAYS — brilliant prismatic explosion ===
            GodRaySystem.CreateBurst(deathPos, MoonlightsCallingVFX.PrismViolet,
                rayCount: 4, radius: 90f, duration: 28,
                GodRaySystem.GodRayStyle.Explosion,
                secondaryColor: MoonlightsCallingVFX.RefractedBlue);

            // === SCREEN EFFECTS — massive impact ===
            if (AdaptiveQualityManager.Instance?.CurrentQuality >= AdaptiveQualityManager.QualityLevel.Medium)
            {
                ScreenDistortionManager.TriggerRipple(deathPos,
                    MoonlightsCallingVFX.PrismViolet, 0.6f, 22);
            }
            MagnumScreenEffects.AddScreenShake(5f);

            // Death sound — resonant crescendo finale
            SoundEngine.PlaySound(SoundID.Item122 with
            {
                Volume = 0.9f,
                Pitch = 0.4f
            }, deathPos);

            // Intense lighting at death position
            Lighting.AddLight(deathPos, MoonlightVFXLibrary.MoonWhite.ToVector3() * 2f);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;

            // ===================================================================
            //  CALAMITY-STYLE COSMIC TRAIL — wide prismatic beam trail
            // ===================================================================

            if (Projectile.oldPos.Length > 1)
            {
                Vector2[] trailPositions = new Vector2[Projectile.oldPos.Length];
                float[] trailRotations = new float[Projectile.oldPos.Length];
                int validCount = 0;

                for (int i = 0; i < Projectile.oldPos.Length; i++)
                {
                    if (Projectile.oldPos[i] == Vector2.Zero) break;
                    trailPositions[i] = Projectile.oldPos[i] + Projectile.Size / 2f;
                    trailRotations[i] = Projectile.oldRot[i];
                    validCount++;
                }

                if (validCount > 1)
                {
                    if (validCount < trailPositions.Length)
                    {
                        Array.Resize(ref trailPositions, validCount);
                        Array.Resize(ref trailRotations, validCount);
                    }

                    // Cycling prismatic primary/secondary colors
                    float colorCycle = Main.GlobalTimeWrappedHourly * 2f;
                    Color primaryColor = Color.Lerp(MoonlightsCallingVFX.PrismViolet,
                        MoonlightsCallingVFX.SpectralCyan,
                        MathF.Sin(colorCycle) * 0.5f + 0.5f);
                    Color secondaryColor = Color.Lerp(MoonlightsCallingVFX.RefractedBlue,
                        MoonlightsCallingVFX.RefractionLavender,
                        MathF.Sin(colorCycle + 1.5f) * 0.5f + 0.5f);

                    CalamityStyleTrailRenderer.DrawTrailWithBloom(
                        trailPositions, trailRotations,
                        CalamityStyleTrailRenderer.TrailStyle.Cosmic,
                        baseWidth: 20f,
                        primaryColor: primaryColor,
                        secondaryColor: secondaryColor,
                        intensity: 1.2f,
                        bloomMultiplier: 3.5f);
                }
            }

            // ===================================================================
            //  BEAM BODY BLOOM — 5-layer prismatic bloom stack at max intensity
            // ===================================================================

            MoonlightsCallingVFX.DrawBeamBloom(sb, Projectile.Center, VFXIntensityTier);

            // ===================================================================
            //  MOTION BLUR BLOOM — velocity-based directional stretch
            // ===================================================================

            if (Projectile.velocity.LengthSquared() > 4f)
            {
                Texture2D texture = TextureAssets.Projectile[Type].Value;
                MotionBlurBloomRenderer.DrawProjectile(sb, texture, Projectile,
                    MoonlightsCallingVFX.PrismViolet, MoonlightsCallingVFX.RefractedBlue,
                    intensityMult: 0.7f);
            }

            return false;
        }
    }
}
