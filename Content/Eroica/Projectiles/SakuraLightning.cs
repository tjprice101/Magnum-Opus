using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using MagnumOpus.Content.Eroica;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Content.Eroica.Projectiles
{
    /// <summary>
    /// Spiral explosion effect spawned by Piercing Light of the Sakura projectile.
    /// Red and gold spiral explosion with scarlet particles.
    /// All old API calls replaced with EroicaVFXLibrary + vanilla Dust + MagnumParticleHandler.
    /// </summary>
    public class SakuraLightning : ModProjectile
    {
        // Override texture to use particle asset since we draw with particles
        public override string Texture => "MagnumOpus/Assets/Particles Asset Library/LightningBurst";

        private bool initialized = false;
        private float spiralAngle = 0f;
        private int spiralCounter = 0;

        public override void SetDefaults()
        {
            Projectile.width = 80;
            Projectile.height = 80;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 45;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.alpha = 255;
            Projectile.light = 0f; // Lighting handled explicitly below
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }

        public override void AI()
        {
            if (!initialized)
            {
                initialized = true;
                SoundEngine.PlaySound(SoundID.Item74 with { Pitch = 0.3f, Volume = 0.8f }, Projectile.Center);
                CreateInitialBurst();
            }

            // Projectile stays in place
            Projectile.velocity = Vector2.Zero;

            // Intense lighting  Ered and gold pulse
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.3f) * 0.3f + 0.7f;
            Lighting.AddLight(Projectile.Center, 1.2f * pulse, 0.5f * pulse, 0.1f * pulse);

            // Expanding spiral effect
            spiralCounter++;
            spiralAngle += 0.3f;

            float progress = 1f - (Projectile.timeLeft / 45f);
            float radius = 20f + progress * 60f;

            // 3 spiral arms  Escarlet red and gold
            for (int arm = 0; arm < 3; arm++)
            {
                float armAngle = spiralAngle + (MathHelper.TwoPi / 3f) * arm;
                Vector2 spiralPos = Projectile.Center + new Vector2((float)Math.Cos(armAngle), (float)Math.Sin(armAngle)) * radius;

                Dust scarlet = Dust.NewDustPerfect(spiralPos, DustID.CrimsonTorch,
                    Main.rand.NextVector2Circular(2f, 2f), 100, default, 2.0f);
                scarlet.noGravity = true;
                scarlet.fadeIn = 1.2f;

                if (Main.rand.NextBool(2))
                {
                    Dust gold = Dust.NewDustPerfect(spiralPos, DustID.GoldFlame,
                        Main.rand.NextVector2Circular(2f, 2f), 100, default, 1.8f);
                    gold.noGravity = true;
                    gold.fadeIn = 1.0f;
                }
            }

            // Inner scarlet glow
            if (spiralCounter % 2 == 0)
            {
                float innerRadius = radius * 0.5f;
                for (int i = 0; i < 4; i++)
                {
                    float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                    Vector2 pos = Projectile.Center + angle.ToRotationVector2() * Main.rand.NextFloat(innerRadius);

                    Dust inner = Dust.NewDustPerfect(pos, DustID.CrimsonTorch, Vector2.Zero, 150, default, 1.5f);
                    inner.noGravity = true;
                }
            }

            // Expanding ring effect
            if (spiralCounter % 5 == 0)
            {
                for (int i = 0; i < 12; i++)
                {
                    float angle = MathHelper.TwoPi * i / 12f;
                    Vector2 vel = angle.ToRotationVector2() * (3f + progress * 5f);

                    int dustType = Main.rand.NextBool() ? DustID.CrimsonTorch : DustID.GoldFlame;
                    Dust ring = Dust.NewDustPerfect(Projectile.Center, dustType, vel, 100, default, 1.5f);
                    ring.noGravity = true;
                }
            }
        }

        private void CreateInitialBurst()
        {
            // Heroic impact flash  Ebloom + halo + directional sparks
            EroicaVFXLibrary.HeroicImpact(Projectile.Center, 2.5f);

            // Sakura petal scatter
            EroicaVFXLibrary.SpawnSakuraPetals(Projectile.Center, 15, 50f);

            // Musical chord burst  Egold music notes
            EroicaVFXLibrary.MusicNoteBurst(Projectile.Center, new Color(255, 215, 0), 6, 4f);

            // Bloom flare + ring at epicenter
            EroicaVFXLibrary.BloomFlare(Projectile.Center, new Color(255, 180, 200), 0.7f, 20);
            var impactRing = new BloomRingParticle(Projectile.Center, Vector2.Zero,
                new Color(255, 150, 170) * 0.8f, 0.5f, 22, 0.08f);
            MagnumParticleHandler.SpawnParticle(impactRing);

            // Large scarlet explosion
            for (int i = 0; i < 25; i++)
            {
                float angle = MathHelper.TwoPi * i / 25f;
                float speed = Main.rand.NextFloat(4f, 12f);
                Vector2 vel = angle.ToRotationVector2() * speed;

                Dust burst = Dust.NewDustPerfect(Projectile.Center, DustID.CrimsonTorch, vel, 100, default, 2.5f);
                burst.noGravity = true;
                burst.fadeIn = 1.5f;
            }

            // Gold accents
            for (int i = 0; i < 15; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(10f, 10f);
                Dust gold = Dust.NewDustPerfect(Projectile.Center, DustID.GoldFlame, vel, 100, default, 2.2f);
                gold.noGravity = true;
                gold.fadeIn = 1.3f;
            }
        }

        public override void OnKill(int timeLeft)
        {
            // Triumphant finale  Eheroic flash + scattered sparks + fading sakura
            EroicaVFXLibrary.DeathHeroicFlash(Projectile.Center, 1.3f);
            EroicaVFXLibrary.SpawnSakuraPetals(Projectile.Center, 6, 35f);

            // Residual ember scatter
            for (int i = 0; i < 10; i++)
            {
                Color col = Color.Lerp(EroicaPalette.Crimson, EroicaPalette.OrangeGold, Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.CrimsonTorch,
                    Main.rand.NextVector2Circular(4f, 4f), 100, col, Main.rand.NextFloat(1.2f, 1.8f));
                d.noGravity = true;
            }

            SoundEngine.PlaySound(SoundID.Item14 with { Pitch = 0.2f, Volume = 0.7f }, Projectile.Center);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            // All visuals done with particles
            return false;
        }
    }
}
