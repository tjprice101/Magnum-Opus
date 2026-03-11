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
    /// RESONANCE SYSTEM:
    /// - Beam builds through 5 resonance stages as channeling continues
    ///   Pianissimo (0) → Piano (1) → Mezzo-Forte (2) → Forte (3) → Fortissimo (4)
    /// - Each stage increases beam width, particle density, shader intensity
    /// - At Mezzo-Forte (2+): Harmonic nodes appear along beam as standing wave
    ///   positions — enemies at nodes take 1.5x damage
    /// - At Forte (3+): Music note floods + enhanced bloom
    /// - At Fortissimo (4+): Maximum power with resonance pulse ring
    /// 
    /// ai[0] = alive timer
    /// ai[1] = beam direction angle
    /// </summary>
    public class SerenadeHoldout : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_1";

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

        /// <summary>Cached previous resonance level for detecting transitions.</summary>
        private int _prevResonanceLevel = 0;

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

            // --- Resonance building ---
            var serenade = Owner.Serenade();
            serenade.TickChannel();
            int currentResonance = serenade.ResonanceLevel;

            // Detect resonance level transitions and spawn burst
            if (currentResonance > _prevResonanceLevel && _prevResonanceLevel >= 0 && !Main.dedServ)
            {
                SpawnResonanceTransitionVFX(currentResonance);
            }
            _prevResonanceLevel = currentResonance;

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
            var serenade = Owner.Serenade();

            float step = 32f;
            for (float dist = 0; dist < MaxBeamLength; dist += step)
            {
                Vector2 checkPos = beamStart + beamDir * dist;
                Rectangle checkRect = new((int)checkPos.X - 20, (int)checkPos.Y - 20, 40, 40);
                float beamProgress = dist / MaxBeamLength;

                // Check if this position is at a harmonic node
                bool atNode = serenade.IsAtHarmonicNode(beamProgress);
                float nodeMult = atNode ? SerenadePlayer.HarmonicNodeDamageMultiplier : 1f;

                foreach (NPC npc in Main.ActiveNPCs)
                {
                    if (!npc.active || npc.dontTakeDamage || npc.friendly || npc.immortal) continue;
                    if (!npc.Hitbox.Intersects(checkRect)) continue;

                    // Apply local iframes
                    if (Projectile.localNPCImmunity[npc.whoAmI] > 0) continue;
                    Projectile.localNPCImmunity[npc.whoAmI] = Projectile.localNPCHitCooldown;

                    // Hit! Harmonic nodes deal 1.5x damage
                    var hitInfo = npc.CalculateHitInfo((int)(Projectile.damage * 1.8f * nodeMult), Owner.direction,
                        false, Projectile.knockBack, Projectile.DamageType, true);

                    npc.StrikeNPC(hitInfo);
                    npc.AddBuff(ModContent.BuffType<MusicalDissonance>(), 300);

                    // Per-hit VFX — enhanced at harmonic nodes
                    if (!Main.dedServ)
                    {
                        float bloomScale = atNode ? 1.8f : 1f;
                        int sparkCount = atNode ? 6 : 3;

                        SerenadeParticleHandler.Spawn(new RefractionBloomParticle(
                            npc.Center, GetSpectralColor(Main.rand.Next(7)), bloomScale, 15
                        ));
                        for (int i = 0; i < sparkCount; i++)
                        {
                            Vector2 vel = Main.rand.NextVector2CircularEdge(3f, 3f);
                            SerenadeParticleHandler.Spawn(new PrismaticSparkParticle(
                                npc.Center, vel, GetSpectralColor(Main.rand.Next(7)), MoonWhite, 0.4f, 18
                            ));
                        }

                        // Extra music note burst at harmonic nodes
                        if (atNode)
                        {
                            MoonlightVFXLibrary.SpawnMusicNotes(npc.Center, count: 3,
                                spread: 12f, minScale: 0.4f, maxScale: 0.7f, lifetime: 30);
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
            var serenade = Owner.Serenade();
            int resonance = serenade.ResonanceLevel;
            float resMult = SerenadePlayer.ResonanceIntensity[resonance];

            // Prismatic sparks along the beam — density scales with resonance
            int sparkInterval = Math.Max(1, 3 - resonance);
            if (AliveTime % sparkInterval == 0)
            {
                float dist = Main.rand.NextFloat(MaxBeamLength * 0.8f);
                Vector2 sparkPos = Owner.MountedCenter + beamDir * dist;
                Vector2 perpVel = beamDir.RotateBy(MathHelper.PiOver2) * Main.rand.NextFloat(-2f, 2f);

                Color col = GetSpectralColor(Main.rand.Next(7));
                SerenadeParticleHandler.Spawn(new PrismaticSparkParticle(
                    sparkPos + Main.rand.NextVector2Circular(BeamWidth * 0.3f, BeamWidth * 0.3f),
                    perpVel, col, MoonWhite, (0.35f + resonance * 0.08f), 20
                ));
            }

            // Spectral notes floating off beam — more frequent at higher resonance
            int noteInterval = Math.Max(3, 10 - resonance * 2);
            if (AliveTime % noteInterval == 0)
            {
                float dist = Main.rand.NextFloat(MaxBeamLength * 0.6f);
                Vector2 notePos = Owner.MountedCenter + beamDir * dist;
                Vector2 perpDir = beamDir.RotateBy(MathHelper.PiOver2);
                Vector2 noteVel = perpDir * Main.rand.NextFloat(-1.5f, 1.5f) + new Vector2(0, -0.5f);

                SerenadeParticleHandler.Spawn(new SpectralNoteParticle(
                    notePos, noteVel, 0.5f + channelProgress * 0.2f + resonance * 0.05f, 60 + Main.rand.Next(30)
                ));
            }

            // Refraction bloom pulses at regular intervals along beam — count scales
            int bloomInterval = Math.Max(8, 15 - resonance * 2);
            if (AliveTime % bloomInterval == 0)
            {
                int rippleCount = 3 + resonance;
                for (int i = 0; i < rippleCount; i++)
                {
                    float d = MaxBeamLength * (i + 1) / (rippleCount + 1);
                    Vector2 ripplePos = Owner.MountedCenter + beamDir * d;
                    Color rippleCol = GetBeamGradient(d / MaxBeamLength);
                    SerenadeParticleHandler.Spawn(new RefractionBloomParticle(
                        ripplePos, rippleCol, 1.2f + resonance * 0.2f, 20
                    ));
                }
            }

            // Harmonic node particles — only at resonance 2+ (Mezzo-Forte)
            if (resonance >= 2 && AliveTime % 6 == 0)
            {
                int nodeCount = serenade.HarmonicNodeCount;
                for (int i = 1; i <= nodeCount; i++)
                {
                    float nodeProgress = i / (float)(nodeCount + 1);
                    Vector2 nodePos = Owner.MountedCenter + beamDir * (MaxBeamLength * nodeProgress * channelProgress);
                    Color nodeBase = SerenadePlayer.ResonanceColors[resonance];
                    Color nodePeak = MoonWhite;

                    SerenadeParticleHandler.Spawn(new HarmonicNodeParticle(
                        nodePos, nodeBase, nodePeak,
                        0.5f + resonance * 0.15f, 12
                    ));
                }
            }

            // Forte (3+): Music note flood from beam — cascading harmonic notes
            if (resonance >= 3 && AliveTime % 5 == 0)
            {
                MoonlightVFXLibrary.SpawnMusicNotes(
                    Owner.MountedCenter + beamDir * Main.rand.NextFloat(MaxBeamLength * 0.5f),
                    count: 2, spread: 10f, minScale: 0.3f, maxScale: 0.6f, lifetime: 40);
            }

            // Mist around source — resonance enhances density
            int mistInterval = Math.Max(3, 6 - resonance);
            if (AliveTime % mistInterval == 0)
            {
                Vector2 mistPos = Owner.MountedCenter + Main.rand.NextVector2Circular(30, 30);
                Color mistCol = GetBeamGradient(Main.rand.NextFloat()) * (0.4f + resonance * 0.08f);
                SerenadeParticleHandler.Spawn(new SerenadeMistParticle(
                    mistPos, beamDir * 0.5f, mistCol, 0.4f + Main.rand.NextFloat(0.3f) + resonance * 0.05f, 40
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
            var serenade = Owner.Serenade();
            int resonance = serenade.ResonanceLevel;
            float resMult = SerenadePlayer.ResonanceIntensity[resonance];

            // === FOUNDATION VFX: MoonlightPuddle (ImpactFoundation DamageZoneShader) ===
            // Persistent prismatic damage zone at the beam origin on channel end.
            if (Projectile.owner == Main.myPlayer)
            {
                Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    Owner.MountedCenter, Vector2.Zero,
                    ModContent.ProjectileType<MoonlightPuddle>(),
                    0, 0f, Projectile.owner,
                    ai0: resonance
                );
            }

            // Grand finale burst — spark count scales with resonance
            int sparkCount = 20 + resonance * 6;
            for (int i = 0; i < sparkCount; i++)
            {
                float angle = MathHelper.TwoPi * i / sparkCount;
                Vector2 vel = angle.ToRotationVector2() * (4f + Main.rand.NextFloat(3f) + resonance * 0.5f);
                Color col = GetSpectralColor(i % 7);
                SerenadeParticleHandler.Spawn(new PrismaticSparkParticle(
                    Owner.MountedCenter, vel, col, MoonWhite, 0.5f + resonance * 0.1f, 25
                ));
            }

            // Central bloom — larger at higher resonance
            SerenadeParticleHandler.Spawn(new RefractionBloomParticle(
                Owner.MountedCenter, PrismViolet, 2f + resonance * 0.5f, 30
            ));

            // Resonance pulse ring on end
            if (resonance >= 2)
            {
                SerenadeParticleHandler.Spawn(new ResonancePulseParticle(
                    Owner.MountedCenter, SerenadePlayer.ResonanceColors[resonance],
                    3f + resonance * 0.8f, 30
                ));
            }

            // Floating spectral notes — count scales
            int noteCount = 5 + resonance * 2;
            for (int i = 0; i < noteCount; i++)
            {
                SerenadeParticleHandler.Spawn(new SpectralNoteParticle(
                    Owner.MountedCenter + Main.rand.NextVector2Circular(20, 20),
                    Main.rand.NextVector2Circular(2, 2) + new Vector2(0, -1.5f),
                    0.6f + resonance * 0.08f, 80 + Main.rand.Next(30)
                ));
            }

            // Music note burst via library — resonance scales count
            MoonlightVFXLibrary.SpawnMusicNotes(Owner.MountedCenter,
                count: 3 + resonance * 2, spread: 20f + resonance * 5f,
                minScale: 0.4f, maxScale: 0.8f, lifetime: 45);
        }

        /// <summary>Spawn VFX burst when resonance level increases (stage transition).</summary>
        private void SpawnResonanceTransitionVFX(int newLevel)
        {
            // Expanding resonance ring
            Color resColor = SerenadePlayer.ResonanceColors[newLevel];
            SerenadeParticleHandler.Spawn(new ResonancePulseParticle(
                Owner.MountedCenter, resColor, 2f + newLevel * 0.5f, 25
            ));

            // Bright bloom flash at source
            SerenadeParticleHandler.Spawn(new RefractionBloomParticle(
                Owner.MountedCenter, resColor, 1.5f + newLevel * 0.3f, 18
            ));

            // Music notes cascading outward — more at higher levels
            int noteCount = 2 + newLevel;
            for (int i = 0; i < noteCount; i++)
            {
                float angle = MathHelper.TwoPi * i / noteCount;
                Vector2 noteVel = angle.ToRotationVector2() * (1.5f + newLevel * 0.3f);
                SerenadeParticleHandler.Spawn(new SpectralNoteParticle(
                    Owner.MountedCenter, noteVel, 0.5f + newLevel * 0.1f, 50 + Main.rand.Next(20)
                ));
            }

            // Sound cue for resonance transition
            SoundEngine.PlaySound(SoundID.Item29 with { Pitch = 0.2f + newLevel * 0.15f, Volume = 0.5f + newLevel * 0.1f },
                Owner.MountedCenter);
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
            SpriteBatch sb = Main.spriteBatch;
            try
            {
            DrawMegaBeam();
            DrawSourceOrb();
            }
            catch { }
            finally
            {
                try { sb.End(); } catch { }
                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                    DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            }

            return false;
        }

        private void DrawMegaBeam()
        {
            Vector2 beamDir = BeamAngle.ToRotationVector2();
            float channelProgress = Math.Min(AliveTime / 15f, 1f); // Ramp-up over 15 ticks
            float currentLength = MaxBeamLength * channelProgress;
            var serenade = Owner.Serenade();
            int resonance = serenade.ResonanceLevel;
            float widthMult = SerenadePlayer.ResonanceWidthMultiplier[resonance];
            float resMult = SerenadePlayer.ResonanceIntensity[resonance];

            // Build beam spine points
            List<Vector2> beamPoints = new();
            for (int i = 0; i < BeamPoints; i++)
            {
                float t = i / (float)(BeamPoints - 1);
                Vector2 pos = Owner.MountedCenter + beamDir * t * currentLength;

                // Sinusoidal wobble — less at higher resonance (beam becomes more focused)
                float wobbleAmp = (4f - resonance * 0.5f) * t;
                float wobble = MathF.Sin(t * 12f + AliveTime * 0.15f) * wobbleAmp;

                // Standing wave overlay at Mezzo-Forte+ — harmonic node breathing
                if (resonance >= 2 && serenade.HarmonicNodeCount > 0)
                {
                    float nodeWave = MathF.Sin(t * MathHelper.Pi * (serenade.HarmonicNodeCount + 1));
                    wobble += nodeWave * 2f * MathF.Sin(AliveTime * 0.1f);
                }

                Vector2 perp = new(-beamDir.Y, beamDir.X);
                pos += perp * wobble;

                beamPoints.Add(pos);
            }

            if (beamPoints.Count < 3) return;

            float intensity = Math.Min(AliveTime / 10f, 1f) * resMult;

            // Pass 1: Wide glow underlayer — wider at higher resonance
            MiscShaderData glowShader = GameShaders.Misc.TryGetValue("MagnumOpus:SerenadePrismaticGlow", out var gs) ? gs : null;
            if (glowShader != null)
            {
                Color resColor = SerenadePlayer.ResonanceColors[resonance];
                glowShader.Shader?.Parameters["uColor"]?.SetValue(Color.Lerp(PrismViolet, resColor, 0.3f).ToVector3());
                glowShader.Shader?.Parameters["uSecondaryColor"]?.SetValue(RefractedBlue.ToVector3());
                glowShader.Shader?.Parameters["uOpacity"]?.SetValue(0.5f * intensity);
                glowShader.Shader?.Parameters["uTime"]?.SetValue(AliveTime * 0.02f);
                glowShader.Shader?.Parameters["uIntensity"]?.SetValue(1.5f + resonance * 0.3f);
                glowShader.Shader?.Parameters["uPhase"]?.SetValue(1f);
                glowShader.Shader?.Parameters["uScrollSpeed"]?.SetValue(2f + resonance * 0.5f);
                glowShader.Shader?.Parameters["uOverbrightMult"]?.SetValue(1.3f + resonance * 0.15f);
            }

            var glowSettings = new SerenadeTrailSettings(
                widthFunction: (t, _) =>
                {
                    float tip = SineOut(Math.Min(t * 5f, 1f));
                    float end = 1f - t * 0.2f;
                    return BeamWidth * 2.5f * widthMult * tip * end * intensity;
                },
                colorFunction: (t, _) => GetBeamGradient(t * 0.7f) * (0.4f * (1f - t * 0.3f)),
                smoothen: false,
                shader: glowShader
            );
            SerenadeTrailRenderer.RenderTrail(beamPoints, glowSettings);

            // Pass 2: Main prismatic beam body — resonance scales intensity
            MiscShaderData beamShader = GameShaders.Misc.TryGetValue("MagnumOpus:SerenadePrismaticBeam", out var bs) ? bs : null;
            if (beamShader != null)
            {
                Color resColor = SerenadePlayer.ResonanceColors[resonance];
                beamShader.Shader?.Parameters["uColor"]?.SetValue(Color.Lerp(PrismViolet, resColor, 0.4f).ToVector3());
                beamShader.Shader?.Parameters["uSecondaryColor"]?.SetValue(RefractedBlue.ToVector3());
                beamShader.Shader?.Parameters["uOpacity"]?.SetValue((0.9f + resonance * 0.05f) * intensity);
                beamShader.Shader?.Parameters["uTime"]?.SetValue(AliveTime * 0.02f);
                beamShader.Shader?.Parameters["uIntensity"]?.SetValue(1.5f + resonance * 0.3f);
                beamShader.Shader?.Parameters["uPhase"]?.SetValue(1f);
                beamShader.Shader?.Parameters["uScrollSpeed"]?.SetValue(4f + resonance * 0.5f);
                beamShader.Shader?.Parameters["uDistortionAmt"]?.SetValue(0.08f + resonance * 0.02f);
                beamShader.Shader?.Parameters["uOverbrightMult"]?.SetValue(1.8f + resonance * 0.2f);
                beamShader.Shader?.Parameters["uHasSecondaryTex"]?.SetValue(0f);
                beamShader.Shader?.Parameters["uSecondaryTexScale"]?.SetValue(1f);
                beamShader.Shader?.Parameters["uSecondaryTexScroll"]?.SetValue(1f);
            }

            var beamSettings = new SerenadeTrailSettings(
                widthFunction: (t, _) =>
                {
                    float tip = SineOut(Math.Min(t * 5f, 1f));
                    float end = 1f - t * 0.15f;
                    return BeamWidth * 1.5f * widthMult * tip * end * intensity;
                },
                colorFunction: (t, _) => GetBeamGradient(t) * (1f - t * 0.2f),
                smoothen: false,
                shader: beamShader
            );
            SerenadeTrailRenderer.RenderTrail(beamPoints, beamSettings);

            // Pass 3: White hot core — resonance makes it broader
            var coreSettings = new SerenadeTrailSettings(
                widthFunction: (t, _) =>
                {
                    float tip = SineOut(Math.Min(t * 5f, 1f));
                    return BeamWidth * (0.4f + resonance * 0.06f) * tip * (1f - t * 0.5f) * intensity;
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

            var serenade = Owner.Serenade();
            int resonance = serenade.ResonanceLevel;
            float resMult = SerenadePlayer.ResonanceIntensity[resonance];

            float intensity = Math.Min(AliveTime / 10f, 1f) * resMult;
            float pulse = 1f + MathF.Sin(AliveTime * 0.2f) * 0.15f;
            Vector2 drawPos = Owner.MountedCenter - Main.screenPosition;
            var origin = tex.Size() * 0.5f;
            Color resColor = SerenadePlayer.ResonanceColors[resonance];

            // Switch to Additive for bloom glow layers
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive,
                Main.DefaultSamplerState, DepthStencilState.None,
                Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

            // Outer glow — shifts color with resonance
            Color outerCol = Color.Lerp(PrismViolet, resColor, 0.4f) with { A = 0 };
            Main.spriteBatch.Draw(tex, drawPos, null, outerCol * (0.5f * intensity * pulse), 0f, origin, (0.1f + resonance * 0.015f) * intensity, SpriteEffects.None, 0f);

            // Mid glow
            Color midCol = Color.Lerp(RefractedBlue, resColor, 0.3f) with { A = 0 };
            Main.spriteBatch.Draw(tex, drawPos, null, midCol * (0.4f * intensity * pulse), 0f, origin, (0.055f + resonance * 0.008f) * intensity, SpriteEffects.None, 0f);

            // Core
            Color coreCol = MoonWhite with { A = 0 };
            Main.spriteBatch.Draw(tex, drawPos, null, coreCol * (0.7f * intensity * pulse), 0f, origin, 0.02f * intensity, SpriteEffects.None, 0f);

            // Restore to AlphaBlend
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                Main.DefaultSamplerState, DepthStencilState.None,
                Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        }
    }
}
