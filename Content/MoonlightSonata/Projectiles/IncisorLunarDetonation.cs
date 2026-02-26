using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using MagnumOpus.Content.MoonlightSonata.Debuffs;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.VFX.Core;
using MagnumOpus.Common.Systems.VFX.Bloom;
using MagnumOpus.Common.Systems.VFX.Optimization;
using MagnumOpus.Content.MoonlightSonata;
using MagnumOpus.Content.MoonlightSonata.Dusts;
using MagnumOpus.Content.MoonlightSonata.Weapons.IncisorOfMoonlight;

namespace MagnumOpus.Content.MoonlightSonata.Projectiles
{
    /// <summary>
    /// Expanding lunar detonation zone — Phase 3 Crescendo finisher projectile.
    /// Creates a full moon sigil that expands, pulses, and damages enemies in radius.
    /// Inspired by Coralite's NoctiflairStrike expanding phase mechanics
    /// combined with VFX+'s RainbowSigil vertex-based rendering.
    /// Visual: expanding moonlight ring with inner sigil pattern, resonant pulses,
    /// constellation star bursts, and a final grand detonation flash.
    /// </summary>
    public class IncisorLunarDetonation : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/GlowingHalo1";

        private float Timer
        {
            get => Projectile.ai[0];
            set => Projectile.ai[0] = value;
        }

        private float MaxRadius
        {
            get => Projectile.ai[1];
            set => Projectile.ai[1] = value;
        }

        private const int ExpandDuration = 20;
        private const int HoldDuration = 15;
        private const int CollapseDuration = 10;
        private int TotalLifetime => ExpandDuration + HoldDuration + CollapseDuration;

        private float CurrentRadius
        {
            get
            {
                float maxR = MaxRadius > 0 ? MaxRadius : 180f;

                if (Timer < ExpandDuration)
                {
                    // Expand with deceleration
                    float t = Timer / ExpandDuration;
                    float ease = 1f - (1f - t) * (1f - t); // EaseOut
                    return maxR * ease;
                }
                else if (Timer < ExpandDuration + HoldDuration)
                {
                    // Hold at max with gentle pulse
                    float pulse = 1f + MathF.Sin((Timer - ExpandDuration) * 0.4f) * 0.05f;
                    return maxR * pulse;
                }
                else
                {
                    // Collapse
                    float t = (Timer - ExpandDuration - HoldDuration) / CollapseDuration;
                    float ease = (1f - t) * (1f - t); // EaseIn (reverse)
                    return maxR * ease;
                }
            }
        }

        public override void SetDefaults()
        {
            Projectile.width = 360;
            Projectile.height = 360;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 60;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.alpha = 0;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 15;
        }

        public override void AI()
        {
            Timer++;
            Projectile.velocity = Vector2.Zero;

            float radius = CurrentRadius;

            // Update hitbox to match radius
            int hitboxSize = (int)(radius * 2f);
            hitboxSize = Math.Max(hitboxSize, 10);
            Projectile.position = Projectile.Center - new Vector2(hitboxSize / 2f);
            Projectile.width = hitboxSize;
            Projectile.height = hitboxSize;

            if (Timer >= TotalLifetime)
                Projectile.Kill();

            // === VFX PHASES ===

            // Expanding phase: ring of resonant pulse dusts
            if (Timer < ExpandDuration && Timer % 3 == 0)
            {
                int ringCount = 6;
                for (int i = 0; i < ringCount; i++)
                {
                    float angle = MathHelper.TwoPi * i / ringCount + Timer * 0.1f;
                    Vector2 pos = Projectile.Center + angle.ToRotationVector2() * radius * 0.8f;
                    Color pulseColor = Color.Lerp(IncisorOfMoonlightVFX.DeepResonance,
                        IncisorOfMoonlightVFX.HarmonicWhite, (float)i / ringCount);

                    Dust ring = Dust.NewDustPerfect(pos,
                        ModContent.DustType<ResonantPulseDust>(),
                        Vector2.Zero, 0, pulseColor, 0.3f);
                    ring.customData = new ResonantPulseBehavior(0.03f, 20);
                }
            }

            // Star points during hold phase
            if (Timer >= ExpandDuration && Timer < ExpandDuration + HoldDuration)
            {
                if (Timer % 2 == 0)
                {
                    float starAngle = Main.rand.NextFloat(MathHelper.TwoPi);
                    float starDist = Main.rand.NextFloat(0.3f, 1f) * radius;
                    Vector2 starPos = Projectile.Center + starAngle.ToRotationVector2() * starDist;

                    Color starColor = IncisorOfMoonlightVFX.GetResonanceColor(Main.rand.NextFloat(), 3);
                    Dust star = Dust.NewDustPerfect(starPos,
                        ModContent.DustType<StarPointDust>(),
                        Main.rand.NextVector2Circular(1f, 1f),
                        0, starColor, 0.5f);
                    star.customData = new StarPointBehavior(0.2f, 15);
                }
            }

            // Perimeter particles every frame
            if (Main.rand.NextBool(2))
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 rimPos = Projectile.Center + angle.ToRotationVector2() * radius;
                Vector2 tangent = new Vector2(-MathF.Sin(angle), MathF.Cos(angle)) * 2f;
                Color rimColor = Color.Lerp(MoonlightVFXLibrary.Violet,
                    MoonlightVFXLibrary.MoonWhite, Main.rand.NextFloat(0.3f));
                Dust d = Dust.NewDustPerfect(rimPos, DustID.Enchanted_Pink,
                    tangent, 0, rimColor, 0.9f);
                d.noGravity = true;
            }

            // Lunar motes orbiting the detonation
            if (Timer % 8 == 0)
            {
                float moteAngle = Timer * 0.15f;
                for (int i = 0; i < 3; i++)
                {
                    float angle = moteAngle + MathHelper.TwoPi * i / 3f;
                    Vector2 motePos = Projectile.Center + angle.ToRotationVector2() * radius * 0.6f;
                    Color moteColor = Color.Lerp(MoonlightVFXLibrary.DarkPurple,
                        MoonlightVFXLibrary.IceBlue, (float)i / 3f);
                    Dust mote = Dust.NewDustPerfect(motePos,
                        ModContent.DustType<LunarMote>(),
                        Vector2.Zero, 0, moteColor, 0.5f);
                    mote.customData = new LunarMoteBehavior(Projectile.Center, angle)
                    {
                        OrbitRadius = radius * 0.6f,
                        OrbitSpeed = 0.06f,
                        Lifetime = 20
                    };
                }
            }

            // Music notes spiraling outward
            if (Timer % 5 == 0)
            {
                float noteAngle = Timer * 0.2f;
                Vector2 notePos = Projectile.Center + noteAngle.ToRotationVector2() * radius * 0.5f;
                MoonlightVFXLibrary.SpawnMusicNotes(notePos, 1, 3f, 0.7f, 0.9f, 25);
            }

            // Collapse phase: inrush particles
            if (Timer >= ExpandDuration + HoldDuration)
            {
                int rushCount = 4;
                for (int i = 0; i < rushCount; i++)
                {
                    float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                    Vector2 startPos = Projectile.Center + angle.ToRotationVector2() * (radius + 30f);
                    Vector2 vel = (Projectile.Center - startPos).SafeNormalize(Vector2.Zero) * 5f;
                    Color rushColor = Color.Lerp(IncisorOfMoonlightVFX.FrequencyPulse,
                        IncisorOfMoonlightVFX.HarmonicWhite, Main.rand.NextFloat());
                    Dust d = Dust.NewDustPerfect(startPos, DustID.MagicMirror, vel, 0, rushColor, 1.2f);
                    d.noGravity = true;
                }
            }

            // Final frame: grand detonation flash
            if (Timer == TotalLifetime - 1)
            {
                // Massive bloom cascade
                CustomParticles.GenericFlare(Projectile.Center, IncisorOfMoonlightVFX.DeepResonance, 1.0f, 25);
                CustomParticles.GenericFlare(Projectile.Center, IncisorOfMoonlightVFX.FrequencyPulse, 0.8f, 22);
                CustomParticles.GenericFlare(Projectile.Center, IncisorOfMoonlightVFX.ResonantSilver, 0.6f, 20);
                CustomParticles.GenericFlare(Projectile.Center, IncisorOfMoonlightVFX.HarmonicWhite, 0.4f, 18);

                // Star burst pattern
                for (int i = 0; i < 12; i++)
                {
                    float angle = MathHelper.TwoPi * i / 12f;
                    Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 8f);
                    Color starColor = IncisorOfMoonlightVFX.GetResonanceColor((float)i / 12f, 3);
                    Dust star = Dust.NewDustPerfect(Projectile.Center,
                        ModContent.DustType<StarPointDust>(),
                        vel, 0, starColor, 0.7f);
                    star.customData = new StarPointBehavior(0.18f, 30);
                }

                // God ray burst
                GodRaySystem.CreateBurst(Projectile.Center, MoonlightVFXLibrary.IceBlue,
                    rayCount: 8, radius: 60f, duration: 30,
                    GodRaySystem.GodRayStyle.Explosion,
                    secondaryColor: IncisorOfMoonlightVFX.FrequencyPulse);

                // Screen effects
                if (AdaptiveQualityManager.Instance?.CurrentQuality >= AdaptiveQualityManager.QualityLevel.Medium)
                {
                    ScreenDistortionManager.TriggerRipple(Projectile.Center,
                        IncisorOfMoonlightVFX.FrequencyPulse, 0.8f, 25);
                    MagnumScreenEffects.AddScreenShake(6f);
                }

                // Music note cascade
                for (int i = 0; i < 8; i++)
                {
                    float noteAngle = MathHelper.TwoPi * i / 8f;
                    Vector2 noteOffset = noteAngle.ToRotationVector2() * 15f;
                    MoonlightVFXLibrary.SpawnMusicNotes(Projectile.Center + noteOffset, 1, 6f, 0.9f, 1.1f, 35);
                }

                // Sound
                SoundEngine.PlaySound(SoundID.Item122 with { Volume = 0.8f, Pitch = 0.3f }, Projectile.Center);
            }

            // Dynamic lighting
            float lightIntensity = MathHelper.Clamp(radius / 100f, 0.3f, 1.2f);
            Lighting.AddLight(Projectile.Center, MoonlightVFXLibrary.Violet.ToVector3() * lightIntensity);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Texture2D haloTex = Terraria.GameContent.TextureAssets.Projectile[Type].Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = haloTex.Size() / 2f;
            float radius = CurrentRadius;
            float lifeProgress = Timer / TotalLifetime;

            // Overall opacity: fade in quickly, hold, fade out during collapse
            float opacity;
            if (Timer < 5)
                opacity = Timer / 5f;
            else if (Timer >= ExpandDuration + HoldDuration)
                opacity = 1f - (Timer - ExpandDuration - HoldDuration) / CollapseDuration;
            else
                opacity = 1f;

            // === OUTER RING ===
            float ringScale = radius / 64f; // Assuming 128px texture, radius in pixels
            float ringRotation = Timer * 0.02f;

            // Layer 1: Deep resonance outer ring
            sb.Draw(haloTex, drawPos, null,
                (IncisorOfMoonlightVFX.DeepResonance with { A = 0 }) * 0.35f * opacity,
                ringRotation, origin, ringScale * 1.15f,
                SpriteEffects.None, 0f);

            // Layer 2: Frequency pulse ring
            sb.Draw(haloTex, drawPos, null,
                (IncisorOfMoonlightVFX.FrequencyPulse with { A = 0 }) * 0.5f * opacity,
                -ringRotation * 0.7f, origin, ringScale,
                SpriteEffects.None, 0f);

            // Layer 3: Ice blue inner ring
            sb.Draw(haloTex, drawPos, null,
                (MoonlightVFXLibrary.IceBlue with { A = 0 }) * 0.4f * opacity,
                ringRotation * 1.3f, origin, ringScale * 0.75f,
                SpriteEffects.None, 0f);

            // === INNER SIGIL (star patterns inside the ring) ===
            var starTex = MagnumTextureRegistry.GetSoftGlow();
            if (starTex != null)
            {
                Vector2 starOrigin = starTex.Size() / 2f;

                // 8-point star sigil inside the ring
                int sigilPoints = 8;
                for (int i = 0; i < sigilPoints; i++)
                {
                    float angle = MathHelper.TwoPi * i / sigilPoints + Timer * 0.03f;
                    float dist = radius * 0.5f;
                    Vector2 sigilPos = drawPos + angle.ToRotationVector2() * dist;

                    float pointOpacity = opacity * (0.4f + MathF.Sin(Timer * 0.2f + i) * 0.2f);
                    Color pointColor = Color.Lerp(IncisorOfMoonlightVFX.ResonantSilver,
                        IncisorOfMoonlightVFX.HarmonicWhite, (float)i / sigilPoints);

                    sb.Draw(starTex, sigilPos, null,
                        (pointColor with { A = 0 }) * pointOpacity,
                        angle, starOrigin, 0.15f,
                        SpriteEffects.None, 0f);

                    // Connecting lines between sigil points
                    if (i > 0)
                    {
                        float prevAngle = MathHelper.TwoPi * (i - 1) / sigilPoints + Timer * 0.03f;
                        Vector2 prevPos = drawPos + prevAngle.ToRotationVector2() * dist;
                        float lineDist = Vector2.Distance(sigilPos, prevPos);
                        float lineAngle = (sigilPos - prevPos).ToRotation();
                        Vector2 lineCenter = (sigilPos + prevPos) / 2f;

                        sb.Draw(starTex, lineCenter, null,
                            (MoonlightVFXLibrary.IceBlue with { A = 0 }) * 0.12f * opacity,
                            lineAngle, starOrigin,
                            new Vector2(lineDist / starTex.Width, 0.015f),
                            SpriteEffects.None, 0f);
                    }
                }

                // Close the sigil loop
                {
                    float firstAngle = Timer * 0.03f;
                    float lastAngle = MathHelper.TwoPi * (sigilPoints - 1) / sigilPoints + Timer * 0.03f;
                    Vector2 firstPos = drawPos + firstAngle.ToRotationVector2() * radius * 0.5f;
                    Vector2 lastPos = drawPos + lastAngle.ToRotationVector2() * radius * 0.5f;
                    float lineDist = Vector2.Distance(firstPos, lastPos);
                    float lineAngle = (firstPos - lastPos).ToRotation();
                    Vector2 lineCenter = (firstPos + lastPos) / 2f;

                    sb.Draw(starTex, lineCenter, null,
                        (MoonlightVFXLibrary.IceBlue with { A = 0 }) * 0.12f * opacity,
                        lineAngle, starOrigin,
                        new Vector2(lineDist / starTex.Width, 0.015f),
                        SpriteEffects.None, 0f);
                }

                // Center glow
                float centerPulse = 1f + MathF.Sin(Timer * 0.3f) * 0.2f;
                sb.Draw(starTex, drawPos, null,
                    (IncisorOfMoonlightVFX.HarmonicWhite with { A = 0 }) * 0.4f * opacity,
                    0f, starOrigin, 0.25f * centerPulse,
                    SpriteEffects.None, 0f);
            }

            return false;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<MusicsDissonance>(), 360);

            // Resonant impact
            CustomParticles.GenericFlare(target.Center, IncisorOfMoonlightVFX.FrequencyPulse, 0.4f, 15);

            // Resonant pulse ring on hit
            Dust pulse = Dust.NewDustPerfect(target.Center,
                ModContent.DustType<ResonantPulseDust>(),
                Vector2.Zero, 0,
                MoonlightVFXLibrary.Violet, 0.6f);
            pulse.customData = new ResonantPulseBehavior(0.04f, 20);
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            // Circular collision based on current radius
            float radius = CurrentRadius;
            Vector2 closestPoint = new Vector2(
                MathHelper.Clamp(Projectile.Center.X, targetHitbox.Left, targetHitbox.Right),
                MathHelper.Clamp(Projectile.Center.Y, targetHitbox.Top, targetHitbox.Bottom));

            return Vector2.Distance(Projectile.Center, closestPoint) <= radius;
        }
    }
}
