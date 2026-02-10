using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.DataStructures;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Common.Systems.VFX
{
    /// <summary>
    /// ENHANCED WEAPON VFX INTEGRATION
    /// 
    /// This system ties together all the unique trail styles, Bézier curve rendering,
    /// screen effects, and theme palettes into a cohesive weapon VFX experience.
    /// 
    /// Key Features:
    /// 1. Automatically detects weapon theme and damage class
    /// 2. Applies unique particle-based trails using 107 particle PNGs
    /// 3. Renders smooth Bézier curved weapon swings
    /// 4. Spawns orbiting music notes and theme particles
    /// 5. Creates spectacular impact effects with proper bloom
    /// 6. Triggers appropriate screen distortions
    /// 
    /// This creates the "buttery smooth" Calamity-style rendering MagnumOpus needs.
    /// </summary>
    public class EnhancedWeaponVFXIntegration : ModSystem
    {
        #region Per-Player State
        
        /// <summary>
        /// Tracks weapon VFX state per player.
        /// </summary>
        public class PlayerWeaponState
        {
            public Item LastHeldItem;
            public string CurrentTheme = "";
            public Color[] CurrentPalette = new[] { Color.White };
            public DamageClass CurrentDamageClass;
            
            // Swing tracking for melee
            public bool IsSwinging;
            public float SwingProgress;
            public float SwingStartAngle;
            public float SwingEndAngle;
            public float SwingRadius;
            public int SwingTimer;
            public int SwingDuration;
            
            // Trail state
            public BezierWeaponTrails.WeaponTrailState TrailState;
            
            // Ambient effect timing
            public int AmbientTimer;
            
            public PlayerWeaponState()
            {
                TrailState = new BezierWeaponTrails.WeaponTrailState();
            }
        }
        
        private static Dictionary<int, PlayerWeaponState> _playerStates = new Dictionary<int, PlayerWeaponState>();
        
        public static PlayerWeaponState GetPlayerState(Player player)
        {
            if (!_playerStates.TryGetValue(player.whoAmI, out var state))
            {
                state = new PlayerWeaponState();
                _playerStates[player.whoAmI] = state;
            }
            return state;
        }
        
        #endregion
        
        #region Theme Detection
        
        /// <summary>
        /// Detects the theme of a weapon based on its namespace, name, or tooltip.
        /// </summary>
        public static string DetectWeaponTheme(Item item)
        {
            if (item == null || item.IsAir) return "Generic";
            
            string fullName = item.ModItem?.FullName ?? "";
            string name = item.Name.ToLowerInvariant();
            
            // Check namespace path
            if (fullName.Contains("SwanLake")) return "SwanLake";
            if (fullName.Contains("Eroica")) return "Eroica";
            if (fullName.Contains("LaCampanella")) return "LaCampanella";
            if (fullName.Contains("EnigmaVariations")) return "EnigmaVariations";
            if (fullName.Contains("Fate")) return "Fate";
            if (fullName.Contains("MoonlightSonata")) return "MoonlightSonata";
            if (fullName.Contains("ClairDeLune")) return "ClairDeLune";
            if (fullName.Contains("DiesIrae")) return "DiesIrae";
            if (fullName.Contains("Nachtmusik")) return "Nachtmusik";
            if (fullName.Contains("OdeToJoy")) return "OdeToJoy";
            if (fullName.Contains("Spring")) return "Spring";
            if (fullName.Contains("Summer")) return "Summer";
            if (fullName.Contains("Autumn")) return "Autumn";
            if (fullName.Contains("Winter")) return "Winter";
            
            // Check item name keywords
            if (name.Contains("swan") || name.Contains("feather") || name.Contains("ballet")) return "SwanLake";
            if (name.Contains("hero") || name.Contains("valor") || name.Contains("triumph")) return "Eroica";
            if (name.Contains("bell") || name.Contains("infern") || name.Contains("campanella")) return "LaCampanella";
            if (name.Contains("enigma") || name.Contains("mystery") || name.Contains("paradox")) return "EnigmaVariations";
            if (name.Contains("fate") || name.Contains("destiny") || name.Contains("cosmic")) return "Fate";
            if (name.Contains("moon") || name.Contains("lunar") || name.Contains("sonata")) return "MoonlightSonata";
            
            return "Generic";
        }
        
        /// <summary>
        /// Gets the color palette for a theme.
        /// </summary>
        public static Color[] GetThemePalette(string theme)
        {
            return MagnumThemePalettes.GetThemePalette(theme);
        }
        
        #endregion
        
        #region Main VFX Application
        
        /// <summary>
        /// Main method to apply all weapon VFX for a player.
        /// Call this in ModPlayer.PostUpdate or similar.
        /// </summary>
        public static void ApplyWeaponVFX(Player player)
        {
            if (player == null || player.dead || !player.active) return;
            if (Main.dedServ) return;
            
            var state = GetPlayerState(player);
            Item heldItem = player.HeldItem;
            
            // Update theme detection when item changes
            if (state.LastHeldItem != heldItem)
            {
                state.LastHeldItem = heldItem;
                state.CurrentTheme = DetectWeaponTheme(heldItem);
                state.CurrentPalette = GetThemePalette(state.CurrentTheme);
                
                if (heldItem != null && !heldItem.IsAir)
                {
                    // Detect damage class
                    if (heldItem.DamageType.CountsAsClass(DamageClass.Melee))
                        state.CurrentDamageClass = DamageClass.Melee;
                    else if (heldItem.DamageType.CountsAsClass(DamageClass.Ranged))
                        state.CurrentDamageClass = DamageClass.Ranged;
                    else if (heldItem.DamageType.CountsAsClass(DamageClass.Magic))
                        state.CurrentDamageClass = DamageClass.Magic;
                    else if (heldItem.DamageType.CountsAsClass(DamageClass.Summon))
                        state.CurrentDamageClass = DamageClass.Summon;
                    else
                        state.CurrentDamageClass = DamageClass.Generic;
                }
            }
            
            // Skip if no item or not MagnumOpus item
            if (heldItem == null || heldItem.IsAir) return;
            if (heldItem.ModItem == null) return;
            if (!heldItem.ModItem.Mod.Name.Equals("MagnumOpus", StringComparison.OrdinalIgnoreCase)) return;
            
            state.AmbientTimer++;
            
            // === MELEE SWING VFX ===
            if (player.itemAnimation > 0 && state.CurrentDamageClass == DamageClass.Melee)
            {
                ApplyMeleeSwingVFX(player, state);
            }
            
            // === RANGED FIRING VFX ===
            else if (player.itemAnimation > 0 && state.CurrentDamageClass == DamageClass.Ranged)
            {
                ApplyRangedFiringVFX(player, state);
            }
            
            // === MAGIC CHANNELING VFX ===
            else if (player.itemAnimation > 0 && state.CurrentDamageClass == DamageClass.Magic)
            {
                ApplyMagicChannelingVFX(player, state);
            }
            
            // === AMBIENT HOLD VFX ===
            else if (state.AmbientTimer % 6 == 0)
            {
                ApplyAmbientHoldVFX(player, state);
            }
        }
        
        #endregion
        
        #region Melee Swing VFX
        
        private static void ApplyMeleeSwingVFX(Player player, PlayerWeaponState state)
        {
            // Calculate swing progress
            float totalSwing = player.itemAnimationMax;
            float currentFrame = player.itemAnimationMax - player.itemAnimation;
            state.SwingProgress = currentFrame / totalSwing;
            
            // Calculate swing arc angles
            int direction = player.direction;
            state.SwingStartAngle = direction == 1 ? -MathHelper.PiOver2 - MathHelper.PiOver4 : MathHelper.PiOver2 + MathHelper.PiOver4;
            state.SwingEndAngle = direction == 1 ? MathHelper.PiOver4 : MathHelper.Pi - MathHelper.PiOver4;
            state.SwingRadius = player.HeldItem.Size.Length() * 0.8f + 40f;
            
            // Current swing position
            float currentAngle = MathHelper.Lerp(state.SwingStartAngle, state.SwingEndAngle, state.SwingProgress);
            Vector2 swingPos = player.Center + currentAngle.ToRotationVector2() * state.SwingRadius;
            Vector2 swingVelocity = currentAngle.ToRotationVector2().RotatedBy(MathHelper.PiOver2 * direction) * 8f;
            
            // === UNIQUE TRAIL SPAWNING ===
            UniqueTrailStyles.SpawnUniqueTrail(swingPos, swingVelocity, state.CurrentTheme, state.CurrentDamageClass, state.CurrentPalette);
            
            // === BÉZIER CURVE PARTICLE RIVER ===
            if (state.SwingProgress > 0.1f && state.SwingProgress < 0.9f && Main.rand.NextBool(3))
            {
                Vector2[] arc = BezierWeaponTrails.GenerateSwingArc(player.Center, state.SwingStartAngle,
                    currentAngle, state.SwingRadius, 12);
                BezierWeaponTrails.SpawnParticlesAlongCurve(arc, state.CurrentPalette, state.CurrentTheme, 0.15f);
            }
            
            // === ORBITING MUSIC NOTES ===
            if (Main.rand.NextBool(6))
            {
                float orbitAngle = Main.GameUpdateCount * 0.1f;
                for (int i = 0; i < 3; i++)
                {
                    float noteAngle = orbitAngle + MathHelper.TwoPi * i / 3f;
                    Vector2 notePos = swingPos + noteAngle.ToRotationVector2() * 18f;
                    Vector2 noteVel = swingVelocity * 0.4f + noteAngle.ToRotationVector2() * 0.8f;
                    
                    Color noteColor = state.CurrentPalette.Length > 2 ? state.CurrentPalette[2] : state.CurrentPalette[0];
                    ThemedParticles.MusicNote(notePos, noteVel, noteColor, 0.8f, 28);
                }
            }
            
            // === SWING AFTERIMAGES ===
            if (state.SwingProgress > 0.2f && state.SwingProgress < 0.85f)
            {
                // Create afterimage cascade effect
                for (int img = 0; img < 4; img++)
                {
                    float imgProgress = state.SwingProgress - img * 0.06f;
                    if (imgProgress <= 0f) continue;
                    
                    float imgAngle = MathHelper.Lerp(state.SwingStartAngle, state.SwingEndAngle, imgProgress);
                    Vector2 imgPos = player.Center + imgAngle.ToRotationVector2() * state.SwingRadius;
                    
                    float alpha = 1f - img * 0.22f;
                    Color imgColor = state.CurrentPalette[0] * alpha;
                    
                    if (Main.rand.NextBool(4))
                    {
                        CustomParticles.GenericFlare(imgPos, imgColor * 0.6f, 0.25f, 8);
                    }
                }
            }
            
            // === IMPACT FLARE AT SWING PEAK ===
            if (state.SwingProgress > 0.45f && state.SwingProgress < 0.55f && Main.rand.NextBool(8))
            {
                CustomParticles.GenericFlare(swingPos, Color.White * 0.8f, 0.5f, 12);
                CustomParticles.HaloRing(swingPos, state.CurrentPalette[0] * 0.6f, 0.3f, 10);
            }
            
            // Record trail position for Bézier rendering
            state.TrailState.RecordPosition(swingPos, currentAngle);
        }
        
        #endregion
        
        #region Ranged Firing VFX
        
        private static void ApplyRangedFiringVFX(Player player, PlayerWeaponState state)
        {
            // Muzzle flash position
            Vector2 direction = (Main.MouseWorld - player.Center).SafeNormalize(Vector2.UnitX * player.direction);
            float itemRotation = direction.ToRotation();
            Vector2 muzzlePos = player.Center + direction * (player.HeldItem.Size.Length() * 0.6f + 20f);
            
            float animProgress = 1f - (player.itemAnimation / (float)player.itemAnimationMax);
            
            // === MUZZLE FLASH ===
            if (animProgress < 0.2f)
            {
                float flashIntensity = 1f - animProgress / 0.2f;
                
                // Multi-layer muzzle flash
                CustomParticles.GenericFlare(muzzlePos, Color.White * flashIntensity, 0.6f * flashIntensity, 8);
                CustomParticles.GenericFlare(muzzlePos, state.CurrentPalette[0] * flashIntensity * 0.8f, 0.5f * flashIntensity, 10);
                
                if (state.CurrentPalette.Length > 1)
                {
                    CustomParticles.GenericFlare(muzzlePos, state.CurrentPalette[1] * flashIntensity * 0.6f, 0.4f * flashIntensity, 12);
                }
                
                // Flash ring
                if (Main.rand.NextBool(3))
                {
                    CustomParticles.HaloRing(muzzlePos, state.CurrentPalette[0] * 0.7f, 0.25f * flashIntensity, 8);
                }
            }
            
            // === TRAIL SPAWNING ===
            if (animProgress > 0.1f && animProgress < 0.6f && Main.rand.NextBool(2))
            {
                Vector2 trailVel = -direction * 3f + Main.rand.NextVector2Circular(1f, 1f);
                UniqueTrailStyles.SpawnUniqueTrail(muzzlePos, trailVel, state.CurrentTheme, state.CurrentDamageClass, state.CurrentPalette);
            }
            
            // === SHELL CASING SPARKLES ===
            if (animProgress > 0.15f && animProgress < 0.35f && Main.rand.NextBool(4))
            {
                Vector2 ejectDir = direction.RotatedBy(-MathHelper.PiOver2 * player.direction);
                Vector2 sparklePos = player.Center + ejectDir * 15f;
                Vector2 sparkleVel = ejectDir * Main.rand.NextFloat(2f, 4f) + new Vector2(0, -1f);
                
                var sparkle = new SparkleParticle(sparklePos, sparkleVel, state.CurrentPalette[0], 0.3f, 20);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
            
            // === MUSIC NOTE EJECTION ===
            if (Main.rand.NextBool(8))
            {
                Vector2 notePos = muzzlePos + Main.rand.NextVector2Circular(10f, 10f);
                Vector2 noteVel = direction * 2f + Main.rand.NextVector2Circular(1f, 1f);
                Color noteColor = state.CurrentPalette.Length > 1 ? state.CurrentPalette[1] : state.CurrentPalette[0];
                ThemedParticles.MusicNote(notePos, noteVel, noteColor, 0.75f, 25);
            }
        }
        
        #endregion
        
        #region Magic Channeling VFX
        
        private static void ApplyMagicChannelingVFX(Player player, PlayerWeaponState state)
        {
            Vector2 direction = (Main.MouseWorld - player.Center).SafeNormalize(Vector2.UnitX * player.direction);
            Vector2 castPos = player.Center + direction * 30f;
            float animProgress = 1f - (player.itemAnimation / (float)player.itemAnimationMax);
            float time = Main.GameUpdateCount * 0.08f;
            
            // === MAGIC CIRCLE ===
            float circleRadius = 30f + (float)Math.Sin(time) * 5f;
            int circlePoints = 12;
            
            for (int i = 0; i < circlePoints; i++)
            {
                float angle = MathHelper.TwoPi * i / circlePoints + time;
                Vector2 circlePos = castPos + angle.ToRotationVector2() * circleRadius;
                
                if (Main.rand.NextBool(4))
                {
                    float progress = (float)i / circlePoints;
                    Color circleColor = VFXUtilities.PaletteLerp(state.CurrentPalette, progress);
                    CustomParticles.GenericFlare(circlePos, circleColor * 0.6f, 0.2f, 8);
                }
            }
            
            // === GLYPH PARTICLES (for arcane themes) ===
            string themeLower = state.CurrentTheme.ToLowerInvariant();
            if ((themeLower.Contains("enigma") || themeLower.Contains("fate")) && Main.rand.NextBool(6))
            {
                float glyphAngle = Main.rand.NextFloat() * MathHelper.TwoPi;
                Vector2 glyphPos = castPos + glyphAngle.ToRotationVector2() * circleRadius * 0.8f;
                CustomParticles.GenericFlare(glyphPos, state.CurrentPalette[0], 0.35f, 15);
            }
            
            // === ENERGY CONVERGENCE (as animation progresses) ===
            if (animProgress < 0.5f)
            {
                float convergeIntensity = 1f - animProgress * 2f;
                int convergeCount = (int)(8 * convergeIntensity);
                
                for (int i = 0; i < convergeCount; i++)
                {
                    float angle = Main.rand.NextFloat() * MathHelper.TwoPi;
                    float dist = 60f + Main.rand.NextFloat(40f);
                    Vector2 startPos = castPos + angle.ToRotationVector2() * dist;
                    Vector2 velocity = (castPos - startPos).SafeNormalize(Vector2.Zero) * 4f;
                    
                    Color convergeColor = VFXUtilities.PaletteLerp(state.CurrentPalette, Main.rand.NextFloat());
                    
                    var glow = new GenericGlowParticle(startPos, velocity, convergeColor * 0.6f, 0.22f, 18, true);
                    MagnumParticleHandler.SpawnParticle(glow);
                }
            }
            
            // === RELEASE BURST ===
            if (animProgress > 0.4f && animProgress < 0.5f)
            {
                float burstProgress = (animProgress - 0.4f) / 0.1f;
                
                // Central flash
                CustomParticles.GenericFlare(castPos, Color.White * burstProgress, 0.7f * burstProgress, 10);
                
                // Ring expansion
                for (int i = 0; i < 3; i++)
                {
                    Color ringColor = state.CurrentPalette[Math.Min(i, state.CurrentPalette.Length - 1)];
                    CustomParticles.HaloRing(castPos, ringColor * burstProgress * 0.7f, 0.2f + i * 0.08f, 12);
                }
            }
            
            // === TRAILING MUSIC NOTES ===
            if (Main.rand.NextBool(8))
            {
                Vector2 notePos = castPos + Main.rand.NextVector2Circular(circleRadius, circleRadius);
                Vector2 noteVel = new Vector2(0, -1f) + Main.rand.NextVector2Circular(0.5f, 0.5f);
                Color noteColor = state.CurrentPalette[state.CurrentPalette.Length - 1];
                ThemedParticles.MusicNote(notePos, noteVel, noteColor, 0.72f, 30);
            }
            
            // === UNIQUE TRAIL SPAWNING ===
            if (Main.rand.NextBool(3))
            {
                UniqueTrailStyles.SpawnUniqueTrail(castPos, direction * 2f, state.CurrentTheme, state.CurrentDamageClass, state.CurrentPalette);
            }
        }
        
        #endregion
        
        #region Ambient Hold VFX
        
        private static void ApplyAmbientHoldVFX(Player player, PlayerWeaponState state)
        {
            Vector2 itemPos = player.Center + new Vector2(player.direction * 20f, 0f);
            float time = Main.GameUpdateCount * 0.05f;
            
            // === SUBTLE ORBITING PARTICLES ===
            if (Main.rand.NextBool(12))
            {
                float orbitAngle = time;
                for (int i = 0; i < 2; i++)
                {
                    float angle = orbitAngle + MathHelper.TwoPi * i / 2f;
                    float radius = 25f + (float)Math.Sin(time + i) * 5f;
                    Vector2 orbitPos = itemPos + angle.ToRotationVector2() * radius;
                    
                    Color orbitColor = state.CurrentPalette[i % state.CurrentPalette.Length];
                    CustomParticles.GenericFlare(orbitPos, orbitColor * 0.4f, 0.18f, 12);
                }
            }
            
            // === OCCASIONAL MUSIC NOTE ===
            if (Main.rand.NextBool(30))
            {
                Vector2 notePos = itemPos + Main.rand.NextVector2Circular(20f, 20f);
                Vector2 noteVel = new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), -0.8f);
                Color noteColor = state.CurrentPalette[0];
                ThemedParticles.MusicNote(notePos, noteVel, noteColor * 0.7f, 0.6f, 40);
            }
            
            // === THEME-SPECIFIC AMBIENT ===
            string themeLower = state.CurrentTheme.ToLowerInvariant();
            
            if (themeLower.Contains("swan") && Main.rand.NextBool(45))
            {
                // Swan Lake: Occasional drifting feather
                Vector2 featherPos = itemPos + new Vector2(Main.rand.NextFloat(-15f, 15f), -10f);
                Vector2 featherVel = new Vector2(Main.rand.NextFloat(-0.3f, 0.3f), 0.4f);
                var glow = new GenericGlowParticle(featherPos, featherVel, Color.White * 0.6f, 0.3f, 60, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }
            else if (themeLower.Contains("campanella") && Main.rand.NextBool(40))
            {
                // La Campanella: Ember wisps
                Vector2 emberPos = itemPos + Main.rand.NextVector2Circular(15f, 15f);
                Vector2 emberVel = new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), -1.5f);
                var glow = new GenericGlowParticle(emberPos, emberVel, new Color(255, 140, 40) * 0.7f, 0.22f, 35, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }
            else if (themeLower.Contains("fate") && Main.rand.NextBool(50))
            {
                // Fate: Tiny star sparkles
                Vector2 starPos = itemPos + Main.rand.NextVector2Circular(30f, 30f);
                CustomParticles.GenericFlare(starPos, Color.White * 0.5f, 0.15f, 15);
            }
        }
        
        #endregion
        
        #region Impact VFX
        
        /// <summary>
        /// Call this when a weapon hits an enemy to create spectacular impact effects.
        /// </summary>
        public static void TriggerHitImpact(Player player, Entity target, float damageDealt)
        {
            var state = GetPlayerState(player);
            Vector2 impactPos = target.Center;
            
            // Scale based on damage (normalized to 100 damage = 1.0 scale)
            float impactScale = Math.Min(2f, 0.5f + damageDealt / 150f);
            
            // === UNIQUE THEME IMPACT ===
            UniqueTrailStyles.SpawnUniqueImpact(impactPos, state.CurrentTheme, state.CurrentDamageClass, state.CurrentPalette, impactScale);
            
            // === SCREEN DISTORTION ===
            if (damageDealt > 50f)
            {
                float distortionIntensity = Math.Min(1f, damageDealt / 200f);
                ScreenDistortionManager.TriggerThemeEffect(state.CurrentTheme, impactPos, distortionIntensity, 20);
            }
            
            // === DUST EXPLOSION ===
            int dustCount = (int)(10 * impactScale);
            for (int i = 0; i < dustCount; i++)
            {
                Vector2 dustVel = Main.rand.NextVector2Circular(8f * impactScale, 8f * impactScale);
                Color dustColor = VFXUtilities.PaletteLerp(state.CurrentPalette, (float)i / dustCount);
                
                int dustType = Terraria.ID.DustID.MagicMirror;
                Dust d = Dust.NewDustPerfect(impactPos, dustType, dustVel, 0, dustColor, 1.3f);
                d.noGravity = true;
                d.fadeIn = 1.2f;
            }
            
            // === LIGHTING ===
            Lighting.AddLight(impactPos, state.CurrentPalette[0].ToVector3() * 1.2f * impactScale);
        }
        
        /// <summary>
        /// Call this when a projectile dies to create death effects.
        /// </summary>
        public static void TriggerProjectileDeath(Projectile projectile, string theme = null)
        {
            if (theme == null)
            {
                // Try to detect from projectile
                theme = projectile.ModProjectile?.FullName?.Contains("SwanLake") == true ? "SwanLake" :
                        projectile.ModProjectile?.FullName?.Contains("Eroica") == true ? "Eroica" :
                        projectile.ModProjectile?.FullName?.Contains("LaCampanella") == true ? "LaCampanella" :
                        projectile.ModProjectile?.FullName?.Contains("EnigmaVariations") == true ? "EnigmaVariations" :
                        projectile.ModProjectile?.FullName?.Contains("Fate") == true ? "Fate" :
                        projectile.ModProjectile?.FullName?.Contains("MoonlightSonata") == true ? "MoonlightSonata" :
                        "Generic";
            }
            
            Color[] palette = GetThemePalette(theme);
            DamageClass damageClass = projectile.DamageType;
            
            // Unique death impact
            UniqueTrailStyles.SpawnUniqueImpact(projectile.Center, theme, damageClass, palette, 0.8f);
            
            // Clear trail state
            BezierWeaponTrails.ClearProjectileTrail(projectile.whoAmI);
        }
        
        #endregion
        
        #region Drawing
        
        /// <summary>
        /// Draws the Bézier trail for the player's current weapon swing.
        /// Call this in PostDraw or similar.
        /// </summary>
        public static void DrawPlayerWeaponTrail(SpriteBatch spriteBatch, Player player)
        {
            if (player == null || player.dead || !player.active) return;
            if (player.itemAnimation <= 0) return;
            
            var state = GetPlayerState(player);
            
            // Only draw for melee currently swinging
            if (state.CurrentDamageClass != DamageClass.Melee) return;
            if (state.SwingProgress <= 0.1f || state.SwingProgress >= 0.95f) return;
            
            // Draw Bézier trail
            BezierWeaponTrails.RenderMeleeSwingTrail(
                spriteBatch,
                player,
                state.SwingProgress,
                state.SwingStartAngle,
                state.SwingEndAngle,
                state.SwingRadius,
                state.CurrentPalette,
                1f
            );
            
            // Draw afterimage cascade for extra flair
            if (state.SwingProgress > 0.2f && state.SwingProgress < 0.8f)
            {
                BezierWeaponTrails.SpawnAfterImageCascade(
                    spriteBatch,
                    player,
                    state.SwingProgress,
                    state.SwingStartAngle,
                    state.SwingEndAngle,
                    state.SwingRadius,
                    state.CurrentPalette,
                    4
                );
            }
        }
        
        #endregion
    }
}
