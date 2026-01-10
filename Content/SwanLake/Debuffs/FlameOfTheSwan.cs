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
    /// The effect creates a mesmerizing dual-colored flame that dances between black and white.
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
    /// </summary>
    public class FlameOfTheSwanNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;

        public bool HasFlameOfTheSwan { get; set; } = false;
        private float flameTimer = 0f;
        private float haloRotation = 0f;
        private int haloParticleTimer = 0;

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
                // Moderate DoT - 8 damage per second
                if (npc.lifeRegen > 0)
                    npc.lifeRegen = 0;
                npc.lifeRegen -= 16; // 8 damage per second (lifeRegen is doubled)
                
                if (damage < 2)
                    damage = 2;
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
                ThemedParticles.SwanLakeHalo(npc.Center, haloRadius, 8);
            }

            // === BLACK AND WHITE FLAME PARTICLES ===
            if (Main.rand.NextBool(2))
            {
                // Alternating black and white flames rising from the NPC
                bool isBlackFlame = Main.rand.NextBool();
                
                Vector2 flamePos = npc.Center + new Vector2(
                    Main.rand.NextFloat(-npc.width * 0.5f, npc.width * 0.5f),
                    Main.rand.NextFloat(-npc.height * 0.3f, npc.height * 0.5f)
                );

                if (isBlackFlame)
                {
                    // Black flame - dark smoke rising
                    Dust blackFlame = Dust.NewDustPerfect(
                        flamePos,
                        DustID.Smoke,
                        new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), Main.rand.NextFloat(-3f, -1.5f)),
                        200,
                        Color.Black,
                        Main.rand.NextFloat(1.3f, 2f)
                    );
                    blackFlame.noGravity = true;
                    blackFlame.fadeIn = 1.2f;
                }
                else
                {
                    // White flame - bright torch rising
                    Dust whiteFlame = Dust.NewDustPerfect(
                        flamePos,
                        DustID.WhiteTorch,
                        new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), Main.rand.NextFloat(-2.5f, -1f)),
                        50,
                        default,
                        Main.rand.NextFloat(1.2f, 1.8f)
                    );
                    whiteFlame.noGravity = true;
                    whiteFlame.fadeIn = 1f;
                }
            }

            // === HALO EFFECT - Ring of alternating black and white flames ===
            if (Main.rand.NextBool(2))
            {
                // Create halo particles orbiting the NPC using custom particles
                int haloParticles = 12;
                for (int i = 0; i < haloParticles; i++)
                {
                    if (!Main.rand.NextBool(3)) continue; // Sparse for performance

                    float angle = haloRotation + (MathHelper.TwoPi * i / haloParticles);
                    float haloRadius = Math.Max(npc.width, npc.height) * 0.7f + 15f;
                    
                    // Pulsing radius
                    float pulse = (float)Math.Sin(flameTimer + i) * 5f;
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
                            new Vector2(0, -0.5f),
                            220,
                            Color.Black,
                            1.8f
                        );
                        haloBlack.noGravity = true;
                    }
                    else
                    {
                        Dust haloWhite = Dust.NewDustPerfect(
                            haloPos,
                            DustID.WhiteTorch,
                            new Vector2(0, -0.3f),
                            40,
                            default,
                            1.5f
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
                // Create a flickering black/white overlay effect
                float flicker = (float)Math.Sin(Main.GameUpdateCount * 0.15f + npc.whoAmI * 0.5f);
                
                if (flicker > 0)
                {
                    // Brighten toward white
                    float intensity = flicker * 0.3f;
                    byte r = (byte)Math.Min(255, drawColor.R + (255 - drawColor.R) * intensity);
                    byte g = (byte)Math.Min(255, drawColor.G + (255 - drawColor.G) * intensity);
                    byte b = (byte)Math.Min(255, drawColor.B + (255 - drawColor.B) * intensity);
                    drawColor = new Color(r, g, b, drawColor.A);
                }
                else
                {
                    // Darken toward black
                    float intensity = -flicker * 0.4f;
                    byte r = (byte)(drawColor.R * (1f - intensity));
                    byte g = (byte)(drawColor.G * (1f - intensity));
                    byte b = (byte)(drawColor.B * (1f - intensity));
                    drawColor = new Color(r, g, b, drawColor.A);
                }
            }
        }
    }
}
