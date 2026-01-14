using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Common.Systems
{
    // ============================================================================
    // UNIFIED VFX SYSTEM - Single point of entry for ALL visual effects
    // Combines: ThemedParticles, CustomParticles, InfernumStyleParticles, VFXCombos
    // 
    // USAGE: UnifiedVFX.[Theme].[Effect](position, scale)
    // Example: UnifiedVFX.LaCampanella.Impact(position, 1.5f)
    //          UnifiedVFX.Eroica.DeathExplosion(position)
    //          UnifiedVFX.Generic.Explosion(position, color1, color2)
    // ============================================================================

    /// <summary>
    /// Master VFX system providing easy access to all themed and generic effects.
    /// </summary>
    public static class UnifiedVFX
    {
        // ============================================================================
        // LA CAMPANELLA THEME - Black → Orange infernal bell effects
        // ============================================================================
        public static class LaCampanella
        {
            public static readonly Color Black = new Color(20, 15, 20);
            public static readonly Color Orange = new Color(255, 100, 0);
            public static readonly Color Yellow = new Color(255, 200, 50);
            public static readonly Color Gold = new Color(218, 165, 32);
            public static readonly Color Red = new Color(200, 50, 30);

            /// <summary>Standard attack impact with bell chime particles.</summary>
            public static void Impact(Vector2 position, float scale = 1f)
            {
                // Infernum-style pulse ring
                SpawnPulseRing(position, Orange, 0f, 2.5f * scale, 30);
                SpawnPulseRing(position, Black * 0.8f, 0.5f * scale, 3f * scale, 40);
                
                // Strong bloom
                SpawnStrongBloom(position, Orange * 0.7f, 2f * scale, 25);
                
                // Themed particles
                ThemedParticles.LaCampanellaImpact(position, scale);
                
                // Gradient sparks Black → Orange
                for (int i = 0; i < (int)(20 * scale); i++)
                {
                    Vector2 velocity = Main.rand.NextVector2Unit() * Main.rand.NextFloat(5f, 15f) * scale;
                    float progress = (float)i / 20f;
                    Color sparkColor = Color.Lerp(Black, Orange, progress);
                    SpawnDirectionalSpark(position, velocity, sparkColor, 35, 1.5f * scale);
                }
                
                // Heavy smoke for infernal feel
                for (int i = 0; i < (int)(8 * scale); i++)
                {
                    Vector2 smokeVel = Main.rand.NextVector2Unit() * Main.rand.NextFloat(2f, 5f);
                    SpawnDenseSmoke(position, smokeVel, Black, 40, 1.5f * scale, 0.8f);
                }
                
                MagnumScreenEffects.AddScreenShake(4f * scale);
            }

            /// <summary>Heavy explosion for major attacks.</summary>
            public static void Explosion(Vector2 position, float scale = 1f)
            {
                // Multiple pulse rings with gradient
                for (int i = 0; i < 3; i++)
                {
                    float ringScale = (1f + i * 0.3f) * scale;
                    Color ringColor = Color.Lerp(Orange, Yellow, i / 3f);
                    SpawnPulseRing(position, ringColor, 0f, 3f * ringScale, 35 + i * 10);
                }
                
                // Black smoke ring
                for (int i = 0; i < 24; i++)
                {
                    float angle = MathHelper.TwoPi * i / 24f;
                    Vector2 velocity = angle.ToRotationVector2() * Main.rand.NextFloat(6f, 12f) * scale;
                    SpawnDenseSmoke(position, velocity, Black, 50, 2f * scale, 1f);
                }
                
                // Flare burst with gradient
                for (int i = 0; i < 12; i++)
                {
                    float angle = MathHelper.TwoPi * i / 12f;
                    Vector2 offset = angle.ToRotationVector2() * 40f * scale;
                    Color flareColor = Color.Lerp(Orange, Gold, (float)i / 12f);
                    CustomParticles.GenericFlare(position + offset, flareColor, 0.6f * scale, 20);
                }
                
                // Electric arcs
                for (int i = 0; i < 8; i++)
                {
                    Vector2 velocity = Main.rand.NextVector2Unit() * Main.rand.NextFloat(10f, 25f) * scale;
                    SpawnElectricArc(position, velocity, Orange, 0.8f, 40);
                }
                
                ThemedParticles.LaCampanellaShockwave(position, scale);
                ExplosionUtility.CreateFireExplosion(position, Orange, Black, scale);
                
                MagnumScreenEffects.AddScreenShake(10f * scale);
                MagnumScreenEffects.SetFlashEffect(position, 1.5f * scale, 25);
            }

            /// <summary>Bell chime effect with music notes and resonance.</summary>
            public static void BellChime(Vector2 position, float scale = 1f)
            {
                ThemedParticles.LaCampanellaBellChime(position, scale);
                ThemedParticles.LaCampanellaMusicNotes(position, (int)(6 * scale), 40f * scale);
                
                // Halo rings
                CustomParticles.HaloRing(position, Orange, 0.4f * scale, 20);
                CustomParticles.HaloRing(position, Gold, 0.3f * scale, 25);
                
                // Resonance sparkles
                for (int i = 0; i < 8; i++)
                {
                    float angle = MathHelper.TwoPi * i / 8f;
                    Vector2 offset = angle.ToRotationVector2() * 25f * scale;
                    CustomParticles.GenericFlare(position + offset, Yellow, 0.3f * scale, 15);
                }
            }

            /// <summary>Weapon swing aura with infernal flames.</summary>
            public static void SwingAura(Vector2 position, Vector2 direction, float scale = 1f)
            {
                ThemedParticles.LaCampanellaSwingAura(position, direction, scale);
                ThemedParticles.LaCampanellaSparks(position + direction * 40f, direction, 6, 6f * scale);
                
                // Gradient flares along swing arc
                for (int i = 0; i < 6; i++)
                {
                    float offsetAngle = direction.ToRotation() + MathHelper.PiOver4 * ((float)i / 6f - 0.5f) * 2f;
                    Vector2 offset = offsetAngle.ToRotationVector2() * Main.rand.NextFloat(30f, 50f) * scale;
                    Color flareColor = Color.Lerp(Black, Orange, (float)i / 6f);
                    CustomParticles.GenericFlare(position + offset, flareColor, 0.4f * scale, 12);
                }
            }

            /// <summary>Trail effect for projectiles.</summary>
            public static void Trail(Vector2 position, Vector2 velocity, float scale = 1f)
            {
                ThemedParticles.LaCampanellaTrail(position, velocity);
                VFXCombos.FireTrail(position, velocity, Orange, scale);
                
                // Add smoke trail
                if (Main.rand.NextBool(2))
                {
                    SpawnDenseSmoke(position, -velocity * 0.1f, Black, 25, 0.6f * scale, 0.5f);
                }
            }

            /// <summary>Boss death explosion - maximum spectacle.</summary>
            public static void DeathExplosion(Vector2 position, float scale = 1f)
            {
                ExplosionUtility.CreateDeathExplosion(position, Orange, Gold, scale * 1.5f);
                
                // Extra infernal effects
                for (int wave = 0; wave < 5; wave++)
                {
                    float waveScale = (1f + wave * 0.4f) * scale;
                    Color waveColor = Color.Lerp(Orange, Red, wave / 5f);
                    SpawnPulseRing(position, waveColor, 0f, 4f * waveScale, 40 + wave * 10);
                }
                
                // Massive smoke cloud
                for (int i = 0; i < 40; i++)
                {
                    Vector2 velocity = Main.rand.NextVector2Unit() * Main.rand.NextFloat(5f, 20f) * scale;
                    SpawnDenseSmoke(position, velocity, Black, 70, Main.rand.NextFloat(2f, 4f) * scale, 1f);
                }
                
                // Bell lightning storm
                for (int i = 0; i < 16; i++)
                {
                    Vector2 velocity = Main.rand.NextVector2Unit() * Main.rand.NextFloat(15f, 40f) * scale;
                    SpawnElectricArc(position, velocity, Orange, 1.5f, 60);
                }
                
                // Flare shine
                SpawnFlareShine(position, Orange, Gold, 0f, new Vector2(12f * scale), 60);
                
                MagnumScreenEffects.AddScreenShake(25f * scale);
                MagnumScreenEffects.SetFlashEffect(position, 2.5f * scale, 50);
            }

            /// <summary>Ambient aura for held items or buffs.</summary>
            public static void Aura(Vector2 position, float radius, float scale = 1f)
            {
                ThemedParticles.LaCampanellaAura(position, radius * scale);
                
                if (Main.rand.NextBool(4))
                {
                    Vector2 offset = Main.rand.NextVector2Circular(radius, radius) * scale;
                    CustomParticles.GenericFlare(position + offset, Orange * 0.6f, 0.25f * scale, 15);
                }
            }
        }

        // ============================================================================
        // EROICA THEME - Scarlet → Gold heroic/triumphant effects
        // ============================================================================
        public static class Eroica
        {
            public static readonly Color Scarlet = new Color(139, 0, 0);
            public static readonly Color Crimson = new Color(220, 50, 50);
            public static readonly Color Flame = new Color(255, 100, 50);
            public static readonly Color Gold = new Color(255, 215, 0);
            public static readonly Color Amber = new Color(255, 191, 100);
            public static readonly Color Black = new Color(30, 20, 25);
            public static readonly Color Sakura = new Color(255, 150, 180);

            /// <summary>Standard attack impact.</summary>
            public static void Impact(Vector2 position, float scale = 1f)
            {
                SpawnPulseRing(position, Crimson, 0f, 2.5f * scale, 30);
                SpawnStrongBloom(position, Gold * 0.6f, 1.8f * scale, 25);
                
                ThemedParticles.EroicaImpact(position, scale);
                
                // Gradient sparks Scarlet → Gold
                for (int i = 0; i < (int)(15 * scale); i++)
                {
                    Vector2 velocity = Main.rand.NextVector2Unit() * Main.rand.NextFloat(5f, 12f) * scale;
                    float progress = (float)i / 15f;
                    Color sparkColor = Color.Lerp(Scarlet, Gold, progress);
                    SpawnDirectionalSpark(position, velocity, sparkColor, 30, 1.3f * scale);
                }
                
                MagnumScreenEffects.AddScreenShake(3f * scale);
            }

            /// <summary>Heroic explosion with triumphant flair.</summary>
            public static void Explosion(Vector2 position, float scale = 1f)
            {
                // Crimson to gold pulse rings
                SpawnPulseRing(position, Crimson, 0f, 3f * scale, 35);
                SpawnPulseRing(position, Gold, 0.3f * scale, 3.5f * scale, 45);
                
                ThemedParticles.EroicaShockwave(position, scale);
                ExplosionUtility.CreateFireExplosion(position, Flame, Gold, scale);
                
                // Sakura petals in explosion
                ThemedParticles.SakuraPetals(position, (int)(12 * scale), 60f * scale);
                
                // Music notes burst
                ThemedParticles.EroicaMusicNotes(position, (int)(8 * scale), 50f * scale);
                
                MagnumScreenEffects.AddScreenShake(8f * scale);
                MagnumScreenEffects.SetFlashEffect(position, 1.2f * scale, 20);
            }

            /// <summary>Sword swing with sakura and fire.</summary>
            public static void SwingAura(Vector2 position, Vector2 direction, float scale = 1f)
            {
                ThemedParticles.EroicaSparks(position + direction * 40f, direction, 6, 5f * scale);
                ThemedParticles.SakuraPetals(position, 4, 30f * scale);
                
                // Gradient flares
                for (int i = 0; i < 5; i++)
                {
                    float offsetAngle = direction.ToRotation() + MathHelper.PiOver4 * ((float)i / 5f - 0.5f) * 2f;
                    Vector2 offset = offsetAngle.ToRotationVector2() * Main.rand.NextFloat(25f, 45f) * scale;
                    Color flareColor = Color.Lerp(Scarlet, Gold, (float)i / 5f);
                    CustomParticles.GenericFlare(position + offset, flareColor, 0.35f * scale, 12);
                }
            }

            /// <summary>Trail for projectiles.</summary>
            public static void Trail(Vector2 position, Vector2 velocity, float scale = 1f)
            {
                ThemedParticles.EroicaTrail(position, velocity);
                VFXCombos.FireTrail(position, velocity, Flame, scale * 0.8f);
                
                // Occasional sakura petal
                if (Main.rand.NextBool(5))
                    ThemedParticles.SakuraPetals(position, 1, 15f);
            }

            /// <summary>Boss phase transition.</summary>
            public static void PhaseTransition(Vector2 position, float scale = 1f)
            {
                ThemedParticles.EroicaMusicalImpact(position, scale * 2f, true);
                ThemedParticles.EroicaShockwave(position, scale * 2f);
                ThemedParticles.EroicaMusicNotes(position, 16, 80f * scale);
                
                // Dramatic pulse rings
                for (int i = 0; i < 4; i++)
                {
                    Color ringColor = Color.Lerp(Crimson, Gold, i / 4f);
                    SpawnPulseRing(position, ringColor, 0f, (3f + i) * scale, 40 + i * 8);
                }
                
                MagnumScreenEffects.AddScreenShake(15f * scale);
                MagnumScreenEffects.SetFlashEffect(position, 2f * scale, 35);
            }

            /// <summary>Boss death explosion.</summary>
            public static void DeathExplosion(Vector2 position, float scale = 1f)
            {
                ExplosionUtility.CreateDeathExplosion(position, Crimson, Gold, scale * 1.3f);
                
                // Massive sakura burst
                ThemedParticles.SakuraPetals(position, 30, 150f * scale);
                
                // Music staff finale
                ThemedParticles.EroicaMusicStaff(position, scale * 2f);
                ThemedParticles.EroicaMusicNotes(position, 24, 100f * scale);
                
                // Electric arcs in gold
                for (int i = 0; i < 12; i++)
                {
                    Vector2 velocity = Main.rand.NextVector2Unit() * Main.rand.NextFloat(15f, 35f) * scale;
                    SpawnElectricArc(position, velocity, Gold, 1.2f, 50);
                }
                
                SpawnFlareShine(position, Gold, Crimson, 0f, new Vector2(10f * scale), 55);
                
                MagnumScreenEffects.AddScreenShake(20f * scale);
                MagnumScreenEffects.SetFlashEffect(position, 2f * scale, 45);
            }

            /// <summary>Ambient aura with sakura.</summary>
            public static void Aura(Vector2 position, float radius, float scale = 1f)
            {
                ThemedParticles.EroicaAura(position, radius * scale);
                
                if (Main.rand.NextBool(6))
                    ThemedParticles.SakuraPetals(position, 2, radius * scale);
            }
        }

        // ============================================================================
        // MOONLIGHT SONATA THEME - Dark Purple → Light Blue lunar effects
        // ============================================================================
        public static class MoonlightSonata
        {
            public static readonly Color DarkPurple = new Color(75, 0, 130);
            public static readonly Color MediumPurple = new Color(138, 43, 226);
            public static readonly Color LightPurple = new Color(180, 150, 255);
            public static readonly Color LightBlue = new Color(135, 206, 250);
            public static readonly Color IceBlue = new Color(200, 230, 255);
            public static readonly Color Silver = new Color(220, 220, 235);

            /// <summary>Lunar impact with ethereal glow.</summary>
            public static void Impact(Vector2 position, float scale = 1f)
            {
                SpawnPulseRing(position, DarkPurple, 0f, 2.5f * scale, 35);
                SpawnPulseRing(position, LightBlue * 0.6f, 0.3f * scale, 3f * scale, 45);
                SpawnStrongBloom(position, MediumPurple * 0.5f, 1.5f * scale, 30);
                
                ThemedParticles.MoonlightImpact(position, scale);
                
                // Gradient sparks Purple → Blue
                for (int i = 0; i < (int)(12 * scale); i++)
                {
                    Vector2 velocity = Main.rand.NextVector2Unit() * Main.rand.NextFloat(4f, 10f) * scale;
                    float progress = (float)i / 12f;
                    Color sparkColor = Color.Lerp(DarkPurple, LightBlue, progress);
                    SpawnDirectionalSpark(position, velocity, sparkColor, 35, 1.2f * scale);
                }
                
                MagnumScreenEffects.AddScreenShake(2.5f * scale);
            }

            /// <summary>Mystical explosion with lunar energy.</summary>
            public static void Explosion(Vector2 position, float scale = 1f)
            {
                ThemedParticles.MoonlightShockwave(position, scale);
                
                // Layered pulse rings
                SpawnPulseRing(position, DarkPurple, 0f, 3f * scale, 40);
                SpawnPulseRing(position, LightBlue, 0.2f * scale, 3.5f * scale, 50);
                
                // Silver sparkles
                ThemedParticles.MoonlightSparkles(position, (int)(15 * scale), 50f * scale);
                
                ExplosionUtility.CreateEnergyExplosion(position, MediumPurple, scale);
                
                MagnumScreenEffects.AddScreenShake(7f * scale);
                MagnumScreenEffects.SetFlashEffect(position, 1f * scale, 25);
            }

            /// <summary>Ethereal weapon swing.</summary>
            public static void SwingAura(Vector2 position, Vector2 direction, float scale = 1f)
            {
                ThemedParticles.MoonlightSparks(position + direction * 35f, direction, 5, 5f * scale);
                ThemedParticles.MoonlightSparkles(position, 4, 25f * scale);
                
                // Gradient flares
                for (int i = 0; i < 4; i++)
                {
                    float offsetAngle = direction.ToRotation() + MathHelper.PiOver4 * ((float)i / 4f - 0.5f) * 2f;
                    Vector2 offset = offsetAngle.ToRotationVector2() * Main.rand.NextFloat(20f, 40f) * scale;
                    Color flareColor = Color.Lerp(DarkPurple, LightBlue, (float)i / 4f);
                    CustomParticles.GenericFlare(position + offset, flareColor, 0.3f * scale, 12);
                }
            }

            /// <summary>Trail for projectiles.</summary>
            public static void Trail(Vector2 position, Vector2 velocity, float scale = 1f)
            {
                ThemedParticles.MoonlightTrail(position, velocity);
                VFXCombos.ElectricTrail(position, velocity, MediumPurple, scale * 0.7f);
            }

            /// <summary>Boss death explosion.</summary>
            public static void DeathExplosion(Vector2 position, float scale = 1f)
            {
                ExplosionUtility.CreateDeathExplosion(position, DarkPurple, LightBlue, scale * 1.2f);
                ThemedParticles.MoonlightSparkles(position, 30, 120f * scale);
                ThemedParticles.MoonlightMusicNotes(position, 20, 80f * scale);
                
                // Ethereal lightning
                for (int i = 0; i < 10; i++)
                {
                    Vector2 velocity = Main.rand.NextVector2Unit() * Main.rand.NextFloat(12f, 30f) * scale;
                    SpawnElectricArc(position, velocity, LightBlue, 1f, 45);
                }
                
                SpawnFlareShine(position, Silver, DarkPurple, 0f, new Vector2(8f * scale), 50);
                
                MagnumScreenEffects.AddScreenShake(18f * scale);
                MagnumScreenEffects.SetFlashEffect(position, 1.8f * scale, 40);
            }

            /// <summary>Ambient lunar aura.</summary>
            public static void Aura(Vector2 position, float radius, float scale = 1f)
            {
                ThemedParticles.MoonlightAura(position, radius * scale);
                
                if (Main.rand.NextBool(5))
                    ThemedParticles.MoonlightSparkles(position, 2, radius * scale);
            }
        }

        // ============================================================================
        // SWAN LAKE THEME - White/Black with rainbow iridescence
        // ============================================================================
        public static class SwanLake
        {
            public static readonly Color White = new Color(255, 255, 255);
            public static readonly Color Black = new Color(20, 20, 30);
            public static readonly Color Silver = new Color(220, 225, 235);

            /// <summary>Get rainbow color based on time.</summary>
            public static Color GetRainbow(float offset = 0f)
            {
                return Main.hslToRgb((Main.GameUpdateCount * 0.01f + offset) % 1f, 0.9f, 0.7f);
            }

            /// <summary>Graceful impact with feathers.</summary>
            public static void Impact(Vector2 position, float scale = 1f)
            {
                // Dual black/white pulse rings
                SpawnPulseRing(position, White, 0f, 2.5f * scale, 30);
                SpawnPulseRing(position, Black, 0.2f * scale, 2.8f * scale, 35);
                
                // Rainbow shimmer
                for (int i = 0; i < 8; i++)
                {
                    float angle = MathHelper.TwoPi * i / 8f;
                    Vector2 offset = angle.ToRotationVector2() * 30f * scale;
                    Color rainbow = GetRainbow(i / 8f);
                    CustomParticles.GenericFlare(position + offset, rainbow * 0.7f, 0.35f * scale, 18);
                }
                
                ThemedParticles.SwanLakeImpact(position, scale);
                
                // Feathers
                ThemedParticles.SwanFeatherBurst(position, 6, 40f * scale);
                
                MagnumScreenEffects.AddScreenShake(3f * scale);
            }

            /// <summary>Elegant explosion with prismatic effects.</summary>
            public static void Explosion(Vector2 position, float scale = 1f)
            {
                // Alternating black/white rings
                for (int i = 0; i < 4; i++)
                {
                    Color ringColor = i % 2 == 0 ? White : Black;
                    SpawnPulseRing(position, ringColor, i * 0.2f * scale, (2.5f + i * 0.5f) * scale, 35 + i * 8);
                }
                
                ThemedParticles.SwanLakeShockwave(position, scale);
                
                // Prismatic burst
                CustomParticles.PrismaticSparkleRainbow(position, (int)(16 * scale));
                
                // Feathers everywhere
                ThemedParticles.SwanFeatherBurst(position, 15, 80f * scale);
                
                MagnumScreenEffects.AddScreenShake(9f * scale);
                MagnumScreenEffects.SetFlashEffect(position, 1.5f * scale, 25);
            }

            /// <summary>Graceful weapon swing.</summary>
            public static void SwingAura(Vector2 position, Vector2 direction, float scale = 1f)
            {
                ThemedParticles.SwanLakeSparks(position + direction * 35f, direction, 5, 5f * scale);
                ThemedParticles.SwanFeatherBurst(position, 3, 25f * scale);
                
                // Rainbow arc
                for (int i = 0; i < 5; i++)
                {
                    float offsetAngle = direction.ToRotation() + MathHelper.PiOver4 * ((float)i / 5f - 0.5f) * 2f;
                    Vector2 offset = offsetAngle.ToRotationVector2() * Main.rand.NextFloat(20f, 40f) * scale;
                    Color flareColor = GetRainbow(i / 5f);
                    CustomParticles.GenericFlare(position + offset, flareColor, 0.3f * scale, 12);
                }
            }

            /// <summary>Feather trail for projectiles.</summary>
            public static void Trail(Vector2 position, Vector2 velocity, float scale = 1f)
            {
                ThemedParticles.SwanLakeTrail(position, velocity);
                
                if (Main.rand.NextBool(4))
                    CustomParticles.SwanFeatherDrift(position, White, 0.3f * scale);
                if (Main.rand.NextBool(5))
                    CustomParticles.SwanFeatherDrift(position, Black, 0.25f * scale);
            }

            /// <summary>Boss death - ultimate elegance.</summary>
            public static void DeathExplosion(Vector2 position, float scale = 1f)
            {
                ExplosionUtility.CreateDeathExplosion(position, White, Black, scale * 1.4f);
                
                // Massive feather explosion
                ThemedParticles.SwanFeatherBurst(position, 50, 150f * scale);
                
                // Prismatic finale
                for (int i = 0; i < 24; i++)
                {
                    float angle = MathHelper.TwoPi * i / 24f;
                    Vector2 offset = angle.ToRotationVector2() * 60f * scale;
                    Color rainbow = GetRainbow(i / 24f);
                    CustomParticles.GenericFlare(position + offset, rainbow, 0.6f * scale, 25);
                    SpawnDirectionalSpark(position, offset.SafeNormalize(Vector2.Zero) * 15f, rainbow, 40, 2f * scale);
                }
                
                SpawnFlareShine(position, White, GetRainbow(), 0f, new Vector2(12f * scale), 60);
                
                MagnumScreenEffects.AddScreenShake(22f * scale);
                MagnumScreenEffects.SetFlashEffect(position, 2.2f * scale, 50);
            }

            /// <summary>Iridescent aura.</summary>
            public static void Aura(Vector2 position, float radius, float scale = 1f)
            {
                ThemedParticles.SwanLakeAura(position, radius * scale);
                
                if (Main.rand.NextBool(6))
                {
                    CustomParticles.SwanFeatherDrift(
                        position + Main.rand.NextVector2Circular(radius, radius) * scale,
                        Main.rand.NextBool() ? White : Black,
                        0.2f * scale
                    );
                }
            }
        }

        // ============================================================================
        // ENIGMA VARIATIONS THEME - Black → Purple → Green flame (mysterious/arcane)
        // ============================================================================
        public static class EnigmaVariations
        {
            public static readonly Color Black = new Color(15, 10, 20);
            public static readonly Color DeepPurple = new Color(80, 20, 120);
            public static readonly Color Purple = new Color(140, 60, 200);
            public static readonly Color GreenFlame = new Color(50, 220, 100);
            public static readonly Color DarkGreen = new Color(30, 100, 50);

            /// <summary>Mysterious impact with eerie flames.</summary>
            public static void Impact(Vector2 position, float scale = 1f)
            {
                SpawnPulseRing(position, Purple, 0f, 2.5f * scale, 35);
                SpawnPulseRing(position, GreenFlame * 0.7f, 0.3f * scale, 3f * scale, 45);
                SpawnStrongBloom(position, DeepPurple * 0.5f, 1.5f * scale, 30);
                
                // Gradient sparks Black → Purple → Green
                for (int i = 0; i < (int)(15 * scale); i++)
                {
                    Vector2 velocity = Main.rand.NextVector2Unit() * Main.rand.NextFloat(5f, 12f) * scale;
                    float progress = (float)i / 15f;
                    Color sparkColor;
                    if (progress < 0.5f)
                        sparkColor = Color.Lerp(Black, Purple, progress * 2f);
                    else
                        sparkColor = Color.Lerp(Purple, GreenFlame, (progress - 0.5f) * 2f);
                    SpawnDirectionalSpark(position, velocity, sparkColor, 35, 1.3f * scale);
                }
                
                MagnumScreenEffects.AddScreenShake(3f * scale);
            }

            /// <summary>Arcane explosion with swirling void.</summary>
            public static void Explosion(Vector2 position, float scale = 1f)
            {
                // Multi-layer gradient pulse rings
                SpawnPulseRing(position, Black, 0f, 2.5f * scale, 30);
                SpawnPulseRing(position, Purple, 0.2f * scale, 3f * scale, 40);
                SpawnPulseRing(position, GreenFlame, 0.4f * scale, 3.5f * scale, 50);
                
                // Green flame arcs
                for (int i = 0; i < 10; i++)
                {
                    Vector2 velocity = Main.rand.NextVector2Unit() * Main.rand.NextFloat(10f, 20f) * scale;
                    SpawnElectricArc(position, velocity, GreenFlame, 0.9f, 40);
                }
                
                ExplosionUtility.CreateEnergyExplosion(position, Purple, scale);
                
                MagnumScreenEffects.AddScreenShake(9f * scale);
                MagnumScreenEffects.SetFlashEffect(position, 1.3f * scale, 25);
            }

            /// <summary>Mysterious weapon swing.</summary>
            public static void SwingAura(Vector2 position, Vector2 direction, float scale = 1f)
            {
                // Gradient arc
                for (int i = 0; i < 5; i++)
                {
                    float offsetAngle = direction.ToRotation() + MathHelper.PiOver4 * ((float)i / 5f - 0.5f) * 2f;
                    Vector2 offset = offsetAngle.ToRotationVector2() * Main.rand.NextFloat(25f, 45f) * scale;
                    float progress = (float)i / 5f;
                    Color flareColor;
                    if (progress < 0.5f)
                        flareColor = Color.Lerp(Black, Purple, progress * 2f);
                    else
                        flareColor = Color.Lerp(Purple, GreenFlame, (progress - 0.5f) * 2f);
                    CustomParticles.GenericFlare(position + offset, flareColor, 0.35f * scale, 14);
                }
            }

            /// <summary>Trail with eerie green flames.</summary>
            public static void Trail(Vector2 position, Vector2 velocity, float scale = 1f)
            {
                VFXCombos.ElectricTrail(position, velocity, GreenFlame, scale * 0.6f);
                
                if (Main.rand.NextBool(3))
                {
                    var glow = new GenericGlowParticle(position, -velocity * 0.1f, Purple, 0.3f * scale, 15);
                    MagnumParticleHandler.SpawnParticle(glow);
                }
            }

            /// <summary>Boss death with void collapse.</summary>
            public static void DeathExplosion(Vector2 position, float scale = 1f)
            {
                ExplosionUtility.CreateDeathExplosion(position, Purple, GreenFlame, scale * 1.3f);
                
                // Void implosion then explosion effect
                for (int wave = 0; wave < 6; wave++)
                {
                    float progress = wave / 6f;
                    Color waveColor;
                    if (progress < 0.5f)
                        waveColor = Color.Lerp(Black, Purple, progress * 2f);
                    else
                        waveColor = Color.Lerp(Purple, GreenFlame, (progress - 0.5f) * 2f);
                    SpawnPulseRing(position, waveColor, 0f, (3f + wave) * scale, 35 + wave * 8);
                }
                
                // Green lightning storm
                for (int i = 0; i < 14; i++)
                {
                    Vector2 velocity = Main.rand.NextVector2Unit() * Main.rand.NextFloat(15f, 35f) * scale;
                    SpawnElectricArc(position, velocity, GreenFlame, 1.3f, 55);
                }
                
                SpawnFlareShine(position, GreenFlame, Purple, 0f, new Vector2(10f * scale), 55);
                
                MagnumScreenEffects.AddScreenShake(20f * scale);
                MagnumScreenEffects.SetFlashEffect(position, 2f * scale, 45);
            }
        }

        // ============================================================================
        // FATE THEME - White → Pink → Purple → Crimson (cosmic endgame)
        // ============================================================================
        public static class Fate
        {
            public static readonly Color White = new Color(255, 255, 255);
            public static readonly Color DarkPink = new Color(200, 80, 120);
            public static readonly Color Purple = new Color(140, 50, 160);
            public static readonly Color Crimson = new Color(180, 30, 60);
            public static readonly Color Black = new Color(10, 5, 15);

            /// <summary>Get cosmic gradient color based on progress.</summary>
            public static Color GetCosmicGradient(float progress)
            {
                if (progress < 0.33f)
                    return Color.Lerp(White, DarkPink, progress * 3f);
                else if (progress < 0.66f)
                    return Color.Lerp(DarkPink, Purple, (progress - 0.33f) * 3f);
                else
                    return Color.Lerp(Purple, Crimson, (progress - 0.66f) * 3f);
            }

            /// <summary>Cosmic impact with reality distortion.</summary>
            public static void Impact(Vector2 position, float scale = 1f)
            {
                // Cosmic gradient pulse rings
                for (int i = 0; i < 3; i++)
                {
                    Color ringColor = GetCosmicGradient(i / 3f);
                    SpawnPulseRing(position, ringColor, i * 0.15f * scale, (2.5f + i * 0.4f) * scale, 30 + i * 8);
                }
                
                SpawnStrongBloom(position, White * 0.7f, 2f * scale, 25);
                
                // Cosmic sparks with full gradient
                for (int i = 0; i < (int)(18 * scale); i++)
                {
                    Vector2 velocity = Main.rand.NextVector2Unit() * Main.rand.NextFloat(6f, 14f) * scale;
                    Color sparkColor = GetCosmicGradient((float)i / 18f);
                    SpawnDirectionalSpark(position, velocity, sparkColor, 35, 1.5f * scale);
                }
                
                MagnumScreenEffects.AddScreenShake(5f * scale);
            }

            /// <summary>Reality-shattering explosion.</summary>
            public static void Explosion(Vector2 position, float scale = 1f)
            {
                // Full cosmic gradient rings
                for (int i = 0; i < 5; i++)
                {
                    Color ringColor = GetCosmicGradient(i / 5f);
                    SpawnPulseRing(position, ringColor, 0f, (3f + i * 0.5f) * scale, 35 + i * 10);
                }
                
                // White core flash
                SpawnStrongBloom(position, White, 3f * scale, 30);
                
                // Cosmic lightning
                for (int i = 0; i < 12; i++)
                {
                    Vector2 velocity = Main.rand.NextVector2Unit() * Main.rand.NextFloat(12f, 25f) * scale;
                    Color arcColor = GetCosmicGradient(i / 12f);
                    SpawnElectricArc(position, velocity, arcColor, 1f, 45);
                }
                
                ExplosionUtility.CreateDeathExplosion(position, White, Crimson, scale);
                
                MagnumScreenEffects.AddScreenShake(12f * scale);
                MagnumScreenEffects.SetFlashEffect(position, 2f * scale, 30);
            }

            /// <summary>Cosmic weapon swing with reality trails.</summary>
            public static void SwingAura(Vector2 position, Vector2 direction, float scale = 1f)
            {
                // Full gradient arc
                for (int i = 0; i < 8; i++)
                {
                    float offsetAngle = direction.ToRotation() + MathHelper.PiOver4 * ((float)i / 8f - 0.5f) * 2f;
                    Vector2 offset = offsetAngle.ToRotationVector2() * Main.rand.NextFloat(25f, 50f) * scale;
                    Color flareColor = GetCosmicGradient(i / 8f);
                    CustomParticles.GenericFlare(position + offset, flareColor, 0.4f * scale, 14);
                }
                
                // Chromatic aberration effect (RGB offset flares)
                Vector2 dir = direction.SafeNormalize(Vector2.UnitX);
                CustomParticles.GenericFlare(position + dir * 45f + new Vector2(-2, 0), Color.Red * 0.5f, 0.3f * scale, 10);
                CustomParticles.GenericFlare(position + dir * 45f, Color.Green * 0.5f, 0.3f * scale, 10);
                CustomParticles.GenericFlare(position + dir * 45f + new Vector2(2, 0), Color.Blue * 0.5f, 0.3f * scale, 10);
            }

            /// <summary>Cosmic trail with afterimages.</summary>
            public static void Trail(Vector2 position, Vector2 velocity, float scale = 1f)
            {
                // Multi-color trail
                for (int i = 0; i < 3; i++)
                {
                    Color trailColor = GetCosmicGradient(i / 3f);
                    var glow = new GenericGlowParticle(position + Main.rand.NextVector2Circular(5f, 5f), -velocity * 0.1f, trailColor, 0.25f * scale, 12);
                    MagnumParticleHandler.SpawnParticle(glow);
                }
            }

            /// <summary>Ultimate cosmic death - reality collapse.</summary>
            public static void DeathExplosion(Vector2 position, float scale = 1f)
            {
                // Massive cosmic gradient explosion
                for (int wave = 0; wave < 8; wave++)
                {
                    Color waveColor = GetCosmicGradient(wave / 8f);
                    SpawnPulseRing(position, waveColor, 0f, (3f + wave) * scale, 40 + wave * 8);
                }
                
                // White core
                SpawnStrongBloom(position, White, 5f * scale, 40);
                SpawnFlareShine(position, White, Crimson, 0f, new Vector2(15f * scale), 70);
                
                // Cosmic lightning storm
                for (int i = 0; i < 20; i++)
                {
                    Vector2 velocity = Main.rand.NextVector2Unit() * Main.rand.NextFloat(20f, 50f) * scale;
                    Color arcColor = GetCosmicGradient(i / 20f);
                    SpawnElectricArc(position, velocity, arcColor, 1.5f, 60);
                }
                
                // Massive spark burst
                for (int i = 0; i < 50; i++)
                {
                    Vector2 velocity = Main.rand.NextVector2Unit() * Main.rand.NextFloat(10f, 40f) * scale;
                    Color sparkColor = GetCosmicGradient(i / 50f);
                    SpawnDirectionalSpark(position, velocity, sparkColor, 60, 2.5f * scale);
                }
                
                ExplosionUtility.CreateDeathExplosion(position, White, Crimson, scale * 2f);
                
                MagnumScreenEffects.AddScreenShake(30f * scale);
                MagnumScreenEffects.SetFlashEffect(position, 3f * scale, 60);
            }
        }

        // ============================================================================
        // GENERIC EFFECTS - Theme-agnostic utilities
        // ============================================================================
        public static class Generic
        {
            /// <summary>Generic impact with custom colors.</summary>
            public static void Impact(Vector2 position, Color primary, Color secondary, float scale = 1f)
            {
                VFXCombos.StandardImpact(position, primary, secondary, scale);
            }

            /// <summary>Generic explosion with custom colors.</summary>
            public static void Explosion(Vector2 position, Color primary, Color secondary, float scale = 1f)
            {
                VFXCombos.MajorExplosion(position, primary, secondary, scale);
            }

            /// <summary>Generic death explosion.</summary>
            public static void DeathExplosion(Vector2 position, Color primary, Color secondary, float scale = 1f)
            {
                VFXCombos.BossDeathExplosion(position, primary, secondary, scale);
            }

            /// <summary>Generic teleport effect.</summary>
            public static void Teleport(Vector2 departure, Vector2 arrival, Color color, float scale = 1f)
            {
                VFXCombos.DramaticTeleport(departure, arrival, color, scale);
            }

            /// <summary>Generic charge windup effect.</summary>
            public static void ChargeWindup(Vector2 position, Color color, float progress, float scale = 1f)
            {
                VFXCombos.ChargePulse(position, color, progress, scale);
            }

            /// <summary>Generic attack release.</summary>
            public static void AttackRelease(Vector2 position, Color primary, Color secondary, float scale = 1f)
            {
                VFXCombos.AttackRelease(position, primary, secondary, scale);
            }

            /// <summary>Fractal geometric burst - signature MagnumOpus look.</summary>
            public static void FractalBurst(Vector2 position, Color primary, Color secondary, int points = 6, float radius = 30f, float scale = 1f)
            {
                for (int i = 0; i < points; i++)
                {
                    float angle = MathHelper.TwoPi * i / points;
                    Vector2 offset = angle.ToRotationVector2() * radius * scale;
                    float progress = (float)i / points;
                    Color flareColor = Color.Lerp(primary, secondary, progress);
                    CustomParticles.GenericFlare(position + offset, flareColor, 0.4f * scale, 18);
                }
                
                // Central flare
                CustomParticles.GenericFlare(position, Color.White, 0.5f * scale, 15);
            }

            /// <summary>Orbiting particles around a position.</summary>
            public static void OrbitingAura(Vector2 center, Color primary, Color secondary, float radius, int count = 5, float scale = 1f)
            {
                float baseAngle = Main.GameUpdateCount * 0.03f;
                for (int i = 0; i < count; i++)
                {
                    float angle = baseAngle + MathHelper.TwoPi * i / count;
                    float particleRadius = radius + (float)Math.Sin(Main.GameUpdateCount * 0.05f + i * 0.7f) * 12f;
                    Vector2 pos = center + angle.ToRotationVector2() * particleRadius * scale;
                    Color flareColor = Color.Lerp(primary, secondary, (float)i / count);
                    CustomParticles.GenericFlare(pos, flareColor, 0.3f * scale, 18);
                }
            }
        }

        // ============================================================================
        // HELPER METHODS - Particle spawning wrappers
        // ============================================================================
        
        private static void SpawnPulseRing(Vector2 position, Color color, float startScale, float endScale, int lifetime)
        {
            try
            {
                var ring = new PulseRingParticle(position, Vector2.Zero, color, startScale, endScale, lifetime);
                MagnumParticleHandler.SpawnParticle(ring);
            }
            catch { }
        }

        private static void SpawnStrongBloom(Vector2 position, Color color, float scale, int lifetime)
        {
            try
            {
                var bloom = new StrongBloomParticle(position, Vector2.Zero, color, scale, lifetime);
                MagnumParticleHandler.SpawnParticle(bloom);
            }
            catch { }
        }

        private static void SpawnDirectionalSpark(Vector2 position, Vector2 velocity, Color color, int lifetime, float scale)
        {
            try
            {
                var spark = new DirectionalSparkParticle(position, velocity, false, lifetime, scale, color);
                MagnumParticleHandler.SpawnParticle(spark);
            }
            catch { }
        }

        private static void SpawnDenseSmoke(Vector2 position, Vector2 velocity, Color color, int lifetime, float scale, float opacity)
        {
            try
            {
                var smoke = new DenseSmokeParticle(position, velocity, color, lifetime, scale, opacity);
                MagnumParticleHandler.SpawnParticle(smoke);
            }
            catch { }
        }

        private static void SpawnElectricArc(Vector2 position, Vector2 velocity, Color color, float intensity, int lifetime)
        {
            try
            {
                var arc = new ElectricArcParticle(position, velocity, color, intensity, lifetime);
                MagnumParticleHandler.SpawnParticle(arc);
            }
            catch { }
        }

        private static void SpawnFlareShine(Vector2 position, Color mainColor, Color bloomColor, float startScale, Vector2 endScale, int lifetime)
        {
            try
            {
                var flare = new FlareShineParticle(position, Vector2.Zero, mainColor, bloomColor, startScale, endScale, lifetime);
                MagnumParticleHandler.SpawnParticle(flare);
            }
            catch { }
        }
    }
}
