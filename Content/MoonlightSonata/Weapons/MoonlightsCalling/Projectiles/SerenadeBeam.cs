using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MagnumOpus.Content.MoonlightSonata.Weapons.MoonlightsCalling.Buffs;
using MagnumOpus.Content.MoonlightSonata.Weapons.MoonlightsCalling.Dusts;
using MagnumOpus.Content.MoonlightSonata.Weapons.MoonlightsCalling.Particles;
using MagnumOpus.Content.MoonlightSonata.Weapons.MoonlightsCalling.Primitives;
using MagnumOpus.Content.MoonlightSonata.Weapons.MoonlightsCalling.Utilities;
using MagnumOpus.Content.MoonlightSonata;
using Terraria;
using Terraria.Audio;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using static MagnumOpus.Content.MoonlightSonata.Weapons.MoonlightsCalling.Utilities.SerenadeUtils;

namespace MagnumOpus.Content.MoonlightSonata.Weapons.MoonlightsCalling.Projectiles
{
    /// <summary>
    /// SerenadeBeam — the primary bouncing beam projectile for Moonlight's Calling.
    /// 
    /// Behavior:
    /// - Fires as a prismatic bolt with a trailing beam rendered via the PrismaticBeam shader.
    /// - Bounces off tiles (max 5 bounces), each bounce:
    ///   · Spawns a RefractionRipple VFX particle burst
    ///   · Increases spectral spread (more rainbow colors in beam)
    ///   · Adds prismatic charge to the player
    ///   · After 3+ bounces: spawns SpectralChild beams that split outward
    /// - Final bounce (5th) triggers a prismatic detonation
    /// - Has homing after 2+ bounces (gentle curve toward nearest NPC)
    /// - Trail stored in oldPos[] for GPU rendering
    /// 
    /// ai[0] = bounce count (0-5)
    /// ai[1] = homing delay timer
    /// localAI[0] = total alive time
    /// localAI[1] = spectral spread phase (0-1, increases with bounces)
    /// </summary>
    public class SerenadeBeam : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_1";

        // === CONSTANTS ===
        private const int MaxBounces = 5;
        private const int SplitBounceThreshold = 3;
        private const int HomingStartBounce = 2;
        private const float HomingRange = 1400f;
        private const float HomingStrength = 0.04f;
        private const float BeamWidth = 24f;
        private const int TrailLength = 30;

        // === PROPERTIES ===
        private Player Owner => Main.player[Projectile.owner];
        private int BounceCount => (int)Projectile.ai[0];
        private float SpectralPhase => Projectile.localAI[1];
        private float AliveTime => Projectile.localAI[0];

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = TrailLength;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 12;
            Projectile.height = 12;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 3;
            Projectile.timeLeft = 300;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.alpha = 0;
            Projectile.extraUpdates = 1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }

        public override void AI()
        {
            Projectile.localAI[0]++;
            Projectile.rotation = Projectile.velocity.ToRotation();

            // Homing after enough bounces
            if (BounceCount >= HomingStartBounce && AliveTime > 15)
            {
                NPC target = ClosestNPCAt(Projectile.Center, HomingRange);
                if (target != null)
                {
                    Vector2 desiredDir = Projectile.SafeDirectionTo(target.Center);
                    float speed = Projectile.velocity.Length();
                    Vector2 currentDir = Vector2.Normalize(Projectile.velocity);

                    // Gentle curve — strength increases with bounces
                    float strength = HomingStrength * (1f + BounceCount * 0.3f);
                    Vector2 newDir = Vector2.Lerp(currentDir, desiredDir, strength);
                    newDir.Normalize();
                    Projectile.velocity = newDir * speed;
                }
            }

            // Spawn flight particles
            SpawnFlightParticles();

            // Emit light
            float intensity = GetBounceIntensity(BounceCount, MaxBounces);
            Color lightColor = GetBeamGradient(SpectralPhase);
            Lighting.AddLight(Projectile.Center, lightColor.ToVector3() * intensity * 0.8f);
        }

        private void SpawnFlightParticles()
        {
            if (Main.dedServ) return;

            // Prismatic sparks along the beam path
            if (AliveTime % 2 == 0)
            {
                float spread = SpectralPhase;
                Color startCol = GetBeamGradient(Main.rand.NextFloat());
                Color endCol = GetBeamGradient(Main.rand.NextFloat());
                Vector2 perpVel = Projectile.velocity.RotateBy(MathHelper.PiOver2) * Main.rand.NextFloat(-0.3f, 0.3f);

                SerenadeParticleHandler.Spawn(new PrismaticSparkParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(6, 6),
                    perpVel - Projectile.velocity * 0.1f,
                    startCol, endCol,
                    0.3f + spread * 0.3f,
                    20 + Main.rand.Next(15)
                ));
            }

            // Music notes at higher bounce counts
            if (BounceCount >= 2 && AliveTime % 12 == 0)
            {
                SerenadeParticleHandler.Spawn(new SpectralNoteParticle(
                    Projectile.Center,
                    Main.rand.NextVector2Circular(1.5f, 1.5f) - Projectile.velocity * 0.05f,
                    0.5f + SpectralPhase * 0.3f,
                    60 + Main.rand.Next(30)
                ));
            }

            // Dust trail
            if (AliveTime % 3 == 0)
            {
                int d = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height,
                    ModContent.DustType<PrismaticDust>(),
                    -Projectile.velocity.X * 0.15f, -Projectile.velocity.Y * 0.15f);
                Main.dust[d].scale = 0.7f + SpectralPhase * 0.5f;
                Main.dust[d].noGravity = true;
            }
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Projectile.ai[0]++; // Increment bounce count
            Projectile.localAI[1] = Math.Min(1f, Projectile.ai[0] / (float)MaxBounces); // Update spectral phase

            if (BounceCount > MaxBounces)
            {
                // Final detonation
                SpawnPrismaticDetonation();
                return true; // Kill projectile
            }

            // Mirror velocity on the axis that changed
            if (Math.Abs(Projectile.velocity.X - oldVelocity.X) > 0.01f)
                Projectile.velocity.X = -oldVelocity.X;
            if (Math.Abs(Projectile.velocity.Y - oldVelocity.Y) > 0.01f)
                Projectile.velocity.Y = -oldVelocity.Y;

            // Slight speed boost per bounce to maintain energy
            Projectile.velocity *= 1.05f;

            // Bounce VFX
            SpawnBounceVFX();

            // Grant prismatic charge to owner
            if (Projectile.owner == Main.myPlayer)
                Owner.Serenade().AddCharge(1);

            // Spectral child split after threshold
            if (BounceCount >= SplitBounceThreshold)
                SpawnSpectralChildren();

            SoundEngine.PlaySound(SoundID.Item29 with { Pitch = 0.3f + BounceCount * 0.1f, Volume = 0.7f },
                Projectile.Center);

            return false;
        }

        private void SpawnBounceVFX()
        {
            if (Main.dedServ) return;

            float intensity = GetBounceIntensity(BounceCount, MaxBounces);

            // Refraction bloom at bounce point
            SerenadeParticleHandler.Spawn(new RefractionBloomParticle(
                Projectile.Center, GetBeamGradient(SpectralPhase),
                1.5f + intensity, 30 + BounceCount * 6
            ));

            // Radial prismatic spark burst
            int sparkCount = 8 + BounceCount * 3;
            for (int i = 0; i < sparkCount; i++)
            {
                float angle = MathHelper.TwoPi * i / sparkCount;
                Vector2 vel = angle.ToRotationVector2() * (3f + Main.rand.NextFloat(2f));
                Color col = GetSpectralColor(i % SerenadeUtils.SpectralColors.Length);

                SerenadeParticleHandler.Spawn(new PrismaticSparkParticle(
                    Projectile.Center, vel, col, MoonWhite,
                    0.5f + intensity * 0.3f, 25 + Main.rand.Next(15)
                ));
            }

            // Prism shards flying outward
            for (int i = 0; i < 4 + BounceCount; i++)
            {
                Vector2 shardVel = Main.rand.NextVector2CircularEdge(4f, 4f);
                Color col = GetSpectralColor(Main.rand.Next(7));
                SerenadeParticleHandler.Spawn(new PrismShardParticle(
                    Projectile.Center, shardVel, col, 0.4f + Main.rand.NextFloat(0.3f),
                    30 + Main.rand.Next(20)
                ));
            }

            // Dust burst
            for (int i = 0; i < 10; i++)
            {
                Vector2 dustVel = Main.rand.NextVector2CircularEdge(5f, 5f);
                int d = Dust.NewDust(Projectile.Center - new Vector2(4), 8, 8,
                    ModContent.DustType<PrismaticDust>(), dustVel.X, dustVel.Y);
                Main.dust[d].scale = 1.2f;
                Main.dust[d].noGravity = true;
            }

            // Hue-shifting music notes — the harp's refracted harmonics
            MoonlightVFXLibrary.SpawnMusicNotes(Projectile.Center, count: 2 + BounceCount,
                spread: 15f + BounceCount * 5f, minScale: 0.5f, maxScale: 0.9f, lifetime: 35);
        }

        private void SpawnSpectralChildren()
        {
            if (Projectile.owner != Main.myPlayer) return;

            // Spawn 3 spectral child beams fanning out
            int childCount = 3;
            float fanAngle = MathHelper.ToRadians(40);

            for (int i = 0; i < childCount; i++)
            {
                float angleOffset = -fanAngle + fanAngle * 2f * i / (childCount - 1);
                Vector2 childVel = Projectile.velocity.RotateBy(angleOffset) * 0.8f;

                Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    Projectile.Center, childVel,
                    ModContent.ProjectileType<SpectralChildBeam>(),
                    (int)(Projectile.damage * 0.5f), Projectile.knockBack * 0.3f,
                    Projectile.owner,
                    ai0: i // spectral color index
                );
            }
        }

        private void SpawnPrismaticDetonation()
        {
            if (Projectile.owner != Main.myPlayer) return;

            Projectile.NewProjectile(
                Projectile.GetSource_FromThis(),
                Projectile.Center, Vector2.Zero,
                ModContent.ProjectileType<PrismaticDetonation>(),
                Projectile.damage * 2, Projectile.knockBack * 2f,
                Projectile.owner
            );
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<MusicalDissonance>(), 300);

            // On-hit prismatic spark burst
            if (!Main.dedServ)
            {
                for (int i = 0; i < 5; i++)
                {
                    Vector2 vel = Main.rand.NextVector2CircularEdge(3f, 3f);
                    Color col = GetSpectralColor(Main.rand.Next(7));
                    SerenadeParticleHandler.Spawn(new PrismaticSparkParticle(
                        target.Center, vel, col, MoonWhite, 0.4f, 20
                    ));
                }
            }
        }

        public override Color? GetAlpha(Color lightColor)
        {
            return Color.White * Projectile.Opacity;
        }

        // === RENDERING ===

        public override bool PreDraw(ref Color lightColor)
        {
            // Draw trail via GPU primitive renderer
            DrawBeamTrail();

            // Draw beam head glow
            DrawBeamHead();

            return false; // Skip default drawing
        }

        private void DrawBeamTrail()
        {
            // Build position list from oldPos
            List<Vector2> trailPositions = new();
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                Vector2 pos = Projectile.oldPos[i];
                if (pos == Vector2.Zero) break;
                trailPositions.Add(pos + Projectile.Size * 0.5f);
            }
            if (trailPositions.Count < 3) return;

            float bounceIntensity = GetBounceIntensity(BounceCount, MaxBounces);

            // Pass 1: Glow underlayer (wider, softer)
            MiscShaderData glowShader = GameShaders.Misc.TryGetValue("MagnumOpus:SerenadePrismaticGlow", out var gs) ? gs : null;
            if (glowShader != null)
            {
                glowShader.Shader?.Parameters["uColor"]?.SetValue(PrismViolet.ToVector3());
                glowShader.Shader?.Parameters["uSecondaryColor"]?.SetValue(RefractedBlue.ToVector3());
                glowShader.Shader?.Parameters["uOpacity"]?.SetValue(0.5f * bounceIntensity);
                glowShader.Shader?.Parameters["uTime"]?.SetValue(AliveTime * 0.02f);
                glowShader.Shader?.Parameters["uIntensity"]?.SetValue(bounceIntensity);
                glowShader.Shader?.Parameters["uPhase"]?.SetValue(SpectralPhase);
                glowShader.Shader?.Parameters["uScrollSpeed"]?.SetValue(2f);
                glowShader.Shader?.Parameters["uOverbrightMult"]?.SetValue(1.3f);
            }

            var glowSettings = new SerenadeTrailSettings(
                widthFunction: (t, _) => BeamWidth * 2f * (1f - t * 0.5f) * bounceIntensity,
                colorFunction: (t, _) => GetBeamGradient(t) * (0.5f * (1f - t)),
                smoothen: true,
                shader: glowShader
            );
            SerenadeTrailRenderer.RenderTrail(trailPositions, glowSettings);

            // Pass 2: Main beam body (sharp, spectral)
            MiscShaderData beamShader = GameShaders.Misc.TryGetValue("MagnumOpus:SerenadePrismaticBeam", out var bs) ? bs : null;
            if (beamShader != null)
            {
                beamShader.Shader?.Parameters["uColor"]?.SetValue(PrismViolet.ToVector3());
                beamShader.Shader?.Parameters["uSecondaryColor"]?.SetValue(RefractedBlue.ToVector3());
                beamShader.Shader?.Parameters["uOpacity"]?.SetValue(0.9f);
                beamShader.Shader?.Parameters["uTime"]?.SetValue(AliveTime * 0.02f);
                beamShader.Shader?.Parameters["uIntensity"]?.SetValue(bounceIntensity);
                beamShader.Shader?.Parameters["uPhase"]?.SetValue(SpectralPhase);
                beamShader.Shader?.Parameters["uScrollSpeed"]?.SetValue(3f);
                beamShader.Shader?.Parameters["uDistortionAmt"]?.SetValue(0.04f + SpectralPhase * 0.06f);
                beamShader.Shader?.Parameters["uOverbrightMult"]?.SetValue(1.5f);
                // Musical wave noise pattern — adds flowing harmonic texture to the beam body
                beamShader.UseImage1(ModContent.Request<Texture2D>(
                    "MagnumOpus/Assets/VFX Asset Library/NoiseTextures/MusicalWavePattern"));
                beamShader.Shader?.Parameters["uHasSecondaryTex"]?.SetValue(1f);
                beamShader.Shader?.Parameters["uSecondaryTexScale"]?.SetValue(1.5f + SpectralPhase * 0.5f);
                beamShader.Shader?.Parameters["uSecondaryTexScroll"]?.SetValue(1.2f + SpectralPhase * 0.8f);
            }

            var beamSettings = new SerenadeTrailSettings(
                widthFunction: (t, _) => BeamWidth * (1f - t * 0.4f) * bounceIntensity,
                colorFunction: (t, _) => GetBeamGradient(t * SpectralPhase) * (1f - t * 0.3f),
                smoothen: true,
                shader: beamShader
            );
            SerenadeTrailRenderer.RenderTrail(trailPositions, beamSettings);
        }

        /// <summary>Multi-layered spectral bloom head — each bounce adds richer prismatic depth.
        /// Uses SoftRadialBloom for wide atmospheric halo + PointBloom for sharp core.
        /// Spectral color bands shift with SpectralPhase for refracting-light effect.</summary>
        private void DrawBeamHead()
        {
            var pointBloom = SerenadeTextures.PointBloom;
            if (pointBloom == null) return;

            var softBloom = ModContent.Request<Texture2D>(
                "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftRadialBloom").Value;

            float bounceIntensity = GetBounceIntensity(BounceCount, MaxBounces);
            float pulse = 1f + MathF.Sin(AliveTime * 0.3f) * 0.15f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            // Layer 1: Wide atmospheric halo (SoftRadialBloom) — prismatic outer glow
            Color haloColor = GetBeamGradient(SpectralPhase * 0.3f) with { A = 0 };
            Main.spriteBatch.Draw(softBloom, drawPos, null,
                haloColor * 0.2f * bounceIntensity * pulse,
                0f, softBloom.Size() * 0.5f, 1.6f * bounceIntensity * pulse,
                SpriteEffects.None, 0f);

            // Layer 2: Spectral band ring (SoftRadialBloom) — shifts color with phase
            Color spectralRing = GetBeamGradient(SpectralPhase * 0.7f) with { A = 0 };
            Main.spriteBatch.Draw(softBloom, drawPos, null,
                spectralRing * 0.35f * bounceIntensity * pulse,
                AliveTime * 0.02f, softBloom.Size() * 0.5f, 0.9f * bounceIntensity * pulse,
                SpriteEffects.None, 0f);

            // Layer 3: Bright core glow (PointBloom) — refracted focal point
            Color coreColor = GetBeamGradient(SpectralPhase * 0.5f) with { A = 0 };
            Main.spriteBatch.Draw(pointBloom, drawPos, null,
                coreColor * 0.6f * bounceIntensity * pulse,
                0f, pointBloom.Size() * 0.5f, 0.6f * bounceIntensity * pulse,
                SpriteEffects.None, 0f);

            // Layer 4: White-hot pinpoint (PointBloom) — coherent light focus
            Main.spriteBatch.Draw(pointBloom, drawPos, null,
                (MoonWhite with { A = 0 }) * 0.7f * bounceIntensity,
                0f, pointBloom.Size() * 0.5f, 0.25f * bounceIntensity * pulse,
                SpriteEffects.None, 0f);
        }
    }
}
