using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MagnumOpus.Content.MoonlightSonata.Weapons.MoonlightsCalling.Buffs;
using MagnumOpus.Content.MoonlightSonata.Weapons.MoonlightsCalling.Dusts;
using MagnumOpus.Content.MoonlightSonata.Weapons.MoonlightsCalling.Particles;
using MagnumOpus.Content.MoonlightSonata.Weapons.MoonlightsCalling.Primitives;
using MagnumOpus.Content.MoonlightSonata.Weapons.MoonlightsCalling.Utilities;
using Terraria;
using Terraria.Audio;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using static MagnumOpus.Content.MoonlightSonata.Weapons.MoonlightsCalling.Utilities.SerenadeUtils;

namespace MagnumOpus.Content.MoonlightSonata.Weapons.MoonlightsCalling.Projectiles
{
    /// <summary>
    /// SerenadeHoldout — the Serenade Mode channeled mega-beam.
    /// 
    /// Right-click fires this holdout projectile. While the player holds the channel:
    /// - A prismatic mega-beam laser extends from the weapon toward the mouse cursor
    /// - The beam pierces enemies infinitely and passes through tiles
    /// - Beam deals 1.8x damage, costs 40 mana to initiate
    /// - Visual: thick prismatic beam with full spectral spread, refraction ripples
    ///   along its length, music notes cascading off, chromatic aberration at edges
    /// - Duration: 3 seconds max channel time, then 3 second cooldown
    /// - Beam rotates to follow the mouse with smooth interpolation
    /// 
    /// ai[0] = alive timer
    /// ai[1] = beam direction angle
    /// </summary>
    public class SerenadeHoldout : ModProjectile
    {
        // === CONSTANTS ===
        private const int MaxChannelTime = 180; // 3 seconds
        private const float MaxBeamLength = 2400f;
        private const float BeamWidth = 48f;
        private const float AimSpeed = 0.08f;
        private const int BeamPoints = 40;

        // === PROPERTIES ===
        private Player Owner => Main.player[Projectile.owner];
        private float AliveTime { get => Projectile.ai[0]; set => Projectile.ai[0] = value; }
        private float BeamAngle { get => Projectile.ai[1]; set => Projectile.ai[1] = value; }

        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = -1;
            Projectile.timeLeft = MaxChannelTime + 60;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.ownerHitCheck = false;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 8;
        }

        public override bool ShouldUpdatePosition() => false;

        public override void AI()
        {
            AliveTime++;

            // --- Channel check ---
            if (!Owner.active || Owner.dead || Owner.noItems || Owner.CCed)
            {
                EndChannel();
                return;
            }

            // Check if player is still channeling (right mouse held)
            if (!Owner.channel && AliveTime > 5)
            {
                EndChannel();
                return;
            }

            if (AliveTime > MaxChannelTime)
            {
                EndChannel();
                return;
            }

            // --- Owner state management ---
            Owner.heldProj = Projectile.whoAmI;
            Owner.itemTime = 2;
            Owner.itemAnimation = 2;

            // Position at owner center
            Projectile.Center = Owner.MountedCenter;

            // --- Smooth aim toward mouse ---
            float targetAngle = (Main.MouseWorld - Owner.MountedCenter).ToRotation();
            float currentAngle = BeamAngle;

            // Initialize on first frame
            if (AliveTime <= 1)
            {
                BeamAngle = targetAngle;
                currentAngle = targetAngle;
            }

            // Smooth interpolation toward target angle
            float angleDiff = MathHelper.WrapAngle(targetAngle - currentAngle);
            BeamAngle = currentAngle + angleDiff * AimSpeed;

            // Player faces beam direction
            Owner.ChangeDir(MathF.Cos(BeamAngle) >= 0 ? 1 : -1);

            // Rotate held item toward beam
            Projectile.rotation = BeamAngle;
            Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, BeamAngle - MathHelper.PiOver2);

            // --- Mana drain (periodic) ---
            if (AliveTime % 30 == 0 && Projectile.owner == Main.myPlayer)
            {
                if (!Owner.CheckMana(Owner.HeldItem, -1, true, false))
                {
                    EndChannel();
                    return;
                }
            }

            // --- Damage enemies along beam ---
            if (AliveTime % 4 == 0)
                DamageAlongBeam();

            // --- VFX ---
            SpawnChannelVFX();
            EmitBeamLight();
        }

        private void EndChannel()
        {
            if (Projectile.owner == Main.myPlayer)
                Owner.Serenade().StartCooldown();

            // Final burst on end
            if (!Main.dedServ)
                SpawnEndBurst();

            Projectile.Kill();
        }

        private void DamageAlongBeam()
        {
            Vector2 beamDir = BeamAngle.ToRotationVector2();
            Vector2 beamStart = Owner.MountedCenter;

            float step = 32f;
            for (float dist = 0; dist < MaxBeamLength; dist += step)
            {
                Vector2 checkPos = beamStart + beamDir * dist;
                Rectangle checkRect = new((int)checkPos.X - 20, (int)checkPos.Y - 20, 40, 40);

                foreach (NPC npc in Main.ActiveNPCs)
                {
                    if (!npc.active || npc.dontTakeDamage || npc.friendly || npc.immortal) continue;
                    if (!npc.Hitbox.Intersects(checkRect)) continue;

                    // Apply local iframes
                    if (Projectile.localNPCImmunity[npc.whoAmI] > 0) continue;
                    Projectile.localNPCImmunity[npc.whoAmI] = Projectile.localNPCHitCooldown;

                    // Hit!
                    var hitInfo = npc.CalculateHitInfo((int)(Projectile.damage * 1.8f), Owner.direction,
                        false, Projectile.knockBack, Projectile.DamageType, true);

                    npc.StrikeNPC(hitInfo);
                    npc.AddBuff(ModContent.BuffType<MusicalDissonance>(), 300);

                    // Per-hit VFX
                    if (!Main.dedServ)
                    {
                        SerenadeParticleHandler.Spawn(new RefractionBloomParticle(
                            npc.Center, GetSpectralColor(Main.rand.Next(7)), 1f, 15
                        ));
                        for (int i = 0; i < 3; i++)
                        {
                            Vector2 vel = Main.rand.NextVector2CircularEdge(3f, 3f);
                            SerenadeParticleHandler.Spawn(new PrismaticSparkParticle(
                                npc.Center, vel, GetSpectralColor(Main.rand.Next(7)), MoonWhite, 0.4f, 18
                            ));
                        }
                    }

                    if (Main.netMode != NetmodeID.SinglePlayer)
                        NetMessage.SendStrikeNPC(npc, hitInfo);
                }
            }
        }

        private void SpawnChannelVFX()
        {
            if (Main.dedServ) return;

            float channelProgress = AliveTime / (float)MaxChannelTime;
            Vector2 beamDir = BeamAngle.ToRotationVector2();

            // Prismatic sparks along the beam
            if (AliveTime % 2 == 0)
            {
                float dist = Main.rand.NextFloat(MaxBeamLength * 0.8f);
                Vector2 sparkPos = Owner.MountedCenter + beamDir * dist;
                Vector2 perpVel = beamDir.RotateBy(MathHelper.PiOver2) * Main.rand.NextFloat(-2f, 2f);

                Color col = GetSpectralColor(Main.rand.Next(7));
                SerenadeParticleHandler.Spawn(new PrismaticSparkParticle(
                    sparkPos + Main.rand.NextVector2Circular(BeamWidth * 0.3f, BeamWidth * 0.3f),
                    perpVel, col, MoonWhite, 0.35f, 20
                ));
            }

            // Spectral notes floating off beam
            if (AliveTime % 8 == 0)
            {
                float dist = Main.rand.NextFloat(MaxBeamLength * 0.6f);
                Vector2 notePos = Owner.MountedCenter + beamDir * dist;
                Vector2 perpDir = beamDir.RotateBy(MathHelper.PiOver2);
                Vector2 noteVel = perpDir * Main.rand.NextFloat(-1.5f, 1.5f) + new Vector2(0, -0.5f);

                SerenadeParticleHandler.Spawn(new SpectralNoteParticle(
                    notePos, noteVel, 0.5f + channelProgress * 0.2f, 60 + Main.rand.Next(30)
                ));
            }

            // Refraction bloom pulses at regular intervals along beam
            if (AliveTime % 15 == 0)
            {
                int rippleCount = 3;
                for (int i = 0; i < rippleCount; i++)
                {
                    float d = MaxBeamLength * (i + 1) / (rippleCount + 1);
                    Vector2 ripplePos = Owner.MountedCenter + beamDir * d;
                    Color rippleCol = GetBeamGradient(d / MaxBeamLength);
                    SerenadeParticleHandler.Spawn(new RefractionBloomParticle(
                        ripplePos, rippleCol, 1.2f, 20
                    ));
                }
            }

            // Mist around source
            if (AliveTime % 6 == 0)
            {
                Vector2 mistPos = Owner.MountedCenter + Main.rand.NextVector2Circular(30, 30);
                Color mistCol = GetBeamGradient(Main.rand.NextFloat()) * 0.4f;
                SerenadeParticleHandler.Spawn(new SerenadeMistParticle(
                    mistPos, beamDir * 0.5f, mistCol, 0.4f + Main.rand.NextFloat(0.3f), 40
                ));
            }

            // Dust at source point
            if (AliveTime % 3 == 0)
            {
                Vector2 dustPos = Owner.MountedCenter + beamDir * 20f + Main.rand.NextVector2Circular(5, 5);
                int d = Dust.NewDust(dustPos - new Vector2(4), 8, 8,
                    ModContent.DustType<PrismaticDust>(), beamDir.X * 2f, beamDir.Y * 2f);
                Main.dust[d].scale = 1f;
                Main.dust[d].noGravity = true;
            }
        }

        private void SpawnEndBurst()
        {
            // Grand finale burst when channel ends
            for (int i = 0; i < 20; i++)
            {
                float angle = MathHelper.TwoPi * i / 20f;
                Vector2 vel = angle.ToRotationVector2() * (4f + Main.rand.NextFloat(3f));
                Color col = GetSpectralColor(i % 7);
                SerenadeParticleHandler.Spawn(new PrismaticSparkParticle(
                    Owner.MountedCenter, vel, col, MoonWhite, 0.5f, 25
                ));
            }

            SerenadeParticleHandler.Spawn(new RefractionBloomParticle(
                Owner.MountedCenter, PrismViolet, 2f, 30
            ));

            for (int i = 0; i < 5; i++)
            {
                SerenadeParticleHandler.Spawn(new SpectralNoteParticle(
                    Owner.MountedCenter + Main.rand.NextVector2Circular(20, 20),
                    Main.rand.NextVector2Circular(2, 2) + new Vector2(0, -1.5f),
                    0.6f, 80 + Main.rand.Next(30)
                ));
            }
        }

        private void EmitBeamLight()
        {
            Vector2 beamDir = BeamAngle.ToRotationVector2();
            float step = 80f;

            for (float d = 0; d < MaxBeamLength; d += step)
            {
                Vector2 lightPos = Owner.MountedCenter + beamDir * d;
                float falloff = 1f - d / MaxBeamLength;
                Color lightCol = GetBeamGradient(d / MaxBeamLength);
                Lighting.AddLight(lightPos, lightCol.ToVector3() * falloff * 0.6f);
            }
        }

        public override bool? CanDamage() => false; // Damage handled manually

        public override bool PreDraw(ref Color lightColor)
        {
            DrawMegaBeam();
            DrawSourceOrb();
            return false;
        }

        private void DrawMegaBeam()
        {
            Vector2 beamDir = BeamAngle.ToRotationVector2();
            float channelProgress = Math.Min(AliveTime / 15f, 1f); // Ramp-up over 15 ticks
            float currentLength = MaxBeamLength * channelProgress;

            // Build beam spine points
            List<Vector2> beamPoints = new();
            for (int i = 0; i < BeamPoints; i++)
            {
                float t = i / (float)(BeamPoints - 1);
                Vector2 pos = Owner.MountedCenter + beamDir * t * currentLength;

                // Slight sinusoidal wobble for organic feel
                float wobble = MathF.Sin(t * 12f + AliveTime * 0.15f) * 3f * t;
                Vector2 perp = new(-beamDir.Y, beamDir.X);
                pos += perp * wobble;

                beamPoints.Add(pos);
            }

            if (beamPoints.Count < 3) return;

            float intensity = Math.Min(AliveTime / 10f, 1f);

            // Pass 1: Wide glow underlayer
            MiscShaderData glowShader = GameShaders.Misc.TryGetValue("MagnumOpus:SerenadePrismaticGlow", out var gs) ? gs : null;
            if (glowShader != null)
            {
                glowShader.Shader?.Parameters["uColor"]?.SetValue(PrismViolet.ToVector3());
                glowShader.Shader?.Parameters["uSecondaryColor"]?.SetValue(RefractedBlue.ToVector3());
                glowShader.Shader?.Parameters["uOpacity"]?.SetValue(0.5f * intensity);
                glowShader.Shader?.Parameters["uTime"]?.SetValue(AliveTime * 0.02f);
                glowShader.Shader?.Parameters["uIntensity"]?.SetValue(1.5f);
                glowShader.Shader?.Parameters["uPhase"]?.SetValue(1f); // Full spectral spread for mega-beam
                glowShader.Shader?.Parameters["uScrollSpeed"]?.SetValue(2f);
                glowShader.Shader?.Parameters["uOverbrightMult"]?.SetValue(1.3f);
            }

            var glowSettings = new SerenadeTrailSettings(
                widthFunction: (t, _) =>
                {
                    float tip = SineOut(Math.Min(t * 5f, 1f));
                    float end = 1f - t * 0.2f;
                    return BeamWidth * 2.5f * tip * end * intensity;
                },
                colorFunction: (t, _) => GetBeamGradient(t * 0.7f) * (0.4f * (1f - t * 0.3f)),
                smoothen: false,
                shader: glowShader
            );
            SerenadeTrailRenderer.RenderTrail(beamPoints, glowSettings);

            // Pass 2: Main prismatic beam body
            MiscShaderData beamShader = GameShaders.Misc.TryGetValue("MagnumOpus:SerenadePrismaticBeam", out var bs) ? bs : null;
            if (beamShader != null)
            {
                beamShader.Shader?.Parameters["uColor"]?.SetValue(PrismViolet.ToVector3());
                beamShader.Shader?.Parameters["uSecondaryColor"]?.SetValue(RefractedBlue.ToVector3());
                beamShader.Shader?.Parameters["uOpacity"]?.SetValue(0.9f * intensity);
                beamShader.Shader?.Parameters["uTime"]?.SetValue(AliveTime * 0.02f);
                beamShader.Shader?.Parameters["uIntensity"]?.SetValue(1.5f);
                beamShader.Shader?.Parameters["uPhase"]?.SetValue(1f);
                beamShader.Shader?.Parameters["uScrollSpeed"]?.SetValue(4f);
                beamShader.Shader?.Parameters["uDistortionAmt"]?.SetValue(0.08f);
                beamShader.Shader?.Parameters["uOverbrightMult"]?.SetValue(1.8f);
                beamShader.Shader?.Parameters["uHasSecondaryTex"]?.SetValue(0f);
                beamShader.Shader?.Parameters["uSecondaryTexScale"]?.SetValue(1f);
                beamShader.Shader?.Parameters["uSecondaryTexScroll"]?.SetValue(1f);
            }

            var beamSettings = new SerenadeTrailSettings(
                widthFunction: (t, _) =>
                {
                    float tip = SineOut(Math.Min(t * 5f, 1f));
                    float end = 1f - t * 0.15f;
                    return BeamWidth * 1.5f * tip * end * intensity;
                },
                colorFunction: (t, _) => GetBeamGradient(t) * (1f - t * 0.2f),
                smoothen: false,
                shader: beamShader
            );
            SerenadeTrailRenderer.RenderTrail(beamPoints, beamSettings);

            // Pass 3: White hot core
            var coreSettings = new SerenadeTrailSettings(
                widthFunction: (t, _) =>
                {
                    float tip = SineOut(Math.Min(t * 5f, 1f));
                    return BeamWidth * 0.4f * tip * (1f - t * 0.5f) * intensity;
                },
                colorFunction: (t, _) => MoonWhite * (0.7f * (1f - t * 0.5f) * intensity),
                smoothen: false,
                shader: glowShader // Reuse glow shader for core
            );
            SerenadeTrailRenderer.RenderTrail(beamPoints, coreSettings);
        }

        private void DrawSourceOrb()
        {
            var tex = SerenadeTextures.SoftRadialBloom;
            if (tex == null) return;

            float intensity = Math.Min(AliveTime / 10f, 1f);
            float pulse = 1f + MathF.Sin(AliveTime * 0.2f) * 0.15f;
            Vector2 drawPos = Owner.MountedCenter - Main.screenPosition;
            var origin = tex.Size() * 0.5f;

            // Outer glow
            Color outerCol = PrismViolet * (0.5f * intensity * pulse);
            Main.spriteBatch.Draw(tex, drawPos, null, outerCol, 0f, origin, 1.5f * intensity, SpriteEffects.None, 0f);

            // Mid glow
            Color midCol = RefractedBlue * (0.4f * intensity * pulse);
            Main.spriteBatch.Draw(tex, drawPos, null, midCol, 0f, origin, 0.8f * intensity, SpriteEffects.None, 0f);

            // Core
            Color coreCol = MoonWhite * (0.7f * intensity * pulse);
            Main.spriteBatch.Draw(tex, drawPos, null, coreCol, 0f, origin, 0.3f * intensity, SpriteEffects.None, 0f);
        }
    }
}
