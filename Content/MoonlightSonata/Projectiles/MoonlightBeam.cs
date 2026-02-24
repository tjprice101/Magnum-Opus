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
using MagnumOpus.Content.MoonlightSonata.Weapons.MoonlightsCalling;

namespace MagnumOpus.Content.MoonlightSonata.Projectiles
{
    /// <summary>
    /// Moonlight beam projectile — "The Serenade's Voice".
    /// Bouncing magic beam that intensifies with each surface bounce.
    ///
    /// VFX overhaul: Raw MagicPixel 4-layer trail replaced with:
    ///   - CalamityStyleTrailRenderer.DrawTrailWithBloom (Cosmic style, prismatic colors)
    ///   - 4-layer {A=0} bloom stack body (no SpriteBatch restart)
    ///   - MotionBlurBloomRenderer for velocity-based stretch
    ///   - MoonlightsCallingVFX for themed bounce/impact/finale effects
    ///   - Music notes dense in trail (every 4 frames)
    /// </summary>
    public class MoonlightBeam : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/SoftGlow3";

        private int bounceCount = 0;
        private const int MaxBounces = 5;

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
                // Grand finale — full spectral explosion
                MoonlightsCallingVFX.OnBeamFinale(Projectile.Center, bounceCount);
                SoundEngine.PlaySound(SoundID.Item10, Projectile.Center);
                return true; // Kill projectile
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

            // Orbiting star points — prismatic motes circling the beam
            if ((int)Projectile.ai[0] % 6 == 0)
            {
                float orbitPhase = Projectile.ai[0] * 0.3f;
                for (int i = 0; i < 3; i++)
                {
                    float angle = orbitPhase + MathHelper.TwoPi * i / 3f;
                    float radius = 8f + MathF.Sin(orbitPhase + i) * 3f;
                    Vector2 starPos = Projectile.Center + angle.ToRotationVector2() * radius;
                    Color starColor = MoonlightsCallingVFX.GetRefractionColor((float)i / 3f, bounceCount);
                    CustomParticles.GenericFlare(starPos, starColor, 0.2f, 10);
                }
            }

            // Core purple dust
            if (Main.rand.NextBool(3))
            {
                Dust core = Dust.NewDustPerfect(Projectile.Center, DustID.PurpleTorch,
                    -Projectile.velocity * 0.05f, 50, default, 1.8f);
                core.noGravity = true;
                core.fadeIn = 1.4f;
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<Debuffs.MusicsDissonance>(), 180);

            // Prismatic impact VFX
            MoonlightsCallingVFX.OnHitImpact(target.Center, bounceCount, hit.Crit);
        }

        public override void OnKill(int timeLeft)
        {
            // Death VFX — gentle prismatic fade
            MoonlightVFXLibrary.ProjectileImpact(Projectile.Center, 0.5f);

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

            // === CALAMITY-STYLE TRAIL RENDERING ===
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

                    // Prismatic color shifts with bounces — the more bounces, the wider the spectral range
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
            }

            // === 4-LAYER {A=0} BLOOM STACK BODY ===
            MoonlightsCallingVFX.DrawBeamBloom(sb, Projectile.Center, bounceCount);

            // === MOTION BLUR BLOOM (velocity-based stretch) ===
            if (Projectile.velocity.LengthSquared() > 4f)
            {
                Texture2D texture = TextureAssets.Projectile[Type].Value;
                MotionBlurBloomRenderer.DrawProjectile(sb, texture, Projectile,
                    MoonlightsCallingVFX.PrismViolet, MoonlightsCallingVFX.RefractedBlue, intensityMult: 0.5f);
            }

            return false;
        }
    }
}
