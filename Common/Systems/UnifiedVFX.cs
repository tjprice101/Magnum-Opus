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
        // DESIGN: Flashy, beautiful, elegant - mysteries made visible
        // Eyes watch meaningfully, glyphs mark arcane power, swirling void consumes
        // ============================================================================
        public static class EnigmaVariations
        {
            public static readonly Color Black = new Color(15, 10, 20);
            public static readonly Color DeepPurple = new Color(80, 20, 120);
            public static readonly Color Purple = new Color(140, 60, 200);
            public static readonly Color GreenFlame = new Color(50, 220, 100);
            public static readonly Color DarkGreen = new Color(30, 100, 50);

            /// <summary>Get enigma gradient: Black → Purple → Green</summary>
            public static Color GetGradient(float progress)
            {
                if (progress < 0.5f)
                    return Color.Lerp(Black, Purple, progress * 2f);
                else
                    return Color.Lerp(Purple, GreenFlame, (progress - 0.5f) * 2f);
            }

            /// <summary>SPECTACULAR mysterious impact with eerie flames and watching eyes.</summary>
            public static void Impact(Vector2 position, float scale = 1f)
            {
                // === PHASE 1: CENTRAL FLASH - The mystery reveals ===
                CustomParticles.GenericFlare(position, Color.White, 0.9f * scale, 18);
                CustomParticles.GenericFlare(position, GreenFlame, 0.8f * scale, 22);
                CustomParticles.GenericFlare(position, Purple, 0.65f * scale, 20);
                
                // === PHASE 2: MULTI-LAYER PULSE RINGS - Reality ripples ===
                for (int ring = 0; ring < 5; ring++)
                {
                    float progress = (float)ring / 5f;
                    Color ringColor = GetGradient(progress);
                    SpawnPulseRing(position, ringColor, ring * 0.1f * scale, (2.5f + ring * 0.4f) * scale, 30 + ring * 6);
                }
                
                SpawnStrongBloom(position, DeepPurple * 0.6f, 2f * scale, 35);
                
                // === PHASE 3: FRACTAL GEOMETRIC BURST ===
                for (int i = 0; i < 8; i++)
                {
                    float angle = MathHelper.TwoPi * i / 8f;
                    Vector2 offset = angle.ToRotationVector2() * 35f * scale;
                    Color flareColor = GetGradient((float)i / 8f);
                    CustomParticles.GenericFlare(position + offset, flareColor, 0.55f * scale, 18);
                }
                
                // === PHASE 4: RADIAL SPARK SPRAY - Mystery fragments ===
                for (int i = 0; i < (int)(25 * scale); i++)
                {
                    Vector2 velocity = Main.rand.NextVector2Unit() * Main.rand.NextFloat(6f, 15f) * scale;
                    Color sparkColor = GetGradient((float)i / 25f);
                    SpawnDirectionalSpark(position, velocity, sparkColor, 40, 1.5f * scale);
                }
                
                // === PHASE 5: WATCHING EYES - Meaningful placement ===
                // Eyes at cardinal positions watching the impact point
                CustomParticles.EnigmaEyeFormation(position, GreenFlame * 0.9f, 4, 50f * scale);
                
                // === PHASE 6: ARCANE GLYPHS - Mark the mystery ===
                CustomParticles.GlyphBurst(position, Purple, (int)(6 * scale), 5f * scale);
                CustomParticles.GlyphImpact(position, DeepPurple, GreenFlame, 0.6f * scale);
                
                // === PHASE 7: MUSIC NOTES - The enigma sings ===
                ThemedParticles.EnigmaMusicNoteBurst(position, (int)(10 * scale), 5f * scale);
                
                // === PHASE 8: GREEN FLAME LIGHTNING FRACTALS ===
                for (int i = 0; i < (int)(8 * scale); i++)
                {
                    Vector2 velocity = Main.rand.NextVector2Unit() * Main.rand.NextFloat(8f, 18f) * scale;
                    SpawnElectricArc(position, velocity, GreenFlame, 1f * scale, 35);
                }
                
                Lighting.AddLight(position, GreenFlame.ToVector3() * 1.2f * scale);
            }

            /// <summary>MASSIVE arcane explosion with swirling void and cascading mysteries.</summary>
            public static void Explosion(Vector2 position, float scale = 1f)
            {
                // === CENTRAL CORE DETONATION ===
                CustomParticles.GenericFlare(position, Color.White, 1.2f * scale, 25);
                CustomParticles.GenericFlare(position, GreenFlame, 1.0f * scale, 30);
                CustomParticles.GenericFlare(position, Purple, 0.85f * scale, 28);
                
                // === CASCADING VOID PULSE RINGS ===
                for (int ring = 0; ring < 8; ring++)
                {
                    float progress = (float)ring / 8f;
                    Color ringColor = GetGradient(progress);
                    SpawnPulseRing(position, ringColor, 0f, (3f + ring * 0.6f) * scale, 35 + ring * 8);
                }
                
                SpawnStrongBloom(position, DeepPurple, 3f * scale, 40);
                
                // === SPIRAL GALAXY BURST - Fractal elegance ===
                for (int arm = 0; arm < 6; arm++)
                {
                    float armAngle = MathHelper.TwoPi * arm / 6f;
                    for (int point = 0; point < 6; point++)
                    {
                        float spiralAngle = armAngle + point * 0.35f;
                        float spiralRadius = 25f + point * 15f;
                        Vector2 spiralPos = position + spiralAngle.ToRotationVector2() * spiralRadius * scale;
                        Color galaxyColor = GetGradient((arm * 6 + point) / 36f);
                        CustomParticles.GenericFlare(spiralPos, galaxyColor, 0.5f + point * 0.05f, 20 + point * 2);
                    }
                }
                
                // === VOID LIGHTNING STORM ===
                for (int i = 0; i < (int)(16 * scale); i++)
                {
                    Vector2 velocity = Main.rand.NextVector2Unit() * Main.rand.NextFloat(12f, 28f) * scale;
                    Color arcColor = i % 2 == 0 ? GreenFlame : Purple;
                    SpawnElectricArc(position, velocity, arcColor, 1.2f * scale, 45);
                }
                
                // === EXPLODING EYES - Watching the void scatter ===
                CustomParticles.EnigmaEyeExplosion(position, GreenFlame, (int)(8 * scale), 6f * scale);
                
                // === ARCANE GLYPH STORM ===
                CustomParticles.GlyphCircle(position, Purple, (int)(10 * scale), 60f * scale, 0.05f);
                CustomParticles.GlyphBurst(position, GreenFlame, (int)(10 * scale), 8f * scale);
                
                // === MUSICAL RESONANCE ===
                ThemedParticles.EnigmaMusicNoteBurst(position, (int)(14 * scale), 7f * scale);
                ThemedParticles.EnigmaMusicNotes(position, (int)(8 * scale), 80f * scale);
                
                ExplosionUtility.CreateEnergyExplosion(position, Purple, scale * 1.5f);
                
                Lighting.AddLight(position, GreenFlame.ToVector3() * 2f * scale);
            }

            /// <summary>ELEGANT mysterious weapon swing with arcane trails.</summary>
            public static void SwingAura(Vector2 position, Vector2 direction, float scale = 1f)
            {
                // === GRADIENT ARC - Beautiful sweep ===
                for (int i = 0; i < 10; i++)
                {
                    float offsetAngle = direction.ToRotation() + MathHelper.PiOver2 * ((float)i / 10f - 0.5f);
                    Vector2 offset = offsetAngle.ToRotationVector2() * Main.rand.NextFloat(30f, 55f) * scale;
                    Color flareColor = GetGradient((float)i / 10f);
                    CustomParticles.GenericFlare(position + offset, flareColor, 0.45f * scale, 16);
                }
                
                // === SPARKLE CASCADE along swing ===
                for (int i = 0; i < 6; i++)
                {
                    float arcAngle = direction.ToRotation() + MathHelper.PiOver4 * ((float)i / 6f - 0.5f);
                    Vector2 sparkPos = position + arcAngle.ToRotationVector2() * 40f * scale;
                    var sparkle = new GenericGlowParticle(sparkPos, arcAngle.ToRotationVector2() * 2f, 
                        GetGradient((float)i / 6f), 0.35f * scale, 20, true);
                    MagnumParticleHandler.SpawnParticle(sparkle);
                }
                
                // === GREEN FLAME SPARKS flying from blade edge ===
                for (int i = 0; i < 5; i++)
                {
                    float sparkAngle = direction.ToRotation() + Main.rand.NextFloat(-0.4f, 0.4f);
                    Vector2 sparkVel = sparkAngle.ToRotationVector2() * Main.rand.NextFloat(5f, 10f);
                    SpawnDirectionalSpark(position + direction * 35f, sparkVel, GreenFlame, 25, 1.2f * scale);
                }
                
                // === OCCASIONAL WATCHING EYE ===
                if (Main.rand.NextBool(3))
                {
                    Vector2 eyePos = position + direction * 55f * scale;
                    CustomParticles.EnigmaEyeGaze(eyePos, GreenFlame * 0.8f, 0.35f * scale, direction);
                }
                
                // === TRAILING GLYPHS ===
                if (Main.rand.NextBool(2))
                {
                    CustomParticles.Glyph(position + direction * 30f, Purple * 0.7f, 0.3f * scale, -1);
                }
                
                // === MUSIC NOTES dancing in the arc ===
                if (Main.rand.NextBool(2))
                {
                    ThemedParticles.EnigmaMusicNotes(position + direction * 40f, 2, 20f * scale);
                }
            }

            /// <summary>BEAUTIFUL trail with eerie green flames and arcane echoes.</summary>
            public static void Trail(Vector2 position, Vector2 velocity, float scale = 1f)
            {
                // === PRIMARY GLOW TRAIL ===
                Color trailColor = GetGradient(Main.rand.NextFloat());
                var glow = new GenericGlowParticle(position, -velocity * 0.1f + Main.rand.NextVector2Circular(1f, 1f), 
                    trailColor, 0.35f * scale, 18, true);
                MagnumParticleHandler.SpawnParticle(glow);
                
                // === GREEN FLAME WISPS ===
                if (Main.rand.NextBool(2))
                {
                    var flame = new GenericGlowParticle(position + Main.rand.NextVector2Circular(8f, 8f), 
                        -velocity * 0.15f + new Vector2(0, -1f), GreenFlame * 0.8f, 0.3f * scale, 15, true);
                    MagnumParticleHandler.SpawnParticle(flame);
                }
                
                // === OCCASIONAL WATCHING EYE in trail ===
                if (Main.rand.NextBool(6))
                {
                    CustomParticles.EnigmaEyeTrail(position, velocity, GreenFlame * 0.7f, 0.25f * scale);
                }
                
                // === SPARSE GLYPHS left behind ===
                if (Main.rand.NextBool(5))
                {
                    CustomParticles.GlyphTrail(position, velocity, Purple * 0.6f, 0.22f * scale);
                }
                
                // === MUSIC NOTES echoing in the trail ===
                if (Main.rand.NextBool(4))
                {
                    ThemedParticles.EnigmaMusicNotes(position, 1, 12f * scale);
                }
                
                VFXCombos.ElectricTrail(position, velocity, GreenFlame, scale * 0.4f);
            }

            /// <summary>ULTIMATE Boss death with void collapse and arcane revelation.</summary>
            public static void DeathExplosion(Vector2 position, float scale = 1f)
            {
                // === PHASE 1: THE MYSTERY REVEALED - White core flash ===
                SpawnFlareShine(position, Color.White, GreenFlame, 0f, new Vector2(15f * scale), 60);
                SpawnStrongBloom(position, Color.White, 5f * scale, 30);
                
                // === PHASE 2: CASCADING VOID RINGS ===
                for (int wave = 0; wave < 10; wave++)
                {
                    float progress = wave / 10f;
                    Color waveColor = GetGradient(progress);
                    SpawnPulseRing(position, waveColor, 0f, (4f + wave * 1f) * scale, 40 + wave * 10);
                }
                
                // === PHASE 3: SPIRAL GALAXY OF MYSTERY ===
                for (int arm = 0; arm < 8; arm++)
                {
                    float armAngle = MathHelper.TwoPi * arm / 8f;
                    for (int point = 0; point < 10; point++)
                    {
                        float spiralAngle = armAngle + point * 0.45f;
                        float spiralRadius = 30f + point * 20f;
                        Vector2 spiralPos = position + spiralAngle.ToRotationVector2() * spiralRadius * scale;
                        Color galaxyColor = GetGradient((arm * 10 + point) / 80f);
                        CustomParticles.GenericFlare(spiralPos, galaxyColor, 0.6f + point * 0.05f, 25 + point * 3);
                    }
                }
                
                // === PHASE 4: LIGHTNING STORM OF REVELATION ===
                for (int i = 0; i < 25; i++)
                {
                    Vector2 velocity = Main.rand.NextVector2Unit() * Main.rand.NextFloat(20f, 50f) * scale;
                    Color arcColor = GetGradient((float)i / 25f);
                    SpawnElectricArc(position, velocity, arcColor, 1.8f * scale, 60);
                }
                
                // === PHASE 5: MASSIVE SPARK BURST ===
                for (int i = 0; i < 60; i++)
                {
                    Vector2 velocity = Main.rand.NextVector2Unit() * Main.rand.NextFloat(10f, 45f) * scale;
                    Color sparkColor = GetGradient(i / 60f);
                    SpawnDirectionalSpark(position, velocity, sparkColor, 70, 2.2f * scale);
                }
                
                // === PHASE 6: EYES WATCHING THE VOID COLLAPSE ===
                CustomParticles.EnigmaEyeExplosion(position, GreenFlame, 12, 10f * scale);
                
                // Multiple eye formations at different radii
                CustomParticles.EnigmaEyeFormation(position, Purple, 6, 80f * scale);
                CustomParticles.EnigmaEyeFormation(position, GreenFlame, 8, 140f * scale);
                
                // === PHASE 7: ARCANE GLYPH NOVA ===
                CustomParticles.GlyphCircle(position, Purple, 16, 100f * scale, 0.03f);
                CustomParticles.GlyphCircle(position, GreenFlame, 12, 60f * scale, 0.05f);
                CustomParticles.GlyphBurst(position, DeepPurple, 16, 12f * scale);
                CustomParticles.GlyphTower(position, Purple, 5, 0.7f * scale);
                
                // === PHASE 8: THE ENIGMA'S FINAL SYMPHONY ===
                ThemedParticles.EnigmaMusicNoteBurst(position, 20, 10f * scale);
                ThemedParticles.EnigmaMusicNotes(position, 15, 120f * scale);
                
                ExplosionUtility.CreateDeathExplosion(position, Purple, GreenFlame, scale * 2f);
                
                Lighting.AddLight(position, GreenFlame.ToVector3() * 3f * scale);
            }
            
            /// <summary>ELEGANT ambient aura with swirling mysteries.</summary>
            public static void Aura(Vector2 center, float radius, float scale = 1f)
            {
                // === ORBITING VOID MOTES ===
                if (Main.rand.NextBool(3))
                {
                    float angle = Main.GameUpdateCount * 0.03f + Main.rand.NextFloat() * MathHelper.TwoPi;
                    Vector2 motePos = center + angle.ToRotationVector2() * radius * Main.rand.NextFloat(0.6f, 1f);
                    Color moteColor = GetGradient(Main.rand.NextFloat());
                    var mote = new GenericGlowParticle(motePos, new Vector2(0, -0.5f), moteColor * 0.65f, 0.25f * scale, 25, true);
                    MagnumParticleHandler.SpawnParticle(mote);
                }
                
                // === ORBITING GLYPH ===
                if (Main.rand.NextBool(15))
                {
                    float glyphAngle = Main.GameUpdateCount * 0.02f;
                    Vector2 glyphPos = center + glyphAngle.ToRotationVector2() * radius * 0.9f;
                    CustomParticles.Glyph(glyphPos, Purple * 0.5f, 0.22f * scale, -1);
                }
                
                // === OCCASIONAL WATCHING EYE ===
                if (Main.rand.NextBool(25))
                {
                    Vector2 eyePos = center + Main.rand.NextVector2Circular(radius * 0.7f, radius * 0.7f);
                    Vector2 lookDir = (center - eyePos).SafeNormalize(Vector2.UnitY);
                    CustomParticles.EnigmaEyeGaze(eyePos, GreenFlame * 0.6f, 0.22f * scale, lookDir);
                }
                
                // === FLOATING MUSIC NOTES ===
                if (Main.rand.NextBool(20))
                {
                    ThemedParticles.EnigmaMusicNotes(center + Main.rand.NextVector2Circular(radius * 0.5f, radius * 0.5f), 1, 15f);
                }
                
                Lighting.AddLight(center, Purple.ToVector3() * 0.25f * scale);
            }
            
            /// <summary>MYSTERIOUS hit effect for weapon strikes.</summary>
            public static void HitEffect(Vector2 position, float scale = 1f)
            {
                // === GRADIENT FLARE BURST ===
                for (int i = 0; i < 10; i++)
                {
                    float angle = MathHelper.TwoPi * i / 10f;
                    Vector2 offset = angle.ToRotationVector2() * 30f * scale;
                    Color flareColor = GetGradient((float)i / 10f);
                    CustomParticles.GenericFlare(position + offset, flareColor, 0.45f * scale, 16);
                }
                
                // === CENTRAL FLASH ===
                CustomParticles.GenericFlare(position, GreenFlame, 0.7f * scale, 18);
                CustomParticles.GenericFlare(position, Color.White, 0.5f * scale, 12);
                
                // === HALOS ===
                for (int i = 0; i < 4; i++)
                {
                    CustomParticles.HaloRing(position, GetGradient(i / 4f), (0.35f + i * 0.15f) * scale, 14 + i * 4);
                }
                
                // === WATCHING EYE at impact ===
                CustomParticles.EnigmaEyeImpact(position, position, GreenFlame * 0.8f, 0.45f * scale);
                
                // === GLYPHS ===
                CustomParticles.GlyphImpact(position, Purple, GreenFlame, 0.5f * scale);
                
                // === MUSIC NOTES ===
                ThemedParticles.EnigmaMusicNoteBurst(position, 6, 4f * scale);
                
                Lighting.AddLight(position, GreenFlame.ToVector3() * 0.8f * scale);
            }
        }

        // ============================================================================
        // FATE THEME - DARK PRISMATIC: Black → Dark Pink → Bright Red (cosmic endgame)
        // ============================================================================
        // DESIGN: ULTIMATE ENDGAME SPECTACLE - Reality itself bending
        // Chromatic aberration, temporal echoes, screen distortions, cosmic inevitability
        // This MUST be the most visually impressive theme - it's the endgame!
        // ============================================================================
        public static class Fate
        {
            // DARK PRISMATIC PALETTE - Black is PRIMARY
            public static readonly Color FateBlack = new Color(15, 5, 20);
            public static readonly Color FateDarkPink = new Color(180, 50, 100);
            public static readonly Color FateBrightRed = new Color(255, 60, 80);
            public static readonly Color FatePurple = new Color(120, 30, 140);
            public static readonly Color FateWhite = new Color(255, 255, 255);
            
            // Legacy aliases for compatibility
            public static Color White => FateWhite;
            public static Color DarkPink => FateDarkPink;
            public static Color Purple => FatePurple;
            public static Color Crimson => FateBrightRed;
            public static Color Black => FateBlack;

            /// <summary>Get DARK prismatic gradient: Black → Pink → Red → White flash.</summary>
            public static Color GetCosmicGradient(float progress)
            {
                if (progress < 0.4f)
                    return Color.Lerp(FateBlack, FateDarkPink, progress / 0.4f);
                else if (progress < 0.8f)
                    return Color.Lerp(FateDarkPink, FateBrightRed, (progress - 0.4f) / 0.4f);
                else
                    return Color.Lerp(FateBrightRed, FateWhite, (progress - 0.8f) / 0.2f);
            }

            /// <summary>SPECTACULAR cosmic impact with reality distortion and chromatic aberration.</summary>
            public static void Impact(Vector2 position, float scale = 1f)
            {
                // === PHASE 1: REALITY FLASH - The moment destiny strikes ===
                CustomParticles.GenericFlare(position, FateWhite, 1.0f * scale, 20);
                CustomParticles.GenericFlare(position, FateBrightRed, 0.85f * scale, 22);
                CustomParticles.GenericFlare(position, FateDarkPink, 0.7f * scale, 20);
                CustomParticles.GenericFlare(position, FateBlack, 0.55f * scale, 18);
                
                // === PHASE 2: CHROMATIC ABERRATION - Reality fractures ===
                // RGB separation at impact - signature Fate visual
                for (int i = 0; i < 3; i++)
                {
                    float separation = 4f * (1f + i * 0.5f) * scale;
                    CustomParticles.GenericFlare(position + new Vector2(-separation, 0), new Color(255, 50, 80) * 0.5f, 0.5f * scale, 14);
                    CustomParticles.GenericFlare(position + new Vector2(separation * 0.5f, -separation * 0.3f), new Color(50, 255, 100) * 0.4f, 0.4f * scale, 12);
                    CustomParticles.GenericFlare(position + new Vector2(separation, separation * 0.2f), new Color(80, 80, 255) * 0.5f, 0.5f * scale, 14);
                }
                
                // === PHASE 3: COSMIC GRADIENT PULSE RINGS ===
                for (int ring = 0; ring < 6; ring++)
                {
                    float progress = (float)ring / 6f;
                    Color ringColor = GetCosmicGradient(progress);
                    SpawnPulseRing(position, ringColor, ring * 0.1f * scale, (2.5f + ring * 0.5f) * scale, 30 + ring * 6);
                }
                
                SpawnStrongBloom(position, FateDarkPink * 0.7f, 2.5f * scale, 30);
                
                // === PHASE 4: FRACTAL GEOMETRIC BURST ===
                for (int i = 0; i < 10; i++)
                {
                    float angle = MathHelper.TwoPi * i / 10f;
                    Vector2 offset = angle.ToRotationVector2() * 40f * scale;
                    Color flareColor = GetCosmicGradient((float)i / 10f);
                    CustomParticles.GenericFlare(position + offset, flareColor, 0.5f * scale, 18);
                    
                    // Additional chromatic aberration on each flare
                    CustomParticles.GenericFlare(position + offset + new Vector2(-2, 0), FateBrightRed * 0.3f, 0.25f * scale, 10);
                    CustomParticles.GenericFlare(position + offset + new Vector2(2, 0), FatePurple * 0.3f, 0.25f * scale, 10);
                }
                
                // === PHASE 5: COSMIC SPARK SPRAY ===
                for (int i = 0; i < (int)(30 * scale); i++)
                {
                    Vector2 velocity = Main.rand.NextVector2Unit() * Main.rand.NextFloat(7f, 16f) * scale;
                    Color sparkColor = GetCosmicGradient((float)i / 30f);
                    SpawnDirectionalSpark(position, velocity, sparkColor, 40, 1.6f * scale);
                }
                
                // === PHASE 6: DESTINY GLYPHS ===
                CustomParticles.GlyphBurst(position, FateDarkPink, (int)(6 * scale), 5f * scale);
                CustomParticles.GlyphImpact(position, FateBlack, FateBrightRed, 0.55f * scale);
                
                // === PHASE 7: COSMIC MUSIC NOTES - The symphony of fate ===
                ThemedParticles.FateMusicNoteBurst(position, (int)(12 * scale), 6f * scale);
                
                Lighting.AddLight(position, FateBrightRed.ToVector3() * 1.5f * scale);
            }

            /// <summary>MASSIVE Reality-shattering explosion with full cosmic spectacle.</summary>
            public static void Explosion(Vector2 position, float scale = 1f)
            {
                // === CENTRAL REALITY DETONATION ===
                CustomParticles.GenericFlare(position, FateWhite, 1.4f * scale, 28);
                CustomParticles.GenericFlare(position, FateBrightRed, 1.1f * scale, 32);
                CustomParticles.GenericFlare(position, FateDarkPink, 0.9f * scale, 28);
                CustomParticles.GenericFlare(position, FateBlack, 0.7f * scale, 25);
                
                // === HEAVY CHROMATIC ABERRATION ===
                for (int layer = 0; layer < 4; layer++)
                {
                    float separation = (6f + layer * 3f) * scale;
                    float alpha = 0.5f - layer * 0.1f;
                    CustomParticles.GenericFlare(position + new Vector2(-separation, 0), new Color(255, 50, 80) * alpha, (0.7f - layer * 0.1f) * scale, 18);
                    CustomParticles.GenericFlare(position + new Vector2(separation, 0), new Color(80, 80, 255) * alpha, (0.7f - layer * 0.1f) * scale, 18);
                }
                
                // === CASCADING COSMIC GRADIENT RINGS ===
                for (int ring = 0; ring < 10; ring++)
                {
                    float progress = (float)ring / 10f;
                    Color ringColor = GetCosmicGradient(progress);
                    SpawnPulseRing(position, ringColor, 0f, (3f + ring * 0.7f) * scale, 35 + ring * 8);
                }
                
                SpawnStrongBloom(position, FateWhite, 4f * scale, 35);
                
                // === SPIRAL GALAXY OF DESTINY ===
                for (int arm = 0; arm < 8; arm++)
                {
                    float armAngle = MathHelper.TwoPi * arm / 8f;
                    for (int point = 0; point < 8; point++)
                    {
                        float spiralAngle = armAngle + point * 0.4f;
                        float spiralRadius = 25f + point * 18f;
                        Vector2 spiralPos = position + spiralAngle.ToRotationVector2() * spiralRadius * scale;
                        Color galaxyColor = GetCosmicGradient((arm * 8 + point) / 64f);
                        CustomParticles.GenericFlare(spiralPos, galaxyColor, 0.55f + point * 0.05f, 22 + point * 2);
                    }
                }
                
                // === COSMIC LIGHTNING STORM ===
                for (int i = 0; i < (int)(18 * scale); i++)
                {
                    Vector2 velocity = Main.rand.NextVector2Unit() * Main.rand.NextFloat(14f, 30f) * scale;
                    Color arcColor = GetCosmicGradient((float)i / 18f);
                    SpawnElectricArc(position, velocity, arcColor, 1.2f * scale, 50);
                }
                
                // === DESTINY GLYPH NOVA ===
                CustomParticles.GlyphCircle(position, FateDarkPink, (int)(8 * scale), 70f * scale, 0.04f);
                CustomParticles.GlyphBurst(position, FateBrightRed, (int)(10 * scale), 9f * scale);
                
                // === COSMIC SYMPHONY ===
                ThemedParticles.FateMusicNoteBurst(position, (int)(16 * scale), 8f * scale);
                ThemedParticles.FateMusicNotes(position, (int)(10 * scale), 90f * scale);
                
                ExplosionUtility.CreateDeathExplosion(position, FateWhite, FateBrightRed, scale * 1.5f);
                
                Lighting.AddLight(position, FateBrightRed.ToVector3() * 2.5f * scale);
            }

            /// <summary>ELEGANT Cosmic weapon swing with temporal echoes and chromatic trails.</summary>
            public static void SwingAura(Vector2 position, Vector2 direction, float scale = 1f)
            {
                // === FULL GRADIENT ARC - Beautiful cosmic sweep ===
                for (int i = 0; i < 12; i++)
                {
                    float offsetAngle = direction.ToRotation() + MathHelper.PiOver2 * ((float)i / 12f - 0.5f);
                    Vector2 offset = offsetAngle.ToRotationVector2() * Main.rand.NextFloat(30f, 60f) * scale;
                    Color flareColor = GetCosmicGradient((float)i / 12f);
                    CustomParticles.GenericFlare(position + offset, flareColor, 0.45f * scale, 16);
                }
                
                // === CHROMATIC ABERRATION at blade edge ===
                Vector2 bladeEdge = position + direction * 50f * scale;
                CustomParticles.GenericFlare(bladeEdge + new Vector2(-4, 0), new Color(255, 50, 80) * 0.55f, 0.4f * scale, 12);
                CustomParticles.GenericFlare(bladeEdge, FateDarkPink * 0.7f, 0.45f * scale, 14);
                CustomParticles.GenericFlare(bladeEdge + new Vector2(4, 0), new Color(80, 80, 255) * 0.55f, 0.4f * scale, 12);
                
                // === TEMPORAL ECHO TRAIL ===
                for (int i = 0; i < 5; i++)
                {
                    float echoOffset = i * 12f * scale;
                    Vector2 echoPos = position + direction * (35f - echoOffset);
                    float echoAlpha = 1f - i * 0.18f;
                    Color echoColor = GetCosmicGradient((float)i / 5f) * echoAlpha;
                    CustomParticles.GenericFlare(echoPos, echoColor, (0.35f - i * 0.05f) * scale, 12 - i);
                }
                
                // === SPARK CASCADE from blade edge ===
                for (int i = 0; i < 6; i++)
                {
                    float sparkAngle = direction.ToRotation() + Main.rand.NextFloat(-0.5f, 0.5f);
                    Vector2 sparkVel = sparkAngle.ToRotationVector2() * Main.rand.NextFloat(6f, 12f);
                    Color sparkColor = GetCosmicGradient(Main.rand.NextFloat());
                    SpawnDirectionalSpark(bladeEdge, sparkVel, sparkColor, 30, 1.3f * scale);
                }
                
                // === DESTINY GLYPHS in the arc ===
                if (Main.rand.NextBool(2))
                {
                    CustomParticles.Glyph(position + direction * 40f, FateDarkPink * 0.7f, 0.32f * scale, -1);
                }
                
                // === MUSIC NOTES dancing in the cosmic arc ===
                if (Main.rand.NextBool(2))
                {
                    ThemedParticles.FateMusicNotes(position + direction * 45f, 3, 25f * scale);
                }
            }

            /// <summary>BEAUTIFUL Cosmic trail with chromatic aberration and temporal echoes.</summary>
            public static void Trail(Vector2 position, Vector2 velocity, float scale = 1f)
            {
                // === PRIMARY COSMIC GRADIENT TRAIL ===
                for (int i = 0; i < 4; i++)
                {
                    Color trailColor = GetCosmicGradient(i / 4f);
                    Vector2 offset = Main.rand.NextVector2Circular(6f, 6f);
                    var glow = new GenericGlowParticle(position + offset, -velocity * 0.12f, trailColor, 0.32f * scale, 16, true);
                    MagnumParticleHandler.SpawnParticle(glow);
                }
                
                // === CHROMATIC ABERRATION TRAIL ===
                if (Main.rand.NextBool(2))
                {
                    CustomParticles.GenericFlare(position + new Vector2(-3, 0), new Color(255, 50, 80) * 0.4f, 0.18f * scale, 8);
                    CustomParticles.GenericFlare(position + new Vector2(3, 0), new Color(80, 80, 255) * 0.4f, 0.18f * scale, 8);
                }
                
                // === TEMPORAL ECHO AFTERIMAGES ===
                if (Main.rand.NextBool(3))
                {
                    float echoProgress = Main.rand.NextFloat();
                    Color echoColor = GetCosmicGradient(echoProgress) * 0.5f;
                    CustomParticles.GenericFlare(position - velocity * 0.2f, echoColor, 0.22f * scale, 10);
                }
                
                // === DESTINY GLYPH TRAIL ===
                if (Main.rand.NextBool(5))
                {
                    CustomParticles.GlyphTrail(position, velocity, FateDarkPink * 0.55f, 0.22f * scale);
                }
                
                // === COSMIC MUSIC NOTES echoing ===
                if (Main.rand.NextBool(4))
                {
                    ThemedParticles.FateMusicNotes(position, 1, 12f * scale);
                }
            }

            /// <summary>ULTIMATE cosmic death - REALITY COLLAPSE with maximum spectacle.</summary>
            public static void DeathExplosion(Vector2 position, float scale = 1f)
            {
                // === PHASE 1: REALITY CORE IMPLOSION then EXPLOSION ===
                SpawnFlareShine(position, FateWhite, FateBrightRed, 0f, new Vector2(20f * scale), 80);
                SpawnStrongBloom(position, FateWhite, 7f * scale, 45);
                SpawnStrongBloom(position, FateBrightRed, 5f * scale, 40);
                
                // === PHASE 2: MASSIVE CHROMATIC ABERRATION EXPLOSION ===
                for (int layer = 0; layer < 6; layer++)
                {
                    float separation = (10f + layer * 6f) * scale;
                    float alpha = 0.6f - layer * 0.08f;
                    float layerScale = (1.2f - layer * 0.15f) * scale;
                    
                    // Red channel
                    CustomParticles.GenericFlare(position + new Vector2(-separation, -separation * 0.3f), new Color(255, 50, 80) * alpha, layerScale, 25);
                    // Green channel
                    CustomParticles.GenericFlare(position + new Vector2(separation * 0.3f, -separation * 0.5f), new Color(50, 255, 100) * alpha * 0.7f, layerScale * 0.8f, 22);
                    // Blue channel
                    CustomParticles.GenericFlare(position + new Vector2(separation, separation * 0.2f), new Color(80, 80, 255) * alpha, layerScale, 25);
                }
                
                // === PHASE 3: CASCADING COSMIC GRADIENT RINGS ===
                for (int wave = 0; wave < 12; wave++)
                {
                    float progress = wave / 12f;
                    Color waveColor = GetCosmicGradient(progress);
                    SpawnPulseRing(position, waveColor, 0f, (4f + wave * 1.2f) * scale, 45 + wave * 10);
                }
                
                // === PHASE 4: DOUBLE SPIRAL GALAXY ===
                for (int arm = 0; arm < 10; arm++)
                {
                    float armAngle = MathHelper.TwoPi * arm / 10f;
                    for (int point = 0; point < 12; point++)
                    {
                        float spiralAngle = armAngle + point * 0.5f;
                        float spiralRadius = 35f + point * 22f;
                        Vector2 spiralPos = position + spiralAngle.ToRotationVector2() * spiralRadius * scale;
                        Color galaxyColor = GetCosmicGradient((arm * 12 + point) / 120f);
                        CustomParticles.GenericFlare(spiralPos, galaxyColor, 0.65f + point * 0.05f, 28 + point * 3);
                    }
                }
                
                // === PHASE 5: COSMIC LIGHTNING MAELSTROM ===
                for (int i = 0; i < 30; i++)
                {
                    Vector2 velocity = Main.rand.NextVector2Unit() * Main.rand.NextFloat(25f, 60f) * scale;
                    Color arcColor = GetCosmicGradient((float)i / 30f);
                    SpawnElectricArc(position, velocity, arcColor, 2f * scale, 70);
                }
                
                // === PHASE 6: MASSIVE SPARK SUPERNOVA ===
                for (int i = 0; i < 80; i++)
                {
                    Vector2 velocity = Main.rand.NextVector2Unit() * Main.rand.NextFloat(12f, 55f) * scale;
                    Color sparkColor = GetCosmicGradient(i / 80f);
                    SpawnDirectionalSpark(position, velocity, sparkColor, 80, 2.8f * scale);
                }
                
                // === PHASE 7: DESTINY GLYPH APOCALYPSE ===
                CustomParticles.GlyphCircle(position, FateDarkPink, 20, 120f * scale, 0.025f);
                CustomParticles.GlyphCircle(position, FateBrightRed, 14, 75f * scale, 0.04f);
                CustomParticles.GlyphBurst(position, FateWhite, 20, 14f * scale);
                CustomParticles.GlyphTower(position, FateDarkPink, 7, 0.8f * scale);
                
                // === PHASE 8: THE FINAL SYMPHONY OF FATE ===
                ThemedParticles.FateMusicNoteBurst(position, 28, 12f * scale);
                ThemedParticles.FateMusicNotes(position, 20, 150f * scale);
                
                ExplosionUtility.CreateDeathExplosion(position, FateWhite, FateBrightRed, scale * 2.5f);
                
                Lighting.AddLight(position, FateBrightRed.ToVector3() * 4f * scale);
            }
            
            /// <summary>ELEGANT cosmic ambient aura with temporal shimmer.</summary>
            public static void Aura(Vector2 center, float radius, float scale = 1f)
            {
                // === ORBITING COSMIC MOTES ===
                if (Main.rand.NextBool(3))
                {
                    float angle = Main.GameUpdateCount * 0.025f + Main.rand.NextFloat() * MathHelper.TwoPi;
                    Vector2 motePos = center + angle.ToRotationVector2() * radius * Main.rand.NextFloat(0.5f, 1f);
                    Color moteColor = GetCosmicGradient(Main.rand.NextFloat());
                    var mote = new GenericGlowParticle(motePos, new Vector2(0, -0.4f), moteColor * 0.6f, 0.28f * scale, 28, true);
                    MagnumParticleHandler.SpawnParticle(mote);
                }
                
                // === CHROMATIC SHIMMER ===
                if (Main.rand.NextBool(12))
                {
                    Vector2 shimmerPos = center + Main.rand.NextVector2Circular(radius * 0.7f, radius * 0.7f);
                    CustomParticles.GenericFlare(shimmerPos + new Vector2(-2, 0), new Color(255, 50, 80) * 0.3f, 0.15f * scale, 10);
                    CustomParticles.GenericFlare(shimmerPos + new Vector2(2, 0), new Color(80, 80, 255) * 0.3f, 0.15f * scale, 10);
                }
                
                // === DESTINY GLYPHS ===
                if (Main.rand.NextBool(18))
                {
                    float glyphAngle = Main.GameUpdateCount * 0.018f;
                    Vector2 glyphPos = center + glyphAngle.ToRotationVector2() * radius * 0.85f;
                    CustomParticles.Glyph(glyphPos, FateDarkPink * 0.45f, 0.22f * scale, -1);
                }
                
                // === COSMIC MUSIC NOTES ===
                if (Main.rand.NextBool(22))
                {
                    ThemedParticles.FateMusicNotes(center + Main.rand.NextVector2Circular(radius * 0.5f, radius * 0.5f), 1, 15f);
                }
                
                Lighting.AddLight(center, FateDarkPink.ToVector3() * 0.3f * scale);
            }
            
            /// <summary>SPECTACULAR hit effect for weapon strikes with chromatic burst.</summary>
            public static void HitEffect(Vector2 position, float scale = 1f)
            {
                // === CENTRAL COSMIC FLASH ===
                CustomParticles.GenericFlare(position, FateWhite, 0.8f * scale, 16);
                CustomParticles.GenericFlare(position, FateBrightRed, 0.65f * scale, 18);
                CustomParticles.GenericFlare(position, FateDarkPink, 0.5f * scale, 16);
                
                // === CHROMATIC ABERRATION BURST ===
                CustomParticles.GenericFlare(position + new Vector2(-4, 0), new Color(255, 50, 80) * 0.5f, 0.45f * scale, 12);
                CustomParticles.GenericFlare(position + new Vector2(4, 0), new Color(80, 80, 255) * 0.5f, 0.45f * scale, 12);
                
                // === GRADIENT FRACTAL BURST ===
                for (int i = 0; i < 10; i++)
                {
                    float angle = MathHelper.TwoPi * i / 10f;
                    Vector2 offset = angle.ToRotationVector2() * 32f * scale;
                    Color flareColor = GetCosmicGradient((float)i / 10f);
                    CustomParticles.GenericFlare(position + offset, flareColor, 0.42f * scale, 15);
                    
                    // Chromatic aberration on each flare
                    CustomParticles.GenericFlare(position + offset + new Vector2(-2, 0), FateBrightRed * 0.25f, 0.2f * scale, 8);
                    CustomParticles.GenericFlare(position + offset + new Vector2(2, 0), FatePurple * 0.25f, 0.2f * scale, 8);
                }
                
                // === COSMIC HALOS ===
                for (int i = 0; i < 5; i++)
                {
                    CustomParticles.HaloRing(position, GetCosmicGradient(i / 5f), (0.35f + i * 0.14f) * scale, 14 + i * 4);
                }
                
                // === DESTINY GLYPHS ===
                CustomParticles.GlyphImpact(position, FateDarkPink, FateBrightRed, 0.5f * scale);
                
                // === COSMIC MUSIC NOTES ===
                ThemedParticles.FateMusicNoteBurst(position, 8, 5f * scale);
                
                Lighting.AddLight(position, FateBrightRed.ToVector3() * scale);
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
