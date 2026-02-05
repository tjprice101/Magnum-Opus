using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Common.Systems.VFX
{
    /// <summary>
    /// UNIFIED VFX MANAGER - The central API for all MagnumOpus visual effects.
    /// 
    /// This manager provides a single, easy-to-use interface for:
    /// - Bloom effects (5 styles: Ethereal, Infernal, Celestial, Chromatic, Void)
    /// - Screen distortions (5 styles: Ripple, Shatter, Warp, Pulse, Tear)
    /// - Trail rendering (5 styles: Flame, Ice, Lightning, Nature, Cosmic)
    /// - Bezier curve projectiles
    /// - Theme-aware automatic styling
    /// 
    /// Usage: UnifiedVFXManager.[Theme].[Effect]() or UnifiedVFXManager.Custom.[Effect]()
    /// </summary>
    public static class UnifiedVFXManager
    {
        #region Theme-Specific Effect Classes

        /// <summary>
        /// La Campanella theme effects - Infernal, fiery, bell-like
        /// </summary>
        public static class LaCampanella
        {
            public static void Impact(Vector2 position, float scale = 1f)
                => AdvancedVFXEffects.InfernalImpact(position, MagnumThemePalettes.CampanellaOrange, MagnumThemePalettes.CampanellaGold, scale);

            public static void ScreenEffect(Vector2 position, float intensity = 1f)
                => ScreenDistortionManager.TriggerRipple(position, MagnumThemePalettes.CampanellaOrange, intensity);

            public static int CreateTrail(Vector2 startPos, float width = 25f)
                => AdvancedTrailSystem.CreateFlameTrail(startPos, MagnumThemePalettes.CampanellaOrange, MagnumThemePalettes.CampanellaGold, width);

            public static void ThemedImpact(Vector2 position, float scale = 1f)
                => AdvancedVFXEffects.ThemedImpact("LaCampanella", position, scale);

            public static void BellChime(Vector2 position, float scale = 1f)
            {
                // Special bell chime effect with concentric rings
                for (int i = 0; i < 5; i++)
                {
                    float ringScale = scale * (0.2f + i * 0.15f);
                    int delay = i * 4;
                    CustomParticles.HaloRing(position, Color.Lerp(MagnumThemePalettes.CampanellaOrange, MagnumThemePalettes.CampanellaGold, i / 5f), ringScale, 20 + delay);
                }
                ThemedParticles.MusicNote(position, Vector2.Zero, MagnumThemePalettes.CampanellaGold, 0.8f, 30);
            }
        }

        /// <summary>
        /// Eroica theme effects - Heroic, triumphant, sakura
        /// </summary>
        public static class Eroica
        {
            public static void Impact(Vector2 position, float scale = 1f)
                => AdvancedVFXEffects.CelestialImpact(position, MagnumThemePalettes.EroicaScarlet, MagnumThemePalettes.EroicaGold, scale);

            public static void ScreenEffect(Vector2 position, float intensity = 1f)
                => ScreenDistortionManager.TriggerPulse(position, MagnumThemePalettes.EroicaScarlet, intensity);

            public static int CreateTrail(Vector2 startPos, float width = 22f)
                => AdvancedTrailSystem.CreateFlameTrail(startPos, MagnumThemePalettes.EroicaScarlet, MagnumThemePalettes.EroicaGold, width);

            public static void ThemedImpact(Vector2 position, float scale = 1f)
                => AdvancedVFXEffects.ThemedImpact("Eroica", position, scale);

            public static void HeroicFanfare(Vector2 position, float scale = 1f)
            {
                // Triumphant burst with sakura and stars
                AdvancedVFXEffects.CelestialImpact(position, MagnumThemePalettes.EroicaScarlet, MagnumThemePalettes.EroicaGold, scale);
                ThemedParticles.SakuraPetals(position, 12, 50f * scale);
                for (int i = 0; i < 6; i++)
                {
                    float angle = MathHelper.TwoPi * i / 6f;
                    ThemedParticles.MusicNote(position + angle.ToRotationVector2() * 30f * scale, angle.ToRotationVector2() * 3f, MagnumThemePalettes.EroicaGold, 0.75f, 35);
                }
            }
        }

        /// <summary>
        /// Swan Lake theme effects - Graceful, prismatic, elegant
        /// </summary>
        public static class SwanLake
        {
            public static void Impact(Vector2 position, float scale = 1f)
                => AdvancedVFXEffects.ChromaticImpact(position, scale);

            public static void ScreenEffect(Vector2 position, float intensity = 1f)
                => ScreenDistortionManager.TriggerShatter(position, Color.White, Color.Black, intensity);

            public static int CreateTrail(Vector2 startPos, float width = 20f)
                => AdvancedTrailSystem.CreateIceTrail(startPos, Color.White, new Color(200, 220, 255), width);

            public static void ThemedImpact(Vector2 position, float scale = 1f)
                => AdvancedVFXEffects.ThemedImpact("SwanLake", position, scale);

            public static void GracefulPirouette(Vector2 position, float scale = 1f)
            {
                // Elegant spinning effect with feathers and rainbow
                AdvancedVFXEffects.ChromaticImpact(position, scale);
                ThemedParticles.SwanFeatherBurst(position, 10, 40f * scale);
                for (int i = 0; i < 8; i++)
                {
                    float hue = i / 8f;
                    float angle = MathHelper.TwoPi * i / 8f + Main.GameUpdateCount * 0.05f;
                    CustomParticles.GenericFlare(position + angle.ToRotationVector2() * 35f * scale, Main.hslToRgb(hue, 1f, 0.7f), scale * 0.4f, 20);
                }
            }
        }

        /// <summary>
        /// Moonlight Sonata theme effects - Ethereal, lunar, dreamy
        /// </summary>
        public static class MoonlightSonata
        {
            public static void Impact(Vector2 position, float scale = 1f)
                => AdvancedVFXEffects.EtherealImpact(position, MagnumThemePalettes.MoonlightDarkPurple, MagnumThemePalettes.MoonlightLightBlue, scale);

            public static void ScreenEffect(Vector2 position, float intensity = 1f)
                => ScreenDistortionManager.TriggerRipple(position, MagnumThemePalettes.MoonlightDarkPurple, intensity * 0.7f);

            public static int CreateTrail(Vector2 startPos, float width = 24f)
                => AdvancedTrailSystem.CreateCosmicTrail(startPos, MagnumThemePalettes.MoonlightDarkPurple, MagnumThemePalettes.MoonlightSilver, width);

            public static void ThemedImpact(Vector2 position, float scale = 1f)
                => AdvancedVFXEffects.ThemedImpact("MoonlightSonata", position, scale);

            public static void LunarGlow(Vector2 position, float scale = 1f)
            {
                // Soft moonlit effect
                AdvancedVFXEffects.EtherealImpact(position, MagnumThemePalettes.MoonlightDarkPurple, MagnumThemePalettes.MoonlightLightBlue, scale);
                ThemedParticles.MoonlightMusicNotes(position, 6, 35f * scale);
                
                // Lunar halo
                for (int i = 0; i < 3; i++)
                {
                    CustomParticles.HaloRing(position, MagnumThemePalettes.MoonlightSilver * (0.6f - i * 0.15f), scale * (0.3f + i * 0.2f), 25 + i * 5);
                }
            }
        }

        /// <summary>
        /// Enigma Variations theme effects - Mysterious, void, arcane
        /// </summary>
        public static class Enigma
        {
            public static void Impact(Vector2 position, float scale = 1f)
                => AdvancedVFXEffects.VoidImpact(position, MagnumThemePalettes.EnigmaPurple, MagnumThemePalettes.EnigmaGreenFlame, scale);

            public static void ScreenEffect(Vector2 position, float intensity = 1f)
                => ScreenDistortionManager.TriggerWarp(position, MagnumThemePalettes.EnigmaPurple, intensity);

            public static int CreateTrail(Vector2 startPos, float width = 18f)
                => AdvancedTrailSystem.CreateLightningTrail(startPos, MagnumThemePalettes.EnigmaPurple, width);

            public static void ThemedImpact(Vector2 position, float scale = 1f)
                => AdvancedVFXEffects.ThemedImpact("Enigma", position, scale);

            public static void MysteriousRevelation(Vector2 position, float scale = 1f)
            {
                // Mysterious arcane effect with eyes and glyphs
                AdvancedVFXEffects.VoidImpact(position, MagnumThemePalettes.EnigmaPurple, MagnumThemePalettes.EnigmaGreenFlame, scale);
                CustomParticles.GlyphBurst(position, MagnumThemePalettes.EnigmaPurple, 8, 5f * scale);
                CustomParticles.EnigmaEyeFormation(position, MagnumThemePalettes.EnigmaGreenFlame, 4, 40f * scale);
            }
        }

        /// <summary>
        /// Fate theme effects - Cosmic, celestial, reality-bending
        /// </summary>
        public static class Fate
        {
            public static void Impact(Vector2 position, float scale = 1f)
                => AdvancedVFXEffects.VoidImpact(position, MagnumThemePalettes.FateDarkPink, MagnumThemePalettes.FateBrightRed, scale);

            public static void ScreenEffect(Vector2 position, float intensity = 1f)
                => ScreenDistortionManager.TriggerTear(position, position + Main.rand.NextVector2Circular(100f, 100f), MagnumThemePalettes.FateDarkPink, MagnumThemePalettes.FateBrightRed, intensity);

            public static int CreateTrail(Vector2 startPos, float width = 26f)
                => AdvancedTrailSystem.CreateCosmicTrail(startPos, MagnumThemePalettes.FateDarkPink, MagnumThemePalettes.FateWhite, width);

            public static void ThemedImpact(Vector2 position, float scale = 1f)
                => AdvancedVFXEffects.ThemedImpact("Fate", position, scale);

            public static void CosmicDestiny(Vector2 position, float scale = 1f)
            {
                // Ultimate cosmic effect with glyphs, stars, and reality distortion
                AdvancedVFXEffects.VoidImpact(position, MagnumThemePalettes.FateDarkPink, MagnumThemePalettes.FateBrightRed, scale);
                
                // Orbiting glyphs
                for (int i = 0; i < 6; i++)
                {
                    float angle = MathHelper.TwoPi * i / 6f + Main.GameUpdateCount * 0.03f;
                    Vector2 glyphPos = position + angle.ToRotationVector2() * 45f * scale;
                    CustomParticles.Glyph(glyphPos, MagnumThemePalettes.FateDarkPink, 0.5f * scale, -1);
                }
                
                // Star sparkles
                for (int i = 0; i < 12; i++)
                {
                    Vector2 starPos = position + Main.rand.NextVector2Circular(50f, 50f) * scale;
                    CustomParticles.GenericFlare(starPos, MagnumThemePalettes.FateWhite, 0.3f * scale, 20);
                }
                
                // Screen tear
                ScreenDistortionManager.TriggerTear(position - new Vector2(60, 60) * scale, position + new Vector2(60, 60) * scale, MagnumThemePalettes.FateDarkPink, MagnumThemePalettes.FateBrightRed, scale * 0.4f);
            }
        }

        /// <summary>
        /// Spring theme effects - Fresh, floral, renewal
        /// </summary>
        public static class Spring
        {
            public static void Impact(Vector2 position, float scale = 1f)
                => AdvancedVFXEffects.EtherealImpact(position, new Color(255, 180, 200), new Color(150, 255, 150), scale);

            public static void ScreenEffect(Vector2 position, float intensity = 1f)
                => ScreenDistortionManager.TriggerPulse(position, new Color(255, 200, 220), intensity * 0.6f);

            public static int CreateTrail(Vector2 startPos, float width = 20f)
                => AdvancedTrailSystem.CreateNatureTrail(startPos, new Color(255, 180, 200), new Color(150, 255, 150), width);

            public static void ThemedImpact(Vector2 position, float scale = 1f)
                => AdvancedVFXEffects.ThemedImpact("Spring", position, scale);
        }

        /// <summary>
        /// Summer theme effects - Warm, radiant, vibrant
        /// </summary>
        public static class Summer
        {
            public static void Impact(Vector2 position, float scale = 1f)
                => AdvancedVFXEffects.InfernalImpact(position, new Color(255, 200, 50), new Color(255, 100, 50), scale);

            public static void ScreenEffect(Vector2 position, float intensity = 1f)
                => ScreenDistortionManager.TriggerRipple(position, new Color(255, 200, 100), intensity * 0.8f);

            public static int CreateTrail(Vector2 startPos, float width = 22f)
                => AdvancedTrailSystem.CreateFlameTrail(startPos, new Color(255, 200, 50), new Color(255, 100, 50), width);

            public static void ThemedImpact(Vector2 position, float scale = 1f)
                => AdvancedVFXEffects.ThemedImpact("Summer", position, scale);
        }

        /// <summary>
        /// Autumn theme effects - Warm, falling, nostalgic
        /// </summary>
        public static class Autumn
        {
            public static void Impact(Vector2 position, float scale = 1f)
                => AdvancedVFXEffects.EtherealImpact(position, new Color(200, 100, 50), new Color(180, 150, 80), scale);

            public static void ScreenEffect(Vector2 position, float intensity = 1f)
                => ScreenDistortionManager.TriggerShatter(position, new Color(200, 100, 50), new Color(180, 150, 80), intensity * 0.5f);

            public static int CreateTrail(Vector2 startPos, float width = 20f)
                => AdvancedTrailSystem.CreateNatureTrail(startPos, new Color(200, 100, 50), new Color(180, 150, 80), width);

            public static void ThemedImpact(Vector2 position, float scale = 1f)
                => AdvancedVFXEffects.ThemedImpact("Autumn", position, scale);
        }

        /// <summary>
        /// Winter theme effects - Cold, crystalline, serene
        /// </summary>
        public static class Winter
        {
            public static void Impact(Vector2 position, float scale = 1f)
                => AdvancedVFXEffects.CelestialImpact(position, new Color(180, 220, 255), Color.White, scale);

            public static void ScreenEffect(Vector2 position, float intensity = 1f)
                => ScreenDistortionManager.TriggerShatter(position, new Color(180, 220, 255), Color.White, intensity * 0.7f);

            public static int CreateTrail(Vector2 startPos, float width = 18f)
                => AdvancedTrailSystem.CreateIceTrail(startPos, new Color(180, 220, 255), Color.White, width);

            public static void ThemedImpact(Vector2 position, float scale = 1f)
                => AdvancedVFXEffects.ThemedImpact("Winter", position, scale);
        }

        #endregion

        #region Custom Effects (Non-Theme-Specific)

        /// <summary>
        /// Custom effects that can be used with any colors.
        /// </summary>
        public static class Custom
        {
            // Bloom Effects
            public static void EtherealImpact(Vector2 position, Color primary, Color secondary, float scale = 1f)
                => AdvancedVFXEffects.EtherealImpact(position, primary, secondary, scale);

            public static void InfernalImpact(Vector2 position, Color primary, Color secondary, float scale = 1f)
                => AdvancedVFXEffects.InfernalImpact(position, primary, secondary, scale);

            public static void CelestialImpact(Vector2 position, Color primary, Color secondary, float scale = 1f)
                => AdvancedVFXEffects.CelestialImpact(position, primary, secondary, scale);

            public static void ChromaticImpact(Vector2 position, float scale = 1f)
                => AdvancedVFXEffects.ChromaticImpact(position, scale);

            public static void VoidImpact(Vector2 position, Color primary, Color accent, float scale = 1f)
                => AdvancedVFXEffects.VoidImpact(position, primary, accent, scale);

            // Screen Effects
            public static void Ripple(Vector2 position, Color color, float intensity = 1f, int duration = 30)
                => ScreenDistortionManager.TriggerRipple(position, color, intensity, duration);

            public static void Shatter(Vector2 position, Color primary, Color secondary, float intensity = 1f, int duration = 25)
                => ScreenDistortionManager.TriggerShatter(position, primary, secondary, intensity, duration);

            public static void Warp(Vector2 position, Color color, float intensity = 1f, int duration = 45)
                => ScreenDistortionManager.TriggerWarp(position, color, intensity, duration);

            public static void Pulse(Vector2 position, Color color, float intensity = 1f, int duration = 20)
                => ScreenDistortionManager.TriggerPulse(position, color, intensity, duration);

            public static void Tear(Vector2 start, Vector2 end, Color primary, Color accent, float intensity = 1f, int duration = 40)
                => ScreenDistortionManager.TriggerTear(start, end, primary, accent, intensity, duration);

            // Trail Creation
            public static int FlameTrail(Vector2 startPos, Color flame, Color ember, float width = 25f)
                => AdvancedTrailSystem.CreateFlameTrail(startPos, flame, ember, width);

            public static int IceTrail(Vector2 startPos, Color ice, Color frost, float width = 18f)
                => AdvancedTrailSystem.CreateIceTrail(startPos, ice, frost, width);

            public static int LightningTrail(Vector2 startPos, Color electric, float width = 15f)
                => AdvancedTrailSystem.CreateLightningTrail(startPos, electric, width);

            public static int NatureTrail(Vector2 startPos, Color leaf, Color flower, float width = 22f)
                => AdvancedTrailSystem.CreateNatureTrail(startPos, leaf, flower, width);

            public static int CosmicTrail(Vector2 startPos, Color nebula, Color star, float width = 28f)
                => AdvancedTrailSystem.CreateCosmicTrail(startPos, nebula, star, width);

            // Trail Management
            public static void UpdateTrail(int trailId, Vector2 position, float rotation)
                => AdvancedTrailSystem.UpdateTrail(trailId, position, rotation);

            public static void EndTrail(int trailId)
                => AdvancedTrailSystem.EndTrail(trailId);
        }

        #endregion

        #region Generic Theme Access

        /// <summary>
        /// Triggers a themed impact effect by theme name string.
        /// </summary>
        public static void ThemedImpact(string theme, Vector2 position, float scale = 1f)
            => AdvancedVFXEffects.ThemedImpact(theme, position, scale);

        /// <summary>
        /// Triggers a themed screen effect by theme name string.
        /// </summary>
        public static void ThemedScreenEffect(string theme, Vector2 position, float intensity = 1f)
            => ScreenDistortionManager.TriggerThemeEffect(theme, position, intensity);

        /// <summary>
        /// Creates a themed trail by theme name string.
        /// </summary>
        public static int ThemedTrail(string theme, Vector2 startPosition, float width = 20f)
            => AdvancedTrailSystem.CreateThemeTrail(theme, width);

        #endregion

        #region Boss VFX Integration

        /// <summary>
        /// Ultimate boss attack VFX - combines all effect types.
        /// </summary>
        public static void BossUltimateAttack(string theme, Vector2 position, float scale = 1.5f)
        {
            // Major impact
            AdvancedVFXEffects.ThemedImpact(theme, position, scale);

            // Screen distortion
            ScreenDistortionManager.TriggerThemeEffect(theme, position, scale * 0.5f, 45);

            // Music note explosion
            var palette = MagnumThemePalettes.GetThemePalette(theme);
            Color noteColor = palette.Length > 0 ? palette[0] : Color.White;
            for (int i = 0; i < 12; i++)
            {
                float angle = MathHelper.TwoPi * i / 12f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 6f);
                ThemedParticles.MusicNote(position, vel, noteColor, 0.85f, 40);
            }

            // Screen shake
            MagnumScreenEffects.AddScreenShake(scale * 10f);
        }

        /// <summary>
        /// Boss phase transition VFX.
        /// </summary>
        public static void BossPhaseTransition(string theme, Vector2 position, float scale = 1.2f)
        {
            var palette = MagnumThemePalettes.GetThemePalette(theme);
            Color primary = palette.Length > 0 ? palette[0] : Color.White;
            Color secondary = palette.Length > 1 ? palette[1] : primary;

            // Expanding shockwave
            for (int i = 0; i < 6; i++)
            {
                float ringScale = scale * (0.4f + i * 0.2f);
                Color ringColor = Color.Lerp(primary, secondary, i / 6f);
                CustomParticles.HaloRing(position, ringColor, ringScale, 20 + i * 5);
            }

            // Central flash
            CustomParticles.GenericFlare(position, Color.White, scale * 1.5f, 25);
            CustomParticles.GenericFlare(position, primary, scale * 1.2f, 22);

            // Theme particles
            AdvancedVFXEffects.ThemedImpact(theme, position, scale * 0.8f);

            // Screen effect
            ScreenDistortionManager.TriggerThemeEffect(theme, position, scale * 0.4f, 35);
        }

        /// <summary>
        /// Boss death explosion VFX.
        /// </summary>
        public static void BossDeathExplosion(string theme, Vector2 position, float scale = 2f)
        {
            var palette = MagnumThemePalettes.GetThemePalette(theme);
            Color primary = palette.Length > 0 ? palette[0] : Color.White;
            Color secondary = palette.Length > 1 ? palette[1] : primary;

            // Multi-phase explosion
            for (int phase = 0; phase < 3; phase++)
            {
                float phaseScale = scale * (1f + phase * 0.3f);
                float delay = phase * 8f;

                // Central flashes
                CustomParticles.GenericFlare(position, Color.White, phaseScale * 1.2f, (int)(20 + delay));
                CustomParticles.GenericFlare(position, primary, phaseScale, (int)(18 + delay));

                // Expanding rings
                for (int i = 0; i < 4; i++)
                {
                    Color ringColor = Color.Lerp(primary, secondary, (phase * 4 + i) / 12f);
                    CustomParticles.HaloRing(position, ringColor, phaseScale * (0.3f + i * 0.15f), (int)(22 + delay + i * 3));
                }
            }

            // Massive particle spray
            for (int i = 0; i < 30; i++)
            {
                float angle = MathHelper.TwoPi * i / 30f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(5f, 12f);
                Color particleColor = Color.Lerp(primary, secondary, Main.rand.NextFloat());
                var particle = new GenericGlowParticle(position, vel, particleColor, 0.4f * scale, 35, true);
                MagnumParticleHandler.SpawnParticle(particle);
            }

            // Music note finale
            for (int i = 0; i < 16; i++)
            {
                float angle = MathHelper.TwoPi * i / 16f;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 8f);
                ThemedParticles.MusicNote(position, vel, primary, 0.9f, 45);
            }

            // Major screen effect
            ScreenDistortionManager.TriggerThemeEffect(theme, position, scale * 0.6f, 50);

            // Screen shake
            MagnumScreenEffects.AddScreenShake(scale * 15f);
        }

        #endregion
    }
}
