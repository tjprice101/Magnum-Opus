using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.Particles;
using static MagnumOpus.Common.Systems.Particles.Particle;

namespace MagnumOpus.Common.Systems.VFX
{
    /// <summary>
    /// GLOBAL WEAPON VFX OVERHAUL
    /// 
    /// Automatically applies Calamity-style buttery-smooth VFX to ALL MagnumOpus weapons.
    /// 
    /// Features:
    /// - Smooth melee swing arcs with primitive trails
    /// - EXO BLADE-STYLE PIECEWISE ANIMATION for "snap" effect
    /// - Ethereal aura effects while holding weapons
    /// - Dynamic muzzle flash for ranged weapons
    /// - Magic channeling effects
    /// - Summon spawn spectacles
    /// - Music note integration throughout
    /// </summary>
    public class GlobalWeaponVFXOverhaul : GlobalItem
    {
        // Track weapon swing state per player
        private static Dictionary<int, WeaponSwingState> _swingStates = new Dictionary<int, WeaponSwingState>();
        
        // Current theme for weapon glint (used in drawing hooks)
        private static string _currentTheme = "";
        
        #region Exo Blade Style Swing Curves
        
        /// <summary>
        /// Exo Blade-style piecewise swing curves for "buttery smooth snap" effect.
        /// SlowStart → FastSwing → EaseOut creates the signature premium feel.
        /// </summary>
        private static readonly CurveSegment[] ExoBladeSwingCurve = new CurveSegment[]
        {
            // 0% - 25%: Slow anticipation buildup (SineBump for subtle wobble)
            new CurveSegment(EasingType.SineBump, 0f, 0f, 0.15f),
            // 25% - 75%: Fast aggressive swing (PolyIn power 3 for acceleration)
            new CurveSegment(EasingType.PolyIn, 0.25f, 0.15f, 0.7f, 3),
            // 75% - 100%: Quick deceleration snap (PolyOut for smooth landing)
            new CurveSegment(EasingType.PolyOut, 0.75f, 0.85f, 0.15f, 2)
        };
        
        /// <summary>
        /// Applies Exo Blade-style piecewise animation to raw swing progress.
        /// Transforms linear 0-1 progress into buttery-smooth curved motion.
        /// </summary>
        private static float GetEasedSwingProgress(float linearProgress)
        {
            return PiecewiseAnimation(MathHelper.Clamp(linearProgress, 0f, 0.999f), ExoBladeSwingCurve);
        }
        
        #endregion
        
        private class WeaponSwingState
        {
            public int TrailId = -1;
            public float SwingProgress = 0f;
            public float EasedSwingProgress = 0f; // NEW: Exo Blade-style eased progress
            public Vector2 SwingDirection = Vector2.UnitX;
            public string Theme = "generic";
            public bool IsSwinging = false;
            public int SwingStartTime = 0;
        }

        public override bool InstancePerEntity => false;

        public override bool AppliesToEntity(Item entity, bool lateInstantiation)
        {
            // MASTER TOGGLE: When disabled, this global system does nothing
            // Each weapon implements its own unique VFX instead
            if (!VFXMasterToggle.GlobalSystemsEnabled)
                return false;
            
            if (entity.ModItem?.Mod != ModContent.GetInstance<MagnumOpus>())
                return false;
            
            // EXCLUDE debug weapons - they handle their own VFX
            if (VFXExclusionHelper.ShouldExcludeItem(entity))
                return false;
            
            return true;
        }

        public override void HoldItem(Item item, Player player)
        {
            if (item.ModItem?.Mod != ModContent.GetInstance<MagnumOpus>())
                return;
            
            string theme = DetectThemeFromItem(item);
            
            // Apply holding effects based on item type
            if (item.CountsAsClass(DamageClass.Melee))
            {
                ApplyMeleeHoldingEffect(player, theme, item);
            }
            else if (item.CountsAsClass(DamageClass.Ranged))
            {
                ApplyRangedHoldingEffect(player, theme, item);
            }
            else if (item.CountsAsClass(DamageClass.Magic))
            {
                ApplyMagicHoldingEffect(player, theme, item);
            }
            else if (item.CountsAsClass(DamageClass.Summon))
            {
                ApplySummonHoldingEffect(player, theme, item);
            }
            else
            {
                // Generic holding effect for other items
                ApplyGenericHoldingEffect(player, theme);
            }
        }

        /// <summary>
        /// CRITICAL: Hooks into UseStyle to coordinate with the interpolated swing system.
        /// The actual swing animation is now handled by InterpolatedMeleeSwingPlayer/InterpolatedMeleeDrawLayer.
        /// This method just ensures the vanilla drawing is suppressed.
        /// </summary>
        public override void UseStyle(Item item, Player player, Rectangle heldItemFrame)
        {
            // Only apply to MagnumOpus melee weapons using swing style
            if (item.ModItem?.Mod != ModContent.GetInstance<MagnumOpus>())
                return;
            
            if (!item.CountsAsClass(DamageClass.Melee))
                return;
            
            // Only apply to swing-style weapons (most melee weapons)
            if (item.useStyle != ItemUseStyleID.Swing)
                return;
            
            // Skip weapons that handle their own graphics
            if (item.noUseGraphic)
                return;
            
            // The InterpolatedMeleeSwingPlayer handles all the swing calculations and rendering.
            // We just need to ensure it's active - it reads from player.itemAnimation directly.
            // The InterpolatedMeleeDrawLayer draws the weapon with sub-frame interpolation.
            
            // Get the swing player to sync state
            var swingPlayer = player.GetModPlayer<InterpolatedMeleeSwingPlayer>();
            
            // If the swing player is handling this, it will set player.itemRotation and player.itemLocation
            // The vanilla drawing will still happen but we hide it via the draw layer system
        }

        public override void UseAnimation(Item item, Player player)
        {
            if (item.ModItem?.Mod != ModContent.GetInstance<MagnumOpus>())
                return;
            
            string theme = DetectThemeFromItem(item);
            
            // Apply use animation effects
            if (item.CountsAsClass(DamageClass.Melee))
            {
                ApplyMeleeSwingEffect(player, theme, item);
            }
            else if (item.CountsAsClass(DamageClass.Ranged))
            {
                ApplyRangedFireEffect(player, theme, item);
            }
            else if (item.CountsAsClass(DamageClass.Magic))
            {
                ApplyMagicCastEffect(player, theme, item);
            }
        }

        #region Melee Effects

        private void ApplyMeleeHoldingEffect(Player player, string theme, Item item)
        {
            // Minimal holding effect - just subtle lighting
            // Old cluttering orbiting particles and weapon glow removed
            var palette = MagnumThemePalettes.GetThemePalette(theme);
            Color primary = palette != null && palette.Length > 0 ? palette[0] : Color.White;
            
            // Just add subtle lighting to the weapon - no particles
            Vector2 itemPos = player.itemLocation + new Vector2(item.width * 0.5f * player.direction, 0);
            Lighting.AddLight(itemPos, primary.ToVector3() * 0.3f);
            
            // === WEAPON GLINT SYSTEM ===
            // Note: Specular shine effect is applied in PreDrawInInventory/PreDrawInWorld
            // Here we just store the theme for later use
            _currentTheme = theme;
        }

        private void ApplyMeleeSwingEffect(Player player, string theme, Item item)
        {
            // Get or create swing state
            if (!_swingStates.TryGetValue(player.whoAmI, out var state))
            {
                state = new WeaponSwingState();
                _swingStates[player.whoAmI] = state;
            }
            
            // Calculate swing progress - RAW linear progress
            float linearProgress = 1f - (float)player.itemAnimation / player.itemAnimationMax;
            
            // === EXO BLADE-STYLE EASING ===
            // Apply piecewise animation curve for buttery-smooth "snap" effect
            // SlowStart (0-25%) → FastSwing (25-75%) → EaseOut (75-100%)
            float useProgress = GetEasedSwingProgress(linearProgress);
            
            // === GET WEAPON-SPECIFIC VARIATION ===
            // Each weapon gets unique whipping/stretching parameters
            var swingVariation = CalamityStyleVFX.GetVariationForItem(item);
            
            // Start new swing
            if (!state.IsSwinging && player.itemAnimation == player.itemAnimationMax - 1)
            {
                state.IsSwinging = true;
                state.SwingStartTime = (int)Main.GameUpdateCount;
                state.Theme = theme;
                state.SwingDirection = (Main.MouseWorld - player.Center).SafeNormalize(Vector2.UnitX * player.direction);
                
                // Create swing trail
                var palette = MagnumThemePalettes.GetThemePalette(theme);
                Color primary = palette != null && palette.Length > 0 ? palette[0] : Color.White;
                state.TrailId = AdvancedTrailSystem.CreateThemeTrail(theme, 25f, maxPoints: 15, intensity: 1.3f);
            }
            
            // During swing
            if (state.IsSwinging && player.itemAnimation > 0)
            {
                state.SwingProgress = linearProgress; // Store raw progress
                state.EasedSwingProgress = useProgress; // Store eased progress
                
                // === CalamityStyleVFX for buttery-smooth particle swing ===
                // Now uses WEAPON-SPECIFIC VARIATION for unique feel per weapon!
                // Each weapon gets its own whipping motion, stretch amount, particle intensity, etc.
                CalamityStyleVFX.SmoothMeleeSwing(player, theme, useProgress, state.SwingDirection, swingVariation);
                
                // === UNIQUE TRAIL STYLES - DISABLED ===
                // CalamityStyleVFX already handles the main trail particles.
                // UniqueTrailStyles was causing excessive flare buildup behind swinging weapons.
                // var palette = MagnumThemePalettes.GetThemePalette(theme);
                // DamageClass damageClass = DamageClass.Melee;
                // Vector2 tipPos = player.Center + state.SwingDirection.RotatedBy((useProgress - 0.5f) * 1.3f) * 60f;
                // Vector2 swingVel = state.SwingDirection.RotatedBy(MathHelper.PiOver2 * player.direction) * 8f;
                // UniqueTrailStyles.SpawnUniqueTrail(tipPos, swingVel, theme, damageClass, palette);
                
                // Get tip position and velocity for remaining effects
                var palette = MagnumThemePalettes.GetThemePalette(theme);
                Vector2 tipPos = player.Center + state.SwingDirection.RotatedBy((useProgress - 0.5f) * 1.3f) * 60f;
                Vector2 swingVel = state.SwingDirection.RotatedBy(MathHelper.PiOver2 * player.direction) * 8f;
                
                // === BÉZIER CURVE PARTICLE RIVER - DISABLED ===
                // This was adding even more particles on top of CalamityStyleVFX, causing visual noise.
                // if (useProgress > 0.15f && useProgress < 0.85f && Main.rand.NextBool(3))
                // {
                //     float swingRadius = item.Size.Length() * 0.8f + 40f;
                //     float startAngle = player.direction == 1 ? -MathHelper.PiOver2 - MathHelper.PiOver4 : MathHelper.PiOver2 + MathHelper.PiOver4;
                //     float currentAngle = startAngle + (state.SwingDirection.ToRotation() - startAngle) * useProgress;
                //     Vector2[] arc = BezierWeaponTrails.GenerateSwingArc(player.Center, startAngle, currentAngle, swingRadius, 12);
                //     BezierWeaponTrails.SpawnParticlesAlongCurve(arc, palette, theme, 0.12f);
                // }
                
                // === ORBITING MUSIC NOTES ===
                if (Main.rand.NextBool(6))
                {
                    float orbitAngle = Main.GameUpdateCount * 0.1f;
                    for (int noteIdx = 0; noteIdx < 3; noteIdx++)
                    {
                        float noteAngle = orbitAngle + MathHelper.TwoPi * noteIdx / 3f;
                        Vector2 notePos = tipPos + noteAngle.ToRotationVector2() * 16f;
                        Vector2 noteVel = swingVel * 0.3f + noteAngle.ToRotationVector2() * 0.6f;
                        Color noteColor = palette != null && palette.Length > 2 ? palette[2] : (palette != null && palette.Length > 0 ? palette[0] : Color.White);
                        ThemedParticles.MusicNote(notePos, noteVel, noteColor, 0.8f, 28);
                    }
                }
                
                // Update trail
                if (state.TrailId >= 0)
                {
                    AdvancedTrailSystem.UpdateTrail(state.TrailId, tipPos, state.SwingDirection.ToRotation());
                }
                
                // === EXTENDED VFX: Dimensional Tears, Constellation Trails, Kinetic Ripples ===
                AdvancedVFXExtensions.ApplyExtendedMeleeSwingVFX(item, player, theme, useProgress);
            }
            
            // End swing
            if (state.IsSwinging && player.itemAnimation <= 1)
            {
                state.IsSwinging = false;
                
                // MeleeSwingPrimitives handles cleanup automatically via GlobalProjectile
                
                if (state.TrailId >= 0)
                {
                    AdvancedTrailSystem.EndTrail(state.TrailId);
                    state.TrailId = -1;
                }
                
                // Final swing burst
                Vector2 endPos = player.Center + state.SwingDirection * 50f;
                CalamityStyleVFX.GlimmerCascadeImpact(endPos, theme, 0.6f);
                
                // === EXTENDED VFX: Kinetic ripple on swing end ===
                KineticRippleSystem.CreateImpact(endPos, 0.4f);
            }
        }

        #endregion

        #region Ranged Effects

        private void ApplyRangedHoldingEffect(Player player, string theme, Item item)
        {
            // Subtle glow at weapon barrel
            if (Main.rand.NextBool(8))
            {
                var palette = MagnumThemePalettes.GetThemePalette(theme);
                Color primary = palette != null && palette.Length > 0 ? palette[0] : Color.White;
                
                Vector2 barrelPos = player.itemLocation + new Vector2(item.width * player.direction, 0);
                CustomParticles.GenericFlare(barrelPos, primary * 0.5f, 0.15f, 6);
                
                // Music note accent
                if (Main.rand.NextBool(5))
                {
                    ThemedParticles.MusicNote(barrelPos, new Vector2(player.direction * 0.5f, -0.5f), primary, 0.55f, 20);
                }
            }
        }

        private void ApplyRangedFireEffect(Player player, string theme, Item item)
        {
            // Only on first frame of use
            if (player.itemAnimation != player.itemAnimationMax - 1)
                return;
            
            var palette = MagnumThemePalettes.GetThemePalette(theme);
            Color primary = palette != null && palette.Length > 0 ? palette[0] : Color.White;
            Color secondary = palette != null && palette.Length > 1 ? palette[1] : primary;
            
            Vector2 barrelPos = player.itemLocation + new Vector2(item.width * player.direction, 0);
            Vector2 fireDirection = (Main.MouseWorld - player.Center).SafeNormalize(Vector2.UnitX * player.direction);
            
            // === MUZZLE FLASH ===
            CustomParticles.GenericFlare(barrelPos, Color.White, 0.7f, 12);
            CustomParticles.GenericFlare(barrelPos, primary, 0.5f, 10);
            
            // Muzzle ring
            CustomParticles.HaloRing(barrelPos, primary, 0.25f, 8);
            
            // Directional sparks
            for (int i = 0; i < 5; i++)
            {
                float sparkAngle = fireDirection.ToRotation() + Main.rand.NextFloat(-0.4f, 0.4f);
                Vector2 sparkVel = sparkAngle.ToRotationVector2() * Main.rand.NextFloat(3f, 7f);
                var sparkle = new SparkleParticle(barrelPos, sparkVel, primary, 0.25f, 12);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
            
            // Music note burst
            if (Main.rand.NextBool(3))
            {
                ThemedParticles.MusicNote(barrelPos, fireDirection * 2f, secondary, 0.65f, 25);
            }
            
            // Screen shake for powerful weapons
            if (item.damage > 50)
            {
                ScreenDistortionManager.TriggerThemeEffect(theme, barrelPos, 0.15f, 8);
            }
            
            // === EXTENDED VFX: God rays and kinetic ripple for powerful shots ===
            if (item.damage > 40)
            {
                // Brief god ray burst at muzzle
                GodRaySystem.CreateBurst(barrelPos, primary, 3, 60f, 8);
                
                // Kinetic ripple for heavy weapons
                if (item.damage > 70)
                {
                    KineticRippleSystem.CreateImpact(barrelPos, 0.25f);
                }
            }
        }

        #endregion

        #region Magic Effects

        private void ApplyMagicHoldingEffect(Player player, string theme, Item item)
        {
            // Ethereal orbiting runes/glyphs
            if (Main.rand.NextBool(4))
            {
                var palette = MagnumThemePalettes.GetThemePalette(theme);
                Color primary = palette != null && palette.Length > 0 ? palette[0] : Color.White;
                
                float orbitAngle = Main.GameUpdateCount * 0.05f;
                float orbitRadius = 25f + (float)Math.Sin(Main.GameUpdateCount * 0.08f) * 5f;
                
                for (int i = 0; i < 3; i++)
                {
                    float angle = orbitAngle + MathHelper.TwoPi * i / 3f;
                    Vector2 glyphPos = player.Center + angle.ToRotationVector2() * orbitRadius;
                    
                    // Glyph particle (use theme color with glow)
                    CustomParticles.GenericFlare(glyphPos, primary * 0.7f, 0.18f, 6);
                }
            }
            
            // Magic mist
            if (Main.rand.NextBool(8))
            {
                var palette = MagnumThemePalettes.GetThemePalette(theme);
                Color primary = palette != null && palette.Length > 0 ? palette[0] : Color.White;
                
                Vector2 mistPos = player.Center + Main.rand.NextVector2Circular(25f, 25f);
                var glow = new GenericGlowParticle(mistPos, new Vector2(0, -0.5f), primary * 0.4f, 0.2f, 25, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }
        }

        private void ApplyMagicCastEffect(Player player, string theme, Item item)
        {
            // Calculate cast progress
            float castProgress = 1f - (float)player.itemAnimation / player.itemAnimationMax;
            
            var palette = MagnumThemePalettes.GetThemePalette(theme);
            Color primary = palette != null && palette.Length > 0 ? palette[0] : Color.White;
            Color secondary = palette != null && palette.Length > 1 ? palette[1] : primary;
            
            // Channeling effect during cast - use direct particle VFX instead of boss method
            Vector2 castCenter = player.Center;
            var pal = MagnumThemePalettes.GetThemePalette(theme);
            Color channelPrimary = pal != null && pal.Length > 0 ? pal[0] : Color.White;
            Color channelSecondary = pal != null && pal.Length > 1 ? pal[1] : channelPrimary;
            
            // Converging magic circle effect
            int partCount = (int)(4 + castProgress * 8);
            float circleRadius = 40f * (1f - castProgress * 0.5f);
            for (int i = 0; i < partCount; i++)
            {
                float angle = MathHelper.TwoPi * i / partCount + (int)Main.GameUpdateCount * 0.03f;
                Vector2 offset = angle.ToRotationVector2() * circleRadius;
                Color partColor = Color.Lerp(channelPrimary, channelSecondary, (float)i / partCount);
                CustomParticles.GenericFlare(castCenter + offset, partColor, 0.25f * (0.5f + castProgress), 8);
            }
            
            // Magic circle effect
            if (player.itemAnimation == player.itemAnimationMax - 1)
            {
                // First frame - cast initiation
                Vector2 castPos = player.Center;
                
                // Central flash
                CustomParticles.GenericFlare(castPos, Color.White, 0.9f, 15);
                CustomParticles.GenericFlare(castPos, primary, 0.7f, 12);
                
                // Magic circle ring
                CustomParticles.HaloRing(castPos, primary, 0.4f, 15);
                CustomParticles.HaloRing(castPos, secondary * 0.8f, 0.3f, 12);
                
                // Sparkle burst
                for (int i = 0; i < 8; i++)
                {
                    float angle = MathHelper.TwoPi * i / 8f;
                    Vector2 sparkleVel = angle.ToRotationVector2() * 4f;
                    var sparkle = new SparkleParticle(castPos, sparkleVel, primary, 0.3f, 18);
                    MagnumParticleHandler.SpawnParticle(sparkle);
                }
                
                // Music notes
                for (int i = 0; i < 4; i++)
                {
                    float angle = MathHelper.TwoPi * i / 4f;
                    Vector2 noteVel = angle.ToRotationVector2() * 2f + new Vector2(0, -1f);
                    ThemedParticles.MusicNote(castPos, noteVel, secondary, 0.7f, 30);
                }
                
                // === EXTENDED VFX: Constellation spark and god rays for magic cast ===
                // Add a constellation line at cast point for magical themes
                if (theme == "MoonlightSonata" || theme == "Fate" || theme == "EnigmaVariations")
                {
                    Vector2 castEnd = castPos + Main.rand.NextVector2Unit() * 50f;
                    VerletConstellationSystem.CreateLine(castPos, castEnd, 4, primary, 0.6f, 45);
                }
                
                // God ray burst for powerful magic
                if (item.damage > 30)
                {
                    GodRaySystem.CreateBurst(castPos, primary, 4, 60f, 12);
                }
            }
        }

        #endregion

        #region Summon Effects

        private void ApplySummonHoldingEffect(Player player, string theme, Item item)
        {
            // Ethereal connection lines to minions
            if (Main.rand.NextBool(10))
            {
                var palette = MagnumThemePalettes.GetThemePalette(theme);
                Color primary = palette != null && palette.Length > 0 ? palette[0] : Color.White;
                
                // Subtle energy particles
                Vector2 energyPos = player.Center + Main.rand.NextVector2Circular(20f, 20f);
                var glow = new GenericGlowParticle(energyPos, new Vector2(0, -0.3f), primary * 0.5f, 0.15f, 20, true);
                MagnumParticleHandler.SpawnParticle(glow);
                
                // Music note accent
                if (Main.rand.NextBool(3))
                {
                    ThemedParticles.MusicNote(energyPos, new Vector2(0, -0.8f), primary, 0.5f, 22);
                }
            }
        }

        #endregion

        #region Generic Effects

        private void ApplyGenericHoldingEffect(Player player, string theme)
        {
            // Subtle ambient particles
            if (Main.rand.NextBool(12))
            {
                var palette = MagnumThemePalettes.GetThemePalette(theme);
                Color primary = palette != null && palette.Length > 0 ? palette[0] : Color.White;
                
                Vector2 particlePos = player.Center + Main.rand.NextVector2Circular(25f, 25f);
                CustomParticles.GenericFlare(particlePos, primary * 0.4f, 0.12f, 8);
            }
        }

        #endregion

        #region Theme Detection

        private string DetectThemeFromItem(Item item)
        {
            if (item.ModItem == null)
                return "generic";
            
            string itemNamespace = item.ModItem.GetType().Namespace ?? "";
            string itemName = item.ModItem.GetType().Name.ToLower();
            
            // Check namespace
            if (itemNamespace.Contains("Eroica")) return "Eroica";
            if (itemNamespace.Contains("SwanLake")) return "SwanLake";
            if (itemNamespace.Contains("LaCampanella")) return "LaCampanella";
            if (itemNamespace.Contains("MoonlightSonata") || itemNamespace.Contains("Moonlight")) return "MoonlightSonata";
            if (itemNamespace.Contains("EnigmaVariations") || itemNamespace.Contains("Enigma")) return "EnigmaVariations";
            if (itemNamespace.Contains("Fate")) return "Fate";
            if (itemNamespace.Contains("DiesIrae")) return "DiesIrae";
            if (itemNamespace.Contains("ClairDeLune")) return "ClairDeLune";
            if (itemNamespace.Contains("Nachtmusik")) return "Nachtmusik";
            if (itemNamespace.Contains("OdeToJoy")) return "OdeToJoy";
            if (itemNamespace.Contains("Spring")) return "Spring";
            if (itemNamespace.Contains("Summer")) return "Summer";
            if (itemNamespace.Contains("Autumn")) return "Autumn";
            if (itemNamespace.Contains("Winter")) return "Winter";
            
            // Check item name
            if (itemName.Contains("sakura") || itemName.Contains("valor") || itemName.Contains("eroica")) return "Eroica";
            if (itemName.Contains("swan") || itemName.Contains("iridescent") || itemName.Contains("feather")) return "SwanLake";
            if (itemName.Contains("bell") || itemName.Contains("campanella") || itemName.Contains("infernal")) return "LaCampanella";
            if (itemName.Contains("lunar") || itemName.Contains("moon") || itemName.Contains("sonata")) return "MoonlightSonata";
            if (itemName.Contains("enigma") || itemName.Contains("void") || itemName.Contains("paradox")) return "EnigmaVariations";
            if (itemName.Contains("fate") || itemName.Contains("cosmic") || itemName.Contains("destiny")) return "Fate";
            
            return "generic";
        }

        #endregion

        #region Melee Impact VFX

        /// <summary>
        /// Apply ImpactLightRays to all MagnumOpus melee weapon hits automatically.
        /// This adds the stretching light ray effect to direct melee hits (not projectiles).
        /// </summary>
        public override void OnHitNPC(Item item, Player player, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (item.ModItem?.Mod != ModContent.GetInstance<MagnumOpus>())
                return;
            
            // Only apply to melee weapons
            if (!item.CountsAsClass(DamageClass.Melee))
                return;
            
            string theme = DetectThemeFromItem(item);
            
            // Apply impact light rays based on crit status
            int rayCount = hit.Crit ? 8 : 5;
            float scale = hit.Crit ? 1.4f : 1.0f;
            
            ImpactLightRays.SpawnImpactRays(target.Center, theme, rayCount, scale, includeMusicNotes: true);
        }

        #endregion
    }
}
