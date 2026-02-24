using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Common.Systems.VFX.Core;
using MagnumOpus.Common.Systems.VFX.Trails;
using MagnumOpus.Content.MoonlightSonata;
using MagnumOpus.Content.MoonlightSonata.Weapons.EternalMoon;

namespace MagnumOpus.Content.MoonlightSonata.Projectiles
{
    /// <summary>
    /// Purple energy wave projectile fired by the Eternal Moon sword.
    ///
    /// VFX overhaul: CalamityStyleTrailRenderer.Cosmic trail with MoonlightTrail.fx shader,
    /// 4-layer bloom stack body with {A=0}, MotionBlurBloomRenderer for velocity stretch,
    /// gentle sine-wave lateral drift, and music note shedding.
    /// </summary>
    public class EternalMoonWave : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/SwordArc2";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 24;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 40;
            Projectile.height = 40;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 5;
            Projectile.timeLeft = 60;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.alpha = 255;
            Projectile.extraUpdates = 1;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();

            // Gentle sine-wave lateral drift for organic flow
            float driftAngle = MathF.Sin(Projectile.ai[0] * 0.12f) * 0.04f;
            Projectile.velocity = Projectile.velocity.RotatedBy(driftAngle);
            Projectile.ai[0]++;

            // Wave pulsing effect
            Projectile.scale = 1f + MathF.Sin(Projectile.timeLeft * 0.3f) * 0.12f;

            // Music note shedding from the wave (every 8 frames)
            if ((int)Projectile.ai[0] % 8 == 0)
            {
                MoonlightVFXLibrary.SpawnMusicNotes(Projectile.Center, 1, 10f, 0.7f, 0.9f, 25);
            }

            // Phase-cycling dust trailing behind
            if (Main.rand.NextBool(2))
            {
                Vector2 dustOffset = Main.rand.NextVector2Circular(12f, 12f);
                Color dustColor = EternalMoonVFX.GetLunarPhaseColor(Main.rand.NextFloat(), 0);
                Dust d = Dust.NewDustPerfect(Projectile.Center + dustOffset, DustID.PurpleTorch,
                    -Projectile.velocity * 0.08f, 0, dustColor, 1.4f);
                d.noGravity = true;
                d.fadeIn = 1.2f;
            }

            // Dynamic lighting
            EternalMoonVFX.AddCrescentLight(Projectile.Center, 0.7f);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;

            // === CALAMITY-STYLE TRAIL RENDERING ===
            if (Projectile.oldPos.Length > 1)
            {
                // Build position array (center offset)
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
                    // Trim to valid count
                    if (validCount < trailPositions.Length)
                    {
                        Array.Resize(ref trailPositions, validCount);
                        Array.Resize(ref trailRotations, validCount);
                    }

                    // CalamityStyleTrailRenderer with Cosmic style — flowing moonlight
                    CalamityStyleTrailRenderer.DrawTrailWithBloom(
                        trailPositions, trailRotations,
                        CalamityStyleTrailRenderer.TrailStyle.Cosmic,
                        baseWidth: 14f,
                        primaryColor: MoonlightVFXLibrary.Violet,
                        secondaryColor: MoonlightVFXLibrary.IceBlue,
                        intensity: 0.8f,
                        bloomMultiplier: 2.0f);
                }
            }

            // === BLOOM STACK BODY (4-layer {A=0} pattern) ===
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float pulse = 1f + MathF.Sin(Projectile.timeLeft * 0.15f) * 0.15f;
            float baseBloomScale = 0.35f * pulse;

            var bloomTex = MagnumTextureRegistry.GetBloom();
            if (bloomTex != null)
            {
                Vector2 origin = bloomTex.Size() * 0.5f;

                // Layer 1: Outer halo (DarkPurple)
                sb.Draw(bloomTex, drawPos, null,
                    (MoonlightVFXLibrary.DarkPurple with { A = 0 }) * 0.3f,
                    0f, origin, baseBloomScale * 2.5f, SpriteEffects.None, 0f);

                // Layer 2: Mid glow (Violet)
                sb.Draw(bloomTex, drawPos, null,
                    (MoonlightVFXLibrary.Violet with { A = 0 }) * 0.5f,
                    0f, origin, baseBloomScale * 1.6f, SpriteEffects.None, 0f);

                // Layer 3: Inner (IceBlue)
                sb.Draw(bloomTex, drawPos, null,
                    (MoonlightVFXLibrary.IceBlue with { A = 0 }) * 0.7f,
                    0f, origin, baseBloomScale * 1.0f, SpriteEffects.None, 0f);

                // Layer 4: White-hot core
                sb.Draw(bloomTex, drawPos, null,
                    (Color.White with { A = 0 }) * 0.85f,
                    0f, origin, baseBloomScale * 0.45f, SpriteEffects.None, 0f);
            }

            // === MOTION BLUR BLOOM (velocity-based stretch) ===
            if (Projectile.velocity.LengthSquared() > 4f)
            {
                Texture2D texture = TextureAssets.Projectile[Type].Value;
                MotionBlurBloomRenderer.DrawProjectile(sb, texture, Projectile,
                    MoonlightVFXLibrary.Violet, MoonlightVFXLibrary.IceBlue, intensityMult: 0.6f);
            }

            return false;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<Debuffs.MusicsDissonance>(), 180);

            // EternalMoon-themed impact with bloom + halo cascade
            MoonlightVFXLibrary.ProjectileImpact(target.Center, 0.8f);
        }

        public override void OnKill(int timeLeft)
        {
            // Death VFX: bloom flash + music note scatter
            MoonlightVFXLibrary.ProjectileImpact(Projectile.Center, 0.5f);
        }
    }
}
