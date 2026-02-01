using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using MagnumOpus.Content.MoonlightSonata.Debuffs;
using MagnumOpus.Content.Eroica.ResonantWeapons;
using MagnumOpus.Common.Systems;
using System;

namespace MagnumOpus.Content.Eroica.Projectiles
{
    /// <summary>
    /// Tracking electric beam that seeks enemies and applies Musical Dissonance.
    /// </summary>
    public class FuneralPrayerBeam : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_0"; // Invisible - particle-based

        private int targetNPC = -1;
        private Vector2 beamEnd;
        private float beamLength = 0f;
        private const float MaxBeamLength = 200f; // Reduced by 50%
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
            Projectile.timeLeft = 40; // Short duration
            Projectile.alpha = 255;
            Projectile.light = 1.0f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.extraUpdates = 2;
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            
            // Initialize shotId and beamIndex from ai parameters
            if (shotId == -1)
            {
                shotId = (int)Projectile.ai[0];
                beamIndex = Projectile.whoAmI;
            }

            // Beam shoots straight out in initial direction
            // No tracking - just extends to max length
            beamEnd = Projectile.Center + Projectile.velocity.SafeNormalize(Vector2.UnitX) * MaxBeamLength;
            beamLength = MaxBeamLength;

            // Check if we've traveled far enough to reach the end
            if (Projectile.timeLeft <= 20 && !hasReachedEnd)
            {
                hasReachedEnd = true;
                // From the beam end, find and arc to nearby enemies
                FindAndArcToEnemy();
            }

            // Intense dark red with pink highlights lighting along beam
            Lighting.AddLight(Projectile.Center, 0.8f, 0.1f, 0.3f);

            // Dark red torch particles for visible beam
            if (Main.rand.NextBool(2))
            {
                Dust beam = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height,
                    DustID.RedTorch, 0f, 0f, 100, default, 2.0f);
                beam.noGravity = true;
                beam.velocity = Projectile.velocity * 0.2f;
            }

            // Pink highlight particles
            if (Main.rand.NextBool(3))
            {
                Dust energy = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height,
                    DustID.PinkTorch, 0f, 0f, 100, default, 1.5f);
                energy.noGravity = true;
                energy.velocity = Main.rand.NextVector2Circular(2f, 2f);
            }

            // Occasional electric spark overlay
            if (Main.rand.NextBool(5))
            {
                Dust spark = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height,
                    DustID.Torch, 0f, 0f, 100, new Color(180, 20, 60), 1.2f);
                spark.noGravity = true;
            }
            
            // ☁EMUSICAL NOTATION - Heroic melody trail
            if (Main.rand.NextBool(6))
            {
                Color noteColor = Color.Lerp(new Color(200, 50, 50), new Color(255, 215, 0), Main.rand.NextFloat());
                Vector2 noteVel = new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), -1f);
                ThemedParticles.MusicNote(Projectile.Center, noteVel, noteColor, 0.35f, 35);
            }

            Projectile.rotation = Projectile.velocity.ToRotation();
        }

        private void FindAndArcToEnemy()
        {
            // From the beam's end point, find nearest enemy
            int arcTarget = -1;
            float minDistance = 300f; // Arc range from beam end
            bool foundBoss = false;

            // First pass: look for bosses near beam end
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

            // Second pass: if no boss, target any enemy
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

            // If we found a target, create arc and deal damage
            if (arcTarget >= 0 && Main.npc[arcTarget].active)
            {
                NPC target = Main.npc[arcTarget];
                targetNPC = arcTarget;

                // Create visual arc from beam end to target
                CreateArcVisual(beamEnd, target.Center);

                // Deal damage to target
                target.SimpleStrikeNPC(Projectile.damage, 0, false, 0f, null, false, 0f, true);
                target.AddBuff(ModContent.BuffType<MusicsDissonance>(), 240);

                // Shock particles on hit
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

                // Sound effect
                SoundEngine.PlaySound(SoundID.DD2_LightningBugZap, target.position);
                
                // ☁EMUSICAL IMPACT - Triumphant chord burst
                ThemedParticles.MusicNoteBurst(target.Center, new Color(255, 215, 0), 5, 3.5f);

                // Register this beam hit
                if (!hasHitEnemy && shotId >= 0)
                {
                    hasHitEnemy = true;
                    FuneralPrayer.RegisterBeamHit(shotId, beamIndex);
                }

                // Then create secondary arc to another nearby enemy
                CreateSecondaryArc(target);
            }
        }

        private void CreateArcVisual(Vector2 start, Vector2 end)
        {
            // Draw Funeral-themed fractal lightning arc using the VFX system
            MagnumVFX.DrawFuneralLightning(start, end, 10, 35f, 3, 0.4f);
            
            // Additional pink/crimson spark burst at endpoints
            MagnumVFX.CreateEroicaBurst(end, 1);
            
            // Create shockwave ring at target
            MagnumVFX.CreateShockwaveRing(end, new Color(255, 80, 120), 30f, 3f, 20);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // This handles if beam directly hits an enemy during its travel
            // Apply Musical Dissonance debuff
            target.AddBuff(ModContent.BuffType<MusicsDissonance>(), 240); // 4 seconds

            // === SIGNATURE FRACTAL FLARE BURST ===
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f;
                Vector2 flareOffset = angle.ToRotationVector2() * 30f;
                float progress = (float)i / 6f;
                Color fractalColor = Color.Lerp(UnifiedVFX.Eroica.Scarlet, UnifiedVFX.Eroica.Gold, progress);
                CustomParticles.GenericFlare(target.Center + flareOffset, fractalColor, 0.45f, 18);
            }
            
            // Music notes on hit
            ThemedParticles.EroicaMusicNotes(target.Center, 3, 25f);

            // Shock explosion particles
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

            // Create secondary arc to another nearby enemy
            CreateSecondaryArc(target);
        }

        private void CreateSecondaryArc(NPC hitTarget)
        {
            // Find another nearby enemy to arc to
            int secondaryTarget = -1;
            float minDistance = 300f; // Secondary arc range

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (i == hitTarget.whoAmI) continue; // Skip the target we just hit
                
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

            // If we found a secondary target, create visual arc and deal damage
            if (secondaryTarget >= 0 && Main.npc[secondaryTarget].active)
            {
                NPC secondary = Main.npc[secondaryTarget];
                
                // Create visual arc between first and second target using fractal lightning
                Vector2 startPos = hitTarget.Center;
                Vector2 endPos = secondary.Center;
                
                // Draw Funeral-themed fractal lightning for secondary arc (smaller, dimmer)
                MagnumVFX.DrawFuneralLightning(startPos, endPos, 6, 25f, 2, 0.3f);
                MagnumVFX.CreateShockwaveRing(endPos, new Color(255, 80, 120), 20f, 2f, 14);

                // Deal 50% of beam damage to secondary target
                int secondaryDamage = (int)(Projectile.damage * 0.5f);
                secondary.SimpleStrikeNPC(secondaryDamage, 0, false, 0f, null, false, 0f, true);
                
                // Apply debuff to secondary target
                secondary.AddBuff(ModContent.BuffType<MusicsDissonance>(), 240);

                // Impact particles on secondary target
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

                // Sound for secondary arc
                SoundEngine.PlaySound(SoundID.DD2_LightningAuraZap with { Volume = 0.5f, Pitch = 0.3f }, secondary.position);
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            // Draw electric beam effect using dark red and pink particles
            Vector2 beamDirection = Projectile.velocity.SafeNormalize(Vector2.UnitX);
            float currentLength = Math.Min(beamLength, MaxBeamLength);
            
            // Create particle trail along beam path
            for (float distance = 0; distance < currentLength; distance += 8f)
            {
                Vector2 position = Projectile.Center + beamDirection * distance;
                
                // Dark red beam core
                Dust beam = Dust.NewDustPerfect(position, DustID.RedTorch, Vector2.Zero, 0, default, 1.8f);
                beam.noGravity = true;
                beam.velocity = Vector2.Zero;

                // Pink highlights - more frequent
                if (Main.rand.NextBool(2))
                {
                    Dust highlight = Dust.NewDustPerfect(position, DustID.PinkTorch, Vector2.Zero, 0, default, 1.3f);
                    highlight.noGravity = true;
                    highlight.velocity = Vector2.Zero;
                }
            }

            return false;
        }
    }
}
