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
using MagnumOpus.Common.Systems.VFX.Trails;
using MagnumOpus.Content.MoonlightSonata;
using MagnumOpus.Content.MoonlightSonata.Dusts;
using MagnumOpus.Content.MoonlightSonata.Weapons.MoonlightsCalling;

namespace MagnumOpus.Content.MoonlightSonata.Projectiles
{
    /// <summary>
    /// Moonlight beam projectile — "The Serenade's Voice".
    /// Bouncing magic beam that intensifies with each surface bounce,
    /// with PrismaticBeam.fx shader-driven spectral trail rendering.
    ///
    /// Overhaul: PrismaticBeam shader integration for GPU-rendered spectral trails,
    /// SpectralChildBeam spawning for distinct visual beam splitting,
    /// RefractionRipple shader overlay at bounce points.
    ///
    /// Bounce behavior:
    ///   Bounce 1-2: Normal reflection + prismatic VFX + shader trail intensifies
    ///   Bounce 3:   Reflection + 2 SpectralChildBeam at ±30° (red + cyan hues)
    ///   Bounce 4:   Reflection + 2 SpectralChildBeam at ±25° (orange + blue hues)
    ///   Bounce 5:   Grand finale — full spectral detonation + refraction ripple
    ///
    /// Rendering pipeline (PreDraw):
    ///   1. CalamityStyleTrailRenderer.Cosmic — base trail geometry
    ///   2. PrismaticBeam.fx PrismaticBeamMain — shader-driven spectral color splitting
    ///   3. PrismaticBeam.fx PrismaticBeamGlow — shader bloom pass
    ///   4. DrawBeamBloom — 5-layer additive bloom stack
    ///   5. MotionBlurBloomRenderer — velocity stretch
    /// </summary>
    public class MoonlightBeam : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/SoftGlow3";

        private int bounceCount = 0;
        private const int MaxBounces = 5;

        /// <summary>Spectral phase for shader — bounceCount / MaxBounces (0 = first shot, 1 = final bounce).</summary>
        private float SpectralPhase => (float)bounceCount / MaxBounces;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 20;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 8;
            Projectile.timeLeft = 180;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.alpha = 255;
            Projectile.extraUpdates = 3;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 15;
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            bounceCount++;

            if (bounceCount >= MaxBounces)
            {
                // Grand finale — full spectral detonation + refraction ripple shader burst
                MoonlightsCallingVFX.OnBeamFinale(Projectile.Center, bounceCount);
                MoonlightsCallingVFX.DrawRefractionRippleBurst(Projectile.Center, bounceCount);

                SoundEngine.PlaySound(SoundID.Item10, Projectile.Center);
                return true;
            }

            // Bounce off walls
            if (Projectile.velocity.X != oldVelocity.X)
                Projectile.velocity.X = -oldVelocity.X;
            if (Projectile.velocity.Y != oldVelocity.Y)
                Projectile.velocity.Y = -oldVelocity.Y;

            // Bounce VFX — escalating prismatic explosion
            MoonlightsCallingVFX.OnBounceVFX(Projectile.Center, Projectile.velocity, bounceCount);

            // Bounce sound with escalating pitch
            SoundEngine.PlaySound(SoundID.Item10 with
            {
                Volume = 0.5f + bounceCount * 0.1f,
                Pitch = -0.2f + bounceCount * 0.15f
            }, Projectile.Center);

            // SPECTRAL SPLIT — spawn SpectralChildBeam on bounces 3+
            if (bounceCount >= 3 && Main.myPlayer == Projectile.owner)
            {
                float spreadAngle = MathHelper.ToRadians(30f - (bounceCount - 3) * 5f);
                float childSpeed = Projectile.velocity.Length() * 0.8f;

                // Two child beams with distinct spectral hues per bounce tier
                float[] childHues = bounceCount switch
                {
                    3 => new[] { 0.0f, 0.5f },   // Red + Cyan
                    4 => new[] { 0.08f, 0.58f },  // Orange + Blue
                    _ => new[] { 0.15f, 0.65f }   // Yellow-green + Violet
                };

                for (int i = 0; i < 2; i++)
                {
                    int side = i == 0 ? -1 : 1;
                    Vector2 childVel = Projectile.velocity.SafeNormalize(Vector2.UnitX)
                        .RotatedBy(spreadAngle * side) * childSpeed;

                    Projectile.NewProjectile(
                        Projectile.GetSource_FromThis(),
                        Projectile.Center, childVel,
                        ModContent.ProjectileType<SpectralChildBeam>(),
                        (int)(Projectile.damage * 0.5f),
                        Projectile.knockBack * 0.3f, Projectile.owner,
                        ai0: childHues[i], ai1: bounceCount);
                }

                MoonlightsCallingVFX.SpectralSplitVFX(Projectile.Center, bounceCount);
            }

            return false;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();

            // Per-frame trail VFX via MoonlightsCallingVFX
            MoonlightsCallingVFX.BeamTrailFrame(Projectile.Center, Projectile.velocity, bounceCount);

            // Music notes dense in trail — every 4 frames (signature "Serenade" effect)
            if ((int)Projectile.ai[0] % 4 == 0)
            {
                MoonlightVFXLibrary.SpawnMusicNotes(Projectile.Center, 1, 5f, 0.7f, 0.85f, 25);
            }
            Projectile.ai[0]++;

            // Orbiting LunarMotes — crescent notes circling the beam
            if ((int)Projectile.ai[0] % 8 == 0)
            {
                float orbitPhase = Projectile.ai[0] * 0.3f;
                Color moteColor = Color.Lerp(MoonlightsCallingVFX.PrismViolet,
                    MoonlightsCallingVFX.RefractedBlue, MathF.Sin(orbitPhase) * 0.5f + 0.5f);
                Dust mote = Dust.NewDustPerfect(Projectile.Center,
                    ModContent.DustType<LunarMote>(),
                    -Projectile.velocity * 0.02f,
                    0, moteColor, 0.22f + bounceCount * 0.03f);
                mote.customData = new LunarMoteBehavior(Projectile.Center, orbitPhase)
                {
                    OrbitRadius = 8f + bounceCount * 1.5f,
                    OrbitSpeed = 0.15f,
                    Lifetime = 18,
                    FadePower = 0.9f
                };
            }

            // StarPointDust sharp twinkles along path
            if ((int)Projectile.ai[0] % 6 == 0)
            {
                float orbitPhase = Projectile.ai[0] * 0.3f;
                for (int i = 0; i < 2; i++)
                {
                    float angle = orbitPhase + MathHelper.Pi * i;
                    float radius = 6f + MathF.Sin(orbitPhase + i) * 3f;
                    Vector2 starPos = Projectile.Center + angle.ToRotationVector2() * radius;
                    Color starColor = MoonlightsCallingVFX.GetRefractionColor((float)i / 2f, bounceCount);
                    Dust star = Dust.NewDustPerfect(starPos,
                        ModContent.DustType<StarPointDust>(),
                        -Projectile.velocity * 0.03f, 0, starColor, 0.15f);
                    star.customData = new StarPointBehavior
                    {
                        RotationSpeed = 0.15f,
                        TwinkleFrequency = 0.5f,
                        Lifetime = 15,
                        FadeStartTime = 4
                    };
                }
            }

            // PrismaticShardDust core trail
            if (Main.rand.NextBool(3))
            {
                Color shardColor = MoonlightsCallingVFX.GetRefractionColor(Main.rand.NextFloat(), bounceCount);
                Dust shard = Dust.NewDustPerfect(Projectile.Center,
                    ModContent.DustType<PrismaticShardDust>(),
                    -Projectile.velocity * 0.05f + Main.rand.NextVector2Circular(0.5f, 0.5f),
                    0, shardColor, 0.18f + bounceCount * 0.02f);
                shard.customData = new PrismaticShardBehavior
                {
                    BaseHue = 0.7f + bounceCount * 0.04f,
                    HueRange = 0.1f + bounceCount * 0.04f,
                    RotationSpeed = 0.08f,
                    BaseScale = 0.18f + bounceCount * 0.02f,
                    Lifetime = 20
                };
            }

            // Prismatic lighting
            MoonlightsCallingVFX.AddPrismaticLight(Projectile.Center, 0.6f + bounceCount * 0.12f, bounceCount);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<Debuffs.MusicsDissonance>(), 180 + bounceCount * 30);

            // Prismatic impact VFX
            MoonlightsCallingVFX.OnHitImpact(target.Center, bounceCount, hit.Crit);
        }

        public override void OnKill(int timeLeft)
        {
            // Death VFX — prismatic fade with custom dusts
            MoonlightVFXLibrary.ProjectileImpact(Projectile.Center, 0.5f);

            // Spectral shard scatter on death
            for (int i = 0; i < 4; i++)
            {
                float angle = MathHelper.TwoPi * i / 4f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(1.5f, 3.5f);
                Color shardColor = MoonlightsCallingVFX.GetRefractionColor(i / 4f, bounceCount);
                Dust shard = Dust.NewDustPerfect(Projectile.Center,
                    ModContent.DustType<PrismaticShardDust>(),
                    vel, 0, shardColor, 0.2f);
                shard.customData = new PrismaticShardBehavior(0.7f + i * 0.1f, 0.2f, 18);
            }

            // ResonantPulseDust ring on death
            Dust pulse = Dust.NewDustPerfect(Projectile.Center,
                ModContent.DustType<ResonantPulseDust>(),
                Vector2.Zero, 0, MoonlightsCallingVFX.PrismViolet, 0.2f);
            pulse.customData = new ResonantPulseBehavior(0.03f, 14);

            // Small spectral ring cascade
            for (int i = 0; i < 3; i++)
            {
                Color ringColor = MoonlightsCallingVFX.GetRefractionColor(i / 3f, bounceCount);
                CustomParticles.HaloRing(Projectile.Center, ringColor, 0.2f + i * 0.08f, 12 + i * 3);
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            float spectralPhase = SpectralPhase;

            // === BUILD TRAIL ARRAYS ===
            Vector2[] trailPositions = null;
            float[] trailRotations = null;
            int validCount = 0;

            if (Projectile.oldPos.Length > 1)
            {
                trailPositions = new Vector2[Projectile.oldPos.Length];
                trailRotations = new float[Projectile.oldPos.Length];

                for (int i = 0; i < Projectile.oldPos.Length; i++)
                {
                    if (Projectile.oldPos[i] == Vector2.Zero) break;
                    trailPositions[i] = Projectile.oldPos[i] + Projectile.Size / 2f;
                    trailRotations[i] = Projectile.oldRot[i];
                    validCount++;
                }

                if (validCount > 1 && validCount < trailPositions.Length)
                {
                    Array.Resize(ref trailPositions, validCount);
                    Array.Resize(ref trailRotations, validCount);
                }
            }

            // === LAYER 1: CALAMITY-STYLE BASE TRAIL ===
            if (validCount > 1)
            {
                Color primaryColor = Color.Lerp(MoonlightVFXLibrary.Violet,
                    MoonlightsCallingVFX.PrismViolet, bounceCount * 0.15f);
                Color secondaryColor = Color.Lerp(MoonlightVFXLibrary.IceBlue,
                    MoonlightsCallingVFX.RefractedBlue, bounceCount * 0.15f);

                CalamityStyleTrailRenderer.DrawTrailWithBloom(
                    trailPositions, trailRotations,
                    CalamityStyleTrailRenderer.TrailStyle.Cosmic,
                    baseWidth: 12f + bounceCount * 2f,
                    primaryColor: primaryColor,
                    secondaryColor: secondaryColor,
                    intensity: 0.8f + bounceCount * 0.1f,
                    bloomMultiplier: 2.0f + bounceCount * 0.3f);
            }

            // === LAYER 2: PRISMATIC BEAM SHADER GLOW (GPU-driven spectral splitting) ===
            if (MoonlightSonataShaderManager.HasPrismaticBeam)
            {
                DrawPrismaticShaderOverlay(sb, spectralPhase);
            }

            // === LAYER 3: BEAM BODY BLOOM ===
            MoonlightsCallingVFX.DrawBeamBloom(sb, Projectile.Center, bounceCount);

            // === LAYER 4: MOTION BLUR BLOOM (velocity-based stretch) ===
            if (Projectile.velocity.LengthSquared() > 4f)
            {
                Texture2D texture = TextureAssets.Projectile[Type].Value;
                MotionBlurBloomRenderer.DrawProjectile(sb, texture, Projectile,
                    MoonlightsCallingVFX.PrismViolet, MoonlightsCallingVFX.RefractedBlue,
                    intensityMult: 0.5f);
            }

            return false;
        }

        /// <summary>
        /// Draws the PrismaticBeam.fx shader glow overlay at the beam position.
        /// Creates a spectral color-splitting effect that widens with each bounce.
        /// Two passes: main spectral body + soft glow bloom.
        /// </summary>
        private void DrawPrismaticShaderOverlay(SpriteBatch sb, float spectralPhase)
        {
            var glowTex = MoonlightSonataTextures.BloomOrb?.Value
                       ?? MagnumTextureRegistry.GetSoftGlow();
            if (glowTex == null) return;

            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = glowTex.Size() * 0.5f;

            // Scale increases with bounces — spectral spread grows
            float baseScale = 0.15f + bounceCount * 0.06f;
            float pulse = 1f + MathF.Sin(Main.GlobalTimeWrappedHourly * 6f + bounceCount) * 0.08f;
            float glowScale = baseScale * pulse;

            try
            {
                // Pass 1: Main prismatic beam overlay
                MoonlightSonataShaderManager.BeginShaderBatch(sb);

                MoonlightSonataShaderManager.ApplyMoonlightsCallingPrismaticBeam(
                    Main.GlobalTimeWrappedHourly, spectralPhase, glowPass: false);

                sb.Draw(glowTex, drawPos, null,
                    Color.White, Projectile.rotation, origin,
                    glowScale * 1.2f, SpriteEffects.None, 0f);

                // Pass 2: Soft glow bloom
                MoonlightSonataShaderManager.ApplyMoonlightsCallingPrismaticBeam(
                    Main.GlobalTimeWrappedHourly, spectralPhase, glowPass: true);

                sb.Draw(glowTex, drawPos, null,
                    Color.White * 0.7f, Projectile.rotation, origin,
                    glowScale * 1.8f, SpriteEffects.None, 0f);

                MoonlightSonataShaderManager.RestoreDefaultBatch(sb);
            }
            catch
            {
                // Fallback: plain additive bloom if shader fails
                try { MoonlightSonataShaderManager.RestoreDefaultBatch(sb); } catch { }
                sb.Draw(glowTex, drawPos, null,
                    MoonlightSonataPalette.Additive(MoonlightsCallingVFX.PrismViolet, 0.3f),
                    Projectile.rotation, origin, glowScale, SpriteEffects.None, 0f);
            }
        }
    }
}
