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
using MagnumOpus.Content.MoonlightSonata;
using MagnumOpus.Content.MoonlightSonata.Debuffs;
using MagnumOpus.Content.MoonlightSonata.Dusts;
using MagnumOpus.Content.MoonlightSonata.Weapons.IncisorOfMoonlight;

namespace MagnumOpus.Content.MoonlightSonata.Projectiles
{
    /// <summary>
    /// Constellation lock marker projectile for Incisor of Moonlight — "The Stellar Scalpel".
    /// Homes to the nearest NPC, attaches to it with a rotating constellation sigil overlay,
    /// then detonates in a harmonic burst after a short duration.
    ///
    /// Phase 1 — Seeking (30 frames): Aggressive homing with constellation trail lines.
    /// Phase 2 — Lock-on (90 frames): Attached to NPC center, rotating 6-pointed sigil expands,
    ///           StarPointDust sparkles, MusicsDissonance debuff applied.
    /// Phase 3 — Detonation (OnKill): Starburst of custom dusts, flare cascade, screen shake.
    ///
    /// ai[0]: -1 = seeking phase, NPC.whoAmI = locked onto that NPC.
    /// ai[1]: internal timer for lock-on phase progression.
    /// </summary>
    public class IncisorConstellationLock : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_0";

        private float LockedTarget
        {
            get => Projectile.ai[0];
            set => Projectile.ai[0] = value;
        }

        private float LockTimer
        {
            get => Projectile.ai[1];
            set => Projectile.ai[1] = value;
        }

        private bool IsLocked => LockedTarget >= 0f;

        // Constellation trail storage for seeking phase
        private Vector2[] _trailPoints = new Vector2[10];
        private int _trailWriteIndex;
        private int _trailTimer;

        // Seeking phase timer for homing ramp
        private int _seekTimer;

        // Lock-on phase sigil expansion
        private float _sigilRadius;

        private const int SeekDuration = 30;
        private const int LockDuration = 90;
        private const float SeekSpeed = 14f;
        private const float HomingMax = 0.12f;
        private const float SeekRange = 600f;

        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 120;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.alpha = 255;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 30;

            // Start in seeking mode
            LockedTarget = -1f;
            LockTimer = 0f;
        }

        public override bool ShouldUpdatePosition()
        {
            return !IsLocked;
        }

        public override void AI()
        {
            if (!IsLocked)
                SeekingPhaseAI();
            else
                LockOnPhaseAI();
        }

        private void SeekingPhaseAI()
        {
            _seekTimer++;

            // Ramp homing intensity from 0 to HomingMax over SeekDuration
            float homingT = MathHelper.Clamp((float)_seekTimer / SeekDuration, 0f, 1f);
            float homingIntensity = homingT * HomingMax;

            // Find nearest NPC
            NPC closestNPC = null;
            float closestDist = SeekRange;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && !npc.friendly && npc.CanBeChasedBy())
                {
                    float dist = Vector2.Distance(Projectile.Center, npc.Center);
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        closestNPC = npc;
                    }
                }
            }

            // Home toward target
            if (closestNPC != null)
            {
                Vector2 toTarget = (closestNPC.Center - Projectile.Center).SafeNormalize(Vector2.UnitX);
                float currentSpeed = Projectile.velocity.Length();
                if (currentSpeed < SeekSpeed)
                    currentSpeed = SeekSpeed;
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, toTarget * currentSpeed, homingIntensity);

                // Check if close enough to lock on
                if (closestDist < 24f)
                {
                    LockedTarget = closestNPC.whoAmI;
                    LockTimer = 0f;
                    Projectile.Center = closestNPC.Center;
                    Projectile.velocity = Vector2.Zero;
                    Projectile.timeLeft = LockDuration;
                    Projectile.netUpdate = true;

                    // Apply debuff on lock
                    closestNPC.AddBuff(ModContent.BuffType<MusicsDissonance>(), 120);

                    // Lock-on flash
                    CustomParticles.GenericFlare(closestNPC.Center,
                        IncisorOfMoonlightVFX.ConstellationBlue, 0.4f, 15);

                    // Star burst on lock
                    for (int s = 0; s < 6; s++)
                    {
                        float angle = MathHelper.TwoPi * s / 6f;
                        Vector2 vel = angle.ToRotationVector2() * 2.5f;
                        Color starColor = Color.Lerp(IncisorOfMoonlightVFX.ConstellationBlue,
                            IncisorOfMoonlightVFX.HarmonicWhite, (float)s / 6f);
                        Dust star = Dust.NewDustPerfect(closestNPC.Center,
                            ModContent.DustType<StarPointDust>(),
                            vel, 0, starColor, 0.5f);
                        star.customData = new StarPointBehavior(0.15f, 20);
                    }

                    return;
                }
            }

            // Record trail points every 3 frames for constellation line drawing
            _trailTimer++;
            if (_trailTimer >= 3)
            {
                _trailTimer = 0;
                _trailPoints[_trailWriteIndex % _trailPoints.Length] = Projectile.Center;
                _trailWriteIndex++;
            }

            // StarPointDust trail sparkle
            if (Main.rand.NextBool(3))
            {
                Color sparkColor = Color.Lerp(IncisorOfMoonlightVFX.ConstellationBlue,
                    IncisorOfMoonlightVFX.ResonantSilver, Main.rand.NextFloat());
                Dust star = Dust.NewDustPerfect(
                    Projectile.Center + Main.rand.NextVector2Circular(4f, 4f),
                    ModContent.DustType<StarPointDust>(),
                    -Projectile.velocity * 0.04f,
                    0, sparkColor, 0.6f);
                star.customData = new StarPointBehavior(0.12f, 25);
            }

            // Kill if seeking phase expires without lock
            if (_seekTimer >= SeekDuration)
                Projectile.Kill();

            // Dynamic lighting
            float pulse = 0.4f + MathF.Sin(Main.GlobalTimeWrappedHourly * 8f) * 0.15f;
            Lighting.AddLight(Projectile.Center, IncisorOfMoonlightVFX.ConstellationBlue.ToVector3() * pulse);
        }

        private void LockOnPhaseAI()
        {
            LockTimer++;

            int targetIdx = (int)LockedTarget;
            if (targetIdx < 0 || targetIdx >= Main.maxNPCs || !Main.npc[targetIdx].active)
            {
                Projectile.Kill();
                return;
            }

            NPC target = Main.npc[targetIdx];

            // Stick to target center
            Projectile.Center = target.Center;

            // Expand sigil radius over the lock duration
            float lockProgress = LockTimer / LockDuration;
            float targetRadius = 40f + lockProgress * 30f;
            _sigilRadius = MathHelper.Lerp(_sigilRadius, targetRadius, 0.08f);

            // StarPointDust sparkles around the sigil perimeter
            if ((int)LockTimer % 5 == 0)
            {
                float sparkAngle = Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 sparkPos = target.Center + sparkAngle.ToRotationVector2() * _sigilRadius;
                Color sparkColor = IncisorOfMoonlightVFX.GetResonanceColor(Main.rand.NextFloat(), 2);
                Dust star = Dust.NewDustPerfect(sparkPos,
                    ModContent.DustType<StarPointDust>(),
                    sparkAngle.ToRotationVector2() * 0.5f,
                    0, sparkColor, 0.45f);
                star.customData = new StarPointBehavior
                {
                    RotationSpeed = 0.18f,
                    Lifetime = 20,
                    FadeStartTime = 6
                };
            }

            // LunarMote orbiting the locked target
            if ((int)LockTimer % 12 == 0)
            {
                float moteAngle = LockTimer * 0.15f;
                for (int i = 0; i < 2; i++)
                {
                    float angle = moteAngle + MathHelper.Pi * i;
                    Color moteColor = Color.Lerp(IncisorOfMoonlightVFX.DeepResonance,
                        IncisorOfMoonlightVFX.ConstellationBlue, (float)i / 2f);
                    Dust mote = Dust.NewDustPerfect(target.Center + angle.ToRotationVector2() * _sigilRadius * 0.5f,
                        ModContent.DustType<LunarMote>(),
                        Vector2.Zero, 0, moteColor, 0.35f);
                    mote.customData = new LunarMoteBehavior(target.Center, angle)
                    {
                        OrbitRadius = _sigilRadius * 0.5f,
                        OrbitSpeed = 0.07f,
                        Lifetime = 25,
                        FadePower = 0.92f
                    };
                }
            }

            // Pulsing intensity ramps up as detonation approaches
            float intensityRamp = 0.5f + lockProgress * 0.5f;
            float pulse = intensityRamp + MathF.Sin(Main.GlobalTimeWrappedHourly * 6f + LockTimer * 0.1f) * 0.15f;
            Lighting.AddLight(target.Center, IncisorOfMoonlightVFX.ConstellationBlue.ToVector3() * pulse);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<MusicsDissonance>(), 120);

            // Resonant pulse ring on hit
            Dust pulse = Dust.NewDustPerfect(target.Center,
                ModContent.DustType<ResonantPulseDust>(),
                Vector2.Zero, 0,
                IncisorOfMoonlightVFX.FrequencyPulse, 0.5f);
            pulse.customData = new ResonantPulseBehavior(0.04f, 18);

            CustomParticles.GenericFlare(target.Center,
                IncisorOfMoonlightVFX.ResonantSilver, 0.3f, 12);
        }

        public override void OnKill(int timeLeft)
        {
            Vector2 deathPos = Projectile.Center;

            // === STAR POINT DUST BURST — 8 directional stars ===
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 6f);
                Color starColor = Color.Lerp(IncisorOfMoonlightVFX.ConstellationBlue,
                    IncisorOfMoonlightVFX.HarmonicWhite, (float)i / 8f);
                Dust star = Dust.NewDustPerfect(deathPos,
                    ModContent.DustType<StarPointDust>(),
                    vel, 0, starColor, 0.65f);
                star.customData = new StarPointBehavior(0.2f, 28);
            }

            // === RESONANT PULSE DUST — 3 expanding rings ===
            for (int i = 0; i < 3; i++)
            {
                Color ringColor = Color.Lerp(IncisorOfMoonlightVFX.DeepResonance,
                    IncisorOfMoonlightVFX.FrequencyPulse, (float)i / 3f);
                Dust ring = Dust.NewDustPerfect(deathPos,
                    ModContent.DustType<ResonantPulseDust>(),
                    Vector2.Zero, 0, ringColor,
                    0.35f + i * 0.1f);
                ring.customData = new ResonantPulseBehavior(0.04f + i * 0.015f, 20 + i * 5);
            }

            // === GENERIC FLARE CASCADE — DeepResonance → FrequencyPulse → HarmonicWhite ===
            CustomParticles.GenericFlare(deathPos, IncisorOfMoonlightVFX.DeepResonance, 0.7f, 22);
            CustomParticles.GenericFlare(deathPos, IncisorOfMoonlightVFX.FrequencyPulse, 0.5f, 18);
            CustomParticles.GenericFlare(deathPos, IncisorOfMoonlightVFX.HarmonicWhite, 0.35f, 15);

            // === HALO RING on detonation ===
            CustomParticles.HaloRing(deathPos, IncisorOfMoonlightVFX.ConstellationBlue, 0.4f, 16);

            // === SCREEN SHAKE ===
            MagnumScreenEffects.AddScreenShake(3f);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;

            if (!IsLocked)
                DrawSeekingPhase(sb);
            else
                DrawLockOnPhase(sb);

            return false;
        }

        private void DrawSeekingPhase(SpriteBatch sb)
        {
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            // === CONSTELLATION TRAIL LINES ===
            int maxPoints = Math.Min(_trailWriteIndex, _trailPoints.Length);
            if (maxPoints > 1)
            {
                var glowTex = MagnumTextureRegistry.GetSoftGlow();
                if (glowTex != null)
                {
                    Vector2 glowOrigin = glowTex.Size() * 0.5f;

                    for (int i = 1; i < maxPoints; i++)
                    {
                        int idx = (_trailWriteIndex - maxPoints + i) % _trailPoints.Length;
                        int prevIdx = (_trailWriteIndex - maxPoints + i - 1) % _trailPoints.Length;
                        Vector2 start = _trailPoints[prevIdx] - Main.screenPosition;
                        Vector2 end = _trailPoints[idx] - Main.screenPosition;

                        if (start == Vector2.Zero || end == Vector2.Zero) continue;

                        float lineDist = Vector2.Distance(start, end);
                        if (lineDist < 2f || lineDist > 300f) continue;

                        float lineAngle = (end - start).ToRotation();
                        float lineAlpha = 0.18f * ((float)i / maxPoints);
                        Vector2 lineCenter = (start + end) / 2f;

                        // Constellation connecting line
                        sb.Draw(glowTex, lineCenter, null,
                            (IncisorOfMoonlightVFX.ConstellationBlue with { A = 0 }) * lineAlpha,
                            lineAngle, glowOrigin,
                            new Vector2(lineDist / glowTex.Width, 0.015f),
                            SpriteEffects.None, 0f);

                        // Star node at each recorded point
                        float twinkle = 1f + MathF.Sin(Main.GlobalTimeWrappedHourly * 8f + i * 1.5f) * 0.2f;
                        float nodeScale = 0.06f * twinkle;
                        Color nodeColor = Color.Lerp(IncisorOfMoonlightVFX.ConstellationBlue,
                            IncisorOfMoonlightVFX.HarmonicWhite, (float)i / maxPoints);

                        sb.Draw(glowTex, end, null,
                            (nodeColor with { A = 0 }) * lineAlpha * 1.5f,
                            0f, glowOrigin, nodeScale, SpriteEffects.None, 0f);
                    }

                    // Star node accent at recorded constellation points
                    var starTex = MoonlightSonataTextures.Star4Point?.Value;
                    if (starTex != null)
                    {
                        Vector2 starOrigin = starTex.Size() * 0.5f;
                        for (int i = 0; i < maxPoints; i++)
                        {
                            int idx = (_trailWriteIndex - maxPoints + i) % _trailPoints.Length;
                            Vector2 nodeScreen = _trailPoints[idx] - Main.screenPosition;
                            if (nodeScreen == Vector2.Zero) continue;

                            float age = 1f - (float)i / maxPoints;
                            float starAlpha = age * 0.35f;
                            float starRot = Main.GlobalTimeWrappedHourly * 2f + i * 1.3f;

                            sb.Draw(starTex, nodeScreen, null,
                                (IncisorOfMoonlightVFX.ConstellationBlue with { A = 0 }) * starAlpha,
                                starRot, starOrigin, 0.04f + age * 0.02f,
                                SpriteEffects.None, 0f);
                        }
                    }
                }
            }

            // === 3-LAYER BLOOM BODY ===
            var bloomTex = MagnumTextureRegistry.GetSoftGlow();
            if (bloomTex != null)
            {
                Vector2 bloomOrigin = bloomTex.Size() * 0.5f;
                float pulse = 1f + MathF.Sin(Main.GlobalTimeWrappedHourly * 10f) * 0.15f;
                float bloomScale = 0.12f * pulse;

                // Layer 1: DeepResonance outer glow
                sb.Draw(bloomTex, drawPos, null,
                    (IncisorOfMoonlightVFX.DeepResonance with { A = 0 }) * 0.3f,
                    0f, bloomOrigin, bloomScale * 2.2f, SpriteEffects.None, 0f);

                // Layer 2: ConstellationBlue mid glow
                sb.Draw(bloomTex, drawPos, null,
                    (IncisorOfMoonlightVFX.ConstellationBlue with { A = 0 }) * 0.55f,
                    0f, bloomOrigin, bloomScale * 1.3f, SpriteEffects.None, 0f);

                // Layer 3: White core
                sb.Draw(bloomTex, drawPos, null,
                    (Color.White with { A = 0 }) * 0.7f,
                    0f, bloomOrigin, bloomScale * 0.4f, SpriteEffects.None, 0f);
            }
        }

        private void DrawLockOnPhase(SpriteBatch sb)
        {
            int targetIdx = (int)LockedTarget;
            if (targetIdx < 0 || targetIdx >= Main.maxNPCs || !Main.npc[targetIdx].active) return;

            NPC target = Main.npc[targetIdx];
            Vector2 drawPos = target.Center - Main.screenPosition;

            float lockProgress = MathHelper.Clamp(LockTimer / LockDuration, 0f, 1f);
            float radius = _sigilRadius;
            float time = Main.GlobalTimeWrappedHourly;

            // Overall opacity: fade in during first 10 frames, pulse during hold
            float opacity;
            if (LockTimer < 10f)
                opacity = LockTimer / 10f;
            else
                opacity = 0.85f + MathF.Sin(time * 6f + LockTimer * 0.1f) * 0.15f;

            var glowTex = MagnumTextureRegistry.GetSoftGlow();
            if (glowTex == null) return;
            Vector2 glowOrigin = glowTex.Size() * 0.5f;

            // === ROTATING 6-POINTED CONSTELLATION SIGIL ===
            float sigilRotation = time * 1.5f;
            int sigilPoints = 6;

            // Draw the 6 sigil vertex points and connecting lines
            Vector2[] vertexScreenPos = new Vector2[sigilPoints];
            for (int i = 0; i < sigilPoints; i++)
            {
                float angle = MathHelper.TwoPi * i / sigilPoints + sigilRotation;
                Vector2 vertexPos = drawPos + angle.ToRotationVector2() * radius;
                vertexScreenPos[i] = vertexPos;

                // Vertex glow node — pulsing brightness
                float vertexPulse = 0.6f + MathF.Sin(time * 4f + i * 1.2f) * 0.3f;
                float vertexScale = (0.08f + lockProgress * 0.04f) * vertexPulse;
                Color vertexColor = Color.Lerp(IncisorOfMoonlightVFX.ConstellationBlue,
                    IncisorOfMoonlightVFX.HarmonicWhite, (float)i / sigilPoints);

                // Outer bloom
                sb.Draw(glowTex, vertexPos, null,
                    (IncisorOfMoonlightVFX.DeepResonance with { A = 0 }) * 0.25f * opacity,
                    0f, glowOrigin, vertexScale * 2.0f, SpriteEffects.None, 0f);

                // Inner bloom
                sb.Draw(glowTex, vertexPos, null,
                    (vertexColor with { A = 0 }) * 0.6f * opacity * vertexPulse,
                    0f, glowOrigin, vertexScale, SpriteEffects.None, 0f);

                // Bright core
                sb.Draw(glowTex, vertexPos, null,
                    (Color.White with { A = 0 }) * 0.4f * opacity * vertexPulse,
                    0f, glowOrigin, vertexScale * 0.35f, SpriteEffects.None, 0f);
            }

            // Connecting lines between adjacent sigil vertices
            for (int i = 0; i < sigilPoints; i++)
            {
                int next = (i + 1) % sigilPoints;
                Vector2 start = vertexScreenPos[i];
                Vector2 end = vertexScreenPos[next];

                float lineDist = Vector2.Distance(start, end);
                if (lineDist < 2f) continue;

                float lineAngle = (end - start).ToRotation();
                Vector2 lineCenter = (start + end) / 2f;
                float lineAlpha = 0.2f * opacity;

                sb.Draw(glowTex, lineCenter, null,
                    (IncisorOfMoonlightVFX.ConstellationBlue with { A = 0 }) * lineAlpha,
                    lineAngle, glowOrigin,
                    new Vector2(lineDist / glowTex.Width, 0.012f),
                    SpriteEffects.None, 0f);
            }

            // Cross-connecting lines (star pattern: connect every other vertex)
            for (int i = 0; i < sigilPoints; i++)
            {
                int opposite = (i + 2) % sigilPoints;
                Vector2 start = vertexScreenPos[i];
                Vector2 end = vertexScreenPos[opposite];

                float lineDist = Vector2.Distance(start, end);
                if (lineDist < 2f) continue;

                float lineAngle = (end - start).ToRotation();
                Vector2 lineCenter = (start + end) / 2f;
                float lineAlpha = 0.12f * opacity;

                sb.Draw(glowTex, lineCenter, null,
                    (IncisorOfMoonlightVFX.FrequencyPulse with { A = 0 }) * lineAlpha,
                    lineAngle, glowOrigin,
                    new Vector2(lineDist / glowTex.Width, 0.008f),
                    SpriteEffects.None, 0f);
            }

            // 4-pointed star accents at each sigil vertex
            var starTex = MoonlightSonataTextures.Star4Point?.Value;
            if (starTex != null)
            {
                Vector2 starOrigin = starTex.Size() * 0.5f;
                for (int i = 0; i < sigilPoints; i++)
                {
                    float starPulse = 0.5f + MathF.Sin(time * 5f + i * 1.5f) * 0.3f;
                    float starScale = (0.035f + lockProgress * 0.02f) * starPulse;
                    float starRot = time * 3f + i * 0.8f;

                    sb.Draw(starTex, vertexScreenPos[i], null,
                        (IncisorOfMoonlightVFX.ConstellationBlue with { A = 0 }) * 0.5f * opacity * starPulse,
                        starRot, starOrigin, starScale,
                        SpriteEffects.None, 0f);
                }
            }

            // === CENTER GLOW — pulsing core at the locked target ===
            float centerPulse = 1f + MathF.Sin(time * 8f + LockTimer * 0.15f) * 0.25f;
            float centerScale = (0.1f + lockProgress * 0.08f) * centerPulse;

            sb.Draw(glowTex, drawPos, null,
                (IncisorOfMoonlightVFX.DeepResonance with { A = 0 }) * 0.3f * opacity,
                0f, glowOrigin, centerScale * 2.5f, SpriteEffects.None, 0f);
            sb.Draw(glowTex, drawPos, null,
                (IncisorOfMoonlightVFX.ConstellationBlue with { A = 0 }) * 0.5f * opacity * centerPulse,
                0f, glowOrigin, centerScale * 1.4f, SpriteEffects.None, 0f);
            sb.Draw(glowTex, drawPos, null,
                (IncisorOfMoonlightVFX.HarmonicWhite with { A = 0 }) * 0.6f * opacity * centerPulse,
                0f, glowOrigin, centerScale * 0.6f, SpriteEffects.None, 0f);
            sb.Draw(glowTex, drawPos, null,
                (Color.White with { A = 0 }) * 0.5f * opacity,
                0f, glowOrigin, centerScale * 0.2f, SpriteEffects.None, 0f);

            // Counter-rotating inner ring accent (3 small bloom spots)
            float innerRotation = -time * 2.5f;
            for (int i = 0; i < 3; i++)
            {
                float angle = MathHelper.TwoPi * i / 3f + innerRotation;
                Vector2 innerPos = drawPos + angle.ToRotationVector2() * radius * 0.4f;
                float innerPulse = 0.5f + MathF.Sin(time * 6f + i * 2f) * 0.3f;

                sb.Draw(glowTex, innerPos, null,
                    (IncisorOfMoonlightVFX.FrequencyPulse with { A = 0 }) * 0.3f * opacity * innerPulse,
                    0f, glowOrigin, 0.04f * centerPulse, SpriteEffects.None, 0f);
            }
        }
    }
}