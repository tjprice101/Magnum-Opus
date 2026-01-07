using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using MagnumOpus.Content.MoonlightSonata.Debuffs;
using MagnumOpus.Content.Eroica.ResonantWeapons;
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
                if (npc.active && !npc.friendly && npc.boss)
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
                    if (npc.active && !npc.friendly && npc.lifeMax > 5)
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
            float arcLength = Vector2.Distance(start, end);
            Vector2 direction = (end - start).SafeNormalize(Vector2.UnitX);

            // Draw particle arc
            for (float dist = 0; dist < arcLength; dist += 8f)
            {
                Vector2 arcPos = start + direction * dist;
                
                // Dark red particles
                Dust arc = Dust.NewDustPerfect(arcPos, DustID.RedTorch, Vector2.Zero, 100, default, 1.5f);
                arc.noGravity = true;
                
                // Pink highlights
                if (Main.rand.NextBool(2))
                {
                    Dust pink = Dust.NewDustPerfect(arcPos, DustID.PinkTorch, Vector2.Zero, 100, default, 1.2f);
                    pink.noGravity = true;
                }
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // This handles if beam directly hits an enemy during its travel
            // Apply Musical Dissonance debuff
            target.AddBuff(ModContent.BuffType<MusicsDissonance>(), 240); // 4 seconds

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
                
                // Create visual arc between first and second target
                Vector2 startPos = hitTarget.Center;
                Vector2 endPos = secondary.Center;
                float arcLength = Vector2.Distance(startPos, endPos);
                Vector2 direction = (endPos - startPos).SafeNormalize(Vector2.UnitX);

                // Draw particle arc between targets
                for (float dist = 0; dist < arcLength; dist += 8f)
                {
                    Vector2 arcPos = startPos + direction * dist;
                    
                    // Dark red particles
                    Dust arc = Dust.NewDustPerfect(arcPos, DustID.RedTorch, Vector2.Zero, 100, default, 1.5f);
                    arc.noGravity = true;
                    
                    // Pink highlights
                    if (Main.rand.NextBool(2))
                    {
                        Dust pink = Dust.NewDustPerfect(arcPos, DustID.PinkTorch, Vector2.Zero, 100, default, 1.2f);
                        pink.noGravity = true;
                    }
                }

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
