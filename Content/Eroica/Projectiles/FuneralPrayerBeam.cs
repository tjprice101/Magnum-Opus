using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using MagnumOpus.Content.MoonlightSonata.Debuffs;
using MagnumOpus.Content.Eroica.Weapons.FuneralPrayer;
using MagnumOpus.Common.Systems.Particles;
using System;

namespace MagnumOpus.Content.Eroica.Projectiles
{
    /// <summary>
    /// Tracking electric beam that seeks enemies and applies Musical Dissonance.
    /// All VFX delegated to FuneralPrayerVFX module.
    /// </summary>
    public class FuneralPrayerBeam : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/LightningStreak";

        private int targetNPC = -1;
        private Vector2 beamEnd;
        private float beamLength = 0f;
        private const float MaxBeamLength = 200f;
        private bool hasReachedEnd = false;
        private bool hasHitEnemy = false;
        private int shotId = -1;
        private int beamIndex = -1;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 20;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 40;
            Projectile.alpha = 255;
            Projectile.light = 0f; // Lighting handled by VFX module
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.extraUpdates = 2;
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];

            if (shotId == -1)
            {
                shotId = (int)Projectile.ai[0];
                beamIndex = Projectile.whoAmI;
            }

            beamEnd = Projectile.Center + Projectile.velocity.SafeNormalize(Vector2.UnitX) * MaxBeamLength;
            beamLength = MaxBeamLength;

            if (Projectile.timeLeft <= 20 && !hasReachedEnd)
            {
                hasReachedEnd = true;
                FindAndArcToEnemy();
            }

            Projectile.rotation = Projectile.velocity.ToRotation();

            // ═══ ALL VFX delegated to module ═══
            FuneralPrayerVFX.BeamTrailFrame(Projectile);
        }

        private void FindAndArcToEnemy()
        {
            int arcTarget = -1;
            float minDistance = 300f;
            bool foundBoss = false;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && !npc.friendly && npc.boss && !npc.dontTakeDamage)
                {
                    float distance = Vector2.Distance(beamEnd, npc.Center);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        arcTarget = i;
                        foundBoss = true;
                    }
                }
            }

            if (!foundBoss)
            {
                minDistance = 300f;
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (npc.active && !npc.friendly && npc.lifeMax > 5 && !npc.dontTakeDamage)
                    {
                        float distance = Vector2.Distance(beamEnd, npc.Center);
                        if (distance < minDistance)
                        {
                            minDistance = distance;
                            arcTarget = i;
                        }
                    }
                }
            }

            if (arcTarget >= 0 && Main.npc[arcTarget].active)
            {
                NPC target = Main.npc[arcTarget];
                targetNPC = arcTarget;

                // Visual arc from beam end to target
                CreateArcVisual(beamEnd, target.Center);

                // Game logic: damage + debuff
                target.SimpleStrikeNPC(Projectile.damage, 0, false, 0f, null, false, 0f, true);
                target.AddBuff(ModContent.BuffType<MusicsDissonance>(), 240);

                // Shock particles
                for (int i = 0; i < 15; i++)
                {
                    Dust shock = Dust.NewDustDirect(target.position, target.width, target.height,
                        DustID.RedTorch, 0f, 0f, 100, default, 2.5f);
                    shock.noGravity = true;
                    shock.velocity = Main.rand.NextVector2Circular(4f, 4f);
                }

                for (int i = 0; i < 10; i++)
                {
                    Dust energy = Dust.NewDustDirect(target.position, target.width, target.height,
                        DustID.PinkTorch, 0f, 0f, 100, default, 1.8f);
                    energy.noGravity = true;
                    energy.velocity = Main.rand.NextVector2Circular(3f, 3f);
                }

                SoundEngine.PlaySound(SoundID.DD2_LightningBugZap, target.position);

                // Musical chord burst at impact
                EroicaVFXLibrary.MusicNoteBurst(target.Center, new Color(255, 215, 0), 5, 3.5f);

                // Register hit
                if (!hasHitEnemy && shotId >= 0)
                {
                    hasHitEnemy = true;
                    FuneralPrayer.RegisterBeamHit(shotId, beamIndex);
                }

                CreateSecondaryArc(target);
            }
        }

        /// <summary>
        /// Draws zigzag lightning arc between two points using dust + bloom ring.
        /// Replaces old MagnumVFX.DrawFuneralLightning.
        /// </summary>
        private void CreateArcVisual(Vector2 start, Vector2 end)
        {
            Vector2 direction = (end - start).SafeNormalize(Vector2.UnitX);
            Vector2 perpendicular = new Vector2(-direction.Y, direction.X);
            int segments = 10;

            for (int i = 1; i <= segments; i++)
            {
                float progress = i / (float)segments;
                Vector2 basePos = Vector2.Lerp(start, end, progress);
                float zigzag = (i % 2 == 0 ? 1f : -1f) * Main.rand.NextFloat(20f, 35f) * (1f - progress * 0.5f);
                Vector2 pos = basePos + perpendicular * zigzag;

                Color col = Color.Lerp(EroicaPalette.Crimson, EroicaPalette.OrangeGold, progress);
                Dust d = Dust.NewDustPerfect(pos, DustID.RedTorch,
                    Main.rand.NextVector2Circular(0.5f, 0.5f), 0, col, 1.8f);
                d.noGravity = true;

                // Pink highlight nodes at alternating segments
                if (i % 3 == 0)
                {
                    Dust pk = Dust.NewDustPerfect(pos, DustID.PinkTorch,
                        Main.rand.NextVector2Circular(0.3f, 0.3f), 0, default, 1.3f);
                    pk.noGravity = true;
                }
            }

            // Impact bloom + shockwave ring at endpoint
            EroicaVFXLibrary.HeroicImpact(end, 1.0f);
            var ring = new BloomRingParticle(end, Vector2.Zero,
                new Color(255, 80, 120) * 0.7f, 0.35f, 20, 0.06f);
            MagnumParticleHandler.SpawnParticle(ring);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Apply debuff
            target.AddBuff(ModContent.BuffType<MusicsDissonance>(), 240);

            // Delegate hit VFX to module — halo, dust burst, bloom
            FuneralPrayerVFX.BeamHitVFX(target.Center);

            // Music notes at impact
            EroicaVFXLibrary.SpawnMusicNotes(target.Center, 3, 25f);

            // Shock dust
            for (int i = 0; i < 15; i++)
            {
                Dust shock = Dust.NewDustDirect(target.position, target.width, target.height,
                    DustID.RedTorch, 0f, 0f, 100, default, 2.5f);
                shock.noGravity = true;
                shock.velocity = Main.rand.NextVector2Circular(4f, 4f);
            }

            for (int i = 0; i < 10; i++)
            {
                Dust energy = Dust.NewDustDirect(target.position, target.width, target.height,
                    DustID.PinkTorch, 0f, 0f, 100, default, 1.8f);
                energy.noGravity = true;
                energy.velocity = Main.rand.NextVector2Circular(3f, 3f);
            }

            SoundEngine.PlaySound(SoundID.DD2_LightningBugZap, Projectile.position);

            CreateSecondaryArc(target);
        }

        private void CreateSecondaryArc(NPC hitTarget)
        {
            int secondaryTarget = -1;
            float minDistance = 300f;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (i == hitTarget.whoAmI) continue;

                if (npc.active && !npc.friendly && npc.lifeMax > 5)
                {
                    float distance = Vector2.Distance(hitTarget.Center, npc.Center);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        secondaryTarget = i;
                    }
                }
            }

            if (secondaryTarget >= 0 && Main.npc[secondaryTarget].active)
            {
                NPC secondary = Main.npc[secondaryTarget];

                // Zigzag lightning arc — smaller, dimmer
                Vector2 startPos = hitTarget.Center;
                Vector2 endPos = secondary.Center;
                Vector2 direction = (endPos - startPos).SafeNormalize(Vector2.UnitX);
                Vector2 perpendicular = new Vector2(-direction.Y, direction.X);

                for (int seg = 1; seg <= 6; seg++)
                {
                    float progress = seg / 6f;
                    Vector2 basePos = Vector2.Lerp(startPos, endPos, progress);
                    float zigzag = (seg % 2 == 0 ? 1f : -1f) * Main.rand.NextFloat(12f, 25f) * (1f - progress * 0.4f);
                    Vector2 pos = basePos + perpendicular * zigzag;

                    Color col = Color.Lerp(EroicaPalette.Crimson, EroicaPalette.OrangeGold, progress);
                    Dust d = Dust.NewDustPerfect(pos, DustID.RedTorch,
                        Main.rand.NextVector2Circular(0.3f, 0.3f), 0, col, 1.4f);
                    d.noGravity = true;
                }

                // Shockwave ring at secondary target
                var ring = new BloomRingParticle(endPos, Vector2.Zero,
                    new Color(255, 80, 120) * 0.5f, 0.25f, 14, 0.05f);
                MagnumParticleHandler.SpawnParticle(ring);

                // Deal 50% damage + debuff
                int secondaryDamage = (int)(Projectile.damage * 0.5f);
                secondary.SimpleStrikeNPC(secondaryDamage, 0, false, 0f, null, false, 0f, true);
                secondary.AddBuff(ModContent.BuffType<MusicsDissonance>(), 240);

                // Impact particles
                for (int i = 0; i < 10; i++)
                {
                    Dust impact = Dust.NewDustDirect(secondary.position, secondary.width, secondary.height,
                        DustID.RedTorch, 0f, 0f, 100, default, 2.0f);
                    impact.noGravity = true;
                    impact.velocity = Main.rand.NextVector2Circular(3f, 3f);
                }

                for (int i = 0; i < 5; i++)
                {
                    Dust pink = Dust.NewDustDirect(secondary.position, secondary.width, secondary.height,
                        DustID.PinkTorch, 0f, 0f, 100, default, 1.5f);
                    pink.noGravity = true;
                    pink.velocity = Main.rand.NextVector2Circular(2f, 2f);
                }

                SoundEngine.PlaySound(SoundID.DD2_LightningAuraZap with { Volume = 0.5f, Pitch = 0.3f }, secondary.position);
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            // Delegate rendering to VFX module
            return FuneralPrayerVFX.DrawBeamProjectile(Main.spriteBatch, Projectile, ref lightColor);
        }
    }
}
