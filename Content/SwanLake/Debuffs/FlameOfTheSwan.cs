using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems;

namespace MagnumOpus.Content.SwanLake.Debuffs
{
    /// <summary>
    /// Flame of the Swan - A unique debuff that creates a distinct black and white flaming halo effect.
    /// Enemies affected take 10% more damage from all damage types.
    /// Deals 0.1% of the enemy's current health as damage per tick.
    /// The effect creates a mesmerizing dual-colored flame that dances between black and white.
    /// Turns enemy sprites to grayscale with dazzling black and white particles.
    /// Note: When applying this debuff, use 1.25x the intended duration for 25% longer effect.
    /// </summary>
    public class FlameOfTheSwan : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = true;
            BuffID.Sets.NurseCannotRemoveDebuff[Type] = true;
        }

        public override void Update(NPC npc, ref int buffIndex)
        {
            npc.GetGlobalNPC<FlameOfTheSwanNPC>().HasFlameOfTheSwan = true;
        }
    }

    /// <summary>
    /// Handles the Flame of the Swan debuff effects on NPCs.
    /// Creates stunning black and white flame visuals with a halo effect.
    /// Renders enemy in grayscale with large dazzling black/white particles.
    /// </summary>
    public class FlameOfTheSwanNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;

        public bool HasFlameOfTheSwan { get; set; } = false;
        private float flameTimer = 0f;
        private float haloRotation = 0f;
        private int haloParticleTimer = 0;
        private int damageTick = 0;

        public override void ResetEffects(NPC npc)
        {
            HasFlameOfTheSwan = false;
        }

        public override void ModifyIncomingHit(NPC npc, ref NPC.HitModifiers modifiers)
        {
            if (HasFlameOfTheSwan)
            {
                // 10% more vulnerable to all damage types
                modifiers.FinalDamage *= 1.10f;
            }
        }

        public override void UpdateLifeRegen(NPC npc, ref int damage)
        {
            if (HasFlameOfTheSwan)
            {
                // Deal 0.1% of current health as damage per tick
                // lifeRegen is per second * 2, so we calculate appropriately
                // For 0.1% per tick at 60 ticks/sec, we need to deal damage differently
                
                if (npc.lifeRegen > 0)
                    npc.lifeRegen = 0;
                
                // Calculate 0.1% of current health - minimum of 2 damage
                int percentDamage = Math.Max(2, (int)(npc.life * 0.001f));
                
                // lifeRegen is doubled for actual damage, so set appropriately
                // This will tick every update, dealing percent-based damage
                npc.lifeRegen -= percentDamage * 2;
                
                if (damage < percentDamage)
                    damage = percentDamage;
            }
        }

        public override void AI(NPC npc)
        {
            if (!HasFlameOfTheSwan) return;

            flameTimer += 0.15f;
            haloRotation += 0.05f;
            haloParticleTimer++;

            // === CUSTOM PARTICLE HALO EFFECT (very visible) ===
            if (haloParticleTimer % 6 == 0)
            {
                // Large visible halo ring using ThemedParticles
                float haloRadius = Math.Max(npc.width, npc.height) * 0.8f + 20f;
                ThemedParticles.SwanLakeHalo(npc.Center, haloRadius, 10);
            }

            // === LARGE DAZZLING BLACK AND WHITE FLAME PARTICLES ===
            // These follow the enemy and are very visible
            if (Main.rand.NextBool(2))
            {
                // Large alternating black and white flames rising from the NPC
                bool isBlackFlame = Main.rand.NextBool();
                
                Vector2 flamePos = npc.Center + new Vector2(
                    Main.rand.NextFloat(-npc.width * 0.6f, npc.width * 0.6f),
                    Main.rand.NextFloat(-npc.height * 0.4f, npc.height * 0.5f)
                );

                if (isBlackFlame)
                {
                    // Large black flame - dark smoke rising
                    Dust blackFlame = Dust.NewDustPerfect(
                        flamePos,
                        DustID.Smoke,
                        new Vector2(Main.rand.NextFloat(-0.8f, 0.8f), Main.rand.NextFloat(-4f, -2f)),
                        180,
                        Color.Black,
                        Main.rand.NextFloat(2.5f, 3.5f) // Much larger!
                    );
                    blackFlame.noGravity = true;
                    blackFlame.fadeIn = 1.5f;
                }
                else
                {
                    // Large white flame - bright dazzling torch rising
                    Dust whiteFlame = Dust.NewDustPerfect(
                        flamePos,
                        DustID.WhiteTorch,
                        new Vector2(Main.rand.NextFloat(-0.8f, 0.8f), Main.rand.NextFloat(-3.5f, -1.5f)),
                        30,
                        Color.White,
                        Main.rand.NextFloat(2.2f, 3f) // Much larger!
                    );
                    whiteFlame.noGravity = true;
                    whiteFlame.fadeIn = 1.3f;
                }
            }
            
            // === EXTRA LARGE DAZZLING PARTICLES - follow the enemy ===
            if (Main.rand.NextBool(3))
            {
                // Spawn large dazzling particles that trail behind enemy
                Vector2 trailPos = npc.Center - npc.velocity * Main.rand.NextFloat(0.5f, 2f);
                trailPos += Main.rand.NextVector2Circular(npc.width * 0.3f, npc.height * 0.3f);
                
                bool isBlack = Main.rand.NextBool();
                int dustType = isBlack ? DustID.Smoke : DustID.WhiteTorch;
                Color dustColor = isBlack ? Color.Black : Color.White;
                int alpha = isBlack ? 150 : 20;
                
                Dust dazzle = Dust.NewDustPerfect(
                    trailPos,
                    dustType,
                    -npc.velocity * 0.1f + new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-2f, 0f)),
                    alpha,
                    dustColor,
                    Main.rand.NextFloat(2.8f, 4f) // Very large dazzling particles!
                );
                dazzle.noGravity = true;
                dazzle.fadeIn = 1.8f;
            }

            // === HALO EFFECT - Ring of alternating black and white flames ===
            if (Main.rand.NextBool(2))
            {
                // Create halo particles orbiting the NPC using custom particles
                int haloParticles = 14;
                for (int i = 0; i < haloParticles; i++)
                {
                    if (!Main.rand.NextBool(3)) continue; // Sparse for performance

                    float angle = haloRotation + (MathHelper.TwoPi * i / haloParticles);
                    float haloRadius = Math.Max(npc.width, npc.height) * 0.8f + 20f;
                    
                    // Pulsing radius
                    float pulse = (float)Math.Sin(flameTimer + i) * 6f;
                    haloRadius += pulse;

                    Vector2 haloPos = npc.Center + new Vector2(
                        (float)Math.Cos(angle) * haloRadius,
                        (float)Math.Sin(angle) * haloRadius * 0.5f // Elliptical for 3D effect
                    );

                    // Alternate black and white in the halo with larger, more visible particles
                    if (i % 2 == 0)
                    {
                        Dust haloBlack = Dust.NewDustPerfect(
                            haloPos,
                            DustID.Smoke,
                            new Vector2(0, -0.7f),
                            200,
                            Color.Black,
                            2.5f // Larger!
                        );
                        haloBlack.noGravity = true;
                    }
                    else
                    {
                        Dust haloWhite = Dust.NewDustPerfect(
                            haloPos,
                            DustID.WhiteTorch,
                            new Vector2(0, -0.5f),
                            30,
                            Color.White,
                            2.2f // Larger!
                        );
                        haloWhite.noGravity = true;
                    }
                }
            }

            // === CUSTOM PARTICLE SPARKLES (very visible) ===
            if (Main.rand.NextBool(4))
            {
                ThemedParticles.SwanLakeSparkles(npc.Center, 2, npc.width * 0.6f);
            }

            // === PEARLESCENT SHIMMER ACCENTS ===
            if (Main.rand.NextBool(4))
            {
                Color pearlescent = Main.rand.Next(3) switch
                {
                    0 => new Color(255, 240, 245), // Pink tint
                    1 => new Color(240, 245, 255), // Blue tint
                    _ => new Color(250, 255, 245)  // Green tint
                };

                Vector2 shimmerPos = npc.Center + Main.rand.NextVector2Circular(npc.width * 0.6f, npc.height * 0.6f);
                Dust shimmer = Dust.NewDustPerfect(
                    shimmerPos,
                    DustID.TintableDustLighted,
                    Main.rand.NextVector2Circular(1f, 1f),
                    0,
                    pearlescent,
                    1.2f
                );
                shimmer.noGravity = true;
            }

            // === SWIRLING VORTEX EFFECT AT CENTER ===
            if (Main.rand.NextBool(4))
            {
                float vortexAngle = flameTimer * 2f + Main.rand.NextFloat(MathHelper.TwoPi);
                float vortexDist = Main.rand.NextFloat(5f, 15f);
                Vector2 vortexPos = npc.Center + new Vector2(
                    (float)Math.Cos(vortexAngle) * vortexDist,
                    (float)Math.Sin(vortexAngle) * vortexDist
                );

                // Spiral inward velocity
                Vector2 vortexVel = (npc.Center - vortexPos).SafeNormalize(Vector2.Zero) * 0.5f;
                vortexVel = vortexVel.RotatedBy(MathHelper.PiOver2);

                int vortexType = Main.rand.NextBool() ? DustID.Smoke : DustID.WhiteTorch;
                Color vortexColor = vortexType == DustID.Smoke ? Color.Black : default;
                int alpha = vortexType == DustID.Smoke ? 180 : 40;

                Dust vortex = Dust.NewDustPerfect(vortexPos, vortexType, vortexVel, alpha, vortexColor, 1.0f);
                vortex.noGravity = true;
            }
            
            // === LIGHT EFFECT ===
            Lighting.AddLight(npc.Center, 0.4f, 0.4f, 0.45f);
        }

        public override void DrawEffects(NPC npc, ref Color drawColor)
        {
            if (HasFlameOfTheSwan)
            {
                // Convert enemy sprite to grayscale with slight black/white flicker
                // Calculate luminance (grayscale value) using standard conversion
                float luminance = (0.299f * drawColor.R + 0.587f * drawColor.G + 0.114f * drawColor.B);
                
                // Add subtle flicker between darker and lighter grayscale
                float flicker = (float)Math.Sin(Main.GameUpdateCount * 0.12f + npc.whoAmI * 0.5f);
                float flickerIntensity = 0.15f;
                
                // Adjust luminance based on flicker - shifts between darker and brighter grayscale
                luminance = MathHelper.Clamp(luminance + flicker * 50f * flickerIntensity, 0, 255);
                
                // Set the grayscale color
                byte gray = (byte)luminance;
                drawColor = new Color(gray, gray, gray, drawColor.A);
            }
        }
    }
}
