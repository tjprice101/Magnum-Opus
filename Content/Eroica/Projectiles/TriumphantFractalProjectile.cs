using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using Terraria.GameContent;
using MagnumOpus.Common.Systems;

namespace MagnumOpus.Content.Eroica.Projectiles
{
    /// <summary>
    /// Triumphant Fractal projectile with massive explosion and fractal lightning effects.
    /// Creates the signature fractal lightning sparks like Moon Lord summon weapons.
    /// </summary>
    public class TriumphantFractalProjectile : ModProjectile
    {
        private float lightningTimer = 0f;
        private List<(Vector2 start, Vector2 end, float time)> activeLightning = new List<(Vector2, Vector2, float)>();

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 15;
            ProjectileID.Sets.TrailingMode[Type] = 2;
            Main.projFrames[Type] = 1;
        }

        public override void SetDefaults()
        {
            Projectile.width = 32;
            Projectile.height = 32;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 180;
            Projectile.alpha = 0;
            Projectile.light = 1f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
            lightningTimer += 1f;

            // HOMING: Track nearby enemies
            float homingRange = 400f;
            float homingStrength = 0.045f; // Moderate tracking
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
                Vector2 toTarget = closestNPC.Center - Projectile.Center;
                toTarget.Normalize();
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, toTarget * Projectile.velocity.Length(), homingStrength);
            }

            // Spawn fractal lightning bolts around the projectile periodically
            if (lightningTimer % 8 == 0)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 lightningEnd = Projectile.Center + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * Main.rand.NextFloat(40f, 80f);
                activeLightning.Add((Projectile.Center, lightningEnd, 0f));
            }

            // Update and remove old lightning
            activeLightning.RemoveAll(l => l.time > 10f);
            for (int i = 0; i < activeLightning.Count; i++)
            {
                var l = activeLightning[i];
                activeLightning[i] = (l.start, l.end, l.time + 1f);
            }

            // Intense crimson flame trail with pulsing using new particle system
            float pulse = 1f + (float)Math.Sin(lightningTimer * 0.3f) * 0.3f;
            ThemedParticles.EroicaTrail(Projectile.Center, Projectile.velocity);
            
            // Enhanced custom particle trail
            CustomParticles.EroicaTrail(Projectile.Center, Projectile.velocity, 0.35f);
            
            for (int i = 0; i < 2; i++)
            {
                Dust flame = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height,
                    DustID.CrimsonTorch, 0f, 0f, 100, default, 2f * pulse);
                flame.noGravity = true;
                flame.velocity = Projectile.velocity * 0.3f + Main.rand.NextVector2Circular(1f, 1f);
            }

            // Gold/heroic sparkles
            if (Main.rand.NextBool(2))
            {
                Dust gold = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height,
                    DustID.GoldCoin, 0f, 0f, 0, default, 1.2f);
                gold.noGravity = true;
                gold.velocity = -Projectile.velocity * 0.2f;
            }

            if (Main.rand.NextBool(3))
            {
                Dust smoke = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height,
                    DustID.Smoke, 0f, 0f, 100, Color.Black, 1.3f);
                smoke.noGravity = true;
                smoke.velocity *= 0.4f;
            }

            Lighting.AddLight(Projectile.Center, 1f, 0.4f, 0.3f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            CreateMassiveExplosion();
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            CreateMassiveExplosion();
            return true;
        }

        public override void OnKill(int timeLeft)
        {
            CreateMassiveExplosion();
        }

        private void CreateMassiveExplosion()
        {
            // Prevent duplicate explosions via flag
            if (Projectile.localAI[0] >= 1f) return;
            Projectile.localAI[0] = 1f;

            SoundEngine.PlaySound(SoundID.DD2_ExplosiveTrapExplode, Projectile.position);

            // OPTIMIZED TRIUMPHANT FRACTAL EXPLOSION
            // Use CustomParticleSystem for efficient GPU-accelerated visuals instead of massive dust counts
            
            // Layer 1: Core white-hot flash
            var coreFlare = CustomParticleSystem.GetParticle().Setup(CustomParticleSystem.EnergyFlares[6], Projectile.Center, Vector2.Zero,
                new Color(255, 255, 240), 2.2f, 25, 0.025f, true, true);
            CustomParticleSystem.SpawnParticle(coreFlare);
            
            // Layer 2: Expanding golden halo (single optimized halo)
            var outerHalo = CustomParticleSystem.GetParticle().Setup(CustomParticleSystem.GlowingHalos[4], Projectile.Center, Vector2.Zero,
                CustomParticleSystem.EroicaColors.Gold, 1.2f, 35, 0.018f, true, true).WithScaleVelocity(0.04f);
            CustomParticleSystem.SpawnParticle(outerHalo);
            
            // Layer 3: Optimized burst rays (reduced count for performance)
            CustomParticles.ExplosionBurst(Projectile.Center, new Color(255, 180, 80), 12, 10f);
            CustomParticles.ExplosionBurst(Projectile.Center, new Color(200, 40, 40), 10, 7f);

            // Single themed impact effect (handles multiple visuals internally)
            MagnumVFX.CreateEroicaSparkBurst(Projectile.Center, 8, 120f);
            ThemedParticles.EroicaImpact(Projectile.Center, 2f);
            
            // Warm glowing aftermath (single glow)
            CustomParticles.GenericGlow(Projectile.Center, new Color(255, 150, 50), 2.2f, 45);

            // OPTIMIZED crimson flames (reduced from 40 to 18)
            for (int i = 0; i < 18; i++)
            {
                Dust explosion = Dust.NewDustDirect(Projectile.position - new Vector2(20, 20),
                    Projectile.width + 40, Projectile.height + 40,
                    DustID.CrimsonTorch, 0f, 0f, 100, default, 2.5f);
                explosion.noGravity = true;
                explosion.velocity = Main.rand.NextVector2Circular(10f, 10f);
            }

            // Gold heroic sparks in a ring (reduced from 24 to 12)
            for (int i = 0; i < 12; i++)
            {
                float angle = MathHelper.TwoPi * i / 12f;
                Vector2 velocity = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 7f;
                Dust gold = Dust.NewDustPerfect(Projectile.Center, DustID.GoldCoin, velocity, 0, default, 1.5f);
                gold.noGravity = true;
            }

            // Optimized smoke (reduced from 40 to 12)
            for (int i = 0; i < 12; i++)
            {
                Dust smoke = Dust.NewDustDirect(Projectile.position - new Vector2(20, 20),
                    Projectile.width + 40, Projectile.height + 40,
                    DustID.Smoke, 0f, 0f, 100, Color.Black, 2.0f);
                smoke.noGravity = true;
                smoke.velocity = Main.rand.NextVector2Circular(8f, 8f);
            }

            // Fractal lightning bolts (reduced from 8 to 5 for performance)
            for (int i = 0; i < 5; i++)
            {
                float angle = MathHelper.TwoPi * i / 5f;
                Vector2 direction = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
                Vector2 lightningEnd = Projectile.Center + direction * Main.rand.NextFloat(80f, 140f);
                MagnumVFX.DrawFractalLightning(Projectile.Center, lightningEnd, new Color(255, 150, 100), 8, 28f, 1, 0.4f);
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            Vector2 origin = texture.Size() / 2f;

            // Switch to additive blending for glow effects
            MagnumVFX.BeginAdditiveBlend(spriteBatch);

            // Draw active lightning bolts around the projectile
            foreach (var lightning in activeLightning)
            {
                float alpha = 1f - (lightning.time / 10f);
                Color lightningColor = new Color(255, 180, 100) * alpha;
                
                // Draw mini lightning bolt
                DrawMiniLightning(lightning.start, lightning.end, lightningColor);
            }
            
            // Draw prismatic gem trail using oldPos for brilliant diamond effect
            MagnumVFX.DrawPrismaticGemTrail(spriteBatch, Projectile.oldPos, true, 0.5f, lightningTimer);

            // Draw enhanced glowing trail with color gradient
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;

                float progress = (float)i / Projectile.oldPos.Length;
                Vector2 drawPos = Projectile.oldPos[i] - Main.screenPosition + Projectile.Size / 2f;
                
                // Gradient from bright gold to deep crimson
                Color trailColor = Color.Lerp(new Color(255, 200, 100), new Color(200, 50, 30), progress);
                trailColor *= (1f - progress) * 0.9f;
                float trailScale = Projectile.scale * MathHelper.Lerp(1.2f, 0.4f, progress);

                spriteBatch.Draw(texture, drawPos, null, trailColor, Projectile.oldRot[i], origin,
                    trailScale, SpriteEffects.None, 0f);

                // Extra glow layer
                spriteBatch.Draw(texture, drawPos, null, trailColor * 0.5f, Projectile.oldRot[i], origin,
                    trailScale * 1.3f, SpriteEffects.None, 0f);
            }

            // Draw the main projectile with glow
            Vector2 mainDrawPos = Projectile.Center - Main.screenPosition;
            float pulseScale = 1f + (float)Math.Sin(lightningTimer * 0.2f) * 0.15f;
            
            // Draw central prismatic gem effect
            MagnumVFX.DrawEroicaPrismaticGem(spriteBatch, Projectile.Center, pulseScale * 1.2f, 1f, lightningTimer);
            
            // Outer glow
            spriteBatch.Draw(texture, mainDrawPos, null, new Color(255, 150, 80) * 0.6f, Projectile.rotation, origin,
                Projectile.scale * pulseScale * 2f, SpriteEffects.None, 0f);
            // Inner glow
            spriteBatch.Draw(texture, mainDrawPos, null, new Color(255, 220, 180) * 0.8f, Projectile.rotation, origin,
                Projectile.scale * pulseScale * 1.3f, SpriteEffects.None, 0f);

            MagnumVFX.EndAdditiveBlend(spriteBatch);

            return true;
        }

        private void DrawMiniLightning(Vector2 start, Vector2 end, Color color)
        {
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            int segments = 5;
            List<Vector2> points = new List<Vector2>();
            points.Add(start);

            Vector2 direction = end - start;
            Vector2 perpendicular = new Vector2(-direction.Y, direction.X);
            perpendicular.Normalize();

            for (int i = 1; i < segments; i++)
            {
                float progress = (float)i / segments;
                Vector2 basePos = Vector2.Lerp(start, end, progress);
                float offset = Main.rand.NextFloat(-15f, 15f) * (float)Math.Sin(progress * Math.PI);
                points.Add(basePos + perpendicular * offset);
            }
            points.Add(end);

            for (int i = 0; i < points.Count - 1; i++)
            {
                Vector2 segStart = points[i] - Main.screenPosition;
                Vector2 segEnd = points[i + 1] - Main.screenPosition;
                Vector2 segDir = segEnd - segStart;
                float length = segDir.Length();
                float rotation = segDir.ToRotation();

                // Glow
                Main.spriteBatch.Draw(pixel, segStart, new Rectangle(0, 0, 1, 1), color * 0.5f,
                    rotation, Vector2.Zero, new Vector2(length, 4f), SpriteEffects.None, 0f);
                // Core
                Main.spriteBatch.Draw(pixel, segStart, new Rectangle(0, 0, 1, 1), Color.White * (color.A / 255f),
                    rotation, Vector2.Zero, new Vector2(length, 1.5f), SpriteEffects.None, 0f);
            }
        }
    }
}
