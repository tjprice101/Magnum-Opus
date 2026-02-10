using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Common.Systems.VFX
{
    /// <summary>
    /// Extension methods that integrate the advanced VFX systems
    /// (GodRays, KineticRipples, DimensionalTears, Constellations, FractalShatter)
    /// into the existing GlobalVFXOverhaul, GlobalWeaponVFXOverhaul, and GlobalBossVFXOverhaul systems.
    /// 
    /// ACTUAL API SIGNATURES:
    /// - GodRaySystem.CreateBurst(position, primaryColor, rayCount=16, radius=150f, duration=45, style, secondaryColor?)
    /// - GodRaySystem.CreateLightSource(position, color, rayCount=24, radius=100f, duration=120)
    /// - GodRaySystem.CreateUniverseSplitterBurst(position, primaryColor, secondaryColor, duration=60)
    /// - KineticRippleSystem.CreateRipple(center, maxRadius=400f, intensity=1f, color?, style, lifetime=40)
    /// - KineticRippleSystem.CreateImpact(position, intensity=1f, color?)
    /// - KineticRippleSystem.CreateShockwave(position, intensity=1f, color?)
    /// - KineticRippleSystem.CreateChromaticRipple(position, intensity=1f)
    /// - KineticRippleSystem.CreatePulse(position, pulseCount=3, color?)
    /// - DimensionalTearSystem.CreateTear(startPos, endPos, width=40f, style, lifetime=60)
    /// - DimensionalTearSystem.CreateSwingTear(center, radius, startAngle, endAngle, style, lifetime=45)
    /// - DimensionalTearSystem.CreateFateTear(start, end, lifetime=50)
    /// - VerletConstellationSystem.CreateLine(startAnchor, endAnchor, segments=10, color, stiffness=0.5f, lifetime=120)
    /// - VerletConstellationSystem.CreateWeb(center, anchorPoints[], color, stiffness=0.5f, lifetime=120)
    /// - FractalShatterSystem.CreateShatter(originPos, targetPos, shardCount=50, primaryColor?, secondaryColor?, reformToPlayer=true, playerIndex=0)
    /// </summary>
    public static class AdvancedVFXExtensions
    {
        #region Rendering Integration
        
        /// <summary>
        /// Draws all active VFX systems. Call from a draw hook.
        /// </summary>
        public static void DrawAllSystems(SpriteBatch spriteBatch)
        {
            GodRaySystem.RenderAll(spriteBatch);
            KineticRippleSystem.RenderAll(spriteBatch);
            DimensionalTearSystem.RenderAll(spriteBatch);
            VerletConstellationSystem.RenderAll(spriteBatch);
            FractalShatterSystem.RenderAll(spriteBatch);
            AlphaErosionSystem.RenderAll(spriteBatch);
        }
        
        #endregion
        
        #region Extended Projectile VFX
        
        /// <summary>
        /// Apply extended projectile VFX using new systems.
        /// Call from GlobalVFXOverhaul.AI() for enhanced projectile effects.
        /// </summary>
        public static void ApplyExtendedProjectileVFX(Projectile projectile, string theme, float intensity = 1f)
        {
            Color primary = GetThemeColor(theme);
            
            // === CONSTELLATION TRAIL FOR MAGICAL THEMES ===
            if (IsMoonlightTheme(theme) || IsFateTheme(theme))
            {
                if (projectile.oldPos.Length >= 2 && Main.rand.NextBool(8))
                {
                    Vector2 startPos = projectile.oldPos[1];
                    Vector2 endPos = projectile.Center;
                    
                    if (startPos != Vector2.Zero && endPos != Vector2.Zero)
                    {
                        VerletConstellationSystem.CreateLine(startPos, endPos, 3, primary, 0.5f, 30);
                    }
                }
            }
            
            // === KINETIC RIPPLE PERIODIC FOR POWERFUL PROJECTILES ===
            if (projectile.damage > 50 && projectile.timeLeft % 20 == 0)
            {
                KineticRippleSystem.CreateRipple(projectile.Center, 80f * intensity, 0.25f, primary, 
                    KineticRippleSystem.RippleStyle.Impact, 20);
            }
        }
        
        /// <summary>
        /// Apply extended death VFX to projectile using new systems.
        /// Call from GlobalVFXOverhaul.OnKill() for enhanced death effects.
        /// </summary>
        public static void ApplyExtendedProjectileDeathVFX(Projectile projectile, string theme, float scale = 1f)
        {
            Color primary = GetThemeColor(theme);
            
            // === GOD RAYS FOR POWERFUL PROJECTILES ===
            if (projectile.damage > 60)
            {
                GodRaySystem.CreateBurst(projectile.Center, primary, 4, 80f * scale, 18);
            }
            
            // === KINETIC IMPACT RIPPLE ===
            KineticRippleSystem.CreateImpact(projectile.Center, 0.4f * scale, primary);
            
            // === DIMENSIONAL TEAR FOR FATE PROJECTILES ===
            if (IsFateTheme(theme) && projectile.damage > 40)
            {
                Vector2 tearEnd = projectile.Center + projectile.velocity.SafeNormalize(Vector2.UnitX) * 50f * scale;
                DimensionalTearSystem.CreateFateTear(projectile.Center, tearEnd, 25);
            }
        }
        
        /// <summary>
        /// Apply extended impact VFX when projectile hits enemy.
        /// Call from GlobalVFXOverhaul.OnHitNPC() for enhanced impact effects.
        /// </summary>
        public static void ApplyExtendedProjectileImpactVFX(Projectile projectile, NPC target, string theme, float scale = 1f)
        {
            Color primary = GetThemeColor(theme);
            Color secondary = GetSecondaryColor(theme);
            int damage = projectile.damage;
            
            // === KINETIC IMPACT RIPPLE ===
            float rippleIntensity = Math.Clamp(damage / 100f, 0.2f, 0.8f) * scale;
            KineticRippleSystem.CreateImpact(target.Center, rippleIntensity, primary);
            
            // === GOD RAYS FOR HIGH DAMAGE HITS ===
            if (damage > 80)
            {
                GodRaySystem.CreateBurst(target.Center, primary, 3, 60f * scale, 15);
            }
            
            // === FRACTAL SHATTER ON CRITICAL HITS ===
            // CreateShatter(originPos, targetPos, shardCount, primaryColor?, secondaryColor?, reformToPlayer, playerIndex)
            if (projectile.CritChance > 0 && Main.rand.Next(100) < projectile.CritChance)
            {
                FractalShatterSystem.CreateShatter(target.Center, projectile.Center, 12, primary, secondary, false, 0);
            }
        }
        
        #endregion
        
        #region Extended Weapon VFX
        
        /// <summary>
        /// Apply weapon glint effect during holding/swinging.
        /// Call from GlobalWeaponVFXOverhaul for specular shine.
        /// </summary>
        public static void ApplyWeaponGlintVFX(SpriteBatch spriteBatch, Item item, Player player, string theme)
        {
            if (!item.CountsAsClass(DamageClass.Melee) && !item.CountsAsClass(DamageClass.Ranged)) return;
            if (spriteBatch == null) return;
            
            Vector2 weaponPos = player.itemLocation + new Vector2(item.width * 0.5f * player.direction, 0);
            
            Color glintColor = GetThemeColor(theme);
            float glintIntensity = GetGlintIntensityForTheme(theme);
            
            if (player.itemAnimation > 0)
            {
                WeaponGlintSystem.DrawSpecularHighlight(spriteBatch, weaponPos, glintColor, glintIntensity, 0.6f);
            }
        }
        
        /// <summary>
        /// Apply extended melee swing VFX.
        /// Call from GlobalWeaponVFXOverhaul for enhanced melee effects.
        /// </summary>
        public static void ApplyExtendedMeleeSwingVFX(Item item, Player player, string theme, float swingProgress)
        {
            Color primary = GetThemeColor(theme);
            Vector2 swingCenter = player.Center;
            float swingRadius = item.width + 40f;
            
            // === DIMENSIONAL TEAR FOR POWERFUL SWINGS ===
            if (IsFateTheme(theme) && item.damage > 60 && swingProgress > 0.3f && swingProgress < 0.7f)
            {
                if (Main.rand.NextBool(5))
                {
                    float angle = player.itemRotation;
                    // CreateSwingTear(center, radius, startAngle, endAngle, style, lifetime)
                    DimensionalTearSystem.CreateSwingTear(swingCenter, swingRadius, angle - 0.3f, angle + 0.3f, 
                        DimensionalTearSystem.TearStyle.Fate, 30);
                }
            }
            
            // === CONSTELLATION LINES FOR MAGICAL MELEE ===
            if (IsMoonlightTheme(theme) && Main.rand.NextBool(6))
            {
                Vector2 startPos = swingCenter;
                Vector2 endPos = swingCenter + player.itemRotation.ToRotationVector2() * swingRadius;
                VerletConstellationSystem.CreateLine(startPos, endPos, 4, primary, 0.6f, 20);
            }
            
            // === KINETIC RIPPLE AT SWING APEX ===
            if (swingProgress > 0.45f && swingProgress < 0.55f && item.damage > 40)
            {
                KineticRippleSystem.CreateRipple(swingCenter, 100f, 0.3f, primary, 
                    KineticRippleSystem.RippleStyle.Impact, 15);
            }
            
            // === GOD RAYS FOR FINISHER SWINGS ===
            if (swingProgress > 0.85f && item.damage > 80)
            {
                GodRaySystem.CreateBurst(swingCenter, primary, 3, 80f, 12);
            }
        }
        
        /// <summary>
        /// Apply extended kill VFX when weapon kills an enemy.
        /// Call from GlobalWeaponVFXOverhaul for spectacular kill effects.
        /// </summary>
        public static void ApplyWeaponKillVFX(NPC target, Item item, string theme, float scale = 1f)
        {
            Color primary = GetThemeColor(theme);
            Color secondary = GetSecondaryColor(theme);
            Vector2 center = target.Center;
            
            // === FRACTAL SHATTER EXPLOSION ===
            int shardCount = 15 + (int)(target.lifeMax / 100f);
            shardCount = Math.Clamp(shardCount, 8, 30);
            // CreateShatter(originPos, targetPos, shardCount, primaryColor?, secondaryColor?, reformToPlayer, playerIndex)
            FractalShatterSystem.CreateShatter(center, center, shardCount, primary, secondary, false, 0);
            
            // === GOD RAYS FOR DRAMATIC KILL ===
            GodRaySystem.CreateBurst(center, primary, 6, 120f * scale, 30);
            
            // === KINETIC SHOCKWAVE ===
            KineticRippleSystem.CreateShockwave(center, 0.8f * scale, primary);
        }
        
        #endregion
        
        #region Extended Boss VFX
        
        /// <summary>
        /// Apply extended ambient VFX around boss.
        /// Call from GlobalBossVFXOverhaul for enhanced boss aura effects.
        /// </summary>
        public static void ApplyExtendedBossAmbientVFX(NPC boss, string theme)
        {
            if (Main.rand.NextBool(60)) return;
            
            Color primary = GetThemeColor(theme);
            
            // === CONSTELLATION WEB AROUND BOSS ===
            if (IsMoonlightTheme(theme) || IsFateTheme(theme))
            {
                Vector2[] points = new Vector2[4];
                for (int i = 0; i < 4; i++)
                {
                    float angle = MathHelper.TwoPi * i / 4f + Main.GameUpdateCount * 0.01f;
                    points[i] = boss.Center + angle.ToRotationVector2() * (boss.width + 80f);
                }
                VerletConstellationSystem.CreateWeb(boss.Center, points, primary, 0.3f, 90);
            }
            
            // === DIMENSIONAL TEAR FLICKERS ===
            if (IsEnigmaTheme(theme) && Main.rand.NextBool(30))
            {
                Vector2 tearStart = boss.Center + Main.rand.NextVector2Circular(boss.width, boss.height);
                Vector2 tearEnd = tearStart + Main.rand.NextVector2Unit() * 60f;
                // CreateTear(startPos, endPos, width, style, lifetime)
                DimensionalTearSystem.CreateTear(tearStart, tearEnd, 20f, DimensionalTearSystem.TearStyle.Void, 25);
            }
            
            // === GOD RAY LIGHT SOURCE ===
            if (Main.rand.NextBool(90))
            {
                GodRaySystem.CreateLightSource(boss.Center + Main.rand.NextVector2Circular(40f, 40f), primary, 8, 80f, 60);
            }
        }
        
        /// <summary>
        /// Apply extended attack windup VFX.
        /// Call from GlobalBossVFXOverhaul for enhanced boss attack telegraphs.
        /// </summary>
        public static void ApplyExtendedBossWindupVFX(NPC boss, float progress, string theme)
        {
            Color primary = GetThemeColor(theme);
            
            // === KINETIC PULSE BUILDUP ===
            if (progress > 0.5f && Main.rand.NextBool(3))
            {
                KineticRippleSystem.CreatePulse(boss.Center, 1, primary);
            }
            
            // === CONSTELLATION FORMING ===
            if (IsFateTheme(theme) && progress > 0.3f)
            {
                float angle = progress * MathHelper.TwoPi;
                Vector2 nodePos = boss.Center + angle.ToRotationVector2() * (80f + progress * 40f);
                if (Main.rand.NextBool(4))
                {
                    VerletConstellationSystem.CreateLine(boss.Center, nodePos, 5, primary, 0.4f, 30);
                }
            }
            
            // === DIMENSIONAL TEAR FLICKERING ===
            if (progress > 0.7f && Main.rand.NextBool(5))
            {
                Vector2 tearStart = boss.Center + Main.rand.NextVector2Unit() * 60f;
                Vector2 tearEnd = tearStart + Main.rand.NextVector2Unit() * 40f;
                DimensionalTearSystem.CreateTear(tearStart, tearEnd, 15f, GetTearStyleForTheme(theme), 20);
            }
        }
        
        /// <summary>
        /// Apply extended attack release VFX.
        /// Call from GlobalBossVFXOverhaul for enhanced boss attack release effects.
        /// </summary>
        public static void ApplyExtendedBossReleaseVFX(NPC boss, string theme, float scale = 1f)
        {
            Color primary = GetThemeColor(theme);
            Color secondary = GetSecondaryColor(theme);
            
            // === GOD RAY BURST ===
            GodRaySystem.CreateBurst(boss.Center, primary, 8, 150f * scale, 25, 
                GodRaySystem.GodRayStyle.Explosion, secondary);
            
            // === KINETIC SHOCKWAVE ===
            KineticRippleSystem.CreateShockwave(boss.Center, 1f * scale, primary);
            
            // === FRACTAL SHATTER FOR POWERFUL ATTACKS ===
            if (Main.rand.NextBool(3))
            {
                FractalShatterSystem.CreateShatter(boss.Center, boss.Center, 10, primary, secondary, false, 0);
            }
        }
        
        /// <summary>
        /// Apply extended dash trail VFX.
        /// Call from GlobalBossVFXOverhaul for enhanced boss dash effects.
        /// </summary>
        public static void ApplyExtendedBossDashVFX(NPC boss, Vector2 velocity, string theme)
        {
            Color primary = GetThemeColor(theme);
            
            // === KINETIC RIPPLE TRAIL ===
            if (velocity.Length() > 10f)
            {
                KineticRippleSystem.CreateRipple(boss.Center, 120f, 0.5f, primary, 
                    KineticRippleSystem.RippleStyle.Impact, 20);
            }
            
            // === DIMENSIONAL TEAR FOR FATE/ENIGMA DASHES ===
            if ((IsFateTheme(theme) || IsEnigmaTheme(theme)) && velocity.Length() > 15f)
            {
                // CreateSwingTear(center, radius, startAngle, endAngle, style, lifetime)
                DimensionalTearSystem.CreateSwingTear(boss.Center, 50f, velocity.ToRotation() - 0.5f, velocity.ToRotation() + 0.5f,
                    GetTearStyleForTheme(theme), 25);
            }
        }
        
        /// <summary>
        /// Apply extended boss death VFX.
        /// Call from GlobalBossVFXOverhaul for spectacular boss death effects.
        /// </summary>
        public static void ApplyExtendedBossDeathVFX(NPC boss, string theme, float scale = 1f)
        {
            Color primary = GetThemeColor(theme);
            Color secondary = GetSecondaryColor(theme);
            Vector2 center = boss.Center;
            
            // === UNIVERSE SPLITTER GOD RAYS ===
            // CreateUniverseSplitterBurst(position, primaryColor, secondaryColor, duration=60)
            GodRaySystem.CreateUniverseSplitterBurst(center, primary, secondary, (int)(90 * scale));
            
            // === MASSIVE SHOCKWAVE ===
            KineticRippleSystem.CreateShockwave(center, 2f * scale, primary);
            
            // === CHROMATIC ABERRATION PULSE ===
            KineticRippleSystem.CreateChromaticRipple(center, 1.5f * scale);
            
            // === FRACTAL SHATTER EXPLOSION ===
            FractalShatterSystem.CreateShatter(center, center, 30, primary, secondary, false, 0);
            
            // === CONSTELLATION SUPERNOVA ===
            if (IsMoonlightTheme(theme) || IsFateTheme(theme))
            {
                Vector2[] novaPoints = new Vector2[8];
                for (int i = 0; i < 8; i++)
                {
                    float angle = MathHelper.TwoPi * i / 8f;
                    novaPoints[i] = center + angle.ToRotationVector2() * 200f * scale;
                }
                VerletConstellationSystem.CreateWeb(center, novaPoints, primary, 0.8f, 60);
            }
            
            // === REALITY TEARS ===
            for (int i = 0; i < 4; i++)
            {
                float angle = MathHelper.TwoPi * i / 4f;
                Vector2 tearStart = center;
                Vector2 tearEnd = center + angle.ToRotationVector2() * 150f * scale;
                DimensionalTearSystem.CreateTear(tearStart, tearEnd, 50f, GetTearStyleForTheme(theme), 60);
            }
        }
        
        /// <summary>
        /// Apply extended phase transition VFX.
        /// Call from GlobalBossVFXOverhaul for dramatic phase changes.
        /// </summary>
        public static void ApplyExtendedBossPhaseTransitionVFX(NPC boss, string theme, float scale = 1f)
        {
            Color primary = GetThemeColor(theme);
            Color secondary = GetSecondaryColor(theme);
            
            // === GOD RAY BURST ===
            GodRaySystem.CreateBurst(boss.Center, primary, 12, 200f * scale, 45, 
                GodRaySystem.GodRayStyle.Spiral, secondary);
            
            // === KINETIC SHOCKWAVE ===
            KineticRippleSystem.CreateShockwave(boss.Center, 1.2f * scale, primary);
            
            // === CONSTELLATION FLASH ===
            if (IsFateTheme(theme) || IsMoonlightTheme(theme))
            {
                Vector2[] points = new Vector2[6];
                for (int i = 0; i < 6; i++)
                {
                    float angle = MathHelper.TwoPi * i / 6f;
                    points[i] = boss.Center + angle.ToRotationVector2() * 120f * scale;
                }
                VerletConstellationSystem.CreateWeb(boss.Center, points, primary, 0.7f, 40);
            }
            
            // === FRACTAL BURST ===
            FractalShatterSystem.CreateShatter(boss.Center, boss.Center, 15, primary, secondary, false, 0);
        }
        
        #endregion
        
        #region Theme Helpers
        
        private static Color GetThemeColor(string theme)
        {
            return theme switch
            {
                "Eroica" => new Color(200, 50, 50),
                "LaCampanella" => new Color(255, 140, 40),
                "MoonlightSonata" => new Color(140, 100, 200),
                "SwanLake" => Color.White,
                "EnigmaVariations" => new Color(140, 60, 200),
                "Fate" => new Color(180, 40, 80),
                "ClairDeLune" => new Color(140, 170, 220),
                "Spring" => new Color(255, 180, 200),
                "Summer" => new Color(255, 200, 100),
                "Autumn" => new Color(200, 120, 60),
                "Winter" => new Color(180, 220, 255),
                _ => Color.White
            };
        }
        
        private static Color GetSecondaryColor(string theme)
        {
            return theme switch
            {
                "Eroica" => new Color(255, 215, 0),
                "LaCampanella" => new Color(255, 200, 50),
                "MoonlightSonata" => new Color(135, 206, 250),
                "SwanLake" => new Color(30, 30, 40),
                "EnigmaVariations" => new Color(50, 220, 100),
                "Fate" => new Color(255, 60, 80),
                "ClairDeLune" => new Color(240, 240, 250),
                "Spring" => new Color(150, 220, 150),
                "Summer" => new Color(255, 140, 50),
                "Autumn" => new Color(180, 60, 40),
                "Winter" => new Color(220, 240, 255),
                _ => Color.Gray
            };
        }
        
        private static bool IsFateTheme(string theme) => 
            theme == "Fate" || theme?.Contains("Fate") == true;
        
        private static bool IsEnigmaTheme(string theme) => 
            theme == "EnigmaVariations" || theme?.Contains("Enigma") == true;
        
        private static bool IsMoonlightTheme(string theme) => 
            theme == "MoonlightSonata" || theme?.Contains("Moonlight") == true;
        
        private static float GetGlintIntensityForTheme(string theme)
        {
            return theme switch
            {
                "SwanLake" => 1.2f,
                "Fate" => 1.0f,
                "MoonlightSonata" => 0.9f,
                "LaCampanella" => 1.1f,
                "Eroica" => 1.0f,
                _ => 0.8f
            };
        }
        
        private static DimensionalTearSystem.TearStyle GetTearStyleForTheme(string theme)
        {
            return theme switch
            {
                "Fate" => DimensionalTearSystem.TearStyle.Fate,
                "EnigmaVariations" => DimensionalTearSystem.TearStyle.Void,
                "LaCampanella" => DimensionalTearSystem.TearStyle.Infernal,
                "SwanLake" => DimensionalTearSystem.TearStyle.Prismatic,
                _ => DimensionalTearSystem.TearStyle.Cosmic
            };
        }
        
        #endregion
    }
}
