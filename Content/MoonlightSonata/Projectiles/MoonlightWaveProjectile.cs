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
using MagnumOpus.Content.MoonlightSonata.Debuffs;
using MagnumOpus.Content.MoonlightSonata.Weapons.IncisorOfMoonlight;

namespace MagnumOpus.Content.MoonlightSonata.Projectiles
{
    /// <summary>
    /// Lunar Crescent Wave — expanding crescent projectile fired by Incisor of Moonlight.
    /// Grows as it travels, gently homes toward enemies.
    ///
    /// VFX overhaul: CalamityStyleTrailRenderer.Cosmic trail, 4-layer {A=0} bloom stack body,
    /// MotionBlurBloomRenderer for velocity stretch, Incisor resonant shimmer palette.
    /// </summary>
    public class MoonlightWaveProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/SoftGlow4";

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 1;
            ProjectileID.Sets.TrailCacheLength[Type] = 28;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 60;
            Projectile.height = 60;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 8;
            Projectile.timeLeft = 90;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.extraUpdates = 1;
            Projectile.alpha = 0;
            Projectile.scale = 0.8f;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }

        public override void AI()
        {
            // Expand as it travels — growing crescent moon
            Projectile.scale += 0.015f;
            if (Projectile.scale > 2.2f)
                Projectile.scale = 2.2f;

            // Grow hitbox
            Projectile.width = (int)(60 * Projectile.scale);
            Projectile.height = (int)(60 * Projectile.scale);

            // Fade out near end of life
            if (Projectile.timeLeft < 30)
                Projectile.alpha += 8;

            if (Projectile.alpha > 255)
            {
                Projectile.Kill();
                return;
            }

            // Gentle homing toward nearest enemy
            if (Projectile.ai[0] == 0f)
            {
                float maxDetectDistance = 400f;
                NPC closest = null;
                float closestDist = maxDetectDistance;

                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (npc.CanBeChasedBy() && npc.active)
                    {
                        float dist = Vector2.Distance(Projectile.Center, npc.Center);
                        if (dist < closestDist)
                        {
                            closestDist = dist;
                            closest = npc;
                        }
                    }
                }

                if (closest != null)
                {
                    Vector2 toTarget = closest.Center - Projectile.Center;
                    toTarget.Normalize();
                    float homingStrength = 0.08f;
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, toTarget * Projectile.velocity.Length(), homingStrength);
                    Projectile.velocity.Normalize();
                    Projectile.velocity *= 12f;
                }
            }

            Projectile.rotation = Projectile.velocity.ToRotation();

            // Resonant shimmer dust trail — precision sparks
            if (Main.rand.NextBool(2))
            {
                Vector2 dustOffset = Main.rand.NextVector2Circular(Projectile.width * 0.25f, Projectile.height * 0.25f);
                Color dustColor = IncisorOfMoonlightVFX.GetResonanceColor(Main.rand.NextFloat(), 1);
                Dust d = Dust.NewDustPerfect(Projectile.Center + dustOffset, DustID.PurpleTorch,
                    -Projectile.velocity * 0.1f, 0, dustColor, 1.4f);
                d.noGravity = true;
                d.fadeIn = 1.3f;
            }

            // Silver contrast sparkle (every 3 frames)
            if ((int)Projectile.ai[1] % 3 == 0)
            {
                MoonlightVFXLibrary.SpawnContrastSparkle(
                    Projectile.Center + Main.rand.NextVector2Circular(10f, 10f),
                    -Projectile.velocity.SafeNormalize(Vector2.Zero));
            }

            // Music notes shed from wave (every 8 frames)
            if ((int)Projectile.ai[1] % 8 == 0)
            {
                MoonlightVFXLibrary.SpawnMusicNotes(Projectile.Center, 1, 8f, 0.7f, 0.85f, 25);
            }

            Projectile.ai[1]++;

            // Dynamic lighting
            float intensity = 1f - (Projectile.alpha / 255f);
            IncisorOfMoonlightVFX.AddResonantLight(Projectile.Center, 0.6f * intensity);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Apply Music's Dissonance debuff
            target.AddBuff(ModContent.BuffType<MusicsDissonance>(), 300);

            // Seeking crystals — 25% chance on hit
            if (Main.rand.NextBool(4))
            {
                SeekingCrystalHelper.SpawnMoonlightCrystals(
                    Projectile.GetSource_FromThis(),
                    target.Center,
                    Projectile.velocity,
                    (int)(damageDone * 0.2f),
                    Projectile.knockBack,
                    Projectile.owner,
                    3);
            }

            // Incisor-themed impact — resonant shockwave
            IncisorOfMoonlightVFX.OnHitImpact(target.Center, 1, hit.Crit);

            // Gradient halo rings — 4 stacked
            CustomParticles.HaloRing(target.Center, MoonlightVFXLibrary.DarkPurple, 0.5f, 15);
            CustomParticles.HaloRing(target.Center, MoonlightVFXLibrary.Violet, 0.4f, 13);
            CustomParticles.HaloRing(target.Center, MoonlightVFXLibrary.IceBlue, 0.3f, 11);
            CustomParticles.HaloRing(target.Center, Color.White * 0.85f, 0.2f, 9);

            // Music note burst (6 notes)
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f;
                Vector2 noteOffset = angle.ToRotationVector2() * 5f;
                MoonlightVFXLibrary.SpawnMusicNotes(target.Center + noteOffset, 1, 4f, 0.8f, 1.0f, 30);
            }

            // Radial dust explosion
            MoonlightVFXLibrary.SpawnRadialDustBurst(target.Center, 12, 5f);

            SoundEngine.PlaySound(SoundID.Item9 with { Volume = 0.6f, Pitch = -0.2f }, target.Center);
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

                    // CalamityStyleTrailRenderer — Cosmic style with Incisor silver shimmer
                    CalamityStyleTrailRenderer.DrawTrailWithBloom(
                        trailPositions, trailRotations,
                        CalamityStyleTrailRenderer.TrailStyle.Cosmic,
                        baseWidth: 12f,
                        primaryColor: IncisorOfMoonlightVFX.ResonantSilver,
                        secondaryColor: MoonlightVFXLibrary.Violet,
                        intensity: 0.75f,
                        bloomMultiplier: 1.8f);
                }
            }

            // === BLOOM STACK BODY (4-layer {A=0} pattern) ===
            float fadeAlpha = 1f - (Projectile.alpha / 255f);
            float pulse = 1f + MathF.Sin(Projectile.timeLeft * 0.15f) * 0.12f;
            float baseBloomScale = 0.3f * Projectile.scale * pulse;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            var bloomTex = MagnumTextureRegistry.GetBloom();
            if (bloomTex != null)
            {
                Vector2 origin = bloomTex.Size() * 0.5f;

                // Layer 1: Outer DeepResonance halo
                sb.Draw(bloomTex, drawPos, null,
                    (IncisorOfMoonlightVFX.DeepResonance with { A = 0 }) * fadeAlpha * 0.25f,
                    0f, origin, baseBloomScale * 2.8f, SpriteEffects.None, 0f);

                // Layer 2: Mid Violet glow
                sb.Draw(bloomTex, drawPos, null,
                    (MoonlightVFXLibrary.Violet with { A = 0 }) * fadeAlpha * 0.45f,
                    0f, origin, baseBloomScale * 1.8f, SpriteEffects.None, 0f);

                // Layer 3: Inner FrequencyPulse
                sb.Draw(bloomTex, drawPos, null,
                    (IncisorOfMoonlightVFX.FrequencyPulse with { A = 0 }) * fadeAlpha * 0.6f,
                    0f, origin, baseBloomScale * 1.1f, SpriteEffects.None, 0f);

                // Layer 4: HarmonicWhite core
                sb.Draw(bloomTex, drawPos, null,
                    (IncisorOfMoonlightVFX.HarmonicWhite with { A = 0 }) * fadeAlpha * 0.8f,
                    0f, origin, baseBloomScale * 0.5f, SpriteEffects.None, 0f);
            }

            // === MOTION BLUR BLOOM (velocity-based stretch) ===
            if (Projectile.velocity.LengthSquared() > 4f)
            {
                Texture2D texture = TextureAssets.Projectile[Type].Value;
                MotionBlurBloomRenderer.DrawProjectile(sb, texture, Projectile,
                    IncisorOfMoonlightVFX.FrequencyPulse, MoonlightVFXLibrary.IceBlue, intensityMult: 0.5f);
            }

            return false;
        }

        public override void OnKill(int timeLeft)
        {
            // Death VFX: bloom flash + crystal shard scatter
            MoonlightVFXLibrary.ProjectileImpact(Projectile.Center, 0.6f);

            for (int i = 0; i < 6; i++)
            {
                Vector2 crystalVel = Main.rand.NextVector2Circular(5f, 5f);
                Dust crystal = Dust.NewDustDirect(Projectile.Center, 1, 1,
                    DustID.PurpleCrystalShard, crystalVel.X, crystalVel.Y, 100, default, 1.2f);
                crystal.noGravity = true;
            }

            SoundEngine.PlaySound(SoundID.Item10 with { Volume = 0.5f, Pitch = -0.4f }, Projectile.Center);
        }

        public override Color? GetAlpha(Color lightColor)
        {
            float alpha = 1f - (Projectile.alpha / 255f);
            return MoonlightVFXLibrary.Violet * alpha;
        }
    }
}
