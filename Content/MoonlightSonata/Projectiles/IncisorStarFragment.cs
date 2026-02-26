using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent;
using MagnumOpus.Content.MoonlightSonata.Debuffs;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.VFX.Core;
using MagnumOpus.Common.Systems.VFX.Trails;
using MagnumOpus.Content.MoonlightSonata;
using MagnumOpus.Content.MoonlightSonata.Dusts;
using MagnumOpus.Content.MoonlightSonata.Weapons.IncisorOfMoonlight;

namespace MagnumOpus.Content.MoonlightSonata.Projectiles
{
    /// <summary>
    /// Seeking star fragment projectile spawned by Incisor of Moonlight.
    /// Small, fast, precise — traces constellation-like connecting lines between positions.
    /// Leaves StarPointDust trail points that twinkle like stars in a constellation.
    /// Aggressively homes toward enemies with tight angular correction.
    /// </summary>
    public class IncisorStarFragment : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/CrispStar4";

        private float SpinRotation
        {
            get => Projectile.ai[0];
            set => Projectile.ai[0] = value;
        }

        private float HomingIntensity
        {
            get => Projectile.ai[1];
            set => Projectile.ai[1] = value;
        }

        // Store trail positions for constellation line drawing
        private Vector2[] _constellationPoints = new Vector2[8];
        private int _pointIndex;
        private int _pointTimer;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 12;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 2;
            Projectile.timeLeft = 90;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.alpha = 0;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 8;
            Projectile.scale = 0.3f;
            Projectile.extraUpdates = 1;
        }

        public override void AI()
        {
            // Rapid spin
            SpinRotation += 0.35f;
            Projectile.rotation = SpinRotation;

            // Ramp up homing over time
            if (HomingIntensity < 0.08f)
                HomingIntensity += 0.002f;

            // Aggressive homing — precision seeking
            float homingRange = 450f;
            NPC closestNPC = null;
            float closestDist = homingRange;

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

            if (closestNPC != null)
            {
                Vector2 toTarget = (closestNPC.Center - Projectile.Center).SafeNormalize(Vector2.UnitX);
                float targetSpeed = Projectile.velocity.Length();
                Projectile.velocity = Vector2.Lerp(Projectile.velocity,
                    toTarget * targetSpeed, HomingIntensity);
            }

            // Record constellation points every 6 frames
            _pointTimer++;
            if (_pointTimer >= 6)
            {
                _pointTimer = 0;
                _constellationPoints[_pointIndex % _constellationPoints.Length] = Projectile.Center;
                _pointIndex++;
            }

            // StarPointDust trail — place constellation "stars" along path
            if (Main.rand.NextBool(3))
            {
                Color starColor = IncisorOfMoonlightVFX.GetResonanceColor(Main.rand.NextFloat(), 2);
                Dust star = Dust.NewDustPerfect(
                    Projectile.Center + Main.rand.NextVector2Circular(4f, 4f),
                    ModContent.DustType<StarPointDust>(),
                    -Projectile.velocity * 0.05f,
                    0, starColor, 0.8f);
                star.customData = new StarPointBehavior(0.15f, 35);
            }

            // Occasional precision spark
            if (Main.rand.NextBool(5))
            {
                Vector2 sparkVel = Projectile.velocity.RotatedByRandom(0.3f) * -0.2f;
                Color sparkColor = Color.Lerp(IncisorOfMoonlightVFX.ResonantSilver,
                    MoonlightVFXLibrary.IceBlue, Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.Enchanted_Gold,
                    sparkVel, 0, sparkColor, 0.7f);
                d.noGravity = true;
            }

            // Lighting
            float pulse = 0.5f + MathF.Sin(Main.GlobalTimeWrappedHourly * 8f) * 0.15f;
            Lighting.AddLight(Projectile.Center, MoonlightVFXLibrary.IceBlue.ToVector3() * pulse);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Vector2 origin = texture.Size() / 2f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            // === CONSTELLATION LINES ===
            // Draw faint connecting lines between recorded trail points
            int maxPoints = Math.Min(_pointIndex, _constellationPoints.Length);
            if (maxPoints > 1)
            {
                var bloomTex = MagnumTextureRegistry.GetSoftGlow();
                if (bloomTex != null)
                {
                    for (int i = 1; i < maxPoints; i++)
                    {
                        int idx = (_pointIndex - maxPoints + i) % _constellationPoints.Length;
                        int prevIdx = (_pointIndex - maxPoints + i - 1) % _constellationPoints.Length;
                        Vector2 start = _constellationPoints[prevIdx] - Main.screenPosition;
                        Vector2 end = _constellationPoints[idx] - Main.screenPosition;

                        if (start == Vector2.Zero || end == Vector2.Zero) continue;

                        float lineDist = Vector2.Distance(start, end);
                        float lineAngle = (end - start).ToRotation();
                        float lineAlpha = 0.15f * ((float)i / maxPoints);

                        // Draw line as stretched glow
                        Vector2 lineCenter = (start + end) / 2f;
                        sb.Draw(bloomTex, lineCenter, null,
                            (MoonlightVFXLibrary.IceBlue with { A = 0 }) * lineAlpha,
                            lineAngle, bloomTex.Size() / 2f,
                            new Vector2(lineDist / bloomTex.Width, 0.02f),
                            SpriteEffects.None, 0f);
                    }
                }
            }

            // === CALAMITY-STYLE TRAIL ===
            if (Projectile.oldPos.Length > 1)
            {
                Vector2[] trailPos = new Vector2[Projectile.oldPos.Length];
                float[] trailRot = new float[Projectile.oldPos.Length];
                int validCount = 0;

                for (int i = 0; i < Projectile.oldPos.Length; i++)
                {
                    if (Projectile.oldPos[i] == Vector2.Zero) break;
                    trailPos[i] = Projectile.oldPos[i] + Projectile.Size / 2f;
                    trailRot[i] = Projectile.oldRot[i];
                    validCount++;
                }

                if (validCount > 1)
                {
                    if (validCount < trailPos.Length)
                    {
                        Array.Resize(ref trailPos, validCount);
                        Array.Resize(ref trailRot, validCount);
                    }

                    CalamityStyleTrailRenderer.DrawTrailWithBloom(
                        trailPos, trailRot,
                        CalamityStyleTrailRenderer.TrailStyle.Ice,
                        baseWidth: 6f,
                        primaryColor: IncisorOfMoonlightVFX.ResonantSilver,
                        secondaryColor: IncisorOfMoonlightVFX.FrequencyPulse,
                        intensity: 0.6f,
                        bloomMultiplier: 1.5f);
                }
            }

            // === BLOOM BODY ===
            var bloom = MagnumTextureRegistry.GetBloom();
            if (bloom != null)
            {
                Vector2 bloomOrigin = bloom.Size() * 0.5f;
                float pulse = 1f + MathF.Sin(Main.GlobalTimeWrappedHourly * 10f) * 0.15f;
                float bloomScale = 0.15f * pulse;

                sb.Draw(bloom, drawPos, null,
                    (IncisorOfMoonlightVFX.DeepResonance with { A = 0 }) * 0.3f,
                    0f, bloomOrigin, bloomScale * 2.2f, SpriteEffects.None, 0f);
                sb.Draw(bloom, drawPos, null,
                    (IncisorOfMoonlightVFX.FrequencyPulse with { A = 0 }) * 0.5f,
                    0f, bloomOrigin, bloomScale * 1.4f, SpriteEffects.None, 0f);
                sb.Draw(bloom, drawPos, null,
                    (MoonlightVFXLibrary.IceBlue with { A = 0 }) * 0.7f,
                    0f, bloomOrigin, bloomScale * 0.8f, SpriteEffects.None, 0f);
                sb.Draw(bloom, drawPos, null,
                    (Color.White with { A = 0 }) * 0.8f,
                    0f, bloomOrigin, bloomScale * 0.3f, SpriteEffects.None, 0f);
            }

            // === MAIN STAR SPRITE ===
            sb.Draw(texture, drawPos, null,
                (IncisorOfMoonlightVFX.ResonantSilver with { A = 0 }) * 0.9f,
                Projectile.rotation, origin, Projectile.scale,
                SpriteEffects.None, 0f);

            // Counter-rotating overlay
            sb.Draw(texture, drawPos, null,
                (MoonlightVFXLibrary.IceBlue with { A = 0 }) * 0.4f,
                -Projectile.rotation * 0.7f, origin, Projectile.scale * 0.6f,
                SpriteEffects.None, 0f);

            return false;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<MusicsDissonance>(), 180);

            // Resonant impact — precision burst
            IncisorOfMoonlightVFX.OnHitImpact(target.Center, 1, hit.Crit);

            // Resonant pulse ring at impact
            Dust pulse = Dust.NewDustPerfect(target.Center,
                ModContent.DustType<ResonantPulseDust>(),
                Vector2.Zero, 0,
                IncisorOfMoonlightVFX.FrequencyPulse, 0.8f);
            pulse.customData = new ResonantPulseBehavior(0.05f, 25);
        }

        public override void OnKill(int timeLeft)
        {
            // Starburst death effect
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f;
                Vector2 vel = angle.ToRotationVector2() * 3f;
                Color starColor = Color.Lerp(IncisorOfMoonlightVFX.ResonantSilver,
                    MoonlightVFXLibrary.IceBlue, (float)i / 6f);
                Dust star = Dust.NewDustPerfect(Projectile.Center,
                    ModContent.DustType<StarPointDust>(),
                    vel, 0, starColor, 0.6f);
                star.customData = new StarPointBehavior(0.2f, 25);
            }

            CustomParticles.GenericFlare(Projectile.Center,
                IncisorOfMoonlightVFX.ResonantSilver, 0.3f, 15);
        }
    }
}
